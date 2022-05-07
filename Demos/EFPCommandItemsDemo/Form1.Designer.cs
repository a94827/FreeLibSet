namespace EFPCommandItemsDemo
{
  partial class Form1
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
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.panSpb1 = new System.Windows.Forms.Panel();
      this.grid1 = new System.Windows.Forms.DataGridView();
      this.panel1 = new System.Windows.Forms.Panel();
      this.tb2 = new System.Windows.Forms.TextBox();
      this.TheTabControl.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.tabPage2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grid1)).BeginInit();
      this.SuspendLayout();
      // 
      // TheTabControl
      // 
      this.TheTabControl.Controls.Add(this.tabPage1);
      this.TheTabControl.Controls.Add(this.tabPage2);
      this.TheTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTabControl.Location = new System.Drawing.Point(0, 0);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(292, 270);
      this.TheTabControl.TabIndex = 0;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.grid1);
      this.tabPage1.Controls.Add(this.panSpb1);
      this.tabPage1.Location = new System.Drawing.Point(4, 22);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(284, 244);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "tabPage1";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.tb2);
      this.tabPage2.Controls.Add(this.panel1);
      this.tabPage2.Location = new System.Drawing.Point(4, 22);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage2.Size = new System.Drawing.Size(284, 244);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "tabPage2";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // panSpb1
      // 
      this.panSpb1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panSpb1.Location = new System.Drawing.Point(3, 3);
      this.panSpb1.Name = "panSpb1";
      this.panSpb1.Size = new System.Drawing.Size(278, 40);
      this.panSpb1.TabIndex = 0;
      // 
      // grid1
      // 
      this.grid1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grid1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grid1.Location = new System.Drawing.Point(3, 43);
      this.grid1.Name = "grid1";
      this.grid1.Size = new System.Drawing.Size(278, 198);
      this.grid1.TabIndex = 1;
      // 
      // panel1
      // 
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(3, 213);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(278, 28);
      this.panel1.TabIndex = 0;
      // 
      // tb2
      // 
      this.tb2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tb2.Location = new System.Drawing.Point(3, 3);
      this.tb2.Multiline = true;
      this.tb2.Name = "tb2";
      this.tb2.Size = new System.Drawing.Size(278, 210);
      this.tb2.TabIndex = 1;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(292, 270);
      this.Controls.Add(this.TheTabControl);
      this.Name = "Form1";
      this.Text = "Form1";
      this.TheTabControl.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.tabPage2.ResumeLayout(false);
      this.tabPage2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grid1)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.DataGridView grid1;
    private System.Windows.Forms.Panel panSpb1;
    private System.Windows.Forms.TextBox tb2;
    private System.Windows.Forms.Panel panel1;
  }
}