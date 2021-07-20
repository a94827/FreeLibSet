using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using AgeyevAV.Remoting;
using AgeyevAV;
using System.Reflection;

namespace TestExecProc
{
  public partial class MainForm : Form
  {
    #region Конструктор формы

    public MainForm()
    {
      Console.SetBufferSize(80, 1000);
      LocalProcCreator = new ProcCreator();
      AppDomain TheDomain = AppDomain.CreateDomain("Second Domain");
      RemoteProcCreator = TheDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, "TestExecProc.ProcCreator") as ProcCreator;

      InitializeComponent();
      efpForm = new EFPFormProvider(this);

      efpProcType = new EFPListComboBox(efpForm, cbProcType);
      efpProcType.SelectedIndex = 0;

      efpStartMode = new EFPListComboBox(efpForm, cbStartMode);
      efpStartMode.SelectedIndex = 0;

      efpFromThread = new EFPListComboBox(efpForm, cbFromThread);
      efpFromThread.SelectedIndex = 0;
      efpFromThread.Validating += efpFromThread_Validating;
      efpStartMode.SelectedIndexEx.ValueChanged += efpFromThread.Validate;

      efpExecutionPlace = new EFPListComboBox(efpForm, cbExecutionPlace);
      efpExecutionPlace.SelectedIndex = 0;

      efpCopies = new EFPExtNumericUpDown(efpForm, edCopies);

      efpSyncTime = new EFPExtNumericUpDown(efpForm, edSyncTime);
      efpSyncTime.IntValue = ExecProc.DefaultSyncTime;

      btnStart.Image = EFPApp.MainImages.Images["Ok"];
      btnStart.ImageAlign = ContentAlignment.MiddleLeft;
      efpStart = new EFPButton(efpForm, btnStart);
      efpStart.Click += new EventHandler(efpStart_Click);

      btnInfo.Image = EFPApp.MainImages.Images["Information"];
      btnInfo.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpInfo = new EFPButton(efpForm, btnInfo);
      efpInfo.Click += new EventHandler(efpInfo_Click);

      EFPButton efpGCCollect = new EFPButton(efpForm, btnGCCollect);
      efpGCCollect.Click += new EventHandler(efpGCCollect_Click);

      btnDebug.Image = EFPApp.MainImages.Images["Debug"];
      btnDebug.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpDebug = new EFPButton(efpForm, btnDebug);
      efpDebug.Click += new EventHandler(efpDebug_Click);
    }

    void efpFromThread_Validating(object sender, EFPValidatingEventArgs args)
    {
      if (args.ValidateState == EFPValidateState.Error)
        return;

      TestStartMode StartMode = (TestStartMode)(efpStartMode.SelectedIndex);
      TestFromThread FromThread = (TestFromThread)(efpFromThread.SelectedIndex);
      if (FromThread != TestFromThread.MainThread)
      {
        switch (StartMode)
        { 
          case TestStartMode.ExecProcCallListExecuteSync:
          case TestStartMode.ExecProcCallListExecuteAsync:
          case TestStartMode.ExecProcCallListExecuteAsyncAndWait:
          case TestStartMode.DistributedProcCallItemAsync:
          case TestStartMode.DistributedProcCallItemAsyncAndWait:
            args.SetError("Свойство EFPApp.ExecProcCallList доступно только из основного потока приложения");
            break;
        }
      }
    }


    #endregion

    #region Поля

    /// <summary>
    /// Создатель процедур в основном домене приложения
    /// </summary>
    ProcCreator LocalProcCreator;

    /// <summary>
    /// Создатель удаленных процедур в отдельном домене
    /// </summary>
    ProcCreator RemoteProcCreator;

    EFPFormProvider efpForm;

    EFPListComboBox efpProcType;

    EFPListComboBox efpStartMode;

    EFPListComboBox efpFromThread;

    EFPListComboBox efpExecutionPlace;

    EFPExtNumericUpDown efpCopies;

    EFPExtNumericUpDown efpSyncTime;

    EFPButton efpStart;

    #endregion

    #region Запуск процедур

    void efpStart_Click(object Sender, EventArgs Args)
    {
      if (!efpForm.ValidateForm())
        return;

      Console.Clear();

      Test test = new Test();
      test.LocalProcCreator = LocalProcCreator;
      test.RemoteProcCreator = RemoteProcCreator;

      test.ProcType = (TestProcType)(cbProcType.SelectedIndex);
      test.StartMode = (TestStartMode)(cbStartMode.SelectedIndex);
      test.FromThread = (TestFromThread)(cbFromThread.SelectedIndex);
      test.ExecutionPlace = (TestExecutionPlace)(cbExecutionPlace.SelectedIndex);
      test.Copies = efpCopies.IntValue;
      test.SyncTime = efpSyncTime.IntValue;
      test.Run();
    }

    #endregion

    #region Прочее

    void efpInfo_Click(object Sender, EventArgs Args)
    {
      System.Diagnostics.Trace.WriteLine("=================== " + DateTime.Now.ToString("G") + " ==================");
      LocalProcCreator.TraceInfo();
      RemoteProcCreator.TraceInfo();
    }

    void efpGCCollect_Click(object Sender, EventArgs Args)
    {
      GC.Collect();
    }

    void efpDebug_Click(object Sender, EventArgs Args)
    {
      DebugTools.ShowDebugInfo("Debug info");
    }

    #endregion
  }
}