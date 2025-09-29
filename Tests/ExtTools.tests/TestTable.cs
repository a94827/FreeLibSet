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

      public const bool VBoolean = true;

      public static readonly DateTime VDateTime = new DateTime(2021, 12, 15);

      public static readonly TimeSpan VTimeSpan = new TimeSpan(1, 2, 3);

      public static readonly Guid VGuid = new Guid("2c0dfea6-8326-44e4-ba55-a994e0bedd10");

      public const TestEnum VEnum = TestEnum.Two;

      public static readonly byte[] VBytes = new byte[] { 1, 2, 3 };

      public const string StrInt32 = "1";
      public const string StrInt64 = "2";
      public const string StrSingle = "3.5";
      public const string StrDouble = "4.5";
      public const string StrDecimal = "5.5";
      public const string StrDateTime = "20211215";
      public const string StrTimeSpan = "01:02:03";
      public const string StrGuid = "2c0dfea6-8326-44e4-ba55-a994e0bedd10";
      public const string StrEnumString = "Two";
      public const string StrEnumInt32 = "2";
      public const int RoundedSingle = 4;
      public const int RoundedDouble = 5;
      public const int RoundedDecimal = 6;
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

      public const bool VBoolean = false;

      public static readonly DateTime VDateTime = new DateTime(2021, 12, 17);

      public static readonly TimeSpan VTimeSpan = new TimeSpan(4, 5, 6);

      public static readonly Guid VGuid = new Guid("a4ae45d5-42cf-41ec-af62-e44155698f48");

      public const TestEnum VEnum = TestEnum.Three;
      public static readonly byte[] VBytes = new byte[] { 4, 5, 6, 0 };

      public const string StrInt32 = "10";
      public const string StrInt64 = "20";
      public const string StrSingle = "30";
      public const string StrDouble = "40";
      public const string StrDecimal = "50";
      public const string StrDateTime = "20211217";
      public const string StrTimeSpan = "04:05:06";
      public const string StrGuid = "a4ae45d5-42cf-41ec-af62-e44155698f48";
      public const string StrEnumString = "Three";
      public const string StrEnumInt32 = "3";
      public const int RoundedSingle = 30;
      public const int RoundedDouble = 40;
      public const int RoundedDecimal = 50;
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
    /// [0] - содержит DBNull во всех Nullable-полях и значение по умолчанию в полях "NN".
    /// [1] - содержит значения <see cref="Row1"/>
    /// [2] - содержит значения <see cref="Row2"/>
    /// </summary>
    /// <returns></returns>
    public static DataTable Create()
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
      tbl.Columns.Add("FEnumInt32", typeof(int));
      tbl.Columns.Add("FEnumString", typeof(string));
      tbl.Columns.Add("FBytes", typeof(byte[]));
      DataColumn col;
      col = tbl.Columns.Add("FStringNN", typeof(string));
      col.AllowDBNull = false;
      col.DefaultValue = String.Empty;

      col = tbl.Columns.Add("FInt32NN", typeof(Int32));
      col.AllowDBNull = false;
      col.DefaultValue = 0;

      col = tbl.Columns.Add("FBooleanNN", typeof(Boolean));
      col.AllowDBNull = false;
      col.DefaultValue = false;

      tbl.Rows.Add(); // [0]

      DataRow row;
      row = tbl.NewRow();
      row["FString"] = Row1.VString;
      row["FInt32"] = Row1.VInt32;
      row["FInt64"] = Row1.VInt64;
      row["FSingle"] = Row1.VSingle;
      row["FDouble"] = Row1.VDouble;
      row["FDecimal"] = Row1.VDecimal;
      row["FBoolean"] = Row1.VBoolean;
      row["FDateTime"] = Row1.VDateTime;
      row["FTimeSpan"] = Row1.VTimeSpan;
      row["FGuid"] = Row1.VGuid;
      row["FEnumInt32"] = Row1.VEnum;
      row["FEnumString"] = Row1.VEnum.ToString();
      row["FBytes"] = Row1.VBytes;
      row["FStringNN"] = Row1.VString;
      row["FInt32NN"] = Row1.VInt32;
      row["FBooleanNN"] = Row1.VBoolean;
      tbl.Rows.Add(row); // [1]

      row = tbl.NewRow();
      row["FString"] = Row2.VString;
      row["FInt32"] = Row2.VInt32;
      row["FInt64"] = Row2.VInt64;
      row["FSingle"] = Row2.VSingle;
      row["FDouble"] = Row2.VDouble;
      row["FDecimal"] = Row2.VDecimal;
      row["FBoolean"] = Row2.VBoolean;
      row["FDateTime"] = Row2.VDateTime;
      row["FTimeSpan"] = Row2.VTimeSpan;
      row["FGuid"] = Row2.VGuid;
      row["FEnumInt32"] = Row2.VEnum;
      row["FEnumString"] = Row2.VEnum.ToString();
      row["FBytes"] = Row2.VBytes;
      row["FStringNN"] = Row2.VString;
      row["FInt32NN"] = Row2.VInt32;
      row["FBooleanNN"] = Row2.VBoolean;
      tbl.Rows.Add(row); // [2]

      tbl.AcceptChanges();

      if (ColumnNames.Length != tbl.Columns.Count)
        throw new BugException("ColumnNames");

      return tbl;
    }

    /// <summary>
    /// Имена столбцов таблицы <see cref="Create()"/>
    /// </summary>
    public static string[] ColumnNames = new string[] { "FString",
      "FInt32", "FInt64", "FSingle", "FDouble", "FDecimal",
      "FBoolean", "FDateTime", "FTimeSpan", "FGuid", "FEnumInt32", "FEnumString", "FBytes", "FStringNN", "FInt32NN", "FBooleanNN" };

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
        sb.Append(StringTools.StrToCSharpString((string)_Source));
      else if (_Source is byte[])
        sb.Append(StringTools.BytesToHex((byte[])_Source, false));
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
