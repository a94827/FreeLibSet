// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;

/*
 * Синхронизация редактируемых значений в управляющих элементах формы
 * 
 * Иногда требуется, чтобы одно поле ввода в редакторе влияло на 
 * значение другого поля. Это не проблема, если поля находятся на одной
 * форме, или, хотя бы, управляются из одного места. В этом случае можно 
 * просто выполнить синхронизацию, присваивая одно значение другому, например:
 *     EFPText efp1=new EFPText(...);
 *     EFPText efp2=new EFPText(...);
 *     efp2.Text=efp1.Text;
 *     efp2.Enabled=false;
 * Однако, все сложнее, если поля управляются из разных мест. В этом случае можно
 * сделать управляющее поле (efp1) общедоступным, чтобы процедура, создающая efp2
 * могла его видеть. Еще хуже, когда наличие первого поля зависит от каких-либо
 * условий, в этом случае требуется проверка внутри второй процедуры. Кроме этого,
 * при такой реализации вторая процедура должна знать о реализации efp1. При этом
 * она будет подвержена изменениям, если, например, вместо простого поля ввода для
 * efp1 будет использован комбоблок. Это нарушает принцип модульности программы.
 * 
 * Для решения проблемы используются объекты синхронизации. 
 * 
 * Большинство управляющих элементов реализуют интерфейс IDepSyncObject. Один или
 * несколько объектов IDepSyncObject могут образовывать группу (объект DepSyncGroup)
 * Каждая группа имеет уникальное имя, по которому и происходит объединение.
 * Если в группе больше одного объекта синхронизации, то один из них становится
 * ведущим, а остальным объектам в группе будет* присваиваться такое же значение, 
 * как и у ведущего объекта. При этом они будут заблокированы. Если же в группе 
 * окажется только один элемент, то никакой синхронизации не будет, а управляющий 
 * элемент останется доступным.
 * 
 * Все группы синхронизации формируются в объекте DepSyncProvider, который является
 * ядром для всей реализации синхронизации. Однако, сами объекты синхронизации 
 * добавляются не в него, а в DepSyncCollection. Сами объекты DepSyncCollection
 * могут в произвольный момент времени присоединяться или отсоединяться от 
 * DepSyncProvider, который может содержать произвольное число коллекций.
 * Это необходимо, когда из основной формы, могут открываться дополнительные формы,
 * например, по нажатию командной кнопки. Если дополнительная форма имеет свои
 * объекты синхронизации, которые должны взаимодействовать с объектами основной
 * формы, то может возникнуть переполнение, если дополнительная форма создается
 * многократно. В этом случае, объекты синхронизации дополнительной формы не
 * удалялись бы при закрытии формы, а накапливались бы в общем списке. Решение
 * заключается в том, что при открытии дополнительной формы создается новая 
 * коллекция DepSyncCollection. В нее добавляются элементы, а, затем, она 
 * присоединяется к основному провайдеру. Когда форма закрывается, коллекция
 * отключается от провайдера и De[SyncProvider больше не имеет никаких ссылок на
 * объекты дополнительной формы
 * 
 * Объект DepSyncCollection имеет свойство Provider типа DepSyncProvider. При установке
 * свойства выполняется присоединение объектов синхронизации из коллекции к 
 * провайдеру, а при сбросе в null - отключение
 * 
 * Для удобства, объект EFPFormProdider имеет свойства Syncs типа DepSyncCollection 
 * и SyncProvider типа DepSyncProvider. Оба эти свойства инициализируются в 
 * конструкторе. Значение свойства SyncProvider можкт быть изменено в дополнительной
 * форме для подключение к провайдеру основной формы. Когда форма выводится на 
 * экран, выполняется присвоение Syncs.Provider=SyncProvider, а когда закрывается -
 * Syncs.Provider=null.
 * 
 * Объекты IDepSyncObject передают друг другу значение типа object (произвольного 
 * типа). Для этого у интерфейса IDepSyncObject предусмотрено свойство 'SyncValue'.
 * У группы DepSyncGroup  есть свойтво 'Value'. Когда один  из IDepSyncObject 
 * (ведущий, тот, у которого выставлено свойство 'SyncMaster') устанавливает свойство
 * 'Value' группы, группа DepSyncGroup выполняет установку свойств 'SyncValue' для
 * всех своих объектов. Свойства обработчиков управляющих элементов, производных от
 * EFPControlBase, например, EFPTextBox.Text поддерживают и правильно обрабатывают
 * циклические зависимости, когда изменение значение свойства по цепочке вызывает
 * изменение этого же саиого свойства. Это позволяет создавать двухсторонние связи
 * полей ввода, то есть когда изменение любого из полей изменяет  состояние другого 
 * без опасности рекурсивного зацикливания. Таким образом, в группе может быть 
 * несколько объектов с выставленным SyncMaster (несколько ведущих). Напротив, если 
 * в группе есть  только ведомые объекты, то синхронизация работать не будет.
 * 
 * Если требуется зафиксировать значение какого-либо управляющего элементы и сделать 
 * его недоступным для редактирования, то в управляющей части редактора можно 
 * использовать объект DepConstSync. Его можно также создать с помощью вызова
 * DepSyncCollection.AddConst()
 */

