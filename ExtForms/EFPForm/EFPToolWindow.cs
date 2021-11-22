// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;
using System.Runtime.InteropServices;

// Запуск форм в единственном экземпляре

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Базовый класс для панелей инструментов, например, калькулятора
  /// При запуске программы следует создать один экземпляр формы и сохранить ссылку
  /// на него. Затем используются методы Show() и Hide() для управления видимостью
  /// окна. Также может быть присвоено свойство CommandItem при создании главного
  /// меню, которая будет управлять видимостью панели
  /// </summary>
  public class EFPToolWindow : Form
  {
    #region Конструктор

    /// <summary>
    /// Конструктор формы
    /// </summary>
    public EFPToolWindow()
    {
      InitializeComponent();

      _FormProvider = new EFPFormProvider(this);
    }

    private void InitializeComponent()
    {
      this.SuspendLayout();
      // 
      // EFPToolWindow
      // 
      this.ClientSize = new System.Drawing.Size(292, 86);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "EFPToolWindow";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.ResumeLayout(false);

    }

    /// <summary>
    /// Вызывает Show() и Activate().
    /// Если на момент вызова метода форма не была показана на экране, вызывается
    /// метод OnUserShow()
    /// </summary>
    public void UserShow()
    {
      bool OldVisible = Visible;

      //if (EFPApp.ActiveDialog != null)
      //  Owner = EFPApp.ActiveDialog;

      Show();
      Activate();

      if (!OldVisible)
        OnUserShow();
    }

    /// <summary>
    /// Вызывается после показа формы в результате действий пользователя
    /// Переопределенный метод, например для окна калькулятора, может очистить поле
    /// ввода
    /// </summary>
    protected virtual void OnUserShow()
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Информация о размещении формы
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct ScreenInfo
    {
      #region Конструктор

      /// <summary>
      /// Запоминает положение формы
      /// </summary>
      /// <param name="form">Форма</param>
      public ScreenInfo(Form form)
      {
        Screen Scr = Screen.FromControl(form);
        _ScreenSize = Scr.Bounds.Size;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Разрешение экрана
      /// </summary>
      public Size ScreenSize { get { return _ScreenSize; } }
      private Size _ScreenSize;

      #endregion
    }

    /// <summary>
    /// Информация о последнем или текущем расположении формы
    /// </summary>
    public ScreenInfo LastScreenInfo
    {
      get
      {
        if (Visible)
          return new ScreenInfo(this);
        else
          return _LastScreenInfo;
      }
    }
    private ScreenInfo _LastScreenInfo;

    /// <summary>
    /// Возвращает true, если панель была показана хотя бы один раз в текущем сеансе
    /// работы
    /// </summary>
    public bool HasBeenShown { get { return _HasBeenShown; } }
    private bool _HasBeenShown;

    /// <summary>
    /// Провайдер формы.
    /// Создается в конструкторе
    /// </summary>
    public EFPFormProvider FormProvider { get { return _FormProvider; } }
    private EFPFormProvider _FormProvider;

    #endregion

    #region Обработчики формы

    /// <summary>
    /// Обработка события закрытия формы
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnFormClosing(FormClosingEventArgs args)
    {
      base.OnFormClosing(args);
      if (args.Cancel)
        return;

      if (args.CloseReason == CloseReason.UserClosing)
      {
        args.Cancel = true;
        Hide();
        Owner = null;
        EFPApp.ToolFormsForDialogs.Remove(this);
      }
    }

    /// <summary>
    /// Обработка события смены видимости формы
    /// </summary>
    /// <param name="args"></param>
    protected override void OnVisibleChanged(EventArgs args)
    {
      base.OnVisibleChanged(args);
      if (Visible)
      {
        _HasBeenShown = true;
        if (Owner == null && EFPApp.IsMainThread)
        {
          Owner = EFPApp.MainWindow;
          if (!EFPApp.ToolFormsForDialogs.Contains(this))
            EFPApp.ToolFormsForDialogs.Add(this);
        }
      }
      else
        _LastScreenInfo = new ScreenInfo(this);
    }

    /// <summary>
    /// Обработка завершения работы
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
      EFPApp.ToolWindows.Remove(this);

      base.Dispose(disposing);
    }

    /// <summary>
    /// Обработка нажатия клавиши
    /// </summary>
    /// <param name="args"></param>
    protected override void OnKeyDown(KeyEventArgs args)
    {
      if ((!args.Handled))
      {
        if (args.KeyData == Keys.Escape)
        {
          EFPApp.MainWindow.Activate();
          args.Handled = true;
          return;
        }
      }
      if (VisibleCommandItem != null)
      { 
        if (VisibleCommandItem.IsShortCut(args.KeyData))
        {
          if (EFPApp.ActiveDialog != null)
            EFPApp.ActiveDialog.Activate(); // 05.04.2014
          else if (EFPApp.MainWindowVisible)
            EFPApp.MainWindow.Activate();
          args.Handled = true;
          return;
        }
      }

      base.OnKeyDown(args);
    }

    /// <summary>
    /// Блокируем активацию окна после закрытия блока диалога
    /// </summary>
    protected override bool ShowWithoutActivation { get { return false; } }

    //protected override void OnActivated(EventArgs args)
    //{
    //  base.OnActivated(args);

    //  if (ActiveControl != null)
    //    InvokeGotFocus(ActiveControl, args);
    //}

    //protected override void OnDeactivate(EventArgs args)
    //{
    //  if (base.ActiveControl != null)
    //    InvokeLostFocus(ActiveControl, args);
    //  base.OnDeactivate(args);
    //}

    #endregion

    #region Команда меню

    /// <summary>
    /// Команда меню (например "Сервис"-"Калькулятор"), выполнение которой показывает
    /// панель, если она не видна, и активирует ее.
    /// Для закрытия панели используется крестик на форме
    /// </summary>
    public EFPCommandItem VisibleCommandItem
    {
      get { return _VisibleCommandItem; }
      set
      {
        if (_VisibleCommandItem != null)
          throw new InvalidOperationException("Повторная установка свойства VisibleClientItem не допускается");
        if (value == null)
          throw new ArgumentNullException();
        _VisibleCommandItem = value;
        _VisibleCommandItem.Click += new EventHandler(FVisibleCommandItem_Click);
      }
    }
    private EFPCommandItem _VisibleCommandItem;

    void FVisibleCommandItem_Click(object sender, EventArgs args)
    {
      UserShow();
    }

    #endregion
  }

  /// <summary>
  /// Список панелей инструментов ClientToolWindow.
  /// Реализация свойства EFPApp.ToolWindows
  /// </summary>
  public class EFPAppToolWindows : List<EFPToolWindow>
  {
  }
}
