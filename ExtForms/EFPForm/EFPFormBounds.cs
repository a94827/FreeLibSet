// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using FreeLibSet.Config;

namespace FreeLibSet.Forms
{
  /*
   * ������������� EFPFormBounds
   * ������ EFPFormProvider �������� ������ �� EFPFormBounds. �� ��������� ������ ����� null, � �����
   * ������������ � �������� ���������, ������� ������ � ��������� �����.
   * �������� Bounds ����� ���� ����������� �� ������ ����� �� ����� ��� �����. ��� ���� �������� �������� 
   * Form.Location, Form.Size, Form.WindowState � Form.StartPosition.
   * ���� �������� �� �����������, ������ EFPFormBounds ��������� ��� ������ �����, � ��� �������� ���������������
   * � ������������ �������� ���������� �����.
   * � ����������, ���� ����� �������� �� �����, �������������� ������� �����.
   * 
   * ��� ���������� ��������� ����� ������� ����� ��������, ������� ������� ����������� ������ �� 
   * EFPFormBounds, ������������� ���������� null. ����� ������� ������� ������������� �������� ��������
   * EFPFormProvider.Bounds
   * 
   * ���������� ������.
   * ���� ��� ������ ��������� �������� ���������� ������ (�������� Screen.Bounds),
   * �� �������������� ��������� ����� �� ������ �����������
   * ����� �������������� �� �����������, ���� ������� ���� ��������� ���������� �� ������ �������.
   * ��������������, �������������� �� ����������� ���� ����������:
   * - Screen.FromControl(MainForm).Bounds
   * - Screen.FromRectangle(Bounds).Bounds
   */

  /// <summary>
  /// ����� � EFPFormBounds
  /// </summary>
  [Flags]
  public enum EFPFormBoundsPart
  {
    /// <summary>
    /// ���
    /// </summary>
    None = 0,

    /// <summary>
    /// ��������� �����
    /// </summary>
    Location = 1,

    /// <summary>
    /// ������ �����
    /// </summary>
    Size = 2,

    /// <summary>
    /// ��������� Normal/Maximized/Minimized
    /// </summary>
    WindowState = 4,

    /// <summary>
    /// ��� �����
    /// </summary>
    All = Location | Size | WindowState
  }


  /// <summary>
  /// ������ ��� ���������� �������� � ��������� ����� ����� ��������
  /// ��������� ������, ��������� ����� � ��������� ����� (�������, ���������������, ��������)
  /// </summary>
  public class EFPFormBounds
  {
    #region �����������

    /// <summary>
    /// ������� ������������� ������
    /// </summary>
    public EFPFormBounds()
    {
    }

    #endregion

    #region �������� �����

    /// <summary>
    /// ����������� ������� ����� � ��������� Normal.
    /// ���� ����� ��������� � ����������������� ��������� ��� ��������, �������� �������� RestoreBounds
    /// </summary>
    public Rectangle Bounds
    {
      get { return _Bounds; }
      set { _Bounds = value; }
    }
    private Rectangle _Bounds;

    /// <summary>
    /// ����������� ������� ������������
    /// </summary>
    public FormWindowState WindowState
    {
      get { return _WindowState; }
      set
      {
        _WindowState = value;
      }
    }
    private FormWindowState _WindowState;

    /// <summary>
    /// ���������� true, ���� ������ �� ���������������.
    /// </summary>
    public bool IsEmpty { get { return _Bounds.IsEmpty; } }

    /// <summary>
    /// ���������� ��������� ������������� � �������������� ������������ � ���������� ����
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (IsEmpty)
        return "{Empty}";
      return Bounds.ToString() + ", WindowState=" + WindowState.ToString();
    }

    #endregion

    #region ������ / ������ ��������� �����

    /// <summary>
    /// ������������� �������� ������� ����� ������� � ������������ � ������
    /// </summary>
    /// <param name="form">�����, ������ ����������� ��������</param>
    public void FromControl(Form form)
    {
      if (form == null)
        throw new ArgumentNullException("form");

      if (form.WindowState == FormWindowState.Normal)
        _Bounds = form.Bounds;
      else
        _Bounds = form.RestoreBounds;
      _WindowState = form.WindowState;
    }

