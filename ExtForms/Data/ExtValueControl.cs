// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;
using System.Drawing;
using FreeLibSet.DependedValues;
using FreeLibSet.Core;
using FreeLibSet.Data;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Data
{
  /// <summary>
  /// Какие действия следует предпринимать при записи значения, когда <see cref="ExtValueControlBase.UserEnabled"/>=false.
  /// Используется свойством <see cref="ExtValueControlBase.UserDisabledMode"/>
  /// </summary>
  public enum ExtValueUserDisabledMode
  {
    /// <summary>
    /// Никаких действий не предпринимать.
    /// Сохраняется значение или значения, которые были на момент открытия редактора.
    /// Предполагается, что значение корректируется, например, в обработчике DocumentEditor.BeforeWrite.
    /// </summary>
    KeepOriginal,

    /// <summary>
    /// Если редактор находится в режиме редактирования "серого" значения, то сохраняются значения, которые
    /// были на момент открытия редактора.
    /// Если редактируется обычное значение, то берется значение из управляющего элемента, в предположении, что оно является вычисленным на основании других полей.
    /// </summary>
    KeepOriginalIfGrayed,

    /// <summary>
    /// Значение берется из управляющего элемента, в предположении, что оно является вычисленным на основании других полей.
    /// </summary>
    AlwaysReplace,
  }


  /// <summary>
  /// Шаблон заготовки переходника для конкретных управляющих элементов.
  /// </summary>
  public abstract class ExtValueControlBase
  {
    #region Конструктор

    /// <summary>
    /// Создает объект          
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="isReadOnly">Сюда передается свойство DBxDocValue.IsReadOnly</param>
    /// <param name="grayedValue">Сюда передается свойство DBxDocValue.Grayed в сочетании с дополнительными условиями.
    /// Если true, а <paramref name="isReadOnly"/>=false, то создается дополнительный управляющий элемент CheckBox</param>
    public ExtValueControlBase(IEFPControl controlProvider, bool isReadOnly, bool grayedValue)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");
      _ControlProvider = controlProvider;

      _UserEnabled = true;
      _UserDisabledMode = ExtValueUserDisabledMode.KeepOriginal;

      _EnabledEx = new DepOutput<Boolean>(true);
      _EnabledEx.OwnerInfo = new DepOwnerInfo(this, "EnabledEx");

      if (grayedValue)
      {
        if (isReadOnly)
        {
          _GrayedEx = new DepConst<Boolean>(true);
          _GrayedEx.OwnerInfo = new DepOwnerInfo(this, "GrayedEx");
        }
        else
          CreateGrayCheckBox();
      }
      else
      {
        _GrayedEx = new DepConst<Boolean>(false);
        _GrayedEx.OwnerInfo = new DepOwnerInfo(this, "GrayedEx");
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public IEFPControl ControlProvider { get { return _ControlProvider; } }
    private IEFPControl _ControlProvider;

    /// <summary>
    /// Значение EnabledEx, зависящее от DocValue и от <see cref="UserEnabledEx"/>.
    /// </summary>
    public DepValue<Boolean> EnabledEx { get { return _EnabledEx; } }
    private DepOutput<Boolean> _EnabledEx;

    /// <summary>
    /// Блокировка редактирования поля прикладным модулем.
    /// Установка свойства в false делает связанное поле ввода недоступным.
    /// Для условной блокировки можно использовать свойство <see cref="UserEnabledEx"/>, что обычно требуется чаще, чем перманентная блокировка поля.
    /// </summary>
    public bool UserEnabled
    {
      get { return _UserEnabled; }
      set
      {
        if (value == _UserEnabled)
          return;
        _UserEnabled = value;
        InitEnabled();
        if (efpGrayCheckBox != null)
          efpGrayCheckBox.Enabled = value;
      }
    }
    private bool _UserEnabled;

    /// <summary>
    /// Блокировка редактирования поля прикладным модулем.
    /// Установка свойства в false делает связанное поле ввода недоступным.
    /// Условная блокировка, которой можно управлять в зависимости от состояния
    /// других элементов редактирования.
    /// Чтобы заблокированное поле очищалось при записи документа, используйте свойство <see cref="UserDisabledMode"/> или
    /// реализуйте обработчик записи данных.
    /// </summary>
    public DepValue<bool> UserEnabledEx
    {
      get
      {
        ReadyUserEnabledEx();
        return _UserEnabledEx;
      }
      set
      {
        ReadyUserEnabledEx();
        _UserEnabledEx.Source = value;
      }
    }

    private void ReadyUserEnabledEx()
    {
      if (_UserEnabledEx == null)
      {
        _UserEnabledEx = new DepInput<bool>(UserEnabled, UserEnabledEx_ValueChanged);
        _UserEnabledEx.OwnerInfo = new DepOwnerInfo(this, "UserEnabledEx");
      }
    }

    private DepInput<bool> _UserEnabledEx;

    void UserEnabledEx_ValueChanged(object sender, EventArgs args)
    {
      UserEnabled = _UserEnabledEx.Value;
    }

    /// <summary>
    /// Какие действия предпринять при записи документа, если <see cref="UserEnabled"/>=false.
    /// По умолчанию используется режим <see cref="ExtValueUserDisabledMode.KeepOriginal"/>: существующие значения не заменяются.
    /// </summary>
    public ExtValueUserDisabledMode UserDisabledMode
    {
      get { return _UserDisabledMode; }
      set { _UserDisabledMode = value; }
    }
    private ExtValueUserDisabledMode _UserDisabledMode;

    /// <summary>
    /// Значение <see cref="Grayed"/> для DocValue
    /// </summary>
    public DepValue<Boolean> GrayedEx
    {
      get { return _GrayedEx; }
      protected set
      {
        if (value == null)
          throw new ArgumentNullException();
        if (_GrayedEx != null)
        {
          _GrayedEx.ValueChanged -= new EventHandler(GrayedEx_ValueChanged);
          //throw new InvalidOperationException("Повторный вызов SetGrayed");
        }
        _GrayedEx = value;
        _GrayedEx.ValueChanged += new EventHandler(GrayedEx_ValueChanged);
      }
    }
    private DepValue<Boolean> _GrayedEx;

    /// <summary>
    /// Свойство <see cref="GrayedEx"/>.Value
    /// </summary>
    protected bool Grayed
    {
      get { return _GrayedEx.Value; }
      set
      {
        if (value == _GrayedEx.Value)
          return;
        DepInput<bool> grayedEx2 = _GrayedEx as DepInput<bool>;
        if (grayedEx2 == null)
        {
          grayedEx2 = new DepInput<bool>(false, null);
          grayedEx2.OwnerInfo = new DepOwnerInfo(this, "Internal GrayedEx input");
          grayedEx2.Source = _GrayedEx;
          _GrayedEx = grayedEx2;
        }
        grayedEx2.Value = value;
      }
    }

    void GrayedEx_ValueChanged(object sender, EventArgs args)
    {
      InitEnabled();
    }

    /// <summary>
    /// Установка значения свойства <see cref="EnabledEx"/>.Value
    /// </summary>
    protected virtual void InitEnabled()
    {
      _EnabledEx.OwnerSetValue(_UserEnabled && GetEnabledState());
    }

    /// <summary>
    /// Метод должен возвращать !<see cref="DBxExtValue.IsReadOnly"/> или аналогичное значение.
    /// Используется методом <see cref="InitEnabled()"/>.
    /// </summary>
    /// <returns>true, если значение поля можно редактировать</returns>
    protected abstract bool GetEnabledState();

    /// <summary>
    /// Если в режиме группового редактирования при <see cref="Grayed"/>=true используется 
    /// отдельный <see cref="CheckBox"/>, то поле содержит провайдер для него.
    /// </summary>
    private EFPCheckBox efpGrayCheckBox;

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Создание отдельного CheckBox'а для отработки свойства Grayed
    /// </summary>
    private void CreateGrayCheckBox()
    {
      // В процессе замены label может потеряться исходное свойство DisplayName элемента
      string orgDisplayName = ControlProvider.DisplayName;

      CheckBox grayCheckBox;
      if (ControlProvider.Label != null && ControlProvider.Label is Label)
        grayCheckBox = ReplaceLabelToCheckBox();
      else
        grayCheckBox = MakeNewCheckBox();
      efpGrayCheckBox = new EFPCheckBox(ControlProvider.BaseProvider, grayCheckBox);

      // Создаем зацикленную цепочку из CheckBox.CheckedEx, двух DepNot и DepInput посередине.
      DepNot not1 = new DepNot(efpGrayCheckBox.CheckedEx);
      not1.OwnerInfo = new DepOwnerInfo(this, "GrayCheckBox Chain - NOT #1");
      DepInput<bool> inp2 = new DepInput<bool>(false, null); // 15.10.2021 DepNot.Arg больше не является DepInput'ом
      inp2.OwnerInfo = new DepOwnerInfo(this, "GrayCheckBox Chain - INPUT #2");
      inp2.Source = not1;
      DepNot not3 = new DepNot(inp2);
      not3.OwnerInfo = new DepOwnerInfo(this, "GrayCheckBox Chain - NOT #3");
      efpGrayCheckBox.CheckedEx = not3;

      this.GrayedEx = not3.Arg; // нельзя использовать not1, т.к. это не EFPInput


      ControlProvider.Label = grayCheckBox;
      // Установка DisplayName должна идти после установки свойства Label
      efpGrayCheckBox.DisplayName = Res.ExtValueControl_Name_GroupCheckBox;
      efpGrayCheckBox.ToolTipText = String.Format(Res.ExtValueControl_ToolTip_GroupCheckBox, orgDisplayName);
      ControlProvider.DisplayName = orgDisplayName;
    }

    /// <summary>
    /// Преобразование существующего объекта Label в CheckBox
    /// </summary>
    /// <returns></returns>
    private CheckBox ReplaceLabelToCheckBox()
    {
      CheckBox grayCheckBox = new CheckBox();
      Label controlLabel = (Label)(ControlProvider.Label);

      grayCheckBox.Text = controlLabel.Text;
      grayCheckBox.Image = ((Label)controlLabel).Image;
      grayCheckBox.ImageList = ((Label)controlLabel).ImageList;
      grayCheckBox.ImageIndex = ((Label)controlLabel).ImageIndex;
      grayCheckBox.ImageAlign = ((Label)controlLabel).ImageAlign;

      grayCheckBox.CheckAlign = ContentAlignment.MiddleRight;

      WinFormsTools.ReplaceControl(controlLabel, grayCheckBox);
      controlLabel = null;
      return grayCheckBox;
    }

    /// <summary>
    /// Создание нового CheckBox без текста. Замена существующего Control на объект
    /// Panel того же размера и размещения. В панель добавляются созданный CheckBox
    /// и Control
    /// </summary>
    /// <returns></returns>
    private CheckBox MakeNewCheckBox()
    {
      Panel thePanel = new Panel();
      WinFormsTools.ReplaceControl(ControlProvider.Control, thePanel);

      CheckBox theCheckBox = new CheckBox();
      theCheckBox.Text = ""; // без заголовка
      theCheckBox.Location = new Point(0, (ControlProvider.Control.Height - 16) / 2);
      theCheckBox.Size = new Size(16, 16);
      theCheckBox.Anchor = AnchorStyles.Left | AnchorStyles.Top;
      theCheckBox.TabIndex = 0;
      thePanel.Controls.Add(theCheckBox);

      ControlProvider.Control.Dock = DockStyle.None;
      ControlProvider.Control.Location = new Point(16, 0);
      ControlProvider.Control.Size = new Size(thePanel.ClientSize.Width - 16, thePanel.ClientSize.Height);
      ControlProvider.Control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom; // 02.04.2025
      ControlProvider.Control.TabIndex = 1;
      thePanel.Controls.Add(ControlProvider.Control);
      thePanel.SizeChanged += new EventHandler(NewCheckBoxPanel_SizeChanged); // 02.04.2025
      return theCheckBox;
    }

    private static void NewCheckBoxPanel_SizeChanged(object sender, EventArgs args)
    {
      Panel thePanel = (Panel)sender;
      CheckBox theCheckBox = (CheckBox)(thePanel.Controls[0]);
      theCheckBox.Location = new Point(0, (thePanel.ClientSize.Height - theCheckBox.Height) / 2);
    }

    #endregion
  }

  /// <summary>
  /// Шаблон заготовки для конкретных управляющих элементов.
  /// Не определяет свойство Control нужного типа.
  /// </summary>
  /// <typeparam name="TValue">Тип редактируемого значения</typeparam>
  public abstract class ExtValueControlBase2<TValue> : ExtValueControlBase, IUIExtEditItem
  {
    #region Конструктор

    /// <summary>
    /// Создает объект          
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="useGrayCheckBox">Если true, то, если <paramref name="extValue"/> содержит "серое" значение,
    /// то будет создан дополнительный управляющий элемент <see cref="CheckBox"/></param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирование (разрешаются "серые" значения)</param>
    public ExtValueControlBase2(DBxExtValue extValue, IEFPControl controlProvider, bool useGrayCheckBox, bool canMultiEdit)
      : base(controlProvider, extValue.IsReadOnly,
      useGrayCheckBox && extValue.Grayed && (canMultiEdit || extValue.RowCount <= 1))
    {
      _ExtValue = extValue;
      if (canMultiEdit)
        _MultiEditDisabled = false;
      else
        _MultiEditDisabled = extValue.RowCount > 1;

      if (_MultiEditDisabled)
        controlProvider.Visible = false;
      base.GrayedEx.ValueChanged += new EventHandler(ControlChanged);

      _ChangeInfo = new DepChangeInfoValueItem();
      _ChangeInfo.DisplayName = controlProvider.DisplayName;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Редактируемое значение в наборе исходных данных
    /// </summary>
    public DBxExtValue ExtValue { get { return _ExtValue; } }
    private /*readonly - это структура, нельзя ее сделать readonly */ DBxExtValue _ExtValue;

    /// <summary>
    /// Текущее редактируемое значение
    /// </summary>
    public DepInput<TValue> CurrentValueEx { get { return _CurrentValueEx; } }
    private DepInput<TValue> _CurrentValueEx;

    /// <summary>
    /// Устанавливает свойство <see cref="CurrentValueEx"/> и присоединяет к нему обработчик, который вызывает метод <see cref="ControlChanged(object, EventArgs)"/>.
    /// </summary>
    /// <param name="value">Устанавливаемое управляемое свойство</param>
    protected void SetCurrentValueEx(DepValue<TValue> value)
    {
      if (value == null)
        throw new ArgumentNullException();
      if (_CurrentValueEx != null)
        throw ExceptionFactory.RepeatedCall(this, "CurrentValueEx");
      _CurrentValueEx = (value as DepInput<TValue>);
      if (_CurrentValueEx == null)
      {
        _CurrentValueEx = new DepInput<TValue>(default(TValue), null); //???
        _CurrentValueEx.OwnerInfo = new DepOwnerInfo(this, "CurrentValueEx");
        _CurrentValueEx.Source = value;
      }
      _CurrentValueEx.ValueChanged += new EventHandler(ControlChanged);
    }

    /// <summary>
    /// Первоначальное значение для сравнения
    /// </summary>
    private TValue _StartValue;

    /// <summary>
    /// Исходное значение в режиме "Grayed".
    /// Если исходное состояние Grayed=false, то не используем это поле, чтобы не хранить бесполезный массив одинаковых значений.
    /// Вместо этого можно использовать поле _StartValue
    /// </summary>
    private object[] _StartValueArray;

    /// <summary>
    /// Первоначальное значение признака Grayed
    /// </summary>
    private bool _StartGrayed;

    /// <summary>
    /// Значение, записываемое при <see cref="ExtValueControlBase.UserEnabled"/>=false, если задан подходящий режим <see cref="ExtValueControlBase.UserDisabledMode"/>.
    /// </summary>
    public TValue UserDisabledValue { get { return _UserDisabledValue; } set { _UserDisabledValue = value; } }
    private TValue _UserDisabledValue;

    /// <summary>
    /// True, если редактирование запрещено из-за наличия нескольких документов
    /// </summary>
    public bool MultiEditDisabled { get { return _MultiEditDisabled; } }
    private readonly bool _MultiEditDisabled;

    #endregion

    #region IExtEditItem Members

    /// <summary>
    /// Перенос значения из <see cref="ExtValue"/> в управляющий элемент
    /// </summary>
    public void ReadValues()
    {
      if (MultiEditDisabled)
        return;

      if (ExtValue.Grayed && (!ExtValue.IsReadOnly))
        Grayed = ExtValue.Grayed;
      OnValueToControl();
      InitEnabled();

      _ChangeInfo.Changed = false;
      _ChangeInfo.OriginalValue = CurrentValueEx.Value;
      _ChangeInfo.CurrentValue = CurrentValueEx.Value;

      if (ExtValue.Grayed)
        _StartValueArray = ExtValue.ValueArray;
      _StartValue = CurrentValueEx.Value;
      _StartGrayed = GrayedEx.Value;
    }

    /// <summary>
    /// Метод интерфейса <see cref="IUIExtEditItem"/>.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    public virtual void BeforeReadValues()
    {
    }

    /// <summary>
    /// Метод интерфейса <see cref="IUIExtEditItem"/>.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    public virtual void AfterReadValues()
    {
    }


    /// <summary>
    /// Перенос значения из управляющего элемента в <see cref="ExtValue"/> при нажатии кнопки "OK".
    /// Учитываются свойства <see cref="ExtValueControlBase.UserEnabled"/> и <see cref="ExtValueControlBase.UserDisabledMode"/>.
    /// </summary>
    public void WriteValues()
    {
      if (MultiEditDisabled)
        return;

      if (!UserEnabled)
      {
        switch (UserDisabledMode)
        {
          case ExtValueUserDisabledMode.KeepOriginal:
            return;
          case ExtValueUserDisabledMode.KeepOriginalIfGrayed:
            if (!GrayedEx.Value)
              _ExtValue.Value = UserDisabledValue;
            return;
          case ExtValueUserDisabledMode.AlwaysReplace:
            _ExtValue.Value = UserDisabledValue;
            return;
          default:
            throw new BugException("Unknown UserDisableMode=" + UserDisabledMode.ToString());
        }
      }

      if (GrayedEx.Value)
      {
        if (_StartValueArray == null)
          _ExtValue.SetValue(_StartValue);
        else
          _ExtValue.ValueArray = _StartValueArray;
      }
      else
        OnValueFromControl();
    }

    DepChangeInfo IUIExtEditItem.ChangeInfo { get { return _ChangeInfo; } }

    /// <summary>
    /// Объект для отслеживания изменений в поле.
    /// В отличие от свойства интерфейса <see cref="IUIExtEditItem"/> имеет тип <see cref="DepChangeInfoValueItem"/>,
    /// что позволяет устанавливать признак изменений.
    /// </summary>
    public DepChangeInfoValueItem ChangeInfo { get { return _ChangeInfo; } }
    private readonly DepChangeInfoValueItem _ChangeInfo;

    #endregion

    #region Вспомогательные методы для наследников

    /// <summary>
    /// Этот метод вызывается управляющим элементом при изменении текущего значения
    /// </summary>
    protected void ControlChanged(object sender, EventArgs args)
    {
      // Сравниваем с начальным значением
      if (EnabledEx.Value)
      {
        if (GrayedEx.Value != _StartGrayed)
          _ChangeInfo.Changed = true;
        else
          _ChangeInfo.Changed = !Object.Equals(CurrentValueEx.Value, _StartValue);
      }
      else
        _ChangeInfo.Changed = false;

      _ChangeInfo.CurrentValue = _CurrentValueEx.Value;
    }

    #endregion

    #region События

    /// <summary>
    /// Ecли обработчик установлен, то он вызывается вместо <see cref="ValueToControl"/> для
    /// инициализации значения управляющего элемента из <see cref="ExtValue"/>.
    /// </summary>
    public event EventHandler WantedValueToControl;

    /// <summary>
    /// Если установлен обработчик события <see cref="WantedValueToControl"/>, то он вызывается.
    /// Иначе вызывается метод <see cref="ValueToControl()"/>.
    /// </summary>
    protected void OnValueToControl()
    {
      if (WantedValueToControl == null)
        ValueToControl();
      else
        WantedValueToControl(this, EventArgs.Empty);
    }

    /// <summary>
    /// Ecли обработчик установлен, то он вызывается вместо <see cref="ValueFromControl"/> для
    /// сохранения значения управляющего элемента из Control в <see cref="ExtValue"/>.
    /// </summary>
    public event EventHandler WantedValueFromControl;

    /// <summary>
    /// Если установлен обработчик события <see cref="WantedValueFromControl"/>, то он вызывается.
    /// Иначе вызывается метод <see cref="ValueFromControl()"/>.
    /// </summary>
    protected void OnValueFromControl()
    {
      if (WantedValueFromControl == null)
        ValueFromControl();
      else
        WantedValueFromControl(this, EventArgs.Empty);
    }

    #endregion

    #region Абстрактные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected abstract void ValueToControl();

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected abstract void ValueFromControl();

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Метод возвращает инвертированное значение <see cref="DBxExtValue.IsReadOnly"/>, если в данный момент не активно "серое" значение поля.
    /// </summary>
    /// <returns>true, если значение поля можно редактировать</returns>
    protected override bool GetEnabledState()
    {
      return (!ExtValue.IsReadOnly) && (!GrayedEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Шаблон заготовки для конкретных управляющих элементов
  /// Доопределяет свойство ControlProvider.
  /// </summary>
  /// <typeparam name="TValue">Тип редактируемого значения</typeparam>
  /// <typeparam name="TControlProvider">Тип провайдера управляющего элемента</typeparam>
  public abstract class ExtValueControl<TValue, TControlProvider> : ExtValueControlBase2<TValue>
    where TControlProvider : IEFPControl
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="useGrayCheckBox">Если true, то, если <paramref name="extValue"/> содержит "серое" значение,
    /// то будет создан дополнительный управляющий элемент <see cref="CheckBox"/>.</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается.</param>
    public ExtValueControl(DBxExtValue extValue, TControlProvider controlProvider, bool useGrayCheckBox, bool canMultiEdit)
      : base(extValue, controlProvider, useGrayCheckBox , canMultiEdit)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента, приведенный к нужному типу
    /// </summary>
    public new TControlProvider ControlProvider
    {
      get { return (TControlProvider)(base.ControlProvider); }
    }

    #endregion
  }

  /// <summary>
  /// Шаблон заготовки для управляющего элемента, редактирующего сразу два поля.
  /// Не определяет свойство ControlProvider нужного типа.
  /// Второе значение может не редактироваться (DocValue2=null)
  /// </summary>
  /// <typeparam name="TValue1">Тип первого редактируемого значения</typeparam>
  /// <typeparam name="TValue2">Тип второго редактируемого значения</typeparam>
  public abstract class TwoExtValueControlBase2<TValue1, TValue2> : ExtValueControlBase, IUIExtEditItem
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue1">Доступ к значению поля (первому)</param>
    /// <param name="extValue2">Доступ к значению поля (второму)</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="useGrayCheckBox">Если true, то, если <paramref name="extValue1"/> или <paramref name="extValue2"/> содержит "серое" значение,
    /// то будет создан дополнительный управляющий элемент <see cref="CheckBox"/></param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public TwoExtValueControlBase2(DBxExtValue extValue1, DBxExtValue extValue2, EFPControlBase controlProvider, bool useGrayCheckBox, bool canMultiEdit)
      : base(controlProvider, extValue1.IsReadOnly,
      useGrayCheckBox && GetCanGrayed(extValue1, extValue2, canMultiEdit))
    {
      _ExtValue1 = extValue1;
      _ExtValue2 = extValue2;
      if (canMultiEdit)
        _MultiEditDisabled = false;
      else
        _MultiEditDisabled = extValue1.RowCount > 1;

      if (_MultiEditDisabled)
        controlProvider.Visible = false;
      base.GrayedEx.ValueChanged += new EventHandler(ControlChanged);

      _ChangeInfo = new DepChangeInfoValueItem();
      _ChangeInfo.DisplayName = controlProvider.DisplayName;
    }

    private static bool GetCanGrayed(DBxExtValue extValue1, DBxExtValue extValue2, bool canMultiEdit)
    {
      if (canMultiEdit || extValue1.RowCount <= 1)
        return extValue1.Grayed || extValue2.Grayed;
      else
        return false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Первое редактируемое значение в наборе исходных данных
    /// </summary>
    public DBxExtValue ExtValue1 { get { return _ExtValue1; } }
    private DBxExtValue _ExtValue1;

    /// <summary>
    /// Второе редактируемое значение в наборе исходных данных
    /// </summary>
    public DBxExtValue ExtValue2 { get { return _ExtValue2; } }
    private DBxExtValue _ExtValue2;

    /// <summary>
    /// Текущее редактируемое значение (первое)
    /// </summary>
    public DepInput<TValue1> CurrentValue1Ex { get { return _CurrentValue1Ex; } }
    private DepInput<TValue1> _CurrentValue1Ex;

    /// <summary>
    /// Текущее редактируемое значение (второе)
    /// </summary>
    public DepInput<TValue2> CurrentValue2Ex { get { return _CurrentValue2Ex; } }
    private DepInput<TValue2> _CurrentValue2Ex;

    /// <summary>
    /// Устанавливает свойства <see cref="CurrentValue1Ex"/> и <see cref="CurrentValue2Ex"/> и присоединяет к ним обработчики на метод <see cref="ControlChanged(object, EventArgs)"/>.
    /// </summary>
    /// <param name="value1">Устанавливаемое управляемое свойство (первое)</param>
    /// <param name="value2">Устанавливаемое управляемое свойство (второе)</param>
    protected void SetCurrentValueEx(DepValue<TValue1> value1, DepValue<TValue2> value2)
    {
      if (value1 == null)
        throw new ArgumentNullException("value1");
      if (value2 == null)
        throw new ArgumentNullException("value2");
      if (_CurrentValue1Ex != null || _CurrentValue2Ex != null)
        throw ExceptionFactory.RepeatedCall(this, "CurrentValueEx");

      _CurrentValue1Ex = value1 as DepInput<TValue1>;
      _CurrentValue2Ex = value2 as DepInput<TValue2>;
      if (_CurrentValue1Ex == null)
      {
        _CurrentValue1Ex = new DepInput<TValue1>(default(TValue1), null); // ????
        _CurrentValue1Ex.OwnerInfo = new DepOwnerInfo(this, "CurrentValue1Ex");
        _CurrentValue1Ex.Source = value1;
      }
      if (_CurrentValue2Ex == null)
      {
        _CurrentValue2Ex = new DepInput<TValue2>(default(TValue2), null); // ????
        _CurrentValue2Ex.OwnerInfo = new DepOwnerInfo(this, "CurrentValue2Ex");
        _CurrentValue2Ex.Source = value2;
      }

      _CurrentValue1Ex.ValueChanged += new EventHandler(ControlChanged);
      _CurrentValue2Ex.ValueChanged += new EventHandler(ControlChanged);
    }

    /// <summary>
    /// Первоначальное значение для сравнения (первое)
    /// </summary>
    private TValue1 _StartValue1;

    /// <summary>
    /// Первоначальное значение для сравнения (второе)
    /// </summary>
    private TValue2 _StartValue2;

    private object[] _StartValueArray1;
    private object[] _StartValueArray2;

    /// <summary>
    /// Первоначальное значение признака Grayed
    /// </summary>
    private bool _StartGrayed;


    /// <summary>
    /// Первое значение, записываемое при <see cref="ExtValueControlBase.UserEnabled"/>=false, если задан подходящий режим <see cref="ExtValueControlBase.UserDisabledMode"/>.
    /// </summary>
    public TValue1 UserDisabledValue1 { get { return _UserDisabledValue1; } set { _UserDisabledValue1 = value; } }
    private TValue1 _UserDisabledValue1;

    /// <summary>
    /// Второе значение, записываемое при <see cref="ExtValueControlBase.UserEnabled"/>=false, если задан подходящий режим <see cref="ExtValueControlBase.UserDisabledMode"/>.
    /// </summary>
    public TValue2 UserDisabledValue2 { get { return _UserDisabledValue2; } set { _UserDisabledValue2 = value; } }
    private TValue2 _UserDisabledValue2;

    /// <summary>
    /// True, если редактирование запрещено из-за наличия нескольких документов
    /// </summary>
    public bool MultiEditDisabled { get { return _MultiEditDisabled; } }
    private readonly bool _MultiEditDisabled;

    #endregion

    #region IExtEditItem Members

    /// <summary>
    /// Перенос значений из <see cref="ExtValue1"/> и <see cref="ExtValue2"/> в управляющий элемент
    /// </summary>
    public void ReadValues()
    {
      if (MultiEditDisabled)
        return;

      bool f1 = ExtValue1.Grayed && (!ExtValue1.IsReadOnly);
      bool f2 = ExtValue2.Grayed && (!ExtValue2.IsReadOnly);
      if (f1 || f2)
      {
        Grayed = ExtValue1.Grayed || ExtValue2.Grayed;
      }

      OnValueToControl();
      InitEnabled();

      _ChangeInfo.Changed = false;
      _ChangeInfo.OriginalValue = GetChangeInfoValue(CurrentValue1Ex.Value, CurrentValue2Ex.Value);
      _ChangeInfo.CurrentValue = _ChangeInfo.OriginalValue;

      if (ExtValue1.Grayed)
        _StartValueArray1 = ExtValue1.ValueArray;
      if (ExtValue2.Grayed)
        _StartValueArray2 = ExtValue2.ValueArray;
      _StartValue1 = CurrentValue1Ex.Value;
      _StartValue2 = CurrentValue2Ex.Value;
      _StartGrayed = GrayedEx.Value;
    }

    private static object GetChangeInfoValue(object v1, object v2)
    {
      string s1, s2;
      if (v1 == null)
        s1 = "null";
      else
        s1 = v1.ToString();

      if (v2 == null)
        s2 = "null";
      else
        s2 = v2.ToString();

      return s1 + "-" + s2;
    }

    /// <summary>
    /// Метод интерфейса <see cref="IUIExtEditItem"/>.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    public void BeforeReadValues()
    {
    }

    /// <summary>
    /// Метод интерфейса <see cref="IUIExtEditItem"/>.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    public void AfterReadValues()
    {
    }

    /// <summary>
    /// Перенос значения из управляющего элемента в <see cref="ExtValue1"/> и <see cref="ExtValue2"/> при нажатии кнопки "OK".
    /// </summary>
    public void WriteValues()
    {
      if (MultiEditDisabled)
        return;

      if (!UserEnabled)
      {
        switch (UserDisabledMode)
        {
          case ExtValueUserDisabledMode.KeepOriginal:
            return;
          case ExtValueUserDisabledMode.KeepOriginalIfGrayed:
            if (!GrayedEx.Value)
            {
              _ExtValue1.Value = UserDisabledValue1;
              _ExtValue2.Value = UserDisabledValue2;
            }
            return;
          case ExtValueUserDisabledMode.AlwaysReplace:
            _ExtValue1.Value = UserDisabledValue1;
            _ExtValue2.Value = UserDisabledValue2;
            return;
          default:
            throw new BugException("Unknown UserDisableMode=" + UserDisabledMode.ToString());
        }
      }

      if (GrayedEx.Value)
      {
        if (_StartValueArray1 == null)
          _ExtValue1.Value = _StartValue1;
        else
          _ExtValue1.ValueArray = _StartValueArray1;
        if (_StartValueArray2 == null)
          _ExtValue2.Value = _StartValue2;
        else
          _ExtValue2.ValueArray = _StartValueArray2;
      }
      else
        OnValueFromControl();
    }

    DepChangeInfo IUIExtEditItem.ChangeInfo { get { return _ChangeInfo; } }

    /// <summary>
    /// Объект для отслеживания изменений в поле.
    /// В отличие от свойства интерфейса <see cref="IUIExtEditItem"/> имеет тип <see cref="DepChangeInfoValueItem"/>,
    /// что позволяет устанавливать признак изменений.
    /// </summary>
    public DepChangeInfoValueItem ChangeInfo { get { return _ChangeInfo; } }
    private readonly DepChangeInfoValueItem _ChangeInfo;


    #endregion

    #region Вспомогательные методы для наследников

    /// <summary>
    /// Этот метод вызывается управляющим элементом при изменении текущего значения
    /// </summary>
    protected void ControlChanged(object sender, EventArgs args)
    {
      // Сравниваем с начальным значением
      if (EnabledEx.Value)
      {
        if (GrayedEx.Value != _StartGrayed)
          _ChangeInfo.Changed = true;
        else
        {
          bool eq1 = Object.Equals(CurrentValue1Ex.Value, _StartValue1);
          bool eq2 = Object.Equals(CurrentValue2Ex.Value, _StartValue2);
          _ChangeInfo.Changed = !(eq1 && eq2);
        }
      }
      else
        _ChangeInfo.Changed = false;

      _ChangeInfo.CurrentValue = GetChangeInfoValue(CurrentValue1Ex.Value, CurrentValue2Ex.Value);
    }

    #endregion

    #region События

    /// <summary>
    /// Ecли обработчик установлен, то он вызывается вместо <see cref="ValueToControl()"/> для
    /// инициализации значения управляющего элемента из <see cref="ExtValue1"/>, <see cref="ExtValue2"/> в Control.
    /// </summary>
    public event EventHandler WantedValueToControl;

    /// <summary>
    /// Если установлен обработчик события <see cref="WantedValueToControl"/>, то он вызывается.
    /// Иначе вызывается метод <see cref="ValueToControl()"/>.
    /// </summary>
    protected void OnValueToControl()
    {
      if (WantedValueToControl == null)
        ValueToControl();
      else
        WantedValueToControl(this, EventArgs.Empty);
    }

    /// <summary>
    /// Ecли обработчик установлен, то он вызывается вместо <see cref="ValueFromControl"/> для
    /// сохранения значения управляющего элемента из Control в <see cref="ExtValue1"/>, <see cref="ExtValue2"/>
    /// </summary>
    public event EventHandler WantedValueFromControl;

    /// <summary>
    /// Если установлен обработчик события <see cref="WantedValueFromControl"/>, то он вызывается.
    /// Иначе вызывается метод <see cref="ValueFromControl()"/>.
    /// </summary>
    protected void OnValueFromControl()
    {
      if (WantedValueFromControl == null)
        ValueFromControl();
      else
        WantedValueFromControl(this, EventArgs.Empty);
    }

    #endregion

    #region Абстрактные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected abstract void ValueToControl();

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected abstract void ValueFromControl();

    /// <summary>
    /// Этот метод вызывается для установки значения в основном управляющем элементе,
    /// когда данные находятся в режиме Grayed.
    /// Метод должен установить <see cref="CurrentValue1Ex"/>.Value и <see cref="CurrentValue2Ex"/>.Value.
    /// Реализация по умолчанию записывет нулевые значения.
    /// Переопределение метода необходимо, если управляюший элемент не может получать
    /// нулевые значения, например <see cref="FreeLibSet.Controls.MonthDayBox"/>.
    /// </summary>
    protected virtual void WriteToControlWhenGrayed()
    {
      CurrentValue1Ex.Value = default(TValue1);
      CurrentValue2Ex.Value = default(TValue2);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Метод возвращает инвертированное значение <see cref="DBxExtValue.IsReadOnly"/>, если в данный момент не активно "серое" значение поля.
    /// </summary>
    /// <returns>true, если значение поля можно редактировать</returns>
    protected override bool GetEnabledState()
    {
      return (!ExtValue1.IsReadOnly) && (!ExtValue2.IsReadOnly) && (!GrayedEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Шаблон заготовки для конкретных управляющих элементов для редактирования двух полей
  /// Доопределяет свойство ControlProvider.
  /// </summary>
  /// <typeparam name="TValue1">Тип первого редактируемого значения</typeparam>
  /// <typeparam name="TValue2">Тип второго редактируемого значения</typeparam>
  /// <typeparam name="TControlProvider">Тип провайдера управляющего элемента</typeparam>
  public abstract class TwoExtValueControl<TValue1, TValue2, TControlProvider> : TwoExtValueControlBase2<TValue1, TValue2>
    where TControlProvider : IEFPControl
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue1">Доступ к значению поля (первому)</param>
    /// <param name="extValue2">Доступ к значению поля (второму)</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="useGrayCheckBox">Если true, то, если <paramref name="extValue1"/> или <paramref name="extValue2"/> содержит "серое" значение,
    /// то будет создан дополнительный управляющий элемент <see cref="CheckBox"/></param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public TwoExtValueControl(DBxExtValue extValue1, DBxExtValue extValue2, TControlProvider controlProvider, bool useGrayCheckBox, bool canMultiEdit)
      : base(extValue1, extValue2, (EFPControlBase)(IEFPControl)controlProvider, useGrayCheckBox , canMultiEdit)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента, приведенный к нужному типу
    /// </summary>
    public new TControlProvider ControlProvider
    {
      get { return (TControlProvider)(base.ControlProvider); }
    }

    #endregion
  }

#if XXX
 /// <summary>
  /// Шаблон заготовки для управляющего элемента, редактирующего сразу 3 поля
  /// Не определяет свойство Control нужного типа.
  /// Второе и третье значение может не редактироваться (DocValue2=null)
  /// </summary>
  /// <typeparam name="TValue1">Тип первого редактируемого значения</typeparam>
  /// <typeparam name="TValue2">Тип второго редактируемого значения</typeparam>
  /// <typeparam name="TValue3">Тип третьего редактируемого значения</typeparam>
  public abstract class ThreeDocValueControlBase2<TValue1, TValue2, TValue3> : DocValueControlBase, IDocEditItem
  {
  #region Конструктор

    public ThreeDocValueControlBase2(IDocValue DocValue1, IDocValue DocValue2, IDocValue DocValue3, EFPControlBase ControlProvider, bool UseGrayCheckBox, bool CanMultiEdit)
      : base(ControlProvider, DocValue1.ReadOnly,
      UseGrayCheckBox && GetCanGrayed(DocValue1, DocValue2, DocValue3, CanMultiEdit))
    {
      FDocValue1 = DocValue1;
      FDocValue2 = DocValue2;
      FDocValue3 = DocValue3;
      if (CanMultiEdit)
        FMultiEditDisabled = false;
      else
        FMultiEditDisabled = DocValue1.DocCount > 1;

      if (FMultiEditDisabled)
        ControlProvider.Visible = false;
      base.GrayedEx.ValueChanged += new EventHandler(ControlChanged);

      FChangeInfo = new DepChangeInfoValueItem();
      FChangeInfo.DisplayName = ControlProvider.DisplayName;
    }

    private static bool GetCanGrayed(IDocValue DocValue1, IDocValue DocValue2, IDocValue DocValue3, bool CanMultiEdit)
    {
      if (CanMultiEdit || DocValue1.DocCount <= 1)
      {
        bool res = DocValue1.CanGrayed;
        if (DocValue2 != null)
          res |= DocValue2.CanGrayed;
        if (DocValue3 != null)
          res |= DocValue3.CanGrayed;
        return res;
      }
      else
        return false;
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Первое редактируемое значение в наборе исходных данных
    /// </summary>
    public IDocValue DocValue1 { get { return FDocValue1; } }
    private IDocValue FDocValue1;

    /// <summary>
    /// Второе редактируемое значение в наборе исходных данных или null
    /// </summary>
    public IDocValue DocValue2 { get { return FDocValue2; } }
    private IDocValue FDocValue2;

    /// <summary>
    /// Третье редактируемое значение в наборе исходных данных или null
    /// </summary>
    public IDocValue DocValue3 { get { return FDocValue3; } }
    private IDocValue FDocValue3;

    /// <summary>
    /// Текущее редактируемое значение (первое)
    /// </summary>
    public DepValue<TValue1> CurrentValue1 { get { return FCurrentValue1; } }
    protected DepValue<TValue1> FCurrentValue1;

    /// <summary>
    /// Текущее редактируемое значение (второе)
    /// </summary>
    public DepValue<TValue2> CurrentValue2 { get { return FCurrentValue2; } }
    protected DepValue<TValue2> FCurrentValue2;

    /// <summary>
    /// Текущее редактируемое значение (третье)
    /// </summary>
    public DepValue<TValue3> CurrentValue3 { get { return FCurrentValue3; } }
    protected DepValue<TValue3> FCurrentValue3;

    protected void SetCurrentValue(DepValue<TValue1> Value1, DepValue<TValue2> Value2, DepValue<TValue3> Value3)
    {
      FCurrentValue1 = Value1;
      FCurrentValue2 = Value2;
      FCurrentValue3 = Value3;
    }

    /// <summary>
    /// Первоначальное значение для сравнения (первое)
    /// </summary>
    private TValue1 StartValue1;

    /// <summary>
    /// Первоначальное значение для сравнения (второе)
    /// </summary>
    private TValue2 StartValue2;

    /// <summary>
    /// Первоначальное значение для сравнения (третье)
    /// </summary>
    private TValue3 StartValue3;

    /// <summary>
    /// True, если редактирование запрещено из-за наличия нескольких документов
    /// </summary>
    public bool MultiEditDisabled { get { return FMultiEditDisabled; } }
    private bool FMultiEditDisabled;

  #endregion

  #region IExtEditItem Members

    /// <summary>
    /// Перенос значения из DocValue в управляющий элемент
    /// </summary>
    public void ReadValues()
    {
      if (MultiEditDisabled)
        return;

      bool f1 = DocValue1.CanGrayed && (!DocValue1.ReadOnly);
      bool f2 = false;
      bool f3 = false;
      if (DocValue2 != null)
        f2 = DocValue2.CanGrayed && (!DocValue2.ReadOnly);
      if (DocValue3 != null)
        f3 = DocValue3.CanGrayed && (!DocValue3.ReadOnly);
      if (f1 || f2 || f3)
      {
        if (DocValue2 == null)
          GrayedEx.Value = DocValue1.Grayed;
        else
        {
          if (DocValue2 == null)
            GrayedEx.Value = DocValue1.Grayed || DocValue2.Grayed;
          else
            GrayedEx.Value = DocValue1.Grayed || DocValue2.Grayed || DocValue3.Grayed;
        }
      }

      OnValueToControl();

      InitEnabled();

      FChangeInfo.Changed = false;
      StartValue1 = CurrentValue1.Value;
      StartValue2 = CurrentValue2.Value;
      StartValue3 = CurrentValue3.Value;
    }

    public void BeforeReadValues() { }
    public void AfterReadValues() { }

    /// <summary>
    /// Перенос значения из управляющего элемента в DocValue при нажатии кнопки "OK"
    /// </summary>
    public void WriteValues()
    {
      if (MultiEditDisabled)
        return;

      if (GrayedEx.Value)
      {
        DocValue1.RestoreValue();
        if (DocValue2 != null)
          DocValue2.RestoreValue();
        if (DocValue3 != null)
          DocValue3.RestoreValue();
      }
      else
        OnValueFromControl();
    }

    public DepChangeInfo ChangeInfo { get { return FChangeInfo; } }
    private DepChangeInfoValueItem FChangeInfo;

  #endregion

  #region Вспомогательные методы для наследников

    /// <summary>
    /// Этот метод вызывается управляющим элементом при изменении текущего значения
    /// </summary>
    protected void ControlChanged(object o, EventArgs a)
    {
      // Сравниваем с начальным значением
      if (EnabledEx.Value)
      {
        bool Eq = DepValue<TValue1>.IsEqualValues(CurrentValue1.Value, StartValue1);
        if (DocValue2 != null)
          Eq &= DepValue<TValue2>.IsEqualValues(CurrentValue2.Value, StartValue2);
        if (DocValue3 != null)
          Eq &= DepValue<TValue3>.IsEqualValues(CurrentValue3.Value, StartValue3);
        FChangeInfo.Changed = !Eq;
      }
      else
        FChangeInfo.Changed = false;

      FChangeInfo.CurrentValue = new object[] { FCurrentValue1.Value, FCurrentValue2.Value, FCurrentValue3.Value };
    }

  #endregion

  #region События

    /// <summary>
    /// Ecли обработчик установлен, то он вызывается вместо ValueToControl для
    /// инициализации значения управляющего элемента из DocValue в Control
    /// </summary>
    public event EventHandler WantedValueToControl;

    protected void OnValueToControl()
    {
      if (WantedValueToControl == null)
        ValueToControl();
      else
        WantedValueToControl(this, EventArgs.Empty);
    }

    /// <summary>
    /// Ecли обработчик установлен, то он вызывается вместо ValueFromControl для
    /// сохранения значения управляющего элемента из Control в DocValue 
    /// </summary>
    public event EventHandler WantedValueFromControl;

    protected void OnValueFromControl()
    {
      if (WantedValueFromControl == null)
        ValueFromControl();
      else
        WantedValueFromControl(this, EventArgs.Empty);
    }

  #endregion

  #region Абстрактные методы

    protected abstract void ValueToControl();
    protected abstract void ValueFromControl();

  #endregion

  #region Переопределенные методы

    protected override bool GetEnabledState()
    {
      return (!DocValue1.ReadOnly) && (!GrayedEx.Value);
    }

  #endregion
  }

  /// <summary>
  /// Шаблон заготовки для конкретных управляющих элементов для редактирования трех полей
  /// Доопределяет свойство Control
  /// </summary>
  /// <typeparam name="TValue1">Тип первого редактируемого значения</typeparam>
  /// <typeparam name="TValue2">Тип второго редактируемого значения</typeparam>
  /// <typeparam name="TValue3">Тип третьего редактируемого значения</typeparam>
  /// <typeparam name="TControlProvider">Тип провайдера управляющего элемента</typeparam>
  public abstract class ThreeDocValueControl<TValue1, TValue2, TValue3, TControlProvider> : ThreeDocValueControlBase2<TValue1, TValue2, TValue3>
    where TControl : EFPControlBase
  {
  #region Конструктор

    public ThreeDocValueControl(IDocValue DocValue1, IDocValue DocValue2, IDocValue DocValue3,
      TControlProvider ControlProvider, bool UseGrayCheckBox, bool CanMultiEdit)
      : base(DocValue1, DocValue2, DocValue3, ControlProvider, UseGrayCheckBox, CanMultiEdit)
    {
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента, приведенный к нужному типу
    /// </summary>
    public new TControlProvider ControlProvider
    {
      get { return (TControlProvider)(base.ControlProvider); }
    }

  #endregion
  }

  /// <summary>
  /// Шаблон заготовки для управляющего элемента, редактирующего сразу несколько полей произвольных типов
  /// путем преобразования их к одному комбинированному значению (обычно строкового типа)
  /// Не определяет свойство Control нужного типа.
  /// </summary>
  /// <typeparam name="TValue">Тип комбинированного редактируемого значения (обычно "string")</typeparam>
  public abstract class MultiDocValueControlBase2<TValue> : DocValueControlBase, IDocEditItem
  {
  #region Конструктор

    public MultiDocValueControlBase2(IDocValue[] DocValues, EFPControlBase ControlProvider, bool UseGrayCheckBox, bool CanMultiEdit)
      : base(ControlProvider, DocValues[0].ReadOnly,
      UseGrayCheckBox && GetCanGrayed(DocValues, CanMultiEdit))
    {
      FDocValues = DocValues;
      if (CanMultiEdit)
        FMultiEditDisabled = false;
      else
        FMultiEditDisabled = DocValues[0].DocCount > 1;

      if (FMultiEditDisabled)
        ControlProvider.Visible = false;
      base.GrayedEx.ValueChanged += new EventHandler(ControlChanged);

      FChangeInfo = new DepChangeInfoItem();
      FChangeInfo.DisplayName = ControlProvider.DisplayName;
    }

    private static bool GetCanGrayed(IDocValue[] DocValues, bool CanMultiEdit)
    {
      if (CanMultiEdit || DocValues[0].DocCount <= 1)
        return DocValues[0].CanGrayed; // ???
      else
        return false;
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Массив редактируемых значений в наборе исходных данных
    /// </summary>
    public IDocValue[] DocValues { get { return FDocValues; } }
    private IDocValue[] FDocValues;

    /// <summary>
    /// Текущее комбинированное редактируемое значение
    /// </summary>
    public DepValue<TValue> CurrentValue { get { return FCurrentValue; } }
    protected DepValue<TValue> FCurrentValue;

    /// <summary>
    /// Первоначальное значение для сравнения
    /// </summary>
    private TValue StartValue;

    /// <summary>
    /// True, если редактирование запрещено из-за наличия нескольких документов
    /// </summary>
    public bool MultiEditDisabled { get { return FMultiEditDisabled; } }
    private bool FMultiEditDisabled;

  #endregion

  #region IExtEditItem Members

    /// <summary>
    /// Перенос значения из DocValue в управляющий элемент
    /// </summary>
    public void ReadValues()
    {
      if (MultiEditDisabled)
        return;

      bool Gr = false;
      for (int i = 0; i < DocValues.Length; i++)
      {
        bool f = DocValues[i].CanGrayed && (!DocValues[i].ReadOnly);
        if (f && DocValues[i].Grayed)
          Gr = true;
      }

      if (Gr)
        GrayedEx.Value = Gr;

      OnValueToControl();

      InitEnabled();

      FChangeInfo.Changed = false;
      StartValue = CurrentValue.Value;
    }

    public void BeforeReadValues() { }
    public void AfterReadValues() { }

    /// <summary>
    /// Перенос значения из управляющего элемента в DocValue при нажатии кнопки "OK"
    /// </summary>
    public void WriteValues()
    {
      if (MultiEditDisabled)
        return;

      if (GrayedEx.Value)
      {
        for (int i = 0; i < DocValues.Length; i++)
          DocValues[i].RestoreValue();
      }
      else
        OnValueFromControl();
    }

    public DepChangeInfo ChangeInfo { get { return FChangeInfo; } }
    private DepChangeInfoItem FChangeInfo;

  #endregion

  #region Вспомогательные методы для наследников

    /// <summary>
    /// Этот метод вызывается управляющим элементом при изменении текущего значения
    /// </summary>
    protected void ControlChanged(object o, EventArgs a)
    {
      // Сравниваем с начальным значением
      if (EnabledEx.Value)
      {
        bool Eq = DepValue<TValue>.IsEqualValues(CurrentValue.Value, StartValue);
        FChangeInfo.Changed = !Eq;
      }
      else
        FChangeInfo.Changed = false;
    }

  #endregion

  #region События

    /// <summary>
    /// Ecли обработчик установлен, то он вызывается вместо ValueToControl для
    /// инициализации значения управляющего элемента из DocValue в Control
    /// </summary>
    public event EventHandler WantedValueToControl;

    protected void OnValueToControl()
    {
      if (WantedValueToControl == null)
        ValueToControl();
      else
        WantedValueToControl(this, EventArgs.Empty);
    }

    /// <summary>
    /// Ecли обработчик установлен, то он вызывается вместо ValueFromControl для
    /// сохранения значения управляющего элемента из Control в DocValue 
    /// </summary>
    public event EventHandler WantedValueFromControl;

    protected void OnValueFromControl()
    {
      if (WantedValueFromControl == null)
        ValueFromControl();
      else
        WantedValueFromControl(this, EventArgs.Empty);
    }

  #endregion

  #region Абстрактные методы

    protected abstract void ValueToControl();
    protected abstract void ValueFromControl();

  #endregion

  #region Переопределенные методы

    protected override bool GetEnabledState()
    {
      return (!DocValues[0].ReadOnly) && (!GrayedEx.Value);
    }

  #endregion
  }

  /// <summary>
  /// Шаблон заготовки для конкретных управляющих элементов для редактирования нескольких полей
  /// путем получения комбинированного значения
  /// Доопределяет свойство Control
  /// </summary>
  /// <typeparam name="TValue">Тип комбинированного редактируемого значения</typeparam>
  /// <typeparam name="TControlProvider">Тип провайдера управляющего элемента</typeparam>
  public abstract class MultiDocValueControl<TValue, TControlProvider> : MultiDocValueControlBase2<TValue>
    where TControlProvider : EFPControlBase
  {
  #region Конструктор

    public MultiDocValueControl(IDocValue[] DocValues, TControlProvider ControlProvider, bool UseGrayCheckBox, bool CanMultiEdit)
      : base(DocValues, ControlProvider, UseGrayCheckBox, CanMultiEdit)
    {
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента, приведенный к нужному типу
    /// </summary>
    public new TControlProvider ControlProvider
    {
      get { return (TControlProvider)(base.ControlProvider); }
    }

  #endregion
  }

#endif
}
