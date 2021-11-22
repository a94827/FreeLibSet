// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data.Docs;
using FreeLibSet.Config;
using FreeLibSet.DependedValues;

namespace FreeLibSet.Forms.Docs
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
