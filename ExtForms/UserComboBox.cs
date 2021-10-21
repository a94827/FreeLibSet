using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Collections;
using System.ComponentModel.Design;
using System.Windows.Forms.Design.Behavior;

#pragma warning disable 1591

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
 * Комбоблоки, реализующие произвольные действия при нажатии кнопки выбора
 * 
 * UserComboBoxBase - базовый класс управляющего элемента. Состоит из следующих 
 * элементов:
 * [i][Main control][v][x][^]
 * [i] - значок
 * [Main control] - основной управляющий элемент (например, TextBox). Создается
 *                  классом - наследником. UserComboBoxBase считает, что он имеет
 *                  тип Control
 * [v] - кнопка выпадающего списка
 * [x] - кнопка очистки
 * [^] - кнопка редактирования
 *    
 *   UserSelComboBox - комбоблок, предназначенный только для выбора, без
 *   возможности редактирования значения
 *   
 *   UserTextComboBoxBase - базовый класс второго уровня, из которого выводятся
 *   комбоблоки с возможностью редактирования. В UserTextComboBoxBase предполагается,
 *   что MainControl имеет тип TextBoxBase. Реализует свойство ReadOnly
 *   
 *     UserTextComboBox - комбоблок с редактированием, предназначенный для ввода
 *     текста. В качестве MainControl предоставляет TextBox
 *     
 *     UserMaskedComboBox комбоблок с редактированием, предназначенный для ввода
 *     форматированного значения с помощью MaskedTextBox. Реализует свойство Mask
 *     
 *     DateBox - еще один комбоблок с MaskedTextBox в качестве поля ввода.
 *     Предназначен для ввода дат. Реализует обработку нажатия кнопки выпадающего
 *     списка для выбора даты из календарика
 */

namespace FreeLibSet.Controls
{
#if XXX
  public class SelectableLabel:Label
  {
  #region Конструктор

    public SelectableLabel()
    {
      SetStyle(ControlStyles.Selectable, true);
      SetStyle(ControlStyles.StandardClick, true);
      SetStyle(ControlStyles.StandardDoubleClick, true);
    }

    #endregion

  #region Заглушки для свойств

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool UseMnemonic { get { return base.UseMnemonic; } set { base.UseMnemonic = value; } }

    #endregion
  }
#endif


  public enum UserComboBoxEditButtonKind
  {
    Edit,
    View
  }

  /// <summary>
  /// Базовый класс для комбоблоков с произвольной реакцией на нажатие кнопки
  /// выпадающего списка. Не определяет тип основного поля. Добавляет необязательный
  /// значок слева от основного поля и две дополнительные кнопки справа - "Очистить"
  /// и "Редактировать"
  /// </summary>
  [Designer(typeof(UserComboBoxDesigner))]
  [ToolboxItem(false)]
  public class UserComboBoxBase : UserControl
  {
    #region Конструктор

    public UserComboBoxBase(Control mainControl)
    {
      SetStyle(ControlStyles.FixedHeight, true);
      //SetStyle(ControlStyles.ResizeRedraw, true);

      base.BackColor = SystemColors.Window;
      base.ForeColor = SystemColors.WindowText;

      base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;

      // Реализовано на уровне EFPControl
      //MainControl.BackColor = SystemColors.Window; 
      //MainControl.ForeColor = SystemColors.WindowText;

      #region Статический список изображений

      if (_TheImageList == null)
      {
        _TheImageList = new System.Windows.Forms.ImageList();
        _TheImageList.ImageSize = new System.Drawing.Size(12, 12);
        _TheImageList.TransparentColor = Color.Magenta;

        FreeLibSet.Forms.DummyForm frm = new FreeLibSet.Forms.DummyForm();
        for (int i = 0; i < frm.UserComboBoxImageList.Images.Count; i++)
          _TheImageList.Images.Add(frm.UserComboBoxImageList.Images.Keys[i], frm.UserComboBoxImageList.Images[i]);
        frm.Dispose();
      }

      #endregion

      #region Создание элементов

      // Все элементы сначала создаются, а потом присоедияются
      // Если делать по порядку, то вызываются виртуальные методы UserControl,
      // а объекты еще не готовы
      _ImageLabel = new Label();
      _MainControl = mainControl;
      _ThePopupButton = new ControlRightButton();
      _TheClearButton = new ControlRightButton();
      _TheEditButton = new ControlRightButton();

      #endregion

      #region Основной элемент

      _MainControl.Dock = DockStyle.Fill;
      _MainControl.TabIndex = 1;
      Controls.Add(_MainControl);

      mainControl.KeyDown += new System.Windows.Forms.KeyEventHandler(MainControl_KeyDown);
      mainControl.KeyUp += new KeyEventHandler(MainControl_KeyUp);
      mainControl.KeyPress += new KeyPressEventHandler(MainControl_KeyPress);

      mainControl.MouseDown += new MouseEventHandler(MainControl_MouseDown);
      mainControl.MouseUp += new MouseEventHandler(MainControl_MouseUp);
      mainControl.MouseClick += new MouseEventHandler(MainControl_MouseClick);
      mainControl.MouseDoubleClick += new MouseEventHandler(MainControl_MouseDoubleClick);
      mainControl.MouseWheel += new MouseEventHandler(MainControl_MouseWheel);
      mainControl.Click += new EventHandler(MainControl_Click);
      mainControl.DoubleClick += new EventHandler(MainControl_DoubleClick);

      #endregion

      #region Изображение

      _ImageLabel.Width = 18;
      _ImageLabel.Dock = DockStyle.Left;
      _ImageLabel.UseMnemonic = false;
      _ImageLabel.Text = String.Empty;
      _ImageLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
      _ImageLabel.TabIndex = 0;
      _ImageLabel.BackColor = SystemColors.Control;
      Controls.Add(_ImageLabel);
      _ImageLabel.Visible = false;

      #endregion

      #region Кнопка выпадающего списка

      _ThePopupButton.Dock = System.Windows.Forms.DockStyle.Right;
      _ThePopupButton.ComboBoxButton = true;
      _ThePopupButton.TabIndex = 2;
      _ThePopupButton.Click += new EventHandler(ThePopupButton_Click);
      Controls.Add(_ThePopupButton);
      _PopupButton = true;

      #endregion

      #region Кнопка очистки

      _TheClearButton.Dock = System.Windows.Forms.DockStyle.Right;
      _TheClearButton.ImageList = _TheImageList;
      _TheClearButton.ImageKey = "Clear";
      _TheClearButton.TabIndex = 3;
      _TheClearButton.Click += new EventHandler(TheClearButton_Click);
      Controls.Add(_TheClearButton);
      _ClearButton = false;
      _TheClearButton.Visible = false;

      #endregion

      #region Кнопка редактирования

      _TheEditButton.Dock = System.Windows.Forms.DockStyle.Right;
      _TheEditButton.ImageList = _TheImageList;
      _TheEditButton.ImageKey = "Edit";
      _TheEditButton.TabIndex = 4;
      _TheEditButton.Click += new EventHandler(TheEditButton_Click);
      Controls.Add(_TheEditButton);
      _EditButton = false;
      _TheEditButton.Visible = false;
      _EditButtonKind = UserComboBoxEditButtonKind.Edit;

      #endregion

    }

