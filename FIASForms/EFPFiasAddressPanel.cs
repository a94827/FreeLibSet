using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.FIAS;
using FreeLibSet.DependedValues;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Controls;
using FreeLibSet.UICore;

/*
 * The BSD License
 * 
 * Copyright (c) 2020, Ageyev A.V.
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

namespace FreeLibSet.Forms.FIAS
{
  /// <summary>
  /// Провайдер панели редактора адреса
  /// </summary>
  public class EFPFiasAddressPanel : EFPControl<FiasAddressPanel>
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="ui">Ссылка на пользовательский интерфейс ФИАС</param>
    /// <param name="editorLevel">Уровень адреса, до которого можно редактировать</param>
    public EFPFiasAddressPanel(EFPBaseProvider baseProvider, FiasAddressPanel control, FiasUI ui, FiasEditorLevel editorLevel)
      : base(baseProvider, control, false)
    {
      Init(ui, editorLevel);
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    /// <param name="ui">Ссылка на пользовательский интерфейс ФИАС</param>
    /// <param name="editorLevel">Уровень адреса, до которого можно редактировать</param>
    public EFPFiasAddressPanel(EFPControlWithToolBar<FiasAddressPanel> controlWithToolBar, FiasUI ui, FiasEditorLevel editorLevel)
      : base(controlWithToolBar, false)
    {
      Init(ui, editorLevel);
    }

    private void Init(FiasUI ui, FiasEditorLevel editorLevel)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;

      _EditorLevel = editorLevel;
      _MinRefBookLevel = FiasTools.DefaultMinRefBookLevel;

      _CanBeEmptyMode = UIValidateState.Error;
      _CanBePartialMode = UIValidateState.Error;

      _Handler = new FiasHandler(ui.Source);

      _AddrObParts = new List<AddrObItem>(6);
      _AddrObParts.Add(new AddrObItem(this, Control.edRegionName, Control.cbRegionAOType, Control.btnRegionSel, Control.btnRegionClear, FiasLevel.Region));
      _AddrObParts.Add(new AddrObItem(this, Control.edDistrictName, Control.cbDistrictAOType, Control.btnDistrictSel, Control.btnDistrictClear, FiasLevel.District));
      _AddrObParts.Add(new AddrObItem(this, Control.edCityName, Control.cbCityAOType, Control.btnCitySel, Control.btnCityClear, FiasLevel.City));
      _AddrObParts.Add(new AddrObItem(this, Control.edVillageName, Control.cbVillageAOType, Control.btnVillageSel, Control.btnVillageClear, FiasLevel.Village));

      bool HasStreet, HasHouse, HasRoom;
      switch (editorLevel)
      {
        case FiasEditorLevel.Village: HasStreet = false; HasHouse = false; HasRoom = false; break;
        case FiasEditorLevel.Street: HasStreet = true; HasHouse = false; HasRoom = false; break;
        case FiasEditorLevel.House: HasStreet = true; HasHouse = true; HasRoom = false; break;
        case FiasEditorLevel.Room: HasStreet = true; HasHouse = true; HasRoom = true; break;
        default: throw new BugException("Неправильный EditorSettings.Level");
      }

      if (HasStreet)
      {
        _AddrObParts.Add(new AddrObItem(this, Control.edPlanStrName, Control.cbPlanStrAOType, Control.btnPSSel, Control.btnPSClear, FiasLevel.PlanningStructure));
        _AddrObParts.Add(new AddrObItem(this, Control.edStreetName, Control.cbStreetAOType, Control.btnStreetSel, Control.btnStreetClear, FiasLevel.Street));
      }
      else
      {
        this.Control.SetLevelVisible(FiasLevel.PlanningStructure, false);
        this.Control.SetLevelVisible(FiasLevel.Street, false);
      }

      if (HasHouse)
      {
        _HousePart = new HouseItem(this, Control.edHouseName, Control.cbHouseAOType, Control.edBuildingName, Control.cbBuildingAOType,
          Control.edStrName, Control.cbStrAOType, Control.btnHouseSel, Control.btnHouseClear);
      }
      else
      {
        this.Control.SetLevelVisible(FiasLevel.House, false);
        this.Control.SetLevelVisible(FiasLevel.Building, false);
        this.Control.SetLevelVisible(FiasLevel.Structure, false);
      }

      if (HasRoom)
      {
        _RoomPart = new RoomItem(this, Control.edFlatName, Control.cbFlatAOType, Control.edRoomName, Control.cbRoomAOType, Control.btnFlatSel, Control.btnFlatClear);
      }
      else
      {
        this.Control.SetLevelVisible(FiasLevel.Flat, false);
        this.Control.SetLevelVisible(FiasLevel.Room, false);
      }

      efpManualPostalCode = new EFPCheckBox(BaseProvider, Control.cbManualPostalCode);
      efpManualPostalCode.ToolTipText = "Позволяет переопределить почтовый индекс, если он не определяется или задан неправильно в классификаторе ФИАС";

      efpPostalCode = new EFPMaskedTextBox(BaseProvider, Control.edPostalCode);
      efpPostalCode.CanBeEmptyMode = UIValidateState.Warning;
      efpPostalCode.EnabledEx = new DepAnd(efpManualPostalCode.EnabledEx, efpManualPostalCode.CheckedEx);

      efpManualPostalCode.CheckedEx.ValueChanged += new EventHandler(ManualPostalCodeChanged);
      efpPostalCode.TextEx.ValueChanged += new EventHandler(ManualPostalCodeChanged);

      _TextView = new EFPTextBox(this.BaseProvider, this.Control.edTextView);
      _TextView.CommandItems.UseStatusBarRC = false;

      #region Кнопки

      Control.btnCopy.Image = EFPApp.MainImages.Images["Copy"];
      Control.btnCopy.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
      efpCopy = new EFPButton(this.BaseProvider, Control.btnCopy);
      efpCopy.DisplayName = "Копировать адрес";
      efpCopy.ToolTipText = "Копирует адрес в буфер обмена в кодированном текстовом формате";
      efpCopy.Click += new EventHandler(efpCopy_Click);

      Control.btnPaste.Image = EFPApp.MainImages.Images["Paste"];
      Control.btnPaste.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
      efpPaste = new EFPButton(this.BaseProvider, Control.btnPaste);
      efpPaste.DisplayName = "Вставить адрес";
      efpPaste.ToolTipText = "Вставляет ранее скопированный адрес из буфера обмена." + Environment.NewLine +
        "Все адресные элементы будут заменены или очищены";
      efpPaste.Click += new EventHandler(efpPaste_Click);


      _ErrorButton = new EFPErrorMessageListButton(this.BaseProvider, this.Control.btnErrors);
      _ErrorButton.AutoText = false;
      _ErrorButton.AutoImageKey = true;

      _MoreButton = new EFPButtonWithMenu(this.BaseProvider, this.Control.btnMore);
      _MoreButton.DisplayName = "Еще";
      _MoreButton.ToolTipText = "Меню с дополнительными командами";

      _CmdSearch = new EFPCommandItem("View", "Search");
      _CmdSearch.MenuText = "Найти адресный объект";
      _CmdSearch.ImageKey = "Find";
      _CmdSearch.ToolTipText = "Поиск адресного объекта";
      _CmdSearch.Click += new EventHandler(CmdSearch_Click);
      _CmdSearch.Enabled = _Handler.AddressSearchEnabled;
      _MoreButton.CommandItems.Add(_CmdSearch);

      _CmdDetails = new EFPCommandItem("View", "Details");
      _CmdDetails.MenuText = "Подробности";
      _CmdDetails.ImageKey = "Fias.Details";
      _CmdDetails.ToolTipText = "Подробная информация по каждому уровню классификатора ФИАС";
      _CmdDetails.Click += new EventHandler(CmdDetails_Click);
      _MoreButton.CommandItems.Add(_CmdDetails);

      _CmdClear = new EFPCommandItem("Edit", "Clear");
      _CmdClear.MenuText = "Очистить адрес";
      _CmdClear.ImageKey = "No";
      _CmdClear.Click += new EventHandler(CmdClear_Click);
      _MoreButton.CommandItems.Add(_CmdClear);

      EFPCommandItem CmdDBSettings = new EFPCommandItem("View", "DBSettings");
      CmdDBSettings.MenuText = "Посмотреть настройки классификатора ФИАС";
      CmdDBSettings.Click += new EventHandler(CmdDBSettings_Click);
      CmdDBSettings.GroupBegin = true;
      CmdDBSettings.GroupEnd = true;
      _MoreButton.CommandItems.Add(CmdDBSettings);

      #endregion

      InitStatusBarItems();

      this.Address = new FiasAddress();
      //OnAddressChanged();
    }

    /// <summary>
    /// Выполняет первое обновление полей адреса
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();
      UpdateAddress(FiasLevel.Unknown, Guid.Empty);
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Возвращает "Адрес".
    /// </summary>
    protected override string DefaultDisplayName { get { return "Адрес"; } }

    /// <summary>
    /// Пользовательский интерфейс
    /// </summary>
    public FiasUI UI { get { return _UI; } }
    private FiasUI _UI;

    private FiasHandler _Handler;

    /// <summary>
    /// Уровень, до которого можно вводить адрес.
    /// Задается в конструкторе
    /// </summary>
    public FiasEditorLevel EditorLevel { get { return _EditorLevel; } }
    private FiasEditorLevel _EditorLevel;

    private List<AddrObItem> _AddrObParts;

    private HouseItem _HousePart;

    private RoomItem _RoomPart;

    private EFPCheckBox efpManualPostalCode;
    private EFPMaskedTextBox efpPostalCode;

    /// <summary>
    /// Можно ли редактировать почтовый индекс?
    /// По умолчанию - true
    /// </summary>
    public bool PostalCodeEditable
    {
      get { return Control.PostalCodeVisible; }
      set
      {
        Control.PostalCodeVisible = value;
        if (!value)
          Address.AutoPostalCode = true;
        UpdateAddress(FiasLevel.Unknown, Guid.Empty);
      }
    }

    /// <summary>
    /// Задает минимальный уровень адреса, который должен быть выбран из справочника, а не задан вручную.
    /// По умолчанию - FiasLevel.City, то есть регион, район и город должны быть в справочнике ФИАС, а населенный пункт,
    /// при необходимости - введен вручную, если его нет в справочнике.
    /// Значение Unknown отключает все проверки. 
    /// Допускаются любые значения, включая House и Room, если они не выходят за пределы FiasEditorLevel.
    /// Свойство можно устанавливать только до вывода элемента на экран. 
    /// </summary>
    public FiasLevel MinRefBookLevel
    {
      get { return _MinRefBookLevel; }
      set
      {
        CheckHasNotBeenCreated();

        switch (value)
        {
          case FiasLevel.Street:
            switch (EditorLevel)
            {
              case FiasEditorLevel.Village:
                throw new ArgumentOutOfRangeException("value", value, "Несовместимо с EditorLevel=" + EditorLevel.ToString());
            }
            break;
          case FiasLevel.House:
          case FiasLevel.Building:
          case FiasLevel.Structure:
            value = FiasLevel.House;
            switch (EditorLevel)
            {
              case FiasEditorLevel.Village:
              case FiasEditorLevel.Street:
                throw new ArgumentOutOfRangeException("value", value, "Несовместимо с EditorLevel=" + EditorLevel.ToString());
            }
            break;
          case FiasLevel.Flat:
          case FiasLevel.Room:
            value = FiasLevel.Flat;
            switch (EditorLevel)
            {
              case FiasEditorLevel.Village:
              case FiasEditorLevel.Street:
              case FiasEditorLevel.House:
                throw new ArgumentOutOfRangeException("value", value, "Несовместимо с EditorLevel=" + EditorLevel.ToString());
            }
            break;
        }
        _MinRefBookLevel = value;
      }
    }
    private FiasLevel _MinRefBookLevel;

    #endregion

    #region Адресные объекты

    /// <summary>
    /// Расширение провайдера текстовых полей наименований и сокращений.
    /// Поддерживают отложенную обработку события TextBox.Leave
    /// </summary>
    private class EFPTextBox2 : EFPTextBox
    {
      #region Конструктор

      public EFPTextBox2(EFPBaseProvider baseProvider, TextBox control)
        : base(baseProvider, control)
      {
        UseIdle = true;
        Control.Leave += new EventHandler(Control_Leave);
        _OrgText = control.Text;
        _Errors = ErrorMessageList.Empty;
      }

      #endregion

      #region Событие Leave

      private bool _LeaveFlag;

      void Control_Leave(object sender, EventArgs args)
      {
        if (!this.Editable)
          return;
        if (_OrgText == base.Text)
          return;

        _LeaveFlag = true;
      }

      public override void HandleIdle()
      {
        base.HandleIdle();
        if (_LeaveFlag)
        {
          _LeaveFlag = false;
          if (Leave != null)
            Leave(this, EventArgs.Empty);
        }
      }

      public event EventHandler Leave;

      #endregion

      #region Отслеживание изменений в тексте

      private string _OrgText;

      /// <summary>
      /// Вызывается при обновлении свойства Address()
      /// </summary>
      public void SetText(string value)
      {
        _OrgText = value;
        base.Text = value;
      }

      #endregion

      //#region DisabledText

      ///// <summary>
      ///// Для полей сокращений
      ///// Свойство ReadOnly не приводит к установке текста "[xxx]".
      ///// Возвращаем свойство, как оно определено в EFPTextBoxBase
      ///// </summary>
      //public override bool IsDisabledText
      //{
      //  get
      //  {
      //    return AllowDisabledText && (!Enabled); // без ReadOnly
      //  }
      //}

      //#endregion

      #region Проверка текущего значения

      /// <summary>
      /// Список сообщений, относящихся к этому уровню
      /// </summary>
      public ErrorMessageList Errors
      {
        get { return _Errors; }
        set
        {
          if (Object.ReferenceEquals(value, _Errors))
            return; // оба списка равны ErrorMessageList.Empty
          _Errors = value;
          Validate();
        }
      }
      private ErrorMessageList _Errors;


      protected override void OnValidate()
      {
        if (BaseProvider.ValidateReason == EFPFormValidateReason.Closing)
        {
          Control_Leave(null, null);
          if (_LeaveFlag)
          {
            _LeaveFlag = false;
            if (Leave != null)
              Leave(this, EventArgs.Empty);
          }
        }

        base.OnValidate();
        if (ValidateState == UIValidateState.Error)
          return;

        if (_OrgText == base.Text)
        {
          // Ошибки показываем только когда текст не изменился
          switch (_Errors.Severity)
          {
            case ErrorMessageKind.Error:
              SetError(_Errors.AllText);
              break;
            case ErrorMessageKind.Warning:
              SetWarning(_Errors.AllText);
              break;
          }
        }
      }

      #endregion
    }

    /// <summary>
    /// Расширение провайдера текстовых полей наименований и сокращений.
    /// Поддерживают отложенную обработку события TextBox.Leave
    /// </summary>
    private class EFPComboBox2 : EFPTextComboBox
    {
      #region Конструктор

      public EFPComboBox2(EFPBaseProvider baseProvider, ComboBox control)
        : base(baseProvider, control)
      {
        UseIdle = true;
        Control.Leave += new EventHandler(Control_Leave);
        Control.DropDown += new EventHandler(Control_DropDown);
        Control.DropDownClosed += new EventHandler(Control_DropDownClosed);
        _OrgText = control.Text;
        _Errors = ErrorMessageList.Empty;
      }

      #endregion

      #region Событие Leave

      private bool _LeaveFlag;

      void Control_Leave(object sender, EventArgs args)
      {
        if (!this.Editable)
          return;
        if (_OrgText == base.Text)
          return;

        _LeaveFlag = true;
      }

      public override void HandleIdle()
      {
        base.HandleIdle();
        if (_LeaveFlag)
        {
          _LeaveFlag = false;
          if (Leave != null)
            Leave(this, EventArgs.Empty);
        }
      }

      public event EventHandler Leave;

      #endregion

      #region Отслеживание изменений в тексте

      private string _OrgText;

      /// <summary>
      /// Вызывается при обновлении свойства Address()
      /// </summary>
      public void SetText(string value)
      {
        _OrgText = value;
        base.Text = value;
      }

      /// <summary>
      /// Свойство DroppedDown работает с задержкой
      /// </summary>
      private bool _HideErrors;

      void Control_DropDown(object sender, EventArgs args)
      {
        // Отключаем подсветку ошибок при открытии выпадающего списка.
        // Зачем нам красный список?
        _HideErrors = true;
        Control.ForeColor = System.Drawing.SystemColors.ControlText; // вдруг OnValidate() будет вызван с задержкой в будущих версиях
        Validate();
      }

      void Control_DropDownClosed(object sender, EventArgs args)
      {
        _HideErrors = false;
        Validate();
      }


      #endregion

      #region Проверка текущего значения

      /// <summary>
      /// Список сообщений, относящихся к этому уровню
      /// </summary>
      public ErrorMessageList Errors
      {
        get { return _Errors; }
        set
        {
          if (Object.ReferenceEquals(value, _Errors))
            return; // оба списка равны ErrorMessageList.Empty
          _Errors = value;
          Validate();
        }
      }
      private ErrorMessageList _Errors;


      protected override void OnValidate()
      {
        if (BaseProvider.ValidateReason == EFPFormValidateReason.Closing)
        {
          Control_Leave(null, null);
          if (_LeaveFlag)
          {
            _LeaveFlag = false;
            if (Leave != null)
              Leave(this, EventArgs.Empty);
          }
        }

        if (_HideErrors)
          return; // 27.10.2020

        base.OnValidate();
        if (ValidateState == UIValidateState.Error)
          return;

        if (_OrgText == base.Text)
        {
          // Ошибки показываем только когда текст не изменился
          switch (_Errors.Severity)
          {
            case ErrorMessageKind.Error:
              SetError(_Errors.AllText);
              break;
            case ErrorMessageKind.Warning:
              SetWarning(_Errors.AllText);
              break;
          }
        }
      }

      #endregion
    }

    /// <summary>
    /// Обработчики для пары полей наименования и типа адресообразующего элемента
    /// </summary>
    private class NameAndAOType
    {
      #region Конструктор

      public NameAndAOType(EFPFiasAddressPanel owner, TextBox edName, ComboBox cbAOType, FiasLevel level)
      {
        _Owner = owner;
        _Level = level;

        _Name = new EFPTextBox2(owner.BaseProvider, edName);
        _Name.Control.AutoCompleteMode = AutoCompleteMode.Append;
        _Name.Control.AutoCompleteSource = AutoCompleteSource.CustomSource;
        _Name.DisplayName = "Наименование для уровня " + FiasEnumNames.ToString(level, true);
        _Name.CommandItems.UseStatusBarRC = false;
        _Name.ReadOnlyEx = _Owner.ReadOnlyEx;
        _Name.Validating += new UIValidatingEventHandler(Name_Validating); // 03.03.2021

        _AOType = new EFPComboBox2(owner.BaseProvider, cbAOType);
        _AOType.Control.AutoCompleteMode = AutoCompleteMode.Append;
        _AOType.Control.AutoCompleteSource = AutoCompleteSource.CustomSource;
        _AOType.DisplayName = "Сокращение для уровня " + FiasEnumNames.ToString(level, true);
        _AOType.CommandItems.UseStatusBarRC = false;

        _AllAOTypes = _Owner._Handler.AOTypes.GetAOTypes(level, FiasAOTypeMode.Full);
        _AOType.Control.Items.AddRange(_AllAOTypes);
        _AOType.Control.AutoCompleteCustomSource.AddRange(_AllAOTypes);
        _AOType.DisabledText = "[" + FiasEnumNames.ToString(level, false) + "]";

#if XXX
        EFPCommandItem ci;

        ci = new EFPCommandItem("Edit", "SelectAOType");
        ci.MenuText = "Список возможных типов";
        ci.Click += new EventHandler(ciSelectAOType_Click);
        ci.EnabledEx = new DepNot(_Owner.ReadOnlyEx);
        ci.GroupBegin = true;
        _AOType.CommandItems.Add(ci);
#endif

        //InitAOTypeEnabled();
      }

      public void InitAOTypeEnabled()
      {
        if (_Owner.EnabledState)
        {
          _AOType.AllowDisabledText = true;
          if (_AOType.Control.AutoCompleteCustomSource.Count == 0)
            _AOType.Enabled = false;
          else if (_Name.Text.Length == 0)
            _AOType.Enabled = false;
          else
            _AOType.Enabled = true;
        }
        else
        {
          _AOType.Enabled = false;
          _AOType.AllowDisabledText = false;
        }
      }

      void Name_Validating(object sender, UIValidatingEventArgs args)
      {
        InitAOTypeEnabled();

        if (args.ValidateState == UIValidateState.Error)
          return;
        if (String.IsNullOrEmpty(_Name.Text))
          return;

        string errorText;
        if (!FiasTools.IsValidName(_Name.Text, _Level, out errorText))
          args.SetError(errorText);
      }

      #endregion

      #region Свойства

      public EFPFiasAddressPanel Owner { get { return _Owner; } }
      private EFPFiasAddressPanel _Owner;

      public FiasLevel Level { get { return _Level; } }
      private FiasLevel _Level;

      /// <summary>
      /// Провайдер поля наименования
      /// </summary>
      public EFPTextBox2 Name { get { return _Name; } }
      private EFPTextBox2 _Name;

      /// <summary>
      /// Провайдер поля типа адресообразующего элемента.
      /// Используется полное наименование типа ("улица"), а не сокращение
      /// </summary>
      public EFPComboBox2 AOType { get { return _AOType; } }
      private EFPComboBox2 _AOType;

      /// <summary>
      /// Список всех возможных типов адресообразующих элементов для данного уровня
      /// </summary>
      public string[] AllAOTypes { get { return _AllAOTypes; } }
      private string[] _AllAOTypes;

      #endregion

      #region Методы

      public void AddressUpdated()
      {
        if (_Owner.ProviderState != EFPControlProviderState.Attached)
          return;

        _Name.SetText(_Owner._Address.GetName(_Level));

        string aoType = _Owner._Address.GetAOType(_Level);
        if (aoType.Length > 0)
          _AOType.SetText(aoType);
        else if (AllAOTypes.Length > 0 && (!_Owner.ReadOnly))
        {
          //_AOType.Text = AllAOTypes[0];
          /*
          int p = Array.IndexOf(AllAOTypes, FiasEnumNames.ToString(_Level, false));
          if (p < 0)
            p = 0;
          _AOType.SetText(AllAOTypes[p]);
           * */
          _AOType.SetText(FiasTools.GetDefaultAOType(Level));
        }
        else
          _AOType.SetText(String.Empty);

        //_Name.ReadOnly = _Owner.ReadOnly;  сделано в конструкторе через ReadOnlyEx
        //_AOType.ReadOnly = _Owner.ReadOnly;

        ErrorMessageList errors = _Owner.Address.GetMessages(Level);
        _Name.Errors = errors;
        _AOType.Errors = errors;
      }

      public void Validate()
      {
        _Name.Validate();
        _AOType.Validate();
      }

      #endregion

