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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EFPDataGridViewOrderForm));
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.tpFixed = new System.Windows.Forms.TabPage();
      this.grpPredefined = new System.Windows.Forms.GroupBox();
      this.grFixed = new System.Windows.Forms.DataGridView();
      this.tpCustom = new System.Windows.Forms.TabPage();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.grpAvailable = new System.Windows.Forms.GroupBox();
      this.grAvailable = new System.Windows.Forms.DataGridView();
      this.grpSelected = new System.Windows.Forms.GroupBox();
      this.grSelected = new System.Windows.Forms.DataGridView();
      this.panSpbSelected = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.btnRemove = new System.Windows.Forms.Button();
      this.btnAdd = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.TheTabControl.SuspendLayout();
      this.tpFixed.SuspendLayout();
      this.grpPredefined.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grFixed)).BeginInit();
      this.tpCustom.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.grpAvailable.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grAvailable)).BeginInit();
      this.grpSelected.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grSelected)).BeginInit();
      this.panel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      resources.ApplyResources(this.panel1, "panel1");
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Name = "panel1";
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      // 
      // btnOk
      // 
      resources.ApplyResources(this.btnOk, "btnOk");
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Name = "btnOk";
      this.btnOk.UseVisualStyleBackColor = true;
      // 
      // TheTabControl
      // 
      resources.ApplyResources(this.TheTabControl, "TheTabControl");
      this.TheTabControl.Controls.Add(this.tpFixed);
      this.TheTabControl.Controls.Add(this.tpCustom);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      // 
      // tpFixed
      // 
      resources.ApplyResources(this.tpFixed, "tpFixed");
      this.tpFixed.Controls.Add(this.grpPredefined);
      this.tpFixed.Name = "tpFixed";
      this.tpFixed.UseVisualStyleBackColor = true;
      // 
      // grpPredefined
      // 
      resources.ApplyResources(this.grpPredefined, "grpPredefined");
      this.grpPredefined.Controls.Add(this.grFixed);
      this.grpPredefined.Name = "grpPredefined";
      this.grpPredefined.TabStop = false;
      // 
      // grFixed
      // 
      resources.ApplyResources(this.grFixed, "grFixed");
      this.grFixed.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grFixed.Name = "grFixed";
      // 
      // tpCustom
      // 
      resources.ApplyResources(this.tpCustom, "tpCustom");
      this.tpCustom.Controls.Add(this.tableLayoutPanel1);
      this.tpCustom.Name = "tpCustom";
      this.tpCustom.UseVisualStyleBackColor = true;
      // 
      // tableLayoutPanel1
      // 
      resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
      this.tableLayoutPanel1.Controls.Add(this.grpAvailable, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.grpSelected, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      // 
      // grpAvailable
      // 
      resources.ApplyResources(this.grpAvailable, "grpAvailable");
      this.grpAvailable.Controls.Add(this.grAvailable);
      this.grpAvailable.Name = "grpAvailable";
      this.grpAvailable.TabStop = false;
      // 
      // grAvailable
      // 
      resources.ApplyResources(this.grAvailable, "grAvailable");
      this.grAvailable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grAvailable.Name = "grAvailable";
      // 
      // grpSelected
      // 
      resources.ApplyResources(this.grpSelected, "grpSelected");
      this.grpSelected.Controls.Add(this.grSelected);
      this.grpSelected.Controls.Add(this.panSpbSelected);
      this.grpSelected.Name = "grpSelected";
      this.grpSelected.TabStop = false;
      // 
      // grSelected
      // 
      resources.ApplyResources(this.grSelected, "grSelected");
      this.grSelected.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grSelected.Name = "grSelected";
      // 
      // panSpbSelected
      // 
      resources.ApplyResources(this.panSpbSelected, "panSpbSelected");
      this.panSpbSelected.Name = "panSpbSelected";
      // 
      // panel2
      // 
      resources.ApplyResources(this.panel2, "panel2");
      this.panel2.Controls.Add(this.btnRemove);
      this.panel2.Controls.Add(this.btnAdd);
      this.panel2.Name = "panel2";
      // 
      // btnRemove
      // 
      resources.ApplyResources(this.btnRemove, "btnRemove");
      this.btnRemove.Name = "btnRemove";
      this.btnRemove.UseVisualStyleBackColor = true;
      // 
      // btnAdd
      // 
      resources.ApplyResources(this.btnAdd, "btnAdd");
      this.btnAdd.Name = "btnAdd";
      this.btnAdd.UseVisualStyleBackColor = true;
      // 
      // EFPDataGridViewOrderForm
      // 
      this.AcceptButton = this.btnOk;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.Controls.Add(this.TheTabControl);
      this.Controls.Add(this.panel1);
      this.Name = "EFPDataGridViewOrderForm";
      this.panel1.ResumeLayout(false);
      this.TheTabControl.ResumeLayout(false);
      this.tpFixed.ResumeLayout(false);
      this.grpPredefined.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grFixed)).EndInit();
      this.tpCustom.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.grpAvailable.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grAvailable)).EndInit();
      this.grpSelected.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grSelected)).EndInit();
      this.panel2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.TabPage tpFixed;
    private System.Windows.Forms.GroupBox grpPredefined;
    private System.Windows.Forms.TabPage tpCustom;
    private System.Windows.Forms.DataGridView grFixed;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.GroupBox grpAvailable;
    private System.Windows.Forms.DataGridView grAvailable;
    private System.Windows.Forms.GroupBox grpSelected;
    private System.Windows.Forms.DataGridView grSelected;
    private System.Windows.Forms.Panel panSpbSelected;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Button btnAdd;
    private System.Windows.Forms.Button btnRemove;
  }
}
