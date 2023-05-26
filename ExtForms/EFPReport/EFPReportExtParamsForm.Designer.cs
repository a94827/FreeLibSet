namespace FreeLibSet.Forms
{
  partial class EFPReportExtParamsForm
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
      this.BottomPanel = new System.Windows.Forms.Panel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.FSetComboBox = new FreeLibSet.Controls.ParamSetComboBox();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.BottomPanel.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // BottomPanel
      // 
      this.BottomPanel.Controls.Add(this.groupBox1);
      this.BottomPanel.Controls.Add(this.btnCancel);
      this.BottomPanel.Controls.Add(this.btnOk);
      this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.BottomPanel.Location = new System.Drawing.Point(0, 222);
      this.BottomPanel.Name = "BottomPanel";
      this.BottomPanel.Size = new System.Drawing.Size(592, 48);
      this.BottomPanel.TabIndex = 1;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.FSetComboBox);
      this.groupBox1.Location = new System.Drawing.Point(196, 2);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(393, 43);
      this.groupBox1.TabIndex = 2;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Готовые наборы";
      // 
      // FSetComboBox
      // 
      this.FSetComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.FSetComboBox.Location = new System.Drawing.Point(6, 17);
      this.FSetComboBox.MinimumSize = new System.Drawing.Size(200, 24);
      this.FSetComboBox.Name = "FSetComboBox";
      this.FSetComboBox.SelectedCode = "";
      this.FSetComboBox.SelectedItem = null;
      this.FSetComboBox.SelectedMD5Sum = "";
      this.FSetComboBox.Size = new System.Drawing.Size(381, 24);
      this.FSetComboBox.TabIndex = 2;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(102, 19);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 19);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // MainPanel
      // 
      this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(592, 222);
      this.MainPanel.TabIndex = 0;
      // 
      // EFPReportExtParamsForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(592, 270);
      this.Controls.Add(this.MainPanel);
      this.Controls.Add(this.BottomPanel);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "EFPReportExtParamsForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.BottomPanel.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel BottomPanel;
    private System.Windows.Forms.GroupBox groupBox1;

    /// <summary>
    /// Основная панель для размещения управляющих элементов диалога параметров
    /// </summary>
    public System.Windows.Forms.Panel MainPanel;
    private FreeLibSet.Controls.ParamSetComboBox FSetComboBox;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
  }
}
