// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Xml;
using FreeLibSet.IO;
using FreeLibSet.Logging;
using FreeLibSet.DependedValues;
using FreeLibSet.Parsing;
using FreeLibSet.Shell;
using FreeLibSet.Core;
using FreeLibSet.UICore;
using FreeLibSet.Models.Tree;
using FreeLibSet.Controls;

#if NET
#pragma warning disable SYSLIB0005 // Assembly.GlobalAssemblyCache is obsolete
#endif


namespace FreeLibSet.Forms.Diagnostics
{
  /// <summary>
  /// Отладочные средства
  /// </summary>
  public static class DebugTools
  {
    #region Статический конструктор

    static DebugTools()
    {
      // Присоедияем обработчик подавления вывода свойств для графических объектов типа Point
      LogoutTools.LogoutProp += LogoutTools_LogoutProp;
    }

    /// <summary>
    /// Вызов статического конструктора
    /// </summary>
    internal static void DummyInit()
    {
    }

    #endregion

    #region Просмотр ошибок

    /// <summary>
    /// Возможность временной блокировки показа формы ShowExceptionForm, если 
    /// вывод сообщения "зациклился"
    /// </summary>
    public static bool ShowExceptionEnabled
    {
      get
      {
        // 27.12.2020
        // Блокировка не нужна

        //lock (typeof(DebugTools))
        //{
        return _ShowExceptionEnabled;
        //}
      }
      set
      {
        //lock (typeof(DebugTools))
        //{
        _ShowExceptionEnabled = value;
        //}
      }
    }
    private static volatile bool _ShowExceptionEnabled = true;

    private static bool _InsideShowException = false;

    /// <summary>
    /// Вывод на экран окна с сообщением об исключении.
    /// Отчет также записывается в log-файл.
    /// Этот метод может вызываться из любого потока.
    /// </summary>
    /// <param name="e">Перехваченное исключение</param>
    /// <param name="title">Заголовок формы</param>
    public static void ShowException(Exception e, string title)
    {
      ShowException(e, title, true);
    }

    /// <summary>
    /// Вывод на экран окна с сообщением об исключении.
    /// Этот метод может вызываться из любого потока.
    /// </summary>
    /// <param name="e">Перехваченное исключение</param>
    /// <param name="title">Заголовок формы</param>
    /// <param name="useLogout">Надо ли записать отчет в log-файл</param>
    public static void ShowException(Exception e, string title, bool useLogout)
    {
      if (e != null)
      {
        if (e is UserCancelException)
          return; // 18.09.2009. Прерывание пользователем операции не должно выдавать окно
      }

      lock (WinFormsTools.InternalSyncRoot)
      {
        if (ShowExceptionEnabled)
        {
          // Записываем исключение в logout
          AbsPath logFilePath = AbsPath.Empty;
          if (e != null && useLogout)
            logFilePath = LogoutTools.LogoutExceptionToFile(e, title);

          // Показываем форму
          if (!_InsideShowException)
          {
            _InsideShowException = true;
            try
            {
              try
              {
                ShowExceptionForm.ShowException(e, title, null, logFilePath);
              }
              catch (Exception e2)
              {
                AbsPath logFilePath2 = AbsPath.Empty;
                try
                {
                  logFilePath2 = LogoutTools.LogoutExceptionToFile(e2, "Ошибка при выводе отладочного окна просмотра ошибки");
                }
                catch { }
                StringBuilder sb = new StringBuilder();
                sb.Append("Ошибка при выводе отладочного окна просмотра ошибки.");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                if (e != null) // 27.12.2020
                {
                  sb.Append("Исходная ошибка: ");
                  sb.Append(e.Message);
                }
                sb.Append(Environment.NewLine);
                sb.Append("Log-файл: ");
                sb.Append(logFilePath.Path);
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("Ошибка вывода окна: ");
                sb.Append(e2.Message);
                sb.Append(Environment.NewLine);
                sb.Append("Log-файл: ");
                if (logFilePath2.IsEmpty)
                  sb.Append("Не создан");
                else
                  sb.Append(logFilePath2.Path);

                MessageBox.Show(sb.ToString(), "Не удалось показать окно ошибки", MessageBoxButtons.OK, MessageBoxIcon.Error);
              }
            }
            finally
            {
              _InsideShowException = false;
            }
          }
        }
      }
    }

    #endregion

    #region Информация о программных модулях

    /// <summary>
    /// Получить информацию о всех загруженных сборках, кроме помещенных в
    /// Global Assembly Cache. Возвращается объект DataTable, который можно 
    /// использовать в окне "О программе"
    /// </summary>
    /// <returns></returns>
    public static DataTable GetAssembliesInfo()
    {
      return GetAssembliesInfo(false);
    }

    /// <summary>
    /// Получить информацию о всех загруженных сборках
    /// Возвращается объект DataTable, который можно использовать в окне "О программе"
    /// </summary>
    /// <param name="includeGAC">true, если в список должны быть включены сборки из Глобального кэша сборок</param>
    /// <returns></returns>
    public static DataTable GetAssembliesInfo(bool includeGAC)
    {
      DataTable res = new DataTable("AssembliesInfo");
      res.Columns.Add("Order", typeof(int));
      res.Columns.Add("Name", typeof(string));
      res.Columns.Add("Version", typeof(string));
      res.Columns.Add("CreationTime", typeof(DateTime));
      res.Columns.Add("Debug", typeof(bool));
      res.Columns.Add("Description", typeof(string));
      res.Columns.Add("Copyright", typeof(string));
      res.Columns.Add("ProcessorArchitecture", typeof(string));
      res.Columns.Add("Location", typeof(string));
      res.Columns.Add("GAC", typeof(bool));
      Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
      for (int i = 0; i < asms.Length; i++)
      {
        if (!includeGAC)
        {
          if (asms[i].GlobalAssemblyCache)
            continue; // не мое
        }
        AddAssemblyInfo(res, asms[i], i+1);
      }
      return res;
    }

    private static void AddAssemblyInfo(DataTable table, Assembly asm, int order)
    {
      DataRow row = table.NewRow();

      row["Order"] = order;

      string name = asm.FullName;
      int p = name.IndexOf(',');
      if (p >= 0)
        name = name.Substring(0, p);
      row["Name"] = name;

      row["Version"] = asm.GetName().Version.ToString();
      //AssemblyVersionAttribute attrVersion = (AssemblyVersionAttribute)Attribute.GetCustomAttribute(a, typeof(AssemblyVersionAttribute));
      //if (attrVersion != null)
      //  Row["Version"] = attrVersion.Version;

      //Attribute[] aa = Attribute.GetCustomAttributes(a);

      ProcessorArchitecture pa = asm.GetName().ProcessorArchitecture;
      if (pa == ProcessorArchitecture.MSIL)
        row["ProcessorArchitecture"] = "Any CPU";
      else
        row["ProcessorArchitecture"] = pa.ToString();

      string fileName = asm.ManifestModule.FullyQualifiedName;
      if (File.Exists(fileName))
        row["CreationTime"] = File.GetLastWriteTime(fileName);

      DebuggableAttribute attrDebug = (DebuggableAttribute)Attribute.GetCustomAttribute(asm, typeof(DebuggableAttribute));
      if (attrDebug != null)
      {
        if (attrDebug.IsJITTrackingEnabled)
          row["Debug"] = true; // не очень хорошо, но ладно
      }

      AssemblyDescriptionAttribute attrDescr = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyDescriptionAttribute));
      if (attrDescr != null)
        row["Description"] = attrDescr.Description;

