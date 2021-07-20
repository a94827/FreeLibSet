using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// Summary description for SplashForm.
  /// </summary>
  internal class SplashForm : System.Windows.Forms.Form
  {
    private System.Windows.Forms.ImageList BigImageList;
    public ListView PhasesListView;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    public Panel SecondPanel;
    private GroupBox groupBox1;
    public Label lblCurrent;
    public ProgressBar pb1;
    public Button btnCancel;
    public Label lblWaitCancel;
    public Button btnDebug;
    private TableLayoutPanel tableLayoutPanel1;
    public Panel MainPanel;
    public Button btnCopyText;
    private ImageList StdImageList;
    private System.ComponentModel.IContainer components;

    public SplashForm()
    {
      // Required for Windows TheForm Designer support
      InitializeComponent();

      if (EFPApp.MainWindowVisible && EFPApp.IsMainThread)
        Icon = EFPApp.MainImageIcon("HourGlass");
      else
        WinFormsTools.InitAppIcon(this);

      // 14.07.2015
      // Используем собственный список изображений на случай, если 
      // вызов выполняется из чужого потока
      btnCancel.Image = StdImageList.Images["Cancel"];

      btnCopyText.Image = StdImageList.Images["Copy"];
      btnCopyText.ImageAlign = ContentAlignment.MiddleCenter;
      btnCopyText.Click += new EventHandler(btnCopyText_Click);

      try
      {
        if (EFPApp.AppWasInit && (!EFPApp.IsMainThread))
        {
          // 10.12.2019
          // Если заставка вызывается не из основного потока, то при вызове ExecProcCallList.ExecuteSync() с удаленной процедурой RemoteExecProc,
          // заставка не будет отображаться правильно. У нее не будет родительского окна (EFPApp.DialogOwnerForm возвращает null из несовновного потока)

          this.ShowInTaskbar = true; 
        }
        else
          EFPApp.InitShowInTaskBar(this);
      }
      catch
      {
      }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashForm));
      System.Windows.Forms.ListViewItem listViewItem9 = new System.Windows.Forms.ListViewItem(new string[] {
            "Один. . Описание может быть очень очень очень, ну очень длинным",
            "Выполнено"}, 0);
      System.Windows.Forms.ListViewItem listViewItem10 = new System.Windows.Forms.ListViewItem(new string[] {
            "Два",
            "Идет выполнение"}, 1);
      System.Windows.Forms.ListViewItem listViewItem11 = new System.Windows.Forms.ListViewItem("Три", 0);
      System.Windows.Forms.ListViewItem listViewItem12 = new System.Windows.Forms.ListViewItem("Четыре");
      System.Windows.Forms.ListViewItem listViewItem13 = new System.Windows.Forms.ListViewItem("Пять");
      System.Windows.Forms.ListViewItem listViewItem14 = new System.Windows.Forms.ListViewItem("Шесть");
      System.Windows.Forms.ListViewItem listViewItem15 = new System.Windows.Forms.ListViewItem("Семь");
      System.Windows.Forms.ListViewItem listViewItem16 = new System.Windows.Forms.ListViewItem("Восемь");
      this.BigImageList = new System.Windows.Forms.ImageList(this.components);
      this.PhasesListView = new System.Windows.Forms.ListView();
      this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
      this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
      this.SecondPanel = new System.Windows.Forms.Panel();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.btnCopyText = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.lblWaitCancel = new System.Windows.Forms.Label();
      this.btnDebug = new System.Windows.Forms.Button();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.lblCurrent = new System.Windows.Forms.Label();
      this.pb1 = new System.Windows.Forms.ProgressBar();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.StdImageList = new System.Windows.Forms.ImageList(this.components);
      this.SecondPanel.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.MainPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // BigImageList
      // 
      this.BigImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("BigImageList.ImageStream")));
      this.BigImageList.TransparentColor = System.Drawing.Color.Magenta;
      this.BigImageList.Images.SetKeyName(0, "Iten");
      this.BigImageList.Images.SetKeyName(1, "Current");
      this.BigImageList.Images.SetKeyName(2, "Ok");
      this.BigImageList.Images.SetKeyName(3, "Cancel");
      this.BigImageList.Images.SetKeyName(4, "No");
      // 
      // PhasesListView
      // 
      this.PhasesListView.Alignment = System.Windows.Forms.ListViewAlignment.Default;
      this.PhasesListView.AutoArrange = false;
      this.PhasesListView.BackColor = System.Drawing.SystemColors.Control;
      this.PhasesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
      this.PhasesListView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.PhasesListView.ForeColor = System.Drawing.SystemColors.ControlText;
      this.PhasesListView.FullRowSelect = true;
      this.PhasesListView.GridLines = true;
      this.PhasesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.PhasesListView.HideSelection = false;
      this.PhasesListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem9,
            listViewItem10,
            listViewItem11,
            listViewItem12,
            listViewItem13,
            listViewItem14,
            listViewItem15,
            listViewItem16});
      this.PhasesListView.LargeImageList = this.BigImageList;
      this.PhasesListView.Location = new System.Drawing.Point(0, 0);
      this.PhasesListView.MultiSelect = false;
      this.PhasesListView.Name = "PhasesListView";
      this.PhasesListView.ShowItemToolTips = true;
      this.PhasesListView.Size = new System.Drawing.Size(430, 164);
      this.PhasesListView.SmallImageList = this.BigImageList;
      this.PhasesListView.TabIndex = 1;
      this.PhasesListView.TabStop = false;
      this.PhasesListView.UseCompatibleStateImageBehavior = false;
      this.PhasesListView.View = System.Windows.Forms.View.Details;
      this.PhasesListView.Resize += new System.EventHandler(this.PhasesListView_Resize);
      // 
      // columnHeader1
      // 
      this.columnHeader1.Text = "Действие";
      this.columnHeader1.Width = 250;
      // 
      // columnHeader2
      // 
      this.columnHeader2.Text = "Состояние";
      this.columnHeader2.Width = 150;
      // 
      // SecondPanel
      // 
      this.SecondPanel.Controls.Add(this.tableLayoutPanel1);
      this.SecondPanel.Controls.Add(this.groupBox1);
      this.SecondPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.SecondPanel.Location = new System.Drawing.Point(0, 164);
      this.SecondPanel.Name = "SecondPanel";
      this.SecondPanel.Size = new System.Drawing.Size(430, 134);
      this.SecondPanel.TabIndex = 0;
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel1.ColumnCount = 5;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 31F));
      this.tableLayoutPanel1.Controls.Add(this.btnCopyText, 4, 0);
      this.tableLayoutPanel1.Controls.Add(this.btnCancel, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.lblWaitCancel, 3, 0);
      this.tableLayoutPanel1.Controls.Add(this.btnDebug, 0, 0);
      this.tableLayoutPanel1.Location = new System.Drawing.Point(14, 98);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 1;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(403, 33);
      this.tableLayoutPanel1.TabIndex = 2;
      // 
      // btnCopyText
      // 
      this.btnCopyText.Location = new System.Drawing.Point(375, 3);
      this.btnCopyText.Name = "btnCopyText";
      this.btnCopyText.Size = new System.Drawing.Size(24, 24);
      this.btnCopyText.TabIndex = 2;
      this.btnCopyText.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.btnCancel.Location = new System.Drawing.Point(160, 3);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(82, 24);
      this.btnCancel.TabIndex = 0;
      this.btnCancel.Text = "Отмена";
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // lblWaitCancel
      // 
      this.lblWaitCancel.Location = new System.Drawing.Point(248, 0);
      this.lblWaitCancel.Name = "lblWaitCancel";
      this.lblWaitCancel.Size = new System.Drawing.Size(121, 30);
      this.lblWaitCancel.TabIndex = 1;
      this.lblWaitCancel.Text = "Ждите прерывания операции...";
      this.lblWaitCancel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lblWaitCancel.UseMnemonic = false;
      // 
      // btnDebug
      // 
      this.btnDebug.Location = new System.Drawing.Point(3, 3);
      this.btnDebug.Name = "btnDebug";
      this.btnDebug.Size = new System.Drawing.Size(24, 24);
      this.btnDebug.TabIndex = 3;
      this.btnDebug.Text = "?";
      this.btnDebug.UseVisualStyleBackColor = true;
      this.btnDebug.Visible = false;
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.lblCurrent);
      this.groupBox1.Controls.Add(this.pb1);
      this.groupBox1.Location = new System.Drawing.Point(8, 6);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(409, 92);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Текущее действие";
      // 
      // lblCurrent
      // 
      this.lblCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.lblCurrent.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblCurrent.Location = new System.Drawing.Point(6, 16);
      this.lblCurrent.Name = "lblCurrent";
      this.lblCurrent.Size = new System.Drawing.Size(397, 44);
      this.lblCurrent.TabIndex = 0;
      this.lblCurrent.Text = "Текущее действие";
      this.lblCurrent.UseMnemonic = false;
      // 
      // pb1
      // 
      this.pb1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.pb1.Location = new System.Drawing.Point(6, 63);
      this.pb1.Name = "pb1";
      this.pb1.Size = new System.Drawing.Size(397, 23);
      this.pb1.TabIndex = 1;
      this.pb1.Value = 40;
      // 
      // MainPanel
      // 
      this.MainPanel.Controls.Add(this.PhasesListView);
      this.MainPanel.Controls.Add(this.SecondPanel);
      this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.MainPanel.Location = new System.Drawing.Point(0, 0);
      this.MainPanel.Name = "MainPanel";
      this.MainPanel.Size = new System.Drawing.Size(430, 298);
      this.MainPanel.TabIndex = 2;
      // 
      // StdImageList
      // 
      this.StdImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("StdImageList.ImageStream")));
      this.StdImageList.TransparentColor = System.Drawing.Color.Magenta;
      this.StdImageList.Images.SetKeyName(0, "Cancel");
      this.StdImageList.Images.SetKeyName(1, "Copy");
      // 
      // SplashForm
      // 
      this.AcceptButton = this.btnCancel;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(430, 298);
      this.ControlBox = false;
      this.Controls.Add(this.MainPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SplashForm";
      this.ShowIcon = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.Text = "Ждите ...";
      this.SecondPanel.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.MainPanel.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    #endregion

    #region Переопределенные методы

    protected override void OnLoad(EventArgs args)
    {
      base.OnLoad(args);

      // Вычисляем размер списка и устанавливаем размер формы
      //int FormH = SystemInformation.SizingBorderWidth * 2 + SystemInformation.CaptionHeight; // Заголовок
      int FormH = Height - ClientSize.Height;
      FormH += SecondPanel.Size.Height; // нижняя панель

      if (PhasesListView.Items.Count > 1) // Когда одна фаза список не показываем
      {
        FormH += (PhasesListView.GetItemRect(0).Height + 1) * PhasesListView.Items.Count +
            2 * SystemInformation.Border3DSize.Height;
      }
      else
        PhasesListView.Visible = false;

      int MaxFormH = Screen.FromControl(this).WorkingArea.Height * 3 / 4;
      if (FormH > MaxFormH)
        FormH = MaxFormH;
      else
        PhasesListView.Scrollable = false;
      Height = FormH;

      //StartPosition = FormStartPosition.CenterScreen;
      EFPApp.PlaceFormInScreenCenter(this);
    }

    protected override void OnClosing(CancelEventArgs args)
    {
      base.OnClosing(args);
      // пользователь не может сам закрыть форму
      args.Cancel = true;
    }

    #endregion

    /// <summary>
    /// Флаг нажатия кнопки отмена
    /// </summary>
    public bool Cancelled;

    /// <summary>
    /// Нажатие кнопки "Отмена"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void btnCancel_Click(object sender, EventArgs args)
    {
      Cancelled = true;
      lblWaitCancel.Visible = true;
      btnCancel.Enabled = false;
    }

    private void PhasesListView_Resize(object sender, EventArgs args)
    {
      PhasesListView.Columns[0].Width = PhasesListView.ClientSize.Width * 5 / 8;
      PhasesListView.Columns[1].Width = -2; // автоподбор
    }

    void btnCopyText_Click(object sender, EventArgs args)
    {
      EFPApp.Clipboard.SetText(lblCurrent.Text);
    }

  }
}
