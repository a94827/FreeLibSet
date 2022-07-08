// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.DependedValues;

namespace FreeLibSet.Forms.Docs
{
  #region Интерфейс IDocEditItem

  /// <summary>
  /// Интерфейс одного элемента, выполняющего редактирование поля или полей
  /// в редакторе документа
  /// </summary>
  public interface IDocEditItem
  {
    /// <summary>
    /// Вызывается перед вызовами ReadValues() для всех элементов. Метод может 
    /// временно отключать инициализацию зависимых полей
    /// </summary>
    void BeforeReadValues();

    /// <summary>
    /// Вызывается при инициализации просмотра
    /// </summary>
    void ReadValues();

    /// <summary>
    /// Вызывается после того, как метод ReadValues() был вызыван для всех объектов
    /// в группе. Должен отменить действия, выполненные BeforeReadValues()
    /// </summary>
    void AfterReadValues();

    /// <summary>
    /// Вызывается при записи значений
    /// </summary>
    void WriteValues();

    /// <summary>
    /// Объект, с помощью которого DocEditItem может оповещать редактор об изменениях
    /// </summary>
    DepChangeInfo ChangeInfo { get; }
  }

  #endregion

  /// <summary>
  /// Список объектов IDocEditItem с дополнительными методами чтения и записи значений
  /// </summary>
  public class DocEditItemList : List<IDocEditItem>
  {
    #region Чтение и запись значений

    /// <summary>
    /// Вызов методов IDocEditItem.BeforeReadValues(), ReadValues() и AfterReadValues() с перехватом ошибок
    /// </summary>
    public void ReadValues()
    {
      foreach (IDocEditItem item in this)
        item.BeforeReadValues();

      foreach (IDocEditItem item in this)
      {
        try
        {
          item.ReadValues();
        }
        catch (Exception e)
        {
          string displayName;
          if (item.ChangeInfo == null)
            displayName = item.ToString();
          else
            displayName = item.ChangeInfo.DisplayName;
          EFPApp.ShowException(e, "Ошибка при считывании значения \"" + displayName + "\"");
        }
      }
      foreach (IDocEditItem item in this)
      {
        item.AfterReadValues();
        //Item.ChangeInfo = new DepChangeInfoItem(FChangeInfo);
        //Item.ChangeInfo.DisplayName = Item.DisplayName;
      }
    }

    /// <summary>
    /// Вызов метода WriteValues() для всех объектов в списке
    /// </summary>
    public void WriteValues()
    {
      foreach (IDocEditItem item in this)
        item.WriteValues();
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс, реализующий интерфейс IDocEditItem
  /// Если пользовательский код должен реализовывать собственный переходник данных
  /// и не требуется наследование от другого класса, можно наследовать этот класс
  /// </summary>
  public abstract class DocEditItem : IDocEditItem
  {
    #region Конструктор

    /// <summary>
    /// Создает объект и DepChangeInfoItem
    /// </summary>
    public DocEditItem()
    {
      _ChangeInfo = new DepChangeInfoItem();
    }

    #endregion

    #region IDocEditItem Members

    /// <summary>
    /// Вызывается перед вызовами ReadValues() для всех элементов. Метод может 
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
    /// Вызывается после того, как метод ReadValues() был вызыван для всех объектов
    /// в группе. Должен отменить действия, выполненные BeforeReadValues()
    /// Непереопределенный метод ничего не делает.
    /// </summary>
    public virtual void AfterReadValues()
    {
    }

    /// <summary>
    /// Вызывается при записи значений
    /// </summary>
    public abstract void WriteValues();


    /// <summary>
    /// Объект, с помощью которого производный класс может оповещать редактор об изменениях.
    /// </summary>
    protected DepChangeInfoItem ChangeInfo { get { return _ChangeInfo; } }
    private DepChangeInfoItem _ChangeInfo;

    DepChangeInfo IDocEditItem.ChangeInfo { get { return _ChangeInfo; } }

    #endregion

    #region Дополнительно

    /// <summary>
    /// Возвращает DisplayName
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _ChangeInfo.DisplayName;
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс, реализующий интерфейс IDocEditItem и содержащий дочерние элементы
  /// Если пользовательский код должен реализовывать собственный переходник данных
  /// и не требуется наследование от другого класса, можно наследовать этот класс
  /// </summary>
  public abstract class DocEditItemWithChildren : IDocEditItem
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    public DocEditItemWithChildren()
    {
      _ChangeInfo = new DepChangeInfoList();

      _Children = new ChildList(this);
    }

    #endregion

    #region Дочерние элементы

    private class ChildList : ICollection<IDocEditItem>
    {
      #region Конструктор

      public ChildList(DocEditItemWithChildren owner)
      {
        _Owner = owner;
        _Items = new List<IDocEditItem>();
      }

      #endregion

      #region Свойства

      private DocEditItemWithChildren _Owner;

      public List<IDocEditItem> Items { get { return _Items; } }
      private List<IDocEditItem> _Items;

      public override string ToString()
      {
        return Items.ToString();
      }

      #endregion

      #region ICollection<IDocEditItem> Members

      public void Add(IDocEditItem item)
      {
        if (item == null)
          throw new ArgumentNullException();

        _Items.Add(item);
        _Owner._ChangeInfo.Add(item.ChangeInfo);
      }

      public void Clear()
      {
        IDocEditItem[] a = _Items.ToArray();
        for (int i = 0; i < a.Length; i++)
          this.Remove(a[i]);
      }

      public bool Contains(IDocEditItem item)
      {
        return _Items.Contains(item);
      }

      public void CopyTo(IDocEditItem[] array, int arrayIndex)
      {
        _Items.CopyTo(array, arrayIndex);
      }

      public int Count
      {
        get { return _Items.Count; }
      }

      public bool IsReadOnly { get { return false; } }

      public bool Remove(IDocEditItem item)
      {
        if (item == null)
          return false;
        _Owner._ChangeInfo.Remove(item.ChangeInfo);
        return _Items.Remove(item);
      }

      public IEnumerator<IDocEditItem> GetEnumerator()
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
    /// иначе будет неправильный порядок вызовов методов чтения/записи и они не будут сохранятся
    /// </summary>
    public ICollection<IDocEditItem> Children { get { return _Children; } }
    private ChildList _Children;

    #endregion

    #region Отслеживание изменений

    /// <summary>
    /// Список изменений. Отслеживает изменение как в самих данных, так и в дочерних элементах
    /// </summary>
    public DepChangeInfo ChangeInfo { get { return _ChangeInfo; } }

    /// <summary>
    /// Доступ к ChangeInfo как к списку
    /// </summary>
    protected DepChangeInfoList ChangeInfoList { get { return _ChangeInfo; } }
    private DepChangeInfoList _ChangeInfo;

    #endregion

    #region IDocEditItem Members

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
    /// использовать значения, записанные дочерними элементами
    /// </summary>
    public virtual void WriteValues()
    {
      for (int i = 0; i < _Children.Items.Count; i++)
        _Children.Items[i].WriteValues();
    }
                                                         
    #endregion

    #region Дополнительно

    /// <summary>
    /// Возвращает DisplayName
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _ChangeInfo.DisplayName;
    }

    #endregion
  }
}
