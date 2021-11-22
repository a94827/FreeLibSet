// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Реализации стандартных команд главного меню.
  /// Класс содержит методы для добавления команд и методы выполнения команд
  /// </summary>
  public class EFPAppCommandItemHelpers
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой объект
    /// </summary>
    public EFPAppCommandItemHelpers()
    {
      _CommandItems = EFPApp.CommandItems;

      _ToolBarVisibleItems = new Dictionary<string, EFPCommandItem>();

      _WindowListCount = 9;

      EFPApp.InterfaceChanged += new EventHandler(EFPApp_InterfaceChanged);
    }

    void EFPApp_InterfaceChanged(object sender, EventArgs args)
    {
      try
      {
        InitCommandsView();
        InitCommandsWindow();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка инициализации состояния команд главного меню при переключении интерфейса");
      }
    }

    /// <summary>
    /// Дублирует свойство EFPApp.CommandItems
    /// </summary>
    public EFPAppCommandItems CommandItems { get { return _CommandItems; } }
    private EFPAppCommandItems _CommandItems;

    #endregion

    #region Меню "Файл"

    /// <summary>
    /// Команда "Файл"-"Выход".
    /// Свойство задано, если был вызван метод AddExit()
    /// </summary>
    public EFPCommandItem Exit { get { return _Exit; } }
    private EFPCommandItem _Exit;

    /// <summary>
    /// Добавление команды "Файл"-"Выход"
    /// </summary>
    /// <param name="menuFile">Меню "Файл"</param>
    public void AddExit(EFPCommandItem menuFile)
    {
      _Exit = CommandItems.Add(EFPAppStdCommandItems.Exit, menuFile);
      _Exit.Click += new EventHandler(Exit_Click);
    }

    /// <summary>
    /// Завершение работы приложения вызовом EFPApp.Exit()
    /// </summary>
    /// <param name="sender">Игнорируется</param>
    /// <param name="args">Игнорируется</param>
    public static void Exit_Click(object sender, EventArgs args)
    {
      Application.Exit();
    }

    #endregion

    #region Меню "Вид"

    /// <summary>
    /// Добавляет подменю "Панели инструментов" и команду "Статусная строка"
    /// в меню "Вид"
    /// </summary>
    /// <param name="menuView">Созданное меню "Вид" в главном меню</param>
    public void AddViewMenuCommands(EFPCommandItem menuView)
    {
      if (EFPApp.ToolBars.Count > 0)
        AddToolBarSubMenu(menuView);
      AddStatusBarVisible(menuView);
    }

    #region "Панели инструментов"

    /// <summary>
    /// Добавляет подменю "Панели инструментов".
    /// В подменю создается по одной команде на каждую панель инструментов.
    /// Также добавляется команда "Восстановить"
    /// </summary>
    /// <param name="menuView">Созданное меню "Вид" в главном меню</param>
    /// <returns>Подменю "Панели инструментов"</returns>
    public EFPCommandItem AddToolBarSubMenu(EFPCommandItem menuView)
    {
      //// Вид - Панели инструментов
      EFPCommandItem MenuViewToolBars = new EFPCommandItem("View", "ToolBars");
      MenuViewToolBars.MenuText = "Панели инструментов";
      MenuViewToolBars.Parent = menuView;
      MenuViewToolBars.GroupBegin = true;
      _CommandItems.Add(MenuViewToolBars);

      for (int i = 0; i < EFPApp.ToolBars.Count; i++)
        AddToolBarVisible(MenuViewToolBars, EFPApp.ToolBars[i]);

      MenuViewToolBars.Children.AddSeparator();
      AddToolBarsRestore(MenuViewToolBars);

      return MenuViewToolBars;
    }

    #region Видимость панели инструментов

    /// <summary>
    /// Добавляет команду управления видимостью одной панелью инструментов
    /// </summary>
    /// <param name="menuViewToolBars">Созданное меню "Панели инструментов"</param>
    /// <param name="toolBar">Панель инструментов, которая будет скрываться / показываться командой</param>
    /// <returns>Созданная команда меню</returns>
    public EFPCommandItem AddToolBarVisible(EFPCommandItem menuViewToolBars, EFPAppToolBarCommandItems toolBar)
    {
      EFPCommandItem ci = new EFPCommandItem("ToolBar", toolBar.Name);
      ci.MenuText = toolBar.DisplayName;
      ci.Parent = menuViewToolBars;
      ci.Click += new EventHandler(ToolBarVisible_Click);
      ci.Tag = toolBar.Name;

      _CommandItems.Add(ci);
      _ToolBarVisibleItems.Add(toolBar.Name, ci);
      return ci;
    }

    /// <summary>
    /// Добавленные команды видимости панелей инструментов.
    /// Ключ - свойство EFPAppToolBarCommandItems.Name, 
    /// Значение - команда управления видимостью
    /// </summary>
    public IDictionary<string, EFPCommandItem> ToolBarVisibleItems
    {
      get { return _ToolBarVisibleItems; }
    }
    private Dictionary<string, EFPCommandItem> _ToolBarVisibleItems;

    private static void ToolBarVisible_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      string Name = (string)(ci.Tag);

      if (MainWindowActive)
      {
        EFPAppToolBar tb = EFPApp.Interface.CurrentMainWindowLayout.ToolBars[Name];
        if (tb == null)
          return; // бяка какая-то
        tb.Visible = !tb.Visible;
        ci.Checked = tb.Visible;
      }
    }

    #endregion

    #region Команда "Восстановить"

    /// <summary>
    /// Команда "Вид - Панели инструментов - Восстановить".
    /// Свойство инициализируется методом AddRestoreToolBars()
    /// </summary>
    public EFPCommandItem RestoreToolBars { get { return _RestoreToolBars; } }
    private EFPCommandItem _RestoreToolBars;

    /// <summary>
    /// Создает команду "Вид - Панели инструментов - Восстановить"
    /// </summary>
    /// <param name="menuViewToolBars">Подменю "Вид - Панели инструментов" для
    /// добавления команды</param>
    public void AddToolBarsRestore(EFPCommandItem menuViewToolBars)
    {
      _RestoreToolBars = new EFPCommandItem("View", "RestoreToolBars");
      _RestoreToolBars.Parent = menuViewToolBars;
      _RestoreToolBars.MenuText = "Восстановить";
      _RestoreToolBars.Click += new EventHandler(RestoreToolBars_Click);
      CommandItems.Add(_RestoreToolBars);
    }

    private static int _RestoreToolBarsAllWindowsMode = 0;

    internal static void RestoreToolBars_Click(object sender, EventArgs args)
    {
      if (!MainWindowActive)
        return;

      bool All = false;
      if (EFPApp.Interface.MainWindowCount > 1)
      {
        RadioSelectDialog dlg = new RadioSelectDialog();
        dlg.Title = "Восстановление панелей инструментов";
        dlg.Items = new string[] { "Только для текущего окна", "Для всех открытых окон" };
        dlg.SelectedIndex = _RestoreToolBarsAllWindowsMode;
        if (dlg.ShowDialog() != DialogResult.OK)
          return;
        _RestoreToolBarsAllWindowsMode = dlg.SelectedIndex;
        All = (dlg.SelectedIndex == 1);
      }
      if (All)
      {
        foreach (EFPAppMainWindowLayout Layout in EFPApp.Interface)
          Layout.RestoreToolBars();
      }
      else
        EFPApp.Interface.CurrentMainWindowLayout.RestoreToolBars();
    }

    #endregion

    #endregion

    #region "Статусная строка"

    /// <summary>
    /// Команда "Вид - Статусная строка"
    /// </summary>
    public EFPCommandItem StatusBarVisible { get { return _StatusBarVisible; } }
    private EFPCommandItem _StatusBarVisible;

    /// <summary>
    /// Создает команду "Вид - Статусная строка"
    /// </summary>
    /// <param name="menuView">Созданное меню "Вид"</param>
    public void AddStatusBarVisible(EFPCommandItem menuView)
    {
      _StatusBarVisible = new EFPCommandItem("View", "StatusBar");
      _StatusBarVisible.MenuText = "Статусная строка";
      _StatusBarVisible.Parent = menuView;
      _StatusBarVisible.Click += new EventHandler(StatusBarVisible_Click);
      _CommandItems.Add(_StatusBarVisible);
    }

    /// <summary>
    /// Переключение видимости статусной строки
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private static void StatusBarVisible_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      if (MainWindowActive)
      {
        //EFPApp.MessageBox(WinFormsTools.GetControls<StatusStrip>(EFPApp.Interface.CurrentMainWindowLayout.MainWindow).Length.ToString(), "До");

        //DebugTools.DebugControls(EFPApp.Interface.CurrentMainWindowLayout.MainWindow);

        EFPApp.Interface.CurrentMainWindowLayout.StatusBar.Visible = !EFPApp.Interface.CurrentMainWindowLayout.StatusBar.Visible;

        //EFPApp.MessageBox(WinFormsTools.GetControls<StatusStrip>(EFPApp.Interface.CurrentMainWindowLayout.MainWindow).Length.ToString(), "После");

        ci.Checked = EFPApp.Interface.CurrentMainWindowLayout.StatusBar.Visible;
      }
    }

    private static bool MainWindowActive
    {
      get
      {
        if (EFPApp.Interface == null)
          return false;
        else
          return EFPApp.Interface.CurrentMainWindowLayout != null;
      }
    }

    #endregion

    #region Инициализация видимости

    private void InitCommandsView()
    {
      foreach (KeyValuePair<string, EFPCommandItem> Pair in ToolBarVisibleItems)
      {
        if (MainWindowActive)
        {
          Pair.Value.Visible = EFPApp.Interface.CurrentMainWindowLayout.ToolBars.Contains(Pair.Key);
          if (Pair.Value.Visible)
            Pair.Value.Checked = EFPApp.Interface.CurrentMainWindowLayout.ToolBars[Pair.Key].Visible;
        }
        else
          Pair.Value.Visible = false;
      }

      if (_StatusBarVisible != null)
      {
        if (MainWindowActive)
        {
          _StatusBarVisible.Visible = true;
          _StatusBarVisible.Checked = EFPApp.Interface.CurrentMainWindowLayout.StatusBar.Visible;
        }
        else
          _StatusBarVisible.Visible = false;
      }
    }

    #endregion

    #endregion

    #region Меню "Окно"

    /// <summary>
    /// Добавляет команды "Сверху вниз", "Слева направо", "Каскадом", "Упорядочить значки", 
    /// "Новое главное окно", "Сохраненные положения окон" и список для переключения окон в меню "Окно"
    /// Наличие и видимость команд определяется используемым интерфейсом
    /// </summary>
    /// <param name="menuWindow">Меню "Окно", в которое добавляются команды</param>
    public void AddWindowMenuCommands(EFPCommandItem menuWindow)
    {
      AddTileHorizontal(menuWindow);
      AddTileVertical(menuWindow);
      AddCascade(menuWindow);
      AddArrangeIcons(menuWindow);
      if (menuWindow != null)
        menuWindow.Children.AddSeparator();
      else
        EFPApp.CommandItems.AddSeparator();

      AddCloseAll(menuWindow);
      AddCloseAllButThis(menuWindow);
      AddNewMainWindow(menuWindow);
      //if (EFPApp.CompositionHistoryCount > 0)
      // всегда добавляем
      AddSavedCompositions(menuWindow);

      if (menuWindow != null)
        menuWindow.Children.AddSeparator();
      else
        EFPApp.CommandItems.AddSeparator();

      AddWindowList(menuWindow);
      AddOtherWindows(menuWindow);
    }


    #region "Сверху вниз"

    /// <summary>
    /// Команда "Сверху вниз".
    /// Свойство задано, если был вызван метод AddTileHorizontal()
    /// </summary>
    public EFPCommandItem TileHorizontal { get { return _TileHorizontal; } }
    private EFPCommandItem _TileHorizontal;

    /// <summary>
    /// Создать команду "Сверху вниз"
    /// </summary>
    /// <param name="menuWindow">Меню "Окно", в которое добавляется команда</param>
    public void AddTileHorizontal(EFPCommandItem menuWindow)
    {
      _TileHorizontal = CommandItems.Add(EFPAppStdCommandItems.TileHorizontal, menuWindow);
      _TileHorizontal.Click += new EventHandler(TileHorizontal_Click);
    }

    /// <summary>
    /// Размещение окон сверху вниз
    /// </summary>
    /// <param name="sender">Игнорируется</param>
    /// <param name="args">Игнорируется</param>
    public static void TileHorizontal_Click(object sender, EventArgs args)
    {
      if (MainWindowActive)
        EFPApp.Interface.CurrentMainWindowLayout.LayoutChildForms(MdiLayout.TileHorizontal);
    }

    #endregion

    #region "Слева направо"

    /// <summary>
    /// Команда "Слева направо".
    /// Свойство задано, если был вызван метод AddTileVertical()
    /// </summary>
    public EFPCommandItem TileVertical { get { return _TileVertical; } }
    private EFPCommandItem _TileVertical;

    /// <summary>
    /// Создать команду "Слева направо"
    /// </summary>
    /// <param name="menuWindow">Меню "Окно", в которое добавляется команда</param>
    public void AddTileVertical(EFPCommandItem menuWindow)
    {
      _TileVertical = CommandItems.Add(EFPAppStdCommandItems.TileVertical, menuWindow);
      _TileVertical.Click += new EventHandler(TileVertical_Click);
    }

    /// <summary>
    /// Размещение окон слева направо
    /// </summary>
    /// <param name="sender">Игнорируется</param>
    /// <param name="args">Игнорируется</param>
    public static void TileVertical_Click(object sender, EventArgs args)
    {
      if (MainWindowActive)
        EFPApp.Interface.CurrentMainWindowLayout.LayoutChildForms(MdiLayout.TileVertical);
    }

    #endregion

    #region "Каскадом"

    /// <summary>
    /// Команда "Каскадом".
    /// Свойство задано, если был вызван метод AddCascade()
    /// </summary>
    public EFPCommandItem Cascade { get { return _Cascade; } }
    private EFPCommandItem _Cascade;

    /// <summary>
    /// Создать команду "Каскадом"
    /// </summary>
    /// <param name="menuWindow">Меню "Окно", в которое добавляется команда</param>
    public void AddCascade(EFPCommandItem menuWindow)
    {
      _Cascade = CommandItems.Add(EFPAppStdCommandItems.Cascade, menuWindow);
      _Cascade.Click += new EventHandler(Cascade_Click);
    }

    /// <summary>
    /// Размещение окон каскадом
    /// </summary>
    /// <param name="sender">Игнорируется</param>
    /// <param name="args">Игнорируется</param>
    public static void Cascade_Click(object sender, EventArgs args)
    {
      if (MainWindowActive)
        EFPApp.Interface.CurrentMainWindowLayout.LayoutChildForms(MdiLayout.Cascade);
    }

    #endregion

    #region "Упорядочить значки"

    /// <summary>
    /// Команда "Упорядочить значки"
    /// Свойство задано, если был вызван метод AddArrangeIcons()
    /// </summary>
    public EFPCommandItem ArrangeIcons { get { return _ArrangeIcons; } }
    private EFPCommandItem _ArrangeIcons;

    /// <summary>
    /// Добавление команды "Упорядочить значки"
    /// </summary>
    /// <param name="menuWindow">Меню "Окно", в которое добавляется команда</param>
    public void AddArrangeIcons(EFPCommandItem menuWindow)
    {
      _ArrangeIcons = CommandItems.Add(EFPAppStdCommandItems.ArrangeIcons, menuWindow);
      _ArrangeIcons.Click += new EventHandler(ArrangeIcons_Click);
    }

    /// <summary>
    /// Упорядочение значков свернутых окон
    /// </summary>
    /// <param name="sender">Игнорируется</param>
    /// <param name="args">Игнорируется</param>
    public static void ArrangeIcons_Click(object sender, EventArgs args)
    {
      if (MainWindowActive)
        EFPApp.Interface.CurrentMainWindowLayout.LayoutChildForms(MdiLayout.ArrangeIcons);
    }

    #endregion

    #region "Закрыть все"

    /// <summary>
    /// Команда "Закрыть все".
    /// Свойство задано, если был вызван метод AddCloseAll()
    /// </summary>
    public EFPCommandItem CloseAll { get { return _CloseAll; } }
    private EFPCommandItem _CloseAll;

    /// <summary>
    /// Добавить команду "Закрыть все"
    /// </summary>
    /// <param name="menuWindow">Меню "Окно", в которое добавляется команда</param>
    public void AddCloseAll(EFPCommandItem menuWindow)
    {
      _CloseAll = CommandItems.Add(EFPAppStdCommandItems.CloseAll, menuWindow);
      _CloseAll.Click += new EventHandler(CloseAll_Click);
    }

    /// <summary>
    /// Закрытие дочерних окон MDI
    /// </summary>
    /// <param name="sender">Игнорируется</param>
    /// <param name="args">Игнорируется</param>
    public static void CloseAll_Click(object sender, EventArgs args)
    {
      if (MainWindowActive)
        EFPApp.Interface.CurrentMainWindowLayout.CloseAllChildren();
    }

    #endregion

    #region "Закрыть все кроме текущего окна"

    /// <summary>
    /// Команда "Закрыть все кроме текущего окна".
    /// Свойство задано, если был вызван метод AddCloseAllButThis()
    /// </summary>
    public EFPCommandItem CloseAllButThis { get { return _CloseAllButThis; } }
    private EFPCommandItem _CloseAllButThis;

    /// <summary>
    /// Добавить команду "Закрыть все кроме текущего окна"
    /// </summary>
    /// <param name="menuWindow">Меню "Окно", в которое добавляется команда</param>
    public void AddCloseAllButThis(EFPCommandItem menuWindow)
    {
      _CloseAllButThis = CommandItems.Add(EFPAppStdCommandItems.CloseAllButThis, menuWindow);
      _CloseAllButThis.Click += new EventHandler(CloseAllButThis_Click);
    }

    /// <summary>
    /// Закрытие всех окон, кроме текущего
    /// </summary>
    /// <param name="sender">Игнорируется</param>
    /// <param name="args">Игнорируется</param>
    public static void CloseAllButThis_Click(object sender, EventArgs args)
    {
      if (MainWindowActive)
      {
        Form Curr = EFPApp.Interface.CurrentChildForm;
        if (Curr == null)
        {
          EFPApp.ShowTempMessage("Нет текущего окна");
          return;
        }

        // SingleScopeList<EFPAppMainWindowLayout> ClosedMainWindows = new SingleScopeList<EFPAppMainWindowLayout>();

        EFPApp.BeginUpdateInterface();
        try
        {
          Form[] AllChildren;
          if (EFPApp.Interface.IsSDI)
            AllChildren = EFPApp.Interface.GetChildForms(false);
          else
            AllChildren = EFPApp.Interface.CurrentMainWindowLayout.GetChildForms(false);

          for (int i = 0; i < AllChildren.Length; i++)
          {
            if (Object.ReferenceEquals(AllChildren[i], Curr))
              continue;
            //ClosedMainWindows.Add(EFPApp.Interface.FindMainWindowLayout())
            AllChildren[i].Close();
          }
        }
        finally
        {
          EFPApp.EndUpdateInterface();
        }
      }
    }

    #endregion

    #region "Новое главное окно"

    /// <summary>
    /// Команда "Новое главное окно".
    /// Свойство задано, если был вызван метод AddNewMainWindow()
    /// </summary>
    public EFPCommandItem NewMainWindow { get { return _NewMainWindow; } }
    private EFPCommandItem _NewMainWindow;

    /// <summary>
    /// Добавить команду "Новое главное окно"
    /// </summary>
    /// <param name="menuWindow">Меню "Окно", в которое добавляется команда</param>
    public void AddNewMainWindow(EFPCommandItem menuWindow)
    {
      _NewMainWindow = CommandItems.Add(EFPAppStdCommandItems.NewMainWindow, menuWindow);
      _NewMainWindow.Click += new EventHandler(NewMainWindow_Click);
    }

    /// <summary>
    /// Обработчик команды "Новое главное окно.
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Не используется</param>
    public static void NewMainWindow_Click(object sender, EventArgs args)
    {
      if (EFPApp.Interface != null)
        EFPApp.Interface.ShowMainWindow();
    }

    #endregion

    #region "Сохраненные положения окон"

    /// <summary>
    /// Команда "Сохраненные положения оконвниз".
    /// Свойство задано, если был вызван метод AddSavedCompositions()
    /// </summary>
    public EFPCommandItem SavedCompositions { get { return _SavedCompositions; } }
    private EFPCommandItem _SavedCompositions;

    /// <summary>
    /// Добавить команду "Сохраненные положения окон"
    /// </summary>
    /// <param name="menuWindow">Меню "Окно", в которое добавляется команда</param>
    public void AddSavedCompositions(EFPCommandItem menuWindow)
    {
      _SavedCompositions = CommandItems.Add(EFPAppStdCommandItems.SavedCompositions, menuWindow);
      _SavedCompositions.Click += new EventHandler(SavedCompositions_Click);
    }

    /// <summary>
    /// Контекст справки для диалога "Композиции рабочего стола" 
    /// </summary>
    public string SelectCompositionDialogHelpContext { get { return _SelectCompositionDialogHelpContext; } set { _SelectCompositionDialogHelpContext = value; } }
    private string _SelectCompositionDialogHelpContext;

    /// <summary>
    /// Вывод диалога "Композиции рабочего стола" с помощью SelectCompositionDialog.
    /// Устанавливается свойство HelpContext = SelectCompositionDialogHelpContext
    /// </summary>
    /// <param name="sender">Игнорируется</param>
    /// <param name="args">Игнорируется</param>
    private void SavedCompositions_Click(object sender, EventArgs args)
    {
      SelectCompositionDialog dlg = new SelectCompositionDialog();
      dlg.HelpContext = SelectCompositionDialogHelpContext;
      dlg.ShowDialog();
    }

    #endregion

    #region Список открытых окон

    /// <summary>
    /// Количество команд для переключения между окнами в меню "Окно".
    /// По умолчанию - 9 команд. Менять это значение не следует, т.к. оно принято для интерфейса MDI
    /// </summary>
    public int WindowListCount
    {
      get { return _WindowListCount; }
      set
      {
        if (_WindowListItems != null)
          throw new InvalidOperationException("Список уже был создан");
        if (value < 0 || value > 100)
          throw new ArgumentOutOfRangeException();
      }
    }
    private int _WindowListCount;

    /// <summary>
    /// Команды для переключения между окнами для меню "Окно".
    /// После создания команд длина массива будет равна WindowListCount
    /// </summary>
    public EFPCommandItem[] WindowListItems { get { return _WindowListItems; } }
    private EFPCommandItem[] _WindowListItems;

    /// <summary>
    /// Добавить команды переключания между окнами.
    /// Количество команд определяется свойством WindowListCount.
    /// Если WindowListCount=0, то метод ничего не делает.
    /// </summary>
    /// <param name="menuWindow">Меню "Окно", в которое добавляется команда</param>
    public void AddWindowList(EFPCommandItem menuWindow)
    {
      if (WindowListCount == 0)
        return;
      _WindowListItems = new EFPCommandItem[WindowListCount];
      for (int i = 0; i < _WindowListItems.Length; i++)
      {
        _WindowListItems[i] = new EFPCommandItem("Window", "WindowItem" + (i + 1).ToString());

        // 07.06.2021
        // Префикс с номером окна "&1" - "&9" (и дальше) с пробелом является фиксированным.
        // Он сохраняется при обновлении текста команды
        string s = (i + 1).ToString() + " Unknown window";
        if (i < 9)
          s = "&" + s;
        _WindowListItems[i].MenuText = s;

        _WindowListItems[i].Parent = menuWindow;
        _WindowListItems[i].Click += WindowListItem_Click;
        _WindowListItems[i].Usage = EFPCommandItemUsage.Menu; // на случай значков
        _WindowListItems[i].MenuOpening += new EventHandler(WindowListItem_MenuOpening); // 07.06.2021
        CommandItems.Add(_WindowListItems[i]);
      }
    }

    /// <summary>
    /// Отладочный режим для показа идентификаторов окон в списке.
    /// По умолчанию - false - выключен
    /// </summary>
    public bool DebugShowHWND
    {
      get { return _DebugShowHWND; }
      set
      {
        if (value == _DebugShowHWND)
          return;
        _DebugShowHWND = value;
        if (_WindowListItems == null)
          return;
        InitCommandsWindow();
      }
    }
    private bool _DebugShowHWND;

    private void WindowListItem_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;

      Form Form = ci.Tag as Form;

      if (Form == null)
        EFPApp.ErrorMessageBox("Форма не присоединена к команде");
      else
        EFPApp.Activate(Form); // 07.06.2021
    }

    void WindowListItem_MenuOpening(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;

      int p = ci.MenuText.IndexOf(' '); // номерок и пробел
#if DEBUG
      if (p < 0 || p > 3)
        throw new BugException("Несанкционированное изменение текста команды меню");
#endif


      Form frm = ci.Tag as Form;
      if (frm == null)
      {
        ci.MenuText = ci.MenuText.Substring(0, p + 1) + "?";
        ci.MenuRightText = String.Empty;
      }
      else
      {
        string s = ci.MenuText.Substring(0, p + 1) + frm.Text; // Цифра и пробел сохранены
        if (EFPApp.IsMinimized(frm))
          s += " (свернуто)";

        if (DebugShowHWND)
          s += " (HWND=" + frm.Handle.ToString() + ")";

        ci.MenuText = s;
        ci.MenuRightText = EFPApp.GetMainWindowNumberText(frm);

        if (EFPApp.ShowListImages)
          ci.Image = EFPApp.GetFormIconImage(frm);
      }
    }


    #endregion

    #region "Другие окна"

    /// <summary>
    /// Команда "Другие окна"
    /// </summary>
    public EFPCommandItem OtherWindows { get { return _OtherWindows; } }
    private EFPCommandItem _OtherWindows;

    /// <summary>
    /// Добавить команду "Другие окна"
    /// </summary>
    /// <param name="menuWindow">Меню "Окно", в которое добавляется команда</param>
    public void AddOtherWindows(EFPCommandItem menuWindow)
    {
      _OtherWindows = CommandItems.Add(EFPAppStdCommandItems.OtherWindows, menuWindow);
      _OtherWindows.GroupBegin = true;
      _OtherWindows.GroupEnd = true;
      _OtherWindows.Click += new EventHandler(OtherWindows_Click);
    }

    private void OtherWindows_Click(object sender, EventArgs args)
    {
      EFPApp.ShowChildFormListDialog(DebugShowHWND);
    }

    #endregion

    #region Инициализация видимости

    private void InitCommandsWindow()
    {
      bool Visible, Enabled;

      if (TileHorizontal != null)
      {
        IsLayoutChildFormsSupported(MdiLayout.TileHorizontal, out Visible, out Enabled);
        TileHorizontal.Visible = Visible;
        TileHorizontal.Enabled = Enabled;
      }

      if (TileVertical != null)
      {
        IsLayoutChildFormsSupported(MdiLayout.TileVertical, out Visible, out Enabled);
        TileVertical.Visible = Visible;
        TileVertical.Enabled = Enabled;
      }

      if (Cascade != null)
      {
        IsLayoutChildFormsSupported(MdiLayout.Cascade, out Visible, out Enabled);
        Cascade.Visible = Visible;
        Cascade.Enabled = Enabled;
      }

      if (ArrangeIcons != null)
      {
        IsLayoutChildFormsSupported(MdiLayout.ArrangeIcons, out Visible, out Enabled);
        ArrangeIcons.Visible = Visible;
        ArrangeIcons.Enabled = Enabled;
      }

      if (CloseAll != null)
      {
        if (EFPApp.Interface == null)
          CloseAll.Visible = false;
        else if (EFPApp.Interface.IsSDI)
          CloseAll.Visible = false;
        else
        {
          CloseAll.Visible = true;
          CloseAll.Enabled = EFPApp.Interface.CurrentChildForm != null;
        }
      }

      if (CloseAllButThis != null)
      {
        if (EFPApp.Interface == null)
          CloseAllButThis.Visible = false;
        else
        {
          CloseAllButThis.Visible = true;
          if (EFPApp.Interface.CurrentMainWindowLayout == null)
            CloseAllButThis.Enabled = false;
          else
          {
            if (EFPApp.Interface.IsSDI)
              CloseAllButThis.Enabled = EFPApp.Interface.ChildFormCount > 1;
            else
              CloseAllButThis.Enabled = EFPApp.Interface.CurrentMainWindowLayout.ChildFormCount > 1;
          }
        }
      }

      if (NewMainWindow != null)
      {
        if (EFPApp.Interface == null)
          NewMainWindow.Visible = false;
        else
          NewMainWindow.Visible = !EFPApp.Interface.IsSDI;
      }

      if (SavedCompositions != null)
        SavedCompositions.Visible = EFPApp.CompositionHistoryCount > 0;

      InitWindowListItems();
    }

    /// <summary>
    /// Инициализация списка в меню "Окно", включая команду "Другие окна"
    /// </summary>
    private void InitWindowListItems()
    {
      if (WindowListItems != null)
      {
        if (EFPApp.Interface == null)
        {
          for (int i = 0; i < WindowListItems.Length; i++)
            ClearWindowListItem(i);
        }
        else
        {
          Form[] Forms = EFPApp.Interface.GetChildForms(false);
          Form Curr = EFPApp.Interface.CurrentChildForm;
          bool CurrFound = false;
          for (int i = 0; i < WindowListItems.Length; i++)
          {
            if (i >= Forms.Length)
              ClearWindowListItem(i);
            else
            {
              InitWindowListItem(i, Forms[i], Object.ReferenceEquals(Forms[i], Curr));
              if (WindowListItems[i].Checked)
                CurrFound = true;
            }
          }

          // Как принято в MDI, если текущая форма не попала в список, ее надо сделать последним элементом
          if ((!CurrFound) && WindowListItems.Length > 0 && (!Object.ReferenceEquals(Curr, null)))
          {
            int i = WindowListItems.Length - 1;
            InitWindowListItem(i, Curr, true);
          }
        }
      }

      if (OtherWindows != null)
      {
        if (EFPApp.Interface == null)
          OtherWindows.Visible = false;
        else
          OtherWindows.Visible = EFPApp.Interface.ChildFormCount > WindowListCount;
      }
    }

    private static void IsLayoutChildFormsSupported(MdiLayout Layout, out bool Visible, out bool Enabled)
    {
      if (EFPApp.Interface == null)
      {
        Visible = false;
        Enabled = false;
      }
      else
      {
        Visible = EFPApp.Interface.IsLayoutChildFormsSupported(Layout);
        if (Visible)
          Enabled = EFPApp.Interface.IsLayoutChildFormsAppliable(Layout);
        else
          Enabled = false;
      }
    }

    private void InitWindowListItem(int i, Form Form, bool IsCurrent)
    {
      WindowListItems[i].Checked = IsCurrent;
      WindowListItems[i].Visible = true;
      WindowListItems[i].Tag = Form;

      // 07.06.2021 Текст и значок обновляется при открытии меню
      // Только, событие MenuStrip.MenuActivate, похоже, не всегда вызывается.
      // Продублируем инициализацию
      WindowListItem_MenuOpening(WindowListItems[i], null);
    }

    private void ClearWindowListItem(int i)
    {
      WindowListItems[i].Visible = false;
      WindowListItems[i].Tag = null;
    }

    #endregion

    #endregion

    #region Меню "Справка"

    /// <summary>
    /// Команда "Справка"-"О программе".
    /// Свойство задано, если был вызван метод AddAbout()
    /// </summary>
    public EFPCommandItem About { get { return _About; } }
    private EFPCommandItem _About;

    /// <summary>
    /// Добавление команды "Справка"-"О программе"
    /// </summary>
    /// <param name="menuHelp">Меню "Справка"</param>
    public void AboutExit(EFPCommandItem menuHelp)
    {
      _About = CommandItems.Add(EFPAppStdCommandItems.About, menuHelp);
      _About.Click += new EventHandler(About_Click);
    }

    /// <summary>
    /// Показ диалога "О программе" с помошью EFPApp.ShowAboutDialog()
    /// </summary>
    /// <param name="sender">Игнорируется</param>
    /// <param name="args">Игнорируется</param>
    public static void About_Click(object sender, EventArgs args)
    {
      EFPApp.ShowAboutDialog();
    }

    #endregion
  }
}
