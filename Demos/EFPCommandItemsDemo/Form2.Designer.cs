namespace EFPCommandItemsDemo
{
  partial class Form2
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
      this.ed1 = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.ed2 = new FreeLibSet.Controls.Int32EditBox();
      this.label3 = new System.Windows.Forms.Label();
      this.ed3 = new FreeLibSet.Controls.DateTimeBox();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnShowDialog = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(17, 18);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "Поле &1";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // ed1
      // 
      this.ed1.Location = new System.Drawing.Point(123, 18);
      this.ed1.Name = "ed1";
      this.ed1.Size = new System.Drawing.Size(261, 20);
      this.ed1.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(17, 55);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(100, 20);
      this.label2.TabIndex = 2;
      this.label2.Text = "Поле &2";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // ed2
      // 
      this.ed2.Increment = 1;
      this.ed2.Location = new System.Drawing.Point(123, 55);
      this.ed2.Name = "ed2";
      this.ed2.Size = new System.Drawing.Size(115, 20);
      this.ed2.TabIndex = 3;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(17, 91);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(100, 20);
      this.label3.TabIndex = 4;
      this.label3.Text = "Поле &3";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // ed3
      // 
      this.ed3.Location = new System.Drawing.Point(123, 91);
      this.ed3.Name = "ed3";
      this.ed3.Size = new System.Drawing.Size(120, 20);
      this.ed3.TabIndex = 5;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(102, 127);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 7;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 127);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 6;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnShowDialog
      // 
      this.btnShowDialog.Location = new System.Drawing.Point(196, 128);
      this.btnShowDialog.Name = "btnShowDialog";
      this.btnShowDialog.Size = new System.Drawing.Size(178, 24);
      this.btnShowDialog.TabIndex = 8;
      this.btnShowDialog.Text = "Показать диалог";
      this.btnShowDialog.UseVisualStyleBackColor = true;
      // 
      // Form2
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(396, 160);
      this.Controls.Add(this.btnShowDialog);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.ed3);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.ed2);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.ed1);
      this.Controls.Add(this.label1);
      this.Name = "Form2";
      this.Text = "Form2";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox ed1;
    private System.Windows.Forms.Label label2;
    private FreeLibSet.Controls.Int32EditBox ed2;
    private System.Windows.Forms.Label label3;
    private FreeLibSet.Controls.DateTimeBox ed3;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnShowDialog;
  }
}
