namespace FIASDemo
{
  partial class MainForm
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
      this.btnLoadFiles = new System.Windows.Forms.Button();
      this.btnStat = new System.Windows.Forms.Button();
      this.btnClearCache = new System.Windows.Forms.Button();
      this.btnGetAllDownloadFileInfo = new System.Windows.Forms.Button();
      this.TheTabControl = new System.Windows.Forms.TabControl();
      this.tpAddress = new System.Windows.Forms.TabPage();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.grMessages = new System.Windows.Forms.DataGridView();
      this.panel1 = new System.Windows.Forms.Panel();
      this.edAddress = new System.Windows.Forms.TextBox();
      this.panel3 = new System.Windows.Forms.Panel();
      this.btnFormat = new System.Windows.Forms.Button();
      this.groupBox5 = new System.Windows.Forms.GroupBox();
      this.cbVFormat = new System.Windows.Forms.ComboBox();
      this.label3 = new System.Windows.Forms.Label();
      this.cbVText = new System.Windows.Forms.ComboBox();
      this.btnFromVText = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbGuid = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.btnFromGuid = new System.Windows.Forms.Button();
      this.panel5 = new System.Windows.Forms.Panel();
      this.btnAbout = new System.Windows.Forms.Button();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.cbPartialMode = new System.Windows.Forms.ComboBox();
      this.label9 = new System.Windows.Forms.Label();
      this.cbEmpyMode = new System.Windows.Forms.ComboBox();
      this.label7 = new System.Windows.Forms.Label();
      this.cbManualPostalCode = new System.Windows.Forms.CheckBox();
      this.cbReadOnly = new System.Windows.Forms.CheckBox();
      this.label2 = new System.Windows.Forms.Label();
      this.cbLevel = new System.Windows.Forms.ComboBox();
      this.btnEdit = new System.Windows.Forms.Button();
      this.tpClassif = new System.Windows.Forms.TabPage();
      this.groupBox9 = new System.Windows.Forms.GroupBox();
      this.btnConvertGuid = new System.Windows.Forms.Button();
      this.groupBox8 = new System.Windows.Forms.GroupBox();
      this.btnToDisk = new System.Windows.Forms.Button();
      this.btnLoadWeb = new System.Windows.Forms.Button();
      this.btnUpdateHistory = new System.Windows.Forms.Button();
      this.groupBox7 = new System.Windows.Forms.GroupBox();
      this.lblRoomCount = new System.Windows.Forms.Label();
      this.lblRoomCount2 = new System.Windows.Forms.Label();
      this.lblHouseCount = new System.Windows.Forms.Label();
      this.lblHouseCount2 = new System.Windows.Forms.Label();
      this.lblAddrObCount = new System.Windows.Forms.Label();
      this.lblAddrObjCount2 = new System.Windows.Forms.Label();
      this.label5 = new System.Windows.Forms.Label();
      this.edActualDate = new AgeyevAV.ExtForms.DateBox();
      this.btnTest = new System.Windows.Forms.Button();
      this.tpSearch = new System.Windows.Forms.TabPage();
      this.groupBox6 = new System.Windows.Forms.GroupBox();
      this.edSearchText = new System.Windows.Forms.TextBox();
      this.groupBox4 = new System.Windows.Forms.GroupBox();
      this.cbSearchLevel = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this.btnBaseAddress = new System.Windows.Forms.Button();
      this.edParseSettingsText = new System.Windows.Forms.TextBox();
      this.panel2 = new System.Windows.Forms.Panel();
      this.infoLabel1 = new AgeyevAV.ExtForms.InfoLabel();
      this.panel4 = new System.Windows.Forms.Panel();
      this.btnSearch = new System.Windows.Forms.Button();
      this.TheTabControl.SuspendLayout();
      this.tpAddress.SuspendLayout();
      this.groupBox2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grMessages)).BeginInit();
      this.panel1.SuspendLayout();
      this.panel3.SuspendLayout();
      this.groupBox5.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.panel5.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.tpClassif.SuspendLayout();
      this.groupBox9.SuspendLayout();
      this.groupBox8.SuspendLayout();
      this.groupBox7.SuspendLayout();
      this.tpSearch.SuspendLayout();
      this.groupBox6.SuspendLayout();
      this.groupBox4.SuspendLayout();
      this.panel2.SuspendLayout();
      this.panel4.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnLoadFiles
      // 
      this.btnLoadFiles.Location = new System.Drawing.Point(189, 18);
      this.btnLoadFiles.Margin = new System.Windows.Forms.Padding(2);
      this.btnLoadFiles.Name = "btnLoadFiles";
      this.btnLoadFiles.Size = new System.Drawing.Size(176, 24);
      this.btnLoadFiles.TabIndex = 1;
      this.btnLoadFiles.Text = "Загрузить с диска";
      this.btnLoadFiles.UseVisualStyleBackColor = true;
      // 
      // btnStat
      // 
      this.btnStat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnStat.Location = new System.Drawing.Point(314, 318);
      this.btnStat.Margin = new System.Windows.Forms.Padding(2);
      this.btnStat.Name = "btnStat";
      this.btnStat.Size = new System.Drawing.Size(156, 37);
      this.btnStat.TabIndex = 4;
      this.btnStat.Text = "Статистика";
      this.btnStat.UseVisualStyleBackColor = true;
      this.btnStat.Click += new System.EventHandler(this.btnStat_Click);
      // 
      // btnClearCache
      // 
      this.btnClearCache.Location = new System.Drawing.Point(9, 18);
      this.btnClearCache.Margin = new System.Windows.Forms.Padding(2);
      this.btnClearCache.Name = "btnClearCache";
      this.btnClearCache.Size = new System.Drawing.Size(176, 24);
      this.btnClearCache.TabIndex = 0;
      this.btnClearCache.Text = "Очистить кэш";
      this.btnClearCache.UseVisualStyleBackColor = true;
      // 
      // btnGetAllDownloadFileInfo
      // 
      this.btnGetAllDownloadFileInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnGetAllDownloadFileInfo.Location = new System.Drawing.Point(484, 319);
      this.btnGetAllDownloadFileInfo.Margin = new System.Windows.Forms.Padding(2);
      this.btnGetAllDownloadFileInfo.Name = "btnGetAllDownloadFileInfo";
      this.btnGetAllDownloadFileInfo.Size = new System.Drawing.Size(132, 36);
      this.btnGetAllDownloadFileInfo.TabIndex = 5;
      this.btnGetAllDownloadFileInfo.Text = "Список обновлений";
      this.btnGetAllDownloadFileInfo.UseVisualStyleBackColor = true;
      this.btnGetAllDownloadFileInfo.Click += new System.EventHandler(this.btnGetAllDownloadFileInfo_Click);
      // 
      // TheTabControl
      // 
      this.TheTabControl.Controls.Add(this.tpAddress);
      this.TheTabControl.Controls.Add(this.tpClassif);
      this.TheTabControl.Controls.Add(this.tpSearch);
      this.TheTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.TheTabControl.Location = new System.Drawing.Point(0, 0);
      this.TheTabControl.Margin = new System.Windows.Forms.Padding(2);
      this.TheTabControl.Name = "TheTabControl";
      this.TheTabControl.SelectedIndex = 0;
      this.TheTabControl.Size = new System.Drawing.Size(632, 452);
      this.TheTabControl.TabIndex = 0;
      // 
      // tpAddress
      // 
      this.tpAddress.Controls.Add(this.groupBox2);
      this.tpAddress.Controls.Add(this.groupBox5);
      this.tpAddress.Controls.Add(this.groupBox1);
      this.tpAddress.Controls.Add(this.panel5);
      this.tpAddress.Controls.Add(this.groupBox3);
      this.tpAddress.Location = new System.Drawing.Point(4, 22);
      this.tpAddress.Margin = new System.Windows.Forms.Padding(2);
      this.tpAddress.Name = "tpAddress";
      this.tpAddress.Padding = new System.Windows.Forms.Padding(2);
      this.tpAddress.Size = new System.Drawing.Size(624, 426);
      this.tpAddress.TabIndex = 0;
      this.tpAddress.Text = "Адрес";
      this.tpAddress.UseVisualStyleBackColor = true;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.grMessages);
      this.groupBox2.Controls.Add(this.panel1);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Location = new System.Drawing.Point(2, 93);
      this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
      this.groupBox2.Size = new System.Drawing.Size(620, 143);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Текущий адрес";
      // 
      // grMessages
      // 
      this.grMessages.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grMessages.Dock = System.Windows.Forms.DockStyle.Fill;
      this.grMessages.Location = new System.Drawing.Point(2, 62);
      this.grMessages.Name = "grMessages";
      this.grMessages.Size = new System.Drawing.Size(616, 79);
      this.grMessages.TabIndex = 1;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.edAddress);
      this.panel1.Controls.Add(this.panel3);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel1.Location = new System.Drawing.Point(2, 15);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(616, 47);
      this.panel1.TabIndex = 0;
      // 
      // edAddress
      // 
      this.edAddress.Dock = System.Windows.Forms.DockStyle.Fill;
      this.edAddress.Location = new System.Drawing.Point(0, 0);
      this.edAddress.Multiline = true;
      this.edAddress.Name = "edAddress";
      this.edAddress.ReadOnly = true;
      this.edAddress.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.edAddress.Size = new System.Drawing.Size(568, 47);
      this.edAddress.TabIndex = 0;
      // 
      // panel3
      // 
      this.panel3.Controls.Add(this.btnFormat);
      this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
      this.panel3.Location = new System.Drawing.Point(568, 0);
      this.panel3.Name = "panel3";
      this.panel3.Size = new System.Drawing.Size(48, 47);
      this.panel3.TabIndex = 0;
      // 
      // btnFormat
      // 
      this.btnFormat.Location = new System.Drawing.Point(8, 11);
      this.btnFormat.Name = "btnFormat";
      this.btnFormat.Size = new System.Drawing.Size(32, 24);
      this.btnFormat.TabIndex = 1;
      this.btnFormat.UseVisualStyleBackColor = true;
      // 
      // groupBox5
      // 
      this.groupBox5.Controls.Add(this.cbVFormat);
      this.groupBox5.Controls.Add(this.label3);
      this.groupBox5.Controls.Add(this.cbVText);
      this.groupBox5.Controls.Add(this.btnFromVText);
      this.groupBox5.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.groupBox5.Location = new System.Drawing.Point(2, 236);
      this.groupBox5.Margin = new System.Windows.Forms.Padding(2);
      this.groupBox5.Name = "groupBox5";
      this.groupBox5.Padding = new System.Windows.Forms.Padding(2);
      this.groupBox5.Size = new System.Drawing.Size(620, 81);
      this.groupBox5.TabIndex = 2;
      this.groupBox5.TabStop = false;
      this.groupBox5.Text = "Сохранение адреса как строки";
      // 
      // cbVFormat
      // 
      this.cbVFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbVFormat.FormattingEnabled = true;
      this.cbVFormat.Location = new System.Drawing.Point(109, 25);
      this.cbVFormat.Name = "cbVFormat";
      this.cbVFormat.Size = new System.Drawing.Size(466, 21);
      this.cbVFormat.TabIndex = 1;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(10, 25);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(93, 21);
      this.label3.TabIndex = 0;
      this.label3.Text = "Формат";
      // 
      // cbVText
      // 
      this.cbVText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbVText.FormattingEnabled = true;
      this.cbVText.Location = new System.Drawing.Point(13, 51);
      this.cbVText.Margin = new System.Windows.Forms.Padding(2);
      this.cbVText.Name = "cbVText";
      this.cbVText.Size = new System.Drawing.Size(563, 21);
      this.cbVText.TabIndex = 2;
      // 
      // btnFromVText
      // 
      this.btnFromVText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnFromVText.Location = new System.Drawing.Point(582, 48);
      this.btnFromVText.Name = "btnFromVText";
      this.btnFromVText.Size = new System.Drawing.Size(32, 24);
      this.btnFromVText.TabIndex = 3;
      this.btnFromVText.UseVisualStyleBackColor = true;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.cbGuid);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Controls.Add(this.btnFromGuid);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.groupBox1.Location = new System.Drawing.Point(2, 317);
      this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
      this.groupBox1.Size = new System.Drawing.Size(620, 67);
      this.groupBox1.TabIndex = 3;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Поиск по GUIDу";
      // 
      // cbGuid
      // 
      this.cbGuid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbGuid.FormattingEnabled = true;
      this.cbGuid.Location = new System.Drawing.Point(12, 31);
      this.cbGuid.Margin = new System.Windows.Forms.Padding(2);
      this.cbGuid.Name = "cbGuid";
      this.cbGuid.Size = new System.Drawing.Size(563, 21);
      this.cbGuid.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(10, 15);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(232, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "GUID адресного объекта, дома, помещения";
      // 
      // btnFromGuid
      // 
      this.btnFromGuid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnFromGuid.Location = new System.Drawing.Point(582, 31);
      this.btnFromGuid.Name = "btnFromGuid";
      this.btnFromGuid.Size = new System.Drawing.Size(32, 24);
      this.btnFromGuid.TabIndex = 2;
      this.btnFromGuid.UseVisualStyleBackColor = true;
      // 
      // panel5
      // 
      this.panel5.Controls.Add(this.btnAbout);
      this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel5.Location = new System.Drawing.Point(2, 384);
      this.panel5.Name = "panel5";
      this.panel5.Size = new System.Drawing.Size(620, 40);
      this.panel5.TabIndex = 4;
      // 
      // btnAbout
      // 
      this.btnAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnAbout.Location = new System.Drawing.Point(482, 11);
      this.btnAbout.Margin = new System.Windows.Forms.Padding(2);
      this.btnAbout.Name = "btnAbout";
      this.btnAbout.Size = new System.Drawing.Size(132, 24);
      this.btnAbout.TabIndex = 4;
      this.btnAbout.Text = "О программе";
      this.btnAbout.UseVisualStyleBackColor = true;
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.cbPartialMode);
      this.groupBox3.Controls.Add(this.label9);
      this.groupBox3.Controls.Add(this.cbEmpyMode);
      this.groupBox3.Controls.Add(this.label7);
      this.groupBox3.Controls.Add(this.cbManualPostalCode);
      this.groupBox3.Controls.Add(this.cbReadOnly);
      this.groupBox3.Controls.Add(this.label2);
      this.groupBox3.Controls.Add(this.cbLevel);
      this.groupBox3.Controls.Add(this.btnEdit);
      this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox3.Location = new System.Drawing.Point(2, 2);
      this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
      this.groupBox3.Size = new System.Drawing.Size(620, 91);
      this.groupBox3.TabIndex = 0;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Редактор адреса";
      // 
      // cbPartialMode
      // 
      this.cbPartialMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbPartialMode.FormattingEnabled = true;
      this.cbPartialMode.Items.AddRange(new object[] {
            "Разрешен",
            "Ошибка",
            "Предупреждение"});
      this.cbPartialMode.Location = new System.Drawing.Point(322, 67);
      this.cbPartialMode.Name = "cbPartialMode";
      this.cbPartialMode.Size = new System.Drawing.Size(148, 21);
      this.cbPartialMode.TabIndex = 7;
      // 
      // label9
      // 
      this.label9.Location = new System.Drawing.Point(216, 67);
      this.label9.Name = "label9";
      this.label9.Size = new System.Drawing.Size(100, 17);
      this.label9.TabIndex = 6;
      this.label9.Text = "Неполный адрес";
      this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbEmpyMode
      // 
      this.cbEmpyMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbEmpyMode.FormattingEnabled = true;
      this.cbEmpyMode.Items.AddRange(new object[] {
            "Разрешен",
            "Ошибка",
            "Предупреждение"});
      this.cbEmpyMode.Location = new System.Drawing.Point(322, 42);
      this.cbEmpyMode.Name = "cbEmpyMode";
      this.cbEmpyMode.Size = new System.Drawing.Size(148, 21);
      this.cbEmpyMode.TabIndex = 5;
      // 
      // label7
      // 
      this.label7.Location = new System.Drawing.Point(216, 44);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(100, 19);
      this.label7.TabIndex = 4;
      this.label7.Text = "Пустой адрес";
      this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbManualPostalCode
      // 
      this.cbManualPostalCode.AutoSize = true;
      this.cbManualPostalCode.Location = new System.Drawing.Point(85, 44);
      this.cbManualPostalCode.Name = "cbManualPostalCode";
      this.cbManualPostalCode.Size = new System.Drawing.Size(115, 17);
      this.cbManualPostalCode.TabIndex = 2;
      this.cbManualPostalCode.Text = "Почтовый индекс";
      this.cbManualPostalCode.UseVisualStyleBackColor = true;
      // 
      // cbReadOnly
      // 
      this.cbReadOnly.AutoSize = true;
      this.cbReadOnly.Location = new System.Drawing.Point(85, 67);
      this.cbReadOnly.Name = "cbReadOnly";
      this.cbReadOnly.Size = new System.Drawing.Size(119, 17);
      this.cbReadOnly.TabIndex = 3;
      this.cbReadOnly.Text = "Режим просмотра";
      this.cbReadOnly.UseVisualStyleBackColor = true;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(8, 21);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(51, 13);
      this.label2.TabIndex = 0;
      this.label2.Text = "Уровень";
      // 
      // cbLevel
      // 
      this.cbLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbLevel.FormattingEnabled = true;
      this.cbLevel.Location = new System.Drawing.Point(85, 17);
      this.cbLevel.Name = "cbLevel";
      this.cbLevel.Size = new System.Drawing.Size(385, 21);
      this.cbLevel.TabIndex = 1;
      // 
      // btnEdit
      // 
      this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnEdit.Location = new System.Drawing.Point(482, 17);
      this.btnEdit.Margin = new System.Windows.Forms.Padding(2);
      this.btnEdit.Name = "btnEdit";
      this.btnEdit.Size = new System.Drawing.Size(132, 24);
      this.btnEdit.TabIndex = 8;
      this.btnEdit.Text = "Редактор";
      this.btnEdit.UseVisualStyleBackColor = true;
      // 
      // tpClassif
      // 
      this.tpClassif.Controls.Add(this.groupBox9);
      this.tpClassif.Controls.Add(this.groupBox8);
      this.tpClassif.Controls.Add(this.groupBox7);
      this.tpClassif.Controls.Add(this.btnTest);
      this.tpClassif.Controls.Add(this.btnGetAllDownloadFileInfo);
      this.tpClassif.Controls.Add(this.btnStat);
      this.tpClassif.Location = new System.Drawing.Point(4, 22);
      this.tpClassif.Margin = new System.Windows.Forms.Padding(2);
      this.tpClassif.Name = "tpClassif";
      this.tpClassif.Padding = new System.Windows.Forms.Padding(2);
      this.tpClassif.Size = new System.Drawing.Size(624, 426);
      this.tpClassif.TabIndex = 1;
      this.tpClassif.Text = "БД ФИАС";
      this.tpClassif.UseVisualStyleBackColor = true;
      // 
      // groupBox9
      // 
      this.groupBox9.Controls.Add(this.btnConvertGuid);
      this.groupBox9.Controls.Add(this.btnClearCache);
      this.groupBox9.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox9.Location = new System.Drawing.Point(2, 163);
      this.groupBox9.Name = "groupBox9";
      this.groupBox9.Size = new System.Drawing.Size(620, 52);
      this.groupBox9.TabIndex = 2;
      this.groupBox9.TabStop = false;
      this.groupBox9.Text = "Дополнительно";
      // 
      // btnConvertGuid
      // 
      this.btnConvertGuid.Location = new System.Drawing.Point(369, 18);
      this.btnConvertGuid.Margin = new System.Windows.Forms.Padding(2);
      this.btnConvertGuid.Name = "btnConvertGuid";
      this.btnConvertGuid.Size = new System.Drawing.Size(176, 24);
      this.btnConvertGuid.TabIndex = 3;
      this.btnConvertGuid.Text = "Преобразование GUID";
      this.btnConvertGuid.UseVisualStyleBackColor = true;
      // 
      // groupBox8
      // 
      this.groupBox8.Controls.Add(this.btnToDisk);
      this.groupBox8.Controls.Add(this.btnLoadWeb);
      this.groupBox8.Controls.Add(this.btnLoadFiles);
      this.groupBox8.Controls.Add(this.btnUpdateHistory);
      this.groupBox8.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox8.Location = new System.Drawing.Point(2, 87);
      this.groupBox8.Name = "groupBox8";
      this.groupBox8.Size = new System.Drawing.Size(620, 76);
      this.groupBox8.TabIndex = 0;
      this.groupBox8.TabStop = false;
      this.groupBox8.Text = "Обновление";
      // 
      // btnToDisk
      // 
      this.btnToDisk.Location = new System.Drawing.Point(9, 46);
      this.btnToDisk.Margin = new System.Windows.Forms.Padding(2);
      this.btnToDisk.Name = "btnToDisk";
      this.btnToDisk.Size = new System.Drawing.Size(176, 24);
      this.btnToDisk.TabIndex = 3;
      this.btnToDisk.Text = "Скачать с сайта на диск";
      this.btnToDisk.UseVisualStyleBackColor = true;
      // 
      // btnLoadWeb
      // 
      this.btnLoadWeb.Location = new System.Drawing.Point(9, 18);
      this.btnLoadWeb.Margin = new System.Windows.Forms.Padding(2);
      this.btnLoadWeb.Name = "btnLoadWeb";
      this.btnLoadWeb.Size = new System.Drawing.Size(176, 24);
      this.btnLoadWeb.TabIndex = 0;
      this.btnLoadWeb.Text = "Загрузить с сайта";
      this.btnLoadWeb.UseVisualStyleBackColor = true;
      // 
      // btnUpdateHistory
      // 
      this.btnUpdateHistory.Location = new System.Drawing.Point(369, 18);
      this.btnUpdateHistory.Margin = new System.Windows.Forms.Padding(2);
      this.btnUpdateHistory.Name = "btnUpdateHistory";
      this.btnUpdateHistory.Size = new System.Drawing.Size(176, 24);
      this.btnUpdateHistory.TabIndex = 2;
      this.btnUpdateHistory.Text = "История обновлений";
      this.btnUpdateHistory.UseVisualStyleBackColor = true;
      // 
      // groupBox7
      // 
      this.groupBox7.Controls.Add(this.lblRoomCount);
      this.groupBox7.Controls.Add(this.lblRoomCount2);
      this.groupBox7.Controls.Add(this.lblHouseCount);
      this.groupBox7.Controls.Add(this.lblHouseCount2);
      this.groupBox7.Controls.Add(this.lblAddrObCount);
      this.groupBox7.Controls.Add(this.lblAddrObjCount2);
      this.groupBox7.Controls.Add(this.label5);
      this.groupBox7.Controls.Add(this.edActualDate);
      this.groupBox7.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox7.Location = new System.Drawing.Point(2, 2);
      this.groupBox7.Name = "groupBox7";
      this.groupBox7.Size = new System.Drawing.Size(620, 85);
      this.groupBox7.TabIndex = 0;
      this.groupBox7.TabStop = false;
      this.groupBox7.Text = "Текущая база данных";
      // 
      // lblRoomCount
      // 
      this.lblRoomCount.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblRoomCount.Location = new System.Drawing.Point(508, 52);
      this.lblRoomCount.Name = "lblRoomCount";
      this.lblRoomCount.Size = new System.Drawing.Size(100, 20);
      this.lblRoomCount.TabIndex = 7;
      this.lblRoomCount.Text = "???";
      this.lblRoomCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // lblRoomCount2
      // 
      this.lblRoomCount2.Location = new System.Drawing.Point(508, 14);
      this.lblRoomCount2.Name = "lblRoomCount2";
      this.lblRoomCount2.Size = new System.Drawing.Size(100, 34);
      this.lblRoomCount2.TabIndex = 6;
      this.lblRoomCount2.Text = "Помещения";
      this.lblRoomCount2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
      // 
      // lblHouseCount
      // 
      this.lblHouseCount.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblHouseCount.Location = new System.Drawing.Point(402, 53);
      this.lblHouseCount.Name = "lblHouseCount";
      this.lblHouseCount.Size = new System.Drawing.Size(100, 20);
      this.lblHouseCount.TabIndex = 5;
      this.lblHouseCount.Text = "???";
      this.lblHouseCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // lblHouseCount2
      // 
      this.lblHouseCount2.Location = new System.Drawing.Point(399, 14);
      this.lblHouseCount2.Name = "lblHouseCount2";
      this.lblHouseCount2.Size = new System.Drawing.Size(100, 34);
      this.lblHouseCount2.TabIndex = 4;
      this.lblHouseCount2.Text = "Здания";
      this.lblHouseCount2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
      // 
      // lblAddrObCount
      // 
      this.lblAddrObCount.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblAddrObCount.Location = new System.Drawing.Point(296, 53);
      this.lblAddrObCount.Name = "lblAddrObCount";
      this.lblAddrObCount.Size = new System.Drawing.Size(100, 20);
      this.lblAddrObCount.TabIndex = 3;
      this.lblAddrObCount.Text = "???";
      this.lblAddrObCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // lblAddrObjCount2
      // 
      this.lblAddrObjCount2.Location = new System.Drawing.Point(296, 14);
      this.lblAddrObjCount2.Name = "lblAddrObjCount2";
      this.lblAddrObjCount2.Size = new System.Drawing.Size(100, 34);
      this.lblAddrObjCount2.TabIndex = 2;
      this.lblAddrObjCount2.Text = "Адресные объекты";
      this.lblAddrObjCount2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(6, 49);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(125, 24);
      this.label5.TabIndex = 0;
      this.label5.Text = "Дата актуальности";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edActualDate
      // 
      this.edActualDate.Location = new System.Drawing.Point(134, 52);
      this.edActualDate.Name = "edActualDate";
      this.edActualDate.ReadOnly = true;
      this.edActualDate.Size = new System.Drawing.Size(156, 20);
      this.edActualDate.TabIndex = 1;
      // 
      // btnTest
      // 
      this.btnTest.Location = new System.Drawing.Point(136, 318);
      this.btnTest.Margin = new System.Windows.Forms.Padding(2);
      this.btnTest.Name = "btnTest";
      this.btnTest.Size = new System.Drawing.Size(156, 32);
      this.btnTest.TabIndex = 3;
      this.btnTest.Text = "Тест";
      this.btnTest.UseVisualStyleBackColor = true;
      this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
      // 
      // tpSearch
      // 
      this.tpSearch.Controls.Add(this.groupBox6);
      this.tpSearch.Controls.Add(this.groupBox4);
      this.tpSearch.Controls.Add(this.panel2);
      this.tpSearch.Location = new System.Drawing.Point(4, 22);
      this.tpSearch.Name = "tpSearch";
      this.tpSearch.Padding = new System.Windows.Forms.Padding(3);
      this.tpSearch.Size = new System.Drawing.Size(624, 426);
      this.tpSearch.TabIndex = 2;
      this.tpSearch.Text = "Поиск";
      this.tpSearch.UseVisualStyleBackColor = true;
      // 
      // groupBox6
      // 
      this.groupBox6.Controls.Add(this.edSearchText);
      this.groupBox6.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox6.Location = new System.Drawing.Point(3, 89);
      this.groupBox6.Name = "groupBox6";
      this.groupBox6.Size = new System.Drawing.Size(618, 294);
      this.groupBox6.TabIndex = 3;
      this.groupBox6.TabStop = false;
      this.groupBox6.Text = "Адреса для поиска";
      // 
      // edSearchText
      // 
      this.edSearchText.Dock = System.Windows.Forms.DockStyle.Fill;
      this.edSearchText.Location = new System.Drawing.Point(3, 16);
      this.edSearchText.Multiline = true;
      this.edSearchText.Name = "edSearchText";
      this.edSearchText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.edSearchText.Size = new System.Drawing.Size(612, 275);
      this.edSearchText.TabIndex = 0;
      this.edSearchText.WordWrap = false;
      // 
      // groupBox4
      // 
      this.groupBox4.Controls.Add(this.cbSearchLevel);
      this.groupBox4.Controls.Add(this.label4);
      this.groupBox4.Controls.Add(this.btnBaseAddress);
      this.groupBox4.Controls.Add(this.edParseSettingsText);
      this.groupBox4.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox4.Location = new System.Drawing.Point(3, 3);
      this.groupBox4.Name = "groupBox4";
      this.groupBox4.Size = new System.Drawing.Size(618, 86);
      this.groupBox4.TabIndex = 0;
      this.groupBox4.TabStop = false;
      this.groupBox4.Text = "Параметры поиска";
      // 
      // cbSearchLevel
      // 
      this.cbSearchLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbSearchLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbSearchLevel.FormattingEnabled = true;
      this.cbSearchLevel.Location = new System.Drawing.Point(153, 58);
      this.cbSearchLevel.Name = "cbSearchLevel";
      this.cbSearchLevel.Size = new System.Drawing.Size(454, 21);
      this.cbSearchLevel.TabIndex = 3;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(8, 58);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(133, 21);
      this.label4.TabIndex = 2;
      this.label4.Text = "Нижний уровень";
      // 
      // btnBaseAddress
      // 
      this.btnBaseAddress.Location = new System.Drawing.Point(10, 26);
      this.btnBaseAddress.Name = "btnBaseAddress";
      this.btnBaseAddress.Size = new System.Drawing.Size(132, 24);
      this.btnBaseAddress.TabIndex = 0;
      this.btnBaseAddress.Text = "Базовый адрес";
      this.btnBaseAddress.UseVisualStyleBackColor = true;
      // 
      // edParseSettingsText
      // 
      this.edParseSettingsText.Location = new System.Drawing.Point(153, 19);
      this.edParseSettingsText.Multiline = true;
      this.edParseSettingsText.Name = "edParseSettingsText";
      this.edParseSettingsText.ReadOnly = true;
      this.edParseSettingsText.Size = new System.Drawing.Size(454, 33);
      this.edParseSettingsText.TabIndex = 1;
      // 
      // panel2
      // 
      this.panel2.Controls.Add(this.infoLabel1);
      this.panel2.Controls.Add(this.panel4);
      this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel2.Location = new System.Drawing.Point(3, 383);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(618, 40);
      this.panel2.TabIndex = 1;
      // 
      // infoLabel1
      // 
      this.infoLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.infoLabel1.Location = new System.Drawing.Point(104, 0);
      this.infoLabel1.Name = "infoLabel1";
      this.infoLabel1.Size = new System.Drawing.Size(514, 40);
      this.infoLabel1.TabIndex = 1;
      this.infoLabel1.Text = "Вставьте в текстовое поле один или несколько адресов, которые нужно найти. Раздел" +
          "ители компонентов - запятые или табуляция. Одна строка - один адрес";
      // 
      // panel4
      // 
      this.panel4.Controls.Add(this.btnSearch);
      this.panel4.Dock = System.Windows.Forms.DockStyle.Left;
      this.panel4.Location = new System.Drawing.Point(0, 0);
      this.panel4.Name = "panel4";
      this.panel4.Size = new System.Drawing.Size(104, 40);
      this.panel4.TabIndex = 0;
      // 
      // btnSearch
      // 
      this.btnSearch.Location = new System.Drawing.Point(8, 8);
      this.btnSearch.Name = "btnSearch";
      this.btnSearch.Size = new System.Drawing.Size(88, 24);
      this.btnSearch.TabIndex = 0;
      this.btnSearch.Text = "Найти";
      this.btnSearch.UseVisualStyleBackColor = true;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(632, 452);
      this.Controls.Add(this.TheTabControl);
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "MainForm";
      this.Text = "Тестирование справочника ФИАС";
      this.TheTabControl.ResumeLayout(false);
      this.tpAddress.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grMessages)).EndInit();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.panel3.ResumeLayout(false);
      this.groupBox5.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.panel5.ResumeLayout(false);
      this.groupBox3.ResumeLayout(false);
      this.groupBox3.PerformLayout();
      this.tpClassif.ResumeLayout(false);
      this.groupBox9.ResumeLayout(false);
      this.groupBox8.ResumeLayout(false);
      this.groupBox7.ResumeLayout(false);
      this.tpSearch.ResumeLayout(false);
      this.groupBox6.ResumeLayout(false);
      this.groupBox6.PerformLayout();
      this.groupBox4.ResumeLayout(false);
      this.groupBox4.PerformLayout();
      this.panel2.ResumeLayout(false);
      this.panel4.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnLoadFiles;
    private System.Windows.Forms.Button btnStat;
    private System.Windows.Forms.Button btnClearCache;
    private System.Windows.Forms.Button btnGetAllDownloadFileInfo;
    private System.Windows.Forms.TabControl TheTabControl;
    private System.Windows.Forms.TabPage tpAddress;
    private System.Windows.Forms.Button btnEdit;
    private System.Windows.Forms.TabPage tpClassif;
    private System.Windows.Forms.GroupBox groupBox3;
    private System.Windows.Forms.CheckBox cbReadOnly;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.ComboBox cbLevel;
    private System.Windows.Forms.Button btnAbout;
    private System.Windows.Forms.Button btnTest;
    private System.Windows.Forms.TabPage tpSearch;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.TextBox edParseSettingsText;
    private System.Windows.Forms.Button btnBaseAddress;
    private AgeyevAV.ExtForms.InfoLabel infoLabel1;
    private System.Windows.Forms.Panel panel4;
    private System.Windows.Forms.Button btnSearch;
    private System.Windows.Forms.GroupBox groupBox5;
    private System.Windows.Forms.ComboBox cbVText;
    private System.Windows.Forms.Button btnFromVText;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.ComboBox cbGuid;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnFromGuid;
    private System.Windows.Forms.Panel panel5;
    private System.Windows.Forms.ComboBox cbVFormat;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.GroupBox groupBox6;
    private System.Windows.Forms.TextBox edSearchText;
    private System.Windows.Forms.GroupBox groupBox4;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ComboBox cbSearchLevel;
    private AgeyevAV.ExtForms.DateBox edActualDate;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.DataGridView grMessages;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.TextBox edAddress;
    private System.Windows.Forms.Panel panel3;
    private System.Windows.Forms.Button btnFormat;
    private System.Windows.Forms.Button btnUpdateHistory;
    private System.Windows.Forms.GroupBox groupBox7;
    private System.Windows.Forms.GroupBox groupBox8;
    private System.Windows.Forms.Label lblRoomCount;
    private System.Windows.Forms.Label lblRoomCount2;
    private System.Windows.Forms.Label lblHouseCount;
    private System.Windows.Forms.Label lblHouseCount2;
    private System.Windows.Forms.Label lblAddrObCount;
    private System.Windows.Forms.Label lblAddrObjCount2;
    private System.Windows.Forms.GroupBox groupBox9;
    private System.Windows.Forms.CheckBox cbManualPostalCode;
    private System.Windows.Forms.ComboBox cbPartialMode;
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.ComboBox cbEmpyMode;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Button btnLoadWeb;
    private System.Windows.Forms.Button btnConvertGuid;
    private System.Windows.Forms.Button btnToDisk;
  }
}

