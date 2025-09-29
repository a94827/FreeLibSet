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

        case DBxColumnType.SByte:
          if (column.IsReplaceableType(DBxColumnType.Byte))
            buffer.SB.Append("TINYINT"); 
          else
            buffer.SB.Append("SMALLINT");
          break;
        case DBxColumnType.Byte:
          buffer.SB.Append("TINYINT");
          break;
        case DBxColumnType.Int16:
          buffer.SB.Append("SMALLINT");
          break;
        case DBxColumnType.UInt16:
          if (column.IsReplaceableType(DBxColumnType.Int16))
            buffer.SB.Append("SMALLINT");
          else
            buffer.SB.Append("INTEGER"); 
          break;
        case DBxColumnType.Int32:
          buffer.SB.Append("INTEGER");
          break;
        case DBxColumnType.UInt32:
          if (column.IsReplaceableType(DBxColumnType.Int32))
            buffer.SB.Append("INTEGER");
          else
            buffer.SB.Append("BIGINT");
          break;
        case DBxColumnType.Int64:
        case DBxColumnType.UInt64:
          buffer.SB.Append("BIGINT");
          break;

        case DBxColumnType.Single:
          buffer.SB.Append("REAL");
          break;

        case DBxColumnType.Double:
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
