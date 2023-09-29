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
      this.grid1 = new System.Windows.Forms.DataGridView();
      this.panSpb1 = new System.Windows.Forms.Panel();
      this.tabPage2 = new System.Windows.Forms.TabPage();
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.panSpb2 = new System.Windows.Forms.Panel();
      this.tb2 = new System.Windows.Forms.TextBox();
      this.TheTabControl.SuspendLayout();
      this.tabPage1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grid1)).BeginInit();
      this.tabPage2.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // TheTabControl
      // 
      this.TheTabControl.Controls.Add(this.tabPage1);
      this.TheTabControl.Controls.Add(this.tabPage2);
      this.TheTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTabControl.Location = new System.Drawing.Point(0, 0);
      this.TheTabControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(389, 332);
      this.TheTabControl.TabIndex = 0;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.grid1);
      this.tabPage1.Controls.Add(this.panSpb1);
      this.tabPage1.Location = new System.Drawing.Point(4, 25);
      this.tabPage1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.tabPage1.Size = new System.Drawing.Size(381, 303);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "tabPage1";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // grid1
      // 
      this.grid1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grid1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grid1.Location = new System.Drawing.Point(4, 53);
      this.grid1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.grid1.Name = "grid1";
      this.grid1.Size = new System.Drawing.Size(373, 246);
      this.grid1.TabIndex = 1;
      // 
      // panSpb1
      // 
      this.panSpb1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panSpb1.Location = new System.Drawing.Point(4, 4);
      this.panSpb1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.panSpb1.Name = "panSpb1";
      this.panSpb1.Size = new System.Drawing.Size(373, 49);
      this.panSpb1.TabIndex = 0;
      // 
      // tabPage2
      // 
      this.tabPage2.Controls.Add(this.panel2);
      this.tabPage2.Controls.Add(this.panel1);
      this.tabPage2.Location = new System.Drawing.Point(4, 25);
      this.tabPage2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.tabPage2.Name = "tabPage2";
      this.tabPage2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.tabPage2.Size = new System.Drawing.Size(381, 303);
      this.tabPage2.TabIndex = 1;
      this.tabPage2.Text = "tabPage2";
      this.tabPage2.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(4, 265);
      this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(373, 34);
      this.panel1.TabIndex = 0;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.tb2);
      this.panel2.Controls.Add(this.panSpb2);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(4, 4);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(373, 261);
      this.panel2.TabIndex = 1;
      // 
      // panSpb2
      // 
      this.panSpb2.Dock = System.Windows.Forms.DockStyle.Top;
      this.panSpb2.Location = new System.Drawing.Point(0, 0);
      this.panSpb2.Name = "panSpb2";
      this.panSpb2.Size = new System.Drawing.Size(373, 34);
      this.panSpb2.TabIndex = 0;
      // 
      // tb2
      // 
      this.tb2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tb2.Location = new System.Drawing.Point(0, 34);
      this.tb2.Margin = new System.Windows.Forms.Padding(4);
      this.tb2.Multiline = true;
      this.tb2.Name = "tb2";
      this.tb2.Size = new System.Drawing.Size(373, 227);
      this.tb2.TabIndex = 2;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(389, 332);
      this.Controls.Add(this.TheTabControl);
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.Name = "Form1";
      this.Text = "Form1";
      this.TheTabControl.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grid1)).EndInit();
      this.tabPage2.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.panel2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.TabPage tabPage2;
    private System.Windows.Forms.DataGridView grid1;
    private System.Windows.Forms.Panel panSpb1;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.TextBox tb2;
    private System.Windows.Forms.Panel panSpb2;
  }
}
