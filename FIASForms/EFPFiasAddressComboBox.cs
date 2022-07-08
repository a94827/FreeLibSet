// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.FIAS;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.FIAS
{
  /// <summary>
  /// Провайдер комбоблока для выбора адреса.
  /// В текстовом поле отображается выбранный адрес, а при открытии выпадающего списка показывается диалог редактирования адреса FiasAddressDialog
  /// </summary>
  public class EFPFiasAddressComboBox : EFPUserSelComboBox
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер комбоблока
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент - комбоблок</param>
    /// <param name="ui">Интерфейс доступа к адресам</param>
    public EFPFiasAddressComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, FiasUI ui)
      : base(baseProvider, control)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;
      _EditorLevel = FiasTools.DefaultEditorLevel;
      _PostalCodeEditable = true;
      _MinRefBookLevel = FiasTools.DefaultMinRefBookLevel;
      _CanBeEmptyMode = UIValidateState.Error;
      _CanBePartialMode = UIValidateState.Error;

      _TextFormat = FiasFormatStringParser.DefaultFormat;
      _Address = new FiasAddress();
      Control.PopupClick += new EventHandler(Control_PopupClick);
      Control.ClearClick += new EventHandler(Control_ClearClick);
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Возвращает "Адрес".
    /// </summary>
    protected override string DefaultDisplayName { get { return "Адрес"; } }

    /// <summary>
    /// Интерфейс доступа к адресам.
    /// Задается в конструкторе
    /// </summary>
    public FiasUI UI { get { return _UI; } }
    private FiasUI _UI;

    /// <summary>
    /// Уровень, до которого можно вводить адрес.
    /// По умолчанию - FiasLevel.Room.
    /// </summary>
    public FiasEditorLevel EditorLevel
    {
      get { return _EditorLevel; }
      set
      {
        _EditorLevel = value;
        this.Address = Address; // Инициализация текста
      }
    }
    private FiasEditorLevel _EditorLevel;

    /// <summary>
    /// Можно ли редактировать почтовый индекс?
    /// По умолчанию - true
    /// </summary>
    public bool PostalCodeEditable
    {
      get { return _PostalCodeEditable; }
      set { _PostalCodeEditable = value; }
    }
    private bool _PostalCodeEditable;

    /// <summary>
    /// Задает минимальный уровень адреса, который должен быть выбран из справочника, а не задан вручную.
    /// По умолчанию - FiasLevel.City, то есть регион, район и город должны быть в справочнике ФИАС, а населенный пункт,
    /// при необходимости - введен вручную, если его нет в справочнике.
    /// Значение Unknown отключает все проверки. 
    /// Допускаются любые значения, включая House и Room, если они не выходят за пределы FiasEditorLevel.
    /// </summary>
    public FiasLevel MinRefBookLevel
    {
      get { return _MinRefBookLevel; }
      set
      {
        if (value == _MinRefBookLevel)
          return;
        _MinRefBookLevel = value;
        Validate();
      }
    }
    private FiasLevel _MinRefBookLevel;

    /// <summary>
    /// Формат вывода текста адреса.
    /// По умолчанию - "TEXT"
    /// </summary>
    public string TextFormat
    {
      get { return _TextFormat; }
      set
      {
        string errorMessage;
        if (!FiasFormatStringParser.IsValidFormat(value, out errorMessage))
          throw new ArgumentException("Неправильный формат для текстового вывода адреса. " + errorMessage);
        _TextFormat = value;

        InitTextAndImage();
      }
    }
    private string _TextFormat;

    /// <summary>
    /// Редактируемый адрес
    /// </summary>
    public FiasAddress Address
    {
      get { return _Address; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Address = value;

        InitTextAndImage();

        Validate();
      }
    }
    private FiasAddress _Address;

    private void InitTextAndImage()
    {
      EFPComboBoxTextValueNeededEventArgs args = new EFPComboBoxTextValueNeededEventArgs();
      if (!_Address.IsEmpty)
        args.ImageKey = "FIAS.Address";
      FiasHandler handler = new FiasHandler(_UI.Source);
      args.TextValue = handler.Format(Address, TextFormat);
      base.InitTextAndImage(args);
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
        Validate();
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
        Validate();
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

    #region Обработчики комбоблока

    void Control_PopupClick(object sender, EventArgs args)
    {
      FiasAddressDialog dlg = new FiasAddressDialog(_UI);
      dlg.Title = DisplayName;
      dlg.DialogPosition.PopupOwnerControl = Control;
      dlg.EditorLevel = EditorLevel;
      dlg.PostalCodeEditable = PostalCodeEditable;
      dlg.Address = Address;
      if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        return;
      this.Address = dlg.Address;
    }

    void Control_ClearClick(object sender, EventArgs args)
    {
      Address = new FiasAddress();
    }

    #endregion

    #region Проверка

    /// <summary>
    /// Проверка
    /// </summary>
    protected override void OnValidate()
    {
      ErrorMessageList errors = Address.Messages.Clone();

      EFPFiasAddressPanel.AddEditorMessages(Address, EditorLevel, errors, CanBeEmptyMode, CanBePartialMode, MinRefBookLevel);

      switch (errors.Severity)
      {
        case ErrorMessageKind.Error:
          SetError(errors.AllText);
          break;
        case ErrorMessageKind.Warning:
          SetWarning(errors.AllText);
          break;
      }
    }

    #endregion
  }
}
