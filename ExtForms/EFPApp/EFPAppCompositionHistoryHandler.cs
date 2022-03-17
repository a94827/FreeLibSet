// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using FreeLibSet.Config;
using System.Data;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  #region ������������ EFPAppCompositionHistoryItemKind

  /// <summary>
  /// ��������� �������� �������� EFPAppCompositionHistoryItem.Kind
  /// </summary>
  public enum EFPAppCompositionHistoryItemKind
  {
    /// <summary>
    /// ������������� ��������� ������ � ������ �������
    /// </summary>
    History,

    /// <summary>
    /// ������� ������, ��������� �������������
    /// </summary>
    User,
  }

  #endregion

  /// <summary>
  /// ���������� � ����������� ���������� ����������������� ���������� � ������ �������.
  /// ������� ��������� � ������ EFPAppCompositionHistoryHandler
  /// </summary>
  public sealed class EFPAppCompositionHistoryItem
  {
    #region ���������� �����������

    internal EFPAppCompositionHistoryItem(EFPAppCompositionHistoryItemKind kind,
      string userSetName, string displayName, DateTime time, string md5)
    {
#if DEBUG
      //if (String.IsNullOrEmpty(UserSetName))
      //  throw new ArgumentNullException("UserSetName");
      if (String.IsNullOrEmpty(displayName))
        throw new ArgumentNullException("displayName");
      if (md5.Length != 32)
        throw new ArgumentException("MD5", "md5");
#endif

      _Kind = kind;
      _UserSetName = userSetName;
      _DisplayName = displayName;
      _Time = time;
      _MD5 = md5;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ����������: �� ������ ������� ��� ������� ����������������
    /// </summary>
    public EFPAppCompositionHistoryItemKind Kind { get { return _Kind; } }
    private EFPAppCompositionHistoryItemKind _Kind;

    /// <summary>
    /// ���������� ���, �������� "Hist1", "User1".
    /// ����� ���� ������ ������� ��� ��������� ����������� ����������
    /// </summary>
    public string UserSetName { get { return _UserSetName; } }
    private string _UserSetName;

    /// <summary>
    /// ������������ ���.
    /// ��� �������� ���������������� ���������� - �������� ������������� ���.
    /// ��� ���������� �� ������� - ����� � ���� "���������", "�������������", ...
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private string _DisplayName;

    /// <summary>
    /// ����� ���������� ����������
    /// </summary>
    public DateTime Time { get { return _Time; } }
    private DateTime _Time;

    /// <summary>
    /// ����������� �����, ������������ ��� ��������� ����������
    /// </summary>
    public string MD5 { get { return _MD5; } }
    private string _MD5;

    /// <summary>
    /// ���������� �������� DisplayName.
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return DisplayName;
    }

    #endregion
  }

  /// <summary>
  /// ��������������� ����� ��� �������� ������� ���������� ���������� ����������������� ����������
  /// � ������� ����������� ����������.
  /// ������������ � EFPApp.SaveComposition(), � ������� ������ ����������� ����������.
  /// ����� ����� �������������� � ���������������� ����.
  /// </summary>
  public static class EFPAppCompositionHistoryHandler
  {
    #region ������

    /// <summary>
    /// ���������� ������ ��������� ����������� ����������.
    /// ������ ������������ �� ����� � ������
    /// </summary>
    /// <returns>������ �������� ����������</returns>
    public static EFPAppCompositionHistoryItem[] GetHistoryItems()
    {
      // ��������� ������������� ������� ��� ���������� �������
      DataTable table = new DataTable();
      table.Columns.Add("UserSetName", typeof(string)); // "Hist1", "Hist2", ...
      table.Columns.Add("Time", typeof(DateTime));
      table.Columns.Add("MD5", typeof(string));
      DataTools.SetPrimaryKey(table, "UserSetName");

      #region ������ ������

      EFPConfigSectionInfo configInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIHistory, String.Empty);
      CfgPart cfgHist;
      using (EFPApp.ConfigManager.GetConfig(configInfoHist, EFPConfigMode.Read, out cfgHist))
      {
        string[] names = cfgHist.GetChildNames();

        for (int i = 0; i < names.Length; i++)
        {
          if (names[i].StartsWith("Hist"))
          {
            CfgPart cfgOne = cfgHist.GetChild(names[i], false);
            /*DataRow Row = */ table.Rows.Add(names[i], cfgOne.GetDateTime("Time"),
              cfgOne.GetString("MD5"));
          }
        }
      }

      #endregion

      table.DefaultView.Sort = "Time DESC"; // �� ��������

      #region �������� �������

      EFPAppCompositionHistoryItem[] items = new EFPAppCompositionHistoryItem[table.DefaultView.Count];
      for (int i = 0; i < table.DefaultView.Count; i++)
      {
        DataRow row = table.DefaultView[i].Row;
        items[i] = CreateHistItem(i, row["UserSetName"].ToString(),
          (DateTime)(row["Time"]),
          row["MD5"].ToString());
      }

      #endregion

      return items;
    }

    private static EFPAppCompositionHistoryItem CreateHistItem(int itemIndex, string userSetName, DateTime time, string md5)
    {
      string prefix;
      switch (itemIndex)
      {
        case 0: prefix = "���������"; break;
        case 1: prefix = "�������������"; break;
        default: prefix = "���������� �" + (itemIndex + 1).ToString(); break;
      }

      return new EFPAppCompositionHistoryItem(EFPAppCompositionHistoryItemKind.History,
        userSetName,
        "[ " + prefix + " " + time.ToString("g") + " ]",
        time, md5);
    }

    /// <summary>
    /// ���������� ������ ������� ����������.
    /// ������ ������������ �� �������� (�������� DisplayName)
    /// </summary>
    /// <returns>������ �������� ����������</returns>
    public static EFPAppCompositionHistoryItem[] GetUserItems()
    {
      // ��������� ������������� ������� ��� ���������� �������
      DataTable table = new DataTable();
      table.Columns.Add("UserSetName", typeof(string)); // "User1", "User2", ...
      table.Columns.Add("Name", typeof(string)); // ���, �������� �������������
      table.Columns.Add("Time", typeof(DateTime));
      table.Columns.Add("MD5", typeof(string));
      DataTools.SetPrimaryKey(table, "UserSetName");

      #region ������ ������

      EFPConfigSectionInfo configInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIUserHistory, String.Empty);
      CfgPart cfgHist;
      using (EFPApp.ConfigManager.GetConfig(configInfoHist, EFPConfigMode.Read, out cfgHist))
      {
        string[] names = cfgHist.GetChildNames();

        for (int i = 0; i < names.Length; i++)
        {
          if (names[i].StartsWith("User"))
          {
            CfgPart cfgOne = cfgHist.GetChild(names[i], false);
            /*DataRow Row = */table.Rows.Add(names[i], cfgOne.GetString("Name"),
              cfgOne.GetDateTime("Time"),
              cfgOne.GetString("MD5"));
          }
        }
      }

      #endregion

      table.DefaultView.Sort = "Name";

      #region �������� �������

      EFPAppCompositionHistoryItem[] items = new EFPAppCompositionHistoryItem[table.DefaultView.Count];
      for (int i = 0; i < table.DefaultView.Count; i++)
      {
        DataRow row = table.DefaultView[i].Row;
        items[i] = CreateUserItem(row["UserSetName"].ToString(),
          row["Name"].ToString(),
          (DateTime)(row["Time"]),
          row["MD5"].ToString());
      }

      #endregion

      return items;
    }

    private static EFPAppCompositionHistoryItem CreateUserItem(string userSetName, string name, DateTime time, string md5)
    {
      return new EFPAppCompositionHistoryItem(EFPAppCompositionHistoryItemKind.User,
        userSetName, name, time, md5);
    }

    /// <summary>
    /// ���������� �������� ��� ���������������� ���������� � �������� ������.
    /// ���� ��� <paramref name="name"/> �� ������ ��� ��� ���������������� ������������ � �����
    /// ������ (��������, ���� �������), ���������� null.
    /// </summary>
    /// <param name="name">��� ������������, �������� �������������. �� ������ � UserSetName</param>
    /// <returns>��������� �������� ��� null</returns>
    public static EFPAppCompositionHistoryItem GetUserItem(string name)
    {
      if (String.IsNullOrEmpty(name))
        return null;

      #region ������ ������

      EFPAppCompositionHistoryItem item = null;

      EFPConfigSectionInfo configInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIUserHistory, String.Empty);
      CfgPart cfgHist;
      using (EFPApp.ConfigManager.GetConfig(configInfoHist, EFPConfigMode.Read, out cfgHist))
      {
        string[] names = cfgHist.GetChildNames();

        for (int i = 0; i < names.Length; i++)
        {
          if (names[i].StartsWith("User"))
          {
            CfgPart cfgOne = cfgHist.GetChild(names[i], false);
            string UserName = cfgOne.GetString("Name");
            if (UserName == name)
            {
              item = CreateUserItem(names[i], UserName, cfgOne.GetDateTime("Time"),
                cfgOne.GetString("MD5"));
              break;
            }
          }
        }
      }

      #endregion

      return item;
    }

    /// <summary>
    /// �������� �������� ������ ������������ ��� ��������� ����������� ����������.
    /// ���������� ������ ����� ���� ������� ������ EFPApp.ConfigManager.GetConfig() ��� �������� ������
    /// </summary>
    /// <param name="item">����������� ������� �������</param>
    /// <returns>���, ��������� � ���������������� ����� ������ ������������</returns>
    public static EFPConfigSectionInfo GetConfigInfo(EFPAppCompositionHistoryItem item)
    {
      switch (item.Kind)
      {
        case EFPAppCompositionHistoryItemKind.History:
          return new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
            EFPConfigCategories.UI, item.UserSetName);
        case EFPAppCompositionHistoryItemKind.User:
          return new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
            EFPConfigCategories.UIUser, item.UserSetName);
        default:
          throw new ArgumentException("����������� Kind=" + item.Kind.ToString(), "item");
      }
    }


    /// <summary>
    /// �������� �������� ������ ������������ �� ������� ��� ��������� ����������� ����������.
    /// ���������� ������ ����� ���� ������� ������ EFPApp.ConfigManager.GetConfig() ��� �������� ������
    /// </summary>
    /// <param name="item">����������� ������� �������</param>
    /// <returns>���, ��������� � ���������������� ����� ������ ������������</returns>
    public static EFPConfigSectionInfo GetSnapshotConfigInfo(EFPAppCompositionHistoryItem item)
    {
      switch (item.Kind)
      {
        case EFPAppCompositionHistoryItemKind.History:
          return new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
            EFPConfigCategories.UISnapshot, item.UserSetName);
        case EFPAppCompositionHistoryItemKind.User:
          return new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
            EFPConfigCategories.UIUserSnapshot, item.UserSetName);
        default:
          throw new ArgumentException("����������� Kind=" + item.Kind.ToString(), "item");
      }
    }

    /// <summary>
    /// ��������� ���������� ����, ���������� ������� EFPAppInterface.SaveComposition().
    /// ��� ������ ����������� ������� ������ � ����� MD5. ���� ��� ���� ����� ������, �� ���
    /// "������������" ������ ������.
    /// ���� ����� ���������� � EFPApp.SaveComposition().
    /// </summary>
    /// <param name="cfg">���������� ������</param>
    /// <param name="snapshot">�����������</param>
    /// <returns>�������� ����������</returns>
    public static EFPAppCompositionHistoryItem SaveHistory(CfgPart cfg, Bitmap snapshot)
    {
      if (cfg == null)
        throw new ArgumentNullException("cfg");
      if (cfg.IsEmpty)
        throw new ArgumentException("���������� �� ��������", "cfg");

      if (EFPApp.CompositionHistoryCount == 0)
        throw new InvalidOperationException("EFPApp.CompositionHistoryCount=0");

      string md5 = cfg.MD5Sum();

      string userSetName = null; // �������� ������ ��� ������ �������. 
      // ���� �� ���� ��������� ������, �������� null.

      EFPAppCompositionHistoryItem item = null;

      #region ������ UIHistory

      EFPConfigSectionInfo configInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIHistory, String.Empty);
      CfgPart cfgHist;
      int currentIndex = -1; // ����� ���������� �� MD5
      using (EFPApp.ConfigManager.GetConfig(configInfoHist, EFPConfigMode.Write, out cfgHist))
      {
        string[] names = cfgHist.GetChildNames();

        DataTable table = new DataTable();
        table.Columns.Add("UserSetName", typeof(string)); // "Hist1", "Hist2", ...
        table.Columns.Add("Time", typeof(DateTime));
        table.Columns.Add("MD5", typeof(string));
        DataTools.SetPrimaryKey(table, "UserSetName");

        DateTime time = DateTime.Now;

        for (int i = 0; i < names.Length; i++)
        {
          if (names[i].StartsWith("Hist"))
          {
            CfgPart cfgOne = cfgHist.GetChild(names[i], false);
            DataRow row = table.Rows.Add(names[i], cfgOne.GetDateTime("Time"),
              cfgOne.GetString("MD5"));
            if (row["MD5"].ToString() == md5)
            {
              currentIndex = i;
              cfgOne.SetDateTime("Time", time); // ������ ��������
              item = CreateHistItem(0, names[i], time, md5);
            }
            // ���������� ��� break;
          }
        }

        if (currentIndex < 0)
        {
          // ��������� �������� ����� ������ � �������
          // ������� ����� ������ ������
          table.DefaultView.Sort = "Time"; // �� �����������
          while (table.DefaultView.Count >= EFPApp.CompositionHistoryCount)
            table.DefaultView[0].Row.Delete();

          // ����������� ����� ��� ������
          for (int i = 1; i <= EFPApp.CompositionHistoryCount; i++)
          {
            string UserSetName2 = "Hist" + i.ToString();
            if (table.Rows.Find(UserSetName2) == null)
            {
              userSetName = UserSetName2;
              break;
            }
          }

          if (userSetName == null)
            throw new BugException("�� ����� ����� ������ ��� ������ �������");
          table.Rows.Add(userSetName, DateTime.Now, md5);
          table.AcceptChanges();

          // ������ ���������� ��� ������
          cfgHist.Clear();
          foreach (DataRow row in table.Rows)
          {
            string userSetName2 = row["UserSetName"].ToString();
            CfgPart cfgOne = cfgHist.GetChild(userSetName2, true);
            cfgOne.SetDateTime("Time", (DateTime)(row["Time"]));
            cfgOne.SetString("MD5", row["MD5"].ToString());
          }

          item = CreateHistItem(0, userSetName, time, md5);
        } // CurrentIndex<0
      } // SectHist

      #endregion

      #region ������ UI ��� �������

      if (userSetName != null)
      {
        EFPConfigSectionInfo configInfo2 = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
          EFPConfigCategories.UI, userSetName);
        CfgPart cfg2;
        using (EFPApp.ConfigManager.GetConfig(configInfo2, EFPConfigMode.Write, out cfg2))
        {
          cfg2.Clear();
          cfg.CopyTo(cfg2);
        }
      }

      #endregion

      #region Snapshot

      // ���������� Snapshot, ������ ���� ������ �����
      if (userSetName != null)
      {
        try
        {
          EFPConfigSectionInfo configInfoSnapshot = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
            EFPConfigCategories.UISnapshot, userSetName);
          EFPApp.SnapshotManager.SaveSnapshot(configInfoSnapshot, snapshot);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "�� ������� ��������� ����������� ��� ���������������� ���������");
        }
      }

      #endregion

