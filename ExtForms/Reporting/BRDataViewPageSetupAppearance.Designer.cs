namespace FreeLibSet.Forms.Reporting
{
  partial class BRDataViewPageSetupAppearance
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
      this.grpPageBreak = new System.Windows.Forms.GroupBox();
      this.cbIgnoreWith = new System.Windows.Forms.CheckBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.edTextFalse = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.edTextTrue = new System.Windows.Forms.TextBox();
      this.btnSelText = new System.Windows.Forms.Button();
      this.lblCellParams = new System.Windows.Forms.Label();
      this.btnCellParams = new System.Windows.Forms.Button();
      this.cbBoolMode = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cbColorStyle = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this.cbBorderStyle = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.MainPanel.SuspendLayout();
      this.grpPageBreak.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.grpPageBreak);
      this.MainPanel.Controls.Add(this.groupBox1);
      this.MainPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Margin = new System.Windows.Forms.Padding(5);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(525, 291);
      this.MainPanel.TabIndex = 7;
      // 
      // grpPageBreak
      // 
      this.grpPageBreak.Controls.Add(this.cbIgnoreWith);
      this.grpPageBreak.Dock = System.Windows.Forms.DockStyle.Top;
      this.grpPageBreak.Location = new System.Drawing.Point(0, 240);
      this.grpPageBreak.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
      this.grpPageBreak.Name = "grpPageBreak";
      this.grpPageBreak.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
      this.grpPageBreak.Size = new System.Drawing.Size(525, 43);
      this.grpPageBreak.TabIndex = 1;
      this.grpPageBreak.TabStop = false;
      this.grpPageBreak.Text = "Разбиение на страницы";
      this.grpPageBreak.Visible = false;
      // 
      // cbIgnoreWith
      // 
      this.cbIgnoreWith.AutoSize = true;
      this.cbIgnoreWith.Location = new System.Drawing.Point(10, 19);
      this.cbIgnoreWith.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
      this.cbIgnoreWith.Name = "cbIgnoreWith";
      this.cbIgnoreWith.Size = new System.Drawing.Size(336, 17);
      this.cbIgnoreWith.TabIndex = 0;
      this.cbIgnoreWith.Text = "Игнорировать запрет отрыва строк заголовков и под&ытогов";
      this.cbIgnoreWith.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.tableLayoutPanel1);
      this.groupBox1.Controls.Add(this.lblCellParams);
      this.groupBox1.Controls.Add(this.btnCellParams);
      this.groupBox1.Controls.Add(this.cbBoolMode);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.cbColorStyle);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.cbBorderStyle);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
      this.groupBox1.Size = new System.Drawing.Size(525, 240);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Внешний вид таблицы";
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel1.ColumnCount = 5;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
      this.tableLayoutPanel1.Controls.Add(this.edTextFalse, 3, 0);
      this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.label5, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.edTextTrue, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.btnSelText, 4, 0);
      this.tableLayoutPanel1.Location = new System.Drawing.Point(235, 123);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 1;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(276, 31);
      this.tableLayoutPanel1.TabIndex = 6;
      // 
      // edTextFalse
      // 
      this.edTextFalse.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edTextFalse.Location = new System.Drawing.Point(175, 5);
      this.edTextFalse.Margin = new System.Windows.Forms.Padding(5);
      this.edTextFalse.Name = "edTextFalse";
      this.edTextFalse.Size = new System.Drawing.Size(60, 20);
      this.edTextFalse.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.Location = new System.Drawing.Point(5, 0);
      this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(40, 31);
      this.label2.TabIndex = 0;
      this.label2.Text = "Вкл.";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label5
      // 
      this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label5.Location = new System.Drawing.Point(125, 0);
      this.label5.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(40, 31);
      this.label5.TabIndex = 2;
      this.label5.Text = "Выкл.";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // edTextTrue
      // 
      this.edTextTrue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edTextTrue.Location = new System.Drawing.Point(55, 5);
      this.edTextTrue.Margin = new System.Windows.Forms.Padding(5);
      this.edTextTrue.Name = "edTextTrue";
      this.edTextTrue.Size = new System.Drawing.Size(60, 20);
      this.edTextTrue.TabIndex = 1;
      // 
      // btnSelText
      // 
      this.btnSelText.Location = new System.Drawing.Point(245, 5);
      this.btnSelText.Margin = new System.Windows.Forms.Padding(5);
      this.btnSelText.Name = "btnSelText";
      this.btnSelText.Size = new System.Drawing.Size(26, 21);
      this.btnSelText.TabIndex = 4;
      this.btnSelText.UseVisualStyleBackColor = true;
      // 
      // lblCellParams
      // 
      this.lblCellParams.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblCellParams.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblCellParams.Location = new System.Drawing.Point(235, 165);
      this.lblCellParams.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
      this.lblCellParams.Name = "lblCellParams";
      this.lblCellParams.Size = new System.Drawing.Size(280, 67);
      this.lblCellParams.TabIndex = 8;
      this.lblCellParams.Text = "???";
      this.lblCellParams.UseMnemonic = false;
      // 
      // btnCellParams
      // 
      this.btnCellParams.Location = new System.Drawing.Point(10, 165);
      this.btnCellParams.Margin = new System.Windows.Forms.Padding(5);
      this.btnCellParams.Name = "btnCellParams";
      this.btnCellParams.Size = new System.Drawing.Size(147, 24);
      this.btnCellParams.TabIndex = 7;
      this.btnCellParams.Text = "Параметры &ячеек";
      this.btnCellParams.UseVisualStyleBackColor = true;
      // 
      // cbBoolMode
      // 
      this.cbBoolMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbBoolMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbBoolMode.FormattingEnabled = true;
      this.cbBoolMode.Items.AddRange(new object[] {
            "Не заменять (ИСТИНА, ЛОЖЬ)",
            "Как число (1, 0)",
            "Как текст"});
      this.cbBoolMode.Location = new System.Drawing.Point(235, 91);
      this.cbBoolMode.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
      this.cbBoolMode.Name = "cbBoolMode";
      this.cbBoolMode.Size = new System.Drawing.Size(276, 21);
      this.cbBoolMode.TabIndex = 5;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(7, 91);
      this.label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(140, 21);
      this.label1.TabIndex = 4;
      this.label1.Text = "&Логические значения";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbColorStyle
      // 
      this.cbColorStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbColorStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbColorStyle.FormattingEnabled = true;
      this.cbColorStyle.Items.AddRange(new object[] {
            "Не используются",
            "Как на экране",
            "Серая шкала"});
      this.cbColorStyle.Location = new System.Drawing.Point(235, 58);
      this.cbColorStyle.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
      this.cbColorStyle.Name = "cbColorStyle";
      this.cbColorStyle.Size = new System.Drawing.Size(276, 21);
      this.cbColorStyle.TabIndex = 3;
      // 
      // label4
      // 
      this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
      this.label4.Location = new System.Drawing.Point(7, 57);
      this.label4.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(136, 21);
      this.label4.TabIndex = 2;
      this.label4.Text = "&Цвета";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbBorderStyle
      // 
      this.cbBorderStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbBorderStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbBorderStyle.FormattingEnabled = true;
      this.cbBorderStyle.Items.AddRange(new object[] {
            "Без границ",
            "Рамка только вокруг заголовка",
            "Вертикальные линии",
            "Сетка"});
      this.cbBorderStyle.Location = new System.Drawing.Point(235, 25);
      this.cbBorderStyle.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
      this.cbBorderStyle.Name = "cbBorderStyle";
      this.cbBorderStyle.Size = new System.Drawing.Size(276, 21);
      this.cbBorderStyle.TabIndex = 1;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(7, 25);
      this.label3.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(137, 21);
      this.label3.TabIndex = 0;
      this.label3.Text = "&Рамки";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // BRDataViewPageSetupAppearance
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(525, 297);
      this.Controls.Add(this.MainPanel);
      this.Margin = new System.Windows.Forms.Padding(5);
      this.Name = "BRDataViewPageSetupAppearance";
      this.Text = "BRDataViewPageSetupAppearance";
      this.MainPanel.ResumeLayout(false);
      this.grpPageBreak.ResumeLayout(false);
      this.grpPageBreak.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.GroupBox grpPageBreak;
    private System.Windows.Forms.CheckBox cbIgnoreWith;
    public System.Windows.Forms.GroupBox groupBox1;
    public System.Windows.Forms.ComboBox cbColorStyle;
    public System.Windows.Forms.Label label4;
    public System.Windows.Forms.ComboBox cbBorderStyle;
    public System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox cbBoolMode;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnCellParams;
    private System.Windows.Forms.Label lblCellParams;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.TextBox edTextFalse;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.TextBox edTextTrue;
    private System.Windows.Forms.Button btnSelText;
  }
}
