// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  internal partial class SimpleAboutDialogForm : Form
  {
    #region Конструктор

    public SimpleAboutDialogForm()
    {
      InitializeComponent();

      Icon = EFPApp.MainImages.Icons["About"];

      EFPFormProvider efpForm = new EFPFormProvider(this);

      lblNetFramework.Text = EnvironmentTools.NetVersionText;

      EFPButton efpModules = new EFPButton(efpForm, btnModules);
      efpModules.Click += efpModules_Click;

      btnInfo.Image = EFPApp.MainImages.Images["Debug"];
      btnInfo.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpInfo = new EFPButton(efpForm, btnInfo);
      efpInfo.Click += efpInfo_Click;
    }

    void efpModules_Click(object sender, EventArgs args)
    {
      EFPApp.ShowAssembliesDialog();
    }

    void efpInfo_Click(object sender, EventArgs args)
    {
      FreeLibSet.Forms.Diagnostics.DebugTools.ShowDebugInfo("Отладочная информация о программе");
    }

    #endregion

    #region Вывод списка загруженных модулей

    internal static void ShowAssembliesDialog()
    {
      Form frm = new Form();
      try
      {
        EFPApp.BeginWait("Получение списка загруженных сборок");
        try
        {
          frm.Text = "Загруженные программные модули";
          frm.Icon = EFPApp.MainImages.Icons["About"];
          EFPApp.SetFormSize(frm, 50, 50);
          frm.StartPosition = FormStartPosition.CenterScreen;
          frm.WindowState = FormWindowState.Maximized;
          EFPFormProvider efpForm = new EFPFormProvider(frm);

          DataTable table = FreeLibSet.Forms.Diagnostics.DebugTools.GetAssembliesInfo(true);

#if NET
          EFPDataGridView ghMain = InitModulesPage(efpForm, frm);
          ghMain.Control.DataSource = table.DefaultView;
          ghMain.TopLeftCellToolTipText = "Всего загружено сборок: " + table.DefaultView.Count.ToString();
#else // Net Framework
          TabControl theTabControl = new TabControl();
          theTabControl.Dock = DockStyle.Fill;
          frm.Controls.Add(theTabControl);

          TabPage tpMain = new TabPage("Основные модули");
          theTabControl.TabPages.Add(tpMain);
          EFPDataGridView ghMain = InitModulesPage(efpForm, tpMain);

          TabPage tpGAC = new TabPage("Из глобального кэша сборок");
          theTabControl.TabPages.Add(tpGAC);
          EFPDataGridView ghGAC = InitModulesPage(efpForm, tpGAC);

          DataView dvMain = new DataView(table);
          dvMain.RowFilter = "GAC=FALSE";
          ghMain.Control.DataSource = dvMain;
          ghMain.TopLeftCellToolTipText ="Всего загружено сборок: "+dvMain.Count.ToString();

          DataView dvGAC = new DataView(table);
          dvGAC.RowFilter = "GAC=TRUE";
          ghGAC.Control.DataSource = dvGAC;
          ghGAC.TopLeftCellToolTipText = "Всего загружено сборок: " + dvGAC.Count.ToString();
#endif
        }
        finally
        {
          EFPApp.EndWait();
        }

        EFPApp.ShowDialog(frm, true);
      }
      catch
      {
        frm.Dispose();
        throw;
      }
    }

    private static EFPDataGridView InitModulesPage(EFPBaseProvider baseProvider, Control parent)
    {
      DataGridView grid = new DataGridView();
      grid.Dock = DockStyle.Fill;
      parent.Controls.Add(grid);
      grid.ReadOnly = true;
      grid.AllowUserToAddRows = false;
      grid.AllowUserToDeleteRows = false;
      grid.AutoGenerateColumns = false;
      EFPDataGridView gh = new EFPDataGridView(baseProvider, grid);
      gh.Columns.AddInt("Order2", false, "№ п/п", 3);
      gh.Columns.LastAdded.PrintWidth = 100;
      gh.Columns.LastAdded.Printed = false;
      gh.Columns.AddText("Name", true, "Имя сборки", 20);
      gh.Columns.LastAdded.CanIncSearch = true;
      gh.Columns.LastAdded.PrintWidth = 500;
      gh.Columns.AddText("Version", true, "Версия", 12);
      gh.Columns.LastAdded.PrintWidth = 250;
      gh.Columns.AddDateTime("CreationTime", true, "Файл создан");
      gh.Columns.LastAdded.PrintWidth = 300;
      gh.Columns.LastAdded.Printed = false;
      gh.Columns.AddBool("Debug", true, "Отладка");
      gh.Columns.LastAdded.GridColumn.ToolTipText = "Флажок установлен, если сборка скомпилирована в отладочном режиме." +Environment.NewLine+
        "Если флажка нет, сборка скомпилирована с оптимизацией кода";
      gh.Columns.LastAdded.PrintWidth = 100;
      gh.Columns.LastAdded.Printed = false;
      gh.Columns.AddText("Description", true, "Описание", 40);
      gh.Columns.LastAdded.PrintWidth = 1050;
      gh.Columns.AddText("Copyright", true, "Авторские права", 40);
      gh.Columns.LastAdded.PrintWidth = 500;
      gh.Columns.LastAdded.Printed = false;
      gh.Columns.AddText("ProcessorArchitecture", true, "Архитектура", 7);
      gh.Columns.LastAdded.GridColumn.ToolTipText = "Поддерживаемая архитектура процессора";
      gh.Columns.LastAdded.PrintWidth = 200;
      gh.Columns.LastAdded.Printed = false;
      gh.Columns.AddText("Location", true, "Путь к сборке", 40);
      gh.Columns.LastAdded.PrintWidth = 500;
      gh.Columns.LastAdded.Printed = false;
      gh.DisableOrdering();
      gh.FrozenColumns = 1;
      gh.GetCellAttributes += ghModules_GetCellAttributes;

      gh.Orders.Add("Order", "В порядке загрузки", new EFPDataGridViewSortInfo("Order2", ListSortDirection.Ascending)); // сортировка выполняется по DataColumn, а не по виртуальному столбцу
      gh.Orders.Add("Name", "Имя сборки");
      gh.Orders.Add("CreationTime DESC,Name", "Файл создан (новые вверху)", new EFPDataGridViewSortInfo("CreationTime", ListSortDirection.Descending));
      gh.Orders.Add("CreationTime ASC,Name", "Файл создан (старые вверху)", new EFPDataGridViewSortInfo("CreationTime", ListSortDirection.Ascending));
      gh.Orders.Add("Copyright,Name", "Авторские права");
      gh.Orders.Add("ProcessorArchitecture", "Архитектура");
      gh.Orders.Add("Location", "Путь к сборке");
      gh.AutoSort = true;

      gh.ReadOnly = true;
      gh.CanView = false;

      return gh;
    }

    private static void ghModules_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      switch (args.ColumnName)
      {
        case "Order2":
          args.Value = args.RowIndex + 1; 
          break;  
      }
    }

