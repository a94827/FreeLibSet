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
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.tpInfo = new System.Windows.Forms.TabPage();
      this.Panel1 = new System.Windows.Forms.Panel();
      this.grpHist = new System.Windows.Forms.GroupBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.label4 = new System.Windows.Forms.Label();
      this.edId = new System.Windows.Forms.TextBox();
      this.DocImageLabel = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.edVersion = new System.Windows.Forms.TextBox();
      this.edDocText = new System.Windows.Forms.TextBox();
      this.label8 = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.edStatus = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.edCreateUser = new System.Windows.Forms.TextBox();
      this.edCreateTime = new System.Windows.Forms.TextBox();
      this.edChangeUser = new System.Windows.Forms.TextBox();
      this.edChangeTime = new System.Windows.Forms.TextBox();
      this.tpRefs = new System.Windows.Forms.TabPage();
      this.Panel2 = new System.Windows.Forms.Panel();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.grpRefStat = new System.Windows.Forms.GroupBox();
      this.grRefStat = new System.Windows.Forms.DataGridView();
      this.panSpbRefStat = new System.Windows.Forms.Panel();
      this.grpRefDet = new System.Windows.Forms.GroupBox();
      this.grRefDet = new System.Windows.Forms.DataGridView();
      this.panSpbRefDet = new System.Windows.Forms.Panel();
      this.lblUserDocsInfo = new FreeLibSet.Controls.InfoLabel();
      this.lblNoRefs = new FreeLibSet.Controls.InfoLabel();
      this.btnDebug = new System.Windows.Forms.Button();
      this.TheTabControl.SuspendLayout();
      this.tpInfo.SuspendLayout();
      this.Panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.tpRefs.SuspendLayout();
      this.Panel2.SuspendLayout();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.grpRefStat.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grRefStat)).BeginInit();
      this.grpRefDet.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grRefDet)).BeginInit();
      this.SuspendLayout();
      // 
      // TheTabControl
      // 
      this.TheTabControl.Controls.Add(this.tpInfo);
      this.TheTabControl.Controls.Add(this.tpRefs);
      this.TheTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTabControl.Location = new System.Drawing.Point(0, 0);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(720, 463);
      this.TheTabControl.TabIndex = 0;
      // 
      // tpInfo
      // 
      this.tpInfo.Controls.Add(this.Panel1);
      this.tpInfo.Location = new System.Drawing.Point(4, 22);
      this.tpInfo.Name = "tpInfo";
      this.tpInfo.Padding = new System.Windows.Forms.Padding(3);
      this.tpInfo.Size = new System.Drawing.Size(712, 437);
      this.tpInfo.TabIndex = 0;
      this.tpInfo.Text = "Информация о документе";
      this.tpInfo.UseVisualStyleBackColor = true;
      // 
      // Panel1
      // 
      this.Panel1.Controls.Add(this.grpHist);
      this.Panel1.Controls.Add(this.groupBox1);
      this.Panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.Panel1.Location = new System.Drawing.Point(3, 3);
      this.Panel1.Name = "Panel1";
      this.Panel1.Size = new System.Drawing.Size(706, 431);
      this.Panel1.TabIndex = 0;
      // 
      // grpHist
      // 
      this.grpHist.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpHist.Location = new System.Drawing.Point(0, 127);
      this.grpHist.Name = "grpHist";
      this.grpHist.Size = new System.Drawing.Size(706, 304);
      this.grpHist.TabIndex = 5;
      this.grpHist.TabStop = false;
      this.grpHist.Text = "История";
      // 
      // groupBox1
      // 
      this.groupBox1.AutoSize = true;
      this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.groupBox1.Controls.Add(this.tableLayoutPanel2);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(706, 127);
      this.groupBox1.TabIndex = 4;
      this.groupBox1.TabStop = false;
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.AutoSize = true;
      this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.tableLayoutPanel2.ColumnCount = 6;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel2.Controls.Add(this.label4, 0, 3);
      this.tableLayoutPanel2.Controls.Add(this.edId, 1, 1);
      this.tableLayoutPanel2.Controls.Add(this.DocImageLabel, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.label9, 4, 1);
      this.tableLayoutPanel2.Controls.Add(this.edVersion, 3, 1);
      this.tableLayoutPanel2.Controls.Add(this.edDocText, 1, 0);
      this.tableLayoutPanel2.Controls.Add(this.label8, 0, 1);
      this.tableLayoutPanel2.Controls.Add(this.label7, 2, 1);
      this.tableLayoutPanel2.Controls.Add(this.edStatus, 5, 1);
      this.tableLayoutPanel2.Controls.Add(this.label1, 0, 2);
      this.tableLayoutPanel2.Controls.Add(this.edCreateTime, 1, 2);
      this.tableLayoutPanel2.Controls.Add(this.edChangeTime, 1, 3);
      this.tableLayoutPanel2.Controls.Add(this.edCreateUser, 3, 2);
      this.tableLayoutPanel2.Controls.Add(this.edChangeUser, 3, 3);
      this.tableLayoutPanel2.Controls.Add(this.btnDebug, 5, 3);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 16);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 4;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel2.Size = new System.Drawing.Size(700, 108);
      this.tableLayoutPanel2.TabIndex = 0;
      // 
      // label4
      // 
      this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label4.Location = new System.Drawing.Point(3, 78);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(74, 24);
      this.label4.TabIndex = 27;
      this.label4.Text = "Изменен";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edId
      // 
      this.edId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edId.Location = new System.Drawing.Point(83, 29);
      this.edId.Name = "edId";
      this.edId.ReadOnly = true;
      this.edId.Size = new System.Drawing.Size(133, 20);
      this.edId.TabIndex = 2;
      // 
      // DocImageLabel
      // 
      this.DocImageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.DocImageLabel.Location = new System.Drawing.Point(3, 0);
      this.DocImageLabel.Name = "DocImageLabel";
      this.DocImageLabel.Size = new System.Drawing.Size(74, 26);
      this.DocImageLabel.TabIndex = 23;
      // 
      // label9
      // 
      this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label9.Location = new System.Drawing.Point(461, 26);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(94, 20);
      this.label9.TabIndex = 3;
      this.label9.Text = "Статус";
      this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edVersion
      // 
      this.edVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edVersion.Location = new System.Drawing.Point(322, 29);
      this.edVersion.Name = "edVersion";
      this.edVersion.ReadOnly = true;
      this.edVersion.Size = new System.Drawing.Size(133, 20);
      this.edVersion.TabIndex = 16;
      // 
      // edDocText
      // 
      this.edDocText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel2.SetColumnSpan(this.edDocText, 5);
      this.edDocText.Location = new System.Drawing.Point(83, 3);
      this.edDocText.Name = "edDocText";
      this.edDocText.ReadOnly = true;
      this.edDocText.Size = new System.Drawing.Size(614, 20);
      this.edDocText.TabIndex = 0;
      // 
      // label8
      // 
      this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label8.Location = new System.Drawing.Point(3, 26);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(74, 20);
      this.label8.TabIndex = 1;
      this.label8.Text = "Id";
      this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label7
      // 
      this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label7.Location = new System.Drawing.Point(222, 26);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(94, 20);
      this.label7.TabIndex = 15;
      this.label7.Text = "Версия";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edStatus
      // 
      this.edStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edStatus.Location = new System.Drawing.Point(561, 29);
      this.edStatus.Name = "edStatus";
      this.edStatus.ReadOnly = true;
      this.edStatus.Size = new System.Drawing.Size(136, 20);
      this.edStatus.TabIndex = 4;
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.Location = new System.Drawing.Point(3, 52);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(74, 19);
      this.label1.TabIndex = 24;
      this.label1.Text = "Создан";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edCreateUser
      // 
      this.edCreateUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel2.SetColumnSpan(this.edCreateUser, 2);
      this.edCreateUser.Location = new System.Drawing.Point(322, 55);
      this.edCreateUser.Name = "edCreateUser";
      this.edCreateUser.ReadOnly = true;
      this.edCreateUser.Size = new System.Drawing.Size(233, 20);
      this.edCreateUser.TabIndex = 26;
      // 
      // edCreateTime
      // 
      this.edCreateTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel2.SetColumnSpan(this.edCreateTime, 2);
      this.edCreateTime.Location = new System.Drawing.Point(83, 55);
      this.edCreateTime.Name = "edCreateTime";
      this.edCreateTime.ReadOnly = true;
      this.edCreateTime.Size = new System.Drawing.Size(233, 20);
      this.edCreateTime.TabIndex = 25;
      // 
      // edChangeUser
      // 
      this.edChangeUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel2.SetColumnSpan(this.edChangeUser, 2);
      this.edChangeUser.Location = new System.Drawing.Point(322, 81);
      this.edChangeUser.Name = "edChangeUser";
      this.edChangeUser.ReadOnly = true;
      this.edChangeUser.Size = new System.Drawing.Size(233, 20);
      this.edChangeUser.TabIndex = 29;
      // 
      // edChangeTime
      // 
      this.edChangeTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel2.SetColumnSpan(this.edChangeTime, 2);
      this.edChangeTime.Location = new System.Drawing.Point(83, 81);
      this.edChangeTime.Name = "edChangeTime";
      this.edChangeTime.ReadOnly = true;
      this.edChangeTime.Size = new System.Drawing.Size(233, 20);
      this.edChangeTime.TabIndex = 28;
      // 
      // tpRefs
      // 
      this.tpRefs.Controls.Add(this.Panel2);
      this.tpRefs.Location = new System.Drawing.Point(4, 22);
      this.tpRefs.Name = "tpRefs";
      this.tpRefs.Padding = new System.Windows.Forms.Padding(3);
      this.tpRefs.Size = new System.Drawing.Size(712, 437);
      this.tpRefs.TabIndex = 1;
      this.tpRefs.Text = "Ссылки";
      this.tpRefs.UseVisualStyleBackColor = true;
      // 
      // Panel2
      // 
      this.Panel2.Controls.Add(this.splitContainer1);
      this.Panel2.Controls.Add(this.lblUserDocsInfo);
      this.Panel2.Controls.Add(this.lblNoRefs);
      this.Panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.Panel2.Location = new System.Drawing.Point(3, 3);
      this.Panel2.Name = "Panel2";
      this.Panel2.Size = new System.Drawing.Size(706, 431);
      this.Panel2.TabIndex = 0;
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.grpRefStat);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.grpRefDet);
      this.splitContainer1.Size = new System.Drawing.Size(706, 383);
      this.splitContainer1.SplitterDistance = 158;
      this.splitContainer1.TabIndex = 5;
      // 
      // grpRefStat
      // 
      this.grpRefStat.Controls.Add(this.grRefStat);
      this.grpRefStat.Controls.Add(this.panSpbRefStat);
      this.grpRefStat.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpRefStat.Location = new System.Drawing.Point(0, 0);
      this.grpRefStat.Name = "grpRefStat";
      this.grpRefStat.Size = new System.Drawing.Size(706, 158);
      this.grpRefStat.TabIndex = 1;
      this.grpRefStat.TabStop = false;
      this.grpRefStat.Text = "Виды документов, которые ссылаются на этот документ";
      // 
      // grRefStat
      // 
      this.grRefStat.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grRefStat.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grRefStat.Location = new System.Drawing.Point(3, 16);
      this.grRefStat.Name = "grRefStat";
      this.grRefStat.Size = new System.Drawing.Size(655, 139);
      this.grRefStat.TabIndex = 0;
      // 
      // panSpbRefStat
      // 
      this.panSpbRefStat.Dock = System.Windows.Forms.DockStyle.Right;
      this.panSpbRefStat.Location = new System.Drawing.Point(658, 16);
      this.panSpbRefStat.Name = "panSpbRefStat";
      this.panSpbRefStat.Size = new System.Drawing.Size(45, 139);
      this.panSpbRefStat.TabIndex = 1;
      // 
      // grpRefDet
      // 
      this.grpRefDet.Controls.Add(this.grRefDet);
      this.grpRefDet.Controls.Add(this.panSpbRefDet);
      this.grpRefDet.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpRefDet.Location = new System.Drawing.Point(0, 0);
      this.grpRefDet.Name = "grpRefDet";
      this.grpRefDet.Size = new System.Drawing.Size(706, 221);
      this.grpRefDet.TabIndex = 0;
      this.grpRefDet.TabStop = false;
      this.grpRefDet.Text = "???";
      // 
      // grRefDet
      // 
      this.grRefDet.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grRefDet.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grRefDet.Location = new System.Drawing.Point(3, 53);
      this.grRefDet.Name = "grRefDet";
      this.grRefDet.Size = new System.Drawing.Size(700, 165);
      this.grRefDet.TabIndex = 1;
      // 
      // panSpbRefDet
      // 
      this.panSpbRefDet.Dock = System.Windows.Forms.DockStyle.Top;
      this.panSpbRefDet.Location = new System.Drawing.Point(3, 16);
      this.panSpbRefDet.Name = "panSpbRefDet";
      this.panSpbRefDet.Size = new System.Drawing.Size(700, 37);
      this.panSpbRefDet.TabIndex = 0;
      // 
      // lblUserDocsInfo
      // 
      this.lblUserDocsInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.lblUserDocsInfo.Icon = System.Windows.Forms.MessageBoxIcon.Asterisk;
      this.lblUserDocsInfo.Location = new System.Drawing.Point(0, 383);
      this.lblUserDocsInfo.Name = "lblUserDocsInfo";
      this.lblUserDocsInfo.Size = new System.Drawing.Size(706, 24);
      this.lblUserDocsInfo.TabIndex = 3;
      this.lblUserDocsInfo.Text = "Служебные ссылки для полей \"Документ создан пользователем\" и \"Документ изменен по" +
    "льзователем\" не показываются";
      this.lblUserDocsInfo.Visible = false;
      // 
      // lblNoRefs
      // 
      this.lblNoRefs.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.lblNoRefs.Icon = System.Windows.Forms.MessageBoxIcon.Asterisk;
      this.lblNoRefs.Location = new System.Drawing.Point(0, 407);
      this.lblNoRefs.Name = "lblNoRefs";
      this.lblNoRefs.Size = new System.Drawing.Size(706, 24);
      this.lblNoRefs.TabIndex = 4;
      this.lblNoRefs.Text = "На этот вид документов не может быть ссылок";
      this.lblNoRefs.Visible = false;
      // 
      // btnDebug
      // 
      this.btnDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDebug.Location = new System.Drawing.Point(665, 81);
      this.btnDebug.Name = "btnDebug";
      this.btnDebug.Size = new System.Drawing.Size(32, 24);
      this.btnDebug.TabIndex = 30;
      this.btnDebug.UseVisualStyleBackColor = true;
      this.btnDebug.Visible = false;
      // 
      // DocInfoForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(720, 463);
      this.Controls.Add(this.TheTabControl);
      this.Name = "DocInfoForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
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
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.grpRefStat.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grRefStat)).EndInit();
      this.grpRefDet.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grRefDet)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.TabPage tpInfo;
    private System.Windows.Forms.TabPage tpRefs;
    public System.Windows.Forms.GroupBox grpHist;
    public System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox edId;
    public System.Windows.Forms.Label DocImageLabel;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.TextBox edVersion;
    public System.Windows.Forms.TextBox edDocText;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.TextBox edStatus;
    private System.Windows.Forms.Label label1;
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
