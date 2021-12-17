// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using FreeLibSet.IO;
using FreeLibSet.Formatting;
using System.Diagnostics;
using FreeLibSet.DependedValues;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using FreeLibSet.Core;
using FreeLibSet.Data;
using FreeLibSet.Shell;
using FreeLibSet.Logging;

namespace FreeLibSet.Forms
{
  #region Перечисления

  /// <summary>
  /// Значения свойства EFPDataGridView.State
  /// </summary>
  public enum EFPDataGridViewState
  {
    /// <summary>
    /// Режим просмотра.
    /// В этом режиме табличный просмотр находится постоянно, пока не выбрана одна из команд редактирования.
    /// Также используется, если выбрана команда "Просмотреть запись".
    /// </summary>
    View,

    /// <summary>
    /// Выбрана команда "Редактировать запись".
    /// </summary>
    Edit,

    /// <summary>
    /// Выбрана команда "Новая запись"
    /// </summary>
    Insert,

    /// <summary>
    /// Выбранная команда "Копия записи"
    /// </summary>
    InsertCopy,

    /// <summary>
    /// Выбрана команда "Удалить запись"
    /// </summary>
    Delete,
  };

  /// <summary>
  /// Сколько строк сейчас выбрано в просмотре: Одна, ни одной или несколько
  /// (свойство EFPDataGridView.SelectedRowState)
  /// </summary>
  public enum EFPDataGridViewSelectedRowsState
  {
    /// <summary>
    /// Выбрана одна ячейка или одна строка целиком или несколько ячеек в одной строке
    /// </summary>
    SingleRow,

    /// <summary>
    /// Выбрано несколько строк или ячейки, расположенные на разных строках
    /// </summary>
    MultiRows,

    /// <summary>
    /// Нет ни одной выбранной ячейки (просмотр пуст)
    /// </summary>
    NoSelection
  }

  #endregion

  #region Цветовые атрибуты ячейки

  #region Перечисления

  #region EFPDataGridViewColorType

  /// <summary>
  /// Аргумент для метода SetCellAttr()
  /// </summary>
  public enum EFPDataGridViewColorType
  {
    /// <summary>
    /// Обычная ячейка
    /// </summary>
    Normal,

    /// <summary>
    /// Альтернативный цвет для создания "полосатых" просмотров, когда строки
    /// (обычно, через одну) имеют немного отличающийся цвет
    /// </summary>
    Alter,

    /// <summary>
    /// Выделение серым цветом
    /// </summary>
    Special,

    /// <summary>
    /// Итог 1 (зеленые строки)
    /// </summary>
    Total1,

    /// <summary>
    /// Подытог 2 (сиреневые строки)
    /// </summary>
    Total2,

    /// <summary>
    /// Итоговая строка по всей таблице
    /// </summary>
    TotalRow,

    /// <summary>
    /// Заголовок
    /// </summary>
    Header,

    /// <summary>
    /// Ячейка с ошибкой (красный фон).
    /// </summary>
    Error,

    /// <summary>
    /// Ячейка с предупреждением (ярко-желтый фон, красные буквы)
    /// </summary>
    Warning,
  }

  #endregion

  #region EFPDataGridViewBorderStyle

  /// <summary>
  /// Типы границы ячейки
  /// </summary>
  public enum EFPDataGridViewBorderStyle
  {
    /// <summary>
    /// Граница по умолчанию (в зависимости от оформления границ таблицы)
    /// </summary>
    Default,

    /// <summary>
    /// Отключить границу
    /// (требуется, чтобы граница была отключена и у соседней ячейки)
    /// </summary>
    None,

    /// <summary>
    /// Тонкая линия
    /// </summary>
    Thin,

    /// <summary>
    /// Толстая линия
    /// </summary>
    Thick
  }

  #endregion

  #region EFPDataGridViewAttributesReason

  /// <summary>
  /// Для чего вызывается событие EFPDataGridView.GetRowAttributes или GetCellAttributes
  /// </summary>
  public enum EFPDataGridViewAttributesReason
  {
    /// <summary>
    /// Для вывода на экран
    /// </summary>
    View,
    /// <summary>
    /// Для вывода на печать или для предварительного просмотра
    /// </summary>
    Print,

    /// <summary>
    /// Обрабатывается сообщение CellToolTipNeeded для заголовка строки
    /// </summary>
    ToolTip,

    /// <summary>
    /// Определяется возможность редактирования ячейки. Обработчик события 
    /// CellBeginEdit или вызван метод GetCellReadOnly
    /// </summary>
    ReadOnly,
  }

  #endregion

  #region EFPDataGridViewImageKind

  /// <summary>
  /// Изображения, которые можно выводить в заголовке строк
  /// С помощью свойства UserImage можно задавать произвольные изображения
  /// </summary>
  public enum EFPDataGridViewImageKind
  {
    /// <summary>
    /// Нет изображения
    /// </summary>
    None,

    /// <summary>
    /// Значок "i"
    /// </summary>
    Information,

    /// <summary>
    /// Восклицательный знак
    /// </summary>
    Warning,

    /// <summary>
    /// Ошибка (красный крест)
    /// </summary>
    Error,
  }

  #endregion

  #endregion

  /// <summary>
  /// Базовый класс для EFPDataGridViewRowAttributesEventArgs и 
  /// EFPDataGridViewCellAttributesEventArgs
  /// </summary>
  public class EFPDataGridViewCellAttributesEventArgsBase : EventArgs
  {
    #region Конструктор

    internal EFPDataGridViewCellAttributesEventArgsBase(EFPDataGridView controlProvider, bool isCellAttributes)
    {
      _ControlProvider = controlProvider;
      _IsCellAttributes = isCellAttributes;
    }

    #endregion

    #region Свойства - исходные данные

    /// <summary>
    /// Обработчик табличного просмотра
    /// </summary>
    public EFPDataGridView ControlProvider { get { return _ControlProvider; } }
    private EFPDataGridView _ControlProvider;

    /// <summary>
    /// Табличный просмотр
    /// </summary>
    public DataGridView Control { get { return ControlProvider.Control; } }

    /// <summary>
    /// Зачем было вызвано событие: для вывода на экран или для печати
    /// </summary>
    public EFPDataGridViewAttributesReason Reason { get { return _Reason; } }
    private EFPDataGridViewAttributesReason _Reason;

    /// <summary>
    /// Номер строки, для которой требуются атрибуты, в табличном просмотре
    /// </summary>
    public int RowIndex { get { return _RowIndex; } }
    private int _RowIndex;

    /// <summary>
    /// Доступ к строке таблицы данных, если источником данных для табличного
    /// просмотра является DataView. Иначе - null
    /// </summary>
    public DataRowView RowView
    {
      get
      {
        DataView dv = ControlProvider.SourceAsDataView;
        if (dv == null)
          return null;
        else
          return dv[RowIndex];
      }
    }

    /// <summary>
    /// Доступ к строке таблицы данных, если источником данных для табличного
    /// просмотра является DataView. Иначе - null
    /// </summary>
    public DataRow DataRow
    {
      get
      {
        return ControlProvider.GetDataRow(RowIndex);
      }
    }

    /// <summary>
    /// Доступ к строке табличного просмотра. 
    /// Вызов метода делает строку Unshared
    /// </summary>
    /// <returns></returns>
    public DataGridViewRow GetGridRow()
    {
      if (RowIndex < 0)
        return null;
      return Control.Rows[RowIndex];
    }

    private bool _IsCellAttributes;

    #endregion

    #region Устанавливаемые свойства

    /// <summary>
    /// Цветовое оформление строки или ячейки.
    /// Установка значения в EFPDataGridViewColorType.TotalRow для объекта EFPDataGridViewRowAttributesEventArgs приводит дополнительно
    /// к установке свойств TopBorder и BottomBorder в значение Thick, что удобно для оформления таблиц отчетов.
    /// Чтобы посторониие свойства не устанавливались, используйте метод SetColorTypeOnly()
    /// </summary>
    public EFPDataGridViewColorType ColorType
    {
      get { return _ColorType; }
      set
      {
        _ColorType = value;
        if (value == EFPDataGridViewColorType.TotalRow)
        {
          if (!_IsCellAttributes) // 17.07.2019
          {
            //LeftBorder = EFPDataGridViewBorderStyle.Thick;
            TopBorder = EFPDataGridViewBorderStyle.Thick;
            //RightBorder = EFPDataGridViewBorderStyle.Thick;
            BottomBorder = EFPDataGridViewBorderStyle.Thick;
          }
        }
      }
    }
    private EFPDataGridViewColorType _ColorType;

    /// <summary>
    /// Установка свойства ColorType без неявной установки других свойств
    /// </summary>
    /// <param name="value">Значение свойства</param>
    public void SetColorTypeOnly(EFPDataGridViewColorType value)
    {
      _ColorType = value;
    }

    /// <summary>
    /// Если установить в true, то текст ячейки будет рисоваться серым шрифтом
    /// По умолчанию - false
    /// </summary>
    public bool Grayed
    {
      get
      {
        if (_Grayed.HasValue)
          return _Grayed.Value;
        else
          return false;
      }
      set
      {
        _Grayed = value;
      }
    }
    internal bool? _Grayed;

    /// <summary>
    /// Левая граница ячейки, используемая при печати
    /// </summary>
    public EFPDataGridViewBorderStyle LeftBorder { get { return _LeftBorder; } set { _LeftBorder = value; } }
    private EFPDataGridViewBorderStyle _LeftBorder;

    /// <summary>
    /// Верняя граница ячейки, используемая при печати
    /// </summary>
    public EFPDataGridViewBorderStyle TopBorder { get { return _TopBorder; } set { _TopBorder = value; } }
    private EFPDataGridViewBorderStyle _TopBorder;

    /// <summary>
    /// Правая граница ячейки, используемая при печати
    /// </summary>
    public EFPDataGridViewBorderStyle RightBorder { get { return _RightBorder; } set { _RightBorder = value; } }
    private EFPDataGridViewBorderStyle _RightBorder;

    /// <summary>
    /// Нижняя граница ячейки, используемая при печати
    /// </summary>
    public EFPDataGridViewBorderStyle BottomBorder { get { return _BottomBorder; } set { _BottomBorder = value; } }
    private EFPDataGridViewBorderStyle _BottomBorder;

    /// <summary>
    /// Перечеркивание ячейки от левого нижнего угла к правому верхнему.
    /// В отличие от других границ, работает в режиме просмотра.
    /// Значение Default идентично None.
    /// </summary>
    public EFPDataGridViewBorderStyle DiagonalUpBorder { get { return _DiagonalUpBorder; } set { _DiagonalUpBorder = value; } }
    private EFPDataGridViewBorderStyle _DiagonalUpBorder;

    /// <summary>
    /// Перечеркивание ячейки от левого верхнего угла к правому нижнему.
    /// В отличие от других границ, работает в режиме просмотра.
    /// Значение Default идентично None.
    /// </summary>
    public EFPDataGridViewBorderStyle DiagonalDownBorder { get { return _DiagonalDownBorder; } set { _DiagonalDownBorder = value; } }
    private EFPDataGridViewBorderStyle _DiagonalDownBorder;

    /// <summary>
    /// Используется в режиме Reason=ReadOnly
    /// </summary>
    public bool ReadOnly { get { return _ReadOnly; } set { _ReadOnly = value; } }
    private bool _ReadOnly;

    /// <summary>
    /// Используется в режиме Reason=ReadOnly
    /// Текст сообщения, почему нельзя редактировать строку / ячейку
    /// </summary>
    public string ReadOnlyMessage { get { return _ReadOnlyMessage; } set { _ReadOnlyMessage = value; } }
    private string _ReadOnlyMessage;

    #endregion

    #region Методы

    /// <summary>
    /// Устанавливает четыре границы (свойства LeftBorder, TopBorder, RightBorder, BottomBorder), используемые для печати.
    /// Свойства DiagonalUpBorder и DiagonalDownBorder не меняются
    /// </summary>
    /// <param name="style">Вид границы</param>
    public void SetAllBorders(EFPDataGridViewBorderStyle style)
    {
      SetAllBorders(style, false);
    }

    /// <summary>
    /// Устанавливает границы (свойства LeftBorder, TopBorder, RightBorder, BottomBorder), используемые для печати.
    /// Также может устанавливать свойства DiagonalUpBorder и DiagonalDownBorder.
    /// </summary>
    /// <param name="style">Вид границы</param>
    /// <param name="diagonals">Если true, то дополнительно будет установлены свойства DiagonalUpBorder и DiagonalDownBorder</param>
    public void SetAllBorders(EFPDataGridViewBorderStyle style, bool diagonals)
    {
      LeftBorder = style;
      TopBorder = style;
      RightBorder = style;
      BottomBorder = style;
      if (diagonals)
      {
        DiagonalUpBorder = style;
        DiagonalDownBorder = style;
      }
    }

    #endregion

    #region Защищенные методы

    /// <summary>
    /// Инициализация аргументов
    /// </summary>
    /// <param name="rowIndex">Индекс строки табличного просмотра</param>
    /// <param name="reason">Свойство Reason</param>
    protected void InitRow(int rowIndex, EFPDataGridViewAttributesReason reason)
    {
      _RowIndex = rowIndex;
      _Reason = reason;
      ColorType = EFPDataGridViewColorType.Normal;
      _Grayed = null;
      LeftBorder = EFPDataGridViewBorderStyle.Default;
      TopBorder = EFPDataGridViewBorderStyle.Default;
      RightBorder = EFPDataGridViewBorderStyle.Default;
      BottomBorder = EFPDataGridViewBorderStyle.Default;
      DiagonalUpBorder = EFPDataGridViewBorderStyle.Default;
      DiagonalDownBorder = EFPDataGridViewBorderStyle.Default;
    }

    #endregion
  }

  /// <summary>
  /// Аргументы события EFPDataGridView.GetRowAttributes
  /// </summary>
  public class EFPDataGridViewRowAttributesEventArgs : EFPDataGridViewCellAttributesEventArgsBase
  {
    #region Конструктор

    /// <summary>
    /// Создает аргументы события
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра</param>
    public EFPDataGridViewRowAttributesEventArgs(EFPDataGridView controlProvider)
      : base(controlProvider, false)
    {
      CellErrorMessages = new Dictionary<int, ErrorMessageList>();
    }

    #endregion

    #region Устанавливаемые свойства

    /// <summary>
    /// При печати запретить отрывать текущую строку от предыдущей
    /// </summary>
    public bool PrintWithPrevious { get { return _PrintWithPrevious; } set { _PrintWithPrevious = value; } }
    private bool _PrintWithPrevious;

    /// <summary>
    /// При печати запретить отрывать текущую строку от следующей
    /// </summary>
    public bool PrintWithNext { get { return _PrintWithNext; } set { _PrintWithNext = value; } }
    private bool _PrintWithNext;

    /// <summary>
    /// Изображение, которое будет выведено в заголовке строки (в серой ячейке)
    /// Должно быть установлено свойство EFPDataGridView.UseRowImages
    /// Это свойство также используется при навигации по Ctrl-[, Ctrl-]
    /// Стандартный значок может быть переопределен установкой свойства UserImage
    /// </summary>
    public EFPDataGridViewImageKind ImageKind { get { return _ImageKind; } set { _ImageKind = value; } }
    private EFPDataGridViewImageKind _ImageKind;

    /// <summary>
    /// Переопределение изображения, которое будет выведено в заголовке строки 
    /// (в серой ячейке)
    /// Если свойство не установлено (по умолчанию), то используются стандартные
    /// значки для ImageKind=Information,Warning и Error, или пустое изображение
    /// при ImageKind=None. 
    /// Свойство не влияет на навигацию по строкам по Ctrl-[, Ctrl-]
    /// Должно быть установлено свойство EFPDataGridView.UseRowImages
    /// </summary>
    public Image UserImage { get { return _UserImage; } set { _UserImage = value; } }
    private Image _UserImage;

    /// <summary>
    /// Всплывающая подсказка, которая будет выведена при наведении курсора на 
    /// ячейку заголовка строки. Поле должно заполняться только при Reason=ToolTip.
    /// В других режимах значение игнорируется
    /// </summary>
    public string ToolTipText { get { return _ToolTipText; } set { _ToolTipText = value; } }
    private string _ToolTipText;

    /// <summary>
    /// Используется методом EFPDataGridView.GetRowErrorMessages
    /// </summary>
    internal ErrorMessageList RowErrorMessages;

    /// <summary>
    /// Словарь для сообщений об ошибках в ячейках
    /// Ключом является индекс столбца ColumnIndex
    /// </summary>
    internal Dictionary<int, ErrorMessageList> CellErrorMessages;

    /// <summary>
    /// Идентификатор строки, используемый при построении списка сообщений об ошибках.
    /// Если свойство явно не устанавливается в обработчике GetRowAttributes, оно принимает значение по умолчанию
    /// </summary>
    public string RowIdText
    {
      get
      {
        if (String.IsNullOrEmpty(_RowIdText))
          return DefaultRowIdText;
        else
          return _RowIdText;
      }
      set
      {
        _RowIdText = value;
      }
    }
    private string _RowIdText;

    /// <summary>
    /// Идентификатор строки по умолчанию.
    /// Если установлено свойство EFPDataGridView.RowIdTextDataColumnName
    /// </summary>
    public string DefaultRowIdText
    {
      get
      {
        if (!String.IsNullOrEmpty(ControlProvider.RowIdTextDataColumnName))
        {
          if (DataRow == null)
            return "Нет строки";
          return DataTools.GetString(DataRow, ControlProvider.RowIdTextDataColumnName);
        }

        // Значение по умолчанию
        return "Строка " + (RowIndex + 1).ToString();
      }
    }

    /// <summary>
    /// Используется вместо штучной установки свойства ContentVisible в обработчике
    /// GetCellAttributes. По умолчанию - true. Если свойство сбросить в false,
    /// то для всех ячеек типа DataGridViewCheckBoxCell, DataGridViewButtonCell и
    /// DataGridViewComboBoxCell будет устанавливаться свойство ContentVisible=false
    /// </summary>
    public bool ControlContentVisible { get { return _ControlContentVisible; } set { _ControlContentVisible = value; } }
    private bool _ControlContentVisible;

    #endregion

    #region Сообщения об ошибках и подсказки

    #region Основные методы

    /// <summary>
    /// Устанавливает для строки подходящее изображение и добавляет сообщения
    /// к свойству ToolTipText из списка ошибок
    /// Эквивалентно вызову метода AddRowErrorMessage() для каждого сообщения в списке
    /// </summary>
    /// <param name="errors">Список ошибок</param>
    public void AddRowErrorMessages(ErrorMessageList errors)
    {
      if (errors == null)
        return;
      for (int i = 0; i < errors.Count; i++)
        AddRowErrorMessage(errors[i]);
    }

    /// <summary>
    /// Устанавливает для строки подходящее изображение и добавляет сообщение
    /// к свойству ToolTipText.
    /// Эквивалентно одному из вызовов: AddRowError(), AddRowWarning() или
    /// AddRowInformation()
    /// </summary>
    /// <param name="kind">Важность: Ошибка, предупреждение или сообщение</param>
    /// <param name="message">Текст сообщения</param>
    public void AddRowErrorMessage(ErrorMessageKind kind, string message)
    {
      AddRowErrorMessage(new ErrorMessageItem(kind, message));
    }

    /// <summary>
    /// Устанавливает для строки подходящее изображение и добавляет сообщение
    /// к свойству ToolTipText.
    /// Эквивалентно одному из вызовов: AddRowError(), AddRowWarning() или
    /// AddRowInformation()
    /// </summary>
    /// <param name="error">Объект ошибки ErrorMessageItem</param>
    public void AddRowErrorMessage(ErrorMessageItem error)
    {
      AddRowErrorMessage(error, String.Empty);
    }

    /// <summary>
    /// Устанавливает для строки подходящее изображение и добавляет сообщение
    /// к свойству ToolTipText.
    /// Позволяет привязать сообщение об ошибке и задать подсветку для выбранных ячеек строки.
    /// Эквивалентно одному из вызовов: AddRowError(), AddRowWarning() или
    /// AddRowInformation()
    /// </summary>
    /// <param name="error">Объект ошибки ErrorMessageItem</param>
    /// <param name="columnNames">Список имен столбцов (через запятую), для которых задается подсветка ошибки</param>
    public void AddRowErrorMessage(ErrorMessageItem error, string columnNames)
    {
      EFPDataGridViewImageKind NewImageKind;
      switch (error.Kind)
      {
        case ErrorMessageKind.Error: NewImageKind = EFPDataGridViewImageKind.Error; break;
        case ErrorMessageKind.Warning: NewImageKind = EFPDataGridViewImageKind.Warning; break;
        case ErrorMessageKind.Info: NewImageKind = EFPDataGridViewImageKind.Information; break;
        default:
          throw new ArgumentException("Неправильное значение Error.Kind=" + error.Kind.ToString());
      }

      if ((int)NewImageKind > (int)ImageKind)
        ImageKind = NewImageKind;

      AddToolTipText(error.Text);
      if (RowErrorMessages != null)
        RowErrorMessages.Add(new ErrorMessageItem(error.Kind, error.Text, error.Code, RowIndex)); // исправлено 09.11.2014

      if (!String.IsNullOrEmpty(columnNames))
      {
        if (columnNames.IndexOf(',') >= 0)
        {
          string[] a = columnNames.Split(',');
          for (int i = 0; i < a.Length; i++)
            AddOneCellErrorMessage(error, a[i]);
        }
        else
          AddOneCellErrorMessage(error, columnNames);
      }
    }

    private void AddOneCellErrorMessage(ErrorMessageItem error, string columnName)
    {
      int ColumnIndex = ControlProvider.Columns.IndexOf(columnName);
      if (ColumnIndex < 0)
        return; // имя несуществующего столбца
      ErrorMessageList Errors;
      if (!CellErrorMessages.TryGetValue(ColumnIndex, out Errors))
      {
        Errors = new ErrorMessageList();
        CellErrorMessages.Add(ColumnIndex, Errors);
      }
      Errors.Add(error);
    }

