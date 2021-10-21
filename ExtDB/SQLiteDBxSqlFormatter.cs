using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2017, Ageyev A.V.
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

namespace FreeLibSet.Data.SQLite
{
  /// <summary>
  /// Форматизатор SQL-выражений для SQLite
  /// </summary>
  public class SQLiteDBxSqlFormatter : BaseDBxSqlFormatter
  {
    #region Конструктор

    /// <summary>
    /// Создает форматизатор
    /// </summary>
    /// <param name="db">База данных</param>
    public SQLiteDBxSqlFormatter(SQLiteDBx db)
    {
      _DB = db;
    }

    /// <summary>
    /// Доступ к базе данных нужен для определения формата даты и времени
    /// </summary>
    private SQLiteDBx _DB;

    #endregion

    #region Имя таблицы и поля

    /// <summary>
    /// Имена заключаются в кавычки
    /// </summary>
    protected override BaseDBxSqlFormatter.EnvelopMode NameEnvelopMode { get { return EnvelopMode.Quotation; } }

    #endregion

    #region Типы значений

    /// <summary>
    /// Форматирование типа поля для операторов CREATE/ALTER TABLE ADD/ALTER COLUMN. 
    /// Добавляется только тип данных, например, "CHAR(20)".
    /// Имя столбца и выражение NULL/NOT NULL не добавляется.
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="column">Описание столбца</param>
    protected override void OnFormatValueType(DBxSqlBuffer buffer, DBxColumnStruct column)
    {
      switch (column.ColumnType)
      {
        case DBxColumnType.Int:
          if (column.MinValue == 0 && column.MaxValue == 0)
            buffer.SB.Append("INT"); // основной тип
          else if (column.MinValue >= Byte.MinValue && column.MaxValue <= Byte.MaxValue)
            buffer.SB.Append("TINYINT");
          else if (column.MinValue >= Int16.MinValue && column.MaxValue <= Int16.MaxValue)
            buffer.SB.Append("SMALLINT");
          else if (column.MinValue >= Int32.MinValue && column.MaxValue <= Int32.MaxValue)
            buffer.SB.Append("INT");
          else
            buffer.SB.Append("BIGINT");
          break;

        case DBxColumnType.Guid:
          buffer.SB.Append("GUID");
          break;

        default:
          base.OnFormatValueType(buffer, column);
          break;
      }
    }

    #endregion

    #region Значения

    /// <summary>
    /// Логические значения записываются как целые числа 0 или 1.
    /// </summary>
    /// <param name="buffer">Буфер для записи значения</param>
    /// <param name="value">Записываемое значение</param>
    protected override void OnFormatBooleanValue(DBxSqlBuffer buffer, bool value)
    {
      // 06.08.2018
      buffer.SB.Append(value ? "1" : "0");
    }


    /// <summary>
    /// Преобразование значения даты и/или времени.
    /// Форматы даты и времени хранятся как свойства SQLiteDbx.DateFormat и TimeFormat.
    /// Этот метод вызывается из OnFormatValue().
    /// </summary>
    /// <param name="buffer">Буфер для записи значения</param>
    /// <param name="value">Записываемое значение</param>
    /// <param name="useDate">если true, то должен быть записан компонент даты</param>
    /// <param name="useTime">если true, то должен быть записан компонент времени</param>
    protected override void OnFormatDateTimeValue(DBxSqlBuffer buffer, DateTime value, bool useDate, bool useTime)
    {
      // 31.07.2018
      // Даты задаются в формате "'YYYY-MM-DD'"
      // Дата и время задается в формате "'YYYY-MM-DD HH:MM:SS.SSS'"

      // 03.10.2018
      // Проверяем даты
      if (useDate)
      {
        if (value.Year < 1000)
          throw new ArgumentOutOfRangeException("value", value,
            "В полях типа \"дата\" и \"дата-время\" нельзя задавать год меньше 1000. База данных SQLite не может потом прочитать такие значения");
      }

      // 08.10.2018
      // Формат даты и времени можно настраивать.
      // В программе CoProDev используется формат даты "'yyyyMMdd'" без разделителей.
      // Х.З., почему так получилось, т.к. в базе данных поля дат объявлены одинаково, но хранят разные значения 

      buffer.SB.Append("'");
      if (useDate)
        buffer.SB.Append(value.ToString(_DB.DateFormat, CultureInfo.InvariantCulture));

      if (useTime)
      {
        if (useDate)
          buffer.SB.Append(' ');
        buffer.SB.Append(value.ToString(_DB.TimeFormat, CultureInfo.InvariantCulture));
      }
      buffer.SB.Append("'");
    }

