using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Calendar;
using System.Data;

namespace ExtTools_tests
{
  /// <summary>
  /// Генераторы объектов для тестирования
  /// </summary>
  public static class Creators
  {
    #region Объекты даты и времени

    #region DateTime

    /// <summary>
    /// Создает DateTime из строки в формате "ГГГГММДД"
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static DateTime CreateDate(string s)
    {
      if (s.Length != 8)
        throw new ArgumentException();
      return DateTime.ParseExact(s, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string ToString(DateTime value)
    {
      return value.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Для пустой строки возвращает null, иначе возвращает DateTime из строки в формате "ГГГГММДД"
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static DateTime? CreateNDate(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;
      else
        return CreateDate(s);
    }

    public static string ToString(DateTime? value)
    {
      if (value.HasValue)
        return ToString(value.Value);
      else
        return String.Empty;
    }

    /// <summary>
    /// Создает объект DateRange из строки в формате "ГГГГММДД-ГГГГММДД".
    /// Возвращает DateRange.Empty для пустой строки
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static DateRange CreateDateRange(string s)
    {
      if (String.IsNullOrEmpty(s))
        return DateRange.Empty;
      else
      {
        if (s.Length != 17 || s[8] != '-')
          throw new ArgumentException();

        return new DateRange(CreateDate(s.Substring(0, 8)), CreateDate(s.Substring(9, 8)));
      }
    }

    public static string ToString(DateRange value)
    {
      if (value.IsEmpty)
        return String.Empty;
      else
        return ToString(value.FirstDate) + "-" + ToString(value.LastDate);
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Преобразует строку вида "Ч:М:С" в структуру TimeSpan.
    /// Для пустой строки возвращает null.
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns>Nullable-структура</returns>
    public static TimeSpan? CreateNTimeSpan(string s)
    {
      if (s.Length == 0)
        return null;
      else
        return TimeSpan.Parse(s);
    }


    #endregion

    #region YearMonth

    /// <summary>
    /// Создает объект YearMonth из строки вида "YYYYMM".
    /// Если строка - пустая, возвращает YearMonth.Empty
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static YearMonth CreateYearMonth(string s)
    {
      if (String.IsNullOrEmpty(s))
        return new YearMonth();
      else
      {
        if (s.Length != 6)
          throw new ArgumentException();
        int y = int.Parse(s.Substring(0, 4));
        int m = int.Parse(s.Substring(4, 2));
        return new YearMonth(y, m);
      }
    }

    #endregion

    #region MonthDay

    /// <summary>
    /// Создает объект MonthDay из четырехсимвольной строки "ММДД".
    /// Для пустой строки возращает пустой объект
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static MonthDay CreateMonthDay(string s)
    {
      if (s.Length == 0)
        return MonthDay.Empty;

      if (s.Length != 4)
        throw new ArgumentException();

      int m = int.Parse(s.Substring(0, 2));
      int d = int.Parse(s.Substring(2, 2));
      return new MonthDay(m, d);
    }

    public static string ToString(MonthDay value)
    {
      if (value.IsEmpty)
        return String.Empty;
      else
        return value.Month.ToString("00") + value.Day.ToString("00");
    }

    /// <summary>
    /// Создает массив текстовых представлений MonthDay в виде "ММДД,ММДД".
    /// Для пустого массива возвращает пустую строку
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static string ToString(MonthDay[] a)
    {
      if (a.Length == 0)
        return string.Empty;
      string[] a2 = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
        a2[i] = ToString(a[i]);
      return String.Join(",", a2);
    }


    /// <summary>
    /// Создает объект MonthDayRange из строки в формате "ММДД-ММДД".
    /// Для пустой строки возращает MonthDayRange.Empty
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static MonthDayRange CreateMonthDayRange(string s)
    {
      if (String.IsNullOrEmpty(s))
        return MonthDayRange.Empty;
      else
      {
        if (s.Length != 9 || s[4] != '-')
          throw new ArgumentException();
        return new MonthDayRange(CreateMonthDay(s.Substring(0, 4)), CreateMonthDay(s.Substring(5, 4)));
      }
    }

    public static string ToString(MonthDayRange value)
    {
      if (value.IsEmpty)
        return String.Empty;
      else
        return ToString(value.First) + "-" + ToString(value.Last);
    }


    /// <summary>
    /// Создает массив текстовых представлений MonthDayRange в виде "ММДД-ММДД,ММДД-ММДД".
    /// Для пустого массива возвращает пустую строку
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static string ToString(MonthDayRange[] a)
    {
      if (a.Length == 0)
        return string.Empty;
      string[] a2 = new string[a.Length];
      for (int i = 0; i < a.Length; i++)
        a2[i] = ToString(a[i]);
      return String.Join(",", a2);
    }

    #endregion

    #endregion

    #region Таблица данных

    /// <summary>
    /// Тестовое перечисление для GetEnum()
    /// </summary>
    public enum TestEnum
    {
      Zero = 0, One = 1, Two = 2, Three = 3
    }

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

      public const float VSingle = (3.5f+30f)/2f;

      public const double VDouble = (4.5+40.0)/2.0;

      public const decimal VDecimal = (5.5m+50m)/2m;

      //public static readonly DateTime VDateTime = new DateTime(2021, 12, 15);

      public static readonly TimeSpan VTimeSpan = new TimeSpan((Row1.VTimeSpan.Ticks + Row2.VTimeSpan.Ticks)/2L); // ((1,2,3)+(4,5,6))
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
}
