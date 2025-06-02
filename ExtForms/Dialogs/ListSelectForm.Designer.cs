namespace FreeLibSet.Forms
{
  partial class ListSelectForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ListSelectForm));
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnCheckAll = new System.Windows.Forms.Button();
      this.btnUnCheckAll = new System.Windows.Forms.Button();
      this.btnCopy = new System.Windows.Forms.Button();
      this.btnPaste = new System.Windows.Forms.Button();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.TheGroupBox = new System.Windows.Forms.GroupBox();
      this.theGrid = new System.Windows.Forms.DataGridView();
      this.ButtonPanel = new System.Windows.Forms.Panel();
      this.OtherButtonPanel = new System.Windows.Forms.Panel();
      this.OkCancelButtonPanel = new System.Windows.Forms.Panel();
      this.MainPanel.SuspendLayout();
      this.TheGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.theGrid)).BeginInit();
      this.ButtonPanel.SuspendLayout();
      this.OtherButtonPanel.SuspendLayout();
      this.OkCancelButtonPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.Name = "btnOk";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnCheckAll
      // 
      resources.ApplyResources(this.btnCheckAll, "btnCheckAll");
      this.btnCheckAll.Name = "btnCheckAll";
      this.btnCheckAll.UseVisualStyleBackColor = true;
      // 
      // btnUnCheckAll
      // 
      resources.ApplyResources(this.btnUnCheckAll, "btnUnCheckAll");
      this.btnUnCheckAll.Name = "btnUnCheckAll";
      this.btnUnCheckAll.UseVisualStyleBackColor = true;
      // 
      // btnCopy
      // 
      resources.ApplyResources(this.btnCopy, "btnCopy");
      this.btnCopy.Name = "btnCopy";
      this.btnCopy.UseVisualStyleBackColor = true;
      // 
      // btnPaste
      // 
      resources.ApplyResources(this.btnPaste, "btnPaste");
      this.btnPaste.Name = "btnPaste";
      this.btnPaste.UseVisualStyleBackColor = true;
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.TheGroupBox);
      this.MainPanel.Controls.Add(this.ButtonPanel);
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Name = "MainPanel";
      // 
      // TheGroupBox
      // 
      this.TheGroupBox.Controls.Add(this.theGrid);
      resources.ApplyResources(this.TheGroupBox, "TheGroupBox");
      this.TheGroupBox.Name = "TheGroupBox";
      this.TheGroupBox.TabStop = false;
      // 
      // theGrid
      // 
      this.theGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      resources.ApplyResources(this.theGrid, "theGrid");
      this.theGrid.Name = "theGrid";
      // 
      // ButtonPanel
      // 
      this.ButtonPanel.Controls.Add(this.OtherButtonPanel);
      this.ButtonPanel.Controls.Add(this.OkCancelButtonPanel);
      resources.ApplyResources(this.ButtonPanel, "ButtonPanel");
      this.ButtonPanel.Name = "ButtonPanel";
      // 
      // OtherButtonPanel
      // 
      this.OtherButtonPanel.Controls.Add(this.btnCheckAll);
      this.OtherButtonPanel.Controls.Add(this.btnPaste);
      this.OtherButtonPanel.Controls.Add(this.btnUnCheckAll);
      this.OtherButtonPanel.Controls.Add(this.btnCopy);
      resources.ApplyResources(this.OtherButtonPanel, "OtherButtonPanel");
      this.OtherButtonPanel.Name = "OtherButtonPanel";
      // 
      // OkCancelButtonPanel
      // 
      this.OkCancelButtonPanel.Controls.Add(this.btnOk);
      this.OkCancelButtonPanel.Controls.Add(this.btnCancel);
      resources.ApplyResources(this.OkCancelButtonPanel, "OkCancelButtonPanel");
      this.OkCancelButtonPanel.Name = "OkCancelButtonPanel";
      // 
      // ListSelectForm
      // 
      this.AcceptButton = this.btnOk;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.MainPanel);
      this.MinimizeBox = false;
      this.Name = "ListSelectForm";
      this.MainPanel.ResumeLayout(false);
      this.TheGroupBox.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.theGrid)).EndInit();
      this.ButtonPanel.ResumeLayout(false);
      this.OtherButtonPanel.ResumeLayout(false);
      this.OkCancelButtonPanel.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion
    public System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnCheckAll;
    private System.Windows.Forms.Button btnUnCheckAll;
    private System.Windows.Forms.Button btnCopy;
    private System.Windows.Forms.Button btnPaste;
    private System.Windows.Forms.Panel ButtonPanel;
    private System.Windows.Forms.Panel OtherButtonPanel;
    public System.Windows.Forms.Panel MainPanel;
    public System.Windows.Forms.Panel OkCancelButtonPanel;
    private System.Windows.Forms.GroupBox TheGroupBox;
    private System.Windows.Forms.DataGridView theGrid;
  }
}
