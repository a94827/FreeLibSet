using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
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

namespace AgeyevAV
{
  #region �������

  /// <summary>
  /// ��������� ������� DataTableRepeater.ValueNeeded
  /// </summary>
  public sealed class DataTableRepeaterValueNeededEventArgs : EventArgs
  {
    #region �������������

    internal void Init(DataRow sourceRow, string columnName)
    {
      _SourceRow = sourceRow;
      _ColumnName = columnName;
      _Value = null;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������ � �������� ������� MasterTable
    /// </summary>
    public DataRow SourceRow { get { return _SourceRow; } }
    private DataRow _SourceRow;

    /// <summary>
    /// ��� ���� � ������� SlaveTable, �������� �������� ��������� ��������
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// ���� ������ ���� �������� ����������� ��������
    /// </summary>
    public object Value { get { return _Value; } set { _Value = value; } }
    private object _Value;

    #endregion
  }

  /// <summary>
  /// ������� ������� DataTableRepeater.ValueNeeded
  /// </summary>
  /// <param name="sender">������ DataTableRepeater</param>
  /// <param name="args">��������� �������</param>
  public delegate void DataTableRepeaterValueNeededEventHandler(object sender, DataTableRepeaterValueNeededEventArgs args);

  #endregion

  /// <summary>
  /// ����������� �������. �������������� ������ � ��������, ������� �������� ������� ����� ��������.
  /// ���������� ���� ������ ����������� � ���������� ����.
  /// ���������� ��� ������ ������� ��������� � ������� DataTableRepeater.SlaveTable � ����������� ������ �����������
  /// �������� � ����������� �������� ValueNeeded (��� � ������-����������). ����� ��������������� �������� MasterTable.
  /// � SlaveTable ����� ���� ����������� ��������� ��������, � ������� ����� ��������� ����������, � ��� ����� � �� ����������� �����.
  /// ���������������� ����������/�������� ����� � ��������� �������� ����� � ������� MasterTable. �������������
  /// ����������� �� ����� ���������� ����� ��� � ������� ����������� �������.
  /// ��� ��������� �������� MasterTable, � ������� �������������� ������������. ���� � ���� ������� DataTableRepeater
  /// ��������� �����������, �� ������� �������� DataTableRepeater.Dispose() ��� ������������� MasterTable=null ��� ���������� ������������.
  /// ����� �� �������� ����������������.
  /// </summary>
  public class DataTableRepeater : SimpleDisposableObject
  {
    #region ����������� � Dispose()

    /// <summary>
    /// ������� ������
    /// </summary>
    public DataTableRepeater()
    {
      _SlaveTable = new DataTable();
      _ValueNeededArgs = new DataTableRepeaterValueNeededEventArgs();
      _SourceColumnMaps = new List<KeyValuePair<int, int>>();
      _CalculatedColumns = new List<int>();

      _RowMapMode = RowMapMode.None;
    }

    /// <summary>
    /// ������������� �������� MasterTable=null.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
        this.MasterTable = null;
      base.Dispose(disposing);
    }

    #endregion

    #region �������-����������� � ��������������� ���������

    /// <summary>
    /// �������-�����������.
    /// ��������� ������� ������ ���� ��������� �� ������������� MasterTable.
    /// ������ ���������� ���� �������, ����� ���������� ����, ��������� �������� ��������.
    /// ���� � ������� ���������� ��������� ����, �� ����� ����������� ������ ������� �����.
    /// ����� ����������� ������� �������, ������ ������ ���������, ��������� ��������� ����� ������ DataTableRepeater.
    /// ��� ������� ����� �������������� � �������� ��������� ������ ���������� ��������� (DataGridView.DataSource)
    /// </summary>
    public DataTable SlaveTable
    {
      get { return _SlaveTable; }
      set
      {
        if (_MasterTable != null)
          throw new InvalidOperationException();
        if (value == null)
          throw new ArgumentNullException();
        _SlaveTable = value;
      }
    }
    private DataTable _SlaveTable;

