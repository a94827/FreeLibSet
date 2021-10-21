using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Logging;
using FreeLibSet.Core;

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

/*
 * Многошаговый диалог "Мастер" с кнопками навигации "Назад" и "Далее"
 * 
 * Мастер удобно реализовывать с помощью вспомогательного класса формы, содержащей
 * закладки. Одна закладка соответствует одному шагу мастера, но порядок закладок и
 * шагов Мастера не обязаны совпадать. Поля класса формы используются для хранения
 * значений, введенных пользователем, между шагами мастера
 * 
 * Программа создает объект StepWizard, описывающий первое окно Мастера и передает
 * его создаваемому объекту Wizard. Устанавливаются необходимые свойства объекта
 * Wizard, после чего вызывается метод Execute().
 * 
 * Мастер присоединяет управляющий элемент, хранящийся в поле StepWizard.Control к
 * основной форме Мастера и вызывает событие StepWizard.BeginStep, которое должно
 * присвоить текущие значения управляющим элементам.
 * 
 * Когда пользователь нажимает кнопку "Далее", сначала вызывается событие EndStep,
 * а, затем, StepWizard.GetNext.
 * Обработчик этого события должен присутствовать обязательно для всех шагов Мастера,
 * кроме последнего. Обработчик должен прочитать введенные пользователем значения из 
 * управляющего элемента и запомнить их в своих локальных переменных. Затем он должен
 * создать новый объект StepWizard и поместить его в поле NextStep переданного 
 * аргумента Args. Текущий объект помещается в стек и выполняется инициализация
 * следующего шага (присоединяется StepWizard.Control и вызывается BeginStep)
 * 
 * В качестве альтернативы может быть возвращен один из уже созданных
 * ранее в цепочке объектов StepWizard. В этом случае цепочка вызовов "зациклиться",
 * а все объекты, идущие после возвращенного, включая и текущий, удаляются.
 * 
 * Если пользователь нажимает кнопку "Назад", то событие EndStep вызывается, а
 * GetNext -нет, вместо
 * этого извлекается из стека предыдущий кадр, присоединяется управляющий элемент
 * StepWizard.Control. Вызывается событие InitStep. Текущий кадр уничтожается.
 * 
 * Последний кадр Мастера должен иметь установленное свойство FinalStep. В этом
 * случае кнопка "Далее" заменяется на "Готово". При ее нажатии вывзывается событие 
 * EndStep, а GetNext не вызывается. Затем вызывается событие Wizard.Finish. Перед 
 * этим форма  мастера закрывается и все объекты StepWizard разрушаются.
 * 
 * Чтобы данные могли считываться независимо от нажатия кнопок, у WizardStep есть
 * события BeginStep и EndStep. Обработчики могут получать аргумент, показывающий 
 * направление движения и выполнять чтение и запись данных. EndStep может 
 * предотвращать переход к следующему шагу или закрытие формы Мастера с помощью
 * аргумента Cancel
 * 
 * 
 * Вызовы Dispose()
 * 
 * Объект Wizard самоликвидируется после закрытия формы Мастера, поэтому вызывать 
 * Dispose() для него в явном виде не требуется. Повторное выполнение Execute() не
 * допускается.
 * 
 * Чтобы разрушилась форма, закладки из которой используются в качестве кадров
 * Мастера, следует вызвать Wizard.AddDisposable(MyForm). В этом случае будет вызван
 * метод Dispose() при закрытии Мастера
 */

namespace FreeLibSet.Forms
{
  internal partial class WizardForm : Form
  {
    #region Конструктор

    public WizardForm(Wizard theWizard)
    {
      InitializeComponent();
      KeyPreview = true;

      efpForm = new EFPFormProvider(this);
      efpForm.FormClosing += new FormClosingEventHandler(WizardForm_FormClosing);

      btnNext.ImageAlign = ContentAlignment.MiddleLeft;
      // картинка инициализируется отдельно после каждого шага
      efpNext = new EFPButton(efpForm, btnNext);
      efpNext.Click += new System.EventHandler(btnNext_Click);

      btnBack.Image = EFPApp.MainImages.Images["LeftLeft"];
      btnBack.ImageAlign = ContentAlignment.MiddleLeft;
      efpBack = new EFPButton(efpForm, btnBack);
      efpBack.Click += new System.EventHandler(btnBack_Click);

      btnCancel.Image = EFPApp.MainImages.Images["Cancel"];
      btnCancel.ImageAlign = ContentAlignment.MiddleLeft;
      efpCancel = new EFPButton(efpForm, btnCancel);
      efpCancel.Click += new System.EventHandler(btnCancel_Click);


      _TheWizard = theWizard;

      if (!theWizard.ShowImage)
        panImage.Visible = false;
    }

    #endregion

    #region Поля

    public EFPFormProvider efpForm;

    public EFPButton efpNext, efpBack, efpCancel;

    #endregion

    #region Свойства

    /// <summary>
    /// Мастер, к которому относится форма.
    /// Задается в конструкторе
    /// </summary>
    public Wizard TheWizard { get { return _TheWizard; } }
    private Wizard _TheWizard;

    /// <summary>
    /// Номер теущего шага
    /// </summary>
    public int CurrentStepIndex
    {
      get
      {
        return _TheWizard.Steps.Count - 1;
      }
    }

