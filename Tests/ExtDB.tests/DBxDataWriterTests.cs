using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.Data.SQLite;
using System.Data;
using FreeLibSet.Data.SqlClient;

namespace ExtDB_tests.Data
{
  /// <summary>
  /// Базовый класс, в котором объявлены методы тестов и структура таблицы данных.
  /// Наследуются реализации для конкретных баз данных и реализаций DBxDataWriter
  /// </summary>
  [TestFixture]
  public abstract class DBxDataWriterTestsBase
  {
    #region Конструктор

    public DBxDataWriterTestsBase()
    {
      _TestStruct = new DBxStruct();
      DBxTableStruct ts = _TestStruct.Tables.Add("Test1");
      ts.Columns.AddId(); // Первичный ключ, не используется для поиска записей
      ts.Columns.AddString("F1", 10, false);
      ts.Columns.AddInt("F2", false);
      ts.Columns.AddString("F3", 10, true);
      ts.Columns.AddInt("F4", true);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Структура базы данных
    /// </summary>
    protected DBxStruct TestStruct { get { return _TestStruct; } }
    private DBxStruct _TestStruct;

    #endregion

    #region Абстрактные методы

    protected abstract DBxConBase CreateCon();

    protected virtual DBxDataWriter CreateWriter(DBxConBase con, DBxDataWriterInfo writerInfo)
    {
      return con.CreateWriter(writerInfo);
    }

    #endregion

    #region Калькулятор для тестирования

    /// <summary>
    /// Чтобы тесты имели смысл, должно обрабатываться, в том числе, большое количество строк.
    /// Сложно проверять результаты вручную, код проверки становится малопонятным.
    /// Этот класс выполняет расчеты с помощью DataView на автономной таблице данных
    /// </summary>
    private class TestCalculator
    {
      #region Конструктор

      public TestCalculator(DBxDataWriterTestsBase owner)
      {
        _Owner = owner;
        _Table = new DataTable();
        _Table.TableName = owner.GetTestTableName("Test1");
        _Table.Columns.Add("F1", typeof(string));
        _Table.Columns.Add("F2", typeof(int));
        _Table.Columns.Add("F3", typeof(string));
        _Table.Columns.Add("F4", typeof(int));
      }

      #endregion

      #region Свойства 

      private DBxDataWriterTestsBase _Owner;

      /// <summary>
      /// Тестовая таблица со столбцами F1, F2, F3, F4
      /// </summary>
      public DataTable Table { get { return _Table; } }
      private DataTable _Table;

      #endregion

      #region Методы

      /// <summary>
      /// Выполняет действие для одной строки.
      /// Вызывается перед <see cref="DBxDataWriter.Write"/>.
      /// </summary>
      /// <param name="sut">Тестируемый объект</param>
      public void ApplyRow(DBxDataWriter sut)
      {
        DoApplyRow(sut, new object[] { sut[0], sut[1], sut[2], sut[3] });
      }

      public void ApplyRow(DBxDataWriter sut, DataRow row)
      {
        DoApplyRow(sut, row.ItemArray);
      }

      private void DoApplyRow(DBxDataWriter sut, object[]values)
      {
        if (values.Length != 4)
          throw new BugException();

        if (sut.WriterInfo.Mode != DBxDataWriterMode.Insert)
        {
          _Table.DefaultView.Sort = sut.SearchColumns.AsString;
          object[] keys = new object[sut.SearchColumns.Count];
          for (int i = 0; i < keys.Length; i++)
            keys[i] = GetValue(values[sut.SearchColumnPositions[i]]);
          int p = _Table.DefaultView.Find(keys);
          if (p >= 0)
          {
            DataRow row = _Table.DefaultView[p].Row;
            for (int i = 0; i < sut.WriterInfo.Columns.Count; i++)
            {
              if (sut.WriterInfo.SearchColumns.Contains(sut.WriterInfo.Columns[i]))
                continue;
              row[sut.WriterInfo.Columns[i]] = GetValue(values[i]);
            }
            return;
          }
          if (sut.WriterInfo.Mode == DBxDataWriterMode.Update)
            return;
        }

        _Table.Rows.Add(GetValue(values[0]), GetValue(values[1]), GetValue(values[2]), GetValue(values[3]));
      }

      private static object GetValue(object v)
      {
        if (v == null)
          return DBNull.Value;
        else
          return v;
      }

      public void TestResults(DBxConBase con)
      {
        DataTable res = con.FillSelect(_Owner.GetTestTableName("Test1"), new DBxColumns("F1,F2,F3,F4"), null, DBxOrder.FromDataViewSort("Id"));
        Assert.AreEqual(Table.Rows.Count, res.Rows.Count, "RowCount");
        for (int i = 0; i < Table.Rows.Count; i++)
        {
          Assert.AreEqual(Table.Rows[i]["F1"], res.Rows[i]["F1"], "Row #" + (i + 1).ToString() + ", F1");
          Assert.AreEqual(Table.Rows[i]["F2"], res.Rows[i]["F2"], "Row #" + (i + 1).ToString() + ", F2");
          Assert.AreEqual(Table.Rows[i]["F3"], res.Rows[i]["F3"], "Row #" + (i + 1).ToString() + ", F3");
          Assert.AreEqual(Table.Rows[i]["F4"], res.Rows[i]["F4"], "Row #" + (i + 1).ToString() + ", F4");
        }
      }

      #endregion
    }

    /// <summary>
    /// Получить имя тестовой таблицы "Test1",  ...
    /// Переопределяется для временных таблиц MS SQL Server.
    /// </summary>
    /// <param name="tableName">"Test1", "Test2", ...</param>
    /// <returns>Скорректированное имя таблицы</returns>
    internal virtual string GetTestTableName(string tableName)
    {
      return tableName;
    }

    #endregion

    #region Constructor

    [Test]
    public void Constructor()
    {
      using (DBxConBase con = CreateCon())
      {
        DBxDataWriterInfo writerInfo = new DBxDataWriterInfo();
        writerInfo.TableName = GetTestTableName("Test1");
        writerInfo.Mode = DBxDataWriterMode.Update;
        writerInfo.Columns = new DBxColumns("F1,F2,F3,F4");
        writerInfo.SearchColumns = new DBxColumns("F1,F2");
        DBxDataWriter sut = null;
        using (sut = CreateWriter(con, writerInfo))
        {
          Assert.IsTrue(writerInfo.IsReadOnly, "DBxDataWriterInfo.IsReadOnly");

          Assert.AreSame(writerInfo, sut.WriterInfo, "WriterInfo");
          //Assert.AreSame(con, sut.Connection, "Connection");
          Assert.AreEqual(DBxDataWriterState.Created, sut.State, "State #1");
          Assert.IsFalse(sut.IsDisposed, "IsDisposed #1");

          Assert.AreSame(TestStruct.Tables["Test1"], sut.TableStruct, "TableStruct");
          CollectionAssert.AreEqual(new DBxColumnStruct[4] {
             TestStruct.Tables["Test1"] .Columns[1],
             TestStruct.Tables["Test1"] .Columns[2],
             TestStruct.Tables["Test1"] .Columns[3],
             TestStruct.Tables["Test1"] .Columns[4]
          }, sut.ColumnDefs, "ColumnDefs");

          Assert.AreEqual("F1,F2", sut.SearchColumns.AsString, "SearchColumns");
          Assert.AreEqual("F3,F4", sut.OtherColumns.AsString, "OtherColumns");
          Assert.AreEqual(4, sut.Values.Length, "Values");
        }

        Assert.AreEqual(DBxDataWriterState.Disposed, sut.State, "State #2");
        Assert.IsTrue(sut.IsDisposed, "IsDisposed #2");
      }
    }

    #endregion

    #region Insert

    [TestCase(100, 0, false, 0)]
    [TestCase(111, 10, false, 0)]
    [TestCase(100, 10, false, 20, Description = "TransactionPulseRowCount is a divisor")]
    [TestCase(100, 10, false, 21, Description = "TransactionPulseRowCount is not a divisor")]
    [TestCase(100, 0, true, 0)]
    public void Insert(int recordCount, int dummyRows, bool useExpectedRowCount, int transactionPulseRowCount)
    {
      TestCalculator calc = new TestCalculator(this);

      using (DBxConBase con = CreateCon())
      {
        con.DeleteAll(GetTestTableName("Test1"));
        Assert.AreEqual(0, con.GetRecordCount(GetTestTableName("Test1")), "RecordCount #1");

        for (int i = 0; i < dummyRows; i++)
          calc.Table.Rows.Add("XXX", i, "XXX", i);
        con.AddRecords(calc.Table);
        Assert.AreEqual(dummyRows, con.GetRecordCount(GetTestTableName("Test1")), "RecordCount #2");

        DBxDataWriterInfo writerInfo = new DBxDataWriterInfo();
        writerInfo.TableName = GetTestTableName("Test1");
        writerInfo.Mode = DBxDataWriterMode.Insert;
        writerInfo.Columns = new DBxColumns("F1,F2,F3,F4");
        if (useExpectedRowCount)
          writerInfo.ExpectedRowCount = recordCount;
        writerInfo.TransactionPulseRowCount = transactionPulseRowCount;
        DBxDataWriter sut = null;
        using (sut = CreateWriter(con, writerInfo))
        {
          Assert.AreEqual(DBxDataWriterState.Created, sut.State, "State #1");

          for (int i = 0; i < recordCount; i++)
          {
            CollectionAssert.AreEqual(new object[4], sut.Values, "Values #1");

            sut.SetString("F1", "X" + StdConvert.ToString(i));
            sut.SetInt("F2", 10000 + i);
            sut.SetString("F3", "Y" + StdConvert.ToString(i));
            sut.SetInt("F4", 20000 + i);
            CollectionAssert.AreEqual(new object[4] {
              "X" + StdConvert.ToString(i),
              10000+i,
              "Y" + StdConvert.ToString(i),
              20000+i
            }, sut.Values, "Values #2");

            calc.ApplyRow(sut);
            sut.Write();

            Assert.AreEqual(DBxDataWriterState.Writing, sut.State, "State #2");
          }

          sut.Finish();
          Assert.AreEqual(DBxDataWriterState.Finished, sut.State, "State #3");
        }
        Assert.AreEqual(DBxDataWriterState.Disposed, sut.State, "State #4");
      }

      using (DBxConBase con = CreateCon())
      {
        calc.TestResults(con);
      }
    }

    #endregion

    #region Update

    [TestCase(100, false, 0, Description = "All the changes are appliable")]
    [TestCase(600, false, 0, Description = "Some changes are appliable not found")]
    [TestCase(600, true, 0, Description = "ExpectedRowCount")]
    [TestCase(200, false, 100, Description = "TransactionPulseRowCount #1")]
    [TestCase(200, false, 99, Description = "TransactionPulseRowCount #2")]
    public void Update(int recordCount, bool useExpectedRowCount, int transactionPulseRowCount)
    {
      TestCalculator calc = new TestCalculator(this);
      using (DBxConBase con = CreateCon())
      {
        con.DeleteAll(GetTestTableName("Test1"));
        for (int i = 1; i <= 1000; i++)
          calc.Table.Rows.Add("X" + StdConvert.ToString(i), 10000 + i, "Y" + StdConvert.ToString(i), 20000 + i);
        con.AddRecords(calc.Table);
        Assert.AreEqual(1000, con.GetRecordCount(GetTestTableName("Test1")), "RecordCount #1");

        DBxDataWriterInfo writerInfo = new DBxDataWriterInfo();
        writerInfo.TableName = GetTestTableName("Test1");
        writerInfo.Mode = DBxDataWriterMode.Update;
        writerInfo.Columns = new DBxColumns("F1,F2,F3,F4");
        writerInfo.SearchColumns = new DBxColumns("F1,F2");
        if (useExpectedRowCount)
          writerInfo.ExpectedRowCount = recordCount;
        writerInfo.TransactionPulseRowCount = transactionPulseRowCount;
        DBxDataWriter sut = null;
        using (sut = CreateWriter(con, writerInfo))
        {
          for (int i = 0; i < recordCount; i++)
          {
            // Будем менять только четные записи
            int i2 = i * 2;
            // Поисковые ключи
            sut.SetString("F1", "X" + StdConvert.ToString(i2));
            sut.SetInt("F2", 10000 + i2);
            // Изменяемые данные
            sut.SetString("F3", "Z" + StdConvert.ToString(i2));
            sut.SetInt("F4", 12345);
            calc.ApplyRow(sut);
            sut.Write();
          }

          sut.Finish();
        }
      }

      using (DBxConBase con = CreateCon())
      {
        calc.TestResults(con);
      }
    }

    #endregion

    #region InsertOrUpdate

    [TestCase(100, false, 0)]
    [TestCase(2000, false, 0)]
    public void InsertOrUpdate(int recordCount, bool useExpectedRowCount, int transactionPulseRowCount)
    {
      TestCalculator calc = new TestCalculator(this);
      using (DBxConBase con = CreateCon())
      {
        con.DeleteAll(GetTestTableName("Test1"));
        for (int i = 1; i <= 1000; i++)
          calc.Table.Rows.Add("X" + StdConvert.ToString(i), 10000 + i, "Y" + StdConvert.ToString(i), 20000 + i);
        con.AddRecords(calc.Table);
        Assert.AreEqual(1000, con.GetRecordCount(GetTestTableName("Test1")), "RecordCount #1");

        DBxDataWriterInfo writerInfo = new DBxDataWriterInfo();
        writerInfo.TableName = GetTestTableName("Test1");
        writerInfo.Mode = DBxDataWriterMode.InsertOrUpdate;
        writerInfo.Columns = new DBxColumns("F1,F2,F3,F4");
        writerInfo.SearchColumns = new DBxColumns("F1,F2");
        if (useExpectedRowCount)
          writerInfo.ExpectedRowCount = recordCount;
        writerInfo.TransactionPulseRowCount = transactionPulseRowCount;
        DBxDataWriter sut = null;
        using (sut = CreateWriter(con, writerInfo))
        {
          for (int i = 1; i <= recordCount; i++)
          {
            int i2 = i * 2;
            // Поисковые ключи
            sut.SetString("F1", "X" + StdConvert.ToString(i2));
            sut.SetInt("F2", 10000 + i2);
            // Изменяемые данные
            sut.SetString("F3", "Z" + StdConvert.ToString(i2));
            sut.SetInt("F4", 12345);
            calc.ApplyRow(sut);
            sut.Write();
          }

          sut.Finish();
        }
      }

      using (DBxConBase con = CreateCon())
      {
        calc.TestResults(con);
      }
    }

    [TestCase(100, false, 0)]
    public void InsertOrUpdate_EmptyTable(int recordCount, bool useExpectedRowCount, int transactionPulseRowCount)
    {
      TestCalculator calc = new TestCalculator(this);
      using (DBxConBase con = CreateCon())
      {
        con.DeleteAll(GetTestTableName("Test1"));
        Assert.AreEqual(0, con.GetRecordCount(GetTestTableName("Test1")), "RecordCount #1");

        DBxDataWriterInfo writerInfo = new DBxDataWriterInfo();
        writerInfo.TableName = "Test1";
        writerInfo.Mode = DBxDataWriterMode.InsertOrUpdate;
        writerInfo.Columns = new DBxColumns("F1,F2,F3,F4");
        writerInfo.SearchColumns = new DBxColumns("F1,F2");
        if (useExpectedRowCount)
          writerInfo.ExpectedRowCount = recordCount;
        writerInfo.TransactionPulseRowCount = transactionPulseRowCount;
        DBxDataWriter sut = null;
        using (sut = CreateWriter(con, writerInfo))
        {
          for (int i = 1; i <= recordCount; i++)
          {
            int i2 = i * 2;
            // Поисковые ключи
            sut.SetString("F1", "X" + StdConvert.ToString(i2));
            sut.SetInt("F2", 10000 + i2);
            // Изменяемые данные
            sut.SetString("F3", "Z" + StdConvert.ToString(i2));
            sut.SetInt("F4", 12345);
            calc.ApplyRow(sut);
            sut.Write();
          }

          sut.Finish();
        }
      }

      using (DBxConBase con = CreateCon())
      {
        calc.TestResults(con);
      }
    }

    #endregion

    #region LoadFrom()

    [Test]
    public void LoadFrom_Insert([Values(false, true)]bool useDataReader)
    {
      TestCalculator calc = new TestCalculator(this);

      using (DBxConBase con = CreateCon())
      {
        con.DeleteAll(GetTestTableName("Test1"));
        Assert.AreEqual(0, con.GetRecordCount(GetTestTableName("Test1")), "RecordCount #1");

        #region Существующие строки в таблице

        for (int i = 0; i < 100; i++)
          calc.Table.Rows.Add("XXX", i, "XXX", i);
        con.AddRecords(calc.Table);
        Assert.AreEqual(100, con.GetRecordCount(GetTestTableName("Test1")), "RecordCount #2");

        #endregion

        DBxDataWriterInfo writerInfo = new DBxDataWriterInfo();
        writerInfo.TableName = GetTestTableName("Test1");
        writerInfo.Mode = DBxDataWriterMode.Insert;
        writerInfo.Columns = new DBxColumns("F1,F2,F3,F4");
        DBxDataWriter sut = null;
        using (sut = CreateWriter(con, writerInfo))
        {
          #region 1. Построчное добавление

          for (int i = 0; i < 500; i++)
          {
            sut.SetString("F1", "X" + StdConvert.ToString(i));
            sut.SetInt("F2", 10000 + i);
            sut.SetString("F3", "Y" + StdConvert.ToString(i));
            sut.SetInt("F4", 20000 + i);

            calc.ApplyRow(sut);
            sut.Write();
          }

          #endregion

          #region 2. Добавление таблицы

          DataTable srcTable = calc.Table.Clone();
          for (int i = 0; i < 700; i++)
          {
            DataRow row = srcTable.Rows.Add("X" + StdConvert.ToString(i),
              11000 + i,
              "Y" + StdConvert.ToString(i),
              21000 + i);

            calc.ApplyRow(sut, row);
          }

          if (useDataReader)
          {
            using (DataTableReader reader = srcTable.CreateDataReader())
            {
              sut.LoadFrom(reader);
            }
          }
          else
            sut.LoadFrom(srcTable);

          #endregion

          #region 3. Еще построчное добавление

          for (int i = 0; i < 500; i++)
          {
            sut.SetString("F1", "X" + StdConvert.ToString(i));
            sut.SetInt("F2", 12000 + i);
            sut.SetString("F3", "Y" + StdConvert.ToString(i));
            sut.SetInt("F4", 12000 + i);

            calc.ApplyRow(sut);
            sut.Write();
          }

          #endregion

          sut.Finish();
        }
      }

      using (DBxConBase con = CreateCon())
      {
        calc.TestResults(con);
      }
    }

    [Test]
    public void LoadFrom_Update_InsertOrUpdate([Values(DBxDataWriterMode.Update, DBxDataWriterMode.InsertOrUpdate)] DBxDataWriterMode mode,
    [Values(false, true)]bool useDataReader)
    {
      TestCalculator calc = new TestCalculator(this);
      using (DBxConBase con = CreateCon())
      {
        con.DeleteAll(GetTestTableName("Test1"));
        for (int i = 1; i <= 1000; i++)
          calc.Table.Rows.Add("X" + StdConvert.ToString(i), 10000 + i, "Y" + StdConvert.ToString(i), 20000 + i);
        con.AddRecords(calc.Table);
        Assert.AreEqual(1000, con.GetRecordCount(GetTestTableName("Test1")), "RecordCount #1");

        DBxDataWriterInfo writerInfo = new DBxDataWriterInfo();
        writerInfo.TableName = GetTestTableName("Test1");
        writerInfo.Mode = mode;
        writerInfo.Columns = new DBxColumns("F1,F2,F3,F4");
        writerInfo.SearchColumns = new DBxColumns("F1,F2");
        DBxDataWriter sut = null;
        using (sut = CreateWriter(con, writerInfo))
        {
          #region 1. Построчное выполнение

          for (int i = 0; i < 200; i++)
          {
            sut.SetString("F1", "X" + StdConvert.ToString(i*10));
            sut.SetInt("F2", 10000 + i*10);
            sut.SetString("F3", "Z" + StdConvert.ToString(i*10));
            sut.SetInt("F4", 11111);
            calc.ApplyRow(sut);
            sut.Write();
          }

          #endregion

          #region 2. Групповое выполнение

          DataTable srcTable = calc.Table.Clone();
          for (int i = 0; i < 700; i++)
          {
            DataRow row = srcTable.Rows.Add("X" + StdConvert.ToString(i+500),
              10000 + i,
              "Y" + StdConvert.ToString(i),
              33333);

            calc.ApplyRow(sut, row);
          }

          if (useDataReader)
          {
            using (DataTableReader reader = srcTable.CreateDataReader())
            {
              sut.LoadFrom(reader);
            }
          }
          else
            sut.LoadFrom(srcTable);

          #endregion

          #region 3. Еще построчное выполнение

          for (int i = 0; i < 200; i++)
          {
            sut.SetString("F1", "X" + StdConvert.ToString(i * 11));
            sut.SetInt("F2", 10000 + i * 10);
            sut.SetString("F3", "Z" + StdConvert.ToString(i * 11));
            sut.SetInt("F4", 33333);
            calc.ApplyRow(sut);
            sut.Write();
          }

          #endregion

          sut.Finish();
        }
      }

      using (DBxConBase con = CreateCon())
      {
        calc.TestResults(con);
      }
    }

    #endregion
  }
}

namespace ExtDB_tests.Data_SQLite
{
  /// <summary>
  /// SQLite с объектом записи по умолчанию
  /// </summary>
  public class DBxDataWriterTests_SQLite_DBxDefaultDataWriter : ExtDB_tests.Data.DBxDataWriterTestsBase
  {
    #region База данных в памяти