#if DEBUG
      if (item == null)
        throw new BugException("Item=null");
#endif

      return item;
    }

    /// <summary>
    /// ���������� ������� ��������������� ����������.
    /// ���� ���������� � ����� ������ ��� ����������, ��� ����������������
    /// </summary>
    /// <param name="name">��� ����������, �������� �������������. ������ ���� ������ �����������</param>
    /// <param name="cfg">���������� ������</param>
    /// <param name="snapshot">�����������</param>
    /// <returns>�������� ����������</returns>
    public static EFPAppCompositionHistoryItem SaveUser(string name, CfgPart cfg, Bitmap snapshot)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException(name);
      if (cfg == null)
        throw new ArgumentNullException("cfg");
      if (cfg.IsEmpty)
        throw new ArgumentException("���������� �� ��������", "cfg");

      string md5 = cfg.MD5Sum();

      string userSetName = null; // �������� ������ ��� ������ �������. 

      EFPAppCompositionHistoryItem item = null;

      #region ������ UIHistory

      EFPConfigSectionInfo configInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIUserHistory, String.Empty);
      CfgPart cfgHist;
      int currentIndex = -1; // ����� ���������� �� �����
      using (EFPApp.ConfigManager.GetConfig(configInfoHist, EFPConfigMode.Write, out cfgHist))
      {
        string[] names = cfgHist.GetChildNames();

        DataTable table = new DataTable();
        table.Columns.Add("UserSetName", typeof(string)); // "User1", "User2", ...
        table.Columns.Add("Name", typeof(string)); // ���������������� ���
        table.Columns.Add("Time", typeof(DateTime));
        table.Columns.Add("MD5", typeof(string));
        DataTools.SetPrimaryKey(table, "UserSetName");

        DateTime time = DateTime.Now;

        for (int i = 0; i < names.Length; i++)
        {
          if (names[i].StartsWith("User"))
          {
            CfgPart cfgOne = cfgHist.GetChild(names[i], false);
            DataRow row = table.Rows.Add(names[i],
              cfgOne.GetString("Name"),
              cfgOne.GetDateTime("Time"),
              cfgOne.GetString("MD5"));
            if (String.Equals(row["Name"].ToString(), name, StringComparison.OrdinalIgnoreCase))
            {
              currentIndex = i;
              cfgOne.SetDateTime("Time", time); // ������ ��������
              item = CreateUserItem(names[i], row["Name"].ToString(), time, md5);
              userSetName = names[i];
            }
            // ���������� ��� break;
          }
        }

        if (currentIndex < 0)
        {
          // ����������� ����� ��� ������
          for (int i = 1; i <= int.MaxValue; i++)
          {
            string userSetName2 = "User" + i.ToString();
            if (table.Rows.Find(userSetName2) == null)
            {
              userSetName = userSetName2;
              break;
            }
          }
          if (userSetName == null)
            throw new BugException("�� ����� ����� ������ ��� ������ �������");

          table.Rows.Add(userSetName, name, DateTime.Now, md5);
          table.AcceptChanges();

          // ������ ���������� ��� ������
          cfgHist.Clear();
          foreach (DataRow row in table.Rows)
          {
            string userSetName2 = row["UserSetName"].ToString();
            CfgPart cfgOne = cfgHist.GetChild(userSetName2, true);
            cfgOne.SetString("Name", row["Name"].ToString());
            cfgOne.SetDateTime("Time", (DateTime)(row["Time"]));
            cfgOne.SetString("MD5", row["MD5"].ToString());
          }

          item = CreateUserItem(userSetName, name, time, md5);
        } // CurrentIndex<0
      } // SectHist

      #endregion

      #region ������ UI ��� �������

      EFPConfigSectionInfo configInfo2 = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIUser, userSetName);
      CfgPart cfg2;
      using (EFPApp.ConfigManager.GetConfig(configInfo2, EFPConfigMode.Write, out cfg2))
      {
        cfg2.Clear();
        cfg.CopyTo(cfg2);
      }

      #endregion

      #region Snapshot

      // ���������� Snapshot, ������ ���� ������ �����
      try
      {
        EFPConfigSectionInfo configInfoSnapshot = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
          EFPConfigCategories.UIUserSnapshot, userSetName);
        EFPApp.SnapshotManager.SaveSnapshot(configInfoSnapshot, snapshot);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "�� ������� ��������� ����������� ��� ���������������� ���������");
      }

      #endregion

