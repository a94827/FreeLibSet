// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Collections;
using FreeLibSet.Config;
using FreeLibSet.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Тип набора идентификаторов (массив, коллекция, список).
  /// Возвращается свойством <see cref="IIdSet.Kind"/>.
  /// </summary>
  public enum IdSetKind
  {
    /// <summary>
    /// Фиксированный массив <see cref="IdArray{T}"/> без возможности добавления / удаления идентификаторов.
    /// Набор идентификаторов реализует интерфейс <see cref="IIndexedIdSet{T}"/>.
    /// </summary>
    Array,

    /// <summary>
    /// Коллекция идентификаторов <see cref="IdCollection{T}"/>, для которой порядок следования элементов не имеет значения.
    /// </summary>
    Collection,

    /// <summary>
    /// Список идентификаторов <see cref="IdList{T}"/>, в котором порядок следования идентификаторов сохраняется при добавлении / удалении элементов.
    /// Набор идентификаторов реализует интерфейс <see cref="IIndexedIdSet{T}"/>.
    /// </summary>
    List
  }

  /// <summary>
  /// Нетипизированный набор идентификаторов.
  /// Интерфейс используется в тех случаях, когда тип идентификаторов не известен в данном контексте.
  /// В остальных случаях применятся типизированый интерфейс <see cref="IIdSet{T}"/>.
  /// Интерфейс реализуется <see cref="IdArray{T}"/>, <see cref="IdCollection{T}"/> и <see cref="IdList{T}"/>.
  /// </summary>
  public interface IIdSet/* : System.Collections.ICollection, ICloneableReadOnlyObject<IIdSet>*/
  {
    #region Свойства

    /// <summary>
    /// Возвращает тип идентификаторов.
    /// Тип является структурой и должен реализовывать интерфейс <see cref="IEquatable{T}"/>.
    /// Может быть составным типом <see cref="IComplexId"/>.
    /// </summary>
    Type IdType { get; }

    /// <summary>
    /// Возвращает тип набора (массив, коллекция или список)
    /// </summary>
    IdSetKind Kind { get; }

    #endregion

    #region Методы

    /// <summary>
    /// Переводит набор в режим "Только чтение".
    /// Не выполняет никаких действий, если <see cref="IReadOnlyObject.IsReadOnly"/> уже возващает true, или
    /// набор является <see cref="IdArray{T}"/>.
    /// </summary>
    void SetReadOnly();

    #endregion
  }

  /// <summary>
  /// Список идентфикаторов заданного типа.
  /// Реализуется классами <see cref="IdArray{T}"/>, <see cref="IdCollection{T}"/> и <see cref="IdList{T}"/>.
  /// </summary>
  /// <typeparam name="T">Тип идентификаторов:
  /// <see cref="System.Int32"/>, 
  /// <see cref="System.Int64"/>, 
  /// <see cref="System.Guid"/>, 
  /// <see cref="StringId"/>) или
  /// один из комлексных идентификаторов, реализующий <see cref="IComplexId"/> (<see cref="ComplexId{T1, T2}"/>) или <see cref="ComplexId{T1, T2, T3}"/></typeparam>
  public interface IIdSet<T> : IIdSet, ICollection<T>, ICloneableReadOnlyObject<IIdSet<T>>
    where T : struct, IEquatable<T>
  {
    #region Методы и свойства

    /// <summary>
    /// Преобразование в простой массив идентификаторов.
    /// Каждый раз создается новый массив, в том числе и для <see cref="IdArray{T}"/>.
    /// </summary>
    /// <returns>Массив</returns>
    T[] ToArray();

    /// <summary>
    /// Возвращает true, если есть хотя бы один совпадающий идентификатор.
    /// Если <paramref name="others"/> задает пустой список или равно null, то возвращается false.
    /// </summary>
    /// <param name="others">Список проверяемых идентификаторов</param>
    /// <returns>true при наличии пересечения</returns>
    bool ContainsAny(IEnumerable<T> others);

    /// <summary>
    /// Возвращает true, если есть хотя бы один совпадающий идентификатор.
    /// Если <paramref name="others"/> задает пустой список или равно null, то возвращается false.
    /// </summary>
    /// <param name="others">Список проверяемых идентификаторов</param>
    /// <param name="someMatchedId">Сюда помещается первый совпадающий идентификатор.
    /// При этом неизвестно, какой именно идентификатор из набора будет возвращен</param>
    /// <returns>true при наличии пересечения</returns>
    bool ContainsAny(IEnumerable<T> others, out T someMatchedId);

    /// <summary>
    /// Возвращает true, если в текущем списке есть ВСЕ идентификаторы из второго списка.
    /// Если <paramref name="others"/> задает пустой список или равно null, то возвращается true.
    /// </summary>
    /// <param name="others">Список проверяемых идентификаторов</param>
    /// <returns>true при наличии всех идентификаторов</returns>
    bool ContainsAll(IEnumerable<T> others);

    /// <summary>
    /// Добавление идентификаторов в набор (операция сложения).
    /// Пустые и повторяющиеся идентификаторы пропускаются.
    /// Если <paramref name="source"/>=null или задает пустой набор, никаких действий не выполняется.
    /// Выбрасывает исключение, если <see cref="IReadOnlyObject.IsReadOnly"/>=true.
    /// </summary>
    /// <param name="source">Список добавляемых идентификаторов. Обычно, но необязятельно, это другой набор <see cref="IIdSet"/>.
    /// Если null, то никаких действий не выполняется.</param>
    void AddRange(IEnumerable<T> source);

    /// <summary>
    /// Удаление идентификаторов из набора (операция вычитания).
    /// Пустые и повторяющиеся идентификаторы пропускаются.
    /// Если <paramref name="source"/>=null или задает пустой набор, никаких действий не выполняется.
    /// Выбрасывает исключение, если <see cref="IReadOnlyObject.IsReadOnly"/>=true.
    /// </summary>
    /// <param name="source">Список удаляемых идентификаторов. Обычно, но необязятельно, это другой набор <see cref="IIdSet"/>.
    /// Если null, то никаких действий не выполняется.</param>
    void RemoveRange(IEnumerable<T> source);

    /// <summary>
    /// Удаление идентификаторов из набора, которых нет в <paramref name="source"/> (операция пересечения).
    /// Если <paramref name="source"/>=null или задает пустой набор, то текущий список очищается.
    /// Выбрасывает исключение, если <see cref="IReadOnlyObject.IsReadOnly"/>=true.
    /// </summary>
    /// <param name="source">Список добавляемых идентификаторов. Обычно, но необязятельно, это другой набор <see cref="IIdSet"/>.
    /// Если null, то текущая коллекция очищается.</param>
    void RemoveOthers(IEnumerable<T> source);

    /// <summary>
    /// Возвращает идентификатор, если он единственный в списке (<see cref="System.Collections.ICollection.Count"/>==1).
    /// Иначе возвращается пустое значение идентификатора.
    /// </summary>
    T SingleId { get; }

    #endregion
  }

  /// <summary>
  /// Список идентфикаторов заданного типа, в котором порядок идентификаторов имеет значение и к ним можно обращаться по индексу.
  /// Реализуется классами <see cref="IdArray{T}"/> и <see cref="IdList{T}"/>.
  /// </summary>
  public interface IIndexedIdSet<T> : IIdSet<T>
    where T : struct, IEquatable<T>
  {
    #region Свойства

    /// <summary>
    /// Возвращает идентификатор по его позиции в списке / массиве.
    /// </summary>
    /// <param name="index">Индекс элемента. Должен быть в диапазоне от 0 до (<see cref="System.Collections.ICollection.Count"/>-1)</param>
    /// <returns>Идентификатор из набора</returns>
    T this[int index] { get; }

    #endregion
  }

  /// <summary>
  /// Нетипизированный список таблиц и идентификаторов.
  /// Интерфейс используется в тех случаях, когда тип идентификаторов не известен в данном контексте.
  /// В остальных случаях применятся типизированый интерфейс <see cref="ITableIdSet{T}"/>.
  /// </summary>
  public interface ITableIdSet
  {
    #region Свойства

    /// <summary>
    /// Возвращает тип идентификаторов.
    /// Тип является структурой, одной из поддерживаемых в <see cref="IIdSet{T}"/>.
    /// </summary>
    Type IdType { get; }

    /// <summary>
    /// Возвращает тип наборов идентификаторов для таблиц <see cref="IIdSet.Kind"/>.
    /// </summary>
    IdSetKind Kind { get; }

    /// <summary>
    /// Возвращает true, если регистр имен таблиц не уситывается.
    /// Свойство не влияет на сравнение идентификаторов <see cref="StringId"/>, которые всегда чувствительны к регистру
    /// </summary>
    bool IgnoreCase { get; }

    #endregion

    #region Методы

    /// <summary>
    /// Переводит коллекцию в режим "Только чтение".
    /// Если <see cref="IReadOnlyObject.IsReadOnly"/> уже равно true, никаких действий не выполняется.
    /// </summary>
    void SetReadOnly();

    #endregion
  }

  /// <summary>
  /// Интерфейс словаря, который хранит имена таблиц и набор идентификаторов для каждой из них
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface ITableIdSet<T> : ITableIdSet, IDictionary<string, IIdSet<T>>
      where T : struct, IEquatable<T>
  {
    #region Свойства

    #endregion
  }

  #region IIdExtractor

  /// <summary>
  /// Нетипизированный интерфейс для извлечения идентификаторов строк из какого-либо источника (таблицы <see cref="DataTable"/>, читателя <see cref="DbDataReader"/> или другого).
  /// В один момент времени экстрактору доступен единственный идентификатор для текущей строки источника.
  /// Используется, если тип идентификаторов не известен в текущем контексте.
  /// Если тип известен, используйте типизированный базовый класс <see cref="IdExtractor{T}"/>.
  /// </summary>
  public interface IIdExtractor
  {
    /// <summary>
    /// Добавить текущий идентификатор к заданному набору <see cref="IdCollection{T}"/> или <see cref="IdList{T}"/>.
    /// Если текушая строка содержит пустой идентификатор, никаких действий не выполняется.
    /// </summary>
    /// <param name="idSet">Заполняемый набор. Свойство <see cref="IReadOnlyObject.IsReadOnly"/> набора должно возвращать false.</param>
    void AddToSet(IIdSet idSet);
  }

  /// <summary>
  /// Типизированный базовый класс для извлечения идентификаторов строк из какого-либо источника (таблицы <see cref="DataTable"/>, читателя <see cref="DbDataReader"/> или другого).
  /// В один момент времени экстрактору доступен единственный идентификатор для текущей строки источника.
  /// Экстракторы возвращаются методами класса <see cref="IdTools"/>.
  /// </summary>
  /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
  public abstract class IdExtractor<T> : IIdExtractor
    where T : struct, IEquatable<T>
  {
    /// <summary>
    /// Возвращает текущий идентификатор в процессе перебора строк источника.
    /// Может возвращать пустое (нулевое) значение.
    /// </summary>
    public abstract T Id { get; }

    /// <summary>
    /// Добавить текущий идентификатор к заданному набору <see cref="IdCollection{T}"/> или <see cref="IdList{T}"/>.
    /// Если текушая строка содержит пустой идентификатор <see cref="Id"/>, никаких действий не выполняется.
    /// </summary>
    /// <param name="idSet">Заполняемый набор. Свойство <see cref="IReadOnlyObject.IsReadOnly"/> набора должно возвращать false.</param>
    public void AddToSet(IIdSet<T> idSet)
    {
      T id2 = Id;
      if (!id2.Equals(default(T)))
        idSet.Add(id2);
    }

    void IIdExtractor.AddToSet(IIdSet idSet)
    {
      AddToSet((IIdSet<T>)idSet);
    }
  }

  #endregion

  /// <summary>
  /// Статические методы для работы с интерфейсом <see cref="IIdSet"/>
  /// </summary>
  public static class IdTools
  {
    #region Создание коллекций

    /// <summary>
    /// Создает <see cref="IdCollection{T}"/> для заданного типа ключа
    /// </summary>
    /// <param name="idType">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</param>
    /// <returns>Пустой набор идентификаторов, который можно заполнять</returns>
    public static IIdSet CreateIdCollection(Type idType)
    {
      CheckIsValidIdType(idType);

      Type tSet = typeof(IdCollection<>).MakeGenericType(new Type[1] { idType });
      return (IIdSet)(tSet.GetConstructor(EmptyArray<Type>.Empty).Invoke(EmptyArray<Object>.Empty));
    }

    /// <summary>
    /// Создает <see cref="IdList{T}"/> для заданного типа ключа
    /// </summary>
    /// <param name="idType">Тип ключа.
    /// Список доступных типов см. в <see cref="IIdSet{T}"/>.</param>
    /// <returns>Пустой список идентификаторов, который можно заполнять</returns>
    public static IIdSet CreateIdList(Type idType)
    {
      CheckIsValidIdType(idType);

      Type tSet = typeof(IdList<>).MakeGenericType(new Type[1] { idType });
      return (IIdSet)(tSet.GetConstructor(EmptyArray<Type>.Empty).Invoke(EmptyArray<Object>.Empty));
    }

    /// <summary>
    /// Возращает пустой массив <see cref="IdArray{T}.Empty"/>.
    /// </summary>
    /// <param name="idType">Тип идентификаторов.
    /// Список доступных типов см. в <see cref="IIdSet{T}"/>.</param>
    /// <returns>Пустой массив идентификаторов</returns>
    public static IIdSet GetEmptyArray(Type idType)
    {
      CheckIsValidIdType(idType);

      Type tSet = typeof(IdArray<>).MakeGenericType(new Type[1] { idType });
      //PropertyInfo pi = tSet.GetProperty("Empty", BindingFlags.Static | BindingFlags.Public);
      FieldInfo pi = tSet.GetField("Empty", BindingFlags.Static | BindingFlags.Public);
#if DEBUG
      if (pi == null)
        throw new BugException("pi=null");
#endif
      //return (IIdSet)(pi.GetValue(null, EmptyArray<object>.Empty));
      return (IIdSet)(pi.GetValue(null));
    }

    private static IIdSet<T> CreateIdSet<T>(bool saveOrder)
      where T : struct, IEquatable<T>
    {
      if (saveOrder)
        return new IdList<T>();
      else
        return new IdCollection<T>();
    }

    private static IIdSet CreateIdSet(Type idType, bool saveOrder)
    {
      if (saveOrder)
        return CreateIdList(idType);
      else
        return CreateIdCollection(idType);
    }

    /// <summary>
    /// Создать массив, коллекцию или список идентификаторов из перечислимого набора.
    /// Пустые и повторяющиеся идентификаторы пропускаются.
    /// </summary>
    /// <typeparam name="T">Тип идентификторов. 
    /// Список допустиымых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="source">Перечислимая коллекция идентификаторов</param>
    /// <param name="kind">Тип требуемого набора</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet<T> CreateIdSet<T>(IEnumerable<T> source, IdSetKind kind)
      where T : struct, IEquatable<T>
    {
      switch (kind)
      {
        case IdSetKind.List: return new IdList<T>(source);
        case IdSetKind.Collection: return new IdCollection<T>(source);
        case IdSetKind.Array: return new IdArray<T>(source);
        default:
          throw ExceptionFactory.ArgUnknownValue("kind", kind);
      }
    }

    #endregion

    #region Преобразования IIdSet

    private interface IConverterHelper
    {
      object CreateConvertEnumerable(object src);
    }

    private class ConvertHelpert<TSource, TResult> : IConverterHelper
    {
      internal ConvertHelpert(Converter<TSource, TResult> converter)
      {
        _Converter = converter;
      }

      private readonly Converter<TSource, TResult> _Converter;

      object IConverterHelper.CreateConvertEnumerable(object src)
      {
        return new ConvertEnumerable<TSource, TResult>((IEnumerable<TSource>)src, _Converter);
      }
    }

    private struct TypePairKey : IEquatable<TypePairKey>
    {
      #region Конструктор

      internal TypePairKey(Type tSource, Type tResult)
      {
        _TSource = tSource;
        _TResult = tResult;
      }

      #endregion

      #region Поля

      private readonly Type _TSource;
      private readonly Type _TResult;

      #endregion

      #region Сравнение

      public static bool operator ==(TypePairKey a, TypePairKey b)
      {
        return a._TSource == b._TSource && a._TResult == b._TResult;
      }
      public static bool operator !=(TypePairKey a, TypePairKey b)
      {
        return !(a == b);
      }

      public bool Equals(TypePairKey other)
      {
        return _TSource == other._TSource && _TResult == other._TResult;
      }

      public override bool Equals(object obj)
      {
        if (obj is TypePairKey)
          return Equals((TypePairKey)obj);
        else
          return false;
      }
      public override int GetHashCode()
      {
        return _TSource.GetHashCode() ^ _TResult.GetHashCode();
      }

      #endregion
    }

    private static readonly IDictionary<TypePairKey, IConverterHelper> _ConvertHelpers = CreateConvertHelpers();

    private static IDictionary<TypePairKey, IConverterHelper> CreateConvertHelpers()
    {
      Dictionary<TypePairKey, IConverterHelper> dict = new Dictionary<TypePairKey, IConverterHelper>();

      dict.Add(new TypePairKey(typeof(Int32), typeof(Int64)), new ConvertHelpert<Int32, Int64>(Converter_Int32_Int64));
      dict.Add(new TypePairKey(typeof(Int64), typeof(Int32)), new ConvertHelpert<Int64, Int32>(Converter_Int64_Int32));

      return dict;
    }

    private static long Converter_Int32_Int64(int input)
    {
      return System.Convert.ToInt64(input);
    }

    private static int Converter_Int64_Int32(long input)
    {
      return System.Convert.ToInt32(input);
    }

    private static IConverterHelper GetConverterHelperRequired(Type tSource, Type tResult)
    {
      TypePairKey types = new TypePairKey(tSource, tResult);
      IConverterHelper convHelper;
      if (_ConvertHelpers.TryGetValue(types, out convHelper))
        return convHelper;
      else
        throw new InvalidCastException(String.Format("There is not convertion from type '{0}' to type '{1}'", tSource, tResult));
    }

    /// <summary>
    /// Возвращает типизированный перечислитель идентификаторов для указанного набора.
    /// Может выполняться преобразование типов <see cref="System.Int32"/> и <see cref="System.Int64"/>.
    /// </summary>
    /// <typeparam name="T">Тип идентификаторов.
    /// Список доступных типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="ids">Исходный массив идентификаторов</param>
    /// <returns></returns>
    public static IEnumerable<T> AsEnumerable<T>(IIdSet ids)
      where T : struct, IEquatable<T>
    {
      IIdSet<T> ids2 = ids as IIdSet<T>;
      if (ids2 != null)
        return ids2;

      IConverterHelper convHelper = GetConverterHelperRequired(ids.IdType, typeof(T));
      return (IEnumerable<T>)(convHelper.CreateConvertEnumerable(ids));
    }

    /// <summary>
    /// Приведение коллекции неопределенного типа к заданному
    /// </summary>
    /// <typeparam name="T">Тип идентификатора</typeparam>
    /// <param name="ids">Коллекция идентификаторов неизвестного типа</param>
    /// <returns></returns>
    public static IIdSet<T> AsIdSet<T>(IIdSet ids)
      where T : struct, IEquatable<T>
    {
      IIdSet<T> ids2 = ids as IIdSet<T>;
      if (ids2 != null)
        return ids2;

      if (ids == null)
        return IdArray<T>.Empty;

      IConverterHelper convHelper = GetConverterHelperRequired(ids.IdType, typeof(T));
      IEnumerable<T> en = (IEnumerable<T>)(convHelper.CreateConvertEnumerable(ids));
      if (ids.Kind == IdSetKind.Collection)
        return new IdCollection<T>(en);
      else
        return new IdList<T>(en);
    }

    /// <summary>
    /// Приведение произвольного перечислителя идентификаторов к набору.
    /// Если <paramref name="ids"/> уже является набором, он возвращается без изменений.
    /// Если <paramref name="ids"/> является массивом или списком, то возвращается <see cref="IdList{T}"/>.
    /// В противном случае возвращается неупорядоченная коллекция <see cref="IdCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">Тип идентификаторов</typeparam>
    /// <param name="ids">Исходный перечислитель</param>
    /// <returns>Ссылка на <paramref name="ids"/> или новый набор</returns>
    public static IIdSet<T> AsIdSet<T>(IEnumerable<T> ids)
      where T : struct, IEquatable<T>
    {
      IIdSet<T> ids2 = ids as IIdSet<T>;
      if (ids2 != null)
        return ids2;

      if (ids == null)
        return IdArray<T>.Empty;

      if ((ids is Array) || (ids is IList<T>))
        return new IdList<T>(ids);
      else
        return new IdCollection<T>(ids);
    }

    /// <summary>
    /// Приведение произвольного перечислителя идентификаторов к массиву идентификаторов <see cref="IdArray{T}"/>.
    /// Если <paramref name="ids"/> уже является массивом, он возвращается без изменений.
    /// </summary>
    /// <typeparam name="T">Тип идентификаторов</typeparam>
    /// <param name="ids">Исходный перечислитель</param>
    /// <returns>Ссылка на <paramref name="ids"/> или новый набор</returns>
    public static IdArray<T> AsIdArray<T>(IEnumerable<T> ids)
      where T : struct, IEquatable<T>
    {
      IdArray<T> ids2 = ids as IdArray<T>;
      if (ids2 != null)
        return ids2;

      if (ids == null)
        return IdArray<T>.Empty;

      return new IdArray<T>(ids);
    }

    #endregion

    #region Сравнение наборов

    /// <summary>
    /// Возвращает true, если два набора содержат одинаковые идентификаторы.
    /// Порядок идентификаторов может отличаться
    /// </summary>
    /// <typeparam name="T">Тип идентификаторов</typeparam>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй набор</param>
    /// <returns></returns>
    public static bool AreEqual<T>(IIdSet<T> a, IIdSet<T> b)
      where T : struct, IEquatable<T>
    {
      if (a == null)
        a = IdArray<T>.Empty;
      if (b == null)
        b = IdArray<T>.Empty;
      if (Object.ReferenceEquals(a, b))
        return true;

      if (a.Count != b.Count)
        return false;

      return a.ContainsAll(b);
    }


    /// <summary>
    /// Возвращает true, если два набора содержат одинаковые идентификаторы.
    /// Порядок идентификаторов может отличаться.
    /// </summary>
    /// <typeparam name="T">Тип идентификаторов</typeparam>
    /// <param name="a">Первый набор</param>
    /// <param name="b">Второй набор</param>
    /// <returns></returns>
    public static bool AreEqual<T>(ITableIdSet<T> a, ITableIdSet<T> b)
      where T : struct, IEquatable<T>
    {
      if (a == null)
        a = TableIdCollection<T>.Empty;
      if (b == null)
        b = TableIdCollection<T>.Empty;
      if (Object.ReferenceEquals(a, b))
        return true;

      bool ignoreCase = a.IgnoreCase && b.IgnoreCase;
      SingleScopeStringList tableNames = new SingleScopeStringList(ignoreCase);
      foreach (KeyValuePair<string, IIdSet<T>> pair in a)
        tableNames.Add(pair.Key);
      foreach (KeyValuePair<string, IIdSet<T>> pair in b)
        tableNames.Add(pair.Key);

      foreach (string tableName in tableNames)
      {
        IIdSet<T> ids1, ids2;
        if (a.ContainsKey(tableName))
          ids1 = a[tableName];
        else
          ids1 = IdArray<T>.Empty;
        if (b.ContainsKey(tableName))
          ids2 = b[tableName];
        else
          ids2 = IdArray<T>.Empty;

        if (!AreEqual<T>(ids1, ids2))
          return false;
      }

      return true;
    }

    #endregion

    #region Определение типа ключевого поля

    /// <summary>
    /// Максимальная длина составного ключа.
    /// В текущей реализации доступны составные идентификаторы из двух <see cref="ComplexId{T1, T2}"/> и трех <see cref="ComplexId{T1, T2, T3}"/> полей.
    /// </summary>
    public const int MaxKeyColumnCount = 3;

    private static readonly Type[] _ValidDataTypes = new Type[] {
      typeof(Int32),
      typeof(Int64),
      typeof(Guid),
      typeof(String)};

    /// <summary>
    /// Проверяет, является ли тип данных поля допустимым в качестве ключевого поля или части составного ключа.
    /// Не путать с типом идентификатора.
    /// </summary>
    /// <param name="dataType">Тип данных поля</param>
    /// <returns>Допустимость типа</returns>
    public static bool IsValidDataType(Type dataType)
    {
      return Array.IndexOf<Type>(_ValidDataTypes, dataType) >= 0;
    }

    /// <summary>
    /// Проверяет, является ли тип подходящим для простого или составного ключа.
    /// Список допустимых типов см. в <see cref="IIdSet{T}"/>.
    /// </summary>
    /// <param name="idType">Проверяемый тип</param>
    /// <returns>Допустимость типа</returns>
    public static bool IsValidIdType(Type idType)
    {
      if (idType == null)
        return false;
      if (idType == typeof(StringId))
        return true;
      if (idType == typeof(String))
        return false;
      if (typeof(IComplexId).IsAssignableFrom(idType))
      {
        // 18.11.2025
        Type[] args = idType.GetGenericArguments();
        if (args.Length < 2 || args.Length > MaxKeyColumnCount)
          return false;
        for (int i = 0; i < args.Length; i++)
        {
          if (typeof(IComplexId).IsAssignableFrom(args[i]))
            return false; // Вложенные структуры ComplexId не допускаются

          if (!IsValidIdType(args[i])) // рекурсивный вызов
            return false; 
        }
        return true;
      }
      return Array.IndexOf<Type>(_ValidDataTypes, idType) >= 0;
    }

    private static void CheckIsValidIdType(Type idType)
    {
      if (idType == null)
        throw new ArgumentNullException("idType");
      if (!IsValidIdType(idType))
        throw ExceptionFactory.ArgUnknownValue("idType", idType);
    }

    /// <summary>
    /// Определение типа значения ключа для заданного типа данных.
    /// Эта перегрузка используется для простого ключа из одного поля.
    /// Для допустимых целочисленных типов возвращает <paramref name="dataType"/>, 
    /// для <see cref="System.String"/> - <see cref="StringId"/>.
    /// Для остальных типов выбрасывает исключение.
    /// </summary>
    /// <param name="dataType">Тип данных поля</param>
    /// <returns>Тип ключа</returns>
    public static Type GetIdType(Type dataType)
    {
      if (dataType == null)
        throw new ArgumentNullException("dataType");
      if (dataType == typeof(String))
        return typeof(StringId);
      if (dataType == typeof(Int32) || dataType == typeof(Int64) || dataType == typeof(Guid))
        return dataType;
      throw ExceptionFactory.ArgUnknownValue("dataType", dataType,
        new Type[] { typeof(Int32), typeof(Int64), typeof(Guid), typeof(String) });
    }

    /// <summary>
    /// Определение типа значения составного ключа для заданных типов данных.
    /// </summary>
    /// <param name="dataTypes">Массив типов данных полей, составляющих ключ</param>
    /// <returns>Тип ключа</returns>
    public static Type GetIdType(Type[] dataTypes)
    {
      if (dataTypes == null)
        throw new ArgumentNullException("dataTypes");
      switch (dataTypes.Length)
      {
        case 0:
          throw ExceptionFactory.ArgIsEmpty("dataTypes");
        case 1:
          return GetIdType(dataTypes[0]);
      }

      Type[] idTypes = new Type[dataTypes.Length];
      for (int i = 0; i < dataTypes.Length; i++)
        idTypes[i] = GetIdType(dataTypes[i]);
      switch (dataTypes.Length)
      {
        case 2:
          return typeof(ComplexId<,>).MakeGenericType(idTypes);
        case 3:
          return typeof(ComplexId<,,>).MakeGenericType(idTypes);
        default:
          throw ExceptionFactory.ArgOutOfRange("dataTypes", dataTypes.Length, 1, MaxKeyColumnCount);
      }
    }

    /// <summary>
    /// Определение типа идентификаторов ссылочного поля таблицы.
    /// Для определения типа составных идентификаторов используйте <see cref="GetIdType(DataTable, DBxColumns)"/>.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Тип идентификатора</returns>
    public static Type GetIdType(DataTable table, string columnName)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      DataColumn col = table.Columns[columnName];
      if (col == null)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", table, columnName);
      return GetIdType(col.DataType);
    }

    /// <summary>
    /// Определение типа идентификаторов ссылочных полей таблицы.
    /// Эта версия используется для составных ключей
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnNames">Имена столбцов, образующих ссылку</param>
    /// <returns>Тип идентификатора</returns>
    public static Type GetIdType(DataTable table, DBxColumns columnNames)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
      if (columnNames.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("columnNames");

      Type[] dataTypes = new Type[columnNames.Count];
      for (int i = 0; i < columnNames.Count; i++)
      {
        DataColumn col = table.Columns[columnNames[i]];
        if (col == null)
          throw ExceptionFactory.ArgUnknownColumnName("columns", table, columnNames[i]);
        dataTypes[i] = col.DataType;
      }
      return GetIdType(dataTypes);
    }

    /// <summary>
    /// Определение типа идентификаторов первичного ключа таблицы.
    /// Таблица должна иметь простой или составной первичный ключ <see cref="DataTable.PrimaryKey"/>.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <returns>Тип идентификатора</returns>
    public static Type GetIdType(DataTable table)
    {
      if (table == null)
        throw new ArgumentNullException("table");

      DBxColumns columnNames = DBxColumns.FromColumns(table.PrimaryKey);
      if (columnNames.IsEmpty || columnNames.Count > MaxKeyColumnCount)
        throw new PrimaryKeyException();
      return GetIdType(table, columnNames);
    }

    /// <summary>
    /// Определение типа идентификаторов для описания структуры данных
    /// </summary>
    /// <param name="table">Описание таблицы</param>
    /// <param name="columnNames">Имена столбцов, образующих ссылку</param>
    /// <returns>Тип идентификатора</returns>
    public static Type GetIdType(DBxTableStruct table, DBxColumns columnNames)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
      if (columnNames.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("columnNames");

      Type[] dataTypes = new Type[columnNames.Count];
      for (int i = 0; i < columnNames.Count; i++)
        dataTypes[i] = table.Columns[columnNames[i]].DataType;
      return GetIdType(dataTypes);
    }

    /// <summary>
    /// Определение типа идентификаторов первичного ключа таблицы.
    /// Таблица должна иметь простой или составной первичный ключ <see cref="DataTable.PrimaryKey"/>.
    /// </summary>
    /// <param name="table">Описание таблицы</param>
    /// <returns>Тип идентификатора</returns>
    public static Type GetIdType(DBxTableStruct table)
    {
      if (table == null)
        throw new ArgumentNullException("table");

      DBxColumns columnNames = table.PrimaryKey;
      if (columnNames.IsEmpty || columnNames.Count > MaxKeyColumnCount)
        throw new PrimaryKeyException();
      return GetIdType(table, columnNames);
    }

    /// <summary>
    /// Определение типов идентификаторов ссылочных полей для <see cref="DbDataReader"/>.
    /// </summary>
    /// <param name="reader">Объект для считывания значений</param>
    /// <param name="columnNames">Имена столбцов, образующих ссылку</param>
    /// <returns>Тип идентификатора</returns>
    public static Type GetIdType(DbDataReader reader, DBxColumns columnNames)
    {
      if (reader == null)
        throw new ArgumentNullException("table");
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
      if (columnNames.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("columnNames");

      Type[] dataTypes = new Type[columnNames.Count];
      for (int i = 0; i < columnNames.Count; i++)
      {
        int colPos = reader.GetOrdinal(columnNames[i]);
        if (colPos < 0)
          throw ExceptionFactory.ArgUnknownColumnName("columns", reader, columnNames[i]);
        dataTypes[i] = reader.GetFieldType(colPos);
      }
      return GetIdType(dataTypes);
    }


    /// <summary>
    /// Определение типов идентификаторов ссылочного поля для <see cref="DbDataReader"/>.
    /// Поддерживаются только простые идентификаторы из одного поля
    /// </summary>
    /// <param name="reader">Объект для считывания значений</param>
    /// <param name="columnName">Имя ссылочного поля</param>
    /// <returns>Тип идентификатора</returns>
    public static Type GetIdType(DbDataReader reader, string columnName)
    {
      if (reader == null)
        throw new ArgumentNullException("table");
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      int colPos = reader.GetOrdinal(columnName);
      if (colPos < 0)
        throw ExceptionFactory.ArgUnknownColumnName("columnName", reader, columnName);
      Type dataType = reader.GetFieldType(colPos);
      return GetIdType(dataType);
    }

    /// <summary>
    /// Возвращает типы данных (один или несколько для составных ключей) для заданного типа идентификаторов.
    /// Если передан недопустимый тип идентификатора, выбрасывается исключение.
    /// </summary>
    /// <param name="idType">Тип идентификатора.
    /// Список допустимых типов см. в <see cref="IIdSet{T}"/>.</param>
    /// <returns>Массив из одного или нескольких типов</returns>
    public static Type[] GetDataTypes(Type idType)
    {
      if (idType == null)
        throw new ArgumentNullException("idType");

      if (typeof(IComplexId).IsAssignableFrom(idType))
      {
        Type[] subIdTypes = idType.GetGenericArguments();
        Type[] a = new Type[subIdTypes.Length];
        for (int i = 0; i < subIdTypes.Length; i++)
          a[i] = DoGetDataType(subIdTypes[i]);
        return a;
      }
      else
        return new Type[1] { DoGetDataType(idType) };
    }

    private static Type DoGetDataType(Type idType)
    {
      if (idType == typeof(Int32) || idType == typeof(Int64) || idType == typeof(Guid))
        return idType;
      if (idType == typeof(StringId))
        return typeof(String);
      throw ExceptionFactory.ArgUnknownValue("idType", idType, new Type[] {
        typeof(Int32), typeof(Int64), typeof(Guid), typeof(StringId)});
    }

    #endregion

    #region Получение IIdExtractor

    #region Классы IdExtractor

    private class Int32IdExtractor : IdExtractor<Int32>
    {
      internal Int32IdExtractor(IDataColumnValue source)
      {
        _Source = source;
      }
      private readonly IDataColumnValue _Source;

      public override int Id { get { return _Source.AsInt32; } }
    }

    private class Int64IdExtractor : IdExtractor<Int64>
    {
      internal Int64IdExtractor(IDataColumnValue source)
      {
        _Source = source;
      }
      private readonly IDataColumnValue _Source;

      public override long Id { get { return _Source.AsInt64; } }
    }

    private class GuidIdExtractor : IdExtractor<Guid>
    {
      internal GuidIdExtractor(IDataColumnValue source)
      {
        _Source = source;
      }
      private readonly IDataColumnValue _Source;

      public override Guid Id { get { return _Source.AsGuid; } }
    }

    private class StringIdExtractor : IdExtractor<StringId>
    {
      internal StringIdExtractor(IDataColumnValue source)
      {
        _Source = source;
      }
      private readonly IDataColumnValue _Source;

      public override StringId Id { get { return new StringId(_Source.AsString); } }
    }

    private class Complex2IdExtractor<T1, T2> : IdExtractor<ComplexId<T1, T2>>
      where T1 : struct, IEquatable<T1>
      where T2 : struct, IEquatable<T2>
    {
      public Complex2IdExtractor(IdExtractor<T1> extractor1, IdExtractor<T2> extractor2)
      {
        _Extractor1 = extractor1;
        _Extractor2 = extractor2;
      }

      private readonly IdExtractor<T1> _Extractor1;
      private readonly IdExtractor<T2> _Extractor2;

      public override ComplexId<T1, T2> Id
      {
        get
        {
          T1 v1 = _Extractor1.Id;
          T2 v2 = _Extractor2.Id;
          if (v1.Equals(default(T1)) || v2.Equals(default(T2)))
            return new ComplexId<T1, T2>();
          else
            return new ComplexId<T1, T2>(v1, v2);
        }
      }
    }

    private class Complex3IdExtractor<T1, T2, T3> : IdExtractor<ComplexId<T1, T2, T3>>
      where T1 : struct, IEquatable<T1>
      where T2 : struct, IEquatable<T2>
      where T3 : struct, IEquatable<T3>
    {
      public Complex3IdExtractor(IdExtractor<T1> extractor1, IdExtractor<T2> extractor2, IdExtractor<T3> extractor3)
      {
        _Extractor1 = extractor1;
        _Extractor2 = extractor2;
        _Extractor3 = extractor3;
      }

      private readonly IdExtractor<T1> _Extractor1;
      private readonly IdExtractor<T2> _Extractor2;
      private readonly IdExtractor<T3> _Extractor3;

      public override ComplexId<T1, T2, T3> Id
      {
        get
        {
          T1 v1 = _Extractor1.Id;
          T2 v2 = _Extractor2.Id;
          T3 v3 = _Extractor3.Id;
          if (v1.Equals(default(T1)) || v2.Equals(default(T2)) || v3.Equals(default(T3)))
            return new ComplexId<T1, T2, T3>();
          else
            return new ComplexId<T1, T2, T3>(v1, v2, v3);
        }
      }
    }

    #endregion

    #region CreateExtractor()

    /// <summary>
    /// Получение объекта для извлечения значения ссылочного поля из строк таблицы <see cref="DataTable"/>.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Доступные типы см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="source">Источник для перебора строк</param>
    /// <param name="columnName">Имя ссылочного столбца</param>
    /// <returns>Извлекатель</returns>
    public static IdExtractor<T> CreateExtractor<T>(IDataRowValues source, string columnName)
      where T : struct, IEquatable<T>
    {
      return (IdExtractor<T>)CreateExtractor(source, columnName, typeof(T));
    }

    /// <summary>
    /// Получение объекта для извлечения значения ссылочного поля из строк таблицы <see cref="DataTable"/>.
    /// Эта версия используется для извлечения составных идентификаторов из нескольких полей.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Доступные типы см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="source">Источник для перебора строк</param>
    /// <param name="columnNames">Имена полей, образующих ссылку</param>
    /// <returns>Извлекатель</returns>
    public static IdExtractor<T> CreateExtractor<T>(IDataRowValues source, DBxColumns columnNames)
      where T : struct, IEquatable<T>
    {
      return (IdExtractor<T>)CreateExtractor(source, columnNames, typeof(T));
    }

    /// <summary>
    /// Получение объекта для извлечения значения ссылочного поля из строк таблицы <see cref="DataTable"/>.
    /// Используйте перегрузку <see cref="CreateExtractor{T}(IDataRowValues, string)"/>, если тип идентификаторов является фиксированным.
    /// </summary>
    /// <param name="source">Источник для перебора строк</param>
    /// <param name="columnName">Имя ссылочного столбца</param>
    /// <param name="idType">Тип идентификатора. Доступные типы см. в <see cref="IIdSet{T}"/>.</param>
    /// <returns>Извлекатель</returns>
    public static IIdExtractor CreateExtractor(IDataRowValues source, string columnName, Type idType)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif
      if (idType == null)
        throw new ArgumentNullException("idType");

      IDataColumnValue colValue = source[columnName];
      if (idType == typeof(Int32))
        return new Int32IdExtractor(colValue);
      if (idType == typeof(Int64))
        return new Int64IdExtractor(colValue);
      if (idType == typeof(Guid))
        return new GuidIdExtractor(colValue);
      if (idType == typeof(StringId))
        return new StringIdExtractor(colValue);

      if (typeof(IComplexId).IsAssignableFrom(idType))
        throw new ArgumentException("This overload cannot be used for complex id", "idtype");
      throw ExceptionFactory.ArgUnknownValue("idType", idType, new Type[] {
        typeof(Int32), typeof(Int64), typeof(Guid), typeof(StringId) });
    }

    /// <summary>
    /// Получение объекта для извлечения значения ссылочного поля из строк таблицы <see cref="DataTable"/>.
    /// Используйте перегрузку <see cref="CreateExtractor{T}(IDataRowValues, string)"/>, если тип идентификаторов является фиксированным.
    /// Эта версия используется для извлечения составных идентификаторов из нескольких полей.
    /// </summary>
    /// <param name="source">Источник для перебора строк</param>
    /// <param name="columnNames">Имена полей, образующих ссылку</param>
    /// <param name="idType">Тип идентификатора. Доступные типы см. в <see cref="IIdSet{T}"/>.</param>
    /// <returns>Извлекатель</returns>
    public static IIdExtractor CreateExtractor(IDataRowValues source, DBxColumns columnNames, Type idType)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
      if (columnNames.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("columnNames");
#endif
      if (idType == null)
        throw new ArgumentNullException("idType");

      if (typeof(IComplexId).IsAssignableFrom(idType))
      {
        Type[] partTypes = idType.GetGenericArguments();
        if (columnNames.Count != partTypes.Length)
          throw ExceptionFactory.ArgWrongCollectionCount("columnNames", columnNames, partTypes.Length);
        Type extractorType;
        Type[] ctorArgTypes = new Type[partTypes.Length];
        object[] ctorArgs = new object[partTypes.Length];
        for (int i = 0; i < partTypes.Length; i++)
        {
          ctorArgTypes[i] = typeof(IdExtractor<>).MakeGenericType(new Type[1] { partTypes[i] });
          ctorArgs[i] = CreateExtractor(source, columnNames[i], partTypes[i]);
        }
        switch (partTypes.Length)
        {
          case 2:
            extractorType = typeof(Complex2IdExtractor<,>).MakeGenericType(partTypes);
            break;
          case 3:
            extractorType = typeof(Complex3IdExtractor<,,>).MakeGenericType(partTypes);
            break;
          default:
            throw new NotImplementedException(String.Format("Complex id extractor is not implemented for part count of {0}", partTypes.Length));
        }

        ConstructorInfo ci = extractorType.GetConstructor(ctorArgTypes);
        if (ci == null)
          throw new BugException(String.Format("Constructor not found for IdExtractor of type '{0}'. Constructor arguments are: {1}", extractorType,
            StringTools.ToStringJoin<Type>(", ", ctorArgTypes)));
        return (IIdExtractor)(ci.Invoke(ctorArgs));
      }
      else
      {
        if (columnNames.Count != 1)
          throw ExceptionFactory.ArgWrongCollectionCount("columnNames", columnNames, 1);
        return CreateExtractor(source, columnNames[0], idType);
      }
    }

    #endregion

    #endregion

    #region Извлечение значений из DbDataReader

    /// <summary>
    /// Извлечение значений ссылочных полей из <see cref="DbDataReader"/>.
    /// Порядок идентификаторов в возвращаемом наборе соответствует порядку строк при чтении.
    /// Перегрузка поддерживает только простые идентификаторы, состоящие из одного поля.
    /// Поле может содержать значения <see cref="DBNull"/> или потворы, которые отбрасыааются.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="reader">Считыватель данных</param>
    /// <param name="columnName">Имя ссылочного поля</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIdsFromColumn(DbDataReader reader, string columnName)
    {
      return GetIdsFromColumn(reader, columnName, true);
    }

    /// <summary>
    /// Извлечение значений ссылочных полей из <see cref="DbDataReader"/>.
    /// Перегрузка поддерживает только простые идентификаторы, состоящие из одного поля.
    /// Поле может содержать значения <see cref="DBNull"/> или потворы, которые отбрасыааются.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="columnName"></param>
    /// <param name="saveOrder">Если true, то порядок идентификаторов в возвращаемом наборе должен соответствовать порядку строк в таблице 
    /// (должен возвращаться <see cref="IIndexedIdSet{T}"/>).
    /// Если false, то порядок идентификаторов не имеет значения и можно вернуть <see cref="IdCollection{T}"/>.</param>
    /// <returns></returns>
    public static IIdSet GetIdsFromColumn(DbDataReader reader, string columnName, bool saveOrder)
    {
      Type idType = GetIdType(reader, columnName);
      IIdSet idSet = CreateIdSet(idType, saveOrder);

      DbDataReaderValues rvs = new DbDataReaderValues(reader);
      IIdExtractor extractor = CreateExtractor(rvs, columnName, idType);
      while (reader.Read())
        extractor.AddToSet(idSet);
      return idSet;
    }

    /// <summary>
    /// Извлечение значений ссылочных полей из <see cref="DbDataReader"/>.
    /// Перегрузка поддерживает только простые идентификаторы, состоящие из одного поля.
    /// Порядок идентификаторов в возвращаемом наборе соответствует порядку строк при чтении.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Допустимые типы см. в описании <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="reader">Считыватель данных</param>
    /// <param name="columnName">Имя ссылочного поля</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIndexedIdSet<T> GetIdsFromColumn<T>(DbDataReader reader, string columnName)
      where T : struct, IEquatable<T>
    {
      return (IIndexedIdSet<T>)GetIdsFromColumn<T>(reader, columnName, true);
    }

    /// <summary>
    /// Извлечение значений ссылочных полей из <see cref="DbDataReader"/>.
    /// Перегрузка поддерживает только простые идентификаторы, состоящие из одного поля.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Допустимые типы см. в описании <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="reader">Считыватель данных</param>
    /// <param name="columnName">Имя ссылочного поля</param>
    /// <param name="saveOrder">Если true, то порядок идентификаторов в возвращаемом наборе должен соответствовать порядку перебора строк
    /// (должен возвращаться <see cref="IIndexedIdSet{T}"/>).
    /// Если false, то порядок идентификаторов не имеет значения и можно вернуть <see cref="IdCollection{T}"/>.</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet<T> GetIdsFromColumn<T>(DbDataReader reader, string columnName, bool saveOrder)
      where T : struct, IEquatable<T>
    {
      IIdSet<T> idSet = CreateIdSet<T>(saveOrder);

      DbDataReaderValues rvs = new DbDataReaderValues(reader);
      IdExtractor<T> extractor = CreateExtractor<T>(rvs, columnName);
      while (reader.Read())
        extractor.AddToSet(idSet);
      return idSet;
    }


    /// <summary>
    /// Извлечение значений ссылочных полей из <see cref="DbDataReader"/> для составных идентификаторов.
    /// Порядок идентификаторов в возвращаемом наборе соответствует порядку строк при чтении.
    /// Если хотя бы одно из полей, образующих ссылку, содержит значение <see cref="DBNull"/>, идентификатор считается пустым и пропускается.
    /// Если комбинация значений образует повторяюющийся идентикатор, он тоже пропускается.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="reader">Считыватель данных</param>
    /// <param name="columnNames">Имена полей, образующих ссылку</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIdsFromColumns(DbDataReader reader, DBxColumns columnNames)
    {
      return GetIdsFromColumns(reader, columnNames, true);
    }

    /// <summary>
    /// Извлечение значений ссылочных полей из <see cref="DbDataReader"/> для составных идентификаторов.
    /// Если хотя бы одно из полей, образующих ссылку, содержит значение <see cref="DBNull"/>, идентификатор считается пустым и пропускается.
    /// Если комбинация значений образует повторяюющийся идентикатор, он тоже пропускается.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="reader">Считыватель данных</param>
    /// <param name="columnNames">Имена полей, образующих ссылку</param>
    /// <param name="saveOrder">Если true, то порядок идентификаторов в возвращаемом наборе должен соответствовать порядку строк при переборе
    /// (должен возвращаться <see cref="IIndexedIdSet{T}"/>).
    /// Если false, то порядок идентификаторов не имеет значения и можно вернуть <see cref="IdCollection{T}"/>.</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIdsFromColumns(DbDataReader reader, DBxColumns columnNames, bool saveOrder)
    {
      Type idType = GetIdType(reader, columnNames);
      IIdSet idSet = CreateIdSet(idType, saveOrder);

      DbDataReaderValues rvs = new DbDataReaderValues(reader);
      IIdExtractor extractor = CreateExtractor(rvs, columnNames, idType);
      while (reader.Read())
        extractor.AddToSet(idSet);
      return idSet;
    }

    /// <summary>
    /// Извлечение значений ссылочных полей из <see cref="DbDataReader"/> для составных идентификаторов.
    /// Порядок идентификаторов в возвращаемом наборе соответствует порядку строк при чтении.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Допустимые типы см. в описании <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="reader">Считыватель данных</param>
    /// <param name="columnNames">Имена полей, образующих ссылку</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIndexedIdSet<T> GetIdsFromColumns<T>(DbDataReader reader, DBxColumns columnNames)
      where T : struct, IEquatable<T>
    {
      return (IIndexedIdSet<T>)GetIdsFromColumns<T>(reader, columnNames, true);
    }

    /// <summary>
    /// Извлечение значений ссылочных полей из <see cref="DbDataReader"/> для составных идентификаторов.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Допустимые типы см. в описании <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="reader">Считыватель данных</param>
    /// <param name="columnNames">Имена полей, образующих ссылку</param>
    /// <param name="saveOrder">Если true, то порядок идентификаторов в возвращаемом наборе должен соответствовать порядку строк при переборе 
    /// (должен возвращаться <see cref="IIndexedIdSet{T}"/>).
    /// Если false, то порядок идентификаторов не имеет значения и можно вернуть <see cref="IdCollection{T}"/>.</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet<T> GetIdsFromColumns<T>(DbDataReader reader, DBxColumns columnNames, bool saveOrder)
      where T : struct, IEquatable<T>
    {
      IIdSet<T> idSet = CreateIdSet<T>(saveOrder);

      DbDataReaderValues rvs = new DbDataReaderValues(reader);
      IdExtractor<T> extractor = CreateExtractor<T>(rvs, columnNames);
      while (reader.Read())
        extractor.AddToSet(idSet);
      return idSet;
    }

    #endregion

    #region Извлечение значений из DataTable

    #region Методы, возвращающие обобщенный IIdSet

    #region DataTable

    /// <summary>
    /// Извлечение идентификаторов таблицы.
    /// Таблица должна иметь простой или составной первичный ключ.
    /// Если таблица не имеет первичного ключа, используйте метод <see cref="GetIdsFromColumn(DataTable, string)"/> или <see cref="GetIdsFromColumns(DataTable, DBxColumns)"/>.
    /// Порядок идентификаторов будет соответствовать порядку строк в таблице.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="table">Таблица, откуда извлекаются идентификаторы</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIds(DataTable table)
    {
      return GetIds(table, true);
    }

    /// <summary>
    /// Извлечение идентификаторов таблицы.
    /// Таблица должна иметь простой или составной первичный ключ.
    /// Если таблица не имеет первичного ключа, используйте метод FromColumn() или FromColumns
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="table">Таблица, откуда извлекаются идентификаторы</param>
    /// <param name="saveOrder">Если true, то порядок идентификаторов в возвращаемом наборе должен соответствовать порядку строк в таблице 
    /// (должен возвращаться <see cref="IIndexedIdSet{T}"/>).
    /// Если false, то порядок идентификаторов не имеет значения и можно вернуть <see cref="IdCollection{T}"/>.</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIds(DataTable table, bool saveOrder)
    {
      Type idType = GetIdType(table); // Заодно проверяет корректность первичного ключа таблицы
      if (table.Rows.Count == 0)
        return GetEmptyArray(idType);

      DataTableValues src = new DataTableValues(table);
      IIdExtractor extractor = CreateExtractor(src, DBxColumns.FromColumns(table.PrimaryKey), idType);

      IIdSet idSet = CreateIdSet(idType, saveOrder);
      while (src.Read())
        extractor.AddToSet(idSet);

      return idSet;
    }

    /// <summary>
    /// Извлечение значений ссылочного поля таблицы.
    /// Перегрузка предназначена для простых идентификаторов, которые используют единственное ссылочное поле.
    /// Порядок идентификаторов будет соответствовать порядку строк в таблице.
    /// Поле может содержать значения <see cref="DBNull"/> или потворы, которые отбрасыааются.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="table">Таблица, откуда извлекаются идентификаторы</param>
    /// <param name="columnName">Имя ссылочного столбца в таблице <paramref name="table"/></param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIdsFromColumn(DataTable table, string columnName)
    {
      return GetIdsFromColumn(table, columnName, true);
    }

    /// <summary>
    /// Извлечение значений ссылочного поля таблицы.
    /// Перегрузка предназначена для простых идентификаторов, которые используют единственное ссылочное поле.
    /// Поле может содержать значения <see cref="DBNull"/> или потворы, которые отбрасыааются.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="table">Таблица, откуда извлекаются идентификаторы</param>
    /// <param name="columnName">Имя ссылочного столбца в таблице <paramref name="table"/></param>
    /// <param name="saveOrder">Если true, то порядок идентификаторов в возвращаемом наборе должен соответствовать порядку строк в таблице 
    /// (должен возвращаться <see cref="IIndexedIdSet{T}"/>).
    /// Если false, то порядок идентификаторов не имеет значения и можно вернуть <see cref="IdCollection{T}"/>.</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIdsFromColumn(DataTable table, string columnName, bool saveOrder)
    {
      Type idType = GetIdType(table, columnName);
      if (table.Rows.Count == 0)
        return GetEmptyArray(idType);

      DataTableValues src = new DataTableValues(table);
      IIdExtractor extractor = CreateExtractor(src, columnName, idType); // испр. 18.11.2025

      IIdSet idSet = CreateIdSet(idType, saveOrder);
      while (src.Read())
        extractor.AddToSet(idSet);

      return idSet;
    }

    /// <summary>
    /// Извлечение значений ссылочных полей таблицы.
    /// Перегрузка предназначена для составных идентификаторов, которые используют несколько ссылочных полей.
    /// Если хотя бы одно из полей, образующих ссылку, содержит значение <see cref="DBNull"/>, идентификатор считается пустым и пропускается.
    /// Если комбинация значений образует повторяюющийся идентикатор, он тоже пропускается.
    /// Порядок идентификаторов будет соответствовать порядку строк в таблице.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="table">Таблица, откуда извлекаются идентификаторы</param>
    /// <param name="columnNames">Имя столбцов в таблице <paramref name="table"/>, которые образуют ссылку</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIdsFromColumns(DataTable table, DBxColumns columnNames)
    {
      return GetIdsFromColumns(table, columnNames, true);
    }

    /// <summary>
    /// Извлечение значений ссылочных полей таблицы.
    /// Перегрузка предназначена для составных идентификаторов, которые используют несколько ссылочных полей.
    /// Если хотя бы одно из полей, образующих ссылку, содержит значение <see cref="DBNull"/>, идентификатор считается пустым и пропускается.
    /// Если комбинация значений образует повторяюющийся идентикатор, он тоже пропускается.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="table">Таблица, откуда извлекаются идентификаторы</param>
    /// <param name="columnNames">Имя столбцов в таблице <paramref name="table"/>, которые образуют ссылку</param>
    /// <param name="saveOrder">Если true, то порядок идентификаторов в возвращаемом наборе должен соответствовать порядку строк в таблице 
    /// (должен возвращаться <see cref="IIndexedIdSet{T}"/>).
    /// Если false, то порядок идентификаторов не имеет значения и можно вернуть <see cref="IdCollection{T}"/>.</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIdsFromColumns(DataTable table, DBxColumns columnNames, bool saveOrder)
    {
      Type idType = GetIdType(table, columnNames);
      if (table.Rows.Count == 0)
        return GetEmptyArray(idType);

      DataTableValues src = new DataTableValues(table);
      IIdExtractor extractor = CreateExtractor(src, columnNames, idType); // испр.18.11.2025

      IIdSet idSet = CreateIdSet(idType, saveOrder);
      while (src.Read())
        extractor.AddToSet(idSet);

      return idSet;
    }

    #endregion

    #region DataView

    /// <summary>
    /// Извлечение идентификаторов таблицы.
    /// Таблица должна иметь простой или составной первичный ключ.
    /// Если таблица не имеет первичного ключа, используйте метод <see cref="GetIdsFromColumn(DataView, string)"/> или <see cref="GetIdsFromColumns(DataView, DBxColumns)"/>.
    /// Порядок идентификаторов будет соответствовать порядку строк <see cref="DataView"/>.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// Порядок сортировки просмотра <see cref="DataView.Sort"/> не обязан совпадать с первичным ключом таблицы.
    /// </summary>
    /// <param name="dv">Просмотр для таблицы, откуда извлекаются идентификаторы</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIds(DataView dv)
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
#endif

      Type idType = GetIdType(dv.Table); // Заодно проверяет корректность первичного ключа таблицы
      if (dv.Count == 0)
        return GetEmptyArray(idType);

      DataViewValues src = new DataViewValues(dv);
      IIdExtractor extractor = CreateExtractor(src, DBxColumns.FromColumns(dv.Table.PrimaryKey), idType);

      IIdSet idSet = CreateIdList(idType);
      while (src.Read())
        extractor.AddToSet(idSet);

      return idSet;
    }

    /// <summary>
    /// Извлечение значений ссылочного поля таблицы.
    /// Перегрузка предназначена для простых идентификаторов, которые используют единственное ссылочное поле.
    /// Порядок идентификаторов будет соответствовать порядку строк <see cref="DataView"/>.
    /// Поле может содержать значения <see cref="DBNull"/> или потворы, которые отбрасыааются.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="dv">Просмотр для таблицы, откуда извлекаются идентификаторы</param>
    /// <param name="columnName">Имя ссылочного столбца в таблице <see cref="DataTable"/>.
    /// Не обязан совпадать с порядком сортировки <see cref="DataView.Sort"/>.</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIdsFromColumn(DataView dv, string columnName)
    {
      if (dv == null)
        throw new ArgumentNullException("dv");

      Type idType = GetIdType(dv.Table, columnName);
      if (dv.Count == 0)
        return GetEmptyArray(idType);

      DataViewValues src = new DataViewValues(dv);
      IIdExtractor extractor = CreateExtractor(src, columnName, idType);

      IIdSet idSet = CreateIdList(idType);
      while (src.Read())
        extractor.AddToSet(idSet);

      return idSet;
    }

    /// <summary>
    /// Извлечение значений ссылочных полей таблицы.
    /// Перегрузка предназначена для составных идентификаторов, которые используют несколько ссылочных полей.
    /// Если хотя бы одно из полей, образующих ссылку, содержит значение <see cref="DBNull"/>, идентификатор считается пустым и пропускается.
    /// Если комбинация значений образует повторяюющийся идентикатор, он тоже пропускается.
    /// Порядок идентификаторов будет соответствовать порядку строк <see cref="DataView"/>.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="dv">Просмотр для таблицы, откуда извлекаются идентификаторы</param>
    /// <param name="columnNames">Имя столбцов в таблице <see cref="DataTable"/>, которые образуют ссылку.
    /// Не обязаны совпадать с порядком сортировки <see cref="DataView.Sort"/>.</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIdsFromColumns(DataView dv, DBxColumns columnNames)
    {
      if (dv == null)
        throw new ArgumentNullException("dv");

      Type idType = GetIdType(dv.Table, columnNames);
      if (dv.Count == 0)
        return GetEmptyArray(idType);

      DataViewValues src = new DataViewValues(dv);
      IIdExtractor extractor = CreateExtractor(src, columnNames, idType);

      IIdSet idSet = CreateIdList(idType);
      while (src.Read())
        extractor.AddToSet(idSet);

      return idSet;
    }

    #endregion

    #region DataRow collection

    /// <summary>
    /// Извлечение идентификаторов таблицы.
    /// Таблица должна иметь простой или составной первичный ключ.
    /// Порядок идентификаторов будет соответствовать порядку строк в коллекции <paramref name="rows"/>.
    /// Если таблица не имеет первичного ключа, используйте метод <see cref="GetIdsFromColumn{T}(IEnumerable{DataRow}, string)"/> или <see cref="GetIdsFromColumns(IEnumerable{DataRow}, DBxColumns, DataTable)"/>.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="rows">Коллекция строк таблицы, откуда извлекаются идентификаторы</param>
    /// <param name="table">Ссылка на таблицу для определения первичного ключа и типа идентификаторов в наборе</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIds(IEnumerable<DataRow> rows, DataTable table)
    {
      Type idType = GetIdType(table); // Заодно проверяет корректность первичного ключа таблицы

      DataRowValues src = new DataRowValues();
      src.Table = table;
      IIdExtractor extractor = CreateExtractor(src, DBxColumns.FromColumns(table.PrimaryKey), idType);
      IIdSet idSet = null;
      foreach (DataRow row in rows)
      {
        src.CurrentRow = row;
        if (idSet == null)
          idSet = CreateIdSet(idType, true);
        extractor.AddToSet(idSet);
      }
      if (idSet == null)
        return GetEmptyArray(idType);
      else
        return idSet;
    }

    /// <summary>
    /// Извлечение значений ссылочного поля таблицы.
    /// Порядок идентификаторов будет соответствовать порядку строк в коллекции <paramref name="rows"/>.
    /// Перегрузка предназначена для простых идентификаторов, которые используют единственное ссылочное поле.
    /// Поле может содержать значения <see cref="DBNull"/> или потворы, которые отбрасыааются.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="rows">Коллекция строк таблицы, откуда извлекаются идентификаторы</param>
    /// <param name="columnName">Имя ссылочного столбца в таблице <paramref name="table"/></param>
    /// <param name="table">Ссылка на таблицу для определения первичного ключа и типа идентификаторов в наборе</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIdsFromColumn(IEnumerable<DataRow> rows, string columnName, DataTable table)
    {
      Type idType = GetIdType(table, columnName);

      DataRowValues src = new DataRowValues();
      src.Table = table;
      IIdExtractor extractor = CreateExtractor(src, columnName, idType);

      IIdSet idSet = null;
      foreach (DataRow row in rows)
      {
        src.CurrentRow = row;
        if (idSet == null)
          idSet = CreateIdSet(idType, true);
        extractor.AddToSet(idSet);
      }
      if (idSet == null)
        return GetEmptyArray(idType);
      else
        return idSet;
    }

    /// <summary>
    /// Извлечение значений составного ссылочного поля таблицы.
    /// Если хотя бы одно из полей, образующих ссылку, содержит значение <see cref="DBNull"/>, идентификатор считается пустым и пропускается.
    /// Если комбинация значений образует повторяюющийся идентикатор, он тоже пропускается.
    /// Порядок идентификаторов будет соответствовать порядку строк в коллекции <paramref name="rows"/>.
    /// Эту перегрузку следует использовать, когда тип идентификаторов неизвестен на этапе компиляции.
    /// </summary>
    /// <param name="rows">Коллекция строк таблицы, откуда извлекаются идентификаторы</param>
    /// <param name="columnNames">Имя столбцов в таблице <see cref="DataTable"/>, которые образуют ссылку</param>
    /// <param name="table">Ссылка на таблицу для определения первичного ключа и типа идентификаторов в наборе</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet GetIdsFromColumns(IEnumerable<DataRow> rows, DBxColumns columnNames, DataTable table)
    {
      Type idType = GetIdType(table, columnNames);

      DataRowValues src = new DataRowValues();
      src.Table = table;
      IIdExtractor extractor = CreateExtractor(src, columnNames, idType);

      IIdSet idSet = null;
      foreach (DataRow row in rows)
      {
        src.CurrentRow = row;
        if (idSet == null)
          idSet = CreateIdSet(idType, true);
        extractor.AddToSet(idSet);
      }
      if (idSet == null)
        return GetEmptyArray(idType);
      else
        return idSet;
    }

    #endregion

    #endregion

    #region Типизированные методы

    #region DataTable

    /// <summary>
    /// Извлечение идентификаторов таблицы.
    /// Таблица должна иметь простой или составной первичный ключ.
    /// Если таблица не имеет первичного ключа, используйте метод <see cref="GetIdsFromColumn(DataTable, string)"/> или <see cref="GetIdsFromColumns(DataTable, DBxColumns)"/>.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="table">Таблица, откуда извлекаются идентификаторы</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIndexedIdSet<T> GetIds<T>(DataTable table)
      where T : struct, IEquatable<T>
    {
      return (IIndexedIdSet<T>)GetIds<T>(table, true);
    }

    /// <summary>
    /// Извлечение идентификаторов таблицы.
    /// Таблица должна иметь простой или составной первичный ключ.
    /// Если таблица не имеет первичного ключа, используйте метод <see cref="GetIdsFromColumn(DataTable, string, bool)"/> или <see cref="GetIdsFromColumns(DataTable, DBxColumns, bool)"/>.
    /// Метод игнорирует аргумент <paramref name="saveOrder"/> и всегда возвращает <see cref="IdArray{T}"/>.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="table">Таблица, откуда извлекаются идентификаторы</param>
    /// <param name="saveOrder">Не используется</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet<T> GetIds<T>(DataTable table, bool saveOrder)
      where T : struct, IEquatable<T>
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif
      if (table.PrimaryKey.Length == 0)
        throw ExceptionFactory.ArgDataTableWithoutPrimaryKey("table", table);

      if (table.Rows.Count == 0)
        return IdArray<T>.Empty;

      DataTableValues src = new DataTableValues(table);
      IdExtractor<T> extractor = CreateExtractor<T>(src, DBxColumns.FromColumns(table.PrimaryKey));

      T[] a = new T[table.Rows.Count];
      for (int i = 0; i < table.Rows.Count; i++)
      {
        src.CurrentRow = table.Rows[i];
        a[i] = extractor.Id;
      }

      return IdArray<T>.FromArrayInternal(a);
    }

    /// <summary>
    /// Извлечение значений ссылочного поля таблицы.
    /// Поле может содержать значения <see cref="DBNull"/> или потворы, которые отбрасыааются.
    /// Порядок идентификаторов соответствует порядку строк в таблице.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="table">Таблица для извлечения значений</param>
    /// <param name="columnName">Имя ключевого или ссылочного поля</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIndexedIdSet<T> GetIdsFromColumn<T>(DataTable table, string columnName)
      where T : struct, IEquatable<T>
    {
      return (IIndexedIdSet<T>)GetIdsFromColumn<T>(table, columnName, true);
    }

    /// <summary>
    /// Извлечение значений ссылочного поля таблицы.
    /// Поле может содержать значения <see cref="DBNull"/> или потворы, которые отбрасыааются.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="table">Таблица для извлечения значений</param>
    /// <param name="columnName">Имя ключевого или ссылочного поля</param>
    /// <param name="saveOrder">Если true, то порядок идентификаторов в возвращаемом наборе должен соответствовать порядку строк в таблице 
    /// (должен возвращаться <see cref="IIndexedIdSet{T}"/>).
    /// Если false, то порядок идентификаторов не имеет значения и можно вернуть <see cref="IdCollection{T}"/>.</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIdSet<T> GetIdsFromColumn<T>(DataTable table, string columnName, bool saveOrder)
      where T : struct, IEquatable<T>
    {
      if (table.Rows.Count == 0)
        return IdArray<T>.Empty;

      DataTableValues src = new DataTableValues(table);
      IdExtractor<T> extractor = CreateExtractor<T>(src, columnName);

      IIdSet<T> idSet = CreateIdSet<T>(saveOrder);

      while (src.Read())
        extractor.AddToSet(idSet);

      return idSet;
    }

    /// <summary>
    /// Извлечение значений ссылочных полей таблицы.
    /// Порядок идентификаторов соответствует порядку строк в таблице.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="table">Таблица для извлечения значений</param>
    /// <param name="columnNames">Имена ссылочныъх полей</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIndexedIdSet<T> GetIdsFromColumns<T>(DataTable table, DBxColumns columnNames)
      where T : struct, IEquatable<T>
    {
      return (IIndexedIdSet<T>)GetIdsFromColumns<T>(table, columnNames, true);
    }

    /// <summary>
    /// Извлечение значений ссылочных полей таблицы.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="table">Таблица для извлечения значений</param>
    /// <param name="columnNames">Имена ссылочныъх полей</param>
    /// <param name="saveOrder">Если true, то порядок идентификаторов в возвращаемом наборе должен соответствовать порядку строк в таблице 
    /// (должен возвращаться <see cref="IIndexedIdSet{T}"/>).
    /// Если false, то порядок идентификаторов не имеет значения и можно вернуть <see cref="IdCollection{T}"/>.</param>
    /// <returns></returns>
    public static IIdSet<T> GetIdsFromColumns<T>(DataTable table, DBxColumns columnNames, bool saveOrder)
      where T : struct, IEquatable<T>
    {
      if (table.Rows.Count == 0)
        return IdArray<T>.Empty;

      DataTableValues src = new DataTableValues(table);
      IdExtractor<T> extractor = CreateExtractor<T>(src, columnNames);

      IIdSet<T> idSet = CreateIdSet<T>(saveOrder);

      while (src.Read())
        extractor.AddToSet(idSet);

      return idSet;
    }

    #endregion

    #region DataView

    /// <summary>
    /// Извлечение идентификаторов таблицы.
    /// Таблица должна иметь простой или составной первичный ключ.
    /// Если таблица не имеет первичного ключа, используйте метод <see cref="GetIdsFromColumn(DataView, string)"/> или <see cref="GetIdsFromColumns(DataView, DBxColumns)"/>.
    /// Порядок строк соответствует порядку строк в просмотре <see cref="DataView"/>.
    /// Порядок сортировки просмотра <see cref="DataView.Sort"/> не обязан совпадать с первичным ключом таблицы.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="dv">Набор данных</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIndexedIdSet<T> GetIds<T>(DataView dv)
      where T : struct, IEquatable<T>
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
#endif
      if (dv.Table.PrimaryKey.Length == 0)
        throw ExceptionFactory.ArgDataTableWithoutPrimaryKey("dv", dv.Table);

      if (dv.Count == 0)
        return IdArray<T>.Empty;

      DataViewValues src = new DataViewValues(dv);
      IdExtractor<T> extractor = CreateExtractor<T>(src, DBxColumns.FromColumns(dv.Table.PrimaryKey));

      T[] a = new T[dv.Count];
      for (int i = 0; i < dv.Count; i++)
      {
        src.CurrentRow = dv[i].Row;
        a[i] = extractor.Id;
      }

      return IdArray<T>.FromArrayInternal(a);
    }

    /// <summary>
    /// Извлечение значений ссылочного поля таблицы.
    /// Порядок строк соответствует порядку строк в просмотре <see cref="DataView"/>.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="dv">Набор данных</param>
    /// <param name="columnName">Имя столбца, содержащего идентификаторы.
    /// Не обязан совпадать с порядком сортировки <see cref="DataView.Sort"/>.</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIndexedIdSet<T> GetIdsFromColumn<T>(DataView dv, string columnName)
      where T : struct, IEquatable<T>
    {
      if (dv.Count == 0)
        return IdArray<T>.Empty;

      DataViewValues src = new DataViewValues(dv);
      IdExtractor<T> extractor = CreateExtractor<T>(src, columnName);

      IdList<T> idSet = new IdList<T>();

      while (src.Read())
        extractor.AddToSet(idSet);

      return idSet;
    }

    /// <summary>
    /// Извлечение значений ссылочных полей таблицы.
    /// Порядок строк соответствует порядку строк в просмотре <see cref="DataView"/>.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="dv">Набор данных</param>
    /// <param name="columnNames">Имена ссылочныъх полей.
    /// Не обязаны совпадать с порядком сортировки <see cref="DataView.Sort"/>.</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIndexedIdSet<T> GetIdsFromColumns<T>(DataView dv, DBxColumns columnNames)
      where T : struct, IEquatable<T>
    {
      if (dv.Count == 0)
        return IdArray<T>.Empty;

      DataViewValues src = new DataViewValues(dv);
      IdExtractor<T> extractor = CreateExtractor<T>(src, columnNames);

      IdList<T> idSet = new IdList<T>();

      while (src.Read())
        extractor.AddToSet(idSet);

      return idSet;
    }

    #endregion

    #region DataRow collection

    /// <summary>
    /// Извлечение идентификаторов таблицы.
    /// Таблица должна иметь простой или составной первичный ключ.
    /// Если таблица не имеет первичного ключа, используйте метод <see cref="GetIdsFromColumn(IEnumerable{DataRow}, string, DataTable)"/>.
    /// Порядок строк соответствует порядку строк в коллекции <paramref name="rows"/>.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="rows">Строки, из которых извлекаются идентификаторы</param>
    /// <param name="table">Описание таблицы, откуда берется описание первичного ключа</param>
    /// <returns>Набор идентификаторов</returns>
    private static IIndexedIdSet<T> GetIds<T>(IEnumerable<DataRow> rows, DataTable table)
      where T : struct, IEquatable<T>
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif
      if (table.PrimaryKey.Length == 0)
        throw ExceptionFactory.ArgDataTableWithoutPrimaryKey("table", table);

      DataRowValues src = new DataRowValues();
      src.Table = table;
      IdExtractor<T> extractor = CreateExtractor<T>(src, DBxColumns.FromColumns(table.PrimaryKey));

      IdList<T> idSet = null;
      foreach (DataRow row in rows)
      {
        src.CurrentRow = row;
        if (idSet == null)
          idSet = new IdList<T>();
        extractor.AddToSet(idSet);
      }
      if (idSet == null)
        return IdArray<T>.Empty;
      else
        return idSet;
    }

    /// <summary>
    /// Извлечение идентификаторов таблицы.
    /// Таблица должна иметь простой или составной первичный ключ.
    /// Если таблица не имеет первичного ключа, используйте метод <see cref="GetIdsFromColumn(IEnumerable{DataRow}, string, DataTable)"/>.
    /// Порядок строк соответствует порядку строк в коллекции <paramref name="rows"/>.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="rows">Строки, из которых извлекаются идентификаторы</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIndexedIdSet<T> GetIds<T>(IEnumerable<DataRow> rows)
      where T : struct, IEquatable<T>
    {
      int n = 0;
      DataTable table = null;
      foreach (DataRow row in rows)
      {
        if (table == null)
          table = row.Table;
        else if (row.Table != table)  // Если таблицы разные, нет гарантии уникальности первичного ключа
          return GetIds<T>(rows, table);

        n++;
      }
      if (n == 0)
        return IdArray<T>.Empty;

      if (table.PrimaryKey.Length == 0)
        throw ExceptionFactory.ArgDataTableWithoutPrimaryKey("rows", table);


      DataRowValues src = new DataRowValues();
      src.Table = table; // 18.11.2025
      IdExtractor<T> extractor = CreateExtractor<T>(src, DBxColumns.FromColumns(table.PrimaryKey));

      T[] a = new T[n];
      int cnt = 0;
      foreach (DataRow row in rows)
      {
        src.CurrentRow = row;
        a[cnt] = extractor.Id;
        cnt++;
      }
      return IdArray<T>.FromArrayInternal(a);
    }


    /// <summary>
    /// Извлечение значений ссылочного поля таблицы.
    /// Порядок строк соответствует порядку строк в коллекции <paramref name="rows"/>.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="rows">Строки, из которых извлекаются идентификаторы</param>
    /// <param name="columnName">Имя столбца, содержащего идентификаторы</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIndexedIdSet<T> GetIdsFromColumn<T>(IEnumerable<DataRow> rows, string columnName)
      where T : struct, IEquatable<T>
    {
      DataRowValues src = new DataRowValues();
      IdList<T> idSet = null;
      IdExtractor<T> extractor = null;
      foreach (DataRow row in rows)
      {
        src.CurrentRow = row;
        if (idSet == null)
        {
          idSet = new IdList<T>();
          extractor = CreateExtractor<T>(src, columnName);
        }
        extractor.AddToSet(idSet);
      }
      if (idSet == null)
        return IdArray<T>.Empty;
      else
        return idSet;
    }

    /// <summary>
    /// Извлечение значений ссылочных полей таблицы.
    /// Порядок строк соответствует порядку строк в коллекции <paramref name="rows"/>.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="rows">Строки, из которых извлекаются идентификаторы</param>
    /// <param name="columnNames">Имя столбцов, образующих ссылку</param>
    /// <returns>Набор идентификаторов</returns>
    public static IIndexedIdSet<T> GetIdsFromColumns<T>(IEnumerable<DataRow> rows, DBxColumns columnNames)
      where T : struct, IEquatable<T>
    {
      DataRowValues src = new DataRowValues();
      IdList<T> idSet = null;
      IdExtractor<T> extractor = null;
      foreach (DataRow row in rows)
      {
        src.CurrentRow = row;
        if (idSet == null)
        {
          idSet = new IdList<T>();
          extractor = CreateExtractor<T>(src, columnNames);
        }
        extractor.AddToSet(idSet);
      }
      if (idSet == null)
        return IdArray<T>.Empty;
      else
        return idSet;
    }

    #endregion

    #endregion

    #endregion

    #region Поиск строк в DataTable

    #region Вспомогательные методы

    private static object[] CreateKeyArray(Type type)
    {
      if (typeof(IComplexId).IsAssignableFrom(type))
        return new object[type.GetGenericArguments().Length];
      else
        return new object[1];
    }

    private static object PrepareKey(object id)
    {
      if (id is StringId)
        return ((StringId)id).Value;
      else
        return id;
    }

    #endregion

    #region Нетипизированные версии

    /// <summary>
    /// Получить массив строк <see cref="DataRow"/> из таблицы для массива идентификаторов для ключевого поля (или полей, если ключ составной)в таблице.
    /// Результирующий массив будет содержать значения null для ненайденных идентификаторов.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="ids">Массив идентификаторов</param>
    /// <returns>Массив строк</returns>
    public static DataRow[] FindRows(DataTable table, IIdSet ids)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      System.Collections.ICollection ids2 = (System.Collections.ICollection)ids;

      if (ids2.Count == 0)
        return EmptyArray<DataRow>.Empty;
      object[] keys = CreateKeyArray(ids.IdType);
      int cnt = 0;
      DataRow[] res = new DataRow[ids2.Count];
      foreach (object id in ids2)
      {
        if (keys.Length > 1)
        {
          for (int j = 0; j < keys.Length; j++)
            keys[j] = PrepareKey(((IComplexId)id)[j]);
        }
        else
          keys[0] = PrepareKey(id);
        res[cnt] = table.Rows.Find(keys);
        cnt++;
      }
      return res;
    }


    /// <summary>
    /// Получить массив строк <see cref="DataRow"/> из таблицы для массива идентификаторов в таблице.
    /// Результирующий массив будет содержать значения null для ненайденных идентификаторов.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя ссылочного поля</param>
    /// <param name="ids">Массив идентификаторов</param>
    /// <returns>Массив строк</returns>
    public static DataRow[] FindRowsFromColumn(DataTable table, string columnName, IIdSet ids)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      System.Collections.ICollection ids2 = (System.Collections.ICollection)ids;

      if (ids2.Count == 0)
        return EmptyArray<DataRow>.Empty;

      if (table.PrimaryKey.Length == 1 && String.Equals(table.PrimaryKey[0].ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
        return FindRows(table, ids);

      using (DataView dv = new DataView(table))
      {
        dv.Sort = columnName;

        DataRow[] res = new DataRow[ids2.Count];
        int cnt = 0;
        foreach (object id in ids2)
        {
          object key = PrepareKey(id);

          int p = dv.Find(key);
          if (p >= 0)
            res[cnt] = dv[p].Row;
          cnt++;
        }
        return res;
      }
    }

    /// <summary>
    /// Получить массив строк <see cref="DataRow"/> из таблицы для массива идентификаторов.
    /// Результирующий массив будет содержать значения null для ненайденных идентификаторов.
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <param name="columnNames">Имена полей, образующих ссылку</param>
    /// <param name="ids">Массив идентификаторов</param>
    /// <returns>Массив строк</returns>
    public static DataRow[] FindRowsFromColumns(DataTable table, DBxColumns columnNames, IIdSet ids)
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
#endif
      if (columnNames.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("columnNames");

      System.Collections.ICollection ids2 = (System.Collections.ICollection)ids;

      if (ids2.Count == 0)
        return EmptyArray<DataRow>.Empty;

      if (columnNames.Count == 1)
        return FindRowsFromColumn(table, columnNames[0], ids);

      DBxColumns cols2 = DBxColumns.FromColumns(table.PrimaryKey);
      if (DBxColumns.AreEqual(cols2, columnNames, true, false))
        return FindRows(table, ids);

      using (DataView dv = new DataView(table))
      {
        dv.Sort = columnNames.AsString;

        object[] keys = CreateKeyArray(ids.IdType);
        DataRow[] res = new DataRow[ids2.Count];
        int cnt = 0;
        foreach (object id in ids2)
        {
          for (int j = 0; j < keys.Length; j++)
            keys[j] = PrepareKey(((IComplexId)id)[j]);

          int p = dv.Find(keys);
          if (p >= 0)
            res[cnt] = dv[p].Row;
          cnt++;
        }
        return res;
      }
    }

    #endregion

    #region Типизированные версии

    /// <summary>
    /// Получить массив строк <see cref="DataRow"/> из таблицы для массива идентификаторов для ключевого поля (или полей, если ключ составной)в таблице.
    /// Результирующий массив будет содержать значения null для ненайденных идентификаторов.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="table">Таблица</param>
    /// <param name="ids">Массив идентификаторов</param>
    /// <returns>Массив строк</returns>
    public static DataRow[] FindRows<T>(DataTable table, IIdSet<T> ids)
      where T : struct, IEquatable<T>
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      if (ids.Count == 0)
        return EmptyArray<DataRow>.Empty;
      object[] keys = CreateKeyArray(typeof(T));
      DataRow[] res = new DataRow[ids.Count];
      int cnt = 0;
      foreach (T id in ids)
      {
        if (keys.Length > 1)
        {
          for (int j = 0; j < keys.Length; j++)
            keys[j] = PrepareKey(((IComplexId)id)[j]);
        }
        else
          keys[0] = PrepareKey(id);
        res[cnt] = table.Rows.Find(keys);
        cnt++;
      }
      return res;
    }

    /// <summary>
    /// Получить массив строк <see cref="DataRow"/> из таблицы для массива идентификаторов в таблице.
    /// Результирующий массив будет содержать значения null для ненайденных идентификаторов.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя ссылочного поля</param>
    /// <param name="ids">Массив идентификаторов</param>
    /// <returns>Массив строк</returns>
    public static DataRow[] FindRowsFromColumn<T>(DataTable table, string columnName, IIdSet<T> ids)
      where T : struct, IEquatable<T>
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");

      if (ids.Count == 0)
        return EmptyArray<DataRow>.Empty;

      if (table.PrimaryKey.Length == 1 && String.Equals(table.PrimaryKey[0].ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
        return FindRows(table, ids);

      using (DataView dv = new DataView(table))
      {
        dv.Sort = columnName;

        DataRow[] res = new DataRow[ids.Count];
        int cnt = 0;
        foreach (T id in ids)
        {
          object key = PrepareKey(id);

          int p = dv.Find(key);
          if (p >= 0)
            res[cnt] = dv[p].Row;
          cnt++;
        }
        return res;
      }
    }


    /// <summary>
    /// Получить массив строк <see cref="DataRow"/> из таблицы для массива идентификаторов.
    /// Результирующий массив будет содержать значения null для ненайденных идентификаторов.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="table">Таблица</param>
    /// <param name="columnNames">Имена полей, образующих ссылку</param>
    /// <param name="ids">Массив идентификаторов</param>
    /// <returns>Массив строк</returns>
    public static DataRow[] FindRowsFromColumns<T>(DataTable table, DBxColumns columnNames, IIdSet<T> ids)
      where T : struct, IEquatable<T>
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (columnNames == null)
        throw new ArgumentNullException("columnNames");
#endif
      if (columnNames.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("columnNames");

      if (ids.Count == 0)
        return EmptyArray<DataRow>.Empty;

      if (columnNames.Count == 1)
        return FindRowsFromColumn<T>(table, columnNames[0], ids);

      DBxColumns cols2 = DBxColumns.FromColumns(table.PrimaryKey);
      if (DBxColumns.AreEqual(cols2, columnNames, true, false))
        return FindRows(table, ids);

      using (DataView dv = new DataView(table))
      {
        dv.Sort = columnNames.AsString;

        object[] keys = CreateKeyArray(typeof(T));
        DataRow[] res = new DataRow[ids.Count];
        int cnt = 0;
        foreach (T id in ids)
        {
          for (int j = 0; j < keys.Length; j++)
            keys[j] = PrepareKey(((IComplexId)id)[j]);

          int p = dv.Find(keys);
          if (p >= 0)
            res[cnt] = dv[p].Row;
          cnt++;
        }
        return res;
      }
    }

    #endregion

    #endregion

    #region GetBlockedArray()
#if XXX
    /// <summary>
    /// Разделяет набор идентификаторов на блоки, содержащие не более чем <paramref name="n"/> элементов
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="source">Исходный набор идентификаторов</param>
    /// <param name="n">Количество элементов в блоке</param>
    /// <returns>Массив наборов идентификаторов</returns>
    public static IIdSet<T>[] GetBlockedArray<T>(IIdSet<T> source, int n)
      where T : struct, IEquatable<T>
    {
      // TODO: Переделать без использования промежутночного массива

      T[] a1 = source.ToArray();
      T[][] a2 = ArrayTools.GetBlockedArray<T>(a1, n);
      IIdSet<T>[] a3 = new IIdSet<T>[a2.Length];
      for (int i = 0; i < a2.Length; i++)
        a3[i] = IdArray<T>.FromArrayInternal(a2[i]);
      return a3;
    }
#endif
    #endregion

    #region GetBlockedIds()

    /// <summary>
    /// Получить массив идентификаторов для ключевого поля для ключевого или первого поля в таблице.
    /// Порядок полученных идентификаторов соответствует порядку строк в таблице.
    /// Возвращается массив наборов идентификаторов, в каждом из которых
    /// не больше <paramref name="n"/> элементов.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="table">Таблица</param>
    /// <param name="n"></param>
    /// <returns>Массив числовых идентификаторов</returns>
    public static IIndexedIdSet<T>[] GetBlockedIds<T>(DataTable table, int n)
      where T : struct, IEquatable<T>
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif
      if (n < 1)
        throw ExceptionFactory.ArgOutOfRange("n", n, 1, null);

      if (table.Rows.Count == 0)
        return EmptyArray<IdArray<T>>.Empty;

      DataTableValues src = new DataTableValues(table);
      IdExtractor<T> extractor = CreateExtractor<T>(src, DBxColumns.FromColumns(table.PrimaryKey));

      int nn = ((table.Rows.Count + (n - 1))) / n;
      IIndexedIdSet<T>[] res = new IIndexedIdSet<T>[nn];
      for (int i = 0; i < nn; i++)
        res[i] = new IdList<T>();

      for (int index = 0; index < table.Rows.Count; index++)
      {
        src.CurrentRow = table.Rows[index];
        int idx1 = index / n;
        extractor.AddToSet(res[idx1]);
      }

      return res;
    }

    /// <summary>
    /// Получить массив идентификаторов для ключевого поля в таблице для 
    /// строк, входящих в просмотр <see cref="DataView"/>.
    /// Порядок полученных идентификаторов соответствует порядку строк в просмотре.
    /// Возвращается массив наборов идентификаторов, в каждом из которых
    /// не больше <paramref name="n"/> элементов.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="dv">Просмотр <see cref="DataView"/></param>
    /// <param name="n">Количество строк в блоке</param>
    /// <returns>Массив числовых идентификаторов</returns>
    public static IIndexedIdSet<T>[] GetBlockedIds<T>(DataView dv, int n)
      where T : struct, IEquatable<T>
    {
#if DEBUG
      if (dv == null)
        throw new ArgumentNullException("dv");
#endif

      if (n < 1)
        throw ExceptionFactory.ArgOutOfRange("n", n, 1, null);
      if (dv.Count == 0)
        return EmptyArray<IIndexedIdSet<T>>.Empty;

      DataViewValues src = new DataViewValues(dv);
      IdExtractor<T> extractor = CreateExtractor<T>(src, DBxColumns.FromColumns(dv.Table.PrimaryKey));

      int nn = ((dv.Count + (n - 1))) / n;

      IIndexedIdSet<T>[] res = new IIndexedIdSet<T>[nn];
      for (int i = 0; i < nn; i++)
        res[i] = new IdList<T>();

      for (int index = 0; index < dv.Count; index++)
      {
        int idx1 = index / n;
        src.CurrentRow = dv[index].Row;
        extractor.AddToSet(res[idx1]);
      }
      return res;
    }

    /// <summary>
    /// Получение значений поля для ключевого поля в таблице из массива строк.
    /// Возвращается массив наборов идентификаторов, в каждом из которых
    /// не больше <paramref name="n"/> элементов.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="rows">Массив строк</param>
    /// <param name="n">Количество элементов в результирующих массивах</param>
    /// <returns>Массив идентификаторов</returns>
    public static IIndexedIdSet<T>[] GetBlockedIds<T>(IEnumerable<DataRow> rows, int n)
      where T : struct, IEquatable<T>
    {
#if DEBUG
      if (rows == null)
        throw new ArgumentNullException("rows");
#endif
      if (n < 1)
        throw ExceptionFactory.ArgOutOfRange("n", n, 1, null);

      IdList<T> currList = null;
      List<IIndexedIdSet<T>> lstRes = null;

      DataRowValues src = null;
      IdExtractor<T> extractor = null;

      foreach (DataRow row in rows)
      {
        if (lstRes == null)
        {
          lstRes = new List<IIndexedIdSet<T>>();
          src = new DataRowValues();
        }
        src.CurrentRow = row;
        if (extractor == null)
          extractor = CreateExtractor<T>(src, DBxColumns.FromColumns(row.Table.PrimaryKey));

        if (currList == null)
          currList = new IdList<T>();
        extractor.AddToSet(currList);

        if (currList.Count == n)
        {
          lstRes.Add(currList);
          currList = null;
        }
      }

      if (currList != null)
        lstRes.Add(currList);

      if (lstRes == null)
        return EmptyArray<IIndexedIdSet<T>>.Empty;
      else
        return lstRes.ToArray();
    }

    /// <summary>
    /// Получение значений поля для ключевого поля в таблице из массива строк.
    /// Возвращается массив наборов идентификаторов, в каждом из которых
    /// не больше <paramref name="n"/> элементов.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="source">Исходный массив идентификаторов</param>
    /// <param name="n">Количество элементов в результирующих массивах</param>
    /// <returns>Массив идентификаторов</returns>
    public static IIndexedIdSet<T>[] GetBlockedIds<T>(IEnumerable<T> source, int n)
      where T : struct, IEquatable<T>
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif
      if (n < 1)
        throw ExceptionFactory.ArgOutOfRange("n", n, 1, null);

      IdList<T> currList = null;
      List<IIndexedIdSet<T>> lstRes = null;

      foreach (T id in source)
      {
        if (lstRes == null)
          lstRes = new List<IIndexedIdSet<T>>();

        if (currList == null)
          currList = new IdList<T>();
        currList.Add(id);

        if (currList.Count == n)
        {
          lstRes.Add(currList);
          currList = null;
        }
      }

      if (currList != null)
        lstRes.Add(currList);

      if (lstRes == null)
        return EmptyArray<IIndexedIdSet<T>>.Empty;
      else
        return lstRes.ToArray();
    }

    #endregion

    #region CloneTableForSelectedIds()

    /// <summary>
    /// Создать копию таблицы <paramref name="table"/>, содержащую строки с идентификаторами <paramref name="ids"/>.
    /// Создается копия таблицы, даже если исходная таблица уже содержит все строки
    /// в нужном порядке.
    /// Исходная таблица должна быть проиндексирована по числовому полю.
    /// если какой-либо ключ из массива <paramref name="ids"/> не будет найден в таблице, то будет 
    /// сгенерировано исключение.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="table">Исходная таблица</param>
    /// <param name="ids">Массив значений ключаевого поля. Не должен содержать
    /// повторяющихся значений</param>
    /// <returns>Копия таблицы, содержащая <paramref name="ids"/>.Length строк</returns>
    public static DataTable CloneTableForSelectedIds<T>(DataTable table, IIdSet<T> ids)
      where T : struct, IEquatable<T>
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (ids == null)
        throw new ArgumentNullException("ids");