    /// <summary>
    /// Записывает GUID-значение в 16-ричном представлении для BLOB-поля в виде строки x'1234567890abcdef1234567890abcdef'
    /// </summary>
    /// <param name="buffer">Заполняемый буфер</param>
    /// <param name="value">Значение</param>
    protected override void OnFormatGuidValue(DBxSqlBuffer buffer, Guid value)
    {
      // 18.01.2020
      // Записываем GUID как BLOB-значение, а не строку

      buffer.SB.Append(@"x'");

      // Так будет неправильный порядок байт!
      // buffer.SB.Append(value.ToString("N"));

      byte[] a = value.ToByteArray();
      DataTools.BytesToHex(buffer.SB, a, false);

      buffer.SB.Append(@"'");
    }

    /// <summary>
    /// Преобразует GUID в массив байт
    /// </summary>
    /// <param name="value"></param>
    /// <param name="columnType"></param>
    /// <returns></returns>
    protected override object OnPrepareParamValue(object value, DBxColumnType columnType)
    {
      if (value is Guid)
        return ((Guid)value).ToByteArray();

      // ? Может быть и DateTime надо преобразовывать

      return value;
    }

    #endregion

    #region FormatFilter


    ///// <summary>
    ///// Заменяем функцию ISNULL(a,b) на COALESCE
    ///// </summary>
    ///// <param name="Buffer"></param>
    ///// <param name="Filter"></param>
    //protected override void OnFormatValueFilter(DBxSqlBuffer Buffer, ValueFilter Filter)
    //{
    //  // Для просто значения null используем функцию IsNull()
    //  if (Filter.Value == null || Filter.Value is DBNull)
    //  {
    //    if (Filter.Kind != ValueFilterKind.Equal)
    //      throw new InvalidOperationException("Значение NULL в фильтре сравнения допускается только для сравнения на равенство (поле \"" + Filter.ColumnName + "\")");

    //    Buffer.FormatColumnName(Filter.ColumnName);
    //    Buffer.SB.Append(" IS NULL");
    //    return;
    //  }

    //  if (Filter.Kind == ValueFilterKind.Equal)
    //  {
    //    // Для значений 0 и false используем ISNULL() в комбинации со сравнением
    //    if (DataTools.IsEmptyValue(Filter.Value))
    //    {
    //      Buffer.SB.Append("COALESCE(");
    //      Buffer.FormatColumnName(Filter.ColumnName);
    //      Buffer.SB.Append(',');
    //      Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
    //      Buffer.SB.Append(")=");
    //      Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
    //      return;
    //    }
    //  }

    //  Buffer.FormatColumnName(Filter.ColumnName);
    //  Buffer.SB.Append(GetSignStr(Filter.Kind));
    //  Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
    //}

    // TODO: ?

    ///// <summary>
    ///// Форматирование фильтра
    ///// </summary>
    ///// <param name="Buffer">Буфер для создания SQL-выражения</param>
    ///// <param name="Filter">Фильтр</param>
    //protected override void OnFormatValuesFilter(DBxSqlBuffer Buffer, ValuesFilter Filter)
    //{
    //  if (Filter.Values.Length == 1)
    //  {
    //    // Как обычный ValueFilter
    //    object Value = Filter.Values.GetValue(0);
    //    if (DataTools.IsEmptyValue(Value)) // 04.03.2016
    //    {
    //      Buffer.FormatColumnName(Filter.ColumnName);
    //      Buffer.SB.Append(" IS NULL");
    //    }
    //    else
    //    {
    //      Buffer.FormatColumnName(Filter.ColumnName);
    //      Buffer.SB.Append('=');
    //      Buffer.FormatValue(Value, DBxColumnType.Unknown);
    //    }
    //    return;
    //  }

    //  // Сложный фильтр использует IN

    //  for (int j = 0; j < Filter.Values.Length; j++)
    //  {
    //    object Value = Filter.Values.GetValue(j);
    //    // Как обычный ValueFilter
    //    if (DataTools.IsEmptyValue(Value)) // 04.03.2016
    //    {
    //      Buffer.SB.Append("COALESCE(");
    //      Buffer.FormatColumnName(Filter.ColumnName);
    //      Buffer.SB.Append(',');
    //      Buffer.FormatValue(Value, DBxColumnType.Unknown);
    //      Buffer.SB.Append(") IN (");
    //      for (int i = 0; i < Filter.Values.Length; i++)
    //      {
    //        if (i > 0)
    //          Buffer.SB.Append(", ");
    //        Buffer.FormatValue(Filter.Values.GetValue(i), DBxColumnType.Unknown);
    //      }
    //      Buffer.SB.Append(')');

