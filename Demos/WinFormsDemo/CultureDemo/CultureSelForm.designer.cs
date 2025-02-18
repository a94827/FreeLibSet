namespace WinFormsDemo.CultureDemo
{
  partial class CultureSelForm
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
      this.grpCulture = new System.Windows.Forms.GroupBox();
      this.btnShowTable = new System.Windows.Forms.Button();
      this.cbCulture = new System.Windows.Forms.ComboBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnTest = new System.Windows.Forms.Button();
      this.grpResults = new System.Windows.Forms.GroupBox();
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.tpInfo = new System.Windows.Forms.TabPage();
      this.grInfo = new System.Windows.Forms.DataGridView();
      this.tpFormats = new System.Windows.Forms.TabPage();
      this.grFormats = new System.Windows.Forms.DataGridView();
      this.tpEditableDateTimeFormatter = new System.Windows.Forms.TabPage();
      this.grEditableDateTimeFormatter = new System.Windows.Forms.DataGridView();
      this.grpCulture.SuspendLayout();
      this.panel1.SuspendLayout();
      this.grpResults.SuspendLayout();
      this.TheTabControl.SuspendLayout();
      this.tpInfo.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grInfo)).BeginInit();
      this.tpFormats.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grFormats)).BeginInit();
      this.tpEditableDateTimeFormatter.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grEditableDateTimeFormatter)).BeginInit();
      this.SuspendLayout();
      // 
      // grpCulture
      // 
      this.grpCulture.Controls.Add(this.btnShowTable);
      this.grpCulture.Controls.Add(this.cbCulture);
      this.grpCulture.Dock = System.Windows.Forms.DockStyle.Top;
      this.grpCulture.Location = new System.Drawing.Point(0, 0);
      this.grpCulture.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.grpCulture.Name = "grpCulture";
      this.grpCulture.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.grpCulture.Size = new System.Drawing.Size(701, 55);
      this.grpCulture.TabIndex = 0;
      this.grpCulture.TabStop = false;
      this.grpCulture.Text = "Selected culture";
      // 
      // btnShowTable
      // 
      this.btnShowTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnShowTable.Location = new System.Drawing.Point(648, 17);
      this.btnShowTable.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.btnShowTable.Name = "btnShowTable";
      this.btnShowTable.Size = new System.Drawing.Size(43, 30);
      this.btnShowTable.TabIndex = 1;
      this.btnShowTable.UseVisualStyleBackColor = true;
      // 
      // cbCulture
      // 
      this.cbCulture.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCulture.FormattingEnabled = true;
      this.cbCulture.Location = new System.Drawing.Point(5, 21);
      this.cbCulture.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.cbCulture.Name = "cbCulture";
      this.cbCulture.Size = new System.Drawing.Size(636, 24);
      this.cbCulture.TabIndex = 0;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnTest);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 383);
      this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(701, 49);
      this.panel1.TabIndex = 2;
      // 
      // btnTest
      // 
      this.btnTest.Location = new System.Drawing.Point(11, 10);
      this.btnTest.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.btnTest.Name = "btnTest";
      this.btnTest.Size = new System.Drawing.Size(117, 30);
      this.btnTest.TabIndex = 0;
      this.btnTest.Text = "Test";
      this.btnTest.UseVisualStyleBackColor = true;
      // 
      // grpResults
      // 
      this.grpResults.Controls.Add(this.TheTabControl);
      this.grpResults.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpResults.Location = new System.Drawing.Point(0, 55);
      this.grpResults.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.grpResults.Name = "grpResults";
      this.grpResults.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.grpResults.Size = new System.Drawing.Size(701, 328);
      this.grpResults.TabIndex = 1;
      this.grpResults.TabStop = false;
      this.grpResults.Text = "Language settings";
      // 
      // TheTabControl
      // 
      this.TheTabControl.Controls.Add(this.tpInfo);
      this.TheTabControl.Controls.Add(this.tpFormats);
      this.TheTabControl.Controls.Add(this.tpEditableDateTimeFormatter);
      this.TheTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTabControl.Location = new System.Drawing.Point(3, 17);
      this.TheTabControl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(695, 309);
      this.TheTabControl.TabIndex = 1;
      // 
      // tpInfo
      // 
      this.tpInfo.Controls.Add(this.grInfo);
      this.tpInfo.Location = new System.Drawing.Point(4, 25);
      this.tpInfo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tpInfo.Name = "tpInfo";
      this.tpInfo.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tpInfo.Size = new System.Drawing.Size(687, 280);
      this.tpInfo.TabIndex = 0;
      this.tpInfo.Text = "Language info";
      this.tpInfo.UseVisualStyleBackColor = true;
      // 
      // grInfo
      // 
      this.grInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grInfo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grInfo.Location = new System.Drawing.Point(3, 2);
      this.grInfo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.grInfo.Name = "grInfo";
      this.grInfo.RowTemplate.Height = 24;
      this.grInfo.Size = new System.Drawing.Size(681, 276);
      this.grInfo.TabIndex = 1;
      // 
      // tpFormats
      // 
      this.tpFormats.Controls.Add(this.grFormats);
      this.tpFormats.Location = new System.Drawing.Point(4, 25);
      this.tpFormats.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tpFormats.Name = "tpFormats";
      this.tpFormats.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tpFormats.Size = new System.Drawing.Size(687, 280);
      this.tpFormats.TabIndex = 1;
      this.tpFormats.Text = "Std. formats";
      this.tpFormats.UseVisualStyleBackColor = true;
      // 
      // grFormats
      // 
      this.grFormats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grFormats.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grFormats.Location = new System.Drawing.Point(3, 2);
      this.grFormats.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.grFormats.Name = "grFormats";
      this.grFormats.RowTemplate.Height = 24;
      this.grFormats.Size = new System.Drawing.Size(681, 276);
      this.grFormats.TabIndex = 0;
      // 
      // tpEditableDateTimeFormatter
      // 
      this.tpEditableDateTimeFormatter.Controls.Add(this.grEditableDateTimeFormatter);
      this.tpEditableDateTimeFormatter.Location = new System.Drawing.Point(4, 25);
      this.tpEditableDateTimeFormatter.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.tpEditableDateTimeFormatter.Name = "tpEditableDateTimeFormatter";
      this.tpEditableDateTimeFormatter.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.tpEditableDateTimeFormatter.Size = new System.Drawing.Size(688, 277);
      this.tpEditableDateTimeFormatter.TabIndex = 2;
      this.tpEditableDateTimeFormatter.Text = "EditableDateTimeFormatter";
      this.tpEditableDateTimeFormatter.UseVisualStyleBackColor = true;
      // 
      // grEditableDateTimeFormatter
      // 
      this.grEditableDateTimeFormatter.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grEditableDateTimeFormatter.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grEditableDateTimeFormatter.Location = new System.Drawing.Point(4, 4);
      this.grEditableDateTimeFormatter.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.grEditableDateTimeFormatter.Name = "grEditableDateTimeFormatter";
      this.grEditableDateTimeFormatter.RowTemplate.Height = 24;
      this.grEditableDateTimeFormatter.Size = new System.Drawing.Size(680, 269);
      this.grEditableDateTimeFormatter.TabIndex = 1;
      // 
      // CultureSelForm
      // 
      this.AcceptButton = this.btnTest;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(701, 432);
      this.Controls.Add(this.grpResults);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.grpCulture);
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.Name = "CultureSelForm";
      this.Text = "CultureInfo testing";
      this.grpCulture.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.grpResults.ResumeLayout(false);
      this.TheTabControl.ResumeLayout(false);
      this.tpInfo.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grInfo)).EndInit();
      this.tpFormats.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grFormats)).EndInit();
      this.tpEditableDateTimeFormatter.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grEditableDateTimeFormatter)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox grpCulture;
    private System.Windows.Forms.ComboBox cbCulture;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnTest;
    private System.Windows.Forms.GroupBox grpResults;
    private System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.TabPage tpInfo;
    private System.Windows.Forms.DataGridView grInfo;
    private System.Windows.Forms.TabPage tpFormats;
    private System.Windows.Forms.DataGridView grFormats;
    private System.Windows.Forms.TabPage tpEditableDateTimeFormatter;
    private System.Windows.Forms.DataGridView grEditableDateTimeFormatter;
    private System.Windows.Forms.Button btnShowTable;
  }
}

