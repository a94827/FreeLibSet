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
  /// Многодокументный интерфейс MDI.
  /// Используется форма главного окна с установленным свойством Form.IsMdiContainer=true.
  /// Для дочерних окон устанавливается свойство Form.MdiParent
  /// </summary>
  public class EFPAppInterfaceMDI : EFPAppInterface
  {
    #region Конструктор

    /// <summary>
    /// Создает объект интерфейса MDI
    /// </summary>
    public EFPAppInterfaceMDI()
    {
    }

    #endregion

    #region Характеристики интерфейса

    /// <summary>
    /// Возвращает "MDI"
    /// </summary>
    public override string Name { get { return "MDI"; } }


    #endregion

    #region Главное окно

    /// <summary>
    /// Создает главное окно программы
    /// </summary>
    /// <returns></returns>
    public override EFPAppMainWindowLayout ShowMainWindow()
    {
      EFPAppMainWindowLayoutMDI Layout = new EFPAppMainWindowLayoutMDI(ObsoleteMode);

      base.AddMainWindow(Layout);

      Layout.MainWindow.Show();

      return Layout;
    }

    /// <summary>
    /// Возвращает текущее главное окно.
    /// Обычно в программе с интерфейсом MDI есть только одно главное окно,
    /// но существует возможность создать дополнительные главные окна, что удобно при наличии нескольких мониторов.
    /// </summary>
    public new EFPAppMainWindowLayoutMDI CurrentMainWindowLayout
    {
      get { return (EFPAppMainWindowLayoutMDI)(base.CurrentMainWindowLayout); }
    }

    #endregion

    #region Дочерняя форма

    /// <summary>
    /// Подготовка к просмотру
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    protected override object OnPrepareChildForm(Form form)
    {
      if (CurrentMainWindowLayout == null)
        ShowMainWindow();
      CurrentMainWindowLayout.PrepareChildForm(form);

      return null;
    }

    /// <summary>
    /// Вывод дочерней формы.
    /// Если в данный момент нет ни одного главного окна (что является ошибкой),
    /// то создается пустое главное окно.
    /// </summary>
    /// <param name="form">Выводимая дочерняя форма</param>
    /// <param name="preparationData">Внутренние данные</param>
    protected override void OnShowChildForm(Form form, object preparationData)
    {
      CurrentMainWindowLayout.ShowChildForm(form);
    }

    /// <summary>
    /// Определяет принципиальную возможность размещения дочерних окон.
    /// Определяет видимость команд главного меню "Окно".
    /// Доступность команд определяется отдельно.
    /// </summary>
    /// <param name="mdiLayout">Предполагаемое выравнивание</param>
    /// <returns>Видимость команды</returns>
    public override bool IsLayoutChildFormsSupported(MdiLayout mdiLayout)
    {
      switch (mdiLayout)
      {
        case MdiLayout.TileHorizontal:
        case MdiLayout.TileVertical:
        case MdiLayout.Cascade:
        case MdiLayout.ArrangeIcons:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Возвращает true, если выравнивание дочерних окон применимо к текущей композиции.
    /// Используется для определения видимости команд главного меню "Окно"
    /// </summary>
    /// <param name="mdiLayout">Тип расположения окон</param>
    /// <returns>Доступность размещения</returns>
    public override bool IsLayoutChildFormsAppliable(MdiLayout mdiLayout)
    {
      if (CurrentMainWindowLayout == null)
        return false;

      if (CurrentMainWindowLayout.ChildFormCount == 0)
        return false;
      if (mdiLayout == MdiLayout.ArrangeIcons)
      {
        Form[] Forms = CurrentMainWindowLayout.GetChildForms(false);
        for (int i = 0; i < Forms.Length; i++)
        {
          if (Forms[i].WindowState == FormWindowState.Minimized)
            return true;
        }
        return false;
      }

      return true;
    }

    #endregion
  }

  /// <summary>
  /// Управляющий объект для главного окна MDI.
  /// </summary>
  public sealed class EFPAppMainWindowLayoutMDI: EFPAppMainWindowLayout
  {
    #region Защищенный конструктор

    internal EFPAppMainWindowLayoutMDI(bool obsoleteMode)
    {
      base.MainWindow = new Form();
      base.MainWindow.BackColor = System.Drawing.SystemColors.AppWorkspace;
      base.MainWindow.IsMdiContainer = true;

      EFPAppMainWindowLayout.DecorateMainWindow(MainWindow);

      _ObsoleteMode = obsoleteMode;

      base.Bounds = new EFPFormBounds();

      _CascadeHelper = new FormStartPositionCascadeHelper();
    }

    #endregion

    #region Главное окно

    private bool _ObsoleteMode;

    #endregion

    #region Дочерние формы

    private FormStartPositionCascadeHelper _CascadeHelper;


    internal new void PrepareChildForm(Form form)
    {
      form.MdiParent = MainWindow;
      CorrectMdiChildFormIcon(form);

      base.PrepareChildForm(form);
    }

    /// <summary>
    /// Исправление большого значка
    /// </summary>
    /// <param name="form">Форма MDI child</param>
    private static void CorrectMdiChildFormIcon(Form form)
    {
      // 22.01.2015
      // Корректировка значка.
      // Проблема:
      // Если форме присвоен значок с размером, большим 16x16, дочернее MDI-окно правильно отображается, пока оно не максимизировано.
      // Если окно развернуто, то главное меню отображает в левом углу полноразмерный значок, увеличивая высоту меню
      // Решение:
      // Уменьшаем размер значка до 16x16. Если в оригинальном Form.Icon нет значка подходящего размера, выполняем преобразование Icon->Bitmap,
      // масштабируем до нужного размера, затем преобразуем обратно Bitmap->Icon
      if (form.Icon == null)
        return;

      Size sis = SystemInformation.SmallIconSize;
      if (form.Icon.Width > sis.Width || form.Icon.Height > sis.Height)
      {
        Icon NewIcon = new Icon(form.Icon, sis);

        if (NewIcon.Size.Width != sis.Width || NewIcon.Size.Height != sis.Height)
        {
          Bitmap bmp = NewIcon.ToBitmap();
          NewIcon.Dispose();
          Bitmap bmp2 = new Bitmap(bmp, sis); // масщтабирование
          bmp.Dispose();
          NewIcon = Icon.FromHandle(bmp2.GetHicon());
          bmp2.Dispose();
        }
        form.Icon = NewIcon;
        //NewIcon.Dispose();
      }

    }

    /// <summary>
    /// Добавляет дочернее окно в окно контейнера MDI.
    /// Выполняется автоматическое позиционирования окон "лесенкой"
    /// </summary>
    /// <param name="form">Дочерняя форма</param>
    internal void ShowChildForm(Form form)
    {
      #region Глюк со значком

      // Не имеет отношения к CorrectMdiChildFormIcon()

      /*
       * 13.08.2008
       * Если ничего не делать, то возникает проблема. Когда форма открывается в
       * развернутом режиме или если предыдущее дочернее окно развернуто (при этом
       * открываемая форма также будет развернута, что соответствует правилам 
       * интерфейса MDI), неправильно отображается значок формы. Вместо значка,
       * определенного свойством TheForm.Icon в левой части главного меню будет 
       * нарисован значок по умолчанию.
       * Решение.
       * На время присоединения формы к интерфесу MDI нужно установить свойство
       * FormBorderStyle в значение FixedDialog, а затем вернуть его обратно.
       * 
       * Это глюк, который должен был быть исправлен в .NET Framework, но так и
       * не исправлен до сих пор.
       * 
       * Источник сведений:
       * https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=106264
       */

      bool SetMaximized = false;
      bool CanMaximize = form.FormBorderStyle == FormBorderStyle.Sizable && form.MaximizeBox;

      // Проверка на form.WindowState может быть только в ShowChildForm(), но не в PrepareChildForm().
      // Пользовательские настройки считываются между этими двумя вызовами.
      if (CanMaximize && form.WindowState == FormWindowState.Maximized)
        // Наше окно будет развернуто
        SetMaximized = true;

      if (MainWindow.ActiveMdiChild != null)
      {
        if (MainWindow.ActiveMdiChild.WindowState == FormWindowState.Maximized)
        {
          if (CanMaximize)
            // Предыдущее дочернее окно будет развернуто, следовательно, наше окно
            // (если это не запрещено) тоже станет развернутым
            SetMaximized = true;
          else
            // Следует запретить автоматическую разветку нашего окна
            // Для этого нужно отменить развертку предыдущего окна
            MainWindow.ActiveMdiChild.WindowState = FormWindowState.Normal;
        }
      }

      if (SetMaximized)
      {
        // 25.08.2008
        // Если просто устанавливать стиль рамки, то все работает для окон, созданных
        // "вручную". Формы, создаваемые в дизайнере, имеют установленное свойство
        // ClientSize (вместо Size). Такие формы отображаются в развернутом режиме в
        // левом верхнем углу области MDI, хотя находятся в развернутом состоянии
        // Чтобы обойти и этот дефект, сначала делаем форму не максимизированной, а
        // затем - разворачиваем. Т.к. на данный момент форма еще не подключена, то
        // никаких неприятных видеоэффектов нет

        form.WindowState = FormWindowState.Normal;
        form.FormBorderStyle = FormBorderStyle.Fixed3D;
        form.WindowState = FormWindowState.Maximized;
      }

      #endregion

      if (ChildFormCount == 1) // форма уже добавлена в список
        _CascadeHelper.ResetStartPosition(); // 13.09.2021

      // 09.06.2021 Перенесено вниз, после вызова Form.Show()
      // 14.06.2021 Перенесено обратно, до показа формы, чтобы окно не прыгало
      Rectangle Area = WinFormsTools.GetMdiContainerArea(MainWindow); // доступная область. Левый верхний угол имеет координаты (0,0)
      _CascadeHelper.SetStartPosition(form, Area);

      form.Show();

      if (SetMaximized)
        form.FormBorderStyle = FormBorderStyle.Sizable; // восстанавливаем рамку обратно после вызова Show()
    }

    /// <summary>
    /// Упорядочивает дочерние окна для главного окна MDI
    /// </summary>
    /// <param name="mdiLayout">Способ упорядочения</param>
    public override void LayoutChildForms(MdiLayout mdiLayout)
    {
      MainWindow.LayoutMdi(mdiLayout);
    }

    #endregion


    /// <summary>
    /// Рисование главного окна для изображения предварительного просмотра.
    /// Стандартный метод Control.DrawToBitmap() умеет рисовать только окно контейнера MDI,
    /// но не его дочерних окон.
    /// Переопредаленный метод выполняет ручную прорисовку окон с учетом их порядка размещения
    /// в контейнере.
    /// </summary>
    /// <param name="bitmap">Изображение, на котором выполняется рисование</param>
    /// <param name="area">Область, куда требуется вписать изображение</param>
    /// <param name="forComposition">True, если требуется нарисовать только те дочерние окна,
    /// которые умеют сохранять собственную композицию между сеансами.
    /// Если false, то будут нарисованы все окна</param>
    internal protected override void DrawMainWindowSnapshot(Bitmap bitmap, Rectangle area, bool forComposition)
    {
      MainWindow.DrawToBitmap(bitmap, area);

      // MDI-окно рисуется в неправильном порядке. Сначала - верхнее окно, затем те, которые под ним
      // Надо рисовать только нужные окна
      // Кроме того, не надо рисовать окна, которые не входят в композицию
      ClearWorkspaceArea(bitmap, area);

      // Рисуем повторно дочерние окна
      Form[] Forms = GetChildForms(true);
      for (int i = Forms.Length - 1; i >= 0; i--)
      {
        if (EFPApp.FormWantsSaveComposition(Forms[i]))
        {
          Rectangle rc2 = Forms[i].RectangleToScreen(area);
          Rectangle rc3 = MainWindow.RectangleToClient(rc2);
          Forms[i].DrawToBitmap(bitmap, rc3);
        }
      }
    }

    private void ClearWorkspaceArea(Bitmap bitmap, Rectangle area)
    {
      Point pt0 = MainWindow.ClientRectangle.Location; // всегда (0,0)
      pt0 = MainWindow.PointToScreen(pt0);
      pt0 = new Point(pt0.X - MainWindow.Left, pt0.Y - MainWindow.Top); // клиентная область относительно верхнего левого угла формы


      Rectangle rc1 = WinFormsTools.GetControlDockFillArea(MainWindow); // относительно MainWindow
      // Rectangle rc2 = MainWindow.RectangleToScreen(rc1); // относительно экрана
      Point pt2 = new Point(area.Left + pt0.X + rc1.Left,
        area.Top + pt0.Y + rc1.Top);
      Rectangle rc3 = new Rectangle(pt2, rc1.Size); // относительно Area
      using (Graphics g = Graphics.FromImage(bitmap))
      {
        g.FillRectangle(SystemBrushes.AppWorkspace, rc3);
      }
    }

  }
}
