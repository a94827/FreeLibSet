using FreeLibSet.DependedValues;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using FreeLibSet.Controls;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.UICore;

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

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Провайдер для UserComboBox, предназначенного для выбора значения с помощью кнопки выбора
  /// Элемент всегда имеет свойство ReadOnly=true
  /// </summary>
  public class EFPUserSelComboBox : EFPTextViewControl<UserSelComboBox>, IDepSyncObject
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPUserSelComboBox(EFPBaseProvider baseProvider, UserSelComboBox control)
      : base(baseProvider, control, true)
    {
      _SyncGroup = null;
      _SyncMaster = false;

      _Selectable = true;
      ClearButtonEnabled = true;
    }

    #endregion

    #region Переопределенные методы

    /*
     * Так не работает, т.к. TextBox находится в режиме ReadOnly
     * 
    protected override void InitControlColors()
    {
      if (Selectable)
        base.InitControlColors();
      else
        Control.ForeColor = SystemColors.GrayText;
    }

     * */
    #endregion

    #region Свойство Selectable

#if XXX
    /// <summary>
    /// true - если комбоблок позволяет выбрать значение,
    /// false - комбоблок предназначен только для просмотра
    /// Свойство совпадает с UserSelComboBox.PopupButtonEnabled
    /// </summary>
    public bool Selectable
    {
      get { return Control.PopupButtonEnabled; }
      set
      {
        Control.PopupButtonEnabled = value;
        ClearButtonEnabled = FClearButtonEnabled;
        InitControlColors();
      }
    }