    #endregion

    #region Статический ImageList

    private static ImageList _TheImageList = null;

    #endregion

    #region Поля дочерних элементов управления

    /// <summary>
    /// Метка слева для показа изображения
    /// </summary>
    private Label _ImageLabel;

    /// <summary>
    /// Основной управляющий элемент TextBox или MaskedTextBox
    /// </summary>
    internal protected Control MainControl { get { return _MainControl; } }
    private Control _MainControl;

    /// <summary>
    /// Кнопка "V" для выпадающего списка
    /// </summary>
    private ControlRightButton _ThePopupButton;

    /// <summary>
    /// Кнопка "X" очистки значения
    /// </summary>
    private ControlRightButton _TheClearButton;

    /// <summary>
    /// Кнопка "^" для редактирования значения
    /// </summary>
    private ControlRightButton _TheEditButton;

    #endregion

    #region Изображение

    [Description("Изображение слева от текста")]
    [Category("Appearance")]
    [DefaultValue(null)]
    [RefreshProperties(RefreshProperties.All)]
    public Image Image
    {
      get { return _ImageLabel.Image; }
      set
      {
        _ImageLabel.Image = value;
        _ImageLabel.Visible = (value != null);
      }
    }

    [Description("Список, из которого выбирается изображение слева от текста")]
    [Category("Appearance")]
    [DefaultValue(null)]
    [RefreshProperties(RefreshProperties.All)]
    public ImageList ImageList
    {
      get { return _ImageLabel.ImageList; }
      set
      {
        _ImageLabel.ImageList = value;
        _ImageLabel.Visible = (value != null);
      }
    }

    [Description("Позиция в списке для изображения слева от текста")]
    [Category("Appearance")]
    [DefaultValue(-1)]
    [RefreshProperties(RefreshProperties.All)]
    public int ImageIndex
    {
      get { return _ImageLabel.ImageIndex; }
      set { _ImageLabel.ImageIndex = value; }
    }

    [Description("Имя в списке для изображения слева от текста")]
    [Category("Appearance")]
    [DefaultValue("")]
    [RefreshProperties(RefreshProperties.All)]
    public string ImageKey
    {
      get { return _ImageLabel.ImageKey; }
      set { _ImageLabel.ImageKey = value; }
    }

    #endregion

    #region BackColor и ForeColor

    [DefaultValue(typeof(Color), "Window")]
    public override Color BackColor
    {
      get
      {
        return base.BackColor;
      }
      set
      {
        base.BackColor = value;
        MainControl.BackColor = value;
      }
    }

    [DefaultValue(typeof(Color), "WindowText")]
    public override Color ForeColor
    {
      get
      {
        //return MainControl.ForeColor;
        // 01.10.2013
        return base.ForeColor;
      }
      set
      {
        base.ForeColor = value;
        MainControl.ForeColor = value;
      }
    }

    public override void ResetBackColor()
    {
      MainControl.ResetBackColor();
      base.BackColor = MainControl.BackColor;
    }

    public override void ResetForeColor()
    {
      MainControl.ResetForeColor();
      base.ForeColor = MainControl.ForeColor;
    }

    #endregion

    #region Свойство Text

    [DefaultValue("")]
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[DispId(-517)]
    public override string Text
    {
      get { return MainControl.Text; }
      set { MainControl.Text = value; }
    }

    #endregion

    #region Обработка нажатия клавиш

    private void MainControl_KeyDown(object sender, KeyEventArgs args)
    {
      if (!args.Handled)
      {
        if (args.KeyCode == Keys.Down || args.KeyCode == Keys.Up)
        {
          if (args.KeyCode == Keys.Down && args.Alt)
          {
            if (_ThePopupButton.Visible && _ThePopupButton.Enabled)
              _ThePopupButton.PerformClick();
          }
          args.Handled = true;
        }

        if (args.KeyCode == Keys.Space && ClearButton && ClearButtonEnabled)
        {
          PerformClear();
          args.Handled = true;
        }

        if (args.KeyCode == Keys.F2 && EditButton && EditButtonEnabled)
        {
          PerformEdit();
          args.Handled = true;
        }
      }

      OnKeyDown(args);
    }


    #endregion

    #region Передача других событий от основного элемента

    void MainControl_Click(object sender, EventArgs args)
    {
      OnClick(args);
    }

    void MainControl_DoubleClick(object sender, EventArgs args)
    {
      OnDoubleClick(args);
    }

