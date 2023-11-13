// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Data;
using System.ComponentModel;
using FreeLibSet.Collections;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Logging;

/*
 * Построение табличных отчетов
 * Возможности:
 * - Отчет выводится в окне, содержащем одну или несколько закладок с табличными
 *   просмотрами
 * - Вверху выводится маленькая табличка с параметрами отчета, которые не помещаются
 *   в заголовке, например, установленных фильтрах
 * - Пользователь может нажать кнопку "Обновить" для перестроения отчета с такими
 *   же параметрами
 * - Пользователь может нажать кнопку "Перестроить" с возможностью ввести новые
 *   параметры. Созданный отчет может либо заместить существующий, либо открыть 
 *   новое окно
 * - Окно отчета с закладками встраивается в интерфейс MDI.
 * - В отчет могут входить как простые табличные просмотры, так и усложненные
 * - Инициализация табличных просмотров данными выполняется при первом обращении к
 *   соответствующей закладке
 * 
 * Класс GridReport является абстрактным. Конкретные отчеты наследуются из него. 
 * Им все равно нужны отдельные классы для работы с сервером
 */

namespace FreeLibSet.Forms
{
  #region Перечисления

  /// <summary>
  /// Значения свойства GridReport.ShowMode
  /// </summary>
  public enum EFPReportShowMode
  {
    /// <summary>
    /// Отчет выводится в модальном режиме, если есть открытые блоки диалога. В
    /// противном случае отчет встраивается в многоконный интерфейс.
    /// Этот режим используется по умолчанию.
    /// </summary>
    Auto,

    /// <summary>
    /// Отчет встраивается в многооконный интерфейс независимо от наличия открытых 
    /// блоков диалога. В этом случае отчет не будет доступен до их закрытия блоков диалога.
    /// Не рекомендуется использовать этот режим.
    /// </summary>
    Modeless,

    /// <summary>
    /// Отчет выводится в модальном режииме (как диалоговое окно)
    /// </summary>
    Modal
  }

  #endregion

  /// <summary>
  /// Процедура построения табличного отчета
  /// </summary>
  public abstract class EFPReport : IEFPFormCreator, IEFPConfigurable
  {
    #region Конструктор

    /// <summary>
    /// Создает объект отчета
    /// </summary>
    /// <param name="configSectionName">Обязательное имя секции конфигурации для идентификации типа отчета 
    /// и хранения параметров отчета</param>
    public EFPReport(string configSectionName)
    {
      if (String.IsNullOrEmpty(configSectionName))
        throw new ArgumentNullException("configSectionName");

      _RootProvider = new EFPReportRootProvider(this);
      _Pages = new PageCollection(this);
      _ConfigSectionName = configSectionName;
      _ReportCommandItems = new EFPReportCommandItems(this);

      _ReportCreated = false;
      _AlwaysShowPages = false;
      _SelectedPageIndex = 0;
      _ShowMode = EFPReportShowMode.Auto;
      _StoreComposition = null;
      _ReportCommandItems.InitCloseCommands();
    }

    #endregion

    #region Прочие свойства

    /// <summary>
    /// Параметры отчета (создаются перед вызовом BuildReport с помощью метода CreateParams)
    /// Также параметры могут быть созданы и присвоены свойству до вызова Run(),
    /// в этом случае запрос параметров не выполняется, если AlwaysQueryParams=false
    /// </summary>
    public EFPReportParams ReportParams
    {
      get { return _ReportParams; }
      set
      {
        if (IsExecuting)
          throw new InvalidOperationException("Свойство ReportParams можно устанавливать только до запуска процедуры отчета на выполнение");
        _ReportParams = value;
      }
    }
    private EFPReportParams _ReportParams;

    /// <summary>
    /// Если true, то вывод диалога параметров перед построением отчета выполняется,
    /// даже если свойство ReportParams было установлено до запуска отчета. При этом
    /// методы ReadConfig/WriteConfig также вызываются
    /// </summary>
    public bool AlwaysQueryParams { get { return _AlwaysQueryParams; } set { _AlwaysQueryParams = value; } }
    private bool _AlwaysQueryParams;

    /// <summary>
    /// Сюда могут быть добавлены команды локального меню, действующие для всей формы отчета.
    /// </summary>
    public EFPReportCommandItems ReportCommandItems { get { return _ReportCommandItems; } }
    private EFPReportCommandItems _ReportCommandItems;

    /// <summary>
    /// Имя изображения в EFPApp.MainImages, которое будт использовано в качестве значка формы
    /// </summary>
    public string MainImageKey
    {
      get { return _MainImageKey; }
      set
      {
        _MainImageKey = value;
        if (_TheForm != null)
          _TheForm.Icon = EFPApp.MainImages.Icons[value];
      }
    }
    private string _MainImageKey;

    /// <summary>
    /// Режим вывода отчета. По умолчанию используется автоматический режим:
    /// модальный, если есть открытые блоки диалога и немодальный в противном случае
    /// </summary>
    public EFPReportShowMode ShowMode
    {
      get { return _ShowMode; }
      set { _ShowMode = value; }
    }
    private EFPReportShowMode _ShowMode;

    /// <summary>
    /// Если установить значение true, то TabControl будет всегда создаваться,
    /// даже если отчет содержит только одну страницу.
    /// Свойство можно устанавливать в начале работы с отчетом.
    /// Установка в true также гарантирует вывод формы отчета на экран, если нет ни основных, ни
    /// дополнительных вкладок отчета.
    /// </summary>
    public bool AlwaysShowPages
    {
      get { return _AlwaysShowPages; }
      set { _AlwaysShowPages = value; }
    }
    private bool _AlwaysShowPages;

    /// <summary>
    /// Свойство возвращает true, если отчет был построен. Свойство может использоваться
    /// внутри реализации BuildReport для определения факта вызова из RefreshReport()
    /// (ReportCreated=true) или первичного построения отчета (ReportCreated=false)
    /// </summary>
    public bool ReportCreated { get { return _ReportCreated; } }
    private bool _ReportCreated;

    /// <summary>
    /// Контекст справки для формы отчета.
    /// Свойство должно устанавливаться до построения отчета.
    /// </summary>
    public string HelpContext
    {
      get { return _HelpContext; }
      set
      {
        if (_TheForm != null)
          throw new InvalidOperationException("Свойство должно быть установлено до вывода отчета");
        _HelpContext = value;
      }
    }
    private string _HelpContext;

    #endregion

    #region Страницы отчета

    /// <summary>
    /// Коллекция закладок отчета для свойства EFPReport.Pages.
    /// Основные вкладки обычно добавляются в конструкторе отчета, а дополнительные - в BuildReport().
    /// </summary>
    public sealed class PageCollection : IEFPReportPages
    {
      #region Конструктор

      internal PageCollection(EFPReport owner)
      {
        _Owner = owner;
        _Items = new List<EFPReportPage>();
        _ExtraPages = null;// new Hashtable(); потом создадим
      }

      #endregion

      #region Коллекция страниц

      /// <summary>
      /// Доступ к странице по индексу
      /// </summary>
      /// <param name="index">Индекс страницы в диапазоне от 0 до Count-1</param>
      /// <returns>Основная или дополнительная страница</returns>
      public EFPReportPage this[int index] { get { return _Items[index]; } }
      private List<EFPReportPage> _Items;

      /// <summary>
      /// Возвращает количество страниц
      /// </summary>
      public int Count { get { return _Items.Count; } }


      /// <summary>
      /// Возвращает индекс основной или дополнительной страницы
      /// </summary>
      /// <param name="page">Страница отчета</param>
      /// <returns>Индекс страницы</returns>
      public int IndexOf(EFPReportPage page)
      {
        return _Items.IndexOf(page);
      }

      /// <summary>
      /// Возвращает true, если страница принадлежит отчету и является страницей верхнего уровня
      /// </summary>
      /// <param name="page">Страница отчета</param>
      /// <returns>Наличие страницы</returns>
      public bool Contains(EFPReportPage page)
      {
        return _Items.Contains(page);
      }

      /// <summary>
      /// Добавление основной или дополнительной страницы отчета.
      /// Дополнительные страницы отличаются установленным свойством ExtraPageKey
      /// Метод может вызываться из команд локального меню для создания дополнительных
      /// страниц отчета. Также метод может вызываться в переопределенном BuildReport(),
      /// чтобы добавить страницы, наличие/отсутствие которых зависит от параметров
      /// отчета и не может быть определено в конструкторе.
      /// Дополнительные страницы, в отличие от основных, закрываются при повторном построении отчета и могут закрываться 
      /// программным способом.
      /// Эта версия метода делает страницу активной, если добавление выполняется не из BuildReport.
      /// </summary>
      /// <param name="page">Добавляемая страница</param>
      public void Add(EFPReportPage page)
      {
        Add(page, true);
      }

      /// <summary>
      /// Добавление основной или дополнительной страницы отчета.
      /// Дополнительные страницы отличаются установленным свойством ExtraPageKey
      /// Метод может вызываться из команд локального меню для создания дополнительных
      /// страниц отчета. Также метод может вызываться в переопределенном BuildReport(),
      /// чтобы добавить страницы, наличие/отсутствие которых зависит от параметров
      /// отчета и не может быть определено в конструкторе.
      /// Дополнительные страницы, в отличие от основных, закрываются при повторном построении отчета и могут закрываться 
      /// программным способом.
      /// Эта версия метода позволяет добавить страницу, но не активировать ее.
      /// </summary>
      /// <param name="page">Добавляемая страница</param>
      /// <param name="activate">Если true, то страница будет активирована</param>
      public void Add(EFPReportPage page, bool activate)
      {
        // CheckNotDisposed();
        if (page == null)
          throw new ArgumentNullException("page");
        page.CheckNotAdded();

        if (!String.IsNullOrEmpty(page.ExtraPageKey))
        {
          if (_ExtraPages == null)
            _ExtraPages = new BidirectionalDictionary<string, EFPReportPage>();

          _ExtraPages.Add(page.ExtraPageKey, page); // здесь возникнет исключение, если ключ повторяется
        }
        _Items.Add(page);
        page.BaseProvider.Parent = _Owner.RootProvider;

        // При первом построении отчета все просто. Не надо создавать сами закладки
        // Если же метод вызывается пользователем, например, при выполнении команды
        // локального меню, то требуются манипуляции с закладками
        if (_Owner.ReportCreated)
        {
          PrepareTheTabControl();
          TabPage theTabPage = new TabPage();
          page.TabPage = theTabPage;
          _Owner._TheTabControl.TabPages.Add(theTabPage);
          page.AssignParentControl(theTabPage);
          if (!_Owner.InsideBuildReport) // 29.07.2012 - но не во время нажатия F5
            page.DataReady = true;
          if (_Owner._TheForm != null)
            _Owner._TheForm.FormProvider.SetHelpContext(theTabPage, page.HelpContext);
          if (activate && (!_Owner.InsideBuildReport))  // 12.08.2012 - но не во время нажатия F5
          {
            page.Select(); // 24.10.2016
            //TheTabControl.SelectedIndex = TheTabControl.TabPages.Count - 1;
          }
        }

        _Owner.ReportCommandItems.InitCloseCommands();
      }

      /// <summary>
      /// Переключает отчет на режим работы с закладками
      /// </summary>
      private void PrepareTheTabControl()
      {
        if (_Owner._TheTabControl == null)
        {
          // Придется изменить вид просмотра и соорудить закладки
          _Owner.CreateTheTabControl();

          // 01.02.2011 Предотвращаем дефект, возникающий в табличном просмотре,
          // когда DataGridView оказывается на панели маленького размера.
          // При этом возникает исключение "Размер табличного просмотра недостаточен
          // для отображения текущей строки". Эта ошибка надоедает только в отладчике
          // и до пользователя не доходит
          _Owner._TheTabControl.Size = _Owner._TheMainPanel.Size;

          if (_Owner._TheMainPanel2 != null)
          {
            TabPage firstPage = new TabPage();
            _Items[0].TabPage = firstPage;
            _Owner._TheTabControl.TabPages.Add(firstPage);

            // Переносим существующую панель на закладку
            firstPage.Controls.Add(_Owner._TheMainPanel2);
          }
          // Присоединяем TabControl
          _Owner._TheMainPanel.Controls.Add(_Owner._TheTabControl);
        }
      }

