namespace FreeLibSet.Forms.Docs
{
  partial class RefDocGridFilterForm
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
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnNo = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.grpDocs = new System.Windows.Forms.GroupBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.cbMode = new System.Windows.Forms.ComboBox();
      this.FilterGrid = new System.Windows.Forms.DataGridView();
      this.panel3 = new System.Windows.Forms.Panel();
      this.grDocSel = new System.Windows.Forms.DataGridView();
      this.panSpeedButtons = new System.Windows.Forms.Panel();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.grpDocs.SuspendLayout();
      this.groupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.FilterGrid)).BeginInit();
      this.panel3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grDocSel)).BeginInit();
      this.SuspendLayout();
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
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(8, 38);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnNo
      // 
      this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
      this.btnNo.Location = new System.Drawing.Point(8, 68);
      this.btnNo.Name = "btnNo";
      this.btnNo.Size = new System.Drawing.Size(88, 24);
      this.btnNo.TabIndex = 2;
      this.btnNo.Text = "&Нет";
      this.btnNo.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Controls.Add(this.btnNo);
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
      this.panel1.Location = new System.Drawing.Point(528, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(104, 452);
      this.panel1.TabIndex = 1;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.grpDocs);
      this.panel2.Controls.Add(this.groupBox2);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(528, 452);
      this.panel2.TabIndex = 0;
      // 
      // grpDocs
      // 
      this.grpDocs.Controls.Add(this.panel3);
      this.grpDocs.Controls.Add(this.FilterGrid);
      this.grpDocs.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpDocs.Location = new System.Drawing.Point(0, 44);
      this.grpDocs.Name = "grpDocs";
      this.grpDocs.Size = new System.Drawing.Size(528, 408);
      this.grpDocs.TabIndex = 1;
      this.grpDocs.TabStop = false;
      this.grpDocs.Text = "Документы";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.cbMode);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox2.Location = new System.Drawing.Point(0, 0);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(528, 44);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Режим фильтра";
      // 
      // cbMode
      // 
      this.cbMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbMode.FormattingEnabled = true;
      this.cbMode.Location = new System.Drawing.Point(6, 17);
      this.cbMode.Name = "cbMode";
      this.cbMode.Size = new System.Drawing.Size(516, 21);
      this.cbMode.TabIndex = 0;
      // 
      // FilterGrid
      // 
      this.FilterGrid.Dock = System.Windows.Forms.DockStyle.Top;
      this.FilterGrid.Enabled = false;
      this.FilterGrid.Location = new System.Drawing.Point(3, 16);
      this.FilterGrid.Name = "FilterGrid";
      this.FilterGrid.Size = new System.Drawing.Size(522, 33);
      this.FilterGrid.TabIndex = 0;
      this.FilterGrid.Visible = false;
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.grDocSel);
      this.panel3.Controls.Add(this.panSpeedButtons);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel3.Location = new System.Drawing.Point(3, 49);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(522, 356);
      this.panel3.TabIndex = 1;
      // 
      // grDocSel
      // 
      this.grDocSel.AllowUserToAddRows = false;
      this.grDocSel.AllowUserToDeleteRows = false;
      this.grDocSel.AllowUserToResizeColumns = false;
      this.grDocSel.AllowUserToResizeRows = false;
      this.grDocSel.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grDocSel.ColumnHeadersVisible = false;
      this.grDocSel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grDocSel.Location = new System.Drawing.Point(0, 33);
      this.grDocSel.Name = "grDocSel";
      this.grDocSel.ReadOnly = true;
      this.grDocSel.Size = new System.Drawing.Size(522, 323);
      this.grDocSel.TabIndex = 3;
      // 
      // panSpeedButtons
      // 
      this.panSpeedButtons.Dock = System.Windows.Forms.DockStyle.Top;
      this.panSpeedButtons.Location = new System.Drawing.Point(0, 0);
      this.panSpeedButtons.Name = "panSpeedButtons";
      this.panSpeedButtons.Size = new System.Drawing.Size(522, 33);
      this.panSpeedButtons.TabIndex = 2;
      // 
      // RefDocGridFilterForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(632, 452);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.MinimizeBox = false;
      this.Name = "RefDocGridFilterForm";
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.grpDocs.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.FilterGrid)).EndInit();
      this.panel3.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grDocSel)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnNo;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.GroupBox grpDocs;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.ComboBox cbMode;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.DataGridView grDocSel;
    private System.Windows.Forms.Panel panSpeedButtons;
    internal System.Windows.Forms.DataGridView FilterGrid;
  }
}