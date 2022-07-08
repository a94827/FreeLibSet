namespace TestMemoryTools
{
  partial class MainForm
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
      this.lblTotalPhysicalMemory = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.lblMemoryLoad = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.lblMemoryState = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.btnInfo = new System.Windows.Forms.Button();
      this.btnFree = new System.Windows.Forms.Button();
      this.label6 = new System.Windows.Forms.Label();
      this.edSize = new AgeyevAV.ExtForms.NumEditBox();
      this.label5 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.edCount = new AgeyevAV.ExtForms.ExtNumericUpDown();
      this.btnAlloc = new System.Windows.Forms.Button();
      this.lb1 = new System.Windows.Forms.ListBox();
      this.lblTotalSize = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.lblGCGetTotalMemory = new System.Windows.Forms.Label();
      this.label9 = new System.Windows.Forms.Label();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.lblTotalPhysicalMemory);
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.lblMemoryLoad);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.lblMemoryState);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.groupBox1.Location = new System.Drawing.Point(0, 419);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(663, 94);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Свойства MemoryTools";
      // 
      // lblTotalPhysicalMemory
      // 
      this.lblTotalPhysicalMemory.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblTotalPhysicalMemory.Location = new System.Drawing.Point(551, 27);
      this.lblTotalPhysicalMemory.Name = "lblTotalPhysicalMemory";
      this.lblTotalPhysicalMemory.Size = new System.Drawing.Size(100, 23);
      this.lblTotalPhysicalMemory.TabIndex = 5;
      this.lblTotalPhysicalMemory.Text = "???";
      this.lblTotalPhysicalMemory.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(421, 27);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(111, 23);
      this.label7.TabIndex = 4;
      this.label7.Text = "TotalPhysicalMemory";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblMemoryLoad
      // 
      this.lblMemoryLoad.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblMemoryLoad.Location = new System.Drawing.Point(145, 60);
      this.lblMemoryLoad.Name = "lblMemoryLoad";
      this.lblMemoryLoad.Size = new System.Drawing.Size(100, 23);
      this.lblMemoryLoad.TabIndex = 3;
      this.lblMemoryLoad.Text = "???";
      this.lblMemoryLoad.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(15, 60);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(111, 23);
      this.label3.TabIndex = 2;
      this.label3.Text = "MemoryLoad";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblMemoryState
      // 
      this.lblMemoryState.BackColor = System.Drawing.Color.White;
      this.lblMemoryState.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblMemoryState.ForeColor = System.Drawing.Color.Black;
      this.lblMemoryState.Location = new System.Drawing.Point(145, 27);
      this.lblMemoryState.Name = "lblMemoryState";
      this.lblMemoryState.Size = new System.Drawing.Size(100, 23);
      this.lblMemoryState.TabIndex = 1;
      this.lblMemoryState.Text = "???";
      this.lblMemoryState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(15, 27);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(111, 23);
      this.label1.TabIndex = 0;
      this.label1.Text = "MemoryState";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.lblGCGetTotalMemory);
      this.groupBox2.Controls.Add(this.label9);
      this.groupBox2.Controls.Add(this.lblTotalSize);
      this.groupBox2.Controls.Add(this.label8);
      this.groupBox2.Controls.Add(this.btnInfo);
      this.groupBox2.Controls.Add(this.btnFree);
      this.groupBox2.Controls.Add(this.label6);
      this.groupBox2.Controls.Add(this.edSize);
      this.groupBox2.Controls.Add(this.label5);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.edCount);
      this.groupBox2.Controls.Add(this.btnAlloc);
      this.groupBox2.Controls.Add(this.lb1);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Location = new System.Drawing.Point(0, 0);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(663, 419);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Выделение блоков памяти";
      // 
      // btnInfo
      // 
      this.btnInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnInfo.Location = new System.Drawing.Point(211, 386);
      this.btnInfo.Name = "btnInfo";
      this.btnInfo.Size = new System.Drawing.Size(132, 24);
      this.btnInfo.TabIndex = 10;
      this.btnInfo.Text = "Инфо";
      this.btnInfo.UseVisualStyleBackColor = true;
      // 
      // btnFree
      // 
      this.btnFree.Location = new System.Drawing.Point(211, 49);
      this.btnFree.Name = "btnFree";
      this.btnFree.Size = new System.Drawing.Size(132, 24);
      this.btnFree.TabIndex = 9;
      this.btnFree.Text = "Удалить";
      this.btnFree.UseVisualStyleBackColor = true;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(625, 23);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(33, 20);
      this.label6.TabIndex = 8;
      this.label6.Text = "МБ";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edSize
      // 
      this.edSize.Location = new System.Drawing.Point(542, 23);
      this.edSize.Name = "edSize";
      this.edSize.Size = new System.Drawing.Size(77, 20);
      this.edSize.TabIndex = 7;
      this.edSize.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(503, 23);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(33, 20);
      this.label5.TabIndex = 6;
      this.label5.Text = "по";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(445, 23);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(52, 20);
      this.label4.TabIndex = 5;
      this.label4.Text = "блок(ов)";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edCount
      // 
      this.edCount.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.edCount.Location = new System.Drawing.Point(360, 23);
      this.edCount.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.edCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.edCount.Name = "edCount";
      this.edCount.Size = new System.Drawing.Size(79, 20);
      this.edCount.TabIndex = 4;
      this.edCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.edCount.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
      // 
      // btnAlloc
      // 
      this.btnAlloc.Location = new System.Drawing.Point(211, 19);
      this.btnAlloc.Name = "btnAlloc";
      this.btnAlloc.Size = new System.Drawing.Size(132, 24);
      this.btnAlloc.TabIndex = 1;
      this.btnAlloc.Text = "Добавить";
      this.btnAlloc.UseVisualStyleBackColor = true;
      // 
      // lb1
      // 
      this.lb1.Dock = System.Windows.Forms.DockStyle.Left;
      this.lb1.FormattingEnabled = true;
      this.lb1.Location = new System.Drawing.Point(3, 16);
      this.lb1.Name = "lb1";
      this.lb1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
      this.lb1.Size = new System.Drawing.Size(191, 394);
      this.lb1.TabIndex = 0;
      // 
      // lblTotalSize
      // 
      this.lblTotalSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblTotalSize.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblTotalSize.Location = new System.Drawing.Point(551, 349);
      this.lblTotalSize.Name = "lblTotalSize";
      this.lblTotalSize.Size = new System.Drawing.Size(100, 23);
      this.lblTotalSize.TabIndex = 12;
      this.lblTotalSize.Text = "???";
      this.lblTotalSize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label8
      // 
      this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label8.Location = new System.Drawing.Point(421, 349);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(124, 23);
      this.label8.TabIndex = 11;
      this.label8.Text = "Всего выделено";
      this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblGCGetTotalMemory
      // 
      this.lblGCGetTotalMemory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblGCGetTotalMemory.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblGCGetTotalMemory.Location = new System.Drawing.Point(551, 386);
      this.lblGCGetTotalMemory.Name = "lblGCGetTotalMemory";
      this.lblGCGetTotalMemory.Size = new System.Drawing.Size(100, 23);
      this.lblGCGetTotalMemory.TabIndex = 14;
      this.lblGCGetTotalMemory.Text = "???";
      this.lblGCGetTotalMemory.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label9
      // 
      this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label9.Location = new System.Drawing.Point(421, 386);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(124, 23);
      this.label9.TabIndex = 13;
      this.label9.Text = "GC.GetTotalMemory";
      this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(663, 513);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "MemoryTools Demo";
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.Label lblMemoryLoad;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label lblMemoryState;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Button btnAlloc;
    private System.Windows.Forms.ListBox lb1;
    private System.Windows.Forms.Button btnFree;
    private System.Windows.Forms.Label label6;
    private AgeyevAV.ExtForms.NumEditBox edSize;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label4;
    private AgeyevAV.ExtForms.ExtNumericUpDown edCount;
    private System.Windows.Forms.Button btnInfo;
    private System.Windows.Forms.Label lblTotalPhysicalMemory;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label lblTotalSize;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Label lblGCGetTotalMemory;
    private System.Windows.Forms.Label label9;
  }
}

