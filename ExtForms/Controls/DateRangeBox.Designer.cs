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
      this.TheRightButton = new FreeLibSet.Controls.ControlRightButton();
      this.lblPeriod = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.First = new FreeLibSet.Controls.DateTimeBox();
      this.label2 = new System.Windows.Forms.Label();
      this.TheMenuButton = new FreeLibSet.Controls.ControlRightButton();
      this.Last = new FreeLibSet.Controls.DateTimeBox();
      this.TheLeftButton = new FreeLibSet.Controls.ControlRightButton();
      this.TheLP.SuspendLayout();
      this.SuspendLayout();
      // 
      // TheLP
      // 
      resources.ApplyResources(this.TheLP, "TheLP");
      this.TheLP.Controls.Add(this.TheRightButton, 7, 0);
      this.TheLP.Controls.Add(this.lblPeriod, 0, 1);
      this.TheLP.Controls.Add(this.label1, 0, 0);
      this.TheLP.Controls.Add(this.First, 1, 0);
      this.TheLP.Controls.Add(this.label2, 2, 0);
      this.TheLP.Controls.Add(this.TheMenuButton, 5, 0);
      this.TheLP.Controls.Add(this.Last, 3, 0);
      this.TheLP.Controls.Add(this.TheLeftButton, 6, 0);
      this.TheLP.Name = "TheLP";
      // 
      // TheRightButton
      // 
      resources.ApplyResources(this.TheRightButton, "TheRightButton");
      this.TheRightButton.BackColor = System.Drawing.SystemColors.Control;
      this.TheRightButton.Name = "TheRightButton";
      this.TheRightButton.Click += new System.EventHandler(this.TheRightButton_Click);
      // 
      // lblPeriod
      // 
      resources.ApplyResources(this.lblPeriod, "lblPeriod");
      this.TheLP.SetColumnSpan(this.lblPeriod, 8);
      this.lblPeriod.Name = "lblPeriod";
      // 
      // label1
      // 
      resources.ApplyResources(this.label1, "label1");
      this.label1.Name = "label1";
      // 
      // First
      // 
      resources.ApplyResources(this.First, "First");
      this.First.ClearButton = true;
      this.First.Name = "First";
      this.First.ValueChanged += new System.EventHandler(this.DateValueChanged);
      this.First.EnabledChanged += new System.EventHandler(this.DateValueChanged);
      this.First.VisibleChanged += new System.EventHandler(this.DateValueChanged);
      // 
      // label2
      // 
      resources.ApplyResources(this.label2, "label2");
      this.label2.Name = "label2";
      // 
      // TheMenuButton
      // 
      resources.ApplyResources(this.TheMenuButton, "TheMenuButton");
      this.TheMenuButton.BackColor = System.Drawing.SystemColors.Control;
      this.TheMenuButton.Name = "TheMenuButton";
      this.TheMenuButton.Click += new System.EventHandler(this.TheMenuButton_Click);
      // 
      // Last
      // 
      resources.ApplyResources(this.Last, "Last");
      this.Last.ClearButton = true;
      this.Last.Name = "Last";
      this.Last.ValueChanged += new System.EventHandler(this.DateValueChanged);
      this.Last.EnabledChanged += new System.EventHandler(this.DateValueChanged);
      this.Last.VisibleChanged += new System.EventHandler(this.DateValueChanged);
      // 
      // TheLeftButton
      // 
      resources.ApplyResources(this.TheLeftButton, "TheLeftButton");
      this.TheLeftButton.BackColor = System.Drawing.SystemColors.Control;
      this.TheLeftButton.Name = "TheLeftButton";
      this.TheLeftButton.Click += new System.EventHandler(this.TheLeftButton_Click);
      // 
      // DateRangeBox
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.TheLP);
      this.Name = "DateRangeBox";
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