#if XXX
      #region Выбор типа элемента из списка

      void ciSelectAOType_Click(object sender, EventArgs args)
      {
        ListSelectDialog dlg = new ListSelectDialog();
        dlg.Title = "Выбор типа адресообразующего элемента";
        dlg.Items = AllAOTypes;
        dlg.ListTitle = "Типы";
        dlg.SelectedItem = _AOType.Text;
        dlg.DialogPosition.PopupOwnerControl = _AOType.Control;
        if (dlg.ShowDialog() != DialogResult.OK)
          return;

        _AOType.Text = dlg.SelectedItem;
      }

      #endregion
#endif
    }

    /// <summary>
    /// Содержит один или несколько объектов NameAndAOType (текстовые поля) и пару кнопок "Выбор" и "Очистить"
    /// </summary>
    private abstract class ItemBase
    {
      #region Конструктор

      public ItemBase(NameAndAOType[] nas, Button btnSel, Button btnClear)
      {
        _NAs = nas;

        btnSel.Image = EFPApp.MainImages.Images["DropDown"];
        efpButtonSel = new EFPButton(this.Owner.BaseProvider, btnSel);
        efpButtonSel.DisplayName = "Выбор";
        efpButtonSel.ToolTipText = "Выбор из справочника " + FiasEnumNames.ToString(this.Level, true);
        efpButtonSel.Click += new EventHandler(efpButtonSel_Click);

        btnClear.Image = EFPApp.MainImages.Images["Clear"];
        efpButtonClear = new EFPButton(this.Owner.BaseProvider, btnClear);
        efpButtonClear.DisplayName = "Очистка";
        efpButtonClear.ToolTipText = "Очистка поля " + FiasEnumNames.ToString(this.Level, true);

        DepValue<bool>[] aNotEmpty = new DepValue<bool>[nas.Length];
        for (int i = 0; i < nas.Length; i++)
          aNotEmpty[i] = nas[i].Name.IsNotEmptyEx;
        efpButtonClear.EnabledEx = new DepAnd(new DepNot(nas[0].Owner.ReadOnlyEx), new DepOr(aNotEmpty));
        efpButtonClear.Click += new EventHandler(efpButtonClear_Click);

        _PageIsEmpty = true;
        _ParentGuid = new Guid("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"); // чтобы обновилось в конструкторе

        for (int i = 0; i < nas.Length; i++)
        {
          _NAs[i].Name.Leave += new EventHandler(TextBox_Leave);
          _NAs[i].AOType.Leave += new EventHandler(TextBox_Leave);
        }
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Обработчики текстовых полей
      /// </summary>
      protected NameAndAOType[] NAs { get { return _NAs; } }
      private NameAndAOType[] _NAs;

      public EFPFiasAddressPanel Owner { get { return _NAs[0].Owner; } }

      public FiasLevel Level { get { return _NAs[0].Level; } }

      /// <summary>
      /// Возвращает true, если страница классификатора не присоединена или не содержит строк.
      /// </summary>
      protected bool PageIsEmpty { get { return _PageIsEmpty; } }
      private bool _PageIsEmpty;

      /// <summary>
      /// Возвращает true, если текст не заполнен
      /// </summary>
      public bool IsEmpty
      {
        get
        {
          for (int i = 0; i < +_NAs.Length; i++)
          {
            if (!String.IsNullOrEmpty(_NAs[i].Name.Text))
              return false;
          }
          return true;
        }
      }

      public override string ToString()
      {
        return "Level=" + Level.ToString() + ", ParentGuid=" + ParentGuid.ToString();
      }

      #endregion

      #region Методы

      /// <summary>
      /// Вызывается при смене или начальной установке адреса
      /// </summary>
      internal void AddressUpdated()
      {
        for (int i = 0; i < _NAs.Length; i++)
          _NAs[i].AddressUpdated();
      }

      private void TextBox_Leave(object sender, EventArgs args)
      {
        OnTextBoxLeave();
      }

      /// <summary>
      /// Вызывается при выходе из текстового поля.
      /// Переопределяется в AddrObItem для обработки специальных страниц
      /// </summary>
      protected virtual void OnTextBoxLeave()
      {
        for (int i = 0; i < _NAs.Length; i++)
        {
          Owner.Address.SetName(_NAs[i].Level, _NAs[i].Name.Text);
          if (_NAs[i].Name.Text.Length > 0)
            Owner.Address.SetAOType(_NAs[i].Level, _NAs[i].AOType.Text);
          else
            Owner.Address.SetAOType(_NAs[i].Level, String.Empty);
        }

        Owner.UpdateAddress(Level, Guid.Empty);
      }

      internal void InitEnabled()
      {
        for (int i = 0; i < _NAs.Length; i++)
          _NAs[i].InitAOTypeEnabled();
        InitButtonSelEnabled();
      }

      #endregion

      #region Свойство ParentGiud

      /// <summary>
      /// Идентификатор родительского адресного объекта
      /// </summary>
      public Guid ParentGuid { get { return _ParentGuid; } }
      private Guid _ParentGuid;

      /// <summary>
      /// Уровень родительского объекта, для которого задан ParentGuid
      /// </summary>
      public FiasLevel ParentLevel { get { return _ParentLevel; } }
      private FiasLevel _ParentLevel;

      public void SetParentGuid(Guid parentGuid, FiasLevel parentLevel)
      {
        if (parentGuid == _ParentGuid && parentLevel == _ParentLevel)
          return;

        _ParentGuid = parentGuid;
        _ParentLevel = parentLevel;

        for (int i = 0; i < _NAs.Length; i++)
          _NAs[i].Name.Control.AutoCompleteCustomSource.Clear();

        bool isInheritable = FiasTools.IsInheritableLevel(parentLevel, this.Level, false);

        OnParentGuidChanged(isInheritable, out _PageIsEmpty);

        InitButtonSelEnabled();


        for (int i = 0; i < _NAs.Length; i++)
          _NAs[i].Validate();
      }

      private void InitButtonSelEnabled()
      {
        if (_PageIsEmpty)
          efpButtonSel.Enabled = false;
        else
          efpButtonSel.Enabled = !NAs[0].Name.ReadOnly;
      }

      /// <summary>
      /// Вызывается при изменении свойства ParentGuid.
      /// Переопределенный метод должен извлечь 
      /// </summary>
      /// <param name="isInheritable">True, если допускается наследование от предыдущего заданного уровня</param>
      /// <param name="isPageEmpty"></param>
      protected abstract void OnParentGuidChanged(bool isInheritable, out bool isPageEmpty);

      #endregion

      #region Обработчики кнопок

      private EFPButton efpButtonSel;

      private EFPButton efpButtonClear;

      void efpButtonSel_Click(object sender, EventArgs args)
      {
        if (PageIsEmpty)
        {
          if (ParentGuid == Guid.Empty)
            EFPApp.ShowTempMessage("Сначала должен быть выбран элемент более высокого уровня");
          else
            EFPApp.ShowTempMessage("Нет элементов уровня " + FiasEnumNames.ToString(Level, true));
          return;
        }

        OnButtonSelClick();
      }

      protected abstract void OnButtonSelClick();

      void efpButtonClear_Click(object sender, EventArgs args)
      {
        Clear();
      }

      protected void Clear()
      {
        bool flag = false;
        for (int i = 0; i < _NAs.Length; i++)
        {
          if (Owner.Address.GetName(_NAs[i].Level).Length == 0)
            continue;

          Owner.Address.SetName(_NAs[i].Level, String.Empty);
          Owner.Address.SetAOType(_NAs[i].Level, String.Empty);
          flag = true;
        }

        if (flag)
          Owner.UpdateAddress(Level, Guid.Empty);
      }

      #endregion
    }

    private class AddrObItem : ItemBase
    {
      #region Конструктор

      public AddrObItem(EFPFiasAddressPanel owner, TextBox edName, ComboBox cbAOType, Button btnSel, Button btnClear, FiasLevel level)
        : base(new NameAndAOType[1] { new NameAndAOType(owner, edName, cbAOType, level) }, btnSel, btnClear)
      {
      }

      #endregion

      #region Корневой узел для выбора

      protected override void OnParentGuidChanged(bool isInheritable, out bool pageIsEmpty)
      {
        if (ParentGuid == Guid.Empty)
        {
          switch (Level)
          {
            case FiasLevel.Region:
              EFPApp.BeginWait("Получение справочника регионов", "FiasAddress", true);
              try { _Page = Owner._Handler.GetAddrObPage(Level, ParentGuid); }
              finally { EFPApp.EndWait(); }
              break;
            case FiasLevel.City:
              EFPApp.BeginWait("Получение полного справочника городов", "FiasAddress", true);
              try
              { _Page = Owner._Handler.Source.GetSpecialAddrObPage(FiasSpecialPageType.AllCities, Guid.Empty); }
              finally { EFPApp.EndWait(); }
              break;
            case FiasLevel.District:
              EFPApp.BeginWait("Получение справочника всех районов", "FiasAddress", true);
              try { _Page = Owner._Handler.Source.GetSpecialAddrObPage(FiasSpecialPageType.AllDistricts, Guid.Empty); }
              finally { EFPApp.EndWait(); }
              break;
            default:
              _Page = null;
              break;
          }
        }
        else if (isInheritable)
        {
          EFPApp.BeginWait("Получение адресных объектов для уровня \"" + FiasEnumNames.ToString(Level, true) + "\"", "FiasAddress", true);
          try
          { _Page = Owner._Handler.GetAddrObPage(Level, ParentGuid); }
          finally { EFPApp.EndWait(); }
        }
        else
          _Page = null;


        if (_Page == null)
          pageIsEmpty = true;
        else
          pageIsEmpty = _Page.IsEmpty;

        if (!pageIsEmpty)
          NAs[0].Name.Control.AutoCompleteCustomSource.AddRange(_Page.Names);
      }

      /// <summary>
      /// Таблица, из которой можно делать выбор
      /// </summary>
      private FiasCachedPageAddrOb _Page;


      #endregion

      #region Выбор из справочника

      protected override void OnButtonSelClick()
      {
        using (DataView dv = _Page.CreateDataView())
        {
          using (OKCancelGridForm form = new OKCancelGridForm())
          {
            form.Text = "Выбор из справочника уровня [" + FiasEnumNames.ToString(this.Level, true) + "] (PARENTGUID=" + ParentGuid.ToString() + ")";
            form.NoButtonProvider.Visible = true;
            form.FormProvider.ConfigSectionName = "Fias_Sel_" + Level.ToString();
            EFPFiasListDataGridView efpGrid = new EFPFiasListDataGridView(form.ControlWithToolBar, Owner.UI, FiasTableType.AddrOb, false);
            efpGrid.CommandItems.EnterAsOk = true;

            efpGrid.Control.DataSource = dv;

            Int32 aoTypeId = Owner._Handler.AOTypes.FindAOTypeId(Level, NAs[0].AOType.Text);
            DataRow row = _Page.FindRow(NAs[0].Name.Text, aoTypeId).Row;
            efpGrid.CurrentDataRow = row;

            EFPDialogPosition pos = new EFPDialogPosition(NAs[0].Name.Control);

            switch (EFPApp.ShowDialog(form, false, pos))
            {
              case DialogResult.OK:
                row = efpGrid.CurrentDataRow;
                if (row == null)
                  EFPApp.ErrorMessageBox("Строка не выбрана");
                else
                {
                  Guid recId = DataTools.GetGuid(row, "AOID");
                  if (recId != Owner.Address.GetRecId(Level))
                    Owner.UpdateAddress(Level, recId);
                }
                break;
              case DialogResult.No:
                Clear();
                break;
              default:
                break;
            }
          }
        }
      }

      protected override void OnTextBoxLeave()
      {
        PerformUpdateAddress();
        base.OnTextBoxLeave();
      }

      private void PerformUpdateAddress()
      {
        // 26.10.2020
        // Можно в поле названия региона ввести код региона
        if (Level == FiasLevel.Region)
        {
          int nRegion;
          if (int.TryParse(NAs[0].Name.Text, out nRegion))
          {
            if (nRegion >= 1 && nRegion <= 99)
            {
              string sRegion = nRegion.ToString("00");
#if XXX
              DataRow row = FiasTools.RegionCodes.Rows.Find(sRegion);
              if (row != null)
              {
                Guid AOGuid = DataTools.GetGuid(row, "AOGuid");
                FiasAddress a2 = new FiasAddress();
                a2.SetGuid(FiasLevel.Region, AOGuid);
                Owner._Handler.FillAddress(a2);
                Owner.UpdateAddress(FiasLevel.Region, a2.GetRecId(FiasLevel.Region));
                return;
              }
#else
              Guid AOGuid = Owner._Handler.GetRegionAOGuid(sRegion); // 03.09.2021
              if (AOGuid != Guid.Empty)
              {
                FiasAddress a2 = new FiasAddress();
                a2.SetGuid(FiasLevel.Region, AOGuid);
                Owner._Handler.FillAddress(a2);
                Owner.UpdateAddress(FiasLevel.Region, a2.GetRecId(FiasLevel.Region));
                return;
              }
#endif
            }
          }
        }

        if (_Page != null)
        {
          Int32 aoTypeId = Owner._Handler.AOTypes.FindAOTypeId(Level, NAs[0].AOType.Text);
          DataRow row = _Page.FindRow(NAs[0].Name.Text, aoTypeId).Row;
          if (row != null)
          {
            Guid recId = DataTools.GetGuid(row, "AOID");
            if (recId != Owner.Address.GetRecId(Level))
              Owner.UpdateAddress(Level, recId);
          }
        }
      }

      #endregion
    }

    #endregion

    #region Дома

    private class HouseItem : ItemBase
    {
      #region Конструктор

      public HouseItem(EFPFiasAddressPanel owner,
        TextBox edHouseName, ComboBox cbHouseAOType,
        TextBox edBuildingName, ComboBox cbBuildingAOType,
        TextBox edStrName, ComboBox cbStrAOType,
        Button btnSel, Button btnClear)
        : base(new NameAndAOType[3]{
          new NameAndAOType(owner, edHouseName, cbHouseAOType, FiasLevel.House),
          new NameAndAOType(owner, edBuildingName, cbBuildingAOType, FiasLevel.Building),
          new NameAndAOType (owner, edStrName, cbStrAOType, FiasLevel .Structure)
        }, btnSel, btnClear)
      {
        if (!owner.UI.DBSettings.UseHouse)
          btnSel.Visible = false;
      }

      #endregion

      #region Корневой узел для выбора

      protected override void OnParentGuidChanged(bool isInheritable, out bool pageIsEmpty)
      {
        if (ParentGuid == Guid.Empty || (!Owner.UI.DBSettings.UseHouse) || (!isInheritable))
        {
          _Page = null;
          pageIsEmpty = true;
        }
        else
        {
          EFPApp.BeginWait("Получение справочника зданий", "FiasAddress", true);
          try { _Page = Owner._Handler.GetHousePage(ParentGuid); }
          finally { EFPApp.EndWait(); }
          pageIsEmpty = _Page.IsEmpty;
          NAs[0].Name.Control.AutoCompleteCustomSource.AddRange(_Page.GetHouseNums());
          NAs[1].Name.Control.AutoCompleteCustomSource.AddRange(_Page.GetBuildingNums(NAs[0].Name.Text));
          NAs[2].Name.Control.AutoCompleteCustomSource.AddRange(_Page.GetStrNums(NAs[0].Name.Text, NAs[1].Name.Text));
        }
      }

      /// <summary>
      /// Таблица, из которой можно делать выбор
      /// </summary>
      private FiasCachedPageHouse _Page;

      #endregion

      #region Выбор из справочника

      protected override void OnButtonSelClick()
      {
        using (DataView dv = _Page.CreateDataView())
        {
          using (OKCancelGridForm form = new OKCancelGridForm())
          {
            form.Text = "Выбор из справочника домов (AOGUID=" + ParentGuid.ToString() + ")";
            form.NoButtonProvider.Visible = true;

            FiasAddress a1 = new FiasAddress();
            a1.AOGuid = ParentGuid;
            Owner._Handler.FillAddress(a1);
            InfoLabel lbl = form.AddInfoLabel(DockStyle.Top);
            lbl.Text = "Адресный объект: " + Owner._Handler.GetTextWithoutPostalCode(a1);

            EFPFiasListDataGridView efpGrid = new EFPFiasListDataGridView(form.ControlWithToolBar, Owner.UI, FiasTableType.House, false);

            efpGrid.CommandItems.EnterAsOk = true;

            efpGrid.Control.DataSource = dv;

            FiasEstateStatus estStatus = FiasEnumNames.ParseEstateStatus(NAs[0].AOType.Text);
            FiasStructureStatus strStatus = FiasEnumNames.ParseStructureStatus(NAs[2].AOType.Text);

            DataRow row = _Page.FindRow(NAs[0].Name.Text, estStatus, NAs[1].Name.Text, NAs[2].Name.Text, strStatus);
            if (row == null)
            {
              #region Пытаемся найти хоть что-нибудь похожее

              // 03.11.2020

              if (!String.IsNullOrEmpty(NAs[0].Name.Text))
                row = EFPFiasAddressPanel.FindSomeNumRow(dv, "HOUSENUM", NAs[0].Name.Text);
              else
                row = EFPFiasAddressPanel.FindSomeNumRow(dv, "STRUCNUM", NAs[2].Name.Text);

              #endregion
            }
            efpGrid.CurrentDataRow = row;

            EFPDialogPosition pos = new EFPDialogPosition(NAs[0].Name.Control);

            switch (EFPApp.ShowDialog(form, false, pos))
            {
              case DialogResult.OK:
                row = efpGrid.CurrentDataRow;
                if (row == null)
                  EFPApp.ErrorMessageBox("Строка не выбрана");
                else
                {
                  Guid recId = DataTools.GetGuid(row, "HOUSEID");
                  if (recId != Owner.Address.GetRecId(Level))
                    Owner.UpdateAddress(Level, recId);
                }
                break;
              case DialogResult.No:
                Clear();
                break;
              default:
                break;
            }
          }
        }
      }



      #endregion

      #region Обработчики поля "Наименование"

      //protected override void OnLeave(FiasLevel level)
      //{
      //  DataRow row = null;
      //  if (_Page != null)
      //    row = _Page.FindRow(NAs[0].Name.Text, NAs[1].Name.Text, NAs[2].Name.Text);
      //  if (row != null)
      //    InitForRow(row);
      //}

      #endregion
    }

    #region Вспомогательные функции

    /// <summary>
    /// Поиск подходящей строки в просмотре для номера дома, строения или квартиры, если нет точного совпадения
    /// </summary>
    /// <param name="dv">Данные для выводимого на экран просмотра</param>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="value">Значение для поиска</param>
    /// <returns>Найденная строка таблицы или null, если совсем ничего не найдено</returns>
    internal static DataRow FindSomeNumRow(DataView dv, string columnName, string value)
    {
      if (string.IsNullOrEmpty(value))
        return null;

      DataRow row = null;
      string OldFilter = dv.RowFilter;
      FreeLibSet.Data.DBxFilter filter = new FreeLibSet.Data.StartsWithFilter(columnName, value);
      filter.AddToDataViewRowFilter(dv);
      if (dv.Count > 0)
        row = dv[0].Row;
      else
      {
        // если есть буквы на конце, пытаемся найти без них
        if (DoGetFirstNumPart(ref value))
        {
          dv.RowFilter = OldFilter;
          filter = new FreeLibSet.Data.StartsWithFilter(columnName, value);
          filter.AddToDataViewRowFilter(dv);
          if (dv.Count > 0)
            row = dv[0].Row;
        }
      }
      dv.RowFilter = OldFilter;
      return row;
    }

    /// <summary>
    /// Пытаемся извлечь числовую часть номера
    /// </summary>
    /// <param name="value">Номер чего-нибудь. Если удалось найти начальную числовую часть, сюда по ссылке помещается обрезанная часть номера</param>
    /// <returns>true, если удалось что-то обрезать</returns>
    private static bool DoGetFirstNumPart(ref string value)
    {
      if (value[0] < '0' || value[0] > '9')
        return false; // нечисловой номер не обрабатываем
      for (int i = 1; i < value.Length; i++)
      {
        if (value[i] < '0' || value[i] > '9')
        {
          // нашли не-цифру
          value = value.Substring(0, i);
          return true;
        }
      }
      return true;
    }

    #endregion

    #endregion

    #region Квартиры (помещения) и комнаты

    private class RoomItem : ItemBase
    {
      #region Конструктор

      public RoomItem(EFPFiasAddressPanel owner,
        TextBox edFlatName, ComboBox cbFlatAOType,
        TextBox edRoomName, ComboBox cbRoomAOType,
        Button btnSel, Button btnClear)
        : base(new NameAndAOType[2]{
          new NameAndAOType(owner, edFlatName, cbFlatAOType, FiasLevel.Flat),
          new NameAndAOType (owner, edRoomName, cbRoomAOType, FiasLevel .Room)
        }, btnSel, btnClear)
      {
        if (!owner.UI.DBSettings.UseRoom)
          btnSel.Visible = false;
      }

      #endregion

      #region Корневой узел для выбора

      protected override void OnParentGuidChanged(bool isInheritable, out bool isPageEmpty)
      {
        if (ParentGuid == Guid.Empty || (!Owner.UI.DBSettings.UseRoom) || (!isInheritable))
        {
          _Page = null;
          isPageEmpty = true;
        }
        else
        {
          EFPApp.BeginWait("Получение справочника помещений", "FiasAddress", true);
          try { _Page = Owner._Handler.GetRoomPage(ParentGuid); }
          finally { EFPApp.EndWait(); }
          NAs[0].Name.Control.AutoCompleteCustomSource.AddRange(_Page.GetFlatNums());
          NAs[1].Name.Control.AutoCompleteCustomSource.AddRange(_Page.GetRoomNums(NAs[0].Name.Text));
          isPageEmpty = _Page.IsEmpty;
        }
      }

      /// <summary>
      /// Таблица, из которой можно делать выбор
      /// </summary>
      private FiasCachedPageRoom _Page;

      #endregion

      #region Выбор из справочника

      protected override void OnButtonSelClick()
      {
        using (DataView dv = _Page.CreateDataView())
        {
          using (OKCancelGridForm form = new OKCancelGridForm())
          {
            form.Text = "Выбор из справочника помещений (HOUSEGUID=" + ParentGuid.ToString() + ")";
            form.NoButtonProvider.Visible = true;

            FiasAddress a1 = new FiasAddress();
            a1.SetGuid(FiasLevel.House, ParentGuid);
            Owner._Handler.FillAddress(a1);
            InfoLabel lbl = form.AddInfoLabel(DockStyle.Top);
            lbl.Text = "Здание: " + Owner._Handler.GetTextWithoutPostalCode(a1);

            EFPFiasListDataGridView efpGrid = new EFPFiasListDataGridView(form.ControlWithToolBar, Owner.UI, FiasTableType.Room, false);

            efpGrid.CommandItems.EnterAsOk = true;

            efpGrid.Control.DataSource = dv;

            FiasFlatType flatType = FiasEnumNames.ParseFlatType(NAs[0].AOType.Text);
            FiasRoomType roomType = FiasEnumNames.ParseRoomType(NAs[1].AOType.Text);

            DataRow row = _Page.FindRow(NAs[0].Name.Text, flatType, NAs[1].Name.Text, roomType);
            if (row == null)
              row = EFPFiasAddressPanel.FindSomeNumRow(dv, "FLATNUM", NAs[0].Name.Text); // 03.11.2020

            efpGrid.CurrentDataRow = row;

            EFPDialogPosition pos = new EFPDialogPosition(NAs[0].Name.Control);

            switch (EFPApp.ShowDialog(form, false, pos))
            {
              case DialogResult.OK:
                row = efpGrid.CurrentDataRow;

                if (row == null)
                  EFPApp.ErrorMessageBox("Строка не выбрана");
                else
                {
                  Guid recId = DataTools.GetGuid(row, "ROOMID");
                  if (recId != Owner.Address.GetRecId(Level))
                    Owner.UpdateAddress(Level, recId);
                }
                break;
              case DialogResult.No:
                Clear();
                break;
              default:
                break;
            }
          }
        }
      }

      #endregion

      #region Обработчики поля "Наименование"

      //protected override void OnLeave(FiasLevel level)
      //{
      //  if (_Page == null)
      //    return;

      //  DataRow row = _Page.FindRow(NAs[0].Name.Text, NAs[1].Name.Text);
      //  if (row != null)
      //    InitForRow(row);
      //}


      #endregion
    }

    #endregion

    #region Текущий адрес

    /// <summary>
    /// Редактируемый адрес. Основное свойство
    /// </summary>
    public FiasAddress Address
    {
      get { return _Address; }
      set
      {
        if (value == null)
          _Address = new FiasAddress();
        else
          _Address = value;

        UpdateAddress(FiasLevel.Unknown, Guid.Empty);
      }
    }
    private FiasAddress _Address;

    private bool _InsideUpdateAddress;

    private void UpdateAddress(FiasLevel level, Guid recId)
    {
      if (ProviderState == EFPControlProviderState.Initialization)
        return; // 27.10.2020

      if (_InsideUpdateAddress)
        return;

      _InsideUpdateAddress = true;
      try
      {
        if (!ReadOnly) // 03.03.2021
          Address.ClearGuids(); // все
        if (level != FiasLevel.Unknown)
        {
#if DEBUG
          if (ReadOnly)
            throw new BugException("ReadOnly=true");
#endif
          Address.SetRecId(level, recId);
          Address.ClearRecIdsBelow(level);
        }
        if (!ReadOnly)
        {
          if (level == FiasLevel.Unknown)
          {
            // Обрезаем нижние уровни адреса
            switch (EditorLevel)
            {
              case FiasEditorLevel.Village: Address.ClearBelow(FiasLevel.Village); break;
              case FiasEditorLevel.Street: Address.ClearBelow(FiasLevel.Street); break;
              //case FiasEditorLevel.House: Address.ClearBelow(FiasLevel.House); break;
              case FiasEditorLevel.House: Address.ClearBelow(FiasLevel.Structure); break; // 25.10.2020
              case FiasEditorLevel.Room:
                // убрано 18.08.2020 Address.ClearBelow(FiasLevel.Flat); 
                break;
              default:
                throw new BugException("EditorLevel=" + EditorLevel.ToString());
            }
          }

          _Handler.FillAddress(Address);
        }


        // Обновляем элементы
        Guid LastG = Guid.Empty;
        FiasLevel LastLevel = FiasLevel.Unknown;
        for (int i = 0; i < _AddrObParts.Count; i++)
        {
          _AddrObParts[i].SetParentGuid(LastG, LastLevel);
          _AddrObParts[i].AddressUpdated();
          if (_Address.GetGuid(_AddrObParts[i].Level) != Guid.Empty)
          {
            LastG = _Address.GetGuid(_AddrObParts[i].Level);
            LastLevel = _AddrObParts[i].Level;
          }
          else if (!_AddrObParts[i].IsEmpty)
          {
            //LastG = Guid.Empty;
            LastG = FiasTools.GuidNotFound; // 03.08.2020 
            LastLevel = FiasLevel.Unknown;
          }
        }

        if (_HousePart != null)
        {
          _HousePart.SetParentGuid(LastG, LastLevel);
          _HousePart.AddressUpdated();
          if (_Address.GetGuid(FiasLevel.House) != Guid.Empty)
          {
            LastG = _Address.GetGuid(FiasLevel.House);
            LastLevel = FiasLevel.House;
          }
          else
          {
            LastG = Guid.Empty;
            LastLevel = FiasLevel.Unknown;
          }
        }

        if (_RoomPart != null)
        {
          _RoomPart.SetParentGuid(LastG, LastLevel);
          _RoomPart.AddressUpdated();

          // Бесполезное действие
          if (_Address.GetGuid(FiasLevel.Flat) != Guid.Empty)
          {
            LastG = _Address.GetGuid(FiasLevel.Flat);
            LastLevel = FiasLevel.Flat;
          }
          else
          {
            LastG = Guid.Empty;
            LastLevel = FiasLevel.Unknown;
          }
        }

        Control.cbManualPostalCode.Checked = !Address.AutoPostalCode;
        if (!_Address.AutoPostalCode)
          Control.edPostalCode.Text = _Address.PostalCode;


        string sText;
        if (Address.IsEmpty)
          sText = "Адрес не задан";
        else
        {
          if (PostalCodeEditable)
            sText = _Handler.GetText(_Address);
          else
            sText = _Handler.GetTextWithoutPostalCode(_Address); // 23.10.2020
          if (_Address.Actuality == FiasActuality.Historical)
            sText = "[Исторический] " + sText;
          else if (!(_Address.Live ?? true))
            sText = "[Недействующий] " + sText;
        }

        _TextView.Text = sText;

        InitErrorMessages();
        InitStatusBarText();

        if (AddressChanged != null)
          AddressChanged(this, EventArgs.Empty);
      }
      finally
      {
        _InsideUpdateAddress = false;
      }
    }

    private void InitErrorMessages()
    {
      ErrorMessageList errors = Address.Messages.Clone();
      AddEditorMessages(Address, EditorLevel, errors, CanBeEmptyMode, CanBePartialMode, MinRefBookLevel);

      _ErrorButton.ErrorMessages = errors;
    }

    internal static void AddEditorMessages(FiasAddress address, FiasEditorLevel editorLevel, ErrorMessageList errors,
      UIValidateState canBeEmptyMode, UIValidateState canBePartialMode, FiasLevel minRefBookLevel)
    {
      // Используется также к EFPAddressComboBox

      if (address.IsEmpty)
      {
        ErrorMessageKind kind = GetErrorOrWarningKind(canBeEmptyMode);
        if (kind != ErrorMessageKind.Info)
          errors.Add(new ErrorMessageItem(kind, "Адрес должен быть заполнен"));
      }
      else
      {
        string errorText;
        FiasLevelCompareResult cmp = address.CompareTo(editorLevel, out errorText);
        switch (cmp)
        {
          case FiasLevelCompareResult.Greater:
            errors.AddError("Заполнены лишние компоненты адреса. " + errorText);
            break;

          case FiasLevelCompareResult.Less:
            ErrorMessageKind kind = GetErrorOrWarningKind(canBePartialMode);
            if (kind != ErrorMessageKind.Info)
              errors.Add(new ErrorMessageItem(kind, "Адрес должен быть заполнен полностью. " + errorText));
            break;
        }

        // Проверка минимального уровня, заполненного из классификатора
        if (!address.IsMinRefBookLevel(minRefBookLevel, out errorText))
          errors.AddError(errorText);
      }
    }

    private static ErrorMessageKind GetErrorOrWarningKind(UIValidateState mode)
    {
      switch (mode)
      {
        case UIValidateState.Error: return ErrorMessageKind.Error;
        case UIValidateState.Warning: return ErrorMessageKind.Warning;
        default: return ErrorMessageKind.Info;
      }
    }

    /// <summary>
    /// Событие вызывается при изменении объекта Address - при установке
    /// снаружи или в процессе редактирования.
    /// </summary>
    public event EventHandler AddressChanged;


    void ManualPostalCodeChanged(object sender, EventArgs args)
    {
      if (_ReadOnly)
        return;
      if (_InsideUpdateAddress)
        return;

      bool changed = false;
      if (efpManualPostalCode.Checked == Address.AutoPostalCode)
      {
        Address.AutoPostalCode = !efpManualPostalCode.Checked;
        changed = true;
      }
      if (efpManualPostalCode.Checked)
      {
        if (efpPostalCode.Text != Address.PostalCode)
        {
          Address.PostalCode = efpPostalCode.Text;
          changed = true;
        }
      }
      if (changed)
        UpdateAddress(FiasLevel.Unknown, Guid.Empty);
    }


    #endregion

    #region Свойства CanBeEmpty и CanBePartial

    /// <summary>
    /// Может ли адрес быть пустым?
    /// По умолчанию - Error - адрес должен быть заполнен.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        if (value == _CanBeEmptyMode)
          return;
        _CanBeEmptyMode = value;
        UpdateAddress(FiasLevel.Unknown, Guid.Empty);
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// Может ли адрес быть пустым?
    /// Дублирует CanBeEmptyMode
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    /// <summary>
    /// Может ли адрес быть заполненным частично (например, введен только регион)?
    /// По умолчанию - Error - адрес должен быть заполнен согласно свойству EditorLevel.
    /// Например, если EditorLevel=Room, то должен быть задан, как минимум, дом.
    /// </summary>
    public UIValidateState CanBePartialMode
    {
      get { return _CanBePartialMode; }
      set
      {
        if (value == _CanBePartialMode)
          return;
        _CanBePartialMode = value;
        UpdateAddress(FiasLevel.Unknown, Guid.Empty);
      }
    }
    private UIValidateState _CanBePartialMode;

    /// <summary>
    /// Может ли адрес быть заполненным частично (например, введен только регион)?
    /// По умолчанию - false - адрес должен быть заполнен согласно свойству EditorLevel.
    /// Например, если EditorLevel=Room, то должен быть задан, как минимум, дом.
    /// Дублирует свойство CanBePartialMode
    /// </summary>
    public bool CanBePartial
    {
      get { return CanBePartialMode != UIValidateState.Error; }
      set { CanBePartialMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Текстовое представление адреса

    //public EFPTextBox TextView { get { return _TextView; } }
    private EFPTextBox _TextView;

    #endregion

    #region Свойство ReadOnly

    /// <summary>
    /// Возвращает true, если установлены свойства Visible и Enabled равны true, а ReadOnly=false
    /// </summary>
    public override bool EnabledState
    {
      get { return Enabled && (!ReadOnly); }
    }

    /// <summary>
    /// Режим "только чтение"
    /// </summary>
    public bool ReadOnly
    {
      get { return _ReadOnly; }
      set
      {
        if (value == _ReadOnly)
          return;
        _ReadOnly = value;
        if (_ReadOnlyEx != null)
          _ReadOnlyEx.Value = value;

        UpdateEnabledState();

        efpPaste.Enabled = !value; // 11.03.2021
        _CmdClear.Enabled = !value;
        _CmdSearch.Enabled = _Handler.AddressSearchEnabled && (!value); // 11.03.2021


        efpManualPostalCode.Enabled = !value;

        Address = Address; // повторная инициализация


        foreach (AddrObItem item in _AddrObParts)
          item.InitEnabled();
        if (_HousePart != null)
          _HousePart.InitEnabled();
        if (_RoomPart != null)
          _RoomPart.InitEnabled();
      }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Управляемое свойство для ReadOnly.
    /// </summary>
    public DepValue<Boolean> ReadOnlyEx
    {
      get
      {
        InitReadOnlyEx();
        return _ReadOnlyEx;
      }
      set
      {
        InitReadOnlyEx();
        _ReadOnlyEx.Source = value;
      }
    }

    private void InitReadOnlyEx()
    {
      if (_ReadOnlyEx == null)
      {
        _ReadOnlyEx = new DepInput<Boolean>(false,ReadOnlyEx_ValueChanged);
        _ReadOnlyEx.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyEx");
      }
    }
    /// <summary>
    /// Выходная часть свойства ReadOnly
    /// </summary>
    private DepInput<Boolean> _ReadOnlyEx;

    private void ReadOnlyEx_ValueChanged(object Sender, EventArgs Args)
    {
      ReadOnly = _ReadOnlyEx.Value;
    }

    #endregion

    #region Кнопки и команды меню "Еще"

    EFPErrorMessageListButton _ErrorButton;

    internal EFPCommandItems MoreCommandItems { get { return _MoreButton.CommandItems; } }
    EFPButtonWithMenu _MoreButton;

    EFPCommandItem _CmdSearch, _CmdDetails, _CmdClear;

    void CmdSearch_Click(object sender, EventArgs args)
    {
      FiasAddress addr = this.Address;
      if (!UI.SearchAddress(ref addr))
        return;

      this.Address = addr;
    }

    void CmdDetails_Click(object sender, EventArgs args)
    {
      UI.ShowDetails(Address);
    }

    void CmdClear_Click(object sender, EventArgs args)
    {
      Address = new FiasAddress();
    }

    void CmdDBSettings_Click(object sender, EventArgs args)
    {
      EFPCommandItem CmdDBSettings = (EFPCommandItem)sender;
      _UI.ShowDBSettings();
    }


    EFPButton efpCopy, efpPaste;

    void efpCopy_Click(object sender, EventArgs args)
    {
      FiasAddressConvert convert = new FiasAddressConvert(_Handler.Source);
      convert.GuidMode = FiasAddressConvertGuidMode.AOGuidWithText; // 10.03.2021
      string s = convert.ToString(Address);
      EFPApp.Clipboard.SetText(s);
    }

    void efpPaste_Click(object sender, EventArgs args)
    {
      string s = EFPApp.Clipboard.GetText(true);
      if (String.IsNullOrEmpty(s))
        return;

      s = DataTools.TrimEndNewLineSeparators(s, true); // 26.10.2020
      if (String.IsNullOrEmpty(s))
        return;

      FiasAddressConvert convert = new FiasAddressConvert(_Handler.Source);
      FiasAddress newAddress;
      if (convert.TryParse(s, out newAddress))
        Address = newAddress;
      else
        EFPApp.ShowTempMessage("В буфере обмена нет правильного адреса");
    }

    #endregion

    #region Статусная строка

    EFPCommandItem _sbPostalCode, _sbOKATO, _sbOKTMO, _sbIFNSFL, _sbIFNSUL;

    private void InitStatusBarItems()
    {
      string sSuffix = Environment.NewLine + Environment.NewLine + "(Двойной щелчок левой кнопки мыши, чтобы скопировать код в буфер обмена)";

      _sbPostalCode = new EFPCommandItem("View", "PostalCode");
      _sbPostalCode.Usage = EFPCommandItemUsage.StatusBar;
      _sbPostalCode.StatusBarText = "_";
      _sbPostalCode.ImageKey = "FIAS.PostalCode";
      _sbPostalCode.ToolTipText = "Почтовый индекс" + sSuffix;
      _sbPostalCode.Click += SB_Click;
      this.CommandItems.Add(_sbPostalCode);

      if (_UI.DBSettings.UseOKATO)
      {
        _sbOKATO = new EFPCommandItem("View", "OKATO");
        _sbOKATO.Usage = EFPCommandItemUsage.StatusBar;
        _sbOKATO.StatusBarText = "ОКАТО";
        _sbOKATO.ToolTipText = "Код ОКАТО" + sSuffix;
        _sbOKATO.Click += SB_Click;
        this.CommandItems.Add(_sbOKATO);
      }

      if (_UI.DBSettings.UseOKTMO)
      {
        _sbOKTMO = new EFPCommandItem("View", "OKTMO");
        _sbOKTMO.Usage = EFPCommandItemUsage.StatusBar;
        _sbOKTMO.StatusBarText = "ОКАТО";
        _sbOKTMO.ToolTipText = "Код ОКАТО" + sSuffix;
        _sbOKTMO.Click += SB_Click;
        this.CommandItems.Add(_sbOKTMO);
      }

      if (_UI.DBSettings.UseIFNS)
      {
        _sbIFNSFL = new EFPCommandItem("View", "IFNSFL");
        _sbIFNSFL.Usage = EFPCommandItemUsage.StatusBar;
        _sbIFNSFL.StatusBarText = "ФЛ";
        _sbIFNSFL.ToolTipText = "Код ИФНС физического лица" + sSuffix;
        _sbIFNSFL.Click += SB_Click;
        this.CommandItems.Add(_sbIFNSFL);

        _sbIFNSUL = new EFPCommandItem("View", "IFNSUL");
        _sbIFNSUL.Usage = EFPCommandItemUsage.StatusBar;
        _sbIFNSUL.StatusBarText = "ЮЛ";
        _sbIFNSUL.ToolTipText = "Код ИФНС юридического лица" + sSuffix;
        _sbIFNSUL.Click += SB_Click;
        this.CommandItems.Add(_sbIFNSUL);
      }
    }

    private void InitStatusBarText()
    {
      if (_sbPostalCode != null)
        InitSBValue(_sbPostalCode, String.Empty, Address.FiasPostalCode);
      if (_sbOKATO != null)
        InitSBValue(_sbOKATO, "ОКАТО: ", Address.OKATO);
      if (_sbOKTMO != null)
        InitSBValue(_sbOKTMO, "ОКТМО: ", Address.OKTMO);
      if (_sbIFNSFL != null)
        InitSBValue(_sbIFNSFL, "ФЛ: ", Address.IFNSFL);
      if (_sbIFNSUL != null)
        InitSBValue(_sbIFNSUL, "ЮЛ: ", Address.IFNSUL);
    }

    private void InitSBValue(EFPCommandItem sb, string prefix, string value)
    {
      if (String.IsNullOrEmpty(value))
        sb.StatusBarText = prefix + "?";
      else
        sb.StatusBarText = prefix + value;
      sb.Tag = value;
      sb.Enabled = !String.IsNullOrEmpty(value);
    }

    private static void SB_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      string s = DataTools.GetString(ci.Tag);
      if (String.IsNullOrEmpty(s))
        EFPApp.ShowTempMessage("Код не определен");
      else
        Clipboard.SetText(s);
    }

    #endregion

  }
}
