using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;

namespace TestEFPTabControl
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
      EFPFormProvider efpForm = new EFPFormProvider(this);

      EFPCommandItem ci;
      ci = new EFPCommandItem("Тест", "Т0");
      ci.MenuText = "Команда Form";
      ci.ImageKey = "CircleGreen";
      ci.Click += new EventHandler(ciTest_Click);
      efpForm.CommandItems.Add(ci);

      // Общедоступные команды должны добавляться на уровне Form, а не TabControl

      ci = new EFPCommandItem("View", "Add");
      ci.MenuText = "Добавить вкладку";
      ci.ImageKey = "Insert";
      ci.Click += new EventHandler(ciAdd_Click);
      efpForm.CommandItems.Add(ci);

      ci = new EFPCommandItem("View", "AddHidden");
      ci.MenuText = "Добавить скрытую вкладку";
      ci.Click += new EventHandler(ciAddHidden_Click);
      efpForm.CommandItems.Add(ci);

      ci = new EFPCommandItem("View", "CloseAll");
      ci.MenuText = "Закрыть все вкладки";
      ci.ImageKey = "CloseAll";
      ci.Click += new EventHandler(ciCloseAll_Click);
      efpForm.CommandItems.Add(ci);

      ci = new EFPCommandItem("View", "ShowOrHide");
      ci.MenuText = "Скрыть/показать ...";
      ci.Click += new EventHandler(ciShowOrHide_Click);
      efpForm.CommandItems.Add(ci);

      efpTC = new EFPTabControl(efpForm, TheTC);
      ci = new EFPCommandItem("Тест", "Т1");
      ci.MenuText = "Команда TabControl";
      ci.ImageKey = "CircleBlue";
      ci.Click += new EventHandler(ciTest_Click);
      efpTC.CommandItems.Add(ci);

      for (int i = 1; i <= 5; i++)
        CreateTab(true);
    }

    void ciAdd_Click(object sender, EventArgs args)
    {
      CreateTab(true);
    }

    void ciAddHidden_Click(object sender, EventArgs args)
    {
      CreateTab(false);
    }

    void ciShowOrHide_Click(object sender, EventArgs args)
    {
      ListSelectDialog dlg = new ListSelectDialog();
      dlg.Title = "Видимость вкладок";
      dlg.ListTitle = "Вкладки";
      dlg.Items = new string[efpTC.TabPages.Count];
      dlg.MultiSelect = true;
      dlg.CanBeEmpty = true;
      for (int i = 0; i < efpTC.TabPages.Count; i++)
      {
        dlg.Items[i] = efpTC.TabPages[i].Text;
        dlg.Selections[i] = efpTC.TabPages[i].Visible;
      }
      if (efpTC.SelectedIndex >= 0)
        dlg.SelectedIndex = efpTC.SelectedIndex;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;

      for (int i = 0; i < efpTC.TabPages.Count; i++)
      {
        dlg.Items[i] = efpTC.TabPages[i].Text;
        efpTC.TabPages[i].Visible = dlg.Selections[i];
      }
    }


    private EFPTabControl efpTC;

    #region Создание вкладки

    private int TabCounter = 0;

    private void CreateTab(bool visible)
    {
      TabCounter++;

      EFPCommandItem ci;

      EFPTabPage efpTP = new EFPTabPage(TabCounter.ToString());
      efpTP.Visible = visible;

      ci = new EFPCommandItem("Тест", "Т2");
      ci.MenuText = "Команда TabPage " + efpTP.Text;
      ci.ImageKey = "CircleYellow";
      ci.Click += new EventHandler(ciTest_Click);
      efpTP.CommandItems.Add(ci);

      ci = new EFPCommandItem("View", "Close");
      ci.MenuText = "Закрыть вкладку";
      ci.ImageKey = "Delete";
      ci.Tag = efpTP;
      ci.Click += new EventHandler(ciCloseTab_Click);
      efpTP.CommandItems.Add(ci);

      ci = new EFPCommandItem("View", "CloseAllButThis");
      ci.MenuText = "Закрыть все вкладки, кроме текущей";
      ci.ImageKey = "CloseAllButThis";
      ci.Tag = efpTP;
      ci.Click += new EventHandler(ciCloseAllButThis_Click);
      efpTP.CommandItems.Add(ci);

      #region Текстовое поле

      Label lbl = new Label();
      lbl.Text = "Поле ввода " + efpTP.Text;
      lbl.Location = new Point(10, 10);
      lbl.AutoSize = true;
      efpTP.Control.Controls.Add(lbl);


      TextBox ed = new TextBox();
      ed.Location = new Point(120, 10);
      ed.Width = 150;
      efpTP.Control.Controls.Add(ed);
      EFPTextBox efpEd = new EFPTextBox(efpTP.BaseProvider, ed);
      efpEd.CanBeEmpty = false;

      #endregion

      efpTC.TabPages.Add(efpTP);
    }

    #endregion

    #region Обработчики команд меню

    void ciCloseTab_Click(object Sender, EventArgs Args)
    {
      EFPCommandItem ci = (EFPCommandItem)Sender;
      EFPTabPage efpTP = (EFPTabPage)(ci.Tag);
      TheTC.TabPages.Remove(efpTP.Control);
    }

    void ciCloseAllButThis_Click(object Sender, EventArgs Args)
    {
      EFPCommandItem ci = (EFPCommandItem)Sender;
      EFPTabPage efpTP = (EFPTabPage)(ci.Tag);
      for (int i = TheTC.TabPages.Count - 1; i >= 0; i--)
      {
        if (TheTC.TabPages[i] == efpTP.Control)
          continue;
        TheTC.TabPages.RemoveAt(i);
      }
    }

    void ciCloseAll_Click(object Sender, EventArgs Args)
    {
      for (int i = TheTC.TabPages.Count - 1; i >= 0; i--)
        TheTC.TabPages.RemoveAt(i);
    }

    void ciTest_Click(object Sender, EventArgs Args)
    {
      EFPCommandItem ci = (EFPCommandItem)Sender;
      EFPApp.MessageBox("Команда \"" + ci.MenuTextWithoutMnemonic + "\"");
    }

    #endregion
  }
}
