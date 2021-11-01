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
  /// ������ ��������� ���������� ������������.
  /// ������� ����� ��� EFPAppInterfaceMDI � EFPAppInterfaceSDI, ������� ��������� ������ ��������
  /// ����������
  /// </summary>
  public abstract class EFPAppInterface : IObjectWithCode, IEnumerable<EFPAppMainWindowLayout>
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    public EFPAppInterface()
    {
      _MainWindows = new ListWithMRU<EFPAppMainWindowLayout>();
    }

    #endregion

    #region �������������� ����������

    /// <summary>
    /// ��� ���������� ��� ���������� ��������.
    /// ���������� �MDI� , �SDI� ��� �TDI�
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// ���������� true ��� ���������� SDI
    /// </summary>
    public virtual bool IsSDI { get { return false; } }

    /// <summary>
    /// ���������� true, ���� ������������ ��������� ������� ���� (�������� EFPAppMainWindowLayout.MainWindowNumberText ���������� �������� ������).
    /// ��� ���������� MDI ��������� true, ���� ������� ������ ������ �������� ����.
    /// ��� ���������� SDI ������ ���������� false
    /// </summary>
    public virtual bool MainWindowNumberUsed
    {
      get { return MainWindowCount > 1; }
    }

    /// <summary>
    /// ���������� �������� Name
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return Name;
    }

    /// <summary>
    /// ������ ��� ������� �������� �������������
    /// </summary>
    internal bool ObsoleteMode;

    #endregion

    #region ������ � ��������� ����������

    /// <summary>
    /// ���������� ��� ������������� ���������� � ����c��� EFPApp.Interface
    /// </summary>
    internal protected virtual void Attach()
    {
    }

    /// <summary>
    /// ���������� ��� ������������ ���������� � ����c��� EFPApp.Interface.
    /// ��������� ��� �������� ����. � ������ ������������� ��������, ��������, ��-�� ������
    /// ������������ ��������� ������, ����������� ����������
    /// </summary>
    internal protected virtual void Detach()
    {
      CloseAll();
      if (MainWindowCount > 0)
        throw new CannotCloseFormException("�� ������� ������� ��� �������� ����");
    }

    #endregion

    #region ������� ����

    /// <summary>
    /// ���������� ������ �������� ������� ����.
    /// ���� <paramref name="useZOrder"/>=true, �� ������ ����������� �� ������� ��������� (CurrentMainWindowLayout ����� ������ � ������).
    /// ���� <paramref name="useZOrder"/>=false, �� ������ ����������� �� ������� �������� ���� (#1, #2)
    /// </summary>
    /// <param name="useZOrder">����� �� ����������� ������� ���� �� ������� ���������</param>
    /// <returns>������ ��� ������ � ������� �����</returns>
    public EFPAppMainWindowLayout[] GetMainWindowLayouts(bool useZOrder)
    {
      if (useZOrder)
        return _MainWindows.MRU.ToArray();
      else
        return _MainWindows.ToArray();
    }


    /// <summary>
    /// ���������� ������� ������� ����.
    /// ���� ���� ��������� ������� ����, �� ���������� �� ����, ������� �������� �������� ��� ���� �������� � ��������� ���.
    /// ��������� �������� ���������� ������� ����, ������ ��� �� �������� ����.
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
    /// ���������� ������� ����.
    /// ��� ���������� SDI ����� ���������� �������� ���� ���� "��������", ���� ����
    /// </summary>
    public int MainWindowCount { get { return _MainWindows.Count; } }

    /// <summary>
    /// ������� ������� ���� � ������ ��� �������
    /// ��� ���������� SDI ������� ����-���������
    /// </summary>
    /// <returns>�������� ���������� �������� ����</returns>
    public abstract EFPAppMainWindowLayout ShowMainWindow();

    #region ���������� ������ ������� ����

    /// <summary>
    /// ������ � ���������� ��� Z-order
    /// </summary>
    private ListWithMRU<EFPAppMainWindowLayout> _MainWindows;

    ///// <summary>
    ///// ������� EFPAppMainWindowLayout 
    ///// </summary>
    ///// <param name="MainWindow"></param>
    ///// <returns></returns>
    //protected EFPAppMainWindowLayout InternalCreateMainWindowLayout(Form MainWindow)
    //{
    //}

    private int _TotalMainWindowCount;

    /// <summary>
    /// ��������� ���� � ������ MainWindows
    /// ������������� �������� EFPAppMainWindowLayout.MainWindow�
    /// ������������� ������� ���� �� ������, ���� IsSDI=false.
    /// </summary>
    /// <param name="layout">��������� �������� ���� ����������</param>
    protected void AddMainWindow(EFPAppMainWindowLayout layout)
    {
      if (layout == null)
        throw new ArgumentNullException("layout");
      if (layout.Interface != null)
        throw new InvalidOperationException("��������� ������������� �������� ����");
      layout.Interface = this;
      if (layout.MainWindow == null)
        throw new NullReferenceException("��� �������� ����");

      #region �������������� ��������� ��� �������� ����

      // 23.08.2013
      // ��������� ������ ����������
      WinFormsTools.InitAppIcon(layout.MainWindow);
      layout.MainWindow.KeyPreview = true;
      layout.MainWindow.KeyDown += new KeyEventHandler(EFPCommandItems.PerformKeyDown);

      if (!IsSDI)
      {
        if (EFPApp.DefaultScreen != null) // �� ������ ������
        {
          int Delta = SystemInformation.CaptionHeight + 2 * SystemInformation.BorderSize.Width;
          Size Offset = new Size(Delta * (_TotalMainWindowCount % 5), Delta * (_TotalMainWindowCount % 5));

          layout.MainWindow.StartPosition = FormStartPosition.Manual;
          layout.MainWindow.Location = EFPApp.DefaultScreen.WorkingArea.Location + Offset;
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
    /// ������� ���� �� ������ MainWindows
    /// </summary>
    /// <param name="layout">��������� �������� ���� ����������</param>
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
    /// ��������� ������� Form.Activated.
    /// ��������� ���� ������ ������ MainWindows
    /// </summary>
    /// <param name="layout">��������� �������� ���� ����������</param>
    internal protected void MainWindowActivated(EFPAppMainWindowLayout layout)
    {
      if (layout == null)
        throw new ArgumentNullException("layout");
      _MainWindows.Touch(layout);

      EFPApp.MainWindow = layout.MainWindow;
    }

    #endregion

    /// <summary>
    /// ������������� ���������� ������� ����.
    /// ����������� ������� �����, ���� �� ������ 1, ��������� "Title #1", "Title #2", ...
    /// ��� Title ������� �� EFPApp.MainWindowTitle.
    /// ��� SDI ������ �� ������
    /// ���� ����� �� ������������ � ���������������� ����.
    /// </summary>
    internal protected virtual void InitMainWindowTitles()
    {
      EFPAppMainWindowLayout[] Layouts = GetMainWindowLayouts(false);
      if (Layouts.Length == 1)
      {
        Layouts[0].MainWindowNumberText = String.Empty;
        Layouts[0].MainWindow.Text = EFPApp.MainWindowTitle;
      }
      else
      {
        for (int i = 0; i < Layouts.Length; i++)
        {
          Layouts[i].MainWindowNumberText = " #" + (i + 1).ToString();
          Layouts[i].MainWindow.Text = EFPApp.MainWindowTitle + " " + Layouts[i].MainWindowNumberText;
        }
      }
    }

    /// <summary>
    /// ��������� ��� ����: � �������� � �������
    /// </summary>
    /// <returns>true, ���� ������� ��� �������</returns>
    public bool CloseAll()
    {
      if (!CloseAllChildren())
        return false;

      EFPAppMainWindowLayout[] Layouts = GetMainWindowLayouts(false);
      for (int i = 0; i < Layouts.Length; i++)
      {
        if (!Layouts[i].CloseMainWindow())
          return false;
      }
      return true;
    }

    #endregion

    #region �������� ����

    #region ������ ����

    /// <summary>
    /// ���������� ������ �������� �������� ���� �� ���� ������� �����. 
    /// ���� ���� ��������� ������� ����, ������������ �������� ���� �� ���� �� ���.
    /// ���� <paramref name="useZOrder"/>=true, �� ������������ ������� ������������ ���� (������� � ��� ������� ����, ����� � ��� �������� ���� � �������� �������� ����). CurrentChildForm ����� � ������ ������
    /// ���� <paramref name="useZOrder"/>=false, �� ������������ ���� � ������� �������� (������� � � ������� �������� ������� ���� #1, #2; ����� � ���� � �������� �������� ����)
    /// </summary>
    /// <param name="useZOrder">����� �� ����������� ���� �� ������� ���������</param>
    /// <returns>������ ����</returns>
    public Form[] GetChildForms(bool useZOrder)
    {
      EFPAppMainWindowLayout[] Layouts = GetMainWindowLayouts(useZOrder);
      List<Form> Forms = new List<Form>();
      for (int i = 0; i < Layouts.Length; i++)
      {
        Form[] a = Layouts[i].GetChildForms(useZOrder);
        Forms.AddRange(a);
      }
      return Forms.ToArray();
    }

    /// <summary>
    /// ���������� �������� �������� ���� ��� ����, ������� ���� �������� ��������� ���.
    /// ��������� �������� �������� ��������� �������� ���� � ��������� ��������� ���� � ���. ��� ���������� SDI ������ ������������ ����.
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
    /// ���������� ����� ���������� �������� ���� �� ���� ������� �����
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

    #region ����� �����

    /// <summary>
    /// ���������� ����� � ������.
    /// ���� ���� ����� �� ������ � ����� ����, �� ���������� ������������� �� ShowChildForm().
    /// ����� �������� OnPrepareChildForm(). ��������� ������ ���������������.
    /// </summary>
    /// <param name="form">�����</param>
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
    /// ��������� ���������� � ��������� �����
    /// � ����������� MDI � TDI ������������ CurrentMainWindowLayout. ��� ����, ���� ������� ���� ��� �� ���� �������, ��� ��������� ������� CreateMainWindow().
    /// � ���������� SDI ������ ��������� ����� ������ EFPAppMainWindowLayout ��� ���������� �����.
    /// </summary>
    /// <param name="form">�����</param>
    /// <returns>���� ����� ���� �������� ������������ ������, ������� ����������� ��� ������ �����</returns>
    protected abstract object OnPrepareChildForm(Form form);

    /// <summary>
    /// ���������� ��������� � ���������������� ���� �����.
    /// ���� ����� PrepareChildForm() �� ��� ������ � ����� ����, �� ����������. ����� ���������� OnShowChildForm().
    /// </summary>
    /// <param name="form">��������� � ���������������� ���� �����, ������� ���� ����������</param>
    public void ShowChildForm(Form form)
    {
      EFPFormProvider formProvider = GetChildFormProvider(form);
      if (!formProvider.Prepared)
        PrepareChildForm(form);
      formProvider.InternalSetVisible(false); // 17.07.2021 - ��� ���������� ��������

      OnShowChildForm(form, formProvider.InternalPreparationData);
      formProvider.InternalPreparationData = null;
    }

    /// <summary>
    /// ��������� ����� ����� �� �����
    /// </summary>
    /// <param name="form">�����</param>
    /// <param name="preparationData">������, ���������� �� OnPrepareChildForm()</param>
    protected abstract void OnShowChildForm(Form form, object preparationData);

    #endregion

    #region ����� ����

    /// <summary>
    /// ����� �������� ����, ����������� �����.
    /// ���������� null, ���� ����� �� �������
    /// </summary>
    /// <param name="form">������� �����</param>
    /// <returns>�������� �������� ���� ��� null</returns>
    public EFPAppMainWindowLayout FindMainWindowLayout(Form form)
    {
      if (form == null)
        return null;

      foreach (EFPAppMainWindowLayout Layout in _MainWindows)
      {
        if (Layout.ContainsForm(form))
          return Layout;
      }
      return null;
    }


    /// <summary>
    /// ����� �������� ����� ��������� ������.
    /// ���������� ������ ��������� ����� ��� null
    /// </summary>
    /// <param name="formType">��� �����</param>
    /// <returns>��������� ����� ��� null</returns>
    public Form FindChildForm(Type formType)
    {
      EFPAppMainWindowLayout[] Layouts = GetMainWindowLayouts(false);
      for (int i = 0; i < Layouts.Length; i++)
      {
        Form Form = Layouts[i].FindChildForm(formType);
        if (Form != null)
          return Form;
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
      EFPAppMainWindowLayout[] Layouts = GetMainWindowLayouts(false);
      for (int i = 0; i < Layouts.Length; i++)
      {
        T Form = Layouts[i].FindChildForm<T>();
        if (Form != null)
          return Form;
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
      EFPAppMainWindowLayout[] Layouts = GetMainWindowLayouts(false);
      if (Layouts.Length == 1)
        return Layouts[0].FindChildForms(formType);
      else
      {
        List<Form> List = new List<Form>();
        for (int i = 0; i < Layouts.Length; i++)
          Layouts[i].FindChildFormsInternal(List, formType);
        return List.ToArray();
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
      EFPAppMainWindowLayout[] Layouts = GetMainWindowLayouts(false);
      if (Layouts.Length == 1)
        return Layouts[0].FindChildForms<T>();
      else
      {
        List<T> List = new List<T>();
        for (int i = 0; i < Layouts.Length; i++)
          Layouts[i].FindChildFormsInternal<T>(List);
        return List.ToArray();
      }
    }

    /// <summary>
    /// ����� � ������������ ����� ��������� ������.
    /// ���������� true � ������ ������
    /// </summary>
    /// <param name="formType">��� �����</param>
    /// <returns>true, ���� ����� ������� � ������������. false, ���� ����� �� �������</returns>
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
    /// ����� � ������������ ����� ��������� ������.
    /// ���������� true � ������ ������
    /// </summary>
    /// <typeparam name="T">����� �����</typeparam>
    /// <returns>true, ���� ����� ������� � ������������. false, ���� ����� �� �������</returns>
    public bool FindAndActivateChildForm<T>()
      where T : Form
    {
#if DEBUG
      EFPApp.CheckMainThread();
#endif

      T frm = FindChildForm<T>();
      if (frm == null)
        return false;
      EFPApp.Activate(frm); // 07.06.2021
      return true;
    }

    #endregion

    #region �������� �����

    /// <summary>
    /// ��������� ��� �������� ����.
    /// ��� ���������� SDI �� �������� �� ������ ��������� ����.
    /// ��� MDI �������� ������ ������� ���� (��� ��������� ����).
    /// �������� ���� ����� ���� ��������, ���� ���� ������ ������������� �� �������� 
    /// ������������� ������, � ������������ ��������� ��������� ����.
    /// </summary>
    /// <returns>
    /// true, ���� ��� �������� ���� ���� �������.
    /// false, ���� �����-���� ���� �� ���� ������� ��-��� ������ ������������.
    /// </returns>
    public bool CloseAllChildren()
    {
      EFPAppMainWindowLayout[] Layouts = GetMainWindowLayouts(true); // � Z-�������
      for (int i = 0; i < Layouts.Length; i++)
      {
        if (!Layouts[i].CloseAllChildren())
          return false;
      }
      return true;
    }

    #endregion

    #region ������������ ����

    /// <summary>
    /// ���������� true, ���� ������ ��� �������������� �������� ���� ������������� �������������� �����������
    /// </summary>
    /// <param name="mdiLayout">������ ���������������</param>
    /// <returns>���������</returns>
    public virtual bool IsLayoutChildFormsSupported(MdiLayout mdiLayout)
    {
      return false;
    }

    /// <summary>
    /// ���������� true, ���� ������ ��� �������������� �������� ���� � ������ ������ ��������
    /// </summary>
    /// <param name="mdiLayout">������ ���������������</param>
    /// <returns>���������</returns>
    public virtual bool IsLayoutChildFormsAppliable(MdiLayout mdiLayout)
    {
      return false;
    }

    /// <summary>
    /// �������������� �������� ����.
    /// ��� ���������� SDI ����������� �� ���� �����.
    /// ��� MDI � TDI ����������� ������ � �������� �������� ����
    /// </summary>
    /// <param name="mdiLayout">������ ������������</param>
    public virtual void LayoutChildForms(MdiLayout mdiLayout)
    {
      if (CurrentMainWindowLayout != null)
        CurrentMainWindowLayout.LayoutChildForms(mdiLayout);
    }

    #endregion

    #endregion

    #region ����������

    // ���������� � �������������� ����������

    /// <summary>
    /// ��������� ���������� ���� � ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������ "Composition"-"UI"</param>
    public void SaveComposition(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      if (EFPApp.InsideLoadComposition || EFPApp.InsideSaveComposition)
        throw new ReenteranceException("��������� ����� SaveComposition()");

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
      EFPAppMainWindowLayout[] Layouts = null;
      int CurrWindowIndex;
      if (!IsSDI)
      {
        // ������� ����
        Layouts = GetMainWindowLayouts(false); // � ��� �������, ��� �������
        cfg.SetInt("MainWindowCount", Layouts.Length);
        CurrWindowIndex = -1;
        for (int i = 0; i < Layouts.Length; i++)
        {
          CfgPart cfgMainWindow = cfg.GetChild("MainWindow" + (i + 1).ToString(), true);
          Layouts[i].Bounds.FromControl(Layouts[i].MainWindow);
          CfgPart cfgBounds = cfgMainWindow.GetChild("Bounds", true);
          Layouts[i].Bounds.WriteConfig(cfgBounds);
          Layouts[i].WriteLayoutConfig(cfgMainWindow);

          if (Object.ReferenceEquals(Layouts[i], CurrentMainWindowLayout))
            CurrWindowIndex = i;
        }
        if (CurrWindowIndex >= 0)
          cfg.SetInt("CurrentMainWindow", CurrWindowIndex + 1);
      }

      Form[] ChildForms = GetChildForms(false); // � ������� ��������, � �� Z-Order
      //Part.SetInt("FormCount", ChildForms.Length);
      cfg.SetInt("FormCount", 0); // ����������� ����� 
      int cnt = 0; // ��������� ���� ����� �� �����������
      CurrWindowIndex = -1;
      Form CurrForm = this.CurrentChildForm;
      for (int i = 0; i < ChildForms.Length; i++)
      {
        if (!EFPApp.FormWantsSaveComposition(ChildForms[i]))
          continue;

        cnt++;

        EFPAppMainWindowLayout Layout = FindMainWindowLayout(ChildForms[i]);
        if (Layout == null)
          throw new BugException("�� ����� EFPAppMainWindowLayout ��� �������� ����� " + ChildForms[i].ToString());
        CfgPart cfgForm = cfg.GetChild("Form" + cnt.ToString(), true);
        if (Layouts != null)
        {
          // 27.12.2020 ������ ��������
          //          if (Layout != null)
          //          {
          int p = Array.IndexOf<EFPAppMainWindowLayout>(Layouts, Layout);
          cfgForm.SetInt("MainWindow", p + 1);
          //          }
        }
        else
          Layout.WriteLayoutConfig(cfgForm);

        EFPFormProvider FormProvider = EFPFormProvider.FindFormProviderRequired(ChildForms[i]);
        FormProvider.WriteComposition(cfgForm);

        if (Object.ReferenceEquals(ChildForms[i], CurrForm))
          CurrWindowIndex = cnt - 1;
      }

      cfg.SetInt("FormCount", cnt);
      if (CurrWindowIndex >= 0)
        cfg.SetInt("CurrentForm", CurrWindowIndex + 1);

      //// ������
      //try
      //{
      //  using (Bitmap bmp = EFPApp.CreateSnapshot())
      //  {
      //    using(System.IO.MemoryStream strm=new System.IO.MemoryStream())
      //    {
      //      bmp.Save(strm,System.Drawing.Imaging.ImageFormat.Gif);
      //      string s = Convert.ToBase64String(strm.GetBuffer());
      //      Part.SetString("Snapshot", s);
      //      //EFPApp.MessageBox("������: " + strm.Length.ToString());
      //    }
      //  }
      //}
      //catch (Exception e)
      //{
      //  LogoutTools.LogoutException(e, "�� ������� ������� Snapshot ����������");
      //}
    }

    /// <summary>
    /// ��������������� ���������� ���� � ������ ������������.
    /// ��������������, ��� ��������� ������ ��� ����������� � EFPApp � ��� �� ������� �� ������ �������� ����
    /// </summary>
    /// <param name="cfg">������ ������������ "Composition"-"UI"</param>
    public void LoadComposition(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      if (EFPApp.InsideLoadComposition || EFPApp.InsideSaveComposition)
        throw new ReenteranceException("��������� ����� LoadComposition()");

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

      EFPApp.SetInterfaceChanged(); // �������������� ����� ������� InterfaceChanged
    }

    private void DoLoadComposition(CfgPart cfg)
    {
      if (!CloseAll())
        throw new InvalidOperationException("�� ������� ������� �������� ����");

      List<EFPAppMainWindowLayout> mwls = new List<EFPAppMainWindowLayout>();

      if (!IsSDI)
      {
        int MainWindowCount = cfg.GetInt("MainWindowCount");
        int CurrMainWindowIndex = cfg.GetInt("CurrentMainWindow") - 1;
        for (int i = 0; i < MainWindowCount; i++)
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
                mwl.Bounds.WindowState = FormWindowState.Normal; // ��������� ���� �������������
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

        if (CurrMainWindowIndex >= 0 && CurrMainWindowIndex < mwls.Count)
          mwls[CurrMainWindowIndex].MainWindow.Select();
      }

      int ChildFormCount = cfg.GetInt("FormCount");
      int CurrChildFormIndex = cfg.GetInt("CurrentForm") - 1;
      Form CurrForm = null;
      for (int i = 0; i < ChildFormCount; i++)
      {
        CfgPart cfgForm = cfg.GetChild("Form" + (i + 1).ToString(), false);
        if (cfgForm != null)
        {
          EFPFormCreatorParams Params = new EFPFormCreatorParams(cfgForm);
          try
          {
            Form Form;
            try
            {
              Form = EFPApp.CreateForm(Params);
            }
            catch (Exception e)
            {
              EFPApp.ErrorMessageBox(e.Message, "�������� ������ ��� �������������� ����� \"" + Params.Title + "\"");
              continue;
            }
            if (Form == null)
              continue;

            EFPFormProvider FormProvider = EFPFormProvider.FindFormProviderRequired(Form);
            if (FormProvider.HasBeenShown)
              throw new BugException("��� �������������� ����� \"" + Form.ToString() + "\" ��� ���� �������� �������� �� �����");

            if (!IsSDI)
            {
              EFPAppMainWindowLayout mwl = mwls[0];
              int MainWindow = cfgForm.GetInt("MainWindow");
              if (MainWindow > 0 && MainWindow <= mwls.Count)
                mwl = mwls[MainWindow - 1];

              mwl.MainWindow.Select();
              mwl.ReadLayoutConfig(cfgForm);
            }

            try
            {
              PrepareChildForm(Form);
              // ������ if (FormProvider != null)
              FormProvider.ReadComposition(cfgForm);

              if (IsSDI)
              {
                EFPAppMainWindowLayout mwl = CurrentMainWindowLayout;
                mwl.ReadLayoutConfig(cfgForm);
              }

              ShowChildForm(Form);
              if (i == CurrChildFormIndex)
                CurrForm = Form;
            }
            catch (Exception e)
            {
              EFPApp.ShowException(e, "�� ������� �������� ����� \"" + Params.Title + "\"");
            }
          }
          catch (Exception e)
          {
            EFPApp.ShowException(e, "�������� ����������� ������ ��� �������������� ����� �" + (i + 1).ToString() + " \"" + Params.Title + "\"");
          }
        }
      } // ���� �� �������� ������

      if (CurrForm != null)
        CurrForm.Select();

      //if (IsSDI && ChildFormCount==0)
      //  EFPApp.def
    }

    #endregion

    #region ���������� ��������� ��������� �������� ����

    /// <summary>
    /// ��������� ��������� ������� ������������� � ������ ��������� �������� ���� � �������� ������
    /// ������������.
    /// ���� ������� ��������� ������� ���� ��� ������������ ��������� SDI, �� ������������ ��������
    /// ������� ����.
    /// ���� ��� �� ������ �������� ����, ������� �������� �� �����������
    /// </summary>
    /// <param name="cfg">����������� ������</param>
    public void SaveMainWindowLayout(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif


      if (CurrentMainWindowLayout != null)
        CurrentMainWindowLayout.WriteLayoutConfig(cfg);

      // TODO: ������ ToolWindow
    }

    /// <summary>
    /// �������� ��������� ������� ������������� � ������ ��������� �������� ���� �� �������� ������
    /// ������������.
    /// ���� ������� ��������� ������� ���� ��� ������������ ��������� SDI, �� ���������������
    /// ��������� ��� ���� ����.
    /// ���� ��� �� ������ �������� ����, ������� �������� �� �����������
    /// </summary>
    /// <param name="cfg">����������� ������ ������������</param>
    public void LoadMainWindowLayout(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      foreach (EFPAppMainWindowLayout Layout in this)
        Layout.ReadLayoutConfig(cfg);

      EFPApp.SetInterfaceChanged(); // ��������� �������������� ���������� ����
    }

    #endregion

    #region IObjectWithCode Members

    string IObjectWithCode.Code { get { return Name; } }

    #endregion

    #region IEnumerable<EFPAppMainWindowLayout> Members

    /// <summary>
    /// ���������� ������������� �� ������� ����� ��������� � ������� �� ���������� (ZOrder=false)
    /// </summary>
    /// <returns>�������������</returns>
    public IEnumerator<EFPAppMainWindowLayout> GetEnumerator()
    {
      return _MainWindows.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// ���������� ������������� �� ������� ����� ��������� 
    /// ���� <paramref name="useZOrder"/>=true, �� ���� ������������ �� ������� ��������� (CurrentMainWindowLayout ����� ������ � ������).
    /// ���� <paramref name="useZOrder"/>=false, �� ���� ������������ �� ������� �������� ���� (#1, #2)
    /// </summary>
    /// <param name="useZOrder">����� �� ����������� ������� ���� �� ������� ���������</param>
    /// <returns>�������������</returns>
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
  /// ������� ��������� ����������
  /// </summary>
  public sealed class EFPAppInterfaceState
  {
    #region �����������

    /// <summary>
    /// ���������� ������� ��������� ����������
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

    #region ��������

    /// <summary>
    /// ������ ���������� �����������
    /// </summary>
    public EFPAppInterface Interface { get { return _Interface; } }
    private EFPAppInterface _Interface;

    /// <summary>
    /// ������� ������� ���� (�������� ��������� ��� ���������� SDI)
    /// </summary>
    public EFPAppMainWindowLayout CurrentMainWindowLayout { get { return _CurrentMainWindowLayout; } }
    private EFPAppMainWindowLayout _CurrentMainWindowLayout;

    /// <summary>
    /// ���������� ������� ���� (��� ���������� SDI-���������� ���� ����)
    /// </summary>
    public int MainWindowCount { get { return _MainWindowCount; } }
    private int _MainWindowCount;

    /// <summary>
    /// �������� �������� ����
    /// </summary>
    public Form CurrentChildForm { get { return _CurrentChildForm; } }
    private Form _CurrentChildForm;

    /// <summary>
    /// ���������� �������� ����
    /// </summary>
    public int ChildFormCount { get { return _ChildFormCount; } }
    private int _ChildFormCount;

    #endregion

    #region ���������

    /// <summary>
    /// ��������� ���� ���������
    /// </summary>
    /// <param name="a">������ ������������ ���������</param>
    /// <param name="b">������ ������������ ���������</param>
    /// <returns>��������� ���������</returns>
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

    // ��������� �� �����, �� ��������� ��� �����������

    /// <summary>
    /// ��������� ���� ���������
    /// </summary>
    /// <param name="a">������ ������������ ���������</param>
    /// <param name="b">������ ������������ ���������</param>
    /// <returns>��������� ���������</returns>
    public static bool operator !=(EFPAppInterfaceState a, EFPAppInterfaceState b)
    {
      return !(a == b);
    }

    /// <summary>
    /// ��������� ���� ���������
    /// </summary>
    /// <param name="obj">������ ������������ ���������</param>
    /// <returns>��������� ���������</returns>
    public override bool Equals(object obj)
    {
      return this == (EFPAppInterfaceState)obj;
    }

    /// <summary>
    /// �� ������������. ���������� 0.
    /// </summary>
    /// <returns>0</returns>
    public override int GetHashCode()
    {
      return 0;
    }

    #endregion
  }


  /// <summary>
  /// ����������, ������������� EFPAppInterface.Detach(), ���� �� ������� ������� ��� ����
  /// </summary>
  [Serializable]
  public class CannotCloseFormException : ApplicationException
  {
    #region �����������

    /// <summary>
    /// ������� ������ ���������� � �������� ����������
    /// </summary>
    /// <param name="message">����� ���������</param>
    public CannotCloseFormException(string message)
      : this(message, null)
    {
    }


    /// <summary>
    /// ������� ������ ���������� � �������� ���������� � ��������� �����������
    /// </summary>
    /// <param name="message">����� ���������</param>
    /// <param name="innerException">��������� ����������</param>
    public CannotCloseFormException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// ������� ���������� � ������� ��������� �� ���������
    /// </summary>
    public CannotCloseFormException()
      : base("�� ������� ������� ����")
    {
    }

    /// <summary>
    /// ��� ������ ������������ ����� ��� ���������� ��������������
    /// </summary>
    protected CannotCloseFormException(System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

}