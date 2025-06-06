﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.Text;

namespace FreeLibSet.Forms
{
  #region Перечисление

  /// <summary>
  /// Вариант команды буфера обмена: "Вставка" или "Специальная вставка"
  /// </summary>
  public enum EFPPasteReason
  {
    /// <summary>
    /// Обычная вставка (Ctrl+V)
    /// </summary>
    Paste,

    /// <summary>
    /// Специальная вставка
    /// </summary>
    PasteSpecial
  }

  #endregion

  #region Делегаты

  /// <summary>
  /// Аргументы события <see cref="EFPPasteFormat.Paste"/>
  /// </summary>
  public class EFPPasteDataObjectEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает объект аргумента
    /// </summary>
    /// <param name="pasteFormat">Объект <see cref="EFPPasteFormat"/></param>
    /// <param name="data">Данные буфера обмена</param>
    /// <param name="reason">Причина вызова: обычная или специальная вставка</param>
    /// <param name="actionCode">Код действия <see cref="EFPPasteAction.Code"/> или пустая строка для обычных команд вставки (<see cref="EFPPasteFormat.HasActions"/>=false)</param>
    public EFPPasteDataObjectEventArgs(EFPPasteFormat pasteFormat, IDataObject data, EFPPasteReason reason, string actionCode)
    {
      _PasteFormat = pasteFormat;
      _Data = data;
      _Reason = reason;
      _ActionCode = actionCode;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект <see cref="EFPPasteFormat"/>
    /// </summary>
    public EFPPasteFormat PasteFormat { get { return _PasteFormat; } }
    private readonly EFPPasteFormat _PasteFormat;

    /// <summary>
    /// Данные буфера обмена
    /// </summary>
    public IDataObject Data { get { return _Data; } }
    private readonly IDataObject _Data;


    /// <summary>
    /// Причина вызова: обычная или специальная вставка
    /// </summary>
    public EFPPasteReason Reason { get { return _Reason; } }
    private readonly EFPPasteReason _Reason;

    /// <summary>
    /// Код действия <see cref="EFPPasteAction.Code"/> или пустая строка для обычных команд вставки (<see cref="EFPPasteFormat.HasActions"/>=false)
    /// </summary>
    public string ActionCode { get { return _ActionCode; } }
    private readonly string _ActionCode;

    /// <summary>
    /// Ссылка на описатель действия <see cref="EFPPasteAction"/>, соответствующий <see cref="ActionCode"/> или null
    /// для обычных команд вставки.
    /// </summary>
    public EFPPasteAction Action
    {
      get
      {
        if (String.IsNullOrEmpty(ActionCode))
          return null;
        else
          return PasteFormat.Actions[ActionCode];
      }
    }

    /// <summary>
    /// Извлекает данные из объекта <see cref="Data"/> в формате, заданном в свойстве <see cref="EFPPasteFormat.DataFormat"/> 
    /// и с возможным преобразованием, в зависимости от <see cref="EFPPasteFormat.AutoConvert"/>.
    /// </summary>
    /// <returns>Данные в требуемом формате</returns>
    public object GetData()
    {
      return Data.GetData(_PasteFormat.DataFormat, _PasteFormat.AutoConvert);
    }

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="EFPPasteFormat.Paste"/>
  /// </summary>
  /// <param name="sender">Ссылка на <see cref="EFPPasteFormat"/>. Дублируется в свойстве <see cref="EFPPasteDataObjectEventArgs.PasteFormat"/>.</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPPasteDataObjectEventHandler(object sender, EFPPasteDataObjectEventArgs args);

  #region EFPTestDataObjectResult

  /// <summary>
  /// Результаты тестирования данных в буфере обмена
  /// </summary>
  public enum EFPTestDataObjectResult
  {
    /// <summary>
    /// В буфере обмена нет данных в подходящем формате
    /// </summary>
    NoData,

    /// <summary>
    /// В буфере обмена есть данные, которые можно вставить
    /// </summary>
    Ok,

    /// <summary>
    /// В буфере обмена есть даные в проверяемом формате, но они содержат ошибки.
    /// Например, в буфере есть текст, но его нельзя преобразовать в прямоугольный блок.
    /// </summary>
    FormatError,

    /// <summary>
    /// В буфере обмена есть даные в проверяемом формате, но они не могут быть вставлены.
    /// Например, в буфере есть текст, но его нельзя преобразовать в число.
    /// </summary>
    ApplyError,
  }

  #endregion

  /// <summary>
  /// Аргументы события <see cref="EFPPasteFormat.TestFormat"/>
  /// </summary>
  public class EFPTestDataObjectEventArgs : EFPPasteDataObjectEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает объект аргументов.
    /// Вызывается из <see cref="EFPPasteFormat"/>.
    /// </summary>
    /// <param name="pasteFormat">Объект <see cref="EFPPasteFormat"/></param>
    /// <param name="data">Содержит данные буфера обмена, которые предполагается вставлять</param>
    /// <param name="reason">Причина вызова: обычная или специальная вставка</param>
    /// <param name="actionCode">Код действия <see cref="EFPPasteAction.Code"/> или пустая строка для обычных команд вставки (<see cref="EFPPasteFormat.HasActions"/>=false)</param>
    public EFPTestDataObjectEventArgs(EFPPasteFormat pasteFormat, IDataObject data, EFPPasteReason reason, string actionCode)
      : base(pasteFormat, data, reason, actionCode)
    {
      _Result = EFPTestDataObjectResult.NoData;
    }

    #endregion

    #region Свойства

    ///// <summary>
    ///// Сюда обработчиком события должно быть помещено значение true, если формат применим и false,
    ///// если данные отсутствуют или не подходят.
    ///// </summary>
    //public bool Appliable { get { return _Appliable; } set { _Appliable = value; } }
    //private bool _Appliable;

    /// <summary>
    /// Результаты тестирования данных.
    /// По умолчанию - <see cref="EFPTestDataObjectResult.NoData"/>.
    /// </summary>
    public EFPTestDataObjectResult Result { get { return _Result; } set { _Result = value; } }
    private EFPTestDataObjectResult _Result;

    /// <summary>
    /// Дублирует свойство <see cref="Result"/>.
    /// Сохранено для совместимости.
    /// </summary>
    public bool Appliable
    {
      get { return Result == EFPTestDataObjectResult.Ok; }
      set { Result = value ? EFPTestDataObjectResult.Ok : EFPTestDataObjectResult.ApplyError; }
    }

    /// <summary>
    /// Сюда может быть помещен текст сообщения, почему данные не подходят или
    /// отсутствуют; или описание обнаруженных данных для списка Special Paste.
    /// </summary>
    public string DataInfoText { get { return _DataInfoText; } set { _DataInfoText = value; } }
    private string _DataInfoText;

    /// <summary>
    /// Сюда может быть помещен значок для обнаруженных данных или значок ошибки
    /// для списка Special Paste
    /// </summary>
    public string DataImageKey { get { return _DataImageKey; } set { _DataImageKey = value; } }
    private string _DataImageKey;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="EFPPasteFormat.TestFormat"/>
  /// </summary>
  /// <param name="sender">Ссылка на EFPPasteFormat.TestFormat</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPTestDataObjectEventHandler(object sender, EFPTestDataObjectEventArgs args);

  /// <summary>
  /// Аргументы события <see cref="EFPPasteFormat.Preview"/>
  /// </summary>
  public class EFPPreviewDataObjectEventArgs : EFPPasteDataObjectEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает объект аргумента
    /// </summary>
    /// <param name="pasteFormat">Объект <see cref="EFPPasteFormat"/></param>
    /// <param name="data">Данные буфера обмена</param>
    /// <param name="reason">Причина вызова: обычная или специальная вставка</param>
    /// <param name="actionCode">Код действия <see cref="EFPPasteAction.Code"/> или пустая строка для обычных команд вставки (<see cref="EFPPasteFormat.HasActions"/>=false)</param>
    public EFPPreviewDataObjectEventArgs(EFPPasteFormat pasteFormat, IDataObject data, EFPPasteReason reason, string actionCode)
      : base(pasteFormat, data, reason, actionCode)
    {
      Cancel = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Если установить в true, то вставка после просмотра не будет выполнена.
    /// </summary>
    public bool Cancel { get { return _Cancel; } set { _Cancel = value; } }
    private bool _Cancel;

    /// <summary>
    /// Рекомендуемый заголовок для просмотра
    /// </summary>
    public string Title
    {
      get
      {
        return String.Format(Res.EFPPasteFormat_Title_Preview, PasteFormat.DisplayName);
      }
    }

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="EFPPasteFormat.Preview"/>
  /// </summary>
  /// <param name="sender">Ссылка на <see cref="EFPPasteFormat"/>. Дублируется в свойстве <see cref="EFPPasteDataObjectEventArgs.PasteFormat"/>.</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPPreviewDataObjectEventHandler(object sender,
    EFPPreviewDataObjectEventArgs args);

  #endregion

  /// <summary>
  /// <para>
  /// Обработчик команд <see cref="EFPCommandItem"/> "Вставка" и "Специальная вставка" для локального меню управляющего элемента.
  /// Содержит список описателей <see cref="EFPPasteFormat"/>. При выполнении обычной вставки
  /// выполняется поиск первого подходящего формата в списке вызовом события <see cref="EFPPasteFormat.TestFormat"/>.
  /// При специальной вставке предлагается выбрать формат из списка подходящих.
  /// </para>
  /// <para>
  /// Также могут быть определены "действия" <see cref="EFPPasteAction"/>. Для каждого действия добавляется команда в локальном меню.
  /// У кнопки "Вставить" (если задается для панели инструментов) появляется треугольник, который открывает выпадающее меню "Дополнительные варианты вставки".
  /// </para>
  /// </summary>
  public class EFPPasteHandler : ListWithReadOnly<EFPPasteFormat>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект обработчика.
    /// Добавляет команды <see cref="EFPCommandItem"/> к локальному меню.
    /// </summary>
    /// <param name="commandItems">Список команд, куда будут добавлены команды "Вставить" и "Специальная вставка".
    /// Не может быть null</param>
    public EFPPasteHandler(EFPCommandItems commandItems)
    {
#if DEBUG
      if (commandItems == null)
        throw new ArgumentNullException("commandItems");
#endif
      _CommandItems = commandItems;

      ciPaste = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Paste);
      ciPaste.Click += new EventHandler(PasteClick);
      _CommandItems.Add(ciPaste);

      menuPasteAux = new EFPCommandItem("Edit", "PasteAuxMenu");
      menuPasteAux.MenuText = Res.Cmd_Menu_Edit_PasteAuxMenu;
      menuPasteAux.Usage = EFPCommandItemUsage.None;
      _CommandItems.Add(menuPasteAux);

      ciPasteSpecial = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.PasteSpecial);
      ciPasteSpecial.Click += new EventHandler(PasteSpecialClick);
      _CommandItems.Add(ciPasteSpecial);

