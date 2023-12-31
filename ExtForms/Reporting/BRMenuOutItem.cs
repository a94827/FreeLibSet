using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Collections;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Drawing.Reporting;
using FreeLibSet.IO;
using FreeLibSet.Reporting;
using FreeLibSet.Shell;

namespace FreeLibSet.Forms.Reporting
{
  #region Перечисления

  /// <summary>
  /// Выполняемое действие: печать, экспорт в файл или отправка
  /// </summary>
  public enum BRAction
  {
    /// <summary>
    /// Диалог "Параметры страницы"
    /// </summary>
    Print,

    /// <summary>
    /// Выполняется команда "Экспорт в файл".
    /// Диалог показывается после создания отчета <see cref="BRReport"/> и ввода имени файла пользователем.
    /// </summary>
    ExportFile,

    /// <summary>
    /// Параметры команды "Файл" - "Отправить".
    /// Диалог показывается до создания отчета
    /// </summary>
    SendTo,
  }

  /// <summary>
  /// Описание действия над отчетом <see cref="BRReport"/>: выполняемое действие, имя файла для экспорта и описание команды отправки/формата экспорта.
  /// </summary>
  public struct BRActionInfo
  {
    #region Свойства

    /// <summary>
    /// Выполняемое действие: Печать, экспорт в файл или команда "Отправить".
    /// </summary>
    public BRAction Action { get { return _Action; } internal set { _Action = value; } }
    private BRAction _Action;

    /// <summary>
    /// Описание формата файла при <see cref="Action"/>=<see cref="BRAction.ExportFile"/>.
    /// </summary>
    public BRExportFileItem ExportFileItem { get { return _ExportFileItem; } internal set { _ExportFileItem = value; } }
    private BRExportFileItem _ExportFileItem;

    /// <summary>
    /// Выбранное пользователем имя файла при <see cref="Action"/>=<see cref="BRAction.ExportFile"/>.
    /// </summary>
    public AbsPath FilePath { get { return _FilePath; } internal set { _FilePath = value; } }
    private AbsPath _FilePath;

    /// <summary>
    /// Описание получателя при <see cref="Action"/>=<see cref="BRAction.SendTo"/>.
    /// </summary>
    public BRSendToItem SendToItem { get { return _SendToItem; } internal set { _SendToItem = value; } }
    private BRSendToItem _SendToItem;

    #endregion
  }

  /// <summary>
  /// Режим показа блока диалога в <see cref="BRMenuOutItem"/>
  /// </summary>
  public enum BRDialogKind
  {
    /// <summary>
    /// Диалог "Параметры страницы".
    /// Диалог может показываться и для управляющего элемента, к которому относится <see cref="BRMenuOutItem"/>, и из окна предварительного просмотра.
    /// Эти диалоги не должны отличаться.
    /// </summary>
    PageSetup,

    /// <summary>
    /// Выполняется команда "Экспорт в файл" для управляющего элемента, к которому относится <see cref="BRMenuOutItem"/>.
    /// Диалог показывается после ввода имени файла пользователем, но до создания отчета <see cref="BRReport"/> в режиме <see cref="BRAction.ExportFile"/>.
    /// </summary>
    ControlExportFile,

    /// <summary>
    /// Выполняется команда "Экспорт в файл" из окна предварительного просмотра.
    /// Диалог показывается после создания отчета <see cref="BRReport"/> в режиме <see cref="BRAction.Print"/> и ввода имени файла пользователем.
    /// Диалог не должен содержать параметров, влияющих на создание отчета <see cref="BRReport"/>.
    /// </summary>
    PreviewExportFile,

    /// <summary>
    /// Выполняется команда "Файл" - "Отправить" для управляющего элемента, к которому относится <see cref="BRMenuOutItem"/>.
    /// Диалог показывается до создания отчета <see cref="BRReport"/> в режиме <see cref="BRAction.SendTo"/>.
    /// </summary>
    ControlSendTo,

    /// <summary>
    /// Выполняется команда "Файл" - "Отправить" из окна предварительного просмотра.
    /// Диалог показывается после создания отчета <see cref="BRReport"/> в режиме <see cref="BRAction.Print"/> .
    /// Диалог не должен содержать параметров, влияющих на создание отчета <see cref="BRReport"/>.
    /// </summary>
    PreviewSendTo,
  }

  #endregion

  #region Делегаты

  /// <summary>
  /// Аргументы события <see cref="BRMenuOutItem.InitDialog"/> 
  /// </summary>
  public sealed class BRMenuOutItemInitDialogEventArgs : EventArgs
  {
    #region Защищенный конструктор