    void MainControl_MouseClick(object sender, MouseEventArgs args)
    {
      OnMouseClick(args);
    }

    void MainControl_MouseDoubleClick(object sender, MouseEventArgs args)
    {
      OnMouseDoubleClick(args);
    }

    void MainControl_MouseUp(object sender, MouseEventArgs args)
    {
      OnMouseUp(args);
    }

    void MainControl_MouseDown(object sender, MouseEventArgs args)
    {
      OnMouseDown(args);
    }

    void MainControl_MouseWheel(object sender, MouseEventArgs args)
    {
      OnMouseWheel(args);
    }

    void MainControl_KeyPress(object sender, KeyPressEventArgs args)
    {
      OnKeyPress(args);
    }

    void MainControl_KeyUp(object sender, KeyEventArgs args)
    {
      OnKeyUp(args);
    }

    #endregion

    #region Кнопка выпадающего списка

    #region PopupButton

    [DefaultValue(true)]
    [Description("Видимость кнопки выпадающего списка (по умолчанию - true)")]
    [Category("Behavior")]
    public bool PopupButton
    {
      get
      {
        return _PopupButton;
      }
      set
      {
        if (value == _PopupButton)
          return;
        _PopupButton = value;
        _ThePopupButton.Visible = value;
        OnPopupButtonChanged(EventArgs.Empty);
      }
    }
    /// <summary>
    /// Нельзя использовать ThePopupButton.Visible в качестве предыдущего значения,
    /// т.к. пока форма не показана, свойство Visible всегда false
    /// </summary>
    private bool _PopupButton;

    [Description("Вызывается при изменении видимости кнопки выпадающего списка (свойства PopupButton)")]
    [Category("Property Changed")]
    public event EventHandler PopupButtonChanged;

    protected virtual void OnPopupButtonChanged(EventArgs args)
    {
      if (PopupButtonChanged != null)
        PopupButtonChanged(this, args);
    }

    #endregion

    #region PopupButtonEnabled

    [Description("Кнопка выпадающего списка может быть заблокирована, оставаясь видимой на экране")]
    [DefaultValue(true)]
    [Category("Behavior")]
    public bool PopupButtonEnabled
    {
      get { return _ThePopupButton.Enabled; }
      set { _ThePopupButton.Enabled = value; }
    }

    [Description("Вызывается при изменении значения свойства PopupButtonEnabled")]
    [Category("Property Changed")]
    public event EventHandler PopupButtonEnabledChanged
    {
      add { _ThePopupButton.EnabledChanged += value; }
      remove { _ThePopupButton.EnabledChanged -= value; }
    }

    #endregion

    #region PopupButtonToolTipText

    [Description("Всплывающая подсказка для кнопки выпадающего списка")]
    [DefaultValue("")]
    [Category("Appearance")]
    public string PopupButtonToolTipText
    {
      get { return _ThePopupButton.ToolTipText; }
      set
      {
        string s = value;

        // Тогда надо убирать описание клавиши из возвращаемого значения
        //if (!String.IsNullOrEmpty(value))
        //  s += " (" + EFPCommandItem.GetShortCutText(Keys.Alt|Keys.Down) + ")";

        _ThePopupButton.ToolTipText = s;
      }
    }

    #endregion

    #region PopupClick

    /// <summary>
    /// Устанавливается в true для предотвращения повторного нажатия кнопки,
    /// если предыдущее нажатие еще не отработано
    /// </summary>
    private bool _InsideButtonClick = false;

    /*
     * Реализации события Click для кнопок Popup, Clear и Edit
     * 1. Объявлены события PopupClick, ClearClick и EditClick
     * 2. Объявлены виртуальные защищенные методы OnXxxClick(EventArgs),
     *    которые вызывают событие. Предназначены для переопределения в классах-наследниках
     * 3. Реализованы невиртуальные public- методы PerformXxxClick() для эмуляции
     *    нажатий на кнопку пользовательским кодом
     * 4. К кнопке присоединяется обработчик события Click, вызывающий PerfofmXxxClick()
     * 5. Объявлены protected- свойства HasXxxClickHandler, которые позволяют
     *    определить наличие обработчика события
     */

    [Description("Вызывается при нажатии пользователем на кнопку выбора комбоблока")]
    [Category("Action")]
    public event EventHandler PopupClick;

    protected virtual void OnPopupClick(EventArgs args)
    {
      if (PopupClick != null)
        PopupClick(this, args);
    }

    /// <summary>
    /// Эмуляция нажатия на кнопку выбора комбоблока
    /// </summary>
    public void PerformPopup()
    {
      if (_InsideButtonClick)
        return;
      if (_ThePopupButton.Visible && _ThePopupButton.Enabled)
      {
        _InsideButtonClick = true;
        try
        {
          OnPopupClick(EventArgs.Empty);
        }
        finally
        {
          _InsideButtonClick = false;
        }
      }
    }

    private void ThePopupButton_Click(object sender, EventArgs args)
    {
      PerformPopup();
    }

    /// <summary>
    /// Возвращает true, если установлен пользовательский обработчик события PopupClick
    /// </summary>
    protected bool HasPopupClickHandler { get { return PopupClick != null; } }

    #endregion

    #endregion

    #region Кнопка "Очистить"

    #region ClearButton

    [DefaultValue(false)]
    [Description("Видимость кнопки \"Очистить\" (\"Х\")")]
    [Category("Behavior")]
    public bool ClearButton
    {
      get
      {
        return _ClearButton;
      }
      set
      {
        if (value == _ClearButton)
          return;
        _ClearButton = value;
        _TheClearButton.Visible = value;
        OnClearButtonChanged(EventArgs.Empty);
      }
    }

