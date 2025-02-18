// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;
using System.ComponentModel;
using FreeLibSet.Collections;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Описание порядка сортировки для табличного просмотра.
  /// Структура содержит имя основного столбца сортировки и порядок сортировки.
  /// </summary>
  public struct EFPDataGridViewSortInfo
  {
    #region Конструктор

    /// <summary>
    /// Инициализация структуры
    /// </summary>
    /// <param name="clickableColumnNames">Имена столбцов <see cref="EFPDataGridViewColumn.Name"/> по которым можно щелкать мышью для сортировки. 
    /// Если задать несколько столбцов, тогда будет использоваться тот столбец, который расположен ближе к левому краю просмотра</param>
    /// <param name="direction">Направление отображаемого порядка сортировки: по возрастанию или по убыванию</param>
    public EFPDataGridViewSortInfo(string[] clickableColumnNames, ListSortDirection direction)
    {
#if DEBUG
      if (clickableColumnNames == null)
        throw new ArgumentNullException("clickableColumnNames");
#endif
      //if (clickableColumnNames.Length == 0)
      //  throw new ArgumentException("Не задано ни одного имени столбца", "clickableColumnNames");

      _ClickableColumnNames = clickableColumnNames;
      _Direction = direction;
    }


    /// <summary>
    /// Инициализация структуры
    /// </summary>
    /// <param name="clickableColumnNames">Имя столбца <see cref="EFPDataGridViewColumn.Name"/> по которому можно щелкать мышью для сортировки. 
    /// Можно задать несколько столбцов, разделенных запятыми, тогда будет использоваться тот столбец, который расположен ближе к левому краю просмотра</param>
    /// <param name="direction">Направление отображаемого порядка сортировки: по возрастанию или по убыванию</param>
    public EFPDataGridViewSortInfo(string clickableColumnNames, ListSortDirection direction)
    {
#if DEBUG
      if (String.IsNullOrEmpty(clickableColumnNames))
        throw new ArgumentNullException("clickableColumnNames");
#endif

      _ClickableColumnNames = clickableColumnNames.Split(',');
      _Direction = direction;
    }

    /// <summary>
    /// Инициализация структуры.
    /// Используется сортировка по возрастанию.
    /// </summary>
    /// <param name="clickableColumnNames">Имя столбца <see cref="EFPDataGridViewColumn.Name"/> по которому можно щелкать мышью для сортировки. 
    /// Можно задать несколько столбцов, разделенных запятыми, тогда будет использоваться тот столбец, который расположен ближе к левому краю просмотра</param>
    public EFPDataGridViewSortInfo(string clickableColumnNames)
      : this(clickableColumnNames, ListSortDirection.Ascending)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имена столбцов <see cref="EFPDataGridViewColumn.Name"/>, однин из которых можно использовать для сортировки.
    /// Если в просмотре есть несколько столбцов из списка, то выбирается тот, который виден ближе к левому краю просмотра.
    /// Если в просмотре нет ни одного видимого столбца, который можно использовать, то порядок сортировки нельзя задать
    /// щелчком мыши по заголовку, но можно выбрать с помощью локального меню.
    /// </summary>
    public string[] ClickableColumnNames
    {
      get
      {
        if (_ClickableColumnNames == null)
          return DataTools.EmptyStrings;
        else
          return _ClickableColumnNames;
      }
    }
    private readonly string[] _ClickableColumnNames;

    /// <summary>
    /// false - сортировка по возрастанию (обычная), true - по убыванию
    /// </summary>
    public ListSortDirection Direction { get { return _Direction; } }
    private readonly ListSortDirection _Direction;

    /// <summary>
    /// Возвращает true, если структура не инициализирована
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        if (ClickableColumnNames == null)
          return true;
        else
          return ClickableColumnNames.Length == 0;
      }
    }

    /// <summary>
    /// Текстовое представление.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (IsEmpty)
        return "[ Empty ]";
      else
        return String.Format("{0} {1}",
          String.Join(",", _ClickableColumnNames),
          UITools.ToString(_Direction));
    }

    #endregion

    #region Статическое свойство

    /// <summary>
    /// Пустое значение
    /// </summary>
    public static readonly EFPDataGridViewSortInfo Empty = new EFPDataGridViewSortInfo();

    #endregion
  }

  /// <summary>
  /// Класс для задания порядка сортировки строк в табличном просмотре.
  /// Свойство <see cref="Sort"/> соответствует сортировке с помощью <see cref="DataView.Sort"/>.
  /// Эти же объекты используются в коллекции <see cref="EFPGridProducer.Orders"/>.
  /// В ExtDBDocForms.dll определен класс-наследник, реализующий расширенную сортировку.
  /// </summary>
  public class EFPDataViewOrder : IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает порядок сортировки по одному полю. 
    /// Устанавливает свойства <see cref="Name"/> и <see cref="Sort"/> одинаковыми.
    /// </summary>
    /// <param name="sort">Имя и порядок сортировки. Не может быть пустой строкой</param>
    public EFPDataViewOrder(string sort)
      : this(sort, sort)
    {
    }

    /// <summary>
    /// Создает порядок сортировки
    /// </summary>
    /// <param name="name">Имя порядка сортировки, используемый для сохранения в секции конфигурации.
    /// Не может быть пустой строкой. Должен быть уникальным в пределах коллекции <see cref="EFPDataViewOrders"/></param>
    /// <param name="sort">Порядок сортировки в формате <see cref="DataView.Sort"/>. 
    /// Может быть пустой строкой.</param>
    public EFPDataViewOrder(string name, string sort)
    {
      if (String.IsNullOrEmpty(name))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("name");

      // 08.07.2024 Эта проверка неправильная. Запятая может использоваться
      //if (name.IndexOf(',') >= 0)
      //  throw new ArgumentException("Недопустимый символ \",\" в имени \"" + name + "\"", "name");

      //if (String.IsNullOrEmpty(sort))
      //  throw new ArgumentNullException("sort");
      if (!String.IsNullOrEmpty(sort))
      {
        if (sort.IndexOf('!') >= 0)
          throw new ArgumentException("You should not use \"!\" char. Use DESC instead", "sort");
      }

      _Name = name;
      if (sort == null)
        _Sort = String.Empty;
      else
        _Sort = sort;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Строка, используемое для идентификации текущего порядка в секции конфигурации.
    /// Задается в конструкторе.
    /// </summary>
    public string Name { get { return _Name; } }
    private readonly string _Name;

    string IObjectWithCode.Code { get { return _Name; } }

    /// <summary>
    /// Порядок сортировки для <see cref="DataView.Sort"/>.
    /// Может быть пустой строкой.
    /// Задается в конструкторе.
    /// </summary>
    public string Sort { get { return _Sort; } }
    private readonly string _Sort;

    /// <summary>
    /// Название для позиции меню.
    /// Наличие символа "амперсанд" не допускается.
    /// Если не задано в явном виде, свойство возвращает свойство <see cref="Sort"/>.
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
          return _Name;
        else
          return _DisplayName;
      }
      set
      {
        _DisplayName = value;
      }
    }
    private string _DisplayName;

    /// <summary>
    /// Столбцы, по которым можно кликать мышью для сортировки
    /// </summary>
    public EFPDataGridViewSortInfo SortInfo { get { return _SortInfo; } set { _SortInfo = value; } }
    private EFPDataGridViewSortInfo _SortInfo;

    /// <summary>
    /// Возвращает свойство <see cref="DisplayName"/>
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName;
    }

    /// <summary>
    /// Изображение для меню (треугольник)
    /// </summary>
    public string ImageKey
    {
      get
      {
        return SortInfo.Direction == ListSortDirection.Ascending ? "Up" : "Down";
      }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Применение порядка сортировки к табличного просмотру.
    /// Непереопределенный метод устанавливает свойство <see cref="DataView.Sort"/>. 
    /// Для доступа к набору данных используется свойство <see cref="IEFPDataView.SourceAsDataView"/>.
    /// </summary>
    /// <param name="controlProvider"></param>
    public virtual void PerformSort(IEFPDataView controlProvider)
    {
      DataView dv = controlProvider.SourceAsDataView;
      if (dv == null)
        throw new InvalidDataSourceException(Res.EFPDataView_Err_NoDataView);

      dv.Sort = Sort;
    }

    /// <summary>
    /// Добавляет в список <paramref name="columnNames"/> имена полей, используемых для сортировки.
    /// Извлекает их из свойства <see cref="Sort"/>.
    /// Не путать со столбцами табличного просмотра, используемыми для переключения сортировки.
    /// </summary>
    /// <param name="columnNames">Заполняемый список</param>
    public virtual void GetColumnNames(IList<string> columnNames)
    {
#if DEBUG
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
#endif

      string[] names = DataTools.GetDataViewSortColumnNames(Sort);
      for (int i = 0; i < names.Length; i++)
        columnNames.Add(names[i]);
    }

    /// <summary>
    /// Возвращает true, если <paramref name="columnNames"/> содержит все столбцы, которые нужны для сортировки.
    /// Сначала вызывает метод <see cref="GetColumnNames(IList{string})"/> для получения списка требуемых столбцов.
    /// </summary>
    /// <param name="columnNames">Проверяемая коллекция со списком столбцов</param>
    /// <returns>Результат проверки коллекции</returns>
    public bool AreAllColumnsPresented(ICollection<string> columnNames)
    {
      List<string> required = new List<string>(); // можно не заморачиваться с SingleScopeList. Наплевать, если будут повторы
      GetColumnNames(required);
      for (int i = 0; i < required.Count; i++)
      {
        if (!columnNames.Contains(required[i]))
          return false;
      }
      return true;
    }

    #endregion
  }

  /// <summary>
  /// Реализация свойства <see cref="EFPDataGridView.Orders"/>.
  /// Эта же коллекция используется в <see cref="EFPGridProducer.Orders"/>.
  /// </summary>
  public class EFPDataViewOrders : NamedList<EFPDataViewOrder>
  {
    #region Методы добавления для EFPDataViewOrder

    /// <summary>
    /// Добавляет новый порядок сортировки из одного поля.
    /// Свойство <see cref="EFPDataViewOrder.Name"/> будет равно <paramref name="sort"/>.
    /// </summary>
    /// <param name="sort">Порядок сортировки в формате <see cref="DataView.Sort"/>. 
    /// Может быть пустой строкой, в этом случае <see cref="EFPDataViewOrder.Name"/> будет равно "*".</param>
    /// <param name="displayName">Отображаемое название для меню</param>
    /// <param name="sortInfo">Описатель столбца(ов) табличного просмотра для выбора этого порядка сортировки мышью.
    /// Если задано пустое значение <see cref="EFPDataGridViewSortInfo.Empty"/>, 
    /// то будет назначен первое используемое поле из <paramref name="sort"/>.</param>
    /// <returns>Объект сортировки</returns>
    public EFPDataViewOrder Add(string sort, string displayName, EFPDataGridViewSortInfo sortInfo)
    {
      string name = sort;
      if (String.IsNullOrEmpty(sort))
        name = "*"; // 24.01.2022
      EFPDataViewOrder order = new EFPDataViewOrder(name, sort);
      if (sortInfo.IsEmpty)
      {
        string[] aColNames;
        ListSortDirection[] aDirs;
        DataTools.GetDataViewSortColumnNames(sort, out aColNames, out aDirs);
        if (aDirs.Length > 0)
          order.SortInfo = new EFPDataGridViewSortInfo(aColNames[0], aDirs[0]);
      }
      else
        order.SortInfo = sortInfo;
      if (!String.IsNullOrEmpty(displayName))
        order.DisplayName = displayName;
      Add(order);
      return order;
    }

    /// <summary>
    /// Добавляет новый порядок сортировки.
    /// Описатель столбца табличного просмотра, используемого для щелчка мышью, создается автоматически.
    /// </summary>
    /// <param name="sort">Порядок сортировки в формате <see cref="DataView.Sort"/>. 
    /// Может быть пустой строкой, в этом случае <see cref="EFPDataViewOrder.Name"/> будет равно "*".</param>
    /// <param name="displayName">Отображаемое название для меню</param>
    /// <returns>Объект сортировки</returns>
    public EFPDataViewOrder Add(string sort, string displayName)
    {
      return Add(sort, displayName, EFPDataGridViewSortInfo.Empty);
    }

    /// <summary>
    /// Добавляет новый порядок сортировки из одного поля.
    /// Отображаемое название для меню равно имени первого столбца в <paramref name="sort"/>.
    /// Описатель столбца табличного просмотра, используемого для щелчка мышью, создается автоматически.
    /// </summary>
    /// <param name="sort">Порядок сортировки в формате <see cref="DataView.Sort"/>. 
    /// Может быть пустой строкой, в этом случае <see cref="EFPDataViewOrder.Name"/> будет равно "*".</param>
    /// <returns>Объект сортировки</returns>
    public EFPDataViewOrder Add(string sort)
    {
      EFPDataViewOrder order = Add(sort, String.Empty, EFPDataGridViewSortInfo.Empty);
      InitDefaultDisplayName(order);
      return order;
    }

    private static void InitDefaultDisplayName(EFPDataViewOrder order)
    {
      string[] aColNames;
      ListSortDirection[] aDirs;
      DataTools.GetDataViewSortColumnNames(order.Sort, out aColNames, out aDirs);
      if (aColNames.Length > 0)
      {
        order.DisplayName = aColNames[0];
        if (aDirs[0] == ListSortDirection.Descending)
          order.DisplayName += " ("+UITools.ToString(aDirs[0])+")";
      }
    }

    /// <summary>
    /// Возвращает ссылку на последний добавленный в список порядок сортировки <see cref="EFPDataViewOrder"/>.
    /// Возвращает null, если список пуст.
    /// </summary>
    public EFPDataViewOrder LastAdded
    {
      get
      {
        if (Count == 0)
          return null;
        else
          return this[Count - 1];
      }
    }

    /// <summary>
    /// Добавляет новый порядок сортировки, используя <see cref="DataViewSortBuilder"/>.
    /// Свойство <see cref="EFPDataViewOrder.Name"/> будет равно <paramref name="sortBuilder"/>.ToString().
    /// После добавления будет вызван метод <see cref="DataViewSortBuilder.Clear()"/>, 
    /// чтобы построитель можно было сразу использовать повторно.
    /// </summary>
    /// <param name="sortBuilder">Заполненный объект <see cref="DataViewSortBuilder"/></param>
    /// <param name="displayName">Отображаемое название для меню</param>
    /// <param name="sortInfo">Описатель столбца(ов) табличного просмотра для выбора этого порядка сортировки мышью.
    /// Если задано пустое значение <see cref="EFPDataGridViewSortInfo.Empty"/>, 
    /// то будет назначен первое используемое поле из <paramref name="sortInfo"/>.</param>
    /// <returns>Объект сортировки</returns>
    public EFPDataViewOrder Add(DataViewSortBuilder sortBuilder, string displayName, EFPDataGridViewSortInfo sortInfo)
    {
#if DEBUG
      if (sortBuilder == null)
        throw new ArgumentNullException("sortBuilder");
#endif

      EFPDataViewOrder order = Add(sortBuilder.ToString(), displayName, sortInfo);
      sortBuilder.Clear();
      return order;
    }

    /// <summary>
    /// Добавляет новый порядок сортировки, используя <see cref="DataViewSortBuilder"/>.
    /// Описатель столбца табличного просмотра, используемого для щелчка мышью, создается автоматически.
    /// После добавления будет вызван метод <see cref="DataViewSortBuilder.Clear()"/>, чтобы построитель можно было сразу использовать повторно.
    /// </summary>
    /// <param name="sortBuilder">Заполненный объект <see cref="DataViewSortBuilder"/></param>
    /// <param name="displayName">Отображаемое название для меню</param>
    /// <returns>Объект сортировки</returns>
    public EFPDataViewOrder Add(DataViewSortBuilder sortBuilder, string displayName)
    {
      return Add(sortBuilder, displayName, EFPDataGridViewSortInfo.Empty);
    }

    /// <summary>
    /// Добавляет новый порядок сортировки, используя <see cref="DataViewSortBuilder"/>.
    /// Отображаемое название для меню равно имени первого столбца в <paramref name="sortBuilder"/>.
    /// Описатель столбца табличного просмотра, используемого для щелчка мышью, создается автоматически.
    /// После добавления будет вызван метод <see cref="DataViewSortBuilder.Clear()"/>, чтобы построитель можно было сразу использовать повторно.
    /// </summary>
    /// <param name="sortBuilder">Заполненный объект <see cref="DataViewSortBuilder"/></param>
    /// <returns>Объект сортировки</returns>
    public EFPDataViewOrder Add(DataViewSortBuilder sortBuilder)
    {
      EFPDataViewOrder order = Add(sortBuilder, String.Empty, EFPDataGridViewSortInfo.Empty);
      InitDefaultDisplayName(order);
      return order;
    }

    #endregion

    #region Методы добавления для EFPDBxViewOrder

    /// <summary>
    /// Создает порядок сортировки, производный от <see cref="EFPDataViewOrder"/> для заданного <see cref="DBxOrder"/> и добавляет его в список.
    /// Для выполнения сортировки по выражениям в таблицу могут динамически добавляться вычисляемые столбцы (с установленным свойством <see cref="DataColumn.Expression"/>).
    /// У возвращаемого объекта не устанавливается свойство <see cref="EFPDataViewOrder.Sort"/>.
    /// Этот метод нельзя использовать, если исходная таблица данных <see cref="DataTable"/> разделяется между потоками.
    /// </summary>
    /// <param name="order">Порядок сортировки для SELECT .. ORDER BY</param>
    /// <param name="displayName">Отображаемое имя для меню</param>
    /// <param name="sortInfo">Столбец табличного просмотра для нажатия мышью</param>
    /// <returns>Созданный порядок сортировки</returns>
    public EFPDataViewOrder Add(DBxOrder order, string displayName, EFPDataGridViewSortInfo sortInfo)
    {
#if DEBUG
      if (order == null)
        throw new ArgumentNullException("order");
#endif     
      string name = order.ToString();
      FreeLibSet.Forms.Data.EFPDBxViewOrder item = new FreeLibSet.Forms.Data.EFPDBxViewOrder(order.ToString(), order);
      item.DisplayName = displayName;
      if (!sortInfo.IsEmpty)
        item.SortInfo = sortInfo;
      base.Add(item);
      //// Если первое поле в порядке сортировки присутствует в просмотре, то столбец
      //// можно щелкать по заголовку
      //EFPDataGridViewColumn Column = GetUsedColumn(Item); // 19.06.2019
      //if (Column != null)
      //  Column.GridColumn.SortMode = DataGridViewColumnSortMode.Programmatic;
      return item;
    }

    /// <summary>
    /// Создает порядок сортировки, производный от <see cref="EFPDataViewOrder"/> для заданного <see cref="DBxOrder"/> и добавляет его в список.
    /// Для выполнения сортировки по выражениям в таблицу могут динамически добавляться вычисляемые столбцы (с установленным свойством <see cref="DataColumn.Expression"/>).
    /// У возвращаемого объекта не устанавливается свойство <see cref="EFPDataViewOrder.Sort"/>.
    /// Этот метод нельзя использовать, если исходная таблица данных <see cref="DataTable"/> разделяется между потоками.
    /// </summary>
    /// <param name="order">Порядок сортировки для SELECT .. ORDER BY</param>
    /// <param name="displayName">Отображаемое имя для меню</param>
    /// <returns>Созданный порядок сортировки</returns>
    public EFPDataViewOrder Add(DBxOrder order, string displayName)
    {
      return Add(order, displayName, EFPDataGridViewSortInfo.Empty);
    }

    #endregion

    #region Другие методы

    /// <summary>
    /// Возвращает первый подходящий объект <see cref="EFPDataViewOrder"/>, который выбирается при щелчке мыши
    /// на заголовке столбца с заданным именем. См. метод <see cref="IndexOfItemForGridColumn(string)"/>.
    /// Возвращает null, если столбец не предназначен для щелкания.
    /// </summary>
    /// <param name="columnName">Имя столбца (<see cref="EFPDataGridViewColumn.Name"/>)</param>
    /// <returns><see cref="EFPDataViewOrder "/> или null</returns>
    public EFPDataViewOrder FindForGridColumn(string columnName)
    {
      int p = IndexOfItemForGridColumn(columnName);
      if (p < 0)
        return null;
      else
        return this[p];
    }

    /// <summary>
    /// Выполняет поиск по имени столбца табличного просмотра, щелчок мыши на котором выполняет сортировку.
    /// Используется свойство <see cref="EFPDataGridViewSortInfo.ClickableColumnNames"/>.
    /// Возвращается индекс первого подходящего порядка сортировки. В списке может быть несколько
    /// порядков сортировки, переключаемых щелчком на этом столбце.
    /// Возвращает (-1), если столбец не предназначен для щелкания.
    /// </summary>
    /// <param name="columnName">Имя столбца (<see cref="EFPDataGridViewColumn.Name"/>)</param>
    /// <returns>Индекс первого подходящего <see cref="EFPDataViewOrder"/> или (-1)</returns>
    public int IndexOfItemForGridColumn(string columnName)
    {
      if (String.IsNullOrEmpty(columnName))
        return -1;

      for (int i = 0; i < Count; i++)
      {
        if (this[i].SortInfo.IsEmpty)
          continue;
        if (Array.IndexOf<string>(this[i].SortInfo.ClickableColumnNames, columnName) >= 0)
          return i;
      }
      return -1;
    }

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    #endregion

    #region Диалог выбора порядка строк

    /// <summary>
    /// Показать окно выбора порядка строк.
    /// Реальный порядок строк в просмотре не меняется.
    /// </summary>
    /// <param name="orderIndex">Индекс выбранного порядка строк (вход и выход)</param>
    /// <returns>true, если выбор сделан</returns>
    public bool ShowSelectDialog(ref int orderIndex)
    {
      ListSelectDialog dlg = new ListSelectDialog();
      dlg.Title = EFPCommandItem.RemoveMnemonic(Res.Cmd_Menu_RowOrder);
      dlg.ImageKey = "OrderAZ";
      dlg.Items = new string[Count];
      dlg.ImageKeys = new string[Count];
      dlg.ConfigSectionName = "EFPDataGridViewOrdersDialog";
      for (int i = 0; i < Count; i++)
      {
        dlg.Items[i] = (i + 1).ToString() + ". " + this[i].DisplayName;
        dlg.ImageKeys[i] = this[i].ImageKey;
      }
      dlg.SelectedIndex = orderIndex;
      bool res = dlg.ShowDialog() == DialogResult.OK;
      if (res)
        orderIndex = dlg.SelectedIndex;
      return res;
    }

    #endregion
  }
}

