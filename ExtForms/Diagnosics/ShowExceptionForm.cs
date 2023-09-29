// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using FreeLibSet.IO;
using FreeLibSet.Logging;
using FreeLibSet.Shell;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Diagnostics
{
  /// <summary>
  /// Отладка выброшенных исключений
  /// </summary>
  internal partial class ShowExceptionForm : Form
  {
    #region Конструктор

    public ShowExceptionForm()
    {
      InitializeComponent();
      try
      {
        EFPApp.InitShowInTaskBar(this);
      }
      catch
      {
      }

      btnDirExplorer.Visible = EFPApp.IsWindowsExplorerSupported; // 12.05.2016
      // пусть будет обычный значок
      //if (btnDirExplorer.Visible)
      //  btnDirExplorer.Image = WinFormsTools.ExtractIconImage(EFPApp.FileExtAssociations.ShowDirectory.OpenItem, true);
    }

    #endregion

    #region Объект исключения

    private Exception _ThisObj;

    private void cbInner_Click(object sender, EventArgs args)
    {
      ShowException(_ThisObj.InnerException, "Вложенная ошибка", this, AbsPath.Empty);
    }

    private void btnDirExplorer_Click(object sender, EventArgs args)
    {
      try
      {
        AbsPath path = new AbsPath(_LogFilePath);
        EFPApp.ShowWindowsExplorer(path.ParentDir);
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e, "Не удалось открыть каталог с отчетами");
        MessageBox.Show(e.Message, "Не удалось открыть каталог с отчетами",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    #endregion

    #region Просмотр отчета

    /// <summary>
    /// Путь к файлу отчета
    /// </summary>
    private string _LogFilePath;

    private void InitEditButtons()
    {
      try
      {
        FileAssociations faItems = FileAssociations.FromFileExtension(".txt");

        #region Открыть с помощью

        if (faItems.Count > 1)
        {
          for (int i=1; i<faItems.Count;i++)
          {
            FileAssociationItem faItem = faItems[i];
            ToolStripMenuItem faMenuItem = new ToolStripMenuItem();
            faMenuItem.Text = faItem.DisplayName;
            try
            {
              faMenuItem.Image = WinFormsTools.ExtractIconImage(faItem, true);
            }
            catch { }
            faMenuItem.Tag = faItem;
            faMenuItem.Click += FAMenuItem_Click;

            OpenWithMenu.Items.Add(faMenuItem);
          }

          OpenWithMenu.Items.Add("-");
        }

        // Команда для показа в окне
        ToolStripMenuItem viewMenuItem = new ToolStripMenuItem();
        viewMenuItem.Text = "Встроенный просмотр";
        viewMenuItem.Image = btnEdit.Image;
        viewMenuItem.Click += new EventHandler(ViewMenuItem_Click);
        OpenWithMenu.Items.Add(viewMenuItem);

        ToolStripMenuItem copyMenuItem = new ToolStripMenuItem();
        copyMenuItem.Text = "Копировать";
        copyMenuItem.Image = MainImagesResource.Copy;
        copyMenuItem.Click += new EventHandler(CopyMenuItem_Click);
        OpenWithMenu.Items.Add(copyMenuItem);

        btnOpenWith.Click += new EventHandler(btnOpenWith_Click);

        #endregion

        #region Основная кнопка "Отчет"

        if (faItems.FirstItem == null)
          btnEdit.Click += new EventHandler(ViewMenuItem_Click);
        else
        {
          try
          {
            btnEdit.Image = WinFormsTools.ExtractIconImage(faItems.FirstItem, true);
            btnEdit.Tag = faItems.FirstItem;
            btnEdit.Click += new EventHandler(FAMenuItem_Click);
          }
          catch { }
        }

        #endregion

      }
      catch { }
    }

    /// <summary>
    /// Запуск ассоциации файла для кнопки или команды меню
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void FAMenuItem_Click(object sender, EventArgs args)
    {
      FileAssociationItem faItem;
      if (sender is Button)
        faItem = (FileAssociationItem)(((Button)sender).Tag);
      else
        faItem = (FileAssociationItem)(((ToolStripMenuItem)sender).Tag);
      if (faItem == null)
        MessageBox.Show("Нет файловой ассоциации", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
      else
      {
        try
        {
          faItem.Execute(new AbsPath(_LogFilePath));
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "Ошибка запуска " + faItem.DisplayName);
          MessageBox.Show(e.Message, "Ошибка запуска " + faItem.DisplayName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    /// <summary>
    /// Открытие отчета во встроенном просмотре
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void ViewMenuItem_Click(object sender, EventArgs args)
    {
      try
      {
        string text = System.IO.File.ReadAllText(_LogFilePath, LogoutTools.LogEncoding);
        EFPApp.ShowTextView(text, _LogFilePath);
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e, "Не удалось открыть файл отчета на просмотр");
        MessageBox.Show(e.Message, "Не удалось открыть файл отчета на просмотр",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    /*
    private void btnEdit_Click(object Sender, EventArgs Args)
    {
      try
      {
        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Win32NT:
          case PlatformID.Win32Windows:
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "notepad.exe";
            psi.Arguments = LogFilePath;
            Process.Start(psi);
            break;
          default: // 12.05.2016
            string Text = System.IO.File.ReadAllText(LogFilePath, LogoutTools.LogEncoding);
            EFPApp.ShowTextView(Text, LogFilePath);
            break;
        }
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e, "Не удалось открыть файл отчета на просмотр");
        MessageBox.Show(e.Message, "Не удалось открыть файл отчета на просмотр",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
      */

    void CopyMenuItem_Click(object sender, EventArgs args)
    {
      try
      {
        string text = System.IO.File.ReadAllText(_LogFilePath, LogoutTools.LogEncoding);
        Clipboard.SetText(text);
      }
      catch (Exception e)
      {
        LogoutTools.LogoutException(e, "Ошибка копирования отчета в буфер обмена");
        MessageBox.Show(e.Message, "Ошибка копирования отчета в буфер обмена",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void btnOpenWith_Click(object sender, EventArgs args)
    {
      btnOpenWith.ContextMenuStrip.Show(btnOpenWith, new Point(0, btnOpenWith.Height));
    }

    #endregion

    #region Табличный просмотр

    private void grData_CellContentClick(object sender, DataGridViewCellEventArgs args)
    {
      if (args.ColumnIndex != 2)
        return;
      try
      {
        DataGridViewRow row = grData.Rows[args.RowIndex];
        string name = row.Cells[0].Value.ToString();
        DebugTools.DebugObject(row.Tag, name);
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Ошибка просмотра объекта данных",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    #endregion

    #region Вывод окна

    public static void ShowException(Exception e, string title, ShowExceptionForm prevForm, AbsPath logFilePath)
    {
      if (!DebugTools.ShowExceptionEnabled)
        return; // возможен повторный вызов, если произошел вложенный вызов ShowException(),
      // и для первого окна пользователь установил флажок
      if (e == null)
      {
        MessageBox.Show("Объект исключения не задан", "ShowExceptionForm.ShowException()");
        return;
      }

      // 12.03.2018, 24.03.2016
      // Выполняем две попытки вывода окна ошибки: используя EFPApp.DialogOwnerWindow и без владельца
      // 14.11.2018
      // Создаем новое окно, если первая попытка не удалась
      try
      {
        DoShowException(e, title, prevForm, logFilePath, true);
      }
      //catch (Exception e2) // 12.03.2018
      //{
      //  if (EFPApp.CanRepeatShowDialogAfterError(e2))
      //    DoShowException(e, Title, PrevForm, LogFilePath, false);
      //  else
      //    throw;
      //}
      catch // 30.06.2019 - упрощаем проверку. Косяк может быть и не с внешним окном, а с самым обычным.
      {
        if (EFPApp.DialogOwnerWindow != null)
          DoShowException(e, title, prevForm, logFilePath, false);
        else
          throw;
      }
    }

    private static void DoShowException(Exception e, string title, ShowExceptionForm prevForm, AbsPath logFilePath, bool useDialogOwnerWindow)
    {
      EFPApp.SuspendIdle(); // 18.08.2021
      try
      {
        using (ShowExceptionForm frm1 = new ShowExceptionForm())
        {
          if (prevForm != null)
          {
            frm1.StartPosition = FormStartPosition.Manual;
            Point pos = prevForm.Location;
            pos.Offset(16, 16);
            frm1.Location = pos;
          }
          if (!String.IsNullOrEmpty(title))
            frm1.Text = title;
          frm1.panStopShow.Visible = (prevForm == null);

          frm1._LogFilePath = logFilePath.Path;
          if (logFilePath.IsEmpty)
            frm1.panLog.Visible = false;
          else
          {
            frm1.edLogPath.Text = logFilePath.Path;
            frm1.InitEditButtons();
          }

          frm1._ThisObj = e;

          frm1.edType.Text = e.GetType().ToString();
          frm1.edMessage.Text = e.Message;

          try
          {
            frm1.pg1.SelectedObject = e;
          }
          catch { }

          #region Стек вызовов

          try
          {
            if (String.IsNullOrEmpty(e.StackTrace))
              frm1.grStack.Rows.Add("Стек вызовов недоступен");
            else
            {
              string[] a = e.StackTrace.Split(DataTools.NewLineSeparators, StringSplitOptions.RemoveEmptyEntries);
              for (int i = 0; i < a.Length; i++)
                frm1.grStack.Rows.Add(a[i]);
            }
          }
          catch
          {
            frm1.grStack.Rows.Add("Не удалось получить стек вызовов");
          }

          #endregion

          #region Exception.Data

          try
          {
            if (e.Data != null)
            {
              foreach (DictionaryEntry pair in e.Data)
              {
                string name = pair.Key.ToString();
                object value = pair.Value;
                frm1.grData.RowCount++;
                DataGridViewRow row = frm1.grData.Rows[frm1.grData.RowCount - 1];
                row.Cells[0].Value = name;

                if (value != null)
                {
                  try
                  {
                    row.Cells[1].Value = value.ToString();
                    row.Cells[2].ToolTipText = "Тип: " + value.GetType().ToString();
                  }
                  catch (Exception e2)
                  {
                    row.Cells[1].Value = e2.Message;
                  }
                }
                else
                {
                  row.Cells[1].Value = "null";
                  row.Cells[2].ToolTipText = "null";
                }
                row.Tag = value;
              }
            }
          }
          catch
          {
          }

          if (frm1.grData.RowCount == 0)
            frm1.TheTabControl.TabPages.Remove(frm1.tpData);

          #endregion

          #region Exception.InnerException

          try
          {
            frm1.cbInner.Enabled = e.InnerException != null;
          }
          catch
          {
            frm1.cbInner.Text = "Нет доступа к InnerException";
            frm1.cbInner.Enabled = false;
          }

          #endregion

          if (useDialogOwnerWindow && EFPApp.DialogOwnerWindow != null)
            frm1.ShowDialog(EFPApp.DialogOwnerWindow);
          else
            frm1.ShowDialog();

          if (frm1.cbStopShow.Checked)
          {
            DebugTools.ShowExceptionEnabled = false; // 31.01.2020
            MessageBox.Show("Вывод сообщений об ошибках отключен. Завершите работу программы как можно быстрее", "Отключение вывода сообщений", MessageBoxButtons.OK, MessageBoxIcon.Stop);
          }
        }
      }
      finally
      {
        EFPApp.ResumeIdle();
      }
    }

    #endregion
  }
}
