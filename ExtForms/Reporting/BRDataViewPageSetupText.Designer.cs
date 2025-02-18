namespace FreeLibSet.Forms.Reporting
{
  partial class BRDataViewPageSetupText
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BRDataViewPageSetupText));
      this.MainPanel = new System.Windows.Forms.Panel();
      this.grpMain = new System.Windows.Forms.GroupBox();
      this.cbExpColumnHeaders = new System.Windows.Forms.CheckBox();
      this.cbRemoveDoubleSpaces = new System.Windows.Forms.CheckBox();
      this.cbSingleLineField = new System.Windows.Forms.CheckBox();
      this.cbQuote = new System.Windows.Forms.ComboBox();
      this.lblQuote = new System.Windows.Forms.Label();
      this.cbFieldDelimiter = new System.Windows.Forms.ComboBox();
      this.lblFieldDelimiter = new System.Windows.Forms.Label();
      this.cbCodePage = new System.Windows.Forms.ComboBox();
      this.lblCodePage = new System.Windows.Forms.Label();
      this.MainPanel.SuspendLayout();
      this.grpMain.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainPanel
      // 
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Controls.Add(this.grpMain);
      this.MainPanel.Name = "MainPanel";
      // 
      // grpMain
      // 
      resources.ApplyResources(this.grpMain, "grpMain");
      this.grpMain.Controls.Add(this.cbExpColumnHeaders);
      this.grpMain.Controls.Add(this.cbRemoveDoubleSpaces);
      this.grpMain.Controls.Add(this.cbSingleLineField);
      this.grpMain.Controls.Add(this.cbQuote);
      this.grpMain.Controls.Add(this.lblQuote);
      this.grpMain.Controls.Add(this.cbFieldDelimiter);
      this.grpMain.Controls.Add(this.lblFieldDelimiter);
      this.grpMain.Controls.Add(this.cbCodePage);
      this.grpMain.Controls.Add(this.lblCodePage);
      this.grpMain.Name = "grpMain";
      this.grpMain.TabStop = false;
      // 
      // cbExpColumnHeaders
      // 
      resources.ApplyResources(this.cbExpColumnHeaders, "cbExpColumnHeaders");
      this.cbExpColumnHeaders.Name = "cbExpColumnHeaders";
      this.cbExpColumnHeaders.UseVisualStyleBackColor = true;
      // 
      // cbRemoveDoubleSpaces
      // 
      resources.ApplyResources(this.cbRemoveDoubleSpaces, "cbRemoveDoubleSpaces");
      this.cbRemoveDoubleSpaces.Name = "cbRemoveDoubleSpaces";
      this.cbRemoveDoubleSpaces.UseVisualStyleBackColor = true;
      // 
      // cbSingleLineField
      // 
      resources.ApplyResources(this.cbSingleLineField, "cbSingleLineField");
      this.cbSingleLineField.Name = "cbSingleLineField";
      this.cbSingleLineField.UseVisualStyleBackColor = true;
      // 
      // cbQuote
      // 
      resources.ApplyResources(this.cbQuote, "cbQuote");
      this.cbQuote.FormattingEnabled = true;
      this.cbQuote.Items.AddRange(new object[] {
            resources.GetString("cbQuote.Items"),
            resources.GetString("cbQuote.Items1")});
      this.cbQuote.Name = "cbQuote";
      // 
      // lblQuote
      // 
      resources.ApplyResources(this.lblQuote, "lblQuote");
      this.lblQuote.Name = "lblQuote";
      // 
      // cbFieldDelimiter
      // 
      resources.ApplyResources(this.cbFieldDelimiter, "cbFieldDelimiter");
      this.cbFieldDelimiter.FormattingEnabled = true;
      this.cbFieldDelimiter.Items.AddRange(new object[] {
            resources.GetString("cbFieldDelimiter.Items"),
            resources.GetString("cbFieldDelimiter.Items1")});
      this.cbFieldDelimiter.Name = "cbFieldDelimiter";
      // 
      // lblFieldDelimiter
      // 
      resources.ApplyResources(this.lblFieldDelimiter, "lblFieldDelimiter");
      this.lblFieldDelimiter.Name = "lblFieldDelimiter";
      // 
      // cbCodePage
      // 
      resources.ApplyResources(this.cbCodePage, "cbCodePage");
      this.cbCodePage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbCodePage.FormattingEnabled = true;
      this.cbCodePage.Name = "cbCodePage";
      // 
      // lblCodePage
      // 
      resources.ApplyResources(this.lblCodePage, "lblCodePage");
      this.lblCodePage.Name = "lblCodePage";
      // 
      // BRDataViewPageSetupText
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.MainPanel);
      this.Name = "BRDataViewPageSetupText";
      this.MainPanel.ResumeLayout(false);
      this.grpMain.ResumeLayout(false);
      this.grpMain.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel MainPanel;
    private System.Windows.Forms.GroupBox grpMain;
    private System.Windows.Forms.ComboBox cbQuote;
    private System.Windows.Forms.Label lblQuote;
    private System.Windows.Forms.ComboBox cbFieldDelimiter;
    private System.Windows.Forms.Label lblFieldDelimiter;
    private System.Windows.Forms.ComboBox cbCodePage;
    private System.Windows.Forms.Label lblCodePage;
    private System.Windows.Forms.CheckBox cbRemoveDoubleSpaces;
    private System.Windows.Forms.CheckBox cbSingleLineField;
    private System.Windows.Forms.CheckBox cbExpColumnHeaders;
  }
}