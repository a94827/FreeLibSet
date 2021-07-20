using System;
using System.Collections.Generic;
using System.Text;

namespace AgeyevAV.ExtDB
{
  /// <summary>
  /// Выполняет проверку имен таблиц и столбцов в DBxConBase при выполнении запросов.
  /// При проверке корректности имен столбцов добавляет ссылки в DBxSqlBuffer.DBxColumnStruct
  /// </summary>
  public sealed class DBxNameValidator
  {
    #region Конструктор

    /// <summary>
    /// Создает объект для заданной точки подключения к базе данных
    /// </summary>
    /// <param name="entry">Точка подключения к базе данных</param>
    /// <param name="buffer">Буфер для создания SQL-запросов</param>
    public DBxNameValidator(DBxEntry entry, DBxSqlBuffer buffer)
    {
      if (entry == null)
        throw new ArgumentNullException("entry");
      if (buffer == null)
        throw new ArgumentNullException("buffer");

      _Entry = entry;
      _Buffer = buffer;
      _NameCheckingEnabled = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Точка подключения к базе данных
    /// </summary>
    public DBxEntry Entry { get { return _Entry; } }
    private DBxEntry _Entry;

    /// <summary>
    /// Буфер для создания SQL-запросов
    /// </summary>
    public DBxSqlBuffer Buffer { get { return _Buffer; } }
    private DBxSqlBuffer _Buffer;

    /// <summary>
    /// Если свойство установлено (по умолчанию), то выполняется проверка существования
    /// описаний таблиц и полей в реальной структуре таблицы.
    /// Если свойство сброшено в false, проверяется только общая корректность имен (на наличие недопустимых символов)
    /// Это свойство не дублируется в основном соединении DBxCon. Следовательно, проверка может быть отключена
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
    /// Проверка имени таблицы на допустимость и прав на чтение/запись в таблицу
    /// Если свойство NameCheckingEnabled установлено, проверяется также наличие описания таблицы в реальной структуре базы данных
    /// </summary>
    /// <param name="tableName">Проверяемое имя</param>
    /// <param name="mode">Предстоящий режим использования таблицы (Full - изменение, ReadOnly - чтение)</param>
    public void CheckTableName(string tableName, DBxAccessMode mode)
    {
      string ErrorText;
      if (!Entry.DB.IsValidTableName(tableName, out ErrorText))
        throw new ArgumentException("Недопустимое имя таблицы \"" + tableName + "\". " + ErrorText);

      if (NameCheckingEnabled)
      {
        if (!Entry.DB.Struct.Tables.Contains(tableName))
          throw new ArgumentException("Определения для таблицы \"" + tableName + "\" не существует для БД \"" + Entry.DB.ToString() + "\"", "tableName");
      }

      switch (mode)
      {
        case DBxAccessMode.Full:
          if (Entry.Permissions.TableModes[tableName] != DBxAccessMode.Full)
            throw new DBxAccessException("Нет разрешения на запись в таблицу \"" + tableName + "\"");
          break;
        case DBxAccessMode.ReadOnly:
          if (Entry.Permissions.TableModes[tableName] == DBxAccessMode.None)
            throw new DBxAccessException("Нет разрешения на доступ к таблице \"" + tableName + "\"");
          break;
      }
    }

    /// <summary>
    /// Проверка имени столбца, включая наличие его в таблице данных и доступа к нему.
    /// Если имя столбца неправильное или столбец недоступен, то выбрасывается исключение
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
        Buffer.ColumnStructs[columnName] = cs; // не Add(), так как могут быть повторяющиеся вызовы
        return cs.ColumnType;
      }
      else
        return DBxColumnType.Unknown;
    }

    private DBxColumnStruct DoCheckTableColumnName(string tableName, string columnName, bool allowDots, DBxAccessMode mode)
    {
      string ErrorText;
      if (!Entry.DB.IsValidColumnName(columnName, allowDots, out ErrorText))
        throw new ArgumentException("Недопустимое имя столбца \"" + columnName + "\". " + ErrorText, "columnName");

      int pDot = columnName.IndexOf('.');

      if (pDot >= 0)
      {
        string MainColumnName = columnName.Substring(0, pDot);
        DBxColumnStruct ColDef = Entry.DB.Struct.Tables[tableName].Columns[MainColumnName];
        if (ColDef == null)
        {
          if (NameCheckingEnabled)
            throw new ArgumentException("Определения для столбца \"" + MainColumnName + "\" нет в определении таблицы \"" + tableName + "\" БД \"" + Entry.DB.ToString() + "\"", "columnName");
          else
            return null;
        }
        if (String.IsNullOrEmpty(ColDef.MasterTableName))
          throw new ArgumentException("Столбец \"" + MainColumnName + "\" таблицы \"" + tableName + "\" БД \"" + Entry.DB.ToString() + "\" не является ссылочным", "columnName");

        if (NameCheckingEnabled)
          CheckTableName(ColDef.MasterTableName, mode);

        // Рекурсивный вызов
        return DoCheckTableColumnName(ColDef.MasterTableName, columnName.Substring(pDot + 1), true, mode);
      }
      else
      {
        if (!Entry.DB.Struct.Tables[tableName].Columns.Contains(columnName))
        {
          if (NameCheckingEnabled)
            throw new ArgumentException("Определения для столбца \"" + columnName + "\" нет в определении таблицы \"" + tableName + "\" БД \"" + Entry.DB.ToString() + "\"", "columnName");
          else
            return null;
        }

        if (NameCheckingEnabled)
        {
          switch (mode)
          {
            case DBxAccessMode.Full:
              switch (Entry.Permissions.ColumnModes[tableName, columnName])
              {
                case DBxAccessMode.ReadOnly:
                  throw new DBxAccessException("Запрещено изменение поля \"" + columnName + "\" таблицы \"" + tableName + "\". Есть право только на просмотр поля");
                case DBxAccessMode.None:
                  throw new DBxAccessException("Запрещен доступ к полю \"" + columnName + "\" таблицы \"" + tableName + "\"");
              }
              break;
            case DBxAccessMode.ReadOnly:
              if (Entry.Permissions.ColumnModes[tableName, columnName] == DBxAccessMode.None)
                throw new DBxAccessException("Запрещен доступ к полю \"" + columnName + "\" таблицы \"" + tableName + "\"");
              break;
          }
        }
        return Entry.DB.Struct.Tables[tableName].Columns[columnName];
      }
    }