    /// <summary>
    /// ���������� ������� ����������, ����� ��������� ��������, �������������� � SlaveTable, ������ �������� ������.
    /// ���������� ��������� �������� ������ �������� � MasterTable
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void SlaveTable_ColumnChanged(object sender, DataColumnChangeEventArgs args)
    {
#if DEBUG
      if (_MasterTable == null)
        throw new BugException("MasterTable=null");
#endif

      if (_InsideRowChanged)
        return;

      DataRow masterRow = GetMasterRow(args.Row);
      if (masterRow == null)
        throw new NullReferenceException("�� ������� ������ master-�������");

      masterRow[args.Column.ColumnName] = args.ProposedValue;
    }

    #endregion

    #region ������� �������

    /// <summary>
    /// ������� �������.
    /// �������� ������ ���� ����������� ����� ����, ��� ��������� ��������� ������� SlaveTable.
    /// ���� SlaveTable ����� ��������� ����, �� �������������� ������� ������ ����� ����� �� ��������� ����.
    /// </summary>
    public DataTable MasterTable
    {
      get { return _MasterTable; }
      set
      {
        if (_SlaveTable.Columns.Count == 0)
          throw new InvalidOperationException("��������� ������� SlaveTable ������ ���� ���������");
        if (Object.ReferenceEquals(value, _SlaveTable))
          throw new ArgumentException("������ ����������� ������ �� SlaveTable");

        _SourceColumnMaps.Clear();
        _CalculatedColumns.Clear();
        _RowMapMode = RowMapMode.None;
        _RowDict = null;

        if (_MasterTable != null)
        {
          _MasterTable.RowChanged -= new DataRowChangeEventHandler(DataSource_RowChanged);
          _MasterTable.RowDeleting -= new DataRowChangeEventHandler(DataSource_RowDeleting);
          _MasterTable.TableCleared -= new DataTableClearEventHandler(DataSource_TableCleared);
        }
        _SlaveTable.ColumnChanged -= new DataColumnChangeEventHandler(SlaveTable_ColumnChanged);

        _MasterTable = value;

        OnMasterTableChanged();

        _SlaveTable.BeginLoadData();
        try
        {
          _SlaveTable.Rows.Clear();

          if (_MasterTable != null)
          {
            for (int i = 0; i < _SlaveTable.Columns.Count; i++)
            {
              int p = _MasterTable.Columns.IndexOf(_SlaveTable.Columns[i].ColumnName);
              if (p >= 0)
              {
                _SourceColumnMaps.Add(new KeyValuePair<int, int>(p, i));
              }
              else
              {
                _CalculatedColumns.Add(i);
                _SlaveTable.Columns[i].ReadOnly = true;
              }
            }

            InitRowMapMode();

            foreach (DataRow SrcRow in _MasterTable.Rows)
            {
              if (SrcRow.RowState == DataRowState.Deleted)
                continue; // 16.06.2021 ? 
              DataRow ResRow = _SlaveTable.NewRow();
              ProcessRow(SrcRow, ResRow);
              _SlaveTable.Rows.Add(ResRow);
              if (_RowDict != null)
                _RowDict.Add(SrcRow, ResRow);
            }
          }
        }
        finally
        {
          _SlaveTable.EndLoadData();
        }

        if (_MasterTable != null)
        {

          _MasterTable.RowChanged += new DataRowChangeEventHandler(DataSource_RowChanged);
          _MasterTable.RowDeleting += new DataRowChangeEventHandler(DataSource_RowDeleting);
          _MasterTable.TableCleared += new DataTableClearEventHandler(DataSource_TableCleared);
        }

        _SlaveTable.ColumnChanged += new DataColumnChangeEventHandler(SlaveTable_ColumnChanged);
      }
    }
    private DataTable _MasterTable;

    /// <summary>
    /// ����� ���������� ��� ��������� �������� MasterTable ��������������� ����� ����������� �������-�����������.
    /// ����� ��������������, ��������, ��� ������������� ���������� �������� ������-����������.
    /// �� ������ ������ �������� MasterTable ����� ���� null.
    /// </summary>
    protected virtual void OnMasterTableChanged()
    { 
    }

    private bool _InsideRowChanged;

