namespace FreeLibSet.Controls
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TwoListSelector));
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
      resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
      this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.groupBox2, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 1);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      // 
      // groupBox1
      // 
      resources.ApplyResources(this.groupBox1, "groupBox1");
      this.groupBox1.Controls.Add(this.AvailableGrid);
      this.groupBox1.Name = "groupBox1";
      this.tableLayoutPanel1.SetRowSpan(this.groupBox1, 3);
      this.groupBox1.TabStop = false;
      // 
      // AvailableGrid
      // 
      resources.ApplyResources(this.AvailableGrid, "AvailableGrid");
      this.AvailableGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.AvailableGrid.Name = "AvailableGrid";
      // 
      // groupBox2
      // 
      resources.ApplyResources(this.groupBox2, "groupBox2");
      this.groupBox2.Controls.Add(this.SelectedGrid);
      this.groupBox2.Controls.Add(this.SelectedToolBarPanel);
      this.groupBox2.Name = "groupBox2";
      this.tableLayoutPanel1.SetRowSpan(this.groupBox2, 3);
      this.groupBox2.TabStop = false;
      // 
      // SelectedGrid
      // 
      resources.ApplyResources(this.SelectedGrid, "SelectedGrid");
      this.SelectedGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.SelectedGrid.Name = "SelectedGrid";
      // 
      // SelectedToolBarPanel
      // 
      resources.ApplyResources(this.SelectedToolBarPanel, "SelectedToolBarPanel");
      this.SelectedToolBarPanel.Name = "SelectedToolBarPanel";
      // 
      // panel1
      // 
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Controls.Add(this.RemoveButton);
      this.panel1.Controls.Add(this.AddButton);
      this.panel1.Name = "panel1";
      // 
      // RemoveButton
      // 
      resources.ApplyResources(this.RemoveButton, "RemoveButton");
      this.RemoveButton.Name = "RemoveButton";
      this.RemoveButton.UseVisualStyleBackColor = true;
      // 
      // AddButton
      // 
      resources.ApplyResources(this.AddButton, "AddButton");
      this.AddButton.Name = "AddButton";
      this.AddButton.UseVisualStyleBackColor = true;
      // 
      // TwoListSelector
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Name = "TwoListSelector";
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
