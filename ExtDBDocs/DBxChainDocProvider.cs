using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using AgeyevAV.IO;
using AgeyevAV.Config;
using AgeyevAV.Caching;
using AgeyevAV.Remoting;
using System.Diagnostics;

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
  /// ��������� ������� ��� �������������� ����������� � �������
  /// </summary>
  public class DBxRetriableExceptionEventArgs : EventArgs
  {
    #region �����������

    /// <summary>
    /// ������� �� ��������� ���������������� �����
    /// </summary>
    /// <param name="exception">������ ����������</param>
    /// <param name="repeatCount">���������� ��������</param>
    public DBxRetriableExceptionEventArgs(Exception exception, int repeatCount)
    {
      if (exception == null)
        throw new ArgumentNullException("exception");
      _Exception = exception;
      _RepeatCount = repeatCount;
      _Retry = false;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ����������.
    /// ���������������� ���������� ����������� ������ ���������, ��� ���������� ������� � ������������ � �������
    /// </summary>
    public Exception Exception { get { return _Exception; } }
    private Exception _Exception;

    /// <summary>
    /// ������� ��������� �������.
    /// ��� ������������� ���������� �������� ����� �������� 0. ���� � ��� �� ������ ��������� ���������
    /// ����������, �������� ���������� 1, � �.�.
    /// ���������������� ���������� �����, ��������, ��������� �������������� ���������� ������ ��� ������
    /// ������.
    /// ������� ��������� � ����������� ������ ������, � �� � ���������� ������.
    /// </summary>
    public int RepeatCount { get { return _RepeatCount; } }
    private int _RepeatCount;

    /// <summary>
    /// ��� �������� ������ ���� ����������� � true, ���� ���������� � �������� ������������� � �������
    /// ��������� ������� ������� ����� �������
    /// </summary>
    public bool Retry { get { return _Retry; } set { _Retry = value; } }
    private bool _Retry;

    #endregion
  }

  /// <summary>
  /// ��� ������� DBxDocProvider.ExceptionCaught
  /// </summary>
  /// <param name="sender">������ �� DBxChainDocProvider.</param>
  /// <param name="args">��������� �������</param>
  public delegate void DBxRetriableExceptionEventHandler(object sender, DBxRetriableExceptionEventArgs args);

  /// <summary>
  /// ������, ����������� ��� ����������� ������� �����������.
  /// ��������� ������������ DBxChainDocProvider. ��� ��������� ������ ��������� ����� DBxDocProvider.CreateProxy().
  /// ���������� �� ���� ��� ������������� ������.
  /// �������� ������ ������������ ��������� (����������� �� MasterByRefObject) � ������������� ������.
  /// �� �������� ������������� ������� � �������
  /// </summary>
  [Serializable]
  public sealed class DBxDocProviderProxy
  {
    #region ���������� �����������

    internal DBxDocProviderProxy(DBxDocProvider source, NamedValues fixedInfo)
    {
      _Source = source;
      _FixedInfo = fixedInfo;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ���������-��������, ��� �������� ��� ������ ����� CreateProxy()
    /// </summary>
    public DBxDocProvider Source { get { return _Source; } }
    private DBxDocProvider _Source;

    internal NamedValues FixedInfo { get { return _FixedInfo; } }
    private NamedValues _FixedInfo;

    #endregion
  }

  /// <summary>
  /// ������� ����� ��� ���������� ������� �����������.
  /// �������������� � ����������-���������. �������� ����� ���� � ������� AppDomain ��� ��������� ����� Remoting.
  /// ��������� �������� ���� DBxCache � ���� �������� ������, ���� �������� ������ �������� ���������.
  /// ����� DBxChainDocProvider ��� �� ���� �������� ����������������, �� ������������� ������� ����� ���� ������������
  /// ���������� ����� �������, ���� ����� ���� � ������������.
  /// </summary>
  public class DBxChainDocProvider : DBxDocProvider
  {
    #region �����������

    /// <summary>
    /// ������� ���������� ���������
    /// </summary>
    /// <param name="sourceProxy">��������� ������ DBxDocProvider.CreateProxy() ��� ����������� ���������� � �������</param>
    /// <param name="currentThreadOnly">���� true, �� ������ ������ ���������� ����� ��������� ������ �� �������� ������</param>
    public DBxChainDocProvider(DBxDocProviderProxy sourceProxy, bool currentThreadOnly)
      : base(sourceProxy.FixedInfo, currentThreadOnly)
    {
      _Source = sourceProxy.Source;
      _SourceIsRemote = System.Runtime.Remoting.RemotingServices.IsTransparentProxy(sourceProxy.Source);

      //ServerTimeDiff = Source.ServerTime - DateTime.Now;
    }

    #endregion

    #region �������� ��������

    /// <summary>
    /// �������� ���������, ����������� �������� ����� ��������
    /// </summary>
    protected DBxDocProvider Source { get { return _Source; } }
    private DBxDocProvider _Source;

    /// <summary>
    /// ���������� true, ���� ���������-�������� �������� ��������� �������� (TransparentProxy).
    /// � ���� ������ DBxChainDocProvider ���������� ����������� ����� DBxCache
    /// </summary>
    public bool SourceIsRemote { get { return _SourceIsRemote; } }
    private bool _SourceIsRemote;

    #endregion

    #region ������ � ��������, ����������� �������� DocProvider

    /// <summary>
    /// ��������� ��������� (��� �������������)
    /// </summary>
    /// <param name="docTypeName">��� ������� ����������</param>
    /// <param name="docIds">������ ���������������</param>
    /// <returns>������� ����������</returns>
    public override DataTable LoadDocData(string docTypeName, Int32[] docIds)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.LoadDocData(docTypeName, docIds);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ��������� ��������� (��� �������������)
    /// </summary>
    /// <param name="docTypeName">��� ������� ����������</param>
    /// <param name="filter">������� ��� ������ ����������</param>
    /// <returns>������� ����������</returns>
    public override DataTable LoadDocData(string docTypeName, DBxFilter filter)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.LoadDocData(docTypeName, filter);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ��������� ������������.
    /// ��������������, ��� ������� ���������� ��� ���������
    /// </summary>
    /// <param name="docTypeName">��� ������� ����������</param>
    /// <param name="subDocTypeName">��� ������� �������������</param>
    /// <param name="docIds">������ ��������������� ����������, ��� ������� ����������� ������������</param>
    /// <returns>������� �������������</returns>
    public override DataTable LoadSubDocData(string docTypeName, string subDocTypeName, Int32[] docIds)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.LoadSubDocData(docTypeName, subDocTypeName, docIds);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ���������� ���������.
    /// ����������� ��������, ��������� � �������� ���������� � �������������
    /// </summary>
    /// <param name="dataSet">����� ������</param>
    /// <param name="reloadData">���� true, �� ����� ��������� ��� �� ����� ������.
    /// � ��� ��������� �������������� ����� ���������� � ������������� ����� �������� �� ��������,
    /// � ������������ ������ �� ��� ����������. ��� ��������� � ���������� ������������, �����
    /// ������������ �������� ������ "���������", ����� ����� ���� ���������� ����� ��������������.
    /// ���� false, �� ������������ ����� ������ �� ������������</param>
    /// <returns>����� � ������������� �������� ��� null</returns>
    protected override DataSet OnApplyChanges(DataSet dataSet, bool reloadData)
    {
      CheckThread();

      return Source.ApplyChanges(dataSet, reloadData);
    }

    /// <summary>
    /// �������� ������� ������ ����� ���������.
    /// ���������� ������� � ����� �������
    /// </summary>
    /// <param name="docTypeName">��� ������� ���������</param>
    /// <param name="docId">������������� ���������</param>
    /// <param name="docVersion">������ ���������</param>
    /// <returns>�������</returns>
    public override DataTable LoadDocDataVersion(string docTypeName, Int32 docId, int docVersion)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.LoadDocDataVersion(docTypeName, docId, docVersion);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� ������� ������ ����� �������������.
    /// </summary>
    /// <param name="docTypeName">��� ������� ���������</param>
    /// <param name="subDocTypeName">��� ������� �������������</param>
    /// <param name="docId">������������� ���������</param>
    /// <param name="docVersion">������ ���������</param>
    /// <returns>�������</returns>
    public override DataTable LoadSubDocDataVersion(string docTypeName, string subDocTypeName, Int32 docId, int docVersion)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.LoadSubDocDataVersion(docTypeName, subDocTypeName, docId, docVersion);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ���������� ��� ������� ������ �� ���������. ���������� ������� ���������� (� ����� �������) � �������������,
    /// ������� ��������� ������. � ����� ����������� ������ �� ������ UserActions � DocActions.
    /// � ����� ����������� ������� ���������� �� ���� ������ �������. ����� �������� �� �� ��������
    /// ����������, ����� ������� ������ ����������� ������� "Undo_".
    /// ����� ������������ ��� ���������� �����
    /// </summary>
    /// <param name="docTypeName">��� ������� ���������</param>
    /// <param name="docId">������������� ���������</param>
    /// <returns>����� ������</returns>
    public override DataSet LoadUnformattedDocData(string docTypeName, Int32 docId)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.LoadUnformattedDocData(docTypeName, docId);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }


    /// <summary>
    /// ���������� SQL-������� SELECT � �������� ���� ��������� ����������
    /// </summary>
    /// <param name="info">��������� ��� �������</param>
    /// <returns>������� ������</returns>
    public override DataTable FillSelect(DBxSelectInfo info)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.FillSelect(info);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ����� ������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="where">������� ������</param>
    /// <param name="orderBy">������� ����������. ����� ��������, ������ ���� �������
    /// ������ ����� ������, ��������������� ������� <paramref name="where"/>.
    /// ����� ���������� ������ �� �������, � ������������ � ��������.
    /// ���� ������� �� �����, ����� ������ ����� ����������, �� ����������</param>
    /// <returns>������������� ��������� ������ ��� 0, ���� ������ �� �������</returns>
    public override Int32 FindRecord(string tableName, DBxFilter where, DBxOrder orderBy)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.FindRecord(tableName, where, orderBy);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ����� ������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="where">������� ������</param>
    /// <param name="singleOnly">���� true � ������� ������ ����� ������, ��������������� �������
    /// <paramref name="where"/>, �� ������������ 0</param>
    /// <returns>������������� ��������� ������ ��� 0, ���� ������ �� �������</returns>
    public override Int32 FindRecord(string tableName, DBxFilter where, bool singleOnly)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.FindRecord(tableName, where, singleOnly);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ��������� �������� ��� ������ ����. ��� ���� ����� ��������� ����� ���
    /// ���������� �������� �� ��������� �������. ����������� ������ ����������
    /// �������� ���� �� ������, � ��� ��������� ������������ ������� ����, ���
    /// ������ �������
    /// </summary>
    /// <param name="tableName">��� �������, � ������� ����������� �����</param>
    /// <param name="id">������������� ������. ����� ���� 0, ����� ������������ Value=null</param>
    /// <param name="columnName">��� ���� (����� ���� � �������)</param>
    /// <param name="value">���� �� ������ ������������ ��������</param>
    /// <returns>true, ���� ���� ���� �������</returns>
    public override bool GetValue(string tableName, Int32 id, string columnName, out object value)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetValue(tableName, id, columnName, out value);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� �������� ��� ��������� ������ ����� ��� ����� ������.
    /// ���� �� ������� ������ � �������� ��������������� <paramref name="id"/>, 
    /// �� ������������ ������, ���������� ���� �������� null.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="id">������������� ������</param>
    /// <param name="columnNames">����� ��������, �������� ������� ����� ��������</param>
    /// <returns>������ ��������</returns>
    public override object[] GetValues(string tableName, Int32 id, DBxColumns columnNames)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetValues(tableName, id, columnNames);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� ������ ��������������� � ������� ��� �����, ��������������� ��������� �������.
    /// ������� �� ���� Deleted ������ ���� ������ � ����� ����
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="where">�������</param>
    /// <returns>������ ���������������</returns>
    public override IdList GetIds(string tableName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetIds(tableName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� ����������� �������� ��������� ����.
    /// ������ �������, ���������� �������� NULL, ������������.
    /// ���� ��� �� ����� ������, ��������������� ������� <paramref name="where"/>, ������������ null.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnName">��� ��������� ����</param>
    /// <param name="where">������ (null ��� ������ ����� ���� ����� �������)</param>
    /// <returns>����������� ��������</returns>
    public override object GetMinValue(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetMinValue(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� ������������ �������� ��������� ����
    /// ������ �������, ���������� �������� NULL, ������������.
    /// ���� ��� �� ����� ������, ��������������� ������� <paramref name="where"/>, ������������ null.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnName">��� ��������� ����</param>
    /// <param name="where">������ (null ��� ������ ����� ���� ����� �������)</param>
    /// <returns>������������ ��������</returns>
    public override object GetMaxValue(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetMaxValue(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� �������� ����� ��� ������, ���������� ����������� �������� ���������
    /// ����.
    /// ������ �������, ���������� �������� NULL, ������������.
    /// ���� �� ������� �� ����� ������, ��������������� ������� <paramref name="where"/>,
    /// ������������ ������, ���������� ���� �������� null.
    /// ����� ����� � <paramref name="columnNames"/>, <paramref name="minColumnName"/> � <paramref name="where"/>
    /// ����� ��������� �����. � ���� ������ ������������ �������� �� ��������� ������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnNames">����� �����, �������� ������� ����� ��������</param>
    /// <param name="minColumnName">��� ����, ����������� �������� �������� �������� �������� ������ ������</param>
    /// <param name="where">������ �����, ����������� � ������</param>
    /// <returns>������ �������� ��� �����, �������� � <paramref name="columnNames"/></returns>
    public override object[] GetValuesForMin(string tableName, DBxColumns columnNames, string minColumnName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetValuesForMin(tableName, columnNames, minColumnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� �������� ����� ��� ������, ���������� ������������ �������� ���������
    /// ����.
    /// ������ �������, ���������� �������� NULL, ������������.
    /// ���� �� ������� �� ����� ������, ��������������� ������� <paramref name="where"/>,
    /// ������������ ������, ���������� ���� �������� null.
    /// ����� ����� � <paramref name="columnNames"/>, <paramref name="maxColumnName"/> � <paramref name="where"/>
    /// ����� ��������� �����. � ���� ������ ������������ �������� �� ��������� ������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnNames">����� �����, �������� ������� ����� ��������</param>
    /// <param name="maxColumnName">��� ����, ������������ �������� �������� �������� �������� ������ ������</param>
    /// <param name="where">������ �����, ����������� � ������</param>
    /// <returns>������ �������� ��� �����, �������� � <paramref name="columnNames"/></returns>
    public override object[] GetValuesForMax(string tableName, DBxColumns columnNames, string maxColumnName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetValuesForMax(tableName, columnNames, maxColumnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� ��������� �������� ��������� ���� ��� ��������� �������
    /// ������ �������, ���������� �������� NULL, ������������
    /// ���� ��� �� ����� ������, ��������������� ������� <paramref name="where"/>, ������������ null.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnName">��� ��������� ����</param>
    /// <param name="where">������ (null ��� ������������ ���� ����� �������)</param>
    /// <returns>��������� �������� ��� null</returns>
    public override object GetSumValue(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetSumValue(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ��������� ������ ���������� �������� ���� SELECT DISTINCT
    /// � ���������� ������� ����� ���� ����. ������� ����� ����������� �� ����� ����
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnName">��� ����</param>
    /// <param name="where">�������������� ������ �������</param>
    /// <returns>������� � ������������ ��������</returns>
    public override DataTable FillUniqueColumnValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.FillUniqueColumnValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� ��������� �������� ���� ��� ��������.
    /// ���� � ������� ����������� �������� NULL, �� ��� ������������.
    /// ������������ ������ �����������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnName">��� ���������� ����. ����� ��������� �����, ���� ��������� �������� �������� ���������� ����</param>
    /// <param name="where">������. ���� null, �� ��������������� ��� ������ �������</param>
    /// <returns>������ ��������</returns>
    public override string[] GetUniqueStringValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueStringValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� �������� �������� ���� ��� ��������.
    /// ���� � ������� ����������� �������� NULL, �� ��� ������������.
    /// ������������ ������ �����������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnName">��� ���������� ����. ����� ��������� �����, ���� ��������� �������� �������� ���������� ����</param>
    /// <param name="where">������. ���� null, �� ��������������� ��� ������ �������</param>
    /// <returns>������ ��������</returns>
    public override int[] GetUniqueIntValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueIntValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }


    /// <summary>
    /// �������� �������� �������� ���� ��� ��������.
    /// ���� � ������� ����������� �������� NULL, �� ��� ������������.
    /// ������������ ������ �����������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnName">��� ���������� ����. ����� ��������� �����, ���� ��������� �������� �������� ���������� ����</param>
    /// <param name="where">������. ���� null, �� ��������������� ��� ������ �������</param>
    /// <returns>������ ��������</returns>
    public override long[] GetUniqueInt64Values(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueInt64Values(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� �������� �������� ���� ��� ��������.
    /// ���� � ������� ����������� �������� NULL, �� ��� ������������.
    /// ������������ ������ �����������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnName">��� ���������� ����. ����� ��������� �����, ���� ��������� �������� �������� ���������� ����</param>
    /// <param name="where">������. ���� null, �� ��������������� ��� ������ �������</param>
    /// <returns>������ ��������</returns>
    public override float[] GetUniqueSingleValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueSingleValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }
 
    /// <summary>
    /// �������� �������� �������� ���� ��� ��������.
    /// ���� � ������� ����������� �������� NULL, �� ��� ������������.
    /// ������������ ������ �����������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnName">��� ���������� ����. ����� ��������� �����, ���� ��������� �������� �������� ���������� ����</param>
    /// <param name="where">������. ���� null, �� ��������������� ��� ������ �������</param>
    /// <returns>������ ��������</returns>
    public override double[] GetUniqueDoubleValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueDoubleValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� �������� �������� ���� ��� ��������.
    /// ���� � ������� ����������� �������� NULL, �� ��� ������������.
    /// ������������ ������ �����������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnName">��� ���������� ����. ����� ��������� �����, ���� ��������� �������� �������� ���������� ����</param>
    /// <param name="where">������. ���� null, �� ��������������� ��� ������ �������</param>
    /// <returns>������ ��������</returns>
    public override decimal[] GetUniqueDecimalValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueDecimalValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� �������� ���� ���� �/��� ������� ��� ��������.
    /// ���� � ������� ����������� �������� NULL, �� ��� ������������.
    /// ������������ ������ �����������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnName">��� ���������� ����. ����� ��������� �����, ���� ��������� �������� �������� ���������� ����</param>
    /// <param name="where">������. ���� null, �� ��������������� ��� ������ �������</param>
    /// <returns>������ ��������</returns>
    public override DateTime[] GetUniqueDateTimeValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueDateTimeValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� �������� ���� GUID ��� ��������.
    /// ���� � ������� ����������� �������� NULL, �� ��� ������������.
    /// ������������ ������ �����������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnName">��� ���������� ����. ����� ��������� �����, ���� ��������� �������� �������� ���������� ����</param>
    /// <param name="where">������. ���� null, �� ��������������� ��� ������ �������</param>
    /// <returns>������ ��������</returns>
    public override Guid[] GetUniqueGuidValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueGuidValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }


    /// <summary>
    /// ��������� �������� ������� ����
    /// </summary>
    /// <param name="request">��������� �������</param>
    /// <returns>��� ��������</returns>
    public override DBxCacheLoadResponse LoadCachePages(DBxCacheLoadRequest request)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.LoadCachePages(request);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    #region GetRecordCount() � IsTableEmpty()

    /// <summary>
    /// �������� ����� ����� ������� � �������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <returns>����� �������</returns>
    public override int GetRecordCount(string tableName)
    {
      return _Source.GetRecordCount(tableName);
    }

    /// <summary>
    /// �������� ����� ������� � �������, ��������������� �������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="where">�������</param>
    /// <returns>����� �������</returns>
    public override int GetRecordCount(string tableName, DBxFilter where)
    {
      return _Source.GetRecordCount(tableName, where);
    }

    /// <summary>
    /// ���������� true, ���� � ������� ��� �� ����� ������.
    /// ���� �����, ��� GetRecordCount()==0, �� ����� ���� ��������������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <returns>���������� �������</returns>
    public override bool IsTableEmpty(string tableName)
    {
      return _Source.IsTableEmpty(tableName);
    }

    #endregion

    /// <summary>
    /// �������� �������� ������� ����
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="columnNames">������ ��������</param>
    /// <param name="firstIds">��������� �������������� �������</param>
    public override void ClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds)
    {
      _Source.ClearCachePages(tableName, columnNames, firstIds);
    }


    /// <summary>
    /// �������� ������� ������� ��� ���������
    /// </summary>
    /// <param name="docTypeName">��� ������� ���������</param>
    /// <param name="docId">������������� ���������</param>
    /// <returns>������� �������</returns>
    public override DataTable GetDocHistTable(string docTypeName, Int32 docId)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetDocHistTable(docTypeName, docId);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� ������� �������� ������������ ��� ���� ������������� �� ������������ ������
    /// </summary>
    /// <param name="firstDate">��������� ����</param>
    /// <param name="lastDate">�������� ����</param>
    /// <param name="userId">������������� ������������. 0-��� ������������</param>
    /// <param name="singleDocTypeName">��� ������� ���������. ������ ������ - ��������� ���� �����</param>
    /// <returns>������� ��������</returns>
    public override DataTable GetUserActionsTable(DateTime? firstDate, DateTime? lastDate, Int32 userId, string singleDocTypeName)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUserActionsTable(firstDate, lastDate, userId, singleDocTypeName);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// �������� ������� ���������� ��� ������ �������� ������������
    /// </summary>
    /// <param name="actionId">������������� �������� � ������� UserActions</param>
    /// <returns>������� ����������</returns>
    public override DataTable GetUserActionDocTable(Int32 actionId)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUserActionDocTable(actionId);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ���������� ����� ���������� �������� ������������ (������� ��������� �������).
    /// ����� ������������ � ������� DataSetDateTime.Unspecified.
    /// ���� ��� ������������ ��� �� ����� ������ � ������� UserActions, ������������ null
    /// </summary>
    /// <param name="userId">������������� ������������, ��� �������� ���� �������� ������</param>
    /// <returns>����� ��� null</returns>
    public override DateTime? GetUserActionsLastTime(Int32 userId)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUserActionsLastTime(userId);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ���������� ���������� ����������
    /// ���� �����-���� �� ���������� ��� ������������, ������������� ���������� DBxLockDocsException
    /// </summary>
    /// <param name="docSel">������� ����������, ������� ��������� �������������</param>
    /// <returns>������������� ������������� ����������</returns>
    public override Guid AddLongLock(DBxDocSelection docSel)
    {
      CheckThread();

      return Source.AddLongLock(docSel);
    }

    /// <summary>
    /// ������� ���������� ����������
    /// </summary>
    /// <param name="lockGuid">������������� ������������� ����������</param>
    /// <returns>true, ���� ���������� ���� �������. false, ���� ���������� �� ������� (���� ������� �����)</returns>
    public override bool RemoveLongLock(Guid lockGuid)
    {
      CheckThread();

      return Source.RemoveLongLock(lockGuid);
    }

    /// <summary>
    /// ��������� ���������� ������������� ��� ��������� / ������������
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="id">�������������</param>
    /// <returns>��������� �������������</returns>
    public override string GetTextValue(string tableName, Int32 id)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return _Source.GetTextValue(tableName, id);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ���������� ����� ��������� ������.
    /// </summary>
    /// <param name="tableName">��� ������� ��������� ��� ������������</param>
    /// <param name="id">������������� ��������� ��� ������������</param>
    /// <param name="primaryDS">��������� ����� ������</param>
    /// <returns>����� ��� ��������� ��� ������������</returns>
    public override string InternalGetTextValue(string tableName, Int32 id, DataSet primaryDS)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return _Source.InternalGetTextValue(tableName, id, primaryDS);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ���������� ����� ��������� �������������� �������� ������
    /// </summary>
    /// <param name="md5">����������� �����</param>
    /// <returns>������������� ������ ��� 0, ���� ����� ������ ���</returns>
    public override int InternalFindBinData(string md5)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return _Source.InternalFindBinData(md5);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ���������� ����� ��������� �������������� ��������� �����
    /// </summary>
    /// <param name="fileInfo">���������� � �����</param>
    /// <param name="md5">����������� ����� ����������� �����</param>
    /// <param name="binDataId">���� ���������� ������������� �������� ������,
    /// ������������ ������� InternalFindBinData()</param>
    /// <returns>������������� ������ ����� � ���� ������</returns>
    public override Int32 InternalFindDBFile(StoredFileInfo fileInfo, string md5, out Int32 binDataId)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return _Source.InternalFindDBFile(fileInfo, md5, out binDataId);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ����������� ����������� ���� � ����������.
    /// </summary>
    /// <param name="docTypeName">��� ���� ���������</param>
    /// <param name="docIds">������ ���������������.
    /// null �������� �������� ���� ����������. ��������� �������� � ��� ����� � ��������� ���������</param>
    public override void RecalcColumns(string docTypeName, Int32[] docIds)
    {
      CheckThread();
      _Source.RecalcColumns(docTypeName, docIds);
    }

    /// <summary>
    /// �������� ����� ������ ��� ��������� ������ �� ���� ��������.
    /// </summary>
    /// <param name="docTypeName">��� ������� ���������, �� ������� ������ ������</param>
    /// <param name="docId">������������� ���������, �� ������� ������ ������</param>
    /// <param name="showDeleted">���� �� �������� � ������� ������ �� ��������� ���������� � �������������,
    /// � ����� ������ �� ��������� ������������ ���������� ���������.
    /// �� ����� ��������, ������� �� ��� �������� <paramref name="docId"/> �� �������� (�� ����������� ������ ��������� �� ������ ����</param>
    /// <param name="unique">���� true, �� ����� ���������� ������ �� ����� ������ �� ������� ���������.
    /// ��� ������������� ������� ��� ������ ������� ������. ���� false, �� � ������� ����� ���� ��������� ������ �� ������ ��������� ���������</param>
    /// <param name="fromSingleDocTypeName">������������ ��� ���������, �� �������� ������� ������. ���� �� ������, �� ������� ������ �� ���� ����������</param>
    /// <param name="fromSingleDocId">������������� ������������� ���������, �� �������� ������� ������. ���� 0, �� ������� ������ �� ���� ����������</param>
    /// <returns>������� ������</returns>
    public override DataTable GetDocRefTable(string docTypeName, Int32 docId, bool showDeleted, bool unique, string fromSingleDocTypeName, Int32 fromSingleDocId)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return _Source.GetDocRefTable(docTypeName, docId, showDeleted, unique, fromSingleDocTypeName, fromSingleDocId);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    /// <summary>
    /// ���������� ������ ��������������� �������� ����� ��� ������, � ������� �����������
    /// ������������ ��������� � ������� ����, ������������ �� ��� �� �������, ������� ������ ������������
    /// �������. ������������ ������ <paramref name="parentId"/> �� ������ � ������
    /// ����� �� �������������, ���� ��������� ������ �������� (���������).
    /// ��� ���������� ���������� ������������� "������������" ����. ������������ ������ ���� ����, 
    /// � �� ��� ������� ������������. ����� ������� ����� ��������� ��������� ������� ������������.
    /// </summary>
    /// <param name="tableName">��� �������</param>
    /// <param name="parentIdColumnName">��� ���������� �������, �������� "ParentId"</param>
    /// <param name="parentId">������������� ������������ ������. ���� 0, �� ����� ���������� 
    /// �������������� ����� ����� �������� ������ ��� ���� ����� (��� <paramref name="nested"/>=true)</param>
    /// <param name="nested">true, ���� ��������� ����������� �����. false, ���� ��������� ������� ������ ���������������� �������� ��������</param>
    /// <param name="where">�������������� ������. ����� ���� null, ���� ������� ���</param>
    /// <param name="loopedId">���� ������������ ������������� "������������" ����</param>
    /// <returns>������ ��������������� �������� ���������</returns>
    public override IdList GetInheritorIds(string tableName, string parentIdColumnName, Int32 parentId, bool nested, DBxFilter where, out Int32 loopedId)
    {
      CheckThread();

      int RepeatCount = 0;
      while (true)
      {
        try
        {
          return _Source.GetInheritorIds(tableName, parentIdColumnName, parentId, nested, where, out loopedId);
        }
        catch (Exception e)
        {
          CatchException(e, ref RepeatCount);
        }
      }
    }

    #endregion

    #region DBCache

    /// <summary>
    /// ������� ����������� ������.
    /// ���� ������ ��������� ������� DBxDocProvider �������� ������ ��� ���������� �������, ��
    /// �� �������� ����������� ����� DBxCache.
    /// ���� �� ������� DBxDocProvider ��������� �� ������ ��������� � ������� ��� �������������
    /// Remoting, �� ������������ Source.DBCache.
    /// </summary>
    public override DBxCache DBCache
    {
      get
      {
        CheckThread();

        if (_SourceIsRemote)
        {
          if (_DBCache == null)
            _DBCache = new DBxCache(this, CurrentThreadOnly);
          return _DBCache;
        }
        else
          return Source.DBCache;
      }
    }
    private DBxCache _DBCache;


    /// <summary>
    /// ������� ����
    /// </summary>
    public override void ClearCache()
    {
      CheckThread();

      base.ClearCache();
      _Source.ClearCache();
      if (_DBCache != null)
        _DBCache.Clear(); // ������� ���� �������
    }

    #endregion

    #region ������ � �������� ������ � ������

    /// <summary>
    /// ����� ��������� �������� ������, ����������� � DBxRealDocProvider.
    /// ���� ����� �� ������ �������������� � ���������� ����.
    /// </summary>
    /// <param name="tableName">��� ������� ��������� ��� ������������</param>
    /// <param name="columnName">��� ��������� �������, ����������� ������������� �������� ������</param>
    /// <param name="wantedId">������������� ���������, ������������ � ������� ������, ������� ����� ��������</param>
    /// <param name="docVersion">������ ���������. 0 - ������� ������</param>
    /// <param name="preloadIds">�������������� ����������, ������������� � ������� ������,
    /// ������� ���������� ���������</param>
    /// <returns>������� ����������� ������. ���� - ������������� �������� ������. �������� - ����������� ������</returns>
    public override Dictionary<Int32, byte[]> InternalGetBinData2(string tableName, string columnName, DBxDocProvider.DocSubDocDataId wantedId, int docVersion, List<DBxDocProvider.DocSubDocDataId> preloadIds)
    {
      return _Source.InternalGetBinData2(tableName, columnName, wantedId, docVersion, preloadIds);
    }

    /// <summary>
    /// ���������� ����� ��������� ��������� �����
    /// ���� ����� �� ������ �������������� � ���������� ����.
    /// </summary>
    /// <param name="tableName">��� ������� ��������� ��� ������������</param>
    /// <param name="columnName">��� ��������� �������, ����������� ������������� �����</param>
    /// <param name="wantedId">������������� ���������, ������������ � �����, ������� ����� ��������</param>
    /// <param name="docVersion">������ ���������. 0 - ������� ������</param>
    /// <param name="preloadIds">�������������� ����������, ������������� � ������,
    /// ������� ���������� ���������</param>
    /// <returns>������� ����������� ������. ���� - ������������� �����. �������� - ��������� � ������</returns>
    public override Dictionary<Int32, FileContainer> InternalGetDBFile2(string tableName, string columnName, DBxDocProvider.DocSubDocDataId wantedId, int docVersion, List<DBxDocProvider.DocSubDocDataId> preloadIds)
    {
      return _Source.InternalGetDBFile2(tableName, columnName, wantedId, docVersion, preloadIds);
    }

    #endregion

    #region ������ ������

    /// <summary>
    /// ������� ����� ����������-���������, � ����� - ����� DBxChainDocProvider
    /// ���� ����� �������� ����������������
    /// </summary>
    /// <returns></returns>
    public override DBxDocProvider Clone()
    {
      DBxDocProvider Source2 = Source.Clone();
      return new DBxChainDocProvider(Source2.CreateProxy(), CurrentThreadOnly);
    }

    /// <summary>
    /// �� ������������ � ���������������� ����
    /// </summary>
    /// <returns></returns>
    public override DistributedCallData StartServerExecProc(NamedValues args)
    {
      return Source.StartServerExecProc(args);
    }

    #endregion

    #region ����� ������������

    /// <summary>
    /// ��������� ��������, ��������������� ��� �������� �������� UserPermission
    /// ���� �������� �����������, �� �������� UserPermissions � DocPermissions ����������
    /// ��������� ����� ������������ ����������.
    /// </summary>
    public UserPermissionCreators UserPermissionCreators
    {
      get
      {
        CheckThread();

        return _UserPermissionCreators;
      }
      set
      {
        CheckThread();

        if (_UserPermissionCreators != null)
          throw new InvalidOperationException("��������� ��������� �������� �� �����������");
        if (value == null)
          throw new ArgumentNullException();
        _UserPermissionCreators = value;
        _UserPermissionCreators.SetReadOnly();

        DBxChainDocProvider Source2 = Source as DBxChainDocProvider;
        if (Source2 != null && (!SourceIsRemote))
        {
          if (Source2.UserPermissionCreators == null)
            Source2.UserPermissionCreators = value;
        }

        ResetPermissions();
      }
    }
    private UserPermissionCreators _UserPermissionCreators;

    /// <summary>
    /// ����� ���� ������������
    /// </summary>
    public override void ResetPermissions()
    {
      CheckThread();

      if (_UserPermissions != null) // 04.09.2017
      {
        _UserPermissions = null;

        int RepeatCount = 0;
        while (true)
        {
          try
          {
            _Source.ResetPermissions();
            break;
          }
          catch (Exception e)
          {
            CatchException(e, ref RepeatCount);  // 04.09.2017
          }
        }
      }
      base.ResetPermissions();
    }

    /// <summary>
    /// �����, ����������� ������������ (������� ������, ������������ � ���������).
    /// ������ DBxChainDocProvider �������� ����������� ����� ����������
    /// </summary>
    public override UserPermissions UserPermissions
    {
      get
      {
        CheckThread();

        if (_UserPermissions == null)
        {
          if (_UserPermissionCreators == null)
          {
            if (SourceIsRemote)
              return null;
            _UserPermissions = _Source.UserPermissions;
          }
          else
          {
            TempCfg Cfg = new TempCfg();
            int RepeatCount = 0;
            while (true)
            {
              try
              {
                Cfg.AsXmlText = UserPermissionsAsXmlString;
                break;
              }
              catch (Exception e)
              {
                CatchException(e, ref RepeatCount);  // 04.09.2017
              }
            }
            _UserPermissions = new UserPermissions(_UserPermissionCreators);
            _UserPermissions.Read(Cfg);
          }
        }
        return _UserPermissions;
      }
    }
    private UserPermissions _UserPermissions;

    #endregion

    #region ��������� ������

    /// <summary>
    /// ��������� ���������� ���������� � ������ ����������.
    /// ������������ � ������ catch.
    /// </summary>
    /// <param name="e">����������</param>
    public override void AddExceptionInfo(Exception e)
    {
      base.AddExceptionInfo(e);
      e.Data["DBxChainDocProvider.SourceIsRemote"] = SourceIsRemote;
    }

    /// <summary>
    /// ������� ���������� ��� ������������� ���������� ��� ������ ������ � ������� ���������� (Source).
    /// ���������������� ���������� ����� ��������� ���������� �, ���� ��� ������� � �����, �����������
    /// ������������ ���������� � ��������. ����� ����� ������� ���������� �������� Retry � ��������� �������.
    /// ��� ������������� ExtDBDocForms.dll ������� ��������� ���������� ������� � DBUI, � �� �����.
    /// </summary>
    /// <remarks>
    /// ��������������.
    /// ���� ���� ������� DBxChainDocProvider � ��������������� ���������� ������� �� ������� �������,
    /// �� ���� ���������� �� ����� ���� �������������� � ���������� ���������� � �������, ����� ��������
    /// ������������ Net Remoting ��� ���������� �� ��������� �����������.
    /// </remarks>
    public event DBxRetriableExceptionEventHandler ExceptionCaught
    {
      add
      {
        if (SourceIsRemote)
          _ExceptionCaught += value;
        else if (Source is DBxChainDocProvider)
          ((DBxChainDocProvider)Source).ExceptionCaught += value;
        else
          _ExceptionCaught += value;
      }
      remove
      {
        if (SourceIsRemote)
          _ExceptionCaught -= value;
        else if (Source is DBxChainDocProvider)
          ((DBxChainDocProvider)Source).ExceptionCaught -= value;
        else
          _ExceptionCaught -= value;
      }
    }

    private event DBxRetriableExceptionEventHandler _ExceptionCaught;

    /// <summary>
    /// ����� ����������, ���� ��� ������ ������ � Source �������� ����������.
    /// ���� ���������� ������� � �����, ����� ������ ���������� ������������ ����������� � �������
    /// � ������� true, ����� ��������� ������� ������� ��� ���.
    /// ��� ���������� ������ ���� ��������, �.�. ����� ���������� ��� ����� ����������, �� �����������
    /// ��������� � ������� ����������
    /// </summary>
    /// <param name="exception">��������� ����������</param>
    /// <param name="repeatCount">������� ��������. ��� ������ ������ ����� 0, ����� 1 � �.�.
    /// ����������� ������ ������ � �������� ������ ������� ��������.
    /// ���������������� ����� �����, ��������, ��������� ������ ������������ ���������� ������� ��������������</param>
    /// <returns>true, ���� ������� ��������� ��������</returns>
    protected bool OnExceptionCaught(Exception exception, int repeatCount)
    {
      if (!SourceIsRemote)
      {
        if (Source is DBxChainDocProvider)
          return ((DBxChainDocProvider)Source).OnExceptionCaught(exception, repeatCount);
      }

      if (_ExceptionCaught == null)
        return false;

      DBxRetriableExceptionEventArgs Args = new DBxRetriableExceptionEventArgs(exception, repeatCount);
      _ExceptionCaught(this, Args);

      return Args.Retry;
    }

    private void CatchException(Exception exception, ref int repeatCount)
    {
      AddExceptionInfo(exception);
      if (SourceIsRemote)
      {
        try
        {
          exception.Data["DBxChainDocProvider"] = this.ToString();
        }
        catch { }
        exception.Data["DBxChainDocProvider.GetType()"] = this.GetType().ToString();
        exception.Data["DBxChainDocProvider.CatchException:RepeatCount"] = repeatCount;
      }

      Stopwatch sw = null; // 17.02.2021
      if (SourceIsRemote)
        sw = Stopwatch.StartNew();
      if (!OnExceptionCaught(exception, repeatCount))
      {
        if (SourceIsRemote)
        {
          sw.Stop();
          exception.Data["ExceptionCaughtHandleTime"] = sw.Elapsed;
        }
        throw exception;
      }

      repeatCount++;
    }

    #endregion

    #region ������� ����� �������
#if XXX
    public override DateTime ServerTime
    {
      get { return DateTime.Now+ServerTimeDiff; }
    }

    /// <summary>
    /// ���������� �������� ����� �������� ������� � ������� ��������
    /// </summary>
    private TimeSpan ServerTimeDiff;
#endif
    #endregion
  }
}
