// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

// DataTools - агрегатные функции Sum/Min/Max/MinMax/Average/GetRowCount()

namespace FreeLibSet.Core
{
  partial class DataTools
  {
    #region Суммирование полей и массивов

    #region Int32

    /// <summary>
    /// Получить сумму значений поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static int SumInt32(DataView dv, string columnName)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int s = 0;
      foreach (DataRowView drv in dv)
        checked { s += DataTools.GetInt32(drv.Row[p]); }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static int SumInt32(DataTable table, string columnName)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int s = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        checked { s += DataTools.GetInt32(row[p]); }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в коллекции строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static int SumInt32(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int s = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        checked { s += rowVals[columnName].AsInt32; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void SumInt32(DataRow totalRow, string columnName)
    {
      totalRow[columnName] = SumInt32(totalRow.Table, columnName);
    }

    /// <summary>
    /// Получить сумму элементов коллекции чисел
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static int SumInt32(IEnumerable<int> items)
    {
      if (items == null)
        return 0;

      int s = 0;
      foreach (int v in items)
        checked { s += v; }
      return s;
    }

    #endregion

    #region Int64

    /// <summary>
    /// Получить сумму значений поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static long SumInt64(DataView dv, string columnName)
    {
      if (dv == null)
        return 0L;
      if (dv.Count == 0)
        return 0L;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      long s = 0L;
      foreach (DataRowView drv in dv)
        checked { s += DataTools.GetInt64(drv.Row[p]); }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static long SumInt64(DataTable table, string columnName)
    {
      if (table == null)
        return 0L;
      if (table.Rows.Count == 0)
        return 0L;
      int p = GetColumnPosWithCheck(table, columnName);
      long s = 0L;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        checked { s += DataTools.GetInt64(row[p]); }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в коллекции строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static long SumInt64(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      long s = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        checked { s += rowVals[columnName].AsInt64; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void SumInt64(DataRow totalRow, string columnName)
    {
      totalRow[columnName] = SumInt64(totalRow.Table, columnName);
    }

    /// <summary>
    /// Получить сумму элементов коллекции чисел
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static long SumInt64(IEnumerable<long> items)
    {
      if (items == null)
        return 0;

      long s = 0;
      foreach (long v in items)
        checked { s += v; }
      return s;
    }

    #endregion

    #region Single

    /// <summary>
    /// Получить сумму значений поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static float SumSingle(DataView dv, string columnName)
    {
      if (dv == null)
        return 0f;
      if (dv.Count == 0)
        return 0f;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      float s = 0f;
      foreach (DataRowView drv in dv)
        checked { s += DataTools.GetSingle(drv.Row[p]); }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static float SumSingle(DataTable table, string columnName)
    {
      if (table == null)
        return 0f;
      if (table.Rows.Count == 0)
        return 0f;
      int p = GetColumnPosWithCheck(table, columnName);
      float s = 0f;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        checked { s += DataTools.GetSingle(row[p]); }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в коллекции строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static float SumSingle(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return 0f;
      // Строки могут относиться к разным таблицам
      DataRowValues values = new DataRowValues();
      float s = 0f;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        values.CurrentRow = row;
        checked { s += values[columnName].AsSingle; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void SumSingle(DataRow totalRow, string columnName)
    {
      totalRow[columnName] = SumSingle(totalRow.Table, columnName);
    }


    /// <summary>
    /// Получить сумму элементов коллекции чисел
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static float SumSingle(IEnumerable<float> items)
    {
      if (items == null)
        return 0f;

      float s = 0f;
      foreach (float v in items)
        checked { s += v; }
      return s;
    }

    #endregion

    #region Double

    /// <summary>
    /// Получить сумму значений поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static double SumDouble(DataView dv, string columnName)
    {
      if (dv == null)
        return 0.0;
      if (dv.Count == 0)
        return 0.0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      double s = 0.0;
      foreach (DataRowView drv in dv)
        checked { s += DataTools.GetDouble(drv.Row[p]); }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static double SumDouble(DataTable table, string columnName)
    {
      if (table == null)
        return 0.0;
      if (table.Rows.Count == 0)
        return 0.0;
      int p = GetColumnPosWithCheck(table, columnName);
      double s = 0.0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        checked { s += DataTools.GetDouble(row[p]); }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в коллекции строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static double SumDouble(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return 0.0;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      double s = 0.0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        checked { s += rowVals[columnName].AsDouble; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void SumDouble(DataRow totalRow, string columnName)
    {
      totalRow[columnName] = SumDouble(totalRow.Table, columnName);
    }

    /// <summary>
    /// Получить сумму элементов коллекции чисел
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static double SumDouble(IEnumerable<double> items)
    {
      if (items == null)
        return 0.0;

      double s = 0;
      foreach (double v in items)
        checked { s += v; }
      return s;
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Получить сумму значений поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static decimal SumDecimal(DataView dv, string columnName)
    {
      if (dv == null)
        return 0m;
      if (dv.Count == 0)
        return 0m;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      decimal s = 0m;
      foreach (DataRowView drv in dv)
        checked { s += DataTools.GetDecimal(drv.Row[p]); }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static decimal SumDecimal(DataTable table, string columnName)
    {
      if (table == null)
        return 0m;
      if (table.Rows.Count == 0)
        return 0m;
      int p = GetColumnPosWithCheck(table, columnName);
      decimal s = 0m;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        checked { s += DataTools.GetDecimal(row[p]); }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в коллекции строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static decimal SumDecimal(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return 0m;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      decimal s = 0m;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        checked { s += rowVals[columnName].AsDecimal; }
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="row">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void SumDecimal(DataRow row, string columnName)
    {
      row[columnName] = SumDecimal(row.Table, columnName);
    }

    /// <summary>
    /// Получить сумму элементов двумерного массива чисел
    /// </summary>
    /// <param name="items">Массив или коллекция(может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static decimal SumDecimal(IEnumerable<decimal> items)
    {
      if (items == null)
        return 0m;

      decimal s = 0m;
      foreach (decimal v in items)
        checked { s += v; }
      return s;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Получить сумму значений поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static TimeSpan SumTimeSpan(DataView dv, string columnName)
    {
      if (dv == null)
        return TimeSpan.Zero;
      if (dv.Count == 0)
        return TimeSpan.Zero;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      TimeSpan s = TimeSpan.Zero;
      foreach (DataRowView drv in dv)
        s += DataTools.GetTimeSpan(drv.Row[p]);
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static TimeSpan SumTimeSpan(DataTable table, string columnName)
    {
      if (table == null)
        return TimeSpan.Zero;
      if (table.Rows.Count == 0)
        return TimeSpan.Zero;
      int p = GetColumnPosWithCheck(table, columnName);
      TimeSpan s = TimeSpan.Zero;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        s += DataTools.GetTimeSpan(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в коллекции строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static TimeSpan SumTimeSpan(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return TimeSpan.Zero;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      TimeSpan s = TimeSpan.Zero;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        s += rowVals[columnName].AsTimeSpan;
      }
      return s;
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="row">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void SumTimeSpan(DataRow row, string columnName)
    {
      row[columnName] = SumTimeSpan(row.Table, columnName);
    }

    /// <summary>
    /// Получить сумму элементов коллекции чисел
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static TimeSpan SumTimeSpan(IEnumerable<TimeSpan> items)
    {
      if (items == null)
        return TimeSpan.Zero;

      TimeSpan s = TimeSpan.Zero;
      foreach (TimeSpan v in items)
        s += v;
      return s;
    }

    #endregion

    #region Внутренние функции для методов XxxValue

    /// <summary>
    /// Получить тип данных для столбца в массиве строк.
    /// Извлекается DataType для столбца в первой строке массива, в которой хранится не ссылка null.
    /// Если массив пустой или содержит только ссылки null, возвращается null
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="column">Сюда помещается столбец, откуда взято описание</param>
    /// <returns>Свойство DataColumn.DataType</returns>
    private static Type GetDataTypeFromRows(IEnumerable<DataRow> rows, string columnName, out DataColumn column)
    {
      column = null;
      if (rows == null)
        return null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        column = row.Table.Columns[columnName];
        if (column == null)
          throw ExceptionFactory.ArgUnknownColumnName("columnName", row.Table, columnName);
        return column.DataType;
      }
      return null; // Нет ни одной строки
    }

    /// <summary>
    /// Возвращает индекс столбца в таблице.
    /// Если столбца нет, генерируется исключение.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Индекс столбца</returns>
    internal static int GetColumnPosWithCheck(DataTable table, string columnName)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif

      int p = table.Columns.IndexOf(columnName);
      if (p < 0)
      {
        ArgumentException e = ExceptionFactory.ArgUnknownColumnName("columnName", table, columnName);
        AddExceptionColumnsInfo(e, table);
        throw e;
      }
      return p;
    }

    /// <summary>
    /// Возвращает индексы заданных столбцов в таблице.
    /// Если какого-либо столбца нет, генерируется исключение.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnNames">Список имен полей, разделенных запятыми</param>
    /// <returns></returns>
    private static int[] GetColumnPosArrayWithCheck(DataTable table, string columnNames)
    {
      if (String.IsNullOrEmpty(columnNames))
        return EmptyArray<Int32>.Empty;

      string[] aColumnNames = columnNames.Split(',');
      int[] aPos = new int[aColumnNames.Length];
      for (int i = 0; i < aColumnNames.Length; i++)
        aPos[i] = GetColumnPosWithCheck(table, aColumnNames[i]);
      return aPos;
    }

    /// <summary>
    /// Добавляет в исключение данные об имеющихся полях таблицы.
    /// </summary>
    /// <param name="e">Созданный объект исключения</param>
    /// <param name="table">Таблица данных</param>
    private static void AddExceptionColumnsInfo(ArgumentException e, DataTable table) // 15.07.2019
    {
      if (table == null)
        return;

      try
      {
        StringBuilder sb = new StringBuilder();
        sb.Append("AvailableColumns (");
        sb.Append(table.Columns.Count.ToString());
        sb.Append("): ");
        for (int i = 0; i < table.Columns.Count; i++)
        {
          if (i > 0)
            sb.Append(", ");
          sb.Append('\"');
          sb.Append(table.Columns[i].ColumnName);
          sb.Append('\"');
        }
        e.Data["Table.ColumnNamesInfo"] = sb.ToString();
      }
      catch { }
    }

    /// <summary>
    /// Используется методами SumValue(), MinValue(), MaxValue и AverageValue().
    /// Они могут получить перечислитель на jagged-массив, который надо преобразовать в нормальный
    /// </summary>
    /// <param name="items"></param>
    private static void CorrectAggregateEnumerable(ref System.Collections.IEnumerable items)
    {
      foreach (object vx in items)
      {
        if (vx == null)
          continue;

        if (vx is Array)
        {
          // Начинаем все заново
          List<object> lst = new List<object>();
          DoAddToAggregableList(lst, items);
          items = lst;
          return;
        }
      }
    }

    private static void DoAddToAggregableList(List<object> lst, System.Collections.IEnumerable items)
    {
      foreach (object vx in items)
      {
        if (vx == null)
          continue;

        if (vx is Array)
          // Рекурсивный вызов 
          DoAddToAggregableList(lst, (Array)vx);
        else
          lst.Add(vx);
      }
    }

    #endregion

    #region Любой тип данных

    /// <summary>
    /// Получить сумму значений поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static object SumValue(DataView dv, string columnName)
    {
      if (dv == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif

      DataColumn col = dv.Table.Columns[columnName];
      if (col == null)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", dv.Table, columnName);

      switch (MathTools.GetSumType(col.DataType))
      {
        case MathTools.SumType.Int32: return SumInt32(dv, columnName);
        case MathTools.SumType.Int64: return SumInt64(dv, columnName);
        case MathTools.SumType.Single: return SumSingle(dv, columnName);
        case MathTools.SumType.Double: return SumDouble(dv, columnName);
        case MathTools.SumType.Decimal: return SumDecimal(dv, columnName);
        case MathTools.SumType.TimeSpan: return SumTimeSpan(dv, columnName);
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static object SumValue(DataTable table, string columnName)
    {
      if (table == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif

      DataColumn col = table.Columns[columnName];
      if (col == null)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", table, columnName);

      switch (MathTools.GetSumType(col.DataType))
      {
        case MathTools.SumType.Int32: return SumInt32(table, columnName);
        case MathTools.SumType.Int64: return SumInt64(table, columnName);
        case MathTools.SumType.Single: return SumSingle(table, columnName);
        case MathTools.SumType.Double: return SumDouble(table, columnName);
        case MathTools.SumType.Decimal: return SumDecimal(table, columnName);
        case MathTools.SumType.TimeSpan: return SumTimeSpan(table, columnName);
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }

    /// <summary>
    /// Получить сумму значений поля для всех строк в коллекции строк
    /// </summary>
    /// <param name="rows">Массив строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Сумма значений для всех строк</returns>
    public static object SumValue(IEnumerable<DataRow> rows, string columnName)
    {
      DataColumn col;
      Type t = GetDataTypeFromRows(rows, columnName, out col);
      if (t == null)
        return null;

      switch (MathTools.GetSumType(t))
      {
        case MathTools.SumType.Int32: return SumInt32(rows, columnName);
        case MathTools.SumType.Int64: return SumInt64(rows, columnName);
        case MathTools.SumType.Single: return SumSingle(rows, columnName);
        case MathTools.SumType.Double: return SumDouble(rows, columnName);
        case MathTools.SumType.Decimal: return SumDecimal(rows, columnName);
        case MathTools.SumType.TimeSpan: return SumTimeSpan(rows, columnName);
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }


    /// <summary>
    /// Получить сумму значений поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом <see cref="DataTable.NewRow()"/>, но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Нулевые значения записываются без использования <see cref="DBNull"/>.
    /// </summary>
    /// <param name="row">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void SumValue(DataRow row, string columnName)
    {
#if DEBUG
      if (row == null)
        throw new ArgumentNullException("row");
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif

      DataColumn col = row.Table.Columns[columnName];
      if (col == null)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", row.Table, columnName); ;

      switch (MathTools.GetSumType(col.DataType))
      {
        case MathTools.SumType.Int32: SumInt32(row, columnName); break;
        case MathTools.SumType.Int64: SumInt64(row, columnName); break;
        case MathTools.SumType.Single: SumSingle(row, columnName); break;
        case MathTools.SumType.Double: SumDouble(row, columnName); break;
        case MathTools.SumType.Decimal: SumDecimal(row, columnName); break;
        case MathTools.SumType.TimeSpan: SumTimeSpan(row, columnName); break;
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }


    /// <summary>
    /// Получить сумму элементов одномерного массива чисел произольного типа.
    /// Выполняется преобразование к самому большому типу.
    /// Значения null пропускаются. Если массив пустой или содержит только null'ы, то
    /// возвращается null
    /// Если в массиве есть значения несовместимых типов (например, int и TimeSpan),
    /// генерируется исключение
    /// </summary>
    /// <param name="items">Коллекция или массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static object SumValue(System.Collections.IEnumerable items)
    {
      if (items == null)
        return null;

      CorrectAggregateEnumerable(ref items);

      object res = null;
      MathTools.SumType st = MathTools.SumType.None;
      foreach (object vx in items)
      {
        if (vx == null)
          continue;
        MathTools.SumType st2 = MathTools.GetSumType(vx.GetType());
        st = MathTools.GetLargestSumType(st, st2);
        res = MathTools.ConvertValue(res, st);
        object v = MathTools.ConvertValue(vx, st);
        checked
        {
          switch (st)
          {
            case MathTools.SumType.Int32: res = (int)res + (int)v; break;
            case MathTools.SumType.Int64: res = (long)res + (long)v; break;
            case MathTools.SumType.Single: res = (float)res + (float)v; break;
            case MathTools.SumType.Double: res = (double)res + (double)v; break;
            case MathTools.SumType.Decimal: res = (decimal)res + (decimal)v; break;
            case MathTools.SumType.TimeSpan: res = (TimeSpan)res + (TimeSpan)v; break;
            default:
              throw ExceptionFactory.ArgInvalidEnumerableType("items", items);
          }
        }
      }

      return res;
    }

    #endregion

    #endregion

    #region Минимальное значение

    #region Int32

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? MinInt32(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetInt32(drv.Row[p]));
        else
          s = DataTools.GetInt32(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? MinInt32(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      int? s = null;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetInt32(row[p]));
        else
          s = DataTools.GetInt32(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? MinInt32(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int? s = null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        int? v = rowVals[columnName].AsNullableInt32;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0);
        else
          s = v ?? 0;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MinInt32(DataRow totalRow, string columnName, bool skipNulls)
    {
      int? v = MinInt32(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить минимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция</returns>
    public static int? MinInt32(IEnumerable<int> items)
    {
      if (items == null)
        return null;

      int? s = null;
      foreach (int item in items)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, item);
        else
          s = item;
      }
      return s;
    }

    #endregion

    #region Int64

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? MinInt64(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      long? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetInt64(drv.Row[p]));
        else
          s = DataTools.GetInt64(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? MinInt64(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      long? s = null;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetInt64(row[p]));
        else
          s = DataTools.GetInt64(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? MinInt64(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      long? s = null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        long? v = rowVals[columnName].AsNullableInt64;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0L);
        else
          s = v ?? 0L;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MinInt64(DataRow totalRow, string columnName, bool skipNulls)
    {
      long? v = MinInt64(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить минимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция</returns>
    public static long? MinInt64(IEnumerable<long> items)
    {
      if (items == null)
        return null;

      long? s = null;
      foreach (long item in items)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, item);
        else
          s = item;
      }
      return s;
    }

    #endregion

    #region Single

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? MinSingle(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      float? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetSingle(drv.Row[p]));
        else
          s = DataTools.GetSingle(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? MinSingle(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      float? s = null;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetSingle(row[p]));
        else
          s = DataTools.GetSingle(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? MinSingle(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      float? s = null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        float? v = rowVals[columnName].AsNullableSingle;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0f);
        else
          s = v ?? 0f;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MinSingle(DataRow totalRow, string columnName, bool skipNulls)
    {
      float? v = MinSingle(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить минимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция</returns>
    public static float? MinSingle(IEnumerable<float> items)
    {
      if (items == null)
        return null;

      float? s = null;
      foreach (float item in items)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, item);
        else
          s = item;
      }
      return s;
    }

    #endregion

    #region Double

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? MinDouble(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      double? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetDouble(drv.Row[p]));
        else
          s = DataTools.GetDouble(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? MinDouble(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      double? s = null;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetDouble(row[p]));
        else
          s = DataTools.GetDouble(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? MinDouble(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      double? s = null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        double? v = rowVals[columnName].AsNullableDouble;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0.0);
        else
          s = v ?? 0.0;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MinDouble(DataRow totalRow, string columnName, bool skipNulls)
    {
      double? v = MinDouble(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить минимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция</returns>
    public static double? MinDouble(IEnumerable<double> items)
    {
      if (items == null)
        return null;

      double? s = null;
      foreach (double item in items)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, item);
        else
          s = item;
      }
      return s;
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? MinDecimal(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      decimal? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetDecimal(drv.Row[p]));
        else
          s = DataTools.GetDecimal(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? MinDecimal(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      decimal? s = null;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, DataTools.GetDecimal(row[p]));
        else
          s = DataTools.GetDecimal(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? MinDecimal(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      decimal? s = null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        decimal? v = rowVals[columnName].AsNullableDecimal;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Min(s.Value, v ?? 0m);
        else
          s = v ?? 0m;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MinDecimal(DataRow totalRow, string columnName, bool skipNulls)
    {
      decimal? v = MinDecimal(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить минимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция</returns>
    public static decimal? MinDecimal(IEnumerable<decimal> items)
    {
      if (items == null)
        return null;

      decimal? s = null;
      foreach (decimal item in items)
      {
        if (s.HasValue)
          s = Math.Min(s.Value, item);
        else
          s = item;
      }
      return s;
    }

    #endregion

    #region DateTime

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime? MinDateTime(DataView dv, string columnName)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      DateTime? s = null;
      foreach (DataRowView drv in dv)
      {
        //if (SkipNulls)
        //{
        //  if (drv.Row.IsNull(p))
        //    continue;
        //}
        if (s.HasValue)
          s = TimeTools.Min(s.Value, DataTools.GetNullableDateTime(drv.Row[p]));
        else
          s = DataTools.GetNullableDateTime(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime? MinDateTime(DataTable table, string columnName)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      DateTime? s = null;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        //if (SkipNulls)
        //{
        //  if (Row.IsNull(p))
        //    continue;
        //}
        if (s.HasValue)
          s = TimeTools.Min(s.Value, DataTools.GetNullableDateTime(row[p]));
        else
          s = DataTools.GetNullableDateTime(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime? MinDateTime(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      DateTime? s = null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        DateTime? v = rowVals[columnName].AsNullableDateTime;
        //if (SkipNulls)
        //{
        //  if (!v.HasValue)
        //    continue;
        //}
        if (s.HasValue)
          s = TimeTools.Min(s.Value, v);
        else
          s = v;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="row">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void MinDateTime(DataRow row, string columnName)
    {
      DateTime? v = MinDateTime(row.Table, columnName);
      if (v.HasValue)
        row[columnName] = v.Value;
      else
        row[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить минимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция</returns>
    public static DateTime? MinDateTime(IEnumerable<DateTime> items)
    {
      if (items == null)
        return null;

      DateTime? s = null;
      foreach (DateTime item in items)
      {
        if (s.HasValue)
          s = TimeTools.Min(s.Value, item);
        else
          s = item;
      }
      return s;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? MinTimeSpan(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      TimeSpan? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = TimeTools.Min(s.Value, DataTools.GetTimeSpan(drv.Row[p]));
        else
          s = DataTools.GetTimeSpan(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? MinTimeSpan(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      TimeSpan? s = null;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = TimeTools.Min(s.Value, DataTools.GetTimeSpan(row[p]));
        else
          s = DataTools.GetTimeSpan(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? MinTimeSpan(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      TimeSpan? s = null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        TimeSpan? v = null;
        if (!rowVals[columnName].IsNull)
          v = rowVals[columnName].AsTimeSpan;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = TimeTools.Min(s.Value, v ?? TimeSpan.Zero);
        else
          s = v ?? TimeSpan.Zero;
      }
      return s;
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MinTimeSpan(DataRow totalRow, string columnName, bool skipNulls)
    {
      TimeSpan? v = MinTimeSpan(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить минимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Массив (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция</returns>
    public static TimeSpan? MinTimeSpan(IEnumerable<TimeSpan> items)
    {
      if (items == null)
        return null;

      TimeSpan? s = null;
      foreach (TimeSpan item in items)
      {
        if (s.HasValue)
          s = TimeTools.Min(s.Value, item);
        else
          s = item;
      }
      return s;
    }

    #endregion

    #region Любой тип данных

    /// <summary>
    /// Получить минимальное значение поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object MinValue(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif

      DataColumn col = dv.Table.Columns[columnName];
      if (col == null)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", dv.Table, columnName);

      switch (MathTools.GetSumType(col.DataType))
      {
        case MathTools.SumType.Int32: return MinInt32(dv, columnName, skipNulls);
        case MathTools.SumType.Int64: return MinInt64(dv, columnName, skipNulls);
        case MathTools.SumType.Single: return MinSingle(dv, columnName, skipNulls);
        case MathTools.SumType.Double: return MinDouble(dv, columnName, skipNulls);
        case MathTools.SumType.Decimal: return MinDecimal(dv, columnName, skipNulls);
        case MathTools.SumType.DateTime: return MinDateTime(dv, columnName);
        case MathTools.SumType.TimeSpan: return MinTimeSpan(dv, columnName, skipNulls);
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object MinValue(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif

      DataColumn col = table.Columns[columnName];
      if (col == null)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", table, columnName);

      switch (MathTools.GetSumType(col.DataType))
      {
        case MathTools.SumType.Int32: return MinInt32(table, columnName, skipNulls);
        case MathTools.SumType.Int64: return MinInt64(table, columnName, skipNulls);
        case MathTools.SumType.Single: return MinSingle(table, columnName, skipNulls);
        case MathTools.SumType.Double: return MinDouble(table, columnName, skipNulls);
        case MathTools.SumType.Decimal: return MinDecimal(table, columnName, skipNulls);
        case MathTools.SumType.DateTime: return MinDateTime(table, columnName);
        case MathTools.SumType.TimeSpan: return MinTimeSpan(table, columnName, skipNulls);
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }

    /// <summary>
    /// Получить минимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static object MinValue(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      DataColumn col;
      Type t = GetDataTypeFromRows(rows, columnName, out col);
      if (t == null)
        return null;

      switch (MathTools.GetSumType(t))
      {
        case MathTools.SumType.Int32: return MinInt32(rows, columnName, skipNulls);
        case MathTools.SumType.Int64: return MinInt64(rows, columnName, skipNulls);
        case MathTools.SumType.Single: return MinSingle(rows, columnName, skipNulls);
        case MathTools.SumType.Double: return MinDouble(rows, columnName, skipNulls);
        case MathTools.SumType.Decimal: return MinDecimal(rows, columnName, skipNulls);
        case MathTools.SumType.DateTime: return MinDateTime(rows, columnName);
        case MathTools.SumType.TimeSpan: return MinTimeSpan(rows, columnName, skipNulls);
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка <paramref name="totalRow"/>. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MinValue(DataRow totalRow, string columnName, bool skipNulls)
    {
#if DEBUG
      if (totalRow == null)
        throw new ArgumentNullException("totalRow");
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif

      DataColumn col = totalRow.Table.Columns[columnName];
      if (col == null)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", totalRow.Table, columnName);

      switch (MathTools.GetSumType(col.DataType))
      {
        case MathTools.SumType.Int32: MinInt32(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.Int64: MinInt64(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.Single: MinSingle(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.Double: MinDouble(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.Decimal: MinDecimal(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.DateTime: MinDateTime(totalRow, columnName); break;
        case MathTools.SumType.TimeSpan: MinTimeSpan(totalRow, columnName, skipNulls); break;
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }


    /// <summary>
    /// Получить минимальное значение среди элементов одномерного массива чисел произольного типа.
    /// Выполняется преобразование к самому большому типу.
    /// Значения null пропускаются. Если массив пустой или содержит только null'ы, то
    /// возвращается null
    /// Если в массиве есть значения несовместимых типов (например, int и TimeSpan),
    /// генерируется исключение
    /// </summary>
    /// <param name="items">Коллекция или массив (может быть null)</param>
    /// <returns>Минимальное значение</returns>
    public static object MinValue(System.Collections.IEnumerable items)
    {
      if (items == null)
        return null;

      CorrectAggregateEnumerable(ref items);


      object res = null;
      MathTools.SumType st = MathTools.SumType.None;
      foreach (object vx in items)
      {
        if (vx == null)
          continue;
        MathTools.SumType st2 = MathTools.GetSumType(vx.GetType());
        object v = MathTools.ConvertValue(vx, st2);

        if (res == null)
        {
          st = st2;
          res = v;
        }
        else
        {
          st = MathTools.GetLargestSumType(st, st2);
          res = MathTools.ConvertValue(res, st);
          switch (st)
          {
            case MathTools.SumType.Int32: res = Math.Min((int)res, (int)v); break;
            case MathTools.SumType.Int64: res = Math.Min((long)res, (long)v); break;
            case MathTools.SumType.Single: res = Math.Min((float)res, (float)v); break;
            case MathTools.SumType.Double: res = Math.Min((double)res, (double)v); break;
            case MathTools.SumType.Decimal: res = Math.Min((decimal)res, (decimal)v); break;
            case MathTools.SumType.DateTime: res = TimeTools.Min((DateTime)res, (DateTime)v); break;
            case MathTools.SumType.TimeSpan: res = TimeTools.Min((TimeSpan)res, (TimeSpan)v); break;
            default:
              throw ExceptionFactory.ArgInvalidEnumerableType("items", items);
          }
        }
      }

      return res;
    }

    #endregion

    #endregion

    #region Максимальное значение

    #region Int32

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? MaxInt32(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetInt32(drv.Row[p]));
        else
          s = DataTools.GetInt32(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? MaxInt32(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      int? s = null;
      foreach (DataRow row in table.Rows)
      {
        if (skipNulls)
        {
          if (row.RowState == DataRowState.Deleted)
            continue;
          if (row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetInt32(row[p]));
        else
          s = DataTools.GetInt32(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? MaxInt32(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int? s = null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        int? v = rowVals[columnName].AsInt32;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0);
        else
          s = v ?? 0;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MaxInt32(DataRow totalRow, string columnName, bool skipNulls)
    {
      int? v = MaxInt32(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Коллекция значений(может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static int? MaxInt32(IEnumerable<int> items)
    {
      if (items == null)
        return null;

      int? s = null;
      foreach (int item in items)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, item);
        else
          s = item;
      }
      return s;
    }

    #endregion

    #region Int64

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? MaxInt64(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      long? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetInt64(drv.Row[p]));
        else
          s = DataTools.GetInt64(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? MaxInt64(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      long? s = null;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetInt64(row[p]));
        else
          s = DataTools.GetInt64(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? MaxInt64(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      long? s = null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        long? v = rowVals[columnName].AsInt64;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0L);
        else
          s = v ?? 0L;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="row">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MaxInt64(DataRow row, string columnName, bool skipNulls)
    {
      long? v = MaxInt64(row.Table, columnName, skipNulls);
      if (v.HasValue)
        row[columnName] = v.Value;
      else
        row[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Коллекция значений(может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static long? MaxInt64(IEnumerable<long> items)
    {
      if (items == null)
        return null;

      long? s = null;
      foreach (long item in items)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, item);
        else
          s = item;
      }
      return s;
    }

    #endregion

    #region Single

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? MaxSingle(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      float? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetSingle(drv.Row[p]));
        else
          s = DataTools.GetSingle(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? MaxSingle(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      float? s = null;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetSingle(row[p]));
        else
          s = DataTools.GetSingle(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? MaxSingle(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      float? s = null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        float? v = rowVals[columnName].AsNullableSingle;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0f);
        else
          s = v ?? 0f;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MaxSingle(DataRow totalRow, string columnName, bool skipNulls)
    {
      float? v = MaxSingle(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Коллекция значений(может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static float? MaxSingle(IEnumerable<float> items)
    {
      if (items == null)
        return null;

      float? s = null;
      foreach (float item in items)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, item);
        else
          s = item;
      }
      return s;
    }

    #endregion

    #region Double

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? MaxDouble(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      double? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetDouble(drv.Row[p]));
        else
          s = DataTools.GetDouble(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? MaxDouble(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      double? s = null;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetDouble(row[p]));
        else
          s = DataTools.GetDouble(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? MaxDouble(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      double? s = null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        double? v = rowVals[columnName].AsNullableDouble;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0.0);
        else
          s = v ?? 0.0;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MaxDouble(DataRow totalRow, string columnName, bool skipNulls)
    {
      double? v = MaxDouble(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Коллекция значений(может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static double? MaxDouble(IEnumerable<double> items)
    {
      if (items == null)
        return null;

      double? s = null;
      foreach (double item in items)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, item);
        else
          s = item;
      }
      return s;
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? MaxDecimal(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      decimal? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetDecimal(drv.Row[p]));
        else
          s = DataTools.GetDecimal(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? MaxDecimal(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      decimal? s = null;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, DataTools.GetDecimal(row[p]));
        else
          s = DataTools.GetDecimal(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? MaxDecimal(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      decimal? s = null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        decimal? v = rowVals[columnName].AsNullableDecimal;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = Math.Max(s.Value, v ?? 0m);
        else
          s = v ?? 0m;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MaxDecimal(DataRow totalRow, string columnName, bool skipNulls)
    {
      decimal? v = MaxDecimal(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Коллекция значений(может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static decimal? MaxDecimal(IEnumerable<decimal> items)
    {
      if (items == null)
        return null;

      decimal? s = null;
      foreach (decimal item in items)
      {
        if (s.HasValue)
          s = Math.Max(s.Value, item);
        else
          s = item;
      }
      return s;
    }

    #endregion

    #region DateTime

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime? MaxDateTime(DataView dv, string columnName)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      DateTime? s = null;
      foreach (DataRowView drv in dv)
      {
        //if (SkipNulls)
        //{
        //  if (drv.Row.IsNull(p))
        //    continue;
        //}
        if (s.HasValue)
          s = TimeTools.Max(s.Value, DataTools.GetNullableDateTime(drv.Row[p]));
        else
          s = DataTools.GetNullableDateTime(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime? MaxDateTime(DataTable table, string columnName)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      DateTime? s = null;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        //if (SkipNulls)
        //{
        //  if (Row.IsNull(p))
        //    continue;
        //}
        if (s.HasValue)
          s = TimeTools.Max(s.Value, DataTools.GetNullableDateTime(row[p]));
        else
          s = DataTools.GetNullableDateTime(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Найденное значение или null</returns>
    public static DateTime? MaxDateTime(IEnumerable<DataRow> rows, string columnName)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      DateTime? s = null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        DateTime? v = rowVals[columnName].AsNullableDateTime;
        //if (SkipNulls)
        //{
        //  if (!v.HasValue)
        //    continue;
        //}
        if (s.HasValue)
          s = TimeTools.Max(s.Value, v);
        else
          s = v;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    public static void MaxDateTime(DataRow totalRow, string columnName)
    {
      DateTime? v = MaxDateTime(totalRow.Table, columnName);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Коллекция значений(может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static DateTime? MaxDateTime(IEnumerable<DateTime> items)
    {
      if (items == null)
        return null;

      DateTime? s = null;
      foreach (DateTime item in items)
      {
        if (s.HasValue)
          s = TimeTools.Max(s.Value, item);
        else
          s = item;
      }
      return s;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? MaxTimeSpan(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      TimeSpan? s = null;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = TimeTools.Max(s.Value, DataTools.GetTimeSpan(drv.Row[p]));
        else
          s = DataTools.GetTimeSpan(drv.Row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? MaxTimeSpan(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      TimeSpan? s = null;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        if (s.HasValue)
          s = TimeTools.Max(s.Value, DataTools.GetTimeSpan(row[p]));
        else
          s = DataTools.GetTimeSpan(row[p]);
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? MaxTimeSpan(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      TimeSpan? s = null;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        TimeSpan? v = null;
        if (!rowVals[columnName].IsNull)
          v = rowVals[columnName].AsTimeSpan;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        if (s.HasValue)
          s = TimeTools.Max(s.Value, v ?? TimeSpan.Zero);
        else
          s = v ?? TimeSpan.Zero;
      }
      return s;
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MaxTimeSpan(DataRow totalRow, string columnName, bool skipNulls)
    {
      TimeSpan? v = MaxTimeSpan(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить максимальное значение из коллекции элементов
    /// </summary>
    /// <param name="items">Коллекция значений(может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static TimeSpan? MaxTimeSpan(IEnumerable<TimeSpan> items)
    {
      if (items == null)
        return null;

      TimeSpan? s = null;
      foreach (TimeSpan item in items)
      {
        if (s.HasValue)
          s = TimeTools.Max(s.Value, item);
        else
          s = item;
      }
      return s;
    }

    #endregion

    #region Любой тип данных

    /// <summary>
    /// Получить максимальное значение поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object MaxValue(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif

      DataColumn col = dv.Table.Columns[columnName];
      if (col == null)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", dv.Table, columnName);

      switch (MathTools.GetSumType(col.DataType))
      {
        case MathTools.SumType.Int32: return MaxInt32(dv, columnName, skipNulls);
        case MathTools.SumType.Int64: return MaxInt64(dv, columnName, skipNulls);
        case MathTools.SumType.Single: return MaxSingle(dv, columnName, skipNulls);
        case MathTools.SumType.Double: return MaxDouble(dv, columnName, skipNulls);
        case MathTools.SumType.Decimal: return MaxDecimal(dv, columnName, skipNulls);
        case MathTools.SumType.DateTime: return MaxDateTime(dv, columnName);
        case MathTools.SumType.TimeSpan: return MaxTimeSpan(dv, columnName, skipNulls);
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object MaxValue(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif

      DataColumn col = table.Columns[columnName];
      if (col == null)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", table, columnName);

      switch (MathTools.GetSumType(col.DataType))
      {
        case MathTools.SumType.Int32: return MaxInt32(table, columnName, skipNulls);
        case MathTools.SumType.Int64: return MaxInt64(table, columnName, skipNulls);
        case MathTools.SumType.Single: return MaxSingle(table, columnName, skipNulls);
        case MathTools.SumType.Double: return MaxDouble(table, columnName, skipNulls);
        case MathTools.SumType.Decimal: return MaxDecimal(table, columnName, skipNulls);
        case MathTools.SumType.DateTime: return MaxDateTime(table, columnName);
        case MathTools.SumType.TimeSpan: return MaxTimeSpan(table, columnName, skipNulls);
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }

    /// <summary>
    /// Получить максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static object MaxValue(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      DataColumn col;
      Type t = GetDataTypeFromRows(rows, columnName, out col);
      if (t == null)
        return null;

      switch (MathTools.GetSumType(t))
      {
        case MathTools.SumType.Int32: return MaxInt32(rows, columnName, skipNulls);
        case MathTools.SumType.Int64: return MaxInt64(rows, columnName, skipNulls);
        case MathTools.SumType.Single: return MaxSingle(rows, columnName, skipNulls);
        case MathTools.SumType.Double: return MaxDouble(rows, columnName, skipNulls);
        case MathTools.SumType.Decimal: return MaxDecimal(rows, columnName, skipNulls);
        case MathTools.SumType.DateTime: return MaxDateTime(rows, columnName);
        case MathTools.SumType.TimeSpan: return MaxTimeSpan(rows, columnName, skipNulls);
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }

    /// <summary>
    /// Получить максимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void MaxValue(DataRow totalRow, string columnName, bool skipNulls)
    {
#if DEBUG
      if (totalRow == null)
        throw new ArgumentNullException("totalRow");
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif

      DataColumn col = totalRow.Table.Columns[columnName];
      if (col == null)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", totalRow.Table, columnName);

      switch (MathTools.GetSumType(col.DataType))
      {
        case MathTools.SumType.Int32: MaxInt32(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.Int64: MaxInt64(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.Single: MaxSingle(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.Double: MaxDouble(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.Decimal: MaxDecimal(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.DateTime: MaxDateTime(totalRow, columnName); break;
        case MathTools.SumType.TimeSpan: MaxTimeSpan(totalRow, columnName, skipNulls); break;
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }

    /// <summary>
    /// Получить максимальное значение среди элементов одномерного массива чисел произольного типа.
    /// Выполняется преобразование к самому большому типу.
    /// Значения null пропускаются. Если массив пустой или содержит только null'ы, то
    /// возвращается null
    /// Если в массиве есть значения несовместимых типов (например, int и TimeSpan),
    /// генерируется исключение
    /// </summary>
    /// <param name="items">Коллекция или массив (может быть null)</param>
    /// <returns>Максимальное значение</returns>
    public static object MaxValue(System.Collections.IEnumerable items)
    {
      if (items == null)
        return null;

      CorrectAggregateEnumerable(ref items);


      object res = null;
      MathTools.SumType st = MathTools.SumType.None;
      foreach (object vx in items)
      {
        if (vx == null)
          continue;
        MathTools.SumType st2 = MathTools.GetSumType(vx.GetType());
        object v = MathTools.ConvertValue(vx, st2);

        if (res == null)
        {
          st = st2;
          res = v;
        }
        else
        {
          st = MathTools.GetLargestSumType(st, st2);
          res = MathTools.ConvertValue(res, st);
          switch (st)
          {
            case MathTools.SumType.Int32: res = Math.Max((int)res, (int)v); break;
            case MathTools.SumType.Int64: res = Math.Max((long)res, (long)v); break;
            case MathTools.SumType.Single: res = Math.Max((float)res, (float)v); break;
            case MathTools.SumType.Double: res = Math.Max((double)res, (double)v); break;
            case MathTools.SumType.Decimal: res = Math.Max((decimal)res, (decimal)v); break;
            case MathTools.SumType.DateTime: res = TimeTools.Max((DateTime)res, (DateTime)v); break;
            case MathTools.SumType.TimeSpan: res = TimeTools.Max((TimeSpan)res, (TimeSpan)v); break;
            default:
              throw ExceptionFactory.ArgInvalidEnumerableType("items", items);
          }
        }
      }

      return res;
    }

    #endregion

    #endregion

    #region Минимальное и максимальное значение вместе

    #region Int32

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Int32> MinMaxInt32(DataView dv, string columnName, bool skipNulls)
    {
      MinMax<Int32> res = new MinMax<Int32>();
      if (dv == null)
        return res;
      if (dv.Count == 0)
        return res;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        res += DataTools.GetInt32(drv.Row[p]);
      }
      return res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Int32> MinMaxInt32(DataTable table, string columnName, bool skipNulls)
    {
      MinMax<Int32> res = new MinMax<Int32>();
      if (table == null)
        return res;
      if (table.Rows.Count == 0)
        return res;
      int p = GetColumnPosWithCheck(table, columnName);
      foreach (DataRow Row in table.Rows)
      {
        if (Row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (Row.IsNull(p))
            continue;
        }
        res += DataTools.GetInt32(Row[p]);
      }
      return res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Int32> MinMaxInt32(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      MinMax<Int32> res = new MinMax<Int32>();
      if (rows == null)
        return res;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        int? v = rowVals[columnName].AsNullableInt32;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        res += v ?? 0;
      }
      return res;
    }

    #endregion

    #region Int64

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Int64> MinMaxInt64(DataView dv, string columnName, bool skipNulls)
    {
      MinMax<Int64> res = new MinMax<Int64>();
      if (dv == null)
        return res;
      if (dv.Count == 0)
        return res;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        res += DataTools.GetInt64(drv.Row[p]);
      }
      return res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Int64> MinMaxInt64(DataTable table, string columnName, bool skipNulls)
    {
      MinMax<Int64> res = new MinMax<Int64>();
      if (table == null)
        return res;
      if (table.Rows.Count == 0)
        return res;
      int p = GetColumnPosWithCheck(table, columnName);
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        res += DataTools.GetInt64(row[p]);
      }
      return res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Int64> MinMaxInt64(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      MinMax<Int64> res = new MinMax<Int64>();
      if (rows == null)
        return res;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        long? v = rowVals[columnName].AsNullableInt64;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        res += v ?? 0L;
      }
      return res;
    }

    #endregion

    #region Single

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Single> MinMaxSingle(DataView dv, string columnName, bool skipNulls)
    {
      MinMax<Single> res = new MinMax<Single>();
      if (dv == null)
        return res;
      if (dv.Count == 0)
        return res;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        res += DataTools.GetSingle(drv.Row[p]);
      }
      return res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Single> MinMaxSingle(DataTable table, string columnName, bool skipNulls)
    {
      MinMax<Single> res = new MinMax<Single>();
      if (table == null)
        return res;
      if (table.Rows.Count == 0)
        return res;
      int p = GetColumnPosWithCheck(table, columnName);
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        res += DataTools.GetSingle(row[p]);
      }
      return res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Single> MinMaxSingle(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      MinMax<Single> res = new MinMax<Single>();
      if (rows == null)
        return res;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        float? v = rowVals[columnName].AsNullableSingle;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        res += v ?? 0f;
      }
      return res;
    }

    #endregion

    #region Double

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Double> MinMaxDouble(DataView dv, string columnName, bool skipNulls)
    {
      MinMax<Double> res = new MinMax<Double>();
      if (dv == null)
        return res;
      if (dv.Count == 0)
        return res;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        res += DataTools.GetDouble(drv.Row[p]);
      }
      return res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Double> MinMaxDouble(DataTable table, string columnName, bool skipNulls)
    {
      MinMax<Double> res = new MinMax<Double>();
      if (table == null)
        return res;
      if (table.Rows.Count == 0)
        return res;
      int p = GetColumnPosWithCheck(table, columnName);
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        res += DataTools.GetDouble(row[p]);
      }
      return res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Double> MinMaxDouble(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      MinMax<Double> res = new MinMax<Double>();
      if (rows == null)
        return res;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        double? v = rowVals[columnName].AsNullableDouble;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        res += v ?? 0.0;
      }
      return res;
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Decimal> MinMaxDecimal(DataView dv, string columnName, bool skipNulls)
    {
      MinMax<Decimal> res = new MinMax<Decimal>();
      if (dv == null)
        return res;
      if (dv.Count == 0)
        return res;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        res += DataTools.GetDecimal(drv.Row[p]);
      }
      return res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Decimal> MinMaxDecimal(DataTable table, string columnName, bool skipNulls)
    {
      MinMax<Decimal> res = new MinMax<Decimal>();
      if (table == null)
        return res;
      if (table.Rows.Count == 0)
        return res;
      int p = GetColumnPosWithCheck(table, columnName);
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        res += DataTools.GetDecimal(row[p]);
      }
      return res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<Decimal> MinMaxDecimal(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      MinMax<Decimal> res = new MinMax<Decimal>();
      if (rows == null)
        return res;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        decimal? v = rowVals[columnName].AsNullableDecimal;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        res += v ?? 0m;
      }
      return res;
    }

    #endregion

    #region DateTime

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Диапазон</returns>
    public static MinMax<DateTime> MinMaxDateTime(DataView dv, string columnName)
    {
      MinMax<DateTime> res = new MinMax<DateTime>();
      if (dv == null)
        return res;
      if (dv.Count == 0)
        return res;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      foreach (DataRowView drv in dv)
      {
        //if (SkipNulls)
        //{
        //  if (drv.Row.IsNull(p))
        //    continue;
        //}
        res += DataTools.GetNullableDateTime(drv.Row[p]);
      }
      return res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Диапазон</returns>
    public static MinMax<DateTime> MinMaxDateTime(DataTable table, string columnName)
    {
      MinMax<DateTime> res = new MinMax<DateTime>();
      if (table == null)
        return res;
      if (table.Rows.Count == 0)
        return res;
      int p = GetColumnPosWithCheck(table, columnName);
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        //if (SkipNulls)
        //{
        //  if (Row.IsNull(p))
        //    continue;
        //}
        res += DataTools.GetNullableDateTime(row[p]);
      }
      return res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// Пустые значения полей пропускаются.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Диапазон</returns>
    public static MinMax<DateTime> MinMaxDateTime(IEnumerable<DataRow> rows, string columnName)
    {
      MinMax<DateTime> res = new MinMax<DateTime>();
      if (rows == null)
        return res;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        DateTime? v = rowVals[columnName].AsNullableDateTime;
        //if (SkipNulls)
        //{
        //  if (!v.HasValue)
        //    continue;
        //}
        res += v;
      }
      return res;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<TimeSpan> MinMaxTimeSpan(DataView dv, string columnName, bool skipNulls)
    {
      MinMax<TimeSpan> res = new MinMax<TimeSpan>();
      if (dv == null)
        return res;
      if (dv.Count == 0)
        return res;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        res += DataTools.GetTimeSpan(drv.Row[p]);
      }
      return res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<TimeSpan> MinMaxTimeSpan(DataTable table, string columnName, bool skipNulls)
    {
      MinMax<TimeSpan> res = new MinMax<TimeSpan>();
      if (table == null)
        return res;
      if (table.Rows.Count == 0)
        return res;
      int p = GetColumnPosWithCheck(table, columnName);
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        res += DataTools.GetTimeSpan(row[p]);
      }
      return res;
    }

    /// <summary>
    /// Получить минимальное и максимальное значение поля для коллекции строк.
    /// Если нет ни одной подходящей строки, возвращается пустое значение.
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Диапазон</returns>
    public static MinMax<TimeSpan> MinMaxTimeSpan(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      MinMax<TimeSpan> res = new MinMax<TimeSpan>();
      if (rows == null)
        return res;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        TimeSpan? v = null;
        if (!rowVals[columnName].IsNull)
          v = rowVals[columnName].AsTimeSpan;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        res += v ?? TimeSpan.Zero;
      }
      return res;
    }

    #endregion

    #endregion

    #region Среднее значение

    #region Int32

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? AverageInt32(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int s = 0;
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        s += DataTools.GetInt32(drv.Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return MathTools.DivideWithRounding(s, cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? AverageInt32(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      int s = 0;
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        s += DataTools.GetInt32(row[p]);
        cnt++;
      }
      if (cnt > 0)
        return MathTools.DivideWithRounding(s, cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для строк в коллекции.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static int? AverageInt32(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int s = 0;
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        int? v = rowVals[columnName].AsNullableInt32;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0;
        cnt++;
      }
      if (cnt > 0)
        return MathTools.DivideWithRounding(s, cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void AverageInt32(DataRow totalRow, string columnName, bool skipNulls)
    {
      int? v = AverageInt32(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить среднее значение элементов коллекции чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static int? AverageInt32(IEnumerable<int> items)
    {
      if (items == null)
        return null;

      int s = 0;
      int cnt = 0;
      foreach (int item in items)
      {
        s += item;
        cnt++;
      }
      if (cnt == 0)
        return null;
      else
        return MathTools.DivideWithRounding(s, cnt);
    }

    #endregion

    #region Int64

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? AverageInt64(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      long s = 0;
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        s += DataTools.GetInt64(drv.Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return MathTools.DivideWithRounding(s, (long)cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? AverageInt64(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      long s = 0;
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        s += DataTools.GetInt64(row[p]);
        cnt++;
      }
      if (cnt > 0)
        return MathTools.DivideWithRounding(s, (long)cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для строк в коллекции.
    /// Если нет ни одной подходящей строки, возвращается null.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="rows">Коллекции строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static long? AverageInt64(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      long s = 0;
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        long? v = rowVals[columnName].AsNullableInt64;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0L;
        cnt++;
      }
      if (cnt > 0)
        return MathTools.DivideWithRounding(s, cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void AverageInt64(DataRow totalRow, string columnName, bool skipNulls)
    {
      long? v = AverageInt64(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить среднее значение элементов коллекции чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static long? AverageInt64(IEnumerable<long> items)
    {
      if (items == null)
        return null;

      long s = 0;
      int cnt = 0;
      foreach (long item in items)
      {
        s += item;
        cnt++;
      }
      if (cnt == 0)
        return null;
      else
        return MathTools.DivideWithRounding(s, cnt);
    }

    #endregion

    #region Single

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? AverageSingle(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      float s = 0;
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        s += DataTools.GetSingle(drv.Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? AverageSingle(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      float s = 0;
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        s += DataTools.GetSingle(row[p]);
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для строк в коллекции.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static float? AverageSingle(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      float s = 0;
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        float? v = rowVals[columnName].AsNullableSingle;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0f;
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void AverageSingle(DataRow totalRow, string columnName, bool skipNulls)
    {
      float? v = AverageSingle(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить среднее значение элементов коллекции чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static float? AverageSingle(IEnumerable<float> items)
    {
      if (items == null)
        return null;

      float s = 0;
      int cnt = 0;
      foreach (float item in items)
      {
        s += item;
        cnt++;
      }

      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    #endregion

    #region Double

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? AverageDouble(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      double s = 0;
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        s += DataTools.GetDouble(drv.Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? AverageDouble(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      double s = 0;
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        s += DataTools.GetDouble(row[p]);
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для строк в коллекции.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static double? AverageDouble(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      double s = 0;
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        double? v = rowVals[columnName].AsNullableDouble;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0.0;
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void AverageDouble(DataRow totalRow, string columnName, bool skipNulls)
    {
      double? v = AverageDouble(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить среднее значение элементов коллекции чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static double? AverageDouble(IEnumerable<double> items)
    {
      if (items == null)
        return null;

      double s = 0;
      int cnt = 0;
      foreach (double item in items)
      {
        s += item;
        cnt++;
      }

      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? AverageDecimal(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      decimal s = 0;
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        s += DataTools.GetDecimal(drv.Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? AverageDecimal(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      decimal s = 0;
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        s += DataTools.GetDecimal(row[p]);
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для строк в коллекции.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static decimal? AverageDecimal(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      decimal s = 0;
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        decimal? v = rowVals[columnName].AsNullableDecimal;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? 0m;
        cnt++;
      }
      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void AverageDecimal(DataRow totalRow, string columnName, bool skipNulls)
    {
      decimal? v = AverageDecimal(totalRow.Table, columnName, skipNulls); // испр.16.12.2021
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить среднее значение элементов коллекции чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static decimal? AverageDecimal(IEnumerable<decimal> items)
    {
      if (items == null)
        return null;

      decimal s = 0;
      int cnt = 0;
      foreach (decimal item in items)
      {
        s += item;
        cnt++;
      }

      if (cnt > 0)
        return s / cnt;
      else
        return null;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? AverageTimeSpan(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;
      if (dv.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      TimeSpan s = TimeSpan.Zero;
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (skipNulls)
        {
          if (drv.Row.IsNull(p))
            continue;
        }
        s += DataTools.GetTimeSpan(drv.Row[p]);
        cnt++;
      }
      if (cnt > 0)
        return new TimeSpan(s.Ticks / cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? AverageTimeSpan(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;
      if (table.Rows.Count == 0)
        return null;
      int p = GetColumnPosWithCheck(table, columnName);
      TimeSpan s = TimeSpan.Zero;
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (skipNulls)
        {
          if (row.IsNull(p))
            continue;
        }
        s += DataTools.GetTimeSpan(row[p]);
        cnt++;
      }
      if (cnt > 0)
        return new TimeSpan(s.Ticks / cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для строк в коллекции.
    /// Если нет ни одной подходящей строки, возвращается null
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение или null</returns>
    public static TimeSpan? AverageTimeSpan(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      if (rows == null)
        return null;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      TimeSpan s = TimeSpan.Zero;
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;

        rowVals.CurrentRow = row;
        TimeSpan? v = null;
        if (!rowVals[columnName].IsNull)
          v = rowVals[columnName].AsTimeSpan;
        if (skipNulls)
        {
          if (!v.HasValue)
            continue;
        }
        s += v ?? TimeSpan.Zero;
        cnt++;
      }
      if (cnt > 0)
        return new TimeSpan(s.Ticks / cnt);
      else
        return null;
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах.
    /// Если нет ни одной подходящей строки, то записывется DBNull.
    /// Иначе значение записывается, включая 0.
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void AverageTimeSpan(DataRow totalRow, string columnName, bool skipNulls)
    {
      TimeSpan? v = AverageTimeSpan(totalRow.Table, columnName, skipNulls);
      if (v.HasValue)
        totalRow[columnName] = v.Value;
      else
        totalRow[columnName] = DBNull.Value;
    }

    /// <summary>
    /// Получить среднее значение элементов коллекции чисел.
    /// Для деления используется DivideWithRounding() для округления к ближайшему целому, а не в сторону нуля.
    /// </summary>
    /// <param name="items">Коллекция (может быть null)</param>
    /// <returns>Найденное значение или null, если коллекция пустая</returns>
    public static TimeSpan? AverageTimeSpan(IEnumerable<TimeSpan> items)
    {
      if (items == null)
        return null;

      TimeSpan s = TimeSpan.Zero;
      int cnt = 0;
      foreach (TimeSpan item in items)
      {
        s += item;
        cnt++;
      }

      if (cnt > 0)
        return new TimeSpan(s.Ticks / cnt);
      else
        return null;
    }

    #endregion

    #region Любой тип данных

    /// <summary>
    /// Получить среднее значение поля для всех строк в просмотре
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object AverageValue(DataView dv, string columnName, bool skipNulls)
    {
      if (dv == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif

      DataColumn col = dv.Table.Columns[columnName];
      if (col == null)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", dv.Table, columnName);

      switch (MathTools.GetSumType(col.DataType))
      {
        case MathTools.SumType.Int32: return AverageInt32(dv, columnName, skipNulls);
        case MathTools.SumType.Int64: return AverageInt64(dv, columnName, skipNulls);
        case MathTools.SumType.Single: return AverageSingle(dv, columnName, skipNulls);
        case MathTools.SumType.Double: return AverageDouble(dv, columnName, skipNulls);
        case MathTools.SumType.Decimal: return AverageDecimal(dv, columnName, skipNulls);
        case MathTools.SumType.TimeSpan: return AverageTimeSpan(dv, columnName, skipNulls);
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }

    /// <summary>
    /// Получить среднее значение поля для всех строк в таблице
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object AverageValue(DataTable table, string columnName, bool skipNulls)
    {
      if (table == null)
        return null;

#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif

      DataColumn col = table.Columns[columnName];
      if (col == null)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", table, columnName);

      switch (MathTools.GetSumType(col.DataType))
      {
        case MathTools.SumType.Int32: return AverageInt32(table, columnName, skipNulls);
        case MathTools.SumType.Int64: return AverageInt64(table, columnName, skipNulls);
        case MathTools.SumType.Single: return AverageSingle(table, columnName, skipNulls);
        case MathTools.SumType.Double: return AverageDouble(table, columnName, skipNulls);
        case MathTools.SumType.Decimal: return AverageDecimal(table, columnName, skipNulls);
        case MathTools.SumType.TimeSpan: return AverageTimeSpan(table, columnName, skipNulls);
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }

    /// <summary>
    /// Получить среднее значение поля для строк в коллекции
    /// </summary>
    /// <param name="rows">Коллекция строк. В массиве могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    /// <returns>Найденное значение для всех строк</returns>
    public static object AverageValue(IEnumerable<DataRow> rows, string columnName, bool skipNulls)
    {
      DataColumn col;
      Type t = GetDataTypeFromRows(rows, columnName, out col);
      if (t == null)
        return null;

      switch (MathTools.GetSumType(t))
      {
        case MathTools.SumType.Int32: return AverageInt32(rows, columnName, skipNulls);
        case MathTools.SumType.Int64: return AverageInt64(rows, columnName, skipNulls);
        case MathTools.SumType.Single: return AverageSingle(rows, columnName, skipNulls);
        case MathTools.SumType.Double: return AverageDouble(rows, columnName, skipNulls);
        case MathTools.SumType.Decimal: return AverageDecimal(rows, columnName, skipNulls);
        case MathTools.SumType.TimeSpan: return AverageTimeSpan(rows, columnName, skipNulls);
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }

    /// <summary>
    /// Получить минимальное значение поля для всех строк в таблице, к которым 
    /// относится строка Row. Полученное значение записывается в поле строки.
    /// Предполагается, что строка получена вызовом DataTable.NewRow(), но еще
    /// не добавлена в таблицу.
    /// Метод используется для расчета итоговой строки в отчетах
    /// Нулевые значения записываются без использования DBNull
    /// </summary>
    /// <param name="totalRow">Итоговая строка в процессе заполнения</param>
    /// <param name="columnName">Имя суммируемого поля</param>
    /// <param name="skipNulls">Пропускать значения DBNull. Если false, то DBNull будут считаться как 0</param>
    public static void AverageValue(DataRow totalRow, string columnName, bool skipNulls)
    {
#if DEBUG
      if (totalRow == null)
        throw new ArgumentNullException("totalRow");
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif

      DataColumn col = totalRow.Table.Columns[columnName];
      if (col == null)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", totalRow.Table, columnName);

      switch (MathTools.GetSumType(col.DataType))
      {
        case MathTools.SumType.Int32: AverageInt32(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.Int64: AverageInt64(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.Single: AverageSingle(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.Double: AverageDouble(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.Decimal: AverageDecimal(totalRow, columnName, skipNulls); break;
        case MathTools.SumType.TimeSpan: AverageTimeSpan(totalRow, columnName, skipNulls); break;
        default:
          throw ExceptionFactory.ArgInvalidColumnType("columnName", col);
      }
    }


    /// <summary>
    /// Получить среднее значение элементов одномерного массива чисел произольного типа.
    /// Выполняется преобразование к самому большому типу.
    /// Значения null пропускаются. Если массив пустой или содержит только null'ы, то
    /// возвращается null
    /// Если в массиве есть значения несовместимых типов (например, int и TimeSpan),
    /// генерируется исключение
    /// </summary>
    /// <param name="items">Коллекция или массив (может быть null)</param>
    /// <returns>Сумма элементов</returns>
    public static object AverageValue(System.Collections.IEnumerable items)
    {
      if (items == null)
        return null;

      CorrectAggregateEnumerable(ref items);

      object res = null;
      MathTools.SumType st = MathTools.SumType.None;
      int cnt = 0;
      foreach (object vx in items)
      {
        if (vx == null)
          continue;
        MathTools.SumType st2 = MathTools.GetSumType(vx.GetType());
        st = MathTools.GetLargestSumType(st, st2);
        res = MathTools.ConvertValue(res, st);
        object v = MathTools.ConvertValue(vx, st);
        switch (st)
        {
          case MathTools.SumType.Int32: res = (int)res + (int)v; break;
          case MathTools.SumType.Int64: res = (long)res + (long)v; break;
          case MathTools.SumType.Single: res = (float)res + (float)v; break;
          case MathTools.SumType.Double: res = (double)res + (double)v; break;
          case MathTools.SumType.Decimal: res = (decimal)res + (decimal)v; break;
          case MathTools.SumType.TimeSpan: res = (TimeSpan)res + (TimeSpan)v; break;
          default:
            throw ExceptionFactory.ArgInvalidEnumerableType("items", items);
        }
        cnt++;
      }

      if (res == null)
        return null;

      switch (st)
      {
        case MathTools.SumType.Int32: return MathTools.DivideWithRounding((int)res, cnt);
        case MathTools.SumType.Int64: return MathTools.DivideWithRounding((long)res, (long)cnt);
        case MathTools.SumType.Single: return (float)res / cnt;
        case MathTools.SumType.Double: return (double)res / cnt;
        case MathTools.SumType.Decimal: return (decimal)res / cnt;
        case MathTools.SumType.TimeSpan:
          return new TimeSpan(((TimeSpan)res).Ticks / cnt);
        default:
          throw new BugException();
      }
    }

    #endregion

    #endregion

    #region Подсчет числа строк

    #region Int32

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithInt32Value(DataView dv, string columnName, int searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetInt32(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithInt32Value(DataTable table, string columnName, int searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetInt32(row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithInt32Value(IEnumerable<DataRow> rows, string columnName, int searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        if (rowVals[columnName].AsInt32 == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region Int64

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithInt64Value(DataView dv, string columnName, long searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetInt64(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithInt64Value(DataTable table, string columnName, long searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetInt64(row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithInt64Value(IEnumerable<DataRow> rows, string columnName, long searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        if (rowVals[columnName].AsInt64 == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region Single

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithSingleValue(DataView dv, string columnName, float searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetSingle(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithSingleValue(DataTable table, string columnName, float searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetSingle(row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithSingleValue(IEnumerable<DataRow> rows, string columnName, float searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        if (rowVals[columnName].AsSingle == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region Double

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithDoubleValue(DataView dv, string columnName, double searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetDouble(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithDoubleValue(DataTable table, string columnName, double searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetDouble(row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithDoubleValue(IEnumerable<DataRow> rows, string columnName, double searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        if (rowVals[columnName].AsDouble == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region Decimal

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithDecimalValue(DataView dv, string columnName, decimal searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetDecimal(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithDecimalValue(DataTable table, string columnName, decimal searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetDecimal(row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithDecimalValue(IEnumerable<DataRow> rows, string columnName, decimal searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        if (rowVals[columnName].AsDecimal == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region String

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля.
    /// Поиск является регистрочувствительным.
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithStringValue(DataView dv, string columnName, string searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetString(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля.
    /// Поиск является регистрочувствительным.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithStringValue(DataTable table, string columnName, string searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetString(row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля. 
    /// Поиск является регистрочувствительным.
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithStringValue(IEnumerable<DataRow> rows, string columnName, string searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        if (rowVals[columnName].AsString == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region Boolean

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithBooleanValue(DataView dv, string columnName, bool searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetBoolean(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithBooleanValue(DataTable table, string columnName, bool searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetBoolean(row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithBooleanValue(IEnumerable<DataRow> rows, string columnName, bool searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        if (rowVals[columnName].AsBoolean == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region DateTime

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithDateTimeValue(DataView dv, string columnName, DateTime searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetDateTime(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithDateTimeValue(DataTable table, string columnName, DateTime searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetDateTime(row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithDateTimeValue(IEnumerable<DataRow> rows, string columnName, DateTime searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        if (rowVals[columnName].AsDateTime == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region TimeSpan

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithTimeSpanValue(DataView dv, string columnName, TimeSpan searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetTimeSpan(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithTimeSpanValue(DataTable table, string columnName, TimeSpan searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetTimeSpan(row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithTimeSpanValue(IEnumerable<DataRow> rows, string columnName, TimeSpan searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        if (rowVals[columnName].AsTimeSpan == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region Guid

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithGuidValue(DataView dv, string columnName, Guid searchValue)
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetGuid(drv.Row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithGuidValue(DataTable table, string columnName, Guid searchValue)
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetGuid(row[p]) == searchValue)
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithGuidValue(IEnumerable<DataRow> rows, string columnName, Guid searchValue)
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        if (rowVals[columnName].AsGuid == searchValue)
          cnt++;
      }
      return cnt;
    }

    #endregion

    #region Enum

    /// <summary>
    /// Сосчитать количество строк в просмотре с заданным значением поля 
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="dv">Просмотр</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithEnumValue<T>(DataView dv, string columnName, T searchValue)
      where T : struct
    {
      if (dv == null)
        return 0;
      if (dv.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(dv.Table, columnName);
      int cnt = 0;
      foreach (DataRowView drv in dv)
      {
        if (DataTools.GetEnum<T>(drv.Row[p]).Equals(searchValue))
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в таблице с заданным значением поля 
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithEnumValue<T>(DataTable table, string columnName, T searchValue)
       where T : struct
    {
      if (table == null)
        return 0;
      if (table.Rows.Count == 0)
        return 0;
      int p = GetColumnPosWithCheck(table, columnName);
      int cnt = 0;
      foreach (DataRow row in table.Rows)
      {
        if (row.RowState == DataRowState.Deleted)
          continue;
        if (DataTools.GetEnum<T>(row[p]).Equals(searchValue))
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Сосчитать количество строк в коллекции с заданным значением поля 
    /// </summary>
    /// <typeparam name="T">Тип перечисления</typeparam>
    /// <param name="rows">Коллекция строк. В коллекции могут быть ссылки null</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="searchValue">Значение поля</param>
    /// <returns>Число строк, удовлетворяющих условию</returns>
    public static int GetRowCountWithEnumValue<T>(IEnumerable<DataRow> rows, string columnName, T searchValue)
      where T : struct
    {
      if (rows == null)
        return 0;
      // Строки могут относиться к разным таблицам
      DataRowValues rowVals = new DataRowValues();
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        if (row == null)
          continue;
        if (row.RowState == DataRowState.Deleted)
          continue;
        rowVals.CurrentRow = row;
        if (rowVals[columnName].GetEnum<T>().Equals(searchValue))
          cnt++;
      }
      return cnt;
    }

    #endregion

    #endregion
  }
}
