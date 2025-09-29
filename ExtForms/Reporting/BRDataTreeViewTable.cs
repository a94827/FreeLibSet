using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Config;
using FreeLibSet.Controls;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Core;
using FreeLibSet.Models.Tree;
using FreeLibSet.Reporting;

namespace FreeLibSet.Forms.Reporting
{
  /// <summary>
  /// Объект для печати/экспорта иерархического просмотра
  /// </summary>
  public class BRDataTreeViewMenuOutItem : BRDataViewMenuOutItemBase
  {
    #region Конструктор

    /// <summary>
    /// Создание объекта для вывода иерархического просмотра
    /// </summary>
    /// <param name="code">Код объекта. Обычно, "Control"</param>
    /// <param name="controlProvider">Провайдер табличного просмотра</param>
    public BRDataTreeViewMenuOutItem(string code, EFPDataTreeView controlProvider)
      :base(code, controlProvider)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");

      DisplayName = Res.EFPTreeViewAdv_Name_Default;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер иерахического просмотра. Задается в конструкторе
    /// </summary>
    public new EFPDataTreeView ControlProvider { get { return (EFPDataTreeView)(base.ControlProvider); } }

    #endregion

    #region Обработчики событий

    /// <summary>
    /// Подготовка к выполнению действий
    /// </summary>
    protected override void OnPrepareAction(EventArgs args)
    {
      SettingsData.GetRequired<BRDataViewSettingsDataItem>().UseExpColumnHeaders = ControlProvider.Control.UseColumns;
      SettingsData.GetRequired<BRDataViewSettingsDataItem>().UseColorStyle = false;

      bool hasBoolColumns = false;
      foreach (NodeControl nc in ControlProvider.Control.NodeControls)
      {
        if (nc is NodeCheckBox)
        {
          hasBoolColumns = true;
          break;
        }
      }
      SettingsData.GetRequired<BRDataViewSettingsDataItem>().UseBoolMode = hasBoolColumns;
      base.OnPrepareAction(args);
    }

    /// <summary>
    /// Создание отчета из табличного просмотра
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnCreateReport(BRMenuOutItemCreateReportEventArgs args)
    {
      BRFontSettingsDataItem fontSettings = SettingsData.GetRequired<BRFontSettingsDataItem>();
      fontSettings.InitCellStyle(args.Report.DefaultCellStyle);
      args.Report.DocumentProperties = ControlProvider.DocumentProperties.Clone();
      BRSection sect = args.Report.Sections.Add();
      sect.PageSetup = SettingsData.GetItem<BRPageSettingsDataItem>().PageSetup;
      AddTitleAndFilterBands(sect, args);
      sect.Bands.Add(new BRDataTreeViewTable(sect, ControlProvider, SettingsData, args.ActionInfo.Action == BRAction.SendTo));

      base.OnCreateReport(args);
    }

    #endregion
  }


  /// <summary>
  /// Виртуальная таблица для иерархического просмотра
  /// </summary>
  internal class BRDataTreeViewTable : BRVirtualTable
  {
    #region Вложенные классы

    /// <summary>
    /// Описатель печатаемого столбца
    /// В TreeViewAdv и EFPDataTreeView одному столбцу TreeColumn/EFPDataTreeViewColumn может соответствовать несколько элементов NodeControl.
    /// При печати они разделяются на отдельные столбцы.
    /// </summary>
    private struct ColumnInfo
    {
      #region Поля

      /// <summary>
      /// Описатель столбца. Null для единственного столбца в режиме TreeViewAdv.UseColumns=false
      /// </summary>
      public EFPDataTreeViewColumn EFPColumn;

      /// <summary>
      /// Объект для извлечения данных в режиме "1 NodeControl - 1 BRColumn". 
      /// </summary>
      public InteractiveControl NodeControl;

      /// <summary>
      /// Объекты для извлечения данных в режиме "несколько NodeControl - 1 RBColumn"
      /// </summary>
      public InteractiveControl[] NodeControls;

      /// <summary>
      /// Ширина столбца в единицах 0.1мм
      /// </summary>
      public int Width;

      public bool AutoGrow;

      /// <summary>
      /// True для первого NodeControl в столбце
      /// </summary>
      public bool FirstInColumn;

