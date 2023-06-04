using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.Remoting;
using System.Data;

namespace ExtDB_tests.Data
{
  [TestFixture]
  public class DBxFunctionTests
  {
    #region Конструкторы

    [Test]
    public void Constructor_DBxExpression()
    {
      DBxColumn arg1 = new DBxColumn("F1");
      DBxConst arg2 = new DBxConst(2);
      DBxConst arg3 = new DBxConst(3);
      DBxFunction sut = new DBxFunction(DBxFunctionKind.Substring, arg1, arg2, arg3);
      Assert.AreEqual(DBxFunctionKind.Substring, sut.Function, "Function");
      Assert.AreEqual(new DBxExpression[3] { arg1, arg2, arg3 }, sut.Arguments, "Arguments");
    }

    [Test]
    public void Constructor_ColumnNames()
    {
      DBxFunction sut = new DBxFunction(DBxFunctionKind.Add, "F1", "F2");
      Assert.AreEqual(DBxFunctionKind.Add, sut.Function, "Function");
      Assert.AreEqual(new DBxExpression[2] { new DBxColumn("F1"), new DBxColumn("F2") },
        sut.Arguments, "Arguments");
    }

    [Test]
    public void Constructor_argcountexception()
    {
      DBxColumn arg1 = new DBxColumn("F1");
      DBxConst arg2 = new DBxConst(2);
      DBxConst arg3 = new DBxConst(3);
      DBxConst arg4 = new DBxConst(4);
      Assert.Catch<ArgumentException>(delegate () { DBxFunction sut = new DBxFunction(DBxFunctionKind.Substring, arg1, arg2); }, "2");
      Assert.DoesNotThrow(delegate () { DBxFunction sut = new DBxFunction(DBxFunctionKind.Substring, arg1, arg2, arg3); }, "3");
      Assert.Catch<ArgumentException>(delegate () { DBxFunction sut = new DBxFunction(DBxFunctionKind.Substring, arg1, arg2, arg3, arg4); }, "4");
    }

    #endregion

    #region GetArgCount()

    [TestCase(DBxFunctionKind.Add, 2, 2)]
    [TestCase(DBxFunctionKind.Substract, 2, 2)]
    [TestCase(DBxFunctionKind.Multiply, 2, 2)]
    [TestCase(DBxFunctionKind.Divide, 2, 2)]
    [TestCase(DBxFunctionKind.Neg, 1, 1)]
    [TestCase(DBxFunctionKind.Abs, 1, 1)]
    [TestCase(DBxFunctionKind.Length, 1, 1)]
    [TestCase(DBxFunctionKind.Lower, 1, 1)]
    [TestCase(DBxFunctionKind.Upper, 1, 1)]
    [TestCase(DBxFunctionKind.Substring, 3, 3)]
    public void GetArgCount_defined(DBxFunctionKind kind, int wantedMin, int wantedMax)
    {
      int min, max;
      DBxFunction.GetArgCount(kind, out min, out max);
      Assert.AreEqual(wantedMin, min, "Min");
      Assert.AreEqual(wantedMax, max, "Max");
    }

    [TestCase(DBxFunctionKind.Coalesce, 2)]
    public void GetArgCount_minOnly(DBxFunctionKind kind, int wantedMin)
    {
      int min, max;
      DBxFunction.GetArgCount(kind, out min, out max);
      Assert.AreEqual(wantedMin, min, "Min");
    }

    #endregion

    #region GetValue()

    [TestCase(DBxFunctionKind.Neg, 1, -1)]
    [TestCase(DBxFunctionKind.Neg, 2.0, -2.0)]
    [TestCase(DBxFunctionKind.Abs, -10, 10)]
    [TestCase(DBxFunctionKind.Abs, 10, 10)]
    [TestCase(DBxFunctionKind.Abs, -20.0, 20.0)]
    [TestCase(DBxFunctionKind.Length, "", 0)]
    [TestCase(DBxFunctionKind.Length, "ABC", 3)]
    [TestCase(DBxFunctionKind.Lower, "", "")]
    [TestCase(DBxFunctionKind.Lower, "AbCd", "abcd")]
    [TestCase(DBxFunctionKind.Upper, "", "")]
    [TestCase(DBxFunctionKind.Upper, "AbCd", "ABCD")]
    public void GetValue_1arg(DBxFunctionKind kind, object arg1, object wantedRes)
    {
      DBxConst const1 = new DBxConst(arg1);
      DBxFunction sut = new DBxFunction(kind, const1);
      DummyNamedValues dummy = new DummyNamedValues();
      object res = sut.GetValue(dummy);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase(DBxFunctionKind.Add, 1, 2, 3)]
    [TestCase(DBxFunctionKind.Add, 10.0, 20.0, 30.0)]
    [TestCase(DBxFunctionKind.Substract, 1, 2, -1)]
    [TestCase(DBxFunctionKind.Substract, 10.0, 20.0, -10.0)]
    [TestCase(DBxFunctionKind.Multiply, 1, 2, 2)]
    [TestCase(DBxFunctionKind.Multiply, 10.0, 20.0, 200.0)]
    [TestCase(DBxFunctionKind.Divide, 6, 2, 3)]
    [TestCase(DBxFunctionKind.Divide, 60.0, 20.0, 3.0)]
    public void GetValue_2arg(DBxFunctionKind kind, object arg1, object arg2, object wantedRes)
    {
      DBxConst const1 = new DBxConst(arg1);
      DBxConst const2 = new DBxConst(arg2);
      DBxFunction sut = new DBxFunction(kind, const1, const2);
      DummyNamedValues dummy = new DummyNamedValues();
      object res = sut.GetValue(dummy);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase(DBxFunctionKind.Substring, "ABCDEF", 2, 3, "BCD")]
    public void GetValue_3arg(DBxFunctionKind kind, object arg1, object arg2, object arg3, object wantedRes)
    {
      DBxConst const1 = new DBxConst(arg1);
      DBxConst const2 = new DBxConst(arg2);
      DBxConst const3 = new DBxConst(arg3);
      DBxFunction sut = new DBxFunction(kind, const1, const2, const3);
      DummyNamedValues dummy = new DummyNamedValues();
      object res = sut.GetValue(dummy);
      Assert.AreEqual(wantedRes, res);
    }

