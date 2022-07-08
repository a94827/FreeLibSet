namespace FIASDemo
{
  partial class ConvertGuidForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConvertGuidForm));
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.rbGuid2Bin = new System.Windows.Forms.RadioButton();
      this.rbBin2Guid = new System.Windows.Forms.RadioButton();
      this.label1 = new System.Windows.Forms.Label();
      this.edGuid = new System.Windows.Forms.MaskedTextBox();
      this.edBin = new System.Windows.Forms.MaskedTextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.infoLabel1 = new AgeyevAV.ExtForms.InfoLabel();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.rbBin2Guid);
      this.groupBox1.Controls.Add(this.rbGuid2Bin);
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(130, 82);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Направление";
      // 
      // rbGuid2Bin
      // 
      this.rbGuid2Bin.AutoSize = true;
      this.rbGuid2Bin.Location = new System.Drawing.Point(22, 26);
      this.rbGuid2Bin.Name = "rbGuid2Bin";
      this.rbGuid2Bin.Size = new System.Drawing.Size(82, 17);
      this.rbGuid2Bin.TabIndex = 0;
      this.rbGuid2Bin.TabStop = true;
      this.rbGuid2Bin.Text = "GUID -> Bin";
      this.rbGuid2Bin.UseVisualStyleBackColor = true;
      // 
      // rbBin2Guid
      // 
      this.rbBin2Guid.AutoSize = true;
      this.rbBin2Guid.Location = new System.Drawing.Point(22, 53);
      this.rbBin2Guid.Name = "rbBin2Guid";
      this.rbBin2Guid.Size = new System.Drawing.Size(82, 17);
      this.rbBin2Guid.TabIndex = 1;
      this.rbBin2Guid.TabStop = true;
      this.rbBin2Guid.Text = "Bin -> GUID";
      this.rbBin2Guid.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(148, 37);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(46, 20);
      this.label1.TabIndex = 1;
      this.label1.Text = "GUID";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edGuid
      // 
      this.edGuid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edGuid.Location = new System.Drawing.Point(200, 37);
      this.edGuid.Name = "edGuid";
      this.edGuid.Size = new System.Drawing.Size(409, 20);
      this.edGuid.TabIndex = 2;
      // 
      // edBin
      // 
      this.edBin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edBin.Location = new System.Drawing.Point(200, 63);
      this.edBin.Name = "edBin";
      this.edBin.Size = new System.Drawing.Size(409, 20);
      this.edBin.TabIndex = 4;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(148, 63);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(46, 20);
      this.label2.TabIndex = 3;
      this.label2.Text = "Bin";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // infoLabel1
      // 
      this.infoLabel1.Location = new System.Drawing.Point(12, 100);
      this.infoLabel1.Name = "infoLabel1";
      this.infoLabel1.Size = new System.Drawing.Size(597, 84);
      this.infoLabel1.TabIndex = 5;
      this.infoLabel1.Text = resources.GetString("infoLabel1.Text");
      // 
      // ConvertGuidForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(621, 196);
      this.Controls.Add(this.infoLabel1);
      this.Controls.Add(this.edBin);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.edGuid);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.groupBox1);
      this.Name = "ConvertGuidForm";
      this.Text = "Преобразование GUID";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RadioButton rbBin2Guid;
    private System.Windows.Forms.RadioButton rbGuid2Bin;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.MaskedTextBox edGuid;
    private System.Windows.Forms.MaskedTextBox edBin;
    private System.Windows.Forms.Label label2;
    private AgeyevAV.ExtForms.InfoLabel infoLabel1;
  }
}