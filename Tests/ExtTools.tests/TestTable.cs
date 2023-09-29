using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Calendar;
using System.Data;
using FreeLibSet.Core;

namespace ExtTools_tests
{
  /// <summary>
  /// Тестовое перечисление для GetEnum()
  /// </summary>
  public enum TestEnum
  {
    Zero = 0, One = 1, Two = 2, Three = 3
  }

  public class TestTable
  {
    #region Таблица данных

    /// <summary>
    /// Константы для строки 1.
    /// Значения в этой строке гарантированно меньше, чем во второй.
    /// </summary>
    public static class Row1
    {
      public const string VString = "ABC";

      public const int VInt32 = 1;

      public const long VInt64 = 2L;

      public const float VSingle = 3.5f;

      public const double VDouble = 4.5;

      public const decimal VDecimal = 5.5m;

      public const bool VBool = true;

      public static readonly DateTime VDateTime = new DateTime(2021, 12, 15);

      public static readonly TimeSpan VTimeSpan = new TimeSpan(1, 2, 3);

      public static readonly Guid VGuid = new Guid("2c0dfea6-8326-44e4-ba55-a994e0bedd10");

      public const TestEnum VEnum = TestEnum.Two;
    }


    /// <summary>
    /// Константы для строки 2.
    /// Значения в этой строке гарантированно больше, чем в первой
    /// </summary>
    public static class Row2
    {
      public const string VString = "DEF";

      public const int VInt32 = 10;

      public const long VInt64 = 20L;

      public const float VSingle = 30f;

      public const double VDouble = 40.0;

      public const decimal VDecimal = 50m;

      public const bool VBool = false;

      public static readonly DateTime VDateTime = new DateTime(2021, 12, 17);

      public static readonly TimeSpan VTimeSpan = new TimeSpan(4, 5, 6);

      public static readonly Guid VGuid = new Guid("a4ae45d5-42cf-41ec-af62-e44155698f48");

      public const TestEnum VEnum = TestEnum.Three;
    }

    /// <summary>
    /// Результаты вычисления среднего по двум строкам Row1 и Row2
    /// </summary>
    public static class AvgRes2
    {
      public const int VInt32 = 6; // (1+10)/2

      public const long VInt64 = 11L; // (2+20)/2

      public const float VSingle = (3.5f + 30f) / 2f;

      public const double VDouble = (4.5 + 40.0) / 2.0;

      public const decimal VDecimal = (5.5m + 50m) / 2m;

      //public static readonly DateTime VDateTime = new DateTime(2021, 12, 15);

      public static readonly TimeSpan VTimeSpan = new TimeSpan((Row1.VTimeSpan.Ticks + Row2.VTimeSpan.Ticks) / 2L); // ((1,2,3)+(4,5,6))
    }

    /// <summary>
    /// Результаты вычисления среднего по трем строкам Row1, Row2 и нулям
    /// </summary>
    public static class AvgRes3
    {
      public const int VInt32 = 4; // (1+10)/3

      public const long VInt64 = 7L; // (2+20)/3

      public const float VSingle = (3.5f + 30f) / 3f;

      public const double VDouble = (4.5 + 40.0) / 3.0;

      public const decimal VDecimal = (5.5m + 50m) / 3m;

      //public static readonly DateTime VDateTime = new DateTime(2021, 12, 15);

      public static readonly TimeSpan VTimeSpan = new TimeSpan((Row1.VTimeSpan.Ticks + Row2.VTimeSpan.Ticks) / 3L); // ((1,2,3)+(4,5,6))
    }


    /// <summary>
    /// Возвращает тестовую таблицу с тремя строками:
    /// [0] - содержит DBNull во всех полях
    /// [1] - содежит значения
    /// [2] - содержит такие же значения
    /// </summary>
    /// <returns></returns>
    public static DataTable CreateTestDataTable()
    {
      DataTable tbl = new DataTable();
      tbl.Columns.Add("FString", typeof(string));
      tbl.Columns.Add("FInt32", typeof(Int32));
      tbl.Columns.Add("FInt64", typeof(Int64));
      tbl.Columns.Add("FSingle", typeof(Single));
      tbl.Columns.Add("FDouble", typeof(Double));
      tbl.Columns.Add("FDecimal", typeof(Decimal));
      tbl.Columns.Add("FBoolean", typeof(Boolean));
      tbl.Columns.Add("FDateTime", typeof(DateTime));
      tbl.Columns.Add("FTimeSpan", typeof(TimeSpan));
      tbl.Columns.Add("FGuid", typeof(Guid));
      tbl.Columns.Add("FEnum", typeof(int));

      tbl.Rows.Add(); // [0]

      DataRow row;
      row = tbl.NewRow();
      row["FString"] = Row1.VString;
      row["FInt32"] = Row1.VInt32;
      row["FInt64"] = Row1.VInt64;
      row["FSingle"] = Row1.VSingle;
      row["FDouble"] = Row1.VDouble;
      row["FDecimal"] = Row1.VDecimal;
      row["FBoolean"] = Row1.VBool;
      row["FDateTime"] = Row1.VDateTime;
      row["FTimeSpan"] = Row1.VTimeSpan;
      row["FGuid"] = Row1.VGuid;
      row["FEnum"] = Row1.VEnum;
      tbl.Rows.Add(row); // [1]

      row = tbl.NewRow();
      row["FString"] = Row2.VString;
      row["FInt32"] = Row2.VInt32;
      row["FInt64"] = Row2.VInt64;
      row["FSingle"] = Row2.VSingle;
      row["FDouble"] = Row2.VDouble;
      row["FDecimal"] = Row2.VDecimal;
      row["FBoolean"] = Row2.VBool;
      row["FDateTime"] = Row2.VDateTime;
      row["FTimeSpan"] = Row2.VTimeSpan;
      row["FGuid"] = Row2.VGuid;
      row["FEnum"] = Row2.VEnum;
      tbl.Rows.Add(row); // [2]

      return tbl;
    }

    #endregion
  }

  /// <summary>
  /// Структура для тестирования функций, принимающих 1 аргумент
  /// </summary>
  public struct TestPair
  {
    // не буду использовать DictionaryEntry, вдруг она не допускает key=null

    #region Конструктор

    public TestPair(object source, object result)
    {
      _Source = source;
      _Result = result;
    }

    #endregion

    #region Свойства

    public object Source { get { return _Source; } }
    private object _Source;

    public object Result { get { return _Result; } }
    private object _Result;

    public override string ToString()
    {
      if (Object.ReferenceEquals(_Source, null))
        return "null";
      StringBuilder sb = new StringBuilder();
      if (_Source is string)
        sb.Append(DataTools.StrToCSharpString((string)_Source));
      else if (_Source is byte[])
        sb.Append(DataTools.BytesToHex((byte[])_Source, false));
      else
        sb.Append(_Source.ToString());

      sb.Append(" (");
      sb.Append(_Source.GetType().Name);
      if (_Source is Array)
      {
        if (_Source.GetType().Name.EndsWith("[]", StringComparison.Ordinal))
          sb.Remove(sb.Length - 2, 2);
        sb.Append("[");
        sb.Append(((Array)_Source).Length);
        sb.Append("]");
      }
      sb.Append(")");
      return sb.ToString();
    }

    #endregion
  }
}