      /// <summary>
      /// Закрыть дополнительную вкладку
      /// </summary>
      /// <param name="page">Страница вкладки</param>
      /// <returns>true, если страница была удалена</returns>
      public bool Remove(EFPReportPage page)
      {
        if (page == null)
          throw new ArgumentNullException("page");
        int pageIndex = _Items.IndexOf(page);
        if (pageIndex < 0)
          return false;

        RemoveAt(pageIndex);

        return true;
      }

      /// <summary>
      /// Удалить страницу с заданным индексом
      /// </summary>
      /// <param name="pageIndex">Индекс страницы в диапазоне от 0 до Count-1</param>
      public void RemoveAt(int pageIndex)
      {
        //if (!Page.IsExtraPage)
        //  throw new InvalidOperationException("Можно закрывать только дополнительные вкладки");

        if (pageIndex < 0 || pageIndex >= Count)
          throw new ArgumentOutOfRangeException("pageIndex", pageIndex, "Недопустимый индекс страницы");

        EFPReportPage page = _Items[pageIndex];
        page.BaseProvider.Parent = null; // 06.07.2021

        //if (FOwner.ReportCreated)
        //  PrepareTheTabControl();

        if (_Owner._TheTabControl != null) // теоретически, метод RemoveAt() может быть вызван до показа отчета
        {
          TabPage thePage = _Owner._TheTabControl.TabPages[pageIndex];
          _Owner._TheTabControl.TabPages.Remove(thePage);
          thePage.Dispose();
        }
        else if (_Owner._TheMainPanel2 != null)
        {
#if DEBUG
          if (Count != 1)
            throw new BugException("Неправильное число страниц: " + Count.ToString());
#endif

          _Owner._TheMainPanel2.Dispose();
          _Owner._TheMainPanel2 = null;
        }


        _Items.RemoveAt(pageIndex);
        if (page.IsExtraPage)
          _ExtraPages.RemoveValue(page);
        _Owner.ReportCommandItems.InitCloseCommands();
      }

      void ICollection<EFPReportPage>.Clear()
      {
        if (ExtraPageCount < Count)
          throw new InvalidOperationException("Нельзя удалить страницы, которые не являются дополнительными");

        for (int i = Count - 1; i >= 0; i--)
          RemoveAt(i);
      }

      /// <summary>
      /// Копирует страницы в массив
      /// </summary>
      /// <param name="array">Заполняемый массив</param>
      /// <param name="arrayIndex">Индекс, с которого надо заполнить элементы массива</param>
      public void CopyTo(EFPReportPage[] array, int arrayIndex)
      {
        _Items.CopyTo(array, arrayIndex);
      }

      /// <summary>
      /// Создает массив страниц
      /// </summary>
      /// <returns>Массив страниц</returns>
      public EFPReportPage[] ToArray()
      {
        return _Items.ToArray();
      }

      bool ICollection<EFPReportPage>.IsReadOnly { get { return false; } }

      #endregion

      #region Прочее

      private EFPReport _Owner;

      #endregion

      #region Дополнительные страницы

      /// <summary>
      /// Поиск дополнительной страницы без активации.
      /// Если страница с заданным ключом не найдена, возвращается null
      /// </summary>
      /// <param name="extraPageKey">Ключ для поиска</param>
      /// <returns>Страница или null, если страница не найдена</returns>
      public EFPReportPage this[string extraPageKey]
      {
        get
        {
          if (String.IsNullOrEmpty(extraPageKey))
            return null;
          if (_ExtraPages == null)
            return null;

          EFPReportPage page;
          if (_ExtraPages.TryGetValue(extraPageKey, out page))
            return page;
          else
            return null;
        }
      }

      /// <summary>
      /// Поиск и активация дополнительной страницы.
      /// </summary>
      /// <param name="extraPageKey">Ключ для поиска</param>
      /// <returns>true, если страница найдена и активирована</returns>
      public bool FindAndActivate(string extraPageKey)
      {
        // CheckNotDisposed();
        if (String.IsNullOrEmpty(extraPageKey))
          throw new ArgumentNullException("extraPageKey");

        if (_ExtraPages == null)
          return false;
        EFPReportPage page;
        if (!_ExtraPages.TryGetValue(extraPageKey, out page))
          return false;
        page.Select(); // 24.10.2016
        return true;
      }

      /// <summary>
      /// Очистка всех дополнительных страниц
      /// </summary>
      public void ClearExtraPages()
      {
        if (_ExtraPages == null)
          return;

        for (int i = _Items.Count - 1; i >= 0; i--)
        {
          if (_Items[i].IsExtraPage)
            RemoveAt(i);
        }
      }

      /// <summary>
      /// Очистка всех дополнительных страниц, кроме заданной
      /// </summary>
      /// <param name="excludePage">Страница, которую надо оставить</param>
      public void ClearExtraPagesExceptOne(EFPReportPage excludePage)
      {
        if (_ExtraPages == null)
          return;

        EFPReportPage[] a = GetExtraPages();
        for (int i = 0; i < a.Length; i++)
        {
          if (Object.ReferenceEquals(a[i], excludePage))
            continue;
          Remove(a[i]);
        }
      }

      /// <summary>
      /// Дополнительные страницы после добавления отчета
      /// </summary>
      private BidirectionalDictionary<string, EFPReportPage> _ExtraPages;

      /// <summary>
      /// Количество дополнительных страниц в отчете
      /// </summary>
      public int ExtraPageCount
      {
        get
        {
          if (_ExtraPages == null)
            return 0;
          else
            return _ExtraPages.Count;
        }
      }

      /// <summary>
      /// Добавлялись ли когда-нибудь доп. страницы?
      /// </summary>
      internal bool ExtraPagesHappened { get { return _ExtraPages != null; } }

      /// <summary>
      /// Возвращает все дополнительные страницы отчета.
      /// Если отчет не содержит дополнительных страниц, возвращается пустой массив
      /// </summary>
      /// <returns>Массив страниц</returns>
      public EFPReportPage[] GetExtraPages()
      {
        if (_ExtraPages == null)
          return new EFPReportPage[0];
        EFPReportPage[] a = new EFPReportPage[_ExtraPages.Count];
        _ExtraPages.Values.CopyTo(a, 0);
        return a;
      }

      #endregion

      #region IEnumerable<GridReportPage> Members

      /// <summary>
      /// Возвращает перечислитель по объектам EFPReportPage основных и дополнительных страниц
      /// </summary>
      /// <returns>Перечислитель</returns>
      public IEnumerator<EFPReportPage> GetEnumerator()
      {
        return _Items.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return _Items.GetEnumerator();
      }

      #endregion

      #region Отладочные методы

      internal string[] GetDebugTitles()
      {
        string[] a = new string[Count];
        for (int i = 0; i < Count; i++)
          a[i] = this[i].Title + " (" + this[i].GetType().ToString() + ")";
        return a;
      }

      #endregion
    }

    /// <summary>
    /// Основные и дополнительные страницы отчета
    /// </summary>
    public PageCollection Pages { get { return _Pages; } }
    private PageCollection _Pages;


    /// <summary>
    /// Индекс текущей страницы отчета
    /// Свойство можно устанавливать как после добавления страниц отчета, так
    /// и динамически
    /// </summary>
    public int SelectedPageIndex
    {
      get
      {
        if (_TheTabControl == null)
          return _SelectedPageIndex;
        else
          return _TheTabControl.SelectedIndex;
      }
      set
      {
        if (_TheTabControl == null)
          _SelectedPageIndex = value;
        else
          _TheTabControl.SelectedIndex = value;
      }
    }
    private int _SelectedPageIndex;

    /// <summary>
    /// Текущая выбранная страница отчета.
    /// Учитываются только страницы верхнего уровня. Страницы в EFPReportTabControlPage не учитываются
    /// </summary>
    public EFPReportPage SelectedPage
    {
      get
      {
        if (SelectedPageIndex >= Pages.Count)
          return null;
        return Pages[SelectedPageIndex];
      }
      set
      {
        int p = Pages.IndexOf(value);
        if (p >= 0)
          SelectedPageIndex = p;
      }
    }

    #endregion

    #region Секция конфигурации и IEFPConfigurable members

    /// <summary>
    /// Имя секции конфигурации для хранения настроек (задается при вызове конструктора)
    /// </summary>
    public string ConfigSectionName { get { return _ConfigSectionName; } }
    private string _ConfigSectionName;

    /// <summary>
    /// Используемый для отчета менеджер конфигураций.
    /// Если свойство не установлено в явном виде, то возвращается EFPApp.ConfigManager.
    /// Обычно нет причин устанавливать это свойство
    /// </summary>
    public IEFPConfigManager ConfigManager
    {
      get
      {
        if (_ConfigManager == null)
          return EFPApp.ConfigManager;
        else
          return _ConfigManager;
      }
      set
      {
        if (ReportCreated)
          throw new InvalidOperationException();
        _ConfigManager = value;
      }
    }
    private IEFPConfigManager _ConfigManager;

    #endregion

    #region Основной метод

    /// <summary>
    /// Основной метод выполнения отчета.
    /// 1. Запрашивает параметры отчета, если свойство ReportParams не было установлено.
    /// 2. Выполняет построение отчета BuildReport().
    /// 3. Показывает форму отчета, если есть хотя бы одна вкладка (основная или дополнительная),
    /// или установлено свойство AlwaysShowPages=true.
    /// </summary>
    public void Run()
    {
      if (_IsExecuting)
        throw new InvalidOperationException("Повторный запуск отчета");
      _IsExecuting = true;
      try
      {
        OnExecute();
      }
      catch (Exception e)
      {
        AddExceptionInfo(e);
        _IsExecuting = false;
        throw;
      }
    }

    /// <summary>
    /// Свойство возвращает true, если в данный момент отчет строится или на экран выведена форма отчета.
    /// </summary>
    public bool IsExecuting { get { return _IsExecuting; } }
    private bool _IsExecuting;

    /// <summary>
    /// Этот метод не должен вызываться напрямую.
    /// Для запуска отчета используйте метод Run(). 
    /// Для перестроение уже открытого отчета используйте RefreshReport()
    /// </summary>
    protected void OnExecute()
    {
      // Создаем основную форму
      _TheForm = new InternalReportForm(this);

      try
      {
        EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName,
        EFPConfigCategories.ReportParams, String.Empty);
        CfgPart cfg;
        using (ConfigManager.GetConfig(configInfo, EFPConfigMode.Read, out cfg))
        {

          //TheForm.StartPosition = FormStartPosition.WindowsDefaultLocation;

          // Убрано 14.09.2021 _TheForm.WindowState = cfg.GetBool("Maximized") ? FormWindowState.Maximized : FormWindowState.Normal;
        }

        _TheOwnerControl = _TheForm;

        _TheForm.FormProvider.HelpContext = HelpContext;
        _RootProvider.Parent = _TheForm.FormProvider;

        if (DoInitReport(_TheForm.FormProvider.CommandItems))
        {
          _ReportCreated = true; // признак построения отчета
          //DisposeAfterExecute = false;

          if (Pages.Count > 0 || AlwaysShowPages) // условие добавлено 24.11.2017
          {
            _TheForm.FormProvider.FormClosing += new FormClosingEventHandler(TheForm_FormClosing);
            _TheForm.FormProvider.FormClosed += new FormClosedEventHandler(TheForm_FormClosed);

            // 05.02.2008
            // Подсказки по страницам могли быть заданы до создания формы.
            // Если не перестроить объект ToolTip, то подсказки выводиться не будут
            // (например, в окне информации о документе)
            _TheForm.FormProvider.RefreshToolTips();

            if (!EFPApp.InsideLoadComposition)
            {
              switch (ShowMode)
              {
                case EFPReportShowMode.Auto:
                  EFPApp.ShowFormOrDialog(_TheForm);
                  break;
                case EFPReportShowMode.Modal:
                  EFPApp.ShowDialog(_TheForm, true);
                  break;
                case EFPReportShowMode.Modeless:
                  EFPApp.ShowChildForm(_TheForm);
                  break;
              }
            }
          }
          // иначе не показываем форму, пусть разрушается
        }
      }
      catch
      {
        // 02.08.2008, 28.08.2018
        // Если при построении отчета возникнет ошибка, то форма останется не разрушенной
        _TheForm.Dispose();
        _TheForm = null;
        throw;
      }
    }

