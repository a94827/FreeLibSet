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
      this.groupBox1.Controls.Add(this.cbCulture);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(789, 69);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Выбрать настройки";
      // 
      // cbCulture
      // 
      this.cbCulture.Dock = System.Windows.Forms.DockStyle.Top;
      this.cbCulture.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCulture.FormattingEnabled = true;
      this.cbCulture.Location = new System.Drawing.Point(3, 22);
      this.cbCulture.Name = "cbCulture";
      this.cbCulture.Size = new System.Drawing.Size(783, 28);
      this.cbCulture.TabIndex = 0;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnTest);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 478);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(789, 62);
      this.panel1.TabIndex = 2;
      // 
      // btnTest
      // 
      this.btnTest.Location = new System.Drawing.Point(12, 12);
      this.btnTest.Name = "btnTest";
      this.btnTest.Size = new System.Drawing.Size(132, 37);
      this.btnTest.TabIndex = 0;
      this.btnTest.Text = "Тест";
      this.btnTest.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.TheTabControl);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Location = new System.Drawing.Point(0, 69);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(789, 409);
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
      this.TheTabControl.Location = new System.Drawing.Point(3, 22);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(783, 384);
      this.TheTabControl.TabIndex = 1;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.grInfo);
      this.tabPage1.Location = new System.Drawing.Point(4, 29);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
      this.tabPage1.Size = new System.Drawing.Size(775, 351);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Параметры";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // grInfo
      // 
      this.grInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grInfo.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grInfo.Location = new System.Drawing.Point(3, 3);
      this.grInfo.Name = "grInfo";
      this.grInfo.RowTemplate.Height = 24;
      this.grInfo.Size = new System.Drawing.Size(769, 345);
      this.grInfo.TabIndex = 1;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.grFormats);
      this.tabPage2.Location = new System.Drawing.Point(4, 29);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
      this.tabPage2.Size = new System.Drawing.Size(775, 351);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Стандартные форматы";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // grFormats
      // 
      this.grFormats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grFormats.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grFormats.Location = new System.Drawing.Point(3, 3);
      this.grFormats.Name = "grFormats";
      this.grFormats.RowTemplate.Height = 24;
      this.grFormats.Size = new System.Drawing.Size(769, 345);
      this.grFormats.TabIndex = 0;
      // 
      // tpEditableDateTimeFormatter
      // 
      this.tpEditableDateTimeFormatter.Controls.Add(this.grEditableDateTimeFormatter);
      this.tpEditableDateTimeFormatter.Location = new System.Drawing.Point(4, 29);
      this.tpEditableDateTimeFormatter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.tpEditableDateTimeFormatter.Name = "tpEditableDateTimeFormatter";
      this.tpEditableDateTimeFormatter.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.tpEditableDateTimeFormatter.Size = new System.Drawing.Size(775, 351);
      this.tpEditableDateTimeFormatter.TabIndex = 2;
      this.tpEditableDateTimeFormatter.Text = "EditableDateTimeFormatter";
      this.tpEditableDateTimeFormatter.UseVisualStyleBackColor = true;
      // 
      // grEditableDateTimeFormatter
      // 
      this.grEditableDateTimeFormatter.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grEditableDateTimeFormatter.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grEditableDateTimeFormatter.Location = new System.Drawing.Point(4, 5);
      this.grEditableDateTimeFormatter.Name = "grEditableDateTimeFormatter";
      this.grEditableDateTimeFormatter.RowTemplate.Height = 24;
      this.grEditableDateTimeFormatter.Size = new System.Drawing.Size(767, 341);
      this.grEditableDateTimeFormatter.TabIndex = 1;
      // 
      // MainForm
      // 
      this.AcceptButton = this.btnTest;
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(789, 540);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.groupBox1);
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
  }
}

