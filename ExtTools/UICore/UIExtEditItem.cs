// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.DependedValues;
using FreeLibSet.Core;

namespace FreeLibSet.UICore
{
  #region Интерфейс IUIExtEditItem

  /// <summary>
  /// Интерфейс одного элемента, выполняющего редактирование поля или полей
  /// в редакторе документа
  /// </summary>
  public interface IUIExtEditItem
  {
    /// <summary>
    /// Вызывается перед вызовом <see cref="ReadValues()"/> для всех элементов. Метод может 
    /// временно отключать инициализацию зависимых полей.
    /// </summary>
    void BeforeReadValues();

    /// <summary>
    /// Вызывается при инициализации просмотра.
    /// Должен перенести данные из полей набора данных (обычно объекта <see cref="FreeLibSet.Data.DBxExtValue"/>)
    /// в управляющий элемент.
    /// </summary>
    void ReadValues();

    /// <summary>
    /// Вызывается после того, как метод <see cref="ReadValues()"/> был вызыван для всех объектов
    /// в группе. Должен отменить действия, выполненные <see cref="BeforeReadValues()"/>.
    /// </summary>
    void AfterReadValues();

    /// <summary>
    /// Вызывается при записи значений.
    /// Должен перенести данные из управляющего элемента в набор данных (обычно объект <see cref="FreeLibSet.Data.DBxExtValue"/>).
    /// </summary>
    void WriteValues();

    /// <summary>
    /// Объект, с помощью которого реализация <see cref="IUIExtEditItem"/> может оповещать редактор документа / поддокумента об изменениях в данных, которые сделал пользователь в процессе редактирования.
    /// </summary>
    DepChangeInfo ChangeInfo { get; }
  }

  #endregion

  /// <summary>
  /// Список объектов <see cref="IUIExtEditItem"/> с дополнительными методами чтения и записи значений.
  /// </summary>
  public class UIExtEditList : List<IUIExtEditItem>
  {
    #region Чтение и запись значений

    /// <summary>
    /// Вызов методов <see cref="IUIExtEditItem.BeforeReadValues()"/>, <see cref="IUIExtEditItem.ReadValues()"/> и <see cref="IUIExtEditItem.AfterReadValues()"/> с перехватом ошибок.
    /// Если в <see cref="IUIExtEditItem.ReadValues()"/> возникает исключение, то методы <see cref="IUIExtEditItem.AfterReadValues()"/> все равно вызываются, прежде чем перевыбросить исключение.
    /// Если исключение возникает в <see cref="IUIExtEditItem.BeforeReadValues()"/> или <see cref="IUIExtEditItem.AfterReadValues()"/>, то оно не перехватывается.
    /// </summary>
    public void ReadValues()
    {
      ReadValues(true);
    }

    /// <summary>
    /// Вызов методов <see cref="IUIExtEditItem.BeforeReadValues()"/>, <see cref="IUIExtEditItem.ReadValues()"/> и <see cref="IUIExtEditItem.AfterReadValues()"/> с перехватом ошибок.
    /// Если в <see cref="IUIExtEditItem.ReadValues()"/> возникает исключение, то методы <see cref="IUIExtEditItem.AfterReadValues()"/> все равно вызываются, прежде чем перевыбросить исключение.
    /// Если исключение возникает в <see cref="IUIExtEditItem.BeforeReadValues()"/> или <see cref="IUIExtEditItem.AfterReadValues()"/>, то оно не перехватывается.
    /// </summary>
    /// <param name="throwException">
    /// <para>
    /// Если true, то при первой ошибке вызова <see cref="IUIExtEditItem.ReadValues()"/> оставшиеся
    /// значения не считываются и исключение выбрасывается в вызывающий код. Результат не возвращается.
    /// </para>
    /// <para>
    /// Если false, то исключение перехватывается и помещается в список, чтение продолжается. 
    /// Собранный список исключений возвращается для обработки вызывающим кодом.
    /// </para>
    /// </param>
    /// <returns>
    /// Если <paramref name="throwException"/>=false - то массив возникших исключений. 
    /// Если ошибок не возникло, то возвращается пустой массив.
    /// Если <paramref name="throwException"/>=true, то возвращается null.
    /// </returns>
    public Exception[] ReadValues(bool throwException)
    {
      foreach (IUIExtEditItem item in this)
        item.BeforeReadValues();

      List<Exception> lstEx = null;

      try
      {
        foreach (IUIExtEditItem item in this)
        {
          try
          {
            item.ReadValues();
          }
          catch (Exception e)
          {
            if (item.ChangeInfo == null)
              e.Data["DisplayName"] = item.ToString();
            else
              e.Data["DisplayName"] = item.ChangeInfo.DisplayName;
            if (throwException)
              throw;
            else
            {
              if (lstEx == null)
                lstEx = new List<Exception>();
              lstEx.Add(e);
            }
          }
        }
      }
      finally
      {
        foreach (IUIExtEditItem item in this)
          item.AfterReadValues();
      }

      if (throwException)
        return null;
      else
      {
        if (lstEx == null)
          return EmptyArray<Exception>.Empty;
        else
          return lstEx.ToArray();
      }
    }

