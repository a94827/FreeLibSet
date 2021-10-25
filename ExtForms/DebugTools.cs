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
          AbsPath LogFilePath = AbsPath.Empty;
          if (e != null && useLogout)
            LogFilePath = LogoutTools.LogoutExceptionToFile(e, title);

          // Показываем форму
          if (!_InsideShowException)
          {
            _InsideShowException = true;
            try
            {
              try
              {
                ShowExceptionForm.ShowException(e, title, null, LogFilePath);
              }
              catch (Exception e2)
              {
                AbsPath LogFilePath2 = AbsPath.Empty;
                try
                {
                  LogFilePath2 = LogoutTools.LogoutExceptionToFile(e2, "Ошибка при выводе отладочного окна просмотра ошибки");
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
                sb.Append(LogFilePath.Path);
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("Ошибка вывода окна: ");
                sb.Append(e2.Message);
                sb.Append(Environment.NewLine);
                sb.Append("Log-файл: ");
                if (LogFilePath2.IsEmpty)
                  sb.Append("Не создан");
                else
                  sb.Append(LogFilePath2.Path);

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
      res.Columns.Add("Name", typeof(string));
      res.Columns.Add("Version", typeof(string));
      res.Columns.Add("CreationTime", typeof(DateTime));
      res.Columns.Add("Debug", typeof(bool));
      res.Columns.Add("Description", typeof(string));
      res.Columns.Add("Copyright", typeof(string));
      res.Columns.Add("ProcessorArchitecture", typeof(string));
      res.Columns.Add("Location", typeof(string));
      res.Columns.Add("GAC", typeof(bool));
      Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
      for (int i = 0; i < asses.Length; i++)
      {
        if (!includeGAC)
        {
          if (asses[i].GlobalAssemblyCache)
            continue; // не мое
        }
        AddAssemblyInfo(res, asses[i]);
      }
      return res;
    }

    private static void AddAssemblyInfo(DataTable table, Assembly assm)
    {
      DataRow Row = table.NewRow();

      string Name = assm.FullName;
      int p = Name.IndexOf(',');
      if (p >= 0)
        Name = Name.Substring(0, p);
      Row["Name"] = Name;

      Row["Version"] = assm.GetName().Version.ToString();
      //AssemblyVersionAttribute attrVersion = (AssemblyVersionAttribute)Attribute.GetCustomAttribute(a, typeof(AssemblyVersionAttribute));
      //if (attrVersion != null)
      //  Row["Version"] = attrVersion.Version;

      //Attribute[] aa = Attribute.GetCustomAttributes(a);

      ProcessorArchitecture pa = assm.GetName().ProcessorArchitecture;
      if (pa == ProcessorArchitecture.MSIL)
        Row["ProcessorArchitecture"] = "Any CPU";
      else
        Row["ProcessorArchitecture"] = pa.ToString();

      string FileName = assm.ManifestModule.FullyQualifiedName;
      if (File.Exists(FileName))
        Row["CreationTime"] = File.GetLastWriteTime(FileName);

      DebuggableAttribute attrDebug = (DebuggableAttribute)Attribute.GetCustomAttribute(assm, typeof(DebuggableAttribute));
      if (attrDebug != null)
      {
        if (attrDebug.IsJITTrackingEnabled)
          Row["Debug"] = true; // не очень хорошо, но ладно
      }

      AssemblyDescriptionAttribute attrDescr = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assm, typeof(AssemblyDescriptionAttribute));
      if (attrDescr != null)
        Row["Description"] = attrDescr.Description;

      AssemblyCopyrightAttribute attrCopyright = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assm, typeof(AssemblyCopyrightAttribute));
      if (attrCopyright != null)
        Row["Copyright"] = attrCopyright.Copyright;

      DataTools.SetString(Row, "Location", assm.Location);

      Row["GAC"] = assm.GlobalAssemblyCache;

      table.Rows.Add(Row);
    }

    /// <summary>
    /// Вывод информационного сообщения, содержащего загруженные сборки
    /// </summary>
    /// <param name="title">Заголовок сообщения</param>
    public static void DebugAssemblies(string title)
    {
      Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
      string s = "Загруженные сборки для домена " + AppDomain.CurrentDomain.ToString() + "\n";
      foreach (Assembly ass in asses)
        s += ass.FullName + "\n";
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

        TextBox TheTB = new TextBox();
        TheTB.Dock = DockStyle.Fill;
        TheTB.Multiline = true;
        TheTB.ScrollBars = ScrollBars.Both;
        TheTB.WordWrap = false;
        TheTB.Font = EFPApp.CreateMonospaceFont();
        TheTB.ReadOnly = true;
        frm.Controls.Add(TheTB);
        TheTB.Text = s;

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

        DataGridView DictGrid = new DataGridView();
        DictGrid.Dock = DockStyle.Fill;
        frm.Controls.Add(DictGrid);

        DataGridViewTextBoxColumn Col;
        Col = new DataGridViewTextBoxColumn();
        Col.HeaderText = "Index";
        Col.Width = 100;
        Col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        Col.SortMode = DataGridViewColumnSortMode.NotSortable;
        DictGrid.Columns.Add(Col);

        Col = new DataGridViewTextBoxColumn();
        Col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        Col.HeaderText = "Key";
        Col.FillWeight = 40;
        Col.SortMode = DataGridViewColumnSortMode.NotSortable;
        DictGrid.Columns.Add(Col);

        Col = new DataGridViewTextBoxColumn();
        Col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        Col.HeaderText = "Value";
        Col.FillWeight = 40;
        Col.SortMode = DataGridViewColumnSortMode.NotSortable;
        DictGrid.Columns.Add(Col);

        DictGrid.Tag = dictionary;
        DictGrid.VirtualMode = true;
        DictGrid.RowCount = dictionary.Count;
        DictGrid.CellValueNeeded += new DataGridViewCellValueEventHandler(DictGrid_CellValueNeeded);
        int cnt = 0;
        foreach (DictionaryEntry Pair in dictionary)
        {
          DictGrid[1, cnt].Tag = Pair.Key;
          DictGrid[2, cnt].Tag = Pair.Value;
          cnt++;
        }

        DictGrid.ReadOnly = true;
        DictGrid.AllowUserToAddRows = false;
        DictGrid.AllowUserToOrderColumns = false;
        DictGrid.CellDoubleClick += new DataGridViewCellEventHandler(DictGrid_CellDoubleClick);

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
        DataGridView DictGrid = (DataGridView)sender;
        object Obj = DictGrid[args.ColumnIndex, args.RowIndex].Tag;
        if (Obj == null)
          args.Value = "null";
        else
          args.Value = Obj.ToString();
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

        DataGridView DictGrid = (DataGridView)sender;
        object Obj = DictGrid[args.ColumnIndex, args.RowIndex].Tag;
        string Suffix;
        if (args.ColumnIndex == 1)
          Suffix = "Keys[" + args.RowIndex + "]";
        else
        {
          object Key = DictGrid[1, args.RowIndex].Tag;
          Suffix = "[" + Key.ToString() + "]"; // Key не может быть null
        }
        DebugObject(Obj, DictGrid.FindForm().Text + " - " + Suffix);
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

      Form Form = new Form();
      Form.ShowIcon = false;
      Form.Text = "Отладка Control";
      SplitContainer Cont = new SplitContainer();
      Cont.Dock = DockStyle.Fill;
      Form.Controls.Add(Cont);

      TreeView TheTV = new TreeView();
      TheTV.Dock = DockStyle.Fill;
      Cont.Panel1.Controls.Add(TheTV);

      PropertyGrid TheGrid = new PropertyGrid();
      TheGrid.Dock = DockStyle.Fill;
      Cont.Panel2.Controls.Add(TheGrid);

      // Заполняем дерево
      AddControlToTV(/*TheTV, */rootControl, TheTV.Nodes);
      TheTV.Tag = TheGrid;
      TheTV.AfterSelect += new TreeViewEventHandler(TheTV_AfterSelect);

      Form.WindowState = FormWindowState.Maximized;
      Form.ShowDialog();
      Form.Dispose();

    }

    private static void AddControlToTV(/*TreeView theTV, */Control control, TreeNodeCollection nodes)
    {
      string Text;
      if (control == null)
        Text = "[ null ]";
      else
        Text = control.ToString();
      TreeNode Node = nodes.Add(Text);
      Node.Tag = control;

      if (control != null) // 27.12.2020
      {
        if (control.HasChildren)
        {
          for (int i = 0; i < control.Controls.Count; i++)
            AddControlToTV(/*theTV, */control.Controls[i], Node.Nodes);
        }
      }
    }

    static void TheTV_AfterSelect(object sender, TreeViewEventArgs args)
    {
      TreeView TheTV = (TreeView)sender;
      PropertyGrid TheGrid = (PropertyGrid)(TheTV.Tag);
      if (args.Node == null)
        TheGrid.SelectedObject = null;
      else
        TheGrid.SelectedObject = args.Node.Tag;
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

      Form TheForm = new Form();
      //AccDepClientExec.InitForm(TheForm);
      //TheForm.Icon = EFPApp.MainImageIcon("Debug");
      TheForm.ShowIcon = false;
      TheForm.Text = title;
      TheForm.WindowState = FormWindowState.Maximized;
      TheForm.MinimizeBox = false;

      TabControl TheTabControl = new TabControl();
      TheTabControl.Dock = DockStyle.Fill;
      TheForm.Controls.Add(TheTabControl);
      for (int i = 0; i < ds.Tables.Count; i++)
      {
        TabPage Page = new TabPage(ds.Tables[i].TableName);
        TheTabControl.Controls.Add(Page);
        AddDebugDataTable(ds.Tables[i], Page);
      }

      TabPage tpProps = new TabPage("Свойства DataSet");
      TheTabControl.TabPages.Add(tpProps);

      PropertyGrid pg = new PropertyGrid();
      pg.Dock = DockStyle.Fill;
      tpProps.Controls.Add(pg);
      pg.SelectedObject = ds;

      TheForm.ShowDialog();
      TheForm.Dispose();
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

      Form form = new Form();
      form.ShowIcon = false;
      //TheForm.Icon = EFPApp.MainImageIcon("Debug");
      //AccDepClientExec.InitForm(TheForm);
      AddDebugDataTable(table, form);
      form.Text = title;
      form.WindowState = FormWindowState.Maximized;
      form.MinimizeBox = false;
      form.ShowDialog();
      form.Close();
    }

    private static void AddDebugDataTable(DataTable table, Control parent)
    {
      if (!String.IsNullOrEmpty(table.DefaultView.RowFilter))
        MessageBox.Show("Предупреждение. У таблицы " + table.ToString() +
          " установлен RowFilter=" + table.DefaultView.RowFilter.ToString(), "Отладка таблицы");

      TabControl TheTabControl = new TabControl();
      TheTabControl.Dock = DockStyle.Fill;
      TheTabControl.Alignment = TabAlignment.Bottom;
      parent.Controls.Add(TheTabControl);

      TabPage tpTable = new TabPage("Rows (" + table.Rows.Count.ToString() + ")");
      TheTabControl.TabPages.Add(tpTable);

      DataGridView grid1 = CreateDebugGrid(tpTable, table, false);
      grid1.RowCount = table.Rows.Count; // должно быть после инициализации столбцов

      TabPage tpProps1 = new TabPage("Свойства DataTable");
      TheTabControl.TabPages.Add(tpProps1);

      PropertyGrid pg1 = new PropertyGrid();
      pg1.Dock = DockStyle.Fill;
      tpProps1.Controls.Add(pg1);
      pg1.SelectedObject = table;

      TabPage tpDV = new TabPage("DefaultView (" + table.DefaultView.Count.ToString() + ")");
      TheTabControl.TabPages.Add(tpDV);
      DataGridView grid2 = CreateDebugGrid(tpDV, table, true);
      grid2.DataSource = table.DefaultView;

      TabPage tpProps2 = new TabPage("Свойства DefaultView");
      TheTabControl.TabPages.Add(tpProps2);

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
      DataGridView Grid = (DataGridView)sender;
      if (args.RowIndex < 0)
        return;

      if (args.ColumnIndex < 0)
        return;

      try
      {
        DataTable Table = (DataTable)(Grid.Tag);
        DataRowState RowState = Table.Rows[args.RowIndex].RowState;

        switch (args.ColumnIndex)
        {
          case 0:
            if (!EFPApp.AppWasInit)
              return;
            if (!EFPApp.IsMainThread)
              return;
            string ImageKey;
            switch (RowState)
            {
              case DataRowState.Added: ImageKey = "Insert"; break;
              case DataRowState.Deleted: ImageKey = "Delete"; break;
              case DataRowState.Modified: ImageKey = "Edit"; break;
              case DataRowState.Unchanged: ImageKey = "View"; break;
              default: ImageKey = "UnknownState"; break;
            }
            args.Value = EFPApp.MainImages.Images[ImageKey];
            break;
          case 1:
            args.Value = args.RowIndex;
            break;
          case 2:
            args.Value = RowState.ToString();
            break;
          default:
            if (Grid.DataSource == null)
            {
              // Просмотр таблицы, а не DefaultView
              switch (RowState)
              {
                case DataRowState.Added:
                case DataRowState.Unchanged:
                case DataRowState.Modified:
                  object x = Table.Rows[args.RowIndex][args.ColumnIndex - 3, DataRowVersion.Current];
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
                  args.Value = Table.Rows[args.RowIndex][args.ColumnIndex - 3, DataRowVersion.Original];
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

      Form TheForm = new Form();
      TheForm.ShowIcon = false;
      //TheForm.Icon = EFPApp.MainImageIcon("Debug");
      //AccDepClientExec.InitForm(TheForm);
      Screen scr = Screen.FromControl(TheForm);
      TheForm.Size = new System.Drawing.Size(scr.Bounds.Width / 2, scr.Bounds.Height - 100);
      TheForm.MinimizeBox = false;
      TheForm.Text = title;
      //TheForm.WindowState = FormWindowState.Maximized;


      TabControl TheTabControl = new TabControl();
      TheTabControl.Dock = DockStyle.Fill;
      TheTabControl.Alignment = TabAlignment.Bottom;
      TheForm.Controls.Add(TheTabControl);

      TabPage tpData = new TabPage("Данные");
      TheTabControl.TabPages.Add(tpData);

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
      TheTabControl.TabPages.Add(tpProps);

      PropertyGrid pg = new PropertyGrid();
      pg.Dock = DockStyle.Fill;
      tpProps.Controls.Add(pg);
      pg.SelectedObject = row;

      TheForm.ShowDialog();
      TheForm.Dispose();
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


      Form TheForm = new Form();
      TheForm.ShowIcon = false;
      //TheForm.Icon = EFPApp.MainImageIcon("Debug");
      //AccDepClientExec.InitForm(TheForm);
      TheForm.Text = title;
      TheForm.WindowState = FormWindowState.Maximized;
      TheForm.MinimizeBox = false;

      TabControl TheTabControl = new TabControl();
      TheTabControl.Dock = DockStyle.Fill;
      TheTabControl.Alignment = TabAlignment.Bottom;
      TheForm.Controls.Add(TheTabControl);

      TabPage tpData = new TabPage("Данные");
      TheTabControl.TabPages.Add(tpData);

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
      TheTabControl.TabPages.Add(tpProps);

      PropertyGrid pg = new PropertyGrid();
      pg.Dock = DockStyle.Fill;
      tpProps.Controls.Add(pg);
      pg.SelectedObject = dv;

      TheForm.ShowDialog();
      TheForm.Close();
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
      Form Form = new Form();
      if (String.IsNullOrEmpty(title))
        Form.Text = "Просмотр изменений";
      else
        Form.Text = title;
      Form.Icon = EFPApp.MainImageIcon("Debug");
      EFPFormProvider efpForm = new EFPFormProvider(Form);
      EFPApp.SetFormSize(Form, 50, 75);

      TreeView tv = new TreeView();
      tv.Dock = DockStyle.Fill;
      Form.Controls.Add(tv);
      tv.ImageList = EFPApp.MainImages;
      AddDebugChangeInfo(tv.Nodes, changeInfo, 0);
      tv.ExpandAll();
      EFPApp.ShowDialog(Form, true);
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
        string DisplayName = changeInfo.DisplayName;
        if (String.IsNullOrEmpty(DisplayName))
          DisplayName = "[ Баз заголовка ]";
        if (changeInfo.Changed)
          DisplayName = "(*) " + DisplayName;
        TreeNode node = nodes.Add(DisplayName);
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
          TreeNode NodeSrc = node.Nodes.Add(null, "Исходное значение: " + s, "View");
          NodeSrc.ImageKey = "EmptyImage"; // NodeSrc.Parent.ImageKey;
          NodeSrc.SelectedImageKey = "EmptyImage"; //NodeSrc.Parent.SelectedImageKey;

          if (ci.CurrentValue == null)
            s = "null";
          else
            s = ci.CurrentValue.ToString();
          TreeNode NodeCurr = node.Nodes.Add(null, "Текущее значение : " + s, "View");
          NodeCurr.ImageKey = "EmptyImage"; //NodeCurr.Parent.ImageKey;
          NodeCurr.SelectedImageKey = "EmptyImage"; //NodeCurr.Parent.SelectedImageKey;
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

      Form Form = new Form();
      Form.Text = title;
      Form.Icon = EFPApp.MainImageIcon("Debug");
      EFPFormProvider efpForm = new EFPFormProvider(Form);
      EFPApp.SetFormSize(Form, 50, 75);

      TreeView tv = new TreeView();
      tv.Dock = DockStyle.Fill;
      Form.Controls.Add(tv);
      tv.ImageList = EFPApp.MainImages;
      AddDebugBaseProvider(tv.Nodes, baseProvider, 0);
      tv.ExpandAll();
      EFPApp.ShowDialog(Form, true);
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
      DataTable Table = new DataTable();
      Table.Columns.Add("Handle", typeof(Int64));
      Table.Columns.Add("ClassName", typeof(string));
      Table.Columns.Add("Caption", typeof(string));
      Table.Columns.Add("Visible", typeof(bool));
      Table.Columns.Add("Enabled", typeof(bool));
      Table.Columns.Add("Modal", typeof(bool));
      Table.Columns.Add("WindowState", typeof(string));
      Table.Columns.Add("OwnerHandle", typeof(Int64));
      Table.Columns.Add("ParentHandle", typeof(Int64));
      Table.Columns.Add("TopLevel", typeof(bool));
      Table.Columns.Add("TopMost", typeof(bool));
      Table.Columns.Add("Icon", typeof(byte[]));
      DataTools.SetPrimaryKey(Table, "Handle");

      foreach (Form Form in Application.OpenForms)
        AddFormInfoToTable(Table, Form);

      return Table;
    }

    private static void AddFormInfoToTable(DataTable table, Form form)
    {
      if (form == null)
        return;
      if (form.Handle.ToInt64() == 0)
        return;

      if (table.Rows.Find(form.Handle.ToInt64()) != null)
        return; // повторный вызов

      DataRow Row = table.NewRow();
      Row["ClassName"] = form.GetType().ToString();
      Row["Caption"] = form.Text;
      Row["Handle"] = form.Handle.ToInt64();
      Row["Visible"] = form.Visible;
      Row["Enabled"] = form.Enabled;
      Row["Modal"] = form.Modal;
      Row["WindowState"] = form.WindowState.ToString();
      if (form.Owner != null)
        Row["OwnerHandle"] = form.Owner.Handle.ToInt64();
      if (form.ParentForm != null)
        Row["ParentHandle"] = form.ParentForm.Handle.ToInt64();
      Row["TopLevel"] = form.TopLevel;
      Row["TopMost"] = form.TopMost;
      if (form.ShowIcon && form.FormBorderStyle != FormBorderStyle.None &&
        form.FormBorderStyle != FormBorderStyle.FixedToolWindow &&
        form.FormBorderStyle != FormBorderStyle.SizableToolWindow)
      {
        MemoryStream stm = new MemoryStream();
        try
        {
          form.Icon.Save(stm);
          Row["Icon"] = stm.ToArray();
        }
        finally
        {
          stm.Dispose();
        }
      }
      table.Rows.Add(Row);

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
            sb.Append(ctrl.Text);
            sb.Append('\"');
            if (ctrl is Form)
              ctrl = ((Form)ctrl).ActiveControl;
            else
              break;
          }
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
      Form TheForm = new Form();
      if (String.IsNullOrEmpty(text))
        TheForm.Text = "Отладочная информация";
      else
        TheForm.Text = title;
      TheForm.ShowIcon = false;
      TheForm.WindowState = FormWindowState.Maximized;

      TextBox edText = new TextBox();
      edText.Multiline = true;
      edText.ReadOnly = true;
      edText.Dock = DockStyle.Fill;
      edText.Font = EFPApp.CreateMonospaceFont();
      edText.ScrollBars = ScrollBars.Both;
      TheForm.Controls.Add(edText);
      if (!String.IsNullOrEmpty(text))
        edText.Text = text;
      edText.WordWrap = false;
      edText.Select(0, 0);

      TheForm.ShowDialog();
      TheForm.Close();
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
      string XmlText;
      StringWriter wrt1 = new StringWriter();
      try
      {
        XmlWriterSettings Settings = new XmlWriterSettings();
        Settings.Indent = true;
        XmlWriter wrt2 = XmlWriter.Create(wrt1, Settings);
        xmlDoc.WriteTo(wrt2);
        wrt2.Close();
        XmlText = wrt1.ToString();
      }
      finally
      {
        wrt1.Dispose();
      }

      // Что получили, то и выводим
      ShowText(XmlText, title);
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
      DebugParsingForm Form = new DebugParsingForm();
      if (String.IsNullOrEmpty(title))
        Form.Text = "Парсинг";
      else
        Form.Text = title;

      Form.ParsingData = parsingData;
      Form.Expression = expression;

      EFPApp.ShowDialog(Form, true);
    }

    /// <summary>
    /// Вывод отладочной информации о парсинге.
    /// Выполняется попытка создать выражение IExpression
    /// </summary>
    /// <param name="parsingData">Отлаживаемый объект</param>
    /// <param name="title">Заголовок окна</param>
    public static void DebugParsingData(ParsingData parsingData, string title)
    {
      IExpression Expr = null;
      if (parsingData.Parsers != null)
        Expr = parsingData.Parsers.CreateExpression(parsingData, false);
      DebugParsingData(parsingData, title, Expr);
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
      if (Application.CurrentInputLanguage != null)
        args.WritePair("CurrentInputLanguage", Application.CurrentInputLanguage.LayoutName);

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
              title = "[* форма создана в другом потоке *]";
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
        args.WriteLine("*** Ошибка получения информации из PowerStatus *** " + e.Message);
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
            title = "[* форма создана в другом потоке *]";
          else
            title = dlgs[i].Text;
          args.WriteLine("[" + i.ToString() + "] " + title + " [" + dlgs[i].GetType().ToString() + "]");
        }
        args.IndentLevel = CurrIndentLevel;

        if (EFPApp.IsMainThread) // 21.01.2021
          args.WritePair("MainImages", EFPApp.MainImages.Images.Count.ToString());
      }

      args.WritePair("Interface", EFPApp.InterfaceName);
      if (EFPApp.Interface != null)
      {
        args.IndentLevel++;
        LogoutTools.LogoutObject(args, EFPApp.Interface);
        args.IndentLevel = CurrIndentLevel;
      }

      args.WritePair("MainWindowVisible", EFPApp.MainWindowVisible.ToString());
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

      args.WriteLine("Упрощение интерфейса");
      args.IndentLevel++;
      args.WritePair("EasyInterface", EFPApp.EasyInterface.ToString());
      args.WritePair("ShowControlToolBars", EFPApp.ShowControlToolBars.ToString());
      args.WritePair("ShowToolTips", EFPApp.ShowToolTips.ToString());
      args.WritePair("ShowListImages", EFPApp.ShowListImages.ToString());
      args.WritePair("ShowAutoCalcSums", EFPApp.ShowAutoCalcSums.ToString());
      args.IndentLevel--;

      IEFPAppTimeHandler[] TimeHandlers = EFPApp.Timers.ToArray();
      args.WritePair("EFPApp.Timers", TimeHandlers.Length.ToString());
      args.IndentLevel++;
      // LogoutTools.LogoutObject(Args, TimeHandlers);
      // 31.10.2018 Подробные сведения излишни
      for (int i = 0; i < TimeHandlers.Length; i++)
        args.WritePair("[" + i.ToString() + "]", TimeHandlers[i].GetType().ToString());
      args.IndentLevel--;

      IEFPAppIdleHandler[] IdleHandlers = EFPApp.IdleHandlers.ToArray();
      args.WritePair("EFPApp.IdleHandlers", IdleHandlers.Length.ToString());
      args.IndentLevel++;
      //LogoutTools.LogoutObject(Args, IdleHandlers);
      // 31.10.2018 Подробные сведения излишни
      for (int i = 0; i < IdleHandlers.Length; i++)
        args.WritePair("[" + i.ToString() + "]", IdleHandlers[i].GetType().ToString());
      args.IndentLevel--;

      if (EFPApp.IsMainThread)
      {
        args.WritePair("AsyncProcList", "Count=" + EFPApp.ExecProcCallCount.ToString());
        if (EFPApp.ExecProcCallCount > 0)
        {
          FreeLibSet.Remoting.IExecProc[] Procs = EFPApp.ExecProcList.ToArray();
          args.IndentLevel++;
          try
          {
            LogoutTools.LogoutObject(args, Procs);
          }
          catch (Exception e)
          {
            args.WriteLine("Ошибка. " + e.Message);
          }
          args.IndentLevel--;
        }

        args.WritePair("RemoteUICallBacks", "Count=" + EFPApp.RemoteUICallBackCount.ToString());
        if (EFPApp.RemoteUICallBackCount > 0)
        {
          FreeLibSet.Remoting.IExecProcCallBack[] Procs = new FreeLibSet.Remoting.IExecProcCallBack[EFPApp.RemoteUICallBacks.Count];
          EFPApp.RemoteUICallBacks.CopyTo(Procs, 0);

          args.IndentLevel++;
          try
          {
            LogoutTools.LogoutObject(args, Procs);
          }
          catch (Exception e)
          {
            args.WriteLine("Ошибка. " + e.Message);
          }
          args.IndentLevel--;
        }
      }
      else
        args.WriteLine("Информация по процедурам обратного вызова доступна только из основного потока приложения");

      #endregion

      args.WritePair("Exited", EFPApp.Exited.ToString());
      args.WritePair("ExitCount", EFPApp.ExitCount.ToString());
      args.WritePair("IsClosing", EFPApp.IsClosing.ToString());

      args.WritePair("ApplicationExceptionHandler.HandlerCount", ApplicationExceptionHandler.HandlerCount.ToString());

      #region EFPFormProvider' s

      EFPFormProvider[] FormProviders = EFPFormProvider.GetAllFormProviders();
      args.WriteLine("Объекты EFPFormProvider (" + FormProviders.Length.ToString() + ")");
      args.IndentLevel++;
      for (int i = 0; i < FormProviders.Length; i++)
      {
        StringBuilder sb = new StringBuilder();
        sb.Append(FormProviders[i].ToString());
        Form frm = FormProviders[i].Form;
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

      args.WriteHeader("Офисные приложения");

      if (MicrosoftOfficeTools.WordVersion == null)
        args.WritePair("Microsoft Word", "Не установлен");
      else
      {
        args.WritePair("Microsoft Word", MicrosoftOfficeTools.WordVersionString);
        args.IndentLevel++;
        args.WritePair("Путь к приложению Word", MicrosoftOfficeTools.WordPath.Path);
        args.IndentLevel--;
      }
      if (MicrosoftOfficeTools.ExcelVersion == null)
        args.WritePair("Microsoft Excel", "Не установлен");
      else
      {
        args.WritePair("Microsoft Excel", MicrosoftOfficeTools.ExcelVersionString);
        args.IndentLevel++;
        args.WritePair("Путь к приложению Excel", MicrosoftOfficeTools.ExcelPath.Path);
        args.IndentLevel--;
      }

      args.WriteLine();

      args.WritePair(EFPApp.OpenOfficeKindName + " Writer", GetVersionStr(EFPApp.OpenOfficeWriterVersion));
      args.WritePair(EFPApp.OpenOfficeKindName + " Calc", GetVersionStr(EFPApp.OpenOfficeCalcVersion));
      args.WriteLine("Все установленные версии OpenOffice / LibreOffice (" + OpenOfficeTools.Installations.Length.ToString() + ")");
      for (int i = 0; i < OpenOfficeTools.Installations.Length; i++)
      {
        OpenOfficeTools.OfficeInfo inf = OpenOfficeTools.Installations[i];
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
        return "Не установлено";
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
