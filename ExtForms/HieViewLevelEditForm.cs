using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using FreeLibSet.Config;

namespace FreeLibSet.Forms
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

#if XXX

  /// <summary>
  /// �������� ������� ����������� (������ ������������� � ������� �������)
  /// � ������� ���������� ���������.
  /// �������� ������������ � ������ ������� ���������� �������������� ������.
  /// � ������������ ����������� ������ �� ������ ������ ��� ���������� ��������� ���������
  /// � BaseProvider.
  /// ����� ��������������� �������� HieConfig, ������� ��������� �� ������,
  /// ���������� ������� ������ �������. ��� ����, � ��������� �������� ����������� ������.
  /// ����� ������������ �������� ����������� ����� ��� ������������ ������,
  /// ��������� ���������� �������� � �������������� EFPHieViewConfig.
  /// ���� ��������� � ������ ������ ������� ���������� ������ �������� � ���������
  /// ������ ��������� ������� ��������, �� ��������������� �������� ��������
  /// HieViewLevel.Visible, Required � ��. ����� ������ ���� ������ �����
  /// ����� ������� RefreshTable() ��� ���������� ����� ���������� ��������� �
  /// ���� �������.
  /// 
  /// ���� ��������� ������������� �������� �������, ��� ����� ������� �� ��������� ������,
  /// ��������� �������� ������ �����������
  /// </summary>
  public class EFPHieViewConfigEditor
  {
    #region ��������� �������� ��������� ���������

    /// <summary>
    /// ��������� �������� ��� �������������� ������ �������
    /// </summary>
    public class EFPLevelGridView : EFPDataGridView
    {
      #region �����������

      public EFPLevelGridView(Control ParentControl, EFPBaseProvider BaseProvider)
        : base(BaseProvider, new DataGridView())
      {
        base.Control.Dock = DockStyle.Fill;
        ParentControl.Controls.Add(base.Control);

        Panel ToolBarPanel = new Panel();
        ToolBarPanel.Dock = DockStyle.Top;
        ParentControl.Controls.Add(ToolBarPanel);

        base.Control.AutoGenerateColumns = false;
        base.Columns.AddImage();
        base.Columns.AddBool("Flag", true, "����");
        base.Columns.AddTextFill("Name", true, "��������", 100, 10);
        base.Columns.AddInt("RowOrder", true, "RowOrder", 3);
        base.Columns.LastAdded.GridColumn.ReadOnly = true;
        base.DisableOrdering();
        base.Control.ColumnHeadersVisible = false;
        //MainGridHandler.MainGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        base.ReadOnly = true;
        base.CanView = false;
        base.CanInsert = false;
        base.CanDelete = false;
        base.Control.ReadOnly = false;
        base.CanMultiEdit = false;
        base.EditData += new EventHandler(MainGridHandler_EditData);

        base.MarkRowsColumnIndex = 1;
        base.UseRowImages = true;
        base.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(MainGridHandler_GetRowAttributes);
        base.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(MainGridHandler_GetCellAttributes);
        base.Control.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(MainGrid_DataBindingComplete);
        base.Control.CellValueChanged += new DataGridViewCellEventHandler(MainGrid_CellValueChanged);

        base.CommandItems.ManualOrderColumn = "RowOrder";
        base.CommandItems.DefaultManualOrderColumn = "DefRowOrder";
        base.CommandItems.UseRowErrors = false;

        base.ToolBarPanel = ToolBarPanel;
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
        //  Args.UserImage = EFPApp.MainImages.Images["Anchor"];
        if (DataTools.GetBool(Row, "FlagIsFixed"))
        {
          Args.Grayed = true;
          if (!DataTools.GetBool(Row, "Flag"))
            Args.AddRowError("���� ������� ����������� �������� ������������");
        }

        if (!Args.DataRow.IsNull("ViolatedOrderRuleIndex"))
        {
          int RuleIndex = DataTools.GetInt(Args.DataRow, "ViolatedOrderRuleIndex");
          EFPHieViewLevel lvl1 = HieConfig.AllLevels[HieConfig.OrderRules[RuleIndex].UpperLevelIndex];
          EFPHieViewLevel lvl2 = HieConfig.AllLevels[HieConfig.OrderRules[RuleIndex].LowerLevelIndex];
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
              Args.ToolTipText += Environment.NewLine+ + "���������: " + Cfg;
            break;
          case 2:
            Args.IndentLevel = Args.RowIndex;
            Args.ToolTipText = DataTools.GetString(Args.DataRow, "Name");
            if (OrderRuleViolated)
              Args.ColorType = EFPDataGridViewColorType.Warning;
            break;
        }
      }

      void MainGrid_CellValueChanged(object Sender, DataGridViewCellEventArgs Args)
      {
        OnChanged();
      }

      void MainGrid_DataBindingComplete(object Sender, DataGridViewBindingCompleteEventArgs Args)
      {
        OnChanged();
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
        if (!base.CheckSingleRow())
          return;

        int LevelIndex = DataTools.GetInt(base.CurrentDataRow, "LevelIndex");
        EFPHieViewLevel Level = HieConfig.AllLevels[LevelIndex];
        if (Level.ParamEditor == null)
        {
          EFPApp.ShowTempMessage("���� ������� �������� �� �������������");
          return;
        }

        if (Level.ParamEditor.PerformEdit(Level))
        {
          HieConfig.RefreshTable();
          OnChanged();
        }
      }

      #endregion

  #region �������� HieConfig

      /// <summary>
      /// �������������� ������������ �������. �������� ������������ ����� ��������
      /// � ��������� �� �������
      /// ��� ��������� �������� ����������� ������������� ��������� ������
      /// HieViewLevelConfig.View � ���������� ���������
      /// </summary>
      public EFPHieViewConfig HieConfig
      {
        get { return FHieConfig; }
        set
        {
          FHieConfig = value;
          if (value == null)
          {
            base.Control.DataSource = null;
            base.ReadOnly = true;
          }
          else
          {
            base.Control.DataSource = value.View;
            base.ReadOnly = !value.HasParamEditor;
          }

          //OnChanged();
        }
      }
      private EFPHieViewConfig FHieConfig;

  #endregion

  #region ������� Changed

      /// <summary>
      /// ������� ���������� ��� ��������� ���������
      /// </summary>
      public event EventHandler Changed;

      protected virtual void OnChanged()
      {
        if (Changed != null)
          Changed(this, EventArgs.Empty);
      }

  #endregion
    }

    #endregion

  #region ��������� �������� �������

    /// <summary>
    /// ��������� �������� �������
    /// </summary>
    public class EFPSampleGridView : EFPDataGridView
    {
  #region ����������� �������

      public EFPSampleGridView(Control ParentControl, EFPBaseProvider BaseProvider)
        : base(BaseProvider, new DataGridView())
      {
        base.Control.Dock = DockStyle.Fill;
        ParentControl.Controls.Add(base.Control);

        SampleTable = new DataTable();
        SampleTable.Columns.Add("�����", typeof(string));
        SampleTable.Columns.Add("������", typeof(int));
        SampleTable.Columns.Add("�������", typeof(int));

        base.Control.ReadOnly = true;
        base.Control.ColumnHeadersVisible = false;
        base.Control.RowHeadersVisible = false;
        base.Control.MultiSelect = false;
        base.Control.AutoGenerateColumns = false;

        base.Columns.AddTextFill("�����", true, String.Empty, 100, 5);
        base.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(SampleHandler_GetCellAttributes);
        base.HideSelection = true;
        base.ReadOnly = true;
        base.CanView = false;

        // ������� ���� �� �����
        // SampleHandler.SetCommandItems(null);

        base.Control.DataSource = SampleTable;

        // ���������� ������� ��������� �� �������, �.�. �������, ���������� � 
        // ����������, ����� ��������� ����� ��� �� ���� �������� ���������
        SampleTimer = new Timer();
        SampleTimer.Interval = 100;
        SampleTimer.Tick += new EventHandler(SampleTimer_Tick);
        SampleTimer.Enabled = true;
        base.Control.Disposed += new EventHandler(SampleGrid_Disposed);
      }

  #endregion

  #region ������� ������ ��� ���������

      private DataTable SampleTable;

  #endregion

  #region �������� HieConfig

      /// <summary>
      /// ������������ ������������.
      /// ��������� �������� �������� � ���������� ���� �������
      /// </summary>
      public EFPHieViewConfig HieConfig
      {
        get { return FHieConfig; }
        set
        {
          FHieConfig = value;
          SampleIsDirty = true;
        }
      }
      private EFPHieViewConfig FHieConfig;

  #endregion

  #region ��������� ���������� ���������

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

  #endregion

  #region ��������� �������

      /// <summary>
      /// ���������� ���������� ���� �������
      /// </summary>
      private bool SampleIsDirty;

      private Timer SampleTimer;

      void SampleGrid_Disposed(object Sender, EventArgs Args)
      {
        SampleTimer.Enabled = false;
        SampleTimer.Dispose();
        SampleTimer = null;
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
              EFPHieViewLevel[] Levels = HieConfig.SelectedLevels;
              for (int i = Levels.Length - 1; i > 0; i--)
              {
                EFPHieViewLevel Level = Levels[i];
                SampleTable.Rows.Add(Level.DisplayName, Levels.Length - i - 1, -2);
              }
              if (Levels.Length > 0)
                SampleTable.Rows.Add(Levels[0].DisplayName, Levels.Length - 1, 0);
              for (int i = 1; i < Levels.Length; i++)
              {
                EFPHieViewLevel Level = Levels[i];
                SampleTable.Rows.Add("�����: " + Level.DisplayName, Levels.Length - i, i);
              }
              SampleTable.Rows.Add("�����", 0, -1);
            }
          }
          finally
          {
            SampleTable.EndLoadData();
          }
          base.Control.Invalidate();

          if (FHieConfig != null)
          {
            FHieConfig.CheckOrderRules();
            base.Control.Invalidate();
          }

          SampleIsDirty = false; // ��� ���, �.�. ��������� CheckOrderRules
        } // SampleIsDirty
      }

  #endregion
    }

  #endregion

  #region ���������� �������� TabControl

    /// <summary>
    /// ���������� ������� TabPage ��� ������������� ������ � ��������� �� ������
    /// </summary>
    public class TabPageController
    {
  #region �����������

      public TabPageController(TabPage TabPage)
      {
        if (TabPage == null)
          throw new ArgumentNullException("TabPage");
        FTabPage = TabPage;
      }

      public TabPage TabPage { get { return FTabPage; } }
      private TabPage FTabPage;

  #endregion

  #region �������� HieConfig

      /// <summary>
      /// ��������� �������� �������� � ���������� ������ ��������
      /// </summary>
      public EFPHieViewConfig HieConfig
      {
        get { return FHieConfig; }
        set
        {
          FHieConfig = value;

          if (FTabPage.Parent != null)
            ((TabControl)(TabPage.Parent)).ShowToolTips = true;

          if (FHieConfig == null)
          {
            TabPage.ToolTipText = "������ �� ������������";
            TabPage.ImageKey = "UnknownState";
          }
          else
          {
            if (HieConfig.SelectedLevelCount == 0)
            {
              TabPage.ToolTipText = "������ �� �������";
              TabPage.ImageKey = "Error";
            }
            else if (HieConfig.UnselectedRequiredLevels.Length > 0)
            {
              if (HieConfig.UnselectedRequiredLevels.Length > 1)
                TabPage.ToolTipText = "�� ������� ������������ ������: " + HieConfig.UnselectedRequiredLevelsTitle;
              else
                TabPage.ToolTipText = "�� ������ ������������ �������: " + HieConfig.UnselectedRequiredLevelsTitle;
              TabPage.ImageKey = "Error";
            }
            else
            {
              TabPage.ToolTipText = "��������� ������: " + HieConfig.SelectedLevelsTitle;
              if (HieConfig.OrderRules.HasViolation)
              {
                TabPage.ImageKey = "Warning";
                TabPage.ToolTipText += Environment.NewLine+"������������ ������� �������";
              }
              else
                TabPage.ImageKey = "�����������";
            }
          }

        }
      }
      private EFPHieViewConfig FHieConfig;

  #endregion
    }

  #endregion

  #region �����������

    /// <summary>
    /// ���������� �� ������ ���������� ��������� � ������� ������������ �
    /// CheckBox'� � �����������
    /// </summary>
    /// <param name="ParentControl"></param>
    public EFPHieViewConfigEditor(Control ParentControl, EFPBaseProvider BaseProvider)
    {
      HieViewLevelEditForm Form = new HieViewLevelEditForm();
      ParentControl.Controls.Add(Form.MainPanel);
      Form.Dispose();

      DoInit(Form, BaseProvider);

      if (ParentControl is TabPage)
        TPController = new TabPageController((TabPage)ParentControl);
    }

    private EFPHieViewConfigEditor(HieViewLevelEditForm Form)
    {
      EFPFormProvider efpForm = new EFPFormProvider(Form);
      DoInit(Form, efpForm);
    }

    private void DoInit(HieViewLevelEditForm Form, EFPBaseProvider BaseProvider)
    {
      efpMainGridHandler = new EFPLevelGridView(Form.grpMain, BaseProvider);

      efpHideExtraSumRows = new EFPCheckBox(BaseProvider, Form.cbHideExtraSumRows);
      efpHideExtraSumRows.ToolTipText = "���� ������ ����������, �� ��� ������� ��������, ���������� ������������ ������ ������ �� ����� ���������� �������� ������." +Environment.NewLine+
        "���� ������ ����, �� ������ ������������� ������ ����� ���������� ������";
      efpHideExtraSumRows.CheckedEx.ValueChanged += new EventHandler(efpHideExtraSumRows_CheckedChanged);



      efpSample = new EFPSampleGridView(Form.grpSample, BaseProvider);

      // ������ �������� �����, ���� �� ������� �� ������ ������
      EFPFormCheck Checker = new EFPFormCheck();
      Checker.Validating += new EFPValidatingEventHandler(Checker_Validating);
      Checker.FocusControl = efpMainGridHandler.Control;
      BaseProvider.Add(Checker);

      efpMainGridHandler.Changed += new EventHandler(efpMainGridHandler_Changed);
    }

  #endregion

  #region �������� ����������

    /// <summary>
    /// �������� ���������� ���������� ��������� (��������� � ������������)
    /// </summary>
    private EFPLevelGridView efpMainGridHandler;

    private EFPCheckBox efpHideExtraSumRows;

    private EFPSampleGridView efpSample;

    private TabPageController TPController;

  #endregion

  #region ������������� ������������

    /// <summary>
    /// �������������� ������������ �������. �������� ������������ ����� ��������
    /// � ��������� �� �������
    /// ��� ��������� �������� ����������� ������������� ��������� ������
    /// HieViewLevelConfig.View � ���������� ���������
    /// </summary>
    public EFPHieViewConfig HieConfig
    {
      get { return efpMainGridHandler.HieConfig; }
      set
      {
        efpMainGridHandler.HieConfig = value;
        if (value == null)
        {
          efpHideExtraSumRows.Enabled = false;
        }
        else
        {
          efpHideExtraSumRows.Checked = value.HideExtraSumRows;
          efpHideExtraSumRows.Enabled = !value.HideExtraSumRowsFixed;
        }
        efpSample.HieConfig = value;
        if (TabPageController != null)
          TabPageController.HieConfig = value;
      }
    }

  #endregion


  #region �����������

    void efpHideExtraSumRows_CheckedChanged(object Sender, EventArgs Args)
    {
      if (HieConfig != null)
        HieConfig.HideExtraSumRows = efpHideExtraSumRows.Checked;
    }

    void efpMainGridHandler_Changed(object Sender, EventArgs Args)
    {
      efpSample.HieConfig = value;
      if (TabPageController != null)
        TabPageController.HieConfig = value;
    }


  #endregion

  #region ����������� ����� �������������� - �������� �������

    public static bool PerformEdit(EFPHieViewConfig HieConfig)
    {
      HieViewLevelEditForm Form = new HieViewLevelEditForm();
      //EFPFormProvider efpForm = new EFPFormProvider(Form);

      // ��������� ������ ������
      int RH = Form.MainGrid.Font.Height + 9;
      int WantedGridH = RH * HieConfig.AllLevels.Length;
      int IncH = WantedGridH - Form.MainGrid.ClientSize.Height;
      if (IncH > 0)
        Form.Height = Math.Min(Form.Height + IncH, Screen.FromControl(Form).WorkingArea.Height);

      EFPHieViewConfigEditor Handler = new EFPHieViewConfigEditor(Form);
      Handler.HieConfig = HieConfig;
      return EFPApp.ShowDialog(Form, true) == DialogResult.OK;
    }

  #endregion
  }
#endif
}