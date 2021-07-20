using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AgeyevAV.ExtForms;

namespace EFPAppRemoteExitDemo
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
        EFPApp.ShowMainWindow();
        EFPApp.MainWindowTitle = "������";


      #region ������� �������� ����

      EFPCommandItem MenuFile = new EFPCommandItem("Menu", "File");
      MenuFile.MenuText = "����";
      EFPApp.CommandItems.Add(MenuFile);

      EFPCommandItem ciNew = EFPApp.CommandItems.CreateCommand(EFPAppStdCommandItems.New);
      ciNew.Click += new EventHandler(ciNew_Click);
      ciNew.Parent = MenuFile;
      EFPApp.CommandItems.Add(ciNew);

      // ������� ������� ������
      EFPCommandItem ciExit = EFPApp.CommandItems.CreateCommand(EFPAppStdCommandItems.Exit);
      ciExit.Parent = MenuFile;
      ciExit.GroupBegin = true;
      ciExit.Click += new EventHandler(ciExit_Click);
      EFPApp.CommandItems.Add(ciExit);

      EFPMainMenu MainMenu = new EFPMainMenu();
      MainMenu.Add(EFPApp.CommandItems);
      MainMenu.Attach();

      
      #endregion


      // ���������� ��������� �����.
        // � ��������� ����������, �������, ����� ��������� � ������ ��������, � ������ ����������� ����� ��������� ������
        ServerForm ServerForm = new ServerForm();
        ServerForm.Show();


        // ������� ���������� "�������"
        new ClientHandler(ServerForm);

        Application.Run();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "������ ������� ���������");
      }
    }

    #region ������� �������� ����

    static void ciNew_Click(object sender, EventArgs e)
    {
      EFPApp.ShowMdiChild(new MdiChildForm());
    }

    static void ciExit_Click(object sender, EventArgs e)
    {
      EFPApp.Exit(); // ������� �����
    }


    #endregion
  }
}