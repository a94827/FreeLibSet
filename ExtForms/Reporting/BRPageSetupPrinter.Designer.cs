namespace FreeLibSet.Forms.Reporting
{
  partial class BRPageSetupPrinter
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
      this.panPrinter = new System.Windows.Forms.Panel();
      this.lblBackInfo = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.btnDebug = new System.Windows.Forms.Button();
      this.lblPrinterInfo = new System.Windows.Forms.Label();
      this.btnPrinterProps = new System.Windows.Forms.Button();
      this.grpPrinter = new System.Windows.Forms.GroupBox();
      this.panel3 = new System.Windows.Forms.Panel();
      this.rbSelPrinter = new System.Windows.Forms.RadioButton();
      this.rbDefaultPrinter = new System.Windows.Forms.RadioButton();
      this.cbPrinter = new System.Windows.Forms.ComboBox();
      this.panPrinter.SuspendLayout();
      this.grpPrinter.SuspendLayout();
      this.panel3.SuspendLayout();
      this.SuspendLayout();
      // 
      // panPrinter
      // 
      this.panPrinter.Controls.Add(this.lblBackInfo);
      this.panPrinter.Controls.Add(this.label9);
      this.panPrinter.Controls.Add(this.btnDebug);
      this.panPrinter.Controls.Add(this.lblPrinterInfo);
      this.panPrinter.Controls.Add(this.btnPrinterProps);
      this.panPrinter.Controls.Add(this.grpPrinter);
      this.panPrinter.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panPrinter.Location = new System.Drawing.Point(0, 0);
      this.panPrinter.Name = "panPrinter";
      this.panPrinter.Size = new System.Drawing.Size(445, 274);
      this.panPrinter.TabIndex = 1;
      // 
      // lblBackInfo
      // 
      this.lblBackInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblBackInfo.Location = new System.Drawing.Point(197, 186);
      this.lblBackInfo.Name = "lblBackInfo";
      this.lblBackInfo.Size = new System.Drawing.Size(242, 25);
      this.lblBackInfo.TabIndex = 14;
      this.lblBackInfo.Text = "???";
      this.lblBackInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(5, 186);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(186, 25);
      this.label9.TabIndex = 13;
      this.label9.Text = "Фоновая печать документа:";
      this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnDebug
      // 
      this.btnDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDebug.Location = new System.Drawing.Point(351, 245);
      this.btnDebug.Name = "btnDebug";
      this.btnDebug.Size = new System.Drawing.Size(88, 24);
      this.btnDebug.TabIndex = 12;
      this.btnDebug.Text = "Отладка";
      this.btnDebug.UseVisualStyleBackColor = true;
      // 
      // lblPrinterInfo
      // 
      this.lblPrinterInfo.Location = new System.Drawing.Point(11, 145);
      this.lblPrinterInfo.Name = "lblPrinterInfo";
      this.lblPrinterInfo.Size = new System.Drawing.Size(361, 23);
      this.lblPrinterInfo.TabIndex = 11;
      this.lblPrinterInfo.Text = "???";
      this.lblPrinterInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // btnPrinterProps
      // 
      this.btnPrinterProps.Location = new System.Drawing.Point(5, 109);
      this.btnPrinterProps.Name = "btnPrinterProps";
      this.btnPrinterProps.Size = new System.Drawing.Size(176, 24);
      this.btnPrinterProps.TabIndex = 10;
      this.btnPrinterProps.Text = "Свойства принтера";
      this.btnPrinterProps.UseVisualStyleBackColor = true;
      // 
      // grpPrinter
      // 
      this.grpPrinter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpPrinter.Controls.Add(this.panel3);
      this.grpPrinter.Controls.Add(this.cbPrinter);
      this.grpPrinter.Location = new System.Drawing.Point(5, 3);
      this.grpPrinter.Name = "grpPrinter";
      this.grpPrinter.Size = new System.Drawing.Size(434, 100);
      this.grpPrinter.TabIndex = 9;
      this.grpPrinter.TabStop = false;
      this.grpPrinter.Text = "Принтер";
      // 
      // panel3
      // 
      this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.panel3.Controls.Add(this.rbSelPrinter);
      this.panel3.Controls.Add(this.rbDefaultPrinter);
      this.panel3.Location = new System.Drawing.Point(6, 13);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(418, 49);
      this.panel3.TabIndex = 0;
      // 
      // rbSelPrinter
      // 
      this.rbSelPrinter.AutoSize = true;
      this.rbSelPrinter.Location = new System.Drawing.Point(13, 29);
      this.rbSelPrinter.Name = "rbSelPrinter";
      this.rbSelPrinter.Size = new System.Drawing.Size(113, 17);
      this.rbSelPrinter.TabIndex = 1;
      this.rbSelPrinter.TabStop = true;
      this.rbSelPrinter.Text = "Выбрать принтер";
      this.rbSelPrinter.UseVisualStyleBackColor = true;
      // 
      // rbDefaultPrinter
      // 
      this.rbDefaultPrinter.AutoSize = true;
      this.rbDefaultPrinter.Location = new System.Drawing.Point(13, 6);
      this.rbDefaultPrinter.Name = "rbDefaultPrinter";
      this.rbDefaultPrinter.Size = new System.Drawing.Size(216, 17);
      this.rbDefaultPrinter.TabIndex = 0;
      this.rbDefaultPrinter.TabStop = true;
      this.rbDefaultPrinter.Text = "Использовать принтер по умолчанию";
      this.rbDefaultPrinter.UseVisualStyleBackColor = true;
      // 
      // cbPrinter
      // 
      this.cbPrinter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbPrinter.FormattingEnabled = true;
      this.cbPrinter.Location = new System.Drawing.Point(40, 68);
      this.cbPrinter.Name = "cbPrinter";
      this.cbPrinter.Size = new System.Drawing.Size(384, 21);
      this.cbPrinter.TabIndex = 1;
      // 
      // BRPageSetupPrinter
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(445, 274);
      this.Controls.Add(this.panPrinter);
      this.Name = "BRPageSetupPrinter";
      this.Text = "BRPageSetupPrinter";
      this.panPrinter.ResumeLayout(false);
      this.grpPrinter.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.panel3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panPrinter;
    private System.Windows.Forms.Label lblBackInfo;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.Button btnDebug;
    private System.Windows.Forms.Label lblPrinterInfo;
    private System.Windows.Forms.Button btnPrinterProps;
    private System.Windows.Forms.GroupBox grpPrinter;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.RadioButton rbSelPrinter;
    private System.Windows.Forms.RadioButton rbDefaultPrinter;
    private System.Windows.Forms.ComboBox cbPrinter;
  }
}