      /// <summary>
      /// True для последнего NodeControl в столбце
      /// </summary>
      public bool LastInColumn;

      /// <summary>
      /// Если true, то для этого столбца будет устанавливаться отступ
      /// </summary>
      public bool UseIndent;

      public bool Repeatable;

      #endregion

      #region Методы

      public override string ToString()
      {
        if (EFPColumn == null)
          return NodeControl.ToString();
        else
          return EFPColumn.ToString();
      }

      #endregion
    }

    /// <summary>
    /// Класс нужен для вызова базового конструктора <see cref="BRVirtualTable"/>, чтобы передать количество строк и столбцов
    /// </summary>
    private class InternalInfo
    {
      #region Конструктор

      public InternalInfo(EFPDataTreeView controlProvider, SettingsDataList settingsData, bool useExport)
      {
        if (controlProvider == null)
          throw new ArgumentNullException("controlProvider");
        if (settingsData == null)
          throw new ArgumentNullException("settingsData");
        _ControlProvider = controlProvider;
        _ViewData = settingsData.GetRequired<BRDataViewSettingsDataItem>();
        BRPageSetup pageSetup = settingsData.GetRequired<BRPageSettingsDataItem>().PageSetup;
        BRFontSettingsDataItem fontData= settingsData.GetRequired<BRFontSettingsDataItem>();

        InitRows(useExport);

        InitColumns(useExport, pageSetup, fontData);

        _FirstDataRow = _Headers.RowCount;

        _SubHeaderNumberRowIndex = -1;
        if (controlProvider.Control.UseColumns && _ViewData.ColumnSubHeaderNumbers != BRDataViewColumnSubHeaderNumbersMode.None && _Headers2.RowCount == 1)
        {
          _SubHeaderNumberRowIndex = _FirstDataRow;
          _FirstDataRow++;
        }

        if (FirstDataRow == 0 && RowNodes.Length == 0)
        {
          // Если нет ни одной строки, то возникнет исключение при создании BRBand.
          // Добавляем одну пустую строку "заголовка"
          _Headers = new BRColumnHeaderArray(1, Columns.Length);
          _FirstDataRow = 1;
        }
      }

      private void InitRows(bool useExport)
      {
        if (_ControlProvider.Control.Model == null)
        {
          //_Rows = new TreePath[0];
          _RowNodes = new TreeNodeAdv[0];
        }
        else
        {
          List<TreeNodeAdv> lstNodes = new List<TreeNodeAdv>();

          if (useExport && _ViewData.ExpRange == EFPDataViewExpRange.Selected)
          {
            // Выбранные узлы в просмотре

            foreach (TreeNodeAdv node in _ControlProvider.Control.SelectedNodes)
            {
              //TreePath path=_ControlProvider.Control.GetPath(node)
              lstNodes.Add(node);
            }
          }
          else
          {
            // Все узлы модели

            foreach (TreePath path in new TreePathEnumerable(_ControlProvider.Control.Model))
            {
              TreeNodeAdv node = _ControlProvider.Control.FindNode(path, true);
              if (node == null)
                throw new BugException("Tree node not found");
              lstNodes.Add(node);
            }
          }
          _RowNodes = lstNodes.ToArray();
        }
      }


