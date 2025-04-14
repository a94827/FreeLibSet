// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Выполняет проверку имен таблиц и столбцов при выполнении запросов.
  /// Свойство <see cref="DBxConBase.Validator"/>.
  /// При проверке корректности имен столбцов добавляет ссылки в <see cref="DBxSqlBuffer.ColumnStructs"/>.
  /// Не используется в прикладном коде.
  /// </summary>
  public sealed class DBxNameValidator
  {
    #region Конструктор

    /// <summary>
    /// Создает объект для заданной точки подключения к базе данных
    /// </summary>
    /// <param name="con">Обслуживаемое подключение</param>
    internal DBxNameValidator(DBxConBase con)
    {
      if (con == null)
        throw new ArgumentNullException("con");

      _Con = con;
      _NameCheckingEnabled = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Обслуживаемое соединение
    /// </summary>
    public DBxConBase Con { get { return _Con; } }
    private readonly DBxConBase _Con;

    /// <summary>
    /// Если свойство установлено (по умолчанию), то выполняется проверка существования
    /// описаний таблиц и полей в реальной структуре таблицы.
    /// Если свойство сброшено в false, проверяется только общая корректность имен (на наличие недопустимых символов)
    /// Это свойство не дублируется в основном соединении <see cref="DBxCon"/>. Следовательно, проверка может быть отключена
    /// только на стороне сервера (безопасность)
    /// </summary>
    public bool NameCheckingEnabled
    {
      get { return _NameCheckingEnabled; }
      set { _NameCheckingEnabled = value; }
    }
    private bool _NameCheckingEnabled;

    #endregion

    #region Методы проверки

    /// <summary>
    /// Проверка имени таблицы на допустимость и прав на чтение/запись в таблицу.
    /// Если свойство <see cref="NameCheckingEnabled"/> установлено, проверяется также наличие описания таблицы в реальной структуре базы данных.
    /// </summary>
    /// <param name="tableName">Проверяемое имя</param>
    /// <param name="mode">Предстоящий режим использования таблицы (Full - изменение, ReadOnly - чтение)</param>
    public void CheckTableName(string tableName, DBxAccessMode mode)
    {
      string errorText;
      if (!Con.Entry.DB.IsValidTableName(tableName, out errorText))
        throw new ArgumentException(String.Format(Res.DBxNameValidator_Arg_InvalidTableName,
          tableName, errorText), "tableName");

      if (NameCheckingEnabled)
      {
        if (Con.GetTableStruct(tableName) == null)
          throw new ArgumentException(String.Format(Res.DBxNameValidator_Arg_UnknownTableName,
            tableName, Con.DB), "tableName");
      }

      switch (mode)
      {
        case DBxAccessMode.Full:
          if (Con.Entry.Permissions.TableModes[tableName] != DBxAccessMode.Full)
            throw new DBxAccessException(String.Format(Res.DBxNameValidator_Err_TableWrite, tableName));
          break;
        case DBxAccessMode.ReadOnly:
          if (Con.Entry.Permissions.TableModes[tableName] == DBxAccessMode.None)
            throw new DBxAccessException(String.Format(Res.DBxNameValidator_Err_TableRead, tableName));
          break;
      }
    }

    /// <summary>
    /// Проверка имени столбца, включая наличие его в таблице данных и доступа к нему.
    /// Если имя столбца неправильное или столбец недоступен, то выбрасывается исключение.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="allowDots">Может ли поле содержать точки</param>
    /// <param name="mode">Режим доступа</param>
    /// <returns>Возвращает тип столбца</returns>
    public DBxColumnType CheckTableColumnName(string tableName, string columnName, bool allowDots, DBxAccessMode mode)
    {
      DBxColumnStruct cs = DoCheckTableColumnName(tableName, columnName, allowDots, mode);
      if (cs != null)
      {
        Con.Buffer.ColumnStructs[columnName] = cs; // не Add(), так как могут быть повторяющиеся вызовы
        return cs.ColumnType;
      }
      else
        return DBxColumnType.Unknown;
    }

    private DBxColumnStruct DoCheckTableColumnName(string tableName, string columnName, bool allowDots, DBxAccessMode mode)
    {
      string errorText;
      if (!Con.DB.IsValidColumnName(columnName, allowDots, out errorText))
        throw new ArgumentException(String.Format(Res.DBxNameValidator_Arg_InvalidColumnName,
          columnName, errorText), "columnName");

      int pDot = columnName.IndexOf('.');

      if (pDot >= 0)
      {
        string mainColumnName = columnName.Substring(0, pDot);
        DBxColumnStruct colDef = Con.GetTableStruct(tableName).Columns[mainColumnName];
        if (colDef == null)
        {
          if (NameCheckingEnabled)
            throw new ArgumentException(String.Format(Res.DBxNameValidator_Arg_UnknownColumnName,
              mainColumnName, tableName, Con), "columnName");
          else
            return null;
        }
        if (String.IsNullOrEmpty(colDef.MasterTableName))
          throw new ArgumentException(String.Format(Res.DBxNameValidator_Arg_ColumnNotRef,
            mainColumnName, tableName, Con.DB), "columnName");

        if (NameCheckingEnabled)
          CheckTableName(colDef.MasterTableName, mode);

        // Рекурсивный вызов
        return DoCheckTableColumnName(colDef.MasterTableName, columnName.Substring(pDot + 1), true, mode);
      }
      else
      {
        DBxTableStruct ts = Con.GetTableStruct(tableName);
        if (ts == null)
        {
          if (NameCheckingEnabled)
            throw new ArgumentException(String.Format(Res.DBxNameValidator_Arg_UnknownTableName,
              tableName, Con.DB), "tableName");
          else
            return null; // 22.07.2021
        }
        if (!ts.Columns.Contains(columnName))
        {
          if (NameCheckingEnabled)
            throw new ArgumentException(String.Format(Res.DBxNameValidator_Arg_UnknownColumnName,
              columnName, tableName, Con.Entry.DB), "columnName");
          else
            return null;
        }

        if (NameCheckingEnabled)
        {
          switch (mode)
          {
            case DBxAccessMode.Full:
              switch (Con.Entry.Permissions.ColumnModes[tableName, columnName])
              {
                case DBxAccessMode.ReadOnly:
                  throw new DBxAccessException(String.Format(Res.DBxNameValidator_Err_ColumnWrite, columnName, tableName));
                case DBxAccessMode.None:
                  throw new DBxAccessException(String.Format(Res.DBxNameValidator_Err_ColumnRead, columnName, tableName));
              }
              break;
            case DBxAccessMode.ReadOnly:
              if (Con.Entry.Permissions.ColumnModes[tableName, columnName] == DBxAccessMode.None)
                throw new DBxAccessException(String.Format(Res.DBxNameValidator_Err_ColumnRead, columnName, tableName));
              break;
          }
        }
        return Con.GetTableStruct(tableName).Columns[columnName];
      }
    }

    /// <summary>
    /// Проверка имен списка столбцов, включая наличие их в таблице данных и доступа к ним.
    /// Если имя какого-либо столбца в списке неправильное или столбец недоступен, то выбрасывается исключение.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список проверяемых имен столбцов</param>
    /// <param name="allowDots">Могут ли поля содержать точки</param>
    /// <param name="mode">Режим доступа</param>
    /// <returns>Возвращает типы столбцов. Длина массива соответствует списку <paramref name="columnNames"/>.</returns>
    public DBxColumnType[] CheckTableColumnNames(string tableName, DBxColumns columnNames, bool allowDots, DBxAccessMode mode)
    {
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
      if (columnNames.Count == 0)
        throw new ArgumentException(Res.DBxNameValidator_Arg_ColumnListIsEmpty, "columnNames");

      DBxColumnType[] columnTypes = new DBxColumnType[columnNames.Count];

      for (int i = 0; i < columnNames.Count; i++)
        columnTypes[i] = CheckTableColumnName(tableName, columnNames[i], allowDots, mode);
      return columnTypes;
    }

    /// <summary>
    /// Проверка имен списка столбцов, включая наличие их в таблице данных и доступа к ним.
    /// Если имя какого-либо столбца в списке неправильное или столбец недоступен, то выбрасывается исключение.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список проверяемых имен столбцов</param>
    /// <param name="allowDots">Могут ли поля содержать точки</param>
    /// <param name="mode">Режим доступа</param>
    /// <returns>Возвращает типы столбцов. Длина массива соответствует списку <paramref name="columnNames"/>.</returns>
    public DBxColumnType[] CheckTableColumnNames(string tableName, DBxColumnList columnNames, bool allowDots, DBxAccessMode mode)
    {
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
      if (columnNames.Count == 0)
        throw new ArgumentException(Res.DBxNameValidator_Arg_ColumnListIsEmpty, "columnNames");

      DBxColumnType[] columnTypes = new DBxColumnType[columnNames.Count];

      for (int i = 0; i < columnNames.Count; i++)
        columnTypes[i] = CheckTableColumnName(tableName, columnNames[i], allowDots, mode);
      return columnTypes;
    }

    /// <summary>
    /// Внутренний объект, используемый при проверке фильтров и порядка сортировки
    /// </summary>
    private DBxColumnList _CheckColumnList;

    /// <summary>
    /// Проверка имен списка столбцов, включая наличие их в таблице данных и доступа к ним.
    /// Имена полей извлекаются из <see cref="DBxNamedExpressionList"/>.
    /// Если имя какого-либо столбца в списке неправильное или столбец недоступен, то выбрасывается исключение.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="expressions">Список проверяемых выражений</param>
    public void CheckExpressionColumnNames(string tableName, DBxNamedExpressionList expressions)
    {
      DBxColumnList list = new DBxColumnList();
      expressions.GetColumnNames(list);
      for (int i = 0; i < list.Count; i++)
        CheckTableColumnName(tableName, list[i], true, DBxAccessMode.ReadOnly);
    }

    /// <summary>
    /// Проверка имен списка столбцов, включая наличие их в таблице данных и доступа к ним.
    /// Имена полей извлекаются из <see cref="DBxNamedExpressionList"/>.
    /// Если имя какого-либо столбца в списке неправильное или столбец недоступен, то выбрасывается исключение.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="expressions">Список проверяемых выражений</param>
    public void CheckExpressionColumnNames(string tableName, IList<DBxExpression> expressions)
    {
      DBxColumnList list = new DBxColumnList();
      for (int i = 0; i < expressions.Count; i++)
        expressions[i].GetColumnNames(list);
      for (int i = 0; i < list.Count; i++)
        CheckTableColumnName(tableName, list[i], true, DBxAccessMode.ReadOnly);
    }

    /// <summary>
    /// Выполнить проверку имен столбцов в фильтрах.
    /// Метод собирает список имен в фильтре вызовом <see cref="DBxFilter.GetColumnNames(DBxColumnList)"/>,
    /// а затем вызывает <see cref="CheckTableColumnName(string, string, bool, DBxAccessMode)"/>.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="filter">Фильтр. Не может быть null</param>
    /// <param name="allowDots">Может ли поле содержать точки</param>
    public void CheckFilterColumnNames(string tableName, DBxFilter filter, bool allowDots)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      if (_CheckColumnList == null)
        _CheckColumnList = new DBxColumnList();
      _CheckColumnList.Clear();
      filter.GetColumnNames(_CheckColumnList);
      for (int i = 0; i < _CheckColumnList.Count; i++)
        CheckTableColumnName(tableName, _CheckColumnList[i], allowDots, DBxAccessMode.ReadOnly);
    }

    ///// <summary>
    ///// Выполнить проверку имен столбцов в порядке сортировки.
    ///// Метод собирает список имен в фильтре вызовом DBxOrder.GetColumnNames(),
    ///// а затем вызывает CheckTableColumnName() для всех имен полей
    ///// </summary>
    ///// <param name="tableName">Имя таблицы</param>
    ///// <param name="order">Порядок сортировки. Не может быть null</param>
    ///// <param name="allowDots">Может ли поле содержать точки</param>
    //public void CheckOrderColumnNames(string tableName, DBxOrder order, bool allowDots)
    //{
    //  if (order == null)
    //    throw new ArgumentNullException("order");

    //  if (CheckColumnList == null)
    //    CheckColumnList = new DBxColumnList();
    //  CheckColumnList.Clear();
    //  order.GetColumnNames(CheckColumnList);
    //  for (int i = 0; i < CheckColumnList.Count; i++)
    //    CheckTableColumnName(tableName, CheckColumnList[i], allowDots, DBxAccessMode.ReadOnly);
    //}

    /// <summary>
    /// Проверяет, что первичным ключом таблицы является единственное целочисленное поле.
    /// Если это не так, генерируется <see cref="DBxPrimaryKeyException"/>.
    /// Предполагается, что проверка <see cref="CheckTableName"/> уже выполнена.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Имя поля первичного ключа</returns>
    public string CheckTablePrimaryKeyInt32(string tableName)
    {
      DBxTableStruct ts = Con.GetTableStruct(tableName);
      return DBxStructChecker.CheckTablePrimaryKeyInt32(ts);
    }

    /// <summary>
    /// Возвращает индекс поля первичного ключа в списке столбцов <paramref name="columnNames"/>
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список столбцов для поиска</param>
    /// <returns>Индекс столбца или (-1), если столбец не найден</returns>
    public int GetPrimaryKeyInt32ColumnIndex(string tableName, DBxColumns columnNames)
    {
      DBxTableStruct ts = Con.GetTableStruct(tableName);
      if (ts.PrimaryKey.Count != 1)
        return -1;

      DBxColumnStruct cs = ts.Columns[ts.PrimaryKey[0]];
      if (cs.ColumnType != DBxColumnType.Int)
        return -1;

      return columnNames.IndexOf(cs.ColumnName);
    }

    #endregion
  }
}
