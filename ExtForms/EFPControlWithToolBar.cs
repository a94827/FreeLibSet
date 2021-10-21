using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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
