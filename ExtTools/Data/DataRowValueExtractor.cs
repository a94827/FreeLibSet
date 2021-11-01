using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2017, Ageyev A.V.
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

namespace FreeLibSet.Data
{
  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowIntExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowIntExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public int this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int32))
            _FieldIndex = p;
          else
            _FieldIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_FieldIndex >= 0)
        {
          object v = row[_FieldIndex];
          if (v is DBNull)
            return 0;
          else
            return (int)v;
        }
        else
          return DataTools.GetInt(row[~_FieldIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableIntExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowNullableIntExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// ���� ���� �� �������� �������� (DBNull), ������������ null
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ���� ��� null</returns>
    public int? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int32))
            _FieldIndex = p;
          else
            _FieldIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_FieldIndex >= 0)
        {
          object v = row[_FieldIndex];
          if (v is DBNull)
            return null;
          else
            return (int)v;
        }
        else
        {
          object v = row[~_FieldIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetInt(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowInt64Extractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowInt64Extractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public long this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int64))
            _FieldIndex = p;
          else
            _FieldIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_FieldIndex >= 0)
        {
          object v = row[_FieldIndex];
          if (v is DBNull)
            return 0L;
          else
            return (Int64)v;
        }
        else
          return DataTools.GetInt64(row[~_FieldIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableInt64Extractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowNullableInt64Extractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// ���� ���� �� �������� �������� (DBNull), ������������ null
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ���� ��� null</returns>
    public long? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int64))
            _FieldIndex = p;
          else
            _FieldIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_FieldIndex >= 0)
        {
          object v = row[_FieldIndex];
          if (v is DBNull)
            return null;
          else
            return (long)v;
        }
        else
        {
          object v = row[~_FieldIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetInt64(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowSingleExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowSingleExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public float this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Single))
            _FieldIndex = p;
          else
            _FieldIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_FieldIndex >= 0)
        {
          object v = row[_FieldIndex];
          if (v is DBNull)
            return 0f;
          else
            return (float)v;
        }
        else
          return DataTools.GetSingle(row[~_FieldIndex]);
      }
    }

    #endregion
  }


  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableSingleExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowNullableSingleExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// ���� ���� �� �������� �������� (DBNull), ������������ null
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ���� ��� null</returns>
    public float? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int64))
            _FieldIndex = p;
          else
            _FieldIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_FieldIndex >= 0)
        {
          object v = row[_FieldIndex];
          if (v is DBNull)
            return null;
          else
            return (float)v;
        }
        else
        {
          object v = row[~_FieldIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetSingle(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowDoubleExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowDoubleExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public double this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Double))
            _FieldIndex = p;
          else
            _FieldIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_FieldIndex >= 0)
        {
          object v = row[_FieldIndex];
          if (v is DBNull)
            return 0.0;
          else
            return (double)v;
        }
        else
          return DataTools.GetDouble(row[~_FieldIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableDoubleExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowNullableDoubleExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// ���� ���� �� �������� �������� (DBNull), ������������ null
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ���� ��� null</returns>
    public double? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Double))
            _FieldIndex = p;
          else
            _FieldIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_FieldIndex >= 0)
        {
          object v = row[_FieldIndex];
          if (v is DBNull)
            return null;
          else
            return (double)v;
        }
        else
        {
          object v = row[~_FieldIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetDouble(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowDecimalExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowDecimalExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public decimal this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Decimal))
            _FieldIndex = p;
          else
            _FieldIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_FieldIndex >= 0)
        {
          object v = row[_FieldIndex];
          if (v is DBNull)
            return 0m;
          else
            return (decimal)v;
        }
        else
          return DataTools.GetDecimal(row[~_FieldIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableDecimalExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowNullableDecimalExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// ���� ���� �� �������� �������� (DBNull), ������������ null
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ���� ��� null</returns>
    public decimal? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Decimal))
            _FieldIndex = p;
          else
            _FieldIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_FieldIndex >= 0)
        {
          object v = row[_FieldIndex];
          if (v is DBNull)
            return null;
          else
            return (decimal)v;
        }
        else
        {
          object v = row[~_FieldIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetDecimal(v);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// DataRowNullableDateTimeExtractor ���������� �������� null, ���� ���� �������� ������ ��������,
  /// � DataRowDateTimeExtractor - DateTime.MinValue.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowDateTimeExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowDateTimeExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0).
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public DateTime this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int32))
            _FieldIndex = p;
          else
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �������� ������� \"" + _FieldName + "\" ������������� ���� " + row.Table.Columns[p].DataType.ToString());
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        object v = row[_FieldIndex];
        if (v is DBNull)
          return DateTime.MinValue;
        else
          return (DateTime)v;
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// DataRowNullableDateTimeExtractor ���������� �������� null, ���� ���� �������� ������ ��������,
  /// � DataRowDateTimeExtractor - DateTime.MinValue.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableDateTimeExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowNullableDateTimeExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0).
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public DateTime? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(DateTime)) // 28.11.2017
            _FieldIndex = p;
          else
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �������� ������� \"" + _FieldName + "\" ������������� ���� " + row.Table.Columns[p].DataType.ToString());
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        object v = row[_FieldIndex];
        if (v is DBNull)
          return null;
        else
          return (DateTime)v;
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowTimeSpanExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowTimeSpanExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0).
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public TimeSpan this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int32))
            _FieldIndex = p;
          else
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �������� ������� \"" + _FieldName + "\" ������������� ���� " + row.Table.Columns[p].DataType.ToString());
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        object v = row[_FieldIndex];
        if (v is DBNull)
          return TimeSpan.Zero;
        else
          return (TimeSpan)v;
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableTimeSpanExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowNullableTimeSpanExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0).
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public TimeSpan? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Int32))
            _FieldIndex = p;
          else
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �������� ������� \"" + _FieldName + "\" ������������� ���� " + row.Table.Columns[p].DataType.ToString());
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        object v = row[_FieldIndex];
        if (v is DBNull)
          return null;
        else
          return (TimeSpan)v;
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowStringExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowStringExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public string this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(String))
            _FieldIndex = p;
          else
            _FieldIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_FieldIndex >= 0)
        {
          object v = row[_FieldIndex];
          if (v is DBNull)
            return String.Empty;
          else
            return ((string)v).Trim();
        }
        else
          return DataTools.GetString(row[~_FieldIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowBoolExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowBoolExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ����</returns>
    public bool this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Boolean))
            _FieldIndex = p;
          else
            _FieldIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_FieldIndex >= 0)
        {
          object v = row[_FieldIndex];
          if (v is DBNull)
            return false;
          else
            return (bool)v;
        }
        else
          return DataTools.GetBool(row[~_FieldIndex]);
      }
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������� ���� � �������� ������ �� ����� DataRow.
  /// ������������� ��� �������������, ����� � ����� ��������� ��������� �������� ������ ���� �� ����� 
  /// �� ��������� �����, ��� ���� ������ ����� ���������� � ������ ��������.
  /// ��� ���������� ���������� �������� �����������, ��������� �� ������ DataRow � ��� �� DataTable, ���
  /// � ��� ���������� ������. ���� ���, �� ������������ ������ ������� DataColumn � �������. 
  /// ��� ����������� ������� ������������ ������ �� ������� ����, � �� �� �����, ��� ����������� ��������
  /// ���������� ��������.
  /// �� ������������� ��� ������ � ���������� �������� � RowState=Deleted ��� Detached.
  /// ��� �������, ��������� ������������ ��� ��������� ���������� � �������� ������ ������.
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DataRowNullableBoolExtractor
  {
    #region �����������

    /// <summary>
    /// �������������� ���������.
    /// </summary>
    /// <param name="fieldName">��� ����</param>
    public DataRowNullableBoolExtractor(string fieldName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fieldName))
        throw new ArgumentNullException("fieldName");
