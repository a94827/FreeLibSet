// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.DependedValues;
using FreeLibSet.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Formatting;
using System.Drawing;
using System.ComponentModel;
using System.Text.RegularExpressions;
using FreeLibSet.Shell;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.Collections;

namespace FreeLibSet.Forms
{
  #region Интерфейс IEFPTextBox

  /// <summary>
  /// Интерфейс провайдера управляющего элемента, содержащего текст, который может вводиться пользователем.
  /// Реализуется такими элементами, как EFPDateBox. 
  /// "Настоящие" поля текстового ввода реализуют расширенный интерфейс IEFPTextBox
  /// </summary>
  public interface IEFPSimpleTextBox : IEFPControl
  {
    /// <summary>
    /// Весь текст элемента
    /// </summary>
    string Text { get; set; }

    /// <summary>
    /// Возвращает длину текста в элементе
    /// </summary>
    int TextLength { get; }

    /// <summary>
    /// Начальная позиция выделенного текста или положение курсора при SelectionLength=0
    /// </summary>
    int SelectionStart { get; set; }

    /// <summary>
    /// Длина выделенного фрагмента текста. 
    /// Если выделения нет, возвращает 0.
    /// </summary>
    int SelectionLength { get; set; }

    /// <summary>
    /// Выделенный фрагмент текста. Если выделения нет, содержит пустую строку.
    /// Установка свойства заменяет выделенный фрагмент
    /// </summary>
    string SelectedText { get; set; }

    /// <summary>
    /// Одновременная установка свойств SelectionStart и SelectionLength
    /// </summary>
    /// <param name="start">Начальная позиция выделения</param>
    /// <param name="length">Длина выделенного фрагмента</param>
    void Select(int start, int length);

    /// <summary>
    /// Выделить весь текст в элементе
    /// </summary>
    void SelectAll();
  }

  /// <summary>
  /// Интерфейс, объявляющий свойства Text, CanBeEmpty и MaxLength.
  /// Применяется вместо EFPTextBoxAnyControl там, где шаблоны неудобны
  /// </summary>
  public interface IEFPTextBox : IEFPSimpleTextBox
  {
    /// <summary>
    /// Управляемое свойство Text
    /// </summary>
    DepValue<string> TextEx { get; set; }

    /// <summary>
    /// Основное свойство для проверки пустых значений. По умолчанию равно Error - пустые значения не допускаются
    /// </summary>
    UIValidateState CanBeEmptyMode { get; set; }

    /// <summary>
    /// true, если элемент может содержать пустой текст.
    /// Дублирует CanBeEmptyMode
    /// </summary>
    bool CanBeEmpty { get; set; }

    /// <summary>
    /// Управляемое свойство, возвращающее true, если есть введенный текст
    /// </summary>
    DepValue<bool> IsNotEmptyEx { get; }

    /// <summary>
    /// Ограничение на максимальную длину текста
    /// </summary>
    int MaxLength { get; set; }

    /// <summary>
    /// Разрешает использование "серого" значения
    /// </summary>
    bool AllowDisabledText { get; set; }

    /// <summary>
    /// "Серое" значение, используемое, когда текст в поле нельзя редактировать.
    /// </summary>
    string DisabledText { get; set; }

    /// <summary>
    /// Управляемое свойство DisabledText
    /// </summary>
    DepValue<string> DisabledTextEx { get; set; }

    /// <summary>
    /// Контекст для поиска текста
    /// </summary>
    IEFPTextSearchContext TextSearchContext { get; }

    /// <summary>
    /// Возвращает true для TextBox и RichTextBox
    /// </summary>
    bool MultiLineSupported { get; }

    /// <summary>
    /// Возвращает true, если в управляющем элементе находится многострочный текст
    /// </summary>
    bool IsMultiLine { get; }

    /// <summary>
    /// Возвращает true, если управляющий элемент использует клавишу Enter для вставки новой строки.
    /// Для EFPTextBox возвращает свойство Control.AcceptReturns. Для остальных элементов возвращает false
    /// </summary>
    bool AcceptsReturn { get; }

    /// <summary>
    /// Для EFPTextBox вовзращает признак ввода звездочек.
    /// Для остальных элементов возвращает false
    /// </summary>
    bool IsPasswordInput { get; }

    /// <summary>
    /// Возвращает true, если управляющий элемент в приципе поддерживает операцию Undo
    /// </summary>
    bool UndoSupported { get; }

    /// <summary>
    /// Возвращает true, если в данный момент доступна операция Undo
    /// </summary>
    bool CanUndo { get; }

    /// <summary>
    /// Выполняет отмену
    /// </summary>
    void Undo();

    /// <summary>
    /// "Вырезать"
    /// </summary>
    void Cut();

    /// <summary>
    /// "Скопировать"
    /// </summary>
    void Copy();

    /// <summary>
    /// "Вставить"
    /// </summary>
    void Paste();
  }

  /// <summary>
  /// Интерфейс для поддержки статусной строки в текстовом управляющем элементе
  /// </summary>
  public interface IEFPTextBoxWithStatusBar : IEFPTextBox
  {
    // /// <summary>
    // /// Возвращает true, если в управляющем элементе находится многострочный текст
    // /// </summary>
    // bool IsMultiLine { get; }

    /// <summary>
    /// Возвращает текущую позицию курсора в виде номера строки и столбца для отображения в статусной строке
    /// Нумерация начинается с 1, а не с нуля
    /// </summary>
    /// <param name="row">Сюда должен быть записан номер текущей строки. Нумерация с 1. 
    /// Для однострочных элементов должно возвращать 1</param>
    /// <param name="column">Сюда должен быть записан номер текущего столбца. Нумерация с 1</param>
    void GetCurrentRC(out int row, out int column);
  }

  #endregion

  #region Интерфейс IEFPReadOnly

  /// <summary>
  /// Интерфейс, публикующий свойство ReadOnly. Некоторые управляющие элементы,
  /// предназначенные для ввода текста, поддерживают это свойство, а другие - нет
  /// </summary>
  public interface IEFPReadOnlyControl : IEFPControl
  {
    /// <summary>
    /// true, если управляющий элемент позволяет просматривать, но не редактировать данные
    /// </summary>
    bool ReadOnly { get; set; }

    /// <summary>
    /// Управляяемое свойство ReadOnly
    /// </summary>
    DepValue<bool> ReadOnlyEx { get; set; }
  }

  #endregion

  /// <summary>
  /// Базовый класс для ComboBox с DropDownStyle=DropDown, TextBox и MaskedTextBox
  /// </summary>
  public abstract class EFPTextBoxAnyControl<T> : EFPSyncControl<T>, IEFPTextBox
    where T : Control
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPTextBoxAnyControl(EFPBaseProvider baseProvider, T control)
      : base(baseProvider, control, true)
    {
      Init();
    }

    /// <summary>
    /// Создает провайдер управляющего элемента
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент с панелью</param>
    public EFPTextBoxAnyControl(IEFPControlWithToolBar<T> controlWithToolBar)
      : base(controlWithToolBar, true)
    {
      Init();
    }

    private void Init()
    {
      _AllowDisabledText = false;
      _DisabledText = String.Empty;
      _SavedText = Control.Text;
      _CanBeEmptyMode = UIValidateState.Error;
      if (!DesignMode)
        Control.TextChanged += new EventHandler(Control_TextChanged);
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Проверка корректности значения.
    /// Используется свойство CanBeEmptyMode
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (base.ValidateState == UIValidateState.Error)
        return; // никогда не будет

      if (String.IsNullOrEmpty(Text))
      {
        switch (CanBeEmptyMode)
        {
          case UIValidateState.Error:
            SetError("Поле \"" + DisplayName + "\" должно быть заполнено");
            break;
          case UIValidateState.Warning:
            SetWarning("Поле \"" + DisplayName + "\" , вероятно, должно быть заполнено");
            break;
        }
      }
    }

    /// <summary>
    /// Синхронизированное значение (дублирует свойство Text)
    /// </summary>
    public override object SyncValue
    {
      get
      {
        return Text;
      }
      set
      {
        Text = (string)value;
      }
    }