namespace FreeLibSet.DependedValues
{
  #region Интерфейс IDepSyncObject

  /// <summary>
  /// Интерфейс объекта синхронизации.
  /// Реализуется EFPSyncControl
  /// </summary>
  public interface IDepSyncObject
  {
    /// <summary>
    /// Текущее значение синхронизируемого объекта
    /// </summary>
    object SyncValue { get; set;}

    /// <summary>
    /// True, если объект является источником значения (управляющим)
    /// </summary>
    bool SyncMaster { get;}

    /// <summary>
    /// Установка режима "Управляющий" или "Управляемый". По умолчанию объект должен
    /// находится в режиме "Управляющий" (true). При переводе в режим "Управляемый"
    /// управляющий элемент должен блокироваться
    /// </summary>
    /// <param name="value">true - "Управляющий", false - "Управляемый"</param>
    void SyncMasterState(bool value);

    /// <summary>
    /// Группа синхронизации, к которой присоединен объект
    /// </summary>
    DepSyncGroup SyncGroup { get; set;}
  }

  #endregion

  /// <summary>
  /// Группа одноименных объектов синхронизации
  /// </summary>
  public sealed class DepSyncGroup
  {
    #region Защищенный конструктор

    /// <summary>
    /// Группы создаются исключительно внутри метода
    /// DocumentEditorSyncCollection.Add()
    /// </summary>
    internal DepSyncGroup()
    {
      _Objects = new List<IDepSyncObject>();
      _Value = false;
      _ValueDefined = false;
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Текущее значение
    /// </summary>
    public object Value
    {
      get
      {
        return _Value;
      }
      set
      {
        if (_ValueDefined && _Value != null)
        {
          if (_Value.Equals(value))
            return; // значение не изменилось
        }
        _Value = value;
        _ValueDefined = true;
        for (int i = 0; i < _Objects.Count; i++)
          _Objects[i].SyncValue = _Value;
      }
    }
    private object _Value;

    /// <summary>
    /// True, если текущее значение было определено
    /// </summary>
    public bool ValueDefined { get { return _ValueDefined; } }
    private bool _ValueDefined;

    /// <summary>
    /// Число объектов, входящих в группу
    /// </summary>
    public int Count { get { return _Objects.Count; } }

    #endregion

    #region Внутренняя реализация

    internal void Add(IDepSyncObject syncObj)
    {
      if (syncObj.SyncGroup != null)
        throw new InvalidOperationException("Нельзя повторно присоединять объект синхронизации к группе");

      _Objects.Add(syncObj);
      syncObj.SyncGroup = this;
      InitValue();
    }

    internal void Remove(IDepSyncObject syncObj)
    {
      syncObj.SyncMasterState(true);
      syncObj.SyncGroup = null;
      _Objects.Remove(syncObj);

      // После удаления объекта из группы в ней могут остаться только пассивные
      // объекты. В этом случае их надо разблокировать
      if (syncObj.SyncMaster)
      {
        _ValueDefined = false;
        InitValue();
      }
    }

    /// <summary>
    /// Этот метод должен вызываться из кода установки свойства SyncObj.SyncMaster,
    /// если объект уже присоединен к группе
    /// </summary>
    /// <param name="syncObj"></param>
    public void ObjectSyncMasterChanged(IDepSyncObject syncObj)
    {
      _ValueDefined = false;
      InitValue();
    }

    /// <summary>
    /// Начальная установка значений. 
    /// </summary>
    private void InitValue()
    {
      if (!ValueDefined)
      {
        // Перебираем все объекты синхронизации в поисках первого master-
        // объекта.
        for (int i = 0; i < _Objects.Count; i++)
        {
          if (_Objects[i].SyncMaster)
          {
            // нашли
            _Value = _Objects[i].SyncValue;
            _ValueDefined = true;
            break;
          }
        }
        if (!ValueDefined)
        {
          for (int i = 0; i < _Objects.Count; i++)
            _Objects[i].SyncMasterState(true);

          return; // Все объекты оказались пассивными
        }
      }
      // Устанавливаем свойство для всех вновь добавленных объектов
      for (int i = 0; i < _Objects.Count; i++)
      {
        IDepSyncObject obj = _Objects[i];
        obj.SyncValue = Value;
        obj.SyncMasterState(obj.SyncMaster);
      }
    }

    /// <summary>
    /// Список объектов синхронизации в группе
    /// </summary>
    private readonly List<IDepSyncObject> _Objects;

    #endregion
  }