    /// <summary>
    /// Объект текущего шага
    /// </summary>
    public WizardStep CurrentStep
    {
      get
      {
        if (CurrentStepIndex < 0)
          return null; // 02.06.2015 какой-то глюк
        return _TheWizard.Steps[CurrentStepIndex];
      }
    }

    #endregion

    #region Выполнение

    bool _FirstShowCall = true;

    protected override void OnShown(EventArgs args)
    {
      base.OnShown(args);
      if (_FirstShowCall)
      {
        // Подготовка первого шага
        // Должно вызываться после того, как форма выведена на экран
        InitStep(WizardAction.Next, true);
        _FirstShowCall = false;
      }
    }

    /// <summary>
    /// Инициализация очередного шага
    /// </summary>
    private void InitStep(WizardAction action, bool firstInit)
    {
#if DEBUG
      if (TheWizard.TempPage != null)
        throw new BugException("Выведена временная страница");
#endif

      panMain.SuspendLayout();
      try
      {
        panMain.Controls.Clear();
        //CurrentStep.Control.Parent = null;
        WinFormsTools.AddControlAndScale(panMain, CurrentStep.Control);
        //panMain.Controls.Add(CurrentStep.Control);
        //CurrentStep.Control.PerformAutoScale();
      }
      finally
      {
        panMain.ResumeLayout();
      }

      InitButtons();

      if (TheWizard.ShowImage)
        panImage.Visible = CurrentStep.ShowImage;

      if (firstInit)
      {
        CurrentStep.BaseProvider.Parent = efpForm;
        CurrentStep._Wizard = TheWizard; // должно быть до вызова OnBeginStep

        CurrentStep.OnBeginStep(action);
        CurrentStep.BaseProvider.Validate();
      }
      panMain.Focus(); // добавлен 09.08.2011
      // Активируем первый "полезный" управляющий элемент, иначе - кнопку "вперед"
      panMain.SelectNextControl(null, true, true, true, false);
      if (!panMain.ContainsFocus)
      {
        if (btnNext.Enabled)
          btnNext.Select();
      }
    }

    internal void InitButtons()
    {
      InitBtnNext();
      InitBtnBack();
      InitBtnCancel();
    }

    internal void InitBtnNext()
    {
      if (TheWizard.TempPage == null)
      {
        if (CurrentStep.FinalStep)
        {
          efpNext.Text = "&Готово";
          btnNext.Image = EFPApp.MainImages.Images["Ok"];
        }
        else
        {
          efpNext.Text = "&Далее";
          btnNext.Image = EFPApp.MainImages.Images["RightRight"];
        }
        efpNext.Enabled = CurrentStep.ForwardEnabled;
      }
      else
      {
        efpNext.Enabled = false;
      }
    }

    internal void InitBtnBack()
    {
      if (TheWizard.TempPage == null)
        efpBack.Enabled = CurrentStepIndex > 0 && CurrentStep.BackEnabled;
      else
        efpBack.Enabled = false;
    }

    internal void InitBtnCancel()
    {
      if (TheWizard.TempPage == null)
        efpCancel.Enabled = true;
      else
        efpCancel.Enabled = TheWizard.TempPage.CancelEnabled;
    }

    private void btnBack_Click(object sender, EventArgs args)
    {
      if (CurrentStepIndex < 1)
      {
        EFPApp.ShowTempMessage("Нельзя отменить первый шаг");
        return;
      }
      if (!CurrentStep.OnEndStep(WizardAction.Back))
        return;

      CurrentStep.BaseProvider.Parent = null;

      panMain.Controls.Clear();
      DeleteLastStep();

      InitStep(WizardAction.Back, true);
    }

    private void DeleteLastStep()
    {
      CurrentStep.Close();
      TheWizard.Steps.RemoveAt(CurrentStepIndex);
    }

    private void btnNext_Click(object sender, EventArgs args)
    {
      if (!efpForm.ValidateForm())
        return;

      if (CurrentStep == null)
        throw new NullReferenceException("Не найден текущий шаг мастера (CurrentStep=null)");

      if (!CurrentStep.OnEndStep(CurrentStep.FinalStep ? WizardAction.Finish : WizardAction.Next))
      {
        CloseAllTempPages(true);
        return;
      }
      if (CurrentStep.FinalStep)
      {
        DialogResult = DialogResult.OK;
        Close(); // Без проверки закрытия

        TheWizard.OnFinish();
        TheWizard.Dispose();
        return;
      }

      // Нажата кнопка "Далее"
      WizardStep NextStep = CurrentStep.OnGetNext();
      if (NextStep == null)
      {
        CloseAllTempPages(true);
        EFPApp.ShowTempMessage("Следующий кадр не определен");
        return;
      }

      CloseAllTempPages(false);
      CurrentStep.BaseProvider.Parent = null;

      int Pos = TheWizard.Steps.IndexOf(NextStep);
      if (Pos < 0)
      {
        // Получен новый кадр
        TheWizard.Steps.Add(NextStep);
        InitStep(WizardAction.Next, true);
      }
      else
      {
        // Возврат к предыдущему шагу
        panMain.Controls.Clear();
        while (CurrentStepIndex > Pos)
          DeleteLastStep();
        InitStep(WizardAction.CircleNext, true);
      }
    }

