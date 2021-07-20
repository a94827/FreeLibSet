using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;

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

namespace AgeyevAV.ExtDB
{
  #region ������������ DBxDataWriterMode

  /// <summary>
  /// ����� ������ DBxDataWriter
  /// </summary>
  public enum DBxDataWriterMode
  {
    /// <summary>
    /// ���������� �����
    /// </summary>
    Insert,

    /// <summary>
    /// ����������� ������������ �����
    /// </summary>
    Update,

    /// <summary>
    /// ���������� ����������� ����� � ���������� ������������
    /// </summary>
    InsertOrUpdate
  }

  #endregion

  /// <summary>
  /// ����� ���������� ��� ������ � DBxDataWriter.
  /// ������ ��������� � ����������� � ���������������� ����. ����� ���������� ����� DBxConBase.CreateWriter()
  /// </summary>
  [Serializable]
  public sealed class DBxDataWriterInfo : IReadOnlyObject, ICloneable
  {
    #region �����������

    /// <summary>
    /// ������� �������������������� ����� ����������
    /// </summary>
    public DBxDataWriterInfo()
    {
      _Mode = DBxDataWriterMode.Insert;
    }

    #endregion

    #region �������� ��������

    /// <summary>
    /// ��� ������� � ���� ������.
    /// �������� ����������� ������ ���� �����������.
    /// </summary>
    public string TableName
    {
      get { return _TableName; }
      set
      {
        CheckNotReadOnly();
        _TableName = value;
      }
    }
    private string _TableName;

    /// <summary>
    /// ������ ��������, ������� ����� �������������� � �������.
    /// �������� ����������� ������ ���� �����������.
    /// ���� ������ ������� ��� ������� ���������� �����, ���� ����� ���������� �� Insert.
    /// � ������� Update � InsertOrUpdate() ������ ���� ����� ���� �� ���� �������, �� �������� � ��������� ���� ��� � ������ SearchColumns, ���� �� �����.
    /// </summary>
    public DBxColumns Columns
    {
      get { return _Columns; }
      set
      {
        CheckNotReadOnly();
        _Columns = value;
      }
    }
    private DBxColumns _Columns;


    /// <summary>
    /// ������ ��������, �� ������� ����� ����������� �����.
    /// ���� �������� �� ����������� � ����� ����, ����� ������������ ������� �� �������� DBxTableStruct.PrimaryKey.
    /// </summary>
    public DBxColumns SearchColumns
    {
      get { return _SearchColumns; }
      set
      {
        CheckNotReadOnly();
        _SearchColumns = value;
      }
    }
    private DBxColumns _SearchColumns;

    /// <summary>
    /// ����� ������.
    /// �� ��������� - Insert
    /// </summary>
    public DBxDataWriterMode Mode
    {
      get { return _Mode; }
      set
      {
        CheckNotReadOnly();
        _Mode = value;
      }
    }
    private DBxDataWriterMode _Mode;

    #endregion

    #region �������������� ��������

    /// <summary>
    /// ��������� ���������� �����, ������� ��������� ����������.
    /// �� ��������� - 0 - ����������.
    /// �������� ����� ���������������� ��������. �������� ���������� �����, ������� ����� ����������, ����� ����������
    /// 
    /// ��������� �������� ����� �������� � ��������� ��������� ������.
    /// ��������, ���� ��������� ���������� ����� �������� ���������� �����, ����� ���� ������� ������� ��������� ���� ������.
    /// � ����������� ������� ���� �������� ������������.
    /// </summary>
    public long ExpectedRowCount
    {
      get { return _ExpectedRowCount; }
      set
      {
        CheckNotReadOnly();
        if (value < 0L)
          throw new ArgumentOutOfRangeException();
        _ExpectedRowCount = value;
      }
    }
    private long _ExpectedRowCount;

