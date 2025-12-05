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
using FreeLibSet.UICore;
using FreeLibSet.Reporting;
using FreeLibSet.Collections;

namespace FreeLibSet.Forms
{
  #region Цветовые атрибуты ячейки

  #region Перечисления

  #region EFPDataViewInfoReason

  /// <summary>
  /// Для чего вызывается событие <see cref="EFPDataGridView.RowInfoNeeded"/> или <see cref="EFPDataGridView.CellInfoNeeded"/>
  /// </summary>
  public enum EFPDataViewInfoReason
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
    /// Обрабатывается сообщение <see cref="DataGridView.CellToolTipTextNeeded"/>
    /// </summary>
    ToolTip,

    /// <summary>
    /// Определяется возможность редактирования ячейки. Обработчик события 
    /// <see cref="DataGridView.CellBeginEdit"/> или вызван метод <see cref="EFPDataGridView.GetCellReadOnly(DataGridViewCell, out string)"/>.
    /// </summary>
    ReadOnly,
  }

  #endregion

  #endregion

  /// <summary>
  /// Базовый класс для <see cref="EFPDataGridViewRowInfoEventArgs"/> и 
  /// <see cref="EFPDataGridViewCellInfoEventArgs"/>
  /// </summary>
  public class EFPDataGridViewInfoEventArgsBase : EventArgs
  {
    #region Конструктор

    internal EFPDataGridViewInfoEventArgsBase(EFPDataGridView controlProvider, bool isCellAttributes)
    {
      _ControlProvider = controlProvider;
      _IsCellAttributes = isCellAttributes;
    }

    #endregion

    #region Свойства - исходные данные

    /// <summary>
    /// Провайдер табличного просмотра, для которого вызвано событие
    /// </summary>
    public EFPDataGridView ControlProvider { get { return _ControlProvider; } }
    private readonly EFPDataGridView _ControlProvider;

    /// <summary>
    /// Табличный просмотр
    /// </summary>
    public DataGridView Control { get { return ControlProvider.Control; } }

    /// <summary>
    /// Зачем было вызвано событие: для вывода на экран, для печати, получения подсказки, определения режима ReadOnly.
    /// В прикладном коде обычно нет необходимости анализировать это свойство. Проще установить в обработчике все свойства сразу.
    /// Исключение, если вычисление какого-либо свойства (например, <see cref="ReadOnly"/>) является достаточно затратным и
    /// желательно избежать задержек. Свойства, используемые для режима <see cref="EFPDataViewInfoReason.View"/>
    /// обязательно должны в обработчике события вычисляться быстро, чтобы не вызывать задержек при прорисовке пользовательского интерфейса.
    /// </summary>
    public EFPDataViewInfoReason Reason { get { return _Reason; } }
    private EFPDataViewInfoReason _Reason;

    /// <summary>
    /// Индекс строки, для которой требуются атрибуты, в табличном просмотре
    /// </summary>
    public int RowIndex { get { return _RowIndex; } }
    private int _RowIndex;

    /// <summary>
    /// Возвращает объект, привязанный к строке данных (например, <see cref="DataRowView"/>), или null.
    /// </summary>
    public object DataBoundItem { get { return ControlProvider.GetDataBoundItem(_RowIndex); } }

    /// <summary>
    /// Доступ к строке таблицы данных, если источником данных для табличного
    /// просмотра является <see cref="DataView"/>. Иначе - null
    /// </summary>
    public DataRowView RowView
    {
      get
      {
        return ControlProvider.GetDataBoundItem(_RowIndex) as DataRowView;
      }
    }

    /// <summary>
    /// Доступ к строке таблицы данных, если источником данных для табличного
    /// просмотра является <see cref="DataView"/>. Иначе - null
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
    /// <returns>Строка табличного просмотра</returns>
    public DataGridViewRow GetGridRow()
    {
      if (RowIndex < 0)
        return null;
      return Control.Rows[RowIndex];
    }

    private readonly bool _IsCellAttributes;

    #endregion

    #region Устанавливаемые свойства

    /// <summary>
    /// Цветовое оформление строки или ячейки.
    /// Установка значения в <see cref="UIDataViewColorType.TotalRow"/> для объекта <see cref="EFPDataGridViewRowInfoEventArgs"/> приводит дополнительно
    /// к установке свойств <see cref="TopBorder"/> и <see cref="BottomBorder"/> в значение <see cref="UIDataViewBorderStyle.Thick"/>, что удобно для оформления таблиц отчетов.
    /// Чтобы посторониие свойства не устанавливались, используйте метод <see cref="SetColorTypeOnly(UIDataViewColorType)"/>.
    /// 
    /// Используется при вызове в режимах <see cref="Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.Print"/>.
    /// </summary>
    public UIDataViewColorType ColorType
    {
      get { return _ColorType; }
      set
      {
        _ColorType = value;
        if (value == UIDataViewColorType.TotalRow)
        {
          if (!_IsCellAttributes) // 17.07.2019
          {
            //LeftBorder = EFPDataGridViewBorderStyle.Thick;
            TopBorder = UIDataViewBorderStyle.Thick;
            //RightBorder = EFPDataGridViewBorderStyle.Thick;
            BottomBorder = UIDataViewBorderStyle.Thick;
          }
        }
      }
    }
    private UIDataViewColorType _ColorType;

    /// <summary>
    /// Установка свойства <see cref="ColorType"/> без неявной установки других свойств.
    /// Используется при вызове в режимах <see cref="Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.Print"/>.
    /// </summary>
    /// <param name="value">Значение свойства</param>
    public void SetColorTypeOnly(UIDataViewColorType value)
    {
      _ColorType = value;
    }

    /// <summary>
    /// Если установить в true, то текст ячейки будет рисоваться серым шрифтом.
    /// По умолчанию - false.
    /// Используется при вызове в режимах <see cref="Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.Print"/>.
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
    /// Левая граница ячейки, используемая при печати.
    /// Используется при вызове в режиме <see cref="Reason"/>=<see cref="EFPDataViewInfoReason.Print"/>.
    /// </summary>
    public UIDataViewBorderStyle LeftBorder { get { return _LeftBorder; } set { _LeftBorder = value; } }
    private UIDataViewBorderStyle _LeftBorder;

    /// <summary>
    /// Верняя граница ячейки, используемая при печати.
    /// Используется при вызове в режиме <see cref="Reason"/>=<see cref="EFPDataViewInfoReason.Print"/>.
    /// </summary>
    public UIDataViewBorderStyle TopBorder { get { return _TopBorder; } set { _TopBorder = value; } }
    private UIDataViewBorderStyle _TopBorder;

    /// <summary>
    /// Правая граница ячейки, используемая при печати.
    /// Используется при вызове в режиме <see cref="Reason"/>=<see cref="EFPDataViewInfoReason.Print"/>.
    /// </summary>
    public UIDataViewBorderStyle RightBorder { get { return _RightBorder; } set { _RightBorder = value; } }
    private UIDataViewBorderStyle _RightBorder;

    /// <summary>
    /// Нижняя граница ячейки, используемая при печати.
    /// Используется при вызове в режиме <see cref="Reason"/>=<see cref="EFPDataViewInfoReason.Print"/>.
    /// </summary>
    public UIDataViewBorderStyle BottomBorder { get { return _BottomBorder; } set { _BottomBorder = value; } }
    private UIDataViewBorderStyle _BottomBorder;

    /// <summary>
    /// Перечеркивание ячейки от левого нижнего угла к правому верхнему.
    /// В отличие от других границ, работает в режиме просмотра (в режимах <see cref="Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.Print"/>).
    /// Значение <see cref="UIDataViewBorderStyle.Default"/> идентично <see cref="UIDataViewBorderStyle.None"/>.
    /// </summary>
    public UIDataViewBorderStyle DiagonalUpBorder { get { return _DiagonalUpBorder; } set { _DiagonalUpBorder = value; } }
    private UIDataViewBorderStyle _DiagonalUpBorder;

    /// <summary>
    /// Перечеркивание ячейки от левого верхнего угла к правому нижнему.
    /// В отличие от других границ, работает в режиме просмотра (в режимах <see cref="Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.Print"/>).
    /// Значение <see cref="UIDataViewBorderStyle.Default"/> идентично <see cref="UIDataViewBorderStyle.None"/>.
    /// </summary>
    public UIDataViewBorderStyle DiagonalDownBorder { get { return _DiagonalDownBorder; } set { _DiagonalDownBorder = value; } }
    private UIDataViewBorderStyle _DiagonalDownBorder;

    /// <summary>
    /// Если установить в true, то ячейка будет недоступна для редактирования по месту.
    /// Событие не вызывается из <see cref="EFPDataGridView.PerformEditData(UIDataState)"/>, если редактирование выполняется
    /// в пользовательском обработчике <see cref="EFPDataGridView.EditData"/>.
    /// Используется при вызове в режиме <see cref="Reason"/>=<see cref="EFPDataViewInfoReason.ReadOnly"/>.
    /// Можно задать более информативное сообщение для вывода пользователю, если установить свойство <see cref="ReadOnlyMessage"/>.
    /// </summary>
    public bool ReadOnly { get { return _ReadOnly; } set { _ReadOnly = value; } }
    private bool _ReadOnly;

    /// <summary>
    /// Текст сообщения, почему нельзя редактировать строку / ячейку.
    /// Применяется совместно с установкой свойства <see cref="ReadOnly"/>=true, если стандартное сообщение недостаточно информативно.
    /// Не используйте вместо установки этого свойства прямой вызов методов <see cref="EFPApp.ErrorMessageBox(string)"/> или <see cref="EFPApp.ShowTempMessage(string)"/>,
    /// так как событие может быть вызвано не только, когда пользователь пытается начать редактирование.
    /// Используется при вызове в режиме <see cref="Reason"/>=<see cref="EFPDataViewInfoReason.ReadOnly"/>.
    /// </summary>
    public string ReadOnlyMessage { get { return _ReadOnlyMessage; } set { _ReadOnlyMessage = value; } }
    private string _ReadOnlyMessage;

    #endregion

    #region Методы

    /// <summary>
    /// Устанавливает четыре границы (свойства <see cref="LeftBorder"/>, <see cref="TopBorder"/>, <see cref="RightBorder"/>, <see cref="BottomBorder"/> ), используемые для печати.
    /// Свойства <see cref="DiagonalUpBorder"/> и <see cref="DiagonalDownBorder"/> не меняются.
    /// Используется при вызове в режиме <see cref="Reason"/>=<see cref="EFPDataViewInfoReason.Print"/>.
    /// </summary>
    /// <param name="style">Вид границы</param>
    public void SetAllBorders(UIDataViewBorderStyle style)
    {
      SetAllBorders(style, false);
    }

    /// <summary>
    /// Устанавливает четыре границы (свойства <see cref="LeftBorder"/>, <see cref="TopBorder"/>, <see cref="RightBorder"/>, <see cref="BottomBorder"/> ), используемые для печати.
    /// Также может устанавливать свойства <see cref="DiagonalUpBorder"/> и <see cref="DiagonalDownBorder"/>.
    /// Используется в режиме просмотра (в режимах <see cref="Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.Print"/>).
    /// </summary>
    /// <param name="style">Вид границы</param>
    /// <param name="diagonals">Если true, то дополнительно будет установлены свойства <see cref="DiagonalUpBorder"/> и <see cref="DiagonalDownBorder"/></param>
    public void SetAllBorders(UIDataViewBorderStyle style, bool diagonals)
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
    /// Инициализация свойств объекта.
    /// </summary>
    /// <param name="rowIndex">Индекс строки табличного просмотра (свойство <see cref="RowIndex"/>)</param>
    /// <param name="reason">Свойство <see cref="Reason"/></param>
    protected void InitRow(int rowIndex, EFPDataViewInfoReason reason)
    {
      _RowIndex = rowIndex;
      _Reason = reason;
      if ((RowIndex & 0x1) != 0 && ControlProvider.UseAlternation)
        _ColorType = UIDataViewColorType.Alter;
      else
        _ColorType = UIDataViewColorType.Normal;
      _Grayed = null;
      LeftBorder = UIDataViewBorderStyle.Default;
      TopBorder = UIDataViewBorderStyle.Default;
      RightBorder = UIDataViewBorderStyle.Default;
      BottomBorder = UIDataViewBorderStyle.Default;
      DiagonalUpBorder = UIDataViewBorderStyle.Default;
      DiagonalDownBorder = UIDataViewBorderStyle.Default;
    }

    #endregion
  }

  /// <summary>
  /// Аргументы события <see cref="EFPDataGridView.RowInfoNeeded"/>
  /// </summary>
  public class EFPDataGridViewRowInfoEventArgs : EFPDataGridViewInfoEventArgsBase
  {
    #region Конструктор

    /// <summary>
    /// Создает аргументы события
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра</param>
    public EFPDataGridViewRowInfoEventArgs(EFPDataGridView controlProvider)
      : base(controlProvider, false)
    {
      CellErrorMessages = new Dictionary<int, ErrorMessageList>();
    }

    #endregion

    #region Устанавливаемые свойства

    /// <summary>
    /// При печати запретить отрывать текущую строку от предыдущей.
    /// Используется при вызове в режиме <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.Print"/>.
    /// </summary>
    public bool PrintWithPrevious { get { return _PrintWithPrevious; } set { _PrintWithPrevious = value; } }
    private bool _PrintWithPrevious;

    /// <summary>
    /// При печати запретить отрывать текущую строку от следующей.
    /// Используется при вызове в режиме <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.Print"/>.
    /// </summary>
    public bool PrintWithNext { get { return _PrintWithNext; } set { _PrintWithNext = value; } }
    private bool _PrintWithNext;

    /// <summary>
    /// Изображение, которое будет выведено в заголовке строки (в серой ячейке)
    /// Должно быть установлено свойство <see cref="EFPDataGridView.UseRowImages"/>
    /// Это свойство также используется при навигации по Ctrl-[, Ctrl-].
    /// Стандартный значок может быть переопределен установкой свойства <see cref="UserImage"/>.
    /// Используется при вызове в режиме <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/>.
    /// 
    /// Вместо прямой установки свойств <see cref="ImageKind"/> и <see cref="ToolTipText"/> обычно удобнее использовать
    /// методы <see cref="AddRowErrorMessage(ErrorMessageItem, string)"/> и производные от него.
    /// </summary>
    public UIDataViewImageKind ImageKind { get { return _ImageKind; } set { _ImageKind = value; } }
    private UIDataViewImageKind _ImageKind;

    /// <summary>
    /// Переопределение изображения, которое будет выведено в заголовке строки 
    /// (в серой ячейке)
    /// Если свойство не установлено (по умолчанию), то используются стандартные
    /// значки для <see cref="ImageKind"/>=Information,Warning и Error, или пустое изображение
    /// при <see cref="ImageKind"/>=None. 
    /// Свойство не влияет на навигацию по строкам по Ctrl-[, Ctrl-], поэтому установка этого свойства не отменяет
    /// необходимость установки свойства <see cref="ImageKind"/>.
    /// Должно быть установлено свойство <see cref="EFPDataGridView.UseRowImages"/>.
    /// Используется при вызове в режиме <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/>.
    /// </summary>
    public Image UserImage { get { return _UserImage; } set { _UserImage = value; } }
    private Image _UserImage;

    /// <summary>
    /// Всплывающая подсказка, которая будет выведена при наведении курсора на 
    /// ячейку заголовка строки. 
    /// Используется при вызове в режиме <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.ToolTip"/>.
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
    /// Если свойство явно не устанавливается, то используется значение по умолчанию <see cref="DefaultRowIdText"/>.
    /// Используется при вызове в режиме <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.ToolTip"/>.
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
    /// Используется, если свойство <see cref="RowIdText"/> не установлено в обработчике события.
    /// Если установлено свойство <see cref="EFPDataGridView.RowIdTextDataColumnName"/> и просмотр присоединен к таблице данных,
    /// то возвращается значение поля из строки <see cref="EFPDataGridViewInfoEventArgsBase.DataRow"/>.
    /// Иначе возвращается порядковый номер строки.
    /// Используется при вызове в режиме <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.ToolTip"/>.
    /// </summary>
    public string DefaultRowIdText
    {
      get
      {
        if (!String.IsNullOrEmpty(ControlProvider.RowIdTextDataColumnName))
        {
          if (DataRow == null)
            return Res.EFPDataView_Msg_RowIdText_NoDataRow;
          return DataTools.GetString(DataRow, ControlProvider.RowIdTextDataColumnName);
        }

        // Значение по умолчанию
        return String.Format(Res.EFPDataView_Msg_RowIdText_RowNumber, RowIndex + 1);
      }
    }

    /// <summary>
    /// Используется вместо штучной установки свойства <see cref="EFPDataGridViewCellInfoEventArgs.ContentVisible"/> в обработчике
    /// <see cref="EFPDataGridView.CellInfoNeeded"/>. По умолчанию - true. Если свойство сбросить в false,
    /// то для всех ячеек типа <see cref="DataGridViewCheckBoxCell"/>, <see cref="DataGridViewButtonCell"/> и
    /// <see cref="DataGridViewComboBoxCell"/> будет устанавливаться свойство <see cref="EFPDataGridViewCellInfoEventArgs.ContentVisible"/>=false.
    /// Используется при вызове в режиме <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/>.
    /// </summary>
    public bool ControlContentVisible { get { return _ControlContentVisible; } set { _ControlContentVisible = value; } }
    private bool _ControlContentVisible;

    #endregion

    #region Сообщения об ошибках и подсказки

    #region Основные методы

    /// <summary>
    /// Устанавливает для строки подходящее изображение <see cref="ImageKind"/> и добавляет сообщения
    /// к свойству <see cref="ToolTipText"/> из списка ошибок.
    /// Устанавливается значок "максимальной серьезности" из ранее установленного значения <see cref="ImageKind"/> и <see cref="ErrorMessageItem.Kind"/>.
    /// Сообщения добавляются в конец <see cref="ToolTipText"/> и отделяются от существующего текста символами новой строки <see cref="Environment.NewLine"/>.
    /// Эквивалентно вызову метода <see cref="AddRowErrorMessage(ErrorMessageItem)"/> для каждого сообщения в списке.
    /// Используется при вызове в режимах <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.ToolTip"/>.
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
    /// Устанавливает для строки подходящее изображение <see cref="ImageKind"/> и добавляет сообщение <paramref name="message"/>
    /// к свойству <see cref="ToolTipText"/>.
    /// Устанавливается значок "максимальной серьезности" из ранее установленного значения <see cref="ImageKind"/> и <see cref="ErrorMessageItem.Kind"/>.
    /// Сообщение добавляется в конец <see cref="ToolTipText"/> и отделяется от существующего текста символом новой строки <see cref="Environment.NewLine"/>.
    /// Эквивалентно одному из вызовов: <see cref="AddRowError(string)"/>, <see cref="AddRowWarning(string)"/> или
    /// Используется при вызове в режимах <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.ToolTip"/>.
    /// <see cref="AddRowInformation(string)"/>
    /// </summary>
    /// <param name="kind">Важность: Ошибка, предупреждение или сообщение</param>
    /// <param name="message">Текст сообщения</param>
    public void AddRowErrorMessage(ErrorMessageKind kind, string message)
    {
      AddRowErrorMessage(new ErrorMessageItem(kind, message));
    }

    /// <summary>
    /// Устанавливает для строки подходящее изображение <see cref="ImageKind"/> и добавляет сообщение
    /// к свойству <see cref="ToolTipText"/>.
    /// Устанавливается значок "максимальной серьезности" из ранее установленного значения <see cref="ImageKind"/> и <see cref="ErrorMessageItem.Kind"/>.
    /// Сообщение добавляется в конец <see cref="ToolTipText"/> и отделяется от существующего текста символом новой строки <see cref="Environment.NewLine"/>.
    /// Эквивалентно одному из вызовов: <see cref="AddRowError(string)"/>, <see cref="AddRowWarning(string)"/> или
    /// <see cref="AddRowInformation(string)"/>.
    /// Используется при вызове в режимах <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.ToolTip"/>.
    /// </summary>
    /// <param name="item">Объект ошибки ErrorMessageItem</param>
    public void AddRowErrorMessage(ErrorMessageItem item)
    {
      AddRowErrorMessage(item, String.Empty);
    }

    /// <summary>
    /// Устанавливает для строки подходящее изображение <see cref="ImageKind"/> и добавляет сообщение
    /// к свойству <see cref="ToolTipText"/>.
    /// Позволяет привязать сообщение об ошибке и задать подсветку для выбранных ячеек строки.
    /// Устанавливается значок "максимальной серьезности" из ранее установленного значения <see cref="ImageKind"/> и <see cref="ErrorMessageItem.Kind"/>.
    /// Сообщение добавляется в конец <see cref="ToolTipText"/> и отделяется от существующего текста символом новой строки <see cref="Environment.NewLine"/>.
    /// Эквивалентно одному из вызовов: <see cref="AddRowError(string)"/>, <see cref="AddRowWarning(string)"/> или
    /// <see cref="AddRowInformation(string)"/>.
    /// Используется при вызове в режимах <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.ToolTip"/>.
    /// </summary>
    /// <param name="item">Объект ошибки <see cref="ErrorMessageItem"/></param>
    /// <param name="columnNames">Список имен столбцов (через запятую), для которых задается подсветка ошибки</param>
    public void AddRowErrorMessage(ErrorMessageItem item, string columnNames)
    {
      UIDataViewImageKind newImageKind;
      switch (item.Kind)
      {
        case ErrorMessageKind.Error: newImageKind = UIDataViewImageKind.Error; break;
        case ErrorMessageKind.Warning: newImageKind = UIDataViewImageKind.Warning; break;
        case ErrorMessageKind.Info: newImageKind = UIDataViewImageKind.Information; break;
        default:
          throw ExceptionFactory.ArgProperty("item", item, "Kind", item.Kind, null);
      }

      if ((int)newImageKind > (int)ImageKind)
        ImageKind = newImageKind;

      AddToolTipText(item.Text);
      if (RowErrorMessages != null)
        RowErrorMessages.Add(new ErrorMessageItem(item.Kind, item.Text, item.Code, RowIndex)); // исправлено 09.11.2014

      if (!String.IsNullOrEmpty(columnNames))
      {
        if (columnNames.IndexOf(',') >= 0)
        {
          string[] a = columnNames.Split(',');
          for (int i = 0; i < a.Length; i++)
            AddOneCellErrorMessage(item, a[i]);
        }
        else
          AddOneCellErrorMessage(item, columnNames);
      }
    }

    private void AddOneCellErrorMessage(ErrorMessageItem error, string columnName)
    {
      int columnIndex = ControlProvider.Columns.IndexOf(columnName);
      if (columnIndex < 0)
        return; // имя несуществующего столбца
      ErrorMessageList errors;
      if (!CellErrorMessages.TryGetValue(columnIndex, out errors))
      {
        errors = new ErrorMessageList();
        CellErrorMessages.Add(columnIndex, errors);
      }
      errors.Add(error);
    }

    private void AddToolTipText(string message)
    {
      if (Reason != EFPDataViewInfoReason.ToolTip &&
        Reason != EFPDataViewInfoReason.View) // 15.09.2015 Сообщения для невиртуального просмотра
        return;

#if DEBUG
      if (String.IsNullOrEmpty(message))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("message");
#endif

      if (String.IsNullOrEmpty(ToolTipText))
        ToolTipText = message;
      else
        ToolTipText += Environment.NewLine + message;
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Устанавливает для строки изображение ошибки <see cref="ImageKind"/>=<see cref="UIDataViewImageKind.Error"/> 
    /// и добавляет сообщение к свойству <see cref="ToolTipText"/>.
    /// Сообщение добавляется в конец <see cref="ToolTipText"/> и отделяется от существующего текста символом новой строки <see cref="Environment.NewLine"/>.
    /// </summary>
    /// <param name="message">Сообщение</param>
    public void AddRowError(string message)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Error, message), String.Empty);
    }

    /// <summary>
    /// Устанавливает для строки изображение ошибки <see cref="ImageKind"/>=<see cref="UIDataViewImageKind.Error"/> 
    /// и добавляет сообщение к свойству <see cref="ToolTipText"/>.
    /// Сообщение добавляется в конец <see cref="ToolTipText"/> и отделяется от существующего текста символом новой строки <see cref="Environment.NewLine"/>.
    /// Позволяет привязать сообщение об ошибке и задать подсветку для выбранных ячеек строки.
    /// Используется при вызове в режимах <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.ToolTip"/>.
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="columnNames">Список имен столбцов (через запятую), для которых задается подсветка ошибки</param>
    public void AddRowError(string message, string columnNames)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Error, message), columnNames);
    }

    /// <summary>
    /// Устанавливает для строки изображение предупреждения <see cref="ImageKind"/>=<see cref="UIDataViewImageKind.Warning"/>,
    /// если оно не установлено уже в ошибку.
    /// Сообщение добавляется в конец <see cref="ToolTipText"/> и отделяется от существующего текста символом новой строки <see cref="Environment.NewLine"/>.
    /// Используется при вызове в режимах <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.ToolTip"/>.
    /// </summary>
    /// <param name="message">Сообщение</param>
    public void AddRowWarning(string message)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Warning, message), String.Empty);
    }

    /// <summary>
    /// Устанавливает для строки изображение предупреждения <see cref="ImageKind"/>=<see cref="UIDataViewImageKind.Warning"/>,
    /// если оно не установлено уже в ошибку.
    /// Сообщение добавляется в конец <see cref="ToolTipText"/> и отделяется от существующего текста символом новой строки <see cref="Environment.NewLine"/>.
    /// Позволяет привязать предупреждение и задать подсветку для выбранных ячеек строки.
    /// Используется при вызове в режимах <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.ToolTip"/>.
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="columnNames">Список имен столбцов (через запятую), для которых задается подсветка предупреждения</param>
    public void AddRowWarning(string message, string columnNames)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Warning, message), columnNames);
    }

    /// <summary>
    /// Устанавливает для строки изображение предупреждения <see cref="ImageKind"/>=<see cref="UIDataViewImageKind.Information"/>,
    /// если оно не установлено уже в ошибку или предупреждение.
    /// Сообщение добавляется в конец <see cref="ToolTipText"/> и отделяется от существующего текста символом новой строки <see cref="Environment.NewLine"/>.
    /// Используется при вызове в режимах <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.ToolTip"/>.
    /// </summary>
    /// <param name="message">Сообщение</param>
    public void AddRowInformation(string message)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Info, message), String.Empty);
    }

    /// <summary>
    /// Устанавливает для строки изображение предупреждения <see cref="ImageKind"/>=<see cref="UIDataViewImageKind.Information"/>,
    /// если оно не установлено уже в ошибку или предупреждение.
    /// Позволяет привязать сообщение для выбранных ячеек строки.
    /// Используется при вызове в режимах <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.ToolTip"/>.
    /// </summary>
    /// <param name="message">Сообщение</param>
    /// <param name="columnNames">Список имен столбцов (через запятую), для которых задается всплывающая подсказка</param>
    public void AddRowInformation(string message, string columnNames)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Info, message), columnNames);
    }

    #endregion

    #endregion

    #region Защищенные методы

    /// <summary>
    /// Инициализация аргументов
    /// </summary>
    /// <param name="rowIndex">Индекс строки табличного просмотра</param>
    /// <param name="reason">Свойство Reason</param>
    public new void InitRow(int rowIndex, EFPDataViewInfoReason reason)
    {
      base.InitRow(rowIndex, reason);
      PrintWithPrevious = false;
      PrintWithNext = false;
      ImageKind = UIDataViewImageKind.None;
      UserImage = null;
      ToolTipText = String.Empty;

      // Эмуляция стандартного поведения DataGridView по обработке DataRow.RowError
      if (ControlProvider.UseRowImagesDataError)
      {
        DataRow row = this.DataRow;
        if (row != null)
        {
          ToolTipText = row.RowError;
          if (!String.IsNullOrEmpty(ToolTipText))
            ImageKind = UIDataViewImageKind.Error;
        }
      }

      ReadOnly = false;
      ReadOnlyMessage = Res.EFPDataView_Msg_ReadOnlyRow;
      if (reason == EFPDataViewInfoReason.ReadOnly)
      {
        DataGridViewRow gridRow = ControlProvider.Control.Rows.SharedRow(rowIndex);
        ReadOnly = (gridRow.GetState(rowIndex) & DataGridViewElementStates.ReadOnly) == DataGridViewElementStates.ReadOnly;
      }

      ControlContentVisible = true;
      CellErrorMessages.Clear();
      RowIdText = null;
    }

    #endregion
  }


  /// <summary>
  /// Аргументы события <see cref="EFPDataGridView.CellInfoNeeded"/>
  /// </summary>
  public class EFPDataGridViewCellInfoEventArgs : EFPDataGridViewInfoEventArgsBase
  {
    #region Конструктор

    /// <summary>
    /// Создает аргуметы события
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра</param>
    public EFPDataGridViewCellInfoEventArgs(EFPDataGridView controlProvider)
      : base(controlProvider, true)
    {
    }

    #endregion

    #region Свойства - исходные данные

    /// <summary>
    /// Индекс столбца в табличном просмотре (независимо от текущего порядка столбцов).
    /// Совпадает с <see cref="DataGridViewBand.Index"/> для столбца.
    /// </summary>
    public int ColumnIndex { get { return _ColumnIndex; } }
    private int _ColumnIndex;

    /// <summary>
    /// Объект столбца в <see cref="EFPDataGridView"/>
    /// </summary>
    public EFPDataGridViewColumn Column { get { return _Column; } }
    private EFPDataGridViewColumn _Column;

    /// <summary>
    /// Объект столбца в <see cref="DataGridView"/>
    /// </summary>
    public DataGridViewColumn GridColumn { get { return Column.GridColumn; } }

    /// <summary>
    /// Имя столбца <see cref="DataGridViewColumn.Name"/>
    /// </summary>
    public string ColumnName { get { return Column.Name; } }

    /// <summary>
    /// Настройки вида ячейки для выравнивания и др. атрибутов.
    /// Используется при вызове в режимах <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.Print"/>.
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
    /// Вызов метода делает строку <see cref="DataGridViewRow"/> Unshared
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
    /// Исходное (неотформатированное) значение.
    /// </summary>
    public object OriginalValue { get { return _OriginalValue; } }
    private object _OriginalValue;

    #endregion

    #region Устанавливаемые свойства

    /// <summary>
    /// Форматируемое значение.
    /// Используется при вызове в режимах <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.Print"/>.
    /// </summary>
    public object Value { get { return _Value; } set { _Value = value; } }
    private object _Value;

    /// <summary>
    /// Свойство должно быть установлено в true, если было применено форматирование значения.
    /// Используется при вызове в режимах <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.Print"/>.
    /// </summary>
    public bool FormattingApplied { get { return _FormattingApplied; } set { _FormattingApplied = value; } }
    private bool _FormattingApplied;

    /// <summary>
    /// Отступ от левого или от правого края (в зависимости от горизонтального 
    /// выравнивания) значения ячейки.
    /// Используется при вызове в режимах <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.Print"/>.
    /// </summary>
    public int IndentLevel { get { return _IndentLevel; } set { _IndentLevel = value; } }
    private int _IndentLevel;

    /// <summary>
    /// Возвращает значение <see cref="Value"/> или <see cref="OriginalValue"/>, в зависимости от наличия значений
    /// и свойства <see cref="FormattingApplied"/>.
    /// Это значение должно использоваться для вывода.
    /// Используется при вызове в режимах <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.View"/> и <see cref="EFPDataViewInfoReason.Print"/>.
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
    /// Всплывающая подсказка, которая будет выведена при наведении курсора на ячейку. 
    /// Перед вызовом события <see cref="EFPDataGridView.CellInfoNeeded"/>
    /// вызывается событие <see cref="DataGridView.CellToolTipTextNeeded"/>, таким образом, поле уже может содержать значение.
    /// Используется при вызове в режиме <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.ToolTip"/>.
    /// Значение подсказки для строки не копируется в это поле. 
    /// Можно использовать значение <see cref="RowToolTipText"/>.
    /// </summary>
    public string ToolTipText
    {
      get { return _ToolTipText; }
      set
      {
        _ToolTipText = value;
        _ToolTipTextHasBeenSet = true;
      }
    }
    private string _ToolTipText;

    internal bool ToolTipTextHasBeenSet { get { return _ToolTipTextHasBeenSet; } }
    private bool _ToolTipTextHasBeenSet;

    /// <summary>
    /// Текст всплывающей подсказки для строки, полученное при вызове события <see cref="EFPDataGridView.RowInfoNeeded"/>.
    /// Значение может быть использовано при формировании свойства <see cref="ToolTipText"/>, чтобы 
    /// не вычислять текст еше раз, когда подсказка для ячеки основывается на подсказке для строки.
    /// Используется при вызове в режиме <see cref="EFPDataGridViewInfoEventArgsBase.Reason"/>=<see cref="EFPDataViewInfoReason.ToolTip"/>.
    /// </summary>
    public string RowToolTipText { get { return _RowToolTipText; } }
    private string _RowToolTipText;


    /// <summary>
    /// Если установить в false, то содержимое ячейки не будет выводиться совсем.
    /// Полезно для CheckBox'ов и кнопок, если для какой-то строки ячеек они не нужны.
    /// Скрытие работает для всех типов столбцов, включая <see cref="DataGridViewTextBoxCell"/>.
    /// </summary>
    public bool ContentVisible { get { return _ContentVisible; } set { _ContentVisible = value; } }
    private bool _ContentVisible;

    #endregion

    #region Защишенные методы

    /// <summary>
    /// Инициализация аргументов.
    /// В первую очередь вызывает <see cref="EFPDataGridViewInfoEventArgsBase.InitRow(int, EFPDataViewInfoReason)"/>, а затем инициализирует параметры для ячейки
    /// </summary>
    /// <param name="rowArgs">Аргументы вызванного ранее события <see cref="EFPDataGridView.RowInfoNeeded"/></param>
    /// <param name="columnIndex">Индекс столбца DataGridViewColumn.Index</param>
    /// <param name="cellStyle">Форматирование ячейки</param>
    /// <param name="styleIsTemplate"></param>
    /// <param name="originalValue"></param>
    public void InitCell(EFPDataGridViewRowInfoEventArgs rowArgs, int columnIndex, DataGridViewCellStyle cellStyle, bool styleIsTemplate, object originalValue)
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

      if (Column.LeftBorder == UIDataViewBorderStyle.Default)
        LeftBorder = rowArgs.LeftBorder;
      else
        LeftBorder = Column.LeftBorder;
      if (Column.RightBorder == UIDataViewBorderStyle.Default)
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
      ReadOnlyMessage = Res.EFPDataView_Msg_ReadOnlyColumn;
      if (Reason == EFPDataViewInfoReason.ReadOnly)
      {
        DataGridViewRow gridRow = ControlProvider.Control.Rows[RowIndex]; // Unshared
        ReadOnly = gridRow.Cells[columnIndex].ReadOnly;
        //  ReadOnly = ControlProvider.Control.Columns[ColumnIndex].ReadOnly;
      }

      ContentVisible = true;
      if (!rowArgs.ControlContentVisible)
      {
        DataGridViewColumn gridCol = Control.Columns[columnIndex];
        if (gridCol is DataGridViewCheckBoxColumn ||
          gridCol is DataGridViewButtonColumn ||
          gridCol is DataGridViewComboBoxColumn)

          ContentVisible = false;
      }

      _RowToolTipText = rowArgs.ToolTipText;

      ErrorMessageList cellErrors;
      if (rowArgs.CellErrorMessages.TryGetValue(columnIndex, out cellErrors))
      {
        // if (CellErrors.Count > 0) - можно не проверять
        switch (cellErrors.Severity)
        {
          case ErrorMessageKind.Error:
            ColorType = UIDataViewColorType.Error;
            break;
          case ErrorMessageKind.Warning:
            if (ColorType != UIDataViewColorType.Error)
              ColorType = UIDataViewColorType.Warning;
            break;
            // Для Info цвет не меняем
        }

        if (rowArgs.Reason == EFPDataViewInfoReason.ToolTip)
        {
          for (int i = 0; i < cellErrors.Count; i++)
          {
            if (!String.IsNullOrEmpty(_RowToolTipText))
              _RowToolTipText += Environment.NewLine;
            _RowToolTipText += cellErrors[i];
          }
        }
      }

      _ToolTipTextHasBeenSet = false;
    }

    #endregion
  }

  #region Делегаты

  /// <summary>
  /// Делегат события <see cref="EFPDataGridView.RowInfoNeeded"/>
  /// </summary>
  /// <param name="sender">Объект <see cref="EFPDataGridView"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPDataGridViewRowInfoEventHandler(object sender,
    EFPDataGridViewRowInfoEventArgs args);

  /// <summary>
  /// Делегат события <see cref="EFPDataGridView.CellInfoNeeded"/>
  /// </summary>
  /// <param name="sender">Объект <see cref="EFPDataGridView"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPDataGridViewCellInfoEventHandler(object sender,
    EFPDataGridViewCellInfoEventArgs args);

  #endregion

  #endregion

  #region Режимы сохранения текущих строк

  /// <summary>
  /// Возможные способы сохранения / восстановления текущей позиции в табличном 
  /// просмотре (свойство <see cref="EFPDataGridView.SelectedRowsMode"/>)
  /// </summary>
  public enum EFPDataGridViewSelectedRowsMode
  {
    /// <summary>
    /// Режим по умолчанию.
    /// Текущая строка не может быть восстановлена.
    /// </summary>
    None,

    /// <summary>
    /// Сохрание номеров строк.
    /// Обычно подходит для табличных просмотров, не связанных с источником данных.
    /// </summary>
    RowIndex,

    /// <summary>
    /// Сохранение ссылок на объекты <see cref="System.Data.DataRow"/>.
    /// Подходит для просмотров, связанных с <see cref="System.Data.DataTable"/> или <see cref="System.Data.DataView"/> при условии, что
    /// обновление таблицы не приводит к замене строк. Зато таблица может не иметь ключевого поля.
    /// </summary>
    DataRow,

    /// <summary>
    /// Самый подходящий режим для просмотров, связанных с <see cref="System.Data.DataTable"/> или <see cref="System.Data.DataView"/>,
    /// имеющих ключевое поле (или несколько полей, составляющих первичный ключ).
    /// </summary>
    PrimaryKey,

    /// <summary>
    /// Сохранение значений полей сортировки, заданных в свойстве <see cref="DataView.Sort"/>
    /// для объекта, связанного с табличным просмотром. Не следует использовать
    /// этот режим, если в просмотре встречаются строки с одинаковыми значениями
    /// полей (сортировка не обеспечивает уникальности).
    /// </summary>
    DataViewSort,
  }

  /// <summary>
  /// Класс для сохранения текущей позиции и выбранных строк/столбцов в табличном просмотре
  /// (свойство <see cref="EFPDataGridView.Selection"/>).
  /// Не содержит открытых полей.
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
    /// true, если свойство <see cref="DataGridView.DataSource"/> было установлено
    /// </summary>
    internal bool DataSourceExists;
  }

  #endregion

  #region Режимы установки отметок строк

  /// <summary>
  /// Аргумент rows метода <see cref="EFPDataGridView.CheckMarkRows(EFPDataGridViewCheckMarkRows, EFPDataGridViewCheckMarkAction)"/>
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
  /// Аргумент action метода <see cref="EFPDataGridView.CheckMarkRows(EFPDataGridViewCheckMarkRows, EFPDataGridViewCheckMarkAction)"/>
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
  /// Причина вызова события <see cref="EFPDataGridView.CellFinished"/>
  /// </summary>
  public enum EFPDataGridViewCellFinishedReason
  {
    /// <summary>
    /// Вызвано событие <see cref="DataGridView.CellValueChanged"/> в процессе редактирования ячейки.
    /// </summary>
    Edit,

    /// <summary>
    /// Вставлено текстовое значение из буфера обмена с помощью <see cref="EFPDataGridView.PerformPasteText(string[,])"/>
    /// </summary>
    Paste,

    /// <summary>
    /// Значение очищено при вырезке текста ячеек в буфер обмена командой Cut
    /// </summary>
    Clear,

    /// <summary>
    /// Установка / снята отметка строки с помощью <see cref="EFPDataGridView.CheckMarkRows(EFPDataGridViewCheckMarkRows, EFPDataGridViewCheckMarkAction)"/>
    /// </summary>
    MarkRow,
  }

  /// <summary>
  /// Аргументы события <see cref="EFPDataGridView.CellFinished"/>
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
    private readonly EFPDataGridView _ControlProvider;

    /// <summary>
    /// Индекс строки
    /// </summary>
    public int RowIndex { get { return _RowIndex; } }
    private readonly int _RowIndex;

    /// <summary>
    /// Индекс строки
    /// </summary>
    public int ColumnIndex { get { return _ColumnIndex; } }
    private readonly int _ColumnIndex;

    /// <summary>
    /// Причина появления события
    /// </summary>
    public EFPDataGridViewCellFinishedReason Reason { get { return _Reason; } }
    private readonly EFPDataGridViewCellFinishedReason _Reason;

    /// <summary>
    /// Ячейка табличного просмотра
    /// </summary>
    public DataGridViewCell Cell
    {
      get { return _ControlProvider.Control[ColumnIndex, RowIndex]; }
    }

    /// <summary>
    /// Строка данных, если табличный просмотр связан с таблицей <see cref="System.Data.DataTable"/>.
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
    /// Имя столбца табличного просмотра <see cref="EFPDataGridViewColumn.Name"/>.
    /// </summary>
    public string ColumnName { get { return _ControlProvider.Columns[ColumnIndex].Name; } }

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="EFPDataGridView.CellFinished"/>
  /// </summary>
  /// <param name="sender">Объект <see cref="EFPDataGridView"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPDataGridViewCellFinishedEventHandler(object sender,
    EFPDataGridViewCellFinishedEventArgs args);

  #endregion

  #region Режимы подбора высоты строк

  /// <summary>
  /// Возможные значения свойства <see cref="EFPDataGridView.AutoSizeRowsMode"/>.
  /// В текущей реализации существует единственный режим Auto.
  /// </summary>
  public enum EFPDataGridViewAutoSizeRowsMode
  {
    /// <summary>
    /// Нет управления высотой строки или оно реализовано средствами <see cref="DataGridView"/>
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
  /// Общие методы и свойства для <see cref="EFPDataGridView"/> и <see cref="EFPDataTreeView"/>
  /// </summary>
  public interface IEFPDataView : IEFPControl
  {
    /// <summary>
    /// Если true, то разрешается выбор для нескольких строк.
    /// См. также <see cref="CanMultiEdit"/>.
    /// </summary>
    bool MultiSelect { get; set; }

    /// <summary>
    /// true, если просмотр предназначен только для просмотра, а не для редактирования данных
    /// </summary>
    bool ReadOnly { get; set; }

    /// <summary>
    /// Управляемое свойство для <see cref="ReadOnly"/>
    /// </summary>
    DepValue<bool> ReadOnlyEx { get; set; }

    /// <summary>
    /// true, если можно редактировать строки (при <see cref="ReadOnly"/>=false).
    /// Значение по умолчанию: true (редактирование разрешено).
    /// Возможность группового редактирования зависит от <see cref="CanMultiEdit"/>.
    /// </summary>
    bool CanEdit { get; set; }

    /// <summary>
    /// Управляемое свойство для <see cref="CanEdit"/>
    /// </summary>
    DepValue<bool> CanEditEx { get; set; }

    /// <summary>
    /// true, если допускается создание новых записей.
    /// Действует, если <see cref="ReadOnly"/>=false
    /// </summary>
    bool CanInsert { get; set; }

    /// <summary>
    /// Управляемое свойство для <see cref="CanInsert"/>
    /// </summary>
    DepValue<bool> CanInsertEx { get; set; }

    /// <summary>
    /// true, если допускается создание новых записей на основании существующей.
    /// Действует, если <see cref="ReadOnly"/>=false, а <see cref="CanInsert"/>=true
    /// </summary>
    bool CanInsertCopy { get; set; }

    /// <summary>
    /// Управляемое свойство для <see cref="CanInsertCopy"/>
    /// </summary>
    DepValue<bool> CanInsertCopyEx { get; set; }

    /// <summary>
    /// true, если допускается удаление записей.
    /// Действует, если <see cref="ReadOnly"/>=false
    /// </summary>
    bool CanDelete { get; set; }

    /// <summary>
    /// Управляемое свойство для <see cref="CanDelete"/>
    /// </summary>
    DepValue<bool> CanDeleteEx { get; set; }

    /// <summary>
    /// true, если допускается групповое редактирование и просмотр записей.
    /// false, если допускается редактирование / просмотр только одной записи за раз.
    /// Действует, если <see cref="MultiSelect"/>=true и <see cref="ReadOnly"/>=false или <see cref="CanView"/>=true.
    /// Не влияет на возможность удаления нескольких записей.
    /// </summary>
    bool CanMultiEdit { get; set; }

    /// <summary>
    /// true, если возможен просмотр записей
    /// </summary>
    bool CanView { get; set; }

    /// <summary>
    /// Управляемое свойство для <see cref="CanView"/>
    /// </summary>
    DepValue<bool> CanViewEx { get; set; }

    /// <summary>
    /// Возвращает источник данных в виде <see cref="DataTable"/>.
    /// Возвращает null, если источник данных не является таблицей или просмотром таблицы
    /// </summary>
    DataTable SourceAsDataTable { get; }

    /// <summary>
    /// Возвращает источник данных в виде <see cref="DataView"/>.
    /// Возвращает null, если источник данных не является таблицей или просмотром таблицы
    /// </summary>
    DataView SourceAsDataView { get; }

    /// <summary>
    /// Возвращает строку <see cref="DataRow"/> таблицы данных, соответствующую текущей строке / узлу
    /// </summary>
    DataRow CurrentDataRow { get; set; }

    /// <summary>
    /// Выбранные строки данных, если поддерживается множественный выбор.
    /// Если допускается выбор только одной строки, то массив содержит значение свойства <see cref="CurrentDataRow"/>
    /// </summary>
    DataRow[] SelectedDataRows { get; set; }

    /// <summary>
    /// Проверяет, что в просмотре выбрана ровно одна строка.
    /// Выдает сообщение для пользователя, если это не так.
    /// Этот метод обычно вызывается в начале выполнения команды локального меню, если команда работает с текущей строкой просмотра.
    /// </summary>
    /// <returns>true, если выбрана одна строка</returns>
    bool CheckSingleRow();

    /// <summary>
    /// Доступна ли для пользователя команда "Обновить просмотр".
    /// Возвращает свойство CommandItems.UseRefresh.
    /// В частности, для просмотра страницы отчета <see cref="EFPReportGridPage"/> возвращает false.
    /// </summary>
    bool UseRefresh { get; }

    /// <summary>
    /// Выполняет обновление просмотра.
    /// </summary>
    void PerformRefresh();

    /// <summary>
    /// Обновить все строки табличного просмотра.
    /// В отличие от <see cref="PerformRefresh()"/>, обновляются только существующие строки, а не выполняется запрос к базе данных.
    /// </summary>
    void InvalidateAllRows();

    /// <summary>
    /// Обновить в просмотре все выбранные строки, например, после редактирования, если имеются вычисляемые столбцы
    /// </summary>
    void InvalidateSelectedRows();

    /// <summary>
    /// Пометить на обновление строку табличного просмотра, связанную с заданной строкой таблицы данных.
    /// Предполагается, что просмотр связан с таблицей данных.
    /// </summary>
    /// <param name="row">Строка связанной таблицы данных</param>
    void InvalidateDataRow(DataRow row);

    /// <summary>
    /// Пометить на обновление строки табличного просмотра, связанные с заданными строками таблицы данных.
    /// Предполагается, что просмотр связан с таблицей данных.
    /// </summary>
    /// <param name="rows">Массив строк связанной таблицы данных</param>
    void InvalidateDataRows(DataRow[] rows);

    /// <summary>
    /// Список столбцов
    /// </summary>
    IEFPDataViewColumns Columns { get; }

    /// <summary>
    /// Возвращает информацию о видимых столбцах
    /// </summary>
    /// <returns></returns>
    IEFPDataViewColumn[] VisibleColumns { get; }

    /// <summary>
    /// Имя текущего столбца. Если выбрано несколько столбцов (например, строка целиком),
    /// то пустая строка ("")
    /// </summary>
    string CurrentColumnName { get; }

    /// <summary>
    /// Возвращает индекс текущего столбца просмотра или (-1), если выбрана вся строка
    /// </summary>
    int CurrentColumnIndex { get; }

    /// <summary>
    /// Интерфейс для определения размеров элементов просмотра.
    /// Не использовать в прикладном коде.
    /// </summary>
    IEFPGridControlMeasures Measures { get; }

    /// <summary>
    /// Текущая конфигурация столбцов просмотра.
    /// </summary>
    EFPDataViewConfig CurrentConfig { get; set; }

    /// <summary>
    /// Генератор столбцов табличного просмотра
    /// </summary>
    IEFPGridProducer GridProducer { get; }

    /// <summary>
    /// Должен заполнить в объекте <see cref="EFPDataViewConfigColumn"/> ширину и другие поля в сответствии с 
    /// реальной шириной в просмотре.
    /// Не использовать в прикладном коде.
    /// </summary>
    /// <param name="configColumn">Заполняемая информация о столбце</param>
    /// <returns>true, если столбец есть в просмотре и данные были заполнены</returns>
    bool InitColumnConfig(EFPDataViewConfigColumn configColumn);

    /// <summary>
    /// Возвращает позицию для будущего выпадающего блока диалога, который будет показан для редактирования ячейки.
    /// В возвращаемом объекте устанавливается свойство <see cref="EFPDialogPosition.PopupOwnerBounds"/>.
    /// Если нет текущей ячейки (просмотр пустой) или текущая ячейка прокручены мышью за пределы видимой области просмотра,
    /// возвращается неинициализированный <see cref="EFPDialogPosition.PopupOwnerBounds"/>.
    /// </summary>
    EFPDialogPosition CurrentPopupPosition { get; }

    #region Ручная сортировка строк с помощью столбца в DataTable

    /// <summary>
    /// Имя числового столбца, который определяет порядок строк при ручной сортировке.
    /// Если задано, то в меню есть команды ручной сортировки.
    /// </summary>
    string ManualOrderColumn { get; set; }

    /// <summary>
    /// Имя числового столбца, который определяет порядок строк по умолчанию при ручной
    /// сортировке. Свойство действует при непустом значении <see cref="ManualOrderColumn"/>.
    /// Если имя присвоено, то действует команда "Восстановить порядок по умолчанию"
    /// </summary>
    string DefaultManualOrderColumn { get; set; }

    /// <summary>
    /// Возвращает объект, реализующий интерфейс <see cref="IDataReorderHelper"/>.
    /// Если свойство <see cref="ManualOrderColumn"/> не установлено, возвращается null.
    /// Для инициализации значения однократно вызывается событие DataReorderHelperNeeded.
    /// Если требуется, чтобы свойство вернуло новое значение, используйте метод <see cref="ResetDataReorderHelper()"/>.
    /// </summary>
    /// <returns>Объект IDataReorderHelper</returns>
    IDataReorderHelper DataReorderHelper { get; }

    /// <summary>
    /// Очищает сохраненное значение свойства <see cref="DataReorderHelper"/>, чтобы при следующем обращении к нему
    /// вновь было вызвано событие DataReorderHelperNeeded.
    /// </summary>
    void ResetDataReorderHelper();

    /// <summary>
    /// Создает новый экземпляр <see cref="IDataReorderHelper"/>.
    /// </summary>
    /// <returns>Объект для упорядочения строк</returns>
    IDataReorderHelper CreateDataReorderHelper();

    /// <summary>
    /// Этот метод может использоваться в обработчике события DataReorderHelperNeeded другого провайдера, который должен использовать реализацию из текущего провайдера
    /// </summary>
    /// <param name="sender">Объект, сгенерировавший событие. Не используется</param>
    /// <param name="args">Аргументы события. В нем устанавливается свойство <see cref="DataReorderHelperNeededEventArgs.Helper"/></param>
    void CreateDataReorderHelper(object sender, DataReorderHelperNeededEventArgs args);

    /// <summary>
    /// Вызывается, когда выполнена ручная сортировка строк (по окончании изменения
    /// значений поля для всех строк)
    /// </summary>
    event EventHandler ManualOrderChanged;

    /// <summary>
    /// Выполняет инициализацию значения поля, заданного свойством <see cref="ManualOrderColumn"/> для новых строк, у которых поле имеет значение 0.
    /// Вызывает метод <see cref="IDataReorderHelper.InitRows(DataRow[], out bool)"/>.
    /// Если свойcтво <see cref="ManualOrderColumn"/> не установлено, никаких действий не выполняется.
    /// </summary>
    /// <param name="rows">Строки данных, которые нужно инициализировать</param>
    /// <param name="otherRowsChanged">Сюда записывается значение true, если были изменены другие строки в просмотре, кроме выбранных.</param>
    /// <returns>True, если строки (одна или несколько) содержали нулевое значение и были инициализированы.
    /// Если все строки уже содержали ненулевое значение, то возвращается false.</returns>
    bool InitManualOrderColumnValue(DataRow[] rows, out bool otherRowsChanged);

    #endregion

    /// <summary>
    /// Получить сводку документа при экспорте.
    /// </summary>
    BRDocumentProperties DocumentProperties { get; }

    /// <summary>
    /// Список объектов для печати/вывода в файл/отправки.
    /// По умолчанию список содержит единственный элемент с кодом "Control"
    /// </summary>
    NamedList<EFPMenuOutItem> MenuOutItems { get; }

    /// <summary>
    /// Параметры печати/экспорта табличного просмотра.
    /// Может возвращать null, если в <see cref="EFPDataGridViewCommandItems.OutHandler"/> был удален вариант "Control"
    /// </summary>
    Reporting.BRDataViewMenuOutItemBase DefaultOutItem { get; }

    /// <summary>
    /// Вызывается для редактирования записей
    /// </summary>
    event EventHandler EditData;

    /// <summary>
    /// Режим работы клавиши "Enter" и двойного щелчка мыши.
    /// По умолчанию - <see cref="EFPDataViewEnterKeyMode.EditOrView"/>.
    /// </summary>
    EFPDataViewEnterKeyMode EnterKeyMode { get; set; }

    /// <summary>
    /// Текущее состояние просмотра.
    /// Если в данный момент не обрабатывается событие <see cref="EditData"/>, возвращается <see cref="UIDataState.View"/>.
    /// </summary>
    UIDataState State { get; }
  }

  #endregion

  /// <summary>
  /// Провайдер табличного просмотра.
  /// Если требуются управляемые пользователем фильтры и/или управление столбцами с помощью <see cref="EFPGridProducer"/>,
  /// используйте расширенный класс <see cref="EFPConfigurableDataGridView"/> (или его производные).
  /// </summary>
  public class EFPDataGridView : EFPControl<DataGridView>, IEFPDataView
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект, привязанный к <see cref="DataGridView"/>
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент Windows Forms</param>
    public EFPDataGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control, true)
    {
      Init();
    }

    /// <summary>
    /// Создает объект, привязанный к <see cref="IEFPControlWithToolBar{DataGridView}"/>
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
      _RowInfoNeededArgs = new EFPDataGridViewRowInfoEventArgs(this);
      _CellInfoNeededArgs = new EFPDataGridViewCellInfoEventArgs(this);
      Control_Leave(null, null);
      // так не помогает. Control.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft; // 27.07.2022. Исправление для Mono.
      _State = UIDataState.View;
      _ReadOnly = false;
      _CanEdit = true;
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
      _DocumentProperties = BRReport.AppDefaultDocumentProperties.Clone();

      _TopLeftCellToolTipText = String.Empty;
      _RowCountTopLeftCellToolTipText = String.Empty;
      _ErrorCountTopLeftCellToolTipText = String.Empty;

      if (!DesignMode)
      {
        Control.CurrentCellChanged += new EventHandler(Control_CurrentCellChanged);
        Control.VisibleChanged += new EventHandler(Control_VisibleChanged);
        Control.DataSourceChanged += new EventHandler(Control_DataBindingChanged);
        Control.DataMemberChanged += new EventHandler(Control_DataBindingChanged);
        Control.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(Control_DataBindingComplete);
        Control.RowsAdded += Control_RowsAdded;
        Control.RowsRemoved += Control_RowsRemoved;
        Control.RowPrePaint += new DataGridViewRowPrePaintEventHandler(Control_RowPrePaint);
        Control.CellPainting += new DataGridViewCellPaintingEventHandler(Control_CellPainting);
        Control.RowPostPaint += new DataGridViewRowPostPaintEventHandler(Control_RowPostPaint);
        Control.CellFormatting += new DataGridViewCellFormattingEventHandler(Control_CellFormatting);
        Control.Enter += new EventHandler(Control_Enter);
        Control.Leave += new EventHandler(Control_Leave);
        Control.KeyDown += new KeyEventHandler(Control_KeyDown);
        Control.KeyPress += new KeyPressEventHandler(Control_KeyPress);
        Control.MouseDown += new MouseEventHandler(Control_MouseDown);
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
      }

      base.UseIdle = true;
    }

    #endregion

    #region Изменения в ProviderState

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
    /// Вызывает <see cref="EFPDataGridViewCommandItems.InitOrderItems()"/>
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();
      CommandItems.InitOrderItems();
      InitColumnHeaderTriangles();

      if (String.IsNullOrEmpty(_DocumentProperties.Title))
        _DocumentProperties.Title = WinFormsTools.GetControlText(Control);

      _ValidateRequiredFlag = true; // требуется обновление верхней левой ячейки
    }

    /// <summary>
    /// Дополнительный вызов <see cref="ResetDataReorderHelper()"/>
    /// </summary>
    protected override void OnDetached()
    {
      ResetDataReorderHelper(); // чтобы не занимал место. Скорее всего, больше не понадобится.
      base.OnDetached();
    }

    /// <summary>
    /// Вызывает DataTableRepeater.Dispose(), если свойство <see cref="TableRepeater"/> было установлено.
    /// </summary>
    protected override void OnDisposed()
    {
      ResetDataReorderHelper();

      if (_TableRepeater != null)
      {
        _TableRepeater.Dispose();
        _TableRepeater = null;
      }

      base.OnDisposed();
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
      // Так не работает, все равно все прыгает.
      // Кроме того, эти методы не являются реентрантыми и могут вызываться только из пользовательского кода.
      // EFPDataGridView.SourceAsDataTable.BeginLoadData();
      // EFPDataGridView.SourceAsDataTable.BeginInit();
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
        throw ExceptionFactory.UnpairedCall(this, "BeginUpdate()", "EndUpdate()");

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
    /// Этот метод вызывается после инициализации команд локального меню.
    /// Добавляет обработчики, которые должны быть в конце цепочки.
    /// </summary>
    protected override void OnAfterPrepareCommandItems()
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

      if (_ValidateRequiredFlag)
        Validate();
    }

    /// <summary>
    /// Вызывается при изменении текущей выбранной ячейки.
    /// Вызывает обновление команд локального меню.
    /// Предотвращается вложенный вызов метода.
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
        if (_CurrentIncSearchColumnIndex != Control.CurrentCellAddress.X)
          CurrentIncSearchColumn = null;
      }

      if (CommandItemsIfAssigned != null)
        CommandItems.RefreshStatItems();
    }

    #endregion

    #region VisibleChanged

    ///// <summary>
    ///// Только после установки этого флага разрешается реагировать на смену ячейки
    ///// </summary>
    //public bool VisibleHasChanged { get { return _VisibleHasChanged; } }
    //private bool _VisibleHasChanged;

    /// <summary>
    /// Этот флажок устанавливается в true, когда нажата мышь или клавиша, что
    /// означает, что просмотр есть на экране, и смена текущей ячейки выполнена
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
              SetFirstDisplayedCell();
          }

          //CommandItems.RefreshStatItems();
        }
        //_VisibleHasChanged = Control.Visible;
        _MouseOrKeyboardFlag = false;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    /// <summary>
    /// Установка свойства выделена в отдельный метод, чтобы подавить надоедливое исключение в отладчике
    /// </summary>
    [DebuggerStepThrough]
    private void SetFirstDisplayedCell()
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

    #endregion

    #region DataBindingChanged

    private void Control_DataBindingChanged(object sender, EventArgs args)
    {
      try
      {
        _SourceAsDataView = WinFormsTools.GetSourceAsDataView(Control.DataSource, Control.DataMember); // 04.07.2021
        OnDataBindingChanged();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    /// <summary>
    /// Вызывается после изменения свойств <see cref="DataGridView.DataSource"/> или <see cref="DataGridView.DataMember"/>.
    /// Если требуется выполнить действия после того, как инициализировано количество строк в просмотре,
    /// используйте <see cref="OnDataBindingComplete(DataGridViewBindingCompleteEventArgs)"/>.
    /// 
    /// Если метод переопределен в классе-наследнике, обязательно должен быть вызван метод базового класса.
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
      ResetDataReorderHelper();
      _ValidateRequiredFlag = true;
    }

    #endregion

    #region DataBindingComplete

    /// <summary>
    /// Событие DataBindingComplete может вызываться вложенно из PerformAutoSort()
    /// </summary>
    private bool _Inside_Control_DataBindingComplete = false;

    void Control_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs args)
    {
      // 06.09.2024 - вызывается всегда
      //if (args.ListChangedType != ListChangedType.Reset)
      //  return;

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
        EFPApp.ShowException(e);
      }
    }

    /// <summary>
    /// Вызывается при получении события <see cref="DataGridView.DataBindingComplete"/>.
    /// Реентрантные события не вызывают метод.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnDataBindingComplete(DataGridViewBindingCompleteEventArgs args)
    {
      CurrentRowIndex = _SavedRowIndex;
      if (args.ListChangedType == ListChangedType.Reset)
      {
        int colIndex = CurrentColumnIndex;
        _CurrentColumnIndex = -1; // 07.07.2025
        CurrentColumnIndex = colIndex;
      }

      _CurrentCellChangedFlag = true;

      // После присоединения источника часто теряются треугольнички
      // в заголовках строк.
      // 17.07.2021. Подтверждено.
      // Если используется произвольная сортировка по двум столбцам, то первый треугольник рисуется, а второй - нет.
      if (args.ListChangedType == ListChangedType.Reset)
        InitColumnHeaderTriangles();

      _ValidateRequiredFlag = true;
    }

    #endregion

    #region RowsAdded/Removed

    private void Control_RowsAdded(object sender, DataGridViewRowsAddedEventArgs args)
    {
      _ValidateRequiredFlag = true;
    }

    private void Control_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs args)
    {
      _ValidateRequiredFlag = true;
    }

    #endregion

    #region KeyDown

    void Control_KeyDown(object sender, KeyEventArgs args)
    {
      try
      {
        _MouseOrKeyboardFlag = true;
        DoControl_KeyDown(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
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
        EFPApp.ShowException(e);
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
            DataGridViewCell gridCell = Control.Rows[ht.RowIndex].Cells[ht.ColumnIndex];
            if (gridCell.Selected)
              return;
            Control.ClearSelection();
            gridCell.Selected = true;
            Control.CurrentCell = gridCell;
            break;
          case DataGridViewHitTestType.RowHeader:
            DataGridViewRow gridRow = Control.Rows[ht.RowIndex];
            foreach (DataGridViewCell selCell in Control.SelectedCells)
            {
              if (selCell.RowIndex == ht.RowIndex)
                return;
            }
            Control.ClearSelection();
            gridRow.Selected = true;
            int ColIdx = Control.CurrentCellAddress.X;
            if (ColIdx < 0)
              ColIdx = Control.FirstDisplayedScrollingColumnIndex;
            if (ColIdx >= 0)
              Control.CurrentCell = gridRow.Cells[ColIdx];
            break;
          case DataGridViewHitTestType.ColumnHeader:
            DataGridViewColumn gridCol = Control.Columns[ht.ColumnIndex];
            foreach (DataGridViewCell selCell in Control.SelectedCells)
            {
              if (selCell.ColumnIndex == ht.ColumnIndex)
                return;
            }
            Control.ClearSelection();
            gridCol.Selected = true;
            int rowIdx = Control.CurrentCellAddress.Y;
            if (rowIdx < 0)
              rowIdx = Control.FirstDisplayedScrollingRowIndex;
            if (rowIdx >= 0)
              Control.CurrentCell = Control.Rows[rowIdx].Cells[ht.ColumnIndex];
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
    /// Свойство возвращает null, если <see cref="TextSearchEnabled"/>=false
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
    /// Создает объект <see cref="EFPDataGridViewSearchContext"/>.
    /// Переопределенный метод может создать расширенный объект для поиска текста.
    /// </summary>
    /// <returns>Объект поиска</returns>
    protected virtual IEFPTextSearchContext CreateTextSearchContext()
    {
      return new EFPDataGridViewSearchContext(this);
    }

    /// <summary>
    /// Если true (по умолчанию), то доступна команда "Найти" (Ctrl-F).
    /// Если false, то свойство <see cref="TextSearchContext"/> возвращает null и поиск недоступен.
    /// Свойство можно устанавливать только до вывода просмотра на экран.
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
    /// На уровне <see cref="EFPDataGridView"/> используется при установке свойства <see cref="CurrentConfig"/> для инициализации
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
    /// <see cref="GridProducer"/>.
    /// Если используется особая реализация просмотра, то можно отключить инициализацию порядка сортировки
    /// строк, установив значение false. Установка допускается только до вывода просмотра на экран.
    /// Как правило, при установке значения в false, следует также устанавливать <see cref="CustomOrderAllowed"/>=false.
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
    /// Используется в событии <see cref="EditData"/>.
    /// Свойство кратковременно устанавливается во время наступления события редактирования.
    /// В остальное время имеет значение <see cref="UIDataState.View"/>.
    /// </summary>
    public UIDataState State { get { return _State; } }
    private UIDataState _State;

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

    IEFPGridControlMeasures IEFPDataView.Measures { get { return Measures; } }

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
        Control.RowTemplate.Height = Measures.SetTextRowHeight(value);
      }
    }

    /// <summary>
    /// Значение свойства <see cref="EFPControlBase.DisplayName"/>, если оно не задано в явном виде
    /// </summary>
    protected override string DefaultDisplayName { get { return Res.EFPDataGridView_Name_Default; } }

    #endregion

    #region Управление поведением просмотра

    #region ReadOnly

    /// <summary>
    /// true, если поддерживается только просмотр данных, а не редактирование.
    /// Установка свойства отключает видимость всех команд редактирования. 
    /// Свойства <see cref="CanEdit"/>, <see cref="CanInsert"/>, <see cref="CanDelete"/> и <see cref="CanInsertCopy"/> перестают действовать.
    /// Значение по умолчанию: false (редактирование разрешено).
    /// Не влияет на возможность inline-редактирования. Возможно любое сочетание
    /// свойств <see cref="ReadOnly"/> и <see cref="DataGridView.ReadOnly"/>.
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
      }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Управляемое свойство для <see cref="ReadOnly"/>.
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

    #region CanEdit

    /// <summary>
    /// true, если можно редактировать строки (при <see cref="ReadOnly"/>=false).
    /// Значение по умолчанию: true (редактирование разрешено).
    /// Возможность группового редактирования зависит от <see cref="CanMultiEdit"/>.
    /// </summary>
    [DefaultValue(true)]
    public bool CanEdit
    {
      get { return _CanEdit; }
      set
      {
        if (value == _CanEdit)
          return;
        _CanEdit = value;
        if (_CanEditEx != null)
          _CanEditEx.Value = value;
      }
    }
    private bool _CanEdit;

    /// <summary>
    /// Управляемое свойство для <see cref="CanEdit"/>
    /// </summary>
    public DepValue<bool> CanEditEx
    {
      get
      {
        InitCanEditEx();
        return _CanEditEx;
      }
      set
      {
        InitCanEditEx();
        _CanEditEx.Source = value;
      }
    }
    private DepInput<bool> _CanEditEx;

    private void InitCanEditEx()
    {
      if (_CanEditEx == null)
      {
        _CanEditEx = new DepInput<bool>(CanInsert, CanEditEx_ValueChanged);
        _CanEditEx.OwnerInfo = new DepOwnerInfo(this, "CanEditEx");
      }
    }

    void CanEditEx_ValueChanged(object Sender, EventArgs Args)
    {
      CanEdit = _CanEditEx.Value;
    }

    #endregion

    #region CanInsert

    /// <summary>
    /// true, если можно добавлять строки (при <see cref="ReadOnly"/>=false).
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
      }
    }
    private bool _CanInsert;

    /// <summary>
    /// Управляемое свойство для <see cref="CanInsert"/>
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
    /// (при <see cref="ReadOnly"/>=false и <see cref="CanInsert"/>=true).
    /// Значение по умолчанию: false (копирование запрещено).
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
      }
    }
    private bool _CanInsertCopy;

    /// <summary>
    /// Управляемое свойство для <see cref="CanInsert"/>.
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
    /// true, если можно удалять строки (при <see cref="ReadOnly"/>=false).
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
      }
    }
    private bool _CanDelete;

    /// <summary>
    /// Управляемое свойство для <see cref="CanDelete"/>.
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

    bool IEFPDataView.MultiSelect { get { return Control.MultiSelect; } set { Control.MultiSelect = value; } }

    /// <summary>
    /// true, если разрешено редактирование и просмотр одновременно 
    /// нескольких выбранных строк.
    /// По умолчанию - false, множественое редактирование запрещено.
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
    /// Блокировка сортировки для всех столбцов.
    /// Для столбцов, определенных в <see cref="Orders"/>, сортировка остается разрешенной.
    /// Этот метод должен вызываться после добавления стобцов с помощью <see cref="EFPDataGridViewColumns"/>.
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
          int сolumnIndex = IndexOfUsedColumn(Orders[i]);
          if (сolumnIndex >= 0)
            Control.Columns[сolumnIndex].SortMode = DataGridViewColumnSortMode.Programmatic;
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
    /// Установить свойство <see cref="DataGridViewColumn.ReadOnly"/> для всех столбцов просмотра.
    /// Метод используется, когда требуется разрешить редактирование "по месту"
    /// только для некоторых столбцов, а большая часть столбцов не редактируется.
    /// Метод должен вызываться после установки общего свойства <see cref="ReadOnly"/> табличного 
    /// просмотра, иначе признак у столбцов может быть изменен неявно.
    /// </summary>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetColumnsReadOnly(bool value)
    {
      for (int i = 0; i < Control.Columns.Count; i++)
        Control.Columns[i].ReadOnly = value;
    }

    /// <summary>
    /// Установить свойство <see cref="DataGridViewColumn.ReadOnly"/> для заданных столбцов просмотра.
    /// Метод используется, когда требуется разрешить редактирование "по месту" 
    /// только для некоторых столбцов, а большая часть столбцов не редактируется.
    /// Метод должен вызываться после установки общего свойства <see cref="ReadOnly"/> табличного 
    /// просмотра, иначе признак у столбцов может быть изменен неявно.
    /// </summary>
    /// <param name="columnNames">Список имен столбцов через запятую. Если столбец с заданным именем не найден, то ошибка не выдается</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetColumnsReadOnly(string columnNames, bool value)
    {
      string[] a = columnNames.Split(',');
      for (int i = 0; i < a.Length; i++)
      {
        EFPDataGridViewColumn сol = Columns[a[i]];
        if (сol != null)
          сol.GridColumn.ReadOnly = value;
      }
    }

    #endregion

    #region Список столбцов

    /// <summary>
    /// Дополнительная информация о столбцах, альтернативные методы добавления столбцов
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

    IEFPDataViewColumns IEFPDataView.Columns { get { return Columns; } }

    /// <summary>
    /// Создает <see cref="EFPDataGridViewColumns"/>.
    /// </summary>
    /// <returns>Новый объект</returns>
    protected virtual EFPDataGridViewColumns CreateColumns()
    {
      return new EFPDataGridViewColumns(this);
    }

    bool IEFPDataView.InitColumnConfig(EFPDataViewConfigColumn configColumn)
    {
      EFPDataGridViewColumn col = Columns[configColumn.ColumnName];
      if (col == null)
        return false;

      DataGridViewColumn gridCol = col.GridColumn;
      configColumn.Width = gridCol.Width;
      configColumn.FillMode = gridCol.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill;
      configColumn.FillWeight = (int)(gridCol.FillWeight);
      return true;
    }

    /// <summary>
    /// Возвращает true, если в списке столбцов имеется хотя бы один с установленным свойством <see cref="EFPDataGridViewColumn.Summable"/>.
    /// </summary>
    internal bool HasSummableColumns
    {
      get
      {
        for (int i = 0; i < Control.Columns.Count; i++)
        {
          if (Columns[i].Summable)
            return true;
        }
        return false;
      }
    }

    #endregion

    #region Доступ к выбранным ячейкам независимо от типа данных

    /// <summary>
    /// Сколько строк выбрано сейчас в просмотре (одна, много или ни одной).
    /// Для точного определения используйте свойство <see cref="SelectedGridRows"/>.
    /// В отличие от обращения к свойству <see cref="SelectedGridRows"/>, это свойство вычисляется всегда быстро.
    /// </summary>
    public UISelectedRowsState SelectedRowsState
    {
      get
      {
        if ((Control.SelectionMode == DataGridViewSelectionMode.FullRowSelect) ||
           Control.SelectionMode == DataGridViewSelectionMode.RowHeaderSelect)
        {
          if (Control.SelectedRows.Count > 0)
          {
            return Control.SelectedRows.Count > 1 ? UISelectedRowsState.MultiRows :
              UISelectedRowsState.SingleRow;
          }
          // Иначе придется определять с помощью SelectedCells
          if (Control.SelectedCells.Count > 0)
          {
            int firstIndex = Control.SelectedCells[0].RowIndex;
            for (int i = 1; i < Control.SelectedCells.Count; i++)
            {
              if (Control.SelectedCells[i].RowIndex != firstIndex)
                return UISelectedRowsState.MultiRows;
            }
            return UISelectedRowsState.SingleRow;
          }
        }
        if (Control.CurrentRow == null)
          return UISelectedRowsState.NoSelection;

        return UISelectedRowsState.SingleRow;
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
        bool firstRowFlag = true;
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
                if (firstRowFlag)
                {
                  CurrentGridRow = value[i];
                  firstRowFlag = false;
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
                  if (firstRowFlag)
                  {
                    CurrentGridRow = value[i];
                    firstRowFlag = false;
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
                  if (firstRowFlag)
                  {
                    CurrentGridRow = value[i];
                    firstRowFlag = false;
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
          List<DataGridViewRow> rows = new List<DataGridViewRow>();
          foreach (DataGridViewCell gridCell in control.SelectedCells)
          {
            DataGridViewRow thisRow = gridCell.OwningRow;
            if (!rows.Contains(thisRow))
              rows.Add(thisRow);
          }
          res = rows.ToArray();
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
    /// Оптимальнее, чем вызов <see cref="SelectedGridRows"/>.Length.
    /// </summary>
    /// <seealso cref="SelectedRowsState"/>
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
              List<DataGridViewRow> rows = new List<DataGridViewRow>();
              foreach (DataGridViewCell gridCell in Control.SelectedCells)
              {
                DataGridViewRow thisRow = gridCell.OwningRow;
                if (!rows.Contains(thisRow))
                  rows.Add(thisRow);
              }
              return rows.Count;
            }
          }
          if (Control.CurrentRow == null)
            return 0;

          return 1;
        }
      }
    }

    /// <summary>
    /// Оригинальное свойство <see cref="DataGridView.CurrentRow"/> не позволяет установить позицию.
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

        int colIdx = CurrentColumnIndex;

        if (colIdx < 0 || colIdx >= Control.Columns.Count /* 21.11.2018 */)
          colIdx = 0;
        Control.CurrentCell = value.Cells[colIdx];
      }
    }

    /// <summary>
    /// Эквивалентно установке свойства <seealso cref="CurrentGridRow"/>, но в просмотре выделяется,
    /// если возможно, строка целиком. Для этого требуется соответствующая установка свойства <seealso cref="DataGridView.SelectionMode"/>.
    /// </summary>
    /// <param name="row">Выделяемая строка</param>
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
    /// Расширение свойства <seealso cref="SelectedGridRows"/>. Вместо объектов <seealso cref="DataGridViewRow"/> 
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
        DataGridViewRow[] rows = new DataGridViewRow[value.Length];
        for (int i = 0; i < value.Length; i++)
        {
          if (value[i] >= 0 && value[i] < Control.Rows.Count)
            rows[i] = Control.Rows[value[i]];
        }
        SelectedGridRows = rows;
      }
    }

    private static int[] GetSelectedRowIndices(DataGridView control)
    {
      DataGridViewRow[] rows = GetSelectedGridRows(control);
      int[] res = new int[rows.Length];
      for (int i = 0; i < rows.Length; i++)
      {
        if (rows[i] == null)
          res[i] = -1;
        else
          res[i] = rows[i].Index;
      }
      return res;
    }

    /// <summary>
    /// Расширение свойства <see cref="CurrentGridRow"/>. Вместо объекта <see cref="DataGridViewRow"/>  
    /// используется индекс строки.
    /// Если нет выбранной строки, возвращается (-1).
    /// </summary>
    public int CurrentRowIndex
    {
      get
      {
        DataGridViewRow row = CurrentGridRow;
        if (row == null)
          return -1;
        else
          return row.Index;
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
    /// Выделяет строку в просмотре с заданным индексом.
    /// Используйте установку свойства <see cref="CurrentRowIndex"/>.
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
    /// возвращается массив всех видимых столбцов.
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
        foreach (DataGridViewCell gridCell in control.SelectedCells)
        {
          DataGridViewColumn col = control.Columns[gridCell.ColumnIndex];
          if (res.IndexOf(col) < 0)
          {
            res.Add(col);
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
      return EmptyArray<DataGridViewColumn>.Empty;
    }

    /// <summary>
    /// Возвращает массив индексов выделенных столбцов просмотра.
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
      DataGridViewColumn[] cols = GetSelectedGridColumns(control);
      int[] indices = new int[cols.Length];
      for (int i = 0; i < cols.Length; i++)
        indices[i] = cols[i].Index;
      return indices;
    }

    /// <summary>
    /// Получить выделенные столбцы таблицы. В режиме выделения
    /// ячейки возвращается массив из одного элемента. В режиме выбора строк
    /// возвращается массив всех видимых столбцов.
    /// </summary>
    public EFPDataGridViewColumn[] SelectedColumns
    {
      get
      {
        DataGridViewColumn[] cols = SelectedGridColumns;
        EFPDataGridViewColumn[] cols2 = new EFPDataGridViewColumn[cols.Length];
        for (int i = 0; i < cols.Length; i++)
          cols2[i] = Columns[cols[i]];
        return cols2;
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
        DataGridViewCell gridCell = Control[columnIndex, rowIndex];
        return Control.SelectedCells.Contains(gridCell);
      }
      if (Control.SelectedRows != null && Control.SelectedRows.Count > 0)
      {
        DataGridViewRow row = Control.Rows[rowIndex];
        return Control.SelectedRows.Contains(row);
      }
      if (Control.SelectedColumns != null && Control.SelectedColumns.Count > 0)
      {
        DataGridViewColumn col = Control.Columns[columnIndex];
        return Control.SelectedColumns.Contains(col);
      }

      return Control.CurrentCellAddress.X == columnIndex && Control.CurrentCellAddress.Y == rowIndex;
    }

    /// <summary>
    /// Вспомогательное свойство только для чтения. Возвращает true, если свойство
    /// <see cref="CurrentGridRow"/> установлено (в просмотре есть текущая строка) и она является
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
    /// Выбор прямоугольной области ячеек.
    /// Свойство возвращает/устанавливает выбранные строки и столбцы в виде 
    /// координат прямоугольника
    /// </summary>
    public Rectangle SelectedRectAddress
    {
      get
      {
        if (Control.AreAllCellsSelected(false))
          return new Rectangle(0, 0, Control.ColumnCount, Control.RowCount);
        int[] rows = SelectedRowIndices;
        int[] cols = SelectedColumnIndices;
        if (rows.Length == 0 || cols.Length == 0)
          return Rectangle.Empty;

        int x1 = cols[0];
        int x2 = cols[cols.Length - 1];
        int y1 = rows[0];
        int y2 = rows[rows.Length - 1];
        return new Rectangle(x1, y1, x2 - x1 + 1, y2 - y1 + 1);
      }
      set
      {
        throw new NotImplementedException();
      }
    }


    /// <summary>
    /// Вспомогательный метод.
    /// Возвращает true, если в просмотре есть только одна выбранная (текущая) строка
    /// Если в просмотре нет текущей строки или выбрано больше одной строки, то
    /// выдается соответствующее сообщение с помошью <see cref="EFPApp.ShowTempMessage(string)"/>.
    /// Возвращаемое значение соответствует свойству <see cref="IsCurrentRowSingleSelected"/>.
    /// Используйте метод для упрощения реализации методов редактирования или команд
    /// локального меню, если они должны работать с единственной строкой. После
    /// вызова метода используйте одно из свойств CurrentXxxRow для доступа к строке
    /// </summary>
    /// <returns>true, если в просмотре выбрана одна строка</returns>
    public bool CheckSingleRow()
    {
      if (CurrentGridRow == null)
      {
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoSelectedRow);
        return false;
      }
      if (!IsCurrentRowSingleSelected)
      {
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_MultiSelectedRows);
        return false;
      }
      return true;
    }

    /// <summary>
    /// Перечислитель по выбранным ячейкам таблицы для всех режимов.
    /// Элементом перечисления является структура <see cref="Point"/>, содержащая индекс строки (Y) и столбца (X)
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

      private readonly DataGridView _Control;

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
        int[] colIdxs = EFPDataGridView.GetSelectedColumnIndices(_Control); // все видимые столбцы

        for (int iRow = 0; iRow < _Control.RowCount; iRow++)
        {
          if (!_Control.Rows.SharedRow(iRow).Visible)
            continue;

          for (int j = 0; j < colIdxs.Length; j++)
            yield return new Point(colIdxs[j], iRow);
        }
      }

      #endregion

      #region Выбраны отдельные ячейки, строки или столбцы

      private IEnumerator<Point> GetSelectedEnumerator()
      {
        if (_Control.SelectedRows.Count > 0)
        {
          int[] colIdxs = EFPDataGridView.GetSelectedColumnIndices(_Control); // все видимые столбцы

          for (int i = 0; i < _Control.SelectedRows.Count; i++)
          {
            for (int j = 0; j < colIdxs.Length; j++)
              yield return new Point(colIdxs[j], _Control.SelectedRows[i].Index);
          }
        }

        if (_Control.SelectedColumns.Count > 0)
        {
          int[] rowIdxs = EFPDataGridView.GetSelectedRowIndices(_Control); // все видимые строки
          for (int i = 0; i < rowIdxs.Length; i++)
          {
            for (int j = 0; j < _Control.SelectedColumns.Count; j++)
              yield return new Point(_Control.SelectedColumns[j].Index, rowIdxs[i]);

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
    /// Перечислитель по адресам выбранных ячеек (для оператора foreach).
    /// Элементом перечисления является структура <see cref="Point"/>, содержащая индекс строки (Y) и столбца (X)
    /// очередной ячейки.
    /// Скрытые строки и столбцы не учитываются.
    /// Правильно обрабатываются и сложно выбранные ячейки.
    /// </summary>
    public SelectedCellAddressEnumerable SelectedCellAddresses { get { return new SelectedCellAddressEnumerable(Control); } }

    #endregion

    #region Доступ к выбранным ячейкам для источника данных DataView или DataTable

    /// <summary>
    /// Получение <see cref="DataGridView.DataSource"/> в виде <see cref="DataTable"/> или null, если присоединен другой источник
    /// (не <see cref="DataTable"/> или <see cref="DataView"/>).
    /// Наличие таблицы-повторителя не учитывается, возвращается таблица, присоединенная к просмотру. 
    /// Если есть повторитель, то возвращается <see cref="DataTableRepeater.SlaveTable"/>.
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
    /// Получение <see cref="DataGridView.DataSource"/> в виде <see cref="DataView"/>.
    /// Для источника <see cref="DataTable"/>возвращает <see cref="DataTable.DefaultView"/>.
    /// Возвращает null, если присоединен другой источник.
    /// 
    /// Наличие таблицы-повторителя не учитывается, возвращается таблица, присоединенная к просмотру. 
    /// Если есть повторитель, то возвращается <see cref="DataTableRepeater.SlaveTable"/>.DefaultView.
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

    /// <summary>
    /// Получение строки <see cref="DataRow"/> по номеру строки в табличном просмотре. 
    /// Не делает строку Unshared, т.к. не обращается к объекту <see cref="DataGridViewRow"/>.
    /// Если используется таблица-повторитель, то возвращается строка таблицы-повторителя.
    /// </summary>
    /// <param name="rowIndex">Номер строки</param>
    /// <returns>Объект <see cref="DataRow"/> или null при любой ошибке</returns>
    public DataRow GetDataRow(int rowIndex)
    {
      return WinFormsTools.GetDataRow(Control, rowIndex);

      // 29.09.2017
      // Через SharedRow не работает
      // Если строка Shared, для нее DataBoundItem равно null
      //if (RowIndex < 0 || RowIndex >= Control.RowCount)
      //  return null;
      //else
      //  return GetDataRow(Control.Rows.SharedRow(RowIndex));
    }


    /// <summary>
    /// Получить массив строк <see cref="DataRow "/> по индексам строк.
    /// Если какой-либо индекс не относится к <see cref="DataRow"/>, элемент массива содержит null.
    /// Если используется таблица-повторитель, то возвращаются строки таблицы-повторителя.
    /// </summary>
    /// <param name="rowIndices">Номера строк в просмотре <see cref="DataGridView.Rows"/></param>
    /// <returns>Массив объектов <see cref="DataRow"/></returns>
    public DataRow[] GetDataRows(int[] rowIndices)
    {
      DataView dv = null;
      bool dvDefined = false;
      DataRow[] rows = new DataRow[rowIndices.Length];
      for (int i = 0; i < rows.Length; i++)
      {
        if (rowIndices[i] >= 0 && rowIndices[i] < Control.RowCount)
        {
          // Если строка уже является unshared, то есть быстрый доступ добраться до DataRow.
          // Обычно, этот вариант и будет работать, так как, скорее всего, опрашиваются выбранные строки
          DataGridViewRow gridRow = Control.Rows.SharedRow(rowIndices[i]);
          if (gridRow.Index >= 0)
            rows[i] = WinFormsTools.GetDataRow(gridRow);
          else
          {
            if (!dvDefined)
            {
              dvDefined = true;
              dv = SourceAsDataView;
            }
            if (rowIndices[i] < dv.Count)
              rows[i] = dv[rowIndices[i]].Row;
          }
        }
      }

      return rows;
    }

    /// <summary>
    /// Получить или установить выбранные строки в просмотре.
    /// Расширяет реализацию свойства <see cref="SelectedGridRows"/>, используя вместо объектов
    /// <see cref="DataGridViewRow"/> строки таблицы данных <see cref="DataRow"/>.
    /// Если просмотр не присоединен к таблице данных, свойство возвращает массив, содержащий значения null.
    /// 
    /// Если используется таблица-повторитель, то возвращаются и должны устанавливаться строки таблицы, присоединенной к просмотру,
    /// то есть <see cref="DataTableRepeater.SlaveTable"/>.
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
            res[i] = WinFormsTools.GetDataRow(gridRows[i]);
        }
        return res;
      }
      set
      {
        if (value.Length == 0)
          return;

        // Тут несколько гадко
        // Поэтому сначала проверяем, есть ли какие-нибудь изменения
        DataRow[] orgRows = SelectedDataRows;
        if (orgRows.Length == value.Length)
        {
          bool isDiff = false;
          for (int i = 0; i < value.Length; i++)
          {
            if (value[i] != orgRows[i])
            {
              isDiff = true;
              break;
            }
          }
          if (!isDiff)
            return; // строки не изменились
        }

        DataView dv = SourceAsDataView;
        if (dv == null)
          throw new InvalidDataSourceException(Res.EFPDataView_Err_NoDataView);

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

        DataGridViewRow[] gridRows = new DataGridViewRow[value.Length];
        // Производительность будет ужасной, если выбрано много строк в большой таблице
        int selCnt = 0; // Вдруг найдем все строки до того, как переберем весь набор данных ?
        for (int j = 0; j < dv.Count; j++)
        {
          DataRow thisRow = GetDataRow(j);
          for (int k = 0; k < value.Length; k++)
          {
            if (value[k] == thisRow)
            {
              gridRows[k] = Control.Rows[j];
              selCnt++;
              break;
            }
          }
          if (selCnt == value.Length)
            break; // уже все нашли
        }
        SelectedGridRows = gridRows;
      }
    }

    /// <summary>
    /// Расширение свойства <see cref="CurrentGridRow"/> (чтение и установка текущей строки).
    /// В качестве значения используется строка таблицы данных (объект <see cref="DataRow"/>)
    /// Просмотр должен быть присоединен к источнику данных типа <see cref="DataTable"/> или <see cref="DataView"/>.
    /// 
    /// Если используется таблица-повторитель, то возвращается и должна устанавливаться строка таблицы, присоединенной к просмотру,
    /// то есть <see cref="DataTableRepeater.SlaveTable"/>.
    /// </summary>
    public DataRow CurrentDataRow
    {
      get
      {
        return WinFormsTools.GetDataRow(Control.CurrentRow); // 24.05.2021
      }
      set
      {
        if (value == null)
          return;
        DataView dv = SourceAsDataView;
        if (dv == null)
        {
          Exception e;
          if (Control.DataSource == null)
            e = ExceptionFactory.ObjectProperty(Control, "DataSource", Control.DataSource, null);
          else
            e = new InvalidDataSourceException(Res.EFPDataView_Err_NoDataView);
          AddExceptionInfo(e);
          throw e;
        }

        // 04.07.2021
        // Оптимизация
        if (Object.ReferenceEquals(value, WinFormsTools.GetDataRow(Control.CurrentRow)))
          return;

        int p = DataTools.FindDataRowViewIndex(dv, value);
        if (p >= 0)
          CurrentRowIndex = p;
      }
    }

    /// <summary>
    /// Расширение свойства <see cref="SelectedGridRows"/> / <see cref="SelectedDataRows"/>.
    /// В качестве текущих позиций запоминаются значения ключевых полей в <see cref="DataTable"/>.
    /// Первый индекс двумерного массива соответствует количеству строк.
    /// Второй индекс соответствует количеству полей в <see cref="DataTable.PrimaryKey"/> (обычно равно 1).
    /// Свойство возвращает null, если таблица не присоединена к просмотру (напрямую или через <see cref="DataView"/>).
    /// или не имеет первичного ключа.
    /// </summary>
    public object[,] SelectedDataRowKeys
    {
      get
      {
        DataRow[] rows = SelectedDataRows;
        DataTable table = SourceAsDataTable;
        if (table == null)
          return null;
        if (table.PrimaryKey == null || table.PrimaryKey.Length == 0)
          return null;
        return DataTools.GetPrimaryKeyValues(table, rows);
      }
      set
      {
        if (value == null)
          return;
        DataTable table = SourceAsDataTable;
        if (table == null)
          throw new InvalidOperationException(Res.EFPDataView_Err_NoDataView);
        DataRow[] rows = DataTools.GetPrimaryKeyRows(table, value);
        SelectedDataRows = rows;
      }
    }

    /// <summary>
    /// Расширение свойств <see cref="CurrentGridRow"/> / <see cref="CurrentDataRow"/> для получения / установки 
    /// текущей строки с помощью значений ключевых полей в <see cref="DataTable"/>.
    /// Индекс массива соответствует количеству полей в <see cref="DataTable.PrimaryKey"/> (обычно равно 1).
    /// Свойство возвращает null, если таблица не присоединена к просмотру (напрямую или через <see cref="DataView"/>).
    /// или не имеет первичного ключа.
    /// </summary>
    public object[] CurrentDataRowKeys
    {
      get
      {
        DataRow row = CurrentDataRow;
        if (row == null)
          return null;
        return DataTools.GetPrimaryKeyValues(row);
      }
      set
      {
        DataTable table = SourceAsDataTable;
        if (table == null)
          throw new InvalidOperationException(Res.EFPDataView_Err_NoDataView);
        if (value == null)
          return;
        DataRow row = table.Rows.Find(value);
        if (row == null)
          return;
        CurrentDataRow = row;
      }
    }

    /// <summary>
    /// Расширение свойства <see cref="SelectedGridRows"/> / <see cref="SelectedDataRows"/>.
    /// В качестве текущих позиций запоминаются значения полей, заданных в <see cref="DataView.Sort"/>.
    /// Первый индекс двумерного массива соответствует количеству строк.
    /// Второй индекс соответствует количеству полей сортировки в <see cref="DataView.Sort"/>.
    /// (обычно равно 1).
    /// Свойство возвращает null, если просмотр не соединен с <see cref="DataView"/>
    /// или <see cref="DataView.Sort"/> не имеет полей сортировки.
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
        DataRow[] rows = SelectedDataRows;
        object[,] values = new object[rows.Length, flds.Length];
        for (int i = 0; i < rows.Length; i++)
        {
          for (int j = 0; j < flds.Length; j++)
            values[i, j] = rows[i][flds[j]];
        }
        return values;
      }
      set
      {
        if (value == null)
          return;
        DataView dv = SourceAsDataView;
        if (dv == null)
          throw new InvalidOperationException(Res.EFPDataView_Err_NoDataView);
        if (String.IsNullOrEmpty(dv.Sort))
          throw new InvalidOperationException(Res.EFPDataView_Err_DataViewNotSorted);

        int nRows = value.GetLength(0);
        if (nRows == 0)
          return;
        object[] keys = new object[value.GetLength(1)];
        List<int> rowIndices = new List<int>();
        for (int i = 0; i < nRows; i++)
        {
          for (int j = 0; j < keys.Length; j++)
            keys[j] = value[i, j];
          int idx = dv.Find(keys);
          if (idx > 0)
            rowIndices.Add(idx);
        }

        SelectedRowIndices = rowIndices.ToArray();
      }
    }

    /// <summary>
    /// Расширение свойств <see cref="CurrentGridRow"/> / <see cref="CurrentDataRow"/> для получения / установки 
    /// текущей строки с помощью значений полей сортировки в <see cref="DataView.Sort"/>.
    /// Свойство возвращает null, если просмотр не соединен с <see cref="DataView"/>
    /// или <see cref="DataView.Sort"/> не имеет полей сортировки.
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
        DataRow row = CurrentDataRow;
        object[] values = new object[flds.Length];
        if (row != null)
        {
          for (int j = 0; j < flds.Length; j++)
            values[j] = row[flds[j]];
        }
        return values;
      }
      set
      {
        if (value == null)
          return;
        DataView dv = SourceAsDataView;
        if (dv == null)
          throw new InvalidOperationException(Res.EFPDataView_Err_NoDataView);
        if (String.IsNullOrEmpty(dv.Sort))
          throw new InvalidOperationException(Res.EFPDataView_Err_DataViewNotSorted);

        int idx = dv.Find(value);
        if (idx > 0)
          CurrentRowIndex = idx;
      }
    }

    #endregion

    #region Повторитель таблицы DataTableRepeater

    /// <summary>
    /// Повторитель таблицы.
    /// При установке свойства устанавливается <see cref="DataGridView.DataSource"/>=<see cref="DataTableRepeater.SlaveTable"/>.
    /// Предполагается, что повторитель относится персонально к этому табличному просмотру.
    /// Когда управляющий элемент разрушается, вызывается <see cref="DataTableRepeater"/>.Dispose().
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
        OnTableRepeaterChanged();
      }
    }
    private DataTableRepeater _TableRepeater;

    /// <summary>
    /// Вызывается после установки свойства <see cref="TableRepeater"/>.
    /// </summary>
    protected virtual void OnTableRepeaterChanged()
    {
    }

    /// <summary>
    /// Возвращает строку в таблице-повторителе <see cref="DataTableRepeater.SlaveTable"/>, которая соответствует строке в исходной таблице <see cref="DataTableRepeater.MasterTable"/>.
    /// Если повторитель <see cref="TableRepeater"/> не установлен, возврашает исходную строку без изменений.
    /// </summary>
    /// <param name="masterRow">Строка таблицы <see cref="DataTableRepeater.MasterTable"/></param>
    /// <returns>Строка таблицы <see cref="DataTableRepeater.SlaveTable"/></returns>
    public DataRow GetSlaveRow(DataRow masterRow)
    {
      if (_TableRepeater == null)
        return masterRow;
      else
        return _TableRepeater.GetSlaveRow(masterRow);
    }

    /// <summary>
    /// Возвращает строки в таблице-повторителе <see cref="DataTableRepeater.SlaveTable"/>, которые соответствуют строкам в исходной таблице <see cref="DataTableRepeater.MasterTable"/>.
    /// Если повторитель <see cref="TableRepeater"/> не установлен, возврашает исходные данные без изменений.
    /// </summary>
    /// <param name="masterRows">Массив строк таблицы <see cref="DataTableRepeater.MasterTable"/></param>
    /// <returns>Строки таблицы <see cref="DataTableRepeater.SlaveTable"/></returns>
    public DataRow[] GetSlaveRows(DataRow[] masterRows)
    {
      if (_TableRepeater == null)
        return masterRows;
      else
        return _TableRepeater.GetSlaveRows(masterRows);
    }

    /// <summary>
    /// Возвращает строку в основной таблице <see cref="DataTableRepeater.MasterTable"/>, которая соответствует строке в таблице-повторителе <see cref="DataTableRepeater.SlaveTable"/>.
    /// Используется для реализации редактирования, т.к. изменения должны вноситься в ведущую
    /// таблицу, а не ту, которая отображается в просмотре.
    /// Если повторитель <see cref="TableRepeater"/> не установлен, возврашает исходные данные без изменений.
    /// </summary>
    /// <param name="slaveRow">Строка таблицы <see cref="DataTableRepeater.SlaveTable"/></param>
    /// <returns>Строка таблицы <see cref="DataTableRepeater.MasterTable"/></returns>
    public DataRow GetMasterRow(DataRow slaveRow)
    {
      if (_TableRepeater == null)
        return slaveRow;
      else
        return _TableRepeater.GetMasterRow(slaveRow);
    }

    /// <summary>
    /// Возвращает строки в основной таблице <see cref="DataTableRepeater.MasterTable"/>, которые соответствуют строкам в таблице-повторителе <see cref="DataTableRepeater.SlaveTable"/>.
    /// Используется для реализации редактирования, т.к. изменения должны вноситься в ведущую
    /// таблицу, а не ту, которая отображается в просмотре.
    /// Если повторитель <see cref="TableRepeater"/> не установлен, возврашает исходные данные без изменений.
    /// </summary>
    /// <param name="slaveRows">Строки таблицы <see cref="DataTableRepeater.SlaveTable"/></param>
    /// <returns>Строки таблицы <see cref="DataTableRepeater.MasterTable"/></returns>
    public DataRow[] GetMasterRows(DataRow[] slaveRows)
    {
      if (_TableRepeater == null)
        return slaveRows;
      else
        return _TableRepeater.GetMasterRows(slaveRows);
    }

    /// <summary>
    /// Если есть таблица-повторитель, то возвращается мастер-таблица <see cref="DataTableRepeater.MasterTable"/>.
    /// Иначе возвращается <see cref="SourceAsDataTable"/>.
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
    /// Если есть таблица-повторитель, то возвращается <see cref="DataView"/> для мастер-таблицы <see cref="DataTableRepeater.MasterTable"/>.DefaultView.
    /// Иначе возвращается <see cref="SourceAsDataView"/>.
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
    /// Возвращает массив объектов <see cref="DataRow"/>, связанных со строками, видимыми в просмотре.
    /// </summary>
    public DataRow[] DisplayedDataRows
    {
      get
      {
        int firstRow = Control.FirstDisplayedScrollingRowIndex;
        int n = Control.DisplayedRowCount(true);
        List<DataRow> rows = new List<DataRow>(n);
        for (int i = 0; i < n; i++)
        {
          DataRow row = GetDataRow(firstRow + i);
          if (row != null)
            rows.Add(row);
        }
        return rows.ToArray();
      }
    }

    #endregion

    #region Сохранение / восстановление выбранных строк

    /// <summary>
    /// Режим сохранения выбранных строк свойствами <see cref="SelectedRowsObject"/> и
    /// <see cref="CurrentRowObject"/>.
    /// По умолчанию для <see cref="EFPDataGridView"/> - значение <see cref="EFPDataGridViewSelectedRowsMode.None"/>.
    /// Может быть переопределено в производных классах.
    /// </summary>
    public EFPDataGridViewSelectedRowsMode SelectedRowsMode
    {
      get { return _SelectedRowsMode; }
      set { _SelectedRowsMode = value; }
    }
    private EFPDataGridViewSelectedRowsMode _SelectedRowsMode;


    /// <summary>
    /// Сохранение и восстановление выбранных строк просмотра в виде одного объекта,
    /// в зависимости от свойства <see cref="SelectedRowsMode"/>
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
    /// Тип свойства зависит от режима <see cref="SelectedRowsMode"/>.
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
            int rowIndex = (int)value;
            if (rowIndex >= 0 && rowIndex < Control.Rows.Count)
              CurrentGridRow = Control.Rows[rowIndex];
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
    /// Режим сохранения строк определеятся свойство <see cref="SelectedRowsMode"/>.
    /// Значение включает в себя: признак <see cref="DataGridView.AreAllCellsSelected(bool)"/>, список выделенных
    /// строк <see cref="SelectedRowsObject"/>, текущую строку <see cref="CurrentRowObject"/> и индекс
    /// столбца с текущей ячейкой <see cref="CurrentColumnIndex"/>.
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
          bool dataSouceExists = (Control.DataSource != null);
          if (dataSouceExists == value.DataSourceExists)
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
    /// то возвращается (-1).
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
        if (value == _CurrentColumnIndex)
          return; // 29.04.2025
        if (!Control.Columns[value].Visible)
          return;
        int rowIndex = Control.CurrentCellAddress.Y;
        if (Control.Visible && rowIndex >= 0 && rowIndex < Control.Rows.Count)
        {
          try
          {
            Control.CurrentCell = Control.Rows[rowIndex].Cells[value];
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
    /// то пустая строка ("").
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
        EFPDataGridViewColumn column = Columns[value];
        if (column != null)
          CurrentColumnIndex = column.GridColumn.Index;
      }
    }

    #endregion

    #region Столбец по умолчанию

    /// <summary>
    /// Возвращает индекс столбца, который следует активировать по умолчанию.
    /// При показе просмотра, если свойство <see cref="CurrentColumnIndex"/> не установлено в явном виде, 
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
        int firstIdx = -1;
        for (int i = 0; i < a.Length; i++)
        {
          if (a[i] is DataGridViewImageColumn)
            continue;
          if (firstIdx < 0)
            firstIdx = a[i].Index;

          EFPDataGridViewColumn col = Columns[a[i]]; // предпочтительный вариант индексированного свойства
          if (col.CanIncSearch)
            return a[i].Index;
        }
        if (firstIdx >= 0)
          return firstIdx;

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
    /// как объекты <see cref="EFPDataGridViewColumn"/>
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

    IEFPDataViewColumn[] IEFPDataView.VisibleColumns { get { return VisibleColumns; } }

    /// <summary>
    /// Получить массив видимых столбцов в просмотре в порядке вывода на экран
    /// как объекты <see cref="DataGridViewColumn"/>
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
    /// как объекты <see cref="DataGridViewRow"/>.
    /// Строки становятся Unshared.
    /// </summary>
    /// <param name="states">Критерий поиска</param>
    /// <returns>Массив строк</returns>
    public DataGridViewRow[] GetGridRows(DataGridViewElementStates states)
    {
      List<DataGridViewRow> rows = new List<DataGridViewRow>();
      int rowIndex = Control.Rows.GetFirstRow(states);
      while (rowIndex >= 0)
      {
        rows.Add(Control.Rows[rowIndex]);
        rowIndex = Control.Rows.GetNextRow(rowIndex, states, DataGridViewElementStates.None);
      }
      return rows.ToArray();
    }

    /// <summary>
    /// Получить массив видимых строк в просмотре в порядке вывода на экран
    /// как объекты <see cref="DataGridViewRow"/>.
    /// Строки становятся Unshared, поэтому обычно не следует обращаться к этому свойству.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public DataGridViewRow[] VisibleGridRows
    {
      get
      {
        return GetGridRows(DataGridViewElementStates.Visible);
      }
    }

    /// <summary>
    /// Возвращает высоту в пикселах, которую бы хотел иметь табличный просмотр
    /// для размещения всех строк.
    /// Возвращаемое значение не превышает высоту экрана.
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
    /// Выполняет только вызов <see cref="DataGridView.InvalidateRow(int)"/>.
    /// Для обновления строк с загрузкой данных используйте <see cref="UpdateRows(int[])"/>.
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

    /// <summary>
    /// Обновить все строки табличного просмотра.
    /// </summary>
    public void InvalidateAllRows()
    {
      InvalidateRows(null);
    }

    /// <summary>
    /// Обновить в просмотре все выбранные строки <see cref="SelectedGridRows"/>, например, после
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

      int firstRow = Control.FirstDisplayedScrollingRowIndex;
      int n = Control.DisplayedRowCount(true);
      Dictionary<DataRow, int> rowIdxs = new Dictionary<DataRow, int>(n);
      for (int i = 0; i < n; i++)
      {
        DataRow r = GetDataRow(firstRow + i);
        if (r != null)
          rowIdxs.Add(r, firstRow + i);
      }

      for (int i = 0; i < rows.Length; i++)
      {
        int rowIdx;
        if (rowIdxs.TryGetValue(rows[i], out rowIdx))
          InvalidateRows(new int[1] { rowIdx });
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

      int firstRow = Control.FirstDisplayedScrollingRowIndex;
      int n = Control.DisplayedRowCount(true);
      for (int i = 0; i < n; i++)
      {
        if (GetDataRow(firstRow + i) == row)
        {
          InvalidateRows(new int[1] { firstRow + i });
          break;
        }
      }
    }

    /// <summary>
    /// Вызывает <see cref="DataGridView.InvalidateColumn(int)"/> для столбца с заданным именем.
    /// Если столбец не найден, никаких действий не выполняется.
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public void InvalidateColumn(string columnName)
    {
      int columnIndex = Columns.IndexOf(columnName);
      if (columnIndex >= 0)
        Control.InvalidateColumn(columnIndex);
    }

    /// <summary>
    /// Вызывает <see cref="DataGridView.InvalidateCell(int, int)"/> для ячейки в заданной строке и заданным именем столбца.
    /// Если столбец не найден, никаких действий не выполняется.
    /// 
    /// Аргументы имеют обратный порядок относительно <see cref="DataGridView.InvalidateCell(int, int)"/>.
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="columnName">Имя столбца</param>
    public void InvalidateCell(int rowIndex, string columnName)
    {
      int columnIndex = Columns.IndexOf(columnName);
      if (columnIndex >= 0 && rowIndex >= 0)
        Control.InvalidateCell(columnIndex, rowIndex);
    }

    #endregion

    #region UpdateRows()

    /// <summary>
    /// Обновить строки табличного просмотра, загрузив исходные данные.
    /// Производный класс должен обновить данные в источнике данных просмотра.
    /// Непереопределенная реализация в <see cref="EFPDataGridView"/> просто вызывает <see cref="InvalidateRows(int[])"/>.
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
    /// Обновить в просмотре все выбранные строки <see cref="SelectedGridRows"/>, например, после
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
        throw new InvalidOperationException(Res.EFPDataView_Err_NoDataView);

      Dictionary<DataRow, int> allRowIndices = new Dictionary<DataRow, int>(dv.Count);
      for (int i = 0; i < dv.Count; i++)
        allRowIndices.Add(dv[i].Row, i);

      List<int> usedIndices = new List<int>(rows.Length); // некоторых строк может не быть
      for (int i = 0; i < rows.Length; i++)
      {
        int idx;
        if (allRowIndices.TryGetValue(rows[i], out idx))
          usedIndices.Add(idx);
      }

      UpdateRows(usedIndices.ToArray());
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
        throw new InvalidOperationException(Res.EFPDataView_Err_NoDataView);

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
    /// Создает объект <see cref="EFPDataViewOrders"/>.
    /// </summary>
    /// <returns>Новый объект</returns>
    protected virtual EFPDataViewOrders CreateOrders()
    {
      return new EFPDataViewOrders();
    }

    /// <summary>
    /// Количество элементов в списке <see cref="Orders"/>
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
    /// Текущий порядок предопределенной сортировки (индекс в списке <see cref="Orders"/>).
    /// Если используется произвольная сортировка сортировка, свойство возвращает (-1).
    /// Для инициализации произвольной сортировки устанавливайте свойство <see cref="CustomOrderActive"/>.
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
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, OrderCount - 1);


        _CurrentOrder = Orders[value];
        _CustomOrderActive = false;
        _CustomOrderAutoActivated = false;
        InternalSetCurrentOrder();
      }
    }

    private void InternalSetCurrentOrder()
    {
      if (ProviderState == EFPControlProviderState.Attached)
      {
        EFPDataGridViewSelection oldSel = Selection;
        // 24.07.2023. Не нужно, т.к. дальше есть вызов InitOrderItems() 
        // CommandItems.InitCurentOrder();
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
              //if (OrderChanged == null)
              //  throw new InvalidOperationException("OrderChangesToRefresh=false, AutoSort=false и событие OrderChanged не имеет обработчика");

              // 22.03.2022. Отсутствие обработчика больше не считается ошибкой
              OnOrderChanged(EventArgs.Empty);
            }
          }
        }
        Selection = oldSel;
        if (ProviderState == EFPControlProviderState.Attached)
          CommandItems.InitOrderItems();
      }
      else
      {
        // 24.08.2023
        // Если элемент невидим, то сортировку все равно выполнять надо
        if (AutoSort)
        {
          if (Control.DataSource != null)
            PerformAutoSort();
        }
        else
        {
          //if (OrderChanged == null)
          //  throw new InvalidOperationException("OrderChangesToRefresh=false, AutoSort=false и событие OrderChanged не имеет обработчика");

          // 22.03.2022. Отсутствие обработчика больше не считается ошибкой
          OnOrderChanged(EventArgs.Empty);
        }
      }
      OnCurrentOrderChanged(EventArgs.Empty);
      InitColumnHeaderTriangles();
    }

    #endregion

    #region Произвольная сортировка

    /// <summary>
    /// Разрешение использовать произвольную сортировку.
    /// По умолчанию - false - запрещено (если не переопределено в конструкторе производного класса).
    /// Свойство можно устанавливать только до вызова события <see cref="EFPControlBase.Created"/>.
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
          throw ExceptionFactory.ObjectPropertyAlreadySet(this, "CustomOrderActive");
        _CustomOrderAllowed = value;
      }
    }
    private bool _CustomOrderAllowed;

    /// <summary>
    /// True, если в текущий момент используется произвольная сортировка.
    /// Для установки в true, требуется сначала установить <see cref="CustomOrderAllowed"/>=true.
    /// 
    /// При переключении с предопределенной сортировки на произвольную проверяется с помощью <see cref="IsSelectableCustomOrder(EFPDataViewOrder)"/>,
    /// что для текущей сортировки <see cref="CurrentOrder"/> в 
    /// просмотре имеются все столбцы, с помощью которых такую сортировку можно воспроизвести. Если столбцов не хватает,
    /// устанавливается произвольная сортировка по умолчанию. Если же и <see cref="DefaultCustomOrder"/> возвращает null, то переключение
    /// не выполняется и <see cref="CustomOrderActive"/> остается равным false.
    /// 
    /// При переключении с произвольной сортировки на предопределенную выполняется поиск в списке <see cref="Orders"/> порядка,
    /// соответствующему <see cref="CurrentOrder"/>. Если найден, то он устанавливается. Иначе выбирается первый элемент в списке <see cref="Orders"/>.
    /// Если список <see cref="Orders"/> пустой, устанавливается <see cref="CurrentOrder"/>=null.
    /// </summary>
    public bool CustomOrderActive
    {
      get { return _CustomOrderActive; }
      set
      {
        if (value == _CustomOrderActive)
          return;

        if (!_CustomOrderAllowed)
          throw new InvalidOperationException(Res.EFPDataView_Err_CustomOrderNotAllowed);

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
    /// Возвращается новый <see cref="EFPDataViewOrder"/> для одного столбца с сортировкой по возрастанию.
    /// Если в просмотре нет ни одного столбца с разрешенной произвольной сортировкой, возвращается null.
    /// 
    /// Никаких изменений в просмотре не происходит.
    /// </summary>
    /// <returns><see cref="EFPDataViewOrder"/> или null</returns>
    public EFPDataViewOrder DefaultCustomOrder
    {
      get
      {
        EFPDataGridViewColumn[] cols = this.VisibleColumns;
        for (int i = 0; i < cols.Length; i++)
        {
          if (cols[i].CustomOrderAllowed)
            return InitOrderDisplayName(new EFPDataViewOrder(cols[i]./*Name*/ CustomOrderColumnName)); // испр. 02.09.2025
        }

        return null;
      }
    }

    /// <summary>
    /// Возвращает true, если указанный порядок сортировки можно получить, нажимая на заголовки столбцов просмотра.
    /// Свойство <see cref="CustomOrderActive"/> не проверяется. Учитываются только свойства столбцов <see cref="EFPDataGridViewColumn.CustomOrderColumnName"/>.
    /// 
    /// Никаких изменений в просмотре не происходит.
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
    /// Содержит один из элементов в списке <see cref="Orders"/>, если используется предопределенная сортировка.
    /// Если используется произвольная сортировка, содержит актуальный объект <see cref="EFPDataViewOrder"/>.
    /// Установка свойства не меняет значения <see cref="CustomOrderActive"/>. Если <see cref="CustomOrderActive"/> имеет значение false,
    /// то присваиваимое значение должно быть из списка <see cref="Orders"/>. Если <see cref="CustomOrderActive"/> имеет значение true,
    /// то присваиваимое значение может быть любым, включая null. Переключения на предопределенную сортировку
    /// не будет, даже если значение совпадает с одним из элементов списка <see cref="Orders"/>.
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
              throw ExceptionFactory.ArgUnknownValue("value", value, Orders.ToArray());
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
    /// <see cref="CurrentOrderIndex"/> (или <see cref="CurrentOrder"/> / <see cref="CurrentOrderName"/>)
    /// </summary>
    public event EventHandler CurrentOrderChanged;

    /// <summary>
    /// Вызывается после установки нового значения свойства 
    /// <see cref="CurrentOrderIndex"/> (или <see cref="CurrentOrder"/> / <see cref="CurrentOrderName"/>).
    /// Непереопределенный метод вызывает событие <see cref="CurrentOrderChanged"/>.
    /// </summary>
    /// <param name="args">Пустой аргумент</param>
    protected virtual void OnCurrentOrderChanged(EventArgs args)
    {
      if (CurrentOrderChanged != null)
        CurrentOrderChanged(this, args);
    }

    /// <summary>
    /// Это событие должно обрабатывать установку порядка строк.
    /// Событие не вызывается, если установлено свойство <see cref="AutoSort"/>=true или
    /// если вызывается метод <see cref="PerformRefresh()"/>.
    /// Поэтому обработчик события <see cref="RefreshData"/> также должен устанавливать порядок строки.
    /// </summary>
    public event EventHandler OrderChanged;

    /// <summary>
    /// Вызывает событие <see cref="OrderChanged"/>
    /// </summary>
    /// <param name="args">Пустой аргумент</param>
    protected virtual void OnOrderChanged(EventArgs args)
    {
      if (OrderChanged != null)
        OrderChanged(this, args);
    }

    /// <summary>
    /// Если установить значение этого свойства в true, то будет вызываться событие
    /// <see cref="RefreshData"/> при изменении порядка строк.
    /// </summary>
    public bool OrderChangesToRefresh { get { return _OrderChangesToRefresh; } set { _OrderChangesToRefresh = value; } }
    private bool _OrderChangesToRefresh;

    private bool _CustomOrderAutoActivated;

    /// <summary>
    /// Проверяет текущее значение <see cref="CurrentOrder"/>.
    /// Если в таблице нет полей, необходимых для сортировки,
    /// то устанавливается новое значение <see cref="CurrentOrder"/>.
    /// Также может измениться значение <see cref="CustomOrderActive"/>.
    /// 
    /// Метод вызывается после присоединения к просмотру таблицы данных.
    /// </summary>
    private void ValidateCurrentOrder()
    {
#if DEBUG
      if (SourceAsDataTable == null)
        throw new InvalidOperationException(Res.EFPDataView_Err_NoDataView);
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
    /// Если установить свойство в true, то не будет вызываться событие <see cref="CurrentOrderChanged"/>.
    /// Вместо этого при переключении порядка сортировки пользователеми будет 
    /// вызываться метод <see cref="PerformAutoSort()"/> для установки значения <see cref="DataView.Sort"/>.
    /// </summary>
    public bool AutoSort { get { return _AutoSort; } set { _AutoSort = value; } }
    private bool _AutoSort;

    private static bool _Inside_PerformAutoSort; // 09.09.2020

    /// <summary>
    /// Установить значение <see cref="DataView.Sort"/>.
    /// Используйте этот метод в обработчике события <see cref="RefreshData"/>
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
        EFPApp.ShowException(e);
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
          EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoPredefinedOrders);
          return false;
        }
        else
        {
          EFPApp.ShowTempMessage(Res.EFPDataView_Err_OrderNotAllowed);
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
          EFPApp.ShowTempMessage(Res.EFPDataView_Err_AlreadySorted);
          return false;
        }
      }


      //int orderIndex = Orders.IndexOfItemForGridColumn(clickedColumnName);
      //if (orderIndex >= 0)
      //{
      //  CurrentOrderIndex = orderIndex;
      //  return true;
      //}

      // 28.09.2022.
      // Неправильно работает при наличии нескольких ClickableColumnNames для сортировки.
      // Пусть есть 2 порядка сортировки:
      // Order1, заданы кликабельные столбцы "C1" и "C2".
      // Order2, задан кликабельный столбец "C2".
      // Пусть в просмотре есть оба столбца. Предполагается, что щелчок на "C1" установит Order1, а на "C2" - Order2.
      // Если пользователь щелкает столбец "C2", то IndexOfItemForGridColumn() вернет Order1, т.к. столбец "C2" тоже подходит.
      // При этом повторный щелчок по "C2" тоже не будет переключать на Order2.
      // Нужно использовать GetUsedColumnName().
      for (int i = 0; i < Orders.Count; i++)
      {
        if (GetUsedColumnName(Orders[i]) == clickedColumnName)
        {
          CurrentOrderIndex = i;
          return true;
        }
      }

      EFPApp.ShowTempMessage(Res.EFPDataView_Err_ColumnHasNoSort);
      return false;
    }

    private bool DoControl_ColumnHeaderMouseClickCustom(string clickedColumnName)
    {
      EFPDataGridViewColumn col = Columns[clickedColumnName];
      if (!col.CustomOrderAllowed)
      {
        EFPApp.ShowTempMessage(String.Format(Res.EFPDataView_Err_ColumnCustomSortNotAllowed, col.GridColumn.HeaderText));
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
          EFPApp.ShowTempMessage(Res.EFPDataView_Err_ComplexSortNotStarted);
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
          EFPApp.ShowTempMessage(Res.EFPDataView_Err_ComplexSortRepeatedColumn);
          return false;

        }
        currColNames = ArrayTools.MergeArrays<string>(currColNames, new string[] { col.CustomOrderColumnName });
        currDirs = ArrayTools.MergeArrays<ListSortDirection>(currDirs, new ListSortDirection[] { ListSortDirection.Ascending });
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

        sb.Append(" (");
        sb.Append(UITools.ToString(dirs[i], colNames.Length == 1)); // короткая форма если сложный порядок сортировки
        sb.Append(")");
      }
      return sb.ToString();
    }

    /// <summary>
    /// Должен ли <see cref="EFPDataGridView"/> управлять свойствами <see cref="DataGridViewColumn.SortMode"/> и <see cref="DataGridViewColumnHeaderCell.SortGlyphDirection"/>
    /// для рисования треугольников в заголовках столбцов, и выполнять сортировку по нажатию кнопки мыши.
    /// По умолчанию - true - обработка разрешена. При этом, если <see cref="OrderCount"/>=0 и <see cref="CustomOrderAllowed"/>=false,
    /// щелчок по заголовку выдает сообщение в статусной строке "Сортировка невозможна".
    /// Установка значения false восстанавливает поведение <see cref="DataGridView"/> по умолчанию для автоматической сортировки.
    /// Свойство можно устанавливать только до появления события <see cref="EFPControlBase.Created"/>.
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
    /// Прорисовка трегольников сортировки в заголовках столбцов в соответствии с 
    /// текущим порядком сортировки CurrentOrder
    /// </summary>
    private void InitColumnHeaderTriangles()
    {
      if (ProviderState != EFPControlProviderState.Attached)
        return;
      if (!UseColumnHeaderClick)
        return;

      foreach (DataGridViewColumn col in Control.Columns)
        col.HeaderCell.SortGlyphDirection = SortOrder.None;

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
            EFPDataGridViewColumn col;
            if (columnsForSort.TryGetValue(columnNames[i], out col))
            {
              try
              {
                if (directions[i] == ListSortDirection.Descending)
                  col.GridColumn.HeaderCell.SortGlyphDirection = SortOrder.Descending;
                else
                  col.GridColumn.HeaderCell.SortGlyphDirection = SortOrder.Ascending;
              }
              catch (Exception e)
              {
                e.Data["DislayName"] = this.DisplayName;
                e.Data["ColumnName"] = col.Name;
                e.Data["CustomOrderActive"] = CustomOrderActive;
                try
                {
                  Form frm = Control.FindForm();
                  if (frm != null)
                    e.Data["Form"] = frm.ToString();
                }
                catch { }

                LogoutTools.LogoutException(e, LogoutTools.GetTitleForCall("DataGridViewColumn.SortGlyphDirection"));
              }
            }
          }
        }
        else // предопределенная конфигурация
        {
          EFPDataGridViewColumn col = GetUsedColumn(CurrentOrder);
          if (col != null && col.GridColumn.SortMode != DataGridViewColumnSortMode.NotSortable)
          {
            try
            {
              if (CurrentOrder.SortInfo.Direction == ListSortDirection.Descending)
                col.GridColumn.HeaderCell.SortGlyphDirection = SortOrder.Descending;
              else
                col.GridColumn.HeaderCell.SortGlyphDirection = SortOrder.Ascending;
            }
            catch (Exception e)
            {
              e.Data["DislayName"] = this.DisplayName;
              e.Data["ColumnName"] = col.Name;
              e.Data["CustomOrderActive"] = CustomOrderActive;
              try
              {
                Form frm = Control.FindForm();
                if (frm != null)
                  e.Data["Form"] = frm.ToString();
              }
              catch { }

              LogoutTools.LogoutException(e, LogoutTools.GetTitleForCall("DataGridViewColumn.SortGlyphDirection"));
            }
          }
        }
      }
    }

    /// <summary>
    /// Показать диалог выбора порядка строк из списка <see cref="Orders"/> или пользовательской сортировки, если она разрешена.
    /// Если пользователь выбирает порядок, то устанавливается свойство <see cref="CurrentOrder"/>
    /// </summary>
    /// <returns>True, если порядок сортировки был установлен пользователем</returns>
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
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_OrderNotAllowed);
        return false;
      }

      return EFPDataGridViewOrderForm.PerformSort(this);