    [OneTimeSetUp]
    public void SetUp()
    {
      _DB = new SQLiteDBx();
      DBxStruct dbs = new DBxStruct();
      _DB.Struct = TestStruct;
      _DB.UpdateStruct();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
      if (_DB != null)
        _DB.Dispose();
    }

    private SQLiteDBx _DB;

    #endregion

    #region Переопределенные методы

    protected override DBxConBase CreateCon()
    {
      return _DB.MainEntry.CreateCon();
    }

    protected override DBxDataWriter CreateWriter(DBxConBase con, DBxDataWriterInfo writerInfo)
    {
      return new DBxDefaultDataWriter(con, writerInfo);
    }

    #endregion
  }

  /// <summary>
  /// SQLite с объектом записи по умолчанию
  /// </summary>
  public class DBxDataWriterTests_SQLite : ExtDB_tests.Data.DBxDataWriterTestsBase
  {
    #region База данных в памяти

    [OneTimeSetUp]
    public void SetUp()
    {
      _DB = new SQLiteDBx();
      DBxStruct dbs = new DBxStruct();
      _DB.Struct = TestStruct;
      _DB.UpdateStruct();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
      if (_DB != null)
        _DB.Dispose();
    }

    private SQLiteDBx _DB;

    #endregion

    #region Переопределенные методы

