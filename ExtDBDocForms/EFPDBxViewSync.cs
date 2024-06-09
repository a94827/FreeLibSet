// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Runtime.InteropServices;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Синхронизация выбранных строк документов между <see cref="EFPDBxTreeView"/> и <see cref="EFPDBxGridView"/> (просмотрами, реализующими <see cref="IEFPDBxView"/>).
  /// Синхронизация наборов данных не выполняется.
  /// Просмотры должны относиться к одному виду документа / поддокумента.
  /// Используется в формах просмотра документов <see cref="DocTableViewForm"/> и поддокументов <see cref="SubDocTableViewForm"/>.
  /// </summary>
  public class EFPDBxViewSync
  {
    #region Вложенная структура

    private struct Info
    {
      #region Конструктор

      public Info(EFPDBxViewSync owner, IEFPDBxView controlProvider, int changedIndex)
      {
        _Owner = owner;
        if (controlProvider == null)
          throw new ArgumentNullException("controlProvider");
        _ControlProvider = controlProvider;
        _ChangedIndex = changedIndex;

        controlProvider.Control.Enter += new EventHandler(Control_Enter);


        controlProvider.SelectionChanged += new EventHandler(ControlProvider_SelectionChanged);
        ControlProvider_SelectionChanged(null, null);
      }

      void ControlProvider_SelectionChanged(object sender, EventArgs args)
      {
        // Этот обработчик должен быть как можно более быстрым

        if (_ControlProvider.Control.Focused)
          _Owner._LastSelectionChangedIndex = _ChangedIndex;
      }

      void Control_Enter(object Sender, EventArgs Args)
      {
        try
        {
          if (_Owner._LastSelectionChangedIndex >= 0 && _Owner._LastSelectionChangedIndex != _ChangedIndex)
            _ControlProvider.SelectedIds = _Owner._InfoList[_Owner._LastSelectionChangedIndex]._ControlProvider.SelectedIds;
        }
        catch { }
      }

      #endregion

      #region Свойства

      private EFPDBxViewSync _Owner;

      private IEFPDBxView _ControlProvider;

      private int _ChangedIndex;

      #endregion
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает синхронизатор для двух просмотров
    /// </summary>
    /// <param name="efp1">Провайдер первого просмотра</param>
    /// <param name="efp2">Провайдер второго просмотра</param>
    public EFPDBxViewSync(IEFPDBxView efp1, IEFPDBxView efp2)
      : this(new IEFPDBxView[2] { efp1, efp2 })
    {
    }

    /// <summary>
    /// Создает синхронизатор для произвольного числа просмотров
    /// </summary>
    /// <param name="controlProviders">Список провайдеров просмотров</param>
    public EFPDBxViewSync(IEFPDBxView[] controlProviders)
    {
#if DEBUG
      if (controlProviders == null)
        throw new ArgumentNullException("controlProviders");
      if (controlProviders.Length < 2)
        throw new ArgumentException("controlProviders.Length<2", "controlProviders");
#endif

      _InfoList = new Info[controlProviders.Length];
      for (int i = 0; i < controlProviders.Length; i++)
        _InfoList[i] = new Info(this, controlProviders[i], i);
      _LastSelectionChangedIndex = -1;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список провайдеров синхронизируемых просмотров
    /// </summary>
    private Info[] _InfoList;

    /// <summary>
    /// Если в элементе, имеющем фокус ввода, изменяется текущая позиция, то свойство содержит индекс
    /// этого элемента
    /// </summary>
    private int _LastSelectionChangedIndex;

    #endregion
  }

  /// <summary>
  /// Синхронизация фильтров документов в просмотрах <see cref="EFPDocGridView"/> и <see cref="EFPDocTreeView"/>
  /// </summary>
  public class EFPDocViewFilterSync
  {
    // Требуется интерфейс IEFPDocView, а не IEFPDBxView, так как используется событие ShowDeletedChanged

    #region Конструкторы

    /// <summary>
    /// Создает синхронизатор для двух просмотров документов.
    /// 
    /// Для первого провайдера устанавливается свойство <see cref="IEFPControlWithFilters.SaveFiltersAllowed"/>=true,
    /// для второго - false.
    /// </summary>
    /// <param name="efp1">Провайдер первого просмотра</param>
    /// <param name="efp2">Провайдер второго просмотра</param>
    public EFPDocViewFilterSync(IEFPDocView efp1, IEFPDocView efp2)
      : this(new IEFPDocView[2] { efp1, efp2 }, true)
    {
    }

    /// <summary>
    /// Создает синхронизатор для произвольного числа просмотров документов.
    /// 
    /// Для первого провайдера в списке устанавливается свойство <see cref="IEFPControlWithFilters.SaveFiltersAllowed"/>=true,
    /// для остальных - false.
    /// </summary>
    /// <param name="controlProviders">Список провайдеров</param>
    public EFPDocViewFilterSync(IEFPDocView[] controlProviders)
      :this(controlProviders, true)
    {
    }

    /// <summary>
    /// Создает синхронизатор для произвольного числа просмотров документов.
    /// </summary>
    /// <param name="controlProviders">Список провайдеров</param>
    /// <param name="initSaveFiltersAllowed">Если true, то будет устанавливаться свойство
    /// <see cref="IEFPControlWithFilters.SaveFiltersAllowed"/> (для первого провайдера в true, для отальных в false).
    /// Если False, то предполагается, что свойство <see cref="IEFPControlWithFilters.SaveFiltersAllowed"/> устанавливается в 
    /// пользовательском коде</param>
    public EFPDocViewFilterSync(IEFPDocView[] controlProviders, bool initSaveFiltersAllowed)
    {
#if DEBUG
      if (controlProviders == null)
        throw new ArgumentNullException("controlProviders");
      if (controlProviders.Length < 2)
        throw new ArgumentException("controlProviders.Length<2", "controlProviders");
#endif

      this._ControlProviders = controlProviders;
      for (int i = 0; i < controlProviders.Length; i++)
      {
        controlProviders[i].AfterSetFilter += new EventHandler(ControlProvider_AfterSetFilter);
        controlProviders[i].ShowDeletedChanged += new EventHandler(ControlProvider_ShowDeletedChanged);
        if (initSaveFiltersAllowed)
          controlProviders[i].SaveFiltersAllowed = (i == 0);
      }
    }

    private bool _InsideHandler;

    void ControlProvider_AfterSetFilter(object sender, EventArgs args)
    {
      if (_InsideHandler)
        return;
      _InsideHandler = true;
      try
      {
        for (int i = 0; i < _ControlProviders.Length; i++)
        {
          if (_ControlProviders[i] == sender)
            continue;

          _ControlProviders[i].Filters = ((IEFPDocView)sender).Filters;
          _ControlProviders[i].PerformFilterChanged();
        }
      }
      finally
      {
        _InsideHandler = false;
      }
    }

    void ControlProvider_ShowDeletedChanged(object sender, EventArgs args)
    {
      if (_InsideHandler)
        return;
      _InsideHandler = true;
      try
      {
        for (int i = 0; i < _ControlProviders.Length; i++)
        {
          if (_ControlProviders[i] == sender)
            continue;

          _ControlProviders[i].ShowDeleted = ((IEFPDocView)sender).ShowDeleted;
        }
      }
      finally
      {
        _InsideHandler = false;
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список провайдеров синхронизируемых просмотров
    /// </summary>
    private readonly IEFPDocView[] _ControlProviders;

    #endregion
  }
}
