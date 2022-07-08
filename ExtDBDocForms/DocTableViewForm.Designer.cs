using System.Windows.Forms;
namespace FreeLibSet.Forms.Docs
{
  partial class DocTableViewForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
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
      this.TheButtonPanel = new System.Windows.Forms.Panel();
      this.TheNoButton = new System.Windows.Forms.Button();
      this.TheCancelButton = new System.Windows.Forms.Button();
      this.TheOKButton = new System.Windows.Forms.Button();
      this.ControlPanel = new System.Windows.Forms.Panel();
      this.TheSplitter = new System.Windows.Forms.SplitContainer();
      this.panel2 = new System.Windows.Forms.Panel();
      this.GroupTree = new FreeLibSet.Controls.TreeViewAdv();
      this.GroupSpeedPanel = new System.Windows.Forms.Panel();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.GroupCBPanel = new System.Windows.Forms.Panel();
      this.label1 = new System.Windows.Forms.Label();
      this.GroupCB = new System.Windows.Forms.ComboBox();
      this.FilterGrid = new System.Windows.Forms.DataGridView();
      this.TheButtonPanel.SuspendLayout();
      this.ControlPanel.SuspendLayout();
      this.TheSplitter.Panel1.SuspendLayout();
      this.TheSplitter.Panel2.SuspendLayout();
      this.TheSplitter.SuspendLayout();
      this.panel2.SuspendLayout();
      this.GroupCBPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.FilterGrid)).BeginInit();
      this.SuspendLayout();
      // 
      // TheButtonPanel
      // 
      this.TheButtonPanel.Controls.Add(this.TheNoButton);
      this.TheButtonPanel.Controls.Add(this.TheCancelButton);
      this.TheButtonPanel.Controls.Add(this.TheOKButton);
      this.TheButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.TheButtonPanel.Location = new System.Drawing.Point(0, 262);
      this.TheButtonPanel.Name = "TheButtonPanel";
      this.TheButtonPanel.Size = new System.Drawing.Size(533, 40);
      this.TheButtonPanel.TabIndex = 3;
      this.TheButtonPanel.Visible = false;
      // 
      // TheNoButton
      // 
      this.TheNoButton.DialogResult = System.Windows.Forms.DialogResult.No;
      this.TheNoButton.Location = new System.Drawing.Point(198, 8);
      this.TheNoButton.Name = "TheNoButton";
      this.TheNoButton.Size = new System.Drawing.Size(88, 24);
      this.TheNoButton.TabIndex = 2;
      this.TheNoButton.Text = "&Нет";
      this.TheNoButton.UseVisualStyleBackColor = true;
      this.TheNoButton.Click += new System.EventHandler(this.FormNoButton_Click);
      // 
      // TheCancelButton
      // 
      this.TheCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.TheCancelButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.TheCancelButton.Location = new System.Drawing.Point(102, 8);
      this.TheCancelButton.Name = "TheCancelButton";
      this.TheCancelButton.Size = new System.Drawing.Size(88, 24);
      this.TheCancelButton.TabIndex = 1;
      this.TheCancelButton.Text = "Отмена";
      this.TheCancelButton.Click += new System.EventHandler(this.FormCancelButton_Click);
      // 
      // TheOKButton
      // 
      this.TheOKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.TheOKButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.TheOKButton.Location = new System.Drawing.Point(8, 8);
      this.TheOKButton.Name = "TheOKButton";
      this.TheOKButton.Size = new System.Drawing.Size(88, 24);
      this.TheOKButton.TabIndex = 0;
      this.TheOKButton.Text = "О&К";
      this.TheOKButton.Click += new System.EventHandler(this.FormOKButton_Click);
      // 
      // ControlPanel
      // 
      this.ControlPanel.Controls.Add(this.TheSplitter);
      this.ControlPanel.Controls.Add(this.GroupCBPanel);
      this.ControlPanel.Controls.Add(this.FilterGrid);
      this.ControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ControlPanel.Location = new System.Drawing.Point(0, 0);
      this.ControlPanel.Name = "ControlPanel";
      this.ControlPanel.Size = new System.Drawing.Size(533, 262);
      this.ControlPanel.TabIndex = 0;
      // 
      // TheSplitter
      // 
      this.TheSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheSplitter.Location = new System.Drawing.Point(0, 65);
      this.TheSplitter.Name = "TheSplitter";
      // 
      // TheSplitter.Panel1
      // 
      this.TheSplitter.Panel1.Controls.Add(this.panel2);
      this.TheSplitter.Panel1MinSize = 100;
      // 
      // TheSplitter.Panel2
      // 
      this.TheSplitter.Panel2.Controls.Add(this.MainPanel);
      this.TheSplitter.Panel2MinSize = 100;
      this.TheSplitter.Size = new System.Drawing.Size(533, 197);
      this.TheSplitter.SplitterDistance = 177;
      this.TheSplitter.SplitterIncrement = 8;
      this.TheSplitter.TabIndex = 0;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.GroupTree);
      this.panel2.Controls.Add(this.GroupSpeedPanel);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(177, 197);
      this.panel2.TabIndex = 1;
      // 
      // GroupTree
      // 
      this.GroupTree.Dock = System.Windows.Forms.DockStyle.Fill;
      this.GroupTree.ForeColor = System.Drawing.SystemColors.ControlText;
      this.GroupTree.Location = new System.Drawing.Point(0, 32);
      this.GroupTree.Name = "GroupTree";
      this.GroupTree.Size = new System.Drawing.Size(177, 165);
      this.GroupTree.TabIndex = 1;
      // 
      // GroupSpeedPanel
      // 
      this.GroupSpeedPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.GroupSpeedPanel.Location = new System.Drawing.Point(0, 0);
      this.GroupSpeedPanel.Name = "GroupSpeedPanel";
      this.GroupSpeedPanel.Size = new System.Drawing.Size(177, 32);
      this.GroupSpeedPanel.TabIndex = 0;
      // 
      // MainPanel
      // 
      this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(352, 197);
      this.MainPanel.TabIndex = 0;
      // 
      // GroupCBPanel
      // 
      this.GroupCBPanel.Controls.Add(this.label1);
      this.GroupCBPanel.Controls.Add(this.GroupCB);
      this.GroupCBPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.GroupCBPanel.Location = new System.Drawing.Point(0, 33);
      this.GroupCBPanel.Name = "GroupCBPanel";
      this.GroupCBPanel.Size = new System.Drawing.Size(533, 32);
      this.GroupCBPanel.TabIndex = 2;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(6, 6);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(63, 21);
      this.label1.TabIndex = 0;
      this.label1.Text = "&Группа";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // GroupCB
      // 
      this.GroupCB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.GroupCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.GroupCB.FormattingEnabled = true;
      this.GroupCB.Location = new System.Drawing.Point(75, 6);
      this.GroupCB.Name = "GroupCB";
      this.GroupCB.Size = new System.Drawing.Size(455, 21);
      this.GroupCB.TabIndex = 1;
      // 
      // FilterGrid
      // 
      this.FilterGrid.Dock = System.Windows.Forms.DockStyle.Top;
      this.FilterGrid.Enabled = false;
      this.FilterGrid.Location = new System.Drawing.Point(0, 0);
      this.FilterGrid.Name = "FilterGrid";
      this.FilterGrid.Size = new System.Drawing.Size(533, 33);
      this.FilterGrid.TabIndex = 1;
      this.FilterGrid.Visible = false;
      // 
      // DocTableViewForm
      // 
      this.AcceptButton = this.TheOKButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.TheCancelButton;
      this.ClientSize = new System.Drawing.Size(533, 302);
      this.Controls.Add(this.ControlPanel);
      this.Controls.Add(this.TheButtonPanel);
      this.Name = "DocTableViewForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.WindowsDefaultBounds;
      this.TheButtonPanel.ResumeLayout(false);
      this.ControlPanel.ResumeLayout(false);
      this.TheSplitter.Panel1.ResumeLayout(false);
      this.TheSplitter.Panel2.ResumeLayout(false);
      this.TheSplitter.ResumeLayout(false);
      this.panel2.ResumeLayout(false);
      this.GroupCBPanel.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.FilterGrid)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    private Panel TheButtonPanel;
    private Button TheOKButton;
    private Button TheCancelButton;
    private Button TheNoButton;
    internal SplitContainer TheSplitter;
    private Panel panel2;
    internal FreeLibSet.Controls.TreeViewAdv GroupTree;
    internal Panel GroupSpeedPanel;
    internal Panel MainPanel;
    internal Panel GroupCBPanel;
    private Label label1;
    internal ComboBox GroupCB;
    internal DataGridView FilterGrid;
    internal Panel ControlPanel;
  }
}