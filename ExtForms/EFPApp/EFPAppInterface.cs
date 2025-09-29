// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using System.Drawing;
using FreeLibSet.Logging;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Объект поддержки интерфейса пользователя.
  /// Базовый класс для EFPAppInterfaceMDI и EFPAppInterfaceSDI, которые реализуют разные варианты
  /// интерфейса
  /// </summary>
  public abstract class EFPAppInterface : IObjectWithCode, IEnumerable<EFPAppMainWindowLayout>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    public EFPAppInterface()
    {
      _MainWindows = new ListWithMRU<EFPAppMainWindowLayout>();
    }

    #endregion

    #region Характеристики интерфейса

    /// <summary>
    /// Тип интерфейса для сохранения настроек.
    /// Возвращает “MDI” , “SDI” или “TDI”
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Возвращает true для интерфейса SDI
    /// </summary>
    public virtual bool IsSDI { get { return false; } }

    /// <summary>
    /// Возвращает true, если используется нумерация главных окон (свойство EFPAppMainWindowLayout.MainWindowNumberText возвращает непустую строку).
    /// Для интерфейса MDI возвращает true, если открыто больше одного главного окна.
    /// Для интерфейса SDI всегда возвращает false
    /// </summary>
    public virtual bool MainWindowNumberUsed
    {
      get { return MainWindowCount > 1; }
    }

    /// <summary>
    /// Возвращает свойство Name
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      return Name;
    }

    /// <summary>
    /// Флажок для старого варианта инициализации
    /// </summary>
    internal bool ObsoleteMode;

    #endregion

    #region Запуск и остановка интерфейса

    /// <summary>
    /// Вызывается при присоединении интерфейса к свойcтву EFPApp.Interface
    /// </summary>
    internal protected virtual void Attach()
    {
    }

    /// <summary>
    /// Вызывается при отсоединении интерфейса к свойcтву EFPApp.Interface.
    /// Закрывает все открытые окна. В случае невозможности закрытия, например, из-за отказа
    /// пользователя сохранить данные, выбрасывает исключение
    /// </summary>
    internal protected virtual void Detach()
    {
      CloseAll();
      if (MainWindowCount > 0)
        throw new CannotCloseFormException(Res.EFPApp_Err_CannotCloseAllForms);
    }

    #endregion

    #region Главное окно

    /// <summary>
    /// Возвращает список открытых главных окон.
    /// Если <paramref name="useZOrder"/>=true, то список сортируется по порядку активации (CurrentMainWindowLayout будет первым в списке).
    /// Если <paramref name="useZOrder"/>=false, то список сортируется по порядку открытия окон (#1, #2)
    /// </summary>
    /// <param name="useZOrder">Нужно ли сортировать главные окна по порядку активации</param>
    /// <returns>Объект для работы с главным окном</returns>
    public EFPAppMainWindowLayout[] GetMainWindowLayouts(bool useZOrder)
    {
      if (useZOrder)
        return _MainWindows.MRU.ToArray();
      else
        return _MainWindows.ToArray();
    }


    /// <summary>
    /// Возвращает текущее главное окно.
    /// Если есть несколько главных окон, то возвращает то окно, которое является активным или было активным в последний раз.
    /// Установка свойства активирует главное окно, выводя его на передний план.
    /// </summary>
    public EFPAppMainWindowLayout CurrentMainWindowLayout
    {
      get
      {
        return _MainWindows.MRU.First;
      }
      set
      {
        if (value == null)
          return;
        value.MainWindow.Activate();
      }
    }

    /// <summary>
    /// Количество главных окон.
    /// Для интерфейса SDI равно количеству дочерних окон плюс "пустышка", если есть
    /// </summary>
    public int MainWindowCount { get { return _MainWindows.Count; } }

    /// <summary>
    /// Создает главное окно и делает его текущим
    /// Для интерфейса SDI создает окно-«пустышку»
    /// </summary>
    /// <returns>Описание созданного главного окна</returns>
    public abstract EFPAppMainWindowLayout ShowMainWindow();

    #region Внутренний список главных окон

    /// <summary>
    /// Список с поддержкой для Z-order
    /// </summary>
    private ListWithMRU<EFPAppMainWindowLayout> _MainWindows;

    ///// <summary>
    ///// Создает EFPAppMainWindowLayout 
    ///// </summary>
    ///// <param name="MainWindow"></param>
    ///// <returns></returns>
    //protected EFPAppMainWindowLayout InternalCreateMainWindowLayout(Form MainWindow)
    //{
    //}

    private int _TotalMainWindowCount;

    /// <summary>
    /// Добавляет окно в список MainWindows
    /// Устанавливает свойство EFPAppMainWindowLayout.MainWindowю
    /// Позиционирует главное окно на экране, если IsSDI=false.
    /// </summary>
    /// <param name="layout">Описатель главного окна интерфейса</param>
    protected void AddMainWindow(EFPAppMainWindowLayout layout)
    {
      if (layout == null)
        throw new ArgumentNullException("layout");
      if (layout.Interface != null)
        throw ExceptionFactory.ArgProperty("layout", layout, "Interface", layout.Interface, new object[] { null });
      layout.Interface = this;
      if (layout.MainWindow == null)
        throw ExceptionFactory.ArgProperty("layout", layout, "MainWindow", layout.MainWindow, null);

      #region Дополнительная обработка для главного окна

      // 23.08.2013
      // Загружаем значок приложения
      WinFormsTools.InitAppIcon(layout.MainWindow);
      layout.MainWindow.KeyPreview = true;
      layout.MainWindow.KeyDown += new KeyEventHandler(EFPCommandItems.PerformKeyDown);

      if (!IsSDI)
      {
        if (EFPApp.DefaultScreen != null) // на всякий случай
        {
          int delta = SystemInformation.CaptionHeight + 2 * SystemInformation.BorderSize.Width;
          Size offset = new Size(delta * (_TotalMainWindowCount % 5), delta * (_TotalMainWindowCount % 5));

          layout.MainWindow.StartPosition = FormStartPosition.Manual;
          layout.MainWindow.Location = EFPApp.DefaultScreen.WorkingArea.Location + offset;
          layout.MainWindow.Size = new System.Drawing.Size(EFPApp.DefaultScreen.WorkingArea.Width * 2 / 3,
            EFPApp.DefaultScreen.WorkingArea.Height * 2 / 3);
        }
        else
          layout.MainWindow.StartPosition = FormStartPosition.WindowsDefaultBounds;
      }


      if (!EFPApp.InsideLoadComposition)
      {
        if (EFPApp.MainWindowDefaultMaximized && this.MainWindowCount == 0)
          layout.MainWindow.WindowState = FormWindowState.Maximized;
      }

      layout.InitCommandItems();

      layout.MainWindow.Activated += new EventHandler(layout.MainWindow_Activated);
      layout.MainWindow.FormClosing += new FormClosingEventHandler(layout.MainWindow_FormClosing);
      layout.MainWindow.FormClosed += new FormClosedEventHandler(layout.MainWindow_FormClosed);

      #endregion

      _MainWindows.Add(layout);

      InitMainWindowTitles();

      EFPApp.MainWindow = layout.MainWindow;
      _TotalMainWindowCount++;
    }

    /// <summary>
    /// Удаляет окно из списка MainWindows
    /// </summary>
    /// <param name="layout">Описатель главного окна интерфейса</param>
    internal protected void RemoveMainWindow(EFPAppMainWindowLayout layout)
    {
      if (layout == null)
        throw new ArgumentNullException("layout");
      _MainWindows.Remove(layout);

      InitMainWindowTitles();

      if (this.CurrentMainWindowLayout == null)
        EFPApp.MainWindow = null;
      else
        EFPApp.MainWindow = this.CurrentMainWindowLayout.MainWindow;
    }

    /// <summary>
    /// Обработка события Form.Activated.
    /// Переносит окно наверх списка MainWindows
    /// </summary>
    /// <param name="layout">Описатель главного окна интерфейса</param>
    internal protected void MainWindowActivated(EFPAppMainWindowLayout layout)
    {
      if (layout == null)
        throw new ArgumentNullException("layout");
      _MainWindows.Touch(layout);

      EFPApp.MainWindow = layout.MainWindow;
    }

    #endregion

    /// <summary>
    /// Инициализация заголовков главных окон.
    /// Присваивает главным окнам, если их больше 1, заголовки "Title #1", "Title #2", ...
    /// где Title берется из EFPApp.MainWindowTitle.
    /// Для SDI ничего не делает
    /// Этот метод не используется в пользовательском коде.
    /// </summary>
    internal protected virtual void InitMainWindowTitles()
    {
      EFPAppMainWindowLayout[] layouts = GetMainWindowLayouts(false);
      if (layouts.Length == 1)
      {
        layouts[0].MainWindowNumberText = String.Empty;
        layouts[0].MainWindow.Text = EFPApp.MainWindowTitle;
      }
      else
      {
        for (int i = 0; i < layouts.Length; i++)
        {
          layouts[i].MainWindowNumberText = " #" + (i + 1).ToString();
          layouts[i].MainWindow.Text = EFPApp.MainWindowTitle + " " + layouts[i].MainWindowNumberText;
        }
      }
    }

    /// <summary>
    /// Закрывает все окна: и дочерние и главные
    /// </summary>
    /// <returns>true, если удалось все закрыть</returns>
    public bool CloseAll()
    {
      if (!CloseAllChildren())
        return false;

      EFPAppMainWindowLayout[] layouts = GetMainWindowLayouts(false);
      for (int i = 0; i < layouts.Length; i++)
      {
        if (!layouts[i].CloseMainWindow())
          return false;
      }
      return true;
    }

    #endregion

    #region Дочерние окна

    #region Список форм

    /// <summary>
    /// Возвращает список открытых дочерних форм во всех главных окнах. 
    /// Если есть несколько главных окон, возвращаются дочерние окна во всех из них.
    /// Если <paramref name="useZOrder"/>=true, то используется порядок расположения окон (сначала – для главных окон, затем – для дочерних окон в пределах главного окна). CurrentChildForm будет в начале списка
    /// Если <paramref name="useZOrder"/>=false, то возвращаются окна в порядке создания (сначала – в порядке создания главных окон #1, #2; затем – окна в пределах главного окна)
    /// </summary>
    /// <param name="useZOrder">Нужно ли сортировать окна по порядку активации</param>
    /// <returns>Массив форм</returns>
    public Form[] GetChildForms(bool useZOrder)
    {
      EFPAppMainWindowLayout[] layouts = GetMainWindowLayouts(useZOrder);
      List<Form> Forms = new List<Form>();
      for (int i = 0; i < layouts.Length; i++)
      {
        Form[] a = layouts[i].GetChildForms(useZOrder);
        Forms.AddRange(a);
      }
      return Forms.ToArray();
    }

    /// <summary>
    /// Возвращает активное дочернее окно или окно, которое было активным последний раз.
    /// Установка свойства вызывает активацию главного окна и активацию дочернего окна в нем. Для интерфейса SDI просто активируется окно.
    /// </summary>
    public Form CurrentChildForm
    {
      get
      {
        if (CurrentMainWindowLayout == null)
          return null;
        else
          return CurrentMainWindowLayout.CurrentChildForm;
      }
      set
      {
        if (value == null)
          return;
        EFPApp.Activate(value); // 07.06.2021
      }
    }

    /// <summary>
    /// Возвращает общее количество дочерних окон во всех главных окнах
    /// </summary>
    public int ChildFormCount
    {
      get
      {
        int cnt = 0;
        for (int i = 0; i < _MainWindows.Count; i++)
          cnt += _MainWindows[i].ChildFormCount;
        return cnt;
      }
    }

    #endregion

    #region Показ формы

    /// <summary>
    /// Подготовка формы к показу.
    /// Если этот метод не вызван в явном виде, он вызывается автоматически из ShowChildForm().
    /// Метод вызывает OnPrepareChildForm(). Повторные вызовы предотвращаются.
    /// </summary>
    /// <param name="form">Форма</param>
    public void PrepareChildForm(Form form)
    {
      EFPFormProvider formProvider = GetChildFormProvider(form);
      if (formProvider.Prepared)
        return;
      formProvider.InitFormIconImage();
      formProvider.InternalPreparationData = OnPrepareChildForm(form);
      formProvider.Prepared = true;
    }

    private static EFPFormProvider GetChildFormProvider(Form form)
    {
      if (form == null)
        throw new ArgumentNullException("form");
      if (form.IsDisposed)
        throw new ObjectDisposedException(form.ToString());
      return EFPFormProvider.FindFormProviderRequired(form);
    }

    /// <summary>
    /// Выполняет подготовку к просмотру формы
    /// В интерфейсах MDI и TDI используется CurrentMainWindowLayout. При этом, если главное окно еще не было создано, оно создается вызовом CreateMainWindow().
    /// В интерфейсе SDI всегда создается новый объект EFPAppMainWindowLayout для размещения формы.
    /// </summary>
    /// <param name="form">Форма</param>
    /// <returns>Сюда могут быть записаны произвольные данные, которые понадобятся для показа формы</returns>
    protected abstract object OnPrepareChildForm(Form form);

    /// <summary>
    /// Показывает созданную в пользовательском коде форму.
    /// Если метод PrepareChildForm() не был вызван в явном виде, он вызывается. Затем вызывается OnShowChildForm().
    /// </summary>
    /// <param name="form">Созданная в пользовательском коде форма, которую надо отобразить</param>
    public void ShowChildForm(Form form)
    {
      EFPFormProvider formProvider = GetChildFormProvider(form);
      if (!formProvider.Prepared)
        PrepareChildForm(form);
      formProvider.InternalSetVisible(false); // 17.07.2021 - для уменьшения мерцания

      OnShowChildForm(form, formProvider.InternalPreparationData);
      formProvider.InternalPreparationData = null;
    }

    /// <summary>
    /// Выполняет вывод формы на экран
    /// </summary>
    /// <param name="form">Форма</param>
    /// <param name="preparationData">Данные, полученные от OnPrepareChildForm()</param>
    protected abstract void OnShowChildForm(Form form, object preparationData);

    #endregion

    #region Поиск форм

    /// <summary>
    /// Поиск главного окна, содержащего форму.
    /// Возвращает null, если форма не найдена
    /// </summary>
    /// <param name="form">Искомая форма</param>
    /// <returns>Описание главного окна или null</returns>
    public EFPAppMainWindowLayout FindMainWindowLayout(Form form)
    {
      if (form == null)
        return null;

      foreach (EFPAppMainWindowLayout layout in _MainWindows)
      {
        if (layout.ContainsForm(form))
          return layout;
      }
      return null;
    }


    /// <summary>
    /// Найти дочернюю форму заданного класса.
    /// Возвращает первую найденную форму или null
    /// </summary>
    /// <param name="formType">Тип формы</param>
    /// <returns>Найденная форма или null</returns>
    public Form FindChildForm(Type formType)
    {
      EFPAppMainWindowLayout[] layouts = GetMainWindowLayouts(false);
      for (int i = 0; i < layouts.Length; i++)
      {
        Form form = layouts[i].FindChildForm(formType);
        if (form != null)
          return form;
      }
      return null;
    }

    /// <summary>
    /// Найти дочернюю форму заданного класса.
    /// Возвращает первую найденную форму или null.
    /// </summary>
    /// <typeparam name="TForm">Класс формы</typeparam>
    /// <returns>Найденная форма или null</returns>
    public TForm FindChildForm<TForm>()
      where TForm : Form
    {
      return FindChildForm<TForm>(null);
    }

    /// <summary>
    /// Найти дочернюю форму заданного класса.
    /// Дополнительно задается критерий для выбора формы, если есть несколько однотипных форм.
    /// Возвращает первую найденную форму или null.
    /// </summary>
    /// <typeparam name="TForm">Класс формы</typeparam>
    /// <param name="match">Критерий для выбора формы. Если null, то возвращается первая форма подходящего типа</param>
    /// <returns>Найденная форма или null</returns>
    public TForm FindChildForm<TForm>(Predicate<TForm> match)
      where TForm : Form
    {
      EFPAppMainWindowLayout[] layouts = GetMainWindowLayouts(false);
      for (int i = 0; i < layouts.Length; i++)
      {
        TForm form = layouts[i].FindChildForm<TForm>(match);
        if (form != null)
          return form;
      }
      return null;
    }

    /// <summary>
    /// Найти все дочерние формы заданного класса
    /// </summary>
    /// <param name="formType">Тип формы</param>
    /// <returns>Массив форм</returns>
    public Form[] FindChildForms(Type formType)
    {
      EFPAppMainWindowLayout[] layouts = GetMainWindowLayouts(false);
      if (layouts.Length == 1)
        return layouts[0].FindChildForms(formType);
      else
      {
        List<Form> lst = new List<Form>();
        for (int i = 0; i < layouts.Length; i++)
          layouts[i].FindChildFormsInternal(lst, formType);
        return lst.ToArray();
      }
    }

    /// <summary>
    /// Найти все дочерние формы заданного класса
    /// </summary>
    /// <typeparam name="TForm">Класс формы</typeparam>
    /// <returns>Массив форм</returns>
    public TForm[] FindChildForms<TForm>()
      where TForm : Form
    {
      EFPAppMainWindowLayout[] layouts = GetMainWindowLayouts(false);
      if (layouts.Length == 1)
        return layouts[0].FindChildForms<TForm>();
      else
      {
        List<TForm> lst = new List<TForm>();
        for (int i = 0; i < layouts.Length; i++)
          layouts[i].FindChildFormsInternal<TForm>(lst);
        return lst.ToArray();
      }
    }

    /// <summary>
    /// Найти и активировать форму заданного класса.
    /// Возвращает true в случае успеха
    /// </summary>
    /// <param name="formType">Тип формы</param>
    /// <returns>true, если форма найдена и активирована. false, если форма не найдена</returns>
    public bool FindAndActivateChildForm(Type formType)
    {
#if DEBUG
      EFPApp.CheckMainThread();
#endif

      Form frm = FindChildForm(formType);
      if (frm == null)
        return false;
      EFPApp.Activate(frm); // 07.06.2021
      return true;
    }

    /// <summary>
    /// Найти и активировать форму заданного класса.
    /// Возвращает true в случае успеха
    /// </summary>
    /// <typeparam name="TForm">Класс формы</typeparam>
    /// <returns>true, если форма найдена и активирована. false, если форма не найдена</returns>
    public bool FindAndActivateChildForm<TForm>()
      where TForm : Form
    {
      return FindAndActivateChildForm<TForm>(null);
    }


    /// <summary>
    /// Найти и активировать форму заданного класса.
    /// Дополнительно задается критерий для выбора формы, если есть несколько однотипных форм.
    /// Возвращает true в случае успеха.
    /// </summary>
    /// <typeparam name="TForm">Класс формы</typeparam>
    /// <returns>true, если форма найдена и активирована. false, если форма не найдена</returns>
    public bool FindAndActivateChildForm<TForm>(Predicate<TForm> match)
      where TForm : Form
    {
#if DEBUG
      EFPApp.CheckMainThread();
#endif

      TForm frm = FindChildForm<TForm>(match);
      if (frm == null)
        return false;
      EFPApp.Activate(frm); // 07.06.2021
      return true;
    }

    #endregion

    #region Закрытие формы

    /// <summary>
    /// Закрывает все дочерние окна.
    /// Для интерфейса SDI не остается ни одного открытого окна.
    /// Для MDI остается пустое главное окно (или несколько окон).
    /// Закрытие окон может быть отменено, если окно выдает подтверждение на закрытие 
    /// несохраненных данных, а пользователь откажется закрывать окно.
    /// </summary>
    /// <returns>
    /// true, если все дочерние окна были закрыты.
    /// false, если какое-либо окно не было закрыто из-зат отказа пользователя.
    /// </returns>
    public bool CloseAllChildren()
    {
      EFPAppMainWindowLayout[] layouts = GetMainWindowLayouts(true); // в Z-порядке
      for (int i = 0; i < layouts.Length; i++)
      {
        if (!layouts[i].CloseAllChildren())
          return false;
      }
      return true;
    }

    #endregion

    #region Упорядочение форм

    /// <summary>
    /// Возвращает true, если данный тип упорядочивания дочерних окон принципиально поддерживается интерфейсом
    /// </summary>
    /// <param name="mdiLayout">Способ упрорядочивания</param>
    /// <returns>Поддержка</returns>
    public virtual bool IsLayoutChildFormsSupported(MdiLayout mdiLayout)
    {
      return false;
    }

    /// <summary>
    /// Возвращает true, если данный тип упорядочивания дочерних окон в данный момент применим
    /// </summary>
    /// <param name="mdiLayout">Способ упрорядочивания</param>
    /// <returns>Поддержка</returns>
    public virtual bool IsLayoutChildFormsAppliable(MdiLayout mdiLayout)
    {
      return false;
    }

    /// <summary>
    /// Упорядочивание дочерних окон.
    /// Для интерфейса SDI применяется ко всем окнам.
    /// Для MDI и TDI применяется только к текущему главному окну
    /// </summary>
    /// <param name="mdiLayout">Способ упорядочения</param>
    public virtual void LayoutChildForms(MdiLayout mdiLayout)
    {
      if (CurrentMainWindowLayout != null)
        CurrentMainWindowLayout.LayoutChildForms(mdiLayout);
    }

    #endregion

    #endregion

    #region Композиция

    // Сохранение и восстановление интерфейса

    /// <summary>
    /// Сохраняет композицию окон в секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации "Composition"-"UI"</param>
    public void SaveComposition(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      if (EFPApp.InsideLoadComposition || EFPApp.InsideSaveComposition)
        throw new ReenteranceException();

      EFPApp.InsideSaveComposition = true;
      try
      {
        DoSaveComposition(cfg);
      }
      finally
      {
        EFPApp.InsideSaveComposition = false;
      }
    }
    private void DoSaveComposition(CfgPart cfg)
    {
      cfg.SetString("InterfaceType", this.Name);
      EFPAppMainWindowLayout[] layouts = null;
      int currWindowIndex;
      if (!IsSDI)
      {
        // Главные окна
        layouts = GetMainWindowLayouts(false); // в том порядке, как открыты
        cfg.SetInt32("MainWindowCount", layouts.Length);
        currWindowIndex = -1;
        for (int i = 0; i < layouts.Length; i++)
        {
          CfgPart cfgMainWindow = cfg.GetChild("MainWindow" + (i + 1).ToString(), true);
          layouts[i].Bounds.FromControl(layouts[i].MainWindow);
          CfgPart cfgBounds = cfgMainWindow.GetChild("Bounds", true);
          layouts[i].Bounds.WriteConfig(cfgBounds);
          layouts[i].WriteLayoutConfig(cfgMainWindow);

          if (Object.ReferenceEquals(layouts[i], CurrentMainWindowLayout))
            currWindowIndex = i;
        }
        if (currWindowIndex >= 0)
          cfg.SetInt32("CurrentMainWindow", currWindowIndex + 1);
      }

      Form[] childForms = GetChildForms(false); // в порядке создания, а не Z-Order
      //Part.SetInt32("FormCount", ChildForms.Length);
      cfg.SetInt32("FormCount", 0); // резервируем место 
      int cnt = 0; // некоторые окна могут не сохраняться
      currWindowIndex = -1;
      Form currForm = this.CurrentChildForm;
      for (int i = 0; i < childForms.Length; i++)
      {
        if (!EFPApp.FormWantsSaveComposition(childForms[i]))
          continue;

        cnt++;

        EFPAppMainWindowLayout layout = FindMainWindowLayout(childForms[i]);
        if (layout == null)
          throw new BugException("Cannot find a EFPAppMainWindowLayout object for the child form:" + childForms[i].ToString());
        CfgPart cfgForm = cfg.GetChild("Form" + cnt.ToString(), true);
        if (layouts != null)
        {
          // 27.12.2020 Лишняя проверка
          //          if (Layout != null)
          //          {
          int p = Array.IndexOf<EFPAppMainWindowLayout>(layouts, layout);
          cfgForm.SetInt32("MainWindow", p + 1);
          //          }
        }
        else
          layout.WriteLayoutConfig(cfgForm);

        EFPFormProvider formProvider = EFPFormProvider.FindFormProviderRequired(childForms[i]);
        formProvider.WriteComposition(cfgForm);

        if (Object.ReferenceEquals(childForms[i], currForm))
          currWindowIndex = cnt - 1;
      }

      cfg.SetInt32("FormCount", cnt);
      if (currWindowIndex >= 0)
        cfg.SetInt32("CurrentForm", currWindowIndex + 1);

      //// Превью
      //try
      //{
      //  using (Bitmap bmp = EFPApp.CreateSnapshot())
      //  {
      //    using(System.IO.MemoryStream strm=new System.IO.MemoryStream())
      //    {
      //      bmp.Save(strm,System.Drawing.Imaging.ImageFormat.Gif);
      //      string s = Convert.ToBase64String(strm.GetBuffer());
      //      Part.SetString("Snapshot", s);
      //      //EFPApp.MessageBox("Размер: " + strm.Length.ToString());
      //    }
      //  }
      //}
      //catch (Exception e)
      //{
      //  LogoutTools.LogoutException(e, "Не удалось создать Snapshot интерфейса");
      //}
    }

    /// <summary>
    /// Восстанавливает композицию окон в секции конфигурации.
    /// Предполагается, что интерфейс только что присоединен к EFPApp и еще не открыто ни одеого главного окна
    /// </summary>
    /// <param name="cfg">Секция конфигурации "Composition"-"UI"</param>
    public void LoadComposition(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      if (EFPApp.InsideLoadComposition || EFPApp.InsideSaveComposition)
        throw new ReenteranceException();

      EFPApp.InsideLoadComposition = true;
      try
      {
        try
        {
          DoLoadComposition(cfg);
        }
        catch (Exception e)
        {
          try { e.Data["CfgPart"] = cfg.GetXmlText(); }
          catch { }

          throw;
        }
      }
      finally
      {
        EFPApp.InsideLoadComposition = false;
      }

      EFPApp.SetInterfaceChanged(); // принудительный вызов события InterfaceChanged
    }

    private void DoLoadComposition(CfgPart cfg)
    {
      if (!CloseAll())
        throw new CannotCloseFormException(Res.EFPApp_Err_CannotCloseAllForms);

      List<EFPAppMainWindowLayout> mwls = new List<EFPAppMainWindowLayout>();

      if (!IsSDI)
      {
        int mainWindowCount = cfg.GetInt32("MainWindowCount");
        int currMainWindowIndex = cfg.GetInt32("CurrentMainWindow") - 1;
        for (int i = 0; i < mainWindowCount; i++)
        {
          EFPAppMainWindowLayout mwl = ShowMainWindow();
          mwls.Add(mwl);
          CfgPart cfgMainWindow = cfg.GetChild("MainWindow" + (i + 1).ToString(), false);
          if (cfgMainWindow != null)
          {
            CfgPart cfgBounds = cfgMainWindow.GetChild("Bounds", false);
            if (cfgBounds != null)
            {
              mwl.Bounds.ReadConfig(cfgBounds);
              if (mwl.Bounds.WindowState == FormWindowState.Minimized)
                mwl.Bounds.WindowState = FormWindowState.Normal; // свернутые окна разворачиваем
              //mwl.Bounds.ClearMainFormScreenBounds();
              mwl.Bounds.ToControl(mwl.MainWindow);
            }
            mwl.ReadLayoutConfig(cfgMainWindow);
          }
        }

        if (mwls.Count == 0)
        {
          EFPAppMainWindowLayout mwl = ShowMainWindow();
          if (EFPApp.MainWindowDefaultMaximized)
            mwl.MainWindow.WindowState = FormWindowState.Maximized;
          mwls.Add(mwl);
        }

        if (currMainWindowIndex >= 0 && currMainWindowIndex < mwls.Count)
          mwls[currMainWindowIndex].MainWindow.Select();
      }

      int childFormCount = cfg.GetInt32("FormCount");
      int currChildFormIndex = cfg.GetInt32("CurrentForm") - 1;
      Form currForm = null;
      for (int i = 0; i < childFormCount; i++)
      {
        CfgPart cfgForm = cfg.GetChild("Form" + (i + 1).ToString(), false);
        if (cfgForm != null)
        {
          EFPFormCreatorParams creatorParams = new EFPFormCreatorParams(cfgForm);
          try
          {
            Form form;
            try
            {
              form = EFPApp.CreateForm(creatorParams);
            }
            catch (Exception e)
            {
              EFPApp.ErrorMessageBox(String.Format(Res.EFPApp_Err_LoadCompositionForm, creatorParams.Title, e.Message));
              continue;
            }
            if (form == null)
              continue;

            EFPFormProvider formProvider = EFPFormProvider.FindFormProviderRequired(form);
            if (formProvider.HasBeenShown)
              throw new BugException("When the form \"" + form.ToString() + "\" restoring, it was shown early");

            if (!IsSDI)
            {
              EFPAppMainWindowLayout mwl = mwls[0];
              int mainWindow = cfgForm.GetInt32("MainWindow");
              if (mainWindow > 0 && mainWindow <= mwls.Count)
                mwl = mwls[mainWindow - 1];

              mwl.MainWindow.Select();
              mwl.ReadLayoutConfig(cfgForm);
            }

            try
            {
              PrepareChildForm(form);
              // лишнее if (FormProvider != null)
              formProvider.ReadComposition(cfgForm);

              if (IsSDI)
              {
                EFPAppMainWindowLayout mwl = CurrentMainWindowLayout;
                mwl.ReadLayoutConfig(cfgForm);
              }

              ShowChildForm(form);
              if (i == currChildFormIndex)
                currForm = form;
            }
            catch (Exception e)
            {
              EFPApp.ShowException(e, String.Format(Res.EFPApp_ErrTitle_ShowForm, creatorParams.Title));
            }
          }
          catch (Exception e)
          {
            EFPApp.ShowException(e, String.Format(Res.EFPApp_ErrTitle_RestoreFormUnknownError, creatorParams.Title));
          }
        }
      } // цикл по дочерним формам

      if (currForm != null)
        currForm.Select();

      //if (IsSDI && ChildFormCount==0)
      //  EFPApp.def
    }

    #endregion

    #region Сохранение видимости элементов главного окна

    /// <summary>
    /// Сохраняет видимость панелей инеструментов и других элементов главного окна в заданной секции
    /// конфигурации.
    /// Если открыто несколько главных окон или используется интерфейс SDI, то используется активное
    /// главное окно.
    /// Если нет ни одного главного окна, никаких действий не выполняется
    /// </summary>
    /// <param name="cfg">Заполняемая секция</param>
    public void SaveMainWindowLayout(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif


      if (CurrentMainWindowLayout != null)
        CurrentMainWindowLayout.WriteLayoutConfig(cfg);

      // TODO: Запись ToolWindow
    }

    /// <summary>
    /// Загрузка видимости панелей инеструментов и других элементов главного окна из заданной секции
    /// конфигурации.
    /// Если открыто несколько главных окон или используется интерфейс SDI, то устанавливается
    /// видимость для всех окон.
    /// Если нет ни одного главного окна, никаких действий не выполняется
    /// </summary>
    /// <param name="cfg">Считываемая секция конфигурации</param>
    public void LoadMainWindowLayout(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      foreach (EFPAppMainWindowLayout layout in this)
        layout.ReadLayoutConfig(cfg);

      EFPApp.SetInterfaceChanged(); // требуется принудительное обновление меню
    }

    #endregion

    #region IObjectWithCode Members

    string IObjectWithCode.Code { get { return Name; } }

    #endregion

    #region IEnumerable<EFPAppMainWindowLayout> Members

    /// <summary>
    /// Возвращает перечислитель по главным окнам программы в порядке их добавления (ZOrder=false)
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<EFPAppMainWindowLayout> GetEnumerator()
    {
      return _MainWindows.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// Возвращает перечислитель по главным окнам программы 
    /// Если <paramref name="useZOrder"/>=true, то окна перебираются по порядку активации (CurrentMainWindowLayout будет первым в списке).
    /// Если <paramref name="useZOrder"/>=false, то окна перебираются по порядку открытия окон (#1, #2)
    /// </summary>
    /// <param name="useZOrder">Нужно ли сортировать главные окна по порядку активации</param>
    /// <returns>Перечислитель</returns>
    public IEnumerator<EFPAppMainWindowLayout> GetEnumerator(bool useZOrder)
    {
      if (useZOrder)
        return _MainWindows.MRU.GetEnumerator();
      else
        return _MainWindows.GetEnumerator();
    }

    #endregion
  }

  /// <summary>
  /// Текущее состояние интерфейса
  /// </summary>
  public sealed class EFPAppInterfaceState
  {
    #region Конструктор

    /// <summary>
    /// Определяет текущее состояние интерфейса
    /// </summary>
    internal EFPAppInterfaceState()
    {
      _Interface = EFPApp.Interface;
      if (EFPApp.Interface != null)
      {
        _CurrentMainWindowLayout = EFPApp.Interface.CurrentMainWindowLayout;
        _MainWindowCount = EFPApp.Interface.MainWindowCount;
        _CurrentChildForm = EFPApp.Interface.CurrentChildForm;
        _ChildFormCount = EFPApp.Interface.ChildFormCount;
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект управления интерфейсом
    /// </summary>
    public EFPAppInterface Interface { get { return _Interface; } }
    private EFPAppInterface _Interface;

    /// <summary>
    /// Текущее главное окно (особенно актуально для интерфейса SDI)
    /// </summary>
    public EFPAppMainWindowLayout CurrentMainWindowLayout { get { return _CurrentMainWindowLayout; } }
    private EFPAppMainWindowLayout _CurrentMainWindowLayout;

    /// <summary>
    /// Количество главных окон (для интерфейса SDI-количество всех окон)
    /// </summary>
    public int MainWindowCount { get { return _MainWindowCount; } }
    private int _MainWindowCount;

    /// <summary>
    /// Активное дочернее окно
    /// </summary>
    public Form CurrentChildForm { get { return _CurrentChildForm; } }
    private Form _CurrentChildForm;

    /// <summary>
    /// Количество дочерних окон
    /// </summary>
    public int ChildFormCount { get { return _ChildFormCount; } }
    private int _ChildFormCount;

    #endregion

    #region Сравнение

    /// <summary>
    /// Сравнение двух состояний
    /// </summary>
    /// <param name="a">Первое сравниваемое состояние</param>
    /// <param name="b">Второе сравниваемое состояние</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(EFPAppInterfaceState a, EFPAppInterfaceState b)
    {
      if (Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null))
        return true;

      if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null))
        return false;

      return Object.ReferenceEquals(a._Interface, b._Interface) &&
        Object.ReferenceEquals(a._CurrentMainWindowLayout, b.CurrentMainWindowLayout) &&
        a._MainWindowCount == b._MainWindowCount &&
        Object.ReferenceEquals(a._CurrentChildForm, b._CurrentChildForm) &&
        a._ChildFormCount == b._ChildFormCount;
    }

    // Остальное не нужно, но требуется для компилятора

    /// <summary>
    /// Сравнение двух состояний
    /// </summary>
    /// <param name="a">Первое сравниваемое состояние</param>
    /// <param name="b">Второе сравниваемое состояние</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(EFPAppInterfaceState a, EFPAppInterfaceState b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Сравнение двух состояний
    /// </summary>
    /// <param name="obj">Второе сравниваемое состояние</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      return this == (EFPAppInterfaceState)obj;
    }

    /// <summary>
    /// Не используется. Возвращает 0.
    /// </summary>
    /// <returns>0</returns>
    public override int GetHashCode()
    {
      return 0;
    }

    #endregion
  }


  /// <summary>
  /// Исключение, выбрасываемое EFPAppInterface.Detach(), если не удалось закрыть все окна
  /// </summary>
  [Serializable]
  public class CannotCloseFormException : ApplicationException
  {
    #region Конструктор

    /// <summary>
    /// Создает объект исключения с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public CannotCloseFormException(string message)
      : this(message, null)
    {
    }


    /// <summary>
    /// Создает объект исключения с заданным сообщением и вложенным исключением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    /// <param name="innerException">Вложенное исключение</param>
    public CannotCloseFormException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Создает исключение с текстом сообщения по умолчанию
    /// </summary>
    public CannotCloseFormException()
      : base("Не удалось закрыть окно")
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected CannotCloseFormException(System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

}