    /// <summary>
    /// Нельзя использовать TheClearButton.Visible в качестве предыдущего значения,
    /// т.к. пока форма не показана, свойство Visible всегда false
    /// </summary>
    private bool _ClearButton;

    [Description("Вызывается при изменении значения свойства ClearButton")]
    [Category("Property Changed")]
    public event EventHandler ClearButtonChanged;

    protected virtual void OnClearButtonChanged(EventArgs args)
    {
      if (ClearButtonChanged != null)
        ClearButtonChanged(this, args);
    }

    #endregion

    #region ClearButtonEnabled

    [DefaultValue(true)]
    [Description("Кнопка \"Очистить\" может быть заблокирована, оставаясь видимой на экране")]
    [Category("Behavior")]
    public bool ClearButtonEnabled
    {
      get { return _TheClearButton.Enabled; }
      set { _TheClearButton.Enabled = value; }
    }

    [Description("Вызывается при изменении значения свойства ClearButtonEnabled")]
    [Category("Property Changed")]
    public event EventHandler ClearButtonEnabledChanged
    {
      add { _TheClearButton.EnabledChanged += value; }
      remove { _TheClearButton.EnabledChanged -= value; }
    }

    #endregion

    #region ClearButtonToolTipText

    [Description("Всплывающая подсказка для кнопки \"Отмена\"")]
    [DefaultValue("")]
    [Category("Appearance")]
    public string ClearButtonToolTipText
    {
      get { return _TheClearButton.ToolTipText; }
      set
      {
        string s = value;
        //if ((!String.IsNullOrEmpty(value)) && ReadOnly)
        //  s += " (" + EFPCommandItem.GetShortCutText(Keys.Space) + ")";
        _TheClearButton.ToolTipText = s;
      }
    }

    #endregion

    #region ClearClick

    [Description("Вызывается при нажатии на кнопку \"X\" или клавиши пробел при ClearButton=True")]
    [Category("Action")]
    public event EventHandler ClearClick;

    /// <summary>
    /// Вызывает обработчик события ClearClick
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnClearClick(EventArgs args)
    {
      if (ClearClick != null)
        ClearClick(this, args);
    }

    /// <summary>
    /// Эмуляция нажатия на кнопку "X"
    /// </summary>
    public void PerformClear()
    {
      if (_InsideButtonClick)
        return;

      if (_TheClearButton.Visible && _TheClearButton.Enabled)
      {
        _InsideButtonClick = true;
        try
        {
          OnClearClick(EventArgs.Empty);
        }
        finally
        {
          _InsideButtonClick = false;
        }
      }
    }

    private void TheClearButton_Click(object sender, EventArgs args)
    {
      PerformClear();
    }

    /// <summary>
    /// Возвращает true, если установлен пользовательский обпвботчик события ClearClick
    /// </summary>
    protected bool HasClearClickHandler { get { return ClearClick != null; } }

    #endregion

    #endregion

    #region Кнопка "Редактировать"

    #region EditButton

    [DefaultValue(false)]
    [Description("Видимость кнопки \"Редактировать\" (\"^\", клавиша F2)")]
    [Category("Behavior")]
    public bool EditButton
    {
      get
      {
        return _EditButton;
      }
      set
      {
        if (value == _EditButton)
          return;
        _EditButton = value;
        _TheEditButton.Visible = value;
        OnEditButtonChanged(EventArgs.Empty);
      }
    }
    /// <summary>
    /// Нельзя использовать TheEditButton.Visible в качестве предыдущего значения,
    /// т.к. пока форма не показана, свойство Visible всегда false
    /// </summary>
    private bool _EditButton;

    [Description("Вызывается при изменении значения свойства EditButton")]
    [Category("Property Changed")]
    public event EventHandler EditButtonChanged;

    protected virtual void OnEditButtonChanged(EventArgs args)
    {
      if (EditButtonChanged != null)
        EditButtonChanged(this, args);
    }

    #endregion

    #region EditButtonEnabled

    [Description("Кнопка \"Редактировать\" может быть заблокирована, оставаясь видимой на экране")]
    [DefaultValue(true)]
    [Category("Behavior")]
    public bool EditButtonEnabled
    {
      get { return _TheEditButton.Enabled; }
      set { _TheEditButton.Enabled = value; }
    }

    [Description("Вызывается при изменении значения свойства EditButtonEnabled")]
    [Category("Property Changed")]
    public event EventHandler EditButtonEnabledChanged
    {
      add { _TheEditButton.EnabledChanged += value; }
      remove { _TheEditButton.EnabledChanged -= value; }
    }

    [Description("Вариант изображения на кнопке \"Редактировать\"")]
    [DefaultValue(UserComboBoxEditButtonKind.Edit)]
    [Category("Appearance")]
    public UserComboBoxEditButtonKind EditButtonKind
    {
      get { return _EditButtonKind; }
      set
      {
        if (value == _EditButtonKind)
          return;
        switch (value)
        {
          case UserComboBoxEditButtonKind.Edit:
            _TheEditButton.ImageKey = "Edit";
            break;
          case UserComboBoxEditButtonKind.View:
            _TheEditButton.ImageKey = "View";
            break;
          default:
            throw new InvalidEnumArgumentException();
        }
        _EditButtonKind = value;
      }
    }
    private UserComboBoxEditButtonKind _EditButtonKind;

    #endregion

    #region EditButtonToolTipText

    [Description("Всплывающая подсказка для кнопки \"Редактировать\"")]
    [DefaultValue("")]
    [Category("Appearance")]
    public string EditButtonToolTipText
    {
      get { return _TheEditButton.ToolTipText; }
      set
      {
        string s = value;
        //if (!String.IsNullOrEmpty(value))
        //  s += " (" + EFPCommandItem.GetShortCutText(Keys.F2) + ")";
        _TheEditButton.ToolTipText = s;
      }
    }

    #endregion

    #region EditClick

