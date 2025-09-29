// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using FreeLibSet.Config;

namespace FreeLibSet.Forms
{
  /*
   * Использование EFPFormBounds
   * Объект EFPFormProvider содержит ссылку на EFPFormBounds. По умолчанию ссылка равна null, и форма
   * показывается в исходном положении, которое задано в свойствах формы.
   * Свойство Bounds может быть установлено до вывода формы на экран или после. При этом меняются свойства 
   * Form.Location, Form.Size, Form.WindowState и Form.StartPosition.
   * Если свойство не установлено, объект EFPFormBounds создается при показе формы, и его свойства устанавливаются
   * в соответствии реальным положением формы.
   * В дальнейшем, пока форма выведена на экран, обрабатываются события формы.
   * 
   * Для сохранения положения блока диалога между вызовами, следует создать статическую ссылку на 
   * EFPFormBounds, первоначально содержащую null. Перед показом диалога устанавливать значение свойства
   * EFPFormProvider.Bounds
   * 
   * Разрешение экрана.
   * Если при работе программы меняется разрешение экрана (свойства Screen.Bounds),
   * то восстановление положения формы не должно выполняться
   * Также восстановление не выполняется, если главное окно программы перемещено на другой монитор.
   * Соответственно, восстановление НЕ выполняется если изменилось:
   * - Screen.FromControl(MainForm).Bounds
   * - Screen.FromRectangle(Bounds).Bounds
   */

  /// <summary>
  /// Части, используемые в <see cref="EFPFormBounds"/>
  /// </summary>
  [Flags]
  public enum EFPFormBoundsPart
  {
    /// <summary>
    /// Нет
    /// </summary>
    None = 0,

    /// <summary>
    /// Положение формы <seealso cref="System.Windows.Forms.Form.Location"/>
    /// </summary>
    Location = 1,

    /// <summary>
    /// Размер формы <seealso cref="System.Windows.Forms.Form.Size"/>
    /// </summary>
    Size = 2,

    /// <summary>
    /// Состояние Normal/Maximized/Minimized <seealso cref="System.Windows.Forms.Form.WindowState"/>
    /// </summary>
    WindowState = 4,

    /// <summary>
    /// Все части
    /// </summary>
    All = Location | Size | WindowState
  }


  /// <summary>
  /// Объект для сохранения размеров и положения формы между вызовами.
  /// Сохраняет размер, положение формы и состояние формы (обычное, максимизировано, свернуто).
  /// </summary>
  public class EFPFormBounds
  {
    #region Конструктор

    /// <summary>
    /// Создает незаполненный объект
    /// </summary>
    public EFPFormBounds()
    {
    }

    #endregion

    #region Свойства формы

    /// <summary>
    /// Сохраненные размеры формы в состоянии <see cref="System.Windows.Forms.FormWindowState.Normal"/>.
    /// Если форма находится в максимизированном состоянии или свернута, свойство содержит <see cref="System.Windows.Forms.Form.RestoreBounds"/>
    /// </summary>
    public Rectangle Bounds
    {
      get { return _Bounds; }
      set { _Bounds = value; }
    }
    private Rectangle _Bounds;

    /// <summary>
    /// Сохраненный признак максимизации
    /// </summary>
    public FormWindowState WindowState
    {
      get { return _WindowState; }
      set
      {
        _WindowState = value;
      }
    }
    private FormWindowState _WindowState;

    /// <summary>
    /// Возвращает true, если объект не инициализирован.
    /// </summary>
    public bool IsEmpty { get { return _Bounds.IsEmpty; } }

    /// <summary>
    /// Возвращает текстовое представление с установленными координатами и состоянием окна
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (IsEmpty)
        return "{Empty}";
      return Bounds.ToString() + ", WindowState=" + WindowState.ToString();
    }

    #endregion

    #region Чтение / запись положения формы

    /// <summary>
    /// Устанавливает значения свойств этого объекта в соответствии с формой
    /// </summary>
    /// <param name="form">Форма, откуда извлекаются значения</param>
    public void FromControl(Form form)
    {
      if (form == null)
        throw new ArgumentNullException("form");

      if (form.WindowState == FormWindowState.Normal)
        _Bounds = form.Bounds;
      else
        _Bounds = form.RestoreBounds;
      _WindowState = form.WindowState;
    }

