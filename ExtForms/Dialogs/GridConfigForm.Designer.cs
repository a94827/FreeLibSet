namespace FreeLibSet.Forms
{
  partial class GridConfigForm
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.grpSets = new System.Windows.Forms.GroupBox();
      this.panDefault = new System.Windows.Forms.Panel();
      this.btnDefault = new System.Windows.Forms.Button();
      this.panel3 = new System.Windows.Forms.Panel();
      this.btnCopy = new System.Windows.Forms.Button();
      this.btnPaste = new System.Windows.Forms.Button();
      this.panel2 = new System.Windows.Forms.Panel();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.SetComboBox = new FreeLibSet.Controls.ParamSetComboBox();
      this.panel1.SuspendLayout();
      this.grpSets.SuspendLayout();
      this.panDefault.SuspendLayout();
      this.panel3.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.grpSets);
      this.panel1.Controls.Add(this.panDefault);
      this.panel1.Controls.Add(this.panel3);
      this.panel1.Controls.Add(this.panel2);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 400);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(632, 52);
      this.panel1.TabIndex = 1;
      // 
      // grpSets
      // 
      this.grpSets.Controls.Add(this.SetComboBox);
      this.grpSets.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpSets.Location = new System.Drawing.Point(340, 0);
      this.grpSets.Name = "grpSets";
      this.grpSets.Size = new System.Drawing.Size(216, 52);
      this.grpSets.TabIndex = 8;
      this.grpSets.TabStop = false;
      this.grpSets.Text = "Готовые наборы";
      // 
      // panDefault
      // 
      this.panDefault.Controls.Add(this.btnDefault);
      this.panDefault.Dock = System.Windows.Forms.DockStyle.Left;
      this.panDefault.Location = new System.Drawing.Point(197, 0);
      this.panDefault.Name = "panDefault";
      this.panDefault.Size = new System.Drawing.Size(143, 52);
      this.panDefault.TabIndex = 7;
      this.panDefault.Visible = false;
      // 
      // btnDefault
      // 
      this.btnDefault.Location = new System.Drawing.Point(6, 17);
      this.btnDefault.Name = "btnDefault";
      this.btnDefault.Size = new System.Drawing.Size(132, 24);
      this.btnDefault.TabIndex = 3;
      this.btnDefault.Text = "По умолчанию";
      this.btnDefault.UseVisualStyleBackColor = true;
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.btnCopy);
      this.panel3.Controls.Add(this.btnPaste);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
      this.panel3.Location = new System.Drawing.Point(556, 0);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(76, 52);
      this.panel3.TabIndex = 6;
      // 
      // btnCopy
      // 
      this.btnCopy.Location = new System.Drawing.Point(3, 17);
      this.btnCopy.Name = "btnCopy";
      this.btnCopy.Size = new System.Drawing.Size(32, 24);
      this.btnCopy.TabIndex = 3;
      this.btnCopy.UseVisualStyleBackColor = true;
      // 
      // btnPaste
      // 
      this.btnPaste.Location = new System.Drawing.Point(41, 17);
      this.btnPaste.Name = "btnPaste";
      this.btnPaste.Size = new System.Drawing.Size(32, 24);
      this.btnPaste.TabIndex = 4;
      this.btnPaste.UseVisualStyleBackColor = true;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.btnOk);
      this.panel2.Controls.Add(this.btnCancel);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(197, 52);
      this.panel2.TabIndex = 5;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 17);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(104, 17);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // MainPanel
      // 
      this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(632, 400);
      this.MainPanel.TabIndex = 0;
      // 
      // SetComboBox
      // 
      this.SetComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.SetComboBox.Location = new System.Drawing.Point(6, 17);
      this.SetComboBox.MinimumSize = new System.Drawing.Size(150, 24);
      this.SetComboBox.Name = "SetComboBox";
      this.SetComboBox.SelectedCode = "";
      this.SetComboBox.SelectedItem = null;
      this.SetComboBox.SelectedMD5Sum = "";
      this.SetComboBox.Size = new System.Drawing.Size(204, 24);
      this.SetComboBox.TabIndex = 0;
      // 
      // GridConfigForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(632, 452);
      this.Controls.Add(this.MainPanel);
      this.Controls.Add(this.panel1);
      this.Name = "GridConfigForm";
      this.Text = "Настройка табличного просмотра";
      this.panel1.ResumeLayout(false);
      this.grpSets.ResumeLayout(false);
      this.panDefault.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    public System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.Button btnCopy;
    private System.Windows.Forms.Button btnPaste;
    private System.Windows.Forms.GroupBox grpSets;
    public FreeLibSet.Controls.ParamSetComboBox SetComboBox;
    private System.Windows.Forms.Panel panDefault;
    private System.Windows.Forms.Button btnDefault;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Panel panel2;
  }
}
