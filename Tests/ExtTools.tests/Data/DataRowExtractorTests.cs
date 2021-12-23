using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using System.Data;

namespace ExtTools_tests.Data
{
  /// <summary>
  /// Тесты для структур DataRowXXXExtractor
  /// </summary>
  [TestFixture]
  public class DataRowExtractorTests
  {
    #region Методы для структур разных типов

    [Test]
    public void DataRowIntExtractor()
    {
      DoTest<int, int>(new DataRowIntExtractor("F1"), 1, 2, 0);
      DoTest<float, int>(new DataRowIntExtractor("F1"), 1f, 2f, 0);
      DoTest<long, int>(new DataRowIntExtractor("F1"), 1L, 2L, 0);
    }

    [Test]
    public void DataRowNullableIntExtractor()
    {
      DoTest<int, int?>(new DataRowNullableIntExtractor("F1"), 1, 2, null);
      DoTest<float, int?>(new DataRowNullableIntExtractor("F1"), 1f, 2f, null);
    }


    [Test]
    public void DataRowInt64Extractor()
    {
      DoTest<long, long>(new DataRowInt64Extractor("F1"), 1L, 2L, 0L);
      DoTest<int, long>(new DataRowInt64Extractor("F1"), 1, 2, 0L);
      DoTest<float, long>(new DataRowInt64Extractor("F1"), 1f, 2f, 0L);
    }

    [Test]
    public void DataRowNullableInt64Extractor()
    {
      DoTest<long, long?>(new DataRowNullableInt64Extractor("F1"), 1L, 2L, null);
      DoTest<float, long?>(new DataRowNullableInt64Extractor("F1"), 1f, 2f, null);
    }


    [Test]
    public void DataRowSingleExtractor()
    {
      DoTest<float, float>(new DataRowSingleExtractor("F1"), 1f, 2f, 0f);
      DoTest<int, float>(new DataRowSingleExtractor("F1"), 1, 2, 0f);
    }

    [Test]
    public void DataRowNullableSingleExtractor()
    {
      DoTest<float, float?>(new DataRowNullableSingleExtractor("F1"), 1f, 2f, null);
      DoTest<int, float?>(new DataRowNullableSingleExtractor("F1"), 1, 2, null);
    }


    [Test]
    public void DataRowDoubleExtractor()
    {
      DoTest<double, double>(new DataRowDoubleExtractor("F1"), 1.0, 2.0, 0.0);
      DoTest<int, double>(new DataRowDoubleExtractor("F1"), 1, 2, 0.0);
    }

    [Test]
    public void DataRowNullableDoubleExtractor()
    {
      DoTest<double, double?>(new DataRowNullableDoubleExtractor("F1"), 1.0, 2.0, null);
      DoTest<int, double?>(new DataRowNullableDoubleExtractor("F1"), 1, 2, null);
    }


    [Test]
    public void DataRowDecimalExtractor()
    {
      DoTest<decimal, decimal>(new DataRowDecimalExtractor("F1"), 1m, 2m, 0m);
      DoTest<int, decimal>(new DataRowDecimalExtractor("F1"), 1, 2, 0m);
    }

    [Test]
    public void DataRowNullableDecimalExtractor()
    {
      DoTest<decimal, decimal?>(new DataRowNullableDecimalExtractor("F1"), 1m, 2m, null);
      DoTest<int, decimal?>(new DataRowNullableDecimalExtractor("F1"), 1, 2, null);
    }


    [Test]
    public void DataRowDateTimeExtractor()
    {
      DoTest<DateTime, DateTime>(new DataRowDateTimeExtractor("F1"), new DateTime(2021, 12, 20), new DateTime(2021, 12, 21), DateTime.MinValue);
    }

    [Test]
    public void DataRowNullableDateTimeExtractor()
    {
      DoTest<DateTime, DateTime?>(new DataRowNullableDateTimeExtractor("F1"), new DateTime(2021, 12, 20), new DateTime(2021, 12, 21), null);
    }


    [Test]
    public void DataRowTimeSpanExtractor()
    {
      DoTest<TimeSpan, TimeSpan>(new DataRowTimeSpanExtractor("F1"), new TimeSpan(1, 2, 3), new TimeSpan(4, 5, 6), TimeSpan.Zero);
    }

    [Test]
    public void DataRowNullableTimeSpanExtractor()
    {
      DoTest<TimeSpan, TimeSpan?>(new DataRowNullableTimeSpanExtractor("F1"), new TimeSpan(1, 2, 3), new TimeSpan(4, 5, 6), null);
    }


