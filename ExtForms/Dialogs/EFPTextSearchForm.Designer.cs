#pragma warning disable 1591

namespace FreeLibSet.Forms
{
  partial class EFPTextSearchForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EFPTextSearchForm));
      this.grpFrom = new System.Windows.Forms.GroupBox();
      this.rbFromCurr = new System.Windows.Forms.RadioButton();
      this.rbFromStart = new System.Windows.Forms.RadioButton();
      this.grpWhere = new System.Windows.Forms.GroupBox();
      this.rbCurrCol = new System.Windows.Forms.RadioButton();
      this.rbAllCols = new System.Windows.Forms.RadioButton();
      this.grpDirection = new System.Windows.Forms.GroupBox();
      this.rbBackward = new System.Windows.Forms.RadioButton();
      this.rbForward = new System.Windows.Forms.RadioButton();
      this.grpConditions = new System.Windows.Forms.GroupBox();
      this.cbWhole = new System.Windows.Forms.CheckBox();
      this.cbSimilarCharsDiff = new System.Windows.Forms.CheckBox();
      this.cbCaseSens = new System.Windows.Forms.CheckBox();
      this.grpText = new System.Windows.Forms.GroupBox();
      this.lblText = new System.Windows.Forms.Label();
      this.cbText = new System.Windows.Forms.ComboBox();
      this.MainPanel.SuspendLayout();
      this.grpFrom.SuspendLayout();
      this.grpWhere.SuspendLayout();
      this.grpDirection.SuspendLayout();
      this.grpConditions.SuspendLayout();
      this.grpText.SuspendLayout();
      this.SuspendLayout();
      // 
      // ButtonsPanel
      // 
      resources.ApplyResources(this.ButtonsPanel, "ButtonsPanel");
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.grpFrom);
      this.MainPanel.Controls.Add(this.grpWhere);
      this.MainPanel.Controls.Add(this.grpDirection);
      this.MainPanel.Controls.Add(this.grpConditions);
      this.MainPanel.Controls.Add(this.grpText);
      resources.ApplyResources(this.MainPanel, "MainPanel");
      // 
      // grpFrom
      // 
      this.grpFrom.Controls.Add(this.rbFromCurr);
      this.grpFrom.Controls.Add(this.rbFromStart);
      resources.ApplyResources(this.grpFrom, "grpFrom");
      this.grpFrom.Name = "grpFrom";
      this.grpFrom.TabStop = false;
      // 
      // rbFromCurr
      // 
      resources.ApplyResources(this.rbFromCurr, "rbFromCurr");
      this.rbFromCurr.Name = "rbFromCurr";
      this.rbFromCurr.TabStop = true;
      this.rbFromCurr.UseVisualStyleBackColor = true;
      // 
      // rbFromStart
      // 
      resources.ApplyResources(this.rbFromStart, "rbFromStart");
      this.rbFromStart.Name = "rbFromStart";
      this.rbFromStart.TabStop = true;
      this.rbFromStart.UseVisualStyleBackColor = true;
      // 
      // grpWhere
      // 
      this.grpWhere.Controls.Add(this.rbCurrCol);
      this.grpWhere.Controls.Add(this.rbAllCols);
      resources.ApplyResources(this.grpWhere, "grpWhere");
      this.grpWhere.Name = "grpWhere";
      this.grpWhere.TabStop = false;
      // 
      // rbCurrCol
      // 
      resources.ApplyResources(this.rbCurrCol, "rbCurrCol");
      this.rbCurrCol.Name = "rbCurrCol";
      this.rbCurrCol.TabStop = true;
      this.rbCurrCol.UseVisualStyleBackColor = true;
      // 
      // rbAllCols
      // 
      resources.ApplyResources(this.rbAllCols, "rbAllCols");
      this.rbAllCols.Name = "rbAllCols";
      this.rbAllCols.TabStop = true;
      this.rbAllCols.UseVisualStyleBackColor = true;
      // 
      // grpDirection
      // 
      this.grpDirection.Controls.Add(this.rbBackward);
      this.grpDirection.Controls.Add(this.rbForward);
      resources.ApplyResources(this.grpDirection, "grpDirection");
      this.grpDirection.Name = "grpDirection";
      this.grpDirection.TabStop = false;
      // 
      // rbBackward
      // 
      resources.ApplyResources(this.rbBackward, "rbBackward");
      this.rbBackward.Name = "rbBackward";
      this.rbBackward.TabStop = true;
      this.rbBackward.UseVisualStyleBackColor = true;
      // 
      // rbForward
      // 
      resources.ApplyResources(this.rbForward, "rbForward");
      this.rbForward.Name = "rbForward";
      this.rbForward.TabStop = true;
      this.rbForward.UseVisualStyleBackColor = true;
      // 
      // grpConditions
      // 
      this.grpConditions.Controls.Add(this.cbWhole);
      this.grpConditions.Controls.Add(this.cbSimilarCharsDiff);
      this.grpConditions.Controls.Add(this.cbCaseSens);
      resources.ApplyResources(this.grpConditions, "grpConditions");
      this.grpConditions.Name = "grpConditions";
      this.grpConditions.TabStop = false;
      // 
      // cbWhole
      // 
      resources.ApplyResources(this.cbWhole, "cbWhole");
      this.cbWhole.Name = "cbWhole";
      this.cbWhole.UseVisualStyleBackColor = true;
      // 
      // cbSimilarCharsDiff
      // 
      resources.ApplyResources(this.cbSimilarCharsDiff, "cbSimilarCharsDiff");
      this.cbSimilarCharsDiff.Name = "cbSimilarCharsDiff";
      this.cbSimilarCharsDiff.UseVisualStyleBackColor = true;
      // 
      // cbCaseSens
      // 
      resources.ApplyResources(this.cbCaseSens, "cbCaseSens");
      this.cbCaseSens.Name = "cbCaseSens";
      this.cbCaseSens.UseVisualStyleBackColor = true;
      // 
      // grpText
      // 
      this.grpText.Controls.Add(this.lblText);
      this.grpText.Controls.Add(this.cbText);
      resources.ApplyResources(this.grpText, "grpText");
      this.grpText.Name = "grpText";
      this.grpText.TabStop = false;
      // 
      // lblText
      // 
      resources.ApplyResources(this.lblText, "lblText");
      this.lblText.Name = "lblText";
      // 
      // cbText
      // 
      resources.ApplyResources(this.cbText, "cbText");
      this.cbText.FormattingEnabled = true;
      this.cbText.Name = "cbText";
      // 
      // EFPTextSearchForm
      // 
      resources.ApplyResources(this, "$this");
      this.Name = "EFPTextSearchForm";
      this.MainPanel.ResumeLayout(false);
      this.grpFrom.ResumeLayout(false);
      this.grpFrom.PerformLayout();
      this.grpWhere.ResumeLayout(false);
      this.grpWhere.PerformLayout();
      this.grpDirection.ResumeLayout(false);
      this.grpDirection.PerformLayout();
      this.grpConditions.ResumeLayout(false);
      this.grpConditions.PerformLayout();
      this.grpText.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.RadioButton rbFromCurr;
    private System.Windows.Forms.RadioButton rbFromStart;
    private System.Windows.Forms.RadioButton rbCurrCol;
    private System.Windows.Forms.RadioButton rbAllCols;
    private System.Windows.Forms.RadioButton rbBackward;
    private System.Windows.Forms.RadioButton rbForward;
    private System.Windows.Forms.CheckBox cbWhole;
    private System.Windows.Forms.CheckBox cbSimilarCharsDiff;
    private System.Windows.Forms.CheckBox cbCaseSens;
    private System.Windows.Forms.GroupBox grpText;
    private System.Windows.Forms.Label lblText;
    private System.Windows.Forms.ComboBox cbText;
    public System.Windows.Forms.GroupBox grpWhere;
    public System.Windows.Forms.GroupBox grpFrom;
    public System.Windows.Forms.GroupBox grpDirection;
    public System.Windows.Forms.GroupBox grpConditions;

  }
}
