using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Reporting
{
  /// <summary>
  /// Таблица отчета.
  /// Таблица является невиртуальной и предназначена для заполнения в прикладном коде.
  /// Объект является хранилищем данных.
  /// Для создания таблицы используйте метод <see cref="BRSection.BandCollection.Add(int, int)"/>.
  /// </summary>
  public class BRTable : BRBand
  {
    #region Конструктор

    internal BRTable(BRSection section, int rowCount, int columnCount)
      : base(section, rowCount, columnCount)
    {
      CellValues = new object[rowCount, columnCount];
      _DefaultCellStyle = new BRCellStyleStorage(section.Report.DefaultCellStyle);
    }

    #endregion

    #region Данные в памяти

    internal readonly object[,] CellValues;

    internal object[,][] CellStyleData;

    internal object[][] RowCellStyleData;

    internal object[][] ColumnCellStyleData;

    /// <summary>
    /// Стили ячеек таблицы по умолчанию.
    /// Например, здесь могут быть заданы границы ячеек и отступы
    /// </summary>
    public BRCellStyle DefaultCellStyle { get { return _DefaultCellStyle; } }
    private BRCellStyleStorage _DefaultCellStyle;

    internal BRRowColumnData[] RowInfoData;

    internal BRRowColumnData[] ColumnInfoData;

    /// <summary>
    /// Список объединений ячеек
    /// </summary>
    internal List<BRRange> MergeList;

    /// <summary>
    /// Ключ - пара {НомерСтроки+НомерСтолбца}, значение - индекс в массиве MergeList
    /// </summary>
    internal Dictionary<ulong, int> CellMergeDict;

    internal static ulong GetCellMergeKey(int rowIndex, int columnIndex)
    {
      ulong v1 = (ulong)rowIndex << 32;
      ulong v2 = (ulong)columnIndex;
      return v1 | v2;
    }

    #endregion

    #region Селектор для доступа к данным

    /// <summary>
    /// Создает новый селектор <see cref="BRTableCellSelector"/>.
    /// </summary>
    /// <returns>Новый селектор</returns>
    public override BRSelector CreateSelector()
    {
      return new BRTableCellSelector(this);
    }

    /// <summary>
    /// Основной селектор, используемый в прикладном коде для заполнения таблицы
    /// </summary>
    public BRTableCellSelector Cells
    {
      get
      {
        if (_Cells == null)
          _Cells = new BRTableCellSelector(this);
        return _Cells;
      }
    }
    private BRTableCellSelector _Cells;

    #endregion

    #region Альтернативные методы установки значений

    /// <summary>
    /// Установка значения без использования селектора
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <param name="value">Значение</param>
    public void SetValue(int rowIndex, int columnIndex, object value)
    {
      CellValues[rowIndex, columnIndex] = value;
    }

    private BRSelector InternalSelector
    {
      get
      {
        if (_InternalSelector == null)
          _InternalSelector = new BRTableCellSelector(this);
        return _InternalSelector;
      }
    }
    private BRSelector _InternalSelector;

    /// <summary>
    /// Установка формата <see cref="BRCellStyle.Format"/> без использования селектора
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <param name="format">Формат</param>
    public void SetFormat(int rowIndex, int columnIndex, string format)
    {
      InternalSelector.RowIndex = rowIndex;
      InternalSelector.ColumnIndex = columnIndex;
      InternalSelector.CellStyle.Format = format;
    }

    #endregion
  }

  /// <summary>
  /// Расширенны селектор для <see cref="BRTable"/>.
  /// Реализует методы для установки значений свойств, а не только для чтения
  /// </summary>
  public sealed class BRTableCellSelector : BRSelector
  {
    /// <summary>
    /// Создает селектор
    /// </summary>
    /// <param name="table">Таблица</param>
    public BRTableCellSelector(BRTable table)
      : base(table)
    {
      _Table = table;
      _CellStyle = new BRInternalCellStyle(this);
      _RowCellStyle = new BRInternalRowCellStyle(this);
      _ColumnCellStyle = new BRInternalColumnCellStyle(this);
      _RowInfo = new BRInternalRowInfo(this);
      _ColumnInfo = new BRInternalColumnInfo(this);
    }

    /// <summary>
    /// Таблица, к которой относится селектор
    /// </summary>
    public BRTable Table { get { return _Table; } }
    private readonly BRTable _Table;

    #region Данные текущей ячейки

    /// <summary>
    /// Значение текущей ячейки.
    /// Допускается чтение и установка свойства.
    /// </summary>
    public override object Value
    {
      get { return _Table.CellValues[RowIndex, ColumnIndex]; }
      set { _Table.CellValues[RowIndex, ColumnIndex] = value; }
    }

    private class BRInternalCellStyle : BRCellStyle
    {
      internal BRInternalCellStyle(BRTableCellSelector selector)
      {
        _Selector = selector;
      }
      private BRTableCellSelector _Selector;

      internal override object GetValue(BRCellStyle caller, int index)
      {
        if (_Selector.Table.CellStyleData != null)
        {
          object[] sd = _Selector.Table.CellStyleData[_Selector.RowIndex, _Selector.ColumnIndex];
          if (sd != null)
          {
            if (sd[index] != null)
              return sd[index];
          }
        }
        if (_Selector.Table.RowCellStyleData != null)
        {
          object[] sd = _Selector.Table.RowCellStyleData[_Selector.RowIndex];
          if (sd != null)
          {
            if (sd[index] != null)
              return sd[index];
          }
        }
        if (_Selector.Table.ColumnCellStyleData != null)
        {
          object[] sd = _Selector.Table.ColumnCellStyleData[_Selector.ColumnIndex];
          if (sd != null)
          {
            if (sd[index] != null)
              return sd[index];
          }
        }
        return _Selector.Table.DefaultCellStyle.GetValue(caller, index);
      }

      internal override void SetValue(int index, object value)
      {
        if (_Selector.Table.CellStyleData == null)
        {
          if (value == null)
            return;
          _Selector.Table.CellStyleData = new object[_Selector.Table.RowCount, _Selector.Table.ColumnCount][];
        }
        if (_Selector.Table.CellStyleData[_Selector.RowIndex, _Selector.ColumnIndex] == null)
        {
          if (value == null)
            return;
          _Selector.Table.CellStyleData[_Selector.RowIndex, _Selector.ColumnIndex] = new object[Array_Size];
        }
        _Selector.Table.CellStyleData[_Selector.RowIndex, _Selector.ColumnIndex][index] = value;
      }

      internal override BRReport Report
      {
        get
        {
          return _Selector.Table.Report;
        }
      }
    }

    /// <summary>
    /// Стиль текущей ячейки
    /// </summary>
    public override BRCellStyle CellStyle { get { return _CellStyle; } }
    private BRInternalCellStyle _CellStyle;

    #endregion

    #region Обобщенные стили всех ячеек строки и столбца

    private class BRInternalRowCellStyle : BRCellStyle
    {
      internal BRInternalRowCellStyle(BRTableCellSelector selector)
      {
        _Selector = selector;
      }
      private BRTableCellSelector _Selector;

      internal override object GetValue(BRCellStyle caller, int index)
      {
        if (_Selector.Table.RowCellStyleData != null)
        {
          object[] sd = _Selector.Table.RowCellStyleData[_Selector.RowIndex];
          if (sd != null)
          {
            if (sd[index] != null)
              return sd[index];
          }
        }
        return _Selector.Table.DefaultCellStyle.GetValue(caller, index);
      }

      internal override void SetValue(int index, object value)
      {
        if (_Selector.Table.RowCellStyleData == null)
        {
          if (value == null)
            return;
          _Selector.Table.RowCellStyleData = new object[_Selector.Table.RowCount][];
        }
        if (_Selector.Table.RowCellStyleData[_Selector.RowIndex] == null)
        {
          if (value == null)
            return;
          _Selector.Table.RowCellStyleData[_Selector.RowIndex] = new object[Array_Size];
        }
        _Selector.Table.RowCellStyleData[_Selector.RowIndex][index] = value;
      }

      internal override BRReport Report { get { return _Selector.Table.Report; } }
    }

    private class BRInternalColumnCellStyle : BRCellStyle
    {
      internal BRInternalColumnCellStyle(BRTableCellSelector selector)
      {
        _Selector = selector;
      }
      private BRTableCellSelector _Selector;

      internal override object GetValue(BRCellStyle caller, int index)
      {
        if (_Selector.Table.ColumnCellStyleData != null)
        {
          object[] sd = _Selector.Table.ColumnCellStyleData[_Selector.ColumnIndex];
          if (sd != null)
          {
            if (sd[index] != null)
              return sd[index];
          }
        }
        return _Selector.Table.DefaultCellStyle.GetValue(caller, index);
      }

      internal override void SetValue(int index, object value)
      {
        if (_Selector.Table.ColumnCellStyleData == null)
        {
          if (value == null)
            return;
          _Selector.Table.ColumnCellStyleData = new object[_Selector.Table.ColumnCount][];
        }
        if (_Selector.Table.ColumnCellStyleData[_Selector.ColumnIndex] == null)
        {
          if (value == null)
            return;
          _Selector.Table.ColumnCellStyleData[_Selector.ColumnIndex] = new object[Array_Size];
        }
        _Selector.Table.ColumnCellStyleData[_Selector.ColumnIndex][index] = value;
      }

      internal override BRReport Report { get { return _Selector.Table.Report; } }
    }

    /// <summary>
    /// Стили ячеек, входящих в строку.
    /// Можно использовать, например, для форматирования заголовков или организации полосатой раскраски строк
    /// </summary>
    public BRCellStyle RowCellStyle { get { return _RowCellStyle; } }
    private BRInternalRowCellStyle _RowCellStyle;

    /// <summary>
    /// Стили ячеек, входящих в столбец.
    /// Можно использовать, например, для задания горизонтального выравнивания.
    /// </summary>
    public BRCellStyle ColumnCellStyle { get { return _ColumnCellStyle; } }
    private BRInternalColumnCellStyle _ColumnCellStyle;

    #endregion

    #region Информация о строке и столбце

    internal sealed class BRInternalRowInfo : BRRowInfo
    {
      #region Конструктор

      public BRInternalRowInfo(BRTableCellSelector selector)
      {
        _Selector = selector;
      }

      private BRTableCellSelector _Selector;

      #endregion

      #region Переопределенные методы

      internal override int GetSize()
      {
        if (_Selector.Table.RowInfoData == null)
          return 0;
        else
          return _Selector.Table.RowInfoData[_Selector.RowIndex].GetSize();
      }

      internal override void SetSize(int value)
      {
        if (_Selector.Table.RowInfoData == null)
        {
          if (value == 0)
            return;
          _Selector.Table.RowInfoData = new BRRowColumnData[_Selector.Table.RowCount];
        }

        BRRowColumnData data = _Selector.Table.RowInfoData[_Selector.RowIndex];
        data.SetSize(value);
        _Selector.Table.RowInfoData[_Selector.RowIndex] = data;
      }

      internal override bool GetFlag(int flag)
      {
        if (_Selector.Table.RowInfoData == null)
          return false;
        else
          return _Selector.Table.RowInfoData[_Selector.RowIndex].GetFlag(flag);
      }

      internal override void SetFlag(int flag, bool value)
      {
        if (_Selector.Table.RowInfoData == null)
        {
          if (!value)
            return;
          _Selector.Table.RowInfoData = new BRRowColumnData[_Selector.Table.RowCount];
        }

        BRRowColumnData data = _Selector.Table.RowInfoData[_Selector.RowIndex];
        data.SetFlag(flag, value);
        _Selector.Table.RowInfoData[_Selector.RowIndex] = data;
      }

      internal override BRReport Report { get { return _Selector.Band.Report; } }

      #endregion
    }


    internal sealed class BRInternalColumnInfo : BRColumnInfo
    {
      #region Конструктор

      public BRInternalColumnInfo(BRTableCellSelector selector)
      {
        _Selector = selector;
      }

      private BRTableCellSelector _Selector;

      #endregion

      #region Переопределенные методы

      internal override int GetSize()
      {
        if (_Selector.Table.ColumnInfoData == null)
          return 0;
        else
          return _Selector.Table.ColumnInfoData[_Selector.ColumnIndex].GetSize();
      }

      internal override void SetSize(int value)
      {
        if (_Selector.Table.ColumnInfoData == null)
        {
          if (value == 0)
            return;
          _Selector.Table.ColumnInfoData = new BRRowColumnData[_Selector.Table.ColumnCount];
        }

        BRRowColumnData data = _Selector.Table.ColumnInfoData[_Selector.ColumnIndex];
        data.SetSize(value);
        _Selector.Table.ColumnInfoData[_Selector.ColumnIndex] = data;
      }

      internal override bool GetFlag(int flag)
      {
        if (_Selector.Table.ColumnInfoData == null)
          return false;
        else
          return _Selector.Table.ColumnInfoData[_Selector.ColumnIndex].GetFlag(flag);
      }

      internal override void SetFlag(int flag, bool value)
      {
        if (_Selector.Table.ColumnInfoData == null)
        {
          if (!value)
            return;
          _Selector.Table.ColumnInfoData = new BRRowColumnData[_Selector.Table.ColumnCount];
        }

        BRRowColumnData data = _Selector.Table.ColumnInfoData[_Selector.ColumnIndex];
        data.SetFlag(flag, value);
        _Selector.Table.ColumnInfoData[_Selector.ColumnIndex] = data;
      }

      internal override BRReport Report { get { return _Selector.Band.Report; } }

      #endregion
    }

    /// <summary>
    /// Параметры строки, задаваемой <see cref="BRSelector.RowIndex"/>.
    /// Используйте для задания высоты строки и других параметров
    /// </summary>
    public override BRRowInfo RowInfo { get { return _RowInfo; } }
    private BRInternalRowInfo _RowInfo;

    /// <summary>
    /// Параметры столбца, задаваемого <see cref="BRSelector.ColumnIndex"/>.
    /// Используйте для задания ширины столбца и других параметров
    /// </summary>
    public override BRColumnInfo ColumnInfo { get { return _ColumnInfo; } }
    private BRInternalColumnInfo _ColumnInfo;

    #endregion

    #region Объединение ячеек

    /// <summary>
    /// Возвращает объединение, в который входит текущая ячейка.
    /// Если ячейка не входит в объединение, возвращает блок, состоящий из одной ячейки.
    /// </summary>
    public override BRRange MergeInfo
    {
      get
      {
        if (Table.CellMergeDict != null)
        {
          int mergeIndex;
          if (Table.CellMergeDict.TryGetValue(BRTable.GetCellMergeKey(RowIndex, ColumnIndex), out mergeIndex))
            return Table.MergeList[mergeIndex];
        }
        return base.MergeInfo;
      }
    }

    /// <summary>
    /// Выполнить объединение ячеек.
    /// На момент вызова должна быть активна верхняя левая ячейка будущего объединения.
    /// Все объединяемые ячейки не должны входить в какие-либо другие объединения.
    /// </summary>
    /// <param name="rowSpan">Количество объединяемых строк</param>
    /// <param name="columnSpan">Количество объединяемых столбцов</param>
    public void Merge(int rowSpan, int columnSpan)
    {
      if (rowSpan < 1 | (RowIndex + rowSpan - 1) >= Table.RowCount)
        throw new ArgumentOutOfRangeException("rowSpan");
      if (columnSpan < 1 | (ColumnIndex + columnSpan - 1) >= Table.ColumnCount)
        throw new ArgumentOutOfRangeException("columnSpan");


      if (Table.MergeList == null)
      {
        Table.MergeList = new List<BRRange>();
        Table.CellMergeDict = new Dictionary<ulong, int>();
      }
      BRRange range = new BRRange(RowIndex, ColumnIndex, rowSpan, columnSpan);
      // Проверяем наличие ошибок
      for (int iRow = range.FirstRowIndex; iRow <= range.LastRowIndex; iRow++)
      {
        for (int iCol = range.FirstColumnIndex; iCol <= range.LastColumnIndex; iCol++)
        {
          if (Table.CellMergeDict.ContainsKey(BRTable.GetCellMergeKey(iRow, iCol)))
            throw new InvalidOperationException("Ячейка с индексом [" + iRow.ToString() + "," + iCol.ToString() + "] уже входит в состав другого объединения");
        }
      }

      Table.MergeList.Add(range);
      int mergeIndex = Table.MergeList.Count - 1;
      for (int iRow = range.FirstRowIndex; iRow <= range.LastRowIndex; iRow++)
      {
        for (int iCol = range.FirstColumnIndex; iCol <= range.LastColumnIndex; iCol++)
        {
          Table.CellMergeDict.Add(BRTable.GetCellMergeKey(iRow, iCol), mergeIndex);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Виртуальная таблица. Абстрактный класс.
  /// Реализации предоставляются хранилищами данных, например, табличным просмотром.
  /// </summary>
  public abstract class BRVirtualTable : BRBand
  {
    #region Конструктор

    /// <summary>
    /// Инициализация таблицы
    /// </summary>
    /// <param name="section">Секция</param>
    /// <param name="rowCount">Количество строк</param>
    /// <param name="columnCount">Количество столбцов</param>
    public BRVirtualTable(BRSection section, int rowCount, int columnCount)
      : base(section, rowCount, columnCount)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Создает объект <see cref="BRVirtualSelector"/> для доступа на чтение к значениям ячеек
    /// </summary>
    /// <returns>Новый селектор</returns>
    public override BRSelector CreateSelector()
    {
      return new BRVirtualSelector(this);
    }

    #endregion

    #region Абстрактные методы

    /// <summary>
    /// Возвращает значение ячейки из источника данных
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <returns>Значение</returns>
    internal protected abstract object GetValue(int rowIndex, int columnIndex);

    /// <summary>
    /// Этот метод вызывается из <see cref="BRVirtualSelector"/> для получения стилей текущей ячекйи
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <param name="style">Заполняемые стили</param>
    internal protected abstract void FillCellStyle(int rowIndex, int columnIndex, BRCellStyle style);

    /// <summary>
    /// Этот метод вызывается из <see cref="BRVirtualSelector"/> для получения параметров текущей строки
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="info">Заполняемые параметры</param>
    internal protected virtual void FillRowInfo(int rowIndex, BRRowInfo info) { }

    /// <summary>
    /// Этот метод вызывается из <see cref="BRVirtualSelector"/> для получения параметров текущего столбца
    /// </summary>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <param name="info">Заполняемые параметры</param>
    internal protected abstract void FillColumnInfo(int columnIndex, BRColumnInfo info);

    /// <summary>
    /// Этот метод вызывается из <see cref="BRVirtualSelector"/> для получения блока объединения текущей ячейки.
    /// Непереопределенный метод возвращает блок из одной ячейки
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <returns>Блок объединения</returns>
    internal protected virtual BRRange GetMergeInfo(int rowIndex, int columnIndex)
    {
      return new BRRange(rowIndex, columnIndex, 1, 1);
    }

    #endregion
  }

  /// <summary>
  /// Селектор для виртуальной таблицы данных <see cref="BRVirtualTable"/>.
  /// Предоставляет доступ только для чтения.
  /// Выполняет запросы к абстрактным методам в <see cref="BRVirtualTable"/>, предоставляя временные хранилища для атрибутов ячейки,
  /// параметров строк и столбцов
  /// </summary>
  public sealed class BRVirtualSelector : BRSelector
  {
    #region Конструктор

    /// <summary>
    /// Создает селектор
    /// </summary>
    /// <param name="table">Виртуальная таблица для извлечения данных</param>
    public BRVirtualSelector(BRVirtualTable table)
      : base(table)
    {
      _Table = table;
      _CellStyle = new BRCellStyleStorage(table.Report.DefaultCellStyle);
      _RowInfo = new BRRowInfoStorage(table.Report);
      _ColumnInfo = new BRColumnInfoStorage(table.Report);
    }

    /// <summary>
    /// Виртуальная таблица для извлечения данных
    /// </summary>
    public BRVirtualTable Table { get { return _Table; } }
    private readonly BRVirtualTable _Table;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Обновляет данные при изменении индекса строки
    /// </summary>
    protected override void OnRowIndexChanged()
    {
      _CellStyleReady = false;
      _RowInfoReady = false;
      base.OnRowIndexChanged();
    }

    /// <summary>
    /// Обновляет данные при изменении индекса столбца
    /// </summary>
    protected override void OnColumnIndexChanged()
    {
      _CellStyleReady = false;
      _ColumnInfoReady = false;
      base.OnColumnIndexChanged();
    }

    #endregion

    #region Доступ к значениям

    /// <summary>
    /// Значение текущей ячейки.
    /// Установка свойства не допускается.
    /// </summary>
    public override object Value
    {
      get { return _Table.GetValue(RowIndex, ColumnIndex); }
      set { throw new InvalidOperationException(); }
    }

    /// <summary>
    /// Стиль текущей ячейки
    /// </summary>
    public override BRCellStyle CellStyle
    {
      get
      {
        if (!_CellStyleReady)
        {
          _CellStyle.Clear();
          _Table.FillCellStyle(RowIndex, ColumnIndex, _CellStyle);
          _CellStyleReady = true;
        }
        return _CellStyle;
      }
    }
    private BRCellStyleStorage _CellStyle;
    private bool _CellStyleReady;

    /// <summary>
    /// Информация о текущей строке
    /// </summary>
    public override BRRowInfo RowInfo
    {
      get
      {
        if (!_RowInfoReady)
        {
          _RowInfo.Clear();
          _Table.FillRowInfo(RowIndex, _RowInfo);
          _RowInfoReady = true;
        }
        return _RowInfo;
      }
    }
    private BRRowInfoStorage _RowInfo;
    private bool _RowInfoReady;

    /// <summary>
    /// Информация о текущем столбце
    /// </summary>
    public override BRColumnInfo ColumnInfo
    {
      get
      {
        if (!_ColumnInfoReady)
        {
          _ColumnInfo.Clear();
          _Table.FillColumnInfo(ColumnIndex, _ColumnInfo);
          _ColumnInfoReady = true;
        }
        return _ColumnInfo;
      }
    }
    private BRColumnInfoStorage _ColumnInfo;
    private bool _ColumnInfoReady;

    /// <summary>
    /// Информация о блоке объединения, в который входит текущая ячейка
    /// </summary>
    public override BRRange MergeInfo
    {
      get
      {
        BRRange r =_Table.GetMergeInfo(RowIndex, ColumnIndex);
        if (r.FirstRowIndex > RowIndex || r.LastRowIndex < RowIndex || r.FirstColumnIndex > ColumnIndex || r.LastColumnIndex < ColumnIndex)
          throw new FreeLibSet.Core.BugException("Для ячейки RowIndex="+RowIndex+", ColumnIndex="+ColumnIndex+" возвращена неправильная область объединения: "+r.ToString());
        return r;
      }
    }

    #endregion
  }
}