      AssemblyCopyrightAttribute attrCopyright = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyCopyrightAttribute));
      if (attrCopyright != null)
        row["Copyright"] = attrCopyright.Copyright;

      DataTools.SetString(row, "Location", asm.Location);

      row["GAC"] = asm.GlobalAssemblyCache;

      table.Rows.Add(row);
    }

    /// <summary>
    /// Вывод информационного сообщения, содержащего загруженные сборки
    /// </summary>
    /// <param name="title">Заголовок сообщения</param>
    public static void DebugAssemblies(string title)
    {
      Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
      string s = "Загруженные сборки для домена " + AppDomain.CurrentDomain.ToString() + "\n";
      foreach (Assembly asm in asms)
        s += asm.FullName + "\n";
      MessageBox.Show(s, title);
    }

    #endregion

    #region Показ информации LogoutTools.GetDebugInfo

    /// <summary>
    /// Вывод отладочной информации о программе
    /// </summary>
    public static void ShowDebugInfo()
    {
      ShowDebugInfo(null);
    }

    /// <summary>
    /// Вывод отладочной информации о программе
    /// </summary>
    /// <param name="title">Заголовок окна</param>
    public static void ShowDebugInfo(string title)
    {
      if (String.IsNullOrEmpty(title))
        title = "Отладочная информация о программе";

      string s;
      if (EFPApp.IsMainThread)
        EFPApp.BeginWait("Сбор информации о программе", "Debug");
      try
      {
        s = LogoutTools.GetDebugInfo();
      }
      finally
      {
        if (EFPApp.IsMainThread)
          EFPApp.EndWait();
      }

      EFPApp.ShowTextView(s, title, true);
    }

    #endregion

    #region Произвольный объект (свойства)

    /// <summary>
    /// Отладка произвольного объекта в окне
    /// </summary>
    /// <param name="theObject">Отлаживаемый объект</param>
    /// <param name="title">Заголовок</param>
    public static void DebugObject(object theObject, string title)
    {
      if (title == null)
        title = "Отладка объекта";
      if (theObject == null)
      {
        MessageBox.Show("null", title);
        return;
      }

      if (theObject is String)
      {
        string s = (string)theObject;
        //MessageBox.Show("Строка: \"" + s + "\" (Length=" + s.Length + ")", Title);
        DebugString(s, title);
        return;
      }

      if (theObject is IDictionary)
      {
        DebugDictionary((IDictionary)theObject, title);
        return;
      }

      Form frm = new Form();
      try
      {
        frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
        frm.Text = title;
        frm.MinimizeBox = false;
        frm.ShowIcon = false;

        PropertyGrid grid = new PropertyGrid();
        grid.SelectedObject = theObject;
        grid.Dock = DockStyle.Fill;
        frm.Controls.Add(grid);

        Panel panType = new Panel();
        panType.Size = new Size(100, 22);
        panType.Dock = DockStyle.Bottom;
        frm.Controls.Add(panType);
        Label lblType = new Label();
        lblType.Dock = DockStyle.Fill;
        try
        {
          lblType.Text = "GetType(): " + theObject.GetType().ToString();
        }
        catch { }
        lblType.TextAlign = ContentAlignment.MiddleLeft;
        lblType.UseMnemonic = false;
        panType.Controls.Add(lblType);

        Panel panStr = new Panel();
        panStr.Size = new Size(100, 22);
        panStr.Dock = DockStyle.Bottom;
        frm.Controls.Add(panStr);
        Label lblStr = new Label();
        lblStr.Dock = DockStyle.Fill;
        try
        {
          lblStr.Text = "ToString(): " + theObject.ToString();
        }
        catch { }
        lblStr.TextAlign = ContentAlignment.MiddleLeft;
        lblStr.UseMnemonic = false;
        panStr.Controls.Add(lblStr);

        frm.ShowDialog();
      }
      finally
      {
        frm.Dispose();
      }
    }

    private static void DebugString(string s, string title)
    {
      Form frm = new Form();
      try
      {
        frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
        frm.Text = title;
        frm.MinimizeBox = false;
        frm.ShowIcon = false;

        TextBox theTB = new TextBox();
        theTB.Dock = DockStyle.Fill;
        theTB.Multiline = true;
        theTB.ScrollBars = ScrollBars.Both;
        theTB.WordWrap = false;
        theTB.Font = EFPApp.CreateMonospaceFont();
        theTB.ReadOnly = true;
        frm.Controls.Add(theTB);
        theTB.Text = s;

        Label lblLen = new Label();
        lblLen.AutoSize = false;
        lblLen.Height = 16;
        lblLen.Text = "Lenght=" + s.Length.ToString();
        lblLen.Dock = DockStyle.Bottom;
        frm.Controls.Add(lblLen);

        frm.ShowDialog();
      }
      finally
      {
        frm.Dispose();
      }
    }

    private static void DebugDictionary(IDictionary dictionary, string title)
    {
      Form frm = new Form();
      try
      {
        frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
        frm.Text = title;
        frm.MinimizeBox = false;
        frm.ShowIcon = false;

        DataGridView dictGrid = new DataGridView();
        dictGrid.Dock = DockStyle.Fill;
        frm.Controls.Add(dictGrid);

        DataGridViewTextBoxColumn col;
        col = new DataGridViewTextBoxColumn();
        col.HeaderText = "Index";
        col.Width = 100;
        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        col.SortMode = DataGridViewColumnSortMode.NotSortable;
        dictGrid.Columns.Add(col);

        col = new DataGridViewTextBoxColumn();
        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        col.HeaderText = "Key";
        col.FillWeight = 40;
        col.SortMode = DataGridViewColumnSortMode.NotSortable;
        dictGrid.Columns.Add(col);

        col = new DataGridViewTextBoxColumn();
        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        col.HeaderText = "Value";
        col.FillWeight = 40;
        col.SortMode = DataGridViewColumnSortMode.NotSortable;
        dictGrid.Columns.Add(col);

        dictGrid.Tag = dictionary;
        dictGrid.VirtualMode = true;
        dictGrid.RowCount = dictionary.Count;
        dictGrid.CellValueNeeded += new DataGridViewCellValueEventHandler(DictGrid_CellValueNeeded);
        int cnt = 0;
        foreach (DictionaryEntry pair in dictionary)
        {
          dictGrid[1, cnt].Tag = pair.Key;
          dictGrid[2, cnt].Tag = pair.Value;
          cnt++;
        }

        dictGrid.ReadOnly = true;
        dictGrid.AllowUserToAddRows = false;
        dictGrid.AllowUserToOrderColumns = false;
        dictGrid.CellDoubleClick += new DataGridViewCellEventHandler(DictGrid_CellDoubleClick);

        frm.ShowDialog();
      }
      finally
      {
        frm.Dispose();
      }
    }

    static void DictGrid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs args)
    {
      if (args.ColumnIndex == 0)
      {
        args.Value = args.RowIndex;
        return;
      }

      try
      {
        DataGridView dictGrid = (DataGridView)sender;
        object obj = dictGrid[args.ColumnIndex, args.RowIndex].Tag;
        if (obj == null)
          args.Value = "null";
        else
          args.Value = obj.ToString();
      }
      catch (Exception e)
      {
        args.Value = "Error: " + e.Message;
      }
    }

    static void DictGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs args)
    {
      if (args.ColumnIndex < 1 || args.RowIndex < 0)
      {
        MessageBox.Show("Должен быть выбран ключ или значение", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      try
      {

        DataGridView dictGrid = (DataGridView)sender;
        object obj = dictGrid[args.ColumnIndex, args.RowIndex].Tag;
        string suffix;
        if (args.ColumnIndex == 1)
          suffix = "Keys[" + args.RowIndex + "]";
        else
        {
          object key = dictGrid[1, args.RowIndex].Tag;
          suffix = "[" + key.ToString() + "]"; // Key не может быть null
        }
        DebugObject(obj, dictGrid.FindForm().Text + " - " + suffix);
      }
      catch (Exception e)
      {
        DebugTools.ShowException(e, "Ошибка показа ключа/значения коллекции");
      }
    }

    #endregion

    #region Управляющий элемент с дочерними элементами

    /// <summary>
    /// Отладка управляющего элемента
    /// </summary>
    /// <param name="rootControl">Отображаемый элемент</param>
    public static void DebugControls(Control rootControl)
    {
      if (rootControl == null)
      {
        MessageBox.Show("null", "Отладка Control");
        return;
      }

      Form frm = new Form();
      frm.ShowIcon = false;
      frm.Text = "Отладка Control";
      SplitContainer splControl = new SplitContainer();
      splControl.Dock = DockStyle.Fill;
      frm.Controls.Add(splControl);

      TreeView theTV = new TreeView();
      theTV.Dock = DockStyle.Fill;
      splControl.Panel1.Controls.Add(theTV);

      PropertyGrid theGrid = new PropertyGrid();
      theGrid.Dock = DockStyle.Fill;
      splControl.Panel2.Controls.Add(theGrid);

      // Заполняем дерево
      AddControlToTV(/*TheTV, */rootControl, theTV.Nodes);
      theTV.Tag = theGrid;
      theTV.AfterSelect += new TreeViewEventHandler(TheTV_AfterSelect);

      frm.WindowState = FormWindowState.Maximized;
      frm.ShowDialog();
      frm.Dispose();

    }

    private static void AddControlToTV(/*TreeView theTV, */Control control, TreeNodeCollection nodes)
    {
      string text;
      if (control == null)
        text = "[ null ]";
      else
        text = control.ToString();
      TreeNode node = nodes.Add(text);
      node.Tag = control;

      if (control != null) // 27.12.2020
      {
        if (control.HasChildren)
        {
          for (int i = 0; i < control.Controls.Count; i++)
            AddControlToTV(/*theTV, */control.Controls[i], node.Nodes);
        }
      }
    }

    static void TheTV_AfterSelect(object sender, TreeViewEventArgs args)
    {
      TreeView theTV = (TreeView)sender;
      PropertyGrid theGrid = (PropertyGrid)(theTV.Tag);
      if (args.Node == null)
        theGrid.SelectedObject = null;
      else
        theGrid.SelectedObject = args.Node.Tag;
    }

    #endregion

    #region Команды меню EFPCommandItems

    /// <summary>
    /// Модель дерева для команд меню.
    /// Объектом узла модели является <see cref="EFPCommandItem"/>
    /// </summary>
    private class EFPCommandItemsTreeModel : Models.Tree.TreeModelBase
    {
      #region Конструктор

      public EFPCommandItemsTreeModel(EFPCommandItems commandItems)
      {
        if (commandItems == null)
          throw new ArgumentNullException("commandItems");

        _CommandItems = commandItems;
      }

      private EFPCommandItems _CommandItems;

      #endregion

      #region Переопределенные методы

      public override IEnumerable GetChildren(TreePath treePath)
      {
        if (treePath.IsEmpty)
          return _CommandItems.TopLevelItems.Items;
        else
        {
          EFPCommandItem item = (EFPCommandItem)(treePath.LastNode);
          return item.Children.Items;
        }
      }

      public override bool IsLeaf(TreePath treePath)
      {
        EFPCommandItem item = (EFPCommandItem)(treePath.LastNode);
        if (item == null)
          return false;
        else
          return !item.HasChildren;
      }

      #endregion
    }

    /// <summary>
    /// Отладка списка команд меню, панелей инструментов.
    /// Метод может вызываться только из того потока, в котором разрешена работа с <see cref="EFPCommandItems"/>.
    /// </summary>
    /// <param name="commandItems">Список команд</param>
    /// <param name="title">Заголовок окна</param>
    public static void DebugCommandItems(EFPCommandItems commandItems, string title)
    {
      if (title == null)
        title = "EFPCommandItems";

      if (commandItems == null)
      {
        EFPApp.MessageBox("EFPCommandItems=null", title);
        return;
      }

      Form frm = new Form();
      frm.Text = title;
      frm.Icon = EFPApp.MainImages.Icons["Debug"];
      frm.StartPosition = FormStartPosition.WindowsDefaultBounds;
      EFPFormProvider formProvider = new EFPFormProvider(frm);
      EFPTabControl efpTabControl = new EFPTabControl(formProvider);

      EFPTabPage efpTPTree = efpTabControl.TabPages.Add("Command");
      efpTPTree.ImageKey = "TreeView";
      EFPControlWithToolBar<TreeViewAdv> cwtTree = new EFPControlWithToolBar<TreeViewAdv>(efpTPTree);
      EFPDataTreeView efpTree = new EFPDataTreeView(cwtTree);
      efpTree.Columns.AddText("CategoryAndName", true, "CategoryAndName", 30, 10);
      efpTree.Columns.AddInt("ItemId", true, "ItemId", 4);
      efpTree.Columns.AddText("Usage", true, "Usage", 20, 10);
      efpTree.Columns.AddBool("MenuUsage", true, "MenuUsage");
      efpTree.Columns.AddBool("ToolBarUsage", true, "ToolBarUsage");
      efpTree.Columns.AddBool("IsInToolBarDropDown", true, "IsInToolBarDropDown");
      efpTree.Columns.AddBool("StatusBarUsage", true, "StatusBarUsage");
      efpTree.Columns.AddBool("ShortCutUsage", true, "ShortCutUsage");
      efpTree.Columns.AddText("MenuText", true, "MenuText", 20, 10);
      //efpTree.Columns.AddBool("HasImage", true, "HasImage");
      Controls.TreeViewAdvNodeControls.NodeIcon niImage = efpTree.Columns.AddImage("Image");
      niImage.VirtualMode = true;
      niImage.ValueNeeded += CommandItemNode_Image_ValueNeeded;
      efpTree.Columns.AddText("ToolTipText", true, "ToolTipText", 30, 10);
      efpTree.Columns.AddText("ShortCutText", true, "ShortCutText", 10, 5);
      efpTree.Columns.AddText("MenuRightText", true, "MenuRightText", 10, 5);
      efpTree.Columns.AddText("StatusBarText", true, "StatusBarText", 15, 5);
      efpTree.Columns.AddText("DisplayName", true, "DisplayName", 20, 10);
      efpTree.Columns.AddBool("Visible", true, "Visible");
      efpTree.Columns.AddBool("Enabled", true, "Enabled");
      efpTree.Columns.AddBool("Checked", true, "Checked");
      efpTree.Columns.AddBool("GroupBegin", true, "GroupBegin");
      efpTree.Columns.AddBool("GroupEnd", true, "GroupEnd");

      Controls.TreeViewAdvNodeControls.NodeTextBox ntbUIObjects = efpTree.Columns.AddText("UIObjects", false, "UIObjects", 40, 10);
      ntbUIObjects.VirtualMode = true;
      ntbUIObjects.ValueNeeded += CommandItemNode_UIObjects_ValueNeeded;

      efpTree.Control.Model = new EFPCommandItemsTreeModel(commandItems);

      EFPApp.ShowFormOrDialog(frm);
    }

    private static void CommandItemNode_Image_ValueNeeded(object sender, Controls.TreeViewAdvNodeControls.NodeControlValueEventArgs args)
    {
      EFPCommandItem ci = args.Node.Tag as EFPCommandItem;
      if (ci == null)
        return;
      if (ci.Image == null)
      {
        if (String.IsNullOrEmpty(ci.ImageKey))
          args.Value = null;
        else
          args.Value = EFPApp.MainImages.Images[ci.ImageKey];
      }
      else
        args.Value = ci.Image;
    }

    private static void CommandItemNode_UIObjects_ValueNeeded(object sender, Controls.TreeViewAdvNodeControls.NodeControlValueEventArgs args)
    {
      EFPCommandItem ci = args.Node.Tag as EFPCommandItem;
      if (ci == null)
        return;

      EFPUIObjBase[] a1 = ci.GetUIObjects();
      string[] a2 = new string[a1.Length];
      for (int i = 0; i < a1.Length; i++)
        a2[i] = a1[i].GetType().Name;
      args.Value = String.Join(", ", a2);
    }

    #endregion

    #region Таблицы

    /// <summary>
    /// Отладка объекта DataSet
    /// </summary>
    /// <param name="ds">Отображаемый объект</param>
    /// <param name="title">Заголовок окна</param>
    public static void DebugDataSet(DataSet ds, string title)
    {
      if (title == null)
        title = "Набор данных DataSet";

      if (ds == null)
      {
        MessageBox.Show("DataSet=null", title);
        return;
      }

      Form frm = new Form();
      //AccDepClientExec.InitForm(TheForm);
      //TheForm.Icon = EFPApp.MainImageIcon("Debug");
      frm.ShowIcon = false;
      frm.Text = title;
      frm.WindowState = FormWindowState.Maximized;
      frm.MinimizeBox = false;

      TabControl theTabControl = new TabControl();
      theTabControl.Dock = DockStyle.Fill;
      frm.Controls.Add(theTabControl);
      for (int i = 0; i < ds.Tables.Count; i++)
      {
        TabPage page = new TabPage(ds.Tables[i].TableName);
        theTabControl.Controls.Add(page);
        AddDebugDataTable(ds.Tables[i], page);
      }

      TabPage tpProps = new TabPage("Свойства DataSet");
      theTabControl.TabPages.Add(tpProps);

      PropertyGrid pg = new PropertyGrid();
      pg.Dock = DockStyle.Fill;
      tpProps.Controls.Add(pg);
      pg.SelectedObject = ds;

      frm.ShowDialog();
      frm.Dispose();
    }

    /// <summary>
    /// Отладка таблицы DataTable
    /// </summary>
    /// <param name="table">Отображаемый объект</param>
    /// <param name="title">Заголовок окна</param>
    public static void DebugDataTable(DataTable table, string title)
    {
      if (table == null)
      {
        MessageBox.Show("Table=null", title);
        return;
      }

      if (title == null)
        title = table.TableName;

      Form frm = new Form();
      frm.ShowIcon = false;
      //TheForm.Icon = EFPApp.MainImageIcon("Debug");
      //AccDepClientExec.InitForm(TheForm);
      AddDebugDataTable(table, frm);
      frm.Text = title;
      frm.WindowState = FormWindowState.Maximized;
      frm.MinimizeBox = false;
      frm.ShowDialog();
      frm.Close();
    }

    private static void AddDebugDataTable(DataTable table, Control parent)
    {
      if (!String.IsNullOrEmpty(table.DefaultView.RowFilter))
        MessageBox.Show("Предупреждение. У таблицы " + table.ToString() +
          " установлен RowFilter=" + table.DefaultView.RowFilter.ToString(), "Отладка таблицы");

      TabControl theTabControl = new TabControl();
      theTabControl.Dock = DockStyle.Fill;
      theTabControl.Alignment = TabAlignment.Bottom;
      parent.Controls.Add(theTabControl);

      TabPage tpTable = new TabPage("Rows (" + table.Rows.Count.ToString() + ")");
      theTabControl.TabPages.Add(tpTable);

      DataGridView grid1 = CreateDebugGrid(tpTable, table, false);
      grid1.RowCount = table.Rows.Count; // должно быть после инициализации столбцов

      TabPage tpProps1 = new TabPage("Свойства DataTable");
      theTabControl.TabPages.Add(tpProps1);

      PropertyGrid pg1 = new PropertyGrid();
      pg1.Dock = DockStyle.Fill;
      tpProps1.Controls.Add(pg1);
      pg1.SelectedObject = table;

      TabPage tpDV = new TabPage("DefaultView (" + table.DefaultView.Count.ToString() + ")");
      theTabControl.TabPages.Add(tpDV);
      DataGridView grid2 = CreateDebugGrid(tpDV, table, true);
      grid2.DataSource = table.DefaultView;

      TabPage tpProps2 = new TabPage("Свойства DefaultView");
      theTabControl.TabPages.Add(tpProps2);

      PropertyGrid pg2 = new PropertyGrid();
      pg2.Dock = DockStyle.Fill;
      tpProps2.Controls.Add(pg2);
      pg2.SelectedObject = table.DefaultView;
    }

    private static DataGridView CreateDebugGrid(Control parentControl, DataTable table, bool isDV)
    {
      DataGridView grid = new DataGridView();
      grid.Dock = DockStyle.Fill;
      grid.ReadOnly = true;
      grid.AllowUserToAddRows = false;
      grid.AllowUserToDeleteRows = true;
      grid.AutoGenerateColumns = false;
      if (table.Rows.Count <= 200)
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
      else
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
      parentControl.Controls.Add(grid);

      DataGridViewImageColumn colImg = new DataGridViewImageColumn();
      colImg.Width = 24;
      grid.Columns.Add(colImg);

      DataGridViewTextBoxColumn colRN = new DataGridViewTextBoxColumn();
      if (isDV)
        colRN.HeaderText = "Index";
      else
        colRN.HeaderText = "Row index";
      colRN.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      grid.Columns.Add(colRN);

      DataGridViewTextBoxColumn colState = new DataGridViewTextBoxColumn();
      colState.HeaderText = "Row state";
      colState.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
      grid.Columns.Add(colState);

      for (int i = 0; i < table.Columns.Count; i++)
      {
        if (table.Columns[i].DataType == typeof(Boolean))
        {
          DataGridViewCheckBoxColumn col = new DataGridViewCheckBoxColumn();
          col.HeaderText = table.Columns[i].ColumnName;
          grid.Columns.Add(col);
        }
        else
        {
          DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
          col.HeaderText = table.Columns[i].ColumnName;

          Type dtp = table.Columns[i].DataType;
          if (dtp == typeof(string))
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
          else if (dtp == typeof(DateTime) || dtp == typeof(TimeSpan))
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
          else
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

          grid.Columns.Add(col);
        }
      }
      DisableOrdering(grid);
      grid.CellValueNeeded += new DataGridViewCellValueEventHandler(DebugTableGrid_CellValueNeeded);
      grid.Tag = table;
      grid.VirtualMode = true;

      return grid;
    }

    private static void DisableOrdering(DataGridView control)
    {
      for (int i = 0; i < control.Columns.Count; i++)
        control.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
    }


    static void DebugTableGrid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs args)
    {
      DataGridView grid = (DataGridView)sender;
      if (args.RowIndex < 0)
        return;

      if (args.ColumnIndex < 0)
        return;

      try
      {
        DataTable table = (DataTable)(grid.Tag);
        DataRowState rowState = table.Rows[args.RowIndex].RowState;

        switch (args.ColumnIndex)
        {
          case 0:
            if (!EFPApp.AppWasInit)
              return;
            if (!EFPApp.IsMainThread)
              return;

            string imageKey;
            switch (rowState)
            {
              case DataRowState.Added: imageKey = "Insert"; break;
              case DataRowState.Deleted: imageKey = "Delete"; break;
              case DataRowState.Modified: imageKey = "Edit"; break;
              case DataRowState.Unchanged: imageKey = "View"; break;
              default: imageKey = "UnknownState"; break;
            }
            args.Value = EFPApp.MainImages.Images[imageKey];
            break;
          case 1:
            args.Value = args.RowIndex;
            break;
          case 2:
            args.Value = rowState.ToString();
            break;
          default:
            if (grid.DataSource == null)
            {
              // Просмотр таблицы, а не DefaultView
              switch (rowState)
              {
                case DataRowState.Added:
                case DataRowState.Unchanged:
                case DataRowState.Modified:
                  object x = table.Rows[args.RowIndex][args.ColumnIndex - 3, DataRowVersion.Current];
                  args.Value = x;
                  /*
                  if (RowState == DataRowState.Modified)
                  {
                    object OldVal = Table.Rows[Args.RowIndex][Args.ColumnIndex - 3, DataRowVersion.Original];
                    if (!DataTools.IsEqualValues(Args.Value, OldVal))
                      Args.
                  }
                   * */
                  break;
                case DataRowState.Deleted:
                  args.Value = table.Rows[args.RowIndex][args.ColumnIndex - 3, DataRowVersion.Original];
                  break;
              }
            }
            break;
        }
      }
      catch (Exception e)
      {
        args.Value = "!!!" + e.Message;
      }
    }

    /// <summary>
    /// Отладка одной строки таблицы в виде пар "Имя столбца - значение"
    /// </summary>
    /// <param name="row">Строка таблицы для показа</param>
    /// <param name="title">Заголовок окна</param>
    public static void DebugDataRow(DataRow row, string title)
    {
      if (row == null)
      {
        if (title == null)
          title = "Строка DataRow";

        MessageBox.Show("Row=null", title);
        return;
      }

      if (title == null)
      {
        title = "Строка таблицы " + row.Table.TableName + " (" + row.RowState.ToString() + ")";
        int p = row.Table.Rows.IndexOf(row);
        if (p >= 0)
          title += " (Номер строки: " + p.ToString() + ")";
        else
          title += " (Не присоединена)";
      }

      Form frm = new Form();
      frm.ShowIcon = false;
      //TheForm.Icon = EFPApp.MainImageIcon("Debug");
      //AccDepClientExec.InitForm(TheForm);
      Screen scr = Screen.FromControl(frm);
      frm.Size = new System.Drawing.Size(scr.Bounds.Width / 2, scr.Bounds.Height - 100);
      frm.MinimizeBox = false;
      frm.Text = title;
      //TheForm.WindowState = FormWindowState.Maximized;


      TabControl theTabControl = new TabControl();
      theTabControl.Dock = DockStyle.Fill;
      theTabControl.Alignment = TabAlignment.Bottom;
      frm.Controls.Add(theTabControl);

      TabPage tpData = new TabPage("Данные");
      theTabControl.TabPages.Add(tpData);

      DataGridView grid = new DataGridView();
      grid.Dock = DockStyle.Fill;
      grid.ReadOnly = true;
      grid.AllowUserToAddRows = false;
      grid.AllowUserToDeleteRows = true;

      DataGridViewTextBoxColumn col;
      col = new DataGridViewTextBoxColumn();
      col.HeaderText = "Index";
      col.Width = 50;
      grid.Columns.Add(col);

      col = new DataGridViewTextBoxColumn();
      col.HeaderText = "Column Name";
      col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      col.FillWeight = 40;
      grid.Columns.Add(col);

      col = new DataGridViewTextBoxColumn();
      col.HeaderText = "Value";
      col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      col.FillWeight = 60;
      grid.Columns.Add(col);

      for (int i = 0; i < row.Table.Columns.Count; i++)
      {
        grid.Rows.Add(i, row.Table.Columns[i].ColumnName,
          row[i]);
      }

      tpData.Controls.Add(grid);

      TabPage tpProps = new TabPage("Свойства DataRow");
      theTabControl.TabPages.Add(tpProps);

      PropertyGrid pg = new PropertyGrid();
      pg.Dock = DockStyle.Fill;
      tpProps.Controls.Add(pg);
      pg.SelectedObject = row;

      frm.ShowDialog();
      frm.Dispose();
    }

    /// <summary>
    /// Отладка объекта DataView
    /// </summary>
    /// <param name="dv">Отображаемый объект</param>
    /// <param name="title">Заголовок окна</param>
    public static void DebugDataView(DataView dv, string title)
    {
      if (dv == null)
      {
        MessageBox.Show("DV=null", title);
        return;
      }

      if (title == null)
        title = "DataView для " + dv.Table.TableName;


      Form frm = new Form();
      frm.ShowIcon = false;
      //TheForm.Icon = EFPApp.MainImageIcon("Debug");
      //AccDepClientExec.InitForm(TheForm);
      frm.Text = title;
      frm.WindowState = FormWindowState.Maximized;
      frm.MinimizeBox = false;

      TabControl theTabControl = new TabControl();
      theTabControl.Dock = DockStyle.Fill;
      theTabControl.Alignment = TabAlignment.Bottom;
      frm.Controls.Add(theTabControl);

      TabPage tpData = new TabPage("Данные");
      theTabControl.TabPages.Add(tpData);

      DataGridView grid = new DataGridView();
      grid.Dock = DockStyle.Fill;
      grid.ReadOnly = true;
      grid.AllowUserToAddRows = false;
      grid.AllowUserToDeleteRows = true;
      grid.AutoGenerateColumns = true;
      grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
      tpData.Controls.Add(grid);
      grid.DataSource = dv;

      TabPage tpProps = new TabPage("Свойства DataView");
      theTabControl.TabPages.Add(tpProps);

      PropertyGrid pg = new PropertyGrid();
      pg.Dock = DockStyle.Fill;
      tpProps.Controls.Add(pg);
      pg.SelectedObject = dv;

      frm.ShowDialog();
      frm.Close();
    }

    #endregion

    #region ChangeInfo

    /// <summary>
    /// Отладка объекта DepChangeInfo
    /// </summary>
    /// <param name="changeInfo">Отображаемый объект</param>
    /// <param name="title">Заголовок окна</param>
    public static void DebugChangeInfo(DepChangeInfo changeInfo, string title)
    {
      Form frm = new Form();
      if (String.IsNullOrEmpty(title))
        frm.Text = "Просмотр изменений";
      else
        frm.Text = title;
      frm.Icon = EFPApp.MainImages.Icons["Debug"];
      EFPFormProvider efpForm = new EFPFormProvider(frm);
      EFPApp.SetFormSize(frm, 50, 75);

      TreeView tv = new TreeView();
      tv.Dock = DockStyle.Fill;
      frm.Controls.Add(tv);
      tv.ImageList = EFPApp.MainImages.ImageList;
      AddDebugChangeInfo(tv.Nodes, changeInfo, 0);
      tv.ExpandAll();
      EFPApp.ShowDialog(frm, true);
    }

    private static void AddDebugChangeInfo(TreeNodeCollection nodes, DepChangeInfo changeInfo, int level)
    {
      if (level > 10)
      {
        TreeNode node = nodes.Add("[ Слишком большой уровень вложения ]");
        node.ImageKey = "Error";
        node.SelectedImageKey = node.ImageKey;
        return;
      }

      if (changeInfo == null)
      {
        TreeNode node = nodes.Add("[ null ]");
        node.ImageKey = "No";
        node.SelectedImageKey = node.ImageKey;
      }
      else
      {
        string displayName = changeInfo.DisplayName;
        if (String.IsNullOrEmpty(displayName))
          displayName = "[ Баз заголовка ]";
        if (changeInfo.Changed)
          displayName = "(*) " + displayName;
        TreeNode node = nodes.Add(displayName);
        if (changeInfo.Changed)
          node.ImageKey = "Warning";
        else
          node.ImageKey = "Item";
        node.SelectedImageKey = node.ImageKey;

        if (changeInfo is DepChangeInfoValueItem)
        {
          DepChangeInfoValueItem ci = (DepChangeInfoValueItem)changeInfo;
          string s;
          if (ci.OriginalValue == null)
            s = "null";
          else
            s = ci.OriginalValue.ToString();
          TreeNode nodeSrc = node.Nodes.Add(null, "Исходное значение: " + s, "View");
          nodeSrc.ImageKey = "EmptyImage"; // NodeSrc.Parent.ImageKey;
          nodeSrc.SelectedImageKey = "EmptyImage"; //NodeSrc.Parent.SelectedImageKey;

          if (ci.CurrentValue == null)
            s = "null";
          else
            s = ci.CurrentValue.ToString();
          TreeNode nodeCurr = node.Nodes.Add(null, "Текущее значение : " + s, "View");
          nodeCurr.ImageKey = "EmptyImage"; //NodeCurr.Parent.ImageKey;
          nodeCurr.SelectedImageKey = "EmptyImage"; //NodeCurr.Parent.SelectedImageKey;
        }
        else if (changeInfo is DepChangeInfoList)
        {
          node.ImageKey = "TreeViewClosedFolder";
          node.SelectedImageKey = "TreeViewOpenFolder";

          DepChangeInfoList cil = (DepChangeInfoList)changeInfo;
          foreach (DepChangeInfo ci in cil)
            AddDebugChangeInfo(node.Nodes, ci, level + 1); // рекурсивный вызов
        }
        // Для простого DepChangeInfoItem ничего не добавляется
      }
    }

    #endregion

    #region EFPBaseProvider

#if !XXX
    /// <summary>
    /// Отладка объекта IEFPCheckItem 
    /// </summary>
    /// <param name="baseProvider">Отображаемый объект (EFPFormProvider или EFPBaseProvider)</param>
    /// <param name="title">Заголовок окна</param>
    public static void DebugBaseProvider(EFPBaseProvider baseProvider, string title)
    {
      if (String.IsNullOrEmpty(title))
      {
        if (baseProvider == null)
          title = "EFPBaseProvider";
        else
          title = baseProvider.ToString();
      }

      if (baseProvider == null)
      {
        EFPApp.ErrorMessageBox("null", title);
        return;
      }

      Form frm = new Form();
      frm.Text = title;
      frm.Icon = EFPApp.MainImages.Icons["Debug"];
      EFPFormProvider efpForm = new EFPFormProvider(frm);
      EFPApp.SetFormSize(frm, 50, 75);

      TreeView tv = new TreeView();
      tv.Dock = DockStyle.Fill;
      frm.Controls.Add(tv);
      tv.ImageList = EFPApp.MainImages.ImageList;
      AddDebugBaseProvider(tv.Nodes, baseProvider, 0);
      tv.ExpandAll();
      EFPApp.ShowDialog(frm, true);
    }

    private static void AddDebugBaseProvider(TreeNodeCollection nodes, EFPBaseProvider baseProvider, int level)
    {
      if (level > 10)
      {
        TreeNode node = nodes.Add("[ Слишком большой уровень вложения ]");
        node.ImageKey = "Error";
        node.SelectedImageKey = node.ImageKey;
        return;
      }

      TreeNode thisNode = nodes.Add(baseProvider.ToString());
      if (baseProvider is EFPFormProvider)
        thisNode.ImageKey = "View";
      else
        thisNode.ImageKey = "Anchor"; // Почему нет?
      thisNode.SelectedImageKey = thisNode.ImageKey;

      if (baseProvider.Children.Count > 0)
      {
        TreeNode grpNode = thisNode.Nodes.Add("Дочерние EFPBaseProvider (" + baseProvider.Children.Count.ToString() + ")");
        grpNode.ImageKey = "TreeViewClosedFolder";
        grpNode.SelectedImageKey = "TreeViewOpenFolder";
        foreach (EFPBaseProvider child in baseProvider.Children)
          AddDebugBaseProvider(grpNode.Nodes, child, level + 1); // рекурсивный вызов
      }

      if (baseProvider.ControlProviders.Count > 0)
      {
        TreeNode grpNode = thisNode.Nodes.Add("Control Providers (" + baseProvider.ControlProviders.Count.ToString() + ")");
        grpNode.ImageKey = "TreeViewClosedFolder";
        grpNode.SelectedImageKey = "TreeViewOpenFolder";
        foreach (EFPControlBase cp in baseProvider.ControlProviders)
        {
          TreeNode node = grpNode.Nodes.Add(cp.ToString());
          if (cp.ProviderState == EFPControlProviderState.Attached)
          {
            if (cp.Control.Visible)
            {
              if (cp.Editable)
                node.ImageKey = GetImageKey(cp.ValidateState);
              else
                node.ImageKey = "Pause";
            }
            else
              node.ImageKey = "NoView";

          }
          else
            node.ImageKey = "Cancel";
          node.SelectedImageKey = node.ImageKey;
        }
      }

      if (baseProvider.FormChecks.Count > 0)
      {
        TreeNode grpNode = thisNode.Nodes.Add("Form checks (" + baseProvider.FormChecks.Count.ToString() + ")");
        grpNode.ImageKey = "TreeViewClosedFolder";
        grpNode.SelectedImageKey = "TreeViewOpenFolder";
        foreach (EFPFormCheck fc in baseProvider.FormChecks)
        {
          TreeNode node = grpNode.Nodes.Add(fc.ToString());
          node.ImageKey = GetImageKey(fc.ValidateState);
          node.SelectedImageKey = node.ImageKey;
        }
      }
    }

    private static string GetImageKey(UIValidateState state)
    {
      switch (state)
      {
        case UIValidateState.Error: return "Error";
        case UIValidateState.Warning: return "Warning";
        case UIValidateState.Ok: return "Ok";
        default: return "UnknownState";
      }
    }

#endif

    #endregion

    #region Список форм

    /// <summary>
    /// Получить информацию обо всех открытых окнах программы (включая модальные и немодальные)
    /// </summary>
    /// <returns></returns>
    public static DataTable CreateFormsTable()
    {
      DataTable table = new DataTable();
      table.Columns.Add("Handle", typeof(Int64));
      table.Columns.Add("ClassName", typeof(string));
      table.Columns.Add("Caption", typeof(string));
      table.Columns.Add("Visible", typeof(bool));
      table.Columns.Add("Enabled", typeof(bool));
      table.Columns.Add("Modal", typeof(bool));
      table.Columns.Add("WindowState", typeof(string));
      table.Columns.Add("OwnerHandle", typeof(Int64));
      table.Columns.Add("ParentHandle", typeof(Int64));
      table.Columns.Add("TopLevel", typeof(bool));
      table.Columns.Add("TopMost", typeof(bool));
      table.Columns.Add("Icon", typeof(byte[]));
      DataTools.SetPrimaryKey(table, "Handle");

      foreach (Form form in Application.OpenForms)
        AddFormInfoToTable(table, form);

      return table;
    }

    private static void AddFormInfoToTable(DataTable table, Form form)
    {
      if (form == null)
        return;
      if (form.Handle.ToInt64() == 0)
        return;

      if (table.Rows.Find(form.Handle.ToInt64()) != null)
        return; // повторный вызов

      DataRow row = table.NewRow();
      row["ClassName"] = form.GetType().ToString();
      row["Caption"] = form.Text;
      row["Handle"] = form.Handle.ToInt64();
      row["Visible"] = form.Visible;
      row["Enabled"] = form.Enabled;
      row["Modal"] = form.Modal;
      row["WindowState"] = form.WindowState.ToString();
      if (form.Owner != null)
        row["OwnerHandle"] = form.Owner.Handle.ToInt64();
      if (form.ParentForm != null)
        row["ParentHandle"] = form.ParentForm.Handle.ToInt64();
      row["TopLevel"] = form.TopLevel;
      row["TopMost"] = form.TopMost;
      if (form.ShowIcon && form.FormBorderStyle != FormBorderStyle.None &&
        form.FormBorderStyle != FormBorderStyle.FixedToolWindow &&
        form.FormBorderStyle != FormBorderStyle.SizableToolWindow)
      {
        MemoryStream stm = new MemoryStream();
        try
        {
          form.Icon.Save(stm);
          row["Icon"] = stm.ToArray();
        }
        finally
        {
          stm.Dispose();
        }
      }
      table.Rows.Add(row);

      // Добавляем соседние формы
      if (form.Owner != null)
        AddFormInfoToTable(table, form.Owner);
      if (form.ParentForm != null)
        AddFormInfoToTable(table, form.ParentForm);
      if (form.MdiParent != null)
        AddFormInfoToTable(table, form.MdiParent);
    }

    #endregion

    #region Отслеживание фокуса ввода

    private class DebugFocusForm : Form, IEFPAppTimeHandler
    {
      #region Конструктор

      public DebugFocusForm()
      {
        base.FormBorderStyle = FormBorderStyle.None;
        base.StartPosition = FormStartPosition.Manual;
        base.Size = new Size(500, 25);
        base.Location = new Point(20, 5);
        base.Enabled = false;
        base.TopMost = true;
        base.BackColor = Color.Yellow;
        base.ForeColor = Color.Navy;

        _TheLabel = new Label();
        _TheLabel.Location = new Point(0, 0);
        _TheLabel.Size = base.Size;
        _TheLabel.AutoSize = true;
        _TheLabel.UseMnemonic = false;
        // TheLabel.AutoEllipsis = true;

        Controls.Add(_TheLabel);

        EFPApp.Timers.Add(this);
      }

      protected override void Dispose(bool disposing)
      {
        EFPApp.Timers.Remove(this);
        base.Dispose(disposing);
      }

      public void TimerTick()
      {
        //TheLabel.Text = DateTime.Now.Second.ToString();
        StringBuilder sb = new StringBuilder();
        //Form TheForm = Form.ActiveForm;

        Control ctrl = Form.ActiveForm;
        if (ctrl == null)
          sb.Append("Form.ActiveForm=null");
        else
        {
          while (ctrl != null)
          {
            if (sb.Length > 0)
              sb.Append(Environment.NewLine);
            sb.Append(ctrl.GetType().Name);
            sb.Append(", Name=\"");
            sb.Append(ctrl.Name);
            sb.Append("\", Text=\"");

            string s = ctrl.Text;
            // 27.07.2022. Если элемент - многострочный TextBox, то надо выводить только первую строку, да и ту не целиком
            int p = DataTools.IndexOfAny(s, "\r\n\t");
            if (p >= 0)
              s = s.Substring(0, p) + " ...";
            if (s.Length > 40)
              s = s.Substring(0, 40) + " ...";
            sb.Append(s);

            sb.Append('\"');
            IContainerControl ctrl2 = ctrl as IContainerControl;
            if (ctrl2 != null)
              ctrl = ctrl2.ActiveControl;
            else
              break;
          }
        }

        EFPCommandItems[] a = EFPCommandItems.GetFocusedObjects();
        sb.Append(Environment.NewLine);
        sb.Append("EFPCommandItems.GetFocusedObjects()");
        sb.Append(" (");
        sb.Append(a.Length.ToString());
        sb.Append(")");
        for (int i = 0; i < a.Length; i++)
        {
          sb.Append(Environment.NewLine);
          sb.Append("  ");
          sb.Append(a[i].GetType().ToString());
          sb.Append(": ");
          sb.Append(a[i].ToString());
        }

        _TheLabel.Text = sb.ToString();
        base.Size = _TheLabel.Size;
      }

      #endregion

      #region Поля

      private readonly Label _TheLabel;

      #endregion
    }

    /// <summary>
    /// Управляет наличием окна трассировки активного управляющего элемента
    /// </summary>
    public static bool DebugFocusWindowVisible
    {
      get { return _TheDebugFocusForm != null; }
      set
      {
        if (value == (_TheDebugFocusForm != null))
          return;
        if (value)
        {
          _TheDebugFocusForm = new DebugFocusForm();
          _TheDebugFocusForm.Visible = true;
        }
        else
        {
          _TheDebugFocusForm.Dispose();
          _TheDebugFocusForm = null;
        }
      }
    }
    private static DebugFocusForm _TheDebugFocusForm;

    #endregion

    #region Вывод текста

    /// <summary>
    /// Вывод текста в отладочных целях.
    /// Показывает модальное окно с TextBox'ом с заданным текстом
    /// Метод может быть вызван из любого потока.
    /// В форме не используется EFPFormProvider
    /// </summary>
    /// <param name="text">Выводимый текст</param>
    /// <param name="title">Заголовок окна</param>
    public static void ShowText(string text, string title)
    {
      Form frm = new Form();
      if (String.IsNullOrEmpty(text))
        frm.Text = "Отладочная информация";
      else
        frm.Text = title;
      frm.ShowIcon = false;
      frm.WindowState = FormWindowState.Maximized;

      TextBox edText = new TextBox();
      edText.Multiline = true;
      edText.ReadOnly = true;
      edText.Dock = DockStyle.Fill;
      edText.Font = EFPApp.CreateMonospaceFont();
      edText.ScrollBars = ScrollBars.Both;
      frm.Controls.Add(edText);
      if (!String.IsNullOrEmpty(text))
        edText.Text = text;
      edText.WordWrap = false;
      edText.Select(0, 0);

      frm.ShowDialog();
      frm.Close();
    }

    #endregion

    #region Документ XML

    /// <summary>
    /// Просмотр XML-документа
    /// </summary>
    /// <param name="xmlDoc">Отображаемый документ</param>
    /// <param name="title">Заголовок окна</param>
    public static void DebugXml(XmlDocument xmlDoc, string title)
    {
      string xmlText;
      StringWriter wrt1 = new StringWriter();
      try
      {
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        XmlWriter wrt2 = XmlWriter.Create(wrt1, settings);
        xmlDoc.WriteTo(wrt2);
        wrt2.Close();
        xmlText = wrt1.ToString();
      }
      finally
      {
        wrt1.Dispose();
      }

      // Что получили, то и выводим
      ShowText(xmlText, title);
    }

    #endregion

    #region Парсинг

    /// <summary>
    /// Отладка объекта ParsingData
    /// </summary>
    /// <param name="parsingData">Отлаживаемый объект</param>
    /// <param name="title">Заголовок окна</param>
    /// <param name="expression"></param>
    public static void DebugParsingData(ParsingData parsingData, string title, IExpression expression)
    {
      DebugParsingForm frm = new DebugParsingForm();
      if (String.IsNullOrEmpty(title))
        frm.Text = "Парсинг";
      else
        frm.Text = title;

      frm.ParsingData = parsingData;
      frm.Expression = expression;

      EFPApp.ShowDialog(frm, true);
    }

    /// <summary>
    /// Вывод отладочной информации о парсинге.
    /// Выполняется попытка создать выражение IExpression
    /// </summary>
    /// <param name="parsingData">Отлаживаемый объект</param>
    /// <param name="title">Заголовок окна</param>
    public static void DebugParsingData(ParsingData parsingData, string title)
    {
      IExpression expr = null;
      if (parsingData.Parsers != null)
        expr = parsingData.Parsers.CreateExpression(parsingData, false);
      DebugParsingData(parsingData, title, expr);
    }

    #endregion

    #region Обработчик LogoutTools_LogoutInfoNeeded

    internal static void LogoutTools_LogoutInfoNeeded(object sender, LogoutInfoNeededEventArgs args)
    {
      int CurrIndentLevel = args.IndentLevel;

      #region Application

      args.WriteHeader("Application");
      args.WritePair("AllowQuit", Application.AllowQuit.ToString());
      //Args.WritePair("CommonAppDataPath", Application.CommonAppDataPath);
      //Args.WritePair("CommonAppDataRegistry", Application.CommonAppDataRegistry);
      args.WritePair("CurrentCulture", Application.CurrentCulture.ToString());

      try
      {
        if (Application.CurrentInputLanguage != null)
          args.WritePair("CurrentInputLanguage", Application.CurrentInputLanguage.LayoutName);
      }
      catch { } // 06.02.2023. Может быть ошибка в Wine

      args.WritePair("MessageLoop", Application.MessageLoop.ToString()); // 02.06.2017

      if (Application.OpenForms != null)
      {
        args.WritePair("OpenForms", Application.OpenForms.Count.ToString());
        if (EFPApp.IsMainThread)
        {
          args.IndentLevel++;
          foreach (Form frm in Application.OpenForms)
          {
            //args.WritePair(frm.GetType().ToString(), frm.Text);
            // 21.01.2021
            // Нельзя вызывать свойство Form.Text из не того потока, в котором создана форма
            string title;
            if (frm.InvokeRequired)
              title = "[* form created in differnet thread *]";
            else
              title = frm.Text;
            args.WritePair(frm.GetType().ToString(), title);
          }
          args.IndentLevel--;
        }
      }
      args.WritePair("RenderWithVisualStyles", Application.RenderWithVisualStyles.ToString());
      args.WritePair("VisualStyleState", Application.VisualStyleState.ToString());
      args.WritePair("ToolStripManager.VisualStylesEnabled", ToolStripManager.VisualStylesEnabled.ToString());
      args.WritePair("ToolStripManager.RenderMode", ToolStripManager.RenderMode.ToString());

      args.WriteLine();

      #endregion

      #region SystemInformation

      args.WriteLine("SystemInformation");
      args.IndentLevel++;
      args.WritePair("BootMode", SystemInformation.BootMode.ToString());
      // уже выведено в LogoutTools Args.WritePair("ComputerName", SystemInformation.ComputerName);
      args.WritePair("DebugOS", SystemInformation.DebugOS.ToString());
      args.WritePair("Network", SystemInformation.Network.ToString());

      args.WriteLine("PowerStatus");
      args.IndentLevel++;
      try
      {
        if (SystemInformation.PowerStatus != null)
        {
          args.WritePair("PowerLineStatus", SystemInformation.PowerStatus.PowerLineStatus.ToString());
          int prc = (int)(Math.Round(SystemInformation.PowerStatus.BatteryLifePercent * 100.0));
          if (prc >= 0 && prc <= 100)
            args.WritePair("Battery charge", prc.ToString() + "%");
          else
            args.WritePair("Battery charge", "Unknown");
        }
        else
          args.WriteLine("PowerStatus=null");
      }
      catch (Exception e)
      {
        args.WriteLine("*** Error PowerStatus information *** " + e.Message);
      }
      args.IndentLevel--;

      args.WritePair("UserInteractive", SystemInformation.UserInteractive.ToString());
      args.WritePair("TerminalServerSession", SystemInformation.TerminalServerSession.ToString());
      args.WritePair("Secure", SystemInformation.Secure.ToString());
      args.WritePair("VirtualScreen", SystemInformation.VirtualScreen.ToString());
      args.WritePair("WorkingArea", SystemInformation.WorkingArea.ToString());

      if (Screen.AllScreens != null)
      {
        args.WritePair("Screens", Screen.AllScreens.Length.ToString());
        args.IndentLevel++;
        for (int i = 0; i < Screen.AllScreens.Length; i++)
        {
          Screen scr = Screen.AllScreens[i];
          string DevName = scr.DeviceName;
          int p = DevName.IndexOf('\0');
          if (p >= 0) // 19.02.2016. Имя устройства может содержать символы CHR(0), которые явно не нужны
            DevName = DevName.Substring(0, p);
          args.WritePair((i + 1).ToString() + (scr.Primary ? " (primary)" : ""), DevName);
          args.IndentLevel++;
          args.WritePair("Bounds", scr.Bounds.ToString());
          args.WritePair("WorkingArea", scr.WorkingArea.ToString());
          args.WritePair("BitsPerPixel", scr.BitsPerPixel.ToString());
          args.IndentLevel--;
        }
        args.IndentLevel--;
      }
      args.IndentLevel--;

      #endregion

      #region EFPApp

      args.WriteHeader("EFPApp");
      args.WritePair("AppStartTime", EFPApp.AppStartTime.ToString());
      args.WritePair("CurrentDirectory", EFPApp.CurrentDirectory.Path);
      args.WritePair("IsMainThread", EFPApp.IsMainThread.ToString());
      if (EFPApp.IsMainThread)
      {
        Form[] dlgs = EFPApp.GetDialogStack();
        args.WritePair("Dialog stack", "Count=" + dlgs.Length.ToString());
        // Выводим диалоги в более привычном порядке
        for (int i = dlgs.Length - 1; i >= 0; i--)
        {
          args.IndentLevel++;

          // 21.01.2021
          // Нельзя вызывать свойство Form.Text из не того потока, в котором создана форма
          string title;
          if (dlgs[i].InvokeRequired)
            title = "[* form created in different thread *]";
          else
            title = dlgs[i].Text;
          args.WriteLine("[" + i.ToString() + "] " + title + " [" + dlgs[i].GetType().ToString() + "]");
        }
        args.IndentLevel = CurrIndentLevel;

        if (EFPApp.IsMainThread) // 21.01.2021
        {
          if (EFPApp.MainImages.Images==null)
            args.WritePair("MainImages", "null"); // 26.04.2024
          else
            args.WritePair("MainImages", EFPApp.MainImages.Images.Count.ToString());
        }
      }

      args.WritePair("Interface", EFPApp.InterfaceName);
      if (EFPApp.Interface != null)
      {
        args.IndentLevel++;
        LogoutTools.LogoutObject(args, EFPApp.Interface);
        args.IndentLevel = CurrIndentLevel;
      }

      args.WritePair("MainWindowVisible", EFPApp.MainWindowVisible.ToString());
      args.WritePair("MainWindowTitle", EFPApp.MainWindowTitle);
      if (EFPApp.IsMainThread && EFPApp.MainWindow != null)
      {
        args.WriteLine("MainWindow");
        args.IndentLevel++;
        args.WritePair("Text", EFPApp.MainWindow.Text);
        args.WritePair("Bounds", EFPApp.MainWindow.Bounds.ToString());
        args.WritePair("WindowState", EFPApp.MainWindow.WindowState.ToString());
        args.IndentLevel--;
      }
      args.WritePair("DialogOwnerWindow", EFPApp.DialogOwnerWindow == null ? "null" : (EFPApp.DialogOwnerWindow.ToString()));
      if (EFPApp.IsMainThread)
        args.WritePair("ExternalDialogOwnerWindow", EFPApp.ExternalDialogOwnerWindow == null ? "null" : (EFPApp.ExternalDialogOwnerWindow.ToString()));
      args.WritePair("CompositionHistoryCount", EFPApp.CompositionHistoryCount.ToString());
      args.WritePair("ConfigManager", EFPApp.ConfigManager.ToString());
      if (!(EFPApp.ConfigManager is EFPDummyConfigManager))
      {
        args.IndentLevel++;
        LogoutTools.LogoutObject(args, EFPApp.ConfigManager);
        args.IndentLevel--;
      }
      args.WriteLine("UI simplicity");
      args.IndentLevel++;
      args.WritePair("EasyInterface", EFPApp.EasyInterface.ToString());
      args.WritePair("ShowControlToolBars", EFPApp.ShowControlToolBars.ToString());
      args.WritePair("ShowToolTips", EFPApp.ShowToolTips.ToString());
      args.WritePair("ShowListImages", EFPApp.ShowListImages.ToString());
      args.WritePair("ShowAutoCalcSums", EFPApp.ShowAutoCalcSums.ToString());
      args.IndentLevel--;

      IEFPAppTimeHandler[] timeHandlers = EFPApp.Timers.ToArray();
      args.WritePair("EFPApp.Timers", timeHandlers.Length.ToString());
      args.IndentLevel++;
      // LogoutTools.LogoutObject(Args, TimeHandlers);
      // 31.10.2018 Подробные сведения излишни
      for (int i = 0; i < timeHandlers.Length; i++)
        args.WritePair("[" + i.ToString() + "]", timeHandlers[i].GetType().ToString());
      args.IndentLevel--;

      IEFPAppIdleHandler[] idleHandlers = EFPApp.IdleHandlers.ToArray();
      args.WritePair("EFPApp.IdleHandlers", idleHandlers.Length.ToString());
      args.IndentLevel++;
      //LogoutTools.LogoutObject(Args, IdleHandlers);
      // 31.10.2018 Подробные сведения излишни
      for (int i = 0; i < idleHandlers.Length; i++)
        args.WritePair("[" + i.ToString() + "]", idleHandlers[i].GetType().ToString());
      args.IndentLevel--;

      if (EFPApp.IsMainThread)
      {
        args.WritePair("AsyncProcList", "Count=" + EFPApp.ExecProcCallCount.ToString());
        if (EFPApp.ExecProcCallCount > 0)
        {
          FreeLibSet.Remoting.IExecProc[] procs = EFPApp.ExecProcList.ToArray();
          args.IndentLevel++;
          try
          {
            LogoutTools.LogoutObject(args, procs);
          }
          catch (Exception e)
          {
            args.WriteLine("Error. " + e.Message);
          }
          args.IndentLevel--;
        }

        args.WritePair("RemoteUICallBacks", "Count=" + EFPApp.RemoteUICallBackCount.ToString());
        if (EFPApp.RemoteUICallBackCount > 0)
        {
          FreeLibSet.Remoting.IExecProcCallBack[] procs = new FreeLibSet.Remoting.IExecProcCallBack[EFPApp.RemoteUICallBacks.Count];
          EFPApp.RemoteUICallBacks.CopyTo(procs, 0);

          args.IndentLevel++;
          try
          {
            LogoutTools.LogoutObject(args, procs);
          }
          catch (Exception e)
          {
            args.WriteLine("Ошибка. " + e.Message);
          }
          args.IndentLevel--;
        }
      }
      else
        args.WriteLine("AsyncProcList information is available from the main thread olny");

      #endregion

      args.WritePair("Exited", EFPApp.Exited.ToString());
      args.WritePair("ExitCount", EFPApp.ExitCount.ToString());
      args.WritePair("IsClosing", EFPApp.IsClosing.ToString());

      args.WritePair("ApplicationExceptionHandler.HandlerCount", ApplicationExceptionHandler.HandlerCount.ToString());

      #region EFPFormProvider' s

      EFPFormProvider[] formProviders = EFPFormProvider.GetAllFormProviders();
      args.WriteLine("EFPFormProvider objects (" + formProviders.Length.ToString() + ")");
      args.IndentLevel++;
      for (int i = 0; i < formProviders.Length; i++)
      {
        StringBuilder sb = new StringBuilder();
        sb.Append(formProviders[i].ToString());
        Form frm = formProviders[i].Form;
        if (frm.IsDisposed)
          sb.Append(", Disposed");
        if (frm.Visible)
          sb.Append(", Visible");
        else
          sb.Append(", Hidden");
        if (frm.Modal)
          sb.Append(", Modal");
        if (frm.IsMdiContainer)
          sb.Append(", MdiContainer");
        if (frm.IsMdiChild)
          sb.Append(", MdiChild");
        if (frm.TopLevel)
          sb.Append(", TopLevel");
        if (frm.DialogResult != DialogResult.None)
        {
          sb.Append(", DialogResult=");
          sb.Append(frm.DialogResult.ToString());
        }
        args.WritePair("[" + i.ToString() + "]", sb.ToString());
        args.IndentLevel++;
        args.IndentLevel--;
      }
      args.IndentLevel--;

      #endregion

      #region Офисные приложения

      args.WriteHeader("Office applications");

      if (MicrosoftOfficeTools.WordVersion == null)
        args.WritePair("Microsoft Word", "Not installed");
      else
      {
        args.WritePair("Microsoft Word", MicrosoftOfficeTools.WordVersionString);
        args.IndentLevel++;
        args.WritePair("Word executable path", MicrosoftOfficeTools.WordPath.Path);
        args.IndentLevel--;
      }
      if (MicrosoftOfficeTools.ExcelVersion == null)
        args.WritePair("Microsoft Excel", "Not installed");
      else
      {
        args.WritePair("Microsoft Excel", MicrosoftOfficeTools.ExcelVersionString);
        args.IndentLevel++;
        args.WritePair("Excel executable path", MicrosoftOfficeTools.ExcelPath.Path);
        args.IndentLevel--;
      }

      args.WriteLine();

      args.WritePair(EFPApp.OpenOfficeKindName + " Writer", GetVersionStr(EFPApp.OpenOfficeWriterVersion));
      args.WritePair(EFPApp.OpenOfficeKindName + " Calc", GetVersionStr(EFPApp.OpenOfficeCalcVersion));
      args.WriteLine("All installed OpenOffice / LibreOffice applications (" + OpenOfficeTools.Installations.Length.ToString() + ")");
      for (int i = 0; i < OpenOfficeTools.Installations.Length; i++)
      {
        OpenOfficeInfo inf = OpenOfficeTools.Installations[i];
        args.WritePair("[" + (i + 1).ToString() + "]", inf.ToString()); // Название и версия
        args.IndentLevel++;
        args.WritePair("Program dir", inf.ProgramDir.Path);
        args.WritePair("Components", inf.ComponentsCSVString);
        string s2 = String.Empty;
        if (!String.IsNullOrEmpty(inf.InfoSourceString))
          s2 = " (" + inf.InfoSourceString + ")";
        args.WritePair("Info Source", inf.InfoSource.ToString() + s2);
        args.IndentLevel--;
      }


      #endregion
    }

    private static string GetVersionStr(Version version)
    {
      if (version.Major == 0 && version.Minor == 0)
        return "Not installed";
      else
        return version.ToString();
    }

    #endregion

    #region Обработчик LogoutTools_LogoutProp

    static void LogoutTools_LogoutProp(object sender, LogoutPropEventArgs args)
    {
      if (args.Object is Form) // 15.04.2019. Для форм выводим границы
      {
        if (!String.IsNullOrEmpty(args.PropertyName))
        {
          switch (args.PropertyName)
          {
            case "Bounds":
            case "WindowState":
            case "Visible":
            case "Modal":
            case "Parent":
            case "Owner":
              args.Mode = LogoutPropMode.ToString;
              break;
            default:
              args.Mode = LogoutPropMode.None;
              break;
          }
        }
        return;
      }

      if (args.Object is Color ||
        args.Object is Point ||
        args.Object is PointF ||
        args.Object is Size ||
        args.Object is SizeF ||
        args.Object is Rectangle ||
        args.Object is RectangleF ||
        args.Object is Padding ||
        args.Object is Control || // 10.09.2015
        args.Object is EFPControlBase || // 10.09.2015
        args.Object is ToolStripItem) // 23.10.2017
      {
        args.Mode = LogoutPropMode.None;
        return;
      }

      if (args.Object is EFPFormBounds)
      {
        args.Mode = LogoutPropMode.ToString;
        return;
      }

      if (args.Object is EFPAppMainWindowLayout)
      {
        switch (args.PropertyName)
        {
          case "MainMenu":
          case "ToolBars":
          case "StatusBar":
          case "Interface":
            args.Mode = LogoutPropMode.None;
            return;
        }
      }
    }

    #endregion
  }
}
