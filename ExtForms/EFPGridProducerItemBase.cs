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

namespace AgeyevAV.ExtForms
{

  /// <summary>
  /// ���������, ���������� ���������� � ������ EFPDataGridView ��� EFPDataTreeView,
  /// ������� ������������ ��� ������ ������� �������� EFPGridProducer
  /// </summary>
  public struct EFPDataViewRowInfo
  {
    #region �����������

    /// <summary>
    /// ��������� ���������
    /// </summary>
    /// <param name="controlProvider"></param>
    /// <param name="dataBoundItem"></param>
    /// <param name="values"></param>
    /// <param name="rowIndex"></param>
    public EFPDataViewRowInfo(IEFPDataView controlProvider, object dataBoundItem, INamedValuesAccess values, int rowIndex)
    {
      _ControlProvider = controlProvider;
      _DataBoundItem = dataBoundItem;
      _Values = values;
      _RowIndex = rowIndex;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ���������� ��� �������������� ���������
    /// </summary>
    public IEFPDataView ControlProvider { get { return _ControlProvider; } }
    private IEFPDataView _ControlProvider;


    /// <summary>
    /// ������, ��������� � ������� ������� ���������.
    /// ������, ��� ������ ������ DataRow.
    /// ����������� �������� Values ��� �������������� ������ ��� ���������� ������
    /// </summary>
    public object DataBoundItem { get { return _DataBoundItem; } }
    private object _DataBoundItem;

    /// <summary>
    /// ��������� ��� ���������� �������� ����� �� ������ ������
    /// </summary>
    public INamedValuesAccess Values { get { return _Values; } }
    private INamedValuesAccess _Values;

    /// <summary>
    /// ������ ������ ���������� ���������
    /// ��� �������������� ��������� ���������� �������� TreeNodeAdv.Row
    /// </summary>
    public int RowIndex { get { return _RowIndex; } }
    private int _RowIndex;

    #endregion

    #region ������

    /// <summary>
    /// ���������� "RowIndex=XXX".
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return "RowIndex=" + _RowIndex.ToString();
    }

    #endregion
  }

  #region �������

  /// <summary>
  /// ������� ����� ��� ���������� �������, ��������� � EFPGridProducer.
  /// </summary>
  public class EFPGridProducerBaseEventArgs : EventArgs
  {
    #region ���������� �����������

    internal EFPGridProducerBaseEventArgs(EFPGridProducerItemBase owner)
    {
      _Owner = owner;
      if (owner.SourceColumnNames == null)
        _SourceColumnNames = new string[1] { owner.Name };
      else
        _SourceColumnNames = owner.SourceColumnNames;
    }

    #endregion

    #region ��������

    private EFPGridProducerItemBase _Owner;

    /// <summary>
    /// ������ ���� �������� ��������.
    /// ���� ������� �� �������� �����������, �� ���������� ������ �� ������ ��������, ����������� �������� ��� ������� ��� ����������� ���������
    /// </summary>
    public string[] SourceColumnNames { get { return _SourceColumnNames; } }
    private string[] _SourceColumnNames;

    internal EFPDataViewRowInfo RowInfo { get { return _RowInfo; } set { _RowInfo = value; } }
    private EFPDataViewRowInfo _RowInfo;

    /// <summary>
    /// ��������� ���������� ��� �������������� ���������
    /// </summary>
    public IEFPDataView ControlProvider { get { return _RowInfo.ControlProvider; } }

    /// <summary>
    /// ������, ��������� � ������� ������� ���������.
    /// ������, ��� ������ ������ DataRow.
    /// ����������� �������� Values ��� �������������� ������ ��� ���������� ������
    /// </summary>
    public object DataBoundItem { get { return _RowInfo.DataBoundItem; } }

    /// <summary>
    /// ��������� ��� ���������� �������� ����� �� ������ ������
    /// </summary>
    public INamedValuesAccess Values { get { return _RowInfo.Values; } }

    /// <summary>
    /// ������ ������ ���������� ���������
    /// ��� �������������� ��������� ���������� �������� TreeNodeAdv.Row
    /// </summary>
    public int RowIndex { get { return _RowInfo.RowIndex; } }

