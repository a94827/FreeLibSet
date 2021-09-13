using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.Config;
using System.Data;
using AgeyevAV.Trees;

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

namespace AgeyevAV.ExtDB.Docs
{
  /// <summary>
  /// ������� ����� ��� ���������� ��������������� ��������.
  /// ������� ������������ � ��������� ���������� � ���������� �������
  /// </summary>
  public abstract class DBxCommonFilter : IObjectWithCode
  {
    #region �����������

    /// <summary>
    /// ������� ������, ��������� �������� UseSqlFilter = true.
    /// </summary>
    public DBxCommonFilter()
    {
      _UseSqlFilter = true;
    }

    #endregion

    #region ���

    /// <summary>
    /// ��� �������. ������������ ��� ������ / ������ ������� ��� ��� ������ ������������.
    /// �������� ������ ���� ����������� � ������������ ������������ ������ ��� � ���������������� ���� �� ������������� � ��������� DBxCommonFilters
    /// </summary>
    public string Code
    {
      get { return _Code; }
      set
      {
        // �� ��������� ������ ��������, ��� ��� ��� ��������� ������ ����� ������ �������������� � ���������

        if (_Owner != null)
          throw new InvalidOperationException("��������� �������� Code ����������� ������ �� ������������� ������� � ���������");
        _Code = value;
      }
    }
    private string _Code;

    #endregion

    #region ��������� - ��������

    /// <summary>
    /// ��������� - ��������.
    /// �������� ���������� null �� ������������� ������� � ���������
    /// </summary>
    public DBxCommonFilters Owner
    {
      get { return _Owner; }
      internal set
      {
        if (value != null)
        {
          if (_Owner != null)
            throw new InvalidOperationException("��������� ��������� �������� Owner �� �����������");

          if (String.IsNullOrEmpty(_Code))
            throw new NullReferenceException("�������� Code ������ ���� ����������� �� �������� Owner");
        }
        _Owner = value;
      }
    }
    private DBxCommonFilters _Owner;

    #endregion

    #region ������� Changed

    /// <summary>
    /// ������� ���������� ��� ��������� ������� � �������� �������������� �������� ��������.
    /// ������� �� ���������� �� ������������� ������� � ���������.
    /// �� ���������� ��� ��������� ������ �������� � ���������.
    /// </summary>
    public event EventHandler Changed;

    /// <summary>
    /// ���� ����� ���������� ��������-������������ ��� ��������� �������� �������.
    /// �������� ������� Changed ��� ����� ������� � ������� Changed ��������� DBxCommonFilters.
    /// �� ������������� ������� � ��������� ����� �� ��������� ������� ��������, � ��� �����, �� �������� ������� Changed.
    /// </summary>
    protected virtual void OnChanged()
    {
      if (_Owner == null)
        return;

      if (Changed != null)
        Changed(this, EventArgs.Empty);

      _Owner.OnChanged(this);
    }

    #endregion

    #region ������ ������ � ��������

    /// <summary>
    /// ��� �������, ������� ���������� � ����� ����� ������� �������.
    /// ���� �������� �� ����������� � ����� ����, ������������ Code.
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (!String.IsNullOrEmpty(_DisplayName))
          return _DisplayName;
        else if (!String.IsNullOrEmpty(Code))
          return Code;
        else
          return "[��� ����� � ����]";
      }
      set { _DisplayName = value; }
    }
    private string _DisplayName;


    /// <summary>
    /// ������������� ���� ������.
    /// �������������� ��� RefDocGridFilter.
    /// ������������������ ����� ���������� ������ ������.
    /// </summary>
    public virtual string DBIdentity { get { return String.Empty; } }

    /// <summary>
    /// ���������� true, ���� ������ ������������ ��� ������������ SQL-�������.
    /// �� ��������� ���������� true. ��� ������� ���������� ��������� ������ ���������� true.
    /// �������� ����� ���� �������� � false ��� ������� ������, ���� ���������� ����������� �� ��������, ������������ �������.
    /// � ���� ������ ��� ���� ��������� ���������
    /// </summary>
    public bool UseSqlFilter
    {
      get { return _UseSqlFilter; }
      set
      {
        if (_Owner != null)
          throw new InvalidOperationException("��������� �������� UseSqlFilter ����������� ������ �� ������������� ������� � ���������");
        _UseSqlFilter = value;
      }
    }
    private bool _UseSqlFilter;

    /// <summary>
    /// ������������ ���������������� ������
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    /// <summary>
    /// ���������� �������� DisplayName.
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return DisplayName;
    }

    #endregion

    #region ������� �������

    /// <summary>
    /// ������� �������
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// ���������� true, ���� ������ �� ����������
    /// </summary>
    public abstract bool IsEmpty { get; }

    #endregion

    #region �������, SQL-������ � ������������ ��������

    /// <summary>
    /// �������� ������ ���� �����, ������� ���������� ��� ���������� �������.
    /// ���� ����������� � ������ ���������� �� ����, ������� ������ ������ ��� ���.
    /// </summary>
    /// <param name="list">������ ��� ���������� �����</param>
    public abstract void GetColumnNames(DBxColumnList list);

    /// <summary>
    /// ��������� SQL-������� ��� ���������� ����� ������� ������
    /// </summary>
    public abstract DBxFilter GetSqlFilter();

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// ����� ����� � �������� ������ ��������� ����������� ����, ����� ����� ������������� ������.
    /// �������� ����������� ����� OnTestValues(), ���� ������ ���������� � �������� UseSqlFilter ����� true
    /// </summary>
    /// <param name="rowValues">��������c ������� � ��������� �����.</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    public bool TestValues(INamedValuesAccess rowValues)
    {
      if (!UseSqlFilter)
        return true;
      if (IsEmpty)
        return true;
      return OnTestValues(rowValues);
    }

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// ����� ����� � �������� ������ ��������� ����������� ����, ����� ����� ������������� ������.
    /// ����� �� ����������, ���� ������ �� ����������, �������������, �������� IsEmpty �� �����.
    /// ����� ����� �� ���������� ��� UseSqlFilter=false
    /// </summary>
    /// <param name="rowValues">��������c ������� � ��������� �����</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected abstract bool OnTestValues(INamedValuesAccess rowValues);

    #endregion

    #region ������ � ������ ������ ������������

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public abstract void ReadConfig(CfgPart cfg);

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public abstract void WriteConfig(CfgPart cfg);

    #endregion

    #region ������������� ������ ���������

    /// <summary>
    /// ���������� ��� �������� ������ ��������� �� ���������.
    /// ��������� ������� ����� ���������� ���������� ��������� �������� ��� 
    /// ����� ��������� � ������������ �� ����� ����������.
    /// </summary>
    /// <param name="newDoc">��������� ��������, � ������� ����� ���������� ����</param>
    public void InitNewDocValues(DBxSingleDoc newDoc)
    {
      if (IsEmpty)
        return;
      if (!UseSqlFilter)
        return;
      OnInitNewDocValues(newDoc);
    }

    /// <summary>
    /// ���������� ��� �������� ������ ��������� �� ���������.
    /// ��������� ������� ����� ���������� ���������� ��������� �������� ��� 
    /// ����� ��������� � ������������ �� ����� ����������.
    /// </summary>
    /// <param name="newDoc">��������� ��������, � ������� ����� ���������� ����</param>
    protected virtual void OnInitNewDocValues(DBxSingleDoc newDoc)
    {
    }

    /// <summary>
    /// ���� ������ ��������� ��������� �������� ������� "�� ������", �� 
    /// ���������������� ����� ������ ������� �������� "�����" ����� �� ������ �
    /// ������� true, ���� ��������� ��������. ���� ��������� ����������� �������
    /// SetAsCurrRow().
    /// ������������������ ����� ���������� false.
    /// </summary>
    /// <param name="row">������, ������ ������� ��������</param>
    /// <returns>������� ���������</returns>
    public virtual bool CanAsCurrRow(DataRow row)
    {
      return false;
    }

    /// <summary>
    /// ��������� �������� ������� "�� ������".
    /// ������������������ ����� ���������� ����������.
    /// </summary>
    /// <param name="row">������, ������ ������� ��������</param>
    public virtual void SetAsCurrRow(DataRow row)
    {
      throw new InvalidOperationException("������ \"" + ToString() + "\" �� ������������ ��������� �������� �� ������� ������");
    }

    #endregion

    #region IInitNewDocValues Members

    /// <summary>
    /// �� �����������
    /// </summary>
    /// <param name="savingDoc"></param>
    /// <param name="errorMessages"></param>
    public void ValidateDocValues(DBxSingleDoc savingDoc, ErrorMessageList errorMessages)
    {
      // TODO:
#if XXX
      if (IsEmpty)
        return;
      DBxColumnList ColumnNames = new DBxColumnList();
      GetColumnNames(ColumnNames);

      object[] Values = new object[ColumnNames.Count];
      for (int i = 0; i < ColumnNames.Count; i++)
      {
        int p = SavingDoc.Values.IndexOf(ColumnNames[i]);
        if (p < 0)
          return;
        Values[i] = SavingDoc.Values[p].Value;
      }

      if (!TestValues(ColumnNames.AsArray, Values))
      {
        string[] StrValues = GetColumnStrValues(Values);
        StringBuilder sb = new StringBuilder();
        if (ColumnNames.Count == 1)
        {
          // ���� ����
          sb.Append("�������� ���� \"");
          sb.Append(ColumnNames[0]);
          sb.Append("\" ");
          if (StrValues != null)
          {
            sb.Append("(");
            sb.Append(StrValues[0]);
            sb.Append(") ");
          }
          sb.Append("�� �������������");
        }
        else
        {
          sb.Append("�������� ����� ");
          for (int i = 0; i < ColumnNames.Count; i++)
          {
            sb.Append("\"");
            sb.Append(ColumnNames[i]);
            sb.Append("\" ");
            if (StrValues != null)
            {
              sb.Append(" (");
              sb.Append(StrValues[i]);
              sb.Append(") ");
            }
            sb.Append("�� �������������");
          }
        }
        sb.Append(" �������������� ������� \"");
        sb.Append(DisplayName);
        sb.Append("\" (");
        sb.Append(FilterText);
        sb.Append(")");
        ErrorMessages.AddWarning(sb.ToString());
      }
#endif
    }

    /// <summary>
    /// ���������������� ����� ����� ������� ��������� ������������� �����������
    /// �������� ��� ����������� � ���������
    /// ������������ ����� ���������� null, ��� �������� ����� �� ������ ��������
    /// </summary>
    /// <param name="columnValues">�������� �����</param>
    /// <returns>��������� ������������� ��������</returns>
    protected virtual string[] GetColumnStrValues(object[] columnValues)
    {
      return null;
    }

    #endregion
  }

  /// <summary>
  /// ����� �� ���������� ��������, ������� ����� ������ �������� � ��������� DBxCommonFiltes
  /// </summary>
  public class DBxCommonFilterSet : NamedList<DBxCommonFilter>
  {
    #region ��������� - ��������

    /// <summary>
    /// ��������� - ��������.
    /// �������� ���������� null �� ������������� ������� � ���������
    /// </summary>
    internal DBxCommonFilters Owner
    {
      get { return _Owner; }
      set
      {
        if (value != null)
        {
          if (_Owner != null)
            throw new InvalidOperationException("��������� ��������� �������� Owner �� �����������");
        }
        _Owner = value;
        SetReadOnly();
      }
    }
    private DBxCommonFilters _Owner;

    #endregion

    #region ������ � ��������

    /// <summary>
    /// ��������� ������ ���� �����, ������� ���������� ��� ���������� ����������.
    /// ���������� ������� �� �����������.
    /// ������� �� ���������� ��������� UseSqlFilter ������������.
    /// ������ ���� � ������ ������ ���� ���.
    /// </summary>
    /// <param name="list">������ ��� ���������� �����</param>
    public void GetColumnNames(DBxColumnList list)
    {
      if (list == null)
        throw new ArgumentNullException("list");

      for (int i = 0; i < Count; i++)
      {
        if (this[i].UseSqlFilter && (!this[i].IsEmpty))
          this[i].GetColumnNames(list);
      }
    }
    /// <summary>
    /// ������������ �������.
    /// ������������ ������� � ������, ������� ����� DBxCommonFilter.TestrValues() ���� ���� �� �������� �� ������ false.
    /// ���������� ������� �� �����������.
    /// ������� �� ���������� ��������� UseSqlFilter ������������.
    /// </summary>
    /// <param name="rowValues">������ � ��������� �����. � ������ ������ ���� ��� ����, ���������� ������� GetColumnNames</param>
    /// <param name="badFilter">���� ���������� ������ �� ������, ������� ������ false. ���� ������ �������� ������� ���� ��������, ���� ������������ null.</param>
    /// <returns>True, ���� ������ �������� ��� �������</returns>
    public bool TestValues(INamedValuesAccess rowValues, out DBxCommonFilter badFilter)
    {
      if (rowValues == null)
        throw new ArgumentNullException("rowValues");

      badFilter = null;
      for (int i = 0; i < Count; i++)
      {
        if (this[i].IsEmpty)
          continue;
        if (!this[i].UseSqlFilter)
          continue;

        if (!this[i].TestValues(rowValues))
        {
          badFilter = this[i];
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// �������� ��������� � ������.
    /// ���� �������������� �������� �������� ��� ���������� �����, ����������� ������ DBxColumnValueArray � ���������� ������, ����������� INamedValuesAccess.
    /// </summary>
    /// <param name="columnNames">����� �����</param>
    /// <param name="values">�������� �����</param>
    /// <param name="badFilter">������, ������� ������� ������</param>
    /// <returns>True, ���� ��� �������� ������� ���������� ��������</returns>
    public bool TestValues(DBxColumns columnNames, object[] values, out DBxCommonFilter badFilter)
    {
      DBxColumnValueArray cva = new DBxColumnValueArray(columnNames, values);
      return TestValues(cva, out badFilter);
    }

    ///// <summary>
    ///// �������� ��������� � ������
    ///// </summary>
    ///// <param name="Columns">����� �����</param>
    ///// <param name="Row">������, ���������� �������� ���� �����</param>
    ///// <param name="BadFilter">������, ������� ������� ������</param>
    ///// <returns>True, ���� ��� �������� ������� ���������� ��������</returns>
    //public bool TestValues(DBxColumns Columns, DataRow Row, out GridFilter BadFilter)
    //{
    //  object[] Values = Columns.GetRowValues(Row);
    //  return TestValues(Columns.AsArray, Values, out BadFilter);
    //}

    /// <summary>
    /// ��������� SQL-������� ��� ���������� ������ ������.
    /// ������ ������� � ������� �� ���������� ��������� UseSqlFilter ������������.
    /// </summary>
    /// <returns>������ DatatFilter, ��������������� �������� ��������. ���� �������� 
    /// ���������, �� ����� ���������� AndDBxFilter</returns>
    public DBxFilter GetSqlFilter()
    {
      List<DBxFilter> Filters = new List<DBxFilter>();
      for (int i = 0; i < Count; i++)
      {
        if (this[i].UseSqlFilter && (!this[i].IsEmpty))
          Filters.Add(this[i].GetSqlFilter());
      }
      return AndFilter.FromArray(Filters.ToArray());
    }


    /// <summary>
    /// ������� ���� ��������. �������� DBxCommonFilters.Clear() ��� ������� �������.
    /// ������ �������� �� ��������. ����� �������� ���������� �� �������� IsReadOnly.
    /// �� ������ ���� ����� � Clear(), ������� ������� ��� ������ ��������.
    /// </summary>
    public void ClearAllFilters()
    {
      for (int i = 0; i < Count; i++)
        this[i].Clear();
    }

    /// <summary>
    /// ���������� true, ���� �� ���� �� �������� � ������ �� ����������
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (!this[i].IsEmpty)
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// ���������� true, ���� ��� �� ������ ��������� �������, ��� �������� UseSqlFilter=true
    /// </summary>
    public bool IsSqlEmpty
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (this[i].UseSqlFilter)
          {
            if (!this[i].IsEmpty)
              return false;
          }
        }
        return true;
      }
    }

    /// <summary>
    /// ���������� true, ���� ��� �� ������ ��������� �������, ��� �������� UseSqlFilter=false
    /// </summary>
    public bool IsNonSqlEmpty
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (!this[i].UseSqlFilter)
          {
            if (!this[i].IsEmpty)
              return false;
          }
        }
        return true;
      }
    }

    #endregion
  }

  /// <summary>
  /// ��������� ��������
  /// </summary>
  public class DBxCommonFilters : NamedListWithNotifications<DBxCommonFilter>
  {
    #region �����������

    /// <summary>
    /// ������� ������ ���������
    /// </summary>
    public DBxCommonFilters()
    {
    }

    #endregion

    #region ���������� / �������� ��������

    /// <summary>
    /// ���������, ��� � ������������ ������� ���� ��� � �� ��� �� ��� ����������� � ������.
    /// </summary>
    /// <param name="item">����������� ������</param>
    protected override void OnBeforeAdd(DBxCommonFilter item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (item.Owner != null)
        throw new InvalidOperationException("��������� ���������� �������� �� �����������");
      if (String.IsNullOrEmpty(item.Code))
        throw new NullReferenceException("�������� DBxCommonFilter.Code ������ ���� ����������� ����� ����������� ��������");

      base.OnBeforeAdd(item);
    }

    /// <summary>
    /// ������������� �������� DBxCommonFilter.Owner
    /// </summary>
    /// <param name="item">����������� ������</param>
    protected override void OnAfterAdd(DBxCommonFilter item)
    {
      base.OnAfterAdd(item);
      item.Owner = this;
    }

    /// <summary>
    /// ������� �������� DBxCommonFilter.Owner
    /// </summary>
    /// <param name="item">��������� ������</param>
    protected override void OnAfterRemove(DBxCommonFilter item)
    {
      base.OnAfterRemove(item);
      item.Owner = null;
    }

    #endregion

    #region ���������� ������ ��������

    /// <summary>
    /// ���������� ������ ��������
    /// </summary>
    /// <param name="filterSet">����� ��������</param>
    public void Add(DBxCommonFilterSet filterSet)
    {
      filterSet.Owner = this;
      AddRange(filterSet);
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// ��������� ������ � ����� "������ ������".
    /// ��� ���� �������� ����� �������� ����� �������������.
    /// </summary>
    public new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    #endregion

    #region ��������� �� ��������� �������

    /// <summary>
    /// ����������, ����� ���������� ������� ��������� � ����� �� ��������.
    /// ������� ������� DBxCommonFilter.Changed ���������� �� ����� �������.
    /// ���� ���������� ������� �������� ��������� ������-���� �������, �� �������
    /// ���������� ��������� �� �����������
    /// </summary>
    public event EventHandler Changed;

    private bool InsideChanged = false;

    /// <summary>
    /// ���������� ����� ��� ������ ������� Changed
    /// </summary>
    /// <param name="filter">������, ������� ������ �������, ��� null</param>
    internal protected virtual void OnChanged(DBxCommonFilter filter)
    {
      int p = IndexOf(filter);
      if (p < 0)
        //throw new ArgumentException("������ �� ��������� � ���������");
        return; // ��������� ��� ������� ����������

      if (InsideChanged)
        return;

      InsideChanged = true;
      try
      {
        if (!base.IsUpdating)
        {
          if (Changed != null)
          {
            //try
            //{
            Changed(this, EventArgs.Empty);
            //}
            //catch (Exception e)
            //{
            //  EFPApp.ShowException(e, "������ ��������� ������� GridFilters.Changed");
            //}
          }
        }

        base.NotifyItemChanged(p);
      }
      finally
      {
        InsideChanged = false;
      }
    }

    #endregion

    #region ������ ������

    /// <summary>
    /// ����� ������� �� ����������������� ����� ������� (�������� GridFilter.DisplayName)
    /// </summary>
    /// <param name="displayName">��� ������� ��� ������</param>
    /// <returns>��������� ������ ��� null, ���� ������ �� ������</returns>
    public DBxCommonFilter FindByDisplayName(string displayName)
    {
      if (String.IsNullOrEmpty(displayName))
        return null;
      for (int i = 0; i < Count; i++)
      {
        if (this[i].DisplayName == displayName)
          return this[i];
      }
      return null;
    }

    /// <summary>
    /// ��������� ������ ���� �����, ������� ���������� ��� ���������� ����������.
    /// ���������� ������� �� �����������.
    /// ������� �� ���������� ��������� UseSqlFilter ������������.
    /// ������ ���� � ������ ������ ���� ���.
    /// </summary>
    /// <param name="list">������ ��� ���������� �����</param>
    public void GetColumnNames(DBxColumnList list)
    {
      if (list == null)
        throw new ArgumentNullException("list");

      for (int i = 0; i < Count; i++)
      {
        if (this[i].UseSqlFilter && (!this[i].IsEmpty))
          this[i].GetColumnNames(list);
      }
    }

    /// <summary>
    /// ��������� ������ ���� �����, ������� ���������� ��� ���������� ����������.
    /// ���������� ������� �� �����������.
    /// ������� �� ���������� ��������� UseSqlFilter ������������.
    /// ���� ��� ������������� ��������, ������������ null.
    /// </summary>
    public DBxColumns GetColumnNames()
    {
      DBxColumnList list = new DBxColumnList();
      GetColumnNames(list);
      if (list.Count > 0)
        return new DBxColumns(list);
      else
        return null;
    }

    /// <summary>
    /// ������������ �������.
    /// ������������ ������� � ������, ������� ����� DBxCommonFilter.TestrValues() ���� ���� �� �������� �� ������ false.
    /// ���������� ������� �� �����������.
    /// ������� �� ���������� ��������� UseSqlFilter ������������.
    /// </summary>
    /// <param name="rowValues">������ � ��������� �����. � ������ ������ ���� ��� ����, ���������� ������� GetColumnNames</param>
    /// <param name="badFilter">���� ���������� ������ �� ������, ������� ������ false. ���� ������ �������� ������� ���� ��������, ���� ������������ null.</param>
    /// <returns>True, ���� ������ �������� ��� �������</returns>
    public bool TestValues(INamedValuesAccess rowValues, out DBxCommonFilter badFilter)
    {
      badFilter = null;
      for (int i = 0; i < Count; i++)
      {
        if (this[i].IsEmpty)
          continue;
        if (!this[i].UseSqlFilter)
          continue;

        if (!this[i].TestValues(rowValues))
        {
          badFilter = this[i];
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// ������������ �������.
    /// ������������ ������� � ������, ������� ����� DBxCommonFilter.TestrValues() ���� ���� �� �������� �� ������ false.
    /// ���������� ������� �� �����������.
    /// ������� �� ���������� ��������� UseSqlFilter ������������.
    /// </summary>
    /// <param name="rowValues">������ � ��������� �����. � ������ ������ ���� ��� ����, ���������� ������� GetColumnNames</param>
    /// <returns>True, ���� ������ �������� ��� �������</returns>
    public bool TestValues(INamedValuesAccess rowValues)
    {
      DBxCommonFilter badFilter;
      return TestValues(rowValues, out badFilter);
    }

    /// <summary>
    /// �������� ��������� � ������.
    /// ���� �������������� �������� �������� ��� ���������� �����, ����������� ������ DBxColumnValueArray � ���������� ������, ����������� INamedValuesAccess.
    /// </summary>
    /// <param name="columns">����� �����</param>
    /// <param name="values">�������� �����</param>
    /// <param name="badFilter">������, ������� ������� ������</param>
    /// <returns>True, ���� ��� �������� ������� ���������� ��������</returns>
    public bool TestValues(DBxColumns columns, object[] values, out DBxCommonFilter badFilter)
    {
      DBxColumnValueArray cva = new DBxColumnValueArray(columns, values);
      return TestValues(cva, out badFilter);
    }


    /// <summary>
    /// �������� ��������� � ������.
    /// ���� �������������� �������� �������� ��� ���������� �����, ����������� ������ DBxColumnValueArray � ���������� ������, ����������� INamedValuesAccess.
    /// </summary>
    /// <param name="columns">����� �����</param>
    /// <param name="values">�������� �����</param>
    /// <returns>True, ���� ��� �������� ������� ���������� ��������</returns>
    public bool TestValues(DBxColumns columns, object[] values)
    {
      DBxCommonFilter badFilter;
      return TestValues(columns, values, out badFilter);
    }

    ///// <summary>
    ///// �������� ��������� � ������
    ///// </summary>
    ///// <param name="Columns">����� �����</param>
    ///// <param name="Row">������, ���������� �������� ���� �����</param>
    ///// <param name="BadFilter">������, ������� ������� ������</param>
    ///// <returns>True, ���� ��� �������� ������� ���������� ��������</returns>
    //public bool TestValues(DBxColumns Columns, DataRow Row, out GridFilter BadFilter)
    //{
    //  object[] Values = Columns.GetRowValues(Row);
    //  return TestValues(Columns.AsArray, Values, out BadFilter);
    //}

    /// <summary>
    /// ��������� SQL-������� ��� ���������� ������ ������.
    /// ������ ������� � ������� �� ���������� ��������� UseSqlFilter ������������.
    /// </summary>
    /// <returns>������ DatatFilter, ��������������� �������� ��������. ���� �������� 
    /// ���������, �� ����� ���������� AndDBxFilter</returns>
    public DBxFilter GetSqlFilter()
    {
      List<DBxFilter> Filters = new List<DBxFilter>();
      for (int i = 0; i < Count; i++)
      {
        if (this[i].UseSqlFilter && (!this[i].IsEmpty))
          Filters.Add(this[i].GetSqlFilter());
      }
      return AndFilter.FromArray(Filters.ToArray());
    }


    /// <summary>
    /// ������� ���� ��������. �������� DBxCommonFilters.Clear() ��� ������� �������.
    /// ������ �������� �� ��������. ����� �������� ���������� �� �������� IsReadOnly.
    /// �� ������ ���� ����� � Clear(), ������� ������� ��� ������ ��������.
    /// </summary>
    public void ClearAllFilters()
    {
      for (int i = 0; i < Count; i++)
        this[i].Clear();
    }


    /// <summary>
    /// ������������� ���� ������.
    /// ������������ �������� GridFilter.DBIdentity ��� ������� ������� � ������,
    /// ���������� �������� ��������.
    /// ���� �� ���� �� �������� �� ������ ��������, ������������ ������ ������.
    /// ��� ��������, ��� � ������ ��� ��������� �������� � ������� ����� ����������/���������
    /// ����� ����� ������ ����� ������ �����������
    /// </summary>
    public string DBIdentity
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          string s = this[i].DBIdentity;
          if (!String.IsNullOrEmpty(s))
            return s;
        }

        return String.Empty;
      }
    }


    /// <summary>
    /// ��� �������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      string s = "Count=" + Count.ToString();
      if (IsReadOnly)
        s += " (ReadOnly)";
      return s;
    }

    /// <summary>
    /// ���������� true, ���� ��� �� ������ ��������� �������
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (!this[i].IsEmpty)
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// ���������� true, ���� ��� �� ������ ��������� �������, ��� �������� UseSqlFilter=true
    /// </summary>
    public bool IsSqlEmpty
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (this[i].UseSqlFilter)
          {
            if (!this[i].IsEmpty)
              return false;
          }
        }
        return true;
      }
    }

    /// <summary>
    /// ���������� true, ���� ��� �� ������ ��������� �������, ��� �������� UseSqlFilter=false
    /// </summary>
    public bool IsNonSqlEmpty
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (!this[i].UseSqlFilter)
          {
            if (!this[i].IsEmpty)
              return false;
          }
        }
        return true;
      }
    }

    #endregion

    #region ������ / ������ ������ ������������

    /// <summary>
    /// ������������� ������� �������� �������� �� ��������� ����������� ����� ��������
    /// ��� ������� ������� ������������� ��������� �����, ������� ������
    /// ���������� ����� ������ �������� (������� ������).
    /// ���� ��� ������-���� ������� (��� ���� ��������) ��� �����, ������ ������������
    /// ��� �� ���������� ������. � ���� ������ ����������� �������� �� ���������
    /// </summary>
    /// <param name="config">������ ������������, ������ ����� ��������� ���������.
    /// ���� null, �� ������ �� ����� ��������� � ������� ��������� ��������� ��� ���������</param>
    public void ReadConfig(CfgPart config)
    {
      if (config == null)
        return;

      for (int i = 0; i < Count; i++)
      {
        //try
        //{
        //  CfgPart Part2 = Config.GetChild(this[i].Code, false);
        //  if (Part2 != null)
        //    this[i].ReadConfig(Part2);
        //}
        //catch (Exception e)
        //{
        //  EFPApp.ShowException(e, "������ �������� ��������� ������� \"" + this[i].DisplayName + "\"");
        //  this[i].Clear();
        //}

        CfgPart Part2 = config.GetChild(this[i].Code, false);
        if (Part2 != null)
        {
          try
          {
            this[i].ReadConfig(Part2);
          }
          catch (Exception e)
          {
            OnReadConfigError(e, this[i], Part2);
          }
        }
      }
    }

    /// <summary>
    /// ���������� ��� ������������� ������ ������ ������������ � DBxCommonFilter.ReadConfig().
    /// ������������������ ����� �������� ����������� ����������.
    /// </summary>
    /// <param name="exception">��������� ����������</param>
    /// <param name="filter">������, ��� �������� �������� ����������</param>
    /// <param name="cfg">����������� ������ ������������</param>
    protected virtual void OnReadConfigError(Exception exception, DBxCommonFilter filter, CfgPart cfg)
    {
      //Filter.Clear();
      throw exception;
    }

    /// <summary>
    /// ���������� ������� �������� �������� � ������ ������������.
    /// </summary>
    /// <param name="cfg">������ ������������ ��� ���������� ��������. ������ ������ ���� ������, ����� ����� ������������� ����������</param>
    public void WriteConfig(CfgPart cfg)
    {
      if (cfg == null)
        throw new ArgumentNullException("cfg", "������ ��� ������ ������������ �������� ������ ���� �����");

      cfg.Clear();

      for (int i = 0; i < Count; i++)
      {
        CfgPart cfg2 = cfg.GetChild(this[i].Code, true); // ����������� ������� ���� ��� ��������������� ��������
        cfg2.Clear(); // 11.09.2012
        if (!this[i].IsEmpty)
          this[i].WriteConfig(cfg2);
      }
    }

    /// <summary>
    /// ������� �������� �������� � ���� ������ ������.
    /// ������ �������� �������� WriteConfig(), � ������ - ReadConfig(). ��� ���� ������������ TempCfg � �������������� � XML-������.
    /// ��� �������� ������ ������������ � ������� ��� �������� ������ �� ������� � �������
    /// </summary>
    public string ConfigAsXmlText
    {
      get
      {
        TempCfg Cfg = new TempCfg();
        WriteConfig(Cfg);
        return Cfg.AsXmlText;
      }
      set
      {
        TempCfg Cfg = new TempCfg();
        Cfg.AsXmlText = value;
        ReadConfig(Cfg);
      }
    }

    #endregion

    #region IInitNewDocValues Members

    /// <summary>
    /// ���������� �� ClientDocType.PerformEditing
    /// </summary>
    /// <param name="newDoc"></param>
    public void InitNewDocValues(DBxSingleDoc newDoc)
    {
      for (int i = 0; i < Count; i++)
        this[i].InitNewDocValues(newDoc);
    }


    /// <summary>
    /// �������� GridFilter.ValidateDocValues ��� ���� �������� � ������
    /// </summary>
    /// <param name="savingDoc"></param>
    /// <param name="errorMessages"></param>
    public void ValidateDocValues(DBxSingleDoc savingDoc, ErrorMessageList errorMessages)
    {
      for (int i = 0; i < Count; i++)
        this[i].ValidateDocValues(savingDoc, errorMessages);
    }

    #endregion
  }

  #region ������� ��� ������ ����

  /// <summary>
  /// ������� ����� ��� �������� �� ������ ����.
  /// ���������� �������� ColumnName
  /// </summary>
  public abstract class SingleColumnCommonFilter : DBxCommonFilter
  {
    #region �����������

    /// <summary>
    /// ������� ������ �������.
    /// ������������� �������� DBxCommonFilter.Code � DisplayName ������� <paramref name="columnName"/>.
    /// </summary>
    /// <param name="columnName">��� ����. ������ ���� ������</param>
    public SingleColumnCommonFilter(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      _ColumnName = columnName;

      base.Code = columnName; // ����� ���� �������� ����� � ���������������� ����
      base.DisplayName = columnName; // ����� ���� �������� ����� � ���������������� ����
    }

    #endregion

    #region �������� � ������

    /// <summary>
    /// ��� ������������ ����.
    /// �������� � ������������ � �� ����� ���� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// �������� ������ ���� �����, ������� ���������� ��� ���������� �������.
    /// ���� ����������� � ������ ���������� �� ����, ������� ������ ������ ��� ���.
    /// ��������� � ������ ���� ColumnName.
    /// </summary>
    /// <param name="list">������ ��� ���������� �����</param>
    public override /*sealed */ void GetColumnNames(DBxColumnList list)
    {
      list.Add(ColumnName);
    }

    /// <summary>
    /// ���������� ��� �������� ������ ��������� �� ���������.
    /// ��������� ������� � ��������� ���� ColumnName � �������� ����� OnInitNewDocValue() ��� �������� ����
    /// </summary>
    /// <param name="newDoc">��������� ��������, � ������� ����� ���������� ����</param>
    protected override /*sealed*/ void OnInitNewDocValues(DBxSingleDoc newDoc)
    {
      int p = newDoc.Values.IndexOf(ColumnName);
      if (p < 0)
        return;

      OnInitNewDocValue(newDoc.Values[p]);
    }

    /// <summary>
    /// ������������� �������� ���� ��� �������� ������ ���������.
    /// ����� ����������, ������ ����� ������ ����������
    /// </summary>
    /// <param name="docValue">�������� ����, ������� ����� ����������</param>
    protected virtual void OnInitNewDocValue(DBxDocValue docValue)
    {
    }

    #endregion
  }

  #region ������� �� ���������� ����

  /// <summary>
  /// ������� ������ �� �������� ���������� ���� (�������� ���� �� ��������� 
  /// ������������� ��������)
  /// </summary>
  public class StringValueCommonFilter : SingleColumnCommonFilter
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public StringValueCommonFilter(string columnName)
      : base(columnName)
    {
      _Value = String.Empty;
    }

    #endregion

    #region ������� ��������

    /// <summary>
    /// ������� �������� �������. ������ ������ �������� ���������� �������
    /// </summary>
    public String Value
    {
      get { return _Value; }
      set
      {
        if (value == null)
          value = String.Empty;
        if (value == _Value)
          return;
        _Value = value;
        OnChanged();
      }
    }
    private string _Value;

    #endregion

    #region ���������������� ������ ��������

    /// <summary>
    /// ������� �������
    /// </summary>
    public override void Clear()
    {
      Value = String.Empty;
    }

    /// <summary>
    /// ���������� true, ���� ������ �� ����������
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return String.IsNullOrEmpty(Value);
      }
    }

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// ����� ����� � �������� ������ ��������� ����������� ���� (������� �����
    /// �������� ������� GetColumnNames()), ����� ����� ������������� ������.
    /// </summary>
    /// <param name="rowValues">��������c ������� � ��������� �����.</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      return DataTools.GetString(rowValues.GetValue(ColumnName)) == Value;
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new ValueFilter(ColumnName, Value);
    }

    /// <summary>
    /// ������������� �������� ���� ��� �������� ������ ���������.
    /// ����� ����������, ������ ����� ������ ����������
    /// </summary>
    /// <param name="docValue">�������� ����, ������� ����� ����������</param>
    protected override void OnInitNewDocValue(DBxDocValue docValue)
    {
      docValue.SetString(Value);
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void ReadConfig(CfgPart cfg)
    {
      Value = cfg.GetString("Value");
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetString("Value", Value);
    }

    #endregion

    #region �������� ��������

    /// <summary>
    /// �������� �������� ��� ������� ������.
    /// ���� ������ �� ���������� (IsEmpty=true), ������������ true
    /// </summary>
    /// <param name="rowValue">����������� ��������</param>
    /// <returns>true, ���� �������� �������� ������� �������</returns>
    public bool TestValue(string rowValue)
    {
      if (IsEmpty)
        return true;
      return rowValue == Value;
    }

    #endregion
  }

  /// <summary>
  /// ������ ��� StartsWithFilter
  /// </summary>
  public class StartsWithCommonFilter : StringValueCommonFilter
  {
    #region �����������

    /// <summary>
    /// ������� ������ �������
    /// </summary>
    /// <param name="columnName">��� ���������� ����</param>
    public StartsWithCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region ���������������� ������

    // ��� ���������� �� ������ �������������� �� � ������ StartsWuthGridFilter � ExtDbDocForms.dll

    /// <summary>
    /// ������� StartsWithFilter
    /// </summary>
    /// <returns></returns>
    public override DBxFilter GetSqlFilter()
    {
      return new StartsWithFilter(ColumnName, Value);
    }

    /// <summary>
    /// �������� ��������
    /// </summary>
    /// <param name="rowValues">������ � ��������� �����</param>
    /// <returns>True, ���� ������� ������� �����������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v = rowValues.GetValue(ColumnName);
      return DataTools.GetString(v).StartsWith(Value);
    }

    /// <summary>
    /// ����� ������ �� ������, � ������� �� �������� ������.
    /// </summary>
    /// <param name="docValue"></param>
    protected override void OnInitNewDocValue(DBxDocValue docValue)
    {
    }

    #endregion

    #region �������� ��������

    /// <summary>
    /// �������� �������� ��� ������� ������.
    /// ���� ������ �� ���������� (IsEmpty=true), ������������ true
    /// </summary>
    /// <param name="rowValue">����������� ��������</param>
    /// <returns>true, ���� �������� �������� ������� �������</returns>
    public new bool TestValue(string rowValue)
    {
      if (IsEmpty)
        return true;
      return rowValue.StartsWith(Value);
    }

    #endregion
  }

  #region ������������ CodesFilterMode

  /// <summary>
  /// ��������� �������� �������� CodeGridFilter.Mode
  /// </summary>
  public enum CodesFilterMode
  {
    /// <summary>
    /// ��� �������
    /// </summary>
    NoFilter,

    /// <summary>
    /// �������� ����
    /// </summary>
    Include,

    /// <summary>
    /// ��������� ����
    /// </summary>
    Exclude
  }

  #endregion

  /// <summary>
  /// ����� ��� ���������� �������� �� �����.
  /// ������������ ����� ��������� ��� ���������� ���������� �����. �������� 
  /// ��������� ������ �����. 
  /// �������� ����� ��� ������ RefBookGridFilter ��� ���������� �� ����� "���-��������"
  /// </summary>
  public class CodeCommonFilter : SingleColumnCommonFilter
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <param name="canBeEmpty">true, ���� �������������� ������ ����</param>
    public CodeCommonFilter(string columnName, bool canBeEmpty)
      : base(columnName)
    {
      _CanBeEmpty = canBeEmpty;
      _Mode = CodesFilterMode.NoFilter;
      _Codes = String.Empty;
      _EmptyCode = false;
    }

    #endregion

    #region ������� ��������� �������

    /// <summary>
    /// ������� ����� �������
    /// </summary>
    public CodesFilterMode Mode
    {
      get { return _Mode; }
      set
      {
        if (value == _Mode)
          return;
        _Mode = value;
        OnChanged();
      }
    }
    private CodesFilterMode _Mode;

    /// <summary>
    /// ������ ���������� ��� ����������� �����, ����������� ��������
    /// </summary>
    public string Codes
    {
      get { return _Codes; }
      set
      {
        if (value == null)
          value = String.Empty;
        if (value == _Codes)
          return;
        _Codes = value;
        OnChanged();
      }
    }
    private string _Codes;

    /// <summary>
    /// �������� �� � ������ ������ ��� ����.
    /// �������� ������������� ������ ��� CanBeEmpty=true
    /// </summary>
    public bool EmptyCode
    {
      get { return _EmptyCode; }
      set
      {
        if (value == _EmptyCode)
          return;
        _EmptyCode = value;
        OnChanged();
      }
    }
    private bool _EmptyCode;

    #endregion

    #region ������ ��������

    /// <summary>
    /// true, ���� �������������� ������ ����. ���� false, �� ��������������, ��� 
    /// ���� ������ ����� ������������� ��������, � ������ "��� �� ����������" ����������
    /// �������� ��������������� � ������������
    /// </summary>
    public bool CanBeEmpty { get { return _CanBeEmpty; } }
    private bool _CanBeEmpty;

    #endregion

    #region ���������������� ������ � ��������

    /// <summary>
    /// ������������ ������ �����. ������� ������� ������ �������
    /// </summary>
    /// <param name="s">������, ��������� �������������</param>
    /// <returns>������������ ������</returns>
    protected static string NormCodes(string s)
    {
      if (String.IsNullOrEmpty(s))
        return String.Empty;
      string[] a = s.Split(',');
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < a.Length; i++)
      {
        string s1 = a[i].Trim();
        if (String.IsNullOrEmpty(s1))
          continue;
        if (sb.Length > 0)
          sb.Append(',');
        sb.Append(s1);
      }
      return sb.ToString();
    }

    /// <summary>
    /// ������� �������
    /// </summary>
    public override void Clear()
    {
      Mode = CodesFilterMode.NoFilter;
    }

    /// <summary>
    /// ���������� true, ���� ������ �� ����������
    /// </summary>
    public override bool IsEmpty { get { return Mode == CodesFilterMode.NoFilter; } }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      List<DBxFilter> Filters = new List<DBxFilter>();
      if (!String.IsNullOrEmpty(Codes))
      {
        string[] a = Codes.Split(',');
        Filters.Add(new ValuesFilter(Code, a));
      }
      if (EmptyCode)
        Filters.Add(new ValueFilter(Code, String.Empty, typeof(string)));

      DBxFilter Filter = OrFilter.FromArray(Filters.ToArray());
      if (Mode == CodesFilterMode.Exclude)
        Filter = new NotFilter(Filter);
      return Filter;
    }

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// </summary>
    /// <param name="rowValues">�������� �����</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      string RowValue = DataTools.GetString(rowValues.GetValue(ColumnName));
      return TestValue(RowValue);
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void ReadConfig(CfgPart cfg)
    {
      switch (cfg.GetString("Mode"))
      {
        case "Include":
          Mode = CodesFilterMode.Include;
          break;
        case "Exclude":
          Mode = CodesFilterMode.Exclude;
          break;
        default:
          Mode = CodesFilterMode.NoFilter;
          break;
      }
      Codes = cfg.GetString("Codes");
      if (CanBeEmpty)
        EmptyCode = cfg.GetBool("EmptyCode");
      else
        EmptyCode = false;
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void WriteConfig(CfgPart cfg)
    {
      switch (Mode)
      {
        case CodesFilterMode.Include:
          cfg.SetString("Mode", "Include");
          break;
        case CodesFilterMode.Exclude:
          cfg.SetString("Mode", "Exclude");
          break;
        default:
          cfg.Remove("Mode");
          break;
      }

      if (String.IsNullOrEmpty(Codes))
        cfg.Remove("Codes");
      else
        cfg.SetString("Codes", Codes);

      if (CanBeEmpty)
        cfg.SetBool("EmptyCode", EmptyCode);
    }

    #endregion

    #region �������� ��������

    /// <summary>
    /// �������� �������� ��� ������� ������.
    /// ���� ������ �� ���������� (IsEmpty=true), ������������ true
    /// </summary>
    /// <param name="rowValue">����������� ��������</param>
    /// <returns>true, ���� �������� �������� ������� �������</returns>
    public bool TestValue(string rowValue)
    {
      if (IsEmpty)
        return true;

      bool Flag;
      if (String.IsNullOrEmpty(rowValue))
        Flag = EmptyCode;
      else
      {
        if (String.IsNullOrEmpty(Codes))
          Flag = false;
        else
          Flag = Codes.Contains(rowValue);
      }
      if (Mode == CodesFilterMode.Exclude)
        return !Flag;
      else
        return Flag;
    }

    #endregion

  }

  #endregion

  #region ������� ValueFilter

  /// <summary>
  /// ������� ����� ��� ���������� �������� �� �������� ����
  /// ���������� ������� ��������������� � ������� ������� Nullable
  /// </summary>
  /// <typeparam name="T">��� �������� ����</typeparam>
  public abstract class ValueCommonFilterBase<T> : SingleColumnCommonFilter
    where T : struct
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="columnName">��� �������</param>
    public ValueCommonFilterBase(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� �������� �������
    /// </summary>
    public Nullable<T> Value
    {
      get { return _Value; }
      set
      {
        if (DataTools.AreValuesEqual(value, _Value))
          return;
        _Value = value;
        OnChanged();
      }
    }
    private Nullable<T> _Value;

    #endregion

    #region ���������������� ��������

    /// <summary>
    /// ������� �������
    /// </summary>
    public override void Clear()
    {
      Value = null;
    }

    /// <summary>
    /// ���������� true, ���� ������ �� ����������
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return !Value.HasValue;
      }
    }

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// ����� ����� � �������� ������ ��������� ����������� ���� (������� �����
    /// �������� ������� GetColumnNames()), ����� ����� ������������� ������.
    /// </summary>
    /// <param name="rowValues">��������c ������� � ��������� �����.</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v = rowValues.GetValue(ColumnName);

      if (Value.Value.Equals(v))
        return true;

      // 11.08.2014
      // ���� �������� Value ����� �������� "�� ���������" (0, false), �� � ������
      // ������ �������� ������ �� ��������� ���� NULL
      if (Value.Value.Equals(default(T)))
      {
        if (v == null)
          return true;

        if (v is DBNull)
          return true;
      }

      return false;
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new ValueFilter(ColumnName, Value.Value, typeof(T));
    }

    /// <summary>
    /// ���������� ��� �������� ������ ��������� �� ���������.
    /// ������������� ��������� �������� ���� ColumnName, ���� � ������� ������� ������������ ��������.
    /// </summary>
    /// <param name="docValue">�������� ����, ������� ����� ����������</param>
    protected override void OnInitNewDocValue(DBxDocValue docValue)
    {
      docValue.SetValue(Value.Value);
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void ReadConfig(CfgPart cfg)
    {
      if (!String.IsNullOrEmpty(cfg.GetString("Value")))
        Value = DoReadConfigValue(cfg);
      else
        Value = null;
    }

    /// <summary>
    /// ����������� �����, ������� ������ ��������� �������� "Value" �� ������ ������������.
    /// ��������, � ������� ������ return Part.GetInt("Value").
    /// </summary>
    /// <param name="cfg">������ ������������ ��� ������ �������</param>
    /// <returns>����������� ��������</returns>
    protected abstract T DoReadConfigValue(CfgPart cfg);

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void WriteConfig(CfgPart cfg)
    {
      if (Value.HasValue)
        DoWriteConfigValue(cfg, Value.Value);
      else
        cfg.Remove("Value");
    }

    /// <summary>
    /// ����������� �����, ������� ������ ���������� �������� "Value" � ������ ������������.
    /// ��������, � ������� ������ Part.SetInt("Value", Value).
    /// </summary>
    /// <param name="cfg">������ ������������ ��� ������ �������</param>
    /// <param name="value">������������ ��������</param>
    protected abstract void DoWriteConfigValue(CfgPart cfg, T value);

    #endregion

    #region �������� ��������

    /// <summary>
    /// �������� �������� ��� ������� ������.
    /// ���� ������ �� ���������� (IsEmpty=true), ������������ true
    /// </summary>
    /// <param name="rowValue">����������� ��������</param>
    /// <returns>true, ���� �������� �������� ������� �������</returns>
    public bool TestValue(T rowValue)
    {
      if (IsEmpty)
        return true;

      if (Value.Value.Equals(rowValue))
        return true;

      return false;
    }

    #endregion
  }

  /// <summary>
  /// ������� ������ �� ����������� ����
  /// </summary>
  public class BoolValueCommonFilter : ValueCommonFilterBase<bool>
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public BoolValueCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// �������� CfgPart.GetBool()
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    /// <returns>��������</returns>
    protected override bool DoReadConfigValue(CfgPart cfg)
    {
      return cfg.GetBool("Value");
    }

    /// <summary>
    /// �������� CfgPart.SetBool()
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    /// <param name="value">��������</param>
    protected override void DoWriteConfigValue(CfgPart cfg, bool value)
    {
      cfg.SetBool("Value", value);
    }

    ///// <summary>
    ///// ���������� FilterTextTrue ��� FilterTextFales, ���� ������ ����������
    ///// </summary>
    ///// <param name="ColumnValues">�������� �����</param>
    ///// <returns>��������� ������������� ��������</returns>
    //protected override string[] GetColumnStrValues(object[] ColumnValues)
    //{
    //  bool Value = DataTools.GetBool(ColumnValues[0]);
    //  return new string[] { Value ? FilterTextTrue : FilterTextFalse };
    //}

    #endregion
  }

  /// <summary>
  /// ������� ������ �� ���� ���� Integer � ����������� �� ������������� ��������
  /// ���� ���� ����� ��������� ������������� ����� ��������, �� ������� ������������
  /// ������ EnumGridFilter
  /// </summary>
  public class IntValueCommonFilter : ValueCommonFilterBase<int>
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public IntValueCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region ���������������� ��������

    /// <summary>
    /// �������� CfgPart.GetInt()
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    /// <returns>��������</returns>
    protected override int DoReadConfigValue(CfgPart cfg)
    {
      return cfg.GetInt("Value");
    }

    /// <summary>
    /// �������� CfgPart.SetBool()
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    /// <param name="value">��������</param>
    protected override void DoWriteConfigValue(CfgPart cfg, int value)
    {
      cfg.SetInt("Value", value);
    }

    /// <summary>
    /// ���������� ��������� ������������� ��� �����
    /// </summary>
    /// <param name="columnValues">�������� �����</param>
    /// <returns>��������� ������������� ��������</returns>
    protected override string[] GetColumnStrValues(object[] columnValues)
    {
      int Value = DataTools.GetInt(columnValues[0]);
      return new string[] { Value.ToString() };
    }

    #endregion
  }


  /// <summary>
  /// ������ �� ���� ��� ��������� ���� ��� ���� ���� "����"
  /// </summary>
  public class YearCommonFilter : IntValueCommonFilter
  {
    #region ������������

    /// <summary>
    /// ����������� ��� ��������� ����
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public YearCommonFilter(string columnName)
      : this(columnName, false)
    {
    }

    /// <summary>
    /// ����������� ��� ���� ���� "����" ��� ��������� ����
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <param name="isDateColumn">���� true, �� ���� ����� ��� "����".
    /// ���� false, �� ���� �������� ��������</param>
    public YearCommonFilter(string columnName, bool isDateColumn)
      : base(columnName)
    {
      _IsDateColumn = isDateColumn;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���� true, �� ���� ����� ��� "����".
    /// ���� false, �� ���� �������� ��������.
    /// </summary>
    public bool IsDateColumn { get { return _IsDateColumn; } }
    private bool _IsDateColumn;

    #endregion

    #region ���������������� ������ � ��������

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      if (!Value.HasValue)
        return null;
      if (IsDateColumn)
        return new DateRangeFilter(ColumnName, Value.Value);
      else
        return new ValueFilter(ColumnName, Value.Value);
    }

    #endregion
  }

  #endregion

  #region ������� �� ���������� ��������

  /// <summary>
  /// ������ �� ��������� ��� ��� ������ ����
  /// </summary>
  public class DateRangeCommonFilter : SingleColumnCommonFilter
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="columnName">��� ������� ���� "����"</param>
    public DateRangeCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� �������� ������� - ��������� ���� ��� null
    /// </summary>
    public DateTime? FirstDate
    {
      get { return _FirstDate; }
      set
      {
        if (value == _FirstDate)
          return;
        _FirstDate = value;
        OnChanged();
      }
    }
    private DateTime? _FirstDate;

    /// <summary>
    /// ������� �������� ������� - �������� ���� ��� null
    /// </summary>
    public DateTime? LastDate
    {
      get { return _LastDate; }
      set
      {
        if (value == _LastDate)
          return;
        _LastDate = value;
        OnChanged();
      }
    }
    private DateTime? _LastDate;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ������� �������
    /// </summary>
    public override void Clear()
    {
      FirstDate = null;
      LastDate = null;
    }

    /// <summary>
    /// ���������� true, ���� ������ �� ����������
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return !(FirstDate.HasValue || LastDate.HasValue);
      }
    }

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// ����� ����� � �������� ������ ��������� ����������� ���� (������� �����
    /// �������� ������� GetColumnNames()), ����� ����� ������������� ������.
    /// </summary>
    /// <param name="rowValues">��������c ������� � ��������� �����.</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v = rowValues.GetValue(ColumnName);
      if (v == null)
        return false;
      if (v is DBNull)
        return false;

      return DataTools.DateInRange((DateTime)v, FirstDate, LastDate);
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new DateRangeFilter(ColumnName, FirstDate, LastDate);

    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void ReadConfig(CfgPart cfg)
    {
      FirstDate = cfg.GetNullableDate("FirstDate");
      LastDate = cfg.GetNullableDate("LastDate");
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableDate("FirstDate", FirstDate);
      cfg.SetNullableDate("LastDate", LastDate);
    }

    /// <summary>
    /// ���������� DateRangeFormatter ��� �������������� � ������ �������� ����
    /// </summary>
    /// <param name="columnValues">�������� �����</param>
    /// <returns>��������� ������������� ��������</returns>
    protected override string[] GetColumnStrValues(object[] columnValues)
    {
      return new string[] { DateRangeFormatter.Default.ToString(DataTools.GetNullableDateTime(columnValues[0]), true) };
    }

    #endregion
  }

  /// <summary>
  /// ������ ���������� ��������� ��� ������ ����, ����������� ������������� ��������.
  /// ����� �������� �������� ��������, ������� ������ ��������� ������.
  /// ����������� ������������ ���������.
  /// ������� ����� ��� IntRangeCommonFilter, SingleRangeCommonFilter, DoubleRangeCommonFilter � DecimalRangeCommonFilter
  /// </summary>
  public abstract class NumRangeCommonFilter<T> : SingleColumnCommonFilter
    where T : struct
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="columnName">��� ����</param>
    public NumRangeCommonFilter(string columnName)
      : base(columnName)
    {
      _NullIsZero = true;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� �������� ������� - ��������� �������� ��������� ��� null
    /// </summary>
    public T? FirstValue
    {
      get { return _FirstValue; }
      set
      {
        if (Object.Equals(value, _FirstValue))
          return;
        _FirstValue = value;
        OnChanged();
      }
    }
    private T? _FirstValue;

    /// <summary>
    /// ������� �������� ������� - �������� �������� ��������� ��� null
    /// </summary>
    public T? LastValue
    {
      get { return _LastValue; }
      set
      {
        if (Object.Equals(value, _LastValue))
          return;
        _LastValue = value;
        OnChanged();
      }
    }
    private T? _LastValue;

    /// <summary>
    /// ���� true (�� ���������, �� �������� NULL ���������� ��� 0)
    /// </summary>
    public bool NullIsZero { get { return _NullIsZero; } set { _NullIsZero = value; } }
    private bool _NullIsZero;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ������� �������
    /// </summary>
    public override void Clear()
    {
      FirstValue = null;
      LastValue = null;
    }

    /// <summary>
    /// ���������� true, ���� ������ �� ����������
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return !(FirstValue.HasValue || LastValue.HasValue);
      }
    }

    /// <summary>
    /// ���� FirstValue � LastValue ����������� � ���� � �� �� ��������, �������� �� null, �� �������� ���� ��������� ���������������� ��������� ���������.
    /// </summary>
    /// <param name="DocValue"></param>
    protected override void OnInitNewDocValue(DBxDocValue DocValue)
    {
      if (FirstValue.HasValue && LastValue.HasValue && Object.Equals(FirstValue, LastValue))
      {
        if (NullIsZero && FirstValue.Value.Equals(default(T)))
          DocValue.SetNull();
        else
          DocValue.SetValue(FirstValue.Value);
      }
    }

    #endregion
  }

  /// <summary>
  /// ������ ���������� ��������� ��� ������ ����, ����������� ������������� ��������.
  /// ����� �������� �������� ��������, ������� ������ ��������� ������.
  /// ����������� ������������ ���������.
  /// </summary>
  public class IntRangeCommonFilter : NumRangeCommonFilter<int>
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="columnName">��� �������</param>
    public IntRangeCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// ����� ����� � �������� ������ ��������� ����������� ���� (������� �����
    /// �������� ������� GetColumnNames()), ����� ����� ������������� ������.
    /// </summary>
    /// <param name="rowValues">��������c ������� � ��������� �����.</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v1 = rowValues.GetValue(ColumnName);
      int v2;
      if (v1 == null || (v1 is DBNull))
      {
        if (NullIsZero)
          v2 = 0;
        else
          return false;
      }
      else
        v2 = DataTools.GetInt(v1);

      return DataTools.IntInRange(v2, FirstValue, LastValue);
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      DBxFilter Filter = new NumRangeFilter(ColumnName, FirstValue, LastValue);

      if (NullIsZero && DataTools.IntInRange(0, FirstValue, LastValue))
        Filter = new OrFilter(new ValueFilter(ColumnName, null, typeof(int)), Filter);
      return Filter;
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void ReadConfig(CfgPart cfg)
    {
      FirstValue = cfg.GetNullableInt("FirstValue");
      LastValue = cfg.GetNullableInt("LastValue");
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableInt("FirstValue", FirstValue);
      cfg.SetNullableInt("LastValue", LastValue);
    }

    #endregion
  }

  /// <summary>
  /// ������ ���������� ��������� ��� ������ ����, ����������� �������� �������� � ��������� ������.
  /// ����� �������� �������� ��������, ������� ������ ��������� ������.
  /// ����������� ������������ ���������.
  /// </summary>
  public class SingleRangeCommonFilter : NumRangeCommonFilter<float>
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="columnName">��� �������</param>
    public SingleRangeCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// ����� ����� � �������� ������ ��������� ����������� ���� (������� �����
    /// �������� ������� GetColumnNames()), ����� ����� ������������� ������.
    /// </summary>
    /// <param name="rowValues">��������c ������� � ��������� �����.</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v1 = rowValues.GetValue(ColumnName);
      float v2;
      if (v1 == null || (v1 is DBNull))
      {
        if (NullIsZero)
          v2 = 0f;
        else
          return false;
      }
      else
        v2 = DataTools.GetSingle(v1);

      return DataTools.SingleInRange(v2, FirstValue, LastValue);
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      DBxFilter Filter = new NumRangeFilter(ColumnName, FirstValue, LastValue);

      if (NullIsZero && DataTools.SingleInRange(0, FirstValue, LastValue))
        Filter = new OrFilter(new ValueFilter(ColumnName, null, typeof(float)), Filter);
      return Filter;
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void ReadConfig(CfgPart cfg)
    {
      FirstValue = cfg.GetNullableSingle("FirstValue");
      LastValue = cfg.GetNullableSingle("LastValue");
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableSingle("FirstValue", FirstValue);
      cfg.SetNullableSingle("LastValue", LastValue);
    }

    #endregion
  }

  /// <summary>
  /// ������ ���������� ��������� ��� ������ ����, ����������� �������� �������� � ��������� ������.
  /// ����� �������� �������� ��������, ������� ������ ��������� ������.
  /// ����������� ������������ ���������.
  /// </summary>
  public class DoubleRangeCommonFilter : NumRangeCommonFilter<double>
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="columnName">��� �������</param>
    public DoubleRangeCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// ����� ����� � �������� ������ ��������� ����������� ���� (������� �����
    /// �������� ������� GetColumnNames()), ����� ����� ������������� ������.
    /// </summary>
    /// <param name="rowValues">��������c ������� � ��������� �����.</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v1 = rowValues.GetValue(ColumnName);
      double v2;
      if (v1 == null || (v1 is DBNull))
      {
        if (NullIsZero)
          v2 = 0.0;
        else
          return false;
      }
      else
        v2 = DataTools.GetDouble(v1);

      return DataTools.DoubleInRange(v2, FirstValue, LastValue);
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      DBxFilter Filter = new NumRangeFilter(ColumnName, FirstValue, LastValue);

      if (NullIsZero && DataTools.DoubleInRange(0, FirstValue, LastValue))
        Filter = new OrFilter(new ValueFilter(ColumnName, null, typeof(double)), Filter);
      return Filter;
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void ReadConfig(CfgPart cfg)
    {
      FirstValue = cfg.GetNullableDouble("FirstValue");
      LastValue = cfg.GetNullableDouble("LastValue");
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableDouble("FirstValue", FirstValue);
      cfg.SetNullableDouble("LastValue", LastValue);
    }

    #endregion
  }

  /// <summary>
  /// ������ ���������� ��������� ��� ������ ����, ����������� �������� �������� � ��������� ������.
  /// ����� �������� �������� ��������, ������� ������ ��������� ������.
  /// ����������� ������������ ���������.
  /// </summary>
  public class DecimalRangeCommonFilter : NumRangeCommonFilter<decimal>
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="columnName">��� �������</param>
    public DecimalRangeCommonFilter(string columnName)
      : base(columnName)
    {
    }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// ����� ����� � �������� ������ ��������� ����������� ���� (������� �����
    /// �������� ������� GetColumnNames()), ����� ����� ������������� ������.
    /// </summary>
    /// <param name="rowValues">��������c ������� � ��������� �����.</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object v1 = rowValues.GetValue(ColumnName);
      decimal v2;
      if (v1 == null || (v1 is DBNull))
      {
        if (NullIsZero)
          v2 = 0m;
        else
          return false;
      }
      else
        v2 = DataTools.GetDecimal(v1);

      return DataTools.DecimalInRange(v2, FirstValue, LastValue);
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      DBxFilter Filter = new NumRangeFilter(ColumnName, FirstValue, LastValue);

      if (NullIsZero && DataTools.DecimalInRange(0, FirstValue, LastValue))
        Filter = new OrFilter(new ValueFilter(ColumnName, null, typeof(float)), Filter);
      return Filter;
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void ReadConfig(CfgPart cfg)
    {
      FirstValue = cfg.GetNullableDecimal("FirstValue");
      LastValue = cfg.GetNullableDecimal("LastValue");
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void WriteConfig(CfgPart cfg)
    {
      cfg.SetNullableDecimal("FirstValue", FirstValue);
      cfg.SetNullableDecimal("LastValue", LastValue);
    }

    #endregion
  }

  #endregion

  #region ������ �������

  /// <summary>
  /// ������ �� ������ ��� ���������� ��������� ��������� ����, ������� ��
  /// ������� ������������� ��������� �������������
  /// </summary>
  public class EnumCommonFilter : SingleColumnCommonFilter
  {
    #region �����������

    /// <summary>
    /// ����������� �������
    /// </summary>
    /// <param name="columnName">��� �������</param>
    /// <param name="itemCount">���������� ��������� � ������������. ���� ����� ��������� �������� �� 0 �� (ItemCount-1). 
    /// ������ ���� �� ������ 1</param>
    public EnumCommonFilter(string columnName, int itemCount)
      : base(columnName)
    {
      if (itemCount < 1)
        throw new ArgumentOutOfRangeException("itemCount", itemCount, "���������� ��������� ������������ ������ ���� �� ������ 1");
      _ItemCount = itemCount;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���������� ��������� � ��������������.
    /// �������� � ������������.
    /// </summary>
    public int ItemCount { get { return _ItemCount; } }
    private int _ItemCount;

    /// <summary>
    /// ������� �������� �������. �������� ������ ������, ��������������� ��������
    /// ��������� ���� 0,1,2,...,(TextValues.Lenght-1).
    /// ���� ������ �� ����������, �� �������� �������� null
    /// </summary>
    public bool[] FilterFlags
    {
      get { return _FilterFlags; }
      set
      {
        _FilterFlags = value;
        OnChanged();
      }
    }
    private bool[] _FilterFlags;

    /// <summary>
    /// �������������� ��������� �������
    /// ����� ������������� ���������� ��������.
    /// �������� (-1) ������������ ������� ������� (FilterFlags=null)
    /// </summary>
    public int SingleFilterItemIndex
    {
      get
      {
        if (FilterFlags == null)
          return -1;
        int Index = -1;
        for (int i = 0; i < FilterFlags.Length; i++)
        {
          if (FilterFlags[i])
          {
            if (Index < 0)
              Index = i;
            else
              // ����������� ������ ������ �����
              return -1;
          }
        }
        return Index;
      }
      set
      {
        if (value < 0)
          FilterFlags = null;
        else
        {
          bool[] a = new bool[ItemCount];
          DataTools.FillArray<bool>(a, false);
          a[value] = true;
          FilterFlags = a;
        }
      }
    }

    #endregion

    #region ���������������� ������ � ��������

    /// <summary>
    /// ������� �������
    /// </summary>
    public override void Clear()
    {
      FilterFlags = null;
    }

    /// <summary>
    /// ���������� true, ���� ������ �� ����������
    /// </summary>
    public override bool IsEmpty
    {
      get { return FilterFlags == null; }
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      if (FilterFlags == null)
        return null;

      List<int> Values = new List<int>();
      for (int i = 0; i < FilterFlags.Length; i++)
      {
        if (FilterFlags[i])
          Values.Add(i);
      }
      DBxFilter Filter = new ValuesFilter(ColumnName, Values.ToArray());
      // 18.10.2019
      // ������������� OnFormatValuesFilter() ������ ��� ��������� ������� ������� �������� ����� Values � �������������� �������� �� NULL �� �����
      //if (FilterFlags[0])
      //  Filter = new OrFilter(Filter, new ValueFilter(ColumnName, null, typeof(Int16)));
      return Filter;
    }

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// ����� ����� � �������� ������ ��������� ����������� ���� (������� �����
    /// �������� ������� GetColumnNames()), ����� ����� ������������� ������.
    /// </summary>
    /// <param name="rowValues">��������c ������� � ��������� �����.</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      int Value = DataTools.GetInt(rowValues.GetValue(ColumnName));
      if (Value < 0 || Value > FilterFlags.Length)
        return false;
      else
        return FilterFlags[Value];
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void ReadConfig(CfgPart config)
    {
      int[] a = DataTools.CommaStringToIds(config.GetString("Flags"));
      if (a == null)
        FilterFlags = null;
      else
      {
        FilterFlags = new bool[ItemCount];
        for (int i = 0; i < a.Length; i++)
        {
          if (a[i] >= 0 && a[i] < FilterFlags.Length)
            FilterFlags[a[i]] = true;
        }
      }
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void WriteConfig(CfgPart config)
    {
      int[] a = null;
      if (FilterFlags != null)
      {
        List<int> lst = new List<int>();
        for (int i = 0; i < FilterFlags.Length; i++)
        {
          if (FilterFlags[i])
            lst.Add(i);
        }
        a = lst.ToArray();
      }
      config.SetString("Flags", DataTools.CommaStringFromIds(a, false));
    }

    /// <summary>
    /// ���������� ��� �������� ������ ��������� �� ���������.
    /// ������������� ��������� �������� ���� ColumnName, ���� � ������� ������� ������������ ��������.
    /// </summary>
    /// <param name="docValue">�������� ����, ������� ����� ���������� ����</param>
    protected override void OnInitNewDocValue(DBxDocValue docValue)
    {
      if (SingleFilterItemIndex >= 0)
        docValue.SetInteger(SingleFilterItemIndex);
    }

    #endregion

    #region �������� ��������

    /// <summary>
    /// �������� �������� ��� ������� ������.
    /// ���� ������ �� ���������� (IsEmpty=true), ������������ true
    /// </summary>
    /// <param name="rowValue">����������� ��������</param>
    /// <returns>true, ���� �������� �������� ������� �������</returns>
    public bool TestValue(int rowValue)
    {
      if (IsEmpty)
        return true;

      if (rowValue < 0 || rowValue >= FilterFlags.Length)
        return false;

      return FilterFlags[rowValue];
    }

    #endregion
  }

  #region ������������ NullNotNullGridFilterValue

  /// <summary>
  /// ��������� ��������� ������� NullNotNullCommonFilter
  /// </summary>
  public enum NullNotNullFilterValue
  {
    /// <summary>
    /// ������ �� ����������
    /// </summary>
    NoFilter,

    /// <summary>
    /// ���� ����� ��������, �������� �� NULL
    /// </summary>
    NotNull,

    /// <summary>
    /// ���� ����� �������� NULL
    /// </summary>
    Null,
  }

  #endregion

  /// <summary>
  /// ������ �� ������� ��� ���������� �������� NULL/NOT NULL (������, ��� ����
  /// ���� "����")
  /// </summary>
  public class NullNotNullCommonFilter : SingleColumnCommonFilter
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <param name="columnType">��� ������, ������� �������� � ����</param>
    public NullNotNullCommonFilter(string columnName, Type columnType)
      : base(columnName)
    {
#if DEBUG
      if (columnType == null)
        throw new ArgumentNullException("columnType");
#endif
      _ColumnType = columnType;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������� ��������� �������: �� ����������, NULL ��� NOT NULL
    /// </summary>
    public NullNotNullFilterValue Value
    {
      get { return _Value; }
      set
      {
        if (value == _Value)
          return;
        _Value = value;
        OnChanged();
      }
    }
    private NullNotNullFilterValue _Value;

    /// <summary>
    /// ��� ��������, ����������� � ����.
    /// �������� �������� � ������������ �������.  
    /// ��������� ��� �������� ������� NotNullFilter
    /// </summary>
    public Type ColumnType { get { return _ColumnType; } }
    private Type _ColumnType;

    #endregion

    #region ���������������� ������ � ��������

    /// <summary>
    /// ������� �������
    /// </summary>
    public override void Clear()
    {
      Value = NullNotNullFilterValue.NoFilter;
    }

    /// <summary>
    /// ���������� true, ���� ������ �� ����������
    /// </summary>
    public override bool IsEmpty
    {
      get { return Value == NullNotNullFilterValue.NoFilter; }
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      switch (Value)
      {
        case NullNotNullFilterValue.NotNull:
          return new NotNullFilter(ColumnName, ColumnType);
        case NullNotNullFilterValue.Null:
          return new ValueFilter(ColumnName, null, ColumnType);
        default:
          return null;
      }
    }

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// ����� ����� � �������� ������ ��������� ����������� ���� (������� �����
    /// �������� ������� GetColumnNames()), ����� ����� ������������� ������.
    /// </summary>
    /// <param name="rowValues">��������c ������� � ��������� �����.</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      object RowValue = rowValues.GetValue(ColumnName);
      return TestValue(RowValue);
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void ReadConfig(CfgPart config)
    {
      string s = config.GetString("Mode");
      switch (s)
      {
        case "Null":
          Value = NullNotNullFilterValue.Null;
          break;
        case "NotNull":
          Value = NullNotNullFilterValue.NotNull;
          break;
        default:
          Value = NullNotNullFilterValue.NoFilter;
          break;
      }
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void WriteConfig(CfgPart config)
    {
      string s;
      switch (Value)
      {
        case NullNotNullFilterValue.Null: s = "Null"; break;
        case NullNotNullFilterValue.NotNull: s = "NotNull"; break;
        default: s = String.Empty; break;
      }
      config.SetString("Mode", s);
    }

    #endregion

    #region �������� ��������

    /// <summary>
    /// �������� �������� ��� ������� ������.
    /// ���� ������ �� ���������� (IsEmpty=true), ������������ true
    /// </summary>
    /// <param name="rowValue">����������� ��������</param>
    /// <returns>true, ���� �������� �������� ������� �������</returns>
    public bool TestValue(object rowValue)
    {
      bool IsNull = (rowValue == null || rowValue is DBNull);
      switch (Value)
      {
        case NullNotNullFilterValue.Null:
          return IsNull;
        case NullNotNullFilterValue.NotNull:
          return !IsNull;
        default:
          return true;
      }
    }

    #endregion
  }

  #endregion

  #region ������������ RefDocFilterMode

  /// <summary>
  /// ������ ������ RefDocGridFilter.
  /// �������������� ����� ���������� �� ��������� ������� � ����� ���������� ��������� �������
  /// </summary>
  public enum RefDocFilterMode
  {
    /// <summary>
    /// ������ �� ����������
    /// </summary>
    NoFilter,

    /// <summary>
    /// ����� ���������� �� ��������� �������
    /// </summary>
    Include,

    /// <summary>
    /// ����� ���������� ��������� �������
    /// </summary>
    Exclude,

    /// <summary>
    /// ������ �� ����� ������.
    /// ������ �������������� ������ ��� ��������� �����, �������������� �������� NULL.
    /// </summary>
    NotNull,

    /// <summary>
    /// ������ �� �������� Null.
    /// ������ �������������� ������ ��� ��������� �����, �������������� �������� NULL.
    /// </summary>
    Null
  }

  #endregion

  /// <summary>
  /// ������ �� �������� ���������� ���� �� ��������
  /// �������� ������ �� ���������� ��������������� � ����� "���������"
  /// ������ �� ������� �������� ���� ����������
  /// </summary>
  public class RefDocCommonFilter : SingleColumnCommonFilter
  {
    #region ������������

    /// <summary>
    /// ������� ����� ������
    /// </summary>
    /// <param name="docProvider">��������� ��� ������� � ����������</param>
    /// <param name="docType">�������� ���� ���������, �� �������� �������������� �����</param>
    /// <param name="columnName">��� ���������� �������</param>
    public RefDocCommonFilter(DBxDocProvider docProvider, DBxDocType docType, string columnName)
      : base(columnName)
    {
      if (docProvider == null)
        throw new ArgumentNullException("docProvider");
      if (docType == null)
        throw new ArgumentNullException("docType");

      _DocProvider = docProvider;
      _DocType = docType;
      _Mode = RefDocFilterMode.NoFilter;
    }

    /// <summary>
    /// ������� ����� ������
    /// </summary>
    /// <param name="docProvider">��������� ��� ������� � ����������</param>
    /// <param name="docTypeName">��� ���� ���������, �� �������� �������������� �����</param>
    /// <param name="columnName">��� ���������� �������</param>
    public RefDocCommonFilter(DBxDocProvider docProvider, string docTypeName, string columnName)
      : this(docProvider, docProvider.DocTypes[docTypeName], columnName)
    {
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ��� ������� � ����������
    /// </summary>
    public DBxDocProvider DocProvider { get { return _DocProvider; } }
    private DBxDocProvider _DocProvider;

    /// <summary>
    /// �������� ���� ���������, �� �������� �������������� �����.
    /// �������� � ������������ ������� �������.
    /// </summary>
    public DBxDocType DocType { get { return _DocType; } }
    private DBxDocType _DocType;

    /// <summary>
    /// ��� ������� ���������
    /// </summary>
    public string DocTypeName { get { return _DocType.Name; } }

    /// <summary>
    /// ����� �������
    /// </summary>
    public RefDocFilterMode Mode { get { return _Mode; } }
    private RefDocFilterMode _Mode;

    /// <summary>
    /// �������������� ����������, ���� ������ ����������
    /// </summary>
    public IdList DocIds { get { return _DocIds; } }
    private IdList _DocIds;

    /// <summary>
    /// ���������� ��� �������� ������
    /// </summary>
    /// <param name="mode">����� �������</param>
    /// <param name="docIds">������ ��������������� ��������� ����������, ���� ��������� � ���������� ������.
    /// ���� �����������, �������� ������������</param>
    public void SetFilter(RefDocFilterMode mode, Int32[] docIds)
    {
      IdList DocIds2 = null;
      if (docIds != null)
        DocIds2 = new IdList(docIds);
      SetFilter(mode, DocIds2);
    }

    /// <summary>
    /// ���������� ��� �������� ������.
    /// ��� ���������� ������������� ������ ��� ������� <paramref name="mode"/>=NoFilter, Null � NotNull.
    /// </summary>
    /// <param name="mode">����� �������</param>
    public void SetFilter(RefDocFilterMode mode)
    {
      SetFilter(mode, (IdList)null);
    }

    /// <summary>
    /// ���������� ��� �������� ������
    /// </summary>
    /// <param name="mode">����� �������</param>
    /// <param name="docIds">������ ��������������� ��������� ����������, ���� ������ ���������������</param>
    public void SetFilter(RefDocFilterMode mode, IdList docIds)
    {
      switch (mode)
      {
        case RefDocFilterMode.NoFilter:
          Clear();
          return;
        case RefDocFilterMode.Include:
        case RefDocFilterMode.Exclude:
          if (docIds == null)
            throw new ArgumentNullException("docIds");
          // 24.07.2019 - ��������� ������ ������ ���������������
          //if (DocIds.Count == 0)
          //  throw new ArgumentException("�� ����� ������ ��������������� ��� �������", "DocIds");
          _Mode = mode;
          _DocIds = docIds;
          _DocIds.SetReadOnly();
          OnChanged();
          break;
        case RefDocFilterMode.NotNull:
        case RefDocFilterMode.Null:
          if (mode == _Mode)
            return;
          _Mode = mode;
          _DocIds = null;
          OnChanged();
          break;
        default:
          throw new ArgumentException("����������� ����� " + mode.ToString(), "mode");
      }
    }

    /// <summary>
    /// ���������� ��������� ������� �� ������������� ��������. 
    /// �������� ���������� ������������� ��������� ��� ������������ ��� ���������� ����, 
    /// ���� ���������� ����� "��������" � ����� ���� ������������� � ������. ���������� 0,
    /// ���� �) ������ �� ���������� ��� �) ������ � ������ "�����", ��� �) �������� 
    /// ��������� ��������������� � ������.
    /// ��������� �������� �������� � ��������� �������� ������������� ������ ��
    /// ������ ���������, � � ������� �������� - ������� ������
    /// </summary>
    public Int32 SingleDocId
    {
      get
      {
        if (_Mode == RefDocFilterMode.Include && _DocIds.Count == 1)
          return _DocIds.SingleId;
        else
          return 0;
      }
      set
      {
        if (value == SingleDocId)
          return;
        if (value == 0)
          Clear();
        else
          SetFilter(RefDocFilterMode.Include, new Int32[1] { value });
      }
    }

    /// <summary>
    /// ������������� ���� ������.
    /// ������������ � ��������� � ������� ������
    /// </summary>
    public override string DBIdentity
    {
      get { return DocProvider.DBIdentity; }
    }

    #endregion

    #region ���������������� ������ � ��������

    /// <summary>
    /// �������� ������
    /// </summary>
    public override void Clear()
    {
      if (IsEmpty)
        return;
      _Mode = RefDocFilterMode.NoFilter;
      _DocIds = null;
      OnChanged();
    }

    /// <summary>
    /// ���������� true, ���� ������ �� ����������
    /// </summary>
    public override bool IsEmpty
    {
      get { return Mode == RefDocFilterMode.NoFilter; }
    }

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// ����� ����� � �������� ������ ��������� ����������� ���� (������� �����
    /// �������� ������� GetColumnNames()), ����� ����� ������������� ������.
    /// </summary>
    /// <param name="rowValues">��������c ������� � ��������� �����.</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Int32 Id = DataTools.GetInt(rowValues.GetValue(ColumnName));
      return TestValue(Id);
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      switch (Mode)
      {
        case RefDocFilterMode.NoFilter:
          return null;
        case RefDocFilterMode.Include:
          if (_DocIds.Count == 0)
            return DummyFilter.AlwaysFalse; // 24.07.2019
          else
            return new IdsFilter(ColumnName, _DocIds);
        case RefDocFilterMode.Exclude:
          if (_DocIds.Count == 0)
            return null; // 24.07.2019
          else
            return new NotFilter(new IdsFilter(ColumnName, _DocIds));
        case RefDocFilterMode.NotNull:
          return new NotNullFilter(ColumnName, typeof(Int32));
        case RefDocFilterMode.Null:
          return new ValueFilter(ColumnName, null, typeof(Int32));
        default:
          throw new BugException("����������� ����� " + Mode.ToString());
      }
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void ReadConfig(CfgPart config)
    {
      RefDocFilterMode NewMode = config.GetEnum<RefDocFilterMode>("Mode");
      Int32[] NewDocIds = DataTools.CommaStringToIds(config.GetString("Ids"));
      SetFilter(NewMode, NewDocIds);
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetEnum("Mode", Mode);
      if (DocIds != null)
        config.SetString("Ids", DataTools.CommaStringFromIds(_DocIds.ToArray(), false));
      else
        config.Remove("Ids");
    }

    /// <summary>
    /// ���������� ��� �������� ������ ��������� �� ���������.
    /// ������������� ��������� �������� ���� ColumnName, ���� � ������� ������� ������������ ��������.
    /// </summary>
    /// <param name="docValue">�������� ����, ������� ����� ����������</param>
    protected override void OnInitNewDocValue(DBxDocValue docValue)
    {
      switch (Mode)
      {
        case RefDocFilterMode.Include:
          Int32 Id = SingleDocId;
          if (Id != 0)
            docValue.SetInteger(Id);
          break;
        case RefDocFilterMode.Null:
          docValue.SetNull();
          break;
      }
    }

#pragma warning disable 1591

    public override bool CanAsCurrRow(DataRow row)
    {
      Int32 ThisId = DataTools.GetInt(row, ColumnName);
      if (ThisId == 0 || ThisId == SingleDocId)
        return false;
      return true;
    }

    public override void SetAsCurrRow(DataRow row)
    {
      Int32 ThisId = DataTools.GetInt(row, ColumnName);
      SingleDocId = ThisId;
    }

#pragma warning restore 1591

    ///// <summary>
    ///// �������� DBxDocTextHandlers.GetTextValue() ��� ��������� ���������� �������������
    ///// </summary>
    ///// <param name="ColumnValues">�������� �����</param>
    ///// <returns>��������� ������������� ��������</returns>
    //protected override string[] GetColumnStrValues(object[] ColumnValues)
    //{
    //  return new string[] { UI.TextHandlers.GetTextValue(DocTypeName, DataTools.GetInt(ColumnValues[0])) };
    //}

    #endregion

    #region ������ ������

    /// <summary>
    /// ��������� ������������ �������������� �������
    /// </summary>
    /// <param name="id">�������������</param>
    /// <returns>true, ���� ������������� �������� ������� �������</returns>
    public bool TestValue(Int32 id)
    {
      switch (Mode)
      {
        case RefDocFilterMode.NoFilter:
          return true;
        case RefDocFilterMode.Include:
          return DocIds.Contains(id);
        case RefDocFilterMode.Exclude:
          return !DocIds.Contains(id);
        case RefDocFilterMode.NotNull:
          return id != 0;
        case RefDocFilterMode.Null:
          return id == 0;
        default:
          throw new BugException("����������� ����� " + Mode.ToString());
      }
    }

    #endregion
  }

  /// <summary>
  /// ����� �� ������� �� ������ � ������� �� ����������
  /// </summary>
  public class RefDocCommonFilterSet : DBxCommonFilterSet
  {
    #region ������������

    /// <summary>
    /// ������� ����� �� ������ ��� ���� ��������
    /// </summary>
    /// <param name="docProvider">��������� ��� ������� � ����������</param>
    /// <param name="docType">�������� ���� ���������, �� �������� �������������� �����</param>
    /// <param name="columnName">��� ���������� �������</param>
    public RefDocCommonFilterSet(DBxDocProvider docProvider, DBxDocType docType, string columnName)
    {
      if (docProvider == null)
        throw new ArgumentNullException("docProvider");
      if (docType == null)
        throw new ArgumentNullException("docType");
      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");

      if (!String.IsNullOrEmpty(docType.GroupRefColumnName))
      {
        DBxColumnStruct GroupIdCol = docType.Struct.Columns[docType.GroupRefColumnName];
        if (GroupIdCol == null)
          throw new ArgumentException("������������ �������� ���� ��������� \"" + docType.Name + "\". ��� ���� \"" + docType.GroupRefColumnName + "\"", "docType");
        if (String.IsNullOrEmpty(GroupIdCol.MasterTableName))
          throw new ArgumentException("������������ �������� ���� ��������� \"" + docType.Name + "\". ���� \"" + docType.GroupRefColumnName + "\" ��� ��������", "docType");
        DBxDocType GroupDocType = docProvider.DocTypes[GroupIdCol.MasterTableName];
        if (GroupDocType == null)
          throw new NullReferenceException("�� ������� ��������� ��� ������-������� \"" + GroupIdCol.MasterTableName + "\"");
        _GroupFilter = new RefDocCommonFilter(docProvider, GroupDocType, columnName + "." + docType.GroupRefColumnName);
        _GroupFilter.DisplayName = GroupDocType.SingularTitle;
        Add(_GroupFilter);
      }

      _DocFilter = new RefDocCommonFilter(docProvider, docType, columnName);
      Add(_DocFilter);
    }

    /// <summary>
    /// ������� ����� �� ������ ��� ���� ��������
    /// </summary>
    /// <param name="docProvider">��������� ��� ������� � ����������</param>
    /// <param name="docTypeName">��� ���� ���������, �� �������� �������������� �����</param>
    /// <param name="columnName">��� ���������� �������</param>
    public RefDocCommonFilterSet(DBxDocProvider docProvider, string docTypeName, string columnName)
      : this(docProvider, docProvider.DocTypes[docTypeName], columnName)
    {
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������ �� ������ ����������.
    /// ���� ��� ����������, �� ������� ��������� ����, �� ���������� ������, �������� ���������� null
    /// </summary>
    public RefDocCommonFilter GroupFilter { get { return _GroupFilter; } }
    private RefDocCommonFilter _GroupFilter;

    /// <summary>
    /// �������� ������
    /// </summary>
    public RefDocCommonFilter DocFilter { get { return _DocFilter; } }
    private RefDocCommonFilter _DocFilter;

    #endregion
  }

  /// <summary>
  /// ������ �� ���� GroupId
  /// </summary>
  public class RefGroupDocCommonFilter : SingleColumnCommonFilter
  {
    #region �����������

    /// <summary>
    /// ������� ����� ������
    /// </summary>
    /// <param name="docProvider">��������� ��� ������� � ����������</param>
    /// <param name="groupDocType">�������� ���� ��������� �����</param>
    /// <param name="groupRefColumnName">��� ���������� ����, ������������� ������</param>
    public RefGroupDocCommonFilter(DBxDocProvider docProvider, DBxDocType groupDocType, string groupRefColumnName)
      : base(groupRefColumnName)
    {
      if (docProvider == null)
        throw new ArgumentNullException("docProvider");
      if (groupDocType == null)
        throw new ArgumentNullException("groupDocType");
      if (String.IsNullOrEmpty(groupDocType.TreeParentColumnName))
        throw new ArgumentException("��� ���������� \"" + groupDocType.Name + "\" �� ����������� �������� TreeParentColumnName. �������������, ��� ��������� �� ����� ���� ������� �����");

      _DocProvider = docProvider;
      _GroupDocType = groupDocType;

      base.DisplayName = groupDocType.SingularTitle; // �����, ��� "GroupId"

      _GroupId = 0;
      _IncludeNestedGroups = true;
    }

    /// <summary>
    /// ������� ����� ������
    /// </summary>
    /// <param name="docProvider">��������� ��� ������� � ����������</param>
    /// <param name="groupDocTypeName">��� ���� ���������� ������ �����</param>
    /// <param name="groupRefColumnName">��� ���������� ����, ������������� ������</param>
    public RefGroupDocCommonFilter(DBxDocProvider docProvider, string groupDocTypeName, string groupRefColumnName)
      : this(docProvider, docProvider.DocTypes[groupDocTypeName], groupRefColumnName)
    {
    }

    #endregion

    #region ����� ��������

    /// <summary>
    /// ��������� ��� ������� � ����������
    /// </summary>
    public DBxDocProvider DocProvider { get { return _DocProvider; } }
    private DBxDocProvider _DocProvider;

    /// <summary>
    /// �������� ���� ��������� �����.
    /// �������� � ������������ ������� �������.
    /// </summary>
    public DBxDocType GroupDocType { get { return _GroupDocType; } }
    private DBxDocType _GroupDocType;

    /// <summary>
    /// ��� ������� ��������� �����.
    /// </summary>
    public string GroupDocTypeName { get { return _GroupDocType.Name; } }

    #endregion

    #region ������� ��������� �������

    /// <summary>
    /// ������������� ��������� ������
    /// </summary>
    public Int32 GroupId
    {
      get { return _GroupId; }
      set
      {
        if (value == _GroupId)
          return;
        _GroupId = value;
        OnChanged();
      }
    }
    private Int32 _GroupId;

    /// <summary>
    /// ���� true, �� ���������� ����� ��������� ������
    /// </summary>
    public bool IncludeNestedGroups
    {
      get { return _IncludeNestedGroups; }
      set
      {
        if (value == _IncludeNestedGroups)
          return;
        _IncludeNestedGroups = value;
        OnChanged();
      }
    }
    private bool _IncludeNestedGroups;


    /// <summary>
    /// ������������� ���� ������.
    /// ������������ � ��������� � ������� ������
    /// </summary>
    public override string DBIdentity
    {
      get { return DocProvider.DBIdentity; }
    }

    #endregion

    #region �������� AuxFilterGroupIds

    /// <summary>
    /// ���������� ������ ��������������� ��������������� ����� ����������.
    /// ������������ ��� ���������� ���������� � �������� ���������.
    /// ���� ������� "��� ���������", ���������� null.
    /// ���� ������� "��������� ��� �����", ���������� ������ ������� �����.
    /// ���� ���� ��������� ������, ���������� ������ �� ������ ��� ���������� ���������,
    /// � ����������� �� IncludeNested
    /// </summary>
    public IdList AuxFilterGroupIdList
    {
      get
      {
        if (!_AuxFilterGroupIdsReady)
        {
          _AuxFilterGroupIdList = GetAuxFilterGroupIdList();
          _AuxFilterGroupIdsReady = true;
        }
        return _AuxFilterGroupIdList;
      }
    }

    private IdList _AuxFilterGroupIdList;

    /// <summary>
    /// ������ ��������������� � true, ���� FAuxFilterGroupIdList �������� ���������� ��������
    /// </summary>
    private bool _AuxFilterGroupIdsReady;

    private IdList GetAuxFilterGroupIdList()
    {
      if (GroupId == 0)
      {
        if (IncludeNestedGroups)
          return null;
        else
          return IdList.Empty;
      }
      else
      {
        if (IncludeNestedGroups)
        {
          DBxDocTreeModel Model = new DBxDocTreeModel(DocProvider,
            GroupDocType,
            new DBxColumns(new string[] { "Id", GroupDocType.TreeParentColumnName }));

          return new IdList(Model.GetIdWithChildren(GroupId));
        }
        else
          return IdList.FromId(GroupId);
      }
    }

    #endregion

    #region ���������������� ������ � ��������

    /// <summary>
    /// ���� ����� ���������� ��� ��������� �������� �������.
    /// </summary>
    protected override void OnChanged()
    {
      _AuxFilterGroupIdsReady = false;
      base.OnChanged();
    }

    /// <summary>
    /// �������� ������
    /// </summary>
    public override void Clear()
    {
      if (IsEmpty)
        return;
      _GroupId = 0;
      _IncludeNestedGroups = true;
      OnChanged();
    }

    /// <summary>
    /// ���������� true, ���� ������ �� ����������:
    /// GroupId=0 � IncludeNestedGroups=true.
    /// </summary>
    public override bool IsEmpty
    {
      get { return GroupId == 0 && IncludeNestedGroups; }
    }

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// ����� ����� � �������� ������ ��������� ����������� ���� (������� �����
    /// �������� ������� GetColumnNames()), ����� ����� ������������� ������.
    /// </summary>
    /// <param name="rowValues">��������c ������� � ��������� �����.</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Int32 Id = DataTools.GetInt(rowValues.GetValue(ColumnName));
      return TestValue(Id);
    }

    /// <summary>
    /// ��������� ��������� �������� ���� � ������
    /// </summary>
    /// <param name="id">����������� �������� ���������� ����</param>
    /// <returns>��������� ��������</returns>
    public bool TestValue(Int32 id)
    {
      if (GroupId == 0)
      {
        if (IncludeNestedGroups)
          return true;
        else
          return id == 0;
      }
      else
        return AuxFilterGroupIdList.Contains(id);
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      if (IncludeNestedGroups)
      {
        if (GroupId == 0)
          return null;
        else
          return new IdsFilter(ColumnName, AuxFilterGroupIdList);
      }
      else
        return new ValueFilter(ColumnName, GroupId);
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void ReadConfig(CfgPart config)
    {
      GroupId = config.GetInt("GroupId");
      IncludeNestedGroups = config.GetBoolDef("IncludeNestedGroups", true);
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetInt("GroupId", GroupId);
      config.SetBool("IncludeNestedGroups", IncludeNestedGroups);
    }

    /// <summary>
    /// ���������� ��� �������� ������ ��������� �� ���������.
    /// ������������� ��������� �������� ���� ColumnName, ���� � ������� ������� ������������ ��������.
    /// </summary>
    /// <param name="docValue">�������� ����, ������� ����� ����������</param>
    protected override void OnInitNewDocValue(DBxDocValue docValue)
    {
      if (IncludeNestedGroups)
        return;

      docValue.SetInteger(GroupId);
    }

    #endregion
  }

  /// <summary>
  /// ������ �� ���� ���������
  /// ������� ��������� ��������� ���� �������� ������������� ������� ��������� DocType.TableId
  /// </summary>
  public class DocTableIdCommonFilter : SingleColumnCommonFilter
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="columnName">������� ���� Int32, �������� ������������� ���� ��������� �� ������� DocTables</param>
    public DocTableIdCommonFilter(string columnName)
      : base(columnName)
    {
      _CurrentTableId = 0;
    }

    #endregion

    #region ������� ���������

    /// <summary>
    /// ��������� ��� ��������� (�������� DocType.TableId) ��� 0, ���� ������ �� ����������.
    /// �������� CurrentDocTypeName, CurrentDocTypeUI � CurrentTableId ����������������.
    /// </summary>
    public Int32 CurrentTableId
    {
      get { return _CurrentTableId; }
      set
      {
        if (value == _CurrentTableId)
          return;
        _CurrentTableId = value;
        OnChanged();
      }
    }
    private Int32 _CurrentTableId;

    #endregion

    #region ���������������� ������ � ��������

    /// <summary>
    /// ������� �������
    /// </summary>
    public override void Clear()
    {
      CurrentTableId = 0;
    }

    /// <summary>
    /// ���������� true, ���� ������ �� ����������
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return CurrentTableId == 0;
      }
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      if (CurrentTableId == 0)
        return null;
      return new ValueFilter(ColumnName, CurrentTableId);
    }

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// ����� ����� � �������� ������ ��������� ����������� ���� (������� �����
    /// �������� ������� GetColumnNames()), ����� ����� ������������� ������.
    /// </summary>
    /// <param name="rowValues">��������c ������� � ��������� �����.</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      int v = DataTools.GetInt(rowValues.GetValue(ColumnName));
      return v == CurrentTableId;
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void ReadConfig(CfgPart config)
    {
      CurrentTableId = config.GetInt("TableId");
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetInt("TableId", CurrentTableId);
    }

    ///// <summary>
    ///// ���������� �������� DBxDocType.PluralTitle, ���� ����� ������ �� ���� ����������.
    ///// </summary>
    ///// <param name="ColumnValues">�������� �����</param>
    ///// <returns>��������� ������������� ��������</returns>
    //protected override string[] GetColumnStrValues(object[] ColumnValues)
    //{
    //  string s;
    //  Int32 ThisTableId = DataTools.GetInt(ColumnValues[0]);
    //  if (ThisTableId == 0)
    //    s = "���";
    //  else
    //  {
    //    DBxDocType DocType = UI.DocProvider.DocTypes.FindByTableId(ThisTableId);
    //    if (DocType == null)
    //      s = "����������� ��� ��������� � TableId=" + ThisTableId.ToString();
    //    else
    //    {
    //      s = DocType.PluralTitle;
    //      if (UI.DebugShowIds)
    //        s += " (TableId=" + DocType.TableId.ToString() + ")";
    //    }
    //  }
    //  return new string[] { s };
    //}

    #endregion

    #region �������� ��������

    /// <summary>
    /// �������� �������� ��� ������� ������.
    /// ���� ������ �� ���������� (IsEmpty=true), ������������ true
    /// </summary>
    /// <param name="rowValue">����������� ��������</param>
    /// <returns>true, ���� �������� �������� ������� �������</returns>
    public bool TestValue(Int32 rowValue)
    {
      if (IsEmpty)
        return true;
      return rowValue == CurrentTableId;
    }

    #endregion
  }

  #endregion

  #region ������� ��� ���� �����

  /// <summary>
  /// ������� ����� ��� �������� �� ������ ����.
  /// ���������� �������� ColumnName
  /// </summary>
  public abstract class TwoColumnsCommonFilter : DBxCommonFilter
  {
    #region �����������

    /// <summary>
    /// ������� ������ �������.
    /// ������������� �������� DBxCommonFilter.Code ������ "ColumnName1_ColumnName2".
    /// �������� DisplayName �������� �� ������������������. ��� �������������� ������������� ��� ����� ����� �������� Code.
    /// </summary>
    /// <param name="columnName1">��� ������� ����. ������ ���� ������</param>
    /// <param name="columnName2">��� ������� ����. ������ ���� ������</param>
    public TwoColumnsCommonFilter(string columnName1, string columnName2)
    {
      if (String.IsNullOrEmpty(columnName1))
        throw new ArgumentNullException("columnName1");
      if (String.IsNullOrEmpty(columnName2))
        throw new ArgumentNullException("columnName2");
      if (columnName1 == columnName2)
        throw new ArgumentException("����� ����� ���������", "columnName2");

      _ColumnName1 = columnName1;
      _ColumnName2 = columnName2;

      base.Code = columnName1 + "_" + columnName2; // ����� ���� �������� ����� � ���������������� ����
    }

    #endregion

    #region �������� � ������

    /// <summary>
    /// ��� ������� ������������ ����.
    /// �������� � ������������ � �� ����� ���� ��������
    /// </summary>
    public string ColumnName1 { get { return _ColumnName1; } }
    private string _ColumnName1;

    /// <summary>
    /// ��� ������� ������������ ����.
    /// �������� � ������������ � �� ����� ���� ��������
    /// </summary>
    public string ColumnName2 { get { return _ColumnName2; } }
    private string _ColumnName2;

    /// <summary>
    /// �������� ������ ���� �����, ������� ���������� ��� ���������� �������.
    /// ���� ����������� � ������ ���������� �� ����, ������� ������ ������ ��� ���.
    /// ��������� � ������ ���� ColumnName.
    /// </summary>
    /// <param name="list">������ ��� ���������� �����</param>
    public override /*sealed*/ void GetColumnNames(DBxColumnList list)
    {
      list.Add(ColumnName1);
      list.Add(ColumnName2);
    }

    /// <summary>
    /// ���������� ��� �������� ������ ��������� �� ���������.
    /// ��������� ������� � ��������� ���� ColumnName � �������� ����� OnInitNewDocValue() ��� �������� ����
    /// </summary>
    /// <param name="newDoc">��������� ��������, � ������� ����� ���������� ����</param>
    protected override /*sealed*/ void OnInitNewDocValues(DBxSingleDoc newDoc)
    {
      int p1 = newDoc.Values.IndexOf(ColumnName1);
      int p2 = newDoc.Values.IndexOf(ColumnName2);
      if (p1 < 0 || p2 < 0)
        return;

      OnInitNewDocValues(newDoc.Values[p1], newDoc.Values[p2]);
    }

    /// <summary>
    /// ������������� �������� ���� ��� �������� ������ ���������.
    /// ����� ����������, ������ ����� ������ ����������
    /// </summary>
    /// <param name="docValue1">�������� ������� ����, ������� ����� ����������</param>
    /// <param name="docValue2">�������� ������� ����, ������� ����� ����������</param>
    protected virtual void OnInitNewDocValues(DBxDocValue docValue1, DBxDocValue docValue2)
    {
    }

    #endregion
  }

  /// <summary>
  /// ������ �� ��������� ���
  /// � ������� ������ ���� ��� ���� ���� ����, ������� ���������� �������� ���.
  /// � ������� �������� ����. � �������� �������� ������, � ������� �������� ���
  /// �������� � ���� ��� ����. �������������� �������� � ������������ ���������,
  /// ����� ���� ��� ��� ���� �������� NULL
  /// ������������ ����������� ����� ������� "������� ����".
  /// </summary>
  public class DateRangeInclusionCommonFilter : TwoColumnsCommonFilter
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="firstDateColumnName">��� ���� ���� "����", ��������� ������ ���������</param>
    /// <param name="lastDateColumnName">��� ���� ���� "����", ��������� ����� ���������</param>
    public DateRangeInclusionCommonFilter(string firstDateColumnName, string lastDateColumnName)
      : base(firstDateColumnName, lastDateColumnName)
    {
      DisplayName = "������";
    }

    #endregion

    #region ������� ���������

    /// <summary>
    /// ������� �������� �������. ���� ��� null, ���� ������� ���
    /// </summary>
    public DateTime? Date
    {
      get { return _Date; }
      set
      {
        if (value == _Date)
          return;
        _Date = value;
        if (_Date.HasValue)
        {
          if (_Date.Value != WorkDate)
            _UseWorkDate = false;
        }
        else
          _UseWorkDate = false;
        OnChanged();
      }
    }
    private DateTime? _Date;

    /// <summary>
    /// ������������ �� ������� ����?
    /// </summary>
    public bool UseWorkDate
    {
      get { return _UseWorkDate; }
      set
      {
        if (value == _UseWorkDate)
          return;
        _UseWorkDate = value;
        if (_UseWorkDate && _Date.HasValue)
          _Date = WorkDate;
        OnChanged();
      }
    }
    private bool _UseWorkDate;

    #endregion

    #region ��������, ������� ����� �������������� ��� ������������� ������� ����

    /// <summary>
    /// ���� ��������������, �� ����� ���������� ������� ���� ������ �������
    /// </summary>
    public virtual DateTime WorkDate { get { return DateTime.Today; } }

    #endregion

    #region ���������������� ������ � ��������

    /// <summary>
    /// ������� �������
    /// </summary>
    public override void Clear()
    {
      Date = null;
    }

    /// <summary>
    /// ���������� true, ���� ������ �� ����������
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return !Date.HasValue;
      }
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new DateRangeInclusionFilter(ColumnName1, ColumnName2, Date.Value);
    }

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// </summary>
    /// <param name="rowValues">�������� �����</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Nullable<DateTime> dt1 = DataTools.GetNullableDateTime(rowValues.GetValue(ColumnName1));
      Nullable<DateTime> dt2 = DataTools.GetNullableDateTime(rowValues.GetValue(ColumnName2));
      return DataTools.DateInRange(Date.Value, dt1, dt2);
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void ReadConfig(CfgPart config)
    {
      Date = config.GetNullableDate("Date");
      UseWorkDate = config.GetBool("UseWorkDate");
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetNullableDate("Date", Date);
      config.SetBool("UseWorkDate", UseWorkDate);
    }

    #endregion

    #region �������� ��������

    /// <summary>
    /// �������� �������� ��� ������� ������.
    /// ���� ������ �� ���������� (IsEmpty=true), ������������ true
    /// </summary>
    /// <param name="rowValue1">����������� �������� ������� ����</param>
    /// <param name="rowValue2">����������� �������� ������� ����</param>
    /// <returns>true, ���� �������� �������� ������� �������</returns>
    public bool TestValue(DateTime? rowValue1, DateTime? rowValue2)
    {
      if (IsEmpty)
        return true;
      return DataTools.DateInRange(Date.Value, rowValue1, rowValue2);
    }

    #endregion
  }

  /// <summary>
  /// ������ �� ���� �����, ���������� �������� ���.
  /// � ������ ������ ������, � �������� ��� ������� �������� ����� �� ��� � ��������� ���������.
  /// �������������� ������������ ��������� � � ���� ������, � � ����������� ���������.
  /// ���������� ������� �� ��������������.
  /// </summary>
  public class DateRangeCrossCommonFilter : TwoColumnsCommonFilter
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="firstDateColumnName">��� ���� ���� "����", ��������� ������ ���������</param>
    /// <param name="lastDateColumnName">��� ���� ���� "����", ��������� ����� ���������</param>
    public DateRangeCrossCommonFilter(string firstDateColumnName, string lastDateColumnName)
      : base(firstDateColumnName, lastDateColumnName)
    {
      DisplayName = "������";
    }

    #endregion

    #region ������� ���������

    /// <summary>
    /// ������� �������� �������. ��������� ���� ��������� ��� null, ���� ������ �� ���������� ��� ����� ������������ ��������
    /// </summary>
    public DateTime? FirstDate
    {
      get { return _FirstDate; }
      set
      {
        if (value == _FirstDate)
          return;
        _FirstDate = value;
        OnChanged();
      }
    }
    private DateTime? _FirstDate;

    /// <summary>
    /// ������� �������� �������. �������� ���� ��������� ��� null, ���� ������ �� ���������� ��� ����� ������������ ��������
    /// </summary>
    public DateTime? LastDate
    {
      get { return _LastDate; }
      set
      {
        if (value == _LastDate)
          return;
        _LastDate = value;
        OnChanged();
      }
    }
    private DateTime? _LastDate;

    #endregion

    #region ���������������� ������ � ��������

    /// <summary>
    /// ������� �������
    /// </summary>
    public override void Clear()
    {
      FirstDate = null;
      LastDate = null;
    }

    /// <summary>
    /// ���������� true, ���� ������ �� ����������
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return !(_FirstDate.HasValue || _LastDate.HasValue);
      }
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      return new DateRangeCrossFilter(ColumnName1, ColumnName2, FirstDate, LastDate);
    }

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// </summary>
    /// <param name="rowValues">�������� �����</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      Nullable<DateTime> dt1 = DataTools.GetNullableDateTime(rowValues.GetValue(ColumnName1));
      Nullable<DateTime> dt2 = DataTools.GetNullableDateTime(rowValues.GetValue(ColumnName2));
      return DataTools.DateRangeCrossed(FirstDate, LastDate, dt1, dt2);
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void ReadConfig(CfgPart config)
    {
      FirstDate = config.GetNullableDate("FirstDate");
      LastDate = config.GetNullableDate("LastDate");
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetNullableDate("FirstDate", FirstDate);
      config.SetNullableDate("LastDate", LastDate);
    }

    #endregion

    #region �������� ��������

    /// <summary>
    /// �������� �������� ��� ������� ������.
    /// ���� ������ �� ���������� (IsEmpty=true), ������������ true
    /// </summary>
    /// <param name="rowValue1">����������� �������� ������� ����</param>
    /// <param name="rowValue2">����������� �������� ������� ����</param>
    /// <returns>true, ���� �������� �������� ������� �������</returns>
    public bool TestValue(DateTime? rowValue1, DateTime? rowValue2)
    {
      return DataTools.DateRangeCrossed(FirstDate, LastDate, rowValue1, rowValue2);
    }

    #endregion
  }

  /// <summary>
  /// ������ �� ��������� ���� � ��������.
  /// � ������� ������ ���� ��� �������� ����, �������� ������ � ��������� ���.
  /// ������ �������� ������, ���� �������� � ������� ��� (Value) �������� � ��������.
  /// �������������� �������� ���� NULL, �������� �������� ���������
  /// </summary>
  public class YearRangeInclusionCommonFilter : TwoColumnsCommonFilter
  {
    #region ������������

    /// <summary>
    /// ����������� ��� �������� �����
    /// </summary>
    /// <param name="firstYearFieldName">��� ��������� ����, ����������� ��������� ��� ���������</param>
    /// <param name="lastYearFieldName">��� ��������� ����, ����������� �������� ��� ���������</param>
    public YearRangeInclusionCommonFilter(string firstYearFieldName, string lastYearFieldName)
      : base(firstYearFieldName, lastYearFieldName)
    {
      DisplayName = "������";
    }

    #endregion

    #region ������� ���������

    /// <summary>
    /// ��������� ���, ���� ������ ����������. 0, ���� ������ �� �����
    /// </summary>
    public int Value
    {
      get { return _Value; }
      set
      {
        if (value == _Value)
          return;
        _Value = value;
        OnChanged();
      }
    }
    private int _Value;

    #endregion

    #region ���������������� ������ � ��������

    /// <summary>
    /// ������� �������
    /// </summary>
    public override void Clear()
    {
      Value = 0;
    }

    /// <summary>
    /// ���������� true, ���� ������ �� ����������
    /// </summary>
    public override bool IsEmpty
    {
      get
      {
        return Value == 0;
      }
    }

    /// <summary>
    /// ��������� ������� ��� ���������� ����� ������� ������
    /// </summary>
    public override DBxFilter GetSqlFilter()
    {
      if (Value == 0)
        return null;

      throw new NotImplementedException();
      // TODO: return DBxFilter.CreateRangeOverYearFilter(FirstYearFieldName, LastYearFieldName, Value);
    }

    /// <summary>
    /// ���������������� ������������ ������� ������ �� ���������� ��������
    /// </summary>
    /// <param name="rowValues">�������� �����.</param>
    /// <returns>True, ���� ������ �������� ������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess rowValues)
    {
      int Year1 = DataTools.GetInt(rowValues.GetValue(ColumnName1));
      int Year2 = DataTools.GetInt(rowValues.GetValue(ColumnName2));

      if (Year1 > 0 && Value < Year1)
        return false;
      if (Year2 > 0 && Value > Year2)
        return false;
      return true;
    }

    /// <summary>
    /// ��������� �������� ������� �� ������ ������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void ReadConfig(CfgPart config)
    {
      Value = config.GetInt("Value");
    }

    /// <summary>
    /// �������� ��������� ������� � XML-������������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetInt("Value", Value);
    }

    #endregion

    #region �������� ��������

    /// <summary>
    /// �������� �������� ��� ������� ������.
    /// ���� ������ �� ���������� (IsEmpty=true), ������������ true
    /// </summary>
    /// <param name="rowValue1">����������� �������� ������� ����</param>
    /// <param name="rowValue2">����������� �������� ������� ����</param>
    /// <returns>true, ���� �������� �������� ������� �������</returns>
    public bool TestValue(int rowValue1, int rowValue2)
    {
      if (IsEmpty)
        return true;
      if (rowValue1 > 0 && Value < rowValue1)
        return false;
      if (rowValue2 > 0 && Value > rowValue2)
        return false;
      return true;
    }

    #endregion
  }

  #endregion

  /// <summary>
  /// ��������� ������, ������� ����� ������ �� ������ ��� �� ���������� �� ����� ������
  /// </summary>
  public class DummyCommonFilter : DBxCommonFilter
  {
    #region �����������

    /// <summary>
    /// ������� ������.
    /// �������� IsTrue ����� �������� true
    /// </summary>
    /// <param name="code">��� ��� �������</param>
    public DummyCommonFilter(string code)
    {
      base.Code = code;
      _IsTrue = true;
    }

    #endregion

    #region ������� ��������

    /// <summary>
    /// ���� true, �� ������ �� ����������.
    /// ���� false, �� ������ �� ���������� �� ����� ������.
    /// �� ��������� - true.
    /// </summary>
    public bool IsTrue
    {
      get { return _IsTrue; }
      set
      {
        if (value == _IsTrue)
          return;
        _IsTrue = value;
        OnChanged();
      }
    }
    private bool _IsTrue;

    #endregion

    #region ���������������� ������ � ��������

    /// <summary>
    /// ������������� IsTrue=true
    /// </summary>
    public override void Clear()
    {
      IsTrue = true;
    }

    /// <summary>
    /// ���������� IsTrue
    /// </summary>
    public override bool IsEmpty
    {
      get { return IsTrue; }
    }

    /// <summary>
    /// ������ �� ������
    /// </summary>
    /// <param name="list">������������</param>
    public override void GetColumnNames(DBxColumnList list)
    {
    }

    /// <summary>
    /// ���������� DummyFilter.AlwaysFalse, ���� ������ ����������
    /// </summary>
    /// <returns>��������� ������ ��� null</returns>
    public override DBxFilter GetSqlFilter()
    {
      if (IsTrue)
        return null;
      else
        return DummyFilter.AlwaysFalse;
    }

    /// <summary>
    /// ���������� IsTrue
    /// </summary>
    /// <param name="RowValues">������������</param>
    /// <returns>�������� �������� IsTrue</returns>
    protected override bool OnTestValues(INamedValuesAccess RowValues)
    {
      return IsTrue;
    }

    /// <summary>
    /// ������ ��������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void WriteConfig(CfgPart config)
    {
      config.SetBool("Value", IsTrue);
    }

    /// <summary>
    /// ������ ��������
    /// </summary>
    /// <param name="Config">������ ������������</param>
    public override void ReadConfig(CfgPart Config)
    {
      IsTrue = Config.GetBool("Value");
    }

    #endregion
  }

  /// <summary>
  /// ������ � ������������� SQL-��������.
  /// � ���� �������� ������ ��������� ������� ��������, � ��� �����, ��������
  /// </summary>
  public class FixedSqlCommonFilter : DBxCommonFilter
  {
    #region �����������

    /// <summary>
    /// ������� ������.
    /// �������� IsTrue ����� �������� true
    /// </summary>
    /// <param name="code">��� ��� �������</param>
    /// <param name="filter">SQL-������. ����������� ������ ���� �����</param>
    public FixedSqlCommonFilter(string code, DBxFilter filter)
    {
      base.Code = code;

      if (filter == null)
        throw new ArgumentNullException("filter");

      _Filter = filter;
    }

    #endregion

    #region SQL-������

    /// <summary>
    /// SQL-������. �������� � ������������
    /// </summary>
    public DBxFilter Filter { get { return _Filter; } }
    private DBxFilter _Filter;

    #endregion

    #region ���������������� ������ � ��������

    /// <summary>
    /// ������ �� ������
    /// </summary>
    public override void Clear()
    {
    }

    /// <summary>
    /// ���������� false
    /// </summary>
    public override bool IsEmpty { get { return false; } }

    /// <summary>
    /// ���������� ������ ����� �� �������
    /// </summary>
    /// <param name="list">������������</param>
    public override void GetColumnNames(DBxColumnList list)
    {
      _Filter.GetColumnNames(list);
    }

    /// <summary>
    /// ���������� Filter.
    /// </summary>
    /// <returns>��������� ������ ��� null</returns>
    public override DBxFilter GetSqlFilter()
    {
      return _Filter;
    }

    /// <summary>
    /// ��������� �������� �� ������������ �������
    /// </summary>
    /// <param name="RowValues">������ � ��������� �����</param>
    /// <returns>����������� �������</returns>
    protected override bool OnTestValues(INamedValuesAccess RowValues)
    {
      return _Filter.TestFilter(RowValues);
    }

    /// <summary>
    /// ������ �� ������
    /// </summary>
    /// <param name="config">������ ������������</param>
    public override void WriteConfig(CfgPart config)
    {
    }

    /// <summary>
    /// ������ �� ������
    /// </summary>
    /// <param name="Config">������ ������������</param>
    public override void ReadConfig(CfgPart Config)
    {
    }

    #endregion
  }
}
