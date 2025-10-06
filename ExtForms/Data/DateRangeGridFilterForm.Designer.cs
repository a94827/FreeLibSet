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
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnNo = new System.Windows.Forms.Button();
      this.TheGroupBox = new System.Windows.Forms.GroupBox();
      this.rbNull = new System.Windows.Forms.RadioButton();
      this.rbNotNull = new System.Windows.Forms.RadioButton();
      this.rbRange = new System.Windows.Forms.RadioButton();
      this.edRange = new FreeLibSet.Controls.DateRangeBox();
      this.panel1.SuspendLayout();
      this.TheGroupBox.SuspendLayout();
      this.SuspendLayout();
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
      this.panel1.Location = new System.Drawing.Point(419, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(106, 158);
      this.panel1.TabIndex = 1;
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
      // TheGroupBox
      // 
      this.TheGroupBox.Controls.Add(this.rbNull);
      this.TheGroupBox.Controls.Add(this.rbNotNull);
      this.TheGroupBox.Controls.Add(this.rbRange);
      this.TheGroupBox.Controls.Add(this.edRange);
      this.TheGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheGroupBox.Location = new System.Drawing.Point(0, 0);
      this.TheGroupBox.Name = "TheGroupBox";
      this.TheGroupBox.Size = new System.Drawing.Size(419, 158);
      this.TheGroupBox.TabIndex = 0;
      this.TheGroupBox.TabStop = false;
      // 
      // rbNull
      // 
      this.rbNull.AutoSize = true;
      this.rbNull.Location = new System.Drawing.Point(14, 127);
      this.rbNull.Name = "rbNull";
      this.rbNull.Size = new System.Drawing.Size(42, 17);
      this.rbNull.TabIndex = 3;
      this.rbNull.TabStop = true;
      this.rbNull.Text = "Null";
      this.rbNull.UseVisualStyleBackColor = true;
      // 
      // rbNotNull
      // 
      this.rbNotNull.AutoSize = true;
      this.rbNotNull.Location = new System.Drawing.Point(14, 104);
      this.rbNotNull.Name = "rbNotNull";
      this.rbNotNull.Size = new System.Drawing.Size(59, 17);
      this.rbNotNull.TabIndex = 2;
      this.rbNotNull.TabStop = true;
      this.rbNotNull.Text = "NotNull";
      this.rbNotNull.UseVisualStyleBackColor = true;
      // 
      // rbRange
      // 
      this.rbRange.AutoSize = true;
      this.rbRange.Location = new System.Drawing.Point(14, 26);
      this.rbRange.Name = "rbRange";
      this.rbRange.Size = new System.Drawing.Size(56, 17);
      this.rbRange.TabIndex = 0;
      this.rbRange.TabStop = true;
      this.rbRange.Text = "Range";
      this.rbRange.UseVisualStyleBackColor = true;
      // 
      // edRange
      // 
      this.edRange.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edRange.Location = new System.Drawing.Point(34, 59);
      this.edRange.Margin = new System.Windows.Forms.Padding(5);
      this.edRange.Name = "edRange";
      this.edRange.Size = new System.Drawing.Size(374, 37);
      this.edRange.TabIndex = 1;
      // 
      // DateRangeGridFilterForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(525, 158);
      this.Controls.Add(this.TheGroupBox);
      this.Controls.Add(this.panel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "DateRangeGridFilterForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.panel1.ResumeLayout(false);
      this.TheGroupBox.ResumeLayout(false);
      this.TheGroupBox.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnNo;
    private System.Windows.Forms.GroupBox TheGroupBox;
    private System.Windows.Forms.RadioButton rbNull;
    private System.Windows.Forms.RadioButton rbNotNull;
    private System.Windows.Forms.RadioButton rbRange;
    private Controls.DateRangeBox edRange;
  }
}