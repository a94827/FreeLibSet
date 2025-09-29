// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Controls;
using FreeLibSet.DependedValues;
using FreeLibSet.UICore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Провайдер управляющего элемента для ввода числа.
  /// Базовый класс
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class EFPNumEditBoxBase<T> : EFPSyncControl<NumEditBoxBase<T>>, IEFPReadOnlyControl, IEFPSimpleTextBox
      where T : struct, IFormattable, IComparable<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер для управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPNumEditBoxBase(EFPBaseProvider baseProvider, NumEditBoxBase<T> control)
      : base(baseProvider, control, true)
    {
      _SavedNValue = control.NValue;
      _CanBeEmptyMode = UIValidateState.Error;

      if (!DesignMode)
        control.TextChanged += new EventHandler(Control_TextChanged); // 23.11.2021
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Возвращает true, если установлены свойства Enabled=true и ReadOnly=false.
    /// </summary>
    public override bool EnabledState
    {
      get { return Enabled && (!ReadOnly); }
    }

    /// <summary>
    /// Вызывается при изменении значения свойства EnabledState.
    /// Инициализация "серого" значения.
    /// </summary>
    protected override void OnEnabledStateChanged()
    {
      base.OnEnabledStateChanged();
      if (AllowDisabledValue && EnabledState)
        _HasSavedNValue = true;
      InitControlValue();
    }

    /// <summary>
    /// Создает EFPTextBoxCommandItems()
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      if (EFPApp.EasyInterface)
        return base.CreateCommandItems();
      else
        return new EFPTextBoxCommandItems(this, false);
    }

    /// <summary>
    /// Установка 0 при показе формы
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();

      // Если поле не заполнено, но пустые значения запрещены, устанавливается значение 0.

      if ((!CanBeEmpty) && (!NValue.HasValue) && EnabledState)
        NValue = default(T);
    }

    #endregion

    #region Свойства Value/NValue

    // Nullable-свойство NValue является основным, а Value - подчиненным

    #region NValue

    /// <summary>
    /// Доступ к выбранной дате.
    /// Значение null используется, если число не введено.
    /// Если пользователь ввел текст, который нельзя преобразовать в число, также возвращается null.
    /// </summary>
    public T? NValue
    {
      get { return Control.NValue; }
      set
      {
        _SavedNValue = value;
        _HasSavedNValue = true;
        InitControlValue();
      }
    }

    /// <summary>
    /// Управляемое свойство NValue.
    /// </summary>
    public DepValue<T?> NValueEx
    {
      get
      {
        InitNValueEx();
        return _NValueEx;
      }
      set
      {
        InitNValueEx();
        _NValueEx.Source = value;
      }
    }
    private DepInput<T?> _NValueEx;

    private void InitNValueEx()
    {
      if (_NValueEx == null)
      {
        _NValueEx = new DepInput<T?>(NValue, NValueEx_ValueChanged);
        _NValueEx.OwnerInfo = new DepOwnerInfo(this, "NValueEx");
      }
    }

    void NValueEx_ValueChanged(object sender, EventArgs args)
    {
      // См. описание в классе EFPDateTimeControl<>.

      if (!_InsideValueChanged)
        NValue = _NValueEx.Value;
    }

    #endregion

    #region Value

    /// <summary>
    /// Доступ к выбранному числу без значения null.
    /// Если поле не заполнено или пользователь ввел текст, который нельзя преобразовать в число, возвращается 0.
    /// </summary>
    public T Value
    {
      get { return NValue ?? default(T); }
      set { NValue = value; }
    }

    /// <summary>
    /// Управляемое свойство Value.
    /// </summary>
    public DepValue<T> ValueEx
    {
      get
      {
        InitValueEx();
        return _ValueEx;
      }
      set
      {
        InitValueEx();
        _ValueEx.Source = value;
      }
    }
    private DepInput<T> _ValueEx;

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        _ValueEx = new DepInput<T>(Value, ValueEx_ValueChanged);
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.CheckValue += ValueEx_CheckValue;
      }
    }

    void ValueEx_CheckValue(object sender, DepInputCheckEventArgs<T> args)
    {
      if ((!NValue.HasValue) && args.CurrValue.Equals(default(T)))
        args.Forced = true;
    }

    void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      // См. комментарий в методе NValueEx_ValueChanged()

      if (!_InsideValueChanged)
        Value = _ValueEx.Value;
    }

    #endregion

    #region InitControlValue()

    /// <summary>
    /// Сохраняемое значение после установки свойства "Value".
    /// Используется, если в данный момент используется DisabledValue.
    /// </summary>
    private T? _SavedNValue;

    /// <summary>
    /// Поле получает значение true, если было установлено поле _SavedNValue.
    /// После переноса значения в управляющий элемент вызовом InitControlValue(), поле сбрасывается в false.
    /// </summary>
    private bool _HasSavedNValue;

    /// <summary>
    /// Инициализация DateBox.Value.
    /// </summary>
    protected void InitControlValue()
    {
      // Не нужно, иначе может не обновляться
      //if (InsideValueChanged)
      //  return;
      if (AllowDisabledValue && (!EnabledState))
        Control.NValue = DisabledNValue;
      else if (_HasSavedNValue)
      {
        _HasSavedNValue = false; // сбрасываем признак сохраненного значения
        Control.NValue = _SavedNValue;
      }
    }

    #endregion

    #region OnValueChanged()

    /// <summary>
    /// Обработчик события DateBox.ValueChanged
    /// </summary>
    /// <param name="sender">Ссылка на DateBox. Игнорируется</param>
    /// <param name="args">Игнорируется</param>
    private void Control_TextChanged(object sender, EventArgs args)
    {
      try
      {
        if (!_InsideValueChanged) // предотвращение реентрантного вызова метода OnValueChanged()
        {
          _InsideValueChanged = true; // Не нужно, чтобы при установке NValueEx/ValueEx.Value рекурсивно устанавливались NValue и Value. Это бы привело к порче значения null.
          try
          {
            OnValueChanged(); // виртуальный метод
          }
          finally
          {
            _InsideValueChanged = false;
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    /// <summary>
    /// Флажок для предотвращения рекурсивного вызова OnValueChanged() или установки свойст NValue/Value из NValueEx/ValueEx.
    /// </summary>
    private bool _InsideValueChanged;

    /// <summary>
    /// Метод вызывается при изменении значения в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод.
    /// </summary>
    protected virtual void OnValueChanged()
    {
      // Поле _InsideValueChanged=true

      if (_NValueEx != null)
        _NValueEx.Value = NValue;
      if (_ValueEx != null)
        _ValueEx.Value = Value;

      if (AllowDisabledValue && EnabledState)
        _SavedNValue = NValue;

      Validate();
      OnSyncValueChanged();
    }

    #endregion

    #endregion

    #region IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство, которое возвращает true, если есть введенное значение (NValue.HasValue=true).
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
          _IsNotEmptyEx = new DepExpr1<bool, T?>(NValueEx, CalcIsNotEmpty);
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmpty(T? value)
    {
      return value.HasValue;
    }

    #endregion

    #region Свойства DisabledValue/DisabledNValue

    #region AllowDisabledValue

    /// <summary>
    /// Разрешает использование свойства DisabledValue или DisabledNValue.
    /// По умолчанию - false
    /// </summary>
    public bool AllowDisabledValue
    {
      get { return _AllowDisabledValue; }
      set
      {
        if (value == _AllowDisabledValue)
          return;
        _AllowDisabledValue = value;
        InitControlValue();
      }
    }
    private bool _AllowDisabledValue;

    #endregion

    #region DisabledNValue

    /// <summary>
    /// Значение, которое показывается в управляющем элементе при EnabledState=false и AllowDisabledValue=true.
    /// Если это значение равно null, то поле ввода будет показано пустым.
    /// </summary>
    public T? DisabledNValue
    {
      get { return _DisabledNValue; }
      set
      {
        if (value.Equals(_DisabledNValue))
          return;

        _DisabledNValue = value;
        if (_DisabledNValueEx != null)
          _DisabledNValueEx.Value = value;
        if (_DisabledValueEx != null)
          _DisabledValueEx.Value = DisabledValue;
        InitControlValue();
      }
    }
    private T? _DisabledNValue;

    /// <summary>
    /// Управляемое свойство DisabledNValue.
    /// Свойство действует при установленном свойстве AllowDisabledValue
    /// </summary>
    public DepValue<T?> DisabledNValueEx
    {
      get
      {
        InitDisabledNValueEx();
        return _DisabledNValueEx;
      }
      set
      {
        InitDisabledNValueEx();
        _DisabledNValueEx.Source = value;
      }
    }
    private DepInput<T?> _DisabledNValueEx;

    private void InitDisabledNValueEx()
    {
      if (_DisabledNValueEx == null)
      {
        _DisabledNValueEx = new DepInput<T?>(DisabledNValue, DisabledNValueEx_ValueChanged);
        _DisabledNValueEx.OwnerInfo = new DepOwnerInfo(this, "DisabledNValueEx");
      }
    }

    private void DisabledNValueEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledNValue = _DisabledNValueEx.Value;
    }

    #endregion

    #region DisabledValue

    /// <summary>
    /// Значение, которое показывается при EnabledState=false и AllowDisabledValue=true.
    /// Это свойство дублирует DisabledNValue. Если DisabledNValue=null, то возвращает DateTime.MinValue.
    /// </summary>
    public T DisabledValue
    {
      get { return DisabledNValue ?? default(T); }
      set { DisabledNValue = value; }
    }

    /// <summary>
    /// Управляемое свойство DisabledValue.
    /// Свойство действует при установленном свойстве AllowDisabledValue.
    /// </summary>
    public DepValue<T> DisabledValueEx
    {
      get
      {
        InitDisabledValueEx();
        return _DisabledValueEx;
      }
      set
      {
        InitDisabledValueEx();
        _DisabledValueEx.Source = value;
      }
    }
    private DepInput<T> _DisabledValueEx;

    private void InitDisabledValueEx()
    {
      if (_DisabledValueEx == null)
      {
        _DisabledValueEx = new DepInput<T>(DisabledValue, DisabledValueEx_ValueChanged);
        _DisabledValueEx.OwnerInfo = new DepOwnerInfo(this, "DisabledValueEx");
      }
    }

    private void DisabledValueEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledValue = _DisabledValueEx.Value;
    }

    #endregion

    #endregion

    #region Свойство ReadOnly

    /// <summary>
    /// Режим "только чтение"
    /// </summary>
    public bool ReadOnly
    {
      get { return Control.ReadOnly; }
      set
      {
        if (value == Control.ReadOnly)
          return;
        Control.ReadOnly = value;
        if (_ReadOnlyEx != null)
          _ReadOnlyEx.Value = value;
        UpdateEnabledState();
      }
    }

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
        _ReadOnlyMain.Source = value;
      }
    }

    private void InitReadOnlyEx()
    {
      if (_ReadOnlyEx == null)
      {
        _ReadOnlyEx = new DepInput<Boolean>(false, ReadOnlyEx_ValueChanged);
        _ReadOnlyEx.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyEx");

        _ReadOnlyMain = new DepInput<bool>(false, null);
        _ReadOnlyMain.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyMain");

        _NotReadOnlySync = new DepInput<bool>(true, null);
        _NotReadOnlySync.OwnerInfo = new DepOwnerInfo(this, "NotReadOnlySync");

        DepOr readOnlyOr = new DepOr(_ReadOnlyMain, new DepNot(_NotReadOnlySync));
        _ReadOnlyEx.Source = readOnlyOr;
      }
    }
    /// <summary>
    /// Выходная часть свойства ReadOnly
    /// </summary>
    private DepInput<Boolean> _ReadOnlyEx;
    /// <summary>
    /// Основной вход для ReadOnly
    /// </summary>
    private DepInput<Boolean> _ReadOnlyMain;
    /// <summary>
    /// Дополнительный вход для ReadOnly для выполнения синхронизации
    /// </summary>
    private DepInput<Boolean> _NotReadOnlySync;

    private void ReadOnlyEx_ValueChanged(object sender, EventArgs args)
    {
      ReadOnly = _ReadOnlyEx.Value;
    }

    #endregion

    #region Свойство CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - <see cref="UIValidateState.Error"/>.
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

    #region Диапазон допустимых значений

    /// <summary>
    /// Минимальная дата, которую можно ввести.
    /// Если значение свойства установлено и свойство Value меньше заданной даты, будет выдана ошибка
    /// при проверке контроля.
    /// По умолчанию ограничение не установлено
    /// </summary>
    public T? Minimum
    {
      get { return Control.Minimum; }
      set
      {
        Control.Minimum = value;
        Validate();
      }
    }

    /// <summary>
    /// Максимальная дата, которую можно ввести.
    /// Если значение свойства установлено и свойство Value больше заданной даты, будет выдана ошибка
    /// при проверке контроля
    /// По умолчанию ограничение не установлено
    /// </summary>
    public T? Maximum
    {
      get { return Control.Maximum; }
      set
      {
        Control.Maximum = value;
        Validate();
      }
    }

    #endregion

    #region Проверка значения

    /// <summary>
    /// Проверка корректности значения.
    /// Выдает сообщение об ошибке, если дата не входит в диапазон значений {Minimum, Maximum}.
    /// Если дата не введена, а CanBeEmpty=false, также устанавливается ошибка.
    /// </summary>
    protected override void OnValidate()
    {
      if (!Control.TextIsValid)
      {
        SetError(UITools.ConvertErrorMessage(Control.Text, typeof(T)));
        return;
      }

      base.OnValidate();


      if (NValue.HasValue)
        UITools.ValidateInRange<T>(Value, Minimum, Maximum, this, DisplayName, true, Control.Format, Control.FormatProvider);
      else
        ValidateCanBeEmptyMode(CanBeEmptyMode);
    }

    #endregion

    #region Синхронизация

    /// <summary>
    /// Синронизированное значение.
    /// Свойство NValue.
    /// </summary>
    public override object SyncValue
    {
      get { return NValue; }
      set
      {
        if (value == null)
          NValue = null;
        else
          //NValue = (T)value; 
          NValue = (T)(Convert.ChangeType(value, typeof(T)));
      }
    }

    #endregion

    #region IEFPSimpleTextBox Members

    string IEFPSimpleTextBox.Text
    {
      get { return Control.Text; }
      set { Control.Text = value; }
    }

    int IEFPSimpleTextBox.TextLength { get { return Control.Text.Length; } }

    int IEFPSimpleTextBox.SelectionStart
    {
      get { return Control.SelectionStart; }
      set { Control.SelectionStart = value; }
    }

    int IEFPSimpleTextBox.SelectionLength
    {
      get { return Control.SelectionLength; }
      set { Control.SelectionLength = value; }
    }

    string IEFPSimpleTextBox.SelectedText
    {
      get { return Control.SelectedText; }
      set { Control.SelectedText = value; }
    }

    void IEFPSimpleTextBox.Select(int start, int length)
    {
      Control.Select(start, length);
    }

    void IEFPSimpleTextBox.SelectAll()
    {
      Control.SelectAll();
    }

    #endregion
  }

  /// <summary>
  /// Провайдер управляющего элемента для ввода числа.
  /// </summary>
  public class EFPIntEditBox : EFPNumEditBoxBase<Int32>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер для управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPIntEditBox(EFPBaseProvider baseProvider, Int32EditBox control)
      : base(baseProvider, control)
    {
    }

    #endregion
  }

  /// <summary>
  /// Провайдер управляющего элемента для ввода числа.
  /// </summary>
  public class EFPSingleEditBox : EFPNumEditBoxBase<Single>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер для управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPSingleEditBox(EFPBaseProvider baseProvider, SingleEditBox control)
      : base(baseProvider, control)
    {
    }

    #endregion
  }

  /// <summary>
  /// Провайдер управляющего элемента для ввода числа.
  /// </summary>
  public class EFPDoubleEditBox : EFPNumEditBoxBase<Double>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер для управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPDoubleEditBox(EFPBaseProvider baseProvider, DoubleEditBox control)
      : base(baseProvider, control)
    {
    }

    #endregion
  }

  /// <summary>
  /// Провайдер управляющего элемента для ввода числа.
  /// </summary>
  public class EFPDecimalEditBox : EFPNumEditBoxBase<Decimal>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер для управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPDecimalEditBox(EFPBaseProvider baseProvider, DecimalEditBox control)
      : base(baseProvider, control)
    {
    }

    #endregion
  }
}
