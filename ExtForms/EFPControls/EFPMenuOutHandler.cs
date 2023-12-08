using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using FreeLibSet.IO;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Описание одного формата файла для команды "Экспорт в файл" для <see cref="EFPMenuOutItem "/>
  /// </summary>
  public class EFPExportFileItem : IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает описание
    /// </summary>
    /// <param name="code">Код формата</param>
    /// <param name="filterText">Описание фильтра для диалога сохранения файла</param>
    /// <param name="fileMask">Маска для выбора файлов</param>
    public EFPExportFileItem(string code, string filterText, string fileMask)
    {
      if (String.IsNullOrEmpty(code))
        throw new ArgumentNullException("code");
      _Code = code;

      if (String.IsNullOrEmpty(filterText))
        throw new ArgumentNullException("filterText");
      if (filterText.IndexOf('|') >= 0)
        throw new ArgumentException("Недопустимые символы", "filterText");
      _FilterText = filterText;

      if (String.IsNullOrEmpty(fileMask))
        throw new ArgumentNullException("fileMask");
      if (fileMask.IndexOf('|') >= 0)
        throw new ArgumentException("Недопустимые символы", "fileMask");
      _FileMask = fileMask;
    }

    #endregion

    #region Свойства 

    /// <summary>
    /// Условный код формата, например, "PDF".
    /// Для одного <see cref="EFPMenuOutItem"/> все коды должны быть разными.
    /// Код чувствителен к регистру.
    /// </summary>
    public string Code { get { return _Code; } }
    private readonly string _Code;

    /// <summary>
    /// Текст фильтра для блока диалога, например, "Файлы PDF"
    /// </summary>
    public string FilterText { get { return _FilterText; } }
    private readonly string _FilterText;

    /// <summary>
    /// Маска для диалога выбора файла, например, "*.pdf"
    /// </summary>
    public string FileMask { get { return _FileMask; } }
    private readonly string _FileMask;

    /// <summary>
    /// Текстовое представление
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Code;
    }

    /// <summary>
    /// Используется для передачи дополнительных данных в обработчик
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    #endregion
  }

  /// <summary>
  /// Описание одной команды "Отправить" для <see cref="EFPMenuOutItem "/>
  /// </summary>
  public class EFPSendToItem : IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает описание
    /// </summary>
    /// <param name="mainCode">Основной код</param>
    /// <param name="auxCode">Дополнительный код или пустая строка</param>
    public EFPSendToItem(string mainCode, string auxCode)
    {
      if (String.IsNullOrEmpty(mainCode))
        throw new ArgumentNullException("mainCode");
      _MainCode = mainCode;
      _AuxCode = auxCode ?? String.Empty;
    }

    /// <summary>
    /// Создает описание
    /// </summary>
    /// <param name="mainCode">Основной код</param>
    public EFPSendToItem(string mainCode)
      : this(mainCode, String.Empty)
    {
    }

    #endregion

    #region Код команды

    /// <summary>
    /// Основной код команды, например, "HTML".
    /// Задается в конструкторе, не может быть пустой строкой
    /// </summary>
    public string MainCode { get { return _MainCode; } }
    private readonly string _MainCode;

    /// <summary>
    /// Дополнительный код команды, например, "1".
    /// Задается в конструкторе. Может быть пустой строкой
    /// </summary>
    public string AuxCode { get { return _AuxCode; } }
    private readonly string _AuxCode;

    /// <summary>
    /// Код команды меню
    /// </summary>
    public string Code
    {
      get
      {
        if (_AuxCode.Length == 0)
          return _MainCode;
        else
          return _MainCode + "_" + _AuxCode;
      }
    }

    /// <summary>
    /// Текстовое представление
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Code;
    }

    /// <summary>
    /// Используется для передачи дополнительных данных в обработчик
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    #endregion

    #region Свойства команды меню

    /// <summary>
    /// Текст команды меню, например, "Internet Explorer". Обычно берется из файловой ассоциации
    /// </summary>
    public string MenuText
    {
      get { return _MenuText ?? Code; }
      set { _MenuText = value; }
    }
    private string _MenuText;

    /// <summary>
    /// Значок для кнопки и команды меню
    /// </summary>
    public Image Image
    {
      get { return _Image ?? EFPApp.MainImages.Images["UnknownState"]; }
      set { _Image = value; }
    }
    private Image _Image;

    /// <summary>
    /// Текст всплывающей подсказки
    /// </summary>
    public string ToolTipText
    {
      get
      {
        if (_ToolTipText == null)
        {
          StringBuilder sb = new StringBuilder();
          sb.Append("Отправить ");
          if (!String.IsNullOrEmpty(SubMenuText))
          {
            sb.Append(SubMenuText);
            sb.Append(" ");
          }
          sb.Append("в ");
          sb.Append(MenuText);
          return sb.ToString();
        }
        else
          return _ToolTipText;
      }
      set { _ToolTipText = value; }
    }
    private string _ToolTipText;

    /// <summary>
    /// Текст подменю, если есть несколько команд с разными доп. кодами
    /// </summary>
    public string SubMenuText
    {
      get { return _SubMenuText ?? MainCode; }
      set { _SubMenuText = value; }
    }
    private string _SubMenuText;

    #endregion
  }

  #region EFPMenuOutItemPrepareEventHandler

  /// <summary>
  /// Аргументы события <see cref="EFPMenuOutItem.Prepare"/>
  /// </summary>
  public class EFPMenuOutItemPrepareEventArgs : EventArgs
  {
    #region Конструктор

    internal EFPMenuOutItemPrepareEventArgs(EFPMenuOutItem outItem, EFPContextCommandItems commandItems)
    {
#if DEBUG
      if (outItem == null)
        throw new ArgumentNullException("outItem");
//      if (commandItems == null)
//        throw new ArgumentNullException("commandItems");
#endif
      _OutItem = outItem;
      _CommandItems = commandItems;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект, для которого вызвано событие
    /// </summary>
    public EFPMenuOutItem OutItem { get { return _OutItem; } }
    private readonly EFPMenuOutItem _OutItem;

    /// <summary>
    /// Список команд локального меню, к которому присоединен объект <see cref="EFPMenuOutHandler"/>
    /// </summary>
    public EFPContextCommandItems CommandItems { get { return _CommandItems; } }
    private readonly EFPContextCommandItems _CommandItems;

    /// <summary>
    /// Провайдер управляющего элемента, к которому относится локальное меню.
    /// Если команды относятся к форме в-целом, возвращается null.
    /// </summary>
    public EFPControlBase ControlProvider
    {
      get
      {
        EFPControlCommandItems ccis = _CommandItems as EFPControlCommandItems;
        if (ccis != null)
          return ccis.ControlProvider;
        return null;
      }
    }

    /// <summary>
    /// Провайдер формы, к которой относится локальное меню.
    /// Если команды относятся к управляющему элементу, который еще не присоединен к форме, возвращается null.
    /// </summary>
    public EFPFormProvider FormProvider
    {
      get
      {
        EFPFormCommandItems fcis = _CommandItems as EFPFormCommandItems;
        if (fcis != null)
          return fcis.FormProvider;

        EFPControlCommandItems ccis = _CommandItems as EFPControlCommandItems;
        if (ccis != null)
          return ccis.ControlProvider.BaseProvider.FormProvider;
        return null;
      }
    }

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="EFPMenuOutItem.Prepare"/>
  /// </summary>
  /// <param name="sender">Ссылка на <see cref="EFPMenuOutItem"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPMenuOutItemPrepareEventHandler(object sender, EFPMenuOutItemPrepareEventArgs args);

  #endregion

  /// <summary>
  /// Базовый класс для выполнения печати/экспорта.
  /// Если управляющий элемент содержит <see cref="EFPMenuOutHandler"/>, то к обработчику может быть добавлено несколько объектов.
  /// Для использования отчетов <see cref="FreeLibSet.Reporting.BRReport"/> используйте реализацию <see cref="FreeLibSet.Forms.Reporting.BRMenuOutItem"/>.
  /// Для печати табличного просмотра используется <see cref="FreeLibSet.Forms.Reporting.BRDataGridViewMenuOutItem"/>, а для иерахического - <see cref="FreeLibSet.Forms.Reporting.BRDataTreeViewMenuOutItem"/>
  /// </summary>
  public abstract class EFPMenuOutItem : IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой объект
    /// </summary>
    /// <param name="code">Код</param>
    public EFPMenuOutItem(string code)
    {
      if (String.IsNullOrEmpty(code))
        throw new ArgumentNullException("code");
      _Code = code;
      _ExportFileItems = new NamedList<EFPExportFileItem>();
      _SendToItems = new NamedList<EFPSendToItem>();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Код объекта, например, "Control" для обычной печати просмотра
    /// </summary>
    public string Code { get { return _Code; } }
    private readonly string _Code;

    /// <summary>
    /// Отображаемое имя. Используется, когда <see cref="EFPMenuOutHandler"/> содержит несколько объектов и пользователю предлагается выбрать один из <see cref="EFPMenuOutItem"/>.
    /// </summary>
    public virtual string DisplayName
    {
      get { return _DisplayName ?? Code; }
      set { _DisplayName = value; }
    }
    private string _DisplayName;

    /// <summary>
    /// Возвращает true, если доступна команда "Печать"
    /// </summary>
    public virtual bool CanPrint { get { return false; } }

    /// <summary>
    /// Возвращает true, если доступна команда "Параметры страницы"
    /// </summary>
    public virtual bool CanPageSetup { get { return false; } }

    /// <summary>
    /// Возвращает true, если доступна команда "Предварительный просмотр"
    /// </summary>
    public virtual bool CanPrintPreview { get { return false; } }

    /// <summary>
    /// Список форматов файлов, для которых возможен экспорт.
    /// </summary>
    public NamedList<EFPExportFileItem> ExportFileItems { get { return _ExportFileItems; } }
    private readonly NamedList<EFPExportFileItem> _ExportFileItems;

    /// <summary>
    /// Список команд "Отправить".
    /// Несколько объектов <see cref="EFPMenuOutItem"/> в списке <see cref="EFPMenuOutHandler.Items"/> могут иметь одинаковые команды.
    /// В этом случае в меню добавляется только одна команда, которая показывает диалог для выбора варианта отправки
    /// </summary>
    public NamedList<EFPSendToItem> SendToItems { get { return _SendToItems; } }
    private readonly NamedList<EFPSendToItem> _SendToItems;

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    #endregion

    #region Методы

    /// <summary>
    /// Выполняет печать.
    /// </summary>
    /// <param name="defaultPrinter">true - печать на принтере по умолчанию, false - требуется выести диалог выбора принтера</param>
    /// <returns></returns>
    public virtual bool Print(bool defaultPrinter)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Показывает диалог "Параметры страницы"
    /// </summary>
    /// <returns>true, если пользователь нажал кнопку "ОК"</returns>
    public virtual bool ShowPageSetup()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Выводит окно предварительного просмотра
    /// </summary>
    public virtual void ShowPrintPreview()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Выполняет экспорт в файл.
    /// На момент вызова уже был показан диалог сохранения файла
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <param name="item">Описание формата файла</param>
    public virtual void ExportFile(AbsPath filePath, EFPExportFileItem item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Выполняет команду "Отправить"
    /// </summary>
    /// <param name="item">Описание режима отправки</param>
    public virtual void SendTo(EFPSendToItem item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает <see cref="DisplayName"/>
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName;
    }

    #endregion

    #region События Prepare и PrepareAction

    internal void PerformPrepareAlone()
    {
      EFPMenuOutItemPrepareEventArgs args = new EFPMenuOutItemPrepareEventArgs(this, null);
      PerformPrepare(args);
    }

    /// <summary>
    /// Предотвращение повторного вызова
    /// </summary>
    private bool _Prepared;

    internal void PerformPrepare(EFPMenuOutItemPrepareEventArgs args)
    {
      if (_Prepared)
        return;
      _Prepared = true;
      OnPrepare(args);
    }

    /// <summary>
    /// Событие вызывается однакратно при инициализации списка команд локального меню
    /// </summary>
    public event EFPMenuOutItemPrepareEventHandler Prepare;

    /// <summary>
    /// Вызывает событие <see cref="Prepare"/>
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnPrepare(EFPMenuOutItemPrepareEventArgs args)
    {
      if (Prepare != null)
        Prepare(this, args);
    }

    internal void PerformPrepareAction()
    {
      OnPrepareAction(EventArgs.Empty);
    }

    /// <summary>
    /// Событие вызывается при подготовке списка команд меню к использованию.
    /// Событие может вызываться многократно.
    /// </summary>
    public event EventHandler PrepareAction;

    /// <summary>
    /// Вызывает событие <see cref="PrepareAction"/>
    /// </summary>
    protected virtual void OnPrepareAction(EventArgs args)
    {
      if (PrepareAction != null)
        PrepareAction(this, args);
    }

    #endregion
  }

  /// <summary>
  /// Обработчик команд локального меню "Печать", "Параметры страницы", "Предварительный просмотр", "Экспорт в файл" и "Отправить"
  /// </summary>
  public sealed class EFPMenuOutHandler
  {
    #region Конструктор

    /// <summary>
    /// Создает обработчик для заданного локального меню
    /// </summary>
    /// <param name="commandItems">Описания команд локального меню</param>
    public EFPMenuOutHandler(EFPContextCommandItems commandItems)
    {
      ciPrintDefault = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.PrintDefault);
      ciPrintDefault.Click += PrintDefault_Click;
      commandItems.Add(ciPrintDefault);

      ciPrint = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Print);
      ciPrint.Usage &= (~EFPCommandItemUsage.ToolBar); // два значка с принтерами не нужны :)
      ciPrint.Click += Print_Click;
      commandItems.Add(ciPrint);

      ciPageSetup = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.PageSetup);
      ciPageSetup.Click += PageSetup_Click;
      commandItems.Add(ciPageSetup);

      ciPrintPreview = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.PrintPreview);
      ciPrintPreview.Click += PrintPreview_Click;
      commandItems.Add(ciPrintPreview);

      ciExportFile = new EFPCommandItem("File", "ExportFile");
      ciExportFile.MenuText = "Экспорт в файл...";
      ciExportFile.Click += ExportFile_Click;
      commandItems.Add(ciExportFile);

      _MenuSendTo = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.MenuSendTo);
      _MenuSendTo.Usage = EFPCommandItemUsage.Menu;
      commandItems.Add(_MenuSendTo);
      // Заполнение подменю выполняется в обработчике Prepare. Прикладной код может добавлять свои команды "Отправить", не связанные с отчетом
      _SendToCommands = new List<EFPCommandItem>();

      _Items = new NamedList<EFPMenuOutItem>();
      _Enabled = true;
      commandItems.Prepare += CommandItems_Prepare;
    }

    //private class SendToGroupInfo
    //{
    //  #region Поля

    //  public EFPSendToItem FirstSendToItem;

    //  public EFPCommandItem FirstCommandInMenu;

    //  public EFPCommandItem FirstCommandInToolBar;

    //  public EFPCommandItem SubMenu;

    //  public EFPCommandItem SubToolBar;

    //  #endregion
    //}

    private void CommandItems_Prepare(object sender, EventArgs args)
    {
      EFPContextCommandItems commandItems = (EFPContextCommandItems)sender;

      bool canPrint = false;
      bool canPageSetup = false;
      bool canPrintPreview = false;
      bool canExport = false;

      // На первом проходе только собираем команды для меню SendTo
      // Значение созданные команды меню
      OrderSortedList<string, List<EFPSendToItem>> lstSendTo = new OrderSortedList<string, List<EFPSendToItem>>();

      foreach (EFPMenuOutItem item in Items)
      {
        EFPMenuOutItemPrepareEventArgs prepareEventArgs = new EFPMenuOutItemPrepareEventArgs(item, commandItems);
        item.PerformPrepare(prepareEventArgs);

        canPrint |= item.CanPrint;
        canPageSetup |= item.CanPageSetup;
        canPrintPreview |= item.CanPrintPreview;
        canExport |= (item.ExportFileItems.Count > 0);

        foreach (EFPSendToItem sendToItem in item.SendToItems)
        {
          List<EFPSendToItem> lst;
          if (!lstSendTo.TryGetValue(sendToItem.MainCode, out lst))
          {
            lst = new List<EFPSendToItem>();
            lstSendTo.Add(sendToItem.MainCode, lst);
          }
          lst.Add(sendToItem);

        } // SendToItems
      }

      // На втором проходе создаем команды для SendTo
      // Создаем две команды: одну для меню (ci1), вторую - для панели инструментов (ci2)
      foreach (KeyValuePair<string, List<EFPSendToItem>> pair in lstSendTo)
      {
        #region Команды локального меню

        // Если есть несколько команд с одинаковым основным кодом, то в меню "Отправить" создается подменю, куда добавляются команды.
        // Если есть только одна команда, то она добавляется непосредственно в меню "Отправить".

        EFPCommandItem subMenu1 = null;
        if (pair.Value.Count > 1)
        {
          subMenu1 = new EFPCommandItem("SendTo", pair.Key + "_SubMenu");
          subMenu1.MenuText = pair.Value[0].SubMenuText;
          subMenu1.Parent = MenuSendTo;
          subMenu1.Usage = EFPCommandItemUsage.Menu;
          commandItems.Add(subMenu1);
        }

        for (int i = 0; i < pair.Value.Count; i++)
        {
          EFPSendToItem sendToItem = pair.Value[i];

          EFPCommandItem ci1 = new EFPCommandItem("SendTo", sendToItem.Code + (i == 0 ? String.Empty : StdConvert.ToString(i + 1)));
          if (pair.Value.Count == 1)
          {
            // При запуске в Wine+Mono многие типы файлов открываются с помощью приложения winebrowser.exe, а других подходящих приложений нет.
            // Надо добавить в текст меню тип файла, иначе будет несколько команд с одинаковыми именами
            ci1.MenuText = sendToItem.SubMenuText + " -> " + sendToItem.MenuText;
            ci1.Parent = MenuSendTo;
          }
          else
          {
            ci1.MenuText = sendToItem.MenuText;
            if (i == 0)
              ci1.GroupEnd = true;
            ci1.Parent = subMenu1;
          }

          ci1.Image = sendToItem.Image;
          ci1.ToolTipText = sendToItem.ToolTipText;
          ci1.Usage = EFPCommandItemUsage.Menu;
          ci1.Tag = sendToItem.Code;
          ci1.Click += ciSendTo_Click;
          ci1.Enabled = Enabled;
          _SendToCommands.Add(ci1);
          commandItems.Add(ci1);
        }

        #endregion

        #region Кнопки панели инструментов

        // Первая ("основная") команда добавляется как обычная кнопка.
        // Если есть еще команды, то создается "уголочек", куда добавляются остальные команды

        EFPCommandItem subMenu2 = null;
        for (int i = 0; i < pair.Value.Count; i++)
        {
          if (i == 1)
          {
            subMenu2 = new EFPCommandItem("SendTo", pair.Key + "_SubTB");
            subMenu2.Parent = MenuSendTo;
            subMenu2.Usage = EFPCommandItemUsage.ToolBarDropDown;
            commandItems.Add(subMenu2);
          }

          EFPSendToItem sendToItem = pair.Value[i];

          EFPCommandItem ci2 = new EFPCommandItem("SendTo", sendToItem.Code + "_TB" + (i == 0 ? String.Empty : StdConvert.ToString(i + 1)));
          ci2.MenuText = sendToItem.MenuText;
          ci2.Image = sendToItem.Image;
          ci2.ToolTipText = sendToItem.ToolTipText;
          if (subMenu2 == null)
          {
            ci2.Parent = MenuSendTo;
            ci2.Usage = EFPCommandItemUsage.ToolBar;
          }
          else
          {
            ci2.Parent = subMenu2;
            ci2.Usage = EFPCommandItemUsage.Menu;
          }
          ci2.Tag = sendToItem.Code;
          ci2.Click += ciSendTo_Click;
          ci2.Enabled = Enabled;
          _SendToCommands.Add(ci2);
          commandItems.Add(ci2);
        }

        #endregion
      }

      if (!canPrint)
      {
        ciPrintDefault.Usage = EFPCommandItemUsage.None;
        ciPrint.Usage = EFPCommandItemUsage.None;
      }
      if (!canPageSetup)
        ciPageSetup.Usage = EFPCommandItemUsage.None;
      if (!canPrintPreview)
        ciPrintPreview.Usage = EFPCommandItemUsage.None;
      if (!canExport)
        ciExportFile.Usage = EFPCommandItemUsage.None;
      if (_MenuSendTo.Children.Count == 0)
        _MenuSendTo.Usage = EFPCommandItemUsage.None;
    }

    #endregion

    #region Список

    /// <summary>
    /// Список объектов, которые выполняют печать/экспорт.
    /// Обычно список состоит из одного объекта, предназначенного для печати просмотра. 
    /// </summary>
    public NamedList<EFPMenuOutItem> Items { get { return _Items; } }
    private readonly NamedList<EFPMenuOutItem> _Items;

    #endregion

    #region Свойства

    /// <summary>
    /// Блокирование всех команд локального меню
    /// </summary>
    public bool Enabled
    {
      get { return _Enabled; }
      set
      {
        _Enabled = value;
        ciPrintDefault.Enabled = value;
        ciPrint.Enabled = value;
        ciPageSetup.Enabled = value;
        ciPrintPreview.Enabled = value;
        ciExportFile.Enabled = value;
        foreach (EFPCommandItem ci in _SendToCommands)
          ci.Enabled = value;
      }
    }
    private bool _Enabled;

    #endregion

    #region Команды

    private readonly EFPCommandItem ciPrintDefault, ciPrint, ciPageSetup, ciPrintPreview;
    private readonly EFPCommandItem ciExportFile;
    private readonly List<EFPCommandItem> _SendToCommands;

    /// <summary>
    /// Подменю "Отправить".
    /// Может использоваться для добавления команд, не связанных с <see cref="EFPMenuOutHandler"/>.
    /// </summary>
    public EFPCommandItem MenuSendTo { get { return _MenuSendTo; } }
    private readonly EFPCommandItem _MenuSendTo;

    //private NamedList<EFPCommandItem> _SendToItems;

    private void PrintDefault_Click(object sender, EventArgs args)
    {
      EFPMenuOutItem outItem = SelectItem("Печать", ciPrint.ImageKey, null, delegate (EFPMenuOutItem item2) { return item2.CanPrint; });
      if (outItem != null)
        outItem.Print(true);
    }

    private void Print_Click(object sender, EventArgs args)
    {
      EFPMenuOutItem outItem = SelectItem("Печать", ciPrint.ImageKey, null, delegate (EFPMenuOutItem item2) { return item2.CanPrint; });
      if (outItem != null)
        outItem.Print(false);
    }

    private void PageSetup_Click(object sender, EventArgs args)
    {
      EFPMenuOutItem outItem = SelectItem(ciPageSetup.MenuTextWithoutMnemonic, ciPageSetup.ImageKey, null, delegate (EFPMenuOutItem item2) { return item2.CanPageSetup; });
      if (outItem != null)
        outItem.ShowPageSetup();
    }

    private void PrintPreview_Click(object sender, EventArgs args)
    {
      EFPMenuOutItem outItem = SelectItem(ciPrintPreview.MenuTextWithoutMnemonic, ciPrintPreview.ImageKey, null, delegate (EFPMenuOutItem item2) { return item2.CanPrintPreview; });
      if (outItem != null)
        outItem.ShowPrintPreview();
    }

    private static string LastExportFileCode = String.Empty;
    private static AbsPath LastExportFileDir = AbsPath.Empty;

    private void ExportFile_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      EFPMenuOutItem outItem = SelectItem(ciExportFile.MenuTextWithoutMnemonic, "Save", null, delegate (EFPMenuOutItem item2) { return item2.ExportFileItems.Count > 0; });
      if (outItem == null)
        return;

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < outItem.ExportFileItems.Count; i++)
      {
        if (i > 0)
          sb.Append('|');
        sb.Append(outItem.ExportFileItems[i].FilterText);
        sb.Append('|');
        sb.Append(outItem.ExportFileItems[i].FileMask);
      }

      SaveFileDialog dlg = new SaveFileDialog();
      dlg.Title = ci.MenuTextWithoutMnemonic;
      dlg.Filter = sb.ToString();
      dlg.FilterIndex = Array.IndexOf<string>(outItem.ExportFileItems.GetCodes(), LastExportFileCode) + 1; // FilterIndex нумеруется с 1
      if (!LastExportFileDir.IsEmpty)
        dlg.InitialDirectory = LastExportFileDir.Path;
      if (EFPApp.ShowDialog(dlg) != DialogResult.OK)
        return;

      LastExportFileCode = outItem.ExportFileItems.GetCodes()[dlg.FilterIndex - 1];

      AbsPath filePath = new AbsPath(dlg.FileName);
      LastExportFileDir = filePath.ParentDir;

      EFPApp.BeginWait("Экспорт в файл " + filePath.FileName, "Save");
      try
      {
        FileTools.ForceDirs(filePath.ParentDir);
        outItem.ExportFile(filePath, outItem.ExportFileItems[dlg.FilterIndex - 1]);
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    private void ciSendTo_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      string sendToCode = (string)(ci.Tag);
      EFPMenuOutItem item = SelectItem("Отправить в " + ci.MenuTextWithoutMnemonic, ci.ImageKey, ci.Image,
        delegate (EFPMenuOutItem item2) { return item2.SendToItems.Contains(sendToCode); });
      if (item == null)
        return;

      EFPSendToItem sendToItem = item.SendToItems.GetRequired(sendToCode);
      EFPApp.BeginWait("Отправка в " + sendToItem.MenuText, "Play");
      try
      {
        item.SendTo(sendToItem);
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    private static string _LastSelectedItemCode = String.Empty;

    private delegate bool ItemTester(EFPMenuOutItem item);

    private EFPMenuOutItem SelectItem(string title, string imageKey, Image image, ItemTester tester)
    {
      NamedList<EFPMenuOutItem> list2 = new NamedList<EFPMenuOutItem>();
      foreach (EFPMenuOutItem item in Items)
      {
        item.PerformPrepareAction();

        if (tester(item))
          list2.Add(item);
      }

      switch (list2.Count)
      {
        case 0:
          throw new BugException("Нет подходящих вариантов");
        case 1:
          return list2[0];
      }

      ListSelectDialog dlg = new ListSelectDialog();
      string[] a = new string[list2.Count];
      int selIndex = 0;
      for (int i = 0; i < list2.Count; i++)
      {
        a[i] = list2[i].DisplayName;
        if (String.Equals(list2[i].Code, _LastSelectedItemCode, StringComparison.Ordinal))
          selIndex = i;
      }
      dlg.Items = a;
      dlg.Title = title;
      if (String.IsNullOrEmpty(imageKey))
        dlg.Image = image;
      else
        dlg.ImageKey = imageKey;
      dlg.SelectedIndex = selIndex;
      if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        _LastSelectedItemCode = list2[dlg.SelectedIndex].Code;
        return list2[dlg.SelectedIndex];
      }
      else
        return null;
    }

    #endregion
  }

}
