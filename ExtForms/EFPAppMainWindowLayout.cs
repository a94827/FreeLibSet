using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.Config;
using System.Drawing;
using AgeyevAV.Logging;

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// ������� ���� ����������
  /// </summary>
  public abstract class EFPAppMainWindowLayout : IEnumerable<Form>
  {
    #region �����������

    /// <summary>
    /// ������� ������, � �������� ���� �� ������������ ���� (MainWindow=null).
    /// </summary>
    public EFPAppMainWindowLayout()
    {
      _ChildForms = new ListWithMRU<Form>();
      _MainWindowNumberText = String.Empty;
      _WindowStateBeforeMinimized = FormWindowState.Normal;
    }

    #endregion

    #region ����� ��������

    /// <summary>
    /// ���������, � �������� ��������� ��� ������� ����
    /// </summary>
    public EFPAppInterface Interface
    {
      get { return _Interface; }
      internal set { _Interface = value; }
    }
    private EFPAppInterface _Interface;

    /// <summary>
    /// ���������� ��������� �������� ����, ���� �������� MainWindow �����������.
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      if (MainWindow == null)
        return "��� ����";
      else
        return MainWindow.Text;
    }

    #endregion

    #region ������� ����

    /// <summary>
    /// ������� ����, �������������� � �������� �������.
    /// ��� �������� �� ������� ������������ � ���������������� ����
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
          throw new InvalidOperationException("��������� ��������� ��������");