#endif

      object[] keys = CreateKeyArray(typeof(T));

      DataTable table2 = table.Clone();
      foreach (T id in ids)
      {
        if (keys.Length > 1)
        {
          for (int j = 0; j < keys.Length; j++)
            keys[j] = PrepareKey(((IComplexId)id)[j]);
        }
        else
          keys[0] = PrepareKey(id);

        DataRow row1 = table.Rows.Find(keys);
        if (row1 == null)
          throw ExceptionFactory.DataRowNotFound(table, new object[1] { id });
        table2.Rows.Add(row1.ItemArray);
      }
      return table2;
    }

    /// <summary>
    /// Создать копию таблицы <paramref name="table"/>, содержащую строки с идентификаторами <paramref name="ids"/>.
    /// Если исходная таблица уже содержит все строки в нужном порядке, то копирование
    /// не выполняется, а возвращается исходная таблица.
    /// Исходная таблица должна быть проиндексирована по числовому полю.
    /// если какой-либо ключ из массива <paramref name="ids"/> не будет найден в таблице, то будет 
    /// сгенерировано исключение.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="table">Исходная таблица</param>
    /// <param name="ids">Массив значений ключаевого поля. Не должен содержать
    /// повторяющихся значений</param>
    /// <returns>Копия таблицы, содержащая Values.Length строк</returns>
    public static DataTable CloneOrSameTableForSelectedIds<T>(DataTable table, IIdSet<T> ids)
      where T : struct, IEquatable<T>
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
      if (ids == null)
        throw new ArgumentNullException("ids");
