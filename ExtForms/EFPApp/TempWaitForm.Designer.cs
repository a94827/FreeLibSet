namespace FreeLibSet.Forms
{
  partial class TempWaitForm
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
      this.TheImg = new System.Windows.Forms.PictureBox();
      this.TheLabel = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.TheImg)).BeginInit();
      this.SuspendLayout();
      // 
      // TheImg
      // 
      this.TheImg.Dock = System.Windows.Forms.DockStyle.Left;
      this.TheImg.Location = new System.Drawing.Point(0, 0);
      this.TheImg.Name = "TheImg";
      this.TheImg.Size = new System.Drawing.Size(20, 22);
      this.TheImg.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.TheImg.TabIndex = 9;
      this.TheImg.TabStop = false;
      // 
      // TheLabel
      // 
      this.TheLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheLabel.Location = new System.Drawing.Point(20, 0);
      this.TheLabel.Name = "TheLabel";
      this.TheLabel.Size = new System.Drawing.Size(491, 22);
      this.TheLabel.TabIndex = 10;
      this.TheLabel.Text = "???";
      this.TheLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.TheLabel.UseMnemonic = false;
      // 
      // TempWaitForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(511, 22);
      this.Controls.Add(this.TheLabel);
      this.Controls.Add(this.TheImg);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "TempWaitForm";
      ((System.ComponentModel.ISupportInitialize)(this.TheImg)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox TheImg;
    private System.Windows.Forms.Label TheLabel;

  }
}