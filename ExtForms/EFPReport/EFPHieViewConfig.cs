// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Config;

#if XXX
namespace FreeLibSet.Forms
{
  /// <summary>
  /// ������������ ������� �������� � ��������� EFPHieView.
  /// ������ ������ ���������� ������� � �� �������.
  /// ���� ������ �� �������� � ����������� ��������� � ������ ��������
  /// � ���������� ������ (������, ����������� �� ReportParams).
  /// ��� �������������� ������������ ����� HieViewLevelConfigHandler
  /// ������� ������������ �������� � �������, ������ � ������� �������� �����
  /// �������� View
  /// </summary>
  public class EFPHieViewConfig
  {
#region �����������

    /// <summary>
    /// ������� ������������� ������������ ������� ��������
    /// ������ ������ ������� ������ ���� �������� � ������� ���������� �������
    /// �� ���������. ������� ��������� ��������� ������ ���� �������� ���������
    /// �������, � ��������� - ����� �������
    /// </summary>
    /// <param name="AllLevels">������ ������ ��������� �������</param>
    public EFPHieViewConfig(ICollection<EFPHieViewLevel> AllLevels)
    {
      FAllLevels = new EFPHieViewLevel[AllLevels.Count];
      AllLevels.CopyTo(FAllLevels, 0);

      FTable = new DataTable();
      FTable.Columns.Add("LevelIndex", typeof(int)); // ������� � AllLevels
      FTable.Columns.Add("Flag", typeof(bool)); // ������� ������
      FTable.Columns.Add("Name", typeof(string)); // ��������
      FTable.Columns.Add("ImageKey", typeof(string)); // ����������� ��� ���������
      FTable.Columns.Add("FlagIsFixed", typeof(bool)); // true, ���� ������ ������ ����������
      FTable.Columns.Add("Visible", typeof(bool)); // false ��� ������� �����
      FTable.Columns.Add("RowOrder", typeof(int)); // ������� ���������� �������
      FTable.Columns.Add("DefRowOrder", typeof(int)); // ������� ���������� ������ ������� ������� �� ���������
      FTable.Columns.Add("RowOrderIsFixed", typeof(bool)); // true, ���� ��� ������ ������ �����������
      FTable.Columns.Add("ViolatedOrderRuleIndex", typeof(int)); // ����� �������, ������� ���� �������� ��� DBNull

      DataTools.SetPrimaryKey(FTable, "LevelIndex");
      FTable.DefaultView.Sort = "RowOrder";
      FTable.DefaultView.RowFilter = "Visible=1";

      for (int i = 0; i < AllLevels.Count; i++)
      {
        DataRow Row = FTable.NewRow();
        Row["LevelIndex"] = i;
        FTable.Rows.Add(Row);
      }

      FOrderRules = new OrderRuleList(this);

      RefreshTable();
      SetDefaultConfig();
    }

#endregion

#region ��������

    /// <summary>
    /// ������ ���� ��������� ������� ��������
    /// �������� � ������������. ��� ���������� �� ��������� AllLevels[0]
    /// ������������� ������ ���������� ������
    /// </summary>
    public EFPHieViewLevel[] AllLevels { get { return FAllLevels; } }
    private EFPHieViewLevel[] FAllLevels;

    /// <summary>
    /// ���������� ������ ������ �� ������� AllLevels �� ���� (�������� HieViewLevel.NamePart)
    /// ���������� null, ���� ������� �� ������
    /// </summary>
    /// <param name="LevelName">��� ������ ��� ������</param>
    /// <returns>������ ������ ��� null</returns>
    public EFPHieViewLevel this[string LevelName]
    {
      get
      {
        for (int i = 0; i < AllLevels.Length; i++)
        {
          if (AllLevels[i].Name == LevelName)
            return AllLevels[i];
        }
        return null;
      }
    }

