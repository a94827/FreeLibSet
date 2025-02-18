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
  [Description("Combobox for selecting, adding and removing named presets of values for a dialog box")]
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

      _ShowImages = true;

      _Items = new ObjectCollection(this);
      TheCB_TextChanged(null, null);
    }

    #endregion

    #region Класс коллекции элементов

    /// <summary>
    /// Реализация свойства <see cref="Items"/>
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

      private readonly ParamSetComboBox _Owner;

      /// <summary>
      /// Возвращает количество элементов в выпадающем списке
      /// </summary>
      public int Count { get { return _Owner.TheCB.Items.Count; } }

      /// <summary>
      /// Получение элемента по индексу
      /// </summary>
      /// <param name="index">Индекс в диапазоне от 0 до (<see cref="Count"/>-1)</param>
      /// <returns></returns>
      public ParamSetComboBoxItem this[int index]
      {
        get { return (ParamSetComboBoxItem)(_Owner.TheCB.Items[index]); }
      }

      ParamSetComboBoxItem IList<ParamSetComboBoxItem>.this[int index]
      {
        get { return this[index]; }
        set
        {
          throw new NotImplementedException();
        }
      }

      #endregion

      #region Методы

      /// <summary>
      /// Удаляет все элементы из списка
      /// </summary>
      public void Clear()
      {
        _Owner.TheCB.Items.Clear();
        _Owner.TheCB.Text = String.Empty;
        _Owner.UseAuxText = false;
      }

      /// <summary>
      /// Добавляет элемент.
      /// Устанавливает свойство <see cref="ParamSetComboBox.UseAuxText"/>=true, если задана непустая строка <see cref="ParamSetComboBoxItem.AuxText"/>.
      /// </summary>
      /// <param name="item">Новый элемент. Не может быть null.</param>
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

      /// <summary>
      /// Добавляет несколько элементов.
      /// Устанавливает свойство <see cref="ParamSetComboBox.UseAuxText"/>=true, если задана непустая строка <see cref="ParamSetComboBoxItem.AuxText"/> хотя бы у одного элемента.
      /// </summary>
      /// <param name="items">Массив элементов.</param>
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

      /// <summary>
      /// Вставляет элемент в указанную позицию.
      /// Устанавливает свойство <see cref="ParamSetComboBox.UseAuxText"/>=true, если задана непустая строка <see cref="ParamSetComboBoxItem.AuxText"/>.
      /// </summary>
      /// <param name="index">Позиция для вставки от 0 до <see cref="Count"/></param>
      /// <param name="item">Новый элемент. Не может быть null.</param>
      public void Insert(int index, ParamSetComboBoxItem item)
      {
#if DEBUG
        if (item == null)
          throw new ArgumentNullException("item");
#endif
        _Owner.TheCB.Items.Insert(index, item);
        if (!String.IsNullOrEmpty(item.AuxText))
          _Owner.UseAuxText = true;
      }

      /// <summary>
      /// Удаление элемента
      /// </summary>
      /// <param name="item">Удаляемый элемент</param>
      /// <returns>true, если если элемент быд найден и успешно удален</returns>
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

      /// <summary>
      /// Выполняет поиск элемента
      /// </summary>
      /// <param name="item">Искомый элемент</param>
      /// <returns>Индекс элемента или (-1), если не найден</returns>
      public int IndexOf(ParamSetComboBoxItem item)
      {
        return _Owner.TheCB.Items.IndexOf(item);
      }

      /// <summary>
      /// Удаление элемента по индексу
      /// </summary>
      /// <param name="index">Индекс в диапазоне от 0 до (<see cref="Count"/>-1)</param>
      public void RemoveAt(int index)
      {
        ParamSetComboBoxItem item = (ParamSetComboBoxItem)(_Owner.TheCB.Items[index]);
        if (_Owner.TheCB.Text == item.DisplayName)
          _Owner.TheCB.Text = String.Empty;
        _Owner.TheCB.Items.RemoveAt(index);
      }


      /// <summary>
      /// Поиск элемента по коду
      /// </summary>
      /// <param name="code">Код элемента <see cref="ParamSetComboBoxItem.Code"/></param>
      /// <returns>Найденный элемент или null</returns>
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

      /// <summary>
      /// Поиск элемента по контрольной сумме
      /// </summary>
      /// <param name="md5Sum">Контрольная сумма элемента <see cref="ParamSetComboBoxItem.MD5Sum"/></param>
      /// <returns>Найденный элемент или null</returns>
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

      /// <summary>
      /// Поиск элемента по отображаемому тексту
      /// </summary>
      /// <param name="displayName">Текст элемента <see cref="ParamSetComboBoxItem.DisplayName"/></param>
      /// <returns>Найденный элемент или null</returns>
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

      /// <summary>
      /// Возвращает true, если элемент добавлен к списку
      /// </summary>
      /// <param name="item">Элемент</param>
      /// <returns>Наличие элемента</returns>
      public bool Contains(ParamSetComboBoxItem item)
      {
        return _Owner.TheCB.Items.Contains(item);
      }

      /// <summary>
      /// Копирует все элементы в массив
      /// </summary>
      /// <param name="array">Заполняемый массив длиной не менее <see cref="Count"/> плюc <paramref name="arrayIndex"/></param>
      /// <param name="arrayIndex">Начальный индекс в массиве</param>
      public void CopyTo(ParamSetComboBoxItem[] array, int arrayIndex)
      {
        _Owner.TheCB.Items.CopyTo(array, arrayIndex);
      }

      bool ICollection<ParamSetComboBoxItem>.IsReadOnly { get { return false; } }

      #endregion

      #region IEnumerable<ParamSetComboBoxItem> Members

      /// <summary>
      /// Создает перечислитель
      /// </summary>
      /// <returns>Перечислитель</returns>
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
    /// Коллекция объектов <see cref="ParamSetComboBoxItem"/>
    /// </summary>
    public ObjectCollection Items { get { return _Items; } }
    private readonly ObjectCollection _Items;

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

    /// <summary>
    /// Текущий выбранный элемент с поиском по коду <see cref="ParamSetComboBoxItem.Code"/>
    /// </summary>
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
        ParamSetComboBoxItem item = Items.FindCode(value);
        SelectedItem = item;
      }
    }

    /// <summary>
    /// Текущий выбранный элемент с поиском по сумме <see cref="ParamSetComboBoxItem.MD5Sum"/>
    /// </summary>
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

    /// <summary>
    /// Надо ли рисовать значки в списке
    /// </summary>
    [Description("Defines wheather images should be show in the drop-down list")]
    [Category("Appearance")]
    [DefaultValue(true)]
    public bool ShowImages
    {
      get { return _ShowImages; }
      set { _ShowImages = value; }
    }
    private bool _ShowImages;


    /// <summary>
    /// Если установить в true, то в тексте элемента будет выводиться сумма MD5 (для отладки)
    /// </summary>
    [Browsable(false)]
    public bool ShowMD5 { get { return _ShowMD5; } set { _ShowMD5 = value; } }
    private bool _ShowMD5;

    #endregion

    #region События

    /// <summary>
    /// Вызывается при выборе элемента из выпадающего списка
    /// </summary>
    public event ParamSetComboBoxItemEventHandler ItemSelected;

    /// <summary>
    /// Вызывает событие <see cref="ItemSelected"/>
    /// </summary>
    /// <param name="item"></param>
    protected void OnItemSelected(ParamSetComboBoxItem item)
    {
      if (ItemSelected == null)
      {
        EFPApp.ErrorMessageBox("ItemSelected handler not set");
        return;
      }
      ParamSetComboBoxItemEventArgs args = new ParamSetComboBoxItemEventArgs(item);
      ItemSelected(this, args);
    }

    /// <summary>
    /// Вызывается при нажатии кнопки "Сохранить набор"
    /// </summary>
    public event ParamSetComboBoxSaveEventHandler SaveClick;

    /// <summary>
    /// Вызывает событие <see cref="SaveClick"/>
    /// </summary>
    /// <param name="displayName">Текст, введенный пользователем в поле (имя сохраняемого набора)</param>
    protected void OnSaveClick(string displayName)
    {
      if (SaveClick == null)
      {
        EFPApp.ErrorMessageBox("SaveClick handler not set");
        return;
      }
      ParamSetComboBoxSaveEventArgs args = new ParamSetComboBoxSaveEventArgs(displayName);
      SaveClick(this, args);
    }

    /// <summary>
    /// Вызывается при нажатии кнопки "Удалить набор"
    /// </summary>
    public event ParamSetComboBoxItemEventHandler DeleteClick;

    /// <summary>
    /// Вызывает событие <see cref="DeleteClick"/>
    /// </summary>
    /// <param name="item">Удаляемый набор</param>
    protected void OnDeleteClick(ParamSetComboBoxItem item)
    {
      if (DeleteClick == null)
      {
        EFPApp.ErrorMessageBox("DeleteClick handler not set");
        return;
      }
      ParamSetComboBoxItemEventArgs args = new ParamSetComboBoxItemEventArgs(item);
      DeleteClick(this, args);
    }

    /// <summary>
    /// Вызывается для проверки возможности удаления элемента.
    /// Используется для блокирования кнопки "Удалить набор".
    /// Обработчик события должен установить свойство <see cref="CancelEventArgs.Cancel"/>=true в аргументах события,
    /// если элемент нельзя удалять (предопределенный набор параметров).
    /// </summary>
    public event ParamSetComboBoxItemCancelEventHandler CanDeleteItem;

    /// <summary>
    /// Вызывает событие <see cref="CanDeleteItem"/>
    /// </summary>
    /// <param name="item">Текущий выбранный набор</param>
    /// <returns>True, если этот набор можно удалить из списка</returns>
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

        if (ShowMD5)
          text += " (" + item.MD5Sum + ")";

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
        EFPApp.ShowException(e);
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
          EFPApp.ShowTempMessage(Res.ParamSetComboBox_Err_NameIsEmpty);
          TheCB.Select();
          return;
        }
        OnSaveClick(TheCB.Text);
        TheCB_TextChanged(null, null);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
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
          EFPApp.ShowTempMessage(Res.ParamSetComboBox_Err_UnknownName);
          TheCB.Select();
          return;
        }
        OnDeleteClick(item);
        TheCB_TextChanged(null, null);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    #endregion

    #endregion
  }

