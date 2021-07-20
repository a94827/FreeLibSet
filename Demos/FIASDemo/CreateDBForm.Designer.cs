namespace FIASDemo
{
  partial class CreateDBForm
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
      this.panel1 = new System.Windows.Forms.Panel();
      this.btnCancel = new System.Windows.Forms.Button();
      this.btnOk = new System.Windows.Forms.Button();
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.tpDatabase = new System.Windows.Forms.TabPage();
      this.btnBuildConnection = new System.Windows.Forms.Button();
      this.edConnectionString = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.edName = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cbProvider = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.tpSettings = new System.Windows.Forms.TabPage();
      this.DBSettingsPanel = new AgeyevAV.ExtForms.FIAS.FiasDBSettingsPanel();
      this.panel1.SuspendLayout();
      this.TheTabControl.SuspendLayout();
      this.tpDatabase.SuspendLayout();
      this.tpSettings.SuspendLayout();
      this.SuspendLayout();
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.btnCancel);
      this.panel1.Controls.Add(this.btnOk);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 308);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(632, 40);
      this.panel1.TabIndex = 1;
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
      // 
      // TheTabControl
      // 
      this.TheTabControl.Controls.Add(this.tpDatabase);
      this.TheTabControl.Controls.Add(this.tpSettings);
      this.TheTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTabControl.Location = new System.Drawing.Point(0, 0);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(632, 308);
      this.TheTabControl.TabIndex = 0;
      // 
      // tpDatabase
      // 
      this.tpDatabase.Controls.Add(this.btnBuildConnection);
      this.tpDatabase.Controls.Add(this.edConnectionString);
      this.tpDatabase.Controls.Add(this.label3);
      this.tpDatabase.Controls.Add(this.edName);
      this.tpDatabase.Controls.Add(this.label2);
      this.tpDatabase.Controls.Add(this.cbProvider);
      this.tpDatabase.Controls.Add(this.label1);
      this.tpDatabase.Location = new System.Drawing.Point(4, 22);
      this.tpDatabase.Name = "tpDatabase";
      this.tpDatabase.Padding = new System.Windows.Forms.Padding(3);
      this.tpDatabase.Size = new System.Drawing.Size(624, 282);
      this.tpDatabase.TabIndex = 0;
      this.tpDatabase.Text = "База данных";
      this.tpDatabase.UseVisualStyleBackColor = true;
      // 
      // btnBuildConnection
      // 
      this.btnBuildConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnBuildConnection.Location = new System.Drawing.Point(528, 86);
      this.btnBuildConnection.Name = "btnBuildConnection";
      this.btnBuildConnection.Size = new System.Drawing.Size(88, 24);
      this.btnBuildConnection.TabIndex = 6;
      this.btnBuildConnection.Text = "Создать";
      this.btnBuildConnection.UseVisualStyleBackColor = true;
      // 
      // edConnectionString
      // 
      this.edConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edConnectionString.Location = new System.Drawing.Point(159, 86);
      this.edConnectionString.Name = "edConnectionString";
      this.edConnectionString.Size = new System.Drawing.Size(363, 20);
      this.edConnectionString.TabIndex = 5;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(8, 85);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(145, 21);
      this.label3.TabIndex = 4;
      this.label3.Text = "Строка подключения";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edName
      // 
      this.edName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.edName.Location = new System.Drawing.Point(159, 20);
      this.edName.Name = "edName";
      this.edName.Size = new System.Drawing.Size(457, 20);
      this.edName.TabIndex = 1;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(8, 20);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(145, 21);
      this.label2.TabIndex = 0;
      this.label2.Text = "Имя базы данных";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbProvider
      // 
      this.cbProvider.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbProvider.FormattingEnabled = true;
      this.cbProvider.Location = new System.Drawing.Point(159, 51);
      this.cbProvider.Name = "cbProvider";
      this.cbProvider.Size = new System.Drawing.Size(457, 21);
      this.cbProvider.TabIndex = 3;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(8, 51);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(145, 21);
      this.label1.TabIndex = 2;
      this.label1.Text = "Провайдер базы данных";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // tpSettings
      // 
      this.tpSettings.Controls.Add(this.DBSettingsPanel);
      this.tpSettings.Location = new System.Drawing.Point(4, 22);
      this.tpSettings.Name = "tpSettings";
      this.tpSettings.Padding = new System.Windows.Forms.Padding(3);
      this.tpSettings.Size = new System.Drawing.Size(624, 282);
      this.tpSettings.TabIndex = 1;
      this.tpSettings.Text = "Настройки ФИАС";
      this.tpSettings.UseVisualStyleBackColor = true;
      // 
      // DBSettingsPanel
      // 
      this.DBSettingsPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.DBSettingsPanel.Location = new System.Drawing.Point(3, 3);
      this.DBSettingsPanel.Name = "DBSettingsPanel";
      this.DBSettingsPanel.Size = new System.Drawing.Size(618, 247);
      this.DBSettingsPanel.TabIndex = 0;
      // 
      // CreateDBForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(632, 348);
      this.Controls.Add(this.TheTabControl);
      this.Controls.Add(this.panel1);
      this.Name = "CreateDBForm";
      this.Text = "Создание базы данных ФИАС";
      this.panel1.ResumeLayout(false);
      this.TheTabControl.ResumeLayout(false);
      this.tpDatabase.ResumeLayout(false);
      this.tpDatabase.PerformLayout();
      this.tpSettings.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.Button btnOk;
    private System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.TabPage tpDatabase;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox cbProvider;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TabPage tpSettings;
    private System.Windows.Forms.TextBox edConnectionString;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox edName;
    private System.Windows.Forms.Button btnBuildConnection;
    private AgeyevAV.ExtForms.FIAS.FiasDBSettingsPanel DBSettingsPanel;
  }
}