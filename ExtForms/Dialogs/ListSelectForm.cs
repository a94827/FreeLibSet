// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.Text;

namespace FreeLibSet.Forms
{
  #region Форма для реализации ListSelectDialog

  /// <summary>
  /// Форма для реализации ListSelectDialog
  /// </summary>
  internal partial class ListSelectForm : Form
  {
    #region Конструктор формы

    public ListSelectForm(ListSelectDialog owner)
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);
      efpForm.FormChecks.Add(new UIValidatingEventHandler(ValidateForm));
      efpForm.OwnStatusBar = true; // удобнее для быстрого поиска
      efpForm.ConfigSectionName = owner.ConfigSectionName;
      if (!String.IsNullOrEmpty(owner.HelpContext))
        efpForm.HelpContext = owner.HelpContext;

      Init(efpForm, owner);

      btnOk.Enabled = _Owner.CanBeEmpty || _Owner.Items.Length > 0;
    }

    /// <summary>
    /// Конструктор для WizardStep
    /// </summary>
    public ListSelectForm()
    {
      InitializeComponent();
    }


    public void Init(EFPBaseProvider baseProvider, IListSelectDialogInternal owner)
    {
      _Owner = owner;

      TheGroupBox.Text = _Owner.ListTitle;

      efpGrid = new EFPDataGridView(baseProvider, theGrid);
      efpGrid.Control.AutoGenerateColumns = false;
      if (_Owner.MultiSelect)
        efpGrid.Columns.AddCheckBox("CheckBox", false, String.Empty);
      if (EFPApp.ShowListImages)
        efpGrid.Columns.AddImage("Image");
      efpGrid.Columns.AddText("Item", false, String.Empty, 10, 1);
      efpGrid.Columns.LastAdded.CanIncSearch = true;
      if (_Owner.SubItems != null)
        efpGrid.Columns.AddText("SubItem", false, String.Empty, 10, 1);
      efpGrid.Control.ColumnHeadersVisible = false;
      efpGrid.Control.RowHeadersVisible = false;
      efpGrid.DisableOrdering();
      efpGrid.ReadOnly = true;
      efpGrid.Control.ReadOnly = true;
      efpGrid.CanView = false;
      efpGrid.Control.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      efpGrid.CellInfoNeeded += EfpGrid_CellInfoNeeded;
      //efpGrid.CommandItems.PasteHandler.Visible = false; // 23.05.2025
      efpGrid.CommandItems.AddTextPasteFormats();
      efpGrid.CommandItems.PasteHandler.Clear();
      if (_Owner.MultiSelect)
      {
        CheckMarks = new SelectionFlagList(_Owner.Items.Length);
        efpGrid.MarkRowsColumnName = "CheckBox";
        efpGrid.Control.CellValuePushed += Control_CellValuePushed;
      }
      efpGrid.Control.VirtualMode = true;

      efpGrid.Control.RowCount = _Owner.Items.Length;
      efpGrid.CommandItems.EnterAsOk = true;
      efpGrid.ConfigSectionName = _Owner.ConfigSectionName;

      if (_Owner is WizardStepWithListSelection)
      {
        // Для ListSelectDialog это делается в OnLoad()
        efpGrid.Columns.LastAdded.GridColumn.FillWeight = 100;
        efpGrid.Columns.LastAdded.GridColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      }
      efpGrid.DocumentProperties.Title = owner.OutItemTitle; // продублируется в заголовок таблицы

      btnCheckAll.Image = EFPApp.MainImages.Images["CheckListAll"];
      btnCheckAll.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpCheckAll = new EFPButton(baseProvider, btnCheckAll);
      efpCheckAll.Click += efpCheckAll_Click;

      btnUnCheckAll.Image = EFPApp.MainImages.Images["CheckListNone"];
      btnUnCheckAll.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpUnCheckAll = new EFPButton(baseProvider, btnUnCheckAll);
      efpUnCheckAll.Click += efpUnCheckAll_Click;

      btnCheckAll.Visible = btnUnCheckAll.Visible = _Owner.MultiSelect;

      // 25.10.2019 - блокируем бесполезные кнопки
      efpCheckAll.Enabled = _Owner.Items.Length > 0;
      efpUnCheckAll.Enabled = _Owner.Items.Length > 0;

      if (_Owner.ClipboardMode != ListSelectDialogClipboardMode.None)
      {
        btnCopy.Image = EFPApp.MainImages.Images["Copy"];
        btnCopy.ImageAlign = ContentAlignment.MiddleCenter;
        EFPButton efpCopy = new EFPButton(baseProvider, btnCopy);
        efpCopy.DisplayName = EFPCommandItem.RemoveMnemonic(Res.Cmd_Menu_Edit_Copy);
        efpCopy.ToolTipText = Res.ListSelectDialog_ToolTip_Copy;
        efpCopy.Click += new EventHandler(efpCopy_Click);

        btnPaste.Image = EFPApp.MainImages.Images["Paste"];
        btnPaste.ImageAlign = ContentAlignment.MiddleCenter;
        EFPButton efpPaste = new EFPButton(baseProvider, btnPaste);
        efpPaste.DisplayName = EFPCommandItem.RemoveMnemonic(Res.Cmd_Menu_Edit_Paste);
        efpPaste.ToolTipText = Res.ListSelectDialog_ToolTip_Paste;
        efpPaste.Click += new EventHandler(efpPaste_Click);
      }
      else
      {
        btnCopy.Visible = false;
        btnPaste.Visible = false;
      }

      if ((_Owner is WizardStepWithListSelection) && 
        (!_Owner.MultiSelect) && 
        (_Owner.ClipboardMode == ListSelectDialogClipboardMode.None))

        ButtonPanel.Visible = false;
    }

    protected override void OnLoad(EventArgs args)
    {
      base.OnLoad(args);

      // 09.02.2020
      // Подбор размеров по ширине списка
      int w = 0;
      efpGrid.Control.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
      for (int i = 0; i < efpGrid.Columns.Count; i++)
        w += efpGrid.Columns[i].Width;
      w += 4 * SystemInformation.BorderSize.Width + SystemInformation.VerticalScrollBarWidth;
      w = Math.Min(w, SystemInformation.VirtualScreen.Width); // 14.05.2021
      int dw = w - efpGrid.Control.Width;
      if (dw > 0)
        this.Width = Math.Min(this.Width + dw, SystemInformation.VirtualScreen.Width);

      efpGrid.Columns.LastAdded.GridColumn.FillWeight = 100;
      efpGrid.Columns.LastAdded.GridColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

      // 14.05.2021
      // Подбор размеров по высоте списка
      if (_Owner.Items.Length > 0)
      {
        int h = efpGrid.Control.Rows[0].Height * _Owner.Items.Length;
        h += 4 * SystemInformation.BorderSize.Width +
          SystemInformation.HorizontalScrollBarHeight; /* 14.05.2021 */
        h = Math.Min(h, SystemInformation.VirtualScreen.Height);
        int dh = h - efpGrid.Control.Height;

        if (dh > 0)
          this.Height = Math.Min(this.Height + dh, SystemInformation.VirtualScreen.Height);
      }

      WinFormsTools.PlaceFormInScreen(this);
    }

    #endregion

    #region Поля

    private IListSelectDialogInternal _Owner;

    public SelectionFlagList CheckMarks;

    #endregion

    #region Табличный просмотр

    public EFPDataGridView efpGrid;

    private void EfpGrid_CellInfoNeeded(object sender, EFPDataGridViewCellInfoEventArgs args)
    {
      switch (args.ColumnName)
      {
        case "CheckBox":
          args.Value = CheckMarks[args.RowIndex];
          break;
        case "Image":
          if (_Owner.HasImages && _Owner.Images[args.RowIndex] != null)
            args.Value = _Owner.Images[args.RowIndex];
          else if (_Owner.HasImageKeys && (!String.IsNullOrEmpty(_Owner.ImageKeys[args.RowIndex])))
            args.Value = EFPApp.MainImages.Images[_Owner.ImageKeys[args.RowIndex]];
          else if (_Owner.Image != null)
            args.Value = _Owner.Image;
          else if (!String.IsNullOrEmpty(_Owner.ImageKey))
            args.Value = EFPApp.MainImages.Images[_Owner.ImageKey];
          else
            args.Value = EFPApp.MainImages.Images["Item"];
          break;
        case "Item":
          args.Value = _Owner.Items[args.RowIndex];
          break;
        case "SubItem":
          args.Value = _Owner.SubItems[args.RowIndex];
          break;
      }
    }

    private void Control_CellValuePushed(object sender, DataGridViewCellValueEventArgs args)
    {
      CheckMarks[args.RowIndex] = (bool)(args.Value);
    }

    #endregion

    #region Проверка формы

    private void ValidateForm(object sender, UIValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;
      if (_Owner.CanBeEmpty)
        return;

      if (CheckMarks != null)
      {
        if (CheckMarks.AreAllUnselected)
        {
          args.SetError(Res.ListSelectDialog_Err_NoneChecked);
          return;
        }
      }
      else
      {
        if (efpGrid.CurrentGridRow == null)
        {
          args.SetError(Res.ListSelectDialog_Err_NoSelection);
        }
      }
    }

    #endregion

    #region Кнопки установки / снятия отметов

    private void efpCheckAll_Click(object sender, EventArgs args)
    {
      efpGrid.CheckMarkRows(EFPDataGridViewCheckMarkRows.All, EFPDataGridViewCheckMarkAction.Check);
    }

    private void efpUnCheckAll_Click(object sender, EventArgs args)
    {
      efpGrid.CheckMarkRows(EFPDataGridViewCheckMarkRows.All, EFPDataGridViewCheckMarkAction.Uncheck);
    }

    #endregion

    #region Команды буфера обмена

    void efpCopy_Click(object sender, EventArgs args)
    {
      switch (_Owner.ClipboardMode)
      {
        case ListSelectDialogClipboardMode.CommaCodes:
          if (CheckMarks != null)
          {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _Owner.Items.Length; i++)
            {
              if (CheckMarks[i])
              {
                if (sb.Length > 0)
                  sb.Append(", ");
                sb.Append(_Owner.Items[i]);
              }
            }
            if (sb.Length == 0)
            {
              EFPApp.ShowTempMessage(Res.ListSelectDialog_Err_NoneChecked);
              return;
            }
            new EFPClipboard().SetText(sb.ToString());
          }
          else
          {
            if (efpGrid.CurrentRowIndex >= 0)
              new EFPClipboard().SetText(_Owner.Items[efpGrid.CurrentRowIndex]);
            else
              EFPApp.ShowTempMessage(Res.ListSelectDialog_Err_NoSelection);
          }
          break;

        default:
          throw new BugException("ClipboardMode=" + _Owner.ClipboardMode.ToString());
      }
    }

    /// <summary>
    /// Ключ - текст элемента, значение - индекс элемента.
    /// Используется для вставки ссылок из буфера обмена.
    /// </summary>
    private TypedStringDictionary<int> _ItemDict;

    void efpPaste_Click(object sender, EventArgs args)
    {
      EFPButton efpPaste = (EFPButton)sender;

      #region Создаем словарь элементов

      if (_ItemDict == null)
      {
        _ItemDict = new TypedStringDictionary<int>(_Owner.Items.Length, false);
        for (int i = 0; i < _Owner.Items.Length; i++)
          _ItemDict[_Owner.Items[i]] = i; // вдруг есть одинаковые строки
      }

      #endregion

      switch (_Owner.ClipboardMode)
      {
        case ListSelectDialogClipboardMode.CommaCodes:
          EFPClipboard clp = new EFPClipboard();
          clp.ErrorIfEmpty = true;
          string s = clp.GetText();
          if (String.IsNullOrEmpty(s))
            return;
          string[] a = s.Split(',');
          if (CheckMarks != null)
          {
            List<int> lst1 = new List<int>();
            List<int> lst2 = new List<int>();
            for (int i = 0; i < a.Length; i++)
            {
              s = a[i].Trim();
              int itemIndex;
              if (_ItemDict.TryGetValue(s, out itemIndex))
              {
                if (CheckMarks[itemIndex])
                  lst1.Add(itemIndex);
                else
                  lst2.Add(itemIndex);
              }
              else
              {
                EFPApp.ShowTempMessage(String.Format(Res.ListSelectDialog_Err_ItemNotFound, s));
                return;
              }
            }
            // lst содержит список элементов, которые нужно отметить
            if (lst2.Count == 0 && lst1.Count == a.Length)
            {
              EFPApp.ShowTempMessage(Res.ListSelectDialog_Err_NoChanges);
              return;
            }
            if (!CheckMarks.AreAllUnselected)
            {
              RadioSelectDialog dlg2 = new RadioSelectDialog();
              dlg2.Title = efpPaste.DisplayName;
              dlg2.ImageKey = "Paste";
              dlg2.Items = new string[]{
                Res.ListSelectDialog_Msg_PasteAdd,
                Res.ListSelectDialog_Msg_PasteReplace};
              dlg2.ImageKeys = new string[] { "Insert", "Replace" };
              if (dlg2.ShowDialog() != DialogResult.OK)
                return;
              if (dlg2.SelectedIndex == 0)
              {
                for (int i = 0; i < lst2.Count; i++)
                  CheckMarks[lst2[i]] = true;
                efpGrid.InvalidateColumn("CheckBox");
                return;
              }
            }

            // Режим замены выделения
            CheckMarks.UnselectAll();
            for (int i = 0; i < lst1.Count; i++)
              CheckMarks[lst1[i]] = true;
            for (int i = 0; i < lst2.Count; i++)
              CheckMarks[lst2[i]] = true;
            efpGrid.InvalidateColumn("CheckBox");
          }
          else // ! CheckBoxes
          {
            if (a.Length > 1)
            {
              EFPApp.ShowTempMessage(Res.ListSelectDialog_Err_PasteTextWithCommas);
              return;
            }
            s = a[0].Trim();
            int itemIndex;
            if (_ItemDict.TryGetValue(s, out itemIndex))
            {
              CheckMarks[itemIndex] = true;
              efpGrid.InvalidateCell(itemIndex, "CheckBox");
            }
            else
              EFPApp.ShowTempMessage(String.Format(Res.ListSelectDialog_Err_ItemNotFound, s));
          }
          break;

        default:
          throw new BugException("ClipboardMode=" + _Owner.ClipboardMode.ToString());
      }
    }

    #endregion
  }

  #endregion

  #region Перечисление ListSelectDialogClipboardMode

  /// <summary>
  /// Режимы использования буфера обмена в диалоге <see cref="ListSelectDialog"/>
  /// </summary>
  public enum ListSelectDialogClipboardMode
  {
    /// <summary>
    /// Буфер обмена не используется
    /// </summary>
    None = 0,

    /// <summary>
    /// Если <see cref="ListSelectDialog.MultiSelect"/>=true, в буфер обмена копируются отмеченные флажками элементы, разделенные запятыми, в виде одной строки текста.
    /// Дополнительные пробелы не добавляются. В режиме <see cref="ListSelectDialog.MultiSelect"/>=false копируется текущий элемент.
    /// Дополнительный столбец <see cref="ListSelectDialog.SubItems"/> не копируется, даже если он есть.
    /// Режим можно использовать, только если в списке <see cref="ListSelectDialog.Items"/> гарантированно нет запятых.
    /// </summary>
    CommaCodes = 1,
  }

  #endregion

  internal interface IListSelectDialogInternal
  {
    string ListTitle { get; }
    string OutItemTitle { get; }
    ListSelectDialogClipboardMode ClipboardMode { get; }
    string ConfigSectionName { get; }
    string[] Items { get; }
    string[] SubItems { get; }
    int SelectedIndex { get; set; }
    bool CanBeEmpty { get; }
    bool MultiSelect { get; }
    bool[] Selections { get; set; }
    string ImageKey { get; }
    Image Image { get; }
    string[] ImageKeys { get; }
    bool HasImageKeys { get; }
    Image[] Images { get; }
    bool HasImages { get; }
  }

  /// <summary>
  /// Диалог выбора одной или нескольких позиций из списка.
  /// Для добавления шага мастера с аналогичными возможностями, используйте <see cref="WizardStepWithListSelection"/>.
  /// </summary>
  public class ListSelectDialog: IListSelectDialogInternal
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога
    /// </summary>
    public ListSelectDialog()
    {
      _MultiSelect = false;
      _CanBeEmpty = false;
      _SelectedIndex = -1;
      _DialogPosition = new EFPDialogPosition();
      _ClipboardMode = ListSelectDialogClipboardMode.None;
      _ConfigSectionName = String.Empty;
    }

    #endregion

    // При добавлении свойств и методов не забыть про WizardStepWithListSelect

    #region Свойства формы

    /// <summary>
    /// Заголовок формы.
    /// Если свойство не установлено в явном виде, используется заголовок по умолчанию.
    /// </summary>
    public string Title
    {
      get
      {
        if (_Title == null)
          return DefaultTitle;
        else
          return _Title;
      }
      set
      {
        _Title = value;
      }
    }
    private string _Title;

    private string DefaultTitle
    {
      get
      {
        if (MultiSelect)
          return Res.ListSelectDialog_Msg_TitleMultiSelect;
        else
          return Res.ListSelectDialog_Msg_TitleSingleSelect;
      }
    }

    /// <summary>
    /// Заголовок над списком.
    /// По умолчанию - пустая строка - заголовок не выводится
    /// </summary>
    public string ListTitle
    {
      get { return _ListTitle ?? String.Empty; }
      set { _ListTitle = value; }
    }
    private string _ListTitle;

    /// <summary>
    /// Заголовок таблицы при печати списка / экспорте в файл.
    /// По умолчанию совпадает с <see cref="ListTitle"/>, если он задан, иначе - с <see cref="Title"/>.
    /// </summary>
    public string OutItemTitle
    {
      get
      {
        if (_OutItemTitle == null)
        {
          if (String.IsNullOrEmpty(ListTitle))
            return Title;
          else
            return ListTitle;
        }
        else
          return _OutItemTitle;
      }
      set
      {
        _OutItemTitle = value;
      }
    }
    private string _OutItemTitle;

    /// <summary>
    /// Контекст справки, вызываемой по нажатию клавиши F1
    /// </summary>
    public string HelpContext { get { return _HelpContext; } set { _HelpContext = value; } }
    private string _HelpContext;


    /// <summary>
    /// Позиция блока диалога на экране.
    /// По умолчанию блок диалога центрируется относительно <see cref="EFPApp.DefaultScreen"/>.
    /// Можно либо модифицировать свойства существующего объекта, либо присвоить свойству ссылку на новый объект <see cref="EFPDialogPosition"/>.
    /// </summary>
    public EFPDialogPosition DialogPosition
    {
      get { return _DialogPosition; }
      set
      {
        if (value == null)
          _DialogPosition = new EFPDialogPosition();
        else
          _DialogPosition = value;
      }
    }
    private EFPDialogPosition _DialogPosition;

    /// <summary>
    /// Можно ли использовать команды копирования и вставки из буфера обмена.
    /// По умолчанию - None - копирование недоступно.
    /// </summary>
    public ListSelectDialogClipboardMode ClipboardMode { get { return _ClipboardMode; } set { _ClipboardMode = value; } }
    private ListSelectDialogClipboardMode _ClipboardMode;

    /// <summary>
    /// Имя секции конфигурации.
    /// Если задано, то будет сохраняться размер, положение (при пустом <see cref="DialogPosition"/>) и состояние (обычный размер/на весь экран)
    /// блока диалога между сеансами работы программы.
    /// По умолчанию - пустая строка - расположение не сохраняется.
    /// </summary>
    public string ConfigSectionName
    {
      get { return _ConfigSectionName; }
      set { _ConfigSectionName = value; }
    }
    private string _ConfigSectionName;

    #endregion

    #region Список элементов

    /// <summary>
    /// Список для выбора.
    /// Это свойство должно задаваться обязательно и в первую очередь.
    /// Установка свойства очищает остальные списочные свойства.
    /// </summary>
    public string[] Items
    {
      get
      {
        return _Items;
      }
      set
      {
        _Items = value;
        _SubItems = null;
        _Images = null;
        _ImageKeys = null;
        _Selections = null;
        _Codes = null;
      }
    }
    private string[] _Items;

    /// <summary>
    /// Список строк для второго столбца.
    /// Если свойство равно null (по умолчанию), то второго столбца нет.
    /// Свойство может устанавливаться только после свойства Items
    /// </summary>
    public string[] SubItems
    {
      get { return _SubItems; }
      set
      {
        if (value != null)
        {
          if (_Items == null)
            throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");
          if (value.Length != _Items.Length)
            throw ExceptionFactory.ArgWrongCollectionCount("value", value, _Items.Length);
        }
        _SubItems = value;
      }
    }
    private string[] _SubItems;

    #endregion

    #region Текущая позиция

    /// <summary>
    /// Текущая позиция при отключенном <see cref="MultiSelect"/> 
    /// </summary>
    public int SelectedIndex
    {
      get { return _SelectedIndex; }
      set { _SelectedIndex = value; }
    }
    private int _SelectedIndex;

    /// <summary>
    /// Установка и получение выбранной позиции как строки.
    /// Выполняет поиск в списке <see cref="Items"/>
    /// </summary>
    public string SelectedItem
    {
      get
      {
        if (Items == null || SelectedIndex < 0)
          return String.Empty;
        else
          return Items[SelectedIndex];
      }
      set
      {
        if (Items == null)
          throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");
        SelectedIndex = Array.IndexOf<string>(Items, value);
      }
    }

    /// <summary>
    /// True, если пользователь может нажимать "ОК", если нет выбранной позиции в списке (при <see cref="MultiSelect"/>=false)
    /// или не отмечено ни одного флажка (при <see cref="MultiSelect"/>=true).
    /// По умолчанию - false.
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    #endregion

    #region Выбор нескольких позиций

    /// <summary>
    /// True, если разрешено выбирать несколько позиций
    /// </summary>
    public bool MultiSelect
    {
      get
      {
        return _MultiSelect;
      }
      set
      {
        if (value == _MultiSelect)
          return;
        _MultiSelect = value;
        _Selections = null;
      }
    }
    private bool _MultiSelect;

    /// <summary>
    /// Флажки выбора в режиме <see cref="MultiSelect"/> 
    /// </summary>
    public bool[] Selections
    {
      get
      {
        if (_Selections == null)
        {
          if (_Items == null || (!MultiSelect))
            return null;
          _Selections = new bool[_Items.Length];
        }
        return _Selections;
      }
      set
      {
        if (!MultiSelect)
          throw ExceptionFactory.ObjectProperty(this, "MultiSelect", MultiSelect, null);
        if (_Items == null)
          throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");
        if (value != null)
        {
          if (value.Length != _Items.Length)
            throw ExceptionFactory.ArgWrongCollectionCount("value", value, _Items.Length);
        }
        _Selections = value;
      }
    }
    private bool[] _Selections;

    /// <summary>
    /// В режиме <see cref="MultiSelect"/> возвращает true, если в <see cref="Selections"/> установлены все флажки
    /// </summary>
    public bool AreAllSelected
    {
      get
      {
        if (_Selections == null)
          return false;
        for (int i = 0; i < _Selections.Length; i++)
        {
          if (!_Selections[i])
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// В режиме <see cref="MultiSelect"/> возвращает true, если в <see cref="Selections"/> сброшены все флажки
    /// </summary>
    public bool AreAllUnselected
    {
      get
      {
        if (_Selections == null)
          return true;
        for (int i = 0; i < _Selections.Length; i++)
        {
          if (_Selections[i])
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// Индексы выбранных строк в режиме <see cref="MultiSelect"/>.
    /// Если <see cref="MultiSelect"/>=false значение содержит один или ноль элементов.
    /// </summary>
    public int[] SelectedIndices
    {
      get
      {
        if (MultiSelect)
        {
          List<int> lst = new List<int>();
          for (int i = 0; i < Selections.Length; i++)
          {
            if (Selections[i])
              lst.Add(i);
          }
          return lst.ToArray();
        }
        else
        {
          if (SelectedIndex >= 0)
            return new int[1] { SelectedIndex };
          else
            return EmptyArray<Int32>.Empty;
        }
      }
      set
      {
        if (MultiSelect)
        {
          ArrayTools.FillArray<bool>(Selections, false);
          if (value != null)
          {
            for (int i = 0; i < value.Length; i++)
            {
              if (value[i] < 0 || value[i] >= Selections.Length)
                throw new ArgumentOutOfRangeException();
              Selections[value[i]] = true;
            }
          }
        }
        else
        {
          if (value == null)
            SelectedIndex = -1;
          else if (value.Length == 0)
            SelectedIndex = -1;
          else if (value.Length == 1)
          {
            if (value[0] < 0 || value[0] >= _Items.Length)
              throw new ArgumentOutOfRangeException();
            SelectedIndex = value[0];
          }
        }
      }
    }

    /// <summary>
    /// Задать выбранные элементы с помощью списка строк.
    /// Для строк <see cref="Items"/>, которые будут найдены в переданном аргументе, будет 
    /// установлена отметка. Для остальных строк отметка будет снята.
    /// Если в массиве <paramref name="selectedItems"/> есть строки, которых нет в списке <see cref="Items"/>,
    /// элемент пропускается без возникновения ошибки
    /// </summary>
    /// <param name="selectedItems">Значения, которые нужно выбрать</param>
    public void SetSelectedItems(string[] selectedItems)
    {
      if (!MultiSelect)
        throw ExceptionFactory.ObjectProperty(this, "MultiSelect", MultiSelect, new object[] { true });
      if (Items == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");

      ArrayTools.FillArray<bool>(Selections, false);

      if (selectedItems != null)
      {
        for (int i = 0; i < selectedItems.Length; i++)
        {
          int p = Array.IndexOf<String>(Items, selectedItems[i]);
          if (p >= 0)
            Selections[p] = true;
        }
      }
    }

    /// <summary>
    /// Получить список отмеченных строк из массива <see cref="Items"/>
    /// </summary>
    /// <returns></returns>
    public string[] GetSelectedItems()
    {
      if (Items == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");

      if (!MultiSelect)
      {
        if (SelectedIndex >= 0)
          return new string[1] { Items[SelectedIndex] };
        return EmptyArray<string>.Empty;
      }

      // Придется делать 2 прохода
      int i;
      int n = 0;
      for (i = 0; i < Selections.Length; i++)
      {
        if (Selections[i])
          n++;
      }
      string[] a = new string[n];
      n = 0;
      for (i = 0; i < Selections.Length; i++)
      {
        if (Selections[i])
        {
          a[n] = Items[i];
          n++;
        }
      }
      return a;
    }

    /// <summary>
    /// Установить отметки для всех позиций
    /// </summary>
    public void SelectAll()
    {
      if (!MultiSelect)
        throw ExceptionFactory.ObjectProperty(this, "MultiSelect", MultiSelect, new object[] { true });

      ArrayTools.FillArray<bool>(Selections, true);
    }


    /// <summary>
    /// Снять отметки для всех позиций
    /// </summary>
    public void UnselectAll()
    {
      if (!MultiSelect)
        throw ExceptionFactory.ObjectProperty(this, "MultiSelect", MultiSelect, new object[] { true });

      ArrayTools.FillArray<bool>(Selections, false);
    }

    #endregion

    #region Коды

    /// <summary>
    /// Полный список возможных кодов строк.
    /// По умолчанию - null - коды не используются.
    /// </summary>
    public string[] Codes
    {
      get { return _Codes; }
      set
      {
        if (value != null)
        {
          if (_Items == null)
            throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");
          if (value.Length != _Items.Length)
            throw ExceptionFactory.ArgWrongCollectionCount("value", value, _Items.Length);

          _CodeIndexer = new StringArrayIndexer(value, false);
        }
        else
          _CodeIndexer = null;
        _Codes = value;
      }
    }
    private string[] _Codes;

    private StringArrayIndexer _CodeIndexer;


    /// <summary>
    /// Текущий код при <see cref="MultiSelect"/>=false
    /// </summary>
    public string SelectedCode
    {
      get
      {
        if (_Codes == null)
          return String.Empty;
        if (SelectedIndex < 0)
          return String.Empty;
        return _Codes[SelectedIndex];
      }
      set
      {
        if (Codes == null)
          throw ExceptionFactory.ObjectPropertyNotSet(this, "Codes");
        if (String.IsNullOrEmpty(value))
          SelectedIndex = -1;
        else
          SelectedIndex = _CodeIndexer.IndexOf(value);
      }
    }

    /// <summary>
    /// Выбранные коды при <see cref="MultiSelect"/>=true
    /// </summary>
    public string[] SelectedCodes
    {
      get
      {
        if (_Codes == null || (!MultiSelect))
          return EmptyArray<string>.Empty;

        List<string> lst = new List<string>();
        for (int i = 0; i < Items.Length; i++)
        {
          if (Selections[i])
            lst.Add(Codes[i]);
        }

        return lst.ToArray();
      }
      set
      {
        if (Codes == null)
          throw ExceptionFactory.ObjectEventHandlerNotSet(this, "Codes");
        if (!MultiSelect)
          throw ExceptionFactory.ObjectEventHandlerNotSet(this, "MuliSelect");
        UnselectAll();
        if (value != null)
        {
          foreach (string code in value)
          {
            int p = _CodeIndexer.IndexOf(code);
            if (p >= 0)
              Selections[p] = true;
          }
        }
      }
    }

    #endregion

    #region Изображения

    /// <summary>
    /// Имя изображения (одного на все элементы). 
    /// Изображения извлекаются из списка <see cref="EFPApp.MainImages"/>.
    /// Может перекрываться с помощью массива <see cref="ImageKeys"/> для задания отдельных изображений.
    /// Также определяет значок формы.
    /// Можно использовать произвольное изображение <see cref="Image"/>, для значка диалога, но при этом в списке будут отображаться значки "Item" или заданные в <see cref="ImageKeys"/>, так как <see cref="ListView"/> может работать только с <see cref="ImageList"/>, но не отдельными изображениями.
    /// Свойства <see cref="Image"/> и <see cref="ImageKey"/> являются взаимоисключающими
    /// </summary>
    public string ImageKey
    {
      get { return _ImageKey ?? String.Empty; }
      set
      {
        _Image = null;
        _ImageKey = value;
      }
    }
    private string _ImageKey;

    /// <summary>
    /// Произвольное изображение, использумое для значка окна.
    /// Свойства <see cref="Image"/> и <see cref="ImageKey"/> являются взаимоисключающими
    /// </summary>
    public Image Image
    {
      get { return _Image; }
      set
      {
        _ImageKey = null;
        _Image = value;
      }
    }
    private Image _Image;

    /// <summary>
    /// Имена индивидуальных изображений для каждого элемента списка.
    /// Изображения извлекаются из списка <see cref="EFPApp.MainImages"/>.
    /// Свойство действительно и может устанавливаться только после установки свойства <see cref="Items"/>.
    /// Длина массива совпадает с <see cref="Items"/>.
    /// Для пустых строк массива используется основное изображение, задаваемое свойствами <see cref="Image"/> или <see cref="ImageKey"/>.
    /// </summary>
    public string[] ImageKeys
    {
      get
      {
        if (_ImageKeys == null)
        {
          if (_Items == null)
            return null;
          _ImageKeys = new string[_Items.Length];
        }
        return _ImageKeys;
      }
      set
      {
        if (value == null)
          _ImageKeys = null;
        else
        {
          if (Items == null)
            throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");
          if (value.Length != Items.Length)
            throw ExceptionFactory.ArgWrongCollectionCount("value", value, Items.Length);
          _ImageKeys = value;
        }
      }
    }
    private string[] _ImageKeys;

    bool IListSelectDialogInternal.HasImageKeys { get { return _ImageKeys != null; } }

    /// Индивидуальные изображения для каждого элемента списка.
    /// Свойство действительно и может устанавливаться только после установки свойства <see cref="Items"/>.
    /// Длина массива совпадает с <see cref="Items"/>.
    /// Для значений null в массиве используется основное изображение, задаваемое свойствами <see cref="Image"/> или <see cref="ImageKey"/>.
    public Image[] Images
    {
      get
      {
        if (_ImageKeys == null)
        {
          if (_Items == null)
            return null;
          _Images = new Image[_Items.Length];
        }
        return _Images;
      }
      set
      {
        if (value == null)
          _Images = null;
        else
        {
          if (Items == null)
            throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");
          if (value.Length != Items.Length)
            throw ExceptionFactory.ArgWrongCollectionCount("value", value, Items.Length);
          _Images = value;
        }
      }
    }
    private Image[] _Images;

    bool IListSelectDialogInternal.HasImages { get { return _Images != null; } }

    #endregion

    #region Показ блока диалога

    /// <summary>
    /// Запуск диалога
    /// </summary>
    /// <returns>OK, если пользователь сделал выбор</returns>
    public DialogResult ShowDialog()
    {
      if (Items == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");

      DialogResult res;
      using (ListSelectForm form = new ListSelectForm(this))
      {
        res = DoShowDialog(form);
      }
      return res;
    }

    /// <summary>
    /// Отдельный метод для гарантированного вызова TheForm.Dispose()
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    private DialogResult DoShowDialog(ListSelectForm form)
    {
      form.Text = Title;

      form.efpGrid.CurrentRowIndex = SelectedIndex;
      if (MultiSelect)
        form.CheckMarks.FromArray(Selections);

      // Значок формы
      if (Image == null)
        EFPApp.MainImages.Icons.InitForm(form, ImageKey, true);
      else
        WinFormsTools.InitIcon(form, Image);


      if (EFPApp.ShowDialog(form, false, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      SelectedIndex = form.efpGrid.CurrentRowIndex;

      if (MultiSelect)
        Selections = form.CheckMarks.ToArray();

      return DialogResult.OK;
    }


    #endregion
  }

  /// <summary>
  /// Шаг мастера для выбора одной или нескольких позиций из списка.
  /// Возможности аналогичны классу <see cref="ListSelectDialog"/>.
  /// Список позиций, начальный выбор и другие свойства могут задаваться после создания шага или
  /// в обработчике события <see cref="WizardStep.BeginStep"/> при <see cref="WizardBeginStepEventArgs.Forward"/>=true.
  /// Выбор, сделанный пользователем, доступен в событии <see cref="WizardStep.EndStep"/> при <see cref="WizardEndStepEventArgs.Forward"/>=true и позже.
  /// </summary>
  public class WizardStepWithListSelection : ExtWizardStep, IListSelectDialogInternal
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует свойства по умолчанию
    /// </summary>
    public WizardStepWithListSelection()
    {
      _Form = new ListSelectForm();
      _Form.OkCancelButtonPanel.Visible = false; // кнопки не нужны
      MainPanel.Controls.Add(_Form.MainPanel);

      _MultiSelect = false;
      _CanBeEmpty = false;
      _SelectedIndex = -1;
      _ClipboardMode = ListSelectDialogClipboardMode.None;
    }

    private ListSelectForm _Form;

    #endregion

    #region Свойства из ListSelectDialog

    #region Форма

    /// <summary>
    /// Заголовок над списком.
    /// По умолчанию - пустая строка - заголовок не выводится
    /// </summary>
    public string ListTitle
    {
      get { return _ListTitle ?? String.Empty; }
      set
      {
        CheckHasNotBeenInit();
        _ListTitle = value;
      }
    }
    private string _ListTitle;

    /// <summary>
    /// Заголовок таблицы при печати / экспорте списка в файл.
    /// По умолчанию совпадает с <see cref="ListTitle"/>.
    /// </summary>
    public string OutItemTitle
    {
      get
      {
        if (_OutItemTitle == null)
          return ListTitle;
        else
          return _OutItemTitle;
      }
      set
      {
        _OutItemTitle = value;
      }
    }
    private string _OutItemTitle;

    /// <summary>
    /// Можно ли использовать команды копирования и вставки из буфера обмена.
    /// По умолчанию - None - копирование недоступно.
    /// </summary>
    public ListSelectDialogClipboardMode ClipboardMode
    {
      get { return _ClipboardMode; }
      set
      {
        CheckHasNotBeenInit();
        _ClipboardMode = value;
      }
    }
    private ListSelectDialogClipboardMode _ClipboardMode;

    string IListSelectDialogInternal.ConfigSectionName { get { return String.Empty; } }

    #endregion

    #region Список элементов

    /// <summary>
    /// Список для выбора.
    /// Это свойство должно задаваться обязательно и в первую очередь.
    /// Установка свойства очищает остальные списочные свойства.
    /// </summary>
    public string[] Items
    {
      get
      {
        return _Items;
      }
      set
      {
        CheckHasNotBeenInit();
        _Items = value;
        _SubItems = null;
        _Images = null;
        _ImageKeys = null;
        _Selections = null;
        _Codes = null;
      }
    }
    private string[] _Items;

    /// <summary>
    /// Список строк для второго столбца.
    /// Если свойство равно null (по умолчанию), то второго столбца нет.
    /// Свойство может устанавливаться только после свойства Items
    /// </summary>
    public string[] SubItems
    {
      get { return _SubItems; }
      set
      {
        CheckHasNotBeenInit();
        if (value != null)
        {
          if (_Items == null)
            throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");
          if (value.Length != _Items.Length)
            throw ExceptionFactory.ArgWrongCollectionCount("value", value, _Items.Length);
        }
        _SubItems = value;
      }
    }
    private string[] _SubItems;

    #endregion

    #region Текущая позиция

    /// <summary>
    /// Текущая позиция при отключенном <see cref="MultiSelect"/> 
    /// </summary>
    public int SelectedIndex
    {
      get { return _SelectedIndex; }
      set { _SelectedIndex = value; }
    }
    private int _SelectedIndex;

    /// <summary>
    /// Установка и получение выбранной позиции как строки.
    /// Выполняет поиск в списке <see cref="Items"/>
    /// </summary>
    public string SelectedItem
    {
      get
      {
        if (Items == null || SelectedIndex < 0)
          return String.Empty;
        else
          return Items[SelectedIndex];
      }
      set
      {
        if (Items == null)
          throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");
        SelectedIndex = Array.IndexOf<string>(Items, value);
      }
    }

    /// <summary>
    /// True, если пользователь может нажимать "ОК", если нет выбранной позиции в списке (при <see cref="MultiSelect"/>=false)
    /// или не отмечено ни одного флажка (при <see cref="MultiSelect"/>=true).
    /// По умолчанию - false.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set
      {
        CheckHasNotBeenInit();
        _CanBeEmpty = value;
      }
    }
    private bool _CanBeEmpty;

    #endregion

    #region Выбор нескольких позиций

    /// <summary>
    /// True, если разрешено выбирать несколько позиций
    /// </summary>
    public bool MultiSelect
    {
      get
      {
        return _MultiSelect;
      }
      set
      {
        CheckHasNotBeenInit();
        if (value == _MultiSelect)
          return;
        _MultiSelect = value;
        _Selections = null;
      }
    }
    private bool _MultiSelect;

    /// <summary>
    /// Флажки выбора в режиме <see cref="MultiSelect"/> 
    /// </summary>
    public bool[] Selections
    {
      get
      {
        if (_Selections == null)
        {
          if (_Items == null || (!MultiSelect))
            return null;
          _Selections = new bool[_Items.Length];
        }
        return _Selections;
      }
      set
      {
        if (!MultiSelect)
          throw ExceptionFactory.ObjectProperty(this, "MultiSelect", MultiSelect, null);
        if (_Items == null)
          throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");
        if (value != null)
        {
          if (value.Length != _Items.Length)
            throw ExceptionFactory.ArgWrongCollectionCount("value", value, _Items.Length);
        }
        _Selections = value;
      }
    }
    private bool[] _Selections;

    /// <summary>
    /// В режиме <see cref="MultiSelect"/> возвращает true, если в <see cref="Selections"/> установлены все флажки
    /// </summary>
    public bool AreAllSelected
    {
      get
      {
        if (_Selections == null)
          return false;
        for (int i = 0; i < _Selections.Length; i++)
        {
          if (!_Selections[i])
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// В режиме <see cref="MultiSelect"/> возвращает true, если в <see cref="Selections"/> сброшены все флажки
    /// </summary>
    public bool AreAllUnselected
    {
      get
      {
        if (_Selections == null)
          return true;
        for (int i = 0; i < _Selections.Length; i++)
        {
          if (_Selections[i])
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// Индексы выбранных строк в режиме <see cref="MultiSelect"/>.
    /// Если <see cref="MultiSelect"/>=false значение содержит один или ноль элементов.
    /// </summary>
    public int[] SelectedIndices
    {
      get
      {
        if (MultiSelect)
        {
          List<int> lst = new List<int>();
          for (int i = 0; i < Selections.Length; i++)
          {
            if (Selections[i])
              lst.Add(i);
          }
          return lst.ToArray();
        }
        else
        {
          if (SelectedIndex >= 0)
            return new int[1] { SelectedIndex };
          else
            return EmptyArray<Int32>.Empty;
        }
      }
      set
      {
        if (MultiSelect)
        {
          ArrayTools.FillArray<bool>(Selections, false);
          if (value != null)
          {
            for (int i = 0; i < value.Length; i++)
            {
              if (value[i] < 0 || value[i] >= Selections.Length)
                throw new ArgumentOutOfRangeException();
              Selections[value[i]] = true;
            }
          }
        }
        else
        {
          if (value == null)
            SelectedIndex = -1;
          else if (value.Length == 0)
            SelectedIndex = -1;
          else if (value.Length == 1)
          {
            if (value[0] < 0 || value[0] >= _Items.Length)
              throw new ArgumentOutOfRangeException();
            SelectedIndex = value[0];
          }
        }
      }
    }

    /// <summary>
    /// Задать выбранные элементы с помощью списка строк.
    /// Для строк <see cref="Items"/>, которые будут найдены в переданном аргументе, будет 
    /// установлена отметка. Для остальных строк отметка будет снята.
    /// Если в массиве <paramref name="selectedItems"/> есть строки, которых нет в списке <see cref="Items"/>,
    /// элемент пропускается без возникновения ошибки
    /// </summary>
    /// <param name="selectedItems">Значения, которые нужно выбрать</param>
    public void SetSelectedItems(string[] selectedItems)
    {
      if (!MultiSelect)
        throw ExceptionFactory.ObjectProperty(this, "MultiSelect", MultiSelect, new object[] { true });
      if (Items == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");

      ArrayTools.FillArray<bool>(Selections, false);

      if (selectedItems != null)
      {
        for (int i = 0; i < selectedItems.Length; i++)
        {
          int p = Array.IndexOf<String>(Items, selectedItems[i]);
          if (p >= 0)
            Selections[p] = true;
        }
      }
    }

    /// <summary>
    /// Получить список отмеченных строк из массива <see cref="Items"/>
    /// </summary>
    /// <returns></returns>
    public string[] GetSelectedItems()
    {
      if (Items == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");

      if (!MultiSelect)
      {
        if (SelectedIndex >= 0)
          return new string[1] { Items[SelectedIndex] };
        return EmptyArray<string>.Empty;
      }

      // Придется делать 2 прохода
      int i;
      int n = 0;
      for (i = 0; i < Selections.Length; i++)
      {
        if (Selections[i])
          n++;
      }
      string[] a = new string[n];
      n = 0;
      for (i = 0; i < Selections.Length; i++)
      {
        if (Selections[i])
        {
          a[n] = Items[i];
          n++;
        }
      }
      return a;
    }

    /// <summary>
    /// Установить отметки для всех позиций
    /// </summary>
    public void SelectAll()
    {
      if (!MultiSelect)
        throw ExceptionFactory.ObjectProperty(this, "MultiSelect", MultiSelect, new object[] { true });

      ArrayTools.FillArray<bool>(Selections, true);
    }


    /// <summary>
    /// Снять отметки для всех позиций
    /// </summary>
    public void UnselectAll()
    {
      if (!MultiSelect)
        throw ExceptionFactory.ObjectProperty(this, "MultiSelect", MultiSelect, new object[] { true });

      ArrayTools.FillArray<bool>(Selections, false);
    }

    #endregion

    #region Коды

    /// <summary>
    /// Полный список возможных кодов строк.
    /// По умолчанию - null - коды не используются.
    /// </summary>
    public string[] Codes
    {
      get { return _Codes; }
      set
      {
        CheckHasNotBeenInit();
        if (value != null)
        {
          if (_Items == null)
            throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");
          if (value.Length != _Items.Length)
            throw ExceptionFactory.ArgWrongCollectionCount("value", value, _Items.Length);

          _CodeIndexer = new StringArrayIndexer(value, false);
        }
        else
          _CodeIndexer = null;
        _Codes = value;
      }
    }
    private string[] _Codes;

    private StringArrayIndexer _CodeIndexer;


    /// <summary>
    /// Текущий код при <see cref="MultiSelect"/>=false
    /// </summary>
    public string SelectedCode
    {
      get
      {
        if (_Codes == null)
          return String.Empty;
        if (SelectedIndex < 0)
          return String.Empty;
        return _Codes[SelectedIndex];
      }
      set
      {
        if (Codes == null)
          throw ExceptionFactory.ObjectPropertyNotSet(this, "Codes");
        if (String.IsNullOrEmpty(value))
          SelectedIndex = -1;
        else
          SelectedIndex = _CodeIndexer.IndexOf(value);
      }
    }

    /// <summary>
    /// Выбранные коды при <see cref="MultiSelect"/>=true
    /// </summary>
    public string[] SelectedCodes
    {
      get
      {
        if (_Codes == null || (!MultiSelect))
          return EmptyArray<string>.Empty;

        List<string> lst = new List<string>();
        for (int i = 0; i < Items.Length; i++)
        {
          if (Selections[i])
            lst.Add(Codes[i]);
        }

        return lst.ToArray();
      }
      set
      {
        if (Codes == null)
          throw ExceptionFactory.ObjectEventHandlerNotSet(this, "Codes");
        if (!MultiSelect)
          throw ExceptionFactory.ObjectEventHandlerNotSet(this, "MuliSelect");
        UnselectAll();
        if (value != null)
        {
          foreach (string code in value)
          {
            int p = _CodeIndexer.IndexOf(code);
            if (p >= 0)
              Selections[p] = true;
          }
        }
      }
    }

    #endregion

    #region Изображения

    /// <summary>
    /// Имя изображения (одного на все элементы). 
    /// Изображения извлекаются из списка <see cref="EFPApp.MainImages"/>.
    /// Может перекрываться с помощью массива <see cref="ImageKeys"/> для задания отдельных изображений.
    /// Также определяет значок формы.
    /// Можно использовать произвольное изображение <see cref="Image"/>, для значка диалога, но при этом в списке будут отображаться значки "Item" или заданные в <see cref="ImageKeys"/>, так как <see cref="ListView"/> может работать только с <see cref="ImageList"/>, но не отдельными изображениями.
    /// Свойства <see cref="Image"/> и <see cref="ImageKey"/> являются взаимоисключающими
    /// </summary>
    public string ImageKey
    {
      get { return _ImageKey ?? String.Empty; }
      set
      {
//        CheckHasNotBeenInit();
        _Image = null;
        _ImageKey = value;
      }
    }
    private string _ImageKey;

    /// <summary>
    /// Произвольное изображение, использумое для значка окна.
    /// Свойства <see cref="Image"/> и <see cref="ImageKey"/> являются взаимоисключающими
    /// </summary>
    public Image Image
    {
      get { return _Image; }
      set
      {
//        CheckHasNotBeenInit();
        _ImageKey = null;
        _Image = value;
      }
    }
    private Image _Image;

    /// <summary>
    /// Имена индивидуальных изображений для каждого элемента списка.
    /// Изображения извлекаются из списка <see cref="EFPApp.MainImages"/>.
    /// Свойство действительно и может устанавливаться только после установки свойства <see cref="Items"/>.
    /// Длина массива совпадает с <see cref="Items"/>.
    /// Для пустых строк массива используется основное изображение, задаваемое свойствами <see cref="Image"/> или <see cref="ImageKey"/>.
    /// </summary>
    public string[] ImageKeys
    {
      get
      {
        if (_ImageKeys == null)
        {
          if (_Items == null)
            return null;
          _ImageKeys = new string[_Items.Length];
        }
        return _ImageKeys;
      }
      set
      {
        if (value == null)
          _ImageKeys = null;
        else
        {
          if (Items == null)
            throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");
          if (value.Length != Items.Length)
            throw ExceptionFactory.ArgWrongCollectionCount("value", value, Items.Length);
          _ImageKeys = value;
        }
      }
    }
    private string[] _ImageKeys;

    bool IListSelectDialogInternal.HasImageKeys { get { return _ImageKeys != null; } }

    /// Индивидуальные изображения для каждого элемента списка.
    /// Свойство действительно и может устанавливаться только после установки свойства <see cref="Items"/>.
    /// Длина массива совпадает с <see cref="Items"/>.
    /// Для значений null в массиве используется основное изображение, задаваемое свойствами <see cref="Image"/> или <see cref="ImageKey"/>.
    public Image[] Images
    {
      get
      {
        if (_ImageKeys == null)
        {
          if (_Items == null)
            return null;
          _Images = new Image[_Items.Length];
        }
        return _Images;
      }
      set
      {
        if (value == null)
          _Images = null;
        else
        {
          if (Items == null)
            throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");
          if (value.Length != Items.Length)
            throw ExceptionFactory.ArgWrongCollectionCount("value", value, Items.Length);
          _Images = value;
        }
      }
    }
    private Image[] _Images;

    bool IListSelectDialogInternal.HasImages { get { return _Images != null; } }

    #endregion


    #endregion

    #region OnBeginStep() / OnEndStep()

    /// <summary>
    /// Однократная инициализация
    /// </summary>
    /// <param name="action"></param>
    protected internal override void OnBeginStep(WizardAction action)
    {
      base.OnBeginStep(action);
      if ((!_HasBeenInit) && Wizard.IsForwardAction(action))
      {
        _HasBeenInit = true;
        _Form.Init(BaseProvider, this);

        if (MultiSelect)
          _Form.CheckMarks.FromArray(Selections);
        _Form.efpGrid.CurrentRowIndex = SelectedIndex;
      }
    }

    private bool _HasBeenInit;

    private void CheckHasNotBeenInit()
    {
      if (_HasBeenInit)
        throw ExceptionFactory.CannotAddItemAgain(this);
    }

    /// <summary>
    /// Проверяет свойство <see cref="CanBeEmpty"/>. Заполняет свойства <see cref="SelectedIndex"/>, <see cref="Selections"/>,
    /// затем вызывает пользовательский обработчик
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    protected internal override bool OnEndStep(WizardAction action)
    {
      if (Wizard.IsForwardAction(action))
      {
        if (!ValidateStep())
          return false;

        SelectedIndex = _Form.efpGrid.CurrentRowIndex;

        if (MultiSelect)
          Selections = _Form.CheckMarks.ToArray();
      }
      return base.OnEndStep(action);
    }

    private bool ValidateStep()
    {
      if (CanBeEmpty)
        return true;

      if (MultiSelect)
      {
        if (_Form.CheckMarks.AreAllUnselected)
        {
          EFPApp.ShowTempMessage(Res.ListSelectDialog_Err_NoneChecked);
          return false;
        }
      }
      else
      {
        if (_Form.efpGrid.CurrentGridRow == null)
        {
          EFPApp.ShowTempMessage(Res.ListSelectDialog_Err_NoSelection);
          return false;
        }
      }
      return true;
    }

    #endregion
  }
}