    [Description("Вызывается при нажатии на кнопку \"^\" или клавиши F2 при EditButton=True")]
    [Category("Action")]
    public event EventHandler EditClick;

    /// <summary>
    /// Вызывает обработчик события EditClick
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnEditClick(EventArgs args)
    {
      if (EditClick != null)
        EditClick(this, args);
    }

    /// <summary>
    /// Эмуляция нажатия на кнопку "^"
    /// </summary>
    public void PerformEdit()
    {
      if (_InsideButtonClick)
        return;
      if (_TheEditButton.Visible && _TheEditButton.Enabled)
      {
        _InsideButtonClick = true;
        try
        {
          OnEditClick(EventArgs.Empty);
        }
        finally
        {
          _InsideButtonClick = false;
        }
      }
    }

    private void TheEditButton_Click(object sender, EventArgs args)
    {
      PerformEdit();
    }

    /// <summary>
    /// Возвращает true, если установлен пользовательский обпвботчик события EditClick
    /// </summary>
    protected bool HasEditClickHandler { get { return EditClick != null; } }

    #endregion

    #endregion

    #region Ограничение размеров

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
      base.SetBoundsCore(x, y, width, MainControl.Height, specified);
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
      return new Size(proposedSize.Width, MainControl.Height);
    }

    protected override Size DefaultSize
    {
      get
      {
        // Метод может вызываться до вызова конструктора

        int h;
        if (MainControl == null)
          h = 20;
        else
          h = MainControl.Height;
        return new Size(150, h);
      }
    }

    #endregion

