﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace FreeLibSet.Forms
{
  /*
   * Во избежание путаницы RowIndex и ColumnIndex означают индексы строки / столбца
   * в табличном просмотре (а не в выделенной области)
   * RowOffset и ColumnOffset означают позиции в выбранной области
   */

  #region Перечисление EFPDataGridViewRectAreaCreation

  /// <summary>
  /// Используется конструктором класса EFPDataGridViewRectArea
  /// </summary>
  public enum EFPDataGridViewRectAreaCreation
  {
    /// <summary>
    /// В область должны войти видимые ячейки просмотра
    /// </summary>
    Visible,

    /// <summary>
    /// В область должны войти выбранные ячейки просмотра
    /// </summary>
    Selected
  }

  #endregion

  /// <summary>
  /// Прямоугольная область ячеек, связанная с табличным просмотром
  /// Некоторые строки и столбцы могут быть пропущены (скрытые)
  /// Используется видимый порядок столбцов
  /// </summary>
  public class EFPDataGridViewRectArea
  {
    #region Конструкторы

    /// <summary>
    /// Обобщенная версия конструктора
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="rowIndices">Массив индексов строк. Не может быть null</param>
    /// <param name="columnIndices">Массив индексов столбцов. Не может быть null</param>
    public EFPDataGridViewRectArea(DataGridView control, int[] rowIndices, int[] columnIndices)
    {
#if DEBUG
      if (control == null)
        throw new ArgumentNullException("control");
      if (rowIndices == null)
        throw new ArgumentNullException("rowIndices");
      if (columnIndices == null)
        throw new ArgumentNullException("columnIndices");
#endif

      _Control = control;
      _RowIndices = rowIndices;
      _ColumnIndices = columnIndices;

      InitAuxProps();
    }

    /// <summary>
    /// Создает объект, содержащий только выбранные ячейки
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    public EFPDataGridViewRectArea(DataGridView control)
      : this(control, EFPDataGridViewRectAreaCreation.Selected)
    {
    }

    /// <summary>
    /// Создает объект, содержащий все видимые строки/столбцы или выбранные ячейки
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="mode">Режим выбора ячеек</param>
    public EFPDataGridViewRectArea(DataGridView control, EFPDataGridViewRectAreaCreation mode)
    {
#if DEBUG
      if (control == null)
        throw new ArgumentNullException("control");
#endif
      _Control = control;
      switch (mode)
      {
        case EFPDataGridViewRectAreaCreation.Selected:
          InitSelected();
          break;
        case EFPDataGridViewRectAreaCreation.Visible:
          InitVisible();
          break;
        default:
          throw new ArgumentException("Неизвестный режим: " + mode.ToString(), "mode");
      }
      InitAuxProps();
    }

    private void InitSelected()
    {
      if (Control.AreAllCellsSelected(false))
      {
        InitVisible();
        return;
      }

      if (Control.SelectedRows != null && Control.SelectedRows.Count > 0)
      {
        List<int> lst = new List<int>();
        int CurrIdx = Control.Rows.GetFirstRow(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected);
        while (CurrIdx >= 0)
        {
          lst.Add(CurrIdx);
          CurrIdx = Control.Rows.GetNextRow(CurrIdx, DataGridViewElementStates.Visible | DataGridViewElementStates.Selected);
        }
        _RowIndices = lst.ToArray();

        InitVisibleColumns();

        return;
      }

      if (Control.SelectedColumns != null && Control.SelectedColumns.Count > 0)
      {
        InitVisibleRows();

        // 17.03.2016
        // Не реализовано в mono
        // Но DataGridViewRowCollection.GetFirstRow()/GetNewxtRow() - реализовано
        /*
        List<int> lst = new List<int>();
        DataGridViewColumn CurrCol = Control.Columns.GetFirstColumn(DataGridViewElementStates.Visible | DataGridViewElementStates.Selected, DataGridViewElementStates.None);
        while (CurrCol != null)
        {
          lst.Add(CurrCol.Index);
          CurrCol = Control.Columns.GetNextColumn(CurrCol, DataGridViewElementStates.Visible | DataGridViewElementStates.Selected, DataGridViewElementStates.None);
        }

        FColumnIndices = lst.ToArray();
         * */

        _ColumnIndices = WinFormsTools.GetColumnIndices(WinFormsTools.GetOrderedSelectedColumns(Control));
        return;
      }

      if (Control.SelectedCells != null && Control.SelectedCells.Count > 0)
      {
        List<int> lstRows = new List<int>();
        // Не так просто. Надо учитывать видимый порядок столбцов
        List<DataGridViewColumn> lstCols = new List<DataGridViewColumn>();
        for (int i = 0; i < Control.SelectedCells.Count; i++)
        {
          if (!Control.SelectedCells[i].Visible)
            continue; // скрытая строка или столбец

          int RowIndex = Control.SelectedCells[i].RowIndex;
          int ColumnIndex = Control.SelectedCells[i].ColumnIndex;

          if (lstRows.IndexOf(RowIndex) < 0)
            lstRows.Add(RowIndex);
          if (lstCols.IndexOf(Control.Columns[ColumnIndex]) < 0)
            lstCols.Add(Control.Columns[ColumnIndex]);
        }

        lstRows.Sort();
        lstCols.Sort(new Comparison<DataGridViewColumn>(CompareColumns));


        _RowIndices = lstRows.ToArray();
        _ColumnIndices = new int[lstCols.Count];
        for (int i = 0; i < lstCols.Count; i++)
          _ColumnIndices[i] = lstCols[i].Index;
        return;
      }

      if (Control.CurrentCell != null)
      {
        _RowIndices = new int[1] { Control.CurrentCell.RowIndex };
        _ColumnIndices = new int[1] { Control.CurrentCell.ColumnIndex };
        return;
      }

      // Нет ничего
      _RowIndices = DataTools.EmptyInts;
      _ColumnIndices = DataTools.EmptyInts;
    }

    private void InitVisible()
    {
      InitVisibleRows();
      InitVisibleColumns();
    }

    private void InitVisibleRows()
    {
      List<int> lst = new List<int>();
      int CurrIdx = Control.Rows.GetFirstRow(DataGridViewElementStates.Visible);
      while (CurrIdx >= 0)
      {
        lst.Add(CurrIdx);
        CurrIdx = Control.Rows.GetNextRow(CurrIdx, DataGridViewElementStates.Visible);
      }

      _RowIndices = lst.ToArray();
    }

    private void InitVisibleColumns()
    {
      // 17.05.2016
      // В mono не реализованы методы DatAGridViewColumnCollection.GetFirstColumn() / GetNextColumn()
      /*
      List<int> lst = new List<int>();
      DataGridViewColumn CurrCol = Control.Columns.GetFirstColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None);
      while (CurrCol != null)
      {
        lst.Add(CurrCol.Index);
        CurrCol = Control.Columns.GetNextColumn(CurrCol, DataGridViewElementStates.Visible, DataGridViewElementStates.None);
      }

      FColumnIndices = lst.ToArray();
       * */

      _ColumnIndices = WinFormsTools.GetColumnIndices(WinFormsTools.GetOrderedVisibleColumns(Control));
    }

    private static int CompareColumns(DataGridViewColumn column1, DataGridViewColumn column2)
    {
      if (column1.DisplayIndex < column2.DisplayIndex)
        return -1;
      if (column1.DisplayIndex > column2.DisplayIndex)
        return +1;
      if (column1.Index < column2.Index)
        return -1;
      if (column1.Index > column2.Index)
        return +1;
      return 0;
    }

    private void InitAuxProps()
    {
      _Rows = new GridRowArray(this);
      _Columns = new GridColumnArray(this);
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Табличный просмотр.
    /// </summary>
    public DataGridView Control { get { return _Control; } }
    private DataGridView _Control;

    /// <summary>
    /// Индексы строк.
    /// </summary>
    public int[] RowIndices { get { return _RowIndices; } }
    private int[] _RowIndices;

    /// <summary>
    /// Индексы столбцов (по свойству Column.Index)
    /// Так как столбцы могут быть переставлены пользователем, элементы в массиве
    /// могут идти не по порядку.
    /// </summary>
    public int[] ColumnIndices { get { return _ColumnIndices; } }
    private int[] _ColumnIndices;

    /// <summary>
    /// Возвращает true, если область не содержит ни одной ячейки (RowCount и ColumnCount=0).
    /// </summary>
    public bool IsEmpty
    {
      get { return _RowIndices.Length == 0 || _ColumnIndices.Length == 0; }
    }

    /// <summary>
    /// Возвращает количество строк
    /// </summary>
    public int RowCount { get { return _RowIndices.Length; } }


    /// <summary>
    /// Возвращает количество столбцов
    /// </summary>
    public int ColumnCount { get { return _ColumnIndices.Length; } }

    /// <summary>
    /// Текст для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Строк: " + RowCount.ToString() + ", столбцов: " + ColumnCount.ToString();
    }

    #endregion

    #region Доступ к объектам DataGridView

    /// <summary>
    /// Коллекция для реализация свойства EFPDataGridViewRectArea.Rows.
    /// </summary>
    public class GridRowArray:IEnumerable<DataGridViewRow>
    {
      #region Конструктор

      internal GridRowArray(EFPDataGridViewRectArea owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Свойства

      private EFPDataGridViewRectArea _Owner;

      /// <summary>
      /// Возвращает количество строк в выборке
      /// </summary>
      public int Count { get { return _Owner.RowIndices.Length; } }

      /// <summary>
      /// Доступ к выбранной строке просмотра по индексу в EFPDataGridViewRectArea (а не DataGridView) 
      /// </summary>
      /// <param name="rowOffset">Смещение в коллекции строк от 0 до (Count-1)</param>
      /// <returns>Объект строки DataGridViewRow</returns>
      public DataGridViewRow this[int rowOffset]
      {
        get
        {
          int RowIndex = _Owner.RowIndices[rowOffset];
          return _Owner.Control.Rows[RowIndex];
        }
      }

      #endregion

      #region IEnumerable

      /// <summary>
      /// Возвращает перечислитель по строкам табличного просмотра DataGridViewRow, входящим в выборку
      /// </summary>
      /// <returns>Перечислитель</returns>
      public IEnumerator<DataGridViewRow> GetEnumerator()
      {
        return new RowEnumerator(_Owner);
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return new RowEnumerator(_Owner);
      }

      #endregion
    }

    /// <summary>
    /// Перечислитель для строк
    /// </summary>
    private class RowEnumerator : IEnumerator<DataGridViewRow>
    {
      #region Конструктор

      public RowEnumerator(EFPDataGridViewRectArea owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Поля

      private EFPDataGridViewRectArea _Owner;

      private int _CurrentIndex;

      #endregion

      #region IEnumerator

      public void Reset()
      {
        _CurrentIndex = -1;
      }

      public bool MoveNext()
      {
        _CurrentIndex++;
        return _CurrentIndex < _Owner.RowCount;
      }

      public DataGridViewRow Current
      {
        get { return _Owner.Rows[_CurrentIndex]; }
      }

      object System.Collections.IEnumerator.Current
      {
        get { return _Owner.Rows[_CurrentIndex]; }
      }

      public void Dispose()
      {
      }

      #endregion
    }

    /// <summary>
    /// Коллекция строк, входящих в выборку
    /// </summary>
    public GridRowArray Rows { get { return _Rows; } }
    private GridRowArray _Rows;

    /// <summary>
    /// Коллекция для реализация свойства EFPDataGridViewRectArea.Columns.
    /// </summary>
    public class GridColumnArray : IEnumerable<DataGridViewColumn>
    {
      #region Конструктор

      internal GridColumnArray(EFPDataGridViewRectArea owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Свойства

      private EFPDataGridViewRectArea _Owner;

      /// <summary>
      /// Возвращает количество столбов в выборке
      /// </summary>
      public int Count { get { return _Owner.ColumnIndices.Length; } }

      /// <summary>
      /// Доступ к выбранному столбцу просмотра по индексу в EFPDataGridViewRectArea (а не DataGridView) 
      /// </summary>
      /// <param name="columnOffset">Смещение в коллекции столбцов от 0 до (Count-1)</param>
      /// <returns>Объект строки DataGridViewColumn</returns>
      public DataGridViewColumn this[int columnOffset]
      {
        get
        {
          int ColumnIndex = _Owner.ColumnIndices[columnOffset];
          return _Owner.Control.Columns[ColumnIndex];
        }
      }

      #endregion

      #region IEnumerator

      /// <summary>
      /// Возвращает перечислитель по столбцам табличного просмотра DataGridViewColumn (не EFPDataGridViewColumn!), входящим в выборку
      /// </summary>
      /// <returns>Перечислитель</returns>
      public IEnumerator<DataGridViewColumn> GetEnumerator()
      {
        return new ColumnEnumerator(_Owner);
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Перечислитель для столбцов
    /// </summary>
    private class ColumnEnumerator : IEnumerator<DataGridViewColumn>
    {
      #region Конструктор

      public ColumnEnumerator(EFPDataGridViewRectArea owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Поля

      private EFPDataGridViewRectArea _Owner;

      private int _CurrentIndex;

      #endregion

      #region IEnumerator

      public void Reset()
      {
        _CurrentIndex = -1;
      }

      public bool MoveNext()
      {
        _CurrentIndex++;
        return _CurrentIndex < _Owner.ColumnCount;
      }

      public DataGridViewColumn Current
      {
        get { return _Owner.Columns[_CurrentIndex]; }
      }

      object System.Collections.IEnumerator.Current
      {
        get { return _Owner.Columns[_CurrentIndex]; }
      }

      public void Dispose()
      {
      }

      #endregion
    }

    /// <summary>
    /// Коллекция столбцов, входящих в выборку
    /// </summary>
    public GridColumnArray Columns { get { return _Columns; } }
    private GridColumnArray _Columns;

    /// <summary>
    /// Доступ к ячейке таблицы
    /// </summary>
    /// <param name="columnOffset">Смещение в коллекции столбцов выборки (а не в DataGridView.Columns)</param>
    /// <param name="rowOffset">Смещение в коллекции строк выборки (а не в DataGridView.Rows)</param>
    /// <returns>Объект DataGridViewCell </returns>
    public DataGridViewCell this[int columnOffset, int rowOffset]
    {
      get
      {
        int ColumnIndex = ColumnIndices[columnOffset];
        int RowIndex = RowIndices[rowOffset];
        return Control[ColumnIndex, RowIndex];
      }
    }

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Свойство возвращает true, если все ячейки области выбраны, то есть нет "загогулин" в выделении
    /// </summary>
    public bool AreAllCellsSelected
    {
      get
      {
        for (int i = 0; i < RowIndices.Length; i++)
        {
          DataGridViewRow Row = Control.Rows.SharedRow(RowIndices[i]);
          for (int j = 0; j < ColumnIndices.Length; j++)
          {
            if (!Row.Cells[ColumnIndices[j]].Selected)
              return false;
          }
        }
        return true;
      }
    }

    #endregion
  }
}