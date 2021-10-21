using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using FreeLibSet.Logging;
using FreeLibSet.Core;

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

namespace FreeLibSet.Forms
{
  internal partial class SimpleAboutDialogForm : Form
  {
    #region Конструктор

    public SimpleAboutDialogForm()
    {
      InitializeComponent();

      Icon = EFPApp.MainImageIcon("About");

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
          frm.Icon = EFPApp.MainImageIcon("About");
          EFPApp.SetFormSize(frm, 50, 50);
          frm.StartPosition = FormStartPosition.CenterScreen;
          frm.WindowState = FormWindowState.Maximized;
          EFPFormProvider efpForm = new EFPFormProvider(frm);

          TabControl TheTabControl = new TabControl();
          TheTabControl.Dock = DockStyle.Fill;
          frm.Controls.Add(TheTabControl);

          TabPage tpMain = new TabPage("Основные модули");
          TheTabControl.TabPages.Add(tpMain);
          EFPDataGridView ghMain = InitModulesPage(efpForm, tpMain);

          TabPage tpGAC = new TabPage("Из глобального кэша сборок");
          TheTabControl.TabPages.Add(tpGAC);
          EFPDataGridView ghGAC = InitModulesPage(efpForm, tpGAC);

          DataTable Table = FreeLibSet.Forms.Diagnostics.DebugTools.GetAssembliesInfo(true);

          DataView dvMain = new DataView(Table);
          dvMain.RowFilter = "GAC=FALSE";
          ghMain.Control.DataSource = dvMain;

          DataView dvGAC = new DataView(Table);
          dvGAC.RowFilter = "GAC=TRUE";
          ghGAC.Control.DataSource = dvGAC;
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

    private static EFPDataGridView InitModulesPage(EFPBaseProvider baseProvider, TabPage tp)
    {
      DataGridView Grid = new DataGridView();
      Grid.Dock = DockStyle.Fill;
      tp.Controls.Add(Grid);
      Grid.ReadOnly = true;
      Grid.AllowUserToAddRows = false;
      Grid.AllowUserToDeleteRows = false;
      Grid.AutoGenerateColumns = false;
      EFPDataGridView Handler = new EFPDataGridView(baseProvider, Grid);
      Handler.Columns.AddText("Name", true, "Имя сборки", 20);
      Handler.Columns.LastAdded.CanIncSearch = true;
      Handler.Columns.AddText("Version", true, "Версия", 12);
      Handler.Columns.AddDateTime("CreationTime", true, "Создан");
      Handler.Columns.AddBool("Debug", true, "Отладка");
      Handler.Columns.LastAdded.GridColumn.ToolTipText = "Флажок установлен, если сборка скомпилирована в отладочном режиме." +Environment.NewLine+
        "Если флажка нет, сборка скомпилирована с оптимизацией кода";
      Handler.Columns.AddText("Description", true, "Описание", 40);
      Handler.Columns.AddText("Copyright", true, "Авторские права", 40);
      Handler.Columns.AddText("ProcessorArchitecture", true, "Архитектура", 7);
      Handler.Columns.LastAdded.GridColumn.ToolTipText = "Поддерживаемая архитектура процессора";
      Handler.Columns.AddText("Location", true, "Путь к сборке", 40);
      Handler.DisableOrdering();
      Handler.FrozenColumns = 1;
      Handler.ReadOnly = true;
      Handler.CanView = false;

      return Handler;
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
    /// Свойства заполняются из основной сборки приложения
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
    /// Значок все равно берется из основной сборки приложения
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
        string ProgName = assembly.FullName;
        AssemblyName an = new AssemblyName(ProgName);

        AppName = GetAppName(assembly);
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

    private static string GetAppName(Assembly assembly)
    {
      AssemblyDescriptionAttribute attrDescr = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
      if (attrDescr != null)
      {
        if (!String.IsNullOrEmpty(attrDescr.Description))
          return attrDescr.Description;
      }

      AssemblyProductAttribute attrProduct = (AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));
      if (attrProduct != null)
      {
        if (!String.IsNullOrEmpty(attrProduct.Product))
          return attrProduct.Product;
      }

      // если ничего не нашли
      return assembly.ToString();
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
    /// Выводится в верхней части диалога
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
      using (SimpleAboutDialogForm Form = new SimpleAboutDialogForm())
      {
        if (AppIcon != null)
        {
          try
          {
            Bitmap bmp = AppIcon.ToBitmap();
            bmp.MakeTransparent(Color.FromArgb(13, 11, 12)); // см. справку к Icon.ToBitmap()
            Form.pbIcon.Image = bmp;
          }
          catch { } // проглатываем ошибку
        }
        Form.Text = Title;

        Form.lblTitle.Text = AppName;
        if (!String.IsNullOrEmpty(Version))
          Form.lblVersion.Text = "Версия " + Version;
        Form.lblCopyright.Text = Copyright;

        return EFPApp.ShowDialog(Form, false);
      }
    }

    #endregion
  }
}