// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;

/*
 * Строки табличек фильтров в отчетах (для отчета в-целом и для отдельных страниц
 */
namespace FreeLibSet.Forms
{
  /// <summary>
  /// Описание одной строки фильтра отчета.
  /// </summary>
  public sealed class EFPReportFilterItem
  {
    #region Конструктор

    /// <summary>
    /// Создает строку фильтра.
    /// Создаваемая строка должна быть добавлена в коллекцию <see cref="EFPReportFilterItems"/>.
    /// </summary>
    /// <param name="displayName">Заголовок фильтра. Не может быть пустой строкой</param>
    public EFPReportFilterItem(string displayName)
      :this(displayName, String.Empty, null)
    {
    }

    /// <summary>
    /// Создает строку фильтра.
    /// Создаваемая строка должна быть добавлена в коллекцию <see cref="EFPReportFilterItems"/>.
    /// <param name="value">Значение фильтра. Свойство <see cref="Value"/> может также устанавливаться в любое время</param>
    /// </summary>
    /// <param name="displayName">Заголовок фильтра. Не может быть пустой строкой</param>
    public EFPReportFilterItem(string displayName, string value)
      : this(displayName, value, null)
    {
    }

    /// <summary>
    /// Создает строку фильтра.
    /// Создаваемая строка должна быть добавлена в коллекцию <see cref="EFPReportFilterItems"/>.
    /// </summary>
    /// <param name="displayName">Заголовок фильтра. Не может быть пустой строкой</param>
    /// <param name="value">Значение фильтра. Свойство <see cref="Value"/> может также устанавливаться в любое время</param>
    /// <param name="imageKey">Значок фильтра. Свойство <see cref="ImageKey"/> может также устанавливаться в любое время</param>
    public EFPReportFilterItem(string displayName, string value, string imageKey)
    {
#if DEBUG
      if (String.IsNullOrEmpty(displayName))
        throw new ArgumentNullException("displayName");
#endif
      _DisplayName = displayName;
      _Value = value;
      _ImageKey = imageKey;
    }

    #endregion

    #region Свойства

    internal EFPReportFilterItems Owner { get { return _Owner; } set { _Owner = value; } }
    private EFPReportFilterItems _Owner;

    internal void CheckNotAdded()
    {
      if (_Owner != null)
        throw new InvalidOperationException("Строка фильтра уже была добавлена в коллекцию");
    }

    /// <summary>
    /// Заголовок фильтра. Задается в конструкторе.
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private readonly string _DisplayName;

    /// <summary>
    /// Значение фильтра.
    /// Устанавливается динамически.
    /// </summary>
    public string Value 
    { 
      get { return _Value; }
      set
      {
        _Value = value;
        if (_Owner != null)
          _Owner.OnChanged();
      }
    }
    private string _Value;

    /// <summary>
    /// Имя значка для фильтра из списка <see cref="EFPApp.MainImages"/>.
    /// По умолчанию используется значок "Filter"
    /// </summary>
    public string ImageKey
    {
      get
      {
        if (_ImageKey == null)
          return EFPGridFilterTools.DefaultFilterImageKey;
        else
          return _ImageKey;
      }
      set 
      { 
        _ImageKey = value;
        if (_Owner != null)
          _Owner.OnChanged();
      }
    }
    private string _ImageKey;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName + "=" + Value;
    }

    #endregion
  }

#if XXX
  public class GridReportRefFilterItem : EFPReportFilterItem
  {
  #region Конструкторы

    public GridReportRefFilterItem(string DocTypeName, Int32 DocId, string DisplayName)
      : base(GetDisplayName(DocTypeName, DisplayName), GetValue(DocTypeName, DocId))
    {
      FDocType = AccDepClientExec.DocTypes[DocTypeName];
      FDocId = DocId;
    }

    public GridReportRefFilterItem(string DocTypeName, Int32 DocId)
      : this(DocTypeName, DocId, null)
    {
    }

    private static string GetDisplayName(string DocTypeName, string DisplayName)
    {
      if (String.IsNullOrEmpty(DisplayName))
        return AccDepClientExec.DocTypes[DocTypeName].SingularTitle;
      else
        return DisplayName;
    }

    private static string GetValue(string DocTypeName, Int32 DocId)
    {
      return AccDepClientExec.DocTypes[DocTypeName].GetTextValue(DocId);
    }

    /// <summary>
    /// Эта версия конструктора удобна для отображения переменных ссылок
    /// </summary>
    /// <param name="TableId">Идентификатор таблицы документа</param>
    /// <param name="DocId">Идентификатор документа</param>
    public GridReportRefFilterItem(Int32 TableId, Int32 DocId)
      : base(GetDisplayName(TableId), AccDepClientExec.DocTypes.GetTextValue(TableId, DocId))
    {
      FDocType = AccDepClientExec.DocTypes.FindByTableId(TableId);
      FDocId = DocId;
    }

    private static string GetDisplayName(Int32 TableId)
    {
      if (TableId == 0)
        return "Нет ссылки";
      ClientDocType DocType = AccDepClientExec.DocTypes.FindByTableId(TableId);
      if (DocType == null)
        return "Неизв. таблица " + TableId.ToString();
      else
        return DocType.SingularTitle;
    }

    #endregion

  #region Свойства

    public ClientDocType DocType { get { return FDocType; } }
    private ClientDocType FDocType;

    public Int32 DocId { get { return FDocId; } }
    private Int32 FDocId;

    #endregion
  }
