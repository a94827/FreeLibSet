#pragma warning disable 1591

namespace AgeyevAV.ExtForms
{
  partial class DateRangeBox
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DateRangeBox));
      this.TheLP = new System.Windows.Forms.TableLayoutPanel();
      this.TheRightButton = new AgeyevAV.ExtForms.ControlRightButton();
      this.TheImageList = new System.Windows.Forms.ImageList(this.components);
      this.lblPeriod = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.FirstDate = new AgeyevAV.ExtForms.DateBox();
      this.label2 = new System.Windows.Forms.Label();
      this.TheMenuButton = new AgeyevAV.ExtForms.ControlRightButton();
      this.LastDate = new AgeyevAV.ExtForms.DateBox();
      this.TheLeftButton = new AgeyevAV.ExtForms.ControlRightButton();
      this.TheLP.SuspendLayout();
      this.SuspendLayout();
      // 
      // TheLP
      // 
      this.TheLP.ColumnCount = 8;
      this.TheLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.TheLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.TheLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.TheLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.TheLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 6F));
      this.TheLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.TheLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.TheLP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.TheLP.Controls.Add(this.TheRightButton, 7, 0);
      this.TheLP.Controls.Add(this.lblPeriod, 0, 1);
      this.TheLP.Controls.Add(this.label1, 0, 0);
      this.TheLP.Controls.Add(this.FirstDate, 1, 0);
      this.TheLP.Controls.Add(this.label2, 2, 0);
      this.TheLP.Controls.Add(this.TheMenuButton, 5, 0);
      this.TheLP.Controls.Add(this.LastDate, 3, 0);
      this.TheLP.Controls.Add(this.TheLeftButton, 6, 0);
      this.TheLP.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheLP.Location = new System.Drawing.Point(0, 0);
      this.TheLP.Name = "TheLP";
      this.TheLP.RowCount = 2;
      this.TheLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.TheLP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 17F));
      this.TheLP.Size = new System.Drawing.Size(350, 37);
      this.TheLP.TabIndex = 0;
      // 
      // TheRightButton
      // 
      this.TheRightButton.Dock = System.Windows.Forms.DockStyle.Right;
      this.TheRightButton.ImageIndex = 2;
      this.TheRightButton.ImageList = this.TheImageList;
      this.TheRightButton.Location = new System.Drawing.Point(331, 0);
      this.TheRightButton.Margin = new System.Windows.Forms.Padding(0);
      this.TheRightButton.Name = "TheRightButton";
      this.TheRightButton.Size = new System.Drawing.Size(19, 20);
      this.TheRightButton.TabIndex = 8;
      this.TheRightButton.Click += new System.EventHandler(this.TheRightButton_Click);
      // 
      // TheImageList
      // 
      this.TheImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("TheImageList.ImageStream")));
      this.TheImageList.TransparentColor = System.Drawing.Color.Magenta;
      this.TheImageList.Images.SetKeyName(0, "Menu");
      this.TheImageList.Images.SetKeyName(1, "Left");
      this.TheImageList.Images.SetKeyName(2, "Right");
      // 
      // lblPeriod
      // 
      this.lblPeriod.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblPeriod.AutoSize = true;
      this.TheLP.SetColumnSpan(this.lblPeriod, 8);
      this.lblPeriod.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblPeriod.Location = new System.Drawing.Point(3, 20);
      this.lblPeriod.Name = "lblPeriod";
      this.lblPeriod.Padding = new System.Windows.Forms.Padding(2);
      this.lblPeriod.Size = new System.Drawing.Size(344, 17);
      this.lblPeriod.TabIndex = 5;
      this.lblPeriod.Text = "????";
      this.lblPeriod.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.Location = new System.Drawing.Point(3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(19, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "c";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // FirstDate
      // 
      this.FirstDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.FirstDate.ClearButton = true;
      this.FirstDate.Location = new System.Drawing.Point(25, 0);
      this.FirstDate.Margin = new System.Windows.Forms.Padding(0);
      this.FirstDate.Name = "FirstDate";
      this.FirstDate.Size = new System.Drawing.Size(118, 20);
      this.FirstDate.TabIndex = 1;
      this.FirstDate.VisibleChanged += new System.EventHandler(this.DateValueChanged);
      this.FirstDate.EnabledChanged += new System.EventHandler(this.DateValueChanged);
      this.FirstDate.ValueChanged += new System.EventHandler(this.DateValueChanged);
      // 
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.Location = new System.Drawing.Point(146, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(19, 20);
      this.label2.TabIndex = 2;
      this.label2.Text = "по";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // TheMenuButton
      // 
      this.TheMenuButton.Dock = System.Windows.Forms.DockStyle.Right;
      this.TheMenuButton.ImageIndex = 0;
      this.TheMenuButton.ImageList = this.TheImageList;
      this.TheMenuButton.Location = new System.Drawing.Point(292, 0);
      this.TheMenuButton.Margin = new System.Windows.Forms.Padding(0);
      this.TheMenuButton.Name = "TheMenuButton";
      this.TheMenuButton.Size = new System.Drawing.Size(19, 20);
      this.TheMenuButton.TabIndex = 6;
      this.TheMenuButton.Visible = false;
      this.TheMenuButton.Click += new System.EventHandler(this.TheMenuButton_Click);
      // 
      // LastDate
      // 
      this.LastDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.LastDate.ClearButton = true;
      this.LastDate.Location = new System.Drawing.Point(168, 0);
      this.LastDate.Margin = new System.Windows.Forms.Padding(0);
      this.LastDate.Name = "LastDate";
      this.LastDate.Size = new System.Drawing.Size(118, 20);
      this.LastDate.TabIndex = 3;
      this.LastDate.VisibleChanged += new System.EventHandler(this.DateValueChanged);
      this.LastDate.EnabledChanged += new System.EventHandler(this.DateValueChanged);
      this.LastDate.ValueChanged += new System.EventHandler(this.DateValueChanged);
      // 
      // TheLeftButton
      // 
      this.TheLeftButton.Dock = System.Windows.Forms.DockStyle.Right;
      this.TheLeftButton.ImageIndex = 1;
      this.TheLeftButton.ImageList = this.TheImageList;
      this.TheLeftButton.Location = new System.Drawing.Point(311, 0);
      this.TheLeftButton.Margin = new System.Windows.Forms.Padding(0);
      this.TheLeftButton.Name = "TheLeftButton";
      this.TheLeftButton.Size = new System.Drawing.Size(19, 20);
      this.TheLeftButton.TabIndex = 7;
      this.TheLeftButton.Click += new System.EventHandler(this.TheLeftButton_Click);
      // 
      // DateRangeBox
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.Controls.Add(this.TheLP);
      this.Name = "DateRangeBox";
      this.Size = new System.Drawing.Size(350, 37);
      this.TheLP.ResumeLayout(false);
      this.TheLP.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel TheLP;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label lblPeriod;
    public AgeyevAV.ExtForms.DateBox FirstDate;
    public AgeyevAV.ExtForms.DateBox LastDate;
    private System.Windows.Forms.ImageList TheImageList;
    private ControlRightButton TheMenuButton;
    internal ControlRightButton TheLeftButton;
    internal ControlRightButton TheRightButton;
  }
}
