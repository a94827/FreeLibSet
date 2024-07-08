// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using FreeLibSet.Models.Tree;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Controls;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.Reporting;
using FreeLibSet.Collections;
using System.ComponentModel;

namespace FreeLibSet.Forms
{
  #region Цветовые атрибуты
#if XXX

  /// <summary>
  /// Базовый класс для EFPDataGridViewRowAttributesEventArgs и 
  /// EFPDataGridViewCellAttributesEventArgs
  /// </summary>
  public class EFPDataTreeViewCellAttributesEventArgsBase : EventArgs
  {
  #region Конструктор

    /// <summary>
    /// Создает объект аргументов
    /// </summary>
    /// <param name="ControlProvider">Провайдер управляющего элемента</param>
    public EFPDataTreeViewCellAttributesEventArgsBase(EFPDataTreeView ControlProvider)
    {
      FControlProvider = ControlProvider;
    }

  #endregion

  #region Свойства - исходные данные

    /// <summary>
    /// Обработчик просмотра
    /// </summary>
    public EFPDataTreeView ControlProvider { get { return FControlProvider; } }
    private EFPDataTreeView FControlProvider;

    /// <summary>
    /// Управляющий элемент
    /// </summary>
    public TreeViewAdv Control { get { return ControlProvider.Control; } }

    /// <summary>
    /// Зачем было вызвано событие: для вывода на экран или для печати
    /// </summary>
    public EFPDataGridViewAttributesReason Reason { get { return FReason; } }
    private EFPDataGridViewAttributesReason FReason;

    /// <summary>
    /// Номер строки, для которой требуются атрибуты, в табличном просмотре
    /// </summary>
    public int RowIndex { get { return FRowIndex; } }
    private int FRowIndex;

    /// <summary>
    /// Доступ к строке таблицы данных, если источником данных для табличного
    /// просмотра является DataView. Иначе - null
    /// </summary>
    public DataRowView RowView
    {
      get
      {
        return null; // TODO: 
        /*
        DataView dv = ControlProvider.SourceAsDataView;
        if (dv == null)
          return null;
        else
          return dv[RowIndex];
         */
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
        return null;
        // TODO: return ControlProvider.GetDataRow(RowIndex);
      }
    }

    /*
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
      */
  #endregion

  #region Устанавливаемые свойства

    /// <summary>
    /// Цветовое оформление
    /// </summary>
    public EFPDataGridViewColorType ColorType
    {
      get { return FColorType; }
      set
      {
        FColorType = value;
        if (value == EFPDataGridViewColorType.TotalRow)
        {
          //LeftBorder = EFPDataGridViewBorderStyle.Thick;
          TopBorder = EFPDataGridViewBorderStyle.Thick;
          //RightBorder = EFPDataGridViewBorderStyle.Thick;
          BottomBorder = EFPDataGridViewBorderStyle.Thick;
        }
      }
    }
    private EFPDataGridViewColorType FColorType;

    /// <summary>
    /// Если установить в true, то текст ячейки будет рисоваться серым шрифтом
    /// По умолчанию - false
    /// </summary>
    public bool Grayed
    {
      get
      {
        if (FGrayed.HasValue)
          return FGrayed.Value;
        else
          return false;
      }
      set
      {
        FGrayed = value;
      }
    }
    internal bool? FGrayed;

    /// <summary>
    /// В текущей реализации не используется
    /// </summary>
    public EFPDataGridViewBorderStyle LeftBorder { get { return FLeftBorder; } set { FLeftBorder = value; } }
    private EFPDataGridViewBorderStyle FLeftBorder;

    /// <summary>
    /// В текущей реализации не используется
    /// </summary>
    public EFPDataGridViewBorderStyle TopBorder { get { return FTopBorder; } set { FTopBorder = value; } }
    private EFPDataGridViewBorderStyle FTopBorder;

    /// <summary>
    /// В текущей реализации не используется
    /// </summary>
    public EFPDataGridViewBorderStyle RightBorder { get { return FRightBorder; } set { FRightBorder = value; } }
    private EFPDataGridViewBorderStyle FRightBorder;

    /// <summary>
    /// В текущей реализации не используется
    /// </summary>
    public EFPDataGridViewBorderStyle BottomBorder { get { return FBottomBorder; } set { FBottomBorder = value; } }
    private EFPDataGridViewBorderStyle FBottomBorder;

    /// <summary>
    /// Используется в режиме Reason=ReadOnly
    /// </summary>
    public bool ReadOnly { get { return FReadOnly; } set { FReadOnly = value; } }
    private bool FReadOnly;

    /// <summary>
    /// Используется в режиме Reason=ReadOnly
    /// Текст сообщения, почему нельзя редактировать строку / ячейку
    /// </summary>
    public string ReadOnlyMessage { get { return FReadOnlyMessage; } set { FReadOnlyMessage = value; } }
    private string FReadOnlyMessage;

  #endregion

  #region Методы

    /// <summary>
    /// В текущей реализации не используется
    /// </summary>
    /// <param name="Style"></param>
    public void SetAllBorders(EFPDataGridViewBorderStyle Style)
    {
      LeftBorder = Style;
      TopBorder = Style;
      RightBorder = Style;
      BottomBorder = Style;
    }

  #endregion

  #region Защищенные методы

    /// <summary>
    /// Инициализация начальных значений при получении атрибутов для строки
    /// </summary>
    /// <param name="RowIndex"></param>
    /// <param name="Reason"></param>
    protected void InitRow(int RowIndex, EFPDataGridViewAttributesReason Reason)
    {
      FRowIndex = RowIndex;
      FReason = Reason;
      ColorType = EFPDataGridViewColorType.Normal;
      FGrayed = null;
      LeftBorder = EFPDataGridViewBorderStyle.Default;
      TopBorder = EFPDataGridViewBorderStyle.Default;
      RightBorder = EFPDataGridViewBorderStyle.Default;
      BottomBorder = EFPDataGridViewBorderStyle.Default;
    }

  #endregion
  }

  /// <summary>
  /// Аргументы события EFPDataTreeView.
  /// </summary>
  public class EFPDataTreeViewRowAttributesEventArgs : EFPDataTreeViewCellAttributesEventArgsBase
  {
  #region Конструктор

    public EFPDataTreeViewRowAttributesEventArgs(EFPDataTreeView ControlProvider)
      : base(ControlProvider)
    {
      CellErrorMessages = new Dictionary<int, ErrorMessageList>();
    }

  #endregion

  #region Устанавливаемые свойства

    /// <summary>
    /// При печати запретить отрывать текущую строку от предыдущей
    /// </summary>
    public bool PrintWithPrevious { get { return FPrintWithPrevious; } set { FPrintWithPrevious = value; } }
    private bool FPrintWithPrevious;

    /// <summary>
    /// При печати запретить отрывать текущую строку от следующей
    /// </summary>
    public bool PrintWithNext { get { return FPrintWithNext; } set { FPrintWithNext = value; } }
    private bool FPrintWithNext;

    /// <summary>
    /// Изображение, которое будет выведено в заголовке строки (в серой ячейке)
    /// Должно быть установлено свойство EFPDataGridView.UseRowImages
    /// Это свойство также используется при навигации по Ctrl-[, Ctrl-]
    /// Стандартный значок может быть переопределен установкой свойства UserImage
    /// </summary>
    public EFPDataGridViewImageKind ImageKind { get { return FImageKind; } set { FImageKind = value; } }
    private EFPDataGridViewImageKind FImageKind;

    /// <summary>
    /// Переопределение изображения, которое будет выведено в заголовке строки 
    /// (в серой ячейке)
    /// Если свойство не установлено (по умолчанию), то используются стандартные
    /// значки для ImageKind=Information,Warning и Error, или пустое изображение
    /// при ImageKind=None. 
    /// Свойство не влияет на навигацию по строкам по Ctrl-[, Ctrl-]
    /// Должно быть установлено свойство EFPDataGridView.UseRowImages
    /// </summary>
    public Image UserImage { get { return FUserImage; } set { FUserImage = value; } }
    private Image FUserImage;

