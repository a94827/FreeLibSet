// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  #region Интерфейсы

  #region IEFPGridFilter

  /// <summary>
  /// Одиночный фильтр табличного просмотра.
  /// Свойство Code задает имя в секции конфигурации при сохранении фильтров
  /// </summary>
  public interface IEFPGridFilter : IObjectWithCode
  {
    /// <summary>
    /// Отображаемое имя фильтра
    /// </summary>
    string DisplayName { get;}

    /// <summary>
    /// Текстовое представления для установленного значения фильтра.
    /// Если фильтр не установлен, возвращает пустую строку.
    /// </summary>
    string FilterText { get;}

    /// <summary>
    /// Выводит блок диалога для редактирования значения фильтра
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
    bool IsEmpty { get;}

    /// <summary>
    /// Читает значение фильтра из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    void ReadConfig(CfgPart cfg);

    /// <summary>
    /// Записывает значение фильтра в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Записываемая секция конфигурации</param>
    void WriteConfig(CfgPart cfg);
  }

  /// <summary>
  /// Коллекция из нескольких объектов IEFPGridFilter
  /// </summary>
  public interface IEFPGridFilterSet : IEnumerable<IEFPGridFilter>
  { 
  }

  #endregion

  #region IEFPGridFilters

  /// <summary>
  /// Список возможных фильтров табличного просмотра
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
    string DBIdentity { get;}

    /// <summary>
    /// Поиск фильтра по имени
    /// </summary>
    /// <param name="Code">Имя для сохранения конфигурации, а не DisplayName.
    /// Определяется свойством IEFPGridFilter.Code.</param>
    /// <returns>Индекс фильтра в списке. (-1), если фильтр не найден</returns>
    int IndexOf(string Code);

    /// <summary>
    /// Поиск фильтра по имени
    /// </summary>
    /// <param name="code">Имя для сохранения конфигурации, а не DisplayName.
    /// Определяется свойством IEFPGridFilter.Code.</param>
    /// <returns>Найденный фильтр или null</returns>
    IEFPGridFilter this[string code] { get; }

    /// <summary>
    /// Очищает все фильтры в списке, вызывая метод Clear()
    /// </summary>
    void ClearAllFilters();

    /// <summary>
    /// Возвращает true, если ни один фильтр не установлен.
    /// Опрашивает свойство IsEmpty у всех фильтров в списке.
    /// </summary>
    bool IsEmpty { get;}

    /// <summary>
    /// Выполняет чтение конфигурации для всех фильтров в списке.
    /// Предполагается, что значение каждого фильтра хранится в отдельной дочерней секции.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    void ReadConfig(CfgPart cfg);

    /// <summary>
    /// Выполняет запись конфигурации для всех фильтров в списке.
    /// Предполагается, что значение каждого фильтра хранится в отдельной дочерней секции.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    void WriteConfig(CfgPart cfg);

    /// <summary>
    /// Переводит список в режим просмотра.
    /// После этого нельзя добавлять фильтры в список, но можно устанавливать значения фильтров.
    /// Этот метод вызывается в EFPConfigurableDataGridView.OnLoadConfig() в профилактических целях.
    /// </summary>
    void SetReadOnly();

    /// <summary>
    /// Начать обновление фильтров.
    /// Этот метод вызывается при показе диалога установки фильтров, чтобы предотвратить 
    /// лишние вызовы события Changed
    /// </summary>
    void BeginUpdate();

    /// <summary>
    /// Закончить обновление фильтров.
    /// Этот метод вызывается после показа диалога установки фильтров.
    /// </summary>
    void EndUpdate();

    ///// <summary>
    ///// Событие вызывается при изменении значения какого-либо фильтра, если не было вызова BeginUpdate()
    ///// </summary>
    //event EventHandler Changed;

    /// <summary>
    /// Добавление набора из нескольких фильтров
    /// </summary>
    /// <param name="filterSet"></param>
    void Add(IEFPGridFilterSet filterSet);
  }

  #endregion

  #region IEFPScrollableGridFilter

  /// <summary>
  /// Интерфейс фильтра табличного просмотра, который можно "прокручивать"
  /// вверх / вниз Alt + клавишами со стрелками.
  /// Реализуется фильтром по дате
  /// </summary>
  public interface IEFPScrollableGridFilter : IEFPGridFilter
  {
    /// <summary>
    /// Возвращает true, если можно перейти к предыдущему значению фильтра.
    /// Это возможно, если в фильтре задан какой-нибудь период (а не полуоткрытый интервал)
    /// и нет других ограничений.
    /// Используется для блокировки команды локального меню табличного просмотра.
    /// </summary>
    bool CanScrollUp { get;}

    /// <summary>
    /// Возвращает true, если можно перейти к следующему значению фильтра.
    /// Это возможно, если в фильтре задан какой-нибудь период (а не полуоткрытый интервал)
    /// и нет других ограничений.
    /// Используется для блокировки команды локального меню табличного просмотра.
    /// </summary>
    bool CanScrollDown { get;}

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
  /// Заглушка для реализации интерфейса IEFPGridFilters.
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
