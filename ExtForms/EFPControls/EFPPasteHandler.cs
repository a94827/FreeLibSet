// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
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
  /// Аргументы события EFPPasteFormat.TestFormat
  /// </summary>
  public class EFPTestDataObjectEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает объект аргументов.
    /// Вызывается из EFPPasteFormat
    /// </summary>
    /// <param name="data">Содержит данные буфера обмены, которые предполагается вставлять</param>
    /// <param name="reason">Причина вызова: обычная или специальная вставка</param>
    public EFPTestDataObjectEventArgs(IDataObject data, EFPPasteReason reason)
    {
      _Data = data;
      _Reason = reason;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Содержит данные буфера обмены, которые предполагается вставлять
    /// </summary>
    public IDataObject Data { get { return _Data; } }
    private IDataObject _Data;

    /// <summary>
    /// Причина вызова: обычная или специальная вставка
    /// </summary>
    public EFPPasteReason Reason { get { return _Reason; } }
    private EFPPasteReason _Reason;

    /// <summary>
    /// Сюда должно быть помещено значение true, если формат применим и false,
    /// если данные отсутствуют или не подходят
    /// </summary>
    public bool Appliable { get { return _Appliable; } set { _Appliable = value; } }
    private bool _Appliable;

    /// <summary>
    /// Сюда может быть помещен текст сообщения, почему данные не подходят или
    /// отсутствуют или описание обнаруженных данных для списка Special Paste
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
  /// Делегат события EFPPasteFormat.TestFormat
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void EFPTestDataObjectEventHandler(object sender, EFPTestDataObjectEventArgs args);

  /// <summary>
  /// Аргументы события EFPPasteFormat.Paste
  /// </summary>
  public class EFPPasteDataObjectEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает объект аргумента
    /// </summary>
    /// <param name="owner">Объект EFPPasteFormat</param>
    /// <param name="data">Данные буфера обмена</param>
    /// <param name="reason">Причина вызова: обычная или специальная вставка</param>
    public EFPPasteDataObjectEventArgs(EFPPasteFormat owner, IDataObject data, EFPPasteReason reason)
    {
      _Owner = owner;
      _Data = data;
      _Reason = reason;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект EFPPasteFormat 
    /// </summary>
    public EFPPasteFormat Owner { get { return _Owner; } }
    private EFPPasteFormat _Owner;

    /// <summary>
    /// Данные буфера обмена
    /// </summary>
    public IDataObject Data { get { return _Data; } }
    private IDataObject _Data;


    /// <summary>
    /// Причина вызова: обычная или специальная вставка
    /// </summary>
    public EFPPasteReason Reason { get { return _Reason; } }
    private EFPPasteReason _Reason;

    /// <summary>
    /// Извлекает данные из объекта Data в формате, заданном в конструкторе
    /// объекта EFPDataFormat и с возможным преобразованием, в зависимости от
    /// EFPDataFormat.AutoConvert
    /// </summary>
    /// <returns></returns>
    public object GetData()
    {
      return Data.GetData(_Owner.DataFormat, _Owner.AutoConvert);
    }

    #endregion
  }

  /// <summary>
  /// Делегат события EFPPasteFormat.Paste
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void EFPPasteDataObjectEventHandler(object sender, EFPPasteDataObjectEventArgs args);

  /// <summary>
  /// Аргументы события EFPasteFormat.Preview
  /// </summary>
  public class EFPPreviewDataObjectEventArgs : EFPPasteDataObjectEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает объект аргумента
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="data"></param>
    /// <param name="reason"></param>
    public EFPPreviewDataObjectEventArgs(EFPPasteFormat owner, IDataObject data, EFPPasteReason reason)
      : base(owner, data, reason)
    {
      Cancel = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Если установить в true, то вставка после просмотра не будет выполнена
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
        return Owner.DisplayName + " - предварительный просмотр";
      }
    }

    #endregion
  }

  /// <summary>
  /// Делегат события EFPasteFormat.Preview
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void EFPPreviewDataObjectEventHandler(object sender,
    EFPPreviewDataObjectEventArgs args);

  #endregion

  /// <summary>
  /// Обработчик команд "Вставка" и "Специальная вставка"
  /// </summary>
  public class EFPPasteHandler : List<EFPPasteFormat>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект обработчика
    /// </summary>
    /// <param name="owner">Список команд, куда будут добавлены команды "Вставить" и "Специальная вставка"</param>
    public EFPPasteHandler(EFPCommandItems owner)
    {
      _Owner = owner;

      ciPaste = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Paste);
      ciPaste.Click += new EventHandler(PasteClick);
      _Owner.Add(ciPaste);

      ciPasteSpecial = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.PasteSpecial);
      ciPasteSpecial.Click += new EventHandler(PasteSpecialClick);
      _Owner.Add(ciPasteSpecial);

      _AlwaysEnabled = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список команд, куда будут добавлены команды "Вставить" и "Специальная вставка"
    /// </summary>
    public EFPCommandItems Owner { get { return _Owner; } }
    private EFPCommandItems _Owner;

    /// <summary>
    /// Вызывается после выполнения команд вставки
    /// </summary>
    public event EventHandler PasteApplied;

    #endregion

    #region Обработчики команд меню

    private EFPCommandItem ciPaste, ciPasteSpecial;

    private void PasteClick(object sender, EventArgs args)
    {
      PerformPaste(EFPPasteReason.Paste);
    }

    private void PasteSpecialClick(object sender, EventArgs args)
    {
      PerformPaste(EFPPasteReason.PasteSpecial);
    }

    /// <summary>
    /// Метод вызывается из переопределенных методов EFPControl.BeforeControlAssigned().
    /// </summary>
    public void InitCommandUsage()
    {
      InitCommandUsage(true);
    }

    /// <summary>
    /// Метод вызывается из переопределенных методов EFPControl.BeforeControlAssigned().
    /// Наличие команды "Вставить" в панели инструментов.
    /// </summary>
    public void InitCommandUsage(bool useToolBar)
    {
      if (Count == 0)
      {
        ciPaste.Usage = EFPCommandItemUsage.None;
        ciPasteSpecial.Usage = EFPCommandItemUsage.None;
      }
      else
      {
        ciPaste.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
        if (useToolBar)
          ciPaste.Usage |= EFPCommandItemUsage.ToolBar;

        ciPasteSpecial.Usage = ciPaste.Usage;
      }
    }

    /// <summary>
    /// true, если команды "Вставка" и "Специальная вставка" видимы.
    /// По умолчанию - true
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
    /// Если свойство AlwaysEnabled не установлено, то доступность команд определяется
    /// доступностью команд редактирования табличного просмотра
    /// </summary>
    public bool Enabled
    {
      get { return ciPaste.Enabled; }
      set
      {
        ciPaste.Enabled = value;
        ciPasteSpecial.Enabled = value;
      }
    }

    /// <summary>
    /// Если true, то доступность команд не будет зависеть от доступности на редактирование табличного
    /// просмотра.
    /// По умолчанию - false - объект-владелец управляет доступностью команд
    /// </summary>
    public bool AlwaysEnabled
    {
      get { return _AlwaysEnabled; }
      set
      {
        _Owner.CheckNotReadOnly();
        _AlwaysEnabled = value;
        if (value)
          Enabled = true;
      }
    }
    private bool _AlwaysEnabled;

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
      IDataObject Data = EFPApp.Clipboard.GetDataObject();
      PerformPaste(Data, reason);
    }

    /// <summary>
    /// Выполнить вставку данных.
    /// </summary>
    /// <param name="data">Данные из буфера обмена</param>
    /// <param name="reason">Обычная или специальная вставка</param>
    public void PerformPaste(IDataObject data, EFPPasteReason reason)
    {
      EFPApp.BeginWait("Вставка из буфера обмена", "Paste");
      try
      {
        string dataInfoText;
        if (PerformPaste(data, reason, out dataInfoText))
        {
          if (PasteApplied != null)
            PasteApplied(this, EventArgs.Empty);
        }
        else
        {
          if (dataInfoText != null)
            EFPApp.ShowTempMessage(dataInfoText);
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
    /// <returns>Возвращает true, если вставка выполнена</returns>
    public bool PerformPaste(IDataObject data, EFPPasteReason reason, out string dataInfoText)
    {
      if (data == null)
      {
        dataInfoText = "Буфер обмена пустой";
        return false;
      }
      if (Count == 0)
      {
        dataInfoText = "Не задано ни одного обработчика форматов данных";
        return false;
      }

      dataInfoText = null;
      switch (reason)
      {
        case EFPPasteReason.Paste:
          // Ищем первый подходящий формат
          for (int i = 0; i < Count; i++)
          {
            string dataInfoText2;
            string dataImageKey;
            if (DoTestFormat(this[i], data, reason, out dataInfoText2, out dataImageKey))
            {
              DoPaste(this[i], data, reason);
              return true;
            }
            // Используем сообщение об ошибке для первого формата в списке
            if (dataInfoText == null)
              dataInfoText = dataInfoText2;
          }
          return false;

        case EFPPasteReason.PasteSpecial:
          // Выводим список подходящих форматов
          List<string> goodNames = new List<string>();
          List<string> goodImageKeys = new List<string>();
          List<EFPPasteFormat> goodFormats = new List<EFPPasteFormat>();
          List<bool> goodValidFlags = new List<bool>();
          int selIdx = 0;
          for (int i = 0; i < Count; i++)
          {
            string dataInfoText2;
            string dataImageKey;
            bool validFlag = DoTestFormat(this[i], data, reason, out dataInfoText2, out dataImageKey);
            if (validFlag || PasteSpecialDebugMode)
            {
              goodNames.Add(dataInfoText2);
              goodImageKeys.Add(dataImageKey);
              goodFormats.Add(this[i]);
              goodValidFlags.Add(validFlag);
              if (this[i].DisplayName == _LastSpecialPasteName)
                selIdx = goodNames.Count - 1;
            }
          }
          if (goodNames.Count == 0)
          {
            dataInfoText = "В буфере обмена нет данных в подходящем формате";
            return false;
          }

          ListSelectDialog dlg = new ListSelectDialog();
          dlg.Title = "Специальная вставка";
          if (PasteSpecialDebugMode)
            dlg.Title += " (все форматы)";
          dlg.ImageKey = "Paste";
          dlg.Items = goodNames.ToArray();
          dlg.ListTitle = "Формат данных в буфере обмена";
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
              if (!goodFormats[dlg.SelectedIndex].PerformPreview(data, reason))
                return false;
            }
            DoPaste(goodFormats[dlg.SelectedIndex], data, reason);
            return true;
          }
          else
          {
            EFPApp.ErrorMessageBox("Нельзя выполнить вставку: " + goodNames[dlg.SelectedIndex],
              goodFormats[dlg.SelectedIndex].DisplayName);
            return false;
          }

        default:
          throw new ArgumentException("Неизвестный Reason=" + reason.ToString(), "reason");
      }
    }

    private bool DoTestFormat(EFPPasteFormat format, IDataObject data, EFPPasteReason reason, out string dataInfoText, out string dataImageKey)
    {
      bool res;

      CheckNotBusy();
      _TestingFormat = format;
      try
      {
        res = format.PerformTestFormat(data, reason, out dataInfoText, out dataImageKey);
      }
      finally
      {
        _TestingFormat = null;
      }
      return res;
    }

    private void DoPaste(EFPPasteFormat format, IDataObject data, EFPPasteReason reason)
    {
      CheckNotBusy();
      _PastingFormat = format;
      try
      {
        format.PerformPaste(data, reason);
      }
      finally
      {
        _PastingFormat = null;
      }
    }

    private void CheckNotBusy()
    {
      if (_TestingFormat != null)
        throw new InvalidOperationException("В данный момент выполняется тестирование другого формата вставки (" + _TestingFormat.DisplayName + ")");
      if (_PastingFormat != null)
        throw new InvalidOperationException("В данный момент выполняется вставка для формата (" + _PastingFormat.DisplayName + ")");
    }

    #endregion

    #region Текущее действие

    /// <summary>
    /// Возвращает true, если в данный момент выполняется тестирование одной из команд
    /// </summary>
    public bool IsTesting { get { return _TestingFormat != null; } }
    private EFPPasteFormat _TestingFormat;

    /// <summary>
    /// Возвращает true, если в данный момент выполняется встака для одной из команд
    /// </summary>
    public bool IsPasting { get { return _PastingFormat != null; } }
    private EFPPasteFormat _PastingFormat;

    /// <summary>
    /// Возвращает формат, для которого в данный момент выполняется тестирование
    /// (при IsTesting=true) или вставка (при IsPasting=true)
    /// Если ни тестирование, ни вставка не выполняется, возвращается null
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
  /// Обработчик для одного формата данных
  /// </summary>
  public class EFPPasteFormat
  {
    #region Конструкторы

    /// <summary>
    /// Создает описание формата
    /// </summary>
    /// <param name="dataFormat">Имя формата данных, например, DataFormats.Text</param>
    public EFPPasteFormat(string dataFormat)
    {
#if DEBUG
      if (String.IsNullOrEmpty(dataFormat))
        throw new ArgumentNullException("dataFormat");
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
    /// Формат данных. Одна из констант в DataFormats или пользовательский формат
    /// Задается в конструкторе
    /// </summary>
    public string DataFormat { get { return _DataFormat; } }
    private string _DataFormat;

    /// <summary>
    /// Имя для отображения пользователю
    /// Не зависит от текущих данных в буфере обмена
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
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    #endregion

    #region Определение наличия данных подходящего формата

    /// <summary>
    /// Используется в базовой реализации OnTestFormat.
    /// Если true, то при вызове IDataObject.GetDataPresent() будет выполнена попытка
    /// преобразования данных.
    /// По умолчанию - false - преобразование не выполняется
    /// </summary>
    public bool AutoConvert { get { return _AutoConvert; } set { _AutoConvert = value; } }
    private bool _AutoConvert;

    /// <summary>
    /// Это событие вызывается для проверки, содержит ли буфер обмена данные
    /// в подходящем формате 
    /// </summary>
    public event EFPTestDataObjectEventHandler TestFormat;

    /// <summary>
    /// Проверить наличие данных подходящего формата
    /// </summary>
    /// <param name="data">Объект данных в буфере обмена</param>
    /// <param name="reason"></param>
    /// <param name="dataInfoText"></param>
    /// <param name="dataImageKey"></param>
    /// <returns></returns>
    public bool PerformTestFormat(IDataObject data, EFPPasteReason reason, out string dataInfoText, out string dataImageKey)
    {
      bool res;
      try
      {
        res = OnTestFormat(data, reason, out dataInfoText, out dataImageKey);
      }
      catch (Exception e)
      {
        res = false;
        dataInfoText = "Ошибка при определении применимости формата. " + e.Message;
        dataImageKey = "Error";
      }
      return res;
    }

    /// <summary>
    /// Выполнить тестирование формата.
    /// Переопределенный метод не должен запрашивать у пользователя какие-либо параметры вставки,
    /// так как метод может вызываться для определения всех доступных форматов до выполнения реальных действий.
    /// </summary>
    /// <param name="data">Данные буфера обмена</param>
    /// <param name="reason">Вставка или специальная вставка</param>
    /// <param name="dataInfoText">Сюда записывается текстовое описание данных в буфере обмена или
    /// сообщение об ошибке, если нет подходящих данных</param>
    /// <param name="dataImageKey">Сюда записывается имя изображения из списка EFPApp.MainImages,
    /// которое будет использовано в качестве значка в списке "Специальная вставка".
    /// В случае ошибки сюда записывается "No" или другой подходящий значок</param>
    /// <returns>True, если данные из буфера могут быть вставлены</returns>
    protected virtual bool OnTestFormat(IDataObject data, EFPPasteReason reason, out string dataInfoText, out string dataImageKey)
    {
      if (data == null)
      {
        dataInfoText = "Нет данных";
        dataImageKey = "No";
        return false;
      }
      dataInfoText = DisplayName;
      dataImageKey = "Item";
      bool appliable = data.GetDataPresent(DataFormat, AutoConvert);
      if (!appliable)
      {
        dataInfoText = "Нет данных в формате " + DisplayName;
        dataImageKey = "No";
      }

      OnTestFormatEvent(data, reason, ref appliable, ref dataInfoText, ref dataImageKey);

      return appliable;
    }

    /// <summary>
    /// Вызов пользовательского обработчика TestFormat.
    /// Этот метод должен вызываться из переопределенных методов TestFormat()
    /// Предполагается, что базовая проверка уже выполнена.
    /// </summary>
    /// <param name="data">Данные буфера обмена</param>
    /// <param name="reason">Вставка или специальная вставка</param>
    /// <param name="appliable">True, если данные из буфера могут быть вставлены</param>
    /// <param name="dataInfoText">Сюда записывается текстовое описание данных в буфере обмена или
    /// сообщение об ошибке, если нет подходящих данных</param>
    /// <param name="dataImageKey">Сюда записывается имя изображения из списка EFPApp.MainImages,
    /// которое будет использовано в качестве значка в списке "Специальная вставка".
    /// В случае ошибки сюда записывается "No" или другой подходящий значок</param>
    protected void OnTestFormatEvent(IDataObject data, EFPPasteReason reason, ref bool appliable, ref string dataInfoText, ref string dataImageKey)
    {
      if (TestFormat == null)
        return;

      EFPTestDataObjectEventArgs args = new EFPTestDataObjectEventArgs(data, reason);
      args.Appliable = appliable;
      args.DataInfoText = dataInfoText;
      args.DataImageKey = dataImageKey;
      TestFormat(this, args);

      dataInfoText = args.DataInfoText;
      dataImageKey = args.DataImageKey;
      appliable = args.Appliable;
      if ((!args.Appliable) && (String.IsNullOrEmpty(dataInfoText)))
        dataInfoText = "Формат не применим";
    }

    #endregion

    #region Предварительный просмотр

    /// <summary>
    /// Это событие вызывается в отладочном режиме для предварительного просмотра
    /// данных перед вставкой
    /// </summary>
    public event EFPPreviewDataObjectEventHandler Preview;

    /// <summary>
    /// Выполнить предварительный просмотр.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    public bool PerformPreview(IDataObject data, EFPPasteReason reason)
    {
      return OnPreview(data, reason);
    }

    /// <summary>
    /// Выполнить предварительный просмотр данных, которые будут вставлены.
    /// </summary>
    /// <param name="data">Вставляет данные</param>
    /// <param name="reason">Вставка или специальная вставка</param>
    /// <returns>True, если просмотр выполнен.
    /// False, если обработчик события Preview установил Cancel=true</returns>
    protected virtual bool OnPreview(IDataObject data, EFPPasteReason reason)
    {
      EFPPreviewDataObjectEventArgs args = new EFPPreviewDataObjectEventArgs(this, data, reason);
      if (Preview == null)
      {
        FreeLibSet.Forms.Diagnostics.DebugTools.DebugObject(args.GetData(), PreviewTitle);
        return true;
      }

      Preview(this, args);
      return !args.Cancel;
    }

    /// <summary>
    /// Возвращает true, если есть обработчик события Preview
    /// </summary>
    protected bool HasPreviewHandler { get { return Preview != null; } }

    /// <summary>
    /// Заголовок для диалога Preview.
    /// </summary>
    protected string PreviewTitle { get { return DisplayName + " - предварительный просмотр"; } }

    #endregion

    #region Применение формата

    /// <summary>
    /// Это событие вызывается для применения выбранного пользователем формата
    /// Обработчик должен быть обязательно установлен, если метод OnPaste не
    /// переопределен
    /// </summary>
    public event EFPPasteDataObjectEventHandler Paste;

    /// <summary>
    /// Выполнить вставку
    /// </summary>
    /// <param name="data">Данные из буфера обмена</param>
    /// <param name="reason">Обычная или специальная вставка</param>
    public void PerformPaste(IDataObject data, EFPPasteReason reason)
    {
      OnPaste(data, reason);
    }

    /// <summary>
    /// Вызывается при вставке данных.
    /// Непереопредеденный метод вызывает событие Paste
    /// </summary>
    /// <param name="data">Данные из буфера обмена</param>
    /// <param name="reason">Обычная или специальная вставка</param>
    protected virtual void OnPaste(IDataObject data, EFPPasteReason reason)
    {
      if (Paste == null)
        throw new NullReferenceException("Обработчик события Paste не установлен");

      EFPPasteDataObjectEventArgs args = new EFPPasteDataObjectEventArgs(this, data, reason);
      Paste(this, args);
    }

    #endregion
  }


  /// <summary>
  /// Версия обработчика для вставки форматов CSV или текст в виде прямоугольной матрицы,
  /// например, для табличного просмотра.
  /// Содержит встроенный обработчик TestFormat (однако, он может быть дополнен 
  /// пользовательским обработчиком). Обработчик Paste должен быть реализован. При
  /// этом он может использовать готовое свойство TextMatrix
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
        DisplayName = "Текст, разделенный запятыми";
      else
        DisplayName = "Неформатированный текст";
    }

    #endregion

    #region Свойства

    /// <summary>
    /// true - формат CSV, false - формат Text
    /// </summary>
    public bool IsCSV { get { return _IsCSV; } }
    private bool _IsCSV;

    /// <summary>
    /// После вызова TestFormat сюда помещается текст в виде прямоугольной матрицы
    /// </summary>
    public string[,] TextMatrix { get { return _TextMatrix; } }
    private string[,] _TextMatrix;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Выполняет проверку наличия подходящего текста (формат Text или CSV) в буфере обмена
    /// Если все в порядке, то устанавливает свойство TextMatrix.
    /// Затем вызывает метод OnTestFormatEvent() для выполнения окончательной проверки
    /// </summary>
    /// <param name="data">Данные буфера обмена</param>
    /// <param name="reason">Вставка или специальная вставка</param>
    /// <param name="dataInfoText">Сюда записывается текстовое описание данных в буфере обмена или
    /// сообщение об ошибке, если нет подходящих данных</param>
    /// <param name="dataImageKey">Сюда записывается имя изображения из списка EFPApp.MainImages,
    /// которое будет использовано в качестве значка в списке "Специальная вставка".
    /// В случае ошибки сюда записывается "No" или другой подходящий значок</param>
    /// <returns>True, если данные из буфера могут быть вставлены</returns>
    protected override bool OnTestFormat(IDataObject data, EFPPasteReason reason, out string dataInfoText, out string dataImageKey)
    {

      _TextMatrix = null;

      try
      {
        if (IsCSV)
          _TextMatrix = WinFormsTools.GetTextMatrixCsv(data);
        else
          _TextMatrix = WinFormsTools.GetTextMatrixText(data);
      }
      catch (Exception e)
      {
        dataInfoText = "Ошибка преобразования текста в прямоугольный блок: " + e.Message;
        dataImageKey = "Error";
        return false;
      }

      if (_TextMatrix == null)
      {
        dataInfoText = "Нет данных в формате " + (IsCSV ? "CSV" : "Текст");
        dataImageKey = "No";
        return false;
      }

      bool appliable = true;
      if (_TextMatrix.GetLength(0) == 1 && _TextMatrix.GetLength(1) == 1)
      {
        dataInfoText = DisplayName;
        dataImageKey = "Font"; // ??
      }
      else
      {
        dataInfoText = DisplayName + " (" + _TextMatrix.GetLength(0).ToString() + " x " + _TextMatrix.GetLength(1).ToString() + ")";
        dataImageKey = "Table";
      }

      base.OnTestFormatEvent(data, reason, ref appliable, ref dataInfoText, ref dataImageKey);
      return appliable;
    }

    /// <summary>
    /// Выполняет предварительный просмотр в виде таблицы
    /// </summary>
    /// <param name="data"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    protected override bool OnPreview(IDataObject data, EFPPasteReason reason)
    {
      if (HasPreviewHandler)
        return base.OnPreview(data, reason);

      if (TextMatrix == null)
      {
        EFPApp.MessageBox("Нет данных", PreviewTitle);
        return true; // 27.12.2020
      }

      OKCancelGridForm frm = new OKCancelGridForm();
      frm.Text = PreviewTitle;
      frm.Control.RowCount = TextMatrix.GetLength(0);
      frm.Control.ColumnCount = TextMatrix.GetLength(1);
      for (int i = 0; i < TextMatrix.GetLength(0); i++)
      {
        for (int j = 0; j < TextMatrix.GetLength(1); j++)
          frm.Control[j, i].Value = TextMatrix[i, j];
      }
      for (int j = 0; j < TextMatrix.GetLength(1); j++)
        frm.Control.Columns[j].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

      frm.Control.ReadOnly = true;

      return EFPApp.ShowDialog(frm, true) == DialogResult.OK;
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
}
