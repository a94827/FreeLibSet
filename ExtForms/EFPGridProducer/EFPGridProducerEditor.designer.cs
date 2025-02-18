namespace FreeLibSet.Forms
{
  partial class EFPGridProducerEditor
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EFPGridProducerEditor));
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      this.tpToolTips = new System.Windows.Forms.TabPage();
      this.panGrToolTips = new System.Windows.Forms.Panel();
      this.grToolTips = new System.Windows.Forms.DataGridView();
      this.panSpbToolTips = new System.Windows.Forms.Panel();
      this.panel3 = new System.Windows.Forms.Panel();
      this.cbCurrentCellToolTip = new System.Windows.Forms.CheckBox();
      this.tpColumns = new System.Windows.Forms.TabPage();
      this.panGrColumns = new System.Windows.Forms.Panel();
      this.grColumns = new System.Windows.Forms.DataGridView();
      this.panSpbColumns = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.cbStartColumn = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.edFrozenColumns = new FreeLibSet.Controls.IntEditBox();
      this.label1 = new System.Windows.Forms.Label();
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.colSelColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.colColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ColColumnWidth = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colColumnPercent = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colToolTipVisible = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.ColToolTipName = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.tpToolTips.SuspendLayout();
      this.panGrToolTips.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grToolTips)).BeginInit();
      this.panel3.SuspendLayout();
      this.tpColumns.SuspendLayout();
      this.panGrColumns.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grColumns)).BeginInit();
      this.panel2.SuspendLayout();
      this.TheTabControl.SuspendLayout();
      this.SuspendLayout();
      // 
      // tpToolTips
      // 
      resources.ApplyResources(this.tpToolTips, "tpToolTips");
      this.tpToolTips.Controls.Add(this.panGrToolTips);
      this.tpToolTips.Controls.Add(this.panel3);
      this.tpToolTips.Name = "tpToolTips";
      this.tpToolTips.UseVisualStyleBackColor = true;
      // 
      // panGrToolTips
      // 
      resources.ApplyResources(this.panGrToolTips, "panGrToolTips");
      this.panGrToolTips.Controls.Add(this.grToolTips);
      this.panGrToolTips.Controls.Add(this.panSpbToolTips);
      this.panGrToolTips.Name = "panGrToolTips";
      // 
      // grToolTips
      // 
      resources.ApplyResources(this.grToolTips, "grToolTips");
      this.grToolTips.AllowUserToAddRows = false;
      this.grToolTips.AllowUserToDeleteRows = false;
      this.grToolTips.AllowUserToResizeColumns = false;
      this.grToolTips.AllowUserToResizeRows = false;
      this.grToolTips.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.grToolTips.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colToolTipVisible,
            this.ColToolTipName});
      this.grToolTips.Name = "grToolTips";
      this.grToolTips.RowHeadersVisible = false;
      // 
      // panSpbToolTips
      // 
      resources.ApplyResources(this.panSpbToolTips, "panSpbToolTips");
      this.panSpbToolTips.Name = "panSpbToolTips";
      // 
      // panel3
      // 
      resources.ApplyResources(this.panel3, "panel3");
      this.panel3.Controls.Add(this.cbCurrentCellToolTip);
      this.panel3.Name = "panel3";
      // 
      // cbCurrentCellToolTip
      // 
      resources.ApplyResources(this.cbCurrentCellToolTip, "cbCurrentCellToolTip");
      this.cbCurrentCellToolTip.Name = "cbCurrentCellToolTip";
      this.cbCurrentCellToolTip.UseVisualStyleBackColor = true;
      // 
      // tpColumns
      // 
      resources.ApplyResources(this.tpColumns, "tpColumns");
      this.tpColumns.Controls.Add(this.panGrColumns);
      this.tpColumns.Controls.Add(this.panel2);
      this.tpColumns.Name = "tpColumns";
      this.tpColumns.UseVisualStyleBackColor = true;
      // 
      // panGrColumns
      // 
      resources.ApplyResources(this.panGrColumns, "panGrColumns");
      this.panGrColumns.Controls.Add(this.grColumns);
      this.panGrColumns.Controls.Add(this.panSpbColumns);
      this.panGrColumns.Name = "panGrColumns";
      // 
      // grColumns
      // 
      resources.ApplyResources(this.grColumns, "grColumns");
      this.grColumns.AllowUserToAddRows = false;
      this.grColumns.AllowUserToDeleteRows = false;
      this.grColumns.AllowUserToResizeColumns = false;
      this.grColumns.AllowUserToResizeRows = false;
      this.grColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.grColumns.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSelColumn,
            this.colColumnName,
            this.ColColumnWidth,
            this.colColumnPercent});
      this.grColumns.Name = "grColumns";
      this.grColumns.RowHeadersVisible = false;
      this.grColumns.StandardTab = true;
      // 
      // panSpbColumns
      // 
      resources.ApplyResources(this.panSpbColumns, "panSpbColumns");
      this.panSpbColumns.Name = "panSpbColumns";
      // 
      // panel2
      // 
      resources.ApplyResources(this.panel2, "panel2");
      this.panel2.Controls.Add(this.cbStartColumn);
      this.panel2.Controls.Add(this.label2);
      this.panel2.Controls.Add(this.edFrozenColumns);
      this.panel2.Controls.Add(this.label1);
      this.panel2.Name = "panel2";
      // 
      // cbStartColumn
      // 
      resources.ApplyResources(this.cbStartColumn, "cbStartColumn");
      this.cbStartColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbStartColumn.FormattingEnabled = true;
      this.cbStartColumn.Name = "cbStartColumn";
      // 
      // label2
      // 
      resources.ApplyResources(this.label2, "label2");
      this.label2.Name = "label2";
      // 
      // edFrozenColumns
      // 
      resources.ApplyResources(this.edFrozenColumns, "edFrozenColumns");
      this.edFrozenColumns.Increment = 1;
      this.edFrozenColumns.Minimum = 0;
      this.edFrozenColumns.Name = "edFrozenColumns";
      // 
      // label1
      // 
      resources.ApplyResources(this.label1, "label1");
      this.label1.Name = "label1";
      // 
      // TheTabControl
      // 
      resources.ApplyResources(this.TheTabControl, "TheTabControl");
      this.TheTabControl.Controls.Add(this.tpColumns);
      this.TheTabControl.Controls.Add(this.tpToolTips);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      // 
      // colSelColumn
      // 
      resources.ApplyResources(this.colSelColumn, "colSelColumn");
      this.colSelColumn.Name = "colSelColumn";
      // 
      // colColumnName
      // 
      this.colColumnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      resources.ApplyResources(this.colColumnName, "colColumnName");
      this.colColumnName.Name = "colColumnName";
      this.colColumnName.ReadOnly = true;
      // 
      // ColColumnWidth
      // 
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      this.ColColumnWidth.DefaultCellStyle = dataGridViewCellStyle1;
      resources.ApplyResources(this.ColColumnWidth, "ColColumnWidth");
      this.ColColumnWidth.Name = "ColColumnWidth";
      // 
      // colColumnPercent
      // 
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      this.colColumnPercent.DefaultCellStyle = dataGridViewCellStyle2;
      resources.ApplyResources(this.colColumnPercent, "colColumnPercent");
      this.colColumnPercent.Name = "colColumnPercent";
      // 
      // colToolTipVisible
      // 
      resources.ApplyResources(this.colToolTipVisible, "colToolTipVisible");
      this.colToolTipVisible.Name = "colToolTipVisible";
      // 
      // ColToolTipName
      // 
      this.ColToolTipName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      resources.ApplyResources(this.ColToolTipName, "ColToolTipName");
      this.ColToolTipName.Name = "ColToolTipName";
      this.ColToolTipName.ReadOnly = true;
      // 
      // EFPGridProducerEditor
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.TheTabControl);
      this.MinimizeBox = false;
      this.Name = "EFPGridProducerEditor";
      this.tpToolTips.ResumeLayout(false);
      this.panGrToolTips.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grToolTips)).EndInit();
      this.panel3.ResumeLayout(false);
      this.panel3.PerformLayout();
      this.tpColumns.ResumeLayout(false);
      this.panGrColumns.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grColumns)).EndInit();
      this.panel2.ResumeLayout(false);
      this.TheTabControl.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabPage tpToolTips;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.CheckBox cbCurrentCellToolTip;
    private System.Windows.Forms.TabPage tpColumns;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.ComboBox cbStartColumn;
    private System.Windows.Forms.Label label2;
    private FreeLibSet.Controls.IntEditBox edFrozenColumns;
    private System.Windows.Forms.Label label1;
    public System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.Panel panGrToolTips;
    private System.Windows.Forms.DataGridView grToolTips;
    private System.Windows.Forms.Panel panSpbToolTips;
    private System.Windows.Forms.Panel panGrColumns;
    private System.Windows.Forms.DataGridView grColumns;
    private System.Windows.Forms.Panel panSpbColumns;
    private System.Windows.Forms.DataGridViewCheckBoxColumn colSelColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn colColumnName;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColColumnWidth;
    private System.Windows.Forms.DataGridViewTextBoxColumn colColumnPercent;
    private System.Windows.Forms.DataGridViewCheckBoxColumn colToolTipVisible;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColToolTipName;
  }
}