      private void InitColumns(bool useExport, BRPageSetup pageSetup, BRFontSettingsDataItem fontData)
      {
        if (_ControlProvider.GetFirstNodeControl<InteractiveControl>() == null)
          throw new InvalidOperationException(Res.BRDataTreeViewTable_Err_NoInteractiveControl);

        // Выводимые столбцы для TreeViewAdv не зависят от режима ExpRange, так как выбирается строка целиком

        List<ColumnInfo> lst = new List<ColumnInfo>();
        bool useIndent = true;
        if (_ControlProvider.Control.UseColumns)
        {
          #region UseColumns=true

          List<string[]> headList = new List<string[]>();
          List<string[]> headList2 = new List<string[]>();

          int cntCol = 0;
          foreach (EFPDataTreeViewColumn efpCol in ControlProvider.Columns)
          {
            if (!efpCol.TreeColumn.IsVisible)
              continue;
            if (!efpCol.Printable)
              continue;

            if (!_ViewData.GetColumnPrinted(efpCol))
              continue;

            int w = _ViewData.GetRealColumnWidth(efpCol, fontData);
            InteractiveControl[] ctrs = ControlProvider.GetNodeControls<InteractiveControl>(efpCol.TreeColumn);
            for (int i = 0; i < ctrs.Length; i++)
            {
              ColumnInfo ci = new ColumnInfo();
              ci.EFPColumn = efpCol;
              ci.NodeControl = ctrs[i];
              ci.Width = w / ctrs.Length; // TODO: Распределение ширины
              ci.AutoGrow = _ViewData.GetColumnAutoGrow(efpCol);
              ci.FirstInColumn = (i == 0);
              ci.LastInColumn = (i == (ctrs.Length - 1));
              if (ctrs[i] is BaseTextControl && useIndent)
              {
                ci.UseIndent = true;
                useIndent = false;
              }

              ci.Repeatable = cntCol < _ViewData.RepeatedColumnCount;

              lst.Add(ci);

              if (efpCol.PrintHeaders != null)
                headList.Add(efpCol.PrintHeaders);
              else if (!String.IsNullOrEmpty(efpCol.TreeColumn.Header))
                headList.Add(new string[1] { efpCol.TreeColumn.Header });
              else
                headList.Add(EmptyArray<string>.Empty);
              headList2.Add(new string[1] { (cntCol + 1).ToString() });
            }

            cntCol++;
          }

          if (useExport && (!_ViewData.ExpColumnHeaders))
          {
            _Headers = new BRColumnHeaderArray(0, headList.Count);
            _Headers2 = null;
          }
          else
          {
            _Headers = new BRColumnHeaderArray(headList.ToArray(), _ControlProvider.ColumnHeaderMixedSpanAllowed);
            _Headers2 = new BRColumnHeaderArray(headList2.ToArray());

#if DEBUG
            if (_Headers2.RowCount > 1)
              throw new BugException("Headers2");
#endif
          }

          _Columns = lst.ToArray();

          #endregion
        }
        else
        {
          #region UseColumns = false

          ColumnInfo ci = new ColumnInfo();

          InteractiveControl[] ctrs = ControlProvider.GetNodeControls<InteractiveControl>();
          if (ctrs.Length == 1)
            ci.NodeControl = ctrs[0];
          else
            ci.NodeControls = ctrs;
          ci.UseIndent = true;
          ci.Width = 0;
          ci.FirstInColumn = true;
          ci.LastInColumn = true;
          _Columns = new ColumnInfo[1] { ci };
          _Headers = new BRColumnHeaderArray(0, 1);
          _Headers2 = null;

          #endregion
        }
      }


      public EFPDataTreeView ControlProvider { get { return _ControlProvider; } }
      private EFPDataTreeView _ControlProvider;

      public BRDataViewSettingsDataItem ViewData { get { return _ViewData; } }
      private BRDataViewSettingsDataItem _ViewData;

      public TreeNodeAdv[] RowNodes { get { return _RowNodes; } }
      private TreeNodeAdv[] _RowNodes;


      /// <summary>
      /// Индекс первой строки данных, с учетом заголовков
      /// </summary>
      public int FirstDataRow { get { return _FirstDataRow; } }
      private int _FirstDataRow;

      public ColumnInfo[] Columns { get { return _Columns; } }
      private ColumnInfo[] _Columns;

      public BRColumnHeaderArray Headers { get { return _Headers; } }
      private BRColumnHeaderArray _Headers;

      /// <summary>
      /// Номера столбцов как однострочный объект заголовков
      /// </summary>
      public BRColumnHeaderArray Headers2 { get { return _Headers2; } }
      private BRColumnHeaderArray _Headers2;


      /// <summary>
      /// Индекс строки, в которой выводятся номера столбцов 1,2,3...
      /// Если нумерация не используется, то возвращает (-1).
      /// </summary>
      public int SubHeaderNumberRowIndex { get { return _SubHeaderNumberRowIndex; } }
      private int _SubHeaderNumberRowIndex;

      #endregion
    }

    #endregion

    #region Конструктор

    public BRDataTreeViewTable(BRSection section, EFPDataTreeView controlProvider, SettingsDataList settingsData, bool useExport)
      : this(section, new InternalInfo(controlProvider, settingsData, useExport))
    {
    }

