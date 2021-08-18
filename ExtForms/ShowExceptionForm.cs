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
using AgeyevAV.IO;
using AgeyevAV.Logging;
using AgeyevAV.Shell;

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
        AbsPath Path = new AbsPath(_LogFilePath);
        EFPApp.ShowWindowsExplorer(Path.ParentDir);
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
        FileAssociations FAs = FileAssociations.FromFileExtension(".txt");

        #region Открыть с помощью

        if (FAs.OpenWithItems.Count > 0)
        {
          foreach (FileAssociationItem FA in FAs.OpenWithItems)
          {
            ToolStripMenuItem FAMenuItem = new ToolStripMenuItem();
            FAMenuItem.Text = FA.DisplayName;
            try
            {
              FAMenuItem.Image = WinFormsTools.ExtractIconImage(FA, true);
            }
            catch { }
            FAMenuItem.Tag = FA;
            FAMenuItem.Click += FAMenuItem_Click;

            OpenWithMenu.Items.Add(FAMenuItem);
          }

          OpenWithMenu.Items.Add("-");
        }

        // Команда для показа в окне
        ToolStripMenuItem ViewMenuItem = new ToolStripMenuItem();
        ViewMenuItem.Text = "Встроенный просмотр";
        ViewMenuItem.Image = btnEdit.Image;
        ViewMenuItem.Click += new EventHandler(ViewMenuItem_Click);
        OpenWithMenu.Items.Add(ViewMenuItem);

        ToolStripMenuItem CopyMenuItem = new ToolStripMenuItem();
        CopyMenuItem.Text = "Копировать";
        CopyMenuItem.Image = TheImageList.Images["Copy"];
        CopyMenuItem.Click += new EventHandler(CopyMenuItem_Click);
        OpenWithMenu.Items.Add(CopyMenuItem);

        btnOpenWith.Click += new EventHandler(btnOpenWith_Click);

        #endregion

        #region Основная кнопка "Отчет"

        if (FAs.OpenItem == null)
          btnEdit.Click += new EventHandler(ViewMenuItem_Click);
        else
        {
          try
          {
            btnEdit.Image = WinFormsTools.ExtractIconImage(FAs.OpenItem, true);
            btnEdit.Tag = FAs.OpenItem;
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
      FileAssociationItem FA;
      if (sender is Button)
        FA = (FileAssociationItem)(((Button)sender).Tag);
      else
        FA = (FileAssociationItem)(((ToolStripMenuItem)sender).Tag);
      if (FA == null)
        MessageBox.Show("Нет файловой ассоциации", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
      else
      {
        try
        {
          FA.Execute(new AbsPath(_LogFilePath));
        }
        catch (Exception e)
        {
          LogoutTools.LogoutException(e, "Ошибка запуска " + FA.DisplayName);
          MessageBox.Show(e.Message, "Ошибка запуска " + FA.DisplayName, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        string Text = System.IO.File.ReadAllText(_LogFilePath, LogoutTools.LogEncoding);
        EFPApp.ShowTextView(Text, _LogFilePath);
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
        string Text = System.IO.File.ReadAllText(_LogFilePath, LogoutTools.LogEncoding);
        Clipboard.SetText(Text);
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
        DataGridViewRow Row = grData.Rows[args.RowIndex];
        string Name = Row.Cells[0].Value.ToString();
        DebugTools.DebugObject(Row.Tag, Name);
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
        using (ShowExceptionForm Form1 = new ShowExceptionForm())
        {
          if (prevForm != null)
          {
            Form1.StartPosition = FormStartPosition.Manual;
            Point pos = prevForm.Location;
            pos.Offset(16, 16);
            Form1.Location = pos;
          }
          if (!String.IsNullOrEmpty(title))
            Form1.Text = title;
          Form1.panStopShow.Visible = (prevForm == null);

          Form1._LogFilePath = logFilePath.Path;
          if (logFilePath.IsEmpty)
            Form1.panLog.Visible = false;
          else
          {
            Form1.edLogPath.Text = logFilePath.Path;
            Form1.InitEditButtons();
          }

          Form1._ThisObj = e;

          Form1.edType.Text = e.GetType().ToString();
          Form1.edMessage.Text = e.Message;

          try
          {
            Form1.pg1.SelectedObject = e;
          }
          catch { }

          #region Стек вызовов

          try
          {
            if (String.IsNullOrEmpty(e.StackTrace))
              Form1.grStack.Rows.Add("Стек вызовов недоступен");
            else
            {
              string[] a = e.StackTrace.Split(DataTools.NewLineSeparators, StringSplitOptions.RemoveEmptyEntries);
              for (int i = 0; i < a.Length; i++)
                Form1.grStack.Rows.Add(a[i]);
            }
          }
          catch
          {
            Form1.grStack.Rows.Add("Не удалось получить стек вызовов");
          }

          #endregion

          #region Exception.Data

          try
          {
            if (e.Data != null)
            {
              foreach (DictionaryEntry Entry in e.Data)
              {
                string Name = Entry.Key.ToString();
                object Value = Entry.Value;
                Form1.grData.RowCount++;
                DataGridViewRow Row = Form1.grData.Rows[Form1.grData.RowCount - 1];
                Row.Cells[0].Value = Name;

                if (Value != null)
                {
                  try
                  {
                    Row.Cells[1].Value = Value.ToString();
                    Row.Cells[2].ToolTipText = "Тип: " + Value.GetType().ToString();
                  }
                  catch (Exception e2)
                  {
                    Row.Cells[1].Value = e2.Message;
                  }
                }
                else
                {
                  Row.Cells[1].Value = "null";
                  Row.Cells[2].ToolTipText = "null";
                }
                Row.Tag = Value;
              }
            }
          }
          catch
          {
          }

          if (Form1.grData.RowCount == 0)
            Form1.TheTabControl.TabPages.Remove(Form1.tpData);

          #endregion

          #region Exception.InnerException

          try
          {
            Form1.cbInner.Enabled = e.InnerException != null;
          }
          catch
          {
            Form1.cbInner.Text = "Нет доступа к InnerException";
            Form1.cbInner.Enabled = false;
          }

          #endregion

          if (useDialogOwnerWindow && EFPApp.DialogOwnerWindow != null)
            Form1.ShowDialog(EFPApp.DialogOwnerWindow);
          else
            Form1.ShowDialog();

          if (Form1.cbStopShow.Checked)
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