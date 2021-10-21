namespace FreeLibSet.Forms
{
  partial class ListSelectForm
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

    #region Windows TheForm Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.TheGroupBox = new System.Windows.Forms.GroupBox();
      this.TheLV = new System.Windows.Forms.ListView();
      this.TheColumn = new System.Windows.Forms.ColumnHeader();
      this.btnOk = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnCheckAll = new System.Windows.Forms.Button();
      this.btnUnCheckAll = new System.Windows.Forms.Button();
      this.btnCopy = new System.Windows.Forms.Button();
      this.btnPaste = new System.Windows.Forms.Button();
      this.btnMore = new System.Windows.Forms.Button();
      this.TheGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // TheGroupBox
      // 
      this.TheGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.TheGroupBox.Controls.Add(this.TheLV);
      this.TheGroupBox.Location = new System.Drawing.Point(12, 19);
      this.TheGroupBox.Name = "TheGroupBox";
      this.TheGroupBox.Size = new System.Drawing.Size(265, 231);
      this.TheGroupBox.TabIndex = 0;
      this.TheGroupBox.TabStop = false;
      // 
      // TheLV
      // 
      this.TheLV.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                  | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.TheLV.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TheColumn});
      this.TheLV.FullRowSelect = true;
      this.TheLV.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.TheLV.Location = new System.Drawing.Point(10, 19);
      this.TheLV.MultiSelect = false;
      this.TheLV.Name = "TheLV";
      this.TheLV.ShowGroups = false;
      this.TheLV.Size = new System.Drawing.Size(244, 206);
      this.TheLV.TabIndex = 0;
      this.TheLV.UseCompatibleStateImageBehavior = false;
      this.TheLV.View = System.Windows.Forms.View.Details;
      this.TheLV.DoubleClick += new System.EventHandler(this.TheLV_DoubleClick);
      this.TheLV.HideSelection = false;
      // 
      // TheColumn
      // 
      this.TheColumn.Width = 240;
      // 
      // btnOk
      // 
      this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(288, 12);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(122, 24);
      this.btnOk.TabIndex = 1;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(288, 41);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(122, 24);
      this.btnCancel.TabIndex = 2;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnCheckAll
      // 
      this.btnCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCheckAll.Location = new System.Drawing.Point(288, 88);
      this.btnCheckAll.Name = "btnCheckAll";
      this.btnCheckAll.Size = new System.Drawing.Size(122, 24);
      this.btnCheckAll.TabIndex = 3;
      this.btnCheckAll.Text = "Отметить все";
      this.btnCheckAll.UseVisualStyleBackColor = true;
      // 
      // btnUnCheckAll
      // 
      this.btnUnCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnUnCheckAll.Location = new System.Drawing.Point(288, 118);
      this.btnUnCheckAll.Name = "btnUnCheckAll";
      this.btnUnCheckAll.Size = new System.Drawing.Size(122, 24);
      this.btnUnCheckAll.TabIndex = 4;
      this.btnUnCheckAll.Text = "Снять отметки";
      this.btnUnCheckAll.UseVisualStyleBackColor = true;
      // 
      // btnCopy
      // 
      this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCopy.Location = new System.Drawing.Point(288, 148);
      this.btnCopy.Name = "btnCopy";
      this.btnCopy.Size = new System.Drawing.Size(32, 24);
      this.btnCopy.TabIndex = 5;
      this.btnCopy.UseVisualStyleBackColor = true;
      // 
      // btnPaste
      // 
      this.btnPaste.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPaste.Location = new System.Drawing.Point(326, 148);
      this.btnPaste.Name = "btnPaste";
      this.btnPaste.Size = new System.Drawing.Size(32, 24);
      this.btnPaste.TabIndex = 6;
      this.btnPaste.UseVisualStyleBackColor = true;
      // 
      // btnMore
      // 
      this.btnMore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnMore.Location = new System.Drawing.Point(378, 148);
      this.btnMore.Name = "btnMore";
      this.btnMore.Size = new System.Drawing.Size(32, 24);
      this.btnMore.TabIndex = 7;
      this.btnMore.UseVisualStyleBackColor = true;
      // 
      // ListSelectForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(422, 262);
      this.Controls.Add(this.btnMore);
      this.Controls.Add(this.btnPaste);
      this.Controls.Add(this.btnCopy);
      this.Controls.Add(this.btnUnCheckAll);
      this.Controls.Add(this.btnCheckAll);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOk);
      this.Controls.Add(this.TheGroupBox);
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(300, 200);
      this.Name = "ListSelectForm";
      this.Resize += new System.EventHandler(this.TheLV_Resize);
      this.TheGroupBox.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ColumnHeader TheColumn;
    public System.Windows.Forms.GroupBox TheGroupBox;
    public System.Windows.Forms.ListView TheLV;
    public System.Windows.Forms.Button btnOk;
    public System.Windows.Forms.Button btnCancel;
    public System.Windows.Forms.Button btnCheckAll;
    public System.Windows.Forms.Button btnUnCheckAll;
    private System.Windows.Forms.Button btnCopy;
    private System.Windows.Forms.Button btnPaste;
    private System.Windows.Forms.Button btnMore;
  }
}