    private BRDataTreeViewTable(BRSection section, InternalInfo info)
      : base(section, info.RowNodes.Length + info.FirstDataRow, info.Columns.Length)
    {
      _Info = info;
      _SB = new StringBuilder();
    }

    private readonly InternalInfo _Info;

    private readonly StringBuilder _SB;

    #endregion

    #region Реализация методов

    protected override object GetValue(int rowIndex, int columnIndex)
    {
      if (rowIndex >= _Info.FirstDataRow)
      {
        TreeNodeAdv node = _Info.RowNodes[rowIndex - _Info.FirstDataRow];
        InteractiveControl nc = _Info.Columns[columnIndex].NodeControl;
        if (nc != null)
        {
          // Обычный режим: один столбец отчета - один NodeControl
          return DoGetValue(node, nc);

        }
        else
        {
          // Несколько NodeControl для столбца отчета
          // Превращаем каждое значение в строку

          _SB.Length = 0;

          for (int i = 0; i < _Info.Columns[columnIndex].NodeControls.Length; i++)
          {
            object v = DoGetValue(node, _Info.Columns[columnIndex].NodeControls[i]);
            if (v == null)
              continue;
            if (v is IFormattable)
            {
              BaseFormattedTextControl ctlFormat = _Info.Columns[columnIndex].NodeControls[i] as BaseFormattedTextControl;
              if (ctlFormat != null)
                v = ((IFormattable)v).ToString(ctlFormat.Format, ctlFormat.FormatProvider);
            }

            string s = v.ToString();
            if (s.Length > 0)
            {
              if (_SB.Length > 0)
                _SB.Append(" ");
              _SB.Append(s);
            }
          }

          return _SB.ToString();
        }
      }
      else if (rowIndex < +_Info.Headers.RowCount)
      {
        return _Info.Headers.Text[rowIndex, columnIndex];
      }
      else if (rowIndex == _Info.SubHeaderNumberRowIndex)
        return _Info.Headers2.Text[0, columnIndex];
      else
        throw new BugException();
    }

    private object DoGetValue(TreeNodeAdv node, InteractiveControl nc)
    {
      object v = nc.GetValue(node);
      if (v is Boolean)
        v = _Info.ViewData.GetBooleanValue((bool)v);
      return v;
    }

