namespace FreeLibSet.Forms.Reporting
{
  partial class BRDataViewPageSetupDbf
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
      this.MainPanel = new System.Windows.Forms.Panel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.grColumns = new System.Windows.Forms.DataGridView();
      this.panSpbColumns = new System.Windows.Forms.Panel();
      this.panel1 = new System.Windows.Forms.Panel();
      this.cbDbfCodePage = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.MainPanel.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grColumns)).BeginInit();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.groupBox1);
      this.MainPanel.Controls.Add(this.panel1);
      this.MainPanel.Location = new System.Drawing.Point(9, 10);
      this.MainPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(381, 332);
      this.MainPanel.TabIndex = 0;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.grColumns);
      this.groupBox1.Controls.Add(this.panSpbColumns);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox1.Size = new System.Drawing.Size(381, 293);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Столбцы";
      // 
      // grColumns
      // 
      this.grColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grColumns.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grColumns.Location = new System.Drawing.Point(2, 48);
      this.grColumns.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.grColumns.Name = "grColumns";
      this.grColumns.RowTemplate.Height = 24;
      this.grColumns.Size = new System.Drawing.Size(377, 243);
      this.grColumns.TabIndex = 2;
      // 
      // panSpbColumns
      // 
      this.panSpbColumns.Dock = System.Windows.Forms.DockStyle.Top;
      this.panSpbColumns.Location = new System.Drawing.Point(2, 15);
      this.panSpbColumns.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.panSpbColumns.Name = "panSpbColumns";
      this.panSpbColumns.Size = new System.Drawing.Size(377, 33);
      this.panSpbColumns.TabIndex = 1;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.cbDbfCodePage);
      this.panel1.Controls.Add(this.label1);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 293);
      this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(381, 39);
      this.panel1.TabIndex = 1;
      // 
      // cbDbfCodePage
      // 
      this.cbDbfCodePage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbDbfCodePage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDbfCodePage.FormattingEnabled = true;
      this.cbDbfCodePage.Location = new System.Drawing.Point(127, 11);
      this.cbDbfCodePage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbDbfCodePage.Name = "cbDbfCodePage";
      this.cbDbfCodePage.Size = new System.Drawing.Size(245, 21);
      this.cbDbfCodePage.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(10, 12);
      this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(113, 19);
      this.label1.TabIndex = 0;
      this.label1.Text = "Кодировка";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // BRDataViewPageSetupDbf
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(399, 351);
      this.Controls.Add(this.MainPanel);
      this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.Name = "BRDataViewPageSetupDbf";
      this.Text = "BRDataViewPageSetupDbf";
      this.MainPanel.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grColumns)).EndInit();
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.ComboBox cbDbfCodePage;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.DataGridView grColumns;
    private System.Windows.Forms.Panel panSpbColumns;
  }
}