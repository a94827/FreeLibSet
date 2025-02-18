namespace FreeLibSet.Forms
{
  partial class GridConfigForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GridConfigForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.grpSets = new System.Windows.Forms.GroupBox();
      this.SetComboBox = new FreeLibSet.Controls.ParamSetComboBox();
      this.panDefault = new System.Windows.Forms.Panel();
      this.btnDefault = new System.Windows.Forms.Button();
      this.panel3 = new System.Windows.Forms.Panel();
      this.btnCopy = new System.Windows.Forms.Button();
      this.btnPaste = new System.Windows.Forms.Button();
      this.panel2 = new System.Windows.Forms.Panel();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.panel1.SuspendLayout();
      this.grpSets.SuspendLayout();
      this.panDefault.SuspendLayout();
      this.panel3.SuspendLayout();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Controls.Add(this.grpSets);
      this.panel1.Controls.Add(this.panDefault);
      this.panel1.Controls.Add(this.panel3);
      this.panel1.Controls.Add(this.panel2);
      this.panel1.Name = "panel1";
      // 
      // grpSets
      // 
      resources.ApplyResources(this.grpSets, "grpSets");
      this.grpSets.Controls.Add(this.SetComboBox);
      this.grpSets.Name = "grpSets";
      this.grpSets.TabStop = false;
      // 
      // SetComboBox
      // 
      resources.ApplyResources(this.SetComboBox, "SetComboBox");
      this.SetComboBox.Name = "SetComboBox";
      this.SetComboBox.SelectedCode = "";
      this.SetComboBox.SelectedItem = null;
      this.SetComboBox.SelectedMD5Sum = "";
      this.SetComboBox.ShowMD5 = false;
      // 
      // panDefault
      // 
      resources.ApplyResources(this.panDefault, "panDefault");
      this.panDefault.Controls.Add(this.btnDefault);
      this.panDefault.Name = "panDefault";
      // 
      // btnDefault
      // 
      resources.ApplyResources(this.btnDefault, "btnDefault");
      this.btnDefault.Name = "btnDefault";
      this.btnDefault.UseVisualStyleBackColor = true;
      // 
      // panel3
      // 
      resources.ApplyResources(this.panel3, "panel3");
      this.panel3.Controls.Add(this.btnCopy);
      this.panel3.Controls.Add(this.btnPaste);
      this.panel3.Name = "panel3";
      // 
      // btnCopy
      // 
      resources.ApplyResources(this.btnCopy, "btnCopy");
      this.btnCopy.Name = "btnCopy";
      this.btnCopy.UseVisualStyleBackColor = true;
      // 
      // btnPaste
      // 
      resources.ApplyResources(this.btnPaste, "btnPaste");
      this.btnPaste.Name = "btnPaste";
      this.btnPaste.UseVisualStyleBackColor = true;
      // 
      // panel2
      // 
      resources.ApplyResources(this.panel2, "panel2");
      this.panel2.Controls.Add(this.btnOk);
      this.panel2.Controls.Add(this.btnCancel);
      this.panel2.Name = "panel2";
      // 
      // btnOk
      // 
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Name = "btnOk";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // MainPanel
      // 
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Name = "MainPanel";
      // 
      // GridConfigForm
      // 
      this.AcceptButton = this.btnOk;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.MainPanel);
      this.Controls.Add(this.panel1);
      this.Name = "GridConfigForm";
      this.panel1.ResumeLayout(false);
      this.grpSets.ResumeLayout(false);
      this.panDefault.ResumeLayout(false);
      this.panel3.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    public System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.Button btnCopy;
    private System.Windows.Forms.Button btnPaste;
    private System.Windows.Forms.GroupBox grpSets;
    public FreeLibSet.Controls.ParamSetComboBox SetComboBox;
    private System.Windows.Forms.Panel panDefault;
    private System.Windows.Forms.Button btnDefault;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Panel panel2;
  }
}
