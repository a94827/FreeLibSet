// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace FreeLibSet.Forms
{
  #region ��������� IEFPControlWithToolBar<T>

  /// <summary>
  /// ��������� ������������ �������� � ������� ������������.
  /// ������ ������������ ��������� ����� EFPControlWithToolBar.
  /// ���������������� ������ ����������
  /// </summary>
  public interface IEFPControlWithToolBar
  {
    /// <summary>
    /// BaseProvider
    /// </summary>
    EFPBaseProvider BaseProvider { get;}

    /// <summary>
    /// �������� ����������� �������
    /// </summary>
    Control Control { get;}

    /// <summary>
    /// ������ ��� ���������� ������������.
    /// ����� ���� null, ���� EFPApp.ShowControlToolBars=false
    /// </summary>
    Panel ToolBarPanel { get;}
  }

  /// <summary>
  /// ��������� ������������ �������� � ������� ������������.
  /// ������ ������������ ��������� ����� EFPControlWithToolBar
  /// </summary>
  /// <typeparam name="T">��� ������������ ��������</typeparam>
  public interface IEFPControlWithToolBar<T> : IEFPControlWithToolBar
    where T : Control
  {
    /// <summary>
    /// �������� ����������� �������
    /// </summary>
    new T Control { get;}
  }

  #endregion

  /// <summary>
  /// ����������� ������� � ������� ������������
  /// </summary>
  /// <typeparam name="T">��� ������������ ��������</typeparam>
  public class EFPControlWithToolBar<T> : IEFPControlWithToolBar<T>
    where T : Control, new()
  {
    #region ��������� ����� ����������

    private class ExProvider : EFPBaseProvider
    {
      #region �����������

      public ExProvider(Panel toolBarPanel)
      {
        _ToolBarPanel = toolBarPanel;
      }

      #endregion

      #region ����

      private Panel _ToolBarPanel;

      private bool _ToolBarPanelAssigned;

      #endregion

      #region ���������������� ������

      // 19.06.2019
      // ������������� ������ ������������ ����������� �� � ������ InitCommandItemList(), � � Add() (� ������ ���������� �������� � ������ ���������)
      // � ������ �������� ������ ������������ �� ����������, ���� ����� ���������� �� ����� ��� ������� ��������� �� EFPApp.LoadComposition()

      protected override void OnControlProviderAdded(EFPControlBase controlProvider)
      {
        base.OnControlProviderAdded(controlProvider);

        if (_ToolBarPanel != null)
        {
          if (!_ToolBarPanelAssigned)
          {
            controlProvider.ToolBarPanel = _ToolBarPanel;
            _ToolBarPanelAssigned = true;
          }
        }
      }

      #endregion
    }

    #endregion

    #region ������������

    /// <summary>
    /// ������� ����� ����������� ������� ���� <typeparamref name="T"/> � ������ ������������. 
    /// �������� ����������� � <paramref name="parent"/>.Control.
    /// � �������� <paramref name="parent"/> ������ ������������ EFPTabPage
    /// </summary>
    /// <param name="parent">��������� ������������� ��������</param>
    public EFPControlWithToolBar(EFPControlBase parent)
      : this(parent.BaseProvider, parent.Control, new T())
    {
    }

    /// <summary>
    /// ������� ����� ����������� ������� ���� <typeparamref name="T"/> � ������ ������������. 
    /// �������� ����������� � ������������ �������� <paramref name="parent"/>.
    /// ������ ��� Panel ��� ������ ������.
    /// �������� ������ ������������.
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="parent">������������ ����������� �������</param>
    public EFPControlWithToolBar(EFPBaseProvider baseProvider, Control parent)
      : this(baseProvider, parent, new T())
    {
    }

    /// <summary>
    /// ���������� ������� ����������� ������� <paramref name="control"/> � ������� ����� ������ ������������. 
    /// �������� ����������� � ������������ �������� <paramref name="parent"/>.
    /// ������ ��� Panel ��� ������ ������.
    /// </summary>
    /// <param name="baseProvider">������� ���������</param>
    /// <param name="parent">������������ ����������� �������</param>
    /// <param name="control">�������� ����������� �������</param>
    public EFPControlWithToolBar(EFPBaseProvider baseProvider, Control parent, T control)
    {
#if DEBUG
      if (baseProvider == null)
        throw new ArgumentNullException("baseProvider");
      if (parent == null)
        throw new ArgumentNullException("parent");
      if (parent.Controls.Count > 0)
        throw new InvalidOperationException("������������ ����������� ������� " + parent.ToString() + " ��� �������� ��������");
      if (control == null)
        throw new ArgumentNullException("control");
      if (control.Parent != null)
        throw new ArgumentException("�������� ���������� ������� �� ������ ����� ��������", "control");
#endif


      _Control = control;
      _Control.Dock = DockStyle.Fill;
      parent.Controls.Add(_Control);

      if (EFPApp.ShowControlToolBars)
      {
        _ToolBarPanel = new Panel();
        _ToolBarPanel.Size = new Size(24, 24);
        _ToolBarPanel.Dock = DockStyle.Top;
        _ToolBarPanel.Visible = false; // 10.09.2012
        parent.Controls.Add(_ToolBarPanel);
      }

      _BaseProvider = new ExProvider(_ToolBarPanel);
      _BaseProvider.Parent = baseProvider;
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� � ������������
    /// </summary>
    public EFPBaseProvider BaseProvider { get { return _BaseProvider; } }
    private ExProvider _BaseProvider;

    /// <summary>
    /// �������� ����������� �������. �������� � ������������
    /// </summary>
    public T Control { get { return _Control; } }
    private T _Control;

    Control IEFPControlWithToolBar.Control { get { return _Control; } }

    /// <summary>
    /// ������ ��� ���������� ������������. ��������� � ������������. ����� ����
    /// null, ���� EFPApp.ShowControlToolBars=false
    /// </summary>
    public Panel ToolBarPanel { get { return _ToolBarPanel; } }
    private Panel _ToolBarPanel;

    #endregion
  }
}