  /// <summary>
  /// Список, в который можно добавить объект синхронизации
  /// </summary>
  public class DepSyncCollection
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public DepSyncCollection()
    {
      _Names = new List<string>();
      _Objects = new List<IDepSyncObject>();
    }

    #endregion

    #region Подключение к провайдеру

    /// <summary>
    /// Присоединение коллекции к провайдеру
    /// </summary>
    public DepSyncProvider Provider
    {
      get { return _Provider; }
      set
      {
        if (value == _Provider)
          return;

        if (_Provider != null)
        {
          _Provider.Collections.Remove(this);
          // Отключаемся от провайдера
          for (int i = 0; i < _Objects.Count; i++)
          {
            DepSyncGroup grp = _Objects[i].SyncGroup; // потом свойство исчезнет
            //if (grp == null)
            //  continue; // ошибка
            grp.Remove(_Objects[i]);
            // Можно было бы удалить и саму группу, если она пустая, но не охота
          }
        }
        _Provider = value;
        if (_Provider != null)
        {
          _Provider.Collections.Add(this);
          // Подключаемся к провайдеру
          for (int i = 0; i < _Objects.Count; i++)
            DoAddToGroup(i);
        }
      }
    }
    private DepSyncProvider _Provider;

    private void DoAddToGroup(int index)
    {
      DepSyncGroup grp;
      if (!_Provider.Groups.TryGetValue(_Names[index], out grp))
      {
        // Добавляем новую группу синхронизации к провайдеру
        grp = new DepSyncGroup();
        _Provider.Groups.Add(_Names[index], grp);
      }
      grp.Add(_Objects[index]);
    }

    #endregion

    #region Список объектов синхронизации

    /// <summary>
    /// Имена объектов синхронизации. Нельзя использовать Dictionary или Hastable,
    /// т.к. в списке пар "Имя-Объект" могут быть одноименные объекты, относящиеся
    /// к одной группе
    /// </summary>
    private List<string> _Names;
    /// <summary>
    /// Список имен объектов синхронизации (для отладки)
    /// </summary>
    public string[] Names { get { return _Names.ToArray(); } }

    private List<IDepSyncObject> _Objects;

    /// <summary>
    /// Список объектов синхронизации (для отладки)
    /// </summary>
    public IDepSyncObject[] Objects { get { return _Objects.ToArray(); } }

    /// <summary>
    /// Возвращает число объектов в списка
    /// </summary>
    public int Count { get { return _Names.Count; } }

