namespace FreeLibSet.Forms.Reporting
{
  partial class BRPageSetupMargins
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
      this.panMargins = new System.Windows.Forms.Panel();
      this.grpMargins = new System.Windows.Forms.GroupBox();
      this.edBottomMargin = new FreeLibSet.Controls.DecimalEditBox();
      this.label6 = new System.Windows.Forms.Label();
      this.edTopMargin = new FreeLibSet.Controls.DecimalEditBox();
      this.label5 = new System.Windows.Forms.Label();
      this.edRightMargin = new FreeLibSet.Controls.DecimalEditBox();
      this.label4 = new System.Windows.Forms.Label();
      this.edLeftMargin = new FreeLibSet.Controls.DecimalEditBox();
      this.label3 = new System.Windows.Forms.Label();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
      this.panMargins.SuspendLayout();
      this.grpMargins.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.tableLayoutPanel3.SuspendLayout();
      this.SuspendLayout();
      // 
      // panMargins
      // 
      this.panMargins.Controls.Add(this.grpMargins);
      this.panMargins.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panMargins.Location = new System.Drawing.Point(0, 0);
      this.panMargins.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.panMargins.Name = "panMargins";
      this.panMargins.Size = new System.Drawing.Size(583, 471);
      this.panMargins.TabIndex = 0;
      // 
      // grpMargins
      // 
      this.grpMargins.Controls.Add(this.tableLayoutPanel3);
      this.grpMargins.Controls.Add(this.tableLayoutPanel1);
      this.grpMargins.Location = new System.Drawing.Point(20, 15);
      this.grpMargins.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.grpMargins.Name = "grpMargins";
      this.grpMargins.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.grpMargins.Size = new System.Drawing.Size(534, 203);
      this.grpMargins.TabIndex = 0;
      this.grpMargins.TabStop = false;
      this.grpMargins.Text = "Поля, см";
      // 
      // edBottomMargin
      // 
      this.edBottomMargin.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edBottomMargin.Format = "0.00";
      this.edBottomMargin.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
      this.edBottomMargin.Location = new System.Drawing.Point(104, 139);
      this.edBottomMargin.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.edBottomMargin.Name = "edBottomMargin";
      this.edBottomMargin.Size = new System.Drawing.Size(92, 22);
      this.edBottomMargin.TabIndex = 7;
      // 
      // label6
      // 
      this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label6.Location = new System.Drawing.Point(104, 110);
      this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(92, 25);
      this.label6.TabIndex = 6;
      this.label6.Text = "&Нижнее";
      this.label6.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
      // 
      // edTopMargin
      // 
      this.edTopMargin.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edTopMargin.Format = "0.00";
      this.edTopMargin.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
      this.edTopMargin.Location = new System.Drawing.Point(104, 29);
      this.edTopMargin.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.edTopMargin.Name = "edTopMargin";
      this.edTopMargin.Size = new System.Drawing.Size(92, 22);
      this.edTopMargin.TabIndex = 1;
      // 
      // label5
      // 
      this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label5.Location = new System.Drawing.Point(104, 0);
      this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(92, 25);
      this.label5.TabIndex = 0;
      this.label5.Text = "&Верхнее";
      this.label5.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
      // 
      // edRightMargin
      // 
      this.edRightMargin.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edRightMargin.Format = "0.00";
      this.edRightMargin.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
      this.edRightMargin.Location = new System.Drawing.Point(204, 84);
      this.edRightMargin.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.edRightMargin.Name = "edRightMargin";
      this.edRightMargin.Size = new System.Drawing.Size(95, 22);
      this.edRightMargin.TabIndex = 5;
      // 
      // label4
      // 
      this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label4.Location = new System.Drawing.Point(204, 55);
      this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(95, 25);
      this.label4.TabIndex = 4;
      this.label4.Text = "&Правое";
      this.label4.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
      // 
      // edLeftMargin
      // 
      this.edLeftMargin.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edLeftMargin.Format = "0.00";
      this.edLeftMargin.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
      this.edLeftMargin.Location = new System.Drawing.Point(4, 84);
      this.edLeftMargin.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.edLeftMargin.Name = "edLeftMargin";
      this.edLeftMargin.Size = new System.Drawing.Size(92, 22);
      this.edLeftMargin.TabIndex = 3;
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.Location = new System.Drawing.Point(4, 55);
      this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(92, 25);
      this.label3.TabIndex = 2;
      this.label3.Text = "&Левое";
      this.label3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.AutoSize = true;
      this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.tableLayoutPanel1.ColumnCount = 5;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 19);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.Size = new System.Drawing.Size(526, 0);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.AutoSize = true;
      this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.tableLayoutPanel2.ColumnCount = 3;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel2.Controls.Add(this.edTopMargin, 1, 1);
      this.tableLayoutPanel2.Controls.Add(this.edRightMargin, 2, 3);
      this.tableLayoutPanel2.Controls.Add(this.label3, 0, 2);
      this.tableLayoutPanel2.Controls.Add(this.label5, 1, 0);
      this.tableLayoutPanel2.Controls.Add(this.edLeftMargin, 0, 3);
      this.tableLayoutPanel2.Controls.Add(this.label4, 2, 2);
      this.tableLayoutPanel2.Controls.Add(this.label6, 1, 4);
      this.tableLayoutPanel2.Controls.Add(this.edBottomMargin, 1, 5);
      this.tableLayoutPanel2.Location = new System.Drawing.Point(85, 3);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 6;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel2.Size = new System.Drawing.Size(303, 165);
      this.tableLayoutPanel2.TabIndex = 0;
      // 
      // tableLayoutPanel3
      // 
      this.tableLayoutPanel3.AutoSize = true;
      this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.tableLayoutPanel3.ColumnCount = 3;
      this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 360F));
      this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel2, 1, 0);
      this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Top;
      this.tableLayoutPanel3.Location = new System.Drawing.Point(4, 19);
      this.tableLayoutPanel3.Name = "tableLayoutPanel3";
      this.tableLayoutPanel3.RowCount = 1;
      this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel3.Size = new System.Drawing.Size(526, 171);
      this.tableLayoutPanel3.TabIndex = 0;
      // 
      // BRPageSetupMargins
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(583, 471);
      this.Controls.Add(this.panMargins);
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.Name = "BRPageSetupMargins";
      this.Text = "BRPageSetupMargins";
      this.panMargins.ResumeLayout(false);
      this.grpMargins.ResumeLayout(false);
      this.grpMargins.PerformLayout();
      this.tableLayoutPanel2.ResumeLayout(false);
      this.tableLayoutPanel3.ResumeLayout(false);
      this.tableLayoutPanel3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panMargins;
    private System.Windows.Forms.GroupBox grpMargins;
    private FreeLibSet.Controls.DecimalEditBox edBottomMargin;
    private System.Windows.Forms.Label label6;
    private FreeLibSet.Controls.DecimalEditBox edTopMargin;
    private System.Windows.Forms.Label label5;
    private FreeLibSet.Controls.DecimalEditBox edRightMargin;
    private System.Windows.Forms.Label label4;
    private FreeLibSet.Controls.DecimalEditBox edLeftMargin;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
  }
}
