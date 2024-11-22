using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Этот класс используется в качестве базового класса для BaseDBxSqlFormatter (ExtDB.dll) и для <see cref="DataViewDBxSqlFormatter"/>.
  /// </summary>                                         
  public abstract class CoreDBxSqlFormatter : DBxSqlFormatter
  {
    #region Имена таблиц и полей

    /// <summary>
    /// Способ оборочивания имен таблиц, альясов и полей.
    /// Обычно используются кавычки, но для <see cref="System.Data.DataView"/>, MS SQL Server и Access должны использоваться квадратные скобки.
    /// </summary>
    protected enum EnvelopMode
    {
      /// <summary>
      /// Основной вариант - имена заключаются в двойные кавычки
      /// </summary>
      Quotation,

      /// <summary>
      /// Для баз данных Microsoft - квадратные скобки
      /// </summary>
      Brackets,

      /// <summary>
      /// Имена не должны экранироваться
      /// </summary>
      None,

      /// <summary>
      /// Все методы реализуются производным классом самостоятельно.
      /// <see cref="CoreDBxSqlFormatter"/> вызывает исключение.
      /// </summary>
      Unsupported
    }

    /// <summary>
    /// Правила обрамления имен таблиц, полей и индексов.
    /// По умолчанию, имена заключаются в кавычки.
    /// </summary>
    protected virtual EnvelopMode NameEnvelopMode { get { return EnvelopMode.Quotation; } }

    /// <summary>
    /// Форматирование имени таблицы.
    /// Непереопределенный метод заключает имя в кавычки или скобки, в зависимости от свойства <see cref="NameEnvelopMode"/>.
    /// </summary>
    /// <param name="buffer">Буфер для формирования SQL-запроса</param>
    /// <param name="tableName">Имя таблицы</param>
    internal protected override void FormatTableName(DBxSqlBuffer buffer, string tableName)
    {
      DoFormatName(buffer, tableName);
    }

    /// <summary>
    /// Форматирование имени поля.
    /// Непереопределенный метод заключает имя в кавычки или скобки, в зависимости от свойства <see cref="NameEnvelopMode"/>.
    /// </summary>
    /// <param name="buffer">Буфер для формирования SQL-запроса</param>
    /// <param name="columnName">Имя столбца</param>
    internal protected override void FormatColumnName(DBxSqlBuffer buffer, string columnName)
    {
      DoFormatName(buffer, columnName);
    }

    private void DoFormatName(DBxSqlBuffer buffer, string name)
    {
      switch (NameEnvelopMode)
      {
        case EnvelopMode.Quotation:
          buffer.SB.Append("\"");
          buffer.SB.Append(name);
          buffer.SB.Append("\"");
          break;
        case EnvelopMode.Brackets:
          buffer.SB.Append("[");
          buffer.SB.Append(name);
          buffer.SB.Append("]");
          break;
        case EnvelopMode.None:
          buffer.SB.Append(name);
          break;
        case EnvelopMode.Unsupported:
          throw new NotSupportedException("Метод должен быть реализован в производном классе");
        default:
          throw new BugException("Недопустимое значение свойства NameEnvelopMode=" + NameEnvelopMode.ToString());
      }
    }

    /// <summary>
    /// Форматирование имени поля и альяса таблицы для запросов SELECT c конструкцией JOIN.
    /// Непереопределенный метод вызывает <see cref="FormatTableName(DBxSqlBuffer, string)"/> для форматирования альяса, добавляет точку и 
    /// форматирует имя поля вызовом <see cref="FormatColumnName(DBxSqlBuffer, string)"/>.
    /// </summary>
    /// <param name="buffer">Буфер для формирования SQL-запроса</param>
    /// <param name="tableAlias">Альяс таблицы (до точки)</param>
    /// <param name="columnName">Имя столбца (после точки)</param>
    internal protected override void FormatColumnName(DBxSqlBuffer buffer, string tableAlias, string columnName)
    {
      FormatTableName(buffer, tableAlias);
      buffer.SB.Append('.');
      FormatColumnName(buffer, columnName);
    }

    #endregion

    #region Значения

    /// <summary>
    /// Форматирование значения поля
    /// Записывает в <paramref name="buffer"/>.SB значение <paramref name="value"/>,
    /// возможно, заключенное в апострофы.
    /// Также отвечает за экранирование символов строкового значения, например, удвоение апострофов.
    /// Метод не выполняет форматирование самостоятельно, а вызывает один из методов OnFormatXxxValue() для значения соответствующего типа.
    /// Идентичные действия выполняются методом <see cref="DataTools.FormatDataValue(object)"/> в ExtTools.dll.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="value">Записываемое значение</param>
    /// <param name="columnType">Тип значения</param>
    internal protected sealed /*временно*/ override void FormatValue(DBxSqlBuffer buffer, object value, DBxColumnType columnType)
    {
      if (value == null)
      {
        OnFormatNullValue(buffer);
        return;
      }
      if (value is DBNull)
      {
        OnFormatNullValue(buffer);
        return;
      }

      if (value is String)
      {
        //// Для DataView строка берется в апострофы. Также выполняются замены
        //// См. раздел справки "DataColumn.Expression Property"

        // 15.02.2016
        // Для строковой константы удваиваем апострофы
        // Остальные символы не заменяются

        string s = (string)value;

        if (columnType == DBxColumnType.Guid)
        {
          // 07.10.2019
          Guid g = new Guid(s);
          OnFormatGuidValue(buffer, g);
        }
        else
          OnFormatStringValue(buffer, s);
        return;
      }

      if (value is Int64)
      {
        OnFormatInt64Value(buffer, (long)value);
        return;
      }

      if (DataTools.IsIntegerType(value.GetType()))
      {
        OnFormatIntValue(buffer, DataTools.GetInt(value)); // нельзя использовать преобразование "(int)value", так как byte -> int вызывает исключение
        return;
      }

      // Числа с плавающей точкой преобразуем с использованием точки,
      // а не разделителя по умолчанию
      if (value is Single)
      {
        OnFormatSingleValue(buffer, (float)value);
        return;
      }
      if (value is Double)
      {
        OnFormatDoubleValue(buffer, (double)value);
        return;
      }
      if (value is Decimal)
      {
        OnFormatDecimalValue(buffer, (decimal)value);
        return;
      }

      if (value is Boolean)
      {
        OnFormatBooleanValue(buffer, (bool)value);
        return;
      }

      if (value is DateTime)
      {
        DateTime timeValue = (DateTime)value;
        timeValue = DateTime.SpecifyKind(timeValue, DateTimeKind.Unspecified);

        bool useDate, useTime;
        switch (columnType)
        {
          case DBxColumnType.Date:
            useDate = true;
            useTime = false;
            break;
          case DBxColumnType.DateTime:
            useDate = true;
            useTime = true;
            break;
          case DBxColumnType.Time:
            useDate = false;
            useTime = true;
            break;
          default:
            useDate = true;
            useTime = (int)(timeValue.TimeOfDay.TotalSeconds) != 0;
            break;
        }

        OnFormatDateTimeValue(buffer, timeValue, useDate, useTime);
        return;
      }

      if (value is TimeSpan)
      {
        DateTime timeValue = new DateTime(((TimeSpan)value).Ticks, DateTimeKind.Unspecified);
        OnFormatDateTimeValue(buffer, timeValue, false, true);
        return;
      }

      if (value is Guid)
      {
        OnFormatGuidValue(buffer, (Guid)value);
        return;
      }

      throw new NotImplementedException("Значение " + value.ToString() + " имеет неизвестный тип " + value.GetType().ToString());
    }

    /// <summary>
    /// Форматирование строкового значения
    /// Записывает в <paramref name="buffer"/>.SB значение <paramref name="value"/>, заключенное в апострофы.
    /// Если строка содержит апострофы, то они удваиваются.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="value">Записываемое значение</param>
    protected virtual void OnFormatStringValue(DBxSqlBuffer buffer, string value)
    {
      buffer.SB.Append(@"'");
      for (int i = 0; i < value.Length; i++)
      {
        if (value[i] == '\'')
          buffer.SB.Append(@"''");
        else
          buffer.SB.Append(value[i]);
      }
      buffer.SB.Append(@"'");
    }

    /// <summary>
    /// Форматирование числового значения
    /// Записывает в <paramref name="buffer"/>.SB значение <paramref name="value"/>.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="value">Записываемое значение</param>
    protected virtual void OnFormatIntValue(DBxSqlBuffer buffer, int value)
    {
      buffer.SB.Append(StdConvert.ToString(value));
    }

    /// <summary>
    /// Форматирование числового значения
    /// Записывает в <paramref name="buffer"/>.SB значение <paramref name="value"/>.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="value">Записываемое значение</param>
    protected virtual void OnFormatInt64Value(DBxSqlBuffer buffer, long value)
    {
      buffer.SB.Append(StdConvert.ToString(value));
    }

    /// <summary>
    /// Форматирование числового значения
    /// Записывает в <paramref name="buffer"/>.SB значение <paramref name="value"/>.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="value">Записываемое значение</param>
    protected virtual void OnFormatSingleValue(DBxSqlBuffer buffer, float value)
    {
      buffer.SB.Append(StdConvert.ToString(value));
    }

    /// <summary>
    /// Форматирование числового значения
    /// Записывает в <paramref name="buffer"/>.SB значение <paramref name="value"/>.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="value">Записываемое значение</param>
    protected virtual void OnFormatDoubleValue(DBxSqlBuffer buffer, double value)
    {
      buffer.SB.Append(StdConvert.ToString(value));
    }

    /// <summary>
    /// Форматирование числового значения
    /// Записывает в <paramref name="buffer"/>.SB значение <paramref name="value"/>.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="value">Записываемое значение</param>
    protected virtual void OnFormatDecimalValue(DBxSqlBuffer buffer, decimal value)
    {
      buffer.SB.Append(StdConvert.ToString(value));
    }

    /// <summary>
    /// Форматирование логического значения.
    /// Записывает в <paramref name="buffer"/>.SB значение <paramref name="value"/>.
    /// Непереопределенный метод записывает "TRUE" или "FALSE".
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="value">Записываемое значение</param>
    protected virtual void OnFormatBooleanValue(DBxSqlBuffer buffer, bool value)
    {
      buffer.SB.Append(value ? "TRUE" : "FALSE");
    }

    /// <summary>
    /// Преобразование значения даты и/или времени.
    /// Этот метод вызывается из <see cref="FormatValue(DBxSqlBuffer, object, DBxColumnType)"/>.
    /// Даты задаются в формате "#M/D/YYYY#"
    /// Дата и время задается в формате "#M/D/YYYY H:M:S#"
    /// </summary>
    /// <param name="buffer">Буфер для записи значения</param>
    /// <param name="value">Записываемое значение</param>
    /// <param name="useDate">если true, то должен быть записан компонент даты</param>
    /// <param name="useTime">если true, то должен быть записан компонент времени</param>
    protected virtual void OnFormatDateTimeValue(DBxSqlBuffer buffer, DateTime value, bool useDate, bool useTime)
    {
      buffer.SB.Append('#');

      if (useDate)
      {
        buffer.SB.Append(StdConvert.ToString(value.Month));
        buffer.SB.Append('/');
        buffer.SB.Append(StdConvert.ToString(value.Day));
        buffer.SB.Append('/');
        buffer.SB.Append(StdConvert.ToString(value.Year));
      }
      if (useTime)
      {
        if (useDate)
          buffer.SB.Append(' ');
        buffer.SB.Append(StdConvert.ToString(value.Hour));
        buffer.SB.Append(':');
        buffer.SB.Append(StdConvert.ToString(value.Minute));
        buffer.SB.Append(':');
        buffer.SB.Append(StdConvert.ToString(value.Second));
      }
      buffer.SB.Append('#');
    }

    /// <summary>
    /// Форматирование значения типа GUID.
    /// Записывает в <paramref name="buffer"/>.SB значение <paramref name="value"/>.
    /// Вызывает метод <see cref="Guid"/>.ToString("D") для получения строки длиной 36 символов (без скобок, но с разделителями "-").
    /// Затем вызывает <see cref="OnFormatStringValue(DBxSqlBuffer, string)"/> для записи строки.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="value">Записываемое значение</param>
    protected virtual void OnFormatGuidValue(DBxSqlBuffer buffer, Guid value)
    {
      OnFormatStringValue(buffer, value.ToString("D"));
    }

    /// <summary>
    /// Форматирование логического значения.
    /// Записывает в <paramref name="buffer"/>.SB текст "NULL".
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    protected virtual void OnFormatNullValue(DBxSqlBuffer buffer)
    {
      buffer.SB.Append("NULL");
    }


    /// <summary>
    /// Возвращает значение по умолчанию для заданного типа данных.
    /// Вызывает <see cref="DBxTools.GetDefaultValue(DBxColumnType)"/>. Переопределяется для SQLite.
    /// </summary>
    /// <param name="columnType">Тип данных</param>
    /// <returns>Значение</returns>
    protected virtual object GetDefaultValue(DBxColumnType columnType)
    {
      return DBxTools.GetDefaultValue(columnType);
    }


    #endregion

    #region Выражения

    /// <summary>
    /// Форматирование выражения.
    /// Обычно этот метод не переопределяется.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="expression">Элемент порядка сортировки. Не может быть null</param>
    /// <param name="formatInfo">Сведения для форматирования имен полей</param>
    internal protected override void FormatExpression(DBxSqlBuffer buffer, DBxExpression expression, DBxFormatExpressionInfo formatInfo)
    {
      if (expression is DBxColumn)
        OnFormatColumn(buffer, (DBxColumn)expression, formatInfo);
      else if (expression is DBxFunction)
        OnFormatFunction(buffer, (DBxFunction)expression, formatInfo);
      else if (expression is DBxConst)
      {
        DBxColumnType columnType = formatInfo.WantedColumnType;
        if (columnType == DBxColumnType.Unknown) // 24.05.2023
          columnType = ((DBxConst)expression).ColumnType;
        FormatValue(buffer, ((DBxConst)expression).Value, columnType);
      }
      else if (expression is DBxAggregateFunction)
        OnFormatAggregateFunction(buffer, (DBxAggregateFunction)expression, formatInfo);
      else
        throw new ArgumentException("Неподдерживаемый тип выражения: " + expression.GetType().ToString());
    }


    /// <summary>
    /// Форматирование части выражения, возвращающего значение столбца.
    /// Если столбец поддерживает значения NULL и установлено свойство <see cref="DBxFormatExpressionInfo.NullAsDefaultValue"/>,
    /// то выполняется форматирование функции COALESCE().
    /// Иначе в запрос добавляется имя поля с помощью <see cref="FormatColumnName(DBxSqlBuffer, string, string)"/>. 
    /// Перед этим выполняется поиск альяса таблицы в списке <see cref="DBxSqlBuffer.ColumnTableAliases"/>.
    /// При необходимости, перед именем столбца выводится альяс таблицы.
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="column">Выражение - имя поля</param>
    /// <param name="formatInfo">Параметры форматирования</param>
    protected virtual void OnFormatColumn(DBxSqlBuffer buffer, DBxColumn column, DBxFormatExpressionInfo formatInfo)
    {
      string tableAlias;
      buffer.ColumnTableAliases.TryGetValue(column.ColumnName, out tableAlias);

      string actualName = column.ColumnName;
      int lastDotPos = column.ColumnName.LastIndexOf('.');
      if (lastDotPos >= 0)
      {
        actualName = actualName.Substring(lastDotPos + 1);
        if (String.IsNullOrEmpty(tableAlias))
          throw new InvalidOperationException("Для ссылочного столбца \"" + column.ColumnName + "\" не найден альяс таблицы");
      }

      bool useCoalesce = false;
      DBxColumnType wantedType = formatInfo.WantedColumnType;
      if (formatInfo.NullAsDefaultValue)
      {
        DBxColumnStruct colStr;
        buffer.ColumnStructs.TryGetValue(column.ColumnName, out colStr);
        if (colStr != null)
        {
          useCoalesce = colStr.Nullable;
          wantedType = colStr.ColumnType;
        }
        else
          useCoalesce = true;
      }

      if (useCoalesce)
      {
        if (wantedType == DBxColumnType.Unknown)
          throw new InvalidOperationException("Для столбца \"" + column.ColumnName + "\" требуется обработка значения NULL. Не найдено описание структуры столбца и не передан требуемый тип данных");

        DBxFunction f2 = new DBxFunction(DBxFunctionKind.Coalesce, column, new DBxConst(GetDefaultValue(wantedType), wantedType));
        FormatExpression(buffer, f2, new DBxFormatExpressionInfo()); // рекурсивный вызов форматировщика, но уже без флага 
      }
      else
      {
        if (String.IsNullOrEmpty(tableAlias))
          FormatColumnName(buffer, actualName);
        else
          FormatColumnName(buffer, tableAlias, actualName);
      }
    }

    /// <summary>
    /// Форматирование выражения-функции или математической операции.
    /// Для форматирования аргументов используется рекурсивный вызов <see cref="DBxSqlBuffer.FormatExpression(DBxExpression, DBxFormatExpressionInfo)"/>.
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="function">Выражение - функция</param>
    /// <param name="formatInfo">Параметры форматирования</param>
    protected virtual void OnFormatFunction(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo)
    {
      switch (function.Function)
      {
        #region Арифметические операции

        case DBxFunctionKind.Add:
          if (formatInfo.WantedColumnType == DBxColumnType.Unknown)
            formatInfo.WantedColumnType = DBxColumnType.Int;

          formatInfo.NoParentheses = true;
          buffer.FormatExpression(function.Arguments[0], formatInfo);
          buffer.SB.Append("+");
          buffer.FormatExpression(function.Arguments[1], formatInfo);
          break;

        case DBxFunctionKind.Substract:
          if (formatInfo.WantedColumnType == DBxColumnType.Unknown)
            formatInfo.WantedColumnType = DBxColumnType.Int;

          formatInfo.NoParentheses = true;
          buffer.FormatExpression(function.Arguments[0], formatInfo);
          buffer.SB.Append("-");
          formatInfo.NoParentheses = false;
          buffer.FormatExpression(function.Arguments[1], formatInfo);
          break;

        case DBxFunctionKind.Multiply:
          formatInfo.NoParentheses = false;
          if (formatInfo.WantedColumnType == DBxColumnType.Unknown)
            formatInfo.WantedColumnType = DBxColumnType.Int;
          buffer.FormatExpression(function.Arguments[0], formatInfo);
          buffer.SB.Append("*");
          buffer.FormatExpression(function.Arguments[1], formatInfo);
          break;

        case DBxFunctionKind.Divide:
          if (formatInfo.WantedColumnType == DBxColumnType.Unknown)
            formatInfo.WantedColumnType = DBxColumnType.Int;
          formatInfo.NoParentheses = false;
          buffer.FormatExpression(function.Arguments[0], formatInfo);
          buffer.SB.Append("/");
          buffer.FormatExpression(function.Arguments[1], formatInfo);
          break;

        case DBxFunctionKind.Neg:
          if (formatInfo.WantedColumnType == DBxColumnType.Unknown)
            formatInfo.WantedColumnType = DBxColumnType.Int;
          formatInfo.NoParentheses = false;
          buffer.SB.Append("-");
          buffer.FormatExpression(function.Arguments[0], formatInfo);
          break;

        #endregion

        #region Операции сравнения

        case DBxFunctionKind.Equal:
          DoFormatOperatorCompare(buffer, function, formatInfo, "=");
          break;
        case DBxFunctionKind.LessThan:
          DoFormatOperatorCompare(buffer, function, formatInfo, "<");
          break;
        case DBxFunctionKind.LessOrEqualThan:
          DoFormatOperatorCompare(buffer, function, formatInfo, "<=");
          break;
        case DBxFunctionKind.GreaterThan:
          DoFormatOperatorCompare(buffer, function, formatInfo, ">");
          break;
        case DBxFunctionKind.GreaterOrEqualThan:
          DoFormatOperatorCompare(buffer, function, formatInfo, ">=");
          break;
        case DBxFunctionKind.NotEqual:
          DoFormatOperatorCompare(buffer, function, formatInfo, "<>");
          break;

        #endregion

        #region Функции

        case DBxFunctionKind.Abs:
          if (formatInfo.WantedColumnType == DBxColumnType.Unknown)
            formatInfo.WantedColumnType = DBxColumnType.Int;
          DoFormatFunction(buffer, function, formatInfo);
          break;

        case DBxFunctionKind.Coalesce:
          // Определяем тип данных из константы
          for (int i = function.Arguments.Length - 1; i >= 1; i--)
          {
            DBxConst constExpr = function.Arguments[i].GetConst();
            if (constExpr != null)
            {
              formatInfo.WantedColumnType = constExpr.ColumnType; // переопределяем переданный тип
              break;
            }
          }
          DoFormatFunction(buffer, function, formatInfo);
          break;

        case DBxFunctionKind.Length:
        case DBxFunctionKind.Lower:
        case DBxFunctionKind.Upper:
          formatInfo.WantedColumnType = DBxColumnType.String;
          DoFormatFunction(buffer, function, formatInfo);
          break;

        case DBxFunctionKind.Substring:
          // Первый аргумент - строка, второй и третий - числа
          buffer.SB.Append(GetFunctionName(function.Function));
          buffer.SB.Append('(');
          formatInfo.NoParentheses = true;
          formatInfo.WantedColumnType = DBxColumnType.String;
          buffer.FormatExpression(function.Arguments[0], formatInfo);
          buffer.SB.Append(',');
          formatInfo.WantedColumnType = DBxColumnType.Int;
          buffer.FormatExpression(function.Arguments[1], formatInfo);
          buffer.SB.Append(',');
          formatInfo.WantedColumnType = DBxColumnType.Int;
          buffer.FormatExpression(function.Arguments[2], formatInfo);
          buffer.SB.Append(')');
          break;

        #endregion

        default:
          DoFormatFunction(buffer, function, formatInfo);
          break;
      }
    }

    private void DoFormatFunction(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo)
    {
      formatInfo.NoParentheses = true;

      buffer.SB.Append(GetFunctionName(function.Function));
      buffer.SB.Append('(');
      for (int i = 0; i < function.Arguments.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(',');
        buffer.FormatExpression(function.Arguments[i], formatInfo);
      }
      buffer.SB.Append(')');
    }


    /// <summary>
    /// Форматирование функции IIF как оператора CASE (для PostgeSQL)
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="function">Выражение - функция</param>
    /// <param name="formatInfo">Параметры форматирования</param>
    /// <param name="useNEoperator">Если true, то после выражения-условия нужно поставить "меньше-больше 0".
    /// Требуется для MS SQL Server 2005</param>
    protected void FormatIIfFunctionAsCaseOperator(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo, bool useNEoperator)
    {
      formatInfo.NoParentheses = false;
      buffer.SB.Append("CASE WHEN ");
      if (useNEoperator)
        buffer.SB.Append("(");
      FormatExpression(buffer, function.Arguments[0], formatInfo);
      if (useNEoperator)
      {
        buffer.SB.Append(")<>");
        FormatValue(buffer, false, DBxColumnType.Boolean);
      }
      buffer.SB.Append(" THEN ");
      FormatExpression(buffer, function.Arguments[1], formatInfo);
      buffer.SB.Append(" ELSE ");
      FormatExpression(buffer, function.Arguments[2], formatInfo);
      buffer.SB.Append(" END");
    }


    /// <summary>
    /// Форматирование функции <see cref="DBxFunctionKind.Coalesce"/> как функции ISNULL() с двумя аргументами.
    /// Если у функции <paramref name="function"/> задано три или более аргумента, то добавляется несколько вызовов ISNULL.
    /// Используется в DataView и MS SQL Server 2005
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="function"></param>
    /// <param name="formatInfo"></param>
    protected void FormatFunctionCoalesceAsIsNullWith2args(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo)
    {
      int currPos = 0;

      DBxFormatExpressionInfo formatInfo2 = new DBxFormatExpressionInfo();
      formatInfo2.WantedColumnType = formatInfo.WantedColumnType;
      formatInfo2.NullAsDefaultValue = false;
      formatInfo2.NoParentheses = true;

      DoFormatFunctionCoalesceAsIsNullWith2args(buffer, function, formatInfo2, currPos);
    }

    private void DoFormatFunctionCoalesceAsIsNullWith2args(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo, int currPos)
    {
      buffer.SB.Append("ISNULL(");
      buffer.FormatExpression(function.Arguments[currPos], formatInfo);
      buffer.SB.Append(",");
      if (currPos == function.Arguments.Length - 2)
        buffer.FormatExpression(function.Arguments[currPos + 1], formatInfo);
      else
        DoFormatFunctionCoalesceAsIsNullWith2args(buffer, function, formatInfo, currPos + 1); // рекурсивный вызов
      buffer.SB.Append(")");
    }


    private void DoFormatOperatorCompare(DBxSqlBuffer buffer, DBxFunction function, DBxFormatExpressionInfo formatInfo, string sign)
    {
      formatInfo.NoParentheses = false;
      if (formatInfo.WantedColumnType == DBxColumnType.Unknown)
        formatInfo.WantedColumnType = DBxColumnType.Int;

      buffer.FormatExpression(function.Arguments[0], formatInfo);
      buffer.SB.Append(sign);
      buffer.FormatExpression(function.Arguments[1], formatInfo);
    }


    /// <summary>
    /// Возвращает true, если выражение <paramref name="expression"/> требуется заключать в круглые скобки для правильного порядка вычислений.
    /// Возвращает true для выражений-математических операций.
    /// </summary>
    /// <param name="expression">Выражение, которое, может быть, нужно заключать в скобки</param>
    /// <returns>true, если скобки требуются</returns>
    internal protected override bool AreParenthesesRequired(DBxExpression expression)
    {
      DBxFunction f = expression as DBxFunction;
      if (f != null)
      {
        switch (f.Function)
        {
          case DBxFunctionKind.Add:
          case DBxFunctionKind.Substract:
          case DBxFunctionKind.Multiply:
          case DBxFunctionKind.Divide:
          case DBxFunctionKind.Neg:

          case DBxFunctionKind.Equal:
          case DBxFunctionKind.LessThan:
          case DBxFunctionKind.LessOrEqualThan:
          case DBxFunctionKind.GreaterThan:
          case DBxFunctionKind.GreaterOrEqualThan:
          case DBxFunctionKind.NotEqual:
            return true;
          default:
            return false;
        }
      }
      else
        return false;
    }

    /// <summary>
    /// Возвращает имя функции.
    /// Для некоторых провайдеров функции называются нестандартным образом.
    /// Например, для MS SQL Server и DataView, функция COALSECE() называется ISNULL(), хотя делает то же самое.
    /// Если СУБД реализует функцию с другими аргументами, то требуется переопределение метода <see cref="OnFormatFunction(DBxSqlBuffer, DBxFunction, DBxFormatExpressionInfo)"/>.
    /// </summary>
    /// <param name="function">Функция</param>
    protected virtual string GetFunctionName(DBxFunctionKind function)
    {
      switch (function)
      {
        case DBxFunctionKind.Abs: return "ABS";
        case DBxFunctionKind.Coalesce: return "COALESCE";
        case DBxFunctionKind.IIf: return "IIF";
        case DBxFunctionKind.Length: return "LEN";
        case DBxFunctionKind.Lower: return "LOWER";
        case DBxFunctionKind.Upper: return "UPPER";
        case DBxFunctionKind.Substring: return "SUBSTRING";
        default:
          throw new ArgumentException("Неизвестная функция " + function.ToString(), "function");
      }
    }

    /// <summary>
    /// Форматирование выражения-функции или математической операции.
    /// Для форматирования аргументов используется рекурсивный вызов <see cref="DBxSqlBuffer.FormatExpression(DBxExpression, DBxFormatExpressionInfo)"/>.
    /// </summary>
    /// <param name="buffer">Буфер для создания SQL-запроса</param>
    /// <param name="function">Выражение - функция</param>
    /// <param name="formatInfo">Параметры форматирования</param>
    protected virtual void OnFormatAggregateFunction(DBxSqlBuffer buffer, DBxAggregateFunction function, DBxFormatExpressionInfo formatInfo)
    {
      buffer.SB.Append(GetFunctionName(function.Function));
      buffer.SB.Append('(');
      if (function.Argument == null)
        buffer.SB.Append('*');
      else
        buffer.FormatExpression(function.Argument, new DBxFormatExpressionInfo());
      buffer.SB.Append(')');
    }

    private string GetFunctionName(DBxAggregateFunctionKind kind)
    {
      // Можно было бы вернуть Kind.ToString().ToUpperInvariant(), но это небезопасно с точки зрения иньекции кода
      switch (kind)
      {
        case DBxAggregateFunctionKind.Sum:
        case DBxAggregateFunctionKind.Count:
        case DBxAggregateFunctionKind.Min:
        case DBxAggregateFunctionKind.Max:
        case DBxAggregateFunctionKind.Avg:
          return kind.ToString().ToUpperInvariant();
        default:
          throw new ArgumentException("Неизвестная агрегатная функция " + kind.ToString());
      }
    }

    #endregion

    #region Фильтры

    /// <summary>
    /// Форматирование фильтра.
    /// Вызывает виртуальный метод для фильтра нужного типа.
    /// Обычно нет необходимости переопределять этот метод.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    internal protected override void FormatFilter(DBxSqlBuffer buffer, DBxFilter filter)
    {
      if (filter is CompareFilter)
        OnFormatCompareFilter(buffer, (CompareFilter)filter);
      else if (filter is IdsFilter)
        OnFormatIdsFilter(buffer, (IdsFilter)filter);
      else if (filter is ValuesFilter)
        OnFormatValuesFilter(buffer, (ValuesFilter)filter);
      else if (filter is StringValueFilter)
        OnFormatStringValueFilter(buffer, (StringValueFilter)filter);
      else if (filter is AndFilter)
        OnFormatAndFilter(buffer, (AndFilter)filter);
      else if (filter is OrFilter)
        OnFormatOrFilter(buffer, (OrFilter)filter);
      else if (filter is NotFilter)
        OnFormatNotFilter(buffer, (NotFilter)filter);
      else if (filter is NumRangeFilter)
        OnFormatNumRangeFilter(buffer, (NumRangeFilter)filter);
      else if (filter is NumRangeInclusionFilter)
        OnFormatNumRangeInclusionFilter(buffer, (NumRangeInclusionFilter)filter);
      else if (filter is NumRangeCrossFilter)
        OnFormatNumRangeCrossFilter(buffer, (NumRangeCrossFilter)filter);
      else if (filter is DateRangeFilter)
        OnFormatDateRangeFilter(buffer, (DateRangeFilter)filter);
      else if (filter is DateRangeInclusionFilter)
        OnFormatDateRangeInclusionFilter(buffer, (DateRangeInclusionFilter)filter);
      else if (filter is DateRangeCrossFilter)
        OnFormatDateRangeCrossFilter(buffer, (DateRangeCrossFilter)filter);
      else if (filter is StartsWithFilter)
        OnFormatStartsWithFilter(buffer, (StartsWithFilter)filter);
      else if (filter is SubstringFilter)
        OnFormatSubstringFilter(buffer, (SubstringFilter)filter);
      else if (filter is DummyFilter)
        OnFormatDummyFilter(buffer, (DummyFilter)filter);
      else
        throw new ArgumentException("Неподдерживаемый тип фильтра: " + filter.GetType().ToString());
    }

    /// <summary>
    /// Получить знак для условия <see cref="CompareKind"/>
    /// </summary>
    /// <param name="kind">Тип сравнения</param>
    /// <returns>Знак операции сравнения</returns>
    protected static string GetSignStr(CompareKind kind)
    {
      switch (kind)
      {
        case CompareKind.Equal: return "=";
        case CompareKind.LessThan: return "<";
        case CompareKind.LessOrEqualThan: return "<=";
        case CompareKind.GreaterThan: return ">";
        case CompareKind.GreaterOrEqualThan: return ">=";
        case CompareKind.NotEqual: return "<>";
        default: throw new ArgumentException("Неизвестный Kind: " + kind.ToString());
      }
    }

    #region Фильтры "Выражение IN (Список значений)"

    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected void OnFormatIdsFilter(DBxSqlBuffer buffer, IdsFilter filter)
    {
      if (filter.Ids.Count == 1)
      {
        Int32 singleId = filter.Ids.SingleId;
        CompareFilter Filter2 = new CompareFilter(filter.Expression, new DBxConst(singleId), CompareKind.Equal, singleId == 0, DBxColumnType.Int);
        buffer.FormatFilter(Filter2);
        return;
      }

      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = true;
      formatInfo.WantedColumnType = DBxColumnType.Int;
      buffer.FormatExpression(filter.Expression, formatInfo);
      buffer.SB.Append(" IN (");
      bool first = true;
      foreach (Int32 id in filter.Ids)
      {
        if (first)
          first = false;
        else
          buffer.SB.Append(", ");

        buffer.SB.Append(id.ToString());
      }
      buffer.SB.Append(')');
    }


    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatValuesFilter(DBxSqlBuffer buffer, ValuesFilter filter)
    {
      // Есть ли в списке значений значение по умолчанию
      if (filter.Values.Length == 1)
      {
        // Как обычный ValueFilter
        CompareFilter filter2 = new CompareFilter(filter.Expression, new DBxConst(filter.Values.GetValue(0)), CompareKind.Equal, filter.NullAsDefaultValue, filter.ColumnTypeInternal);
        buffer.FormatFilter(filter2);
        return;
      }

      // Сложный фильтр использует IN
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = filter.NullAsDefaultValue;
      formatInfo.WantedColumnType = filter.ColumnTypeInternal;
      FormatExpression(buffer, filter.Expression, formatInfo);

      buffer.SB.Append(" IN (");
      for (int i = 0; i < filter.Values.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(", ");
        buffer.FormatValue(filter.Values.GetValue(i), formatInfo.WantedColumnType);
      }
      buffer.SB.Append(')');
    }


    #endregion

    #region Строковые фильтры

    /// <summary>
    /// Форматирование фильтра.
    /// Непереопределенная реализация заменяет <see cref="StringValueFilter"/> на <see cref="CompareFilter"/> с вызовом функции UPPER()
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatStringValueFilter(DBxSqlBuffer buffer, StringValueFilter filter)
    {
      bool ignoreCase = filter.IgnoreCase && StringIsCaseSensitive(filter.Value);

      DBxExpression expr1, expr2;
      if (ignoreCase)
      {
        expr1 = new DBxFunction(DBxFunctionKind.Upper, filter.Expression);
        expr2 = new DBxConst(filter.Value.ToUpperInvariant(), DBxColumnType.String);
      }
      else
      {
        expr1 = filter.Expression;
        expr2 = new DBxConst(filter.Value, DBxColumnType.String);
      }

      bool nullAsDefaultValue = filter.Value.Length == 0; // 02.06.2023
      CompareFilter filter2 = new CompareFilter(expr1, expr2, CompareKind.Equal, nullAsDefaultValue);
      buffer.FormatFilter(filter2);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatStartsWithFilter(DBxSqlBuffer buffer, StartsWithFilter filter)
    {
      bool ignoreCase = filter.IgnoreCase && StringIsCaseSensitive(filter.Value);

      DBxExpression expr1;
      if (ignoreCase)
        expr1 = new DBxFunction(DBxFunctionKind.Upper, filter.Expression);
      else
        expr1 = filter.Expression;

      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = false; //
      formatInfo.WantedColumnType = DBxColumnType.String;
      formatInfo.NoParentheses = false;
      buffer.FormatExpression(expr1, formatInfo);
      buffer.SB.Append(" LIKE '");

      string v = filter.Value;
      if (ignoreCase)
        v = v.ToUpperInvariant();
      MakeEscapedChars(buffer, v, new char[] { '%', '_', '[', '\'' }, "[", "]");
      buffer.SB.Append("%\'");
    }

    /// <summary>
    /// Окружение специальных символов в строке для фильтров, основанных на предложении LIKE
    /// </summary>
    /// <param name="buffer">Буфер для записи строки</param>
    /// <param name="value">Строка, возможно содержащая символы</param>
    /// <param name="chars">Символы, которые требуется окружить</param>
    /// <param name="prefix">Окружение слева</param>
    /// <param name="suffix">Окружение справа</param>
    protected static void MakeEscapedChars(DBxSqlBuffer buffer, string value, char[] chars, string prefix, string suffix)
    {
      if (String.IsNullOrEmpty(value))
        return;
      if (value.IndexOfAny(chars) < 0)
      {
        buffer.SB.Append(value);
        return;
      }

      for (int i = 0; i < value.Length; i++)
      {
        char c = value[i];
        if (Array.IndexOf<char>(chars, c) >= 0)
        {
          // Спецсимвол
          buffer.SB.Append(prefix);
          buffer.SB.Append(c);
          buffer.SB.Append(suffix);
        }
        else
          // Обычный символ
          buffer.SB.Append(c);
      }
    }


    /// <summary>
    /// Форматирование фильтра.
    /// Непереопределенная реализация заменяет <see cref="SubstringFilter"/> на <see cref="CompareFilter"/> с вызовом функций UPPER() и SUBSTRING()
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatSubstringFilter(DBxSqlBuffer buffer, SubstringFilter filter)
    {
      bool ignoreCase = filter.IgnoreCase && StringIsCaseSensitive(filter.Value);

      // 24.06.2019.
      // То же, что и классе DataViewDBxSqlFormatter, но с переводом к верхнему регистру
      DBxExpression expr1, expr2;
      if (ignoreCase)
      {
        expr1 = new DBxFunction(DBxFunctionKind.Upper, new DBxFunction(DBxFunctionKind.Substring,
          filter.Expression,
          new DBxConst(filter.StartIndex + 1),
          new DBxConst(filter.Value.Length)));
        expr2 = new DBxConst(filter.Value.ToUpperInvariant(), DBxColumnType.String);
      }
      else
      {
        expr1 = new DBxFunction(DBxFunctionKind.Substring,
          filter.Expression,
          new DBxConst(filter.StartIndex + 1),
          new DBxConst(filter.Value.Length));
        expr2 = new DBxConst(filter.Value, DBxColumnType.String);
      }

      CompareFilter filter2 = new CompareFilter(expr1, expr2, CompareKind.Equal, true);
      buffer.FormatFilter(filter2);
    }

    /// <summary>
    /// Возвращает true, если переданная строка содержит буквенные символы.
    /// В этом случае имеет значение режим сравнения для строковых фильтров.
    /// </summary>
    /// <param name="s">Проверяемое строковое выражение</param>
    /// <returns>Чувствительность к регистру</returns>
    protected static bool StringIsCaseSensitive(string s)
    {
      if (String.IsNullOrEmpty(s))
        return false;
      return !String.Equals(s.ToUpperInvariant(), s.ToLowerInvariant(), StringComparison.Ordinal);
    }

    #endregion

    #region Фильтры по диапазонам

    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatNumRangeFilter(DBxSqlBuffer buffer, NumRangeFilter filter)
    {
      if (filter.MinValue.HasValue)
      {
        if (filter.MaxValue.HasValue)
        {
          if (filter.MaxValue.Value == filter.MinValue.Value)
          {
            CompareFilter filter3 = new CompareFilter(filter.Expression, new DBxConst(filter.MinValue.Value));
            buffer.FormatFilter(filter3);
          }
          else
          {
            if (BetweenInstructionSupported)
            {
              DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
              formatInfo.NoParentheses = false;
              formatInfo.NullAsDefaultValue = true;
              formatInfo.WantedColumnType = DBxColumnType.Decimal;
              buffer.FormatExpression(filter.Expression, formatInfo);
              buffer.SB.Append(" BETWEEN ");
              buffer.FormatValue(filter.MinValue.Value, DBxColumnType.Unknown);
              buffer.SB.Append(" AND ");
              buffer.FormatValue(filter.MaxValue.Value, DBxColumnType.Unknown);
            }
            else
            {
              CompareFilter filter1 = new CompareFilter(filter.Expression, new DBxConst(filter.MinValue.Value), CompareKind.GreaterOrEqualThan, true);
              CompareFilter filter2 = new CompareFilter(filter.Expression, new DBxConst(filter.MaxValue.Value), CompareKind.LessOrEqualThan, true);
              AndFilter filter3 = new AndFilter(filter1, filter2); // 25.12.2020
              buffer.FormatFilter(filter3);
            }
          }
        }
        else
        {
          CompareFilter filter1 = new CompareFilter(filter.Expression, new DBxConst(filter.MinValue.Value), CompareKind.GreaterOrEqualThan, true);
          buffer.FormatFilter(filter1);
        }
      }
      else
      {
        if (filter.MaxValue.HasValue)
        {
          CompareFilter filter2 = new CompareFilter(filter.Expression, new DBxConst(filter.MaxValue.Value), CompareKind.LessOrEqualThan, true);
          buffer.FormatFilter(filter2);
        }
      }
    }

    /// <summary>
    /// Возвращает true, если реализация SQL поддерживает инструкцию BETWEEN.
    /// Непереопределенная реализация возвращает true
    /// </summary>
    protected virtual bool BetweenInstructionSupported { get { return true; } }


    /// <summary>
    /// Форматирование фильтра.
    /// Непереопределенная реализация заменяет фильтр на два <see cref="CompareFilter"/>, объединенных <see cref="AndFilter"/> и функции COALESCE для учета значения NULL
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatNumRangeInclusionFilter(DBxSqlBuffer buffer, NumRangeInclusionFilter filter)
    {
      DBxFunction expr1 = new DBxFunction(DBxFunctionKind.Coalesce, filter.Expression1, new DBxConst(filter.Value));
      CompareFilter filter1 = new CompareFilter(expr1, new DBxConst(filter.Value), CompareKind.LessOrEqualThan, false);

      DBxFunction expr2 = new DBxFunction(DBxFunctionKind.Coalesce, filter.Expression2, new DBxConst(filter.Value));
      CompareFilter filter2 = new CompareFilter(expr2, new DBxConst(filter.Value), CompareKind.GreaterOrEqualThan, false);

      DBxFilter filter3 = new AndFilter(filter1, filter2);
      buffer.FormatFilter(filter3);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Непереопределенная реализация использует один или два <see cref="CompareFilter"/>.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatNumRangeCrossFilter(DBxSqlBuffer buffer, NumRangeCrossFilter filter)
    {
      List<DBxFilter> filters = new List<DBxFilter>();

      if (filter.MinValue.HasValue)
      {
        // Не путать. Первое поле (начальное значение) сравнивается с конечным значением фильтра и наоборот.
        DBxFunction expr = new DBxFunction(DBxFunctionKind.Coalesce, filter.Expression2, new DBxConst(filter.MinValue.Value));
        CompareFilter filter2 = new CompareFilter(expr, new DBxConst(filter.MinValue.Value), CompareKind.GreaterOrEqualThan, false);
        filters.Add(filter2);
      }
      if (filter.MaxValue.HasValue)
      {
        DBxFunction expr = new DBxFunction(DBxFunctionKind.Coalesce, filter.Expression1, new DBxConst(filter.MaxValue.Value));
        CompareFilter filter2 = new CompareFilter(expr, new DBxConst(filter.MaxValue.Value), CompareKind.LessOrEqualThan, false);
        filters.Add(filter2);
      }
      DBxFilter resFilter = AndFilter.FromList(filters);
      if (resFilter == null)
        resFilter = DummyFilter.AlwaysTrue;

      buffer.FormatFilter(resFilter);
    }

    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatDateRangeFilter(DBxSqlBuffer buffer, DateRangeFilter filter)
    {
      // Так как поле может содержать компонент времени, нельзя использовать конструкцию
      // "Значение <= #КонечнаяДата#". Вместо этого надо использовать конструкцию
      // "Значение < #КонечнаяДата+1#"
      // Поэтому же нельзя использовать упрощенный фильтр на равенство при MinValue=MaxValue

      if (filter.MinValue.HasValue)
      {
        if (filter.MaxValue.HasValue)
        {
          CompareFilter filter1 = new CompareFilter(filter.Expression, new DBxConst(filter.MinValue.Value), CompareKind.GreaterOrEqualThan, false);
          CompareFilter filter2 = new CompareFilter(filter.Expression, new DBxConst(filter.MaxValue.Value.AddDays(1)), CompareKind.LessThan, false);
          AndFilter filter3 = new AndFilter(filter1, filter2);
          buffer.FormatFilter(filter3);
        }
        else
        {
          CompareFilter filter1 = new CompareFilter(filter.Expression, new DBxConst(filter.MinValue.Value), CompareKind.GreaterOrEqualThan, false);
          buffer.FormatFilter(filter1);
        }
      }
      else
      {
        if (filter.MaxValue.HasValue)
        {
          CompareFilter filter2 = new CompareFilter(filter.Expression, new DBxConst(filter.MaxValue.Value.AddDays(1)), CompareKind.LessThan, false);
          buffer.FormatFilter(filter2);
        }
        else
        {
          // Вырожденный фильтр
          buffer.FormatFilter(DummyFilter.AlwaysTrue);
        }
      }
    }

    /// <summary>
    /// Форматирование фильтра.
    /// Непереопределенная реализация заменяет фильтр на два <see cref="CompareFilter"/>, объединенных <see cref="AndFilter"/> и функции COALESCE для учета значения NULL
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatDateRangeInclusionFilter(DBxSqlBuffer buffer, DateRangeInclusionFilter filter)
    {
      DBxFunction expr1 = new DBxFunction(DBxFunctionKind.Coalesce, filter.Expression1, new DBxConst(filter.Value));
      CompareFilter filter1 = new CompareFilter(expr1, new DBxConst(filter.Value), CompareKind.LessOrEqualThan, false);

      DBxFunction expr2 = new DBxFunction(DBxFunctionKind.Coalesce, filter.Expression2, new DBxConst(filter.Value));
      CompareFilter filter2 = new CompareFilter(expr2, new DBxConst(filter.Value), CompareKind.GreaterOrEqualThan, false);

      DBxFilter filter3 = new AndFilter(filter1, filter2);
      buffer.FormatFilter(filter3);
    }


    /// <summary>
    /// Форматирование фильтра.
    /// Непереопределенная реализация использует один или два <see cref="CompareFilter"/>.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatDateRangeCrossFilter(DBxSqlBuffer buffer, DateRangeCrossFilter filter)
    {
      // 30.06.2019
      // Исправлено, чтобы больше не использовать DateTime.MinValue и MaxValue.
      // Вместо этого используем сам диапазон.
      // Нужно, на случай, если провайдер базы данных (SQLite) не поддерживает весь диапазон дат DateTime

      List<DBxFilter> filters = new List<DBxFilter>();

      if (filter.FirstDate.HasValue)
      {
        // Не путать. Первое поле (начальная дата) сравнивается с конечной датой фильтра и наоборот.
        DBxFunction expr = new DBxFunction(DBxFunctionKind.Coalesce, filter.Expression2, new DBxConst(filter.FirstDate.Value));
        CompareFilter filter2 = new CompareFilter(expr, new DBxConst(filter.FirstDate.Value), CompareKind.GreaterOrEqualThan, false);
        filters.Add(filter2);
      }
      if (filter.LastDate.HasValue)
      {
        DBxFunction expr = new DBxFunction(DBxFunctionKind.Coalesce, filter.Expression1, new DBxConst(filter.LastDate.Value));
        CompareFilter filter2 = new CompareFilter(expr, new DBxConst(filter.LastDate.Value), CompareKind.LessOrEqualThan, false);
        filters.Add(filter2);
      }
      DBxFilter resFilter = AndFilter.FromList(filters);
      if (resFilter == null)
        resFilter = DummyFilter.AlwaysTrue;

      buffer.FormatFilter(resFilter);
    }

    #endregion

    #region Логические фильтры AND/OR/NOT

    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatAndFilter(DBxSqlBuffer buffer, AndFilter filter)
    {
      for (int i = 0; i < filter.Filters.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(" AND ");

        bool parentheses = FilterNeedsParentheses(filter.Filters[i]);

        if (parentheses)
          buffer.SB.Append('(');
        buffer.FormatFilter(filter.Filters[i]);
        if (parentheses)
          buffer.SB.Append(')');
      }
    }

    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatOrFilter(DBxSqlBuffer buffer, OrFilter filter)
    {
      for (int i = 0; i < filter.Filters.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(" OR ");
        bool parentheses = FilterNeedsParentheses(filter.Filters[i]);

        if (parentheses)
          buffer.SB.Append('(');

        buffer.FormatFilter(filter.Filters[i]);

        if (parentheses)
          buffer.SB.Append(')');
      }
    }

    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatNotFilter(DBxSqlBuffer buffer, NotFilter filter)
    {
      buffer.SB.Append("NOT ");
      bool parentheses = FilterNeedsParentheses(filter.BaseFilter);
      if (parentheses)
        buffer.SB.Append("(");

      buffer.FormatFilter(filter.BaseFilter);

      if (parentheses)
        buffer.SB.Append(')');
    }

    /// <summary>
    /// Метод возвращает true, если вокруг дочернего фильтра <paramref name="filter"/> должны быть скобки.
    /// Для большинства фильтров возвращается true
    /// </summary>
    /// <param name="filter">Фильтр, для которого определяется необходимость окружить его скобками</param>
    /// <returns>Необходимость в скобках</returns>
    protected virtual bool FilterNeedsParentheses(DBxFilter filter)
    {
      // TODO:
      //if (filter is ValueFilter)
      //{
      //  if (((ValueFilter)filter).Value == null)
      //    return true;
      //  else
      //    return false;
      //}

      return true;
    }

    #endregion

    #region Прочие фильтры

    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatCompareFilter(DBxSqlBuffer buffer, CompareFilter filter)
    {
      DBxConst cnst2 = filter.Expression2.GetConst();
      if (cnst2 != null)
      {
        if (cnst2.Value == null)
        {
          switch (filter.Kind)
          {
            case CompareKind.Equal:
            case CompareKind.NotEqual:
              OnFormatNullNotNullCompareFilter(buffer, filter.Expression1, cnst2.ColumnType, filter.Kind);
              return;
            default:
              throw new ArgumentException("В фильтре задано сравнение значения с NULL в режиме " + filter.Kind.ToString() + ". Допускаются только сравнения на равенство и неравенство");
          }
        }

      }
      else
      {
        DBxConst cnst1 = filter.Expression1.GetConst();
        if (cnst1 != null)
        {
          if (cnst1.Value == null)
          {
            switch (filter.Kind)
            {
              case CompareKind.Equal:
              case CompareKind.NotEqual:
                OnFormatNullNotNullCompareFilter(buffer, filter.Expression2, cnst1.ColumnType, filter.Kind);
                return;
              default:
                throw new ArgumentException("В фильтре задано сравнение значения с NULL в режиме " + filter.Kind.ToString() + ". Допускаются только сравнения на равенство и неравенство");
            }
          }

        }
      }

      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.NullAsDefaultValue = filter.NullAsDefaultValue;
      formatInfo.WantedColumnType = filter.ColumnTypeInternal; // 24.05.2023

      buffer.FormatExpression(filter.Expression1, formatInfo);
      buffer.SB.Append(GetSignStr(filter.Kind));
      buffer.FormatExpression(filter.Expression2, formatInfo);
    }

    /// <summary>
    /// Запись фильтра <see cref="CompareFilter"/> в режиме сравнения значения с NULL.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="expression">Выражение, которое надо сравнить с NULL (левая часть условия)</param>
    /// <param name="columnType">Тип данных</param>
    /// <param name="kind">Режим сравнения: Equal или NotEqual</param>
    protected virtual void OnFormatNullNotNullCompareFilter(DBxSqlBuffer buffer, DBxExpression expression, DBxColumnType columnType, CompareKind kind)
    {
      DBxFormatExpressionInfo formatInfo = new DBxFormatExpressionInfo();
      formatInfo.WantedColumnType = columnType;
      formatInfo.NoParentheses = false;
      formatInfo.NullAsDefaultValue = false;
      buffer.FormatExpression(expression, formatInfo);
      switch (kind)
      {
        case CompareKind.Equal:
          buffer.SB.Append(" IS NULL");
          break;
        case CompareKind.NotEqual:
          buffer.SB.Append(" IS NOT NULL");
          break;
        default:
          throw new ArgumentException("Недопустимый kind=" + kind.ToString(), "kind");
      }
    }

#if XXX
    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="Buffer">Буфер для записи</param>
    /// <param name="Filter">Фильтр</param>
    protected override void OnFormatValueFilter(DBxSqlBuffer Buffer, ValueFilter Filter)
    {
      // Для просто значения null используем функцию IsNull()
      if (Filter.Value == null || Filter.Value is DBNull)
      {
        if (Filter.Kind != ValueFilterKind.Equal)
          throw new InvalidOperationException("Значение NULL в фильтре сравнения допускается только для сравнения на равенство (поле \"" + Filter.ColumnName + "\")");

        if (Filter.DataType == null)
          throw new InvalidOperationException("Для сравнения с NULL требуется, чтобы был задан тип значения в свойстве ValueFilter.DataType (поле \"" + Filter.ColumnName + "\")");

        Buffer.SB.Append(IsNullFunctionName);
        Buffer.SB.Append("(");
        Buffer.FormatColumnName(Filter.ColumnName);
        Buffer.SB.Append(", ");
        Buffer.FormatValue(DataTools.GetEmptyValue(Filter.DataType), DBxColumnType.Unknown);
        Buffer.SB.Append(")=");
        Buffer.FormatValue(DataTools.GetEmptyValue(Filter.DataType), DBxColumnType.Unknown);
        return;
      }

      if (Filter.Kind == ValueFilterKind.Equal)
      {
        // Для значений 0 и false используем ISNULL() в комбинации со сравнением
        if (DataTools.IsEmptyValue(Filter.Value))
        {
          Buffer.SB.Append(IsNullFunctionName);
          Buffer.SB.Append("(");
          Buffer.FormatColumnName(Filter.ColumnName);
          Buffer.SB.Append(',');
          Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
          Buffer.SB.Append(")=");
          Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
          return;
        }
      }

      Buffer.FormatColumnName(Filter.ColumnName);
      Buffer.SB.Append(GetSignStr(Filter.Kind));
      Buffer.FormatValue(Filter.Value, DBxColumnType.Unknown);
    }

#endif


    /// <summary>
    /// Форматирование фильтра.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="filter">Фильтр</param>
    protected virtual void OnFormatDummyFilter(DBxSqlBuffer buffer, DummyFilter filter)
    {
      if (filter.IsTrue)
        buffer.SB.Append("1=1");
      else
        buffer.SB.Append("1=0");
    }

    #endregion

    #endregion

    #region Порядок сортировки

    /// <summary>
    /// Форматирование порядка сортировки.
    /// Вызывает <paramref name="buffer"/>.FormatOrderItem() для каждого элемента и
    /// добавляет суффикс "DESC" по необходимости.
    /// Нет необходимости переопределять этот метод.
    /// </summary>
    /// <param name="buffer">Буфер для записи</param>
    /// <param name="order">Порядок сортировки</param>
    internal protected override void FormatOrder(DBxSqlBuffer buffer, DBxOrder order)
    {
      for (int i = 0; i < order.Parts.Length; i++)
      {
        if (i > 0)
          buffer.SB.Append(", ");
        buffer.FormatExpression(order.Parts[i].Expression, new DBxFormatExpressionInfo());
        if (order.Parts[i].SortOrder == ListSortDirection.Descending)
          buffer.SB.Append(" DESC");
      }
    }

    #endregion

    #region Форматирование для SELECT

    /// <summary>
    /// Способ задания ограничения MaxRecordCount в SELECT.
    /// По умолчанию используется форма SELECT TOP x.
    /// </summary>
    public override DBxSelectMaxRecordCountMode SelectMaxRecordCountMode { get { return DBxSelectMaxRecordCountMode.Top; } }

    /// <summary>
    /// Нужно ли корректировать типы данных в наборе, возвращаемом оператором SELECT
    /// Используется только в SQLite.
    /// </summary>
    public override bool UseTypeCorrectionInSelectResult { get { return false; } }
    // (использовать оператор TYPES перед SELECT - не помогает).

    #endregion

    #region Список параметров запроса

    /// <summary>
    /// Добавляет в <paramref name="buffer"/>.SB место для подстановки параметра, "@P1",
    /// "@P2",... 
    /// </summary>
    /// <param name="buffer">Буфер, в котором создается SQL-запрос</param>
    /// <param name="paramIndex">Индекс параметра (0,1, ...).</param>
    internal protected override void FormatParamPlaceholder(DBxSqlBuffer buffer, int paramIndex)
    {
      buffer.SB.Append("@P");
      buffer.SB.Append(StdConvert.ToString(paramIndex + 1));
    }

    /// <summary>
    /// Подготовка значения, передаваемого в качестве параметра запроса.
    /// Непереопределенный метод возвращает <paramref name="value"/> неизменным.
    /// </summary>
    /// <param name="value">Значение</param>
    /// <param name="columnType">Тип данных</param>
    /// <returns>Скорректированное значение</returns>
    internal protected override object OnPrepareParamValue(object value, DBxColumnType columnType)
    {
      return value;
    }


    #endregion

    #region Прочие параметры SQL запросов

    /// <summary>
    /// Максимальная длина SQL-запроса в символах
    /// </summary>
    public override int MaxSqlLength { get { return Int16.MaxValue; } }

    /// <summary>
    /// Максимальное количество строк данных в запросе INSERT INTO таблица (столбцы) VALUES (значения) [, (значения), ...].
    /// По умолчанию возвращает 1 - многострочная вставка не поддерживается.
    /// </summary>
    public override int MaxInsertIntoValueRowCount { get { return 1; } }

    #endregion
  }

}
