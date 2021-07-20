using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Globalization;
using AgeyevAV;
using AgeyevAV.DBF;
using AgeyevAV.TextMasks;
using AgeyevAV.ExtForms.NodeControls;

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

/*
 * Дополнительные описания для стоблцов TreeViewAdv
 * TreeViewAdv определяет объекты TreeColumn, у которых есть заголовок, признак видимости, сортировка.
 * Также есть объекты NodeControl, которые выводят значения
 */
namespace AgeyevAV.ExtForms
{

#if XXX
  #region GridProducerColumnCellToolTipTextNeededEventHandler

  public class EFPDataTreeViewCellToolTipTextNeededEventArgs : EventArgs
  {
  #region Конструктор

    public EFPDataTreeViewCellToolTipTextNeededEventArgs(EFPDataTreeViewColumn Column, int RowIndex)
    {
      FColumn = Column;
      FRowIndex = RowIndex;
      ToolTipText = String.Empty;
    }

  #endregion

  #region Свойства

    public EFPDataTreeViewColumn Column { get { return FColumn; } }
    private EFPDataTreeViewColumn FColumn;

    public EFPDataTreeView ControlProvider { get { return FColumn.ControlProvider; } }

    public int RowIndex { get { return FRowIndex; } }
    private int FRowIndex;

    //public DataRow Row { get { return ControlProvider.GetDataRow(FRowIndex); } }

    public string ColumnName { get { return FColumn.Name; } }

    /// <summary>
    /// Сюда должен быть помещен текст подсказки
    /// </summary>
    public string ToolTipText;

  #endregion
  }

  public delegate void EFPDataTreeViewCellToolTipTextNeededEventHandler(object Sender,
    EFPDataTreeViewCellToolTipTextNeededEventArgs Args);

  #endregion

#endif

  /// <summary>
  /// Объект расширенного описания столбца в EFPDataTreeView
  /// </summary>
  public class EFPDataTreeViewColumn
  {
    #region Защищенный конструктор

