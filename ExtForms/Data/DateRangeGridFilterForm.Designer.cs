namespace FreeLibSet.Data
{
  partial class DateRangeGridFilterForm
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
      this.edRange = new FreeLibSet.Controls.DateRangeBox();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnNo = new System.Windows.Forms.Button();
      this.rbRange = new System.Windows.Forms.RadioButton();
      this.rbNotNull = new System.Windows.Forms.RadioButton();
      this.rbNull = new System.Windows.Forms.RadioButton();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // edRange
      // 
      this.edRange.Location = new System.Drawing.Point(47, 59);
      this.edRange.Margin = new System.Windows.Forms.Padding(5);
      this.edRange.Name = "edRange";
      this.edRange.Size = new System.Drawing.Size(459, 37);
      this.edRange.TabIndex = 3;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
      this.btnCancel.Location = new System.Drawing.Point(8, 40);
      this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Margin = new System.Windows.Forms.Padding(4);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "OK";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnNo);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
      this.panel1.Location = new System.Drawing.Point(529, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(106, 160);
      this.panel1.TabIndex = 4;
      // 
      // btnNo
      // 
      this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
      this.btnNo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
      this.btnNo.Location = new System.Drawing.Point(8, 72);
      this.btnNo.Margin = new System.Windows.Forms.Padding(4);
      this.btnNo.Name = "btnNo";
      this.btnNo.Size = new System.Drawing.Size(88, 24);
      this.btnNo.TabIndex = 2;
      this.btnNo.Text = "No";
      this.btnNo.UseVisualStyleBackColor = true;
      // 
      // rbRange
      // 
      this.rbRange.AutoSize = true;
      this.rbRange.Location = new System.Drawing.Point(27, 26);
      this.rbRange.Name = "rbRange";
      this.rbRange.Size = new System.Drawing.Size(57, 17);
      this.rbRange.TabIndex = 0;
      this.rbRange.TabStop = true;
      this.rbRange.Text = "Range";
      this.rbRange.UseVisualStyleBackColor = true;
      // 
      // rbNotNull
      // 
      this.rbNotNull.AutoSize = true;
      this.rbNotNull.Location = new System.Drawing.Point(27, 104);
      this.rbNotNull.Name = "rbNotNull";
      this.rbNotNull.Size = new System.Drawing.Size(60, 17);
      this.rbNotNull.TabIndex = 1;
      this.rbNotNull.TabStop = true;
      this.rbNotNull.Text = "NotNull";
      this.rbNotNull.UseVisualStyleBackColor = true;
      // 
      // rbNull
      // 
      this.rbNull.AutoSize = true;
      this.rbNull.Location = new System.Drawing.Point(27, 127);
      this.rbNull.Name = "rbNull";
      this.rbNull.Size = new System.Drawing.Size(43, 17);
      this.rbNull.TabIndex = 2;
      this.rbNull.TabStop = true;
      this.rbNull.Text = "Null";
      this.rbNull.UseVisualStyleBackColor = true;
      // 
      // DateRangeGridFilterForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(635, 160);
      this.Controls.Add(this.rbNull);
      this.Controls.Add(this.rbNotNull);
      this.Controls.Add(this.rbRange);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.edRange);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "DateRangeGridFilterForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private Controls.DateRangeBox edRange;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnNo;
    private System.Windows.Forms.RadioButton rbRange;
    private System.Windows.Forms.RadioButton rbNotNull;
    private System.Windows.Forms.RadioButton rbNull;
  }
}