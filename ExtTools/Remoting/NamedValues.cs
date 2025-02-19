﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

#define USE_CUSTOMSERIALIZATION // Если определено, используем специальный формат сериализации для NamedValues, чтобы уменьшить размер объектов
// 17.04.2020 Так гораздо короче
// Например, уменьшение пакетов AliveSignal в АССОО-2: 
// отправка запроса: с 2128 байт до 650
// получение ответа: с 2614 байт до 1120

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Collections;
using System.Threading;
using FreeLibSet.Core;

namespace FreeLibSet.Remoting
{
  /// <summary>
  /// Коллекция значений с доступом по имени.
  /// Имена чувствительны к регистру
  /// </summary>
  [Serializable]
  public sealed class NamedValues : IDictionary<string, object>, ICloneable, IReadOnlyObject, INamedValuesAccess
#if USE_CUSTOMSERIALIZATION
, ISerializable
#endif
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустую коллекцию
    /// </summary>
    public NamedValues()
    {
      _Items = new Dictionary<string, object>();
    }

    private NamedValues(int capacity, bool isReadOnly)
    {
      _Items = new Dictionary<string, object>(capacity /* 27.12.2020 */);
      if (isReadOnly)
        SetReadOnly();
    }

    // Конструктор сериализации внизу

    #endregion

    #region Свойства

    /// <summary>
    /// Доступ к значению по имени.
    /// При чтении, если в коллекции нет объекта с таким именем, возвращается null.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Объект</returns>
    public object this[string name]
    {
      get
      {
        object res;
        if (_Items.TryGetValue(name, out res))
          return res;
        else
          return null;
      }
      set
      {
        CheckNotReadOnly();
        if (String.IsNullOrEmpty(name))
          throw ExceptionFactory.ArgStringIsNullOrEmpty("name");

        if (_Items.ContainsKey(name))
          _Items[name] = value;
        else
          _Items.Add(name, value);
      }
    }

    [NonSerialized]
    private Dictionary<string, object> _Items;

    /// <summary>
    /// Возвращает true, если коллекция пуста
    /// </summary>
    public bool IsEmpty { get { return _Items.Count == 0; } }

    #endregion

    #region Форматированный доступ

    /// <summary>
    /// Возвращает значениеи как строку
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public string GetString(string name)
    {
      return DataTools.GetString(this[name]);
    }

    /// <summary>
    /// Возвращает значение как число
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public int GetInt(string name)
    {
      return DataTools.GetInt(this[name]);
    }

    /// <summary>
    /// Возвращает значение как число
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public long GetInt64(string name)
    {
      return DataTools.GetInt64(this[name]);
    }

    /// <summary>
    /// Возвращает логическое значение
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public bool GetBool(string name)
    {
      return DataTools.GetBool(this[name]);
    }

    /// <summary>
    /// Возвращает значение как число
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public float GetSingle(string name)
    {
      return DataTools.GetSingle(this[name]);
    }

    /// <summary>
    /// Возвращает значение как число
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public double GetDouble(string name)
    {
      return DataTools.GetDouble(this[name]);
    }

    /// <summary>
    /// Возвращает значение как число
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public decimal GetDecimal(string name)
    {
      return DataTools.GetDecimal(this[name]);
    }

    /// <summary>
    /// Возвращает значение как дату и время или null.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public DateTime? GetNullableDateTime(string name)
    {
      return DataTools.GetNullableDateTime(this[name]);
    }

    /// <summary>
    /// Возвращает значение как дату и время.
    /// Пустое значение преобразуется в DateTime.MinValue, для непустой строки выполняется попытка преобразования с помощью Convert.ToDateTime().
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public DateTime GetDateTime(string name)
    {
      return DataTools.GetDateTime(this[name]);
    }

    /// <summary>
    /// Возвращает значение как интервал времени
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public TimeSpan GetTimeSpan(string name)
    {
      return DataTools.GetTimeSpan(this[name]);
    }

