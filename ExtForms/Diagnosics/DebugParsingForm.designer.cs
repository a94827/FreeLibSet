namespace FreeLibSet.Forms.Diagnostics
{
  partial class DebugParsingForm
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
      this.grpText = new System.Windows.Forms.GroupBox();
      this.edText = new System.Windows.Forms.RichTextBox();
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.tpTokens = new System.Windows.Forms.TabPage();
      this.grTokens = new System.Windows.Forms.DataGridView();
      this.tpExpr = new System.Windows.Forms.TabPage();
      this.tvExpr = new System.Windows.Forms.TreeView();
      this.tpErrors = new System.Windows.Forms.TabPage();
      this.grErrors = new System.Windows.Forms.DataGridView();
      this.PanSpbErrors = new System.Windows.Forms.Panel();
      this.grpText.SuspendLayout();
      this.TheTabControl.SuspendLayout();
      this.tpTokens.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grTokens)).BeginInit();
      this.tpExpr.SuspendLayout();
      this.tpErrors.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grErrors)).BeginInit();
      this.SuspendLayout();
      // 
      // grpText
      // 
      this.grpText.Controls.Add(this.edText);
      this.grpText.Dock = System.Windows.Forms.DockStyle.Top;
      this.grpText.Location = new System.Drawing.Point(0, 0);
      this.grpText.Name = "grpText";
      this.grpText.Size = new System.Drawing.Size(672, 54);
      this.grpText.TabIndex = 1;
      this.grpText.TabStop = false;
      this.grpText.Text = "Parsed text";
      // 
      // edText
      // 
      this.edText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edText.Location = new System.Drawing.Point(6, 17);
      this.edText.Multiline = false;
      this.edText.Name = "edText";
      this.edText.ReadOnly = true;
      this.edText.Size = new System.Drawing.Size(660, 25);
      this.edText.TabIndex = 1;
      this.edText.Text = "";
      // 
      // TheTabControl
      // 
      this.TheTabControl.Controls.Add(this.tpTokens);
      this.TheTabControl.Controls.Add(this.tpExpr);
      this.TheTabControl.Controls.Add(this.tpErrors);
      this.TheTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTabControl.Location = new System.Drawing.Point(0, 54);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(672, 276);
      this.TheTabControl.TabIndex = 2;
      // 
      // tpTokens
      // 
      this.tpTokens.Controls.Add(this.grTokens);
      this.tpTokens.Location = new System.Drawing.Point(4, 22);
      this.tpTokens.Name = "tpTokens";
      this.tpTokens.Padding = new System.Windows.Forms.Padding(3);
      this.tpTokens.Size = new System.Drawing.Size(664, 250);
      this.tpTokens.TabIndex = 0;
      this.tpTokens.Text = "Tokens";
      this.tpTokens.UseVisualStyleBackColor = true;
      // 
      // grTokens
      // 
      this.grTokens.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grTokens.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grTokens.Location = new System.Drawing.Point(3, 3);
      this.grTokens.Name = "grTokens";
      this.grTokens.Size = new System.Drawing.Size(658, 244);
      this.grTokens.TabIndex = 1;
      // 
      // tpExpr
      // 
      this.tpExpr.Controls.Add(this.tvExpr);
      this.tpExpr.Location = new System.Drawing.Point(4, 22);
      this.tpExpr.Name = "tpExpr";
      this.tpExpr.Padding = new System.Windows.Forms.Padding(3);
      this.tpExpr.Size = new System.Drawing.Size(664, 250);
      this.tpExpr.TabIndex = 1;
      this.tpExpr.Text = "Expressions";
      this.tpExpr.UseVisualStyleBackColor = true;
      // 
      // tvExpr
      // 
      this.tvExpr.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tvExpr.Location = new System.Drawing.Point(3, 3);
      this.tvExpr.Name = "tvExpr";
      this.tvExpr.ShowNodeToolTips = true;
      this.tvExpr.Size = new System.Drawing.Size(658, 244);
      this.tvExpr.TabIndex = 0;
      // 
      // tpErrors
      // 
      this.tpErrors.Controls.Add(this.grErrors);
      this.tpErrors.Controls.Add(this.PanSpbErrors);
      this.tpErrors.Location = new System.Drawing.Point(4, 22);
      this.tpErrors.Name = "tpErrors";
      this.tpErrors.Padding = new System.Windows.Forms.Padding(3);
      this.tpErrors.Size = new System.Drawing.Size(664, 250);
      this.tpErrors.TabIndex = 2;
      this.tpErrors.Text = "Errors";
      this.tpErrors.UseVisualStyleBackColor = true;
      // 
      // grErrors
      // 
      this.grErrors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grErrors.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grErrors.Location = new System.Drawing.Point(3, 49);
      this.grErrors.Name = "grErrors";
      this.grErrors.Size = new System.Drawing.Size(658, 198);
      this.grErrors.TabIndex = 1;
      // 
      // PanSpbErrors
      // 
      this.PanSpbErrors.Dock = System.Windows.Forms.DockStyle.Top;
      this.PanSpbErrors.Location = new System.Drawing.Point(3, 3);
      this.PanSpbErrors.Name = "PanSpbErrors";
      this.PanSpbErrors.Size = new System.Drawing.Size(658, 46);
      this.PanSpbErrors.TabIndex = 0;
      // 
      // DebugParsingForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(672, 330);
      this.Controls.Add(this.TheTabControl);
      this.Controls.Add(this.grpText);
      this.Name = "DebugParsingForm";
      this.Text = "Expression view";
      this.grpText.ResumeLayout(false);
      this.TheTabControl.ResumeLayout(false);
      this.tpTokens.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grTokens)).EndInit();
      this.tpExpr.ResumeLayout(false);
      this.tpErrors.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grErrors)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox grpText;
    private System.Windows.Forms.RichTextBox edText;
    private System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.TabPage tpTokens;
    private System.Windows.Forms.DataGridView grTokens;
    private System.Windows.Forms.TabPage tpExpr;
    private System.Windows.Forms.TreeView tvExpr;
    private System.Windows.Forms.TabPage tpErrors;
    private System.Windows.Forms.Panel PanSpbErrors;
    private System.Windows.Forms.DataGridView grErrors;

  }
}
