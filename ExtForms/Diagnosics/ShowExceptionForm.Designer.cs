namespace FreeLibSet.Forms.Diagnostics
{
  partial class ShowExceptionForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowExceptionForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.edMessage = new System.Windows.Forms.TextBox();
      this.lblMessage = new System.Windows.Forms.Label();
      this.btnInner = new System.Windows.Forms.Button();
      this.edType = new System.Windows.Forms.TextBox();
      this.lblType = new System.Windows.Forms.Label();
      this.panel2 = new System.Windows.Forms.Panel();
      this.panStopShow = new System.Windows.Forms.Panel();
      this.label3 = new System.Windows.Forms.Label();
      this.cbStopShow = new System.Windows.Forms.CheckBox();
      this.panel3 = new System.Windows.Forms.Panel();
      this.btnClose = new System.Windows.Forms.Button();
      this.panLog = new System.Windows.Forms.Panel();
      this.btnOpenWith = new System.Windows.Forms.Button();
      this.OpenWithMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.btnDirExplorer = new System.Windows.Forms.Button();
      this.btnEdit = new System.Windows.Forms.Button();
      this.edLogPath = new System.Windows.Forms.TextBox();
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.tpObj = new System.Windows.Forms.TabPage();
      this.pg1 = new System.Windows.Forms.PropertyGrid();
      this.tpData = new System.Windows.Forms.TabPage();
      this.grData = new System.Windows.Forms.DataGridView();
      this.colData1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colData2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.colData3 = new System.Windows.Forms.DataGridViewButtonColumn();
      this.tpStack = new System.Windows.Forms.TabPage();
      this.grStack = new System.Windows.Forms.DataGridView();
      this.colStack1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.panel1.SuspendLayout();
      this.panel2.SuspendLayout();
      this.panStopShow.SuspendLayout();
      this.panel3.SuspendLayout();
      this.panLog.SuspendLayout();
      this.TheTabControl.SuspendLayout();
      this.tpObj.SuspendLayout();
      this.tpData.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grData)).BeginInit();
      this.tpStack.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grStack)).BeginInit();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.AccessibleDescription = null;
      this.panel1.AccessibleName = null;
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.BackgroundImage = null;
      this.panel1.Controls.Add(this.edMessage);
      this.panel1.Controls.Add(this.lblMessage);
      this.panel1.Controls.Add(this.btnInner);
      this.panel1.Controls.Add(this.edType);
      this.panel1.Controls.Add(this.lblType);
      this.panel1.Font = null;
      this.panel1.Name = "panel1";
      // 
      // edMessage
      // 
      this.edMessage.AccessibleDescription = null;
      this.edMessage.AccessibleName = null;
      resources.ApplyResources(this.edMessage, "edMessage");
      this.edMessage.BackgroundImage = null;
      this.edMessage.Font = null;
      this.edMessage.Name = "edMessage";
      this.edMessage.ReadOnly = true;
      // 
      // lblMessage
      // 
      this.lblMessage.AccessibleDescription = null;
      this.lblMessage.AccessibleName = null;
      resources.ApplyResources(this.lblMessage, "lblMessage");
      this.lblMessage.Font = null;
      this.lblMessage.Name = "lblMessage";
      // 
      // btnInner
      // 
      this.btnInner.AccessibleDescription = null;
      this.btnInner.AccessibleName = null;
      resources.ApplyResources(this.btnInner, "btnInner");
      this.btnInner.BackgroundImage = null;
      this.btnInner.Font = null;
      this.btnInner.Name = "btnInner";
      this.btnInner.UseVisualStyleBackColor = true;
      this.btnInner.Click += new System.EventHandler(this.cbInner_Click);
      // 
      // edType
      // 
      this.edType.AccessibleDescription = null;
      this.edType.AccessibleName = null;
      resources.ApplyResources(this.edType, "edType");
      this.edType.BackgroundImage = null;
      this.edType.Font = null;
      this.edType.Name = "edType";
      this.edType.ReadOnly = true;
      // 
      // lblType
      // 
      this.lblType.AccessibleDescription = null;
      this.lblType.AccessibleName = null;
      resources.ApplyResources(this.lblType, "lblType");
      this.lblType.Font = null;
      this.lblType.Name = "lblType";
      // 
      // panel2
      // 
      this.panel2.AccessibleDescription = null;
      this.panel2.AccessibleName = null;
      resources.ApplyResources(this.panel2, "panel2");
      this.panel2.BackgroundImage = null;
      this.panel2.Controls.Add(this.panStopShow);
      this.panel2.Controls.Add(this.panel3);
      this.panel2.Font = null;
      this.panel2.Name = "panel2";
      // 
      // panStopShow
      // 
      this.panStopShow.AccessibleDescription = null;
      this.panStopShow.AccessibleName = null;
      resources.ApplyResources(this.panStopShow, "panStopShow");
      this.panStopShow.BackgroundImage = null;
      this.panStopShow.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.panStopShow.Controls.Add(this.label3);
      this.panStopShow.Controls.Add(this.cbStopShow);
      this.panStopShow.Font = null;
      this.panStopShow.Name = "panStopShow";
      // 
      // label3
      // 
      this.label3.AccessibleDescription = null;
      this.label3.AccessibleName = null;
      resources.ApplyResources(this.label3, "label3");
      this.label3.BackColor = System.Drawing.SystemColors.Info;
      this.label3.Font = null;
      this.label3.ForeColor = System.Drawing.SystemColors.InfoText;
      this.label3.Name = "label3";
      // 
      // cbStopShow
      // 
      this.cbStopShow.AccessibleDescription = null;
      this.cbStopShow.AccessibleName = null;
      resources.ApplyResources(this.cbStopShow, "cbStopShow");
      this.cbStopShow.BackgroundImage = null;
      this.cbStopShow.Font = null;
      this.cbStopShow.Name = "cbStopShow";
      this.cbStopShow.UseVisualStyleBackColor = true;
      // 
      // panel3
      // 
      this.panel3.AccessibleDescription = null;
      this.panel3.AccessibleName = null;
      resources.ApplyResources(this.panel3, "panel3");
      this.panel3.BackgroundImage = null;
      this.panel3.Controls.Add(this.btnClose);
      this.panel3.Font = null;
      this.panel3.Name = "panel3";
      // 
      // btnClose
      // 
      this.btnClose.AccessibleDescription = null;
      this.btnClose.AccessibleName = null;
      resources.ApplyResources(this.btnClose, "btnClose");
      this.btnClose.BackgroundImage = null;
      this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnClose.Font = null;
      this.btnClose.Name = "btnClose";
      this.btnClose.UseVisualStyleBackColor = true;
      // 
      // panLog
      // 
      this.panLog.AccessibleDescription = null;
      this.panLog.AccessibleName = null;
      resources.ApplyResources(this.panLog, "panLog");
      this.panLog.BackgroundImage = null;
      this.panLog.Controls.Add(this.btnOpenWith);
      this.panLog.Controls.Add(this.btnDirExplorer);
      this.panLog.Controls.Add(this.btnEdit);
      this.panLog.Controls.Add(this.edLogPath);
      this.panLog.Font = null;
      this.panLog.Name = "panLog";
      // 
      // btnOpenWith
      // 
      this.btnOpenWith.AccessibleDescription = null;
      this.btnOpenWith.AccessibleName = null;
      resources.ApplyResources(this.btnOpenWith, "btnOpenWith");
      this.btnOpenWith.BackgroundImage = null;
      this.btnOpenWith.ContextMenuStrip = this.OpenWithMenu;
      this.btnOpenWith.Font = null;
      this.btnOpenWith.Name = "btnOpenWith";
      this.btnOpenWith.UseVisualStyleBackColor = true;
      // 
      // OpenWithMenu
      // 
      this.OpenWithMenu.AccessibleDescription = null;
      this.OpenWithMenu.AccessibleName = null;
      resources.ApplyResources(this.OpenWithMenu, "OpenWithMenu");
      this.OpenWithMenu.BackgroundImage = null;
      this.OpenWithMenu.Font = null;
      this.OpenWithMenu.Name = "OpenWithMenu";
      // 
      // btnDirExplorer
      // 
      this.btnDirExplorer.AccessibleDescription = null;
      this.btnDirExplorer.AccessibleName = null;
      resources.ApplyResources(this.btnDirExplorer, "btnDirExplorer");
      this.btnDirExplorer.BackgroundImage = null;
      this.btnDirExplorer.Font = null;
      this.btnDirExplorer.Name = "btnDirExplorer";
      this.btnDirExplorer.UseVisualStyleBackColor = true;
      this.btnDirExplorer.Click += new System.EventHandler(this.btnDirExplorer_Click);
      // 
      // btnEdit
      // 
      this.btnEdit.AccessibleDescription = null;
      this.btnEdit.AccessibleName = null;
      resources.ApplyResources(this.btnEdit, "btnEdit");
      this.btnEdit.BackgroundImage = null;
      this.btnEdit.Font = null;
      this.btnEdit.Name = "btnEdit";
      this.btnEdit.UseVisualStyleBackColor = true;
      // 
      // edLogPath
      // 
      this.edLogPath.AccessibleDescription = null;
      this.edLogPath.AccessibleName = null;
      resources.ApplyResources(this.edLogPath, "edLogPath");
      this.edLogPath.BackgroundImage = null;
      this.edLogPath.Font = null;
      this.edLogPath.Name = "edLogPath";
      this.edLogPath.ReadOnly = true;
      // 
      // TheTabControl
      // 
      this.TheTabControl.AccessibleDescription = null;
      this.TheTabControl.AccessibleName = null;
      resources.ApplyResources(this.TheTabControl, "TheTabControl");
      this.TheTabControl.BackgroundImage = null;
      this.TheTabControl.Controls.Add(this.tpObj);
      this.TheTabControl.Controls.Add(this.tpData);
      this.TheTabControl.Controls.Add(this.tpStack);
      this.TheTabControl.Font = null;
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      // 
      // tpObj
      // 
      this.tpObj.AccessibleDescription = null;
      this.tpObj.AccessibleName = null;
      resources.ApplyResources(this.tpObj, "tpObj");
      this.tpObj.BackgroundImage = null;
      this.tpObj.Controls.Add(this.pg1);
      this.tpObj.Font = null;
      this.tpObj.Name = "tpObj";
      this.tpObj.UseVisualStyleBackColor = true;
      // 
      // pg1
      // 
      this.pg1.AccessibleDescription = null;
      this.pg1.AccessibleName = null;
      resources.ApplyResources(this.pg1, "pg1");
      this.pg1.BackgroundImage = null;
      this.pg1.Font = null;
      this.pg1.Name = "pg1";
      this.pg1.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
      this.pg1.ToolbarVisible = false;
      // 
      // tpData
      // 
      this.tpData.AccessibleDescription = null;
      this.tpData.AccessibleName = null;
      resources.ApplyResources(this.tpData, "tpData");
      this.tpData.BackgroundImage = null;
      this.tpData.Controls.Add(this.grData);
      this.tpData.Font = null;
      this.tpData.Name = "tpData";
      this.tpData.UseVisualStyleBackColor = true;
      // 
      // grData
      // 
      this.grData.AccessibleDescription = null;
      this.grData.AccessibleName = null;
      this.grData.AllowUserToAddRows = false;
      this.grData.AllowUserToDeleteRows = false;
      this.grData.AllowUserToResizeColumns = false;
      this.grData.AllowUserToResizeRows = false;
      resources.ApplyResources(this.grData, "grData");
      this.grData.BackgroundImage = null;
      this.grData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colData1,
            this.colData2,
            this.colData3});
      this.grData.Font = null;
      this.grData.Name = "grData";
      this.grData.ReadOnly = true;
      this.grData.RowHeadersVisible = false;
      this.grData.StandardTab = true;
      this.grData.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grData_CellContentClick);
      // 
      // colData1
      // 
      this.colData1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.colData1.FillWeight = 50F;
      resources.ApplyResources(this.colData1, "colData1");
      this.colData1.Name = "colData1";
      this.colData1.ReadOnly = true;
      this.colData1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // colData2
      // 
      this.colData2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.colData2.FillWeight = 50F;
      resources.ApplyResources(this.colData2, "colData2");
      this.colData2.Name = "colData2";
      this.colData2.ReadOnly = true;
      this.colData2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // colData3
      // 
      resources.ApplyResources(this.colData3, "colData3");
      this.colData3.Name = "colData3";
      this.colData3.ReadOnly = true;
      // 
      // tpStack
      // 
      this.tpStack.AccessibleDescription = null;
      this.tpStack.AccessibleName = null;
      resources.ApplyResources(this.tpStack, "tpStack");
      this.tpStack.BackgroundImage = null;
      this.tpStack.Controls.Add(this.grStack);
      this.tpStack.Font = null;
      this.tpStack.Name = "tpStack";
      this.tpStack.UseVisualStyleBackColor = true;
      // 
      // grStack
      // 
      this.grStack.AccessibleDescription = null;
      this.grStack.AccessibleName = null;
      this.grStack.AllowUserToAddRows = false;
      this.grStack.AllowUserToDeleteRows = false;
      this.grStack.AllowUserToResizeColumns = false;
      this.grStack.AllowUserToResizeRows = false;
      resources.ApplyResources(this.grStack, "grStack");
      this.grStack.BackgroundImage = null;
      this.grStack.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grStack.ColumnHeadersVisible = false;
      this.grStack.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colStack1});
      this.grStack.Font = null;
      this.grStack.Name = "grStack";
      this.grStack.ReadOnly = true;
      this.grStack.RowHeadersVisible = false;
      this.grStack.RowTemplate.Height = 18;
      this.grStack.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.grStack.StandardTab = true;
      // 
      // colStack1
      // 
      this.colStack1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      resources.ApplyResources(this.colStack1, "colStack1");
      this.colStack1.Name = "colStack1";
      this.colStack1.ReadOnly = true;
      // 
      // ShowExceptionForm
      // 
      this.AccessibleDescription = null;
      this.AccessibleName = null;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackgroundImage = null;
      this.CancelButton = this.btnClose;
      this.Controls.Add(this.TheTabControl);
      this.Controls.Add(this.panLog);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.Font = null;
      this.MinimizeBox = false;
      this.Name = "ShowExceptionForm";
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.panel2.ResumeLayout(false);
      this.panStopShow.ResumeLayout(false);
      this.panStopShow.PerformLayout();
      this.panel3.ResumeLayout(false);
      this.panLog.ResumeLayout(false);
      this.panLog.PerformLayout();
      this.TheTabControl.ResumeLayout(false);
      this.tpObj.ResumeLayout(false);
      this.tpData.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grData)).EndInit();
      this.tpStack.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grStack)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.TextBox edType;
    private System.Windows.Forms.Label lblType;
    private System.Windows.Forms.Button btnInner;
    private System.Windows.Forms.TextBox edMessage;
    private System.Windows.Forms.Label lblMessage;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Panel panStopShow;
    private System.Windows.Forms.CheckBox cbStopShow;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.Panel panLog;
    private System.Windows.Forms.TextBox edLogPath;
    private System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.TabPage tpObj;
    private System.Windows.Forms.PropertyGrid pg1;
    private System.Windows.Forms.TabPage tpStack;
    private System.Windows.Forms.DataGridView grStack;
    private System.Windows.Forms.DataGridViewTextBoxColumn colStack1;
    private System.Windows.Forms.Button btnEdit;
    private System.Windows.Forms.TabPage tpData;
    private System.Windows.Forms.DataGridView grData;
    private System.Windows.Forms.Button btnDirExplorer;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button btnOpenWith;
    private System.Windows.Forms.ContextMenuStrip OpenWithMenu;
    private System.Windows.Forms.DataGridViewTextBoxColumn colData1;
    private System.Windows.Forms.DataGridViewTextBoxColumn colData2;
    private System.Windows.Forms.DataGridViewButtonColumn colData3;
  }
}