    /// <summary>
    /// ���������� ������ ��������� ������� ��������� (LevelIndex) � �������
    /// ���� �������. ������ �� ������� �� ������� ��������� ������� � �������
    /// ����������.
    /// </summary>
    /// <param name="LevelName">��� ������</param>
    /// <returns>������ � ������� AllLevels</returns>
    public int IndexOf(string LevelName)
    {
      for (int i = 0; i < AllLevels.Length; i++)
      {
        if (AllLevels[i].Name == LevelName)
          return i;
      }

      return -1;
    }

    /// <summary>
    /// ���������� true, ���� ���� �� ��� ������ ������ �������� ���� �������� ���������
    /// </summary>
    public bool HasParamEditor
    {
      get
      {
        for (int i = 0; i < AllLevels.Length; i++)
        {
          if (AllLevels[i].ParamEditor != null)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// ������ ��������� ������� � �������� �������
    /// ������� ������� ������������� ������ ���������� ������ (� �������� �����),
    /// ��������� ������� - ������ ��������, �� ���� � �������, ������� ����������
    /// �������� HieViewHandler.Levels
    /// </summary>
    public EFPHieViewLevel[] SelectedLevels
    {
      get
      {
        List<EFPHieViewLevel> lst = new List<EFPHieViewLevel>();
        foreach (DataRowView drv in View)
        {
          if (DataTools.GetBool(drv.Row, "Flag"))
          {
            int LevelIndex = DataTools.GetInt(drv.Row, "LevelIndex");
            lst.Insert(0, FAllLevels[LevelIndex]);
          }
        }
        return lst.ToArray();
      }
      set
      {
        if (value == null)
          value = new EFPHieViewLevel[0];
        for (int i = 0; i < FTable.Rows.Count; i++)
        {
          int Index = Array.IndexOf<EFPHieViewLevel>(value, AllLevels[i]);
          if (Index < 0)
            FTable.Rows[i]["Flag"] = false;
          else
          {
            FTable.Rows[i]["Flag"] = true;
            FTable.Rows[i]["RowOrder"] = value.Length - Index - 1;
          }
        }
      }
    }

    /// <summary>
    /// ������ ���� ��������� ������� �������� � �������� �������
    /// �������� ����� ���� ������������ ��� �������� ������ ������� �������
    /// ������� ������� ������� ������������� �������� ���������� ������
    /// </summary>
    public string[] SelectedNames
    {
      get
      {
        List<string> lst = new List<string>();
        foreach (DataRowView drv in View)
        {
          if (DataTools.GetBool(drv.Row, "Flag"))
          {
            int LevelIndex = DataTools.GetInt(drv.Row, "LevelIndex");
            lst.Insert(0, FAllLevels[LevelIndex].Name);
          }
        }
        return lst.ToArray();
      }
      set
      {
        if (value == null)
          value = DataTools.EmptyStrings;
        for (int i = 0; i < FTable.Rows.Count; i++)
        {
          int Index = Array.IndexOf<string>(value, AllLevels[i].Name);
          if (Index < 0)
            FTable.Rows[i]["Flag"] = false;
          else
          {
            FTable.Rows[i]["Flag"] = true;
            FTable.Rows[i]["RowOrder"] = value.Length - Index - 1;
          }
        }
      }
    }

    /// <summary>
    /// �������������� ������ � ������ ���� ��������� ������� (�������� SelectedNames)
    /// � ���� ������ � �������, ����������� ��������
    /// ������ �������� �� �������� ���������� � ��������
    /// </summary>
    public string SelectedNamesCSV
    {
      get
      {
        return String.Join(",", SelectedNames);
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          SelectedNames = DataTools.EmptyStrings;
        else
          SelectedNames = value.Split(',');
      }
    }

    /// <summary>
    /// ���������� ������������ �������� DisplayName ��������� ������� SelectedLevels � ����
    /// ������, ����������� ��������.
    /// �������� ����� ���� ������������ � �������� ��������� ���� "Hie_Text" ��� 
    /// ������������� �������� ���������� ���������.
    /// ������� ��������� �������� �������� �� ��������� � SelectedLevels. �� ����
    /// ������� ���� ����� ������� ������� ��������, � ����� - ���������
    /// </summary>
    public string SelectedLevelsTitle
    {
      get
      {
        List<string> lst = new List<string>();
        foreach (DataRowView drv in View)
        {
          if (DataTools.GetBool(drv.Row, "Flag"))
          {
            int LevelIndex = DataTools.GetInt(drv.Row, "LevelIndex");
            lst.Add(FAllLevels[LevelIndex].DisplayName);
          }
        }
        return String.Join(", ", lst.ToArray());
      }
    }

    /// <summary>
    /// ���������� ���������� ��������� ������� SelectedLevels.Length,
    /// �� �������� �������, ��� ��������� � SelectedLevels
    /// </summary>
    public int SelectedLevelCount
    {
      get
      {
        int cnt = 0;
        foreach (DataRowView drv in View)
        {
          if (DataTools.GetBool(drv.Row, "Flag"))
            cnt++;
        }
        return cnt;
      }
    }

    /// <summary>
    /// �������, ������������ � ��������� �������.
    /// ������ ����� ������������� �������� ������� ������� (���� "RowOrder").
    /// � ������� �� ������ �������, � ������ ������� ����� ��������. ������ �������
    /// ���� ����� ������� �������, � ��������� - ����� ����������. ������ "�����"
    /// � ������� �� ������
    /// </summary>
    public DataView View { get { return FTable.DefaultView; } }
    private DataTable FTable;

    /// <summary>
    /// true, ���� � ������������� ��������� ������ ���� ������ ������ ������ ����
    /// �� ��������� - false
    /// </summary>
    public bool HideExtraSumRows;

    /// <summary>
    /// ���� �������� �����������, �� ������������ �� ����� �������� ��������
    /// HideExtraSumRows � ���������. ��� ����� �� ����� �������� � ������������
    /// � ������ ������������
    /// �� ��������� - false (����� �������������)
    /// ��������� �������� ������ ����������� �� ������ ReadConfig() � �� ��������
    /// ���������
    /// </summary>
    public bool HideExtraSumRowsFixed;

    /// <summary>
    /// ������ �������, � ������� ���������� �������� Required � Visible, �� �������
    /// �� ������� (������)
    /// </summary>
    public EFPHieViewLevel[] UnselectedRequiredLevels
    {
      get
      {
        List<EFPHieViewLevel> lvls = new List<EFPHieViewLevel>();
        foreach (DataRowView drv in View)
        {
          if (DataTools.GetBool(drv.Row, "Visible") &&
            DataTools.GetBool(drv.Row, "FlagIsFixed"))
          {
            if (DataTools.GetBool(drv.Row, "Flag"))
              continue;

            int LevelIndex = DataTools.GetInt(drv.Row, "LevelIndex");
            lvls.Add(FAllLevels[LevelIndex]);
          }
        }
        return lvls.ToArray();
      }
    }

    public string UnselectedRequiredLevelsTitle
    {
      get
      {
        EFPHieViewLevel[] a1 = UnselectedRequiredLevels;
        string[] a2 = new string[a1.Length];
        for (int i = 0; i < a1.Length; i++)
          a2[i] = a1[i].DisplayName;
        return String.Join(", ", a2);
      }
    }

#endregion

#region ������

    public void RefreshTable()
    {
      int cntVisible = 0;
      for (int i = FAllLevels.Length - 1; i >= 0; i--)
      {
        EFPHieViewLevel lvl = FAllLevels[i];
        DataRow Row = FTable.Rows[i];
        string s = lvl.DisplayName;
        if (lvl.ParamEditor != null)
          s += ": " + lvl.ParamEditor.GetText(lvl);
        Row["Name"] = s;
        Row["ImageKey"] = lvl.ImageKey;
        Row["FlagIsFixed"] = lvl.Requred || (!lvl.Visible);
        if (lvl.Requred)
          Row["Flag"] = true;
        if (!lvl.Visible)
          Row["Flag"] = false;
        Row["Visible"] = lvl.Visible;
        Row["RowOrderIsFixed"] = lvl.Position != EFPHieViewLevelPosition.Normal;
        if (lvl.Visible)
        {
          cntVisible++;
          Row["DefRowOrder"] = cntVisible;
        }
        else
          Row["DefRowOrder"] = 0;

      }

      CheckOrderRules();
    }

    /// <summary>
    /// ��������� ���� ViolatedOrderRuleIndex
    /// </summary>
    public void CheckOrderRules()
    {
      foreach (DataRow Row in FTable.Rows)
        Row["ViolatedOrderRuleIndex"] = DBNull.Value;

      for (int i = 0; i < OrderRules.Count; i++)
      {
        OrderRules[i].FViolated = false;
        DataRow Row1 = FTable.Rows.Find(OrderRules[i].UpperLevelIndex);
        DataRow Row2 = FTable.Rows.Find(OrderRules[i].LowerLevelIndex);
        if (!DataTools.GetBool(Row1, "Flag"))
          continue;
        if (!DataTools.GetBool(Row2, "Flag"))
          continue;

        int RowOrder1 = DataTools.GetInt(Row1, "RowOrder");
        int RowOrder2 = DataTools.GetInt(Row2, "RowOrder");

        if (RowOrder1 < RowOrder2)
          continue;

        // ������������� ������� ������ ��� ����� �����
        Row1["ViolatedOrderRuleIndex"] = i;
        Row2["ViolatedOrderRuleIndex"] = i;
        OrderRules[i].FViolated = true;
      }
    }

    /// <summary>
    /// ��������� ������������� ������������ � ���������
    /// </summary>
    /// <param name="HieView"></param>
    public void CopyTo(EFPHieView HieView)
    {
      HieView.Levels = SelectedLevels;
      HieView.HideExtraSumRows = HideExtraSumRows;
    }

#endregion

#region ������� ��� ������� �������

    /// <summary>
    /// ����������� ������ ������� ������� ���������� ���� �������
    /// </summary>
    public class OrderRule
    {
#region �����������

      /// <summary>
      /// ������� ����� �������
      /// </summary>
      /// <param name="UpperLevelName">�������, ������� ����� ���� ����</param>
      /// <param name="LowerLevelName">�������, ������� ����� ���� ����</param>
      public OrderRule(string UpperLevelName, string LowerLevelName)
      {
#if DEBUG
        if (String.IsNullOrEmpty(UpperLevelName))
          throw new ArgumentNullException("UpperLevelName");
        if (String.IsNullOrEmpty(LowerLevelName))
          throw new ArgumentNullException("LowerLevelName");
        if (UpperLevelName == LowerLevelName)
          throw new BugException("������� � ������ ������ �� ����� ���������: " + UpperLevelName);
#endif

        FUpperLevelName = UpperLevelName;
        FLowerLevelName = LowerLevelName;
      }

#endregion

#region ��������

      /// <summary>
      /// ��� ������ ��������, ������� ����� ���� ����.
      /// �������� �������� � ������������
      /// </summary>
      public string UpperLevelName { get { return FUpperLevelName; } }
      private string FUpperLevelName;

      /// <summary>
      /// ��� ������ ��������, ������� ����� ���� ����.
      /// �������� �������� � ������������
      /// </summary>
      public string LowerLevelName { get { return FLowerLevelName; } }
      private string FLowerLevelName;

      internal int UpperLevelIndex;

      internal int LowerLevelIndex;

      /// <summary>
      /// ���������� true, ���� ������� ��������
      /// </summary>
      public bool Violated { get { return FViolated; } }
      internal bool FViolated;

#endregion
    }

    /// <summary>
    /// ������ ������ ���������� ������� � ��������
    /// </summary>
    public class OrderRuleList : List<OrderRule>
    {
#region �����������

      internal OrderRuleList(EFPHieViewConfig Owner)
      {
        FOwner = Owner;
      }

#endregion

#region ��������

      private EFPHieViewConfig FOwner;

      /// <summary>
      /// ���������� true, ���� ���� �� ���� �� ������ ��������
      /// </summary>
      public bool HasViolation
      {
        get
        {
          for (int i = 0; i < Count; i++)
          {
            if (this[i].Violated)
              return true;
          }
          return false;
        }
      }

#endregion

#region ������
                  
      // TODO: !!! ��� ���� ����������

      public new void Add(OrderRule Rule)
      {
        if (Rule == null)
          return;

        int UpperLevelIndex = FOwner.IndexOf(Rule.UpperLevelName);
        int LowerLevelIndex = FOwner.IndexOf(Rule.LowerLevelName);
        if (UpperLevelIndex < 0 || LowerLevelIndex < 0)
          return;

        Rule.UpperLevelIndex = UpperLevelIndex;
        Rule.LowerLevelIndex = LowerLevelIndex;
        base.Add(Rule);
      }

      public void Add(string UpperLevelName, string LowerLevelName)
      {
        OrderRule Rule = new OrderRule(UpperLevelName, LowerLevelName);
        Add(Rule);
      }

#endregion
    }

    /// <summary>
    /// ������ ������ ��������� ������������ �������
    /// </summary>
    public OrderRuleList OrderRules { get { return FOrderRules; } }
    private OrderRuleList FOrderRules;

#endregion

#region ��������������� ������ ��������� ��������

    /// <summary>
    /// ������������� �������� "Visible" ��� ������, ���� �� ����������
    /// </summary>
    /// <param name="LevelName">��� ������</param>
    /// <param name="Visible">��������� ������</param>
    public void SetVisible(string LevelName, bool Visible)
    {
      EFPHieViewLevel lvl = this[LevelName];
      if (lvl != null)
        lvl.Visible = Visible;
    }

#if XXX
    /// <summary>
    /// ������������� �������� "Visible" ��� ������, ���� �� ����������.
    /// ����� ���� ����� �������� �������� Selected, ����� Visible=false,
    /// � UnselectInvisible=true
    /// </summary>
    /// <param name="LevelName">��� ������ ��������</param>
    /// <param name="Visible">�������� �������� Visible</param>
    /// <param name="UnselectInvisible">���� true, �� ����� ����������� Selected=false ��� Visible=false</param>
    public void SetVisible(string LevelName, bool Visible, bool UnselectInvisible)
    {
      HieViewLevel lvl = this[LevelName];
      if (lvl != null)
      {
        lvl.Visible = Visible;
        if (UnselectInvisible && (!Visible))
          SetSelected(LevelName, false);
      }
    }
#endif

    /// <summary>
    /// ���������� ������� ��������� ������, ���� �� ����������
    /// </summary>
    /// <param name="LevelName">��� ������</param>
    /// <returns>������� ���������</returns>
    public bool GetVisible(string LevelName)
    {
      EFPHieViewLevel lvl = this[LevelName];
      if (lvl != null)
        return lvl.Visible;
      else
        return false;
    }

    /// <summary>
    /// ������������� �������� "Required" ��� ������, ���� �� ����������
    /// </summary>
    /// <param name="LevelName">��� ������</param>
    /// <param name="Required">�������� "Required"</param>
    public void SetRequired(string LevelName, bool Required)
    {
      EFPHieViewLevel lvl = this[LevelName];
      if (lvl != null)
        lvl.Requred = Required;
    }

    /// <summary>
    /// ���������� �������� "Required" ��� ������, ���� �� ����������
    /// </summary>
    /// <param name="LevelName">��� ������</param>
    /// <returns>�������� "Required"</returns>
    public bool GetRequired(string LevelName)
    {
      EFPHieViewLevel lvl = this[LevelName];
      if (lvl != null)
        return lvl.Requred;
      else
        return false;
    }

    public void SetSelected(string LevelName, bool Selected)
    {
      int LevelIndex = IndexOf(LevelName);
      if (LevelIndex < 0)
        return;
      DataRow Row = FTable.Rows.Find(LevelIndex);
      Row["Flag"] = Selected;
    }

    public bool GetSelected(string LevelName)
    {
      int LevelIndex = IndexOf(LevelName);
      if (LevelIndex < 0)
        return false;
      DataRow Row = FTable.Rows.Find(LevelIndex);
      return DataTools.GetBool(Row, "Flag");
    }

#endregion

#region ������ � ������ ������������

    /// <summary>
    /// ��������� ������� ����� � ������� � �������� �� ���������
    /// </summary>
    public void SetDefaultConfig()
    {
      for (int i = FAllLevels.Length - 1; i >= 0; i--)
      {
        EFPHieViewLevel lvl = FAllLevels[i];
        DataRow Row = FTable.Rows[i];
        Row["RowOrder"] = Row["DefRowOrder"];
        if ((!lvl.Requred) && lvl.Visible)
          Row["Flag"] = lvl.DefaultSelected;
      }

      if (!HideExtraSumRowsFixed)
        HideExtraSumRows = false;
    }

    public void WriteConfig(CfgPart Config)
    {
      if (Config == null)
        return;

      CfgPart PartFlag = Config.GetChild("HieLevels", true);
      CfgPart PartOrd = Config.GetChild("HieOrders", true);
      CfgPart PartParams = null;
      if (HasParamEditor)
        PartParams = Config.GetChild("HieLevelParams", true);
      for (int i = 0; i < AllLevels.Length; i++)
      {
        if (AllLevels[i].Visible)
        {
          DataRow Row = FTable.Rows[i];
          bool Flag = DataTools.GetBool(Row, "Flag");
          int Order = DataTools.GetInt(Row, "RowOrder");
          PartFlag.SetBool(AllLevels[i].Name, Flag);
          PartOrd.SetInt(AllLevels[i].Name, Order);
          if (AllLevels[i].ParamEditor != null)
          {
            CfgPart PartParam = PartParams.GetChild(AllLevels[i].Name, true);
            AllLevels[i].ParamEditor.WriteConfig(AllLevels[i], PartParam);
          }
        }
      }

      if (!HideExtraSumRowsFixed)
        Config.SetBool("HideExtraSubTotals", HideExtraSumRows);
    }

    public void ReadConfig(CfgPart Config)
    {
      if (Config == null)
      {
        SetDefaultConfig();
        return;
      }
      CfgPart PartFlag = Config.GetChild("HieLevels", false);
      CfgPart PartOrd = Config.GetChild("HieOrders", false);
      CfgPart PartParams = Config.GetChild("HieLevelParams", false);
      for (int i = 0; i < AllLevels.Length; i++)
      {
        if (AllLevels[i].Visible)
        {
          DataRow Row = FTable.Rows[i];
          bool Flag = DataTools.GetBool(Row, "Flag");
          int Order = DataTools.GetInt(Row, "RowOrder");
          if (PartFlag != null)
            PartFlag.GetBool(AllLevels[i].Name, ref Flag);
          if (PartOrd != null)
            PartOrd.GetInt(AllLevels[i].Name, ref Order);
          Row["Flag"] = Flag;
          Row["RowOrder"] = Order;
          if (AllLevels[i].ParamEditor != null)
          {
            // ������� ����� ������ �������� ��������� ����, ���� ���� ��� ����
            // ��� ������ ������������. � ���� ������ ������������ ������ ������
            CfgPart PartParam = null;
            if (PartParams != null)
              PartParam = PartParams.GetChild(AllLevels[i].Name, false);
            if (PartParam == null)
              PartParam = new TempCfg();
            AllLevels[i].ParamEditor.ReadConfig(AllLevels[i], PartParam);
          }
        }
      }

      if (!HideExtraSumRowsFixed)
        HideExtraSumRows = Config.GetBool("HideExtraSubTotals");

      RefreshTable(); // ��� ���������� ����� ��� ���� "��������"
    }

#endregion
  }

}
#endif

