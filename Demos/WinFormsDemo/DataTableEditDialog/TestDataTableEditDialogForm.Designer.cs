namespace WinFormsDemo.DataTableEditDialogDemo
{
  partial class TestDataTableEditDialogForm
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
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tabPage1 = new System.Windows.Forms.TabPage();
      this.Panel1 = new System.Windows.Forms.Panel();
      this.ed2 = new FreeLibSet.Controls.Int32EditBox();
      this.label2 = new System.Windows.Forms.Label();
      this.ed1 = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.tabControl1.SuspendLayout();
      this.tabPage1.SuspendLayout();
      this.Panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tabPage1);
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.Location = new System.Drawing.Point(0, 0);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(466, 321);
      this.tabControl1.TabIndex = 0;
      // 
      // tabPage1
      // 
      this.tabPage1.Controls.Add(this.Panel1);
      this.tabPage1.Location = new System.Drawing.Point(4, 25);
      this.tabPage1.Name = "tabPage1";
      this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
      this.tabPage1.Size = new System.Drawing.Size(458, 292);
      this.tabPage1.TabIndex = 0;
      this.tabPage1.Text = "tabPage1";
      this.tabPage1.UseVisualStyleBackColor = true;
      // 
      // Panel1
      // 
      this.Panel1.Controls.Add(this.ed2);
      this.Panel1.Controls.Add(this.label2);
      this.Panel1.Controls.Add(this.ed1);
      this.Panel1.Controls.Add(this.label1);
      this.Panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.Panel1.Location = new System.Drawing.Point(3, 3);
      this.Panel1.Name = "Panel1";
      this.Panel1.Size = new System.Drawing.Size(452, 116);
      this.Panel1.TabIndex = 0;
      // 
      // ed2
      // 
      this.ed2.Location = new System.Drawing.Point(210, 73);
      this.ed2.Name = "ed2";
      this.ed2.Size = new System.Drawing.Size(186, 22);
      this.ed2.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(12, 73);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(179, 22);
      this.label2.TabIndex = 2;
      this.label2.Text = "F2. Int32EditBox, MultiEdit";
      // 
      // ed1
      // 
      this.ed1.Location = new System.Drawing.Point(210, 32);
      this.ed1.Name = "ed1";
      this.ed1.Size = new System.Drawing.Size(186, 22);
      this.ed1.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(12, 32);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(179, 22);
      this.label1.TabIndex = 0;
      this.label1.Text = "F1. TextBox, NoMultiEdit";
      // 
      // TestDBxDataTableExtEditorForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(466, 321);
      this.Controls.Add(this.tabControl1);
      this.Name = "TestDBxDataTableExtEditorForm";
      this.Text = "TestExtEditDialogForm";
      this.tabControl1.ResumeLayout(false);
      this.tabPage1.ResumeLayout(false);
      this.Panel1.ResumeLayout(false);
      this.Panel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tabPage1;
    private System.Windows.Forms.Panel Panel1;
    private FreeLibSet.Controls.Int32EditBox ed2;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox ed1;
    private System.Windows.Forms.Label label1;
  }
}