using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Collections;
using FreeLibSet.Remoting;
using FreeLibSet.Core;

// Выполняется тестирование класса фильтров вне связи с табличными данными

namespace ExtTools_tests.Data
{
  [TestFixture]
  public class ValueFilterTests
  {
    #region Конструкторы для выражения

    [Test]
    public void Constructor_expr_4args()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      ValueFilter sut = new ValueFilter(expr1, 123, CompareKind.GreaterThan, DBxColumnType.Int32);
      Assert.AreSame(expr1, sut.Expression1, "Expression1");
      Assert.IsInstanceOf<DBxConst>(sut.Expression2, "Expression2 type");
      Assert.AreEqual(123, sut.Expression2.GetConst().Value, "Expression2 value");
      Assert.AreEqual(CompareKind.GreaterThan, sut.Kind, "Kind");
      Assert.IsFalse(sut.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.IsFalse(sut.ComparisionToNull, "ComparisionToNull");
      Assert.AreEqual(DBxColumnType.Int32, sut.ColumnType, "ColumnType");
    }

    [Test]
    public void Constructor_expr_3args()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      ValueFilter sut = new ValueFilter(expr1, 123, CompareKind.GreaterThan);
      Assert.AreSame(expr1, sut.Expression1, "Expression1");
      Assert.IsInstanceOf<DBxConst>(sut.Expression2, "Expression2 type");
      Assert.AreEqual(123, sut.Expression2.GetConst().Value, "Expression2 value");
      Assert.AreEqual(CompareKind.GreaterThan, sut.Kind, "Kind");
      Assert.IsFalse(sut.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.IsFalse(sut.ComparisionToNull, "ComparisionToNull");
    }

    [Test]
    public void Constructor_expr_2args()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      ValueFilter sut = new ValueFilter(expr1, 123);
      Assert.AreSame(expr1, sut.Expression1, "Expression1");
      Assert.IsInstanceOf<DBxConst>(sut.Expression2, "Expression2 type");
      Assert.AreEqual(123, sut.Expression2.GetConst().Value, "Expression2 value");
      Assert.AreEqual(CompareKind.Equal, sut.Kind, "Kind");
      Assert.IsFalse(sut.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.IsFalse(sut.ComparisionToNull, "ComparisionToNull");
    }


