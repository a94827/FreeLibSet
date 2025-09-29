namespace WinFormsDemo.EFPClipboardDemo
{
  partial class EFPClipboardForm
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.btnGetImage = new System.Windows.Forms.Button();
      this.btnSetImage = new System.Windows.Forms.Button();
      this.btnGetTextMatrix = new System.Windows.Forms.Button();
      this.btnSetTextMatrix = new System.Windows.Forms.Button();
      this.btnGetText = new System.Windows.Forms.Button();
      this.btnSetText = new System.Windows.Forms.Button();
      this.btnGetData = new System.Windows.Forms.Button();
      this.btnSetData = new System.Windows.Forms.Button();
      this.btnGetDataObject = new System.Windows.Forms.Button();
      this.btnSetDataObject = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.cbErrorIfEmpty = new System.Windows.Forms.CheckBox();
      this.cbErrorHandling = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this.edRepeatDelay = new FreeLibSet.Controls.Int32EditBox();
      this.label3 = new System.Windows.Forms.Label();
      this.edRepeatCount = new FreeLibSet.Controls.Int32EditBox();
      this.label2 = new System.Windows.Forms.Label();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.btnExceptionView = new System.Windows.Forms.Button();
      this.edExceptionMessage = new System.Windows.Forms.TextBox();
      this.edExceptionClass = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox4 = new System.Windows.Forms.GroupBox();
      this.edClipboardContents = new System.Windows.Forms.TextBox();
      this.groupBox1.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.AutoSize = true;
      this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.groupBox1.Controls.Add(this.tableLayoutPanel1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(861, 171);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Methods";
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.AutoSize = true;
      this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.btnGetImage, 1, 4);
      this.tableLayoutPanel1.Controls.Add(this.btnSetImage, 0, 4);
      this.tableLayoutPanel1.Controls.Add(this.btnGetTextMatrix, 1, 3);
      this.tableLayoutPanel1.Controls.Add(this.btnSetTextMatrix, 0, 3);
      this.tableLayoutPanel1.Controls.Add(this.btnGetText, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.btnSetText, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.btnGetData, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.btnSetData, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.btnGetDataObject, 1, 0);
      this.tableLayoutPanel1.Controls.Add(this.btnSetDataObject, 0, 0);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 18);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 5;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.Size = new System.Drawing.Size(855, 150);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // btnGetImage
      // 
      this.btnGetImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnGetImage.Location = new System.Drawing.Point(430, 123);
      this.btnGetImage.Name = "btnGetImage";
      this.btnGetImage.Size = new System.Drawing.Size(422, 24);
      this.btnGetImage.TabIndex = 9;
      this.btnGetImage.Text = "GetImage()";
      this.btnGetImage.UseVisualStyleBackColor = true;
      // 
      // btnSetImage
      // 
      this.btnSetImage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSetImage.Location = new System.Drawing.Point(3, 123);
      this.btnSetImage.Name = "btnSetImage";
      this.btnSetImage.Size = new System.Drawing.Size(421, 24);
      this.btnSetImage.TabIndex = 8;
      this.btnSetImage.Text = "SetImage()";
      this.btnSetImage.UseVisualStyleBackColor = true;
      // 
      // btnGetTextMatrix
      // 
      this.btnGetTextMatrix.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnGetTextMatrix.Location = new System.Drawing.Point(430, 93);
      this.btnGetTextMatrix.Name = "btnGetTextMatrix";
      this.btnGetTextMatrix.Size = new System.Drawing.Size(422, 24);
      this.btnGetTextMatrix.TabIndex = 7;
      this.btnGetTextMatrix.Text = "GetTextMatrix()";
      this.btnGetTextMatrix.UseVisualStyleBackColor = true;
      // 
      // btnSetTextMatrix
      // 
      this.btnSetTextMatrix.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSetTextMatrix.Location = new System.Drawing.Point(3, 93);
      this.btnSetTextMatrix.Name = "btnSetTextMatrix";
      this.btnSetTextMatrix.Size = new System.Drawing.Size(421, 24);
      this.btnSetTextMatrix.TabIndex = 6;
      this.btnSetTextMatrix.Text = "SetTextMatrix()";
      this.btnSetTextMatrix.UseVisualStyleBackColor = true;
      // 
      // btnGetText
      // 
      this.btnGetText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnGetText.Location = new System.Drawing.Point(430, 63);
      this.btnGetText.Name = "btnGetText";
      this.btnGetText.Size = new System.Drawing.Size(422, 24);
      this.btnGetText.TabIndex = 5;
      this.btnGetText.Text = "GetText()";
      this.btnGetText.UseVisualStyleBackColor = true;
      // 
      // btnSetText
      // 
      this.btnSetText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSetText.Location = new System.Drawing.Point(3, 63);
      this.btnSetText.Name = "btnSetText";
      this.btnSetText.Size = new System.Drawing.Size(421, 24);
      this.btnSetText.TabIndex = 4;
      this.btnSetText.Text = "SetText()";
      this.btnSetText.UseVisualStyleBackColor = true;
      // 
      // btnGetData
      // 
      this.btnGetData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnGetData.Location = new System.Drawing.Point(430, 33);
      this.btnGetData.Name = "btnGetData";
      this.btnGetData.Size = new System.Drawing.Size(422, 24);
      this.btnGetData.TabIndex = 3;
      this.btnGetData.Text = "GetData()";
      this.btnGetData.UseVisualStyleBackColor = true;
      // 
      // btnSetData
      // 
      this.btnSetData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSetData.Location = new System.Drawing.Point(3, 33);
      this.btnSetData.Name = "btnSetData";
      this.btnSetData.Size = new System.Drawing.Size(421, 24);
      this.btnSetData.TabIndex = 2;
      this.btnSetData.Text = "SetData()";
      this.btnSetData.UseVisualStyleBackColor = true;
      // 
      // btnGetDataObject
      // 
      this.btnGetDataObject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnGetDataObject.Location = new System.Drawing.Point(430, 3);
      this.btnGetDataObject.Name = "btnGetDataObject";
      this.btnGetDataObject.Size = new System.Drawing.Size(422, 24);
      this.btnGetDataObject.TabIndex = 1;
      this.btnGetDataObject.Text = "GetDataObject()";
      this.btnGetDataObject.UseVisualStyleBackColor = true;
      // 
      // btnSetDataObject
      // 
      this.btnSetDataObject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnSetDataObject.Location = new System.Drawing.Point(3, 3);
      this.btnSetDataObject.Name = "btnSetDataObject";
      this.btnSetDataObject.Size = new System.Drawing.Size(421, 24);
      this.btnSetDataObject.TabIndex = 0;
      this.btnSetDataObject.Text = "SetDataObject()";
      this.btnSetDataObject.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.cbErrorIfEmpty);
      this.groupBox2.Controls.Add(this.cbErrorHandling);
      this.groupBox2.Controls.Add(this.label4);
      this.groupBox2.Controls.Add(this.edRepeatDelay);
      this.groupBox2.Controls.Add(this.label3);
      this.groupBox2.Controls.Add(this.edRepeatCount);
      this.groupBox2.Controls.Add(this.label2);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox2.Location = new System.Drawing.Point(0, 171);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(861, 119);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "EFPClipboard controlling properties";
      // 
      // cbErrorIfEmpty
      // 
      this.cbErrorIfEmpty.AutoSize = true;
      this.cbErrorIfEmpty.Location = new System.Drawing.Point(316, 74);
      this.cbErrorIfEmpty.Name = "cbErrorIfEmpty";
      this.cbErrorIfEmpty.Size = new System.Drawing.Size(108, 21);
      this.cbErrorIfEmpty.TabIndex = 6;
      this.cbErrorIfEmpty.Text = "ErrorIfEmpty";
      this.cbErrorIfEmpty.UseVisualStyleBackColor = true;
      // 
      // cbErrorHandling
      // 
      this.cbErrorHandling.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbErrorHandling.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbErrorHandling.FormattingEnabled = true;
      this.cbErrorHandling.Location = new System.Drawing.Point(438, 33);
      this.cbErrorHandling.Name = "cbErrorHandling";
      this.cbErrorHandling.Size = new System.Drawing.Size(411, 24);
      this.cbErrorHandling.TabIndex = 5;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(313, 38);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(97, 20);
      this.label4.TabIndex = 4;
      this.label4.Text = "ErrorHandling";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edRepeatDelay
      // 
      this.edRepeatDelay.Location = new System.Drawing.Point(142, 72);
      this.edRepeatDelay.Maximum = 10000;
      this.edRepeatDelay.Minimum = 1;
      this.edRepeatDelay.Name = "edRepeatDelay";
      this.edRepeatDelay.Size = new System.Drawing.Size(150, 20);
      this.edRepeatDelay.TabIndex = 3;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(18, 73);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(97, 20);
      this.label3.TabIndex = 2;
      this.label3.Text = "RepeatDelay";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edRepeatCount
      // 
      this.edRepeatCount.Increment = 1;
      this.edRepeatCount.Location = new System.Drawing.Point(142, 37);
      this.edRepeatCount.Maximum = 100;
      this.edRepeatCount.Minimum = 1;
      this.edRepeatCount.Name = "edRepeatCount";
      this.edRepeatCount.Size = new System.Drawing.Size(150, 20);
      this.edRepeatCount.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(18, 38);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(97, 20);
      this.label2.TabIndex = 0;
      this.label2.Text = "RepeatCount";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.btnExceptionView);
      this.groupBox3.Controls.Add(this.edExceptionMessage);
      this.groupBox3.Controls.Add(this.edExceptionClass);
      this.groupBox3.Controls.Add(this.label1);
      this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox3.Location = new System.Drawing.Point(0, 290);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(861, 95);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "EFPClipboard result properties";
      // 
      // btnExceptionView
      // 
      this.btnExceptionView.Location = new System.Drawing.Point(817, 28);
      this.btnExceptionView.Name = "btnExceptionView";
      this.btnExceptionView.Size = new System.Drawing.Size(32, 24);
      this.btnExceptionView.TabIndex = 3;
      this.btnExceptionView.UseVisualStyleBackColor = true;
      // 
      // edExceptionMessage
      // 
      this.edExceptionMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edExceptionMessage.Location = new System.Drawing.Point(140, 56);
      this.edExceptionMessage.Name = "edExceptionMessage";
      this.edExceptionMessage.ReadOnly = true;
      this.edExceptionMessage.Size = new System.Drawing.Size(654, 22);
      this.edExceptionMessage.TabIndex = 2;
      // 
      // edExceptionClass
      // 
      this.edExceptionClass.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.edExceptionClass.Location = new System.Drawing.Point(140, 28);
      this.edExceptionClass.Name = "edExceptionClass";
      this.edExceptionClass.ReadOnly = true;
      this.edExceptionClass.Size = new System.Drawing.Size(654, 22);
      this.edExceptionClass.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(15, 28);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 22);
      this.label1.TabIndex = 0;
      this.label1.Text = "Exception";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.edClipboardContents);
      this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox4.Location = new System.Drawing.Point(0, 385);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(861, 184);
      this.groupBox4.TabIndex = 3;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Current clipboard contents";
      // 
      // edClipboardContents
      // 
      this.edClipboardContents.Dock = System.Windows.Forms.DockStyle.Fill;
      this.edClipboardContents.Location = new System.Drawing.Point(3, 18);
      this.edClipboardContents.Multiline = true;
      this.edClipboardContents.Name = "edClipboardContents";
      this.edClipboardContents.ReadOnly = true;
      this.edClipboardContents.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.edClipboardContents.Size = new System.Drawing.Size(855, 163);
      this.edClipboardContents.TabIndex = 0;
      // 
      // EFPClipboardForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(861, 569);
      this.Controls.Add(this.groupBox4);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "EFPClipboardForm";
      this.Text = "EFPClipboardForm";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.tableLayoutPanel1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.Button btnExceptionView;
    private System.Windows.Forms.TextBox edExceptionMessage;
    private System.Windows.Forms.TextBox edExceptionClass;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnGetImage;
    private System.Windows.Forms.Button btnSetImage;
    private System.Windows.Forms.Button btnGetTextMatrix;
    private System.Windows.Forms.Button btnSetTextMatrix;
    private System.Windows.Forms.Button btnGetText;
    private System.Windows.Forms.Button btnSetText;
    private System.Windows.Forms.Button btnGetData;
    private System.Windows.Forms.Button btnSetData;
    private System.Windows.Forms.Button btnGetDataObject;
    private System.Windows.Forms.Button btnSetDataObject;
    private FreeLibSet.Controls.Int32EditBox edRepeatDelay;
    private System.Windows.Forms.Label label3;
    private FreeLibSet.Controls.Int32EditBox edRepeatCount;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.CheckBox cbErrorIfEmpty;
    private System.Windows.Forms.ComboBox cbErrorHandling;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.GroupBox groupBox4;
    private System.Windows.Forms.TextBox edClipboardContents;
  }
}