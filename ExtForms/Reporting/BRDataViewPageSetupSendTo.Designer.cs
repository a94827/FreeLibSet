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
      this.panel2 = new System.Windows.Forms.Panel();
      this.rbSelected = new System.Windows.Forms.RadioButton();
      this.rbAll = new System.Windows.Forms.RadioButton();
      this.cbExpTableFilters = new System.Windows.Forms.CheckBox();
      this.cbExpTableHeader = new System.Windows.Forms.CheckBox();
      this.cbExpColumnHeaders = new System.Windows.Forms.CheckBox();
      this.MainPanel.SuspendLayout();
      this.grpArea.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.lblInfo);
      this.MainPanel.Controls.Add(this.grpArea);
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Margin = new System.Windows.Forms.Padding(4);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(579, 192);
      this.MainPanel.TabIndex = 0;
      // 
      // lblInfo
      // 
      this.lblInfo.AutoSize = true;
      this.lblInfo.Dock = System.Windows.Forms.DockStyle.Top;
      this.lblInfo.Location = new System.Drawing.Point(0, 163);
      this.lblInfo.Margin = new System.Windows.Forms.Padding(4);
      this.lblInfo.Name = "lblInfo";
      this.lblInfo.Size = new System.Drawing.Size(579, 17);
      this.lblInfo.TabIndex = 1;
      this.lblInfo.Text = "???";
      // 
      // grpArea
      // 
      this.grpArea.Controls.Add(this.panel2);
      this.grpArea.Controls.Add(this.cbExpTableFilters);
      this.grpArea.Controls.Add(this.cbExpTableHeader);
      this.grpArea.Controls.Add(this.cbExpColumnHeaders);
      this.grpArea.Dock = System.Windows.Forms.DockStyle.Top;
      this.grpArea.Location = new System.Drawing.Point(0, 0);
      this.grpArea.Margin = new System.Windows.Forms.Padding(4);
      this.grpArea.Name = "grpArea";
      this.grpArea.Padding = new System.Windows.Forms.Padding(4);
      this.grpArea.Size = new System.Drawing.Size(579, 163);
      this.grpArea.TabIndex = 0;
      this.grpArea.TabStop = false;
      this.grpArea.Text = "Что выводить";
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.rbSelected);
      this.panel2.Controls.Add(this.rbAll);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel2.Location = new System.Drawing.Point(4, 19);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(571, 50);
      this.panel2.TabIndex = 0;
      // 
      // rbSelected
      // 
      this.rbSelected.AutoSize = true;
      this.rbSelected.Location = new System.Drawing.Point(4, 25);
      this.rbSelected.Margin = new System.Windows.Forms.Padding(4);
      this.rbSelected.Name = "rbSelected";
      this.rbSelected.Size = new System.Drawing.Size(157, 21);
      this.rbSelected.TabIndex = 1;
      this.rbSelected.TabStop = true;
      this.rbSelected.Text = "Выбранные ячейки";
      this.rbSelected.UseVisualStyleBackColor = true;
      // 
      // rbAll
      // 
      this.rbAll.AutoSize = true;
      this.rbAll.Location = new System.Drawing.Point(4, 0);
      this.rbAll.Margin = new System.Windows.Forms.Padding(4);
      this.rbAll.Name = "rbAll";
      this.rbAll.Size = new System.Drawing.Size(112, 21);
      this.rbAll.TabIndex = 0;
      this.rbAll.TabStop = true;
      this.rbAll.Text = "Вся таблица";
      this.rbAll.UseVisualStyleBackColor = true;
      // 
      // cbExpTableFilters
      // 
      this.cbExpTableFilters.AutoSize = true;
      this.cbExpTableFilters.Location = new System.Drawing.Point(8, 105);
      this.cbExpTableFilters.Margin = new System.Windows.Forms.Padding(4);
      this.cbExpTableFilters.Name = "cbExpTableFilters";
      this.cbExpTableFilters.Size = new System.Drawing.Size(91, 21);
      this.cbExpTableFilters.TabIndex = 2;
      this.cbExpTableFilters.Text = "Фильтры";
      this.cbExpTableFilters.UseVisualStyleBackColor = true;
      // 
      // cbExpTableHeader
      // 
      this.cbExpTableHeader.AutoSize = true;
      this.cbExpTableHeader.Location = new System.Drawing.Point(8, 76);
      this.cbExpTableHeader.Margin = new System.Windows.Forms.Padding(4);
      this.cbExpTableHeader.Name = "cbExpTableHeader";
      this.cbExpTableHeader.Size = new System.Drawing.Size(151, 21);
      this.cbExpTableHeader.TabIndex = 1;
      this.cbExpTableHeader.Text = "Заголовок таблицы";
      this.cbExpTableHeader.UseVisualStyleBackColor = true;
      // 
      // cbExpColumnHeaders
      // 
      this.cbExpColumnHeaders.AutoSize = true;
      this.cbExpColumnHeaders.Location = new System.Drawing.Point(8, 134);
      this.cbExpColumnHeaders.Margin = new System.Windows.Forms.Padding(4);
      this.cbExpColumnHeaders.Name = "cbExpColumnHeaders";
      this.cbExpColumnHeaders.Size = new System.Drawing.Size(163, 21);
      this.cbExpColumnHeaders.TabIndex = 3;
      this.cbExpColumnHeaders.Text = "Заголовки столбцов";
      this.cbExpColumnHeaders.UseVisualStyleBackColor = true;
      // 
      // BRDataViewPageSetupSendTo
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(595, 199);
      this.Controls.Add(this.MainPanel);
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "BRDataViewPageSetupSendTo";
      this.Text = "BRDataViewPageSetupExport";
      this.MainPanel.ResumeLayout(false);
      this.MainPanel.PerformLayout();
      this.grpArea.ResumeLayout(false);
      this.grpArea.PerformLayout();
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.GroupBox grpArea;
    private Controls.InfoLabel lblInfo;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.RadioButton rbSelected;
    private System.Windows.Forms.RadioButton rbAll;
    private System.Windows.Forms.CheckBox cbExpTableFilters;
    private System.Windows.Forms.CheckBox cbExpTableHeader;
    private System.Windows.Forms.CheckBox cbExpColumnHeaders;
  }
}
