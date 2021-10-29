namespace FreeLibSet.Controls
{
  partial class ParamSetComboBox
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ParamSetComboBox));
      this.TheImgList = new System.Windows.Forms.ImageList(this.components);
      this.DeleteButton = new System.Windows.Forms.Button();
      this.SaveButton = new System.Windows.Forms.Button();
      this.TheCB = new System.Windows.Forms.ComboBox();
      this.SuspendLayout();
      // 
      // TheImgList
      // 
      this.TheImgList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("TheImgList.ImageStream")));
      this.TheImgList.TransparentColor = System.Drawing.Color.Magenta;
      this.TheImgList.Images.SetKeyName(0, "Insert");
      this.TheImgList.Images.SetKeyName(1, "Delete");
      // 
      // DeleteButton
      // 
      this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.DeleteButton.ImageIndex = 1;
      this.DeleteButton.ImageList = this.TheImgList;
      this.DeleteButton.Location = new System.Drawing.Point(226, 0);
      this.DeleteButton.Name = "DeleteButton";
      this.DeleteButton.Size = new System.Drawing.Size(24, 24);
      this.DeleteButton.TabIndex = 2;
      this.DeleteButton.UseVisualStyleBackColor = true;
      // 
      // SaveButton
      // 
      this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.SaveButton.ImageIndex = 0;
      this.SaveButton.ImageList = this.TheImgList;
      this.SaveButton.Location = new System.Drawing.Point(202, 0);
      this.SaveButton.Name = "SaveButton";
      this.SaveButton.Size = new System.Drawing.Size(24, 24);
      this.SaveButton.TabIndex = 1;
      this.SaveButton.UseVisualStyleBackColor = true;
      // 
      // TheCB
      // 
      this.TheCB.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.TheCB.FormattingEnabled = true;
      this.TheCB.Location = new System.Drawing.Point(0, 0);
      this.TheCB.MaxDropDownItems = 20;
      this.TheCB.Name = "TheCB";
      this.TheCB.Size = new System.Drawing.Size(202, 21);
      this.TheCB.TabIndex = 0;
      // 
      // ParamSetComboBox
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
      this.Controls.Add(this.TheCB);
      this.Controls.Add(this.SaveButton);
      this.Controls.Add(this.DeleteButton);
      this.MinimumSize = new System.Drawing.Size(150, 24);
      this.Name = "ParamSetComboBox";
      this.Size = new System.Drawing.Size(250, 24);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ImageList TheImgList;

    /// <summary>
    /// Кнопка [-]
    /// </summary>
    public System.Windows.Forms.Button DeleteButton;

    /// <summary>
    /// Кнопка [+]
    /// </summary>
    public System.Windows.Forms.Button SaveButton;

    /// <summary>
    /// Основное поле
    /// </summary>
    public System.Windows.Forms.ComboBox TheCB;
  }
}