    internal BRMenuOutItemInitDialogEventArgs(SettingsDialog dialog, BRDialogKind dialogKind, BRActionInfo actionInfo)
    {
      _Dialog = dialog;
      _DialogKind = dialogKind;
      _ActionInfo = actionInfo;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Заполняемый диалог. У него должны заполняться вкладки <see cref="SettingsDialog.Pages"/>.
    /// Используйте свойство <see cref="DialogKind"/> для определения необходимости добавления вкладок
    /// </summary>
    public SettingsDialog Dialog { get { return _Dialog; } }
    private readonly SettingsDialog _Dialog;

    /// <summary>
    /// Режим показа диалога
    /// </summary>
    public BRDialogKind DialogKind { get { return _DialogKind; } }
    private readonly BRDialogKind _DialogKind;

    /// <summary>
    /// Информация о выполняемом действии: печати, экспорте в файл или отправки во внешнее приложение.
    /// </summary>
    public BRActionInfo ActionInfo { get { return _ActionInfo; } }
    private readonly BRActionInfo _ActionInfo;

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Добавляет в диалог вкладку "Шрифты".
    /// Шрифты сохраняются в объекте <see cref="BRFontSettingsDataItem"/>, который должен быть присоединен к <see cref="SettingsDialog.Data"/>.
    /// </summary>
    public void AddFontPage()
    {
      Dialog.Data.GetRequired<BRFontSettingsDataItem>();
      new BRPageSetupFont(Dialog);
    }

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="BRMenuOutItem.InitDialog"/> 
  /// </summary>
  /// <param name="sender">Объект <see cref="BRMenuOutItem"/> </param>
  /// <param name="args">Аргументы события</param>
  public delegate void BRMenuOutItemInitDialogEventHandler(object sender, BRMenuOutItemInitDialogEventArgs args);

  /// <summary>
  /// Аргумент события <see cref="BRMenuOutItem.CreateReport"/>
  /// </summary>
  public sealed class BRMenuOutItemCreateReportEventArgs : EventArgs
  {
    #region Защищенный конструктор

    internal BRMenuOutItemCreateReportEventArgs(BRActionInfo actionInfo)
    {
      _ActionInfo = actionInfo;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Информация о выполняемом действии: печати, экспорте в файл или выполнении команды "Отправить".
    /// </summary>
    public BRActionInfo ActionInfo { get { return _ActionInfo; } }
    private readonly BRActionInfo _ActionInfo;

    /// <summary>
    /// Отчет, который должен быть заполнен.
    /// Можно также присвоить значение свойству.
    /// </summary>
    public BRReport Report
    {
      get
      {
        if (_Report == null)
          _Report = new BRReport();
        return _Report;
      }
      set
      {
        _Report = value;
      }
    }
    private BRReport _Report;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="BRMenuOutItem.CreateReport"/>
  /// </summary>
  /// <param name="sender">Объект <see cref="BRMenuOutItem"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void BRMenuOutItemCreateReportEventHandler(object sender, BRMenuOutItemCreateReportEventArgs args);

  /// <summary>
  /// Аргументы события <see cref="BRExportFileItem.PerformAction"/> и <see cref="BRSendToItem.PerformAction"/>
  /// </summary>
  public sealed class BRActionItemPerformEventArgs : EventArgs
  {
    #region Конструктор

    internal BRActionItemPerformEventArgs(BRMenuOutItem outItem, BRReport report, BRActionInfo actionInfo, ISimpleSplash splash)
    {
      _OutItem = outItem;
      _Report = report;
      _ActionInfo = actionInfo;
      if (splash == null)
        _Splash = new DummySplash();
      else
        _Splash = splash;
    }

    internal BRActionItemPerformEventArgs(BRActionItemPerformEventArgs baseArgs, BRActionInfo actionInfo)
      : this(baseArgs.OutItem, baseArgs.Report, actionInfo, baseArgs.Splash)
    {
    }


    #endregion

    #region Свойства

    /// <summary>
    /// Заполненный отчет.
    /// Обработчик события не должен вносить изменения в отчет, для этого следует использовать обработчик события <see cref="BRMenuOutItem.CreateReport"/>.
    /// </summary>
    public BRReport Report { get { return _Report; } }
    private readonly BRReport _Report;

    /// <summary>
    /// Объект печати, для которого выполняется действие
    /// </summary>
    public BRMenuOutItem OutItem { get { return _OutItem; } }
    private readonly BRMenuOutItem _OutItem;

    /// <summary>
    /// Информация о выполняемом действии.
    /// </summary>
    public BRActionInfo ActionInfo { get { return _ActionInfo; } }
    private readonly BRActionInfo _ActionInfo;

    /// <summary>
    /// Объект для управления заставкой.
    /// </summary>
    public ISimpleSplash Splash { get { return _Splash; } }
    private readonly ISimpleSplash _Splash;

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Этот метод можно использовать для упрощения создания файла
    /// </summary>
    /// <param name="fileCreator">Объект для создания файла, производный от <see cref="BRFileCreator"/></param>
    public void CreateFile(BRFileCreator fileCreator)
    {
      if (fileCreator == null)
        throw new ArgumentNullException("fileCreator");

      fileCreator.Splash = Splash;
      fileCreator.CreateFile(Report, ActionInfo.FilePath);
    }

    /// <summary>
    /// Этот метод можно использовать для создания файла перед выполнением команды "Отправить".
    /// Используется <see cref="BRExportFileItem"/>, добавленный в список форматов файлов <see cref="EFPMenuOutItem.ExportFileItems"/>
    /// </summary>
    /// <param name="code">Код формата файла в списке <see cref="EFPMenuOutItem.ExportFileItems"/></param>
    /// <param name="filePath">Путь к создаваемому файлу</param>
    public void CreateFile(string code, AbsPath filePath)
    {
      BRExportFileItem expItem = (BRExportFileItem)(OutItem.ExportFileItems[code]);
      if (expItem == null)
        throw new ArgumentException("В списке форматов файлов для экспорта нет записи с кодом \"" + code + "\"");

      BRActionInfo info2 = new BRActionInfo();
      info2.Action = BRAction.ExportFile;
      info2.FilePath = filePath;
      info2.ExportFileItem = expItem;
      BRActionItemPerformEventArgs args2 = new BRActionItemPerformEventArgs(this, info2);
      ((IBRActionItem)expItem).CallPerformAction(args2);
    }

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="BRExportFileItem.PerformAction"/> и <see cref="BRSendToItem.PerformAction"/>
  /// </summary>
  /// <param name="sender">Объект <see cref="BRExportFileItem"/> или <see cref="BRSendToItem"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void BRActionItemPerformEventHandler(object sender, BRActionItemPerformEventArgs args);

  //public class BRMenuOutItemInitEventArgs : EFPMenuOutItemPrepareEventArgs
  //{
  //  public BRMenuOutItemInitEventArgs(EFPContextCommandItems commandItems, BRMenuOutItem outItem)
  //    : base(commandItems)
  //  {
  //    _OutItem = outItem;
  //  }

  //  public BRMenuOutItem OutItem { get { return _OutItem; } }
  //  private readonly BRMenuOutItem _OutItem;
  //}

  //public delegate void BRMenuOutItemInitEventHandler(object sender, BRMenuOutItemInitEventArgs args);

  #endregion

  /// <summary>
  /// Общая часть <see cref="BRExportFileItem"/> и <see cref="BRSendToItem"/>
  /// </summary>
  internal interface IBRActionItem
  {
    #region Свойства

    /// <summary>
    /// Если true (по умолчанию), то действие может быть выполнено для управляющего элемента, к которому присоединен <see cref="BRMenuOutItem"/>.
    /// </summary>
    bool UseWithControl { get; }

    /// <summary>
    /// Если true (по умолчанию), то действие может быть выполнено из окна предварительного просмотра.
    /// </summary>
    bool UseWithPreview { get; }

    #endregion

    #region Методы

    /// <summary>
    /// Вызвать обработчик инициализации диалога параметров
    /// </summary>
    /// <param name="args">Аргументы события</param>
    void CallInitDialog(BRMenuOutItemInitDialogEventArgs args);

    /// <summary>
    /// Выполнить основное действие над отчетом
    /// </summary>
    /// <param name="args">Аргументы события</param>
    void CallPerformAction(BRActionItemPerformEventArgs args);

    #endregion
  }

  /// <summary>
  /// Расширение описания формата экспорта в файл для <see cref="BRMenuOutItem"/> 
  /// </summary>
  public class BRExportFileItem : EFPExportFileItem, IBRActionItem
  {
    #region Конструктор

    /// <summary>
    /// Создает описание
    /// </summary>
    /// <param name="code">Код формата</param>
    /// <param name="filterText">Описание фильтра для диалога сохранения файла</param>
    /// <param name="fileMask">Маска для выбора файлов</param>
    /// <param name="performAction">Обработчик события <see cref="PerformAction"/></param>
    public BRExportFileItem(string code, string filterText, string fileMask, BRActionItemPerformEventHandler performAction)
      : base(code, filterText, fileMask)
    {
      PerformAction += performAction;
      _UseWithControl = true;
      _UseWithPreview = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Если установлено в true (по умолчанию), то формат будет присутствовать в команде меню управляющего элемента, к которому относится <see cref="BRMenuOutItem"/>,
    /// например, в табличном просмотре.
    /// </summary>
    public bool UseWithControl
    {
      get { return _UseWithControl; }
      set { _UseWithControl = value; }
    }
    private bool _UseWithControl;

    /// <summary>
    /// Если установлено в true (по умолчанию), то формат будет присутствовать в команде меню окна предварительного просмотра.
    /// </summary>
    public bool UseWithPreview
    {
      get { return _UseWithPreview; }
      set { _UseWithPreview = value; }
    }
    private bool _UseWithPreview;

    #endregion

    #region События

    void IBRActionItem.CallInitDialog(BRMenuOutItemInitDialogEventArgs args)
    {
      OnInitDialog(args);
    }

    /// <summary>
    /// Событие вызывается перед показом блока диалога, который выводится после того, как пользователь выбрал файл для экспорта.
    /// Если имеются нестандартные данные в <see cref="BRMenuOutItem.SettingsData"/>, то обработчик события может добавить вкладки в блок диалога для редактирования этих данных
    /// </summary>
    public event BRMenuOutItemInitDialogEventHandler InitDialog;

    /// <summary>
    /// Вызывает событие <see cref="InitDialog"/>
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnInitDialog(BRMenuOutItemInitDialogEventArgs args)
    {
      if (InitDialog != null)
        InitDialog(this, args);
    }

    void IBRActionItem.CallPerformAction(BRActionItemPerformEventArgs args)
    {
      OnPerformAction(args);
    }

    /// <summary>
    /// Обработчик события должен выполнить экспорт в файл.
    /// Обработчик является обязательным и задается в конструкторе.
    /// </summary>
    public event BRActionItemPerformEventHandler PerformAction;

    /// <summary>
    /// Вызывает обработчик события <see cref="PerformAction"/>
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnPerformAction(BRActionItemPerformEventArgs args)
    {
      if (PerformAction != null)
        PerformAction(this, args);
    }

    #endregion
  }

  /// <summary>
  /// Коды поддерживаемых форматов файлов, используемые <see cref="BRExportFileItem"/> (свойство <see cref="EFPExportFileItem.Code"/>).
  /// </summary>
  public static class BRExportFileItemCodes
  {
    #region Константы

    /// <summary>
    /// Формат PDF
    /// </summary>
    public const string PDF = "PDF";

    /// <summary>
    /// Формат (многостраничного) изображения TIFF
    /// </summary>
    public const string TIFF = "TIFF";

    /// <summary>
    /// Формат HTML
    /// </summary>
    public const string HTML = "HTML";

    /// <summary>
    /// Формат XML для Microsoft Excel 2003
    /// </summary>
    public const string ExcelXML = "ExcelXML";

    /// <summary>
    /// Формат XML для Microsoft Word 2003
    /// </summary>
    public const string WordXML = "WordXML";

    /// <summary>
    /// Формат zip-xml для OpenOffice/LibreOffice Calc
    /// </summary>
    public const string ODS = "ODS";

    /// <summary>
    /// Формат zip-xml для OpenOffice/LibreOffice Writer
    /// </summary>
    public const string ODT = "ODT";

    #endregion
  }

  /// <summary>
  /// Коды (основные) поддерживаемых команд "Отправить", используемые <see cref="BRSendToItem"/> (свойство <see cref="EFPSendToItem.MainCode"/>).
  /// </summary>
  public static class BRSendToItemCodes
  {
    #region Константы

    #region Отправка в конкретные приложения

    /// <summary>
    /// Microsoft Excel.
    /// Для версий Microsoft Excel XP и новее используется промежуточный файл в формате <see cref="BRExportFileItemCodes.ExcelXML"/>.
    /// Для версии Microsoft Excel 2000 используется более медленный способ: динамическое создание документа через OLE.
    /// </summary>
    public const string Excel = "Excel";

    /// <summary>
    /// Microsoft Word 2003 или новее.
    /// Используется промежуточный файл в формате <see cref="BRExportFileItemCodes.WordXML"/>.
    /// </summary>
    public const string Word = "Word";

    /// <summary>
    /// OpenOffice / LibreOffice Calc.
    /// Используется промежуточный файл в формате <see cref="BRExportFileItemCodes.ODS"/>.
    /// Если есть несколько установленных офисов, команды создаются для каждого из них.
    /// </summary>
    public const string Calc = "Calc";

    /// <summary>
    /// OpenOffice / LibreOffice Writer.
    /// Используется промежуточный файл в формате <see cref="BRExportFileItemCodes.ODT"/>.
    /// Если есть несколько установленных офисов, команды создаются для каждого из них.
    /// </summary>
    public const string Writer = "Writer";

    #endregion

    #region Отправка в приложения по файловым ассоциациям

    /// <summary>
    /// Файлы 
    /// </summary>
    public const string PDF = "PDF";

    /// <summary>
    /// Формат HTML
    /// </summary>
    public const string HTML = "HTML";

    #endregion

    #endregion
  }

  /// <summary>
  /// Расширение описания команды "Отправить" для <see cref="BRMenuOutItem"/> 
  /// </summary>
  public class BRSendToItem : EFPSendToItem, IBRActionItem
  {
    #region Конструктор

    /// <summary>
    /// Создает описание
    /// </summary>
    /// <param name="mainCode">Основной код</param>
    /// <param name="auxCode">Дополнительный код или пустая строка</param>
    /// <param name="performAction">Обработчик события <see cref="PerformAction"/></param>
    public BRSendToItem(string mainCode, string auxCode, BRActionItemPerformEventHandler performAction)
      : base(mainCode, auxCode)
    {
      PerformAction += performAction;
      _UseWithControl = true;
      _UseWithPreview = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Если установлено в true (по умолчанию), то команда будет присутствовать в команде меню управляющего элемента, к которому относится <see cref="BRMenuOutItem"/>,
    /// например, в табличном просмотре.
    /// </summary>
    public bool UseWithControl
    {
      get { return _UseWithControl; }
      set { _UseWithControl = value; }
    }
    private bool _UseWithControl;

    /// <summary>
    /// Если установлено в true (по умолчанию), то команда будет присутствовать в команде меню окна предварительного просмотра.
    /// </summary>
    public bool UseWithPreview
    {
      get { return _UseWithPreview; }
      set { _UseWithPreview = value; }
    }
    private bool _UseWithPreview;

    internal OpenOfficeInfo OOInfo;

    internal FileAssociationItem FA;

    #endregion

    #region События

    void IBRActionItem.CallInitDialog(BRMenuOutItemInitDialogEventArgs args)
    {
      OnInitDialog(args);
    }

    /// <summary>
    /// Событие вызывается перед показом блока диалога, который выводится после того, как пользователь выбрал файл для экспорта.
    /// Если имеются нестандартные данные в <see cref="BRMenuOutItem.SettingsData"/>, то обработчик события может добавить вкладки в блок диалога для редактирования этих данных
    /// </summary>
    public event BRMenuOutItemInitDialogEventHandler InitDialog;

    /// <summary>
    /// Вызывает событие <see cref="InitDialog"/>
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnInitDialog(BRMenuOutItemInitDialogEventArgs args)
    {
      if (InitDialog != null)
        InitDialog(this, args);
    }

    void IBRActionItem.CallPerformAction(BRActionItemPerformEventArgs args)
    {
      OnPerformAction(args);
    }

    /// <summary>
    /// Обработчик события должен выполнить отправку.
    /// Обработчик является обязательным.
    /// </summary>
    public event BRActionItemPerformEventHandler PerformAction;

    /// <summary>
    /// Вызывает обработчик события <see cref="PerformAction"/>
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnPerformAction(BRActionItemPerformEventArgs args)
    {
      if (PerformAction != null)
        PerformAction(this, args);
    }

    #endregion
  }

  /// <summary>
  /// Неабстрактная реализация объекта печати/экспорта с помощью <see cref="BRReport"/>.
  /// </summary>
  public class BRMenuOutItem : EFPMenuOutItem
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="code">Код</param>
    public BRMenuOutItem(string code)
      : base(code)
    {
      _SettingsData = new SettingsDataList();
      _SettingsData.Add(new BRPageSettingsDataItem());
      _SettingsData.Add(new BRBitmapSettingsDataItem());

      _ClosePreviewWhenPrinting = true;
    }

    #endregion

    #region Инициализация

    /// <summary>
    /// Это статическое событие вызывается однократно для каждого объекта <see cref="BRMenuOutItem"/>, непосредственно перед
    /// обработчиком событие <see cref="EFPMenuOutItem.Prepare"/>.
    /// Позволяет на уровне приложения добавить или изменить форматы экспорта отчета <see cref="BRReport"/> и варианты команды "Отправить".
    /// В списки <see cref="EFPMenuOutItem.ExportFileItems"/> и <see cref="EFPMenuOutItem.SendToItems"/> можно добавлять только объекты
    /// <see cref="BRExportFileItem"/> и <see cref="BRSendToItem"/> соответственно, но не других классов.
    /// Обработчик не предназначен для внесения изменений в отчет <see cref="BRReport"/>.
    /// </summary>
    public static event EFPMenuOutItemPrepareEventHandler PrepareDefaults;

    /// <summary>
    /// Заполняет списки <see cref="EFPMenuOutItem.ExportFileItems"/> и <see cref="EFPMenuOutItem.SendToItems"/> стандартными форматами экспорта файлов и командами "Отправить".
    /// Затем вызывается обработчик статического события <see cref="PrepareDefaults"/> для пользовательской инициализации на уровне приложения.
    /// Затем вызывается базовый обработчик инициализации <see cref="EFPMenuOutItem.Prepare"/>.
    /// После этого в списках оставляются только действия с UseWithControl=true. Действия, предназначенные для окна предварительного просмотра,
    /// сохраняются в отдельных списках.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnPrepare(EFPMenuOutItemPrepareEventArgs args)
    {
      // Стандартная инициализация
      InitDefaults();

      // Статическая пользовательская инициализация
      if (PrepareDefaults != null)
        PrepareDefaults(this, args);

      // Пользовательская инициализация для объекта
      base.OnPrepare(args);

      // Разделение списков
      PreviewExportFileItems = new List<EFPExportFileItem>();
      PreviewSendToItems = new List<EFPSendToItem>();
      SplitList(base.ExportFileItems, this.PreviewExportFileItems);
      SplitList(base.SendToItems, this.PreviewSendToItems);
    }

    /// <summary>
    /// Статический список объектов для создания файлов различных форматов командой "Экспорт в файл".
    /// По умолчанию содержит генераторы файлов с кодами из <see cref="BRExportFileItemCodes"/>, для которых присутствуют необходимые библиотеки.
    /// Метод <see cref="OnPrepare(EFPMenuOutItemPrepareEventArgs)"/> копирует объекты из этого списка в <see cref="EFPMenuOutItem.ExportFileItems"/>.
    /// Затем создаются команды "Отправить" на основании имеющихся генераторов файлов.
    /// Прикладной код может заменить генераторы стандартных форматов на собственные реализации. Тогда они будут использованы и для <see cref="BRSendToItem"/>.
    /// Также можно добавить пользовательские форматы файлов. Однако, для добавления пользовательских команд "Отправить" требуется обработка события <see cref="PrepareDefaults"/>.
    /// </summary>
    public static NamedList<BRExportFileItem> AppDefaultExportFileItems
    {
      get
      {
        if (_AppDefaultExportFileItems == null)
          _AppDefaultExportFileItems = CreateAppDefaultExportFileItems();
        return _AppDefaultExportFileItems;
      }
    }
    private static NamedList<BRExportFileItem> _AppDefaultExportFileItems;

    private static NamedList<BRExportFileItem> CreateAppDefaultExportFileItems()
    {
      NamedList<BRExportFileItem> lst = new NamedList<BRExportFileItem>();
      BRExportFileItem item1;
      if (PdfFileTools.PdfLibAvailable)
        lst.Add(new BRExportFileItem(BRExportFileItemCodes.PDF, "Файлы PDF", "*.pdf", ExportFilePdf));
      if (BRFileTiff.IsSupported)
      {
        item1 = new BRExportFileItem(BRExportFileItemCodes.TIFF, "Файлы TIFF", "*.tif", ExportFileTiff);
        item1.InitDialog += ExportFileTiff_InitDialog;
        lst.Add(item1);
      }
      lst.Add(new BRExportFileItem(BRExportFileItemCodes.HTML, "Файлы HTML", "*.html", ExportFileHtml));
      lst.Add(new BRExportFileItem(BRExportFileItemCodes.ExcelXML, "Microsoft Excel 2003 XML", "*.xml", ExportFileExcelXml));
      lst.Add(new BRExportFileItem(BRExportFileItemCodes.WordXML, "Microsoft Word 2003 XML", "*.xml", ExportFileWordXml));
      if (BRFileODFBase.IsSupported)
      {
        lst.Add(new BRExportFileItem(BRExportFileItemCodes.ODS, "OpenOffice/LibreOffice Calc (ODS)", "*.ods", ExportFileOds));
        lst.Add(new BRExportFileItem(BRExportFileItemCodes.ODT, "OpenOffice/LibreOffice Writer (ODT)", "*.odt", ExportFileOdt));
      }
      return lst;
    }

    /// <summary>
    /// Добавляет команды "Отправить" и форматы файлов для экспорта, ипользуемые по умолчанию
    /// </summary>
    private void InitDefaults()
    {
      #region ExportFileItems

      foreach (BRExportFileItem item1 in AppDefaultExportFileItems)
        ExportFileItems.Add(item1);

      #endregion

      #region SendToItems

      #region Офисные приложения

      BRSendToItem item2;
      if (EFPDataGridView.CanSendToMicrosoftExcel) // TODO: !!!!!!!!!!!!!!!!!!!!!!!!!
      {
        item2 = new BRSendToItem(BRSendToItemCodes.Excel, String.Empty, SendToExcel);
        item2.MenuText = "Microsoft Excel";
        item2.Image = EFPApp.MicrosoftExcelImage;
        item2.ToolTipText = "Отправить в " + MicrosoftOfficeTools.ExcelDisplayName;
        SendToItems.Add(item2);
      }

      if (EFPApp.MicrosoftWordVersion.Major >= MicrosoftOfficeTools.MicrosoftOffice_2003) // или _XP?
      {
        item2 = new BRSendToItem(BRSendToItemCodes.Word, String.Empty, SendToWord);
        item2.MenuText = "Microsoft Word";
        item2.Image = EFPApp.MicrosoftWordImage;
        item2.ToolTipText = "Отправить в " + MicrosoftOfficeTools.WordDisplayName;
        item2.UseWithControl = false;
        SendToItems.Add(item2);
      }

      if (BRFileODFBase.IsSupported)
      {
        OpenOfficeInfo[] offices = OpenOfficeTools.GetPartInstallations(OpenOfficePart.Calc);
        for (int i = 0; i < offices.Length; i++)
        {

          OpenOfficeInfo info = offices[i];
          OpenOfficePartInfo calc = info.Parts[OpenOfficePart.Calc];
          if (calc != null)
          {
            item2 = new BRSendToItem(BRSendToItemCodes.Calc, StdConvert.ToString(i), SendToCalc);
            item2.MenuText = calc.DisplayName;
            item2.OOInfo = info;
            FileAssociationItem fa = calc.FileAssociation;
            item2.Image = EFPApp.FileExtAssociations.GetIconImage(fa.IconPath, fa.IconIndex, true);
            item2.ToolTipText = "Отправить в " + calc.DisplayName;
            item2.SubMenuText = "Calc";
            SendToItems.Add(item2);
          }

          OpenOfficePartInfo writer = info.Parts[OpenOfficePart.Writer];
          if (writer != null)
          {
            item2 = new BRSendToItem(BRSendToItemCodes.Writer, StdConvert.ToString(i), SendToWriter);
            item2.MenuText = writer.DisplayName;
            item2.OOInfo = info;
            FileAssociationItem fa = writer.FileAssociation;
            item2.Image = EFPApp.FileExtAssociations.GetIconImage(fa.IconPath, fa.IconIndex, true);
            item2.ToolTipText = "Отправить в " + writer.DisplayName;
            item2.SubMenuText = "Writer";
            item2.UseWithControl = false;
            SendToItems.Add(item2);
          }
        }
      }

      #endregion

      #region Файловые ассоциации PDF и HTML

      // Отправку в PDF и HTML разрешаем только в диалоге предварительного просмотра, чтобы не было завала кнопок в табличном просмотре
      FileAssociations fas;
      if (ExportFileItems.Contains(BRExportFileItemCodes.PDF))
      {
        fas = EFPApp.FileExtAssociations[".pdf"];
        for (int i = 0; i < fas.Count; i++)
        {
          FileAssociationItem fa = fas[i];

          item2 = new BRSendToItem(BRSendToItemCodes.PDF, StdConvert.ToString(i + 1), SendToPdf);
          item2.MenuText = fa.DisplayName;
          item2.Image = EFPApp.FileExtAssociations.GetIconImage(fa.IconPath, fa.IconIndex, true);
          item2.SubMenuText = "PDF";
          item2.FA = fa;
          item2.UseWithControl = false;
          SendToItems.Add(item2);
        }
      }

      if (ExportFileItems.Contains(BRExportFileItemCodes.HTML))
      {
        fas = EFPApp.FileExtAssociations[".html"];
        for (int i = 0; i < fas.Count; i++)
        {
          FileAssociationItem fa = fas[i];

          item2 = new BRSendToItem(BRSendToItemCodes.HTML, StdConvert.ToString(i + 1), SendToHtml);
          item2.MenuText = fa.DisplayName;
          item2.Image = EFPApp.FileExtAssociations.GetIconImage(fa.IconPath, fa.IconIndex, true);
          item2.SubMenuText = "HTML";
          item2.FA = fa;
          item2.UseWithControl = false;
          SendToItems.Add(item2);
        }
      }

      #endregion

      #endregion
    }

    internal List<EFPExportFileItem> PreviewExportFileItems;

    internal List<EFPSendToItem> PreviewSendToItems;

    private static void SplitList(System.Collections.IList main, System.Collections.IList preview)
    {
      List<int> removedIndexes = null;
      for (int i = 0; i < main.Count; i++)
      {
        IBRActionItem item = (IBRActionItem)main[i]; // если пользователь добавил постороннее, то возникнет исключение
        if (item.UseWithPreview)
          preview.Add(item);
        if (!item.UseWithControl)
        {
          if (removedIndexes == null)
            removedIndexes = new List<int>();
          removedIndexes.Add(i);
        }
      }

      if (removedIndexes != null)
      {
        for (int i = removedIndexes.Count - 1; i >= 0; i--)
          main.RemoveAt(removedIndexes[i]);
      }
    }

    #endregion

    #region Данные

    /// <summary>
    /// Имя секции конфигурации.
    /// </summary>
    public string ConfigSectionName
    {
      get { return _ConfigSectionName ?? String.Empty; }
      set { _ConfigSectionName = value; }
    }
    private string _ConfigSectionName;

    /// <summary>
    /// Список данных параметров страницы.
    /// Должен быть заполнен до вывода формы на экран.
    /// При первом показе диалога параметров странице, печати, экспорте вызывается <see cref="SettingsDataListBase.ReadConfig(CfgPart, SettingsPart)"/>,
    /// при условии, что свойство <see cref="ConfigSectionName"/> установлено.
    /// При повторных действиях чтение не выполняется.
    /// После каждого показа диалога, включая повторные, вызывается <see cref="SettingsDataListBase.WriteConfig(CfgPart, SettingsPart)"/>.
    /// </summary>
    public SettingsDataList SettingsData { get { return _SettingsData; } }
    private readonly SettingsDataList _SettingsData;

    //public BRPageSettingsDataItem PageSettings { get { return SettingsData.GetRequired<BRPageSettingsDataItem>(); } }

    /// <summary>
    /// Параметры страницы
    /// </summary>
    public BRPageSetup PageSetup { get { return SettingsData.GetRequired<BRPageSettingsDataItem>().PageSetup; } }

    //public BRPageSetup GetPageSetup(string defCfgName)
    //{
    //  if (String.IsNullOrEmpty(defCfgName))
    //    return PageSetup;

    //  BRPageSettingsDataItem di = SettingsData.DefaultConfigs[defCfgName].GetItem<BRPageSettingsDataItem>();
    //  if (di == null)
    //  {
    //    di = new BRPageSettingsDataItem(PageSetup.Clone());
    //    SettingsData.DefaultConfigs[defCfgName].Add(di);
    //  }
    //  return di.PageSetup;
    //}

    /// <summary>
    /// Настройки для экспорта в формат TIFF
    /// </summary>
    public BRBitmapSettingsDataItem BitmapSettings { get { return SettingsData.GetRequired<BRBitmapSettingsDataItem>(); } }

    private bool _ReadConfigCalled;

    private string UserConfigCategory { get { return EFPConfigCategories.PageSetup; } }

    private string MachineConfigCategory { get { return EFPConfigCategories.PageSetupFiles; } }

    private void CallReadConfig()
    {
      if (!_ReadConfigCalled)
      {
        SettingsData.GetDefaultConfigDict(); // запоминаем значения по умолчанию. Сам словарь секций не нужен

        string name = GetConfigSectionName();
        if (name.Length > 0)
        {
          SettingsPart usedParts = SettingsData.UsedParts;

          CfgPart cfg;
          if ((usedParts & SettingsPart.User) != 0)
          {
            using (EFPApp.ConfigManager.GetConfig(new EFPConfigSectionInfo(name, UserConfigCategory), EFPConfigMode.Read, out cfg))
            {
              SettingsData.ReadConfig(cfg, SettingsPart.User);
            }
          }
          if ((usedParts & SettingsPart.Machine) != 0)
          {
            using (EFPApp.ConfigManager.GetConfig(new EFPConfigSectionInfo(name, MachineConfigCategory), EFPConfigMode.Read, out cfg))
            {
              SettingsData.ReadConfig(cfg, SettingsPart.Machine);
            }
          }
          //if ((usedParts & SettingsPart.NoHistory) != 0)
          //{
          //  using (ConfigManager.GetConfig(new EFPConfigSectionInfo(ConfigSectionName, NoHistoryCategory), EFPConfigMode.Read, out cfg))
          //  {
          //    Data.ReadConfig(cfg, SettingsPart.NoHistory);
          //  }
          //}
        }
        _ReadConfigCalled = true;
      }
    }

    private void CallWriteConfig()
    {
      string name = GetConfigSectionName();
      if (name.Length > 0)
      {
        SettingsPart usedParts = SettingsData.UsedParts;

        CfgPart cfg;
        if ((usedParts & SettingsPart.User) != 0)
        {
          using (EFPApp.ConfigManager.GetConfig(new EFPConfigSectionInfo(name, UserConfigCategory), EFPConfigMode.Write, out cfg))
          {
            SettingsData.WriteConfig(cfg, SettingsPart.User);
          }
        }
        if ((usedParts & SettingsPart.Machine) != 0)
        {
          using (EFPApp.ConfigManager.GetConfig(new EFPConfigSectionInfo(name, MachineConfigCategory), EFPConfigMode.Write, out cfg))
          {
            SettingsData.WriteConfig(cfg, SettingsPart.Machine);
          }
        }
        //if ((usedParts & SettingsPart.NoHistory) != 0)
        //{
        //  using (ConfigManager.GetConfig(new EFPConfigSectionInfo(ConfigSectionName, NoHistoryCategory), EFPConfigMode.Read, out cfg))
        //  {
        //    Data.ReadConfig(cfg, SettingsPart.NoHistory);
        //  }
        //}
      }
    }

    private string GetConfigSectionName()
    {
      if (String.IsNullOrEmpty(ConfigSectionName))
        return "Default";
      else
        return ConfigSectionName;
    }

    #endregion

    #region CreateReport Построение отчета

    /// <summary>
    /// Событие вызывается для создания отчета <see cref="BRReport"/>
    /// </summary>
    public event BRMenuOutItemCreateReportEventHandler CreateReport;

    /// <summary>
    /// Вызывает обработчик события <see cref="CreateReport"/>
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnCreateReport(BRMenuOutItemCreateReportEventArgs args)
    {
      if (CreateReport != null)
        CreateReport(this, args);
    }

    internal BRReport PerformCreateReport(BRActionInfo actionInfo)
    {
      BRMenuOutItemCreateReportEventArgs args;
      EFPApp.BeginWait("Создание отчета \"" + DisplayName + "\"");
      try
      {
        CallReadConfig();
        args = new BRMenuOutItemCreateReportEventArgs(actionInfo);
        OnCreateReport(args);
      }
      finally
      {
        EFPApp.EndWait();
      }
      return args.Report;
    }

    #endregion

    #region InitDialog

    /// <summary>
    /// Событие вызывается для инициализации диалога параметров.
    /// Диалог параметров используется при выполнении команды "Параметры страницы", после выбора файла в режиме экспорта, и при выполнении команды "Отправить".
    /// Обработчик события может добавить собственные вкладки в диалог.
    /// Если в диалоге не будет ни одной вкладки, диалог не будет показан.
    /// </summary>
    public event BRMenuOutItemInitDialogEventHandler InitDialog;

    /// <summary>
    /// Вызывает обработчик события <see cref="InitDialog"/>
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnInitDialog(BRMenuOutItemInitDialogEventArgs args)
    {
      if (InitDialog != null)
        InitDialog(this, args);
    }

    internal bool ShowDialog(BRDialogKind dialogKind, BRActionInfo actionInfo, IBRActionItem item, bool forceWriteConfig)
    {
      #region Создание SettingsDialog 

      CallReadConfig();
      SettingsDialog dialog = new SettingsDialog();
      dialog.ConfigSectionName = GetConfigSectionName();
      dialog.Data = this.SettingsData;

      #endregion

      #region Заголовок

      switch (dialogKind)
      {
        case BRDialogKind.PageSetup:
          dialog.Title = "Параметры страницы";
          dialog.ImageKey = "PageSetup";
          break;
        case BRDialogKind.ControlExportFile:
        case BRDialogKind.PreviewExportFile:
          dialog.Title = "Экспорт в " + actionInfo.FilePath.FileName;
          dialog.ImageKey = "Save";
          break;
        case BRDialogKind.ControlSendTo:
        case BRDialogKind.PreviewSendTo:
          dialog.Title = "Отправить в " + actionInfo.SendToItem.MenuText;
          dialog.Image = actionInfo.SendToItem.Image;
          break;
        default:
          throw new BugException("Unknown dialogKind");
      }

      #endregion

      #region Вкладки диалога

      if (dialogKind == BRDialogKind.PageSetup && dialog.Data.GetItem<BRPageSettingsDataItem>() != null)
      {
        new BRPageSetupPaper(dialog);
        new BRPageSetupMargins(dialog);
      }

      BRMenuOutItemInitDialogEventArgs args = new BRMenuOutItemInitDialogEventArgs(dialog, dialogKind, actionInfo);
      OnInitDialog(args);
      if (item != null)
      {

        //bool useEventHandler;
        //if (commandSource == BROutCommandSource.OutItem)
        //  useEventHandler = true;
        //else
        //  useEventHandler = (mode == BRDialogMode.PageSetup);
        //if (useEventHandler)
        item.CallInitDialog(args);
      }

      #endregion

      #region Показ диалога

      // Если нет ни одной страницы, диалог не показываем, но считаем, что пользователь нажал ОК
      if (dialog.Pages.Count == 0)
      {
        if (dialogKind == BRDialogKind.PageSetup)
          EFPApp.MessageBox("Нет параметров страницы", dialog.Title);

        if (forceWriteConfig)
          CallWriteConfig();
        return true;
      }

      if (dialog.ShowDialog() == DialogResult.OK)
      {
        CallWriteConfig();
        return true;
      }
      else
        return false;

      #endregion
    }

    #endregion

    #region Печать

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool CanPrint { get { return true; } }

    /// <summary>
    /// Выполняет печать
    /// </summary>
    /// <param name="defaultPrinter">true-на принтере по умолчанию, false-показывается диалог выбора принтера</param>
    /// <returns>true, если печать начата</returns>
    public override bool Print(bool defaultPrinter)
    {
      BRActionInfo actionInfo = new BRActionInfo();
      actionInfo.Action = BRAction.Print;
      BRReport report = PerformCreateReport(actionInfo);
      return DoPrint(defaultPrinter, report);
    }
    internal bool DoPrint(bool defaultPrinter, BRReport report)
    {
      BRPaginatorPageInfo[] pages;
      PrintDocument pd = FreeLibSet.Drawing.Reporting.BRReportPainter.CreatePrintDocument(report, out pages);
      //PrintController prcontr = pd.PrintController;

      if (!defaultPrinter)
      {
        PrintDialog dlg = new PrintDialog();
        dlg.Document = pd;
        //dlg.AllowCurrentPage = true;
        //dlg.AllowSelection = true;
        dlg.AllowSomePages = true;
        dlg.PrinterSettings.MinimumPage = 1;
        dlg.PrinterSettings.MaximumPage = pages.Length;
        dlg.PrinterSettings.FromPage = 1;
        dlg.PrinterSettings.ToPage = pages.Length;
        dlg.UseEXDialog = true; // Иначе не работает в Windows-7 64 bit

        if (dlg.ShowDialog() != DialogResult.OK)
          return false;
      }

      // TODO: Принтер по умолчанию из настроек
      try
      {
        if (EFPApp.BackgroundPrinting.Enabled /*&& AllowBackground ???*/ )
          EFPApp.BackgroundPrinting.Add(pd);
        else
        {
          PrintController oldPD = pd.PrintController;
          EFPApp.BeginWait("Идет печать документа", "Print");
          try
          {
            //MessageBox.Show("PrintController.IsPreview:" + pd.PrintController.IsPreview.ToString(), pd.DocumentName);
            ////if (pd.PrintController.IsPreview)
            //pd.PrintController = prcontr; // 13.11.2023
            pd.Print();
            //MessageBox.Show("Печать выполнена");
          }
          finally
          {
            EFPApp.EndWait();
            pd.PrintController = oldPD;
          }
        }
        return true;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка печати документа");
        return false;
      }
    }

    #endregion

    #region Параметры страницы

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool CanPageSetup { get { return true; } }

    /// <summary>
    /// Показывает диалог "Параметры страницы"
    /// </summary>
    /// <returns>true, если пользователь нажал "ОК"</returns>
    public override bool ShowPageSetup()
    {
      BRActionInfo actionInfo = new BRActionInfo();
      actionInfo.Action = BRAction.Print;
      return ShowDialog(BRDialogKind.PageSetup, actionInfo, null, false);
    }

    #endregion

    #region Предварительный просмотр

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool CanPrintPreview { get { return true; } }

    /// <summary>
    /// Если true (по умолчанию), то окно предварительного просмотра закрывается при выполнении команды "Печать".
    /// </summary>
    public bool ClosePreviewWhenPrinting { get { return _ClosePreviewWhenPrinting; } set { _ClosePreviewWhenPrinting = value; } }
    private bool _ClosePreviewWhenPrinting;

    /// <summary>
    /// Показывает окно предварительного просмотра.
    /// В окне просмотра используется собственный <see cref="EFPMenuOutHandler"/> с единственным <see cref="EFPMenuOutItem"/>.
    /// Пользователь может менять параметры страницы, выполнять сохранение в файл и команду "Отправить".
    /// </summary>
    public override void ShowPrintPreview()
    {
      BRActionInfo actionInfo = new BRActionInfo();
      actionInfo.Action = BRAction.Print;
      BRReport report = PerformCreateReport(actionInfo);
      Form previewForm = new Form();
      previewForm.Text = DisplayName;
      previewForm.Icon = EFPApp.MainImages.Icons["Preview"];
      previewForm.StartPosition = FormStartPosition.CenterScreen;
      EFPApp.SetFormSize(previewForm, 75, 75);
      previewForm.WindowState = FormWindowState.Maximized;

      EFPFormProvider efpPreviewForm = new EFPFormProvider(previewForm);
      efpPreviewForm.ConfigSectionName = "PrintPreview";

      EFPControlWithToolBar<ExtPrintPreviewControl> cwt = new EFPControlWithToolBar<ExtPrintPreviewControl>(efpPreviewForm, previewForm);
      EFPExtPrintPreviewControl efpPreview = new EFPExtPrintPreviewControl(cwt);
      efpPreview.DisplayName = "PreviewControl";
      efpPreview.ConfigSectionName = efpPreviewForm.ConfigSectionName;
      efpPreview.Control.Document = FreeLibSet.Drawing.Reporting.BRReportPainter.CreatePrintDocument(report);
      this.PerformPrepareAlone();

      efpPreview.CommandItems.OutHandler.Items.Add(new BRPreviewOutItem(this, efpPreviewForm, efpPreview, report));

      EFPApp.ShowDialog(previewForm, true);
    }

    #endregion

    #region Сохранение в файл

    #region Основной метод

    /// <summary>
    /// Если требуется, показывает блок диалога параметров, а затем выполняет экспорт в файл
    /// </summary>
    /// <param name="filePath">Путь к файлу, выбранный пользователем</param>
    /// <param name="item">Описатель формата экспорта <see cref="BRExportFileItem"/></param>
    public override void ExportFile(AbsPath filePath, EFPExportFileItem item)
    {
      BRExportFileItem item2 = (BRExportFileItem)item;

      BRActionInfo actionInfo = new BRActionInfo();
      actionInfo.Action = BRAction.ExportFile;
      actionInfo.ExportFileItem = item2;
      actionInfo.FilePath = filePath;

      BRReport report = PerformCreateReport(actionInfo);

      if (!ShowDialog(BRDialogKind.ControlExportFile, actionInfo, item2, true))
        return;

      EFPApp.BeginWait("Сохранение файла " + filePath.FileName, "Save");
      try
      {
        FileTools.ForceDirs(filePath.ParentDir);

        BRActionItemPerformEventArgs args = new BRActionItemPerformEventArgs(this, report, actionInfo, null);
        ((IBRActionItem)item2).CallPerformAction(args);
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    #endregion

    #region Реализации для форматов файлов

    private static void ExportFilePdf(object sender, BRActionItemPerformEventArgs args)
    {
      args.CreateFile(new BRFilePdf());
    }

    private static void ExportFileTiff_InitDialog(object sender, BRMenuOutItemInitDialogEventArgs args)
    {
      new BRPageSetupBitmap(args.Dialog);
    }

    private static void ExportFileTiff(object sender, BRActionItemPerformEventArgs args)
    {
      BRFileTiff tiffCreator = new BRFileTiff();
      tiffCreator.BitmapSettings = args.OutItem.SettingsData.GetRequired<BRBitmapSettingsDataItem>();
      args.CreateFile(tiffCreator);
    }

    private static void ExportFileHtml(object sender, BRActionItemPerformEventArgs args)
    {
      args.CreateFile(new BRFileHtml());
    }

    private static void ExportFileExcelXml(object sender, BRActionItemPerformEventArgs args)
    {
      args.CreateFile(new BRFileExcel2003Xml());
    }

    private static void ExportFileWordXml(object sender, BRActionItemPerformEventArgs args)
    {
      args.CreateFile(new BRFileWord2003Xml(BRMeasurer.Default));
    }

    private static void ExportFileOds(object sender, BRActionItemPerformEventArgs args)
    {
      args.CreateFile(new BRFileODS());
    }

    private static void ExportFileOdt(object sender, BRActionItemPerformEventArgs args)
    {
      args.CreateFile(new BRFileODT(BRMeasurer.Default));
    }

    #endregion

    #endregion

    #region Отправить

    #region Свойства

    /// <summary>
    /// Если true, то при передаче в Word/Excel будет использоваться OLE для создания документов. Это медленный способ.
    /// Если false (по умолчанию), то создается временный файл, а OLE используется только для открытия документа.
    /// Свойство используется в отладочных целях.
    /// Если установлен старый MS Office 2000, который не может читать файлы в формате XML, то OLE используется независимо от этой настройки.
    /// </summary>
    public static bool OLEPreferred
    {
      get { return _OLEPreferred; }
      set { _OLEPreferred = value; }
    }

    private static bool _OLEPreferred = false;

    #endregion

    #region Основной метод

    /// <summary>
    /// Выполнение команды отправить
    /// </summary>
    /// <param name="item">Ссылка на описатель <see cref="BRSendToItem"/>. Использование других типов описателей не допускается</param>
    public override void SendTo(EFPSendToItem item)
    {
      BRSendToItem item2 = (BRSendToItem)item;

      BRActionInfo actionInfo = new BRActionInfo();
      actionInfo.Action = BRAction.SendTo;
      actionInfo.SendToItem = item2;

      if (!ShowDialog(BRDialogKind.ControlSendTo, actionInfo, item2, false))
        return;

      BRReport report = PerformCreateReport(actionInfo);
      EFPApp.BeginWait("Отправка в " + item2.MenuText);
      try
      {
        BRActionItemPerformEventArgs args = new BRActionItemPerformEventArgs(this, report, actionInfo, null);
        ((IBRActionItem)item2).CallPerformAction(args);
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    #endregion

    #region Реализации команд "Отправить"

    private static void SendToExcel(object sender, BRActionItemPerformEventArgs args)
    {
      if (!OLEPreferred)
      {
        if (EFPApp.MicrosoftExcelVersion.Major >= MicrosoftOfficeTools.MicrosoftOffice_XP)
        {
          using (Splash spl = new Splash(new string[] {
          "Создание файла",
          "Запуск Microsoft Excel"}))
          {
            AbsPath filePath = EFPApp.SharedTempDir.GetTempFileName("xml");
            args.CreateFile(BRExportFileItemCodes.ExcelXML, filePath);
            spl.Complete();
            MicrosoftOfficeTools.OpenWithExcel(filePath, true);
            spl.Complete();
          }
          return;
        }
      }

      using (Splash spl = new Splash("Создание книги в Excel"))
      {
        BRFileExcelOLE creator = new BRFileExcelOLE();
        creator.Splash = spl;
        creator.Send(args.Report);
      }
    }

    private static void SendToWord(object sender, BRActionItemPerformEventArgs args)
    {
      using (Splash spl = new Splash(new string[] {
          "Создание файла",
          "Запуск Microsoft Word"}))
      {
        AbsPath filePath = EFPApp.SharedTempDir.GetTempFileName("xml");
        args.CreateFile(BRExportFileItemCodes.WordXML, filePath);
        spl.Complete();
        MicrosoftOfficeTools.OpenWithWord(filePath, true);
        spl.Complete();
      }
    }

    private static void SendToCalc(object sender, BRActionItemPerformEventArgs args)
    {
      using (Splash spl = new Splash("Создание книги в " + args.ActionInfo.SendToItem.OOInfo.Parts[OpenOfficePart.Calc].DisplayName))
      {
        AbsPath filePath = EFPApp.SharedTempDir.GetTempFileName("ods");
        args.CreateFile(BRExportFileItemCodes.ODS, filePath);
        args.ActionInfo.SendToItem.OOInfo.Parts[OpenOfficePart.Calc].OpenFile(filePath, true);
      }
    }

    private static void SendToWriter(object sender, BRActionItemPerformEventArgs args)
    {
      using (Splash spl = new Splash("Создание документа в " + args.ActionInfo.SendToItem.OOInfo.Parts[OpenOfficePart.Writer].DisplayName))
      {
        AbsPath filePath = EFPApp.SharedTempDir.GetTempFileName("odt");
        args.CreateFile(BRExportFileItemCodes.ODT, filePath);
        args.ActionInfo.SendToItem.OOInfo.Parts[OpenOfficePart.Writer].OpenFile(filePath, true);
      }
    }

    private static void SendToPdf(object sender, BRActionItemPerformEventArgs args)
    {
      using (Splash spl = new Splash("Создание PDF-файла"))
      {
        AbsPath filePath = EFPApp.SharedTempDir.GetTempFileName("pdf");
        args.CreateFile(BRExportFileItemCodes.PDF, filePath);
        args.ActionInfo.SendToItem.FA.Execute(filePath);
      }
    }

    private static void SendToHtml(object sender, BRActionItemPerformEventArgs args)
    {
      using (Splash spl = new Splash("Создание HTML-файла"))
      {
        AbsPath filePath = EFPApp.SharedTempDir.GetTempFileName("html");
        args.CreateFile(BRExportFileItemCodes.HTML, filePath);
        args.ActionInfo.SendToItem.FA.Execute(filePath);
      }
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// Команды печати в окне предварительного просмотра
  /// </summary>
  internal class BRPreviewOutItem : EFPMenuOutItem
  {
    #region Конструктор

    public BRPreviewOutItem(BRMenuOutItem owner, EFPFormProvider efpPreviewForm, EFPExtPrintPreviewControl efpPreview, BRReport report)
      : base("Print")
    {
      _Owner = owner;
      _efpPreviewForm = efpPreviewForm;
      _efpPreview = efpPreview;
      _Report = report;

      ExportFileItems.AddRange(owner.PreviewExportFileItems);
      SendToItems.AddRange(owner.PreviewSendToItems);
    }

    private BRMenuOutItem _Owner;
    EFPFormProvider _efpPreviewForm;
    EFPExtPrintPreviewControl _efpPreview;
    BRReport _Report;

    #endregion

    #region Печать

    public override bool CanPrint { get { return true; } }

    public override bool Print(bool defaultPrinter)
    {
      bool res = _Owner.DoPrint(defaultPrinter, _Report);
      if (res && _Owner.ClosePreviewWhenPrinting)
        _efpPreviewForm.CloseForm(DialogResult.Cancel);
      return res;
    }

    #endregion

    #region Параметры страницы

    public override bool CanPageSetup { get { return true; } }

    public override bool ShowPageSetup()
    {
      if (!_Owner.ShowPageSetup())
        return false;

      BRActionInfo actionInfo = new BRActionInfo();
      actionInfo.Action = BRAction.Print;
      _Report = _Owner.PerformCreateReport(actionInfo);
      _efpPreview.Control.Document = FreeLibSet.Drawing.Reporting.BRReportPainter.CreatePrintDocument(_Report);
      _efpPreview.Control.InvalidatePreview();
      _efpPreview.Control.StartPage = 0;
      return true;
    }

    #endregion

    #region Сохранение в файл

    public override void ExportFile(AbsPath filePath, EFPExportFileItem item)
    {
      BRExportFileItem item2 = (BRExportFileItem)item;

      BRActionInfo actionInfo = new BRActionInfo();
      actionInfo.Action = BRAction.ExportFile;
      actionInfo.ExportFileItem = item2;
      actionInfo.FilePath = filePath;

      if (!_Owner.ShowDialog(BRDialogKind.PreviewExportFile, actionInfo, item2, true))
        return;

      EFPApp.BeginWait("Сохранение файла " + filePath.FileName, "Save");
      try
      {
        FileTools.ForceDirs(filePath.ParentDir);

        BRActionItemPerformEventArgs args = new BRActionItemPerformEventArgs(_Owner, _Report, actionInfo, null);
        ((IBRActionItem)item2).CallPerformAction(args);
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    #endregion

    #region Отправить

    public override void SendTo(EFPSendToItem item)
    {
      BRSendToItem item2 = (BRSendToItem)item;

      BRActionInfo actionInfo = new BRActionInfo();
      actionInfo.Action = BRAction.SendTo;
      actionInfo.SendToItem = item2;

      if (!_Owner.ShowDialog(BRDialogKind.PreviewSendTo, actionInfo, item2, false))
        return;

      EFPApp.BeginWait("Отправка в " + item2.MenuText);
      try
      {
        BRActionItemPerformEventArgs args = new BRActionItemPerformEventArgs(_Owner, _Report, actionInfo, null);
        ((IBRActionItem)item2).CallPerformAction(args);
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    #endregion
  }


  /// <summary>
  /// Диалог просмотра отчета <see cref="BRReport"/>
  /// </summary>
  public sealed class BRPrintPreviewDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    public BRPrintPreviewDialog()
    {
      _CloseWhenPrinting = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основное свойство - просматриваемый отчет
    /// </summary>
    public BRReport Report { get { return _Report; } set { _Report = value; } }
    private BRReport _Report;

    /// <summary>
    /// Секция конфигурации, используемая для сохранения ???
    /// </summary>
    public string ConfigSectionName
    {
      get { return _ConfigSectionName; }
      set { _ConfigSectionName = value; }
    }
    private string _ConfigSectionName;

    /// <summary>
    /// Если true (по умолчанию), то окно предварительного просмотра закрывается при выполнении команды "Печать".
    /// </summary>
    public bool CloseWhenPrinting { get { return _CloseWhenPrinting; } set { _CloseWhenPrinting = value; } }
    private bool _CloseWhenPrinting;

    #endregion

    #region Показ диалога

    /// <summary>
    /// Показывает диалог с просмотром
    /// </summary>
    public void ShowDialog()
    {
      if (Report == null)
        EFPApp.ErrorMessageBox("Отчет не присоединен");

      BRMenuOutItem outItem = new BRMenuOutItem("1");
      if (!String.IsNullOrEmpty(Report.DocumentProperties.Title))
        outItem.DisplayName = Report.DocumentProperties.Title;
      else
        outItem.DisplayName = "Отчет";
      outItem.CreateReport += OutItem_CreateReport;
      outItem.ConfigSectionName = ConfigSectionName;
      outItem.ClosePreviewWhenPrinting = CloseWhenPrinting;
      outItem.ShowPrintPreview();
    }

    private void OutItem_CreateReport(object sender, BRMenuOutItemCreateReportEventArgs args)
    {
      args.Report = this.Report;
    }

    #endregion
  }
}
