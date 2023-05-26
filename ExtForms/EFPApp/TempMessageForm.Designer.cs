namespace FreeLibSet.Forms
{
  partial class TempMessageForm
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
      this.MsgLabel = new System.Windows.Forms.Label();
      this.TempTimer = new System.Timers.Timer();
      ((System.ComponentModel.ISupportInitialize)(this.TempTimer)).BeginInit();
      this.SuspendLayout();
      // 
      // MsgLabel
      // 
      this.MsgLabel.BackColor = System.Drawing.Color.Red;
      this.MsgLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.MsgLabel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MsgLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.MsgLabel.ForeColor = System.Drawing.Color.White;
      this.MsgLabel.Location = new System.Drawing.Point(0, 0);
      this.MsgLabel.Name = "MsgLabel";
      this.MsgLabel.Size = new System.Drawing.Size(618, 22);
      this.MsgLabel.TabIndex = 0;
      this.MsgLabel.Text = "???";
      this.MsgLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.MsgLabel.Click += new System.EventHandler(this.MsgLabel_Click);
      // 
      // TempTimer
      // 
      this.TempTimer.Interval = 3000;
      this.TempTimer.SynchronizingObject = this;
      this.TempTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.TempTimer_Elapsed);
      // 
      // TempMessageForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(618, 22);
      this.Controls.Add(this.MsgLabel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Name = "TempMessageForm";
      ((System.ComponentModel.ISupportInitialize)(this.TempTimer)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label MsgLabel;
    private System.Timers.Timer TempTimer;

  }
}
