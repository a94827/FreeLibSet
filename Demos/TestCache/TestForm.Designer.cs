namespace TestCache
{
  partial class TestForm
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
      this.components = new System.ComponentModel.Container();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.grThreads = new System.Windows.Forms.DataGridView();
      this.TheTimer = new System.Windows.Forms.Timer(this.components);
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.grStat = new System.Windows.Forms.DataGridView();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnClear = new System.Windows.Forms.Button();
      this.btnInfo = new System.Windows.Forms.Button();
      this.lblMemoryLoad = new System.Windows.Forms.Label();
      this.label7 = new System.Windows.Forms.Label();
      this.lblMemoryState = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.lblGCTotalMemory = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.btnCollect = new System.Windows.Forms.Button();
      this.lblCollectInfo = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.btnPersistDir = new System.Windows.Forms.Button();
      this.btnTempDir = new System.Windows.Forms.Button();
      this.edPersistDir = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.edTempDir = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grThreads)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.groupBox3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grStat)).BeginInit();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.grThreads);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Location = new System.Drawing.Point(0, 0);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(731, 166);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Потоки выполнения";
      // 
      // grThreads
      // 
      this.grThreads.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grThreads.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grThreads.Location = new System.Drawing.Point(3, 16);
      this.grThreads.Name = "grThreads";
      this.grThreads.Size = new System.Drawing.Size(725, 147);
      this.grThreads.TabIndex = 0;
      // 
      // TheTimer
      // 
      this.TheTimer.Interval = 1000;
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.groupBox3);
      this.splitContainer1.Panel2.Controls.Add(this.panel1);
      this.splitContainer1.Size = new System.Drawing.Size(731, 426);
      this.splitContainer1.SplitterDistance = 166;
      this.splitContainer1.TabIndex = 0;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.grStat);
      this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox3.Location = new System.Drawing.Point(0, 0);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(731, 139);
      this.groupBox3.TabIndex = 0;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Статистика объекта CacheStat";
      // 
      // grStat
      // 
      this.grStat.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grStat.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grStat.Location = new System.Drawing.Point(3, 16);
      this.grStat.Name = "grStat";
      this.grStat.Size = new System.Drawing.Size(725, 120);
      this.grStat.TabIndex = 0;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnClear);
      this.panel1.Controls.Add(this.btnInfo);
      this.panel1.Controls.Add(this.lblMemoryLoad);
      this.panel1.Controls.Add(this.label7);
      this.panel1.Controls.Add(this.lblMemoryState);
      this.panel1.Controls.Add(this.label6);
      this.panel1.Controls.Add(this.lblGCTotalMemory);
      this.panel1.Controls.Add(this.label4);
      this.panel1.Controls.Add(this.btnCollect);
      this.panel1.Controls.Add(this.lblCollectInfo);
      this.panel1.Controls.Add(this.label3);
      this.panel1.Controls.Add(this.btnPersistDir);
      this.panel1.Controls.Add(this.btnTempDir);
      this.panel1.Controls.Add(this.edPersistDir);
      this.panel1.Controls.Add(this.label2);
      this.panel1.Controls.Add(this.edTempDir);
      this.panel1.Controls.Add(this.label1);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 139);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(731, 117);
      this.panel1.TabIndex = 2;
      // 
      // btnClear
      // 
      this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnClear.Location = new System.Drawing.Point(634, 16);
      this.btnClear.Name = "btnClear";
      this.btnClear.Size = new System.Drawing.Size(88, 24);
      this.btnClear.TabIndex = 6;
      this.btnClear.Text = "Сброс";
      this.btnClear.UseVisualStyleBackColor = true;
      // 
      // btnInfo
      // 
      this.btnInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnInfo.Location = new System.Drawing.Point(634, 88);
      this.btnInfo.Name = "btnInfo";
      this.btnInfo.Size = new System.Drawing.Size(88, 24);
      this.btnInfo.TabIndex = 18;
      this.btnInfo.Text = "Инфо";
      this.btnInfo.UseVisualStyleBackColor = true;
      // 
      // lblMemoryLoad
      // 
      this.lblMemoryLoad.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblMemoryLoad.Location = new System.Drawing.Point(371, 88);
      this.lblMemoryLoad.Name = "lblMemoryLoad";
      this.lblMemoryLoad.Size = new System.Drawing.Size(103, 23);
      this.lblMemoryLoad.TabIndex = 15;
      this.lblMemoryLoad.Text = "???";
      this.lblMemoryLoad.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(281, 88);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(84, 23);
      this.label7.TabIndex = 14;
      this.label7.Text = "MemoryLoad";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblMemoryState
      // 
      this.lblMemoryState.BackColor = System.Drawing.Color.White;
      this.lblMemoryState.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblMemoryState.ForeColor = System.Drawing.Color.Black;
      this.lblMemoryState.Location = new System.Drawing.Point(126, 88);
      this.lblMemoryState.Name = "lblMemoryState";
      this.lblMemoryState.Size = new System.Drawing.Size(149, 23);
      this.lblMemoryState.TabIndex = 13;
      this.lblMemoryState.Text = "???";
      this.lblMemoryState.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(6, 88);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(120, 23);
      this.label6.TabIndex = 12;
      this.label6.Text = "AvailableMemoryState";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // lblGCTotalMemory
      // 
      this.lblGCTotalMemory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.lblGCTotalMemory.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblGCTotalMemory.Location = new System.Drawing.Point(619, 53);
      this.lblGCTotalMemory.Name = "lblGCTotalMemory";
      this.lblGCTotalMemory.Size = new System.Drawing.Size(103, 23);
      this.lblGCTotalMemory.TabIndex = 11;
      this.lblGCTotalMemory.Text = "???";
      this.lblGCTotalMemory.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label4
      // 
      this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label4.Location = new System.Drawing.Point(513, 55);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(100, 23);
      this.label4.TabIndex = 10;
      this.label4.Text = "GetTotalMemory()";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnCollect
      // 
      this.btnCollect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCollect.Location = new System.Drawing.Point(475, 54);
      this.btnCollect.Name = "btnCollect";
      this.btnCollect.Size = new System.Drawing.Size(32, 24);
      this.btnCollect.TabIndex = 9;
      this.btnCollect.UseVisualStyleBackColor = true;
      // 
      // lblCollectInfo
      // 
      this.lblCollectInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblCollectInfo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblCollectInfo.Location = new System.Drawing.Point(126, 54);
      this.lblCollectInfo.Name = "lblCollectInfo";
      this.lblCollectInfo.Size = new System.Drawing.Size(343, 23);
      this.lblCollectInfo.TabIndex = 8;
      this.lblCollectInfo.Text = "???";
      this.lblCollectInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(3, 54);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(117, 23);
      this.label3.TabIndex = 7;
      this.label3.Text = "GCCollect";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // btnPersistDir
      // 
      this.btnPersistDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPersistDir.Location = new System.Drawing.Point(596, 28);
      this.btnPersistDir.Name = "btnPersistDir";
      this.btnPersistDir.Size = new System.Drawing.Size(32, 24);
      this.btnPersistDir.TabIndex = 5;
      this.btnPersistDir.UseVisualStyleBackColor = true;
      // 
      // btnTempDir
      // 
      this.btnTempDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnTempDir.Location = new System.Drawing.Point(596, 3);
      this.btnTempDir.Name = "btnTempDir";
      this.btnTempDir.Size = new System.Drawing.Size(32, 24);
      this.btnTempDir.TabIndex = 2;
      this.btnTempDir.UseVisualStyleBackColor = true;
      // 
      // edPersistDir
      // 
      this.edPersistDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edPersistDir.Location = new System.Drawing.Point(126, 31);
      this.edPersistDir.Name = "edPersistDir";
      this.edPersistDir.ReadOnly = true;
      this.edPersistDir.Size = new System.Drawing.Size(462, 20);
      this.edPersistDir.TabIndex = 4;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(3, 31);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(117, 20);
      this.label2.TabIndex = 3;
      this.label2.Text = "Постоянный каталог";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edTempDir
      // 
      this.edTempDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edTempDir.Location = new System.Drawing.Point(126, 5);
      this.edTempDir.Name = "edTempDir";
      this.edTempDir.ReadOnly = true;
      this.edTempDir.Size = new System.Drawing.Size(462, 20);
      this.edTempDir.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(3, 5);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(117, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "Временный каталог";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // TestForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(731, 426);
      this.Controls.Add(this.splitContainer1);
      this.Name = "TestForm";
      this.Text = "Тестирования кэша";
      this.groupBox2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grThreads)).EndInit();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grStat)).EndInit();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.DataGridView grThreads;
    private System.Windows.Forms.Timer TheTimer;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.DataGridView grStat;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnClear;
    private System.Windows.Forms.Button btnInfo;
    private System.Windows.Forms.Label lblMemoryLoad;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label lblMemoryState;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label lblGCTotalMemory;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Button btnCollect;
    private System.Windows.Forms.Label lblCollectInfo;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button btnPersistDir;
    private System.Windows.Forms.Button btnTempDir;
    private System.Windows.Forms.TextBox edPersistDir;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox edTempDir;
    private System.Windows.Forms.Label label1;
  }
}