#endif

    /// <summary>
    /// true - если комбоблок позволяет выбрать значение,
    /// false - комбоблок предназначен только для просмотра
    /// </summary>
    public bool Selectable
    {
      get { return _Selectable; }
      set
      {
        if (value == _Selectable)
          return;
        _Selectable = value;
        Control.PopupButtonEnabled = value;
        //if (FSelectableMainEx != null)
        //  FSelectableMainEx.Value = value;
        // 21.01.2017 Исправлено, как EFPControlBase.EnabledEx
        if (_SelectableEx != null)
          _SelectableEx.Value = value;
        ClearButtonEnabled = _ClearButtonEnabled;
        UpdateEnabledState();
      }
    }
    private bool _Selectable;

    /// <summary>
    /// Управляемая версия свойства Selectable
    /// </summary>
    public DepValue<Boolean> SelectableEx
    {
      get
      {
        InitSelectableEx();
        return _SelectableEx;
      }
      set
      {
        InitSelectableEx();
        _SelectableMain.Source = value;
      }
    }

    private void InitSelectableEx()
    {
      if (_SelectableEx == null)
      {
        _SelectableEx = new DepInput<bool>(Selectable, SelectableEx_ValueChanged);
        _SelectableEx.OwnerInfo = new DepOwnerInfo(this, "SelectableEx");

        _SelectableMain = new DepInput<bool>(true, null);
        _SelectableMain.OwnerInfo = new DepOwnerInfo(this, "SelectableMain");

        _SelectableSync = new DepInput<bool>(true, null);
        _SelectableSync.OwnerInfo = new DepOwnerInfo(this, "SelectableSync");

        DepAnd SelectableAnd = new DepAnd(_SelectableMain, _SelectableSync);
        SelectableAnd.OwnerInfo = new DepOwnerInfo(this, "SelectableMain & SelectableSync");
        _SelectableEx.Source = SelectableAnd;
      }
    }

    /// <summary>
    /// Выходная часть SelectableEx
    /// </summary>
    private DepInput<Boolean> _SelectableEx;

    /// <summary>
    /// Основной вход для SelectableEx
    /// </summary>
    private DepInput<Boolean> _SelectableMain;
    /// <summary>
    /// Дополнительный вход для SelectableEx для выполнения синхронизации
    /// </summary>
    private DepInput<Boolean> _SelectableSync;


    void SelectableEx_ValueChanged(object sender, EventArgs args)
    {
      Selectable = _SelectableEx.Value;
    }

    /// <summary>
    /// Возвращает true, если установлены свойства Enabled=true и Selectable=true.
    /// </summary>
    public override bool EnabledState
    {
      get { return Enabled && Selectable; }
    }

    /// <summary>
    /// Блокировка при синхронизации выполняется не через свойство ReadOnly, как
    /// у других управляющих элементов, а через свойство Selectable
    /// </summary>
    /// <param name="value">True-выключить блокировку, false-включить</param>
    public override void SyncMasterState(bool value)
    {
      InitSelectableEx();
      _SelectableSync.Value = value;
    }

    #endregion

    #region ClearButtonEnabled

    /// <summary>
    /// Свойство разрешает кнопку "Очистить", только если
    /// Selectable=true
    /// </summary>
    public bool ClearButtonEnabled
    {
      get { return Control.ClearButtonEnabled; }
      set
      {
        _ClearButtonEnabled = value;
        Control.ClearButtonEnabled = value && Selectable;
      }
    }
    private bool _ClearButtonEnabled;

    #endregion

    #region Свойства IEFPSyncObject

    /// <summary>
    /// True, если объект является ведущим в группе синхронизации.
    /// False, если объект является ведомым и будет заблокированным
    /// </summary>
    public bool SyncMaster
    {
      get
      {
        return _SyncMaster;
      }
      set
      {
        if (value == _SyncMaster)
          return;
        _SyncMaster = value;
        if (_SyncGroup != null)
          _SyncGroup.ObjectSyncMasterChanged((IDepSyncObject)this);
      }
    }
    private bool _SyncMaster;

    /// <summary>
    /// Группа синхронизации, к которой в настоящий момент подключен управляющий
    /// элемент.
    /// Это свойство реализует часть интерфейса IEFPSyncObject и не должно 
    /// устанавливаться из прикладной программы.
    /// </summary>
    public DepSyncGroup SyncGroup
    {
      get { return _SyncGroup; }
      set { _SyncGroup = value; }
    }
    private DepSyncGroup _SyncGroup;

    /// <summary>
    /// Доступ к синхронизированному значению должен быть определен в классе-наследнике
    /// </summary>
    public virtual object SyncValue
    {
      get { return Text; }
      set
      {
        if (value == null)
          Text = String.Empty;
        else
          Text = value.ToString();
      }
    }

    #endregion

    #region Использование EFPComboBoxTextValueNeededEventArgs

    /// <summary>
    /// Инициализирует свойства UserSelComboBox.Text, ForeColor, Image, EFPControl.ValueToolTipText
    /// на основании аргументов EFPComboBoxTextValueNeededEventArgs.
    /// </summary>
    /// <param name="textValueNeededArgs">Заполненные аргументы события</param>
    protected void InitTextAndImage(EFPComboBoxTextValueNeededEventArgs textValueNeededArgs)
    {
      Control.Text = textValueNeededArgs.TextValue;
      if (textValueNeededArgs.Grayed)
        Control.ForeColor = SystemColors.GrayText;
      else
        // Control.ResetForeColor();
        InitControlColors(); // 21.06.2021. Терялся цвет подсветки ошибки/предупреждения

      if (EFPApp.ShowListImages)
      {
        if (String.IsNullOrEmpty(textValueNeededArgs.ImageKey))
          Control.Image = null;
        else
        {
          // стоит ли использовать свойство Selectable?
          //if (!Enabled)
          if (!Editable)
            Control.Image = System.Windows.Forms.ToolStripRenderer.CreateDisabledImage(EFPApp.MainImages.Images[textValueNeededArgs.ImageKey]); // 13.06.2019
          else
            Control.Image = EFPApp.MainImages.Images[textValueNeededArgs.ImageKey];
        }
      }
      if (EFPApp.ShowToolTips)
        ValueToolTipText = textValueNeededArgs.ToolTipText;
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Вызывается в обработчиках изменения редактируемого значения для передачи
    /// значения в группу синхронизации в режиме SyncMaster=true
    /// </summary>
    protected void DoSyncValueChanged()
    {
      if (SyncMaster && (SyncGroup != null))
      {
        object value = ((IDepSyncObject)this).SyncValue;
        SyncGroup.Value = value;
      }
    }

    #endregion
  }

  #region Делегаты

  /// <summary>
  /// Аргументы события EFPListMultiSelComboBox.TextValueNeeded
  /// </summary>
  public class EFPComboBoxTextValueNeededEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    public EFPComboBoxTextValueNeededEventArgs()
    {
      Clear();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если всплывающая подсказка используется
    /// </summary>
    public bool ToolTipTextNeeded { get { return EFPApp.ShowToolTips; } }

    /// <summary>
    /// Возвращает true, если изображение используется
    /// </summary>
    public bool ImageNeeded { get { return EFPApp.ShowListImages; } }

    /// <summary>
    /// Сюда может быть помещен текст для вывода в комбоблоке
    /// </summary>
    public string TextValue { get { return _TextValue; } set { _TextValue = value; } }
    private string _TextValue;

    /// <summary>
    /// Сюда может быть помещен текст всплывающей подсказки
    /// </summary>
    public string ToolTipText { get { return _ToolTipText; } set { _ToolTipText = value; } }
    private string _ToolTipText;

    /// <summary>
    /// Сюда может быть помещено изображение
    /// </summary>
    public string ImageKey { get { return _ImageKey; } set { _ImageKey = value; } }
    private string _ImageKey;

    /// <summary>
    /// Если свойство установлено в true, текст выводится бледным шрифтом
    /// </summary>
    public bool Grayed { get { return _Grayed; } set { _Grayed = value; } }
    private bool _Grayed;

    #endregion

    #region Методы

    /// <summary>
    /// Очищает все свойства
    /// </summary>
    public void Clear()
    {
      TextValue = String.Empty;
      ToolTipText = String.Empty;
      ImageKey = String.Empty;
      Grayed = false;
    }

    #endregion
  }

  /// <summary>
  /// Делегат события EFPListMultiSelComboBox.TextValueNeeded
  /// </summary>
  /// <param name="sender">Вызывающий объект EFPListMultiSelComboBox</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPComboBoxTextValueNeededEventHandler(object sender,
    EFPComboBoxTextValueNeededEventArgs args);

  #endregion



  /// <summary>
  /// Комбоблок выбора одного или нескольких перечислимых значений.
  /// Выбор осуществляется с помощью ListSelectDialog
  /// </summary>
  public class EFPListMultiSelComboBox : EFPUserSelComboBox
  {
    #region Константы

    /// <summary>
    /// Текст комбоблока по умолчанию, когда не выбран ни один элемент
    /// </summary>
    public const string DefaultEmptyText = "[ нет ]";

    /// <summary>
    /// Текст комбоблока по умолчанию, когда выбраны все элементы
    /// </summary>
    public const string DefaultAllSelectedText = "[ все ]";

    #endregion

    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="items">Список элементов, откуда осуществляется выбор</param>
    public EFPListMultiSelComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, string[] items)
      : base(baseProvider, control)
    {
      if (items == null)
        throw new ArgumentNullException("items");
      _Items = items;
      _Selections = new SelList(this);
      control.ClearButton = false;
      _EmptyText = DefaultEmptyText;
      _AllSelectedText = DefaultAllSelectedText;
      _CanBeEmptyMode = UIValidateState.Error;
      _EmptyImageKey = "CheckListNone";
      _AllSelectedImageKey = "CheckListAll";
      _ImageKey = String.Empty;
      _TextValueNeededArgs = new EFPComboBoxTextValueNeededEventArgs();

      if (!DesignMode)
      {
        control.PopupClick += new EventHandler(Control_PopupClick);
        control.ClearClick += new EventHandler(Control_ClearClick);
      }
    }

    /// <summary>
    /// Выполняет начальную инициализацию текста и значка выбранных элементов
    /// </summary>
    protected override void OnCreated()
    {
      base.OnCreated();
      InitTextAndImage(true);
    }

    #endregion

    #region Свойства Для списка выбора

    /// <summary>
    /// Список для выбора.
    /// Задается в конструкторе
    /// </summary>
    public string[] Items { get { return _Items; } }
    private string[] _Items;


    /// <summary>
    /// Список строк для второго столбца.
    /// Если свойство равно null (по умолчанию), то второго столбца нет
    /// </summary>
    public string[] SubItems
    {
      get { return _SubItems; }
      set
      {
        if (value != null)
        {
          if (value.Length != _Items.Length)
            throw new ArgumentException("Длина массива должна совпадать с Items");
        }
        _SubItems = value;
      }
    }
    private string[] _SubItems;

    #endregion

    #region Свойство CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - Error.
    /// Это свойство переопределяется для нестандартных элементов, содержащих
    /// кнопку очистки справа от элемента
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        if (value == _CanBeEmptyMode)
          return;
        _CanBeEmptyMode = value;
        Validate();
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// True, если ли элемент может содержать пустое значение.
    /// Дублирует CanBeEmptyMode
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Свойство EmptyText

    /// <summary>
    /// Текст, выводимый, когда нет ни одного выбранного элемента.
    /// По умолчанию - "[ нет ]"
    /// </summary>
    public string EmptyText
    {
      get { return _EmptyText; }
      set
      {
        if (value == _EmptyText)
          return;
        _EmptyText = value;
        if (_EmptyTextEx != null)
          _EmptyTextEx.Value = value;
        InitTextAndImage(false);
      }
    }
    private string _EmptyText;

    /// <summary>
    /// Текст, выводимый в строке комбоблока, когда нет выбранных элементов
    /// (по умолчанию "[ нет ]")
    /// </summary>
    public DepValue<String> EmptyTextEx
    {
      get
      {
        InitEmptyTextEx();
        return _EmptyTextEx;
      }
      set
      {
        InitEmptyTextEx();
        _EmptyTextEx.Source = value;
      }
    }

    private void InitEmptyTextEx()
    {
      if (_EmptyTextEx == null)
      {
        _EmptyTextEx = new DepInput<string>(EmptyText, EmptyTextEx_ValueChanged);
        _EmptyTextEx.OwnerInfo = new DepOwnerInfo(this, "EmptyTextEx");
      }
    }

    private DepInput<String> _EmptyTextEx;

    void EmptyTextEx_ValueChanged(object sender, EventArgs args)
    {
      EmptyText = _EmptyTextEx.Value;
    }

    #endregion

    #region Свойство EmptyImageKey

    /// <summary>
    /// Изображение, выводимое в строке комбоблока, когда нет выбранных элементов
    /// (по умолчанию "CheckListNone")
    /// </summary>
    public string EmptyImageKey
    {
      get { return _EmptyImageKey; }
      set
      {
        if (value == _EmptyImageKey)
          return;
        _EmptyImageKey = value;
        if (_EmptyImageKeyEx != null)
          _EmptyImageKeyEx.Value = value;
        InitTextAndImage(false);
      }
    }
    private string _EmptyImageKey;

    /// <summary>
    /// Изображение, выводимое в строке комбоблока, когда нет выбранных элементов
    /// (по умолчанию "CheckListNone")
    /// </summary>
    public DepValue<String> EmptyImageKeyEx
    {
      get
      {
        InitEmptyImageKeyEx();
        return _EmptyImageKeyEx;
      }
      set
      {
        InitEmptyImageKeyEx();
        _EmptyImageKeyEx.Source = value;
      }
    }

    private void InitEmptyImageKeyEx()
    {
      if (_EmptyImageKeyEx == null)
      {
        _EmptyImageKeyEx = new DepInput<string>(EmptyImageKey, EmptyImageKeyEx_ValueChanged);
        _EmptyImageKeyEx.OwnerInfo = new DepOwnerInfo(this, "EmptyImageKeyEx");
      }
    }

    private DepInput<String> _EmptyImageKeyEx;

    void EmptyImageKeyEx_ValueChanged(object sender, EventArgs args)
    {
      EmptyImageKey = _EmptyImageKeyEx.Value;
    }

    #endregion

    #region Свойство AllSelectedText

    /// <summary>
    /// Текст, выводимый в строке комбоблока, когда выбраны все элементы
    /// (по умолчанию "[ Все ]")
    /// </summary>
    public string AllSelectedText
    {
      get { return _AllSelectedText; }
      set
      {
        if (value == _AllSelectedText)
          return;
        _AllSelectedText = value;
        if (_AllSelectedTextEx != null)
          _AllSelectedTextEx.Value = value;
        InitTextAndImage(false);
      }
    }
    private string _AllSelectedText;

    /// <summary>
    /// Текст, выводимый в строке комбоблока, когда выбраны все элементы
    /// (по умолчанию "[ Все ]")
    /// </summary>
    public DepValue<String> AllSelectedTextEx
    {
      get
      {
        InitAllSelectedTextEx();
        return _AllSelectedTextEx;
      }
      set
      {
        InitAllSelectedTextEx();
        _AllSelectedTextEx.Source = value;
      }
    }

    private void InitAllSelectedTextEx()
    {
      if (_AllSelectedTextEx == null)
      {
        _AllSelectedTextEx = new DepInput<string>(EmptyText, AllSelectedTextEx_ValueChanged);
        _AllSelectedTextEx.OwnerInfo = new DepOwnerInfo(this, "AllSelectedTextEx");
      }
    }

    private DepInput<String> _AllSelectedTextEx;

    void AllSelectedTextEx_ValueChanged(object sender, EventArgs args)
    {
      AllSelectedText = _AllSelectedTextEx.Value;
    }

    #endregion

    #region Свойство AllSelectedImageKey

    /// <summary>
    /// Изображение, выводимое, когда в списке отмечены все элементы.
    /// По умолчанию - "CheckListAll"
    /// </summary>
    public string AllSelectedImageKey
    {
      get { return _AllSelectedImageKey; }
      set
      {
        if (value == _AllSelectedImageKey)
          return;
        _AllSelectedImageKey = value;
        if (_AllSelectedImageKeyEx != null)
          _AllSelectedImageKeyEx.Value = value;
        InitTextAndImage(false);
      }
    }
    private string _AllSelectedImageKey;

    /// <summary>
    /// Изображение, выводимое в строке комбоблока, когда выбраны все элементы
    /// </summary>
    public DepValue<String> AllSelectedImageKeyEx
    {
      get
      {
        InitAllSelectedImageKeyEx();
        return _AllSelectedImageKeyEx;
      }
      set
      {
        InitAllSelectedImageKeyEx();
        _AllSelectedImageKeyEx.Source = value;
      }
    }

    private void InitAllSelectedImageKeyEx()
    {
      if (_AllSelectedImageKeyEx == null)
      {
        _AllSelectedImageKeyEx = new DepInput<string>(AllSelectedImageKey, AllSelectedImageKeyEx_ValueChanged);
        _AllSelectedImageKeyEx.OwnerInfo = new DepOwnerInfo(this, "AllSelectedImageKeyEx");
      }
    }

    private DepInput<String> _AllSelectedImageKeyEx;

    void AllSelectedImageKeyEx_ValueChanged(object sender, EventArgs args)
    {
      AllSelectedImageKey = _AllSelectedImageKeyEx.Value;
    }

    #endregion

    #region Свойства ImageKey и ImageKeys

    /// <summary>
    /// "Основное" изображения для элементов.
    /// Используется: 
    ///   1-в качестве значка диалога выбора элементов;
    ///   2-в строке комбоблока, когда выбрано больше одного элемента, но не все элементы.
    ///   3-в качестве значка элемента, когда ImageKeys не задано
    /// По умолчанию - пустая строка - нет изображения.
    /// </summary>
    public string ImageKey
    {
      get { return _ImageKey; }
      set
      {
        _ImageKey = value;
        InitTextAndImage(false);
      }
    }
    private string _ImageKey;

    /// <summary>
    /// Массив изображений элементов.
    /// Используется:
    ///   1-в строке комбоблока, когда выбран один элемент
    ///   2-в списке выбора элементов
    /// По умолчанию - null - нет индивидуальных изображений. Для всех элементов используется 
    /// одинаковый значок, задаваемый свойством ImageKey.
    /// </summary>
    public string[] ImageKeys
    {
      get { return _ImageKeys; }
      set
      {
        if (value != null)
        {
          if (value.Length != _Items.Length)
            throw new ArgumentException("Длина массива должна совпадать с Items");
        }
        _ImageKeys = value;
        InitTextAndImage(false);
      }
    }
    private string[] _ImageKeys;

    #endregion

    #region Свойство Selections

    private class SelList : SelectionFlagList
    {
      #region Защищенный конструктор

      internal SelList(EFPListMultiSelComboBox owner)
        : base(owner.Items.Length)
      {
        _Owner = owner;
      }

      private EFPListMultiSelComboBox _Owner;

      #endregion

      #region OnChanged

      protected override void OnChanged()
      {
        base.OnChanged();
        _Owner.OnSelectionChanged();
      }

      #endregion
    }

    /// <summary>
    /// Флажки выбора.
    /// </summary>
    public SelectionFlagList Selections { get { return _Selections; } }
    private SelList _Selections;

    internal void OnSelectionChanged()
    {
      Validate();
      InitTextAndImage(false);
    }

    #endregion

    #region Проверка

    /// <summary>
    /// Проверка выбранных значений
    /// </summary>
    protected override void OnValidate()
    {
      if (Selections.AreAllUnselected)
      {
        switch (CanBeEmptyMode)
        {
          case UIValidateState.Error:
            SetError("Хотя бы одно значение должно быть выбрано");
            break;
          case UIValidateState.Warning:
            SetWarning("Хотя бы одно значение должно быть выбрано");
            break;
        }
        Control.ClearButtonEnabled = false;
      }
      else
        Control.ClearButtonEnabled = true;

      base.OnValidate();
    }

    #endregion

    #region Обработчики кнопок

    void Control_PopupClick(object sender, EventArgs args)
    {
      ListSelectDialog dlg = new ListSelectDialog();
      dlg.Title = DisplayName;
      dlg.Items = Items;
      dlg.SubItems = SubItems;
      dlg.MultiSelect = true;
      dlg.Selections = Selections.ToArray();
      dlg.ImageKey = ImageKey;
      dlg.ImageKeys = ImageKeys;
      dlg.CanBeEmpty = CanBeEmpty;
      dlg.ConfigSectionName = this.ConfigSectionName;
      dlg.DialogPosition.PopupOwnerControl = Control;
      if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        return;
      Selections.FromArray(dlg.Selections);
    }

    void Control_ClearClick(object sender, EventArgs args)
    {
      Selections.UnselectAll();
    }

    #endregion

    #region Текст, подсказка и значок

    private EFPComboBoxTextValueNeededEventArgs _TextValueNeededArgs; // чтобы не создавать каждый раз

    /// <summary>
    /// Событие вызывается при текстового поля.
    /// Обработчик может изменить выводимый текст и изображение.
    /// Обычно обработчик не нужен, достаточно использования свойств, задающих текст и изображение.
    /// </summary>
    public event EFPComboBoxTextValueNeededEventHandler TextValueNeeded;

    private void InitTextAndImage(bool insideOnShown)
    {
      if (!(HasBeenCreated || insideOnShown))
        return;

      _TextValueNeededArgs.Clear();
      if (Selections.AreAllUnselected)
      {
        _TextValueNeededArgs.TextValue = EmptyText;
        _TextValueNeededArgs.ImageKey = EmptyImageKey;
      }
      else if (Selections.AreAllSelected)
      {
        _TextValueNeededArgs.TextValue = AllSelectedText;
        _TextValueNeededArgs.ImageKey = AllSelectedImageKey;
      }
      else
      {
        StringBuilder sb = new StringBuilder();
        int SingleSelIndex = -1;
        int SelCnt = 0;
        for (int i = 0; i < Items.Length; i++)
        {
          if (Selections[i])
          {
            if (sb.Length > 0)
              sb.Append(", ");
            sb.Append(Items[i]);
            SingleSelIndex = i;
            SelCnt++;
          }
        }
        _TextValueNeededArgs.TextValue = sb.ToString();

        if (SelCnt == 1)
        {
          if (ImageKeys != null)
          {
#if DEBUG
            if (SingleSelIndex < 0 || SingleSelIndex >= ImageKeys.Length)
              throw new BugException("Неправильный SingleSelIndex=" + SingleSelIndex.ToString());
#endif
            _TextValueNeededArgs.ImageKey = ImageKeys[SingleSelIndex];
          }
          else
            _TextValueNeededArgs.ImageKey = ImageKey;
        }
        else
          _TextValueNeededArgs.ImageKey = ImageKey;
      }

      OnTextValueNeeded(_TextValueNeededArgs);

      // Устанавливаем значения. Изображение используется отдельно
      base.InitTextAndImage(_TextValueNeededArgs);
    }

    /// <summary>
    /// Вызывает событие TextValueNeeded
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnTextValueNeeded(EFPComboBoxTextValueNeededEventArgs args)
    {
      if (TextValueNeeded != null)
        TextValueNeeded(this, args);
    }

    #endregion

    #region Подстановочные значения

    /// <summary>
    /// Необязательный список подстановочных значений, соответствующих элементам списка
    /// </summary>
    public string[] Codes
    {
      get { return _Codes; }
      set
      {
#if DEBUG
        if (value != null)
        {
          if (value.Length != _Items.Length)
            throw new ArgumentException("Длина массива должна совпадать с Items");
          foreach (string s in value)
          {
            if (s == null)
              throw new ArgumentException("Значения null не допускаются. Используйте String.Empty");
          }
        }
#endif
        _Codes = value;
        if (_SelectedCodesEx != null)
          _SelectedCodesEx.OwnerSetValue(SelectedCodes);
      }
    }
    private string[] _Codes;


    /// <summary>
    /// Текущие выбранные позиции в виде массива кодов из массива Codes. 
    /// Если нет выбранных кодов, возвращается пустой массив.
    /// Если при присвоении значения заданые несуществующие коды, они игнорируются
    /// </summary>
    public string[] SelectedCodes
    {
      get
      {
        if (Codes == null)
          return DataTools.EmptyStrings;

        int[] a1 = Selections.SelectedIndices;
        string[] a2 = new string[a1.Length];
        for (int i = 0; i < a1.Length; i++)
          a2[i] = Codes[a1[i]];
        return a2;
      }
      set
      {
        if (Codes == null)
          return;
        if (value == null)
          value = DataTools.EmptyStrings;
        int[] a1 = new int[value.Length];
        for (int i = 0; i < value.Length; i++)
          a1[i] = Array.IndexOf<string>(Codes, value[i]);
        Selections.SelectedIndices = a1;
      }
    }

    /// <summary>
    /// Текущие выбранные позиции в виде массива кодов из массива Codes. 
    /// Если нет выбранных кодов, возвращается пустой массив.
    /// </summary>
    public DepValue<string[]> SelectedCodesEx
    {
      get
      {
        InitSelectedCodesEx();
        return _SelectedCodesEx;
      }
      set
      {
        InitSelectedCodesEx();
        _SelectedCodesEx.Source = value;
      }
    }
    private DepInput<string[]> _SelectedCodesEx;

    private void InitSelectedCodesEx()
    {
      if (_SelectedCodesEx == null)
      {
        _SelectedCodesEx = new DepInput<string[]>(SelectedCodes, SelectedCodesEx_ValueChanged);
        _SelectedCodesEx.OwnerInfo = new DepOwnerInfo(this, "SelectedCodesEx");
      }
    }

    /// <summary>
    /// Вызывается, когда снаружи устанавливается значение SelectedCodeEx.ValueEx
    /// </summary>
    void SelectedCodesEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedCodes = _SelectedCodesEx.Value;
    }


    /// <summary>
    /// Свойство SelectedCodes в виде списка кодов, разделенных запятыми
    /// </summary>
    public string SelectedCodesCSV
    {
      get
      {
        return String.Join(",", SelectedCodes);
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          Selections.UnselectAll();
        else
          SelectedCodes = value.Split(',');
      }
    }

    /// <summary>
    /// Свойство SelectedCodesEx в виде списка кодов, разделенных запятыми
    /// </summary>
    public DepValue<string> SelectedCodesCSVEx
    {
      get
      {
        InitSelectedCodesCSVEx();
        return _SelectedCodesCSVEx;
      }
      set
      {
        InitSelectedCodesCSVEx();
        _SelectedCodesCSVEx.Source = value;
      }
    }

    private void InitSelectedCodesCSVEx()
    {
      if (_SelectedCodesCSVEx == null)
      {
        _SelectedCodesCSVEx = new DepInput<string>(SelectedCodesCSV, SelectedCodesCSVEx_ValueChanged);
        _SelectedCodesCSVEx.OwnerInfo = new DepOwnerInfo(this, "SelectedCodesCSVEx");
      }
    }

    private DepInput<string> _SelectedCodesCSVEx;

    void SelectedCodesCSVEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedCodesCSV = _SelectedCodesCSVEx.Value;
    }

    #endregion
  }

  /// <summary>
  /// Провайдер для UserComboBox, предназначенного для ввода текста.
  /// Расширяет EFPUserComboBox, синхронизируя разрешение для кнопки выбора из списка
  /// со свойством ReadOnly
  /// </summary>
  public class EFPUserTextComboBox : EFPTextBoxControlWithReadOnly<UserTextComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPUserTextComboBox(EFPBaseProvider baseProvider, UserTextComboBox control)
      : base(baseProvider, control)
    {
      control.ClearButtonToolTipText = "Очистить введенное значение";
    }

    #endregion

    #region Переопределенные методы и свойства


    /// <summary>
    /// Установка кнопки очистки
    /// </summary>
    public override UIValidateState CanBeEmptyMode
    {
      get { return base.CanBeEmptyMode; }
      set
      {
        base.CanBeEmptyMode = value;
        Control.ClearButton = CanBeEmpty;
      }
    }


    /// <summary>
    /// Дублирует соответствующее свойство управляющего элемента UserTextComboBox
    /// </summary>
    protected override bool ControlReadOnly
    {
      get { return Control.ReadOnly; }
      set { Control.ReadOnly = value; }
    }

    /// <summary>
    /// Дублирует соответствующее свойство управляющего элемента UserTextComboBox
    /// </summary>
    protected override int ControlMaxLength
    {
      get { return Control.MaxLength; }
      set { Control.MaxLength = value; }
    }

    #endregion

    #region Свойства для выделения текста

    /// <summary>
    /// Дублирует соответствующее свойство управляющего элемента UserTextComboBox
    /// </summary>
    public override int SelectionStart
    {
      get { return Control.SelectionStart; }
      set { Control.SelectionStart = value; }
    }

    /// <summary>
    /// Дублирует соответствующее свойство управляющего элемента UserTextComboBox
    /// </summary>
    public override int SelectionLength
    {
      get { return Control.SelectionLength; }
      set { Control.SelectionLength = value; }
    }

    /// <summary>
    /// Дублирует соответствующее свойство управляющего элемента UserTextComboBox
    /// </summary>
    public override string SelectedText
    {
      get { return Control.SelectedText; }
      set { Control.SelectedText = value; }
    }

    /// <summary>
    /// Дублирует соответствующий метод управляющего элемента UserTextComboBox
    /// </summary>
    ///<param name="start">Начальная позиция</param>
    ///<param name="length">Длина выделения</param>
    public override void Select(int start, int length)
    {
      Control.Select(start, length);
    }

    /// <summary>
    /// Дублирует соответствующий метод управляющего элемента UserTextComboBox
    /// </summary>
    public override void SelectAll()
    {
      Control.SelectAll();
    }

    #endregion
  }

  #region Делегат

  /// <summary>
  /// Аргументы события EFPCsvCodesComboBoxBase.CodeValidating
  /// </summary>
  public class EFPCodeValidatingEventArgs : EventArgs, IUIValidableObject
  {
    #region Конструктор

    /// <summary>
    /// Инициализация объекта
    /// </summary>
    /// <param name="code">Проверяемый код. Не может быть пустым</param>
    public void Init(string code)
    {
      if (String.IsNullOrEmpty(code))
        throw new ArgumentNullException("code");

      _Code = code;
      _ValidateState = UIValidateState.Ok;
      _Message = String.Empty;
      _Name = String.Empty;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Проверяемый код.
    /// Не может быть пустым.
    /// </summary>
    public string Code { get { return _Code; } }
    private string _Code;

    /// <summary>
    /// Сюда можно поместить описание для кода
    /// </summary>
    public string Name { get { return _Name; } set { _Name = value; } }
    private string _Name;

    /// <summary>
    /// Сообщение об ошибке или предупреждение
    /// </summary>
    public string Message { get { return _Message; } }
    private string _Message;

    #endregion

    #region IUIValidableObject

    /// <summary>
    /// Задать сообщение об ошибке
    /// </summary>
    /// <param name="message">Сообщение</param>
    public void SetError(string message)
    {
      if (message == null)
        throw new ArgumentNullException("message");

      if (_ValidateState == UIValidateState.Error)
        return;

      _ValidateState = UIValidateState.Error;
      _Message = message;
    }

    /// <summary>
    /// Задать предупреждение
    /// </summary>
    /// <param name="message">Сообщение</param>
    public void SetWarning(string message)
    {
      if (message == null)
        throw new ArgumentNullException("message");

      if (_ValidateState != UIValidateState.Ok)
        return;

      _ValidateState = UIValidateState.Warning;
      _Message = message;
    }

    /// <summary>
    /// Текущее состояние
    /// </summary>
    public UIValidateState ValidateState { get { return _ValidateState; } }
    private UIValidateState _ValidateState;

    #endregion
  }

  /// <summary>
  /// Делегат события EFPCsvCodesComboBoxBase.CodeValidating
  /// </summary>
  /// <param name="sender">Провайдер управляющего элемента</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPCodeValidatingEventHandler(object sender, EFPCodeValidatingEventArgs args);

  #endregion

  /// <summary>
  /// Комбоблок для выбора одного или нескольких кодов, разделенных запятыми.
  /// Базовый класс, который не имеет встроенного списка кодов.
  /// </summary>
  public class EFPCsvCodesComboBoxBase : EFPUserTextComboBox
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPCsvCodesComboBoxBase(EFPBaseProvider baseProvider, UserTextComboBox control)
      : base(baseProvider, control)
    {
      _UseSpace = true;
      control.PopupButtonToolTipText = "Выбрать коды из списка";
      control.ClearButtonToolTipText = "Очистить выбранные коды";

      _CodeValidatingEventArgs = new EFPCodeValidatingEventArgs();
    }

    #endregion

    #region Список кодов

    /// <summary>
    /// Список выбранных кодов
    /// </summary>
    public string[] SelectedCodes
    {
      get
      {
        if (String.IsNullOrEmpty(base.Text))
          return DataTools.EmptyStrings;

        string[] a = base.Text.Split(',');
        List<string> lst = new List<string>(a.Length);
        for (int i = 0; i < a.Length; i++)
        {
          string s = a[i].Trim();
          if (s.Length > 0)
            lst.Add(s);
        }
        return lst.ToArray();
      }
      set
      {
        if (value == null)
          value = DataTools.EmptyStrings;
        base.Text = String.Join(_UseSpace ? ", " : ",", value);
      }
    }

    /// <summary>
    /// Управляемое значение для SelectedCodes.
    /// </summary>
    public DepValue<string[]> SelectedCodesEx
    {
      get
      {
        InitSelectedCodesEx();
        return _SelectedCodesEx;
      }
      set
      {
        InitSelectedCodesEx();
        _SelectedCodesEx.Source = value;
      }
    }
    private DepInput<string[]> _SelectedCodesEx;

    /// <summary>
    /// Возвращает true, если обработчик свойства SelectedCodesEx инициализирован.
    /// Это свойство не предназначено для использования в пользовательском коде.
    /// </summary>
    public bool HasSelectedCodesExProperty { get { return _SelectedCodesEx != null; } }

    private void InitSelectedCodesEx()
    {
      if (_SelectedCodesEx == null)
      {
        _SelectedCodesEx = new DepInput<string[]>(SelectedCodes, SelectedCodesEx_ValueChanged);
        _SelectedCodesEx.OwnerInfo = new DepOwnerInfo(this, "SelectedCodesEx");
      }
    }

    private void SelectedCodesEx_ValueChanged(object sender, EventArgs args)
    {
      if (!InsideTextChanged) // избегаем помех при вводе текста
        SelectedCodes = _SelectedCodesEx.Value;
    }

    #endregion

    #region Локальное меню

    /// <summary>
    /// Установка свойства EFPTextBoxCommandItems.UseConvert=false
    /// </summary>
    protected override void OnBeforePrepareCommandItems()
    {
      CommandItems.UseConvert = false;
      base.OnBeforePrepareCommandItems();
    }

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то между кодами после запятых будет добавляться по одному пробелу.
    /// Если false, то дополнительные пробелы не добавляются
    /// </summary>
    public bool UseSpace
    {
      get { return _UseSpace; }
      set
      {
        if (value == _UseSpace)
          return;
        _UseSpace = value;

        this.SelectedCodes = this.SelectedCodes; // корректировка пробелов
      }
    }
    private bool _UseSpace;

    #endregion

    #region Проверка элемента

    /// <summary>
    /// Обработка SelectedCodesEx.
    /// </summary>
    protected override void OnTextChanged()
    {
      base.OnTextChanged();

      if (_SelectedCodesEx != null)
        _SelectedCodesEx.Value = SelectedCodes;
    }

    private EFPCodeValidatingEventArgs _CodeValidatingEventArgs;

    /// <summary>
    /// Проверка корректности значения.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (base.ValidateState == UIValidateState.Error)
        return;

      if (String.IsNullOrEmpty(base.Text))
        return;

      string[] a = base.Text.Split(',');
      SingleScopeList<string> lst = new SingleScopeList<string>();

      ValueToolTipText = "Выбрано кодов: " + a.Length.ToString();
      for (int i = 0; i < a.Length; i++)
      {
        string s = a[i].Trim();
        if (s.Length == 0)
        {
          base.SetError("Задан пустой код");
          return;
        }

        _CodeValidatingEventArgs.Init(s);
        if (_ValidatingCodeEx != null)
          _ValidatingCodeEx.OwnerSetValue(s);
        OnCodeValidating(_CodeValidatingEventArgs);
        switch (_CodeValidatingEventArgs.ValidateState)
        { 
          case UIValidateState.Error:
            SetError("Неправильный код №" + (i + 1).ToString() + " \"" + s + "\". " + _CodeValidatingEventArgs.Message);
            return;
          case UIValidateState.Warning:
            SetWarning("Код №" + (i + 1).ToString() + " \"" + s + "\". " + _CodeValidatingEventArgs.Message);
            break;
        }
        if (a.Length == 1 && (!String.IsNullOrEmpty(_CodeValidatingEventArgs.Name)))
          ValueToolTipText = s + " - " + _CodeValidatingEventArgs.Name;

        if (lst.Contains(s))
        {
          SetError("Код \"" + s + "\" задан дважды");
          return;
        }
        lst.Add(s);
      }
    }

    #endregion

    #region Проверка одного кода

    /// <summary>
    /// Событие вызывается для проверки каждого кода в списке
    /// </summary>
    public event EFPCodeValidatingEventHandler CodeValidating;

    /// <summary>
    /// Управляемое свойство, возвращающее текущий проверяемый код в списке SelectedCodes.
    /// Используется в валидаторах из списка CodeValidators.
    /// Не используйте свойство в валидаторах основного списка Validators.
    /// В основном, предназначено для проверки в удаленном интерфейсе.
    /// В обычных приложениях удобнее использовать обработчик события CodeValidating.
    /// </summary>
    public DepValue<string> ValidatingCodeEx
    {
      get
      {
        if (_ValidatingCodeEx == null)
        {
          _ValidatingCodeEx = new DepOutput<string>(String.Empty);
          _ValidatingCodeEx.OwnerInfo = new DepOwnerInfo(this, "ValidatingCodeEx");
        }
        return _ValidatingCodeEx;
      }
    }
    private DepOutput<string> _ValidatingCodeEx;


    /// <summary>
    /// Список объектов-валидаторов для проверки корректности значения выбранных кодов.
    /// Используйте в качестве проверочного выражение какую-либо вычисляемую функцию, основанную на управляемом свойстве ValidatingCodeEx
    /// (и на других управляемых свойствах, в том числе, других элементов формы).
    /// В основном, предназначено для проверки в удаленном интерфейсе.
    /// В обычных приложениях удобнее использовать обработчик события CodeValidating.
    /// </summary>
    public UIValidatorList CodeValidators
    {
      get
      {
        if (_CodeValidators == null)
        {
          if (ProviderState!=EFPControlProviderState.Initialization)
            _CodeValidators = UIValidatorList.Empty;
          else
            _CodeValidators = new UIValidatorList();
        }
        return _CodeValidators;
      }
    }
    private UIValidatorList _CodeValidators;

    /// <summary>
    /// Возвращает true, если список CodeValidators не пустой.
    /// Используется для оптимизации, вместо обращения к CodeValidators.Count, позволяя обойтись без создания объекта списка, когда у управляющего элемента нет валидаторов.
    /// </summary>
    public bool HasCodeValidators
    {
      get
      {
        if (_CodeValidators == null)
          return false;
        else
          return _CodeValidators.Count > 0;
      }
    }

    /// <summary>
    /// Блокирует список CodeValidators от изменений.
    /// Присоединяет 
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();

      if (_CodeValidators!=null)
      {
        _CodeValidators.SetReadOnly();

        foreach (FreeLibSet.UICore.UIValidator v in this._CodeValidators)
        {
          v.ExpressionEx.ValueChanged += new EventHandler(this.Validate); // Не знаю, нужно ли
          if (v.PreconditionEx != null)
            v.PreconditionEx.ValueChanged += new EventHandler(this.Validate);
        }
      }
    }

    /// <summary>
    /// Проверка кода в списке.
    /// Непереопределенный метод сначала выполняет проверку с помощью валидаторов CodeValidators(), если они есть, 
    /// затем вызывает обработчик события CodeValidating, если он установлен.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnCodeValidating(EFPCodeValidatingEventArgs args)
    {
      if (_CodeValidators != null)
        _CodeValidators.Validate(args);

      if (CodeValidating != null)
        CodeValidating(this, args);
    }

    #endregion
  }

  /// <summary>
  /// Комбоблок для выбора одного или нескольких кодов, разделенных запятыми.
  /// Хранит список кодов, доступных для выбора, в виде массива.
  /// </summary>
  public class EFPCsvCodesComboBox : EFPCsvCodesComboBoxBase
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="codes">Массив кодов, из которых можно выбирать</param>
    public EFPCsvCodesComboBox(EFPBaseProvider baseProvider, UserTextComboBox control, string[] codes)
      : this(baseProvider, control, codes, null, false)
    {
    }

    /// <summary>
    /// Создает провайдер.
    /// Эта перегрузка позволяет сразу задать массив имен и выполнить проверку повторов
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="codes">Массив кодов, из которых можно выбирать</param>
    /// <param name="names">Массив описаний, соответствующий кодам <paramref name="codes"/>.
    /// Может быть null, если описания не используются.</param>
    /// <param name="checkRepeats">Если true, то в массиве <paramref name="codes"/> будет выполнен
    /// поиск совпадений. Повторяющиеся коды будут удалены, вместе с соответствующими им элементами <paramref name="names"/> (если массив задан).
    /// Повторы в массиве <paramref name="names"/> допускаются.
    /// Если false, то в массиве <paramref name="codes"/> не может быть повторений, иначе возникнет исключение.</param>
    public EFPCsvCodesComboBox(EFPBaseProvider baseProvider, UserTextComboBox control, string[] codes, string[] names, bool checkRepeats)
      : base(baseProvider, control)
    {
      if (codes == null)
        throw new ArgumentNullException("codes");
      if (names != null)
      {
        if (names.Length != codes.Length)
          throw new ArgumentException("Длина массивов codes (" + codes.Length.ToString() + ") и names (" + names.Length.ToString() + ") должна совпадать", "names");
      }

      if (checkRepeats)
        RemoveRepeats(ref codes, ref names);

      _Codes = codes;
      _CodeIndexer = new StringArrayIndexer(codes);
      _Names = names;
      _UnknownCodeSeverity = UIValidateState.Error;

      Control.PopupClick += new EventHandler(Control_PopupClick);
    }

    #endregion

    #region Поиск повторов

    /// <summary>
    /// Выполняет поиск повторов в массиве <paramref name="codes"/>.
    /// Обнаруженные повторые удаляются, как и из массива <paramref name="names"/>, если он задан.
    /// </summary>
    /// <param name="codes">Массив кодов</param>
    /// <param name="names">Массив значений. Может быть null</param>
    public static void RemoveRepeats(ref string[] codes, ref string[] names)
    {
      if (codes == null)
        throw new ArgumentNullException("codes");
      if (names != null)
      {
        if (names.Length != codes.Length)
          throw new ArgumentException("Длина массивов codes (" + codes.Length.ToString() + ") и names (" + names.Length.ToString() + ") должна совпадать", "names");
      }

      SingleScopeStringList lstCodes = new SingleScopeStringList(codes.Length, false);
      List<string> lstNames = null;
      bool pairFound = false;

      for (int i = 0; i < codes.Length; i++)
      {
        if (lstCodes.Contains(codes[i]))
        {
          // Нашли повтор
          if (!pairFound)
          {
            pairFound = true;
            if (names != null)
            {
              lstNames = new List<string>(codes.Length - 1);
              // добавляем пропущенные ранее записи в список значений
              for (int j = 0; j < i; j++)
                lstNames.Add(names[j]);
            }
          }
        }
        else
        {
          // не повтор
          lstCodes.Add(codes[i]);
          if (lstNames != null)
          {
#if DEBUG
            if (names == null)
              throw new BugException("names=null");
#endif
            lstNames.Add(names[i]);
          }
        }
      }

      if (pairFound)
      {
        // Были повторы. Создаем новые массивы
        codes = lstCodes.ToArray();
        if (lstNames != null)
        {
#if DEBUG
          if (lstNames.Count != lstCodes.Count)
            throw new BugException("Получена разная длина списков кодов и значений");
#endif
          names = lstNames.ToArray();
        }
      }
      // иначе ничего делать не надо, исходнные массивы корректны
    }

    #endregion

    #region Список доступных кодов

    /// <summary>
    /// Список доступных кодов. Задается в конструкторе
    /// </summary>
    public string[] Codes { get { return _Codes; } }
    private string[] _Codes;

    private StringArrayIndexer _CodeIndexer;

    /// <summary>
    /// Описания, соответствующие кодам Codes.
    /// По умолчанию null - нет описаний.
    /// </summary>
    public string[] Names
    {
      get { return _Names; }
      set
      {
        if (value != null)
        {
          if (value.Length != _Codes.Length)
            throw new ArgumentException("Неправильная длина массива: " + value.Length.ToString() + ". Ожидалось: " + _Codes.Length.ToString());
          _Names = value;
        }
      }
    }
    private string[] _Names;

    #endregion

    #region Проверка

    /// <summary>
    /// Нужно ли выдавать сообщение об ошибке или предупреждение, если кода нет в списке.
    /// По умолчанию - Error.
    /// </summary>
    public UIValidateState UnknownCodeSeverity
    {
      get { return _UnknownCodeSeverity; }
      set
      {
        if (value == _UnknownCodeSeverity)
          return;
        _UnknownCodeSeverity = value;
        Validate();
      }
    }
    private UIValidateState _UnknownCodeSeverity;

    /// <summary>
    /// Выполнить проверку одного кода.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnCodeValidating(EFPCodeValidatingEventArgs args)
    {
      base.OnCodeValidating(args);
      int p = _CodeIndexer.IndexOf(args.Code);
      if (p < 0)
      {
        switch (UnknownCodeSeverity)
        {
          case UIValidateState.Error:
            args.SetError("Кода нет в списке");
            break;
          case UIValidateState.Warning:
            args.SetWarning("Кода нет в списке");
            break;
        }
      }
      else
      {
        if (_Names != null)
          args.Name = _Names[p];
      }
    }

    #endregion

    #region Выбор из списка

    void Control_PopupClick(object sender, EventArgs args)
    {
      ListSelectDialog dlg = new ListSelectDialog();
      dlg.Items = _Codes;
      dlg.Title = DisplayName;
      dlg.MultiSelect = true;
      dlg.CanBeEmpty = this.CanBeEmpty;
      dlg.SubItems = _Names;
      dlg.ClipboardMode = ListSelectDialogClipboardMode.CommaCodes;
      dlg.SetSelectedItems(SelectedCodes);
      dlg.DialogPosition.PopupOwnerControl = Control;
      dlg.ConfigSectionName = this.ConfigSectionName;
      if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        return;
      SelectedCodes = dlg.GetSelectedItems();
    }

    #endregion
  }

  /// <summary>
  /// Провайдер комбоблока, который в качестве основного поля отображает единственную ячейку DataGridView.
  /// Используется для настроек форматирования ячеек в диалогах параметров.
  /// Изображение в основном поле комбоблока (ячейке таблицы) управляется пользовательским обработчиком события GetCellValues.
  /// Элемент является очень тяжелым и его не следует использовать без необходимости.
  /// </summary>
  public class EFPUserSingleGridCellComboBox : EFPControl<UserSingleGridCellComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Комбоблок</param>
    public EFPUserSingleGridCellComboBox(EFPBaseProvider baseProvider, UserSingleGridCellComboBox control)
      : base(baseProvider, control, true)
    {
      _MainControlProvider = new EFPDataGridView(baseProvider, control.MainControl);
      _MainControlProvider.GetCellAttributes += MainControlProvider_GetCellAttributes;
      _MainControlProvider.HideSelection = true;
      _MainControlProvider.GrayWhenDisabled = true;
      _MainControlProvider.Control.ShowCellToolTips = false; // включилось в конструкторе EFPDataGridView
    }

    #endregion

    #region Табличный просмотр

    private EFPDataGridView _MainControlProvider;

    /// <summary>
    /// Событие вызывается при прорисовке элемента.
    /// Пользовательский обработчик может установить значение и задать форматирование ячейки способом, обычным для EFPDataGridView.
    /// </summary>
    public event EFPDataGridViewCellAttributesEventHandler GetCellAttributes;

    /// <summary>
    /// Вызывает событие GetCellAttributes
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnGetCellAttributes(EFPDataGridViewCellAttributesEventArgs args)
    {
      if (GetCellAttributes != null)
        OnGetCellAttributes(args);
    }

    void MainControlProvider_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      OnGetCellAttributes(args);
    }


    #endregion
  }
}
