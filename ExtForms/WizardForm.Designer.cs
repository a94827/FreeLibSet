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
      this.panButtons.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.panButtons.Controls.Add(this.btnCancel);
      this.panButtons.Controls.Add(this.btnNext);
      this.panButtons.Controls.Add(this.btnBack);
      this.panButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panButtons.Location = new System.Drawing.Point(0, 288);
      this.panButtons.Name = "panButtons";
      this.panButtons.Size = new System.Drawing.Size(498, 40);
      this.panButtons.TabIndex = 2;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.Location = new System.Drawing.Point(396, 7);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnNext
      // 
      this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnNext.Location = new System.Drawing.Point(291, 7);
      this.btnNext.Name = "btnNext";
      this.btnNext.Size = new System.Drawing.Size(88, 24);
      this.btnNext.TabIndex = 1;
      this.btnNext.Text = "&Далее";
      this.btnNext.UseVisualStyleBackColor = true;
      // 
      // btnBack
      // 
      this.btnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnBack.Location = new System.Drawing.Point(197, 7);
      this.btnBack.Name = "btnBack";
      this.btnBack.Size = new System.Drawing.Size(88, 24);
      this.btnBack.TabIndex = 0;
      this.btnBack.Text = "&Назад";
      this.btnBack.UseVisualStyleBackColor = true;
      // 
      // panImage
      // 
      this.panImage.BackColor = System.Drawing.Color.DarkOliveGreen;
      this.panImage.Controls.Add(this.pbImage);
      this.panImage.Dock = System.Windows.Forms.DockStyle.Left;
      this.panImage.Location = new System.Drawing.Point(0, 0);
      this.panImage.Name = "panImage";
      this.panImage.Size = new System.Drawing.Size(171, 288);
      this.panImage.TabIndex = 0;
      // 
      // pbImage
      // 
      this.pbImage.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pbImage.Location = new System.Drawing.Point(0, 0);
      this.pbImage.Name = "pbImage";
      this.pbImage.Size = new System.Drawing.Size(171, 288);
      this.pbImage.TabIndex = 0;
      this.pbImage.TabStop = false;
      // 
      // panMain
      // 
      this.panMain.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panMain.Location = new System.Drawing.Point(171, 0);
      this.panMain.Name = "panMain";
      this.panMain.Size = new System.Drawing.Size(327, 288);
      this.panMain.TabIndex = 1;
      // 
      // WizardForm
      // 
      this.AcceptButton = this.btnNext;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(498, 328);
      this.Controls.Add(this.panMain);
      this.Controls.Add(this.panImage);
      this.Controls.Add(this.panButtons);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "WizardForm";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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