namespace WinFormsDemo.EFPTabControlDemo
{
  partial class EFPTabControlForm
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
      this.TheTC = new System.Windows.Forms.TabControl();
      this.SuspendLayout();
      // 
      // TheTC
      // 
      this.TheTC.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTC.Location = new System.Drawing.Point(0, 0);
      this.TheTC.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.TheTC.Name = "TheTC";
      this.TheTC.SelectedIndex = 0;
      this.TheTC.Size = new System.Drawing.Size(604, 322);
      this.TheTC.TabIndex = 0;
      // 
      // EFPTabControlForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(604, 322);
      this.Controls.Add(this.TheTC);
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.Name = "EFPTabControlForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
      this.Text = "Use context menu commands";
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl TheTC;
  }
}

