// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Форма заставки
  /// </summary>
  internal class SplashForm : System.Windows.Forms.Form
  {
    public ListView PhasesListView;
    private ColumnHeader columnHeader1;
    private ColumnHeader columnHeader2;
    public Panel SecondPanel;
    private GroupBox grpCurrent;
    public Label lblCurrent;
    public ProgressBar pb1;
    public Button btnCancel;
    public Label lblWaitCancel;
    public Button btnDebug;
    private TableLayoutPanel tableLayoutPanel1;
    public Panel MainPanel;
    public Button btnCopyText;

    public SplashForm()
    {
      // Required for Windows TheForm Designer support
      InitializeComponent();

      if (EFPApp.MainWindowVisible && EFPApp.IsMainThread)
        Icon = EFPApp.MainImages.Icons["HourGlass"];
      else
        WinFormsTools.InitAppIcon(this);

      // 14.07.2015
      // Используем собственный список изображений на случай, если 
      // вызов выполняется из чужого потока
      if (_ListImageList == null)
        _ListImageList = CreateListImageList(); // для текущего потока
      PhasesListView.SmallImageList = _ListImageList;

      btnCancel.Image = MainImagesResource.Cancel;

      btnCopyText.Image = MainImagesResource.Copy;
      btnCopyText.ImageAlign = ContentAlignment.MiddleCenter;
      btnCopyText.Click += new EventHandler(btnCopyText_Click);

      try
      {
        if (EFPApp.AppHasBeenInit && (!EFPApp.IsMainThread))
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

    #region Windows TheForm Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashForm));
      this.PhasesListView = new System.Windows.Forms.ListView();
      this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.SecondPanel = new System.Windows.Forms.Panel();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.btnCopyText = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.lblWaitCancel = new System.Windows.Forms.Label();
      this.btnDebug = new System.Windows.Forms.Button();
      this.grpCurrent = new System.Windows.Forms.GroupBox();
      this.lblCurrent = new System.Windows.Forms.Label();
      this.pb1 = new System.Windows.Forms.ProgressBar();
      this.MainPanel = new System.Windows.Forms.Panel();
      this.SecondPanel.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.grpCurrent.SuspendLayout();
      this.MainPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // PhasesListView
      // 
      resources.ApplyResources(this.PhasesListView, "PhasesListView");
      this.PhasesListView.AutoArrange = false;
      this.PhasesListView.BackColor = System.Drawing.SystemColors.Control;
      this.PhasesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
      this.PhasesListView.ForeColor = System.Drawing.SystemColors.ControlText;
      this.PhasesListView.FullRowSelect = true;
      this.PhasesListView.GridLines = true;
      this.PhasesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.PhasesListView.HideSelection = false;
      this.PhasesListView.MultiSelect = false;
      this.PhasesListView.Name = "PhasesListView";
      this.PhasesListView.ShowItemToolTips = true;
      this.PhasesListView.TabStop = false;
      this.PhasesListView.UseCompatibleStateImageBehavior = false;
      this.PhasesListView.View = System.Windows.Forms.View.Details;
      this.PhasesListView.Resize += new System.EventHandler(this.PhasesListView_Resize);
      // 
      // columnHeader1
      // 
      resources.ApplyResources(this.columnHeader1, "columnHeader1");
      // 
      // columnHeader2
      // 
      resources.ApplyResources(this.columnHeader2, "columnHeader2");
      // 
      // SecondPanel
      // 
      resources.ApplyResources(this.SecondPanel, "SecondPanel");
      this.SecondPanel.Controls.Add(this.tableLayoutPanel1);
      this.SecondPanel.Controls.Add(this.grpCurrent);
      this.SecondPanel.Name = "SecondPanel";
      // 
      // tableLayoutPanel1
      // 
      resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
      this.tableLayoutPanel1.Controls.Add(this.btnCopyText, 4, 0);
      this.tableLayoutPanel1.Controls.Add(this.btnCancel, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.lblWaitCancel, 3, 0);
      this.tableLayoutPanel1.Controls.Add(this.btnDebug, 0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      // 
      // btnCopyText
      // 
      resources.ApplyResources(this.btnCopyText, "btnCopyText");
      this.btnCopyText.Name = "btnCopyText";
      this.btnCopyText.UseVisualStyleBackColor = true;
      // 
      // btnCancel
      // 
      resources.ApplyResources(this.btnCancel, "btnCancel");
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // lblWaitCancel
      // 
      resources.ApplyResources(this.lblWaitCancel, "lblWaitCancel");
      this.lblWaitCancel.Name = "lblWaitCancel";
      this.lblWaitCancel.UseMnemonic = false;
      // 
      // btnDebug
      // 
      resources.ApplyResources(this.btnDebug, "btnDebug");
      this.btnDebug.Name = "btnDebug";
      this.btnDebug.UseVisualStyleBackColor = true;
      // 
      // grpCurrent
      // 
      resources.ApplyResources(this.grpCurrent, "grpCurrent");
      this.grpCurrent.Controls.Add(this.lblCurrent);
      this.grpCurrent.Controls.Add(this.pb1);
      this.grpCurrent.Name = "grpCurrent";
      this.grpCurrent.TabStop = false;
      // 
      // lblCurrent
      // 
      resources.ApplyResources(this.lblCurrent, "lblCurrent");
      this.lblCurrent.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.lblCurrent.Name = "lblCurrent";
      this.lblCurrent.UseMnemonic = false;
      // 
      // pb1
      // 
      resources.ApplyResources(this.pb1, "pb1");
      this.pb1.Name = "pb1";
      this.pb1.Value = 40;
      // 
      // MainPanel
      // 
      resources.ApplyResources(this.MainPanel, "MainPanel");
      this.MainPanel.Controls.Add(this.PhasesListView);
      this.MainPanel.Controls.Add(this.SecondPanel);
      this.MainPanel.Name = "MainPanel";
      // 
      // SplashForm
      // 
      this.AcceptButton = this.btnCancel;
      resources.ApplyResources(this, "$this");
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ControlBox = false;
      this.Controls.Add(this.MainPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "SplashForm";
      this.ShowIcon = false;
      this.SecondPanel.ResumeLayout(false);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.grpCurrent.ResumeLayout(false);
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
      int formH = Height - ClientSize.Height;
      formH += SecondPanel.Size.Height; // нижняя панель

      if (PhasesListView.Items.Count > 1) // Когда одна фаза список не показываем
      {
        formH += (PhasesListView.GetItemRect(0).Height + 1) * PhasesListView.Items.Count +
            2 * SystemInformation.Border3DSize.Height;
      }
      else
        PhasesListView.Visible = false;

      int maxFormH = Screen.FromControl(this).WorkingArea.Height * 3 / 4;
      if (formH > maxFormH)
        formH = maxFormH;
      else
        PhasesListView.Scrollable = false;
      Height = formH;

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

    #region Статический список изображений

    [ThreadStatic]
    private static ImageList _ListImageList;

    private static ImageList CreateListImageList()
    {
      ImageList il = new ImageList();
      il.ImageSize = new Size(32, 32);
      il.ColorDepth = ColorDepth.Depth32Bit; // с прозрачностью
      il.Images.Add("Item", SplashImagesResource.Item); // 0
      il.Images.Add("Current", SplashImagesResource.Current); // 1
      il.Images.Add("Ok", SplashImagesResource.Ok); // 2
      il.Images.Add("Skip", SplashImagesResource.Skip); // 3
      il.Images.Add("Stop", SplashImagesResource.Stop); // 4 не используется
      return il;
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
      new EFPClipboard().SetText(lblCurrent.Text);
    }

  }
}