    [Test]
    public void DataRowStringExtractor()
    {
      DoTest<string, string>(new DataRowStringExtractor("F1"), "AAA", "BBB", String.Empty);
    }


    [Test]
    public void DataRowBoolExtractor()
    {
      DoTest<bool, bool>(new DataRowBoolExtractor("F1"), true, false, false);
    }

    [Test]
    public void DataRowNullableBoolExtractor()
    {
      DoTest<bool, bool?>(new DataRowNullableBoolExtractor("F1"), true, false, null);
    }


    [Test]
    public void DataRowGuidExtractor()
    {
      DoTest<Guid, Guid>(new DataRowGuidExtractor("F1"), Creators.Row1.VGuid, Creators.Row2.VGuid, Guid.Empty);
    }

    [Test]
    public void DataRowNullableGuidExtractor()
    {
      DoTest<Guid, Guid?>(new DataRowNullableGuidExtractor("F1"), Creators.Row1.VGuid, Creators.Row2.VGuid, null);
    }


    [Test]
    public void DataRowEnumExtractor()
    {
      DoTest<int, Creators.TestEnum>(new DataRowEnumExtractor<Creators.TestEnum>("F1"), Creators.TestEnum.One, Creators.TestEnum.Two, Creators.TestEnum.Zero);
    }

    [Test]
    public void DataRowNullableEnumExtractor()
    {
      DoTest<int, Creators.TestEnum?>(new DataRowNullableEnumExtractor<Creators.TestEnum>("F1"), Creators.TestEnum.One, Creators.TestEnum.Two, null);
    }

    #endregion

    #region Реализация теста

    /// <summary>
    /// Выполнение теста.
    /// Создает две таблицы с разной струкурой и несколькими строками. В таблице имеется тестируемое поле "F1".
    /// Выполняет извлечение данных, в том числе, с переключением между таблицами
    /// </summary>
    /// <typeparam name="TData">Тип данных DataColumn.DataType, который храниться в таблице данных</typeparam>
    /// <typeparam name="TRes">Тип извлекаемых данных. В частности, может быть nullable-тип и/или тип, к которому может быть выполнено преобразование</typeparam>
    /// <param name="sut">Тестируемый объект</param>
    /// <param name="value1">Первое тестовое значение для размещения в таблице</param>
    /// <param name="value2">Второе тестовое значение для размещения в таблице</param>
    /// <param name="defaultRes">Значение по умолчанию, которое извлекается, когда в поле находится DBNull.
    /// Оно не обязано совпадать с default(T).</param>
    private static void DoTest<TData, TRes>(IDataRowExtractor<TRes> sut, object value1, object value2, TRes defaultRes)
    {
      DataTable t1 = new DataTable("T1");
      t1.Columns.Add("F1", typeof(TData));

      DataTable t2 = new DataTable("T2");
      t2.Columns.Add("Dummy", typeof(int));
      t2.Columns.Add("F1", typeof(TData)); // важно проверить с изменившейся позицией столбца

      DataTable t3 = new DataTable("T3"); // Таблица без требуемого поля
      t3.Columns.Add("Dummy", typeof(int));


      t1.Rows.Add(value1);
      t1.Rows.Add(DBNull.Value);
      t1.Rows.Add(value2);

      t2.Rows.Add(DBNull.Value, value1);
      t2.Rows.Add(DBNull.Value, value2);

      t3.Rows.Add();

      Assert.AreEqual("F1", sut.ColumnName, sut.GetType().ToString() + " - " + typeof(TData).ToString() + " ColumnName");

      Assert.AreEqual(value1, sut[t1.Rows[0]], sut.GetType().ToString() + " - " + typeof(TData).ToString() + " #1");
      Assert.AreEqual(defaultRes, sut[t1.Rows[1]], sut.GetType().ToString() + " - " + typeof(TData).ToString() + " #2");
      Assert.AreEqual(value2, sut[t1.Rows[2]], sut.GetType().ToString() + " - " + typeof(TData).ToString() + " #3");

      Assert.AreEqual(value1, sut[t2.Rows[0]], sut.GetType().ToString() + " - " + typeof(TData).ToString() + " #4");
      Assert.AreEqual(value2, sut[t2.Rows[1]], sut.GetType().ToString() + " - " + typeof(TData).ToString() + " #5");

      TRes dummyRes;
      Assert.Catch(delegate() { dummyRes = sut[t3.Rows[0]]; }, sut.GetType().ToString() + " - " + typeof(TData).ToString() + " #6");
    }

    #endregion
  }
}
