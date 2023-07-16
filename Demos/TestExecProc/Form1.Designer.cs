namespace TestExecProc
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
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.cbFromThread = new System.Windows.Forms.ComboBox();
      this.label5 = new System.Windows.Forms.Label();
      this.cbExecutionPlace = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this.cbStartMode = new System.Windows.Forms.ComboBox();
      this.btnStart = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.cbProcType = new System.Windows.Forms.ComboBox();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.btnDebug = new System.Windows.Forms.Button();
      this.btnInfo = new System.Windows.Forms.Button();
      this.btnGCCollect = new System.Windows.Forms.Button();
      this.label6 = new System.Windows.Forms.Label();
      this.edSyncTime = new FreeLibSet.Controls.IntEditBox();
      this.edCopies = new FreeLibSet.Controls.IntEditBox();
      this.groupBox1.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.edSyncTime);
      this.groupBox1.Controls.Add(this.label6);
      this.groupBox1.Controls.Add(this.cbFromThread);
      this.groupBox1.Controls.Add(this.label5);
      this.groupBox1.Controls.Add(this.cbExecutionPlace);
      this.groupBox1.Controls.Add(this.label4);
      this.groupBox1.Controls.Add(this.cbStartMode);
      this.groupBox1.Controls.Add(this.btnStart);
      this.groupBox1.Controls.Add(this.edCopies);
      this.groupBox1.Controls.Add(this.label3);
      this.groupBox1.Controls.Add(this.label2);
      this.groupBox1.Controls.Add(this.cbProcType);
      this.groupBox1.Controls.Add(this.label1);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(462, 231);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Запустить процедуру";
      // 
      // cbFromThread
      // 
      this.cbFromThread.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbFromThread.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbFromThread.FormattingEnabled = true;
      this.cbFromThread.Items.AddRange(new object[] {
            "Из основного потока приложения",
            "Из отдельных потоков"});
      this.cbFromThread.Location = new System.Drawing.Point(117, 91);
      this.cbFromThread.Name = "cbFromThread";
      this.cbFromThread.Size = new System.Drawing.Size(333, 21);
      this.cbFromThread.TabIndex = 5;
      // 
      // label5
      // 
      this.label5.Location = new System.Drawing.Point(12, 91);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(99, 21);
      this.label5.TabIndex = 4;
      this.label5.Text = "Из потока";
      this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbExecutionPlace
      // 
      this.cbExecutionPlace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbExecutionPlace.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbExecutionPlace.FormattingEnabled = true;
      this.cbExecutionPlace.Items.AddRange(new object[] {
            "Основной домен приложения (ExecProc)",
            "Отдельный домен (RemoteExecProc)"});
      this.cbExecutionPlace.Location = new System.Drawing.Point(117, 119);
      this.cbExecutionPlace.Name = "cbExecutionPlace";
      this.cbExecutionPlace.Size = new System.Drawing.Size(333, 21);
      this.cbExecutionPlace.TabIndex = 7;
      // 
      // label4
      // 
      this.label4.Location = new System.Drawing.Point(12, 119);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(99, 21);
      this.label4.TabIndex = 6;
      this.label4.Text = "Место";
      this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbStartMode
      // 
      this.cbStartMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbStartMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbStartMode.FormattingEnabled = true;
      this.cbStartMode.Items.AddRange(new object[] {
            "Синхронно, используя ExecProcCallList.ExecuteSync()",
            "Асинхронно, используя ExecProcCallList.ExecuteAsync() (периодический опрос)",
            "Используя ExecProcCallList.ExecuteAsyncAndWait()",
            "С ожиданием срабатывания WaitHandle",
            "Асинхронное получение уведомления AsyncCallback",
            "Немедленный вызов EndExecute() с автоматическим ожиданием",
            "Без ожидания (ExecuteNoResults)",
            "Distributed -ExecProcCallList.ExecuteAsync()",
            "Distributed -ExecProcCallList.ExecuteAsyncAndWait()",
            "RemoteDistributedProc с ожиданием срабатывания WaitHandle"});
      this.cbStartMode.Location = new System.Drawing.Point(117, 64);
      this.cbStartMode.Name = "cbStartMode";
      this.cbStartMode.Size = new System.Drawing.Size(333, 21);
      this.cbStartMode.TabIndex = 3;
      // 
      // btnStart
      // 
      this.btnStart.Location = new System.Drawing.Point(15, 196);
      this.btnStart.Name = "btnStart";
      this.btnStart.Size = new System.Drawing.Size(88, 24);
      this.btnStart.TabIndex = 12;
      this.btnStart.Text = "Пуск";
      this.btnStart.UseVisualStyleBackColor = true;
      // 
      // label3
      // 
      this.label3.Location = new System.Drawing.Point(12, 146);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(99, 21);
      this.label3.TabIndex = 8;
      this.label3.Text = "Экземпляров";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(12, 64);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(99, 21);
      this.label2.TabIndex = 2;
      this.label2.Text = "Тип запуска";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // cbProcType
      // 
      this.cbProcType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cbProcType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbProcType.FormattingEnabled = true;
      this.cbProcType.Items.AddRange(new object[] {
            "Ожидание 10 секунд",
            "Быстро (0.1 секунды)",
            "Пять секунд и ошибка",
            "Сразу ошибка"});
      this.cbProcType.Location = new System.Drawing.Point(117, 37);
      this.cbProcType.Name = "cbProcType";
      this.cbProcType.Size = new System.Drawing.Size(333, 21);
      this.cbProcType.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(12, 37);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(99, 21);
      this.label1.TabIndex = 0;
      this.label1.Text = "Процедура";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.btnDebug);
      this.groupBox2.Controls.Add(this.btnInfo);
      this.groupBox2.Controls.Add(this.btnGCCollect);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Location = new System.Drawing.Point(0, 231);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(462, 59);
      this.groupBox2.TabIndex = 1;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Дополнительно";
      // 
      // btnDebug
      // 
      this.btnDebug.Location = new System.Drawing.Point(255, 29);
      this.btnDebug.Name = "btnDebug";
      this.btnDebug.Size = new System.Drawing.Size(132, 24);
      this.btnDebug.TabIndex = 2;
      this.btnDebug.Text = "Отладка";
      this.btnDebug.UseVisualStyleBackColor = true;
      // 
      // btnInfo
      // 
      this.btnInfo.Location = new System.Drawing.Point(15, 28);
      this.btnInfo.Name = "btnInfo";
      this.btnInfo.Size = new System.Drawing.Size(88, 24);
      this.btnInfo.TabIndex = 0;
      this.btnInfo.Text = "Инфо";
      this.btnInfo.UseVisualStyleBackColor = true;
      // 
      // btnGCCollect
      // 
      this.btnGCCollect.Location = new System.Drawing.Point(117, 28);
      this.btnGCCollect.Name = "btnGCCollect";
      this.btnGCCollect.Size = new System.Drawing.Size(132, 24);
      this.btnGCCollect.TabIndex = 1;
      this.btnGCCollect.Text = "GC.Collect";
      this.btnGCCollect.UseVisualStyleBackColor = true;
      // 
      // label6
      // 
      this.label6.Location = new System.Drawing.Point(12, 172);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(99, 21);
      this.label6.TabIndex = 10;
      this.label6.Text = "SyncTime, мс";
      this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // edSyncTime
      // 
      this.edSyncTime.Increment = 1;
      this.edSyncTime.Location = new System.Drawing.Point(117, 172);
      this.edSyncTime.Maximum = 5000;
      this.edSyncTime.Minimum =0;
      this.edSyncTime.Name = "edSyncTime";
      this.edSyncTime.Size = new System.Drawing.Size(100, 20);
      this.edSyncTime.TabIndex = 11;
      this.edSyncTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // edCopies
      // 
      this.edCopies.Increment = 1;
      this.edCopies.Location = new System.Drawing.Point(117, 146);
      this.edCopies.Maximum = 10;
      this.edCopies.Minimum = 1;
      this.edCopies.Name = "edCopies";
      this.edCopies.Size = new System.Drawing.Size(100, 20);
      this.edCopies.TabIndex = 9;
      this.edCopies.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.edCopies.Value = 1;
      // 
      // MainForm
      // 
      this.AcceptButton = this.btnStart;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(462, 290);
      this.Controls.Add(this.groupBox2);
      this.Controls.Add(this.groupBox1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
      this.MaximizeBox = false;
      this.Name = "MainForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "Тестирование ExecProc";
      this.groupBox1.ResumeLayout(false);
      this.groupBox2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.ComboBox cbProcType;
    private System.Windows.Forms.Label label1;
    private FreeLibSet.Controls.IntEditBox edCopies;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.ComboBox cbStartMode;
    private System.Windows.Forms.ComboBox cbExecutionPlace;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.GroupBox groupBox2;
    private System.Windows.Forms.Button btnGCCollect;
    private System.Windows.Forms.Button btnInfo;
    private System.Windows.Forms.Button btnDebug;
    private System.Windows.Forms.ComboBox cbFromThread;
    private System.Windows.Forms.Label label5;
    private FreeLibSet.Controls.IntEditBox edSyncTime;
    private System.Windows.Forms.Label label6;
  }
}

