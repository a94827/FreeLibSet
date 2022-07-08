namespace FreeLibSet.Forms
{
  partial class HieViewLevelEditForm
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
      this.panel2 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.grpMain = new System.Windows.Forms.GroupBox();
      this.grpSample = new System.Windows.Forms.GroupBox();
      this.panel3 = new System.Windows.Forms.Panel();
      this.cbHideExtraSumRows = new System.Windows.Forms.CheckBox();
      this.panel2.SuspendLayout();
      this.MainPanel.SuspendLayout();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.panel3.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.btnCancel);
      this.panel2.Controls.Add(this.btnOk);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel2.Location = new System.Drawing.Point(0, 247);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(517, 39);
      this.panel2.TabIndex = 1;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(102, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.splitContainer1);
      this.MainPanel.Controls.Add(this.panel3);
      this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(517, 247);
      this.MainPanel.TabIndex = 3;
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.grpMain);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.grpSample);
      this.splitContainer1.Size = new System.Drawing.Size(517, 222);
      this.splitContainer1.SplitterDistance = 354;
      this.splitContainer1.TabIndex = 3;
      // 
      // grpMain
      // 
      this.grpMain.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpMain.Location = new System.Drawing.Point(0, 0);
      this.grpMain.Name = "grpMain";
      this.grpMain.Size = new System.Drawing.Size(354, 222);
      this.grpMain.TabIndex = 0;
      this.grpMain.TabStop = false;
      this.grpMain.Text = "Настройка наличия и порядка уровней";
      // 
      // grpSample
      // 
      this.grpSample.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpSample.Location = new System.Drawing.Point(0, 0);
      this.grpSample.Name = "grpSample";
      this.grpSample.Size = new System.Drawing.Size(159, 222);
      this.grpSample.TabIndex = 0;
      this.grpSample.TabStop = false;
      this.grpSample.Text = "Образец";
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.cbHideExtraSumRows);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel3.Location = new System.Drawing.Point(0, 222);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(517, 25);
      this.panel3.TabIndex = 2;
      // 
      // cbHideExtraSumRows
      // 
      this.cbHideExtraSumRows.AutoSize = true;
      this.cbHideExtraSumRows.Location = new System.Drawing.Point(4, 4);
      this.cbHideExtraSumRows.Margin = new System.Windows.Forms.Padding(0);
      this.cbHideExtraSumRows.Name = "cbHideExtraSumRows";
      this.cbHideExtraSumRows.Size = new System.Drawing.Size(316, 17);
      this.cbHideExtraSumRows.TabIndex = 0;
      this.cbHideExtraSumRows.Text = "Скрывать итоговые строки для уровней с одной строкой";
      this.cbHideExtraSumRows.UseVisualStyleBackColor = true;
      // 
      // HieViewLevelEditForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(517, 286);
      this.Controls.Add(this.MainPanel);
      this.Controls.Add(this.panel2);
      this.MinimizeBox = false;
      this.Name = "HieViewLevelEditForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Уровни иерархии";
      this.panel2.ResumeLayout(false);
      this.MainPanel.ResumeLayout(false);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.panel3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    public System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.Panel panel3;
    public System.Windows.Forms.CheckBox cbHideExtraSumRows;
    private System.Windows.Forms.SplitContainer splitContainer1;
    public System.Windows.Forms.GroupBox grpMain;
    public System.Windows.Forms.GroupBox grpSample;
  }
}