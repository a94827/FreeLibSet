// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data.Docs;
using System.Data;
using FreeLibSet.Data;
using FreeLibSet.Remoting;
using FreeLibSet.Logging;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Forms.Docs
{

  /// <summary>
  /// ���������� DocProvider ��� ������������� � DBUI
  /// ��������� ���������� ��������� ���������� � ����� �� ApplyChanges()
  /// </summary>
  public sealed class DocProviderUI : DBxChainDocProvider
  {
    #region ���������� �����������

    internal DocProviderUI(DBxDocProviderProxy sourceProxy, DBUI ui)
      : base(sourceProxy, true)
    {
      _UI = ui;

      base.ExceptionCaught += new FreeLibSet.Data.Docs.DBxRetriableExceptionEventHandler(DocProviderUI_ExceptionCaught);
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ������� � ����������, � �������� ��������� ���������
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    #endregion

    #region ���������� ������ ��� ������ ��������

    private string GetTableImageKey(string tableName)
    {
      if (String.IsNullOrEmpty(tableName))
        return "Table";
      DocTypeUIBase dtui = UI.DocTypes.FindByTableName(tableName);
      if (dtui == null)
        return "Table";
      else
        return dtui.ImageKey;
    }

    private void BeginWait(string message, string imageKey)
    {
      if (_UI.ShowWaitMessages)
        EFPApp.BeginWait(message, imageKey);
    }

    private void EndWait()
    {
      if (_UI.ShowWaitMessages)
        EFPApp.EndWait();
    }

    #endregion

    #region ��������������� ������� ��� ������ ��������

    /// <summary>
    /// ���������� SQL-������� SELECT � �������� ���� ��������� ����������
    /// </summary>
    /// <param name="info">��������� ��� �������</param>
    /// <returns>������� ������</returns>
    public override DataTable FillSelect(DBxSelectInfo info)
    {
      DataTable table;
      BeginWait("������ ������� \"" + info.TableName + "\"", GetTableImageKey(info.TableName));
      try
      {
        // 29.08.2020
        // ���������� ��������� �� ������ ����������� ������

        NamedValues DispArgs = new NamedValues();
        DispArgs["Action"] = "FillSelect";
        DispArgs["SelectInfo"] = info;
        NamedValues DispRes = ExecuteServerAsync(DispArgs, "SELECT ��� ������� \"" + info.TableName + "\"");
        table = (DataTable)(DispRes["Table"]);
      }
      finally
      {
        EndWait();
      }
      return table;
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
      DataTable table;
      BeginWait("��������� ������ �������� ��� ������� \"" + tableName + "\"", GetTableImageKey(tableName));
      try { table = base.FillUniqueColumnValues(tableName, columnName, where); }
      finally { EndWait(); }
      return table;
    }

    /// <summary>
    /// �������� ������� ������� ��� ���������
    /// </summary>
    /// <param name="docTypeName">��� ������� ���������</param>
    /// <param name="docId">������������� ���������</param>
    /// <returns>������� �������</returns>
    public override DataTable GetDocHistTable(string docTypeName, Int32 docId)
    {
      DataTable table;
      BeginWait("��������� ������� ������� ���������", "Information");
      try { table = base.GetDocHistTable(docTypeName, docId); }
      finally { EndWait(); }
      return table;
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
      DataTable table;
      BeginWait("��������� ������� ������ ��� ���������", "DocRefs");
      try { table = base.GetDocRefTable(docTypeName, docId, showDeleted, unique, fromSingleDocTypeName, fromSingleDocId); }
      finally { EndWait(); }
      return table;
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
      DataTable table;
      BeginWait("��������� ������ �������� ������������", "UserActions");
      try { table = base.GetUserActionsTable(firstDate, lastDate, userId, singleDocTypeName); }
      finally { EndWait(); }
      return table;
    }

    /// <summary>
    /// �������� ������� ���������� ��� ������ �������� ������������
    /// </summary>
    /// <param name="actionId">������������� �������� � ������� UserActions</param>
    /// <returns>������� ����������</returns>
    public override DataTable GetUserActionDocTable(Int32 actionId)
    {
      DataTable table;
      BeginWait("��������� ������� ���������� ��� �������� ������������", "UserActions");
      try { table = base.GetUserActionDocTable(actionId); }
      finally { EndWait(); }
      return table;
    }

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
      Dictionary<Int32, byte[]> res;
      BeginWait("��������� �������� ������", GetTableImageKey(tableName));
      try
      {
        // 14.10.2020
        // ���������� ��������� �� ������ ����������� ������
        //res = base.InternalGetBinData2(tableName, columnName, wantedId, docVersion, preloadIds); 
        NamedValues DispArgs = new NamedValues();
        DispArgs["Action"] = "InternalGetBinData2";
        DispArgs["TableName"] = tableName;
        DispArgs["ColumnName"] = columnName;
        DispArgs["WantedId"] = wantedId;
        DispArgs["DocVersion"] = docVersion;
        DispArgs["PreloadIds"] = preloadIds;
        NamedValues DispRes = ExecuteServerAsync(DispArgs, "��������� �������� ������");
        res = (Dictionary<Int32, byte[]>)(DispRes["Data"]);
      }
      finally
      {
        EndWait();
      }
      return res;
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
    public override Dictionary<Int32, FreeLibSet.IO.FileContainer> InternalGetDBFile2(string tableName, string columnName, DBxDocProvider.DocSubDocDataId wantedId, int docVersion, List<DBxDocProvider.DocSubDocDataId> preloadIds)
    {
      Dictionary<Int32, FreeLibSet.IO.FileContainer> res;
      BeginWait("��������� ����� �� ���� ������", GetTableImageKey(tableName));
      try
      {
        // 14.10.2020
        // ���������� ��������� �� ������ ����������� ������
        //res = base.InternalGetDBFile2(tableName, columnName, wantedId, docVersion, preloadIds);
        NamedValues DispArgs = new NamedValues();
        DispArgs["Action"] = "InternalGetDBFile2";
        DispArgs["TableName"] = tableName;
        DispArgs["ColumnName"] = columnName;
        DispArgs["WantedId"] = wantedId;
        DispArgs["DocVersion"] = docVersion;
        DispArgs["PreloadIds"] = preloadIds;
        NamedValues DispRes = ExecuteServerAsync(DispArgs, "��������� ����������� �����");
        res = (Dictionary<Int32, FreeLibSet.IO.FileContainer>)(DispRes["Data"]);
      }
      finally
      {
        EndWait();
      }
      return res;
    }

    /// <summary>
    /// ��������� �������� ������� ����
    /// </summary>
    /// <param name="request">��������� �������</param>
    /// <returns>��� ��������</returns>
    public override DBxCacheLoadResponse LoadCachePages(DBxCacheLoadRequest request)
    {
      DBxCacheLoadResponse res;
      BeginWait("��������� ������������ ������� ��� " + request.ToString(), "Database");
      try { res = base.LoadCachePages(request); }
      finally { EndWait(); }
      return res;
    }

    /// <summary>
    /// ��������� ��������� (��� �������������)
    /// </summary>
    /// <param name="docTypeName">��� ������� ����������</param>
    /// <param name="filter">������� ��� ������ ����������</param>
    /// <returns>������� ����������</returns>
    public override DataTable LoadDocData(string docTypeName, DBxFilter filter)
    {
      DataTable table;
      BeginWait("�������� ���������� \"" + DocTypes[docTypeName].PluralTitle + "\"", GetTableImageKey(docTypeName));
      try { table = base.LoadDocData(docTypeName, filter); }
      finally { EndWait(); }
      return table;
    }

    /// <summary>
    /// ��������� ��������� (��� �������������)
    /// </summary>
    /// <param name="docTypeName">��� ������� ����������</param>
    /// <param name="docIds">������ ���������������</param>
    /// <returns>������� ����������</returns>
    public override DataTable LoadDocData(string docTypeName, Int32[] docIds)
    {
      DataTable table;
      BeginWait("�������� ���������� \"" + DocTypes[docTypeName].PluralTitle + "\" �� �������", GetTableImageKey(docTypeName));
      try { table = base.LoadDocData(docTypeName, docIds); }
      finally { EndWait(); }
      return table;
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
      DataTable table;
      BeginWait("�������� �������������", GetTableImageKey(subDocTypeName));
      try { table = base.LoadSubDocData(docTypeName, subDocTypeName, docIds); }
      finally { EndWait(); }
      return table;
    }

    #endregion

    #region OnApplyChanges()

    /// <summary>
    /// ���������� ���������.
    /// ����������� ��������, ��������� � �������� ���������� � �������������.
    /// ���� <paramref name="reloadData"/>=true, �� ����������� ���������� �������� ����������
    /// ���������� �������� DocumnetViewHandler.ApplyChanges()
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
      DataSet res;
      BeginWait("������ ��������� � ����������", "Save");
      try { res = DoOnApplyChanges(dataSet, reloadData); }
      finally { EndWait(); }
      return res;
    }

    private DataSet DoOnApplyChanges(DataSet dataSet, bool reloadData)
    {
      if (dataSet == null)
        throw new ArgumentNullException("dataSet");

      bool UseAsyncCall = DataTools.GetBool(dataSet.ExtendedProperties["UseAsyngWriting"]);

      DataSet DataSet2;
      if (UseAsyncCall)
      {
        // ����������� �����
        NamedValues DispArgs = new NamedValues();
        DispArgs["Action"] = "ApplyChanges";
        DispArgs["DataSet"] = dataSet;
        DispArgs["ReloadData"] = reloadData;
        NamedValues DispRes = ExecuteServerAsync(DispArgs, "������ ��������� ����������");
        DataSet2 = (DataSet)(DispRes["DataSet"]);
      }
      else
      {
        // ���������� �����, ������� ������������ ���� �����
        DataSet2 = base.OnApplyChanges(dataSet, reloadData);
      }

      // ����� ����������� ��������� ���������� �� ����, ����������� �� ������� ����� �� �������
      // (�������� ReloadData)
      // ����� ���� ����������� �� ���������� ��������� ����������
      DBxDocProvider.ClearCache(dataSet, UI.TextHandlers.DBCache);

      if (reloadData)
      {
        if (DataSet2 != null)
          AfterApplyChanges(DataSet2);
        else
        {
          try
          {
            NullReferenceException e = new NullReferenceException("��� ������ OnApplyChanges() �� ������� �������� ����� ������ ��� ReloadData=true");
            e.Data["UseAsyncCall"] = UseAsyncCall;
          }
          catch (Exception e2)
          {
            LogoutTools.LogoutException(e2, "DocProviderUI.OnApplyChanges()");
          }
        }
      }

      return DataSet2;
    }

    private void AfterApplyChanges(DataSet dataSet2)
    {
      SingleScopeList<DocTypeUI> DocTypeUIs = new SingleScopeList<DocTypeUI>();
      foreach (DataTable Table in dataSet2.Tables)
      {
        DocTypeUI DocTypeUI;
        SubDocTypeUI SubDocTypeUI;
        if (!_UI.DocTypes.FindByTableName(Table.TableName, out DocTypeUI, out SubDocTypeUI))
          continue; // �������������

        DocTypeUIs.Add(DocTypeUI);
      }

      // ���������� ��������� ����������
      Guid BrowserGuid;
      if (DocumentViewHandler.CurrentHandler == null)
        BrowserGuid = Guid.Empty;
      else
        BrowserGuid = DocumentViewHandler.CurrentHandler.BrowserGuid;

      foreach (DocTypeUI DocTypeUI in DocTypeUIs)
      {
        for (int i = 0; i < DocTypeUI.Browsers.Count; i++)
        {
          DocumentViewHandler dvh=DocTypeUI.Browsers[i];
          bool IsCaller = (dvh.BrowserGuid == BrowserGuid) && BrowserGuid != Guid.Empty;
          try
          {
            dvh.ApplyChanges(dataSet2, IsCaller);
          }
          catch (Exception e) // 17.06.2019
          {
            e.Data["DocumentViewHandler.GetType()"] = dvh.GetType().ToString();
            e.Data["DocumentViewHandler"] = dvh.ToString();
            e.Data["BrowserGuid"] = dvh.BrowserGuid;
            e.Data["IsCaller"] = IsCaller;
            EFPApp.ShowException(e, "������ ���������� ��������� ����� ������ ��������� ��� ���������� \"" + DocTypeUI.DocType.PluralTitle+"\"");
          }
        }
      }
      //DebugTools.DebugDataSet(DataSet, "�����������");
    }

    #endregion

    #region ����������� ����������

    /// <summary>
    /// �������� ��������� ���������, ���������� CreateServerExecProc()
    /// </summary>
    /// <param name="dispArgs">��������� ������</param>
    /// <param name="displayName">������������ ��� � ������ ��������</param>
    /// <returns>����������</returns>
    private NamedValues ExecuteServerAsync(NamedValues dispArgs, string displayName)
    {
      DistributedCallData startData = base.StartServerExecProc(dispArgs);

      DistributedProcCallItem CallItem = new DistributedProcCallItem(startData);
      CallItem.DisplayName = displayName;
      NamedValues dispRes = EFPApp.ExecProcList.ExecuteAsyncAndWait(CallItem);
      return dispRes;
    }

    #endregion

    #region ������

    /// <summary>
    /// ���������� ��������� ������������� ��������� ��� ������������,
    /// ��������� DBUI.TextHandlers
    /// </summary>
    /// <param name="tableName">��� ������� ��������� ��� ������������</param>
    /// <param name="id">�������������</param>
    /// <returns>��������� �������������</returns>
    public override string GetTextValue(string tableName, Int32 id)
    {
      return _UI.TextHandlers.GetTextValue(tableName, id);
    }

    #endregion

    #region ��������� ����������

    void DocProviderUI_ExceptionCaught(object sender, FreeLibSet.Data.Docs.DBxRetriableExceptionEventArgs args)
    {
      System.Net.Sockets.SocketException se = LogoutTools.GetException<System.Net.Sockets.SocketException>(args.Exception);
      if (se != null)
      {
        if (se.SocketErrorCode == System.Net.Sockets.SocketError.TimedOut)
        {
          if (args.RepeatCount > 1)
            return; // 21.08.2020
        }
      }

      _UI.OnExceptionCaught(args);
    }

    #endregion
  }
}
