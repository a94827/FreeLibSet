#if XXXX // ���� �� ����, ��� ������������
using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using System.Data;
using System.Runtime.Serialization;


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

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// ��������� ������� "DataVersion" � ���� ������ ����������.
  /// ������������� ������������� ���� ������ � ������ ����������.
  /// ������ ������ ������, ������������� downgrade ��������� �� ������������� ������.
  /// 
  /// </summary>
  public class DBxDataVersionHandler
  {
    #region �����������

    public DBxDataVersionHandler(Guid AppGuid, int CurrentVersion, int MinVersion)
    {
      if (AppGuid == Guid.Empty)
        throw new ArgumentException("�� ����� AppGuid");
      if (CurrentVersion < 1)
        throw new ArgumentException("������ ������ ���� �� ������ 1", "CurrentVersion");
      if (MinVersion < 1)
        throw new ArgumentException("����������� ������ ������ ���� �� ������ 1", "MinVersion");
      if (MinVersion > CurrentVersion)
        throw new ArgumentException("����������� ������ �� ����� ���� ������ �������� ������", "MinVersion");

      FAppGuid = AppGuid;
      FCurrentVersion = CurrentVersion;
      FMinVersion = CurrentVersion;
    }

    #endregion

    #region ��������

    public Guid AppGuid { get { return FAppGuid; } }
    private Guid FAppGuid;

    public int CurrentVersion { get { return FCurrentVersion; } }
    private int FCurrentVersion;

    public int MinVersion { get { return FMinVersion; } }
    private int FMinVersion;

    #endregion

    #region ���������� ��������� �������

    public DBxTableStruct AddTableStruct(DBxStruct DBStruct)
    {
      DBxTableStruct ts = new DBxTableStruct("DataVersion");
      ts.Columns.AddId(); // �� ������������
      ts.Columns.AddString("AppGUID", 36, false);
      ts.Columns.AddString("DataGUID", 36, false);
      ts.Columns.AddInt("CurrentVersion", 1, Int32.MaxValue);
      ts.Columns.AddInt("MinVersion", 1, Int32.MaxValue);
      DBStruct.Tables.Add(ts);
      return ts;
    }

    #endregion

    #region �������������

    public Guid DataGuid { get { return FDataGuid; } }
    private Guid FDataGuid;

    public int PrevVersion { get { return FPrevVersion; } }
    private int FPrevVersion;

    public void InitTableRow(DBxCon Con)
    {
      DataTable Table = Con.FillSelect("DataVersion");
      if (Table.Rows.Count == 0)
      {
        // ������ ������

        FDataGuid = Guid.NewGuid();
        Con.AddRecord("DataVersion", new DBxColumns("AppGUID,DataGUID,CurrentVersion,MinVersion"),
          new object[] { AppGuid.ToString(), FDataGuid.ToString(), CurrentVersion, MinVersion });
      }
      else if (Table.Rows.Count > 1)
        throw new DBxDataVersionHandlerException("������� DataVersion �������� ������������ ����� �����: " + Table.Rows.Count.ToString());
      else
      {
        // ��������� ������

        DataRow Row = Table.Rows[0];
        Guid OldAppGuid = new Guid(DataTools.GetString(Row, "AppGUID"));
        if (OldAppGuid != AppGuid)
          throw new DBxDataVersionHandlerException("���� ������ ������������� ��� ������ � ������ ����������");

        FDataGuid = new Guid(DataTools.GetString(Row, "DataGUID"));
        FPrevVersion = DataTools.GetInt(Row, "CurrentVersion");

        int OldMinVersion = DataTools.GetInt(Row, "MinVersion");
        if (CurrentVersion < OldMinVersion)
          throw new DBxDataVersionHandlerException("���� ������ ���� ��������� � ����� ����� ������ ���������. ����� �� ������� ������ ����������");

        if (CurrentVersion != FPrevVersion)
        {
          Int32 DummyId = DataTools.GetInt(Row, "Id");
          Con.SetValues("DataVersion", DummyId, new DBxColumns("CurrentVesrion,MinVersion"),
            new object[] { CurrentVersion, MinVersion });
        }
      }
    }

    #endregion
  }

  [Serializable]
  public class DBxDataVersionHandlerException : ApplicationException
  {
    #region �����������

    public DBxDataVersionHandlerException(string Message)
      : base(Message)
    {
    }

    /// <summary>
    /// ��� ������ ������������ ����� ��� ���������� ��������������
    /// </summary>
    protected DBxDataVersionHandlerException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }
}
#endif