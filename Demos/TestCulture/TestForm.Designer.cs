namespace TestCulture
{
  partial class TestForm
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.cbDoRB = new AgeyevAV.ExtForms.UserMaskedComboBox();
      this.edNUD2 = new AgeyevAV.ExtForms.ExtNumericUpDown();
      this.label10 = new System.Windows.Forms.Label();
      this.edNUD1 = new System.Windows.Forms.NumericUpDown();
      this.label9 = new System.Windows.Forms.Label();
      this.edNEB = new AgeyevAV.ExtForms.NumEditBox();
      this.label8 = new System.Windows.Forms.Label();
      this.edMDB = new AgeyevAV.ExtForms.MonthDayBox();
      this.label7 = new System.Windows.Forms.Label();
      this.edYMRB = new AgeyevAV.ExtForms.YearMonthRangeBox();
      this.label6 = new System.Windows.Forms.Label();
      this.edYMB = new AgeyevAV.ExtForms.YearMonthBox();
      this.label5 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.edDRB = new AgeyevAV.ExtForms.DateRangeBox();
      this.label3 = new System.Windows.Forms.Label();
      this.edDB = new AgeyevAV.ExtForms.DateBox();
      this.label2 = new System.Windows.Forms.Label();
      this.edDTP = new System.Windows.Forms.DateTimePicker();
      this.label1 = new System.Windows.Forms.Label();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.gr1 = new System.Windows.Forms.DataGridView();
      this.panel1.SuspendLayout();
      this.TheTabControl.SuspendLayout();
      this.tabPage1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.edNUD1)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edNEB)).BeginInit();
      this.tabPage2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.gr1)).BeginInit();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 343);
      this.panel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(531, 40);
      this.panel1.TabIndex = 0;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(100, 8);
      this.btnCancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // TheTabControl
      // 
      this.TheTabControl.Controls.Add(this.tabPage1);
      this.TheTabControl.Controls.Add(this.tabPage2);
      this.TheTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTabControl.Location = new System.Drawing.Point(0, 0);
      this.TheTabControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(531, 343);
      this.TheTabControl.TabIndex = 1;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.cbDoRB);
      this.tabPage1.Controls.Add(this.edNUD2);
      this.tabPage1.Controls.Add(this.label10);
      this.tabPage1.Controls.Add(this.edNUD1);
      this.tabPage1.Controls.Add(this.label9);
      this.tabPage1.Controls.Add(this.edNEB);
      this.tabPage1.Controls.Add(this.label8);
      this.tabPage1.Controls.Add(this.edMDB);
      this.tabPage1.Controls.Add(this.label7);
      this.tabPage1.Controls.Add(this.edYMRB);
      this.tabPage1.Controls.Add(this.label6);
      this.tabPage1.Controls.Add(this.edYMB);
      this.tabPage1.Controls.Add(this.label5);
      this.tabPage1.Controls.Add(this.label4);
      this.tabPage1.Controls.Add(this.edDRB);
      this.tabPage1.Controls.Add(this.label3);
      this.tabPage1.Controls.Add(this.edDB);
      this.tabPage1.Controls.Add(this.label2);
      this.tabPage1.Controls.Add(this.edDTP);
      this.tabPage1.Controls.Add(this.label1);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.tabPage1.Size = new System.Drawing.Size(523, 317);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "Управляющие элементы";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // cbDoRB
      // 
      this.cbDoRB.ClearButtonEnabled = false;
      this.cbDoRB.Culture = new System.Globalization.CultureInfo("ru-RU");
      this.cbDoRB.Location = new System.Drawing.Point(140, 112);
      this.cbDoRB.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.cbDoRB.Name = "cbDoRB";
      this.cbDoRB.Size = new System.Drawing.Size(143, 20);
      this.cbDoRB.TabIndex = 7;
      // 
      // edNUD2
      // 
      this.edNUD2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.edNUD2.Location = new System.Drawing.Point(140, 276);
      this.edNUD2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.edNUD2.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.edNUD2.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
      this.edNUD2.Name = "edNUD2";
      this.edNUD2.Size = new System.Drawing.Size(91, 20);
      this.edNUD2.TabIndex = 19;
      // 
      // label10
      // 
      this.label10.Location = new System.Drawing.Point(7, 276);
      this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(129, 18);
      this.label10.TabIndex = 18;
      this.label10.Text = "ExtNumericUpDown";
      this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edNUD1
      // 
      this.edNUD1.Location = new System.Drawing.Point(140, 254);
      this.edNUD1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.edNUD1.Name = "edNUD1";
      this.edNUD1.Size = new System.Drawing.Size(91, 20);
      this.edNUD1.TabIndex = 17;
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(6, 254);
      this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(129, 18);
      this.label9.TabIndex = 16;
      this.label9.Text = "NumericUpDown";
      this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edNEB
      // 
      this.edNEB.DecimalPlaces = 2;
      this.edNEB.Location = new System.Drawing.Point(140, 231);
      this.edNEB.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.edNEB.Name = "edNEB";
      this.edNEB.Size = new System.Drawing.Size(151, 20);
      this.edNEB.TabIndex = 15;
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(6, 231);
      this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(129, 18);
      this.label8.TabIndex = 14;
      this.label8.Text = "NumEditBox";
      this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edMDB
      // 
      this.edMDB.Location = new System.Drawing.Point(140, 187);
      this.edMDB.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.edMDB.Name = "edMDB";
      this.edMDB.Size = new System.Drawing.Size(150, 17);
      this.edMDB.TabIndex = 13;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(6, 187);
      this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(129, 18);
      this.label7.TabIndex = 12;
      this.label7.Text = "MonthDayBox";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edYMRB
      // 
      this.edYMRB.Location = new System.Drawing.Point(140, 162);
      this.edYMRB.Name = "edYMRB";
      this.edYMRB.Size = new System.Drawing.Size(370, 21);
      this.edYMRB.TabIndex = 11;
      this.edYMRB.Year = 2021;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(6, 163);
      this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(129, 18);
      this.label6.TabIndex = 10;
      this.label6.Text = "YearMonthRangeBox";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edYMB
      // 
      this.edYMB.Location = new System.Drawing.Point(140, 136);
      this.edYMB.Month = 2;
      this.edYMB.Name = "edYMB";
      this.edYMB.Size = new System.Drawing.Size(345, 21);
      this.edYMB.TabIndex = 9;
      this.edYMB.Year = 2021;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(6, 137);
      this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(129, 18);
      this.label5.TabIndex = 8;
      this.label5.Text = "YearMonthBox";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(6, 112);
      this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(129, 18);
      this.label4.TabIndex = 6;
      this.label4.Text = "DateOrRangeBox";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDRB
      // 
      this.edDRB.Location = new System.Drawing.Point(140, 70);
      this.edDRB.Name = "edDRB";
      this.edDRB.Size = new System.Drawing.Size(350, 37);
      this.edDRB.TabIndex = 5;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(6, 70);
      this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(129, 18);
      this.label3.TabIndex = 4;
      this.label3.Text = "DateRangeBox";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDB
      // 
      this.edDB.Location = new System.Drawing.Point(140, 46);
      this.edDB.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.edDB.Name = "edDB";
      this.edDB.Size = new System.Drawing.Size(90, 20);
      this.edDB.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(6, 44);
      this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(129, 18);
      this.label2.TabIndex = 2;
      this.label2.Text = "DateBox";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDTP
      // 
      this.edDTP.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      this.edDTP.Location = new System.Drawing.Point(140, 20);
      this.edDTP.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.edDTP.Name = "edDTP";
      this.edDTP.Size = new System.Drawing.Size(151, 20);
      this.edDTP.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(6, 21);
      this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(129, 18);
      this.label1.TabIndex = 0;
      this.label1.Text = "DateTimePicker";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.gr1);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.tabPage2.Size = new System.Drawing.Size(523, 317);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "Табличный просмотр";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // gr1
      // 
      this.gr1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.gr1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.gr1.Location = new System.Drawing.Point(2, 2);
      this.gr1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.gr1.Name = "gr1";
      this.gr1.RowTemplate.Height = 24;
      this.gr1.Size = new System.Drawing.Size(519, 313);
      this.gr1.TabIndex = 0;
      // 
      // TestForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(531, 383);
      this.Controls.Add(this.TheTabControl);
      this.Controls.Add(this.panel1);
      this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.Name = "TestForm";
      this.Text = "TestForm";
      this.panel1.ResumeLayout(false);
      this.TheTabControl.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.edNUD1)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edNEB)).EndInit();
      this.tabPage2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.gr1)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.TabPage tabPage1;
    private AgeyevAV.ExtForms.UserMaskedComboBox cbDoRB;
    private AgeyevAV.ExtForms.ExtNumericUpDown edNUD2;
    private System.Windows.Forms.Label label10;
    private System.Windows.Forms.NumericUpDown edNUD1;
    private System.Windows.Forms.Label label9;
    private AgeyevAV.ExtForms.NumEditBox edNEB;
    private System.Windows.Forms.Label label8;
    private AgeyevAV.ExtForms.MonthDayBox edMDB;
    private System.Windows.Forms.Label label7;
    private AgeyevAV.ExtForms.YearMonthRangeBox edYMRB;
    private System.Windows.Forms.Label label6;
    private AgeyevAV.ExtForms.YearMonthBox edYMB;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private AgeyevAV.ExtForms.DateRangeBox edDRB;
    private System.Windows.Forms.Label label3;
    private AgeyevAV.ExtForms.DateBox edDB;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.DateTimePicker edDTP;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.DataGridView gr1;

  }
}
