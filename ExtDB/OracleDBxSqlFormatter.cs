using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

/*
 * The BSD License
 * 
 * Copyright (c) 2016, Ageyev A.V.
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

namespace AgeyevAV.ExtDB.OracleClient
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

    #region Параметры запроса

    /// <summary>
    /// Добавляет в <paramref name="buffer"/>.SB место для подстановки параметра, "@P1",
    /// "@P2",... 
    /// </summary>
    /// <param name="buffer">Буфер, в котором создается SQL-запрос</param>
    /// <param name="paramIndex">Индекс параметра (0,1, ...).</param>
    protected override void OnFormatParamPlaceholder(DBxSqlBuffer buffer, int paramIndex)
    {
      buffer.SB.Append("@P");
      //buffer.SB.Append(paramIndex + 1).ToString();
      buffer.SB.Append((paramIndex + 1).ToString()); // 28.12.2020
    }

    #endregion
  }
}
