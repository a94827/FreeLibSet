namespace FreeLibSet.Forms.Docs
{
  partial class EditDocTypePermissionForm
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
      this.grpMode = new FreeLibSet.Controls.RadioGroupBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label1 = new System.Windows.Forms.Label();
      this.panel1 = new System.Windows.Forms.Panel();
      this.rbOneType = new System.Windows.Forms.RadioButton();
      this.rbAllTypes = new System.Windows.Forms.RadioButton();
      this.cbDocType = new System.Windows.Forms.ComboBox();
      this.MainPanel.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.grpMode);
      this.MainPanel.Controls.Add(this.groupBox1);
      this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(321, 237);
      this.MainPanel.TabIndex = 1;
      // 
      // grpMode
      // 
      this.grpMode.AutoSize = true;
      this.grpMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.grpMode.Dock = System.Windows.Forms.DockStyle.Top;
      this.grpMode.Items = new string[0];
      this.grpMode.Location = new System.Drawing.Point(0, 143);
      this.grpMode.Name = "grpMode";
      this.grpMode.Size = new System.Drawing.Size(321, 35);
      this.grpMode.TabIndex = 1;
      this.grpMode.TabStop = false;
      this.grpMode.Text = "&Разрешение";
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.panel1);
      this.groupBox1.Controls.Add(this.cbDocType);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(321, 143);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Применить к видам документов";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 97);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(89, 13);
      this.label1.TabIndex = 1;
      this.label1.Text = "Вид &документов";
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.rbOneType);
      this.panel1.Controls.Add(this.rbAllTypes);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(3, 16);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(315, 65);
      this.panel1.TabIndex = 0;
      // 
      // rbOneType
      // 
      this.rbOneType.AutoSize = true;
      this.rbOneType.Location = new System.Drawing.Point(9, 36);
      this.rbOneType.Name = "rbOneType";
      this.rbOneType.Size = new System.Drawing.Size(168, 17);
      this.rbOneType.TabIndex = 1;
      this.rbOneType.TabStop = true;
      this.rbOneType.Text = "В&ыбранный вид документов";
      this.rbOneType.UseVisualStyleBackColor = true;
      // 
      // rbAllTypes
      // 
      this.rbAllTypes.AutoSize = true;
      this.rbAllTypes.Location = new System.Drawing.Point(9, 13);
      this.rbAllTypes.Name = "rbAllTypes";
      this.rbAllTypes.Size = new System.Drawing.Size(136, 17);
      this.rbAllTypes.TabIndex = 0;
      this.rbAllTypes.TabStop = true;
      this.rbAllTypes.Text = "&Все виды документов";
      this.rbAllTypes.UseVisualStyleBackColor = true;
      // 
      // cbDocType
      // 
      this.cbDocType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbDocType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbDocType.FormattingEnabled = true;
      this.cbDocType.Location = new System.Drawing.Point(12, 113);
      this.cbDocType.Name = "cbDocType";
      this.cbDocType.Size = new System.Drawing.Size(297, 21);
      this.cbDocType.TabIndex = 2;
      // 
      // EditDocTypePermissionForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(321, 237);
      this.Controls.Add(this.MainPanel);
      this.Name = "EditDocTypePermissionForm";
      this.MainPanel.ResumeLayout(false);
      this.MainPanel.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private FreeLibSet.Controls.RadioGroupBox grpMode;
    private System.Windows.Forms.ComboBox cbDocType;
    public System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.RadioButton rbOneType;
    private System.Windows.Forms.RadioButton rbAllTypes;
    private System.Windows.Forms.Label label1;

  }
}
