namespace FreeLibSet.Forms
{
  partial class EFPDataViewCopyFormatsForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EFPDataViewCopyFormatsForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.infoLabel1 = new FreeLibSet.Controls.InfoLabel();
      this.cbHtml = new System.Windows.Forms.CheckBox();
      this.cbCsv = new System.Windows.Forms.CheckBox();
      this.cbText = new System.Windows.Forms.CheckBox();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Name = "panel1";
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Name = "btnOk";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      resources.ApplyResources(this.groupBox1, "groupBox1");
      this.groupBox1.Controls.Add(this.infoLabel1);
      this.groupBox1.Controls.Add(this.cbHtml);
      this.groupBox1.Controls.Add(this.cbCsv);
      this.groupBox1.Controls.Add(this.cbText);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.TabStop = false;
      // 
      // infoLabel1
      // 
      resources.ApplyResources(this.infoLabel1, "infoLabel1");
      this.infoLabel1.Name = "infoLabel1";
      // 
      // cbHtml
      // 
      resources.ApplyResources(this.cbHtml, "cbHtml");
      this.cbHtml.Name = "cbHtml";
      this.cbHtml.UseVisualStyleBackColor = true;
      // 
      // cbCsv
      // 
      resources.ApplyResources(this.cbCsv, "cbCsv");
      this.cbCsv.Name = "cbCsv";
      this.cbCsv.UseVisualStyleBackColor = true;
      // 
      // cbText
      // 
      resources.ApplyResources(this.cbText, "cbText");
      this.cbText.Name = "cbText";
      this.cbText.UseVisualStyleBackColor = true;
      // 
      // EFPDataViewCopyFormatsForm
      // 
      this.AcceptButton = this.btnOk;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "EFPDataViewCopyFormatsForm";
      this.panel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.GroupBox groupBox1;
    private Controls.InfoLabel infoLabel1;
    private System.Windows.Forms.CheckBox cbHtml;
    private System.Windows.Forms.CheckBox cbCsv;
    private System.Windows.Forms.CheckBox cbText;
  }
}