    protected override void FillCellStyle(int rowIndex, int columnIndex, BRCellStyle style)
    {
      style.LeftMargin = _Info.ViewData.CellLeftMargin;
      style.TopMargin = _Info.ViewData.CellTopMargin;
      style.RightMargin = _Info.ViewData.CellRightMargin;
      style.BottomMargin = _Info.ViewData.CellBottomMargin;

      if (rowIndex >= _Info.FirstDataRow)
      {
        #region Область данных

        if (_Info.ViewData.BorderStyle == BRDataViewBorderStyle.All)
        {
          style.TopBorder = BRLine.Thin;
          style.BottomBorder = BRLine.Thin;
        }

        if (_Info.ViewData.BorderStyle == BRDataViewBorderStyle.All || _Info.ViewData.BorderStyle == BRDataViewBorderStyle.Vertical)
        {
          if (_Info.Columns[columnIndex].FirstInColumn)
          {
            //if (_Info.Columns[columnIndex].EFPColumn == null)
            style.LeftBorder = BRLine.Thin;
            //else (_Info.Columns[columnIndex].EFPColumn.LeftBorder.)
          }
          else
            style.LeftBorder = BRLine.None;

          if (_Info.Columns[columnIndex].LastInColumn)
          {
            //if (_Info.Columns[columnIndex].EFPColumn == null)
            style.RightBorder = BRLine.Thin;
            //else (_Info.Columns[columnIndex].EFPColumn.LeftBorder.)
          }
          else
            style.LeftBorder = BRLine.None;
        }

        style.VAlign = BRVAlign.Center;
        style.WrapMode = BRWrapMode.WordWrap;

        if (_Info.Columns[columnIndex].UseIndent)
          style.IndentLevel = _Info.RowNodes[rowIndex - _Info.FirstDataRow].Level - 1;
        if (_Info.Columns[columnIndex].NodeControl == null)
        {
          style.HAlign = BRHAlign.Left;
          style.VAlign = BRVAlign.Center;
        }
        else
        {
          BaseTextControl ctlText = _Info.Columns[columnIndex].NodeControl as BaseTextControl;
          if (ctlText != null)
          {
            switch (ctlText.TextAlign)
            {
              case System.Windows.Forms.HorizontalAlignment.Left: style.HAlign = BRHAlign.Left; break;
              case System.Windows.Forms.HorizontalAlignment.Center: style.HAlign = BRHAlign.Center; break;
              case System.Windows.Forms.HorizontalAlignment.Right: style.HAlign = BRHAlign.Right; break;
            }
            switch (ctlText.VerticalAlign)
            {
              case System.Windows.Forms.VisualStyles.VerticalAlignment.Top: style.VAlign = BRVAlign.Top; break;
              case System.Windows.Forms.VisualStyles.VerticalAlignment.Center: style.VAlign = BRVAlign.Center; break;
              case System.Windows.Forms.VisualStyles.VerticalAlignment.Bottom: style.VAlign = BRVAlign.Bottom; break;
            }
          }
          else
          {
            style.HAlign = BRHAlign.Center;
            style.VAlign = BRVAlign.Center;
          }
          BaseFormattedTextControl ctlFormat = _Info.Columns[columnIndex].NodeControl as BaseFormattedTextControl;
          if (ctlFormat != null)
          {
            style.Format = ctlFormat.Format;
            style.FormatProvider = ctlFormat.FormatProvider;
          }
        }

        #endregion
      }
      else if (rowIndex < _Info.Headers.RowCount)
      {
        #region Заголовки

        style.HAlign = BRHAlign.Center;
        if (_Info.ViewData.BorderStyle != BRDataViewBorderStyle.None)
          style.AllBorders = BRLine.Thin;
        style.WrapMode = BRWrapMode.WordWrap;

        #endregion
      }
      else if (rowIndex == _Info.SubHeaderNumberRowIndex)
      {
        #region Нумерация столбцов

        style.HAlign = BRHAlign.Center;
        if (_Info.ViewData.BorderStyle != BRDataViewBorderStyle.None)
          style.AllBorders = BRLine.Thin;

        #endregion
      }
      else
        throw new BugException();
    }

    protected override void FillColumnInfo(int columnIndex, BRColumnInfo columnInfo)
    {
      if (_Info.Columns[columnIndex].Width > 0)
        columnInfo.SetWidth(_Info.Columns[columnIndex].Width, _Info.Columns[columnIndex].AutoGrow);
      columnInfo.Repeatable = _Info.Columns[columnIndex].Repeatable;
    }

    protected override void FillRowInfo(int rowIndex, BRRowInfo rowInfo)
    {
      if (rowIndex < _Info.Headers.RowCount)
      {
        rowInfo.KeepWithNext = true;
        if (_Info.ViewData.ColumnSubHeaderNumbers != BRDataViewColumnSubHeaderNumbersMode.Replace)
          rowInfo.Repeatable = true;
      }
      else if (rowIndex == _Info.SubHeaderNumberRowIndex)
      {
        rowInfo.KeepWithNext = true;
        rowInfo.Repeatable = true;
      }
      else if (rowIndex == (_Info.RowNodes.Length - 1 + _Info.FirstDataRow))
        rowInfo.KeepWithPrev = true;
    }

    protected override BRRange GetMergeInfo(int rowIndex, int columnIndex)
    {
      if (rowIndex < _Info.Headers.RowCount)
      {
        int firstRowIndex, firstColumn, rowCount, columnCount;
        _Info.Headers.GetMergeArea(rowIndex, columnIndex, out firstRowIndex, out firstColumn, out rowCount, out columnCount);
        return new BRRange(firstRowIndex, firstColumn, rowCount, columnCount);
      }
      else if (rowIndex == _Info.SubHeaderNumberRowIndex)
      {
        int firstRowIndex, firstColumn, rowCount, columnCount;
        _Info.Headers2.GetMergeArea(0, columnIndex, out firstRowIndex, out firstColumn, out rowCount, out columnCount);
        return new BRRange(firstRowIndex + rowIndex, firstColumn, rowCount, columnCount);
      }
      else
        return base.GetMergeInfo(rowIndex, columnIndex);
    }

    #endregion
  }
}
