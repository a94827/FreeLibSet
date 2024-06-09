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
  /// Главное окно интерфейса
  /// </summary>
  public abstract class EFPAppMainWindowLayout : IEnumerable<Form>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект, к которому пока не присоединено окно (MainWindow=null).
    /// </summary>
    public EFPAppMainWindowLayout()
    {
      _ChildForms = new ListWithMRU<Form>();
      _MainWindowNumberText = String.Empty;
      _WindowStateBeforeMinimized = FormWindowState.Normal;
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Интерфейс, к которому относится это главное окно
    /// </summary>
    public EFPAppInterface Interface
    {
      get { return _Interface; }
      internal set { _Interface = value; }
    }
    private EFPAppInterface _Interface;

    /// <summary>
    /// Возвращает заголовок главного окна, если свойство MainWindow установлено.
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      if (MainWindow == null)
        return "Нет окна";
      else
        return MainWindow.Text;
    }

    #endregion

    #region Главное окно

    /// <summary>
    /// Главное окно, присоединенное к текущему объекту.
    /// Это свойство не следует использовать в пользовательском коде
    /// </summary>
    public Form MainWindow
    {
      get { return _MainWindow; }
      protected set 
      { 
#if DEBUG
        if (value == null)
          throw new ArgumentNullException();
        if (_MainWindow != null)
          throw new InvalidOperationException("Повторная установка свойства");
#endif
        _MainWindow = value;
        _MainWindow.SizeChanged += new EventHandler(MainWindow_SizeChanged);
        MainWindow_SizeChanged(null, null);
      }
    }
    private Form _MainWindow;

    /// <summary>
    /// Закрывает текущее главное окно без попытки завершить приложение
    /// </summary>
    /// <returns>true, если окно было успешно закрыто</returns>
    public bool CloseMainWindow()
    {
      if (!MainWindow.Visible)
        return true;
      if (InsideCloseMainWindow)
        return true;
      _InsideCloseMainWindow = true;
      try
      {
        MainWindow.Close();
      }
      finally
      {
        _InsideCloseMainWindow = false;
      }
      return !MainWindow.Visible;
    }

    /// <summary>
    /// Возвращает true, если в данный момент выполняется метод CloseMainWindow()
    /// </summary>
    public bool InsideCloseMainWindow { get { return _InsideCloseMainWindow; } }
    private bool _InsideCloseMainWindow;

    /// <summary>
    /// Если в интерфейсе MDI открыто больше одного главного окна,
    /// то возвращается строка "#1", "#2", ...
    /// Если есть только одно главного окно, или интерфейс SDI, возвращается пустая строка
    /// </summary>
    public string MainWindowNumberText
    {
      get { return _MainWindowNumberText; }
      internal set { _MainWindowNumberText = value; }
    }
    private string _MainWindowNumberText;

    /// <summary>
    /// Возвращает состояние (Normal или Maximized), которое было до минимизации.
    /// Если в текущий момент окно не свернуто, возвращает Form.WindowState
    /// </summary>
    public FormWindowState WindowStateBeforeMinimized { get { return _WindowStateBeforeMinimized; } }
    private FormWindowState _WindowStateBeforeMinimized;

    #endregion

    #region Дочерние окна

    /// <summary>
    /// Список с поддержкой для Z-order
    /// </summary>
    private ListWithMRU<Form> _ChildForms;

    /// <summary>
    /// Возвращает список дочерних форм, обслуживаемых данным окном.
    /// Если <paramref name="useZOrder"/>=true, то список сортируется по порядку размещения окон 
    /// в текущем главном окне (CurrentChildForm будет первым в списке).
    /// Если <paramref name="useZOrder"/>=false, то список сортируется по порядку открытия окон в данном главном окне
    /// Для интерфейса SDI возвращает массив из одной формы (равным свойству Form).
    /// Если интерфейс SDI вывел «пустышку», то метод возвращает пустой массив.
    /// </summary>
    /// <param name="useZOrder">Нужно ли сортировать окна по порядку активации</param>
    /// <returns>Массив форм</returns>
    public Form[] GetChildForms(bool useZOrder)
    {
      if (useZOrder)
        return _ChildForms.MRU.ToArray();
      else
        return _ChildForms.ToArray();
    }


    /// <summary>
    /// Возвращает дочернюю форму, которая является текущей (или была текущей в последний раз) в пределах главного окна.
    /// Для SDI всегда возвращает один объект Form.
    /// </summary>
    public Form CurrentChildForm
    {
      get
      {
        return _ChildForms.MRU.First;
      }
      set
      {
        throw new NotImplementedException();
      }
    }

    /// <summary>
    /// Возвращает количество дочерних форм в этом окне.
    /// Для интерфейса SDI возвращает 1, если форма не "пустышка".
    /// </summary>
    public int ChildFormCount { get { return _ChildForms.Count; } }

    /// <summary>
    /// Возвращает true, если форма относится к этому главному окну (или сама является главным окном)
    /// </summary>
    /// <param name="form">Искомая форма</param>
    /// <returns>Наличие формы</returns>
    public bool ContainsForm(Form form)
    {
      if (form == null)
        return false;
      if (form == _MainWindow)
        return true;
      foreach (Form child in _ChildForms)
      {
        if (form == child)
          return true;
      }
      return false;
    }

    /// <summary>
    /// Найти дочернюю форму заданного класса.
    /// Возвращает первую найденную форму или null
    /// </summary>
    /// <param name="formType">Тип формы</param>
    /// <returns>Найденная форма или null</returns>
    public Form FindChildForm(Type formType)
    {
      Form[] forms = GetChildForms(false);
      for (int i = 0; i < forms.Length; i++)
      {
        if (forms[i].GetType() == formType)
          return forms[i];
      }
      return null;
    }

    /// <summary>
    /// Найти дочернюю форму заданного класса.
    /// Возвращает первую найденную форму или null
    /// </summary>
    /// <typeparam name="T">Класс формы</typeparam>
    /// <returns>Найденная форма или null</returns>
    public T FindChildForm<T>()
      where T : Form
    {
      Form[] forms = GetChildForms(false);
      for (int i = 0; i < forms.Length; i++)
      {
        if (forms[i] is T)
          return (T)(forms[i]);
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
      List<Form> list = new List<Form>();
      FindChildFormsInternal(list, formType);
      return list.ToArray();
    }

    internal void FindChildFormsInternal(List<Form> list, Type formType)
    {
      Form[] forms = GetChildForms(false);
      for (int i = 0; i < forms.Length; i++)
      {
        if (forms[i].GetType() == formType)
          list.Add(forms[i]);
      }
    }

    /// <summary>
    /// Найти все дочерние формы заданного класса
    /// </summary>
    /// <typeparam name="T">Класс формы</typeparam>
    /// <returns>Массив форм</returns>
    public T[] FindChildForms<T>()
      where T : Form
    {
      List<T> list = new List<T>();
      FindChildFormsInternal<T>(list);
      return list.ToArray();
    }

    internal void FindChildFormsInternal<T>(List<T> list)
      where T : Form
    {
      Form[] forms = GetChildForms(false);
      for (int i = 0; i < forms.Length; i++)
      {
        if (forms[i] is T)
          list.Add((T)(forms[i]));
      }
    }

    /// <summary>
    /// Подготовка дочернего окна к присоединению.
    /// Добавляет окно в список ChildForms и присоединяет необходимые обработчики форм.
    /// </summary>
    /// <param name="form">Дочернее окно</param>
    protected void PrepareChildForm(Form form)
    {
      if (form == null)
        throw new ArgumentNullException("form");
      if (form.IsDisposed)
        throw new ObjectDisposedException("form");

      if (_ChildForms.Contains(form))
        throw new InvalidOperationException("Форма " + form.ToString() + " уже есть в списке форм главного окна");

      _ChildForms.Add(form);

      form.Activated += new EventHandler(Form_Activated);
      form.Enter += new EventHandler(Form_Activated); // дубль
      form.VisibleChanged += new EventHandler(Form_VisibleChanged);
    }

    void Form_Activated(object sender, EventArgs args)
    {
      if (Interface == null)
        return; // 10.06.2021, чтобы не зацикливалось при ошибочном вызове

      Form form = (Form)sender;
      if (Interface.CurrentChildForm == form)
        return; // не изменилось

      _ChildForms.Touch(form);
      Interface.MainWindowActivated(this); // иначе EFPApp получит неверную текущую форму

      //System.Diagnostics.Trace.WriteLine("Activated для формы " + Form.Text);

      EFPApp.TestInterfaceChanged();
    }

    void Form_VisibleChanged(object sender, EventArgs args)
    {
      if (Interface == null)
        return; // 10.06.2021, чтобы не зацикливалось при ошибочном вызове

      Form form = (Form)sender;
      if (form.Visible)
      {
        if (!_ChildForms.Contains(form))
          _ChildForms.Add(form);
      }
      else
      {
        _ChildForms.Remove(form);
      }
      EFPApp.TestInterfaceChanged();
    }

    /// <summary>
    /// Упорядочить дочерние окна.
    /// Применимо только для интерфейса MDI
    /// </summary>
    /// <param name="mdiLayout">Способ упорядочения</param>
    public virtual void LayoutChildForms(MdiLayout mdiLayout)
    {
      throw new InvalidOperationException("Не реализовано для интерфейса " + Interface.Name);
    }

    /// <summary>
    /// Закрыть все дочерние окна.
    /// Для SDI закрывается единственное окно
    /// </summary>
    /// <returns>true, если все окна были закрыты.
    /// false, если пользователь отказался закрывать одно из окон</returns>
    public bool CloseAllChildren()
    {
      bool res;
      EFPApp.BeginUpdateInterface(); // Исправлено 13.09.2021
      try
      {
        res = DoCloseAllChildren();
      }
      finally
      {
        EFPApp.EndUpdateInterface();
      }
      return res;
    }

    private bool DoCloseAllChildren()
    {
      Form[] forms = GetChildForms(true);

      for (int i = 0; i < forms.Length; i++)
      {
        forms[i].Close();
        if (forms[i].Visible)
          return false;
      }
      return true;
    }

    #endregion

    #region Статические методы декорирования

    /// <summary>
    /// Добавляет в форму четыре объекта ToolStripPanel для меню и панелей инструментов, статусную строку
    /// </summary>
    /// <param name="mainWindow"></param>
    protected static void DecorateMainWindow(Form mainWindow)
    {
      // Create ToolStripPanel controls.
      ToolStripPanel stripPanelTop = new ToolStripPanel();
      ToolStripPanel stripPanelBottom = new ToolStripPanel();
      ToolStripPanel stripPanelLeft = new ToolStripPanel();
      ToolStripPanel stripPanelRight = new ToolStripPanel();

      // Dock the ToolStripPanel controls to the edges of the TheForm.
      stripPanelTop.Dock = DockStyle.Top;
      stripPanelBottom.Dock = DockStyle.Bottom;
      stripPanelLeft.Dock = DockStyle.Left;
      stripPanelRight.Dock = DockStyle.Right;

      /*
      if (EnvironmentTools.IsMono)
      {
        StripPanelTop.BackColor = SystemColors.Control;
        StripPanelBottom.BackColor = SystemColors.Control;
        StripPanelLeft.BackColor = SystemColors.Control;
        StripPanelRight.BackColor = SystemColors.Control;
      } */

      // Add the ToolStripPanels to the TheForm in reverse order.
      mainWindow.Controls.Add(stripPanelRight);
      mainWindow.Controls.Add(stripPanelLeft);
      mainWindow.Controls.Add(stripPanelBottom);
      mainWindow.Controls.Add(stripPanelTop);

      StatusStrip theStatusBar = new System.Windows.Forms.StatusStrip();
      EFPApp.SetStatusStripHeight(theStatusBar, mainWindow); // 16.06.2021
      mainWindow.Controls.Add(theStatusBar);
    }

    #endregion

    #region Обработчики главного окна

    private void MainWindow_SizeChanged(object sender, EventArgs args)
    {
      if (_MainWindow.WindowState != FormWindowState.Minimized)
        _WindowStateBeforeMinimized = _MainWindow.WindowState;
    }

    internal void MainWindow_FormClosing(object sender, FormClosingEventArgs args)
    {
      if (args.Cancel)
        return;

      if (EFPApp.IsClosing)
        return;

      if (EFPApp.Interface == this.Interface && (!EFPApp.InsideInterfaceAssignation))
      {
        // Закрываем последнее главное окно?
        bool closeApp = (EFPApp.Interface.MainWindowCount == 1) && (!InsideCloseMainWindow);
        if ((!closeApp) && (!Interface.IsSDI) && (!InsideCloseMainWindow))
        {
          RadioSelectDialog dlg = new RadioSelectDialog();
          dlg.Title = "Закрытие окна " + this.MainWindow.Text;
          //dlg.ImageKey = "CloseWindow"; // значок слишком реалистично выглядит - кнопка закрытия вместо системного меню
          dlg.GroupTitle = "Что надо сделать";
          dlg.Items = new string[] { "Закрыть текущее главное окно", "Завершить работу" };
          dlg.ImageKeys = new string[] { "CloseWindow", "Exit" };
          if (dlg.ShowDialog() != DialogResult.OK)
          {
            args.Cancel = true;
            return;
          }
          if (dlg.SelectedIndex == 1)
            closeApp = true;
        }

        if (closeApp)
        {
          //EFPApp.OnClosing(Args, false);
          // 21.09.2018
          _InsideCloseMainWindow = true;
          try
          {
            if(!EFPApp.Exit())
              args.Cancel=true;
          }
          finally
          {
            _InsideCloseMainWindow = false;
          }
        }
      }
    }

    internal void MainWindow_FormClosed(object sender, FormClosedEventArgs args)
    {
      Interface.RemoveMainWindow(this);

      if (EFPApp.Interface == this.Interface && (!EFPApp.InsideInterfaceAssignation) && (!InsideCloseMainWindow))
      {
        if (Interface.MainWindowCount == 0)
        {
          // Приложение все еще может оставаться запущенным. Если его второй раз закрыть,
          // ничего плохого не случится
          // А вот если не закрыть, то видимых на экране форм не будет, а процесс в
          // памяти останется
          Application.Exit();
        }
      }
    }

    internal void MainWindow_Activated(object sender, EventArgs args)
    {
      try
      {
        Interface.MainWindowActivated(this);
        if (EFPApp.ActiveDialog != null)
          EFPApp.ActiveDialog.Select();
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e, "MainWindow_Activated");
      }
    }

    #endregion

    #region Элементы оформления главного окна

    /// <summary>
    /// Главное меню 
    /// </summary>
    public EFPMainMenu MainMenu
    {
      get { return _MainMenu; }
      internal set { _MainMenu = value; }
    }
    private EFPMainMenu _MainMenu;

    /// <summary>
    /// Список панелей инструментов, видимостью которых можно управлять
    /// </summary>
    public EFPAppToolBars ToolBars
    {
      get { return _ToolBars; }
      internal set { _ToolBars = value; }
    }
    private EFPAppToolBars _ToolBars;

    /// <summary>
    /// Статусная строка, которой можно управлять
    /// </summary>
    public EFPAppStatusBar StatusBar
    {
      get { return _StatusBar; }
      internal set { _StatusBar = value; }
    }
    private EFPAppStatusBar _StatusBar;


    /// <summary>
    /// Инициализирует главное меню, панели инструментов и статусную строку для главного окна
    /// </summary>
    internal void InitCommandItems()
    {
      // Делаем видимыми или невидимыми нужные команды ?? надо ли?
      EFPApp.CommandItems.InitMenuVisible();

      if (!Interface.ObsoleteMode)
      {
#if DEBUG
        if (_MainMenu != null || _ToolBars != null || _StatusBar != null)
          throw new BugException("Повторная инициализация");
#endif

        #region Главное меню

        _MainMenu = new EFPMainMenu();
        _MainMenu.Name = "TheMainMenu";
        _MainMenu.Add(EFPApp.CommandItems);
        // ???? cmm.InitWindowMenu(MenuWindow);
        _MainMenu.Attach(MainWindow);

        #endregion

        #region Панели инструментов

        _ToolBars = new EFPAppToolBars();
        for (int i = 0; i < EFPApp.ToolBars.Count; i++)
        {
          EFPAppToolBarCommandItems Src = EFPApp.ToolBars[i];

          EFPAppToolBar res = new EFPAppToolBar(Src.Name);
          res.Info = new FormToolStripInfo(MainWindow);
          res.DisplayName = Src.DisplayName;
          res.Add(Src);

          EFPAppToolBar currTB = null;
          if (Interface.CurrentMainWindowLayout != null)
            currTB = Interface.CurrentMainWindowLayout.ToolBars[Src.Name];
          if (currTB == null)
          {
            res.Visible = Src.DefaultVisible;
            res.Dock = Src.DefaultDock;
          }
          else
          {
            res.Visible = currTB.Visible;
            res.Dock = currTB.Dock;
          }

          _ToolBars.Add(res);
        }

        // 22.11.2018
        // Команда "Восстановить"
        EFPCommandItem ciRestore = new EFPCommandItem("View", "RestoreToolBars");
        ciRestore.MenuText = "Восстановить";
        ciRestore.Click += new EventHandler(EFPAppCommandItemHelpers.ToolBarsRestore_Click);
        ciRestore.GroupBegin = true;
        _ToolBars.ContextMenu.Add(ciRestore);

        _ToolBars.Attach();

        #endregion

        #region Статусная строка

        _StatusBar = new EFPAppStatusBar();

        FormToolStripInfo info = new FormToolStripInfo(MainWindow);

        _StatusBar.StatusStripControl = info.StatusBar;

        if (Interface.CurrentMainWindowLayout == null)
          _StatusBar.Visible = true; // ??
        else
          _StatusBar.Visible = Interface.CurrentMainWindowLayout.StatusBar.Visible;

        EFPStatusBarPanels sbp = new EFPStatusBarPanels(_StatusBar, null);
        sbp.Add(EFPApp.CommandItems);
        sbp.Attach();

        #endregion
      }
    }

    /// <summary>
    /// Восстанавливает видиммость и положение панелей инструментов
    /// </summary>
    public void RestoreToolBars()
    {
      for (int i = 0; i < EFPApp.ToolBars.Count; i++)
      {
        EFPAppToolBarCommandItems src = EFPApp.ToolBars[i];

        EFPAppToolBar res = ToolBars[src.Name];
        res.Visible = src.DefaultVisible;
        res.Dock = src.DefaultDock;
      }
    }

    #endregion

    #region Хранение видимости панелей инструментов и статусной строки

    internal void WriteLayoutConfig(CfgPart cfg)
    {
      CfgPart cfgToolBars = cfg.GetChild("ToolBars", true);
      for (int i = 0; i < ToolBars.Count; i++)
      {
        EFPAppToolBar toolBar = ToolBars[i];
        CfgPart cfgOneTB = cfgToolBars.GetChild(toolBar.Name, true);
        cfgOneTB.SetBool("Visible", toolBar.Visible);
        cfgOneTB.SetEnum<DockStyle>("Dock", toolBar.Dock);
        if (toolBar.UseLocation)
        {
          cfgOneTB.SetInt("Left", toolBar.Location.X);
          cfgOneTB.SetInt("Top", toolBar.Location.Y);
          cfgOneTB.Remove("RowIndex");
        }
        else
        {
          cfgOneTB.SetInt("RowIndex", toolBar.RowIndex);
          cfgOneTB.Remove("Left");
          cfgOneTB.Remove("Top");
        }
      }

      CfgPart cfgStatusBar = cfg.GetChild("StatusBar", true);
      cfgStatusBar.SetBool("Visible", StatusBar.Visible);
    }

    internal void ReadLayoutConfig(CfgPart cfg)
    {
      CfgPart cfgToolBars = cfg.GetChild("ToolBars", false);
      if (cfgToolBars != null)
      {
        for (int i = 0; i < ToolBars.Count; i++)
        {
          EFPAppToolBar toolBar = ToolBars[i];
          // Список панелей инструментов мог поменяться и в секции конфигурации записаны не все панели
          // Для отсутствующих секций оставляем параметры панелей "как есть".
          CfgPart cfgOneTB = cfgToolBars.GetChild(toolBar.Name, false);
          if (cfgOneTB != null)
          {
            toolBar.Visible = cfgOneTB.GetBoolDef("Visible", toolBar.Visible);
            toolBar.Dock = cfgOneTB.GetEnumDef<DockStyle>("Dock", toolBar.Dock);
            if (cfgOneTB.HasValue("RowIndex"))
              toolBar.RowIndex = cfgOneTB.GetInt("RowIndex");
            else
            {
              toolBar.Location = new System.Drawing.Point(cfgOneTB.GetInt("Left"),
                cfgOneTB.GetInt("Top"));
            }
          }
        }
      }

      CfgPart cfgStatusBar = cfg.GetChild("StatusBar", false);
      if (cfgStatusBar != null)
        StatusBar.Visible = cfgStatusBar.GetBoolDef("Visible", StatusBar.Visible);
    }

    #endregion

    #region Расположение окон

    /// <summary>
    /// Отслеживание положения главного окна.
    /// Для интерфейса SDI равно null
    /// </summary>
    public EFPFormBounds Bounds
    {
      get { return _Bounds; }
      protected set { _Bounds = value; }
    }
    private EFPFormBounds _Bounds;

    #endregion

    #region IEnumerable<EFPAppMainWindowLayout> Members

    /// <summary>
    /// Возвращает перечислитель по дочерним окнам в порядке их добавления (ZOrder=false)
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<Form> GetEnumerator()
    {
      return _ChildForms.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// Возвращает перечислитель по дочерним окнам.
    /// Если <paramref name="useZOrder"/>=true, то окна перебираются по порядку активации (CurrentChildForm будет первым в списке).
    /// Если <paramref name="useZOrder"/>=false, то окна перебираются по порядку открытия окон (#1, #2)
    /// </summary>
    /// <param name="useZOrder">Нужно ли сортировать окна по порядку активации</param>
    /// <returns>Перечислитель</returns>
    public IEnumerator<Form> GetEnumerator(bool useZOrder)
    {
      if (useZOrder)
        return _ChildForms.MRU.GetEnumerator();
      else
        return _ChildForms.GetEnumerator();
    }

    #endregion

    #region Snapshot

    /// <summary>
    /// Прорисовка главного окна для рисунка предварительного просмотра композиции.
    /// Непереопределенный метод вызывает метод Control.DrawToBitmap().
    /// Переопределяется для интерфейса MDI.
    /// </summary>
    /// <param name="bitmap">Изображение, на котором требуется выполнить рисование</param>
    /// <param name="area">Координаты в пределах изображения <paramref name="bitmap"/>,
    /// в которые требуется вписать изображение окна</param>
    /// <param name="forComposition">Если true, то будут сохранены только те, окна,
    /// которые могут быть записаны в композицию окон.
    /// Если false, то будут включены все окна.
    /// Параметр учитывается только для интерфейса MDI.</param>
    internal protected virtual void DrawMainWindowSnapshot(Bitmap bitmap, Rectangle area, bool forComposition)
    {
      MainWindow.DrawToBitmap(bitmap, area);
    }

    #endregion
  }

  /// <summary>
  /// Класс для поиска элементов ToolStripPanel и StatusBar на произвольной форме.
  /// Не должен использоваться в пользовательском коде
  /// </summary>
  public sealed class FormToolStripInfo
  {
    #region Конструктор

    /// <summary>
    /// Выполняет перебор управляющих элементов формы и заполняет свойства найденными
    /// элементами.
    /// Обрабатывает элементы ToolStripPanel, ToolStripContainer и StatusStrip
    /// </summary>
    /// <param name="form">Форма</param>
    public FormToolStripInfo(Form form)
    {
      if (form == null)
        throw new ArgumentNullException();

      foreach (Control control in form.Controls)
      {
        ToolStripPanel tsp = control as ToolStripPanel;
        if (tsp != null)
        {
          switch (tsp.Dock)
          {
            case DockStyle.Top: _StripPanelTop = tsp; break;
            case DockStyle.Bottom: _StripPanelBottom = tsp; break;
            case DockStyle.Left: _StripPanelLeft = tsp; break;
            case DockStyle.Right: _StripPanelRight = tsp; break;
          }
          continue;
        }

        ToolStripContainer tsc = control as ToolStripContainer;
        if (tsc != null)
        {
          _StripPanelTop = tsc.TopToolStripPanel;
          _StripPanelBottom = tsc.BottomToolStripPanel;
          _StripPanelLeft = tsc.LeftToolStripPanel;
          _StripPanelRight = tsc.RightToolStripPanel;
          continue;
        }

        //StatusStrip ss = Control as StatusStrip;
        //if (ss != null)
        //{
        //  FStatusBar = ss;
        //  continue;
        //}
      }

      // Статусная строка может быть на панели
      _StatusBar = WinFormsTools.FindControl<StatusStrip>(form, true);
    }

    #endregion

    #region Найденные элементы

    /// <summary>
    /// Найденная панель для размещения сверху формы.
    /// Возвращает null, если панель не найдена.
    /// </summary>
    public ToolStripPanel StripPanelTop { get { return _StripPanelTop; } }
    private readonly ToolStripPanel _StripPanelTop;

    /// <summary>
    /// Найденная панель для размещения снизу формы.
    /// Возвращает null, если панель не найдена.
    /// </summary>
    public ToolStripPanel StripPanelBottom { get { return _StripPanelBottom; } }
    private readonly ToolStripPanel _StripPanelBottom;

    /// <summary>
    /// Найденная вертикальная панель для размещения в левой части формы.
    /// Возвращает null, если панель не найдена.
    /// </summary>
    public ToolStripPanel StripPanelLeft { get { return _StripPanelLeft; } }
    private readonly ToolStripPanel _StripPanelLeft;

    /// <summary>
    /// Найденная вертикальная панель для размещения в правой части формы.
    /// Возвращает null, если панель не найдена.
    /// </summary>
    public ToolStripPanel StripPanelRight { get { return _StripPanelRight; } }
    private readonly ToolStripPanel _StripPanelRight;

    /// <summary>
    /// Найденная статусная строка.
    /// Возвращает null, если статусная строка не найдена.
    /// </summary>
    public StatusStrip StatusBar { get { return _StatusBar; } }
    private readonly StatusStrip _StatusBar;

    #endregion
  }

  /// <summary>
  /// Помощник для установки начальной позиции окон для эмуляции DefaultLocation.
  /// Используется в интерфейсе MDI и SDI.
  /// Не следует использовать в пользовательском коде.
  /// </summary>
  public sealed class FormStartPositionCascadeHelper
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует объект
    /// </summary>
    public FormStartPositionCascadeHelper()
    {
      _PrevLocation = new Point(int.MaxValue, int.MaxValue);
    }

    #endregion

    #region Размещение нового окна

    /// <summary>
    /// Смещение для нового окна относительно предыдущего
    /// </summary>
    public Size Delta
    {
      get
      {
        int x = SystemInformation.CaptionHeight + SystemInformation.SizingBorderWidth * 2;
        return new Size(x, x);
      }
    }

    /// <summary>
    /// Результат для предыдущей формы
    /// </summary>
    private Point _PrevLocation;

    /// <summary>
    /// Размещение окна в области с отступом
    /// </summary>
    /// <param name="form">Форма, которую требуется разместить</param>
    /// <param name="area">Область</param>
    public void SetStartPosition(Form form, Rectangle area)
    {
      FormWindowState OldState = form.WindowState;
      try
      {
        // чтобы работать с настоящими размерами
        form.WindowState = FormWindowState.Normal;

        if (form.StartPosition == FormStartPosition.WindowsDefaultBounds)
        {
          form.StartPosition = FormStartPosition.WindowsDefaultLocation;
          Size sz = new Size(area.Width * 3 / 4, area.Height * 3 / 4);
          sz = WinFormsTools.Max(sz, form.MinimumSize);
          sz = WinFormsTools.Max(sz, new Size(600, 400)); // размеры от фонаря
          form.Size = sz;
        }

        if (form.StartPosition == FormStartPosition.WindowsDefaultLocation)
        {
          form.StartPosition = FormStartPosition.Manual;

          // Сдвиг
          if (_PrevLocation.X == int.MaxValue) // первый вызов
            _PrevLocation = area.Location;
          else
            _PrevLocation = Point.Add(_PrevLocation, Delta);

          if ((_PrevLocation.X + form.Width) > area.Right)
            _PrevLocation.X = area.X;
          else if (_PrevLocation.X < area.X)
            _PrevLocation.X = area.X;

          if ((_PrevLocation.Y + form.Height) > area.Height)
            _PrevLocation.Y = area.Y;
          else if (_PrevLocation.Y < area.Y)
            _PrevLocation.Y = area.Y;

          form.Location = _PrevLocation;
        }

        if (form.StartPosition == FormStartPosition.Manual)
        {
          // Корректируем размеры
          WinFormsTools.PlaceFormInRectangle(form, area);
        }
      }
      finally
      {
        form.WindowState = OldState;
      }
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Этот метод вызывается, когда закрываются все дочерние окна.
    /// В следующий раз окно откроется в левом верхнем углу без смещения, как для первого окна.
    /// </summary>
    public void ResetStartPosition()
    {
      _PrevLocation = new Point(int.MaxValue, int.MaxValue);
    }

    #endregion
  }
}
