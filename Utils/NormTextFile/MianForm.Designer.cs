namespace NormTextFile
{
  partial class MianForm
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
      this.infoLabel1 = new FreeLibSet.Controls.InfoLabel();
      this.btnStart = new System.Windows.Forms.Button();
      this.cbMask = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cbNested = new System.Windows.Forms.CheckBox();
      this.btnBrowse = new System.Windows.Forms.Button();
      this.cbDir = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.grRes = new System.Windows.Forms.DataGridView();
      this.label3 = new System.Windows.Forms.Label();
      this.cbCodePage = new System.Windows.Forms.ComboBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grRes)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbCodePage);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.infoLabel1);
      this.groupBox1.Controls.Add(this.btnStart);
      this.groupBox1.Controls.Add(this.cbMask);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.cbNested);
      this.groupBox1.Controls.Add(this.btnBrowse);
      this.groupBox1.Controls.Add(this.cbDir);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(622, 175);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Параметры";
      // 
      // infoLabel1
      // 
      this.infoLabel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.infoLabel1.Location = new System.Drawing.Point(3, 148);
      this.infoLabel1.Name = "infoLabel1";
      this.infoLabel1.Size = new System.Drawing.Size(616, 24);
      this.infoLabel1.TabIndex = 9;
      this.infoLabel1.Text = "Файлы сохраняются к кодировке UTF-8 и с концами строк CR+LF";
      // 
      // btnStart
      // 
      this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnStart.Location = new System.Drawing.Point(522, 111);
      this.btnStart.Name = "btnStart";
      this.btnStart.Size = new System.Drawing.Size(88, 24);
      this.btnStart.TabIndex = 8;
      this.btnStart.Text = "Пуск";
      this.btnStart.UseVisualStyleBackColor = true;
      // 
      // cbMask
      // 
      this.cbMask.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbMask.FormattingEnabled = true;
      this.cbMask.Location = new System.Drawing.Point(118, 78);
      this.cbMask.Name = "cbMask";
      this.cbMask.Size = new System.Drawing.Size(391, 24);
      this.cbMask.TabIndex = 5;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(12, 78);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(100, 24);
      this.label2.TabIndex = 4;
      this.label2.Text = "Маска";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbNested
      // 
      this.cbNested.AutoSize = true;
      this.cbNested.Location = new System.Drawing.Point(118, 51);
      this.cbNested.Name = "cbNested";
      this.cbNested.Size = new System.Drawing.Size(213, 21);
      this.cbNested.TabIndex = 3;
      this.cbNested.Text = "Рекурсивный поиск файлов";
      this.cbNested.UseVisualStyleBackColor = true;
      // 
      // btnBrowse
      // 
      this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnBrowse.Location = new System.Drawing.Point(522, 21);
      this.btnBrowse.Name = "btnBrowse";
      this.btnBrowse.Size = new System.Drawing.Size(88, 24);
      this.btnBrowse.TabIndex = 2;
      this.btnBrowse.Text = "Обзор";
      this.btnBrowse.UseVisualStyleBackColor = true;
      // 
      // cbDir
      // 
      this.cbDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbDir.FormattingEnabled = true;
      this.cbDir.Location = new System.Drawing.Point(118, 21);
      this.cbDir.Name = "cbDir";
      this.cbDir.Size = new System.Drawing.Size(391, 24);
      this.cbDir.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(12, 21);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 24);
      this.label1.TabIndex = 0;
      this.label1.Text = "Каталог";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.grRes);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Location = new System.Drawing.Point(0, 175);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(622, 258);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Результат";
      // 
      // grRes
      // 
      this.grRes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grRes.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grRes.Location = new System.Drawing.Point(3, 18);
      this.grRes.Name = "grRes";
      this.grRes.RowTemplate.Height = 24;
      this.grRes.Size = new System.Drawing.Size(616, 237);
      this.grRes.TabIndex = 0;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(12, 112);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(255, 24);
      this.label3.TabIndex = 6;
      this.label3.Text = "Кодировка файлов не UTF-8";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbCodePage
      // 
      this.cbCodePage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbCodePage.FormattingEnabled = true;
      this.cbCodePage.Location = new System.Drawing.Point(273, 112);
      this.cbCodePage.Name = "cbCodePage";
      this.cbCodePage.Size = new System.Drawing.Size(236, 24);
      this.cbCodePage.TabIndex = 7;
      // 
      // MianForm
      // 
      this.AcceptButton = this.btnStart;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(622, 433);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "MianForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
      this.Text = "Нормализация текстовых файлов";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grRes)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button btnBrowse;
    private System.Windows.Forms.ComboBox cbDir;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox2;
    private FreeLibSet.Controls.InfoLabel infoLabel1;
    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.ComboBox cbMask;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.CheckBox cbNested;
    private System.Windows.Forms.DataGridView grRes;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox cbCodePage;
  }
}

