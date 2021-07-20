using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.FIAS;
using System.Windows.Forms;

/*
 * The BSD License
 * 
 * Copyright (c) 2020, Ageyev A.V.
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

namespace AgeyevAV.ExtForms.FIAS
{
  /// <summary>
  /// ������ ��� �������������� ��� ��������� ������
  /// </summary>
  public class FiasAddressDialog
  {
    #region �����������

    /// <summary>
    /// ������� ������ ��� �������������� ������ � ����������� �� ���������
    /// </summary>
    /// <param name="ui">���������������� ��������� ����</param>
    public FiasAddressDialog(FiasUI ui)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;

      _Title = "�����";
      _EditorLevel = FiasTools.DefaultEditorLevel;
      _PostalCodeEditable = true;
      _MinRefBookLevel = FiasTools.DefaultMinRefBookLevel;
      _Address = new FiasAddress();
      _DialogPosition = new EFPDialogPosition();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���������������� ���������.
    /// �������� � ������������.
    /// </summary>
    public FiasUI UI { get { return _UI; } }
    private FiasUI _UI;

    /// <summary>
    /// ��������� �����
    /// </summary>
    public string Title
    {
      get { return _Title; }
      set { _Title = value; }
    }
    private string _Title;

    /// <summary>
    /// �������, �� �������� ����� ������� �����.
    /// �� ��������� - FiasEditorLevel.Room.
    /// </summary>
    public FiasEditorLevel EditorLevel
    {
      get { return _EditorLevel; }
      set { _EditorLevel = value; }
    }
    private FiasEditorLevel _EditorLevel;

    /// <summary>
    /// ����� �� ����� ���� ������?
    /// �� ��������� - false - ����� ������ ���� ��������.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return _CanBeEmpty; }
      set { _CanBeEmpty = value; }
    }
    private bool _CanBeEmpty;

    /// <summary>
    /// �������� ��������������, ���� ����� �� ��������
    /// �� ��������� - false - �� ��������.
    /// ��������� ������ ��� ��������� �������� CanBeEmpty=true, ����� ����� ���������� ������, � �� ��������������.
    /// </summary>
    public bool WarningIfEmpty
    {
      get { return _WarningIfEmpty; }
      set { _WarningIfEmpty = value; }
    }
    private bool _WarningIfEmpty;

    /// <summary>
    /// ����� �� ����� ���� ����������� �������� (��������, ������ ������ ������)?
    /// �� ��������� - false - ����� ������ ���� �������� �������� �������� EditorLevel.
    /// ��������, ���� EditorLevel=Room, �� ������ ���� �����, ��� �������, ���.
    /// </summary>
    public bool CanBePartial
    {
      get { return _CanBePartial; }
      set { _CanBePartial = value; }
    }
    private bool _CanBePartial;

    /// <summary>
    /// �������� ��������������, ���� ����� �������� �������� (��������, ������ ������ ������).
    /// �� ��������� - false - �� ��������.
    /// ��������� ������ ��� ��������� �������� CanBePartial=true, ����� ����� ���������� ������, � �� ��������������.
    /// </summary>
    public bool WarningIfPartial
    {
      get { return _WarningIfPartial; }
      set { _WarningIfPartial = value; }
    }
    private bool _WarningIfPartial;

    /// <summary>
    /// ����� �� ������������� �������� ������?
    /// �� ��������� - true
    /// </summary>
    public bool PostalCodeEditable
    {
      get { return _PostalCodeEditable; }
      set { _PostalCodeEditable = value; }
    }
    private bool _PostalCodeEditable;


    /// <summary>
    /// ������ ����������� ������� ������, ������� ������ ���� ������ �� �����������, � �� ����� �������.
    /// �� ��������� - FiasLevel.City, �� ���� ������, ����� � ����� ������ ���� � ����������� ����, � ���������� �����,
    /// ��� ������������� - ������ �������, ���� ��� ��� � �����������.
    /// �������� Unknown ��������� ��� ��������. 
    /// ����������� ����� ��������, ������� House � Room, ���� ��� �� ������� �� ������� FiasEditorLevel.
    /// </summary>
    public FiasLevel MinRefBookLevel
    {
      get { return _MinRefBookLevel; }
      set { _MinRefBookLevel = value; }
    }
    private FiasLevel _MinRefBookLevel;

    /// <summary>
    /// ���� �������� ���������� � true, �� �������� ����� ������ � ������ ���������, � �� ��������������.
    /// �� ��������� - false
    /// </summary>
    public bool ReadOnly
    {
      get { return _ReadOnly; }
      set { _ReadOnly = value; }
    }
    private bool _ReadOnly;

    /// <summary>
    /// ������������� �����
    /// </summary>
    public FiasAddress Address
    {
      get { return _Address; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Address = value;
      }
    }
    private FiasAddress _Address;

    /// <summary>
    /// �������������� ������ ������� ������, ���� ������������ ��������� ������ ������
    /// </summary>
    internal bool InsideSearch;

    /// <summary>
    /// ������� ����� ������� �� ������.
    /// �� ��������� ���� ������� ������������ ������������ EFPApp.DefaultScreen.
    /// </summary>
    public EFPDialogPosition DialogPosition { get { return _DialogPosition; } }
    private EFPDialogPosition _DialogPosition;

    #endregion

    #region ����� �������

    /// <summary>
    /// ������� ������ �� �����
    /// </summary>
    /// <returns>��������� ������ �������</returns>
    public DialogResult ShowDialog()
    {
      OKCancelForm form = new OKCancelForm();
      form.FormBorderStyle = FormBorderStyle.Sizable;
      form.MaximizeBox = true;
      form.Text = Title;
      form.Icon = EFPApp.MainImageIcon("FIAS.Address");
      form.FormProvider.OwnStatusBar = true;
      form.Width = 640;

      FiasAddressPanel panel = new FiasAddressPanel();
      panel.Dock = DockStyle.Top;
      form.MainPanel.Controls.Add(panel);
      panel.SizeChanged += new EventHandler(panel_SizeChanged); // 18.08.2020
      form.FormProvider.Shown += new EventHandler(form_Shown); // 18.08.2020

      EFPFiasAddressPanel efp = new EFPFiasAddressPanel(form.FormProvider, panel, _UI, _EditorLevel);
      efp.Address = Address.Clone();
      efp.ReadOnly = _ReadOnly;
      efp.CanBeEmpty = _CanBeEmpty;
      efp.WarningIfEmpty = _WarningIfEmpty;
      efp.CanBePartial = _CanBePartial;
      efp.WarningIfPartial = _WarningIfPartial;
      efp.MinRefBookLevel = this.MinRefBookLevel;
      efp.PostalCodeEditable = this.PostalCodeEditable;
      if (InsideSearch)
        efp.MoreCommandItems["View", "Search"].Enabled = false;
      if (_ReadOnly)
        WinFormsTools.OkCancelFormToOkOnly(form);

      DialogResult res = EFPApp.ShowDialog(form, true, DialogPosition);
      if (res == DialogResult.OK)
        Address = efp.Address;
      return res;
    }

    void form_Shown(object sender, EventArgs args)
    {
      EFPFormProvider formProvider = (EFPFormProvider)sender;
      OKCancelForm form = formProvider.Form as OKCancelForm;
      Control panel = form.MainPanel.Controls[0];
      panel_SizeChanged(panel, EventArgs.Empty);
    }

    private static bool _InsideSizeChanged = false;
    private static void panel_SizeChanged(object sender, EventArgs args)
    {
      FiasAddressPanel panel = (FiasAddressPanel)sender;
      OKCancelForm form = panel.FindForm() as OKCancelForm;
      if (form == null)
        return;

      if (_InsideSizeChanged)
        return;
      _InsideSizeChanged = true;
      try
      {
        int h = form.Height - form.MainPanel.Height + panel.Height;
        form.MinimumSize = new System.Drawing.Size(form.Width - panel.Width + panel.MinimumSize.Width, h);
        form.MaximumSize = new System.Drawing.Size(Screen.FromControl(form).WorkingArea.Width, h);
      }
      finally
      {
        _InsideSizeChanged = false;
      }
    }

    #endregion
  }
}