    private void btnCancel_Click(object sender, EventArgs args)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    void WizardForm_FormClosing(object sender, FormClosingEventArgs args)
    {
      try
      {
        WizardForm_DoFormClosing(args);
      }
      catch (Exception e)
      {
        TheWizard.AddExceptionInfo(e);
        LogoutTools.LogoutException(e, "Ошибка WizardForm.FormClosing");
        args.Cancel = false; // пусть закрывают
      }
    }
    void WizardForm_DoFormClosing(FormClosingEventArgs args)
    {
      if (DialogResult == DialogResult.OK)
        // "Готово"
        return;

      if (args.Cancel)
        return;

      // Закрытие формы или "Отмена"
      if (TheWizard.TempPage == null)
      {
        if (CurrentStep == null)
          // 02.06.2015 - Бяка
          return;

        if (!CurrentStep.OnEndStep(WizardAction.Cancel))
          args.Cancel = true;
      }
      else
      {
        if (TheWizard.TempPage.CancelEnabled)
          TheWizard.TempPage.OnCancelClick();
        args.Cancel = true;
      }
    }

    internal void InitTempPage(bool firstInit)
    {
#if DEBUG
      if (TheWizard.TempPage == null)
        throw new BugException("Нет временной страницы");
#endif
      panMain.Controls.Clear();
      panMain.Controls.Add(TheWizard.TempPage.Control);

      InitButtons();

      //if (TheWizard.ShowImage)
      //  panImage.VisibleEx = CurrentStep.ShowImage;

      if (firstInit)
      {
        TheWizard.TempPage.BaseProvider.Parent = efpForm;
        TheWizard.TempPage._Wizard = TheWizard;

        //CurrentStep.OnBeginStep(Action);
        TheWizard.TempPage.BaseProvider.Validate();
      }
      // Активируем первый "полезный" управляющий элемент, иначе - кнопку "вперед"
      SelectNextControl(null, true, true, true, false);
      // Control ctl = panMain.GetNextControl(null, true);
      // if (ctl!=null)
      //   ctl.Select();
      if (!panMain.ContainsFocus)
      {
        if (btnNext.Enabled)
          btnNext.Select();
      }

      // Требуется принудительно перерисовать форму.
      // В отличие от обычного шага, для временной страницы может не вызываться
      // опрос событий
      Update();
    }

    /// <summary>
    /// Закрыть все открытые временные страницы
    /// </summary>
    /// <param name="restoreCurrStep"></param>
    private void CloseAllTempPages(bool restoreCurrStep)
    {
      if (TheWizard.TempPage == null)
        return;
      panMain.Controls.Clear(); // Иначе нельзя разрушать управляющие элементы закладки
      while (TheWizard.TempPage != null)
      {
        WizardTempPage ThisPage = TheWizard.TempPage;
        ThisPage.Close();
        TheWizard._TempPage = ThisPage.PrevTempPage;
        ThisPage.PrevTempPage = null;
        ThisPage._Wizard = null;
      }
      if (restoreCurrStep && CurrentStep != null)
        InitStep(WizardAction.Cancel, false);
    }

    internal void CloseTempPage()
    {

      if (TheWizard.TempPage == null)
        throw new BugException("Нет временной страницы");
      panMain.Controls.Clear(); // Иначе нельзя разрушать управляющие элементы закладки

      WizardTempPage ThisPage = TheWizard.TempPage;
      ThisPage.Close();
      TheWizard._TempPage = ThisPage.PrevTempPage;
      ThisPage.PrevTempPage = null;
      ThisPage._Wizard = null;

      if (TheWizard.TempPage == null)
        InitStep(WizardAction.Cancel, false);
      else
        InitTempPage(false);
    }

    #endregion
  }

  /// <summary>
  /// Объект "Мастер".
  /// Вывод последовательности диалогов с навигацией "Вперед" и "Назад"
  /// </summary>
  public class Wizard : DisposableObject
  {
    // 03.01.2021
    // Не знаю, можно ли использовать SimpleDisposableObject

    #region Конструктор

    /// <summary>
    /// Создает мастер, который будет начинаться с шага <paramref name="firstStep"/>
    /// </summary>
    /// <param name="firstStep">Первый шаг Мастера. Объект должен быть задан</param>
    public Wizard(WizardStep firstStep)
    {
      Steps = new List<WizardStep>();

      if (firstStep == null)
        throw new ArgumentNullException("firstStep");

      Steps.Add(firstStep);

      ShowImage = true;
    }

    /// <summary>
    /// Разрушение мастера.
    /// Вызывает обработчик события Disposed, если он установлен.
    /// Затем вызывается метод Dispose() для всех присоединенных объектов
    /// в списке DisposableObjects. Затем ликвидируются объекты WizardStep.
    /// </summary>
    /// <param name="disposing">true, если был вызван метод Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (Disposed != null)
        {
          try
          {
            Disposed(this, EventArgs.Empty);
          }
          catch (Exception e)
          {
            AddExceptionInfo(e);
            EFPApp.ShowException(e, "Событие Wizard.Disposed"); // не Wizard.ShowException()
          }
        }
      }

      if (_DisposableObjects != null)
      {
        for (int i = 0; i < _DisposableObjects.Count; i++)
          _DisposableObjects[i].Dispose();
        _DisposableObjects = null;
      }

      for (int j = 0; j < Steps.Count; j++)
        Steps[j].Close();
      Steps.Clear();