    /// <summary>
    /// ���� ������ ������������� ��������, �� ����� ��������� ���������� �������� ����� ����������� COMMINT TRANSACION,
    /// � ����� ���������� ����� ����������.
    /// �� ��������� - 0 - ������������� ������������� ���������� �� �����������.
    /// �������� ��������, ������ ���� ����������� DBxDataWriter �������� ����������. ���� �� ������ ������ DBxConBase.CreateWrite().
    /// 
    /// ��� SQLite ���� ���������� ������ ������, ������� ����� ����������. ���� ������ ��������, �� ��������� ������,
    /// ���������� � ������������ Windows. ��� ���������� ������ ��� ����� ������� ���������� �������.
    /// ������������� ������ ���������� ������� ��������, ��������, 100000, ���� �������� ���������.
    /// ��������� ���������� �������� (������ 1000) ������� �������� ������.
    /// </summary>
    public int TransactionPulseRowCount
    {
      get { return _TransactionPulseRowCount; }
      set
      {
        CheckNotReadOnly();
        if (value < 0)
          throw new ArgumentOutOfRangeException();
        _TransactionPulseRowCount = value;
      }
    }
    private int _TransactionPulseRowCount;

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// ���������� true, ���� ����� ���������� ��������� � ����� "������ ������"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// �������� IsReadOnly=false
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// ��������� ����� ���������� � ����� "������ ������"
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// ������� ����� ������ ����������, ������� ����� �������������
    /// </summary>
    public DBxDataWriterInfo Clone()
    {
      DBxDataWriterInfo res = new DBxDataWriterInfo();
      res.TableName = TableName;
      res.Columns = Columns;
      res.Mode = Mode;
      res.ExpectedRowCount = ExpectedRowCount;
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }

  #region ������������ DBxDataWriterState

  /// <summary>
  /// ������� ��������� ������� DBxDataWriter
  /// </summary>
  [Serializable]
  public enum DBxDataWriterState
  {
    /// <summary>
    /// ������ ������ � ����� � ���������� �����.
    /// ������ Write() � LoadFrom() ��� �� ����������
    /// </summary>
    Created,

    /// <summary>
    /// ���� ������ Write() ��� LoadFrom(), ����� Finish() ��� �� ���������
    /// </summary>
    Writing,

    /// <summary>
    /// ��� ����� ������ Finish()
    /// </summary>
    Finished,

    /// <summary>
    /// ��� ������ Write() ��� Finish() ��������� ������.
    /// ����� ������ �������, ����� Dispose(), ��������
    /// </summary>
    Error,

    /// <summary>
    /// ��� ����� ������ Dispose()
    /// </summary>
    Disposed
  }

  #endregion

  /// <summary>
  /// ������ ��� ���������� �/��� ��������� ����� � �������.
  /// ��������� ������� DBxConBase.CreateWriter().
  /// ���������� ��� ������ ��������� ����� ������ � �������� ����� Write() ��� ������ ������.
  /// ����� ����� ������������ ����� LoadFrom() ��� ���������� ����������.
  /// �� ��������� ������ ������ ���� ������� ������ Finish() � Dispose().
  /// ���� ������ ������� Write() � Finish() �� ������� ����������, ������ ������ ��������� �������.
  /// </summary>
  public abstract class DBxDataWriter : MarshalByRefDisposableObject
  {
    #region ����������� � Dispose

