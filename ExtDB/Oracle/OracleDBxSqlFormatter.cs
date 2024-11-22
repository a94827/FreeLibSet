// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace FreeLibSet.Data.OracleClient
{
  /// <summary>
  /// Форматизатор SQL-выражений для Oracle
  /// </summary>
  public class OracleDBxSqlFormatter : BaseDBxSqlFormatter
  {
    #region Имя таблицы и поля

    // Х.З. насчет имен в кавычках.
    // Судя по документации, если таблица/столбец объявлены без кавычек, то их нельзя использовать и в запросах

    /// <summary>
    /// Имена не должны ни во что заключаться.
    /// </summary>
    protected override BaseDBxSqlFormatter.EnvelopMode NameEnvelopMode { get { return EnvelopMode.None; } }


    ///// <summary>
    ///// Форматирование имени таблицы.
    ///// </summary>
    ///// <param name="Buffer">Буфер для записи</param>
    ///// <param name="TableName">Имя таблицы</param>
    //protected override void OnFormatTableName(DBxSqlBuffer Buffer, string TableName)
    //{
    //  if (String.IsNullOrEmpty(TableName))
    //    throw new ArgumentNullException("TableName");
    //  if (TableName.IndexOf('\"') >= 0)
    //    throw new ArgumentException("Имя таблицы не может содержать кавычки", "TableName");

    //  //Buffer.SB.Append("\"");
    //  Buffer.SB.Append(TableName);
    //  //Buffer.SB.Append("\"");
    //}

    ///// <summary>
    ///// Форматирование имени столбца.
    ///// </summary>
    ///// <param name="Buffer">Буфер для записи</param>
    ///// <param name="ColumnName">Имя столбца</param>
    //protected override void OnFormatColumnName(DBxSqlBuffer Buffer, string ColumnName)
    //{
    //  if (String.IsNullOrEmpty(ColumnName))
    //    throw new ArgumentNullException("ColumnName");
    //  if (ColumnName.IndexOf('\"') >= 0)
    //    throw new ArgumentException("Имя столбца не может содержать кавычки", "ColumnName");

    //  //Buffer.SB.Append("\"");
    //  Buffer.SB.Append(ColumnName);
    //  //Buffer.SB.Append("\"");
    //}

    #endregion

    /// <summary>
    /// Преобразование значения даты и/или времени.
    /// Этот метод вызывается из OnFormatValue().
    /// </summary>
    /// <param name="buffer">Буфер для записи значения</param>
    /// <param name="value">Записываемое значение</param>
    /// <param name="useDate">если true, то должен быть записан компонент даты</param>
    /// <param name="useTime">если true, то должен быть записан компонент времени</param>
    protected override void OnFormatDateTimeValue(DBxSqlBuffer buffer, DateTime value, bool useDate, bool useTime)
    {
      // Даты задаются в формате "'YYYY-MM-DD'"
      // Дата и время задается в формате "'YYYY-MM-DD HH:MM:SS.SSS'"
      // Но, при этом, надо использовать функцию TO_DATE или TO_TIMESTAMP
      buffer.SB.Append("TO_DATE(\'");

      #region Первый аргумент - значение

      if (useDate)
        buffer.SB.Append(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

      if (useTime)
      {
        if (useDate)
          buffer.SB.Append(' ');
        buffer.SB.Append(value.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture));
      }
      buffer.SB.Append("\',\'");

      #endregion

      #region Второй аргумент - формат

      if (useDate)
        buffer.SB.Append("YYYY-MM-DD");
      if (useTime)
      {
        if (useDate)
          buffer.SB.Append(' ');
        buffer.SB.Append("HH:MI:SS");
      }

      #endregion

      buffer.SB.Append("\')");
    }

    #region FormatFilter

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
  }
}
