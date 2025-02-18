namespace WinFormsDemo.CultureDemo
{
  partial class CultureTestForm
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
      this.tp1 = new System.Windows.Forms.TabPage();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.label12 = new System.Windows.Forms.Label();
      this.edDB5 = new FreeLibSet.Controls.DateTimeBox();
      this.label11 = new System.Windows.Forms.Label();
      this.edDB4 = new FreeLibSet.Controls.DateTimeBox();
      this.label10 = new System.Windows.Forms.Label();
      this.edDB3 = new FreeLibSet.Controls.DateTimeBox();
      this.label9 = new System.Windows.Forms.Label();
      this.edDB2 = new FreeLibSet.Controls.DateTimeBox();
      this.label2 = new System.Windows.Forms.Label();
      this.edDB1 = new FreeLibSet.Controls.DateTimeBox();
      this.tp2 = new System.Windows.Forms.TabPage();
      this.cbDoRB = new FreeLibSet.Controls.UserMaskedComboBox();
      this.edDEB = new FreeLibSet.Controls.DecimalEditBox();
      this.label8 = new System.Windows.Forms.Label();
      this.edMDB = new FreeLibSet.Controls.MonthDayBox();
      this.label7 = new System.Windows.Forms.Label();
      this.edYMRB = new FreeLibSet.Controls.YearMonthRangeBox();
      this.label6 = new System.Windows.Forms.Label();
      this.edYMB = new FreeLibSet.Controls.YearMonthBox();
      this.label5 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.edDRB = new FreeLibSet.Controls.DateRangeBox();
      this.label3 = new System.Windows.Forms.Label();
      this.edDTP = new System.Windows.Forms.DateTimePicker();
      this.label1 = new System.Windows.Forms.Label();
      this.tp3 = new System.Windows.Forms.TabPage();
      this.gr1 = new System.Windows.Forms.DataGridView();
      this.panel1.SuspendLayout();
      this.TheTabControl.SuspendLayout();
      this.tp1.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.tp2.SuspendLayout();
      this.tp3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.gr1)).BeginInit();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 422);
      this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(764, 49);
      this.panel1.TabIndex = 0;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(133, 10);
      this.btnCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(117, 30);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(11, 10);
      this.btnOk.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(117, 30);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "O&K";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // TheTabControl
      // 
      this.TheTabControl.Controls.Add(this.tp1);
      this.TheTabControl.Controls.Add(this.tp2);
      this.TheTabControl.Controls.Add(this.tp3);
      this.TheTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTabControl.Location = new System.Drawing.Point(0, 0);
      this.TheTabControl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(764, 422);
      this.TheTabControl.TabIndex = 1;
      // 
      // tp1
      // 
      this.tp1.Controls.Add(this.tableLayoutPanel1);
      this.tp1.Location = new System.Drawing.Point(4, 25);
      this.tp1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.tp1.Name = "tp1";
      this.tp1.Size = new System.Drawing.Size(756, 393);
      this.tp1.TabIndex = 2;
      this.tp1.Text = "DateTimeBox";
      this.tp1.UseVisualStyleBackColor = true;
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.AutoSize = true;
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Controls.Add(this.label12, 0, 4);
      this.tableLayoutPanel1.Controls.Add(this.edDB5, 1, 4);
      this.tableLayoutPanel1.Controls.Add(this.label11, 0, 3);
      this.tableLayoutPanel1.Controls.Add(this.edDB4, 1, 3);
      this.tableLayoutPanel1.Controls.Add(this.label10, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.edDB3, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.label9, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.edDB2, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.edDB1, 1, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 5;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.Size = new System.Drawing.Size(756, 150);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // label12
      // 
      this.label12.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label12.AutoSize = true;
      this.label12.Location = new System.Drawing.Point(4, 120);
      this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label12.Name = "label12";
      this.label12.Size = new System.Drawing.Size(125, 30);
      this.label12.TabIndex = 8;
      this.label12.Text = "DateTime";
      this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDB5
      // 
      this.edDB5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.edDB5.Kind = FreeLibSet.Formatting.EditableDateTimeFormatterKind.DateTime;
      this.edDB5.Location = new System.Drawing.Point(137, 124);
      this.edDB5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.edDB5.Name = "edDB5";
      this.edDB5.Size = new System.Drawing.Size(275, 22);
      this.edDB5.TabIndex = 9;
      // 
      // label11
      // 
      this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label11.AutoSize = true;
      this.label11.Location = new System.Drawing.Point(4, 90);
      this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label11.Name = "label11";
      this.label11.Size = new System.Drawing.Size(125, 30);
      this.label11.TabIndex = 6;
      this.label11.Text = "ShortDateTime";
      this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDB4
      // 
      this.edDB4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.edDB4.Kind = FreeLibSet.Formatting.EditableDateTimeFormatterKind.ShortDateTime;
      this.edDB4.Location = new System.Drawing.Point(137, 94);
      this.edDB4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.edDB4.Name = "edDB4";
      this.edDB4.Size = new System.Drawing.Size(275, 22);
      this.edDB4.TabIndex = 7;
      // 
      // label10
      // 
      this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label10.AutoSize = true;
      this.label10.Location = new System.Drawing.Point(4, 60);
      this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label10.Name = "label10";
      this.label10.Size = new System.Drawing.Size(125, 30);
      this.label10.TabIndex = 4;
      this.label10.Text = "Time";
      this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDB3
      // 
      this.edDB3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.edDB3.Kind = FreeLibSet.Formatting.EditableDateTimeFormatterKind.Time;
      this.edDB3.Location = new System.Drawing.Point(137, 64);
      this.edDB3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.edDB3.Name = "edDB3";
      this.edDB3.Size = new System.Drawing.Size(160, 22);
      this.edDB3.TabIndex = 5;
      // 
      // label9
      // 
      this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label9.AutoSize = true;
      this.label9.Location = new System.Drawing.Point(4, 30);
      this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(125, 30);
      this.label9.TabIndex = 2;
      this.label9.Text = "ShortTime";
      this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDB2
      // 
      this.edDB2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.edDB2.Kind = FreeLibSet.Formatting.EditableDateTimeFormatterKind.ShortTime;
      this.edDB2.Location = new System.Drawing.Point(137, 34);
      this.edDB2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.edDB2.Name = "edDB2";
      this.edDB2.Size = new System.Drawing.Size(160, 22);
      this.edDB2.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(4, 0);
      this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(125, 30);
      this.label2.TabIndex = 0;
      this.label2.Text = "Date";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDB1
      // 
      this.edDB1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
      this.edDB1.Location = new System.Drawing.Point(137, 4);
      this.edDB1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.edDB1.Name = "edDB1";
      this.edDB1.Size = new System.Drawing.Size(160, 22);
      this.edDB1.TabIndex = 1;
      // 
      // tp2
      // 
      this.tp2.Controls.Add(this.cbDoRB);
      this.tp2.Controls.Add(this.edDEB);
      this.tp2.Controls.Add(this.label8);
      this.tp2.Controls.Add(this.edMDB);
      this.tp2.Controls.Add(this.label7);
      this.tp2.Controls.Add(this.edYMRB);
      this.tp2.Controls.Add(this.label6);
      this.tp2.Controls.Add(this.edYMB);
      this.tp2.Controls.Add(this.label5);
      this.tp2.Controls.Add(this.label4);
      this.tp2.Controls.Add(this.edDRB);
      this.tp2.Controls.Add(this.label3);
      this.tp2.Controls.Add(this.edDTP);
      this.tp2.Controls.Add(this.label1);
      this.tp2.Location = new System.Drawing.Point(4, 25);
      this.tp2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tp2.Name = "tp2";
      this.tp2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tp2.Size = new System.Drawing.Size(756, 393);
      this.tp2.TabIndex = 0;
      this.tp2.Text = "Other controls";
      this.tp2.UseVisualStyleBackColor = true;
      // 
      // cbDoRB
      // 
      this.cbDoRB.ClearButtonEnabled = false;
      this.cbDoRB.Culture = new System.Globalization.CultureInfo("ru-RU");
      this.cbDoRB.Location = new System.Drawing.Point(187, 138);
      this.cbDoRB.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.cbDoRB.Name = "cbDoRB";
      this.cbDoRB.Size = new System.Drawing.Size(200, 22);
      this.cbDoRB.TabIndex = 7;
      // 
      // edDEB
      // 
      this.edDEB.Format = "0.00";
      this.edDEB.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.edDEB.Location = new System.Drawing.Point(187, 284);
      this.edDEB.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.edDEB.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.edDEB.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
      this.edDEB.Name = "edDEB";
      this.edDEB.Size = new System.Drawing.Size(200, 22);
      this.edDEB.TabIndex = 15;
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(8, 284);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(133, 28);
      this.label8.TabIndex = 14;
      this.label8.Text = "DecimalEditBox";
      this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edMDB
      // 
      this.edMDB.Location = new System.Drawing.Point(187, 233);
      this.edMDB.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.edMDB.Name = "edMDB";
      this.edMDB.Size = new System.Drawing.Size(240, 25);
      this.edMDB.TabIndex = 13;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(8, 233);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(172, 22);
      this.label7.TabIndex = 12;
      this.label7.Text = "MonthDayBox";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edYMRB
      // 
      this.edYMRB.Location = new System.Drawing.Point(187, 199);
      this.edYMRB.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
      this.edYMRB.Name = "edYMRB";
      this.edYMRB.Size = new System.Drawing.Size(565, 24);
      this.edYMRB.TabIndex = 11;
      this.edYMRB.Year = 2021;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(8, 201);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(172, 22);
      this.label6.TabIndex = 10;
      this.label6.Text = "YearMonthRangeBox";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edYMB
      // 
      this.edYMB.Location = new System.Drawing.Point(187, 167);
      this.edYMB.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
      this.edYMB.Month = 2;
      this.edYMB.Name = "edYMB";
      this.edYMB.Size = new System.Drawing.Size(460, 24);
      this.edYMB.TabIndex = 9;
      this.edYMB.Year = 2021;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(8, 169);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(172, 22);
      this.label5.TabIndex = 8;
      this.label5.Text = "YearMonthBox";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(8, 138);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(172, 22);
      this.label4.TabIndex = 6;
      this.label4.Text = "DateOrRangeBox";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDRB
      // 
      this.edDRB.Location = new System.Drawing.Point(187, 86);
      this.edDRB.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
      this.edDRB.Name = "edDRB";
      this.edDRB.Size = new System.Drawing.Size(467, 45);
      this.edDRB.TabIndex = 5;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(8, 86);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(172, 22);
      this.label3.TabIndex = 4;
      this.label3.Text = "DateRangeBox";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edDTP
      // 
      this.edDTP.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      this.edDTP.Location = new System.Drawing.Point(187, 25);
      this.edDTP.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.edDTP.Name = "edDTP";
      this.edDTP.Size = new System.Drawing.Size(265, 22);
      this.edDTP.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 26);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(172, 22);
      this.label1.TabIndex = 0;
      this.label1.Text = "DateTimePicker";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // tp3
      // 
      this.tp3.Controls.Add(this.gr1);
      this.tp3.Location = new System.Drawing.Point(4, 25);
      this.tp3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tp3.Name = "tp3";
      this.tp3.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tp3.Size = new System.Drawing.Size(756, 393);
      this.tp3.TabIndex = 1;
      this.tp3.Text = "DataGridView";
      this.tp3.UseVisualStyleBackColor = true;
      // 
      // gr1
      // 
      this.gr1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.gr1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.gr1.Location = new System.Drawing.Point(3, 2);
      this.gr1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.gr1.Name = "gr1";
      this.gr1.RowTemplate.Height = 24;
      this.gr1.Size = new System.Drawing.Size(750, 389);
      this.gr1.TabIndex = 0;
      // 
      // CultureTestForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(764, 471);
      this.Controls.Add(this.TheTabControl);
      this.Controls.Add(this.panel1);
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.Name = "CultureTestForm";
      this.Text = "TestForm";
      this.panel1.ResumeLayout(false);
      this.TheTabControl.ResumeLayout(false);
      this.tp1.ResumeLayout(false);
      this.tp1.PerformLayout();
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.tp2.ResumeLayout(false);
      this.tp3.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.gr1)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.TabPage tp2;
    private FreeLibSet.Controls.UserMaskedComboBox cbDoRB;
    private FreeLibSet.Controls.DecimalEditBox edDEB;
    private FreeLibSet.Controls.MonthDayBox edMDB;
    private System.Windows.Forms.Label label7;
    private FreeLibSet.Controls.YearMonthRangeBox edYMRB;
    private System.Windows.Forms.Label label6;
    private FreeLibSet.Controls.YearMonthBox edYMB;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private FreeLibSet.Controls.DateRangeBox edDRB;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.DateTimePicker edDTP;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TabPage tp3;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.DataGridView gr1;
    private System.Windows.Forms.TabPage tp1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Label label12;
    private FreeLibSet.Controls.DateTimeBox edDB5;
    private System.Windows.Forms.Label label11;
    private FreeLibSet.Controls.DateTimeBox edDB4;
    private System.Windows.Forms.Label label10;
    private FreeLibSet.Controls.DateTimeBox edDB3;
    private System.Windows.Forms.Label label9;
    private FreeLibSet.Controls.DateTimeBox edDB2;
    private System.Windows.Forms.Label label2;
    private FreeLibSet.Controls.DateTimeBox edDB1;
    private System.Windows.Forms.Label label8;
  }
}
