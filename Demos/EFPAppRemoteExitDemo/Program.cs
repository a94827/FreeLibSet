using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FreeLibSet.Forms;

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
        EFPApp.MainWindowTitle = "Клиент";


      #region Команды главного меню

      EFPCommandItem MenuFile = new EFPCommandItem("Menu", "File");
      MenuFile.MenuText = "Файл";
      EFPApp.CommandItems.Add(MenuFile);

      EFPCommandItem ciNew = EFPApp.CommandItems.Add(EFPAppStdCommandItems.New, MenuFile);
      ciNew.Click += new EventHandler(ciNew_Click);

      // Обычная команда выхода
      EFPCommandItem ciExit = EFPApp.CommandItems.Add(EFPAppStdCommandItems.Exit, MenuFile);
      ciExit.GroupBegin = true;
      ciExit.Click += new EventHandler(ciExit_Click);

      EFPMainMenu MainMenu = new EFPMainMenu();
      MainMenu.Add(EFPApp.CommandItems);
      MainMenu.Attach();

      
      #endregion


      // Изображаем серверную форму.
        // В настоящем приложении, конечно, форма находится в другом процессе, а запрос выполняется через удаленный доступ
        ServerForm ServerForm = new ServerForm();
        ServerForm.Show();


        // Создаем обработчик "клиента"
        new ClientHandler(ServerForm);

        Application.Run();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка запуска программы");
      }
    }

    #region Команды главного меню

    static void ciNew_Click(object sender, EventArgs e)
    {
      EFPApp.ShowChildForm(new MdiChildForm());
    }

    static void ciExit_Click(object sender, EventArgs e)
    {
      EFPApp.Exit(); // обычный выход
    }


    #endregion
  }
}
