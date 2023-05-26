namespace FreeLibSet.Forms
{
  partial class GridFilterForm
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCopy = new System.Windows.Forms.Button();
      this.btnPaste = new System.Windows.Forms.Button();
      this.grpSets = new System.Windows.Forms.GroupBox();
      this.SetComboBox = new FreeLibSet.Controls.ParamSetComboBox();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.panSpb = new System.Windows.Forms.Panel();
      this.FilterGrid = new System.Windows.Forms.DataGridView();
      this.panel1.SuspendLayout();
      this.grpSets.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.FilterGrid)).BeginInit();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCopy);
      this.panel1.Controls.Add(this.btnPaste);
      this.panel1.Controls.Add(this.grpSets);
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 250);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(624, 52);
      this.panel1.TabIndex = 2;
      // 
      // btnCopy
      // 
      this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCopy.Location = new System.Drawing.Point(547, 20);
      this.btnCopy.Name = "btnCopy";
      this.btnCopy.Size = new System.Drawing.Size(32, 24);
      this.btnCopy.TabIndex = 3;
      this.btnCopy.UseVisualStyleBackColor = true;
      // 
      // btnPaste
      // 
      this.btnPaste.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPaste.Location = new System.Drawing.Point(584, 20);
      this.btnPaste.Name = "btnPaste";
      this.btnPaste.Size = new System.Drawing.Size(32, 24);
      this.btnPaste.TabIndex = 4;
      this.btnPaste.UseVisualStyleBackColor = true;
      // 
      // grpSets
      // 
      this.grpSets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpSets.Controls.Add(this.SetComboBox);
      this.grpSets.Location = new System.Drawing.Point(196, 3);
      this.grpSets.Name = "grpSets";
      this.grpSets.Size = new System.Drawing.Size(345, 43);
      this.grpSets.TabIndex = 2;
      this.grpSets.TabStop = false;
      this.grpSets.Text = "Готовые наборы";
      // 
      // SetComboBox
      // 
      this.SetComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.SetComboBox.Location = new System.Drawing.Point(6, 17);
      this.SetComboBox.MinimumSize = new System.Drawing.Size(200, 24);
      this.SetComboBox.Name = "SetComboBox";
      this.SetComboBox.SelectedCode = "";
      this.SetComboBox.SelectedItem = null;
      this.SetComboBox.SelectedMD5Sum = "";
      this.SetComboBox.Size = new System.Drawing.Size(333, 24);
      this.SetComboBox.TabIndex = 2;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(106, 20);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(12, 20);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // panSpb
      // 
      this.panSpb.Dock = System.Windows.Forms.DockStyle.Top;
      this.panSpb.Location = new System.Drawing.Point(0, 0);
      this.panSpb.Name = "panSpb";
      this.panSpb.Size = new System.Drawing.Size(624, 51);
      this.panSpb.TabIndex = 0;
      // 
      // FilterGrid
      // 
      this.FilterGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.FilterGrid.Location = new System.Drawing.Point(0, 51);
      this.FilterGrid.Name = "FilterGrid";
      this.FilterGrid.Size = new System.Drawing.Size(624, 199);
      this.FilterGrid.StandardTab = true;
      this.FilterGrid.TabIndex = 1;
      // 
      // GridFilterForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(624, 302);
      this.Controls.Add(this.FilterGrid);
      this.Controls.Add(this.panSpb);
      this.Controls.Add(this.panel1);
      this.MinimizeBox = false;
      this.Name = "GridFilterForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Установка фильтров";
      this.panel1.ResumeLayout(false);
      this.grpSets.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.FilterGrid)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Panel panSpb;
    private System.Windows.Forms.GroupBox grpSets;
    private FreeLibSet.Controls.ParamSetComboBox SetComboBox;
    private System.Windows.Forms.DataGridView FilterGrid;
    private System.Windows.Forms.Button btnCopy;
    private System.Windows.Forms.Button btnPaste;
  }
}