    /// <summary>
    /// Проверка имен списка столбцов, включая наличие их в таблице данных и доступа к ним.
    /// Если имя какого-либо столбца в списке неправильное или столбец недоступен, то выбрасывается исключение
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
        throw new ArgumentException("Пустой список имен полей", "columnNames");

      DBxColumnType[] ColumnTypes = new DBxColumnType[columnNames.Count];

      for (int i = 0; i < columnNames.Count; i++)
        ColumnTypes[i] = CheckTableColumnName(tableName, columnNames[i], allowDots, mode);
      return ColumnTypes;
    }

    /// <summary>
    /// Проверка имен списка столбцов, включая наличие их в таблице данных и доступа к ним.
    /// Если имя какого-либо столбца в списке неправильное или столбец недоступен, то выбрасывается исключение
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
        throw new ArgumentException("Пустой список имен полей", "columnNames");

      DBxColumnType[] ColumnTypes = new DBxColumnType[columnNames.Count];

      for (int i = 0; i < columnNames.Count; i++)
        ColumnTypes[i] = CheckTableColumnName(tableName, columnNames[i], allowDots, mode);
      return ColumnTypes;
    }



    /// <summary>
    /// Внутренний объект, используемый при проверке фильтров и порядка сортировки
    /// </summary>
    private DBxColumnList CheckColumnList;


    /// <summary>
    /// Проверка имен списка столбцов, включая наличие их в таблице данных и доступа к ним.
    /// Имена полей извлекаются из DBxNamedExpressionList.
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
    /// Имена полей извлекаются из DBxNamedExpressionList.
    /// Если имя какого-либо столбца в списке неправильное или столбец недоступен, то выбрасывается исключение.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="expressions">Список проверяемых выражений</param>
    public void CheckExpressionColumnNames(string tableName, IList<DBxExpression> expressions)
    {
      DBxColumnList list = new DBxColumnList();
      for (int i = 0; i < expressions.Count;i++ )
        expressions[i].GetColumnNames(list);
      for (int i = 0; i < list.Count; i++)
        CheckTableColumnName(tableName, list[i], true, DBxAccessMode.ReadOnly);
    }

    /// <summary>
    /// Выполнить проверку имен столбцов в фильтрах.
    /// Метод собирает список имен в фильтре вызовом DBxFilter.GetColumnNames(),
    /// а затем вызывает CheckTableColumnName() для всех имен полей
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="filter">Фильтр. Не может быть null</param>
    /// <param name="allowDots">Может ли поле содержать точки</param>
    public void CheckFilterColumnNames(string tableName, DBxFilter filter, bool allowDots)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      if (CheckColumnList == null)
        CheckColumnList = new DBxColumnList();
      CheckColumnList.Clear();
      filter.GetColumnNames(CheckColumnList);
      for (int i = 0; i < CheckColumnList.Count; i++)
        CheckTableColumnName(tableName, CheckColumnList[i], allowDots, DBxAccessMode.ReadOnly);
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
    /// Если это не так, генерируется DBxPrimaryKeyException.
    /// Предполагается, что проверка CheckTableName уже выполнена
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Имя поля первичного ключа</returns>
    public string CheckTablePrimaryKeyInt32(string tableName)
    {
      DBxTableStruct ts = Entry.DB.Struct.Tables[tableName];
      return ts.CheckTablePrimaryKeyInt32();
    }

    /// <summary>
    /// Возвращает индекс поля первичного ключа в списке столбцов <paramref name="columnNames"/>
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список столбцов для поиска</param>
    /// <returns>Индекс столбца или (-1), если столбец не найден</returns>
    public int GetPrimaryKeyInt32ColumnIndex(string tableName, DBxColumns columnNames)
    {
      DBxTableStruct ts = Entry.DB.Struct.Tables[tableName];
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
