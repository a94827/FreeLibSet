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
      this.cbBoolMode = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.cbColorStyle = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this.cbBorderStyle = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.btnCellParams = new System.Windows.Forms.Button();
      this.lblCellParams = new System.Windows.Forms.Label();
      this.MainPanel.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.groupBox2);
      this.MainPanel.Controls.Add(this.groupBox1);
      this.MainPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(544, 248);
      this.MainPanel.TabIndex = 7;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.cbIgnoreWith);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox2.Location = new System.Drawing.Point(0, 172);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
      this.groupBox2.Size = new System.Drawing.Size(544, 54);
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
      this.groupBox1.Size = new System.Drawing.Size(544, 172);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Внешний вид";
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
            "Как текст ([X], [ ])"});
      this.cbBoolMode.Location = new System.Drawing.Point(199, 79);
      this.cbBoolMode.Margin = new System.Windows.Forms.Padding(4);
      this.cbBoolMode.Name = "cbBoolMode";
      this.cbBoolMode.Size = new System.Drawing.Size(329, 24);
      this.cbBoolMode.TabIndex = 7;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 79);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(183, 24);
      this.label1.TabIndex = 6;
      this.label1.Text = "Логические значения";
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
      // btnCellParams
      // 
      this.btnCellParams.Location = new System.Drawing.Point(7, 111);
      this.btnCellParams.Name = "btnCellParams";
      this.btnCellParams.Size = new System.Drawing.Size(176, 24);
      this.btnCellParams.TabIndex = 8;
      this.btnCellParams.Text = "Параметры ячеек";
      this.btnCellParams.UseVisualStyleBackColor = true;
      // 
      // lblCellParams
      // 
      this.lblCellParams.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblCellParams.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblCellParams.Location = new System.Drawing.Point(199, 111);
      this.lblCellParams.Name = "lblCellParams";
      this.lblCellParams.Size = new System.Drawing.Size(329, 54);
      this.lblCellParams.TabIndex = 9;
      this.lblCellParams.Text = "???";
      this.lblCellParams.UseMnemonic = false;
      // 
      // BRDataViewPageSetupAppearance
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(544, 333);
      this.Controls.Add(this.MainPanel);
      this.Name = "BRDataViewPageSetupAppearance";
      this.Text = "BRDataViewPageSetupAppearance";
      this.MainPanel.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
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
  }
}