#endif
    }

    #endregion

    #endregion

    #region Индекс используемого столбца для порядка сортировки

    /// <summary>
    /// Возвращает индекс столбца табличного просмотра (DataGridViewColumn.Index), 
    /// который будет использоваться для щелчка мыши для заданного порядка сортировки.
    /// Если в просмотре нет ни одного подходящего столбца, возвращается (-1).
    /// </summary>
    /// <param name="item">Порядок сортировки</param>
    /// <returns>Индекс столбца</returns>
    public int IndexOfUsedColumn(EFPDataViewOrder item)
    {
      if (item == null)
        return (-1);

      if (item.SortInfo.IsEmpty)
        return -1;

      // int bestColumnIndex = -1;
      // int bestDisplayIndex = -1;
      for (int i = 0; i < item.SortInfo.ClickableColumnNames.Length; i++)
      {
        int columnIndex = Columns.IndexOf(item.SortInfo.ClickableColumnNames[i]);
        if (columnIndex < 0)
          continue;
        DataGridViewColumn col = Control.Columns[columnIndex];
        if (!col.Visible)
          continue; // скрытые столбцы не считаются

        // 05.11.2024
        // Приоритет столбцов, заданный в ClickableColumnNames важнее порядка столбцов в просмотре
        return columnIndex;

        //int displayIndex = col.DisplayIndex;
        //if (bestColumnIndex < 0 || displayIndex < bestDisplayIndex)
        //{
        //  bestColumnIndex = columnIndex;
        //  bestDisplayIndex = displayIndex;
        //}
      }

      //return bestColumnIndex;
      return -1;
    }

    /// <summary>
    /// Возвращает имя столбца табличного просмотра (<see cref="EFPDataGridViewColumn.Name"/>), 
    /// который будет использоваться для щелчка мыши для заданного порядка сортировки.
    /// Если в просмотре нет ни одного подходящего столбца, возвращается пустая строка.
    /// </summary>
    /// <param name="item">Порядок сортировки</param>
    /// <returns>Имя столбца</returns>
    public string GetUsedColumnName(EFPDataViewOrder item)
    {
      int p = IndexOfUsedColumn(item);
      if (p < 0)
        return String.Empty;
      else
        return Columns[p].Name;
    }

    /// <summary>
    /// Возвращает столбец <see cref="EFPDataGridViewColumn"/>, 
    /// который будет использоваться для щелчка мыши для заданного порядка сортировки.
    /// Если в просмотре нет ни одного подходящего столбца, возвращается null.
    /// </summary>
    /// <param name="item">Порядок сортировки</param>
    /// <returns>Объект столбца</returns>
    public EFPDataGridViewColumn GetUsedColumn(EFPDataViewOrder item)
    {
      int p = IndexOfUsedColumn(item);
      if (p < 0)
        return null;
      else
        return Columns[p];
    }

    #endregion

    #region Ручная сортировка строк

    #region Общая часть

    /// <summary>
    /// Свойство возвращает true, если подерживается ручная перестановка строк с помощью стрелочек вверх/вниз
    /// </summary>
    public bool ManualOrderSupported
    {
      get { return ManualOrderRows || (!String.IsNullOrEmpty(ManualOrderColumn)); }
    }

    /// <summary>
    /// Свойство возвращает true, если есть возможность восстановить порядок сортировки строк по умолчанию
    /// </summary>
    public bool RestoreManualOrderSupported
    {
      get
      {
        return DefaultManualOrderRows != null ||
          (!String.IsNullOrEmpty(DefaultManualOrderColumn));
      }
    }

    /// <summary>
    /// Выполняет сдвиг выбранных строк в просмотре вверх или вниз
    /// </summary>
    /// <param name="down"></param>
    internal void ChangeManualOrder(bool down)
    {
      if (!Control.EndEdit())
      {
        EFPApp.ErrorMessageBox(Res.EFPDataView_Err_EditNotFinished);
        return;
      }

      int oldColIdx = CurrentColumnIndex;

      bool changed;
      if (ManualOrderRows)
        changed = DoReorderByGridRows(down);
      else
        changed = DoReorderByDataColumn(down);

      // 9. Обновляем табличный просмотр
      if (changed)
      {
        CurrentColumnIndex = oldColIdx;
        OnManualOrderChanged(EventArgs.Empty);
      }
    }

    /// <summary>
    /// Восстанавливает порядок сортировки
    /// </summary>
    internal void RestoreManualOrder()
    {
      int oldColIdx = CurrentColumnIndex;

      bool changed;
      if (ManualOrderRows)
        changed = DoSortRestoreRows();
      else
        changed = DoSortRestoreColumn();

      // Обновляем табличный просмотр
      if (changed)
      {
        CurrentColumnIndex = oldColIdx;

        OnManualOrderChanged(EventArgs.Empty);
      }
    }

    /// <summary>
    /// Вызывается, когда выполнена ручная сортировка строк (по окончании изменения
    /// значений поля для всех строк)
    /// </summary>
    public event EventHandler ManualOrderChanged;

    /// <summary>
    /// Генерирует событие <see cref="ManualOrderChanged"/>
    /// </summary>
    /// <param name="args">Пустой аргумент события</param>
    protected virtual void OnManualOrderChanged(EventArgs args)
    {
      if (ManualOrderChanged != null)
        ManualOrderChanged(this, args);
    }

    #endregion

    #region Перестановка строк DataGridViewRow

    /// <summary>
    /// Если установлено в true, то доступны команды ручной сортировки строк.
    /// Сортировка основывается на порядке строк в коллекции, а не на значении
    /// столбца. При сортировке строки переставляются местами внутри объекта
    /// <see cref="DataGridViewRowCollection"/>
    /// </summary>
    public bool ManualOrderRows
    {
      get { return _ManualOrderRows; }
      set
      {
        CheckHasNotBeenCreated();
        _ManualOrderRows = value;
      }
    }
    private bool _ManualOrderRows;

    /// <summary>
    /// Массив строк табличного просмотра в порядке по умолчанию. Свойство действует
    /// при <see cref="ManualOrderRows"/>=true. Если массив присвоен, то действует команда
    /// "Восстановить порядок по умолчанию"
    /// </summary>
    public DataGridViewRow[] DefaultManualOrderRows { get { return _DefaultManualOrderRows; } set { _DefaultManualOrderRows = value; } }
    private DataGridViewRow[] _DefaultManualOrderRows;


    /// <summary>
    /// Изменение порядка строк на основании их расположения в коллекции
    /// </summary>
    /// <param name="down">true - сдвиг вниз, false -  сдвиг вверх</param>
    /// <returns>true, если перемещение было выполнено</returns>
    private bool DoReorderByGridRows(bool down)
    {
      // 1. Загружаем полный список строк DataGridViewRow в массив
      DataGridViewRow[] rows1 = new DataGridViewRow[Control.Rows.Count];
      Control.Rows.CopyTo(rows1, 0);

      // 2. Запоминаем выбранные строки
      DataGridViewRow[] selRows = SelectedGridRows;
      // 3. Получаем позиции выбранных строк в массиве всех строк
      int[] selPoss = SelectedRowIndices;
      if (selPoss.Length == 0)
      {
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoSelectedRow);
        return false;
      }

      // 4. Проверяем, что не уперлись в границы списка
      bool lBound = false;
      if (down)
      {
        if (selPoss[selPoss.Length - 1] == rows1.Length - 1)
          lBound = true;
      }
      else
      {
        if (selPoss[0] == 0)
          lBound = true;
      }
      if (lBound)
      {
        EFPApp.ShowTempMessage(String.Format(Res.EFPDataView_Err_MoveRowsUpDown,
          selPoss.Length,
          down ? Res.EFPDataView_Msg_MoveDown : Res.EFPDataView_Msg_MoveUp));
        return false;
      }

      // 5. Подготавливаем массив строк для их размещения в новом порядке
      // Значения null в этом массиве означают временно пустые позиции
      DataGridViewRow[] rows2 = new DataGridViewRow[rows1.Length];

      // 6. Копируем в Rows2 строки из Rows1 со сдвигом для позиций, существующих
      // в SelRows.
      // В процессе перемещения будем очищать массив Rows1
      int delta = down ? 1 : -1; // значение смещения
      int i;
      for (i = 0; i < selPoss.Length; i++)
      {
        int thisPos = selPoss[i];
        rows2[thisPos + delta] = rows1[thisPos];
        rows1[thisPos] = null;
      }

      // 7. Перебираем исходный массив и оставшиеся непустые строки размещаем в
      // новом массиве, отыскивая пустые места. Для этого используем переменную FreePos
      // для указания на очередную пустую позицию второго массива
      int freePos = 0;
      for (i = 0; i < rows1.Length; i++)
      {
        if (rows1[i] == null) // перемещенная позиция
          continue;
        // Поиск места
        while (rows2[freePos] != null)
          freePos++;
        // Нашли дырку
        rows2[freePos] = rows1[i];
        freePos++;
      }

      // 8. Замещаем коллекцию строк
      Control.Rows.Clear();
      Control.Rows.AddRange(rows2);

      // 9. Восстанавливаем выбор
      SelectedGridRows = selRows;
      return true;
    }

    /// <summary>
    /// Восстановление порядка по умолчанию на основании <see cref="DefaultManualOrderRows"/>
    /// </summary>
    /// <returns></returns>
    private bool DoSortRestoreRows()
    {
      if (DefaultManualOrderRows == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "DefaultManualOrderRows");

      // 1. Загружаем полный список строк DataGridViewRow в массив
      DataGridViewRow[] rows1 = new DataGridViewRow[Control.Rows.Count];
      Control.Rows.CopyTo(rows1, 0);

      // 2. Запоминаем выбранные строки
      DataGridViewRow[] selRows = SelectedGridRows;

      // 3. Подготавливаем массив строк для их размещения в новом порядке
      // Значения null в этом массиве означают временно пустые позиции
      DataGridViewRow[] rows2 = new DataGridViewRow[rows1.Length];

      // 4. Копируем строки из массива по умолчанию
      int i;
      int cnt = 0;
      bool changed = false;
      for (i = 0; i < DefaultManualOrderRows.Length; i++)
      {
        int thisPos = Array.IndexOf<DataGridViewRow>(rows1, DefaultManualOrderRows[i]);
        if (thisPos < 0)
          continue; // Ошибка

        rows2[cnt] = rows1[thisPos];
        rows1[thisPos] = null;
        if (cnt != thisPos)
          changed = true;
        cnt++;
      }

      // 5. Копируем "лишние" строки, которых нет в массиве по умолчанию
      for (i = 0; i < rows1.Length; i++)
      {
        if (rows1[i] != null)
        {
          rows2[cnt] = rows1[i];
          if (cnt != i)
            changed = true;
          cnt++;
        }
      }

      if (!changed)
        return false;

      // 6. Замещаем коллекцию строк
      Control.Rows.Clear();
      Control.Rows.AddRange(rows2);

      // 7. Восстанавливаем выбор
      SelectedGridRows = selRows;
      return true;
    }

    #endregion

    #region Перестановка с помощью числового поля

    #region Свойство DataReorderHelper

    /// <summary>
    /// Возвращает объект, реализующий интерфейс <see cref="IDataReorderHelper"/>.
    /// Если свойство <see cref="ManualOrderColumn"/> не установлено, возвращается null.
    /// Для инициализации значения однократно вызывается событие <see cref="DataReorderHelperNeeded"/>.
    /// Если требуется, чтобы свойство вернуло новое значение, используйте метод <see cref="ResetDataReorderHelper()"/>.
    /// </summary>
    /// <returns>Объект IDataReorderHelper</returns>
    public IDataReorderHelper DataReorderHelper
    {
      get
      {
        if (String.IsNullOrEmpty(ManualOrderColumn))
          return null;

        if (ProviderState == EFPControlProviderState.Initialization || ProviderState == EFPControlProviderState.Disposed)
          return null;

        if (_DataReorderHelper == null)
          _DataReorderHelper = CreateDataReorderHelper();
        return _DataReorderHelper;
      }
    }
    private IDataReorderHelper _DataReorderHelper;

    /// <summary>
    /// Очищает сохраненное значение свойства <see cref="DataReorderHelper"/>, чтобы при следующем обращении к нему
    /// вновь было вызвано событие <see cref="DataReorderHelperNeeded"/>.
    /// </summary>
    public virtual void ResetDataReorderHelper()
    {
      _DataReorderHelper = null;
    }

    /// <summary>
    /// Событие вызывается при выполнении команд ручной сортировки, если установлено свойство <see cref="ManualOrderColumn"/>.
    /// Обработчик события может создать собственный экземпляр и установить свойство <see cref="DataReorderHelperNeededEventArgs.Helper"/>.
    /// Если обработчика нет, или он не создал объект, то создается объект по умолчанию.
    /// </summary>
    public event DataReorderHelperNeededEventHandler DataReorderHelperNeeded;

    /// <summary>
    /// Вызывает обработчик события <see cref="DataReorderHelperNeeded"/>.
    /// Если <see cref="IDataReorderHelper"/> не получен, объект создается вызовом метода <see cref="CreateDefaultDataReorderHelper()"/>.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnDataReorderHelperNeeded(DataReorderHelperNeededEventArgs args)
    {
      if (DataReorderHelperNeeded != null)
        DataReorderHelperNeeded(this, args);
      if (args.Helper == null)
        args.Helper = CreateDefaultDataReorderHelper();
    }

    /// <summary>
    /// Создает новый экземпляр <see cref="IDataReorderHelper"/>.
    /// Вызывает метод <see cref="OnDataReorderHelperNeeded(DataReorderHelperNeededEventArgs)"/>.
    /// </summary>
    /// <returns>Объект для упорядочения строк</returns>
    public IDataReorderHelper CreateDataReorderHelper()
    {
      DataReorderHelperNeededEventArgs args = new DataReorderHelperNeededEventArgs();
      OnDataReorderHelperNeeded(args);
      if (args.Helper == null)
        throw ExceptionFactory.ObjectPropertyNotSet(args, "Helper");
      return args.Helper;
    }

    /// <summary>
    /// Этот метод может использоваться в обработчике события <see cref="DataReorderHelperNeeded"/> другого провайдера, который должен использовать реализацию из текущего провайдера
    /// </summary>
    /// <param name="sender">Объект, сгенерировавший событие. Не используется</param>
    /// <param name="args">Аргументы события. В нем устанавливается свойство <see cref="DataReorderHelperNeededEventArgs.Helper"/></param>
    public void CreateDataReorderHelper(object sender, DataReorderHelperNeededEventArgs args)
    {
      args.Helper = CreateDataReorderHelper();
    }

    /// <summary>
    /// Создает объект, предназначенный для сортировки строк с помощью числового поля.
    /// Используется при установленном свойстве <see cref="ManualOrderColumn"/>, если не установлен пользовательский обработчик события <see cref="DataReorderHelperNeeded"/>.
    /// Непереопределенный метод создает новый объект <see cref="DataTableReorderHelper"/>.
    /// Переопределяется, если табличный просмотр предназначен для просмотра иерархических данных, а не плоской таблицы.
    /// </summary>
    /// <returns>Объект, реализующий интерфейс <see cref="IDataReorderHelper"/></returns>
    protected virtual IDataReorderHelper CreateDefaultDataReorderHelper()
    {
      if (String.IsNullOrEmpty(ManualOrderColumn))
        throw ExceptionFactory.ObjectPropertyNotSet(this, "ManualOrderColumn");

      // Получаем доступ к объекту DataView
      DataView dv = SourceAsDataView;
      if (dv == null)
        throw new InvalidDataSourceException(Res.EFPDataView_Err_NoDataView);

      return new DataTableReorderHelper(dv, ManualOrderColumn);
    }
    #endregion

    #region Перестановка строк

    /// <summary>
    /// Имя числового столбца, который определяет порядок строк при ручной сортировке.
    /// Если задано, то в меню есть команды ручной сортировки.
    /// </summary>
    public string ManualOrderColumn
    {
      get { return _ManualOrderColumn; }
      set
      {
        CheckHasNotBeenCreated();
        _ManualOrderColumn = value;

        if (!String.IsNullOrEmpty(value))
          this.AutoSort = false; // 21.07.2021
      }
    }
    private string _ManualOrderColumn;


    /// <summary>
    /// Изменение порядка строк на основании числового поля
    /// </summary>
    /// <param name="down">true - сдвиг вниз, false -  сдвиг вверх</param>
    /// <returns>true, если перемещение было выполнено</returns>
    private bool DoReorderByDataColumn(bool down)
    {
      if (DataReorderHelper == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "DataReorderHelper");

      // Загружаем выбранные строки
      DataRow[] selRows = this.SelectedDataRows;
      if (selRows.Length == 0)
      {
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoSelectedRow);
        return false;
      }

      bool res;
      if (down)
        res = DataReorderHelper.MoveDown(selRows);
      else
        res = DataReorderHelper.MoveUp(selRows);

      if (!res)
      {
        EFPApp.ShowTempMessage(String.Format(Res.EFPDataView_Err_MoveRowsUpDown,
          selRows.Length,
          down ? Res.EFPDataView_Msg_MoveDown : Res.EFPDataView_Msg_MoveUp));
        return false;
      }

      // Обновляем просмотр
      Control.Invalidate();
      this.SelectedDataRows = selRows; // 26.10.2017

      return true;
    }

    #endregion

    #region Восстановление порядка

    /// <summary>
    /// Имя числового столбца, который определяет порядок строк по умолчанию при ручной
    /// сортировке. Свойство действует при непустом значении <see cref="ManualOrderColumn"/>.
    /// Если имя присвоено, то действует команда "Восстановить порядок по умолчанию"
    /// </summary>
    public string DefaultManualOrderColumn
    {
      get { return _DefaultManualOrderColumn; }
      set
      {
        CheckHasNotBeenCreated();
        _DefaultManualOrderColumn = value;
      }
    }
    private string _DefaultManualOrderColumn;

    /// <summary>
    /// Восстановление порядка по умолчанию на основании DefaultManualOrderColumn
    /// </summary>
    /// <returns></returns>
    private bool DoSortRestoreColumn()
    {
      if (String.IsNullOrEmpty(DefaultManualOrderColumn))
        throw ExceptionFactory.ObjectPropertyNotSet(this, "DefaultManualOrderColumn");
      if (DataReorderHelper == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "DataReorderHelper");

      DataRow[] desiredOrder;
      using (DataView dv = new DataView(SourceAsDataTable))
      {
        dv.Sort = DefaultManualOrderColumn;
        desiredOrder = DataTools.GetDataViewRows(dv);
      }

      bool changed = DataReorderHelper.Reorder(desiredOrder);

      if (changed)
        Control.Refresh();
      return changed;
    }

    #endregion

    #region InitManualOrderColumnValue()

    /// <summary>
    /// Выполняет инициализацию значения поля, заданного свойством <see cref="ManualOrderColumn"/> для новых строк, у которых поле имеет значение 0.
    /// Вызывает метод <see cref="IDataReorderHelper.InitRows(DataRow[], out bool)"/>.
    /// Если свойcтво <see cref="ManualOrderColumn"/> не установлено, никаких действий не выполняется.
    /// </summary>
    /// <param name="rows">Строки данных, которые нужно инициализировать</param>
    /// <param name="otherRowsChanged">Сюда записывается значение true, если были изменены другие строки в просмотре, кроме выбранных.</param>
    /// <returns>True, если строки (одна или несколько) содержали нулевое значение и были инициализированы.
    /// Если все строки уже содержали ненулевое значение, то возвращается false.</returns>
    public bool InitManualOrderColumnValue(DataRow[] rows, out bool otherRowsChanged)
    {
      if (DataReorderHelper == null)
      {
        otherRowsChanged = false;
        return false;
      }

      return DataReorderHelper.InitRows(rows, out otherRowsChanged);
    }

    #endregion

    #endregion

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Список команд локального меню.
    /// </summary>
    public new EFPDataGridViewCommandItems CommandItems { get { return (EFPDataGridViewCommandItems)(base.CommandItems); } }

    /// <summary>
    /// Создает <see cref="EFPDataGridViewCommandItems"/>
    /// </summary>
    /// <returns>Новый экземпляр команд меню</returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      return new EFPDataGridViewCommandItems(this);
    }

    ///// <summary>
    ///// </summary>
    //protected override void OnBeforePrepareCommandItems()
    //{
    //  base.OnBeforePrepareCommandItems();
    //  //// 01.12.2020
    //  //// При выполнением 
    //  //CommandItems.AddClickHandler(ResetCurrentIncSearchColumn);
    //}

    /// <summary>
    /// Возвращает список для добавления пользовательских вариантов печати/экспорта.
    /// Обычно список содержит единственный объект с именем "Control" для стандартной печати табличного просмотра, 
    /// доступ к которому удобнее получать с помощью свойства <see cref="DefaultOutItem"/>.
    /// </summary>
    public NamedList<EFPMenuOutItem> MenuOutItems { get { return CommandItems.OutHandler.Items; } }

    /// <summary>
    /// Параметры печати/экспорта табличного просмотра.
    /// Может возвращать null, если в <see cref="EFPDataGridViewCommandItems.OutHandler"/> был удален вариант "Control"
    /// </summary>
    public Reporting.BRDataGridViewMenuOutItem DefaultOutItem
    {
      get { return MenuOutItems["Control"] as Reporting.BRDataGridViewMenuOutItem; }
    }

    Reporting.BRDataViewMenuOutItemBase IEFPDataView.DefaultOutItem { get { return DefaultOutItem; } }

    #endregion

    #region Поддержка Inline-редактирования

    /// <summary>
    /// Встроенное выполнение операций редактирования без использования
    /// внешних обработчиков.
    /// Поддерживаются режимы "Редактирование" и "Удаление"
    /// В режиме <paramref name="state"/>=Edit вызывается <see cref="DataGridView.BeginEdit(bool)"/>, при этом проверяются свойства ReadOnly
    /// </summary>
    /// <param name="state">Edit или Delete</param>
    public void InlineEditData(UIDataState state)
    {
      switch (state)
      {
        case UIDataState.Edit:
          // 09.01.2013
          // Лучше лишний раз проверить режим ReadOnly и выдать ShowTempMessage(),
          // т.к. обычный BeginEdit() не будет выдавать сообщение, если для столбца
          // установлено ReadOnly=true

          if (Control.IsCurrentCellInEditMode)
          {
            EFPApp.ShowTempMessage(Res.EFPDataView_Err_EditNotFinished);
            return;
          }
          string readOnlyMessage;
          if (GetCellReadOnly(Control.CurrentCell, out readOnlyMessage))
          {
            EFPApp.ShowTempMessage(readOnlyMessage);
            return;
          }

          Control.BeginEdit(true);
          break;
        case UIDataState.Delete:
          if (WholeRowsSelected && Control.AllowUserToDeleteRows)
          {
            DataGridViewRow[] rows = SelectedGridRows;
            if (rows.Length == 0)
              EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoSelectedRow);
            else
            {
              for (int i = 0; i < rows.Length; i++)
                Control.Rows.Remove(rows[i]);
            }
          }
          else
            ClearSelectedCells();
          break;
        default:
          EFPApp.ShowTempMessage(String.Format(Res.EFPDataView_Err_InlineEditNotImplemented, state));
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
      EFPDataGridViewRectArea area = new EFPDataGridViewRectArea(Control, EFPDataViewExpRange.Selected);
      ClearCells(area);
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
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoSelectedCell);
        return;
      }

      #region Первый проход. Проверяем возможность удаления

      // Проверяем, что мы можем записать пустой текст во все ячейки
      for (int i = 0; i < area.RowCount; i++)
      {
        for (int j = 0; j < area.ColumnCount; j++)
        {
          DataGridViewCell cell = area[j, i];
          if (!cell.Selected)
            continue;

          string errorText;
          if (GetCellReadOnly(cell, out errorText))
          {
            Control.CurrentCell = cell;
            EFPApp.ShowTempMessage(errorText);
            return;
          }

          if (!TrySetTextValue(cell, String.Empty, out errorText, true, EFPDataGridViewCellFinishedReason.Clear))
          {
            Control.CurrentCell = cell;
            EFPApp.ShowTempMessage(errorText);
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
          DataGridViewCell cell = area[j, i];
          if (!cell.Selected)
            continue;

          SetTextValue(cell, String.Empty, EFPDataGridViewCellFinishedReason.Edit);
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
        EFPApp.ShowException(e);
      }
    }
    void DoControl_CellBeginEdit1(DataGridViewCellCancelEventArgs args)
    {
      if (args.Cancel)
        return;
      if (args.ColumnIndex < 0)
        return; // никогда не бывает

      string ReadOnlyMessage;
      bool isRO = GetCellReadOnly(args.RowIndex, args.ColumnIndex, out ReadOnlyMessage);

      if (isRO)
      {
        args.Cancel = true;
        DataGridViewColumn col = Control.Columns[args.ColumnIndex];
        if (!(col is DataGridViewCheckBoxColumn))
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
        EFPApp.ShowException(e);
      }
    }
    void DoControl_CellBeginEdit2(DataGridViewCellCancelEventArgs args)
    {
      if (args.Cancel)
        return;

      if (args.ColumnIndex < 0)
        return; // никогда не бывает

      DataGridViewColumn col = Control.Columns[args.ColumnIndex];
      if (col is DataGridViewCheckBoxColumn) // 04.11.2009 - для CheckBox-не надо
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
        EFPApp.ShowException(e);
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
        EFPApp.ShowException(e);
      }
    }

    private void DoControl_CellContentClick(DataGridViewCellEventArgs args)
    {
      if (args.RowIndex < 0 || args.ColumnIndex < 0)
        return;

      if (Control.Columns[args.ColumnIndex] is DataGridViewCheckBoxColumn)
      {
        string readOnlyMessage;
        bool isRO = GetCellReadOnly(args.RowIndex, args.ColumnIndex, out readOnlyMessage);
        if (isRO && GetCellContentVisible(args.RowIndex, args.ColumnIndex))
          EFPApp.ShowTempMessage(readOnlyMessage);
      }
      else if (Control.Columns[args.ColumnIndex] is DataGridViewLinkColumn /*||
        Control.Columns[args.ColumnIndex] is DataGridViewButtonColumn */) // пока не надо
      {
        //BRValueWithLink linkVal = Control[args.ColumnIndex, args.RowIndex].Value as BRValueWithLink;
        GetRowInfo(args.RowIndex, EFPDataViewInfoReason.View);
        EFPDataGridViewCellInfoEventArgs cellArgs = GetCellInfo(args.ColumnIndex);
        BRValueWithLink value = cellArgs.FormattedValue as BRValueWithLink;
        if (value == null)
          value = cellArgs.Value as BRValueWithLink;
        if (value != null)
        {
          if (value.LinkData[0] != '#')
          {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = value.LinkData;
            psi.UseShellExecute = true;
            Process.Start(psi);
          }
        }
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
        EFPApp.ShowException(e);
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
        EFPApp.ShowException(e);
      }
    }

    private void DoControl_CellParsing(DataGridViewCellParsingEventArgs args)
    {
      if (args.ParsingApplied)
        return;

      DataGridViewCell cell = Control[args.ColumnIndex, args.RowIndex];
      if (cell is DataGridViewTextBoxCell)
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
      if (!MathTools.IsFloatType(args.DesiredType))
        return false; // не число с плавающей точкой

      UITools.CorrectNumberString(ref s, args.InheritedCellStyle.FormatProvider);
      DataGridViewCell cell = Control[args.ColumnIndex, args.RowIndex];
      try
      {
        args.Value = cell.ParseFormattedValue(s, args.InheritedCellStyle, null, null);
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
        EFPApp.ShowException(e);
        args.Cancel = true;
      }
    }

    private void DoControl_CellValidating(DataGridViewCellValidatingEventArgs args)
    {
      if (args.Cancel)
        return;

      DataGridViewCell cell = Control[args.ColumnIndex, args.RowIndex];
      if (!Control.IsCurrentCellInEditMode)
        return;

      // 15.12.2012
      // Проверяем корректность маски
      IMaskProvider maskProvider = Columns[args.ColumnIndex].MaskProvider;
      if (maskProvider != null && cell.ValueType == typeof(string))
      {
        string s = args.FormattedValue.ToString().Trim();
        if (s.Length > 0)
        {
          string errorText;
          if (!maskProvider.Test(args.FormattedValue.ToString(), out errorText))
          {
            EFPApp.ShowTempMessage(errorText);
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
        EFPApp.ShowException(e);
      }
    }

    private bool _InsideCellValueChanged;

    private void DoControl_CellValueChanged(DataGridViewCellEventArgs args)
    {
      if (_InsideCellValueChanged)
        return;

      _InsideCellValueChanged = true;
      try
      {
        if (Control.IsCurrentCellInEditMode)
        {
          if (Control.CurrentCell.RowIndex == args.RowIndex && Control.CurrentCell.ColumnIndex == args.ColumnIndex)
          {
            OnCellFinished(args.RowIndex, args.ColumnIndex, EFPDataGridViewCellFinishedReason.Edit);
            Control.InvalidateRow(args.RowIndex);
          }
        }
      }
      finally
      {
        _InsideCellValueChanged = false;
      }
    }

    /// <summary>
    /// Событие вызывается, когда закончено редактирование ячейки и новое значение
    /// <see cref="DataGridViewCell.Value"/> установлено (вызывается из <see cref="DataGridView.CellValueChanged"/>. Событие не 
    /// вызывается, если пользователь нажал Esc.
    /// Обработчик может, например, выполнить расчет других значений строки или
    /// расчет итоговой строки.
    /// Также событие вызывается после вставки значений из буфера обмена и
    /// установки / снятия отметки строки (MarkRow)
    /// </summary>
    public event EFPDataGridViewCellFinishedEventHandler CellFinished;

    private bool _CellFinishedErrorShown = false;

    /// <summary>
    /// Вызов события <see cref="CellFinished"/>
    /// </summary>
    /// <param name="rowIndex">Индекс строки данных</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <param name="reason">Причина вызова (ручное редактирование, буфер обмена, ...)</param>
    public virtual void OnCellFinished(int rowIndex, int columnIndex, EFPDataGridViewCellFinishedReason reason)
    {
      CurrentIncSearchColumn = null; // 29.04.2025. Если переключен флажок выбора строк, то надо закончить поиск по первым буквам

      if (CellFinished == null)
        return;

      EFPDataGridViewCellFinishedEventArgs args = new EFPDataGridViewCellFinishedEventArgs(this,
        rowIndex, columnIndex, reason);
      try
      {
        CellFinished(this, args);
      }
      catch (Exception e)
      {
        if (!_CellFinishedErrorShown)
        {
          _CellFinishedErrorShown = true;
          EFPApp.ShowException(e);
        }
      }

      _ValidateRequiredFlag = true;
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
        readOnlyMessage = Res.EFPDataView_Err_NoSelectedCell;
        return true;
      }
      return GetCellReadOnly(cell.RowIndex, cell.ColumnIndex, out readOnlyMessage);
    }

    /// <summary>
    /// Возвращает true, если заданная ячейка имеет установленный атрибут "только чтение".
    /// В первую очередь вызывается <see cref="GetRowReadOnly(int, out string)"/> для проверки атрибута строки.
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <param name="readOnlyMessage">Сюда помещается текст, почему нельзя редактировать ячейку</param>
    /// <returns>true, если ячейку нельзя редактировать</returns>
    public bool GetCellReadOnly(int rowIndex, int columnIndex, out string readOnlyMessage)
    {
      if (GetRowReadOnly(rowIndex, out readOnlyMessage))
        return true;

      GetCellInfo(columnIndex);
      readOnlyMessage = _CellInfoNeededArgs.ReadOnlyMessage;
      return _CellInfoNeededArgs.ReadOnly;
    }

    /// <summary>
    /// Возвращает true, если заданная строка имеет установленный атрибут "только чтение".
    /// Также может быть задан атрибут для отдельных ячеек.
    /// Обычно следует использовать метод <see cref="GetCellReadOnly(int, int, out string)"/>.
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="readOnlyMessage">Сюда помещается текст, почему нельзя редактировать строку</param>
    /// <returns>true, если ячейку нельзя редактировать</returns>
    public bool GetRowReadOnly(int rowIndex, out string readOnlyMessage)
    {
      if (Control.ReadOnly)
      {
        readOnlyMessage = Res.EFPDataView_Msg_ReadOnlyView;
        return true;
      }

      if (rowIndex < 0 || rowIndex >= Control.RowCount)
      {
        readOnlyMessage = "Wrong row index";
        return true;
      }

      GetRowInfo(rowIndex, EFPDataViewInfoReason.ReadOnly);
      readOnlyMessage = _RowInfoNeededArgs.ReadOnlyMessage;
      return _RowInfoNeededArgs.ReadOnly;
    }

    /// <summary>
    /// Запрашивает атрибуты строки и ячейки в режиме <see cref="EFPDataViewInfoReason.View"/> и возвращает признак
    /// ContentVisible.
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="columnIndex">Индекс столбца</param>
    /// <returns>false -  если содержимое ячейки не должно отображаться</returns>
    public bool GetCellContentVisible(int rowIndex, int columnIndex)
    {
      GetRowInfo(rowIndex, EFPDataViewInfoReason.View);
      return GetCellInfo(columnIndex).ContentVisible;
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
      string errorText;
      if (DoPasteText(textArray, false, out errorText))
        return true;
      else
      {
        EFPApp.ShowTempMessage(errorText);
        return false;
      }
    }

    /// <summary>
    /// Выполняет вставку текста в прямоугольную область.
    /// Левая верхняя ячейка области определяется текущем выделением ячейки.
    /// Эта перегрузка не выводит сообщение об ошибке, а возвращает его в виде строки.
    /// </summary>
    /// <param name="textArray">Вставляемый текст (строки и столбцы)</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <returns>true, если вставка выполнена</returns>
    public bool PerformPasteText(string[,] textArray, out string errorText)
    {
      return DoPasteText(textArray, false, out errorText);
    }


    /// <summary>
    /// Проверяет возможность вставки текста в прямоугольную область.
    /// Левая верхняя ячейка области определяется текущем выделением ячейки.
    /// </summary>
    /// <param name="textArray">Вставляемый текст (строки и столбцы)</param>
    /// <returns>true, если вставка может быть выполнена</returns>
    public bool TestCanPasteText(string[,] textArray)
    {
      string errorText;
      return TestCanPasteText(textArray, out errorText);
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
        errorText = Res.EFPDataView_Msg_ReadOnlyView;
        return false;
      }

      #endregion

      #region Область вставки

      // Прямоугольный массив a содержит значения для вставки
      int nY1 = textArray.GetLength(0);
      int nX1 = textArray.GetLength(1);

      if (nX1 == 0 || nY1 == 0)
      {
        errorText = Res.EFPDataView_Err_PasteNoText;
        return false; // 27.12.2020
      }

      Rectangle rect = SelectedRectAddress;
      int nX2 = rect.Width;
      int nY2 = rect.Height;
      if (nX2 == 0 || nY2 == 0)
      {
        errorText = Res.EFPDataView_Err_NoSelectedCell;
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

          errorText = String.Format(Res.EFPDataView_Err_PasteMore,
            nY1, nX1, nY2, nX2);
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
          errorText = String.Format(Res.EFPDataView_Err_PasteNoDivided,
            nY1, nX1, nY2, nX2);
          return false;
        }
      }
      else
      {
        rect = new Rectangle(rect.Location, new Size(nX1, nY1));
        if (((rect.Bottom - 1) >= Control.RowCount && (!Control.AllowUserToAddRows)) || (rect.Right - 1) >= Control.ColumnCount)
        {
          /*
          EFPApp.ShowTempMessage("Текст в буфере обмена содержит " +
            RusNumberConvert.IntWithNoun(nY1, "строку", "строки", "строк") + " и " +
            RusNumberConvert.IntWithNoun(nX1, "столбец", "столбца", "столбцов") +
            " - не помещается");
           * */
          errorText = String.Format(Res.EFPDataView_Err_PasteOutIOfBounds,
            nY1, nX1);
          return false;
        }

        if ((!testOnly) && (rect.Bottom - 1) >= Control.RowCount)
        {
          int addCount = rect.Bottom - Control.RowCount + 1;
          //int addCount = Rect.Bottom - Control.RowCount + (Control.AllowUserToAddRows ? 0 : 1); // изм. 11.01.2022
#if DEBUG
          if (addCount <= 0)
            throw new BugException("addCount=" + addCount.ToString());
#endif
          //AddInsuficientRows(addCount);
        }
      }

      // Rect содержит прямоугольник заменяемых значений

      #endregion

      #region Опрос свойства ReadOnly ячеек и возможности вставить значения

      for (int i = rect.Top; i < rect.Bottom; i++)
      {
        if (/*testOnly && */i >= Control.RowCount)
          break;

        for (int j = rect.Left; j < rect.Right; j++)
        {
          DataGridViewCell cell = Control[j, i];
          if (GetCellReadOnly(i, j, out errorText))
          {
            Control.CurrentCell = cell;
            return false;
          }

          int i1 = (i - rect.Top) % nY1;
          int j1 = (j - rect.Left) % nX1;

          if (!TrySetTextValue(cell, textArray[i1, j1], out errorText, true, EFPDataGridViewCellFinishedReason.Paste))
          {
            // убрано 23.04.2025. Иначе после теста не работает "Специальная вставка"
            // Control.CurrentCell = cell;
            EFPDataGridViewColumn col = Columns[cell.ColumnIndex];
            errorText = String.Format(Res.EFPDataView_Err_PasteToColumn, col.DisplayName, errorText);
            return false;
          }
        }
      }

      #endregion

      #region Записываем значение

      if (!testOnly)
      {
        IBindingList bl = ListBindingHelper.GetList(Control.DataSource, Control.DataMember) as IBindingList;

        DataGridViewCell oldCell = Control.CurrentCell;
        bool oldCellInNewRow = Control.AllowUserToAddRows && Control.CurrentCellAddress.Y == (Control.RowCount - 1); // курсор на строке со звездочкой?

        for (int i = rect.Top; i < rect.Bottom; i++)
        {
          int i1 = (i - rect.Top) % nY1;

          if (bl == null)
          {
            if (i >= Control.RowCount)
              Control.RowCount++;
          }
          else
          {
            if (i >= bl.Count)
            {
              bl.AddNew();
            }
          }

          for (int j = rect.Left; j < rect.Right; j++)
          {
            DataGridViewCell cell = Control[j, i];
            int j1 = (j - rect.Left) % nX1;

            SetTextValue(cell, textArray[i1, j1], EFPDataGridViewCellFinishedReason.Paste);
          }
        }

        Control.EndEdit();

        //if (Control.AllowUserToAddRows && rect.Bottom == Control.RowCount && bl != null)
        //{
        //  if (bl.Count == rect.Bottom)
        //    bl.AddNew();  // так сразу добавляется 2 строки
        //}

        // 07.07.2023
        // Для DataView принудительно заканчиваем редактирование.
        try
        {
          /*
          DataView dv = SourceAsDataView;
          if (dv != null)
          {
            DataRowView drv = dv[rect.Bottom - 1];
            if (drv.IsEdit)
              drv.EndEdit();
          }
          */

          // Вариант 2
          if (bl != null)
          {
            IEditableObject item = bl[rect.Bottom - 1] as IEditableObject; // DataRowView
            if (item != null)
              item.EndEdit();
          }
        }
        catch { }

        // 11.01.2022
        // Надо сразу обновить текущую строку, как будто пользователь перешел на другую строку, а потом вернулся обратно.
        // Иначе текущая строка оказывается некомплектной и для нее могут возникать ошибки контроля в EFPInputDataGridView.
        Control.CurrentCell = null;
        if (oldCell != null)
        {
          if (oldCellInNewRow)
          {
            Control.CurrentCell = Control[oldCell.ColumnIndex, Control.RowCount - 1];
            //if (bl!=null)
            //bl.res
          }
          else
            Control.CurrentCell = oldCell;
        }
      }

      #endregion

      errorText = null;
      return true;
    }

    #endregion

    #region События

    /// <summary>
    /// Вызывается для редактирования данных, просмотра, вставки, копирования
    /// и удаления строк
    /// </summary>
    public event EventHandler EditData;

    /// <summary>
    /// Вызывает обработчик события <see cref="EditData"/> и возвращает true, если он установлен
    /// Если метод возвращает false, выполняется редактирование "по месту"
    /// </summary>
    /// <param name="args">Пустой аргумент</param>
    /// <returns>True, если обработчик события был вызван</returns>
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
    /// Вызывает событие <see cref="RefreshData"/>.
    /// Вызывайте этот метод из <see cref="OnRefreshData(EventArgs)"/>, если Вы полностью переопределяете его, не вызывая
    /// метод базового класса.
    /// </summary>
    protected void CallRefreshDataEventHandler(EventArgs args)
    {
      if (RefreshData != null)
        RefreshData(this, args);
    }

    /// <summary>
    /// Непереопределенный метод вызывает событие <see cref="RefreshData"/>.
    /// Если метод переопределен, также должно быть переопределено свойство <see cref="HasRefreshDataHandler"/>
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnRefreshData(EventArgs args)
    {
      CallRefreshDataEventHandler(args);
    }

    /// <summary>
    /// Возвращает true, если есть установленный обработчик события <see cref="RefreshData"/>
    /// </summary>
    public virtual bool HasRefreshDataHandler { get { return RefreshData != null; } }

    bool IEFPDataView.UseRefresh { get { return CommandItems.UseRefresh; } }

    EFPDataViewEnterKeyMode IEFPDataView.EnterKeyMode
    {
      get { return CommandItems.EnterKeyMode; }
      set { CommandItems.EnterKeyMode = value; }
    }

    /// <summary>
    /// Событие вызывается при выполнении метода <see cref="PerformRefresh()"/> после отработки основного события.
    /// В отличие от события <see cref="RefreshData"/>, наличие обработчика этого события не влияет на <see cref="HasRefreshDataHandler"/> и на наличие команды "Обновить".
    /// </summary>
    public event EventHandler AfterRefreshData;

    /// <summary>
    /// Вызывает обработчик события <see cref="AfterRefreshData"/>
    /// </summary>
    /// <param name="args">Фиктивные аргументы события</param>
    protected virtual void OnAfterRefreshData(EventArgs args)
    {
      if (AfterRefreshData != null)
        AfterRefreshData(this, args);
    }

    /// <summary>
    /// Принудительное обновление просмотра.
    /// Обновление выполняется, даже если <see cref="EFPDataGridViewCommandItems.UseRefresh"/>=false.
    /// Сохраняется и восстанавливается, если возможно, текущее выделение в просмотре с помощью свойства <see cref="Selection"/>
    /// </summary>
    public virtual void PerformRefresh()
    {
      if (Control.IsDisposed)
        return; // 27.03.2018
      EFPDataGridViewSelection oldSel = null;
      if (ProviderState == EFPControlProviderState.Attached) // 04.07.2021
        oldSel = Selection;


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

      if (oldSel != null)
      {
        try
        {
          Selection = oldSel;
        }
        catch (Exception e)
        {
          EFPApp.ErrorMessageBox(String.Format(Res.EFPDataView_Err_RefreshRestoreSelection, e.Message));
        }
      }

      OnAfterRefreshData(EventArgs.Empty);
    }

    /// <summary>
    /// Вызов метода <see cref="PerformRefresh()"/>, который можно выполнять из обработчика события
    /// </summary>
    /// <param name="sender">Игнорируется</param>
    /// <param name="args">Игнорируется</param>
    public void PerformRefresh(object sender, EventArgs args)
    {
      PerformRefresh();
    }

    #endregion

    #region Методы

    /// <summary>
    /// Перевод просмотра в один из режимов
    /// </summary>
    public void PerformEditData(UIDataState state)
    {
      if (_InsideEditData)
      {
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_EditNotFinished);
        return;
      }
      // Любая операция редактирования останавливает поиск по первым буквам
      CurrentIncSearchColumn = null;

      bool res;
      // Пытаемся вызвать специальный обработчик в GridProducerColum
      if (CurrentColumn != null)
      {
        if (CurrentColumn.ColumnProducer != null)
        {
          _InsideEditData = true;
          _State = state;
          try
          {
            EFPDataViewRowValues rowValues = this.GetRowValues(Control.CurrentCellAddress.Y);
            res = CurrentColumn.ColumnProducer.PerformCellEdit(rowValues, CurrentColumnName);
            FreeRowValues(rowValues);
          }
          finally
          {
            _State = UIDataState.View;
            _InsideEditData = false;
          }
          if (res)
            return;
        }
      }

      _InsideEditData = true;
      _State = state;
      try
      {
        res = OnEditData(EventArgs.Empty);
      }
      finally
      {
        _State = UIDataState.View;
        _InsideEditData = false;
      }

      if (!res)
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
    /// невозможно, вызывается исключение.
    /// </summary>
    /// <param name="cell">Ячейка просмотра</param>
    /// <param name="textValue">Записываемое значение</param>
    /// <param name="reason">Причина</param>
    public void SetTextValue(DataGridViewCell cell, string textValue,
        EFPDataGridViewCellFinishedReason reason)
    {
      string errorText;
      if (!TrySetTextValue(cell, textValue, out errorText, false, reason))
        throw new InvalidOperationException(String.Format(Res.EFPDataView_Err_SetTextValue, textValue, errorText));
    }

    /// <summary>
    /// Попытка присвоить текстовое значение ячейке.
    /// Операция считается выполнимой, даже если ячейка имеет свойство ReadOnly.
    /// Следует также выполнять проверку методом <see cref="GetCellReadOnly(int, int, out string)"/>.
    /// Выполняется преобразование в нужный тип данных и проверка корректности
    /// (соответствие маске, если она задана для столбца). Если преобразование
    /// невозможно, возвращается false и текст сообщения об ошибке.
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

      Type valueType;
      if (cell.ValueType != null)
        valueType = cell.ValueType;
      else
        valueType = typeof(string);

      IMaskProvider maskProvider = Columns[cell.ColumnIndex].MaskProvider;

      if (MathTools.IsFloatType(valueType))
        UITools.CorrectNumberString(ref textValue);
      else if (valueType == typeof(string) && maskProvider != null)
      {
        if (!maskProvider.Test(textValue, out errorText))
          return false;
      }

      object v;
      try
      {
        v = Convert.ChangeType(textValue, valueType);
      }
      catch
      {
        errorText = UITools.ConvertErrorMessage(textValue, valueType); // 23.04.2025
        return false;
      }

      try
      {
        if (!testOnly)
        {
          if (valueType == typeof(DateTime))
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

    /// <summary>
    /// Возвращает объект, привязанный к строке данных.
    /// Аналогично обращению к DataGridView.Rows[rowIndex].DataBoundItem, но при этом строка не становится Unshared.
    /// Если просмотр не привязян к набору данных, возвращается null.
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <returns>Объект, привязанный к строке или null</returns>
    public object GetDataBoundItem(int rowIndex)
    {
      System.Collections.IList list = ListBindingHelper.GetList(Control.DataSource, Control.DataMember) as System.Collections.IList;
      if (list == null)
        return null;
      if (rowIndex < 0 || rowIndex >= list.Count || rowIndex == Control.NewRowIndex)
        return null;
      return list[rowIndex];
    }

    /// <summary>
    /// Возвращает позицию для будущего выпадающего блока диалога, который будет показан для редактирования ячейки.
    /// В возвращаемом объекте устанавливается свойство <see cref="EFPDialogPosition.PopupOwnerBounds"/>.
    /// Если нет текущей ячейки (просмотр пустой) или текущая ячейка прокручены мышью за пределы видимой области просмотра,
    /// возвращается неинициализированный <see cref="EFPDialogPosition.PopupOwnerBounds"/>.
    /// </summary>
    public EFPDialogPosition CurrentPopupPosition
    {
      get
      {
        Point pt = Control.CurrentCellAddress;
        if (pt.X < 0 || pt.Y < 0)
          return new EFPDialogPosition();
        Rectangle rc = Control.GetCellDisplayRectangle(pt.X, pt.Y, true);
        if (rc.Width <= 0 || rc.Height <= 0)
          return new EFPDialogPosition();
        EFPDialogPosition res = new EFPDialogPosition();
        res.PopupOwnerBounds = Control.RectangleToScreen(rc);
        return res;
      }
    }

    #endregion

    #region Обработка события DataError

    /// <summary>
    /// Класс исключения для отладки перехваченной ошибки в событии DataGridView.DataError
    /// </summary>
    [Serializable]
    private class DataGridViewDataErrorException : ApplicationException
    {
      #region Конструктор

      public DataGridViewDataErrorException(DataGridView control, DataGridViewDataErrorEventArgs args)
        : base(String.Format(Res.EFPDataGridView_Err_DataErrorException, args.Context), args.Exception)
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
    /// Использовать ли обработку события <see cref="DataGridView.DataError"/> по умолчанию.
    /// Если свойство установлено в true, то обработчик события выдает отладочное окно
    /// и выводит в log-файл специальный тип исключения о возникшей ошибке.
    /// Если свойство установлено в false, то никакие действия не выполняются и
    /// сообщения об ошибке не выдается (включая стандартное). При этом предполагается,
    /// что имеется дополнительный пользовательский обработчик события <see cref="DataGridView.DataError"/>,
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
        EFPApp.ShowException(e);
      }
    }

    private bool _FormatDisplayErrorWasShown = false;

    /// <summary>
    /// Выдача сообщения об ошибке в обработчике <see cref="DataGridView.DataError"/> 
    /// </summary>
    /// <param name="sender">Объект <see cref="DataGridView"/></param>
    /// <param name="args">Аргументы события</param>
    public void DefaultShowDataError(object sender, DataGridViewDataErrorEventArgs args)
    {
      DataGridView control = (DataGridView)sender;
      if ((args.Context & (DataGridViewDataErrorContexts.Commit | DataGridViewDataErrorContexts.Parsing)) ==
        (DataGridViewDataErrorContexts.Commit | DataGridViewDataErrorContexts.Parsing))
      {
        // Введено неправильное значение в поле. Вместо большого сообщения об ошибке
        // выдаем всплывающее сообщение внизу экрана
        DataGridViewCell cell = control.Rows[args.RowIndex].Cells[args.ColumnIndex];
        EFPApp.ShowTempMessage(String.Format(Res.EFPDataGridView_Err_DataErrorParsing, cell.EditedFormattedValue) + "\"");
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
        EFPApp.ShowException(e1, String.Format(Res.EFPDataGridView_Err_DataErrorFormatting, args.RowIndex, args.ColumnIndex));
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
        EFPApp.ShowException(e);
      }
    }
    void DoControl_CellClick(DataGridViewCellEventArgs args)
    {
      EFPApp.ShowTempMessage(null);
      if (args.ColumnIndex < 0 || args.RowIndex < 0)
        return;
      EFPDataGridViewColumn Column = Columns[args.ColumnIndex];
      if (Column.ColumnProducer == null)
        return;
      EFPDataViewRowValues rowValues = GetRowValues(args.RowIndex);
      Column.ColumnProducer.PerformCellClick(rowValues, Column.Name);
      FreeRowValues(rowValues);
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

      /// <summary>
      /// Использование полосатой раскраски, при которой каждая вторая строка имеет цвет <see cref="UIDataViewColorType.Alter"/>.
      /// Свойство является амбьентным. Если значение не установлено в явном виде, возвращается глобальное значение
      /// <see cref="EFPApp.UseAlternation"/>.
      /// </summary>
    public bool UseAlternation
    {
      get { return _UseAlternation ?? EFPApp.UseAlternation; }
      set
      {
        _UseAlternation = value;
        Control.Invalidate();
      }
    }
    private bool? _UseAlternation;

    /// <summary>
    /// Сброс амбьентного свойства <see cref="UseAlternation"/>
    /// </summary>
    public void ResetUseAlternation()
    {
      _UseAlternation = null;
      Control.Invalidate();
    }

    #region Событие RowInfoNeeded

    /// <summary>
    /// Событие вызывается перед рисованием строки и должно устанавливать общие атрибуты
    /// для ячеек строки.
    /// Если будет использоваться вывод сообщений об ошибках по строке, то должно быть установлено свойство
    /// <see cref="UseRowImages"/>.
    /// </summary>
    public event EFPDataGridViewRowInfoEventHandler RowInfoNeeded;

    /// <summary>
    /// Вызов события <see cref="RowInfoNeeded"/>.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnRowInfoNeeded(EFPDataGridViewRowInfoEventArgs args)
    {
      if (RowInfoNeeded != null)
        RowInfoNeeded(this, _RowInfoNeededArgs);
    }


    /// <summary>
    /// Постоянно используем единственный объект аргумента для событий
    /// </summary>
    private EFPDataGridViewRowInfoEventArgs _RowInfoNeededArgs;

    /// <summary>
    /// Общедоступный метод получения атрибутов строки
    /// </summary>
    /// <param name="rowIndex">Индекс строки</param>
    /// <param name="reason"></param>
    /// <returns>Атрибуты строки</returns>
    public EFPDataGridViewRowInfoEventArgs GetRowInfo(int rowIndex, EFPDataViewInfoReason reason)
    {
      _RowInfoNeededArgs.InitRow(rowIndex, reason);
      OnRowInfoNeeded(_RowInfoNeededArgs);
      return _RowInfoNeededArgs;
    }

    #endregion

    #region Событие CellInfoNeeded

    /// <summary>
    /// Событие вызывается перед рисованием ячейки и должно выполнить форматирование
    /// значения и установить цветовые атрибуты ячейки
    /// </summary>
    public event EFPDataGridViewCellInfoEventHandler CellInfoNeeded;

    /// <summary>
    /// Вызывает событие <see cref="CellInfoNeeded"/>.
    /// Этот метод должен вызываться только после предварительного вызова <see cref="OnRowInfoNeeded(EFPDataGridViewRowInfoEventArgs)"/> (возможно,
    /// несколько раз для разных столбцов), в котором указываются индекс строки и причина вызова.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnCellInfoNeeded(EFPDataGridViewCellInfoEventArgs args)
    {
      if (CellInfoNeeded != null)
        CellInfoNeeded(this, _CellInfoNeededArgs);
    }

    private bool _Inside_CallOnCellInfoNeeded; // 06.02.2018 - блокировка вложенного вызова

    private void CallOnCellInfoNeeded(int columnIndex, DataGridViewCellStyle cellStyle, bool styleIsTemplate, object originalValue)
    {
      if (_Inside_CallOnCellInfoNeeded)
        return;
      _Inside_CallOnCellInfoNeeded = true;
      try
      {
        _CellInfoNeededArgs.InitCell(_RowInfoNeededArgs, columnIndex, cellStyle, styleIsTemplate, originalValue);
        OnCellInfoNeeded(_CellInfoNeededArgs);
      }
      finally
      {
        _Inside_CallOnCellInfoNeeded = false;
      }
    }

    private EFPDataGridViewCellInfoEventArgs _CellInfoNeededArgs;

    /// <summary>
    /// Общедоступный метод получения атрибутов ячейки.
    /// Перед вызовами метода должен идти один вызов <see cref="GetRowInfo(int, EFPDataViewInfoReason)"/> для строки
    /// </summary>
    /// <param name="columnIndex">Номер столбца</param>
    /// <returns>Атрибуты ячейки</returns>
    public EFPDataGridViewCellInfoEventArgs GetCellInfo(int columnIndex)
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

      DataGridViewCell cell = Control[columnIndex, _RowInfoNeededArgs.RowIndex];

      // 30.06.2021
      // Если строкое значение получено из DataRow, которая была загружена из базы данных, 
      // то значение может содержать концевые пробелы
      string sValue = cell.Value as String;
      if (sValue != null)
        _CellInfoNeededArgs.Value = sValue.TrimEnd();
      else
        _CellInfoNeededArgs.Value = cell.Value;
      _CellInfoNeededArgs.FormattingApplied = false;

      CallOnCellInfoNeeded(columnIndex, cell.InheritedStyle, true, _CellInfoNeededArgs.Value);

      if (_CellInfoNeededArgs.Reason == EFPDataViewInfoReason.View ||
        _CellInfoNeededArgs.Reason == EFPDataViewInfoReason.Print) // 06.02.2018
      {
        _CellInfoNeededArgs.MoveTemplateToStyle();

        // Убрано 25.06.2021.
        // Непонятно, зачем было нужно. Было всегда (в 2008 году, в программе cs1, Modules\ClientAccDep\GridHandler.cs).

        // Если значение возвращается из EFPGridProducerColumn, то не устанавливается свойство EFPDataGridViewCellAttributesEventArgs.FormattingApplied.
        // Неприятность возникает после экспорта в Excel/Calc. Если для столбца задано форматирование с разделителем тысяч ("#,##0"), то
        // в Excel попадает форматированное значение-строка, которое не преобразуется в число из-за пробелов.

        //if (!_GetCellAttributesArgs.FormattingApplied)
        //  _GetCellAttributesArgs.Value = Cell.FormattedValue;
      }
      return _CellInfoNeededArgs;
    }

    #endregion

    /// <summary>
    /// Устанавливается в true в событии DataGridView.RowPrePaint и сбрасывается в RowPostPaint
    /// </summary>
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
          EFPApp.ShowException(e);
          _RowPrePaintErrorMessageWasShown = true;
        }
      }
    }
    void DoControl_RowPrePaint(DataGridViewRowPrePaintEventArgs args)
    {
      if (!_InsideRowPaint)
      {
        _InsideRowPaint = true;
        GetRowInfo(args.RowIndex, EFPDataViewInfoReason.View);

        // 15.09.2015
        // Для невиртуального табличного просмотра событие CellToolTipTextNeeded не вызывается.
        // Соответственно, не появится всплывающая подсказка по ошибке для строки.
        // Для такого просмотра устанавливаем ее в явном виде
        if ((!Control.VirtualMode) && Control.DataSource == null && UseRowImages)
        {
          Control.Rows[args.RowIndex].HeaderCell.ToolTipText = _RowInfoNeededArgs.ToolTipText;
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
          DataGridViewPaintParts pp = args.PaintParts;
          if (args.CellStyle.ForeColor == Color.Transparent)
            pp &= ~(DataGridViewPaintParts.ContentForeground | DataGridViewPaintParts.ContentBackground);
          args.Paint(args.ClipBounds, pp);

          if (_CellInfoNeededArgs.DiagonalUpBorder >= UIDataViewBorderStyle.Thin)
            args.Graphics.DrawLine(CreateBorderPen(args.CellStyle.ForeColor, _CellInfoNeededArgs.DiagonalUpBorder),
              args.CellBounds.Left, args.CellBounds.Bottom - 1, args.CellBounds.Right - 1, args.CellBounds.Top);
          if (_CellInfoNeededArgs.DiagonalDownBorder >= UIDataViewBorderStyle.Thin)
            args.Graphics.DrawLine(CreateBorderPen(args.CellStyle.ForeColor, _CellInfoNeededArgs.DiagonalDownBorder),
              args.CellBounds.Left, args.CellBounds.Top, args.CellBounds.Right - 1, args.CellBounds.Bottom - 1);

          args.Handled = true;
        }
      }
      catch (Exception e)
      {
        if (!_RowPrePaintErrorMessageWasShown)
        {
          EFPApp.ShowException(e);
          _RowPrePaintErrorMessageWasShown = true;
        }
      }
    }

    private static Pen CreateBorderPen(Color color, UIDataViewBorderStyle borderStyle)
    {
      float w;
      if (borderStyle == UIDataViewBorderStyle.Thick)
        w = 2;
      else
        w = 1;
      return new Pen(color, w);
    }

    private bool _Control_CellFormatting_Logged;

    void Control_CellFormatting(object sender, DataGridViewCellFormattingEventArgs args)
    {
      if (args.RowIndex < 0)
        return;
      if (args.ColumnIndex < 0)
        return;
      try
      {
        if (!_InsideRowPaint)
          GetRowInfo(args.RowIndex, EFPDataViewInfoReason.View); // 25.10.2022

        // Если таблица получена из базы данных, то у строковых полей часто идут пробелы для заполнения по ширине поля.
        // Если их не убрать, то значение "не влезет" в ячейку и будут символы "..." в правой части столбца
        // 22.04.2021
        // В отличие от предыдущей реализации, думаю, можно не выполнять лишние проверки.
        // Вряд ли где-нибудь могут понадобиться концевые пробелы в строке.
        string sValue = args.Value as String;
        if (sValue != null)
          args.Value = sValue.TrimEnd();

        EFPDataGridViewColumn colInfo = this.Columns[args.ColumnIndex];

        // Форматирование значения и получение цветовых атрибутов
        _CellInfoNeededArgs.Value = args.Value;
        _CellInfoNeededArgs.FormattingApplied = args.FormattingApplied;
        CallOnCellInfoNeeded(args.ColumnIndex, args.CellStyle, false, args.Value);
        args.Value = _CellInfoNeededArgs.Value;
        args.FormattingApplied = _CellInfoNeededArgs.FormattingApplied;
        // Установка цветов в просмотре
        SetCellAttr(args.CellStyle, _CellInfoNeededArgs.ColorType, _CellInfoNeededArgs.Grayed, _CellInfoNeededArgs.ContentVisible);

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
        if (_CellInfoNeededArgs.IndentLevel > 0)
        {
          Padding pd = args.CellStyle.Padding;
          switch (args.CellStyle.Alignment)
          {
            case DataGridViewContentAlignment.TopLeft:
            case DataGridViewContentAlignment.MiddleLeft:
            case DataGridViewContentAlignment.BottomLeft:
              pd.Left += (int)(Measures.CharWidthPixel * 2 * _CellInfoNeededArgs.IndentLevel);
              break;
            case DataGridViewContentAlignment.TopRight:
            case DataGridViewContentAlignment.MiddleRight:
            case DataGridViewContentAlignment.BottomRight:
              pd.Right += (int)(Measures.CharWidthPixel * 2 * _CellInfoNeededArgs.IndentLevel);
              break;
          }
          args.CellStyle.Padding = pd;
        }
      }
      catch (Exception e)
      {
        if (!_Control_CellFormatting_Logged)
        {
          _Control_CellFormatting_Logged = true;
          LogoutTools.LogoutException(e);
        }
        EFPApp.ShowTempMessage(String.Format(Res.EFPDataView_Err_CellFormatting, e.Message));
      }
    }

    #region Статические методы SetCellAttr

    /// <summary>
    /// Получить цвет фона и шрифта для заданных параметров ячейки.
    /// Цвета помещаются в объект <see cref="DataGridViewCellStyle"/>.
    /// </summary>
    /// <param name="cellStyle">Заполняемый объект</param>
    /// <param name="colorType">Вариант цветного оформления ячейки</param>
    public static void SetCellAttr(DataGridViewCellStyle cellStyle, UIDataViewColorType colorType)
    {
      SetCellAttr(cellStyle, colorType, false, true);
    }

    /// <summary>
    /// Получить цвет фона и шрифта для заданных параметров ячейки.
    /// Цвета помещаются в объект <see cref="DataGridViewCellStyle"/>.
    /// </summary>
    /// <param name="cellStyle">Заполняемый объект</param>
    /// <param name="colorType">Вариант цветного оформления ячейки</param>
    /// <param name="grayed">true -Выделение серым цветом</param>
    public static void SetCellAttr(DataGridViewCellStyle cellStyle, UIDataViewColorType colorType, bool grayed)
    {
      SetCellAttr(cellStyle, colorType, grayed, true);
    }

    /// <summary>
    /// Получить цвет фона и шрифта для заданных параметров ячейки.
    /// Цвета помещаются в объект <see cref="DataGridViewCellStyle"/>.
    /// </summary>
    /// <param name="cellStyle">Заполняемый объект</param>
    /// <param name="colorType">Вариант цветного оформления ячейки</param>
    /// <param name="grayed">true -Выделение серым цветом</param>
    /// <param name="contentVisible">true - должно ли быть видно содержимое ячейки.
    /// Если false, то в <paramref name="cellStyle"/>.ForeColor помещается <see cref="Color.Transparent"/></param>
    public static void SetCellAttr(DataGridViewCellStyle cellStyle, UIDataViewColorType colorType, bool grayed, bool contentVisible)
    {
      //if (ColorType == EFPDataGridViewColorType.Alter)
      //{
      //  // !!! Нужно сделать исходя из текущего цвета ColorWindow
      //  CellStyle.BackColor = ColorEx.FromArgb(244,244,244); 
      //}
      Color backColor, foreColor;
      SetCellAttr(colorType, grayed, contentVisible, out backColor, out foreColor);
      if (!backColor.IsEmpty)
        cellStyle.BackColor = backColor;
      if (!foreColor.IsEmpty)
        cellStyle.ForeColor = foreColor;
    }

    /// <summary>
    /// Получить цвет фона и шрифта для заданных параметров ячейки.
    /// Статический метод, не привязанный к табличному просмотру и объекту <see cref="DataGridViewCellStyle"/>.
    /// </summary>
    /// <param name="colorType">Вариант цветного оформления ячейки</param>
    /// <param name="grayed">true -Выделение серым цветом</param>
    /// <param name="contentVisible">true - должно ли быть видно содержимое ячейки.
    /// Если false, то в <paramref name="foreColor"/> помещается <see cref="Color.Transparent"/></param>
    /// <param name="backColor">Сюда помещается цвет фона, зависящий от <paramref name="colorType"/>.
    /// Для <paramref name="colorType"/>=Normal записывается <see cref="Color.Empty"/></param>
    /// <param name="foreColor">Сюда помещается цвет текста, зависящий от <paramref name="colorType"/>,
    /// <paramref name="grayed"/> и <paramref name="contentVisible"/>.</param>
    public static void SetCellAttr(UIDataViewColorType colorType, bool grayed, bool contentVisible, out Color backColor, out Color foreColor)
    {
      backColor = Color.Empty;
      foreColor = Color.Empty;
      switch (colorType)
      {
        case UIDataViewColorType.Header:
          backColor = EFPApp.Colors.GridHeaderBackColor;
          foreColor = EFPApp.Colors.GridHeaderForeColor;
          //CellStyle.Font.Style =FontStyle.Bold | FontStyle.Underline;
          break;
        case UIDataViewColorType.Alter:
          backColor = EFPApp.Colors.GridAlterBackColor;
          break;
        case UIDataViewColorType.Special:
          backColor = EFPApp.Colors.GridSpecialBackColor;
          break;
        case UIDataViewColorType.Total1:
          backColor = EFPApp.Colors.GridTotal1BackColor;
          break;
        case UIDataViewColorType.Total2:
          backColor = EFPApp.Colors.GridTotal2BackColor;
          break;
        case UIDataViewColorType.TotalRow:
          backColor = EFPApp.Colors.GridTotalRowBackColor;
          foreColor = EFPApp.Colors.GridTotalRowForeColor;
          break;
        case UIDataViewColorType.Error:
          backColor = EFPApp.Colors.GridErrorBackColor;
          foreColor = EFPApp.Colors.GridErrorForeColor;
          break;
        case UIDataViewColorType.Warning:
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
    /// ячеек не выполняется (аналогично свойству у <see cref="TextBox"/>, <see cref="TreeView"/> и др.)
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
    /// Возвращает имя изображения в списке <see cref="EFPApp.MainImages"/>, соответствующее <see cref="UIDataViewImageKind"/>.
    /// Для <paramref name="imageKind"/>=None возвращает "EmptyImage".
    /// </summary>
    /// <param name="imageKind">Вариант изображения</param>
    /// <returns>Имя изображения</returns>
    public static string GetImageKey(UIDataViewImageKind imageKind)
    {
      return GetImageKey(imageKind, "EmptyImage");
    }

    /// <summary>
    /// Возвращает имя изображения в списке <see cref="EFPApp.MainImages"/>, соответствующее <see cref="UIDataViewImageKind"/>.
    /// </summary>
    /// <param name="imageKind">Вариант изображения</param>
    /// <param name="noneImageKey">Имя изображения для <paramref name="imageKind"/>=None</param>
    /// <returns>Имя изображения</returns>
    public static string GetImageKey(UIDataViewImageKind imageKind, string noneImageKey)
    {
      switch (imageKind)
      {
        case UIDataViewImageKind.Error: return "Error";
        case UIDataViewImageKind.Warning: return "Warning";
        case UIDataViewImageKind.Information: return "Information";
        case UIDataViewImageKind.None: return noneImageKey;
        default: return "UnknownState";
      }
    }

    /// <summary>
    /// Разрешить рисование изображений в заголовках строк и вывод всплывающих
    /// подсказок. При установленном свойстве не будут правильно отображаться 
    /// значки и подсказки об ошибках в строках с помощью <see cref="DataRow.RowError"/>.
    /// По умолчанию - false (можно использовать стандартный механизм индикации в
    /// <see cref="DataGridView"/>).
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
    /// Использовать свойство <see cref="DataRow.RowError"/> для отображения признака ошибки в 
    /// заголовке строки и подсказки, как это по умолчанию делает <see cref="DataGridView"/>.
    /// Свойство имеет значение при установленном <see cref="UseRowImages"/>=true.
    /// По умолчанию - false (предполагается, что признак ошибки строки 
    /// устанавливается обработчиком события <see cref="RowInfoNeeded"/>).
    /// Установка свойства не препятствует реализации <see cref="RowInfoNeeded"/>
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
    /// Изображение, которое следует вывести в верхней левой ячейке просмотра.
    /// Произвольное изображение может быть задано с помощью свойства <see cref="TopLeftCellUserImage"/>.
    /// Свойство <see cref="UseRowImages"/> должно быть установлено.
    /// Свойство можно устанавливать из прикладного кода только при <see cref="ShowErrorCountInTopLeftCell"/>=false,
    /// в противном случае оно устанавливается автоматически.
    /// Если одновременно заданы свойства <see cref="TopLeftCellUserImage"/> и <see cref="TopLeftCellImageKind"/>!=<see cref="UIDataViewImageKind.None"/>,
    /// то приоритет имеет <see cref="TopLeftCellUserImage"/>.
    /// </summary>
    [DefaultValue(UIDataViewImageKind.None)]
    public UIDataViewImageKind TopLeftCellImageKind
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
    private UIDataViewImageKind _TopLeftCellImageKind;

    /// <summary>
    /// Произвольное изображение, которое следует вывести в верхней левой ячейке просмотра.
    /// Свойство <see cref="UseRowImages"/> должно быть установлено.
    /// Если одновременно заданы свойства <see cref="TopLeftCellUserImage"/> и <see cref="TopLeftCellImageKind"/>!=<see cref="UIDataViewImageKind.None"/>,
    /// то приоритет имеет <see cref="TopLeftCellUserImage"/>.
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
    /// Свойство инициализируется автоматически, если <see cref="UseRowImages"/> установлено в true.
    /// Свойство не включает в себя текст, генерируемый <see cref="ShowRowCountInTopLeftCell"/>
    /// </summary>
    public string TopLeftCellToolTipText
    {
      get { return Control.TopLeftHeaderCell.ToolTipText; }
      set
      {
        if (value == null)
          value = String.Empty;
        if (_TopLeftCellToolTipText == value)
          return;
        _TopLeftCellToolTipText = value;

        InitTopLeftHeaderCellToolTipText();
      }
    }
    private string _TopLeftCellToolTipText;

    private string _RowCountTopLeftCellToolTipText;
    private string _ErrorCountTopLeftCellToolTipText;

    private void InitTopLeftHeaderCellToolTipText()
    {
      string[] a = new string[3]
      {
        _TopLeftCellToolTipText,
        _RowCountTopLeftCellToolTipText,
        _ErrorCountTopLeftCellToolTipText
      };
      Control.TopLeftHeaderCell.ToolTipText = StringTools.JoinNotEmptyStrings(Environment.NewLine, a);

    }

    /// <summary>
    /// Если свойство установить в true, то в верхней левой ячейке будет отображаться всплывающая
    /// подсказка "Строк в просмотре: XXX". 
    /// По умолчанию - false - свойство отключено.
    /// </summary>
    public bool ShowRowCountInTopLeftCell
    {
      get { return _ShowRowCountInTopLeftCell; }
      set
      {
        if (value == _ShowRowCountInTopLeftCell)
          return;
        _ShowRowCountInTopLeftCell = value;

        _ValidateRequiredFlag = true; // обновляем подсказку
      }
    }
    private bool _ShowRowCountInTopLeftCell;

    /// <summary>
    /// Если свойство установить в true, то в верхней левой ячейке будет отображаться всплывающая
    /// подсказка "Строк с ошибками: XXX, предупрежденями YYY". 
    /// Также будет выводиться значок установкой свойства <see cref="TopLeftCellImageKind"/>.
    /// Свойство действует только при <see cref="UseRowImages"/>=true.
    /// По умолчанию - false - свойство отключено.
    /// </summary>
    public bool ShowErrorCountInTopLeftCell
    {
      get { return _ShowErrorCountInTopLeftCell; }
      set
      {
        if (value == _ShowErrorCountInTopLeftCell)
          return;
        _ShowErrorCountInTopLeftCell = value;

        _ValidateRequiredFlag = true; // обновляем подсказку
      }
    }
    private bool _ShowErrorCountInTopLeftCell;


    private void DoControl_RowHeaderCellPainting(DataGridViewCellPaintingEventArgs args)
    {
      if (!_UseRowImages)
        return;
      try
      {
        if (args.RowIndex < 0)
          DoControl_RowHeaderCellPainting(args, _TopLeftCellImageKind, _TopLeftCellUserImage);
        else
          DoControl_RowHeaderCellPainting(args, _RowInfoNeededArgs.ImageKind, _RowInfoNeededArgs.UserImage);
      }
      catch
      {
        DoControl_RowHeaderCellPainting(args, UIDataViewImageKind.Error, null);
      }
    }

    private void DoControl_RowHeaderCellPainting(DataGridViewCellPaintingEventArgs args, UIDataViewImageKind imageKind, Image userImage)
    {
      args.PaintBackground(args.ClipBounds, false);
      args.PaintContent(args.ClipBounds);

      if (userImage == null && imageKind != UIDataViewImageKind.None)
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
    /// <see cref="RowInfoNeeded"/> или значение свойства <see cref="TopLeftCellToolTipText"/></returns>
    public string GetRowToolTipText(int rowIndex)
    {
      if (rowIndex < 0)
        return TopLeftCellToolTipText;
      else
        return GetRowInfo(rowIndex, EFPDataViewInfoReason.ToolTip).ToolTipText;
    }

    /// <summary>
    /// Получить текст подсказки для ячейки
    /// </summary>
    /// <param name="rowIndex">Индекс строки или (-1) для заголовков столбцов</param>
    /// <param name="columnIndex">Индекс столбца или (-1) для заголовков строк</param>
    /// <returns>Текст всплывающей подсказки</returns>
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
          sText = UITools.ToYesNo(DataTools.GetBoolean(v));
        }
        else
          sText = String.Empty;

        // 2. Подсказка, добавляемая обработчиком столбца
        col.CallCellToolTextNeeded(rowIndex, ref sText);
        if (!String.IsNullOrEmpty(sText))
        {
          string sHead = col.GridColumn.ToolTipText;
          if (String.IsNullOrEmpty(sHead) && Control.ColumnHeadersVisible)
          {
            sHead = col.GridColumn.HeaderText;
            if (String.IsNullOrEmpty(sHead))
              sHead = Res.EFPDataView_Name_NoNameColumn;
          }
          if (!String.IsNullOrEmpty(sHead)) // 26.09.2025
            sText = sHead + ": " + sText;
        }
      }

      // 3. Пользовательский обработчик
      GetRowInfo(rowIndex, EFPDataViewInfoReason.ToolTip);
      _CellInfoNeededArgs.ToolTipText = sText;
      GetCellInfo(columnIndex);

      if (_CellInfoNeededArgs.ContentVisible || _CellInfoNeededArgs.ToolTipTextHasBeenSet)
        return _CellInfoNeededArgs.ToolTipText;
      else
        return String.Empty; // 12.01.2022
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
        args.ToolTipText = String.Format(Res.EFPDataView_ToolTip_Exception, e.Message);
      }
    }


    /// <summary>
    /// Перейти к следующей или предыдущей строке с ошибкой.
    /// Используется для реализации соответствующих команд локального меню.
    /// </summary>
    /// <param name="fromTableBegin">true - перейти к первой или последней строке в таблице с подходящим условием,
    /// false - перейти к следующей или предыдущей относительно текущей строки</param>
    /// <param name="forward">true - перейти вперед (в сторону конца таблицы), false - назад (к началу)</param>
    /// <param name="imageKind">Какую строку найти (условие поиска)</param>
    public void GotoNextErrorRow(bool fromTableBegin, bool forward, UIDataViewImageKind imageKind)
    {
      if (!UseRowImages)
      {
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_RowErrNavNotSuppoprted);
        return;
      }

      if (Control.RowCount == 0)
      {
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_Empty);
        return;
      }

      int startRowIndex;
      if (fromTableBegin)
        startRowIndex = -1;
      else
        startRowIndex = CurrentRowIndex;
      int newRowIndex = FindErrorRow(startRowIndex, forward, imageKind);
      if (newRowIndex < 0)
      {
        string msg1 = forward ? Res.EFPDataView_Msg_RowErrNavTableBottom : Res.EFPDataView_Msg_RowErrNavTableTop;
        string msg2;
        switch (imageKind)
        {
          case UIDataViewImageKind.Error: msg2 = Res.EFPDataView_Msg_RowErrNav_Error; break;
          case UIDataViewImageKind.Warning: msg2 = Res.EFPDataView_Msg_RowErrNav_Warning; break;
          case UIDataViewImageKind.Information: msg2 = Res.EFPDataView_Msg_RowErrNav_Information; break;
          case UIDataViewImageKind.None: msg2 = Res.EFPDataView_Msg_RowErrNav_None; break;
          default:
            throw ExceptionFactory.ArgUnknownValue("imageKind", imageKind);
        }
        EFPApp.ShowTempMessage(String.Format(Res.EFPDataView_Msg_RowErrNav, msg1, msg2));
        return;
      }

      CurrentRowIndex = newRowIndex;
    }

    /// <summary>
    /// <para>
    /// Найти строку, содержащую признак ошибки.
    /// Используются следующие критерии поиска в зависимости от аргумента <paramref name="imageKind"/>:
    /// </para>
    /// <list type="bullet">
    /// <item><description><see cref="UIDataViewImageKind.None"/> - Строки без ошибок, предупреждений и информационных сообщений</description></item>
    /// <item><description><see cref="UIDataViewImageKind.Error"/> - Строки с ошибками</description></item>
    /// <item><description><see cref="UIDataViewImageKind.Warning"/> - Строки с ошибками и предупреждениями</description></item>
    /// <item><description><see cref="UIDataViewImageKind.Information"/> - Строки с ошибками, предупреждениями и информационными сообщениями</description></item>
    /// </list>
    /// </summary>
    /// <param name="startRowIndex">С какой строки начать поиск. Значение (-1) указывает на поиск с начала (с конца) таблицы</param>
    /// <param name="forward">true-искать вперед к концу таблицы, false - искать назад к началу таблицы</param>
    /// <param name="imageKind">Какую строку найти (условие поиска). 
    /// </param>
    /// <returns>Индекс найденной строки или (-1), если строка не найдена</returns>
    public int FindErrorRow(int startRowIndex, bool forward, UIDataViewImageKind imageKind)
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

    private bool TestRowImageKind(int rowIndex, UIDataViewImageKind imageKind)
    {
      UIDataViewImageKind thisKind = GetRowImageKind(rowIndex);
      switch (imageKind)
      {
        case UIDataViewImageKind.None:
          return thisKind == UIDataViewImageKind.None;
        case UIDataViewImageKind.Information:
          return thisKind != UIDataViewImageKind.None;
        case UIDataViewImageKind.Warning:
          return thisKind == UIDataViewImageKind.Warning || thisKind == UIDataViewImageKind.Error;
        case UIDataViewImageKind.Error:
          return thisKind == UIDataViewImageKind.Error;
        default:
          throw ExceptionFactory.ArgUnknownValue("imageKind", imageKind);
      }
    }

    /// <summary>
    /// Получить тип изображения для строки
    /// </summary>
    /// <param name="rowIndex">Индекс строки в просмотре. 
    /// Если равно (-1), возвращается значение свойства <see cref="TopLeftCellImageKind"/>.
    /// Иначе генерируется событие <see cref="RowInfoNeeded"/> для получения значка строки</param>
    /// <returns>Тип изображения</returns>
    public UIDataViewImageKind GetRowImageKind(int rowIndex)
    {
      if (rowIndex < 0)
        return TopLeftCellImageKind;
      else
        return GetRowInfo(rowIndex, EFPDataViewInfoReason.View).ImageKind;
    }


    /// <summary>
    /// Имя текстового столбца в связанном наборе данных, используемое для идентификации строки при выводе сообщений об ошибках для группы строк.
    /// Если использования столбца недостаточно для идентификации, устанавливайте свойство <see cref="EFPDataGridViewRowInfoEventArgs.RowIdText"/> 
    /// в обработчике события <see cref="RowInfoNeeded"/>.
    /// </summary>
    public string RowIdTextDataColumnName { get { return _RowIdTextDataColumnName; } set { _RowIdTextDataColumnName = value; } }
    private string _RowIdTextDataColumnName;

    /// <summary>
    /// Добавить в список <paramref name="errorMessages"/> сообщения, относящиеся к строке с заданным
    /// индексом.
    /// Выполняет вызов обработчика события <see cref="RowInfoNeeded"/> и возвращает сообщения для строки.
    /// В отличие от использования свойств <see cref="EFPDataGridViewRowInfoEventArgs.ToolTipText"/> и <see cref="EFPDataGridViewRowInfoEventArgs.ImageKind"/>,
    /// не теряется информация, какое сообщение является ошибкой, какое предупреждением, и т.д.
    /// </summary>
    /// <param name="errorMessages">Список для добавления сообщений</param>
    /// <param name="rowIndex">Индекс строки просмотра</param>
    public void GetRowErrorMessages(ErrorMessageList errorMessages, int rowIndex)
    {
      GetRowErrorMessages(errorMessages, rowIndex, false);
    }

    /// <summary>
    /// Добавить в список <paramref name="errorMessages"/> сообщения, относящиеся к строке с заданным
    /// индексом.
    /// Выполняет вызов обработчика события <see cref="RowInfoNeeded"/> и возвращает сообщения для строки.
    /// В отличие от использования свойств <see cref="EFPDataGridViewRowInfoEventArgs.ToolTipText"/> и <see cref="EFPDataGridViewRowInfoEventArgs.ImageKind"/>,
    /// не теряется информация, какое сообщение является ошибкой, какое предупреждением, и т.д.
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
      _RowInfoNeededArgs.RowErrorMessages = errorMessages;
      try
      {
        DoGetRowErrorMessages(rowIndex, useRowIdText);
      }
      finally
      {
        _RowInfoNeededArgs.RowErrorMessages = null;
      }
    }

    private void DoGetRowErrorMessages(int rowIndex, bool useRowIdText)
    {
      int cnt = _RowInfoNeededArgs.RowErrorMessages.Count;
      GetRowInfo(rowIndex, EFPDataViewInfoReason.ToolTip);
      if (_RowInfoNeededArgs.ImageKind != UIDataViewImageKind.None &&
        cnt == _RowInfoNeededArgs.RowErrorMessages.Count)
      {
        // Признак ошибки установлен, но сообщение не добавлено
        string msg;
        if (String.IsNullOrEmpty(_RowInfoNeededArgs.ToolTipText))
          msg = "No message";
        else
          msg = _RowInfoNeededArgs.ToolTipText.Replace(Environment.NewLine, " ");

        ErrorMessageKind kind2;
        switch (_RowInfoNeededArgs.ImageKind)
        {
          case UIDataViewImageKind.Error: kind2 = ErrorMessageKind.Error; break;
          case UIDataViewImageKind.Warning: kind2 = ErrorMessageKind.Warning; break;
          default: kind2 = ErrorMessageKind.Info; break;
        }
        _RowInfoNeededArgs.RowErrorMessages.Add(new ErrorMessageItem(kind2, msg, null, rowIndex));
      }

      if (useRowIdText && (_RowInfoNeededArgs.RowErrorMessages.Count > cnt))
        _RowInfoNeededArgs.RowErrorMessages.SetPrefix(_RowInfoNeededArgs.RowIdText + " - ", cnt);
    }

    /// <summary>
    /// Получить список сообщений для выбранных строк в просмотре.
    /// См. пояснения к <see cref="GetRowErrorMessages(ErrorMessageList, int, bool)"/>.
    /// </summary>
    /// <param name="errorMessages">Список, куда будут добавлены сообщения</param>
    /// <param name="useRowIdText">true - добавить перед сообщениями номера строк</param>
    public void GetSelectedRowsErrorMessages(ErrorMessageList errorMessages, bool useRowIdText)
    {
      if (errorMessages == null)
        throw new ArgumentNullException("errorMessages");

      int[] rowIdxs = SelectedRowIndices;
      _RowInfoNeededArgs.RowErrorMessages = errorMessages;
      try
      {
        for (int i = 0; i < rowIdxs.Length; i++)
          DoGetRowErrorMessages(rowIdxs[i], useRowIdText);
      }
      finally
      {
        _RowInfoNeededArgs.RowErrorMessages = null;
      }
    }

    /// <summary>
    /// Получить список сообщений для выбранных строк в просмотре
    /// Текст, идентифицирующий строку, задается, если число выбранных строк больше 1
    /// См. пояснения к <see cref="GetRowErrorMessages(ErrorMessageList, int, bool)"/>.
    /// </summary>
    /// <param name="errorMessages">Список, куда будут добавлены сообщения</param>
    public void GetSelectedRowsErrorMessages(ErrorMessageList errorMessages)
    {
      if (errorMessages == null)
        throw new ArgumentNullException("errorMessages");

      int[] rowIdxs = SelectedRowIndices;
      _RowInfoNeededArgs.RowErrorMessages = errorMessages;
      try
      {
        for (int i = 0; i < rowIdxs.Length; i++)
          DoGetRowErrorMessages(rowIdxs[i], rowIdxs.Length > 1);
      }
      finally
      {
        _RowInfoNeededArgs.RowErrorMessages = null;
      }
    }

    /// <summary>
    /// Получить список сообщений для всех строк в просмотре.
    /// Перед сообщениями добавляется текст, идентифицирующий строку.
    /// См. пояснения к <see cref="GetRowErrorMessages(ErrorMessageList, int, bool)"/>.
    /// </summary>
    /// <param name="errorMessages">Список, куда будут добавлены сообщения</param>
    public void GetAllRowsErrorMessages(ErrorMessageList errorMessages)
    {
      if (errorMessages == null)
        throw new ArgumentNullException("errorMessages");

      _RowInfoNeededArgs.RowErrorMessages = errorMessages;
      try
      {
        for (int i = 0; i < Control.RowCount; i++)
          DoGetRowErrorMessages(i, true);
      }
      finally
      {
        _RowInfoNeededArgs.RowErrorMessages = null;
      }
    }

    /// <summary>
    /// Перебирает все строки просмотра и возвращает максимальное значение типа ошибки для строки
    /// </summary>
    /// <returns>Тип значка</returns>
    public UIDataViewImageKind GetAllRowsImageKind()
    {
      int n = Control.RowCount;
      if (n == 0)
      {
        if (SourceAsDataView != null)
          n = SourceAsDataView.Count;
      }

      UIDataViewImageKind maxLevel = UIDataViewImageKind.None;

      for (int i = 0; i < n; i++)
      {
        UIDataViewImageKind thisLevel = GetRowImageKind(i);

        if (thisLevel > maxLevel)
        {
          maxLevel = thisLevel;
          if (maxLevel == UIDataViewImageKind.Error)
            break;
        }
      }
      return maxLevel;
    }

    #endregion

    #region Проверка управляющего элемента

    private bool _ValidateRequiredFlag;

    /// <summary>
    /// Выполняет инициализацию значка и подсказки для верхней левой ячейки таблицы
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      _ValidateRequiredFlag = false;

      if (ProviderState == EFPControlProviderState.Attached)
      {
        if (ShowRowCountInTopLeftCell)
          _RowCountTopLeftCellToolTipText = String.Format(Res.EFPDataView_ToolTip_TopLeftCellRowCount, Control.RowCount);
        else
          _RowCountTopLeftCellToolTipText = String.Empty;

        if (UseRowImages && ShowErrorCountInTopLeftCell)
        {
          int errorCount = 0;
          int warningCount = 0;
          int n = Control.RowCount;
          for (int i = 0; i < n; i++)
          {
            switch (GetRowImageKind(i))
            {
              case UIDataViewImageKind.Error:
                errorCount++;
                break;
              case UIDataViewImageKind.Warning:
                warningCount++;
                break;
            }
          }

          if (errorCount == 0 && warningCount == 0)
            _ErrorCountTopLeftCellToolTipText = Res.EFPDataView_ToolTip_TopLeftCellNoErrors;
          else
          {
            StringBuilder sb = new StringBuilder();
            if (errorCount > 0)
            {
              sb.Append(String.Format(Res.EFPDataView_ToolTip_TopLeftCellErrors, errorCount));
              sb.Append(Environment.NewLine);
            }
            if (warningCount > 0)
            {
              sb.Append(String.Format(Res.EFPDataView_ToolTip_TopLeftCellWarmings, warningCount));
              sb.Append(Environment.NewLine);
            }

            if (CommandItems.UseRowErrors)
              sb.Append(Res.EFPDataView_ToolTip_TopLeftCellErrNav);
            _ErrorCountTopLeftCellToolTipText = sb.ToString();
          }

          if (errorCount > 0)
            TopLeftCellImageKind = UIDataViewImageKind.Error;
          else
          {
            if (warningCount > 0)
              TopLeftCellImageKind = UIDataViewImageKind.Warning;
            else
              TopLeftCellImageKind = UIDataViewImageKind.None;
          }
        }
        else
        {
          _ErrorCountTopLeftCellToolTipText = String.Empty;
          // TopLeftCellImageKind не устанавливаем, его может устанавливать прикладной код
        }

        InitTopLeftHeaderCellToolTipText();
        CommandItems.RefreshStatItems();
      }
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
    /// Установка в null завершает поиск по буквам.
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
        _CurrentIncSearchColumnIndex = Control.CurrentCellAddress.X;
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

    /// <summary>
    /// Устанавливается вместе с _CurrentIncSearchColumn, но задает номер текущего столбца, который может не совпадать в режиме
    /// выделения всей строки
    /// </summary>
    private int _CurrentIncSearchColumnIndex;

    /// <summary>
    /// Возвращает столбец, который следует использовать для быстрого поиска.
    /// Возвращает <see cref="CurrentColumn"/>, если <see cref="EFPDataGridViewColumn.CanIncSearch"/>=true,
    /// иначе возвращает null.
    /// Если просмотр находится в режиме выбора целых строк, возвращается первый столбец, поддерживающий поиск
    /// </summary>
    public EFPDataGridViewColumn DesiredIncSearchColumn
    {
      get
      {
        EFPDataGridViewColumn col = CurrentColumn;
        if (col != null)
        {
          if (col.CanIncSearch)
            return col;
        }

        if (WholeRowsSelected)
          return FirstIncSearchColumn;

        return null;
      }
    }

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
      string editMask = _CurrentIncSearchColumn.MaskProvider.EditMask;
      if (String.IsNullOrEmpty(editMask))
        return null;
      MaskedTextProvider provider = new MaskedTextProvider(editMask,
        // System.Globalization.CultureInfo.CurrentCulture,
        _CurrentIncSearchColumn.MaskProvider.Culture, // 04.06.2019
        false, (char)0, (char)0, false);
      provider.SkipLiterals = true;
      provider.ResetOnSpace = false;
      provider.Set(_CurrentIncSearchChars);
      return provider;
    }

    void Control_KeyPress(object sender, KeyPressEventArgs args)
    {
      try
      {
        DoControl_KeyPress(args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }
    void DoControl_KeyPress(KeyPressEventArgs args)
    {
      if (args.Handled)
        return;
      if (args.KeyChar < ' ')
        return;

      if (args.KeyChar == ' ')
      {
        if (System.Windows.Forms.Control.ModifierKeys != Keys.None)
          return; // 31.10.2018 Отсекаем Shift-Space, Ctrl-Space и прочее, так как эти клавиши обрабатываются не как символы для поиска 

        if (WholeRowsSelected && MarkRowsGridColumn != null)
        {
          // 29.04.2025
          CurrentColumn = MarkRowsColumn; // переходим на столбец с отметками
          return;
        }
      }

      // Нажатие клавиши может начать поиск по первым буквам
      if (CurrentIncSearchColumn == null)
      {
        // Если поиск не начат, пытаемся его начать
        if (DesiredIncSearchColumn == null)
          return;
        CurrentIncSearchColumn = DesiredIncSearchColumn;
      }

      args.Handled = true;

      EFPApp.ShowTempMessage(null);

      // Под Windows-98 неправильная кодировка русских букв - исправляем
      char keyChar = WinFormsTools.CorrectCharWin98(args.KeyChar);

      MaskedTextProvider maskProvider = GetIncSearchMaskProvider();
      string s;
      if (maskProvider != null)
      {
        // для столбца задана маска
        if (!AddCharToIncSearchMask(maskProvider, keyChar, out s))
          return;
      }
      else
      {
        // Нет маски
        s = (CurrentIncSearchChars + keyChar);
      }

      bool res;
      EFPApp.BeginWait(Res.EFPDataView_Phase_IncSearch);
      try
      {
        //Res = CurrentColumn.PerformIncSearch(s.ToUpper(), false);
        // 27.12.2020
        res = CurrentIncSearchColumn.PerformIncSearch(s.ToUpper(), false);
      }
      finally
      {
        EFPApp.EndWait();
      }
      if (!res)
      {
        EFPApp.ShowTempMessage(String.Format(Res.EFPDataView_Err_IncSearchCannotAddChar, keyChar, String.Empty));
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
      int resPos;
      MaskedTextResultHint resHint;
      if (!maskProvider.Add(keyChar, out resPos, out resHint))
      {
        // Пытаемся подобрать символ в нижнем регистре
        s1 = s1.ToLower();
        char keyChar2 = s1[0];
        if (!maskProvider.Add(keyChar2, out resPos, out resHint))
        {
          // MaskedTextProvider почему-то не пропускает символы-разделители в 
          // качестве ввода, независимо от SkipLiterals
          // Поэтому, если следующим символом ожидается разделитель, то его
          // добавляем к строке поиска вручную
          if (resHint != MaskedTextResultHint.UnavailableEditPosition)
          {
            if ((!maskProvider.IsEditPosition(maskProvider.LastAssignedPosition + 1)) && maskProvider[maskProvider.LastAssignedPosition + 1] == keyChar)
            {
              s = maskProvider.ToString(0, maskProvider.LastAssignedPosition + 1) + keyChar;
              return true;
            }
          }
          EFPApp.ShowTempMessage(String.Format(Res.EFPDataView_Err_IncSearchCannotAddChar, keyChar, UITools.ToString(resHint)));
          return false;
        }
      }
      if (resHint == MaskedTextResultHint.CharacterEscaped)
      {
        EFPApp.ShowTempMessage(String.Format(Res.EFPDataView_Err_IncSearchCannotAddChar, keyChar, UITools.ToString(resHint)));
        return false;
      }
      s = maskProvider.ToString(0, maskProvider.LastAssignedPosition + 1);
      return true;
    }

    void DoControl_KeyDown(KeyEventArgs args)
    {
      if (args.Handled)
        return; // уже обработано
      if (Control.IsCurrentCellInEditMode &&
        (!(Control.CurrentCell is DataGridViewCheckBoxCell))) // 05.12.2025
        return; // в режиме редактирования ячейки не обрабатывается

      if (CurrentIncSearchColumn != null)
      {
        if (args.KeyCode == Keys.Back)
        {
          args.Handled = true;
          EFPApp.ShowTempMessage(null);
          if (_CurrentIncSearchChars.Length < 1)
          {
            EFPApp.ShowTempMessage(Res.EFPDataView_Err_IncSearchIsEmpty);
            return;
          }

          MaskedTextProvider maskProvider = GetIncSearchMaskProvider();
          string s;
          if (maskProvider != null)
          {
            if (_CurrentIncSearchChars.Length == maskProvider.LastAssignedPosition + 1)
            {
              maskProvider.Remove();
              s = maskProvider.ToString(0, maskProvider.LastAssignedPosition + 1);
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
    /// Получить индекс первого столбца с установленным <see cref="EFPDataGridViewColumn.CanIncSearch"/> или (-1), если
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

    /// <summary>
    /// Получить первый столбец с установленным <see cref="EFPDataGridViewColumn.CanIncSearch"/> или null, если
    /// таких столбцов нет.
    /// Может применяться при инициализации просмотра для определения начального 
    /// активного столбца.
    /// </summary>
    public EFPDataGridViewColumn FirstIncSearchColumn
    {
      get
      {
        int colIndex = FirstIncSearchColumnIndex;
        if (colIndex < 0)
          return null;
        else
          return Columns[colIndex];
      }
    }

    #endregion

    #region Поиск ячейки по содержимому

    /// <summary>
    /// Переход к следующей или предыдущей строке, содержимое ячейки в которой отличиается от содержимого текущей ячейки
    /// </summary>
    /// <param name="forward">True - переход к следующей строке, false - к предыдущей</param>
    public void GotoRowWithDiffValue(bool forward)
    {
      if (Control.CurrentCell == null)
      {
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoSelectedCell);
        return;
      }

      int gotoRowIndex = FindRowWithDiffValue(Control.CurrentCell.RowIndex, Control.CurrentCell.ColumnIndex, forward);
      if (gotoRowIndex < 0)
      {
        EFPApp.ShowTempMessage(Res.EFPDataView_Err_NoRowWithDiffValue);
        return;
      }
      CurrentRowIndex = gotoRowIndex;
    }

    /// <summary>
    /// Возвращает номер строки, больший или меньший <paramref name="startRowIndex"/>, в которой значение ячейки отличается
    /// от значения в ячейке текущей строки.
    /// Если <paramref name="startRowIndex"/> равен первой или последней строке просмотра (в зависимости от <paramref name="forward"/>), возвращается (-1).
    /// Если все строки до начала/конца таблицы содержат одинаковые значения, также возвращается (-1).
    /// </summary>
    /// <param name="startRowIndex">Индекс текущей строки</param>
    /// <param name="columnIndex">Индекс столбца со значением ячейки</param>
    /// <param name="forward">true - поиск строк вниз таблицы, false - вверх</param>
    /// <returns>Индекс найденной строки или (-1)</returns>
    public int FindRowWithDiffValue(int startRowIndex, int columnIndex, bool forward)
    {
      if (startRowIndex < 0 || columnIndex < 0)
        return -1;

      if (Control.Columns[columnIndex] is DataGridViewImageColumn)
      {
        // 10.10.2025. Сравнение значков

        object value = InternalGetCellValue(startRowIndex, columnIndex);
        if (forward)
        {
          for (int iRow = startRowIndex + 1; iRow < Control.RowCount; iRow++)
          {
            if (!Object.ReferenceEquals(InternalGetCellValue(iRow, columnIndex), value))
              return iRow;
          }
        }
        else
        {
          for (int iRow = startRowIndex - 1; iRow >= 0; iRow--)
          {
            if (!Object.ReferenceEquals(InternalGetCellValue(iRow, columnIndex), value))
              return iRow;
          }
        }
      }
      else
      {
        string text = GetCellTextValue(startRowIndex, columnIndex);
        if (forward)
        {
          for (int iRow = startRowIndex + 1; iRow < Control.RowCount; iRow++)
          {
            if (!String.Equals(GetCellTextValue(iRow, columnIndex), text, StringComparison.OrdinalIgnoreCase))
              return iRow;
          }
        }
        else
        {
          for (int iRow = startRowIndex - 1; iRow >= 0; iRow--)
          {
            if (!String.Equals(GetCellTextValue(iRow, columnIndex), text, StringComparison.OrdinalIgnoreCase))
              return iRow;
          }
        }
      }
      return -1;
    }


    /// <summary>
    /// Получение текстового значения для одной ячейки
    /// </summary>
    /// <param name="rowIndex">Индекс строки в просмотре</param>
    /// <param name="columnIndex">Индекс столбца в просмотре</param>
    /// <returns>Текстовое значение</returns>
    private object InternalGetCellValue(int rowIndex, int columnIndex)
    {
      GetRowInfo(rowIndex, EFPDataViewInfoReason.View);
      EFPDataGridViewCellInfoEventArgs cellArgs = GetCellInfo(columnIndex);
      return cellArgs.FormattedValue;
    }


    #endregion

    #region Свойство CurrentGridConfig

    /// <summary>
    /// Выбранная настройка табличного просмотра.
    /// Если свойство <see cref="GridProducer"/> не установлено, должен быть задан обработчик <see cref="CurrentConfigChanged"/>,
    /// который выполнит реальную настройку просмотра
    /// </summary>
    public EFPDataViewConfig CurrentConfig
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
            CancelEventArgs args = new CancelEventArgs();
            args.Cancel = false;
            OnCurrentConfigChanged(args);
            if ((!args.Cancel) && (GridProducer != null))
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
    private EFPDataViewConfig _CurrentConfig;

    /// <summary>
    /// Флажок нахождения в пределах сеттера свойства <see cref="CurrentConfig"/>.
    /// </summary>
    protected bool InsideSetCurrentConfig = false;

    /// <summary>
    /// Признак того, что свойство <see cref="CurrentConfig"/> уже устанавливалось
    /// </summary>
    protected bool CurrentConfigHasBeenSet { get { return _CurrentConfigHasBeenSet; } }
    private bool _CurrentConfigHasBeenSet = false;

    /// <summary>
    /// Вызывается при изменении свойства <see cref="CurrentConfig"/>.
    /// Если аргумент <see cref="CancelEventArgs.Cancel"/> обработчиком установлен в true, то предполагается,
    /// что инициализация просмотра выполнена в обработчике. В противном случае
    /// (по умолчанию Cancel=false или при отстутствии обработчика) будет вызван
    /// метод <see cref="IEFPGridProducer.InitGridView(EFPDataGridView, bool)"/>.
    /// </summary>
    public event CancelEventHandler CurrentConfigChanged;

    /// <summary>
    /// Вызывает событие <see cref="CurrentConfigChanged"/>.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnCurrentConfigChanged(CancelEventArgs args)
    {
      if (CurrentConfigChanged != null)
        CurrentConfigChanged(this, args);
    }

    /// <summary>
    /// Это событие вызывается после того, как конфигурация табличного просмотра инициализирована
    /// с помощью <see cref="IEFPGridProducer.InitGridView(EFPDataGridView, bool)"/>.
    /// </summary>
    public event EventHandler GridProducerPostInit;

    /// <summary>
    /// Вызывает событие <see cref="GridProducerPostInit"/>
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

      // Добавлено 17.02.2022
      InitColumnSortMode();
      InternalSetCurrentOrder();
    }

    /// <summary>
    /// Вызывает событие <see cref="GridProducerPostInit"/>
    /// </summary>
    protected virtual void OnGridProducerPostInit()
    {
      if (GridProducerPostInit != null)
        GridProducerPostInit(this, EventArgs.Empty);
    }

    #endregion

    #region Установка отметок строк в столбце CheckBox

    /// <summary>
    /// Столбец <see cref="DataGridViewCheckBoxColumn"/>, с помощью которого устанавливаются отметки для строк
    /// Если свойство установлено, то в локальном меню просмотра появляется
    /// подменю "Установка отметок для строк".
    /// Свойство может быть установлено при добавлении столбца с помощью аргумента
    /// markRows в методе <see cref="EFPDataGridViewColumns.AddCheckBox(string, bool, string)"/>.
    /// Свойство следует устанавливать после добавления всех столбцов в просмотр,
    /// но перед показом на экране. Также допускается повторная установка свойства в процессе работы, если столбцы создаются заново.
    /// При установке свойства, если <see cref="DataGridView.ReadOnly"/>=true, устанавливается свойство
    /// <see cref="DataGridViewColumn.ReadOnly"/> для всех столбцов, кроме заданного, а <see cref="DataGridView.ReadOnly"/>  устанавливается в false.
    /// Также устанавливается в true свойство <see cref="DataGridView.MultiSelect"/>.
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
            throw ExceptionFactory.ArgProperty("value", value, "DataGridView", value.DataGridView, new object[] { Control });
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
    /// Альтернативная установка свойства <see cref="MarkRowsColumn"/>
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
    /// Задает один из столбцов с списке <see cref="Columns"/> для установки отметок строк.
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
    /// Имя столбца в списке <see cref="Columns"/>, соответствующего <see cref="MarkRowsColumn"/>.
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
    /// Установка отметок в <see cref="DataGridViewCheckBoxColumn"/> для требуемых строк.
    /// Должно быть установлено свойство <see cref="MarkRowsColumn"/>.
    /// Скрытые строки пропускаются
    /// </summary>
    /// <param name="rows">Какие строки обрабатываются</param>
    /// <param name="action">Установка, сброс или инверсия отметки</param>
    /// <returns>Число строк, которые были изменены</returns>
    public int CheckMarkRows(EFPDataGridViewCheckMarkRows rows, EFPDataGridViewCheckMarkAction action)
    {
      if (MarkRowsGridColumn == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "MarkRowsGridColumn");

      // Отменяем режим редактирования и поиск по буквам
      CurrentIncSearchColumn = null;
      Control.EndEdit();

      int columnIndex = _MarkRowsGridColumn.Index;
      int cnt = 0;
      switch (rows)
      {
        case EFPDataGridViewCheckMarkRows.Selected:
          int[] selRows = SelectedRowIndices;
          for (int i = 0; i < selRows.Length; i++)
          {
            if (CheckMarkRow(selRows[i], columnIndex, action))
            {
              cnt++;
              Control.InvalidateCell(columnIndex, selRows[i]);
            }
          }
          break;
        case EFPDataGridViewCheckMarkRows.All:
          for (int i = 0; i < Control.RowCount; i++)
          {
            if (CheckMarkRow(i, columnIndex, action))
              cnt++;
          }
          if (cnt > 0)
            Control.InvalidateColumn(columnIndex);

          break;
        default:
          throw ExceptionFactory.ArgUnknownValue("rows", rows);
      }
      return cnt;
    }

    private bool CheckMarkRow(int rowIndex, int columnIndex, EFPDataGridViewCheckMarkAction action)
    {
      DataGridViewRow row;
      row = Control.Rows.SharedRow(rowIndex);
      if (row.Index >= 0)
      {
        // Для разделяемой строки такое действие не допускается
        if (!row.Visible)
          return false;
        if (row.ReadOnly)
          return false;
      }
      /*EFPDataGridViewRowAttributesEventArgs RowArgs = */
      GetRowInfo(rowIndex, EFPDataViewInfoReason.View);
      EFPDataGridViewCellInfoEventArgs cellArgs = GetCellInfo(columnIndex);
      bool orgValue = DataTools.GetBoolean(cellArgs.Value);
      bool newValue;
      switch (action)
      {
        case EFPDataGridViewCheckMarkAction.Check:
          newValue = true;
          break;
        case EFPDataGridViewCheckMarkAction.Uncheck:
          newValue = false;
          break;
        case EFPDataGridViewCheckMarkAction.Invert:
          newValue = !orgValue;
          break;
        default:
          throw ExceptionFactory.ArgUnknownValue("action", action);
      }

      if (newValue == orgValue)
        return false;
      // Требуется не Shared-строка
      row = Control.Rows[rowIndex];
      row.Cells[columnIndex].Value = newValue;

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
        GetRowInfo(area.RowIndices[i], EFPDataViewInfoReason.View);
        for (int j = 0; j < area.ColumnCount; j++)
        {
          EFPDataGridViewCellInfoEventArgs cellArgs = GetCellInfo(area.ColumnIndices[j]);
          a[i, j] = DoGetCellTextValue(cellArgs);
        }
      }

      return a;
    }

    private string DoGetCellTextValue(EFPDataGridViewCellInfoEventArgs cellArgs)
    {
      object v = cellArgs.FormattedValue;
      if (cellArgs.FormattingApplied)
      {
        if (v == null)
          return String.Empty;
        else
          return v.ToString();
      }
      else
      {
        IFormattable fv = v as IFormattable;
        if (cellArgs.CellStyle.Format != null && fv != null)
          return fv.ToString(cellArgs.CellStyle.Format, cellArgs.CellStyle.FormatProvider); // 18.11.2024
        else if (v == null)
          return String.Empty;
        else
          return v.ToString();
      }
    }

    /// <summary>
    /// Получение текстового значения для одной ячейки
    /// </summary>
    /// <param name="rowIndex">Индекс строки в просмотре</param>
    /// <param name="columnIndex">Индекс столбца в просмотре</param>
    /// <returns>Текстовое значение</returns>
    public string GetCellTextValue(int rowIndex, int columnIndex)
    {
      GetRowInfo(rowIndex, EFPDataViewInfoReason.View);
      EFPDataGridViewCellInfoEventArgs cellArgs = GetCellInfo(columnIndex);
      return DoGetCellTextValue(cellArgs);
    }

    /// <summary>
    /// Получить массив строк и столбцов для записи в файл
    /// </summary>
    /// <param name="rangeMode">Заданный режим выделения</param>
    /// <returns>Массив строк и столбцов</returns>
    public EFPDataGridViewRectArea GetRectArea(EFPDataViewExpRange rangeMode)
    {
      return new EFPDataGridViewRectArea(Control, rangeMode);
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
    public virtual BRColumnHeaderArray GetColumnHeaderArray(EFPDataGridViewColumn[] columns)
    {
      // Метод переопределен в программе "Бюджетный учет" для поддержки заголовков из параметров страницы.

#if DEBUG
      if (columns == null)
        throw new ArgumentNullException("columns");
#endif

      #region Получение массива заголовков столбцов

      string[][] headers = new string[columns.Length][];

      for (int i = 0; i < columns.Length; i++)
      {
        headers[i] = columns[i].PrintHeaders;
        if (headers[i] == null)
          headers[i] = new string[] { columns[i].GridColumn.HeaderText };
      }

      #endregion

      BRColumnHeaderArray ha = new BRColumnHeaderArray(headers, ColumnHeaderMixedSpanAllowed);
      return ha;
    }

    /// <summary>
    /// Создание описателей заголовков столбцов для выбранной области ячеек.
    /// </summary>
    /// <param name="area">Выбранная область. Используются только столбцы, строки игнорируются</param>
    /// <returns>Описания столбцов</returns>
    public /*virtual*/ BRColumnHeaderArray GetColumnHeaderArray(EFPDataGridViewRectArea area)
    {
#if DEBUG
      if (area == null)
        throw new ArgumentNullException("area");
#endif

      EFPDataGridViewColumn[] selCols = new EFPDataGridViewColumn[area.ColumnCount];
      for (int i = 0; i < area.ColumnCount; i++)
        selCols[i] = Columns[area.ColumnIndices[i]];
      return GetColumnHeaderArray(selCols);
    }

    /// <summary>
    /// Получить сводку документа при экспорте.
    /// По умолчанию содержит копию <see cref="BRReport.AppDefaultDocumentProperties"/>
    /// с установленным свойством <see cref="BRDocumentProperties.Title"/> в соответствии с <see cref="EFPControlBase.DisplayName"/> или
    /// заголовком окна, если просмотр является единственным управляющим элементом.
    /// </summary>
    public BRDocumentProperties DocumentProperties
    {
      get { return _DocumentProperties; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _DocumentProperties = value;
      }
    }
    private BRDocumentProperties _DocumentProperties;

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
          DataGridViewRow row = Control.Rows.SharedRow(i);
          if (row.Index >= 0)
            cnt++;
        }
        return cnt;
      }
    }

    /// <summary>
    /// Для отладочных целей.
    /// Возвращает количество строк в просмотре, для которых еще не было выполнено Unshare.
    /// Выполняется медленно, т.к. требуется перебрать все строки в просмотре.
    /// </summary>
    public int SharedRowCount
    {
      get
      {
        int cnt = 0;
        int n = Control.RowCount;
        for (int i = 0; i < n; i++)
        {
          DataGridViewRow row = Control.Rows.SharedRow(i);
          if (row.Index < 0)
            cnt++;
        }
        return cnt;
      }
    }

    #endregion

    #region Получение EFPDataViewRowValues

    private DataRowValues _RowValueArray;

    /// <summary>
    /// Получить объект <see cref="EFPDataViewRowValues"/> для строки.
    /// Когда он больше не нужен, должен быть вызван <see cref="FreeRowValues(EFPDataViewRowValues)"/>.
    /// </summary>
    /// <param name="rowIndex">Индекс строки табличного просмотра</param>
    /// <returns>Информация о строке</returns>
    public EFPDataViewRowValues GetRowValues(int rowIndex)
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

      return new EFPDataViewRowValues(this, rwa.CurrentRow, rwa, rowIndex);
    }

    /// <summary>
    /// Создает объект <see cref="DataRowValues"/> для доступа к данным.
    /// Класс-наследник может создавать объект, который, например, использует буферизацию данных
    /// </summary>
    /// <returns>Объект для доступа к данным</returns>
    protected virtual IDataRowNamedValuesAccess CreateRowValueAccessObject()
    {
      return new DataRowValues();
    }

    /// <summary>
    /// Освободить данные, полученные <see cref="GetRowValues(int)"/>.
    /// </summary>
    /// <param name="rowValues"></param>
    public void FreeRowValues(EFPDataViewRowValues rowValues)
    {
      _RowValueArray = rowValues.Values as DataRowValues; // для повторного использования.
      //      if (_RowValueArray != null)
      //        _RowValueArray.CurrentRow = null;
    }

    #endregion
  }

  /// <summary>
  /// Исключение, выбрасываемое, если источник данных (например, свойство <see cref="DataGridView.DataSource"/>)
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