    /// <summary>
    /// Добавление объекта синхронизации
    /// </summary>
    /// <param name="name"></param>
    /// <param name="syncObj"></param>
    public void Add(string name, IDepSyncObject syncObj)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
      if (syncObj == null)
        throw new ArgumentNullException("syncObj");
      _Names.Add(name);
      _Objects.Add(syncObj);
      if (_Provider != null)
        DoAddToGroup(_Names.Count - 1);
    }

    /// <summary>
    /// Добавить константное значение в качестве управляющего объекта синхронизации
    /// (с SyncMaster=true)
    /// </summary>
    /// <typeparam name="T">Тип константного значения</typeparam>
    /// <param name="name">Имя группы синхронизации</param>
    /// <param name="value">Значение</param>
    public void AddConst<T>(string name, T value)
    {
      DepSync<T> ConstObj = new DepSync<T>();
      ConstObj.Value = value;
      ConstObj.SyncMaster = true;
      Add(name, ConstObj);
    }

    /// <summary>
    /// Добавить фиксированное значение, управляемое внешним значением DepValueEx
    /// </summary>
    /// <typeparam name="T">Тип значения</typeparam>
    /// <param name="name">Имя группы</param>
    /// <param name="valueEx">Управляющее значение</param>
    public void AddConst<T>(string name, DepValue<T> valueEx)
    {
      if (valueEx == null)
        throw new ArgumentNullException("valueEx");
      DepSyncValue<T> SyncObj = new DepSyncValue<T>();
      SyncObj.ValueEx = valueEx;
      SyncObj.SyncMaster = true;
      Add(name, SyncObj);
    }


    /// <summary>
    /// Удаляет объект из списка
    /// </summary>
    /// <param name="syncObj">Удаляемый объект</param>
    public void Remove(IDepSyncObject syncObj)
    {
      if (syncObj == null)
        return;
      int p = _Objects.IndexOf(syncObj);
      if (p < 0)
        return;

      _Names.RemoveAt(p);
      _Objects.RemoveAt(p);

      if (_Provider != null)
        syncObj.SyncGroup.Remove(syncObj);
    }

    /// <summary>
    /// Получить текущее значение. Значение возвращается, если существует группа
    /// синхронизации с указанным именем и в ней есть элемент с SyncMaster=true
    /// Иначе возвращается null
    /// </summary>
    /// <param name="name">Имя группы синхронизации</param>
    /// <returns>Текущее значение или null</returns>
    public object GetValue(string name)
    {
      if (String.IsNullOrEmpty(name))
        return null;
      int p = _Names.IndexOf(name);
      if (p < 0)
        return null;
      IDepSyncObject SyncObj = _Objects[p];
      return SyncObj.SyncValue;
    }

    /// <summary>
    /// Текстовое представление "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + _Names.Count.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Провайдер для нескольких коллекций DepSyncCollection
  /// </summary>
  public class DepSyncProvider
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    public DepSyncProvider()
    {
      _Collections = new List<DepSyncCollection>();
      _Groups = new Dictionary<string, DepSyncGroup>();
    }

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Коллекции, подключенные к провайдеру
    /// </summary>
    internal List<DepSyncCollection> Collections { get { return _Collections; } }
    private readonly List<DepSyncCollection> _Collections;

    /// <summary>
    /// Активные группы синхронизации
    /// </summary>
    internal Dictionary<string, DepSyncGroup> Groups { get { return _Groups; } }
    private readonly Dictionary<string, DepSyncGroup> _Groups;