#endif

      // Сначала пытаемся проверить, не подойдет ли исходная таблица
      if (table.Rows.Count == ids.Count)
      {
        DataTableValues tabValues = new DataTableValues(table);
        IdExtractor<T> extractor = CreateExtractor<T>(tabValues, DBxColumns.FromColumns(table.PrimaryKey));

        if (ids.Count == 0)
          return table; // пустая таблица. Extractor все равно нужен для проверки наличия первичного ключа

        IIdSet<T> ids2 = ids;
        if (ids is IdArray<T>)
          ids2 = new IdCollection<T>(ids); // для быстрого поиска

        bool isGood = true;
        while (tabValues.Read())
        {
          if (!ids2.Contains(extractor.Id))
          {
            isGood = false;
            break;
          }
        }

        if (isGood)
          return table;
      }

      // Возвращаем копию таблицы
      return CloneTableForSelectedIds(table, ids);
    }

    #endregion

    #region GetRandomId()

    private interface IRandomGenerator<T>
      where T : struct, IEquatable<T>
    {
      T Next();
    }

    private static readonly Dictionary<Type, object> _RandomGenerators = CreateRandomGenerators();

    private class Int32RandomGenerator : IRandomGenerator<Int32>
    {
      public Int32 Next()
      {
        while (true)
        {
          Int32 id = -DataTools.TheRandom.Next(); // отрицательные значения для наглядности
          if (id != 0)
            return id;
        }
      }
    }

    private class Int64RandomGenerator : IRandomGenerator<Int64>
    {
      public Int64 Next()
      {
        // TODO: Сделать Int64
        while (true)
        {
          Int32 id = -DataTools.TheRandom.Next(); // отрицательные значения для наглядности
          if (id != 0)
            return id;
        }
      }
    }

    private class GuidRandomGenerator : IRandomGenerator<Guid>
    {
      public Guid Next()
      {
        return Guid.NewGuid();
      }
    }

    private class StringRandomGenerator : IRandomGenerator<StringId>
    {
      public StringId Next()
      {
        return Guid.NewGuid().ToString("D");
      }
    }

    private static Dictionary<Type, object> CreateRandomGenerators()
    {
      Dictionary<Type, object> dict = new Dictionary<Type, object>();
      dict.Add(typeof(Int32), new Int32RandomGenerator());
      dict.Add(typeof(Int64), new Int64RandomGenerator());
      dict.Add(typeof(Guid), new GuidRandomGenerator());
      dict.Add(typeof(StringId), new StringRandomGenerator());
      return dict;
    }

    /// <summary>
    /// Возвращает случайное значение для ключевого поля
    /// В текущей реализации не реализованы комплексные идентификаторы
    /// </summary>
    /// <returns>Идентификатор для новой строки</returns>
    public static T GetRandomId<T>()
      where T : struct, IEquatable<T>
    {
      object obj;
      if (!_RandomGenerators.TryGetValue(typeof(T), out obj))
      {
        throw new NotImplementedException();
      }
      IRandomGenerator<T> gen = (IRandomGenerator<T>)obj;
      return gen.Next();
    }

    /// <summary>
    /// Возвращает значение ключевого поля, которого нет в таблице данных.
    /// Таблица должна иметь первичный ключ 
    /// </summary>
    /// <param name="table">Таблица</param>
    /// <returns>Идентификатор для новой строки</returns>
    public static T GetRandomId<T>(DataTable table)
      where T : struct, IEquatable<T>
    {
#if DEBUG
      if (table == null)
        throw new ArgumentNullException("table");
#endif

      object[] keys = CreateKeyArray(typeof(T));

      lock (DataTools.TheRandom)
      {
        while (true)
        {
          T id = GetRandomId<T>();
          if (keys.Length > 1)
          {
            for (int j = 0; j < keys.Length; j++)
              keys[j] = PrepareKey(((IComplexId)id)[j]);
          }
          else
            keys[0] = PrepareKey(id);

          if (table.Rows.Find(keys) == null)
            return id;
        }
      }
    }

    #endregion

    #region TableFromIds()

    /// <summary>
    /// Создает таблицу, содержащую единственный столбец c указанным именем
    /// </summary>
    /// <param name="ids">Массив идентификаторов. 
    /// Если null, то будет возвращена пустая таблица.
    /// Значения 0 пропускаются, но повторы в текущей реализации не отбрасываются.</param>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Таблица со столбцом <paramref name="columnName"/></returns>
    public static DataTable TableFromIds<T>(IEnumerable<T> ids, string columnName)
      where T : struct, IEquatable<T>
    {
      Type[] idTypes = GetDataTypes(typeof(T));
      if (idTypes.Length != 1)
        throw new InvalidOperationException("Complex id are not supported");

      DataTable table = new DataTable();
      table.Columns.Add(columnName, idTypes[0]);
      if (ids != null)
      {
        foreach (T id in ids)
        {
          if (!id.Equals(default(T)))
            table.Rows.Add(PrepareKey(id));
        }
      }
      return table;
    }

    #endregion

    #region Чтение и запись в секцию конфигурации

    /// <summary>
    /// Записать идентификатор в секцию конфигурации
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="cfg">Секция конфигурации</param>
    /// <param name="name">Имя</param>
    /// <param name="value">Идентификатор</param>
    public static void WriteId<T>(CfgPart cfg, string name, T value)
      where T : struct, IEquatable<T>
    {
      WriteId(cfg, name, (object)value, null);
    }

    /// <summary>
    /// Записать идентификатор в секцию конфигурации.
    /// Перегрузка для составного идентификатора.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="cfg">Секция конфигурации</param>
    /// <param name="name">Имя</param>
    /// <param name="value">Идентификатор</param>
    /// <param name="complexNames">Имена параметров вложенной секции для хранения составного идентификатора</param>
    public static void WriteId<T>(CfgPart cfg, string name, T value, string[] complexNames)
      where T : struct, IEquatable<T>
    {
      WriteId(cfg, name, (object)value, complexNames);
    }

    /// <summary>
    /// Записать идентификатор в секцию конфигурации.
    /// Нетипизированная версия.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    /// <param name="name">Имя</param>
    /// <param name="value">Идентификатор</param>
    /// <param name="complexNames">Имена параметров вложенной секции для хранения составного идентификатора</param>
    public static void WriteId(CfgPart cfg, string name, object value, string[] complexNames)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
      if (String.IsNullOrEmpty(name))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("name");
