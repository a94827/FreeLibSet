namespace FreeLibSet.Forms
{
  partial class EFPReportExtParamsTwoPageForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EFPReportExtParamsTwoPageForm));
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.MainTabPage = new System.Windows.Forms.TabPage();
      this.FiltersTabPage = new System.Windows.Forms.TabPage();
      this.MainPanel.SuspendLayout();
      this.TheTabControl.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Controls.Add(this.TheTabControl);
      // 
      // TheTabControl
      // 
      resources.ApplyResources(this.TheTabControl, "TheTabControl");
      this.TheTabControl.Controls.Add(this.MainTabPage);
      this.TheTabControl.Controls.Add(this.FiltersTabPage);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      // 
      // MainTabPage
      // 
      resources.ApplyResources(this.MainTabPage, "MainTabPage");
      this.MainTabPage.Name = "MainTabPage";
      this.MainTabPage.UseVisualStyleBackColor = true;
      // 
      // FiltersTabPage
      // 
      resources.ApplyResources(this.FiltersTabPage, "FiltersTabPage");
      this.FiltersTabPage.Name = "FiltersTabPage";
      this.FiltersTabPage.UseVisualStyleBackColor = true;
      // 
      // EFPReportExtParamsTwoPageForm
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Name = "EFPReportExtParamsTwoPageForm";
      this.MainPanel.ResumeLayout(false);
      this.TheTabControl.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TabControl TheTabControl;

    /// <summary>
    /// Вкладка "Общие".
    /// Сюда должны быть добавлены пользовательские управляющие элементы, задающие основные параметры отчета
    /// </summary>
    public System.Windows.Forms.TabPage MainTabPage;
    private System.Windows.Forms.TabPage FiltersTabPage;
  }
}
