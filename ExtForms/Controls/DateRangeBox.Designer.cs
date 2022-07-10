#pragma warning disable 1591

namespace FreeLibSet.Controls
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DateRangeBox));
      this.TheLP = new System.Windows.Forms.TableLayoutPanel();
      this.lblPeriod = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.TheRightButton = new FreeLibSet.Controls.ControlRightButton();
      this.First = new FreeLibSet.Controls.DateTimeBox();
      this.TheMenuButton = new FreeLibSet.Controls.ControlRightButton();
      this.Last = new FreeLibSet.Controls.DateTimeBox();
      this.TheLeftButton = new FreeLibSet.Controls.ControlRightButton();
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
      this.TheLP.Controls.Add(this.First, 1, 0);
      this.TheLP.Controls.Add(this.label2, 2, 0);
      this.TheLP.Controls.Add(this.TheMenuButton, 5, 0);
      this.TheLP.Controls.Add(this.Last, 3, 0);
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
      // label2
      // 
      this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.label2.Location = new System.Drawing.Point(145, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(19, 20);
      this.label2.TabIndex = 2;
      this.label2.Text = "по";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // TheRightButton
      // 
      this.TheRightButton.BackColor = System.Drawing.SystemColors.Control;
      this.TheRightButton.Dock = System.Windows.Forms.DockStyle.Right;
      this.TheRightButton.Image = ((System.Drawing.Image)(resources.GetObject("TheRightButton.Image")));
      this.TheRightButton.Location = new System.Drawing.Point(330, 0);
      this.TheRightButton.Margin = new System.Windows.Forms.Padding(0);
      this.TheRightButton.Name = "TheRightButton";
      this.TheRightButton.Size = new System.Drawing.Size(20, 20);
      this.TheRightButton.TabIndex = 8;
      this.TheRightButton.Click += new System.EventHandler(this.TheRightButton_Click);
      // 
      // First
      // 
      this.First.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.First.ClearButton = true;
      this.First.Location = new System.Drawing.Point(25, 0);
      this.First.Margin = new System.Windows.Forms.Padding(0);
      this.First.Name = "First";
      this.First.Size = new System.Drawing.Size(117, 20);
      this.First.TabIndex = 1;
      this.First.VisibleChanged += new System.EventHandler(this.DateValueChanged);
      this.First.EnabledChanged += new System.EventHandler(this.DateValueChanged);
      this.First.ValueChanged += new System.EventHandler(this.DateValueChanged);
      // 
      // TheMenuButton
      // 
      this.TheMenuButton.BackColor = System.Drawing.SystemColors.Control;
      this.TheMenuButton.Dock = System.Windows.Forms.DockStyle.Right;
      this.TheMenuButton.Image = ((System.Drawing.Image)(resources.GetObject("TheMenuButton.Image")));
      this.TheMenuButton.Location = new System.Drawing.Point(290, 0);
      this.TheMenuButton.Margin = new System.Windows.Forms.Padding(0);
      this.TheMenuButton.Name = "TheMenuButton";
      this.TheMenuButton.Size = new System.Drawing.Size(20, 20);
      this.TheMenuButton.TabIndex = 6;
      this.TheMenuButton.Visible = false;
      this.TheMenuButton.Click += new System.EventHandler(this.TheMenuButton_Click);
      // 
      // Last
      // 
      this.Last.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.Last.ClearButton = true;
      this.Last.Location = new System.Drawing.Point(167, 0);
      this.Last.Margin = new System.Windows.Forms.Padding(0);
      this.Last.Name = "Last";
      this.Last.Size = new System.Drawing.Size(117, 20);
      this.Last.TabIndex = 3;
      this.Last.VisibleChanged += new System.EventHandler(this.DateValueChanged);
      this.Last.EnabledChanged += new System.EventHandler(this.DateValueChanged);
      this.Last.ValueChanged += new System.EventHandler(this.DateValueChanged);
      // 
      // TheLeftButton
      // 
      this.TheLeftButton.BackColor = System.Drawing.SystemColors.Control;
      this.TheLeftButton.Dock = System.Windows.Forms.DockStyle.Right;
      this.TheLeftButton.Image = ((System.Drawing.Image)(resources.GetObject("TheLeftButton.Image")));
      this.TheLeftButton.Location = new System.Drawing.Point(310, 0);
      this.TheLeftButton.Margin = new System.Windows.Forms.Padding(0);
      this.TheLeftButton.Name = "TheLeftButton";
      this.TheLeftButton.Size = new System.Drawing.Size(20, 20);
      this.TheLeftButton.TabIndex = 7;
      this.TheLeftButton.Click += new System.EventHandler(this.TheLeftButton_Click);
      // 
      // DateRangeBox
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
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
    public FreeLibSet.Controls.DateTimeBox First;
    public FreeLibSet.Controls.DateTimeBox Last;
    private ControlRightButton TheMenuButton;
    internal ControlRightButton TheLeftButton;
    internal ControlRightButton TheRightButton;
  }
}
