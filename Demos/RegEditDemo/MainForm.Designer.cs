namespace RegeditDemo
{
  partial class MainForm
  {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      btnSetPath = new System.Windows.Forms.Button();
      edPath = new System.Windows.Forms.TextBox();
      label1 = new System.Windows.Forms.Label();
      splitContainer1 = new System.Windows.Forms.SplitContainer();
      tvTree = new FreeLibSet.Controls.TreeViewAdv();
      panSpbTree = new System.Windows.Forms.Panel();
      grValues = new System.Windows.Forms.DataGridView();
      lblError = new FreeLibSet.Controls.InfoLabel();
      panSpbValues = new System.Windows.Forms.Panel();
      tableLayoutPanel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
      splitContainer1.Panel1.SuspendLayout();
      splitContainer1.Panel2.SuspendLayout();
      splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)grValues).BeginInit();
      SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      tableLayoutPanel1.AutoSize = true;
      tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      tableLayoutPanel1.ColumnCount = 3;
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.Controls.Add(btnSetPath, 0, 0);
      tableLayoutPanel1.Controls.Add(edPath, 0, 0);
      tableLayoutPanel1.Controls.Add(label1, 0, 0);
      tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
      tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
      tableLayoutPanel1.Name = "tableLayoutPanel1";
      tableLayoutPanel1.RowCount = 1;
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel1.Size = new System.Drawing.Size(817, 32);
      tableLayoutPanel1.TabIndex = 0;
      // 
      // btnSetPath
      // 
      btnSetPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
      btnSetPath.Location = new System.Drawing.Point(776, 2);
      btnSetPath.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
      btnSetPath.Name = "btnSetPath";
      btnSetPath.Size = new System.Drawing.Size(37, 28);
      btnSetPath.TabIndex = 5;
      btnSetPath.UseVisualStyleBackColor = true;
      // 
      // edPath
      // 
      edPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
      edPath.Location = new System.Drawing.Point(59, 2);
      edPath.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
      edPath.Name = "edPath";
      edPath.Size = new System.Drawing.Size(709, 23);
      edPath.TabIndex = 4;
      // 
      // label1
      // 
      label1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
      label1.Location = new System.Drawing.Point(4, 0);
      label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(47, 32);
      label1.TabIndex = 3;
      label1.Text = "Путь";
      label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // splitContainer1
      // 
      splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      splitContainer1.Location = new System.Drawing.Point(0, 32);
      splitContainer1.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
      splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      splitContainer1.Panel1.Controls.Add(tvTree);
      splitContainer1.Panel1.Controls.Add(panSpbTree);
      // 
      // splitContainer1.Panel2
      // 
      splitContainer1.Panel2.Controls.Add(grValues);
      splitContainer1.Panel2.Controls.Add(lblError);
      splitContainer1.Panel2.Controls.Add(panSpbValues);
      splitContainer1.Size = new System.Drawing.Size(817, 358);
      splitContainer1.SplitterDistance = 269;
      splitContainer1.SplitterWidth = 5;
      splitContainer1.TabIndex = 1;
      // 
      // tvTree
      // 
      tvTree.Dock = System.Windows.Forms.DockStyle.Fill;
      tvTree.ForeColor = System.Drawing.SystemColors.ControlText;
      tvTree.Location = new System.Drawing.Point(0, 32);
      tvTree.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
      tvTree.Name = "tvTree";
      tvTree.Size = new System.Drawing.Size(269, 326);
      tvTree.TabIndex = 1;
      // 
      // panSpbTree
      // 
      panSpbTree.Dock = System.Windows.Forms.DockStyle.Top;
      panSpbTree.Location = new System.Drawing.Point(0, 0);
      panSpbTree.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
      panSpbTree.Name = "panSpbTree";
      panSpbTree.Size = new System.Drawing.Size(269, 32);
      panSpbTree.TabIndex = 0;
      // 
      // grValues
      // 
      grValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      grValues.Dock = System.Windows.Forms.DockStyle.Fill;
      grValues.Location = new System.Drawing.Point(0, 32);
      grValues.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
      grValues.Name = "grValues";
      grValues.RowHeadersWidth = 51;
      grValues.RowTemplate.Height = 29;
      grValues.Size = new System.Drawing.Size(543, 289);
      grValues.TabIndex = 4;
      // 
      // lblError
      // 
      lblError.AutoSize = true;
      lblError.Dock = System.Windows.Forms.DockStyle.Bottom;
      lblError.Icon = System.Windows.Forms.MessageBoxIcon.Hand;
      lblError.IconSize = FreeLibSet.Controls.MessageBoxIconSize.Large;
      lblError.Location = new System.Drawing.Point(0, 326);
      lblError.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
      lblError.Name = "lblError";
      lblError.Size = new System.Drawing.Size(543, 37);
      lblError.TabIndex = 2;
      lblError.Text = "???";
      // 
      // panSpbValues
      // 
      panSpbValues.Dock = System.Windows.Forms.DockStyle.Top;
      panSpbValues.Location = new System.Drawing.Point(0, 0);
      panSpbValues.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
      panSpbValues.Name = "panSpbValues";
      panSpbValues.Size = new System.Drawing.Size(543, 32);
      panSpbValues.TabIndex = 1;
      // 
      // MainForm
      // 
      AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      ClientSize = new System.Drawing.Size(817, 390);
      Controls.Add(splitContainer1);
      Controls.Add(tableLayoutPanel1);
      Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
      Name = "MainForm";
      Text = "Редактор реестра";
      tableLayoutPanel1.ResumeLayout(false);
      tableLayoutPanel1.PerformLayout();
      splitContainer1.Panel1.ResumeLayout(false);
      splitContainer1.Panel2.ResumeLayout(false);
      splitContainer1.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
      splitContainer1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)grValues).EndInit();
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion

    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.Panel panSpbTree;
    private FreeLibSet.Controls.TreeViewAdv tvTree;
    private System.Windows.Forms.Panel panSpbValues;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Button btnSetPath;
    private System.Windows.Forms.TextBox edPath;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.DataGridView grValues;
    private FreeLibSet.Controls.InfoLabel lblError;
  }
}
