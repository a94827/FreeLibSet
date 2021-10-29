using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms.Design;
using System.Collections;
using System.ComponentModel.Design;
using System.Windows.Forms.Design.Behavior;
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

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Простой GroupBox с автоматически создаваемыми радиокнопками
  /// В основном, предназначен для использования без дизайнера форм
  /// </summary>
  [Designer(typeof(RadioGroupBoxDesigner))]
  [Description("Простой GroupBox с автоматически создаваемыми радиокнопками")]
  [ToolboxBitmap(typeof(RadioGroupBox), "RadioGroupBox.bmp")]
  [ToolboxItem(true)]
  [DefaultProperty("Items")]
  [DefaultEvent("SelectedIndexChanged")]
  public class RadioGroupBox : GroupBox
  {
    #region Конструкторы

    public RadioGroupBox()
    {
      _Panel = new TableLayoutPanel();
      _Panel.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
      SetPanelLocation();
      _Panel.AutoSize = true;
      _Panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      _Panel.ColumnCount = 2;
      _Panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 24)); // 31.01.2020
      _Panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
      _Panel.Margin = new System.Windows.Forms.Padding(0);
      Controls.Add(_Panel);

      _UseMnemonic = true;
      _PrevSelectedIndex = -1;
      _Buttons = new RadioButton[0];
      _ImageLabels = new Label[0];
    }

    private void SetPanelLocation()
    {
      Rectangle rc = DisplayRectangle;
      _Panel.Location = new Point(rc.Left + Padding.Left, rc.Top + Padding.Top);
      this.Size = (this.Size - rc.Size) + _Panel.Size + Padding.Size; // 21.10.2019
    }

    public RadioGroupBox(string[] items)
      : this()
    {
      this.Items = items;
    }

    #endregion

    #region Controls

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Control.ControlCollection Controls
    {
      get
      {
        return base.Controls;
      }
    }

    private TableLayoutPanel _Panel;

    /// <summary>
    /// Массив радиокнопок.
    /// Элементы обновляются после установки свойства Items
    /// Для отдельных кнопок можно установить, например, свойство Enabled
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RadioButton[] Buttons { get { return _Buttons; } }
    private RadioButton[] _Buttons;

    /// <summary>
    /// Метки для вывода изображений
    /// </summary>
    internal Label[] ImageLabels { get { return _ImageLabels; } }
    private Label[] _ImageLabels;

    #endregion

    #region Свойство Items

    [Category("Appearance")]
    [Description("Строки текста радиокнопок. Количество строк определяет количество кнопок")]
    public string[] Items
    {
      get
      {
        string[] a = new string[Buttons.Length];
        for (int i = 0; i < Buttons.Length; i++)
          a[i] = Buttons[i].Text;
        return a;
      }
      set
      {
        if (value == null)
          value = DataTools.EmptyStrings;

        _Panel.SuspendLayout();
        try
        {
          _Panel.Controls.Clear();
          _Panel.RowStyles.Clear();
          _Panel.ColumnCount = 2;
          _Panel.RowCount = Math.Max(value.Length, 1);
          _Buttons = new RadioButton[value.Length];
          _ImageLabels = new Label[value.Length];

          for (int i = 0; i < value.Length; i++)
          {
            _Panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            _ImageLabels[i] = new Label();
            _ImageLabels[i].Text = String.Empty;
            _ImageLabels[i].UseMnemonic = false;
            _ImageLabels[i].Size = new Size(24, 16); // 31.01.2020
            _ImageLabels[i].ImageAlign = ContentAlignment.MiddleCenter;
            _ImageLabels[i].Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _ImageLabels[i].Padding = new Padding(0);
            _Panel.Controls.Add(_ImageLabels[i], 0, i);

            _Buttons[i] = new RadioButton();
            _Buttons[i].Text = value[i];
            _Buttons[i].AutoSize = true;
            _Buttons[i].Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right; // 31.01.2020
            _Buttons[i].CheckedChanged += btn_CheckedChanged;
            _Panel.Controls.Add(_Buttons[i], 1, i);
          }
          InitButtonProperties();
          _ItemImages = null; // создаем новый объект
        }
        finally
        {
          _Panel.ResumeLayout();
        }

        OnItemsChanged(EventArgs.Empty);

        // Восстанавливаем текущую позицию
        if (_PrevSelectedIndex < value.Length)
          SelectedIndex = _PrevSelectedIndex;
        else
          SelectedIndex = value.Length - 1;

        SetPanelLocation(); // 21.10.2019
      }
    }

    [Category("PropertyChanged")]
    [Description("Вызывается при установке свойства Items")]
    public event EventHandler ItemsChanged;

    protected virtual void OnItemsChanged(EventArgs args)
    {
      if (ItemsChanged != null)
        ItemsChanged(this, args);
    }

    private void InitButtonProperties()
    {
      for (int i = 0; i < _Buttons.Length; i++)
      {
        _Buttons[i].UseMnemonic = _UseMnemonic;
        _Buttons[i].FlatStyle = FlatStyle;

        _ImageLabels[i].ImageList = ImageList;
      }
    }

    protected int ItemCount { get { return _Buttons.Length; } }

    #endregion

    #region Свойство Text

    protected override void OnTextChanged(EventArgs args)
    {
      base.OnTextChanged(args);
      SetPanelLocation();
    }

    protected override void OnFontChanged(EventArgs args)
    {
      base.OnFontChanged(args);
      SetPanelLocation();
    }

    #endregion

    #region Свойство Padding

    protected override void OnPaddingChanged(EventArgs args)
    {
      base.OnPaddingChanged(args);

      //Panel.Padding = Padding;
      SetPanelLocation();
    }

    protected override void OnSizeChanged(EventArgs args)
    {
      base.OnSizeChanged(args);
      // base.Text = Size.Height.ToString();
    }

    #endregion

    #region Свойство UseMnemonic

    [DefaultValue(true)]
    [Category("Appearance")]
    [Description("Получает или задает значение, которое указывает, должен ли первый знак, следующий за знаком амперсанда (&), " +
      "должен использоваться как мнемонический ключ элемента управления.")]
    public bool UseMnemonic
    {
      get { return _UseMnemonic; }
      set
      {
        if (value == _UseMnemonic)
          return;
        _UseMnemonic = value;

        InitButtonProperties();

        // Событие не предусмотрено
      }
    }
    private bool _UseMnemonic;

    #endregion

    #region FlatStyle

    public new FlatStyle FlatStyle
    {
      get { return base.FlatStyle; }
      set
      {
        if (value == base.FlatStyle)
          return;

        base.FlatStyle = value;
        InitButtonProperties();
      }
    }

    #endregion

    #region Свойство SelectedIndex

    [DefaultValue(-1)]
    [Category("Behavior")]
    [Description("Текущая выбранная позиция в группе")]
    public int SelectedIndex
    {
      get
      {
        for (int i = 0; i < _Buttons.Length; i++)
        {
          if (_Buttons[i].Checked)
            return i;
        }
        return -1;
      }
      set
      {
        if (value == this.SelectedIndex)
          return;
        if (value < -1 || value >= ItemCount)
          throw new ArgumentOutOfRangeException();

        for (int i = 0; i < _Buttons.Length; i++)
        {
          _Buttons[i].Checked = (i == value);
        }

        OnSelectedIndexChanged(EventArgs.Empty);
        _PrevSelectedIndex = value;
      }
    }
    private int _PrevSelectedIndex;

    [Category("PropertyChanged")]
    [Description("Вызывается при изменении свойства SelectedIndex")]
    public event EventHandler SelectedIndexChanged;

    protected virtual void OnSelectedIndexChanged(EventArgs args)
    {
      if (SelectedIndexChanged != null)
        SelectedIndexChanged(this, args);
    }


    void btn_CheckedChanged(object sender, EventArgs args)
    {
      int NewSelectedIndex = this.SelectedIndex;
      if (NewSelectedIndex == _PrevSelectedIndex)
        return; // не изменилось

      _PrevSelectedIndex = NewSelectedIndex;
      OnSelectedIndexChanged(EventArgs.Empty);
    }

    #endregion

    #region Свойство DefaultSize

    public override Size GetPreferredSize(Size proposedSize)
    {
      if (_Panel == null)
        return base.GetPreferredSize(proposedSize); // вызов из конструктора

      if (ItemCount == 0)
        return base.GetPreferredSize(proposedSize);

      Size sz1 = this.Size;
      Size sz2 = DisplayRectangle.Size;
      Size sz3 = _Panel.Size;
      Size sz = (sz3 + (sz1 - sz2) + Padding.Size);
      //return WinFormsTools.Max(sz, proposedSize);
      return sz;
    }

    #endregion

    #region Изображения

    internal void InitImagesVisible()
    {
      bool HasImage = false;
      for (int i = 0; i < _Buttons.Length; i++)
      {
        if (_ImageLabels[i].Image != null)
          HasImage = true;
        else if ((_ImageLabels[i].ImageIndex >= 0 || (!String.IsNullOrEmpty(_ImageLabels[i].ImageKey))) && _ImageList != null)
          HasImage = true;
        if (HasImage)
          break;
      }

      if (HasImage)
        _Panel.ColumnStyles[0] = new ColumnStyle(SizeType.AutoSize);
      else
        _Panel.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, 0);

      _Panel.PerformLayout(); // 31.01.2020. Само по себе изменение в списке TableLayoutColumnStyleCollection не вызывает никаких действий.
                              // Это просто список. (см. исходные тексты Net Framework)
    }

    protected override void OnVisibleChanged(EventArgs args)
    {
      base.OnVisibleChanged(args);
      InitImagesVisible();
    }

    /// <summary>
    /// Свойство RadioGroupBox.ItemImages
    /// </summary>
    public sealed class ItemImageCollection : IList<RadioGroupBoxItemImage>
    {
      #region Защищенный конструктор

      internal ItemImageCollection(RadioGroupBox owner)
      {
        _Items = new RadioGroupBoxItemImage[owner._Buttons.Length];
        for (int i = 0; i < _Items.Length; i++)
          _Items[i] = new RadioGroupBoxItemImage(owner, owner._ImageLabels[i]);
      }

      private RadioGroupBoxItemImage[] _Items;

      #endregion

      #region IList<RadioGroupBoxImage> Members

      int IList<RadioGroupBoxItemImage>.IndexOf(RadioGroupBoxItemImage item)
      {
        return Array.IndexOf<RadioGroupBoxItemImage>(_Items, item);
      }

      void IList<RadioGroupBoxItemImage>.Insert(int index, RadioGroupBoxItemImage item)
      {
        throw new ObjectReadOnlyException();
      }

      void IList<RadioGroupBoxItemImage>.RemoveAt(int index)
      {
        throw new ObjectReadOnlyException();
      }

      /// <summary>
      /// Параметры изображения для заданной кнопки
      /// </summary>
      /// <param name="index">Индекс кнопки в списке Items</param>
      /// <returns>Параметры изображения для кнопки</returns>
      public RadioGroupBoxItemImage this[int index]
      {
        get { return _Items[index]; }
      }

      RadioGroupBoxItemImage IList<RadioGroupBoxItemImage>.this[int index]
      {
        get
        {
          return _Items[index];
        }
        set
        {
          throw new ObjectReadOnlyException();
        }
      }

      #endregion

      #region ICollection<RadioGroupBoxImage> Members

      void ICollection<RadioGroupBoxItemImage>.Add(RadioGroupBoxItemImage item)
      {
        throw new ObjectReadOnlyException();
      }

      void ICollection<RadioGroupBoxItemImage>.Clear()
      {
        throw new ObjectReadOnlyException();
      }

      bool ICollection<RadioGroupBoxItemImage>.Contains(RadioGroupBoxItemImage item)
      {
        return Array.IndexOf<RadioGroupBoxItemImage>(_Items, item) >= 0;
      }

      void ICollection<RadioGroupBoxItemImage>.CopyTo(RadioGroupBoxItemImage[] array, int arrayIndex)
      {
        _Items.CopyTo(array, arrayIndex);
      }

      public int Count { get { return _Items.Length; } }

      bool ICollection<RadioGroupBoxItemImage>.IsReadOnly { get { return true; } }

      bool ICollection<RadioGroupBoxItemImage>.Remove(RadioGroupBoxItemImage item)
      {
        throw new ObjectReadOnlyException();
      }

      #endregion

      #region IEnumerable<RadioGroupBoxImage> Members

      public IEnumerator<RadioGroupBoxItemImage> GetEnumerator()
      {
        return new ArrayEnumerator<RadioGroupBoxItemImage>(_Items);
      }

      #endregion

      #region IEnumerable Members

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      #endregion
    }

    // TODO: Сделать ItemImages досупным из дизайнера форм. Но тут куча проблем возникнет

    /// <summary>
    /// Управление изображениями для кнопок.
    /// Установка свойств в объектах коллекции должна выполняться после установки свойства Items
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ItemImageCollection ItemImages
    {
      get
      {
        if (_ItemImages == null)
          _ItemImages = new ItemImageCollection(this); // создаем по требованию
        return _ItemImages;
      }
    }
    private ItemImageCollection _ItemImages;

    /// <summary>
    /// Список изображений, на который можно ссылаться из свойств RadioGroupBoxImage.ImageKey или ImageIndex
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ImageList ImageList
    {
      get { return _ImageList; }
      set
      {
        _ImageList = value;
        InitButtonProperties();
        InitImagesVisible();
      }
    }
    private ImageList _ImageList;

    /// <summary>
    /// Групповая установка значков кнопок.
    /// Должно быть также установлено свойство ImageList
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string[] ImageKeys
    {
      get
      {
        string[] a = new string[_ImageLabels.Length];
        for (int i = 0; i < a.Length; i++)
          a[i] = _ImageLabels[i].ImageKey;
        return a;
      }
      set
      {
        if (value == null)
          value = new string[_ImageLabels.Length];
        if (value.Length != _ImageLabels.Length)
          throw new ArgumentException("Неправильная длина массива");

        for (int i = 0; i < _ImageLabels.Length; i++)
          _ImageLabels[i].ImageKey = value[i];
        InitImagesVisible();
      }
    }


    /// <summary>
    /// Групповая установка значков кнопок
    /// Должно быть также установлено свойство ImageList
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int[] ImageIndices
    {
      get
      {
        int[] a = new int[_ImageLabels.Length];
        for (int i = 0; i < a.Length; i++)
          a[i] = _ImageLabels[i].ImageIndex;
        return a;
      }
      set
      {
        if (value == null)
          value = new int[_ImageLabels.Length];
        if (value.Length != _ImageLabels.Length)
          throw new ArgumentException("Неправильная длина массива");

        for (int i = 0; i < _ImageLabels.Length; i++)
          _ImageLabels[i].ImageIndex = value[i];
        InitImagesVisible();
      }
    }

    /// <summary>
    /// Групповая установка значков кнопок
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image[] Images
    {
      get
      {
        Image[] a = new Image[_ImageLabels.Length];
        for (int i = 0; i < a.Length; i++)
          a[i] = _ImageLabels[i].Image;
        return a;
      }
      set
      {
        if (value == null)
          value = new Image[_ImageLabels.Length];
        if (value.Length != _ImageLabels.Length)
          throw new ArgumentException("Неправильная длина массива");

        for (int i = 0; i < _ImageLabels.Length; i++)
          _ImageLabels[i].Image = value[i];
        InitImagesVisible();
      }
    }

    #endregion
  }

  /// <summary>
  /// Свойства для задания изображения для одной кнопки в RadionGroupBox
  /// </summary>
  public sealed class RadioGroupBoxItemImage
  {
    #region Защищенный конструктор

    internal RadioGroupBoxItemImage(RadioGroupBox owner, Label label)
    {
      _Owner = owner;
      _Label = label;
    }

    private RadioGroupBox _Owner;
    private Label _Label;

    #endregion

    #region Свойства

    /// <summary>
    /// Изображение для элемента
    /// </summary>
    public Image Image
    {
      get { return _Label.Image; }
      set
      {
        if (object.ReferenceEquals(value, _Label.Image))
          return;

        _Label.Image = value;
        _Owner.InitImagesVisible();
      }
    }

    /// <summary>
    /// Индекс изображения для кнопки.
    /// Изображения хранятся в списке, задаваемым свойство RadioGroupBox.ImageList.
    /// </summary>
    public int ImageIndex
    {
      get { return _Label.ImageIndex; }
      set
      {
        //if (object.ReferenceEquals(value, _Label.ImageIndex))
        if (value == _Label.ImageIndex) // 27.12.2020
          return;

        _Label.ImageIndex = value;
        _Owner.InitImagesVisible();
      }
    }

    /// <summary>
    /// Тег изображения для кнопки.
    /// Изображения хранятся в списке, задаваемым свойство RadioGroupBox.ImageList.
    /// </summary>
    public string ImageKey
    {
      get { return _Label.ImageKey; }
      set
      {
        if (object.ReferenceEquals(value, _Label.ImageKey))
          return;

        _Label.ImageKey = value;
        _Owner.InitImagesVisible();
      }
    }

    #endregion
  }



  public class RadioGroupBoxDesigner : ControlDesigner
  {
    #region Snap lines

    // Добавляем "сиреневую" линию базовой линии текста для дизайнера формы
    // Линия берется из поля ввода первой даты
    // Взято из 
    // http://stackoverflow.com/questions/93541/baseline-snaplines-in-custom-winforms-controls
    //

#if XXXX // Не работает. Линия не на месте

    public override IList SnapLines
    {
      get
      {
        /* Code from above */
        IList snapLines = base.SnapLines;

        // *** This will need to be modified to match your user control
        RadioGroupBox control = Control as RadioGroupBox;
        if (control == null)
          return snapLines;

        if (control.Controls[0].Controls.Count == 0)
          return snapLines; // нет ни одной кнопки

        Control FirstButton=control.Controls[0].Controls[0];

        // *** This will need to be modified to match the item in your user control
        // This is the control in your UC that you want SnapLines for the entire UC
        IDesigner designer = TypeDescriptor.CreateDesigner(FirstButton, typeof(IDesigner));
        if (designer == null)
          return snapLines;

        // *** This will need to be modified to match the item in your user control
        designer.Initialize(FirstButton);

        using (designer)
        {
          ControlDesigner boxDesigner = designer as ControlDesigner;
          if (boxDesigner == null)
            return snapLines;

          foreach (SnapLine line in boxDesigner.SnapLines)
          {
            if (line.SnapLineType == SnapLineType.Baseline)
            {
              // *** This will need to be modified to match the item in your user control
              snapLines.Add(new SnapLine(SnapLineType.Baseline,
                 line.Offset + FirstButton.Top, 
                 line.Filter, line.Priority));
              break;
            }
          }
        }

        return snapLines;
      }
    }
#endif


    #endregion
  }
}
