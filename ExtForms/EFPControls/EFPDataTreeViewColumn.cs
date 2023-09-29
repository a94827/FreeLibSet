// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Globalization;
using FreeLibSet.DBF;
using FreeLibSet.Formatting;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.Collections;
using System.Collections;
using FreeLibSet.Forms.Diagnostics;

/*
 * Дополнительные описания для стоблцов TreeViewAdv
 * TreeViewAdv определяет объекты TreeColumn, у которых есть заголовок, признак видимости, сортировка.
 * Также есть объекты NodeControl, которые выводят значения
 */

namespace FreeLibSet.Forms
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
  /// Объект расширенного описания столбца в <see cref="EFPDataTreeView"/>.
  /// Столбцы не используются, если <see cref="TreeViewAdv.UseColumns"/>=false.
  /// </summary>
  public class EFPDataTreeViewColumn : IEFPDataViewColumn
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

    IEFPDataView IEFPDataViewColumn.ControlProvider { get { return ControlProvider; } }

    /// <summary>
    /// Столбец древовидного просмотра <see cref="TreeViewAdv"/>
    /// </summary>
    public TreeColumn TreeColumn { get { return _TreeColumn; } }
    private TreeColumn _TreeColumn;

    /// <summary>
    /// Объект работы с ячейками столбца
    /// </summary>
    public NodeControl NodeControl { get { return _NodeControl; } }
    private NodeControl _NodeControl;

    /// <summary>
    /// Имя столбца. Всегда определено
    /// </summary>
    public string Name { get { return _Name; } }
    private string _Name;

    /// <summary>
    /// Пользовательские данные 
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    /// <summary>
    /// Если столбец был создан с помощью <see cref="EFPGridProducer"/>, то ссылка на генератор столбца,
    /// иначе - null.
    /// </summary>
    public IEFPGridProducerColumn ColumnProducer { get { return _ColumnProducer; } set { _ColumnProducer = value; } }
    private IEFPGridProducerColumn _ColumnProducer;

    bool IEFPDataViewColumn.Visible { get { return true; } }

    /// <summary>
    /// Имя столбца, отображаемое в диалоге параметров страницы.
    /// Если не задано в явном виде, возвращает свойство <see cref="TreeColumn.Header"/> или <see cref="Name"/>
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
        {
          if (String.IsNullOrEmpty(TreeColumn.Header))
            return Name;
          else
            return TreeColumn.Header.Replace(Environment.NewLine, " ");
        }
        else
          return _DisplayName;
      }
      set { _DisplayName = value; }
    }
    private string _DisplayName;

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
    /// Ширина столбца в пикселях (дублирование <see cref="TreeColumn.Width"/>)
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
      if (String.IsNullOrEmpty(SizeGroup))
        return;
      for (int i = 0; i < ControlProvider.Columns.Count; i++)
      {
        EFPDataTreeViewColumn thisCol = ControlProvider.Columns[i];
        if (thisCol == this)
          continue;
        if (thisCol.SizeGroup == SizeGroup)
        {
          //if (thisCol.GridColumn.InheritedAutoSizeMode != DataGridViewAutoSizeColumnMode.None)
          //  continue; // 29.03.2013

          thisCol.TreeColumn.Width = TreeColumn.Width;
        }
      }
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

    // TODO: Не используется
    /// <summary>
    /// Цвет столбца. Используется, если для текущей строки в событии 
    /// EFPDataTreeView.GetRowAttributres не определен более приоритетный цвет
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
    /// Возвращает true для всех столбцов, кроме изображений
    /// </summary>
    public bool Printable
    {
      get
      {
        if (NodeControl is NodeIcon)
          return false;
        else
          return true;
      }
    }

    /// <summary>
    /// Многострочные заголовки при печати таблицы.
    /// Если свойство не установлено (свойство возвращает null), то используется <see cref="TreeColumn.Header"/>.
    /// </summary>
    public string[] PrintHeaders { get { return _PrintHeaders; } set { _PrintHeaders = value; } }
    private string[] _PrintHeaders;

    /// <summary>
    /// Многострочные заголовки при печати таблицы (свойство <see cref="PrintHeaders"/>)
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
    /// Осуществляет доступ к свойству <see cref="BaseTextControl.TextAlign"/>.
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
    /// Возвращает свойство <see cref="Name"/>
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
    /// виде или получено из <see cref="EFPGridProducerColumn"/>. Иначе-пустая структура
    /// </summary>
    public DbfFieldInfo DbfInfo { get { return _DbfInfo; } set { _DbfInfo = value; } }
    private DbfFieldInfo _DbfInfo;

    /// <summary>
    /// Имя DBF-поля для этого столбца по умолчанию
    /// Текущее значение свойства <see cref="DbfInfo"/> не учитывается
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
  /// Псевдоколлекция столбцов - реализация свойства <see cref="EFPDataTreeView.Columns"/>.
  /// Используется, если <see cref="TreeViewAdv.UseColumns"/>=true, иначе список пустой.
  /// </summary>
  public class EFPDataTreeViewColumns : IEnumerable<EFPDataTreeViewColumn>, IEFPDataViewColumns
  {
    #region Защищенный конструктор

    /// <summary>
    /// Защищенный конструктор
    /// </summary>
    /// <param name="controlProvider">Владелец</param>
    internal protected EFPDataTreeViewColumns(EFPDataTreeView controlProvider)
    {
      _ControlProvider = controlProvider;
      _ColumnDict = new Dictionary<TreeColumn, EFPDataTreeViewColumn>();
      //_NameDict = new Dictionary<string, EFPDataTreeViewColumn>();
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

    /// <summary>
    /// Нет возможности хранить в TreeColumn ссылку на EFPDataTreeViewColumn.
    /// Используем словарь, который надо периодически проверять на актуальность
    /// </summary>
    private Dictionary<TreeColumn, EFPDataTreeViewColumn> _ColumnDict;

    //private Dictionary<string, EFPDataTreeViewColumn> _NameDict;

    private void UpdateColumnDict()
    {
      if (ControlProvider.Control.IsDisposed)
        throw new ObjectDisposedException("TreeViewAdv is disposed");

      if (!ControlProvider.Control.UseColumns)
      {
        _ColumnDict.Clear();
        return;
      }

      #region Удаляем столбцы, которых больше нет в TreeViewAdv

      List<TreeColumn> lstDel = null;
      foreach (KeyValuePair<TreeColumn, EFPDataTreeViewColumn> pair in _ColumnDict)
      {
        if (pair.Key.Owner == null)
        {
          if (lstDel == null)
            lstDel = new List<TreeColumn>();
          lstDel.Add(pair.Key);
        }
      }

      if (lstDel != null)
      {
        foreach (TreeColumn treeCol in lstDel)
          _ColumnDict.Remove(treeCol);
      }

      #endregion

      #region Добавляем недостающие столбцы

      SingleScopeStringList names = new SingleScopeStringList(false);
      foreach (KeyValuePair<TreeColumn, EFPDataTreeViewColumn> pair in _ColumnDict)
        names.Add(pair.Value.Name);

      foreach (TreeColumn treeCol in ControlProvider.Control.Columns)
      {
        if (!_ColumnDict.ContainsKey(treeCol))
        {
          string colName = "Noname_" + (treeCol.Index + 1).ToString();
          int cnt = 1;
          while (names.Contains(colName))
          {
            cnt++;
            colName = "Noname_" + (treeCol.Index + 1).ToString() + "_" + cnt.ToString();
          }

          NodeControl mainNC = null;
          foreach (NodeControl nc in ControlProvider.Control.NodeControls)
          {
            if (nc.ParentColumn != treeCol)
              continue;

            if (nc is InteractiveControl)
            {
              mainNC = nc;
              break;
            }
            if (mainNC == null)
              mainNC = nc;
          }
          EFPDataTreeViewColumn efpCol = new EFPDataTreeViewColumn(ControlProvider, treeCol, mainNC, colName);
          _ColumnDict.Add(treeCol, efpCol);
        }
      }

      #endregion
    }


    /// <summary>
    /// Доступ к столбцу, соответствующему объекту <see cref="TreeColumn"/>
    /// </summary>
    /// <param name="treeColumn">Столбец в элементе<see cref="TreeViewAdv"/></param>
    /// <returns>Объект <see cref="EFPDataTreeViewColumn"/></returns>
    public EFPDataTreeViewColumn this[TreeColumn treeColumn]
    {
      get
      {
#if DEBUG
        if (treeColumn == null)
          throw new ArgumentNullException("treeColumn");
        //if (GridColumn.DataGridView != null)
        //{
        //  if (GridColumn.DataGridView != GridHandler.Control)
        //    throw new ArgumentException("Столбец относится к другому табличному просмотру", "GridColumn");
        //}
#endif

        EFPDataTreeViewColumn res;
        if (_ColumnDict.TryGetValue(treeColumn, out res))
          return res;

        if (treeColumn.Owner == null)
          throw new ArgumentException("Столбец не присоединен к просмотру", "treeColumn");

        if (!Object.ReferenceEquals(treeColumn.Owner.TreeView, ControlProvider.Control))
          throw new ArgumentException("Столбец не относится к текущему просмотру", "treeColumn");

        UpdateColumnDict();
        return _ColumnDict[treeColumn];
      }
    }

    /// <summary>
    /// Доступ к столбцу по индексу <see cref="TreeColumn.Index"/>.
    /// </summary>
    /// <param name="index">Индекс столбца табличного просмотра</param>
    /// <returns>Объект <see cref="EFPDataTreeViewColumn"/></returns>
    public EFPDataTreeViewColumn this[int index]
    {
      get
      {
        if (index < 0 || index >= ControlProvider.Columns.Count)
          throw new ArgumentOutOfRangeException();
        return this[ControlProvider.Control.Columns[index]];
      }
    }

    IEFPDataViewColumn IEFPDataViewColumns.this[int index]
    {
      get { return this[index]; }
    }

    /// <summary>
    /// Доступ по имени столбца
    /// Если просмотр не содержит столбца с таким именем, возвращается null
    /// </summary>
    /// <param name="name">Имя столбца</param>
    /// <returns>Объект <see cref="EFPDataTreeViewColumn"/></returns>
    public EFPDataTreeViewColumn this[string name]
    {
      get
      {
        // Может быть, нужна оптимизация

        if (String.IsNullOrEmpty(name))
          return null;
        for (int i = 0; i < ControlProvider.Control.Columns.Count; i++)
        {
          if (this[i].Name == name)
            return this[i];
        }
        return null;
      }
    }

    IEFPDataViewColumn IEFPDataViewColumns.this[string name]
    {
      get { return this[name]; }
    }

    /// <summary>
    /// Доступ к столбцу, созданному с помощью <see cref="EFPGridProducer"/>.
    /// Если нет такого столбца, возвращается null.
    /// </summary>
    /// <param name="columnProducer">Генератор столбца</param>
    /// <returns>Объект <see cref="EFPDataTreeViewColumn"/></returns>
    public EFPDataTreeViewColumn this[IEFPGridProducerColumn columnProducer]
    {
      get
      {
        if (columnProducer == null)
          return null;

        for (int i = 0; i < ControlProvider.Control.Columns.Count; i++)
        {
          if (this[i].ColumnProducer == columnProducer)
            return this[i];
        }
        return null;
      }
    }

    IEFPDataViewColumn IEFPDataViewColumns.this[IEFPGridProducerColumn columnProducer]
    {
      get { return this[columnProducer]; }
    }

    /// <summary>
    /// Возвращает индекс столбца по именм
    /// </summary>
    /// <param name="name">Имя столбца</param>
    /// <returns>Индекс столбца</returns>
    public int IndexOf(string name)
    {
      if (String.IsNullOrEmpty(name))
        return -1;
      for (int i = 0; i < ControlProvider.Control.Columns.Count; i++)
      {
        if (this[i].Name == name)
          return i;
      }
      return -1;
    }

    #endregion

    #region IEnumerable<EFPDataTreeViewColumn>

    /// <summary>
    /// Перечислитель для столбцов
    /// </summary>
    public struct Enumerator : IEnumerator<EFPDataTreeViewColumn>, IEnumerator<IEFPDataViewColumn>
    {
      #region Защищенный конструктор

      internal Enumerator(EFPDataTreeViewColumns columns)
      {
        _Columns = columns;
        _CurrentIndex = -1;
      }

      #endregion

      #region Поля

      private readonly EFPDataTreeViewColumns _Columns;
      private int _CurrentIndex;

      /// <summary>
      /// Возвращает текущий объект столбца в перечислении
      /// </summary>
      public EFPDataTreeViewColumn Current { get { return _Columns[_CurrentIndex]; } }

      IEFPDataViewColumn IEnumerator<IEFPDataViewColumn>.Current { get { return Current; } }

      object IEnumerator.Current { get { return Current; } }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      /// <summary>
      /// Переход к следующему перечисляемому столбцу
      /// </summary>
      /// <returns>True, если есть столбцы</returns>
      public bool MoveNext()
      {
        _CurrentIndex++;
        return _CurrentIndex < _Columns.Count;
      }

      void IEnumerator.Reset()
      {
        _CurrentIndex = -1;
      }

      #endregion
    }

    /// <summary>
    /// Создает перечислитель по всем столбцам просмотра, включая скрытые
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<EFPDataTreeViewColumn> IEnumerable<EFPDataTreeViewColumn>.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator<IEFPDataViewColumn> IEnumerable<IEFPDataViewColumn>.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

#pragma warning disable 1591

    #region Методы добавления столбцов

    #region Обобщенный метод AddControl()

    /// <summary>
    /// Добавляет <paramref name="nodeControl"/> в коллекцию элементов дерева <see cref="TreeViewAdv.NodeControls"/>.
    /// Создает столбец <see cref="TreeColumn"/>, инициализирует его свойства, и присоединяет к нему <paramref name="nodeControl"/>, устанавливая свойство <see cref="NodeControl.ParentColumn"/>.
    /// Если <see cref="TreeViewAdv.UseColumns"/>=false, то выполняется только добавление <paramref name="nodeControl"/>, а столбец не создается.
    /// Созданный объект <see cref="EFPDataTreeViewColumn"/> не возвращается. Для доступа к нему используйте свойство <see cref="LastAdded"/> или одну из перегрузок индексированного свойства, например, <see cref="this[string]"/>.
    /// </summary>
    /// <param name="nodeControl">Созданный элемент <see cref="NodeControl"/>, например <see cref="NodeTextBox"/>. Элемент не должен быть присоединен к дереву</param>
    /// <param name="columnName">Имя столбца, которое может быть использовано для поиска столбца</param>
    /// <param name="headerText">Заголовок столбца <see cref="TreeColumn.Header"/></param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина столбца в текстовых единицах</param>
    public void AddControl(NodeControl nodeControl, string columnName, string headerText, int textWidth, int minTextWidth)
    {
      if (nodeControl == null)
        throw new ArgumentNullException("nodeControl");
      if (nodeControl.Parent != null)
        throw new InvalidOperationException("Элемент уже добавлен");

      //if (!ControlProvider.Control.UseColumns)
      //  throw new InvalidOperationException("Нельзя добавлять столбцы, когда UseColumns=false");

      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
      if (textWidth < 1)
        throw new ArgumentOutOfRangeException("textWidth");
      if (minTextWidth < 1)
        throw new ArgumentOutOfRangeException("minTextWidth");

      if (minTextWidth > textWidth)
        throw new ArgumentOutOfRangeException("minTextWidth", minTextWidth,
          "Минимальная ширина столбца (" + minTextWidth.ToString() + ") не может быть больше, чем устанавливаемая (" + textWidth.ToString() + ")");

      TreeColumn treeCol = null;
      if (ControlProvider.Control.UseColumns)
      {
        int w1 = ControlProvider.Measures.GetTextColumnWidth(textWidth);
        int w2 = ControlProvider.Measures.GetTextColumnWidth(minTextWidth);
        treeCol = new TreeColumn(headerText, w1);
        treeCol.MinColumnWidth = w2;
        ControlProvider.Control.Columns.Add(treeCol);
      }

      if (ControlProvider.Control.UseColumns)
        nodeControl.ParentColumn = treeCol;

      ControlProvider.Control.NodeControls.Add(nodeControl);

      if (ControlProvider.Control.UseColumns)
      {
        EFPDataTreeViewColumn efpCol = new EFPDataTreeViewColumn(ControlProvider, treeCol, nodeControl, columnName);
        _ColumnDict.Add(treeCol, efpCol);
      }
    }

    #endregion

    #region Text

    /// <summary>
    /// Создает элемент <see cref="NodeTextBox"/> для отображения текста и добавляет его в коллекцию. Если <see cref="TreeViewAdv.UseColumns"/>=true, то также создается столбец <see cref="TreeColumn"/>.
    /// Созданный объект <see cref="EFPDataTreeViewColumn"/> не возвращается. Для доступа к нему используйте свойство <see cref="LastAdded"/> или одну из перегрузок индексированного свойства, например, <see cref="this[string]"/>.
    /// </summary>
    /// <param name="columnName">Имя столбца, которое можно использовать для поиска объекта <see cref="EFPDataTreeViewColumn"/>. 
    /// Если <paramref name="isDataColumn"/>=true, то также задает свойство данных <see cref="BindableControl.DataPropertyName"/>.</param>
    /// <param name="isDataColumn">Если true, то элемент будет автоматически извлекать данные из объектов модели. 
    /// При этом <paramref name="columnName"/> должно указывать на имя свойства или поля объектов узлов модели, или на имя поля, если узлами модели являются <see cref="DataRow"/> или <see cref="DataRowView"/>.
    /// Если false, то извлечение данных должно выполняться прикладным кодом</param>
    /// <param name="headerText">Заголовок столбца <see cref="TreeColumn.Header"/></param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина столбца в текстовых единицах</param>
    /// <param name="alignment">Выравнивание текста (по левому краю, по центру или по правому краю)</param>
    /// <returns>Созданный элемент</returns>
    public NodeTextBox AddText(string columnName, bool isDataColumn, string headerText, int textWidth, int minTextWidth, HorizontalAlignment alignment)
    {
      NodeTextBox nodeControl = new NodeTextBox();
      nodeControl.TextAlign = alignment;
      //tb.EditEnabled = !ReadOnly;

      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, textWidth, minTextWidth);

      return nodeControl;
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

#endif

    #endregion

    #region Дата и время

    /// <summary>
    /// Добавляет столбец <see cref="NodeDateTimeBox"/> для отображения даты без времени.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным.
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Объект <see cref="NodeControl"/></returns>
    public NodeDateTimeBox AddDate(string columnName, bool isDataColumn, string headerText)
    {
      return AddDateTime(columnName, isDataColumn, headerText, EditableDateTimeFormatterKind.Date);
    }

    /// <summary>
    /// Добавляет столбец <see cref="NodeDateTimeBox"/> для отображения даты без времени.
    /// Столбец привязывается к полю данных <paramref name="columnName"/>.
    /// Заголовок столбца совпадает с именем поля.
    /// </summary>
    /// <param name="columnName">Имя столбца, столбца DataColumn и заголовок столбца</param>
    /// <returns>Объект <see cref="NodeControl"/></returns>
    public NodeDateTimeBox AddDate(string columnName)
    {
      return AddDateTime(columnName, true, columnName, EditableDateTimeFormatterKind.Date);
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn для отображения даты и времени.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Объект <see cref="NodeControl"/></returns>
    public NodeDateTimeBox AddDateTime(string columnName, bool isDataColumn, string headerText)
    {
      return AddDateTime(columnName, isDataColumn, headerText, EditableDateTimeFormatterKind.DateTime);
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn для отображения даты и времени.
    /// Столбец привязывается к полю данных <paramref name="columnName"/>.
    /// Заголовок столбца совпадает с именем поля.
    /// </summary>
    /// <param name="columnName">Имя столбца, столбца DataColumn и заголовок столбца</param>
    /// <returns>Объект <see cref="NodeControl"/></returns>
    public NodeDateTimeBox AddDateTime(string columnName)
    {
      return AddDateTime(columnName, true, columnName, EditableDateTimeFormatterKind.DateTime);
    }

    /// <summary>
    /// Добавляет текстовый столбец DataGridViewTextBoxColumn для отображения даты и/или времени.
    /// </summary>
    /// <param name="columnName">Имя столбца. Если столбец привязывается к данным, то должно совпадать с именем столбца DataColumn.</param>
    /// <param name="isDataColumn">True, если столбец будет привязан к данным (нужно установить свойство DataGridViewColumn.DataPropertyName).
    /// False - если столбец не привязывается к данным (например, вычисляемый столбец) или сам просмотр не связан с набором данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="kind">Тип даты и/или времени</param>
    /// <returns>Объект <see cref="NodeControl"/></returns>
    public NodeDateTimeBox AddDateTime(string columnName, bool isDataColumn, string headerText, EditableDateTimeFormatterKind kind)
    {
      NodeDateTimeBox nodeControl = new NodeDateTimeBox();
      nodeControl.Formatter = EditableDateTimeFormatters.Get(kind);
      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, nodeControl.Formatter.TextWidth, nodeControl.Formatter.TextWidth);
      if (ControlProvider.Control.UseColumns)
        LastAdded.SizeGroup = kind.ToString();

      return nodeControl;
    }

    #endregion

    public NodeDecimalEditBox AddDecimal(string columnName, bool isDataColumn, string headerText, int textWidth, int decimalPlaces, string sizeGroup)
    {
      NodeDecimalEditBox nodeControl = new NodeDecimalEditBox();
      nodeControl.DecimalPlaces = decimalPlaces;
      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, textWidth, textWidth);
      if (ControlProvider.Control.UseColumns)
        LastAdded.SizeGroup = sizeGroup;
      return nodeControl;
    }

    public NodeDoubleEditBox AddDouble(string columnName, bool isDataColumn, string headerText, int textWidth, int decimalPlaces, string sizeGroup)
    {
      NodeDoubleEditBox nodeControl = new NodeDoubleEditBox();
      nodeControl.DecimalPlaces = decimalPlaces;
      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, textWidth, textWidth);
      if (ControlProvider.Control.UseColumns)
        LastAdded.SizeGroup = sizeGroup;
      return nodeControl;
    }

    public NodeSingleEditBox AddSingle(string columnName, bool isDataColumn, string headerText, int textWidth, int decimalPlaces, string sizeGroup)
    {
      NodeSingleEditBox nodeControl = new NodeSingleEditBox();
      nodeControl.DecimalPlaces = decimalPlaces;
      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, textWidth, textWidth);
      if (ControlProvider.Control.UseColumns)
        LastAdded.SizeGroup = sizeGroup;
      return nodeControl;
    }


    public NodeIntEditBox AddInt(string columnName, bool isDataColumn, string headerText, int textWidth)
    {
      NodeIntEditBox nodeControl = new NodeIntEditBox();
      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, textWidth, textWidth);
      return nodeControl;
    }

    public NodeIntEditBox AddInt(string columnName)
    {
      return AddInt(columnName, true, columnName, 5);
    }

    public NodeCheckBox AddBool(string columnName, bool isDataColumn, string headerText)
    {
      NodeCheckBox nodeControl = new NodeCheckBox();
      if (isDataColumn)
        nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, headerText, 3, 3);
      if (ControlProvider.Control.UseColumns)
      {
        LastAdded.TreeColumn.Width = ControlProvider.Measures.CheckBoxColumnWidth;
        LastAdded.TreeColumn.MinColumnWidth = LastAdded.TreeColumn.Width;
        LastAdded.SizeGroup = "CheckBox";
      }
      return nodeControl;
    }

    public NodeCheckBox AddBool(string columnName)
    {
      return AddBool(columnName, true, columnName);
    }

    public NodeIcon AddImage(string columnName)
    {
      NodeIcon nodeControl = new NodeIcon();
      //if (isDataColumn)
      //  nodeControl.DataPropertyName = columnName;

      AddControl(nodeControl, columnName, String.Empty, 3, 3);
      if (ControlProvider.Control.UseColumns)
      {
        LastAdded.TreeColumn.Width = ControlProvider.Measures.ImageColumnWidth;
        LastAdded.TreeColumn.MinColumnWidth = LastAdded.TreeColumn.Width;
        LastAdded.TreeColumn.MaxColumnWidth = LastAdded.TreeColumn.Width;
        LastAdded.SizeGroup = "Image";
      }

      return nodeControl;
    }
