namespace FreeLibSet.Controls
{
  partial class MonthDayBox
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
      this.edDay = new FreeLibSet.Controls.Int32EditBox();
      this.cbMonth = new System.Windows.Forms.ComboBox();
      this.SuspendLayout();
      // 
      // edDay
      // 
      this.edDay.Dock = System.Windows.Forms.DockStyle.Left;
      this.edDay.Location = new System.Drawing.Point(0, 0);
      this.edDay.Name = "edDay";
      this.edDay.Size = new System.Drawing.Size(47, 20);
      this.edDay.TabIndex = 0;
      this.edDay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // cbMonth
      // 
      this.cbMonth.Dock = System.Windows.Forms.DockStyle.Fill;
      this.cbMonth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbMonth.FormattingEnabled = true;
      this.cbMonth.Location = new System.Drawing.Point(47, 0);
      this.cbMonth.Name = "cbMonth";
      this.cbMonth.Size = new System.Drawing.Size(133, 21);
      this.cbMonth.TabIndex = 1;
      // 
      // MonthDayBox
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
      this.Controls.Add(this.cbMonth);
      this.Controls.Add(this.edDay);
      this.Name = "MonthDayBox";
      this.Size = new System.Drawing.Size(180, 21);
      this.ResumeLayout(false);

    }

    #endregion

    private FreeLibSet.Controls.Int32EditBox edDay;
    private System.Windows.Forms.ComboBox cbMonth;
  }
}
