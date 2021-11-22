// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// ���������������� ��������� MDI.
  /// ������������ ����� �������� ���� � ������������� ��������� Form.IsMdiContainer=true.
  /// ��� �������� ���� ��������������� �������� Form.MdiParent
  /// </summary>
  public class EFPAppInterfaceMDI : EFPAppInterface
  {
    #region �����������

    /// <summary>
    /// ������� ������ ���������� MDI
    /// </summary>
    public EFPAppInterfaceMDI()
    {
    }

    #endregion

    #region �������������� ����������

    /// <summary>
    /// ���������� "MDI"
    /// </summary>
    public override string Name { get { return "MDI"; } }


    #endregion

    #region ������� ����

    /// <summary>
    /// ������� ������� ���� ���������
    /// </summary>
    /// <returns></returns>
    public override EFPAppMainWindowLayout ShowMainWindow()
    {
      EFPAppMainWindowLayoutMDI Layout = new EFPAppMainWindowLayoutMDI(ObsoleteMode);

      base.AddMainWindow(Layout);

      Layout.MainWindow.Show();

      return Layout;
    }

    /// <summary>
    /// ���������� ������� ������� ����.
    /// ������ � ��������� � ����������� MDI ���� ������ ���� ������� ����,
    /// �� ���������� ����������� ������� �������������� ������� ����, ��� ������ ��� ������� ���������� ���������.
    /// </summary>
    public new EFPAppMainWindowLayoutMDI CurrentMainWindowLayout
    {
      get { return (EFPAppMainWindowLayoutMDI)(base.CurrentMainWindowLayout); }
    }

    #endregion

    #region �������� �����

    /// <summary>
    /// ���������� � ���������
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    protected override object OnPrepareChildForm(Form form)
    {
      if (CurrentMainWindowLayout == null)
        ShowMainWindow();
      CurrentMainWindowLayout.PrepareChildForm(form);

      return null;
    }

    /// <summary>
    /// ����� �������� �����.
    /// ���� � ������ ������ ��� �� ������ �������� ���� (��� �������� �������),
    /// �� ��������� ������ ������� ����.
    /// </summary>
    /// <param name="form">��������� �������� �����</param>
    /// <param name="preparationData">���������� ������</param>
    protected override void OnShowChildForm(Form form, object preparationData)
    {
      CurrentMainWindowLayout.ShowChildForm(form);
    }

    /// <summary>
    /// ���������� �������������� ����������� ���������� �������� ����.
    /// ���������� ��������� ������ �������� ���� "����".
    /// ����������� ������ ������������ ��������.
    /// </summary>
    /// <param name="mdiLayout">�������������� ������������</param>
    /// <returns>��������� �������</returns>
    public override bool IsLayoutChildFormsSupported(MdiLayout mdiLayout)
    {
      switch (mdiLayout)
      {
        case MdiLayout.TileHorizontal:
        case MdiLayout.TileVertical:
        case MdiLayout.Cascade:
        case MdiLayout.ArrangeIcons:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// ���������� true, ���� ������������ �������� ���� ��������� � ������� ����������.
    /// ������������ ��� ����������� ��������� ������ �������� ���� "����"
    /// </summary>
    /// <param name="mdiLayout">��� ������������ ����</param>
    /// <returns>����������� ����������</returns>
    public override bool IsLayoutChildFormsAppliable(MdiLayout mdiLayout)
    {
      if (CurrentMainWindowLayout == null)
        return false;

      if (CurrentMainWindowLayout.ChildFormCount == 0)
        return false;
      if (mdiLayout == MdiLayout.ArrangeIcons)
      {
        Form[] Forms = CurrentMainWindowLayout.GetChildForms(false);
        for (int i = 0; i < Forms.Length; i++)
        {
          if (Forms[i].WindowState == FormWindowState.Minimized)
            return true;
        }
        return false;
      }

      return true;
    }

    #endregion
  }

  /// <summary>
  /// ����������� ������ ��� �������� ���� MDI.
  /// </summary>
  public sealed class EFPAppMainWindowLayoutMDI: EFPAppMainWindowLayout
  {
    #region ���������� �����������

    internal EFPAppMainWindowLayoutMDI(bool obsoleteMode)
    {
      base.MainWindow = new Form();
      base.MainWindow.BackColor = System.Drawing.SystemColors.AppWorkspace;
      base.MainWindow.IsMdiContainer = true;

      EFPAppMainWindowLayout.DecorateMainWindow(MainWindow);

      _ObsoleteMode = obsoleteMode;

      base.Bounds = new EFPFormBounds();

      _CascadeHelper = new FormStartPositionCascadeHelper();
    }

    #endregion

    #region ������� ����

    private bool _ObsoleteMode;

    #endregion

    #region �������� �����

    private FormStartPositionCascadeHelper _CascadeHelper;


    internal new void PrepareChildForm(Form form)
    {
      form.MdiParent = MainWindow;
      CorrectMdiChildFormIcon(form);

      base.PrepareChildForm(form);
    }

    /// <summary>
    /// ����������� �������� ������
    /// </summary>
    /// <param name="form">����� MDI child</param>
    private static void CorrectMdiChildFormIcon(Form form)
    {
      // 22.01.2015
      // ������������� ������.
      // ��������:
      // ���� ����� �������� ������ � ��������, ������� 16x16, �������� MDI-���� ��������� ������������, ���� ��� �� ���������������.
      // ���� ���� ����������, �� ������� ���� ���������� � ����� ���� �������������� ������, ���������� ������ ����
      // �������:
      // ��������� ������ ������ �� 16x16. ���� � ������������ Form.Icon ��� ������ ����������� �������, ��������� �������������� Icon->Bitmap,
      // ������������ �� ������� �������, ����� ����������� ������� Bitmap->Icon
      if (form.Icon == null)
        return;

      Size sis = SystemInformation.SmallIconSize;
      if (form.Icon.Width > sis.Width || form.Icon.Height > sis.Height)
      {
        Icon NewIcon = new Icon(form.Icon, sis);

        if (NewIcon.Size.Width != sis.Width || NewIcon.Size.Height != sis.Height)
        {
          Bitmap bmp = NewIcon.ToBitmap();
          NewIcon.Dispose();
          Bitmap bmp2 = new Bitmap(bmp, sis); // ���������������
          bmp.Dispose();
          NewIcon = Icon.FromHandle(bmp2.GetHicon());
          bmp2.Dispose();
        }
        form.Icon = NewIcon;
        //NewIcon.Dispose();
      }

    }

    /// <summary>
    /// ��������� �������� ���� � ���� ���������� MDI.
    /// ����������� �������������� ���������������� ���� "��������"
    /// </summary>
    /// <param name="form">�������� �����</param>
    internal void ShowChildForm(Form form)
    {
      #region ���� �� �������

      // �� ����� ��������� � CorrectMdiChildFormIcon()

      /*
       * 13.08.2008
       * ���� ������ �� ������, �� ��������� ��������. ����� ����� ����������� �
       * ����������� ������ ��� ���� ���������� �������� ���� ���������� (��� ����
       * ����������� ����� ����� ����� ����������, ��� ������������� �������� 
       * ���������� MDI), ����������� ������������ ������ �����. ������ ������,
       * ������������� ��������� TheForm.Icon � ����� ����� �������� ���� ����� 
       * ��������� ������ �� ���������.
       * �������.
       * �� ����� ������������� ����� � ��������� MDI ����� ���������� ��������
       * FormBorderStyle � �������� FixedDialog, � ����� ������� ��� �������.
       * 
       * ��� ����, ������� ������ ��� ���� ��������� � .NET Framework, �� ��� �
       * �� ��������� �� ��� ���.
       * 
       * �������� ��������:
       * https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=106264
       */

      bool SetMaximized = false;
      bool CanMaximize = form.FormBorderStyle == FormBorderStyle.Sizable && form.MaximizeBox;

      // �������� �� form.WindowState ����� ���� ������ � ShowChildForm(), �� �� � PrepareChildForm().
      // ���������������� ��������� ����������� ����� ����� ����� ��������.
      if (CanMaximize && form.WindowState == FormWindowState.Maximized)
        // ���� ���� ����� ����������
        SetMaximized = true;

      if (MainWindow.ActiveMdiChild != null)
      {
        if (MainWindow.ActiveMdiChild.WindowState == FormWindowState.Maximized)
        {
          if (CanMaximize)
            // ���������� �������� ���� ����� ����������, �������������, ���� ����
            // (���� ��� �� ���������) ���� ������ �����������
            SetMaximized = true;
          else
            // ������� ��������� �������������� �������� ������ ����
            // ��� ����� ����� �������� ��������� ����������� ����
            MainWindow.ActiveMdiChild.WindowState = FormWindowState.Normal;
        }
      }

      if (SetMaximized)
      {
        // 25.08.2008
        // ���� ������ ������������� ����� �����, �� ��� �������� ��� ����, ���������
        // "�������". �����, ����������� � ���������, ����� ������������� ��������
        // ClientSize (������ Size). ����� ����� ������������ � ����������� ������ �
        // ����� ������� ���� ������� MDI, ���� ��������� � ����������� ���������
        // ����� ������ � ���� ������, ������� ������ ����� �� �����������������, �
        // ����� - �������������. �.�. �� ������ ������ ����� ��� �� ����������, ��
        // ������� ���������� ������������� ���

        form.WindowState = FormWindowState.Normal;
        form.FormBorderStyle = FormBorderStyle.Fixed3D;
        form.WindowState = FormWindowState.Maximized;
      }

      #endregion

      if (ChildFormCount == 1) // ����� ��� ��������� � ������
        _CascadeHelper.ResetStartPosition(); // 13.09.2021

      // 09.06.2021 ���������� ����, ����� ������ Form.Show()
      // 14.06.2021 ���������� �������, �� ������ �����, ����� ���� �� �������
      Rectangle Area = WinFormsTools.GetMdiContainerArea(MainWindow); // ��������� �������. ����� ������� ���� ����� ���������� (0,0)
      _CascadeHelper.SetStartPosition(form, Area);

      form.Show();

      if (SetMaximized)
        form.FormBorderStyle = FormBorderStyle.Sizable; // ��������������� ����� ������� ����� ������ Show()
    }

    /// <summary>
    /// ������������� �������� ���� ��� �������� ���� MDI
    /// </summary>
    /// <param name="mdiLayout">������ ������������</param>
    public override void LayoutChildForms(MdiLayout mdiLayout)
    {
      MainWindow.LayoutMdi(mdiLayout);
    }

    #endregion


    /// <summary>
    /// ��������� �������� ���� ��� ����������� ���������������� ���������.
    /// ����������� ����� Control.DrawToBitmap() ����� �������� ������ ���� ���������� MDI,
    /// �� �� ��� �������� ����.
    /// ���������������� ����� ��������� ������ ���������� ���� � ������ �� ������� ����������
    /// � ����������.
    /// </summary>
    /// <param name="bitmap">�����������, �� ������� ����������� ���������</param>
    /// <param name="area">�������, ���� ��������� ������� �����������</param>
    /// <param name="forComposition">True, ���� ��������� ���������� ������ �� �������� ����,
    /// ������� ����� ��������� ����������� ���������� ����� ��������.
    /// ���� false, �� ����� ���������� ��� ����</param>
    internal protected override void DrawMainWindowSnapshot(Bitmap bitmap, Rectangle area, bool forComposition)
    {
      MainWindow.DrawToBitmap(bitmap, area);

      // MDI-���� �������� � ������������ �������. ������� - ������� ����, ����� ��, ������� ��� ���
      // ���� �������� ������ ������ ����
      // ����� ����, �� ���� �������� ����, ������� �� ������ � ����������
      ClearWorkspaceArea(bitmap, area);

      // ������ �������� �������� ����
      Form[] Forms = GetChildForms(true);
      for (int i = Forms.Length - 1; i >= 0; i--)
      {
        if (EFPApp.FormWantsSaveComposition(Forms[i]))
        {
          Rectangle rc2 = Forms[i].RectangleToScreen(area);
          Rectangle rc3 = MainWindow.RectangleToClient(rc2);
          Forms[i].DrawToBitmap(bitmap, rc3);
        }
      }
    }

    private void ClearWorkspaceArea(Bitmap bitmap, Rectangle area)
    {
      Point pt0 = MainWindow.ClientRectangle.Location; // ������ (0,0)
      pt0 = MainWindow.PointToScreen(pt0);
      pt0 = new Point(pt0.X - MainWindow.Left, pt0.Y - MainWindow.Top); // ��������� ������� ������������ �������� ������ ���� �����


      Rectangle rc1 = WinFormsTools.GetControlDockFillArea(MainWindow); // ������������ MainWindow
      // Rectangle rc2 = MainWindow.RectangleToScreen(rc1); // ������������ ������
      Point pt2 = new Point(area.Left + pt0.X + rc1.Left,
        area.Top + pt0.Y + rc1.Top);
      Rectangle rc3 = new Rectangle(pt2, rc1.Size); // ������������ Area
      using (Graphics g = Graphics.FromImage(bitmap))
      {
        g.FillRectangle(SystemBrushes.AppWorkspace, rc3);
      }
    }

  }
}
