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
      foreach (Form child in _ChildForms)
      {
        if (form == child)
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
      Form[] forms = GetChildForms(false);
      for (int i = 0; i < forms.Length; i++)
      {
        if (forms[i].GetType() == formType)
          return forms[i];
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
      Form[] forms = GetChildForms(false);
      for (int i = 0; i < forms.Length; i++)
      {
        if (forms[i] is T)
          return (T)(forms[i]);
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
    /// ����� ��� �������� ����� ��������� ������
    /// </summary>
    /// <typeparam name="T">����� �����</typeparam>
    /// <returns>������ ����</returns>
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

      Form form = (Form)sender;
      if (Interface.CurrentChildForm == form)
        return; // �� ����������

      _ChildForms.Touch(form);
      Interface.MainWindowActivated(this); // ����� EFPApp ������� �������� ������� �����

      //System.Diagnostics.Trace.WriteLine("Activated ��� ����� " + Form.Text);

      EFPApp.TestInterfaceChanged();
    }

    void Form_VisibleChanged(object sender, EventArgs args)
    {
      if (Interface == null)
        return; // 10.06.2021, ����� �� ������������� ��� ��������� ������

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
      bool res;
      EFPApp.BeginUpdateInterface(); // ���������� 13.09.2021
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

    #region ����������� ������ �������������

    /// <summary>
    /// ��������� � ����� ������ ������� ToolStripPanel ��� ���� � ������� ������������, ��������� ������
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
        bool closeApp = (EFPApp.Interface.MainWindowCount == 1) && (!InsideCloseMainWindow);
        if ((!closeApp) && (!Interface.IsSDI) && (!InsideCloseMainWindow))
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
    /// ��������������� ���������� � ��������� ������� ������������
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

    #region �������� ��������� ������� ������������ � ��������� ������

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
          // ������ ������� ������������ ��� ���������� � � ������ ������������ �������� �� ��� ������
          // ��� ������������� ������ ��������� ��������� ������� "��� ����".
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