    /// <summary>
    /// Возвращает значение типа Guid.
    /// Возвращает Guid.Empty, если в коллекции нет элемента с заданным имененм
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public Guid GetGuid(string name)
    {
      return DataTools.GetGuid(this[name]);
    }

    /// <summary>
    /// Возвращает значение перечислимого типа.
    /// Возвращает нулевое значение перечисления (default), если в коллекции нет элемента с заданным имененм
    /// </summary>
    /// <param name="name">Имя</param>
    /// <returns>Значение</returns>
    public T GetEnum<T>(string name)
      where T:struct
    {
      return DataTools.GetEnum<T>(this[name]);
    }

    /// <summary>
    /// Если в коллекции нет объекта с заданным именем <paramref name="name"/>, то он создается. Иначе возвращается существующий объект.
    /// </summary>
    /// <typeparam name="T">Тип данных. Должен быть классом, поддерживающим конструктор без параметров</typeparam>
    /// <param name="name">Имя</param>
    /// <returns>Созданный или существующий объект</returns>
    public T GetOrCreate<T>(string name)
      where T:class, new()
    {
      CheckNotReadOnly();
      object res;
      if (_Items.TryGetValue(name, out res))
        return (T)res;

      if (String.IsNullOrEmpty(name))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("name");

      res = new T();
      _Items.Add(name, res);
      return (T)res;
    }

    /// <summary>
    /// Если в коллекции нет объекта с заданным именем <paramref name="name"/>, то он создается. Иначе возвращается существующий объект.
    /// </summary>
    /// <param name="objType">Тип создаваемого объекта. Должен быть классом, поддерживающим конструктор без параметров</param>
    /// <param name="name">Имя</param>
    /// <returns>Созданный или существующий объект</returns>
    public object GetOrCreate(Type objType, string name)
    {
      CheckNotReadOnly();
      object res;
      if (_Items.TryGetValue(name, out res))
        return res;

      if (String.IsNullOrEmpty(name))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("name");

#if DEBUG
      if (objType == null)
        throw new ArgumentNullException("objType");
#endif
      if (!objType.IsClass)
        throw new ArgumentException(String.Format(Res.NamedValues_Arg_MustBeClass, objType), "objType");

      System.Reflection.ConstructorInfo ci = objType.GetConstructor(Type.EmptyTypes);
      if (ci == null)
        throw new ArgumentException(String.Format(Res.NamedValues_Arg_ConstructorNeeded, objType.ToString()));

      res = ci.Invoke(DataTools.EmptyObjects);
      _Items.Add(name, res);
      return res;
    }

    #endregion

    #region Инкремент значений

    /// <summary>
    /// Инкремент целочисленного значения.
    /// Если на момент вызова в коллекции нет значения с таким именем, то оно считается равным 0.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="delta">На сколько нужно увеличить значение</param>
    public void IncInt(string name, int delta)
    {
      this[name] = GetInt(name) + delta;
    }

    /// <summary>
    /// Инкремент целочисленного значения.
    /// Если на момент вызова в коллекции нет значения с таким именем, то оно считается равным 0.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="delta">На сколько нужно увеличить значение</param>
    public void IncInt64(string name, long delta)
    {
      this[name] = GetInt64(name) + delta;
    }

    /// <summary>
    /// Инкремент значения с плавающей точкой.
    /// Если на момент вызова в коллекции нет значения с таким именем, то оно считается равным 0.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="delta">На сколько нужно увеличить значение</param>
    public void IncSingle(string name, float delta)
    {
      this[name] = GetSingle(name) + delta;
    }

    /// <summary>
    /// Инкремент значения с плавающей точкой.
    /// Если на момент вызова в коллекции нет значения с таким именем, то оно считается равным 0.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="delta">На сколько нужно увеличить значение</param>
    public void IncDouble(string name, double delta)
    {
      this[name] = GetDouble(name) + delta;
    }

