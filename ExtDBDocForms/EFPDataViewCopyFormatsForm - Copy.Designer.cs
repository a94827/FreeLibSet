namespace FreeLibSet.Forms
{
  partial class EFPDataViewCopyFormatsForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EFPDataViewCopyFormatsForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.infoLabel1 = new FreeLibSet.Controls.InfoLabel();
      this.cbHtml = new System.Windows.Forms.CheckBox();
      this.cbCsv = new System.Windows.Forms.CheckBox();
      this.cbText = new System.Windows.Forms.CheckBox();
      this.panel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 349);
      this.panel1.Margin = new System.Windows.Forms.Padding(4);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(521, 49);
      this.panel1.TabIndex = 1;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(136, 10);
      this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(117, 30);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(11, 10);
      this.btnOk.Margin = new System.Windows.Forms.Padding(4);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(117, 30);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.infoLabel1);
      this.groupBox1.Controls.Add(this.cbHtml);
      this.groupBox1.Controls.Add(this.cbCsv);
      this.groupBox1.Controls.Add(this.cbText);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
      this.groupBox1.Size = new System.Drawing.Size(521, 349);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Форматы копирования";
      // 
      // infoLabel1
      // 
      this.infoLabel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.infoLabel1.Location = new System.Drawing.Point(4, 186);
      this.infoLabel1.Margin = new System.Windows.Forms.Padding(4);
      this.infoLabel1.Name = "infoLabel1";
      this.infoLabel1.Size = new System.Drawing.Size(513, 159);
      this.infoLabel1.TabIndex = 3;
      this.infoLabel1.Text = resources.GetString("infoLabel1.Text");
      // 
      // cbHtml
      // 
      this.cbHtml.AutoSize = true;
      this.cbHtml.Location = new System.Drawing.Point(27, 134);
      this.cbHtml.Margin = new System.Windows.Forms.Padding(4);
      this.cbHtml.Name = "cbHtml";
      this.cbHtml.Size = new System.Drawing.Size(68, 21);
      this.cbHtml.TabIndex = 2;
      this.cbHtml.Text = "HTML";
      this.cbHtml.UseVisualStyleBackColor = true;
      // 
      // cbCsv
      // 
      this.cbCsv.AutoSize = true;
      this.cbCsv.Location = new System.Drawing.Point(27, 87);
      this.cbCsv.Margin = new System.Windows.Forms.Padding(4);
      this.cbCsv.Name = "cbCsv";
      this.cbCsv.Size = new System.Drawing.Size(57, 21);
      this.cbCsv.TabIndex = 1;
      this.cbCsv.Text = "CSV";
      this.cbCsv.UseVisualStyleBackColor = true;
      // 
      // cbText
      // 
      this.cbText.AutoSize = true;
      this.cbText.Location = new System.Drawing.Point(27, 43);
      this.cbText.Margin = new System.Windows.Forms.Padding(4);
      this.cbText.Name = "cbText";
      this.cbText.Size = new System.Drawing.Size(68, 21);
      this.cbText.TabIndex = 0;
      this.cbText.Text = "Текст";
      this.cbText.UseVisualStyleBackColor = true;
      // 
      // EFPDataViewCopyFormatsForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(521, 398);
      this.Controls.Add(this.groupBox1);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.Margin = new System.Windows.Forms.Padding(4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "EFPDataViewCopyFormatsForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Настройка копирования";
      this.panel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.GroupBox groupBox1;
    private Controls.InfoLabel infoLabel1;
    private System.Windows.Forms.CheckBox cbHtml;
    private System.Windows.Forms.CheckBox cbCsv;
    private System.Windows.Forms.CheckBox cbText;
  }
}