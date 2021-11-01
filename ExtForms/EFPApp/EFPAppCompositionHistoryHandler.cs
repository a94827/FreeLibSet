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
      DataTable Table = new DataTable();
      Table.Columns.Add("UserSetName", typeof(string)); // "Hist1", "Hist2", ...
      Table.Columns.Add("Time", typeof(DateTime));
      Table.Columns.Add("MD5", typeof(string));
      DataTools.SetPrimaryKey(Table, "UserSetName");

      #region ������ ������

      EFPConfigSectionInfo ConfigInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIHistory, String.Empty);
      CfgPart cfgHist;
      using (EFPApp.ConfigManager.GetConfig(ConfigInfoHist, EFPConfigMode.Read, out cfgHist))
      {
        string[] Names = cfgHist.GetChildNames();

        for (int i = 0; i < Names.Length; i++)
        {
          if (Names[i].StartsWith("Hist"))
          {
            CfgPart cfgOne = cfgHist.GetChild(Names[i], false);
            /*DataRow Row = */ Table.Rows.Add(Names[i], cfgOne.GetDateTime("Time"),
              cfgOne.GetString("MD5"));
          }
        }
      }

      #endregion

      Table.DefaultView.Sort = "Time DESC"; // �� ��������

      #region �������� �������

      EFPAppCompositionHistoryItem[] Items = new EFPAppCompositionHistoryItem[Table.DefaultView.Count];
      for (int i = 0; i < Table.DefaultView.Count; i++)
      {
        DataRow Row = Table.DefaultView[i].Row;
        Items[i] = CreateHistItem(i, Row["UserSetName"].ToString(),
          (DateTime)(Row["Time"]),
          Row["MD5"].ToString());
      }

      #endregion

      return Items;
    }

    private static EFPAppCompositionHistoryItem CreateHistItem(int itemIndex, string userSetName, DateTime time, string md5)
    {
      string Prefix;
      switch (itemIndex)
      {
        case 0: Prefix = "���������"; break;
        case 1: Prefix = "�������������"; break;
        default: Prefix = "���������� �" + (itemIndex + 1).ToString(); break;
      }

      return new EFPAppCompositionHistoryItem(EFPAppCompositionHistoryItemKind.History,
        userSetName,
        "[ " + Prefix + " " + time.ToString("g") + " ]",
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
      DataTable Table = new DataTable();
      Table.Columns.Add("UserSetName", typeof(string)); // "User1", "User2", ...
      Table.Columns.Add("Name", typeof(string)); // ���, �������� �������������
      Table.Columns.Add("Time", typeof(DateTime));
      Table.Columns.Add("MD5", typeof(string));
      DataTools.SetPrimaryKey(Table, "UserSetName");

      #region ������ ������

      EFPConfigSectionInfo ConfigInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIUserHistory, String.Empty);
      CfgPart cfgHist;
      using (EFPApp.ConfigManager.GetConfig(ConfigInfoHist, EFPConfigMode.Read, out cfgHist))
      {
        string[] Names = cfgHist.GetChildNames();

        for (int i = 0; i < Names.Length; i++)
        {
          if (Names[i].StartsWith("User"))
          {
            CfgPart cfgOne = cfgHist.GetChild(Names[i], false);
            /*DataRow Row = */Table.Rows.Add(Names[i], cfgOne.GetString("Name"),
              cfgOne.GetDateTime("Time"),
              cfgOne.GetString("MD5"));
          }
        }
      }

      #endregion

      Table.DefaultView.Sort = "Name";

      #region �������� �������

      EFPAppCompositionHistoryItem[] Items = new EFPAppCompositionHistoryItem[Table.DefaultView.Count];
      for (int i = 0; i < Table.DefaultView.Count; i++)
      {
        DataRow Row = Table.DefaultView[i].Row;
        Items[i] = CreateUserItem(Row["UserSetName"].ToString(),
          Row["Name"].ToString(),
          (DateTime)(Row["Time"]),
          Row["MD5"].ToString());
      }

      #endregion

      return Items;
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

      EFPAppCompositionHistoryItem Item = null;

      EFPConfigSectionInfo ConfigInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIUserHistory, String.Empty);
      CfgPart cfgHist;
      using (EFPApp.ConfigManager.GetConfig(ConfigInfoHist, EFPConfigMode.Read, out cfgHist))
      {
        string[] Names = cfgHist.GetChildNames();

        for (int i = 0; i < Names.Length; i++)
        {
          if (Names[i].StartsWith("User"))
          {
            CfgPart cfgOne = cfgHist.GetChild(Names[i], false);
            string UserName = cfgOne.GetString("Name");
            if (UserName == name)
            {
              Item = CreateUserItem(Names[i], UserName, cfgOne.GetDateTime("Time"),
                cfgOne.GetString("MD5"));
              break;
            }
          }
        }
      }

      #endregion

      return Item;
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

      string MD5 = cfg.MD5Sum();

      string UserSetName = null; // �������� ������ ��� ������ �������. 
      // ���� �� ���� ��������� ������, �������� null.

      EFPAppCompositionHistoryItem Item = null;

      #region ������ UIHistory

      EFPConfigSectionInfo ConfigInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIHistory, String.Empty);
      CfgPart cfgHist;
      int CurrentIndex = -1; // ����� ���������� �� MD5
      using (EFPApp.ConfigManager.GetConfig(ConfigInfoHist, EFPConfigMode.Write, out cfgHist))
      {
        string[] Names = cfgHist.GetChildNames();

        DataTable Table = new DataTable();
        Table.Columns.Add("UserSetName", typeof(string)); // "Hist1", "Hist2", ...
        Table.Columns.Add("Time", typeof(DateTime));
        Table.Columns.Add("MD5", typeof(string));
        DataTools.SetPrimaryKey(Table, "UserSetName");

        DateTime Time = DateTime.Now;

        for (int i = 0; i < Names.Length; i++)
        {
          if (Names[i].StartsWith("Hist"))
          {
            CfgPart cfgOne = cfgHist.GetChild(Names[i], false);
            DataRow Row = Table.Rows.Add(Names[i], cfgOne.GetDateTime("Time"),
              cfgOne.GetString("MD5"));
            if (Row["MD5"].ToString() == MD5)
            {
              CurrentIndex = i;
              cfgOne.SetDateTime("Time", Time); // ������ ��������
              Item = CreateHistItem(0, Names[i], Time, MD5);
            }
            // ���������� ��� break;
          }
        }

        if (CurrentIndex < 0)
        {
          // ��������� �������� ����� ������ � �������
          // ������� ����� ������ ������
          Table.DefaultView.Sort = "Time"; // �� �����������
          while (Table.DefaultView.Count >= EFPApp.CompositionHistoryCount)
            Table.DefaultView[0].Row.Delete();

          // ����������� ����� ��� ������
          for (int i = 1; i <= EFPApp.CompositionHistoryCount; i++)
          {
            string UserSetName2 = "Hist" + i.ToString();
            if (Table.Rows.Find(UserSetName2) == null)
            {
              UserSetName = UserSetName2;
              break;
            }
          }

          if (UserSetName == null)
            throw new BugException("�� ����� ����� ������ ��� ������ �������");
          Table.Rows.Add(UserSetName, DateTime.Now, MD5);
          Table.AcceptChanges();

          // ������ ���������� ��� ������
          cfgHist.Clear();
          foreach (DataRow Row in Table.Rows)
          {
            string UserSetName2 = Row["UserSetName"].ToString();
            CfgPart cfgOne = cfgHist.GetChild(UserSetName2, true);
            cfgOne.SetDateTime("Time", (DateTime)(Row["Time"]));
            cfgOne.SetString("MD5", Row["MD5"].ToString());
          }

          Item = CreateHistItem(0, UserSetName, Time, MD5);
        } // CurrentIndex<0
      } // SectHist

      #endregion

      #region ������ UI ��� �������

      if (UserSetName != null)
      {
        EFPConfigSectionInfo ConfigInfo2 = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
          EFPConfigCategories.UI, UserSetName);
        CfgPart cfg2;
        using (EFPApp.ConfigManager.GetConfig(ConfigInfo2, EFPConfigMode.Write, out cfg2))
        {
          cfg2.Clear();
          cfg.CopyTo(cfg2);
        }
      }

      #endregion

      #region Snapshot

      // ���������� Snapshot, ������ ���� ������ �����
      if (UserSetName != null)
      {
        try
        {
          EFPConfigSectionInfo ConfigInfoSnapshot = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
            EFPConfigCategories.UISnapshot, UserSetName);
          EFPApp.SnapshotManager.SaveSnapshot(ConfigInfoSnapshot, snapshot);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "�� ������� ��������� ����������� ��� ���������������� ���������");
        }
      }

      #endregion