    void DataSource_RowChanged(object sender, DataRowChangeEventArgs args)
    {
      DataRow ResRow;

      // �� ������
      //if ((args.Action & DataRowAction.Delete) != 0)
      //{
      //  ResRow = _RowDictionary[args.Row];
      //  ResRow.Delete();
      //  _RowDictionary.Remove(args.Row);
      //}

      if (_InsideRowChanged)
        return;

      _InsideRowChanged = true;
      try
      {
        if ((args.Action & DataRowAction.Add) != 0)
        {
          ResRow = _SlaveTable.NewRow();
          ProcessRow(args.Row, ResRow);
          _SlaveTable.Rows.Add(ResRow);
          if (_RowDict != null)
            _RowDict.Add(args.Row, ResRow);
        }
        else if ((args.Action & DataRowAction.Change) != 0)
        {
          ResRow = GetSlaveRow(args.Row);
          if (ResRow != null) // 20.06.2021
            ProcessRow(args.Row, ResRow);
        }
      }
      finally
      {
        _InsideRowChanged = false;
      }
    }

    void DataSource_RowDeleting(object sender, DataRowChangeEventArgs args)
    {
      DataRow ResRow = GetSlaveRow(args.Row);
      ResRow.Delete();
      if (_RowDict != null)
        _RowDict.Remove(args.Row);
    }

    void DataSource_TableCleared(object sender, DataTableClearEventArgs args)
    {
      _SlaveTable.Rows.Clear();
      if (_RowDict != null)
        _RowDict.Clear();
    }

    /// <summary>
    /// ��������� ������ ������������ ����� � SlaveTable.
    /// ���� ����� ��� ������ ������������ � ���������� ����, ��� ��� ���������� ������ ����������� ���������
    /// ����� �������� ������� (��������� �� ���� ������) � ���������� �������� MasterTable.
    /// </summary>
    public void Refresh()
    {
      this.MasterTable = this.MasterTable;
    }

    #endregion

    #region ������� ValueNeeded

    /// <summary>
    /// ������� ���������� ��� ������� �������� � ������� SlaveTable, ������� ��������� ���������.
    /// </summary>
    public event DataTableRepeaterValueNeededEventHandler ValueNeeded;

    /// <summary>
    /// �������� ������� ValueNeeded.
    /// �����-��������� ����� �������������� ����� ��� ���������� �������� �����.
    /// </summary>
    /// <param name="args">��������� �������</param>
    protected virtual void OnValueNeeded(DataTableRepeaterValueNeededEventArgs args)
    {
      if (ValueNeeded != null)
        ValueNeeded(this, args);
    }

    /// <summary>
    /// ���������� ������������ ��������� ���������� �������, ����� ��������� ���������� ������.
    /// </summary>
    private DataTableRepeaterValueNeededEventArgs _ValueNeededArgs;

    /// <summary>
    /// ������ �����, ���������� �� �������� �������
    /// Key - ������ ������� � ������� DataSource
    /// Value - ������ ������� � ������� ResultTable
    /// </summary>
    private List<KeyValuePair<int, int>> _SourceColumnMaps;

    /// <summary>
    /// ������ �����, ��� ������� ���������� ������� ValueNeeded.
    /// �������� ������� �������� � ������� ResultTable
    /// </summary>
    private List<int> _CalculatedColumns;

    /// <summary>
    /// ��������� ��� ����� ������
    /// </summary>
    /// <param name="srcRow">������ � MasterTable</param>
    /// <param name="resRow">������ � SlaveTable, ������� ����� ���������</param>
    private void ProcessRow(DataRow srcRow, DataRow resRow)
    {
      #region �����������

      for (int i = 0; i < _SourceColumnMaps.Count; i++)
        resRow[_SourceColumnMaps[i].Value] = srcRow[_SourceColumnMaps[i].Key];

      #endregion

      #region ������

      for (int i = 0; i < _CalculatedColumns.Count; i++)
      {
        _ValueNeededArgs.Init(srcRow, _SlaveTable.Columns[_CalculatedColumns[i]].ColumnName);
        OnValueNeeded(_ValueNeededArgs);

        _SlaveTable.Columns[_CalculatedColumns[i]].ReadOnly = false;
        if (_ValueNeededArgs.Value == null)
          resRow[_CalculatedColumns[i]] = DBNull.Value;
        else
          resRow[_CalculatedColumns[i]] = _ValueNeededArgs.Value;
        _SlaveTable.Columns[_CalculatedColumns[i]].ReadOnly = true;
      }

      #endregion
    }

    #endregion

    #region ������������ �����

    #region ������������ RowMapMode