    /// <summary>
    /// ������������� ������� ����� � ������������ � ������� �������� �������
    /// </summary>
    /// <param name="form">�����, ��������� ������� ���������������</param>
    /// <returns>������ �������/���������/���������, ������� ���� ��������� � �����</returns>
    public EFPFormBoundsPart ToControl(Form form)
    {
      return ToControl(form, EFPFormBoundsPart.All);
    }

    /// <summary>
    /// ������������� ������� ����� � ������������ � ������� �������� �������
    /// </summary>
    /// <param name="form">�����, ��������� ������� ���������������</param>
    /// <param name="parts">������ ���������� ���������, �������� � ��������� �����.
    /// ��������� ������ ����� ���� ���������������, ��������, Size, ���� ����� �� �������� Sizeable</param>
    /// <returns>������������ ������� �� <paramref name="parts"/>, ������� ���� ��������� � �����</returns>
    public EFPFormBoundsPart ToControl(Form form, EFPFormBoundsPart parts)
    {
      if (form == null)
        throw new ArgumentNullException("form");

      EFPFormBoundsPart ResParts = EFPFormBoundsPart.None;

      if (IsEmpty)
        return ResParts;

      CorrectBounds(form);

      // ���������� 13.09.2021
      if ((parts & EFPFormBoundsPart.Size) !=0)
      {
        if (form.StartPosition==FormStartPosition.WindowsDefaultBounds)
          form.StartPosition =FormStartPosition.WindowsDefaultLocation;
      }
      if ((parts & EFPFormBoundsPart.Location) != 0)
      {
        form.StartPosition = FormStartPosition.Manual;
      }

      FormWindowState OrgState = form.WindowState;

      EFPFormBoundsPart boundsparts = parts & (EFPFormBoundsPart.Location | EFPFormBoundsPart.Size);

      if (boundsparts != 0)
      {
        form.WindowState = FormWindowState.Normal;

        switch (form.FormBorderStyle)
        {
          case FormBorderStyle.Sizable:
          case FormBorderStyle.SizableToolWindow:
            if (boundsparts == (EFPFormBoundsPart.Location | EFPFormBoundsPart.Size))
              form.Bounds = Bounds;
            else if (boundsparts == EFPFormBoundsPart.Location)
              form.Location = Bounds.Location;
            else
              form.Size = Bounds.Size;
            ResParts |= boundsparts;
            break;
          default: // ������ ���������, �� �� �������
            if ((parts & EFPFormBoundsPart.Location) != 0)
            {
              form.Location = Bounds.Location;
              ResParts |= EFPFormBoundsPart.Location;
            }
            break;
        }
      }

      bool StateSetFlag = false;

      if ((parts & EFPFormBoundsPart.WindowState) != 0)
      {
        if (form.MaximizeBox || form.MinimizeBox)
        {
          if (WindowState == FormWindowState.Maximized && form.MaximizeBox)
          {
            form.WindowState = FormWindowState.Maximized;
            StateSetFlag = true;
            ResParts |= EFPFormBoundsPart.WindowState;
          }
          else if (WindowState == FormWindowState.Minimized && form.MinimizeBox)
          {
            form.WindowState = FormWindowState.Minimized;
            StateSetFlag = true;
            ResParts |= EFPFormBoundsPart.WindowState;
          }
          else
          {
            form.WindowState = FormWindowState.Normal;
            StateSetFlag = true;
          }
        }
      }
      if (!StateSetFlag)
        form.WindowState = OrgState;

      return ResParts;
    }

    #endregion

    #region ������ � ������ ������ ������������

    /// <summary>
    /// ���������� �������, ��������� � ��������� �����
    /// </summary>
    /// <param name="cfg">������ ��� ������ ������.
    /// ��� �������, ������ �������������� ������ ������ ��� ����� � ��������� � ����������� �������</param>
    public void WriteConfig(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      cfg.SetInt("Left", Bounds.Left);
      cfg.SetInt("Top", Bounds.Top);
      cfg.SetInt("Width", Bounds.Width);
      cfg.SetInt("Height", Bounds.Height);
      cfg.SetEnum<FormWindowState>("State", WindowState);
    }