      base.Dispose(disposing);
    }

    private List<IDisposable> _DisposableObjects;

    /// <summary>
    /// Добавляет в список объект, который должен быть удален.
    /// Когда работа мастера завершается, то для всех добавленных
    /// в список объектов будет вызван метод Dispose()
    /// </summary>
    /// <param name="obj">Объект, реализующий интерфейс IDisposable</param>
    public void AddDisposable(IDisposable obj)
    {
      CheckNotDisposed();

      if (obj == null)
        return;
      if (_DisposableObjects == null)
        _DisposableObjects = new List<IDisposable>();
      if (_DisposableObjects.Contains(obj))
        return;
      _DisposableObjects.Add(obj);
    }

    #endregion

    #region Свойства, устанавливаемые до запуска

    /// <summary>
    /// Заголовок окна
    /// </summary>
    public string Title { get { return _Title; } set { _Title = value; } }
    private string _Title;

    /// <summary>
    /// Должно ли показываться изображение в левой части экрана (по умолчанию - true).
    /// Можно запретить изображение для отдельных шагов, устанавливая 
    /// StepWizard.ShowImage=false
    /// </summary>
    public bool ShowImage { get { return _ShowImage; } set { _ShowImage = value; } }
    private bool _ShowImage;

    /// <summary>
    /// Если true, то пользователь может менять размеры окна. Также имеется кнопка
    /// "Развернуть". По умолчанию - false, изменять размеры окна нельзя
    /// </summary>
    public bool Sizeable { get { return _Sizeable; } set { _Sizeable = value; } }
    private bool _Sizeable;

    /// <summary>
    /// Изображения для значка формы мастера из EFPApp.MainImages.
    /// Если не задано (по умолчанию), то окно не содержит иконки
    /// </summary>
    public string ImageKey { get { return _ImageKey; } set { _ImageKey = value; } }
    private string _ImageKey;

    /// <summary>
    /// Контекст справки для формы в-целом
    /// </summary>
    public string HelpContext
    {
      get { return _HelpContext; }
      set
      {
        if (TheForm != null)
          throw new InvalidOperationException("Свойство можно устанавливать только до запуска мастера");
        _HelpContext = value;
      }
    }
    private string _HelpContext;

    #endregion

    #region Свойства времени выполнения мастера

    internal WizardForm TheForm;

    /// <summary>
    /// Текущий шаг "Мастера".
    /// Переключение шагов мастера выполняется при нажатии кнопок пользователем.
    /// Определение следующего шага мастера определяется обработчиком события
    /// WizardStep.NextStep и не может быть изменено снаружи.
    /// </summary>
    public WizardStep CurrentStep
    {
      get
      {
        if (TheForm == null)
          return null;
        else
          return TheForm.CurrentStep;
      }
    }

    /// <summary>
    /// Свойство возвращает true, если мастер был завершен нажатием кнопки "Готово"
    /// </summary>
    public bool Finished { get { return _Finished; } }
    private bool _Finished;

    #endregion

    #region События

    /// <summary>
    /// Вызывается после завершения последнего кадра нажатием кнопки "Готово"
    /// </summary>
    public event EventHandler Finish;

    internal void OnFinish()
    {
      try
      {
        if (Finish != null)
          Finish(this, EventArgs.Empty);

        _Finished = true; // только если успешно завершено
      }
      catch (Exception e)
      {
        ShowException(e, "Завершение работы мастера");
      }
    }

    /// <summary>
    /// Вызывается при завершении работы мастера независимо от результата
    /// </summary>
    public event EventHandler Disposed;

    #endregion

    #region Выполнение

    /// <summary>
    /// Уже выполненные шаги
    /// </summary>
    internal List<WizardStep> Steps;

    /// <summary>
    /// Запуск на выполнение
    /// </summary>
    public void Execute()
    {
      CheckNotDisposed();

      _Finished = false;

      TheForm = new WizardForm(this);
      TheForm.Text = Title;
      if (String.IsNullOrEmpty(ImageKey))
      {
        if (EFPApp.MainWindowVisible)
          TheForm.ShowIcon = false;
        else
          // 26.01.2014
          // Программа может быть запущена без главного окна и реализована как мастер
          // В этом случае значком формы мастера должен быть значок приложения
          WinFormsTools.InitAppIcon(TheForm);
      }
      else
      {
        TheForm.Icon = EFPApp.MainImageIcon(ImageKey);
        TheForm.ShowIcon = true;
      }
      if (Sizeable)
      {
        TheForm.FormBorderStyle = FormBorderStyle.Sizable;
        TheForm.MaximizeBox = true;
      }
      TheForm.efpForm.HelpContext = HelpContext;

      EFPApp.ShowDialog(TheForm, true);
      // Самоликвидируемся
      TheForm = null;
      Dispose();
    }

    #endregion

    #region Временные страницы

    /// <summary>
    /// Отобразить временную страницу с произвольным содержимым
    /// </summary>
    /// <param name="page">Временная страница</param>
    public void BeginTempPage(WizardTempPage page)
    {
#if DEBUG
      if (page == null)
        throw new ArgumentNullException("page");
#endif
      if (page.Wizard != null || page.PrevTempPage != null)
        throw new ArgumentException("Повторное присоединение временной страницы", "page");

      page.PrevTempPage = TempPage;
      _TempPage = page;
      TheForm.InitTempPage(true);
    }

    /// <summary>
    /// Простая реализация временной страницы, содержащей только текст посередине (с одним элементом)
    /// </summary>
    /// <param name="item">Текст для временной заставки</param>
    public ISplash BeginTempPage(string item)
    {
      //Panel ThePanel = new Panel();
      //ThePanel.Dock = DockStyle.Fill;
      //Label TheLabel = new Label();
      //TheLabel.AutoSize = false;
      //TheLabel.Dock = DockStyle.Fill;
      //TheLabel.UseMnemonic = false;
      //TheLabel.Text = Text;
      //TheLabel.TextAlign = ContentAlignment.MiddleCenter;
      //ThePanel.Controls.Add(TheLabel);
      //WizardTempPage Page = new WizardTempPage(ThePanel, true);
      //BeginTempPage(Page);

      return BeginTempPage(new string[] { item });
    }
                
    /// <summary>
    /// Создать временную страницу с закладкой
    /// </summary>
    /// <param name="items">Список действий, выводимый в splash-заставки</param>
    /// <returns>Интерфейс для управления splash-заставкой</returns>
    public ISplash BeginTempPage(string[] items)
    {
      WizardSplashPage Page = new WizardSplashPage(items);
      BeginTempPage(Page);
      return Page.Splash;
    }

    /// <summary>
    /// Закрыть временную страницу, созданную BefinTempPage().
    /// </summary>
    public void EndTempPage()
    {
      TheForm.CloseTempPage();
    }

    /// <summary>
    /// Текущая отображаемая временная страница или null в обычном состоянии, когда
    /// отображается шаг мастера
    /// </summary>
    public WizardTempPage TempPage { get { return _TempPage; } }
    internal WizardTempPage _TempPage;

    #endregion

    #region Обработка исключений

    /// <summary>
    /// Это событие вызывается, когда при выполнении мастера возникает исключение, перед вызовом
    /// EFPApp.ShowException().
    /// Обработчик может, например, добавить к исключению собственные данные или самостоятельно
    /// вывести сообщение об ошибке.
    /// Если обработик установил свойство Handled, показ исключения не выполняется
    /// </summary>
    public event EFPAppExceptionEventHandler HandleException;

    /// <summary>
    /// Вызывает EFPApp.ShowException(), предварительно вызывая событие HandleException
    /// </summary>
    /// <param name="exception">Перехваченное исключение</param>
    /// <param name="exceptionTitle">Заголовок исключения</param>
    public void ShowException(Exception exception, string exceptionTitle)
    {
      exception.Data["ExceptionTitle"] = exceptionTitle;
      AddExceptionInfo(exception);

      if (HandleException != null)
      {
        try
        {
          EFPAppExceptionEventArgs Args = new EFPAppExceptionEventArgs(exception, exceptionTitle);
          HandleException(this, Args);
          if (Args.Handled)
            return;
        }
        catch (Exception e2)
        {
          AddExceptionInfo(e2);
          LogoutTools.LogoutException(e2, "Ошибка обработки события Wizard.HandlerException");
        }
      }
      EFPApp.ShowException(exception, exceptionTitle);
    }

    internal void AddExceptionInfo(Exception exception)
    {
      try
      {
        exception.Data["Wizard.Title"] = this.Title;
        exception.Data["Wizard.CurrentStepIndex"] = this.Steps.IndexOf(this.CurrentStep);
      }
      catch { }
    }

    #endregion
  }

  #region Перечисления

  /// <summary>
  /// Нажатия кнопок в Мастере
  /// </summary>
  public enum WizardAction
  {
    /// <summary>
    /// Нажата кнопка "Вперед" или первый кадр Мастера
    /// </summary>
    Next,

    /// <summary>
    /// Нажата кнопка "Назад"
    /// </summary>
    Back,

    /// <summary>
    /// Нажата кнопка "Готово"
    /// </summary>
    Finish,

    /// <summary>
    /// Нажата кнопка "Отмена" или форма закрывается
    /// </summary>
    Cancel,

    /// <summary>
    /// Кадр открывается повторно при зацикливании цепочки
    /// </summary>
    CircleNext
  }

  #endregion

  #region Делегаты

  /// <summary>
  /// Аргументы события WizardStep.GetNext
  /// </summary>
  public class WizardGetNextEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает аргументы события
    /// </summary>
    /// <param name="wizard">Мастер</param>
    public WizardGetNextEventArgs(Wizard wizard)
    {
      _Wizard = wizard;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Мастер, к которому относится шаг. 
    /// </summary>
    public Wizard Wizard { get { return _Wizard; } }
    private Wizard _Wizard;

    /// <summary>
    /// Сюда должен быть помещен следующий шаг
    /// </summary>
    public WizardStep NextStep { get { return _NextStep; } set { _NextStep = value; } }
    private WizardStep _NextStep;

    #endregion
  }

  /// <summary>
  /// Делегат события WizardStep.GetNext
  /// </summary>
  /// <param name="sender">Ссылка на текщий шаг Мастера (WizardStep)</param>
  /// <param name="args">Аргументы события</param>
  public delegate void WizardGetNextEventHandler(object sender,
    WizardGetNextEventArgs args);

  /// <summary>
  /// Аргументы события WizardStep.BeginStep
  /// </summary>
  public class WizardBeginStepEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает аргументы события
    /// </summary>
    /// <param name="wizard">Мастер</param>
    /// <param name="action">Выполняемое действие</param>
    public WizardBeginStepEventArgs(Wizard wizard, WizardAction action)
    {
      _Wizard = wizard;
      _Action = action;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Мастер, к которому относится шаг. 
    /// </summary>
    public Wizard Wizard { get { return _Wizard; } }
    private Wizard _Wizard;

    /// <summary>
    /// Выполняемое действие (Next, Back или CircleNext)
    /// </summary>
    public WizardAction Action { get { return _Action; } }
    private WizardAction _Action;

    /// <summary>
    /// Возвращает true, если выполняемое действие означает движение вперед
    /// (Next или Finish)
    /// </summary>
    public bool Forward { get { return _Action == WizardAction.Next || _Action == WizardAction.Finish || _Action == WizardAction.CircleNext; } }

    #endregion
  }

  /// <summary>
  /// Делегат события WizardStep.BeginStep
  /// </summary>
  /// <param name="sender">Ссылка на текщий шаг Мастера (WizardStep)</param>
  /// <param name="args">Аргументы события</param>
  public delegate void WizardBeginStepEventHandler(object sender,
    WizardBeginStepEventArgs args);

  /// <summary>
  /// Аргументы события WizardStep.EndStep
  /// </summary>           
  public class WizardEndStepEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает аргументы события
    /// </summary>
    /// <param name="wizard">Мастер</param>
    /// <param name="action">Выполняемое действие</param>
    public WizardEndStepEventArgs(Wizard wizard, WizardAction action)
    {
      _Wizard = wizard;
      _Action = action;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Мастер, к которому относится шаг. 
    /// </summary>
    public Wizard Wizard { get { return _Wizard; } }
    private Wizard _Wizard;

    /// <summary>
    /// Выполняемое действие (Next, Finish, Back или Cancel)
    /// </summary>
    public WizardAction Action { get { return _Action; } }
    private WizardAction _Action;

    /// <summary>
    /// Возвращает true, если выполняемое действие означает движение вперед
    /// (Next или Finish)
    /// </summary>
    public bool Forward { get { return _Action == WizardAction.Next || _Action == WizardAction.Finish || _Action == WizardAction.CircleNext; } }

    /// <summary>
    /// Сюда должно быть помещено значение true, чтобы предотвратить переход к
    /// другому кадру или закрытие Мастера.
    /// </summary>
    public bool Cancel { get { return _Cancel; } set { _Cancel = value; } }
    private bool _Cancel;

    #endregion
  }

  /// <summary>
  /// Делегат события WizardStep.EndStep
  /// </summary>
  /// <param name="sender">Ссылка на текщий шаг Мастера (WizardStep)</param>
  /// <param name="args">Аргументы события</param>
  public delegate void WizardEndStepEventHandler(object sender,
    WizardEndStepEventArgs args);

  #endregion

  /// <summary>
  /// Описание для одного шага мастера
  /// </summary>
  public class WizardStep
  {
    #region Конструкторы

    /// <summary>
    /// Создает шаг мастера для заданной панели с управляющими элементами.
    /// При закрытии шага панель не будет удалена.
    /// Эта версия конструктора обычно применяется, когда шаги управляющие элементы мастера располагаются на форме-заготовке
    /// и шаги создаются однократно в сеансе работы мастера
    /// </summary>
    /// <param name="control">Панель с управляющими элементами</param>
    public WizardStep(Panel control)
      : this(control, false)
    {
    }

    /// <summary>
    /// Создает шаг мастера для заданной панели с управляющими элементами.
    /// </summary>
    /// <param name="control">Панель с управляющими элементами</param>
    /// <param name="controlOwning">Должен ли шаг мастера вызовать Control.Dispose(), когда шаг закрывается.
    /// Используйте значение true, если панель с элементами была создана специально для этого шага и не явлляется "многоразовой"</param>
    public WizardStep(Panel control, bool controlOwning)
    {
      if (control == null)
        throw new ArgumentNullException("control");
      if (control.IsDisposed)
        throw new ObjectDisposedException("control");
      control.Dock = DockStyle.Fill;
      _Control = control;
      _ControlOwning = controlOwning;

      _BaseProvider = new EFPBaseProvider();
      ShowImage = true;
      _ForwardEnabled = true;
      _BackEnabled = true;
    }

    /// <summary>
    /// Разрушает Control, если данный объект является его владельцем
    /// </summary>
    internal protected virtual void Close()
    {
      if (_Control != null && _ControlOwning)
      {
        _Control.Dispose();
        _Control = null;
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Управляющий элемент, выводимый в форме
    /// </summary>
    public Panel Control { get { return _Control; } }
    private Panel _Control;

    /// <summary>
    /// True, если управляющий элемент должен быть разрушен вместе с данным шагом.
    /// Используется для шагов, экраны которых не предоставлены формой-хозяином, а
    /// создаются динамически
    /// </summary>
    public bool ControlOwning { get { return _ControlOwning; } }
    private bool _ControlOwning;

    /// <summary>
    /// Провайдер для присоединения управляющих элементов
    /// </summary>
    public EFPBaseProvider BaseProvider { get { return _BaseProvider; } }
    private EFPBaseProvider _BaseProvider;

    /// <summary>
    /// Показывать ли изображение для этого шага? (по умолчанию - true)
    /// Свойство не действует, если Wizard.ShowImage=false;
    /// </summary>
    public bool ShowImage { get { return _ShowImage; } set { _ShowImage = value; } }
    private bool _ShowImage;

    /// <summary>
    /// Должно быть установлено в true для последнего шага мастера
    /// Свойство может устанавливаться динамически в процессе показа шага на
    /// экран, переключая кнопку в нужный режим
    /// </summary>
    public bool FinalStep
    {
      get { return _FinalStep; }
      set
      {
        if (value == _FinalStep)
          return;
        _FinalStep = value;
        if (IsCurrentStep)
          Wizard.TheForm.InitBtnNext();
      }
    }
    private bool _FinalStep;

    /// <summary>
    /// Свойство определяет доступность кнопки "Вперед" или "Готово" (по умолчанию-true)
    /// Свойство, вероятно, должно устанавливаться в процессе показа текущего шага
    /// на экране, в зависимости от состояния пользовательского ввода. 
    /// Также может "жестко" устанавливаться для тупиковых кадров мастера (с выводом
    /// соответствующего сообщения в кадре), если  неправильные данные были обнаружены ранее
    /// </summary>
    public bool ForwardEnabled
    {
      get { return _ForwardEnabled; }
      set
      {
        if (value == _ForwardEnabled)
          return;
        _ForwardEnabled = value;
        if (IsCurrentStep)
          Wizard.TheForm.InitBtnNext();
      }
    }
    private bool _ForwardEnabled;

    /// <summary>
    /// Это свойство опрделяет доступность кнопки "Назад". По умолчанию - true
    /// (шаг назад разрешается). Свойство может быть установлено в false, если 
    /// возврат назад невозможен. Возможна динамическая установка свойства в
    /// процессе показа кадра
    /// Свойство не имеет значения для первого кадра мастера, когда кнопка "Назад"
    /// отключена
    /// </summary>
    public bool BackEnabled
    {
      get { return _BackEnabled; }
      set
      {
        if (value == _BackEnabled)
          return;
        _BackEnabled = value;
        if (IsCurrentStep)
          Wizard.TheForm.InitBtnBack();
      }
    }
    private bool _BackEnabled;

    /// <summary>
    /// Контекст справки для данного шага мастера.
    /// Если свойство не установлено, используется общий контекст справки
    /// </summary>
    public string HelpContext
    {
      get { return _HelpContext; }
      set
      {
        if (Wizard != null)
          throw new InvalidOperationException("Свойство может быть установлено только до присоединения шага к мастеру");
        _HelpContext = value;
      }
    }
    private string _HelpContext;

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    /// <summary>
    /// Мастер, к которому относится шаг. Свойство устанавливатся при первом 
    /// подключении шага к мастеру
    /// </summary>
    public Wizard Wizard { get { return _Wizard; } }
    internal Wizard _Wizard;

    /// <summary>
    /// Свойство возврщае тrue, когда этот шаг является текущим в мастере
    /// </summary>
    public bool IsCurrentStep
    {
      get
      {
        if (_Wizard == null)
          return false; // еще не подключались
        else
          return Wizard.CurrentStep == this;
      }
    }

    #endregion

    #region События

    /// <summary>
    /// Вызывается при открытии кадра
    /// </summary>
    public event WizardBeginStepEventHandler BeginStep;

    /// <summary>
    /// Вызывается при активации шага мастера.
    /// Непереопределенный метод активирует контекст справки, если свойство WizardStep.HelpContext установлено.
    /// Затем вызывается событие BeginStep.
    /// </summary>
    /// <param name="action">Причина открытия</param>
    internal protected virtual void OnBeginStep(WizardAction action)
    {
#if DEBUG
      if (BaseProvider.Parent == null)
        throw new NullReferenceException("BaseProvider.Parent==null");

#endif

      if (!String.IsNullOrEmpty(HelpContext))
        _Wizard.TheForm.efpForm.HelpContext = HelpContext;
      else
        _Wizard.TheForm.efpForm.HelpContext = _Wizard.HelpContext;


      if (BeginStep != null)
      {
        try
        {
          WizardBeginStepEventArgs Args = new WizardBeginStepEventArgs(Wizard, action);
          BeginStep(this, Args);
        }
        catch (Exception e)
        {
          _Wizard.ShowException(e, "Инициализация очередного шага мастера");
        }
      }
    }

    /// <summary>
    /// Вызывается при закрытии кадра
    /// </summary>
    public event WizardEndStepEventHandler EndStep;

    /// <summary>
    /// Завершение шага мастера.
    /// Непереопределенный метод вызывает событие EndStep.
    /// Метод возвращает true, если шаг мастера может быть завершен
    /// </summary>
    /// <param name="action"></param>
    /// <returns>true-шаг можно закрыть. false-есть ошибка и завершение запрещено</returns>
    // ReSharper disable once ArrangeModifiersOrder
    internal protected virtual bool OnEndStep(WizardAction action)
    {
      if (EndStep != null)
      {
        try
        {
          WizardEndStepEventArgs Args = new WizardEndStepEventArgs(Wizard, action);
          Args.Cancel = false;
          EndStep(this, Args);
          return !Args.Cancel;
        }
        catch (UserCancelException)
        {
          return false;
        }
        catch (Exception e)
        {
          _Wizard.ShowException(e, "Завершение текущего шага мастера");
          return false;
        }
      }
      else
        return true;
    }

    /// <summary>
    /// Это событие вызывается при нажатии кнопки "Далее". Обработчик должен быть
    /// у каждого шага, кроме последнего
    /// </summary>
    public event WizardGetNextEventHandler GetNext;

    /// <summary>
    /// Непереопределенный метод вызывает событие GetNext.
    /// </summary>
    /// <returns></returns>
    internal protected virtual WizardStep OnGetNext()
    {
      if (GetNext != null)
      {
        try
        {
          WizardGetNextEventArgs Args = new WizardGetNextEventArgs(Wizard);
          GetNext(this, Args);
          return Args.NextStep;
        }
        catch (UserCancelException)
        {
          return null;
        }
        catch (Exception e)
        {
          _Wizard.ShowException(e, "Получение следующего шага мастера");
          return null;
        }
      }
      else
        return null;
    }

    #endregion
  }

  /// <summary>
  /// Описание для временной страницы мастера (без процетного индикатора)
  /// </summary>
  public class WizardTempPage
  {
    #region Конструкторы

    /// <summary>
    /// Создает временную страницу
    /// </summary>
    /// <param name="control">Панель для размещения содержимого</param>
    public WizardTempPage(Panel control)
      : this(control, false)
    {
    }

    /// <summary>
    /// Создает временную страницу
    /// </summary>
    /// <param name="control">Панель для размещения содержимого</param>
    /// <param name="controlOwning">True, если управляющий элемент должен быть разрушен вместе с данным шагом.
    /// Используется для шагов, экраны которых не предоставлены формой-хозяином, а
    /// создаются динамически</param>
    public WizardTempPage(Panel control, bool controlOwning)
    {
      if (control == null)
        throw new ArgumentNullException("control");
      if (control.IsDisposed)
        throw new ObjectDisposedException("control");
      _Control = control;
      _ControlOwning = controlOwning;

      _BaseProvider = new EFPBaseProvider();
    }

    /// <summary>
    /// Разрушает Control, если данный объект является его владельцем
    /// </summary>
    internal protected virtual void Close()
    {
      if (_Control != null && _ControlOwning)
      {
        _Control.Dispose();
        _Control = null;
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Управляющий элемент, выводимый в форме
    /// </summary>
    public Panel Control { get { return _Control; } }
    private Panel _Control;

    /// <summary>
    /// True, если управляющий элемент должен быть разрушен вместе с данным шагом.
    /// Используется для шагов, экраны которых не предоставлены формой-хозяином, а
    /// создаются динамически
    /// </summary>
    public bool ControlOwning { get { return _ControlOwning; } }
    private bool _ControlOwning;

    /// <summary>
    /// Провайдер для присоединения управляющих элементов
    /// </summary>
    public EFPBaseProvider BaseProvider { get { return _BaseProvider; } }
    private EFPBaseProvider _BaseProvider;

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    /// <summary>
    /// Мастер, к которому относится временный кадр. Свойство устанавливатся только
    /// на время вывода этой временной страницы
    /// </summary>
    public Wizard Wizard { get { return _Wizard; } }
    internal Wizard _Wizard;

    /// <summary>
    /// Доступность кнопки "Отмена"
    /// </summary>
    public bool CancelEnabled
    {
      get { return _CancelEnabled; }
      set
      {
        if (value == _CancelEnabled)
          return;
        _CancelEnabled = true;
        if (Wizard != null)
          Wizard.TheForm.InitBtnCancel();
      }
    }
    private bool _CancelEnabled;

    /// <summary>
    /// Предыдущая временная страница.
    /// Свойство используется, если открывается несколько временных страниц подряд
    /// без закрытия предыдущей страницы
    /// </summary>
    internal WizardTempPage PrevTempPage;

    #endregion

    #region События

    /// <summary>
    /// Вызывается при нажатии кнопки "Отмена"
    /// </summary>
    public event EventHandler CancelClick;

    /// <summary>
    /// Вызывает обработчик события CancelClick, если он установлен.
    /// Если CancelEnabled=false, то событие не вызывается.
    /// </summary>
    internal protected virtual void OnCancelClick()
    {
#if DEBUG
      if (BaseProvider.Parent == null)
        throw new NullReferenceException("BaseProvider.Parent==null");

#endif
      if (CancelClick != null && CancelEnabled)
      {
        try
        {
          CancelClick(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
          _Wizard.ShowException(e, "Ошибка при обработке нажатия кнопки \"Отмена\"");
        }
      }
    }

    #endregion
  }

  internal class WizardSplashPage : WizardTempPage
  {
    #region Конструктор

    public WizardSplashPage(string[] items)
      : this(items, new SplashForm())
    {
      // Не знаю, как обойтись без фиктивного конструктора
    }

    internal WizardSplashPage(string[] items, SplashForm form)
      : base(form.MainPanel, false) // сами удалим
    {
      _Splash = new Splash2(items, form, this);
    }

    protected internal override void Close()
    {
      base.Close();
      _Splash.Close();
      _Splash = null;
    }

    #endregion

    #region Вложенный класс

    private class Splash2 : Splash
    {
      #region Конструктор

      public Splash2(string[] items, SplashForm form, WizardSplashPage page)
        : base(items, form, true)
      {
        _Page = page;
      }

      #endregion

      #region Свойства

      private WizardSplashPage _Page;

      #endregion

      #region Переопределенный метод

      public new bool Cancelled
      {
        get { return base.Cancelled; }
        set { base.Cancelled = value; } // оригинальный метод - protected
      }

      protected override void InitCancelButton()
      {
        _Page.CancelEnabled = AllowCancel;
      }

      #endregion
    }

    #endregion

    #region Свойства

    public ISplash Splash { get { return _Splash; } }
    private Splash2 _Splash;

    #endregion

    #region Переопределенные методы

    protected internal override void OnCancelClick()
    {
      _Splash.Cancelled = true;
      base.OnCancelClick();
    }

    #endregion
  }
}