    /// <summary>
    /// ������ ������������� ����� � MasterTable � SlaveTable
    /// </summary>
    private enum RowMapMode
    {
      /// <summary>
      /// ������� �� ������������
      /// </summary>
      None,

      /// <summary>
      /// ������������ BidirectionalDictionary
      /// </summary>
      Dictionary,

      /// <summary>
      /// ������������ �� ���������� �����, ������� ������� �� ������ ����
      /// </summary>
      SimplePrimaryKey,

      /// <summary>
      /// ������������ �� ���������� �����, ������� ������� �� ���������� �����
      /// </summary>
      ComplexPrimaryKey
    }

    #endregion

    /// <summary>
    /// ������� ����� ������������ �����
    /// </summary>
    private RowMapMode _RowMapMode;

    /// <summary>
    /// ������������ ����� � ������ RowMapMode.Dictionary
    /// </summary>
    private BidirectionalDictionary<DataRow, DataRow> _RowDict;

    private int _MasterPKColPos, _SlavePKColPos;

    private void InitRowMapMode()
    {
      if (_SlaveTable.PrimaryKey.Length == 0)
      {
        _RowMapMode = RowMapMode.Dictionary;
        _RowDict = new BidirectionalDictionary<DataRow, DataRow>(_MasterTable.Rows.Count);
      }
      else
      {
        string MasterPK = DataTools.GetPrimaryKey(_MasterTable);
        string SlavePK = DataTools.GetPrimaryKey(_SlaveTable);
        if (String.Compare(MasterPK, SlavePK, StringComparison.Ordinal) != 0)
          throw new InvalidOperationException("������� SlaveTable ����� ��������� ���� \"" + SlavePK + "\". ������������ ������� MasterTable ������ ����� ����� �� ����, � �� \"" + MasterPK + "\"");

        if (SlavePK.IndexOf(',') >= 0)
          _RowMapMode = RowMapMode.ComplexPrimaryKey;
        else
        {
          _RowMapMode = RowMapMode.SimplePrimaryKey;
          _MasterPKColPos = _MasterTable.Columns.IndexOf(MasterPK);
          _SlavePKColPos = _SlaveTable.Columns.IndexOf(SlavePK);
        }
      }
    }

    #region Master -> Slave

    /// <summary>
    /// ���������� ������ � �������-����������� SlaveTable, ������� ������������� ������ � �������� ������� MasterTable
    /// </summary>
    /// <param name="masterRow">������ ������� MasterTable</param>
    /// <returns>������ ������� SlaveTable</returns>
    public DataRow GetSlaveRow(DataRow masterRow)
    {
      if (masterRow == null)
        return null;

      switch (_RowMapMode)
      {
        case RowMapMode.Dictionary:
          DataRow slaveRow;
          _RowDict.TryGetValue(masterRow, out slaveRow);
          return slaveRow;
        case RowMapMode.SimplePrimaryKey:
          return _SlaveTable.Rows.Find(masterRow[_MasterPKColPos]);
        case RowMapMode.ComplexPrimaryKey:
          return _SlaveTable.Rows.Find(DataTools.GetPrimaryKeyValues(masterRow));
        case RowMapMode.None:
          return null;
        default:
          throw new BugException();
      }
    }

    /// <summary>
    /// ���������� ������ � �������-����������� SlaveTable, ������� ������������� ������� � �������� ������� MasterTable
    /// </summary>
    /// <param name="masterRows">������ ����� ������� MasterTable</param>
    /// <returns>������ ������� SlaveTable</returns>
    public DataRow[] GetSlaveRows(DataRow[] masterRows)
    {
      if (masterRows == null)
        return null;

      DataRow[] slaveRows = new DataRow[masterRows.Length];

      switch (_RowMapMode)
      {
        case RowMapMode.Dictionary:
          DataRow slaveRow;
          for (int i = 0; i < masterRows.Length; i++)
          {
            _RowDict.TryGetValue(masterRows[i], out slaveRow);
            slaveRows[i] = slaveRow;
          }
          break;
        case RowMapMode.SimplePrimaryKey:
          for (int i = 0; i < masterRows.Length; i++)
            slaveRows[i] = _SlaveTable.Rows.Find(masterRows[i][_MasterPKColPos]);
          break;
        case RowMapMode.ComplexPrimaryKey:
          for (int i = 0; i < masterRows.Length; i++)
            slaveRows[i] = _SlaveTable.Rows.Find(DataTools.GetPrimaryKeyValues(masterRows[i]));
          break;
        case RowMapMode.None:
          break;
        default:
          throw new BugException();
      }

      return slaveRows;
    }