    /// <summary>
    /// Инкремент значения с плавающей точкой.
    /// Если на момент вызова в коллекции нет значения с таким именем, то оно считается равным 0.
    /// </summary>
    /// <param name="name">Имя</param>
    /// <param name="delta">На сколько нужно увеличить значение</param>
    public void IncDecimal(string name, decimal delta)
    {
      this[name] = GetDecimal(name) + delta;
    }

    #endregion

    #region IsReadOnly

    /// <summary>
    /// Возвращает признак "Только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    [NonSerialized]
    private bool _IsReadOnly;

    /// <summary>
    /// Переводит коллекцию в режим "Только чтение". Повторные вызовы метода игнорируются.
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    /// <summary>
    /// Выбрасывает исключение, если IsReadOnly=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region Другие методы

    /// <summary>
    /// Очистка коллекции
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();
      _Items.Clear();
    }

    /// <summary>
    /// Возвращает true, если все значения могут быть переданы как marshal-by-reference или сериализованы.
    /// Значения null также допускаются
    /// </summary>
    /// <returns></returns>
    public bool IsMarshallable()
    {
      foreach (KeyValuePair<string, object> pair in _Items)
      {
        if (!SerializationTools.IsMarshallable(pair.Value))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Проверяет возможность сериализации для каждого значения в списке.
    /// Если сериализация невозможна, выбрасывается исключение
    /// </summary>
    public void CheckIfMarshallable()
    {
      foreach (KeyValuePair<string, object> pair in _Items)
      {
        if (!SerializationTools.IsMarshallable(pair.Value))
          throw new SerializationException(String.Format(Res.NamedValues_Err_NotSerializable, pair.Key, pair.Value.GetType().ToString()));
      }
    }

    /// <summary>
    /// Возвращает true, если в списке есть DataSet или DataTable с сериализацией XML.
    /// Метод предназначен для отладочных целей
    /// </summary>
    /// <returns>true, если запись обнаружена</returns>
    public bool ContainsXmlSerialized()
    {
      foreach (KeyValuePair<string, object> pair in _Items)
      {
        if (pair.Value is System.Data.DataSet)
        {
          if (((System.Data.DataSet)(pair.Value)).RemotingFormat == System.Data.SerializationFormat.Xml)
            return true;
        }
        if (pair.Value is System.Data.DataTable)
        {
          if (((System.Data.DataTable)(pair.Value)).RemotingFormat == System.Data.SerializationFormat.Xml)
            return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Текстовое представление для отладочных целей
    /// </summary>
    /// <returns>Список имен, разделенных запятыми</returns>
    public override string ToString()
    {
      string s;
      if (IsEmpty)
        s = "[Empty]";
      else
      {
        string[] a = new string[Count];
        Keys.CopyTo(a, 0);
        s = String.Join(", ", a);
      }
      if (IsReadOnly)
        s += " (Read only)";

      return s;
    }

    /// <summary>
    /// Копирует элементы из текущего набора в другой набор.
    /// </summary>
    /// <param name="dest">Заполняемый набор</param>
    /// <param name="names">Список имен для копирования, разделенный запятыми. Если строка пустая, никаких действий не выполняется.</param>
    public void CopyTo(NamedValues dest, string names)
    {
      if (dest == null)
        throw new ArgumentNullException("dest");
      dest.CheckNotReadOnly();
      if (String.IsNullOrEmpty(names))
        return;

      string[] aNames = names.Split(',');
      for (int i = 0; i < aNames.Length; i++)
        dest[aNames[i]] = this[aNames[i]];
    }

    /// <summary>
    /// Добавляет все элементы из коллекции <paramref name="source"/>.
    /// Если в текущем объекте есть элементы с совпадающими именами, то значения заменяются на переданные.
    /// </summary>
    /// <param name="source">Исходный набор. Не может быть null</param>
    public void Add(IDictionary<string, object> source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      CheckNotReadOnly();
      foreach (KeyValuePair<string, object> pair in source)
        this[pair.Key] = pair.Value;
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создание копии коллекции.
    /// У полученной коллекции свойство IsReadOnly не установлено
    /// </summary>
    /// <returns></returns>
    public NamedValues Clone()
    {
      NamedValues newObj = new NamedValues(_Items.Count, false);
      foreach (KeyValuePair<string, object> pair in _Items)
        newObj._Items.Add(pair.Key, pair.Value);
      return newObj;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region Статический экземпляр

    /// <summary>
    /// Пустая коллекция с установленным признаком IsReadOnly
    /// </summary>
    public static readonly NamedValues Empty = new NamedValues(0, true);

    #endregion

    #region IDictionary<string,object> Members

    /// <summary>
    /// Добавляет объект в коллекцию.
    /// </summary>
    /// <param name="key">Имя</param>
    /// <param name="value">Значение</param>
    public void Add(string key, object value)
    {
      CheckNotReadOnly();
      _Items.Add(key, value);
    }

    /// <summary>
    /// Возвращает true, если в коллекции есть объект с указаным именем
    /// </summary>
    /// <param name="key">Имя</param>
    /// <returns>Наличие значения</returns>
    public bool ContainsKey(string key)
    {
      return _Items.ContainsKey(key);
    }

    /// <summary>
    /// Возвращает коллекцию имен
    /// </summary>
    public ICollection<string> Keys
    {
      get { return _Items.Keys; }
    }

    /// <summary>
    /// Удаляет объект из коллекции
    /// </summary>
    /// <param name="key">Имя</param>
    /// <returns>true, если объект быд в коллекции</returns>
    public bool Remove(string key)
    {
      CheckNotReadOnly();
      return _Items.Remove(key);
    }

    /// <summary>
    /// Попытка получить объект из коллекции
    /// </summary>
    /// <param name="key">Имя</param>
    /// <param name="value">Сюда помещается значение, если объект есть в коллекции</param>
    /// <returns>true, если объект есть в коллекции</returns>
    public bool TryGetValue(string key, out object value)
    {
      return _Items.TryGetValue(key, out value);
    }

    /// <summary>
    /// Бесполезная коллекция объектов без имен.
    /// </summary>
    public ICollection<object> Values
    {
      get { return _Items.Values; }
    }

    #endregion

    #region ICollection<KeyValuePair<string,object>> Members

    void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
    {
      CheckNotReadOnly();
      ((ICollection<KeyValuePair<string, object>>)_Items).Add(item);
    }

    bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
    {
      return ((ICollection<KeyValuePair<string, object>>)_Items).Contains(item);
    }

    void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
      ((ICollection<KeyValuePair<string, object>>)_Items).CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает количество объектов в коллекции
    /// </summary>
    public int Count
    {
      get { return _Items.Count; }
    }

    bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
    {
      CheckNotReadOnly();
      return ((ICollection<KeyValuePair<string, object>>)_Items).Remove(item);
    }

    #endregion

    #region IEnumerable<KeyValuePair<string,object>> Members

    /// <summary>
    /// Возвращает перечислитель по KeyValuePair.
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Dictionary<string, object>.Enumerator GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    bool INamedValuesAccess.Contains(string name)
    {
      return ContainsKey(name);
    }

    /// <summary>
    /// Получить список всех имен, которые есть в коллекции
    /// </summary>
    /// <returns>Массив имен</returns>
    public string[] GetNames()
    {
      string[] a = new string[Count];
      Keys.CopyTo(a, 0);
      return a;
    }

    #endregion

    #region ISerializable Members

    // 03.07.2019
    // Нельзя просто так сериализовать значение AddValue("Values", aValues)
    // Сериализация почему-то не заработала.
    // В АССОО-2 все работает правильно до возврата ответа для запроса GetStartInfo
    // "Keys" возвращает 6 ключей, как положено.
    // "Values" возвращает 6 объектов, но из них заполнено только "ServerTime", а остальные значения равны null.

#if USE_CUSTOMSERIALIZATION

    // См сериализацию класса Dictionary в исходных текстах .Net Framework


    //private const string SNKeys = "Keys";
    //private const string SNValues = "Values";
    //private const string SNIsReadOnly = "RO";

    [Serializable]
    private class SN
    {
      #region Поля

      public string Keys;
      public object[] Values;
      public bool RO;

      #endregion
    }

    /// <summary>
    /// Промежуточное хранилище при сериализации.
    /// В классе Dictionary используется внешняя коллекция HashHelpers.SerializationInfoTable
    /// Пока использую поле прямо в объекте
    /// </summary>
    [NonSerialized]
    private SerializationInfo _StoredSerInfo;

    /// <summary>
    /// Сериализация
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new ArgumentNullException("info");

      SN sn = new SN();
      if (Count > 0)
      {
        string[] aKeys = new string[Count];
        _Items.Keys.CopyTo(aKeys, 0);
        sn.Keys = String.Join("|", aKeys);
        sn.Values = new object[Count];
        _Items.Values.CopyTo(sn.Values, 0);
      }
      sn.RO = _IsReadOnly;

      info.AddValue("SN", sn);
    }

    /// <summary>
    /// Десериализация
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    private NamedValues(SerializationInfo info, StreamingContext context)
    {
      //We can't do anything with the keys and values until the entire graph has been deserialized
      //and we have a resonable estimate that GetHashCode is not going to fail.  For the time being,
      //we'll just cache this.  The graph is not valid until OnDeserialization has been called.
      _StoredSerInfo = info;
    }

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      if (_StoredSerInfo == null)
      {
        // It might be necessary to call OnDeserialization from a container if the container object also implements
        // OnDeserialization. However, remoting will call OnDeserialization again.
        // We can return immediately if this function is called twice. 
        // Note we set remove the serialization info from the table at the end of this method.
        return;
      }

      SN sn = (SN)(_StoredSerInfo.GetValue("SN", typeof(SN)));
      if (sn == null)
        throw new NullReferenceException("Cannot extract SN");

      if (sn.Keys != null)
      {
        string[] aKeys = sn.Keys.Split('|');
        if (sn.Values.Length != aKeys.Length)
          throw new InvalidOperationException("Key list length (" + aKeys.Length.ToString() + ") and value count (" + sn.Values.Length.ToString() + ") are different");
        _Items = new Dictionary<string, object>(aKeys.Length);
        for (int i = 0; i < aKeys.Length; i++)
          _Items.Add(aKeys[i], sn.Values[i]);
      }
      else
        _Items = new Dictionary<string, object>();
      _IsReadOnly = sn.RO;

      _StoredSerInfo = null;
    }

#endif
    #endregion
  }


#if USE_CUSTOMSERIALIZATION

#if XXX
  internal static class SerializationTools
  {
    // Взято из исходных текстов Net Framework, внутренний класс HashHelpers

    // Used by Hashtable and Dictionary's SeralizationInfo .ctor's to store the SeralizationInfo
    // object until OnDeserialization is called.
    private static ConditionalWeakTable<object, SerializationInfo> s_SerializationInfoTable;

    internal static ConditionalWeakTable<object, SerializationInfo> SerializationInfoTable
    {
      get
      {
        if (s_SerializationInfoTable == null)
        {
          ConditionalWeakTable<object, SerializationInfo> newTable = new ConditionalWeakTable<object, SerializationInfo>();
          Interlocked.CompareExchange(ref s_SerializationInfoTable, newTable, null);
        }

        return s_SerializationInfoTable;
      }

    }
  }
#endif
#endif
}
