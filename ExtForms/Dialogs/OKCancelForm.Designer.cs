namespace FreeLibSet.Forms
{
  partial class OKCancelForm
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
      this.ButtonsPanel = new System.Windows.Forms.Panel();
      this.btnNo = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.panel1 = new System.Windows.Forms.Panel();
      this.TopPanel = new System.Windows.Forms.Panel();
      this.BottomPanel = new System.Windows.Forms.Panel();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.ButtonsPanel.SuspendLayout();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // ButtonsPanel
      // 
      this.ButtonsPanel.Controls.Add(this.btnNo);
      this.ButtonsPanel.Controls.Add(this.btnCancel);
      this.ButtonsPanel.Controls.Add(this.btnOk);
      this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.ButtonsPanel.Location = new System.Drawing.Point(0, 287);
      this.ButtonsPanel.Name = "ButtonsPanel";
      this.ButtonsPanel.Size = new System.Drawing.Size(355, 40);
      this.ButtonsPanel.TabIndex = 1;
      // 
      // btnNo
      // 
      this.btnNo.DialogResult = System.Windows.Forms.DialogResult.No;
      this.btnNo.Location = new System.Drawing.Point(196, 8);
      this.btnNo.Name = "btnNo";
      this.btnNo.Size = new System.Drawing.Size(88, 24);
      this.btnNo.TabIndex = 2;
      this.btnNo.Text = "&Нет";
      this.btnNo.UseVisualStyleBackColor = true;
      this.btnNo.Visible = false;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(102, 8);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 24);
      this.btnCancel.TabIndex = 1;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btn_Click);
      // 
      // btnOk
      // 
      this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.btnOk.Location = new System.Drawing.Point(8, 8);
      this.btnOk.Name = "btnOk";
      this.btnOk.Size = new System.Drawing.Size(88, 24);
      this.btnOk.TabIndex = 0;
      this.btnOk.Text = "О&К";
      this.btnOk.UseVisualStyleBackColor = true;
      this.btnOk.Click += new System.EventHandler(this.btn_Click);
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.MainPanel);
      this.panel1.Controls.Add(this.BottomPanel);
      this.panel1.Controls.Add(this.TopPanel);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.panel1.Location = new System.Drawing.Point(0, 0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(355, 287);
      this.panel1.TabIndex = 0;
      // 
      // TopPanel
      // 
      //this.TopPanel.AutoSize = true;
      //this.TopPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.TopPanel.Location = new System.Drawing.Point(0, 0);
      this.TopPanel.Name = "TopPanel";
      this.TopPanel.Size = new System.Drawing.Size(355, 0);
      this.TopPanel.TabIndex = 0;
      this.TopPanel.Visible = false;
      // 
      // BottomPanel
      // 
      //this.BottomPanel.AutoSize = true;
      //this.BottomPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.BottomPanel.Location = new System.Drawing.Point(0, 287);
      this.BottomPanel.Name = "BottomPanel";
      this.BottomPanel.Size = new System.Drawing.Size(355, 0);
      this.BottomPanel.TabIndex = 2;
      this.BottomPanel.Visible = false;
      // 
      // MainPanel
      // 
      this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(355, 287);
      this.MainPanel.TabIndex = 1;
      // 
      // OKCancelForm
      // 
      this.AcceptButton = this.btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(355, 327);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.ButtonsPanel);
      this.Name = "OKCancelForm";
      this.ButtonsPanel.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    /// <summary>
    /// Панель для размещения кнопок
    /// </summary>
    public System.Windows.Forms.Panel ButtonsPanel;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.Button btnNo;
    private System.Windows.Forms.Panel panel1;

    /// <summary>
    /// Основная панель для добавления элементов
    /// </summary>
    public System.Windows.Forms.Panel MainPanel;

    /// <summary>
    /// Вспомогательная панель в нижней части.
    /// По умолчанию панель скрыта.
    /// Размеры панели подбираются автоматически
    /// </summary>
    public System.Windows.Forms.Panel BottomPanel;

    /// <summary>
    /// Вспомогательная панель в верхней части.
    /// По умолчанию панель скрыта.
    /// Размеры панели подбираются автоматически
    /// </summary>
    public System.Windows.Forms.Panel TopPanel;

  }
}