    /// <summary>
    /// Инициализация свойств формы в соответствии с данными текущего объекта
    /// </summary>
    /// <param name="form">Форма, положение которой устанавливается</param>
    /// <returns>Флажки размера/положения/состояния, которые были применены к форме</returns>
    public EFPFormBoundsPart ToControl(Form form)
    {
      return ToControl(form, EFPFormBoundsPart.All);
    }

    /// <summary>
    /// Инициализация свойств формы в соответствии с данными текущего объекта
    /// </summary>
    /// <param name="form">Форма, положение которой устанавливается</param>
    /// <param name="parts">Флажки применения положения, размеров и состояния формы.
    /// Некоторые флажки могут быть проигнорированы, например, <see cref="EFPFormBoundsPart.Size"/>, если форма не является sizeable</param>
    /// <returns>Подмножество флажков из <paramref name="parts"/>, которые были применены к форме</returns>
    public EFPFormBoundsPart ToControl(Form form, EFPFormBoundsPart parts)
    {
      if (form == null)
        throw new ArgumentNullException("form");

      EFPFormBoundsPart resParts = EFPFormBoundsPart.None;

      if (IsEmpty)
        return resParts;

      CorrectBounds(form);

      // Исправлено 13.09.2021
      if ((parts & EFPFormBoundsPart.Size) !=0)
      {
        if (form.StartPosition==FormStartPosition.WindowsDefaultBounds)
          form.StartPosition =FormStartPosition.WindowsDefaultLocation;
      }
      if ((parts & EFPFormBoundsPart.Location) != 0)
      {
        form.StartPosition = FormStartPosition.Manual;
      }

      FormWindowState orgState = form.WindowState;

      EFPFormBoundsPart boundsparts = parts & (EFPFormBoundsPart.Location | EFPFormBoundsPart.Size);

      if (boundsparts != 0)
      {
        form.WindowState = FormWindowState.Normal;

//#if DEBUG
//        if ((boundsparts & EFPFormBoundsPart.Location) != 0)
//        {
//          if (Bounds.Location.X < (-Screen.PrimaryScreen.Bounds.Width) ||
//            Bounds.Location.Y < (-Screen.PrimaryScreen.Bounds.Height))
//          {
//          }
//        }
//#endif

        switch (form.FormBorderStyle)
        {
          case FormBorderStyle.Sizable:
          case FormBorderStyle.SizableToolWindow:
            if (boundsparts == (EFPFormBoundsPart.Location | EFPFormBoundsPart.Size))
              form.Bounds = Bounds;
            else if (boundsparts == EFPFormBoundsPart.Location)
              form.Location = Bounds.Location;
            else
              form.Size = Bounds.Size;
            resParts |= boundsparts;
            break;
          default: // только положение, но не размеры
            if ((parts & EFPFormBoundsPart.Location) != 0)
            {
              form.Location = Bounds.Location;
              resParts |= EFPFormBoundsPart.Location;
            }
            break;
        }
      }

      bool stateSetFlag = false;

      if ((parts & EFPFormBoundsPart.WindowState) != 0)
      {
        if (form.MaximizeBox || form.MinimizeBox)
        {
          if (WindowState == FormWindowState.Maximized && form.MaximizeBox)
          {
            form.WindowState = FormWindowState.Maximized;
            stateSetFlag = true;
            resParts |= EFPFormBoundsPart.WindowState;
          }
          else if (WindowState == FormWindowState.Minimized && form.MinimizeBox)
          {
            form.WindowState = FormWindowState.Minimized;
            stateSetFlag = true;
            resParts |= EFPFormBoundsPart.WindowState;
          }
          else
          {
            form.WindowState = FormWindowState.Normal;
            stateSetFlag = true;
          }
        }
      }
      if (!stateSetFlag)
        form.WindowState = orgState;

      return resParts;
    }

    #endregion

    #region Чтение и запись секции конфигурации

