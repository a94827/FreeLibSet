// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Этот класс используется в качестве базового для специфических форматизаторов баз данных
  /// </summary>
  public abstract class BaseDBxSqlFormatter : CoreDBxSqlFormatter
  {
    // Зарезервировано для поддержки InSelectFilter

    /// <summary>
    /// Добавляет в буфер выражение ON DELETE XXX в соответствии с типом ссылки
    /// </summary>
    /// <param name="buffer">Заполняемый буфер</param>
    /// <param name="refType">Требуемое действие</param>
    public static void FormatRefColumnDeleteAction(DBxSqlBuffer buffer, DBxRefType refType)
    {
      switch (refType)
      {
        case DBxRefType.Disallow:
          buffer.SB.Append(" ON DELETE NO ACTION");
          break;
        case DBxRefType.Delete:
          buffer.SB.Append(" ON DELETE CASCADE");
          break;
        case DBxRefType.Clear:
          buffer.SB.Append(" ON DELETE SET NULL");
          break;
        case DBxRefType.Emulation:
          throw new ArgumentException(Res.DBxFSqlFormatter_Arg_RefTypeEmulation, "refType");
        default:
          throw ExceptionFactory.ArgUnknownValue("refType", refType);
      }
    }

    #region Типы данных

    /// <summary>
    /// Форматирование типа поля для операторов CREATE/ALTER TABLE ADD/ALTER COLUMN. 
    /// Добавляется только тип данных, например, "CHAR(20)".
    /// Имя столбца и выражение NULL/NOT NULL не добавляется.
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="column">Описание столбца</param>
    protected override void FormatValueType(DBxSqlBuffer buffer, DBxColumnStruct column)
    {
      switch (column.ColumnType)
      {
        #region Строка

        case DBxColumnType.String:
          buffer.SB.Append("CHAR(");
          buffer.SB.Append(column.MaxLength);
          buffer.SB.Append(")");
          break;

        #endregion

        #region Логический

        case DBxColumnType.Boolean:
          buffer.SB.Append("BOOLEAN");
          break;

        #endregion

        #region Числа

        case DBxColumnType.Int:
          if (column.MinValue == 0 && column.MaxValue == 0)
            buffer.SB.Append("INTEGER"); // основной тип
          else if (column.MinValue >= Int16.MinValue && column.MaxValue <= Int16.MaxValue)
            buffer.SB.Append("SMALLINT");
          else if (column.MinValue >= Int32.MinValue && column.MaxValue <= Int32.MaxValue)
            buffer.SB.Append("INTEGER");
          else
            buffer.SB.Append("BIGINT");
          break;

        case DBxColumnType.Float:
          if (column.MinValue == 0 && column.MaxValue == 0)
            buffer.SB.Append("DOUBLE PRECISION");
          else if (column.MinValue >= Single.MinValue && column.MaxValue <= Single.MaxValue)
            buffer.SB.Append("REAL");
          else
            buffer.SB.Append("DOUBLE PRECISION");
          break;

        case DBxColumnType.Decimal: // Отдельного денежного типа нет
          buffer.SB.Append("NUMERIC(18,2)");
          break;

        #endregion

        #region Дата / время

        case DBxColumnType.Date: // Только дата
          buffer.SB.Append("DATE");
          break;

        case DBxColumnType.DateTime: // Дата и время
          buffer.SB.Append("TIMESTAMP");
          break;

        case DBxColumnType.Time:
          buffer.SB.Append("TIME");
          break;

        #endregion

        #region GUID

        case DBxColumnType.Guid:
          buffer.SB.Append("CHAR(36)"); // 32 символа + 4 разделителя "-"
          break;

        #endregion

        #region MEMO

        case DBxColumnType.Memo:
          buffer.SB.Append("CLOB"); // Интересно, его кто-нибудь поддерживает?
          break;

        case DBxColumnType.Xml:
          buffer.SB.Append("XML");
          break;

        case DBxColumnType.Binary:
          buffer.SB.Append("BLOB");
          break;

        #endregion

        default:
          throw new BugException("Unknown column type " + column.ColumnType.ToString());
      }
    }

    #endregion


    /// <summary>
    /// Форматирование фильтра.
    /// Вызывает виртуальный метод для фильтра нужного типа.
    /// Обычно нет необходимости переопределять этот метод.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected override void FormatFilter(DBxSqlBuffer buffer, DBxFilter filter)
    {
      if (filter is InSelectFilter)
        OnFormatInSelectFilter(buffer, (InSelectFilter)filter);
      else
        base.FormatFilter(buffer, filter);
    }


    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatInSelectFilter(DBxSqlBuffer buffer, InSelectFilter filter)
    {
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      if (filter.ColumnType != DBxColumnType.Unknown)
        formatInfo.WantedColumnType = filter.ColumnType;

      FormatExpression(buffer, filter.Expression, formatInfo);

      buffer.SB.Append(" IN (");

      //DBxSelectFormatter formatter2=new DBxSelectFormatter(filter.selectInfo, DBxNameValidator.

      buffer.SB.Append(')');
      throw new NotImplementedException(); // TODO: InSelectFilter (04.12.2020)
    }
  }
}
