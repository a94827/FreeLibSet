namespace FreeLibSet.Forms.Reporting
{
  partial class BRDataViewPageSetupSendTo
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
      this.MainPanel = new System.Windows.Forms.Panel();
      this.lblInfo = new FreeLibSet.Controls.InfoLabel();
      this.grpArea = new System.Windows.Forms.GroupBox();
      this.cbExpHeaders = new System.Windows.Forms.CheckBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.rbAll = new System.Windows.Forms.RadioButton();
      this.rbSelected = new System.Windows.Forms.RadioButton();
      this.MainPanel.SuspendLayout();
      this.grpArea.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.lblInfo);
      this.MainPanel.Controls.Add(this.grpArea);
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(434, 226);
      this.MainPanel.TabIndex = 0;
      // 
      // lblInfo
      // 
      this.lblInfo.AutoSize = true;
      this.lblInfo.Dock = System.Windows.Forms.DockStyle.Top;
      this.lblInfo.Location = new System.Drawing.Point(0, 95);
      this.lblInfo.Name = "lblInfo";
      this.lblInfo.Size = new System.Drawing.Size(434, 13);
      this.lblInfo.TabIndex = 1;
      this.lblInfo.Text = "???";
      // 
      // grpArea
      // 
      this.grpArea.Controls.Add(this.cbExpHeaders);
      this.grpArea.Controls.Add(this.panel1);
      this.grpArea.Dock = System.Windows.Forms.DockStyle.Top;
      this.grpArea.Location = new System.Drawing.Point(0, 0);
      this.grpArea.Name = "grpArea";
      this.grpArea.Size = new System.Drawing.Size(434, 95);
      this.grpArea.TabIndex = 0;
      this.grpArea.TabStop = false;
      this.grpArea.Text = "Область";
      // 
      // cbExpHeaders
      // 
      this.cbExpHeaders.AutoSize = true;
      this.cbExpHeaders.Location = new System.Drawing.Point(17, 70);
      this.cbExpHeaders.Name = "cbExpHeaders";
      this.cbExpHeaders.Size = new System.Drawing.Size(130, 17);
      this.cbExpHeaders.TabIndex = 1;
      this.cbExpHeaders.Text = "Заголовки столбцов";
      this.cbExpHeaders.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.panel1.Controls.Add(this.rbAll);
      this.panel1.Controls.Add(this.rbSelected);
      this.panel1.Location = new System.Drawing.Point(14, 19);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(414, 43);
      this.panel1.TabIndex = 0;
      // 
      // rbAll
      // 
      this.rbAll.AutoSize = true;
      this.rbAll.Location = new System.Drawing.Point(3, 0);
      this.rbAll.Name = "rbAll";
      this.rbAll.Size = new System.Drawing.Size(88, 17);
      this.rbAll.TabIndex = 0;
      this.rbAll.TabStop = true;
      this.rbAll.Text = "Вся таблица";
      this.rbAll.UseVisualStyleBackColor = true;
      // 
      // rbSelected
      // 
      this.rbSelected.AutoSize = true;
      this.rbSelected.Location = new System.Drawing.Point(3, 23);
      this.rbSelected.Name = "rbSelected";
      this.rbSelected.Size = new System.Drawing.Size(122, 17);
      this.rbSelected.TabIndex = 1;
      this.rbSelected.TabStop = true;
      this.rbSelected.Text = "Выбранные ячейки";
      this.rbSelected.UseVisualStyleBackColor = true;
      // 
      // BRDataViewPageSetupExport
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(446, 238);
      this.Controls.Add(this.MainPanel);
      this.Name = "BRDataViewPageSetupExport";
      this.Text = "BRDataViewPageSetupExport";
      this.MainPanel.ResumeLayout(false);
      this.MainPanel.PerformLayout();
      this.grpArea.ResumeLayout(false);
      this.grpArea.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.GroupBox grpArea;
    private System.Windows.Forms.CheckBox cbExpHeaders;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.RadioButton rbAll;
    private System.Windows.Forms.RadioButton rbSelected;
    private Controls.InfoLabel lblInfo;
  }
}
