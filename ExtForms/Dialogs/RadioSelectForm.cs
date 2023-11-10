// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Форма для RadioSelectDialog
  /// </summary>
  internal sealed partial class RadioSelectForm : Form
  {
    public RadioSelectForm(string formTitle, string groupTitle, string[] items, string[] imageKeys)
    {
      InitializeComponent();
      EFPApp.InitFormImages(this);
      Text = formTitle;
      TheGroupBox.Text = groupTitle;

      if (formTitle == null)
        formTitle = string.Empty;
      if (groupTitle == null)
        groupTitle = String.Empty;

      int maxLen = Math.Max(formTitle.Length, groupTitle.Length);
      RadioButton[] buttons = new RadioButton[items.Length];

      int dy = 0; // расстояние между кнопками

      for (int i = 0; i < items.Length; i++)
      {
        RadioButton btn = new RadioButton();
        if (i == 0)
        {
          btn.TabStop = true;
          btn.TabIndex = 0;
        }
        btn.Text = items[i];
        btn.Location = new Point(imageKeys==null?8:40, (String.IsNullOrEmpty(groupTitle) ? 12 : 24) + dy * i);
        btn.AutoSize = true;
        // не работает
        //        btn.DoubleClick += new EventHandler(btn_DoubleClick);
        TheGroupBox.Controls.Add(btn);
        buttons[i] = btn;
        maxLen = Math.Max(maxLen, items[i].Length);

        if (imageKeys != null)
        {
          // У RadioButton есть свойство Image, но изображение располагается некрасиво

          string thisImageKey = imageKeys[i];
          if (String.IsNullOrEmpty(thisImageKey))
            thisImageKey = "EmptyImage";
          Label lbl = new Label();
          lbl.Text = String.Empty;
          lbl.Image = EFPApp.MainImages.Images[thisImageKey];
          lbl.Size = new Size(16, 16);
          int yOff = (btn.Height - lbl.Height) / 2; // 17.01.2019 - центрируем от кнопок
          lbl.Location = new Point(8, btn.Location.Y+yOff);
          TheGroupBox.Controls.Add(lbl);
        }



        if (dy == 0) // вычисляем зазор между кнопками
          //DY = Math.Max(18/* значок*/, btn.Height + SystemInformation.SizingBorderWidth);
          dy = Math.Max(18/* значок*/, (btn.Height *3)/2);
      }
      //int H = 70 + Items.Length * DY;
      //int W = 8 * MaxLen + 120;
      //if (ImageKeys != null)
      //  W += 32;
      //this.ClientSize = new Size(Math.Max(W, this.ClientSize.Width), Math.Max(H, this.ClientSize.Height));

      efpForm = new EFPFormProvider(this);
      efpRadio = new EFPRadioButtons(efpForm, buttons);
    }

    // не работает
    ///// <summary>
    ///// Двойной щелчок на радиокнопке нажимает кнопку "ОК"
    ///// </summary>
    //void btn_DoubleClick(object Sender, EventArgs Args)
    //{
    //  btnOk.PerformClick();
    //}

    public EFPFormProvider efpForm;

    public EFPRadioButtons efpRadio;

    #region Подбор размера формы

    protected override void OnLoad(EventArgs args)
    {
      base.OnLoad(args);
      PerformAutoScale();

      Size sz = WinFormsTools.GetControlExcess(TheGroupBox);
      if (!sz.IsEmpty)
      {
        this.Size += sz;
        switch (StartPosition)
        {
          case FormStartPosition.CenterScreen:
            WinFormsTools.PlaceFormInScreenCenter(this, true); // для красоты
            break;
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Диалог выбора позиции с помощью группы радиокнопок
  /// </summary>
  public class RadioSelectDialog
  {
    /// <summary>
    /// Создает объект диалога
    /// </summary>
    #region Конструктор

    public RadioSelectDialog()
    {
      _Title = "Выбор из списка";
      _SelectedIndex = 0;
      _DialogPosition = new EFPDialogPosition();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список для выбора.
    /// Строки могут содержать символ "амперсанд" для подчеркивания буквы
    /// </summary>
    public string[] Items
    {
      get { return _Items; }
      set { _Items = value; }
    }
    private string[] _Items;

    /// <summary>
    /// Текущая позиция
    /// </summary>
    public int SelectedIndex
    {
      get { return _SelectedIndex; }
      set { _SelectedIndex = value; }
    }
    private int _SelectedIndex;

    /// <summary>
    /// Установка и получение выбранной позиции как строки.
    /// Выполняет поиск в списке Items
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
          throw new NullReferenceException("Свойство Items должно быть установлено до установки текущей позиции");
        SelectedIndex = Array.IndexOf<string>(Items, value);
      }
    }

    /// <summary>
    /// Заголовок формы
    /// </summary>
    public string Title
    {
      get { return _Title; }
      set { _Title = value; }
    }
    private string _Title;

    /// <summary>
    /// Заголовок над кнопками
    /// </summary>
    public string GroupTitle
    {
      get { return _GroupTitle; }
      set { _GroupTitle = value; }
    }
    private string _GroupTitle;

    /// <summary>
    /// Изображение для значка формы, извлекаемое из коллекции EFPApp.MainImages.
    /// По умолчанию - нет значка
    /// </summary>
    public string ImageKey
    {
      get { return _ImageKey; }
      set { _ImageKey = value; }
    }
    private string _ImageKey;

    /// <summary>
    /// Массив флажков доступных позиций.
    /// По умолчанию, все позиции доступны.
    /// Свойство доступно после установки свойства Items
    /// </summary>
    public bool[] EnabledItemFlags
    {
      get
      {
        if (_EnabledItemFlags == null)
        {
          if (_Items == null)
            return null;
          _EnabledItemFlags = new bool[_Items.Length];
          DataTools.FillArray<bool>(_EnabledItemFlags, true);
        }
        return _EnabledItemFlags;
      }
      set
      {
        _EnabledItemFlags = value;
      }
    }
    private bool[] _EnabledItemFlags;

    /// <summary>
    /// Значки для отдельных элементов.
    /// По умолчанию массив содержит пустые значения. При  этом значки не выводятся
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
          DataTools.FillArray<string>(_ImageKeys, String.Empty);
        }
        return _ImageKeys;
      }
      set
      {
        _ImageKeys = value;
      }
    }
    private string[] _ImageKeys;

    /// <summary>
    /// Контекст справки, вызываемой по F1
    /// </summary>
    public string HelpContext { get { return _HelpContext; } set { _HelpContext = value; } }
    private string _HelpContext;

    /// <summary>
    /// Позиция блока диалога на экране.
    /// По умолчанию блок диалога центрируется относительно EFPApp.DefaultScreen.
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
      if (Items == null)
        throw new NullReferenceException("Свойство Items не было установлено");

      #region Нужны ли картинки у кнопок?

      string[] imageKeys2 = null;
      if (_ImageKeys != null && EFPApp.ShowListImages /* 20.05.2021 */)
      { 
        if (_ImageKeys.Length!=Items.Length)
          throw new InvalidOperationException("Длина массива ImageKeys ("+_ImageKeys.Length.ToString()+") не совпадает с длиной массива Items ("+Items.Length.ToString()+")");
        for (int i = 0; i < Items.Length; i++)
        {
          if (!String.IsNullOrEmpty(_ImageKeys[i]))
          {
            imageKeys2 = _ImageKeys;
            break;
          }
        }
      }

      #endregion

      if (_EnabledItemFlags != null)
      {
        if (_EnabledItemFlags.Length != Items.Length)
          throw new InvalidOperationException("Длина массива EnabledItemFlags  (" + _EnabledItemFlags.Length.ToString() + ") не совпадает с длиной массива Items (" + Items.Length.ToString() + ")");
      }

      RadioSelectForm frm = new RadioSelectForm(Title, GroupTitle, Items, imageKeys2);
      EFPApp.MainImages.Icons.InitForm(frm, ImageKey, true);

      if (_EnabledItemFlags != null)
      {
        for (int i = 0; i < _Items.Length; i++)
          frm.efpRadio[i].Enabled = _EnabledItemFlags[i];
      }
      frm.efpRadio.SelectedIndex = SelectedIndex;
      frm.efpRadio.SelectNextIfNeeded();


      if (!String.IsNullOrEmpty(HelpContext))
        frm.efpForm.HelpContext = HelpContext;

      if (EFPApp.ShowDialog(frm, true, DialogPosition) != DialogResult.OK)
        return DialogResult.Cancel;

      SelectedIndex = frm.efpRadio.SelectedIndex;

      return DialogResult.OK;
    }


    #endregion
  }

}