namespace FreeLibSet.Forms.Data
{
  /// <summary>
  /// Расширенная сортировка табличного просмотра, основанного на наборе данных.
  /// Поддерживает задание полей с помощью выражений <see cref="DBxOrder"/>, в том числе, содержащих функции.
  /// Для этого в набор данных добавляются виртуальные вычисляемые столбцы.
  /// </summary>
  internal class EFPDBxViewOrder : EFPDataViewOrder
  {
    #region Конструктор

    /// <summary>
    /// Создает объект сортировки
    /// </summary>
    /// <param name="name">Условное имя для порядка сортировки</param>
    /// <param name="order">Порядок сортировки для ORDER BY</param>
    public EFPDBxViewOrder(string name, DBxOrder order)
      : base(name, String.Empty)
    {
      if (order == null)
        throw new ArgumentNullException("order");
      _Order = order;

      string columnName;
      ListSortDirection sortOrder;
      _Order.GetFirstColumnInfo(out columnName, out sortOrder);
      SortInfo = new EFPDataGridViewSortInfo(columnName, sortOrder);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Порядок сортировки для SQL-запроса SELECT .. ORDER BY
    /// </summary>
    public DBxOrder Order { get { return _Order; } }
    private readonly DBxOrder _Order;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName + " (" + Order.ToString() + ")";
    }