    /// <summary>
    /// Вызов метода <see cref="IUIExtEditItem.WriteValues()"/> для всех объектов в списке.
    /// </summary>
    public void WriteValues()
    {
      foreach (IUIExtEditItem item in this)
        item.WriteValues();
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс, реализующий интерфейс <see cref="IUIExtEditItem"/>.
  /// Если пользовательский код должен реализовывать собственный переходник данных
  /// и не требуется наследование от другого класса, можно наследовать этот класс.
  /// </summary>
  public abstract class UIExtEditItem : IUIExtEditItem
  {
    #region Конструктор

    /// <summary>
    /// Создает объект и <see cref="DepChangeInfoItem"/> для свойства <see cref="ChangeInfo"/>.
    /// </summary>
    public UIExtEditItem()
    {
      _ChangeInfo = new DepChangeInfoItem();
    }

    #endregion

    #region IExtEditItem Members

    /// <summary>
    /// Вызывается перед вызовами <see cref="ReadValues()"/> для всех элементов. Метод может 
    /// временно отключать инициализацию зависимых полей.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    public virtual void BeforeReadValues()
    {
    }

    /// <summary>
    /// Вызывается при инициализации просмотра.
    /// </summary>
    public abstract void ReadValues();

    /// <summary>
    /// Вызывается после того, как метод <see cref="ReadValues()"/> был вызыван для всех объектов
    /// в редакторе. Должен отменить действия, выполненные <see cref="BeforeReadValues()"/>.
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    public virtual void AfterReadValues()
    {
    }

    /// <summary>
    /// Вызывается при записи значений.
    /// </summary>
    public abstract void WriteValues();


    /// <summary>
    /// Объект, с помощью которого производный класс может оповещать редактор об изменениях.
    /// </summary>
    protected DepChangeInfoItem ChangeInfo { get { return _ChangeInfo; } }
    private readonly DepChangeInfoItem _ChangeInfo;

    DepChangeInfo IUIExtEditItem.ChangeInfo { get { return _ChangeInfo; } }

    #endregion

    #region Дополнительно

    /// <summary>
    /// Возвращает свойство <see cref="DepChangeInfo.DisplayName"/> из объекта <see cref="ChangeInfo"/>.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _ChangeInfo.DisplayName;
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс, реализующий интерфейс <see cref="IUIExtEditItem"/> и содержащий дочерние элементы.
  /// Если пользовательский код должен реализовывать собственный переходник данных
  /// и не требуется наследование от другого класса, можно наследовать этот класс.
  /// </summary>
  public abstract class UIExtEditItemWithChildren : IUIExtEditItem
  {
    #region Конструктор

    /// <summary>
    /// Создает объект и <see cref="DepChangeInfoItem"/> для свойства <see cref="ChangeInfo"/>.
    /// </summary>
    public UIExtEditItemWithChildren()
    {
      _ChangeInfo = new DepChangeInfoList();

      _Children = new ChildList(this);
    }

    #endregion

    #region Дочерние элементы

    private class ChildList : ICollection<IUIExtEditItem>
    {
      #region Конструктор

      public ChildList(UIExtEditItemWithChildren owner)
      {
        _Owner = owner;
        _Items = new List<IUIExtEditItem>();
      }

      #endregion

      #region Свойства

      private readonly UIExtEditItemWithChildren _Owner;

      public List<IUIExtEditItem> Items { get { return _Items; } }
      private readonly List<IUIExtEditItem> _Items;

      public override string ToString()
      {
        return Items.ToString();
      }

      #endregion

      #region ICollection<IExtEditItem> Members

      public void Add(IUIExtEditItem item)
      {
        if (item == null)
          throw new ArgumentNullException();

        _Items.Add(item);
        _Owner._ChangeInfo.Add(item.ChangeInfo);
      }

      public void Clear()
      {
        IUIExtEditItem[] a = _Items.ToArray();
        for (int i = 0; i < a.Length; i++)
          this.Remove(a[i]);
      }

      public bool Contains(IUIExtEditItem item)
      {
        return _Items.Contains(item);
      }

      public void CopyTo(IUIExtEditItem[] array, int arrayIndex)
      {
        _Items.CopyTo(array, arrayIndex);
      }

      public int Count
      {
        get { return _Items.Count; }
      }

      public bool IsReadOnly { get { return false; } }

      public bool Remove(IUIExtEditItem item)
      {
        if (item == null)
          return false;
        _Owner._ChangeInfo.Remove(item.ChangeInfo);
        return _Items.Remove(item);
      }

      public IEnumerator<IUIExtEditItem> GetEnumerator()
      {
        return _Items.GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return _Items.GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Дочерние управляющие элементы, обращаюшиеся к значениям.
    /// Они должны обязательно подключаться к этому списку а не к редактору непосредственно,
    /// иначе будет неправильный порядок вызовов методов чтения/записи и они не будут сохранятся.
    /// </summary>
    public ICollection<IUIExtEditItem> Children { get { return _Children; } }
    private readonly ChildList _Children;

    #endregion

    #region Отслеживание изменений

    /// <summary>
    /// Список изменений. Отслеживает изменение как в самих данных, так и в дочерних элементах
    /// </summary>
    public DepChangeInfo ChangeInfo { get { return _ChangeInfo; } }

    /// <summary>
    /// Доступ к свойству <see cref="ChangeInfo"/> как к списку
    /// </summary>
    protected DepChangeInfoList ChangeInfoList { get { return _ChangeInfo; } }
    private readonly DepChangeInfoList _ChangeInfo;

    #endregion

    #region IExtEditItem Members

    /// <summary>
    /// Вызывает метод для всех дочерних объектов
    /// </summary>
    public virtual void BeforeReadValues()
    {
      for (int i = 0; i < _Children.Items.Count; i++)
        _Children.Items[i].BeforeReadValues();
    }

    /// <summary>
    /// Чтение значений.
    /// Метод должен быть переопределен. 
    /// Вызывать базовый метод следует после выполнения своих действий, чтобы дочерние элементы
    /// получили уже прочитанные значения
    /// </summary>
    public virtual void ReadValues()
    {
      for (int i = 0; i < _Children.Items.Count; i++)
        _Children.Items[i].ReadValues();
    }

    /// <summary>
    /// Вызывает метод для всех дочерних объектов
    /// </summary>
    public virtual void AfterReadValues()
    {
      for (int i = 0; i < _Children.Items.Count; i++)
        _Children.Items[i].AfterReadValues();
    }

    /// <summary>
    /// Запись значений.
    /// Метод должен быть переопределен. 
    /// Вызывать базовый метод следует перед выполнением своих действий, чтобы дочерние элементы
    /// использовать значения, записанные дочерними элементами.
    /// </summary>
    public virtual void WriteValues()
    {
      for (int i = 0; i < _Children.Items.Count; i++)
        _Children.Items[i].WriteValues();
    }

    #endregion

    #region Дополнительно

    /// <summary>
    /// Возвращает свойство <see cref="DepChangeInfo.DisplayName"/> из объекта <see cref="ChangeInfo"/>.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _ChangeInfo.DisplayName;
    }

    #endregion
  }
}