#if DEBUG
      if (Item == null)
        throw new BugException("Item=null");
#endif

      return Item;
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

      string MD5 = cfg.MD5Sum();

      string UserSetName = null; // �������� ������ ��� ������ �������. 

      EFPAppCompositionHistoryItem Item = null;

      #region ������ UIHistory

      EFPConfigSectionInfo ConfigInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIUserHistory, String.Empty);
      CfgPart cfgHist;
      int CurrentIndex = -1; // ����� ���������� �� �����
      using (EFPApp.ConfigManager.GetConfig(ConfigInfoHist, EFPConfigMode.Write, out cfgHist))
      {
        string[] Names = cfgHist.GetChildNames();

        DataTable Table = new DataTable();
        Table.Columns.Add("UserSetName", typeof(string)); // "User1", "User2", ...
        Table.Columns.Add("Name", typeof(string)); // ���������������� ���
        Table.Columns.Add("Time", typeof(DateTime));
        Table.Columns.Add("MD5", typeof(string));
        DataTools.SetPrimaryKey(Table, "UserSetName");

        DateTime Time = DateTime.Now;

        for (int i = 0; i < Names.Length; i++)
        {
          if (Names[i].StartsWith("User"))
          {
            CfgPart cfgOne = cfgHist.GetChild(Names[i], false);
            DataRow Row = Table.Rows.Add(Names[i],
              cfgOne.GetString("Name"),
              cfgOne.GetDateTime("Time"),
              cfgOne.GetString("MD5"));
            if (String.Equals(Row["Name"].ToString(), name, StringComparison.OrdinalIgnoreCase))
            {
              CurrentIndex = i;
              cfgOne.SetDateTime("Time", Time); // ������ ��������
              Item = CreateUserItem(Names[i], Row["Name"].ToString(), Time, MD5);
              UserSetName = Names[i];
            }
            // ���������� ��� break;
          }
        }

        if (CurrentIndex < 0)
        {
          // ����������� ����� ��� ������
          for (int i = 1; i <= int.MaxValue; i++)
          {
            string UserSetName2 = "User" + i.ToString();
            if (Table.Rows.Find(UserSetName2) == null)
            {
              UserSetName = UserSetName2;
              break;
            }
          }
          if (UserSetName == null)
            throw new BugException("�� ����� ����� ������ ��� ������ �������");

          Table.Rows.Add(UserSetName, name, DateTime.Now, MD5);
          Table.AcceptChanges();

          // ������ ���������� ��� ������
          cfgHist.Clear();
          foreach (DataRow Row in Table.Rows)
          {
            string UserSetName2 = Row["UserSetName"].ToString();
            CfgPart cfgOne = cfgHist.GetChild(UserSetName2, true);
            cfgOne.SetString("Name", Row["Name"].ToString());
            cfgOne.SetDateTime("Time", (DateTime)(Row["Time"]));
            cfgOne.SetString("MD5", Row["MD5"].ToString());
          }

          Item = CreateUserItem(UserSetName, name, Time, MD5);
        } // CurrentIndex<0
      } // SectHist

      #endregion

      #region ������ UI ��� �������

      EFPConfigSectionInfo ConfigInfo2 = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIUser, UserSetName);
      CfgPart cfg2;
      using (EFPApp.ConfigManager.GetConfig(ConfigInfo2, EFPConfigMode.Write, out cfg2))
      {
        cfg2.Clear();
        cfg.CopyTo(cfg2);
      }

      #endregion

      #region Snapshot

      // ���������� Snapshot, ������ ���� ������ �����
      try
      {
        EFPConfigSectionInfo ConfigInfoSnapshot = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
          EFPConfigCategories.UIUserSnapshot, UserSetName);
        EFPApp.SnapshotManager.SaveSnapshot(ConfigInfoSnapshot, snapshot);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "�� ������� ��������� ����������� ��� ���������������� ���������");
      }

      #endregion

#if DEBUG
      if (Item == null)
        throw new BugException("Item=null");
#endif

      return Item;
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

      EFPConfigSectionInfo ConfigInfoHist;
      CfgPart cfgHist;
      switch (item.Kind)
      {
        case EFPAppCompositionHistoryItemKind.History:
          ConfigInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
          EFPConfigCategories.UIHistory, String.Empty);
          using (EFPApp.ConfigManager.GetConfig(ConfigInfoHist, EFPConfigMode.Write, out cfgHist))
          {
            cfgHist.Remove(item.UserSetName);
          }
          break;

        case EFPAppCompositionHistoryItemKind.User:
          ConfigInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
          EFPConfigCategories.UIUserHistory, String.Empty);
          using (EFPApp.ConfigManager.GetConfig(ConfigInfoHist, EFPConfigMode.Write, out cfgHist))
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