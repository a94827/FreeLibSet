namespace FreeLibSet.Forms.Docs
{
  partial class DocInfoForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocInfoForm));
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.grpRefStat = new System.Windows.Forms.GroupBox();
      this.grRefStat = new System.Windows.Forms.DataGridView();
      this.panSpbRefStat = new System.Windows.Forms.Panel();
      this.grpRefDet = new System.Windows.Forms.GroupBox();
      this.grRefDet = new System.Windows.Forms.DataGridView();
      this.panSpbRefDet = new System.Windows.Forms.Panel();
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.tpInfo = new System.Windows.Forms.TabPage();
      this.Panel1 = new System.Windows.Forms.Panel();
      this.grpHistory = new System.Windows.Forms.GroupBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.lblChanged = new System.Windows.Forms.Label();
      this.edId = new System.Windows.Forms.TextBox();
      this.DocImageLabel = new System.Windows.Forms.Label();
      this.lblStatus = new System.Windows.Forms.Label();
      this.edVersion = new System.Windows.Forms.TextBox();
      this.edDocText = new System.Windows.Forms.TextBox();
      this.label8 = new System.Windows.Forms.Label();
      this.lblVersion = new System.Windows.Forms.Label();
      this.edStatus = new System.Windows.Forms.TextBox();
      this.lblCreated = new System.Windows.Forms.Label();
      this.edCreateTime = new System.Windows.Forms.TextBox();
      this.edChangeTime = new System.Windows.Forms.TextBox();
      this.edCreateUser = new System.Windows.Forms.TextBox();
      this.edChangeUser = new System.Windows.Forms.TextBox();
      this.btnDebug = new System.Windows.Forms.Button();
      this.tpRefs = new System.Windows.Forms.TabPage();
      this.Panel2 = new System.Windows.Forms.Panel();
      this.lblUserDocsInfo = new FreeLibSet.Controls.InfoLabel();
      this.lblNoRefs = new FreeLibSet.Controls.InfoLabel();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.grpRefStat.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grRefStat)).BeginInit();
      this.grpRefDet.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grRefDet)).BeginInit();
      this.TheTabControl.SuspendLayout();
      this.tpInfo.SuspendLayout();
      this.Panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.tpRefs.SuspendLayout();
      this.Panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      resources.ApplyResources(this.splitContainer1, "splitContainer1");
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
      this.splitContainer1.Panel1.Controls.Add(this.grpRefStat);
      // 
      // splitContainer1.Panel2
      // 
      resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
      this.splitContainer1.Panel2.Controls.Add(this.grpRefDet);
      // 
      // grpRefStat
      // 
      resources.ApplyResources(this.grpRefStat, "grpRefStat");
      this.grpRefStat.Controls.Add(this.grRefStat);
      this.grpRefStat.Controls.Add(this.panSpbRefStat);
      this.grpRefStat.Name = "grpRefStat";
      this.grpRefStat.TabStop = false;
      // 
      // grRefStat
      // 
      resources.ApplyResources(this.grRefStat, "grRefStat");
      this.grRefStat.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grRefStat.Name = "grRefStat";
      // 
      // panSpbRefStat
      // 
      resources.ApplyResources(this.panSpbRefStat, "panSpbRefStat");
      this.panSpbRefStat.Name = "panSpbRefStat";
      // 
      // grpRefDet
      // 
      resources.ApplyResources(this.grpRefDet, "grpRefDet");
      this.grpRefDet.Controls.Add(this.grRefDet);
      this.grpRefDet.Controls.Add(this.panSpbRefDet);
      this.grpRefDet.Name = "grpRefDet";
      this.grpRefDet.TabStop = false;
      // 
      // grRefDet
      // 
      resources.ApplyResources(this.grRefDet, "grRefDet");
      this.grRefDet.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grRefDet.Name = "grRefDet";
      // 
      // panSpbRefDet
      // 
      resources.ApplyResources(this.panSpbRefDet, "panSpbRefDet");
      this.panSpbRefDet.Name = "panSpbRefDet";
      // 
      // TheTabControl
      // 
      resources.ApplyResources(this.TheTabControl, "TheTabControl");
      this.TheTabControl.Controls.Add(this.tpInfo);
      this.TheTabControl.Controls.Add(this.tpRefs);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      // 
      // tpInfo
      // 
      resources.ApplyResources(this.tpInfo, "tpInfo");
      this.tpInfo.Controls.Add(this.Panel1);
      this.tpInfo.Name = "tpInfo";
      this.tpInfo.UseVisualStyleBackColor = true;
      // 
      // Panel1
      // 
      resources.ApplyResources(this.Panel1, "Panel1");
      this.Panel1.Controls.Add(this.grpHistory);
      this.Panel1.Controls.Add(this.groupBox1);
      this.Panel1.Name = "Panel1";
      // 
      // grpHistory
      // 
      resources.ApplyResources(this.grpHistory, "grpHistory");
      this.grpHistory.Name = "grpHistory";
      this.grpHistory.TabStop = false;
      // 
      // groupBox1
      // 
      resources.ApplyResources(this.groupBox1, "groupBox1");
      this.groupBox1.Controls.Add(this.tableLayoutPanel2);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.TabStop = false;
      // 
      // tableLayoutPanel2
      // 
      resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
      this.tableLayoutPanel2.Controls.Add(this.lblChanged, 0, 3);
      this.tableLayoutPanel2.Controls.Add(this.edId, 1, 1);
      this.tableLayoutPanel2.Controls.Add(this.DocImageLabel, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.lblStatus, 4, 1);
      this.tableLayoutPanel2.Controls.Add(this.edVersion, 3, 1);
      this.tableLayoutPanel2.Controls.Add(this.edDocText, 1, 0);
      this.tableLayoutPanel2.Controls.Add(this.label8, 0, 1);
      this.tableLayoutPanel2.Controls.Add(this.lblVersion, 2, 1);
      this.tableLayoutPanel2.Controls.Add(this.edStatus, 5, 1);
      this.tableLayoutPanel2.Controls.Add(this.lblCreated, 0, 2);
      this.tableLayoutPanel2.Controls.Add(this.edCreateTime, 1, 2);
      this.tableLayoutPanel2.Controls.Add(this.edChangeTime, 1, 3);
      this.tableLayoutPanel2.Controls.Add(this.edCreateUser, 3, 2);
      this.tableLayoutPanel2.Controls.Add(this.edChangeUser, 3, 3);
      this.tableLayoutPanel2.Controls.Add(this.btnDebug, 5, 3);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      // 
      // lblChanged
      // 
      resources.ApplyResources(this.lblChanged, "lblChanged");
      this.lblChanged.Name = "lblChanged";
      // 
      // edId
      // 
      resources.ApplyResources(this.edId, "edId");
      this.edId.Name = "edId";
      this.edId.ReadOnly = true;
      // 
      // DocImageLabel
      // 
      resources.ApplyResources(this.DocImageLabel, "DocImageLabel");
      this.DocImageLabel.Name = "DocImageLabel";
      // 
      // lblStatus
      // 
      resources.ApplyResources(this.lblStatus, "lblStatus");
      this.lblStatus.Name = "lblStatus";
      // 
      // edVersion
      // 
      resources.ApplyResources(this.edVersion, "edVersion");
      this.edVersion.Name = "edVersion";
      this.edVersion.ReadOnly = true;
      // 
      // edDocText
      // 
      resources.ApplyResources(this.edDocText, "edDocText");
      this.tableLayoutPanel2.SetColumnSpan(this.edDocText, 5);
      this.edDocText.Name = "edDocText";
      this.edDocText.ReadOnly = true;
      // 
      // label8
      // 
      resources.ApplyResources(this.label8, "label8");
      this.label8.Name = "label8";
      // 
      // lblVersion
      // 
      resources.ApplyResources(this.lblVersion, "lblVersion");
      this.lblVersion.Name = "lblVersion";
      // 
      // edStatus
      // 
      resources.ApplyResources(this.edStatus, "edStatus");
      this.edStatus.Name = "edStatus";
      this.edStatus.ReadOnly = true;
      // 
      // lblCreated
      // 
      resources.ApplyResources(this.lblCreated, "lblCreated");
      this.lblCreated.Name = "lblCreated";
      // 
      // edCreateTime
      // 
      resources.ApplyResources(this.edCreateTime, "edCreateTime");
      this.tableLayoutPanel2.SetColumnSpan(this.edCreateTime, 2);
      this.edCreateTime.Name = "edCreateTime";
      this.edCreateTime.ReadOnly = true;
      // 
      // edChangeTime
      // 
      resources.ApplyResources(this.edChangeTime, "edChangeTime");
      this.tableLayoutPanel2.SetColumnSpan(this.edChangeTime, 2);
      this.edChangeTime.Name = "edChangeTime";
      this.edChangeTime.ReadOnly = true;
      // 
      // edCreateUser
      // 
      resources.ApplyResources(this.edCreateUser, "edCreateUser");
      this.tableLayoutPanel2.SetColumnSpan(this.edCreateUser, 2);
      this.edCreateUser.Name = "edCreateUser";
      this.edCreateUser.ReadOnly = true;
      // 
      // edChangeUser
      // 
      resources.ApplyResources(this.edChangeUser, "edChangeUser");
      this.tableLayoutPanel2.SetColumnSpan(this.edChangeUser, 2);
      this.edChangeUser.Name = "edChangeUser";
      this.edChangeUser.ReadOnly = true;
      // 
      // btnDebug
      // 
      resources.ApplyResources(this.btnDebug, "btnDebug");
      this.btnDebug.Name = "btnDebug";
      this.btnDebug.UseVisualStyleBackColor = true;
      // 
      // tpRefs
      // 
      resources.ApplyResources(this.tpRefs, "tpRefs");
      this.tpRefs.Controls.Add(this.Panel2);
      this.tpRefs.Name = "tpRefs";
      this.tpRefs.UseVisualStyleBackColor = true;
      // 
      // Panel2
      // 
      resources.ApplyResources(this.Panel2, "Panel2");
      this.Panel2.Controls.Add(this.splitContainer1);
      this.Panel2.Controls.Add(this.lblUserDocsInfo);
      this.Panel2.Controls.Add(this.lblNoRefs);
      this.Panel2.Name = "Panel2";
      // 
      // lblUserDocsInfo
      // 
      resources.ApplyResources(this.lblUserDocsInfo, "lblUserDocsInfo");
      this.lblUserDocsInfo.Icon = System.Windows.Forms.MessageBoxIcon.Asterisk;
      this.lblUserDocsInfo.Name = "lblUserDocsInfo";
      // 
      // lblNoRefs
      // 
      resources.ApplyResources(this.lblNoRefs, "lblNoRefs");
      this.lblNoRefs.Icon = System.Windows.Forms.MessageBoxIcon.Asterisk;
      this.lblNoRefs.Name = "lblNoRefs";
      // 
      // DocInfoForm
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.TheTabControl);
      this.Name = "DocInfoForm";
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.grpRefStat.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grRefStat)).EndInit();
      this.grpRefDet.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grRefDet)).EndInit();
      this.TheTabControl.ResumeLayout(false);
      this.tpInfo.ResumeLayout(false);
      this.Panel1.ResumeLayout(false);
      this.Panel1.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tableLayoutPanel2.ResumeLayout(false);
      this.tableLayoutPanel2.PerformLayout();
      this.tpRefs.ResumeLayout(false);
      this.Panel2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl TheTabControl;
    public System.Windows.Forms.TabPage tpInfo;
    public System.Windows.Forms.TabPage tpRefs;
    public System.Windows.Forms.GroupBox grpHistory;
    public System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.Label lblChanged;
    private System.Windows.Forms.TextBox edId;
    public System.Windows.Forms.Label DocImageLabel;
    private System.Windows.Forms.Label lblStatus;
    private System.Windows.Forms.TextBox edVersion;
    public System.Windows.Forms.TextBox edDocText;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label lblVersion;
    private System.Windows.Forms.TextBox edStatus;
    private System.Windows.Forms.Label lblCreated;
    private System.Windows.Forms.TextBox edCreateUser;
    private System.Windows.Forms.TextBox edCreateTime;
    private System.Windows.Forms.TextBox edChangeUser;
    private System.Windows.Forms.TextBox edChangeTime;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.GroupBox grpRefStat;
    private System.Windows.Forms.DataGridView grRefStat;
    private System.Windows.Forms.Panel panSpbRefStat;
    private System.Windows.Forms.GroupBox grpRefDet;
    private System.Windows.Forms.DataGridView grRefDet;
    private System.Windows.Forms.Panel panSpbRefDet;
    public FreeLibSet.Controls.InfoLabel lblUserDocsInfo;
    public FreeLibSet.Controls.InfoLabel lblNoRefs;
    public System.Windows.Forms.Panel Panel1;
    public System.Windows.Forms.Panel Panel2;
    private System.Windows.Forms.Button btnDebug;
  }
}