#endif

      _FieldName = fieldName;
      // ��������� ����������� ��� ������������ ���������
      _CurrentTable = null;
      _FieldIndex = 0;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����, �� �������� ����������� ��������
    /// </summary>
    public string FieldName { get { return _FieldName; } }
    private string _FieldName;

    /// <summary>
    /// ���������� ��� ����.
    /// </summary>
    /// <returns>�������� FieldName</returns>
    public override string ToString()
    {
      return _FieldName;
    }

    /// <summary>
    /// �������, �� ������� �������� ��� ���� ��������� ��������.
    /// �� ������� ���������� �������� null.
    /// </summary>
    private DataTable _CurrentTable;

    /// <summary>
    /// ������ ���� (������� ��� ������ 0), ���� ��� ���������� �������� ������������ ������ ������ � ����,
    /// ����� ���� ����� ���������� ���.
    /// ���� ��� ���������� �������� ��������� ��������������, �� �������� ������������� ��������, 
    /// ������� �������� �������� ����������� (�������� "~") ������� ����.
    /// ��������, ���� ���� ����� ������ 1, �� ��� �������� ���������� ���� �������� �������� 0x00000001,
    /// � ��� ���������� � ��������������� - 0xFFFFFFFE.
    /// ����� �����, ������ ����� CurrentTable �� null.
    /// </summary>
    private int _FieldIndex;

    #endregion

    #region ���������� ��������

    /// <summary>
    /// ���������� �������� ���� �� ������.
    /// ���� ���� �� �������� �������� (DBNull), ������������ null
    /// </summary>
    /// <param name="row">������ �������</param>
    /// <returns>�������� ���� ��� null</returns>
    public bool? this[DataRow row]
    {
      get
      {
        if (!Object.ReferenceEquals(row.Table, _CurrentTable))
        {
          int p = row.Table.Columns.IndexOf(_FieldName);
          if (p < 0)
            throw new ArgumentException("������� \"" + row.Table.TableName + "\" �� �������� ������� \"" + _FieldName + "\"");
          if (row.Table.Columns[p].DataType == typeof(Boolean))
            _FieldIndex = p;
          else
            _FieldIndex = ~p;
          _CurrentTable = row.Table; // ����������� � ��������� �������
        }

        if (_FieldIndex >= 0)
        {
          object v = row[_FieldIndex];
          if (v is DBNull)
            return null;
          else
            return (bool)v;
        }
        else
        {
          object v = row[~_FieldIndex];
          if (v is DBNull)
            return null;
          else
            return DataTools.GetBool(v);
        }
      }
    }

    #endregion
  }
}