    /// <summary>
    /// Общая часть инициализации для просмотра в собственном окне и во встроенной панели
    /// </summary>
    private bool DoInitReport(EFPCommandItems dstItems)
    {
      EFPApp.BeginWait("Подготовка к созданию отчета");
      try
      {
        DataGridView filtGrid = new DataGridView();
        filtGrid.Dock = DockStyle.Top;
        _TheFilterGridProvider = new EFPReportFilterGridView(_RootProvider, filtGrid);

        _TheMainPanel = new Panel();
        _TheMainPanel.Dock = DockStyle.Fill;


        dstItems.AddSeparator();
        dstItems.Add(ReportCommandItems); // 28.01.2021
      }
      finally
      {
        EFPApp.EndWait();
      }

      if (_ReportParams == null || AlwaysQueryParams)
      {
        if (_ReportParams == null)
          _ReportParams = CreateParams();

        // Обычный режим:
        // Загрузка, ввод параметров пользователем и их сохранение до следующего вызова

        OnBeforeQueryParams();

        if (_ReportParams is EFPReportExtParams)
        {
          if (!PerformQueryExtParams())
            return false;
        }
        else
        {
          if (!PerformQueryNormParams())
            return false;
        }
        OnAfterQueryParams();
      }

      // Если параметры были присоединены снаружи, то никаких действий не выполняется

      ReportParams.InitTitle();
      InitTitleAndGridFilter(); // заголовок формы установлен
      try
      {
        EFPApp.BeginWait("Построение отчета \"" + ReportParams.Title + "\"", MainImageKey);

        DoBuildReport();

        if (Pages.Count > 1 || AlwaysShowPages)
        {
          CreateTheTabControl();
          _TheMainPanel.Controls.Add(_TheTabControl);
        }
        _TheOwnerControl.Controls.Add(_TheMainPanel);
        _TheOwnerControl.Controls.Add(_TheFilterGridProvider.Control);

        if (_TheTabControl != null)
        {
          for (int i = 0; i < Pages.Count; i++)
          {
            TabPage theTabPage = new TabPage();
            Pages[i].TabPage = theTabPage;
            //TheTabPage.MouseClick += new MouseEventHandler(TabPage_MouseClick);
            _TheTabControl.TabPages.Add(theTabPage);
            Pages[i].AssignParentControl(theTabPage);
            if (_TheForm != null)
              _TheForm.FormProvider.SetHelpContext(theTabPage, Pages[i].HelpContext);
          }
          if (Pages.Count > 0)
          {
            if (_SelectedPageIndex < 0 || _SelectedPageIndex >= Pages.Count)
              _SelectedPageIndex = 0;
            _TheTabControl.SelectedIndex = _SelectedPageIndex;
            // TabControl не посылает сообщения при первой активации закладки
            TheTabControl_SelectedIndexChanged(null, null);
          }
        }
        else
        {
          if (Pages.Count > 0)
          {
            _TheMainPanel2 = new Panel();
            _TheMainPanel2.Dock = DockStyle.Fill;
            _TheMainPanel.Controls.Add(_TheMainPanel2);
            Pages[0].AssignParentControl(_TheMainPanel2);
            if (_TheForm != null)
            {
              if (!String.IsNullOrEmpty(Pages[0].HelpContext))
                _TheForm.FormProvider.HelpContext = Pages[0].HelpContext;
              else
                _TheForm.FormProvider.HelpContext = this.HelpContext; // 01.03.2023
            }
            // Страница становится активной
            Pages[0].OnPageSelected();
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }

      return true;
    }

    /// <summary>
    /// Этот метод вызывается перед выводом диалога параметров на экран.
    /// Чтение сохраненных настроек в объект ReportParams еше не выполнялось.
    /// Если отчет выводится без запроса параметров, метод не вызывается.
    /// </summary>
    protected virtual void OnBeforeQueryParams()
    {
    }

    /// <summary>
    /// Этот метод вызывается после закрытия диалога параметров отчета нажатием кнопки "ОК".
    /// Если пользователь отказался от построения отчета, метод не вызывается.
    /// Если отчет выводится без запроса параметров, метод не вызывается.
    /// </summary>
    protected virtual void OnAfterQueryParams()
    {
    }

    // Не работает так
    //internal static void TabPage_MouseClick(object Sender, MouseEventArgs Args)
    //{
    //  if (Args.Button == MouseButtons.Right)
    //  {
    //    TabPage TabPage = (TabPage)Sender;
    //    TabPage.Select();
    //  }
    //}

    private void CreateTheTabControl()
    {
      _TheTabControl = new TabControl();
      _TheTabControl.Dock = DockStyle.Fill;
      _TheTabControl.ImageList = EFPApp.MainImages.ImageList;
      _TheTabControl.ShowToolTips = true;
      _TheTabControl.SelectedIndexChanged += new EventHandler(TheTabControl_SelectedIndexChanged);
    }

    /// <summary>
    /// Выбрана закладка табличного просмотра
    /// </summary>
    void TheTabControl_SelectedIndexChanged(object sender, EventArgs args)
    {
      if (_TheTabControl.SelectedIndex < 0 || _TheTabControl.SelectedIndex >= Pages.Count)
        return;

      if (InsideBuildReport)
        return;

      try
      {
        EFPReportPage page = Pages[_TheTabControl.SelectedIndex];
        page.OnPageSelected();
      }
      catch (Exception e)
      {
        AddExceptionInfo(e);
        EFPApp.ShowException(e, "Ошибка при переключении на страницу отчета");
      }

      ReportCommandItems.InitCloseCommands();
    }

    void TheForm_FormClosing(object sender, FormClosingEventArgs args)
    {
      if (args.Cancel)
        return;
      try
      {
        if (!SaveViewConfig())
          args.Cancel = true;
        else
          OnReportFormClosing(args); // 07.09.2016
      }
      catch (Exception e)
      {
        AddExceptionInfo(e);
        EFPApp.ShowException(e, "Ошибка при закрытии окна отчета");
        args.Cancel = (EFPApp.MessageBox("Закрыть окно \"" + _TheForm.Text + "\" несмотря на возникшую ошибку?",
          "Ошибка при закрытии окна отчета", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK);
      }
    }

    /// <summary>
    /// Событие вызывается при попытке закрытия окна отчета.
    /// Обычно следует переопределять метод OnReportFormClosing(), а не использовать событие.
    /// </summary>
    public event FormClosingEventHandler ReportFormClosing;

    /// <summary>
    /// Вызывается при попытке закрытия окна отчета.
    /// Вызывает событие ReportFormClosing.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnReportFormClosing(FormClosingEventArgs args)
    {
      if (ReportFormClosing != null)
        ReportFormClosing(this, args);
    }

    void TheForm_FormClosed(object sender, FormClosedEventArgs args)
    {
      //DisposeWhenClosed();
      _IsExecuting = false;
      OnReportFormClosed(args); // 12.11.2016
    }

    /// <summary>
    /// Событие вызывается после закрытия окна отчета
    /// Обычно следует переопределять метод OnReportFormClosed(), а не использовать событие
    /// </summary>
    public event FormClosedEventHandler ReportFormClosed;

    /// <summary>
    /// Вызывается после закрытия окна отчета.
    /// Вызывает событие ReportFormClosed.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnReportFormClosed(FormClosedEventArgs args)
    {
      if (ReportFormClosed != null)
        ReportFormClosed(this, args);
    }


    /// <summary>
    /// Инициализация заголовка и таблички параметров
    /// </summary>
    private void InitTitleAndGridFilter()
    {
      if ((_TheOwnerControl is Form) || (_TheOwnerControl is GroupBox))
        _TheOwnerControl.Text = ReportParams.Title;

      _TheFilterGridProvider.Filters = ReportParams.FilterInfo.ToArray();
    }

    /// <summary>
    /// Этот метод вызывается перед закрытием отчета и должен запомнить текущие
    /// состояния отчета и вкладок. Для каждой вкладки вызывается EFPReportPage.SaveViewConfig()
    /// </summary>
    /// <returns>Возвращает true, если настройки для всех вкладок были успешно сохранены.</returns>
    protected virtual bool SaveViewConfig()
    {
      EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName,
        EFPConfigCategories.ReportParams, String.Empty);
      CfgPart cfg;
      using (ConfigManager.GetConfig(configInfo, EFPConfigMode.Write, out cfg))
      {
        // Убрано 14.09.2021. cfg.SetBool("Maximized", _TheForm.WindowState == FormWindowState.Maximized);
        for (int i = 0; i < Pages.Count; i++)
        {
          if (!Pages[i].SaveViewConfig(cfg))
            return false;
        }
      }
      return true;
    }

    #endregion

    #region Запуск отчета, встроенного в другую форму

    /// <summary>
    /// Запуск отчета, встроенного в другую форму.
    /// </summary>
    /// <param name="ownerControlProvider"></param>
    /// <param name="baseProvider"></param>
    /// <returns></returns>
    public bool RunBuiltIn(EFPControlBase ownerControlProvider, EFPBaseProvider baseProvider)
    {
      _TheOwnerControl = ownerControlProvider.Control;

      _TheForm = null;
      //TheFormProvider.HelpContext = "BuxBase.chm::Report.htm";
      _RootProvider.Parent = baseProvider;

      if (!DoInitReport(ownerControlProvider.CommandItems))
        return false;

      _ReportCreated = true; // признак построения отчета
      //DisposeAfterExecute = false;
      //TheForm.FormClosing += new FormClosingEventHandler(TheForm_FormClosing);
      //TheForm.FormClosed += new FormClosedEventHandler(TheForm_FormClosed);

      // 05.02.2008
      // Подсказки по страницам могли быть заданы до создания формы.
      // Если не перестроить объект ToolTip, то подсказки выводиться не будут
      // (например, в окне информации о документе)
      //TheFormProvider.RefreshToolTips();

      return true;
    }

    #endregion

    #region Абстрактные методы

    /// <summary>
    /// Создать объект параметров с установками по умолчанию. Используется при
    /// запуске отчета и в случае, если загрузка сохраненной конфигурации вызвала
    /// исключение.
    /// Непереопределенный метод вызывает исключение.
    /// </summary>
    /// <returns>Созданный объект параметров</returns>
    protected virtual EFPReportParams CreateParams()
    {
      throw new InvalidOperationException("Этот отчет всегда использует задаваемые снаружи параметры");
    }

    /// <summary>
    /// Вывод диалога для запроса параметров отчета. Метод берет параметры из свойства
    /// ReportParams и туда же их записывает
    /// Этот виртуальный метод не вызывается, если CreateParams() вернул расширенный
    /// объект параметров GridReportExtParams. В последнем случае форма создается
    /// и обрабатывется с помощью методов объекта расширенных параметров
    /// </summary>
    /// <returns>True, если параметры введены и можно строить отчет. 
    /// False, если пользователь нажал кнопку "Отмена"</returns>
    protected virtual bool QueryParams()
    {
      return false;
    }

    /// <summary>
    /// Построение отчета.
    /// Метод может добавить дополнительные вкладки отчета.
    /// </summary>
    protected abstract void BuildReport();

    #endregion

    #region Запрос параметров отчета

    /// <summary>
    /// Обычный запрос (нерасширенных) параметров с использованием метода
    /// QueryParams()
    /// Пользователь не может хранить готовые наборы параметров
    /// </summary>
    /// <returns></returns>
    private bool PerformQueryNormParams()
    {
      EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName,
        EFPConfigCategories.ReportParams, String.Empty);
      CfgPart cfg;
      using (ConfigManager.GetConfig(configInfo, EFPConfigMode.Write, out cfg))
      {
        if (!cfg.IsEmpty) // 31.08.2015
          SafeReadConfig(cfg);
        try
        {
          if (!QueryParams())
            return false;
        }
        catch (Exception e)
        {
          AddExceptionInfo(e);
          EFPApp.ShowException(e, "Ошибка при запросе параметров");
          return false;
        }

        SafeWriteConfig(cfg);
      }
      return true;
    }

    /// <summary>
    /// Запрос параметров отчета с использованием расширенных возможностей
    /// </summary>
    /// <returns></returns>
    private bool PerformQueryExtParams()
    {
      EFPReportExtParamsHelper paramsHelper = new EFPReportExtParamsHelper(this);
      return paramsHelper.PerformQueryParams();
    }

    private void SafeReadConfig(CfgPart cfg)
    {
      try
      {
        _ReportParams.ReadConfig(cfg);
      }
      catch (Exception e)
      {
        EFPApp.ErrorMessageBox("Возникла ошибка при загрузке ранее сохраненных параметров отчета. " +
          "Будут использованы значения по умолчанию. Сообщение об ошибке: " + e.Message,
          "Ошибка загрузки параметров");
        _ReportParams = CreateParams();
      }
    }

    private void SafeWriteConfig(CfgPart cfg)
    {
      try
      {
        ReportParams.WriteConfig(cfg);
      }
      catch (Exception e)
      {
        EFPApp.ErrorMessageBox("Возникла ошибка при сохранении параметров отчета. " +
          "Сообщение об ошибке: " + e.Message,
          "Ошибка записи параметров");
      }
    }

    //void ciDebugParams_Click(object sender, EventArgs args)
    //{
    //  TempCfg Cfg = new TempCfg();
    //  ReportParams.WriteConfig(Cfg);
    //  EFPApp.ShowXmlView(Cfg.Document, "Параметры отчета");
    //}

    #endregion

    #region Методы управления отчетом

    /// <summary>
    /// Возвращает true, если в данный момент выполняется метод BuildReport.
    /// В отличие от IsExecuting, После построения отчета, когда форма выведена на экран, свойство возвращает false
    /// </summary>
    public bool InsideBuildReport { get { return _InsideBuildReport; } }
    private bool _InsideBuildReport;

    private void DoBuildReport()
    {
      // CheckNotDisposed();

      if (InsideBuildReport)
        throw new InvalidOperationException("Нельзя повторно вызвать BulidReport()");

      _InsideBuildReport = true;
      try
      {
        _ReportParams.FilterInfoModified = false;
        using (LocalLogoutObjects llo = new LocalLogoutObjects())
        {
          llo.Title = "Current EFPReport Building";
          llo.Add("Report class", GetType().ToString());
          llo.Add("ReportParams", ReportParams);
          llo.Add("ReportCreated", ReportCreated);

          // Вызываем виртуальный метод
          BuildReport();
        }

        if (_ReportParams.FilterInfoModified)
          InitTitleAndGridFilter(); // 12.11.2013
      }
      finally
      {
        _InsideBuildReport = false;
      }

      ReportCommandItems.InitCloseCommands(); // во время построения отчета команды могли не обновляться
    }

    /// <summary>
    /// Обновить отчет.
    /// </summary>
    public void RefreshReport()
    {
      // CheckNotDisposed();

      Pages.ClearExtraPages();

      // Сбрасываем буферизацию документов
      // TODO: AccDepClientExec.BufTables.Clear();

      // Очищаем признаки готовности данных
      for (int i = 0; i < Pages.Count; i++)
        Pages[i].DataReady = false;

      // Построение отчета
      DoBuildReport();

      // Текущей странице отчета посылаем PageSelected
      int currPageIndex;
      if (_TheTabControl == null)
        currPageIndex = 0;
      else
        currPageIndex = _TheTabControl.SelectedIndex;
      if (currPageIndex < Pages.Count)
      {
        Pages[currPageIndex].OnPageSelected();

        //// 30.08.2017
        //Pages[CurrPageIndex].ParentControl.Visible = false;
        //Pages[CurrPageIndex].ParentControl.Visible = true;
        //// Pages[CurrPageIndex].Select();
      }
    }

    /// <summary>
    /// Возвращает форму, содержащую отчет (это может быть, в частности, форма мастера)
    /// в процессе работы отчета.
    /// Если отчет еще не запущен или уже закрыт, возвращается null
    /// </summary>
    public Form ReportForm
    {
      get
      {
        if (_TheOwnerControl == null)
          return null;
        Form frm = _TheOwnerControl.FindForm();
        if (frm == null)
          return null;
        if (frm.IsDisposed)
          return null;
        if (!frm.Visible)
          return null;
        return frm;
      }
    }

    /// <summary>
    /// Закрыть окно отчета.
    /// Метод может использоваться в обработчиках кнопок или команд локального меню,
    /// если требуется закрыть окно отчета и, например, построить другой отчет
    /// Если отчет встроен в шаг мастера, выполняется закрытие всего мастера
    /// Если отчет еще не открыт или уже закрыт, никаких действий не выполняется
    /// </summary>
    public void CloseReport()
    {
      Form frm = ReportForm;
      if (frm != null)
        frm.Close();
    }

    /// <summary>
    /// Выполняют рекурсивную инициализацию всех страниц отчета, для которых еше не был вызван метод SetPageCreated()
    /// </summary>
    public void SetAllPagesCreated()
    {
      for (int i = 0; i < Pages.Count; i++)
        Pages[i].SetAllPagesCreated();
    }

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Отдельный класс формы нужен, чтобы можно было искать отчеты
    /// </summary>
    private class InternalReportForm : Form
    {
      #region Конструктор

      public InternalReportForm(EFPReport report)
      {
        _Report = report;

        if (String.IsNullOrEmpty(report.MainImageKey))
          Icon = EFPApp.MainImages.Icons["Table"];
        else
          Icon = EFPApp.MainImages.Icons[report.MainImageKey];
        Size = new System.Drawing.Size(Screen.PrimaryScreen.Bounds.Width * 3 / 4,
          Screen.PrimaryScreen.Bounds.Height * 3 / 4);

        _FormProvider = new EFPFormProvider(this);

        // Будет записан другой тег Class
        _FormProvider.ConfigSectionName = report.ConfigSectionName;
        if (report.StoreComposition)
        {
          _FormProvider.ConfigClassName = report.GetType().ToString();
          _FormProvider.ConfigHandler.Sources.Add(report);
        }
        else
        {
          _FormProvider.ConfigClassName = String.Empty;
          // убрано 14.09.2021 _FormProvider.ConfigSectionName = String.Empty;
        }
      }

      #endregion

      public EFPReport Report { get { return _Report; } }
      private EFPReport _Report;

      /// <summary>
      /// Провайдер формы для поддержки справочной системы
      /// Может быть null, если отчет запускается встроенным в окно
      /// </summary>
      public EFPFormProvider FormProvider { get { return _FormProvider; } }
      private EFPFormProvider _FormProvider;


#if DEBUG

      //public bool TestClosing()
      //{
      //  FormClosingEventArgs Args1 = new FormClosingEventArgs(CloseReason.UserClosing, false);
      //  OnFormClosing(Args1);
      //  CancelEventArgs Args2 = new CancelEventArgs(Args1.Cancel);
      //  OnClosing(Args2);
      //  return Args2.Cancel;
      //}

#endif
    }

    /// <summary>
    /// Основная форма отчета.
    /// null, если отчет запущен встроенным вдругое окно
    /// </summary>
    private InternalReportForm _TheForm;

    /// <summary>
    /// Панель, куда встроен отчет или совпадает с TheForm, если отчет запущен
    /// в собственном окне
    /// </summary>
    private Control _TheOwnerControl;

    internal class EFPReportRootProvider : EFPBaseProvider
    {
      #region Конструктор

      internal EFPReportRootProvider(EFPReport ownerReport)
      {
        _OwnerReport = ownerReport;
      }

      #endregion

      #region Свойства

      public EFPReport OwnerReport { get { return _OwnerReport; } }
      private EFPReport _OwnerReport;

      #endregion
    }

    /// <summary>
    /// Провайдер формы создается вместе с формой.
    /// Этот провайдер используется как родительский при добавлении страниц и 
    /// создается в конструкторе
    /// </summary>
    internal EFPReportRootProvider RootProvider { get { return _RootProvider; } }
    private EFPReportRootProvider _RootProvider;

    /// <summary>
    /// Набор закладок в режиме нескольких страниц
    /// </summary>
    private TabControl _TheTabControl;

    /// <summary>
    /// Основная панель для TabControl или для одной страницы
    /// </summary>
    private Panel _TheMainPanel;

    /// <summary>
    /// В режиме одной страницы используем еще одну панель, которая целиком
    /// занимает TheMainPanel, чтобы потом можно было сделать перенос в
    /// TabControl. Без нее пришлось бы переносить дочерние элементы по одному
    /// </summary>
    private Panel _TheMainPanel2;

    private EFPReportFilterGridView _TheFilterGridProvider;

    /// <summary>
    /// Добавление отладочной информации в случае возникновения исключения
    /// </summary>
    /// <param name="e">Объект исключения</param>
    public virtual void AddExceptionInfo(Exception e)
    {
      try
      {
        e.Data["EFPReport.ConfigSectionName"] = ConfigSectionName;
        e.Data["EFPReport.ShowMode"] = ShowMode;
        if (ReportParams == null)
          e.Data["EFPReport.ReportParams"] = null;
        else
        {
          EFPReportExtParams ep = ReportParams as EFPReportExtParams;
          if (ep != null)
          {
            if ((ep.UsedParts & SettingsPart.User) != 0)
            {
              TempCfg cfg = new TempCfg();
              ep.WriteConfig(cfg, SettingsPart.User);
              e.Data["EFPReport.ReportParams.CfgPartUser"] = DataTools.XmlDocumentToString(cfg.Document);
            }
            if ((ep.UsedParts & SettingsPart.Machine) != 0)
            {
              TempCfg cfg = new TempCfg();
              ep.WriteConfig(cfg, SettingsPart.Machine);
              e.Data["EFPReport.ReportParams.CfgPartFiles"] = DataTools.XmlDocumentToString(cfg.Document);
            }
          }
          else
          {
            TempCfg cfg = new TempCfg();
            ReportParams.WriteConfig(cfg);
            e.Data["EFPReport.ReportParams.CfgPart"] = DataTools.XmlDocumentToString(cfg.Document);
          }
        }
        e.Data["EFPReport.ReportCreated"] = ReportCreated;
        e.Data["EFPReport.Pages"] = Pages.GetDebugTitles();
        e.Data["EFPReport.SelectedPageIndex"] = SelectedPageIndex;
        e.Data["EFPReport.IsExecuting"] = IsExecuting;
        e.Data["EFPReport.InsideBuildReport"] = InsideBuildReport;
      }
      catch { } // вложенные исключения не повторяются
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Найти и активировать форму отчета выбранного типа (по имени типа)
    /// </summary>
    /// <param name="configSectionName">Имя секции конфигурации</param>
    /// <param name="refresh">Если True и отчет найден, то он будет перестроен вызовом PereformRefresh()</param>
    /// <returns>Объект отчета, если он был найден</returns>
    public static EFPReport FindAndActivate(string configSectionName, bool refresh)
    {
      EFPReport report = FindReport(configSectionName);
      if (report != null)
      {
        EFPApp.Activate(report._TheForm);
        if (refresh)
          report.RefreshReport();
      }
      return report;
    }

    /// <summary>
    /// Найти и активировать форму отчета выбранного типа (по типу объекта GridReport)
    /// Тип должен быть указан точно с помощью typeof(). Если есть открытый отчет с
    /// классом, производным от искомого, то он активирован не будет
    /// </summary>
    /// <param name="gridReportType">typeof(ТипОбъекта, производный от GridReport)</param>
    /// <param name="refresh">Надо обновить найденный объект отчета?</param>
    /// <returns>Объект отчета, если он был найден</returns>
    public static EFPReport FindAndActivate(Type gridReportType, bool refresh)
    {
      EFPReport report = FindReport(gridReportType);
      if (report != null)
      {
        EFPApp.Activate(report._TheForm);
        if (refresh)
          report.RefreshReport();
      }
      return report;
    }

    /// <summary>
    /// Найти отчет заданного типа
    /// </summary>
    /// <param name="configSectionName">Имя секции конфигурации для отчета</param>
    /// <returns>Найденный отчет или null</returns>
    public static EFPReport FindReport(string configSectionName)
    {
      // Ищем уже существующую форму
      if (EFPApp.Interface == null)
        return null;
      InternalReportForm[] forms = EFPApp.Interface.FindChildForms<InternalReportForm>();
      for (int i = 0; i < forms.Length; i++)
      {
        if (forms[i].Report.ConfigSectionName == configSectionName)
          return forms[i].Report;
      }
      return null;
    }

    /// <summary>
    /// Найти отчет заданного типа
    /// </summary>
    /// <param name="gridReportType">Тип отчета, производного от EFPReport</param>
    /// <returns>Найденный отчет или null</returns>
    public static EFPReport FindReport(Type gridReportType)
    {
      // Ищем уже существующую форму
      if (EFPApp.Interface == null)
        return null;
      InternalReportForm[] forms = EFPApp.Interface.FindChildForms<InternalReportForm>();
      for (int i = 0; i < forms.Length; i++)
      {
        if (forms[i].Report.GetType() == gridReportType)
          return forms[i].Report;
      }
      return null;
    }

    /// <summary>
    /// Шаблонный статический метод, который можно вызывать из главного меню.
    /// Позволяет не реализовывать отдельные обработчики для запуска отчетов
    /// Создает объект отчета EFPReport и вызывает его метод Run().
    /// </summary>
    /// <typeparam name="T">Класс отчета, производного от EFPReport. Класс должен реализовывать конструктор по умолчанию</typeparam>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Не используется</param>
    /// <remarks>
    /// Можно не бояться перерасхода памяти. Класс <typeref name="T"/> будет загружен в память только при 
    /// выполнении команды меню, а не когда создается обработчик при добавлении команды меню.
    /// </remarks>
    public static void Run<T>(object sender, EventArgs args)
      where T : EFPReport, new()
    {
      new T().Run();
    }

    #endregion

    #region Сохранение интерфейса

    #region StoreComposition

    /// <summary>
    /// Надо ли восстанавливать окно отчета при восстановлении композиции рабочего стола.
    /// По умолчанию true - при восстановлении композиции отчет будет построен заново.
    /// Если у класса отчета нет конструктора без параметров или класс не имеет видимости public,
    /// значением по умолачнию является false.
    /// Если DefaultStoreComposition=false, то значением по умолчанию также является false
    /// </summary>
    public bool StoreComposition
    {
      get
      {
        if (_StoreComposition.HasValue)
          return _StoreComposition.Value;
        else
        {
          if (DefaultStoreComposition)
          {
            if (!this.GetType().IsPublic)
              return false;
            if (this.GetType().GetConstructor(Type.EmptyTypes) == null)
              return false;
            return true;
          }
          else
            return false;
        }
      }
      set
      {
        if (IsExecuting || ReportCreated)
          throw new InvalidOperationException();
        _StoreComposition = value;
      }
    }

    private bool? _StoreComposition;

    /// <summary>
    /// Надо ли сохранять композицию окон отчетов по умолчанию.
    /// Действует только для тех отчетов, для которых это возможно
    /// </summary>
    public static bool DefaultStoreComposition
    {
      get { return _DefaultStoreComposition; }
      set { _DefaultStoreComposition = value; }
    }
    private static bool _DefaultStoreComposition = true;

    #endregion

    #region IEFPFormCreator Members

    Form IEFPFormCreator.CreateForm(EFPFormCreatorParams creatorParams)
    {
      // На момент вызова создан объект класса, производного от EFPReport.
      // Конструктор EFPReport вызван, но параметры еще не созданы
      // Форма тоже еше не создана
      try
      {
        ReportParams = CreateParams();
      }
      catch (InvalidOperationException)
      {
        return null;
      }
      if (ReportParams == null)
        return null;
      // EFPReportEmptyParams являются допустимыми параметрами для восстановления отчета

      CfgPart cfgReportParams = creatorParams.Config.GetChild(EFPConfigCategories.ReportParams, false);
      if (cfgReportParams == null)
        throw new NullReferenceException("Не удалось найти секцию параметров отчета " + creatorParams.Title);
      ReportParams.ReadConfig(cfgReportParams);

      Run();
      _TheForm.FormProvider.ReadComposition(creatorParams.Config);
      return _TheForm;
    }

    #endregion

    #region IEFPConfigurable Members

    /// <summary>
    /// Добавление категорий при сохранении конфигурации для реализации интерфейса IEFPConfigurable.
    /// Добавляет категорию "ReportParams", если выполняется сохранение композиции рабочего стола.
    /// </summary>
    /// <param name="categories">Список для добавления категорий</param>
    /// <param name="rwMode">Чтение или запись</param>
    /// <param name="actionInfo">Информация о действии</param>
    public void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      if (actionInfo.Purpose == EFPConfigPurpose.Composition && rwMode == EFPConfigMode.Write)
        categories.Add(EFPConfigCategories.ReportParams);

      if (actionInfo.Purpose == EFPConfigPurpose.Composition)
        SetAllPagesCreated();
    }

    /// <summary>
    /// Запись секции конфигурации
    /// </summary>
    /// <param name="category">Категория "ReportParams"</param>
    /// <param name="cfg">Записываемая секция конфигурации</param>
    /// <param name="actionInfo">Информация о действии</param>
    public void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      if (category == EFPConfigCategories.ReportParams)
        ReportParams.WriteConfig(cfg);
    }

