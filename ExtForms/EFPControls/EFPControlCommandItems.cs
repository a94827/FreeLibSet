// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Windows.Forms;
using System.Text;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Базовый класс для EFPControlCommandItems и EFPFormCommandItems 
  /// </summary>
  public abstract class EFPContextCommandItems : EFPCommandItems
  {
    #region Конструктор

    internal EFPContextCommandItems()
    {
      _Active = false;
    }

    #endregion

    #region Свойство Active

    internal abstract bool GetIsModalForm();

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

        // Модальность формы проверяем только при активации меню, при дезактивации
        // используем предыдущее значение
        if (value)
        {
          if (!IsReadOnly)
            throw new InvalidOperationException("Не было вызова SetReadOnly()");

          _IsModalForm = GetIsModalForm();
        }

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

    #endregion

    #region Свойство IsModalForm


    #endregion

    #region Свойство StatusBarPanels

    /// <summary>
    /// Панели статусной строки. 
    /// </summary>
    internal EFPStatusBarPanels StatusBarPanels { get { return _StatusBarPanels; } }
    private EFPStatusBarPanels _StatusBarPanels;

    #endregion

    #region Подготовка

    internal void SetReadOnly()
    {
      if (IsReadOnly)
        return; // повторный вызов

      OnPrepare();

      base.IsReadOnly = true;

      bool sbFlag = false;
      foreach (EFPCommandItem item in this)
      {
        if (item.StatusBarUsage)
        {
          sbFlag = true;
          break;
        }
      }

      if (sbFlag)
      {
        _StatusBarPanels = CreateStatusBarPanels();
        _StatusBarPanels.Add(this);
      }
    }

    internal abstract EFPStatusBarPanels CreateStatusBarPanels();


    /// <summary>
    /// Событие вызывается при подготовке списка команд к использованию.
    /// На момент вызова свойство IsReadOnly=false.
    /// Событие используется, в основном, в отладочных целях.
    /// Классы-наследники, вместо обработчика события, переопределяют метод OnPrepare().
    /// Прикладной код обычно использует событие EFPControlBase.BeforePrepareCommandItems.
    /// </summary>
    public event EventHandler Prepare;

    /// <summary>
    /// Этот метод вызывается однократно перед началом использования списка команд.
    /// После вызова список переводится в режим "только для чтения".
    /// </summary>
    protected virtual void OnPrepare()
    {
      if (Prepare != null)
        Prepare(this, EventArgs.Empty);
    }

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

  /// <summary>
  /// Команды, относящиеся к управляющему элементу или форме. Объект
  /// осуществляет подключение и отключение отношений Master - Servant
  /// при изменении фокуса ввода
  /// </summary>
  public class EFPControlCommandItems : EFPContextCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список команд
    /// </summary>
    public EFPControlCommandItems(EFPControlBase controlProvider)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");
      switch (controlProvider.ProviderState)
      {
        case EFPControlProviderState.Detached:
        case EFPControlProviderState.Disposed:
          throw new InvalidOperationException();
      }
      _ControlProvider = controlProvider;
    }

    #endregion

    #region Общие методы и свойства

    /// <summary>
    /// Возвращает провайдер управляющего элемента, к которому относится список команд.
    /// </summary>
    public EFPControlBase ControlProvider { get { return _ControlProvider; } }
    private EFPControlBase _ControlProvider;

    /// <summary>
    /// Возвращает отладочную информацию
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("СontrolProvider=");
      sb.Append(ControlProvider.ToString());
      sb.Append(" ItemCount=");
      sb.Append(Count.ToString());
      //sb.Append(" IsReadOnly=");
      //sb.Append(IsReadOnly.ToString());
      //sb.Append(" Active=");
      //sb.Append(Active.ToString());
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

    internal override bool GetIsModalForm()
    {
      return ControlProvider.BaseProvider.FormProvider.Modal;
    }

    internal override EFPStatusBarPanels CreateStatusBarPanels()
    {
      return new EFPStatusBarPanels(ControlProvider, ControlProvider.Control);
    }

    #endregion
  }

  /// <summary>
  /// Список команд локального меню, относящегося к форме в-целом
  /// </summary>
  public class EFPFormCommandItems : EFPContextCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает список команд
    /// </summary>
    /// <param name="formProvider"></param>
    public EFPFormCommandItems(EFPFormProvider formProvider)
    {
      if (formProvider == null)
        throw new ArgumentNullException("formProvider");
      _FormProvider = formProvider;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер формы, для которой задаются команды
    /// </summary>
    public EFPFormProvider FormProvider { get { return _FormProvider; } }
    private EFPFormProvider _FormProvider;

    internal override bool GetIsModalForm()
    {
      return _FormProvider.Modal;
    }

    #endregion

    #region Методы

    internal override EFPStatusBarPanels CreateStatusBarPanels()
    {
      return new EFPStatusBarPanels(FormProvider, FormProvider.Form);
    }

    /// <summary>
    /// Возвращает отладочную информацию
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("FormProvider=");
      sb.Append(FormProvider.ToString());
      sb.Append(" ItemCount=");
      sb.Append(Count.ToString());
      //sb.Append(" IsReadOnly=");
      //sb.Append(IsReadOnly.ToString());
      //sb.Append(" Active=");
      //sb.Append(Active.ToString());
      return sb.ToString();
    }

    #endregion
  }
}
