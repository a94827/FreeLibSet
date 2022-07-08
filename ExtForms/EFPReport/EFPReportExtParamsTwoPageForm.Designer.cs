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
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.MainTabPage = new System.Windows.Forms.TabPage();
      this.FiltersTabPage = new System.Windows.Forms.TabPage();
      this.MainPanel.SuspendLayout();
      this.TheTabControl.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.TheTabControl);
      this.MainPanel.Size = new System.Drawing.Size(632, 404);
      // 
      // TheTabControl
      // 
      this.TheTabControl.Controls.Add(this.MainTabPage);
      this.TheTabControl.Controls.Add(this.FiltersTabPage);
      this.TheTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTabControl.Location = new System.Drawing.Point(0, 0);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(632, 404);
      this.TheTabControl.TabIndex = 0;
      // 
      // MainTabPage
      // 
      this.MainTabPage.Location = new System.Drawing.Point(4, 22);
      this.MainTabPage.Name = "MainTabPage";
      this.MainTabPage.Padding = new System.Windows.Forms.Padding(3);
      this.MainTabPage.Size = new System.Drawing.Size(624, 378);
      this.MainTabPage.TabIndex = 0;
      this.MainTabPage.Text = "Общие";
      this.MainTabPage.UseVisualStyleBackColor = true;
      // 
      // FiltersTabPage
      // 
      this.FiltersTabPage.Location = new System.Drawing.Point(4, 22);
      this.FiltersTabPage.Name = "FiltersTabPage";
      this.FiltersTabPage.Padding = new System.Windows.Forms.Padding(3);
      this.FiltersTabPage.Size = new System.Drawing.Size(624, 378);
      this.FiltersTabPage.TabIndex = 1;
      this.FiltersTabPage.Text = "Фильтры";
      this.FiltersTabPage.UseVisualStyleBackColor = true;
      // 
      // EFPReportExtParamsTwoPageForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(632, 452);
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