using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.ExtDB.Docs;
using AgeyevAV.Config;
using AgeyevAV.DependedValues;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace AgeyevAV.ExtForms.Docs
{

  /// <summary>
  /// ������������� ���������� ���� ��� �������� ���������������� ������ ��� XmlCfgPart.
  /// ����� ����� �������������� � �������� �������� ��� ����������� ���������� ������/������ ������,
  /// ���� ��������������. � ��������� ������ ��������� ��������� �������, ����������� IDocEditItem
  /// (�������� DocValueTextBox) � �������������� ��� �������� ��������. ��� �� �������� �� ����������
  /// DBxDocValues, ���������� �� �������� Values ����� �������
  /// </summary>
  public class XmlCfgDocEditItem : DocEditItemWithChildren
  {
    #region �����������

    /// <summary>
    /// ������� ������, �������������� � ���������� ���� � ������� XML
    /// </summary>
    /// <param name="docValue">������ ��� ������� � ��������</param>
    public XmlCfgDocEditItem(DBxDocValue docValue)
    {
      _DocValue = docValue;

      base.ChangeInfo.DisplayName = docValue.DisplayName;
      _DataChangeInfo = new DepChangeInfoItem();
      base.ChangeInfoList.Add(_DataChangeInfo);


      _Data = new TempCfg();
      //FData.Changed += new EventHandler(Data_Changed);

      _Values = new XmlCfgDocValues(_Data, docValue.IsReadOnly);
    }

    //void Data_Changed(object Sender, EventArgs Args)
    //{
    //  ChangeInfo.Changed = true;
    //}

    #endregion

    #region ��������

    /// <summary>
    /// �������� XML-����, ����������� � ���� ������.
    /// �������� � ������������.
    /// </summary>
    public DBxDocValue DocValue { get { return _DocValue; } }
    private DBxDocValue _DocValue;

    /// <summary>
    /// ���������������� ������, ������� ������ �������� � ������������
    /// </summary>
    public TempCfg Data { get { return _Data; } }
    private TempCfg _Data;

    private string _OrgValue;

    private DepChangeInfoItem _DataChangeInfo;

    #endregion

    #region ������ � ��������� ��� � DBxDocValue

    /// <summary>
    /// ����������� � ���������
    /// </summary>
    public XmlCfgDocValues Values { get { return _Values; } }
    private XmlCfgDocValues _Values;

    #endregion

    #region ������ � ������

    /// <summary>
    /// ��������� � ������ Data �������� �� ���������� ���� DocValue
    /// </summary>
    public override void ReadValues()
    {
      _OrgValue = _DocValue.AsString;
      try
      {
        _Data.AsXmlText = _DocValue.AsString;
      }
      catch
      {
      }

      base.ReadValues();
    }

    /// <summary>
    /// ���������� �� ������� Data �������� � ��������� ���� DocValue
    /// </summary>
    public override void WriteValues()
    {
      base.WriteValues();

      _DocValue.SetString(_Data.AsXmlText);
      // ���� ����� ������� ������� Changed ��������� ���������� ��������� � �������� ��������, ����� ��������
      _DataChangeInfo.Changed = (_DocValue.AsString != _OrgValue);
    }

    #endregion
  }
}
