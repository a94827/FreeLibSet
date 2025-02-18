namespace FreeLibSet.Forms
{
  partial class WizardForm
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

    #region Windows TheForm Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardForm));
      this.panButtons = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnNext = new System.Windows.Forms.Button();
      this.btnBack = new System.Windows.Forms.Button();
      this.panImage = new System.Windows.Forms.Panel();
      this.pbImage = new System.Windows.Forms.PictureBox();
      this.panMain = new System.Windows.Forms.Panel();
      this.panButtons.SuspendLayout();
      this.panImage.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pbImage)).BeginInit();
      this.SuspendLayout();
      // 
      // panButtons
      // 
      resources.ApplyResources(this.panButtons, "panButtons");
      this.panButtons.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.panButtons.Controls.Add(this.btnCancel);
      this.panButtons.Controls.Add(this.btnNext);
      this.panButtons.Controls.Add(this.btnBack);
      this.panButtons.Name = "panButtons";
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnNext
      // 
      resources.ApplyResources(this.btnNext, "btnNext");
      this.btnNext.Name = "btnNext";
      this.btnNext.UseVisualStyleBackColor = true;
      // 
      // btnBack
      // 
      resources.ApplyResources(this.btnBack, "btnBack");
      this.btnBack.Name = "btnBack";
      this.btnBack.UseVisualStyleBackColor = true;
      // 
      // panImage
      // 
      resources.ApplyResources(this.panImage, "panImage");
      this.panImage.BackColor = System.Drawing.Color.DarkOliveGreen;
      this.panImage.Controls.Add(this.pbImage);
      this.panImage.Name = "panImage";
      // 
      // pbImage
      // 
      resources.ApplyResources(this.pbImage, "pbImage");
      this.pbImage.Name = "pbImage";
      this.pbImage.TabStop = false;
      // 
      // panMain
      // 
      resources.ApplyResources(this.panMain, "panMain");
      this.panMain.Name = "panMain";
      // 
      // WizardForm
      // 
      this.AcceptButton = this.btnNext;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.panMain);
      this.Controls.Add(this.panImage);
      this.Controls.Add(this.panButtons);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "WizardForm";
      this.ShowIcon = false;
      this.panButtons.ResumeLayout(false);
      this.panImage.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pbImage)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panButtons;
    private System.Windows.Forms.Panel panImage;
    private System.Windows.Forms.Panel panMain;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnNext;
    private System.Windows.Forms.Button btnBack;
    private System.Windows.Forms.PictureBox pbImage;
  }
}