    /// <summary>
    /// Чтение секции конфигурации для реализации интерфейса IEFPConfigurable.
    /// Ничего не делается, так как инициализация выполняется отдельно
    /// </summary>
    /// <param name="category">Не используется</param>
    /// <param name="cfg">Не используется</param>
    /// <param name="actionInfo">Не используется</param>
    public void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      //if (Category == EFPConfigCategories.ReportParams)
      //  ReportParams.ReadConfig(Config);
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// Базовый класс для хранения параметров отчета. 
  /// Наследник должен обеспечивать преобразование в xml-формат для хранения и
  /// копирования параметров
  /// </summary>
  public abstract class EFPReportParams
  {
    #region Конструктор

    /// <summary>
    /// Создает объект набора параметров.
    /// Инициализиует пустой список фильтров FilterInfo
    /// </summary>
    public EFPReportParams()
    {
      _FilterInfo = new GridReportFilterItems2(this);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Заголовок отчета.
    /// Обычно свойство должно устанавливаться из переопределенного метода OnInitTitle() или обработчика
    /// события TitleNeeded. Если заголовок не зависит от параметров отчета, то его можно установить
    /// после создания объекта параметров.
    /// </summary>
    public string Title { get { return _Title; } set { _Title = value; } }
    private string _Title;

    private class GridReportFilterItems2 : EFPReportFilterItems
    {
      #region Конструктор

      public GridReportFilterItems2(EFPReportParams owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Свойства

      private EFPReportParams _Owner;

      #endregion

      #region Переопределенные методы

      internal protected override void OnChanged()
      {
        base.OnChanged();

        _Owner.FilterInfoModified = true;
      }

      #endregion
    }

    /// <summary>
    /// Фильтры отчета
    /// Свойство заполняется при вызове InitTitle()
    /// </summary>
    public EFPReportFilterItems FilterInfo { get { return _FilterInfo; } }
    private EFPReportFilterItems _FilterInfo;

    /// <summary>
    /// Флажок для отслеживания изменений в фильтрах отчета, если они выполняются в процессе построения отчета
    /// </summary>
    internal bool FilterInfoModified;

    #endregion

    #region Переопределяемые методы методы

    /// <summary>
    /// Очищает список фильтров и вызывает виртуальный метод OnInitTitle()
    /// </summary>
    public void InitTitle()
    {
      FilterInfo.Clear();
      OnInitTitle();
      if (TitleNeeded != null)
        TitleNeeded(this, EventArgs.Empty);
    }

    /// <summary>
    /// Событие вызывается из InitTitle().
    /// </summary>
    public event EventHandler TitleNeeded;

    /// <summary>
    /// Метод должен установить свойство Title и заполнить FilterInfo.
    /// Вместо переопределения этого метода можно присоединить обработчик события TitleNeeded.
    /// Если заголовок отчета не зависит от его параметров, свойство Title можно установить после создания объекта.
    /// Непереопределенный метод не выполняет никаких действий
    /// </summary>
    protected virtual void OnInitTitle()
    {
    }

    /// <summary>
    /// Прочитать параметры из секции конфигурации
    /// </summary>
    /// <param name="cfg"></param>
    public abstract void ReadConfig(CfgPart cfg);

    /// <summary>
    /// Записать параметры в секцию конфигурации
    /// </summary>
    /// <param name="cfg"></param>
    public abstract void WriteConfig(CfgPart cfg);

    #endregion
  }

  /// <summary>
  /// Неабстрактная реализация EFPReportParams для отчетов, не требующих параметров.
  /// Хранит заголовок отчета
  /// </summary>
  public sealed class EFPReportEmptyParams : EFPReportParams
  {
    #region Конструктор

    /// <summary>
    /// Создает набор "параметров"
    /// </summary>
    /// <param name="title">Заголовок отчета</param>
    public EFPReportEmptyParams(string title)
    {
      base.Title = title;
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Этот метод ничего не делает, так как заголовок задается в конструкторе отчета и в дальнейшем не меняется
    /// </summary>
    protected override void OnInitTitle()
    {
      // Заголовок уже установлен
    }

    /// <summary>
    /// Ничего не делает, так как нет сохраняемых параметров
    /// </summary>
    /// <param name="cfg"></param>
    public override void ReadConfig(CfgPart cfg)
    {
      // нет
    }

    /// <summary>
    /// Ничего не делает, так как нет сохраняемых параметров
    /// </summary>
    /// <param name="cfg"></param>
    public override void WriteConfig(CfgPart cfg)
    {
      // нет
    }

    #endregion
  }

  /// <summary>
  /// Страничка отчета. Базовый класс
  /// </summary>
  public abstract class EFPReportPage : IEFPTabPageControl
  {
    #region Конструктор

    /// <summary>
    /// Инициализация объекта
    /// </summary>
    public EFPReportPage()
    {
      //HelpContext = "BuxBase.chm::Report.htm";
      _BaseProvider = new EFPReportPageBaseProvider();
      _BaseProvider.ReportPage = this;
      _FilterInfo = new EFPReportPageFilterItems(this);
      _ExtraPageKey = String.Empty;
    }

    #endregion

    #region BaseProvider

    private class EFPReportPageBaseProvider : EFPBaseProvider
    {
      #region Поля

      public EFPReportPage ReportPage;

      #endregion

      #region Команды локального меню

      public override void InitCommandItemList(List<EFPCommandItems> list)
      {
        if (ReportPage._CommandItems != null)
          list.Add(ReportPage._CommandItems);
        base.InitCommandItemList(list);
      }

      #endregion
    }

    /// <summary>
    /// Базовый провайдер для страницы отчета.
    /// Используется (прямо или косвенно) при создании при вызове CreatePage()
    /// для создания провайдеров элементов управления на странице отчета.
    /// </summary>
    public EFPBaseProvider BaseProvider { get { return _BaseProvider; } }
    private EFPReportPageBaseProvider _BaseProvider;

    private class EFPReportPageCommandItems : EFPCommandItems
    {
      #region Конструктор

      public EFPReportPageCommandItems(EFPReportPage reportPage)
      {
        _ReportPage = reportPage;

        #region Команды закрытия доп. вкладок

        ciClose = new EFPCommandItem("View", "CloseTab");
        ciClose.MenuText = "Закрыть вкладку";
        ciClose.Click += new EventHandler(ciClose_Click);
        ciClose.GroupBegin = true;
        Add(ciClose);

        ciCloseAllButThis = new EFPCommandItem("View", "CloseAllButThis");
        ciCloseAllButThis.MenuText = "Закрыть все дополнительные вкладки, кроме текущей";
        ciCloseAllButThis.Click += new EventHandler(ciCloseAllButThis_Click);
        Add(ciCloseAllButThis);

        #endregion
      }

      private EFPReportPage _ReportPage;

      #endregion

      #region Команды закрытия вкладок

      private EFPCommandItem ciClose, ciCloseAllButThis;

      void ciClose_Click(object sender, EventArgs args)
      {
#if DEBUG
        if (_ReportPage.OwnerReport == null)
          throw new BugException("ReportPage.OwnerReport==null");
#endif
        _ReportPage.OwnerReport.Pages.Remove(_ReportPage);
      }

      void ciCloseAllButThis_Click(object sender, EventArgs args)
      {
#if DEBUG
        if (_ReportPage.OwnerReport == null)
          throw new BugException("ReportPage.OwnerReport==null");
#endif
        _ReportPage.OwnerReport.Pages.ClearExtraPagesExceptOne(_ReportPage);
      }

      #endregion
    }

    /// <summary>
    /// Команды локального меню для дополнительных вкладок
    /// </summary>
    private EFPReportPageCommandItems _CommandItems;

    #endregion

    #region Остальные свойства

    /// <summary>
    /// Текст закладки
    /// </summary>
    public virtual string Title
    {
      get { return _Title; }
      set
      {
        _Title = value;
        if (_TabPage != null)
          _TabPage.Text = value;
      }
    }
    private string _Title;

    /// <summary>
    /// Возвращает заголовок с учетом родительских закладок
    /// </summary>
    public string WholeTitle
    {
      get
      {
        if (ParentPage == null)
          return Title;
        else
          return ParentPage.WholeTitle + " | " + Title;
      }
    }

    /// <summary>
    /// Имя изображения для закладки (необязательное)
    /// </summary>
    public virtual string ImageKey
    {
      get { return _ImageKey; }
      set
      {
        if (value == _ImageKey)
          return;
        _ImageKey = value;
        if (_TabPage != null)
        {
          //_TabPage.ImageKey = value;
          if (String.IsNullOrEmpty(value))
            _TabPage.ImageIndex = -1;
          else
            _TabPage.ImageIndex = EFPApp.MainImages.ImageList.Images.IndexOfKey(value); // 27.09.2022
        }
        if (ParentPage != null)
          ParentPage.OnChildImageKeyChanged();
      }
    }
    private string _ImageKey;

    /// <summary>
    /// Установка свойства ImageKey
    /// </summary>
    /// <param name="kind"></param>
    public void InitStateImageKey(EFPDataGridViewImageKind kind)
    {
      ImageKey = EFPDataGridView.GetImageKey(kind, "Ok");
    }

    /// <summary>
    /// Установка свойства ImageKey
    /// </summary>
    /// <param name="kind"></param>
    public void InitStateImageKey(ErrorMessageKind kind)
    {
      ImageKey = EFPApp.GetErrorImageKey(kind);
    }

    /// <summary>
    /// Подсказка для закладки
    /// </summary>
    public virtual string ToolTipText
    {
      get { return _ToolTipText; }
      set
      {
        _ToolTipText = value;
        if (_TabPage != null)
          _TabPage.ToolTipText = value;
      }
    }
    private string _ToolTipText;

    /// <summary>
    /// Установка свойства ToolTipText на основании свойства FilterItems
    /// Свойство FilterInfo.ToolTipText
    /// </summary>
    public void ToolTipTextFromFilters()
    {
      ToolTipText = FilterInfo.ToolTipText;
    }

    /// <summary>
    /// Контекст справки для страницы отчета
    /// </summary>
    public virtual string HelpContext
    {
      get { return _HelpContext; }
      set { _HelpContext = value; }
    }
    private string _HelpContext;

    /// <summary>
    /// Закладка страничного просмотра или null, если есть только одна страница 
    /// просмотра
    /// </summary>
    internal TabPage TabPage
    {
      get { return _TabPage; }
      set
      {
        _TabPage = value;
        if (value != null)
        {
          _TabPage.Text = Title;
          _TabPage.ToolTipText = ToolTipText;

          //_TabPage.ImageKey = _ImageKey;
          // 27.09.2022. Почему-то установка свойства TabPage.ImageKey перестала работать, если страница еще не присоединена к просмотру
          // Когда испортилось - непонятно
          if (String.IsNullOrEmpty(_ImageKey))
            _TabPage.ImageIndex = -1;
          else
            _TabPage.ImageIndex = EFPApp.MainImages.ImageList.Images.IndexOfKey(_ImageKey); 
        }
      }
    }
    private TabPage _TabPage;

    /// <summary>
    /// Это событие вызывается каждый раз, когда страница отчета становится активной
    /// Обработчик, например, может выполнить расчеты, которые зависят от других
    /// страниц отчета
    /// </summary>
    public event EventHandler PageSelected;

    internal void OnPageSelected()
    {
      try
      {
        // 17.07.2013
        // TODO:
        //if (OwnerReport.IsDisposed)
        //  return;

        DataReady = true;
        if (PageSelected != null)
          PageSelected(this, EventArgs.Empty);

        // 19.02.2015
        // Рекурсивная установка свойства
        EFPReportTabControlPage thisTabControlPage = this as EFPReportTabControlPage;
        if (thisTabControlPage != null)
        {
          if (thisTabControlPage.SelectedPage != null)
            thisTabControlPage.SelectedPage.OnPageSelected();
        }
        else
        {
          //int n = ParentControl.Controls.Count;
          ParentControl.SelectNextControl(null, true, true, true, false); // 31.08.2017 Активируем дочерний элемент
        }
      }
      catch (Exception e)
      {
        if (OwnerReport != null)
          OwnerReport.AddExceptionInfo(e);
        EFPApp.ShowException(e, "Ошибка при активации закладки \"" + WholeTitle + "\"");
      }
    }

    /// <summary>
    /// Возвращает true, если эта страница является дополнительной
    /// </summary>
    public bool IsExtraPage
    {
      get { return !String.IsNullOrEmpty(_ExtraPageKey); }
    }

    /// <summary>
    /// Содержит ключ, если эта страница является дополнительной.
    /// Чтобы добавить в отчет дополнительную страницу, следует установлить это свойство перед вызовом EFPReport.Pages.Add()
    /// </summary>
    public string ExtraPageKey
    {
      get { return _ExtraPageKey; }
      set
      {
        CheckNotAdded();
        if (value == null)
          value = String.Empty;
        _ExtraPageKey = value;
        if (!String.IsNullOrEmpty(value))
          _CommandItems = new EFPReportPageCommandItems(this);
      }
    }
    private string _ExtraPageKey;

    /// <summary>
    /// Дополнительные фильтры, которые будут выведены вверху страницы отчета
    /// Допускается динамическая установка массива
    /// </summary>
    public EFPReportPageFilterItems FilterInfo { get { return _FilterInfo; } }
    private EFPReportPageFilterItems _FilterInfo;

    /// <summary>
    /// Дополнительная панель в верхней части страницы отчета (ниже строк фильтров)
    /// Чтобы панель появилась, необходимо присвоить значение до того, как 
    /// страница отчета будет присоединена к отчету
    /// </summary>
    public Control AuxTopPanel
    {
      get { return _AuxTopPanel; }
      set
      {
        CheckPageNotCreated();
        if (_AuxTopPanel != null)
          throw new InvalidOperationException("Свойство AuxTopPanel уже было установлено");
        _AuxTopPanel = value;
      }
    }
    private Control _AuxTopPanel;

    /// <summary>
    /// Дополнительная панель в нижней части отчета
    /// Чтобы панель появилась, необходимо присвоить значение до того, как 
    /// страница отчета будет присоединена к отчету
    /// </summary>
    public Control AuxBottomPanel
    {
      get { return _AuxBottomPanel; }
      set
      {
        CheckPageNotCreated();
        if (_AuxBottomPanel != null)
          throw new InvalidOperationException("Свойство AuxBottomPanel уже было установлено");
        _AuxBottomPanel = value;
      }
    }
    private Control _AuxBottomPanel;

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s = Title;
      if (IsExtraPage)
        s += " (дополнительная страница)";
      if (!String.IsNullOrEmpty(Name))
        s += ", Name=" + Name;
      return GetType().Name + ": " + s;
    }

    /// <summary>
    /// Родительская страница с закладками или null, если страница присоединена 
    /// непросредственно к отчету (страница вернего уровня) или вообще не присоединена
    /// </summary>
    public EFPReportTabControlPage ParentPage
    {
      get { return _ParentPage; }
      internal set { _ParentPage = value; }
    }
    private EFPReportTabControlPage _ParentPage;

    /// <summary>
    /// Отчет - владелец.
    /// Пока страница не присоединена к отчету (прямо или косвенно через GridReportTabControlPage),
    /// возвращает null
    /// </summary>
    public EFPReport OwnerReport
    {
      get
      {
        EFPBaseProvider baseProvider = BaseProvider.Parent;
        while (baseProvider != null)
        {
          if (baseProvider is EFPReport.EFPReportRootProvider)
            return ((EFPReport.EFPReportRootProvider)baseProvider).OwnerReport;
          baseProvider = baseProvider.Parent;
        }
        return null;
      }
    }

    /// <summary>
    /// Проверяет, что страница еще не присоединена к отчету.
    /// В противном случае выбрасывается исключение InvalidOperationException
    /// </summary>
    internal protected void CheckNotAdded()
    {
      if (OwnerReport != null)
        throw new InvalidOperationException("Страница уже присоединена к отчету");
    }

    /// <summary>
    /// Имя секции конфигурации, используемой при сохранении композиции окон между сеансами работы программы.
    /// Если свойство не установлено в явном виде, для основных страниц отчета используются имена "Page1", "Page2", ...,
    /// а для дополнительных страниц композиция не сохраняется.
    /// Если в приложении предусмотрено восстановление интерфейса при загрузке программы, рекомендуется
    /// устанавливать свойство. Это обеспечит правильную загрузку при обновлении программы, если в отчет
    /// добавляются новые страницы.
    /// </summary>
    public string Name
    {
      get { return _Name; }
      set
      {
        CheckPageNotCreated();
        _Name = value;
      }
    }
    private string _Name;

    #endregion

    #region Создание страницы

    /// <summary>
    /// Этот метод вызывается внутри системы построения отчета и не должен вызываться
    /// прикладными модулями
    /// Запоминает родительский управляющий элемент (панель), на которой будет 
    /// создана страница отчета. Сама страница создается при первом обращении к
    /// методу SetPageCreated().
    /// Метод может быть переопределен в классе-наследнике для немедленного создания
    /// страницы (используется GridReportControlPage)
    /// </summary>
    /// <param name="parentControl">Пустая панель, на которой будет расположена страница</param>
    public virtual void AssignParentControl(Panel parentControl)
    {
#if DEBUG
      if (parentControl == null)
        throw new ArgumentNullException("parentControl");
      if (parentControl.HasChildren)
        throw new ArgumentException("Родительский элемент уже имеет присоединенные управляющие элементы", "parentControl");
      if (parentControl.FindForm() == null)
        throw new ArgumentException("Родительский элемент не присоединен к форме", "parentControl");
      if (_ParentControl != null)
        throw new InvalidOperationException("Повторный вызов AssignParentControl()");
#endif
      _ParentControl = parentControl;
    }

    /// <summary>
    /// Свойство возвращает true, если страница отчета была создана.
    /// Однако, это не означает, что были присоединены данные. Для проверки данных
    /// следует использовать свойство DataReady.
    /// Для установки свойство можно использовать метод SetPageCreated()
    /// </summary>
    public bool PageCreated
    {
      get
      {
        if (_ParentControl == null)
          return false;
        else
          return _ParentControl.HasChildren;
      }
    }

    internal Panel ParentControl { get { return _ParentControl; } }
    private Panel _ParentControl;

    /// <summary>
    /// Этот метод вызывается внутри системы построения отчета и обычно не должен
    /// использоваться в прикладном модуле. Однако он может быть вызван, если
    /// страница должна быть обязательно создана
    /// Допускается вложенный вызов метода из обработчика события активации страницы
    /// </summary>
    public void SetPageCreated()
    {
#if DEBUG
      if (_ParentControl == null)
        throw new InvalidOperationException("Нельзя вызывать SetPageCreated() до AssignParentControl()");
#endif

      if (_ParentControl.HasChildren)
        return; // уже создан

      Panel panel2 = new Panel();
      panel2.Dock = DockStyle.Fill;
      panel2.TabIndex = 2;
      _ParentControl.Controls.Add(panel2);
      // Вызываем виртуальный метод
      CreatePage(panel2);

      // Присоединяем дополнительные панели
      if (AuxTopPanel != null)
      {
        AuxTopPanel.Dock = DockStyle.Top;
        AuxTopPanel.TabIndex = 1;
        _ParentControl.Controls.Add(AuxTopPanel);
      }

      if (AuxBottomPanel != null)
      {
        AuxBottomPanel.Dock = DockStyle.Bottom;
        AuxBottomPanel.TabIndex = 3;
        _ParentControl.Controls.Add(AuxBottomPanel);
      }

      // Создаем локальную таблицу фильтров
      DataGridView ghFilt = new DataGridView();
      ghFilt.Dock = DockStyle.Top;
      ghFilt.TabIndex = 0;
      _ParentControl.Controls.Add(ghFilt);
      _TheFilterGridProvider = new EFPReportFilterGridView(_BaseProvider, ghFilt);
      DoSetFilterItems();
    }

    /// <summary>
    /// Проверяет, что страница еще не была создана (PageCreated=false).
    /// В противном случае выбрасывается исключение InvalidOperationException
    /// </summary>
    protected void CheckPageNotCreated()
    {
      if (PageCreated)
        throw new InvalidOperationException("Страница отчета уже была создана");
    }

    /// <summary>
    /// Рекурсивная инициализация страниц отчета.
    /// Для обычных страниц вызывает SetPageCreated().
    /// Для страниц EFPReportTabControlPage() рекурсивно вызывает себя для каждой вложенной страницы отчета.
    /// Этот метод обычно не должен вызываться из пользовательского кода.
    /// </summary>
    public virtual void SetAllPagesCreated()
    {
      SetPageCreated();
    }

    private EFPReportFilterGridView _TheFilterGridProvider;

    internal void DoSetFilterItems()
    {
      if (_TheFilterGridProvider == null)
        return;
      _TheFilterGridProvider.Filters = FilterInfo.ToArray();
    }

    /// <summary>
    /// Устанавливает свойство Control.Name для фиксированной страницы отчета.
    /// Этот метод должен вызываться производным классом из CreatePage()
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента, добавляемого на страницу</param>
    /// <param name="nameSuffix">Суффикс имени управляющего элемента</param>
    protected void SetControlName(EFPControlBase controlProvider, string nameSuffix)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");
      if (String.IsNullOrEmpty(nameSuffix))
        throw new ArgumentNullException("nameSuffix");

      if (OwnerReport == null)
        throw new InvalidOperationException("Страница не присоединена к отчету");

      string prefix;

      if (String.IsNullOrEmpty(Name))
      {
        if (ParentPage == null)
        {
          // Страница отчета верхнего уровня
          if (this.IsExtraPage)
            return; // для дополнительных страниц не устанавливаем
          int p = OwnerReport.Pages.IndexOf(this);
          if (p < 0)
            throw new BugException();
          prefix = "Page" + (p + 1).ToString();
        }
        else
        {
          int p = ParentPage.Pages.IndexOf(this);
          if (p < 0)
            throw new BugException();
          ParentPage.SetControlName(controlProvider, "Page" + (p + 1).ToString() + "_" + nameSuffix);
          return;
        }
      }
      else
        prefix = Name;

      controlProvider.Control.Name = prefix + "_" + nameSuffix;
    }

    #endregion

    #region Готовность данных

    /// <summary>
    /// Готовность данных. Это свойство устанавливается в true, когда активируется
    /// страница отчета. Свойство сбрасывается в false, когда пользователь обновляет
    /// отчет нажатием клавиши F5
    /// </summary>
    public bool DataReady
    {
      get { return _DataReady; }
      set
      {
        if (value == _DataReady)
          return;
        if (value)
        {
          // Страница должна быть создана
          SetPageCreated();

          if (DataQuery != null)
            DataQuery(this, EventArgs.Empty);
          InitData();
        }
        else
          InvalidateData();
        _DataReady = value;
      }
    }
    private bool _DataReady;

    /// <summary>
    /// Событие вызывается перед InitData(). Обработчик может инициализировать 
    /// данные. Событие может вызываться несколько раз при выполнении Refresh
    /// </summary>
    public event EventHandler DataQuery;

    /// <summary>
    /// Активация вкладки отчета.
    /// Если страница расположена на GridReportTabControlPage, то выполняется
    /// рекурсивный вызов.
    /// При необходимости, выполняется установка свойства DataReady
    /// </summary>
    public void Select()
    {
#if DEBUG
      if (OwnerReport == null)
        throw new BugException("OwnerReport==null");
#endif
      if (OwnerReport.ReportForm == null)
        return;
      if (OwnerReport.ReportForm.WindowState == FormWindowState.Minimized)
        OwnerReport.ReportForm.WindowState = FormWindowState.Normal; // 21.09.2017
      OwnerReport.ReportForm.Activate(); // 24.10.2016

      if (ParentPage != null)
      {
        ParentPage.Select();
        ParentPage.SelectedPage = this;
      }
      else
        OwnerReport.SelectedPage = this;
    }

    #endregion

    #region Абстрактные методы

    /// <summary>
    /// Создание управляющих элементов. Этот метод вызывается однократно при первом
    /// обращении к странице
    /// </summary>
    /// <param name="parent">Родительский элемент, куда добавляются элементы</param>
    protected abstract void CreatePage(Panel parent);

    /// <summary>
    /// Инициализировать страницу после построения отчета. Этот метод может вызываться
    /// несколько раз при выполнении Refresh
    /// </summary>
    protected virtual void InitData()
    {
    }

    /// <summary>
    /// Этот метод вызывается при обновлении отчета нажатием клавиши F5 при сбросе
    /// свойства DataReady в false
    /// </summary>
    protected virtual void InvalidateData()
    {
    }

    /// <summary>
    /// Запоминает конфигурацию табличного просмотра
    /// Метод должен вернуть true, если сохранение успешно выполнено и false,
    /// если возникла ошибка и закрывать форму отчета нельзя
    /// </summary>
    /// <param name="cfg">Заполняемая секция конфигурации</param>
    /// <returns>Обычно следует возвращать true.</returns>
    public virtual bool SaveViewConfig(CfgPart cfg)
    {
      return true;
    }

    #endregion

    #region IEFPTabPageControl Members

    string IEFPTabPageControl.Text
    {
      get { return Title; }
      set { Title = value; }
    }

    #endregion

    #region Печать страницы

    /// <summary>
    /// Инициализация заголовка и таблички фильтров для печати страницы отчета
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    protected void DefaultOutItem_TitleNeeded(object sender, EventArgs args)
    {
      Reporting.BRDataViewMenuOutItemBase outItem = (Reporting.BRDataViewMenuOutItemBase)sender;

      outItem.Title = OwnerReport.ReportParams.Title;
      if (OwnerReport.Pages.Count > 1 || OwnerReport.AlwaysShowPages)
        outItem.Title += " - " + this.Title;

      outItem.FilterInfo.Clear();
      foreach (EFPReportFilterItem filter in OwnerReport.ReportParams.FilterInfo)
        outItem.FilterInfo.Add(filter.DisplayName, filter.Value);
      foreach (EFPReportFilterItem filter in this.FilterInfo)
        outItem.FilterInfo.Add(filter.DisplayName, filter.Value);
    }

    #endregion
  }

