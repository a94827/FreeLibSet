namespace TestCache
{
  partial class ParamForm
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
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
      this.panel2 = new System.Windows.Forms.Panel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.label7 = new System.Windows.Forms.Label();
      this.eCheckMemoryInterval = new FreeLibSet.Controls.IntEditBox();
      this.label8 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.edCriticalMemoryLoad = new FreeLibSet.Controls.IntEditBox();
      this.label6 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.edLowMemorySize = new FreeLibSet.Controls.IntEditBox();
      this.label3 = new System.Windows.Forms.Label();
      this.edCapacity = new FreeLibSet.Controls.IntEditBox();
      this.label1 = new System.Windows.Forms.Label();
      this.panel3 = new System.Windows.Forms.Panel();
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.grObjs = new System.Windows.Forms.DataGridView();
      this.edThreads = new FreeLibSet.Controls.IntEditBox();
      this.label2 = new System.Windows.Forms.Label();
      this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colPersistance = new System.Windows.Forms.DataGridViewComboBoxColumn();
      this.colDel = new System.Windows.Forms.DataGridViewCheckBoxColumn();
      this.colSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colKeys = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colValues = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.panel2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.panel3.SuspendLayout();
      this.panel1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grObjs)).BeginInit();
      this.SuspendLayout();
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.groupBox2);
      this.panel2.Controls.Add(this.panel3);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(634, 368);
      this.panel2.TabIndex = 0;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.label7);
      this.groupBox1.Controls.Add(this.eCheckMemoryInterval);
      this.groupBox1.Controls.Add(this.label8);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.edCriticalMemoryLoad);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.edLowMemorySize);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.edCapacity);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(634, 159);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Параметры Cache";
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(402, 124);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(38, 20);
      this.label7.TabIndex = 12;
      this.label7.Text = "с";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // eCheckMemoryInterval
      // 
      this.eCheckMemoryInterval.DecimalPlaces = 0;
      this.eCheckMemoryInterval.Location = new System.Drawing.Point(259, 124);
      this.eCheckMemoryInterval.Name = "eCheckMemoryInterval";
      this.eCheckMemoryInterval.Size = new System.Drawing.Size(134, 20);
      this.eCheckMemoryInterval.TabIndex = 11;
      // 
      // label8
      // 
      this.label8.Location = new System.Drawing.Point(12, 124);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(221, 20);
      this.label8.TabIndex = 10;
      this.label8.Text = "CheckMemoryInterval";
      this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(399, 90);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(38, 20);
      this.label5.TabIndex = 9;
      this.label5.Text = "%";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edCriticalMemoryLoad
      // 
      this.edCriticalMemoryLoad.DecimalPlaces = 0;
      this.edCriticalMemoryLoad.Location = new System.Drawing.Point(259, 90);
      this.edCriticalMemoryLoad.Name = "edCriticalMemoryLoad";
      this.edCriticalMemoryLoad.Size = new System.Drawing.Size(134, 20);
      this.edCriticalMemoryLoad.TabIndex = 8;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(12, 90);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(221, 20);
      this.label6.TabIndex = 7;
      this.label6.Text = "CriticalMemoryLoad";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(399, 61);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(38, 20);
      this.label4.TabIndex = 6;
      this.label4.Text = "МБ";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edLowMemorySize
      // 
      this.edLowMemorySize.DecimalPlaces = 0;
      this.edLowMemorySize.Location = new System.Drawing.Point(259, 61);
      this.edLowMemorySize.Name = "edLowMemorySize";
      this.edLowMemorySize.Size = new System.Drawing.Size(134, 20);
      this.edLowMemorySize.TabIndex = 5;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(12, 61);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(221, 20);
      this.label3.TabIndex = 4;
      this.label3.Text = "LowMemorySize";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edCapacity
      // 
      this.edCapacity.DecimalPlaces = 0;
      this.edCapacity.Location = new System.Drawing.Point(259, 28);
      this.edCapacity.Name = "edCapacity";
      this.edCapacity.Size = new System.Drawing.Size(134, 20);
      this.edCapacity.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(12, 28);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(221, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "Макс. число элементов кэша";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.panel1);
      this.panel3.Controls.Add(this.groupBox1);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel3.Location = new System.Drawing.Point(0, 0);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(634, 159);
      this.panel3.TabIndex = 2;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
      this.panel1.Location = new System.Drawing.Point(530, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(104, 159);
      this.panel1.TabIndex = 2;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(8, 38);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.grObjs);
      this.groupBox2.Controls.Add(this.edThreads);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Location = new System.Drawing.Point(0, 159);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(634, 209);
      this.groupBox2.TabIndex = 3;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Параметры тестирования";
      // 
      // grObjs
      // 
      this.grObjs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grObjs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grObjs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colType,
            this.colPersistance,
            this.colDel,
            this.colSize,
            this.colKeys,
            this.colValues});
      this.grObjs.Location = new System.Drawing.Point(3, 65);
      this.grObjs.Name = "grObjs";
      this.grObjs.RowHeadersVisible = false;
      this.grObjs.Size = new System.Drawing.Size(631, 143);
      this.grObjs.TabIndex = 3;
      // 
      // edThreads
      // 
      this.edThreads.Location = new System.Drawing.Point(259, 29);
      this.edThreads.Maximum = 64;
      this.edThreads.Minimum = 1;
      this.edThreads.Name = "edThreads";
      this.edThreads.Size = new System.Drawing.Size(134, 20);
      this.edThreads.TabIndex = 1;
      this.edThreads.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.edThreads.Value = 1;
      this.edThreads.Increment = 1;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(12, 29);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(221, 20);
      this.label2.TabIndex = 0;
      this.label2.Text = "Число потоков";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // colType
      // 
      this.colType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.colType.DataPropertyName = "ObjType";
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      this.colType.DefaultCellStyle = dataGridViewCellStyle1;
      this.colType.HeaderText = "Тип объекта";
      this.colType.Name = "colType";
      // 
      // colPersistance
      // 
      this.colPersistance.DataPropertyName = "Persistance";
      this.colPersistance.HeaderText = "Хранение";
      this.colPersistance.Name = "colPersistance";
      this.colPersistance.Resizable = System.Windows.Forms.DataGridViewTriState.True;
      this.colPersistance.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
      this.colPersistance.Width = 200;
      // 
      // colDel
      // 
      this.colDel.DataPropertyName = "AllowDelete";
      this.colDel.HeaderText = "Удалять";
      this.colDel.Name = "colDel";
      this.colDel.Width = 24;
      // 
      // colSize
      // 
      this.colSize.DataPropertyName = "Size";
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      this.colSize.DefaultCellStyle = dataGridViewCellStyle2;
      this.colSize.HeaderText = "Размер";
      this.colSize.Name = "colSize";
      // 
      // colKeys
      // 
      this.colKeys.DataPropertyName = "KeyCount";
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
      this.colKeys.DefaultCellStyle = dataGridViewCellStyle3;
      this.colKeys.HeaderText = "Кол-во ключей";
      this.colKeys.Name = "colKeys";
      this.colKeys.Width = 30;
      // 
      // colValues
      // 
      this.colValues.DataPropertyName = "ValueCount";
      dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
      this.colValues.DefaultCellStyle = dataGridViewCellStyle4;
      this.colValues.HeaderText = "Число значений ключа";
      this.colValues.Name = "colValues";
      // 
      // ParamForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(634, 368);
      this.Controls.Add(this.panel2);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ParamForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Тестирование кэша";
      this.panel2.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.panel3.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grObjs)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.GroupBox groupBox1;
    private FreeLibSet.Controls.IntEditBox edCapacity;
    private System.Windows.Forms.Label label1;
    private FreeLibSet.Controls.IntEditBox edLowMemorySize;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private FreeLibSet.Controls.IntEditBox edCriticalMemoryLoad;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label7;
    private FreeLibSet.Controls.IntEditBox eCheckMemoryInterval;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.DataGridView grObjs;
    private FreeLibSet.Controls.IntEditBox edThreads;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.DataGridViewTextBoxColumn colType;
    private System.Windows.Forms.DataGridViewComboBoxColumn colPersistance;
    private System.Windows.Forms.DataGridViewCheckBoxColumn colDel;
    private System.Windows.Forms.DataGridViewTextBoxColumn colSize;
    private System.Windows.Forms.DataGridViewTextBoxColumn colKeys;
    private System.Windows.Forms.DataGridViewTextBoxColumn colValues;
  }
}