#endif

      if (DataTools.IsEmptyValue(value))
      {
        cfg.Remove(name);
        return;
      }

      if (value is IComplexId)
      {
        IComplexId value2 = (IComplexId)value;
        CfgPart cfg2 = cfg.GetChild(name, true);
        PrepareComplexNames(ref complexNames, value2.Count);
        for (int i = 0; i < value2.Count; i++)
        {
          WriteId(cfg2, complexNames[i], value2[i], null);
        }
      }
      else
      {
        if (complexNames != null)
          throw ExceptionFactory.ArgUnknownValue("complexNames", complexNames, new object[1] { null });
        if (value is Int64)
          cfg.SetInt64(name, (Int64)value);
        else if (value is Int32)
          cfg.SetInt32(name, (Int32)value);
        else if (value is Guid)
          cfg.SetGuid(name, (Guid)value);
        else if (value is StringId)
          cfg.SetString(name, ((StringId)value).Value);
        else
          throw ExceptionFactory.ArgUnknownType("value", value);
      }
    }

    private static readonly string[] _ComplexNames2 = new string[] { "Id1", "Id2" };
    private static readonly string[] _ComplexNames3 = new string[] { "Id1", "Id2", "Id3" };

    private static void PrepareComplexNames(ref string[] complexNames, int count)
    {
      if (complexNames == null)
      {
        switch (count)
        {
          case 2: complexNames = _ComplexNames2; return;
          case 3: complexNames = _ComplexNames3; return;
          default:
            throw ExceptionFactory.ArgOutOfRange("count", count, 2, 3);
        }
      }
      if (complexNames.Length != 2)
        throw ExceptionFactory.ArgWrongCollectionCount("complexNames", complexNames, count);
      foreach (string name in complexNames)
      {
        if (String.IsNullOrEmpty(name))
          throw ExceptionFactory.ArgInvalidEnumerableItem("complexNames", complexNames, name);
      }
    }

    /// <summary>
    /// Прочитать идентификатор из секции конфигурации.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="cfg">Секция конфигурации</param>
    /// <param name="name">Имя</param>
    /// <returns>Идентификатор</returns>
    public static T ReaId<T>(CfgPart cfg, string name)
      where T : struct, IEquatable<T>
    {
      return ReadId<T>(cfg, name, null);
    }

    /// <summary>
    /// Прочитать идентификатор из секции конфигурации.
    /// Перегрузка для составного идентификатора.
    /// </summary>
    /// <typeparam name="T">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</typeparam>
    /// <param name="cfg">Секция конфигурации</param>
    /// <param name="name">Имя</param>
    /// <param name="complexNames">Имена параметров вложенной секции для хранения составного идентификатора</param>
    /// <returns>Идентификатор</returns>
    public static T ReadId<T>(CfgPart cfg, string name, string[] complexNames)
      where T : struct, IEquatable<T>
    {
      object res = ReadId(cfg, name, typeof(T), complexNames);
      if (res == null)
        return default(T);
      else
        return (T)res;
    }

    /// <summary>
    /// Прочитать идентификатор из секции конфигурации.
    /// Нетипизированная версия.
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    /// <param name="name">Имя</param>
    /// <param name="idType">Тип идентификатора. Список допустимых типов см. в <see cref="IIdSet{T}"/>.</param>
    /// <param name="complexNames">Имена параметров вложенной секции для хранения составного идентификатора</param>
    /// <returns>Идентификатор</returns>
    public static object ReadId(CfgPart cfg, string name, Type idType, string[] complexNames)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
      if (String.IsNullOrEmpty(name))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("name");
      if (idType == null)
        throw new ArgumentNullException("idType");