  #region Интерфейс IEFPReportPages

  /// <summary>
  /// Интерфейс для добавления страниц, общий для EFPReportPages и
  /// EFPReportTabControlPage.
  /// Дополняет обобщенный интерфейс ICollection некоторыми методами из класса List
  /// </summary>
  public interface IEFPReportPages : ICollection<EFPReportPage>
  {
    /// <summary>
    /// Доступ к странице по индексу
    /// </summary>
    /// <param name="index">Индекс страницы в пределах набора страниц отчета верхнего уровня
    /// или дочерних страниц в EFPReportTabControlPage</param>
    /// <returns></returns>
    EFPReportPage this[int index] { get; }

    /// <summary>
    /// Поиск страницы в группе.
    /// Рекурсивный поиск не выполняется
    /// </summary>
    /// <param name="page">Искомая страница</param>
    /// <returns>Индекс страницы или (-1)</returns>
    int IndexOf(EFPReportPage page);

    /// <summary>
    /// Получить список страниц в виде массива
    /// </summary>
    /// <returns></returns>
    EFPReportPage[] ToArray();
  }

  #endregion

  /// <summary>
  /// Закладка, содержащая вложенный объект с закладками, куда могут добавляться
  /// другие страницы
  /// </summary>
  public class EFPReportTabControlPage : EFPReportPage
  {
    #region Конструктор