    /// <summary>
    /// Всплывающая подсказка, которая будет выведена при наведении курсора на 
    /// ячейку заголовка строки. Поле должно заполняться только при Reason=ToolTip.
    /// В других режимах значение игнорируется
    /// </summary>
    public string ToolTipText { get { return FToolTipText; } set { FToolTipText = value; } }
    private string FToolTipText;

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
        if (String.IsNullOrEmpty(FRowIdText))
          return DefaultRowIdText;
        else
          return FRowIdText;
      }
      set
      {
        FRowIdText = value;
      }
    }
    private string FRowIdText;

    /// <summary>
    /// Идентификатор строки по умолчанию.
    /// Если установлено свойство EFPDataGridView.RowIdTextDataColumnName
    /// </summary>
    public string DefaultRowIdText
    {
      get
      {
        // TODO:
        /*
        if (!String.IsNullOrEmpty(ControlProvider.RowIdTextDataColumnName))
        {
          if (DataRow == null)
            return "Нет строки";
          return DataTools.GetString(DataRow, ControlProvider.RowIdTextDataColumnName);
        } */

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
    public bool ControlContentVisible { get { return FControlContentVisible; } set { FControlContentVisible = value; } }
    private bool FControlContentVisible;

  #endregion

  #region Сообщения об ошибках и подсказки

  #region Основные методы

    /// <summary>
    /// Устанавливает для строки подходящее изображение и добавляет сообщения
    /// к свойству ToolTipText из списка ошибок
    /// Эквивалентно вызову метода AddRowErrorMessage() для каждого сообщения в списке
    /// </summary>
    /// <param name="Errors">Список ошибок</param>
    public void AddRowErrorMessages(ErrorMessageList Errors)
    {
      if (Errors == null)
        return;
      for (int i = 0; i < Errors.Count; i++)
        AddRowErrorMessage(Errors[i]);
    }

    /// <summary>
    /// Устанавливает для строки подходящее изображение и добавляет сообщение
    /// к свойству ToolTipText.
    /// Эквивалентно одному из вызовов: AddRowError(), AddRowWarning() или
    /// AddRowInformation()
    /// </summary>
    /// <param name="Kind">Важность: Ошибка, предупреждение или сообщение</param>
    /// <param name="Message">Текст сообщения</param>
    public void AddRowErrorMessage(ErrorMessageKind Kind, string Message)
    {
      AddRowErrorMessage(new ErrorMessageItem(Kind, Message));
    }

    /// <summary>
    /// Устанавливает для строки подходящее изображение и добавляет сообщение
    /// к свойству ToolTipText.
    /// Эквивалентно одному из вызовов: AddRowError(), AddRowWarning() или
    /// AddRowInformation()
    /// </summary>
    /// <param name="Error">Объект ошибки ErrorMessageItem</param>
    public void AddRowErrorMessage(ErrorMessageItem Error)
    {
      AddRowErrorMessage(Error, String.Empty);
    }

    public void AddRowErrorMessage(ErrorMessageItem Error, string ColumnNames)
    {
      EFPDataGridViewImageKind NewImageKind;
      switch (Error.Kind)
      {
        case ErrorMessageKind.Error: NewImageKind = EFPDataGridViewImageKind.Error; break;
        case ErrorMessageKind.Warning: NewImageKind = EFPDataGridViewImageKind.Warning; break;
        case ErrorMessageKind.Info: NewImageKind = EFPDataGridViewImageKind.Information; break;
        default:
          throw new ArgumentException("Неправильное значение Error.Kind=" + Error.Kind.ToString());
      }

      if ((int)NewImageKind > (int)ImageKind)
        ImageKind = NewImageKind;

      AddToolTipText(Error.Text);
      if (RowErrorMessages != null)
        RowErrorMessages.Add(new ErrorMessageItem(Error.Kind, Error.Text, Error.Code, RowIndex)); // исправлено 09.11.2014

      if (!String.IsNullOrEmpty(ColumnNames))
      {
        if (ColumnNames.IndexOf(',') >= 0)
        {
          string[] a = ColumnNames.Split(',');
          for (int i = 0; i < a.Length; i++)
            AddOneCellErrorMessage(Error, a[i]);
        }
        else
          AddOneCellErrorMessage(Error, ColumnNames);
      }
    }

    private void AddOneCellErrorMessage(ErrorMessageItem Error, string ColumnName)
    {
      /*
      int ColumnIndex = ControlProvider.Columns.IndexOf(ColumnName);
      if (ColumnIndex < 0)
        return; // имя несуществующего столбца
      ErrorMessageList Errors;
      if (!CellErrorMessages.TryGetValue(ColumnIndex, out Errors))
      {
        Errors = new ErrorMessageList();
        CellErrorMessages.Add(ColumnIndex, Errors);
      }
      Errors.Add(Error);
       * */
    }

    private void AddToolTipText(string Message)
    {
      if (Reason != EFPDataGridViewAttributesReason.ToolTip)
        return;

#if DEBUG
      if (String.IsNullOrEmpty(Message))
        throw new ArgumentNullException("Message");
#endif

      if (String.IsNullOrEmpty(ToolTipText))
        ToolTipText = Message;
      else
        ToolTipText += Environment.NewLine + Message;
    }

  #endregion

  #region Дополнительные методы

    /// <summary>
    /// Устанавливает для строки изображение ошибки и добавляет сообщение к свойству
    /// ToolTipText
    /// </summary>
    /// <param name="Message">Сообщение</param>
    public void AddRowError(string Message)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Error, Message), String.Empty);
    }

    public void AddRowError(string Message, string ColumnNames)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Error, Message), ColumnNames);
    }

    /// <summary>
    /// Устанавливает для строки изображение предупреждения (если оно не установлено уже в ошибку)
    /// и добавляет сообщение к свойству ToolTipText
    /// </summary>
    /// <param name="Message">Сообщение</param>
    public void AddRowWarning(string Message)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Warning, Message), String.Empty);
    }

    /// <summary>
    /// Устанавливает для строки изображение предупреждения (если оно не установлено уже в ошибку)
    /// и добавляет сообщение к свойству ToolTipText
    /// </summary>
    /// <param name="Message">Сообщение</param>
    /// <param name="ColumnNames"></param>
    public void AddRowWarning(string Message, string ColumnNames)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Warning, Message), ColumnNames);
    }

    /// <summary>
    /// Устанавливает для строки изображение сообщения и добавляет сообщение к свойству
    /// ToolTipText
    /// </summary>
    /// <param name="Message">Сообщение</param>
    public void AddRowInformation(string Message)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Info, Message), String.Empty);
    }

    public void AddRowInformation(string Message, string ColumnNames)
    {
      AddRowErrorMessage(new ErrorMessageItem(ErrorMessageKind.Info, Message), ColumnNames);
    }

  #endregion

  #endregion

  #region Защишенные методы

    public new void InitRow(int RowIndex, EFPDataGridViewAttributesReason Reason)
    {
      base.InitRow(RowIndex, Reason);
      PrintWithPrevious = false;
      PrintWithNext = false;
      ImageKind = EFPDataGridViewImageKind.None;
      UserImage = null;
      ToolTipText = String.Empty;

      ReadOnly = false;
      ReadOnlyMessage = "Строка предназначена только для просмотра";
      if (Reason == EFPDataGridViewAttributesReason.ReadOnly)
      {
        // TODO:
        //DataGridViewRow GridRow = ControlProvider.Control.Rows.SharedRow(RowIndex);
        //ReadOnly = (GridRow.GetState(RowIndex) & DataGridViewElementStates.ReadOnly) == DataGridViewElementStates.ReadOnly;
      }

      ControlContentVisible = true;
      CellErrorMessages.Clear();
      RowIdText = null;
    }

  #endregion
  }


  public class EFPDataTreeViewCellAttributesEventArgs : EFPDataTreeViewCellAttributesEventArgsBase
  {
  #region Конструктор

    public EFPDataTreeViewCellAttributesEventArgs(EFPDataTreeView ControlProvider)
      : base(ControlProvider)
    {
    }

  #endregion

  #region Свойства - исходные данные

    /// <summary>
    /// Индекс столбца в табличном просмотре (независимо от текущего порядка столбцов).
    /// Совпадает с DataGridViewColumn.Index
    /// </summary>
    public int ColumnIndex { get { return FColumnIndex; } }
    private int FColumnIndex;

    ///// <summary>
    ///// Объект столбца в EFPDataGridView
    ///// </summary>
    //public EFPDataTreeViewColumn Column { get { return FColumn; } }
    //private EFPDataTreeViewColumn FColumn;

    ///// <summary>
    ///// Объект столбца в DataGridView
    ///// </summary>
    //public DataGridViewColumn GridColumn { get { return Column.GridColumn; } }

    /// <summary>
    /// Имя столбца DataGridViewColumn.Name
    /// </summary>
    // TODO:
    public string ColumnName { get { return "";/*  return Column.Name; */} }

    /*
    /// <summary>
    /// Настройки вида ячейки для выравнивания и др. атрибутов
    /// </summary>
    public DataGridViewCellStyle CellStyle
    {
      get
      {
        if (FCellStyle == null)
          FCellStyle = new DataGridViewCellStyle(FTemplateCellStyle);
        return FCellStyle;
      }
    }
    private DataGridViewCellStyle FCellStyle;
     * */
    ///// <summary>
    ///// Вместо реального стиля, который можно непосредственно менять, может быть
    ///// использован шаблон стиля. В этом случае будет сгенерирован новый объект
    ///// при любом обращении к CellStyle
    ///// </summary>
    //private DataGridViewCellStyle FTemplateCellStyle;

    /// <summary>
    /// Используется в DoGetCellAttributes() после вызова обработчика, чтобы вызвавший
    /// модуль мог работать с CellStyle не задумываясь о лишнем копировании
    /// </summary>
    internal void MoveTemplateToStyle()
    {
      //if (FCellStyle == null)
      //  FCellStyle = FTemplateCellStyle;
    }

    /*
    /// <summary>
    /// Доступ к ячейке табличного просмотра.
    /// Вызов метода делает строку Unshared
    /// </summary>
    /// <returns></returns>
    public DataGridViewCell GetGridCell()
    {
      if (ColumnIndex < 0)
        return null;
      else
        return GetGridRow().Cells[ColumnIndex];
    } */

    /// <summary>
    /// Исходное (неотформатированное) значение
    /// </summary>
    public object OriginalValue { get { return FOriginalValue; } }
    private object FOriginalValue;

  #endregion

  #region Устанавливаемые свойства

    /// <summary>
    /// Форматируемое значение
    /// </summary>
    public object Value { get { return FValue; } set { FValue = value; } }
    private object FValue;

    /// <summary>
    /// Свойство должно быть установлено в true, если было применено форматирование
    /// значения
    /// </summary>
    public bool FormattingApplied { get { return FFormattingApplied; } set { FFormattingApplied = value; } }
    private bool FFormattingApplied;

    /// <summary>
    /// Отступ от левого или от правого края (в зависимости от горизонтального 
    /// выравнивания) значения ячейки
    /// </summary>
    public int IndentLevel { get { return FIndentLevel; } set { FIndentLevel = value; } }
    private int FIndentLevel;

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
    public string ToolTipText { get { return FToolTipText; } set { FToolTipText = value; } }
    private string FToolTipText;

    /// <summary>
    /// Текст всплывающей подсказки для строки.
    /// Свойство имеет значение только при Reason=ToolTip
    /// Значение может быть использовано при формировании ToolTipText, чтобы 
    /// не вычислять текст еше раз, когда подсказка для ячеки основывается на подсказке
    /// для строки
    /// </summary>
    public string RowToolTipText { get { return FRowToolTipText; } }
    private string FRowToolTipText;


    /// <summary>
    /// Если установить в false, то содержимое ячейки не будет выводиться совсем.
    /// Полезно для CheckBox'ов и кнопок, если для какой-то строки ячеек они не нужны
    /// </summary>
    public bool ContentVisible { get { return FContentVisible; } set { FContentVisible = value; } }
    private bool FContentVisible;

  #endregion

  #region Защишенные методы

    public void InitCell(EFPDataTreeViewRowAttributesEventArgs RowArgs, int ColumnIndex, /*DataGridViewCellStyle CellStyle, bool StyleIsTemplate, */object OriginalValue)
    {
      base.InitRow(RowArgs.RowIndex, RowArgs.Reason);
      FColumnIndex = ColumnIndex;
      // TODO: FColumn = ControlProvider.Columns[ColumnIndex];

      ColorType = RowArgs.ColorType;
      // TODO: if (Column.ColorType > ColorType)
      // TODO:   ColorType = Column.ColorType;

      if (RowArgs.FGrayed.HasValue)
        FGrayed = RowArgs.FGrayed.Value; // была явная установка свойства в обработчике строки
      /*
    else if (Column.Grayed)
      FGrayed = true;
    else
      FGrayed = null; // в принципе, это не нужно
        */
      TopBorder = RowArgs.TopBorder;
      BottomBorder = RowArgs.BottomBorder;

      /*
      if (Column.LeftBorder == EFPDataGridViewBorderStyle.Default)
        LeftBorder = RowArgs.LeftBorder;
      else
        LeftBorder = Column.LeftBorder;
      if (Column.RightBorder == EFPDataGridViewBorderStyle.Default)
        RightBorder = RowArgs.RightBorder;
      else
        RightBorder = Column.RightBorder;
        */
      IndentLevel = 0;
      /*
      if (StyleIsTemplate)
      {
        FTemplateCellStyle = CellStyle;
        FCellStyle = null;
      }
      else
      {
        FCellStyle = CellStyle;
        FTemplateCellStyle = null;
      } */
      FOriginalValue = OriginalValue;

      ReadOnly = false;
      ReadOnlyMessage = "Столбец предназначен только для просмотра";
      if (Reason == EFPDataGridViewAttributesReason.ReadOnly)
      {
        //DataGridViewRow GridRow = ControlProvider.Control.Rows[RowIndex]; // Unshared
        //ReadOnly = GridRow.Cells[ColumnIndex].ReadOnly;
        //  ReadOnly = ControlProvider.Control.Columns[ColumnIndex].ReadOnly;
      }

      ContentVisible = true;
      if (!RowArgs.ControlContentVisible)
      {
        /*
        DataGridViewColumn GridCol = Control.Columns[ColumnIndex];
        if (GridCol is DataGridViewCheckBoxColumn ||
          GridCol is DataGridViewButtonColumn ||
          GridCol is DataGridViewComboBoxColumn)

          ContentVisible = false;*/
      }

      FRowToolTipText = RowArgs.ToolTipText;

      ErrorMessageList CellErrors;
      if (RowArgs.CellErrorMessages.TryGetValue(ColumnIndex, out CellErrors))
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

        if (RowArgs.Reason == EFPDataGridViewAttributesReason.ToolTip)
        {
          for (int i = 0; i < CellErrors.Count; i++)
          {
            if (!String.IsNullOrEmpty(FRowToolTipText))
              FRowToolTipText += Environment.NewLine;
            FRowToolTipText += CellErrors[i];
          }
        }
      }
    }

  #endregion
  }

  #region Делегаты

  public delegate void EFPDataTreeViewRowAttributesEventHandler(object Sender,
    EFPDataTreeViewRowAttributesEventArgs Args);

  public delegate void EFPDataTreeViewCellAttributesEventHandler(object Sender,
    EFPDataTreeViewCellAttributesEventArgs Args);

  #endregion