#endif

      if (idType.IsAssignableFrom(typeof(IComplexId)))
      {
        CfgPart cfg2 = cfg.GetChild(name, false);
        if (cfg2 == null)
          return null;

        Type genType = idType.GetGenericTypeDefinition();
        Type[] partTypes = idType.GetGenericArguments();
        PrepareComplexNames(ref complexNames, partTypes.Length);

        object[] args = new object[partTypes.Length];
        for (int i = 0; i < args.Length; i++)
        {
          args[i] = ReadId(cfg2, complexNames[i], partTypes[i], null);
          if (args[i] == null)
            return null;
        }
        ConstructorInfo ci = idType.GetConstructor(partTypes);
        if (ci == null)
          throw new BugException(String.Format("Constructor not found for the type {0}", idType.ToString()));
        return ci.Invoke(args);
      }
      else
      {
        if (idType == typeof(Int32))
          return cfg.GetNullableInt32(name);
        if (idType == typeof(Int64))
          return cfg.GetNullableInt64(name);
        if (idType == typeof(Guid))
          return cfg.GetNullableGuid(name);
        if (idType == typeof(StringId))
        {
          string s = cfg.GetString(name);
          if (String.IsNullOrEmpty(s))
            return null;
          else
            return new StringId(s);
        }
        throw ExceptionFactory.ArgUnknownValue("idType", idType);
      }
    }

    #endregion
  }
}