    /// <summary>
    /// Создает страницу отчета для добавления вложенных страниц
    /// </summary>
    public EFPReportTabControlPage()
    {
      _Pages = new PageCollection(this);
      _Alignment = TabAlignment.Top;
      _SelectedPageIndex = 0;
    }

    #endregion

    #region Список страниц

    /// <summary>
    /// Коллекция страниц в EFPReportTabControlPage
    /// </summary>
    public sealed class PageCollection : IEFPReportPages
    {
      #region Конструктор

      internal PageCollection(EFPReportTabControlPage owner)
      {
        _Owner = owner;
        _Items = new List<EFPReportPage>();
      }

      #endregion

      #region Коллекция страниц

      /// <summary>
      /// Доступ к странице по индексу
      /// </summary>
      /// <param name="index">Индекс страницы в диапазоне от 0 до Count-1</param>
      /// <returns>Дочерняя страница</returns>
      public EFPReportPage this[int index] { get { return _Items[index]; } }
      private List<EFPReportPage> _Items;

      /// <summary>
      /// Возвращает количество дочерних страниц
      /// </summary>
      public int Count { get { return _Items.Count; } }

      /// <summary>
      /// Возвращает индекс дочерней страницы в диапазоне от 0 до Count-1.
      /// Возвращает (-1), если страница не найдена
      /// </summary>
      /// <param name="page">Дочерняя страница для поиска</param>
      /// <returns>Индекс страницы</returns>
      public int IndexOf(EFPReportPage page)
      {
        return _Items.IndexOf(page);
      }