    protected override DBxConBase CreateCon()
    {
      return _DB.MainEntry.CreateCon();
    }

    #endregion
  }
}

#if XXX // TODO: Так не работает. Временная таблица существует только в пределах соединения

namespace ExtDB_tests.Data_SqlClient
{
  /// <summary>
  /// SQLite с объектом записи по умолчанию
  /// </summary>
  public class DBxDataWriterTests_MSSQL : ExtDB_tests.Data.DBxDataWriterTestsBase
  {
    #region База данных в памяти

    [OneTimeSetUp]
    public void SetUp()
    {
      _DB = CreateDB();

      _TestTableNames = new Dictionary<string, string>();

      using (SqlDBxCon con = (SqlDBxCon)CreateCon())
      {
        foreach (DBxTableStruct ts in TestStruct.Tables)
        {
          DBxTableStruct ts2 = ts.Clone();
          foreach (DBxColumnStruct col in ts2.Columns)
          {
            if (!String.IsNullOrEmpty(col.MasterTableName))
              col.MasterTableName = GetTestTableName(col.MasterTableName);
          }
          _TestTableNames.Add(ts.TableName, con.CreateTempTableInternal(ts2));
        }
      }
    }

    private SqlDBx CreateDB()
    {
      System.Data.SqlClient.SqlConnectionStringBuilder csb = new System.Data.SqlClient.SqlConnectionStringBuilder();
      csb.DataSource = @".\SQLEXPRESS";
      csb.IntegratedSecurity = true;
      csb.InitialCatalog = "master";
      return new SqlDBx(csb);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
      if (_DB != null)
        _DB.Dispose();
    }

    private SqlDBx _DB;

    #endregion

    #region Переопределенные методы

    internal override string GetTestTableName(string tableName)
    {
      return _TestTableNames[tableName];
    }
    private Dictionary<string, string> _TestTableNames;

    protected override DBxConBase CreateCon()
    {
      DBxConBase con = _DB.MainEntry.CreateCon();
      con.NameCheckingEnabled = false; // имя таблицы начинается с "#"
      return con;
    }

    #endregion
  }
}

#endif