    /// <summary>
    /// ��������� �������, ��������� � ��������� �����
    /// </summary>
    /// <param name="cfg">������ ��� ������ ������.
    /// ��� �������, ������ �������������� ������ ������ ��� ����� � ��������� � ����������� �������</param>
    public void ReadConfig(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      int l = cfg.GetInt("Left");
      int t = cfg.GetInt("Top");
      int w = cfg.GetInt("Width");
      int h = cfg.GetInt("Height");
      this.Bounds = new Rectangle(l, t, w, h);
      WindowState = cfg.GetEnumDef<FormWindowState>("State", this.WindowState);
    }

    #endregion

    #region ������������� ��������

    private void CorrectBounds(Form form)
    {
      Size MinSize;
      switch (form.FormBorderStyle)
      {
        case FormBorderStyle.Sizable:
        case FormBorderStyle.SizableToolWindow:
          MinSize = WinFormsTools.Max(form.MinimumSize, SystemInformation.MinimumWindowSize);
          break;
        default:
          MinSize = form.Size;
          break;
      }

      if (form.MdiParent != null)
      {
        Rectangle Area = WinFormsTools.GetMdiContainerArea(form.MdiParent);
        Bounds = WinFormsTools.PlaceRectangle(Bounds, Area, MinSize);
      }
      else if (form.Parent != null)
      {
        Rectangle Area = WinFormsTools.GetControlDockFillArea(form.Parent);
        Bounds = WinFormsTools.PlaceRectangle(Bounds, Area, MinSize);
      }
      else
      {
        Screen Screen = Screen.FromRectangle(Bounds);
        if (Screen == null)
        {
          Screen = EFPApp.DefaultScreen;
          if (Screen == null)
            Screen = Screen.PrimaryScreen;
        }
        Bounds = WinFormsTools.PlaceRectangle(Bounds, Screen.WorkingArea, MinSize);
      }
    }

    #endregion

    #region ����������� ������

    /// <summary>
    /// ���������� ������ ���������� ������, ������ �� ����������� ������ ������ ����� � ������ ������������/�����������
    /// </summary>
    /// <param name="form">�����</param>
    /// <returns>��������� �����</returns>
    internal static EFPFormBoundsPart GetParts(Form form)
    {
      if (form == null)
        return EFPFormBoundsPart.None;

      if ((!form.MaximizeBox) && form.WindowState == FormWindowState.Maximized)
        return EFPFormBoundsPart.None;

      EFPFormBoundsPart parts = EFPFormBoundsPart.Location;
      switch (form.FormBorderStyle)
      {
        case FormBorderStyle.Sizable:
        case FormBorderStyle.SizableToolWindow:
          parts |= EFPFormBoundsPart.Size;
          break;
      }
      if (form.MinimizeBox | form.MaximizeBox)
        parts |= EFPFormBoundsPart.WindowState;

      return parts;
    }

    /// <summary>
    /// ��������� ��������� ���� ������� ��������� � ������������ � ��������� ������� ������
    /// </summary>
    /// <param name="a">������ ������������ �����</param>
    /// <param name="b">������ ������������ �����</param>
    /// <param name="parts">�����, ������� ���� ����������</param>
    /// <returns>true, ���� ���������� ����������</returns>
    public static bool Equals(EFPFormBounds a, EFPFormBounds b, EFPFormBoundsPart parts)
    {
      if (a == null)
        a = new EFPFormBounds();
      if (b == null)
        b = new EFPFormBounds();

      if ((parts & EFPFormBoundsPart.Location) != 0)
      {
        if (a.Bounds.Left != b.Bounds.Left || a.Bounds.Top != b.Bounds.Top)
          return false;
      }
      if ((parts & EFPFormBoundsPart.Size) != 0)
      {
        if (a.Bounds.Width != b.Bounds.Width || a.Bounds.Height != b.Bounds.Height)
          return false;
      }
      if ((parts & EFPFormBoundsPart.WindowState) != 0)
      {
        if (a.WindowState != b.WindowState)
          return false;
      }
      return true;
    }

    #endregion
  }
}