      /// <summary>
      /// Возвращает true, если дочерняя страница принадлежит этой странице.
      /// Этот метод не является рекурсивным. Определяется только принадлежность к этой странице, а не к какой-либо из дочерних страниц
      /// </summary>
      /// <param name="page"></param>
      /// <returns></returns>
      public bool Contains(EFPReportPage page)
      {
        return _Items.Contains(page);
      }

      /// <summary>
      /// Реализация метода интерфейса ICollection
      /// </summary>
      /// <param name="array"></param>
      /// <param name="arrayIndex"></param>
      public void CopyTo(EFPReportPage[] array, int arrayIndex)
      {
        _Items.CopyTo(array, arrayIndex);
      }

      /// <summary>
      /// Создает массив всех дочерних страниц
      /// </summary>
      /// <returns>Массив страниц</returns>
      public EFPReportPage[] ToArray()
      {
        return _Items.ToArray();
      }

      bool ICollection<EFPReportPage>.IsReadOnly { get { return false; } }

      /// <summary>
      /// Добавление странички
      /// </summary>
      /// <param name="page">Добавляемая страница</param>
      public void Add(EFPReportPage page)
      {
        if (page == null)
          throw new ArgumentNullException("page");

        if (page.ParentPage != null)
          throw new InvalidOperationException("Добавляемая страница уже принадлежит другому просмотру с закладками");

        _Items.Add(page);
        page.BaseProvider.Parent = _Owner.BaseProvider;
        page.ParentPage = _Owner;

        if (_Owner._TheTabControl != null)
        {
          _Owner.DoCreatePage(_Items.Count - 1);
          _Owner.SelectedPageIndex = _Items.Count - 1;
          if (_Items.Count == 1)
            // Для первой добавленной закладки всегда посылаем извещение
            _Owner.TheTabControl_SelectedIndexChanged(null, null);
        }

        _Owner.OnChildImageKeyChanged();
      }

      /// <summary>
      /// Удаляет дочернюю страницу по индексу
      /// </summary>
      /// <param name="index">Индекс страницы в диапазоне от 0 до Count-1</param>
      public void RemoveAt(int index)
      {
        if (index >= 0 && index < _Items.Count)
        {
          _Items[index].BaseProvider.Parent = null; // 06.07.2021
          if (_Owner._TheTabControl != null)
            _Owner._TheTabControl.TabPages.RemoveAt(index);
          _Items[index].ParentPage = null;
          _Items.RemoveAt(index);

          _Owner.OnChildImageKeyChanged();
        }
      }

