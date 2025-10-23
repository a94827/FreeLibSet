// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Интерфейс пользователя, в котором все окна располагаются непосредственно на рабочем столе
  /// Windows и имеют собственное главное меню, панели инструментов и статусную строку.
  /// Обычно используется для программ, в которых есть только одно окно для документа.
  /// Свойство Name возвращает значение "SDI".
  /// </summary>
  public class EFPAppInterfaceSDI : EFPAppInterface
  {
    #region Конструктор

    /// <summary>
    /// Создает интерфейс
    /// </summary>
    public EFPAppInterfaceSDI()
    {
      _CascadeHelper = new FormStartPositionCascadeHelper();
    }

    #endregion

    #region Характеристики интерфейса

    /// <summary>
    /// Возвращает "SDI"
    /// </summary>
    public override string Name { get { return "SDI"; } }

    /// <summary>
    /// Возвращает true.
    /// </summary>
    public override bool IsSDI { get { return true; } }

    /// <summary>
    /// Возвращает false
    /// </summary>
    public override bool MainWindowNumberUsed { get { return false; } }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Ничего не делает
    /// </summary>
    internal protected override void InitMainWindowTitles()
    {
      // Ничего не делаем. Заголовок определен в момент присоединения формы и больше не меняется
    }

    /// <summary>
    /// Создает окно - "пустышку"
    /// </summary>
    /// <returns></returns>
    public override EFPAppMainWindowLayout ShowMainWindow()
    {
      Form dummyForm = new Form();
      dummyForm.StartPosition = FormStartPosition.WindowsDefaultBounds;
      ToolStripContainer stripContainer = new ToolStripContainer();
      stripContainer.Dock = DockStyle.Fill;
      stripContainer.ContentPanel.BackColor = SystemColors.AppWorkspace;

      StatusStrip theStatusBar = new System.Windows.Forms.StatusStrip();
      stripContainer.BottomToolStripPanel.Controls.Add(theStatusBar);

      _CascadeHelper.SetStartPosition(dummyForm, EFPApp.DefaultScreen.WorkingArea);  // обязательно до AddMainWindow()

      EFPAppMainWindowLayoutSDI layout = new EFPAppMainWindowLayoutSDI(dummyForm, false);
      base.AddMainWindow(layout);

      EFPApp.SystemMethods.Show(layout.MainWindow, null);

      // Пустышка не попадает в список дочерних окон

      return layout;
    }


    private readonly FormStartPositionCascadeHelper _CascadeHelper;

    private class PreparationData
    {
      #region Поля

      internal EFPAppMainWindowLayoutSDI Layout;

      internal FormWindowState SrcState;

      internal Size SrcSize;

      internal ToolStripContainer StripContainer;

      #endregion
    }

    /// <summary>
    /// Подготовка к просмотру
    /// </summary>
    /// <param name="form">Форма, которая будет показана</param>
    /// <returns>Внутренние данные</returns>
    protected override object OnPrepareChildForm(Form form)
    {
      PreparationData pd = new PreparationData();

      pd.SrcState = form.WindowState;
      form.WindowState = FormWindowState.Normal;
      pd.SrcSize = Size.Empty;
      if (form.StartPosition != FormStartPosition.WindowsDefaultBounds)
        pd.SrcSize = form.ClientSize;

      pd.StripContainer = new ToolStripContainer();
      pd.StripContainer.Dock = DockStyle.Fill;
      WinFormsTools.MoveControls(form, pd.StripContainer.ContentPanel);
      form.Controls.Add(pd.StripContainer);

      //base.PrepareChildForm(Form);

      StatusStrip theStatusBar = new System.Windows.Forms.StatusStrip();
      EFPApp.SetStatusStripHeight(theStatusBar, form); // 16.06.2021
      pd.StripContainer.BottomToolStripPanel.Controls.Add(theStatusBar);
      form.Controls.Add(theStatusBar);

      //TheStatusBar.Items.Add("Статусная строка SDI");

      string displayName;
      ToolStrip localToolStrip = MoveSingleToolBar(form, pd.StripContainer.TopToolStripPanel, out displayName);

      // Умещаем на экран
      _CascadeHelper.SetStartPosition(form, EFPApp.DefaultScreen.WorkingArea); // обязательно до AddMainWindow()

      pd.Layout = new EFPAppMainWindowLayoutSDI(form, true);
      if (localToolStrip != null)
      {
        pd.Layout.LocalToolBar = new EFPAppToolBar("CurrentForm", localToolStrip);
        pd.Layout.LocalToolBar.DisplayName = displayName;
      }

      form.WindowState = pd.SrcState; // на всякий случай
      base.AddMainWindow(pd.Layout);
      pd.SrcState = form.WindowState; // 26.02.2025 EFPAppInterface.AddMainWindow() может установить состояние Maximized

      return pd;
    }

    /// <summary>
    /// Перенос единственной панели инструментов
    /// </summary>
    private static ToolStrip MoveSingleToolBar(Form form, ToolStripPanel stripPanel, out string displayName)
    {
      displayName = null;
      IEFPControlWithToolBar formCWT = form as IEFPControlWithToolBar; // SimpleForm, например
      if (formCWT == null)
        return null;
      if (formCWT.ToolBarPanel == null)
        return null;
      Control[] a = WinFormsTools.GetControls<Control>(formCWT.ToolBarPanel, false);
      if (a.Length != 2) // ToolBarPanel и ToolStrip в нем
        return null;
      ToolStrip strip = a[1] as ToolStrip;
      if (strip == null)
        return null;
      //if (!strip.Visible)
      //  return;

      strip.GripStyle = ToolStripGripStyle.Visible;
      //strip.Parent = stripPanel;
      strip.Parent = null;
      stripPanel.Controls.Add(strip);
      formCWT.ToolBarPanel.Visible = false;

      displayName = Res.EFPApp_Name_FormToolBar;
      if (formCWT.BaseProvider.ControlProviders.Count > 0)
        displayName = formCWT.BaseProvider.ControlProviders[0].DisplayName;

      return strip;
    }

    /// <summary>
    /// Показ дочернего окна.
    /// Создает <see cref="ToolStripContainer"/>, в центральную панель которого переносит все существующие
    /// дочерние элементы формы
    /// </summary>
    /// <param name="form">Созданная в пользовательском коде форма</param>
    /// <param name="preparationData">Внутренние данные</param>
    protected override void OnShowChildForm(Form form, object preparationData)
    {
      PreparationData pd = (PreparationData)preparationData;

      EFPApp.SystemMethods.Show(pd.Layout.MainWindow, null);

      // Делаем, чтобы размеры сохранились
      EFPFormProvider formProvider = EFPFormProvider.FindFormProviderRequired(form);
      if ((!pd.SrcSize.IsEmpty) && (formProvider.ReadConfigFormBoundsParts & EFPFormBoundsPart.Size) == 0)
      {
        form.WindowState = FormWindowState.Normal; // 26.02.2025
        // Должно быть после вызова AddMainWindow(), т.к. там добавляется статусная строка и панели
        Size newSize = pd.StripContainer.ContentPanel.ClientSize;
        int dx = pd.SrcSize.Width - newSize.Width;
        int dy = pd.SrcSize.Height - newSize.Height;
        form.Size = new Size(form.Size.Width + dx,
          form.Size.Height + dy);
      }

      WinFormsTools.PlaceFormInRectangle(form, EFPApp.DefaultScreen.WorkingArea); // еще раз, с учетом добавленных панелей

      form.WindowState = pd.SrcState;
    }

    /// <summary>
    /// Выбрасывает исключение
    /// </summary>
    /// <param name="mdiLayout"></param>
    public override void LayoutChildForms(MdiLayout mdiLayout)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// Расширение <see cref="EFPAppMainWindowLayout"/> для интерфейса SDI.
  /// </summary>
  public sealed class EFPAppMainWindowLayoutSDI : EFPAppMainWindowLayout
  {
    #region Защищенный конструктор

    internal EFPAppMainWindowLayoutSDI(Form form, bool isRealForm)
    {
      base.MainWindow = form;
      if (isRealForm)
        base.PrepareChildForm(form);
    }

    #endregion
  }
}
