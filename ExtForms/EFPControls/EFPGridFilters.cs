// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Collections;
using FreeLibSet.Core;
using System.ComponentModel;
using FreeLibSet.Data;

namespace FreeLibSet.Forms
{
  #region Интерфейсы

  #region IEFPGridFilter

  /// <summary>
  /// Одиночный фильтр табличного просмотра.
  /// Свойство <see cref="IObjectWithCode.Code"/> задает имя в секции конфигурации при сохранении фильтров.
  /// </summary>
  public interface IEFPGridFilter : IObjectWithCode
  {
    /// <summary>
    /// Отображаемое имя фильтра
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Текстовое представления для установленного значения фильтра.
    /// Если фильтр не установлен, возвращает пустую строку.
    /// </summary>
    string FilterText { get; }

    /// <summary>
    /// Выводит блок диалога для редактирования значения фильтра.
    /// </summary>
    /// <param name="dialogPosition">Желательное положение для блока диалога. Может быть пустым, но не null</param>
    /// <returns>True, если (возможно) было установлено значение фильтра.
    /// False, если пользователь нажал кнопку "Отмена".</returns>
    bool ShowFilterDialog(EFPDialogPosition dialogPosition);

    /// <summary>
    /// Сбрасывает фильтр.
    /// </summary>
    void Clear();

    /// <summary>
    /// Возвращает true, если фильтр не установлен.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Записывает значение фильтра в секцию конфигурации.
    /// Для каждого фильтра используется персональная секция, которая очищается перед вызовом метода.
    /// </summary>
    /// <param name="cfg">Записываемая секция конфигурации</param>
    void WriteConfig(CfgPart cfg);

    /// <summary>
    /// Читает значение фильтра из секции конфигурации.
    /// Для каждого фильтра используется персональная секция.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    void ReadConfig(CfgPart cfg);
  }

  /// <summary>
  /// Коллекция из нескольких объектов <see cref="IEFPGridFilter"/>.
  /// </summary>
  public interface IEFPGridFilterSet : IEnumerable<IEFPGridFilter>
  {
  }

  #endregion

  #region IEFPGridFilters

  /// <summary>
  /// Список возможных фильтров табличного просмотра <see cref="IEFPGridFilter"/>.
  /// </summary>
  // ReSharper disable once PossibleInterfaceMemberAmbiguity
  public interface IEFPGridFilters : IList<IEFPGridFilter>, IReadOnlyObject
  {
    /// <summary>
    /// Идентификатор базы данных.
    /// Используется при работе с буфером обмена для предотвращения вставки значений фильтра,
    /// если они были скопированы в другой программе.
    /// Если два экземпляра программы работают с одной базой данных, то могут возвращаться
    /// одинаковые значения, чтобы можно было работать с общим буфером обмена.
    /// Если же один экземпляр программы работает с несколлькими базами, то должны возвращаться
    /// разные значения, для предотвращения "кривой" вставки.
    /// </summary>
    string DBIdentity { get; }

    /// <summary>
    /// Поиск фильтра по имени
    /// </summary>
    /// <param name="code">Имя для сохранения конфигурации, а не <see cref="IEFPGridFilter.DisplayName"/>.
    /// Определяется свойством <see cref="IObjectWithCode.Code"/>.</param>
    /// <returns>Индекс фильтра в списке. (-1), если фильтр не найден.</returns>
    int IndexOf(string code);

    /// <summary>
    /// Поиск фильтра <see cref="IEFPGridFilter"/> по имени.
    /// Возвращает найденный фильтр или null, если фильтр не найден.
    /// Имеет ли фильтр установленное значение или фильтр пустой - не имеет значения.
    /// </summary>
    /// <param name="code">Имя для сохранения конфигурации, а не <see cref="IEFPGridFilter.DisplayName"/>.
    /// Определяется свойством <see cref="IObjectWithCode.Code"/>.</param>
    /// <returns>Найденный фильтр или null</returns>
    IEFPGridFilter this[string code] { get; }

    /// <summary>
    /// Очищает все фильтры в списке, вызывая метод <see cref="IEFPGridFilter.Clear()"/>.
    /// </summary>
    void ClearAllFilters();

    /// <summary>
    /// Возвращает true, если ни один фильтр не установлен.
    /// Опрашивает свойство <see cref="IEFPGridFilter.IsEmpty"/> у всех фильтров в списке.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Выполняет запись конфигурации для всех фильтров в списке.
    /// Предполагается, что значение каждого фильтра хранится в отдельной дочерней секции.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    void WriteConfig(CfgPart cfg);

    /// <summary>
    /// Выполняет чтение конфигурации для всех фильтров в списке.
    /// Предполагается, что значение каждого фильтра хранится в отдельной дочерней секции.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    void ReadConfig(CfgPart cfg);

