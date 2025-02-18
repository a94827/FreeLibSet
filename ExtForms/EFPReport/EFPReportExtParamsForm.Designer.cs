namespace FreeLibSet.Forms
{
  partial class EFPReportExtParamsForm
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

    #region Windows TheForm Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EFPReportExtParamsForm));
      this.BottomPanel = new System.Windows.Forms.Panel();
      this.grpPresets = new System.Windows.Forms.GroupBox();
      this.FSetComboBox = new FreeLibSet.Controls.ParamSetComboBox();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.BottomPanel.SuspendLayout();
      this.grpPresets.SuspendLayout();
      this.SuspendLayout();
      // 
      // BottomPanel
      // 
      resources.ApplyResources(this.BottomPanel, "BottomPanel");
      this.BottomPanel.Controls.Add(this.grpPresets);
      this.BottomPanel.Controls.Add(this.btnCancel);
      this.BottomPanel.Controls.Add(this.btnOk);
      this.BottomPanel.Name = "BottomPanel";
      // 
      // grpPresets
      // 
      resources.ApplyResources(this.grpPresets, "grpPresets");
      this.grpPresets.Controls.Add(this.FSetComboBox);
      this.grpPresets.Name = "grpPresets";
      this.grpPresets.TabStop = false;
      // 
      // FSetComboBox
      // 
      resources.ApplyResources(this.FSetComboBox, "FSetComboBox");
      this.FSetComboBox.Name = "FSetComboBox";
      this.FSetComboBox.SelectedCode = "";
      this.FSetComboBox.SelectedItem = null;
      this.FSetComboBox.SelectedMD5Sum = "";
      this.FSetComboBox.ShowMD5 = false;
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Name = "btnOk";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // MainPanel
      // 
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Name = "MainPanel";
      // 
      // EFPReportExtParamsForm
      // 
      this.AcceptButton = this.btnOk;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.MainPanel);
      this.Controls.Add(this.BottomPanel);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "EFPReportExtParamsForm";
      this.BottomPanel.ResumeLayout(false);
      this.grpPresets.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel BottomPanel;
    private System.Windows.Forms.GroupBox grpPresets;

    /// <summary>
    /// Основная панель для размещения управляющих элементов диалога параметров
    /// </summary>
    public System.Windows.Forms.Panel MainPanel;
    private FreeLibSet.Controls.ParamSetComboBox FSetComboBox;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
  }
}
