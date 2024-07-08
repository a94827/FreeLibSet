// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Windows.Forms;
using System.Text;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Базовый класс для <see cref="EFPControlCommandItems"/> и <see cref="EFPFormCommandItems"/>
  /// </summary>
  public abstract class EFPContextCommandItems : EFPCommandItems
  {
    #region Конструктор

    internal EFPContextCommandItems()
    {
      _HasFocus = false;
      _HasStatus = false;
    }

    #endregion

    #region Свойство Active

    internal abstract bool GetIsModalForm();

    private bool _IsModalForm;

    /// <summary>
    /// Свойство устанавливается в true, когда управляющий элемент получает фокус ввода,
    /// и в false, когда теряет фокус.
    /// </summary>
    internal bool HasFocus { get { return _HasFocus; } }
    private bool _HasFocus;

    internal void SetHasFocus(bool value)
    {
      if (value == _HasFocus)
        return;

      // Модальность формы проверяем только при активации меню, при дезактивации
      // используем предыдущее значение
      if (value)
      {
        if (!IsReadOnly)
          throw new InvalidOperationException("Не было вызова SetReadOnly()");

        _IsModalForm = GetIsModalForm();
      }

      _HasFocus = value;
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
    }

    internal bool HasStatus { get { return _HasStatus; } }
    private bool _HasStatus;

    internal void SetHasStatus(bool value)
    {
      if (value == _HasStatus)
        return;
      _HasStatus = value;

      if (StatusBarPanels != null)
      {
        if (value)
          StatusBarPanels.Attach();
        else
          StatusBarPanels.Detach();
      }
    }

    /// <summary>
    /// Комбинированная установка свойств
    /// </summary>
    /// <param name="value"></param>
    internal void SetActive(bool value)
    {
      SetHasFocus(value);
      SetHasStatus(value);
    }

    #endregion

    #region Свойство StatusBarPanels

    /// <summary>
    /// Панели статусной строки. 
    /// </summary>
    internal EFPStatusBarPanels StatusBarPanels { get { return _StatusBarPanels; } }
    private EFPStatusBarPanels _StatusBarPanels;

    #endregion

    #region Подготовка

    /// <summary>
    /// Создает панели статусной строки
    /// </summary>
    protected override void OnAfterSetReadOnly()
    {
      base.OnAfterSetReadOnly();

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

    #endregion
  }

  #region Делегат для события EFPApp.ControlCommandItemsNeeded

  /// <summary>
  /// Аргументы события <see cref="EFPApp.ControlCommandItemsNeeded"/>
  /// </summary>
  public class EFPControlCommandItemsNeededEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Вызывается из <see cref="EFPApp"/>
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    public EFPControlCommandItemsNeededEventArgs(EFPControlBase controlProvider)
    {
      _ControlProvider = controlProvider;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Обработчик события может установить свойство
    /// <see cref="ControlProvider"/>.CommandItems.
    /// </summary>
    public EFPControlBase ControlProvider { get { return _ControlProvider; } }
    private readonly EFPControlBase _ControlProvider;

    #endregion
  }

  /// <summary>
  /// Делегат для события <see cref="EFPApp.ControlCommandItemsNeeded"/>
  /// </summary>
  /// <param name="sender">Не используется</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPControlCommandItemsNeededEventHandler(object sender,
    EFPControlCommandItemsNeededEventArgs args);

  #endregion

  /// <summary>
  /// Команды, относящиеся к управляющему элементу. Объект
  /// осуществляет подключение и отключение отношений Master - Servant
  /// при изменении фокуса ввода
  /// </summary>
  public class EFPControlCommandItems : EFPContextCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список команд
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента. Должен быть задан</param>
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
    private readonly EFPControlBase _ControlProvider;

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
    /// Обычно, это команда должна обрабатываться по двойному щелчку мыши. 
    /// Обработка события мыши должна быть реализована отдельно.
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
    /// <param name="formProvider">Провайдер формы. Должен быть задан</param>
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
    private readonly EFPFormProvider _FormProvider;

    #endregion

    #region Методы

    internal override bool GetIsModalForm()
    {
      return _FormProvider.Modal;
    }

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
