namespace Bmp2Png
{
  partial class ParamForm
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
      this.infoLabel1 = new FreeLibSet.Controls.InfoLabel();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbSubDirs = new System.Windows.Forms.CheckBox();
      this.btnBrowseResDir = new System.Windows.Forms.Button();
      this.edResDir = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.btnBrowseSrcDir = new System.Windows.Forms.Button();
      this.edSrcDir = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // infoLabel1
      // 
      this.infoLabel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.infoLabel1.Location = new System.Drawing.Point(0, 187);
      this.infoLabel1.Name = "infoLabel1";
      this.infoLabel1.Size = new System.Drawing.Size(624, 48);
      this.infoLabel1.TabIndex = 2;
      this.infoLabel1.Text = "Преобразует 16-цветные значки *.bmp с фоновым цветом Magenta в изображеения *.png" +
          "\r\nТакже преобразуются файлы *.ico";
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 147);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(624, 40);
      this.panel1.TabIndex = 1;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(102, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbSubDirs);
      this.groupBox1.Controls.Add(this.btnBrowseResDir);
      this.groupBox1.Controls.Add(this.edResDir);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.btnBrowseSrcDir);
      this.groupBox1.Controls.Add(this.edSrcDir);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(624, 147);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // cbSubDirs
      // 
      this.cbSubDirs.AutoSize = true;
      this.cbSubDirs.Location = new System.Drawing.Point(8, 122);
      this.cbSubDirs.Name = "cbSubDirs";
      this.cbSubDirs.Size = new System.Drawing.Size(154, 17);
      this.cbSubDirs.TabIndex = 6;
      this.cbSubDirs.Text = "Обработка подкаталогов";
      this.cbSubDirs.UseVisualStyleBackColor = true;
      // 
      // btnBrowseResDir
      // 
      this.btnBrowseResDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnBrowseResDir.Location = new System.Drawing.Point(531, 82);
      this.btnBrowseResDir.Name = "btnBrowseResDir";
      this.btnBrowseResDir.Size = new System.Drawing.Size(88, 24);
      this.btnBrowseResDir.TabIndex = 5;
      this.btnBrowseResDir.Text = "Обзор";
      this.btnBrowseResDir.UseVisualStyleBackColor = true;
      // 
      // edResDir
      // 
      this.edResDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edResDir.Location = new System.Drawing.Point(9, 85);
      this.edResDir.Name = "edResDir";
      this.edResDir.Size = new System.Drawing.Size(516, 20);
      this.edResDir.TabIndex = 4;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(6, 69);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(139, 13);
      this.label2.TabIndex = 3;
      this.label2.Text = "Каталог для записи *..png";
      // 
      // btnBrowseSrcDir
      // 
      this.btnBrowseSrcDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnBrowseSrcDir.Location = new System.Drawing.Point(530, 29);
      this.btnBrowseSrcDir.Name = "btnBrowseSrcDir";
      this.btnBrowseSrcDir.Size = new System.Drawing.Size(88, 24);
      this.btnBrowseSrcDir.TabIndex = 2;
      this.btnBrowseSrcDir.Text = "Обзор";
      this.btnBrowseSrcDir.UseVisualStyleBackColor = true;
      // 
      // edSrcDir
      // 
      this.edSrcDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edSrcDir.Location = new System.Drawing.Point(8, 32);
      this.edSrcDir.Name = "edSrcDir";
      this.edSrcDir.Size = new System.Drawing.Size(516, 20);
      this.edSrcDir.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(5, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(232, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Каталог с исходными изображениями *.bmp";
      // 
      // ParamForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(624, 235);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.infoLabel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ParamForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Преобразование значков bmp в png";
      this.panel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private FreeLibSet.Controls.InfoLabel infoLabel1;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnBrowseResDir;
    private System.Windows.Forms.TextBox edResDir;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btnBrowseSrcDir;
    private System.Windows.Forms.TextBox edSrcDir;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.CheckBox cbSubDirs;
  }
}

