﻿// Part of FreeLibSet.
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

    public ListSelectForm(int itemCount, bool multiSelect, bool canBeEmpty, ListSelectDialogClipboardMode clipboardMode, bool hasSubItems)
    {
      InitializeComponent();

      _ClipboardMode = clipboardMode;

      TheLV.CheckBoxes = multiSelect;

      if (hasSubItems)
        _SubColumn = TheLV.Columns.Add(Res.ListSelectDialog_ColTitle_Sub);

      _CanBeEmpty = canBeEmpty;

      efpForm = new EFPFormProvider(this);
      efpForm.FormChecks.Add(new UIValidatingEventHandler(ValidateForm));

      btnCheckAll.Image = EFPApp.MainImages.Images["CheckListAll"];
      btnCheckAll.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpCheckAll = new EFPButton(efpForm, btnCheckAll);
      efpCheckAll.Click += efpCheckAll_Click;

      btnUnCheckAll.Image = EFPApp.MainImages.Images["CheckListNone"];
      btnUnCheckAll.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpUnCheckAll = new EFPButton(efpForm, btnUnCheckAll);
      efpUnCheckAll.Click += efpUnCheckAll_Click;

      btnCheckAll.Visible = multiSelect;
      btnUnCheckAll.Visible = multiSelect;

      // 25.10.2019 - блокируем бесполезные кнопки
      btnCheckAll.Enabled = itemCount > 0;
      btnUnCheckAll.Enabled = itemCount > 0;
      btnOk.Enabled = canBeEmpty || itemCount > 0;

      if (clipboardMode != ListSelectDialogClipboardMode.None)
      {
        btnCopy.Image = EFPApp.MainImages.Images["Copy"];
        btnCopy.ImageAlign = ContentAlignment.MiddleCenter;
        EFPButton efpCopy = new EFPButton(efpForm, btnCopy);
        efpCopy.DisplayName = EFPCommandItem.RemoveMnemonic(Res.Cmd_Menu_Edit_Copy);
        efpCopy.ToolTipText = Res.ListSelectDialog_ToolTip_Copy;
        efpCopy.Click += new EventHandler(efpCopy_Click);

        btnPaste.Image = EFPApp.MainImages.Images["Paste"];
        btnPaste.ImageAlign = ContentAlignment.MiddleCenter;
        EFPButton efpPaste = new EFPButton(efpForm, btnPaste);
        efpPaste.DisplayName = EFPCommandItem.RemoveMnemonic(Res.Cmd_Menu_Edit_Paste);
        efpPaste.ToolTipText = Res.ListSelectDialog_ToolTip_Paste;
        efpPaste.Click += new EventHandler(efpPaste_Click);
      }
      else
      {
        btnCopy.Visible = false;
        btnPaste.Visible = false;
      }

      EFPButtonWithMenu efpMore = new EFPButtonWithMenu(efpForm, btnMore);
      efpMore.DisplayName = Res.Btn_Text_More;
      efpMore.ToolTipText = Res.Btn_ToolTip_More;
      //efpMore.Visible = false;

      EFPCommandItem ciCopyAll = new EFPCommandItem("Edit", "CopyAllText");
      ciCopyAll.MenuText = Res.ListSelectDialog_Menu_CopyAllText;
      ciCopyAll.Click += new EventHandler(ciCopyAll_Click);
      ciCopyAll.Enabled = itemCount > 0;
      efpMore.CommandItems.Add(ciCopyAll);
    }

    #endregion

    #region Поля

    public EFPFormProvider efpForm;

    /// <summary>
    /// Если есть подтемы, то они отображаются здесь
    /// </summary>
    private ColumnHeader _SubColumn;

    private bool _CanBeEmpty;

    private ListSelectDialogClipboardMode _ClipboardMode;

    #endregion

    #region Изменение размеров формы

    private bool _InsideResize = false;
    public void TheLV_Resize(object sender, EventArgs args)
    {
      if (_InsideResize)
        return;

      _InsideResize = true;
      try
      {
        if (_SubColumn == null)
          TheColumn.Width = -2;
        else
        {
          TheColumn.Width = -1;
          _SubColumn.Width = -2;
        }
      }
      finally
      {
        _InsideResize = false;
      }
    }

    #endregion

    #region Проверка формы

    private void ValidateForm(object sender, UIValidatingEventArgs args)
    {
      if (args.ValidateState == UIValidateState.Error)
        return;
      if (_CanBeEmpty)
        return;

      if (TheLV.CheckBoxes)
      {
        if (TheLV.CheckedItems.Count == 0)
        {
          // Если не одной темы не выбрано, то можно использовать текущую тему
          // 18.03.2016
          // Так не работает
          /*
          if (TheLV.SelectedItems.Count > 0)
          {
            TheLV.Items[TheLV.SelectedItems[0].Index].Selected = true;
            return;
          }
           * */
          args.SetError(Res.ListSelectDialog_Err_NoneChecked);
          return;
        }
      }
      else
      {
        if (TheLV.SelectedItems.Count == 0)
        {
          args.SetError(Res.ListSelectDialog_Err_NoSelection);
        }
      }
    }

    #endregion

    #region Кнопки установки / снятия отметов

    private void efpCheckAll_Click(object sender, EventArgs args)
    {
      TheLV.BeginUpdate();
      try
      {
        for (int i = 0; i < TheLV.Items.Count; i++)
          TheLV.Items[i].Checked = true;
      }
      finally
      {
        TheLV.EndUpdate();
      }
    }

    private void efpUnCheckAll_Click(object sender, EventArgs args)
    {
      TheLV.BeginUpdate();
      try
      {
        for (int i = 0; i < TheLV.Items.Count; i++)
          TheLV.Items[i].Checked = false;
      }
      finally
      {
        TheLV.EndUpdate();
      }
    }

    #endregion

    #region Двойной щелчок для нажатия кнопки ОК

    private void TheLV_DoubleClick(object sender, EventArgs args)
    {
      if (TheLV.CheckBoxes)
        return;
      btnOk.PerformClick();
    }

    #endregion

    #region Команды буфера обмена

    void efpCopy_Click(object sender, EventArgs args)
    {
      switch (_ClipboardMode)
      {
        case ListSelectDialogClipboardMode.CommaCodes:
          if (TheLV.CheckBoxes)
          {
            StringBuilder sb = new StringBuilder();
            foreach (ListViewItem li in TheLV.CheckedItems)
            {
              if (sb.Length > 0)
                sb.Append(", ");
              sb.Append(li.Text);
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
            if (TheLV.SelectedItems.Count == 1)
              new EFPClipboard().SetText(TheLV.SelectedItems[0].Text);
            else
              EFPApp.ShowTempMessage(Res.ListSelectDialog_Err_NoSelection);
          }
          break;

        default:
          throw new BugException("ClipboardMode=" + _ClipboardMode.ToString());
      }
    }

    /// <summary>
    /// Ключ - текст элемента, значение - ссылка на элемент.
    /// Используется для вставки ссылок из буфера обмена.
    /// Можно было бы использовать метод ListView.ListViewItemCollection.Find(), но свойство Name является регистронечувствительным.
    /// </summary>
    private TypedStringDictionary<ListViewItem> _ItemDict;

    void efpPaste_Click(object sender, EventArgs args)
    {
      EFPButton efpPaste = (EFPButton)sender;

      #region Создаем словарь элементов

      if (_ItemDict == null)
      {
        _ItemDict = new TypedStringDictionary<ListViewItem>(TheLV.Items.Count, false);
        foreach (ListViewItem li in TheLV.Items)
          _ItemDict[li.Text] = li; // вдруг есть одинаковые строки
      }

      #endregion

      switch (_ClipboardMode)
      {
        case ListSelectDialogClipboardMode.CommaCodes:
          EFPClipboard clp = new EFPClipboard();
          clp.ErrorIfEmpty = true;
          string s = clp.GetText();
          if (String.IsNullOrEmpty(s))
            return;
          string[] a = s.Split(',');
          if (TheLV.CheckBoxes)
          {
            List<ListViewItem> lst1 = new List<ListViewItem>();
            List<ListViewItem> lst2 = new List<ListViewItem>();
            for (int i = 0; i < a.Length; i++)
            {
              s = a[i].Trim();
              ListViewItem li;
              if (_ItemDict.TryGetValue(s, out li))
              {
                if (li.Checked)
                  lst1.Add(li);
                else
                  lst2.Add(li);
              }
              else
              {
                EFPApp.ShowTempMessage(String.Format(Res.ListSelectDialog_Err_ItemNotFound, s));
                return;
              }
            }
            // lst содержит список элементов, которые нужно отметить
            if (lst2.Count == 0 && lst1.Count == TheLV.CheckedItems.Count)
            {
              EFPApp.ShowTempMessage(Res.ListSelectDialog_Err_NoChanges);
              return;
            }
            if (TheLV.CheckedItems.Count > 0)
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
                  lst2[i].Checked = true;
                return;
              }
            }

            // Режим замены выделения
            ListViewItem[] a0 = new ListViewItem[TheLV.CheckedItems.Count];
            TheLV.CheckedItems.CopyTo(a0, 0);
            for (int i = 0; i < a0.Length; i++)
              a0[i].Checked = false;
            for (int i = 0; i < lst1.Count; i++)
              lst1[i].Checked = true;
            for (int i = 0; i < lst2.Count; i++)
              lst2[i].Checked = true;
          }
          else // ! CheckBoxes
          {
            if (a.Length > 1)
            {
              EFPApp.ShowTempMessage(Res.ListSelectDialog_Err_PasteTextWithCommas);
              return;
            }
            s = a[0].Trim();
            ListViewItem li;
            if (_ItemDict.TryGetValue(s, out li))
              li.Selected = true;
            else
              EFPApp.ShowTempMessage(String.Format(Res.ListSelectDialog_Err_ItemNotFound, s));
          }
          break;

        default:
          throw new BugException("ClipboardMode=" + _ClipboardMode.ToString());
      }
    }

    void ciCopyAll_Click(object sender, EventArgs args)
    {
      EFPCommandItem ciCopyAll = (EFPCommandItem)sender;

      EFPApp.BeginWait(ciCopyAll.MenuText, ciCopyAll.ImageKey);
      try
      {
        string[,] a = new string[TheLV.Items.Count, _SubColumn == null ? 1 : 2];
        for (int i = 0; i < TheLV.Items.Count; i++)
        {
          a[i, 0] = TheLV.Items[i].Text;
          if (_SubColumn != null)
            a[i, 1] = TheLV.Items[i].SubItems[1].Text;
        }

        DataObject dobj = new DataObject();

        WinFormsTools.SetTextMatrix(dobj, a);

        byte[] buffer = CreateHtmlFormat(a);
        //System.IO.File.WriteAllBytes(@"d:\temp\table.html", Buffer);
        dobj.SetData(DataFormats.Html, false, new MemoryStream(buffer));

        new EFPClipboard().SetDataObject(dobj, true);
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    private byte[] CreateHtmlFormat(string[,] a)
    {
      const int PosWrStartHtml = 23;
      const int PosWrEndHtml = 43;
      const int PosWrStartFragment = 70;
      const int PosWrEndFragment = 93;


      byte[] buffer;
      using (MemoryStream strm = new MemoryStream())
      {
        using (StreamWriter wrt = new StreamWriter(strm, Encoding.UTF8))
        {
          // Убираем сигнатуру utf-8: EF BB BF
          wrt.Write("");
          wrt.Flush();
          strm.Flush();
          strm.SetLength(0);

          int PosOff = 0;

          wrt.WriteLine("Version:1.0");
          wrt.WriteLine("StartHTML:0000000000");
          wrt.WriteLine("EndHTML:0000000000");
          wrt.WriteLine("StartFragment:0000000000");
          wrt.WriteLine("EndFragment:0000000000");
          wrt.WriteLine();

          int OffStartHtml = GetHtmlOff(wrt, strm);
          wrt.WriteLine("<HTML>");
          wrt.WriteLine("<HEAD>");
          string CharSetText = wrt.Encoding.WebName;
          wrt.WriteLine("<META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html; charset=" + CharSetText + "\">");
          wrt.WriteLine("</HEAD>");

          wrt.WriteLine("<BODY>");
          wrt.WriteLine("<TABLE BORDER=1 CellPadding=4 CellSpacing=2 COLS=" + a.GetLength(1).ToString() + ">");

          wrt.WriteLine("<!--StartFragment-->");
          int OffStartFragment = GetHtmlOff(wrt, strm);

          //wrt.WriteLine("<col width=" + Column.Width.ToString() + ">");
          wrt.WriteLine();
          for (int i = 0; i < a.GetLength(0); i++)
          {
            wrt.WriteLine("<TR>");
            for (int j = 0; j < a.GetLength(1); j++)
            {
              string txt = MakeHtmlSpc(a[i, j]);
              wrt.WriteLine("<TD>" + txt + "</TD>");
            }
            wrt.WriteLine("</TR>");
          }

          int OffEndFragment = GetHtmlOff(wrt, strm);
          wrt.WriteLine("<!--EndFragment-->");

          wrt.WriteLine("</TABLE>");
          wrt.WriteLine("</BODY>");
          wrt.WriteLine("</HTML>");
          int OffEndHtml = GetHtmlOff(wrt, strm) - 2; // CRLF

          strm.Flush();

          WriteHtmlOff(strm, PosWrStartHtml + PosOff, OffStartHtml - PosOff);
          WriteHtmlOff(strm, PosWrEndHtml + PosOff, OffEndHtml - PosOff);
          WriteHtmlOff(strm, PosWrStartFragment + PosOff, OffStartFragment - PosOff);
          WriteHtmlOff(strm, PosWrEndFragment + PosOff, OffEndFragment - PosOff);

          buffer = strm.ToArray();
        }
      }
      return buffer;
    }


    /// <summary>
    /// Замена пробелов. 
    /// Если в строке более одного пробела подряд, заменяем второй пробел на
    /// код 160 (неразрывный пробел)
    /// Также заменяем специальные символы "больше", "меньше" и "амперсанд"
    /// </summary>
    /// <param name="txt"></param>
    /// <returns></returns>
    internal static string MakeHtmlSpc(string txt)
    {
      if (String.IsNullOrEmpty(txt))
        return txt;
      StringBuilder sb = new StringBuilder(txt);
      // Убираем гадкие символы (заменяем их на точки)
      for (int i = 0; i < sb.Length; i++)
      {
        if (sb[i] < ' ' && sb[i] != '\r' && sb[i] != '\n')
          sb[i] = '.';
      }

      // Замена второго и далее пробелов на неразрывный пробел
      // ??? sb.Replace("  ", ???);
      //txt:=STRTRAN(txt, '  ', CHR(32)+CHR(160))

      // Заменяем плохие символы на комбинации
      sb.Replace("&", "&amp;");
      sb.Replace("<", "&lt;");
      sb.Replace(">", "&gt;");

      return sb.ToString();
    }

    /// <summary>
    /// Запись значения смещения в указанную позицию файла
    /// Позиция записывается в виде десятичного числа в формате 0000000000 (10 разрядов)
    /// </summary>
    /// <param name="strm"></param>
    /// <param name="posWr">Позиция начала заглушки 0000000000</param>
    /// <param name="value">Записываемое значение</param>
    private static void WriteHtmlOff(Stream strm, int posWr, int value)
    {
      if (!strm.CanSeek)
        throw ExceptionFactory.ArgProperty("strm", strm, "CanSeek", strm.CanSeek, new object[] { true });
      string text = value.ToString("d10");
      strm.Seek(posWr, SeekOrigin.Begin);
      for (int i = 0; i < text.Length; i++)
      {
        byte b = (byte)(text[i]);
        strm.WriteByte(b);
      }
    }

    private static int GetHtmlOff(TextWriter wrt, Stream strm)
    {
      wrt.Flush();
      strm.Flush();
      strm.Seek(0, SeekOrigin.End);
      return (int)(strm.Position);
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

  /// <summary>
  /// Диалог выбора одной или нескольких позиций из списка
  /// </summary>
  public class ListSelectDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает объект диалога
    /// </summary>
    public ListSelectDialog()
    {
      MultiSelect = false;
      _CanBeEmpty = false;
      _SelectedIndex = -1;
      _DialogPosition = new EFPDialogPosition();
      _ClipboardMode = ListSelectDialogClipboardMode.None;
      _ConfigSectionName = String.Empty;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список для выбора
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
        RecreateSelection();
      }
    }
    private string[] _Items;

    /// <summary>
    /// Список строк для второго столбца.
    /// Если свойство равно null (по умолчанию), то второго столбца нет
    /// </summary>
    public string[] SubItems
    {
      get
      {
        return _SubItems;
      }
      set
      {
        _SubItems = value;
      }
    }
    private string[] _SubItems;



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
        RecreateSelection();
      }
    }
    private bool _MultiSelect;

    /// <summary>
    /// Флажки выбора в режиме <see cref="MultiSelect"/> 
    /// </summary>
    public bool[] Selections
    {
      get { return _Selections; }
      set
      {
        if (!MultiSelect)
          throw ExceptionFactory.ObjectProperty(this, "MultiSelect", MultiSelect, null);
        value.CopyTo(_Selections, 0);
      }
    }
    private bool[] _Selections;

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

    private void RecreateSelection()
    {
      _Selections = null;
      if (_Items == null || (!_MultiSelect))
        return;
      _Selections = new bool[_Items.Length];
    }


    /// <summary>
    /// True, если пользователь может нажимать "ОК", если нет выбранной позиции в списке (при <see cref="MultiSelect"/>=false)
    /// или не отмечено ни одного флажка (при <see cref="MultiSelect"/>=true).
    /// По умолчанию - false.
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

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
      get { return _ImageKey??String.Empty; }
      set
      {
        _Image = null;
        _ImageKey = value;
      }
    }
    private string _ImageKey;

    /// <summary>
    /// Произвольное изображение, использумое для значка окна.
    /// Оно не будет использоваться в списке. 
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
    /// Для пустых строк массива используется изображение, задаваемое свойством <see cref="ImageKey"/>.
    /// Нельзя использовать произвольные пользовательские изображения <see cref="Image"/>, так как <see cref="ListView"/> может работать только с <see cref="ImageList"/>, но не отдельными изображениями.
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
          _ImageKey = null;
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
      get { return _ListTitle??String.Empty; }
      set { _ListTitle = value; }
    }
    private string _ListTitle;

    /// <summary>
    /// Контекст справки, вызываемой по нажатию клавиши F1
    /// </summary>
    public string HelpContext { get { return _HelpContext; } set { _HelpContext = value; } }
    private string _HelpContext;

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
          return false;
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
          for (int i = 0; i < _Selections.Length; i++)
          {
            if (_Selections[i])
              lst.Add(i);
          }
          return lst.ToArray();
        }
        else
        {
          if (SelectedIndex >= 0)
            return new int[1] { SelectedIndex };
          else
            return DataTools.EmptyInts;
        }
      }
      set
      {
        if (MultiSelect)
        {
          DataTools.FillArray<bool>(_Selections, false);
          if (value != null)
          {
            for (int i = 0; i < value.Length; i++)
            {
              if (value[i] < 0 || value[i] >= _Selections.Length)
                throw new ArgumentOutOfRangeException();
              _Selections[value[i]] = true;
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

    #region Методы

    /// <summary>
    /// Запуск диалога
    /// </summary>
    /// <returns>OK, если пользователь сделал выбор</returns>
    public DialogResult ShowDialog()
    {
      if (Items == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");
      if (SubItems != null)
      {
        if (SubItems.Length != Items.Length)
          throw ExceptionFactory.ObjectPropertyCount(this, "SubItems", SubItems, Items.Length);
      }

      ListSelectForm frm = new ListSelectForm(Items.Length, MultiSelect, CanBeEmpty, ClipboardMode, SubItems != null);
      DialogResult res;
      try
      {
        res = DoShowDialog(frm);
      }
      finally
      {
        frm.Dispose();
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
      form.TheGroupBox.Text = ListTitle;
      form.efpForm.ConfigSectionName = ConfigSectionName;

      if ((String.IsNullOrEmpty(ImageKey) && ImageKeys==null) || (!EFPApp.ShowListImages))
        form.TheLV.SmallImageList = null;
      else
        form.TheLV.SmallImageList = EFPApp.MainImages.ImageList;

      for (int i = 0; i < Items.Length; i++)
      {
        string thisImageKey;
        if (EFPApp.ShowListImages)
        {
          if (Image == null)
            thisImageKey = ImageKey;
          else
            thisImageKey = "Item";
          if (_ImageKeys != null)
          {
            if (!String.IsNullOrEmpty(_ImageKeys[i]))
              thisImageKey = _ImageKeys[i];
          }
        }
        else
          thisImageKey = String.Empty;
        ListViewItem li = form.TheLV.Items.Add(Items[i], thisImageKey);
        if (SubItems != null)
          li.SubItems.Add(SubItems[i]);
        if (MultiSelect)
          li.Checked = Selections[i];
      }

      // Значок формы
      if (Image == null)
        EFPApp.MainImages.Icons.InitForm(form, ImageKey, true);
      else
        WinFormsTools.InitIcon(form, Image);

      #region Активация элемента списка

      // Все очень мерзко работает, пока не создан дескрипор HWND списка
      // Чтение свойства Control.Handler создает дескриптор. А метод CreateControl()
      // почему-то это не делает
      int dummy = (int)form.TheLV.Handle;

      if (SelectedIndex >= 0)
      {
        form.TheLV.Items[SelectedIndex].Focused = true;
        form.TheLV.Items[SelectedIndex].Selected = true;
        form.TheLV.EnsureVisible(SelectedIndex);
      }
      else if (MultiSelect)
      {
        // 06.12.2018
        // Делаем видимым первый флажок, если есть отмеченные
        for (int i = 0; i < Selections.Length; i++)
        {
          if (Selections[i])
          {
            form.TheLV.Items[i].Focused = true;
            form.TheLV.Items[i].Selected = true;
            form.TheLV.EnsureVisible(i);
            break;
          }
        }
      }

      #endregion

      // 09.02.2020
      // Подбор размеров по ширине списка
      int w = 0;
      for (int i = 0; i < form.TheLV.Columns.Count; i++)
      {
        form.TheLV.Columns[i].Width = -1;
        w += form.TheLV.Columns[i].Width;
      }
      w = Math.Min(w,  SystemInformation.VirtualScreen.Width); // 14.05.2021
      int dw = w +
        4 * SystemInformation.BorderSize.Width +
        SystemInformation.VerticalScrollBarWidth -
        form.TheLV.Width;
      if (dw > 0)
        form.Width += dw;

      // 14.05.2021
      // Подбор размеров по ширине списка
      if (Items.Length > 0)
      {
        int h = form.TheLV.GetItemRect(0).Height * Items.Length;
        int dh = h +
          4 * SystemInformation.BorderSize.Width +
          SystemInformation.HorizontalScrollBarHeight /* 14.05.2021 */ -
        form.TheLV.Height;

        if (dh > 0)
          form.Height += dh;
        h = Math.Min(h, SystemInformation.VirtualScreen.Height);
      }

      form.TheLV_Resize(null, null);

      if (!String.IsNullOrEmpty(HelpContext))
        form.efpForm.HelpContext = HelpContext;


      if (EFPApp.ShowDialog(form, false, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      if (form.TheLV.SelectedItems.Count > 0)
        SelectedIndex = form.TheLV.SelectedItems[0].Index;
      else
        SelectedIndex = -1;

      if (MultiSelect)
      {
        for (int i = 0; i < Items.Length; i++)
          Selections[i] = form.TheLV.Items[i].Checked;
      }

      return DialogResult.OK;
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
        throw ExceptionFactory.ObjectProperty(this, "MultiSelect", MultiSelect, new object[] { true});
      if (Items == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "Items");

      Array.Clear(_Selections, 0, _Selections.Length);

      if (selectedItems != null)
      {
        for (int i = 0; i < selectedItems.Length; i++)
        {
          int p = Array.IndexOf<String>(_Items, selectedItems[i]);
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
        return DataTools.EmptyStrings;
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

      for (int i = 0; i < Selections.Length; i++)
        Selections[i] = true;
    }


    /// <summary>
    /// Снять отметки для всех позиций
    /// </summary>
    public void UnselectAll()
    {
      if (!MultiSelect)
        throw ExceptionFactory.ObjectProperty(this, "MultiSelect", MultiSelect, new object[] { true });

      for (int i = 0; i < Selections.Length; i++)
        Selections[i] = false;
    }

    #endregion
  }


  /// <summary>
  /// Диалог выбора нескольких позиций из списка с возможностью задания порядка строк
  /// </summary>
  public class CodesListSelectDialog
  {
    #region Конструктор

    /// <summary>
    /// Инициализация диалога значениями по умолчанию
    /// </summary>
    public CodesListSelectDialog()
    {
      _CanBeEmpty = false;
      _DialogPosition = new EFPDialogPosition();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Полный список возможных кодов строк
    /// </summary>
    public string[] Codes { get { return _Codes; } set { _Codes = value; } }
    private string[] _Codes;

    /// <summary>
    /// Названия тем, соответствующих кодам <see cref="Codes"/> 
    /// </summary>
    public string[] Items { get { return _Items; } set { _Items = value; } }
    private string[] _Items;

    /// <summary>
    /// Выбранные коды в требуемом порядке
    /// </summary>
    public string[] SelectedCodes { get { return _SelectedCodes; } set { _SelectedCodes = value; } }
    private string[] _SelectedCodes;

    /// <summary>
    /// Необязательный список строк для второго столбца
    /// </summary>
    public string[] SubItems { get { return _SubItems; } set { _SubItems = value; } }
    private string[] _SubItems;

    /// <summary>
    /// True, если пользователь может нажимать "ОК", если нет выбранной позиции в списке
    /// По умолчанию - false.
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    /// <summary>
    /// Имя изображения (одного на все элементы). Может перекрываться с помощью
    /// <see cref="ImageKeys"/>.
    /// Также определяет значок формы
    /// </summary>
    public string ImageKey
    {
      get { return _ImageKey; }
      set { _ImageKey = value; }
    }
    private string _ImageKey;

    /// <summary>
    /// Индивидуальные изображения для тем
    /// </summary>
    public string[] ImageKeys { get { return _ImageKeys; } set { _ImageKeys = value; } }
    private string[] _ImageKeys;


    /// <summary>
    /// Заголовок формы.
    /// Если свойство не установлено в явном виде, используется заголовок по умолчанию.
    /// </summary>
    public string Title
    {
      get
      {
        if (_Title == null)
          return Res.ListSelectDialog_Msg_TitleMultiSelect;
        else
          return _Title;
      }
      set
      {
        _Title = value;
      }
    }
    private string _Title;

    /// <summary>
    /// Заголовок над списком.
    /// По умолчанию - пустая строка - нет заголовка
    /// </summary>
    public string ListTitle
    {
      get { return _ListTitle??String.Empty; }
      set { _ListTitle = value; }
    }
    private string _ListTitle;

    /// <summary>
    /// Контекст справки, вызываемой по нажатию клавиши F1
    /// </summary>
    public string HelpContext { get { return _HelpContext; } set { _HelpContext = value; } }
    private string _HelpContext;

    /// <summary>
    /// Позиция блока диалога на экране.
    /// По умолчанию блок диалога центрируется относительно <see cref="EFPApp.DefaultScreen"/>.
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

    #endregion

    #region Методы

    /// <summary>
    /// Запуск диалога
    /// </summary>
    /// <returns>OK, если пользователь сделал выбор</returns>
    public DialogResult ShowDialog()
    {
      if (Codes == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "Codes");
      if (Items == null)
        Items = Codes;
      if (Items.Length != Codes.Length)
        throw ExceptionFactory.ObjectPropertyCount(this, "Codes", Codes, Items.Length);
      if (SubItems != null)
      {
        if (SubItems.Length != Codes.Length)
          throw ExceptionFactory.ObjectPropertyCount(this, "SubItems", SubItems, Items.Length);
      }
      if (ImageKeys != null)
      {
        if (ImageKeys.Length != Codes.Length)
          throw ExceptionFactory.ObjectPropertyCount(this, "ImageKeys", SubItems, Items.Length);
      }

      ListSelectForm frm = new ListSelectForm(Codes.Length, true, CanBeEmpty, ListSelectDialogClipboardMode.CommaCodes, SubItems != null);
      DialogResult res;
      try
      {
        res = DoShowDialog(frm);
      }
      finally
      {
        frm.Dispose();
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
      form.TheGroupBox.Text = ListTitle;

      if ((String.IsNullOrEmpty(ImageKey) && ImageKeys==null) || (!EFPApp.ShowListImages))
        form.TheLV.SmallImageList = null;
      else
        form.TheLV.SmallImageList = EFPApp.MainImages.ImageList;

      int i;
      for (i = 0; i < Codes.Length; i++)
      {
        string thisImageKey;
        if (EFPApp.ShowListImages)
        {
          thisImageKey = ImageKey;
          if (_ImageKeys != null)
          {
            if (!String.IsNullOrEmpty(_ImageKeys[i]))
              thisImageKey = _ImageKeys[i];
          }
        }
        else
          thisImageKey = String.Empty;
        ListViewItem li = form.TheLV.Items.Add(Items[i], thisImageKey);
        if (SubItems != null)
          li.SubItems.Add(SubItems[i]);
        if (SelectedCodes != null)
          li.Checked = Array.IndexOf<string>(SelectedCodes, Codes[i]) >= 0;
        li.Tag = Codes[i];
      }

      // Значок формы
      EFPApp.MainImages.Icons.InitForm(form, ImageKey, true);

      // Все очень мерзко работает, пока не создан дескрипор HWND списка
      // Чтение свойства Control.Handler создает дескриптор. А метод CreateControl()
      // почему-то это не делает
      int dummy = (int)form.TheLV.Handle;
      if (SelectedCodes != null)
      {
        if (SelectedCodes.Length > 0)
        {
          // Позионируемся на первый выбранный элемент
          int startIndex = Array.IndexOf<string>(Codes, SelectedCodes[0]);// !!!
          if (startIndex >= 0)
          {
            form.TheLV.Items[startIndex].Focused = true;
            form.TheLV.Items[startIndex].Selected = true;
            form.TheLV.EnsureVisible(startIndex);
          }
        }
      }

      form.TheLV_Resize(null, null); // глюк ?

      if (!String.IsNullOrEmpty(HelpContext))
        form.efpForm.HelpContext = HelpContext;

      if (EFPApp.ShowDialog(form, false, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      List<string> lst = new List<string>();
      foreach (ListViewItem li in form.TheLV.Items)
      {
        if (li.Checked)
        {
          string scode = (string)(li.Tag);
          lst.Add(scode);
        }
      }

      SelectedCodes = lst.ToArray();

      return DialogResult.OK;
    }

    #endregion
  }
}
