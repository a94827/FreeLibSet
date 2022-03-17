// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Windows.Forms;
using System.Text;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Команды, относящиеся к управляющему элементу или форме. Объект
  /// осуществляет подключение и отключение отношений Master - Servant
  /// при изменении фокуса ввода
  /// </summary>
  public class EFPControlCommandItems : EFPCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список команд
    /// </summary>
    public EFPControlCommandItems()
    {
      _Active = false;
      _EHFormEnter = new EventHandler(FormEnter);
      _EHFormLeave = new EventHandler(FormLeave);

    }

    private EFPControlCommandItems(bool SetReadOnly)
      : this()
    {
      base.IsReadOnly = SetReadOnly;
    }

    #endregion

    #region Общие методы и свойства

    /// <summary>
    /// Возвращает управляющий элемент, к которому относится список команд,
    /// если метод EFPControl.PrepareCommandItems() был успешно вызван.
    /// До этого список команд находится в процессе заполнения и возвращает null
    /// </summary>
    public Control Control { get { return _Control; } }
    private Control _Control;

    internal void SetControl(Control control)
    {
      if (control == null)
        throw new ArgumentNullException("control");

      if (_Control != null)
        throw new InvalidOperationException("Свойство Control можно устанавливать только один раз");

      _Control = control;

      if (control is Form)
      {
        _LastForm = (Form)control;
        // Обработка ShortCut'ов
        _LastForm.KeyPreview = true;
      }
      else
      {
        _LastForm = control.FindForm();
        if (_LastForm == null)
          throw new InvalidOperationException("Управляющий элемент не был присоединен к форме");
        _Control.Enter += new EventHandler(ControlEnter);
        _Control.Leave += new EventHandler(ControlLeave);
        _Control.HandleCreated += new EventHandler(ControlHandleCreated);
      }
      _LastForm.Activated += _EHFormEnter;
      _LastForm.Deactivate += _EHFormLeave;

      _Control.Disposed += new EventHandler(ControlDisposed);
      //FControl.KeyDown += new KeyEventHandler(ClientItems.PerformKeyDown);
      Active = _LastForm.Visible && _Control.ContainsFocus;
    }

    /// <summary>
    /// Выполняет проверку, что список команд еще не присоединен к управляющему элементу.
    /// Если свойство Control уже установлено, то генерируется исключение InvalidOperationException.
    /// Метод используется при установке управляющих свойств, которые не могут меняться в процессе работы.
    /// </summary>
    public void CheckNotAssigned()
    {
      if (Control != null)
        throw new InvalidOperationException("Действие может выполняться только до вызова EFPDataGridView.SetCommandItems()");
    }

    internal void CallBeforeControlAssigned()
    {
      BeforeControlAssigned();
    }

    /// <summary>
    /// Вызывается перед присвоением значения свойству Control
    /// Метод вызывается однократно.
    /// </summary>
    protected virtual void BeforeControlAssigned()
    {
    }

    internal void CallAfterControlAssigned()
    {
      AfterControlAssigned();
    }

    /// <summary>
    /// Вызывается после присвоения значения свойству Control
    /// Метод вызывается однократно.
    /// </summary>
    protected virtual void AfterControlAssigned()
    {
    }

    private Form _LastForm;

    private bool _IsModalForm;

    /// <summary>
    /// Свойство устанавливается в true, когда управляющий элемент получает фокус ввода,
    /// и в false, когда теряет фокус.
    /// В прикладном коде не следует устанавливать свойство, т.к. это может нарушить работу меню
    /// и статусной строки
    /// </summary>
    public bool Active
    {
      get
      {
        return _Active;
      }
      set
      {
        if (value == _Active)
          return;

        if (_Control == null)
          throw new InvalidOperationException("Свойство Control должно быть задано до установки свойства Active");

        // Модальность формы проверяем только при активации меню, при дезактивации
        // используем предыдущее значение
        // 15.12.2011: Еще проверяем наличие самой формы
        if (value/* && Control.FindForm()!=null*/)
          _IsModalForm = Control.FindForm().Modal;

        _Active = value;
        foreach (EFPCommandItem item in this)
        {
          if (_IsModalForm && (item.Master != null) && item.Master.IsGlobalItem)
            continue;
          item.MasterActive = value;
        }
        if (value)
          AddFocus();
        else
          RemoveFocus();
        if (StatusBarPanels != null)
        {
          if (value)
            StatusBarPanels.Attach();
          else
            StatusBarPanels.Detach();
        }

      }
    }
    private bool _Active;

    /// <summary>
    /// Панели статусной строки. Объект должен быть заполнен заранее
    /// и присоединен до установки свойства Control
    /// </summary>
    public EFPStatusBarPanels StatusBarPanels
    {
      get
      {
        return _StatusBarPanels;
      }
      set
      {
        if (Control != null)
          throw new InvalidOperationException("Свойство должно устанавливаться до Control");
        _StatusBarPanels = value;
      }
    }
    private EFPStatusBarPanels _StatusBarPanels;


    /// <summary>
    /// Возвращает отладочную информацию
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(GetType().Name);
      sb.Append(" Сontrol=");
      if (Control == null)
        sb.Append("null");
      else
        sb.Append(Control.ToString());
      sb.Append(" Active=");
      sb.Append(Active.ToString());
      sb.Append(" ItemCount=");
      sb.Append(Count.ToString());
      return sb.ToString();
    }


    /// <summary>
    /// Команда, выделенная жирным шрифтом.
    /// Обычно, это команда должна обрабатываться по двойному щелчку мыши. Это должно быть реализовано отдельно
    /// </summary>
    public EFPCommandItem DefaultCommandItem
    {
      get
      {
        return _DefaultCommandItem;
      }
      set
      {
        CheckNotReadOnly();
        _DefaultCommandItem = value;
      }
    }
    private EFPCommandItem _DefaultCommandItem;


    #endregion

    #region Внутренняя реализация

    void ControlHandleCreated(object sender, EventArgs args)
    {
      ValidateFormHandlers();
    }

    private void ControlEnter(object sender, EventArgs args)
    {
      // 09.08.2011
      // Управляющий элемент может быть прицеплен к другой форме в процессе работы
      ValidateFormHandlers();

        if (_LastForm.Visible)  // проверка условия добавлена 10.04.2009
        // Событие может вызываться с помощью установки свойства
        // TheForm.ActiveControl до показа формы на экране
        Active = true;
    }

    private void ValidateFormHandlers()
    {
      if (_Control == null)
        return; // 07.10.2019

      if (_Control.FindForm() != _LastForm)
      {
        _LastForm.Activated -= _EHFormEnter;
        _LastForm.Deactivate -= _EHFormLeave;
        _LastForm = _Control.FindForm();
        _LastForm.Activated += _EHFormEnter;
        _LastForm.Deactivate += _EHFormLeave;
      }
    }

    private void ControlLeave(object sender, EventArgs args)
    {
      Active = false;
    }

    /// <summary>
    /// Перед дезактивацией формы запоминаем, имеет ли упр. элемент фокус ввода
    /// </summary>
    private bool _ControlWasActive;

    /// <summary>
    /// Управляющий элемент может быть присоединен к другой форме (например, в Мастере)
    /// В этом случае обработчик формы должен быть прицеплен к новой форме
    /// </summary>
    private EventHandler _EHFormEnter;
    private EventHandler _EHFormLeave;

    private void FormEnter(object sender, EventArgs args)
    {
      if (_Control.ContainsFocus)
      {
        Active = true;
      }
      else
      {
        Active = _ControlWasActive;
      }
    }
    private void FormLeave(object sender, EventArgs args)
    {
      _ControlWasActive = Active;
      Active = false;
    }

    private void ControlDisposed(object sender, EventArgs args)
    {
      Active = false;
      Dispose();
    }

    #endregion

    #region Пустой список

    /// <summary>
    /// Пустой список команд с установленными признаком "Только чтение"
    /// </summary>
    public static readonly EFPControlCommandItems Empty = new EFPControlCommandItems(true);

    #endregion
  }

  #region Делегат для события EFPApp.ControlCommandItemsNeeded

  /// <summary>
  /// Аргументы события EFPApp.ControlCommandItemsNeeded
  /// </summary>
  public class EFPControlCommandItemsNeededEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Вызывается из EFPApp
    /// </summary>
    /// <param name="controlProvider"></param>
    public EFPControlCommandItemsNeededEventArgs(EFPControlBase controlProvider)
    {
      _ControlProvider = controlProvider;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Обработчик события ControlCommandItemsNeeded может установить свойство
    /// ControlProvider.CommandItems
    /// </summary>
    public EFPControlBase ControlProvider { get { return _ControlProvider; } }
    private EFPControlBase _ControlProvider;

    #endregion
  }

  /// <summary>
  /// Делегат для события EFPApp.ControlCommandItemsNeeded
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void EFPControlCommandItemsNeededEventHandler(object sender,
    EFPControlCommandItemsNeededEventArgs args);

  #endregion
}
