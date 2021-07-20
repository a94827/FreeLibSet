using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using AgeyevAV.CS1.Client;
using CommonAccDep;
using AgeyevAV.CS1;
using AgeyevAV;
using AgeyevAV.Config;

namespace ClientAccDep
{
  /// <summary>
  /// ����� ��������� �������.
  /// ����� �������������� ��� ����� ������� ��� ���������� ������ MainPanel � 
  /// ���� ���������� ������
  /// ������������ HieViewLevelConfigEditHandler
  /// </summary>
  internal partial class HieViewLevelEditForm : Form
  {
    #region �����������

    public HieViewLevelEditForm()
    {
      InitializeComponent();
      Icon = EFPApp.MainImageIcon("�����������");
    }

    #endregion
  }

  /// <summary>
  /// ������������ ������� �������� � ��������� HieView.
  /// ������ ������ ���������� ������� � �� �������.
  /// ���� ������ �� �������� � ����������� ��������� � ������ ��������
  /// � ���������� ������ (������, ����������� �� ReportParams).
  /// ��� �������������� ������������ ����� HieViewLevelConfigHandler
  /// ������� ������������ �������� � �������, ������ � ������� �������� �����
  /// �������� View
  /// </summary>
  public class HieViewLevelConfig
  {
    #region �����������

    /// <summary>
    /// ������� ������������� ������������ ������� ��������
    /// ������ ������ ������� ������ ���� �������� � ������� ���������� �������
    /// �� ���������. ������� ��������� ��������� ������ ���� �������� ���������
    /// �������, � ��������� - ����� �������
    /// </summary>
    /// <param name="AllLevels">������ ������ ��������� �������</param>
    public HieViewLevelConfig(ICollection<HieViewLevel> AllLevels)
    {
      FAllLevels = new HieViewLevel[AllLevels.Count];
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
      FTable.DefaultView.RowFilter = new ValueFilter("Visible", true).GetSQL();

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
    public HieViewLevel[] AllLevels { get { return FAllLevels; } }
    private HieViewLevel[] FAllLevels;

    /// <summary>
    /// ���������� ������ ������ �� ������� AllLevels �� ���� (�������� HieViewLevel.NamePart)
    /// ���������� null, ���� ������� �� ������
    /// </summary>
    /// <param name="LevelName">��� ������ ��� ������</param>
    /// <returns>������ ������ ��� null</returns>
    public HieViewLevel this[string LevelName]
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
    public HieViewLevel[] SelectedLevels
    {
      get
      {
        List<HieViewLevel> lst = new List<HieViewLevel>();
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
          value = new HieViewLevel[0];
        for (int i = 0; i < FTable.Rows.Count; i++)
        {
          int Index = Array.IndexOf<HieViewLevel>(value, AllLevels[i]);
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
    /// ��������� �������� ������ ����������� �� ������ ReadConfig � �� ��������
    /// ���������
    /// </summary>
    public bool HideExtraSumRowsFixed;

    /// <summary>
    /// ������ �������, � ������� ���������� �������� Required � Visible, �� �������
    /// �� ������� (������)
    /// </summary>
    public HieViewLevel[] UnselectedRequiredLevels
    {
      get
      {
        List<HieViewLevel> lvls = new List<HieViewLevel>();
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
        HieViewLevel[] a1 = UnselectedRequiredLevels;
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
        HieViewLevel lvl = FAllLevels[i];
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
        Row["RowOrderIsFixed"] = lvl.Position != HieViewLevelPosition.Normal;
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
    /// <param name="HieViewHandler"></param>
    public void CopyTo(HieViewHandler HieViewHandler)
    {
      HieViewHandler.Levels = SelectedLevels;
      HieViewHandler.HideExtraSumRows = HideExtraSumRows;
    }

    #endregion

    #region ������� ��� ������� �������

    public class OrderRule
    {
      #region �����������

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

      public string UpperLevelName { get { return FUpperLevelName; } }
      private string FUpperLevelName;

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

    public class OrderRuleList : List<OrderRule>
    {
      #region �����������

      internal OrderRuleList(HieViewLevelConfig Owner)
      {
        FOwner = Owner;
      }

      #endregion

      #region ��������

      private HieViewLevelConfig FOwner;

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
    /// <param name="LevelName"></param>
    /// <param name="Visible"></param>
    public void SetVisible(string LevelName, bool Visible)
    {
      HieViewLevel lvl = this[LevelName];
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

    public bool GetVisible(string LevelName)
    {
      HieViewLevel lvl = this[LevelName];
      if (lvl != null)
        return lvl.Visible;
      else
        return false;
    }

    /// <summary>
    /// ������������� �������� "Required" ��� ������, ���� �� ����������
    /// </summary>
    /// <param name="LevelName"></param>
    /// <param name="Required"></param>
    public void SetRequired(string LevelName, bool Required)
    {
      HieViewLevel lvl = this[LevelName];
      if (lvl != null)
        lvl.Requred = Required;
    }

    public bool GetRequired(string LevelName)
    {
      HieViewLevel lvl = this[LevelName];
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
        HieViewLevel lvl = FAllLevels[i];
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

      CfgPart PartFlag = Config.GetChild("��������������", true);
      CfgPart PartOrd = Config.GetChild("���������������", true);
      CfgPart PartParams = null;
      if (HasParamEditor)
        PartParams = Config.GetChild("������������������������", true);
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
        Config.SetBool("����������������������", HideExtraSumRows);
    }

    public void ReadConfig(CfgPart Config)
    {
      if (Config == null)
      {
        SetDefaultConfig();
        return;
      }
      CfgPart PartFlag = Config.GetChild("��������������", false);
      CfgPart PartOrd = Config.GetChild("���������������", false);
      CfgPart PartParams = Config.GetChild("������������������������", false);
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
              PartParam = new ConfigSection();
            AllLevels[i].ParamEditor.ReadConfig(AllLevels[i], PartParam);
          }
        }
      }

      if (!HideExtraSumRowsFixed)
        HideExtraSumRows = Config.GetBool("����������������������");

      RefreshTable(); // ��� ���������� ����� ��� ���� "��������"
    }

    #endregion
  }

  /// <summary>
  /// �������������� ������� ����������� (������ ������������� � ������� �������)
  /// � ������� ���������� ���������.
  /// �������� ������������ � ������ ������� ���������� �������������� ������.
  /// � ������������ ����������� ������ �� ������ MainGridHandler, ������� ������������
  /// ��� �������������� (�� ������ SetCommandItems). ����������� ��������� �
  /// �������� ������� �������� � ��������.
  /// ����� ��������������� �������� HieConfig, ������� ��������� �� ������,
  /// ���������� ������� ������ �������. � ��������� �������� ����������� ������.
  /// ����� ������������ �������� ����������� ����� ��� ������������ ������,
  /// ��������� ���������� �������� � �������������� HieViewLevelConfig.
  /// ���� ��������� � ������ ������ ������� ���������� ������ �������� � ���������
  /// ������ ��������� ������� ��������, �� ��������������� �������� ��������
  /// HieViewLevel.Visible, Required � ��. ����� ������ ���� ������ �����
  /// ����� ������� RefreshTable() ��� ���������� ����� ���������� ��������� �
  /// ���� �������
  /// </summary>
  public class HieViewLevelConfigEditHandler
  {
    #region �����������


    /// <summary>
    /// ���������� �� ������ ���������� ��������� � ������� ������������ �
    /// CheckBox'� � �����������
    /// </summary>
    /// <param name="ParentControl"></param>
    public HieViewLevelConfigEditHandler(Control ParentControl, EFPBaseProvider BaseProvider)
    {
      HieViewLevelEditForm Form = new HieViewLevelEditForm();
      ParentControl.Controls.Add(Form.MainPanel);
      Form.Dispose();

      DoInit(Form, BaseProvider);

      if (ParentControl is TabPage)
        ControlledTabPage = (TabPage)ParentControl;
    }

    private HieViewLevelConfigEditHandler(HieViewLevelEditForm Form)
    {
      EFPFormProvider efpForm = new EFPFormProvider(Form);
      DoInit(Form, efpForm);
    }

    private void DoInit(HieViewLevelEditForm Form, EFPBaseProvider BaseProvider)
    {
      FMainGridHandler = new EFPAccDepGrid(BaseProvider, Form.MainGrid);

      InitMainGridHandler();
      FMainGridHandler.ToolBarPanel = Form.panSpb;

      efpHideExtraSumRows = new EFPCheckBox(BaseProvider, Form.cbHideExtraSumRows);
      efpHideExtraSumRows.ToolTipText = "���� ������ ����������, �� ��� ������� ��������, ���������� ������������ ������ ������ �� ����� ���������� �������� ������.\r\n" +
        "���� ������ ����, �� ������ ������������� ������ ����� ���������� ������";
      efpHideExtraSumRows.CheckedEx.ValueChanged += new EventHandler(efpHideExtraSumRows_CheckedChanged);

      SampleHandler = new EFPAccDepGrid(BaseProvider, Form.SampleGrid);
      InitSampleHandler();

      // ������ �������� �����, ���� �� ������� �� ������ ������
      EFPFormCheck Checker = new EFPFormCheck();
      Checker.Validating += new EFPValidatingEventHandler(Checker_Validating);
      Checker.FocusControl = FMainGridHandler.Control;
      BaseProvider.Add(Checker);
    }

    private void InitMainGridHandler()
    {
      MainGridHandler.Control.AutoGenerateColumns = false;
      MainGridHandler.Columns.AddImage();
      MainGridHandler.Columns.AddBool("Flag", true, "����");
      MainGridHandler.Columns.AddTextFill("Name", true, "��������", 100, 10);
      MainGridHandler.Columns.AddInt("RowOrder", true, "RowOrder", 3);
      MainGridHandler.Columns.LastAdded.GridColumn.ReadOnly = true;
      MainGridHandler.DisableOrdering();
      MainGridHandler.Control.ColumnHeadersVisible = false;
      //MainGridHandler.MainGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
      MainGridHandler.ReadOnly = true;
      MainGridHandler.CanView = false;
      MainGridHandler.CanInsert = false;
      MainGridHandler.CanDelete = false;
      MainGridHandler.Control.ReadOnly = false;
      MainGridHandler.CanMultiEdit = false;
      MainGridHandler.EditData += new EventHandler(MainGridHandler_EditData);

      MainGridHandler.MarkRowsColumnIndex = 1;
      MainGridHandler.UseRowImages = true;
      MainGridHandler.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(MainGridHandler_GetRowAttributes);
      MainGridHandler.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(MainGridHandler_GetCellAttributes);
      MainGridHandler.Control.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(MainGrid_DataBindingComplete);
      MainGridHandler.Control.CellValueChanged += new DataGridViewCellEventHandler(MainGrid_CellValueChanged);

      MainGridHandler.CommandItems.ManualOrderColumn = "RowOrder";
      MainGridHandler.CommandItems.DefaultManualOrderColumn = "DefRowOrder";
      MainGridHandler.CommandItems.UseRowErrors = false;
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� ���������� ���������� ��������� (�������� � ������������)
    /// </summary>
    public EFPAccDepGrid MainGridHandler { get { return FMainGridHandler; } }
    private EFPAccDepGrid FMainGridHandler;

    private EFPCheckBox efpHideExtraSumRows;

    /// <summary>
    /// �������������� ������������ �������. �������� ������������ ����� ��������
    /// � ��������� �� �������
    /// ��� ��������� �������� ����������� ������������� ��������� ������
    /// HieViewLevelConfig.View � ���������� ���������
    /// </summary>
    public HieViewLevelConfig HieConfig
    {
      get { return FHieConfig; }
      set
      {
        FHieConfig = value;
        if (value == null)
        {
          FMainGridHandler.Control.DataSource = null;
          efpHideExtraSumRows.Enabled = false;
          FMainGridHandler.ReadOnly = true;
        }
        else
        {
          FMainGridHandler.Control.DataSource = value.View;
          efpHideExtraSumRows.Checked = value.HideExtraSumRows;
          efpHideExtraSumRows.Enabled = !value.HideExtraSumRowsFixed;
          FMainGridHandler.ReadOnly = !value.HasParamEditor;
        }
        InvalidateSample();
      }
    }
    private HieViewLevelConfig FHieConfig;

    public TabPage ControlledTabPage
    {
      get { return FControlledTabPage; }
      set
      {
        FControlledTabPage = value;
        InitControlledObject();
      }
    }
    private TabPage FControlledTabPage;

    #endregion

    #region ������

    /// <summary>
    /// ��������� ������� ��������, ������� HieViewLevelConfigEditHandler.RefreshTable().
    /// ����� ����������� ������� ��������
    /// </summary>
    public void RefreshTable()
    {
      if (HieConfig != null)
        HieConfig.RefreshTable();
      InvalidateSample();
    }

    #endregion

    #region �����������

    private bool OrderRuleViolated;

    void MainGridHandler_GetRowAttributes(object Sender, EFPDataGridViewRowAttributesEventArgs Args)
    {
      OrderRuleViolated = false;

      DataRow Row = Args.DataRow;
      if (Row == null)
        return;
      // !!! �� �����������
      //if (DataTools.GetBool(Row, "RowOrderIsFixed"))
      //  Args.UserImage = ClientApp.MainImages.Images["Anchor"];
      if (DataTools.GetBool(Row, "FlagIsFixed"))
      {
        Args.Grayed = true;
        if (!DataTools.GetBool(Row, "Flag"))
          Args.AddRowError("���� ������� ����������� �������� ������������");
      }

      if (!Args.DataRow.IsNull("ViolatedOrderRuleIndex"))
      {
        int RuleIndex = DataTools.GetInt(Args.DataRow, "ViolatedOrderRuleIndex");
        HieViewLevel lvl1 = HieConfig.AllLevels[HieConfig.OrderRules[RuleIndex].UpperLevelIndex];
        HieViewLevel lvl2 = HieConfig.AllLevels[HieConfig.OrderRules[RuleIndex].LowerLevelIndex];
        Args.AddRowWarning("������� \"" + lvl1.DisplayName + "\" ������ ���� ����� ������� \"" + lvl2.DisplayName + "\"");
        OrderRuleViolated = true;
      }
    }

    void MainGridHandler_GetCellAttributes(object Sender, EFPDataGridViewCellAttributesEventArgs Args)
    {
      DataRow Row = Args.DataRow;
      if (Row == null)
        return;
      switch (Args.ColumnIndex)
      {
        case 0:
          Args.Value = EFPApp.MainImages.Images[DataTools.GetString(Row, "ImageKey")];
          string Name = DataTools.GetString(Args.DataRow, "Name");
          string Cfg = null;
          int p = Name.IndexOf(':');
          if (p >= 0)
          {
            Cfg = Name.Substring(p + 1);
            Name = Name.Substring(0, p);
          }
          Args.ToolTipText = "�������: " + Name;
          if (Cfg != null)
            Args.ToolTipText += "\r\n" + "���������: " + Cfg;
          break;
        case 2:
          Args.IndentLevel = Args.RowIndex;
          Args.ToolTipText = DataTools.GetString(Args.DataRow, "Name");
          if (OrderRuleViolated)
            Args.ColorType = EFPDataGridViewColorType.Warning;
          break;
      }
    }

    void efpHideExtraSumRows_CheckedChanged(object Sender, EventArgs Args)
    {
      if (FHieConfig != null)
        FHieConfig.HideExtraSumRows = efpHideExtraSumRows.Checked;
    }

    void MainGrid_CellValueChanged(object Sender, DataGridViewCellEventArgs Args)
    {
      InvalidateSample();
    }

    void MainGrid_DataBindingComplete(object Sender, DataGridViewBindingCompleteEventArgs Args)
    {
      InvalidateSample();
    }

    void Checker_Validating(object Sender, EFPValidatingEventArgs Args)
    {
      if (FHieConfig != null)
      {
        if (FHieConfig.SelectedLevelCount == 0)
        {
          Args.SetError("������ ���� ������ ���� �� ���� ������� ��������");
          return;
        }

        for (int i = 0; i < FHieConfig.AllLevels.Length; i++)
        {
          if (FHieConfig.AllLevels[i].Visible && FHieConfig.AllLevels[i].Requred)
          {
            if (!FHieConfig.GetSelected(FHieConfig.AllLevels[i].Name))
            {
              Args.SetError("������� \"" + FHieConfig.AllLevels[i].DisplayName + "\" ������ ���� ������");
              return;
            }
          }
        }
      }
    }

    void MainGridHandler_EditData(object Sender, EventArgs Args)
    {
      if (!MainGridHandler.CheckSingleRow())
        return;

      int LevelIndex = DataTools.GetInt(MainGridHandler.CurrentDataRow, "LevelIndex");
      HieViewLevel Level = HieConfig.AllLevels[LevelIndex];
      if (Level.ParamEditor == null)
      {
        EFPApp.ShowTempMessage("���� ������� �������� �� �������������");
        return;
      }

      if (Level.ParamEditor.PerformEdit(Level))
      {
        HieConfig.RefreshTable();
        InvalidateSample();
      }
    }

    #endregion

    #region ���� ��������

    private DataTable SampleTable;

    private EFPAccDepGrid SampleHandler;

    private void InitSampleHandler()
    {
      SampleTable = new DataTable();
      SampleTable.Columns.Add("�����", typeof(string));
      SampleTable.Columns.Add("������", typeof(int));
      SampleTable.Columns.Add("�������", typeof(int));

      SampleHandler.Control.ReadOnly = true;
      SampleHandler.Control.ColumnHeadersVisible = false;
      SampleHandler.Control.RowHeadersVisible = false;
      SampleHandler.Control.MultiSelect = false;
      SampleHandler.Control.AutoGenerateColumns = false;

      SampleHandler.Columns.AddTextFill("�����", true, String.Empty, 100, 5);
      SampleHandler.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(SampleHandler_GetCellAttributes);
      SampleHandler.HideSelection = true;
      SampleHandler.ReadOnly = true;
      SampleHandler.CanView = false;

      // ������� ���� �� �����
      // SampleHandler.SetCommandItems(null);

      SampleHandler.Control.DataSource = SampleTable;

      // ���������� ������� ��������� �� �������, �.�. �������, ���������� � 
      // ����������, ����� ��������� ����� ��� �� ���� �������� ���������
      SampleTimer = new Timer();
      SampleTimer.Interval = 100;
      SampleTimer.Tick += new EventHandler(SampleTimer_Tick);
      SampleTimer.Enabled = true;
      SampleHandler.Control.Disposed += new EventHandler(SampleGrid_Disposed);
    }

    void SampleGrid_Disposed(object Sender, EventArgs Args)
    {
      SampleTimer.Enabled = false;
      SampleTimer.Dispose();
      SampleTimer = null;
    }

    void SampleHandler_GetCellAttributes(object Sender, EFPDataGridViewCellAttributesEventArgs Args)
    {
      DataRow Row = Args.DataRow;
      if (Row == null)
        return;
      Args.IndentLevel = DataTools.GetInt(Row, "������");
      int Level = DataTools.GetInt(Row, "�������");
      switch (Level)
      {
        case -1:
          Args.ColorType = EFPDataGridViewColorType.TotalRow;
          break;
        case -2:
          Args.ColorType = EFPDataGridViewColorType.Header;
          break;
        case 0:
          break;
        default:
          if (Level % 2 == 1)
            Args.ColorType = EFPDataGridViewColorType.Total2;
          else
            Args.ColorType = EFPDataGridViewColorType.Total1;
          break;
      }
    }

    private bool SampleIsDirty;

    private Timer SampleTimer;

    /// <summary>
    /// ���������� �������� ��������
    /// </summary>
    private void InvalidateSample()
    {
      if (SampleIsDirty)
        return;
      SampleIsDirty = true;
    }

    void SampleTimer_Tick(object Sender, EventArgs Args)
    {
      if (SampleIsDirty)
      {
        SampleIsDirty = false;
        SampleTable.BeginLoadData();
        try
        {
          SampleTable.Rows.Clear();
          if (HieConfig != null)
          {
            HieViewLevel[] Levels = HieConfig.SelectedLevels;
            for (int i = Levels.Length - 1; i > 0; i--)
            {
              HieViewLevel Level = Levels[i];
              SampleTable.Rows.Add(Level.DisplayName, Levels.Length - i - 1, -2);
            }
            if (Levels.Length > 0)
              SampleTable.Rows.Add(Levels[0].DisplayName, Levels.Length - 1, 0);
            for (int i = 1; i < Levels.Length; i++)
            {
              HieViewLevel Level = Levels[i];
              SampleTable.Rows.Add("�����: " + Level.DisplayName, Levels.Length - i, i);
            }
            SampleTable.Rows.Add("�����", 0, -1);
          }
        }
        finally
        {
          SampleTable.EndLoadData();
        }
        SampleHandler.Control.Invalidate();

        if (FHieConfig != null)
        {
          FHieConfig.CheckOrderRules();
          MainGridHandler.Control.Invalidate();
        }

        // ����� ��������� ���������, ���� ��� ����
        InitControlledObject();
        SampleIsDirty = false; // ��� ���, �.�. ��������� CheckOrderRules
      } // SampleIsDirty
    }

    /// <summary>
    /// ���������� ��������� ��� �������� ControlledTabPage
    /// </summary>
    private void InitControlledObject()
    {
      if (FControlledTabPage == null)
        return;

      if (FControlledTabPage.Parent != null)
        ((TabControl)(FControlledTabPage.Parent)).ShowToolTips = true;

      if (FHieConfig == null)
      {
        FControlledTabPage.ToolTipText = "������ �� ������������";
        FControlledTabPage.ImageKey = "UnknownState";
      }
      else
      {
        if (FHieConfig.SelectedLevelCount == 0)
        {
          FControlledTabPage.ToolTipText = "������ �� �������";
          FControlledTabPage.ImageKey = "Error";
        }
        else if (FHieConfig.UnselectedRequiredLevels.Length > 0)
        {
          if (FHieConfig.UnselectedRequiredLevels.Length > 1)
            FControlledTabPage.ToolTipText = "�� ������� ������������ ������: " + FHieConfig.UnselectedRequiredLevelsTitle;
          else
            FControlledTabPage.ToolTipText = "�� ������ ������������ �������: " + FHieConfig.UnselectedRequiredLevelsTitle;
          FControlledTabPage.ImageKey = "Error";
        }
        else
        {
          FControlledTabPage.ToolTipText = "��������� ������: " + FHieConfig.SelectedLevelsTitle;
          if (FHieConfig.OrderRules.HasViolation)
          {
            FControlledTabPage.ImageKey = "Warning";
            FControlledTabPage.ToolTipText += "\r\n������������ ������� �������";
          }
          else
            FControlledTabPage.ImageKey = "�����������";
        }
      }
    }

    #endregion

    #region ����������� ����� �������������� - �������� �������

    public static bool PerformEdit(HieViewLevelConfig HieConfig)
    {
      HieViewLevelEditForm Form = new HieViewLevelEditForm();
      //EFPFormProvider efpForm = new EFPFormProvider(Form);

      // ��������� ������ ������
      int RH = Form.MainGrid.Font.Height + 9;
      int WantedGridH = RH * HieConfig.AllLevels.Length;
      int IncH = WantedGridH - Form.MainGrid.ClientSize.Height;
      if (IncH > 0)
        Form.Height = Math.Min(Form.Height + IncH, Screen.FromControl(Form).WorkingArea.Height);

      HieViewLevelConfigEditHandler Handler = new HieViewLevelConfigEditHandler(Form);
      Handler.HieConfig = HieConfig;
      return EFPApp.ShowDialog(Form, true) == DialogResult.OK;
    }

    #endregion
  }

}