    internal EFPDataTreeViewColumn(EFPDataTreeView controlProvider, TreeColumn treeColumn, NodeControl nodeControl, string name)
    {
      _ControlProvider = controlProvider;
      _TreeColumn = treeColumn;
      _NodeControl = nodeControl;
      _Name = name;

      _CanIncSearch = false;

      _ColorType = EFPDataGridViewColorType.Normal;
      _LeftBorder = EFPDataGridViewBorderStyle.Default;
      _RightBorder = EFPDataGridViewBorderStyle.Default;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект - владелец
    /// </summary>
    public EFPDataTreeView ControlProvider { get { return _ControlProvider; } }
    private EFPDataTreeView _ControlProvider;

    /// <summary>
    /// Столбец древовидного просмотра
    /// </summary>
    public TreeColumn TreeColumn { get { return _TreeColumn; } }
    private TreeColumn _TreeColumn;

    /// <summary>
    /// Объект работы с ячейками столбца
    /// </summary>
    public NodeControl NodeControl { get { return _NodeControl; } }
    private NodeControl _NodeControl;

    /// <summary>
    /// Имя столбца. В отличие от свойства DataGridViewColumn.GroupName, всегда определено
    /// </summary>
    public string Name { get { return _Name; } }
    private string _Name;

    /// <summary>
    /// Пользовательские данные (вместо занятого DataGridViewColumn.Tag)
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    /// <summary>
    /// Если столбец был создан с помощью GridProducer, то ссылка на генератор столбца,
    /// иначе - null.
    /// </summary>
    public IEFPGridProducerColumn ColumnProducer { get { return _ColumnProducer; } set { _ColumnProducer = value; } }
    private IEFPGridProducerColumn _ColumnProducer;

    #endregion

    #region Поиск по первым буквам

    /// <summary>
    /// Установка в true разрешает быстрый поиск по первым буквам для этого столбца
    /// </summary>
    public bool CanIncSearch { get { return _CanIncSearch; } set { _CanIncSearch = value; } }
    private bool _CanIncSearch;

    /// <summary>
    /// Используется при поиске по первым буквам
    /// </summary>
    public IMaskProvider MaskProvider { get { return _MaskProvider; } set { _MaskProvider = value; } }
    private IMaskProvider _MaskProvider;

#if XXX
    /// <summary>
    /// Реализация поиска по буквам для столбца
    /// </summary>
    /// <param name="SearchStr">Строка, которую надо найти</param>
    /// <returns>true, если поиск успешно выполнен</returns>
    internal bool PerformIncSearch(string SearchStr, bool NextSearch)
    {
      // Для пустой строки поиска позиционируемся на первую строку
      if (String.IsNullOrEmpty(SearchStr))
      {
        if (NextSearch)
        {
          if (ControlProvider.Control.Rows.Count > 1)
          {
            if (ControlProvider.CurrentRowIndex == ControlProvider.Control.RowCount - 1)
              ControlProvider.CurrentRowIndex = 0;
            else
              ControlProvider.CurrentRowIndex = ControlProvider.CurrentRowIndex + 1;
          }
          else
            return false;
        }
        else
        { // !NextSearch
          if (ControlProvider.Control.Rows.Count > 0)
            ControlProvider.CurrentRowIndex = 0;
          else
            return false;
        }
        return true;
      }

      DataView OrgView = ControlProvider.SourceAsDataView;
      if (ControlProvider.IncSearchDataView == null)
      {
        ControlProvider.IncSearchDataViewIsManual = false;
        if (OrgView == null)
          ControlProvider.IncSearchDataViewIsManual = true;
        else
        {
          if (String.IsNullOrEmpty(GridColumn.DataPropertyName))
            ControlProvider.IncSearchDataViewIsManual = true; // динамически вычисляемый столбец
          else
          {
            if (!OrgView.Table.Columns.Contains(Name))
              ControlProvider.IncSearchDataViewIsManual = true; // динамически вычисляемый столбец, отсутствующий в
            // исходном набор
          }
        }

        if (ControlProvider.IncSearchDataViewIsManual)
        {
          // Создаем временную таблицу из одного столбца, в котором будеи искать
          EFPApp.BeginWait("Загрузка данных столбца для поиска по первым буквам");
          try
          {
            DataTable TempTable = new DataTable();
            TempTable.Columns.Add(Name, typeof(string));
            for (int i = 0; i < ControlProvider.Control.RowCount; i++)
            {
              ControlProvider.DoGetRowAttributes(i, EFPDataGridViewAttributesReason.View);
              EFPDataGridViewCellAttributesEventArgs CellArgs = ControlProvider.DoGetCellAttributes(GridColumn.Index);
              object v = CellArgs.Value;
              if (v == null || v is DBNull)
                TempTable.Rows.Add(String.Empty);
              else
                TempTable.Rows.Add(v.ToString());
            }
            ControlProvider.IncSearchDataView = TempTable.DefaultView;
          }
          finally
          {
            EFPApp.EndWait();
          }
        }
        else
        {
          ControlProvider.IncSearchDataView = new DataView(OrgView.Table);
          ControlProvider.IncSearchDataView.Sort = OrgView.Sort;
        }

        NextSearch = false; // некуда продолжать
      }

      DataRow FoundRow; // Строка в EFPDataGridView.IncSearchDataView, на которую надо позиционироваться
      if (NextSearch)
      {
        if (ControlProvider.IncSearchDataView.Count < 2)
          return false; // некуда переходить

        DataRow CurrRow;
        if (ControlProvider.IncSearchDataViewIsManual)
          CurrRow = ControlProvider.IncSearchDataView.Table.Rows[ControlProvider.CurrentRowIndex];
        else
          CurrRow = ControlProvider.CurrentDataRow;
        int CurrIdx = DataTools.FindDataRowViewIndex(ControlProvider.IncSearchDataView, CurrRow);
        if (CurrIdx < 0)
          // Переместились на какую-то строку, которая не соответствует условию
          // Возвращаемся на первую подходящую (как если начать поиск)
          FoundRow = ControlProvider.IncSearchDataView[0].Row;
        else
        {
          if (CurrIdx == ControlProvider.IncSearchDataView.Count - 1)
            // Были на последней строке - переходим к первой
            FoundRow = ControlProvider.IncSearchDataView[0].Row;
          else
            FoundRow = ControlProvider.IncSearchDataView[CurrIdx + 1].Row;
        }
      }
      else //!NextSearch
      {
        // Устанавливаем фильтр
        // Исходный фильтр:
        string OrgViewRowFilter = ControlProvider.IncSearchDataViewIsManual ? String.Empty : OrgView.RowFilter;
        string PrevSearchFilter = ControlProvider.IncSearchDataView.RowFilter; // есои не найдем - вернем обратно
        string s1;
        if (ControlProvider.IncSearchDataView.Table.Columns[Name].DataType == typeof(string))
          s1 = "[" + Name + "] " +  DataTools.GetDataViewLikeExpressionString(SearchStr);
        else
          s1 = "CONVERT([" + Name + "], 'System.String') " + DataTools.GetDataViewLikeExpressionString(SearchStr);
        if (String.IsNullOrEmpty(OrgViewRowFilter))
          ControlProvider.IncSearchDataView.RowFilter = s1;
        else
          ControlProvider.IncSearchDataView.RowFilter = "(" + OrgViewRowFilter + ") AND " + s1;

        // Позиционируемся на первую строку в выбранном подмножестве
        if (ControlProvider.IncSearchDataView.Count == 0)
        {
          ControlProvider.IncSearchDataView.RowFilter = PrevSearchFilter;
          return false;
        }
        else
          FoundRow = ControlProvider.IncSearchDataView[0].Row;
      }

      // Выполняем позиционирование
      if (ControlProvider.IncSearchDataViewIsManual)
      {
        int p = ControlProvider.IncSearchDataView.Table.Rows.IndexOf(FoundRow);
        ControlProvider.CurrentRowIndex = p;
      }
      else
        ControlProvider.CurrentDataRow = FoundRow;

      return true;
    }
#endif

    #endregion

    #region Ширина столбца

    /// <summary>
    /// Ширина столбца в пикселях (дублирование TreeColumn.Width)
    /// </summary>
    public int Width
    {
      get { return TreeColumn.Width; }
      set { TreeColumn.Width = value; }
    }

    /// <summary>
    /// Ширина столбца в пунктах, в зависимости от разрешения экрана
    /// </summary>
    public int WidthPt
    {
      get { return (int)(Math.Round(TreeColumn.Width / ControlProvider.Measures.DpiX * 72)); }
      set { TreeColumn.Width = (int)Math.Round(value * ControlProvider.Measures.DpiX / 72); }
    }

    /// <summary>
    /// Ширина столбца в текстовых единицах (условная)
    /// </summary>
    public double TextWidth
    {
      get { return ControlProvider.Measures.GetColumnWidthChars(TreeColumn.Width); }
      set { TreeColumn.Width = ControlProvider.Measures.GetTextColumnWidth(value); }
    }

    #endregion

    #region Синхронное изменение размеров столбцов

    /// <summary>
    /// Имя группы для синхронизации размеров столбцов. Если несколько столбцов имеют
    /// одинаковое и непустое значение этого свойства, то изменение размера одного из
    /// этих столбцов приводит к синхронному изменению размеров остальных столбцов группы.
    /// По умолчанию - не задано
    /// </summary>
    public string SizeGroup { get { return _SizeGroup; } set { _SizeGroup = value; } }
    private string _SizeGroup;

    /// <summary>
    /// Установить размеры всех столбцов группы по текущему столбцу 
    /// </summary>
    internal void ResizeGroup()
    {
      throw new NotImplementedException();

#if XXX
      if (String.IsNullOrEmpty(SizeGroup))
        return;
      for (int i = 0; i < ControlProvider.Columns.Count; i++)
      {
        EFPDataGridViewColumn ThisCol = ControlProvider.Columns[i];
        if (ThisCol == this)
          continue;
        if (ThisCol.SizeGroup == SizeGroup)
        {
          if (ThisCol.GridColumn.InheritedAutoSizeMode != DataGridViewAutoSizeColumnMode.None)
            continue; // 29.03.2013

          ThisCol.GridColumn.Width = GridColumn.Width;
        }
      }
#endif
    }

    #endregion

    #region Подсказка для ячейки

#if XXX
    /// <summary>
    /// Событие вызывается при наведении курсора на ячейку, относящуюся к столбцу
    /// Обработчик может установить свойство Args.ToolTipText.
    /// К подсказке могут быть добавлены дополнительные строки, если это указано
    /// в текущей конфигурации просмотра
    /// </summary>
    public event EFPDataGridViewCellToolTipTextNeededEventHandler CellToopTextNeeded;

    /// <summary>
    /// Получить текст подсказки для ячейки
    /// Вызывается событие CellToopTextNeeded. Если столбец создан с помощью
    /// GridProducer, то добавляется текст, созданный продюсером
    /// </summary>
    /// <param name="RowIndex">Номер строки</param>
    /// <returns>Текст подсказки</returns>
    internal string GetCellToolTipText(int RowIndex)
    {
      if (RowIndex < 0)
        return String.Empty;
      List<string> a = new List<string>(); // Собираем подсказку из нескольких строк
      if (CellToopTextNeeded != null)
      {
        EFPDataGridViewCellToolTipTextNeededEventArgs Args = new EFPDataGridViewCellToolTipTextNeededEventArgs(this, RowIndex);
        CellToopTextNeeded(this, Args);
        if (Args.ToolTipText != null)
          a.Add(Args.ToolTipText);
      }

      ControlProvider.AddGridProducerToolTip(this, a, RowIndex);
      // Преобразуем в строку
      return String.Join(Environment.NewLine, a.ToArray());
    }
#endif

    #endregion

    #region Раскраска столбцов

    /// <summary>
    /// Цвет столбца. Используется, если для текущей строки в событии 
    /// EFPDataGridView.GetRowAttributres не определен более приоритетный цвет
    /// На момент вызова обработчика EFPDataGridView.GetCellAttributres, если он задан,
    /// цвет уже применен
    /// </summary>
    public EFPDataGridViewColorType ColorType { get { return _ColorType; } set { _ColorType = value; } }
    private EFPDataGridViewColorType _ColorType;

    /// <summary>
    /// Окрашивание всех значений в столбце "серым" цветом
    /// </summary>
    public bool Grayed { get { return _Grayed; } set { _Grayed = value; } }
    private bool _Grayed;

    /// <summary>
    /// Рамка для левой границы столбца
    /// На момент вызова обработчика EFPDataGridView.GetCellAttributres, если он задан,
    /// стиль рамки уже применен
    /// </summary>
    public EFPDataGridViewBorderStyle LeftBorder { get { return _LeftBorder; } set { _LeftBorder = value; } }
    private EFPDataGridViewBorderStyle _LeftBorder;

    /// <summary>
    /// Рамка для правой границы столбца
    /// На момент вызова обработчика EFPDataGridView.GetCellAttributres, если он задан,
    /// стиль рамки уже применен
    /// </summary>
    public EFPDataGridViewBorderStyle RightBorder { get { return _RightBorder; } set { _RightBorder = value; } }
    private EFPDataGridViewBorderStyle _RightBorder;

    #endregion

    #region Печать

    /// <summary>
    /// Многострочные заголовки при печати таблицы
    /// </summary>
    public string[] PrintHeaders { get { return _PrintHeaders; } set { _PrintHeaders = value; } }
    private string[] _PrintHeaders;

    /// <summary>
    /// Многострочные заголовки при печати таблицы (свойство PrintHeaders)
    /// Версия для установки в виде одной строки с заменой символов:
    /// "|" - разделитель многострочного заголовка
    /// "^" - мягкий перенос
    /// "_" - неразрывный пробел
    /// </summary>
    public string PrintHeadersSpec
    {
      get { return DataTools.StrFromSpecCharsArray(PrintHeaders); }
      set { PrintHeaders = DataTools.StrToSpecCharsArray(value); }
    }

    #endregion

    #region Вспомогательные методы и свойства

    /// <summary>
    /// Горизонтальное выравнивание ячейки
    /// Осуществляет доступ к свойству Column.DefaultCellStyle.Alignment,
    /// преобразуя его в формат выравнивания для текста.
    /// При установке свойства существующее выравнивание ячейки по вертикали сохраняется
    /// </summary>
    public HorizontalAlignment TextAlign
    {
      get
      {
        BaseTextControl tc = NodeControl as BaseTextControl;
        if (tc == null)
          return HorizontalAlignment.Center;
        else
          return tc.TextAlign;
      }
      set
      {
        BaseTextControl tc = NodeControl as BaseTextControl;
        if (tc != null)
          tc.TextAlign = value;
        else
          throw new InvalidOperationException();
      }
    }

    /// <summary>
    /// Возвращает свойство Name
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Name;
    }

