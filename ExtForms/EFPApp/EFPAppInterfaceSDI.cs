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
      Form DummyForm = new Form();
      DummyForm.StartPosition = FormStartPosition.WindowsDefaultBounds;
      ToolStripContainer StripContainer = new ToolStripContainer();
      StripContainer.Dock = DockStyle.Fill;
      StripContainer.ContentPanel.BackColor = SystemColors.AppWorkspace;

      StatusStrip TheStatusBar = new System.Windows.Forms.StatusStrip();
      StripContainer.BottomToolStripPanel.Controls.Add(TheStatusBar);

      _CascadeHelper.SetStartPosition(DummyForm, EFPApp.DefaultScreen.WorkingArea);  // обязательно до AddMainWindow()

      EFPAppMainWindowLayoutSDI Layout = new EFPAppMainWindowLayoutSDI(DummyForm, false);
      base.AddMainWindow(Layout);

      Layout.MainWindow.Show();

      // Пустышка не попадает в список дочерних окон

      return Layout;
    }


    private FormStartPositionCascadeHelper _CascadeHelper;

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
    /// <param name="form"></param>
    /// <returns></returns>
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

      StatusStrip TheStatusBar = new System.Windows.Forms.StatusStrip();
      EFPApp.SetStatusStripHeight(TheStatusBar, form); // 16.06.2021
      pd.StripContainer.BottomToolStripPanel.Controls.Add(TheStatusBar);
      form.Controls.Add(TheStatusBar);

      //TheStatusBar.Items.Add("Статусная строка SDI");

      // Умещаем на экран
      _CascadeHelper.SetStartPosition(form, EFPApp.DefaultScreen.WorkingArea); // обязательно до AddMainWindow()

      pd.Layout = new EFPAppMainWindowLayoutSDI(form, true);
      base.AddMainWindow(pd.Layout);

      return pd;
    }

    /// <summary>
    /// Показ дочернего окна.
    /// Создает ToolStripContainer, в центральную панель которого переносит все существующие
    /// дочерние элементы формы
    /// </summary>
    /// <param name="form">Созданная в пользовательском коде форма</param>
    /// <param name="preparationData">Внутренние данные</param>
    protected override void OnShowChildForm(Form form, object preparationData)
    {
      PreparationData pd = (PreparationData)preparationData;

      pd.Layout.MainWindow.Show();

      // Делаем, чтобы размеры сохранились
      EFPFormProvider formProvider = EFPFormProvider.FindFormProviderRequired(form);
      if ((!pd.SrcSize.IsEmpty) && (formProvider.ReadConfigFormBoundsParts & EFPFormBoundsPart.Size) == 0)
      {
        // Должно быть после вызова AddMainWindow(), т.к. там добавляется статусная строка и панели
        Size NewSize = pd.StripContainer.ContentPanel.ClientSize;
        int dx = pd.SrcSize.Width - NewSize.Width;
        int dy = pd.SrcSize.Height - NewSize.Height;
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
      throw new NotImplementedException("Размещение окон интерфейса SDI не реализовано");
    }

    #endregion
  }

  /// <summary>
  /// Расширение EFPAppMainWindowLayout для интерфейса SDI.
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
