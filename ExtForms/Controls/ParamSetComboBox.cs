// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.Core;
using FreeLibSet.UICore;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Комбоблок для выбора набора готовых параметров для диалогов параметров страницы,
  /// параметров отчета, набора фильтров
  /// Содержит комбоблок для выбора существующего набора или ввода имени нового набора,
  /// а также кнопки "Сохранить" и "Удалить"
  /// </summary>
  [Description("Комбоблок для выбора набора готовых параметров")]
  [ToolboxBitmap(typeof(ParamSetComboBox), "ParamSetComboBox.bmp")]
  public partial class ParamSetComboBox : UserControl
  {
    #region Конструктор

    /// <summary>
    /// Создает комбоблок
    /// </summary>
    public ParamSetComboBox()
    {
      InitializeComponent();
      TheCB.DrawItem += new DrawItemEventHandler(TheCB_DrawItem);
      TheCB.Enter += new EventHandler(TheCB_Enter);
      TheCB.Leave += new EventHandler(TheCB_Leave);
      TheCB.SelectedValueChanged += new EventHandler(TheCB_SelectedValueChanged);
      TheCB.TextChanged += new EventHandler(TheCB_TextChanged);
      TheCB.DrawMode = DrawMode.OwnerDrawFixed;
      TheCB.MeasureItem += new MeasureItemEventHandler(TheCB_MeasureItem);
      TheCB.DropDownHeight = 2 * TheCB.ItemHeight * 20;

      SaveButton.Click += new EventHandler(SaveButton_Click);
      SaveButton.Image = MainImagesResource.Insert;
      DeleteButton.Click += new EventHandler(DeleteButton_Click);
      DeleteButton.Image = MainImagesResource.Delete;

      _InsideSetSelectedItem = false;

      _DrawImages = true;

      _Items = new ObjectCollection(this);
      TheCB_TextChanged(null, null);
    }

    #endregion

    #region Класс коллекции элементов

    /// <summary>
    /// Реализация свойства Items
    /// </summary>
    public class ObjectCollection : IList<ParamSetComboBoxItem>
    {
      #region Конструктор

      internal ObjectCollection(ParamSetComboBox owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Свойства

      private ParamSetComboBox _Owner;

      public int Count { get { return _Owner.TheCB.Items.Count; } }

      public ParamSetComboBoxItem this[int index]
      {
        get { return (ParamSetComboBoxItem)(_Owner.TheCB.Items[index]); }
        set { throw new NotImplementedException(); }
      }

      #endregion

      #region Методы

      public void Clear()
      {
        _Owner.TheCB.Items.Clear();
        _Owner.TheCB.Text = String.Empty;
        _Owner.UseAuxText = false;
      }

      public void Add(ParamSetComboBoxItem item)
      {
#if DEBUG
        if (item == null)
          throw new ArgumentNullException("item");
#endif
        _Owner.TheCB.Items.Add(item);
        if (!String.IsNullOrEmpty(item.AuxText))
          _Owner.UseAuxText = true;
      }

      public void AddRange(ParamSetComboBoxItem[] items)
      {
        _Owner.TheCB.Items.AddRange(items);
        for (int i = 0; i < items.Length; i++)
        {
          if (!String.IsNullOrEmpty(items[i].AuxText))
          {
            _Owner.UseAuxText = true;
            break;
          }
        }
      }

      public void Insert(int Index, ParamSetComboBoxItem item)
      {
#if DEBUG
        if (item == null)
          throw new ArgumentNullException("item");
#endif
        _Owner.TheCB.Items.Insert(Index, item);
        if (!String.IsNullOrEmpty(item.AuxText))
          _Owner.UseAuxText = true;
      }

      public bool Remove(ParamSetComboBoxItem item)
      {
        if (item == null)
          return false;
        if (!_Owner.TheCB.Items.Contains(item))
          return false;
        _Owner.TheCB.Items.Remove(item);
        if (_Owner.TheCB.Text == item.DisplayName)
          _Owner.TheCB.Text = String.Empty;
        return true;
      }

      public int IndexOf(ParamSetComboBoxItem item)
      {
        return _Owner.TheCB.Items.IndexOf(item);
      }

      public void RemoveAt(int index)
      {
        ParamSetComboBoxItem item = (ParamSetComboBoxItem)(_Owner.TheCB.Items[index]);
        if (_Owner.TheCB.Text == item.DisplayName)
          _Owner.TheCB.Text = String.Empty;
        _Owner.TheCB.Items.RemoveAt(index);
      }


      public ParamSetComboBoxItem FindCode(string code)
      {
        if (String.IsNullOrEmpty(code))
          return null;
        for (int i = 0; i < Count; i++)
        {
          if (this[i].Code == code)
            return this[i];
        }
        return null;
      }

      public ParamSetComboBoxItem FindMD5Sum(string md5Sum)
      {
        if (String.IsNullOrEmpty(md5Sum))
          return null;
        for (int i = 0; i < Count; i++)
        {
          if (this[i].MD5Sum == md5Sum)
            return this[i];
        }
        return null;
      }

      public ParamSetComboBoxItem FindDisplayName(string displayName)
      {
        if (String.IsNullOrEmpty(displayName))
          return null;
        for (int i = 0; i < Count; i++)
        {
          if (this[i].DisplayName == displayName)
            return this[i];
        }
        return null;
      }

      #endregion

      #region ICollection<ParamSetComboBoxItem> Members

      public bool Contains(ParamSetComboBoxItem item)
      {
        return _Owner.TheCB.Items.Contains(item);
      }

      public void CopyTo(ParamSetComboBoxItem[] array, int arrayIndex)
      {
        _Owner.TheCB.Items.CopyTo(array, arrayIndex);
      }

      bool ICollection<ParamSetComboBoxItem>.IsReadOnly { get { return false; } }

      #endregion

      #region IEnumerable<ParamSetComboBoxItem> Members

      public IEnumerator<ParamSetComboBoxItem> GetEnumerator()
      {
        return new ConvertEnumerable<ParamSetComboBoxItem>.Enumerator(_Owner.TheCB.Items.GetEnumerator());
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return _Owner.TheCB.Items.GetEnumerator(); // преобразование не требуется
      }

      #endregion
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Коллекция объектов ParamSetComboBoxItem
    /// </summary>
    public ObjectCollection Items { get { return _Items; } }
    private ObjectCollection _Items;

    /// <summary>
    /// Устанавливается в true, если в добавляемых элементах есть доп. текст
    /// </summary>
    private bool UseAuxText
    {
      get { return TheCB.DrawMode == DrawMode.OwnerDrawVariable; }
      set
      {
        TheCB.DrawMode = value ? DrawMode.OwnerDrawVariable : DrawMode.OwnerDrawFixed;
      }
    }

    /// <summary>
    /// Текущий выбранный элемент
    /// </summary>
    public ParamSetComboBoxItem SelectedItem
    {
      get { return (ParamSetComboBoxItem)(TheCB.SelectedItem); }
      set
      {
        if (_InsideSetSelectedItem)
          return;

        _InsideSetSelectedItem = true;
        try
        {
          TheCB.SelectedItem = value;
          if (TheCB.Focused)
            TheCB.SelectAll();
          else
            TheCB.Select(0, 0);
          TheCB_TextChanged(null, null);
        }
        finally
        {
          _InsideSetSelectedItem = false;
        }
      }
    }

    private bool _InsideSetSelectedItem;

    public string SelectedCode
    {
      get
      {
        ParamSetComboBoxItem item = SelectedItem;
        if (item == null)
          return String.Empty;
        else
          return item.Code;
      }
      set
      {
        ParamSetComboBoxItem Item = Items.FindCode(value);
        SelectedItem = Item;
      }
    }

    public string SelectedMD5Sum
    {
      get
      {
        ParamSetComboBoxItem item = SelectedItem;
        if (item == null)
          return String.Empty;
        else
          return item.MD5Sum;
      }
      set
      {
        ParamSetComboBoxItem item = Items.FindMD5Sum(value);
        SelectedItem = item;
      }
    }

    [Description("Надо ли рисовать значки в списке")]
    [Category("Appearance")]
    [DefaultValue(true)]
    public bool ShowImages
    {
      get { return _DrawImages; }
      set { _DrawImages = value; }
    }
    private bool _DrawImages;

    #endregion

    #region События

    public event ParamSetComboBoxItemEventHandler ItemSelected;

    protected void OnItemSelected(ParamSetComboBoxItem item)
    {
      if (ItemSelected == null)
      {
        EFPApp.ErrorMessageBox("Обработчик ItemSelected не установлен");
        return;
      }
      ParamSetComboBoxItemEventArgs args = new ParamSetComboBoxItemEventArgs(item);
      ItemSelected(this, args);
    }

    public event ParamSetComboBoxSaveEventHandler SaveClick;

    protected void OnSaveClick(string displayName)
    {
      if (SaveClick == null)
      {
        EFPApp.ErrorMessageBox("Обработчик SaveClick не установлен");
        return;
      }
      ParamSetComboBoxSaveEventArgs args = new ParamSetComboBoxSaveEventArgs(displayName);
      SaveClick(this, args);
    }

    public event ParamSetComboBoxItemEventHandler DeleteClick;

    protected void OnDeleteClick(ParamSetComboBoxItem item)
    {
      if (DeleteClick == null)
      {
        EFPApp.ErrorMessageBox("Обработчик DeleteClick не установлен");
        return;
      }
      ParamSetComboBoxItemEventArgs args = new ParamSetComboBoxItemEventArgs(item);
      DeleteClick(this, args);
    }

    public event ParamSetComboBoxItemCancelEventHandler CanDeleteItem;

    protected bool OnCanDeleteItem(ParamSetComboBoxItem item)
    {
      if (CanDeleteItem == null)
        return true;
      ParamSetComboBoxItemCancelEventArgs args = new ParamSetComboBoxItemCancelEventArgs(item);
      args.Cancel = false;
      CanDeleteItem(this, args);
      return !args.Cancel;
    }

    #endregion

    #region Обработчики

    #region От комбоблока

    private IButtonControl _OldDefButton;

    void TheCB_Enter(object sender, EventArgs args)
    {
      _OldDefButton = FindForm().AcceptButton;
      FindForm().AcceptButton = SaveButton;
    }

    void TheCB_Leave(object sender, EventArgs args)
    {
      FindForm().AcceptButton = _OldDefButton;
    }

    void TheCB_DrawItem(object sender, DrawItemEventArgs args)
    {
      string text;
      string imageKey;
      bool sepLine = false; // нужно ли нарисовать горизонтальную линию после текущего элемента списка
      UIValidateState validateState = UIValidateState.Ok;
      if (args.Index < 0 || args.Index >= _Items.Count)
      {
        text = TheCB.Text;
        imageKey = "EmptyImage";
      }
      else
      {
        ParamSetComboBoxItem item = _Items[args.Index];
        text = item.DisplayName;
        imageKey = item.ImageKey;
        if (item.WriteTime.HasValue)
        {
          text += " (" + item.WriteTime.Value.ToString() + ")";
          if (item.WriteTime.Value > DateTime.Now)
          {
            validateState = UIValidateState.Warning;
            imageKey = "Warning";
          }
        }
        //if (AccDepClientExec.DebugShowIds)
        //  Text += " \"" + Item.Code + "\"";

        if (!String.IsNullOrEmpty(item.AuxText)) // а не UseAuxText
          text += Environment.NewLine + item.AuxText;

        if (args.Index < (_Items.Count - 1))
        {
          if (item.Group != _Items[args.Index + 1].Group)
            sepLine = true;
        }
      }

      if (args.Index < 0)
        ListControlImagePainter.PerformDrawItem(TheCB, args, text, ShowImages ? imageKey : String.Empty, validateState);
      else
      {
        bool alterColor = false;
        if (UseAuxText)
          alterColor = (args.Index % 2) == 1;

        ListControlImagePainter.PerformDrawItem(TheCB, args, text, ShowImages ? imageKey : String.Empty,
          alterColor ? EFPApp.Colors.ListAlter : EFPApp.Colors.ListStateOk);
      }

      if (sepLine)
      {
        Pen linePen = new Pen(Color.Black, 2);
        linePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
        args.Graphics.DrawLine(linePen,
          args.Bounds.Left + 24, args.Bounds.Bottom - 2, args.Bounds.Right, args.Bounds.Bottom - 2);
      }
    }

    void TheCB_SelectedValueChanged(object sender, EventArgs args)
    {
      if (DesignMode)
        return;

      if (_InsideSetSelectedItem)
        return;

      try
      {
        if (TheCB.SelectedIndex >= 0 && TheCB.SelectedIndex < Items.Count)
        {
          int index = TheCB.SelectedIndex;
          //TheCB.SelectedIndexEx = -1;
          //TheCB.Text = Items[Index].DisplayName;
          OnItemSelected(Items[index]);
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка выбора элемента");
      }
    }

    void TheCB_TextChanged(object sender, EventArgs args)
    {
      string s = TheCB.Text.Trim();
      if (String.IsNullOrEmpty(s))
      {
        SaveButton.Enabled = false;
        DeleteButton.Enabled = false;
      }
      else
      {
        SaveButton.Enabled = true;
        ParamSetComboBoxItem item = Items.FindDisplayName(s);
        if (item == null)
          DeleteButton.Enabled = false;
        else
          DeleteButton.Enabled = OnCanDeleteItem(item);
      }
    }

    void TheCB_MeasureItem(object sender, MeasureItemEventArgs args)
    {
      if (args.Index >= 0 && UseAuxText)
        args.ItemHeight *= 2;
    }

    #endregion

    #region От кнопок

    void SaveButton_Click(object sender, EventArgs args)
    {
      if (DesignMode)
        return;

      try
      {
        TheCB.Text = TheCB.Text.Trim();
        if (String.IsNullOrEmpty(TheCB.Text))
        {
          EFPApp.ShowTempMessage("Не задано название для набора");
          TheCB.Select();
          return;
        }
        OnSaveClick(TheCB.Text);
        TheCB_TextChanged(null, null);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработки нажатия кнопки сохранения");
      }
    }

    void DeleteButton_Click(object sender, EventArgs args)
    {
      if (DesignMode)
        return;

      try
      {
        TheCB.Text = TheCB.Text.Trim();
        ParamSetComboBoxItem item = Items.FindDisplayName(TheCB.Text);
        if (item == null)
        {
          EFPApp.ShowTempMessage("Нет такой строки в списке");
          TheCB.Select();
          return;
        }
        OnDeleteClick(item);
        TheCB_TextChanged(null, null);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработки нажатия кнопки удаления");
      }
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// Описание одного элемента для комбоблока ParamSetComboBox
  /// </summary>
  public class ParamSetComboBoxItem
  {
    #region Конструкторы

    /// <summary>
    /// Создает элемент
    /// </summary>
    /// <param name="code">Код элемента. Не отображается на экране</param>
    /// <param name="displayName">Текст элемента. Выводится в выпадающем списке и при закрытом комбоблоке</param>
    /// <param name="imageKey">Имя изображения в списке EFPApp.MainImages</param>
    /// <param name="writeTime">Время. Если задано, выводится в выпадающем списке в скобках после <paramref name="displayName"/>.</param>
    /// <param name="group">Номер группы. Элементы с разными группами разделяются горизонтальной чертой</param>
    /// <param name="md5Sum">Контрольная сумма. Используется методом ParamSetComboBox.ObjectCollection.FindMD5Sum()</param>
    public ParamSetComboBoxItem(string code, string displayName, string imageKey, DateTime? writeTime, int group, string md5Sum)
      : this(code, displayName, imageKey, writeTime, group, md5Sum, String.Empty)
    {
    }

    /// <summary>
    /// Создает элемент
    /// </summary>
    /// <param name="code">Код элемента. Не отображается на экране</param>
    /// <param name="displayName">Текст элемента. Выводится в выпадающем списке и при закрытом комбоблоке</param>
    /// <param name="imageKey">Имя изображения в списке EFPApp.MainImages</param>
    /// <param name="writeTime">Время. Если задано, выводится в выпадающнм списке в скобках после <paramref name="displayName"/>.</param>
    /// <param name="group">Номер группы. Элементы с разными группами разделяются горизонтальной чертой</param>
    /// <param name="md5Sum">Контрольная сумма. Используется методом ParamSetComboBox.ObjectCollection.FindMD5Sum()</param>
    /// <param name="auxText">Дополнительный текст, отображаемый в выпадающем списке под основным текстом.
    /// Когда комбоблок закрыт, этот текст не выводится</param>
    public ParamSetComboBoxItem(string code, string displayName, string imageKey, DateTime? writeTime, int group, string md5Sum, string auxText)
    {
      _Code = code;
      _DisplayName = displayName;
      _ImageKey = imageKey;
      _WriteTime = writeTime;
      _Group = group;
      _MD5Sum = md5Sum;
      _AuxText = auxText;
    }

    #endregion

    #region Свойства

    public string Code { get { return _Code; } }
    private string _Code;

    public string DisplayName { get { return _DisplayName; } }
    private string _DisplayName;

    public string ImageKey { get { return _ImageKey; } }
    private string _ImageKey;

    public DateTime? WriteTime { get { return _WriteTime; } }
    private DateTime? _WriteTime;

    public int Group { get { return _Group; } }
    private int _Group;

    public string MD5Sum { get { return _MD5Sum; } }
    private string _MD5Sum;

    /// <summary>
    /// Дополнительный текст, который выводится во второй строке выпадающего списка.
    /// Когда комбоблок закрыт, этот текст не выводится.
    /// </summary>
    public string AuxText { get { return _AuxText; } }
    private string _AuxText;

    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    public override string ToString()
    {
      return _DisplayName;
      //return base.ToString();
    }

    #endregion
  }

  #region Делегаты

  public class ParamSetComboBoxItemEventArgs : EventArgs
  {
    #region Конструктор

    public ParamSetComboBoxItemEventArgs(ParamSetComboBoxItem item)
    {
      _Item = item;
    }

    #endregion

    #region Свойства

    public ParamSetComboBoxItem Item { get { return _Item; } }
    private ParamSetComboBoxItem _Item;

    #endregion
  }

  public delegate void ParamSetComboBoxItemEventHandler(object sender,
    ParamSetComboBoxItemEventArgs args);

  public class ParamSetComboBoxSaveEventArgs : EventArgs
  {
    #region Конструктор

    public ParamSetComboBoxSaveEventArgs(string displayName)
    {
      _DisplayName = displayName;
    }

    #endregion

    #region Свойства

    public string DisplayName { get { return _DisplayName; } }
    private string _DisplayName;

    #endregion
  }

  public delegate void ParamSetComboBoxSaveEventHandler(object sender,
    ParamSetComboBoxSaveEventArgs args);

  public class ParamSetComboBoxItemCancelEventArgs : CancelEventArgs
  {
    #region Конструктор

    public ParamSetComboBoxItemCancelEventArgs(ParamSetComboBoxItem item)
    {
      _Item = item;
    }

    #endregion

    #region Свойства

    public ParamSetComboBoxItem Item { get { return _Item; } }
    private ParamSetComboBoxItem _Item;

    #endregion
  }

  public delegate void ParamSetComboBoxItemCancelEventHandler(object sender,
    ParamSetComboBoxItemCancelEventArgs args);

  #endregion
}