      _AlwaysEnabled = false;
      _Enabled = true;
      _UseToolBar = true;
      _CommandItems.Prepare += CommandItems_Prepare;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список команд, куда добавлены команды "Вставить" и "Специальная вставка".
    /// Задается в конструкторе.
    /// </summary>
    public EFPCommandItems CommandItems { get { return _CommandItems; } }
    private readonly EFPCommandItems _CommandItems;

    /// <summary>
    /// Если true (по умолчанию), то кнопка "Вставить" будет на панели инструментов.
    /// Свойство можно устанавливать только до вывода формы на экран
    /// </summary>
    public bool UseToolBar
    {
      get { return _UseToolBar; }
      set
      {
        _CommandItems.CheckNotReadOnly();
        _UseToolBar = value;
      }
    }
    private bool _UseToolBar;

    /// <summary>
    /// Вызывается после выполнения команд вставки
    /// </summary>
    public event EventHandler PasteApplied;

    #endregion

    #region Prepare

    private void CommandItems_Prepare(object sender, EventArgs args)
    {
      SetReadOnly(); // 27.02.2025

      if (Count == 0)
      {
        ciPaste.Usage = EFPCommandItemUsage.None;
        menuPasteAux.Usage = EFPCommandItemUsage.None;
        ciPasteSpecial.Usage = EFPCommandItemUsage.None;
      }
      else
      {
        ciPaste.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
        if (UseToolBar)
          ciPaste.Usage |= EFPCommandItemUsage.ToolBar;

        ciPasteSpecial.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
        if (HasActions)
        {
          menuPasteAux.Usage = EFPCommandItemUsage.Menu;
          if (UseToolBar)
            menuPasteAux.Usage |= EFPCommandItemUsage.ToolBarDropDown;

          EFPPasteAction[] actions = this.Actions;
          _ActionCommandItems = new EFPCommandItem[actions.Length];
          for (int i = 0; i < actions.Length; i++)
          {
            EFPCommandItem ciAction = new EFPCommandItem("Edit", "Paste." + actions[i].Code);
            _ActionCommandItems[i] = ciAction;
            ciAction.MenuText = actions[i].MenuText;
            ciAction.ImageKey = actions[i].ImageKey;
            ciAction.Tag = actions[i];
            ciAction.Click += PasteAction_Click;
            ciAction.Parent = menuPasteAux;
            ciAction.Usage = EFPCommandItemUsage.Menu;
            _CommandItems.Add(ciAction);
          }

          ciPasteSpecial.Parent = menuPasteAux;
          ciPasteSpecial.GroupBegin = true;
        }
        else
          menuPasteAux.Usage = EFPCommandItemUsage.None;

        if (Enabled)
          SetRealEnabled();

        /*
        foreach (EFPPasteFormat format in this)
        {
          if (format.HasIdle)
          {
            CommandItems.Idle += CommandItems_Idle;
          }
        }*/
        if (!EFPApp.EasyInterface)
          CommandItems.Idle += CommandItems_Idle;
      }

    }