      /// <summary>
      /// Удалить все страницы, оставив только Count страниц
      /// </summary>
      /// <param name="count">Количество страниц, которое должно остаться</param>
      public void RemoveAtEnd(int count)
      {
        if (count < 0)
          throw new ArgumentException();

        for (int i2 = _Items.Count - 1; i2 >= count; i2--)
          _Items[i2].BaseProvider.Parent = null; // 06.07.2021

        if (_Owner._TheTabControl != null)
        {
          for (int i = _Owner._TheTabControl.TabCount - 1; i >= count; i--)
            _Owner._TheTabControl.TabPages.RemoveAt(i);
        }
        for (int i2 = _Items.Count - 1; i2 >= count; i2--)
        {
          _Items[i2].ParentPage = null;
          _Items.RemoveAt(i2);
        }
        _Owner.OnChildImageKeyChanged();
      }

      /// <summary>
      /// Удаляет страницу
      /// </summary>
      /// <param name="page">Удаляемая страница</param>
      /// <returns>true, если страница была удалена, false, если страница не является дочерней для данной страницы</returns>
      public bool Remove(EFPReportPage page)
      {
        int index = IndexOf(page);
        if (index < 0)
          return false;
        else
        {
          RemoveAt(index);
          return true;
        }
      }

      /// <summary>
      /// Удаляет все дочерние страницы
      /// </summary>
      public void Clear()
      {
        RemoveAtEnd(0);
      }

      #endregion

      #region Прочее

      private EFPReportTabControlPage _Owner;

      #endregion

      #region IEnumerable<GridReportPage> Members

      /// <summary>
      /// Возвращает нерекурсивный перечислитель по дочерним страницам.
      /// </summary>
      /// <returns>Перечислитель</returns>
      public IEnumerator<EFPReportPage> GetEnumerator()
      {
        return _Items.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return _Items.GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Список страниц
    /// </summary>
    public PageCollection Pages { get { return _Pages; } }
    private PageCollection _Pages;

    #endregion

    #region Прочее

    /// <summary>
    /// Окно с закладками
    /// </summary>
    private TabControl _TheTabControl;

    /// <summary>
    /// Размещение закладок (по умолчанию - сверху)
    /// </summary>
    public TabAlignment Alignment
    {
      get { return _Alignment; }
      set
      {
        if (_TheTabControl != null)
          _TheTabControl.Alignment = Alignment;
        _Alignment = value;
      }
    }
    private TabAlignment _Alignment;

    /// <summary>
    /// Индекс текущей выбранной дочерней страницы
    /// </summary>
    public int SelectedPageIndex
    {
      get
      {
        if (_TheTabControl == null)
          return _SelectedPageIndex;
        else
          return _TheTabControl.SelectedIndex;
      }
      set
      {
        if (_TheTabControl == null)
          _SelectedPageIndex = value;
        else
          _TheTabControl.SelectedIndex = value;
      }
    }
    private int _SelectedPageIndex;

    /// <summary>
    /// Текущая выбранная дочерняя страница
    /// </summary>
    public EFPReportPage SelectedPage
    {
      get
      {
        if (SelectedPageIndex < 0 || SelectedPageIndex >= Pages.Count)
          return null;
        else
          return Pages[SelectedPageIndex];
      }
      set
      {
        SelectedPageIndex = Pages.IndexOf(value);
      }
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Создает элемент TabControl и добавляет ранее созданные вложенные страницы
    /// </summary>
    /// <param name="parent">Панель для добавления TabControl</param>
    protected override void CreatePage(Panel parent)
    {
      _TheTabControl = new TabControl();
      _TheTabControl.Dock = DockStyle.Fill;
      parent.Controls.Add(_TheTabControl);
      _TheTabControl.Alignment = _Alignment;
      _TheTabControl.ShowToolTips = true;
      _TheTabControl.ImageList = EFPApp.MainImages.ImageList;

      // Инициализируем существующие страницы
      for (int i = 0; i < Pages.Count; i++)
        DoCreatePage(i);
      // Активируем страницу
      if (Pages.Count > 0)
      {
        if (_SelectedPageIndex < 0 || _SelectedPageIndex >= Pages.Count)
          _SelectedPageIndex = 0;
        _TheTabControl.SelectedIndex = _SelectedPageIndex;
      }

      _TheTabControl.SelectedIndexChanged += new EventHandler(TheTabControl_SelectedIndexChanged);
      // TabControl не посылает извещения IndexChanged при начальной установке
      TheTabControl_SelectedIndexChanged(null, null);
    }

    private void DoCreatePage(int pageIndex)
    {
      EFPReportPage page = Pages[pageIndex];
      TabPage newPage = new TabPage();
      page.TabPage = newPage;
      _TheTabControl.TabPages.Add(newPage);
      page.AssignParentControl(newPage);
    }

    void TheTabControl_SelectedIndexChanged(object sender, EventArgs args)
    {
      if (_TheTabControl.SelectedIndex >= 0 && _TheTabControl.SelectedIndex < Pages.Count)
        Pages[_TheTabControl.SelectedIndex].OnPageSelected();
    }

    /// <summary>
    /// Устанавливает для активной дочерней страницы свойство DataReady=true.
    /// </summary>
    protected override void InitData()
    {
      base.InitData();
      if (_TheTabControl.SelectedIndex >= 0 && _TheTabControl.SelectedIndex < Pages.Count)
        Pages[_TheTabControl.SelectedIndex].DataReady = true;
    }

    /// <summary>
    /// Устанавливает для всех дочерних страниц свойство DataReady=false.
    /// </summary>
    protected override void InvalidateData()
    {
      base.InvalidateData();
      for (int i = 0; i < Pages.Count; i++)
        Pages[i].DataReady = false;
    }

    /// <summary>
    /// Запоминает конфигурацию табличного просмотра.
    /// Вызывает SaveViewConfig() для всех дочерних страниц.
    /// Возвращает true, если все дочерние страницы успешно сохранили конфигурацию.
    /// </summary>
    /// <param name="cfg">Секция конфигурации для записи</param>
    /// <returns>Признак завершения</returns>
    public override bool SaveViewConfig(CfgPart cfg)
    {
      if (!base.SaveViewConfig(cfg))
        return false;
      for (int i = 0; i < Pages.Count; i++)
      {
        if (!Pages[i].SaveViewConfig(cfg))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Рекурсивная инициализация страниц отчета.
    /// Метод вызывает себя для каждой вложенной страницы отчета.
    /// Этот метод обычно не должен вызываться из пользовательского кода.
    /// </summary>
    public override void SetAllPagesCreated()
    {
      base.SetAllPagesCreated();

      for (int i = 0; i < Pages.Count; i++)
        Pages[i].SetAllPagesCreated();
    }

    #endregion

    #region ChildImageKeyChanged и AutoInitStateImageKey

    /// <summary>
    /// Событие вызывается, когда у любой из дочерних закладок устанавливается
    /// свойство ImageKey или после добавления или удаления закладки
    /// </summary>
    public event EventHandler ChildImageKeyChanged;

    /// <summary>
    /// Метод вызывается, когда у любой из дочерних закладок устанавливается
    /// свойство ImageKey или после добавления или удаления закладки.
    /// Если свойство AutoInitStateImageKey установлено, то устанавливается свойство ImageKey
    /// для текущей вкладки отчета на основании значков дочерних вкладок.
    /// Затем вызывается обработчик события ChildImageKeyChanged, если он установлен.
    /// </summary>
    public virtual void OnChildImageKeyChanged()
    {
      if (_AutoInitStateImageKey)
      {
        EFPDataGridViewImageKind kind2 = EFPDataGridViewImageKind.None;
        bool hasUnknownState = false;
        for (int i = 0; i < Pages.Count; i++)
        {
          EFPDataGridViewImageKind Kind1;
          switch (Pages[i].ImageKey)
          {
            case "Error": Kind1 = EFPDataGridViewImageKind.Error; break;
            case "Warning": Kind1 = EFPDataGridViewImageKind.Warning; break;
            case "Information": Kind1 = EFPDataGridViewImageKind.Information; break;
            case "UnknownState":
              hasUnknownState = true;
              continue;
            default:
              continue;
          }
          if (Kind1 > kind2)
            kind2 = Kind1;
        }

        if (kind2 == EFPDataGridViewImageKind.None && hasUnknownState)
          base.ImageKey = "UnknownState";
        else
          base.InitStateImageKey(kind2);
      }

      if (ChildImageKeyChanged != null)
        ChildImageKeyChanged(this, EventArgs.Empty);
    }

    /// <summary>
    /// Если свойство установлено, то выполняется автоматическое управление значком
    /// вкладки (свойство ImageKey), в зависимости от наличия во вложенных вкладках
    /// значков ошибкок и предупреждений. Выбирается максимально "серьзное" значение
    /// свойства ImageKey во вложенных вкладках. Если вкладка имеет ImageKey, отличное
    /// от "OK", "Инфо", "Предупреждение" или "Ошибка", считается, что данная страница
    /// не имеет ошибок и не участвует в формировании значка 
    /// Установка свойства заставляет отчет обеспечить готовность данных на вложенных 
    /// страницах немедленно после построения отчета, а не при первом обращении 
    /// пользователя к странице, что может привести к замедлению построения
    /// По умолчанию свойство не установлено
    /// Если свойство установлено, то ручное задание свойства ImageKey не требуется
    /// </summary>
    public bool AutoInitStateImageKey
    {
      get { return _AutoInitStateImageKey; }
      set
      {
        _AutoInitStateImageKey = value;
        if (value)
          OnChildImageKeyChanged();
      }
    }
    private bool _AutoInitStateImageKey;


    #endregion
  }

#if XXX
  /// <summary>
  /// Закладка, содержащая предварительный просмотр изображения
  /// </summary>
  public class EFPReportPreviewPage : EFPReportPage
  {
  #region Конструктор

    public EFPReportPreviewPage()
    {
      FPreview = new AccDepPrintPreview();
    }

  #endregion

  #region Свойства

    /// <summary>
    /// Объект просмотра. Устанавливайте свойство Preview.PageSetup для присвоения
    /// задания на печать
    /// </summary>
    public AccDepPrintPreview Preview { get { return FPreview; } }
    private AccDepPrintPreview FPreview;

  #endregion

  #region Переопределенные методы

    protected override void CreatePage(Panel Parent)
    {
      Preview.InitControl(BaseProvider, Parent, false);
    }

  #endregion
  }
#endif

  /// <summary>
  /// Команды меню, относящиеся к отчету в целом
  /// </summary>
  public class EFPReportCommandItems : EFPCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создание списка команд меню
    /// </summary>
    /// <param name="owner">Отчет</param>
    internal EFPReportCommandItems(EFPReport owner)
    {
      _Owner = owner;

      ciCloseAll = new EFPCommandItem("View", "CloseAllTabs");
      ciCloseAll.MenuText = "Закрыть все дополнительные вкладки";
      ciCloseAll.Click += new EventHandler(ciCloseAll_Click);
      ciCloseAll.GroupEnd = true;
      Add(ciCloseAll);

      ciRefresh = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Refresh);
      ciRefresh.Enabled = true;
      ciRefresh.MenuText = "Обновить отчет без изменения параметров";
      ciRefresh.Click += new EventHandler(ciRefresh_Click);
      ciRefresh.GroupBegin = true;
      ciRefresh.GroupEnd = true;
      Add(ciRefresh);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Отчет, к которому относятся команды
    /// </summary>
    public EFPReport Owner { get { return _Owner; } }
    private EFPReport _Owner;

    #endregion

    #region Команды закрытия вкладок

    EFPCommandItem ciCloseAll;

    void ciCloseAll_Click(object sender, EventArgs args)
    {
      _Owner.Pages.ClearExtraPages();
    }

    internal void InitCloseCommands()
    {
      //ciCloseAll.Visible = FOwner.ExtraPagesHappened;
      //ciCloseAll.Enabled = FOwner.ExtraPageCount > 0;
      ciCloseAll.Visible = _Owner.Pages.ExtraPageCount > 0;
    }

    #endregion

    #region Команды обновления отчета

    private EFPCommandItem ciRefresh;

    void ciRefresh_Click(object sender, EventArgs args)
    {
      if (Owner.InsideBuildReport)
      {
        EFPApp.ShowTempMessage("Построение отчета уже начато");
        return;
      }
      EFPApp.BeginWait("Повторное построение отчета", Owner.MainImageKey);
      try
      {
        ciRefresh.Enabled = false;
        Owner.RefreshReport();
      }
      finally
      {
        EFPApp.EndWait();
        ciRefresh.Enabled = true;
      }
    }

    #endregion
  }
}