    #endregion

    #region Для DBF-файлов

    /// <summary>
    /// Описание поля для сохранения в Dbf формате, если было задано в явном
    /// виде или получено из GridProducerColumn. Иначе-пустая структура
    /// </summary>
    public DbfFieldInfo DbfInfo { get { return _DbfInfo; } set { _DbfInfo = value; } }
    private DbfFieldInfo _DbfInfo;

    /// <summary>
    /// Имя DBF-поля для этого столбца по умолчанию
    /// Текущее значение свойства DbfInfo не учитывается
    /// </summary>
    public string DefaultDbfName
    {
      get
      {
        string nm = this.Name;
        if (!String.IsNullOrEmpty(nm))
        {
          nm = nm.ToUpper();
          if (DbfFieldInfo.IsValidFieldName(nm))
            return nm;
        }
        return "FIELD" + (TreeColumn.Index + 1).ToString("00000");
      }
    }

#if XXX
    /// <summary>
    /// Описание поля по умолчанию
    /// Не бывает значение null. При первой попытке чтения свойства анализирует
    /// исходные данные для определения типа столбца
    /// </summary>
    public DbfFieldInfo DefaultDbfInfo
    {
      get
      {
        if (FDefaultDbfInfo.IsEmpty)
          FDefaultDbfInfo = CreateDefaultDbfInfo();

        return FDefaultDbfInfo;
      }
      //set
      //{
      //  FDefaultDbfInfo = value;
      //}
    }
    private DbfFieldInfo FDefaultDbfInfo;

