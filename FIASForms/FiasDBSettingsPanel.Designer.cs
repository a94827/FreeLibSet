namespace FreeLibSet.Controls.FIAS
{
  partial class FiasDBSettingsPanel
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
      this.label1 = new System.Windows.Forms.Label();
      this.cbRegionCodes = new FreeLibSet.Controls.UserSelComboBox();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbUseRoom = new System.Windows.Forms.CheckBox();
      this.cbUseHouse = new System.Windows.Forms.CheckBox();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.cbUseHistory = new System.Windows.Forms.CheckBox();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.cbUseDates = new System.Windows.Forms.CheckBox();
      this.cbUseIFNS = new System.Windows.Forms.CheckBox();
      this.cbUseOKTMO = new System.Windows.Forms.CheckBox();
      this.cbUseOKATO = new System.Windows.Forms.CheckBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(6, 19);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(100, 20);
      this.label1.TabIndex = 0;
      this.label1.Text = "Регионы";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbRegionCodes
      // 
      this.cbRegionCodes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbRegionCodes.Location = new System.Drawing.Point(130, 19);
      this.cbRegionCodes.Name = "cbRegionCodes";
      this.cbRegionCodes.Size = new System.Drawing.Size(384, 20);
      this.cbRegionCodes.TabIndex = 1;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbUseRoom);
      this.groupBox1.Controls.Add(this.cbUseHouse);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(520, 65);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Наличие таблиц";
      // 
      // cbUseRoom
      // 
      this.cbUseRoom.AutoSize = true;
      this.cbUseRoom.Location = new System.Drawing.Point(16, 42);
      this.cbUseRoom.Name = "cbUseRoom";
      this.cbUseRoom.Size = new System.Drawing.Size(147, 17);
      this.cbUseRoom.TabIndex = 1;
      this.cbUseRoom.Text = "Квартиры и помещения";
      this.cbUseRoom.UseVisualStyleBackColor = true;
      // 
      // cbUseHouse
      // 
      this.cbUseHouse.AutoSize = true;
      this.cbUseHouse.Location = new System.Drawing.Point(16, 19);
      this.cbUseHouse.Name = "cbUseHouse";
      this.cbUseHouse.Size = new System.Drawing.Size(128, 17);
      this.cbUseHouse.TabIndex = 0;
      this.cbUseHouse.Text = "Дома и сооружения";
      this.cbUseHouse.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.cbUseHistory);
      this.groupBox2.Controls.Add(this.label1);
      this.groupBox2.Controls.Add(this.cbRegionCodes);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox2.Location = new System.Drawing.Point(0, 65);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(520, 68);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Фильтрация";
      // 
      // cbUseHistory
      // 
      this.cbUseHistory.AutoSize = true;
      this.cbUseHistory.Location = new System.Drawing.Point(16, 45);
      this.cbUseHistory.Name = "cbUseHistory";
      this.cbUseHistory.Size = new System.Drawing.Size(149, 17);
      this.cbUseHistory.TabIndex = 2;
      this.cbUseHistory.Text = "Исторические сведения";
      this.cbUseHistory.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.cbUseDates);
      this.groupBox3.Controls.Add(this.cbUseIFNS);
      this.groupBox3.Controls.Add(this.cbUseOKTMO);
      this.groupBox3.Controls.Add(this.cbUseOKATO);
      this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox3.Location = new System.Drawing.Point(0, 133);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(520, 114);
      this.groupBox3.TabIndex = 2;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Колонки в таблицах";
      // 
      // cbUseDates
      // 
      this.cbUseDates.AutoSize = true;
      this.cbUseDates.Location = new System.Drawing.Point(16, 88);
      this.cbUseDates.Name = "cbUseDates";
      this.cbUseDates.Size = new System.Drawing.Size(149, 17);
      this.cbUseDates.TabIndex = 3;
      this.cbUseDates.Text = "Даты действия записей";
      this.cbUseDates.UseVisualStyleBackColor = true;
      // 
      // cbUseIFNS
      // 
      this.cbUseIFNS.AutoSize = true;
      this.cbUseIFNS.Location = new System.Drawing.Point(16, 65);
      this.cbUseIFNS.Name = "cbUseIFNS";
      this.cbUseIFNS.Size = new System.Drawing.Size(90, 17);
      this.cbUseIFNS.TabIndex = 2;
      this.cbUseIFNS.Text = "Коды ИФНС";
      this.cbUseIFNS.UseVisualStyleBackColor = true;
      // 
      // cbUseOKTMO
      // 
      this.cbUseOKTMO.AutoSize = true;
      this.cbUseOKTMO.Location = new System.Drawing.Point(16, 42);
      this.cbUseOKTMO.Name = "cbUseOKTMO";
      this.cbUseOKTMO.Size = new System.Drawing.Size(65, 17);
      this.cbUseOKTMO.TabIndex = 1;
      this.cbUseOKTMO.Text = "ОКТМО";
      this.cbUseOKTMO.UseVisualStyleBackColor = true;
      // 
      // cbUseOKATO
      // 
      this.cbUseOKATO.AutoSize = true;
      this.cbUseOKATO.Location = new System.Drawing.Point(16, 19);
      this.cbUseOKATO.Name = "cbUseOKATO";
      this.cbUseOKATO.Size = new System.Drawing.Size(63, 17);
      this.cbUseOKATO.TabIndex = 0;
      this.cbUseOKATO.Text = "ОКАТО";
      this.cbUseOKATO.UseVisualStyleBackColor = true;
      // 
      // FiasDBSettingsPanel
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.Controls.Add(this.groupBox3);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.Name = "FiasDBSettingsPanel";
      this.Size = new System.Drawing.Size(520, 247);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.groupBox2.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.GroupBox groupBox3;
    internal FreeLibSet.Controls.UserSelComboBox cbRegionCodes;
    internal System.Windows.Forms.CheckBox cbUseRoom;
    internal System.Windows.Forms.CheckBox cbUseHouse;
    internal System.Windows.Forms.CheckBox cbUseHistory;
    internal System.Windows.Forms.CheckBox cbUseOKATO;
    internal System.Windows.Forms.CheckBox cbUseOKTMO;
    internal System.Windows.Forms.CheckBox cbUseIFNS;
    internal System.Windows.Forms.CheckBox cbUseDates;
  }
}
