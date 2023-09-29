using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;

namespace FreeLibSet.Config
{
  #region Перечисление SettingsPart

  /// <summary>
  /// Секции конфигурации для хранения параметров
  /// </summary>
  [Flags]
  public enum SettingsPart
  {
    /// <summary>
    /// Основная секция данных, привязанная к пользователю
    /// </summary>
    User = 0x1,

    /// <summary>
    /// Здесь должна храниться дата или период, за который строится отчет.
    /// Такие данные не хранятся в наборах истории или пользовательских наборах,
    /// а сохраняется всегда последнее значение
    /// </summary>
    NoHistory = 0x2,

    /// <summary>
    /// Здесь должны храниться ссылки на файлы и каталоги, размещенные на компьютере пользователя.
    /// Такие данные привязываются к пользователю и компьюьеру. Для приложений, работающих локально
    /// использование этой секции не имеет смысла. Для сетевых приложений это важно, если пользователь
    /// может входить с разных компьютеров, а настройки пользователя хранятся в базе данных
    /// </summary>
    Machine = 0x4,
  }

  #endregion

  /// <summary>
  /// Интерфейс объекта, выступающего в качестве хранилиза данных, которые можно записать и прочитать из секции конфигурации
  /// </summary>
  public interface ISettingsDataItem /*: IObjectWithCode*/
  {
    // Нет смысла использовать коды и NamedList, так как не может быть двух однотипных объектов в одном SettingsDataList

    /// <summary>
    /// Возвращает флаги мест хранения данных, используемых этим набором.
    /// Может быть возвращена комбинация из нескольких флагов.
    /// </summary>
    SettingsPart UsedParts { get; }

    /// <summary>
    /// Записать данные в секцию конфигурации.
    /// Метод вызыывается только для флагов, возвращаемых <see cref="UsedParts"/>.
    /// </summary>
    /// <param name="cfg">Записываемая секция</param>
    /// <param name="part">Вариант хранения данных. За один вызов может быть задан только один флаг.</param>
    void WriteConfig(CfgPart cfg, SettingsPart part);

    /// <summary>
    /// Прочитать данные из секции конфигурации.
    /// Метод вызыывается только для флагов, возвращаемых <see cref="UsedParts"/>.
    /// </summary>
    /// <param name="cfg">Секция с данными</param>
    /// <param name="part">Вариант хранения данных. За один вызов может быть задан только один флаг.</param>
    void ReadConfig(CfgPart cfg, SettingsPart part);
  }

  /// <summary>
  /// Абстрактная реализация интерфейса <see cref="ISettingsDataItem"/>
  /// </summary>
  public abstract class SettingsDataItem : ISettingsDataItem
  {
    /// <summary>
    /// Непереопределенное свойство возвращает <see cref="SettingsPart.User"/>
    /// </summary>
    public virtual SettingsPart UsedParts { get { return SettingsPart.User; } }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="part"></param>
    public abstract void ReadConfig(CfgPart cfg, SettingsPart part);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="part"></param>
    public abstract void WriteConfig(CfgPart cfg, SettingsPart part);
  }

  /// <summary>
  /// Объект-заглушка для реализации интерфейса <see cref="ISettingsDataItem"/>
  /// </summary>
  public sealed class DummySettingsDataItem : SettingsDataItem
  {
    /// <summary>
    /// Возвращает 0
    /// </summary>
    public override SettingsPart UsedParts { get { return (SettingsPart)0; } }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="part"></param>
    public override void WriteConfig(CfgPart cfg, SettingsPart part)
    {
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="cfg"></param>
    /// <param name="part"></param>
    public override void ReadConfig(CfgPart cfg, SettingsPart part)
    {
    }
  }

  /// <summary>
  /// Коллекция наборов данных <see cref="ISettingsDataItem"/>.
  /// В коллекции могут присутствовать только объекты разных классов.
  /// </summary>
  public class SettingsDataList : ICollection<ISettingsDataItem>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public SettingsDataList()
    {
      _Items = new List<ISettingsDataItem>();
    }

    #endregion

    #region ICollection

    private List<ISettingsDataItem> _Items;

    /// <summary>
    /// Возвращает количество элементов
    /// </summary>
    public int Count { get { return _Items.Count; } }

    /// <summary>
    /// Добавляет объект в список.
    /// </summary>
    /// <param name="item">Добавляемый объект</param>
    public void Add(ISettingsDataItem item)
    {
      if (item == null)
        throw new ArgumentNullException();
      foreach (ISettingsDataItem item2 in _Items)
      {
        if (item2.GetType() == item.GetType())
          throw new ArgumentException("В списке уже есть данные такого типа");
      }
      _Items.Add(item);
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      _Items.Clear();
    }

    bool ICollection<ISettingsDataItem>.Contains(ISettingsDataItem item)
    {
      return _Items.Contains(item);
    }