    #endregion

    #region Выполнение сортировки

    /// <summary>
    /// Реализация интерфейса <see cref="INamedValuesAccess"/>.
    /// Возвращает значения по умолчанию для всех столбцов данных таблицы.
    /// Используется для определения типа данных вычисляемого столбца.
    /// </summary>
    private class DefValAccess : INamedValuesAccess
    {
      #region Конструктор

      public DefValAccess(DataTable table)
      {
        _Table = table;
      }

      private DataTable _Table;

      #endregion

      #region INamedValuesAccess Members

      public object GetValue(string name)
      {
        int p = _Table.Columns.IndexOf(name);
        if (p < 0)
          throw ExceptionFactory.ArgUnknownColumnName("name", _Table, name);

        return DataTools.GetEmptyValue(_Table.Columns[p].DataType);
      }

      public bool Contains(string name)
      {
        return _Table.Columns.Contains(name);
      }

      public string[] GetNames()
      {
        string[] names = new string[_Table.Columns.Count];
        for (int i = 0; i < names.Length; i++)
          names[i] = _Table.Columns[i].ColumnName;
        return names;
      }

      #endregion
    }

    /// <summary>
    /// Выполняет сортировку в табличного просмотре.
    /// Источником данных должен быть объект <see cref="DataView"/> (свойство <see cref="EFPDataGridView.SourceAsDataView"/> должно возвращать непустое значение).
    /// Устанавливает свойство <see cref="DataView.Sort"/>.
    /// Если среди компонентов сортировки есть объекты, отличные от <see cref="DBxColumn"/>,
    /// то в таблицу добавляются вычисляемые столбцы "$$Sort_XXX", для которых устанавливается свойство <see cref="DataColumn.Expression"/>.
    /// </summary>     
    /// <param name="controlProvider">Провайдер табличного просмотра, для которого нужно установить порядок сортировки.</param>
    public override void PerformSort(IEFPDataView controlProvider)
    {
      //if (!(controlProvider is EFPDBxGridView))
      //  throw new InvalidOperationException("Неправильный тип ControlProvider: " + controlProvider.GetType().ToString());

      DataView dv = controlProvider.SourceAsDataView;
      if (dv == null)
        throw new InvalidDataSourceException(Res.EFPDataView_Err_NoDataView);

      // Обычно, порядок сортировки задается нормальными полями и вычисляемые столбцы
      // не нужны. При наличии выражения, например, DBxOrderColumnIfNull, его нельзя
      // задавать в DataView.Sort. Вместо этого создается вычисляемый столбец,
      // который содержит установленное свойство Expression. По нему выполняется
      // сортировка.
      // В одном DataOrder может быть несколько выражений, через запятую, а, также,
      // обычные поля. Вычисляемые столбцы используются только для вычисляемых
      // DataOrderItem, а для простых полей не используются.

      DBxSqlBuffer buf = new DBxSqlBuffer();
      buf.Clear();
      for (int i = 0; i < Order.Parts.Length; i++)
      {
        if (i > 0)
          buf.SB.Append(',');
        if ((Order.Parts[i].Expression) is DBxColumn)
          // Простое поле
          buf.FormatExpression(Order.Parts[i].Expression, new DBxFormatExpressionInfo());
        else
        {
          // Вычисляемое поле
          DBxSqlBuffer buf2 = new DBxSqlBuffer();
          buf2.Clear();
          buf2.FormatExpression(Order.Parts[i].Expression, new DBxFormatExpressionInfo());
          string expr = buf2.SB.ToString();
          // Имя столбца
          string exprColName = "$$Sort_" + DataTools.MD5SumFromString(expr);

          // 16.10.2019
          // Тип данных для столбца проверяем по другому
          DefValAccess dva = new DefValAccess(dv.Table);
          object defVal = Order.Parts[i].Expression.GetValue(dva);
          if (defVal == null)
            throw new NullReferenceException(String.Format(Res.EFPDataViewOrder_Err_NoDefaultValue,
              expr, DisplayName));
          Type dataType = defVal.GetType();


          // Столбец добавляется только при необходимости, чтобы исключить размножение
          // столбцов при каждом переключении сортировки
          if (!dv.Table.Columns.Contains(exprColName))
          {
            DataColumn col = new DataColumn(exprColName, dataType, expr);
            dv.Table.Columns.Add(col);
          }

          // В Sort добавляется имя вычисляемого столбца
          buf.SB.Append(exprColName);
        }

        // Признак обратной сортировки
        if (Order.Parts[i].SortOrder == ListSortDirection.Descending)
          buf.SB.Append(" DESC");
      }
      dv.Sort = buf.SB.ToString();
    }
    #endregion
  }
}