#endregion
  }

  /// <summary>
  /// Блок диалога "О программе"
  /// </summary>
  public class AboutDialog
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует диалог значениями по умолчанию.
    /// Свойства заполняются из основной сборки приложения.
    /// </summary>
    public AboutDialog()
    {
      Title = "О программе";
      try
      {
        Assembly asm = EnvironmentTools.EntryAssembly;
        if (asm != null)
          Init(asm);
        else
          AppName = "Не удалось определить сборку";
      }
      catch (Exception e)
      {
        AppName = "Ошибка определения выполняемой сборки. " + e.Message;
      }
    }

    /// <summary>
    /// Инициализирует диалог значениями из указанной сборки.
    /// Значок все равно берется из основной сборки приложения.
    /// </summary>
    /// <param name="assembly">Сборка для извлечения значений</param>
    public AboutDialog(Assembly assembly)
    {
      if (assembly == null)
        throw new ArgumentNullException("assembly");
      Title = "О программе";
      Init(assembly);
    }

    private void Init(Assembly assembly)
    {
      try
      {
        //AppIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        AppIcon = WinFormsTools.AppIcon;
      }
      catch { }

      try
      {
        string progName = assembly.FullName;
        AssemblyName an = new AssemblyName(progName);

        AppName = EnvironmentTools.GetAssemblyDescription(assembly);
        Version = an.Version.ToString();

        AssemblyCopyrightAttribute attrCopyright = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute));
        if (attrCopyright != null)
          Copyright = attrCopyright.Copyright;

        AssemblyProductAttribute attrProduct = (AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));
        if (attrProduct != null)
          Title = "О программе \""+attrProduct.Product+"\"";

      }
      catch (Exception e)
      {
        Copyright = "Ошибка получения атрибутов. " + e.Message;
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Заголовок блока диалога.
    /// По умолчанию равен "О программе ProductName"
    /// </summary>
    public string Title
    {
      get { return _Title; }
      set { _Title = value; }
    }
    private string _Title;

    /// <summary>
    /// Название программного продукта.
    /// Выводится в верхней части диалога.
    /// </summary>
    public string AppName
    {
      get { return _AppName; }
      set { _AppName = value; }
    }
    private string _AppName;

    /// <summary>
    /// Версия программы
    /// </summary>
    public string Version
    {
      get { return _Version; }
      set { _Version = value; }
    }
    private string _Version;

    /// <summary>
    /// Авторские права
    /// </summary>
    public string Copyright
    {
      get { return _Copyright; }
      set { _Copyright = value; }
    }
    private string _Copyright;

    /// <summary>
    /// Значок программы
    /// </summary>
    public Icon AppIcon
    {
      get { return _AppIcon; }
      set { _AppIcon = value; }
    }
    private Icon _AppIcon;

    #endregion

    #region Показ диалога

    /// <summary>
    /// Показать блок диалога "О программе".
    /// </summary>
    /// <returns>Результат не имеет значения</returns>
    public DialogResult ShowDialog()
    {
      using (SimpleAboutDialogForm frm = new SimpleAboutDialogForm())
      {
        if (AppIcon != null)
        {
          try
          {
            Bitmap bmp = AppIcon.ToBitmap();
            bmp.MakeTransparent(Color.FromArgb(13, 11, 12)); // см. справку к Icon.ToBitmap()
            frm.pbIcon.Image = bmp;
          }
          catch { } // проглатываем ошибку
        }
        frm.Text = Title;

        frm.lblTitle.Text = AppName;
        if (!String.IsNullOrEmpty(Version))
          frm.lblVersion.Text = "Версия " + Version;
        frm.lblCopyright.Text = Copyright;

        return EFPApp.ShowDialog(frm, false);
      }
    }

    #endregion
  }
}
