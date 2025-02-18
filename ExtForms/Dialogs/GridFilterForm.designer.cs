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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GridFilterForm));
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
      this.panel1.AccessibleDescription = null;
      this.panel1.AccessibleName = null;
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.BackgroundImage = null;
      this.panel1.Controls.Add(this.btnCopy);
      this.panel1.Controls.Add(this.btnPaste);
      this.panel1.Controls.Add(this.grpSets);
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Font = null;
      this.panel1.Name = "panel1";
      // 
      // btnCopy
      // 
      this.btnCopy.AccessibleDescription = null;
      this.btnCopy.AccessibleName = null;
      resources.ApplyResources(this.btnCopy, "btnCopy");
      this.btnCopy.BackgroundImage = null;
      this.btnCopy.Font = null;
      this.btnCopy.Name = "btnCopy";
      this.btnCopy.UseVisualStyleBackColor = true;
      // 
      // btnPaste
      // 
      this.btnPaste.AccessibleDescription = null;
      this.btnPaste.AccessibleName = null;
      resources.ApplyResources(this.btnPaste, "btnPaste");
      this.btnPaste.BackgroundImage = null;
      this.btnPaste.Font = null;
      this.btnPaste.Name = "btnPaste";
      this.btnPaste.UseVisualStyleBackColor = true;
      // 
      // grpSets
      // 
      this.grpSets.AccessibleDescription = null;
      this.grpSets.AccessibleName = null;
      resources.ApplyResources(this.grpSets, "grpSets");
      this.grpSets.BackgroundImage = null;
      this.grpSets.Controls.Add(this.SetComboBox);
      this.grpSets.Font = null;
      this.grpSets.Name = "grpSets";
      this.grpSets.TabStop = false;
      // 
      // SetComboBox
      // 
      this.SetComboBox.AccessibleDescription = null;
      this.SetComboBox.AccessibleName = null;
      resources.ApplyResources(this.SetComboBox, "SetComboBox");
      this.SetComboBox.BackgroundImage = null;
      this.SetComboBox.Font = null;
      this.SetComboBox.MinimumSize = new System.Drawing.Size(200, 24);
      this.SetComboBox.Name = "SetComboBox";
      this.SetComboBox.SelectedCode = "";
      this.SetComboBox.SelectedItem = null;
      this.SetComboBox.SelectedMD5Sum = "";
      this.SetComboBox.ShowMD5 = false;
      // 
      // btnCancel
      // 
      this.btnCancel.AccessibleDescription = null;
      this.btnCancel.AccessibleName = null;
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.BackgroundImage = null;
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Font = null;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.AccessibleDescription = null;
      this.btnOk.AccessibleName = null;
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.BackgroundImage = null;
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Font = null;
      this.btnOk.Name = "btnOk";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // panSpb
      // 
      this.panSpb.AccessibleDescription = null;
      this.panSpb.AccessibleName = null;
      resources.ApplyResources(this.panSpb, "panSpb");
      this.panSpb.BackgroundImage = null;
      this.panSpb.Font = null;
      this.panSpb.Name = "panSpb";
      // 
      // FilterGrid
      // 
      this.FilterGrid.AccessibleDescription = null;
      this.FilterGrid.AccessibleName = null;
      resources.ApplyResources(this.FilterGrid, "FilterGrid");
      this.FilterGrid.BackgroundImage = null;
      this.FilterGrid.Font = null;
      this.FilterGrid.Name = "FilterGrid";
      this.FilterGrid.StandardTab = true;
      // 
      // GridFilterForm
      // 
      this.AccessibleDescription = null;
      this.AccessibleName = null;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackgroundImage = null;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.FilterGrid);
      this.Controls.Add(this.panSpb);
      this.Controls.Add(this.panel1);
      this.Font = null;
      this.Icon = null;
      this.MinimizeBox = false;
      this.Name = "GridFilterForm";
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
