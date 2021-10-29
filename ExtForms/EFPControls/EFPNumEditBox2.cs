using FreeLibSet.Controls;
using FreeLibSet.DependedValues;
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
  public class EFPNumEditBoxBase<T> : EFPSyncControl<NumEditBoxBase<T>>
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

      if (!DesignMode)
        control.ValueChanged += new EventHandler(Control_ValueChanged);
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
        _NValueEx = new DepInput<T?>();
        _NValueEx.OwnerInfo = new DepOwnerInfo(this, "NValueEx");
        _NValueEx.Value = NValue;
        _NValueEx.ValueChanged += new EventHandler(NValueEx_ValueChanged);
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
        _ValueEx = new DepInput<T>();
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
        _ValueEx.Value = Value;
        _ValueEx.ValueChanged += new EventHandler(ValueEx_ValueChanged);
      }
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
    private void Control_ValueChanged(object sender, EventArgs args)
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
        EFPApp.ShowException(e, "Ошибка обработчика NumEditBox.ValueChanged");
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
      DoSyncValueChanged();
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
        _DisabledNValueEx = new DepInput<T?>();
        _DisabledNValueEx.OwnerInfo = new DepOwnerInfo(this, "DisabledNValueEx");
        _DisabledNValueEx.Value = DisabledNValue;
        _DisabledNValueEx.ValueChanged += new EventHandler(DisabledNValueEx_ValueChanged);
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
        _DisabledValueEx = new DepInput<T>();
        _DisabledValueEx.OwnerInfo = new DepOwnerInfo(this, "DisabledValueEx");
        _DisabledValueEx.Value = DisabledValue;
        _DisabledValueEx.ValueChanged += new EventHandler(DisabledValueEx_ValueChanged);
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
        _ReadOnlyEx = new DepInput<Boolean>();
        _ReadOnlyEx.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyEx");
        _ReadOnlyEx.Value = false;
        _ReadOnlyEx.ValueChanged += new EventHandler(ReadOnlyEx_ValueChanged);

        _ReadOnlyMain = new DepInput<bool>();
        _ReadOnlyMain.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyMain");
        _ReadOnlyMain.Value = false;

        _NotReadOnlySync = new DepInput<bool>();
        _NotReadOnlySync.OwnerInfo = new DepOwnerInfo(this, "NotReadOnlySync");
        _NotReadOnlySync.Value = true;

        DepOr ReadOnlyOr = new DepOr(_ReadOnlyMain, new DepNot(_NotReadOnlySync));
        _ReadOnlyEx.Source = ReadOnlyOr;
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
    /// Допускается ли значение null.
    /// Значение совпадает со свойством DateBox.ClearButton, которое по умолчанию имеет значение false.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set
      {
        if (value == _CanBeEmpty)
          return;
        _CanBeEmpty = value;
        if (_CanBeEmptyEx != null)
          _CanBeEmptyEx.Value = value;
        Validate();
      }
    }
    private bool _CanBeEmpty;

    /// <summary>.
    /// Управляемое свойство для CanBeEmpty.
    /// True, если можно выбирать значение null. По умолчанию - false.
    /// </summary>
    public DepValue<Boolean> CanBeEmptyEx
    {
      get
      {
        InitCanBeEmptyEx();
        return _CanBeEmptyEx;
      }
      set
      {
        InitCanBeEmptyEx();
        _CanBeEmptyEx.Source = value;
      }
    }
    private DepInput<Boolean> _CanBeEmptyEx;

    private void InitCanBeEmptyEx()
    {
      if (_CanBeEmptyEx == null)
      {
        _CanBeEmptyEx = new DepInput<bool>();
        _CanBeEmptyEx.OwnerInfo = new DepOwnerInfo(this, "CanBeEmptyEx");
        _CanBeEmptyEx.Value = CanBeEmpty;
        _CanBeEmptyEx.ValueChanged += new EventHandler(CanBeEmptyEx_ValueChanged);
      }
    }

    void CanBeEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      CanBeEmpty = _CanBeEmptyEx.Value;
    }

    #endregion

    #region Свойство WarningIfEmpty

    /// <summary>
    /// Выдавать предупреждение, если текст не введен (при условии, что CanBeEmpty=true).
    /// </summary>
    public bool WarningIfEmpty
    {
      get { return _WarningIfEmpty; }
      set
      {
        if (value == _WarningIfEmpty)
          return;
        _WarningIfEmpty = value;
        if (_WarningIfEmptyEx != null)
          _WarningIfEmptyEx.Value = value;
        Validate();
      }
    }
    private bool _WarningIfEmpty;

    /// <summary>
    /// Управляемое свойство для WarningIfEmpty.
    /// Если True и свойство CanBeEmpty=True, то при проверке состояния выдается
    /// предупреждение, если свойство Value=null.
    /// По умолчанию - False.
    /// </summary>
    public DepValue<Boolean> WarningIfEmptyEx
    {
      get
      {
        InitWarningIfEmptyEx();
        return _WarningIfEmptyEx;
      }
      set
      {
        InitWarningIfEmptyEx();
        _WarningIfEmptyEx.Source = value;
      }
    }
    private DepInput<Boolean> _WarningIfEmptyEx;

    private void InitWarningIfEmptyEx()
    {
      if (_WarningIfEmptyEx == null)
      {
        _WarningIfEmptyEx = new DepInput<bool>();
        _WarningIfEmptyEx.OwnerInfo = new DepOwnerInfo(this, "WarningIfEmptyEx");
        _WarningIfEmptyEx.Value = WarningIfEmpty;
        _WarningIfEmptyEx.ValueChanged += new EventHandler(WarningIfEmptyEx_ValueChanged);
      }
    }

    void WarningIfEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      WarningIfEmpty = _WarningIfEmptyEx.Value;
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

    /// <summary>
    /// Если свойство установлено в true, а введенное значение выходит за диапазон, заданный свойствами
    /// Minimum и Maximum, то при проверке выдается предупреждение, а не ошибка.
    /// По умолчанию - false (выдача ошибки)
    /// </summary>
    public virtual bool WarningIfOutOfRange
    {
      get { return _WarningIfOutOfRange; }
      set { _WarningIfOutOfRange = value; }
    }
    private bool _WarningIfOutOfRange;

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
        SetError("Введенный текст нельзя преобразовать " + (Control.DecimalPlaces == 0 ? "в целое число" : "в число"));
        return;
      }

      string ErrorMessage = null;

      if (NValue.HasValue)
      {
        if (Minimum.HasValue)
        {
          if (Maximum.HasValue)
          {
            if (Value.CompareTo(Minimum.Value) < 0 || Value.CompareTo(Maximum.Value) > 0)
              ErrorMessage = "Значение должно быть в диапазоне от " + Minimum.Value.ToString() + " до " + Maximum.Value.ToString();
          }
          else
          {
            if (Value.CompareTo(Minimum.Value) < 0)
              ErrorMessage = "Значение должно быть не меньше " + Minimum.Value.ToString();
          }
        }
        else
        {
          if (Maximum.HasValue)
          {
            if (Value.CompareTo(Maximum.Value) > 0)
              ErrorMessage = "Значение должно не больше " + Maximum.Value.ToString();
          }
        }

        if (ErrorMessage != null)
        {
          if (WarningIfOutOfRange)
            SetWarning(ErrorMessage);
          else
            SetError(ErrorMessage);
        }
      }
      else
      {
        if (CanBeEmpty)
        {
          if (WarningIfEmpty)
            SetWarning("Поле \"" + DisplayName + "\" , вероятно, должно быть заполнено");
        }
        else
          SetError("Поле \"" + DisplayName + "\" должно быть заполнено");
      }

      base.OnValidate();

      base.OnValidate();
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
          NValue = (T)value; // TODO: Преобразования между разными типами
      }
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
    public EFPIntEditBox(EFPBaseProvider baseProvider, IntEditBox control)
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