    /// <summary>
    /// ������������� ������� � ��������� Created.
    /// ���� ��� ������� ��������� (DBxConBase.CurrentTransaction=null), �� �������� ��������� ������� DBxConBase.TransactionBegin().
    /// ���� � ���������� ��� ������ ����������, �� DBxDataWriter �� ����� ������������ ����������.
    /// </summary>
    /// <param name="con"></param>
    /// <param name="writerInfo"></param>
    public DBxDataWriter(DBxConBase con, DBxDataWriterInfo writerInfo)
    {
      // �������� ���������� ��������� � DBxConBase.CreateWriter()

      _Con = con;
      _WriterInfo = writerInfo;
      _State = DBxDataWriterState.Created;
      _Values = new object[writerInfo.Columns.Count];
      _ColumnNameIndexer = new StringArrayIndexer(writerInfo.Columns.AsArray, false);

      _TableStruct = con.DB.Struct.Tables[writerInfo.TableName];
#if DEBUG
      if (_TableStruct == null)
        throw new ArgumentException("�� ������� ��������� ������� \"" + writerInfo.TableName + "\"");
#endif

      _ColumnDefs = new DBxColumnStruct[writerInfo.Columns.Count];
      for (int i = 0; i < writerInfo.Columns.Count; i++)
      {
        _ColumnDefs[i] = _TableStruct.Columns[writerInfo.Columns[i]];
        if (_ColumnDefs[i] == null)
          throw new ArgumentException("�� ������ ������� \"" + writerInfo.Columns[i] + "\" � ������� \"" + writerInfo.TableName + "\"");
      }


      if (writerInfo.SearchColumns == null)
      {
        if (writerInfo.Columns.ContainsAny(_TableStruct.PrimaryKey))
          _SearchColumns = _TableStruct.PrimaryKey;
        else
          _SearchColumns = DBxColumns.Empty;
      }
      else
        _SearchColumns = writerInfo.SearchColumns; // 02.08.2020

      if (_SearchColumns.Count > 0)
      {
        if (writerInfo.Columns.ContainsAny(_SearchColumns) &&
          (!writerInfo.Columns.Contains(_SearchColumns)))
          throw new ArgumentException("������ ���� �������� �� ����� ��������� ������ ����� �������� ���������� ����� ��� ������ (" + _SearchColumns.ToString() + ") � ������� \"" + _TableStruct.TableName + "\"", "writerInfo");
      }
      else // SearchColumns.IsEmpty
      {
        switch (writerInfo.Mode)
        {
          case DBxDataWriterMode.Update:
          case DBxDataWriterMode.InsertOrUpdate:
            throw new ArgumentException("������� \"" + writerInfo.TableName + "\" �� �������� ���������� �����. ��������� ���� ��������� ��� ������ " + writerInfo.Mode.ToString(), "writerInfo");
        }
      }


      _SearchColumnPositions = new int[_SearchColumns.Count];
      for (int i = 0; i < _SearchColumns.Count; i++)
        _SearchColumnPositions[i] = _ColumnNameIndexer.IndexOf(SearchColumns[i]);

      _OtherColumns = writerInfo.Columns - _SearchColumns;

      _OtherColumnPositions = new int[_OtherColumns.Count];
      for (int i = 0; i < _OtherColumns.Count; i++)
        _OtherColumnPositions[i] = _ColumnNameIndexer.IndexOf(OtherColumns[i]);

      switch (writerInfo.Mode)
      {
        case DBxDataWriterMode.Update:
        case DBxDataWriterMode.InsertOrUpdate:
          if (_SearchColumns.Count == 0)
            throw new ArgumentException("� ������ " + writerInfo.Mode.ToString() + " ������ ���� ����� ������ �������� ��� ������", "writerInfo");
          if (_OtherColumns.Count == 0)
            throw new ArgumentException("� ������ " + writerInfo.Mode.ToString() + " ������ �������� ������ �������� � ���� ���� �� ���� �������, �� ���������� �������� ��� ������ � ������� \"" + _TableStruct.TableName + "\"", "writerInfo");
          break;
      }

      if (con.CurrentTransaction == null)
      {
        con.TransactionBegin();
        _TransactionStarted = true;
      }
      _PulseCounter = writerInfo.TransactionPulseRowCount;
    }

    /// <summary>
    /// ��������� ������ � ��������� State=Disposed.
    /// ��� ���������� ������, ����� ������� ������ Dispose() ������ ���������� ����� Finish().
    /// ���� ����� Finish() �� ���������, ��� ��� �� ������� ��������� ����������, ���������� DBxConBase.TransactionRollback(),
    /// ���� ���������� ���� ������ � ������������ �������
    /// </summary>
    /// <param name="disposing">true, ���� ��� ������ ����� Dispose(), � �� ���������� �������</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_TransactionStarted)
        {
          _Con.TransactionRollback();
          _TransactionStarted = false;
        }
      }

      _State = DBxDataWriterState.Disposed;
      _Values = null;
      base.Dispose(disposing);

    }

    #endregion

    #region �������� ��������

    /// <summary>
    /// ���������� � ����� ������, ��� �������� ��� ������ ����� CreateCon().
    /// �������� ��������� ����������, ����� ��� ������ �� ���� �� ���� ����������� ������� � �������.
    /// </summary>
    protected DBxConBase Con { get { return _Con; } }
    private readonly DBxConBase _Con;

    /// <summary>
    /// ����������� ���������.
    /// �������� ����� ������� ������ ������.
    /// </summary>
    public DBxDataWriterInfo WriterInfo { get { return _WriterInfo; } }
    private readonly DBxDataWriterInfo _WriterInfo;

    /// <summary>
    /// ������� ��������� �������
    /// </summary>
    public DBxDataWriterState State { get { return _State; } }
    private DBxDataWriterState _State;

    bool _TransactionStarted;

    #endregion

    #region ����� ����������� ������

    /// <summary>
    /// ����� ������.
    /// ����� ������� ������������� ����� � DBxDataWriterInfo.Columns.
    /// ������ ������������ ��������������� �������� ������� DBxDataWriter ��� ������ �������� �� ������
    /// </summary>
    public object[] Values
    {
      get { return _Values; }
      set
      {
        CheckStateWriteable();
        if (value == null)
          throw new ArgumentNullException();
        if (value.Length != _Values.Length)
          throw new ArgumentException("������������ ����� �������");
        _Values = value;
      }
    }
    private object[] _Values;