#endif
  #endregion

  #region Режимы сохранения текущих строк

  /// <summary>
  /// Возможные способы сохранения / восстановления текущей позиции в древовидном 
  /// просмотре (свойство <see cref="EFPDataTreeView.SelectedNodesMode"/>)
  /// </summary>
  public enum EFPDataTreeViewSelectedNodesMode
  {
    /// <summary>
    /// Режим по умолчанию. Текущая строка не может быть восстановлена
    /// </summary>
    None,

    /// <summary>
    /// Сохранение индексов узлов.
    /// То есть, для узла запоминается индекс узла верхнего узла, в нем - индекс дочернего узла, в нем индекс узла 3 уровня - и т.д.
    /// Обычно подходит для табличных просмотров, не связанных с источником данных.
    /// </summary>
    Position,

    /// <summary>
    /// Сохранение ссылок на объекты <see cref="System.Data.DataRow"/>.
    /// Подходит для просмотров, связанных с <see cref="IDataTableTreeModel"/> при условии, что
    /// обновление таблицы не приводит к замене строк. Зато таблица может не иметь ключевого поля.
    /// </summary>
    DataRow,

    /// <summary>
    /// Режим для просмотров, связанных с <see cref="IDataTableTreeModel"/> при условии, что таблица имеет первичный ключ
    /// </summary>
    PrimaryKey,
  }

  /// <summary>
  /// Класс для сохранения текущей позиции и выбранных строк/столбцов в древовидном просмотре
  /// (свойство <see cref="EFPDataTreeView.Selection"/>)
  /// Не содержит открытых полей.
  /// </summary>
  public class EFPDataTreeViewSelection
  {
    /// <summary>
    /// Выбранные узлы просмотра
    /// </summary>
    internal object SelectedNodes;

    /// <summary>
    /// Текущая строка
    /// </summary>
    internal object CurrentNode;

    /// <summary>
    /// Режим "Выбраны все узлы". В этом случае выбранные узлы не запоминаются
    /// </summary>
    internal bool SelectAll;

    /// <summary>
    /// true, если свойство TreeViewAdv.Model было установлено
    /// </summary>
    internal bool ModelExists;
  }

  #endregion

  /// <summary>
  /// Древовидный просмотр с поддержкой столбцов
  /// </summary>
  public class EFPDataTreeView : EFPTreeViewAdv, IEFPDataView
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPDataTreeView(EFPBaseProvider baseProvider, TreeViewAdv control)
      : base(baseProvider, control)
    {
      Init();
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    public EFPDataTreeView(IEFPControlWithToolBar<TreeViewAdv> controlWithToolBar)
      : base(controlWithToolBar)
    {
      Init();
    }

    private void Init()
    {
      //GetRowAttributesArgs = new EFPDataTreeViewRowAttributesEventArgs(this);
      //GetCellAttributesArgs = new EFPDataTreeViewCellAttributesEventArgs(this);

      _CurrentOrderIndex = 0;
      _OrderChangesToRefresh = false;
      _AutoSort = false;
      _DocumentProperties = BRReport.AppDefaultDocumentProperties.Clone();

      if (!DesignMode)
      {
        Control.VisibleChanged += new EventHandler(Control_VisibleChanged);
        Control.KeyDown += new KeyEventHandler(Control_KeyDown);
        Control.MouseDown += new MouseEventHandler(Control_MouseDown);
        Control.KeyDown += new KeyEventHandler(Control_KeyDown);
        Control.ModelChanged += Control_ModelChanged;
        Control.SizeChanged += Control_SizeChanged;
      }
      Control.UseColumns = true;
      UseIdle = true; // автоподбор размеров столбцов
    }

    #endregion

    #region Изменения в ProviderState

    /// <summary>
    /// Инициализирует свойство <see cref="BRDocumentProperties.Title"/> и устанавливает размеры столбцов с заполнением по ширине.
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();

      if (String.IsNullOrEmpty(_DocumentProperties.Title))
        _DocumentProperties.Title = WinFormsTools.GetControlText(Control);

      PerformAutoResizeColumns();
    }

    /// <summary>
    /// Вызов <see cref="ResetDataReorderHelper()"/>.
    /// </summary>
    protected override void OnDetached()
    {
      base.OnDetached();
      ResetDataReorderHelper();
    }

    /// <summary>
    /// Вызов <see cref="ResetDataReorderHelper()"/>.
    /// </summary>
    protected override void OnDisposed()
    {
      ResetDataReorderHelper();
      base.OnDisposed();
    }

    #endregion

    #region Обработчики событий

    ///// <summary>
    ///// Этот метод вызывается после инициализации команд локального меню.
    ///// Добавляем обработчики, которые должны быть в конце цепочки.
    ///// </summary>
    //protected override void OnAfterPrepareCommandItems()
    //{
    //  // Мы должны присоединить обработчик CellBeginEdit после пользовательского, т.к.
    //  // нам нужно проверить свойство Cancel
    //  //Control.CellBeginEdit += new DataGridViewCellCancelEventHandler(Control_CellBeginEdit2);
    //  //Control.CellEndEdit += new DataGridViewCellEventHandler(Control_CellEndEdit2);
    //}

    #region CurrentCellChanged

    /*
    void Control_CurrentCellChanged(object Sender, EventArgs Args)
    {
      try
      {
        OnCurrentCellChanged();
      }
      catch (Exception e)
      {
        DebugTools.ShowException(e, "Control_CurrentCellChanged");
      }
    }

    protected virtual void OnCurrentCellChanged()
    {
      if (Control.Visible && (!Inside_Control_DataBindingComplete) && Control.CurrentCellAddress.X >= 0 && FVisibleHasChanged && MouseOrKeyboardFlag)
        FCurrentColumnIndex = Control.CurrentCellAddress.X;

      // При смене текущего столбца отключаем поиск по первым буквам
      if (CurrentIncSearchColumn != null)
      {
        if (CurrentIncSearchColumn.GridColumn.Index != Control.CurrentCellAddress.X)
          CurrentIncSearchColumn = null;
      }

    }
     * */

    #endregion

    #region VisibleChanged

    /// <summary>
    /// Только после установки этого флага разрешается реагировать на смену ячейки
    /// </summary>
    public bool VisibleHasChanged { get { return _VisibleHasChanged; } }
    private bool _VisibleHasChanged = false;

#pragma warning disable 0414 // временно
    /// <summary>
    /// Этот флажок устанавливается в true, когда нажата мышь или клавиша, что
    /// означает, что просмотр есть на экране, и смена текущей ячейки выполнена
    /// пользователем
    /// </summary>
    private bool _MouseOrKeyboardFlag;