    /// <summary>
    /// Переводит список в режим просмотра.
    /// После этого нельзя добавлять фильтры в список, но можно устанавливать значения фильтров.
    /// Этот метод вызывается в <see cref="EFPConfigurableDataGridView.OnLoadConfig()"/>() профилактических целях.
    /// </summary>
    void SetReadOnly();

    /// <summary>
    /// Начать обновление фильтров.
    /// Этот метод вызывается при показе диалога установки фильтров, чтобы предотвратить 
    /// лишние вызовы события ListChanged.
    /// </summary>
    void BeginUpdate();

    /// <summary>
    /// Закончить обновление фильтров.
    /// Этот метод вызывается после показа диалога установки фильтров.
    /// </summary>
    void EndUpdate();

    /// <summary>
    /// Событие вызывается при изменении значения какого-либо фильтра, если не было вызова BeginUpdate()
    /// </summary>
    event ListChangedEventHandler ListChanged;

    /// <summary>
    /// Добавление набора из нескольких фильтров
    /// </summary>
    /// <param name="filterSet">Добавляемый набор. Не может быть null</param>
    void Add(IEFPGridFilterSet filterSet);
  }

  #endregion

  #region IEFPScrollableGridFilter

  /// <summary>
  /// Интерфейс фильтра табличного просмотра, который можно "прокручивать"
  /// вверх / вниз Alt + клавишами со стрелками.
  /// Реализуется фильтрами по дате.
  /// </summary>
  public interface IEFPScrollableGridFilter : IEFPGridFilter
  {
    /// <summary>
    /// Возвращает true, если можно перейти к предыдущему значению фильтра.
    /// Это возможно, если в фильтре задан какой-нибудь период (а не полуоткрытый интервал)
    /// и нет других ограничений.
    /// Используется для блокировки команды локального меню табличного просмотра.
    /// </summary>
    bool CanScrollUp { get; }

    /// <summary>
    /// Возвращает true, если можно перейти к следующему значению фильтра.
    /// Это возможно, если в фильтре задан какой-нибудь период (а не полуоткрытый интервал)
    /// и нет других ограничений.
    /// Используется для блокировки команды локального меню табличного просмотра.
    /// </summary>
    bool CanScrollDown { get; }

    /// <summary>
    /// Перейти к предыдущему значению фильтра.
    /// </summary>
    void ScrollUp();

    /// <summary>
    /// Перейти к следующему значению фильтра.
    /// </summary>
    void ScrollDown();
  }

  #endregion

  #region IEFPGridFilterWithImageKey

  /// <summary>
  /// Расширение интерфейса фильтра табличного просмотра, который поддерживает разные значки для фильтров
  /// </summary>
  public interface IEFPGridFilterWithImageKey : IEFPGridFilter
  {
    /// <summary>
    /// Свойство должно возвращать имя изображения из списка EFPApp.MainImages.
    /// Свойство опрашивается только для установленного фильтра. При этом, если возвращается пустая строка,
    /// используется константа EFPGridFilterTools.DefaultFilterImageKey
    /// </summary>
    string FilterImageKey { get; }
  }

  #endregion

  #endregion


  /// <summary>
  /// Заглушка для реализации интерфейса <see cref="IEFPGridFilters"/>.
  /// Реализует пустой список фильтров.
  /// </summary>
  public class EFPDummyGridFilters : DummyList<IEFPGridFilter>, IEFPGridFilters
  {
    #region IEFPGridFilters Members

    string IEFPGridFilters.DBIdentity { get { return String.Empty; } }

    int IEFPGridFilters.IndexOf(string code)
    {
      return -1;
    }

    IEFPGridFilter IEFPGridFilters.this[string code] { get { return null; } }

    void IEFPGridFilters.ClearAllFilters() { }

    bool IEFPGridFilters.IsEmpty { get { return true; } }

    void IEFPGridFilters.ReadConfig(CfgPart cfg) { }

    void IEFPGridFilters.WriteConfig(CfgPart cfg) { }

    void IEFPGridFilters.SetReadOnly() { }

    void IEFPGridFilters.BeginUpdate() { }

    void IEFPGridFilters.EndUpdate() { }

    void IEFPGridFilters.Add(IEFPGridFilterSet filterSet) { }

    event ListChangedEventHandler IEFPGridFilters.ListChanged
    {
      add { }
      remove { }
    }


    #endregion
  }

  /// <summary>
  /// Реализация <see cref="IEFPGridFilters"/> для работы с пользовательскими фильтрами.
  /// Для использования <see cref="DBxCommonFilter"/> следует использовать расширенный класс <see cref="FreeLibSet.Forms.Data.EFPDBxGridFilters"/>.
  /// </summary>
  public class EFPGridFilters : NamedListWithNotifications<IEFPGridFilter>, IEFPGridFilters
  {
    #region IEFPGridFilters 

