namespace AgeyevAV.ExtForms.FIAS
{
  partial class DummyForm
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DummyForm));
      this.MainImageList = new System.Windows.Forms.ImageList(this.components);
      this.SuspendLayout();
      // 
      // MainImageList
      // 
      this.MainImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("MainImageList.ImageStream")));
      this.MainImageList.TransparentColor = System.Drawing.Color.Magenta;
      this.MainImageList.Images.SetKeyName(0, "FIAS.Address");
      this.MainImageList.Images.SetKeyName(1, "FIAS.PostalCode");
      this.MainImageList.Images.SetKeyName(2, "FIAS.Details");
      // 
      // DummyForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(379, 322);
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "DummyForm";
      this.Text = "DummyForm";
      this.ResumeLayout(false);

    }

    #endregion

    public System.Windows.Forms.ImageList MainImageList;

  }
}