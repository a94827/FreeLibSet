﻿namespace FreeLibSet.Forms.Reporting
{
  partial class BRPageSetupMargins
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BRPageSetupMargins));
      this.panMargins = new System.Windows.Forms.Panel();
      this.infoLabel1 = new FreeLibSet.Controls.InfoLabel();
      this.grpMargins = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.edTopMargin = new FreeLibSet.Controls.DecimalEditBox();
      this.edRightMargin = new FreeLibSet.Controls.DecimalEditBox();
      this.lblLeftMargin = new System.Windows.Forms.Label();
      this.lblTopMargin = new System.Windows.Forms.Label();
      this.edLeftMargin = new FreeLibSet.Controls.DecimalEditBox();
      this.lblRightMargin = new System.Windows.Forms.Label();
      this.lblBottomMargin = new System.Windows.Forms.Label();
      this.edBottomMargin = new FreeLibSet.Controls.DecimalEditBox();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.panMargins.SuspendLayout();
      this.grpMargins.SuspendLayout();
      this.tableLayoutPanel3.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // panMargins
      // 
      resources.ApplyResources(this.panMargins, "panMargins");
      this.panMargins.Controls.Add(this.infoLabel1);
      this.panMargins.Controls.Add(this.grpMargins);
      this.panMargins.Name = "panMargins";
      // 
      // infoLabel1
      // 
      resources.ApplyResources(this.infoLabel1, "infoLabel1");
      this.infoLabel1.Name = "infoLabel1";
      // 
      // grpMargins
      // 
      resources.ApplyResources(this.grpMargins, "grpMargins");
      this.grpMargins.Controls.Add(this.tableLayoutPanel3);
      this.grpMargins.Controls.Add(this.tableLayoutPanel1);
      this.grpMargins.Name = "grpMargins";
      this.grpMargins.TabStop = false;
      // 
      // tableLayoutPanel3
      // 
      resources.ApplyResources(this.tableLayoutPanel3, "tableLayoutPanel3");
      this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel2, 1, 0);
      this.tableLayoutPanel3.Name = "tableLayoutPanel3";
      // 
      // tableLayoutPanel2
      // 
      resources.ApplyResources(this.tableLayoutPanel2, "tableLayoutPanel2");
      this.tableLayoutPanel2.Controls.Add(this.edTopMargin, 1, 1);
      this.tableLayoutPanel2.Controls.Add(this.edRightMargin, 2, 3);
      this.tableLayoutPanel2.Controls.Add(this.lblLeftMargin, 0, 2);
      this.tableLayoutPanel2.Controls.Add(this.lblTopMargin, 1, 0);
      this.tableLayoutPanel2.Controls.Add(this.edLeftMargin, 0, 3);
      this.tableLayoutPanel2.Controls.Add(this.lblRightMargin, 2, 2);
      this.tableLayoutPanel2.Controls.Add(this.lblBottomMargin, 1, 4);
      this.tableLayoutPanel2.Controls.Add(this.edBottomMargin, 1, 5);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      // 
      // edTopMargin
      // 
      resources.ApplyResources(this.edTopMargin, "edTopMargin");
      this.edTopMargin.Format = "0.00";
      this.edTopMargin.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
      this.edTopMargin.Name = "edTopMargin";
      // 
      // edRightMargin
      // 
      resources.ApplyResources(this.edRightMargin, "edRightMargin");
      this.edRightMargin.Format = "0.00";
      this.edRightMargin.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
      this.edRightMargin.Name = "edRightMargin";
      // 
      // lblLeftMargin
      // 
      resources.ApplyResources(this.lblLeftMargin, "lblLeftMargin");
      this.lblLeftMargin.Name = "lblLeftMargin";
      // 
      // lblTopMargin
      // 
      resources.ApplyResources(this.lblTopMargin, "lblTopMargin");
      this.lblTopMargin.Name = "lblTopMargin";
      // 
      // edLeftMargin
      // 
      resources.ApplyResources(this.edLeftMargin, "edLeftMargin");
      this.edLeftMargin.Format = "0.00";
      this.edLeftMargin.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
      this.edLeftMargin.Name = "edLeftMargin";
      // 
      // lblRightMargin
      // 
      resources.ApplyResources(this.lblRightMargin, "lblRightMargin");
      this.lblRightMargin.Name = "lblRightMargin";
      // 
      // lblBottomMargin
      // 
      resources.ApplyResources(this.lblBottomMargin, "lblBottomMargin");
      this.lblBottomMargin.Name = "lblBottomMargin";
      // 
      // edBottomMargin
      // 
      resources.ApplyResources(this.edBottomMargin, "edBottomMargin");
      this.edBottomMargin.Format = "0.00";
      this.edBottomMargin.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
      this.edBottomMargin.Name = "edBottomMargin";
      // 
      // tableLayoutPanel1
      // 
      resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      // 
      // BRPageSetupMargins
      // 
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.panMargins);
      this.Name = "BRPageSetupMargins";
      this.panMargins.ResumeLayout(false);
      this.grpMargins.ResumeLayout(false);
      this.grpMargins.PerformLayout();
      this.tableLayoutPanel3.ResumeLayout(false);
      this.tableLayoutPanel3.PerformLayout();
      this.tableLayoutPanel2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panMargins;
    private System.Windows.Forms.GroupBox grpMargins;
    private FreeLibSet.Controls.DecimalEditBox edBottomMargin;
    private System.Windows.Forms.Label lblBottomMargin;
    private FreeLibSet.Controls.DecimalEditBox edTopMargin;
    private System.Windows.Forms.Label lblTopMargin;
    private FreeLibSet.Controls.DecimalEditBox edRightMargin;
    private System.Windows.Forms.Label lblRightMargin;
    private FreeLibSet.Controls.DecimalEditBox edLeftMargin;
    private System.Windows.Forms.Label lblLeftMargin;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
    private Controls.InfoLabel infoLabel1;
  }
}
