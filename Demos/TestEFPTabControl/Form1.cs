using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;

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
      ci.Click += new EventHandler(ciTest_Click);
      efpForm.CommandItems.Add(ci);

      efpTC = new EFPTabControl(efpForm, TheTC);
      ci = new EFPCommandItem("Тест", "Т1");
      ci.MenuText = "Команда TabControl";
      ci.Click += new EventHandler(ciTest_Click);
      efpTC.CommandItems.Add(ci);


      ci = new EFPCommandItem("View", "CloseAll");
      ci.MenuText = "Закрыть все вкладки";
      ci.Click += new EventHandler(ciCloseAll_Click);
      efpTC.CommandItems.Add(ci);


      for (int i = 1; i <= 5; i++)
        CreateTab();

      EFPButton efpAdd = new EFPButton(efpForm, btnAdd);
      efpAdd.Click += new EventHandler(efpAdd_Click);
    }

    void efpAdd_Click(object sender, EventArgs e)
    {
      CreateTab();
    }

    private EFPTabControl efpTC;

    #region Создание вкладки

    private int TabCounter = 0;

    private void CreateTab()
    {
      TabCounter++;

      EFPCommandItem ci;

      EFPTabPage efpTP = efpTC.TabPages.Add(TabCounter.ToString());
      ci = new EFPCommandItem("Тест", "Т2");
      ci.MenuText = "Команда TabPage " + efpTP.Text;
      ci.Click += new EventHandler(ciTest_Click);
      efpTP.CommandItems.Add(ci);

      ci = new EFPCommandItem("View", "Close");
      ci.MenuText = "Закрыть вкладку";
      ci.Tag = efpTP;
      ci.Click += new EventHandler(ciCloseTab_Click);
      efpTP.CommandItems.Add(ci);

      ci = new EFPCommandItem("View", "CloseAllButThis");
      ci.MenuText = "Закрыть все вкладки, кроме текущей";
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
    }

    #endregion

    #region Обработчики команд меню

    void ciCloseTab_Click(object Sender, EventArgs Args)
    {
      EFPCommandItem ci=(EFPCommandItem )Sender;
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