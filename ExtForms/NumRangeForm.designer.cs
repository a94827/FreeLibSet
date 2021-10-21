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
      this.btn2eq1 = new System.Windows.Forms.Button();
      this.label2 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.edLastValue = new FreeLibSet.Controls.NumEditBox();
      this.edFirstValue = new FreeLibSet.Controls.NumEditBox();
      this.btnNo = new System.Windows.Forms.Button();
      this.lblRange = new System.Windows.Forms.Label();
      this.TheGroup.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.edLastValue)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.edFirstValue)).BeginInit();
      this.SuspendLayout();
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(301, 42);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(301, 12);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
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
      this.TheGroup.Controls.Add(this.label2);
      this.TheGroup.Controls.Add(this.label1);
      this.TheGroup.Controls.Add(this.edLastValue);
      this.TheGroup.Controls.Add(this.edFirstValue);
      this.TheGroup.Location = new System.Drawing.Point(6, 12);
      this.TheGroup.Name = "TheGroup";
      this.TheGroup.Size = new System.Drawing.Size(286, 116);
      this.TheGroup.TabIndex = 0;
      this.TheGroup.TabStop = false;
      // 
      // btn2eq1
      // 
      this.btn2eq1.Location = new System.Drawing.Point(153, 58);
      this.btn2eq1.Name = "btn2eq1";
      this.btn2eq1.Size = new System.Drawing.Size(32, 24);
      this.btn2eq1.TabIndex = 4;
      this.btn2eq1.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(153, 16);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(127, 18);
      this.label2.TabIndex = 2;
      this.label2.Text = "М&аксимум";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(13, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(130, 18);
      this.label1.TabIndex = 0;
      this.label1.Text = "М&инимум";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edLastValue
      // 
      this.edLastValue.Location = new System.Drawing.Point(153, 36);
      this.edLastValue.Name = "edLastValue";
      this.edLastValue.Size = new System.Drawing.Size(127, 20);
      this.edLastValue.TabIndex = 3;
      // 
      // edFirstValue
      // 
      this.edFirstValue.Location = new System.Drawing.Point(16, 36);
      this.edFirstValue.Name = "edFirstValue";
      this.edFirstValue.Size = new System.Drawing.Size(127, 20);
      this.edFirstValue.TabIndex = 1;
      // 
      // btnNo
      // 
      this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
      this.btnNo.Location = new System.Drawing.Point(301, 72);
      this.btnNo.Name = "btnNo";
      this.btnNo.Size = new System.Drawing.Size(88, 24);
      this.btnNo.TabIndex = 3;
      this.btnNo.Text = "&Нет";
      this.btnNo.UseVisualStyleBackColor = true;
      // 
      // lblRange
      // 
      this.lblRange.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.lblRange.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
      this.lblRange.Location = new System.Drawing.Point(3, 91);
      this.lblRange.Name = "lblRange";
      this.lblRange.Size = new System.Drawing.Size(280, 22);
      this.lblRange.TabIndex = 5;
      this.lblRange.Text = "???";
      this.lblRange.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // NumRangeForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(401, 140);
      this.Controls.Add(this.btnNo);
      this.Controls.Add(this.TheGroup);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "NumRangeForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.TheGroup.ResumeLayout(false);
      this.TheGroup.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.edLastValue)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.edFirstValue)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private FreeLibSet.Controls.NumEditBox edFirstValue;
    private FreeLibSet.Controls.NumEditBox edLastValue;
    public System.Windows.Forms.GroupBox TheGroup;
    private System.Windows.Forms.Button btn2eq1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnNo;
    private System.Windows.Forms.Label lblRange;
  }
}