#if XXX

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

    /// <summary>
    /// Возвращает последний столбец <see cref="EFPDataTreeViewColumn"/>, добавленный с помощью методов AddXXX().
    /// Свойство удобно использовать для установки дополнительных атрибутов столбца,
    /// которые не были установлены в методе AddXXX().
    /// В режиме <see cref="TreeViewAdv.UseColumns"/>=false свойство возвращает null, так как столбцы не создаются
    /// </summary>
    public EFPDataTreeViewColumn LastAdded
    {
      get
      {
        if (Count == 0)
          return null;
        else
          return this[Count - 1];
      }
    }

    #endregion

    #region Синхронное изменение размеров столбцов

    private bool _InsideColumnWidthChanged = false;

    private bool _ColumnWidthChangedErrorWasShown = false;

    private void Control_ColumnWidthChanged(object sender, TreeColumnEventArgs args)
    {
      if (_InsideColumnWidthChanged)
        return;
      _InsideColumnWidthChanged = true;
      try
      {
        try
        {
          this[args.Column].ResizeGroup();
        }
        catch (Exception e) // 29.03.2013
        {
          if (!_ColumnWidthChangedErrorWasShown)
          {
            _ColumnWidthChangedErrorWasShown = true;
            DebugTools.ShowException(e, "Ошибка при установке размеров одинаковых столбцов");
          }
        }
      }
      finally
      {
        _InsideColumnWidthChanged = false;
      }
    }

    #endregion

    #region Методы групповой установки свойств столбцов

    /// <summary>
    /// Установить свойство <see cref="InteractiveControl.EditEnabled"/> для всех столбцов
    /// </summary>
    /// <param name="value">Устанавливаемое значение (инвертированное) свойства <see cref="InteractiveControl.EditEnabled"/></param>
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

    /// <summary>
    /// Массив имен всех столбцов в просмотре (отладочное средство)
    /// Свойства <see cref="EFPDataTreeViewColumn.Name"/> 
    /// </summary>
    public string[] ControlProviderColumnNames
    {
      get
      {
        string[] a = new string[Count];
        for (int i = 0; i < a.Length; i++)
          a[i] = this[i].Name;
        return a;
      }
    }

    #endregion
  }

}