    //      return;
    //    }
    //  }

    //  Buffer.FormatColumnName(Filter.ColumnName);
    //  Buffer.SB.Append(" IN (");
    //  for (int i = 0; i < Filter.Values.Length; i++)
    //  {
    //    if (i > 0)
    //      Buffer.SB.Append(", ");
    //    Buffer.FormatValue(Filter.Values.GetValue(i), DBxColumnType.Unknown);
    //  }
    //  Buffer.SB.Append(')');
    //}

    //protected override void OnFormatDateRangeInclusionFilter(DBxSqlBuffer Buffer, DateRangeInclusionFilter Filter)
    //{
    //  // В текущей реализации компонент времени не поддерживается
    //  // TODO: Надо проверить, что получается
    //  Buffer.SB.Append("(COALESCE(");
    //  Buffer.FormatColumnName(Filter.FirstColumnName);
    //  Buffer.SB.Append(", ");
    //  Buffer.FormatValue(DateTime.MinValue, DBxColumnType.Unknown);
    //  Buffer.SB.Append(")<=");
    //  Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);

    //  Buffer.SB.Append(") AND (COALESCE(");
    //  Buffer.FormatColumnName(Filter.LastColumnName);
    //  Buffer.SB.Append(", ");
    //  Buffer.FormatValue(DateTime.MaxValue, DBxColumnType.Unknown);
    //  Buffer.SB.Append(")>=");
    //  Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
    //  Buffer.SB.Append(")");
    //}


    // TODO: Вместо LIKE использовать GLOB для регистрочувствительных фильтров. Нужно дополнение (?) для русских символов

    #endregion

    #region FormatOrder

    // TODO: ?

    ///// <summary>
    ///// Форматирование элемента порядка сортировки.
    ///// Вызывает виртуальные методы для записи конкретных элементов сортировки.
    ///// Метод не должен добавлять суффиксы "ASC" и "DESC".
    ///// Обычно этот метод не переопределяется
    ///// </summary>
    ///// <param name="Buffer">Буфер для записи</param>
    ///// <param name="OrderItem">Элемент порядка сортировки</param>
    //protected override void OnFormatOrderColumnIfNull(DBxSqlBuffer Buffer, DBxOrderColumnIfNull OrderItem)
    //{
    //  Buffer.SB.Append("IF(");
    //  Buffer.FormatColumnName(OrderItem.ColumnName);
    //  Buffer.SB.Append(" IS NULL, ");
    //  Buffer.FormatOrderItem(OrderItem.IfNull);
    //  Buffer.SB.Append(", ");
    //  Buffer.FormatColumnName(OrderItem.ColumnName);
    //  Buffer.SB.Append(")");
    //}

    #endregion

    #region SELECT

    /// <summary>
    /// Используется форма SELECT ... LIMIT x
    /// </summary>
    public override DBxSelectMaxRecordCountMode SelectMaxRecordCountMode
    { get { return DBxSelectMaxRecordCountMode.Limit; } }

    /// <summary>
    /// Используем "TYPES ...; SELECT ..."
    /// </summary>
    public override bool UseTypeCorrectionInSelectResult { get { return true; } }

    #endregion

    #region Параметры запроса

    /// <summary>
    /// Добавляет в <paramref name="buffer"/>.SB место для подстановки параметра, ":P1",":P2",... 
    /// </summary>
    /// <param name="buffer">Буфер, в котором создается SQL-запрос</param>
    /// <param name="paramIndex">Индекс параметра (0,1, ...).</param>
    protected override void OnFormatParamPlaceholder(DBxSqlBuffer buffer, int paramIndex)
    {
      buffer.SB.Append(":P");
      //buffer.SB.Append(paramIndex + 1).ToString();
      buffer.SB.Append((paramIndex + 1).ToString()); // 28.12.2020
    }

    #endregion

    #region Прочее

    /// <summary>
    /// Наугад, возвращаем 1000
    /// </summary>
    public override int MaxInsertIntoValueRowCount { get { return 1000; } }

    private const int SQLITE_MAX_SQL_LENGTH = 1000000000;

    /// <summary>
    /// Максимальная длина SQL-запроса
    /// </summary>
    public override int MaxSqlLength
    {
      get
      {
        return SQLITE_MAX_SQL_LENGTH;
      }
    }

    #endregion
  }
}
