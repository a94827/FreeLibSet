namespace EFPCommandItemsDemo
{
  partial class NewFormParamForm
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
      this.panel2 = new System.Windows.Forms.Panel();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.rbDialog = new System.Windows.Forms.RadioButton();
      this.rbChild = new System.Windows.Forms.RadioButton();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.rb2 = new System.Windows.Forms.RadioButton();
      this.rb1 = new System.Windows.Forms.RadioButton();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.rb3 = new System.Windows.Forms.RadioButton();
      this.panel2.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.groupBox2);
      this.panel2.Controls.Add(this.groupBox1);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(292, 230);
      this.panel2.TabIndex = 2;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.rbDialog);
      this.groupBox2.Controls.Add(this.rbChild);
      this.groupBox2.Location = new System.Drawing.Point(12, 126);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(200, 81);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Способ показа";
      // 
      // rbDialog
      // 
      this.rbDialog.AutoSize = true;
      this.rbDialog.Location = new System.Drawing.Point(16, 49);
      this.rbDialog.Name = "rbDialog";
      this.rbDialog.Size = new System.Drawing.Size(82, 17);
      this.rbDialog.TabIndex = 1;
      this.rbDialog.TabStop = true;
      this.rbDialog.Text = "ShowDialog";
      this.rbDialog.UseVisualStyleBackColor = true;
      // 
      // rbChild
      // 
      this.rbChild.AutoSize = true;
      this.rbChild.Location = new System.Drawing.Point(16, 26);
      this.rbChild.Name = "rbChild";
      this.rbChild.Size = new System.Drawing.Size(104, 17);
      this.rbChild.TabIndex = 0;
      this.rbChild.TabStop = true;
      this.rbChild.Text = "ShowChildForm()";
      this.rbChild.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.rb3);
      this.groupBox1.Controls.Add(this.rb2);
      this.groupBox1.Controls.Add(this.rb1);
      this.groupBox1.Location = new System.Drawing.Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(200, 108);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Форма";
      // 
      // rb2
      // 
      this.rb2.AutoSize = true;
      this.rb2.Location = new System.Drawing.Point(16, 49);
      this.rb2.Name = "rb2";
      this.rb2.Size = new System.Drawing.Size(112, 17);
      this.rb2.TabIndex = 1;
      this.rb2.TabStop = true;
      this.rb2.Text = "Форма с полями";
      this.rb2.UseVisualStyleBackColor = true;
      // 
      // rb1
      // 
      this.rb1.AutoSize = true;
      this.rb1.Location = new System.Drawing.Point(16, 26);
      this.rb1.Name = "rb1";
      this.rb1.Size = new System.Drawing.Size(130, 17);
      this.rb1.TabIndex = 0;
      this.rb1.TabStop = true;
      this.rb1.Text = "Форма с вкладками";
      this.rb1.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 230);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(292, 40);
      this.panel1.TabIndex = 3;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(94, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(80, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(80, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // rb3
      // 
      this.rb3.AutoSize = true;
      this.rb3.Location = new System.Drawing.Point(16, 72);
      this.rb3.Name = "rb3";
      this.rb3.Size = new System.Drawing.Size(141, 17);
      this.rb3.TabIndex = 2;
      this.rb3.TabStop = true;
      this.rb3.Text = "Форма с одним полем";
      this.rb3.UseVisualStyleBackColor = true;
      // 
      // NewFormParamForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(292, 270);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "NewFormParamForm";
      this.Text = "Показать форму";
      this.panel2.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RadioButton rb2;
    private System.Windows.Forms.RadioButton rb1;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.RadioButton rbDialog;
    private System.Windows.Forms.RadioButton rbChild;
    private System.Windows.Forms.RadioButton rb3;
  }
}
