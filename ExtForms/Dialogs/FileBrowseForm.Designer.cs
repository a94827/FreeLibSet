namespace FreeLibSet.Forms
{
  partial class FileBrowseForm
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

    #region Windows TheForm Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.infoLabel1 = new FreeLibSet.Controls.InfoLabel();
      this.panel3 = new System.Windows.Forms.Panel();
      this.btnExplorer = new System.Windows.Forms.Button();
      this.grpMain = new System.Windows.Forms.GroupBox();
      this.lblDescription = new FreeLibSet.Controls.InfoLabel();
      this.PanelSubFolders = new System.Windows.Forms.Panel();
      this.cbSubFolders = new System.Windows.Forms.CheckBox();
      this.panel2 = new System.Windows.Forms.Panel();
      this.TextLabel = new System.Windows.Forms.Label();
      this.btnBrowse = new System.Windows.Forms.Button();
      this.MainCB = new System.Windows.Forms.ComboBox();
      this.MainPanel.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.panel3.SuspendLayout();
      this.grpMain.SuspendLayout();
      this.PanelSubFolders.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // ButtonsPanel
      // 
      this.ButtonsPanel.Location = new System.Drawing.Point(0, 200);
      this.ButtonsPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.ButtonsPanel.Size = new System.Drawing.Size(465, 40);
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.grpMain);
      this.MainPanel.Controls.Add(this.groupBox1);
      this.MainPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.MainPanel.Size = new System.Drawing.Size(465, 200);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.infoLabel1);
      this.groupBox1.Controls.Add(this.panel3);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.groupBox1.Location = new System.Drawing.Point(0, 128);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(465, 72);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Дополнительно";
      // 
      // infoLabel1
      // 
      this.infoLabel1.BackColor = System.Drawing.SystemColors.Control;
      this.infoLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.infoLabel1.ForeColor = System.Drawing.SystemColors.ControlText;
      this.infoLabel1.Location = new System.Drawing.Point(3, 16);
      this.infoLabel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.infoLabel1.Name = "infoLabel1";
      this.infoLabel1.Size = new System.Drawing.Size(299, 53);
      this.infoLabel1.TabIndex = 4;
      this.infoLabel1.Text = "Используйте кнопку \"Открыть папку\", если требуется выполнить какие-либо предварит" +
    "ельные действия с папками или файлами";
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.btnExplorer);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
      this.panel3.Location = new System.Drawing.Point(302, 16);
      this.panel3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(160, 53);
      this.panel3.TabIndex = 3;
      // 
      // btnExplorer
      // 
      this.btnExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnExplorer.Location = new System.Drawing.Point(5, 3);
      this.btnExplorer.Name = "btnExplorer";
      this.btnExplorer.Size = new System.Drawing.Size(148, 20);
      this.btnExplorer.TabIndex = 1;
      this.btnExplorer.Text = "Открыть папку";
      this.btnExplorer.UseVisualStyleBackColor = true;
      // 
      // grpMain
      // 
      this.grpMain.Controls.Add(this.lblDescription);
      this.grpMain.Controls.Add(this.PanelSubFolders);
      this.grpMain.Controls.Add(this.panel2);
      this.grpMain.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpMain.Location = new System.Drawing.Point(0, 0);
      this.grpMain.Name = "grpMain";
      this.grpMain.Size = new System.Drawing.Size(465, 128);
      this.grpMain.TabIndex = 0;
      this.grpMain.TabStop = false;
      this.grpMain.Text = "???";
      // 
      // lblDescription
      // 
      this.lblDescription.BackColor = System.Drawing.SystemColors.Control;
      this.lblDescription.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lblDescription.ForeColor = System.Drawing.SystemColors.ControlText;
      this.lblDescription.Location = new System.Drawing.Point(3, 58);
      this.lblDescription.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.lblDescription.Name = "lblDescription";
      this.lblDescription.Size = new System.Drawing.Size(459, 43);
      this.lblDescription.TabIndex = 3;
      this.lblDescription.Text = "????";
      // 
      // PanelSubFolders
      // 
      this.PanelSubFolders.Controls.Add(this.cbSubFolders);
      this.PanelSubFolders.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.PanelSubFolders.Location = new System.Drawing.Point(3, 101);
      this.PanelSubFolders.Name = "PanelSubFolders";
      this.PanelSubFolders.Size = new System.Drawing.Size(459, 24);
      this.PanelSubFolders.TabIndex = 2;
      this.PanelSubFolders.Visible = false;
      // 
      // cbSubFolders
      // 
      this.cbSubFolders.AutoSize = true;
      this.cbSubFolders.Location = new System.Drawing.Point(6, 4);
      this.cbSubFolders.Name = "cbSubFolders";
      this.cbSubFolders.Size = new System.Drawing.Size(137, 17);
      this.cbSubFolders.TabIndex = 0;
      this.cbSubFolders.Text = "Включая подкаталоги";
      this.cbSubFolders.UseVisualStyleBackColor = true;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.TextLabel);
      this.panel2.Controls.Add(this.btnBrowse);
      this.panel2.Controls.Add(this.MainCB);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel2.Location = new System.Drawing.Point(3, 16);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(459, 42);
      this.panel2.TabIndex = 0;
      // 
      // TextLabel
      // 
      this.TextLabel.AutoSize = true;
      this.TextLabel.Location = new System.Drawing.Point(6, 0);
      this.TextLabel.Name = "TextLabel";
      this.TextLabel.Size = new System.Drawing.Size(25, 13);
      this.TextLabel.TabIndex = 0;
      this.TextLabel.Text = "???";
      // 
      // btnBrowse
      // 
      this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnBrowse.Location = new System.Drawing.Point(368, 18);
      this.btnBrowse.Name = "btnBrowse";
      this.btnBrowse.Size = new System.Drawing.Size(85, 20);
      this.btnBrowse.TabIndex = 2;
      this.btnBrowse.Text = "Обзор...";
      this.btnBrowse.UseVisualStyleBackColor = true;
      // 
      // MainCB
      // 
      this.MainCB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.MainCB.FormattingEnabled = true;
      this.MainCB.Location = new System.Drawing.Point(6, 19);
      this.MainCB.Name = "MainCB";
      this.MainCB.Size = new System.Drawing.Size(357, 21);
      this.MainCB.TabIndex = 1;
      // 
      // FileBrowseForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(465, 240);
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.MinimumSize = new System.Drawing.Size(485, 106);
      this.Name = "FileBrowseForm";
      this.MainPanel.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.grpMain.ResumeLayout(false);
      this.PanelSubFolders.ResumeLayout(false);
      this.PanelSubFolders.PerformLayout();
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    public System.Windows.Forms.GroupBox grpMain;
    public System.Windows.Forms.Button btnBrowse;
    public System.Windows.Forms.ComboBox MainCB;
    public System.Windows.Forms.Label TextLabel;
    private System.Windows.Forms.GroupBox groupBox1;
    public System.Windows.Forms.Button btnExplorer;
    public System.Windows.Forms.Panel PanelSubFolders;
    private System.Windows.Forms.Panel panel2;
    public System.Windows.Forms.CheckBox cbSubFolders;
    private Controls.InfoLabel infoLabel1;
    private System.Windows.Forms.Panel panel3;
    public Controls.InfoLabel lblDescription;
  }
}