    private DbfFieldInfo CreateDefaultDbfInfo()
    {
      if (GridColumn is DataGridViewCheckBoxColumn)
        return DbfFieldInfo.CreateBool(DefaultDbfName);

      if (GridColumn is DataGridViewTextBoxColumn)
      {
        switch (SizeGroup)
        { 
          case "Date":
            return DbfFieldInfo.CreateDate(DefaultDbfName);
          case "DateTime":
            return DbfFieldInfo.CreateString(DefaultDbfName, 19);
        }

        Type t = ((DataGridViewTextBoxColumn)GridColumn).ValueType;
        if (t == null)
          t = typeof(string);
        switch (t.Name)
        {
          case "Byte":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 3);
          case "SByte":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 4);
          case "Int16":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 6);
          case "UInt16":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 5);
          case "Int32":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 11);
          case "UInt32":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 10);
          case "Int64":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 22); // ???
          case "UInt64":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 21);
          case "Single":
          case "Double":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 20, 8); // ???
          case "Decimal":
            return DbfFieldInfo.CreateNum(DefaultDbfName, 20, 2);
        }

        if (!String.IsNullOrEmpty(GridColumn.DataPropertyName))
        {
          DataTable tbl = ControlProvider.SourceAsDataTable;
          if (tbl != null)
          {
            int p = tbl.Columns.IndexOf(GridColumn.DataPropertyName);
            if (p >= 0)
            {
              if (tbl.Columns[p].MaxLength > 0)
              { 
                if (tbl.Columns[p].MaxLength<=255 )
                  return DbfFieldInfo.CreateString(DefaultDbfName, tbl.Columns[p].MaxLength);
                else
                  return DbfFieldInfo.CreateMemo(DefaultDbfName);
              }
            }
          }
        }
      }

      // Возвращаем строковое поле

      // Перебираем все значения, выискивая самое длинное
      int w = 1;
      EFPApp.BeginWait("Определение ширины столблца \"" + GridColumn.HeaderText + "\"");
      try
      {
        for (int i = 0; i < ControlProvider.Control.RowCount; i++)
        {
          ControlProvider.DoGetRowAttributes(i, EFPDataGridViewAttributesReason.View);
          EFPDataGridViewCellAttributesEventArgs CellArgs = ControlProvider.DoGetCellAttributes(GridColumn.Index);
          if (CellArgs.Value != null)
          {
            w = Math.Max(w, CellArgs.Value.ToString().Length);
            if (w > 255)
              break;
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }

      if (w > 255)
        return DbfFieldInfo.CreateMemo(DefaultDbfName);
      else
        return DbfFieldInfo.CreateString(DefaultDbfName, w);
    }

#endif

    #endregion
  }

  /// <summary>
  /// Псевдоколлекция столбцов - реализация свойства EFPDataTreeView.Columns
  /// </summary>
  public class EFPDataTreeViewColumns
  {
    #region Защищенный конструктор

    /// <summary>
    /// Защищенный конструктор
    /// </summary>
    /// <param name="controlProvider">Владелец</param>
    internal protected EFPDataTreeViewColumns(EFPDataTreeView controlProvider)
    {
      _ControlProvider = controlProvider;
      controlProvider.Control.ColumnWidthChanged += new EventHandler<TreeColumnEventArgs>(Control_ColumnWidthChanged);
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Объект - владелец
    /// </summary>
    public EFPDataTreeView ControlProvider { get { return _ControlProvider; } }
    private EFPDataTreeView _ControlProvider;

    #endregion

    #region Свойства и методы доступа к элементам коллекции

    /// <summary>
    /// 
    /// </summary>
    public int Count
    {
      get
      {
        return ControlProvider.Control.Columns.Count;
      }
    }

#if XXX

    /// <summary>
    /// Доступ к столбцу, соответствующему объекту DataGridViewColumn
    /// </summary>
    /// <param name="GridColumn"></param>
    /// <returns></returns>
    public EFPDataGridViewColumn this[DataGridViewColumn GridColumn]
    {
      get
      {
#if DEBUG
        if (GridColumn == null)
          throw new ArgumentNullException("GridColumn");
        //if (GridColumn.DataGridView != null)
        //{
        //  if (GridColumn.DataGridView != GridHandler.Control)
        //    throw new ArgumentException("Столбец относится к другому табличному просмотру", "GridColumn");
        //}
#endif
        if (GridColumn.Tag == null)
          return new EFPDataGridViewColumn(ControlProvider, GridColumn);
        return (EFPDataGridViewColumn)(GridColumn.Tag);
      }
    }

    /// <summary>
    /// Доступ к столбцу по индексу DataGridViewColumn.Index
    /// </summary>
    /// <param name="Index"></param>
    /// <returns></returns>
    public EFPDataGridViewColumn this[int Index]
    {
      get
      {
        return this[ControlProvider.Control.Columns[Index]];
      }
    }

    /// <summary>
    /// Доступ по имени столбца
    /// Если просмотр не содержит столбца с таким именем, возвращается null
    /// </summary>
    /// <param name="Name"></param>
    /// <returns></returns>
    public EFPDataGridViewColumn this[string Name]
    {
      get
      {
        if (String.IsNullOrEmpty(Name))
          return null;
        for (int i = 0; i < ControlProvider.Control.Columns.Count; i++)
        {
          if (this[i].Name == Name)
            return this[i];
        }
        return null;
      }
    }

    public EFPDataGridViewColumn this[IGridProducerColumn Column]
    {
      get
      {
        if (Column == null)
          return null;
        return this[Column.ColumnName];
      }
    }

    public int IndexOf(string Name)
    {
      if (String.IsNullOrEmpty(Name))
        return -1;
      for (int i = 0; i < ControlProvider.Control.Columns.Count; i++)
      {
        if (this[i].Name == Name)
          return i;
      }
      return -1;
    }

#endif

    #endregion

#pragma warning disable 1591

    #region Методы добавления столбцов

    public NodeTextBox AddText(string columnName, bool isDataColumn, string headerText, int textWidth, int minTextWidth, HorizontalAlignment alignment)
    {
      if (textWidth < 1)
        throw new ArgumentOutOfRangeException("textWidth");
      if (minTextWidth < 1)
        throw new ArgumentOutOfRangeException("minTextWidth");

      if (minTextWidth > textWidth)
        throw new ArgumentOutOfRangeException("minTextWidth", minTextWidth,
          "Минимальная ширина столбца (" + minTextWidth.ToString() + ") не может быть больше, чем устанавливаемая (" + textWidth.ToString() + ")");

      TreeColumn Column = new TreeColumn(headerText, textWidth * 10);
      Column.MinColumnWidth = minTextWidth * 10;
      ControlProvider.Control.Columns.Add(Column);

      NodeTextBox tb = new NodeTextBox();
      tb.TextAlign = alignment;
      //tb.EditEnabled = !ReadOnly;

      tb.VirtualMode = true;
      if (isDataColumn)
      {
        tb.DataPropertyName = columnName;
        tb.ValueNeeded += tb_ValueNeeded;
      }
      tb.ParentColumn = Column;
      ControlProvider.Control.NodeControls.Add(tb);

      return tb;
    }

    void tb_ValueNeeded(object sender, NodeControlValueEventArgs args)
    {
      NodeTextBox tb = sender as NodeTextBox;
      DataRow Row = args.Node.Tag as DataRow;
      if (Row != null)
      {
        args.Value = Row[tb.DataPropertyName];
      }
    }

    public NodeTextBox AddText(string columnName, bool isDataColumn, string headerText, int textWidth, int minTextWidth)
    {
      return AddText(columnName, isDataColumn, headerText, textWidth, minTextWidth, HorizontalAlignment.Left);
    }

    public NodeTextBox AddText(string columnName, bool isDataColumn, string headerText, int textWidth)
    {
      return AddText(columnName, isDataColumn, headerText, textWidth, Math.Min(textWidth, 5));
    }

    public NodeTextBox AddText(string columnName, bool isDataColumn, string headerText)
    {
      return AddText(columnName, isDataColumn, headerText, 10);
    }

    public NodeTextBox AddText(string columnName)
    {
      return AddText(columnName, true, columnName, 10);
    }

#if XXX

    public DataGridViewTextBoxColumn AddTextFill(string FieldName, bool DataColumn, string HeaderText, int FillWeight, int MinTextWidth)
    {
      DataGridViewTextBoxColumn col = AddText(FieldName, DataColumn, HeaderText, MinTextWidth, MinTextWidth);
      col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      col.FillWeight = FillWeight;
      return col;
    }

    public DataGridViewTextBoxColumn AddTextFill(string FieldName, bool DataColumn, string HeaderText)
    {
      return AddTextFill(FieldName, DataColumn, HeaderText, 100, 5);
    }

    public DataGridViewTextBoxColumn AddTextFill(string FieldName)
    {
      return AddTextFill(FieldName, true, FieldName, 100, 5);
    }

    public DataGridViewTextBoxColumn AddTextFill(string FieldName, int FillWeight)
    {
      return AddTextFill(FieldName, true, FieldName, FillWeight, 5);
    }


    public DataGridViewTextBoxColumn AddDate(string FieldName, bool DataColumn, string HeaderText)
    {
      DataGridViewTextBoxColumn col = AddText(FieldName, DataColumn, HeaderText, 10);
      col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
      col.DefaultCellStyle.Format = "dd.MM.yyyy";
      col.DefaultCellStyle.FormatProvider = DateBox.DotDateTimeConv;
      EFPDataGridViewColumn ghCol = ControlProvider.Columns[col];
      ghCol.SizeGroup = "Date";
      //ghCol.DefaultDbfInfo = DbfFieldInfo.CreateDate(ghCol.DefaultDbfName);

      //!!!ghCol.Mask = DataConv.DateStr10Mask;
      //ghCol.CanIncSearch = true;

      return col;
    }

    public DataGridViewTextBoxColumn AddDate(string FieldName)
    {
      return AddDate(FieldName, true, FieldName);
    }

    public DataGridViewTextBoxColumn AddDateTime(string FieldName, bool DataColumn, string HeaderText)
    {
      DataGridViewTextBoxColumn col = AddText(FieldName, DataColumn, HeaderText, 19);
      col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
      col.DefaultCellStyle.Format = "dd.MM.yyyy HH:mm:ss";
      col.DefaultCellStyle.FormatProvider = DateBox.DotDateTimeConv;
      EFPDataGridViewColumn ghCol = ControlProvider.Columns[col];
      ghCol.SizeGroup = "DateTime";

      //!!ghCol.Mask = DataConv.DateTimeStr19Mask;
      //ghCol.CanIncSearch = true;

      return col;
    }

    public DataGridViewTextBoxColumn AddDateTime(string FieldName)
    {
      return AddDateTime(FieldName, true, FieldName);
    }

    public DataGridViewTextBoxColumn AddFixedPoint(string FieldName, bool DataColumn, string HeaderText, int TextWidth, int DecimalPlaces, string SizeGroup)
    {
#if DEBUG
      if (DecimalPlaces < 0)
        throw new ArgumentException("Количество знаков после запятой не может быть отрицательным", "DecimalPlaces");
#endif
      DataGridViewTextBoxColumn col = AddText(FieldName, DataColumn, HeaderText, TextWidth, DecimalPlaces + 2);
      col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      if (DecimalPlaces > 0)
        col.DefaultCellStyle.Format = "0." + new string('0', DecimalPlaces);
      else
        col.DefaultCellStyle.Format = "0";
      EFPDataGridViewColumn ghCol = ControlProvider.Columns[col];
      ghCol.SizeGroup = SizeGroup;
      //ghCol.DefaultDbfInfo = DbfFieldInfo.CreateNum(ghCol.DefaultDbfName, TextWidth, DecimalPlaces); 
      return col;
    }

#endif

    public NodeTextBox AddInt(string columnName, bool isDataColumn, string headerText, int textWidth)
    {
      NodeTextBox col = AddText(columnName, isDataColumn, headerText, textWidth, 1, HorizontalAlignment.Right);
      return col;
    }

    public NodeTextBox AddInt(string columnName)
    {
      return AddInt(columnName, true, columnName, 5);
    }

#if XXX

    public DataGridViewCheckBoxColumn AddBool(string FieldName, bool DataColumn, string HeaderText)
    {
      ExtDataGridViewCheckBoxColumn col = new ExtDataGridViewCheckBoxColumn();
      col.Name = FieldName;
      if (DataColumn)
        col.DataPropertyName = FieldName;
      col.HeaderText = HeaderText;
      col.Width = FControlProvider.Measures.CheckBoxColumnWidth;
      col.MinimumWidth = col.Width;
      ControlProvider.Control.Columns.Add(col);
      EFPDataGridViewColumn ghCol = ControlProvider.Columns[col];
      ghCol.SizeGroup = "CheckBox";
      //ghCol.DefaultDbfInfo = DbfFieldInfo.CreateBool(ghCol.DefaultDbfName);
      return col;
    }

    public DataGridViewCheckBoxColumn AddBool(string FieldName)
    {
      return AddBool(FieldName, true, FieldName);
    }

    public DataGridViewImageColumn AddImage(string FieldName)
    {
      DataGridViewImageColumn Column = new DataGridViewImageColumn();
      if (!String.IsNullOrEmpty(FieldName))
      {
        Column.Name = FieldName;
        Column.HeaderText = String.Empty;
      }
      Column.Width = ControlProvider.Measures.ImageColumnWidth;
      Column.Resizable = DataGridViewTriState.False;
      Column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
      ControlProvider.Control.Columns.Add(Column);
      return Column;
    }

    public DataGridViewImageColumn AddImage()
    {
      return AddImage(null);
    }

    public DataGridViewComboBoxColumn AddCombo(string FieldName, bool DataColumn, string HeaderText, int TextWidth, int MinTextWidth)
    {
      if (TextWidth < 1)
        throw new ArgumentOutOfRangeException("TextWidth");
      if (MinTextWidth < 1)
        throw new ArgumentOutOfRangeException("MinTextWidth");

      if (MinTextWidth > TextWidth)
        throw new ArgumentOutOfRangeException("MinTextWidth", MinTextWidth,
          "Минимальная ширина столбца (" + MinTextWidth.ToString() + ") не может быть больше, чем устанавливаемая (" + TextWidth.ToString() + ")");

      DataGridViewComboBoxColumn col = new DataGridViewComboBoxColumn();
      col.Name = FieldName;
      if (DataColumn)
        col.DataPropertyName = FieldName;
      col.HeaderText = HeaderText;
      col.Width = TextWidth * 8 + 10; // !!!
      col.MinimumWidth = MinTextWidth * 8 + 10; // !!!
      col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
      ControlProvider.Control.Columns.Add(col);

      return col;
    }

    public DataGridViewComboBoxColumn AddComboFill(string FieldName, bool DataColumn, string HeaderText, int FillWeight, int MinTextWidth)
    {
      DataGridViewComboBoxColumn col = AddCombo(FieldName, DataColumn, HeaderText, MinTextWidth, MinTextWidth);
      col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      col.FillWeight = FillWeight;
      return col;
    }

    public DataGridViewComboBoxColumn AddComboFill(string FieldName, bool DataColumn, string HeaderText)
    {
      return AddComboFill(FieldName, DataColumn, HeaderText, 100, 5);
    }

    public DataGridViewComboBoxColumn AddComboFill(string FieldName)
    {
      return AddComboFill(FieldName, true, FieldName, 100, 5);
    }

    public DataGridViewComboBoxColumn AddComboFill(string FieldName, int FillWeight)
    {
      return AddComboFill(FieldName, true, FieldName, FillWeight, 5);
    }

#endif

#if XXX
    /// <summary>
    /// Возвращает последний столбец EFPDataGridViewColumn, добавленный с помощью методов AddXXX().
    /// Значение не определено, если выполнялось добавление столбцов непосредственно
    /// в табличный просмотр DataGridView.Columns
    /// Свойство удобно использовать для установки дополнительных атрибутов столбца,
    /// которые не были установлены в методе AddXXX()
    /// </summary>
    public EFPDataGridViewColumn LastAdded
    {
      get
      {
        if (ControlProvider.Control.ColumnCount == 0)
          return null;
        else
          return this[ControlProvider.Control.Columns[ControlProvider.Control.ColumnCount - 1]];
      }
    }
#endif

    #endregion

    #region Синхронное изменение размеров столбцов

    private bool _InsideColumnWidthChanged = false;

    //private bool ColumnWidthChangedErrorWasShown = false;

    private void Control_ColumnWidthChanged(object sender, TreeColumnEventArgs args)
    {
      if (_InsideColumnWidthChanged)
        return;
      _InsideColumnWidthChanged = true;
      try
      {
#if XXX

        try
        {
          this[Args.Column].ResizeGroup();
        }
        catch (Exception e) // 29.03.2013
        {
          if (!ColumnWidthChangedErrorWasShown)
          {
            ColumnWidthChangedErrorWasShown = true;
            DebugTools.ShowException(e, "Ошибка при установке размеров одинаковых столбцов");
          }
        }

#endif
      }
      finally
      {
        _InsideColumnWidthChanged = false;
      }
    }

    #endregion

    #region Методы групповой установки свойств столбцов

    /// <summary>
    /// Установить свойство EditEnabled для всех столбцов
    /// </summary>
    /// <param name="value">Устанавливаемое значение (инвертированное) свойства InteractiveControl.EditEnabled</param>
    public void SetReadOnly(bool value)
    {
      for (int i = 0; i < ControlProvider.Control.NodeControls.Count; i++)
      {
        InteractiveControl nc = ControlProvider.Control.NodeControls[i] as InteractiveControl;
        if (nc == null)
          continue;
        nc.EditEnabled = !value;
      }
    }

    #endregion

    #region Отладочные средства

    ///// <summary>
    ///// Массив имен всех столбцов в просмотре (отладочное средство)
    ///// Свойства DataGridViewColumn.GroupName
    ///// </summary>
    //public string[] GridColumnNames
    //{
    //  get
    //  {
    //    string[] a = new string[ControlProvider.Control.Columns.Count];
    //    for (int i = 0; i < a.Length; i++)
    //      a[i] = ControlProvider.Control.Columns[i].Name;
    //    return a;
    //  }
    //}

    ///// <summary>
    ///// Массив имен всех столбцов в просмотре (отладочное средство)
    ///// Свойства EFPDataGridViewColumn.Name
    ///// </summary>
    //public string[] ControlProviderColumnNames
    //{
    //  get
    //  {
    //    string[] a = new string[Count];
    //    for (int i = 0; i < a.Length; i++)
    //      a[i] = this[i].Name;
    //    return a;
    //  }
    //}

    #endregion
  }

}
