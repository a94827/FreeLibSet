using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using System.Xml;
using FreeLibSet.Core;
using FreeLibSet.Controls;

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

// ���� ������ �������� �������� ������ � ������������ ����������, ��������� �� ��������� ����������



namespace FreeLibSet.Forms
{
  /// <summary>
  /// �������� � ������������ ����������� ���������.
  /// ������ ������������ ��� �������� ������� ������� � ����������� ���������� ����������.
  /// ��� ������� � ������������ ��������� ���������� ����� ������������ ������������������ ������,
  /// ��������, EFPReportGridPage.
  /// </summary>
  /// <remarks>
  /// ����� � �������� ������������ �������� ������������ Panel ��� ������ ������������ �������,
  /// ������� ������� �� ������� ������� Form. 
  /// ������ �������� ��������� ������ ���������, ��� ������� ����� ����������, ����������� �� EFPControl/
  /// ��� �������� ����������� ����������� ��������� ������ ��������� ��������� ����� EFPFormProvider, 
  /// �.�. ���� �����, �� ������� ����������� ��������, �� ������������ � ��������� �������� �� �����. 
  /// ������ ����� ������� ������������ �������� EFPReportPage.BaseProvider ����� �������. 
  /// �����-��������� ������ ������ ����� ����������� � ���������� EFPBaseProvider.
  /// ���� �����-��������� ������������ �� ������ � ������ ������, �� � ��������������, � ��� ������ ���� �����
  /// �����������, ��������� ���� EFPFormProvider.
  /// 
  /// ��� ���������� ��������� ����������, �������������� �������� 
  /// EFPDataGridView.CommandItems.UseRefresh = false, ����� �������� ������� "�������� �����"
  /// </remarks>
  public class EFPReportControlPage : EFPReportPage
  {
    #region �����������

    /// <summary>
    /// ������� ��������
    /// </summary>
    public EFPReportControlPage()
    {
      _Control = null;
    }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ���������� ������������ ����������� ������� (������), �� ������� ����� 
    /// ������� �������� ������.  
    /// </summary>
    /// <param name="parentControl">������ ������, �� ������� ����� ����������� ��������</param>
    public override void AssignParentControl(Panel parentControl)
    {
      base.AssignParentControl(parentControl);

      // ����� ������������ ���� ����������� �������, ����� ��� ���� �� ����� ������ Disposing()
      SetPageCreated();
    }

    /// <summary>
    /// ������� ��������.
    /// ���� �������� Control �� ���� ��������� ��������, �� ��������� ����� � ������� "��� ������".
    /// </summary>
    /// <param name="parent"></param>
    protected override void CreatePage(Panel parent)
    {
      if (_Control == null)
      {
        _Control = new Label();
        _Control.Text = "��� ������";
      }
      _Control.Dock = DockStyle.Fill;

      // TODO: ��������� �������� ��� �����

      parent.Controls.Add(_Control);
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� ����������� �������. 
    /// </summary>
    public Control Control
    {
      get { return _Control; }
      set
      {
#if DEBUG
        if (value == null)
          throw new ArgumentNullException();
#endif

        Control ParentControl = null;
        if (base.PageCreated)
        {
#if DEBUG
          if (_Control == null)
            throw new BugException("Control==null ��� PageCreated==true");
#endif
          ParentControl = _Control.Parent;
        }

        if (_Control != null)
        {
          _Control.Parent = null;
          _Control.Dispose();
        }

        _Control = value;

        if (base.PageCreated)
        {
          _Control.Dock = DockStyle.Fill;
          ParentControl.Controls.Add(_Control);
        }
      }
    }
    private Control _Control;

    #endregion
  }

  /// <summary>
  /// �������� � ���������� ����������-���������
  /// </summary>
  public class EFPReportTextViewPage : EFPReportPage
  {
    #region �����������

    /// <summary>
    /// ������� �������� ������
    /// </summary>
    public EFPReportTextViewPage()
    {
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� ������ ��� ������
    /// �������� ����� ���������� �� �������� ������ ��������� (������ SetPageCreated)
    /// </summary>
    public string Text
    {
      get
      {
        if (_TheTextBox == null)
          return _Text;
        else
          return _TheTextBox.Control.Text;
      }
      set
      {
        if (_TheTextBox == null)
          _Text = value;
        else
          _TheTextBox.Control.Text = value;
      }
    }
    private string _Text;

    /// <summary>
    /// ���������� true, ���� ������ �� ���� �����������
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return String.IsNullOrEmpty(Text);
      }
    }

    /// <summary>
    /// ��������� ��������� ������������ ��������.
    /// �� ���������� ������ ����� �������� ��������.
    /// ��� �������������� �������� �������� ����������� SetPageCreated()
    /// </summary>
    public EFPTextBox TheTextBox { get { return _TheTextBox; } }
    private EFPTextBox _TheTextBox;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// �������� ��������
    /// </summary>
    /// <param name="parent"></param>
    protected override void CreatePage(Panel parent)
    {
      bool IsEmpty2 = IsEmpty;

      EFPControlWithToolBar<TextBox> cwt = new EFPControlWithToolBar<TextBox>(BaseProvider, parent);
      cwt.Control.Multiline = true;
      cwt.Control.ScrollBars = ScrollBars.Both;
      cwt.Control.WordWrap = false;
      cwt.Control.ReadOnly = true;
      _TheTextBox = new EFPTextBox(cwt);

      if (!IsEmpty2)
        _TheTextBox.Control.Text = _Text;

      _Text = null;

      _TheTextBox.PrepareCommandItems(); // ��������� TooBar
      SetControlName(_TheTextBox, "TextBox");
    }

    #endregion
  }


  // TODO: �� ������, ��� ���� ������������ EFPWebBrowser, � �� �����-������ ����������� �����
#if XXX
  /// <summary>
  /// �������� � ���������� HTML-���������
  /// </summary>
  public class EFPReportHtmlViewPage : EFPReportPage
  {
    #region �����������

    public EFPReportHtmlViewPage()
    {
      FSimpleText = "�����...";
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ��� ���������
    /// </summary>
    public EFPWebBrowser HtmlView { get { return FHtmlView; } }
    private EFPWebBrowser FHtmlView;

    /// <summary>
    /// ������� �������� ������ ��� ��������
    /// </summary>
    public string SimpleText
    {
      get
      {
        if (HtmlView == null)
          return FSimpleText;
        else
        {
          return HtmlView.Control.Document.Body.InnerText;
        }
      }
      set
      {
        if (HtmlView == null)
          FSimpleText = value;
        else
        {
          HtmlView.Control.Document.Body.InnerText = value;
          HtmlView.Control.Update();
        }
      }
    }
    private string FSimpleText;

    #endregion

    #region ���������������� ������

    protected override void CreatePage(Panel Parent)
    {
      EFPControlWithToolBar<WebBrowser> cwt = new EFPControlWithToolBar<WebBrowser>(BaseProvider, Parent);
      FHtmlView = new EFPWebBrowser(cwt);
      FHtmlView.Control.DocumentText = String.Empty;
      FHtmlView.Control.Document.Write(FSimpleText);

      FHtmlView.GetReadyCommandItems(); // ��������� TooBar
    }

    #endregion
  }
#endif

  /// <summary>
  /// �������� � ���������� XML-���������
  /// </summary>
  public class EFPReportXmlViewPage : EFPReportPage
  {
    #region �����������

    /// <summary>
    /// ������� �������� ������
    /// </summary>
    public EFPReportXmlViewPage()
    {
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���� � �������� XML-����� ��� ���������
    /// �������� ����� ���������� �� �������� ������ ��������� (������ SetPageCreated)
    /// </summary>
    public string XmlFilePath
    {
      get
      {
        if (_XmlView == null)
          return _XmlFilePath;
        else
          return _XmlView.Control.XmlFilePath;
      }
      set
      {
        if (_XmlView == null)
          _XmlFilePath = value;
        else
          _XmlView.Control.XmlFilePath = value;
        _XmlBytes = null;
        _XmlDocument = null;
        _XmlText = null;
      }
    }
    private string _XmlFilePath;

    /// <summary>
    /// �������� ��������� XML �� ������
    /// �������� ����� ���������� �� �������� ������ ��������� (������ SetPageCreated)
    /// </summary>
    public byte[] XmlBytes
    {
      get
      {
        if (_XmlView == null)
          return _XmlBytes;
        else
          return _XmlView.Control.XmlBytes;
      }
      set
      {
        if (_XmlView == null)
          _XmlBytes = value;
        else
          _XmlView.Control.XmlBytes = value;
        _XmlFilePath = null;
        _XmlDocument = null;
        _XmlText = null;
      }
    }
    private byte[] _XmlBytes;

    /// <summary>
    /// �������� ������ ��� XML-���������
    /// �������� ����� ���������� �� �������� ������ ��������� (������ SetPageCreated)
    /// </summary>
    public XmlDocument XmlDocument
    {
      get
      {
        if (_XmlView == null)
          return _XmlDocument;
        else
          return _XmlView.Control.XmlDocument;
      }
      set
      {
        if (_XmlView == null)
          _XmlDocument = value;
        else
          _XmlView.Control.XmlDocument = value;
        _XmlFilePath = null;
        _XmlBytes = null;
        _XmlText = null;
      }
    }
    private XmlDocument _XmlDocument;

    /// <summary>
    /// �������� ������ ��� ������ XML
    /// �������� ����� ���������� �� �������� ������ ��������� (������ SetPageCreated)
    /// </summary>
    public string XmlText
    {
      get
      {
        if (_XmlView == null)
          return _XmlText;
        else
          return _XmlView.Control.XmlText;
      }
      set
      {
        if (_XmlView == null)
          _XmlText = value;
        else
          _XmlView.Control.XmlText = value;
        _XmlFilePath = null;
        _XmlBytes = null;
        _XmlDocument = null;
      }
    }
    private string _XmlText;

    /// <summary>
    /// ���������� true, ���� ������ �� ���� �����������
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return String.IsNullOrEmpty(XmlFilePath) && XmlBytes == null && XmlDocument == null && String.IsNullOrEmpty(XmlText);
      }
    }

    /// <summary>
    /// ��������� ��������� ������������ ��������.
    /// �� ���������� ������ ����� �������� ��������.
    /// ��� �������������� �������� �������� ����������� SetPageCreated()
    /// </summary>
    public EFPXmlViewBox XmlView { get { return _XmlView; } }
    private EFPXmlViewBox _XmlView;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// �������� ��������
    /// </summary>
    /// <param name="parent"></param>
    protected override void CreatePage(Panel parent)
    {
      bool IsEmpty2 = IsEmpty;

      EFPControlWithToolBar<XmlViewBox> cwt = new EFPControlWithToolBar<XmlViewBox>(BaseProvider, parent);
      _XmlView = new EFPXmlViewBox(cwt);

      if (!IsEmpty2)
      {
        EFPApp.BeginWait("�������� XML-���������", "XML");
        try
        {
          if (!String.IsNullOrEmpty(_XmlFilePath))
            _XmlView.Control.XmlFilePath = _XmlFilePath;
          else if (_XmlBytes != null)
            _XmlView.Control.XmlBytes = _XmlBytes;
          else if (_XmlDocument != null)
            _XmlView.Control.XmlDocument = _XmlDocument;
          else if (!String.IsNullOrEmpty(_XmlText))
            _XmlView.Control.XmlText = _XmlText;
        }
        finally
        {
          EFPApp.EndWait();
        }
      }

      _XmlFilePath = null;
      _XmlBytes = null;
      _XmlDocument = null;
      _XmlText = null;

      _XmlView.PrepareCommandItems(); // ��������� TooBar
      SetControlName(_XmlView, "XmlView");
    }

    #endregion
  }
}