    [TestCase(2, 0, 1)]
    [TestCase(2, 1, 2)]
    [TestCase(2, 2, null)]
    [TestCase(3, 0, 1)]
    [TestCase(3, 1, 2)]
    [TestCase(3, 2, 3)]
    [TestCase(3, 3, null)]
    public void GetValue_Colalesce(int nArgs, int nNulls, object wantedRes)
    {
      if (nArgs < 2)
        throw new ArgumentException();
      if (nNulls > nArgs)
        throw new ArgumentException();

      DBxExpression[] args = new DBxExpression[nArgs];
      for (int i = 0; i < nArgs; i++)
      {
        if (i < nNulls)
          args[i] = new DBxConst(null, DBxColumnType.Int);
        else
          args[i] = new DBxConst(i + 1);
      }
      DBxFunction sut = new DBxFunction(DBxFunctionKind.Coalesce, args);
      DummyNamedValues dummy = new DummyNamedValues();
      object res = sut.GetValue(dummy);
      Assert.AreEqual(wantedRes, res);
    }

    #endregion

    #region Прочие методы

    [Test]
    public void GetColumnNames()
    {
      DBxColumn arg1 = new DBxColumn("F1");
      DBxColumn arg2 = new DBxColumn("F2");
      DBxFunction sut = new DBxFunction(DBxFunctionKind.Add, arg1, arg2);
      DBxColumnList lst = new DBxColumnList();
      sut.GetColumnNames(lst);
      Assert.AreEqual("F1,F2", lst.AsString);
    }

    [Test]
    public void SetColumnNamePrefix()
    {
      DBxColumn arg1 = new DBxColumn("F1");
      DBxColumn arg2 = new DBxColumn("F2");
      DBxFunction sut = new DBxFunction(DBxFunctionKind.Add, arg1, arg2);
      DBxFunction res = (DBxFunction)(sut.SetColumnNamePrefix("ABC"));
      Assert.AreEqual(DBxFunctionKind.Add, res.Function, "Function");
      DBxColumn resCol1 = (DBxColumn)(res.Arguments[0]);
      DBxColumn resCol2 = (DBxColumn)(res.Arguments[1]);
      Assert.AreEqual("ABCF1", resCol1.ColumnName, "ColumnName [0]");
      Assert.AreEqual("ABCF2", resCol2.ColumnName, "ColumnName [1]");
    }

    // GetConst() не проверяем, так как метод пока возвращает null

    [Test]
    public void Equals()
    {
      DBxColumn arg11 = new DBxColumn("F1");
      DBxColumn arg12 = new DBxColumn("F2");
      DBxFunction sut = new DBxFunction(DBxFunctionKind.Add, arg11, arg12);

      DBxColumn arg21 = new DBxColumn("F1");
      DBxColumn arg22 = new DBxColumn("F2");
      DBxFunction res1 = new DBxFunction(DBxFunctionKind.Add, arg21, arg22);
      Assert.IsTrue(res1 == sut, "Equal");

      DBxFunction res2 = new DBxFunction(DBxFunctionKind.Substract, arg21, arg22);
      Assert.IsFalse(res2 == sut, "Different function kind");

      DBxColumn arg23 = new DBxColumn("F3");
      DBxFunction res3 = new DBxFunction(DBxFunctionKind.Add, arg21, arg23);
      Assert.IsFalse(res3 == sut, "Different argument");
    }

    [Test]
    public void GetAllExpressions()
    {
      DBxColumn arg1 = new DBxColumn("F1");
      DBxColumn arg2 = new DBxColumn("F2");
      DBxFunction arg21 = new DBxFunction(DBxFunctionKind.Multiply, arg1, arg2);
      DBxColumn arg3 = new DBxColumn("F3");
      DBxColumn arg4 = new DBxColumn("F4");
      DBxFunction arg34 = new DBxFunction(DBxFunctionKind.Multiply, arg3, arg4);

      DBxFunction sut = new DBxFunction(DBxFunctionKind.Add, arg21, arg34);
      List<DBxExpression> lst = new List<DBxExpression>();
      sut.GetAllExpressions(lst);
      CollectionAssert.AreEqual(new DBxExpression[] { sut, arg21, arg1, arg2, arg34, arg3, arg4 }, lst);
    }

    #endregion

    #region Сериализация

    [Test]
    public void Serialization()
    {
      DBxColumn arg1 = new DBxColumn("F1");
      DBxConst arg2 = new DBxConst(2);
      DBxConst arg3 = new DBxConst(3);
      DBxFunction sut = new DBxFunction(DBxFunctionKind.Substring, arg1, arg2, arg3);
      byte[] b = SerializationTools.SerializeBinary(sut);
      DBxFunction res = (DBxFunction)(SerializationTools.DeserializeBinary(b));
      Assert.AreEqual(DBxFunctionKind.Substring, res.Function, "Function");
      Assert.AreEqual(3, res.Arguments.Length, "Arguments");
      Assert.AreEqual("F1", ((DBxColumn)(res.Arguments[0])).ColumnName, "ColumnName[0]");
      Assert.AreEqual(sut.ToString(), res.ToString(), "ToString()");
    }

    #endregion
  }
}
