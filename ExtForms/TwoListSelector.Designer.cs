namespace AgeyevAV.ExtForms
{
  partial class TwoListSelector
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.AvailableGrid = new System.Windows.Forms.DataGridView();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.SelectedGrid = new System.Windows.Forms.DataGridView();
      this.SelectedToolBarPanel = new System.Windows.Forms.Panel();
      this.panel1 = new System.Windows.Forms.Panel();
      this.RemoveButton = new System.Windows.Forms.Button();
      this.AddButton = new System.Windows.Forms.Button();
      this.tableLayoutPanel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.AvailableGrid)).BeginInit();
      this.groupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.SelectedGrid)).BeginInit();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 46F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.groupBox2, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 1);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(600, 300);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.AvailableGrid);
      this.groupBox1.Location = new System.Drawing.Point(3, 3);
      this.groupBox1.Name = "groupBox1";
      this.tableLayoutPanel1.SetRowSpan(this.groupBox1, 3);
      this.groupBox1.Size = new System.Drawing.Size(271, 294);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Доступно";
      // 
      // AvailableGrid
      // 
      this.AvailableGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.AvailableGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.AvailableGrid.Location = new System.Drawing.Point(3, 16);
      this.AvailableGrid.Name = "AvailableGrid";
      this.AvailableGrid.Size = new System.Drawing.Size(265, 275);
      this.AvailableGrid.TabIndex = 0;
      // 
      // groupBox2
      // 
      this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox2.Controls.Add(this.SelectedGrid);
      this.groupBox2.Controls.Add(this.SelectedToolBarPanel);
      this.groupBox2.Location = new System.Drawing.Point(326, 3);
      this.groupBox2.Name = "groupBox2";
      this.tableLayoutPanel1.SetRowSpan(this.groupBox2, 3);
      this.groupBox2.Size = new System.Drawing.Size(271, 294);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Выбрано";
      // 
      // SelectedGrid
      // 
      this.SelectedGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.SelectedGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.SelectedGrid.Location = new System.Drawing.Point(3, 55);
      this.SelectedGrid.Name = "SelectedGrid";
      this.SelectedGrid.Size = new System.Drawing.Size(265, 236);
      this.SelectedGrid.TabIndex = 1;
      // 
      // SelectedToolBarPanel
      // 
      this.SelectedToolBarPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.SelectedToolBarPanel.Location = new System.Drawing.Point(3, 16);
      this.SelectedToolBarPanel.Name = "SelectedToolBarPanel";
      this.SelectedToolBarPanel.Size = new System.Drawing.Size(265, 39);
      this.SelectedToolBarPanel.TabIndex = 0;
      // 
      // panel1
      // 
      this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.panel1.Controls.Add(this.RemoveButton);
      this.panel1.Controls.Add(this.AddButton);
      this.panel1.Location = new System.Drawing.Point(280, 117);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(40, 66);
      this.panel1.TabIndex = 2;
      // 
      // RemoveButton
      // 
      this.RemoveButton.Location = new System.Drawing.Point(5, 33);
      this.RemoveButton.Name = "RemoveButton";
      this.RemoveButton.Size = new System.Drawing.Size(32, 24);
      this.RemoveButton.TabIndex = 4;
      this.RemoveButton.UseVisualStyleBackColor = true;
      // 
      // AddButton
      // 
      this.AddButton.Location = new System.Drawing.Point(5, 3);
      this.AddButton.Name = "AddButton";
      this.AddButton.Size = new System.Drawing.Size(32, 24);
      this.AddButton.TabIndex = 3;
      this.AddButton.UseVisualStyleBackColor = true;
      // 
      // TwoListSelector
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "TwoListSelector";
      this.Size = new System.Drawing.Size(600, 300);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.AvailableGrid)).EndInit();
      this.groupBox2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.SelectedGrid)).EndInit();
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Panel panel1;

    /// <summary>
    /// Список "Доступные"
    /// </summary>
    public System.Windows.Forms.DataGridView AvailableGrid;

    /// <summary>
    /// Список "Выбранные"
    /// </summary>
    public System.Windows.Forms.DataGridView SelectedGrid;

    /// <summary>
    /// Кнопка "Добавить"
    /// </summary>
    public System.Windows.Forms.Button AddButton;

    /// <summary>
    /// Кнопка "Удалить"
    /// </summary>
    public System.Windows.Forms.Button RemoveButton;

    /// <summary>
    /// Панель инструментов для списка выбранных элементов
    /// </summary>
    public System.Windows.Forms.Panel SelectedToolBarPanel;
  }
}