#pragma warning restore 1591

  /// <summary>
  /// Описание одного элемента для комбоблока <see cref="ParamSetComboBox"/>.
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
    /// <param name="imageKey">Имя изображения в списке <see cref="EFPApp.MainImages"/></param>
    /// <param name="writeTime">Время. Если задано, выводится в выпадающнм списке в скобках после <paramref name="displayName"/>.</param>
    /// <param name="group">Номер группы. Элементы с разными группами разделяются горизонтальной чертой</param>
    /// <param name="md5Sum">Контрольная сумма. Используется методом <see cref="ParamSetComboBox.ObjectCollection.FindMD5Sum(string)"/></param>
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

    /// <summary>
    /// Код элемента. Не отображается на экране
    /// </summary>
    public string Code { get { return _Code; } }
    private readonly string _Code;

    /// <summary>
    /// Текст элемента. Выводится в выпадающем списке и при закрытом комбоблоке
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private readonly string _DisplayName;

    /// <summary>
    /// Имя изображения в списке <see cref="EFPApp.MainImages"/>
    /// </summary>
    public string ImageKey { get { return _ImageKey; } }
    private readonly string _ImageKey;

    /// <summary>
    /// Время. Если задано, выводится в выпадающем списке в скобках после <see cref="DisplayName"/>.
    /// </summary>
    public DateTime? WriteTime { get { return _WriteTime; } }
    private readonly DateTime? _WriteTime;

    /// <summary>
    /// Номер группы. Элементы с разными группами разделяются горизонтальной чертой.
    /// </summary>
    public int Group { get { return _Group; } }
    private readonly int _Group;

    /// <summary>
    /// Контрольная сумма. Используется методом <see cref="ParamSetComboBox.ObjectCollection.FindMD5Sum(string)"/>.
    /// </summary>
    public string MD5Sum { get { return _MD5Sum; } }
    private readonly string _MD5Sum;

    /// <summary>
    /// Дополнительный текст, который выводится во второй строке выпадающего списка.
    /// Когда комбоблок закрыт, этот текст не выводится.
    /// </summary>
    public string AuxText { get { return _AuxText; } }
    private readonly string _AuxText;

    /// <summary>
    /// Произвольные данные вызывающего кода.
    /// Это свойство может устанавливаться после вызова конструктора.
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    /// <summary>
    /// Возвращает <see cref="DisplayName"/>
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _DisplayName;
      //return base.ToString();
    }

    #endregion
  }

  #region Делегаты

  /// <summary>
  /// Аргументы события <see cref="ParamSetComboBox.ItemSelected"/> и <see cref="ParamSetComboBox.DeleteClick"/>
  /// </summary>
  public class ParamSetComboBoxItemEventArgs : EventArgs
  {
    #region Конструктор

    internal ParamSetComboBoxItemEventArgs(ParamSetComboBoxItem item)
    {
      _Item = item;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текущий выбранный элемент
    /// </summary>
    public ParamSetComboBoxItem Item { get { return _Item; } }
    private readonly ParamSetComboBoxItem _Item;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="ParamSetComboBox.ItemSelected"/> и <see cref="ParamSetComboBox.DeleteClick"/>
  /// </summary>
  /// <param name="sender">Объект <see cref="ParamSetComboBox"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void ParamSetComboBoxItemEventHandler(object sender,
    ParamSetComboBoxItemEventArgs args);

  /// <summary>
  /// Аргументы события <see cref="ParamSetComboBox.SaveClick"/>
  /// </summary>
  public class ParamSetComboBoxSaveEventArgs : EventArgs
  {
    #region Конструктор

    internal ParamSetComboBoxSaveEventArgs(string displayName)
    {
      _DisplayName = displayName;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текст, введенный пользователем в поле комбоблока. Название сохраняемого набора.
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private readonly string _DisplayName;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="ParamSetComboBox.SaveClick"/>
  /// </summary>
  /// <param name="sender">Объект <see cref="ParamSetComboBox"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void ParamSetComboBoxSaveEventHandler(object sender,
    ParamSetComboBoxSaveEventArgs args);

  /// <summary>
  /// Аргументы события <see cref="ParamSetComboBox.CanDeleteItem"/>
  /// </summary>
  public class ParamSetComboBoxItemCancelEventArgs : CancelEventArgs
  {
    #region Конструктор

    internal ParamSetComboBoxItemCancelEventArgs(ParamSetComboBoxItem item)
    {
      _Item = item;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Проверяемый элемент
    /// </summary>
    public ParamSetComboBoxItem Item { get { return _Item; } }
    private readonly ParamSetComboBoxItem _Item;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="ParamSetComboBox.CanDeleteItem"/>
  /// </summary>
  /// <param name="sender">Объект <see cref="ParamSetComboBox"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void ParamSetComboBoxItemCancelEventHandler(object sender,
    ParamSetComboBoxItemCancelEventArgs args);

  #endregion
}
