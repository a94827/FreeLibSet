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
      this.TheGroupBox = new System.Windows.Forms.GroupBox();
      this.TheLV = new System.Windows.Forms.ListView();
      this.TheColumn = new System.Windows.Forms.ColumnHeader();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnCheckAll = new System.Windows.Forms.Button();
      this.btnUnCheckAll = new System.Windows.Forms.Button();
      this.btnCopy = new System.Windows.Forms.Button();
      this.btnPaste = new System.Windows.Forms.Button();
      this.btnMore = new System.Windows.Forms.Button();
      this.TheGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // TheGroupBox
      // 
      this.TheGroupBox.AccessibleDescription = null;
      this.TheGroupBox.AccessibleName = null;
      resources.ApplyResources(this.TheGroupBox, "TheGroupBox");
      this.TheGroupBox.BackgroundImage = null;
      this.TheGroupBox.Controls.Add(this.TheLV);
      this.TheGroupBox.Font = null;
      this.TheGroupBox.Name = "TheGroupBox";
      this.TheGroupBox.TabStop = false;
      // 
      // TheLV
      // 
      this.TheLV.AccessibleDescription = null;
      this.TheLV.AccessibleName = null;
      resources.ApplyResources(this.TheLV, "TheLV");
      this.TheLV.BackgroundImage = null;
      this.TheLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TheColumn});
      this.TheLV.Font = null;
      this.TheLV.FullRowSelect = true;
      this.TheLV.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.TheLV.HideSelection = false;
      this.TheLV.MultiSelect = false;
      this.TheLV.Name = "TheLV";
      this.TheLV.ShowGroups = false;
      this.TheLV.UseCompatibleStateImageBehavior = false;
      this.TheLV.View = System.Windows.Forms.View.Details;
      this.TheLV.DoubleClick += new System.EventHandler(this.TheLV_DoubleClick);
      // 
      // TheColumn
      // 
      resources.ApplyResources(this.TheColumn, "TheColumn");
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
      // btnCheckAll
      // 
      this.btnCheckAll.AccessibleDescription = null;
      this.btnCheckAll.AccessibleName = null;
      resources.ApplyResources(this.btnCheckAll, "btnCheckAll");
      this.btnCheckAll.BackgroundImage = null;
      this.btnCheckAll.Font = null;
      this.btnCheckAll.Name = "btnCheckAll";
      this.btnCheckAll.UseVisualStyleBackColor = true;
      // 
      // btnUnCheckAll
      // 
      this.btnUnCheckAll.AccessibleDescription = null;
      this.btnUnCheckAll.AccessibleName = null;
      resources.ApplyResources(this.btnUnCheckAll, "btnUnCheckAll");
      this.btnUnCheckAll.BackgroundImage = null;
      this.btnUnCheckAll.Font = null;
      this.btnUnCheckAll.Name = "btnUnCheckAll";
      this.btnUnCheckAll.UseVisualStyleBackColor = true;
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
      // btnMore
      // 
      this.btnMore.AccessibleDescription = null;
      this.btnMore.AccessibleName = null;
      resources.ApplyResources(this.btnMore, "btnMore");
      this.btnMore.BackgroundImage = null;
      this.btnMore.Font = null;
      this.btnMore.Name = "btnMore";
      this.btnMore.UseVisualStyleBackColor = true;
      // 
      // ListSelectForm
      // 
      this.AcceptButton = this.btnOk;
      this.AccessibleDescription = null;
      this.AccessibleName = null;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackgroundImage = null;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.btnMore);
      this.Controls.Add(this.btnPaste);
      this.Controls.Add(this.btnCopy);
      this.Controls.Add(this.btnUnCheckAll);
      this.Controls.Add(this.btnCheckAll);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.TheGroupBox);
      this.Font = null;
      this.Icon = null;
      this.MinimizeBox = false;
      this.Name = "ListSelectForm";
      this.Resize += new System.EventHandler(this.TheLV_Resize);
      this.TheGroupBox.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ColumnHeader TheColumn;
    public System.Windows.Forms.GroupBox TheGroupBox;
    public System.Windows.Forms.ListView TheLV;
    public System.Windows.Forms.Button btnOk;
    public System.Windows.Forms.Button btnCancel;
    public System.Windows.Forms.Button btnCheckAll;
    public System.Windows.Forms.Button btnUnCheckAll;
    private System.Windows.Forms.Button btnCopy;
    private System.Windows.Forms.Button btnPaste;
    private System.Windows.Forms.Button btnMore;
  }
}
