// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  #region ListControlImageEventHandler

  /// <summary>
  /// Аргументы события рисования элемента списка.
  /// Обработчик события не выполняет рисование самостоятельно, вместо этого он устанавливает
  /// свойства Text, ValidateState (определяет цвет текста) и ImageKey.
  /// </summary>
  public class ListControlImageEventArgs : EventArgs
  {
    #region Конструктор

    internal ListControlImageEventArgs(ListControl control)
    {
      _Control = control;
    }

    #endregion

    #region Свойства

    private ListControl _Control;

    /// <summary>
    /// Возвращает индекс позиции, которую требуется нарисовать. 
    /// Возможно значение (-1), когда требуется нарисовать текст комбоблока в свернутом состоянии,
    /// когда нет выбранного элемента
    /// </summary>
    public int ItemIndex { get { return _ItemIndex; } }
    internal int _ItemIndex;

    /// <summary>
    /// Доступ к текущей позиции как к объекту (обычно строке)
    /// Может быть null
    /// </summary>
    public object Item
    {
      get
      {
        System.Collections.IList items;
        if (_Control is ListBox)
          items = ((ListBox)_Control).Items;
        else
          items = ((ComboBox)_Control).Items;
        if (_ItemIndex < 0 || _ItemIndex >= items.Count)
          return null;
        else
          return items[_ItemIndex];
      }
    }

    /// <summary>
    /// Возвращает признаки прорисовываемой темы.
    /// Можно, например, определить, прорисовывается ли редактируемая часть
    /// комбоблока, или строка в списке.
    /// Обычно не используется в обработчике.
    /// </summary>
    public DrawItemState State { get { return _State; } }
    internal DrawItemState _State;

    /// <summary>
    /// Выводимый текст. Может быть переопределен.
    /// По умолчанию содержит текстововое представление рисуемой темы.
    /// </summary>
    public string Text { get { return _Text; } set { _Text = value; } }
    private string _Text;

    /// <summary>
    /// Сюда должно быть помещено имя изображения из EFPApp.MainImages
    /// </summary>
    public string ImageKey { get { return _ImageKey; } set { _ImageKey = value; } }
    private string _ImageKey;

    /// <summary>
    /// Здесь можно задать произвольные цвета для элемента.
    /// Установка свойств Colors и ValidateState является взаимоисключающей. Если одно свойство устанавливается, то второе сбрасывается в null.
    /// Если оба свойства равны null, используются цвета Control.BackColor и ForeColor.
    /// </summary>
    public ListItemColors Colors
    {
      get { return _Colors; }
      set
      {
        _Colors = value;
        _ValidateState = null;
      }
    }
    private ListItemColors _Colors;

    /// <summary>
    /// Здесь можно задать цвет текста (обычный черный, красный (при ошибке) или сиреневый (предупреждение).
    /// Установка свойств Colors и ValidateState является взаимоисключающей. Если одно свойство устанавливается, то второе сбрасывается в null.
    /// Если оба свойства равны null, используются цвета Control.BackColor и ForeColor.
    /// </summary>
    public UIValidateState? ValidateState
    {
      get { return _ValidateState; }
      set
      {
        _ValidateState = value;
        _Colors = null;
      }
    }
    private UIValidateState? _ValidateState;

    /// <summary>
    /// Здесь можно задать сдвиг изображения и картинки вправо на указанное число пикселей
    /// </summary>
    public int LeftMargin { get { return _LeftMargin; } set { _LeftMargin = value; } }
    private int _LeftMargin;

    #endregion
  }

  /// <summary>
  /// Пользовательский обработчик события рисования элемента списка
  /// </summary>
  /// <param name="sender">Не используется</param>
  /// <param name="args">Аргументы события</param>
  public delegate void ListControlImageEventHandler(object sender, ListControlImageEventArgs args);

  #endregion

  /// <summary>
  /// Цвета для рисования элемента списка.
  /// Позволяет задавать цвет фона и цвет текста для обычого элемента и для элемента в состоянии Selected.
  /// Если какой-либо цвет не задан (Color.Empty), при рисовании используется соответствующий системный цвет.
  /// Класс однократной записи.
  /// </summary>
  public sealed class ListItemColors
  {
    #region Конструктор

    /// <summary>
    /// Создает набор из 4 цветов
    /// </summary>
    /// <param name="backColor">Цвет фона невыделенного элемента</param>
    /// <param name="foreColor">Цвет текста невыделенного элемента</param>
    /// <param name="selectedBackColor">Цвет фона выделенного элемента</param>
    /// <param name="selectedForeColor">Цвет текста выделенного элемента</param>
    public ListItemColors(Color backColor, Color foreColor, Color selectedBackColor, Color selectedForeColor)
    {
      _BackColor = backColor;
      _ForeColor = foreColor;
      _SelectedBackColor = selectedBackColor;
      _SelectedForeColor = selectedForeColor;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Цвет фона невыделенного элемента. Например, SystemColors.Window
    /// </summary>
    public Color BackColor { get { return _BackColor; } }
    private Color _BackColor;

    /// <summary>
    /// Цвет текста невыделенного элемента. Например, SystemColors.WindowText
    /// </summary>
    public Color ForeColor { get { return _ForeColor; } }
    private Color _ForeColor;

    /// <summary>
    /// Цвет фона выделенного элемента. Например, SystemColors.Highlight
    /// </summary>
    public Color SelectedBackColor { get { return _SelectedBackColor; } }
    private Color _SelectedBackColor;

    /// <summary>
    /// Цвет текста выделенного элемента. Например, SystemColors.HighliteText
    /// </summary>
    public Color SelectedForeColor { get { return _SelectedForeColor; } }
    private Color _SelectedForeColor;

    #endregion

    #region Статические экземполяры

    /// <summary>
    /// Использовать цвета, определенные для элемента.
    /// Все цвета заданы как Color.Empty
    /// </summary>
    public static readonly ListItemColors ControlDefault = new ListItemColors(Color.Empty, Color.Empty, Color.Empty, Color.Empty);


    /// <summary>
    /// Использовать цвета, определенные для элемента.
    /// Цвета заданы из SystemColors.Window, WindowText, Highlight, HighlightText
    /// </summary>
    public static readonly ListItemColors SystemDefault = new ListItemColors(SystemColors.Window, SystemColors.WindowText, SystemColors.Highlight, SystemColors.HighlightText);

    /// <summary>
    /// Возвращает цвета для заданного состояния проверки
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static ListItemColors FromValidateState(UIValidateState state)
    {
      switch (state)
      {
        case UIValidateState.Ok: return EFPApp.Colors.ListStateOk;
        case UIValidateState.Error: return EFPApp.Colors.ListStateError;
        case UIValidateState.Warning: return EFPApp.Colors.ListStateWarning;
        default:
          throw new ArgumentException("Неизвестное значение " + state.ToString(), "state");
      }
    }

    #endregion
  }

  /// <summary>
  /// Рисование значков из EFPApp.MainImages в комбоблоке (ComboBox) или списке (ListBox)
  /// </summary>
  public class ListControlImagePainter
  {
    #region Конструкторы

    /// <summary>
    /// Создать рисователь изображений в комбоблоке.
    /// Эта версия позволяет задать массив изображений для элементов списка и пользовательский обработчик
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="itemImageKeys">Массив имен изображений в EFPApp.MainImages.</param>
    /// <param name="handler">Пользовательский обработчик</param>
    public ListControlImagePainter(ComboBox control, string[] itemImageKeys, ListControlImageEventHandler handler)
    {
#if DEBUG
      if (control == null)
        throw new ArgumentNullException("control");
      if (itemImageKeys == null && handler == null)
        throw new ArgumentNullException("handler", "Список изображений или обработчик должны быть заданы");
#endif
      _Control = control;
      _ItemImageKeys = itemImageKeys;
      NoSelImageKey = String.Empty;
      OutOfRangeImageKey = "Error";
      _Handler = handler;
      _PainterArgs = new ListControlImageEventArgs(control);

      control.DrawMode = DrawMode.OwnerDrawFixed;
      control.DrawItem += new DrawItemEventHandler(Control_DrawItem);
    }

    /// <summary>
    /// Создать рисователь изображений в списке.
    /// Эта версия позволяет задать массив изображений для элементов списка и пользовательский обработчик
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="itemImageKeys">Массив имен изображений в EFPApp.MainImages.</param>
    /// <param name="handler">Пользовательский обработчик</param>
    public ListControlImagePainter(ListBox control, string[] itemImageKeys, ListControlImageEventHandler handler)
    {
#if DEBUG
      if (control == null)
        throw new ArgumentNullException("control");
      if (itemImageKeys == null && handler == null)
        throw new ArgumentNullException("handler", "Список изображений или обработчик должны быть заданы");
#endif
      _Control = control;
      _ItemImageKeys = itemImageKeys;
      NoSelImageKey = String.Empty;
      OutOfRangeImageKey = "Error";
      _Handler = handler;
      _PainterArgs = new ListControlImageEventArgs(control);

      control.DrawMode = DrawMode.OwnerDrawFixed;
      control.DrawItem += new DrawItemEventHandler(Control_DrawItem);
    }

    /// <summary>
    /// Создать рисователь изображений в комбоблоке.
    /// Эта версия предполагает использование пользовательского обработчика
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="handler">Пользовательский обработчик</param>
    public ListControlImagePainter(ComboBox control, ListControlImageEventHandler handler)
      : this(control, null, handler)
    {
    }

    /// <summary>
    /// Создать рисователь изображений в списке.
    /// Эта версия предполагает использование пользовательского обработчика
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="handler">Пользовательский обработчик</param>
    public ListControlImagePainter(ListBox control, ListControlImageEventHandler handler)
      : this(control, null, handler)
    {
    }

    /// <summary>
    /// Создать рисователь изображений в комбоблоке.
    /// Эта версия не позволяет задать пользовательский обработчик
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="itemImageKeys">Массив имен изображений в EFPApp.MainImages.</param>
    public ListControlImagePainter(ComboBox control, string[] itemImageKeys)
      : this(control, itemImageKeys, null)
    {
    }

    /// <summary>
    /// Создать рисователь изображений в списке.
    /// Эта версия не позволяет задать пользовательский обработчик
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="itemImageKeys">Массив имен изображений в EFPApp.MainImages.</param>
    public ListControlImagePainter(ListBox control, string[] itemImageKeys)
      : this(control, itemImageKeys, null)
    {
    }

    /// <summary>
    /// Создать рисователь одинаковых изображений в комбоблоке.
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="imageKey">Имя изображения в EFPApp.MainImages</param>
    public ListControlImagePainter(ComboBox control, string imageKey)
      : this(control, DataTools.EmptyStrings, null)
    {
#if DEBUG
      if (String.IsNullOrEmpty(imageKey))
        throw new ArgumentNullException("imageKey");
#endif
      this.OutOfRangeImageKey = imageKey;
    }

    /// <summary>
    /// Создать рисователь одинаковых изображений в списке.
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="imageKey">Имя изображения в EFPApp.MainImages</param>
    public ListControlImagePainter(ListBox control, string imageKey)
      : this(control, DataTools.EmptyStrings, null)
    {
#if DEBUG
      if (String.IsNullOrEmpty(imageKey))
        throw new ArgumentNullException("imageKey");
#endif
      this.OutOfRangeImageKey = imageKey;
    }

    #endregion

    #region Поля и свойства

    /// <summary>
    /// Управляющий элемент (задается в конструкторе)
    /// </summary>
    public ListControl Control { get { return _Control; } }
    private ListControl _Control;

    private ListControlImageEventHandler _Handler;

    /// <summary>
    /// Чтобы каждый раз не создавать
    /// </summary>
    private ListControlImageEventArgs _PainterArgs;

    /// <summary>
    /// Список имен изображений, соответствующих ItemIndex
    /// (задается в конструкторе).
    /// Если в списке больше элементов, чем в этом массиве, используется изображенин с именем OutOfRangeImageKey.
    /// </summary>
    public string[] ItemImageKeys { get { return _ItemImageKeys; } }
    private string[] _ItemImageKeys;

    /// <summary>
    /// Имя изображения, соответствующее значению ItemIndex=-1
    /// По умолчанию - пустая строка
    /// </summary>
    public string NoSelImageKey { get { return _NoSelImageKey; } set { _NoSelImageKey = value; } }
    private string _NoSelImageKey;

    /// <summary>
    /// Имя изображения, если ItemIndex превышает ItemImageKeys.Length
    /// По умолчанию - Error
    /// </summary>
    public string OutOfRangeImageKey { get { return _OutOfRangeImageKey; } set { _OutOfRangeImageKey = value; } }
    private string _OutOfRangeImageKey;

    #endregion

    #region Обработчик

    void Control_DrawItem(object sender, DrawItemEventArgs args)
    {
      // Запрашиваем имя изображения
      _PainterArgs._ItemIndex = args.Index;
      _PainterArgs._State = args.State;
      _PainterArgs.ValidateState = null;
      _PainterArgs.LeftMargin = 0;
      if (args.Index < 0)
        _PainterArgs.ImageKey = NoSelImageKey;
      else
      {
        if (ItemImageKeys == null)
          _PainterArgs.ImageKey = String.Empty;
        else
        {
          if (args.Index < ItemImageKeys.Length)
            _PainterArgs.ImageKey = ItemImageKeys[args.Index];
          else
            _PainterArgs.ImageKey = OutOfRangeImageKey;
        }
      }

      // Извлекаем текст
      if (args.Index < 0)
        _PainterArgs.Text = _Control.Text;
      else
      {
        if (_Control is ComboBox)
          _PainterArgs.Text = _Control.GetItemText(((ComboBox)_Control).Items[args.Index]);
        else
          _PainterArgs.Text = _Control.GetItemText(((ListBox)_Control).Items[args.Index]);
      }

      if (_Handler != null)
        _Handler(this, _PainterArgs);

      // Рисуем
      if (_PainterArgs.Colors == null)
        PerformDrawItem(_Control, args, _PainterArgs.Text, _PainterArgs.ImageKey, _PainterArgs.ValidateState, _PainterArgs.LeftMargin);
      else
        PerformDrawItem(_Control, args, _PainterArgs.Text, _PainterArgs.ImageKey, _PainterArgs.Colors, _PainterArgs.LeftMargin);
    }

    #endregion

    #region Статический метод рисования

    /// <summary>
    /// Статический конструктор
    /// </summary>
    static ListControlImagePainter()
    {
      _TheStringFormat = new StringFormat(StringFormatFlags.NoWrap);
      _TheStringFormat.Alignment = StringAlignment.Near;
      _TheStringFormat.LineAlignment = StringAlignment.Center;
    }

    private static StringFormat _TheStringFormat;

    /// <summary>
    /// Статический метод рисования элемента списка.
    /// Предназначен для реализации обработки события ListBox/ComboBox.DrawItem.
    /// </summary>
    /// <param name="control">Управляющий элемент, для которого выполняется рисования</param>
    /// <param name="args">Аргументы события DrawItem. Методы объекта используются для рисования в контексте устройства.</param>
    /// <param name="text">Текст элемента</param>
    /// <param name="imageKey">Значок элемента в списке EFPApp.MainImages.
    /// Пустая строка или null означает отсутствие значка, при этом место используется для рисования текста</param>
    public static void PerformDrawItem(Control control, DrawItemEventArgs args, string text, string imageKey)
    {
      PerformDrawItem(control, args, text, imageKey, ListItemColors.ControlDefault, 0);
    }

    /// <summary>
    /// Статический метод рисования элемента списка.
    /// Предназначен для реализации обработки события ListBox/ComboBox.DrawItem.
    /// </summary>
    /// <param name="control">Управляющий элемент, для которого выполняется рисования</param>
    /// <param name="args">Аргументы события DrawItem. Методы объекта используются для рисования в контексте устройства.</param>
    /// <param name="text">Текст элемента</param>
    /// <param name="imageKey">Значок элемента в списке EFPApp.MainImages.
    /// Пустая строка или null означает отсутствие значка, при этом место используется для рисования текста</param>
    /// <param name="validateState">Определяет цвет текста элемента 
    /// (обычный черный, красный (ошибка) или сиреневый (предупреждение)).
    /// Если передано значение null, то цвет элемента не задается в явном виде. 
    /// Используется основной цвет текста элемента Control.ForeColor</param>
    public static void PerformDrawItem(Control control, DrawItemEventArgs args, string text, string imageKey, UIValidateState? validateState)
    {
      PerformDrawItem(control, args, text, imageKey, validateState, 0);
    }

    /// <summary>
    /// Статический метод рисования элемента списка.
    /// Предназначен для реализации обработки события ListBox/ComboBox.DrawItem.
    /// </summary>
    /// <param name="control">Управляющий элемент, для которого выполняется рисования</param>
    /// <param name="args">Аргументы события DrawItem. Методы объекта используются для рисования в контексте устройства.</param>
    /// <param name="text">Текст элемента</param>
    /// <param name="imageKey">Значок элемента в списке EFPApp.MainImages.
    /// Пустая строка или null означает отсутствие значка, при этом место используется для рисования текста</param>
    /// <param name="validateState">Определяет цвет текста элемента 
    /// (обычный черный, красный (ошибка) или сиреневый (предупреждение)).
    /// Если передано значение null, то цвет элемента не задается в явном виде. 
    /// Используется основной цвет текста элемента Control.ForeColor</param>
    /// <param name="leftMargin">Отступ от левого края области, где рисуется элемент списка,
    /// до значка. Используется для рисования иерархического списка с отступами.</param>
    public static void PerformDrawItem(Control control, DrawItemEventArgs args, string text, string imageKey, UIValidateState? validateState, int leftMargin)
    {
      Image image;
      if (String.IsNullOrEmpty(imageKey))
        image = null;
      else
      {
        image = EFPApp.MainImages.Images[imageKey];
        if (image == null)
          image = EFPApp.MainImages.Images["Error"];
      }
      PerformDrawItem(control, args, text, image, validateState, leftMargin);
    }


    /// <summary>
    /// Статический метод рисования элемента списка.
    /// Предназначен для реализации обработки события ListBox/ComboBox.DrawItem.
    /// </summary>
    /// <param name="control">Управляющий элемент, для которого выполняется рисования</param>
    /// <param name="args">Аргументы события DrawItem. Методы объекта используются для рисования в контексте устройства.</param>
    /// <param name="text">Текст элемента</param>
    /// <param name="ImageKey">Значок элемента в списке EFPApp.MainImages.
    /// Пустая строка или null означает отсутствие значка, при этом место используется для рисования текста</param>
    /// <param name="Colors">Цвета для элемента. Если null, то используются стандартные цвета</param>
    public static void PerformDrawItem(Control control, DrawItemEventArgs args, string text, string ImageKey, ListItemColors Colors)
    {
      PerformDrawItem(control, args, text, ImageKey, Colors, 0);
    }

    /// <summary>
    /// Статический метод рисования элемента списка.
    /// Предназначен для реализации обработки события ListBox/ComboBox.DrawItem.
    /// </summary>
    /// <param name="control">Управляющий элемент, для которого выполняется рисования</param>
    /// <param name="args">Аргументы события DrawItem. Методы объекта используются для рисования в контексте устройства.</param>
    /// <param name="text">Текст элемента</param>
    /// <param name="imageKey">Значок элемента в списке EFPApp.MainImages.
    /// Пустая строка или null означает отсутствие значка, при этом место используется для рисования текста</param>
    /// <param name="colors">Цвета для элемента. Если null, то используются стандартные цвета</param>
    /// <param name="leftMargin">Отступ от левого края области, где рисуется элемент списка,
    /// до значка. Используется для рисования иерархического списка с отступами.</param>
    public static void PerformDrawItem(Control control, DrawItemEventArgs args, string text, string imageKey, ListItemColors colors, int leftMargin)
    {
      Image image;
      if (String.IsNullOrEmpty(imageKey))
        image = null;
      else
      {
        image = EFPApp.MainImages.Images[imageKey];
        if (image == null)
          image = EFPApp.MainImages.Images["Error"];
      }
      PerformDrawItem(control, args, text, image, colors, leftMargin);
    }


    /// <summary>
    /// Статический метод рисования элемента списка.
    /// Предназначен для реализации обработки события ListBox/ComboBox.DrawItem.
    /// </summary>
    /// <param name="control">Управляющий элемент, для которого выполняется рисования</param>
    /// <param name="args">Аргументы события DrawItem. Методы объекта используются для рисования в контексте устройства.</param>
    /// <param name="text">Текст элемента</param>
    /// <param name="image">Значок элемента.
    /// Значение null означает отсутствие значка, при этом место используется для рисования текста</param>
    public static void PerformDrawItem(Control control, DrawItemEventArgs args, string text, Image image)
    {
      PerformDrawItem(control, args, text, image, ListItemColors.ControlDefault, 0);
    }

    /// <summary>
    /// Статический метод рисования элемента списка.
    /// Предназначен для реализации обработки события ListBox/ComboBox.DrawItem.
    /// </summary>
    /// <param name="control">Управляющий элемент, для которого выполняется рисования</param>
    /// <param name="args">Аргументы события DrawItem. Методы объекта используются для рисования в контексте устройства.</param>
    /// <param name="text">Текст элемента</param>
    /// <param name="image">Значок элемента.
    /// Значение null означает отсутствие значка, при этом место используется для рисования текста</param>
    /// <param name="validateState">Определяет цвет текста элемента 
    /// (обычный черный, красный (ошибка) или сиреневый (предупреждение)).
    /// Если передано значение null, то цвет элемента не задается в явном виде. 
    /// Используется основной цвет текста элемента Control.ForeColor</param>
    public static void PerformDrawItem(Control control, DrawItemEventArgs args, string text, Image image, UIValidateState? validateState)
    {
      PerformDrawItem(control, args, text, image, validateState, 0);
    }

    /// <summary>
    /// Статический метод рисования элемента списка.
    /// Предназначен для реализации обработки события ListBox/ComboBox.DrawItem.
    /// </summary>
    /// <param name="control">Управляющий элемент, для которого выполняется рисования</param>
    /// <param name="args">Аргументы события DrawItem. Методы объекта используются для рисования в контексте устройства.</param>
    /// <param name="text">Текст элемента</param>
    /// <param name="image">Значок элемента.
    /// Значение null означает отсутствие значка, при этом место используется для рисования текста</param>
    /// <param name="validateState">Определяет цвет текста элемента 
    /// (обычный черный, красный (ошибка) или сиреневый (предупреждение)).
    /// Если передано значение null, то цвет элемента не задается в явном виде. 
    /// Используется основной цвет текста элемента Control.ForeColor</param>
    /// <param name="leftMargin">Отступ от левого края области, где рисуется элемент списка,
    /// до значка. Используется для рисования иерархического списка с отступами.</param>
    public static void PerformDrawItem(Control control, DrawItemEventArgs args, string text, Image image, UIValidateState? validateState, int leftMargin)
    {
      ListItemColors colors;
      if (validateState.HasValue)
        colors = ListItemColors.FromValidateState(validateState.Value);
      else
        colors = ListItemColors.ControlDefault;

      PerformDrawItem(control, args, text, image, colors, leftMargin);
    }

    /// <summary>
    /// Статический метод рисования элемента списка.
    /// Предназначен для реализации обработки события ListBox/ComboBox.DrawItem.
    /// </summary>
    /// <param name="control">Управляющий элемент, для которого выполняется рисования</param>
    /// <param name="args">Аргументы события DrawItem. Методы объекта используются для рисования в контексте устройства.</param>
    /// <param name="text">Текст элемента</param>
    /// <param name="image">Значок элемента.
    /// Значение null означает отсутствие значка, при этом место используется для рисования текста</param>
    /// <param name="colors">Цвета для элемента. Если null, то используются стандартные цвета</param>
    public static void PerformDrawItem(Control control, DrawItemEventArgs args, string text, Image image, ListItemColors colors)
    {
      PerformDrawItem(control, args, text, image, colors, 0);
    }

    /// <summary>
    /// Статический метод рисования элемента списка.
    /// Предназначен для реализации обработки события ListBox/ComboBox.DrawItem.
    /// </summary>
    /// <param name="control">Управляющий элемент, для которого выполняется рисования</param>
    /// <param name="args">Аргументы события DrawItem. Методы объекта используются для рисования в контексте устройства.</param>
    /// <param name="text">Текст элемента</param>
    /// <param name="image">Значок элемента.
    /// Значение null означает отсутствие значка, при этом место используется для рисования текста</param>
    /// <param name="colors">Цвета для элемента. Если null, то используются стандартные цвета</param>
    /// <param name="leftMargin">Отступ от левого края области, где рисуется элемент списка,
    /// до значка. Используется для рисования иерархического списка с отступами.</param>
    public static void PerformDrawItem(Control control, DrawItemEventArgs args, string text, Image image, ListItemColors colors, int leftMargin)
    {
      #region Определяем цвета для рисования

      if (colors == null)
        colors = ListItemColors.ControlDefault;

      // инвертированный цвет?
      bool isSelected = (args.State & DrawItemState.Selected) != 0;

      Color backColor, foreColor;
      if (control.Enabled)
      {

        backColor = isSelected ? colors.SelectedBackColor : colors.BackColor;
        if (backColor.IsEmpty)
          backColor = args.BackColor;
      }
      else
        backColor = SystemColors.Control;

      foreColor = isSelected ? colors.SelectedForeColor : colors.ForeColor;
      if (foreColor.IsEmpty)
        foreColor = args.ForeColor;

      #endregion

      args.Graphics.FillRectangle(new SolidBrush(backColor), args.Bounds);

      // Координаты рисования строки
      Rectangle r1 = args.Bounds; // место значка
      //r1.X += 2;
      //r1.Width -= 4;
      r1.X += leftMargin;
      r1.Width -= leftMargin;

      Rectangle r2 = r1; // место для текста
      r1.Width = 16;

      if (image != null)
      {
        r2.Width -= 18;
        r2.X += 18;
        if (r1.Height > 16)
        {
          r1.Y += (r1.Height - 16) / 2;
          r1.Height = 16;
        }

        if (!control.Enabled)
          image = ToolStripRenderer.CreateDisabledImage(image); // 13.06.2019
        args.Graphics.DrawImage(image, r1);
      }

      if (!String.IsNullOrEmpty(text))
        args.Graphics.DrawString(text, args.Font, new SolidBrush(foreColor), r2, _TheStringFormat);

      args.DrawFocusRectangle();
    }

    #endregion
  }
}
