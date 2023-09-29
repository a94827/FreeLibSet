using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.UICore;

namespace EFPCommandItemsDemo
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();

      Program.LastFormNumber++;
      _FormNumber = Program.LastFormNumber;

      Text = "Форма №" + _FormNumber.ToString();
      Icon = EFPApp.MainImages.Icons["CircleBlack"];

      EFPFormProvider efpForm = new EFPFormProvider(this);
      EFPCommandItem ci;
      ci = new EFPCommandItem("Test", "Item1");
      ci.MenuText = "Команда на уровне формы";
      ci.ImageKey = "CircleBlack";
      ci.ShortCut = Keys.F11;
      ci.StatusBarText = "Ф.№" + _FormNumber.ToString();
      ci.Click += new EventHandler(MenuItem_Click);
      efpForm.CommandItems.Add(ci);

      EFPTabControl efpTabControl = new EFPTabControl(efpForm, TheTabControl);

      ci = new EFPCommandItem("Test", "Item21");
      ci.MenuText = "Команда для вкладки 1";
      ci.ImageKey = "CircleBlue";
      ci.ShortCut = Keys.F12;
      ci.StatusBarText = "Вкладка 1";
      ci.Click += new EventHandler(MenuItem_Click);
      efpTabControl.TabPages[0].CommandItems.Add(ci);

      ci = new EFPCommandItem("Test", "Item22");
      ci.MenuText = "Команда для вкладки 2";
      ci.ImageKey = "CircleGreen";
      ci.ShortCut = Keys.F12;
      ci.StatusBarText = "Вкладка 2";
      ci.Click += new EventHandler(MenuItem_Click);
      efpTabControl.TabPages[1].CommandItems.Add(ci);

      UIInputGridData data1 = new UIInputGridData();
      data1.Table.Columns.Add("Col1", typeof(string));
      data1.Columns[0].FillWeight = 100;

      EFPInputDataGridView efpGr1 = new EFPInputDataGridView(efpTabControl.TabPages[0].BaseProvider, grid1);
      efpGr1.ToolBarPanel = panSpb1;
      efpGr1.Data = data1;

      ci = new EFPCommandItem("Test", "Item31");
      ci.MenuText = "Команда для табличного просмотра";
      ci.Click += new EventHandler(MenuItem_Click);
      ci.ImageKey = "CircleRed";
      ci.ShortCut = Keys.F8;
      ci.StatusBarText = "Таблица";
      efpGr1.CommandItems.Add(ci);

      EFPTextBox efpText2 = new EFPTextBox(efpTabControl.TabPages[1].BaseProvider, tb2);
      efpText2.CanBeEmpty = true;
      efpText2.ToolBarPanel = panSpb2;


      ci = new EFPCommandItem("Test", "Item32");
      ci.MenuText = "Команда для редактора";
      ci.Click += new EventHandler(MenuItem_Click);
      ci.ImageKey = "CircleYellow";
      ci.ShortCut = Keys.F8;
      ci.StatusBarText = "Редактор";
      efpText2.CommandItems.Add(ci);
    }

    int _FormNumber;

    void MenuItem_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      EFPApp.MessageBox("Выполнена команда: " + ci.MenuText, this.Text);
    }
  }
}