    /// <summary>
    /// ������������ ���������������� ������, �������������� � �������
    /// </summary>
    public object Tag { get { return _Owner.Tag; } } // ������ ������������ ��������� �����

    #endregion

    #region ������ ���������������� ������� � ����� ������ �� �����

    /// <summary>
    /// �������� ��������� �������� ����.
    /// ��� ���� ������ ���� � ������ �������� �������� SourceColumnNames, �������� ��� ���������� ������������ �������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <returns>��������</returns>
    public string GetString(string columnName)
    {
      return DataTools.GetString(Values.GetValue(columnName));
    }

    /// <summary>
    /// �������� �������� �������� ����.
    /// ��� ���� ������ ���� � ������ �������� �������� SourceColumnNames, �������� ��� ���������� ������������ �������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <returns>��������</returns>
    public int GetInt(string columnName)
    {
      return DataTools.GetInt(Values.GetValue(columnName));
    }

    /// <summary>
    /// �������� �������� �������� ����.
    /// ��� ���� ������ ���� � ������ �������� �������� SourceColumnNames, �������� ��� ���������� ������������ �������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <returns>��������</returns>
    public long GetInt64(string columnName)
    {
      return DataTools.GetInt64(Values.GetValue(columnName));
    }

    /// <summary>
    /// �������� �������� �������� ����.
    /// ��� ���� ������ ���� � ������ �������� �������� SourceColumnNames, �������� ��� ���������� ������������ �������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <returns>��������</returns>
    public float GetSingle(string columnName)
    {
      return DataTools.GetSingle(Values.GetValue(columnName));
    }

    /// <summary>
    /// �������� �������� �������� ����.
    /// ��� ���� ������ ���� � ������ �������� �������� SourceColumnNames, �������� ��� ���������� ������������ �������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <returns>��������</returns>
    public double GetDouble(string columnName)
    {
      return DataTools.GetDouble(Values.GetValue(columnName));
    }

    /// <summary>
    /// �������� �������� �������� ����.
    /// ��� ���� ������ ���� � ������ �������� �������� SourceColumnNames, �������� ��� ���������� ������������ �������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <returns>��������</returns>
    public decimal GetDecimal(string columnName)
    {
      return DataTools.GetDecimal(Values.GetValue(columnName));
    }

    /// <summary>
    /// �������� �������� ����������� ����.
    /// ��� ���� ������ ���� � ������ �������� �������� SourceColumnNames, �������� ��� ���������� ������������ �������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <returns>��������</returns>
    public bool GetBool(string columnName)
    {
      return DataTools.GetBool(Values.GetValue(columnName));
    }

    /// <summary>
    /// �������� �������� ����, ����������� ���� �/��� �����.
    /// ���� ���� �������� DBNull, ������������ �������������������� ����.
    /// ��� ���� ������ ���� � ������ �������� �������� SourceColumnNames, �������� ��� ���������� ������������ �������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <returns>��������</returns>
    public DateTime GetDateTime(string columnName)
    {
      return DataTools.GetDateTime(Values.GetValue(columnName));
    }

    /// <summary>
    /// �������� �������� ����, ����������� ���� �/��� �����.
    /// ���� ���� �������� DBNull, ������������ null.
    /// ��� ���� ������ ���� � ������ �������� �������� SourceColumnNames, �������� ��� ���������� ������������ �������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <returns>��������</returns>
    public DateTime? GetNullableDateTime(string columnName)
    {
      return DataTools.GetNullableDateTime(Values.GetValue(columnName));
    }

    /// <summary>
    /// �������� �������� ����, ����������� �������� �������.
    /// ���� ���� �������� DBNull, ������������ TimeSpan.Zero.
    /// ��� ���� ������ ���� � ������ �������� �������� SourceColumnNames, �������� ��� ���������� ������������ �������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <returns>��������</returns>
    public TimeSpan GetTimeSpan(string columnName)
    {
      return DataTools.GetTimeSpan(Values.GetValue(columnName));
    }

