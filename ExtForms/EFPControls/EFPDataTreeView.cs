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
  /// Возможные способы сохранения / восстановления текущей позиции в табличном 
  /// просмотре (свойство EFPDataGridView.SelectedRowsMode)
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
    /// Сохранение ссылок на объекты DataRow
    /// Подходит для просмотров, связанных с IDataTableTreeModel при условии, что
    /// обновление таблицы не приводит к замене строк. Зато таблица может не иметь
    /// ключевого поля.
    /// </summary>
    DataRow,

    /// <summary>
    /// Режим для просмотров, связанных с IDataTableTreeModel при условии, что таблица имеет первичный ключ
    /// </summary>
    PrimaryKey,
  }

  /// <summary>
  /// Класс для сохранения текущей позиции и выбранных строк/столбцов в таблчном просмотре
  /// (свойство EFPDataGridView.Selection)
  /// Не содержит открытых полей
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
    /// Номер текущего столбца
    /// </summary>
    internal int CurrentColumnIndex;

    /// <summary>
    /// true, если свойство TreeViewAdv.Model было установлено
    /// </summary>
    internal bool ModelExists;
  }

  #endregion

  /// <summary>
  /// Древовидный просмотр с поддержкой столбцов
  /// </summary>
  public class EFPDataTreeView : EFPTreeViewAdv, IEFPDataView, IEFPGridControl
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

      if (!DesignMode)
      {
        //Control.CurrentCellChanged += new EventHandler(Control_CurrentCellChanged);
        Control.VisibleChanged += new EventHandler(Control_VisibleChanged);
        //Control.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(Control_DataBindingComplete);
        Control.KeyDown += new KeyEventHandler(Control_KeyDown);
        Control.MouseDown += new MouseEventHandler(Control_MouseDown);
        //Control.RowPrePaint += new DataGridViewRowPrePaintEventHandler(Control_RowPrePaint);
        //Control.CellPainting += new DataGridViewCellPaintingEventHandler(Control_CellPainting);
        //Control.RowPostPaint += new DataGridViewRowPostPaintEventHandler(Control_RowPostPaint);
        //Control.CellFormatting += new DataGridViewCellFormattingEventHandler(Control_CellFormatting);
        //Control.Enter += new EventHandler(Control_Enter);
        //Control.Leave += new EventHandler(Control_Leave);
        //Control.KeyPress += new KeyPressEventHandler(Control_KeyPress);
        Control.KeyDown += new KeyEventHandler(Control_KeyDown);
        //Control.CellClick += new DataGridViewCellEventHandler(Control_CellClick);
        //Control.CellContentClick += new DataGridViewCellEventHandler(Control_CellContentClick);
        //Control.CurrentCellDirtyStateChanged += new EventHandler(Control_CurrentCellDirtyStateChanged);
        //Control.CellValueChanged += new DataGridViewCellEventHandler(Control_CellValueChanged);
        //Control.CellParsing += new DataGridViewCellParsingEventHandler(Control_CellParsing);
        //Control.CellValidating += new DataGridViewCellValidatingEventHandler(Control_CellValidating);
        //Control.CellBeginEdit += new DataGridViewCellCancelEventHandler(Control_CellBeginEdit1);
        //if (EFPApp.ShowToolTips)
        //  Control.CellToolTipTextNeeded += new DataGridViewCellToolTipTextNeededEventHandler(Control_CellToolTipTextNeeded);
        //Control.ColumnHeaderMouseClick += new DataGridViewCellMouseEventHandler(Control_ColumnHeaderMouseClick);
        //Control.ReadOnlyChanged += Control_ReadOnlyChanged;
        Control.ModelChanged += Control_ModelChanged;
      }
      Control.UseColumns = true;
    }

    #endregion

    #region Изменения в ProviderState

    /// <summary>
    /// Вызов ResetDataReorderHelper();
    /// </summary>
    protected override void OnDetached()
    {
      base.OnDetached();
      ResetDataReorderHelper();
    }

    /// <summary>
    /// Вызов ResetDataReorderHelper();
    /// </summary>
    protected override void OnDisposed()
    {
      ResetDataReorderHelper();
      base.OnDisposed();
    }

    #endregion

    #region Обработчики событий

    /// <summary>
    /// Этот метод вызывается после установки свойства CommandItems.Control
    /// Добавляем обработчики, которые должны быть в конце цепочки
    /// </summary>
    public virtual void AfterControlAssigned()
    {
      // Мы должны присоединить обработчик CellBeginEdit после пользовательского, т.к.
      // нам нужно проверить свойство Cancel
      //Control.CellBeginEdit += new DataGridViewCellCancelEventHandler(Control_CellBeginEdit2);
      //Control.CellEndEdit += new DataGridViewCellEventHandler(Control_CellEndEdit2);
    }

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
      // !!! Модификатор public нужен для EFPAppCommandItems

      try
      {
        if (Control.Visible)
        {
          CurrentColumnIndex = CurrentColumnIndex;
          /*
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
          } */

          //CommandItems.RefreshStatItems();
        }
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

    IEFPGridControlMeasures IEFPGridControl.Measures { get { return Measures; } }


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
    /// Для столбцов, определенных в Orders, сортировка остается разрешенной
    /// </summary>
    public void DisableOrdering()
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
    /// Установить свойство DataReadOnly для всех столбцов просмотра.
    /// Метод используется, когда требуется разрешить редактирование "по месту" 
    /// только для некоторых столбцов, а большая часть столбцов не редактируется
    /// Метод должен вызываться после установки общих свойств DataReadOnly табличного 
    /// просмотра, иначе признак у столбцов может быть изменен неявно
    /// </summary>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetColumnsReadOnly(bool value)
    {
      for (int i = 0; i < Control.NodeControls.Count; i++)
      {
        EditableControl ec = Control.NodeControls[i] as EditableControl;
        if (ec != null)
          ec.EditEnabled = !value; // 17.11.2017
      }
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
      for (int i = 0; i < Control.NodeControls.Count; i++)
      {
        EditableControl ec = Control.NodeControls[i] as EditableControl;
        if (ec != null)
        {
          if (Array.IndexOf<string>(a, ec.DataPropertyName) >= 0)
            ec.EditEnabled = !ReadOnly;
        }
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

    /// <summary>
    /// Создает объект EFPDataTreeViewColumns
    /// </summary>
    /// <returns>Новый объект</returns>
    protected virtual EFPDataTreeViewColumns CreateColumns()
    {
      return new EFPDataTreeViewColumns(this);
    }

    //internal protected virtual void AddGridProducerToolTip(EFPDataTreeViewColumn Column, List<string> a, int RowIndex)
    //{
    //}

    EFPDataViewColumnInfo[] IEFPGridControl.GetVisibleColumnsInfo()
    {
      throw new NotImplementedException();
    }

    bool IEFPGridControl.InitColumnConfig(EFPDataGridViewConfigColumn configColumn)
    {
      throw new NotImplementedException();
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
    /// Оптимальнее, чем вызов SelectedRows.Length
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

    /// <summary>
    /// Вспомогательное свойство только для чтения. Возвращает true, если свойство
    /// CurrentRow установлено (в просмотре есть текущая строка) и она является
    /// единственной выбранной строкой
    /// </summary>
    public bool IsCurrentRowSingleSelected
    {
      get
      {
        return true;
        /*
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
         * */
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
        throw new NotSupportedException();
        /*
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
         * */
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
    /// Если модель реализует интерфейс IDataTableTreeModel, то возвращается таблица модели.
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
    /// Если модель реализует интерфейс IDataTableTreeModel, то возвращается DataView модели или Table.DefaultView.
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
    /// Возвращает IDataTableTreeModel или выбрасывает исключение
    /// </summary>
    /// <returns></returns>
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
    /// Расширяет реализацию свойства TreeViewAdv.SelectedNodes, преобразуя TreeNodeAdv.Tag
    /// в строки таблицы данных DataRow.
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

            TreeNodeAdv node = Control.FindNode(treePath);
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
    /// Получить строки DataRow таблицы для выбранных узлов
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
    /// Расширение свойства CurrentRow (чтение и установка текущей строки).
    /// В качестве значения используется строка таблицы данных (объект DataRow)
    /// Просмотр должен быть присоединен к DataSource типа DataView
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
        TreeNodeAdv node = Control.FindNode(treePath);
        if (node != null)
          Control.SelectedNode = node;

        EnsureSelectionVisible();
      }
    }

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
    /// Расширение свойств CurrentRow / CurrentDataRow для получения / установки 
    /// текущей строки с помощью значений ключевых полей в DataTable
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
    /// Режим сохранения выбранных строк свойствами SelectedNodesObject и CurrentNodeObject.
    /// В отличие от EFPDataGridView, значение свойства определяется исключительно присоединенной моделью.
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
    /// в зависимости от свойства SelectedRowsMode
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
    /// Тип свойства зависит от режима SelectedRowsMode.
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
    /// Режим сохранения строк определеятся свойство SelectedRowsMode
    /// Значение включает в себя: признак AreAllCellsSelected, список выделенных
    /// строк SelectedRowsObject, текущую строку CurrentRowObject и индекс
    /// столбца с текущей ячейкой CurrentColumnIndex
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
        res.CurrentColumnIndex = CurrentColumnIndex;

        return res;
      }
      set
      {
        if (!value.ModelExists)
          return;

        CurrentColumnIndex = value.CurrentColumnIndex;
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

      CommandItems.PerformRefreshItems();
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
        {
          for (int i = 0; i < Control.Columns.Count; i++)
          {
            if (Control.Columns[i].IsVisible)
              return i;
          }
          return -1;
        }
        return _CurrentColumnIndex;
      }
      set
      {     /*
        if (value < 0 || value >= Control.ColumnCount)
          return;
        if (!Control.Columns[value].IsVisible)
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
        }     */
        _CurrentColumnIndex = value;
      }
    }

    /// <summary>
    /// Сохраняем установленный ColumnIndex, если потом произойдет пересоздание просмотра
    /// </summary>
    private int _CurrentColumnIndex = -1;

    /*
    /// <summary>
    /// Текущий столбец. Если выбрано несколько столбцов (например, строка целиком),
    /// то null
    /// </summary>
    public EFPDataTreeViewColumn CurrentColumn
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
    } */

    /// <summary>
    /// Имя текущего столбца. Если выбрано несколько столбцов (например, строка целиком),
    /// то пустая строка ("")
    /// </summary>
    public string CurrentColumnName
    {
      get
      {
        return string.Empty;
        /*
        if (CurrentColumnIndex < 0 || CurrentColumnIndex >= Control.ColumnCount)
          return String.Empty;
        else
          return Columns[CurrentColumnIndex].Name;*/
      }
      set
      {          /*
        EFPDataTreeViewColumn Column = Columns[value];
        if (Column != null)
          CurrentColumnIndex = Column.GridColumn.Index;*/
      }
    }

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
    /// Прорисовка трегольников сортировки в заголовках столбов в соответствии с 
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

      int oldColIdx = CurrentColumnIndex;

      bool changed;
      if (ManualOrderRows)
        changed = DoReorderByRows(down);
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
        {
          DataReorderHelperNeededEventArgs args = new DataReorderHelperNeededEventArgs();
          OnDataReorderHelperNeeded(args);
          if (args.Helper == null)
            throw new NullReferenceException("Объект, реализующий IDataReorderHelper, не был создан");
          _DataReorderHelper = args.Helper;
        }
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
    protected override EFPControlCommandItems GetCommandItems()
    {
      return new EFPDataTreeViewCommandItems(this);
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

  }
}
