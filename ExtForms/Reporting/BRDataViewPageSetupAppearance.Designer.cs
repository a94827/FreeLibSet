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
      this.groupBox2 = new System.Windows.Forms.GroupBox();
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
      this.grpTitle = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.rbTitleManual = new System.Windows.Forms.RadioButton();
      this.rbTitleAuto = new System.Windows.Forms.RadioButton();
      this.edTitleText = new System.Windows.Forms.TextBox();
      this.MainPanel.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.grpTitle.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.grpTitle);
      this.MainPanel.Controls.Add(this.groupBox2);
      this.MainPanel.Controls.Add(this.groupBox1);
      this.MainPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(544, 417);
      this.MainPanel.TabIndex = 7;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.cbIgnoreWith);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox2.Location = new System.Drawing.Point(0, 207);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
      this.groupBox2.Size = new System.Drawing.Size(544, 53);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Разбиение на страницы";
      this.groupBox2.Visible = false;
      // 
      // cbIgnoreWith
      // 
      this.cbIgnoreWith.AutoSize = true;
      this.cbIgnoreWith.Location = new System.Drawing.Point(12, 23);
      this.cbIgnoreWith.Margin = new System.Windows.Forms.Padding(4);
      this.cbIgnoreWith.Name = "cbIgnoreWith";
      this.cbIgnoreWith.Size = new System.Drawing.Size(427, 21);
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
      this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
      this.groupBox1.Size = new System.Drawing.Size(544, 207);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Внешний вид таблицы";
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel1.ColumnCount = 5;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 39F));
      this.tableLayoutPanel1.Controls.Add(this.edTextFalse, 3, 0);
      this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.label5, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.edTextTrue, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.btnSelText, 4, 0);
      this.tableLayoutPanel1.Location = new System.Drawing.Point(199, 110);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 1;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(329, 30);
      this.tableLayoutPanel1.TabIndex = 6;
      // 
      // edTextFalse
      // 
      this.edTextFalse.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edTextFalse.Location = new System.Drawing.Point(208, 3);
      this.edTextFalse.Name = "edTextFalse";
      this.edTextFalse.Size = new System.Drawing.Size(79, 22);
      this.edTextFalse.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.Location = new System.Drawing.Point(3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(54, 30);
      this.label2.TabIndex = 0;
      this.label2.Text = "Вкл.";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label5
      // 
      this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label5.Location = new System.Drawing.Point(148, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(54, 30);
      this.label5.TabIndex = 2;
      this.label5.Text = "Выкл.";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // edTextTrue
      // 
      this.edTextTrue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edTextTrue.Location = new System.Drawing.Point(63, 3);
      this.edTextTrue.Name = "edTextTrue";
      this.edTextTrue.Size = new System.Drawing.Size(79, 22);
      this.edTextTrue.TabIndex = 1;
      // 
      // btnSelText
      // 
      this.btnSelText.Location = new System.Drawing.Point(293, 3);
      this.btnSelText.Name = "btnSelText";
      this.btnSelText.Size = new System.Drawing.Size(33, 23);
      this.btnSelText.TabIndex = 4;
      this.btnSelText.UseVisualStyleBackColor = true;
      // 
      // lblCellParams
      // 
      this.lblCellParams.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblCellParams.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblCellParams.Location = new System.Drawing.Point(199, 143);
      this.lblCellParams.Name = "lblCellParams";
      this.lblCellParams.Size = new System.Drawing.Size(329, 54);
      this.lblCellParams.TabIndex = 8;
      this.lblCellParams.Text = "???";
      this.lblCellParams.UseMnemonic = false;
      // 
      // btnCellParams
      // 
      this.btnCellParams.Location = new System.Drawing.Point(7, 143);
      this.btnCellParams.Name = "btnCellParams";
      this.btnCellParams.Size = new System.Drawing.Size(176, 24);
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
      this.cbBoolMode.Location = new System.Drawing.Point(199, 79);
      this.cbBoolMode.Margin = new System.Windows.Forms.Padding(4);
      this.cbBoolMode.Name = "cbBoolMode";
      this.cbBoolMode.Size = new System.Drawing.Size(329, 24);
      this.cbBoolMode.TabIndex = 5;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 79);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(183, 24);
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
      this.cbColorStyle.Location = new System.Drawing.Point(199, 47);
      this.cbColorStyle.Margin = new System.Windows.Forms.Padding(4);
      this.cbColorStyle.Name = "cbColorStyle";
      this.cbColorStyle.Size = new System.Drawing.Size(329, 24);
      this.cbColorStyle.TabIndex = 3;
      // 
      // label4
      // 
      this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
      this.label4.Location = new System.Drawing.Point(8, 47);
      this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(183, 26);
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
      this.cbBorderStyle.Location = new System.Drawing.Point(199, 16);
      this.cbBorderStyle.Margin = new System.Windows.Forms.Padding(4);
      this.cbBorderStyle.Name = "cbBorderStyle";
      this.cbBorderStyle.Size = new System.Drawing.Size(329, 24);
      this.cbBorderStyle.TabIndex = 1;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(8, 16);
      this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(183, 26);
      this.label3.TabIndex = 0;
      this.label3.Text = "&Рамки";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // grpTitle
      // 
      this.grpTitle.Controls.Add(this.tableLayoutPanel2);
      this.grpTitle.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grpTitle.Location = new System.Drawing.Point(0, 260);
      this.grpTitle.Name = "grpTitle";
      this.grpTitle.Size = new System.Drawing.Size(544, 157);
      this.grpTitle.TabIndex = 2;
      this.grpTitle.TabStop = false;
      this.grpTitle.Text = "Заголовок";
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 2;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel2.Controls.Add(this.rbTitleManual, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.rbTitleAuto, 1, 0);
      this.tableLayoutPanel2.Controls.Add(this.edTitleText, 0, 1);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 18);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 2;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(538, 136);
      this.tableLayoutPanel2.TabIndex = 0;
      // 
      // rbTitleManual
      // 
      this.rbTitleManual.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.rbTitleManual.Location = new System.Drawing.Point(3, 3);
      this.rbTitleManual.Name = "rbTitleManual";
      this.rbTitleManual.Size = new System.Drawing.Size(263, 24);
      this.rbTitleManual.TabIndex = 0;
      this.rbTitleManual.TabStop = true;
      this.rbTitleManual.Text = "Задать текст";
      this.rbTitleManual.UseVisualStyleBackColor = true;
      // 
      // rbTitleAuto
      // 
      this.rbTitleAuto.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.rbTitleAuto.Location = new System.Drawing.Point(272, 3);
      this.rbTitleAuto.Name = "rbTitleAuto";
      this.rbTitleAuto.Size = new System.Drawing.Size(263, 24);
      this.rbTitleAuto.TabIndex = 1;
      this.rbTitleAuto.TabStop = true;
      this.rbTitleAuto.Text = "Автоматически";
      this.rbTitleAuto.UseVisualStyleBackColor = true;
      // 
      // edTitleText
      // 
      this.edTitleText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel2.SetColumnSpan(this.edTitleText, 2);
      this.edTitleText.Location = new System.Drawing.Point(3, 33);
      this.edTitleText.Multiline = true;
      this.edTitleText.Name = "edTitleText";
      this.edTitleText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.edTitleText.Size = new System.Drawing.Size(532, 100);
      this.edTitleText.TabIndex = 2;
      this.edTitleText.WordWrap = false;
      // 
      // BRDataViewPageSetupAppearance
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(544, 429);
      this.Controls.Add(this.MainPanel);
      this.Name = "BRDataViewPageSetupAppearance";
      this.Text = "BRDataViewPageSetupAppearance";
      this.MainPanel.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.grpTitle.ResumeLayout(false);
      this.tableLayoutPanel2.ResumeLayout(false);
      this.tableLayoutPanel2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.GroupBox groupBox2;
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
    private System.Windows.Forms.GroupBox grpTitle;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private System.Windows.Forms.RadioButton rbTitleManual;
    private System.Windows.Forms.RadioButton rbTitleAuto;
    private System.Windows.Forms.TextBox edTitleText;
  }
}
