namespace AgeyevAV.ExtForms
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
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      this.tpToolTips = new System.Windows.Forms.TabPage();
      this.panel3 = new System.Windows.Forms.Panel();
      this.cbCurrentCellToolTip = new System.Windows.Forms.CheckBox();
      this.tpColumns = new System.Windows.Forms.TabPage();
      this.panel2 = new System.Windows.Forms.Panel();
      this.cbStartColumn = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.edFrozenColumns = new AgeyevAV.ExtForms.ExtNumericUpDown();
      this.label1 = new System.Windows.Forms.Label();
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.panGrColumns = new System.Windows.Forms.Panel();
      this.grColumns = new System.Windows.Forms.DataGridView();
      this.colSelColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.colColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ColColumnWidth = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colColumnPercent = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.panSpbColumns = new System.Windows.Forms.Panel();
      this.panGrToolTips = new System.Windows.Forms.Panel();
      this.grToolTips = new System.Windows.Forms.DataGridView();
      this.colToolTipVisible = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.ColToolTipName = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.panSpbToolTips = new System.Windows.Forms.Panel();
      this.tpToolTips.SuspendLayout();
      this.panel3.SuspendLayout();
      this.tpColumns.SuspendLayout();
      this.panel2.SuspendLayout();
      this.TheTabControl.SuspendLayout();
      this.panGrColumns.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grColumns)).BeginInit();
      this.panGrToolTips.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grToolTips)).BeginInit();
      this.SuspendLayout();
      // 
      // tpToolTips
      // 
      this.tpToolTips.Controls.Add(this.panGrToolTips);
      this.tpToolTips.Controls.Add(this.panel3);
      this.tpToolTips.Location = new System.Drawing.Point(4, 22);
      this.tpToolTips.Name = "tpToolTips";
      this.tpToolTips.Padding = new System.Windows.Forms.Padding(3);
      this.tpToolTips.Size = new System.Drawing.Size(545, 304);
      this.tpToolTips.TabIndex = 1;
      this.tpToolTips.Text = "Всплывающие подсказки";
      this.tpToolTips.UseVisualStyleBackColor = true;
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.cbCurrentCellToolTip);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel3.Location = new System.Drawing.Point(3, 3);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(539, 33);
      this.panel3.TabIndex = 0;
      // 
      // cbCurrentCellToolTip
      // 
      this.cbCurrentCellToolTip.AutoSize = true;
      this.cbCurrentCellToolTip.Location = new System.Drawing.Point(5, 10);
      this.cbCurrentCellToolTip.Name = "cbCurrentCellToolTip";
      this.cbCurrentCellToolTip.Size = new System.Drawing.Size(194, 17);
      this.cbCurrentCellToolTip.TabIndex = 0;
      this.cbCurrentCellToolTip.Text = "Подсказка по выбранной ячейке";
      this.cbCurrentCellToolTip.UseVisualStyleBackColor = true;
      // 
      // tpColumns
      // 
      this.tpColumns.Controls.Add(this.panGrColumns);
      this.tpColumns.Controls.Add(this.panel2);
      this.tpColumns.Location = new System.Drawing.Point(4, 22);
      this.tpColumns.Name = "tpColumns";
      this.tpColumns.Padding = new System.Windows.Forms.Padding(3);
      this.tpColumns.Size = new System.Drawing.Size(545, 304);
      this.tpColumns.TabIndex = 0;
      this.tpColumns.Text = "Столбцы";
      this.tpColumns.UseVisualStyleBackColor = true;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.cbStartColumn);
      this.panel2.Controls.Add(this.label2);
      this.panel2.Controls.Add(this.edFrozenColumns);
      this.panel2.Controls.Add(this.label1);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel2.Location = new System.Drawing.Point(3, 237);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(539, 64);
      this.panel2.TabIndex = 1;
      // 
      // cbStartColumn
      // 
      this.cbStartColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbStartColumn.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbStartColumn.FormattingEnabled = true;
      this.cbStartColumn.Location = new System.Drawing.Point(208, 35);
      this.cbStartColumn.Name = "cbStartColumn";
      this.cbStartColumn.Size = new System.Drawing.Size(326, 21);
      this.cbStartColumn.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(8, 35);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(194, 21);
      this.label2.TabIndex = 2;
      this.label2.Text = "Активировать при открытии";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edFrozenColumns
      // 
      this.edFrozenColumns.Location = new System.Drawing.Point(208, 8);
      this.edFrozenColumns.Name = "edFrozenColumns";
      this.edFrozenColumns.Size = new System.Drawing.Size(61, 20);
      this.edFrozenColumns.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(5, 8);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(197, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "Число \"&замороженных\" столбцов";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // TheTabControl
      // 
      this.TheTabControl.Controls.Add(this.tpColumns);
      this.TheTabControl.Controls.Add(this.tpToolTips);
      this.TheTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTabControl.Location = new System.Drawing.Point(0, 0);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(553, 330);
      this.TheTabControl.TabIndex = 0;
      // 
      // panGrColumns
      // 
      this.panGrColumns.Controls.Add(this.grColumns);
      this.panGrColumns.Controls.Add(this.panSpbColumns);
      this.panGrColumns.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panGrColumns.Location = new System.Drawing.Point(3, 3);
      this.panGrColumns.Name = "panGrColumns";
      this.panGrColumns.Size = new System.Drawing.Size(539, 234);
      this.panGrColumns.TabIndex = 0;
      // 
      // grColumns
      // 
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
      this.grColumns.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grColumns.Location = new System.Drawing.Point(0, 30);
      this.grColumns.Name = "grColumns";
      this.grColumns.RowHeadersVisible = false;
      this.grColumns.Size = new System.Drawing.Size(539, 204);
      this.grColumns.StandardTab = true;
      this.grColumns.TabIndex = 1;
      // 
      // colSelColumn
      // 
      this.colSelColumn.HeaderText = "Вид";
      this.colSelColumn.Name = "colSelColumn";
      this.colSelColumn.Width = 22;
      // 
      // colColumnName
      // 
      this.colColumnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.colColumnName.HeaderText = "Столбец";
      this.colColumnName.MinimumWidth = 200;
      this.colColumnName.Name = "colColumnName";
      this.colColumnName.ReadOnly = true;
      // 
      // ColColumnWidth
      // 
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      this.ColColumnWidth.DefaultCellStyle = dataGridViewCellStyle1;
      this.ColColumnWidth.HeaderText = "Ширина";
      this.ColColumnWidth.MinimumWidth = 80;
      this.ColColumnWidth.Name = "ColColumnWidth";
      this.ColColumnWidth.Width = 80;
      // 
      // colColumnPercent
      // 
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      this.colColumnPercent.DefaultCellStyle = dataGridViewCellStyle2;
      this.colColumnPercent.HeaderText = "%";
      this.colColumnPercent.MinimumWidth = 60;
      this.colColumnPercent.Name = "colColumnPercent";
      this.colColumnPercent.Width = 60;
      // 
      // panSpbColumns
      // 
      this.panSpbColumns.Dock = System.Windows.Forms.DockStyle.Top;
      this.panSpbColumns.Location = new System.Drawing.Point(0, 0);
      this.panSpbColumns.Name = "panSpbColumns";
      this.panSpbColumns.Size = new System.Drawing.Size(539, 30);
      this.panSpbColumns.TabIndex = 0;
      // 
      // panGrToolTips
      // 
      this.panGrToolTips.Controls.Add(this.grToolTips);
      this.panGrToolTips.Controls.Add(this.panSpbToolTips);
      this.panGrToolTips.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panGrToolTips.Location = new System.Drawing.Point(3, 36);
      this.panGrToolTips.Name = "panGrToolTips";
      this.panGrToolTips.Size = new System.Drawing.Size(539, 265);
      this.panGrToolTips.TabIndex = 1;
      // 
      // grToolTips
      // 
      this.grToolTips.AllowUserToAddRows = false;
      this.grToolTips.AllowUserToDeleteRows = false;
      this.grToolTips.AllowUserToResizeColumns = false;
      this.grToolTips.AllowUserToResizeRows = false;
      this.grToolTips.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
      this.grToolTips.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colToolTipVisible,
            this.ColToolTipName});
      this.grToolTips.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grToolTips.Location = new System.Drawing.Point(0, 33);
      this.grToolTips.Name = "grToolTips";
      this.grToolTips.RowHeadersVisible = false;
      this.grToolTips.Size = new System.Drawing.Size(539, 232);
      this.grToolTips.TabIndex = 1;
      // 
      // colToolTipVisible
      // 
      this.colToolTipVisible.HeaderText = "Вид";
      this.colToolTipVisible.Name = "colToolTipVisible";
      this.colToolTipVisible.Width = 22;
      // 
      // ColToolTipName
      // 
      this.ColToolTipName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.ColToolTipName.HeaderText = "Элемент подсказки";
      this.ColToolTipName.MinimumWidth = 200;
      this.ColToolTipName.Name = "ColToolTipName";
      this.ColToolTipName.ReadOnly = true;
      // 
      // panSpbToolTips
      // 
      this.panSpbToolTips.Dock = System.Windows.Forms.DockStyle.Top;
      this.panSpbToolTips.Location = new System.Drawing.Point(0, 0);
      this.panSpbToolTips.Name = "panSpbToolTips";
      this.panSpbToolTips.Size = new System.Drawing.Size(539, 33);
      this.panSpbToolTips.TabIndex = 0;
      // 
      // GridProducerEditor
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(553, 330);
      this.Controls.Add(this.TheTabControl);
      this.MinimizeBox = false;
      this.Name = "GridProducerEditor";
      this.Text = "Настройка просмотра";
      this.tpToolTips.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.panel3.PerformLayout();
      this.tpColumns.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.TheTabControl.ResumeLayout(false);
      this.panGrColumns.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grColumns)).EndInit();
      this.panGrToolTips.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grToolTips)).EndInit();
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
    private AgeyevAV.ExtForms.ExtNumericUpDown edFrozenColumns;
    private System.Windows.Forms.Label label1;
    public System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.Panel panGrToolTips;
    private System.Windows.Forms.DataGridView grToolTips;
    private System.Windows.Forms.DataGridViewCheckBoxColumn colToolTipVisible;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColToolTipName;
    private System.Windows.Forms.Panel panSpbToolTips;
    private System.Windows.Forms.Panel panGrColumns;
    private System.Windows.Forms.DataGridView grColumns;
    private System.Windows.Forms.DataGridViewCheckBoxColumn colSelColumn;
    private System.Windows.Forms.DataGridViewTextBoxColumn colColumnName;
    private System.Windows.Forms.DataGridViewTextBoxColumn ColColumnWidth;
    private System.Windows.Forms.DataGridViewTextBoxColumn colColumnPercent;
    private System.Windows.Forms.Panel panSpbColumns;

  }
}