#if DEBUG
      if (item == null)
        throw new BugException("Item=null");
#endif

      return item;
    }

    /// <summary>
    /// ������� ����������� ���������� �� ������� ��� ������� ���������������� ����������.
    /// </summary>
    /// <param name="item">�������� ����������� ����������</param>
    public static void Delete(EFPAppCompositionHistoryItem item)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
#endif

      EFPConfigSectionInfo configInfoHist;
      CfgPart cfgHist;
      switch (item.Kind)
      {
        case EFPAppCompositionHistoryItemKind.History:
          configInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
          EFPConfigCategories.UIHistory, String.Empty);
          using (EFPApp.ConfigManager.GetConfig(configInfoHist, EFPConfigMode.Write, out cfgHist))
          {
            cfgHist.Remove(item.UserSetName);
          }
          break;

        case EFPAppCompositionHistoryItemKind.User:
          configInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
          EFPConfigCategories.UIUserHistory, String.Empty);
          using (EFPApp.ConfigManager.GetConfig(configInfoHist, EFPConfigMode.Write, out cfgHist))
          {
            cfgHist.Remove(item.UserSetName);
          }
          break;

        default:
          throw new ArgumentException("����������� Kind=" + item.Kind.ToString(), "item");
      }
    }

    #endregion
  }
}