    /// <summary>
    /// Записывает размеры, положение и состояние формы
    /// </summary>
    /// <param name="cfg">Секция для записи данных.
    /// Как правило, должны использоваться разные секции для формы в модальном и немодальном режимах</param>
    public void WriteConfig(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      cfg.SetInt32("Left", Bounds.Left);
      cfg.SetInt32("Top", Bounds.Top);
      cfg.SetInt32("Width", Bounds.Width);
      cfg.SetInt32("Height", Bounds.Height);
      cfg.SetEnum<FormWindowState>("State", WindowState);
    }

    /// <summary>
    /// Считывает размеры, положение и состояние формы
    /// </summary>
    /// <param name="cfg">Секция для записи данных.
    /// Как правило, должны использоваться разные секции для формы в модальном и немодальном режимах</param>
    public void ReadConfig(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      int l = cfg.GetInt32("Left");
      int t = cfg.GetInt32("Top");
      int w = cfg.GetInt32("Width");
      int h = cfg.GetInt32("Height");
      this.Bounds = new Rectangle(l, t, w, h);
      WindowState = cfg.GetEnumDef<FormWindowState>("State", this.WindowState);
    }

    #endregion

    #region Корректировка размеров

    private void CorrectBounds(Form form)
    {
      Size minSize;
      switch (form.FormBorderStyle)
      {
        case FormBorderStyle.Sizable:
        case FormBorderStyle.SizableToolWindow:
          minSize = WinFormsTools.Max(form.MinimumSize, SystemInformation.MinimumWindowSize);
          break;
        default:
          minSize = form.Size;
          break;
      }

      if (form.MdiParent != null)
      {
        Rectangle area = WinFormsTools.GetMdiContainerArea(form.MdiParent);
        Bounds = WinFormsTools.PlaceRectangle(Bounds, area, minSize);
      }
      else if (form.Parent != null)
      {
        Rectangle area = WinFormsTools.GetControlDockFillArea(form.Parent);
        Bounds = WinFormsTools.PlaceRectangle(Bounds, area, minSize);
      }
      else
      {
        Screen screen = Screen.FromRectangle(Bounds);
        if (screen == null)
        {
          screen = EFPApp.DefaultScreen;
          if (screen == null)
            screen = Screen.PrimaryScreen;
        }
        Bounds = WinFormsTools.PlaceRectangle(Bounds, screen.WorkingArea, minSize);
      }
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Возвращает список применимых частей, исходя из возмоджости менять размер формы и кнопок максимизации/минимизации
    /// </summary>
    /// <param name="form">Форма</param>
    /// <returns>Доступные части</returns>
    internal static EFPFormBoundsPart GetParts(Form form)
    {
      if (form == null)
        return EFPFormBoundsPart.None;

      if ((!form.MaximizeBox) && form.WindowState == FormWindowState.Maximized)
        return EFPFormBoundsPart.None;

      EFPFormBoundsPart parts = EFPFormBoundsPart.Location;
      switch (form.FormBorderStyle)
      {
        case FormBorderStyle.Sizable:
        case FormBorderStyle.SizableToolWindow:
          parts |= EFPFormBoundsPart.Size;
          break;
      }
      if (form.MinimizeBox | form.MaximizeBox)
        parts |= EFPFormBoundsPart.WindowState;

      return parts;
    }

    /// <summary>
    /// Выполняет сравнение двух наборов координат в соответствии с заданными флагами частей
    /// </summary>
    /// <param name="a">Первый сравниваемый набор</param>
    /// <param name="b">Второй сравниваемый набор</param>
    /// <param name="parts">Части, которые надо сравнивать</param>
    /// <returns>true, если координаты одинаковые</returns>
    public static bool Equals(EFPFormBounds a, EFPFormBounds b, EFPFormBoundsPart parts)
    {
      if (a == null)
        a = new EFPFormBounds();
      if (b == null)
        b = new EFPFormBounds();

      if ((parts & EFPFormBoundsPart.Location) != 0)
      {
        if (a.Bounds.Left != b.Bounds.Left || a.Bounds.Top != b.Bounds.Top)
          return false;
      }
      if ((parts & EFPFormBoundsPart.Size) != 0)
      {
        if (a.Bounds.Width != b.Bounds.Width || a.Bounds.Height != b.Bounds.Height)
          return false;
      }
      if ((parts & EFPFormBoundsPart.WindowState) != 0)
      {
        if (a.WindowState != b.WindowState)
          return false;
      }
      return true;
    }

    #endregion
  }
}