    #endregion

    #region Slave -> Master

    /// <summary>
    /// ���������� ������ � �������� ������� MasterTable, ������� ������������� ������ � �������-����������� SlaveTable.
    /// ������������ ��������� ���������� ��� ���������� ��������������, �.�. ��������� ������ ��������� � �������
    /// �������, � �� ��, ������� ������������ � ���������.
    /// </summary>
    /// <param name="slaveRow">������ ������� SlaveTable</param>
    /// <returns>������ ������� MasterTable</returns>
    public DataRow GetMasterRow(DataRow slaveRow)
    {
      if (slaveRow == null)
        return null;

      switch (_RowMapMode)
      {
        case RowMapMode.Dictionary:
          DataRow masterRow;
          _RowDict.TryGetKey(slaveRow, out masterRow);
          return masterRow;
        case RowMapMode.SimplePrimaryKey:
          return _MasterTable.Rows.Find(slaveRow[_SlavePKColPos]);
        case RowMapMode.ComplexPrimaryKey:
          return _MasterTable.Rows.Find(DataTools.GetPrimaryKeyValues(slaveRow));
        case RowMapMode.None:
          return null;
        default:
          throw new BugException();
      }
    }

    /// <summary>
    /// ���������� ������ � �������� ������� MasterTable, ������� ������������� ������� � �������-����������� SlaveTable.
    /// ������������ ��������� ���������� ��� ���������� ��������������, �.�. ��������� ������ ��������� � �������
    /// �������, � �� ��, ������� ������������ � ���������.
    /// </summary>
    /// <param name="slaveRows">������ ������� SlaveTable</param>
    /// <returns>������ ������� MasterTable</returns>
    public DataRow[] GetMasterRows(DataRow[] slaveRows)
    {
      if (slaveRows == null)
        return null;

      DataRow[] masterRows = new DataRow[slaveRows.Length];

      switch (_RowMapMode)
      {
        case RowMapMode.Dictionary:
          DataRow masterRow;
          for (int i = 0; i < slaveRows.Length; i++)
          {
            _RowDict.TryGetKey(slaveRows[i], out masterRow);
            masterRows[i] = masterRow;
          }
          break;
        case RowMapMode.SimplePrimaryKey:
          for (int i = 0; i < slaveRows.Length; i++)
            masterRows[i] = _MasterTable.Rows.Find(slaveRows[i][_SlavePKColPos]);
          break;
        case RowMapMode.ComplexPrimaryKey:
          for (int i = 0; i < slaveRows.Length; i++)
            masterRows[i] = _MasterTable.Rows.Find(DataTools.GetPrimaryKeyValues(slaveRows[i]));
          break;
        case RowMapMode.None:
          break;
        default:
          throw new BugException();
      }
      return masterRows;
    }

    #endregion

    #region ������

    /// <summary>
    /// ��� �������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      if (MasterTable != null)
      {
        sb.Append("\"");
        if (String.IsNullOrEmpty(MasterTable.TableName))
          sb.Append("(��� �����)");
        else
          sb.Append(MasterTable.TableName);
        sb.Append("\" (RowCount=");
        sb.Append(MasterTable.Rows.Count);
        sb.Append(") -> ");
      }

      sb.Append("\"");
      if (String.IsNullOrEmpty(SlaveTable.TableName))
        sb.Append("(��� �����)");
      else
        sb.Append(SlaveTable.TableName);
      sb.Append("\" (RowCount=");
      sb.Append(SlaveTable.Rows.Count);
      sb.Append(")");
      if (MasterTable == null)
        sb.Append(" ��� ���������");

      string sPK = DataTools.GetPrimaryKey(SlaveTable);
      if (!String.IsNullOrEmpty(sPK))
      {
        sb.Append(". PrimaryKey=\"");
        sb.Append(sPK);
        sb.Append("\"");
      }

      return sb.ToString();
    }

    #endregion

    #endregion
  }
}
