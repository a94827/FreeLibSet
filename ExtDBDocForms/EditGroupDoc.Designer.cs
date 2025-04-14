namespace FreeLibSet.Forms.Docs
{
  partial class EditGroupDoc
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditGroupDoc));
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tpMain = new System.Windows.Forms.TabPage();
      this.MainPanel1 = new System.Windows.Forms.Panel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbParent = new FreeLibSet.Controls.UserSelComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.edName = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.tabControl1.SuspendLayout();
      this.tpMain.SuspendLayout();
      this.MainPanel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl1
      // 
      resources.ApplyResources(this.tabControl1, "tabControl1");
      this.tabControl1.Controls.Add(this.tpMain);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      // 
      // tpMain
      // 
      resources.ApplyResources(this.tpMain, "tpMain");
      this.tpMain.Controls.Add(this.MainPanel1);
      this.tpMain.Name = "tpMain";
      this.tpMain.UseVisualStyleBackColor = true;
      // 
      // MainPanel1
      // 
      resources.ApplyResources(this.MainPanel1, "MainPanel1");
      this.MainPanel1.Controls.Add(this.groupBox1);
      this.MainPanel1.Name = "MainPanel1";
      // 
      // groupBox1
      // 
      resources.ApplyResources(this.groupBox1, "groupBox1");
      this.groupBox1.Controls.Add(this.cbParent);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.edName);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.TabStop = false;
      // 
      // cbParent
      // 
      resources.ApplyResources(this.cbParent, "cbParent");
      this.cbParent.Name = "cbParent";
      // 
      // label2
      // 
      resources.ApplyResources(this.label2, "label2");
      this.label2.Name = "label2";
      // 
      // edName
      // 
      resources.ApplyResources(this.edName, "edName");
      this.edName.Name = "edName";
      // 
      // label1
      // 
      resources.ApplyResources(this.label1, "label1");
      this.label1.Name = "label1";
      // 
      // EditGroupDoc
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tabControl1);
      this.Name = "EditGroupDoc";
      this.tabControl1.ResumeLayout(false);
      this.tpMain.ResumeLayout(false);
      this.MainPanel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tpMain;
    private System.Windows.Forms.Panel MainPanel1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.TextBox edName;
    private System.Windows.Forms.Label label1;
    private FreeLibSet.Controls.UserSelComboBox cbParent;
    private System.Windows.Forms.Label label2;
  }
}
