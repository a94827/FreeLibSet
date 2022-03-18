// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data;
using System.IO;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Генератор SQL-выражения для FillSelect().
  /// Используется в методах DBxConBase.FillSelect() и подобных.
  /// Объект является "одноразовым"
  /// Использование:
  /// 1. Создать DBxSelectFormatter.
  /// 2. Вызвать Format()
  /// 3. Вызвать CorrectColumnNames()
  /// 4. Вызвать CorrectColumnTypes()
  /// </summary>
  internal class DBxSelectFormatter
  {
    #region Конструктор

    public DBxSelectFormatter(DBxSelectInfo info, DBxNameValidator validator)
    {
      if (info == null)
        throw new ArgumentNullException("info");
      if (validator == null)
        throw new ArgumentNullException("validator");

      this._Info = info;
      this._Validator = validator;
    }

    #endregion

    #region Поля

    private DBxSelectInfo _Info;

    private DBxNameValidator _Validator;

    #endregion

    #region Создание SQL-запроса

    /// <summary>
    /// Дополнительный буфер для создания строки.
    /// </summary>
    private DBxSqlBuffer _Buffer2;

    /// <summary>
    /// Основной метод - получение SQL-выражения
    /// На момент вызова все управляющие поля установлены
    /// </summary>
    /// <param name="buffer">Заполняемый буфер</param>
    public void Format(DBxSqlBuffer buffer)
    {
      #region Проверка аргументов

      if (_Buffer2 != null)
        throw new InvalidOperationException("Повторный вызов метода Format не допускается. Объект FillSelectFormatter является одноразовым");

      if (buffer == null)
        throw new ArgumentNullException("buffer");
      if (String.IsNullOrEmpty(_Info.TableName))
        throw new InvalidDataException("Не задано DBxSelectInfo.TableName");

      _Validator.CheckTableName(_Info.TableName, DBxAccessMode.ReadOnly);

      if (_Info.HasGroupBy)
      {
        if (_Info.Expressions.Count == 0)
          throw new InvalidOperationException("Задан список GROUP BY без задания списка выражений (SELECT *)");
      }
      else
      {
        if (_Info.Having != null)
          throw new InvalidOperationException("Задано предложение HAVING без GROUP BY");
      }

      PrepareColumnNames();

      //Validator.CheckExpressionColumnNames(Info.TableName, Info.Expressions);
      //if (Info.Where != null)
      //  Validator.CheckFilterColumnNames(Info.TableName, Info.Where, true);
      //Validator.CheckExpressionColumnNames(Info.TableName, Info.GroupBy);
      //if (Info.Having != null)
      //  Validator.CheckFilterColumnNames(Info.TableName, Info.Having, true);
      ////if (Info.OrderBy != null)
      ////  Validator.CheckOrderColumnNames(Info.TableName, Info.OrderBy, true);

      #region Полный список столбцов ColumnNames2

      // Если предложения Where и OrderBy содержит имена полей с точками, то их придется 
      // добавить в конец списка
      DBxColumnList columnNames2 = new DBxColumnList();
      _Info.GetColumnNames(columnNames2); // все существующие поля

      _Validator.CheckTableColumnNames(_Info.TableName, columnNames2, true, DBxAccessMode.ReadOnly);

      #endregion

      PrepareInternal(columnNames2, buffer); // 28.02.2020
      _Buffer2 = new DBxSqlBuffer(buffer.Formatter);

      #endregion

      #region TYPES

      // все равно не работает
      //if (UseTypesOperatorBeforeSelect)
      //  AddTypesList(Buffer);

      #endregion

      #region SELECT

      buffer.SB.Append("SELECT ");

      #endregion

      #region TOP

      if (_Info.MaxRecordCount != 0)
      {
        if (_Info.MaxRecordCount < 0)
          throw new InvalidOperationException("DBxSelectInfo.MaxRecordCount<0");

        switch (buffer.Formatter.SelectMaxRecordCountMode)
        {
          case DBxSelectMaxRecordCountMode.Top:
            buffer.SB.Append("TOP ");
            buffer.SB.Append(_Info.MaxRecordCount);
            buffer.SB.Append(" ");
            break;
          case DBxSelectMaxRecordCountMode.Limit:
            // Добавим в конце
            break;
          default:
            throw new BugException("Неизвестное значение SelectMaxRecordCountMode=" + buffer.Formatter.SelectMaxRecordCountMode);
        }
      }

      #endregion

      #region DISTINCT

      if (_Info.Unique)
        buffer.SB.Append("DISTINCT ");

      #endregion

      #region Список полей и других выражений, которые должны попасть в таблицу

      for (int i = 0; i < _Info.Expressions.Count; i++)
      {
        if (i > 0)
          buffer.SB.Append(", ");

        buffer.FormatExpression(_Info.Expressions[i].Expression, new DBxFormatExpressionInfo());
        if (_Info.Expressions[i].AliasRequired)
        {
          buffer.SB.Append(" AS ");
          buffer.FormatColumnName(_Info.Expressions[i].Alias);
        }
      }

      #endregion

      // Собираем имена для инструкции FROM

      #region Имя (основной) таблицы

      _Buffer2.SB.Length = 0;
      _Buffer2.FormatTableName(_Info.TableName.Trim());

      #endregion

      #region Инструкции LEFT JOIN

      if (_HasDotColumns)
      {
        for (int i = 0; i < _JoinInfoList.Count; i++)
        {
          JoinInfo thisInfo = _JoinInfoList[i];
          if (i > 0)
          {
            _Buffer2.SB.Insert(0, "(");
            // Скобки вокруг предыдущего выражения
            _Buffer2.SB.Append(")");
          }
          _Buffer2.SB.Append(" LEFT JOIN ");
          _Buffer2.FormatTableName(thisInfo.RightTableName);
          _Buffer2.SB.Append(" AS ");
          _Buffer2.FormatTableName(thisInfo.AliasName);
          _Buffer2.SB.Append(" ON ");
          _Buffer2.FormatTableName(thisInfo.LeftTableName);
          _Buffer2.SB.Append(".");
          _Buffer2.FormatColumnName(thisInfo.RefColumnName);
          _Buffer2.SB.Append("=");
          _Buffer2.FormatTableName(thisInfo.AliasName);
          _Buffer2.SB.Append(".");
          // Buffer2.FormatColumnName("Id");
          // 01.10.2019
          // Не всегда первичным ключом является поле "Id"
          DBxTableStruct RightTS = _Validator.Entry.DB.Struct.Tables[thisInfo.RightTableName];
          if (RightTS == null)
            throw new BugException("Не найдено описание мастер-таблицы \"" + thisInfo.RightTableName + "\"");
          if (RightTS.PrimaryKeyColumns.Length != 1)
            throw new BugException("Для мастер-таблицы \"" + thisInfo.RightTableName + "\" не задан первичный ключ или он составной");
          _Buffer2.FormatColumnName(RightTS.PrimaryKeyColumns[0].ColumnName);
        }
      }

      #endregion

      #region FROM

      buffer.SB.Append(" FROM ");
      buffer.SB.Append(_Buffer2.SB.ToString());

      #endregion

      #region WHERE

      if (_Info.Where != null && _Info.Where.Degeneration != DBxFilterDegeneration.AlwaysTrue)
      {
        buffer.SB.Append(" WHERE ");
        buffer.FormatFilter(_Info.Where);
      }

      #endregion

      #region GROUP BY

      if (_Info.GroupBy.Count > 0)
      {
        buffer.SB.Append(" GROUP BY ");
        for (int i = 0; i < _Info.GroupBy.Count; i++)
        {
          if (i > 0)
            buffer.SB.Append(",");
          buffer.FormatExpression(_Info.GroupBy[i], new DBxFormatExpressionInfo());
        }
      }

      #endregion

      #region HAVING

      if (_Info.Having != null && _Info.Having.Degeneration != DBxFilterDegeneration.AlwaysTrue)
      {
        buffer.SB.Append(" HAVING ");
        buffer.FormatFilter(_Info.Having);
      }

      #endregion

      #region ORDER BY

      if (_Info.OrderBy != null)
      {
        buffer.SB.Append(" ORDER BY ");
        buffer.FormatOrder(_Info.OrderBy);
      }

      #endregion

      #region LIMIT

      if (_Info.MaxRecordCount != 0)
      {
        switch (buffer.Formatter.SelectMaxRecordCountMode)
        {
          case DBxSelectMaxRecordCountMode.Limit:
            buffer.SB.Append(" LIMIT ");
            buffer.SB.Append(_Info.MaxRecordCount);
            break;
        }
      }

      #endregion
    }

    #endregion

    #region PrepareColumnNames

    /// <summary>
    /// Заполняем список ColumnNames, если он не задан
    /// </summary>
    private void PrepareColumnNames()
    {
      // 06.12.2017
      // Больше не используем конструкцию "SELECT *"
      // Всегда используем явный список полей
      if (_Info.Expressions.Count > 0)
        return;

      foreach (DBxColumnStruct colDef in _Validator.Entry.DB.Struct.Tables[_Info.TableName].Columns)
      {
        if (_Validator.Entry.Permissions.ColumnModes[_Info.TableName, colDef.ColumnName] == DBxAccessMode.None)
          continue;
        _Info.Expressions.Add(new DBxColumn(colDef.ColumnName));
      }
      if (_Info.Expressions.Count == 0)
        throw new ArgumentException("Для выражения SELECT * FROM " + _Info.TableName + " невозможно получить список полей, так как у пользователя нет прав на просмотр каких-либо столбцов");
    }

    #endregion

    #region PrepareInternal

    #region Внутренние данные

    /// <summary>
    /// Объекты для сбора информации об объединении вида
    /// LEFT JOIN [RightTableName] AS [AliasName] ON [LeftTableName].[RefColumnName]=[AliasName].Id
    /// </summary>
    private class JoinInfo
    {
      #region Поля

      /// <summary>
      /// Имя реальной таблицы, стоящей справа от LEFT JOIN
      /// </summary>
      public string RightTableName;

      /// <summary>
      /// Имя таблицы (обычно основной), которая содержит поле RefColumnName
      /// </summary>
      public string LeftTableName;

      /// <summary>
      /// Имя ссылочного поля 
      /// </summary>
      public string RefColumnName;

      /// <summary>
      /// Псевдоним для таблицы RightTableName
      /// </summary>
      public string AliasName;

      #endregion

      #region Поиск

      /// <summary>
      /// Поиск уже существующего элемента в массиве. Если он не найден, 
      /// добавляем новый элемент
      /// </summary>
      /// <param name="items"></param>
      /// <param name="leftTableName"></param>
      /// <param name="rightTableName"></param>
      /// <param name="refColumnName"></param>
      public static JoinInfo FindOrAdd(List<JoinInfo> items, string leftTableName, string rightTableName, string refColumnName)
      {
        foreach (JoinInfo info in items)
        {
          if (info.LeftTableName == leftTableName && info.RightTableName == rightTableName && info.RefColumnName == refColumnName)
            return info; // уже есть такая таблица
        }

        // Добавляем новую тему
        JoinInfo newInfo = new JoinInfo();
        newInfo.LeftTableName = leftTableName;
        newInfo.RightTableName = rightTableName;
        newInfo.RefColumnName = refColumnName;
        newInfo.AliasName = rightTableName + "_" + (items.Count + 1).ToString();
        items.Add(newInfo);
        return newInfo;
      }

      #endregion

      #region ToString

      public override string ToString()
      {
        return AliasName;
      }

      #endregion
    }


    /// <summary>
    /// True, если есть поля с точками
    /// </summary>
    private bool _HasDotColumns;

    private List<JoinInfo> _JoinInfoList;

    #endregion

    #region Методы
#if XXX
      private void WriteTableAndName(DBxSqlBuffer Buffer, TableAndColumn TC)
      {
        Buffer2.Clear();
        if (String.IsNullOrEmpty(TC.TableName))
          Buffer2.FormatColumnName(TC.ColumnName);
        else
        {
          Buffer2.FormatTableName(TC.TableName);
          Buffer2.SB.Append('.');
          Buffer2.FormatColumnName(TC.ColumnName);
        }

        Buffer.SB.Append(Buffer2.SB.ToString());
      }
#endif
    #endregion


    /// <summary>
    /// Инициализация HasDotColumns, NameConvPairs и JoinInfoList
    /// </summary>
    private void PrepareInternal(DBxColumnList columnNames2, DBxSqlBuffer buffer)
    {
      #region HasDotColumns

      _HasDotColumns = false;
      // Определяем, есть ли поля с точками. Если они есть, то придется задавать
      // имя таблицы перед всеми полями
      for (int i = 0; i < columnNames2.Count; i++)
      {
        if (columnNames2[i].IndexOf('.') >= 0)
        {
          _HasDotColumns = true;
          break;
        }
      }

      #endregion

      #region NameConvPairs и JoinInfoList

      _JoinInfoList = null;  // Описание таблиц Join
      if (_HasDotColumns)
        _JoinInfoList = new List<JoinInfo>();

      // Это - обязательное действие, иначе форматировщик не получит доступ к структуре столбцов
      // Не надо. Было выполнено в методе Format
      //Validator.CheckTableColumnNames(Info.TableName, ColumnNames2, true, DBxAccessMode.ReadOnly);

      for (int i = 0; i < columnNames2.Count; i++)
      {

        string thisColumnName = columnNames2[i].Trim();
        string thisTableName1 = _Info.TableName; // Без псевдонима
        string thisTableName2 = _Info.TableName; // С псевдонимом

        while (true)
        {
          int p = thisColumnName.IndexOf('.');
          if (p < 0)
            break;
          string name1 = thisColumnName.Substring(0, p); // слева от точки
          // Ищем описание ссылочного поля в описании структуры БД
          if (!_Validator.Entry.DB.Struct.Tables[thisTableName1].Columns.Contains(name1))
            throw new InvalidOperationException("Таблица \"" + thisTableName1 + "\" не содержит поля \"" + name1 + "\" ");
          DBxColumnStruct colDef = _Validator.Entry.DB.Struct.Tables[thisTableName1].Columns[name1];
          if (String.IsNullOrEmpty(colDef.MasterTableName))
            throw new InvalidOperationException("Поле \"" + name1 + "\" таблицы \"" + thisTableName1 + "\" не является ссылочным");

          JoinInfo ji = JoinInfo.FindOrAdd(_JoinInfoList, thisTableName2, colDef.MasterTableName, name1);

          thisTableName1 = colDef.MasterTableName;
          thisTableName2 = ji.AliasName;
          thisColumnName = thisColumnName.Substring(p + 1);
        }

        if (_HasDotColumns)
          buffer.ColumnTableAliases.Add(columnNames2[i], thisTableName2);
      } // счетчик по списку полей 2

      #endregion
    }

    #endregion

    #region Форматизатор

    ///// <summary>
    ///// Флажок устанавливается на время работы GetSqlCmd(),
    ///// Так как объект используется в цепочке не только при обработке запросов FillSelect(), но и
    ///// во всех других запросах, требуется, чтобы этот объект не выполнял никаких действий
    ///// </summary>
    //private bool IsFillSelectActive;

    // TODO: ?

    ///// <summary>
    ///// Форматирование имени столбца.
    ///// </summary>
    ///// <param name="Buffer">Буфер для записи</param>
    ///// <param name="ColumnName">Имя столбца</param>
    //protected override void OnFormatColumnName(DBxSqlBuffer Buffer, string ColumnName)
    //{
    //  if ((!IsFillSelectActive) || (!HasDotColumns))
    //  {
    //    // Не вмешиваемся
    //    base.OnFormatColumnName(Buffer, ColumnName);
    //    return;
    //  }

    //  if (ColumnName.IndexOf('.') >= 0)
    //    WriteTableAndName(Buffer, NameConvPairs[ColumnName]);
    //  else
    //  {
    //    FormatTableName(Buffer, TableName);
    //    Buffer.SB.Append(".");
    //    base.OnFormatColumnName(Buffer, ColumnName);
    //  }
    //}

    #endregion

    #region Оператор TYPES

#if XXX
    /// <summary>
    /// Добавляем предшествующий оператор "TYPES типы;" для SQLite
    /// </summary>
    /// <param name="Buffer"></param>
    private void AddTypesList(DBxSqlBuffer Buffer)
    {
      Buffer.SB.Append("TYPES ");
      for (int i = 0; i < ColumnNames.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(", ");
        DBxColumnStruct cs = GetColumnDef(ColumnNames[i]);
        if (cs != null)
          FormatValueType(Buffer, cs);
      }
      Buffer.SB.Append(";");
    }
#endif

    #endregion

    #region Коррекция имен полей в полученном наборе

    public void CorrectColumnNames(DataTable table)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif
      if (table.Columns.Count < _Info.Expressions.Count) // 20.11.2019
      { 
        ArgumentException e=new ArgumentException("Неправильное количество столбцов в таблице: "+table.Columns.Count.ToString()+". Ожидалось: "+_Info.Expressions.Count.ToString(), "table");
        e.Data["table.ColumnNames"] = DataTools.GetColumnNames(table);
        e.Data["DBxFillSelectInfo"] = _Info;
        throw e;
      }

      for (int i = 0; i < _Info.Expressions.Count; i++)
        table.Columns[i].ColumnName = _Info.Expressions[i].Alias;
    }

    #endregion

    #region Коррекция типов полей

    public void CorrectColumnTypes(ref DataTable table, DBxSqlBuffer buffer)
    {
      if (!buffer.Formatter.UseTypeCorrectionInSelectResult)
        return;

      #region Проверяем наличие несоответствующих типов полей

      bool hasChanges = false;
      DBxColumnStruct[] colStructs = new DBxColumnStruct[table.Columns.Count];
      for (int i = 0; i < table.Columns.Count; i++)
      {
        DBxNamedExpression ne;
        if (!_Info.Expressions.TryGetValue(table.Columns[i].ColumnName, out ne))
          throw new InvalidOperationException("Таблица " + table.TableName + " содержит поле \"" + table.Columns[i].ColumnName + "\", для которого не найден альяс в списке DBxFillSelectInfo.Expressions");

        DBxColumn col = ne.Expression as DBxColumn;
        if (col == null)
          continue; // для функций и констант корректировка невозможна
        DBxColumnStruct cs;
        buffer.ColumnStructs.TryGetValue(table.Columns[i].ColumnName, out cs);
        if (cs == null)
        {
          if (_Validator.NameCheckingEnabled)
            throw new NullReferenceException("Для столбца \"" + col.ColumnName + "\" не найдено описание поля в списке DBxSqlBuffer.ColumnStructs");
          else
            continue; // проверка отключена
        }
        if (table.Columns[i].DataType != cs.DataType)
        {
          colStructs[i] = cs;
          hasChanges = true;
          break;
        }
      }

      if (!hasChanges)
        return; // ничего делать не надо

      #endregion

      #region Создаем правильную структуру таблицы

      DataTable resTable = new DataTable(table.TableName);
      for (int i = 0; i < table.Columns.Count; i++)
      {
        DataColumn newCol;
        if (colStructs[i] != null)
          newCol = colStructs[i].CreateDataColumn(table.Columns[i].ColumnName);
        else
          newCol = DataTools.CloneDataColumn(table.Columns[i]);
        resTable.Columns.Add(newCol);
      }

      #endregion

      #region Копируем данные

      DataTools.CopyRowsToRows(table, resTable, false, true);

      #endregion

      table = resTable;
    }

#if XXX
    /// <summary>
    /// Возвращает описание столбца для заданного имени столбца (с учетом ссылочных полей)
    /// </summary>
    /// <param name="ColumnName">Имя столбца в результирующем наборе</param>
    /// <returns></returns>
    public DBxColumnStruct GetColumnDef(string ColumnName)
    {
#if DEBUG
      if (NameConvPairs == null)
        throw new InvalidOperationException("Не было вызова GetSQLCmd()");
#endif

      if (!NameConvPairs.ContainsKey(ColumnName))
        return null;

      TableAndColumn TC = NameConvPairs[ColumnName];
      string ThisTableName = TC.TableName;
      if (String.IsNullOrEmpty(ThisTableName))
        ThisTableName = Info.TableName;

      DBxTableStruct ts = Validator.Entry.DB.Struct.Tables[Info.TableName];
      if (ts == null)
        return null;
      DBxColumnStruct cs = ts.Columns[ColumnName];
      return cs;
    }
#endif

    #endregion
  }
}
