using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// ���������� ����������� ������ �������� ����.
  /// ����� �������� ������ ��� ���������� ������ � ������ ���������� ������
  /// </summary>
  public class EFPAppCommandItemHelpers
  {
    #region �����������

    /// <summary>
    /// ������� ������ ������
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
        EFPApp.ShowException(e, "������ ������������� ��������� ������ �������� ���� ��� ������������ ����������");
      }
    }

    /// <summary>
    /// ��������� �������� EFPApp.CommandItems
    /// </summary>
    public EFPAppCommandItems CommandItems { get { return _CommandItems; } }
    private EFPAppCommandItems _CommandItems;

    #endregion

    #region ���� "����"

    /// <summary>
    /// ������� "����"-"�����".
    /// �������� ������, ���� ��� ������ ����� AddExit()
    /// </summary>
    public EFPCommandItem Exit { get { return _Exit; } }
    private EFPCommandItem _Exit;

    /// <summary>
    /// ���������� ������� "����"-"�����"
    /// </summary>
    /// <param name="menuFile">���� "����"</param>
    public void AddExit(EFPCommandItem menuFile)
    {
      _Exit = CommandItems.Add(EFPAppStdCommandItems.Exit, menuFile);
      _Exit.Click += new EventHandler(Exit_Click);
    }

    /// <summary>
    /// ���������� ������ ���������� ������� EFPApp.Exit()
    /// </summary>
    /// <param name="sender">������������</param>
    /// <param name="args">������������</param>
    public static void Exit_Click(object sender, EventArgs args)
    {
      Application.Exit();
    }

    #endregion

    #region ���� "���"

    /// <summary>
    /// ��������� ������� "������ ������������" � ������� "��������� ������"
    /// � ���� "���"
    /// </summary>
    /// <param name="menuView">��������� ���� "���" � ������� ����</param>
    public void AddViewMenuCommands(EFPCommandItem menuView)
    {
      if (EFPApp.ToolBars.Count > 0)
        AddToolBarSubMenu(menuView);
      AddStatusBarVisible(menuView);
    }

    #region "������ ������������"

    /// <summary>
    /// ��������� ������� "������ ������������".
    /// � ������� ��������� �� ����� ������� �� ������ ������ ������������.
    /// ����� ����������� ������� "������������"
    /// </summary>
    /// <param name="menuView">��������� ���� "���" � ������� ����</param>
    /// <returns>������� "������ ������������"</returns>
    public EFPCommandItem AddToolBarSubMenu(EFPCommandItem menuView)
    {
      //// ��� - ������ ������������
      EFPCommandItem MenuViewToolBars = new EFPCommandItem("View", "ToolBars");
      MenuViewToolBars.MenuText = "������ ������������";
      MenuViewToolBars.Parent = menuView;
      MenuViewToolBars.GroupBegin = true;
      _CommandItems.Add(MenuViewToolBars);

      for (int i = 0; i < EFPApp.ToolBars.Count; i++)
        AddToolBarVisible(MenuViewToolBars, EFPApp.ToolBars[i]);

      MenuViewToolBars.Children.AddSeparator();
      AddToolBarsRestore(MenuViewToolBars);

      return MenuViewToolBars;
    }

    #region ��������� ������ ������������

    /// <summary>
    /// ��������� ������� ���������� ���������� ����� ������� ������������
    /// </summary>
    /// <param name="menuViewToolBars">��������� ���� "������ ������������"</param>
    /// <param name="toolBar">������ ������������, ������� ����� ���������� / ������������ ��������</param>
    /// <returns>��������� ������� ����</returns>
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
    /// ����������� ������� ��������� ������� ������������.
    /// ���� - �������� EFPAppToolBarCommandItems.Name, 
    /// �������� - ������� ���������� ����������
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
          return; // ���� �����-��
        tb.Visible = !tb.Visible;
        ci.Checked = tb.Visible;
      }
    }

    #endregion

    #region ������� "������������"

    /// <summary>
    /// ������� "��� - ������ ������������ - ������������".
    /// �������� ���������������� ������� AddRestoreToolBars()
    /// </summary>
    public EFPCommandItem RestoreToolBars { get { return _RestoreToolBars; } }
    private EFPCommandItem _RestoreToolBars;

    /// <summary>
    /// ������� ������� "��� - ������ ������������ - ������������"
    /// </summary>
    /// <param name="menuViewToolBars">������� "��� - ������ ������������" ���
    /// ���������� �������</param>
    public void AddToolBarsRestore(EFPCommandItem menuViewToolBars)
    {
      _RestoreToolBars = new EFPCommandItem("View", "RestoreToolBars");
      _RestoreToolBars.Parent = menuViewToolBars;
      _RestoreToolBars.MenuText = "������������";
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
        dlg.Title = "�������������� ������� ������������";
        dlg.Items = new string[] { "������ ��� �������� ����", "��� ���� �������� ����" };
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

    #region "��������� ������"

    /// <summary>
    /// ������� "��� - ��������� ������"
    /// </summary>
    public EFPCommandItem StatusBarVisible { get { return _StatusBarVisible; } }
    private EFPCommandItem _StatusBarVisible;

    /// <summary>
    /// ������� ������� "��� - ��������� ������"
    /// </summary>
    /// <param name="menuView">��������� ���� "���"</param>
    public void AddStatusBarVisible(EFPCommandItem menuView)
    {
      _StatusBarVisible = new EFPCommandItem("View", "StatusBar");
      _StatusBarVisible.MenuText = "��������� ������";
      _StatusBarVisible.Parent = menuView;
      _StatusBarVisible.Click += new EventHandler(StatusBarVisible_Click);
      _CommandItems.Add(_StatusBarVisible);
    }

    /// <summary>
    /// ������������ ��������� ��������� ������
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private static void StatusBarVisible_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      if (MainWindowActive)
      {
        //EFPApp.MessageBox(WinFormsTools.GetControls<StatusStrip>(EFPApp.Interface.CurrentMainWindowLayout.MainWindow).Length.ToString(), "��");

        //DebugTools.DebugControls(EFPApp.Interface.CurrentMainWindowLayout.MainWindow);

        EFPApp.Interface.CurrentMainWindowLayout.StatusBar.Visible = !EFPApp.Interface.CurrentMainWindowLayout.StatusBar.Visible;

        //EFPApp.MessageBox(WinFormsTools.GetControls<StatusStrip>(EFPApp.Interface.CurrentMainWindowLayout.MainWindow).Length.ToString(), "�����");

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

    #region ������������� ���������

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

    #region ���� "����"

    /// <summary>
    /// ��������� ������� "������ ����", "����� �������", "��������", "����������� ������", 
    /// "����� ������� ����", "����������� ��������� ����" � ������ ��� ������������ ���� � ���� "����"
    /// ������� � ��������� ������ ������������ ������������ �����������
    /// </summary>
    /// <param name="menuWindow">���� "����", � ������� ����������� �������</param>
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
      // ������ ���������
      AddSavedCompositions(menuWindow);

      if (menuWindow != null)
        menuWindow.Children.AddSeparator();
      else
        EFPApp.CommandItems.AddSeparator();

      AddWindowList(menuWindow);
      AddOtherWindows(menuWindow);
    }


    #region "������ ����"

    /// <summary>
    /// ������� "������ ����".
    /// �������� ������, ���� ��� ������ ����� AddTileHorizontal()
    /// </summary>
    public EFPCommandItem TileHorizontal { get { return _TileHorizontal; } }
    private EFPCommandItem _TileHorizontal;

    /// <summary>
    /// ������� ������� "������ ����"
    /// </summary>
    /// <param name="menuWindow">���� "����", � ������� ����������� �������</param>
    public void AddTileHorizontal(EFPCommandItem menuWindow)
    {
      _TileHorizontal = CommandItems.Add(EFPAppStdCommandItems.TileHorizontal, menuWindow);
      _TileHorizontal.Click += new EventHandler(TileHorizontal_Click);
    }

    /// <summary>
    /// ���������� ���� ������ ����
    /// </summary>
    /// <param name="sender">������������</param>
    /// <param name="args">������������</param>
    public static void TileHorizontal_Click(object sender, EventArgs args)
    {
      if (MainWindowActive)
        EFPApp.Interface.CurrentMainWindowLayout.LayoutChildForms(MdiLayout.TileHorizontal);
    }

    #endregion

    #region "����� �������"

    /// <summary>
    /// ������� "����� �������".
    /// �������� ������, ���� ��� ������ ����� AddTileVertical()
    /// </summary>
    public EFPCommandItem TileVertical { get { return _TileVertical; } }
    private EFPCommandItem _TileVertical;

    /// <summary>
    /// ������� ������� "����� �������"
    /// </summary>
    /// <param name="menuWindow">���� "����", � ������� ����������� �������</param>
    public void AddTileVertical(EFPCommandItem menuWindow)
    {
      _TileVertical = CommandItems.Add(EFPAppStdCommandItems.TileVertical, menuWindow);
      _TileVertical.Click += new EventHandler(TileVertical_Click);
    }

    /// <summary>
    /// ���������� ���� ����� �������
    /// </summary>
    /// <param name="sender">������������</param>
    /// <param name="args">������������</param>
    public static void TileVertical_Click(object sender, EventArgs args)
    {
      if (MainWindowActive)
        EFPApp.Interface.CurrentMainWindowLayout.LayoutChildForms(MdiLayout.TileVertical);
    }

    #endregion

    #region "��������"

    /// <summary>
    /// ������� "��������".
    /// �������� ������, ���� ��� ������ ����� AddCascade()
    /// </summary>
    public EFPCommandItem Cascade { get { return _Cascade; } }
    private EFPCommandItem _Cascade;

    /// <summary>
    /// ������� ������� "��������"
    /// </summary>
    /// <param name="menuWindow">���� "����", � ������� ����������� �������</param>
    public void AddCascade(EFPCommandItem menuWindow)
    {
      _Cascade = CommandItems.Add(EFPAppStdCommandItems.Cascade, menuWindow);
      _Cascade.Click += new EventHandler(Cascade_Click);
    }

    /// <summary>
    /// ���������� ���� ��������
    /// </summary>
    /// <param name="sender">������������</param>
    /// <param name="args">������������</param>
    public static void Cascade_Click(object sender, EventArgs args)
    {
      if (MainWindowActive)
        EFPApp.Interface.CurrentMainWindowLayout.LayoutChildForms(MdiLayout.Cascade);
    }

    #endregion

    #region "����������� ������"

    /// <summary>
    /// ������� "����������� ������"
    /// �������� ������, ���� ��� ������ ����� AddArrangeIcons()
    /// </summary>
    public EFPCommandItem ArrangeIcons { get { return _ArrangeIcons; } }
    private EFPCommandItem _ArrangeIcons;

    /// <summary>
    /// ���������� ������� "����������� ������"
    /// </summary>
    /// <param name="menuWindow">���� "����", � ������� ����������� �������</param>
    public void AddArrangeIcons(EFPCommandItem menuWindow)
    {
      _ArrangeIcons = CommandItems.Add(EFPAppStdCommandItems.ArrangeIcons, menuWindow);
      _ArrangeIcons.Click += new EventHandler(ArrangeIcons_Click);
    }

    /// <summary>
    /// ������������ ������� ��������� ����
    /// </summary>
    /// <param name="sender">������������</param>
    /// <param name="args">������������</param>
    public static void ArrangeIcons_Click(object sender, EventArgs args)
    {
      if (MainWindowActive)
        EFPApp.Interface.CurrentMainWindowLayout.LayoutChildForms(MdiLayout.ArrangeIcons);
    }

    #endregion

    #region "������� ���"

    /// <summary>
    /// ������� "������� ���".
    /// �������� ������, ���� ��� ������ ����� AddCloseAll()
    /// </summary>
    public EFPCommandItem CloseAll { get { return _CloseAll; } }
    private EFPCommandItem _CloseAll;

    /// <summary>
    /// �������� ������� "������� ���"
    /// </summary>
    /// <param name="menuWindow">���� "����", � ������� ����������� �������</param>
    public void AddCloseAll(EFPCommandItem menuWindow)
    {
      _CloseAll = CommandItems.Add(EFPAppStdCommandItems.CloseAll, menuWindow);
      _CloseAll.Click += new EventHandler(CloseAll_Click);
    }

    /// <summary>
    /// �������� �������� ���� MDI
    /// </summary>
    /// <param name="sender">������������</param>
    /// <param name="args">������������</param>
    public static void CloseAll_Click(object sender, EventArgs args)
    {
      if (MainWindowActive)
        EFPApp.Interface.CurrentMainWindowLayout.CloseAllChildren();
    }

    #endregion

    #region "������� ��� ����� �������� ����"

    /// <summary>
    /// ������� "������� ��� ����� �������� ����".
    /// �������� ������, ���� ��� ������ ����� AddCloseAllButThis()
    /// </summary>
    public EFPCommandItem CloseAllButThis { get { return _CloseAllButThis; } }
    private EFPCommandItem _CloseAllButThis;

    /// <summary>
    /// �������� ������� "������� ��� ����� �������� ����"
    /// </summary>
    /// <param name="menuWindow">���� "����", � ������� ����������� �������</param>
    public void AddCloseAllButThis(EFPCommandItem menuWindow)
    {
      _CloseAllButThis = CommandItems.Add(EFPAppStdCommandItems.CloseAllButThis, menuWindow);
      _CloseAllButThis.Click += new EventHandler(CloseAllButThis_Click);
    }

    /// <summary>
    /// �������� ���� ����, ����� ��������
    /// </summary>
    /// <param name="sender">������������</param>
    /// <param name="args">������������</param>
    public static void CloseAllButThis_Click(object sender, EventArgs args)
    {
      if (MainWindowActive)
      {
        Form Curr = EFPApp.Interface.CurrentChildForm;
        if (Curr == null)
        {
          EFPApp.ShowTempMessage("��� �������� ����");
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

    #region "����� ������� ����"

    /// <summary>
    /// ������� "����� ������� ����".
    /// �������� ������, ���� ��� ������ ����� AddNewMainWindow()
    /// </summary>
    public EFPCommandItem NewMainWindow { get { return _NewMainWindow; } }
    private EFPCommandItem _NewMainWindow;

    /// <summary>
    /// �������� ������� "����� ������� ����"
    /// </summary>
    /// <param name="menuWindow">���� "����", � ������� ����������� �������</param>
    public void AddNewMainWindow(EFPCommandItem menuWindow)
    {
      _NewMainWindow = CommandItems.Add(EFPAppStdCommandItems.NewMainWindow, menuWindow);
      _NewMainWindow.Click += new EventHandler(NewMainWindow_Click);
    }

    /// <summary>
    /// ���������� ������� "����� ������� ����.
    /// </summary>
    /// <param name="sender">�� ������������</param>
    /// <param name="args">�� ������������</param>
    public static void NewMainWindow_Click(object sender, EventArgs args)
    {
      if (EFPApp.Interface != null)
        EFPApp.Interface.ShowMainWindow();
    }

    #endregion

    #region "����������� ��������� ����"

    /// <summary>
    /// ������� "����������� ��������� ��������".
    /// �������� ������, ���� ��� ������ ����� AddSavedCompositions()
    /// </summary>
    public EFPCommandItem SavedCompositions { get { return _SavedCompositions; } }
    private EFPCommandItem _SavedCompositions;

    /// <summary>
    /// �������� ������� "����������� ��������� ����"
    /// </summary>
    /// <param name="menuWindow">���� "����", � ������� ����������� �������</param>
    public void AddSavedCompositions(EFPCommandItem menuWindow)
    {
      _SavedCompositions = CommandItems.Add(EFPAppStdCommandItems.SavedCompositions, menuWindow);
      _SavedCompositions.Click += new EventHandler(SavedCompositions_Click);
    }

    /// <summary>
    /// �������� ������� ��� ������� "���������� �������� �����" 
    /// </summary>
    public string SelectCompositionDialogHelpContext { get { return _SelectCompositionDialogHelpContext; } set { _SelectCompositionDialogHelpContext = value; } }
    private string _SelectCompositionDialogHelpContext;

    /// <summary>
    /// ����� ������� "���������� �������� �����" � ������� SelectCompositionDialog.
    /// ��������������� �������� HelpContext = SelectCompositionDialogHelpContext
    /// </summary>
    /// <param name="sender">������������</param>
    /// <param name="args">������������</param>
    private void SavedCompositions_Click(object sender, EventArgs args)
    {
      SelectCompositionDialog dlg = new SelectCompositionDialog();
      dlg.HelpContext = SelectCompositionDialogHelpContext;
      dlg.ShowDialog();
    }

    #endregion

    #region ������ �������� ����

    /// <summary>
    /// ���������� ������ ��� ������������ ����� ������ � ���� "����".
    /// �� ��������� - 9 ������. ������ ��� �������� �� �������, �.�. ��� ������� ��� ���������� MDI
    /// </summary>
    public int WindowListCount
    {
      get { return _WindowListCount; }
      set
      {
        if (_WindowListItems != null)
          throw new InvalidOperationException("������ ��� ��� ������");
        if (value < 0 || value > 100)
          throw new ArgumentOutOfRangeException();
      }
    }
    private int _WindowListCount;

    /// <summary>
    /// ������� ��� ������������ ����� ������ ��� ���� "����".
    /// ����� �������� ������ ����� ������� ����� ����� WindowListCount
    /// </summary>
    public EFPCommandItem[] WindowListItems { get { return _WindowListItems; } }
    private EFPCommandItem[] _WindowListItems;

    /// <summary>
    /// �������� ������� ������������ ����� ������.
    /// ���������� ������ ������������ ��������� WindowListCount.
    /// ���� WindowListCount=0, �� ����� ������ �� ������.
    /// </summary>
    /// <param name="menuWindow">���� "����", � ������� ����������� �������</param>
    public void AddWindowList(EFPCommandItem menuWindow)
    {
      if (WindowListCount == 0)
        return;
      _WindowListItems = new EFPCommandItem[WindowListCount];
      for (int i = 0; i < _WindowListItems.Length; i++)
      {
        _WindowListItems[i] = new EFPCommandItem("Window", "WindowItem" + (i + 1).ToString());

        // 07.06.2021
        // ������� � ������� ���� "&1" - "&9" (� ������) � �������� �������� �������������.
        // �� ����������� ��� ���������� ������ �������
        string s = (i + 1).ToString() + " Unknown window";
        if (i < 9)
          s = "&" + s;
        _WindowListItems[i].MenuText = s;

        _WindowListItems[i].Parent = menuWindow;
        _WindowListItems[i].Click += WindowListItem_Click;
        _WindowListItems[i].Usage = EFPCommandItemUsage.Menu; // �� ������ �������
        _WindowListItems[i].MenuOpening += new EventHandler(WindowListItem_MenuOpening); // 07.06.2021
        CommandItems.Add(_WindowListItems[i]);
      }
    }

    /// <summary>
    /// ���������� ����� ��� ������ ��������������� ���� � ������.
    /// �� ��������� - false - ��������
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
        EFPApp.ErrorMessageBox("����� �� ������������ � �������");
      else
        EFPApp.Activate(Form); // 07.06.2021
    }

    void WindowListItem_MenuOpening(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;

      int p = ci.MenuText.IndexOf(' '); // ������� � ������
#if DEBUG
      if (p < 0 || p > 3)
        throw new BugException("������������������� ��������� ������ ������� ����");
#endif


      Form frm = ci.Tag as Form;
      if (frm == null)
      {
        ci.MenuText = ci.MenuText.Substring(0, p + 1) + "?";
        ci.MenuRightText = String.Empty;
      }
      else
      {
        string s = ci.MenuText.Substring(0, p + 1) + frm.Text; // ����� � ������ ���������
        if (EFPApp.IsMinimized(frm))
          s += " (��������)";

        if (DebugShowHWND)
          s += " (HWND=" + frm.Handle.ToString() + ")";

        ci.MenuText = s;
        ci.MenuRightText = EFPApp.GetMainWindowNumberText(frm);

        if (EFPApp.ShowListImages)
          ci.Image = EFPApp.GetFormIconImage(frm);
      }
    }


    #endregion

    #region "������ ����"

    /// <summary>
    /// ������� "������ ����"
    /// </summary>
    public EFPCommandItem OtherWindows { get { return _OtherWindows; } }
    private EFPCommandItem _OtherWindows;

    /// <summary>
    /// �������� ������� "������ ����"
    /// </summary>
    /// <param name="menuWindow">���� "����", � ������� ����������� �������</param>
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

    #region ������������� ���������

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
    /// ������������� ������ � ���� "����", ������� ������� "������ ����"
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

          // ��� ������� � MDI, ���� ������� ����� �� ������ � ������, �� ���� ������� ��������� ���������
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

      // 07.06.2021 ����� � ������ ����������� ��� �������� ����
      // ������, ������� MenuStrip.MenuActivate, ������, �� ������ ����������.
      // ������������ �������������
      WindowListItem_MenuOpening(WindowListItems[i], null);
    }

    private void ClearWindowListItem(int i)
    {
      WindowListItems[i].Visible = false;
      WindowListItems[i].Tag = null;
    }

    #endregion

    #endregion

    #region ���� "�������"

    /// <summary>
    /// ������� "�������"-"� ���������".
    /// �������� ������, ���� ��� ������ ����� AddAbout()
    /// </summary>
    public EFPCommandItem About { get { return _About; } }
    private EFPCommandItem _About;

    /// <summary>
    /// ���������� ������� "�������"-"� ���������"
    /// </summary>
    /// <param name="menuHelp">���� "�������"</param>
    public void AboutExit(EFPCommandItem menuHelp)
    {
      _About = CommandItems.Add(EFPAppStdCommandItems.About, menuHelp);
      _About.Click += new EventHandler(About_Click);
    }

    /// <summary>
    /// ����� ������� "� ���������" � ������� EFPApp.ShowAboutDialog()
    /// </summary>
    /// <param name="sender">������������</param>
    /// <param name="args">������������</param>
    public static void About_Click(object sender, EventArgs args)
    {
      EFPApp.ShowAboutDialog();
    }

    #endregion
  }
}
