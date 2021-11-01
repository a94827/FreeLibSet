using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// ��������� ������������, � ������� ��� ���� ������������� ��������������� �� ������� �����
  /// Windows � ����� ����������� ������� ����, ������ ������������ � ��������� ������.
  /// ������ ������������ ��� ��������, � ������� ���� ������ ���� ���� ��� ���������.
  /// �������� Name ���������� �������� "SDI".
  /// </summary>
  public class EFPAppInterfaceSDI : EFPAppInterface
  {
    #region �����������

    /// <summary>
    /// ������� ���������
    /// </summary>
    public EFPAppInterfaceSDI()
    {
      _CascadeHelper = new FormStartPositionCascadeHelper();
    }

    #endregion

    #region �������������� ����������

    /// <summary>
    /// ���������� "SDI"
    /// </summary>
    public override string Name { get { return "SDI"; } }

    /// <summary>
    /// ���������� true.
    /// </summary>
    public override bool IsSDI { get { return true; } }

    /// <summary>
    /// ���������� false
    /// </summary>
    public override bool MainWindowNumberUsed { get { return false; } }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ������ �� ������
    /// </summary>
    internal protected override void InitMainWindowTitles()
    {
      // ������ �� ������. ��������� ��������� � ������ ������������� ����� � ������ �� ��������
    }

    /// <summary>
    /// ������� ���� - "��������"
    /// </summary>
    /// <returns></returns>
    public override EFPAppMainWindowLayout ShowMainWindow()
    {
      Form DummyForm = new Form();
      DummyForm.StartPosition = FormStartPosition.WindowsDefaultBounds;
      ToolStripContainer StripContainer = new ToolStripContainer();
      StripContainer.Dock = DockStyle.Fill;
      StripContainer.ContentPanel.BackColor = SystemColors.AppWorkspace;

      StatusStrip TheStatusBar = new System.Windows.Forms.StatusStrip();
      StripContainer.BottomToolStripPanel.Controls.Add(TheStatusBar);

      _CascadeHelper.SetStartPosition(DummyForm, EFPApp.DefaultScreen.WorkingArea);  // ����������� �� AddMainWindow()

      EFPAppMainWindowLayoutSDI Layout = new EFPAppMainWindowLayoutSDI(DummyForm, false);
      base.AddMainWindow(Layout);

      Layout.MainWindow.Show();

      // �������� �� �������� � ������ �������� ����

      return Layout;
    }


    private FormStartPositionCascadeHelper _CascadeHelper;

    private class PreparationData
    {
      #region ����

      internal EFPAppMainWindowLayoutSDI Layout;

      internal FormWindowState SrcState;

      internal Size SrcSize;

      internal ToolStripContainer StripContainer;

      #endregion
    }

    /// <summary>
    /// ���������� � ���������
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    protected override object OnPrepareChildForm(Form form)
    {
      PreparationData pd = new PreparationData();

      pd.SrcState = form.WindowState;
      form.WindowState = FormWindowState.Normal;
      pd.SrcSize = Size.Empty;
      if (form.StartPosition != FormStartPosition.WindowsDefaultBounds)
        pd.SrcSize = form.ClientSize;

      pd.StripContainer = new ToolStripContainer();
      pd.StripContainer.Dock = DockStyle.Fill;
      WinFormsTools.MoveControls(form, pd.StripContainer.ContentPanel);
      form.Controls.Add(pd.StripContainer);

      //base.PrepareChildForm(Form);

      StatusStrip TheStatusBar = new System.Windows.Forms.StatusStrip();
      EFPApp.SetStatusStripHeight(TheStatusBar, form); // 16.06.2021
      pd.StripContainer.BottomToolStripPanel.Controls.Add(TheStatusBar);
      form.Controls.Add(TheStatusBar);

      //TheStatusBar.Items.Add("��������� ������ SDI");

      // ������� �� �����
      _CascadeHelper.SetStartPosition(form, EFPApp.DefaultScreen.WorkingArea); // ����������� �� AddMainWindow()

      pd.Layout = new EFPAppMainWindowLayoutSDI(form, true);
      base.AddMainWindow(pd.Layout);

      return pd;
    }

    /// <summary>
    /// ����� ��������� ����.
    /// ������� ToolStripContainer, � ����������� ������ �������� ��������� ��� ������������
    /// �������� �������� �����
    /// </summary>
    /// <param name="form">��������� � ���������������� ���� �����</param>
    /// <param name="preparationData">���������� ������</param>
    protected override void OnShowChildForm(Form form, object preparationData)
    {
      PreparationData pd = (PreparationData)preparationData;

      pd.Layout.MainWindow.Show();

      // ������, ����� ������� �����������
      EFPFormProvider formProvider = EFPFormProvider.FindFormProviderRequired(form);
      if ((!pd.SrcSize.IsEmpty) && (formProvider.ReadConfigFormBoundsParts & EFPFormBoundsPart.Size) == 0)
      {
        // ������ ���� ����� ������ AddMainWindow(), �.�. ��� ����������� ��������� ������ � ������
        Size NewSize = pd.StripContainer.ContentPanel.ClientSize;
        int dx = pd.SrcSize.Width - NewSize.Width;
        int dy = pd.SrcSize.Height - NewSize.Height;
        form.Size = new Size(form.Size.Width + dx,
          form.Size.Height + dy);
      }

      WinFormsTools.PlaceFormInRectangle(form, EFPApp.DefaultScreen.WorkingArea); // ��� ���, � ������ ����������� �������

      form.WindowState = pd.SrcState;
    }

    /// <summary>
    /// ����������� ����������
    /// </summary>
    /// <param name="mdiLayout"></param>
    public override void LayoutChildForms(MdiLayout mdiLayout)
    {
      throw new NotImplementedException("���������� ���� ���������� SDI �� �����������");
    }

    #endregion
  }

  /// <summary>
  /// ���������� EFPAppMainWindowLayout ��� ���������� SDI.
  /// </summary>
  public sealed class EFPAppMainWindowLayoutSDI : EFPAppMainWindowLayout
  {
    #region ���������� �����������

    internal EFPAppMainWindowLayoutSDI(Form form, bool isRealForm)
    {
      base.MainWindow = form;
      if (isRealForm)
        base.PrepareChildForm(form);
    }

    #endregion
  }
}