    #region Заглушки для свойств

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new BorderStyle BorderStyle { get { return base.BorderStyle; } set { base.BorderStyle = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new Image BackgroundImage { get { return base.BackgroundImage; } set { base.BackgroundImage = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new ImageLayout BackgroundImageLayout { get { return base.BackgroundImageLayout; } set { base.BackgroundImageLayout = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new AutoValidate AutoValidate { get { return base.AutoValidate; } set { base.AutoValidate = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool AutoScroll { get { return base.AutoScroll; } set { base.AutoScroll = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new Size AutoScrollMargin { get { return base.AutoScrollMargin; } set { base.AutoScrollMargin = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new Size AutoScrollMinSize { get { return base.AutoScrollMinSize; } set { base.AutoScrollMinSize = value; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new bool AutoSize { get { return base.AutoSize; } set { base.AutoSize = value; } }


    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Bindable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Browsable(false)]
    public new AutoSizeMode AutoSizeMode { get { return base.AutoSizeMode; } set { base.AutoSizeMode = value; } }
    /*
[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
[Bindable(false)]
[EditorBrowsable(EditorBrowsableState.Never)]
[Browsable(false)]
public new Color BackColor { get { return base.BackColor; } set { base.BackColor = value; } }
      */
    #endregion

    #region Обновление

    /// <summary>
    /// При явно вызванном методе Invalidate() надо обновить и дочерний элемент
    /// </summary>
    /// <param name="args">Аргументы события, где указана область для обновления</param>
    protected override void OnInvalidated(InvalidateEventArgs args)
    {
      base.OnInvalidated(args);
      MainControl.Invalidate(MainControl.RectangleToClient(this.RectangleToScreen(args.InvalidRect))); // 22.07.2019
    }

    #endregion

  }


  public class UserComboBoxDesigner : ControlDesigner
  {
    #region Изменение размеров

    /// <summary>
    /// Разрешено менять только горизонтальные размеры
    /// </summary>
    public override SelectionRules SelectionRules
    {
      get
      {
        SelectionRules Rules = base.SelectionRules;
        Rules = Rules & (~(System.Windows.Forms.Design.SelectionRules.BottomSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable));
        return Rules;
      }
    }

    #endregion

    #region Snap lines

    // Добавляем "сиреневую" линию базовой линии текста для дизайнера формы
    // Линия берется из основного элемента
    // Взято из 
    // http://stackoverflow.com/questions/93541/baseline-snaplines-in-custom-winforms-controls
    //

    public override IList SnapLines
    {
      get
      {
        /* Code from above */
        IList snapLines = base.SnapLines;

        // *** This will need to be modified to match your user control
        UserComboBoxBase control = Control as UserComboBoxBase;
        if (control == null)
          return snapLines;

        // *** This will need to be modified to match the item in your user control
        // This is the control in your UC that you want SnapLines for the entire UC
        IDesigner designer = TypeDescriptor.CreateDesigner(control.MainControl, typeof(IDesigner));
        if (designer == null)
          return snapLines;

        // *** This will need to be modified to match the item in your user control
        designer.Initialize(control.MainControl);

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
                 line.Offset + control.MainControl.Top, // всегда 0
                 line.Filter, line.Priority));
              break;
            }
          }
        }

        return snapLines;
      }
    }

    #endregion

    //public override void InitializeNewComponent(IDictionary defaultValues)
    //{
    //  defaultValues.Remove("Text");
    //  base.InitializeNewComponent(defaultValues);
    //}
  }

  /// <summary>
  /// Комбоблок для выбора значения с помощью произвольного обработчика,
  /// без возможности редактирования значения
  /// </summary>
  [DefaultEvent("PopupClick")]
  [Description("Комбоблок для выбора из списка с произвольной обработкой нажатия стрелочки")]
  [ToolboxBitmap(typeof(UserSelComboBox), "UserSelComboBox.bmp")]
  [ToolboxItem(true)]
  public class UserSelComboBox : UserComboBoxBase
  {
    #region Конструктор

    public UserSelComboBox()
      : base(CreateMainControl())
    {
      MainControl.MouseClick += MainControl_MouseClick;
      MainControl.MouseDoubleClick += MainControl_MouseDoubleClick;
      MainControl.MouseMove += MainControl_MouseMove;
      PopupButtonEnabledChanged += MainControl_PopupButtonEnabledChanged;

      InitTextBoxCursor();
      MainControl.BackColor = SystemColors.Window;
      MainControl.ForeColor = SystemColors.WindowText;
    }

    private static Control CreateMainControl()
    {
#if !XXXXX
      TextBox MainControl = new TextBox();
      MainControl.ReadOnly = true;
      return MainControl;
#else
        RichTextBox MainControl = new RichTextBox();
        MainControl.Multiline = false;
        MainControl.ScrollBars = RichTextBoxScrollBars.None;
        MainControl.ReadOnly = true;
        MainControl.MaximumSize = new Size(10000, 21);
        return MainControl;
#endif
    }

    #endregion

    #region Свойство Text

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string Text
    {
      get { return base.Text; }
      set { base.Text = value; }
    }

    #endregion

    #region Обработка нажатия кнопки мыши

    protected override void OnKeyDown(KeyEventArgs args)
    {
      base.OnKeyDown(args);
      KeyActivity = true;
    }

    protected override void OnEnter(EventArgs args)
    {
      base.OnEnter(args);
      KeyActivity = false;
    }

    protected override void OnLeave(EventArgs args)
    {
      base.OnLeave(args);
      KeyActivity = false;
    }

    protected override void OnPopupButtonChanged(EventArgs args)
    {
      base.OnPopupButtonChanged(args);
      InitTextBoxCursor();
    }

    void MainControl_PopupButtonEnabledChanged(object sender, EventArgs args)
    {
      if (PopupButtonEnabled)
      {
        MainControl.BackColor = SystemColors.Window;
        MainControl.ForeColor = SystemColors.WindowText;
      }
      else
      {
        MainControl.BackColor = SystemColors.Control;
        MainControl.ForeColor = SystemColors.ControlText;
      }

      InitTextBoxCursor();
    }

    /*
     * Когда пользователь нажимает левую кнопку мыши на тексте, в режиме ReadOnly=true,
     * PopupButton=true и PopupButtonEnabled=true, выполняются следующие действия:
     * - Если не было клавиатурной активности, то вызывается PopupClick
     * - Иначе не выполняется никаких специальных действий
     */

    private bool KeyActivity
    {
      get { return _KeyActivity; }
      set
      {
        if (value == _KeyActivity)
          return;
        _KeyActivity = value;
        InitTextBoxCursor();
      }
    }

    private bool _KeyActivity;

    /// <summary>
    /// Возвращает true, если нажатие левой кнопки мыши должно вызвать PopupClick
    /// </summary>
    private bool PopupOnClickWanted
    {
      get
      {
        if (!Enabled)
          return false;
        //   if (!MainControl.Visible)
        //     return false;
        if (!MainControl.Enabled)
          return false;

        //if (!ReadOnly)
        //  return false;

        if (!PopupButton)
          return false;
        if (!PopupButtonEnabled)
          return false;

        if (KeyActivity)
          return false;

        return true;
      }
    }

    private void InitTextBoxCursor()
    {
      if (PopupOnClickWanted)
        MainControl.Cursor = Cursors.Arrow;
      else
        MainControl.Cursor = Cursors.IBeam;
    }

    void MainControl_MouseMove(object sender, MouseEventArgs args)
    {
      // Надо вычислять, насколько переместилась мышь. Если чуть-чуть, то не считается
      // Лень
      //if (Args.Button == System.Windows.Forms.MouseButtons.Left)
      //  KeyActivity = true;
    }

    void MainControl_MouseClick(object sender, MouseEventArgs args)
    {
      if (args.Button != System.Windows.Forms.MouseButtons.Left)
      {
        // На самом деле так не бывает, т.к. событие MouseClick приходит
        // только от левой кнопки мыши

        KeyActivity = true;
        return;
      }

      if (PopupOnClickWanted)
      {
        MainControl.Select();
        PerformPopup();
      }
    }

    void MainControl_MouseDoubleClick(object sender, MouseEventArgs args)
    {
      if (args.Button != System.Windows.Forms.MouseButtons.Left)
        return;

      PerformPopup();
    }

    protected override void OnMouseUp(MouseEventArgs args)
    {
      if (args.Button != System.Windows.Forms.MouseButtons.Left)
        KeyActivity = true;

      base.OnMouseUp(args);
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс для комбоблока с пользовательским обработчиком,
  /// предназначенным для редактирования
  /// </summary>
  [ToolboxItem(false)]
  public class UserTextComboBoxBase : UserComboBoxBase
  {
    #region Конструктор

    public UserTextComboBoxBase(TextBoxBase MainControl)
      : base(MainControl)
    {
      _Text = String.Empty;
      MainControl.TextChanged += new EventHandler(TheTextBox_TextChanged);
      MainControl.ReadOnlyChanged += new EventHandler(TheTextBox_ReadOnlyChanged);

      // ReSharper disable once VirtualMemberCallInConstructor
      InitButtonsEnabled();
    }

    #endregion

    #region MainControl

    protected new TextBoxBase MainControl { get { return (TextBoxBase)(base.MainControl); } }

    #endregion

    #region Text

    [DefaultValue("")]
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    //[DispId(-517)]
    public override string Text
    {
      get
      {
        if (MainControl.IsDisposed)
          return _Text;
        else
          return MainControl.Text;
      }
      set
      {
        if (value == null)
          value = String.Empty;
        _Text = value;
        MainControl.Text = value;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Always)]
    [Browsable(true)]
    public new event EventHandler TextChanged
    {
      add { base.TextChanged += value; }
      remove { base.TextChanged -= value; }
    }

    /// <summary>
    /// Буферизация свойства Text для обхода ошибки в свойстве ReadOnly
    /// См. комментарий в ReadOnly
    /// </summary>
    private string _Text;

    public override void ResetText()
    {
      Text = String.Empty;
    }

    private void TheTextBox_TextChanged(object sender, EventArgs args)
    {
      _Text = MainControl.Text;
      OnTextChanged(args);
    }

    protected override void OnTextChanged(EventArgs args)
    {
      base.OnTextChanged(args);

      InitButtonsEnabled();
    }

    #endregion

    #region Выбранный текст

    [Browsable(false)]
    [DesignerSerializationVisibility(0)]
    public string SelectedText
    {
      get { return MainControl.SelectedText; }
      set { MainControl.SelectedText = value; }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(0)]
    public int SelectionStart
    {
      get { return MainControl.SelectionStart; }
      set { MainControl.SelectionStart = value; }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(0)]
    public int SelectionLength
    {
      get { return MainControl.SelectionLength; }
      set { MainControl.SelectionLength = value; }
    }

    public void Select(int Start, int Length)
    {
      MainControl.Select(Start, Length);
    }

    public void SelectAll()
    {
      MainControl.SelectAll();
    }

    #endregion

    #region Свойство Modified

    /// <summary>
    /// Gets or sets a value that indicates that the text box control has been modified
    /// by the user since the control was created or its contents were last set.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(0)]
    public bool Modified
    {
      get { return MainControl.Modified; }
      set { MainControl.Modified = value; }
    }

    /// <summary>
    /// Вызывается при изменении свойства Modified
    /// </summary>
    public event EventHandler ModifiedChanged
    {
      add { MainControl.ModifiedChanged += value; }
      remove { MainControl.ModifiedChanged -= value; }
    }

    #endregion

    #region Undo

    /// <summary>
    /// Возвращает true, если команда отмены поддерживается.
    /// Для MaskedTextBox возвращает false
    /// </summary>
    public bool UndoSupported
    {
      get
      {
        return !(MainControl is MaskedTextBox);
      }
    }

    /// <summary>
    /// Доступность команды "Отменить" в данный момент
    /// </summary>
    public bool CanUndo { get { return MainControl.CanUndo; } }

    /// <summary>
    /// Отмена последнего редактирования
    /// </summary>
    public void Undo() { MainControl.Undo(); }

    #endregion

    #region Буфер обмена

    /// <summary>
    /// Выполнение команды "Вырезать"
    /// </summary>
    public void Cut() { MainControl.Cut(); }

    /// <summary>
    /// Выполнение команды "Копировать"
    /// </summary>
    public void Copy() { MainControl.Copy(); }

    /// <summary>
    /// Выполнение команды "Вставить"
    /// </summary>
    public void Paste() { MainControl.Paste(); }

    #endregion

    #region ReadOnly

    /*
     * Есть интересный дефект у свойства TextBoxBase.ReadOnly
     * 1. Есть форма с закладками TabControl. 
     * 2. Элемент TextBox размещается на второй закладке
     * 3. Свойство TextBox.ReadOnly устанавливается в true
     * 4. Свойство TextBox.ReadOnly  сбрасывается обратно в false
     * 5. Устанавливается свойство TextBox.Text
     * 6. Форма показывается в модальном режиме.
     * 7. Переход на закладку №2 не выполнялся
     * 8. Форма закрывается. 
     * 9. Пока Form.Dispose() не вызван, TextBox.Text возвращает установленное п.5 значение
     * 10. Вызывается Form.Dispose() и, соответственно, TextBox.Dispose()
     * 11. Теперь TextBox.Text возвращает пустую строку - значение потерялось
     * 
     * - Если при просмотре перейти на вкладку, а затем - вернуться, дефект не проявляется
     * - Если пропустить п.4 (оставить ReadOnly=true), дефект не проявляется
     * 
     * Решение - буферизовать свойство Text
     * Наверное, есть способ лучше
     */


    [Description("Режим \"Только для просмотра\"")]
    [Category("Appearance")]
    [DefaultValue(false)]
    public bool ReadOnly
    {
      get { return MainControl.ReadOnly; }
      set { MainControl.ReadOnly = value; }
    }

    [Description("Изменилось свойство ReadOnly")]
    [Category("PropertyChanged")]
    public event EventHandler ReadOnlyChanged;

    protected virtual void OnReadOnlyChanged(EventArgs args)
    {
      if (ReadOnlyChanged != null)
        ReadOnlyChanged(this, args);

      InitButtonsEnabled();
    }

    private void TheTextBox_ReadOnlyChanged(object sender, EventArgs args)
    {
      OnReadOnlyChanged(args);
    }

    #endregion

    #region Кнопка очистки

    /// <summary>
    /// Если есть установленный обработчик свойства ClearClick, то он вызывается.
    /// Иначе нажатие кнопки приводит к очистке текста
    /// </summary>
    /// <param name="args"></param>
    protected override void OnClearClick(EventArgs args)
    {
      if (HasClearClickHandler)
        base.OnClearClick(args);
      else
        Text = String.Empty;
    }

    #endregion

    #region Управление видимостью кнопок

    /// <summary>
    /// Вызывается при каждом изменении текста и при установке свойства ReadOnly
    /// Метод должен установить свойства PopupButtonEnabled и ClearButtonEnabled 
    /// </summary>
    protected virtual void InitButtonsEnabled()
    {
      PopupButtonEnabled = !ReadOnly;
      ClearButtonEnabled = (!ReadOnly) && Text.Length > 0;
    }

    #endregion
  }


  /// <summary>
  /// Комбоблок с возможностью ввода текста и вызовом пользовательского кодового блока PopupClick
  /// </summary>
  [DefaultEvent("PopupClick")]
  [Description("Комбоблок для ввода текста с произвольной обработкой нажатия стрелочки")]
  [ToolboxBitmap(typeof(UserTextComboBox), "UserTextComboBox.bmp")]
  [ToolboxItem(true)]
  public class UserTextComboBox : UserTextComboBoxBase
  {
    #region Конструктор

    public UserTextComboBox()
      : base(new TextBox())
    {
    }

    #endregion

    #region Свойство MainControl

    protected new TextBox MainControl { get { return (TextBox)(base.MainControl); } }

    #endregion

    #region Свойство MaxLength

    [Description("Максимальная длина текста")]
    [Category("Behavior")]
    [DefaultValue(32767)]
    public int MaxLength
    {
      get { return MainControl.MaxLength; }
      set { MainControl.MaxLength = value; }
    }

    #endregion
  }

  /// <summary>
  /// Комбоблок с маской и с вызовом пользовательского кодового блока PopupClick
  /// </summary>
  [DefaultEvent("PopupClick")]
  [Description("Комбоблок для ввода текста с маской и произвольной обработкой нажатия стрелочки")]
  [ToolboxBitmap(typeof(UserMaskedComboBox), "UserMaskedComboBox.bmp")]
  [ToolboxItem(true)]
  public class UserMaskedComboBox : UserTextComboBoxBase
  {
    #region Конструктор

    public UserMaskedComboBox()
      : base(new MaskedTextBox())
    {
      MainControl.MaskChanged += new EventHandler(MainControl_MaskChanged);
    }

    #endregion

    #region Свойство TheTextBox

    protected new MaskedTextBox MainControl { get { return (MaskedTextBox)(base.MainControl); } }

    #endregion

    #region Mask

    [Description("Маска для редактирования текста")]
    [Category("Appearance")]
    [DefaultValue("")]
    public string Mask
    {
      get { return MainControl.Mask; }
      set { MainControl.Mask = value; }
    }

    [Description("Вызывается после изменения маски редактивования")]
    [Category("Property Changed")]
    public event EventHandler MaskChanged;

    protected virtual void OnMaskChanged(EventArgs args)
    {
      if (MaskChanged != null)
        MaskChanged(this, args);
    }

    void MainControl_MaskChanged(object sender, EventArgs args)
    {
      OnMaskChanged(args);
    }

    [RefreshProperties(RefreshProperties.Repaint)]
    public System.Globalization.CultureInfo Culture 
    {
      get { return MainControl.Culture; }
      set { MainControl.Culture = value; }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IFormatProvider FormatProvider
    {
      get { return MainControl.FormatProvider; }
      set { MainControl.FormatProvider = value; }
    }

    #endregion

    #region MaskCompeted и MaskTextProvider

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public bool MaskCompleted { get { return MainControl.MaskCompleted; } }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public MaskedTextProvider MaskedTextProvider
    {
      get { return MainControl.MaskedTextProvider; }
    }

    /// <summary>
    /// Возвращает количество заполненных позиций в маске
    /// Если маска не присоединена - возвращает число введенных символов (Text.Length)
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public int AssignedEditPositionCount
    {
      get
      {
        if (MainControl.MaskedTextProvider == null)
        {
          if (Text == null)
            return 0;
          else
            return Text.Length;
        }
        else
          return MainControl.MaskedTextProvider.AssignedEditPositionCount;
      }
    }

    #endregion

    #region Кнопка выбора

    protected override void InitButtonsEnabled()
    {
      PopupButtonEnabled = !ReadOnly;
      ClearButtonEnabled = (!ReadOnly) && AssignedEditPositionCount > 0;
    }

    #endregion
  }

  /// <summary>
  /// Комбоблок, основным управляющим элементом которого является табличный просмотр DataGridView, состоящий из одной ячейки
  /// </summary>
  [DefaultEvent("PopupClick")]
  [Description("Комбоблок с табличным просмотр DataGridView, состоящим из одной ячейки")]
  [ToolboxBitmap(typeof(UserSingleGridCellComboBox), "UserSingleGridCellComboBox.bmp")]
  [ToolboxItem(true)]
  public class UserSingleGridCellComboBox : UserComboBoxBase
  {
    #region Конструктор

    public UserSingleGridCellComboBox()
      :base(CreateMainControl())
    {
      MainControl.CellClick += MainControl_CellClick;
      MainControl.MouseClick += MainControl_MouseClick;
    }

    void MainControl_MouseClick(object sender, MouseEventArgs args)
    {
      OnMouseClick(args);
      base.PerformPopup();
    }

    void MainControl_CellClick(object sender, DataGridViewCellEventArgs args)
    {
      OnClick(EventArgs.Empty);
    }

    private static DataGridView CreateMainControl()
    {
      DataGridView Control = new DataGridView();
      Control.RowCount = 2;
      Control.ColumnCount = 1;
      Control.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      Control.ScrollBars = ScrollBars.None;
      Control.RowHeadersVisible = false;
      Control.ColumnHeadersVisible = false;
      Control.AllowUserToAddRows = false;
      Control.AllowUserToDeleteRows = false;
      Control.AllowUserToOrderColumns = false;
      Control.AllowUserToResizeColumns = false;
      Control.AllowUserToResizeRows = false;
      Control.ReadOnly = true;
      Control.Resize += Control_Resize;
      //Control.BorderStyle = BorderStyle.None;
      Control.CellBorderStyle = DataGridViewCellBorderStyle.None;
      Control.ShowCellToolTips = false;
      Control_Resize(Control, null);

      return Control;
    }

    static void Control_Resize(object sender, EventArgs args)
    {
      DataGridView Control = (DataGridView)sender;
      Control.Rows[0].Height = Control.ClientSize.Height - 2 * SystemInformation.BorderSize.Height;
    }

    #endregion

    #region Основной управляющий элемент

    /// <summary>
    /// Основной управляющий элемент
    /// </summary>
    public new DataGridView MainControl { get { return (DataGridView)(base.MainControl); } }

    protected override void OnEnabledChanged(EventArgs args)
    {
      base.OnEnabledChanged(args);
      MainControl.Enabled = Enabled;
    }

    #endregion
  }
}
