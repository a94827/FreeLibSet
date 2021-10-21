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
      this.label2 = new System.Windows.Forms.Label();
      this.cbInner = new System.Windows.Forms.Button();
      this.TheImageList = new System.Windows.Forms.ImageList(this.components);
      this.edType = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
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
      this.panel1.Controls.Add(this.edMessage);
      this.panel1.Controls.Add(this.label2);
      this.panel1.Controls.Add(this.cbInner);
      this.panel1.Controls.Add(this.edType);
      this.panel1.Controls.Add(this.label1);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(632, 160);
      this.panel1.TabIndex = 0;
      // 
      // edMessage
      // 
      this.edMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edMessage.Location = new System.Drawing.Point(131, 38);
      this.edMessage.Multiline = true;
      this.edMessage.Name = "edMessage";
      this.edMessage.ReadOnly = true;
      this.edMessage.Size = new System.Drawing.Size(489, 116);
      this.edMessage.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 38);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(65, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Сообщение";
      // 
      // cbInner
      // 
      this.cbInner.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.cbInner.ImageKey = "View";
      this.cbInner.ImageList = this.TheImageList;
      this.cbInner.Location = new System.Drawing.Point(8, 81);
      this.cbInner.Name = "cbInner";
      this.cbInner.Size = new System.Drawing.Size(112, 47);
      this.cbInner.TabIndex = 4;
      this.cbInner.Text = "Вложенная ошибка";
      this.cbInner.UseVisualStyleBackColor = true;
      this.cbInner.Click += new System.EventHandler(this.cbInner_Click);
      // 
      // TheImageList
      // 
      this.TheImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("TheImageList.ImageStream")));
      this.TheImageList.TransparentColor = System.Drawing.Color.Magenta;
      this.TheImageList.Images.SetKeyName(0, "Cancel");
      this.TheImageList.Images.SetKeyName(1, "View");
      this.TheImageList.Images.SetKeyName(2, "Notepad");
      this.TheImageList.Images.SetKeyName(3, "WindowsExplorer");
      this.TheImageList.Images.SetKeyName(4, "MenuButton");
      this.TheImageList.Images.SetKeyName(5, "Copy");
      // 
      // edType
      // 
      this.edType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edType.Location = new System.Drawing.Point(131, 7);
      this.edType.Name = "edType";
      this.edType.ReadOnly = true;
      this.edType.Size = new System.Drawing.Size(489, 20);
      this.edType.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 10);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(67, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Тип ошибки";
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.panStopShow);
      this.panel2.Controls.Add(this.panel3);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel2.Location = new System.Drawing.Point(0, 355);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(632, 51);
      this.panel2.TabIndex = 3;
      // 
      // panStopShow
      // 
      this.panStopShow.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.panStopShow.Controls.Add(this.label3);
      this.panStopShow.Controls.Add(this.cbStopShow);
      this.panStopShow.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panStopShow.Location = new System.Drawing.Point(130, 0);
      this.panStopShow.Name = "panStopShow";
      this.panStopShow.Size = new System.Drawing.Size(502, 51);
      this.panStopShow.TabIndex = 1;
      // 
      // label3
      // 
      this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.label3.BackColor = System.Drawing.SystemColors.Info;
      this.label3.ForeColor = System.Drawing.SystemColors.InfoText;
      this.label3.Location = new System.Drawing.Point(198, 3);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(300, 44);
      this.label3.TabIndex = 1;
      this.label3.Text = "Отключите вывод сообщений только если программа \"зациклилась\" и сообщение появляе" +
          "тся непрерывно. После этого следует завершить работу программы";
      // 
      // cbStopShow
      // 
      this.cbStopShow.AutoSize = true;
      this.cbStopShow.Location = new System.Drawing.Point(4, 15);
      this.cbStopShow.Name = "cbStopShow";
      this.cbStopShow.Size = new System.Drawing.Size(181, 17);
      this.cbStopShow.TabIndex = 0;
      this.cbStopShow.Text = "Прекратить вывод сообщений";
      this.cbStopShow.UseVisualStyleBackColor = true;
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.btnClose);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
      this.panel3.Location = new System.Drawing.Point(0, 0);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(130, 51);
      this.panel3.TabIndex = 0;
      // 
      // btnClose
      // 
      this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnClose.ImageKey = "Cancel";
      this.btnClose.ImageList = this.TheImageList;
      this.btnClose.Location = new System.Drawing.Point(8, 13);
      this.btnClose.Name = "btnClose";
      this.btnClose.Size = new System.Drawing.Size(112, 24);
      this.btnClose.TabIndex = 0;
      this.btnClose.Text = "Закрыть";
      this.btnClose.UseVisualStyleBackColor = true;
      // 
      // panLog
      // 
      this.panLog.Controls.Add(this.btnOpenWith);
      this.panLog.Controls.Add(this.btnDirExplorer);
      this.panLog.Controls.Add(this.btnEdit);
      this.panLog.Controls.Add(this.edLogPath);
      this.panLog.Dock = System.Windows.Forms.DockStyle.Top;
      this.panLog.Location = new System.Drawing.Point(0, 160);
      this.panLog.Name = "panLog";
      this.panLog.Size = new System.Drawing.Size(632, 32);
      this.panLog.TabIndex = 1;
      // 
      // btnOpenWith
      // 
      this.btnOpenWith.ContextMenuStrip = this.OpenWithMenu;
      this.btnOpenWith.ImageKey = "MenuButton";
      this.btnOpenWith.ImageList = this.TheImageList;
      this.btnOpenWith.Location = new System.Drawing.Point(116, 4);
      this.btnOpenWith.Name = "btnOpenWith";
      this.btnOpenWith.Size = new System.Drawing.Size(32, 24);
      this.btnOpenWith.TabIndex = 1;
      this.btnOpenWith.UseVisualStyleBackColor = true;
      // 
      // OpenWithMenu
      // 
      this.OpenWithMenu.Name = "OpenWithMenu";
      this.OpenWithMenu.Size = new System.Drawing.Size(61, 4);
      // 
      // btnDirExplorer
      // 
      this.btnDirExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnDirExplorer.ImageKey = "WindowsExplorer";
      this.btnDirExplorer.ImageList = this.TheImageList;
      this.btnDirExplorer.Location = new System.Drawing.Point(593, 4);
      this.btnDirExplorer.Name = "btnDirExplorer";
      this.btnDirExplorer.Size = new System.Drawing.Size(32, 24);
      this.btnDirExplorer.TabIndex = 3;
      this.btnDirExplorer.UseVisualStyleBackColor = true;
      this.btnDirExplorer.Click += new System.EventHandler(this.btnDirExplorer_Click);
      // 
      // btnEdit
      // 
      this.btnEdit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnEdit.ImageKey = "Notepad";
      this.btnEdit.ImageList = this.TheImageList;
      this.btnEdit.Location = new System.Drawing.Point(8, 4);
      this.btnEdit.Name = "btnEdit";
      this.btnEdit.Size = new System.Drawing.Size(112, 24);
      this.btnEdit.TabIndex = 0;
      this.btnEdit.Text = "Отчет";
      this.btnEdit.UseVisualStyleBackColor = true;
      // 
      // edLogPath
      // 
      this.edLogPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edLogPath.Location = new System.Drawing.Point(154, 4);
      this.edLogPath.Name = "edLogPath";
      this.edLogPath.ReadOnly = true;
      this.edLogPath.Size = new System.Drawing.Size(433, 20);
      this.edLogPath.TabIndex = 2;
      // 
      // TheTabControl
      // 
      this.TheTabControl.Controls.Add(this.tpObj);
      this.TheTabControl.Controls.Add(this.tpData);
      this.TheTabControl.Controls.Add(this.tpStack);
      this.TheTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTabControl.Location = new System.Drawing.Point(0, 192);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(632, 163);
      this.TheTabControl.TabIndex = 2;
      // 
      // tpObj
      // 
      this.tpObj.Controls.Add(this.pg1);
      this.tpObj.Location = new System.Drawing.Point(4, 22);
      this.tpObj.Name = "tpObj";
      this.tpObj.Padding = new System.Windows.Forms.Padding(3);
      this.tpObj.Size = new System.Drawing.Size(624, 137);
      this.tpObj.TabIndex = 0;
      this.tpObj.Text = "Объект ошибки";
      this.tpObj.UseVisualStyleBackColor = true;
      // 
      // pg1
      // 
      this.pg1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pg1.HelpVisible = false;
      this.pg1.Location = new System.Drawing.Point(3, 3);
      this.pg1.Name = "pg1";
      this.pg1.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
      this.pg1.Size = new System.Drawing.Size(618, 131);
      this.pg1.TabIndex = 0;
      this.pg1.ToolbarVisible = false;
      // 
      // tpData
      // 
      this.tpData.Controls.Add(this.grData);
      this.tpData.Location = new System.Drawing.Point(4, 22);
      this.tpData.Name = "tpData";
      this.tpData.Padding = new System.Windows.Forms.Padding(3);
      this.tpData.Size = new System.Drawing.Size(624, 137);
      this.tpData.TabIndex = 2;
      this.tpData.Text = "Данные";
      this.tpData.UseVisualStyleBackColor = true;
      // 
      // grData
      // 
      this.grData.AllowUserToAddRows = false;
      this.grData.AllowUserToDeleteRows = false;
      this.grData.AllowUserToResizeColumns = false;
      this.grData.AllowUserToResizeRows = false;
      this.grData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colData1,
            this.colData2,
            this.colData3});
      this.grData.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grData.Location = new System.Drawing.Point(3, 3);
      this.grData.Name = "grData";
      this.grData.ReadOnly = true;
      this.grData.RowHeadersVisible = false;
      this.grData.Size = new System.Drawing.Size(618, 131);
      this.grData.StandardTab = true;
      this.grData.TabIndex = 0;
      this.grData.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grData_CellContentClick);
      // 
      // colData1
      // 
      this.colData1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.colData1.FillWeight = 50F;
      this.colData1.HeaderText = "Код";
      this.colData1.Name = "colData1";
      this.colData1.ReadOnly = true;
      this.colData1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // colData2
      // 
      this.colData2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.colData2.FillWeight = 50F;
      this.colData2.HeaderText = "Значение";
      this.colData2.Name = "colData2";
      this.colData2.ReadOnly = true;
      this.colData2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
      // 
      // colData3
      // 
      this.colData3.HeaderText = "";
      this.colData3.Name = "colData3";
      this.colData3.ReadOnly = true;
      this.colData3.Width = 30;
      // 
      // tpStack
      // 
      this.tpStack.Controls.Add(this.grStack);
      this.tpStack.Location = new System.Drawing.Point(4, 22);
      this.tpStack.Name = "tpStack";
      this.tpStack.Padding = new System.Windows.Forms.Padding(3);
      this.tpStack.Size = new System.Drawing.Size(624, 137);
      this.tpStack.TabIndex = 1;
      this.tpStack.Text = "Стек вызовов";
      this.tpStack.UseVisualStyleBackColor = true;
      // 
      // grStack
      // 
      this.grStack.AllowUserToAddRows = false;
      this.grStack.AllowUserToDeleteRows = false;
      this.grStack.AllowUserToResizeColumns = false;
      this.grStack.AllowUserToResizeRows = false;
      this.grStack.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grStack.ColumnHeadersVisible = false;
      this.grStack.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colStack1});
      this.grStack.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grStack.Location = new System.Drawing.Point(3, 3);
      this.grStack.Name = "grStack";
      this.grStack.ReadOnly = true;
      this.grStack.RowHeadersVisible = false;
      this.grStack.RowTemplate.Height = 18;
      this.grStack.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.grStack.Size = new System.Drawing.Size(618, 131);
      this.grStack.StandardTab = true;
      this.grStack.TabIndex = 0;
      // 
      // colStack1
      // 
      this.colStack1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      this.colStack1.HeaderText = "Column1";
      this.colStack1.Name = "colStack1";
      this.colStack1.ReadOnly = true;
      // 
      // ShowExceptionForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnClose;
      this.ClientSize = new System.Drawing.Size(632, 406);
      this.Controls.Add(this.TheTabControl);
      this.Controls.Add(this.panLog);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MinimizeBox = false;
      this.Name = "ShowExceptionForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Отладка сообщения об ошибке";
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
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button cbInner;
    private System.Windows.Forms.TextBox edMessage;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Panel panStopShow;
    private System.Windows.Forms.CheckBox cbStopShow;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Button btnClose;
    private System.Windows.Forms.ImageList TheImageList;
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
    private System.Windows.Forms.DataGridViewTextBoxColumn colData1;
    private System.Windows.Forms.DataGridViewTextBoxColumn colData2;
    private System.Windows.Forms.DataGridViewButtonColumn colData3;
    private System.Windows.Forms.Button btnDirExplorer;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Button btnOpenWith;
    private System.Windows.Forms.ContextMenuStrip OpenWithMenu;
  }
}