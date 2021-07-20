using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data;
using System.IO;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace AgeyevAV.ExtDB
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

      this.Info = info;
      this.Validator = validator;
    }

    #endregion

    #region Поля

    private DBxSelectInfo Info;

    private DBxNameValidator Validator;

    #endregion

    #region Создание SQL-запроса

    /// <summary>
    /// Дополнительный буфер для создания строки.
    /// </summary>
    private DBxSqlBuffer Buffer2;

    /// <summary>
    /// Основной метод - получение SQL-выражения
    /// На момент вызова все управляющие поля установлены
    /// </summary>
    /// <param name="buffer">Заполняемый буфер</param>
    public void Format(DBxSqlBuffer buffer)
    {
      #region Проверка аргументов

      if (Buffer2 != null)
        throw new InvalidOperationException("Повторный вызов метода Format не допускается. Объект FillSelectFormatter является одноразовым");

      if (buffer == null)
        throw new ArgumentNullException("buffer");
      if (String.IsNullOrEmpty(Info.TableName))
        throw new InvalidDataException("Не задано DBxSelectInfo.TableName");

      Validator.CheckTableName(Info.TableName, DBxAccessMode.ReadOnly);

      if (Info.HasGroupBy)
      {
        if (Info.Expressions.Count == 0)
          throw new InvalidOperationException("Задан список GROUP BY без задания списка выражений (SELECT *)");
      }
      else
      {
        if (Info.Having != null)
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
      DBxColumnList ColumnNames2 = new DBxColumnList();
      Info.GetColumnNames(ColumnNames2); // все существующие поля

      Validator.CheckTableColumnNames(Info.TableName, ColumnNames2, true, DBxAccessMode.ReadOnly);

      #endregion

      PrepareInternal(ColumnNames2, buffer); // 28.02.2020
      Buffer2 = new DBxSqlBuffer(buffer.Formatter);

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

      if (Info.MaxRecordCount != 0)
      {
        if (Info.MaxRecordCount < 0)
          throw new InvalidOperationException("DBxSelectInfo.MaxRecordCount<0");

        switch (buffer.Formatter.SelectMaxRecordCountMode)
        {
          case DBxSelectMaxRecordCountMode.Top:
            buffer.SB.Append("TOP ");
            buffer.SB.Append(Info.MaxRecordCount);
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

      if (Info.Unique)
        buffer.SB.Append("DISTINCT ");

      #endregion

      #region Список полей и других выражений, которые должны попасть в таблицу

      for (int i = 0; i < Info.Expressions.Count; i++)
      {
        if (i > 0)
          buffer.SB.Append(", ");

        buffer.FormatExpression(Info.Expressions[i].Expression, new DBxFormatExpressionInfo());
        if (Info.Expressions[i].AliasRequired)
        {
          buffer.SB.Append(" AS ");
          buffer.FormatColumnName(Info.Expressions[i].Alias);
        }
      }

      #endregion

      // Собираем имена для инструкции FROM

      #region Имя (основной) таблицы

      Buffer2.SB.Length = 0;
      Buffer2.FormatTableName(Info.TableName.Trim());

      #endregion

      #region Инструкции LEFT JOIN

      if (HasDotColumns)
      {
        for (int i = 0; i < JoinInfoList.Count; i++)
        {
          JoinInfo ThisInfo = JoinInfoList[i];
          if (i > 0)
          {
            Buffer2.SB.Insert(0, "(");
            // Скобки вокруг предыдущего выражения
            Buffer2.SB.Append(")");
          }
          Buffer2.SB.Append(" LEFT JOIN ");
          Buffer2.FormatTableName(ThisInfo.RightTableName);
          Buffer2.SB.Append(" AS ");
          Buffer2.FormatTableName(ThisInfo.AliasName);
          Buffer2.SB.Append(" ON ");
          Buffer2.FormatTableName(ThisInfo.LeftTableName);
          Buffer2.SB.Append(".");
          Buffer2.FormatColumnName(ThisInfo.RefColumnName);
          Buffer2.SB.Append("=");
          Buffer2.FormatTableName(ThisInfo.AliasName);
          Buffer2.SB.Append(".");
          // Buffer2.FormatColumnName("Id");
          // 01.10.2019
          // Не всегда первичным ключом является поле "Id"
          DBxTableStruct RightTS = Validator.Entry.DB.Struct.Tables[ThisInfo.RightTableName];
          if (RightTS == null)
            throw new BugException("Не найдено описание мастер-таблицы \"" + ThisInfo.RightTableName + "\"");
          if (RightTS.PrimaryKeyColumns.Length != 1)
            throw new BugException("Для мастер-таблицы \"" + ThisInfo.RightTableName + "\" не задан первичный ключ или он составной");
          Buffer2.FormatColumnName(RightTS.PrimaryKeyColumns[0].ColumnName);
        }
      }

      #endregion

      #region FROM

      buffer.SB.Append(" FROM ");
      buffer.SB.Append(Buffer2.SB.ToString());

      #endregion

      #region WHERE

      if (Info.Where != null && Info.Where.Degeneration != DBxFilterDegeneration.AlwaysTrue)
      {
        buffer.SB.Append(" WHERE ");
        buffer.FormatFilter(Info.Where);
      }

      #endregion

      #region GROUP BY

      if (Info.GroupBy.Count > 0)
      {
        buffer.SB.Append(" GROUP BY ");
        for (int i = 0; i < Info.GroupBy.Count; i++)
        {
          if (i > 0)
            buffer.SB.Append(",");
          buffer.FormatExpression(Info.GroupBy[i], new DBxFormatExpressionInfo());
        }
      }

      #endregion

      #region HAVING

      if (Info.Having != null && Info.Having.Degeneration != DBxFilterDegeneration.AlwaysTrue)
      {
        buffer.SB.Append(" HAVING ");
        buffer.FormatFilter(Info.Having);
      }

      #endregion

      #region ORDER BY

      if (Info.OrderBy != null)
      {
        buffer.SB.Append(" ORDER BY ");
        buffer.FormatOrder(Info.OrderBy);
      }

      #endregion

      #region LIMIT

      if (Info.MaxRecordCount != 0)
      {
        switch (buffer.Formatter.SelectMaxRecordCountMode)
        {
          case DBxSelectMaxRecordCountMode.Limit:
            buffer.SB.Append(" LIMIT ");
            buffer.SB.Append(Info.MaxRecordCount);
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
      if (Info.Expressions.Count > 0)
        return;

      foreach (DBxColumnStruct ColDef in Validator.Entry.DB.Struct.Tables[Info.TableName].Columns)
      {
        if (Validator.Entry.Permissions.ColumnModes[Info.TableName, ColDef.ColumnName] == DBxAccessMode.None)
          continue;
        Info.Expressions.Add(new DBxColumn(ColDef.ColumnName));
      }
      if (Info.Expressions.Count == 0)
        throw new ArgumentException("Для выражения SELECT * FROM " + Info.TableName + " невозможно получить список полей, так как у пользователя нет прав на просмотр каких-либо столбцов");
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
      /// <param name="Items"></param>
      /// <param name="LeftTableName"></param>
      /// <param name="RightTableName"></param>
      /// <param name="RefColumnName"></param>
      public static JoinInfo FindOrAdd(List<JoinInfo> Items, string LeftTableName, string RightTableName, string RefColumnName)
      {
        foreach (JoinInfo Info in Items)
        {
          if (Info.LeftTableName == LeftTableName && Info.RightTableName == RightTableName && Info.RefColumnName == RefColumnName)
            return Info; // уже есть такая таблица
        }

        // Добавляем новую тему
        JoinInfo NewInfo = new JoinInfo();
        NewInfo.LeftTableName = LeftTableName;
        NewInfo.RightTableName = RightTableName;
        NewInfo.RefColumnName = RefColumnName;
        NewInfo.AliasName = RightTableName + "_" + (Items.Count + 1).ToString();
        Items.Add(NewInfo);
        return NewInfo;
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
    private bool HasDotColumns;

    private List<JoinInfo> JoinInfoList;

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

      HasDotColumns = false;
      // Определяем, есть ли поля с точками. Если они есть, то придется задавать
      // имя таблицы перед всеми полями
      for (int i = 0; i < columnNames2.Count; i++)
      {
        if (columnNames2[i].IndexOf('.') >= 0)
        {
          HasDotColumns = true;
          break;
        }
      }

      #endregion

      #region NameConvPairs и JoinInfoList

      JoinInfoList = null;  // Описание таблиц Join
      if (HasDotColumns)
        JoinInfoList = new List<JoinInfo>();

      // Это - обязательное действие, иначе форматировщик не получит доступ к структуре столбцов
      // Не надо. Было выполнено в методе Format
      //Validator.CheckTableColumnNames(Info.TableName, ColumnNames2, true, DBxAccessMode.ReadOnly);

      for (int i = 0; i < columnNames2.Count; i++)
      {

        string ThisFieldName = columnNames2[i].Trim();
        string ThisTableName1 = Info.TableName; // Без псевдонима
        string ThisTableName2 = Info.TableName; // С псевдонимом

        while (true)
        {
          int p = ThisFieldName.IndexOf('.');
          if (p < 0)
            break;
          string Name1 = ThisFieldName.Substring(0, p); // слева от точки
          // Ищем описание ссылочного поля в описании структуры БД
          if (!Validator.Entry.DB.Struct.Tables[ThisTableName1].Columns.Contains(Name1))
            throw new InvalidOperationException("Таблица \"" + ThisTableName1 + "\" не содержит поля \"" + Name1 + "\" ");
          DBxColumnStruct FieldDef = Validator.Entry.DB.Struct.Tables[ThisTableName1].Columns[Name1];
          if (String.IsNullOrEmpty(FieldDef.MasterTableName))
            throw new InvalidOperationException("Поле \"" + Name1 + "\" таблицы \"" + ThisTableName1 + "\" не является ссылочным");

          JoinInfo ji = JoinInfo.FindOrAdd(JoinInfoList, ThisTableName2, FieldDef.MasterTableName, Name1);

          ThisTableName1 = FieldDef.MasterTableName;
          ThisTableName2 = ji.AliasName;
          ThisFieldName = ThisFieldName.Substring(p + 1);
        }

        if (HasDotColumns)
          buffer.ColumnTableAliases.Add(columnNames2[i], ThisTableName2);
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
      if (table.Columns.Count < Info.Expressions.Count) // 20.11.2019
      { 
        ArgumentException e=new ArgumentException("Неправильное количество столбцов в таблице: "+table.Columns.Count.ToString()+". Ожидалось: "+Info.Expressions.Count.ToString(), "table");
        e.Data["table.ColumnNames"] = DataTools.GetColumnNames(table);
        e.Data["DBxFillSelectInfo"] = Info;
        throw e;
      }

      for (int i = 0; i < Info.Expressions.Count; i++)
        table.Columns[i].ColumnName = Info.Expressions[i].Alias;
    }

    #endregion

    #region Коррекция типов полей


    public void CorrectColumnTypes(ref DataTable Table, DBxSqlBuffer buffer)
    {
      if (!buffer.Formatter.UseTypeCorrectionInSelectResult)
        return;

      #region Проверяем наличие несоответствующих типов полей

      bool HasChanges = false;
      DBxColumnStruct[] ColStructs = new DBxColumnStruct[Table.Columns.Count];
      for (int i = 0; i < Table.Columns.Count; i++)
      {
        DBxNamedExpression ne;
        if (!Info.Expressions.TryGetValue(Table.Columns[i].ColumnName, out ne))
          throw new InvalidOperationException("Таблица " + Table.TableName + " содержит поле \"" + Table.Columns[i].ColumnName + "\", для которого не найден альяс в списке DBxFillSelectInfo.Expressions");

        DBxColumn col = ne.Expression as DBxColumn;
        if (col == null)
          continue; // для функций и констант корректировка невозможна
        DBxColumnStruct cs;
        buffer.ColumnStructs.TryGetValue(Table.Columns[i].ColumnName, out cs);
        if (cs == null)
        {
          if (Validator.NameCheckingEnabled)
            throw new NullReferenceException("Для столбца \"" + col.ColumnName + "\" не найдено описание поля в списке DBxSqlBuffer.ColumnStructs");
          else
            continue; // проверка отключена
        }
        if (Table.Columns[i].DataType != cs.DataType)
        {
          ColStructs[i] = cs;
          HasChanges = true;
          break;
        }
      }

      if (!HasChanges)
        return; // ничего делать не надо

      #endregion

      #region Создаем правильную структуру таблицы

      DataTable ResTable = new DataTable(Table.TableName);
      for (int i = 0; i < Table.Columns.Count; i++)
      {
        DataColumn NewCol;
        if (ColStructs[i] != null)
          NewCol = ColStructs[i].CreateDataColumn(Table.Columns[i].ColumnName);
        else
          NewCol = DataTools.CloneDataColumn(Table.Columns[i]);
        ResTable.Columns.Add(NewCol);
      }

      #endregion

      #region Копируем данные

      DataTools.CopyRowsToRows(Table, ResTable, false, true);

      #endregion

      Table = ResTable;
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
