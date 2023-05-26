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
      this.btnExplorer = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.grpMain = new System.Windows.Forms.GroupBox();
      this.lblDescription = new System.Windows.Forms.Label();
      this.btnBrowse = new System.Windows.Forms.Button();
      this.MainCB = new System.Windows.Forms.ComboBox();
      this.TextLabel = new System.Windows.Forms.Label();
      this.panel2 = new System.Windows.Forms.Panel();
      this.PanelSubFolders = new System.Windows.Forms.Panel();
      this.cbSubFolders = new System.Windows.Forms.CheckBox();
      this.MainPanel.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.grpMain.SuspendLayout();
      this.panel2.SuspendLayout();
      this.PanelSubFolders.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.grpMain);
      this.MainPanel.Controls.Add(this.groupBox1);
      this.MainPanel.Size = new System.Drawing.Size(437, 200);
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.btnExplorer);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.groupBox1.Location = new System.Drawing.Point(0, 128);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(437, 72);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Дополнительно";
      // 
      // btnExplorer
      // 
      this.btnExplorer.Location = new System.Drawing.Point(272, 30);
      this.btnExplorer.Name = "btnExplorer";
      this.btnExplorer.Size = new System.Drawing.Size(157, 24);
      this.btnExplorer.TabIndex = 1;
      this.btnExplorer.Text = "Проводник Windows";
      this.btnExplorer.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(9, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(257, 53);
      this.label1.TabIndex = 0;
      this.label1.Text = "Используйте кнопку \"Проводник Windows\", если требуется выполнить какие-либо предв" +
          "арительные действия с папками или файлами";
      // 
      // grpMain
      // 
      this.grpMain.Controls.Add(this.lblDescription);
      this.grpMain.Controls.Add(this.PanelSubFolders);
      this.grpMain.Controls.Add(this.panel2);
      this.grpMain.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpMain.Location = new System.Drawing.Point(0, 0);
      this.grpMain.Name = "grpMain";
      this.grpMain.Size = new System.Drawing.Size(437, 128);
      this.grpMain.TabIndex = 0;
      this.grpMain.TabStop = false;
      this.grpMain.Text = "???";
      // 
      // lblDescription
      // 
      this.lblDescription.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lblDescription.Location = new System.Drawing.Point(3, 58);
      this.lblDescription.Name = "lblDescription";
      this.lblDescription.Size = new System.Drawing.Size(431, 43);
      this.lblDescription.TabIndex = 1;
      this.lblDescription.Text = "???";
      this.lblDescription.UseMnemonic = false;
      // 
      // btnBrowse
      // 
      this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnBrowse.Location = new System.Drawing.Point(341, 16);
      this.btnBrowse.Name = "btnBrowse";
      this.btnBrowse.Size = new System.Drawing.Size(88, 24);
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
      this.MainCB.Size = new System.Drawing.Size(329, 21);
      this.MainCB.TabIndex = 1;
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
      // panel2
      // 
      this.panel2.Controls.Add(this.TextLabel);
      this.panel2.Controls.Add(this.btnBrowse);
      this.panel2.Controls.Add(this.MainCB);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel2.Location = new System.Drawing.Point(3, 16);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(431, 42);
      this.panel2.TabIndex = 0;
      // 
      // PanelSubFolders
      // 
      this.PanelSubFolders.Controls.Add(this.cbSubFolders);
      this.PanelSubFolders.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.PanelSubFolders.Location = new System.Drawing.Point(3, 101);
      this.PanelSubFolders.Name = "PanelSubFolders";
      this.PanelSubFolders.Size = new System.Drawing.Size(431, 24);
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
      // FileBrowseForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(437, 240);
      this.Name = "FileBrowseForm";
      this.MainPanel.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.grpMain.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.PanelSubFolders.ResumeLayout(false);
      this.PanelSubFolders.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    public System.Windows.Forms.GroupBox grpMain;
    public System.Windows.Forms.Label lblDescription;
    public System.Windows.Forms.Button btnBrowse;
    public System.Windows.Forms.ComboBox MainCB;
    public System.Windows.Forms.Label TextLabel;
    private System.Windows.Forms.GroupBox groupBox1;
    public System.Windows.Forms.Button btnExplorer;
    private System.Windows.Forms.Label label1;
    public System.Windows.Forms.Panel PanelSubFolders;
    private System.Windows.Forms.Panel panel2;
    public System.Windows.Forms.CheckBox cbSubFolders;

  }
}