    #endregion
  }


  // ******************************************************************************
  // Реализации интерфейса IDepSyncObject

  /// <summary>
  /// Простейшая реализация интерфейса IDepSyncObject
  /// </summary>
  public class DepSyncBase : IDepSyncObject
  {
    #region Конструктор

    /// <summary>
    /// Устанавливает DepSyncBase=false
    /// </summary>
    public DepSyncBase()
    {
      _SyncMaster = false;
    }

    #endregion

    #region IDepSyncObject Members

    /// <summary>
    /// Синронизированное значение
    /// </summary>
    public object SyncValue
    {
      get { return _SyncValue; }
      set
      {
        _SyncValue = value;
        if (SyncValueChanged != null)
          SyncValueChanged(this, EventArgs.Empty);
      }
    }
    private object _SyncValue;

    /// <summary>
    /// true, если текущий объект является ведушим
    /// </summary>
    public bool SyncMaster
    {
      get { return _SyncMaster; }
      set
      {
        if (value == _SyncMaster)
          return;
        _SyncMaster = value;
        if (SyncGroup != null)
          SyncGroup.ObjectSyncMasterChanged(this);
      }
    }
    private bool _SyncMaster;

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="value"></param>
    public void SyncMasterState(bool value)
    {
      // Этот метод ничего не делает
    }

    /// <summary>
    /// Устанавливаемон свойство
    /// </summary>
    public DepSyncGroup SyncGroup
    {
      get { return _SyncGroup; }
      set { _SyncGroup = value; }
    }
    private DepSyncGroup _SyncGroup;

    #endregion

    #region Событие

    /// <summary>
    /// Вызывается при установке значения SyncValue.
    /// Внутри этого события нельзя изменять значение свойства SyncValue.
    /// Событие может быть использовано пассивным объектом для выполнения действий
    /// </summary>
    public event EventHandler SyncValueChanged;

    #endregion
  }

  /// <summary>
  /// Простейшая реализация интерфейса IDepSyncObject.
  /// Расширяет базовый класс свойством Value, которое приводит SyncValue к
  /// требуемому типу
  /// </summary>
  /// <typeparam name="T">Тип синхронизируемого значения</typeparam>
  public class DepSync<T> : DepSyncBase
  {
    /// <summary>
    /// Значение, приведенное к нужному типу.
    /// </summary>
    public T Value
    {
      get { return (T)SyncValue; }
      set { SyncValue = value; }
    }
  }

  /// <summary>
  /// Реализация объекта синхронизации, который использует объект типа DepValue
  /// для управления значением.
  /// Может иметь SyncMaster как равный true, так и false
  /// </summary>
  /// <typeparam name="T">Тип значения</typeparam>
  public class DepSyncValue<T> : IDepSyncObject
  {
    #region Конструктор

    /// <summary>
    /// Создает объект с SyncMaster=false
    /// </summary>
    public DepSyncValue()
    {
      _SyncMaster = false;
      _ValueEx = new DepInput<T>(Value, ValueEx_ValueChanged);
      _ValueEx.OwnerInfo = new DepOwnerInfo(this, "ValueEx");
    }

    void ValueEx_ValueChanged(object sender, EventArgs args)
    {
      if (SyncGroup == null)
        return;
      SyncGroup.Value = ValueEx.Value;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Значение SyncValue, приведенное к типу T.
    /// </summary>
    public T Value
    {
      get { return (T)SyncValue; }
      set { SyncValue = value; } // исправлено 06.12.2018
    }

    /// <summary>
    /// Управляемое свойство SyncValue
    /// </summary>
    public DepValue<T> ValueEx
    {
      get { return _ValueEx; }
      set { _ValueEx.Source = value; }
    }
    private DepInput<T> _ValueEx;

    #endregion

    #region IDepSyncObject Members

    /// <summary>
    /// Синхронизированное значение
    /// </summary>
    public object SyncValue
    {
      get { return ValueEx.Value; }
      set { _ValueEx.Value = (T)value; }
    }

    /// <summary>
    /// true, если объект является ведущим
    /// </summary>
    public bool SyncMaster
    {
      get { return _SyncMaster; }
      set
      {
        if (value == _SyncMaster)
          return;
        _SyncMaster = value;
        if (SyncGroup != null)
          SyncGroup.ObjectSyncMasterChanged(this);
      }
    }
    private bool _SyncMaster;

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="value"></param>
    public void SyncMasterState(bool value)
    {
      // Этот метод ничего не делает
    }

    /// <summary>
    /// Группа синхронизации
    /// </summary>
    public DepSyncGroup SyncGroup
    {
      get { return _SyncGroup; }
      set { _SyncGroup = value; }
    }
    private DepSyncGroup _SyncGroup;

    #endregion
  }
}
