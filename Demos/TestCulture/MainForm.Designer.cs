namespace TestCulture
{
  partial class MainForm
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbCulture = new System.Windows.Forms.ComboBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnTest = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.grInfo = new System.Windows.Forms.DataGridView();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.grFormats = new System.Windows.Forms.DataGridView();
      this.tpEditableDateTimeFormatter = new System.Windows.Forms.TabPage();
      this.grEditableDateTimeFormatter = new System.Windows.Forms.DataGridView();
      this.btnShowTable = new System.Windows.Forms.Button();
      this.groupBox1.SuspendLayout();
      this.panel1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.TheTabControl.SuspendLayout();
      this.tabPage1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grInfo)).BeginInit();
      this.tabPage2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grFormats)).BeginInit();
      this.tpEditableDateTimeFormatter.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grEditableDateTimeFormatter)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.btnShowTable);
      this.groupBox1.Controls.Add(this.cbCulture);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox1.Size = new System.Drawing.Size(526, 45);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Выбрать настройки";
      // 
      // cbCulture
      // 
      this.cbCulture.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCulture.FormattingEnabled = true;
      this.cbCulture.Location = new System.Drawing.Point(4, 17);
      this.cbCulture.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbCulture.Name = "cbCulture";
      this.cbCulture.Size = new System.Drawing.Size(478, 21);
      this.cbCulture.TabIndex = 0;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnTest);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 311);
      this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(526, 40);
      this.panel1.TabIndex = 2;
      // 
      // btnTest
      // 
      this.btnTest.Location = new System.Drawing.Point(8, 8);
      this.btnTest.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.btnTest.Name = "btnTest";
      this.btnTest.Size = new System.Drawing.Size(88, 24);
      this.btnTest.TabIndex = 0;
      this.btnTest.Text = "Тест";
      this.btnTest.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.TheTabControl);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Location = new System.Drawing.Point(0, 45);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.groupBox2.Size = new System.Drawing.Size(526, 266);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Региональные стандарты";
      // 
      // TheTabControl
      // 
      this.TheTabControl.Controls.Add(this.tabPage1);
      this.TheTabControl.Controls.Add(this.tabPage2);
      this.TheTabControl.Controls.Add(this.tpEditableDateTimeFormatter);
      this.TheTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTabControl.Location = new System.Drawing.Point(2, 15);
      this.TheTabControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(522, 249);
      this.TheTabControl.TabIndex = 1;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.grInfo);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.tabPage1.Size = new System.Drawing.Size(514, 223);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Параметры";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // grInfo
      // 
      this.grInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grInfo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grInfo.Location = new System.Drawing.Point(2, 2);
      this.grInfo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.grInfo.Name = "grInfo";
      this.grInfo.RowTemplate.Height = 24;
      this.grInfo.Size = new System.Drawing.Size(510, 219);
      this.grInfo.TabIndex = 1;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.grFormats);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.tabPage2.Size = new System.Drawing.Size(514, 224);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Стандартные форматы";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // grFormats
      // 
      this.grFormats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grFormats.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grFormats.Location = new System.Drawing.Point(2, 2);
      this.grFormats.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.grFormats.Name = "grFormats";
      this.grFormats.RowTemplate.Height = 24;
      this.grFormats.Size = new System.Drawing.Size(510, 220);
      this.grFormats.TabIndex = 0;
      // 
      // tpEditableDateTimeFormatter
      // 
      this.tpEditableDateTimeFormatter.Controls.Add(this.grEditableDateTimeFormatter);
      this.tpEditableDateTimeFormatter.Location = new System.Drawing.Point(4, 22);
      this.tpEditableDateTimeFormatter.Name = "tpEditableDateTimeFormatter";
      this.tpEditableDateTimeFormatter.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
      this.tpEditableDateTimeFormatter.Size = new System.Drawing.Size(514, 224);
      this.tpEditableDateTimeFormatter.TabIndex = 2;
      this.tpEditableDateTimeFormatter.Text = "EditableDateTimeFormatter";
      this.tpEditableDateTimeFormatter.UseVisualStyleBackColor = true;
      // 
      // grEditableDateTimeFormatter
      // 
      this.grEditableDateTimeFormatter.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grEditableDateTimeFormatter.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grEditableDateTimeFormatter.Location = new System.Drawing.Point(3, 3);
      this.grEditableDateTimeFormatter.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.grEditableDateTimeFormatter.Name = "grEditableDateTimeFormatter";
      this.grEditableDateTimeFormatter.RowTemplate.Height = 24;
      this.grEditableDateTimeFormatter.Size = new System.Drawing.Size(508, 218);
      this.grEditableDateTimeFormatter.TabIndex = 1;
      // 
      // btnShowTable
      // 
      this.btnShowTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnShowTable.Location = new System.Drawing.Point(486, 14);
      this.btnShowTable.Name = "btnShowTable";
      this.btnShowTable.Size = new System.Drawing.Size(32, 24);
      this.btnShowTable.TabIndex = 1;
      this.btnShowTable.UseVisualStyleBackColor = true;
      // 
      // MainForm
      // 
      this.AcceptButton = this.btnTest;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(526, 351);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.groupBox1);
      this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.Name = "MainForm";
      this.Text = "Тестирование настроек языка";
      this.groupBox1.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.TheTabControl.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grInfo)).EndInit();
      this.tabPage2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grFormats)).EndInit();
      this.tpEditableDateTimeFormatter.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grEditableDateTimeFormatter)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.ComboBox cbCulture;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnTest;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.DataGridView grInfo;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.DataGridView grFormats;
    private System.Windows.Forms.TabPage tpEditableDateTimeFormatter;
    private System.Windows.Forms.DataGridView grEditableDateTimeFormatter;
    private System.Windows.Forms.Button btnShowTable;
  }
}