    /// <summary>
    /// ������ � ���� ������ ������ �� ������� ���� � ������ DBxDataWriterInfo.Columns.
    /// </summary>
    /// <param name="columnIndex">������ ������� � ������</param>
    /// <returns>�������� ����</returns>
    public object this[int columnIndex]
    {
      get { return _Values[columnIndex]; }
      set { _Values[columnIndex] = value; }
    }

    /// <summary>
    /// ������ � ������ ������ �� ����� ���� � ������ DBxDataWriterInfo.Columns.
    /// </summary>
    /// <param name="columnName">��� ����</param>
    /// <returns>�������� ����</returns>
    public object this[string columnName]
    {
      get { return _Values[GetColumnIndex(columnName)]; }
      set { _Values[GetColumnIndex(columnName)] = value; }
    }

    private readonly StringArrayIndexer _ColumnNameIndexer;

    private int GetColumnIndex(string columnName)
    {
      int p = _ColumnNameIndexer.IndexOf(columnName);
      if (p >= 0)
        return p;

      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
      else
        throw new ArgumentException("����������� ��� ������� \"" + columnName + "\"", "columnName");
    }

    #endregion

    #region ��������������� ������

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public string GetString(string columnName) { return DataTools.GetString(this[columnName]); }

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public string GetString(int columnIndex) { return DataTools.GetString(this[columnIndex]); }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetString(string columnName, string value)
    {
      SetString(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetString(int columnIndex, string value)
    {
      if (value == null)
        value = String.Empty;

      if (value.Length == 0 && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Int:
            _Values[columnIndex] = StdConvert.ToInt64(value);
            break;
          case DBxColumnType.Float:
            _Values[columnIndex] = StdConvert.ToDouble(value);
            break;
          case DBxColumnType.Money:
            _Values[columnIndex] = StdConvert.ToDecimal(value);
            break;
          case DBxColumnType.Boolean:
            _Values[columnIndex] = StdConvert.ToInt32(value) != 0;
            break;
          case DBxColumnType.String:
            _Values[columnIndex] = value;
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Int32));
        }
      }
    }


    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public int GetInt(string columnName) { return DataTools.GetInt(this[columnName]); }

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public int GetInt(int columnIndex) { return DataTools.GetInt(this[columnIndex]); }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetInt(string columnName, int value)
    {
      SetInt(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetInt(int columnIndex, int value)
    {
      if (value == 0 && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Int:
          case DBxColumnType.Float:
          case DBxColumnType.Money:
            _Values[columnIndex] = value;
            break;
          case DBxColumnType.String:
            _Values[columnIndex] = StdConvert.ToString(value);
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Int32));
        }
      }
    }


    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public long GetInt64(string columnName) { return DataTools.GetInt64(this[columnName]); }

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public long GetInt64(int columnIndex) { return DataTools.GetInt64(this[columnIndex]); }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetInt64(string columnName, long value)
    {
      SetInt64(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetInt64(int columnIndex, long value)
    {
      if (value == 0L && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Int:
          case DBxColumnType.Float:
          case DBxColumnType.Money:
            _Values[columnIndex] = value;
            break;
          case DBxColumnType.String:
            _Values[columnIndex] = StdConvert.ToString(value);
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Int64));
        }
      }
    }


    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public float GetSingle(string columnName) { return DataTools.GetSingle(this[columnName]); }

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public float GetSingle(int columnIndex) { return DataTools.GetSingle(this[columnIndex]); }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetSingle(string columnName, float value)
    {
      SetSingle(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetSingle(int columnIndex, float value)
    {
      if (value == 0f && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Int:
          case DBxColumnType.Float:
          case DBxColumnType.Money:
            _Values[columnIndex] = value;
            break;
          case DBxColumnType.String:
            _Values[columnIndex] = StdConvert.ToString(value);
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Single));
        }
      }
    }


    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public double GetDouble(string columnName) { return DataTools.GetDouble(this[columnName]); }

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public double GetDouble(int columnIndex) { return DataTools.GetDouble(this[columnIndex]); }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetDouble(string columnName, double value)
    {
      SetDouble(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetDouble(int columnIndex, double value)
    {
      if (value == 0.0 && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Int:
          case DBxColumnType.Float:
          case DBxColumnType.Money:
            _Values[columnIndex] = value;
            break;
          case DBxColumnType.String:
            _Values[columnIndex] = StdConvert.ToString(value);
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Double));
        }
      }
    }


    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public decimal GetDecimal(string columnName) { return DataTools.GetDecimal(this[columnName]); }

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public decimal GetDecimal(int columnIndex) { return DataTools.GetDecimal(this[columnIndex]); }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetDecimal(string columnName, decimal value)
    {
      SetDecimal(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetDecimal(int columnIndex, decimal value)
    {
      if (value == 0 && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Int:
          case DBxColumnType.Float:
          case DBxColumnType.Money:
            _Values[columnIndex] = value;
            break;
          case DBxColumnType.String:
            _Values[columnIndex] = StdConvert.ToString(value);
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Decimal));
        }
      }
    }

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public bool GetBool(string columnName) { return DataTools.GetBool(this[columnName]); }

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public bool GetBool(int columnIndex) { return DataTools.GetBool(this[columnIndex]); }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetBool(string columnName, bool value)
    {
      SetBool(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetBool(int columnIndex, bool value)
    {
      if (value == false && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Boolean:
            _Values[columnIndex] = value;
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Decimal));
        }
      }
    }

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public DateTime GetDateTime(string columnName) { return DataTools.GetDateTime(this[columnName]); }

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public DateTime GetDateTime(int columnIndex) { return DataTools.GetDateTime(this[columnIndex]); }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetDateTime(string columnName, DateTime value)
    {
      SetDateTime(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetDateTime(int columnIndex, DateTime value)
    {
      switch (_ColumnDefs[columnIndex].ColumnType)
      {
        case DBxColumnType.Date:
        case DBxColumnType.DateTime:
          _Values[columnIndex] = value;
          break;
        default:
          throw CreateColumnTypeException(columnIndex, typeof(Decimal));
      }
    }

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public DateTime? GetNullableDateTime(string columnName) { return DataTools.GetNullableDateTime(this[columnName]); }

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public DateTime? GetNullableDateTime(int columnIndex) { return DataTools.GetNullableDateTime(this[columnIndex]); }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetNullableDateTime(string columnName, DateTime? value)
    {
      SetNullableDateTime(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetNullableDateTime(int columnIndex, DateTime? value)
    {
      if ((!value.HasValue) && (!_ColumnDefs[columnIndex].Nullable))
        throw new InvalidOperationException("������� \"" + _ColumnDefs[columnIndex] + "\" ������� \"" + _TableStruct.TableName + "\" �� ��������� �������� null");
      // ���� ���� ��� ������� ���������� �������� DEFAULT, �� ��� ������ ��������� � ������ VALUES()

      switch (_ColumnDefs[columnIndex].ColumnType)
      {
        case DBxColumnType.Date:
        case DBxColumnType.DateTime:
          _Values[columnIndex] = value;
          break;
        default:
          throw CreateColumnTypeException(columnIndex, typeof(Decimal));
      }
    }

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public Guid GetGuid(string columnName) { return DataTools.GetGuid(this[columnName]); }

    /// <summary>
    /// �������� ����� ���������� �������� �� ������ ����������� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <returns>�������� � ������</returns>
    public Guid GetGuid(int columnIndex) { return DataTools.GetGuid(this[columnIndex]); }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnName">��� ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetGuid(string columnName, Guid value)
    {
      SetGuid(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// �������� �������� � ����� ������.
    /// </summary>
    /// <param name="columnIndex">������ ���� �� ������ DBxDataWriterInfo.Columns</param>
    /// <param name="value">��������������� ��������</param>
    public void SetGuid(int columnIndex, Guid value)
    {
      if (value == Guid.Empty && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Guid:
            _Values[columnIndex] = value;
            break;
          case DBxColumnType.String:
            _Values[columnIndex] = value.ToString();
            break;
          case DBxColumnType.Binary:
            _Values[columnIndex] = value.ToByteArray();
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Decimal));
        }
      }
    }

    private Exception CreateColumnTypeException(int columnIndex, Type type)
    {
      return new Exception("������� \"" + _ColumnDefs[columnIndex] + "\" ������� \"" + _TableStruct.TableName + "\" ����� ��� " +
        _ColumnDefs[columnIndex].ColumnType.ToString() + " � �� ����� ��������� �������� ���� " + type.ToString());
    }

    #endregion

    #region ������� �������� ��� ������ � ������ ��������

    /// <summary>
    /// �������� ��������� �������
    /// </summary>
    public DBxTableStruct TableStruct { get { return _TableStruct; } }
    private readonly DBxTableStruct _TableStruct;


    /// <summary>
    /// �������� �������� �������, ��������������� DBxDataWriterInfo.Columns
    /// </summary>
    public DBxColumnStruct[] ColumnDefs { get { return _ColumnDefs; } }
    private readonly DBxColumnStruct[] _ColumnDefs;

    /// <summary>
    /// ����� �������� ���������� ����� ��� �������� � �������� DBxDataWriterInfo.SearchColumns
    /// </summary>
    public DBxColumns SearchColumns { get { return _SearchColumns; } }
    private readonly DBxColumns _SearchColumns;

    /// <summary>
    /// ������� �������� ���������� ����� � ������� Values
    /// </summary>
    public int[] SearchColumnPositions { get { return _SearchColumnPositions; } }
    private readonly int[] _SearchColumnPositions;

    /// <summary>
    /// ����� ��������, �� �������� � ��������� ����
    /// </summary>
    public DBxColumns OtherColumns { get { return _OtherColumns; } }
    private readonly DBxColumns _OtherColumns;

    /// <summary>
    /// ������� ������ �������� � ������� Values
    /// </summary>
    public int[] OtherColumnPositions { get { return _OtherColumnPositions; } }
    private readonly int[] _OtherColumnPositions;

    #endregion

    #region �������� ������, ���������� �� ����������� ����

    /// <summary>
    /// ������� ��� ������ PulseTransaction()
    /// </summary>
    private int _PulseCounter;

    /// <summary>
    /// ��������� ����� ������.
    /// �� ������ ������ ������ ���� �������� ����� ������
    /// </summary>
    public void Write()
    {
      CheckStateWriteable();
      try
      {
        OnWrite();
      }
      catch
      {
        _State = DBxDataWriterState.Error;
        throw;
      }

      for (int i = 0; i < _Values.Length; i++)
        _Values[i] = null;

      if (_WriterInfo.TransactionPulseRowCount > 0)
      {
        _PulseCounter--;
        if (_PulseCounter < 0)
        {
          PulseTransaction();
        }
      }
    }

    /// <summary>
    /// ���������� ������.
    /// ���� ������������ �����-���� ���������� ������, ��� ����� ���������.
    /// ����� ����� ��������� ����������
    /// � ������ ������, ����� ��������� ������ � ��������� Finished.
    /// </summary>
    public void Finish()
    {
      CheckStateWriteable();
      try
      {
        OnFinish();

        if (_TransactionStarted)
        {
          _Con.TransactionCommit();
          _TransactionStarted = false;
        }


        _State = DBxDataWriterState.Finished;
      }
      catch
      {
        _State = DBxDataWriterState.Error;
        throw;
      }
    }

    #endregion

    #region ���������� ������

    private void CheckStateWriteable()
    {
      switch (_State)
      {
        case DBxDataWriterState.Created:
        case DBxDataWriterState.Writing:
          break;
        default:
          throw new InvalidOperationException("DBxDataWriter ��������� � ��������� " + _State.ToString());
      }
    }

    /// <summary>
    /// ������ ��������� ���������� ������ �� ������
    /// </summary>
    protected abstract void OnWrite();

    /// <summary>
    /// ������ �������� ���������� �������������� ������, ���� ����� ����.
    /// ���� ����� ����� ���������� �� PulseTransaction()
    /// </summary>
    protected virtual void OnFinish()
    {
    }

    /// <summary>
    /// ������� ������ ������� ������, ���������� ������� ColumnDefs
    /// </summary>
    /// <returns>����� ������� DataTable</returns>
    protected DataTable CreateDataTable()
    {
      DataTable table = new DataTable(WriterInfo.TableName);
      for (int i = 0; i < ColumnDefs.Length; i++)
        table.Columns.Add(ColumnDefs[i].CreateDataColumn());
      return table;
    }


    #endregion

    #region �������������� ������

    /// <summary>
    /// �������� ������ �� �������
    /// </summary>
    /// <param name="table">�������, ������ ����� ����� ������</param>
    public virtual void LoadFrom(DataTable table)
    {
      if (table == null)
        throw new ArgumentNullException("table");

      if (table.Rows.Count == 0)
        return;

      // ���������� �������� ����� � �������
      int[] columnIndexes = new int[_Values.Length];
      for (int i = 0; i < columnIndexes.Length; i++)
      {
        columnIndexes[i] = table.Columns.IndexOf(_WriterInfo.Columns[i]);
        if (columnIndexes[i] < 0)
          throw new ArgumentException("������� \"" + table.TableName + "\" �� �������� ������� \"" + _WriterInfo.Columns[i] + "\"", "table");
      }

      foreach (DataRow row in table.Rows)
      {
        for (int i = 0; i < _Values.Length; i++)
          _Values[i] = row[columnIndexes[i]];
        Write();
      }
    }

    /// <summary>
    /// �������� ������ �� DbDataReader
    /// </summary>
    /// <param name="reader">��������, ������ ����� ����� ������</param>
    public virtual void LoadFrom(DbDataReader reader)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");

      // ���������� �������� ����� � �������
      int[] columnIndexes = new int[_Values.Length];
      for (int i = 0; i < columnIndexes.Length; i++)
      {
        columnIndexes[i] = reader.GetOrdinal(_WriterInfo.Columns[i]);
        if (columnIndexes[i] < 0)
          throw new ArgumentException("�������� ������ �� �������� ������� \"" + _WriterInfo.Columns[i] + "\"", "reader");
      }

      while (reader.Read())
      {
        for (int i = 0; i < _Values.Length; i++)
          _Values[i] = reader[columnIndexes[i]];
        Write();
      }
    }

    /// <summary>
    /// ��������� ������� ����������, ������� COMMIT_TRANSACTION, � �������� ����� ����������.
    /// ���� ����� ����� ���������� ������������� �� Write(), ���� ���� ����������� ��������
    /// DBxDataWriterInfo.TransactionPulseRowCount
    /// </summary>
    /// <returns></returns>
    public bool PulseTransaction()
    {
      // �������� ������� �����, �� ������, ���� ����� ������ �� ����������� ����, � �������� TransactionPulseRowCount
      // ����� �����������
      _PulseCounter = _WriterInfo.TransactionPulseRowCount;

      if (!_TransactionStarted)
        return false;

      OnFinish();

      OnPulseTransaction();

      return true;
    }

    /// <summary>
    /// ���������� �� PulseTransaction().
    /// ��������� DBxCon.TransactionCommit() � DBxCon.TransactionBegin()
    /// </summary>
    protected virtual void OnPulseTransaction()
    {
      _Con.TransactionCommit();
      _TransactionStarted = false; // �� ������ ������ ������ ����� ����������

      _Con.TransactionBegin();
      _TransactionStarted = true;
    }


    #endregion
  }

#if XXX
  /// <summary>
  /// �����-��������, ������� ������ ������ ��� ������
  /// </summary>
  internal class DBxDummyDataWriter : DBxDataWriter
  {
  #region �����������

    /// <summary>
    /// ������������� ������� � ��������� Created
    /// </summary>
    /// <param name="con"></param>
    /// <param name="writerInfo"></param>
    public DBxDummyDataWriter(DBxConBase con, DBxDataWriterInfo writerInfo)
      : base(con, writerInfo)
    {
    }

  #endregion

  #region ��������

    /// <summary>
    /// ������ �� ������
    /// </summary>
    protected override void OnWrite()
    {
    }

  #endregion
  }
#endif

  /// <summary>
  /// ���������� DBxDataWriter �� ���������.
  /// �� �������� ������ ������ DBxConBase ��� ��������� ������ ������.
  /// ��������, ���� ����� ������� �� ����� ��������������
  /// </summary>
  public class DBxDefaultDataWriter : DBxDataWriter
  {
    #region �����������

    /// <summary>
    /// ������������� ������� � ��������� Created
    /// </summary>
    /// <param name="con"></param>
    /// <param name="writerInfo"></param>
    public DBxDefaultDataWriter(DBxConBase con, DBxDataWriterInfo writerInfo)
      : base(con, writerInfo)
    {
    }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ��������� ������
    /// </summary>
    protected override void OnWrite()
    {
      switch (WriterInfo.Mode)
      {
        case DBxDataWriterMode.Insert:
          Con.AddRecord(WriterInfo.TableName, WriterInfo.Columns, Values);
          break;
        case DBxDataWriterMode.Update:
        case DBxDataWriterMode.InsertOrUpdate:
          DBxFilter[] Filters = new DBxFilter[SearchColumns.Count];
          for (int i = 0; i < SearchColumns.Count; i++)
            Filters[i] = new ValueFilter(SearchColumns[i], this[SearchColumnPositions[i]]);
          DBxFilter f = AndFilter.FromArray(Filters);

          if (WriterInfo.Mode == DBxDataWriterMode.InsertOrUpdate)
          {
            if (Con.GetRecordCount(WriterInfo.TableName, f) == 0)
            {
              // INSERT
              Con.AddRecord(WriterInfo.TableName, WriterInfo.Columns, Values);
              return;
            }
          }

          // ����������
          object[] v2 = new object[OtherColumns.Count];
          for (int i = 0; i < v2.Length; i++)
            v2[i] = this[OtherColumnPositions[i]];

          Con.SetValues(WriterInfo.TableName, AndFilter.FromArray(Filters), OtherColumns, v2);
          break;
        default:
          throw new BugException("Mode=" + WriterInfo.Mode.ToString());
      }
    }

    #endregion
  }

#if XXX
  /// <summary>
  /// ������ ��� ������, ������������ ������� DBxCommand � ����������� "P1", "P2", ...
  /// ���� ����� �� �������� � ������ Insert-or-update
  /// </summary>
  internal class DBxParametri�DataWriter : DBxDataWriter
  {
  #region �����������

    public DBxParametri�DataWriter(DBxConBase con, DBxDataWriterInfo writerInfo)
      : base(con, writerInfo)
    {
    }

  #endregion

  #region �������������� �������

    /// <summary>
    /// �������, ����������� ������ ��� ������ ������
    /// </summary>
    protected DbCommand Command { get { return _Command; } }
    private DbCommand _Command;

    protected void PrepareCommand()
    {
      DBxSqlBuffer Buffer = new DBxSqlBuffer(Con.DB.Formatter);
      switch (WriterInfo.Mode)
      {
        case DBxDataWriterMode.Insert:
          FormatInsertSQL(Buffer);
          break;

        case DBxDataWriterMode.Update:
          FormatUpdateSql(Buffer);
          break;

        case DBxDataWriterMode.InsertOrUpdate:
          throw new NotImplementedException("�� ����������� ��� ������ InsertOrUpdate");

        default:
          throw new BugException("����������� Mode=" + WriterInfo.Mode.ToString());
      }

      _Command = Con.DB.ProviderFactory.CreateCommand();
      _Command.CommandText = Buffer.SB.ToString();
      for (int i = 0; i < Values.Length; i++)
      {
        DbParameter p = Con.DB.ProviderFactory.CreateParameter();
        p.ParameterName="P" + (i + 1).ToString();
        _Command.Parameters.Add(p);
      }
      _Command.Connection = Con.�onnection;
      _Command.Prepare(); // ���������� �������. ��� SQLite ������ �� ������, � ��� ������ ����������� - ��� �����
    }

    private void FormatInsertSQL(DBxSqlBuffer Buffer)
    {
      Buffer.SB.Append("INSERT INTO ");
      Buffer.FormatTableName(WriterInfo.TableName);
      Buffer.SB.Append(" (");
      Buffer.FormatCSColumnNames(WriterInfo.Columns);
      Buffer.SB.Append(") VALUES (");

      for (int i = 0; i < Values.Length; i++)
      {
        if (i > 0)
          Buffer.SB.Append(',');
        Buffer.FormatParamPlaceholder(i);
      }
      Buffer.SB.Append(")");
    }

    private void FormatUpdateSql(DBxSqlBuffer Buffer)
    {
      Buffer.SB.Append("UPDATE ");
      Buffer.FormatTableName(WriterInfo.TableName);
      Buffer.SB.Append(" SET ");

      for (int i = 0; i < OtherColumnNames.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(", ");
        Buffer.FormatColumnName(OtherColumnNames[i]);
        Buffer.SB.Append("=");
        Buffer.FormatParamPlaceholder(OtherColumnPositions[i]);
      }

      Buffer.SB.Append(" WHERE ");
      for (int i = 0; i < PKColumnNames.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(" AND ");
        Buffer.FormatColumnName(PKColumnNames[i]);
        Buffer.SB.Append("=");
        Buffer.FormatParamPlaceholder(PKColumnPositions[i]);
      }
    }

  #endregion

  #region OnWrite

    protected override void OnWrite()
    {
      if (_Command == null)
        PrepareCommand();

      for (int i = 0; i < Values.Length; i++)
      {
        object v = Con.DB.Formatter.PrepareParamValue(Values[i], ColumnDefs[i].ColumnType);
        _Command.Parameters[i].Value = v;
      }
      _Command.ExecuteNonQuery();
    }

  #endregion
  }
#endif
}