    /// <summary>
    /// Ицициализация текста при смене состояния блокировки элемента
    /// </summary>
    protected override void OnEnabledStateChanged()
    {
      base.OnEnabledStateChanged();
      if (AllowDisabledText && EnabledState)
        _HasSavedText = true;
      InitControlText();
    }

    /// <summary>
    /// Создает EFPTextBoxCommandItems.
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      //if (EFPApp.EasyInterface)
      //  return base.GetCommandItems();
      //else
      return new EFPTextBoxCommandItems(this, true); // с командами преобразования регистра
    }

    /// <summary>
    /// Команда локального меню
    /// </summary>
    public new EFPTextBoxCommandItems CommandItems
    {
      get { return (EFPTextBoxCommandItems)(base.CommandItems); }
      set { base.CommandItems = value; }
    }

    #endregion

    #region Свойство MaxLength

    /// <summary>
    /// Максимальная длина текста
    /// </summary>
    public int MaxLength
    {
      get { return ControlMaxLength; }
      set
      {
        if (value == ControlMaxLength)
          return;
        ControlMaxLength = value;
        Validate();
      }
    }

    /// <summary>
    /// Это свойство используется для доступа к свойству MaxLength у управляющего элемента
    /// </summary>
    protected abstract int ControlMaxLength { get; set; }

    #endregion

    #region Свойство Text

    /// <summary>
    /// Введенный текст (свойство Control.Text)
    /// </summary>
    public string Text
    {
      get { return ControlText; }
      set
      {
        _SavedText = value;
        _HasSavedText = true;
        InitControlText();
      }
    }

    private string _SavedText;
    private bool _HasSavedText;

    /// <summary>
    /// Установка свойства ControlText
    /// </summary>
    protected void InitControlText()
    {
      // Не нужно, иначе может не обновляться
      //if (InsideTextChanged)
      //  return;
      if (AllowDisabledText && (!EnabledState))
      {
        if (!String.Equals(ControlText, DisabledText)) // 21.01.2022
          ControlText = DisabledText;
      }
      else if (_HasSavedText)
      {
        _HasSavedText = false;
        if (!String.Equals(ControlText, _SavedText)) // 21.01.2022
          ControlText = _SavedText;
      }
    }

    /// <summary>
    /// Свойство TextEx
    /// </summary>
    public DepValue<string> TextEx
    {
      get
      {
        InitTextEx();
        return _TextEx;
      }
      set
      {
        InitTextEx();
        _TextEx.Source = value;
      }
    }
    private DepInput<string> _TextEx;

    private void InitTextEx()
    {
      if (_TextEx == null)
      {
        _TextEx = new DepInput<string>(Text, TextEx_ValueChanged);
        _TextEx.OwnerInfo = new DepOwnerInfo(this, "TextEx");
      }
    }

    void TextEx_ValueChanged(object sender, EventArgs args)
    {
      Text = _TextEx.Value;
    }

    private void Control_TextChanged(object sender, EventArgs args)
    {
      try
      {
        if (!_InsideTextChanged)
        {
          _InsideTextChanged = true;
          try
          {
            OnTextChanged();
          }
          finally
          {
            _InsideTextChanged = false;
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика Control.TextChanged");
      }
    }

    /// <summary>
    /// Возвращает true, если в настоящий момент обрабатывается событие Control.TextChanged.
    /// </summary>
    protected bool InsideTextChanged { get { return _InsideTextChanged; } }
    private bool _InsideTextChanged;

    /// <summary>
    /// Метод вызывается при изменении текста в управляющем элементе.
    /// Переопределенный метод может, например, установить свойство ValueToolTipText.
    /// Обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnTextChanged()
    {
      if (_TextEx != null)
        _TextEx.Value = Text;

      if (AllowDisabledText && EnabledState)
        _SavedText = Text;

      Validate();
      DoSyncValueChanged();
    }

    /// <summary>
    /// Получение текста от управляющего элемента.
    /// Этот метод переопределен для MaskedTextBox
    /// </summary>
    /// <returns></returns>
    protected virtual string ControlText
    {
      get { return Control.Text; }
      set { Control.Text = value; }
    }

    /// <summary>
    /// Объект содержит true, если есть введенная строка
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
          _IsNotEmptyEx = new DepExpr1<bool, string>(TextEx, CalcIsNotEmpty);
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmpty(string text)
    {
      return !String.IsNullOrEmpty(text);
    }

    #endregion

    #region Свойство DisabledText

    /// <summary>
    /// Этот текст замещает свойство Text, когда Enabled=false или ReadOnly=true (для управляющих элементов, у которых есть это свойство).
    /// Свойство действует при установленном свойстве AllowDisabledText.
    /// По умолчанию - пустая строка.
    /// </summary>
    public string DisabledText
    {
      get { return _DisabledText; }
      set
      {
        if (value == _DisabledText)
          return;
        _DisabledText = value;
        if (_DisabledTextEx != null)
          _DisabledTextEx.Value = value;
        InitControlText();
      }
    }
    private string _DisabledText;

    /// <summary>
    /// Этот текст замещает свойство Text, когда Enabled=false или ReadOnly=true (для управляющих элементов, у которых есть это свойство).
    /// Свойство действует при установленном свойстве AllowDisabledText.
    /// По умолчанию - пустая строка.
    /// Управляемое свойство для DisabledText.
    /// </summary>
    public DepValue<string> DisabledTextEx
    {
      get
      {
        InitDisabledTextEx();
        return _DisabledTextEx;
      }
      set
      {
        InitDisabledTextEx();
        _DisabledTextEx.Source = value;
      }
    }

    private void InitDisabledTextEx()
    {
      if (_DisabledTextEx == null)
      {
        _DisabledTextEx = new DepInput<string>(DisabledText, DisabledTextEx_ValueChanged);
        _DisabledTextEx.OwnerInfo = new DepOwnerInfo(this, "DisabledTextEx");
      }
    }
    private DepInput<string> _DisabledTextEx;

    /// <summary>
    /// Вызывается, когда снаружи было изменено свойство DisabledCheckStateEx
    /// </summary>
    private void DisabledTextEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledText = _DisabledTextEx.Value;
    }

    /// <summary>
    /// Разрешает использование свойства DisabledText
    /// </summary>
    public bool AllowDisabledText
    {
      get { return _AllowDisabledText; }
      set
      {
        if (value == _AllowDisabledText)
          return;
        _AllowDisabledText = value;
        InitControlText();
      }
    }
    private bool _AllowDisabledText;

    #endregion

    #region Свойство CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - Error.
    /// Это свойство переопределяется для нестандартных элементов, содержащих
    /// кнопку очистки справа от элемента
    /// </summary>
    public virtual UIValidateState CanBeEmptyMode
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
    /// True, если ли элемент может содержать пустой текст.
    /// Дублирует CanBeEmptyMode
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Выбранный текст

    /// <summary>
    /// Начальная позиция выделения или позиция курсора, если выделения нет
    /// </summary>
    public abstract int SelectionStart { get; set; }

    /// <summary>
    /// Длина выделенного фрагмента текста
    /// </summary>
    public abstract int SelectionLength { get; set; }

    /// <summary>
    /// Выделенный текст.
    /// Установка свойства заменяет выделенный текст
    /// </summary>
    public abstract string SelectedText { get; set; }

    /// <summary>
    /// Выделяет весь текст в просмотре
    /// </summary>
    public abstract void SelectAll();

    /// <summary>
    /// Одновременная установка свойств SelectionStart и SelectionLength.
    /// </summary>
    /// <param name="start">Начальная позиция выделения</param>
    /// <param name="length">Длина выделенного фрагмента текста</param>
    public abstract void Select(int start, int length);

    #endregion

    #region Поиск текста

    /// <summary>
    /// Контекст поиска по Ctrl-F / F3
    /// </summary>
    public IEFPTextSearchContext TextSearchContext
    {
      get
      {
        if (TextSearchEnabled)
        {
          if (_TextSearchContext == null)
            _TextSearchContext = CreateTextSearchContext();
          return _TextSearchContext;
        }
        else
          return null;
      }
    }
    private IEFPTextSearchContext _TextSearchContext;

    /// <summary>
    /// Вызывается при первом обращении к свойству TextSearchContext.
    /// Непереопределенный метод создает и возвращает объект EFPTextSearchContext
    /// </summary>
    /// <returns></returns>
    protected virtual IEFPTextSearchContext CreateTextSearchContext()
    {
      return new EFPTextBoxSearchContext(this);
    }

    /// <summary>
    /// Если true, то доступна команда "Найти" (Ctrl-F).
    /// Если false, то свойство TextSearchContext возвращает null и поиск недоступен.
    /// Свойство имеет по умолчанию значение true, если поле предназначено для ввода многострочного
    /// текста и false - если для однострочного
    /// Свойство можно устанавливать только до вывода поля на экран.
    /// </summary>
    public bool TextSearchEnabled
    {
      get
      {
        if (_TextSearchEnabled.HasValue)
          return _TextSearchEnabled.Value;
        else
        {
          IEFPTextBoxWithStatusBar sb = this as IEFPTextBoxWithStatusBar;
          if (sb == null)
            return false;
          else
            return sb.IsMultiLine;
        }
      }
      set
      {
        CheckHasNotBeenCreated();
        _TextSearchEnabled = value;
      }
    }
    private bool? _TextSearchEnabled;

    #endregion

    #region IEFPTextBox Members

    /// <summary>
    /// Реализация интерфейса IEFPTextBox.
    /// Свойство должно быть переопределено.
    /// </summary>
    public virtual bool MultiLineSupported { get { return false; } }

    /// <summary>
    /// Реализация интерфейса IEFPTextBox.
    /// Свойство должно быть переопределено.
    /// </summary>
    public virtual bool IsMultiLine { get { return false; } }

    /// <summary>
    /// Реализация интерфейса IEFPTextBox.
    /// Свойство должно быть переопределено.
    /// </summary>
    public virtual bool AcceptsReturn { get { return false; } }

    /// <summary>
    /// Реализация интерфейса IEFPTextBox.
    /// Свойство должно быть переопределено.
    /// </summary>
    public virtual bool IsPasswordInput { get { return false; } }

    /// <summary>
    /// Реализация интерфейса IEFPTextBox.
    /// Свойство должно быть переопределено.
    /// </summary>
    public virtual bool UndoSupported { get { return false; } }

    /// <summary>
    /// Реализация интерфейса IEFPTextBox.
    /// Свойство должно быть переопределено.
    /// </summary>
    public virtual bool CanUndo { get { return false; } }

    /// <summary>
    /// Реализация интерфейса IEFPTextBox.
    /// Метод должен быть переопределен.
    /// </summary>
    public virtual void Undo()
    {
      throw new NotSupportedException("Команда Undo не поддерживается для " + GetType().ToString());
    }

    /// <summary>
    /// Реализация интерфейса IEFPTextBox.
    /// Метод может быть переопределен, если управляющий элемент имеет собственную реализацию метода Cut().
    /// </summary>
    public virtual void Cut()
    {
      if (String.IsNullOrEmpty(SelectedText))
        EFPApp.ShowTempMessage("Нет выделенного фрагмента текста");
      else
      {
        Clipboard.SetText(SelectedText);
        SelectedText = String.Empty;
      }
    }

    /// <summary>
    /// Реализация интерфейса IEFPTextBox.
    /// Метод может быть переопределен, если управляющий элемент имеет собственную реализацию метода Copy().
    /// </summary>
    public virtual void Copy()
    {
      if (String.IsNullOrEmpty(SelectedText))
        EFPApp.ShowTempMessage("Нет выделенного фрагмента текста");
      else
        Clipboard.SetText(SelectedText);
    }

    /// <summary>
    /// Реализация интерфейса IEFPTextBox.
    /// Метод может быть переопределен, если управляющий элемент имеет собственную реализацию метода Paste().
    /// </summary>
    public virtual void Paste()
    {
      String s = Clipboard.GetText();
      if (String.IsNullOrEmpty(s))
        EFPApp.ShowTempMessage("В буфере обмена нет текста");
      else
        SelectedText = s;
    }

    #endregion

    #region IEFPSimpleTextBox Members

    /// <summary>
    /// IEFPSimpleTextBox
    /// </summary>
    public virtual int TextLength
    {
      get
      {
        string s = Text;
        if (String.IsNullOrEmpty(s))
          return 0;
        else
          return s.Length;
      }
    }

    #endregion
  }

  /// <summary>
  /// Добавляет поддержку свойства ReadOnly
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class EFPTextBoxControlWithReadOnly<T> : EFPTextBoxAnyControl<T>, IEFPReadOnlyControl
    where T : Control
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPTextBoxControlWithReadOnly(EFPBaseProvider baseProvider, T control)
      : base(baseProvider, control)
    {
    }

    /// <summary>
    /// Создает провайдер управляющего элемента
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    public EFPTextBoxControlWithReadOnly(IEFPControlWithToolBar<T> controlWithToolBar)
      : base(controlWithToolBar)
    {
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Возвращает true, если установлены свойства Enabled=true и ReadOnly=false.
    /// </summary>
    public override bool EnabledState
    {
      get { return Enabled && (!ReadOnly); }
    }

    /// <summary>
    /// Блокировка при синхронизации выполняется не через свойство EnabledEx, как
    /// у других управляющих элементов, а через свойство ReadOnly
    /// </summary>
    /// <param name="value">True-выключить блокировку, false-включить</param>
    public override void SyncMasterState(bool value)
    {
      InitReadOnlyEx();
      _NotReadOnlySync.Value = value;
    }

    #endregion

    #region Свойство ReadOnly

    /// <summary>
    /// Режим работы текстового поля для просмотра без возможности редактирования.
    /// По умолчанию - false - пользователь может редактировать текст
    /// В отличие от установки свойства Enabled=false, при RedaOnly=true пользователь может выделять текст и копировать его в буфер обмена.
    /// Контроль введенного значения не выполняется при ReadOnly=true.
    /// </summary>
    public bool ReadOnly
    {
      get { return ControlReadOnly; }
      set
      {
        if (value == ControlReadOnly)
          return;
        ControlReadOnly = value;

        if (_ReadOnlyEx != null)
          _ReadOnlyEx.Value = value;
        UpdateEnabledState();
      }
    }

    /// <summary>
    /// Свойство Control.ReadOnly
    /// </summary>
    protected abstract bool ControlReadOnly { get; set; }

    /// <summary>
    /// Управляемое свойство ReadOnly
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
  }

  /// <summary>
  /// Провайдер управляющего элемента TextBox.
  /// </summary>
  public class EFPTextBox : EFPTextBoxControlWithReadOnly<TextBox>, IEFPTextBoxWithStatusBar
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор провайдера
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPTextBox(EFPBaseProvider baseProvider, TextBox control)
      : base(baseProvider, control)
    {
    }

    /// <summary>
    /// Конструктор провайдера.
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент со статусной строкой</param>
    public EFPTextBox(IEFPControlWithToolBar<TextBox> controlWithToolBar)
      : base(controlWithToolBar)
    {
      Control.Multiline = true;
    }

    #endregion

    #region Переопределенные свойства

    /// <summary>
    /// Свойство TextBox.ReadOnly
    /// </summary>
    protected override bool ControlReadOnly
    {
      get { return Control.ReadOnly; }
      set { Control.ReadOnly = value; }
    }

    /// <summary>
    /// Свойство TextBox.MaxLength
    /// </summary>
    protected override int ControlMaxLength
    {
      get { return Control.MaxLength; }
      set { Control.MaxLength = value; }
    }

    /// <summary>
    /// Возвращает true, если установлено свойство TextBox.PasswordChar или UseSystemPasswordChar
    /// </summary>
    public override bool IsPasswordInput
    {
      get
      {
        return Control.UseSystemPasswordChar || (Control.PasswordChar != 0);
      }
    }

    #endregion

    #region Свойства для выделения текста

    /// <summary>
    /// Начало выделенного фрагмента текста
    /// </summary>
    public override int SelectionStart
    {
      get { return Control.SelectionStart; }
      set { Control.SelectionStart = value; }
    }

    /// <summary>
    /// Длина выделенного фрагмента
    /// </summary>
    public override int SelectionLength
    {
      get { return Control.SelectionLength; }
      set { Control.SelectionLength = value; }
    }

    /// <summary>
    /// Выделенный фрагмент
    /// </summary>
    public override string SelectedText
    {
      get { return Control.SelectedText; }
      set { Control.SelectedText = value; }
    }

    /// <summary>
    /// Вызов Control.Select() и ScrollToCaret()
    /// </summary>
    /// <param name="start">Начало выделенного фрагмента текста</param>
    /// <param name="length">Длина выделенного фрагмента</param>
    public override void Select(int start, int length)
    {
      Control.Select(start, length);
      Control.ScrollToCaret();
    }

    /// <summary>
    /// Выделить весь текст в поле ввода
    /// </summary>
    public override void SelectAll()
    {
      Control.SelectAll();
    }

    #endregion

    #region IEFPTextBox

    /// <summary>
    /// Возвращает Control.TextLength
    /// </summary>
    public override int TextLength { get { return Control.TextLength; } }

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool UndoSupported { get { return true; } }

    /// <summary>
    /// Возвращает true, если есть правка, которую можно отменить
    /// </summary>
    public override bool CanUndo { get { return Control.CanUndo; } }

    /// <summary>
    /// Выполняет команду "Отменить"
    /// </summary>
    public override void Undo()
    {
      Control.Undo();
    }

    /// <summary>
    /// Возвращает true.
    /// </summary>
    public override bool MultiLineSupported { get { return true; } }

    /// <summary>
    /// Возвращает Control.Multiline
    /// </summary>
    public override bool IsMultiLine { get { return Control.Multiline; } }

    /// <summary>
    /// Возвращает Control.AcceptReturns
    /// </summary>
    public override bool AcceptsReturn { get { return Control.AcceptsReturn; } }

    /// <summary>
    /// Выполняет команду "Вырезать"
    /// </summary>
    public override void Cut()
    {
      Control.Cut();
    }

    /// <summary>
    /// Выполняет команду "Копировать"
    /// </summary>
    public override void Copy()
    {
      Control.Copy();
    }

    /// <summary>
    /// Выполняет команду "Вставить"
    /// </summary>
    public override void Paste()
    {
      Control.Paste();
    }

    #endregion

    #region IEFPTextBoxWithStatusBar Members

    void IEFPTextBoxWithStatusBar.GetCurrentRC(out int row, out int column)
    {
      // Проблема
      // Методы TextBoxBase.GetXXX() возвращают номера строк и столбцов с учетом переноса, если WrapText=true
      // Для статусной строки желательно показывать "правильные" номера строки и столбца, без учета переноса
      GetCurrentRC(Control.Text, Control.SelectionStart, out row, out column);
    }

    internal static void GetCurrentRC(string text, int pos, out int row, out int column)
    {
      // Находим все символы переноса строки

      // 02.09.2015
      // Проверяем все сепараторы новой строки, а не только Environment.NewLine.
      // Сначала длинные, затем, короткие

      int p = 0;
      row = 1;
      while (p < pos)
      {
        bool Found = false;
        for (int i = 0; i < DataTools.AllPossibleLineSeparators.Length; i++)
        {
          int p2 = text.IndexOf(DataTools.AllPossibleLineSeparators[i], p, pos - p, StringComparison.Ordinal);
          if (p2 < 0)
            continue;
          row++;
          p = p2 + DataTools.AllPossibleLineSeparators[i].Length;
          Found = true;
          break;
        }

        if (!Found)
          break;
      }

      column = pos - p + 1;
    }

    #endregion
  }

  /// <summary>
  /// Провайдер текстового поля для ввода текста с маской MaskedTextBox.
  /// </summary>
  public class EFPMaskedTextBox : EFPTextBoxControlWithReadOnly<MaskedTextBox>, IEFPTextBoxWithStatusBar
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPMaskedTextBox(EFPBaseProvider baseProvider, MaskedTextBox control)
      : base(baseProvider, control)
    {
    }

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Проверка корректности введенного значения.
    /// Кроме проверки на пустое значение, выполняется проверка ImaskProvider.Test(),
    /// если свойство MaskProvider установлено. Иначе выполняется проверка с использованием
    /// объекта MaskedTextProvider, заданным в управляющем элементе.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      // На момент вызова свойство Control всегда установлено

      if (ValidateState == UIValidateState.Error)
        return;

      if (MaskCanBePartial)
        return;

      if (MaskProvider == null)
      {
        if (Control.MaskedTextProvider == null)
          return;
        if (Control.MaskedTextProvider.AssignedEditPositionCount == 0)
          return; // Нет ни одного введенного символа

        if (!Control.MaskCompleted)
          SetError("Должны быть введены все символы");
      }
      else
      {
        string errorText;
        if (!MaskProvider.Test(ControlText, out errorText))
          SetError(errorText);
      }
    }

    /// <summary>
    /// Получение текста от управляющего элемента
    /// Когда не заполнено ни одного символа по маске, свойство MaskedTextBox.Text
    /// иногда возвращает пустую строку, а иногда - маску с пробелами вместо знаков
    /// </summary>
    protected override string ControlText
    {
      get
      {
        if (Control.MaskedTextProvider == null)
          return Control.Text;
        if (Control.MaskedTextProvider.AssignedEditPositionCount == 0)
          return "";

        // Убрано 27.03.2013
        // if (Control.MaskCompleted)
        //   return Control.Text;

        // Маска заполнена частично
        int p = Control.MaskedTextProvider.LastAssignedPosition;
        if (p >= Control.Text.Length)
          return Control.Text;
        return Control.Text.Substring(0, p + 1);
      }
      set
      {
        Control.Text = value;
      }
    }

    /// <summary>
    /// Дублирует свойство MaskedTextBox.ReadOnly
    /// </summary>
    protected override bool ControlReadOnly
    {
      get { return Control.ReadOnly; }
      set { Control.ReadOnly = value; }
    }

    /// <summary>
    /// Дублирует свойство MaskedTextBox.MaxLength
    /// </summary>
    protected override int ControlMaxLength
    {
      get { return Control.MaxLength; }
      set { Control.MaxLength = value; }
    }

    #endregion

    #region Свойство Mask

    /// <summary>
    /// Дублирует свойство MaskedTextBox.Mask.
    /// По умолчанию - пустая строка.
    /// Следует использовать это свойство, а не оригинальное, так как иначе не будет работать
    /// управляемое свойство MaskEx.
    /// </summary>
    public string Mask
    {
      get { return Control.Mask; }
      set
      {
        if (value == Control.Mask)
          return;
        Control.Mask = value;
        if (_MaskEx != null)
          _MaskEx.Value = value;
        Validate();
      }
    }

    /// <summary>
    /// Управляемое свойство Mask
    /// </summary>
    public DepValue<string> MaskEx
    {
      get
      {
        InitMaskEx();
        return _MaskEx;
      }
      set
      {
        InitMaskEx();
        _MaskEx.Source = value;
      }
    }

    private void InitMaskEx()
    {
      if (_MaskEx == null)
      {
        _MaskEx = new DepInput<string>(Mask, MaskEx_ValueChanged);
        _MaskEx.OwnerInfo = new DepOwnerInfo(this, "MaskEx");
      }
    }
    private DepInput<string> _MaskEx;

    private void MaskEx_ValueChanged(object sender, EventArgs args)
    {
      Mask = _MaskEx.Value;
    }

    #endregion

    #region Свойство MaskProvider

    /// <summary>
    /// Провайдер обработки маски.
    /// Позволяет использовать нестандартные маски.
    /// Установка свойства также устанавливает свойство Mask, используя IMaskProvider.EditMask.
    /// </summary>
    public IMaskProvider MaskProvider
    {
      get { return _MaskProvider; }
      set
      {
        _MaskProvider = value;
        if (value != null)
        {
          Mask = value.EditMask;
          Control.Culture = value.Culture; // 04.06.2019
        }
        else // 04.06.2019
        {
          Mask = String.Empty;
          Control.Culture = System.Globalization.CultureInfo.CurrentCulture;
        }
        Validate();
      }
    }
    private IMaskProvider _MaskProvider;

    #endregion

    #region Свойство MaskCanBePartial

    /// <summary>
    /// Доступ к MaskCanBePartialEx.ValueEx без принудительного создания объекта
    /// По умолчанию - false
    /// </summary>
    public bool MaskCanBePartial
    {
      get { return _MaskCanBePartial; }
      set
      {
        if (value == _MaskCanBePartial)
          return;
        _MaskCanBePartial = value;
        if (_MaskCanBePartialEx != null)
          _MaskCanBePartialEx.Value = value;
        Validate();
      }
    }
    private bool _MaskCanBePartial;

    /// <summary>
    /// Свойство, разрешающее частичное заполнение маски. По умолчанию - false.
    /// При этом выдается ошибка, если введены не все символы маски. Однако, 
    /// полностью пустое значение проверяется с помощью свойства EmptyCheck
    /// </summary>
    public DepValue<bool> MaskCanBePartialEx
    {
      get
      {
        InitMaskCanBePartialEx();
        return _MaskCanBePartialEx;
      }
      set
      {
        InitMaskCanBePartialEx();
        _MaskCanBePartialEx.Source = value;
      }
    }

    private void InitMaskCanBePartialEx()
    {
      if (_MaskCanBePartialEx == null)
      {
        _MaskCanBePartialEx = new DepInput<bool>(MaskCanBePartial, MaskCanBePartialEx_ValueChanged);
        _MaskCanBePartialEx.OwnerInfo = new DepOwnerInfo(this, "MaskCanBePartialEx");
      }
    }
    private DepInput<Boolean> _MaskCanBePartialEx;

    private void MaskCanBePartialEx_ValueChanged(object sender, EventArgs args)
    {
      MaskCanBePartial = _MaskCanBePartialEx.Value;
    }

    #endregion

    #region Свойства для выделения текста

    /// <summary>
    /// Дублирует MaskedTextBox.SelectionStart
    /// </summary>
    public override int SelectionStart
    {
      get { return Control.SelectionStart; }
      set { Control.SelectionStart = value; }
    }

    /// <summary>
    /// Дублирует MaskedTextBox.SelectionLength
    /// </summary>
    public override int SelectionLength
    {
      get { return Control.SelectionLength; }
      set { Control.SelectionLength = value; }
    }

    /// <summary>
    /// Дублирует MaskedTextBox.SelectedText
    /// </summary>
    public override string SelectedText
    {
      get { return Control.SelectedText; }
      set { Control.SelectedText = value; }
    }

    /// <summary>
    /// Дублирует MaskedTextBox.Select()
    /// </summary>
    /// <param name="start"></param>
    /// <param name="length"></param>
    public override void Select(int start, int length)
    {
      Control.Select(start, length);
      // ?? Control.ScrollToCaret();
    }

    /// <summary>
    /// Дублирует MaskedTextBox.SelectAll()
    /// </summary>
    public override void SelectAll()
    {
      Control.SelectAll();
    }

    #endregion

    #region IEFPTextBox

    /// <summary>
    /// Возвращает Control.TextLength
    /// </summary>
    public override int TextLength { get { return Control.TextLength; } }

    // Методы Undo не поддерживаются

    /// <summary>
    /// Выполняет команду "Вырезать"
    /// </summary>
    public override void Cut()
    {
      Control.Cut();
    }

    /// <summary>
    /// Выполняет команду "Копировать"
    /// </summary>
    public override void Copy()
    {
      Control.Copy();
    }

    /// <summary>
    /// Выполняет команду "Вставить"
    /// </summary>
    public override void Paste()
    {
      Control.Paste();
    }

    #endregion

    #region IEFPTextBoxWithStatusBar Members

    void IEFPTextBoxWithStatusBar.GetCurrentRC(out int row, out int column)
    {
      row = 1;
      column = Control.SelectionStart + 1;
    }

    #endregion
  }

  /// <summary>
  /// Провайдер управляющего элемента Rich Text Box
  /// </summary>
  public class EFPRichTextBox : EFPTextBoxControlWithReadOnly<RichTextBox>, IEFPTextBoxWithStatusBar
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPRichTextBox(EFPBaseProvider baseProvider, RichTextBox control)
      : base(baseProvider, control)
    {
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    public EFPRichTextBox(IEFPControlWithToolBar<RichTextBox> controlWithToolBar)
      : base(controlWithToolBar)
    {
    }

    #endregion

    #region Переопределенные свойства

    /// <summary>
    /// Дублирует свойство RichTextBox.ReadOnly
    /// </summary>
    protected override bool ControlReadOnly
    {
      get { return Control.ReadOnly; }
      set { Control.ReadOnly = value; }
    }

    /// <summary>
    /// Дублирует свойство RichTextBox.MaxLength
    /// </summary>
    protected override int ControlMaxLength
    {
      get { return Control.MaxLength; }
      set { Control.MaxLength = value; }
    }

    #endregion

    #region Свойства для выделения текста

    /// <summary>
    /// Дублирует свойство в RichTextBox
    /// </summary>
    public override int SelectionStart
    {
      get { return Control.SelectionStart; }
      set { Control.SelectionStart = value; }
    }

    /// <summary>
    /// Дублирует свойство в RichTextBox
    /// </summary>
    public override int SelectionLength
    {
      get { return Control.SelectionLength; }
      set { Control.SelectionLength = value; }
    }

    /// <summary>
    /// Дублирует свойство в RichTextBox
    /// </summary>
    public override string SelectedText
    {
      get { return Control.SelectedText; }
      set { Control.SelectedText = value; }
    }

    /// <summary>
    /// Дублирует метод в RichTextBox
    /// </summary>
    /// <param name="start"></param>
    /// <param name="length"></param>
    public override void Select(int start, int length)
    {
      Control.Select(start, length);
      Control.ScrollToCaret();
    }

    /// <summary>
    /// Дублирует метод в RichTextBox
    /// </summary>
    public override void SelectAll()
    {
      Control.SelectAll();
    }

    #endregion

    #region IEFPTextBox

    /// <summary>
    /// Возвращает Control.TextLength
    /// </summary>
    public override int TextLength { get { return Control.TextLength; } }

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool UndoSupported { get { return true; } }

    /// <summary>
    /// Возвращает true, если есть правка, которую можно отменить
    /// </summary>
    public override bool CanUndo { get { return Control.CanUndo; } }

    /// <summary>
    /// Выполняет команду "Отменить"
    /// </summary>
    public override void Undo()
    {
      Control.Undo();
    }

    /// <summary>
    /// Возвращает true.
    /// </summary>
    public override bool MultiLineSupported { get { return true; } }

    /// <summary>
    /// Возвращает Control.Multiline
    /// </summary>
    public override bool IsMultiLine { get { return Control.Multiline; } }

    /// <summary>
    /// Всегда возвращает false
    /// </summary>
    public override bool AcceptsReturn { get { return false; } }

    /// <summary>
    /// Выполняет команду "Вырезать"
    /// </summary>
    public override void Cut()
    {
      Control.Cut();
    }

    /// <summary>
    /// Выполняет команду "Копировать"
    /// </summary>
    public override void Copy()
    {
      Control.Copy();
    }

    /// <summary>
    /// Выполняет команду "Вставить"
    /// </summary>
    public override void Paste()
    {
      Control.Paste();
    }

    #endregion

    #region IEFPTextBoxWithStatusBar Members

    void IEFPTextBoxWithStatusBar.GetCurrentRC(out int row, out int column)
    {
      EFPTextBox.GetCurrentRC(Control.Text, Control.SelectionStart, out row, out column);
    }

    #endregion
  }

  /// <summary>
  /// Поле ввода одного или нескольких кодов, разделенных запятыми.
  /// </summary>
  public class EFPCsvCodesTextBox : EFPTextBox
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPCsvCodesTextBox(EFPBaseProvider baseProvider, TextBox control)
      : base(baseProvider, control)
    {
      _UseSpace = true;
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
        if (_CodeValidators != null)
          _CodeValidators.InternalSetValue(s);
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
    /// Список объектов-валидаторов для проверки корректности значения выбранных кодов.
    /// Используйте в качестве проверочного выражение какую-либо вычисляемую функцию, основанную на управляемом свойстве ValidatingCodeEx
    /// (и на других управляемых свойствах, в том числе, других элементов формы).
    /// В основном, предназначено для проверки в удаленном интерфейсе.
    /// В обычных приложениях удобнее использовать обработчик события CodeValidating.
    /// </summary>
    public UIValueValidatorList<string> CodeValidators
    {
      get
      {
        if (_CodeValidators == null)
          _CodeValidators = new UIValueValidatorList<string>();
        return _CodeValidators;
      }
    }
    private UIValueValidatorList<string> _CodeValidators;

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

      if (_CodeValidators != null)
      {
        _CodeValidators.SetReadOnly();

        foreach (FreeLibSet.UICore.UIValidator v in this._CodeValidators)
        {
          v.ResultEx.ValueChanged += new EventHandler(this.Validate); // Не знаю, нужно ли
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
  /// Подключение к полю ввода текста команд буфера обмена
  /// </summary>
  public class EFPTextBoxCommandItems : EFPControlCommandItems
  {
    #region Конструкторы

    /// <summary>
    /// Создает набор команд
    /// </summary>
    /// <param name="owner">Провайдер управляющего элемента</param>
    /// <param name="useConvert">true, если будут использоваться команды подменю "Преобразовать текст"</param>
    public EFPTextBoxCommandItems(IEFPSimpleTextBox owner, bool useConvert)
      : this(owner, useConvert, false)
    {
    }

    /// <summary>
    /// Создает набор команд
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="useConvert">true, если будут использоваться команды подменю "Преобразовать текст"</param>
    /// <param name="alwaysReadOnly">Если true, то не будут созданы команды "Вырезать" и "Вставить",
    /// а будет только команда "Копировать"</param>
    public EFPTextBoxCommandItems(IEFPSimpleTextBox controlProvider, bool useConvert, bool alwaysReadOnly)
      :base((EFPControlBase)controlProvider)
    {
      _Owner = controlProvider;

      #region Undo

      bool undoSupported = false;
      if (controlProvider is IEFPTextBox)
        undoSupported = ((IEFPTextBox)controlProvider).UndoSupported;

      if (undoSupported)
      {
        ciUndo = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Undo);
        ciUndo.ShortCutToRightText();
        ciUndo.GroupBegin = true;
        ciUndo.Click += new EventHandler(Undo);
        ciUndo.MenuText = "&Отменить";
        Add(ciUndo);
      }

      #endregion

      #region Буфер обмена

      if (!alwaysReadOnly)
      {
        ciCut = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Cut);
        ciCut.ShortCutToRightText();
        ciCut.GroupBegin = true;
        ciCut.Click += new EventHandler(Cut);
        Add(ciCut);
      }
      ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
      ciCopy.ShortCutToRightText();
      ciCopy.Click += new EventHandler(Copy);
      Add(ciCopy);

      if (!alwaysReadOnly)
      {
        ciPaste = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Paste);
        ciPaste.ShortCutToRightText();
        ciPaste.GroupEnd = true;
        ciPaste.Click += new EventHandler(Paste);
        Add(ciPaste);
      }
      //ciDelete=new VisinleClientItem(MainMenu.Delete);
      //ciDelete.Click+=new EventHandler(Delete);
      //Add(ciDelete);

      #endregion

      #region Выделить все

      ciSelectAll = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SelectAll);
      // Убрано 20.09.2017 ciSelectAll.ShortCutToRightText();
      ciSelectAll.GroupBegin = true;
      ciSelectAll.GroupEnd = true;
      ciSelectAll.Click += new EventHandler(SelectAll);
      Add(ciSelectAll);

      #endregion

      if (controlProvider is IEFPTextBox)
      {
        #region Поиск

        ciFind = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Find);
        ciFind.Click += new EventHandler(Find);
        ciFind.GroupBegin = true;
        Add(ciFind);

        ciFindNext = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.FindNext);
        ciFindNext.Click += new EventHandler(FindNext);
        ciFindNext.GroupEnd = true;
        Add(ciFindNext);

        #endregion
      }

      if (controlProvider is IEFPTextBox)
      {
        if (((IEFPTextBox)controlProvider).MultiLineSupported)
        {
          #region Вставить перевод строки

          ciNewLine = new EFPCommandItem("Edit", "InsertNewLine");
          ciNewLine.MenuText = "Вставить перевод строки";
          ciNewLine.ImageKey = "InsertNewLine";
          ciNewLine.GroupBegin = true;
          ciNewLine.GroupEnd = true;
          ciNewLine.Click += new EventHandler(ciNewLine_Click);
          Add(ciNewLine);

          #endregion

          #region Открыть и Открыть с помощью

          AddSeparator();
          _FileAssociationsHandler = new EFPFileAssociationsCommandItemsHandler(this, ".txt"); // TODO: Для RichTextBox можно было бы и rtf-формат сделать
          _FileAssociationsHandler.FileNeeded += new System.ComponentModel.CancelEventHandler(FileAssociationsHandler_FileNeeded);

          #endregion

          #region Отправить

          ciMenuSendTo = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.MenuSendTo);
          ciMenuSendTo.Usage = EFPCommandItemUsage.Menu;
          Add(ciMenuSendTo);
          AddSeparator();

          ciSendToMicrosoftWord = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SendToMicrosoftWord);
          ciSendToMicrosoftWord.Parent = ciMenuSendTo;
          ciSendToMicrosoftWord.Click += SendToMicrosoftWord;
          Add(ciSendToMicrosoftWord);

          ciSendToOpenOfficeWriter = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.SendToOpenOfficeWriter);
          ciSendToOpenOfficeWriter.Parent = ciMenuSendTo;
          ciSendToOpenOfficeWriter.Click += SendToOpenOfficeWriter;
          Add(ciSendToOpenOfficeWriter);

          #endregion
        }
      }

      #region Преобразование символов

      _UseConvert = useConvert;
      if (useConvert)
      {
        // Подменю команд преобразования регистра
        ciCase = new EFPCommandItem("Edit", "MenuCase");
        ciCase.MenuText = "Преобразовать текст";
        ciCase.GroupBegin = true;
        Add(ciCase);

        ciUpperCase = new EFPCommandItem("Edit", "ToUpper");
        ciUpperCase.Parent = ciCase;
        ciUpperCase.MenuText = "К &ВЕРХНЕМУ РЕГИСТРУ";
        ciUpperCase.ShortCut = Keys.Control | Keys.U;
        ciUpperCase.GroupBegin = true;
        ciUpperCase.Click += new EventHandler(UpperCase);
        //ciUpperCase.GroupBegin=true;
        Add(ciUpperCase);

        ciLowerCase = new EFPCommandItem("Edit", "ToLower");
        ciLowerCase.Parent = ciCase;
        ciLowerCase.MenuText = "к &нижнему регистру";
        ciLowerCase.ShortCut = Keys.Control | Keys.L;
        ciLowerCase.Click += new EventHandler(LowerCase);
        Add(ciLowerCase);

        ciChangeCase = new EFPCommandItem("Edit", "InverCase");
        ciChangeCase.Parent = ciCase;
        ciChangeCase.MenuText = "&иЗМЕНИТЬ рЕГИСТР";
        ciChangeCase.ShortCut = Keys.Control | Keys.R;
        ciChangeCase.GroupEnd = true;
        ciChangeCase.Click += new EventHandler(ChangeCase);
        Add(ciChangeCase);

        ciRusLat = new EFPCommandItem("Edit", "ChangeRusLat");
        ciRusLat.Parent = ciCase;
        ciRusLat.MenuText = "Преобразовать РУС <-> ЛАТ";
        ciRusLat.ShortCut = Keys.Control | Keys.T;
        ciRusLat.GroupBegin = true;
        ciRusLat.GroupEnd = true;
        ciRusLat.Click += new EventHandler(RusLat);
        //ciRusLat.GroupEnd = true;
        Add(ciRusLat);
      }

      #endregion

      #region Панели статусной строки

      if (controlProvider is IEFPTextBoxWithStatusBar)
      {
        UseStatusBarRC = true;

        ciStatusRow = new EFPCommandItem("View", "StatusRow");
        ciStatusRow.Usage = EFPCommandItemUsage.StatusBar;
        ciStatusRow.StatusBarText = "???";
        ciStatusRow.Click += new EventHandler(EFPCommandItem.NoActionClick);
        Add(ciStatusRow);

        ciStatusColumn = new EFPCommandItem("View", "StatusColumn");
        ciStatusColumn.Usage = EFPCommandItemUsage.StatusBar;
        ciStatusColumn.StatusBarText = "???";
        ciStatusColumn.Click += new EventHandler(EFPCommandItem.NoActionClick);
        Add(ciStatusColumn);
      }

      #endregion
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер управляющего элемента.
    /// Он должен реализовывать интерфейс IEFPSimpleTextBox, может также реализовывать IEFPTextBox и IEFPTextBoxWithStatusBar
    /// </summary>
    public IEFPSimpleTextBox Owner { get { return _Owner; } }
    private IEFPSimpleTextBox _Owner;

    /*
     * Лень делать
    /// <summary>
    /// Если свойство установить в true, то команда "Вставить" обрабатывает
    /// формат буфера обмена "FileDrop"
    /// Свойство устанавливается для полей ввода и комбоблоков выбора файлов и папок
    /// </summary>
    public bool UseFileName;
     * */

    /// <summary>
    /// Установка свойств EFPCommandItem.Usage
    /// </summary>
    protected override void OnPrepare()
    {
      base.OnPrepare();

      if (ciFind != null)
      {
        if (((IEFPTextBox)Owner).TextSearchContext == null)
        {
          ciFind.Usage = EFPCommandItemUsage.None;
          ciFindNext.Usage = EFPCommandItemUsage.None;
        }
      }

      if (ciCase != null && (!_UseConvert))
      {
        ciCase.Usage = EFPCommandItemUsage.None;
        ciUpperCase.Usage = EFPCommandItemUsage.None;
        ciLowerCase.Usage = EFPCommandItemUsage.None;
        ciChangeCase.Usage = EFPCommandItemUsage.None;
        ciRusLat.Usage = EFPCommandItemUsage.None;
      }

      Owner.EditableEx.ValueChanged += new EventHandler(InitEnabled);
      ControlProvider.Control.TextChanged += new EventHandler(InitEnabled);
      ControlProvider.Control.KeyUp += new KeyEventHandler(KeyUp);
      ControlProvider.Control.MouseUp += new MouseEventHandler(MouseUp);

      // Начальная установка
      if (!UseStatusBarRC)
      {
        if (ciStatusRow != null)
          ciStatusRow.Visible = false;
        if (ciStatusColumn != null)
          ciStatusColumn.Visible = false;
      }

      InitEnabled();
      RefreshSearchItems();
    }

    #endregion

    #region Внутренняя реализация

    private EFPCommandItem ciUndo, ciCut, ciCopy, ciPaste, /*ciDelete, */ciSelectAll;
    private EFPCommandItem ciCase;
    private EFPCommandItem ciUpperCase, ciLowerCase, ciChangeCase, ciRusLat;
    private EFPCommandItem ciNewLine;

    ///// <summary>
    ///// Возвращает TextBox.AcceptsReturn.
    ///// Для остальных управляющих элементов возвращает false.
    ///// </summary>
    //protected bool ControlAcceptsReturn
    //{
    //  get
    //  {
    //    if (Control is TextBox)
    //      return ((TextBox)Control).AcceptsReturn;
    //    if (Control is RichTextBox)
    //      return false; // всегда используется Ctrl-Enter
    //    return false;
    //  }
    //}

    /// <summary>
    /// Возвращает true для TextBox, если он предназначен для ввода пароля
    /// </summary>
    //protected bool IsPasswordInput
    //{
    //  get
    //  {
    //    TextBox tb = Owner.Control as TextBox;
    //    if (tb != null)
    //      return tb.UseSystemPasswordChar || (tb.PasswordChar != 0);
    //    else
    //      return false;
    //  }
    //}

    private void InitEnabled(object sender, EventArgs args)
    {
      InitEnabled();
    }

    /// <summary>
    /// Инициализация видимости команд меню.
    /// Вызывается при любом изменении текста.
    /// Если пользовательский код устанавливает свойства TextBox.UseSystemPasswordChar или
    /// PasswordChar (которые не имеют связанных событий) в процессе показа окна
    /// (обычно нажатие кнопки "Показать введенные символы"), то этот метод
    /// следует вызывать в явном виде.
    /// </summary>
    public virtual void InitEnabled()
    {
      IEFPTextBox owner2 = Owner as IEFPTextBox;
      bool isPasswordInput = false;
      bool canUndo = false; // выделено 27.12.2020
      bool isMultiLine = false; // выделено 27.12.2020
      bool acceptsReturn = false; // выделено 27.12.2020
      if (owner2 != null)
      {
        isPasswordInput = owner2.IsPasswordInput;
        canUndo = owner2.CanUndo;
        isMultiLine = owner2.IsMultiLine;
        acceptsReturn = owner2.AcceptsReturn;
      }

      int textLength = Owner.TextLength;
      int selectionLength = Owner.SelectionLength;

      if (ciUndo != null)
        ciUndo.Enabled = (Owner.Editable && canUndo);
      if (ciCut != null)
        ciCut.Enabled = (Owner.Editable && selectionLength > 0) &&
          (!isPasswordInput); // 24.01.2019
      if (ciCopy != null)
        ciCopy.Enabled = (selectionLength > 0) &&
          (!isPasswordInput); // 24.01.2019
      if (ciPaste != null)
        ciPaste.Enabled = Owner.Editable; // !!! проверка буфера обмена
      //ciDelete.EnabledEx=((!Control.DataReadOnly) && Control.SelectionLength>0);
      ciSelectAll.Enabled = (selectionLength < textLength);
      if (ciCase != null)
      {
        ciCase.Visible = !isPasswordInput; // 24.01.2019

        ciUpperCase.Enabled = Owner.Editable && (textLength > 0) /*&& ControlCanChangeCase*/;
        ciLowerCase.Enabled = ciUpperCase.Enabled;
        ciChangeCase.Enabled = ciUpperCase.Enabled;
        ciRusLat.Enabled = Owner.Editable && (textLength > 0);
        ciCase.Enabled = Owner.Editable;
      }

      if (ciNewLine != null)
      {
        ciNewLine.Visible = isMultiLine;
        ciNewLine.Enabled = Owner.Editable;
        ciNewLine.MenuRightText = EFPCommandItem.GetShortCutText(acceptsReturn ? Keys.Enter :
          Keys.Control | Keys.Enter);
      }

      if (ciSendToMicrosoftWord != null)
        ciSendToMicrosoftWord.Visible = EFPApp.MicrosoftWordVersion.Major > 0 && isMultiLine;
      if (ciSendToOpenOfficeWriter != null)
        ciSendToOpenOfficeWriter.Visible = EFPApp.OpenOfficeKind != OpenOfficeKind.Unknown && isMultiLine;

      if (_FileAssociationsHandler != null)
        _FileAssociationsHandler.Visible = (!isPasswordInput) && // 24.01.2019
                                             isMultiLine; // 30.07.2020

      InitStatusBar();
    }

    private void KeyUp(object sender, KeyEventArgs args)
    {
      InitEnabled();
    }

    private void MouseUp(object sender, MouseEventArgs args)
    {
      InitEnabled();
    }

    private void Undo(object sender, EventArgs args)
    {
      if (Owner is IEFPTextBox)
        ((IEFPTextBox)Owner).Undo();
      else
        throw new BugException("Операция Undo неприменима к элементу " + ControlProvider.Control.GetType().ToString());
      InitEnabled();
    }

    private void Cut(object sender, EventArgs args)
    {
      if (Owner is IEFPTextBox)
        ((IEFPTextBox)Owner).Cut();
      else
      {
        EFPApp.Clipboard.SetText(Owner.SelectedText);
        Owner.SelectedText = String.Empty;
      }
      InitEnabled();
    }

    private void Copy(object sender, EventArgs args)
    {
      if (Owner is IEFPTextBox)
        ((IEFPTextBox)Owner).Copy();
      else
      {
        string Txt = Owner.SelectedText;
        EFPApp.Clipboard.SetText(Txt);
      }
      InitEnabled();
    }

    private void Paste(object sender, EventArgs args)
    {
      string s = EFPApp.Clipboard.GetText();
      if (EFPApp.Clipboard.HasError)
        return;
      if (!String.IsNullOrEmpty(s))
      {
        if (Owner is IEFPTextBox)
          ((IEFPTextBox)Owner).Paste();
        else
          Owner.SelectedText = s;

        InitEnabled();
        return;
      }

      /*
      if (UseFileName)
      {
        string[] FileNames = Clipboard.GetData(DataFormats.FileDrop) as string[];
        PasteFileNames(FileNames);
      }
       * */

      EFPApp.ShowTempMessage("Буфер обмена не содержит текста");
    }

    /*
    private void PasteFileNames(string[] FileNames)
    {
      if (FileNames == null)
        return;

      if (FileNames.Length != 1)
      {
        EFPApp.ShowTempMessage("Поддерживается вставка только одного файла");
        return;
      }

      Control.Text = FileNames[0];
    }
     * */

    //private void Delete(object Sender, EventArgs Args)
    //{
    //  Control.Clear();
    //  InitEnabled();
    //}

    private void SelectAll(object sender, EventArgs args)
    {
      Owner.SelectAll();

      InitEnabled();
    }

    ///// <summary>
    ///// Выделение фрагмента текста.
    ///// Если провайдер управляющего элемента реализует интерфейс IEFPSimpleTextBox, то вызывается
    ///// его метод Select(). Иначе выбрасывается исключение
    ///// </summary>
    ///// <param name="start"></param>
    ///// <param name="lenght"></param>
    //protected void ControlSelect(int start, int lenght)
    //{
    //  if (Owner is IEFPSimpleTextBox)
    //    ((IEFPSimpleTextBox)Owner).Select(start, lenght);
    //  else
    //    throw new BugException("Операция Select неприменима к элементу " + Control.GetType().ToString());
    //}

    void ciNewLine_Click(object sender, EventArgs args)
    {
      Owner.SelectedText = Environment.NewLine;
    }

    #endregion

    #region Команды поиска

    EFPCommandItem ciFind, ciFindNext;

    private void Find(object sender, EventArgs args)
    {
      ((IEFPTextBox)Owner).TextSearchContext.StartSearch();
      RefreshSearchItems();
    }

    private void FindNext(object sender, EventArgs args)
    {
      ((IEFPTextBox)Owner).TextSearchContext.ContinueSearch();
    }

    private void RefreshSearchItems()
    {
      IEFPTextBox owner2 = Owner as IEFPTextBox;
      if (owner2 == null)
        return;
      if (owner2.TextSearchContext == null)
        return;
      ciFindNext.Enabled = owner2.TextSearchContext.ContinueEnabled;
    }

    #endregion

    #region Команды "Преобразовать"

    /// <summary>
    /// Доступность команд "Преобразовать".
    /// По умолчанию - true.
    /// Свойство имеет смысл только для элементов, производных от EFPTextBoxAnyControl
    /// </summary>
    public bool UseConvert
    {
      get { return _UseConvert; }
      set
      {
        CheckNotReadOnly();
        _UseConvert = value;
      }
    }
    private bool _UseConvert;

    private void UpperCase(object sender, EventArgs args)
    {
      DoChangeText(1);
    }

    private void LowerCase(object sender, EventArgs args)
    {
      DoChangeText(2);
    }

    private void ChangeCase(object sender, EventArgs args)
    {
      DoChangeText(3);
    }

    private void RusLat(object sender, EventArgs args)
    {
      DoChangeText(4);
    }

    private void DoChangeText(int mode)
    {
      string s;

      if (Owner.SelectionLength == 0)
      {
        int pos = Owner.SelectionStart;
        s = ControlProvider.Control.Text;
        s = DoChangeText2(s, mode);
        ControlProvider.Control.Text = s;
        Owner.SelectionStart = pos;
      }
      else
      {
        int pos = Owner.SelectionStart;
        int len = Owner.SelectionLength;
        s = Owner.SelectedText;
        s = DoChangeText2(s, mode);
        Owner.SelectedText = s;

        Owner.Select(pos, len);
      }
      InitEnabled();
    }

    private static string DoChangeText2(string s, int mode)
    {
      switch (mode)
      {
        case 1:
          return s.ToUpper();
        case 2:
          return s.ToLower();
        case 3:
          return DataTools.ChangeUpperLowerInvariant(s);
        case 4:
          return WinFormsTools.ChangeRusLat(s);
        default:
          throw new ArgumentException();
      }
    }

    #endregion

    #region Команды "Открыть" и "Открыть с помощью"

    /// <summary>
    /// Обработчик команд "Открыть" и "Открыть с помощью"
    /// </summary>
    protected EFPFileAssociationsCommandItemsHandler FileAssociationsHandler { get { return _FileAssociationsHandler; } }
    private EFPFileAssociationsCommandItemsHandler _FileAssociationsHandler;

    /// <summary>
    /// Получение файла
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    protected virtual void FileAssociationsHandler_FileNeeded(object sender, CancelEventArgs args)
    {
      if (Owner.TextLength == 0)
      {
        EFPApp.ShowTempMessage("Нет текста");
        args.Cancel = true;
        return;
      }

      // 13.12.2018
      // Под Windows-98/Me нужно использовать кодировку ANSI
      Encoding enc = FileTools.TextFileEncoding;

      // Каждый раз создаем новую копию
      FileAssociationsHandler.FilePath = EFPApp.SharedTempDir.GetTempFileName("txt");
      System.IO.File.WriteAllText(FileAssociationsHandler.FilePath.Path,
        Owner.Text, enc);
    }

    #endregion

    #region Команды "Отправить"

    private EFPCommandItem ciMenuSendTo;
    private EFPCommandItem ciSendToMicrosoftWord, ciSendToOpenOfficeWriter;

    private void SendToMicrosoftWord(object sender, EventArgs args)
    {
      string text = Owner.Text;
      if (String.IsNullOrEmpty(text))
      {
        EFPApp.ShowTempMessage("Текст не введен");
        return;
      }

      try
      {
        using (FreeLibSet.OLE.Word.WordHelper helper = new OLE.Word.WordHelper())
        {
          FreeLibSet.OLE.Word.Document doc = helper.Application.Documents.Add();
          doc.Range().InsertAfter(text);
          doc.Saved = true;
        }
      }
      catch (Exception e)
      {
        MessageBox.Show("Не удалось запустить Microsoft Word. " + e.Message);
      }
    }

    private void SendToOpenOfficeWriter(object sender, EventArgs args)
    {
      string text = Owner.Text;
      if (String.IsNullOrEmpty(text))
      {
        EFPApp.ShowTempMessage("Текст не введен");
        return;
      }

      string tempFileName = EFPApp.SharedTempDir.GetTempFileName("TXT").Path;
      System.IO.File.WriteAllText(tempFileName, text, Encoding.Default);
      EFPApp.UsedOpenOffice.OpenWithWriter(new AbsPath(tempFileName), true);
    }

    #endregion

    #region Статусная строка

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то в статусной строке выводятся панельки "Строка"
    /// и "Столбец" для отображения текущей позиции.
    /// Свойство может быть сброшено в false для отключения панелек только после конструктора управляющего 
    /// элемента. Динамическая установка свойства недоступна.
    /// Свойство недействительно для управляющих элементов, не реализующих интерфейс IEFPTextBoxWithStatusBar
    /// </summary>
    public bool UseStatusBarRC { get { return _UseStatusBarRC; } set { _UseStatusBarRC = value; } }
    private bool _UseStatusBarRC;

    EFPCommandItem ciStatusRow, ciStatusColumn;

    private void InitStatusBar()
    {
      if (ciStatusRow == null)
        return;

      IEFPTextBoxWithStatusBar tb = (IEFPTextBoxWithStatusBar)Owner;

      ciStatusRow.Visible = tb.IsMultiLine && UseStatusBarRC /* 21.02.2020 */;
      int currRow, currCol;
      tb.GetCurrentRC(out currRow, out currCol);
      ciStatusRow.StatusBarText = "Строка " + currRow.ToString();
      ciStatusColumn.StatusBarText = "Столбец " + currCol.ToString();
    }

    #endregion
  }
}
