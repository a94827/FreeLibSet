using System;
using System.Collections.Generic;
using System.Text;

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

/*
 * ������������� ������ �� ���� EFPDataGridView
 */

#if XXX

namespace FreeLibSet.Forms.Docs
{
  public class EFPDBxGridViewHieHandler:EFPDataGridViewHieHandler
  {
#region �����������

    public EFPDBxGridViewHieHandler(EFPDBxGridView ControlProvider)
      :base(ControlProvider)
    { 
    }

#endregion

#region ��������

    public EFPDBxGridView ControlProvider { get { return (EFPDBxGridView)(base.ControlProvider); } }

#endregion

#region ������� ����������


    /// <summary>
    /// ���� ����� ����� ���� ������ �� ����������� GridHandler.CommandItems.GetDocSel
    /// �� ��������� ������ �� ��������� � ����������� ������� ����������
    /// ��������������. ���� ������������ GridReportHiePage, �� ����������� ��
    /// ����� OnGetDocSel ��� ���������� �����������
    /// </summary>
    /// <param name="Args"></param>
    public void OnGetDocSel(object Sender, EFPAccDepGridDocSelEventArgs Args)
    {
      // ��������� �� ���������� �������� �������� � �������, ����� ��� �������
      // �� ������ ������ � EFPDocComboBox �������� ����� ������������� ���������,
      // ���� ��� ������ ���� ������ �� ���������� ��� ����������, ��������, "�����������"
      for (int i = 0; i < Levels.Length; i++)
        Levels[i].OnGetDocSel(Args);
    }

    /// <summary>
    /// ���������� ������ �������� ������� ���������� ��� ������������ ����� ����������
    /// ��������� ������
    /// </summary>
    /// <param name="ControlProvider">��������� �������� ��� �������, ��������� CreateResTable()</param>
    /// <param name="Reason">���������� �������</param>
    /// <param name="RowIndices">������� ����� ������, ��� ������� ��������� �������</param>
    /// <returns>������� ����������</returns>
    public AccDepDocSel CreateDocSel(EFPAccDepGrid ControlProvider, EFPAccDepGridDocSelReason Reason, int[] RowIndices)
    {
      EFPAccDepGridDocSelEventArgs Args = new EFPAccDepGridDocSelEventArgs(ControlProvider, Reason, RowIndices);
      OnGetDocSel(null, Args);
      return Args.DocSel;
    }
#endregion
  }
}

#endif