#pragma warning restore 0414

    internal void Control_VisibleChanged(object sender, EventArgs args)
    {
      // !!! Модификатор internal нужен для EFPDataTreeViewCommandItems

      try
      {
        _VisibleHasChanged = Control.Visible;
        _MouseOrKeyboardFlag = false;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Control_VisibleChanged");
      }
    }

    #endregion

    #region KeyDown

    void Control_KeyDown(object sender, KeyEventArgs args)
    {
      try
      {
        _MouseOrKeyboardFlag = true;
        //DoControl_KeyDown2(Args);
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
      { /*
        if (Control.AreAllCellsSelected(false))
          return;

        DataGridView.HitTestInfo ht = Control.HitTest(Args.X, Args.Y);
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
         * */
      }
    }

    #endregion

    #region ModelChanged

    void Control_ModelChanged(object sender, EventArgs args)
    {
      ResetDataReorderHelper();
    }

    #endregion

    #endregion

    #region Другие свойства

    /// <summary>
    /// Размеры и масштабные коэффициенты для древовидного просмотра.
    /// Не предназначено для использования в прикладном коде
    /// </summary>
    public EFPDataTreeViewMeasures Measures
    {
      get
      {
        if (_Measures == null)
          _Measures = new EFPDataTreeViewMeasures(this);
        return _Measures;
      }
    }
    private EFPDataTreeViewMeasures _Measures;

    IEFPGridControlMeasures IEFPDataView.Measures { get { return Measures; } }


    /*
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
    } */


    bool IEFPDataView.UseRefresh { get { return CommandItems.UseRefresh; } }

    #endregion

    #region Управление поведением просмотра

    /// <summary>
    /// Блокировка сортировки для всех столбцов
    /// </summary>
    internal void DisableOrdering() // Сделать public, если сортировка будет разрешена
    {
      for (int i = 0; i < Control.Columns.Count; i++)
        Control.Columns[i].Sortable = false;

      // 03.06.2011
      // Для столбцов, установленных в Orders, разрешаем сортировку
      //if (OrderCount > 0)
      //{
      //for (int i = 0; i < Orders.Count; i++)
      //{
      /*
      EFPDataGridViewColumn Column = Control.NodeControls[Orders[i].SortInfo.ColumnName];
      if (Column != null)
        Column.GridColumn.SortMode = DataGridViewColumnSortMode.Programmatic;*/
      //}
      //}
    }

    /// <summary>
    /// Установить свойство <see cref="InteractiveControl.EditEnabled"/> (инвертированное значение) для всех столбцов просмотра.
    /// Метод используется, когда требуется разрешить редактирование "по месту" 
    /// только для некоторых столбцов, а большая часть столбцов не редактируется.
    /// </summary>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetColumnsReadOnly(bool value)
    {
      for (int i = 0; i < Control.NodeControls.Count; i++)
      {
        InteractiveControl ic = Control.NodeControls[i] as InteractiveControl;
        if (ic != null)
          ic.EditEnabled = !value; // 17.11.2017
      }
    }


    /// <summary>
    /// Установить свойство <see cref="InteractiveControl.EditEnabled"/> (инвертированное значение) для заданных столбцов просмотра.
    /// Метод используется, когда требуется разрешить редактирование "по месту" 
    /// только для некоторых столбцов, а большая часть столбцов не редактируется.
    /// </summary>
    /// <param name="columnNames">Список имен столбцов через запятую. Если столбец с заданным именем не найден, то ошибка не выдается</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetColumnsReadOnly(string columnNames, bool value)
    {
      string[] a = columnNames.Split(',');
      for (int i = 0; i < Control.NodeControls.Count; i++)
      {
        InteractiveControl ic = Control.NodeControls[i] as InteractiveControl;
        if (ic != null)
        {
          if (Array.IndexOf<string>(a, ic.DataPropertyName) >= 0)
            ic.EditEnabled = !ReadOnly;
        }
      }
    }

    bool IEFPDataView.MultiSelect
    {
      get { return Control.SelectionMode != TreeViewAdvSelectionMode.Single; }
      set { Control.SelectionMode = value ? TreeViewAdvSelectionMode.Multi : TreeViewAdvSelectionMode.Single; }
    }

    #endregion

    #region Список столбцов

    /// <summary>
    /// Дополнительная информация о столбцах, альтернативные методы добавления
    /// столбцов
    /// </summary>
    public EFPDataTreeViewColumns Columns
    {
      get
      {
        if (_Columns == null)
          _Columns = CreateColumns();
        return _Columns;
      }
    }
    private EFPDataTreeViewColumns _Columns;

    IEFPDataViewColumns IEFPDataView.Columns { get { return Columns; } }

    /// <summary>
    /// Создает объект <see cref="EFPDataTreeViewColumns"/>.
    /// </summary>
    /// <returns>Новый объект</returns>
    protected virtual EFPDataTreeViewColumns CreateColumns()
    {
      return new EFPDataTreeViewColumns(this);
    }

    //internal protected virtual void AddGridProducerToolTip(EFPDataTreeViewColumn Column, List<string> a, int RowIndex)
    //{
    //}

    /// <summary>
    /// Получить массив видимых столбцов в просмотре в порядке вывода на экран
    /// как объекты <see cref="EFPDataTreeViewColumn"/>
    /// </summary>
    public EFPDataTreeViewColumn[] VisibleColumns
    {
      get
      {
        List<EFPDataTreeViewColumn> lst = new List<EFPDataTreeViewColumn>();
        for (int i = 0; i < Columns.Count; i++)
        {
          if (Columns[i].TreeColumn.IsVisible)
            lst.Add(Columns[i]);
        }
        return lst.ToArray();
      }
    }

    IEFPDataViewColumn[] IEFPDataView.VisibleColumns { get { return VisibleColumns; } }

    bool IEFPDataView.InitColumnConfig(EFPDataGridViewConfigColumn configColumn)
    {
      EFPDataTreeViewColumn col = Columns[configColumn.ColumnName];
      if (col == null)
        return false;

      configColumn.Width = col.TreeColumn.Width;
      configColumn.FillMode = col.FillWeight > 0;
      configColumn.FillWeight = col.FillWeight;

      return true;
    }

    #endregion

    #region Автоматический подбор размеров столбцов по ширине

    /// <summary>
    /// Установка флага означает необходимость определения размера столбцов в событии Idle.
    /// </summary>
    internal bool AutoResizeColumnsNeeded;

    private void PerformAutoResizeColumns()
    {
      AutoResizeColumnsNeeded = false;

      int fixWidth = 0; // Полная ширина всех столбцов без автоподбора.
      int autoFillWeight = 0; // Полный процент ширины столбцов
      for (int i = 0; i < Columns.Count; i++)
      {
        if (Columns[i].FillWeight == 0)
          fixWidth += Columns[i].TreeColumn.Width;
        else
        {
          int mw = Columns[i].TreeColumn.MinColumnWidth;
          if (mw < 1)
            mw = 1;
          autoFillWeight += Columns[i].FillWeight;
        }
      }

      if (autoFillWeight > 0)
      {
        int wantedAutoWidth = Control.ClientSize.Width - fixWidth;
        if (wantedAutoWidth <= 0)
          return; // ??

        double k = (double)wantedAutoWidth / (double)autoFillWeight;

        for (int i = 0; i < Columns.Count; i++)
        {
          if (Columns[i].FillWeight > 0)
          {
            int w = (int)(Columns[i].FillWeight * k);
            if (w < 1)
              w = 1;
            if (w < Columns[i].TreeColumn.MinColumnWidth)
              w = Columns[i].TreeColumn.MinColumnWidth;
            if (Columns[i].TreeColumn.MaxColumnWidth > 0 && w > Columns[i].TreeColumn.MaxColumnWidth)
              w = Columns[i].TreeColumn.MaxColumnWidth;
            Columns[i].TreeColumn.Width = w;
          }
        }
      }
    }

    /// <summary>
    /// Автоматический подбор ширины столбцов
    /// </summary>
    public override void HandleIdle()
    {
      base.HandleIdle();
      if (AutoResizeColumnsNeeded)
        PerformAutoResizeColumns();
    }

    private void Control_SizeChanged(object sender, EventArgs args)
    {
      if (ProviderState == EFPControlProviderState.Attached)
        AutoResizeColumnsNeeded = true;
    }


    #endregion

    #region GridProducer

    /// <summary>
    /// Генератор столбцов таблицы. Если задан, то в локальном меню доступны
    /// команды настройки столбцов таблицы, при условии, что также установлено свойство <see cref="EFPControlBase.ConfigSectionName"/>.
    /// </summary>
    public IEFPGridProducer GridProducer
    {
      get { return _GridProducer; }
      set
      {
        if (value != null)
          value.SetReadOnly();
        CheckHasNotBeenCreated();

        _GridProducer = value;
      }
    }
    private IEFPGridProducer _GridProducer;

    #endregion

    #region Свойство CurrentGridConfig

    /// <summary>
    /// Выбранная настройка табличного просмотра.
    /// Если свойство <see cref="GridProducer"/> не установлено, должен быть задан обработчик <see cref="CurrentConfigChanged"/>,
    /// который выполнит реальную настройку просмотра.
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
            CancelEventArgs args = new CancelEventArgs();
            args.Cancel = false;
            OnCurrentConfigChanged(args);
            if ((!args.Cancel) && (GridProducer != null))
            {
              this.Columns.Clear();
              //int nCols1 = Control.Columns.Count;
              GridProducer.InitTreeView(this, CurrentConfigHasBeenSet);
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
      OnGridProducerPostInit();

      if (ProviderState == EFPControlProviderState.Attached)
        PerformAutoResizeColumns();
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

    #region Доступ к выбранным ячейкам независимо от типа данных

    /// <summary>
    /// Возвращает true, если в просмотре есть хотя бы одна выбранная ячейка
    /// </summary>
    public bool HasSelectedRows
    {
      get
      {
        // ???
        return Control.SelectedNode != null;
      }
    }

    /// <summary>
    /// Вспомогательное свойство. Возвращает число выбранных строк в просмотре.
    /// </summary>
    public int SelectedRowCount
    {
      get
      {
        return Control.SelectedNodes.Count;
      }
    }

    ///// <summary>
    ///// Расширение свойства SelectedRows. Вместо объекта DataGridViewRow 
    ///// используются индексы строк
    ///// </summary>
    //public int[] SelectedRowIndices
    //{
    //  get
    //  {
    //    return new int[0];
    //    /*
    //    DataGridViewRow[] Rows = SelectedGridRows;
    //    int[] res = new int[Rows.Length];
    //    for (int i = 0; i < Rows.Length; i++)
    //    {
    //      if (Rows[i] == null)
    //        res[i] = -1;
    //      else
    //        res[i] = Rows[i].Index;
    //    }
    //    return res;
    //     * */
    //  }
    //  set
    //  {
    //    /*
    //    DataGridViewRow[] Rows = new DataGridViewRow[value.Length];
    //    for (int i = 0; i < value.Length; i++)
    //    {
    //      if (value[i] >= 0 && value[i] < Control.Rows.Count)
    //        Rows[i] = Control.Rows[value[i]];
    //    }
    //    SelectedGridRows = Rows;
    //     * */
    //  }
    //}

    ///// <summary>
    ///// Индекс текущего узла дерева
    ///// </summary>
    //public int CurrentRowIndex
    //{
    //  get
    //  {
    //    if (Control.SelectedNode == null)
    //      return -1;
    //    else
    //      return Control.SelectedNode.Index;
    //  }
    //  set
    //  {
    //    /*
    //    if (Control.Rows.Count == 0)
    //    {
    //      SavedRowIndex = value;
    //      return;
    //    }
    //    if (value >= 0 && value < Control.Rows.Count)
    //      CurrentGridRow = Control.Rows[value];
    //    SavedRowIndex = -1;
    //     * */
    //  }
    //}

#pragma warning disable 0414 // временно
    private int _SavedRowIndex = -1;
#pragma warning restore 0414

#if XXX
    public void SelectRowIndex(int RowIndex)
    {
      /*
      if (Control.Rows.Count == 0)
      {
        SavedRowIndex = RowIndex;
        return;
      }
      if (RowIndex >= 0 && RowIndex < Control.Rows.Count)
        SelectGridRow(Control.Rows[RowIndex]);
      SavedRowIndex = -1;
       * */
    }
#endif

#if XXX // В TreeViewAdv всегда выбирается строка целиком, а не ячейка
    /*
    public int[] SelectedColumnIndices
    {
      get
      {
        DataGridViewColumn[] Cols = SelectedGridColumns;
        int[] Indices = new int[Cols.Length];
        for (int i = 0; i < Cols.Length; i++)
          Indices[i] = Cols[i].Index;
        return Indices;
      }
    }
      */

    /// <summary>
    /// Получить выделенные столбцы таблицы. В режиме выделения
    /// ячейки возвращается массив из одного элемента. В режиме выбора строк
    /// возвращается массив всех видимых столбцов
    /// </summary>
    public EFPDataGridViewColumn[] SelectedColumns
    {
      get
      {
        return null;
        /*
        DataGridViewColumn[] Cols = SelectedGridColumns;
        EFPDataGridViewColumn[] Cols2 = new EFPDataGridViewColumn[Cols.Length];
        for (int i = 0; i < Cols.Length; i++)
          Cols2[i] = Columns[Cols[i]];
        return Cols2;
         * */
      }
    }

    /// <summary>
    /// Возвращает true, если ячейка является выбранной, находится в выбранной 
    /// целиком строке или столбце. 
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <param name="columnIndex"></param>
    /// <returns></returns>
    public bool IsCellSelected(int rowIndex, int columnIndex)
    {
      return false;
      /*
      if (RowIndex < 0 || RowIndex >= Control.RowCount)
        return false;
      if (ColumnIndex < 0 || ColumnIndex >= Control.Columns.Count)
        return false;

      if (!Control.Rows.SharedRow(RowIndex).Visible)
        return false;
      if (!Control.Columns[ColumnIndex].Visible)
        return false;

      if (Control.AreAllCellsSelected(false))
        return true;
      if (Control.SelectedCells != null && Control.SelectedCells.Count > 0)
      {
        DataGridViewCell Cell = Control[ColumnIndex, RowIndex];
        return Control.SelectedCells.Contains(Cell);
      }
      if (Control.SelectedRows != null && Control.SelectedRows.Count > 0)
      {
        DataGridViewRow Row = Control.Rows[RowIndex];
        return Control.SelectedRows.Contains(Row);
      }
      if (Control.SelectedColumns != null && Control.SelectedColumns.Count > 0)
      {
        DataGridViewColumn Col = Control.Columns[ColumnIndex];
        return Control.SelectedColumns.Contains(Col);
      }

      return Control.CurrentCellAddress.X == ColumnIndex && Control.CurrentCellAddress.Y == RowIndex;
       * */
    }
#endif

    /// <summary>
    /// Вспомогательное свойство только для чтения. Возвращает true, если свойство
    /// CurrentRow установлено (в просмотре есть текущая строка) и она является
    /// единственной выбранной строкой
    /// </summary>
    public bool IsCurrentRowSingleSelected
    {
      get
      {
        return Control.SelectedNodes.Count == 1;
      }
    }

    /// <summary>
    /// Вспомогательный метод
    /// Возвращает true, если в просмотре есть только одна выбранная (текущая) строка.
    /// Если в просмотре нет текущей строки или выбрано больше одной строки, то
    /// выдается соответствующее сообщение с помошью <see cref="EFPApp.ShowTempMessage(string)"/>.
    /// Возвращаемое значение соответствует свойству <see cref="IsCurrentRowSingleSelected"/>.
    /// Используйте метод для упрощения реализации методов редактирования или команд
    /// локального меню, если они должны работать с единственной строкой. После
    /// вызова метода используйте одно из свойств CurrentXxxRow для доступа к строке
    /// </summary>
    /// <returns></returns>
    public bool CheckSingleRow()
    {
      if (Control.SelectedNode == null)
      {
        EFPApp.ShowTempMessage("В просмотре нет выбранной строки");
        return false;
      }
      if (Control.SelectedNodes.Count > 1)
      {
        EFPApp.ShowTempMessage("В просмотре выбрано больше одной строки");
        return false;
      }

      return true;
    }

    #endregion

    #region Доступ к выбранным ячейкам для источника данных DataView или DataTable

    /// <summary>
    /// Если модель реализует интерфейс <see cref="IDataTableTreeModel"/>, то возвращается таблица модели.
    /// Иначе возвращается null.
    /// </summary>
    public DataTable SourceAsDataTable
    {
      get
      {
        IDataTableTreeModel model = Control.Model as IDataTableTreeModel;
        if (model == null)
          return null;
        else
          return model.Table;
      }
    }

    /// <summary>
    /// Если модель реализует интерфейс <see cref="IDataTableTreeModel"/>, то возвращается <see cref="System.Data.DataView"/> модели или <see cref="System.Data.DataTable.DefaultView"/>.
    /// Иначе возвращается null.
    /// </summary>
    public DataView SourceAsDataView
    {
      get
      {
        IDataTableTreeModel model = Control.Model as IDataTableTreeModel;
        if (model == null)
          return null;
        else
        {
          if (model.DataView != null)
            return model.DataView;
          if (model.Table != null)
            return model.Table.DefaultView; // 31.10.2017
          else
            return null;
        }
      }
    }

    /// <summary>
    /// Возвращает <see cref="TreeViewAdv.Model"/> как <see cref="IDataTableTreeModel"/> или выбрасывает исключение,
    /// если модель не установлена или имеет неподходящий тип.
    /// </summary>
    /// <returns>Модель, не может быть null</returns>
    protected IDataTableTreeModel GetDataTableModelWithCheck()
    {
      if (Control.Model == null)
        throw new NullReferenceException("Свойство TreeViewAdv.Model не установлено");

      IDataTableTreeModel model = Control.Model as IDataTableTreeModel;
      if (model == null)
        throw new NullReferenceException("Присоединенная модель не реализует IDataTableTreeModel");

      return model;
    }

    /*
    /// <summary>
    /// Получить строку DataRow таблицы, связанную с заданным объектом строки
    /// </summary>
    /// <param name="Row">Объект строки</param>
    /// <returns>Строка в таблице данных</returns>
    public static DataRow GetDataRow(DataGridViewRow Row)
    {
      if (Row == null)
        return null;

      // Может вылазить исключение при попытке обращения к Row.DataBoundItem, если
      // оно было связано с DataViewRow для последней строки, когда произошло ее 
      // удаление 
      object Item;
      try
      {
        Item = Row.DataBoundItem;
      }
      catch
      {
        return null;
      }
      if (Item is DataRowView)
        return ((DataRowView)Item).Row;
      if (Item is DataRow) // ?? так бывает
        return (DataRow)Item;
      return null;
    }
      */

    /*
    /// <summary>
    /// Более предпочтительный способ получения строки DataRow по номеру строки в
    /// табличном просмотре. Не делает строку Unshared, т.к. не обращается к
    /// объекту DataGridViewRow
    /// Статический вариант метода
    /// !!! Не работает, если выполнена автоматическая сортировка по какому-нибудь столбцу
    /// </summary>
    /// <param name="Control">Табличный просмотр, не обязательно имеющий DocGridHandler</param>
    /// <param name="RowIndex">Номер строки</param>
    /// <returns>Объект DataRow или null при любой ошибке</returns>
    public static DataRow GetDataRow(DataGridView Control, int RowIndex)
    {
      //DataGridViewRow GridRow = MainGrid.Rows.SharedRow(RowIndex);
      //if (GridRow.DataBoundItem is DataRowView)
      //  return ((DataRowView)(GridRow.DataBoundItem)).Row;
      //if (GridRow.DataBoundItem is DataRow)
      //  return (DataRow)(GridRow.DataBoundItem);

      if (Control.DataSource is DataView)
      {
        if (RowIndex < 0 || RowIndex >= ((DataView)(Control.DataSource)).Count)
          return null;
        return ((DataView)(Control.DataSource))[RowIndex].Row;
      }
      if (Control.DataSource is DataTable)
      {
        if (RowIndex < 0 || RowIndex >= ((DataTable)(Control.DataSource)).Rows.Count)
          return null;
        return ((DataTable)(Control.DataSource)).DefaultView[RowIndex].Row;
      }
      return null;
    } */

    /*
    /// <summary>
    /// Более предпочтительный способ получения строки DataRow по номеру строки в
    /// табличном просмотре. Не делает строку Unshared, т.к. не обращается к
    /// объекту DataGridViewRow
    /// Нестатический вариант
    /// </summary>
    /// <param name="RowIndex">Номер строки</param>
    /// <returns>Объект DataRow или null при любой ошибке</returns>
    public DataRow GetDataRow(int RowIndex)
    {
      return EFPDataGridView.GetDataRow(Control, RowIndex);
    }
      */

    //public Int32[] SelectedIds


    /// <summary>
    /// Получить или установить выбранные строки в просмотре.
    /// Расширяет реализацию свойства <see cref="TreeViewAdv.SelectedNodes"/>, преобразуя <see cref="TreeNodeAdv.Tag"/>
    /// в строки таблицы данных <see cref="System.Data.DataRow"/>.
    /// В возвращаемые строки не входят дочерние дочерние узлы, которые не были выбраны.
    /// </summary>
    public DataRow[] SelectedDataRows
    {
      get
      {
        return GetSelectedDataRows(EFPGetTreeNodesMode.NoChildren);
      }
      set
      {
        if (value == null)
          return;
        if (value.Length == 0)
          return;

        IDataTableTreeModel model = GetDataTableModelWithCheck();

        Control.BeginUpdate();
        try
        {
          Control.ClearSelection();

          for (int i = 0; i < value.Length; i++)
          {
            TreePath treePath = model.TreePathFromDataRow(value[i]);

            TreeNodeAdv node = Control.FindNode(treePath, true);
            if (node != null)
              node.IsSelected = true;
          }

          EnsureSelectionVisible();
        }
        finally
        {
          Control.EndUpdate();
        }
      }
    }

    /// <summary>
    /// Получить строки <see cref="System.Data.DataRow"/> таблицы для выбранных узлов
    /// </summary>
    /// <param name="mode">Режим включения дочерних строк</param>
    /// <returns>Массив строк</returns>
    public DataRow[] GetSelectedDataRows(EFPGetTreeNodesMode mode)
    {
      object[] tags = base.GetSelectedNodeTags(mode);
      List<DataRow> lst = new List<DataRow>(tags.Length);
      for (int i = 0; i < tags.Length; i++)
      {
        DataRow row = tags[i] as DataRow;
        if (row != null)
          lst.Add(row);
      }

      return lst.ToArray();
    }


    /// <summary>
    /// Расширение свойства <see cref="TreeViewAdv.SelectedNode"/> (чтение и установка текущей строки).
    /// В качестве значения используется строка таблицы данных (объект <see cref="System.Data.DataRow"/>).
    /// Просмотр должен быть присоединен к модели, реализующей интерфейс <see cref="IDataTableTreeModel"/>.
    /// </summary>
    public DataRow CurrentDataRow
    {
      get
      {
        if (Control.SelectedNode == null)
          return null;
        else
          return Control.SelectedNode.Tag as DataRow;
      }
      set
      {
        if (value == null)
          return;

        IDataTableTreeModel model = GetDataTableModelWithCheck();
        TreePath treePath = model.TreePathFromDataRow(value);
        TreeNodeAdv node = Control.FindNode(treePath, true);
        if (node != null)
          Control.SelectedNode = node;

        EnsureSelectionVisible();
      }
    }

    /// <summary>
    /// Расширение свойств <see cref="TreeViewAdv.SelectedNodes"/> и <see cref="SelectedDataRows"/>.
    /// В качестве текущих позиций запоминаются значения ключевых полей в <see cref="System.Data.DataTable"/>.
    /// Первый индекс двумерного массива соответствует количеству строк.
    /// Второй индекс соответствует количеству полей в <see cref="System.Data.DataTable.PrimaryKey"/>.
    /// (обычно равно 1).
    /// Свойство возвращает null, если таблица не присоединена к модели, реализующей <see cref="IDataTableTreeModel"/>,
    /// или таблица не имеет первичного ключа.
    /// </summary>
    public object[,] SelectedDataRowKeys
    {
      get
      {
        DataRow[] rows = SelectedDataRows;
        if (rows.Length == 0)
          return null;

        DataTable table = rows[0].Table;
        if (table.PrimaryKey == null || table.PrimaryKey.Length == 0)
          return null;
        return DataTools.GetPrimaryKeyValues(table, rows);
      }
      set
      {
        if (value == null)
          return;
        if (value.GetLength(0) == 0)
          return;
        DataTable table = SourceAsDataTable;
        if (table == null)
          throw new InvalidOperationException("Control.Model не реализует интерфейс IDataTableTreeModel");
        DataRow[] rows = DataTools.GetPrimaryKeyRows(table, value);
        SelectedDataRows = rows;
      }
    }

    /// <summary>
    /// Расширение свойств <see cref="TreeViewAdv.SelectedNodes"/> и <see cref="SelectedDataRows"/>.
    /// текущей строки с помощью значений ключевых полей в <see cref="System.Data.DataTable"/>.
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
          throw new InvalidOperationException("Grid.DataSource не является DataTable");
        if (value == null)
          return;
        DataRow row = table.Rows.Find(value);
        if (row == null)
          return;
        CurrentDataRow = row;
      }
    }

