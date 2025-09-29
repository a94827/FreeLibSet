using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Core;

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
  /// Базовый класс объекта, выступающего в качестве хранилища данных, которые можно записать и прочитать из секции конфигурации.
  /// Объекты используются как для хранения текущих настроек в составе <see cref="SettingsDataList"/>, так и в наборе именных настроек по умолчанию <see cref="DefaultSettingsDataList"/>
  /// </summary>
  public abstract class SettingsDataItem
  {
    /// <summary>
    /// Возвращает флаги мест хранения данных, используемых этим набором.
    /// Может быть возвращена комбинация из нескольких флагов.
    /// Непереопределенное свойство возвращает <see cref="SettingsPart.User"/>
    /// </summary>
    public virtual SettingsPart UsedParts { get { return SettingsPart.User; } }

    /// <summary>
    /// Записать данные в секцию конфигурации.
    /// Метод вызывается только для флагов, возвращаемых <see cref="UsedParts"/>.
    /// </summary>
    /// <param name="cfg">Записываемая секция</param>
    /// <param name="part">Вариант хранения данных. За один вызов может быть задан только один флаг.</param>
    public abstract void WriteConfig(CfgPart cfg, SettingsPart part);

    /// <summary>
    /// Прочитать данные из секции конфигурации.
    /// Метод вызывается только для флагов, возвращаемых <see cref="UsedParts"/>.
    /// </summary>
    /// <param name="cfg">Секция с данными</param>
    /// <param name="part">Вариант хранения данных. За один вызов может быть задан только один флаг.</param>
    public abstract void ReadConfig(CfgPart cfg, SettingsPart part);

    internal static readonly SettingsPart[] AllParts = new SettingsPart[] { SettingsPart.User, SettingsPart.Machine, SettingsPart.NoHistory };
    internal const SettingsPart AllPartValue = SettingsPart.User | SettingsPart.Machine | SettingsPart.NoHistory;

    /// <summary>
    /// Записывает данные для всех частей, возвращаемых свойством <see cref="UsedParts"/>, в одну секцию конфигурации <paramref name="cfg"/>.
    /// </summary>
    /// <param name="cfg">Записываемая секция</param>
    public void WriteConfig(CfgPart cfg)
    {
      for (int j = 0; j < AllParts.Length; j++)
      {
        if ((UsedParts & AllParts[j]) != 0)
          WriteConfig(cfg, AllParts[j]);
      }
    }

    /// <summary>
    /// Записывает данные для всех частей, возвращаемых свойством <see cref="UsedParts"/>, в одну секцию конфигурации <paramref name="cfg"/>.
    /// </summary>
    /// <param name="cfg">Секция с данными</param>
    public void ReadConfig(CfgPart cfg)
    {
      for (int j = 0; j < AllParts.Length; j++)
      {
        if ((UsedParts & AllParts[j]) != 0)
          ReadConfig(cfg, AllParts[j]);
      }
    }
  }

  /// <summary>
  /// Объект-заглушка реализации <see cref="SettingsDataItem"/>, которая не хранит никаких данных
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
  /// Базовый класс для <see cref="SettingsDataList"/> и <see cref="DefaultSettingsDataList"/>
  /// </summary>
  public abstract class SettingsDataListBase : ICollection<SettingsDataItem>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public SettingsDataListBase()
    {
      _Items = new List<SettingsDataItem>();
    }

    #endregion

    #region ICollection

    private readonly List<SettingsDataItem> _Items;

    /// <summary>
    /// Возвращает количество элементов
    /// </summary>
    public int Count { get { return _Items.Count; } }

    /// <summary>
    /// Добавляет объект в список.
    /// </summary>
    /// <param name="item">Добавляемый объект</param>
    public void Add(SettingsDataItem item)
    {
      if (item == null)
        throw new ArgumentNullException();
      foreach (SettingsDataItem item2 in _Items)
      {
        if (item2.GetType() == item.GetType())
          throw new ArgumentException(String.Format(Res.SettingsDataList_Arg_ItemTypeAlreadyExists, item.GetType()), "item");
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

    bool ICollection<SettingsDataItem>.Contains(SettingsDataItem item)
    {
      return _Items.Contains(item);
    }

    /// <summary>
    /// Копирует список в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Индекс в <paramref name="array"/></param>
    public void CopyTo(SettingsDataItem[] array, int arrayIndex)
    {
      _Items.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Создает массив со всеми объектами
    /// </summary>
    /// <returns>Новый массив</returns>
    public SettingsDataItem[] ToArray()
    {
      SettingsDataItem[] a = new SettingsDataItem[_Items.Count];
      _Items.CopyTo(a, 0);
      return a;
    }

    bool ICollection<SettingsDataItem>.Remove(SettingsDataItem item)
    {
      return _Items.Remove(item);
    }

    /// <summary>
    /// Создает перечислитель по всем элементам списка
    /// </summary>
    /// <returns></returns>
    public List<SettingsDataItem>.Enumerator GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator<SettingsDataItem> IEnumerable<SettingsDataItem>.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    bool ICollection<SettingsDataItem>.IsReadOnly
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

    /// <summary>
    /// Записывает данные в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Записываемая секция</param>
    /// <param name="part">Места хранения данных. В отличие от <see cref="SettingsDataItem.WriteConfig(CfgPart, SettingsPart)"/>,
    /// допускается задавать комбинацию из нескольких флагов.</param>
    public void WriteConfig(CfgPart cfg, SettingsPart part)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        for (int j = 0; j < SettingsDataItem.AllParts.Length; j++)
        {
          if ((part & SettingsDataItem.AllParts[j]) != 0 && (_Items[i].UsedParts & SettingsDataItem.AllParts[j]) != 0)
            _Items[i].WriteConfig(cfg, SettingsDataItem.AllParts[j]);
        }
      }
    }

    /// <summary>
    /// Читает данные из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция для чтения</param>
    /// <param name="part">Места хранения данных. В отличие от <see cref="SettingsDataItem.WriteConfig(CfgPart, SettingsPart)"/>,
    /// допускается задавать комбинацию из нескольких флагов.</param>
    public void ReadConfig(CfgPart cfg, SettingsPart part)
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        for (int j = 0; j < SettingsDataItem.AllParts.Length; j++)
        {
          if ((part & SettingsDataItem.AllParts[j]) != 0 && (_Items[i].UsedParts & SettingsDataItem.AllParts[j]) != 0)
            _Items[i].ReadConfig(cfg, SettingsDataItem.AllParts[j]);
        }
      }
    }

    /// <summary>
    /// Записывает данные в секцию конфигурации без разделения на части
    /// </summary>
    /// <param name="cfg">Записываемая секция</param>
    public void WriteConfig(CfgPart cfg)
    {
      WriteConfig(cfg, SettingsDataItem.AllPartValue);
    }

    /// <summary>
    /// Читает данные из секции конфигурации без разбиения на части
    /// </summary>
    /// <param name="cfg">Секция для чтения</param>
    public void ReadConfig(CfgPart cfg)
    {
      ReadConfig(cfg, SettingsDataItem.AllPartValue);
    }

    /// <summary>
    /// Возвращает из списка объект заданного типа.
    /// Возвращает null, если объект не найден
    /// </summary>
    /// <typeparam name="T">Тип объекта в списке</typeparam>
    /// <returns>Объект</returns>
    public T GetItem<T>()
      where T : SettingsDataItem
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
    /// Выбрасывает исключение, если объект не найден и не может быть создан автоматически
    /// </summary>
    /// <typeparam name="T">Тип объекта в списке</typeparam>
    /// <returns>Объект</returns>
    public abstract T GetRequired<T>() where T : SettingsDataItem;

    /// <summary>
    /// Удаляет из списка объект заданного типа
    /// </summary>
    /// <typeparam name="T">Тип удаляемого объекта</typeparam>
    /// <returns>True, если объект найден и удален</returns>
    public bool Remove<T>()
      where T : SettingsDataItem
    {
      T item = GetItem<T>();
      if (item == null)
        return false;
      else
        return _Items.Remove(item); // должно вернуть true
    }

    internal void CopyTo(SettingsDataListBase dest)
    {
      if (dest == null)
        throw new ArgumentNullException("dest");

      foreach (SettingsDataItem srcItem in this)
      {
        foreach (SettingsDataItem destItem in dest)
        {
          if (destItem.GetType() == srcItem.GetType())
          {
            TempCfg cfg = new TempCfg();
            srcItem.WriteConfig(cfg);
            destItem.ReadConfig(cfg);
            break;
          }
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Коллекция наборов данных <see cref="SettingsDataItem"/>.
  /// В коллекции могут присутствовать только объекты разных классов.
  /// Кроме основных настроек, список может содержать наборы именных настроек по умолчанию.
  /// </summary>
  public sealed class SettingsDataList : SettingsDataListBase
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список параметров
    /// </summary>
    public SettingsDataList()
    {
      _DefaultConfigs = new DefaultConfigCollection(this);
    }

    #endregion

    #region Именные настройки

    /// <summary>
    /// Реализация свойства <see cref="DefaultConfigs"/>
    /// </summary>
    public sealed class DefaultConfigCollection
    {
      #region Защищенный конструктор

      internal DefaultConfigCollection(SettingsDataList owner)
      {
        _Owner = owner;
      }

      private readonly SettingsDataList _Owner;

      #endregion

      #region Доступ к именным настройкам

      private NamedList<DefaultSettingsDataList> _Items;

      /// <summary>
      /// Получить или создать набор настроек с заданным кодом
      /// </summary>
      /// <param name="code">Код набора настроек</param>
      /// <returns>Хранилище настроек по умолчанию</returns>
      public DefaultSettingsDataList this[string code]
      {
        get
        {
          if (String.IsNullOrEmpty(code))
            throw ExceptionFactory.ArgStringIsNullOrEmpty("code");

          if (_Items == null)
            _Items = new NamedList<DefaultSettingsDataList>();
          DefaultSettingsDataList res;
          if (!_Items.TryGetValue(code, out res))
          {
            res = new DefaultSettingsDataList(_Owner, code);
            _Items.Add(res);
          }
          return res;
        }
      }

      /// <summary>
      /// Список кодов именных настроек
      /// </summary>
      /// <returns></returns>
      public string[] GetCodes()
      {
        if (_Items == null)
          return EmptyArray<string>.Empty;

        List<string> lst = null;
        foreach (DefaultSettingsDataList item in _Items)
        {
          if (item.IsEmpty)
            continue;
          if (lst == null)
            lst = new List<string>();
          lst.Add(item.Code);
        }
        if (lst == null)
          return EmptyArray<string>.Empty;
        return lst.ToArray();
      }

      #endregion
    }

    /// <summary>
    /// Наборы именных настроек по умолчанию
    /// </summary>
    public DefaultConfigCollection DefaultConfigs { get { return _DefaultConfigs; } }
    private readonly DefaultConfigCollection _DefaultConfigs;

    #endregion

    #region GetDefaultConfigDict()

    private TempCfg _DefaultCfg;

    /// <summary>
    /// Получить секции конфигурации настроек по умолчанию.
    /// Если есть именные настройки в <see cref="DefaultConfigs"/>, то возвращается словарь, ключом которого является код набора настроек, а значением - сохраненные данные.
    /// Если нет именных настроек, то возвращается словарь с единственной записью: ключ - пустая строка, значение - сохраненные значения по умолчанию.
    /// Используется для выбора готовых наборов в диалоге настройки параметров.
    /// При первом вызове метода сохраняются текущие настройки набора как набор значений по умолчанию. Поэтому метод должен быть вызван перед загрузкой сохраненных данных, если она выполняется из прикладного кода, а не диалога параметров.
    /// </summary>
    /// <returns>Словарь настроек по умолчанию.</returns>
    public Dictionary<string, TempCfg> GetDefaultConfigDict()
    {
      bool isFirstCall = (_DefaultCfg == null);
      if (isFirstCall)
      {
        _DefaultCfg = new TempCfg();
        WriteConfig(_DefaultCfg);
      }

      string[] codes = DefaultConfigs.GetCodes();
      Dictionary<string, TempCfg> dict = new Dictionary<string, TempCfg>(codes.Length == 0 ? 1 : codes.Length);
      if (codes.Length == 0)
        dict.Add(String.Empty, _DefaultCfg.Clone());
      else
      {
        foreach (string code in codes)
        {
          DefaultSettingsDataList ds = DefaultConfigs[code];
          TempCfg cfg = _DefaultCfg.Clone();
          ds.WriteConfig(cfg);
          if (isFirstCall)
            ds.ReadConfig(cfg);
          dict.Add(code, cfg);
        }
      }
      return dict;
    }

    #endregion

    #region Прочее

    /// <summary>
    /// Возвращает из списка объект заданного типа.
    /// Выбрасывает исключение, если объект не найден
    /// </summary>
    /// <typeparam name="T">Тип объекта в списке</typeparam>
    /// <returns>Объект</returns>
    public override T GetRequired<T>()
      /*where T : SettingsDataItem*/
    {
      T res = GetItem<T>();
      if (res == null)
        throw ExceptionFactory.KeyNotFound(typeof(T));
      return res;
    }

    /// <summary>
    /// Копирует настройки в другой набор, включая наборы настроек по умолчанию
    /// </summary>
    /// <param name="dest">Заполняемый набор</param>
    public void CopyTo(SettingsDataList dest)
    {
      base.CopyTo(dest);
      foreach (string code in DefaultConfigs.GetCodes())
      {
        DefaultSettingsDataList srcDS = DefaultConfigs[code];
        if (srcDS.Count == 0)
          continue;
        DefaultSettingsDataList destDS = dest.DefaultConfigs[code];
        srcDS.CopyTo(destDS);
      }
    }

    #endregion
  }

  /// <summary>
  /// Именной набор настроек по умолчанию
  /// </summary>
  public sealed class DefaultSettingsDataList : SettingsDataListBase, IObjectWithCode
  {
    #region Конструктор

    internal DefaultSettingsDataList(SettingsDataList owner, string code)
    {
      _Owner = owner;
      if (String.IsNullOrEmpty(code))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("code");
      _Code = code;
    }

    #endregion

    #region IObjectWithCode

    private readonly SettingsDataList _Owner;

    /// <summary>
    /// Код именной настройки. Задается в конструкторе
    /// </summary>
    public string Code { get { return _Code; } }
    private readonly string _Code;

    /// <summary>
    /// Отображаемое имя для списка "Готовые наборы".
    /// Если не установлено в явном виде, возвращает <see cref="Code"/>.
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
          return Code;
        else
          return _DisplayName;
      }
      set { _DisplayName = value; }
    }
    private string _DisplayName;

    /// <summary>
    /// Возвращает true, если набор настроек является пустым
    /// </summary>
    public bool IsEmpty { get { return Count == 0 && String.IsNullOrEmpty(DisplayName); } }

    /// <summary>
    /// Возвращает из списка объект заданного типа для заданной настройки по умолчанию.
    /// Если объект не был добавлен в явном виде, но поддерживается клонирование, создается копия
    /// </summary>
    /// <typeparam name="T">Тип объекта <see cref="SettingsDataItem"/></typeparam>
    /// <returns>Объект для именной настройки</returns>
    public override T GetRequired<T>()
      /*where T : SettingsDataItem*/
    {
      T res = base.GetItem<T>();
      if (res == null)
      {
        ICloneable mainObj = _Owner.GetRequired<T>() as ICloneable;
        if (mainObj == null)
          throw ExceptionFactory.TypeNotCloneable(typeof(T));
        res = (T)(mainObj.Clone());
        base.Add(res);
      }
      return res;
    }

    #endregion
  }
}
