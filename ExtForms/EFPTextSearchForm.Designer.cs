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
      this.label1 = new System.Windows.Forms.Label();
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
      this.ButtonsPanel.Location = new System.Drawing.Point(0, 231);
      this.ButtonsPanel.Size = new System.Drawing.Size(411, 40);
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.grpFrom);
      this.MainPanel.Controls.Add(this.grpWhere);
      this.MainPanel.Controls.Add(this.grpDirection);
      this.MainPanel.Controls.Add(this.grpConditions);
      this.MainPanel.Controls.Add(this.grpText);
      this.MainPanel.Size = new System.Drawing.Size(411, 231);
      // 
      // grpFrom
      // 
      this.grpFrom.Controls.Add(this.rbFromCurr);
      this.grpFrom.Controls.Add(this.rbFromStart);
      this.grpFrom.Location = new System.Drawing.Point(220, 158);
      this.grpFrom.Name = "grpFrom";
      this.grpFrom.Size = new System.Drawing.Size(176, 68);
      this.grpFrom.TabIndex = 9;
      this.grpFrom.TabStop = false;
      this.grpFrom.Text = "Откуда начать";
      // 
      // rbFromCurr
      // 
      this.rbFromCurr.AutoSize = true;
      this.rbFromCurr.Location = new System.Drawing.Point(11, 42);
      this.rbFromCurr.Name = "rbFromCurr";
      this.rbFromCurr.Size = new System.Drawing.Size(116, 17);
      this.rbFromCurr.TabIndex = 1;
      this.rbFromCurr.TabStop = true;
      this.rbFromCurr.Text = "С тек&ущей строки";
      this.rbFromCurr.UseVisualStyleBackColor = true;
      // 
      // rbFromStart
      // 
      this.rbFromStart.AutoSize = true;
      this.rbFromStart.Location = new System.Drawing.Point(11, 19);
      this.rbFromStart.Name = "rbFromStart";
      this.rbFromStart.Size = new System.Drawing.Size(123, 17);
      this.rbFromStart.TabIndex = 0;
      this.rbFromStart.TabStop = true;
      this.rbFromStart.Text = "&С начала /  с конца";
      this.rbFromStart.UseVisualStyleBackColor = true;
      // 
      // grpWhere
      // 
      this.grpWhere.Controls.Add(this.rbCurrCol);
      this.grpWhere.Controls.Add(this.rbAllCols);
      this.grpWhere.Location = new System.Drawing.Point(14, 158);
      this.grpWhere.Name = "grpWhere";
      this.grpWhere.Size = new System.Drawing.Size(200, 68);
      this.grpWhere.TabIndex = 8;
      this.grpWhere.TabStop = false;
      this.grpWhere.Text = "Где искать";
      // 
      // rbCurrCol
      // 
      this.rbCurrCol.AutoSize = true;
      this.rbCurrCol.Location = new System.Drawing.Point(9, 42);
      this.rbCurrCol.Name = "rbCurrCol";
      this.rbCurrCol.Size = new System.Drawing.Size(124, 17);
      this.rbCurrCol.TabIndex = 1;
      this.rbCurrCol.TabStop = true;
      this.rbCurrCol.Text = "В текущем стол&бце";
      this.rbCurrCol.UseVisualStyleBackColor = true;
      // 
      // rbAllCols
      // 
      this.rbAllCols.AutoSize = true;
      this.rbAllCols.Location = new System.Drawing.Point(9, 19);
      this.rbAllCols.Name = "rbAllCols";
      this.rbAllCols.Size = new System.Drawing.Size(113, 17);
      this.rbAllCols.TabIndex = 0;
      this.rbAllCols.TabStop = true;
      this.rbAllCols.Text = "Во &всех столбцах";
      this.rbAllCols.UseVisualStyleBackColor = true;
      // 
      // grpDirection
      // 
      this.grpDirection.Controls.Add(this.rbBackward);
      this.grpDirection.Controls.Add(this.rbForward);
      this.grpDirection.Location = new System.Drawing.Point(220, 61);
      this.grpDirection.Name = "grpDirection";
      this.grpDirection.Size = new System.Drawing.Size(176, 68);
      this.grpDirection.TabIndex = 7;
      this.grpDirection.TabStop = false;
      this.grpDirection.Text = "Направление поиска";
      // 
      // rbBackward
      // 
      this.rbBackward.AutoSize = true;
      this.rbBackward.Location = new System.Drawing.Point(11, 41);
      this.rbBackward.Name = "rbBackward";
      this.rbBackward.Size = new System.Drawing.Size(57, 17);
      this.rbBackward.TabIndex = 1;
      this.rbBackward.TabStop = true;
      this.rbBackward.Text = "На&зад";
      this.rbBackward.UseVisualStyleBackColor = true;
      // 
      // rbForward
      // 
      this.rbForward.AutoSize = true;
      this.rbForward.Location = new System.Drawing.Point(11, 21);
      this.rbForward.Name = "rbForward";
      this.rbForward.Size = new System.Drawing.Size(62, 17);
      this.rbForward.TabIndex = 0;
      this.rbForward.TabStop = true;
      this.rbForward.Text = "Вп&еред";
      this.rbForward.UseVisualStyleBackColor = true;
      // 
      // grpConditions
      // 
      this.grpConditions.Controls.Add(this.cbWhole);
      this.grpConditions.Controls.Add(this.cbSimilarCharsDiff);
      this.grpConditions.Controls.Add(this.cbCaseSens);
      this.grpConditions.Location = new System.Drawing.Point(14, 61);
      this.grpConditions.Name = "grpConditions";
      this.grpConditions.Size = new System.Drawing.Size(200, 91);
      this.grpConditions.TabIndex = 6;
      this.grpConditions.TabStop = false;
      this.grpConditions.Text = "Условия поиска";
      // 
      // cbWhole
      // 
      this.cbWhole.AutoSize = true;
      this.cbWhole.Location = new System.Drawing.Point(9, 65);
      this.cbWhole.Name = "cbWhole";
      this.cbWhole.Size = new System.Drawing.Size(109, 17);
      this.cbWhole.TabIndex = 2;
      this.cbWhole.Text = "Строка &целиком";
      this.cbWhole.UseVisualStyleBackColor = true;
      // 
      // cbSimilarCharsDiff
      // 
      this.cbSimilarCharsDiff.AutoSize = true;
      this.cbSimilarCharsDiff.Location = new System.Drawing.Point(9, 42);
      this.cbSimilarCharsDiff.Name = "cbSimilarCharsDiff";
      this.cbSimilarCharsDiff.Size = new System.Drawing.Size(153, 17);
      this.cbSimilarCharsDiff.TabIndex = 1;
      this.cbSimilarCharsDiff.Text = "Отличать похо&жие буквы";
      this.cbSimilarCharsDiff.UseVisualStyleBackColor = true;
      // 
      // cbCaseSens
      // 
      this.cbCaseSens.AutoSize = true;
      this.cbCaseSens.Location = new System.Drawing.Point(9, 19);
      this.cbCaseSens.Name = "cbCaseSens";
      this.cbCaseSens.Size = new System.Drawing.Size(120, 17);
      this.cbCaseSens.TabIndex = 0;
      this.cbCaseSens.Text = "С учетом &регистра";
      this.cbCaseSens.UseVisualStyleBackColor = true;
      // 
      // grpText
      // 
      this.grpText.Controls.Add(this.label1);
      this.grpText.Controls.Add(this.cbText);
      this.grpText.Location = new System.Drawing.Point(14, 8);
      this.grpText.Name = "grpText";
      this.grpText.Size = new System.Drawing.Size(382, 47);
      this.grpText.TabIndex = 5;
      this.grpText.TabStop = false;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(6, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(88, 21);
      this.label1.TabIndex = 0;
      this.label1.Text = "Искать &текст";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbText
      // 
      this.cbText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbText.FormattingEnabled = true;
      this.cbText.Location = new System.Drawing.Point(100, 16);
      this.cbText.Name = "cbText";
      this.cbText.Size = new System.Drawing.Size(275, 21);
      this.cbText.TabIndex = 1;
      // 
      // EFPTextSearchForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(411, 271);
      this.MinimumSize = new System.Drawing.Size(352, 84);
      this.Name = "EFPTextSearchForm";
      this.Text = "Поиск текста";
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
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ComboBox cbText;
    public System.Windows.Forms.GroupBox grpWhere;
    public System.Windows.Forms.GroupBox grpFrom;
    public System.Windows.Forms.GroupBox grpDirection;
    public System.Windows.Forms.GroupBox grpConditions;

  }
}