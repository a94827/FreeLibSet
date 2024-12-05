namespace LogWatcher
{
  partial class MainForm
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
      this.panSpb1 = new System.Windows.Forms.Panel();
      this.gr1 = new System.Windows.Forms.DataGridView();
      ((System.ComponentModel.ISupportInitialize)(this.gr1)).BeginInit();
      this.SuspendLayout();
      // 
      // panSpb1
      // 
      this.panSpb1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panSpb1.Location = new System.Drawing.Point(0, 0);
      this.panSpb1.Name = "panSpb1";
      this.panSpb1.Size = new System.Drawing.Size(413, 40);
      this.panSpb1.TabIndex = 0;
      // 
      // gr1
      // 
      this.gr1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.gr1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.gr1.Location = new System.Drawing.Point(0, 40);
      this.gr1.Name = "gr1";
      this.gr1.RowTemplate.Height = 24;
      this.gr1.Size = new System.Drawing.Size(413, 322);
      this.gr1.TabIndex = 1;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(413, 362);
      this.Controls.Add(this.gr1);
      this.Controls.Add(this.panSpb1);
      this.MinimizeBox = false;
      this.Name = "MainForm";
      this.Text = "Слежение за файлами";
      ((System.ComponentModel.ISupportInitialize)(this.gr1)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panSpb1;
    private System.Windows.Forms.DataGridView gr1;
  }
}

