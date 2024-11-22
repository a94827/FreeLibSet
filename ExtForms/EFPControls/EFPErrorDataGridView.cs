// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Расширение класса табличного просмотра для просмотра списка ошибок
  /// <see cref="ErrorMessageList"/>. Поддерживается "редактирование" сообщения с помощью пользовательского обработчика.
  /// Поддерживается управление закладкой <see cref="TabPage"/> для показа значка, соответствующего серьезности сообщений в списке.
  /// </summary>
  public class EFPErrorDataGridView : EFPDataGridView
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер табличного просмотра
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Должен быть задан</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPErrorDataGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control)
    {
      Init();
    }

    /// <summary>
    /// Создает провайдер табличного просмотра
    /// </summary>
    /// <param name="controlWithPreview">Управляющий элемент и панель инструментов</param>
    public EFPErrorDataGridView(IEFPControlWithToolBar<DataGridView> controlWithPreview)
      : base(controlWithPreview)
    {
      Init();
    }

    private void Init()
    {
      Control.ReadOnly = true;
      Control.AllowUserToAddRows = false;
      Control.AllowUserToDeleteRows = false;
      Control.AllowUserToResizeColumns = false;
      Control.AllowUserToOrderColumns = false;
      Control.MultiSelect = true; // 31.01.2013
      //Control.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      Control.AutoGenerateColumns = false;
      //Control.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
      AutoSizeRowsMode = EFPDataGridViewAutoSizeRowsMode.Auto; // 23.03.2018
      Control.ScrollBars = ScrollBars.Vertical;

      //TheHandler.Columns.AddImage();
      Columns.AddInt("NPop", false, "№ п/п", 4);
      DataGridViewTextBoxColumn col;
      col = Columns.AddText("Kind", false, "Тип", 4);
      col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

      col = Columns.AddText("Code", false, "Код", 1, 1, DataGridViewContentAlignment.MiddleCenter);
      col.Visible = false;
      col = Columns.AddTextFill("Text", false, "Сообщение", 100, 20);
      col.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
      UseRowImages = true;
      ShowErrorCountInTopLeftCell = true;
      CommandItems.UseRowErrorsListView = false;
      DisableOrdering();

      ReadOnly = true; // разрешаем после присоедиенения списка(EditHandler == null);
      CanInsert = false;
      CanDelete = false;
      CanView = false;
      //CommandItems.UseRowErrorsListView = false;
      CommandItems.EnterAsOk = true;

      /*
      if (CodeWidth == 0)
        ConfigSectionName = "СообщенияОбОшибках";
      else
        ConfigSectionName = "СообщенияОбОшибках" + "_Код" + CodeWidth.ToString();
      AddGridPrint();
      GridPageSetup.DocumentName = "Список ошибок";
      GridPageSetup.Title = "Список ошибок";
      GridPageSetup.InitDefaults += new EventHandler(ErrorMessageList_GridPageSetup_InitDefaults);
        */
      Control.RowCount = 1;
      Control[0, 0].Value = null;
      Control[1, 0].Value = null;
      Control[2, 0].Value = null;
      Control[3, 0].Value = "Список ошибок не присоединен!";
      Control.Rows[0].Tag = null;

      _FirstFlag = true;

      _TabPageControlOptions = EFPTabPageControlOptions.All;
    }

    #endregion

    #region Свойство ErrorMessages

    private bool _FirstFlag;

    /// <summary>
    /// Текущий присоединенный список ошибок.
    /// Допускается присвоение значения null.
    /// </summary>
    public ErrorMessageList ErrorMessages
    {
      get
      {
        return _ErrorMessages;
      }
      set
      {
        bool prevIsEmpty = true;
        if (_ErrorMessages != null)
          prevIsEmpty = _ErrorMessages.Count == 0;

        _ErrorMessages = value;

        bool thisIsEmpty = true;
        if (value != null)
          thisIsEmpty = value.Count == 0; // 27.12.2020

        if (HasEditMessageHandler)
          base.ReadOnly = thisIsEmpty;


        bool needsUpdate = true;
        if (!_FirstFlag)
        {
          if (thisIsEmpty && prevIsEmpty)
            needsUpdate = false;
        }
        _FirstFlag = false;

        if (needsUpdate)
        {
          if (thisIsEmpty)
          {
            Control.RowCount = 1;
            Control[0, 0].Value = null;
            Control[1, 0].Value = null;
            Control[2, 0].Value = null;
            Control[3, 0].Value = "Нет ошибок";
            Control.Rows[0].Tag = null;
          }
          else
          {
#if DEBUG
            if (value == null)
              throw new BugException("value==null при ThisIsEmpty=false");
#endif
            Control.RowCount = value.Count;
            for (int i = 0; i < value.Count; i++)
            {
              //          Image img;
              string kindText;
              switch (value[i].Kind)
              {
                case ErrorMessageKind.Error:
                  //              img = EFPApp.MainImages.Images["Error"];
                  kindText = "Ош.";
                  break;
                case ErrorMessageKind.Warning:
                  //              img = EFPApp.MainImages.Images["Warning"];
                  kindText = "Пр.";
                  break;
                case ErrorMessageKind.Info:
                  //              img = EFPApp.MainImages.Images["Information"];
                  kindText = "Инфо";
                  break;
                default:
                  throw new BugException("Неизвестный тип сообщения");
              }
              //          TheHandler.MainGrid[0, i].ValueEx = img;
              Control[0, i].Value = i + 1;
              Control[1, i].Value = kindText;
              Control[2, i].Value = value[i].Code;
              Control[3, i].Value = value[i].Text;
              Control.Rows[i].Tag = value[i].Kind;
            }
          }

          Control.Invalidate(); // иначе не обновится столбец значков
        }
        InitControlledObject();
        OnErrorMessagesChanged();
        InitEditCommandItems();
      }
    }
    private ErrorMessageList _ErrorMessages;

    /// <summary>
    /// Событие вызывается при установке свойства <see cref="ErrorMessages"/>.
    /// </summary>
    public event EventHandler ErrorMessagesChanged;

    /// <summary>
    /// Вызывает обработчик события <see cref="ErrorMessagesChanged"/>, если он присоединен
    /// </summary>
    protected virtual void OnErrorMessagesChanged()
    {
      if (ErrorMessagesChanged != null)
        ErrorMessagesChanged(this, EventArgs.Empty);
    }


    /// <summary>
    /// Инициализация свойства <see cref="EFPDataGridView.ReadOnly"/> после изменения списка сообщений и присоединения обработчика.
    /// </summary>
    protected virtual void InitEditCommandItems()
    {
      ReadOnly = !(HasEditMessageHandler);
      if (!CommandItems.IsReadOnly) // 03.02.2018
        CommandItems.EnterAsOk = !HasEditMessageHandler;
    }


    #endregion

    #region Свойство CodeWidth

    /// <summary>
    /// Задает ширину столбца "Код" в символах. См.свойство <see cref="EFPDataGridViewColumn.TextWidth"/>.
    /// Нулевое значение (по умолчанию) задает скрытый столбец.
    /// </summary>
    public int CodeWidth
    {
      get
      {
        return _CodeWidth;
      }
      set
      {
#if DEBUG
        if (value < 0 || value > 20)
          throw new ArgumentException("Недопустимая ширина колонки \"Код\": " + CodeWidth);
#endif
        int codeWidth2 = value;
        if (codeWidth2 == 0)
          codeWidth2 = 1; // колонка не должна быть нулевой ширины
        _CodeWidth = value;

        Columns["Code"].GridColumn.Visible = value > 0;
        Columns["Code"].TextWidth = codeWidth2;
      }
    }
    private int _CodeWidth;

    /// <summary>
    /// Событие обработчика редактирования строки с ошибкой.
    /// Вызывается, когда пользователь выполняет команду "Редактировать" для строки таблицы.
    /// Групповое редактирование нескольких строк не поддерживается.
    /// Обработчик получает одно сообщение <see cref="ErrorMessageItem"/> из списка <see cref="ErrorMessages"/>, к которому относится текущая
    /// строка табличного просмотра.
    /// Обработчик может показать детальное сообщение об ошибке, либо перейти к объекту, к которому
    /// относится сообщение.
    /// Обработчик события не должен устанавливаться, если "редактирование" не имеет смысла. При этом
    /// команды редактирования не будут доступны, если не установлен обработчик события <see cref="EFPDataGridView.EditData"/>.
    /// 
    /// Если "редактирование" имеет смысл только для некоторых сообщений об ошибке (может быть 
    /// выполнен переход к ошибочному объекту), а для остальных - нет, то присоединяемый обработчик
    /// должен выдавать пользователю какое-либо сообщение, когда переход невозможен.
    /// </summary>
    public event ErrorMessageItemEventHandler EditMessage;

    /// <summary>
    /// Возвращает true, если установлен обработчик события <see cref="EditMessage"/>.
    /// </summary>
    public bool HasEditMessageHandler { get { return EditMessage != null; } }

    #endregion

    #region Обработчики таблицы

    /// <summary>
    /// Добавляет значки в заголовок строки
    /// </summary>
    /// <param name="args">Аргументы события <see cref="EFPDataGridView.GetRowAttributes"/></param>
    protected override void OnGetRowAttributes(EFPDataGridViewRowAttributesEventArgs args)
    {
      DataGridViewRow row = args.Control.Rows[args.RowIndex];
      if (row.Tag != null)
      {
        ErrorMessageKind kind = (ErrorMessageKind)(row.Tag);
        switch (kind)
        {
          case ErrorMessageKind.Error:
            args.AddRowError("Ошибка");
            break;
          case ErrorMessageKind.Warning:
            args.AddRowWarning("Предупреждение");
            break;
          case ErrorMessageKind.Info:
            args.AddRowInformation("Сообщение");
            break;
        }
      }

      base.OnGetRowAttributes(args);
    }

    /// <summary>
    /// Вызывается обработчик события <see cref="EFPDataGridView.EditData"/> базового класса, если он установлен.
    /// Иначе вызывается обработчик события <see cref="EditMessage"/>, если он установлен.
    /// </summary>
    /// <param name="args">Не используется</param>
    /// <returns>Возвращает true, если событие было обработано</returns>
    protected override bool OnEditData(EventArgs args)
    {
      if (base.OnEditData(args))
        return true;

      if (State != UIDataState.Edit)
      {
        EFPApp.ShowTempMessage("Режим " + State.ToString() + " не поддерживается");
        return true;
      }
      if (!HasEditMessageHandler)
      {
        EFPApp.ShowTempMessage("Обработчик для \"редактирования\" сообщения не задан");
        return true;
      }
      if (!CheckSingleRow())
        return true;

      // 21.08.2019
      if (ErrorMessages == null || ErrorMessages.Count == 0)
      {
        EFPApp.ShowTempMessage("Нет сообщений об ошибках");
        return true;
      }

      ErrorMessageItemEventArgs editArgs = new ErrorMessageItemEventArgs(ErrorMessages,
        CurrentRowIndex);

      EditMessage(this, editArgs);

      return true;
    }

    #endregion

    #region Размещение на закладке

    /// <summary>
    /// Сюда может быть помещена ссылка на закладку <see cref="System.Windows.Forms.TabPage"/>, на которой размещается
    /// просмотр ошибок. Если свойство задано, то у закладки меняется заголовок,
    /// всплывающая подсказка и значок, в зависимости от свойства <see cref="TabPageControlOptions"/>.
    /// </summary>
    public TabPage ControlledTabPage
    {
      get { return _ControlledTabPage; }
      set
      {
        _ControlledTabPage = value;
        InitControlledObject();
      }
    }
    private TabPage _ControlledTabPage;

    /// <summary>
    /// Сюда может быть помещена ссылка на интерфейс управления закладкой, на которой размещается
    /// просмотр ошибок. Если свойство задано, то у закладки меняется заголовок,
    /// всплывающая подсказка и значок, в зависимости от свойства <see cref="TabPageControlOptions"/>.
    /// </summary>
    public IEFPTabPageControl ControlledTabPageControl
    {
      get { return _ControlledTabPageControl; }
      set
      {
        _ControlledTabPageControl = value;
        InitControlledObject();
      }
    }
    private IEFPTabPageControl _ControlledTabPageControl;

    private void InitControlledObject()
    {
      if (_ControlledTabPage != null)
      {
        if ((_TabPageControlOptions & EFPTabPageControlOptions.Text) == EFPTabPageControlOptions.Text)
          _ControlledTabPage.Text = EFPApp.GetErrorTitleText(ErrorMessages);
        if ((_TabPageControlOptions & EFPTabPageControlOptions.ImageKey) == EFPTabPageControlOptions.ImageKey)
          _ControlledTabPage.ImageKey = EFPApp.GetErrorImageKey(ErrorMessages);
        if ((_TabPageControlOptions & EFPTabPageControlOptions.ToolTipText) == EFPTabPageControlOptions.ToolTipText)
          _ControlledTabPage.ToolTipText = EFPApp.GetErrorToolTipText(ErrorMessages);
      }
      if (_ControlledTabPageControl != null)
      {
        if ((_TabPageControlOptions & EFPTabPageControlOptions.Text) == EFPTabPageControlOptions.Text)
          _ControlledTabPageControl.Text = EFPApp.GetErrorTitleText(ErrorMessages);
        if ((_TabPageControlOptions & EFPTabPageControlOptions.ImageKey) == EFPTabPageControlOptions.ImageKey)
          _ControlledTabPageControl.ImageKey = EFPApp.GetErrorImageKey(ErrorMessages);
        if ((_TabPageControlOptions & EFPTabPageControlOptions.ToolTipText) == EFPTabPageControlOptions.ToolTipText)
          _ControlledTabPageControl.ToolTipText = EFPApp.GetErrorToolTipText(ErrorMessages);
      }
    }

    /// <summary>
    /// Какими свойствами закладки следует управлять.
    /// Это свойство должно устанавливаться до присвоения свойств <see cref="ControlledTabPage"/> и <see cref="ControlledTabPageControl"/>.
    /// По умолчанию имеет значение <see cref="EFPTabPageControlOptions.All"/>.
    /// </summary>
    public EFPTabPageControlOptions TabPageControlOptions
    {
      get { return _TabPageControlOptions; }
      set { _TabPageControlOptions = value; }
    }
    private EFPTabPageControlOptions _TabPageControlOptions;

    #endregion
  }
}
