namespace FreeLibSet.Forms.Docs
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
      this.MainImageList.Images.SetKeyName(0, "ShowHiddenDocs");
      this.MainImageList.Images.SetKeyName(1, "ShowIds");
      this.MainImageList.Images.SetKeyName(2, "MergeDocs");
      this.MainImageList.Images.SetKeyName(3, "DBxDocSelection");
      this.MainImageList.Images.SetKeyName(4, "UserActions");
      this.MainImageList.Images.SetKeyName(5, "UserPermission");
      this.MainImageList.Images.SetKeyName(6, "FindUserPermission");
      this.MainImageList.Images.SetKeyName(7, "RecalcColumns");
      this.MainImageList.Images.SetKeyName(8, "DocRefs");
      this.MainImageList.Images.SetKeyName(9, "DocSelfRef");
      this.MainImageList.Images.SetKeyName(10, "DocRefSchema");
      this.MainImageList.Images.SetKeyName(11, "GroupDocTreePanel");
      this.MainImageList.Images.SetKeyName(12, "GroupDocTreeCB");
      this.MainImageList.Images.SetKeyName(13, "OnlyOneGroupDoc");
      this.MainImageList.Images.SetKeyName(14, "XMLDataSet");
      this.MainImageList.Images.SetKeyName(15, "DifferentDatabase");
      // 
      // DummyForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(426, 403);
      this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.Name = "DummyForm";
      this.Text = "DummyForm";
      this.ResumeLayout(false);

    }

    #endregion

    public System.Windows.Forms.ImageList MainImageList;

  }
}