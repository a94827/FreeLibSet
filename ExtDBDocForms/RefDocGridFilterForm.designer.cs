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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RefDocGridFilterForm));
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnNo = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.grpDocs = new System.Windows.Forms.GroupBox();
      this.panel3 = new System.Windows.Forms.Panel();
      this.grDocSel = new System.Windows.Forms.DataGridView();
      this.panSpeedButtons = new System.Windows.Forms.Panel();
      this.FilterGrid = new System.Windows.Forms.DataGridView();
      this.grpMode = new System.Windows.Forms.GroupBox();
      this.cbMode = new System.Windows.Forms.ComboBox();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.grpDocs.SuspendLayout();
      this.panel3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grDocSel)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.FilterGrid)).BeginInit();
      this.grpMode.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnOk
      // 
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Name = "btnOk";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnNo
      // 
      resources.ApplyResources(this.btnNo, "btnNo");
      this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
      this.btnNo.Name = "btnNo";
      this.btnNo.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Controls.Add(this.btnNo);
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Name = "panel1";
      // 
      // panel2
      // 
      resources.ApplyResources(this.panel2, "panel2");
      this.panel2.Controls.Add(this.grpDocs);
      this.panel2.Controls.Add(this.grpMode);
      this.panel2.Name = "panel2";
      // 
      // grpDocs
      // 
      resources.ApplyResources(this.grpDocs, "grpDocs");
      this.grpDocs.Controls.Add(this.panel3);
      this.grpDocs.Controls.Add(this.FilterGrid);
      this.grpDocs.Name = "grpDocs";
      this.grpDocs.TabStop = false;
      // 
      // panel3
      // 
      resources.ApplyResources(this.panel3, "panel3");
      this.panel3.Controls.Add(this.grDocSel);
      this.panel3.Controls.Add(this.panSpeedButtons);
      this.panel3.Name = "panel3";
      // 
      // grDocSel
      // 
      resources.ApplyResources(this.grDocSel, "grDocSel");
      this.grDocSel.AllowUserToAddRows = false;
      this.grDocSel.AllowUserToDeleteRows = false;
      this.grDocSel.AllowUserToResizeColumns = false;
      this.grDocSel.AllowUserToResizeRows = false;
      this.grDocSel.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grDocSel.ColumnHeadersVisible = false;
      this.grDocSel.Name = "grDocSel";
      this.grDocSel.ReadOnly = true;
      // 
      // panSpeedButtons
      // 
      resources.ApplyResources(this.panSpeedButtons, "panSpeedButtons");
      this.panSpeedButtons.Name = "panSpeedButtons";
      // 
      // FilterGrid
      // 
      resources.ApplyResources(this.FilterGrid, "FilterGrid");
      this.FilterGrid.Name = "FilterGrid";
      // 
      // grpMode
      // 
      resources.ApplyResources(this.grpMode, "grpMode");
      this.grpMode.Controls.Add(this.cbMode);
      this.grpMode.Name = "grpMode";
      this.grpMode.TabStop = false;
      // 
      // cbMode
      // 
      resources.ApplyResources(this.cbMode, "cbMode");
      this.cbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbMode.FormattingEnabled = true;
      this.cbMode.Name = "cbMode";
      // 
      // RefDocGridFilterForm
      // 
      this.AcceptButton = this.btnOk;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.MinimizeBox = false;
      this.Name = "RefDocGridFilterForm";
      this.panel1.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.grpDocs.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grDocSel)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.FilterGrid)).EndInit();
      this.grpMode.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnNo;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.GroupBox grpDocs;
    private System.Windows.Forms.GroupBox grpMode;
    private System.Windows.Forms.ComboBox cbMode;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.DataGridView grDocSel;
    private System.Windows.Forms.Panel panSpeedButtons;
    internal System.Windows.Forms.DataGridView FilterGrid;
  }
}
