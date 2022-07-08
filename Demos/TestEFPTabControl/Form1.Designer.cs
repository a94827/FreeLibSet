namespace TestEFPTabControl
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnAdd = new System.Windows.Forms.Button();
      this.TheTC = new System.Windows.Forms.TabControl();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnAdd);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 225);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(284, 37);
      this.panel1.TabIndex = 1;
      // 
      // btnAdd
      // 
      this.btnAdd.Location = new System.Drawing.Point(8, 8);
      this.btnAdd.Name = "btnAdd";
      this.btnAdd.Size = new System.Drawing.Size(88, 24);
      this.btnAdd.TabIndex = 0;
      this.btnAdd.Text = "Добавить";
      this.btnAdd.UseVisualStyleBackColor = true;
      // 
      // TheTC
      // 
      this.TheTC.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTC.Location = new System.Drawing.Point(0, 0);
      this.TheTC.Name = "TheTC";
      this.TheTC.SelectedIndex = 0;
      this.TheTC.Size = new System.Drawing.Size(284, 225);
      this.TheTC.TabIndex = 0;
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 262);
      this.Controls.Add(this.TheTC);
      this.Controls.Add(this.panel1);
      this.Name = "Form1";
      this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
      this.Text = "Form1";
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.TabControl TheTC;
    private System.Windows.Forms.Button btnAdd;
  }
}

