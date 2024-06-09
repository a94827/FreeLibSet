// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Printing;
using System.ComponentModel;
using System.Windows.Forms;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Менеджер фоновой печати. Реализация свойства <see cref="EFPApp.BackgroundPrinting"/>.
  /// <list type="bullet">
  /// <item><description>1. При построении меню должна быть добавлена <see cref="EFPCommandItem"/> для статусной строки</description></item>
  /// <item><description>2. Должно быть установлено свойство <see cref="Enabled"/>=true</description></item>
  /// <item><description>3. Вместо обычной печати вызовом <see cref="PrintDocument.Print()"/> в основном потоке
  /// должен вызываться метод <see cref="Add(PrintDocument)"/></description></item>
  /// </list>
  /// </summary>
  public class BackgroundPrinting
  {
    #region Конструктор

    const string DefaultStatusToolTipText = "Окно состояния фоновой печати. Нет заданий";

    internal BackgroundPrinting()
    {
      _Enabled = false;
      _StatusItem = new EFPCommandItem("View", "BackgroundPrint");
      _StatusItem.ToolTipText = DefaultStatusToolTipText;
      _StatusItem.Usage = EFPCommandItemUsage.StatusBar;
      _StatusItem.ImageKey = "Print";
      _StatusItem.Enabled = false;
      _StatusItem.StatusBarText = EFPCommandItem.EmptyStatusBarText;
      _StatusItem.Visible = false;
      _StatusItem.Click += new EventHandler(StatusItem_Click);

      _TheQueue = new Queue<PrintDocument>();

      _Worker = new BackgroundWorker();
      _Worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
      _Worker.WorkerReportsProgress = true;
      _Worker.WorkerSupportsCancellation = true;
      _Worker.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);
      _Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// true, если фоновая печать разрешена.
    /// По умолчанию - false.
    /// </summary>
    public bool Enabled
    {
      get { return _Enabled; }
      set
      {
        if (_Enabled == value)
          return;
        if (!value)
        {
          if (!QueryCancelPrinting())
            return;
        }
        _Enabled = value;
        _StatusItem.Visible = value;
      }
    }
    private bool _Enabled;

    /// <summary>
    /// Элемент для статусной строки. Он должен быть добавлен при построении
    /// главного меню программы
    /// </summary>
    public EFPCommandItem StatusItem { get { return _StatusItem; } }
    private readonly EFPCommandItem _StatusItem;

    #endregion

    #region Прерывание печати

    void StatusItem_Click(object sender, EventArgs args)
    {
      QueryCancelPrinting();
    }

    /// <summary>
    /// Запрос на прекращение печати заданий в фоновом режиме, если они есть
    /// </summary>
    /// <returns></returns>
    public bool QueryCancelPrinting()
    {
      Splash spl = null;
      try
      {
        while (_Worker.IsBusy)
        {
          if (spl == null)
          {
            spl = new Splash("Фоновая печать документов");
            spl.AllowCancel = true;
          }
          lock (_TheQueue)
          {
            StringBuilder sb = new StringBuilder();
            if (_LastStatusInfo != null)
            {
              sb.Append("Фоновая печать документа \"" + _LastStatusInfo.DocumentName + "\", ");
              if (_LastStatusInfo.CurrentPage < 0)
                sb.Append("завершение печати");
              else
                sb.Append("страница " + _LastStatusInfo.CurrentPage);
            }
            sb.Append(". Еще осталось документов в очереди: ");
            sb.Append(_TheQueue.Count);
            spl.PhaseText = sb.ToString();
          }
          Application.DoEvents();
          if (spl.Cancelled)
            _Worker.CancelAsync();
        }

        // Очищаем очередь
        lock (_TheQueue)
        {
          _TheQueue.Clear();
        }
      }
      finally
      {
        if (spl != null)
          spl.Close();
      }
      return true;
    }

    #endregion

    #region Очередь печати

    /// <summary>
    /// Очередь заданий на печать.
    /// При обращении должна выполняться блокировка.
    /// </summary>
    private readonly Queue<PrintDocument> _TheQueue;

    /// <summary>
    /// Добавляет документ в очередь фоновой печати
    /// </summary>
    /// <param name="document">Документ</param>
    public void Add(PrintDocument document)
    {
#if DEBUG
      if (document == null)
        throw new ArgumentNullException("document");
#endif
      EFPApp.BeginWait("Постановка задания в очередь", "Print");
      try
      {
        lock (_TheQueue)
        {
          _TheQueue.Enqueue(document);
          // Не выходим из блокировки
          if (!_InProgress)
            _Worker.RunWorkerAsync();
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    #endregion

    #region Исполнение в фоновом режиме

    /// <summary>
    /// Свойство устанавливается и сбрасывается в фоновом потоке
    /// При обращении должно блокироваться и обрабатываться до разблокирования TheQueue
    /// Нельзя использовать Worker.IsBusy, т.к. свойство еще будет true, когда 
    /// фоновый поток завершается, и новое задание не будет запущено, т.к. очередь
    /// некому просматривать
    /// </summary>
    private bool _InProgress;

    private readonly BackgroundWorker _Worker;

    void Worker_DoWork(object sender, DoWorkEventArgs args)
    {
      while (true)
      {
        PrintDocument document;
        lock (_TheQueue)
        {
          if (_TheQueue.Count == 0 || _Worker.CancellationPending)
          {
            _InProgress = false; // если что, пусть запускается новое задание
            break;
          }
          _InProgress = true; // Говорим, что у нас есть работа
          document = _TheQueue.Dequeue();
        }

        PrintOneDocument(document);
      }
    }

    /// <summary>
    /// Информация, передаваемая из фонового потока в основной поток приложения
    /// </summary>
    private class StatusInfo
    {
      #region Конструктор

      public StatusInfo(string documentName, int currentPage)
      {
        _DocumentName = documentName;
        _CurrentPage = currentPage;
      }

      #endregion

      #region Свойства

      public string DocumentName { get { return _DocumentName; } }
      private readonly string _DocumentName;

      public int CurrentPage { get { return _CurrentPage; } }
      private readonly int _CurrentPage;

      #endregion
    }

    /// <summary>
    /// Счетчик страниц - используется в фоновом потоке
    /// </summary>
    private int _CurrentPage;

    private void PrintOneDocument(PrintDocument document)
    {
      _CurrentPage = 1;
      _Worker.ReportProgress(0, new StatusInfo(document.DocumentName, 1));
      document.PrintController = new StandardPrintController();
      document.PrintPage += new PrintPageEventHandler(Document_PrintPage);
      document.Print();
    }

    void Document_PrintPage(object sender, PrintPageEventArgs args)
    {
      PrintDocument document = (PrintDocument)sender;
      if (args.HasMorePages)
      {
        _CurrentPage++;
        _Worker.ReportProgress(0, new StatusInfo(document.DocumentName, _CurrentPage));
        if (_Worker.CancellationPending)
          args.HasMorePages = false; // ?? Может быть нужно предупреждение
      }
      else
        _Worker.ReportProgress(0, new StatusInfo(document.DocumentName, -1));
    }

    /// <summary>
    /// Этот объект текущего состояния получен основным потоком приложения
    /// </summary>
    private StatusInfo _LastStatusInfo;

    void Worker_ProgressChanged(object sender, ProgressChangedEventArgs args)
    {
      _LastStatusInfo = (StatusInfo)(args.UserState);
      //FStatusItem.ImageKey = "Print";
      _StatusItem.Enabled = true;
      if (_LastStatusInfo.CurrentPage < 0)
      {
        _StatusItem.StatusBarText = "...";
        _StatusItem.ToolTipText = "Завершается печать документа \"" + _LastStatusInfo.DocumentName + "\"";
      }
      else
      {
        _StatusItem.StatusBarText = _LastStatusInfo.CurrentPage.ToString();
        _StatusItem.ToolTipText = "Идет печать страницы " + _LastStatusInfo.CurrentPage.ToString() + " документа \"" + _LastStatusInfo.DocumentName + "\"";
      }
    }

    void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
    {
      _LastStatusInfo = null;
      _StatusItem.Enabled = false;
      _StatusItem.StatusBarText = EFPCommandItem.EmptyStatusBarText;
      _StatusItem.ToolTipText = DefaultStatusToolTipText;
    }

    #endregion
  }
}
