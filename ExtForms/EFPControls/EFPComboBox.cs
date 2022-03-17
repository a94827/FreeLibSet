// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.DependedValues;
using FreeLibSet.Formatting;
using FreeLibSet.Controls;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.UICore;

// Обработчики для комбоблоков отличаются. 
// EFPListComboBox предназначен для комбоблоков со стилем DropDownList. В них
// основным "значением" является свойство SelectedIndexEx
// EFPTextComboBox, напротив, предназначен для комбоблока, который используется 
// для редактирования текстового значения. В нем основным свойством является "Text"

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Обработчик для комбоблока, предназначенного для ввода текста
  /// </summary>
  public class EFPTextComboBox : EFPTextBoxAnyControl<ComboBox>, IEFPTextBoxWithStatusBar
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPTextComboBox(EFPBaseProvider baseProvider, ComboBox control)
      : base(baseProvider, control)
    {
      if (control.DropDownStyle == ComboBoxStyle.DropDownList)
        control.DropDownStyle = ComboBoxStyle.DropDown;

      _CharacterCasing = CharacterCasing.Normal;
    }

    #endregion

    #region Свойство Text

    /// <summary>
    /// Свойство ComboBox.Text.
    /// </summary>
    protected override string ControlText
    {
      get { return Control.Text; }
      set
      {
        // 06.12.2011
        // Без сброса SelectedIndex, если есть список строк в комбоблоке и до этого
        // была выбрана одна из строк до показа формы на экране, текст установится,
        // но после вывода формы на экран будет заменен выбранной ранее строкой
        //  Control.SelectedIndex = -1;

        // 09.12.2011
        // Работает, если комбоблок еще не выведен на экран.
        // Если комбоблок уже на экране и установлено свойство AutoCompleteMode=Append,
        // то начинаются глюки при ввода текста в редакторе адреса в поле "Улица"
        // и "населенный пункт", включая попытку записи по неправильному адресу памяти
        // Оставляем сброс, только если форма еще не на экране

        if (!Control.Visible)
          Control.SelectedIndex = -1;
        Control.Text = value;
      }
    }

    #endregion

    #region Свойство ControlMaxLength

    /// <summary>
    /// Свойство ComboBox.MaxLength.
    /// </summary>
    protected override int ControlMaxLength
    {
      get
      {
        return Control.MaxLength;
      }
      set
      {
        Control.MaxLength = value;
      }
    }

    #endregion

    #region Свойство CharacterCasing

    /// <summary>
    /// Автоматическое преобразование регистра символов к верхнему или нижнему регистру.
    /// По умолчанию преобразование не выполняется (CharacterCasing.Normal).
    /// </summary>
    public CharacterCasing CharacterCasing
    {
      get { return _CharacterCasing; }
      set
      {
        if (value == _CharacterCasing)
          return;
        _CharacterCasing = value;

        // При первой установке свойства присоединяем обработчик
        if (!_CharacterCasingWasSet)
        {
          Control.KeyPress += new KeyPressEventHandler(Control_KeyPress);
          Control.TextChanged += new EventHandler(Control_TextChanged);
          _CharacterCasingWasSet = true;
        }
      }
    }

    private CharacterCasing _CharacterCasing;
    private bool _CharacterCasingWasSet;


    private void Control_KeyPress(object sender, KeyPressEventArgs args)
    {
      try
      {
        DoControl_KeyPress(args);
      }
      catch
      {
      }
    }

    private void DoControl_KeyPress(KeyPressEventArgs args)
    {
      if (args.Handled)
        return;
      if (_CharacterCasing == CharacterCasing.Normal)
        return;
      string s = new string(args.KeyChar, 1);
      if (_CharacterCasing == CharacterCasing.Upper)
        s = s.ToUpper();
      else
        s = s.ToLower();
      if (s.Length == 1)
        args.KeyChar = s[0];
    }


    private void Control_TextChanged(object sender, EventArgs args)
    {
      try
      {
        DoControl_TextChanged();
      }
      catch
      {
      }
    }

    private void DoControl_TextChanged()
    {
      if (String.IsNullOrEmpty(Control.Text))
        return;

      string s2;
      switch (_CharacterCasing)
      {
        case CharacterCasing.Upper:
          s2 = Control.Text.ToUpper();
          break;
        case CharacterCasing.Lower:
          s2 = Control.Text.ToLower();
          break;
        default:
          return;
      }
      if (s2 == Control.Text)
        return;

      int oldSS = Control.SelectionStart;
      int oldSL = Control.SelectionLength;
      Control.Text = s2;
      Control.SelectionStart = oldSS;
      Control.SelectionLength = oldSL;

      // при вставке из буфера обмена выделение все равно не сохраняется
    }

    #endregion

    #region Свойства для выделения текста

    /// <summary>
    /// Свойство ComboBox.SelectionStart.
    /// </summary>
    public override int SelectionStart
    {
      get { return Control.SelectionStart; }
      set { Control.SelectionStart = value; }
    }

    /// <summary>
    /// Свойство ComboBox.SelectionLength.
    /// </summary>
    public override int SelectionLength
    {
      get { return Control.SelectionLength; }
      set { Control.SelectionLength = value; }
    }

    /// <summary>
    /// Свойство ComboBox.SelectedText.
    /// </summary>
    public override string SelectedText
    {
      get { return Control.SelectedText; }
      set { Control.SelectedText = value; }
    }

    /// <summary>
    /// Выделение фрагмента текста или установка позиции курсора.
    /// </summary>
    /// <param name="start">Начальная позиция</param>
    /// <param name="length">Длина выбранного текста</param>
    public override void Select(int start, int length)
    {
      Control.Select(start, length);
    }

    /// <summary>
    /// Выделение всего текста
    /// </summary>
    public override void SelectAll()
    {
      Control.SelectAll();
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
  /// Обработчик комбоблока для ввода текста и списком, хранящим историю изменений
  /// (например, список последних открытых файлов).
  /// Расширяет класс EFPTextComboBox массивом строк HistList
  /// </summary>
  public class EFPHistComboBox : EFPTextComboBox
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер управляющего элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPHistComboBox(EFPBaseProvider baseProvider, ComboBox control)
      : base(baseProvider, control)
    {
      _MaxHistLength = HistoryList.DefaultMaxHistLength;
      _DefaultItems = DataTools.EmptyStrings;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Максимальная длина списка истории.
    /// Возможные значения: от 2 до 100. По умолчанию: 10.
    /// Свойство должно устанавливаться до записи/чтения свойства HistList.
    /// </summary>
    public int MaxHistLength
    {
      get { return _MaxHistLength; }
      set
      {
        if (value < 2 || value > 100)
          throw new ArgumentOutOfRangeException("value", value,
            "Максимальное число строк истории может быть в диапазоне от 2 до 100");
        _MaxHistLength = value;
      }
    }
    private int _MaxHistLength;

    /// <summary>
    /// Список истории. Более новые значения идут в начале списка, а более старые 
    /// - в конце. Таким образом, первый элемент массива содержит текущее значение.
    /// При записи свойства заполняется выпадающий список, а свойство Text получает
    /// значение первой строки (если массив не пустой). Если массив длиннее MaxHistLength,
    /// то лишние элементы отбрасываются.
    /// При чтении свойства первому элементу массива присваивается текущее значение
    /// (свойство Text), остальные элементы сдвигаются. Если в списке истории 
    /// присутствует такое значение, то оно пропускается (то есть строки переставляются).
    /// Если значение не введено (пустое поле), то оно будет включено.
    /// Если требуется корректировка вводимого значения (например, добавление "\" к
    /// имени каталога), то она должна выполняться до чтения свойства.
    /// </summary>
    public HistoryList HistList
    {
      get
      {
        List<string> lst = new List<string>();

        // 07.09.2015
        // Добавляем в том числе и пустое значение.
        // Иначе при вводе пользователем пустого значения в поле, оно не будет получено 
        // if (!String.IsNullOrEmpty(Text))
        lst.Add(Text);

        for (int i = 0; i < Control.Items.Count; i++)
        {
          if (lst.Count >= MaxHistLength)
            break;
          string s = Control.Items[i].ToString();
          if (i > 0)
          {
            if (s == lst[0])
              continue;
          }
          int p = Array.IndexOf<string>(_DefaultItems, s);
          if (p >= 0)
          {
            if (_DefaultItemFlags[p])
              continue;
          }
          lst.Add(s);
        }
        return new HistoryList(lst.ToArray());
      }
      set
      {
        Control.Items.Clear();
        if (value.IsEmpty)
        {
          Text = String.Empty;
          return;
        }
        value = value.SetLimit(MaxHistLength);

        Control.Items.AddRange(value.ToArray());
        Control.SelectedIndex = 0;
      }
    }

    /// <summary>
    /// Дополнительные строки по умолчанию. По умолчанию - нет дополнительных строк (пустой массив).
    /// Свойство должно устанавливаться ПОСЛЕ HistList. При этом внизу списка 
    /// добавляются дополнительные строкм, если они отсутствуют в основном списке.
    /// При чтении свойства HistList дополнительные строки не попадают в список.
    /// </summary>
    public string[] DefaultItems
    {
      get { return _DefaultItems; }
      set
      {
        if (value == null)
          value = DataTools.EmptyStrings;
        _DefaultItems = value;
        _DefaultItemFlags = new bool[value.Length];
        for (int i = 0; i < value.Length; i++)
        {
          if (!Control.Items.Contains(value[i]))
          {
            Control.Items.Add(value[i]);
            _DefaultItemFlags[i] = true;
            if (i == 0 && Control.SelectedIndex < 0)
              Control.SelectedIndex = 0;
          }
        }
      }
    }
    private string[] _DefaultItems;
    private bool[] _DefaultItemFlags;

    #endregion
  }

  /// <summary>
  /// Обработчик для комбоблока, предназначенного для ввода текста с маской
  /// </summary>
  public class EFPMaskedComboBox : EFPTextBoxControlWithReadOnly<UserMaskedComboBox>, IEFPTextBoxWithStatusBar
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPMaskedComboBox(EFPBaseProvider baseProvider, UserMaskedComboBox control)
      : base(baseProvider, control)
    {
       control.ClearButtonToolTipText = "Очистить введенное значение";
    }

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Возвращает 32767
    /// </summary>
    protected override int ControlMaxLength
    {
      get
      {
        return Int16.MaxValue;
      }
      set
      {
        // Установка свойства не поддеживается
        //throw new NotSupportedException();
      }
    }

    /// <summary>
    /// Свойство Control.ReadOnly
    /// </summary>
    protected override bool ControlReadOnly
    {
      get { return Control.ReadOnly; }
      set { Control.ReadOnly = value; }
    }

    /// <summary>
    /// Установка видимости кнопки очистки [x] (свойство UserComboBoxBase.ClearButton).
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
    /// Проверка значения
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      // На момент вызова свойство Control всегда установлено

      if (MaskCanBePartial)
        return;

      if (ValidateState == UIValidateState.Error)
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
    /// иногда возвращает пустую строку, а иногда - маску с пробелами вместо знаков.
    /// </summary>
    /// <returns></returns>
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
        return Control.Text.Substring(0, p + 1);
      }
      set
      {
        Control.Text = value;
      }
    }

    #endregion

    #region Свойство Mask

    /// <summary>
    /// Маска.
    /// По умолчанию - пустая строка
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
    /// Провайдер для работы маски.
    /// Установка свойства автоматически устанавливает свойство Mask.
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
    /// Свойство, разрешающее частичное заполнение маски. По умолчанию - false.
    /// При этом выдается ошибка, если введены не все символы маски. Однако, 
    /// полностью пустое значение проверяется с помощью свойства CanBeEmpty.
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
    /// полностью пустое значение проверяется с помощью свойства CanBeEmpty.
    /// Управляемое свойство.
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
    /// Дублирует свойство объекта UserMaskedComboBox 
    /// </summary>
    public override int SelectionStart
    {
      get { return Control.SelectionStart; }
      set { Control.SelectionStart = value; }
    }

    /// <summary>
    /// Дублирует свойство объекта UserMaskedComboBox 
    /// </summary>
    public override int SelectionLength
    {
      get { return Control.SelectionLength; }
      set { Control.SelectionLength = value; }
    }

    /// <summary>
    /// Дублирует свойство объекта UserMaskedComboBox 
    /// </summary>
    public override string SelectedText
    {
      get { return Control.SelectedText; }
      set { Control.SelectedText = value; }
    }

    /// <summary>
    /// Дублирует метод объекта UserMaskedComboBox 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="length"></param>
    public override void Select(int start, int length)
    {
      Control.Select(start, length);
    }

    /// <summary>
    /// Дублирует метод объекта UserMaskedComboBox 
    /// </summary>
    public override void SelectAll()
    {
      Control.SelectAll();
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
  /// Провайдер комбоблока выбора цвета
  /// </summary>
  public class EFPColorComboBox : EFPControl<ColorComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPColorComboBox(EFPBaseProvider baseProvider, ColorComboBox control)
      : base(baseProvider, control, true)
    {
      if (!DesignMode)
        control.ColorChanged += new EventHandler(Control_ColorChanged);
    }

    #endregion

    #region Свойство Color

    /// <summary>
    /// Текущий выбранный цвет.
    /// Допускаются обычные цвета, заданные в RGB-формате (например, Color.Red) или
    /// пустой цвет (Color.Empty)
    /// Системные цвета (например, SystemColors.Window) не поддерживаются
    /// 
    /// Значение по умолчанию - Color.Empty
    /// </summary>
    public Color Color
    {
      get { return Control.Color; }
      set { Control.Color = value; }
    }

    void Control_ColorChanged(object sender, EventArgs args)
    {
      try
      {
        OnColorChanged();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика ColorComboBox.ColorChanged");
      }
    }

    /// <summary>
    /// Метод вызывается при изменении выбора в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnColorChanged()
    {
      if (_ColorEx != null)
        _ColorEx.OwnerSetValue(Color);
    }

    /// <summary>
    /// Текущий выбранный цвет.
    /// По умолчанию - нет выбранного цвета (Color.Empty)
    /// </summary>
    public DepValue<Color> ColorEx
    {
      get
      {
        InitColorEx();
        return _ColorEx;
      }
      set
      {
        InitColorEx();
        _ColorEx.Source = value;
      }
    }

    private void InitColorEx()
    {
      if (_ColorEx == null)
      {
        _ColorEx = new DepInput<Color>(Color, ColorEx_ValueChanged);
        _ColorEx.OwnerInfo = new DepOwnerInfo(this, "ColorEx");
      }
    }

    private DepInput<Color> _ColorEx;

    void ColorEx_ValueChanged(object sender, EventArgs args)
    {
      Color = _ColorEx.Value;
    }

    #endregion
  }


  /// <summary>
  /// Провайдер комбоблока, который рисуется в пользовательском коде.
  /// При нажатии уголочка вызывается пользовательский код, который должен показать блок диалога
  /// </summary>
  public class EFPOwnerDrawUserSelComboBox : EFPControl<ComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPOwnerDrawUserSelComboBox(EFPBaseProvider baseProvider, ComboBox control)
      : base(baseProvider, control, true)
    {
      control.DropDownStyle = ComboBoxStyle.DropDownList;
      control.DrawMode = DrawMode.OwnerDrawFixed;
      if (!DesignMode)
      {
        control.DrawItem += Control_DrawItem;
        control.DropDown += Control_DropDown;
      }

      UseIdle = true;
    }

    #endregion

    #region Виртуальные методы и события

    /// <summary>
    /// Событие вызывается при нажатии на уголок
    /// </summary>
    public event EventHandler Popup;

    /// <summary>
    /// Вызывает событие Popup
    /// </summary>
    /// <param name="args">Фиктивные аргументы</param>
    protected virtual void OnPopup(EventArgs args)
    {
      if (Popup != null)
        Popup(this, args);
    }

    /// <summary>
    /// Событие вызывается для прорисовки элемента
    /// </summary>
    public event DrawItemEventHandler DrawItem;

    /// <summary>
    /// Вызывает событие DrawItem
    /// </summary>
    /// <param name="args">Аргумент события</param>
    protected virtual void OnDrawItem(DrawItemEventArgs args)
    {
      if (DrawItem != null)
        DrawItem(this, args);
    }

    #endregion

    #region Обработчики

    void Control_DrawItem(object sender, DrawItemEventArgs args)
    {
      OnDrawItem(args);
    }

    void Control_DropDown(object sender, EventArgs args)
    {
      _PopupFlag = true;
      Control.DroppedDown = false;
    }

    private bool _PopupFlag;

    /// <summary>
    /// Вызывает по таймеру.
    /// Реализует вызов OnPopup() с задержкой
    /// </summary>
    public override void HandleIdle()
    {
      base.HandleIdle();
      Control.DroppedDown = false;
      if (_PopupFlag)
      {
        _PopupFlag = false;
        OnPopup(EventArgs.Empty);
      }
    }

    #endregion
  }

  /// <summary>
  /// Комбоблок с редактированием для ввода целых чисел. Также поддерживаются 
  /// специальные текстовые значения, которые могут быть выбраны из выпадающего
  /// списка или введены вручную. Каждому такому значению соответствует числовой код
  /// Текущим значением является целочисленное свойство Value.
  /// </summary>
  public class EFPIntEditComboBox : EFPControl<ComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPIntEditComboBox(EFPBaseProvider baseProvider, ComboBox control)
      : base(baseProvider, control, true)
    {
      control.DropDownStyle = ComboBoxStyle.DropDown; // редактирование возможно
      if (!DesignMode)
        control.TextChanged += new EventHandler(Control_TextChanged);
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Проверка корректности значения
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (ValidateState == UIValidateState.Error)
        return;
      int x;
      if (!TryTextToValue(Control.Text, out x))
        SetError("Неправильное значение");
    }

    #endregion

    #region Текстовые подстановки

    /// <summary>
    /// Коды подстановки.
    /// Свойство устанавливается вызовом AssignSubsts()
    /// </summary>
    public int[] SubstCodes { get { return _SubstCodes; } }
    private int[] _SubstCodes;

    /// <summary>
    /// Текстовые значения в выпадающем списке
    /// Свойство устанавливается вызовом AssignSubsts()
    /// </summary>
    public string[] SubstValues { get { return _SubstValues; } }
    private string[] _SubstValues;

    /// <summary>
    /// Присвоение кодов подстановки и соответствующих текстовых значений.
    /// Элементы SubstValues добавляются в комбоблок сразу или когда он будет 
    /// присоединен
    /// </summary>
    /// <param name="codes">Числовые коды подстановок</param>
    /// <param name="values">Текстовые представления для выпадающего списка</param>
    public void AssignSubsts(int[] codes, string[] values)
    {
      if (_SubstCodes != null)
        throw new InvalidOperationException("Повторный вызов AssignSubsts()");

      if (codes == null)
        throw new ArgumentNullException("codes");
      if (values == null)
        throw new ArgumentNullException("values");
      if (values.Length != codes.Length)
        throw new ArgumentException("Массивы имеют разную длину", "values");

      _SubstCodes = codes;
      _SubstValues = values;
      Control.Items.AddRange(values);
      Validate();
    }

    /// <summary>
    /// Преобразование числового значения в текстовое значение из SubstValues.
    /// Если в SubstCodes нет такого значения, возвращается <paramref name="value"/>.ToString().
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>Текст</returns>
    public string ValueToText(int value)
    {
      if (_SubstCodes != null)
      {
        int p = Array.IndexOf<int>(_SubstCodes, value);
        if (p >= 0)
          return _SubstValues[p];
      }

      return value.ToString();
    }

    /// <summary>
    /// Преобразование строки в числовое значение с учетом подстановок SubstValues.
    /// Пустая строка считается равной 0.
    /// Если <paramref name="text"/> содержит некорректное значение, то возникает исключение
    /// </summary>
    /// <param name="text">Преобразуемая текстовая строка</param>
    /// <returns>Числовое значение</returns>
    public int TextToValue(string text)
    {
      int value;
      if (!TryTextToValue(text, out value))
        throw new InvalidCastException("Нельзя преобразовать \"" + text + "\" в целое число");
      return value;
    }

    /// <summary>
    /// Преобразование строки в числовое значение с учетом подстановок SubstValues.
    /// Пустая строка считается равной 0.
    /// </summary>
    /// <param name="text">Преобразуемая текстовая строка</param>
    /// <param name="value">Числовое значение</param>
    /// <returns>true, если преобразование успешно выполнено</returns>
    public bool TryTextToValue(string text, out int value)
    {
      if (_SubstCodes != null)
      {
        int p = Array.IndexOf<string>(_SubstValues, text);
        if (p >= 0)
        {
          value = _SubstCodes[p];
          return true;
        }
      }
      if (String.IsNullOrEmpty(text))
      {
        value = 0;
        return true;
      }
      return int.TryParse(text, out value);
    }

    #endregion

    #region Свойство Value

    /// <summary>
    /// Основное свойство - числовое значение.
    /// Если в поле ввода введено некорректное значение, то возвращает 0
    /// </summary>
    public int Value
    {
      get
      {
        int res;
        if (TryTextToValue(Control.Text, out res))
          return res;
        else
          return 0; // 11.01.2018
      }
      set
      {
        // Не нужно, иначе может не обновляться
        //if (InsideValueChanged)
        //  return;
        Control.Text = ValueToText(value);
      }
    }

    private bool _InsideValueChanged = false;

    void Control_TextChanged(object sender, EventArgs args)
    {
      try
      {
        if (!_InsideValueChanged)
        {
          _InsideValueChanged = true;
          try
          {
            OnValueChanged();
          }
          finally
          {
            _InsideValueChanged = false;
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика ComboBox.TextChanged");
      }
    }

    /// <summary>
    /// Метод вызывается при изменении текста в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnValueChanged()
    {
      if (_ValueEx != null)
      {
        int x;
        if (TryTextToValue(Control.Text, out x))
          _ValueEx.OwnerSetValue(x);
      }
      Validate();
    }

    /// <summary>
    /// Текущий числовое значение. Управляемое свойство для Value
    /// </summary>
    public DepValue<int> ValueEx
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
    private DepInput<int> _ValueEx;

    private void InitValueEx()
    {
      if (_ValueEx == null)
      {
        int x;
        TryTextToValue(Control.Text, out x);
        _ValueEx = new DepInput<int>(x, ValueEx_ValueChanged);
        _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
      }
    }

    void ValueEx_ValueChanged(object Sender, EventArgs args)
    {
      Value = _ValueEx.Value;
    }

    #endregion
  }
}