#endif

  /// <summary>
  /// Коллекция строк фильтра для отчета
  /// </summary>
  public class EFPReportFilterItems : IList<EFPReportFilterItem>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public EFPReportFilterItems()
    {
      _Items = new List<EFPReportFilterItem>();
    }

    #endregion

    #region Свойства

    private List<EFPReportFilterItem> _Items;

    /// <summary>
    /// Количество добавленных строк фильтра
    /// </summary>
    public int Count { get { return _Items.Count; } }

    /// <summary>
    /// Доступ к строке фильтра по индексу
    /// </summary>
    /// <param name="index">Индекс в диапазоне от 0 до <see cref="Count"/>-1</param>
    /// <returns>Объект <see cref="EFPReportFilterItem"/></returns>
    public EFPReportFilterItem this[int index]
    {
      get { return _Items[index]; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _Items[index] = value;
        OnChanged();
      }
    }

    #endregion

    #region Методы

    /// <summary>
    /// Добавление строки фильтра
    /// </summary>
    /// <param name="item">Объект строки</param>
    public void Add(EFPReportFilterItem item)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
#endif
      item.CheckNotAdded();
      item.Owner = this;
      _Items.Add(item);

      OnChanged();
    }

    /// <summary>
    /// Добавление строки фильтра
    /// </summary>
    /// <param name="displayName">Заголовок фильтра. Не может быть пустой строкой</param>
    /// <param name="value">Значение фильтра. Может быть пустой строкой</param>
    public EFPReportFilterItem Add(string displayName, string value)
    {
      EFPReportFilterItem item = new EFPReportFilterItem(displayName);
      item.Value = value;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавление строки фильтра
    /// </summary>
    /// <param name="displayName">Заголовок фильтра. Не может быть пустой строкой</param>
    /// <param name="value">Значение фильтра. Может быть пустой строкой</param>
    /// <param name="imageKey">Имя изображения в <see cref="EFPApp.MainImages"/>.
    /// Если задана пустая строка, будет использован стандартный значок фильтра</param>
    public EFPReportFilterItem Add(string displayName, string value, string imageKey)
    {
      EFPReportFilterItem item = new EFPReportFilterItem(displayName);
      item.Value = value;
      item.ImageKey = imageKey;
      Add(item);
      return item;
    }

    /// <summary>
    /// Добавление строк фильтра из другого списка
    /// </summary>
    /// <param name="items">Перечислимый список фильтров. Не может быть null</param>
    public void AddRange(IEnumerable<EFPReportFilterItem> items)
    {
#if DEBUG
      if (items == null)
        throw new ArgumentNullException("items");
#endif
      foreach (EFPReportFilterItem item in items)
      {
#if DEBUG
        if (item == null)
          throw new ArgumentException("Один из элементов равен null", "items");
#endif
        item.CheckNotAdded();
        item.Owner = this;
      }
      _Items.AddRange(items);
      OnChanged();
    }

    /// <summary>
    /// Возвращает последний добавленный объект <see cref="EFPReportFilterItem"/> или null, если нет ни одной строки фильтра
    /// </summary>
    public EFPReportFilterItem LastAdded
    {
      get
      {
        if (_Items.Count == 0)
          return null;
        else
          return _Items[_Items.Count - 1];
      }
    }

    /// <summary>
    /// Добавление строки фильтра
    /// </summary>
    /// <param name="index">Индекс для добавления строки</param>
    /// <param name="item">Объект строки</param>
    void IList<EFPReportFilterItem>.Insert(int index, EFPReportFilterItem item)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
      item.CheckNotAdded();
#endif
      item.Owner = this;
      _Items.Insert(index, item);

      OnChanged();
    }

    /// <summary>
    /// Замена списка фильтров.
    /// Комбинация вызовов <see cref="Clear()"/> и <see cref="AddRange(IEnumerable{EFPReportFilterItem})"/>. Если <paramref name="items"/> равно 0, просто
    /// очищается список строк.
    /// </summary>
    /// <param name="items">Перечислимый список фильтров</param>
    public void Assign(IEnumerable<EFPReportFilterItem> items)
    {
      if (items == null)
        Clear();
      else
      {
        _Items.Clear();
        AddRange(items);
      }
    }

    /// <summary>
    /// Удаление строки фильтра
    /// </summary>
    /// <param name="item">Удаляемый объет</param>
    /// <returns>true, если строка была найдена</returns>
    public bool Remove(EFPReportFilterItem item)
    {
      bool res = _Items.Remove(item);
      if (res)
        OnChanged();
      return res;
    }

    /// <summary>
    /// Удаление строки фильтра
    /// </summary>
    /// <param name="index">Индекс строки</param>
    public void RemoveAt(int index)
    {
      _Items.RemoveAt(index);
      OnChanged();
    }

    /// <summary>
    /// Очистка фильтра
    /// </summary>
    public void Clear()
    {
      if (_Items.Count == 0)
        return;

      _Items.Clear();
      OnChanged();
    }

    /// <summary>
    /// Возвращает индекс объекта строки фильтра.
    /// Если такого объекта нет, возвращается (-1).
    /// </summary>
    /// <param name="item">Объект строки</param>
    /// <returns>Индекс строки фильтра</returns>
    public int IndexOf(EFPReportFilterItem item)
    {
      return _Items.IndexOf(item);
    }

    /// <summary>
    /// Возвращает индекс строки фильтра с заданным наименованием.
    /// Если такого объекта нет, возвращается (-1).
    /// </summary>
    /// <param name="displayName">Заголовок строки фильтра</param>
    /// <returns>Индекс строки фильтра</returns>
    public int IndexOf(string displayName)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        if (String.Equals(_Items[i].DisplayName, displayName))
          return i;
      }
      return -1;
    }

    /// <summary>
    /// Возвращает true при наличии строки фильтра
    /// </summary>
    /// <param name="item">Объект строки</param>
    /// <returns>Наличие</returns>
    public bool Contains(EFPReportFilterItem item)
    {
      return _Items.Contains(item);
    }

    /// <summary>
    /// Копирование в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Первый индекс в заполняемом массиве</param>
    public void CopyTo(EFPReportFilterItem[] array, int arrayIndex)
    {
      _Items.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Копирование в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    public void CopyTo(EFPReportFilterItem[] array)
    {
      _Items.CopyTo(array);
    }

    /// <summary>
    /// Копирование в массив
    /// </summary>
    /// <returns>Новый массив</returns>
    public EFPReportFilterItem[] ToArray()
    {
      return _Items.ToArray();
    }

    #endregion

    #region Виртуальные методы

    /// <summary>
    /// Вызывается при всех изменениях в списке фильтров
    /// </summary>
    internal protected virtual void OnChanged()
    {
    }

    #endregion

    #region IEnumerable<GridReportFilterItem> Members

    /// <summary>
    /// Возвращает перечислитель по объектам <see cref="EFPReportFilterItem"/>
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<EFPReportFilterItem> GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    #endregion

    #region ICollection<GridReportFilterItem> Members

    bool ICollection<EFPReportFilterItem>.IsReadOnly
    {
      get { return false; }
    }

    #endregion
  }

  /// <summary>
  /// Реализация списка фильтров для одной страницы отчета <see cref="EFPReportPage"/>.
  /// </summary>
  public class EFPReportPageFilterItems : EFPReportFilterItems
  {
    #region Конструктор

    internal EFPReportPageFilterItems(EFPReportPage page)
    {
      _Page = page;
      _UpdateCount = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Страница, к которой относится отчет
    /// </summary>
    public EFPReportPage Page { get { return _Page; } }
    private readonly EFPReportPage _Page;

    /// <summary>
    /// Возвращает всплывающую подсказку, собранную из строк фильтра в виде длинной строки,
    /// содержащей пары "DisplayName=Value".
    /// Используется методом <see cref="EFPReportPage.ToolTipTextFromFilters()"/>.
    /// </summary>
    public string ToolTipText
    {
      get
      {
        if (Count == 0)
          return "Без фильтра";
        else
        {
          StringBuilder sb = new StringBuilder();
          for (int i = 0; i < Count; i++)
          {
            if (i > 0)
              sb.Append(", ");
            sb.Append(this[i].DisplayName);
            sb.Append("=");
            sb.Append(this[i].Value);
          }
          return sb.ToString();
        }
      }
    }

    #endregion

    #region Методы

    private int _UpdateCount;

    /// <summary>
    /// Начать обновление списка фильтров.
    /// Подавляет последующие вызовы метода <see cref="OnChanged()"/>.
    /// Допускаются вложенные пары вызовов <see cref="BeginUpdate()"/> / <see cref="EndUpdate()"/>.
    /// </summary>
    public void BeginUpdate()
    {
      _UpdateCount++;
    }

    /// <summary>
    /// Закончить обновление списка фильтров.
    /// Допускаются вложенные пары вызовов <see cref="BeginUpdate()"/> / <see cref="EndUpdate()"/>.
    /// Вызывает <see cref="OnChanged()"/>.
    /// </summary>
    public void EndUpdate()
    {
      _UpdateCount--;
      if (_UpdateCount == 0)
        OnChanged();
    }

    /// <summary>
    /// Инициализирует фильтры страницы, если нет незакрытых вызовов <see cref="BeginUpdate()"/>.
    /// </summary>
    internal protected override void OnChanged()
    {
      if (_UpdateCount > 0)
        return;
      _Page.DoSetFilterItems();
    }

    #endregion
  }
}