    private void AddToolTipText(string message)
    {
      if (Reason != EFPDataGridViewAttributesReason.ToolTip &&
        Reason != EFPDataGridViewAttributesReason.View) // 15.09.2015 Сообщения для невиртуального просмотра
        return;

#if DEBUG
      if (String.IsNullOrEmpty(message))
        throw new ArgumentNullException("message");
#endif

      if (String.IsNullOrEmpty(ToolTipText))
        ToolTipText = message;
      else
        ToolTipText += Environment.NewLine + message;
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Устанавливает для строки изображение ошибки и добавляет сообщение к свойству
    /// ToolTipText
    /// </summary>
    /// <param name="message">Сообщение</param>
    public void AddRowError(string message)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Error, message), String.Empty);
    }

    /// <summary>
    /// Устанавливает для строки изображение ошибки и добавляет сообщение к свойству
    /// ToolTipText
    /// Позволяет привязать сообщение об ошибке и задать подсветку для выбранных ячеек строки.
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="columnNames">Список имен столбцов (через запятую), для которых задается подсветка ошибки</param>
    public void AddRowError(string message, string columnNames)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Error, message), columnNames);
    }

    /// <summary>
    /// Устанавливает для строки изображение предупреждения (если оно не установлено уже в ошибку)
    /// и добавляет сообщение к свойству ToolTipText
    /// </summary>
    /// <param name="message">Сообщение</param>
    public void AddRowWarning(string message)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Warning, message), String.Empty);
    }

    /// <summary>
    /// Устанавливает для строки изображение предупреждения (если оно не установлено уже в ошибку)
    /// и добавляет сообщение к свойству ToolTipText
    /// Позволяет привязать предупреждение и задать подсветку для выбранных ячеек строки.
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="columnNames">Список имен столбцов (через запятую), для которых задается подсветка предупреждения</param>
    public void AddRowWarning(string message, string columnNames)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Warning, message), columnNames);
    }

    /// <summary>
    /// Устанавливает для строки изображение сообщения и добавляет сообщение к свойству
    /// ToolTipText
    /// </summary>
    /// <param name="message">Сообщение</param>
    public void AddRowInformation(string message)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Info, message), String.Empty);
    }

    /// <summary>
    /// Устанавливает для строки изображение сообщения и добавляет сообщение к свойству
    /// ToolTipText.
    /// Позволяет привязать сообщение для выбранных ячеек строки.
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="columnNames">Список имен столбцов (через запятую), для которых задается всплывающая подсказка</param>
    public void AddRowInformation(string message, string columnNames)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Info, message), columnNames);
    }

    #endregion

    #endregion

    #region Защишенные методы

    /// <summary>
    /// Инициализация аргументов
    /// </summary>
    /// <param name="rowIndex">Индекс строки табличного просмотра</param>
    /// <param name="reason">Свойство Reason</param>
    public new void InitRow(int rowIndex, EFPDataGridViewAttributesReason reason)
    {
      base.InitRow(rowIndex, reason);
      PrintWithPrevious = false;
      PrintWithNext = false;
      ImageKind = EFPDataGridViewImageKind.None;
      UserImage = null;
      ToolTipText = String.Empty;

      // Эмуляция стандартного поведения DataGridView по обработке DataRow.RowError
      if (ControlProvider.UseRowImagesDataError)
      {
        DataRow Row = this.DataRow;
        if (Row != null)
        {
          ToolTipText = Row.RowError;
          if (!String.IsNullOrEmpty(ToolTipText))
            ImageKind = EFPDataGridViewImageKind.Error;
        }
      }

      ReadOnly = false;
      ReadOnlyMessage = "Строка предназначена только для просмотра";
      if (reason == EFPDataGridViewAttributesReason.ReadOnly)
      {
        DataGridViewRow GridRow = ControlProvider.Control.Rows.SharedRow(rowIndex);
        ReadOnly = (GridRow.GetState(rowIndex) & DataGridViewElementStates.ReadOnly) == DataGridViewElementStates.ReadOnly;
      }

      ControlContentVisible = true;
      CellErrorMessages.Clear();
      RowIdText = null;
    }

    #endregion
  }


  /// <summary>
  /// Аргументы события EFPDataGridView.GetCellAttributes
  /// </summary>
  public class EFPDataGridViewCellAttributesEventArgs : EFPDataGridViewCellAttributesEventArgsBase
  {
    #region Конструктор

    /// <summary>
    /// Создает аргуметы события
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра</param>
    public EFPDataGridViewCellAttributesEventArgs(EFPDataGridView controlProvider)
      : base(controlProvider, true)
    {
    }

    #endregion

    #region Свойства - исходные данные

    /// <summary>
    /// Индекс столбца в табличном просмотре (независимо от текущего порядка столбцов).
    /// Совпадает с DataGridViewColumn.Index
    /// </summary>
    public int ColumnIndex { get { return _ColumnIndex; } }
    private int _ColumnIndex;

    /// <summary>
    /// Объект столбца в EFPDataGridView
    /// </summary>
    public EFPDataGridViewColumn Column { get { return _Column; } }
    private EFPDataGridViewColumn _Column;

    /// <summary>
    /// Объект столбца в DataGridView
    /// </summary>
    public DataGridViewColumn GridColumn { get { return Column.GridColumn; } }

    /// <summary>
    /// Имя столбца DataGridViewColumn.Name
    /// </summary>
    public string ColumnName { get { return Column.Name; } }

    /// <summary>
    /// Настройки вида ячейки для выравнивания и др. атрибутов
    /// </summary>
    public DataGridViewCellStyle CellStyle
    {
      get
      {
        if (_CellStyle == null)
          _CellStyle = new DataGridViewCellStyle(_TemplateCellStyle);
        return _CellStyle;
      }
    }
    private DataGridViewCellStyle _CellStyle;
    /// <summary>
    /// Вместо реального стиля, который можно непосредственно менять, может быть
    /// использован шаблон стиля. В этом случае будет сгенерирован новый объект
    /// при любом обращении к CellStyle
    /// </summary>
    private DataGridViewCellStyle _TemplateCellStyle;

    /// <summary>
    /// Используется в DoGetCellAttributes() после вызова обработчика, чтобы вызвавший
    /// модуль мог работать с CellStyle не задумываясь о лишнем копировании
    /// </summary>
    internal void MoveTemplateToStyle()
    {
      if (_CellStyle == null)
        _CellStyle = _TemplateCellStyle;
    }

    /// <summary>
    /// Доступ к ячейке табличного просмотра.
    /// Вызов метода делает строку Unshared
    /// </summary>
    /// <returns>Объект ячейки</returns>
    public DataGridViewCell GetGridCell()
    {
      if (ColumnIndex < 0)
        return null;
      else
      {
        DataGridViewRow r = GetGridRow();
        if (r == null)
          return null;
        else
          return r.Cells[ColumnIndex];
      }
    }

    /// <summary>
    /// Исходное (неотформатированное) значение
    /// </summary>
    public object OriginalValue { get { return _OriginalValue; } }
    private object _OriginalValue;

    #endregion

    #region Устанавливаемые свойства

    /// <summary>
    /// Форматируемое значение
    /// </summary>
    public object Value { get { return _Value; } set { _Value = value; } }
    private object _Value;

    /// <summary>
    /// Свойство должно быть установлено в true, если было применено форматирование
    /// значения
    /// </summary>
    public bool FormattingApplied { get { return _FormattingApplied; } set { _FormattingApplied = value; } }
    private bool _FormattingApplied;

    /// <summary>
    /// Отступ от левого или от правого края (в зависимости от горизонтального 
    /// выравнивания) значения ячейки
    /// </summary>
    public int IndentLevel { get { return _IndentLevel; } set { _IndentLevel = value; } }
    private int _IndentLevel;

    /// <summary>
    /// Возвращает значение Value или OriginalValue, в зависимости от наличия значений
    /// и свойства FormattingApplied
    /// Это значение должно использоваться для вывода
    /// </summary>
    public object FormattedValue
    {
      get
      {
        if (FormattingApplied)
          return Value;
        else
        {
          if (OriginalValue == null || OriginalValue is DBNull)
            return Value;
          else
            return OriginalValue;
        }
      }
    }

    /// <summary>
    /// Всплывающая подсказка, которая будет выведена при наведении курсора на 
    /// ячейку. Поле должно заполняться только при Reason=ToolTip.
    /// В других режимах значение игнорируется. Перед вызовом события GetCellAttributes
    /// вызывается событик GridColumn.ToolTipNeeded, таким образом, поле уже может
    /// содержать значение.
    /// Значение подсказки для строки не копируется в это поле. Можно использовать
    /// значение RowToolTipText
    /// </summary>
    public string ToolTipText { get { return _ToolTipText; } set { _ToolTipText = value; } }
    private string _ToolTipText;

    /// <summary>
    /// Текст всплывающей подсказки для строки.
    /// Свойство имеет значение только при Reason=ToolTip
    /// Значение может быть использовано при формировании ToolTipText, чтобы 
    /// не вычислять текст еше раз, когда подсказка для ячеки основывается на подсказке
    /// для строки
    /// </summary>
    public string RowToolTipText { get { return _RowToolTipText; } }
    private string _RowToolTipText;


    /// <summary>
    /// Если установить в false, то содержимое ячейки не будет выводиться совсем.
    /// Полезно для CheckBox'ов и кнопок, если для какой-то строки ячеек они не нужны
    /// Скрытие работает для всех типов столбцов, включая DataGridViewTextBoxCell
    /// </summary>
    public bool ContentVisible { get { return _ContentVisible; } set { _ContentVisible = value; } }
    private bool _ContentVisible;

    #endregion

    #region Защишенные методы

    /// <summary>
    /// Инициализация аргументов.
    /// В первую очередь вызывает InitRow(), а затем инициализирует параметры для ячейки
    /// </summary>
    /// <param name="rowArgs">Аргументы вызванного ранее события EFPDataGridView.GetRowAttributes</param>
    /// <param name="columnIndex">Индекс столбца DataGridViewColumn.Index</param>
    /// <param name="cellStyle">Форматирование ячейки</param>
    /// <param name="styleIsTemplate"></param>
    /// <param name="originalValue"></param>
    public void InitCell(EFPDataGridViewRowAttributesEventArgs rowArgs, int columnIndex, DataGridViewCellStyle cellStyle, bool styleIsTemplate, object originalValue)
    {
      base.InitRow(rowArgs.RowIndex, rowArgs.Reason);
      _ColumnIndex = columnIndex;
      _Column = ControlProvider.Columns[columnIndex];

      ColorType = rowArgs.ColorType;
      if (Column.ColorType > ColorType)
        ColorType = Column.ColorType;

      if (rowArgs._Grayed.HasValue)
        _Grayed = rowArgs._Grayed.Value; // была явная установка свойства в обработчике строки
      else if (Column.Grayed)
        _Grayed = true;
      else
        _Grayed = null; // в принципе, это не нужно

      TopBorder = rowArgs.TopBorder;
      BottomBorder = rowArgs.BottomBorder;

      if (Column.LeftBorder == EFPDataGridViewBorderStyle.Default)
        LeftBorder = rowArgs.LeftBorder;
      else
        LeftBorder = Column.LeftBorder;
      if (Column.RightBorder == EFPDataGridViewBorderStyle.Default)
        RightBorder = rowArgs.RightBorder;
      else
        RightBorder = Column.RightBorder;

      IndentLevel = 0;
      if (styleIsTemplate)
      {
        _TemplateCellStyle = cellStyle;
        _CellStyle = null;
      }
      else
      {
        _CellStyle = cellStyle;
        _TemplateCellStyle = null;
      }
      _OriginalValue = originalValue;

      ReadOnly = false;
      ReadOnlyMessage = "Столбец предназначен только для просмотра";
      if (Reason == EFPDataGridViewAttributesReason.ReadOnly)
      {
        DataGridViewRow GridRow = ControlProvider.Control.Rows[RowIndex]; // Unshared
        ReadOnly = GridRow.Cells[columnIndex].ReadOnly;
        //  ReadOnly = ControlProvider.Control.Columns[ColumnIndex].ReadOnly;
      }

      ContentVisible = true;
      if (!rowArgs.ControlContentVisible)
      {
        DataGridViewColumn GridCol = Control.Columns[columnIndex];
        if (GridCol is DataGridViewCheckBoxColumn ||
          GridCol is DataGridViewButtonColumn ||
          GridCol is DataGridViewComboBoxColumn)

          ContentVisible = false;
      }

      _RowToolTipText = rowArgs.ToolTipText;

      ErrorMessageList CellErrors;
      if (rowArgs.CellErrorMessages.TryGetValue(columnIndex, out CellErrors))
      {
        // if (CellErrors.Count > 0) - можно не проверять
        switch (CellErrors.Severity)
        {
          case ErrorMessageKind.Error:
            ColorType = EFPDataGridViewColorType.Error;
            break;
          case ErrorMessageKind.Warning:
            if (ColorType != EFPDataGridViewColorType.Error)
              ColorType = EFPDataGridViewColorType.Warning;
            break;
          // Для Info цвет не меняем
        }

        if (rowArgs.Reason == EFPDataGridViewAttributesReason.ToolTip)
        {
          for (int i = 0; i < CellErrors.Count; i++)
          {
            if (!String.IsNullOrEmpty(_RowToolTipText))
              _RowToolTipText += Environment.NewLine;
            _RowToolTipText += CellErrors[i];
          }
        }
      }
    }

    #endregion
  }

  #region Делегаты

  /// <summary>
  /// Делегат события EFPDataGridView.GetRowAttributes
  /// </summary>
  /// <param name="sender">Объект EFPDataGridView</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPDataGridViewRowAttributesEventHandler(object sender,
    EFPDataGridViewRowAttributesEventArgs args);

  /// <summary>
  /// Делегат события EFPDataGridView.GetCellAttributes
  /// </summary>
  /// <param name="sender">Объект EFPDataGridView</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPDataGridViewCellAttributesEventHandler(object sender,
    EFPDataGridViewCellAttributesEventArgs args);

  #endregion

  #endregion

  #region EFPDataGridViewExcelCellAttributes

  /// <summary>
  /// В MS Excel нельзя использовать те же цвета, что и в табличном просмотре
  /// на экране. Вместо этого можно использовать атрибуты шрифта
  /// Метод EFPDataGridView.GetExcelCellAttr() позволяет получить атрибуты ячейки,
  /// совместимые с Microsoft Excel
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct EFPDataGridViewExcelCellAttributes
  {
    #region Поля

    /// <summary>
    /// Цвет фона ячейки
    /// Значение Color.Empty означает использование цвета фона по умолчанию ("Авто")
    /// </summary>
    public Color BackColor;

    /// <summary>
    /// Цвет текста
    /// Значение Color.Empty означает использование цвета шрифта по умолчанию ("Авто")
    /// </summary>
    public Color ForeColor;

    /// <summary>
    /// Использовать жирный шрифт
    /// </summary>
    public bool Bold;

    /// <summary>
    /// Использовать наклонный шрифт
    /// </summary>
    public bool Italic;

    /// <summary>
    /// Использовать подчеркнутый шрифт
    /// </summary>
    public bool Underline;

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Возвращает true, если никакие атрибцты не установлены
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return (!BackColor.IsEmpty) && (!ForeColor.IsEmpty) && (!Bold) && (!Italic) && (!Underline);
      }
    }

    #endregion
  }

  #endregion

  #region Режимы сохранения текущих строк

  /// <summary>
  /// Возможные способы сохранения / восстановления текущей позиции в табличном 
  /// просмотре (свойство EFPDataGridView.SelectedRowsMode)
  /// </summary>
  public enum EFPDataGridViewSelectedRowsMode
  {
    /// <summary>
    /// Режим по умолчанию Текущая строка не может быть восстановлена
    /// </summary>
    None,

    /// <summary>
    /// Сохрание номеров строк.
    /// Обычно подходит для табличных просмотров, не связанных с источником данных
    /// </summary>
    RowIndex,

    /// <summary>
    /// Сохранение ссылок на объекты DataRow
    /// Подходит для просмотров, связанных с DataTable/DataView при условии, что
    /// обновление таблицы не приводит к замене строк. Зато таблица может не иметь
    /// ключевого поля.
    /// </summary>
    DataRow,

    /// <summary>
    /// Самый подходящий режим для просмотров, связанных с DataTable/DataView,
    /// имеющих ключевое поле (или несколько полей, составляющих первичный ключ)
    /// </summary>
    PrimaryKey,

    /// <summary>
    /// Сохранение значений полей сортировки, заданных в свойстве DataView.ViewOrder
    /// для объекта, связанного с табличным просмотром. Не следует использовать
    /// этот режим, если в просмотре встречаются строки с одинаковыми значенями
    /// полей (сортировка не обеспечивает уникальности)
    /// </summary>
    DataViewSort,
  }

  /// <summary>
  /// Класс для сохранения текущей позиции и выбранных строк/столбцов в таблчном просмотре
  /// (свойство EFPDataGridView.Selection)
  /// Не содержит открытых полей
  /// </summary>
  public class EFPDataGridViewSelection
  {
    /// <summary>
    /// Выбранные строки
    /// </summary>
    internal object SelectedRows;

    /// <summary>
    /// Текущая строка
    /// </summary>
    internal object CurrentRow;

    /// <summary>
    /// Режим "Выбраны все строки". В этом случае выбранные строки не запоминаются
    /// </summary>
    internal bool SelectAll;

    /// <summary>
    /// Номер текущего столбца
    /// </summary>
    internal int CurrentColumnIndex;

    /// <summary>
    /// true, если свойство DataGridView.DataSource было установлено
    /// </summary>
    internal bool DataSourceExists;
  }

  #endregion

  #region Режимы установки отметок строк

  /// <summary>
  /// Аргумент RowList метода EFPDataGridView.CheckMarkRows()
  /// </summary>
  public enum EFPDataGridViewCheckMarkRows
  {
    /// <summary>
    /// Для выбранных ячеек
    /// </summary>
    Selected,

    /// <summary>
    /// Для всех ячеек просмотра
    /// </summary>
    All
  }

  /// <summary>
  /// Аргумент Action метода EFPDataGridView.CheckMarkRows()
  /// </summary>
  public enum EFPDataGridViewCheckMarkAction
  {
    /// <summary>
    /// Установить флажки
    /// </summary>
    Check,

    /// <summary>
    /// Снять флажки
    /// </summary>
    Uncheck,

    /// <summary>
    /// Переключить состояние флажков на противоположное
    /// </summary>
    Invert
  }

  #endregion

  #region Событие CellFinished

  /// <summary>
  /// Причина вызова события CellFinished
  /// </summary>
  public enum EFPDataGridViewCellFinishedReason
  {
    /// <summary>
    /// Вызвано событие DataGridView.CellValueChanged в процессе редактирования ячейки
    /// или значение вставлено из калькулятора
    /// </summary>
    Edit,

    /// <summary>
    /// Вставлено текстовое значение из буфера обмена с помощью EFPDataGridViewCommandItems.PerformPasteText()
    /// </summary>
    Paste,

    /// <summary>
    /// Значение очищено при вырезке текста ячеек в буфер обмена командой Cut
    /// </summary>
    Clear,

    /// <summary>
    /// Установка / снята отметка строки с помощью EFPDataGridView.CheckMarkRows()
    /// </summary>
    MarkRow,
  }

  /// <summary>
  /// Аргументы события EFPDataGridView.CellFinished
  /// </summary>
  public class EFPDataGridViewCellFinishedEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает аргументы события
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра, вызвавшего событие</param>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="columnIndex">Индекс строки</param>
    /// <param name="reason">Причина появления события</param>
    public EFPDataGridViewCellFinishedEventArgs(EFPDataGridView controlProvider, int rowIndex, int columnIndex,
      EFPDataGridViewCellFinishedReason reason)
    {
      _ControlProvider = controlProvider;
      _RowIndex = rowIndex;
      _ColumnIndex = columnIndex;
      _Reason = reason;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра, вызвавшего событие
    /// </summary>
    public EFPDataGridView ControlProvider { get { return _ControlProvider; } }
    private EFPDataGridView _ControlProvider;

    /// <summary>
    /// Индекс строки
    /// </summary>
    public int RowIndex { get { return _RowIndex; } }
    private int _RowIndex;

    /// <summary>
    /// Индекс строки
    /// </summary>
    public int ColumnIndex { get { return _ColumnIndex; } }
    private int _ColumnIndex;

    /// <summary>
    /// Причина появления события
    /// </summary>
    public EFPDataGridViewCellFinishedReason Reason { get { return _Reason; } }
    private EFPDataGridViewCellFinishedReason _Reason;

    /// <summary>
    /// Ячейка табличного просмотра
    /// </summary>
    public DataGridViewCell Cell
    {
      get { return _ControlProvider.Control[ColumnIndex, RowIndex]; }
    }

    /// <summary>
    /// Строка данных, если табличный просмотр связан с таблицей DataTable.
    /// Иначе возвращает null.
    /// </summary>
    public DataRow DataRow
    {
      get
      {
        return _ControlProvider.GetDataRow(RowIndex);
      }
    }

    /// <summary>
    /// Имя столбца табличного просмотра EFPDataGridViewColumn.Name.
    /// </summary>
    public string ColumnName { get { return _ControlProvider.Columns[ColumnIndex].Name; } }

    #endregion
  }

  /// <summary>
  /// Делегат события EFPDataGridView.CellFinished
  /// </summary>
  /// <param name="sender">Объект EFPDataGridView</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPDataGridViewCellFinishedEventHandler(object sender,
    EFPDataGridViewCellFinishedEventArgs args);

  #endregion

  #region Режимы подбора высоты строк

  /// <summary>
  /// Возможные значения свойства EFPDataGridView.AutoSizeRowsMode.
  /// В текущей реализации существует единственный режим Auto.
  /// </summary>
  public enum EFPDataGridViewAutoSizeRowsMode
  {
    /// <summary>
    /// Нет управления высотой строки или оно реализовано средствами DataGridView
    /// </summary>
    None,

    /// <summary>
    /// Выполняется автоподбор высоты отображаемых на экране строк
    /// </summary>
    Auto
  }

  #endregion


  #region Интерфейс IEFPDataView

  /// <summary>
  /// Информация об одном столбце EFPDataGridView или EFPDataTreeView
  /// </summary>
  public struct EFPDataViewColumnInfo
  {
    #region Конструктор

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dataPropertName"></param>
    /// <param name="columnProducer"></param>
    public EFPDataViewColumnInfo(string name, string dataPropertName, IEFPGridProducerColumn columnProducer)
    {
      _Name = name;
      _DataPropertyName = dataPropertName;
      _ColumnProducer = columnProducer;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя столбца. Должно быть уникальным в пределах просмотра
    /// </summary>
    public string Name { get { return _Name; } }
    private string _Name;

    /// <summary>
    /// Имя поля источника данных, если столбец отображает данные из источника.
    /// Для вычисляемых столбцов содержит null
    /// </summary>
    public string DataPropertyName { get { return _DataPropertyName; } }
    private string _DataPropertyName;

    /// <summary>
    /// Генератор столбца табличного просмотра. Null, если не использовался GridProducer.
    /// </summary>
    public IEFPGridProducerColumn ColumnProducer { get { return _ColumnProducer; } }
    private IEFPGridProducerColumn _ColumnProducer;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return _Name;
    }

    #endregion
  }

  /// <summary>
  /// Общие методы и свойства для EFPDataGridView и EFPDataTreeView
  /// </summary>
  public interface IEFPDataView : IEFPControl
  {
    /// <summary>
    /// true, если просмотр предназначен только для просмотра, а не для редактирования данных
    /// </summary>
    bool ReadOnly { get; set; }

    /// <summary>
    /// Управляемое свойство для ReadOnly
    /// </summary>
    DepValue<bool> ReadOnlyEx { get; set; }

    /// <summary>
    /// true, если допускается создание новых записей.
    /// Действует, если ReadOnly=false
    /// </summary>
    bool CanInsert { get; set; }

    /// <summary>
    /// Управляемое свойство для CanInsert
    /// </summary>
    DepValue<bool> CanInsertEx { get; set; }

    /// <summary>
    /// true, если допускается создание новых записей на основании существующей.
    /// Действует, если ReadOnly=false, а CanInsert=true
    /// </summary>
    bool CanInsertCopy { get; set; }

    /// <summary>
    /// Управляемое свойство для CanInsertCopy
    /// </summary>
    DepValue<bool> CanInsertCopyEx { get; set; }

    /// <summary>
    /// true, если допускается удаление записей.
    /// Действует, если ReadOnly=false
    /// </summary>
    bool CanDelete { get; set; }

    /// <summary>
    /// Управляемое свойство для CanDelete
    /// </summary>
    DepValue<bool> CanDeleteEx { get; set; }

    /// <summary>
    /// true, если допускается групповое редактирование и просмотр записей.
    /// false, если допускается редактирование / просмотр только одной записи за раз.
    /// Действует, если ReadOnly=false или CanView=true.
    /// Не влияет на возможность удаления нескольких записей
    /// </summary>
    bool CanMultiEdit { get; set; }

    /// <summary>
    /// true, если возможен просмотр записей
    /// </summary>
    bool CanView { get; set; }

    /// <summary>
    /// Управляемое свойство для ReadOnly
    /// </summary>
    DepValue<bool> CanViewEx { get; set; }

    /// <summary>
    /// Возвращает источник данных в виде DataTable.
    /// Возвращает null, если источник данных не является таблицей или просмотром таблицы
    /// </summary>
    DataTable SourceAsDataTable { get; }

    /// <summary>
    /// Возвращает источник данных в виде DataView.
    /// Возвращает null, если источник данных не является таблицей или просмотром таблицы
    /// </summary>
    DataView SourceAsDataView { get; }

    /// <summary>
    /// Возвращает строку таблицы данных, соответствующую текущей строке / узлу
    /// </summary>
    DataRow CurrentDataRow { get; set; }

    /// <summary>
    /// Выбранные строки данных, если поддерживается множественный выбор.
    /// Если допускается выбор только одной строки, то массив содержит значение свойства CurrentDataRow
    /// </summary>
    DataRow[] SelectedDataRows { get; set; }

    /// <summary>
    /// Проверяет, что в просмотре выбрана ровно одна строка.
    /// Выдает сообщение, если это не так
    /// </summary>
    /// <returns>true, если выбрана одна строка</returns>
    bool CheckSingleRow();

    /// <summary>
    /// Доступна ли для пользователя команда "Обновить просмотр".
    /// Возвращает свойство CommandItems.UseRefresh.
    /// В частности, для отчетов EFPReportDataGridViewPage возвращает false
    /// </summary>
    bool UseRefresh { get; }

    /// <summary>
    /// Выполняет обновление просмотра
    /// </summary>
    void PerformRefresh();

    /// <summary>
    /// Обновить все строки табличного просмотра.
    /// В отличие от PerformRefresh(), обновляются только существующие строки, а не выполняется запрос к базе данных
    /// </summary>
    void InvalidateAllRows();

    /// <summary>
    /// Обновить в просмотре все выбранные строки SelectedRows, например, после
    /// редактирования, если имеются вычисляемые столбцы
    /// </summary>
    void InvalidateSelectedRows();

    /// <summary>
    /// Пометить на обновление строку табличного просмотра, связанную с заданной строкой таблицы данных
    /// </summary>
    /// <param name="row">Строка связанной таблицы данных</param>
    void InvalidateDataRow(DataRow row);

    /// <summary>
    /// Пометить на обновление строки табличного просмотра, связанные с заданными строками таблицы данных
    /// </summary>
    /// <param name="rows">Массив строк связанной таблицы данных</param>
    void InvalidateDataRows(DataRow[] rows);

    /// <summary>
    /// Имя текущего столбца. Если выбрано несколько столбцов (например, строка целиком),
    /// то пустая строка ("")
    /// </summary>
    string CurrentColumnName { get; }
  }

  #endregion

  #region Интерфейс IEFPGridControl

  /// <summary>
  /// Интерфейс провайдера табличного просмотра.
  /// Предназначен исключительно для использования в библиотеке ExtForms и ExtDBDocForms.
  /// </summary>
  public interface IEFPGridControl : IEFPControl
  {
    /// <summary>
    /// Интерфейс для определения размеров элементов просмотра
    /// </summary>
    IEFPGridControlMeasures Measures { get; }

    /// <summary>
    /// Возвращает индекс текущего столбца просмотра или (-1), если выбрана вся строка
    /// </summary>
    int CurrentColumnIndex { get; }

    /// <summary>
    /// Текущая конфигурация столбцов просмотра
    /// </summary>
    EFPDataGridViewConfig CurrentConfig { get; set; }

    /// <summary>
    /// Генератор столбцов табличного просмотра
    /// </summary>
    IEFPGridProducer GridProducer { get; }

    /// <summary>
    /// Возвращает информацию о видимых столбцах
    /// </summary>
    /// <returns></returns>
    EFPDataViewColumnInfo[] GetVisibleColumnsInfo();

    /// <summary>
    /// Должен заполнить в объекте EFPDataGridViewConfigColumn ширину и другие поля в сответствии с 
    /// реальной шириной в просмотре.
    /// </summary>
    /// <param name="configColumn">Заполняемая информация о столбце</param>
    /// <returns>true, если столбец есть в просмотре и данные были заполнены</returns>
    bool InitColumnConfig(EFPDataGridViewConfigColumn configColumn);
  }

  #endregion

  /// <summary>
  /// Провайдер табличного просмотра
  /// </summary>
  public class EFPDataGridView : EFPControl<DataGridView>, IEFPDataView, IEFPGridControl
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, привязанный к DataGridView
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент Windows Forms</param>
    public EFPDataGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control, true)
    {
      Init();
    }

    /// <summary>
    /// Создает объект, привязанный к ControlWithToolBar
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элмент и панель инструментов</param>
    public EFPDataGridView(IEFPControlWithToolBar<DataGridView> controlWithToolBar)
      : base(controlWithToolBar, true)
    {
      Init();
    }

    private void Init()
    {
      Control.MultiSelect = false;
      //Control.RowTemplate.Height = DefaultRowHeight;
      Control.AllowUserToResizeRows = false;
      Control.AllowUserToAddRows = false;
      Control.AllowUserToDeleteRows = false;
      Control.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      Control.StandardTab = true;
      Control.DefaultCellStyle.ForeColor = SystemColors.WindowText; // 13.12.2017
      Control.ShowCellToolTips = EFPApp.ShowToolTips; // 05.04.2018
      _GetRowAttributesArgs = new EFPDataGridViewRowAttributesEventArgs(this);
      _GetCellAttributesArgs = new EFPDataGridViewCellAttributesEventArgs(this);
      Control_Leave(null, null);

      _State = EFPDataGridViewState.View;
      _ReadOnly = false;
      _CanInsert = true;
      _CanInsertCopy = false;
      _CanDelete = true;
      _CanView = true;
      _CanMultiEdit = false;
      _InsideEditData = false;

      _CurrentColumnIndex = -1;

      _CurrentIncSearchChars = String.Empty;
      //FCurrentIncSearchMask = null;
      _TextSearchContext = null;
      _TextSearchEnabled = true;

      _SelectedRowsMode = EFPDataGridViewSelectedRowsMode.None;

      _OrderChangesToRefresh = false;
      _AutoSort = false;
      _UseGridProducerOrders = true;
      _UseColumnHeaderClick = true;
      _UseDefaultDataError = true;

      if (!DesignMode)
      {
        Control.CurrentCellChanged += new EventHandler(Control_CurrentCellChanged);
        Control.VisibleChanged += new EventHandler(Control_VisibleChanged);
        Control.DataSourceChanged += new EventHandler(Control_DataBindingChanged);
        Control.DataMemberChanged += new EventHandler(Control_DataBindingChanged);
        Control.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(Control_DataBindingComplete);
        Control.KeyDown += new KeyEventHandler(Control_KeyDown);
        Control.MouseDown += new MouseEventHandler(Control_MouseDown);
        Control.RowPrePaint += new DataGridViewRowPrePaintEventHandler(Control_RowPrePaint);
        Control.CellPainting += new DataGridViewCellPaintingEventHandler(Control_CellPainting);
        Control.RowPostPaint += new DataGridViewRowPostPaintEventHandler(Control_RowPostPaint);
        Control.CellFormatting += new DataGridViewCellFormattingEventHandler(Control_CellFormatting);
        Control.Enter += new EventHandler(Control_Enter);
        Control.Leave += new EventHandler(Control_Leave);
        Control.KeyPress += new KeyPressEventHandler(Control_KeyPress);
        Control.KeyDown += new KeyEventHandler(Control_KeyDown);
        Control.CellClick += new DataGridViewCellEventHandler(Control_CellClick);
        Control.CellContentClick += new DataGridViewCellEventHandler(Control_CellContentClick);
        Control.CurrentCellDirtyStateChanged += new EventHandler(Control_CurrentCellDirtyStateChanged);
        Control.CellValueChanged += new DataGridViewCellEventHandler(Control_CellValueChanged);
        Control.CellParsing += new DataGridViewCellParsingEventHandler(Control_CellParsing);
        Control.CellValidating += new DataGridViewCellValidatingEventHandler(Control_CellValidating);
        Control.CellBeginEdit += new DataGridViewCellCancelEventHandler(Control_CellBeginEdit1);
        //Control.CellEndEdit += new DataGridViewCellEventHandler(Control_CellEndEdit1);
        Control.DataError += new DataGridViewDataErrorEventHandler(Control_DataError);
        if (EFPApp.ShowToolTips)
        {
          //Control.VirtualMode = true; // Так нельзя. Перестает работать EFPErrorDataGridView и множество других просмотров без источников данных
          Control.CellToolTipTextNeeded += new DataGridViewCellToolTipTextNeededEventHandler(Control_CellToolTipTextNeeded);
        }
        Control.ReadOnlyChanged += Control_ReadOnlyChanged;
      }

      base.UseIdle = true;
    }

    #endregion

    #region Предотвращение мерцания при обновлении

    private int _UpdateCount = 0;

    private bool? _BeginUpdateVisible;

    /// <summary>
    /// Начать обновление данных табличного просмотра
    /// </summary>
    public void BeginUpdate()
    {
      // Так не работает, все равно все прыгает
      // EFPDataGridView.SourceAsDataTable.BeginLoadData();
      // EFPDatGridView.SourceAsDataTable.BeginInit();

      if (_UpdateCount == 0)
      {
        if (WinFormsTools.AreControlAndFormVisible(Control) &&
          ProviderState == EFPControlProviderState.Attached /* 15.07.2021 */)
        {
          _BeginUpdateVisible = Control.Visible;
          Control.Visible = false;
        }
        else
          _BeginUpdateVisible = null;
      }
      _UpdateCount++;
    }

    /// <summary>
    /// Закончить обновление данных табличного просмотра
    /// </summary>
    public void EndUpdate()
    {
      if (_UpdateCount <= 0)
        throw new InvalidOperationException("Не было вызова BeginUpdate");

      _UpdateCount--;
      if (_UpdateCount == 0)
      {
        if (_BeginUpdateVisible.HasValue)
          Control.Visible = _BeginUpdateVisible.Value;
      }
    }

    #endregion

    #region Обработчики событий

    /// <summary>
    /// Этот метод вызывается после установки свойства CommandItems.Control
    /// Добавляем обработчики, которые должны быть в конце цепочки
    /// </summary>
    internal protected virtual void AfterControlAssigned()
    {
      // Мы должны присоединить обработчик CellBeginEdit после пользовательского, т.к.
      // нам нужно проверить свойство Cancel
      Control.CellBeginEdit += new DataGridViewCellCancelEventHandler(Control_CellBeginEdit2);
      Control.CellEndEdit += new DataGridViewCellEventHandler(Control_CellEndEdit2);
    }

    #region CurrentCellChanged

    private bool _CurrentCellChangedFlag;

    void Control_CurrentCellChanged(object sender, EventArgs args)
    {
      _CurrentCellChangedFlag = true;
    }

    /// <summary>
    /// Вызывает при первом выводе таблицы на экран
    /// </summary>
    protected override void OnCreated()
    {
      // Должно быть до вызова метода базового класса
      if (AutoSizeRowsMode == EFPDataGridViewAutoSizeRowsMode.Auto)
        new EFPDataGridViewRowsResizer(this);

      base.OnCreated();
      _CurrentCellChangedFlag = true; // на случай, если с таблицей выполнялись манипуляции до показа на экране

      if (UseColumnHeaderClick)
      {
        Control.ColumnHeaderMouseClick += new DataGridViewCellMouseEventHandler(Control_ColumnHeaderMouseClick);
        Control.ColumnHeaderMouseDoubleClick += new DataGridViewCellMouseEventHandler(Control_ColumnHeaderMouseClick); // 04.07.2021
      }
    }

    /// <summary>
    /// Вызывает RefreshOrders()
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();
      CommandItems.InitOrderItems();
      InitColumnHeaderTriangles();
    }

    /// <summary>
    /// Обработка сигнала Idle
    /// </summary>
    public override void HandleIdle()
    {
      base.HandleIdle();

      if (_CurrentCellChangedFlag)
      {
        // обязательно до вызова CurrentCellChangedFlag.
        // OnCurrentCellChanged() может установить флаг еще раз
        _CurrentCellChangedFlag = false;
        OnCurrentCellChanged();
      }

      if (_TopLeftCellToolTipTextUpdateFlag)
      {
        // Обновляем подсказку по числу строк
        _TopLeftCellToolTipTextUpdateFlag = false;
        TopLeftCellToolTipText = _TopLeftCellToolTipText;
      }
    }

    /// <summary>
    /// Вызывается при изменении текущей выбранной ячейки.
    /// Вызывает обновление команд локального меню.
    /// Предотвращается вложенный вызов метода
    /// </summary>
    protected virtual void OnCurrentCellChanged()
    {
      if (Control.Visible &&
        (!_Inside_Control_DataBindingComplete) &&
        Control.CurrentCellAddress.X >= 0 &&
        //_VisibleHasChanged && 
        _MouseOrKeyboardFlag)

        _CurrentColumnIndex = Control.CurrentCellAddress.X;

      // При смене текущего столбца отключаем поиск по первым буквам
      if (CurrentIncSearchColumn != null)
      {
        if (CurrentIncSearchColumn.GridColumn.Index != Control.CurrentCellAddress.X)
          CurrentIncSearchColumn = null;
      }

      // Обработка перемещений для установки видимости команд локального меню
      CommandItems.PerformRefreshItems();
    }

    #endregion

    #region VisibleChanged

    ///// <summary>
    ///// Только после установки этого флага разрешается реагировать на смену ячейки
    ///// </summary>
    //public bool VisibleHasChanged { get { return _VisibleHasChanged; } }
    //private bool _VisibleHasChanged;

    /// <summary>
    /// Этот флажок устанавливается в true, когда нажата мышь иди клавиша, что
    /// означает, что просмотр есть на экране, и смена текущей ячейки выаолнена
    /// пользователем
    /// </summary>
    private bool _MouseOrKeyboardFlag;

    internal void Control_VisibleChanged(object sender, EventArgs args)
    {
      try
      {
        if (Control.Visible)
        {
          CurrentColumnIndex = CurrentColumnIndex;

          if (Control.CurrentCell != null && Control.Height > 0 && Control.Width > 0)
          {
            if (!Control.CurrentCell.Displayed)
            {
              try
              {
                Control.FirstDisplayedCell = Control.CurrentCell;
              }
              catch
              {
                // Может быть InvalidOpertationException, если строки не помещаются
              }
            }
          }

          //CommandItems.RefreshStatItems();
        }
        //_VisibleHasChanged = Control.Visible;
        _MouseOrKeyboardFlag = false;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Control_VisibleChanged");
      }
    }

    #endregion

    #region DataBindingChanged

    private void Control_DataBindingChanged(object sender, EventArgs args)
    {
      try
      {
        _SourceAsDataView = GetSourceAsDataView(Control.DataSource, Control.DataMember); // 04.07.2021
        OnDataBindingChanged();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка вызова OnDataBindingChanged()");
      }
    }

    /// <summary>
    /// Вызывается после изменения свойств DataSource или DataMember.
    /// Если требуется выполнить действия после того, как инициализировано количество строк в просмотре,
    /// используйте OnDataBindingComplete.
    /// 
    /// Если метод переопределен в классе-наследнике, обязательно должен быть вызван метод базового класса
    /// </summary>
    protected virtual void OnDataBindingChanged()
    {
      // 04.07.2021
      // Делаем сортировку здесь, а не в OnDataBindingComplete(), чтобы избежать лишних действий
      if (SourceAsDataView != null)
      {
        ValidateCurrentOrder();
        if (AutoSort)
          PerformAutoSort();
      }

      InitColumnSortMode();
    }

    #endregion

    #region DataBindingComplete

    /// <summary>
    /// Событие DataBindingComplete может вызызываться вложенно из PerformAutoSort()
    /// </summary>
    private bool _Inside_Control_DataBindingComplete = false;

    void Control_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs args)
    {
      _TopLeftCellToolTipTextUpdateFlag = true; // число строк могло поменяться

      if (args.ListChangedType != ListChangedType.Reset)
        return;

      if (_Inside_Control_DataBindingComplete)
        return;

      try
      {
        _Inside_Control_DataBindingComplete = true;
        try
        {
          OnDataBindingComplete(args);
        }
        finally
        {
          _Inside_Control_DataBindingComplete = false;
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Control_DataBindingComplete");
      }
    }

    /// <summary>
    /// Вызывается при получении события DataGridView.DataBindingComplete.
    /// Реентрантные события не вызывают метод.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnDataBindingComplete(DataGridViewBindingCompleteEventArgs args)
    {
      CurrentRowIndex = _SavedRowIndex;
      if (args.ListChangedType == ListChangedType.Reset)
        CurrentColumnIndex = CurrentColumnIndex;

      _CurrentCellChangedFlag = true;

      // После присоединения источника часто теряются треугольнички
      // в заголовках строк
      // 17.07.2021. Подтверждено.
      // Если используется произвольная сортировка по двум столбцам, то первый треугольник рисуется, а второй - нет
      if (args.ListChangedType == ListChangedType.Reset)
        InitColumnHeaderTriangles();
    }

    #endregion

    #region KeyDown

    void Control_KeyDown(object sender, KeyEventArgs args)
    {
      try
      {
        _MouseOrKeyboardFlag = true;
        DoControl_KeyDown2(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработки нажатия клавиши KeyDown");
      }
    }

    #endregion

    #region MouseDown

    /// <summary>
    /// При нажатии правой кнопки мыши на невыделенной ячейке надо поместить туда
    /// выделение, а затем показывать локальное меню
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void Control_MouseDown(object sender, MouseEventArgs args)
    {
      try
      {
        DoControl_MouseDown(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Control_MouseDown");
      }
    }
    void DoControl_MouseDown(MouseEventArgs args)
    {
      _MouseOrKeyboardFlag = true;
      if (args.Button == MouseButtons.Right)
      {
        if (Control.AreAllCellsSelected(false))
          return;

        DataGridView.HitTestInfo ht = Control.HitTest(args.X, args.Y);
        switch (ht.Type)
        {
          case DataGridViewHitTestType.Cell:
            DataGridViewCell Cell = Control.Rows[ht.RowIndex].Cells[ht.ColumnIndex];
            if (Cell.Selected)
              return;
            Control.ClearSelection();
            Cell.Selected = true;
            Control.CurrentCell = Cell;
            break;
          case DataGridViewHitTestType.RowHeader:
            DataGridViewRow Row = Control.Rows[ht.RowIndex];
            foreach (DataGridViewCell SelCell in Control.SelectedCells)
            {
              if (SelCell.RowIndex == ht.RowIndex)
                return;
            }
            Control.ClearSelection();
            Row.Selected = true;
            int ColIdx = Control.CurrentCellAddress.X;
            if (ColIdx < 0)
              ColIdx = Control.FirstDisplayedScrollingColumnIndex;
            if (ColIdx >= 0)
              Control.CurrentCell = Row.Cells[ColIdx];
            break;
          case DataGridViewHitTestType.ColumnHeader:
            DataGridViewColumn Column = Control.Columns[ht.ColumnIndex];
            foreach (DataGridViewCell SelCell in Control.SelectedCells)
            {
              if (SelCell.ColumnIndex == ht.ColumnIndex)
                return;
            }
            Control.ClearSelection();
            Column.Selected = true;
            int RowIdx = Control.CurrentCellAddress.Y;
            if (RowIdx < 0)
              RowIdx = Control.FirstDisplayedScrollingRowIndex;
            if (RowIdx >= 0)
              Control.CurrentCell = Control.Rows[RowIdx].Cells[ht.ColumnIndex];
            break;
          case DataGridViewHitTestType.TopLeftHeader:
            if (Control.MultiSelect)
              Control.SelectAll();
            break;
        }
      }
    }

    #endregion

    #endregion

    #region Поиск текста

    /// <summary>
    /// Контекст поиска по Ctrl-F / F3.
    /// Свойство возвращает null, если TextSearchEnabled=false
    /// </summary>
    public IEFPTextSearchContext TextSearchContext
    {
      get
      {
        if (_TextSearchEnabled)
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
    /// Создает объект EFPDataGridViewSearchContext.
    /// Переопределенный метод может создать расширенный объект для поиска текста.
    /// </summary>
    /// <returns></returns>
    protected virtual IEFPTextSearchContext CreateTextSearchContext()
    {
      return new EFPDataGridViewSearchContext(this);
    }

    /// <summary>
    /// Если true (по умолчанию), то доступна команда "Найти" (Ctrl-F).
    /// Если false, то свойство TextSearchContext возвращает null и поиск недоступен.
    /// Свойство можно устанавливать только до вывода просмотра на экран
    /// </summary>
    public bool TextSearchEnabled
    {
      get { return _TextSearchEnabled; }
      set
      {
        CheckHasNotBeenCreated();
        _TextSearchEnabled = value;
      }
    }
    private bool _TextSearchEnabled;

    #endregion

    #region GridProducer


    /// <summary>
    /// Генератор столбцов таблицы. Может устанавливаться только до показа табличного просмотра
    /// На уровне EFPDataGridView используется при установке свойства CurrentConfig для инициализации
    /// табличного просмотра.
    /// </summary>
    public IEFPGridProducer GridProducer
    {
      get { return _GridProducer; }
      set
      {
        if (value != null)
          value.SetReadOnly();
        CheckHasNotBeenCreated();
        Control.AutoGenerateColumns = false; // 10.03.2016
        _GridProducer = value;
      }
    }
    private IEFPGridProducer _GridProducer;

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то используется порядок сортировки строк, заданный в 
    /// GridProducer.
    /// Если используется особая реализация просмотра, то можно отключить инициализацию порядка сортировки
    /// строк, установив значение false. Установка допускается только до вывода просмотра на экран.
    /// Как правило, при установке значения в false, следует также устанавливать CustomOrderAllowed=false.
    /// </summary>
    public bool UseGridProducerOrders
    {
      get { return _UseGridProducerOrders; }
      set
      {
        CheckHasNotBeenCreated();
        _UseGridProducerOrders = value;
      }
    }
    private bool _UseGridProducerOrders;


    #endregion

    #region Другие свойства

    /// <summary>
    /// Текущее состояние просмотра.
    /// Используется в событии EditData.
    /// Свойство кратковременно устанавливается во время наступления события редактирования.
    /// В остальное время имеет значение View.
    /// </summary>
    public EFPDataGridViewState State { get { return _State; } }
    private EFPDataGridViewState _State;

    /// <summary>
    /// Размеры и масштабные коэффициенты для табличного просмотра
    /// </summary>
    public EFPDataGridViewMeasures Measures
    {
      get
      {
        if (_Measures == null)
          _Measures = new EFPDataGridViewMeasures(this);
        return _Measures;
      }
    }
    private EFPDataGridViewMeasures _Measures;

    IEFPGridControlMeasures IEFPGridControl.Measures { get { return Measures; } }

    /// <summary>
    /// Установка / получения высоты всех строк просмотра в единицах количества
    /// строк текста, которые можно разместить в одной строке таблицы
    /// </summary>
    public int TextRowHeight
    {
      get
      {
        return Measures.GetTextRowHeight(Control.RowTemplate.Height);
      }
      set
      {
        int H = Measures.SetTextRowHeight(value);
        Control.RowTemplate.Height = H;
      }
    }

    #endregion

    #region Управление поведением просмотра

    #region ReadOnly

    /// <summary>
    /// true, если поддерживается только просмотр данных, а не редактирование.
    /// Установка свойства отключает видимость всех команд редактирования. 
    /// Свойства CanInsert, CanDelete и CanInsertCopy перестают действовать
    /// Значение по умолчанию: false (редактирование разрешено)
    /// Не влияет на возможность inline-редактирования. Возможно любое сочетание
    /// свойств ReadOnly и Control.ReadOnly
    /// </summary>
    [DefaultValue(false)]
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

        if (CommandItems != null)
          CommandItems.PerformRefreshItems();
      }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Управляемое свойство для ReadOnly.
    /// </summary>
    public DepValue<bool> ReadOnlyEx
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
    private DepInput<bool> _ReadOnlyEx;

    private void InitReadOnlyEx()
    {
      if (_ReadOnlyEx == null)
      {
        _ReadOnlyEx = new DepInput<bool>(ReadOnly, ReadOnlyEx_ValueChanged);
        _ReadOnlyEx.OwnerInfo = new DepOwnerInfo(this, "ReadOnlyEx");
      }
    }

    void ReadOnlyEx_ValueChanged(object Sender, EventArgs Args)
    {
      ReadOnly = _ReadOnlyEx.Value;
    }

    #endregion

    #region CanInsert

    /// <summary>
    /// true, если можно добаввлять строки (при DataReadOnly=false)
    /// Значение по умолчанию: true (добавление строк разрешено)
    /// </summary>
    [DefaultValue(true)]
    public bool CanInsert
    {
      get { return _CanInsert; }
      set
      {
        if (value == _CanInsert)
          return;
        _CanInsert = value;
        if (_CanInsertEx != null)
          _CanInsertEx.Value = value;
        if (CommandItems != null)
          CommandItems.PerformRefreshItems();
      }
    }
    private bool _CanInsert;

    /// <summary>
    /// Управляемое свойство для CanInsert
    /// </summary>
    public DepValue<bool> CanInsertEx
    {
      get
      {
        InitCanInsertEx();
        return _CanInsertEx;
      }
      set
      {
        InitCanInsertEx();
        _CanInsertEx.Source = value;
      }
    }
    private DepInput<bool> _CanInsertEx;

    private void InitCanInsertEx()
    {
      if (_CanInsertEx == null)
      {
        _CanInsertEx = new DepInput<bool>(CanInsert, CanInsertEx_ValueChanged);
        _CanInsertEx.OwnerInfo = new DepOwnerInfo(this, "CanInsertEx");
      }
    }

    void CanInsertEx_ValueChanged(object Sender, EventArgs Args)
    {
      CanInsert = _CanInsertEx.Value;
    }

    #endregion

    #region CanInsertCopy

    /// <summary>
    /// true, если разрешено добавлять строку по образцу существующей
    /// (при DataReadOnly=false и CanInsert=true)
    /// Значение по умолчанию: false (копирование запрещено)
    /// </summary>
    [DefaultValue(false)]
    public bool CanInsertCopy
    {
      get { return _CanInsertCopy; }
      set
      {
        if (value == _CanInsertCopy)
          return;
        _CanInsertCopy = value;
        if (_CanInsertCopyEx != null)
          _CanInsertCopyEx.Value = value;
        if (CommandItems != null)
          CommandItems.PerformRefreshItems();
      }
    }
    private bool _CanInsertCopy;

    /// <summary>
    /// Управляемое свойство для Checked.
    /// </summary>
    public DepValue<bool> CanInsertCopyEx
    {
      get
      {
        InitCanInsertCopyEx();
        return _CanInsertCopyEx;
      }
      set
      {
        InitCanInsertCopyEx();
        _CanInsertCopyEx.Source = value;
      }
    }
    private DepInput<bool> _CanInsertCopyEx;

    private void InitCanInsertCopyEx()
    {
      if (_CanInsertCopyEx == null)
      {
        _CanInsertCopyEx = new DepInput<bool>(CanInsertCopy, CanInsertCopyEx_ValueChanged);
        _CanInsertCopyEx.OwnerInfo = new DepOwnerInfo(this, "CanInsertCopyEx");
      }
    }

    void CanInsertCopyEx_ValueChanged(object Sender, EventArgs Args)
    {
      CanInsertCopy = _CanInsertCopyEx.Value;
    }

    #endregion

    #region CanDelete

    /// <summary>
    /// true, если можно удалять строки (при DataReadOnly=false)
    /// Значение по умолчанию: true (удаление разрешено)
    /// </summary>
    [DefaultValue(true)]
    public bool CanDelete
    {
      get { return _CanDelete; }
      set
      {
        if (value == _CanDelete)
          return;
        _CanDelete = value;
        if (_CanDeleteEx != null)
          _CanDeleteEx.Value = value;
        if (CommandItems != null)
          CommandItems.PerformRefreshItems();
      }
    }
    private bool _CanDelete;

    /// <summary>
    /// Управляемое свойство для CanDelete.
    /// </summary>
    public DepValue<bool> CanDeleteEx
    {
      get
      {
        InitCanDeleteEx();
        return _CanDeleteEx;
      }
      set
      {
        InitCanDeleteEx();
        _CanDeleteEx.Source = value;
      }
    }
    private DepInput<bool> _CanDeleteEx;

    private void InitCanDeleteEx()
    {
      if (_CanDeleteEx == null)
      {
        _CanDeleteEx = new DepInput<bool>(CanDelete, CanDeleteEx_ValueChanged);
        _CanDeleteEx.OwnerInfo = new DepOwnerInfo(this, "CanDeleteEx");
      }
    }

    void CanDeleteEx_ValueChanged(object Sender, EventArgs Args)
    {
      CanDelete = _CanDeleteEx.Value;
    }

    #endregion

    #region CanView

    /// <summary>
    /// true, если можно просмотреть выбранные строки в отдельном окне
    /// По умолчанию: true
    /// </summary>
    [DefaultValue(true)]
    public bool CanView
    {
      get { return _CanView; }
      set
      {
        if (value == _CanView)
          return;
        _CanView = value;
        if (_CanViewEx != null)
          _CanViewEx.Value = value;
        if (CommandItems != null)
          CommandItems.PerformRefreshItems();
      }
    }
    private bool _CanView;

    /// <summary>
    /// Управляемое свойство для CanView.
    /// </summary>
    public DepValue<bool> CanViewEx
    {
      get
      {
        InitCanViewEx();
        return _CanViewEx;
      }
      set
      {
        InitCanViewEx();
        _CanViewEx.Source = value;
      }
    }
    private DepInput<bool> _CanViewEx;

    private void InitCanViewEx()
    {
      if (_CanViewEx == null)
      {
        _CanViewEx = new DepInput<bool>(CanView, CanViewEx_ValueChanged);
        _CanViewEx.OwnerInfo = new DepOwnerInfo(this, "CanViewEx");
      }
    }

    void CanViewEx_ValueChanged(object Sender, EventArgs Args)
    {
      CanView = _CanViewEx.Value;
    }

    #endregion

    /// <summary>
    /// true, если разрешено редактирование и просмотр одновременно 
    /// нескольких выбранных строк
    /// По умолчанию - false
    /// </summary>
    [DefaultValue(false)]
    public bool CanMultiEdit
    {
      get
      {
        return _CanMultiEdit;
      }
      set
      {
        _CanMultiEdit = value;
        if (value)
          Control.MultiSelect = true;
      }
    }
    private bool _CanMultiEdit;

    /// <summary>
    /// Блокировка сортировки для всех столбцов
    /// Для столбцов, определенных в Orders, сортировка остается разрешенной
    /// </summary>
    public void DisableOrdering()
    {
      for (int i = 0; i < Control.Columns.Count; i++)
        Control.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;

      // 03.06.2011
      // Для столбцов, установленных в Orders, разрешаем сортировку
      if (OrderCount > 0)
      {
        for (int i = 0; i < Orders.Count; i++)
        {
          int ColumnIndex = IndexOfUsedColumnName(Orders[i]);
          if (ColumnIndex >= 0)
            Control.Columns[ColumnIndex].SortMode = DataGridViewColumnSortMode.Programmatic;
        }
      }
    }

#if XXX
    // бессмыслено
    /// <summary>
    /// Возвращает свойство DataGridView.ReadOnly.
    /// При установке свойства, в отличие от оригинального, сохраняются существующие
    /// значения свойства "ReadOnly" для всех столбцов
    /// </summary>
    public bool ControlReadOnly
    {
      get { return Control.ReadOnly; }
      set
      {
        if (value == Control.ReadOnly)
          return;

        bool[] Flags = new bool[Control.ColumnCount];
        for (int i = 0; i < Control.ColumnCount; i++)
          Flags[i] = Control.Columns[i].ReadOnly;

        Control.ReadOnly = value;

        for (int i = 0; i < Control.ColumnCount; i++)
          Control.Columns[i].ReadOnly = Flags[i];
      }
    }
#endif

    /// <summary>
    /// Установить свойство DataReadOnly для всех столбцов просмотра.
    /// Метод используется, когда требуется разрешить редактирование "по месту" 
    /// только для некоторых столбцов, а большая часть столбцов не редактируется
    /// Метод должен вызываться после установки общих свойств DataReadOnly табличного 
    /// просмотра, иначе признак у столбцов может быть изменен неявно
    /// </summary>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetColumnsReadOnly(bool value)
    {
      for (int i = 0; i < Control.Columns.Count; i++)
        Control.Columns[i].ReadOnly = value;
    }

    /// <summary>
    /// Установить свойство DataReadOnly для заданных столбцов просмотра.
    /// Метод используется, когда требуется разрешить редактирование "по месту" 
    /// только для некоторых столбцов, а большая часть столбцов не редактируется
    /// Метод должен вызываться после установки общих свойств DataReadOnly табличного 
    /// просмотра, иначе признак у столбцов может быть изменен неявно
    /// </summary>
    /// <param name="columnNames">Список имен столбцов через запятую. Если столбец с заданным именем не найден, то ошибка не выдается</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetColumnsReadOnly(string columnNames, bool value)
    {
      string[] a = columnNames.Split(',');
      for (int i = 0; i < a.Length; i++)
      {
        EFPDataGridViewColumn Col = Columns[a[i]];
        if (Col != null)
          Col.GridColumn.ReadOnly = value;
      }
    }

    void Control_ReadOnlyChanged(object sender, EventArgs args)
    {
      if (CommandItems != null)
        CommandItems.PerformRefreshItems();
    }


    #endregion

    #region Список столбцов

    /// <summary>
    /// Дополнительная информация о столбцах, альтернативные методы добавления
    /// столбцов
    /// </summary>
    public EFPDataGridViewColumns Columns
    {
      get
      {
        if (_Columns == null)
          _Columns = CreateColumns();
        return _Columns;
      }
    }
    private EFPDataGridViewColumns _Columns;

    /// <summary>
    /// Создает EFPDataGridViewColumns.
    /// </summary>
    /// <returns>Новый объект</returns>
    protected virtual EFPDataGridViewColumns CreateColumns()
    {
      return new EFPDataGridViewColumns(this);
    }

    EFPDataViewColumnInfo[] IEFPGridControl.GetVisibleColumnsInfo()
    {
      EFPDataGridViewColumn[] RealColumns = VisibleColumns;
      EFPDataViewColumnInfo[] a = new EFPDataViewColumnInfo[RealColumns.Length];
      for (int i = 0; i < RealColumns.Length; i++)
        a[i] = new EFPDataViewColumnInfo(RealColumns[i].Name, RealColumns[i].GridColumn.DataPropertyName, RealColumns[i].ColumnProducer);
      return a;
    }

    bool IEFPGridControl.InitColumnConfig(EFPDataGridViewConfigColumn configColumn)
    {
      EFPDataGridViewColumn Col = Columns[configColumn.ColumnName];
      if (Col == null)
        return false;

      DataGridViewColumn GridCol = Col.GridColumn;
      configColumn.Width = GridCol.Width;
      configColumn.FillMode = GridCol.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill;
      configColumn.FillWeight = (int)(GridCol.FillWeight);
      return true;
    }

    #endregion

    #region Доступ к выбранным ячейкам независимо от типа данных

    /// <summary>
    /// Сколько строк выбрано сейчас в просмотре (одна, много или ни одной)
    /// Для точного определения используйте свойство SelectedRows.
    /// В отличие от SelectedRows метод вычисляется всегда быстро
    /// </summary>
    public EFPDataGridViewSelectedRowsState SelectedRowsState
    {
      get
      {
        if ((Control.SelectionMode == DataGridViewSelectionMode.FullRowSelect) ||
           Control.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect)
        {
          if (Control.SelectedRows.Count > 0)
          {
            return Control.SelectedRows.Count > 1 ? EFPDataGridViewSelectedRowsState.MultiRows :
              EFPDataGridViewSelectedRowsState.SingleRow;
          }
          // Иначе придется определять с помощью SelectedCells
          if (Control.SelectedCells.Count > 0)
          {
            int FirstIndex = Control.SelectedCells[0].RowIndex;
            for (int i = 1; i < Control.SelectedCells.Count; i++)
            {
              if (Control.SelectedCells[i].RowIndex != FirstIndex)
                return EFPDataGridViewSelectedRowsState.MultiRows;
            }
            return EFPDataGridViewSelectedRowsState.SingleRow;
          }
        }
        if (Control.CurrentRow == null)
          return EFPDataGridViewSelectedRowsState.NoSelection;

        return EFPDataGridViewSelectedRowsState.SingleRow;
      }
    }

    /// <summary>
    /// Возвращает true, если в просмотре есть хотя бы одна выбранная ячейка
    /// </summary>
    public bool HasSelectedRows
    {
      get
      {
        return Control.SelectedRows.Count > 0 || Control.SelectedCells.Count > 0 || Control.CurrentRow != null;
      }
    }

    /// <summary>
    /// Возвращает true, если в просмотре выбраны целые строки
    /// </summary>
    public bool WholeRowsSelected
    {
      get
      {
        if (Control.AreAllCellsSelected(false))
          return true;
        return Control.SelectedRows != null && Control.SelectedRows.Count > 0;
      }
    }

    /// <summary>
    /// Внутренний класс для реализации сравнения
    /// </summary>
    private class RCComparer : IComparer<DataGridViewRow>, IComparer<DataGridViewColumn>
    {
      #region IComparer<DataGridViewRow> Members

      /// <summary>
      /// Сравнение двух строк для сортировки
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public int Compare(DataGridViewRow x, DataGridViewRow y)
      {
        if (x == null)
        {
          if (y == null)
            return 0;
          else
            return -1;
        }
        if (y == null)
          return 1;

        if (x.Index == y.Index)
          return 0;
        if (x.Index > y.Index)
          return 1;
        else
          return -1;
      }

      #endregion

      #region IComparer<DataGridViewColumn> Members

      /// <summary>
      /// Сравнение двух столбцов для сортировки в порядке их отображения на экране
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public int Compare(DataGridViewColumn x, DataGridViewColumn y)
      {
        if (x == null)
        {
          if (y == null)
            return 0;
          else
            return -1;
        }
        if (y == null)
          return 1;

        if (x.DisplayIndex == y.DisplayIndex)
        {
          if (x.Index == y.Index)
            return 0;
          if (x.Index > y.Index)
            return 1;
          else
            return -1;
        }
        if (x.DisplayIndex > y.DisplayIndex)
          return 1;
        else
          return -1;
      }

      #endregion

      #region Методы сортировки

      /// <summary>
      /// Сортировка массива строк в соответствии с их порядком на экране
      /// </summary>
      /// <param name="rows"></param>
      public void SortGridRows(DataGridViewRow[] rows)
      {
        Array.Sort<DataGridViewRow>(rows, this);
      }

      #endregion
    }

    /// <summary>
    /// Реализация сортировки строк и столбцов
    /// </summary>
    private static readonly RCComparer _TheRCComparer = new RCComparer();

    /// <summary>
    /// Получить или установить выделенные строки таблицы. В режиме выделения
    /// ячейки, а не целой строки, возвращается массив из одного элемента
    /// </summary>
    public DataGridViewRow[] SelectedGridRows
    {
      get
      {
        return GetSelectedGridRows(Control);
      }
      set
      {
        if (value == null)
          return; // 27.12.2020
        if (value.Length == 0)
          return;
        bool FirstRowFlag = true;
        switch (Control.SelectionMode)
        {
          case DataGridViewSelectionMode.FullRowSelect:
            // Режим выбора только целых строк
            Control.ClearSelection();
            for (int i = 0; i < value.Length; i++)
            {
              //if (value != null)
              if (value[i] != null) // 27.12.2020
              {
                if (FirstRowFlag)
                {
                  CurrentGridRow = value[i];
                  FirstRowFlag = false;
                }
                value[i].Selected = true;
              }
            }
            break;
          case DataGridViewSelectionMode.RowHeaderSelect:
            // В этом режиме можно либо выбирать целые строки, либо ячейки
            if (Control.SelectedRows.Count > 0)
            {
              // До этого были выбраны строки, делаем также
              Control.ClearSelection();
              for (int i = 0; i < value.Length; i++)
              {
                if (value[i] != null)
                {
                  if (FirstRowFlag)
                  {
                    CurrentGridRow = value[i];
                    FirstRowFlag = false;
                  }
                  value[i].Selected = true;
                }
              }
            }
            else
            {
              // До этого была выбрана одна ячейка. Выбираем строки, если их 
              // несколько или выбираем ячейку, если она одна
              if (value.Length > 1)
              {
                for (int i = 0; i < value.Length; i++)
                {
                  if (FirstRowFlag)
                  {
                    CurrentGridRow = value[i];
                    FirstRowFlag = false;
                  }
                  if (value[i] != null)
                    value[i].Selected = true;
                }
              }
              else
              {
                if (value[0] != null)
                  CurrentGridRow = value[0];
              }
            }
            break;
          default:
            // Просмотр не в режиме выбора строк. Очищаем выбор и делаем текущей
            // первую строку из набора
            Control.ClearSelection();
            if (value[0] != null)
              CurrentGridRow = value[0];
            break;
        }

        _CurrentCellChangedFlag = true;
      }
    }

    private static DataGridViewRow[] GetSelectedGridRows(DataGridView control)
    {
      DataGridViewRow[] res;

      if ((control.SelectionMode == DataGridViewSelectionMode.FullRowSelect) ||
           control.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect)
      {
        if (control.SelectedRows.Count > 0)
        {
          res = new DataGridViewRow[control.SelectedRows.Count];
          control.SelectedRows.CopyTo(res, 0);
          _TheRCComparer.SortGridRows(res);
          return res;
        }
        if (control.SelectedCells.Count > 0)
        {
          // Собираем строки для выбранных ячеек
          List<DataGridViewRow> Rows = new List<DataGridViewRow>();
          foreach (DataGridViewCell Cell in control.SelectedCells)
          {
            DataGridViewRow ThisRow = Cell.OwningRow;
            if (!Rows.Contains(ThisRow))
              Rows.Add(ThisRow);
          }
          res = Rows.ToArray();
          _TheRCComparer.SortGridRows(res);
          return res;
        }
      }
      if (control.CurrentRow == null)
        return new DataGridViewRow[0];

      res = new DataGridViewRow[1];
      res[0] = control.CurrentRow;
      return res;
    }

    /// <summary>
    /// Вспомогательное свойство. Возвращает число выбранных строк в просмотре.
    /// Оптимальнее, чем вызов SelectedRows.Length
    /// </summary>
    public int SelectedRowCount
    {
      get
      {
        if (Control.AreAllCellsSelected(true))
          return Control.RowCount;
        else
        {
          if ((Control.SelectionMode == DataGridViewSelectionMode.FullRowSelect) ||
          Control.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect)
          {
            if (Control.SelectedRows.Count > 0)
              return Control.SelectedRows.Count;
            if (Control.SelectedCells.Count > 0)
            {
              // Собираем строки для выбранных ячеек
              List<DataGridViewRow> Rows = new List<DataGridViewRow>();
              foreach (DataGridViewCell Cell in Control.SelectedCells)
              {
                DataGridViewRow ThisRow = Cell.OwningRow;
                if (!Rows.Contains(ThisRow))
                  Rows.Add(ThisRow);
              }
              return Rows.Count;
            }
          }
          if (Control.CurrentRow == null)
            return 0;

          return 1;
        }
      }
    }



    /// <summary>
    /// Оригинальное свойство DataGridView.CurrentRow не позволяет
    /// установить позицию.
    /// </summary>
    public DataGridViewRow CurrentGridRow
    {
      get
      {
        return Control.CurrentRow;
      }
      set
      {
        if (value == null)
          return;

        int ColIdx = CurrentColumnIndex;

        if (ColIdx < 0 || ColIdx >= Control.Columns.Count /* 21.11.2018 */)
          ColIdx = 0;
        Control.CurrentCell = value.Cells[ColIdx];
      }
    }

    /// <summary>
    /// Эквивалентно установке свойства CurrentGridRow, но в просмотре выделяется,
    /// если возможно, строка целиком
    /// </summary>
    /// <param name="row"></param>
    void SelectGridRow(DataGridViewRow row)
    {
      if (row == null)
        return;

      // Установка ячейки
      CurrentGridRow = row;

      if ((Control.SelectionMode == DataGridViewSelectionMode.FullRowSelect) ||
         Control.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect)
      {
        // Выделение целой строки
        Control.ClearSelection();
        row.Selected = true;
      }
    }

    /// <summary>
    /// Расширение свойства SelectedRows. Вместо объекта DataGridViewRow 
    /// используются индексы строк
    /// </summary>
    public int[] SelectedRowIndices
    {
      get
      {
        return GetSelectedRowIndices(Control);
      }
      set
      {
        DataGridViewRow[] Rows = new DataGridViewRow[value.Length];
        for (int i = 0; i < value.Length; i++)
        {
          if (value[i] >= 0 && value[i] < Control.Rows.Count)
            Rows[i] = Control.Rows[value[i]];
        }
        SelectedGridRows = Rows;
      }
    }

    private static int[] GetSelectedRowIndices(DataGridView control)
    {
      DataGridViewRow[] Rows = GetSelectedGridRows(control);
      int[] res = new int[Rows.Length];
      for (int i = 0; i < Rows.Length; i++)
      {
        if (Rows[i] == null)
          res[i] = -1;
        else
          res[i] = Rows[i].Index;
      }
      return res;
    }

    /// <summary>
    /// Расширение свойства CurrentRow. Вместо объекта DataGridViewRow 
    /// используется индекс строки
    /// </summary>
    public int CurrentRowIndex
    {
      get
      {
        DataGridViewRow Row = CurrentGridRow;
        if (Row == null)
          return -1;
        else
          return Row.Index;
      }
      set
      {
        if (Control.Rows.Count == 0)
        {
          _SavedRowIndex = value;
          return;
        }
        if (value >= 0 && value < Control.Rows.Count)
          CurrentGridRow = Control.Rows[value];
        _SavedRowIndex = -1;
      }
    }

    private int _SavedRowIndex = -1;


    /// <summary>
    /// Выделяет строку в просмотре с заданыым индексом.
    /// Используйте установку свойства CurrentRowIndex
    /// </summary>
    /// <param name="rowIndex"></param>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void SelectRowIndex(int rowIndex)
    {
      if (Control.Rows.Count == 0)
      {
        _SavedRowIndex = rowIndex;
        return;
      }
      if (rowIndex >= 0 && rowIndex < Control.Rows.Count)
        SelectGridRow(Control.Rows[rowIndex]);
      _SavedRowIndex = -1;
    }

    /// <summary>
    /// Получить выделенные столбцы таблицы. В режиме выделения
    /// ячейки возвращается массив из одного элемента. В режиме выбора строк
    /// возвращается массив всех видимых столбцов
    /// </summary>
    public DataGridViewColumn[] SelectedGridColumns
    {
      get
      {
        return GetSelectedGridColumns(Control);
      }
    }

    private static DataGridViewColumn[] GetSelectedGridColumns(DataGridView control)
    {
      if (control.AreAllCellsSelected(false))
        return WinFormsTools.GetOrderedVisibleColumns(control);

      if (control.SelectedColumns != null && control.SelectedColumns.Count > 0)
      {
        // Есть выбранные целые столбцы
        DataGridViewColumn[] res = new DataGridViewColumn[control.SelectedColumns.Count];
        for (int i = 0; i < control.SelectedColumns.Count; i++)
          res[i] = control.SelectedColumns[i];
        return res;
      }

      if (control.SelectedCells != null && control.SelectedCells.Count > 0)
      {
        // Есть отдельные выбранные ячейки
        List<DataGridViewColumn> res = new List<DataGridViewColumn>();
        foreach (DataGridViewCell Cell in control.SelectedCells)
        {
          DataGridViewColumn Col = control.Columns[Cell.ColumnIndex];
          if (res.IndexOf(Col) < 0)
          {
            res.Add(Col);
            if (res.Count == control.ColumnCount)
              break; // найдены все столбцы
          }
        }
        res.Sort(_TheRCComparer);
        return res.ToArray();
      }

      if (control.SelectedRows != null && control.SelectedRows.Count > 0)
      {
        // Есть целиком выбранные строки - возвращаем все видимые столбцы
        return WinFormsTools.GetOrderedVisibleColumns(control);
      }

      if (control.CurrentCellAddress.X >= 0)
        // Есть только выделенная ячейка
        return new DataGridViewColumn[1] { control.Columns[control.CurrentCellAddress.X] };

      // Нет выделенной ячейки
      return new DataGridViewColumn[0];
    }

    /// <summary>
    /// Возвращает массив индексов выделенных столбов просмотра.
    /// </summary>
    public int[] SelectedColumnIndices
    {
      get
      {
        return GetSelectedColumnIndices(Control);
      }
    }

    private static int[] GetSelectedColumnIndices(DataGridView control)
    {
      DataGridViewColumn[] Cols = GetSelectedGridColumns(control);
      int[] indices = new int[Cols.Length];
      for (int i = 0; i < Cols.Length; i++)
        indices[i] = Cols[i].Index;
      return indices;
    }

    /// <summary>
    /// Получить выделенные столбцы таблицы. В режиме выделения
    /// ячейки возвращается массив из одного элемента. В режиме выбора строк
    /// возвращается массив всех видимых столбцов
    /// </summary>
    public EFPDataGridViewColumn[] SelectedColumns
    {
      get
      {
        DataGridViewColumn[] Cols = SelectedGridColumns;
        EFPDataGridViewColumn[] Cols2 = new EFPDataGridViewColumn[Cols.Length];
        for (int i = 0; i < Cols.Length; i++)
          Cols2[i] = Columns[Cols[i]];
        return Cols2;
      }
    }

    /// <summary>
    /// Возвращает true, если ячейка является выбранной, находится в выбранной 
    /// целиком строке или столбце. 
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <returns>true, если ячейка является текущей, выбранной индивидуально или в составе выделенной строки или столбца</returns>
    public bool IsCellSelected(int rowIndex, int columnIndex)
    {
      if (rowIndex < 0 || rowIndex >= Control.RowCount)
        return false;
      if (columnIndex < 0 || columnIndex >= Control.Columns.Count)
        return false;

      if (!Control.Rows.SharedRow(rowIndex).Visible)
        return false;
      if (!Control.Columns[columnIndex].Visible)
        return false;

      if (Control.AreAllCellsSelected(false))
        return true;
      if (Control.SelectedCells != null && Control.SelectedCells.Count > 0)
      {
        DataGridViewCell Cell = Control[columnIndex, rowIndex];
        return Control.SelectedCells.Contains(Cell);
      }
      if (Control.SelectedRows != null && Control.SelectedRows.Count > 0)
      {
        DataGridViewRow Row = Control.Rows[rowIndex];
        return Control.SelectedRows.Contains(Row);
      }
      if (Control.SelectedColumns != null && Control.SelectedColumns.Count > 0)
      {
        DataGridViewColumn Col = Control.Columns[columnIndex];
        return Control.SelectedColumns.Contains(Col);
      }

      return Control.CurrentCellAddress.X == columnIndex && Control.CurrentCellAddress.Y == rowIndex;
    }

    /// <summary>
    /// Вспомогательное свойство только для чтения. Возвращает true, если свойство
    /// CurrentRow установлено (в просмотре есть текущая строка) и она является
    /// единственной выбранной строкой
    /// </summary>
    public bool IsCurrentRowSingleSelected
    {
      get
      {
        DataGridViewRow TheRow = CurrentGridRow; // самое простое свойство
        if (TheRow == null)
          return false; // нет текущей строки совсем

        if ((Control.SelectionMode == DataGridViewSelectionMode.FullRowSelect) ||
             Control.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect)
        {
          // Проверяем наличие других выбранных строк
          if (Control.SelectedRows.Count > 0)
          {
            for (int i = 0; i < Control.SelectedRows.Count; i++)
            {
              if (Control.SelectedRows[i] != TheRow)
                return false; // Нашли другую строку
            }
          }
        }

        // Проверяем наличие выделенных ячеек от других строк
        if (Control.SelectedCells.Count > 0)
        {
          for (int i = 0; i < Control.SelectedCells.Count; i++)
          {
            if (Control.SelectedCells[i].OwningRow != TheRow)
              return false;
          }
        }

        return true;
      }
    }

    /// <summary>
    /// Выбор прямоугольной области ячеек
    /// Свойство возвращает/устанавливает выбранные строки и столбцы в виде 
    /// координат прямоугольника
    /// </summary>
    public Rectangle SelectedRectAddress
    {
      get
      {
        if (Control.AreAllCellsSelected(false))
          return new Rectangle(0, 0, Control.ColumnCount, Control.RowCount);
        int[] Rows = SelectedRowIndices;
        int[] Cols = SelectedColumnIndices;
        if (Rows.Length == 0 || Cols.Length == 0)
          return Rectangle.Empty;

        int x1 = Cols[0];
        int x2 = Cols[Cols.Length - 1];
        int y1 = Rows[0];
        int y2 = Rows[Rows.Length - 1];
        return new Rectangle(x1, y1, x2 - x1 + 1, y2 - y1 + 1);
      }
      set
      {
        throw new BugException("Не реализовано");
      }
    }


    /// <summary>
    /// Вспомогательный метод
    /// Возвращает true, если в просмотре есть только одна выбранная (текущая) строка
    /// Если в просмотре нет текущей строки или выбрано больше одной строки, то
    /// выдается соответствующее сообщение с помошью EFPApp.ShowTempMessage().
    /// Возвращаемое значение соответствует свойству IsCurrentRowSingleSelected
    /// Используйте метод для упрощения реализации методов редактирования или команд
    /// локального меню, если они должны работать с единственной строкой. После
    /// вызова метода используйте одно из свойств CurrentXxxRow для доступа к строке
    /// </summary>
    /// <returns></returns>
    public bool CheckSingleRow()
    {
      if (CurrentGridRow == null)
      {
        EFPApp.ShowTempMessage("В табличном просмотре нет выбранной строки");
        return false;
      }
      if (!IsCurrentRowSingleSelected)
      {
        EFPApp.ShowTempMessage("В табличном просмотре выбрано больше одной строки");
        return false;
      }
      return true;
    }

    /// <summary>
    /// Перечислитель по выбранным ячейкам таблицы для всех режимов.
    /// Элементом перечисления является структура Point, содержащая индекс строки (Y) и столбца (X)
    /// очередной ячейки.
    /// Скрытые строки и столбцы не учитываются.
    /// Правильно обрабатываются и сложно выбранные ячейки.
    /// </summary>
    public sealed class SelectedCellAddressEnumerable : IEnumerable<Point>
    {
      #region Конструктор

      internal SelectedCellAddressEnumerable(DataGridView control)
      {
        _Control = control;
      }

      private DataGridView _Control;

      #endregion

      #region IEnumerable<Point> Members

      /// <summary>
      /// Создает перечислитель
      /// </summary>
      /// <returns>Перечислитель</returns>
      public IEnumerator<Point> GetEnumerator()
      {
        if (_Control.AreAllCellsSelected(false))
          return GetAllCellsEnumerator();

        if (_Control.SelectedRows.Count > 0 || _Control.SelectedColumns.Count > 0 || _Control.SelectedCells.Count > 0)
          return GetSelectedEnumerator();

        // нельзя здесь сделать yeld return
        return GetCurrentCellEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      #endregion

      #region Выбраны все ячейки

      private IEnumerator<Point> GetAllCellsEnumerator()
      {
        int[] ColIdxs = EFPDataGridView.GetSelectedColumnIndices(_Control); // все видимые столбцы

        for (int iRow = 0; iRow < _Control.RowCount; iRow++)
        {
          if (!_Control.Rows.SharedRow(iRow).Visible)
            continue;

          for (int j = 0; j < ColIdxs.Length; j++)
            yield return new Point(ColIdxs[j], iRow);
        }
      }

      #endregion

      #region Выбраны отдельные ячейки, строки или столбцы

      private IEnumerator<Point> GetSelectedEnumerator()
      {
        if (_Control.SelectedRows.Count > 0)
        {
          int[] ColIdxs = EFPDataGridView.GetSelectedColumnIndices(_Control); // все видимые столбцы

          for (int i = 0; i < _Control.SelectedRows.Count; i++)
          {
            for (int j = 0; j < ColIdxs.Length; j++)
              yield return new Point(ColIdxs[j], _Control.SelectedRows[i].Index);
          }
        }

        if (_Control.SelectedColumns.Count > 0)
        {
          int[] RowIdxs = EFPDataGridView.GetSelectedRowIndices(_Control); // все видимые строки
          for (int i = 0; i < RowIdxs.Length; i++)
          {
            for (int j = 0; j < _Control.SelectedColumns.Count; j++)
              yield return new Point(_Control.SelectedColumns[j].Index, RowIdxs[i]);

          }
        }

        if (_Control.SelectedCells.Count > 0)
        {
          for (int i = 0; i < _Control.SelectedCells.Count; i++)
            yield return new Point(_Control.SelectedCells[i].ColumnIndex, _Control.SelectedCells[i].RowIndex);
        }
      }

      #endregion

      #region Только текущая ячейка

      private IEnumerator<Point> GetCurrentCellEnumerator()
      {
        if (_Control.CurrentCellAddress.X >= 0 && _Control.CurrentCellAddress.Y >= 0)
          yield return _Control.CurrentCellAddress;
      }

      #endregion
    }

    /// <summary>
    /// Перечислитель по адресам выбранных ячеек (для оператора foreach)
    /// </summary>
    public SelectedCellAddressEnumerable SelectedCellAddresses { get { return new SelectedCellAddressEnumerable(Control); } }

    #endregion

    #region Доступ к выбранным ячейкам для источника данных DataView или DataTable

    /// <summary>
    /// Получение Control.DataSource в виде DataTable или null, если другой источник
    /// (не DataTable или DataView).
    /// Наличие таблицы-повторителя не учитывается, возвращается таблица, присоединенная к просмотру. 
    /// Если есть повторитель, то возвращается DataTableRepeater.SlaveTable.
    /// </summary>
    public DataTable SourceAsDataTable
    {
      get
      {
        /*
        DataView dv = Control.DataSource as DataView;
        if (dv != null)
          return dv.Table;

        return Control.DataSource as DataTable;
         * */

        // 24.05.2021
        DataView dv = SourceAsDataView;
        if (dv == null)
          return null;
        else
          return dv.Table;
      }
      set
      {
        ((ISupportInitialize)Control).BeginInit();
        try
        {
          Control.DataSource = value;
          Control.DataMember = String.Empty;
        }
        finally
        {
          ((ISupportInitialize)Control).EndInit();
        }
      }
    }

    /// <summary>
    /// Получение DataGridView.DataSource в виде DataTable 
    /// Для источника DataTable возвращает DefaultView
    /// Возвращает null, если другой источник.
    /// 
    /// Наличие таблицы-повторителя не учитывается, возвращается таблица, присоединенная к просмотру. 
    /// Если есть повторитель, то возвращается DataTableRepeater.SlaveTable.DefaultView
    /// </summary>
    public DataView SourceAsDataView
    {
      get
      {
        /*
        DataView dv = Control.DataSource as DataView;
        if (dv != null)
          return dv;

        DataTable table = Control.DataSource as DataTable;
        if (table != null)
          return table.DefaultView;

        return null;
         * */

        // 24.05.2021
        // return GetSourceAsDataView(Control.DataSource, Control.DataMember);
        // 04.07.2021
        return _SourceAsDataView;
      }
      set
      {
        ((ISupportInitialize)Control).BeginInit();
        try
        {
          Control.DataSource = value;
          Control.DataMember = String.Empty;
        }
        finally
        {
          ((ISupportInitialize)Control).EndInit();
        }
      }
    }
    private DataView _SourceAsDataView;

    private static DataView GetSourceAsDataView(object dataSource, string dataMember)
    {
      object x = ListBindingHelper.GetList(dataSource, dataMember);
      DataView dv = x as DataView;
      if (dv != null)
        return dv;
      if (x == null)
        return null;

      BindingSource bs = x as BindingSource;
      if (bs != null)
        return GetSourceAsDataView(bs.DataSource, bs.DataMember); // рекурсивный вызов

      // Можно было бы проверить у объекта x атрибут [ComplexBindingProperties], но нет гарантии, что это будет DataView с нужным порядком сортировки
      return null;
    }

    /// <summary>
    /// Получить строку DataRow таблицы, связанную с заданным объектом строки
    /// Строка <paramref name="gridRow"/> должна быть Unshared
    /// </summary>
    /// <param name="gridRow">Объект строки табличного просмотра</param>
    /// <returns>Строка в таблице DataTable</returns>
    public static DataRow GetDataRow(DataGridViewRow gridRow)
    {
      if (gridRow == null)
        return null;

      // 24.05.2021
      // DataBoundItem не может быть ссылкой на DataRow. Всегда только DataRowView

      // Может вылазить исключение при попытке обращения к Row.DataBoundItem, если
      // оно было связано с DataViewRow для последней строки, когда произошло ее 
      // удаление 
      object boundItem;
      try
      {
        boundItem = gridRow.DataBoundItem;
        DataRowView drv = boundItem as DataRowView;
        if (drv != null)
          return drv.Row;
      }
      catch { }

#if DEBUG
      if (gridRow.Index < 0)
        throw new ArgumentException("Строка DataGridViewRow является Shared", "gridRow");
#endif

      return null;
    }

    /// <summary>
    /// Более предпочтительный способ получения строки DataRow по номеру строки в
    /// табличном просмотре. Не делает строку Unshared, т.к. не обращается к
    /// объекту DataGridViewRow
    /// Статический вариант метода
    /// </summary>
    /// <param name="control">Табличный просмотр, не обязательно имеющий DocGridHandler</param>
    /// <param name="rowIndex">Номер строки</param>
    /// <returns>Объект DataRow или null при любой ошибке</returns>
    public static DataRow GetDataRow(DataGridView control, int rowIndex)
    {
      /*
      if (control.DataSource is DataView)
      {
        if (rowIndex < 0 || rowIndex >= ((DataView)(control.DataSource)).Count)
          return null;
        return ((DataView)(control.DataSource))[rowIndex].Row;
      }
      if (control.DataSource is DataTable)
      {
        if (rowIndex < 0 || rowIndex >= ((DataTable)(control.DataSource)).Rows.Count)
          return null;
        return ((DataTable)(control.DataSource)).DefaultView[rowIndex].Row;
      }

      return null;
       */


      // 24.05.2021
      if (rowIndex < 0 || rowIndex >= control.RowCount)
        return null;

      // Если строка уже является unshared, то есть быстрый доступ добраться до DataRow.
      DataGridViewRow gridRow = control.Rows.SharedRow(rowIndex);
      if (gridRow.Index >= 0)
        return GetDataRow(gridRow);

      DataView dv = GetSourceAsDataView(control.DataSource, control.DataMember);
      if (dv == null)
      {
        // Делаем строку Unshared
        gridRow = control.Rows[rowIndex];
        return GetDataRow(gridRow);
      }
      else
      {
        if (rowIndex >= dv.Count)
          return null;
        else
          return dv[rowIndex].Row;
      }
    }

    /// <summary>
    /// Более предпочтительный способ получения строки DataRow по номеру строки в
    /// табличном просмотре. Не делает строку Unshared, т.к. не обращается к
    /// объекту DataGridViewRow
    /// Нестатический вариант
    /// </summary>
    /// <param name="rowIndex">Номер строки</param>
    /// <returns>Объект DataRow или null при любой ошибке</returns>
    public DataRow GetDataRow(int rowIndex)
    {
      return EFPDataGridView.GetDataRow(Control, rowIndex);

      // 29.09.2017
      // Через SharedRow не работает
      // Если строка Shared, для нее DataBoundItem равно null
      //if (RowIndex < 0 || RowIndex >= Control.RowCount)
      //  return null;
      //else
      //  return GetDataRow(Control.Rows.SharedRow(RowIndex));
    }


    /// <summary>
    /// Получить массив строк DataRow по индексам строк.
    /// Если какой-либо индекс не относится к DataRow, элемент массива содержит null.
    /// </summary>
    /// <param name="rowIndices">Номера строк в просмотре DataGridView</param>
    /// <returns>Массив объектов DataRow</returns>
    public DataRow[] GetDataRows(int[] rowIndices)
    {
      DataView dv = null;
      bool dvDefined = false;
      DataRow[] Rows = new DataRow[rowIndices.Length];
      for (int i = 0; i < Rows.Length; i++)
      {
        if (rowIndices[i] >= 0 && rowIndices[i] < Control.RowCount)
        {
          // Если строка уже является unshared, то есть быстрый доступ добраться до DataRow.
          // Обычно, этот вариант и будет работать, так как, скорее всего, опрашиваются выбранные строки
          DataGridViewRow gridRow = Control.Rows.SharedRow(rowIndices[i]);
          if (gridRow.Index >= 0)
            Rows[i] = GetDataRow(gridRow);
          else
          {
            if (!dvDefined)
            {
              dvDefined = true;
              dv = SourceAsDataView;
            }
            if (rowIndices[i] < dv.Count)
              Rows[i] = dv[rowIndices[i]].Row;
          }
        }
      }

      return Rows;
    }

    /// <summary>
    /// Получить или установить выбранные строки в просмотре.
    /// Расширяет реализацию свойства SelectedRows, используя вместо объектов
    /// DataGridViewRow строки таблицы данных DataRow.
    /// Если просмотр не присоединен к таблице данных, свойство возвращает массив, содержащий значения null.
    /// 
    /// Если используется таблица-повторитель, то возвращаются и должны устанавливаться строки таблицы, присоединенной к просмотру,
    /// то есть DataTableRepeater.SlaveTable.
    /// </summary>
    public DataRow[] SelectedDataRows
    {
      get
      {
        DataRow[] res;

        if (Control.AreAllCellsSelected(true)) // надо бы AreAllRowsSelected
        {
          // 24.05.2021
          // Оптимизированный метод без доступа к DataGridViewRow

          DataView dv = SourceAsDataView;
          if (dv == null)
            return new DataRow[Control.RowCount];
          else
          {
            res = new DataRow[dv.Count];
            for (int i = 0; i < res.Length; i++)
              res[i] = dv[i].Row;
          }
        }
        else
        {
          DataGridViewRow[] gridRows = SelectedGridRows;
          res = new DataRow[gridRows.Length];
          for (int i = 0; i < res.Length; i++)
            res[i] = GetDataRow(gridRows[i]);
        }
        return res;
      }
      set
      {
        if (value.Length == 0)
          return;

        // Тут несколько гадко
        // Поэтому сначала проверяем, есть ли какие-нибудь изменения
        DataRow[] OrgRows = SelectedDataRows;
        if (OrgRows.Length == value.Length)
        {
          bool Differ = false;
          for (int i = 0; i < value.Length; i++)
          {
            if (value[i] != OrgRows[i])
            {
              Differ = true;
              break;
            }
          }
          if (!Differ)
            return; // строки не изменились
        }

        DataView dv = SourceAsDataView;
        if (dv == null)
          throw new InvalidDataSourceException("Просмотр не связан с DataView");

        if (dv.Count != Control.RowCount)
          return; // 09.02.2021. Просмотр может быть еще не выведен на экран и Control.RowCount==0.

        if (value.Length == 1)
        {
          // 04.07.2021 Оптимизация
          if (value[0] == null)
            return;
          if (Object.ReferenceEquals(CurrentDataRow, value[0]))
          {
            SelectedGridRows = new DataGridViewRow[1] { CurrentGridRow };
            return;
          }

          int p = DataTools.FindDataRowViewIndex(dv, value[0]);
          if (p >= 0) // исправлено 16.08.2021
            SelectedRowIndices = new int[1] { p };
          return;
        }

        DataGridViewRow[] Rows = new DataGridViewRow[value.Length];
        // Производительность будет ужасной, если выбрано много строк в большой таблице
        int SelCnt = 0; // Вдруг найдем все строки до того, как переберем весь набор данных ?
        for (int j = 0; j < dv.Count; j++)
        {
          DataRow ThisRow = GetDataRow(j);
          for (int k = 0; k < value.Length; k++)
          {
            if (value[k] == ThisRow)
            {
              Rows[k] = Control.Rows[j];
              SelCnt++;
              break;
            }
          }
          if (SelCnt == value.Length)
            break; // уже все нашли
        }
        SelectedGridRows = Rows;
      }
    }

    /// <summary>
    /// Расширение свойства CurrentRow (чтение и установка текущей строки).
    /// В качестве значения используется строка таблицы данных (объект DataRow)
    /// Просмотр должен быть присоединен к DataSource типа DataView.
    /// 
    /// Если используется таблица-повторитель, то возвращается и должна устанавливаться строка таблицы, присоединенной к просмотру,
    /// то есть DataTableRepeater.SlaveTable.
    /// </summary>
    public DataRow CurrentDataRow
    {
      get
      {
        return GetDataRow(Control.CurrentRow); // 24.05.2021
      }
      set
      {
        if (value == null)
          return;
        DataView dv = SourceAsDataView;
        if (dv == null)
          throw new InvalidDataSourceException("Свойство DataGridView.DataSource не является DataView или DataTable");

        // 04.07.2021
        // Оптимизация
        if (Object.ReferenceEquals(value, GetDataRow(Control.CurrentRow)))
          return;

        int p = DataTools.FindDataRowViewIndex(dv, value);
        if (p >= 0)
          CurrentRowIndex = p;
      }
    }

#if XXX
    /// <summary>
    /// Выделяет строку в просмотре, соответствующую заданной строке DataRow.
    /// Используйте установку свойства CurrentDataRow
    /// </summary>
    /// <param name="row"></param>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Используйте установку свойства CurrentDataRow", false)]
    public void SelectDataRow(DataRow row)
    {
      CurrentDataRow = row;
    }
#endif

    /// <summary>
    /// Расширение свойств SelectedRows / SelectedDataRows
    /// В качестве текущих позиций запоминаются значения ключевых полей в DataTable
    /// Первый индекс двумерного массива соответствует количеству строк.
    /// Второй индекс соответствует количеству полей в DataTable.PrimaryKey 
    /// (обычно равно 1)
    /// Свойство возвращает null, если таблица не присоединена к просмотру (напрямую или через DataView)
    /// или не имеет первичного ключа
    /// </summary>
    public object[,] SelectedDataRowKeys
    {
      get
      {
        DataRow[] Rows = SelectedDataRows;
        DataTable Table = SourceAsDataTable;
        if (Table == null)
          return null;
        if (Table.PrimaryKey == null || Table.PrimaryKey.Length == 0)
          return null;
        return DataTools.GetPrimaryKeyValues(Table, Rows);
      }
      set
      {
        if (value == null)
          return;
        DataTable Table = SourceAsDataTable;
        if (Table == null)
          throw new InvalidOperationException("DataGridView.DataSource не является DataTable");
        DataRow[] Rows = DataTools.GetPrimaryKeyRows(Table, value);
        SelectedDataRows = Rows;
      }
    }

    /// <summary>
    /// Расширение свойств CurrentRow / CurrentDataRow для получения / установки 
    /// текущей строки с помощью значений ключевых полей в DataTable
    /// </summary>
    public object[] CurrentDataRowKeys
    {
      get
      {
        DataRow Row = CurrentDataRow;
        if (Row == null)
          return null;
        return DataTools.GetPrimaryKeyValues(Row);
      }
      set
      {
        DataTable Table = SourceAsDataTable;
        if (Table == null)
          throw new InvalidOperationException("DataGridView.DataSource не является DataTable");
        if (value == null)
          return;
        DataRow Row = Table.Rows.Find(value);
        if (Row == null)
          return;
        CurrentDataRow = Row;
      }
    }

    /// <summary>
    /// Расширение свойств SelectedRows / SelectedDataRows
    /// В качестве текущих позиций запоминаются значения полей, заданных в DataView.ViewOrder
    /// Первый индекс двумерного массива соответствует количеству строк.
    /// Второй индекс соответствует количеству полей сортировки в DataView
    /// (обычно равно 1)
    /// Свойство возвращает null, если просмотру не соединен с DataView 
    /// или DataView не имеет полей сортировки
    /// </summary>
    public object[,] SelectedDataViewSortKeys
    {
      get
      {
        DataView dv = SourceAsDataView;
        if (dv == null)
          return null;
        if (String.IsNullOrEmpty(dv.Sort))
          return null;
        string[] flds = DataTools.GetDataViewSortColumnNames(dv.Sort);
        DataRow[] Rows = SelectedDataRows;
        object[,] Values = new object[Rows.Length, flds.Length];
        for (int i = 0; i < Rows.Length; i++)
        {
          for (int j = 0; j < flds.Length; j++)
            Values[i, j] = Rows[i][flds[j]];
        }
        return Values;
      }
      set
      {
        if (value == null)
          return;
        DataView dv = SourceAsDataView;
        if (dv == null)
          throw new InvalidOperationException("К просмотру не присоединен объект DataView");
        if (String.IsNullOrEmpty(dv.Sort))
          throw new InvalidOperationException("Присоединенный к просмотру объект DataView не имеет полей сортировки");

        int nRows = value.GetLength(0);
        if (nRows == 0)
          return;
        object[] Keys = new object[value.GetLength(1)];
        List<int> rowIndices = new List<int>();
        for (int i = 0; i < nRows; i++)
        {
          for (int j = 0; j < Keys.Length; j++)
            Keys[j] = value[i, j];
          int idx = dv.Find(Keys);
          if (idx > 0)
            rowIndices.Add(idx);
        }

        SelectedRowIndices = rowIndices.ToArray();
      }
    }

    /// <summary>
    /// Расширение свойств CurrentRow / CurrentDataRow для получения / установки 
    /// текущей строки с помощью значений полей сортировки в DataView
    /// </summary>
    public object[] CurrentDataViewSortKeys
    {
      get
      {
        DataView dv = SourceAsDataView;
        if (dv == null)
          return null;
        if (String.IsNullOrEmpty(dv.Sort))
          return null;
        string[] flds = DataTools.GetDataViewSortColumnNames(dv.Sort);
        DataRow Row = CurrentDataRow;
        object[] Values = new object[flds.Length];
        if (Row != null)
        {
          for (int j = 0; j < flds.Length; j++)
            Values[j] = Row[flds[j]];
        }
        return Values;
      }
      set
      {
        if (value == null)
          return;
        DataView dv = SourceAsDataView;
        if (dv == null)
          throw new InvalidOperationException("К просмотру не присоединен объект DataView");
        if (String.IsNullOrEmpty(dv.Sort))
          throw new InvalidOperationException("Присоединенный к просмотру объект DataView не имеет полей сортировки");

        int idx = dv.Find(value);
        if (idx > 0)
          CurrentRowIndex = idx;
      }
    }

    #endregion

    #region Повторитель таблицы DataTableRepeater

    /// <summary>
    /// Повторитель таблицы.
    /// При установке свойства устанавливается DataGridView.DataSource=DataTableRepeater.SlaveTable.
    /// Предполагается, что повторитель относится персонально к этому табличному просмотру.
    /// Когда управляющий элемент разрушается, вызывается DataTableRepeater.Dispose().
    /// При повторной установке значения предыдущий повторитель также удаляется.
    /// </summary>
    public DataTableRepeater TableRepeater
    {
      get { return _TableRepeater; }
      set
      {
        if (object.ReferenceEquals(value, _TableRepeater))
          return;
        Control.DataSource = null;
        Control.DataMember = null;
        if (_TableRepeater != null)
          _TableRepeater.Dispose();
        _TableRepeater = value;
        if (_TableRepeater != null)
          Control.DataSource = _TableRepeater.SlaveTable.DefaultView;
      }
    }
    private DataTableRepeater _TableRepeater;

    /// <summary>
    /// Вызывает DataTableRepeater.Dispose(), если свойство TableRepeater было установлено.
    /// </summary>
    protected override void OnDisposed()
    {
      if (_TableRepeater != null)
      {
        _TableRepeater.Dispose();
        _TableRepeater = null;
      }

      base.OnDisposed();
    }

    /// <summary>
    /// Возвращает строку в таблице-повторителе DataTableRepeater.SlaveTable, которая соответствует строке в исходной таблице DataTableRepeater.MasterTable.
    /// Если повторитель TableRepeater не установлен, возврашает исходные данные без изменений.
    /// </summary>
    /// <param name="masterRow">Строка таблицы MasterTable</param>
    /// <returns>Строка таблицы SlaveTable</returns>
    public DataRow GetSlaveRow(DataRow masterRow)
    {
      if (_TableRepeater == null)
        return masterRow;
      else
        return _TableRepeater.GetSlaveRow(masterRow);
    }

    /// <summary>
    /// Возвращает строки в таблице-повторителе DataTableRepeater.SlaveTable, которые соответствуют строкам в исходной таблице DataTableRepeater.MasterTable
    /// Если повторитель TableRepeater не установлен, возврашает исходные данные без изменений.
    /// </summary>
    /// <param name="masterRows">Массив строк таблицы MasterTable</param>
    /// <returns>Строки таблицы SlaveTable</returns>
    public DataRow[] GetSlaveRows(DataRow[] masterRows)
    {
      if (_TableRepeater == null)
        return masterRows;
      else
        return _TableRepeater.GetSlaveRows(masterRows);
    }

    /// <summary>
    /// Возвращает строку в основной таблице DataTableRepeater.MasterTable, которая соответствует строке в таблице-повторителе DataTableRepeater.SlaveTable.
    /// Используется для реализации редактирования, т.к. изменения должны вноситься в ведущую
    /// таблицу, а не ту, которая отображается в просмотре.
    /// Если повторитель TableRepeater не установлен, возврашает исходные данные без изменений.
    /// </summary>
    /// <param name="slaveRow">Строка таблицы SlaveTable</param>
    /// <returns>Строка таблицы MasterTable</returns>
    public DataRow GetMasterRow(DataRow slaveRow)
    {
      if (_TableRepeater == null)
        return slaveRow;
      else
        return _TableRepeater.GetMasterRow(slaveRow);
    }

    /// <summary>
    /// Возвращает строки в основной таблице DataTableRepeater.MasterTable, которые соответствуют строкам в таблице-повторителе DataTableRepeater.SlaveTable.
    /// Используется для реализации редактирования, т.к. изменения должны вноситься в ведущую
    /// таблицу, а не ту, которая отображается в просмотре.
    /// Если повторитель TableRepeater не установлен, возврашает исходные данные без изменений.
    /// </summary>
    /// <param name="slaveRows">Строки таблицы SlaveTable</param>
    /// <returns>Строки таблицы MasterTable</returns>
    public DataRow[] GetMasterRows(DataRow[] slaveRows)
    {
      if (_TableRepeater == null)
        return slaveRows;
      else
        return _TableRepeater.GetMasterRows(slaveRows);
    }

    /// <summary>
    /// Если есть таблица-повторитель, то возвращается мастер-таблица DataTableRepeater.MasterTable.
    /// Иначе возвращается SourceAsDataTable.
    /// </summary>
    public DataTable MasterDataTable
    {
      get
      {
        if (_TableRepeater == null)
          return SourceAsDataTable;
        else
          return _TableRepeater.MasterTable;
      }
    }

    /// <summary>
    /// Если есть таблица-повторитель, то возвращается DataView для мастер-таблицы DataTableRepeater.MasterTable.DefaultView.
    /// Иначе возвращается SourceAsDataView.
    /// </summary>
    public DataView MasterDataView
    {
      get
      {
        if (TableRepeater == null)
          return SourceAsDataView;
        else
          return _TableRepeater.MasterTable.DefaultView;
      }
    }

    #endregion

    #region Видимые строки

    /// <summary>
    /// Возвращает массив объектов DataRow, связанных со строками, видимыми в просмотре
    /// </summary>
    public DataRow[] DisplayedDataRows
    {
      get
      {
        int FirstRow = Control.FirstDisplayedScrollingRowIndex;
        int n = Control.DisplayedRowCount(true);
        List<DataRow> Rows = new List<DataRow>(n);
        for (int i = 0; i < n; i++)
        {
          DataRow Row = GetDataRow(FirstRow + i);
          if (Row != null)
            Rows.Add(Row);
        }
        return Rows.ToArray();
      }
    }

    #endregion

    #region Сохранение / восстановление выбранных строк

    /// <summary>
    /// Режим сохранения выбранных строк свойствами SelectedRowsObject и
    /// CurrentRowObject
    /// По умолчанию для EFPDataGridView - значение None.
    /// Может быть переопределено в производных классах
    /// </summary>
    public EFPDataGridViewSelectedRowsMode SelectedRowsMode
    {
      get { return _SelectedRowsMode; }
      set { _SelectedRowsMode = value; }
    }
    private EFPDataGridViewSelectedRowsMode _SelectedRowsMode;


    /// <summary>
    /// Сохранение и восстановление выбранных строк просмотра в виде одного объекта,
    /// в зависимости от свойства SelectedRowsMode
    /// </summary>
    public virtual object SelectedRowsObject
    {
      get
      {
        try
        {
          switch (SelectedRowsMode)
          {
            case EFPDataGridViewSelectedRowsMode.RowIndex:
              return SelectedRowIndices;
            case EFPDataGridViewSelectedRowsMode.DataRow:
              return GetMasterRows(SelectedDataRows);
            case EFPDataGridViewSelectedRowsMode.PrimaryKey:
              return SelectedDataRowKeys;
            case EFPDataGridViewSelectedRowsMode.DataViewSort:
              return SelectedDataViewSortKeys;
          }
        }
        catch
        {
        }
        return null;
      }
      set
      {
        if (value == null)
          return;
        switch (SelectedRowsMode)
        {
          case EFPDataGridViewSelectedRowsMode.RowIndex:
            SelectedRowIndices = (int[])value;
            break;
          case EFPDataGridViewSelectedRowsMode.DataRow:
            SelectedDataRows = GetSlaveRows((DataRow[])value);
            break;
          case EFPDataGridViewSelectedRowsMode.PrimaryKey:
            SelectedDataRowKeys = (object[,])value;
            break;
          case EFPDataGridViewSelectedRowsMode.DataViewSort:
            SelectedDataViewSortKeys = (object[,])value;
            break;
        }
      }
    }

    /// <summary>
    /// Сохранение и восстановление текущей (одной) строки просмотра.
    /// Тип свойства зависит от режима SelectedRowsMode.
    /// </summary>
    public virtual object CurrentRowObject
    {
      get
      {
        try
        {
          switch (SelectedRowsMode)
          {
            case EFPDataGridViewSelectedRowsMode.RowIndex:
              return Control.CurrentCellAddress.Y;
            case EFPDataGridViewSelectedRowsMode.DataRow:
              return GetMasterRow(CurrentDataRow); // 04.07.2021
            case EFPDataGridViewSelectedRowsMode.PrimaryKey:
              return CurrentDataRowKeys;
            case EFPDataGridViewSelectedRowsMode.DataViewSort:
              return CurrentDataViewSortKeys;
          }
        }
        catch
        {
        }
        return null;
      }
      set
      {
        if (value == null)
          return;
        switch (SelectedRowsMode)
        {
          case EFPDataGridViewSelectedRowsMode.RowIndex:
            int RowIndex = (int)value;
            if (RowIndex >= 0 && RowIndex < Control.Rows.Count)
              CurrentGridRow = Control.Rows[RowIndex];
            break;
          case EFPDataGridViewSelectedRowsMode.DataRow:
            CurrentDataRow = GetSlaveRow((DataRow)value); // 04.07.2021
            break;
          case EFPDataGridViewSelectedRowsMode.PrimaryKey:
            CurrentDataRowKeys = (object[])value;
            break;
          case EFPDataGridViewSelectedRowsMode.DataViewSort:
            CurrentDataViewSortKeys = (object[])value;
            break;
        }
      }
    }

    /// <summary>
    /// Сохранение и восстановление текущих строк и столбца табличного просмотра.
    /// Режим сохранения строк определеятся свойство SelectedRowsMode
    /// Значение включает в себя: признак AreAllCellsSelected, список выделенных
    /// строк SelectedRowsObject, текущую строку CurrentRowObject и индекс
    /// столбца с текущей ячейкой CurrentColumnIndex.
    /// При установке значения свойства перехватываются все исключения.
    /// </summary>
    public EFPDataGridViewSelection Selection
    {
      get
      {
        EFPDataGridViewSelection res = new EFPDataGridViewSelection();
        if (Control.RowCount > 0) // 11.08.2015. Иначе при отсутствии строк считается, что выделены все строки
        {
          res.SelectAll = Control.AreAllCellsSelected(false);
          if (!res.SelectAll)
            res.SelectedRows = SelectedRowsObject;
          res.CurrentRow = CurrentRowObject;
        }
        res.CurrentColumnIndex = CurrentColumnIndex;
        res.DataSourceExists = Control.DataSource != null;
        return res;
      }
      set
      {
        if (value == null)
          return;
        try
        {
          CurrentColumnIndex = value.CurrentColumnIndex;
          bool DataSouceExists = (Control.DataSource != null);
          if (DataSouceExists == value.DataSourceExists)
          {
            CurrentRowObject = value.CurrentRow;
            if (value.SelectAll)
              Control.SelectAll();
            else
              SelectedRowsObject = value.SelectedRows;
          }
        }
        catch // 26.09.2018
        {
        }
      }
    }

    #endregion

    #region Текущий столбец

    /// <summary>
    /// Номер текущего столбца. Если выбрано несколько столбцов (например, строка целиком),
    /// то (-1)
    /// </summary>
    public int CurrentColumnIndex
    {
      get
      {
        if (_CurrentColumnIndex < 0)
          return DefaultColumnIndex; // 16.05.2018
        //{
        //  for (int i = 0; i < Control.Columns.Count; i++)
        //  {
        //    if (Control.Columns[i].Visible)
        //      return i;
        //  }
        //  return -1;
        //}
        else
          return _CurrentColumnIndex;
      }
      set
      {
        if (value < 0 || value >= Control.ColumnCount)
          return;
        if (!Control.Columns[value].Visible)
          return;
        int RowIndex = Control.CurrentCellAddress.Y;
        if (Control.Visible && RowIndex >= 0 && RowIndex < Control.Rows.Count)
        {
          try
          {
            Control.CurrentCell = Control.Rows[RowIndex].Cells[value];
          }
          catch
          {
          }
        }
        _CurrentColumnIndex = value;
      }
    }

    /// <summary>
    /// Сохраняем установленный ColumnIndex, если потом произойдет пересоздание просмотра
    /// </summary>
    private int _CurrentColumnIndex;

    /// <summary>
    /// Текущий столбец. Если выбрано несколько столбцов (например, строка целиком),
    /// то null
    /// </summary>
    public EFPDataGridViewColumn CurrentColumn
    {
      get
      {
        if (CurrentColumnIndex < 0 || CurrentColumnIndex >= Control.ColumnCount)
          return null;
        else
          return Columns[CurrentColumnIndex];
      }
      set
      {
        CurrentColumnIndex = value.GridColumn.Index;
      }
    }

    /// <summary>
    /// Имя текущего столбца. Если выбрано несколько столбцов (например, строка целиком),
    /// то пустая строка ("")
    /// </summary>
    public string CurrentColumnName
    {
      get
      {
        if (CurrentColumnIndex < 0 || CurrentColumnIndex >= Control.ColumnCount)
          return String.Empty;
        else
          return Control.Columns[CurrentColumnIndex].Name;
      }
      set
      {
        EFPDataGridViewColumn Column = Columns[value];
        if (Column != null)
          CurrentColumnIndex = Column.GridColumn.Index;
      }
    }

    #endregion

    #region Столбец по умолчанию

    /// <summary>
    /// Возвращает индекс столбца, который следует активировать по умолчанию.
    /// При показе просмотра, если свойство CurrentColumnIndex не установлено в явном виде, 
    /// выбирается этот столбец.
    /// Предпочтительно возвращает первый столбец с инкрементным поиском.
    /// Иначе старается вернуть первый столбец, кроме значка.
    /// Скрытые столбцы пропускаются.
    /// </summary>
    public int DefaultColumnIndex
    {
      get
      {
        DataGridViewColumn[] a = VisibleGridColumns;
        int FirstIdx = -1;
        for (int i = 0; i < a.Length; i++)
        {
          if (a[i] is DataGridViewImageColumn)
            continue;
          if (FirstIdx < 0)
            FirstIdx = a[i].Index;

          EFPDataGridViewColumn Col = Columns[a[i]]; // предпочтительный вариант индексированного свойства
          if (Col.CanIncSearch)
            return a[i].Index;
        }
        if (FirstIdx >= 0)
          return FirstIdx;

        // Ничего не нашли
        if (a.Length > 0)
          return a[0].Index;
        else
          return -1;
      }
    }

    #endregion

    #region Видимые столбцы

    /// <summary>
    /// Получить массив видимых столбцов в просмотре в порядке вывода на экран
    /// Объекты EFPDataGridViewColumn
    /// </summary>
    public EFPDataGridViewColumn[] VisibleColumns
    {
      get
      {
        DataGridViewColumn[] cols = VisibleGridColumns;
        EFPDataGridViewColumn[] a = new EFPDataGridViewColumn[cols.Length];
        for (int i = 0; i < cols.Length; i++)
          a[i] = Columns[cols[i]];
        return a;
      }
    }

#if XXX
    /// <summary>
    /// Получить столбцы с заданными условиями в виде массива
    /// </summary>
    /// <param name="states"></param>
    /// <returns></returns>
    [Obsolete("Не работает в MONO", false)]
    public DataGridViewColumn[] GetGridColumns(DataGridViewElementStates states)
    {
      List<DataGridViewColumn> Columns = new List<DataGridViewColumn>();
      DataGridViewColumn Col = Control.Columns.GetFirstColumn(states);
      while (Col != null)
      {
        Columns.Add(Col);
        Col = Control.Columns.GetNextColumn(Col, states, DataGridViewElementStates.None);
      }
      return Columns.ToArray();
    }
#endif

    /// <summary>
    /// Получить массив видимых столбцов в просмотре в порядке вывода на экран
    /// Объекты столбцов табличного просмотра
    /// </summary>
    public DataGridViewColumn[] VisibleGridColumns
    {
      get
      {
        return WinFormsTools.GetOrderedVisibleColumns(Control);
        //return GetGridColumns(DataGridViewElementStates.Visible);
      }
    }

    /// <summary>
    /// Количество "замороженных" столбцов в просмотре
    /// (по умолчанию 0 - нет замороженных столбцов)
    /// </summary>
    [DefaultValue(0)]
    public int FrozenColumns
    {
      get
      {
        return Control.Columns.GetColumnCount(DataGridViewElementStates.Frozen);
      }
      set
      {
        // ???
        if (value > 0)
          Control.Columns[value - 1].Frozen = true;
        if (Control.Columns.Count > value)
          Control.Columns[value].Frozen = false;
      }
    }

    #endregion

    #region Видимые строки

    /// <summary>
    /// Получить строки с заданными условиями в виде массива
    /// </summary>
    /// <param name="states"></param>
    /// <returns></returns>
    public DataGridViewRow[] GetGridRows(DataGridViewElementStates states)
    {
      List<DataGridViewRow> Rows = new List<DataGridViewRow>();
      int RowIndex = Control.Rows.GetFirstRow(states);
      while (RowIndex >= 0)
      {
        Rows.Add(Control.Rows[RowIndex]);
        RowIndex = Control.Rows.GetNextRow(RowIndex, states, DataGridViewElementStates.None);
      }
      return Rows.ToArray();
    }

    /// <summary>
    /// Получить массив видимых строк в просмотре в порядке вывода на экран
    /// Объекты видимых строк табличного просмотра
    /// </summary>
    public DataGridViewRow[] VisibleGridRows
    {
      get
      {
        return GetGridRows(DataGridViewElementStates.Visible);
      }
    }

    /// <summary>
    /// Возвращает высоту в пикселах, которую бы хотел иметь табличный просмотр
    /// для размещения всех строк
    /// Возвращаемое значение не превышает высоту экрана
    /// </summary>
    public int WantedHeight
    {
      get
      {
        // Максимальная высота, которую можно вернуть
        int hMax = Screen.FromControl(Control).Bounds.Height;

        int h = 0;


        // рамка
        switch (Control.BorderStyle)
        {
          case BorderStyle.FixedSingle:
            h += SystemInformation.BorderSize.Height * 2;
            break;
          case BorderStyle.Fixed3D:
            h += SystemInformation.Border3DSize.Height * 2;
            break;
        }

        // полоса прокрутки
        if (Control.ScrollBars == ScrollBars.Horizontal || Control.ScrollBars == ScrollBars.Both)
          h += SystemInformation.HorizontalScrollBarHeight;

        // заголовки столбцов
        if (Control.ColumnHeadersVisible)
          h += Control.ColumnHeadersHeight;

        // строки
        for (int i = 0; i < Control.Rows.Count; i++)
        {
          if (Control.Rows[i].Visible)
          {
            h += Control.Rows[i].Height;
            if (h >= hMax)
              break;
          }
        }

        return Math.Min(h, hMax);
      }
    }

    #endregion

    #region InvalidateRows()

    /// <summary>
    /// Обновить строки табличного просмотра.
    /// Выполняет только вызов Control.InvalidateRow().
    /// Для обновления строк с загрузкой данных используйте UpdateRows()
    /// </summary>
    /// <param name="rowIndices">Индексы строк в просмотре или null, если должны быть обновлены все строки</param>
    public void InvalidateRows(int[] rowIndices)
    {
      if (rowIndices == null)
        Control.Invalidate();
      else
      {
        for (int i = 0; i < rowIndices.Length; i++)
        {
          if (rowIndices[i] >= 0 && rowIndices[i] < Control.RowCount) // проверка добавлена 16.04.2018
            Control.InvalidateRow(rowIndices[i]);
        }
      }
    }


#if XXX
    /// <summary>
    /// Обновить все строки табличного просмотра.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Используйте InvalidateAllRows()", false)]
    public void InvalidateRows()
    {
      InvalidateAllRows();
    }
#endif

    /// <summary>
    /// Обновить все строки табличного просмотра.
    /// </summary>
    public void InvalidateAllRows()
    {
      InvalidateRows(null);
    }

    /// <summary>
    /// Обновить в просмотре все выбранные строки SelectedRows, например, после
    /// редактирования, если имеются вычисляемые столбцы
    /// </summary>
    public void InvalidateSelectedRows()
    {
      if (Control.AreAllCellsSelected(false))
        InvalidateRows(null);
      else
        InvalidateRows(SelectedRowIndices);
    }

    /// <summary>
    /// Пометить на обновление строки табличного просмотра, связанные с заданными строками таблицы данных
    /// </summary>
    /// <param name="rows">Массив строк связанной таблицы данных</param>
    public void InvalidateDataRows(DataRow[] rows)
    {
      if (rows == null)
        return;

      if (rows.Length == 0)
        return;

      if (rows.Length == 1)
      {
        InvalidateDataRow(rows[0]);
        return;
      }

      // Выгодно сначала найти все DataRow, которые есть на экране (их немного), 
      // а затем просматривать требуемый список DataRow (может быть длинный)

      int FirstRow = Control.FirstDisplayedScrollingRowIndex;
      int n = Control.DisplayedRowCount(true);
      Dictionary<DataRow, int> RowIdxs = new Dictionary<DataRow, int>(n);
      for (int i = 0; i < n; i++)
      {
        DataRow R = GetDataRow(FirstRow + i);
        if (R != null)
          RowIdxs.Add(R, FirstRow + i);
      }

      for (int i = 0; i < rows.Length; i++)
      {
        int RowIdx;
        if (RowIdxs.TryGetValue(rows[i], out RowIdx))
          InvalidateRows(new int[1] { RowIdx });
      }
    }

    /// <summary>
    /// Пометить на обновление строку табличного просмотра, связанную с заданной строкой таблицы данных
    /// </summary>
    /// <param name="row">Строка связанной таблицы данных</param>
    public void InvalidateDataRow(DataRow row)
    {
      if (row == null)
        return;

      int FirstRow = Control.FirstDisplayedScrollingRowIndex;
      int n = Control.DisplayedRowCount(true);
      for (int i = 0; i < n; i++)
      {
        if (GetDataRow(FirstRow + i) == row)
        {
          InvalidateRows(new int[1] { FirstRow + i });
          break;
        }
      }
    }

    #endregion

    #region UpdateRows()

    /// <summary>
    /// Обновить строки табличного просмотра, загрузив исходные данные.
    /// Производный класс должен обновить данные в источнике данных просмотра.
    /// Непереопределенная реализация в EFPDataGridView() просто вызывает InvalidateRows()
    /// </summary>
    /// <param name="rowIndices">Индексы строк в просмотре или null, если должны быть обновлены все строки</param>
    public virtual void UpdateRows(int[] rowIndices)
    {
      InvalidateRows(rowIndices);
    }

    /// <summary>
    /// Обновить все строки табличного просмотра.
    /// </summary>
    public void UpdateRows()
    {
      UpdateRows(null);
    }

    /// <summary>
    /// Обновить в просмотре все выбранные строки SelectedRows, например, после
    /// редактирования, если имеются вычисляемые столбцы
    /// </summary>
    public void UpdateSelectedRows()
    {
      if (Control.AreAllCellsSelected(false))
        UpdateRows(null);
      else
        UpdateRows(SelectedRowIndices);
    }

    /// <summary>
    /// Обновить строки табличного просмотра, связанные с заданными строками таблицы данных
    /// </summary>
    /// <param name="rows">Массив строк связанной таблицы данных</param>
    public void UpdateDataRows(DataRow[] rows)
    {
      if (rows == null)
        return;

      if (rows.Length == 0)
        return;

      if (rows.Length == 1)
      {
        UpdateDataRow(rows[0]); // более быстрый метод
        return;
      }

      DataView dv = SourceAsDataView;
      if (dv == null)
        throw new InvalidOperationException("Табличный просмотр не связан с набором данных");

      Dictionary<DataRow, int> allRowIndices = new Dictionary<DataRow, int>(dv.Count);
      for (int i = 0; i < dv.Count; i++)
        allRowIndices.Add(dv[i].Row, i);

      List<int> UsedIndices = new List<int>(rows.Length); // некоторых строк может не быть
      for (int i = 0; i < rows.Length; i++)
      {
        int idx;
        if (allRowIndices.TryGetValue(rows[i], out idx))
          UsedIndices.Add(idx);
      }

      UpdateRows(UsedIndices.ToArray());
    }

    /// <summary>
    /// Обновить строку табличного просмотра, связанную с заданной строкой таблицы данных
    /// </summary>
    /// <param name="row">Строка связанной таблицы данных</param>
    public void UpdateDataRow(DataRow row)
    {
      if (row == null)
        return;

      DataView dv = SourceAsDataView;
      if (dv == null)
        throw new InvalidOperationException("Табличный просмотр не связан с набором данных");

      // Нет оптимального метода поиска
      for (int i = 0; i < dv.Count; i++)
      {
        if (dv[i].Row == row)
        {
          UpdateRows(new int[1] { i });
          return;
        }
      }
    }

    #endregion

    #region Порядок строк

    #region Предопределенная сортировка

    /// <summary>
    /// Описатели команд меню "Порядок строк" - массив объектов, задающих режимы
    /// предопределенной сортировки.
    /// </summary>
    public EFPDataViewOrders Orders
    {
      get
      {
        if (_Orders == null)
          _Orders = CreateOrders();
        return _Orders;
      }
    }
    private EFPDataViewOrders _Orders;

    /// <summary>
    /// Создает объект EFPDataGridViewOrders.
    /// </summary>
    /// <returns>Новый объект</returns>
    protected virtual EFPDataViewOrders CreateOrders()
    {
      return new EFPDataViewOrders();
    }

    /// <summary>
    /// Количество элементов в массиве Orders
    /// </summary>
    public int OrderCount
    {
      get
      {
        if (_Orders == null)
          return 0;
        else
          return _Orders.Count;
      }
    }

    /// <summary>
    /// Текущий порядок предопределенной сортировки (индекс в массиве Orders).
    /// Если используется произвольная сортировка сортировка не установлена, свойство возращает (-1).
    /// Для инициализации произвольной сортировки устанавливайте свойство CustomOrderActive.
    /// </summary>
    public int CurrentOrderIndex
    {
      get
      {
        if ((!_CustomOrderActive) && _CurrentOrder != null)
          return Orders.IndexOf(_CurrentOrder);
        else
          return -1;
      }
      set
      {
        if (value < 0 || value >= OrderCount)
          throw new ArgumentOutOfRangeException("value",
            "Индекс сортировки должен быть в диапазоне от 0 до " + (OrderCount - 1).ToString());


        _CurrentOrder = Orders[value];
        _CustomOrderActive = false;
        _CustomOrderAutoActivated = false;
        InternalSetCurrentOrder();
      }
    }

    private void InternalSetCurrentOrder()
    {
      if (ProviderState != EFPControlProviderState.Attached)
        return;

      EFPDataGridViewSelection OldSel = Selection;
      CommandItems.InitCurentOrder();
      if (WinFormsTools.AreControlAndFormVisible(Control))
      {
        if (OrderChangesToRefresh)
          PerformRefresh();
        else
        {
          if (AutoSort)
          {
            if (Control.DataSource != null)
              PerformAutoSort();
          }
          else
          {
            if (OrderChanged == null)
              throw new InvalidOperationException("OrderChangesToRefresh=false, AutoSort=false и событие OrderChanged не имеет обработчика");
            OnOrderChanged(EventArgs.Empty);
          }
        }
      }

      Selection = OldSel;

      OnCurrentOrderChanged(EventArgs.Empty);
      if (ProviderState == EFPControlProviderState.Attached)
        CommandItems.InitOrderItems();
      InitColumnHeaderTriangles();
    }

    #endregion

    #region Произвольная сортировка

    /// <summary>
    /// Разрешение использовать произвольную сортировку.
    /// По умолчанию - false - запрещено (если не переопределено в конструкторе производного класса).
    /// Свойство можно устанавливать только до вызова события Created.
    /// </summary>
    public bool CustomOrderAllowed
    {
      get { return _CustomOrderAllowed; }
      set
      {
        CheckHasNotBeenCreated();
        if (value == _CustomOrderAllowed)
          return;
        if ((!value) && CustomOrderActive)
          throw new InvalidOperationException("Свойство CustomOrderActive уже установлено");
        _CustomOrderAllowed = value;
      }
    }
    private bool _CustomOrderAllowed;

    /// <summary>
    /// True, если в текущий момент используется произвольная сортировка.
    /// Для установки в true, требуется сначала установить CustomOrderAllowed=true.
    /// 
    /// При переключении с предопределнной сортировки на произвольную проверяется с помощью IsSelectableCustomOrder(), что для текущей сортировки CurrentOrder в 
    /// просмотре имеются все столбцы, с помощью которых такую сортировку можно воспроизвести. Если столбцов не хватает,
    /// устанавливается произвольная сортировка по умолчанию. Если же и DefaultCustomOrder возвращает null, то переключение
    /// не выполняется и CustomOrderActive остается равным false.
    /// 
    /// При переключении с произвольной сортировки на предопределенную выполняется поиск в списке Orders порядка,
    /// соответствующему CurrentOrder. Если найден, то он устанавливается. Иначе выбирается первый элемент в списке Orders.
    /// Если список Orders пустой, устанавливается CurrentOrder=null.
    /// </summary>
    public bool CustomOrderActive
    {
      get { return _CustomOrderActive; }
      set
      {
        if (value == _CustomOrderActive)
          return;

        if (!_CustomOrderAllowed)
          throw new InvalidOperationException("Произвольная сортировка запрещена для этого просмотра");

        _CustomOrderActive = value;
        _CustomOrderAutoActivated = false;

        if (value)
        {
          // Если можно, сохраняем тот порядок, который был
          if (_CurrentOrder == null || (!IsSelectableCustomOrder(_CurrentOrder)))
          {
            _CurrentOrder = DefaultCustomOrder;
            if (_CurrentOrder == null) // на нашли сортировки по умолчанию
              _CustomOrderActive = false;
          }
        }
        else
        {
          bool found = false;
          if (CurrentOrder != null)
          {
            for (int i = 0; i < Orders.Count; i++)
            {
              if (Orders[i].Sort == CurrentOrder.Sort)
              {
                CurrentOrderIndex = i;
                found = true;
                break;
              }
            }
          }
          if (!found)
          {
            if (OrderCount > 0)
              CurrentOrderIndex = 0;
            else
              CurrentOrder = null;
          }
        }

        InitColumnSortMode();
        InternalSetCurrentOrder();
      }
    }
    private bool _CustomOrderActive;

    /// <summary>
    /// Возвращает порядок выборочной сортировки по умолчанию.
    /// Находится первый видимый столбец, для которого разрешена пользовательская сортировка.
    /// Возвращается новый EFPDataViewOrder для одного столбца с сортировкой по возрастанию.
    /// Если в просмотре нет ни одного столбца с разрешенной произвольной сортировкой, возвращается null.
    /// 
    /// Никаких изменений в просмотре не происходит
    /// </summary>
    /// <returns>EFPDataViewOrder или null</returns>
    public EFPDataViewOrder DefaultCustomOrder
    {
      get
      {
        EFPDataGridViewColumn[] cols = this.VisibleColumns;
        for (int i = 0; i < cols.Length; i++)
        {
          if (cols[i].CustomOrderAllowed)
            return InitOrderDisplayName(new EFPDataViewOrder(cols[i].Name));
        }

        return null;
      }
    }

    /// <summary>
    /// Возвращает true, если указанный порядок сортировки можно получить, нажимая на заголовки столбцов просмотра.
    /// Свойство EFPDataGidView.CustomOrderActive не проверяется. Учитываются только свойства столбцов EFPDatGridViewColumn.CustomOrderColumnName
    /// 
    /// Никаких изменений в просмотре не происходит
    /// </summary>
    /// <param name="order">Проверяемый порядок</param>
    /// <returns>Возможность его выбора</returns>
    public bool IsSelectableCustomOrder(EFPDataViewOrder order)
    {
      if (order == null)
        return false;

      // Словарик столбцов для сортировки
      Dictionary<string, EFPDataGridViewColumn> columnsForSort = new Dictionary<string, EFPDataGridViewColumn>();
      foreach (EFPDataGridViewColumn col in Columns)
      {
        if (col.CustomOrderAllowed)
          columnsForSort[col.CustomOrderColumnName] = col;
      }

      string[] colNames = DataTools.GetDataViewSortColumnNames(order.Sort);
      for (int i = 0; i < colNames.Length; i++)
      {
        if (!columnsForSort.ContainsKey(colNames[i]))
          return false;
      }
      return true;
    }

    #endregion

    #region Общее

    /// <summary>
    /// Текущий порядок сортировки строк.
    /// Содержит один из элементов в списке Orders, если используется предопределенная сортировка.
    /// Если используется произвольная сортировка, содержит актуальный объект EFPDataViewOrder.
    /// Установка свойства не меняет значения CustomOrderActive. Если CustomOrderActive имеет значение false,
    /// то присваиваимое значение должно быть из списка Orders. Если CustomOrderActive имеет значение true,
    /// то присваиваимое значение может быть любым, включая null. Переключения на предопределенную сортировку
    /// не будет, даже если значение совпадает с одним из элементов списка Orders.
    /// </summary>
    public EFPDataViewOrder CurrentOrder
    {
      get { return _CurrentOrder; }
      set
      {
        if (CustomOrderActive)
        {
          _CurrentOrder = value;
          InternalSetCurrentOrder();
        }
        else
        {
          if (value == null) // 20.07.2021
          {
            _CurrentOrder = value;
            InternalSetCurrentOrder();
          }
          else
          {
            int p = Orders.IndexOf(value);
            if (p < 0)
              throw new ArgumentException("Значение отсутствует в коллекции Orders", "value");
            CurrentOrderIndex = p;
          }
        }
      }
    }
    private EFPDataViewOrder _CurrentOrder;

    /// <summary>
    /// Текущий порядок сортировки строк в виде текста
    /// </summary>
    public string CurrentOrderName
    {
      get
      {
        if (CurrentOrder == null)
          return String.Empty;
        else
          return CurrentOrder.Name;
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          return;
        if (CustomOrderActive)
        {
          if (String.IsNullOrEmpty(value))
            CurrentOrder = null;
          else
          {
            string[] requiredColumns = DataTools.GetDataViewSortColumnNames(value);
            bool ok = true;
            for (int i = 0; i < requiredColumns.Length; i++)
            {
              EFPDataGridViewColumn col = Columns[requiredColumns[i]];
              if (col == null)
              {
                ok = false;
                break;
              }
              else if (!col.CustomOrderAllowed)
              {
                ok = false;
                break;
              }
            }
            if (ok)
              CurrentOrder = InitOrderDisplayName(new EFPDataViewOrder(value));
            else
              CurrentOrder = null;
          }
        }
        else
        {
          for (int i = 0; i < Orders.Count; i++)
          {
            if (Orders[i].Name == value)
            {
              CurrentOrderIndex = i;
              break;
            }
          }
        }
      }
    }

    /// <summary>
    /// Это извещение посылается после установки нового значения свойства 
    /// CurrentOrderIndex (или CurrentOrder / CurrentOrderName)
    /// </summary>
    public event EventHandler CurrentOrderChanged;

    /// <summary>
    /// Вызывается после установки нового значения свойства 
    /// CurrentOrderIndex (или CurrentOrder / CurrentOrderName).
    /// Непереопределенный метод вызывает событие CurrentOrderChanged.
    /// </summary>
    /// <param name="args">Пустой аргумент</param>
    protected virtual void OnCurrentOrderChanged(EventArgs args)
    {
      if (CurrentOrderChanged != null)
        CurrentOrderChanged(this, args);
    }

    /// <summary>
    /// Это событие должно обрабатывать установку порядка строк
    /// Событие не вызывается, если установлено свойство AutoSort или
    /// если вызывается метод PerformRefresh.
    /// Поэтому, обработчик Refresh также должен устанвливать порядок строки
    /// </summary>
    public event EventHandler OrderChanged;

    /// <summary>
    /// Вызывает событие OrderChanged
    /// </summary>
    /// <param name="args">Пустой аргумент</param>
    protected virtual void OnOrderChanged(EventArgs args)
    {
      if (OrderChanged != null)
        OrderChanged(this, args);
    }

    /// <summary>
    /// Если установить значение этого свойства в true, то будет вызываться событие
    /// Refresh при изменении порядка строк.
    /// </summary>
    public bool OrderChangesToRefresh { get { return _OrderChangesToRefresh; } set { _OrderChangesToRefresh = value; } }
    private bool _OrderChangesToRefresh;

    private bool _CustomOrderAutoActivated;

    /// <summary>
    /// Проверяет текущее значение CurrentOrder.
    /// Если в таблице нет полей, необходимых для сортировки,
    /// то устанавливается новое значение CurrentOrder.
    /// Также может измениться значение CustomOrderActive.
    /// 
    /// Метод вызывается после присоединения к просмотру таблицы данных
    /// </summary>
    private void ValidateCurrentOrder()
    {
#if DEBUG
      if (SourceAsDataTable == null)
        throw new InvalidOperationException("SourceAsDataTable==null");
      //if (!AutoSort)
      //  throw new InvalidOperationException("AutoSort=false");
#endif

      if (CustomOrderActive)
      {
        if (_CustomOrderAutoActivated && Orders.Count > 0)
        {
          string[] tableColumnNames = DataTools.GetColumnNames(SourceAsDataTable);
          if (Orders[0].AreAllColumnsPresented(tableColumnNames))
          {
            CurrentOrderIndex = 0;
            return;
          }
        }
        if (CurrentOrder != null)
        {
          string[] tableColumnNames = DataTools.GetColumnNames(SourceAsDataTable);
          if (CurrentOrder.AreAllColumnsPresented(tableColumnNames))
            return;
        }

        bool oldCustomOrderAutoActivated = _CustomOrderAutoActivated;
        CurrentOrder = DefaultCustomOrder;
        _CustomOrderAutoActivated = oldCustomOrderAutoActivated;
      }
      else
      {
        if (CurrentOrder != null)
        {
          string[] tableColumnNames = DataTools.GetColumnNames(SourceAsDataTable);
          if (CurrentOrder.AreAllColumnsPresented(tableColumnNames))
            return;
        }
        else if (OrderCount > 0)
        {
          string[] tableColumnNames = DataTools.GetColumnNames(SourceAsDataTable);
          if (Orders[0].AreAllColumnsPresented(tableColumnNames))
          {
            CurrentOrderIndex = 0;
            return;
          }
        }
        if (CustomOrderAllowed)
        {
          EFPDataViewOrder defOrd = DefaultCustomOrder;
          if (defOrd == null)
            CurrentOrder = null; // не устанавливаем CustomOrderActive=true;
          else
          {
            CustomOrderActive = true;
            CurrentOrder = defOrd;
          }
          _CustomOrderAutoActivated = true;
        }
      }
    }


    /// <summary>
    /// Если установить свойство в true, то не будет вызываться событие CurrentOrderChanged
    /// Вместо этого при переключении порядка сортировки пользователеми будет 
    /// вызываться метод PerformAutoSort() для установки значения DataView.ViewOrder
    /// </summary>
    public bool AutoSort { get { return _AutoSort; } set { _AutoSort = value; } }
    private bool _AutoSort;

    private static bool _Inside_PerformAutoSort; // 09.09.2020

    /// <summary>
    /// Установить значение DataView.ViewOrder
    /// Используйте этот метод в событии Refresh
    /// </summary>
    public void PerformAutoSort()
    {
      if (_Inside_PerformAutoSort)
        return;
      _Inside_PerformAutoSort = true;
      try
      {
        if (Visible && HasBeenCreated)
        {
          if (SelectedRowsMode == EFPDataGridViewSelectedRowsMode.None || SelectedRowsMode == EFPDataGridViewSelectedRowsMode.RowIndex)
            DoPerformAutoSort(); // не восстанавливаем выбранную позицию
          else
          {
            EFPDataGridViewSelection oldSel = this.Selection;
            DoPerformAutoSort();
            this.Selection = oldSel;
          }
        }
        else
          DoPerformAutoSort(); // не восстанавливаем выбранную позицию
      }
      finally
      {
        _Inside_PerformAutoSort = false;
      }
    }

    private void DoPerformAutoSort()
    {
      if (CurrentOrder == null)
        SourceAsDataView.Sort = String.Empty; // 16.07.2021
      else
        CurrentOrder.PerformSort(this);
    }

    void Control_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs args)
    {
      try
      {
        DoControl_ColumnHeaderMouseClick(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка нажатия мыши на заголовке табличного просмотра");
      }
    }

    void DoControl_ColumnHeaderMouseClick(DataGridViewCellMouseEventArgs args)
    {
      EFPApp.ShowTempMessage(null);
      if (args.ColumnIndex < 0)
        return;
      if (args.Button != MouseButtons.Left)
        return;
      string clickedColumnName = Columns[args.ColumnIndex].Name;
      if (String.IsNullOrEmpty(clickedColumnName))
        return; // на всякий случай

      bool res;
      if (CustomOrderActive)
        res = DoControl_ColumnHeaderMouseClickCustom(clickedColumnName);
      else
        res = DoControl_ColumnHeaderMouseClickPredefined(clickedColumnName);
      if (res)
      {
        // 01.07.2021
        // Если в просмотре выделена ячейка, которая не видна на экране на момент щелчка, то после сортировки
        // просмотр будет прокручен, чтобы эта ячейка попала в просмотр.
        // Это может быть неудобно.
        // По возможности, переключаемся на ячейку, по которой щелнкули
        switch (Control.SelectionMode)
        {
          case DataGridViewSelectionMode.CellSelect:
          case DataGridViewSelectionMode.RowHeaderSelect:
            if (IsCurrentRowSingleSelected)
              CurrentColumnName = clickedColumnName;
            break;
          case DataGridViewSelectionMode.FullColumnSelect:
            CurrentColumnName = clickedColumnName;
            break;
        }
      }
    }

    private bool DoControl_ColumnHeaderMouseClickPredefined(string clickedColumnName)
    {
      if (OrderCount == 0)
      {
        if (CustomOrderAllowed)
        {
          EFPApp.ShowTempMessage("Нет предопределенных порядков сортировки. Переключитесь на произвольный порядок строк");
          return false;
        }
        else
        {
          EFPApp.ShowTempMessage("Сортировка не предусмотрена");
          return false;
        }
      }
      if (CurrentOrderIndex >= 0)
      {
        if (GetUsedColumnName(Orders[CurrentOrderIndex]) == clickedColumnName)
        {
          // Щелкнутый столбец уже является текущим столбцом сортировки
          for (int i = CurrentOrderIndex + 1; i < Orders.Count; i++)
          {
            if (GetUsedColumnName(Orders[i]) == clickedColumnName)
            {
              // Есть еще один такой столбец
              CurrentOrderIndex = i;
              return true;
            }
          }
          for (int i = 0; i < CurrentOrderIndex; i++)
          {
            if (GetUsedColumnName(Orders[i]) == clickedColumnName)
            {
              // Есть еще один такой столбец
              CurrentOrderIndex = i;
              return false;
            }
          }
          EFPApp.ShowTempMessage("Строки уже отсортированы по этому столбцу");
          return false;
        }
      }
      int OrderIndex = Orders.IndexOfItemForGridColumn(clickedColumnName);
      if (OrderIndex >= 0)
      {
        CurrentOrderIndex = OrderIndex;
        return true;
      }

      EFPApp.ShowTempMessage("Для этого столбца нет сортировки. Выберите порядок строк из меню");
      return false;
    }

    private bool DoControl_ColumnHeaderMouseClickCustom(string clickedColumnName)
    {
      EFPDataGridViewColumn col = Columns[clickedColumnName];
      if (!col.CustomOrderAllowed)
      {
        EFPApp.ShowTempMessage("Нельзя выполнять сортировку по столбцу " + col.GridColumn.HeaderText);
        return false;
      }

      bool subMode = (System.Windows.Forms.Control.ModifierKeys == Keys.Control);

      string[] currColNames;
      ListSortDirection[] currDirs;
      DataTools.GetDataViewSortColumnNames(CurrentOrderName, out currColNames, out currDirs);

      if (subMode)
      {
        if (currColNames.Length == 0)
        {
          EFPApp.ShowTempMessage("В данный момент сортировка не выполнена. Не нажимайте <Ctrl>");
          return false;
        }

        if (currColNames[currColNames.Length - 1] == col.CustomOrderColumnName)
        {
          // Переключаем направление
          InvertLastSortDirection(currDirs);
          CurrentOrder = InitOrderDisplayName(new EFPDataViewOrder(DataTools.GetDataViewSort(currColNames, currDirs)));
          return true;
        }
        if (Array.IndexOf<string>(currColNames, col.CustomOrderColumnName) >= 0)
        {
          EFPApp.ShowTempMessage("Нельзя добавить столбец к сортировке, так как он уже в ней участвует");
          return false;

        }
        currColNames = DataTools.MergeArrays<string>(currColNames, new string[] { col.CustomOrderColumnName });
        currDirs = DataTools.MergeArrays<ListSortDirection>(currDirs, new ListSortDirection[] { ListSortDirection.Ascending });
        CurrentOrder = InitOrderDisplayName(new EFPDataViewOrder(DataTools.GetDataViewSort(currColNames, currDirs)));
      }
      else
      {
        if (currColNames.Length == 1)
        {
          if (currColNames[0] == col.CustomOrderColumnName)
          {
            InvertLastSortDirection(currDirs);
            CurrentOrder = InitOrderDisplayName(new EFPDataViewOrder(DataTools.GetDataViewSort(currColNames, currDirs)));
            return true;
          }
        }
        CurrentOrder = InitOrderDisplayName(new EFPDataViewOrder(col.CustomOrderColumnName));
      }
      return true;
    }

    private static void InvertLastSortDirection(ListSortDirection[] currDirs)
    {
      if (currDirs[currDirs.Length - 1] == ListSortDirection.Ascending)
        currDirs[currDirs.Length - 1] = ListSortDirection.Descending;
      else
        currDirs[currDirs.Length - 1] = ListSortDirection.Ascending;
    }

    private EFPDataViewOrder InitOrderDisplayName(EFPDataViewOrder order)
    {
      order.DisplayName = GetOrderDisplayName(order);
      return order;
    }

    /// <summary>
    /// Возвращает подходящее отображаемое имя для произвольного порядка сортировки.
    /// Используются заголовки столбцов табличного просмотра.
    /// </summary>
    /// <param name="order">Произвольный порядок сортировки</param>
    /// <returns>Отображаемое название</returns>
    public string GetOrderDisplayName(EFPDataViewOrder order)
    {
      if (order == null)
        return String.Empty;

      // Словарик столбцов для сортировки
      Dictionary<string, EFPDataGridViewColumn> columnsForSort = new Dictionary<string, EFPDataGridViewColumn>();
      foreach (EFPDataGridViewColumn col in Columns)
      {
        if (col.CustomOrderAllowed)
          columnsForSort[col.CustomOrderColumnName] = col;
      }

      string[] colNames;
      ListSortDirection[] dirs;
      DataTools.GetDataViewSortColumnNames(order.Sort, out colNames, out dirs);

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < colNames.Length; i++)
      {
        if (i > 0)
          sb.Append(", ");
        EFPDataGridViewColumn col;
        if (columnsForSort.TryGetValue(colNames[i], out col))
          sb.Append(col.DisplayName);
        else
          sb.Append(colNames[i]);

        if (colNames.Length == 1)
        {
          if (dirs[i] == ListSortDirection.Ascending)
            sb.Append(" (по возрастанию)");
          else
            sb.Append(" (по убыванию)");
        }
        else
        {
          if (dirs[i] == ListSortDirection.Ascending)
            sb.Append(" (возр.)");
          else
            sb.Append(" (убыв.)");
        }
      }
      return sb.ToString();
    }

    /// <summary>
    /// Должен ли EFPDataGridView управлять свойствами DataGridViewColumn.SortMode и DataGridViewColumnHeaderCell.SortGlyphDirection
    /// для рисования треугольников в заголовках столбцов, и выполнять сортировку по нажатию кнопки мыши.
    /// По умолчанию - true - обработка разрешена. При этом, если OrderCount=0 и CustomOrderAllowed=false,
    /// щелчок по заголовку выдает сообщение в статусной строке "Сортировка невозможна".
    /// Установка значения false восстанавливает поведение DataGridView по умолчанию для автоматической сортировки.
    /// Свойство можно устанавливать только до появления события Created.
    /// </summary>
    public bool UseColumnHeaderClick
    {
      get { return _UseColumnHeaderClick; }
      set
      {
        CheckHasNotBeenCreated();
        _UseColumnHeaderClick = value;
      }
    }
    private bool _UseColumnHeaderClick;

    /// <summary>
    /// Инициализация свойства DataGridViewColumn.SortMode для всех столбцов
    /// </summary>
    private void InitColumnSortMode()
    {
      if (!UseColumnHeaderClick)
        return;

      if (CustomOrderActive)
      {
        foreach (EFPDataGridViewColumn col in Columns)
        {
          if (col.CustomOrderAllowed)
            col.GridColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
          else
            col.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
        }
      }
      else
      {
        foreach (EFPDataGridViewColumn col in Columns)
          col.GridColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

        foreach (EFPDataViewOrder order in Orders)
        {
          EFPDataGridViewColumn col = GetUsedColumn(order);
          if (col != null)
            col.GridColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
        }
      }

      CommandItems.InitOrderItems();
      InitColumnHeaderTriangles();
    }

    /// <summary>
    /// Прорисовка трегольников сортировки в заголовках столбов в соответствии с 
    /// текущим порядком сортировки CurrentOrder
    /// </summary>
    private void InitColumnHeaderTriangles()
    {
      if (ProviderState != EFPControlProviderState.Attached)
        return;
      if (!UseColumnHeaderClick)
        return;

      foreach (DataGridViewColumn Col in Control.Columns)
        Col.HeaderCell.SortGlyphDirection = SortOrder.None;

      if (CurrentOrder != null)
      {
        if (CustomOrderActive)
        {
          // Словарик столбцов для сортировки
          Dictionary<string, EFPDataGridViewColumn> columnsForSort = new Dictionary<string, EFPDataGridViewColumn>();
          foreach (EFPDataGridViewColumn col in Columns)
          {
            if (col.CustomOrderAllowed)
              columnsForSort[col.CustomOrderColumnName] = col;
          }

          string[] columnNames;
          ListSortDirection[] directions;
          DataTools.GetDataViewSortColumnNames(CurrentOrder.Sort, out columnNames, out directions);
          for (int i = 0; i < columnNames.Length; i++)
          {
            EFPDataGridViewColumn Col;
            if (columnsForSort.TryGetValue(columnNames[i], out Col))
            {
              try
              {
                if (directions[i] == ListSortDirection.Descending)
                  Col.GridColumn.HeaderCell.SortGlyphDirection = SortOrder.Descending;
                else
                  Col.GridColumn.HeaderCell.SortGlyphDirection = SortOrder.Ascending;
              }
              catch (Exception e)
              {
                e.Data["DislayName"] = this.DisplayName;
                e.Data["ColumnName"] = Col.Name;
                e.Data["CustomOrderActive"] = CustomOrderActive;
                try
                {
                  Form frm = Control.FindForm();
                  if (frm != null)
                    e.Data["Form"] = frm.ToString();
                }
                catch { }

                FreeLibSet.Logging.LogoutTools.LogoutException(e, "Ошибка установки DataGridViewColumn.SortGlyphDirection");
              }
            }
          }
        }
        else // предопределенная конфигурация
        {
          EFPDataGridViewColumn Col = GetUsedColumn(CurrentOrder);
          if (Col != null && Col.GridColumn.SortMode != DataGridViewColumnSortMode.NotSortable)
          {
            try
            {
              if (CurrentOrder.SortInfo.Direction == ListSortDirection.Descending)
                Col.GridColumn.HeaderCell.SortGlyphDirection = SortOrder.Descending;
              else
                Col.GridColumn.HeaderCell.SortGlyphDirection = SortOrder.Ascending;
            }
            catch (Exception e)
            {
              e.Data["DislayName"] = this.DisplayName;
              e.Data["ColumnName"] = Col.Name;
              e.Data["CustomOrderActive"] = CustomOrderActive;
              try
              {
                Form frm = Control.FindForm();
                if (frm != null)
                  e.Data["Form"] = frm.ToString();
              }
              catch { }

              FreeLibSet.Logging.LogoutTools.LogoutException(e, "Ошибка установки DataGridViewColumn.SortGlyphDirection");
            }
          }
        }
      }
    }

    /// <summary>
    /// Показать диалог выбора порядка строк из массива Orders
    /// Если пользователь выбирает порядок, то устанавливается свойство CurrentOrder
    /// </summary>
    /// <returns></returns>
    public bool ShowSelectOrderDialog()
    {
#if XXX
      if (OrderCount == 0)
      {
        EFPApp.ShowTempMessage("Нельзя выбрать порядок строк в табличном просмотре");
        return false;
      }
      int idx = CurrentOrderIndex;
      if (!Orders.ShowSelectDialog(ref idx))
        return false;
      CurrentOrderIndex = idx;
      return true;
#else

      if (OrderCount == 0 && (!CustomOrderAllowed))
      {
        EFPApp.ShowTempMessage("Нельзя выбрать порядок строк в табличном просмотре");
        return false;
      }

      return EFPDataGridViewOrderForm.PerformSort(this);

#endif
    }

    #endregion

    #endregion

    #region Индекс используемого столбца для порядка сортировки

    /// <summary>
    /// Возвращает индекс столбца табличного просмотра (DataGridViewColumn.Index), который будет использоваться для щелчка мыши для заданного порядка сортировки.
    /// Если в просмотре нет ни одного подходящего столбца, возвращается (-1)
    /// </summary>
    /// <param name="item">Порядок сортировки</param>
    /// <returns>Индекс столбца</returns>
    public int IndexOfUsedColumnName(EFPDataViewOrder item)
    {
      if (item == null)
        return (-1);

      if (item.SortInfo.IsEmpty)
        return -1;

      int BestColumnIndex = -1;
      int BestDisplayIndex = -1;
      for (int i = 0; i < item.SortInfo.ClickableColumnNames.Length; i++)
      {
        int ColumnIndex = Columns.IndexOf(item.SortInfo.ClickableColumnNames[i]);
        if (ColumnIndex < 0)
          continue;
        DataGridViewColumn Col = Control.Columns[ColumnIndex];
        if (!Col.Visible)
          continue; // скрытые столбцы не считаются

        int DisplayIndex = Col.DisplayIndex;
        if (BestColumnIndex < 0 || DisplayIndex < BestDisplayIndex)
        {
          BestColumnIndex = ColumnIndex;
          BestDisplayIndex = DisplayIndex;
        }
      }

      return BestColumnIndex;
    }

    /// <summary>
    /// Возвращает имя столбца табличного просмотра (EFPDataGridViewColumn.Name), который будет использоваться для щелчка мыши для заданного порядка сортировки.
    /// Если в просмотре нет ни одного подходящего столбца, возвращается пустая строка
    /// </summary>
    /// <param name="item">Порядок сортировки</param>
    /// <returns>Индекс столбца</returns>
    public string GetUsedColumnName(EFPDataViewOrder item)
    {
      int p = IndexOfUsedColumnName(item);
      if (p < 0)
        return String.Empty;
      else
        return Columns[p].Name;
    }

    /// <summary>
    /// Возвращает столбец EFPDataGridViewColumn, который будет использоваться для щелчка мыши для заданного порядка сортировки.
    /// Если в просмотре нет ни одного подходящего столбца, возвращается null
    /// </summary>
    /// <param name="item">Порядок сортировки</param>
    /// <returns>Индекс столбца</returns>
    public EFPDataGridViewColumn GetUsedColumn(EFPDataViewOrder item)
    {
      int p = IndexOfUsedColumnName(item);
      if (p < 0)
        return null;
      else
        return Columns[p];
    }

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Список команд локального меню.
    /// </summary>
    public new EFPDataGridViewCommandItems CommandItems { get { return (EFPDataGridViewCommandItems)(base.CommandItems); } }

    /// <summary>
    /// Создает EFPDataGridViewCommandItems
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      return new EFPDataGridViewCommandItems(this);
    }

    /// <summary>
    /// Делает командой, выделенной по умолчанию, команду "Edit" или "View"
    /// </summary>
    protected override void OnBeforePrepareCommandItems()
    {
      base.OnBeforePrepareCommandItems();
      if ((!CommandItems.EnterAsOk) && CommandItems.DefaultCommandItem == null)
      {
        EFPCommandItem ci = CommandItems["Edit", "Edit"];
        if (ci.Visible)
          CommandItems.DefaultCommandItem = ci;
        else
        {
          ci = CommandItems["Edit", "View"];
          if (ci.Visible)
            CommandItems.DefaultCommandItem = ci;
        }
      }

      //// 01.12.2020
      //// При выполнением 
      //CommandItems.AddClickHandler(ResetCurrentIncSearchColumn);
    }

    #endregion

    #region Поддержка Inline-редактирования

    /// <summary>
    /// Встроенное выполнение операций редактирования без использования
    /// внешних обработчиков.
    /// Поддерживаются режимы "Редактирование" и "Удаление"
    /// В режиме Edit вызывается DataGridView.BeginEdit(), при этом проверяются свойства ReadOnly
    /// </summary>
    /// <param name="state">Edit или Delete</param>
    public void InlineEditData(EFPDataGridViewState state)
    {
      switch (state)
      {
        case EFPDataGridViewState.Edit:
          // 09.01.2013
          // Лучше лишний раз проверить режим ReadOnly и выдать ShowTempMessage(),
          // т.к. обычный BeginEdit() не будет выдавать сообщение, если для столбца
          // установлено ReadOnly=true

          if (Control.IsCurrentCellInEditMode)
          {
            EFPApp.ShowTempMessage("Ячейка уже редактируется");
            return;
          }
          string ReadOnlyMessage;
          if (GetCellReadOnly(Control.CurrentCell, out ReadOnlyMessage))
          {
            EFPApp.ShowTempMessage(ReadOnlyMessage);
            return;
          }

          Control.BeginEdit(true);
          break;
        case EFPDataGridViewState.Delete:
          if (WholeRowsSelected && Control.AllowUserToDeleteRows)
          {
            DataGridViewRow[] Rows = SelectedGridRows;
            if (Rows.Length == 0)
              EFPApp.ShowTempMessage("Нет выбранных строк для удаления");
            else
            {
              for (int i = 0; i < Rows.Length; i++)
                Control.Rows.Remove(Rows[i]);
            }
          }
          else
            ClearSelectedCells();
          break;
        default:
          EFPApp.ShowTempMessage("Редактирование по месту для режима \"" +
            state.ToString() + "\" не реализовано");
          break;
      }
    }

    /// <summary>
    /// Очищает значения в выбранных ячейках.
    /// Если хотя бы одна из ячеек не разрешает редактирование, выдается сообщение
    /// об ошибке и выделяется эта ячейка
    /// </summary>
    public void ClearSelectedCells()
    {
      EFPDataGridViewRectArea Area = new EFPDataGridViewRectArea(Control, EFPDataGridViewRectAreaCreation.Selected);
      ClearCells(Area);
    }

    /// <summary>
    /// Очищает значения в указанной области ячеек.
    /// Если хотя бы одна из ячеек не разрешает редактирование, выдается сообщение
    /// об ошибке и выделяется эта ячейка.
    /// </summary>
    /// <param name="area">Область строк и столбцов</param>
    public void ClearCells(EFPDataGridViewRectArea area)
    {
      if (area.IsEmpty)
      {
        EFPApp.ShowTempMessage("Нет выбранных ячеек");
        return;
      }

      #region Первый проход. Проверяем возможность удаления

      // Проверяем, что мы можем записать пустой текст во все ячейки
      for (int i = 0; i < area.RowCount; i++)
      {
        for (int j = 0; j < area.ColumnCount; j++)
        {
          DataGridViewCell Cell = area[j, i];
          if (!Cell.Selected)
            continue;

          string ErrorText;
          if (GetCellReadOnly(Cell, out ErrorText))
          {
            Control.CurrentCell = Cell;
            EFPApp.ShowTempMessage(ErrorText);
            return;
          }

          if (!TrySetTextValue(Cell, String.Empty, out ErrorText, true, EFPDataGridViewCellFinishedReason.Clear))
          {
            Control.CurrentCell = Cell;
            EFPApp.ShowTempMessage(ErrorText);
            return;
          }
        }
      }

      #endregion

      #region Второй проход. Удаляем значения

      for (int i = 0; i < area.Rows.Count; i++)
      {
        for (int j = 0; j < area.Columns.Count; j++)
        {
          DataGridViewCell Cell = area[j, i];
          if (!Cell.Selected)
            continue;

          SetTextValue(Cell, String.Empty, EFPDataGridViewCellFinishedReason.Edit);
        }
      }

      #endregion
    }

    /*
     * Проблема.
     * Если просмотр находится в режиме inline-редактирования (в ячейке), а форма,
     * на которой расположен просмотр, имеет кнопку "Отмена" (установлено свойство
     * TheForm.CancelButton), то при нажатии пользователем клавиши Esc, вместо отмены
     * редактирования и возврата к навигации по ячейкам, как ожидалось, 
     * редактирование ячейки сохраняется, а форма закрывается.
     * 
     * Где перехватить нажатие клавиши Esc - неизвестно.
     * 
     * Решение.
     * На время нахождения просмотра в режиме inline-редактирования будем отключать
     * свойство TheForm.CancelButton, не забывая его восстановить. Для этого 
     * обрабатываем события CellBeginEdit и CellEndEdit. Первое из них имеет поле
     * Cancel, которое позволяет не начинать редактирование. В этом случае отключение
     * кнопки "Отмена" не должно выполняться, т.к. иначе форма перестанет реагировать
     * на Esc совсем. Следовательно, наш обработчик события CellBeginEdit должен 
     * располагаться в конце цепочки обработчиков, чтобы можно было анализировать
     * поле Args.Cancel. Поэтому обработчик присоедияется не в конструкторе объекта,
     * а в методе SetCommandItems()
     * 
     */


    void Control_CellBeginEdit1(object sender, DataGridViewCellCancelEventArgs args)
    {
      try
      {
        DoControl_CellBeginEdit1(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Control_CellBeginEdit");
      }
    }
    void DoControl_CellBeginEdit1(DataGridViewCellCancelEventArgs args)
    {
      if (args.Cancel)
        return;
      if (args.ColumnIndex < 0)
        return; // никогда не бывает

      string ReadOnlyMessage;
      bool RO = GetCellReadOnly(args.RowIndex, args.ColumnIndex, out ReadOnlyMessage);

      if (RO)
      {
        args.Cancel = true;
        DataGridViewColumn Column = Control.Columns[args.ColumnIndex];
        if (!(Column is DataGridViewCheckBoxColumn))
          EFPApp.ShowTempMessage(ReadOnlyMessage);
      }
    }


    /// <summary>
    /// Сохраняем свойство Form.CancelButton на время редактирования ячейки "по месту", чтобы
    /// нажатие клавиши "Esc" не приводило сразу к закрытию кнопки
    /// </summary>
    private IButtonControl _SavedCancelButton;

    void Control_CellBeginEdit2(object sender, DataGridViewCellCancelEventArgs args)
    {
      try
      {
        DoControl_CellBeginEdit2(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Control_CellBeginEdit");
      }
    }
    void DoControl_CellBeginEdit2(DataGridViewCellCancelEventArgs args)
    {
      if (args.Cancel)
        return;

      if (args.ColumnIndex < 0)
        return; // никогда не бывает

      DataGridViewColumn Column = Control.Columns[args.ColumnIndex];
      if (Column is DataGridViewCheckBoxColumn) // 04.11.2009 - для CheckBox-не надо
        return;
      _SavedCancelButton = Control.FindForm().CancelButton;
      Control.FindForm().CancelButton = null;
    }

#if XXX
    void Control_CellEndEdit1(object Sender, DataGridViewCellEventArgs Args)
    {
      try
      {
        DoControl_CellEndEdit1(Args);
      }
      catch (Exception e)
      {
        DebugTools.ShowException(e, "Control_CellEndEdit");
      }
    }

    void DoControl_CellEndEdit1(DataGridViewCellEventArgs Args)
    {
    }
#endif

    void Control_CellEndEdit2(object sender, DataGridViewCellEventArgs args)
    {
      try
      {
        if (_SavedCancelButton != null)
        {
          Control.FindForm().CancelButton = _SavedCancelButton;
          _SavedCancelButton = null;
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Control_CellEndEdit");
      }
    }

    void Control_CellContentClick(object sender, DataGridViewCellEventArgs args)
    {
      // Обработка столбцов флажков
      // Стандартное поведение неудобно. При нажатии на флажок, строка переходит
      // в режим редактирования, при этом значение не будет передано в набор данных,
      // пока пользователь не перейдет к другой строке

      try
      {
        DoControl_CellContentClick(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработки щелчка мыши на содержимом ячейки табличного просмотра");
      }
    }

    private void DoControl_CellContentClick(DataGridViewCellEventArgs args)
    {
      if (args.RowIndex < 0 || args.ColumnIndex < 0)
        return;

      if (Control[args.ColumnIndex, args.RowIndex] is DataGridViewCheckBoxCell)
      {
        string ReadOnlyMessage;
        bool RO = GetCellReadOnly(args.RowIndex, args.ColumnIndex, out ReadOnlyMessage);
        if (RO && GetCellContentVisible(args.RowIndex, args.ColumnIndex))
          EFPApp.ShowTempMessage(ReadOnlyMessage);
      }
    }

    private void Control_CurrentCellDirtyStateChanged(object sender, EventArgs args)
    {
      try
      {
        if (Control.CurrentCell is DataGridViewCheckBoxCell || Control.CurrentCell is DataGridViewComboBoxCell)
          Control.CommitEdit(DataGridViewDataErrorContexts.Commit);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработки события CurrentCellDirtyStateChanged");
      }
    }


    void Control_CellParsing(object sender, DataGridViewCellParsingEventArgs args)
    {
      try
      {
        DoControl_CellParsing(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработки события CellParsing");
      }
    }

    private void DoControl_CellParsing(DataGridViewCellParsingEventArgs args)
    {
      if (args.ParsingApplied)
        return;

      DataGridViewCell Cell = Control[args.ColumnIndex, args.RowIndex];
      if (Cell is DataGridViewTextBoxCell)
      {
        if (args.Value is String)
        {
          // 16.08.2012
          // Разрешаем пользователю затирать значение ячейки не только нажатием <Ctrl>+<0>,
          // но и пробелом. При этом введенное значение равно " " (или строка из нескольких
          // пробелов). 
          // Я не знаю, как это сделать красивее, т.к. требуется преобразовать Value
          // к заданному типу. Нельзя сделать Value=InheritedCellStyle.NullValue,
          // т.к. в этом случае все равно будет вызван стандартный обработчик, который
          // преобразует исходную строку еще раз
          string s = args.Value.ToString().Trim();
          if (s.Length == 0)
          {
            args.InheritedCellStyle.NullValue = args.Value;
            args.ParsingApplied = false;
          }
          else TryParseNumbers(s, args);
        }
      }
    }

    /// <summary>
    /// 13.12.2012
    /// Попытка преобразовать числа с плавающей точкой.
    /// Замена "." на "," или наоборот
    /// </summary>
    /// <param name="s"></param>
    /// <param name="args"></param>
    private bool TryParseNumbers(string s, DataGridViewCellParsingEventArgs args)
    {
      if (!DataTools.IsFloatType(args.DesiredType))
        return false; // не число с плавающей точкой

      WinFormsTools.CorrectNumberString(ref s, args.InheritedCellStyle.FormatProvider);
      DataGridViewCell Cell = Control[args.ColumnIndex, args.RowIndex];
      try
      {
        args.Value = Cell.ParseFormattedValue(s, args.InheritedCellStyle, null, null);
      }
      catch
      {
        return false;
      }
      args.ParsingApplied = true;
      return true;
    }

    void Control_CellValidating(object sender, DataGridViewCellValidatingEventArgs args)
    {
      try
      {
        DoControl_CellValidating(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка при обработк события CellValidating");
        args.Cancel = true;
      }
    }

    private void DoControl_CellValidating(DataGridViewCellValidatingEventArgs args)
    {
      if (args.Cancel)
        return;

      DataGridViewCell Cell = Control[args.ColumnIndex, args.RowIndex];
      if (!Control.IsCurrentCellInEditMode)
        return;

      // 15.12.2012
      // Проверяем корректность маски
      IMaskProvider MaskProvider = Columns[args.ColumnIndex].MaskProvider;
      if (MaskProvider != null && Cell.ValueType == typeof(string))
      {
        string s = args.FormattedValue.ToString().Trim();
        if (s.Length > 0)
        {
          string ErrorText;
          if (!MaskProvider.Test(args.FormattedValue.ToString(), out ErrorText))
          {
            EFPApp.ShowTempMessage(ErrorText);
            args.Cancel = true;
          }
        }
      }
    }


    void Control_CellValueChanged(object sender, DataGridViewCellEventArgs args)
    {
      try
      {
        DoControl_CellValueChanged(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработки события CellValueChanged");
      }
    }

    private bool InsideCellValueChanged = false;
    private void DoControl_CellValueChanged(DataGridViewCellEventArgs Args)
    {
      if (InsideCellValueChanged)
        return;

      InsideCellValueChanged = true;
      try
      {
        if (Control.IsCurrentCellInEditMode)
        {
          if (Control.CurrentCell.RowIndex == Args.RowIndex && Control.CurrentCell.ColumnIndex == Args.ColumnIndex)
          {
            OnCellFinished(Args.RowIndex, Args.ColumnIndex, EFPDataGridViewCellFinishedReason.Edit);
            Control.InvalidateRow(Args.RowIndex);
          }
        }
      }
      finally
      {
        InsideCellValueChanged = false;
      }
    }

    /// <summary>
    /// Событие вызывается, когда закончено редактирование ячейки и новое значение
    /// Cell.Value установлено (вызывается из CellValueChanged). Событие не 
    /// вызывается, если пользователь нажал Esc.
    /// Обработчик может, например, выполнить расчет других значений строки или
    /// расчет итоговой строки.
    /// Также событие вызывается после вставки значений из буфера обмена и
    /// установки / снятия отметки строки (MarkRow)
    /// </summary>
    public event EFPDataGridViewCellFinishedEventHandler CellFinished;

    private bool _CellFinishedErrorShown = false;

    /// <summary>
    /// Вызов события CellFinished
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <param name="columnIndex"></param>
    /// <param name="reason"></param>
    public virtual void OnCellFinished(int rowIndex, int columnIndex, EFPDataGridViewCellFinishedReason reason)
    {
      if (CellFinished == null)
        return;

      EFPDataGridViewCellFinishedEventArgs Args = new EFPDataGridViewCellFinishedEventArgs(this,
        rowIndex, columnIndex, reason);
      try
      {
        CellFinished(this, Args);
      }
      catch (Exception e)
      {
        if (!_CellFinishedErrorShown)
        {
          _CellFinishedErrorShown = true;
          EFPApp.ShowException(e, "Ошибка в обработчике события CellFinished");
        }
      }

      Validate();
    }


    /// <summary>
    /// Возвращает true, если заданная ячейка имеет установленный атрибут "только чтение"
    /// </summary>
    /// <param name="cell">Ячейка</param>
    /// <param name="readOnlyMessage">Сюда помещается текст, почему нельзя редактировать ячейку</param>
    /// <returns>true, если ячейку нельзя редактировать</returns>
    public bool GetCellReadOnly(DataGridViewCell cell, out string readOnlyMessage)
    {
      if (cell == null)
      {
        readOnlyMessage = "Ячейка не выбрана";
        return true;
      }
      return GetCellReadOnly(cell.RowIndex, cell.ColumnIndex, out readOnlyMessage);
    }

    /// <summary>
    /// Возвращает true, если заданная ячейка имеет установленный атрибут "только чтение".
    /// В первую очередь вызывается GetRowReadOnly() для проверки атрибута строки.
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <param name="readOnlyMessage">Сюда помещается текст, почему нельзя редактировать ячейку</param>
    /// <returns>true, если ячейку нельзя редактировать</returns>
    public bool GetCellReadOnly(int rowIndex, int columnIndex, out string readOnlyMessage)
    {
      if (GetRowReadOnly(rowIndex, out readOnlyMessage))
        return true;

      DoGetCellAttributes(columnIndex);
      readOnlyMessage = _GetCellAttributesArgs.ReadOnlyMessage;
      return _GetCellAttributesArgs.ReadOnly;
    }

    /// <summary>
    /// Возвращает true, если заданная строка имеет установленный атрибут "только чтение".
    /// Также может быть задан атрибут для отдельных ячеек.
    /// Обычно следует использовать метод GetCellReadOnly().
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="readOnlyMessage">Сюда помещается текст, почему нельзя редактировать строку</param>
    /// <returns>true, если ячейку нельзя редактировать</returns>
    public bool GetRowReadOnly(int rowIndex, out string readOnlyMessage)
    {
      if (Control.ReadOnly)
      {
        readOnlyMessage = "Просмотр предназначен только для чтения";
        return true;
      }

      if (rowIndex < 0 || rowIndex >= Control.RowCount)
      {
        readOnlyMessage = "Неправильный номер строки";
        return true;
      }

      DoGetRowAttributes(rowIndex, EFPDataGridViewAttributesReason.ReadOnly);
      readOnlyMessage = _GetRowAttributesArgs.ReadOnlyMessage;
      return _GetRowAttributesArgs.ReadOnly;
    }

    /// <summary>
    /// Запрашивает атрибуты строки и ячейки в режиме View и возвращает признак
    /// ContentVisible.
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <returns>false -  если содержимое ячейки не должно отображаться</returns>
    public bool GetCellContentVisible(int rowIndex, int columnIndex)
    {
      DoGetRowAttributes(rowIndex, EFPDataGridViewAttributesReason.View);
      return DoGetCellAttributes(columnIndex).ContentVisible;
    }


    #endregion

    #region Вставка текста из буфера обмена

    /// <summary>
    /// Выполняет вставку текста в прямоугольную область.
    /// Левая верхняя ячейка области определяется текущем выделением ячейки.
    /// Если вставка не может быть выполнена, выдается сообщение об ошибке и возвращается false
    /// </summary>
    /// <param name="textArray">Вставляемый текст (строки и столбцы)</param>
    /// <returns>true, если вставка выполнена</returns>
    public bool PerformPasteText(string[,] textArray)
    {
      string ErrorText;
      if (DoPasteText(textArray, false, out ErrorText))
        return true;
      else
      {
        EFPApp.ShowTempMessage(ErrorText);
        return false;
      }
    }

    /// <summary>
    /// Проверяет возможность вставки текста в прямоугольную область.
    /// Левая верхняя ячейка области определяется текущем выделением ячейки.
    /// </summary>
    /// <param name="textArray">Вставляемый текст (строки и столбцы)</param>
    /// <returns>true, если вставка может быть выполнена</returns>
    public bool TestCanPasteText(string[,] textArray)
    {
      string ErrorText;
      return TestCanPasteText(textArray, out ErrorText);
    }


    /// <summary>
    /// Проверяет возможность вставки текста в прямоугольную область.
    /// Левая верхняя ячейка области определяется текущем выделением ячейки.
    /// </summary>
    /// <param name="textArray">Вставляемый текст (строки и столбцы)</param>
    /// <param name="errorText">Сюда помещается пояснение, если вставка недоступна</param>
    /// <returns>true, если вставка выполнена</returns>
    public bool TestCanPasteText(string[,] textArray, out string errorText)
    {
      return DoPasteText(textArray, true, out errorText);
    }

    /// <summary>
    /// Выполнение вставки текста в прямоугольную область или проверка возможности вставки.
    /// </summary>
    /// <param name="textArray">Вставляемый текст</param>
    /// <param name="testOnly">true - только тестирование</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <returns>true, если вставка возможна или выполнена</returns>
    protected virtual bool DoPasteText(string[,] textArray, bool testOnly, out string errorText)
    {
      #region DataGridView.ReadOnly

      if (Control.ReadOnly)
      {
        errorText = "Табличный просмотр не допускает редактирование";
        return false;
      }

      #endregion

      #region Область вставки

      // Прямоугольный массив a содержит значения для вставки
      int nY1 = textArray.GetLength(0);
      int nX1 = textArray.GetLength(1);

      if (nX1 == 0 || nY1 == 0)
      {
        errorText = "Текст не задан";
        return false; // 27.12.2020
      }

      Rectangle Rect = SelectedRectAddress;
      int nX2 = Rect.Width;
      int nY2 = Rect.Height;
      if (nX2 == 0 || nY2 == 0)
      {
        errorText = "Нет выбранной ячейки";
        return false;
      }

      if (nX2 > 1 || nY2 > 1)
      {
        // В текущий момент выбрано больше одной ячейки.
        // Текст в буфере должен укладываться ровное число раз
        if (nX1 > nX2 || nY1 > nY2)
        {
          /*
          EFPApp.ShowTempMessage("Текст в буфере обмена содержит " +
            RusNumberConvert.IntWithNoun(nY1, "строку", "строки", "строк") + " и " +
            RusNumberConvert.IntWithNoun(nX1, "столбец", "столбца", "столбцов") +
            ", а выбрано меньше: " +
            RusNumberConvert.IntWithNoun(nY2, "строка", "строки", "строк") + " и " +
            RusNumberConvert.IntWithNoun(nX2, "столбец", "столбца", "столбцов"));
           * */

          errorText = "Текст в буфере обмена содержит строк: " +
            nY1.ToString() + " и столбцов: " + nX1.ToString() +
            ", а выбрано меньше: строк: " + nY2.ToString() + " и столбцов: " + nX2.ToString();
          return false;
        }

        if ((nX2 % nX1) != 0 || (nY2 % nY1) != 0)
        {
          /*
          EFPApp.ShowTempMessage("Текст в буфере обмена содержит " +
            RusNumberConvert.IntWithNoun(nY1, "строку", "строки", "строк") + " и " +
            RusNumberConvert.IntWithNoun(nX1, "столбец", "столбца", "столбцов") +
            ", а выбрано " +
            RusNumberConvert.IntWithNoun(nY2, "строка", "строки", "строк") + " и " +
            RusNumberConvert.IntWithNoun(nX2, "столбец", "столбца", "столбцов") +
            ". Не делится нацело");
           * */
          errorText = "Текст в буфере обмена содержит строк: " +
            nY1.ToString() + " и столбцов: " + nX1.ToString() +
            ", а выбрано строк: " + nY2.ToString() + " и столбцов: " + nX2.ToString() +
            ". Не делится нацело";
          return false;
        }
      }
      else
      {
        Rect = new Rectangle(Rect.Location, new Size(nX1, nY1));
        if (((Rect.Bottom - 1) >= Control.RowCount && (!AutoAddRowsAllowed)) || (Rect.Right - 1) >= Control.ColumnCount)
        {
          /*
          EFPApp.ShowTempMessage("Текст в буфере обмена содержит " +
            RusNumberConvert.IntWithNoun(nY1, "строку", "строки", "строк") + " и " +
            RusNumberConvert.IntWithNoun(nX1, "столбец", "столбца", "столбцов") +
            " - не помещается");
           * */
          errorText = "Текст в буфере обмена содержит строк: " + nY1.ToString() + " и столбцов:" + nX1.ToString() +
            " - не помещается";
          return false;
        }

        if ((!testOnly) && (Rect.Bottom - 1) >= Control.RowCount)
        {
          int addCount = Rect.Bottom - Control.RowCount + 1;
#if DEBUG
          if (addCount <= 0)
            throw new BugException("addCount=" + addCount.ToString());
#endif
          AddInsuficientRows(addCount);
        }
      }

      // Rect содержит прямоугольник заменяемых значений

      #endregion

      #region Опрос свойства ReadOnly ячеек и возможности вставить значения

      for (int i = Rect.Top; i < Rect.Bottom; i++)
      {
        if (testOnly && i >= Control.RowCount)
          break;

        for (int j = Rect.Left; j < Rect.Right; j++)
        {
          DataGridViewCell Cell = Control[j, i];
          if (GetCellReadOnly(i, j, out errorText))
          {
            Control.CurrentCell = Cell;
            return false;
          }

          int i1 = (i - Rect.Top) % nY1;
          int j1 = (j - Rect.Left) % nX1;

          if (!TrySetTextValue(Cell, textArray[i1, j1], out errorText, true, EFPDataGridViewCellFinishedReason.Paste))
          {
            Control.CurrentCell = Cell;
            return false;
          }
        }
      }

      //Handler.Control.CancelEdit();

      //if (OldCell != null)
      //  Handler.Control.CurrentCell = OldCell;
      //Handler.Control.CancelEdit();

      #endregion

      #region Записываем значение

      if (!testOnly)
      {
        for (int i = Rect.Top; i < Rect.Bottom; i++)
        {
          for (int j = Rect.Left; j < Rect.Right; j++)
          {
            DataGridViewCell Cell = Control[j, i];
            int i1 = (i - Rect.Top) % nY1;
            int j1 = (j - Rect.Left) % nX1;

            SetTextValue(Cell, textArray[i1, j1], EFPDataGridViewCellFinishedReason.Paste);
          }
        }

        Control.EndEdit();
      }

      #endregion

      errorText = null;
      return true;
    }

    /// <summary>
    /// Свойство должно возвращать true, если разрешается автоматическое добавление недостающих строк при вставке текста
    /// из буфера обмена. При этом должен также быть переопределен метод AddInsuficientRows().
    /// Реализовано в классе EFPInputGridView.
    /// По умолчанию - false - строки не добавляются.
    /// </summary>
    protected virtual bool AutoAddRowsAllowed { get { return false; } }

    /// <summary>
    /// Метод должен добавлять указанное число строк в конец просмотра при вставке текста из буфера обмена.
    /// Метод должен быть переопределен, если AutoAddRowsAllowed возвращает true.
    /// Реализовано в классе EFPInputGridView.
    /// Непереопределенный метод выбрасывает исключение.
    /// </summary>
    /// <param name="addCount"></param>
    protected virtual void AddInsuficientRows(int addCount)
    {
      throw new NotImplementedException();
    }

    #endregion

    #region События

    /// <summary>
    /// Вызывается для редактирования данных, просмотра, вставки, копирования
    /// и удаления строк
    /// </summary>
    public event EventHandler EditData;

    /// <summary>
    /// Вызывает обработчик события EditData и возвращает true, если он установлен
    /// Если метод возвращает false, выполняется редактирование "по месту"
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    protected virtual bool OnEditData(EventArgs args)
    {
      if (EditData != null)
      {
        EditData(this, args);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Вызывается при нажатии кнопки "Обновить", а также при смене
    /// фильтра (когда будет реализовано)
    /// </summary>
    public event EventHandler RefreshData;

    /// <summary>
    /// Вызывает событие RefreshData.
    /// Вызывайте этот метод из OnRefreshData(), если Вы полностью переопределяете его, не вызывая
    /// метод базового класса.
    /// </summary>
    protected void CallRefreshDataEventHandler(EventArgs args)
    {
      if (RefreshData != null)
        RefreshData(this, args);
    }

    /// <summary>
    /// Непереопределенный метод вызывает событие RefreshData.
    /// Если метод переопределен, также должно быть переопределено свойство HasRefreshDataHandler
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnRefreshData(EventArgs args)
    {
      CallRefreshDataEventHandler(args);
    }

    /// <summary>
    /// Возвращает true, если есть установленный обработчик события RefreshData
    /// </summary>
    public virtual bool HasRefreshDataHandler { get { return RefreshData != null; } }

    bool IEFPDataView.UseRefresh { get { return CommandItems.UseRefresh; } }

    /// <summary>
    /// Принудительное обновление просмотра.
    /// Обновление выполняется, даже если CommandItems.UseRefresh=false.
    /// Сохраняется и восстанавливается, если возможно, текущее выделение в просмотре с помощью свойства Selection
    /// </summary>
    public virtual void PerformRefresh()
    {
      if (Control.IsDisposed)
        return; // 27.03.2018
      EFPDataGridViewSelection OldSel = null;
      if (ProviderState == EFPControlProviderState.Attached) // 04.07.2021
        OldSel = Selection;


#if XXX
      for (int i = 0; i < Columns.Count; i++)
      // Сброс буферизации GridProducerColumn
      {
        if (Columns[i].ColumnProducer != null)
          Columns[i].ColumnProducer.Refresh();
      }
#endif

      // Вызов пользовательского события
      OnRefreshData(EventArgs.Empty);

      IncSearchDataView = null; // больше недействителен (???)

      if (OldSel != null)
      {
        try
        {
          Selection = OldSel;
        }
        catch (Exception e)
        {
          EFPApp.MessageBox("Возникла ошибка при восстановлении выбранных строк при обновлении таблицы. " +
            e.Message, "Ошибка табличного просмотра", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }

      CommandItems.PerformRefreshItems();
    }

    /// <summary>
    /// Вызов метода PerformRefresh(), который можно выполнять из обработчика события
    /// </summary>
    /// <param name="sender">Игнорируется</param>
    /// <param name="args">Игнеорируется</param>
    public void PerformRefresh(object sender, EventArgs args)
    {
      PerformRefresh();
    }

    #endregion

    #region Методы

    /// <summary>
    /// Перевод просмотра в один из режимов
    /// </summary>
    public void PerformEditData(EFPDataGridViewState state)
    {
      if (_InsideEditData)
      {
        EFPApp.ShowTempMessage("Предыдущее редактирование еще не выполнено");
        return;
      }
      // Любая операция редактирования останавливает поиск по первым буквам
      CurrentIncSearchColumn = null;

      bool Res;
      // Пытаемся вызвать специальный обработчик в GridProducerColum
      if (CurrentColumn != null)
      {
        if (CurrentColumn.ColumnProducer != null)
        {
          _InsideEditData = true;
          _State = state;
          try
          {
            EFPDataViewRowInfo rowInfo = this.GetRowInfo(Control.CurrentCellAddress.Y);
            Res = CurrentColumn.ColumnProducer.PerformCellEdit(rowInfo, CurrentColumnName);
            FreeRowInfo(rowInfo);
          }
          finally
          {
            _State = EFPDataGridViewState.View;
            _InsideEditData = false;
          }
          if (Res)
            return;
        }
      }

      _InsideEditData = true;
      _State = state;
      try
      {
        Res = OnEditData(EventArgs.Empty);
        if (CommandItems != null)
          CommandItems.PerformRefreshItems();
      }
      finally
      {
        _State = EFPDataGridViewState.View;
        _InsideEditData = false;
      }

      if (!Res)
        InlineEditData(state);
    }
    /// <summary>
    /// Предотвращение повторного запуска редактора
    /// </summary>
    private bool _InsideEditData;

    /// <summary>
    /// Присвоить текстовое значение ячейке.
    /// Операция выполнима, даже если ячейка имеет свойство ReadOnly.
    /// Выполняется преобразование в нужный тип данных и проверка коррекстности
    /// (соответствие маске, если она задана для столбца). Если преобразование
    /// невозможно, вызывается исключение
    /// </summary>
    /// <param name="cell">Ячейка просмотра</param>
    /// <param name="textValue">Записываемое значение</param>
    /// <param name="reason">Причина</param>
    public void SetTextValue(DataGridViewCell cell, string textValue,
        EFPDataGridViewCellFinishedReason reason)
    {
      string ErrorText;
      if (!TrySetTextValue(cell, textValue, out ErrorText, false, reason))
        throw new InvalidOperationException("Нельзя записать значение \"" + textValue + "\". " + ErrorText);
    }

    /// <summary>
    /// Попытка присвоить текстовое значение ячейке.
    /// Операция считается выполнимой, даже если ячейка имеет свойство ReadOnly.
    /// Следует также выполнять проверку методом GetCellReadOnly()
    /// Выполняется преобразование в нужный тип данных и проверка корректности
    /// (соответствие маске, если она задана для столбца). Если преобразование
    /// невозможно, возвращается false и текст сообщения об ошибке
    /// </summary>
    /// <param name="cell">Ячейка просмотра</param>
    /// <param name="textValue">Записываемое значение</param>
    /// <param name="errorText">Сюда помещается сообщение об ошибке</param>
    /// <param name="testOnly">Если true, то только выполняется проверка возможности записи</param>
    /// <param name="reason">Причина</param>
    /// <returns>true, если значение успешно записано или могло быть записано</returns>
    public bool TrySetTextValue(DataGridViewCell cell, string textValue, out string errorText, bool testOnly,
        EFPDataGridViewCellFinishedReason reason)
    {
      errorText = null;
      if (String.IsNullOrEmpty(textValue))
      {
        if (!testOnly)
        {
          cell.Value = cell.InheritedStyle.DataSourceNullValue;
          OnCellFinished(cell.RowIndex, cell.ColumnIndex, reason);
          Control.InvalidateRow(cell.RowIndex);
        }
        return true;
      }

      Type ValueType;
      if (cell.ValueType != null)
        ValueType = cell.ValueType;
      else
        ValueType = typeof(string);

      IMaskProvider MaskProvider = Columns[cell.ColumnIndex].MaskProvider;

      if (DataTools.IsFloatType(ValueType))
        WinFormsTools.CorrectNumberString(ref textValue);
      else if (ValueType == typeof(string) && MaskProvider != null)
      {
        if (!MaskProvider.Test(textValue, out errorText))
          return false;
      }

      try
      {
        object v = Convert.ChangeType(textValue, ValueType);
        if (!testOnly)
        {
          if (ValueType == typeof(DateTime))
          {
            // 30.03.2021, 13.04.2021
            // Если формат не предусматривает ввода времени, его требуется убрать
            if (!FormatStringTools.ContainsTime(cell.InheritedStyle.Format))
              v = ((DateTime)v).Date;
          }
          cell.Value = v;
          OnCellFinished(cell.RowIndex, cell.ColumnIndex, reason);
          Control.InvalidateRow(cell.RowIndex);
        }
      }
      catch (Exception e)
      {
        errorText = e.Message;
        return false;
      }

      return true;
    }

    #endregion

    #region Обработка события DataError

    /// <summary>
    /// Класс исключения для отладки перехваченной ошибки в событии DataGridView.DataError
    /// </summary>
    private class DataGridViewDataErrorException : ApplicationException
    {
      #region Конструктор

      public DataGridViewDataErrorException(DataGridView control, DataGridViewDataErrorEventArgs args)
        : base("Ошибка DataGridView.DataError (" + args.Context.ToString() + ")", args.Exception)
      {
        _Control = control;
        _Arguments = args;
      }

      #endregion

      #region Свойства

      public DataGridView Control { get { return _Control; } }
      private readonly DataGridView _Control;

      //[DesignerSerializationVisibility(DesignerSerializationVisibility.VisibleEx)]
      //public DataGridViewDataErrorEventArgs Arguments { get { return _Arguments; } }
      private readonly DataGridViewDataErrorEventArgs _Arguments;

      public int RowIndex { get { return _Arguments.RowIndex; } }

      public int ColumnIndex { get { return _Arguments.ColumnIndex; } }

      public DataGridViewDataErrorContexts Context { get { return _Arguments.Context; } }

      #endregion
    }

    /// <summary>
    /// Использовать ли обработку события DataGridView.DataError по умолчанию
    /// Если свойство установлено в true, то обработчик события выдает отладочное окно
    /// и выводит в log-файл специальный тип исключения о возникшей ошибке.
    /// Если свойство установлено в false, то никакие действия не выполняются и
    /// сообщения об ошибке не выдается (включая стандартное). При этом предполагается,
    /// что имеется дополнительный пользовательский обработчик события DataGridView.DataError,
    /// который может вызывать метод DefaultShowDataError() для выдачи сообщения.
    /// Значение по умолчанию - true
    /// </summary>
    public bool UseDefaultDataError { get { return _UseDefaultDataError; } set { _UseDefaultDataError = value; } }
    private bool _UseDefaultDataError;

    void Control_DataError(object sender, DataGridViewDataErrorEventArgs args)
    {
      //try
      //{
      //  throw new DataGridViewDataErrorException((DataGridView)Sender, Args);
      //}
      //catch (Exception Args)
      //{
      //  ShowExceptionForm.ShowException(Args, Args.Message);
      //}

      try
      {
        if (UseDefaultDataError)
          DefaultShowDataError(sender, args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Control_DataError");
      }
    }

    private bool _FormatDisplayErrorWasShown = false;

    /// <summary>
    /// Выдача сообщения об ошибке в обработчике DataGridView.DataError
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void DefaultShowDataError(object sender, DataGridViewDataErrorEventArgs args)
    {
      DataGridView control = (DataGridView)sender;
      if ((args.Context & (DataGridViewDataErrorContexts.Commit | DataGridViewDataErrorContexts.Parsing)) ==
        (DataGridViewDataErrorContexts.Commit | DataGridViewDataErrorContexts.Parsing))
      {
        // Введено неправильное значение в поле. Вместо большого сообщения об ошибке
        // выдаем всплывающее сообщение внизу экрана
        DataGridViewCell cell = control.Rows[args.RowIndex].Cells[args.ColumnIndex];
        EFPApp.ShowTempMessage("Ошибка преобразования введенного значения \"" + cell.EditedFormattedValue.ToString() + "\"");
        return;
      }

      // 16.10.2013
      // Для Formatting без Display сообщение тоже выдаем один раз
      //if (Args.Context == (DataGridViewDataErrorContexts.Display | DataGridViewDataErrorContexts.Formatting))
      if ((args.Context & DataGridViewDataErrorContexts.Formatting) == DataGridViewDataErrorContexts.Formatting)
      {
        if (_FormatDisplayErrorWasShown)
          return; // сообщение выдается только один раз
        _FormatDisplayErrorWasShown = true;
        Exception e1 = new DataGridViewDataErrorException(control, args);
        EFPApp.ShowException(e1, "Ошибка форматирования для ячейки RowIndex=" + args.RowIndex.ToString() + " и ColIndex=" + args.ColumnIndex.ToString());
        return;
      }

      Exception e2 = new DataGridViewDataErrorException(control, args);
      EFPApp.ShowException(e2, e2.Message);
    }

    #endregion

    #region Перенаправление событий отдельным столбцам

    void Control_CellClick(object sender, DataGridViewCellEventArgs args)
    {
      try
      {
        DoControl_CellClick(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Control_CellClick");
      }
    }
    void DoControl_CellClick(DataGridViewCellEventArgs args)
    {
      EFPApp.ShowTempMessage(null);
      CommandItems.PerformRefreshItems();
      if (args.ColumnIndex < 0 || args.RowIndex < 0)
        return;
      EFPDataGridViewColumn Column = Columns[args.ColumnIndex];
      if (Column.ColumnProducer == null)
        return;
      EFPDataViewRowInfo rowInfo = GetRowInfo(args.RowIndex);
      Column.ColumnProducer.PerformCellClick(rowInfo, Column.Name);
      FreeRowInfo(rowInfo);
    }

    #endregion

    #region Раскрашивание ячеек

    /*
     * Проект
     * ------
     * Цветовые атрибуты ячеек для табличного просмотра и печати
     * 
     * Назначение.
     * Система свойств предназначена для перехвата и расширения возможностей
     * события DataGridView.CellFormatting.
     * - Единый стиль цветового оформления ячеек в зависимости от типа данных в
     *   ячейке (обычные данные, итого и подытоги, и т.д.) для всего приложения
     * - Преобразование цветовых атрибутов в выделение при печати таблицы
     * - Доступ к форматированию значения, т.к. станартное событи CellFormatting
     *   не позволяет получить доступ к отформатированному значению вне DataGridView
     *   и производных классов.
     * - Дополнительные атрибуты группировки свойств при печати
     * 
     * Чтобы использовать цветовые атрибуты, должен быть установлен один или оба обработчика
     * события GetRowAttributes и GetCellAttributes.
     * Эти события вызываются изнутри стандартного события DataGridView.CellFormatting.
     * Событие GetRowAttributes вызывается один раз для всей строки, а 
     * GetCellAttributes - для каждой ячейки. GetRowAttributes вызывется перед
     * GetCellAttributes. Значения, заданные в GetRowAttributes, копируются перед
     * каждым вызовом GetCellAttributes.
     * Оба события передают через аргумент Args следующие параметры
     * RowIndex - номер строки в наборе данных. Это аргумент всегда больше или равен
     * нулю, иначе событие не вызывается. Если требуется получить доступ к строке
     * таблицы данных, можно использовать метод GetDataRow(), который вернет 
     * соответствующий объект DataRow.
     * Свойство ColorType позволяет задать один из возможных типов оформления ячейки
     * (обычный, специальный, итоговая строка или подытоги, заголовок)
     * Свойство Grayed позволяет задать приглушенный цвет ячеек
     * 
     * Событие GetRowAttributes, кроме общих, поддерживает следующие свойства:
     * PrintWithPrevious - Логическое значение, которое при установке в true 
     * предотвращает при печати отрыв этой строки от предыдущей.
     * PrintWithNext - аналогично PrintWithPrevious, но не отрывать от следующей.
     * При наличии большого числа слепленных строк разрыв все равно будет выполнен,
     * если все эти строки нельзя разместить на странице.
     * 
     * Событие GetCellAttributes, кроме общих, поддерживает следующие свойства
     * ColumnIndex - номер столбца. Метод GetColumn() обеспечивает доступ к объекту
     * столбца, при необходимости.
     * ValueEx, FormattingApplied - передаются непосредственно из CellFormatting
     */

    #region Событие GetRowAttributes

    /// <summary>
    /// Событие вызывается перед рисованием строки и должно устанавливать общие атрибуты
    /// для ячеек строки.
    /// Если будет использоваться вывод сообщений об ошибках по строке, то должно быть установлено свойство
    /// UserRowImages
    /// </summary>
    public event EFPDataGridViewRowAttributesEventHandler GetRowAttributes;

    /// <summary>
    /// Вызов события GetRowAttributes.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnGetRowAttributes(EFPDataGridViewRowAttributesEventArgs args)
    {
      if (GetRowAttributes != null)
        GetRowAttributes(this, _GetRowAttributesArgs);
    }


    /// <summary>
    /// Постоянно используем единственный объект аргумента для событий
    /// </summary>
    private EFPDataGridViewRowAttributesEventArgs _GetRowAttributesArgs;

    /// <summary>
    /// Общедоступный метод получения атрибутов строки
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="reason"></param>
    /// <returns>Атрибуты строки</returns>
    public EFPDataGridViewRowAttributesEventArgs DoGetRowAttributes(int rowIndex, EFPDataGridViewAttributesReason reason)
    {
      _GetRowAttributesArgs.InitRow(rowIndex, reason);
      OnGetRowAttributes(_GetRowAttributesArgs);
      return _GetRowAttributesArgs;
    }

    #endregion

    #region Событие GetCellAttributes

    /// <summary>
    /// Событие вызывается перед рисованием ячейки и должно выполнить форматирование
    /// значения и установить цветовые атрибуты ячейки
    /// </summary>
    public event EFPDataGridViewCellAttributesEventHandler GetCellAttributes;

    /// <summary>
    /// Вызывает событие GetCellAttributes.
    /// Этот метод вызывается только после предварительного вызова OnGetRowAttributes() (возможно,
    /// несколько раз для разных столбцов), в котором указываются индекс строки и причина вызова.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnGetCellAttributes(EFPDataGridViewCellAttributesEventArgs args)
    {
      if (GetCellAttributes != null)
        GetCellAttributes(this, _GetCellAttributesArgs);
    }

    private bool _Inside_OnGetCellAttributes; // 06.02.2018 - блокировка вложенного вызова

    private void CallOnGetCellAttributes(int columnIndex, DataGridViewCellStyle cellStyle, bool styleIsTemplate, object originalValue)
    {
      if (_Inside_OnGetCellAttributes)
        return;
      _Inside_OnGetCellAttributes = true;
      try
      {
        _GetCellAttributesArgs.InitCell(_GetRowAttributesArgs, columnIndex, cellStyle, styleIsTemplate, originalValue);
        OnGetCellAttributes(_GetCellAttributesArgs);
      }
      finally
      {
        _Inside_OnGetCellAttributes = false;
      }
    }

    private EFPDataGridViewCellAttributesEventArgs _GetCellAttributesArgs;

    /// <summary>
    /// Общедоступный метод получения атрибутов ячейки.
    /// Перед вызовами метода должен идти один вызов DoGetRowAttributes() для строки
    /// </summary>
    /// <param name="_ColumnIndex">Номер столбца</param>
    /// <returns>Атрибуты ячейки</returns>
    public EFPDataGridViewCellAttributesEventArgs DoGetCellAttributes(int _ColumnIndex)
    {
      // // Избегаем делать строки Unshared
      // DataGridViewRow Row = Control.RowList.SharedRow(GetRowAttributesArgs.RowIndex);
      // DataGridViewCell Cell = Row.Cells[ColumnIndex];
      // !!! Так не работает. Если строка Unshared, то ее RowIndex=-1 и свойство
      // Cell вызывает ошибку при получении значения.

      // 25.06.2021. Исследование исходников Net Framework.
      // 1. DataGridView выполняет рисование строк во внутреннем методе методе PaintRows(), который получает "разделяемую" строку SharedRow().
      // 2. Вызывается protected virtual метод DataGridViewRow.Paint().
      // 3. Вызывается protected virtual метод DataGridViewRow.PaintCells().
      // 4. Вызывается internal-метод DataGridViewCell.PaintWork(), ячейка относится к shared-строке.
      // 5. Для получения значения вызывается DataGridViewCell.GetValue(). Он является protected virtual. 
      // Ему передается индекс строки, но реальный, а не (-1), как у SharedRow.
      // Есть также internal-метод GetValueInternal(), он вызывается из DataGridView, но не нашел, как его вызвать косвенно.
      // 6. Метод DataGridViewCell.GetValue() - весьма сложный. Он либо возвращает хранимое значение, либо вызывает
      // DataGridView.OnCellValueNeeded(), либо обращается к связанному набору данных.
      // В принципе, это можно повторить, но только если нет переопределения метода GetValue() в какой-нибудь ячейке нестандартного класса.
      // Свойство DataGridViewCell.Value вызывает метод GetValue() передавая DataGridViewRow.Index в качестве номера строки.
      // У SharedRow он равен (-1), что вызовет исключение.
      // 7. Метод SharedRow() возвращает один и тот же объект DataGridViewRow(), если строка является Unshared. Его нельзя никак настроить,
      // чтобы он мог вернуть значение ячейки. 
      // И почему нельзя было сделать метод DataGridViewCell.GetValue() общедоступным?

      DataGridViewCell Cell = Control[_ColumnIndex, _GetRowAttributesArgs.RowIndex];

      // 30.06.2021
      // Если строкое значение получено из DataRow, которая была загружена из базы данных, 
      // то значение может содержать концевые пробелы
      string sValue = Cell.Value as String;
      if (sValue != null)
        _GetCellAttributesArgs.Value = sValue.TrimEnd();
      else
        _GetCellAttributesArgs.Value = Cell.Value;
      _GetCellAttributesArgs.FormattingApplied = false;

      CallOnGetCellAttributes(_ColumnIndex, Cell.InheritedStyle, true, _GetCellAttributesArgs.Value);

      if (_GetCellAttributesArgs.Reason == EFPDataGridViewAttributesReason.View ||
        _GetCellAttributesArgs.Reason == EFPDataGridViewAttributesReason.Print) // 06.02.2018
      {
        _GetCellAttributesArgs.MoveTemplateToStyle();

        // Убрано 25.06.2021.
        // Непонятно, зачем было нужно. Было всегда (в 2008 году, в программе cs1, Modules\ClientAccDep\GridHandler.cs).

        // Если значение возвращается из EFPGridProducerColumn, то не устанавливается свойство EFPDataGridViewCellAttributesEventArgs.FormattingApplied.
        // Неприятность возникает после экспорта в Excel/Calc. Если для столбца задано форматирование с разделителем тысяч ("#,##0"), то
        // в Excel попадает форматированное значение-строка, которое не преобразуется в число из-за пробелов.

        //if (!_GetCellAttributesArgs.FormattingApplied)
        //  _GetCellAttributesArgs.Value = Cell.FormattedValue;
      }
      return _GetCellAttributesArgs;
    }

    #endregion

    private bool _InsideRowPaint = false;

    private bool _RowPrePaintErrorMessageWasShown = false;

    void Control_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs args)
    {
      try
      {
        DoControl_RowPrePaint(args);
      }
      catch (Exception e)
      {
        if (!_RowPrePaintErrorMessageWasShown)
        {
          EFPApp.ShowException(e, "Control_RowPrePaint");
          _RowPrePaintErrorMessageWasShown = true;
        }
      }
    }
    void DoControl_RowPrePaint(DataGridViewRowPrePaintEventArgs args)
    {
      if (!_InsideRowPaint)
      {
        _InsideRowPaint = true;
        DoGetRowAttributes(args.RowIndex, EFPDataGridViewAttributesReason.View);

        // 15.09.2015
        // Для невиртуального табличного просмотра событие CellToolTipTextNeeded не вызывается.
        // Соответственно, не появится всплывающая подсказка по ошибке для строки.
        // Для такого просмотра устанавливаем ее в явном виде
        if ((!Control.VirtualMode) && Control.DataSource == null && UseRowImages)
        {
          Control.Rows[args.RowIndex].HeaderCell.ToolTipText = _GetRowAttributesArgs.ToolTipText;
        }
      }
      else
      {
        args.Graphics.FillRectangle(Brushes.Black, args.RowBounds);
        args.Handled = true;
        //System.Threading.Thread.Sleep(100);
      }
    }

    void Control_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs args)
    {
      _InsideRowPaint = false;
    }

    private void Control_CellPainting(object sender, DataGridViewCellPaintingEventArgs args)
    {
      try
      {
        if (args.ColumnIndex == -1)
          DoControl_RowHeaderCellPainting(args);
        else if (args.RowIndex >= 0)
        {
          if (args.CellStyle.ForeColor == Color.Transparent)
          {
            args.Paint(args.ClipBounds, args.PaintParts & (~DataGridViewPaintParts.ContentForeground));
          }
          else
          {
            args.Paint(args.ClipBounds, args.PaintParts);

            if (_GetCellAttributesArgs.DiagonalUpBorder >= EFPDataGridViewBorderStyle.Thin)
              args.Graphics.DrawLine(CreateBorderPen(args.CellStyle.ForeColor, _GetCellAttributesArgs.DiagonalUpBorder),
                args.CellBounds.Left, args.CellBounds.Bottom - 1, args.CellBounds.Right - 1, args.CellBounds.Top);
            if (_GetCellAttributesArgs.DiagonalDownBorder >= EFPDataGridViewBorderStyle.Thin)
              args.Graphics.DrawLine(CreateBorderPen(args.CellStyle.ForeColor, _GetCellAttributesArgs.DiagonalDownBorder),
                args.CellBounds.Left, args.CellBounds.Top, args.CellBounds.Right - 1, args.CellBounds.Bottom - 1);

            args.Handled = true;
          }
        }
      }
      catch (Exception e)
      {
        if (!_RowPrePaintErrorMessageWasShown)
        {
          EFPApp.ShowException(e, "Control_CellPaint");
          _RowPrePaintErrorMessageWasShown = true;
        }
      }
    }

    private static Pen CreateBorderPen(Color color, EFPDataGridViewBorderStyle borderStyle)
    {
      float w;
      if (borderStyle == EFPDataGridViewBorderStyle.Thick)
        w = 2;
      else
        w = 1;
      return new Pen(color, w);
    }



    void Control_CellFormatting(object sender, DataGridViewCellFormattingEventArgs args)
    {
      if (args.RowIndex < 0)
        return;
      if (args.ColumnIndex < 0)
        return;
      try
      {
        // Если таблица получена из базы данных, то у строковых полей часто идут пробелы для заполнения по ширине поля.
        // Если их не убрать, то значение "не влезет" в ячейку и будут символы "..." в правой части столбца
        // 22.04.2021
        // В отличие от предыдущей реализации, думаю, можно не выполнять лишние проверки.
        // Вряд ли где-нибудь могут понадобиться концевые пробелы в строке.
        string sValue = args.Value as String;
        if (sValue != null)
          args.Value = sValue.TrimEnd();

        EFPDataGridViewColumn ColInfo = this.Columns[args.ColumnIndex];

        // Форматирование значения и получение цветовых атрибутов
        _GetCellAttributesArgs.Value = args.Value;
        _GetCellAttributesArgs.FormattingApplied = args.FormattingApplied;
        CallOnGetCellAttributes(args.ColumnIndex, args.CellStyle, false, args.Value);
        args.Value = _GetCellAttributesArgs.Value;
        args.FormattingApplied = _GetCellAttributesArgs.FormattingApplied;
        // Установка цветов в просмотре
        SetCellAttr(args.CellStyle, _GetCellAttributesArgs.ColorType, _GetCellAttributesArgs.Grayed, _GetCellAttributesArgs.ContentVisible);

        if (GrayWhenDisabled && (!ControlEnabled))
        {
          // 18.07.2019
          args.CellStyle.BackColor = SystemColors.ControlLight;
          args.CellStyle.ForeColor = SystemColors.GrayText;
          args.CellStyle.SelectionBackColor = SystemColors.ControlDark;
          args.CellStyle.SelectionForeColor = SystemColors.ControlLight;
        }

        if (HideSelection && (!Control.Focused)/* 18.07.2019 */)
        {
          args.CellStyle.SelectionBackColor = args.CellStyle.BackColor;
          args.CellStyle.SelectionForeColor = args.CellStyle.ForeColor;
        }
        // Отступ
        if (_GetCellAttributesArgs.IndentLevel > 0)
        {
          Padding pd = args.CellStyle.Padding;
          switch (args.CellStyle.Alignment)
          {
            case DataGridViewContentAlignment.TopLeft:
            case DataGridViewContentAlignment.MiddleLeft:
            case DataGridViewContentAlignment.BottomLeft:
              pd.Left += (int)(Measures.CharWidthDots * 2 * _GetCellAttributesArgs.IndentLevel);
              break;
            case DataGridViewContentAlignment.TopRight:
            case DataGridViewContentAlignment.MiddleRight:
            case DataGridViewContentAlignment.BottomRight:
              pd.Right += (int)(Measures.CharWidthDots * 2 * _GetCellAttributesArgs.IndentLevel);
              break;
          }
          args.CellStyle.Padding = pd;
        }
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e, "Ошибка CellFormatting");
        EFPApp.ShowTempMessage("Ошибка CellFormatting. " + e.Message);
      }
    }

    #region Статические методы SetCellAttr

    /// <summary>
    /// Получить цвет фона и шрифта для заданных параметров ячейки.
    /// Цвета помещаются в объект CellStyle
    /// </summary>
    /// <param name="cellStyle">Заполняемый объект</param>
    /// <param name="colorType">Вариант цветного оформления ячейки</param>
    public static void SetCellAttr(DataGridViewCellStyle cellStyle, EFPDataGridViewColorType colorType)
    {
      SetCellAttr(cellStyle, colorType, false, true);
    }

    /// <summary>
    /// Получить цвет фона и шрифта для заданных параметров ячейки.
    /// Цвета помещаются в объект CellStyle
    /// </summary>
    /// <param name="cellStyle">Заполняемый объект</param>
    /// <param name="colorType">Вариант цветного оформления ячейки</param>
    /// <param name="grayed">true -Выделение серым цветом</param>
    public static void SetCellAttr(DataGridViewCellStyle cellStyle, EFPDataGridViewColorType colorType, bool grayed)
    {
      SetCellAttr(cellStyle, colorType, grayed, true);
    }

    /// <summary>
    /// Получить цвет фона и шрифта для заданных параметров ячейки.
    /// Цвета помещаются в объект CellStyle
    /// </summary>
    /// <param name="cellStyle">Заполняемый объект</param>
    /// <param name="colorType">Вариант цветного оформления ячейки</param>
    /// <param name="grayed">true -Выделение серым цветом</param>
    /// <param name="contentVisible">true - должно ли быть видно содержимое ячейки.
    /// Если false, то в <paramref name="cellStyle"/>.ForeColor помещается Color.Transparent</param>
    public static void SetCellAttr(DataGridViewCellStyle cellStyle, EFPDataGridViewColorType colorType, bool grayed, bool contentVisible)
    {
      //if (ColorType == EFPDataGridViewColorType.Alter)
      //{
      //  // !!! Нужно сделать исходя из текущего цвета ColorWindow
      //  CellStyle.BackColor = ColorEx.FromArgb(244,244,244); 
      //}
      Color BackColor, ForeColor;
      SetCellAttr(colorType, grayed, contentVisible, out BackColor, out ForeColor);
      if (!BackColor.IsEmpty)
        cellStyle.BackColor = BackColor;
      if (!ForeColor.IsEmpty)
        cellStyle.ForeColor = ForeColor;
    }

    /// <summary>
    /// Получить цвет фона и шрифта для заданных параметров ячейки.
    /// Статический метод, не привязанный к табличному просмотру и объекту DataGridViewCellStyle.
    /// </summary>
    /// <param name="colorType">Вариант цветного оформления ячейки</param>
    /// <param name="grayed">true -Выделение серым цветом</param>
    /// <param name="contentVisible">true - должно ли быть видно содержимое ячейки.
    /// Если false, то в <paramref name="foreColor"/> помещается Color.Transparent</param>
    /// <param name="backColor">Сюда помещается цвет фона, зависящий от <paramref name="colorType"/>.
    /// Для ColorType.Normal записывается Color.Empty</param>
    /// <param name="foreColor">Сюда помещается цвет текста, зависящий от <paramref name="colorType"/>,
    /// <paramref name="grayed"/> и <paramref name="contentVisible"/>.</param>
    public static void SetCellAttr(EFPDataGridViewColorType colorType, bool grayed, bool contentVisible, out Color backColor, out Color foreColor)
    {
      backColor = Color.Empty;
      foreColor = Color.Empty;
      switch (colorType)
      {
        case EFPDataGridViewColorType.Header:
          backColor = EFPApp.Colors.GridHeaderBackColor;
          foreColor = EFPApp.Colors.GridHeaderForeColor;
          //CellStyle.Font.Style =FontStyle.Bold | FontStyle.Underline;
          break;
        case EFPDataGridViewColorType.Alter:
          backColor = EFPApp.Colors.GridAlterBackColor;
          break;
        case EFPDataGridViewColorType.Special:
          backColor = EFPApp.Colors.GridSpecialBackColor;
          break;
        case EFPDataGridViewColorType.Total1:
          backColor = EFPApp.Colors.GridTotal1BackColor;
          break;
        case EFPDataGridViewColorType.Total2:
          backColor = EFPApp.Colors.GridTotal2BackColor;
          break;
        case EFPDataGridViewColorType.TotalRow:
          backColor = EFPApp.Colors.GridTotalRowBackColor;
          foreColor = EFPApp.Colors.GridTotalRowForeColor;
          break;
        case EFPDataGridViewColorType.Error:
          backColor = EFPApp.Colors.GridErrorBackColor;
          foreColor = EFPApp.Colors.GridErrorForeColor;
          break;
        case EFPDataGridViewColorType.Warning:
          backColor = EFPApp.Colors.GridWarningBackColor;
          foreColor = EFPApp.Colors.GridWarningForeColor;
          break;
      }
      if (grayed)
        foreColor = Color.Gray;
      if (!contentVisible)
        foreColor = Color.Transparent;
    }

    #endregion

    #region Атрибуты для Excel

    /// <summary>
    /// Получить атрибуты ячейки для передачи в Excel, соответствующие атрибутам ячейки табличного просмотра.
    /// </summary>
    /// <param name="cellAttr">Атрибуты ячейки ддя EFPDataGridView</param>
    /// <returns>Атрибуnы ячейки для передачи в Excel</returns>
    public static EFPDataGridViewExcelCellAttributes GetExcelCellAttr(EFPDataGridViewCellAttributesEventArgs cellAttr)
    {
      return GetExcelCellAttr(cellAttr.ColorType, cellAttr.Grayed);
    }

    /// <summary>
    /// Получить атрибуты ячейки для передачи в Excel, соответствующие атрибутам ячейки табличного просмотра.
    /// </summary>
    /// <param name="colorType">Цвет ячейки</param>
    /// <param name="grayed">true, если текст выделяется серым цветом</param>
    /// <returns>Атрибуnы ячейки для передачи в Excel</returns>
    public static EFPDataGridViewExcelCellAttributes GetExcelCellAttr(EFPDataGridViewColorType colorType, bool grayed)
    {
      EFPDataGridViewExcelCellAttributes Attr = new EFPDataGridViewExcelCellAttributes();
      Attr.BackColor = Color.Empty;
      Attr.ForeColor = Color.Empty;
      switch (colorType)
      {
        case EFPDataGridViewColorType.Header:
          Attr.ForeColor = Color.Navy;
          Attr.BackColor = Color.White;
          Attr.Bold = true;
          //Attr.Underline = true;
          Attr.Italic = true;
          break;
        case EFPDataGridViewColorType.Special:
          Attr.BackColor = Color.FromArgb(255, 255, 204);
          break;
        case EFPDataGridViewColorType.Total1:
          Attr.BackColor = Color.FromArgb(204, 255, 204);
          Attr.Bold = true;
          break;
        case EFPDataGridViewColorType.Total2:
          //Attr.BackColor = Color.FromArgb(255, 0, 255); // менее яркого нет
          // 22.06.2016
          Attr.BackColor = Color.FromArgb(0xFF, 0x99, 0xCC); // ColorIndex=38
          Attr.Bold = true;
          break;
        case EFPDataGridViewColorType.TotalRow:
          Attr.BackColor = Color.FromArgb(204, 204, 204);
          Attr.Bold = true;
          break;
        case EFPDataGridViewColorType.Error:
          Attr.BackColor = Color.Red;
          Attr.Bold = true;
          break;
        case EFPDataGridViewColorType.Warning:
          Attr.BackColor = Color.Yellow;
          Attr.ForeColor = Color.Red;
          Attr.Bold = true;
          break;
        case EFPDataGridViewColorType.Alter:
          Attr.BackColor = Color.FromArgb(0xCC, 0xFF, 0xFF); // ColorIndex=34
          break;
      }
      if (grayed)
        Attr.ForeColor = Color.Gray;
      return Attr;
    }

    #endregion

    // Когда табличный просмотр не имеет фокуса ввода, выбранные строки подсвечиваются
    // серым цветом, а не обычным цветом выделения

    void Control_Enter(object sender, EventArgs args)
    {
      // if (!HideSelection)
      Control.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
    }

    void Control_Leave(object sender, EventArgs args)
    {
      // if (!HideSelection)
      Control.DefaultCellStyle.SelectionBackColor = Color.DarkGray;
    }

    /// <summary>
    /// Если true, то при утере просмотром фокуса ввода, подсветка выделенных
    /// ячеек не выполняется (аналогично свойству у TextBox, TreeView и др.)
    /// В отличие от этих управляющих элементов, значением по умолчанию
    /// является false, то есть подсветка сохраняется
    /// </summary>
    public bool HideSelection
    {
      get { return _HideSelection; }
      set
      {
        if (value == _HideSelection)
          return;
        _HideSelection = value;
        //if (value)
        //  Grid.DefaultCellStyle.SelectionBackColor = Grid.DefaultCellStyle.BackColor;
        //else
        //{
        //  if (Grid.Focused)
        //    Control_Enter(null, null);
        //  else
        //    Control_Leave(null, null);
        //}
        Control.Invalidate();
      }
    }
    private bool _HideSelection;

    /// <summary>
    /// Если установить в true, то заблокированный табличный просмотр (при Enabled=false) будет от ображаться серым цветом, как другие управляющие элементы.
    /// По умолчанию - false, заблокированный табличный просмотр раскрашивается обычным образом
    /// </summary>
    public bool GrayWhenDisabled
    {
      get { return _GrayWhenDisabled; }
      set
      {
        if (value == _GrayWhenDisabled)
          return;
        _GrayWhenDisabled = value;
        if (!Enabled)
          Control.Invalidate();
      }
    }
    private bool _GrayWhenDisabled;

    #endregion

    #region Картинки в заголовках строк и всплывающие подсказки

    /// <summary>
    /// Возвращает имя изображения в списке EFPApp.MainImages, соответствующее EFPDataGridViewImageKind.
    /// Для <paramref name="imageKind"/>=None возвращает "EmptyImage".
    /// </summary>
    /// <param name="imageKind">Вариант изображения</param>
    /// <returns>Имя изображения</returns>
    public static string GetImageKey(EFPDataGridViewImageKind imageKind)
    {
      return GetImageKey(imageKind, "EmptyImage");
    }

    /// <summary>
    /// Возвращает имя изображения в списке EFPApp.MainImages, соответствующее EFPDataGridViewImageKind.
    /// </summary>
    /// <param name="imageKind">Вариант изображения</param>
    /// <param name="noneImageKey">Имя изображения для <paramref name="imageKind"/>=None</param>
    /// <returns>Имя изображения</returns>
    public static string GetImageKey(EFPDataGridViewImageKind imageKind, string noneImageKey)
    {
      switch (imageKind)
      {
        case EFPDataGridViewImageKind.Error: return "Error";
        case EFPDataGridViewImageKind.Warning: return "Warning";
        case EFPDataGridViewImageKind.Information: return "Information";
        case EFPDataGridViewImageKind.None: return noneImageKey;
        default: return "UnknownState";
      }
    }

    /// <summary>
    /// Разрешить рисование изображений в заголовках строк и вывод всплывающих
    /// подсказок. При установленном свойстве не будут правильно отображаться 
    /// значки и подсказки об ошибках в строках с помощью DataRow.RowError.
    /// По умолчанию - false (можно использовать стандартный механизм индикации в
    /// DataGridView).
    /// </summary>
    [DefaultValue(false)]
    public bool UseRowImages
    {
      get { return _UseRowImages; }
      set
      {
        if (value == _UseRowImages)
          return;
        _UseRowImages = value;

        if (value)
          Control.ShowRowErrors = false;

        if (Control.Visible && ProviderState == EFPControlProviderState.Attached)
          Control.InvalidateColumn(-1);
      }
    }
    private bool _UseRowImages;

    /// <summary>
    /// Использовать свойство DataRow.RowError для отображения признака ошибки в 
    /// заголовке строки и подсказки, как это по умолчанию делеает DataGridView.
    /// Свойство имеет значение при установленном UseRowImages.
    /// По умолчанию - false (предполагается, что признак ошибки строки 
    /// устанавливается обработчиком GetRowAttributes)
    /// Установка свойства не препятствует реализации GetRowAttributes
    /// </summary>
    [DefaultValue(false)]
    public bool UseRowImagesDataError
    {
      get { return _UseRowImagesDataError; }
      set
      {
        if (value == _UseRowImagesDataError)
          return;
        _UseRowImagesDataError = value;
        if (Control.Visible && ProviderState == EFPControlProviderState.Attached)
          Control.InvalidateColumn(-1);
      }
    }
    private bool _UseRowImagesDataError;

    /// <summary>
    /// Изображение, которое следует вывести в верхней левой ячейке просмотра
    /// Произвольное изображение может быть задано с помощью свойства TopLeftCellUserImage
    /// Свойство UseRowImages должно быть установлено
    /// </summary>
    [DefaultValue(EFPDataGridViewImageKind.None)]
    public EFPDataGridViewImageKind TopLeftCellImageKind
    {
      get { return _TopLeftCellImageKind; }
      set
      {
        if (value == _TopLeftCellImageKind)
          return;
        _TopLeftCellImageKind = value;

        if (Control.Visible && ProviderState == EFPControlProviderState.Attached)
          Control.InvalidateCell(-1, -1);
      }
    }
    private EFPDataGridViewImageKind _TopLeftCellImageKind;

    /// <summary>
    /// Произвольное изображение, которое следует вывести в верхней левой ячейке 
    /// просмотра
    /// Свойство UseRowImages должно быть установлено
    /// </summary>
    [DefaultValue(null)]
    public Image TopLeftCellUserImage
    {
      get { return _TopLeftCellUserImage; }
      set
      {
        if (value == _TopLeftCellUserImage)
          return;
        _TopLeftCellUserImage = value;

        if (Control.Visible && ProviderState == EFPControlProviderState.Attached)
          Control.InvalidateCell(-1, -1);
      }
    }
    private Image _TopLeftCellUserImage;

    /// <summary>
    /// Текст всплывающей подсказки, выводимой при наведении курсора мыши на
    /// верхнюю левую ячейку просмотра.
    /// Свойство инициализируется автоматически, если UseRowImages установлено в true.
    /// Свойство не включает в себя текст, генерируемый ShowRowCountInTopLeftCellToolTipText
    /// </summary>
    public string TopLeftCellToolTipText
    {
      get { return Control.TopLeftHeaderCell.ToolTipText; }
      set
      {
        _TopLeftCellToolTipText = value;

        // Обновляем, даже если подсказка не изменилась
        string s = String.Empty;
        if (value != null)
          s = value;
        if (ShowRowCountInTopLeftCellToolTipText && Control.DataSource != null)
        {
          if (s.Length > 0)
            s += Environment.NewLine;
          s += "Строк в просмотре: " + Control.RowCount.ToString();
        }
        Control.TopLeftHeaderCell.ToolTipText = s;
      }
    }
    private string _TopLeftCellToolTipText;

    /// <summary>
    /// Необходимость обновить подсказку по таймеру
    /// </summary>
    private bool _TopLeftCellToolTipTextUpdateFlag = false;

    /// <summary>
    /// Если свойство установить в true, то в верхней левой ячейке будет отображаться всплывающая
    /// подсказка "Записей в просмотре: XXX". 
    /// Свойство подключает обработку события DataGridView.DataBindingComplete. Не работает, если табличный
    /// просмотр не присоединен к источнику данных.
    /// По умолчанию - false - свойство отключено.
    /// </summary>
    public bool ShowRowCountInTopLeftCellToolTipText
    {
      get { return _ShowRowCountInTopLeftCellToolTipText; }
      set
      {
        if (value == _ShowRowCountInTopLeftCellToolTipText)
          return;
        _ShowRowCountInTopLeftCellToolTipText = value;

        TopLeftCellToolTipText = _TopLeftCellToolTipText; // обновляем подсказку
      }
    }
    private bool _ShowRowCountInTopLeftCellToolTipText;

    private void DoControl_RowHeaderCellPainting(DataGridViewCellPaintingEventArgs args)
    {
      if (!_UseRowImages)
        return;
      try
      {
        if (args.RowIndex < 0)
          DoControl_RowHeaderCellPainting(args, _TopLeftCellImageKind, _TopLeftCellUserImage);
        else
          DoControl_RowHeaderCellPainting(args, _GetRowAttributesArgs.ImageKind, _GetRowAttributesArgs.UserImage);
      }
      catch
      {
        DoControl_RowHeaderCellPainting(args, EFPDataGridViewImageKind.Error, null);
      }
    }

    private void DoControl_RowHeaderCellPainting(DataGridViewCellPaintingEventArgs args, EFPDataGridViewImageKind imageKind, Image userImage)
    {
      args.PaintBackground(args.ClipBounds, false);
      args.PaintContent(args.ClipBounds);

      if (userImage == null && imageKind != EFPDataGridViewImageKind.None)
        userImage = EFPApp.MainImages.Images[GetImageKey(imageKind, String.Empty)];
      if (userImage != null)
      {
        Rectangle r = args.CellBounds;
        Point pt = r.Location;
        pt.X += r.Width / 2 + 3;
        pt.Y += (r.Height - userImage.Height) / 2;
        args.Graphics.DrawImage(userImage, pt);
      }
      args.Handled = true;
    }

    /// <summary>
    /// Получить текст подсказки для заголовка строки
    /// </summary>
    /// <param name="rowIndex">Индекс строки или (-1) для верхней левой ячейки</param>
    /// <returns>Текст всплывающей подсказки, получаемый с помощью события 
    /// GetRowAttributes или значениек свойства TopLeftCellToolTpText</returns>
    public string GetRowToolTipText(int rowIndex)
    {
      if (rowIndex < 0)
        return TopLeftCellToolTipText;
      else
        return DoGetRowAttributes(rowIndex, EFPDataGridViewAttributesReason.ToolTip).ToolTipText;
    }

    /// <summary>
    /// Получить текст подсказки для ячейки
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <param name="columnIndex"></param>
    /// <returns></returns>
    public string GetCellToolTipText(int rowIndex, int columnIndex)
    {
      if (columnIndex < 0)
        return GetRowToolTipText(rowIndex); // для заголовка столбка

      if (rowIndex < 0)
        return Control.Columns[columnIndex].ToolTipText; // для заголовка столбца

      EFPDataGridViewColumn col = Columns[columnIndex];
      string sText = String.Empty;

      // Нужна ли подсказка по содержимому ячейки?
      bool CurrentCellToolTip = true;
      if (CurrentConfig != null)
        CurrentCellToolTip = CurrentConfig.CurrentCellToolTip;

      if (CurrentCellToolTip)
      {
        // 1. Подсказка по текущему значению ячейки
        if (col.GridColumn is DataGridViewTextBoxColumn)
          sText = GetCellTextValue(rowIndex, columnIndex);
        else if (col.GridColumn is DataGridViewCheckBoxColumn)
        {
          object v = Control.Rows[rowIndex].Cells[columnIndex].Value;
          if (DataTools.GetBool(v))
            sText = "Да";
          else
            sText = "Нет";
        }
        else
          sText = String.Empty;

        // 2. Подсказка, добавляемая обработчиком столбца
        col.CallCellToolTextNeeded(rowIndex, ref sText);
        if (!String.IsNullOrEmpty(sText))
        {
          string sHead = col.GridColumn.ToolTipText;
          if (String.IsNullOrEmpty(sHead))
            sHead = col.GridColumn.HeaderText;
          sText = sHead + ": " + sText;
        }
      }

      // 3. Пользовательский обработчик
      DoGetRowAttributes(rowIndex, EFPDataGridViewAttributesReason.ToolTip);
      _GetCellAttributesArgs.ToolTipText = sText;
      return DoGetCellAttributes(columnIndex).ToolTipText;
    }



    /// <summary>
    /// Нужно ли для данной строки получать подсказку с помощью EFPGridProducer?
    /// Непереопределенный метод возвращает true.
    /// </summary>
    /// <param name="rowIndex">Индекс строки табличного просмотра</param>
    /// <returns>Признак использования</returns>
    protected virtual bool GetGridProducerToolTipRequired(int rowIndex)
    {
      return true;
    }

    private void Control_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs args)
    {
      if (args.RowIndex < 0) // Подсказка для верхней левой ячейки задана свойством TopLeftCellToolTipText
        return;              // Если еще раз к нему обратиться, то будет Stack overflow

      try
      {
        if (args.ColumnIndex < 0)
          args.ToolTipText = GetRowToolTipText(args.RowIndex);
        else
          args.ToolTipText = GetCellToolTipText(args.RowIndex, args.ColumnIndex);
      }
      catch (Exception e)
      {
        args.ToolTipText = "!!! Ошибка при получении подсказки !!!" + Environment.NewLine + e.Message;
      }
    }


    /// <summary>
    /// Перейти к следующей иди предыдущей строке с ошибкой
    /// </summary>
    /// <param name="fromTableBegin">true - перейти к первой или последней строке в таблице с подходящим условием,
    /// false - перейти к следующей или предыдущей относительно текущей строки</param>
    /// <param name="forward">true - перейти вперед (в сторону конца таблицы), false - назад (к началу)</param>
    /// <param name="imageKind">Какую строку найти (условие поиска)</param>
    public void GotoNextErrorRow(bool fromTableBegin, bool forward, EFPDataGridViewImageKind imageKind)
    {
      if (!UseRowImages)
      {
        EFPApp.ShowTempMessage("Просмотр не поддерживает навигацию по строкам с ошибками");
        return;
      }

      if (Control.RowCount == 0)
      {
        EFPApp.ShowTempMessage("Просмотр не содержит ни одной строки");
      }
      int StartRowIndex;
      if (fromTableBegin)
        StartRowIndex = -1;
      else
        StartRowIndex = CurrentRowIndex;
      int NewRowIndex = FindErrorRow(StartRowIndex, forward, imageKind);
      if (NewRowIndex < 0)
      {
        string msg;
        switch (imageKind)
        {
          case EFPDataGridViewImageKind.Error:
            msg = " с ошибкой";
            break;
          case EFPDataGridViewImageKind.Warning:
            msg = " с ошибкой или предупреждением";
            break;
          case EFPDataGridViewImageKind.Information:
            msg = " с сообщением";
            break;
          case EFPDataGridViewImageKind.None:
            msg = " без ошибок";
            break;
          default:
            msg = ", удовлетворяющая условию,";
            break;
        }
        EFPApp.ShowTempMessage((forward ? "Достигнут конец таблицы" : "Достигнуто начало таблицы") +
          ", но строка" + msg + " не найдена");
        return;
      }
      CurrentRowIndex = NewRowIndex;
    }

    /// <summary>
    /// Найти строку, содержащую признак ошибки
    /// </summary>
    /// <param name="startRowIndex">С какой строки начать поиск. Значение (-1) указывает на поиск с начала (с конца) таблицы</param>
    /// <param name="forward">true-искать вперед к концу таблицы, false - искать назад к началу таблицы</param>
    /// <param name="imageKind">Какую строку найти (условие поиска)</param>
    /// <returns>Индекс найденной строки или (-1), если строка не найдена</returns>
    public int FindErrorRow(int startRowIndex, bool forward, EFPDataGridViewImageKind imageKind)
    {
      if (forward)
      {
        if (startRowIndex < 0)
        {
          for (int i = 0; i < Control.RowCount; i++)
          {
            if (TestRowImageKind(i, imageKind))
              return i;
          }
        }
        else
        {
          for (int i = startRowIndex + 1; i < Control.RowCount; i++)
          {
            if (TestRowImageKind(i, imageKind))
              return i;
          }
        }
      }
      else
      {
        if (startRowIndex < 0)
        {
          for (int i = Control.RowCount - 1; i >= 0; i--)
          {
            if (TestRowImageKind(i, imageKind))
              return i;
          }
        }
        else
        {
          for (int i = startRowIndex - 1; i >= 0; i--)
          {
            if (TestRowImageKind(i, imageKind))
              return i;
          }
        }
      }
      // Строка не найдена
      return -1;
    }

    private bool TestRowImageKind(int rowIndex, EFPDataGridViewImageKind imageKind)
    {
      EFPDataGridViewImageKind ThisKind = GetRowImageKind(rowIndex);
      switch (imageKind)
      {
        case EFPDataGridViewImageKind.None:
          return ThisKind == EFPDataGridViewImageKind.None;
        case EFPDataGridViewImageKind.Information:
          return ThisKind != EFPDataGridViewImageKind.None;
        case EFPDataGridViewImageKind.Warning:
          return ThisKind == EFPDataGridViewImageKind.Warning || ThisKind == EFPDataGridViewImageKind.Error;
        case EFPDataGridViewImageKind.Error:
          return ThisKind == EFPDataGridViewImageKind.Error;
        default:
          throw new ArgumentException("Недопустимый тип изображения строки для поиска:" + imageKind.ToString(), "imageKind");
      }
    }

    /// <summary>
    /// Получить тип изображения для строки
    /// </summary>
    /// <param name="rowIndex">Индекс строки в просмотре. 
    /// Если равно (-1), возвращается значение свойства TopLeftCellImageKind.
    /// Иначе генерируется событие GetRowAttributes для получения значка строки</param>
    /// <returns></returns>
    public EFPDataGridViewImageKind GetRowImageKind(int rowIndex)
    {
      if (rowIndex < 0)
        return TopLeftCellImageKind;
      else
        return DoGetRowAttributes(rowIndex, EFPDataGridViewAttributesReason.View).ImageKind;
    }


    /// <summary>
    /// Имя текстового столбца в связанном наборе данных, используемое для идентификации строки при выводе сообщений об ошибках для группы строк
    /// </summary>
    public string RowIdTextDataColumnName { get { return _RowIdTextDataColumnName; } set { _RowIdTextDataColumnName = value; } }
    private string _RowIdTextDataColumnName;

    /// <summary>
    /// Добавить в список ErrorMessages сообщения, относящиеся к строке с заданным
    /// индексом.
    /// </summary>
    /// <param name="errorMessages">Список для добавления сообщений</param>
    /// <param name="rowIndex">Индекс строки просмотра</param>
    public void GetRowErrorMessages(ErrorMessageList errorMessages, int rowIndex)
    {
      GetRowErrorMessages(errorMessages, rowIndex, false);
    }

    /// <summary>
    /// Добавить в список ErrorMessages сообщения, относящиеся к строке с заданным
    /// индексом.
    /// </summary>
    /// <param name="errorMessages">Список для добавления сообщений</param>
    /// <param name="rowIndex">Индекс строки просмотра</param>
    /// <param name="useRowIdText">Если true, то перед сообщениями будет добавлен текст с идентификатором строки</param>
    public void GetRowErrorMessages(ErrorMessageList errorMessages, int rowIndex, bool useRowIdText)
    {
#if DEBUG
      if (errorMessages == null)
        throw new ArgumentNullException("errorMessages");
#endif
      _GetRowAttributesArgs.RowErrorMessages = errorMessages;
      try
      {
        DoGetRowErrorMessages(rowIndex, useRowIdText);
      }
      finally
      {
        _GetRowAttributesArgs.RowErrorMessages = null;
      }
    }

    private void DoGetRowErrorMessages(int rowIndex, bool useRowIdText)
    {
      int cnt = _GetRowAttributesArgs.RowErrorMessages.Count;
      DoGetRowAttributes(rowIndex, EFPDataGridViewAttributesReason.ToolTip);
      if (_GetRowAttributesArgs.ImageKind != EFPDataGridViewImageKind.None &&
        cnt == _GetRowAttributesArgs.RowErrorMessages.Count)
      {
        // Признак ошибки установлен, но сообщение не добавлено
        string Msg;
        if (String.IsNullOrEmpty(_GetRowAttributesArgs.ToolTipText))
          Msg = "Сообщение об ошибке отсутствует";
        else
          Msg = _GetRowAttributesArgs.ToolTipText.Replace(Environment.NewLine, " ");

        ErrorMessageKind Kind2;
        switch (_GetRowAttributesArgs.ImageKind)
        {
          case EFPDataGridViewImageKind.Error: Kind2 = ErrorMessageKind.Error; break;
          case EFPDataGridViewImageKind.Warning: Kind2 = ErrorMessageKind.Warning; break;
          default: Kind2 = ErrorMessageKind.Info; break;
        }
        _GetRowAttributesArgs.RowErrorMessages.Add(new ErrorMessageItem(Kind2, Msg, null, rowIndex));
      }

      if (useRowIdText && (_GetRowAttributesArgs.RowErrorMessages.Count > cnt))
        _GetRowAttributesArgs.RowErrorMessages.SetPrefix(_GetRowAttributesArgs.RowIdText + " - ", cnt);
    }

    /// <summary>
    /// Получить список сообщений для выбранных строк в просмотре
    /// </summary>
    /// <param name="errorMessages">Список, куда будут добавлены сообщения</param>
    /// <param name="useRowIdText">true - добавить перед сообщениями номера строк</param>
    public void GetSelectedRowsErrorMessages(ErrorMessageList errorMessages, bool useRowIdText)
    {
      if (errorMessages == null)
        throw new ArgumentNullException("errorMessages");

      int[] RowIdxs = SelectedRowIndices;
      _GetRowAttributesArgs.RowErrorMessages = errorMessages;
      try
      {
        for (int i = 0; i < RowIdxs.Length; i++)
          DoGetRowErrorMessages(RowIdxs[i], useRowIdText);
      }
      finally
      {
        _GetRowAttributesArgs.RowErrorMessages = null;
      }
    }

    /// <summary>
    /// Получить список сообщений для выбранных строк в просмотре
    /// Текст, идентифицирующий строку, задается, если число выбранных строк больше 1
    /// </summary>
    /// <param name="errorMessages">Список, куда будут добавлены сообщения</param>
    public void GetSelectedRowsErrorMessages(ErrorMessageList errorMessages)
    {
      if (errorMessages == null)
        throw new ArgumentNullException("errorMessages");

      int[] RowIdxs = SelectedRowIndices;
      _GetRowAttributesArgs.RowErrorMessages = errorMessages;
      try
      {
        for (int i = 0; i < RowIdxs.Length; i++)
          DoGetRowErrorMessages(RowIdxs[i], RowIdxs.Length > 1);
      }
      finally
      {
        _GetRowAttributesArgs.RowErrorMessages = null;
      }
    }

    /// <summary>
    /// Получить список сообщений для всех строк в просмотре.
    /// Перед сообщениями добавляется текст, идентифицирующий строку
    /// </summary>
    /// <param name="errorMessages"></param>
    public void GetAllRowsErrorMessages(ErrorMessageList errorMessages)
    {
      if (errorMessages == null)
        throw new ArgumentNullException("errorMessages");

      _GetRowAttributesArgs.RowErrorMessages = errorMessages;
      try
      {
        for (int i = 0; i < Control.RowCount; i++)
          DoGetRowErrorMessages(i, true);
      }
      finally
      {
        _GetRowAttributesArgs.RowErrorMessages = null;
      }
    }

    /// <summary>
    /// Инициализировать значок и всплывающую подсказку для верхней левой ячейки 
    /// табличного просмотра, где будет выведено количество ошибок и предупреждений
    /// Устанавливает свойства TopLeftCellImageKind и TopLeftCellToolTipText
    /// При вызове метода перебираются все строки табличного просмотра. Для каждой
    /// строки вызывается метод GetRowImageKind(), который, в свою очередь, вызывает
    /// для строки событие GetRowAttributes.
    /// Строки с состоянием Information не учитываются и значок (i) в верхней левой
    /// ячейке не выводится.
    /// Метод ничего не делает при UseRowImages=false.
    /// </summary>
    public void InitTopLeftCellTotalInfo()
    {
      if (!UseRowImages)
        return;

      int ErrorCount = 0;
      int WarningCount = 0;
      int n = Control.RowCount;
      if (n == 0)
      {
        if (SourceAsDataView != null)
          n = SourceAsDataView.Count;
      }
      for (int i = 0; i < n; i++)
      {
        switch (GetRowImageKind(i))
        {
          case EFPDataGridViewImageKind.Error:
            ErrorCount++;
            break;
          case EFPDataGridViewImageKind.Warning:
            WarningCount++;
            break;
        }
      }

      if (ErrorCount == 0 && WarningCount == 0)
        TopLeftCellToolTipText = "Нет строк с ошибками или предупреждениями";
      else
      {
        StringBuilder sb = new StringBuilder();
        if (ErrorCount > 0)
        {
          sb.Append("Есть строки с ошибками (");
          sb.Append(ErrorCount);
          sb.Append(")");
          sb.Append(Environment.NewLine);
        }
        if (WarningCount > 0)
        {
          sb.Append("Есть строки с предупреждениями (");
          sb.Append(WarningCount);
          sb.Append(")");
          sb.Append(Environment.NewLine);
        }
        sb.Append("Используйте Ctrl+] и Ctrl+[ для перехода к этим строкам");
        TopLeftCellToolTipText = sb.ToString();
      }
      if (ErrorCount > 0)
        TopLeftCellImageKind = EFPDataGridViewImageKind.Error;
      else
      {
        if (WarningCount > 0)
          TopLeftCellImageKind = EFPDataGridViewImageKind.Warning;
        else
          TopLeftCellImageKind = EFPDataGridViewImageKind.None;
      }
    }

    /// <summary>
    /// Перебирает все строки просмотра и возвращает максимальное значение типа
    /// ошибки для строки
    /// </summary>
    /// <returns>Тип значка.</returns>
    public EFPDataGridViewImageKind GetAllRowsImageKind()
    {
      int n = Control.RowCount;
      if (n == 0)
      {
        if (SourceAsDataView != null)
          n = SourceAsDataView.Count;
      }

      EFPDataGridViewImageKind MaxLevel = EFPDataGridViewImageKind.None;

      for (int i = 0; i < n; i++)
      {
        EFPDataGridViewImageKind ThisLevel = GetRowImageKind(i);

        if (ThisLevel > MaxLevel)
        {
          MaxLevel = ThisLevel;
          if (MaxLevel == EFPDataGridViewImageKind.Error)
            break;
        }
      }
      return MaxLevel;
    }

    #endregion

    #region Быстрый поиск по первым буквам

    /// <summary>
    /// Возвращает true, если хотя бы один столбец поддерживает поиск по буквам
    /// </summary>
    public bool CanIncSearch
    {
      get
      {
        for (int i = 0; i < Columns.Count; i++)
        {
          if (Columns[i].CanIncSearch)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Текущий столбец, в котором осуществляется поиск по первым буквам.
    /// Установка свойства в значение, отличное от null начинает поиск. 
    /// Установка в null завершает поиск по буквам
    /// </summary>
    public EFPDataGridViewColumn CurrentIncSearchColumn
    {
      get
      {
        return _CurrentIncSearchColumn;
      }
      set
      {
        if (value == _CurrentIncSearchColumn)
          return;
        _CurrentIncSearchColumn = value;
        _CurrentIncSearchChars = String.Empty;
        // FCurrentIncSearchMask = null;

        // Некорректно работает при нажатии Backspace (норовит съесть лишнее)
        // Как разрешить ввод разделителей (SkipLiterals не работает?)

        //if (value!=null)
        //{
        //  if (value.Mask != null)
        //  {
        //    string EditMask = value.Mask.EditMask;
        //    if (!String.IsNullOrEmpty(EditMask))
        //    {
        //      FCurrentIncSearchMask = new MaskedTextProvider(EditMask);
        //      FCurrentIncSearchMask.SkipLiterals = true;
        //    }
        //  }
        //}
        IncSearchDataView = null; // сброс DataView
        if (value != null && TextSearchContext != null)
          TextSearchContext.ResetContinueEnabled();
        if (CommandItems != null)
          CommandItems.RefreshIncSearchItems();
      }
    }
    private EFPDataGridViewColumn _CurrentIncSearchColumn;

    //private void ResetCurrentIncSearchColumn(object sender, EventArgs args)
    //{
    //  CurrentIncSearchColumn = null;
    //}

    /// <summary>
    /// Текущие набранные символы для быстрого поиска
    /// </summary>
    public string CurrentIncSearchChars
    {
      get { return _CurrentIncSearchChars; }
      internal set { _CurrentIncSearchChars = value; }
    }
    private string _CurrentIncSearchChars;

    /// <summary>
    /// Провайдер маски для поиска по первым буквам
    /// </summary>
    //private MaskedTextProvider FCurrentIncSearchMask;

    private MaskedTextProvider GetIncSearchMaskProvider()
    {
      if (_CurrentIncSearchColumn == null)
        return null;
      if (_CurrentIncSearchColumn.MaskProvider == null)
        return null;
      string EditMask = _CurrentIncSearchColumn.MaskProvider.EditMask;
      if (String.IsNullOrEmpty(EditMask))
        return null;
      MaskedTextProvider Provider = new MaskedTextProvider(EditMask,
        // System.Globalization.CultureInfo.CurrentCulture,
        _CurrentIncSearchColumn.MaskProvider.Culture, // 04.06.2019
        false, (char)0, (char)0, false);
      Provider.SkipLiterals = true;
      Provider.ResetOnSpace = false;
      Provider.Set(_CurrentIncSearchChars);
      return Provider;
    }

    void Control_KeyPress(object sender, KeyPressEventArgs args)
    {
      try
      {
        DoControl_KeyPress(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработки нажатия клавиши KeyPress");
      }
    }
    void DoControl_KeyPress(KeyPressEventArgs args)
    {
      if (args.Handled)
        return;
      if (args.KeyChar < ' ')
        return;

      if (args.KeyChar == ' ' && System.Windows.Forms.Control.ModifierKeys != Keys.None)
        return; // 31.10.2018 Отсекаем Shift-Space, Ctrl-Space и прочее, так как эти клавиши обрабатываются не как символы для поиска 

      // Нажатие клавиши может начать поиск по первым буквам
      if (CurrentIncSearchColumn == null)
      {
        // Если поиск не начат, пытаемся его начать
        if (CurrentColumn == null)
          return;
        if (!CurrentColumn.CanIncSearch)
          return;
        CurrentIncSearchColumn = CurrentColumn;
      }

      args.Handled = true;

      EFPApp.ShowTempMessage(null);

      // Под Windows-98 неправильная кодировка русских букв - исправляем
      char KeyChar = WinFormsTools.CorrectCharWin98(args.KeyChar);

      MaskedTextProvider MaskProvider = GetIncSearchMaskProvider();
      string s;
      if (MaskProvider != null)
      {
        // для столбца задана маска
        if (!AddCharToIncSearchMask(MaskProvider, KeyChar, out s))
          return;
      }
      else
      {
        // Нет маски
        s = (CurrentIncSearchChars + KeyChar);
      }

      bool Res;
      EFPApp.BeginWait("Поиск");
      try
      {
        //Res = CurrentColumn.PerformIncSearch(s.ToUpper(), false);
        // 27.12.2020
        Res = CurrentIncSearchColumn.PerformIncSearch(s.ToUpper(), false);
      }
      finally
      {
        EFPApp.EndWait();
      }
      if (!Res)
      {
        EFPApp.ShowTempMessage("Символ \"" + KeyChar + "\" не может быть добавлен к строке быстрого поиска");
        return;
      }

      _CurrentIncSearchChars = s;

      if (CommandItems != null)
        CommandItems.RefreshIncSearchItems();
    }

    private bool AddCharToIncSearchMask(MaskedTextProvider maskProvider, char keyChar, out string s)
    {
      s = null;
      string s1 = new string(keyChar, 1);
      s1 = s1.ToUpper();
      keyChar = s1[0];
      int ResPos;
      MaskedTextResultHint ResHint;
      if (!maskProvider.Add(keyChar, out ResPos, out ResHint))
      {
        // Пытаемся подобрать символ в нижнем регистре
        s1 = s1.ToLower();
        char KeyChar2 = s1[0];
        if (!maskProvider.Add(KeyChar2, out ResPos, out ResHint))
        {
          // MaskedTextProvider почему-то не пропускает символы-разделители в 
          // качестве ввода, независимо от SkipLiterals
          // Поэтому, если следующим символом ожидается разделитель, то его
          // добавляем к строке поиска вручную
          if (ResHint != MaskedTextResultHint.UnavailableEditPosition)
          {
            if ((!maskProvider.IsEditPosition(maskProvider.LastAssignedPosition + 1)) && maskProvider[maskProvider.LastAssignedPosition + 1] == keyChar)
            {
              s = maskProvider.ToString(0, maskProvider.LastAssignedPosition + 1) + keyChar;
              return true;
            }
          }
          EFPApp.ShowTempMessage("Нельзя добавить символ \"" + keyChar + "\" для быстрого поиска. " + GetMaskedTextResultHintText(ResHint));
          return false;
        }
      }
      if (ResHint == MaskedTextResultHint.CharacterEscaped)
      {
        EFPApp.ShowTempMessage("Нельзя добавить символ \"" + keyChar + "\" для быстрого поиска. " + GetMaskedTextResultHintText(ResHint));
        return false;
      }
      s = maskProvider.ToString(0, maskProvider.LastAssignedPosition + 1);
      return true;
    }

    private static string GetMaskedTextResultHintText(MaskedTextResultHint resHint)
    {
      switch (resHint)
      {
        case MaskedTextResultHint.DigitExpected:
          return "Ожидалась цифра";
        case MaskedTextResultHint.AlphanumericCharacterExpected:
          return "Ожидалась цифра или буква";
        case MaskedTextResultHint.LetterExpected:
          return "Ожидалась буква";
        case MaskedTextResultHint.UnavailableEditPosition:
          return "Все символы уже введены";
        case MaskedTextResultHint.CharacterEscaped:
          return "Символ пропускается";
        default:
          return resHint.ToString();
      }
    }

    void DoControl_KeyDown2(KeyEventArgs args)
    {
      if (args.Handled)
        return; // уже обработано
      if (Control.IsCurrentCellInEditMode)
        return; // в режиме редактирования ячейки не обрабатывается

      if (CurrentIncSearchColumn != null)
      {
        if (args.KeyCode == Keys.Back)
        {
          args.Handled = true;
          EFPApp.ShowTempMessage(null);
          if (_CurrentIncSearchChars.Length < 1)
          {
            EFPApp.ShowTempMessage("Строка быстрого поиска не содержит введенных символов");
            return;
          }

          MaskedTextProvider MaskProvider = GetIncSearchMaskProvider();
          string s;
          if (MaskProvider != null)
          {
            if (_CurrentIncSearchChars.Length == MaskProvider.LastAssignedPosition + 1)
            {
              MaskProvider.Remove();
              s = MaskProvider.ToString(0, MaskProvider.LastAssignedPosition + 1);
            }
            else
              // Последний символ в строке поиска - разделитель, введенный вручную
              s = _CurrentIncSearchChars.Substring(0, _CurrentIncSearchChars.Length - 1);
          }
          else
            s = _CurrentIncSearchChars.Substring(0, _CurrentIncSearchChars.Length - 1);

          _CurrentIncSearchChars = s;
          //CurrentColumn.PerformIncSearch(s.ToUpper(), false);
          // 27.12.2020
          CurrentIncSearchColumn.PerformIncSearch(s.ToUpper(), false);

          if (CommandItems != null)
            CommandItems.RefreshIncSearchItems();
        }
      }

#if XXXXX
      // Обработка сочетаний для буфера обмена
      if (Args.Modifiers == Keys.Control)
      {
        switch (Args.KeyCode)
        {
          case Keys.X:
            if ((CommandItems.ciCut.Usage & EFPCommandItemUsage.ShortCut) == EFPCommandItemUsage.ShortCut)
              CommandItems.ciCut.PerformClick();
            Args.Handled = true;
            break;
          case Keys.C:
          case Keys.Insert:
            if ((CommandItems.ciCopy.Usage & EFPCommandItemUsage.ShortCut) == EFPCommandItemUsage.ShortCut)
              CommandItems.ciCopy.PerformClick();
            Args.Handled = true;
            break;
          case Keys.V:
            if (CommandItems.PasteHandler.Count > 0)
              CommandItems.PasteHandler.PerformPaste(EFPPasteReason.Paste);
            Args.Handled = true;
            break;
        }
      }
      if (Args.Modifiers == Keys.Shift)
      {
        switch (Args.KeyCode)
        {
          case Keys.Delete:
            if ((CommandItems.ciCut.Usage & EFPCommandItemUsage.ShortCut) == EFPCommandItemUsage.ShortCut)
              CommandItems.ciCut.PerformClick();
            Args.Handled = true;
            break;
          case Keys.Insert:
            if (CommandItems.PasteHandler.Count > 0)
              CommandItems.PasteHandler.PerformPaste(EFPPasteReason.Paste);
            Args.Handled = true;
            break;
        }
      }
#endif
    }

    /// <summary>
    /// Вспомогательный объект для выполнения поиска
    /// </summary>
    internal DataView IncSearchDataView;

    /// <summary>
    /// Признак того, что IncSearchDataView ссылается на созданную вручную таблицу,
    /// а не на исходные данные просмотра. Свойство устанавливается в момент 
    /// инициализации IncSearchDataView
    /// </summary>
    internal bool IncSearchDataViewIsManual;

    /// <summary>
    /// Получить индекс первого столбца с установленным CanIncSearch или (-1), если
    /// таких столбцов нет.
    /// Может применяться при инициализации просмотра для определения начального 
    /// активного столбца
    /// </summary>
    public int FirstIncSearchColumnIndex
    {
      get
      {
        for (int i = 0; i < Columns.Count; i++)
        {
          if (Columns[i].CanIncSearch)
            return i;
        }
        return -1;
      }
    }

    #endregion

    #region Свойство CurrentGridConfig

    /// <summary>
    /// Выбранная настройка табличного просмотра.
    /// Если свойство GridProducer не установлено, должен быть задан обработчик CurrentGridConfigChanged,
    /// который выполнит реальную настройку просмотра
    /// </summary>
    public EFPDataGridViewConfig CurrentConfig
    {
      get { return _CurrentConfig; }
      set
      {
        if (value == _CurrentConfig)
          return; // предотвращение рекурсивной установки

        if (value != null)
          value.SetReadOnly();
        _CurrentConfig = value;

        if (!InsideSetCurrentConfig)
        {
          InsideSetCurrentConfig = true;
          try
          {
            CancelEventArgs Args = new CancelEventArgs();
            Args.Cancel = false;
            OnCurrentConfigChanged(Args);
            if ((!Args.Cancel) && (GridProducer != null))
            {
              //Control.Columns.Clear(); // 21.01.2012
              this.Columns.Clear(); // 10.01.2016
              //int nCols1 = Control.Columns.Count;
              GridProducer.InitGridView(this, CurrentConfigHasBeenSet);
              //int nCols2 = Control.Columns.Count;
              PerformGridProducerPostInit();
            }
          }
          finally
          {
            InsideSetCurrentConfig = false;
          }
        }
        _CurrentConfigHasBeenSet = true;
      }
    }
    private EFPDataGridViewConfig _CurrentConfig;

    /// <summary>
    /// Флажок нахождения в пределах сеттера свойства CurrentConfig.
    /// </summary>
    protected bool InsideSetCurrentConfig = false;

    /// <summary>
    /// Признак того, что свойство CurrentConfig уже устанавливалось
    /// </summary>
    protected bool CurrentConfigHasBeenSet { get { return _CurrentConfigHasBeenSet; } }
    private bool _CurrentConfigHasBeenSet = false;

    /// <summary>
    /// Вызывается при изменении свойства CurrentConfig.
    /// Если аргумент Cancel обработчиком установлен в true, то предполагается,
    /// что инициализация просмотра выполнена в обработчике. В противном случае
    /// (по умолчанию Cancel=false или при отстутствии обработчика) будет вызван
    /// метод GridProducer.InitGrid()
    /// </summary>
    public event CancelEventHandler CurrentConfigChanged;

    /// <summary>
    /// Вызывает событие CurrentConfigChanged.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnCurrentConfigChanged(CancelEventArgs args)
    {
      if (CurrentConfigChanged != null)
        CurrentConfigChanged(this, args);
    }

    /// <summary>
    /// Это событие вызывается после того, как конфигурация табличного просмотра инициализирована
    /// с помощью GridProducer.InitGrid()
    /// </summary>
    public event EventHandler GridProducerPostInit;

    /// <summary>
    /// Вызывает событие GridProducerPostInit
    /// </summary>
    public void PerformGridProducerPostInit()
    {
      // 12.10.2016
      // Проверка недействительна
      // В DBxDocGridView свойство GridProducer не устанавливается
      //#if DEBUG
      //      if (GridProducer==null)
      //        throw new InvalidOperationException("Этот метод не может быть вызван, если свойство GridProducer не установлено");
      //#endif
      OnGridProducerPostInit();
    }

    /// <summary>
    /// Вызывает событие GridProducerPostInit
    /// </summary>
    protected virtual void OnGridProducerPostInit()
    {
      if (GridProducerPostInit != null)
        GridProducerPostInit(this, EventArgs.Empty);
    }

    #endregion

    #region Установка отметок строк в столбце CheckBox

    /// <summary>
    /// Столбец CheckBox, с помощью которого устанавливаются отметки для строк
    /// Если свойство установлено, то в локальном меню просмотра появляется
    /// подменю "Установка отметок для строк"
    /// Свойство может быть установлено при добавлении столбца с помощью аргумента
    /// MarkRows в методе EFPDataGridViewColumns.AddBool()
    /// Свойство следует устанавливать после добавления всех столбцов в просмотр,
    /// но перед вызовом SetCommandItems(). Также допускается повторная установка свойства в процессе работы, если столбцы создаются заново
    /// При установке свойства, если DataGridView.ReadOnly=true, устанавливается свойство
    /// DataGridViewColumn.ReadOnly для всех столбцов, кроме заданного, а DataGridView.ReadOnly устанавливается в false.
    /// Также устанавливается в true свойство DataGridView.MultiSelect=true.
    /// </summary>
    public DataGridViewCheckBoxColumn MarkRowsGridColumn
    {
      get { return _MarkRowsGridColumn; }
      set
      {
        if (value == _MarkRowsGridColumn)
          return;
#if DEBUG
        // 13.05.2016
        // Разрешена повторная установка свойства
        //if (FMarkRowsGridColumn != null)
        //  throw new InvalidOperationException("Повторная установка свойства MarkRowsColumn не допускается");
        //if (value == null)
        //  throw new ArgumentNullException("value");
        if (value != null)
        {
          if (value.DataGridView != Control)
            throw new ArgumentException("Столбец не принадлежит табличному просмотру", "value");
        }
#endif
        _MarkRowsGridColumn = value;
        if (_MarkRowsGridColumn != null)
        {
          if (!HasBeenCreated) // 19.02.2021
          {
            // Отключаем команду "Select All", т.к. она не совместима по горячей клавише
            CommandItems.UseSelectAll = false;

            // Заменяем "таблицу для просмотра" на "столбцы для просмотра"
            if (Control.ReadOnly)
            {
              Control.ReadOnly = false;
              SetColumnsReadOnly(true);
            }
            // Разрешаем выбор множества строк
            Control.MultiSelect = true;
          }
          _MarkRowsGridColumn.ReadOnly = false;

        }
      }
    }
    private DataGridViewCheckBoxColumn _MarkRowsGridColumn;

    /// <summary>
    /// Альтернативная установка свойства MarkRowsColumn
    /// </summary>
    public int MarkRowsColumnIndex
    {
      get
      {
        if (MarkRowsGridColumn == null)
          return -1;
        else
          return MarkRowsGridColumn.Index;
      }
      set
      {
        if (value < 0)
          MarkRowsGridColumn = null;
        else
          MarkRowsGridColumn = Control.Columns[value] as DataGridViewCheckBoxColumn;
      }
    }

    /// <summary>
    /// Задает один из столбцов с списке Columns для установки отметок строк.
    /// По умолчанию возвращает null.
    /// </summary>
    public EFPDataGridViewColumn MarkRowsColumn
    {
      get
      {
        if (MarkRowsGridColumn == null)
          return null;
        else
          return Columns[MarkRowsGridColumn];
      }
      set
      {
        if (value == null)
          MarkRowsGridColumn = null;
        else
          MarkRowsGridColumn = value.GridColumn as DataGridViewCheckBoxColumn;
      }
    }

    /// <summary>
    /// Имя столбца в списке Columns, соответствующего MarkRowsColumn.
    /// </summary>
    public string MarkRowsColumnName
    {
      get
      {
        if (MarkRowsGridColumn == null)
          return String.Empty;
        else
          return Columns[MarkRowsGridColumn].Name;
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          MarkRowsGridColumn = null;
        else
          MarkRowsGridColumn = (Columns[value].GridColumn) as DataGridViewCheckBoxColumn;
      }
    }

    /// <summary>
    /// Установка отметок в CheckBoxColumn для требуемых строк
    /// Должно быть установлено свойство MarkRowsColumn
    /// Скрытые строки пропускаются
    /// </summary>
    /// <param name="rows">Какие строки обрабатываются</param>
    /// <param name="action">Установка, сброс или инверсия отметки</param>
    /// <returns>Число строк, которые были изменены</returns>
    public int CheckMarkRows(EFPDataGridViewCheckMarkRows rows, EFPDataGridViewCheckMarkAction action)
    {
      if (_MarkRowsGridColumn == null)
        throw new InvalidOperationException("Свойство MarkRowsColumn не установлено");

      // Отменяем режим редактирования и поиск по буквам
      CurrentIncSearchColumn = null;
      Control.EndEdit();

      int ColumnIndex = _MarkRowsGridColumn.Index;
      int cnt = 0;
      switch (rows)
      {
        case EFPDataGridViewCheckMarkRows.Selected:
          int[] SelRows = SelectedRowIndices;
          for (int i = 0; i < SelRows.Length; i++)
          {
            if (CheckMarkRow(SelRows[i], ColumnIndex, action))
              cnt++;
            Control.InvalidateCell(ColumnIndex, SelRows[i]);
          }
          break;
        case EFPDataGridViewCheckMarkRows.All:
          for (int i = 0; i < Control.RowCount; i++)
          {
            if (CheckMarkRow(i, ColumnIndex, action))
              cnt++;
          }
          Control.InvalidateColumn(ColumnIndex);
          break;
        default:
          throw new ArgumentException("Неивестный Rows=" + rows.ToString(), "rows");
      }
      return cnt;
    }

    private bool CheckMarkRow(int rowIndex, int columnIndex, EFPDataGridViewCheckMarkAction action)
    {
      DataGridViewRow Row;
      Row = Control.Rows.SharedRow(rowIndex);
      if (Row.Index >= 0)
      {
        // Для разделяемой строки такое действие не допускается
        if (!Row.Visible)
          return false;
        if (Row.ReadOnly)
          return false;
      }
      /*EFPDataGridViewRowAttributesEventArgs RowArgs = */
      DoGetRowAttributes(rowIndex, EFPDataGridViewAttributesReason.View);
      EFPDataGridViewCellAttributesEventArgs CellArgs = DoGetCellAttributes(columnIndex);
      bool OrgValue = DataTools.GetBool(CellArgs.Value);
      bool NewValue;
      switch (action)
      {
        case EFPDataGridViewCheckMarkAction.Check:
          NewValue = true;
          break;
        case EFPDataGridViewCheckMarkAction.Uncheck:
          NewValue = false;
          break;
        case EFPDataGridViewCheckMarkAction.Invert:
          NewValue = !OrgValue;
          break;
        default:
          throw new ArgumentException("Неизвестный Action=" + action.ToString(), "action");
      }

      if (NewValue == OrgValue)
        return false;
      // Требуется не Shared-строка
      Row = Control.Rows[rowIndex];
      Row.Cells[columnIndex].Value = NewValue;

      // 20.04.2014
      OnCellFinished(rowIndex, columnIndex, EFPDataGridViewCellFinishedReason.MarkRow);

      return true;
    }

    #endregion

    #region Автоподбор высоты строки

    /// <summary>
    /// Режим автоматического подбора высоты строк.
    /// По умолчанию автоподбор выключен (None).
    /// Свойство может устанавливаться только до вывода управляющего элемента на экран
    /// </summary>
    public EFPDataGridViewAutoSizeRowsMode AutoSizeRowsMode
    {
      get { return _AutoSizeRowsMode; }
      set
      {
        CheckHasNotBeenCreated();
        _AutoSizeRowsMode = value;
        if (value != EFPDataGridViewAutoSizeRowsMode.None)
          Control.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None; // отключаем стандартные средства
      }
    }
    private EFPDataGridViewAutoSizeRowsMode _AutoSizeRowsMode;

    #endregion

    #region Поддержка экспорта

    /// <summary>
    /// Получение двумерного массива текстовых значений для выбранных ячеек таблицы
    /// </summary>
    /// <param name="area">Выделенные ячейки</param>
    /// <returns>Двумерный массив строк. Первый индекс соответствует строкам, второй - столбцам</returns>
    public string[,] GetCellTextValues(EFPDataGridViewRectArea area)
    {
      string[,] a = new string[area.RowCount, area.ColumnCount];

      for (int i = 0; i < area.RowCount; i++)
      {
        DoGetRowAttributes(area.RowIndices[i], EFPDataGridViewAttributesReason.View);
        for (int j = 0; j < area.ColumnCount; j++)
        {
          EFPDataGridViewCellAttributesEventArgs CellArgs = DoGetCellAttributes(area.ColumnIndices[j]);
          a[i, j] = DoGetCellTextValue(CellArgs);
        }
      }

      return a;
    }

    private string DoGetCellTextValue(EFPDataGridViewCellAttributesEventArgs CellArgs)
    {
      if (CellArgs.FormattedValue == null)
        return String.Empty;
      else
        return CellArgs.FormattedValue.ToString();
    }

    /// <summary>
    /// Получение текстового значения для одной ячейки
    /// </summary>
    /// <param name="rowIndex">Индекс строки в просмотре</param>
    /// <param name="columnIndex">Индекс столбца в просмотре</param>
    /// <returns>Текстовое значение</returns>
    public string GetCellTextValue(int rowIndex, int columnIndex)
    {
      DoGetRowAttributes(rowIndex, EFPDataGridViewAttributesReason.View);
      EFPDataGridViewCellAttributesEventArgs CellArgs = DoGetCellAttributes(columnIndex);
      return DoGetCellTextValue(CellArgs);
    }


    /// <summary>
    /// Запись файла в формате HTML
    /// </summary>
    /// <param name="fileName">Путь к файлу</param>
    /// <param name="settings">Настройки экспорта</param>
    public void SaveHtml(string fileName, EFPDataGridViewExpHtmlSettings settings)
    {
      EFPDataGridViewExpHtml.SaveFile(this, fileName, settings);
    }

    /// <summary>
    /// Запись файла в формате Excel-2003 (XML)
    /// </summary>
    /// <param name="fileName">Путь к файлу</param>
    /// <param name="settings">Настройки экспорта</param>
    public void SaveExcel2003(string fileName, EFPDataGridViewExpExcelSettings settings)
    {
      EFPDataGridViewExpExcel2003.SaveFile(this, fileName, settings);
    }

    /// <summary>
    /// Запись файла в формате Excel-2007/2010 (XLSX)
    /// Вызов этого метода требует наличия сборки ICSharpCode.SharpZipLib.dll
    /// </summary>
    /// <param name="fileName">Путь к файлу</param>
    /// <param name="settings">Настройки экспорта</param>
    public void SaveExcel2007(string fileName, EFPDataGridViewExpExcelSettings settings)
    {
      EFPDataGridViewExpExcel2007.SaveFile(this, fileName, settings);
    }

    /// <summary>
    /// Запись файла в формате Open Office Calc (ODS)
    /// Вызов этого метода требует наличия сборки ICSharpCode.SharpZipLib.dll
    /// </summary>
    /// <param name="fileName">Путь к файлу</param>
    /// <param name="settings">Настройки экспорта</param>
    public void SaveOpenOfficeCalc(string fileName, EFPDataGridViewExpExcelSettings settings)
    {
      EFPDataGridViewExpOpenOfficeCalc.SaveFile(this, fileName, settings);
    }

    /// <summary>
    /// Получить массив строк и столбцов для записи в файл
    /// </summary>
    /// <param name="rangeMode">Заданный режим выделения</param>
    /// <returns>Массив строк и столбцов</returns>
    public EFPDataGridViewRectArea GetRectArea(EFPDataGridViewExpRange rangeMode)
    {
      switch (rangeMode)
      {
        case EFPDataGridViewExpRange.All:
          return new EFPDataGridViewRectArea(Control, EFPDataGridViewRectAreaCreation.Visible);
        case EFPDataGridViewExpRange.Selected:
          return new EFPDataGridViewRectArea(Control, EFPDataGridViewRectAreaCreation.Selected);
        default:
          throw new ArgumentException("Неизвестный режим выделения: " + rangeMode.ToString(), "rangeMode");
      }
    }

    /// <summary>
    /// Используется при получении иерархических заголовков столбцов методами GetColumnHeaderArray().
    /// Если true, то разрешено объединение по горизонтали для строк, когда выше имеются необъединенные ячейки.
    /// Если false (по умолчанию), то заголовки являются строго иерархическими, при этом может быть повторяющийся текст.
    /// </summary>
    public bool ColumnHeaderMixedSpanAllowed
    {
      get { return _ColumnHeaderMixedSpanAllowed; }
      set { _ColumnHeaderMixedSpanAllowed = value; }
    }
    private bool _ColumnHeaderMixedSpanAllowed;

    /// <summary>
    /// Возвращает массив заголовков столбцов, используемый при экспорте в электронные таблицы и других случаях, когда
    /// поддерживаются многоуровневые заголовки
    /// </summary>
    /// <param name="columns">Столбцы</param>
    /// <returns>Массив заголовков</returns>
    public virtual EFPDataGridViewColumnHeaderArray GetColumnHeaderArray(EFPDataGridViewColumn[] columns)
    {
      // Метод переопределен в программе "Бюджетный учет" для поддержки заголовков из параметров страницы.

#if DEBUG
      if (columns == null)
        throw new ArgumentNullException("columns");
#endif

      #region Получение массива заголовков столбцов

      string[][] Headers = new string[columns.Length][];

      for (int i = 0; i < columns.Length; i++)
      {
        Headers[i] = columns[i].PrintHeaders;
        if (Headers[i] == null)
          Headers[i] = new string[] { columns[i].GridColumn.HeaderText };
      }

      #endregion

      EFPDataGridViewColumnHeaderArray ha = new EFPDataGridViewColumnHeaderArray(Headers, ColumnHeaderMixedSpanAllowed);
      return ha;
    }

    /// <summary>
    /// Создание описателей заголовков столбцов для выбранной области ячеек.
    /// </summary>
    /// <param name="area">Выбранная область. Используются только столбцы, строки игнорируются</param>
    /// <returns>Описания столбцов</returns>
    public /*virtual*/ EFPDataGridViewColumnHeaderArray GetColumnHeaderArray(EFPDataGridViewRectArea area)
    {
#if DEBUG
      if (area == null)
        throw new ArgumentNullException("area");
#endif

      EFPDataGridViewColumn[] SelCols = new EFPDataGridViewColumn[area.ColumnCount];
      for (int i = 0; i < area.ColumnCount; i++)
        SelCols[i] = Columns[area.ColumnIndices[i]];
      return GetColumnHeaderArray(SelCols);
    }

    /// <summary>
    /// Получить сводку документа при экспорте.
    /// Реализация по умолчанию возвращает копию EFPApp.DefaultDocProperties
    /// с установленным свойством Title в соответствии с DisplayName или
    /// заголовком окна, если просмотр является единственным управляющим элементом
    /// </summary>
    public virtual EFPDocumentProperties DocumentProperties
    {
      // Свойство доопределяется в программе "Бюджетный учет" для учета имени текущего пользователя в качестве автора

      get
      {
        EFPDocumentProperties Props = EFPApp.DefaultDocumentProperties;
        //if (String.IsNullOrEmpty(DisplayName))
        try
        {
          Props.Title = WinFormsTools.GetControlText(Control);
        }
        catch
        {
        }
        //else
        //  Props.Title = DisplayName;

        return Props;
      }
    }

    /// <summary>
    /// Возвращает true, если есть установленная версия Microsoft Excel, в которую можно выполнить передачу данных табличного просмотра
    /// </summary>
    public static bool CanSendToMicrosoftExcel
    {
      get
      {
        // Для версии XP и новее выполняется создание файла в XML-формате,
        // Для версии 2000 выполняется прямое манипулирование OLE-объектом
        return EFPApp.MicrosoftExcelVersion.Major >= MicrosoftOfficeTools.MicrosoftOffice_2000;
      }
    }

    /// <summary>
    /// Возвращает true, если есть установленная версия Open Office или Libre Office Calc, в которую можно выполнить передачу данных табличного просмотра
    /// </summary>
    public static bool CanSendToOpenOfficeCalc
    {
      [DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
      get
      {
        return EFPApp.OpenOfficeCalcVersion.Major >= 3
          && ZipFileTools.ZipLibAvailable; // 25.01.2014
      }
    }

    /// <summary>
    /// Передача данных табличного просмотра в Microsoft Excel.
    /// Создается новая книга с одним листом, содержащим все или только выбранные ячейки табличного просмотра
    /// </summary>
    /// <param name="settings">Параметры передачи</param>
    public void SendToMicrosoftExcel(EFPDataGridViewExpExcelSettings settings)
    {
      EFPDataGridViewSendToExcel.SendTable(this, settings);
    }

    /// <summary>
    /// Передача данных табличного просмотра в Open Office Calc или Libre Office Calc.
    /// Создается новая книга с одним листом, содержащим все или только выбранные ячейки табличного просмотра
    /// </summary>
    /// <param name="settings">Параметры передачи</param>
    public void SendToOpenOfficeCalc(EFPDataGridViewExpExcelSettings settings)
    {
      EFPDataGridViewSendToOpenOfficeCalc.SendTable(this, settings);
    }

    #endregion

    #region Отладка SharedRow

    /// <summary>
    /// Для отладочных целей.
    /// Посчитать, сколько строк Shared.
    /// Выполняется медленно, т.к. требцется перебрать все строки в просмотре.
    /// </summary>
    public int UnsharedRowCount
    {
      get
      {
        int cnt = 0;
        int n = Control.RowCount;
        for (int i = 0; i < n; i++)
        {
          DataGridViewRow Row = Control.Rows.SharedRow(i);
          if (Row.Index >= 0)
            cnt++;
        }
        return cnt;
      }
    }

    /// <summary>
    /// Для отладочных целей.
    /// Возвращает количество строк в просмотре, для которых еще не было выполнено Unshare.
    /// Выполняется медленно, т.к. требцется перебрать все строки в просмотре.
    /// </summary>
    public int SharedRowCount
    {
      get
      {
        int cnt = 0;
        int n = Control.RowCount;
        for (int i = 0; i < n; i++)
        {
          DataGridViewRow Row = Control.Rows.SharedRow(i);
          if (Row.Index < 0)
            cnt++;
        }
        return cnt;
      }
    }


    #endregion

    #region Получение EFPDataViewRowInfo

    private DataRowValueArray _RowValueArray;

    /// <summary>
    /// Получить объект EFPDataViewRowInfo для строки.
    /// Когда он больше не нужен, должен быть вызван FreeRowInfo()
    /// </summary>
    /// <param name="rowIndex">Индекс строки табличного просмотра</param>
    /// <returns>Информация о строке</returns>
    public EFPDataViewRowInfo GetRowInfo(int rowIndex)
    {
      IDataRowNamedValuesAccess rwa = _RowValueArray;
      if (rwa == null) // Первый вызов или вложенный
        rwa = CreateRowValueAccessObject();

      // 06.07.2021
      // Если использовать таблицу-повторитель, а не исходную таблицу, то возникает проблема в EFPSubDocGridView (ExtDBDocForms.dll).
      // Если выполняется команда "Создать копию", то для ссылочных полей значения, ссылающиеся на поддокументы этого же документа,
      // не будут возвращаться. Для таких ссылок используются фиктивные отрицательные идентификаторы. DBxDocTextHandlers
      // использует поиск в наборе данных DataSet, к которому относится строка DataRow. Для таблицы-повторителя нет набора
      // данных. Без таблицы-повторителя-это набор данных, относящийся к DBxDocSet, который позволяет получить значения для всех фиктивных ссылок.
      // Поэтому, строка DataRowValueArray.CurrentRow должна относиться к мастер-таблице, если используется повторитель.

      //rwa.CurrentRow = GetDataRow(rowIndex);
      rwa.CurrentRow = GetMasterRow(GetDataRow(rowIndex));

      return new EFPDataViewRowInfo(this, rwa.CurrentRow, rwa, rowIndex);
    }

    /// <summary>
    /// Создает объект DataRowValueArray для доступа к данным.
    /// Класс-наследник может создавать объект, который, например, использует буферизацию данных
    /// </summary>
    /// <returns>Объект для доступа к данным</returns>
    protected virtual IDataRowNamedValuesAccess CreateRowValueAccessObject()
    {
      return new DataRowValueArray();
    }

    /// <summary>
    /// Освободить данные, полученные GetRowInfo().
    /// </summary>
    /// <param name="rowInfo"></param>
    public void FreeRowInfo(EFPDataViewRowInfo rowInfo)
    {
      _RowValueArray = rowInfo.Values as DataRowValueArray; // для повторного использования.
      //      if (_RowValueArray != null)
      //        _RowValueArray.CurrentRow = null;
    }

    #endregion
  }


  /// <summary>
  /// Исключение, выбрасываемое, если источник данных (например, свойство DataGridView.DataSource)
  /// имеет неподходящий тип.
  /// </summary>
  [Serializable]
  public class InvalidDataSourceException : ApplicationException
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект исключения
    /// </summary>
    /// <param name="message">Сообщение</param>
    public InvalidDataSourceException(string message)
      : this(message, null)
    {
    }

    /// <summary>
    /// Создает новый объект исключения с вложенным исключением
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="innerException">Вложенное исключение</param>
    public InvalidDataSourceException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected InvalidDataSourceException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }
}