    [Test]
    public void Constructor_expr_NULL()
    {
      DBxExpression expr1 = new DBxColumn("F1");
      ValueFilter sut = new ValueFilter(expr1, null, CompareKind.NotEqual, DBxColumnType.Int32);
      Assert.IsNull(sut.Expression2.GetConst().Value, "Expression2 value");
      Assert.AreEqual(CompareKind.NotEqual, sut.Kind, "Kind");
      Assert.IsFalse(sut.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.IsTrue(sut.ComparisionToNull, "ComparisionToNull");
    }

    #endregion

    #region Конструкторы для имени поля

    [Test]
    public void Constructor_colName_4args()
    {
      ValueFilter sut = new ValueFilter("F1", 123, CompareKind.GreaterThan, DBxColumnType.Int32);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression1, "Expression1 type");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression1)).ColumnName, "ColumnName1");
      Assert.IsInstanceOf<DBxConst>(sut.Expression2, "Expression2 type");
      Assert.AreEqual(123, sut.Expression2.GetConst().Value, "Expression2 value");
      Assert.AreEqual(CompareKind.GreaterThan, sut.Kind, "Kind");
      Assert.IsFalse(sut.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.IsFalse(sut.ComparisionToNull, "ComparisionToNull");
      Assert.AreEqual(DBxColumnType.Int32, sut.ColumnType, "ColumnType");
    }

    [Test]
    public void Constructor_colName_3args()
    {
      ValueFilter sut = new ValueFilter("F1", 123, CompareKind.GreaterThan);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression1, "Expression1 type");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression1)).ColumnName, "ColumnName1");
      Assert.IsInstanceOf<DBxConst>(sut.Expression2, "Expression2 type");
      Assert.AreEqual(123, sut.Expression2.GetConst().Value, "Expression2 value");
      Assert.AreEqual(CompareKind.GreaterThan, sut.Kind, "Kind");
    }

    [Test]
    public void Constructor_colName_2args()
    {
      ValueFilter sut = new ValueFilter("F1", 123);
      Assert.IsInstanceOf<DBxColumn>(sut.Expression1, "Expression1 type");
      Assert.AreEqual("F1", ((DBxColumn)(sut.Expression1)).ColumnName, "ColumnName1");
      Assert.IsInstanceOf<DBxConst>(sut.Expression2, "Expression2 type");
      Assert.AreEqual(123, sut.Expression2.GetConst().Value, "Expression2 value");
      Assert.AreEqual(CompareKind.Equal, sut.Kind, "Kind");
    }


    [Test]
    public void Constructor_ColumnType()
    {
      Assert.DoesNotThrow(delegate () { ValueFilter sut = new ValueFilter("F1", null, CompareKind.NotEqual, DBxColumnType.Int32); }, "Int");
      Assert.Catch(delegate () { ValueFilter sut = new ValueFilter("F1", null, CompareKind.NotEqual, DBxColumnType.Unknown); }, "Unknown");
    }

    #endregion

    #region NullAsDefaultValue

    [TestCase(-1, CompareKind.Equal, false)]
    [TestCase(0, CompareKind.Equal, true)]
    [TestCase(1, CompareKind.Equal, false)]
    // Для остальных режимов проверяем только, что NullAsDefaultValue=true. Обратное условие зависит от реализации
    [TestCase(-1, CompareKind.NotEqual, true)]
    [TestCase(1, CompareKind.NotEqual, true)]
    [TestCase(1, CompareKind.LessThan, true)]
    [TestCase(1, CompareKind.LessOrEqualThan, true)]
    [TestCase(0, CompareKind.LessOrEqualThan, true)]
    [TestCase(-1, CompareKind.GreaterThan, true)]
    [TestCase(-1, CompareKind.GreaterOrEqualThan, true)]
    [TestCase(0, CompareKind.GreaterOrEqualThan, true)]
    public void NullAsDefaultValue(object constValue, CompareKind kind, bool wantedValue)
    {
      DBxExpression expr1 = new DBxColumn("F1");
      ValueFilter sut = new ValueFilter(expr1, constValue, kind);
      Assert.AreEqual(wantedValue, sut.NullAsDefaultValue);
    }

    #endregion

    #region TestFilter()

    [TestCase(1, 0, CompareKind.Equal, DBxColumnType.Unknown, false)]
    [TestCase(0, 0, CompareKind.Equal, DBxColumnType.Unknown, true)]
    [TestCase(null, 0, CompareKind.Equal, DBxColumnType.Unknown, true)]
    [TestCase(null, 1, CompareKind.Equal, DBxColumnType.Unknown, false)]
    [TestCase(null, 1, CompareKind.GreaterThan, DBxColumnType.Unknown, false)]
    [TestCase(null, 0, CompareKind.GreaterThan, DBxColumnType.Unknown, false)]
    [TestCase(null, -1, CompareKind.GreaterThan, DBxColumnType.Unknown, true)]

    [TestCase(1, null, CompareKind.Equal, DBxColumnType.Int32, false)]
    [TestCase(0, null, CompareKind.Equal, DBxColumnType.Int32, false)]
    [TestCase(null, null, CompareKind.Equal, DBxColumnType.Int32, true)]

    [TestCase(false, false, CompareKind.Equal, DBxColumnType.Unknown, true)]
    [TestCase(null, false, CompareKind.Equal, DBxColumnType.Unknown, true)]
    [TestCase(true, false, CompareKind.Equal, DBxColumnType.Unknown, false)]
    public void TestFilter(object v1, object constValue, CompareKind kind, DBxColumnType columnType, bool wantedRes)
    {
      TypedStringDictionary<object> vals = new TypedStringDictionary<object>(false);
      vals.Add("F1", v1);
      ValueFilter sut = new ValueFilter("F1", constValue, kind, columnType);
      bool res = sut.TestFilter(vals);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region CreateFilter()

    #region 0

    [Test]
    public void CreateFilter_0()
    {
      DBxFilter sut1 = ValueFilter.CreateFilter(DBxColumns.Empty, EmptyArray<object>.Empty);
      Assert.IsNull(sut1, "#1");

      System.Collections.Hashtable ht = new System.Collections.Hashtable();
      DBxFilter sut2 = ValueFilter.CreateFilter(ht);
      Assert.IsNull(sut2, "#2");
    }

    #endregion

    #region 1

    [Test]
    public void CreateFilter_1()
    {
      DBxFilter sut1 = ValueFilter.CreateFilter(new DBxColumns("F1"), new object[1] { 123 });
      DoCreateFilter_1_Test(sut1, "F1", 123, "#1");

      System.Collections.Hashtable ht = new System.Collections.Hashtable();
      ht.Add("F1", 123);
      DBxFilter sut2 = ValueFilter.CreateFilter(ht);
      DoCreateFilter_1_Test(sut2, "F1", 123, "#2");
    }

    private void DoCreateFilter_1_Test(DBxFilter sut, string columnName, object value, string msgPrefix)
    {
      Assert.IsInstanceOf<ValueFilter>(sut, msgPrefix + ". Filter type");
      ValueFilter filter2 = (ValueFilter)sut;
      Assert.IsInstanceOf<DBxColumn>(filter2.Expression1, msgPrefix + ". Expression1 type");
      Assert.IsInstanceOf<DBxConst>(filter2.Expression2, msgPrefix + ". Expression2 type");
      Assert.AreEqual(CompareKind.Equal, filter2.Kind, msgPrefix + ". CompareKind");

      string thisColName = ((DBxColumn)(filter2.Expression1)).ColumnName;
      Assert.AreEqual(columnName, thisColName, msgPrefix + ". Column name");
      object thisValue = filter2.Expression2.GetConst().Value;
      Assert.AreEqual(value, thisValue, msgPrefix + ". Value");
    }

    #endregion

    #region 2+

    [Test]
    public void CreateFilter_2()
    {
      DoCreateFilterAnd(new DBxColumns("F1,F2"), new object[2] { 1, "ABC" });
    }

    [Test]
    public void CreateFilter_3()
    {
      DoCreateFilterAnd(new DBxColumns("F1,F2,F3"), new object[3] { 1, true, "ABC" });
    }

    private static void DoCreateFilterAnd(DBxColumns columnNames, object[] values)
    {
      DBxFilter sut1 = ValueFilter.CreateFilter(columnNames, values);
      DoCreateFilterAnd_Test(sut1, columnNames, values, "#1");

      System.Collections.Hashtable ht = new System.Collections.Hashtable();
      for (int i = 0; i < columnNames.Count; i++)
        ht.Add(columnNames[i], values[i]);
      DBxFilter sut2 = ValueFilter.CreateFilter(ht);
      DoCreateFilterAnd_Test(sut2, columnNames, values, "#2");
    }

    private static void DoCreateFilterAnd_Test(DBxFilter sut, DBxColumns columnNames, object[] values, string msgPrefix)
    {
      Assert.IsInstanceOf<AndFilter>(sut, msgPrefix + ". Filter type");
      DBxFilter[] filters = ((AndFilter)sut).Filters;
      Assert.AreEqual(columnNames.Count, filters.Length, msgPrefix + ". Filters.Count");
      // Порядок фильтров в списке может не совпадать с исходным в случае хэш-таблицы
      foreach (DBxFilter filter in filters)
      {
        Assert.IsInstanceOf<ValueFilter>(filter, msgPrefix + ". Internal filter type");
        ValueFilter filter2 = (ValueFilter)filter;
        Assert.IsInstanceOf<DBxColumn>(filter2.Expression1, msgPrefix + ". Internal filter Expression1 type");
        Assert.IsInstanceOf<DBxConst>(filter2.Expression2, msgPrefix + ". Internal filter Expression2 type");
        Assert.AreEqual(CompareKind.Equal, filter2.Kind, msgPrefix + ". Internal filter CompareKind");

        string thisColName = ((DBxColumn)(filter2.Expression1)).ColumnName;
        object thisValue = filter2.Expression2.GetConst().Value;
        Assert.IsTrue(columnNames.Contains(thisColName), msgPrefix+ ". Column name");
        object wantedValue = values[columnNames.IndexOf(thisColName)];
        Assert.AreEqual(wantedValue, thisValue, msgPrefix + ". Value");
      }
    }

    #endregion

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      ValueFilter sut = new ValueFilter("F1", 123, CompareKind.GreaterThan, DBxColumnType.Int32);
      byte[] b = SerializationTools.SerializeBinary(sut);
      ValueFilter res = (ValueFilter)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
      Assert.AreEqual(sut.Expression1, res.Expression1, "Expression1");
      Assert.AreEqual(sut.Expression2, res.Expression2, "Expression2");
      Assert.AreEqual(sut.Kind, res.Kind, "Kind");
      Assert.AreEqual(sut.NullAsDefaultValue, res.NullAsDefaultValue, "NullAsDefaultValue");
      Assert.AreEqual(sut.ColumnType, res.ColumnType, "ColumnType");
    }

    #endregion

  }
}
