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
    int TextLength { get;}

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
  /// Интерфейс, объявляющий свойства Text, CanBeEmptyEx и MaxLength.
  /// Применяется вместо EFPTextBoxAnyControl там, где шаблоны неудобны
  /// </summary>
  public interface IEFPTextBox : IEFPSimpleTextBox
  {
    /// <summary>
    /// Управляемое свойство Text
    /// </summary>
    DepValue<string> TextEx { get; set; }

    /// <summary>
    /// true, если элемент может содержать пустой текст.
    /// </summary>
    bool CanBeEmpty { get; set; }

    /// <summary>
    /// Управляемое свойство CanBeEmpty
    /// </summary>
    DepValue<bool> CanBeEmptyEx { get; set; }

    /// <summary>
    /// true, если нужно выдавать предупреждение, когда в элементе введен пустой текст.
    /// Свойство действует только при CanBeEmpty=true
    /// </summary>
    bool WarningIfEmpty { get; set; }

    /// <summary>
    /// Управляемое свойство WarningIfEmpty
    /// </summary>
    DepValue<bool> WarningIfEmptyEx { get; set; }

    /// <summary>
    /// Ограничение на максимальную длину текста
    /// </summary>
    int MaxLength { get; set; }

    /// <summary>
    /// Управляемое свойство MaxLength
    /// </summary>
    DepValue<int> MaxLengthEx { get; set; }

    /// <summary>
    /// Разрешает использование "серого" значения
    /// </summary>
    bool AllowDisabledText { get; set;}

    /// <summary>
    /// "Серое" значение, используемое, когда текст в поле нельзя редактировать.
    /// </summary>
    string DisabledText { get;set;}

    /// <summary>
    /// Управляемое свойство DisabledText
    /// </summary>
    DepValue<string> DisabledTextEx { get; set;}

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
    bool IsPasswordInput { get;}

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
      _CanBeEmpty = true;
      if (!DesignMode)
        Control.TextChanged += new EventHandler(Control_TextChanged);
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Проверка корректности значения.
    /// Используются свойства CanBeEnpty и WarningIfEmpty
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (base.ValidateState == EFPValidateState.Error)
        return; // никогда не будет

      if (String.IsNullOrEmpty(Text))
      {
        if (CanBeEmpty)
        {
          if (WarningIfEmpty)
            SetWarning("Поле \"" + DisplayName + "\" , вероятно, должно быть заполнено");
        }
        else
          SetError("Поле \"" + DisplayName + "\" должно быть заполнено");
      }
      else
      {
        if (!String.IsNullOrEmpty(ErrorRegExPattern))
        {
          if (!Regex.IsMatch(Text, ErrorRegExPattern))
          {
            if (String.IsNullOrEmpty(ErrorRegExMessage))
              SetError("Значение не соответствует формату");
            else
              SetError(ErrorRegExMessage);
          }
        }

        if (base.ValidateState == EFPValidateState.Ok && (!String.IsNullOrEmpty(WarningRegExPattern)))
        {
          if (!Regex.IsMatch(Text, WarningRegExPattern))
          {
            if (String.IsNullOrEmpty(WarningRegExMessage))
              SetWarning("Значение не соответствует формату");
            else
              SetWarning(WarningRegExMessage);
          }
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
        if (_MaxLengthEx != null)
          _MaxLengthEx.Value = value;
        // ?? Validate();
      }
    }

    /// <summary>
    /// Управляемое свойство MaxLength
    /// </summary>
    public DepValue<int> MaxLengthEx
    {
      get
      {
        InitMaxLengthEx();
        return _MaxLengthEx;
      }
      set
      {
        InitMaxLengthEx();
        _MaxLengthEx.Source = value;
      }
    }

    private void InitMaxLengthEx()
    {
      if (_MaxLengthEx == null)
      {
        _MaxLengthEx = new DepInput<int>();
        _MaxLengthEx.OwnerInfo = new DepOwnerInfo(this, "MaxLengthEx");
        _MaxLengthEx.Value = MaxLength;
        _MaxLengthEx.ValueChanged += new EventHandler(MaxLengthEx_ValueChanged);
      }
    }

    private DepInput<int> _MaxLengthEx;

    private void MaxLengthEx_ValueChanged(object sender, EventArgs args)
    {
      MaxLength = _MaxLengthEx.Value;
    }

    /// <summary>
    /// Это свойство используется для доступа к свойству MaxLength у управляющего элемента
    /// </summary>
    protected abstract int ControlMaxLength { get; set; }

    #endregion

    #region Свойство Text

    /// <summary>
    /// Доступ к свойству Text.ValueEx без принудительного создания объекта
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
        Control.Text = DisabledText;
      else if (_HasSavedText)
      {
        _HasSavedText = false;
        Control.Text = _SavedText;
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
        _TextEx = new DepInput<string>();
        _TextEx.OwnerInfo = new DepOwnerInfo(this, "TextEx");
        _TextEx.Value = Text;
        _TextEx.ValueChanged += new EventHandler(TextEx_ValueChanged);
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
        _DisabledTextEx = new DepInput<string>();
        _DisabledTextEx.OwnerInfo = new DepOwnerInfo(this, "DisabledTextEx");
        _DisabledTextEx.Value = DisabledText;
        _DisabledTextEx.ValueChanged += new EventHandler(DisabledTextEx_ValueChanged);
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
    /// True, если ли элемент содержать пустой текст.
    /// Значение по умолчанию - true.
    /// 
    /// Это свойство переопределяется для нестандартных элементов, содержащих
    /// кнопку очистки справа от элемента
    /// </summary>
    public virtual bool CanBeEmpty
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

    /// <summary>
    /// True, если ли элемент содержать пустой текст.
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

    private DepInput<Boolean> _CanBeEmptyEx;

    void CanBeEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      CanBeEmpty = _CanBeEmptyEx.Value;
    }

    #endregion

    #region Свойство WarningIfEmpty

    /// <summary>
    /// Выдавать предупреждение, если текст не введен (при условии, что CanBeEmpty=true)
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
    /// Если True и свойство CanBeEmpty=True, то при проверке состояния выдается
    /// предупреждение, если свойство Text содержит пустую строку
    /// По умолчанию - False
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
    private DepInput<Boolean> _WarningIfEmptyEx;

    void WarningIfEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      WarningIfEmpty = _WarningIfEmptyEx.Value;
    }

    #endregion

    #region Свойства ErrorRegEx и WarningRegEx

    /// <summary>
    /// Проверка введенного значения с помощью регулярного выражения (RegularExpression).
    /// Проверка выполняется, если свойство содержит выражение, а поле ввода содержит непустое значение.
    /// Если в поле введен текст, не соответствующий выражению, выдается сообщение об ошибке, определяемое свойством ErrorRegExMessage.
    /// </summary>
    public string ErrorRegExPattern
    {
      get { return _ErrorRegExPattern; }
      set
      {
        if (value == _ErrorRegExPattern)
          return;
        _ErrorRegExPattern = value;
        Validate();
      }
    }
    private string _ErrorRegExPattern;

    /// <summary>
    /// Текст сообщения об ошибке, которое выводится, если введенное значение не соответствует регулярному выражению ErrorRegEx.
    /// Если свойство не установлено, используется сообщение по умолчанию.
    /// </summary>
    public string ErrorRegExMessage
    {
      get { return _ErrorRegExMessage; }
      set
      {
        if (value == _ErrorRegExMessage)
          return;
        _ErrorRegExMessage = value;
        Validate();
      }
    }
    private string _ErrorRegExMessage;

    /// <summary>
    /// Проверка введенного значения с помощью регулярного выражения (RegularExpression).
    /// Проверка выполняется, если свойство содержит выражение, а поле ввода содержит непустое значение.
    /// Если в поле введен текст, не соответствующий выражению, выдается предупреждение, определяемое свойством WarningRegExMessage.
    /// Проверка не выполняется, если обнаружена ошибка при проверке значения с помощью свойства ErrorRegEx.
    /// </summary>
    public string WarningRegExPattern
    {
      get { return _WarningRegExPattern; }
      set
      {
        if (value == _WarningRegExPattern)
          return;
        _WarningRegExPattern = value;
        Validate();
      }
    }
    private string _WarningRegExPattern;

    /// <summary>
    /// Текст предупреждения, которое выводится, если введенное значение не соответствует регулярному выражению WarningRegEx.
    /// Если свойство не установлено, используется сообщение по умолчанию.
    /// </summary>
    public string WarningRegExMessage
    {
      get { return _WarningRegExMessage; }
      set
      {
        if (value == _WarningRegExMessage)
          return;
        _WarningRegExMessage = value;
        Validate();
      }
    }
    private string _WarningRegExMessage;

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
          IEFPTextBoxWithStatusBar SB = this as IEFPTextBoxWithStatusBar;
          if (SB == null)
            return false;
          else
            return SB.IsMultiLine;
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

    #region Свойство DisabledText


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

    /// <summary>
    /// 02.09.2015
    /// Проверяем все сепараторы новой строки, а не только Environment.NewLine.
    /// Сначала длинные, затем, короткие
    /// </summary>
    private static readonly string[] NewLineSeps = new string[] { "\r\n", "\r\n", "\n" , "\r" };

    internal static void GetCurrentRC(string text, int pos, out int row, out int column)
    {
      // Находим все символы переноса строки
      int p = 0;
      row = 1;
      while (p < pos)
      {
        bool Found = false;
        for (int i = 0; i < NewLineSeps.Length; i++)
        {
          int p2 = text.IndexOf(NewLineSeps[i], p, pos - p, StringComparison.Ordinal);
          if (p2 < 0)
            continue;
          row++;
          p = p2 + NewLineSeps[i].Length;
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

      if (ValidateState == EFPValidateState.Error)
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
        string ErrorText;
        if (!MaskProvider.Test(ControlText, out ErrorText))
          SetError(ErrorText);
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
        _MaskEx = new DepInput<string>();
        _MaskEx.OwnerInfo = new DepOwnerInfo(this, "MaskEx");
        _MaskEx.Value = Mask;
        _MaskEx.ValueChanged += new EventHandler(MaskEx_ValueChanged);
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
        _MaskCanBePartialEx = new DepInput<bool>();
        _MaskCanBePartialEx.OwnerInfo = new DepOwnerInfo(this, "MaskCanBePartialEx");
        _MaskCanBePartialEx.Value = MaskCanBePartial;
        _MaskCanBePartialEx.ValueChanged += new EventHandler(FMaskCanBePartialEx_ValueChanged);
      }
    }
    private DepInput<Boolean> _MaskCanBePartialEx;

    private void FMaskCanBePartialEx_ValueChanged(object sender, EventArgs args)
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
    /// <param name="owner">Провайдер управляющего элемента</param>
    /// <param name="useConvert">true, если будут использоваться команды подменю "Преобразовать текст"</param>
    /// <param name="alwaysReadOnly">Если true, то не будут созданы команды "Вырезать" и "Вставить",
    /// а будет только команда "Копировать"</param>
    public EFPTextBoxCommandItems(IEFPSimpleTextBox owner, bool useConvert, bool alwaysReadOnly)
    {
      _Owner = owner;

      #region Undo

      bool UndoSupported = false;
      if (owner is IEFPTextBox)
        UndoSupported = ((IEFPTextBox)owner).UndoSupported;

      if (UndoSupported)
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

      if (owner is IEFPTextBox)
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

      if (owner is IEFPTextBox)
      {
        if (((IEFPTextBox)owner).MultiLineSupported)
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

      if (owner is IEFPTextBoxWithStatusBar)
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
    protected override void BeforeControlAssigned()
    {
      base.BeforeControlAssigned();
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
    }

    /// <summary>
    /// Присоединение обработчиков к управляющему элементу и установка свойств EFPCommandItem.Visible
    /// </summary>
    protected override void AfterControlAssigned()
    {
      base.AfterControlAssigned();

      Owner.EditableEx.ValueChanged += new EventHandler(InitEnabled);
      Control.TextChanged += new EventHandler(InitEnabled);
      Control.KeyUp += new KeyEventHandler(KeyUp);
      Control.MouseUp += new MouseEventHandler(MouseUp);

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
      IEFPTextBox Owner2 = Owner as IEFPTextBox;
      bool IsPasswordInput = false;
      bool CanUndo = false; // выделено 27.12.2020
      bool IsMultiLine = false; // выделено 27.12.2020
      bool AcceptsReturn = false; // выделено 27.12.2020
      if (Owner2 != null)
      {
        IsPasswordInput = Owner2.IsPasswordInput;
        CanUndo = Owner2.CanUndo;
        IsMultiLine = Owner2.IsMultiLine;
        AcceptsReturn = Owner2.AcceptsReturn;
      }

      int TextLength = Owner.TextLength;
      int SelectionLength = Owner.SelectionLength;

      if (ciUndo != null)
        ciUndo.Enabled = (Owner.Editable && CanUndo);
      if (ciCut != null)
        ciCut.Enabled = (Owner.Editable && SelectionLength > 0) &&
          (!IsPasswordInput); // 24.01.2019
      if (ciCopy != null)
        ciCopy.Enabled = (SelectionLength > 0) &&
          (!IsPasswordInput); // 24.01.2019
      if (ciPaste != null)
        ciPaste.Enabled = Owner.Editable; // !!! проверка буфера обмена
      //ciDelete.EnabledEx=((!Control.DataReadOnly) && Control.SelectionLength>0);
      ciSelectAll.Enabled = (SelectionLength < TextLength);
      if (ciCase != null)
      {
        ciCase.Visible = !IsPasswordInput; // 24.01.2019

        ciUpperCase.Enabled = Owner.Editable && (TextLength > 0) /*&& ControlCanChangeCase*/;
        ciLowerCase.Enabled = ciUpperCase.Enabled;
        ciChangeCase.Enabled = ciUpperCase.Enabled;
        ciRusLat.Enabled = Owner.Editable && (TextLength > 0);
        ciCase.Enabled = Owner.Editable;
      }

      if (ciNewLine != null)
      {
        ciNewLine.Visible = IsMultiLine;
        ciNewLine.Enabled = Owner.Editable;
        ciNewLine.MenuRightText = EFPCommandItem.GetShortCutText(AcceptsReturn ? Keys.Enter :
          Keys.Control | Keys.Enter);
      }

      if (ciSendToMicrosoftWord != null)
        ciSendToMicrosoftWord.Visible = EFPApp.MicrosoftWordVersion.Major > 0 && IsMultiLine;
      if (ciSendToOpenOfficeWriter != null)
        ciSendToOpenOfficeWriter.Visible = EFPApp.OpenOfficeKind != OpenOfficeKind.Unknown && IsMultiLine;

      if (_FileAssociationsHandler != null)
        _FileAssociationsHandler.Visible = (!IsPasswordInput) && // 24.01.2019
                                             IsMultiLine; // 30.07.2020

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
        throw new BugException("Операция Undo неприменима к элементу " + Control.GetType().ToString());
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
      IEFPTextBox Owner2 = Owner as IEFPTextBox;
      if (Owner2 == null)
        return;
      if (Owner2.TextSearchContext == null)
        return;
      ciFindNext.Enabled = Owner2.TextSearchContext.ContinueEnabled;
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
        CheckNotAssigned();
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
        s = Control.Text;
        s = DoChangeText2(s, mode);
        Control.Text = s;
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
      string Text = Owner.Text;
      if (String.IsNullOrEmpty(Text))
      {
        EFPApp.ShowTempMessage("Текст не введен");
        return;
      }

      try
      {
        using (FreeLibSet.OLE.Word.WordHelper Helper = new OLE.Word.WordHelper())
        {
          FreeLibSet.OLE.Word.Document Doc = Helper.Application.Documents.Add();
          Doc.Range().InsertAfter(Text);
          Doc.Saved = true;
        }
      }
      catch (Exception e)
      {
        MessageBox.Show("Не удалось запустить Microsoft Word. " + e.Message);
      }
    }

    private void SendToOpenOfficeWriter(object sender, EventArgs args)
    {
      string Text = Owner.Text;
      if (String.IsNullOrEmpty(Text))
      {
        EFPApp.ShowTempMessage("Текст не введен");
        return;
      }

      string TempFileName = EFPApp.SharedTempDir.GetTempFileName("TXT").Path;
      System.IO.File.WriteAllText(TempFileName, Text, Encoding.Default);
      EFPApp.UsedOpenOffice.OpenWithWriter(new AbsPath(TempFileName), true);
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
      int CurrRow, CurrCol;
      tb.GetCurrentRC(out CurrRow, out CurrCol);
      ciStatusRow.StatusBarText = "Строка " + CurrRow.ToString();
      ciStatusColumn.StatusBarText = "Столбец " + CurrCol.ToString();
    }

    #endregion
  }
}