    /// <summary>
    /// Копирует список в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Индекс в <paramref name="array"/></param>
    public void CopyTo(ISettingsDataItem[] array, int arrayIndex)
    {
      _Items.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Создает массив со всеми объектами
    /// </summary>
    /// <returns>Новый массив</returns>
    public ISettingsDataItem[] ToArray()
    {
      ISettingsDataItem[] a = new ISettingsDataItem[_Items.Count];
      _Items.CopyTo(a, 0);
      return a;
    }

    bool ICollection<ISettingsDataItem>.Remove(ISettingsDataItem item)
    {
      return _Items.Remove(item);
    }

    /// <summary>
    /// Создает перечислитель по всем элементам списка
    /// </summary>
    /// <returns></returns>
    public List<ISettingsDataItem>.Enumerator GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator<ISettingsDataItem> IEnumerable<ISettingsDataItem>.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    bool ICollection<ISettingsDataItem>.IsReadOnly
    {
      get
      {
        throw new NotImplementedException();
      }
    }
    #endregion

    #region Методы

    /// <summary>
    /// Возвращает все флаги мест размещения данных, используемых хранящимися объектами
    /// </summary>
    public SettingsPart UsedParts
    {
      get
      {
        SettingsPart res = 0;
        for (int i = 0; i < _Items.Count; i++)
          res |= _Items[i].UsedParts;
        return res;
      }
    }

    private static readonly SettingsPart[] _AllParts = new SettingsPart[] { SettingsPart.User, SettingsPart.Machine, SettingsPart.NoHistory };
    private const SettingsPart AllPartValue = SettingsPart.User | SettingsPart.Machine | SettingsPart.NoHistory;

    /// <summary>
    /// Записывает данные в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Записываемая секция</param>
    /// <param name="part">Места хранения данных. В отличие от <see cref="ISettingsDataItem.WriteConfig(CfgPart, SettingsPart)"/>,
    /// допускается задавать комбинацию из нескольких флагов.</param>
    public void WriteConfig(CfgPart cfg, SettingsPart part)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        for (int j = 0; j < _AllParts.Length; j++)
        {
          if ((part & _AllParts[j]) != 0 && (_Items[i].UsedParts & _AllParts[j]) != 0)
            _Items[i].WriteConfig(cfg, _AllParts[j]);
        }
      }
    }

    /// <summary>
    /// Читает данные из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция для чтения</param>
    /// <param name="part">Места хранения данных. В отличие от <see cref="ISettingsDataItem.WriteConfig(CfgPart, SettingsPart)"/>,
    /// допускается задавать комбинацию из нескольких флагов.</param>
    public void ReadConfig(CfgPart cfg, SettingsPart part)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        for (int j = 0; j < _AllParts.Length; j++)
        {
          if ((part & _AllParts[j]) != 0 && (_Items[i].UsedParts & _AllParts[j]) != 0)
            _Items[i].ReadConfig(cfg, _AllParts[j]);
        }
      }
    }

    /// <summary>
    /// Записывает данные в секцию конфигурации без разделения на части
    /// </summary>
    /// <param name="cfg">Записываемая секция</param>
    public void WriteConfig(CfgPart cfg)
    {
      WriteConfig(cfg, AllPartValue);
    }

    /// <summary>
    /// Читает данные из секции конфигурации без разбиения на части
    /// </summary>
    /// <param name="cfg">Секция для чтения</param>
    public void ReadConfig(CfgPart cfg)
    {
      ReadConfig(cfg, AllPartValue);
    }

    /// <summary>
    /// Возвращает из списка объект заданного типа.
    /// Возвращает null, если объект не найден
    /// </summary>
    /// <typeparam name="T">Тип объекта в списке</typeparam>
    /// <returns>Объект</returns>
    public T GetItem<T>()
      where T : class, ISettingsDataItem
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        T item = _Items[i] as T;
        if (item != null)
          return item;
      }
      return null;
    }

    /// <summary>
    /// Возвращает из списка объект заданного типа.
    /// Выбрасывает исключение, если объект не найден
    /// </summary>
    /// <typeparam name="T">Тип объекта в списке</typeparam>
    /// <returns>Объект</returns>
    public T GetRequired<T>()
      where T : class, ISettingsDataItem
    {
      T res = GetItem<T>();
      if (res == null)
        throw new InvalidOperationException("Набор данных не содержит объекта класса " + typeof(T).Name);
      return res;
    }

    /// <summary>
    /// Удаляет из списка объект заданного типа
    /// </summary>
    /// <typeparam name="T">Тип удаляемого объекта</typeparam>
    /// <returns>True, если объект найден и удален</returns>
    public bool Remove<T>()
      where T : class, ISettingsDataItem
    {
      T item = GetItem<T>();
      if (item == null)
        return false;
      else
        return _Items.Remove(item); // должно вернуть true
    }

    #endregion
  }
}
