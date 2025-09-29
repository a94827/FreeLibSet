using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Collections;
using FreeLibSet.Remoting;
using FreeLibSet.Core;

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class DBxToolsTests
  {
    #region DataTypeToColumnType(), ColumnTypeToDataType() 

    [TestCase(null, DBxColumnType.Unknown)]
    [TestCase(typeof(DBNull), DBxColumnType.Unknown)]
    [TestCase(typeof(String), DBxColumnType.String)]
    [TestCase(typeof(Int16), DBxColumnType.Int16)]
    [TestCase(typeof(Int32), DBxColumnType.Int32)]
    [TestCase(typeof(Int64), DBxColumnType.Int64)]
    [TestCase(typeof(Single), DBxColumnType.Single)]
    [TestCase(typeof(Double), DBxColumnType.Double)]
    [TestCase(typeof(Decimal), DBxColumnType.Decimal)]
    [TestCase(typeof(Boolean), DBxColumnType.Boolean)]
    [TestCase(typeof(DateTime), DBxColumnType.DateTime)]
    [TestCase(typeof(TimeSpan), DBxColumnType.Time)]
    [TestCase(typeof(Guid), DBxColumnType.Guid)]
    [TestCase(typeof(byte[]), DBxColumnType.Binary)]
    public void DataTypeToColumnType(Type t, DBxColumnType wantedRes)
    {
      DBxColumnType res = DBxTools.DataTypeToColumnType(t);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase(DBxColumnType.Unknown, null)]
    [TestCase(DBxColumnType.String, typeof(string))]
    [TestCase(DBxColumnType.Memo, typeof(string))]
    // пока не определено [TestCase(DBxColumnType.Int, typeof(string))]
    [TestCase(DBxColumnType.Double, typeof(Double))]
    [TestCase(DBxColumnType.Decimal, typeof(Decimal))]
    [TestCase(DBxColumnType.Boolean, typeof(Boolean))]
    [TestCase(DBxColumnType.Date, typeof(DateTime))]
    [TestCase(DBxColumnType.DateTime, typeof(DateTime))]
    [TestCase(DBxColumnType.Time, typeof(TimeSpan))]
    [TestCase(DBxColumnType.Binary, typeof(byte[]))]
    [TestCase(DBxColumnType.Guid, typeof(Guid))]
    // ?? Xml
    public void ColumnTypeToDataType(DBxColumnType columnType, Type wantedRes)
    {
      Type res = DBxTools.ColumnTypeToDataType(columnType);
      Assert.AreEqual(wantedRes, res);

      // Не верно
      //// Проверка обратимости
      //DBxColumnType res2 = DBxTools.DataTypeToColumnType(res);
      //Assert.AreEqual(columnType, res2, "DataTypeToColumnType()");
    }

    #endregion

    #region ValueToColumnType()

    public struct ValueToColumnTypeTestInfo
    {
      #region Конструктор

      public ValueToColumnTypeTestInfo(object value, DBxColumnType columnType)
      {
        this.Value = value;
        this.ColumnType = columnType;
      }

      #endregion

      #region Поля

      public object Value;

      public DBxColumnType ColumnType;

      public override string ToString()
      {
        if (Value == null)
          return "null";
        else
          return Value.ToString() + " (" + Value.GetType().Name + ")";
      }

      #endregion
    }

    public static readonly ValueToColumnTypeTestInfo[] ValueToColumnTypeTests = new ValueToColumnTypeTestInfo[] {
      new ValueToColumnTypeTestInfo(null, DBxColumnType.Unknown),
      new ValueToColumnTypeTestInfo(DBNull.Value, DBxColumnType.Unknown),
      new ValueToColumnTypeTestInfo("ABC", DBxColumnType.String),
      new ValueToColumnTypeTestInfo(123, DBxColumnType.Int32),
      new ValueToColumnTypeTestInfo(123L, DBxColumnType.Int64),
      new ValueToColumnTypeTestInfo(123f, DBxColumnType.Single),
      new ValueToColumnTypeTestInfo(123.0, DBxColumnType.Double),
      new ValueToColumnTypeTestInfo(123m, DBxColumnType.Decimal),
      new ValueToColumnTypeTestInfo(false, DBxColumnType.Boolean),
      new ValueToColumnTypeTestInfo(new DateTime(2023, 5, 17), DBxColumnType.Date),
      new ValueToColumnTypeTestInfo(new DateTime(2023, 5, 17, 12, 34, 56), DBxColumnType.DateTime),
      new ValueToColumnTypeTestInfo(DateTime.MinValue +  new TimeSpan(12, 34, 56), DBxColumnType.Time),
      new ValueToColumnTypeTestInfo(new TimeSpan(12, 34, 56), DBxColumnType.Time),
      new ValueToColumnTypeTestInfo(Guid.Empty, DBxColumnType.Guid),
      new ValueToColumnTypeTestInfo(EmptyArray<Byte>.Empty, DBxColumnType.Binary) };

    [TestCaseSource("ValueToColumnTypeTests")]
    public void ValueToColumnType(ValueToColumnTypeTestInfo info)
    {
      DBxColumnType res = DBxTools.ValueToColumnType(info.Value);
      Assert.AreEqual(info.ColumnType, res);
    }

    #endregion

    #region GetDefaultValue()

    public struct GetDefaultValueTestInfo
    {
      #region Конструктор

      public GetDefaultValueTestInfo(DBxColumnType columnType, object defaultValue)
      {
        this.ColumnType = columnType;
        this.DefaultValue = defaultValue;
      }

      #endregion

      #region Поля

      public DBxColumnType ColumnType;

      public object DefaultValue;

      public override string ToString()
      {
        return ColumnType.ToString();
      }

      #endregion
    }

    public static readonly GetDefaultValueTestInfo[] GetDefaultValueTests = new GetDefaultValueTestInfo[] {
      new GetDefaultValueTestInfo(DBxColumnType.Unknown, null),
      new GetDefaultValueTestInfo(DBxColumnType.String, String.Empty),
      // пока не определено new GetDefaultValueTestInfo(DBxColumnType.Int, 0),
      new GetDefaultValueTestInfo(DBxColumnType.Double, 0.0),
      new GetDefaultValueTestInfo(DBxColumnType.Decimal, 0m),
      new GetDefaultValueTestInfo(DBxColumnType.Boolean, false),
      new GetDefaultValueTestInfo(DBxColumnType.Date, DateTime.MinValue),
      new GetDefaultValueTestInfo(DBxColumnType.DateTime, DateTime.MinValue),
      new GetDefaultValueTestInfo(DBxColumnType.Time, TimeSpan.Zero),
      new GetDefaultValueTestInfo(DBxColumnType.Guid, Guid.Empty),
      new GetDefaultValueTestInfo(DBxColumnType.Binary, EmptyArray<Byte>.Empty) };

    [TestCaseSource("GetDefaultValueTests")]
    public void GetDefaultValue(GetDefaultValueTestInfo info)
    {
      object res = DBxTools.GetDefaultValue(info.ColumnType);
      Assert.AreEqual(info.DefaultValue, res);
    }

    #endregion

    #region Convert()

    public struct ConvertTestInfo
    {
      #region Конструктор

      public ConvertTestInfo(object source, DBxColumnType colType, object wantedRes)
      {
        this.Source = source;
        this.ColType = colType;
        this.wantedRes = wantedRes;
      }

      #endregion

      #region Свойства

      public object Source;
      public DBxColumnType ColType;
      public object wantedRes;

      public override string ToString()
      {
        StringBuilder sb = new StringBuilder();
        if (Source == null)
          sb.Append("null");
        else
        {
          if (Source is string)
            sb.Append(StringTools.StrToCSharpString((string)Source));
          else
            sb.Append(Source.ToString());
          sb.Append(" (");
          sb.Append(Source.GetType().Name);
          sb.Append(")");
        }
        sb.Append(" -> ");
        sb.Append(ColType.ToString());
        return sb.ToString();
      }

      #endregion
    }

    private const string GuidString = "b0733159-7ad7-4233-9253-af06906aea27";
    private static readonly Guid GuidValue = new Guid(GuidString);

    public static readonly ConvertTestInfo[] ConvertTests = new ConvertTestInfo[] {
      // To Unknown
      new ConvertTestInfo("ABC ", DBxColumnType.Unknown, "ABC "),
      new ConvertTestInfo(123, DBxColumnType.Unknown, 123),
      new ConvertTestInfo(123m, DBxColumnType.Unknown, 123m),
      new ConvertTestInfo(GuidValue, DBxColumnType.Unknown, GuidValue),
      new ConvertTestInfo(null, DBxColumnType.Unknown, null),
      new ConvertTestInfo(DBNull.Value, DBxColumnType.Unknown, null),

      // From null
      new ConvertTestInfo(null, DBxColumnType.String, ""),
      new ConvertTestInfo(null, DBxColumnType.Int32, 0),
      new ConvertTestInfo(null, DBxColumnType.Double, 0.0),
      new ConvertTestInfo(null, DBxColumnType.Decimal, 0m),
      new ConvertTestInfo(null, DBxColumnType.Boolean, false),
      new ConvertTestInfo(null, DBxColumnType.Date, DateTime.MinValue),
      new ConvertTestInfo(null, DBxColumnType.DateTime, DateTime.MinValue),
      new ConvertTestInfo(null, DBxColumnType.Time, TimeSpan.Zero),
      new ConvertTestInfo(null, DBxColumnType.Guid, Guid.Empty),
      new ConvertTestInfo(null, DBxColumnType.Binary, EmptyArray<Byte>.Empty),

      // From DBNull
      new ConvertTestInfo(DBNull.Value, DBxColumnType.String, ""),
      new ConvertTestInfo(DBNull.Value, DBxColumnType.Int32, 0),
      new ConvertTestInfo(DBNull.Value, DBxColumnType.Double, 0.0),
      new ConvertTestInfo(DBNull.Value, DBxColumnType.Decimal, 0m),
      new ConvertTestInfo(DBNull.Value, DBxColumnType.Boolean, false),
      new ConvertTestInfo(DBNull.Value, DBxColumnType.Date, DateTime.MinValue),
      new ConvertTestInfo(DBNull.Value, DBxColumnType.DateTime, DateTime.MinValue),
      new ConvertTestInfo(DBNull.Value, DBxColumnType.Time, TimeSpan.Zero),
      new ConvertTestInfo(DBNull.Value, DBxColumnType.Guid, Guid.Empty),
      new ConvertTestInfo(DBNull.Value, DBxColumnType.Binary, EmptyArray<Byte>.Empty),

      // Из пустой строки
      new ConvertTestInfo("", DBxColumnType.String, ""),
      new ConvertTestInfo("", DBxColumnType.Int32, 0),
      new ConvertTestInfo("", DBxColumnType.Double, 0.0),
      new ConvertTestInfo("", DBxColumnType.Decimal, 0m),
      new ConvertTestInfo("", DBxColumnType.Boolean, false),
      new ConvertTestInfo("", DBxColumnType.Date, DateTime.MinValue),
      new ConvertTestInfo("", DBxColumnType.DateTime, DateTime.MinValue),
      new ConvertTestInfo("", DBxColumnType.Time, TimeSpan.Zero),
      new ConvertTestInfo("", DBxColumnType.Guid, Guid.Empty),
      new ConvertTestInfo("", DBxColumnType.Binary, EmptyArray<Byte>.Empty),

      // Из непустой строки
      new ConvertTestInfo("ABC", DBxColumnType.String, "ABC"),
      new ConvertTestInfo("123", DBxColumnType.Int32, 123),
      new ConvertTestInfo("123.45", DBxColumnType.Double, 123.45),
      new ConvertTestInfo("123.45", DBxColumnType.Decimal, 123.45m),
      new ConvertTestInfo("true", DBxColumnType.Boolean, true),
      new ConvertTestInfo("false", DBxColumnType.Boolean, false),
      new ConvertTestInfo("1", DBxColumnType.Boolean, true),
      new ConvertTestInfo("0", DBxColumnType.Boolean, false),
      new ConvertTestInfo("2023-05-16", DBxColumnType.Date, new DateTime(2023, 5, 16)),
      new ConvertTestInfo("2023-05-16T12:34:56", DBxColumnType.DateTime, new DateTime(2023, 5, 16, 12, 34, 56)),
      new ConvertTestInfo("12:34:56", DBxColumnType.Time, new TimeSpan(12, 34, 56)),
      new ConvertTestInfo(GuidString, DBxColumnType.Guid, GuidValue),
      new ConvertTestInfo("123456", DBxColumnType.Binary, new byte[] { 0x12, 0x34, 0x56}),

      // В строку
      new ConvertTestInfo(123, DBxColumnType.String, "123"),
      new ConvertTestInfo(123L, DBxColumnType.String, "123"),
      new ConvertTestInfo(123.4f, DBxColumnType.String, "123.4"),
      new ConvertTestInfo(123.4, DBxColumnType.String, "123.4"),
      new ConvertTestInfo(123.4m, DBxColumnType.String, "123.4"),
      new ConvertTestInfo(false, DBxColumnType.String, Boolean.FalseString),
      new ConvertTestInfo(true, DBxColumnType.String, Boolean.TrueString),
      // для DateTime зависит от реализации
      new ConvertTestInfo(new TimeSpan(12, 34, 56), DBxColumnType.String, "12:34:56"),
      new ConvertTestInfo(GuidValue, DBxColumnType.String, GuidString),
      new ConvertTestInfo(new byte[] { 0x01, 0x02, 0x03}, DBxColumnType.String, "010203"),

      // Между числовыми типами
      // Не забываем об ограниченной точности Single и Double
      new ConvertTestInfo(123, DBxColumnType.Int32, 123),
      new ConvertTestInfo(123L, DBxColumnType.Int32, 123),
      new ConvertTestInfo(1.2f, DBxColumnType.Int32, 1),
      new ConvertTestInfo(1.2, DBxColumnType.Int32, 1),
      new ConvertTestInfo(1.2m, DBxColumnType.Int32, 1),
    
      new ConvertTestInfo(123, DBxColumnType.Double, 123.0),
      new ConvertTestInfo(123L, DBxColumnType.Double, 123.0),
      new ConvertTestInfo(1.2f, DBxColumnType.Double, 1.2),
      new ConvertTestInfo(1.2, DBxColumnType.Double, 1.2),
      new ConvertTestInfo(1.2m, DBxColumnType.Double, 1.2),

      new ConvertTestInfo(123, DBxColumnType.Decimal, 123m),
      new ConvertTestInfo(123L, DBxColumnType.Decimal, 123m),
      new ConvertTestInfo(1.2f, DBxColumnType.Decimal, 1.2m),
      new ConvertTestInfo(1.2, DBxColumnType.Decimal, 1.2m),
      new ConvertTestInfo(1.2m, DBxColumnType.Decimal, 1.2m),

      // Между датой и временем
      new ConvertTestInfo(new DateTime(2023, 5, 16, 12, 34, 56), DBxColumnType.DateTime, new DateTime(2023, 5, 16, 12, 34, 56)),
      new ConvertTestInfo(new TimeSpan(12, 34, 56), DBxColumnType.DateTime, new DateTime(1, 1, 1, 12, 34, 56)),
      new ConvertTestInfo(new DateTime(2023, 5, 16, 12, 34, 56), DBxColumnType.Date, new DateTime(2023, 5, 16)),
      // зависит от реализации new ConvertTestInfo(new TimeSpan(12, 34, 56), DBxColumnType.Date, new DateTime(1, 1, 1)),
      new ConvertTestInfo(new DateTime(2023, 5, 16, 12, 34, 56), DBxColumnType.Time, new TimeSpan(12, 34, 56)),
      new ConvertTestInfo(new TimeSpan(12, 34, 56), DBxColumnType.Time, new TimeSpan(12, 34, 56)),
    };

    [TestCaseSource("ConvertTests")]
    public void Convert(ConvertTestInfo info)
    {
      object res = DBxTools.Convert(info.Source, info.ColType);

      // Для DBxColumnType.Int может возвращаться либо Int32, либо Int64 (зависит от реализации)
      if (!(IsInteger(info.wantedRes) && IsInteger(res)))
        Assert.AreEqual(GetType(info.wantedRes), GetType(res), "Type");

      if (GetType(info.wantedRes) == typeof(double))
        Assert.AreEqual((double)(info.wantedRes), (double)res, 0.01, "Value");
      else if (GetType(info.wantedRes) == typeof(float))
      {
        double dblWanted = (double)(info.wantedRes);
        double dblReal = (double)res;
        Assert.AreEqual(dblWanted, dblReal, 0.01, "Value");
      }
      else
        Assert.AreEqual(info.wantedRes, res);
    }

    private static Type GetType(object x)
    {
      if (Object.ReferenceEquals(x, null))
        return null;
      else
        return x.GetType();
    }

    private static bool IsInteger(object x)
    {
      if (Object.ReferenceEquals(x, null))
        return false;
      else
        return MathTools.IsIntegerType(x.GetType());
    }


    #endregion

    #region GetColumnNameExpressions()

    [Test]
    public void GetColumnNameExpressions()
    {
      string[] colNames = new string[] { "F1", "F2", "F3" };
      DBxExpression[] res = DBxTools.GetColumnNameExpressions(colNames);
      Assert.AreEqual(colNames.Length, res.Length, "Length");
      for (int i = 0; i < colNames.Length; i++)
      {
        Assert.IsInstanceOf<DBxColumn>(res[i], "Type");
        Assert.AreEqual(colNames[i], ((DBxColumn)(res[i])).ColumnName, "ColumnName");
      }
    }

    #endregion
  }
}
