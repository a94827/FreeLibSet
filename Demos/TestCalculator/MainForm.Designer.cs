namespace TestCalculator
{
  partial class MainForm
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.edExpr = new System.Windows.Forms.RichTextBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.edResType = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.edRes = new System.Windows.Forms.TextBox();
      this.btnDebug = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.lblCheckRes = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.edExpr);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox1.Size = new System.Drawing.Size(669, 68);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Выражение для вычисления";
      // 
      // edExpr
      // 
      this.edExpr.Dock = System.Windows.Forms.DockStyle.Top;
      this.edExpr.Location = new System.Drawing.Point(4, 19);
      this.edExpr.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.edExpr.Multiline = false;
      this.edExpr.Name = "edExpr";
      this.edExpr.Size = new System.Drawing.Size(661, 29);
      this.edExpr.TabIndex = 0;
      this.edExpr.Text = "";
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.edResType);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Controls.Add(this.edRes);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox2.Location = new System.Drawing.Point(0, 68);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.groupBox2.Size = new System.Drawing.Size(669, 85);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Результат";
      // 
      // edResType
      // 
      this.edResType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edResType.Location = new System.Drawing.Point(145, 52);
      this.edResType.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.edResType.Name = "edResType";
      this.edResType.ReadOnly = true;
      this.edResType.Size = new System.Drawing.Size(515, 22);
      this.edResType.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(8, 55);
      this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(85, 17);
      this.label1.TabIndex = 1;
      this.label1.Text = "Тип данных";
      // 
      // edRes
      // 
      this.edRes.Dock = System.Windows.Forms.DockStyle.Top;
      this.edRes.Location = new System.Drawing.Point(4, 19);
      this.edRes.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.edRes.Name = "edRes";
      this.edRes.ReadOnly = true;
      this.edRes.Size = new System.Drawing.Size(661, 22);
      this.edRes.TabIndex = 0;
      // 
      // btnDebug
      // 
      this.btnDebug.Location = new System.Drawing.Point(4, 18);
      this.btnDebug.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.btnDebug.Name = "btnDebug";
      this.btnDebug.Size = new System.Drawing.Size(176, 30);
      this.btnDebug.TabIndex = 2;
      this.btnDebug.Text = "Отладка";
      this.btnDebug.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.lblCheckRes);
      this.panel1.Controls.Add(this.btnDebug);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(0, 153);
      this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(669, 64);
      this.panel1.TabIndex = 3;
      // 
      // lblCheckRes
      // 
      this.lblCheckRes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblCheckRes.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblCheckRes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblCheckRes.ForeColor = System.Drawing.Color.Red;
      this.lblCheckRes.Location = new System.Drawing.Point(188, 11);
      this.lblCheckRes.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblCheckRes.Name = "lblCheckRes";
      this.lblCheckRes.Size = new System.Drawing.Size(473, 43);
      this.lblCheckRes.TabIndex = 3;
      this.lblCheckRes.Text = "???";
      this.lblCheckRes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // MainForm
      // 
      this.AcceptButton = this.btnDebug;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(669, 222);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.Name = "MainForm";
      this.Text = "Тест калькулятора";
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RichTextBox edExpr;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.TextBox edRes;
    private System.Windows.Forms.Button btnDebug;
    private System.Windows.Forms.TextBox edResType;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Label lblCheckRes;
  }
}

