using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.Forms.Diagnostics;
using FreeLibSet.Core;

namespace EFPCommandItemsDemo
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      EFPApp.InitApp();
      try
      {
        StartParamsForm paramForm = new StartParamsForm();
        if (EFPApp.ShowDialog(paramForm, true) != DialogResult.OK)
          return;

        if (paramForm.efpDebugWindow.Checked)
          DebugTools.DebugFocusWindowVisible = true;

        InitMainMenu();


        EFPApp.AvailableInterfaces = new EFPAppInterface[] { new EFPAppInterfaceMDI(), new EFPAppInterfaceSDI() };
        EFPApp.SetInterface(paramForm.efpUIType.SelectedIndex == 0 ? "MDI" : "SDI", typeof(Form1));

        Application.Run();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка запуска программы");
      }
    }

    private static void InitMainMenu()
    {
      EFPAppCommandItemHelpers h = new EFPAppCommandItemHelpers();

      EFPCommandItem menuFile = h.CommandItems.Add(EFPAppStdCommandItems.MenuFile);
      EFPCommandItem ciNew = h.CommandItems.Add(EFPAppStdCommandItems.New, menuFile);
      ciNew.Click += new EventHandler(ciNew_Click);

      EFPCommandItem ciWizard = new EFPCommandItem("File", "WizardTest");
      ciWizard.MenuText = "Тест запуска Wizard";
      ciWizard.ImageKey = "ArrowRight";
      ciWizard.ShortCut = Keys.F4;
      ciWizard.Click += new EventHandler(ciWizard_Click);
      ciWizard.Parent = menuFile;
      h.CommandItems.Add(ciWizard);

      h.AddExit(menuFile);

      EFPCommandItem menuEdit = h.CommandItems.Add(EFPAppStdCommandItems.MenuEdit);
      h.CommandItems.Add(EFPAppStdCommandItems.Cut, menuEdit).Enabled = false;
      h.CommandItems.Add(EFPAppStdCommandItems.Copy, menuEdit).Enabled = false;
      h.CommandItems.Add(EFPAppStdCommandItems.Paste, menuEdit).Enabled = false;

      EFPCommandItem menuWindow = h.CommandItems.Add(EFPAppStdCommandItems.MenuWindow);
      h.AddWindowMenuCommands(menuWindow);

      EFPCommandItem menuHelp = h.CommandItems.Add(EFPAppStdCommandItems.MenuHelp);
      h.AddAbout(menuHelp);

      EFPAppToolBarCommandItems tb = new EFPAppToolBarCommandItems("Main");
      tb.AddRange(h.CommandItems);
      EFPApp.ToolBars.Add(tb);
    }

    static void ciNew_Click(object sender, EventArgs args)
    {
      NewFormParamForm dlg = new NewFormParamForm();
      if (EFPApp.ShowDialog(dlg, true) == DialogResult.OK)
      {
        Form frm;
        switch (dlg.efpFormType.SelectedIndex)
        {
          case 0: frm = new Form1(); break;
          case 1: frm = new Form2(); break;
          case 2: frm = new Form3(); break;
          default:
            throw new BugException();
        }

        if (dlg.efpMode.SelectedIndex == 0)
          EFPApp.ShowChildForm(frm);
        else
          EFPApp.ShowDialog(frm, true);
      }

    }

    public static int LastFormNumber = 0;

    static void ciWizard_Click(object sender, EventArgs args)
    {
      new WizardTest().Test();
    }
  }
}