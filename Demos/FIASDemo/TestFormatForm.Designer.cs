namespace FIASDemo
{
  partial class TestFormatForm
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
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.cbFormat = new System.Windows.Forms.ComboBox();
      this.edRes = new System.Windows.Forms.RichTextBox();
      this.btnClose = new System.Windows.Forms.Button();
      this.btnDebugParsing = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 19);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(389, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Введите строку форматирования или выберите простой формат из списка";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 74);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(200, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Результат вызова FiasHandler.Format()";
      // 
      // cbFormat
      // 
      this.cbFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbFormat.FormattingEnabled = true;
      this.cbFormat.Location = new System.Drawing.Point(12, 35);
      this.cbFormat.Name = "cbFormat";
      this.cbFormat.Size = new System.Drawing.Size(520, 21);
      this.cbFormat.TabIndex = 1;
      // 
      // edRes
      // 
      this.edRes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edRes.Location = new System.Drawing.Point(12, 90);
      this.edRes.Name = "edRes";
      this.edRes.ReadOnly = true;
      this.edRes.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
      this.edRes.Size = new System.Drawing.Size(520, 96);
      this.edRes.TabIndex = 3;
      this.edRes.Text = "";
      // 
      // btnClose
      // 
      this.btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnClose.Location = new System.Drawing.Point(12, 202);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(88, 24);
      this.btnClose.TabIndex = 4;
      this.btnClose.Text = "Закрыть";
      this.btnClose.UseVisualStyleBackColor = true;
      // 
      // btnDebugParsing
      // 
      this.btnDebugParsing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDebugParsing.Location = new System.Drawing.Point(500, 202);
      this.btnDebugParsing.Name = "btnDebugParsing";
      this.btnDebugParsing.Size = new System.Drawing.Size(32, 24);
      this.btnDebugParsing.TabIndex = 5;
      this.btnDebugParsing.UseVisualStyleBackColor = true;
      // 
      // TestFormatForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnClose;
      this.ClientSize = new System.Drawing.Size(544, 232);
      this.Controls.Add(this.btnDebugParsing);
      this.Controls.Add(this.btnClose);
      this.Controls.Add(this.edRes);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.cbFormat);
      this.Name = "TestFormatForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Тестирование метода FiasHandler.Format()";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox cbFormat;
    private System.Windows.Forms.RichTextBox edRes;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Button btnDebugParsing;
  }
}