    #endregion

    #region Обработчики команд меню

    private EFPCommandItem ciPaste, menuPasteAux, ciPasteSpecial;

    private void PasteClick(object sender, EventArgs args)
    {
      PerformPaste(EFPPasteReason.Paste);
    }

    private void PasteAction_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      EFPPasteAction action = (EFPPasteAction)(ci.Tag);
      PerformPaste(EFPPasteReason.Paste, action.Code);
    }

    private void PasteSpecialClick(object sender, EventArgs args)
    {
      PerformPaste(EFPPasteReason.PasteSpecial);
    }

    /// <summary>
    /// Вызов событий <see cref="EFPPasteFormat.Idle"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void CommandItems_Idle(object sender, EventArgs args)
    {
#if DEBUG
      EFPApp.CheckMainThread();
#endif

      foreach (EFPPasteFormat format in this)
        format.HandleIdle();
      if (Enabled)
        SetRealEnabled();
    }

    /// <summary>
    /// true, если команды "Вставка" и "Специальная вставка" видимы.
    /// По умолчанию - true.
    /// </summary>
    public bool Visible
    {
      get { return ciPaste.Visible; }
      set
      {
        ciPaste.Visible = value;
        ciPasteSpecial.Visible = value;
      }
    }

    /// <summary>
    /// true, если команды "Вставка" и "Специальная вставка" доступны.
    /// По умолчанию - true.
    /// Если свойство <see cref="AlwaysEnabled"/> не установлено, то доступность команд определяется
    /// управляющим элементом (доступностью команд редактирования для табличного просмотра).
    /// </summary>
    public bool Enabled
    {
      get { return _Enabled; }
      set
      {
        if (value == _Enabled)
          return;

        _Enabled = value;
        if (value)
        {
          if (EFPApp.EasyInterface)
            SetAllEnabled(true);
          else
            SetRealEnabled();
        }
        else
          SetAllEnabled(false);
      }
    }

    private void SetAllEnabled(bool value)
    {
      ciPaste.Enabled = value;
      ciPasteSpecial.Enabled = value;
      if (_ActionCommandItems != null)
      {
        foreach (EFPCommandItem ci in _ActionCommandItems)
          ci.Enabled = value;
      }
    }

    [DebuggerStepThrough]
    private IDataObject ClipboardGetDataObjectForTest()
    {

      try
      {
        Form frm = Form.ActiveForm;
        if (frm != null)
        {
          if (frm.WindowState == FormWindowState.Minimized)
          {
            //Trace.WriteLine("Form.ActiveForm.WindowState=Minimized");
            return null; // 27.02.2025 Без этого возникает глюк. 
                         // Видимо, запрос к буферу выполняется от имени активной формы приложения.
                         // Но если она свернута, то после такого запроса ее нельзя восстановить с помощью мыши.
          }
        }

        return Clipboard.GetDataObject();
      }
      catch
      {
        return null;
      }
    }

    [DebuggerStepThrough] // Clipboard.GetDataObject() часто выбрасывает исключение
    private void SetRealEnabled()
    {
      try
      {
        bool hasAnyFormat = false;

        IDataObject dataObj = ClipboardGetDataObjectForTest();
        if (dataObj != null)
        {
          for (int j = 0; j < Count; j++)
          {
            if (dataObj.GetDataPresent(this[j].DataFormat, this[j].AutoConvert))
            {
              hasAnyFormat = true;
              break;
            }
          }
        }

        ciPaste.Enabled = hasAnyFormat;
        if (HasActions)
        {
          EFPPasteAction[] actions = this.Actions;
          for (int i = 0; i < actions.Length; i++)
          {
            bool enabled = false;
            if (hasAnyFormat)
            {
              for (int j = 0; j < Count; j++)
              {
                if (this[j].HasActions)
                {
                  EFPPasteAction thisAct = this[j].Actions[actions[i].Code];
                  if (thisAct != null &&
                    dataObj.GetDataPresent(this[j].DataFormat, this[j].AutoConvert))
                  {
                    if (thisAct.Enabled)
                    {
                      enabled = true;
                      break;
                    }
                  }
                }
              }
            }
            _ActionCommandItems[i].Enabled = enabled;
          }
        }
        ciPasteSpecial.Enabled = true;
      }
      catch
      {
        SetAllEnabled(true);
      }
    }

    private bool _Enabled;

    /// <summary>
    /// Если true, то доступность команд не будет зависеть от доступности на редактирование табличного
    /// просмотра.
    /// По умолчанию - false - объект-владелец управляет доступностью команд.
    /// </summary>
    public bool AlwaysEnabled
    {
      get { return _AlwaysEnabled; }
      set
      {
        _CommandItems.CheckNotReadOnly();
        _AlwaysEnabled = value;
        if (value)
          Enabled = true;
      }
    }
    private bool _AlwaysEnabled;

    /// <summary>
    /// Возвращает true, если для какого-либо формата определены действия <see cref="EFPPasteAction"/>.
    /// При этом будут добавлены дополнительные команды локального меню и треугольник выпадающего меню рядом с кнопкой на панели инструментов.
    /// </summary>
    public bool HasActions
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (this[i].HasActions)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Возвращает список действий <see cref="EFPPasteAction"/> для всех форматов.
    /// Действия с одинаковыми кодами отбрасываются.
    /// Для полученного списка будут добавлены дополнительные команды локального меню.
    /// Если <see cref="HasActions"/>=false, то возвращается пустой массив.
    /// </summary>
    public EFPPasteAction[] Actions
    {
      get
      {
        NamedList<EFPPasteAction> lst = null;
        for (int i = 0; i < Count; i++)
        {
          if (this[i].HasActions)
          {
            if (lst == null)
              lst = new NamedList<EFPPasteAction>();
            foreach (EFPPasteAction action in this[i].Actions)
            {
              if (!lst.Contains(action.Code))
                lst.Add(action);
            }
          }
        }
        if (lst == null)
          return new EFPPasteAction[0];
        else
          return lst.ToArray();
      }
    }

    private EFPCommandItem[] _ActionCommandItems;

    #endregion

    #region Выполнение действий

    private static string _LastSpecialPasteName;

    /// <summary>
    /// Если установить флажок в true, то в списке выбора формата для специальной вставки будут представлены
    /// все форматы данных, включая отсутствующие в буфере обмена
    /// </summary>
    public static bool PasteSpecialDebugMode { get { return _PasteSpecialDebugMode; } set { _PasteSpecialDebugMode = value; } }
    private static bool _PasteSpecialDebugMode = false;

    /// <summary>
    /// Выполнить вставку из буфера обмена
    /// </summary>
    /// <param name="reason">Обычная или специальная вставка</param>
    public void PerformPaste(EFPPasteReason reason)
    {
      PerformPaste(reason, String.Empty);
    }

    /// <summary>
    /// Выполнить вставку из буфера обмена
    /// </summary>
    /// <param name="reason">Обычная или специальная вставка</param>
    /// <param name="actionCode">Код действия <see cref="EFPPasteAction.Code"/> или пустая строка для обычных команд вставки (<see cref="EFPPasteFormat.HasActions"/>=false).
    /// Используется только при <paramref name="reason"/>=<see cref="EFPPasteReason.Paste"/>.</param>
    public void PerformPaste(EFPPasteReason reason, string actionCode)
    {
      IDataObject data = new EFPClipboard().GetDataObject();
      PerformPaste(data, reason, actionCode);
    }

    /// <summary>
    /// Выполнить вставку данных.
    /// </summary>
    /// <param name="data">Данные из буфера обмена</param>
    /// <param name="reason">Обычная или специальная вставка</param>
    public void PerformPaste(IDataObject data, EFPPasteReason reason)
    {
    }

    /// <summary>
    /// Выполнить вставку данных.
    /// </summary>
    /// <param name="data">Данные из буфера обмена</param>
    /// <param name="reason">Обычная или специальная вставка</param>
    /// <param name="actionCode">Код действия <see cref="EFPPasteAction.Code"/> или пустая строка для обычных команд вставки (<see cref="EFPPasteFormat.HasActions"/>=false).
    /// Используется только при <paramref name="reason"/>=<see cref="EFPPasteReason.Paste"/>.</param>
    public void PerformPaste(IDataObject data, EFPPasteReason reason, string actionCode)
    {
      EFPApp.BeginWait(Res.Clipboard_Phase_GetData, "Paste");
      try
      {
        string dataInfoText;
        if (PerformPaste(data, reason, actionCode, out dataInfoText))
        {
          if (PasteApplied != null)
            PasteApplied(this, EventArgs.Empty);
        }
        else
        {
          if (dataInfoText != null)
          {
            if (reason == EFPPasteReason.PasteSpecial)
              EFPApp.ErrorMessageBox(dataInfoText, EFPCommandItem.RemoveMnemonic(Res.Cmd_Menu_Edit_PasteSpecial));
            else
              EFPApp.ShowTempMessage(dataInfoText);
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// Выполнить вставку данных.
    /// </summary>
    /// <param name="data">Данные из буфера обмена</param>
    /// <param name="reason">Обычная или специальная вставка</param>
    /// <param name="dataInfoText">Сюда помещается сообщение об ошибке.
    /// Если пользователь отказался выполнить специальную вставку, содержит null</param>
    /// <param name="actionCode">Код действия <see cref="EFPPasteAction.Code"/> или пустая строка для обычных команд вставки (<see cref="EFPPasteFormat.HasActions"/>=false).
    /// Используется только при <paramref name="reason"/>=<see cref="EFPPasteReason.Paste"/>.</param>
    /// <returns>Возвращает true, если вставка выполнена</returns>
    public bool PerformPaste(IDataObject data, EFPPasteReason reason, string actionCode, out string dataInfoText)
    {
      if (data == null)
      {
        dataInfoText = Res.Clipboard_Err_Empty;
        return false;
      }
      if (Count == 0)
      {
        dataInfoText = Res.EFPPasteFormat_Err_NoPasteFormats;
        return false;
      }

      dataInfoText = null;
      switch (reason)
      {
        case EFPPasteReason.Paste:
          // Ищем первый подходящий формат
          for (int i = 0; i < Count; i++)
          {
            EFPTestDataObjectEventArgs args = new EFPTestDataObjectEventArgs(this[i], data, reason, actionCode);
            DoTestFormat(args);
            if (args.Result == EFPTestDataObjectResult.Ok)
            {
              DoPaste(args);
              return true;
            }
            // Используем сообщение об ошибке для первого формата в списке
            if (dataInfoText == null)
              dataInfoText = args.DataInfoText;
          }
          return false;

        case EFPPasteReason.PasteSpecial:
          // Выводим список подходящих форматов
          List<string> goodNames = new List<string>();
          List<string> goodImageKeys = new List<string>();
          List<EFPPasteFormat> goodFormats = new List<EFPPasteFormat>();
          List<string> goodActionCodes = new List<string>();
          List<bool> goodValidFlags = new List<bool>();
          int selIdx = 0;
          ErrorMessageList errors = new ErrorMessageList();
          for (int i = 0; i < Count; i++)
          {
            if (this[i].HasActions)
            {
              foreach (EFPPasteAction action in this[i].Actions)
              {
                EFPTestDataObjectEventArgs args = new EFPTestDataObjectEventArgs(this[i], data, reason, action.Code);
                DoTestFormat(args);
                if (args.Result != EFPTestDataObjectResult.NoData || PasteSpecialDebugMode)
                {
                  goodNames.Add(action.DisplayName + " - " + args.DataInfoText);
                  goodImageKeys.Add(action.ImageKey);
                  goodFormats.Add(this[i]);
                  goodActionCodes.Add(action.Code);
                  goodValidFlags.Add(args.Result != EFPTestDataObjectResult.NoData);
                  if (this[i].DisplayName == _LastSpecialPasteName)
                    selIdx = goodNames.Count - 1;
                }
                else
                {
                  errors.AddError(String.Format(Res.EFPPasteFormat_Err_FormatError, action.DisplayName, args.DataInfoText));
                }
              }
            }
            else
            {
              EFPTestDataObjectEventArgs args = new EFPTestDataObjectEventArgs(this[i], data, reason, actionCode);
              DoTestFormat(args);
              if (args.Result != EFPTestDataObjectResult.NoData || PasteSpecialDebugMode)
              {
                string name = args.DataInfoText;
                goodNames.Add(name);
                goodImageKeys.Add(args.DataImageKey);
                goodFormats.Add(this[i]);
                goodActionCodes.Add(String.Empty);
                goodValidFlags.Add(args.Result != EFPTestDataObjectResult.NoData);
                if (this[i].DisplayName == _LastSpecialPasteName)
                  selIdx = goodNames.Count - 1;
              }
              else
              {
                errors.AddError(String.Format(Res.EFPPasteFormat_Err_FormatError, this[i].DisplayName, args.DataInfoText));
              }
            }
          }

          if (goodNames.Count == 0)
          {
            EFPApp.ShowErrorMessageListDialog(errors, EFPCommandItem.RemoveMnemonic(Res.Cmd_Menu_Edit_PasteSpecial));
            return false;
          }

          ListSelectDialog dlg = new ListSelectDialog();
          dlg.Title = EFPCommandItem.RemoveMnemonic(Res.Cmd_Menu_Edit_PasteSpecial);
          if (PasteSpecialDebugMode)
            dlg.Title = String.Format(Res.EFPPasteFormat_Title_AllFormats, dlg.Title);
          dlg.ImageKey = "Paste";
          dlg.Items = goodNames.ToArray();
          dlg.ListTitle = Res.EFPPasteFormat_Title_SpecialPasteFormatGroup;
          dlg.ImageKeys = goodImageKeys.ToArray();
          dlg.ConfigSectionName = "SpecialPasteFormatDialog";

          if (PasteSpecialDebugMode)
          {
            dlg.SubItems = new string[goodNames.Count];
            for (int i = 0; i < goodNames.Count; i++)
              dlg.SubItems[i] = goodFormats[i].DisplayName;
          }

          dlg.SelectedIndex = selIdx;

          if (dlg.ShowDialog() != DialogResult.OK)
            return false;

          _LastSpecialPasteName = goodNames[dlg.SelectedIndex];

          if (goodValidFlags[dlg.SelectedIndex])
          {
            if (PasteSpecialDebugMode)
            {
              EFPPreviewDataObjectEventArgs args2 = new EFPPreviewDataObjectEventArgs(goodFormats[dlg.SelectedIndex], data, reason, goodActionCodes[dlg.SelectedIndex]);
              goodFormats[dlg.SelectedIndex].PerformPreview(args2);
              if (args2.Cancel)
                return false;
            }

            EFPPasteDataObjectEventArgs args = new EFPPasteDataObjectEventArgs(goodFormats[dlg.SelectedIndex], data, reason, goodActionCodes[dlg.SelectedIndex]);
            DoPaste(args);
            return true;
          }
          else
          {
            EFPApp.ErrorMessageBox(String.Format(Res.EFPPasteFormat_Err_PasteSpecial,
              goodNames[dlg.SelectedIndex]),
              goodFormats[dlg.SelectedIndex].DisplayName);
            return false;
          }

        default:
          throw ExceptionFactory.ArgUnknownValue("reason", reason);
      }
    }

    private void DoTestFormat(EFPTestDataObjectEventArgs args)
    {
      CheckNotBusy();
      _TestingFormat = args.PasteFormat;
      try
      {
        args.PasteFormat.PerformTestFormat(args);
      }
      finally
      {
        _TestingFormat = null;
      }
    }

    private void DoPaste(EFPPasteDataObjectEventArgs args)
    {
      CheckNotBusy();
      _PastingFormat = args.PasteFormat;
      try
      {
        args.PasteFormat.PerformPaste(args);
      }
      finally
      {
        _PastingFormat = null;
      }
    }

    private void CheckNotBusy()
    {
      if (_TestingFormat != null)
        throw new InvalidOperationException(String.Format(Res.EFPPasteFormat_Err_BusyTest, _TestingFormat.DisplayName));
      if (_PastingFormat != null)
        throw new InvalidOperationException(String.Format(Res.EFPPasteFormat_Err_BusyPaste, _PastingFormat.DisplayName));
    }

    #endregion

    #region Текущее действие

    /// <summary>
    /// Возвращает true, если в данный момент выполняется тестирование одной из команд
    /// (выполняется обработчик события <see cref="EFPPasteFormat.TestFormat"/>.
    /// </summary>
    public bool IsTesting { get { return _TestingFormat != null; } }
    private EFPPasteFormat _TestingFormat;

    /// <summary>
    /// Возвращает true, если в данный момент выполняется вставка для одной из команд
    /// (выполняется обработчик события <see cref="EFPPasteFormat.Paste"/>.
    /// </summary>
    public bool IsPasting { get { return _PastingFormat != null; } }
    private EFPPasteFormat _PastingFormat;

    /// <summary>
    /// Возвращает описатель формата, для которого в данный момент выполняется тестирование
    /// (при <see cref="IsTesting"/>=true) или вставка (при <see cref="IsPasting"/>=true).
    /// Если ни тестирование, ни вставка не выполняется, возвращается null.
    /// </summary>
    public EFPPasteFormat CurrentFormat
    {
      get
      {
        if (_PastingFormat == null)
          return _TestingFormat;
        else
          return _PastingFormat;
      }
    }

    #endregion
  }

  /// <summary>
  /// Описатель/обработчик для одного формата данных.
  /// Созданные объекты должны быть добавлены к обработчику команд <see cref="EFPPasteHandler"/>.
  /// </summary>
  public class EFPPasteFormat
  {
    #region Конструкторы

    /// <summary>
    /// Создает описание формата
    /// </summary>
    /// <param name="dataFormat">Имя формата данных, например, <see cref="DataFormats.Text"/></param>
    public EFPPasteFormat(string dataFormat)
    {
#if DEBUG
      if (String.IsNullOrEmpty(dataFormat))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("dataFormat");
#endif
      _DataFormat = dataFormat;
    }

    /// <summary>
    /// Создает формат для типа данных
    /// </summary>
    /// <param name="type">Тип данных</param>
    public EFPPasteFormat(Type type)
      : this(type.FullName)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Формат данных. Одна из констант в <see cref="DataFormats"/> или пользовательский формат.
    /// Задается в конструкторе.
    /// </summary>
    public string DataFormat { get { return _DataFormat; } }
    private readonly string _DataFormat;

    /// <summary>
    /// Имя для отображения пользователю.
    /// Не зависит от текущих данных в буфере обмена.
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
          return DataFormat;
        else
          return _DisplayName;
      }
      set
      {
        _DisplayName = value;
      }
    }
    private string _DisplayName;

    /// <summary>
    /// Список для добавления действий.
    /// Для каждого описателя действия добавляется команда локального меню.
    /// Действия с одинаковыми кодами у разных форматов <see cref="EFPPasteFormat"/> объединяются при построении локального меню.
    /// В большинстве случаев список пустой.
    /// </summary>
    public NamedList<EFPPasteAction> Actions
    {
      get
      {
        if (_Actions == null)
          _Actions = new NamedList<EFPPasteAction>();
        return _Actions;
      }
    }
    private NamedList<EFPPasteAction> _Actions;

    /// <summary>
    /// Возвращает true, если для формата было добавлено хотя бы одно действие в список <see cref="Actions"/>.
    /// </summary>
    public bool HasActions
    {
      get
      {
        if (_Actions == null)
          return false;
        else
          return _Actions.Count > 0;
      }
    }

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    #endregion

    #region Определение наличия данных подходящего формата

    /// <summary>
    /// Используется в базовой реализации <see cref="OnTestFormatEvent(EFPTestDataObjectEventArgs)"/>.
    /// Если true, то при вызове <see cref="IDataObject.GetDataPresent(string, bool)"/> будет выполнена попытка
    /// преобразования данных.
    /// По умолчанию - false - преобразование не выполняется.
    /// </summary>
    public bool AutoConvert { get { return _AutoConvert; } set { _AutoConvert = value; } }
    private bool _AutoConvert;

    /// <summary>
    /// Это событие вызывается для проверки, содержит ли буфер обмена данные
    /// в подходящем формате.
    /// Обрабочик события должен установить свойство <see cref="EFPTestDataObjectEventArgs.Result"/>.
    /// Когда выполняется обычная вставка, будет использован первый в списке описатель формата, который сообщил, что данные подходят.
    /// </summary>
    public event EFPTestDataObjectEventHandler TestFormat;

    /// <summary>
    /// Проверить наличие данных подходящего формата
    /// </summary>
    /// <param name="data">Объект данных в буфере обмена</param>
    /// <param name="reason">Обычная или специальная вставка</param>
    /// <param name="dataInfoText">Сюда записывается описание формата или сообщение об ошибке</param>
    /// <param name="dataImageKey">Сюда записывается код значка</param>
    /// <returns>true, если формат подходит</returns>
    [Obsolete("Use method overload with EFPTestDataObjectEventArgs", false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool PerformTestFormat(IDataObject data, EFPPasteReason reason, out string dataInfoText, out string dataImageKey)
    {
      EFPTestDataObjectEventArgs args = new EFPTestDataObjectEventArgs(this, data, reason, String.Empty);
      PerformTestFormat(args);
      dataInfoText = args.DataInfoText;
      dataImageKey = args.DataImageKey;
      return args.Result == EFPTestDataObjectResult.Ok;
    }

    /// <summary>
    /// Проверить наличие данных подходящего формата.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    public void PerformTestFormat(EFPTestDataObjectEventArgs args)
    {
      try
      {
        if (args.Data == null)
        {
          args.DataInfoText = Res.Clipboard_Err_Empty;
          args.DataImageKey = "No";
          args.Result = EFPTestDataObjectResult.NoData;
        }
        else
        {
          if ((!String.IsNullOrEmpty(args.ActionCode)) && HasActions)
          {
            if (!Actions.Contains(args.ActionCode))
            {
              args.DataInfoText = String.Format(Res.EFPPasteFormat_Err_ActionCode, args.ActionCode);
              args.DataImageKey = "No";
              args.Result = EFPTestDataObjectResult.ApplyError;
              return;
            }
          }

          args.DataInfoText = DisplayName;
          args.DataImageKey = "Item";
          OnTestFormat(args);
          if (String.IsNullOrEmpty(args.DataInfoText))
          {
            switch (args.Result)
            {
              case EFPTestDataObjectResult.ApplyError:
                args.DataInfoText = Res.EFPPasteFormat_Err_FormatNotAppliable;
                break;
              case EFPTestDataObjectResult.Ok:
                args.DataInfoText = DisplayName;
                break;
              default:
                args.DataInfoText = args.Result.ToString();
                break;
            }
          }
        }
      }
      catch (Exception e)
      {
        args.Result = EFPTestDataObjectResult.FormatError;
        args.DataInfoText = String.Format(Res.EFPPasteFormat_Err_TestError, e.Message);
        args.DataImageKey = "Error";
      }
    }

    /// <summary>
    /// Выполнить тестирование формата.
    /// Переопределенный метод не должен запрашивать у пользователя какие-либо параметры вставки,
    /// так как метод может вызываться для определения всех доступных форматов до выполнения реальных действий.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnTestFormat(EFPTestDataObjectEventArgs args)
    {
      if (args.Data.GetDataPresent(DataFormat, AutoConvert))
        args.Result = EFPTestDataObjectResult.Ok;
      else
      {
        args.Result = EFPTestDataObjectResult.NoData;
        args.DataInfoText = String.Format(Res.Clipboard_Err_NoDataFormat, DisplayName);
        args.DataImageKey = "No";
        return;
      }

      OnTestFormatEvent(args);
    }

    /// <summary>
    /// Вызов пользовательского обработчика <see cref="TestFormat"/>.
    /// Этот метод должен вызываться из переопределенных методов <see cref="OnTestFormatEvent(EFPTestDataObjectEventArgs)"/>.
    /// Предполагается, что базовая проверка наличия формата уже выполнена.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected void OnTestFormatEvent(EFPTestDataObjectEventArgs args)
    {
      if (TestFormat != null)
        TestFormat(this, args);
    }

    #endregion

    #region Предварительный просмотр

    /// <summary>
    /// Это событие вызывается в отладочном режиме для предварительного просмотра
    /// данных перед вставкой.
    /// </summary>
    public event EFPPreviewDataObjectEventHandler Preview;

    /// <summary>
    /// Выполнить предварительный просмотр.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    [Obsolete("Используйте method overload with EFPPreviewDataObjectEventArgs", false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool PerformPreview(IDataObject data, EFPPasteReason reason)
    {
      EFPPreviewDataObjectEventArgs args = new EFPPreviewDataObjectEventArgs(this, data, reason, String.Empty);
      PerformPreview(args);
      return !args.Cancel;
    }

    /// <summary>
    /// Выполнить предварительный просмотр.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    public void PerformPreview(EFPPreviewDataObjectEventArgs args)
    {
      OnPreview(args);
    }

    /// <summary>
    /// Выполнить предварительный просмотр данных, которые будут вставлены.
    /// Вызывает обработчик события <see cref="Preview"/>, если он установлен.
    /// Иначе выводится отладочная информация об объекте данных.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnPreview(EFPPreviewDataObjectEventArgs args)
    {
      if (Preview == null)
      {
        FreeLibSet.Forms.Diagnostics.DebugTools.DebugObject(args.GetData(), PreviewTitle);
        args.Cancel = false;
      }
      else
        Preview(this, args);
    }

    /// <summary>
    /// Возвращает true, если есть обработчик события <see cref="Preview"/>
    /// </summary>
    protected bool HasPreviewHandler { get { return Preview != null; } }

    /// <summary>
    /// Заголовок для диалога Preview.
    /// </summary>
    protected string PreviewTitle { get { return String.Format(Res.EFPPasteFormat_Title_Preview, DisplayName); } }

    #endregion

    #region Применение формата

    /// <summary>
    /// Это событие вызывается для применения выбранного пользователем формата.
    /// Обработчик должен быть обязательно установлен, если метод <see cref="OnPaste"/> не переопределен.
    /// </summary>
    public event EFPPasteDataObjectEventHandler Paste;

    /// <summary>
    /// Выполнить вставку
    /// </summary>
    /// <param name="data">Данные из буфера обмена</param>
    /// <param name="reason">Обычная или специальная вставка</param>
    [Obsolete("Use overload method with EFPPasteDataObjectEventArgs", false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void PerformPaste(IDataObject data, EFPPasteReason reason)
    {
      EFPPasteDataObjectEventArgs args = new EFPPasteDataObjectEventArgs(this, data, reason, String.Empty);
      PerformPaste(args);
    }

    /// <summary>
    /// Выполнить вставку
    /// </summary>
    /// <param name="args">Аргументы события</param>
    public void PerformPaste(EFPPasteDataObjectEventArgs args)
    {
      OnPaste(args);
    }

    /// <summary>
    /// Вызывается при вставке данных.
    /// Непереопредеденный метод вызывает обработчик события <see cref="Paste"/>.
    /// Если обработчик события не установлен, выбрасывается исключение.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnPaste(EFPPasteDataObjectEventArgs args)
    {
      if (Paste == null)
        throw ExceptionFactory.ObjectEventHandlerNotSet(this, "Paste");

      Paste(this, args);
    }

    #endregion

    #region Событие Idle

    /// <summary>
    /// Событие может использоваться, если требуется динамическая установка свойств <see cref="EFPPasteAction.Enabled"/>
    /// </summary>
    public EventHandler Idle;

    internal void HandleIdle()
    {
      if (Idle != null)
        Idle(this, EventArgs.Empty);
    }

    internal bool HasIdle { get { return Idle != null; } }

    #endregion
  }

  /// <summary>
  /// Описание одного действия для команд вставки.
  /// Добавление действия в список <see cref="EFPPasteFormat.Actions"/> приводит к появлению
  /// подменю "Дополнительные варианты вставки" с командами меню
  /// </summary>
  public sealed class EFPPasteAction : IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает описатель действия с заданным кодом
    /// </summary>
    /// <param name="code">Код действия. Не может быть пустым</param>
    public EFPPasteAction(string code)
      : this(code, String.Empty, String.Empty)
    {
    }

    /// <summary>
    /// Создает описатель действия с заданным кодом и текстом команды меню.
    /// </summary>
    /// <param name="code">Код действия. Не может быть пустым</param>
    /// <param name="displayName">Текст дополнительной команды меню</param>
    public EFPPasteAction(string code, string displayName)
      : this(code, displayName, String.Empty)
    {
    }

    /// <summary>
    /// Создает описатель действия с заданным кодом, текстом команды меню и значком.
    /// </summary>
    /// <param name="code">Код действия. Не может быть пустым</param>
    /// <param name="displayName">Текст дополнительной команды меню</param>
    /// <param name="imageKey">Имя изображения в списке <see cref="EFPApp.MainImages"/></param>
    public EFPPasteAction(string code, string displayName, string imageKey)
    {
      if (String.IsNullOrEmpty(code))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("code");
      _Code = code;
      _MenuText = displayName;
      _ImageKey = imageKey;
      _Enabled = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Код действия.
    /// Передается обработчикам событий как свойство <see cref="EFPPasteDataObjectEventArgs.ActionCode"/>.
    /// При построении локального меню действия с одинаковыми кодами объединяются.
    /// </summary>
    public string Code { get { return _Code; } }
    private readonly string _Code;

    /// <summary>
    /// Текст для дополнительной команды меню.
    /// Если свойство не установлено, возвращается <see cref="Code"/>.
    /// </summary>
    public string MenuText
    {
      get
      {
        if (String.IsNullOrEmpty(_MenuText))
          return Code;
        else
          return _MenuText;
      }
      set { _MenuText = value; }
    }
    private string _MenuText;

    /// <summary>
    /// Название для отображения в списке
    /// </summary>
    public string DisplayName { get { return EFPCommandItem.RemoveMnemonic(MenuText); } }

    /// <summary>
    /// Имя изображения из <see cref="EFPApp.MainImages"/> для значка команды локального меню.
    /// Если не установлено в явном виде, возвращается "Item".
    /// </summary>
    public string ImageKey
    {
      get
      {
        if (String.IsNullOrEmpty(_ImageKey))
          return "Item";
        else
          return _ImageKey;
      }
      set { _ImageKey = value; }
    }
    private string _ImageKey;

    /// <summary>
    /// Признак доступности команды локального меню для выбора действия.
    /// Свойство может устанавливаться в пользовательском обработчике события <see cref="EFPPasteFormat.Idle"/>.
    /// </summary>
    public bool Enabled
    {
      get { return _Enabled; }
      set { _Enabled = value; }
    }
    private bool _Enabled;

    /// <summary>
    /// Возвращает <see cref="Code"/>
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Code;
    }

    #endregion
  }


  /// <summary>
  /// Версия обработчика для вставки форматов CSV или текст в виде прямоугольной матрицы,
  /// например, для табличного просмотра.
  /// Содержит переопределенный метод <see cref="EFPPasteFormat.OnTestFormat(EFPTestDataObjectEventArgs)"/>,
  /// (однако, он может быть дополнен  пользовательским обработчиком). 
  /// Обработчик события <see cref="EFPPasteFormat.Paste"/> должен быть реализован. 
  /// При этом он может использовать готовое свойство <see cref="TextMatrix"/>.
  /// </summary>
  public class EFPPasteTextMatrixFormat : EFPPasteFormat
  {
    #region Конструктор

    /// <summary>
    /// Создает формат
    /// </summary>
    /// <param name="isCSV">true - формат CSV, false - формат Text</param>
    public EFPPasteTextMatrixFormat(bool isCSV)
      : base(isCSV ? DataFormats.CommaSeparatedValue : DataFormats.Text)
    {
      AutoConvert = !isCSV;
      _IsCSV = isCSV;
      if (isCSV)
        DisplayName = Res.EFPPasteTextMatrixFormat_Name_Csv;
      else
        DisplayName = Res.EFPPasteTextMatrixFormat_Name_Text;

      _SingleLineSeparators = DataTools.EmptyStrings;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// true - формат CSV, false - формат Text.
    /// Задается в конструкторе.
    /// </summary>
    public bool IsCSV { get { return _IsCSV; } }
    private readonly bool _IsCSV;

    /// <summary>
    /// После вызова TestFormat сюда помещается текст в виде прямоугольной матрицы
    /// </summary>
    public string[,] TextMatrix { get { return _TextMatrix; } }
    private string[,] _TextMatrix;

    #endregion

    #region Переопределенные методы

    private string _UsedSeparator;

    /// <summary>
    /// Выполняет проверку наличия подходящего текста (формат Text или CSV, в зависимости от свойства <see cref="IsCSV"/>) в буфере обмена.
    /// Если все в порядке, то устанавливает свойство <see cref="TextMatrix"/>.
    /// Затем вызывает метод <see cref="EFPPasteFormat.OnTestFormatEvent(EFPTestDataObjectEventArgs)"/> для выполнения окончательной проверки в пользовательском коде.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnTestFormat(EFPTestDataObjectEventArgs args)
    {

      _TextMatrix = null;
      _UsedSeparator = null;
      try
      {
        if (IsCSV)
          _TextMatrix = WinFormsTools.GetTextMatrixCsv(args.Data);
        else
        {
          //_TextMatrix = WinFormsTools.GetTextMatrixText(args.Data);
          string s = WinFormsTools.GetText(args.Data);
          if (!String.IsNullOrEmpty(s))
          {
            _TextMatrix = ConvertTextToTextMatrix(s);
          }
        }
      }
      catch (Exception e)
      {
        args.DataInfoText = String.Format(Res.EFPPasteTextMatrixFormat_Err_ToMatrix, e.Message);
        args.DataImageKey = "Error";
        args.Result = EFPTestDataObjectResult.FormatError;
        return;
      }

      if (_TextMatrix == null)
      {
        args.DataInfoText = String.Format(Res.Clipboard_Err_NoDataFormat, DisplayName);
        args.DataImageKey = "No";
        args.Result = EFPTestDataObjectResult.NoData;
        return;
      }

      args.Result = EFPTestDataObjectResult.Ok;
      if (_TextMatrix.GetLength(0) == 1 && _TextMatrix.GetLength(1) == 1)
      {
        args.DataInfoText = DisplayName;
        args.DataImageKey = "Font"; // ??
      }
      else
      {
        args.DataInfoText = DisplayName + " (" + _TextMatrix.GetLength(0).ToString() + " x " + _TextMatrix.GetLength(1).ToString() + ")";
        if (!String.IsNullOrEmpty(_UsedSeparator))
          args.DataInfoText = String.Format(Res.EFPPasteTextMatrixFormat_Name_WithLineSeparator, args.DataInfoText, _UsedSeparator);
        args.DataImageKey = "Table";
      }

      base.OnTestFormatEvent(args);
    }

    /// <summary>
    /// Альтернативные разделители строк.
    /// Используются, если в буфере обмена хранится единственная строка текста и в ней нет символов табуляции.
    /// Сепараторы просматриваются по очереди и используется первый обнаруженный из них.
    /// Если сепаратор найден, на выходе получается матрица из одного столбца.
    /// По умолчанию массив пустой.
    /// </summary>
    public string[] SingleLineSeparators
    {
      get { return _SingleLineSeparators; }
      set
      {
        if (value == null)
          _SingleLineSeparators = DataTools.EmptyStrings;
        else
          _SingleLineSeparators = value;
      }
    }
    private string[] _SingleLineSeparators;

    /// <summary>
    /// Преобразование строки текста в текстовую матрицу.
    /// Вызывается при <see cref="IsCSV"/>=false, когда в буфере обмена есть непустая строка текста.
    /// Непереопределенный метод использует <see cref="TabTextConvert"/> для преобразования.
    /// Если получена матрица из одной ячейки, используется свойство <see cref="SingleLineSeparators"/> для
    /// преобразование в одноколоночную матрицу.
    ///
    /// Не используется при <see cref="IsCSV"/>=true.
    /// </summary>
    /// <param name="s">Строка из буфера обмена</param>
    /// <returns>Матрица или null, если преобразование не удалось</returns>
    protected virtual string[,] ConvertTextToTextMatrix(string s)
    {
      TabTextConvert conv = new TabTextConvert();
      conv.AutoDetectNewLine = true;
      string[,] matrix = conv.ToMatrix(s); // не может быть null.
      if (matrix.GetLength(0) == 1 && matrix.GetLength(1) == 1 && _SingleLineSeparators.Length > 0)
      {
        // Используем строку из матрицы, а не исходную строку, так как в конце строки могли быть символы
        // новой строки, которые были убраны TabTextConvert.
        s = matrix[0, 0];
        for (int i = 0; i < SingleLineSeparators.Length; i++)
        {
          if (s.Contains(SingleLineSeparators[i]))
          {
            _UsedSeparator = SingleLineSeparators[i];
            string[] a = s.Split(new string[1] { SingleLineSeparators[i] }, StringSplitOptions.None);
            matrix = DataTools.MatrixFromColumns<string>(a);
          }
        }
      }
      return matrix;
    }

    /// <summary>
    /// Выполняет предварительный просмотр в виде таблицы
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnPreview(EFPPreviewDataObjectEventArgs args)
    {
      if (HasPreviewHandler)
      {
        base.OnPreview(args);
        return;
      }

      if (TextMatrix == null)
      {
        EFPApp.MessageBox(String.Format(Res.Clipboard_Err_NoDataFormat, DisplayName),
          PreviewTitle);
        args.Cancel = false; // 27.12.2020
        return;
      }

      OKCancelGridForm frm = new OKCancelGridForm();
      frm.Text = PreviewTitle;
      InitPreviewGrid(frm.Control, TextMatrix);

      args.Cancel = EFPApp.ShowDialog(frm, true) != DialogResult.OK;
    }

    private static void InitPreviewGrid(DataGridView control, string[,] textMatrix)
    {
      control.RowCount = textMatrix.GetLength(0);
      control.ColumnCount = textMatrix.GetLength(1);
      for (int i = 0; i < textMatrix.GetLength(0); i++)
      {
        for (int j = 0; j < textMatrix.GetLength(1); j++)
          control[j, i].Value = textMatrix[i, j];
      }
      for (int j = 0; j < textMatrix.GetLength(1); j++)
        control.Columns[j].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

      control.ReadOnly = true;
    }

    #endregion
  }

  /// <summary>
  /// Интерфейс команд локального меню для работы с буфером обмена Cut/Copy/Paste
  /// </summary>
  public interface IEFPClipboardCommandItems
  {
    /// <summary>
    /// Обработчик команды "Вырезать"
    /// </summary>
    event EventHandler Cut;

    /// <summary>
    /// Вызывается для выполнения команды "Копировать"
    /// </summary>
    event DataObjectEventHandler AddCopyFormats;

    /// <summary>
    /// Обработчик команды "Вставить"
    /// </summary>
    EFPPasteHandler PasteHandler { get; }

    /// <summary>
    /// Наличие кнопок на панели инструментов
    /// </summary>
    bool ClipboardInToolBar { get; set; }
  }

  internal interface IEFPDataViewClipboardCommandItems
  {
    EFPDataViewCopyFormats CopyFormats { get; set; }

    EFPDataViewCopyFormats SelectedCopyFormats { get; }
  }
}