    /// <summary>
    /// �������� �������� ���� ���� Guid.
    /// ���� ���� �������� DBNull, ������������ Guid.Empty.
    /// ��� ���� ������ ���� � ������ �������� �������� SourceColumnNames, �������� ��� ���������� ������������ �������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <returns>��������</returns>
    public Guid GetGuid(string columnName)
    {
      return DataTools.GetGuid(Values.GetValue(columnName));
    }

    /// <summary>
    /// ���������� true, ���� ���� �������� �������� null ��� DBNull.
    /// ��� ���� ������ ���� � ������ �������� �������� SourceColumnNames, �������� ��� ���������� ������������ �������.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <returns>������� ������� ��������</returns>
    public bool IsNull(string columnName)
    {
      object v = Values.GetValue(columnName);
      return (v == null) || (v is DBNull);
    }

    /// <summary>
    /// �������� �������� ������������� ����.
    /// ��� ���� ������ ���� � ������ �������� �������� SourceColumnNames, �������� ��� ���������� ������������ �������.
    /// </summary>
    /// <typeparam name="T">��� ������������</typeparam>
    /// <param name="columnName">��� ����</param>
    /// <returns>��������</returns>
    public T GetEnum<T>(string columnName)
      where T : struct
    {
      return DataTools.GetEnum<T>(Values.GetValue(columnName));
    }

    #endregion

    #region ������ ���������������� ������� �� ������� ��������� �������

    /// <summary>
    /// �������� ��������� �������� ����.
    /// </summary>
    /// <param name="sourceColumnIndex">������ ��������� ������� � ������� SourceColumnNames.
    /// ���� ������� �� �������� �����������, �� ����������� ������ �������� 0, ����� �������� �������� ����</param>
    /// <returns>��������</returns>
    public string GetString(int sourceColumnIndex)
    {
      return DataTools.GetString(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// �������� �������� �������� ����.
    /// </summary>
    /// <param name="sourceColumnIndex">������ ��������� ������� � ������� SourceColumnNames.
    /// ���� ������� �� �������� �����������, �� ����������� ������ �������� 0, ����� �������� �������� ����</param>
    /// <returns>��������</returns>
    public int GetInt(int sourceColumnIndex)
    {
      return DataTools.GetInt(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// �������� �������� �������� ����.
    /// </summary>
    /// <param name="sourceColumnIndex">������ ��������� ������� � ������� SourceColumnNames.
    /// ���� ������� �� �������� �����������, �� ����������� ������ �������� 0, ����� �������� �������� ����</param>
    /// <returns>��������</returns>
    public long GetInt64(int sourceColumnIndex)
    {
      return DataTools.GetInt64(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// �������� �������� �������� ����.
    /// </summary>
    /// <param name="sourceColumnIndex">������ ��������� ������� � ������� SourceColumnNames.
    /// ���� ������� �� �������� �����������, �� ����������� ������ �������� 0, ����� �������� �������� ����</param>
    /// <returns>��������</returns>
    public float GetSingle(int sourceColumnIndex)
    {
      return DataTools.GetSingle(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// �������� �������� �������� ����.
    /// </summary>
    /// <param name="sourceColumnIndex">������ ��������� ������� � ������� SourceColumnNames.
    /// ���� ������� �� �������� �����������, �� ����������� ������ �������� 0, ����� �������� �������� ����</param>
    /// <returns>��������</returns>
    public double GetDouble(int sourceColumnIndex)
    {
      return DataTools.GetDouble(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// �������� �������� �������� ����.
    /// </summary>
    /// <param name="sourceColumnIndex">������ ��������� ������� � ������� SourceColumnNames.
    /// ���� ������� �� �������� �����������, �� ����������� ������ �������� 0, ����� �������� �������� ����</param>
    /// <returns>��������</returns>
    public decimal GetDecimal(int sourceColumnIndex)
    {
      return DataTools.GetDecimal(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// �������� �������� ����������� ����.
    /// </summary>
    /// <param name="sourceColumnIndex">������ ��������� ������� � ������� SourceColumnNames.
    /// ���� ������� �� �������� �����������, �� ����������� ������ �������� 0, ����� �������� �������� ����</param>
    /// <returns>��������</returns>
    public bool GetBool(int sourceColumnIndex)
    {
      return DataTools.GetBool(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// �������� �������� ����, ����������� ���� �/��� �����.
    /// ���� ���� �������� DBNull, ������������ �������������������� ����.
    /// </summary>
    /// <param name="sourceColumnIndex">������ ��������� ������� � ������� SourceColumnNames.
    /// ���� ������� �� �������� �����������, �� ����������� ������ �������� 0, ����� �������� �������� ����</param>
    /// <returns>��������</returns>
    public DateTime GetDateTime(int sourceColumnIndex)
    {
      return DataTools.GetDateTime(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// �������� �������� ����, ����������� ���� �/��� �����.
    /// ���� ���� �������� DBNull, ������������ null.
    /// </summary>
    /// <param name="sourceColumnIndex">������ ��������� ������� � ������� SourceColumnNames.
    /// ���� ������� �� �������� �����������, �� ����������� ������ �������� 0, ����� �������� �������� ����</param>
    /// <returns>��������</returns>
    public DateTime? GetNullableDateTime(int sourceColumnIndex)
    {
      return DataTools.GetNullableDateTime(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// �������� �������� ����, ����������� �������� �������.
    /// ���� ���� �������� DBNull, ������������ TimeSpan.Zero.
    /// </summary>
    /// <param name="sourceColumnIndex">������ ��������� ������� � ������� SourceColumnNames.
    /// ���� ������� �� �������� �����������, �� ����������� ������ �������� 0, ����� �������� �������� ����</param>
    /// <returns>��������</returns>
    public TimeSpan GetTimeSpan(int sourceColumnIndex)
    {
      return DataTools.GetTimeSpan(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// �������� �������� ���� ���� Guid.
    /// ���� ���� �������� DBNull, ������������ Guid.Empty.
    /// </summary>
    /// <param name="sourceColumnIndex">������ ��������� ������� � ������� SourceColumnNames.
    /// ���� ������� �� �������� �����������, �� ����������� ������ �������� 0, ����� �������� �������� ����</param>
    /// <returns>��������</returns>
    public Guid GetGuid(int sourceColumnIndex)
    {
      return DataTools.GetGuid(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    /// <summary>
    /// ���������� true, ���� ���� �������� �������� null ��� DBNull.
    /// ��� ���� ������ ���� � ������ �������� �������� SourceColumnNames, �������� ��� ���������� ������������ �������.
    /// </summary>
    /// <param name="sourceColumnIndex">������ ��������� ������� � ������� SourceColumnNames.
    /// ���� ������� �� �������� �����������, �� ����������� ������ �������� 0, ����� �������� �������� ����</param>
    /// <returns>������� ������� ��������</returns>
    public bool IsNull(int sourceColumnIndex)
    {
      object v = Values.GetValue(SourceColumnNames[sourceColumnIndex]);
      return (v == null) || (v is DBNull);
    }

    /// <summary>
    /// �������� �������� ������������� ����.
    /// </summary>
    /// <typeparam name="T">��� ������������</typeparam>
    /// <param name="sourceColumnIndex">������ ��������� ������� � ������� SourceColumnNames.
    /// ���� ������� �� �������� �����������, �� ����������� ������ �������� 0, ����� �������� �������� ����</param>
    /// <returns>��������</returns>
    public T GetEnum<T>(int sourceColumnIndex)
      where T : struct
    {
      return DataTools.GetEnum<T>(Values.GetValue(SourceColumnNames[sourceColumnIndex]));
    }

    #endregion
  }

  #region ������������ EFPGridProducerValueReason

  /// <summary>
  /// ������� ������ ������� EFPGridProducerColumn.ValueNeeded
  /// </summary>
  public enum EFPGridProducerValueReason
  {
    /// <summary>
    /// ��������� �������� ������
    /// </summary>
    Value,

    /// <summary>
    /// ��������� ����������� ���������
    /// </summary>
    ToolTipText
  }

  #endregion

  /// <summary>
  /// ��������� ������� EFPGridProducerColumn � EFPGridProducerToolTip.ValueNeeded
  /// </summary>
  public class EFPGridProducerValueNeededEventArgs : EFPGridProducerBaseEventArgs
  {
    #region ���������� �����������

    internal EFPGridProducerValueNeededEventArgs(EFPGridProducerItemBase owner)
      : base(owner)
    {
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� ������ ������� EFPGridProducerColumn.ValueNeeded.
    /// ��� ������� EFPGridProducerToolTip.ValueNeeded ������ ���������� ToolTipText
    /// </summary>
    public EFPGridProducerValueReason Reason
    {
      get { return _Reason; }
      internal set { _Reason = value; }
    }
    private EFPGridProducerValueReason _Reason;

    /// <summary>
    /// ���� ������ ���� �������� ����������� ��������
    /// </summary>
    public object Value
    {
      get { return _Value; }
      set { _Value = value; }
    }
    private object _Value;

    /// <summary>
    /// ����� ����������� ���������
    /// </summary>
    public string ToolTipText
    {
      get { return _ToolTipText; }
      set { _ToolTipText = value; }
    }
    private string _ToolTipText;

    #endregion
  }

  /// <summary>
  /// ������� ������� GridProducerUserColumn.ValueNeeded
  /// </summary>
  /// <param name="sender">��������� ������� GridProducerColumn</param>
  /// <param name="args">��������� �������</param>
  public delegate void EFPGridProducerValueNeededEventHandler(object sender,
    EFPGridProducerValueNeededEventArgs args);

  #endregion

  /// <summary>
  /// ������� ����� ��� EFPGridProducerColumn � EFPGridProducerToolTip
  /// </summary>
  public abstract class EFPGridProducerItemBase : IObjectWithCode
  {
    #region ���������� �����������

    internal EFPGridProducerItemBase(string name, string[] sourceColumnNames)
    {
#if DEBUG
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
      if (name[0] >= '0' && name[0] <= '9')
        throw new ArgumentException("��� �� ����� ���������� � �����, �.�. ��� ������������ � ����������� ��������� ���������", "name");

      EFPApp.CheckMainThread();
#endif

      _Name = name;

      if (sourceColumnNames != null)
      {
        // ����� ���� ������ ������
        for (int i = 0; i < sourceColumnNames.Length; i++)
        {
          if (String.IsNullOrEmpty(sourceColumnNames[i]))
            throw new ArgumentException("SourceColumnNames �� ����� ��������� ������ ��������", "sourceColumnNames");
          if (sourceColumnNames[i].IndexOf(',') >= 0)
            throw new ArgumentException("�������� � SourceColumnNames �� ����� ��������� �������", "sourceColumnNames");

          // 21.05.2021
          if (String.Compare(name, sourceColumnNames[i], StringComparison.OrdinalIgnoreCase)==0)
            throw new ArgumentException("������� � SourceColumnNames ��������� � ������ ������������ �������/��������� \""+name+"\"", "sourceColumnNames");

          for (int j = 0; j < i; j++)
          {
            if (String.Compare(sourceColumnNames[i], sourceColumnNames[j], StringComparison.OrdinalIgnoreCase) == 0)
              throw new ArgumentException("��� ���������� �������� \"" + sourceColumnNames[i] + "\" � SourceColumnNames", "sourceColumnNames");
          }
        }

        _SourceColumnNames = sourceColumnNames;
        _ValueNeededArgs = new EFPGridProducerValueNeededEventArgs(this);
      }
    }

    #endregion

    #region �������� ��������

    /// <summary>
    /// �������� ���.
    /// ��� ��������, �������������� �������/��������� ����� ����� ����
    /// �������� � ������������.
    /// </summary>
    public string Name { get { return _Name; } }
    private string _Name;

    string IObjectWithCode.Code { get { return _Name; } }

    /// <summary>
    /// ���������� �������� Name
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return _Name;
    }

    /// <summary>
    /// ��� ��� ����������� � ��������� ���������. ���� �� ������ � �����
    /// ����, �� ���������� �������� HeaderText ��� ColumnName (���� HeaderText ������).
    /// ����� �������� ������������ ��� ����������� ����������� ��������� ��� 
    /// ��������� ������� �� ��������� �������, ���� ���� �� ���������� �������� HeaderToolTipText
    /// </summary>
    public virtual string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
          return GetDefaultDisplayName();
        else
          return _DisplayName;
      }
      set
      {
        _DisplayName = value;
      }
    }
    private string _DisplayName;

    /// <summary>
    /// ���������������� ��� EFPGridProducerColumn
    /// </summary>
    /// <returns></returns>
    protected virtual string GetDefaultDisplayName()
    {
      return _Name;
    }

    /// <summary>
    /// ������������ ���������������� ������
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    #endregion

    #region SourceColumnNames

    /// <summary>
    /// ���������� true, ���� �������/��������� �������� �����������, � �� ��������� ������ �� ��������� ������ (�������)
    /// </summary>
    public bool IsCalculated { get { return _SourceColumnNames != null; } }

    /// <summary>
    /// �������� ������� ��� ������������ �������.
    /// ���� ������� ��������� ������ �� ���������, �� �������� ����� null.
    /// ���� ������� �� ��������� � ������ �������� ��������, �� �������� ����� ������� ������� �����.
    /// </summary>
    public string[] SourceColumnNames { get { return _SourceColumnNames; } }
    private string[] _SourceColumnNames;

    /// <summary>
    /// �������� ����� �����, ������� ������ ���� � ������ ������.
    /// ���� ������� �������� �����������, � ������ ����������� ����� �������� �������� SourceColumnNames.
    /// ����� ����������� ��� Name ��� �������������� �������/���������
    /// </summary>
    /// <param name="columns">������ ��� ���������� ���� �����</param>
    public virtual void GetColumnNames(IList<string> columns)
    {
#if DEBUG
      if (columns == null)
        throw new ArgumentNullException("columns");
#endif

      if (_SourceColumnNames == null)
        columns.Add(Name);
      else
      {
        for (int i = 0; i < _SourceColumnNames.Length; i++)
          columns.Add(_SourceColumnNames[i]);
      }
    }

    #endregion

    #region ������� ValueNeeded

    /// <summary>
    /// ���������� ���������� ��� ������������ ������� ������ ���, ����� ��������� �������� ��������.
    /// ��� �������������� ������� �� ����������
    /// </summary>
    public event EFPGridProducerValueNeededEventHandler ValueNeeded;

    /// <summary>
    /// ��������� �������� ������������ �������/���������.
    /// ���� �������/��������� �� �������� �����������, ����� �� ����������
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnValueNeeded(EFPGridProducerValueNeededEventArgs args)
    {
      if (ValueNeeded != null)
        ValueNeeded(this, args);
    }

    /// <summary>
    /// ����� ������ ��� �� ��������� ����� ������ ����������
    /// </summary>
    private EFPGridProducerValueNeededEventArgs _ValueNeededArgs;


    internal void DoGetValue(EFPGridProducerValueReason reason, EFPDataViewRowInfo rowInfo, out object value, out string toolTipText)
    {
#if DEBUG
      EFPApp.CheckMainThread();
#endif


      if (IsCalculated)
      {
        _ValueNeededArgs.RowInfo = rowInfo;
        _ValueNeededArgs.Reason = reason;
        _ValueNeededArgs.Value = null;
        _ValueNeededArgs.ToolTipText = null;
        OnValueNeeded(_ValueNeededArgs);
        _ValueNeededArgs.RowInfo = new EFPDataViewRowInfo(); // ����������� ������
        value = _ValueNeededArgs.Value;
        toolTipText = _ValueNeededArgs.ToolTipText;
      }
      else
      {
        value = rowInfo.Values.GetValue(Name);
        toolTipText = null;
      }

      if (value == null || value is DBNull)
        value = EmptyValue;
    }

    /// <summary>
    /// ������ ��������.
    /// ������������ ������� GetValue(), ����� �������� ���� ��� ��������, ����������� ������������ ������� ValueNeeded,
    /// ����� null ��� DBNull.
    /// �� ��������� - null.
    /// </summary>
    public object EmptyValue
    {
      get { return _EmptyValue; }
      set { _EmptyValue = value; }
    }
    private object _EmptyValue;

    #endregion
  }
}