    /// <summary>
    /// Идентификатор базы данных.
    /// </summary>
    public string DBIdentity
    {
      get { return _DBIdentity; }
      set { _DBIdentity = value; }
    }
    private string _DBIdentity;

    /// <summary>
    /// Возвращает true, если нет ни одного активного фильтра.
    /// Чтобы определить наличие установленного фильтра среди определенного подмножества, используйте <see cref="IsAnyNotEmpty(string)"/>.
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (!this[i].IsEmpty)
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// Добавление набора из нескольких фильтров
    /// </summary>
    /// <param name="filterSet">Добавляемый набор. Не может быть null</param>
    public void Add(IEFPGridFilterSet filterSet)
    {
#if DEBUG
      if (filterSet == null)
        throw new ArgumentNullException("filterSet");
#endif
      foreach (IEFPGridFilter item in filterSet)
        Add(item);
    }

    /// <summary>
    /// Очищает все фильтры в списке, вызывая метод <see cref="IEFPGridFilter.Clear()"/>.
    /// </summary>
    public void ClearAllFilters()
    {
      for (int i = 0; i < Count; i++)
        this[i].Clear();
    }

    /// <summary>
    /// Выполняет запись конфигурации для всех фильтров в списке.
    /// Предполагается, что значение каждого фильтра хранится в отдельной дочерней секции.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void WriteConfig(CfgPart cfg)
    {
      if (cfg == null)
        throw new ArgumentNullException("cfg");

      cfg.Clear();

      for (int i = 0; i < Count; i++)
      {
        CfgPart cfg2 = cfg.GetChild(this[i].Code, true); // обязательно создаем даже для неустановленных фильтров
        cfg2.Clear(); // 11.09.2012
        if (!this[i].IsEmpty)
          this[i].WriteConfig(cfg2);
      }
    }

    /// <summary>
    /// Выполняет чтение конфигурации для всех фильтров в списке.
    /// Предполагается, что значение каждого фильтра хранится в отдельной дочерней секции.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void ReadConfig(CfgPart cfg)
    {
      if (cfg == null)
        return;

      for (int i = 0; i < Count; i++)
      {
        CfgPart cfg2 = cfg.GetChild(this[i].Code, false);
        if (cfg2 != null)
        {
          try
          {
            this[i].ReadConfig(cfg2);
          }
          catch (Exception e)
          {
            OnReadConfigError(e, this[i], cfg2);
          }
        }
      }
      if (cfg == null)
        return;

      for (int i = 0; i < Count; i++)
      {
        CfgPart cfg2 = cfg.GetChild(this[i].Code, false);
        if (cfg2 != null)
        {
          try
          {
            this[i].ReadConfig(cfg2);
          }
          catch (Exception e)
          {
            OnReadConfigError(e, this[i], cfg2);
          }
        }
      }
    }

    /// <summary>
    /// Вызывается при возникновении ошибки чтении конфигурации в <see cref="IEFPGridFilter.ReadConfig(CfgPart)"/>.
    /// Выводит сообщение с помощью <see cref="EFPApp.ShowException(Exception, string)"/> и очищает фильтр.
    /// </summary>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="filter">Фильтр, для которого возникло исключение</param>
    /// <param name="cfg">Считываемая секция конфигурации</param>
    protected virtual void OnReadConfigError(Exception exception, IEFPGridFilter filter, CfgPart cfg)
    {
      EFPApp.ShowException(exception, Res.EFPGridFilters_ErrTitle_ReadConfig);
      filter.Clear();
    }

