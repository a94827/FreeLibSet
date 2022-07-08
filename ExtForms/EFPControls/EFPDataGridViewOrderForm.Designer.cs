namespace FreeLibSet.Forms
{
  partial class EFPDataGridViewOrderForm
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.ThetabControl = new System.Windows.Forms.TabControl();
      this.tpFixed = new System.Windows.Forms.TabPage();
      this.tpCustom = new System.Windows.Forms.TabPage();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.grFixed = new System.Windows.Forms.DataGridView();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.grAvailable = new System.Windows.Forms.DataGridView();
      this.panSpbSelected = new System.Windows.Forms.Panel();
      this.grSelected = new System.Windows.Forms.DataGridView();
      this.panel2 = new System.Windows.Forms.Panel();
      this.btnAdd = new System.Windows.Forms.Button();
      this.btnRemove = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.ThetabControl.SuspendLayout();
      this.tpFixed.SuspendLayout();
      this.tpCustom.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grFixed)).BeginInit();
      this.tableLayoutPanel1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grAvailable)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.grSelected)).BeginInit();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 412);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(632, 40);
      this.panel1.TabIndex = 1;
      // 
      // ThetabControl
      // 
      this.ThetabControl.Controls.Add(this.tpFixed);
      this.ThetabControl.Controls.Add(this.tpCustom);
      this.ThetabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ThetabControl.Location = new System.Drawing.Point(0, 0);
      this.ThetabControl.Name = "ThetabControl";
      this.ThetabControl.SelectedIndex = 0;
      this.ThetabControl.Size = new System.Drawing.Size(632, 412);
      this.ThetabControl.TabIndex = 0;
      // 
      // tpFixed
      // 
      this.tpFixed.Controls.Add(this.groupBox1);
      this.tpFixed.Location = new System.Drawing.Point(4, 22);
      this.tpFixed.Name = "tpFixed";
      this.tpFixed.Padding = new System.Windows.Forms.Padding(3);
      this.tpFixed.Size = new System.Drawing.Size(624, 386);
      this.tpFixed.TabIndex = 0;
      this.tpFixed.Text = "Предопределенный";
      this.tpFixed.UseVisualStyleBackColor = true;
      // 
      // tpCustom
      // 
      this.tpCustom.Controls.Add(this.tableLayoutPanel1);
      this.tpCustom.Location = new System.Drawing.Point(4, 22);
      this.tpCustom.Name = "tpCustom";
      this.tpCustom.Padding = new System.Windows.Forms.Padding(3);
      this.tpCustom.Size = new System.Drawing.Size(624, 386);
      this.tpCustom.TabIndex = 1;
      this.tpCustom.Text = "Произвольный";
      this.tpCustom.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.grFixed);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(3, 3);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(618, 380);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Порядок сортировки";
      // 
      // grFixed
      // 
      this.grFixed.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grFixed.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grFixed.Location = new System.Drawing.Point(3, 16);
      this.grFixed.Name = "grFixed";
      this.grFixed.Size = new System.Drawing.Size(612, 361);
      this.grFixed.TabIndex = 0;
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.groupBox3, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 1;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(618, 380);
      this.tableLayoutPanel1.TabIndex = 0;
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
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(102, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.grAvailable);
      this.groupBox2.Location = new System.Drawing.Point(3, 3);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(276, 374);
      this.groupBox2.TabIndex = 0;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Доступные столбцы";
      // 
      // groupBox3
      // 
      this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox3.Controls.Add(this.grSelected);
      this.groupBox3.Controls.Add(this.panSpbSelected);
      this.groupBox3.Location = new System.Drawing.Point(339, 3);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(276, 374);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Выбранные столбцы";
      // 
      // grAvailable
      // 
      this.grAvailable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grAvailable.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grAvailable.Location = new System.Drawing.Point(3, 16);
      this.grAvailable.Name = "grAvailable";
      this.grAvailable.Size = new System.Drawing.Size(270, 355);
      this.grAvailable.TabIndex = 0;
      // 
      // panSpbSelected
      // 
      this.panSpbSelected.Dock = System.Windows.Forms.DockStyle.Top;
      this.panSpbSelected.Location = new System.Drawing.Point(3, 16);
      this.panSpbSelected.Name = "panSpbSelected";
      this.panSpbSelected.Size = new System.Drawing.Size(270, 38);
      this.panSpbSelected.TabIndex = 0;
      // 
      // grSelected
      // 
      this.grSelected.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grSelected.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grSelected.Location = new System.Drawing.Point(3, 54);
      this.grSelected.Name = "grSelected";
      this.grSelected.Size = new System.Drawing.Size(270, 317);
      this.grSelected.TabIndex = 1;
      // 
      // panel2
      // 
      this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.panel2.Controls.Add(this.btnRemove);
      this.panel2.Controls.Add(this.btnAdd);
      this.panel2.Location = new System.Drawing.Point(285, 3);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(48, 374);
      this.panel2.TabIndex = 1;
      // 
      // btnAdd
      // 
      this.btnAdd.Location = new System.Drawing.Point(8, 86);
      this.btnAdd.Name = "btnAdd";
      this.btnAdd.Size = new System.Drawing.Size(32, 24);
      this.btnAdd.TabIndex = 0;
      this.btnAdd.UseVisualStyleBackColor = true;
      // 
      // btnRemove
      // 
      this.btnRemove.Location = new System.Drawing.Point(8, 116);
      this.btnRemove.Name = "btnRemove";
      this.btnRemove.Size = new System.Drawing.Size(32, 24);
      this.btnRemove.TabIndex = 1;
      this.btnRemove.UseVisualStyleBackColor = true;
      // 
      // EFPDataGridViewOrderForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(632, 452);
      this.Controls.Add(this.ThetabControl);
      this.Controls.Add(this.panel1);
      this.Name = "EFPDataGridViewOrderForm";
      this.Text = "Порядок строк";
      this.AcceptButton = btnOk;
      this.CancelButton = btnCancel;
      this.panel1.ResumeLayout(false);
      this.ThetabControl.ResumeLayout(false);
      this.tpFixed.ResumeLayout(false);
      this.tpCustom.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grFixed)).EndInit();
      this.tableLayoutPanel1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grAvailable)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.grSelected)).EndInit();
      this.panel2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.TabControl ThetabControl;
    private System.Windows.Forms.TabPage tpFixed;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.TabPage tpCustom;
    private System.Windows.Forms.DataGridView grFixed;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.DataGridView grAvailable;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.DataGridView grSelected;
    private System.Windows.Forms.Panel panSpbSelected;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Button btnAdd;
    private System.Windows.Forms.Button btnRemove;
  }
}