#endif
        _MainWindow = value;
        _MainWindow.SizeChanged += new EventHandler(MainWindow_SizeChanged);
        MainWindow_SizeChanged(null, null);
      }
    }
    private Form _MainWindow;

    /// <summary>
    /// ��������� ������� ������� ���� ��� ������� ��������� ����������
    /// </summary>
    /// <returns>true, ���� ���� ���� ������� �������</returns>
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
    /// ���������� true, ���� � ������ ������ ����������� ����� CloseMainWindow()
    /// </summary>
    public bool InsideCloseMainWindow { get { return _InsideCloseMainWindow; } }
    private bool _InsideCloseMainWindow;

    /// <summary>
    /// ���� � ���������� MDI ������� ������ ������ �������� ����,
    /// �� ������������ ������ "#1", "#2", ...
    /// ���� ���� ������ ���� �������� ����, ��� ��������� SDI, ������������ ������ ������
    /// </summary>
    public string MainWindowNumberText
    {
      get { return _MainWindowNumberText; }
      internal set { _MainWindowNumberText = value; }
    }
    private string _MainWindowNumberText;

    /// <summary>
    /// ���������� ��������� (Normal ��� Maximized), ������� ���� �� �����������.
    /// ���� � ������� ������ ���� �� ��������, ���������� Form.WindowState
    /// </summary>
    public FormWindowState WindowStateBeforeMinimized { get { return _WindowStateBeforeMinimized; } }
    private FormWindowState _WindowStateBeforeMinimized;

    #endregion

    #region �������� ����

    /// <summary>
    /// ������ � ���������� ��� Z-order
    /// </summary>
    private ListWithMRU<Form> _ChildForms;

    /// <summary>
    /// ���������� ������ �������� ����, ������������� ������ �����.
    /// ���� <paramref name="useZOrder"/>=true, �� ������ ����������� �� ������� ���������� ���� 
    /// � ������� ������� ���� (CurrentChildForm ����� ������ � ������).
    /// ���� <paramref name="useZOrder"/>=false, �� ������ ����������� �� ������� �������� ���� � ������ ������� ����
    /// ��� ���������� SDI ���������� ������ �� ����� ����� (������ �������� Form).
    /// ���� ��������� SDI ����� ���������, �� ����� ���������� ������ ������.
    /// </summary>
    /// <param name="useZOrder">����� �� ����������� ���� �� ������� ���������</param>
    /// <returns>������ ����</returns>
    public Form[] GetChildForms(bool useZOrder)
    {
      if (useZOrder)
        return _ChildForms.MRU.ToArray();
      else
        return _ChildForms.ToArray();
    }


    /// <summary>
    /// ���������� �������� �����, ������� �������� ������� (��� ���� ������� � ��������� ���) � �������� �������� ����.
    /// ��� SDI ������ ���������� ���� ������ Form.
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
    /// ���������� ���������� �������� ���� � ���� ����.
    /// ��� ���������� SDI ���������� 1, ���� ����� �� "��������".
    /// </summary>
    public int ChildFormCount { get { return _ChildForms.Count; } }

    /// <summary>
    /// ���������� true, ���� ����� ��������� � ����� �������� ���� (��� ���� �������� ������� �����)
    /// </summary>
    /// <param name="form">������� �����</param>
    /// <returns>������� �����</returns>
    public bool ContainsForm(Form form)
    {
      if (form == null)
        return false;
      if (form == _MainWindow)
        return true;
      foreach (Form Child in _ChildForms)
      {
        if (form == Child)
          return true;
      }
      return false;
    }

    /// <summary>
    /// ����� �������� ����� ��������� ������.
    /// ���������� ������ ��������� ����� ��� null
    /// </summary>
    /// <param name="formType">��� �����</param>
    /// <returns>��������� ����� ��� null</returns>
    public Form FindChildForm(Type formType)
    {
      Form[] Forms = GetChildForms(false);
      for (int i = 0; i < Forms.Length; i++)
      {
        if (Forms[i].GetType() == formType)
          return Forms[i];
      }
      return null;
    }

    /// <summary>
    /// ����� �������� ����� ��������� ������.
    /// ���������� ������ ��������� ����� ��� null
    /// </summary>
    /// <typeparam name="T">����� �����</typeparam>
    /// <returns>��������� ����� ��� null</returns>
    public T FindChildForm<T>()
      where T : Form
    {
      Form[] Forms = GetChildForms(false);
      for (int i = 0; i < Forms.Length; i++)
      {
        if (Forms[i] is T)
          return (T)(Forms[i]);
      }
      return null;
    }

    /// <summary>
    /// ����� ��� �������� ����� ��������� ������
    /// </summary>
    /// <param name="formType">��� �����</param>
    /// <returns>������ ����</returns>
    public Form[] FindChildForms(Type formType)
    {
      List<Form> List = new List<Form>();
      FindChildFormsInternal(List, formType);
      return List.ToArray();
    }

    internal void FindChildFormsInternal(List<Form> list, Type formType)
    {
      Form[] Forms = GetChildForms(false);
      for (int i = 0; i < Forms.Length; i++)
      {
        if (Forms[i].GetType() == formType)
          list.Add(Forms[i]);
      }
    }

    /// <summary>
    /// ����� ��� �������� ����� ��������� ������
    /// </summary>
    /// <typeparam name="T">����� �����</typeparam>
    /// <returns>������ ����</returns>
    public T[] FindChildForms<T>()
      where T : Form
    {
      List<T> List = new List<T>();
      FindChildFormsInternal<T>(List);
      return List.ToArray();
    }

    internal void FindChildFormsInternal<T>(List<T> list)
      where T : Form
    {
      Form[] Forms = GetChildForms(false);
      for (int i = 0; i < Forms.Length; i++)
      {
        if (Forms[i] is T)
          list.Add((T)(Forms[i]));
      }
    }

    /// <summary>
    /// ���������� ��������� ���� � �������������.
    /// ��������� ���� � ������ ChildForms � ������������ ����������� ����������� ����.
    /// </summary>
    /// <param name="form">�������� ����</param>
    protected void PrepareChildForm(Form form)
    {
      if (form == null)
        throw new ArgumentNullException("form");
      if (form.IsDisposed)
        throw new ObjectDisposedException("form");

      if (_ChildForms.Contains(form))
        throw new InvalidOperationException("����� " + form.ToString() + " ��� ���� � ������ ���� �������� ����");

      _ChildForms.Add(form);

      form.Activated += new EventHandler(Form_Activated);
      form.Enter += new EventHandler(Form_Activated); // �����
      form.VisibleChanged += new EventHandler(Form_VisibleChanged);
    }

    void Form_Activated(object sender, EventArgs args)
    {
      if (Interface == null)
        return; // 10.06.2021, ����� �� ������������� ��� ��������� ������

      Form Form = (Form)sender;
      if (Interface.CurrentChildForm == Form)
        return; // �� ����������



      _ChildForms.Touch(Form);
      Interface.MainWindowActivated(this); // ����� EFPApp ������� �������� ������� �����

      //System.Diagnostics.Trace.WriteLine("Activated ��� ����� " + Form.Text);

      EFPApp.TestInterfaceChanged();
    }

    void Form_VisibleChanged(object sender, EventArgs args)
    {
      if (Interface == null)
        return; // 10.06.2021, ����� �� ������������� ��� ��������� ������

      Form Form = (Form)sender;
      if (Form.Visible)
      {
        if (!_ChildForms.Contains(Form))
          _ChildForms.Add(Form);
      }
      else
      {
        _ChildForms.Remove(Form);
      }
      EFPApp.TestInterfaceChanged();
    }

    /// <summary>
    /// ����������� �������� ����.
    /// ��������� ������ ��� ���������� MDI
    /// </summary>
    /// <param name="mdiLayout">������ ������������</param>
    public virtual void LayoutChildForms(MdiLayout mdiLayout)
    {
      throw new InvalidOperationException("�� ����������� ��� ���������� " + Interface.Name);
    }

    /// <summary>
    /// ������� ��� �������� ����.
    /// ��� SDI ����������� ������������ ����
    /// </summary>
    /// <returns>true, ���� ��� ���� ���� �������.
    /// false, ���� ������������ ��������� ��������� ���� �� ����</returns>
    public bool CloseAllChildren()
    {
      bool Res;
      EFPApp.BeginUpdateInterface(); // ���������� 13.09.2021
      try
      {
        Res = DoCloseAllChildren();
      }
      finally
      {
        EFPApp.EndUpdateInterface();
      }
      return Res;
    }

    private bool DoCloseAllChildren()
    {
      Form[] Forms = GetChildForms(true);

      for (int i = 0; i < Forms.Length; i++)
      {
        Forms[i].Close();
        if (Forms[i].Visible)
          return false;
      }
      return true;
    }

    #endregion

    #region ����������� ������ �������������

    /// <summary>
    /// ��������� � ����� ������ ������� ToolStripPanel ��� ���� � ������� ������������, ��������� ������
    /// </summary>
    /// <param name="mainWindow"></param>
    protected static void DecorateMainWindow(Form mainWindow)
    {
      // Create ToolStripPanel controls.
      ToolStripPanel StripPanelTop = new ToolStripPanel();
      ToolStripPanel StripPanelBottom = new ToolStripPanel();
      ToolStripPanel StripPanelLeft = new ToolStripPanel();
      ToolStripPanel StripPanelRight = new ToolStripPanel();

      // Dock the ToolStripPanel controls to the edges of the TheForm.
      StripPanelTop.Dock = DockStyle.Top;
      StripPanelBottom.Dock = DockStyle.Bottom;
      StripPanelLeft.Dock = DockStyle.Left;
      StripPanelRight.Dock = DockStyle.Right;

      /*
      if (EnvironmentTools.IsMono)
      {
        StripPanelTop.BackColor = SystemColors.Control;
        StripPanelBottom.BackColor = SystemColors.Control;
        StripPanelLeft.BackColor = SystemColors.Control;
        StripPanelRight.BackColor = SystemColors.Control;
      } */

      // Add the ToolStripPanels to the TheForm in reverse order.
      mainWindow.Controls.Add(StripPanelRight);
      mainWindow.Controls.Add(StripPanelLeft);
      mainWindow.Controls.Add(StripPanelBottom);
      mainWindow.Controls.Add(StripPanelTop);

      StatusStrip TheStatusBar = new System.Windows.Forms.StatusStrip();
      EFPApp.SetStatusStripHeight(TheStatusBar, mainWindow); // 16.06.2021
      mainWindow.Controls.Add(TheStatusBar);
    }

    #endregion

    #region ����������� �������� ����

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
        // ��������� ��������� ������� ����?
        bool CloseApp = (EFPApp.Interface.MainWindowCount == 1) && (!InsideCloseMainWindow);
        if ((!CloseApp) && (!Interface.IsSDI) && (!InsideCloseMainWindow))
        {
          RadioSelectDialog dlg = new RadioSelectDialog();
          dlg.Title = "�������� ���� " + this.MainWindow.Text;
          //dlg.ImageKey = "CloseWindow"; // ������ ������� ����������� �������� - ������ �������� ������ ���������� ����
          dlg.GroupTitle = "��� ���� �������";
          dlg.Items = new string[] { "������� ������� ������� ����", "��������� ������" };
          dlg.ImageKeys = new string[] { "CloseWindow", "Exit" };
          if (dlg.ShowDialog() != DialogResult.OK)
          {
            args.Cancel = true;
            return;
          }
          if (dlg.SelectedIndex == 1)
            CloseApp = true;
        }

        if (CloseApp)
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
          // ���������� ��� ��� ����� ���������� ����������. ���� ��� ������ ��� �������,
          // ������ ������� �� ��������
          // � ��� ���� �� �������, �� ������� �� ������ ���� �� �����, � ������� �
          // ������ ���������
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

    #region �������� ���������� �������� ����

    /// <summary>
    /// ������� ���� 
    /// </summary>
    public EFPMainMenu MainMenu
    {
      get { return _MainMenu; }
      internal set { _MainMenu = value; }
    }
    private EFPMainMenu _MainMenu;

    /// <summary>
    /// ������ ������� ������������, ���������� ������� ����� ���������
    /// </summary>
    public EFPAppToolBars ToolBars
    {
      get { return _ToolBars; }
      internal set { _ToolBars = value; }
    }
    private EFPAppToolBars _ToolBars;

    /// <summary>
    /// ��������� ������, ������� ����� ���������
    /// </summary>
    public EFPAppStatusBar StatusBar
    {
      get { return _StatusBar; }
      internal set { _StatusBar = value; }
    }
    private EFPAppStatusBar _StatusBar;


    /// <summary>
    /// �������������� ������� ����, ������ ������������ � ��������� ������ ��� �������� ����
    /// </summary>
    internal void InitCommandItems()
    {
      // ������ �������� ��� ���������� ������ ������� ?? ���� ��?
      EFPApp.CommandItems.InitMenuVisible();

      if (!Interface.ObsoleteMode)
      {
#if DEBUG
        if (_MainMenu != null || _ToolBars != null || _StatusBar != null)
          throw new BugException("��������� �������������");
#endif

        #region ������� ����

        _MainMenu = new EFPMainMenu();
        _MainMenu.Name = "TheMainMenu";
        _MainMenu.Add(EFPApp.CommandItems);
        // ???? cmm.InitWindowMenu(MenuWindow);
        _MainMenu.Attach(MainWindow);

        #endregion

        #region ������ ������������

        _ToolBars = new EFPAppToolBars();
        for (int i = 0; i < EFPApp.ToolBars.Count; i++)
        {
          EFPAppToolBarCommandItems Src = EFPApp.ToolBars[i];

          EFPAppToolBar Res = new EFPAppToolBar(Src.Name);
          Res.Info = new FormToolStripInfo(MainWindow);
          Res.DisplayName = Src.DisplayName;
          Res.Add(Src);

          EFPAppToolBar CurrTB = null;
          if (Interface.CurrentMainWindowLayout != null)
            CurrTB = Interface.CurrentMainWindowLayout.ToolBars[Src.Name];
          if (CurrTB == null)
          {
            Res.Visible = Src.DefaultVisible;
            Res.Dock = Src.DefaultDock;
          }
          else
          {
            Res.Visible = CurrTB.Visible;
            Res.Dock = CurrTB.Dock;
          }

          _ToolBars.Add(Res);
        }

        // 22.11.2018
        // ������� "������������"
        EFPCommandItem ciRestore = new EFPCommandItem("View", "RestoreToolBars");
        ciRestore.MenuText = "������������";
        ciRestore.Click += new EventHandler(EFPAppCommandItemHelpers.RestoreToolBars_Click);
        ciRestore.GroupBegin = true;
        _ToolBars.ContextMenu.Add(ciRestore);

        _ToolBars.Attach();

        #endregion

        #region ��������� ������

        _StatusBar = new EFPAppStatusBar();

        FormToolStripInfo Info = new FormToolStripInfo(MainWindow);

        _StatusBar.StatusStripControl = Info.StatusBar;

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
    /// ��������������� ���������� � ��������� ������� ������������
    /// </summary>
    public void RestoreToolBars()
    {
      for (int i = 0; i < EFPApp.ToolBars.Count; i++)
      {
        EFPAppToolBarCommandItems Src = EFPApp.ToolBars[i];

        EFPAppToolBar Res = ToolBars[Src.Name];
        Res.Visible = Src.DefaultVisible;
        Res.Dock = Src.DefaultDock;
      }
    }

    #endregion

    #region �������� ��������� ������� ������������ � ��������� ������

    internal void WriteLayoutConfig(CfgPart cfg)
    {
      CfgPart cfgToolBars = cfg.GetChild("ToolBars", true);
      for (int i = 0; i < ToolBars.Count; i++)
      {
        EFPAppToolBar ToolBar = ToolBars[i];
        CfgPart cfgOneTB = cfgToolBars.GetChild(ToolBar.Name, true);
        cfgOneTB.SetBool("Visible", ToolBar.Visible);
        cfgOneTB.SetEnum<DockStyle>("Dock", ToolBar.Dock);
        if (ToolBar.UseLocation)
        {
          cfgOneTB.SetInt("Left", ToolBar.Location.X);
          cfgOneTB.SetInt("Top", ToolBar.Location.Y);
          cfgOneTB.Remove("RowIndex");
        }
        else
        {
          cfgOneTB.SetInt("RowIndex", ToolBar.RowIndex);
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
          EFPAppToolBar ToolBar = ToolBars[i];
          // ������ ������� ������������ ��� ���������� � � ������ ������������ �������� �� ��� ������
          // ��� ������������� ������ ��������� ��������� ������� "��� ����".
          CfgPart cfgOneTB = cfgToolBars.GetChild(ToolBar.Name, false);
          if (cfgOneTB != null)
          {
            ToolBar.Visible = cfgOneTB.GetBoolDef("Visible", ToolBar.Visible);
            ToolBar.Dock = cfgOneTB.GetEnumDef<DockStyle>("Dock", ToolBar.Dock);
            if (cfgOneTB.HasValue("RowIndex"))
              ToolBar.RowIndex = cfgOneTB.GetInt("RowIndex");
            else
            {
              ToolBar.Location = new System.Drawing.Point(cfgOneTB.GetInt("Left"),
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

    #region ������������ ����

    /// <summary>
    /// ������������ ��������� �������� ����.
    /// ��� ���������� SDI ����� null
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
    /// ���������� ������������� �� �������� ����� � ������� �� ���������� (ZOrder=false)
    /// </summary>
    /// <returns>�������������</returns>
    public IEnumerator<Form> GetEnumerator()
    {
      return _ChildForms.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// ���������� ������������� �� �������� �����.
    /// ���� <paramref name="useZOrder"/>=true, �� ���� ������������ �� ������� ��������� (CurrentChildForm ����� ������ � ������).
    /// ���� <paramref name="useZOrder"/>=false, �� ���� ������������ �� ������� �������� ���� (#1, #2)
    /// </summary>
    /// <param name="useZOrder">����� �� ����������� ���� �� ������� ���������</param>
    /// <returns>�������������</returns>
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
    /// ���������� �������� ���� ��� ������� ���������������� ��������� ����������.
    /// ����������������� ����� �������� ����� Control.DrawToBitmap().
    /// ���������������� ��� ���������� MDI.
    /// </summary>
    /// <param name="bitmap">�����������, �� ������� ��������� ��������� ���������</param>
    /// <param name="area">���������� � �������� ����������� <paramref name="bitmap"/>,
    /// � ������� ��������� ������� ����������� ����</param>
    /// <param name="forComposition">���� true, �� ����� ��������� ������ ��, ����,
    /// ������� ����� ���� �������� � ���������� ����.
    /// ���� false, �� ����� �������� ��� ����.
    /// �������� ����������� ������ ��� ���������� MDI.</param>
    internal protected virtual void DrawMainWindowSnapshot(Bitmap bitmap, Rectangle area, bool forComposition)
    {
      MainWindow.DrawToBitmap(bitmap, area);
    }

    #endregion
  }

  /// <summary>
  /// ����� ��� ������ ��������� ToolStripPanel � StatusBar �� ������������ �����.
  /// �� ������ �������������� � ���������������� ����
  /// </summary>
  public sealed class FormToolStripInfo
  {
    #region �����������

    /// <summary>
    /// ��������� ������� ����������� ��������� ����� � ��������� �������� ����������
    /// ����������.
    /// ������������ �������� ToolStripPanel, ToolStripContainer � StatusStrip
    /// </summary>
    /// <param name="form">�����</param>
    public FormToolStripInfo(Form form)
    {
      if (form == null)
        throw new ArgumentNullException();

      foreach (Control Control in form.Controls)
      {
        ToolStripPanel tsp = Control as ToolStripPanel;
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

        ToolStripContainer tsc = Control as ToolStripContainer;
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

      // ��������� ������ ����� ���� �� ������
      _StatusBar = WinFormsTools.FindControl<StatusStrip>(form, true);
    }

    #endregion

    #region ��������� ��������

    /// <summary>
    /// ��������� ������ ��� ���������� ������ �����.
    /// ���������� null, ���� ������ �� �������.
    /// </summary>
    public ToolStripPanel StripPanelTop { get { return _StripPanelTop; } }
    private readonly ToolStripPanel _StripPanelTop;

    /// <summary>
    /// ��������� ������ ��� ���������� ����� �����.
    /// ���������� null, ���� ������ �� �������.
    /// </summary>
    public ToolStripPanel StripPanelBottom { get { return _StripPanelBottom; } }
    private readonly ToolStripPanel _StripPanelBottom;

    /// <summary>
    /// ��������� ������������ ������ ��� ���������� � ����� ����� �����.
    /// ���������� null, ���� ������ �� �������.
    /// </summary>
    public ToolStripPanel StripPanelLeft { get { return _StripPanelLeft; } }
    private readonly ToolStripPanel _StripPanelLeft;

    /// <summary>
    /// ��������� ������������ ������ ��� ���������� � ������ ����� �����.
    /// ���������� null, ���� ������ �� �������.
    /// </summary>
    public ToolStripPanel StripPanelRight { get { return _StripPanelRight; } }
    private readonly ToolStripPanel _StripPanelRight;

    /// <summary>
    /// ��������� ��������� ������.
    /// ���������� null, ���� ��������� ������ �� �������.
    /// </summary>
    public StatusStrip StatusBar { get { return _StatusBar; } }
    private readonly StatusStrip _StatusBar;

    #endregion
  }

  /// <summary>
  /// �������� ��� ��������� ��������� ������� ���� ��� �������� DefaultLocation.
  /// ������������ � ���������� MDI � SDI.
  /// �� ������� ������������ � ���������������� ����.
  /// </summary>
  public sealed class FormStartPositionCascadeHelper
  {
    #region �����������

    /// <summary>
    /// �������������� ������
    /// </summary>
    public FormStartPositionCascadeHelper()
    {
      _PrevLocation = new Point(int.MaxValue, int.MaxValue);
    }

    #endregion

    #region ���������� ������ ����

    /// <summary>
    /// �������� ��� ������ ���� ������������ �����������
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
    /// ��������� ��� ���������� �����
    /// </summary>
    private Point _PrevLocation;

    /// <summary>
    /// ���������� ���� � ������� � ��������
    /// </summary>
    /// <param name="form">�����, ������� ��������� ����������</param>
    /// <param name="area">�������</param>
    public void SetStartPosition(Form form, Rectangle area)
    {
      FormWindowState OldState = form.WindowState;
      try
      {
        // ����� �������� � ���������� ���������
        form.WindowState = FormWindowState.Normal;

        if (form.StartPosition == FormStartPosition.WindowsDefaultBounds)
        {
          form.StartPosition = FormStartPosition.WindowsDefaultLocation;
          Size sz = new Size(area.Width * 3 / 4, area.Height * 3 / 4);
          sz = WinFormsTools.Max(sz, form.MinimumSize);
          sz = WinFormsTools.Max(sz, new Size(600, 400)); // ������� �� ������
          form.Size = sz;
        }

        if (form.StartPosition == FormStartPosition.WindowsDefaultLocation)
        {
          form.StartPosition = FormStartPosition.Manual;

          // �����
          if (_PrevLocation.X == int.MaxValue) // ������ �����
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
          // ������������ �������
          WinFormsTools.PlaceFormInRectangle(form, area);
        }
      }
      finally
      {
        form.WindowState = OldState;
      }
    }

    #endregion

    #region ������ ������

    /// <summary>
    /// ���� ����� ����������, ����� ����������� ��� �������� ����.
    /// � ��������� ��� ���� ��������� � ����� ������� ���� ��� ��������, ��� ��� ������� ����.
    /// </summary>
    public void ResetStartPosition()
    {
      _PrevLocation = new Point(int.MaxValue, int.MaxValue);
    }

    #endregion
  }
}