#if XXX
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
        string[] flds = DataTools.GetDataViewSortFieldNames(dv.Sort);
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
        List<int> RowIndices = new List<int>();
        for (int i = 0; i < nRows; i++)
        {
          for (int j = 0; j < Keys.Length; j++)
            Keys[j] = value[i, j];
          int idx = dv.Find(Keys);
          if (idx > 0)
            RowIndices.Add(idx);
        }

        SelectedRowIndices = RowIndices.ToArray();
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
        string[] flds = DataTools.GetDataViewSortFieldNames(dv.Sort);
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
#endif

    #endregion

    #region Сохранение / восстановление выбранных строк

    /// <summary>
    /// Режим сохранения выбранных строк свойствами <see cref="SelectedNodesObject"/> и <see cref="CurrentNodeObject"/>.
    /// В отличие от <see cref="EFPDataGridView"/>, значение свойства определяется исключительно присоединенной моделью.
    /// Свойство нельзя устанавливать.
    /// </summary>
    public EFPDataTreeViewSelectedNodesMode SelectedNodesMode
    {
      get
      {
        if (Control.Model is IDataTableTreeModel)
        {
          DataTable table = ((IDataTableTreeModel)(Control.Model)).Table;
          if (table.PrimaryKey.Length == 0)
            return EFPDataTreeViewSelectedNodesMode.DataRow;
          else
            return EFPDataTreeViewSelectedNodesMode.PrimaryKey;
        }
        if (Control.Model != null)
          return EFPDataTreeViewSelectedNodesMode.Position;
        return EFPDataTreeViewSelectedNodesMode.None;
      }
    }


    /// <summary>
    /// Сохранение и восстановление выбранных узлов просмотра в виде одного объекта,
    /// в зависимости от свойства <see cref="SelectedNodesMode"/>
    /// </summary>
    public virtual object SelectedNodesObject
    {
      get
      {
        try
        {
          switch (SelectedNodesMode)
          {
            case EFPDataTreeViewSelectedNodesMode.Position:
              return base.SelectedNodePositions;
            case EFPDataTreeViewSelectedNodesMode.DataRow:
              return SelectedDataRows;
            case EFPDataTreeViewSelectedNodesMode.PrimaryKey:
              return SelectedDataRowKeys;
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
        switch (SelectedNodesMode)
        {
          case EFPDataTreeViewSelectedNodesMode.Position:
            SelectedNodePositions = value as EFPTreeNodePosition[];
            break;
          case EFPDataTreeViewSelectedNodesMode.DataRow:
            SelectedDataRows = value as DataRow[];
            break;
          case EFPDataTreeViewSelectedNodesMode.PrimaryKey:
            SelectedDataRowKeys = value as object[,];
            break;
        }
      }
    }

    /// <summary>
    /// Сохранение и восстановление текущей (одной) строки просмотра.
    /// Тип свойства зависит от режима <see cref="SelectedNodesMode"/>.
    /// </summary>
    public virtual object CurrentNodeObject
    {
      get
      {
        try
        {
          switch (SelectedNodesMode)
          {
            case EFPDataTreeViewSelectedNodesMode.Position:
              return CurrentNodePosition;
            case EFPDataTreeViewSelectedNodesMode.DataRow:
              return CurrentDataRow;
            case EFPDataTreeViewSelectedNodesMode.PrimaryKey:
              return CurrentDataRowKeys;
          }
        }
        catch
        {
        }
        return null;
      }
      set
      {
        switch (SelectedNodesMode)
        {
          case EFPDataTreeViewSelectedNodesMode.Position:
            if (value is EFPTreeNodePosition)
              CurrentNodePosition = (EFPTreeNodePosition)value;
            break;
          case EFPDataTreeViewSelectedNodesMode.DataRow:
            CurrentDataRow = value as DataRow;
            break;
          case EFPDataTreeViewSelectedNodesMode.PrimaryKey:
            if (value is Int32)
              CurrentDataRowKeys = (object[])value;
            break;
        }
      }
    }

    /// <summary>
    /// Сохранение и восстановление текущих строк и столбца табличного просмотра.
    /// Режим сохранения строк определяется свойством <see cref="SelectedNodesMode"/>.
    /// Значение включает в себя: список выделенных
    /// строк <see cref="SelectedNodesObject"/>, текущую строку <see cref="CurrentNodeObject"/>.
    /// Не содержит информации о выбранном столбце, так как выбирается всегда строка целиком.
    /// </summary>
    public EFPDataTreeViewSelection Selection
    {
      get
      {
        EFPDataTreeViewSelection res = new EFPDataTreeViewSelection();
        res.ModelExists = (Control.Model != null);

        res.SelectAll = this.AreAllNodesSelected;
        if (!res.SelectAll)
          res.SelectedNodes = SelectedNodesObject;
        res.CurrentNode = CurrentNodeObject;

        return res;
      }
      set
      {
        if (!value.ModelExists)
          return;

        CurrentNodeObject = value.CurrentNode;
        if (value.SelectAll)
          Control.SelectAllNodes();
        else
          SelectedNodesObject = value.SelectedNodes;
      }
    }

    /// <summary>
    /// Принудительное обновление просмотра
    /// </summary>
    public override void PerformRefresh()
    {
      if (Control.IsDisposed)
        return;

      Control.BeginUpdate();
      try
      {
        EFPDataTreeViewSelection oldSel = Selection;

        // Вызов пользовательского события
        OnRefreshData(EventArgs.Empty);

        //IncSearchDataView = null; // больше недействителен (???)

        try
        {
          Selection = oldSel;
        }
        catch (Exception e)
        {
          EFPApp.MessageBox("Возникла ошибка при восстановлении выбранных строк при обновлении таблицы. " +
            e.Message, "Ошибка табличного просмотра", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
      finally
      {
        Control.EndUpdate();
      }
    }

    #endregion

    #region Текущий столбец

    // TreeViewAdv не поддерживает понятие "Текущий столбец".
    // Нажатие клавиши "F2" начинает редактирование первого попавшегося столбца

    int IEFPDataView.CurrentColumnIndex { get { return -1; } }

    string IEFPDataView.CurrentColumnName { get { return String.Empty; } }

    #endregion

    #region Видимые столбцы


    /*
    /// <summary>
    /// Получить массив видимых столбцов в просмотре в порядке вывода на экран
    /// Объекты EFPDataGridViewColumn
    /// </summary>
    public EFPDataTreeViewColumn[] VisibleColumns
    {
      get
      {
        DataTreeViewAdvColumn[] cols = VisibleGridColumns;
        EFPDataGridViewColumn[] a = new EFPDataGridViewColumn[cols.Length];
        for (int i = 0; i < cols.Length; i++)
          a[i] = Columns[cols[i]];
        return a;
        return Columns.ToArray();
      }
    }
         * */

    /*
    /// <summary>
    /// Получить столбцы с заданными условиями в виде массива
    /// </summary>
    /// <param name="States"></param>
    /// <returns></returns>
    public DataGridViewColumn[] GetGridColumns(DataGridViewElementStates States)
    {
      List<DataGridViewColumn> Columns = new List<DataGridViewColumn>();
      DataGridViewColumn Col = Control.Columns.GetFirstColumn(States);
      while (Col != null)
      {
        Columns.Add(Col);
        Col = Control.Columns.GetNextColumn(Col, States, DataGridViewElementStates.None);
      }
      return Columns.ToArray();
    } */

    /*
    /// <summary>
    /// Получить массив видимых столбцов в просмотре в порядке вывода на экран
    /// Объекты столбцов табличного просмотра
    /// </summary>
    public DataGridViewColumn[] VisibleGridColumns
    {
      get
      {
        return GetGridColumns(DataGridViewElementStates.Visible);
      }
    } */


    #endregion

    #region Видимые строки
    /*
    /// <summary>
    /// Получить строки с заданными условиями в виде массива
    /// </summary>
    /// <param name="States"></param>
    /// <returns></returns>
    public DataGridViewRow[] GetGridRows(DataGridViewElementStates States)
    {
      List<DataGridViewRow> Rows = new List<DataGridViewRow>();
      int RowIndex = Control.Rows.GetFirstRow(States);
      while (RowIndex >= 0)
      {
        Rows.Add(Control.Rows[RowIndex]);
        RowIndex = Control.Rows.GetNextRow(RowIndex, States, DataGridViewElementStates.None);
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
        if ((Control.ScrollBars & ScrollBars.Horizontal) == ScrollBars.Horizontal)
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
              */
    #endregion

    #region Вызовы Invalidate()


    /// <summary>
    /// Обновить в просмотре все строки
    /// </summary>
    public void InvalidateAllRows()
    {
      // TODO: Не реализовано
    }

    /// <summary>
    /// Обновить в просмотре все выбранные строки SelectedRows, например, после
    /// редактирования, если в имеются вычисляемые столбцы
    /// </summary>
    public void InvalidateSelectedRows()
    {
      // TODO: Не реализовано
      /*
      if (Control.AreAllCellsSelected(false))
        Control.Invalidate();
      else
      {
        int[] idxs = SelectedRowIndices;
        for (int i = 0; i < idxs.Length; i++)
          Control.InvalidateRow(idxs[i]);
      } */
    }

    /// <summary>
    /// Пометить на обновление строку просмотра, связанную с заданной строкой таблицы данных
    /// </summary>
    /// <param name="row">Строка связанной таблицы данных</param>
    public void InvalidateDataRow(DataRow row)
    {
      if (row == null)
        return;
      IDataTableTreeModel model1 = GetDataTableModelWithCheck();
      TreePath treePath = model1.TreePathFromDataRow(row);
      IRefreshableTreeModel model2 = model1 as IRefreshableTreeModel;
      if (model2 == null)
        throw new InvalidOperationException("Модель не поддерживает обновление");
      model2.RefreshNode(treePath);
    }

    /// <summary>
    /// Пометить на обновление строки табличного просмотра, связанные с заданными строками таблицы данных
    /// </summary>
    /// <param name="rows">Массив строк связанной таблицы данных</param>
    public void InvalidateDataRows(DataRow[] rows)
    {
      if (rows == null)
        return;
      IDataTableTreeModel model1 = GetDataTableModelWithCheck();
      IRefreshableTreeModel model2 = model1 as IRefreshableTreeModel;
      if (model2 == null)
        throw new InvalidOperationException("Модель не поддерживает обновление");

      for (int i = 0; i < rows.Length; i++)
      {
        TreePath treePath = model1.TreePathFromDataRow(rows[i]);
        model2.RefreshNode(treePath);
      }
    }

    #endregion

    #region Порядок строк

    /// <summary>
    /// Описатели команд меню "Порядок строк" - массив объектов, задающих режимы
    /// сортировки
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
    /// Создает объект EFPDataTreeViewOrders.
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
    /// Текущий порядок сортировки (индекс в массиве Orders)
    /// </summary>
    public int CurrentOrderIndex
    {
      get { return _CurrentOrderIndex; }
      set
      {
        if (value == _CurrentOrderIndex)
          return;
        if (value < 0 || value >= OrderCount)
          throw new ArgumentOutOfRangeException("value",
            "Индекс сортировки должен быть в диапазоне от 0 до " + (OrderCount - 1).ToString());
        _CurrentOrderIndex = value;

        InternalSetCurrentOrder();
      }
    }
    private int _CurrentOrderIndex;

    private void InternalSetCurrentOrder()
    {
      EFPDataTreeViewSelection oldSel = Selection;

      CommandItems.InitCurentOrder();
      if (WinFormsTools.AreControlAndFormVisible(Control))
      {
        if (OrderChangesToRefresh)
          PerformRefresh();
        else
        {
          if (AutoSort)
          {
            //TODO: if (Control.DataSource != null)
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

      OnCurrentOrderChanged(EventArgs.Empty);
    }

    /// <summary>
    /// Текущий выбранный порядок сортировки
    /// </summary>
    public EFPDataViewOrder CurrentOrder
    {
      get
      {
        if (CurrentOrderIndex < 0 || CurrentOrderIndex >= OrderCount)
          return null;
        else
          return Orders[CurrentOrderIndex];
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");
        int p = Orders.IndexOf(value);
        if (p < 0)
          throw new ArgumentException("Значение отсутствует в коллекции Orders", "value");
        CurrentOrderIndex = p;
      }
    }

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

    /// <summary>
    /// Это извещение посылается после установки нового значения свойства 
    /// CurrentOrderIndex (или CurrentOrder / CurrentOrderName)
    /// </summary>
    public event EventHandler CurrentOrderChanged;

    /// <summary>
    /// Вызывает обработчик события CurrentOrderChanged, если он установлен
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnCurrentOrderChanged(EventArgs args)
    {
      if (CurrentOrderChanged != null)
        CurrentOrderChanged(this, args);
    }

    /// <summary>
    /// Это событие должно обрабатывать установку порядка строк
    /// Событие не вызывается, если установлено свойство AutoSort или
    /// если вызывается метод PerformRefresh.
    /// Поэтому обработчик Refresh также должен устанавливать порядок строки.
    /// </summary>
    public event EventHandler OrderChanged;

    /// <summary>
    /// Вызывает обработчик события OrderChanged, если он установлен
    /// </summary>
    /// <param name="args">Аргументы события</param>
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

    /// <summary>
    /// Если установить свойство в true, то не будет вызываться событие CurrentOrderChanged
    /// Вместо этого при переключении порядка сортировки пользователеми будет 
    /// вызываться метод PerformAutoSort() для установки значения DataView.ViewOrder
    /// </summary>
    public bool AutoSort { get { return _AutoSort; } set { _AutoSort = value; } }
    private bool _AutoSort;

    /// <summary>
    /// Установить значение DataView.ViewOrder
    /// Используйте этот метод в событии Refresh
    /// </summary>
    public void PerformAutoSort()
    {
      if (CurrentOrder == null)
        return;
      // TODO: CurrentOrder.PerformSort(this);
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
      /*
      EFPApp.ShowTempMessage(null);
      if (Args.ColumnIndex < 0)
        return;
      if (OrderCount == 0)
        return;
      string ColumnName = Columns[Args.ColumnIndex].Name;
      if (CurrentOrderIndex >= 0)
      {
        if (Orders[CurrentOrderIndex].SortInfo.ColumnName == ColumnName)
        {
          // Щелкнутый столбец уже является текущим столбцом сортировки
          for (int i = CurrentOrderIndex + 1; i < Orders.Count; i++)
          {
            if (Orders[i].SortInfo.ColumnName == ColumnName)
            {
              // Есть еще один такой столбец
              CurrentOrderIndex = i;
              return;
            }
          }
          for (int i = 0; i < CurrentOrderIndex; i++)
          {
            if (Orders[i].SortInfo.ColumnName == ColumnName)
            {
              // Есть еще один такой столбец
              CurrentOrderIndex = i;
              return;
            }
          }
          EFPApp.ShowTempMessage("Строки уже отсортированы по этому столбцу");
          return;
        }
      }
      int OrderIndex = Orders.IndexOfFirstColumnName(ColumnName);
      if (OrderIndex >= 0)
        CurrentOrderIndex = OrderIndex;
       * */
    }

    /// <summary>
    /// Прорисовка трегольников сортировки в заголовках столбцов в соответствии с 
    /// текущим порядком сортировки
    /// </summary>
    public void InitColumnHeaderTriangles()
    {
      /*
      foreach (DataGridViewColumn Col in Control.Columns)
        Col.HeaderCell.SortGlyphDirection = SortOrder.None;

      if (CurrentOrderIndex >= 0 && OrderCount > 0)
      {
        EFPDataGridViewOrder Order = Orders[CurrentOrderIndex];
        EFPDataGridViewSortInfo SortInfo = Order.SortInfo;
        EFPDataGridViewColumn Col = Columns[SortInfo.ColumnName];
        if (Col != null && Col.GridColumn.SortMode != DataGridViewColumnSortMode.NotSortable)
        {
          if (SortInfo.Descend)
            Col.GridColumn.HeaderCell.SortGlyphDirection = SortOrder.Descending;
          else
            Col.GridColumn.HeaderCell.SortGlyphDirection = SortOrder.Ascending;
        }
      } */
    }

    /// <summary>
    /// Показать диалог выбора порядка строк из массива Orders
    /// Если пользователь выбирает порядок, то устанавливается свойство CurrentOrder
    /// </summary>
    /// <returns></returns>
    public bool ShowSelectOrderDialog()
    {
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
      /*
      if (!Owner.Control.EndEdit())
      {
        EFPApp.ErrorMessageBox("Редактирование не закончено");
        return;
      } */


      bool changed;
      if (ManualOrderRows)
        changed = DoReorderByRows(down);
      else
        changed = DoReorderByDataColumn(down);

      // 9. Обновляем табличный просмотр
      if (changed)
      {
        OnManualOrderChanged(EventArgs.Empty);
      }
    }

    /// <summary>
    /// Восстанавливает порядок сортировки
    /// </summary>
    internal void RestoreManualOrder()
    {
      EFPDataTreeViewSelection oldSel = this.Selection;

      bool changed;
      if (ManualOrderRows)
        changed = DoSortRestoreRows();
      else
        changed = DoSortRestoreColumn();

      // Обновляем табличный просмотр
      if (changed)
      {
        this.Selection = oldSel;
        OnManualOrderChanged(EventArgs.Empty);
      }
    }

    /// <summary>
    /// Вызывается, когда выполнена ручная сортировка строк (по окончании изменения
    /// значений поля для всех строк)
    /// </summary>
    public event EventHandler ManualOrderChanged;

    /// <summary>
    /// Генерирует событие ManualOrderChanged
    /// </summary>
    protected virtual void OnManualOrderChanged(EventArgs args)
    {
      if (ManualOrderChanged != null)
        ManualOrderChanged(this, EventArgs.Empty);
    }

    #endregion

    #region Перестановка узлов дерева (не реализовано)

    /// <summary>
    /// НЕ РЕАЛИЗОВАНО.
    /// Если установлено в true, то доступны команды ручной сортировки строк.
    /// Сортировка основывается на порядке строк в коллекции, а не на значении
    /// столбца. При сортировке строки переставляются местами внутри объекта
    /// DataGridViewRowCollection
    /// </summary>
    public bool ManualOrderRows { get { return _ManualOrderRows; } set { _ManualOrderRows = value; } }
    private bool _ManualOrderRows;

    /// <summary>
    /// НЕ РЕАЛИЗОВАНО.
    /// Массив строк табличного просмотра в порядке по умолчанию. Свойство действует
    /// при ManualOrderRows=true. Если массив присвоен, то действует команда
    /// "Восстановить порядок по умолчанию"
    /// </summary>
    public DataGridViewRow[] DefaultManualOrderRows { get { return _DefaultManualOrderRows; } set { _DefaultManualOrderRows = value; } }
    private DataGridViewRow[] _DefaultManualOrderRows;

    /// <summary>
    /// Изменение порядка строк на основании их расположени в коллекции
    /// </summary>
    /// <param name="down"></param>
    /// <returns></returns>
    private bool DoReorderByRows(bool down)
    {
      throw new NotImplementedException();
      /*
// 1. Загружаем полный список строк DataGridViewRow в массив
DataGridViewRow[] Rows1 = new DataGridViewRow[Owner.Control.Rows.Count];
Owner.Control.Rows.CopyTo(Rows1, 0);

// 2. Запоминаем выбранные строки
DataGridViewRow[] SelRows = Owner.SelectedGridRows;
// 3. Получаем позиции выбранных строк в массиве всех строк
int[] SelPoss = Owner.SelectedRowIndices;
if (SelPoss.Length == 0)
{
EFPApp.ShowTempMessage("Нет ни одной выбранной строки, которую надо перемещать");
return false;
}

// 4. Проверяем, что не уперлись в границы списка
bool lBound = false;
if (Down)
{
if (SelPoss[SelPoss.Length - 1] == Rows1.Length - 1)
lBound = true;
}
else
{
if (SelPoss[0] == 0)
lBound = true;
}
if (lBound)
{
string msg = "Нельзя передвинуть ";
if (SelPoss.Length > 1)
msg += "выбранные строки ";
else
msg += "выбранную строку ";
if (Down)
msg += "вниз";
else
msg += "вверх";
EFPApp.ShowTempMessage(msg);
return false;
}

// 5. Подготавливаем массив строк для их размещения в новом порядке
// Значения null в этом массиве означают временно пустые позиции
DataGridViewRow[] Rows2 = new DataGridViewRow[Rows1.Length];

// 6. Копируем в Rows2 строки из Rows1 со сдвигом для позиций, существующих
// в SelRows.
// В процессе перемещения будем очищать массив Rows1
int Delta = Down ? 1 : -1; // значение смещения
int i;
for (i = 0; i < SelPoss.Length; i++)
{
int ThisPos = SelPoss[i];
Rows2[ThisPos + Delta] = Rows1[ThisPos];
Rows1[ThisPos] = null;
}

// 7. Перебираем исходный массив и оставшиеся непустые строки размещаем в
// новом массиве, отыскивая пустые места. Для этого используем переменную FreePos
// для указания на очередную пустую позицию второго массива
int FreePos = 0;
for (i = 0; i < Rows1.Length; i++)
{
if (Rows1[i] == null) // перемещенная позиция
continue;
// Поиск места
while (Rows2[FreePos] != null)
FreePos++;
// Нашли дырку
Rows2[FreePos] = Rows1[i];
FreePos++;
}

// 8. Замещаем коллекцию строк
Owner.Control.Rows.Clear();
Owner.Control.Rows.AddRange(Rows2);

// 9. Восстанавливаем выбор
Owner.SelectedGridRows = SelRows;     
return true;                          */
    }

    /// <summary>
    /// Восстановление порядка по умолчанию на основании DefaultManualOrderRows
    /// </summary>
    /// <returns></returns>
    private bool DoSortRestoreRows()
    {
      throw new NotImplementedException();

      /*
      if (DefaultManualOrderRows == null)
        throw new NullReferenceException("Свойство DefaultManulOrderRows не установлено");

      // 1. Загружаем полный список строк DataGridViewRow в массив
      DataGridViewRow[] Rows1 = new DataGridViewRow[Owner.Control.Rows.Count];
      Owner.Control.Rows.CopyTo(Rows1, 0);

      // 2. Запоминаем выбранные строки
      DataGridViewRow[] SelRows = Owner.SelectedGridRows;

      // 3. Подготавливаем массив строк для их размещения в новом порядке
      // Значения null в этом массиве означают временно пустые позиции
      DataGridViewRow[] Rows2 = new DataGridViewRow[Rows1.Length];

      // 4. Копируем строки из массива по умолчанию
      int i;
      int cnt = 0;
      bool Changed = false;
      for (i = 0; i < DefaultManualOrderRows.Length; i++)
      {
        int ThisPos = Array.IndexOf<DataGridViewRow>(Rows1, DefaultManualOrderRows[i]);
        if (ThisPos < 0)
          continue; // Ошибка

        Rows2[cnt] = Rows1[ThisPos];
        Rows1[ThisPos] = null;
        if (cnt != ThisPos)
          Changed = true;
        cnt++;
      }

      // 5. Копируем "лишние" строки, которых нет в массиве по умолчанию
      for (i = 0; i < Rows1.Length; i++)
      {
        if (Rows1[i] != null)
        {
          Rows2[cnt] = Rows1[i];
          if (cnt != i)
            Changed = true;
          cnt++;
        }
      }

      if (!Changed)
        return false;

      // 6. Замещаем коллекцию строк
      Owner.Control.Rows.Clear();
      Owner.Control.Rows.AddRange(Rows2);

      // 7. Восстанавливаем выбор
      Owner.SelectedGridRows = SelRows;
      return true; */
    }

    #endregion

    #region Перестановка с помощью числового поля

    #region Свойство DataReorderHelper

    /// <summary>
    /// Возвращает объект, реализующий интерфейс IDataReorderHelper.
    /// Если свойство ManualOrderColumn не установлено, возвращается null.
    /// Для инициализации значения однократно вызывается событие DataReorderHelperNeeded.
    /// Если требуется, чтобы свойство вернуло новое значение, используйте метод ResetDataReorderHelper().
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
    /// Очищает сохраненное значение свойства DataReorderHelper, чтобы при следующем обращении к нему
    /// вновь было вызвано событие DataReorderHelperNeeded
    /// </summary>
    public virtual void ResetDataReorderHelper()
    {
      _DataReorderHelper = null;
    }

    /// <summary>
    /// Событие вызывается при выполнении команд ручной сортировки, если установлено свойство ManualOrderColumn.
    /// Обработчик события может создать собственный экземпляр и установить свойство Helper.
    /// Если обработчика нет, или он не создал объект, то создается объект по умолчанию
    /// </summary>
    public event DataReorderHelperNeededEventHandler DataReorderHelperNeeded;

    /// <summary>
    /// Вызывает обработчик события DataReorderHelperNeeded.
    /// Если IDataReorderHelper не получен, объект создается вызовом метода EFPDataGridView.CreateDataReorderHelper().
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
        throw new NullReferenceException("Объект, реализующий IDataReorderHelper, не был создан");
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
    /// Используется при установленном свойстве EFPDataTreeViewCommandItems.ManualOrderColumn.
    /// Непереопределенный метод создает новый объект DataTableTreeReorderHelper.
    /// </summary>
    /// <returns>Объект, реализующий интерфейс IDataReorderHelper </returns>
    protected virtual IDataReorderHelper CreateDefaultDataReorderHelper()
    {
      if (String.IsNullOrEmpty(ManualOrderColumn))
        throw new InvalidOperationException("Не установлено свойство ManualOrderColumn");

      IDataTableTreeModel dtmodel = Control.Model as IDataTableTreeModel;
      if (dtmodel != null)
        return new DataTableTreeReorderHelper(dtmodel, ManualOrderColumn);

      throw new InvalidOperationException("Модель не присоединена или она не реализует интерфейс IDataTableTreeModel");

      /*
      // Получаем доступ к объекту DataView
      DataView dv = SourceAsDataView;
      if (dv == null)
        throw new InvalidDataSourceException("Нельзя получить DataView");

      return new DataTableReorderHelper(dv, CommandItems.ManualOrderColumn);
       * */
    }


    #endregion

    #region Перестановка строк

    /// <summary>
    /// Имя числового столбца, который определяет порядок строк при ручной сортировке
    /// Если задано, то в меню есть команды ручной сортировки
    /// </summary>
    public string ManualOrderColumn { get { return _ManualOrderColumn; } set { _ManualOrderColumn = value; } }
    private string _ManualOrderColumn;

    /// <summary>
    /// Изменение порядка строк на основании числового поля
    /// </summary>
    /// <param name="down"></param>
    /// <returns></returns>
    private bool DoReorderByDataColumn(bool down)
    {
      if (DataReorderHelper == null)
        throw new NullReferenceException("DataReorderHelper=null");

      //Загружаем выбранные строки
      DataRow[] selRows = this.SelectedDataRows;
      if (selRows.Length == 0)
      {
        EFPApp.ShowTempMessage("Нет ни одной выбранной строки, которую надо перемещать");
        return false;
      }

      bool res;
      if (down)
        res = DataReorderHelper.MoveDown(selRows);
      else
        res = DataReorderHelper.MoveUp(selRows);

      if (!res)
      {
        string msg = "Нельзя передвинуть ";
        if (selRows.Length > 1)
          msg += "выбранные строки ";
        else
          msg += "выбранную строку ";
        if (down)
          msg += "вниз";
        else
          msg += "вверх";
        EFPApp.ShowTempMessage(msg);
        return false;
      }

      // Обновляем просмотр
      //ControlProvider.PerformRefresh();
      //Model.Refresh();
      /*
      if (Owner.Control.CurrentCell != null)
      {
        if (Owner.Control.CurrentCell.IsInEditMode)
        {
          DataGridViewCell Cell = Owner.Control.CurrentCell;
          Owner.Control.CurrentCell = null;
          Owner.Control.CurrentCell = Cell;
        }
      } */

      this.SelectedDataRows = selRows; // восстанавливаем выбранные строки

      return true;
    }

    #endregion

    #region Восстановление порядка


    /// <summary>
    /// Имя числового столбца, который определяет порядок строк по умолчанию при ручной
    /// сортировке. Свойство действует при непустом значении ManualOrderColumn.
    /// Если имя присвоено, то действует команда "Восстановить порядок по умолчанию"
    /// </summary>
    public string DefaultManualOrderColumn { get { return _DefaultManualOrderColumn; } set { _DefaultManualOrderColumn = value; } }
    private string _DefaultManualOrderColumn;

    /// <summary>
    /// Восстановление порядка по умолчанию на основании DefaultManualOrderColumn
    /// </summary>
    /// <returns></returns>
    private bool DoSortRestoreColumn()
    {
      if (String.IsNullOrEmpty(DefaultManualOrderColumn))
        throw new NullReferenceException("Свойство DefaultManualOrderColumn не установлено");
      if (DataReorderHelper == null)
        throw new NullReferenceException("DataReorderHelper=null");


      DataRow[] desiredOrder;
      using (DataView dv = new DataView(SourceAsDataTable))
      {
        dv.Sort = DefaultManualOrderColumn;
        desiredOrder = DataTools.GetDataViewRows(dv);
      }

      bool changed = DataReorderHelper.Reorder(desiredOrder);

      // Обновляем просмотр
      //if (changed)
      //  ControlProvider.Control.Refresh();

      return changed;
    }

    #endregion

    #region InitManualOrderColumnValue()

    /// <summary>
    /// Выполняет инициализацию значения поля, заданного свойством ManualOrderColumn для новых строк, у которых поле имеет значение 0.
    /// Вызывает метод IDataReorderHelper.InitRows().
    /// Если свойcтво ManualOrderColumn не установлено, никаких действий не выполняется.
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
    /// Команды локального меню
    /// </summary>
    public new EFPDataTreeViewCommandItems CommandItems { get { return (EFPDataTreeViewCommandItems)(base.CommandItems); } }

    /// <summary>
    /// Создает новый объект EFPDataTreeViewCommandItems
    /// </summary>
    /// <returns>Созданный объект</returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      return new EFPDataTreeViewCommandItems(this);
    }

    /// <summary>
    /// Возвращает список для добавления пользовательских вариантов печати/экспорта
    /// </summary>
    public NamedList<EFPMenuOutItem> MenuOutItems
    {
      get
      {
        return CommandItems.OutHandler.Items;
      }
    }
    /// <summary>
    /// Параметры печати/экспорта табличного просмотра.
    /// Может возвращать null, если в <see cref="EFPDataGridViewCommandItems.OutHandler"/> был удален вариант "Control"
    /// </summary>
    public Reporting.BRDataTreeViewMenuOutItem DefaultOutItem
    {
      get { return MenuOutItems["Control"] as Reporting.BRDataTreeViewMenuOutItem; }
    }

    #endregion

    #region Получение EFPDataViewRowInfo

    private DataRowValueArray _RowValueArray;

    /// <summary>
    /// Получить объект EFPDataViewRowInfo для узла.
    /// Когда он больше не нужен, должен быть вызван FreeRowInfo()
    /// </summary>
    /// <param name="node">Узел дерева</param>
    /// <returns>Информация о строке</returns>
    public EFPDataViewRowInfo GetRowInfo(TreeNodeAdv node)
    {
      IDataRowNamedValuesAccess rwa = _RowValueArray;
      if (rwa == null) // Первый вызов или вложенный
        rwa = CreateRowValueAccessObject();
      rwa.CurrentRow = node.Tag as DataRow;
      return new EFPDataViewRowInfo(this, rwa.CurrentRow, rwa, node.Row);
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
    }

    #endregion

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
    public BRColumnHeaderArray GetColumnHeaderArray(EFPDataTreeViewColumn[] columns)
    {
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
          headers[i] = new string[] { columns[i].TreeColumn.Header };
      }

      #endregion

      BRColumnHeaderArray ha = new BRColumnHeaderArray(headers, ColumnHeaderMixedSpanAllowed);
      return ha;
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
  }
}
