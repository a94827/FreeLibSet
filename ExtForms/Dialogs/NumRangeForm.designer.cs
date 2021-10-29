namespace FreeLibSet.Forms
{
  partial class NumRangeForm
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

    #region Windows TheForm Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.TheGroup = new System.Windows.Forms.GroupBox();
      this.lblRange = new System.Windows.Forms.Label();
      this.btn2eq1 = new System.Windows.Forms.Button();
      this.lblMaximum = new System.Windows.Forms.Label();
      this.lblMinimum = new System.Windows.Forms.Label();
      this.btnNo = new System.Windows.Forms.Button();
      this.TheGroup.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(401, 52);
      this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(117, 30);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(401, 15);
      this.btnOk.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(117, 30);
      this.btnOk.TabIndex = 1;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // TheGroup
      // 
      this.TheGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.TheGroup.Controls.Add(this.lblRange);
      this.TheGroup.Controls.Add(this.btn2eq1);
      this.TheGroup.Controls.Add(this.lblMaximum);
      this.TheGroup.Controls.Add(this.lblMinimum);
      this.TheGroup.Location = new System.Drawing.Point(8, 15);
      this.TheGroup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.TheGroup.Name = "TheGroup";
      this.TheGroup.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.TheGroup.Size = new System.Drawing.Size(381, 143);
      this.TheGroup.TabIndex = 0;
      this.TheGroup.TabStop = false;
      // 
      // lblRange
      // 
      this.lblRange.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.lblRange.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblRange.Location = new System.Drawing.Point(4, 112);
      this.lblRange.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblRange.Name = "lblRange";
      this.lblRange.Size = new System.Drawing.Size(373, 27);
      this.lblRange.TabIndex = 5;
      this.lblRange.Text = "???";
      this.lblRange.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // btn2eq1
      // 
      this.btn2eq1.Location = new System.Drawing.Point(204, 71);
      this.btn2eq1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.btn2eq1.Name = "btn2eq1";
      this.btn2eq1.Size = new System.Drawing.Size(43, 30);
      this.btn2eq1.TabIndex = 4;
      this.btn2eq1.UseVisualStyleBackColor = true;
      // 
      // lblMaximum
      // 
      this.lblMaximum.Location = new System.Drawing.Point(204, 20);
      this.lblMaximum.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblMaximum.Name = "lblMaximum";
      this.lblMaximum.Size = new System.Drawing.Size(168, 22);
      this.lblMaximum.TabIndex = 2;
      this.lblMaximum.Text = "М&аксимум";
      this.lblMaximum.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblMinimum
      // 
      this.lblMinimum.Location = new System.Drawing.Point(21, 20);
      this.lblMinimum.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.lblMinimum.Name = "lblMinimum";
      this.lblMinimum.Size = new System.Drawing.Size(168, 22);
      this.lblMinimum.TabIndex = 0;
      this.lblMinimum.Text = "М&инимум";
      this.lblMinimum.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnNo
      // 
      this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
      this.btnNo.Location = new System.Drawing.Point(401, 89);
      this.btnNo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.btnNo.Name = "btnNo";
      this.btnNo.Size = new System.Drawing.Size(117, 30);
      this.btnNo.TabIndex = 3;
      this.btnNo.Text = "&Нет";
      this.btnNo.UseVisualStyleBackColor = true;
      // 
      // NumRangeForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(535, 172);
      this.Controls.Add(this.btnNo);
      this.Controls.Add(this.TheGroup);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "NumRangeForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.TheGroup.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    public System.Windows.Forms.GroupBox TheGroup;
    private System.Windows.Forms.Button btn2eq1;
    internal System.Windows.Forms.Label lblMaximum;
    internal System.Windows.Forms.Label lblMinimum;
    private System.Windows.Forms.Button btnNo;
    public System.Windows.Forms.Label lblRange;
  }
}