    void IEFPGridFilters.SetReadOnly()
    {
      base.SetReadOnly();
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Возвращает true, если среди запрашиваемых фильтров хотя бы один установлен.
    /// Если в списке <paramref name="codes"/> присутствуют несуществующие коды фильтров, то они пропускаются,
    /// как будто для них <see cref="IEFPGridFilter.IsEmpty"/>=true, без выдачи сообщения об ошибке.
    /// Чтобы проверить наличие установки любого фильтра, используйте свойство <see cref="IEFPGridFilter.IsEmpty"/>.
    /// </summary>
    /// <param name="codes">Список кодов проверяемых </param>
    /// <returns>Наличие установленных фильтров</returns>
    public bool IsAnyNotEmpty(string codes)
    {
      if (string.IsNullOrEmpty(codes))
        return false;

      if (codes.IndexOf(',') >= 0)
      {
        string[] a = codes.Split(',');
        for (int i = 0; i < a.Length; i++)
        {
          IEFPGridFilter filter = this[a[i]];
          if (filter != null)
          {
            if (!filter.IsEmpty)
              return true;
          }
        }
        return false;
      }
      else
      {
        IEFPGridFilter filter = this[codes];
        if (filter != null)
        {
          if (!filter.IsEmpty)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Поиск фильтра по пользовательскому имени фильтра (свойству <see cref="IEFPGridFilter.DisplayName"/>).
    /// </summary>
    /// <param name="displayName">Имя фильтра для поиска</param>
    /// <returns>Найденный фильтр или null, если фильтр не найден</returns>
    public IEFPGridFilter FindByDisplayName(string displayName)
    {
      if (String.IsNullOrEmpty(displayName))
        return null;
      for (int i = 0; i < Count; i++)
      {
        if (this[i].DisplayName == displayName)
          return this[i];
      }
      return null;
    }

    /// <summary>
    /// Выполняет очистку фильтра, если он существует.
    /// Если нет фильтра с кодом <paramref name="code"/>, никаких действий не выполняется.
    /// </summary>
    /// <param name="code">Код фильтра</param>
    public void ClearFilter(string code)
    {
      IEFPGridFilter filter = this[code];
      if (filter != null)
        filter.Clear();
    }

    /// <summary>
    /// Выполняет очистку фильтров с заданными кодами.
    /// Если в списке <paramref name="codes"/> указаны фильтра с несуществующими кодами, то они пропускаются.
    /// </summary>
    /// <param name="codes">Список кодов фильтров, разделенных запятыми</param>
    public void ClearFilters(string codes)
    {
      if (String.IsNullOrEmpty(codes))
        return;
      if (codes.IndexOf(',') >= 0)
      {
        string[] a = codes.Split(',');
        for (int i = 0; i < a.Length; i++)
          ClearFilter(a[i]);
      }
      else
        ClearFilter(codes);
    }

    /// <summary>
    /// Получение SQL-фильтра для фильтрации набора данных.
    /// Пустые фильтры и фильтры со сброшенным свойством <see cref="DBxCommonFilter.UseSqlFilter"/> пропускаются.
    /// Также пропускаются фильтры, отличные от <see cref="DBxCommonFilter"/>.
    /// Если нет ни одного фильтра, возвращается null.
    /// </summary>
    /// <returns>Объект <see cref="DBxFilter"/>, соответствующий активным фильтрам. Если фильтров 
    /// несколько, то будет возвращен <see cref="AndFilter"/></returns>
    public DBxFilter GetSqlFilter()
    {
      List<DBxFilter> filters = new List<DBxFilter>();
      for (int i = 0; i < Count; i++)
      {
        DBxCommonFilter filter = this[i] as DBxCommonFilter;
        if (filter != null)
        {
          if (filter.UseSqlFilter && (!filter.IsEmpty))
            filters.Add(filter.GetSqlFilter());
        }
      }
      return AndFilter.FromList(filters);
    }

    /// <summary>
    /// Возвращает установленные фильтры в формате <see cref="System.Data.DataView.RowFilter"/>.
    /// См. метод <see cref="GetSqlFilter()"/>.
    /// Если нет установленных фильтров, возвращается пустая строка.
    /// </summary>
    public string DataViewRowFilter
    {
      get
      {
        DBxFilter sqlFilter = GetSqlFilter();
        if (sqlFilter == null)
          return String.Empty;
        else
          return sqlFilter.ToString();
      }
    }

    #endregion
  }

  /// <summary>
  /// Статические методы для работы с фильтрами табличного просмотра
  /// </summary>
  public static class EFPGridFilterTools
  {
    #region Значки для редакторов фильтров

    /// <summary>
    /// Значок в EFPApp.MainImages для установленного (непустого) фильтра
    /// </summary>
    public const string DefaultFilterImageKey = "Filter";

    /// <summary>
    /// Значок в EFPApp.MainImages для сброса фильтра
    /// </summary>
    public const string NoFilterImageKey = "No";

    #endregion
  }

#if XXX

  /// <summary>
  /// Провайдер комбоблока для одиночного фильтра табличного просмотра
  /// </summary>
  public class EFPGridFilterComboBox : EFPUserSelComboBox
  {
  #region Конструктор

    public EFPGridFilterComboBox(EFPBaseProvider baseProvider, FreeLibSet.Controls.UserSelComboBox control, IEFPGridFilter gridFilter)
      :base(baseProvider, control)
    {
      if (gridFilter == null)
        throw new ArgumentNullException("gridFilter");
      _GridFilter = gridFilter;
    }

  #endregion

  #region Фильтр

    public IEFPGridFilter GridFilter { get { return _GridFilter; } }
    private IEFPGridFilter _GridFilter;

    private DBxCommonFilter CommonFilter { }

  #endregion
  }

#endif
}
