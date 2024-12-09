using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FreeLibSet.Core;
using FreeLibSet.IO;

namespace FreeLibSet.Win32
{
  /// <summary>
  /// Идентификатор или имя ресурса
  /// </summary>
  [Serializable]
  public struct ResourceID : IComparable<ResourceID>, IEquatable<ResourceID>
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор для идентификатора ресурса
    /// </summary>
    /// <param name="id">Числовой идентификатор</param>
    public ResourceID(int id)
    {
      _Name = String.Empty;
      _ID = id;
    }

    /// <summary>
    /// Конструктор для имени ресурса
    /// </summary>
    /// <param name="name">Имя. Не может быть пустой строкой</param>
    public ResourceID(string name)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
      _Name = name;
      _ID = 0;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Числовой идентификатор ресурса или 0, если <see cref="IsID"/>=false.
    /// </summary>
    public int ID { get { return _ID; } }
    private readonly int _ID;

    /// <summary>
    /// Имя ресурса, если <see cref="IsName"/>=true.
    /// </summary>
    public string Name { get { return _Name; } }
    private readonly string _Name;

    /// <summary>
    /// Возвращает true, если структура не была инициализирована
    /// </summary>
    public bool IsEmpty { get { return _Name == null; } }

    /// <summary>
    /// Возвращает true, если используется числовой идентификатор ресурса <see cref="ID"/>.
    /// </summary>
    public bool IsID
    {
      get
      {
        if (_Name == null)
          return false;
        else
          return _Name.Length == 0;
      }
    }

    /// <summary>
    /// Возвращает true, если используется имя ресурса <see cref="Name"/>.
    /// </summary>
    public bool IsName
    {
      get
      {
        if (_Name == null)
          return false;
        else
          return _Name.Length > 0;
      }
    }

    /// <summary>
    /// Неинициализированная структура.
    /// </summary>
    public static readonly ResourceID Empty = new ResourceID();

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Возвращает текстовое представление имени или идентификатора ресурса.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (IsName)
        return Name;
      if (IsID)
        return "#" + ID.ToString();
      return String.Empty;
    }

    #endregion

    #region Сравнение


    /// <summary>
    /// Сравнение на больше/меньше.
    /// При сравнении, "именные" идентификаторы идут перед "числовыми".
    /// Именные идентификаторы считаются нечувствительными к регистру.
    /// Пустое значение меньше, чем непустое
    /// </summary>
    /// <param name="other">Второй сравниваемый объект</param>
    /// <returns>0, если объекты одинаковые. Отрицательное значение, если текущий объект меньше, чем <paramref name="other"/>.
    /// Положительное значение, если текущий объект больше, чем <paramref name="other"/>.</returns>
    public int CompareTo(ResourceID other)
    {
      if (this.IsEmpty)
      {
        if (other.IsEmpty)
          return 0;
        else
          return -1;
      }

      if (other.IsEmpty)
        return +1;

      if (this.IsName)
      {
        if (other.IsName)
          return String.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        else // other.IsID
          return -1;
      }
      else // this.IsId
      {
        if (other.IsID)
          return this.ID.CompareTo(other.ID);
        else // other.IsName
          return +1;
      }
    }

    /// <summary>
    /// Сравнение на равенство.
    /// Идентификаторы считаются равными, если оба являются числовыми или строковыми.
    /// Для ресурсов со строковыми именами регистр символов не учитываются.
    /// Два пустых объекта считаются одинаковыми.
    /// </summary>
    /// <param name="other">Второй идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(ResourceID other)
    {
      return this._ID == other._ID && String.Equals(this._Name, other._Name, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Сравнение на равенство.
    /// Идентификаторы считаются равными, если оба являются числовыми или строковыми.
    /// Для ресурсов со строковыми именами регистр символов не учитываются.
    /// Два пустых объекта считаются одинаковыми.
    /// </summary>
    /// <param name="obj">Второй идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      if (obj is ResourceID)
        return Equals((ResourceID)obj);
      else
        return false;
    }

    /// <summary>
    /// Хэш-код для создания коллекций
    /// </summary>
    /// <returns>Хэш-код</returns>
    public override int GetHashCode()
    {
      if (_Name == null)
        return 0;
      if (_ID == 0)
        return StringComparer.OrdinalIgnoreCase.GetHashCode(_Name);
      else
        return _ID;
    }

    /// <summary>
    /// Сравнение на равенство.
    /// Идентификаторы считаются равными, если оба являются числовыми или строковыми.
    /// Для ресурсов со строковыми именами регистр символов не учитываются.
    /// Два пустых объекта считаются одинаковыми.
    /// </summary>
    /// <param name="a">Первый сравниваемый идентификатор</param>
    /// <param name="b">Второй сравниваемый идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(ResourceID a, ResourceID b)
    {
      return a.Equals(b);
    }

    /// <summary>
    /// Сравнение на неравенство.
    /// Идентификаторы считаются равными, если оба являются числовыми или строковыми.
    /// Для ресурсов со строковыми именами регистр символов не учитываются.
    /// Два пустых объекта считаются одинаковыми.
    /// </summary>
    /// <param name="a">Первый сравниваемый идентификатор</param>
    /// <param name="b">Второй сравниваемый идентификатор</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(ResourceID a, ResourceID b)
    {
      return !a.Equals(b);
    }

    #endregion
  }

  #region Перечисление ResourceType

  /// <summary>
  /// Идентификаторы типов стандартных ресурсов
  /// </summary>
  public enum ResourceType
  {
    /// <summary>
    /// Курсор (одно изображение для <see cref="GroupCursor"/>)
    /// </summary>
    Cursor = 1,

    /// <summary>
    /// BMP
    /// </summary>
    Bitmap = 2,

    /// <summary>
    /// Значок (одно изображение для <see cref="GroupIcon"/>)
    /// </summary>
    Icon = 3,

    /// <summary>
    /// Меню
    /// </summary>
    Menu = 4,

    /// <summary>
    /// Диалог
    /// </summary>
    Dialog = 5,

    /// <summary>
    /// Таблица строк
    /// </summary>
    String = 6,

    /// <summary>
    /// 
    /// </summary>
    FontDir = 7,

    /// <summary>
    /// 
    /// </summary>
    Font = 8,

    /// <summary>
    /// 
    /// </summary>
    Accelerator = 9,

    /// <summary>
    /// 
    /// </summary>
    RCData = 10,

    /// <summary>
    /// 
    /// </summary>
    MessageTable = 11,

    /// <summary>
    /// Курсор (каталог отдельных изображений разного размера и цветности. Сами изображения хранятся в <see cref="Cursor"/>.
    /// </summary>
    GroupCursor = 12,

    /// <summary>
    /// Значок (каталог отдельных изображений разного размера и цветности. Сами изображения хранятся в <see cref="Icon"/>.
    /// </summary>
    GroupIcon = 14,

    /// <summary>
    /// Информация о версии
    /// </summary>
    Version = 16,

    /// <summary>
    /// 
    /// </summary>
    DlgInclude = 17,

    /// <summary>
    /// 
    /// </summary>
    PlugPlay = 19,

    /// <summary>
    /// 
    /// </summary>
    VXD = 20,

    /// <summary>
    /// 
    /// </summary>
    AniCursor = 21,

    /// <summary>
    /// 
    /// </summary>
    AniIcon = 22,

    /// <summary>
    /// 
    /// </summary>
    HTML = 23,

    /// <summary>
    /// 
    /// </summary>
    Manifest = 24,
  }

  #endregion

  /// <summary>
  /// Таблица ресурсов. Предназначен для извлечения информации.
  /// Используется 3 уровня:
  /// 1. Тип ресурса.
  /// 2. Имя ресурса.
  /// 3. Кодовая страница.
  /// 
  /// Класс является абстрактным. Класс-наследник должен определить метод для чтения данных.
  /// </summary>
  public abstract class ResourceTable
  {
    #region Конструктор

    /// <summary>
    /// Создает пустую таблицу
    /// </summary>
    public ResourceTable()
    {
      _Types = new TypeCollection(this);
    }

    #endregion

    #region Тип ресурса

    /// <summary>
    /// Коллекция однотипных ресурсов, обычно входящих в перечисление <see cref="ResourceType"/>, но могут быть и пользовательские типы ресурсов.
    /// </summary>
    public sealed class TypeInfo : List<NameInfo>
    {
      #region Конструктор

      internal TypeInfo(ResourceTable table, ResourceID typeId)
      {
        _Table = table;
        _TypeId = typeId;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Объект-владелец
      /// </summary>
      public ResourceTable Table { get { return _Table; } }
      private readonly ResourceTable _Table;

      /// <summary>
      /// Идентификатор типа ресурсов
      /// </summary>
      public ResourceID TypeId { get { return _TypeId; } }
      private readonly ResourceID _TypeId;

      /// <summary>
      /// Значение для <see cref="ResourceType"/> для нестандартных типов ресурсов
      /// </summary>
      public const ResourceType UnknownResourceType = (ResourceType)0;

      /// <summary>
      /// Тип ресурса. Для нестандартных ресурсов возвращается <see cref="UnknownResourceType"/>
      /// </summary>
      public ResourceType ResourceType
      {
        get
        {
          if (TypeId.IsID)
          {
            ResourceType rt = (ResourceType)(TypeId.ID);
            if (Array.IndexOf(Enum.GetValues(typeof(ResourceType)), rt) >= 0)
              return rt;
          }
          return UnknownResourceType;
        }
      }

      /// <summary>
      /// Для стандартных ресурсов возвращается текстовое представления, для нестандартных - <see cref="TypeId"/>.
      /// </summary>
      /// <returns>Текстовое представление</returns>
      public override string ToString()
      {
        ResourceType rt = this.ResourceType;
        if (rt == UnknownResourceType)
          return TypeId.ToString();
        else
          return rt.ToString().ToUpperInvariant();
      }

      #endregion

      #region Доступ по имени

      /// <summary>
      /// Возвращает ресурс с заданным именем (идентификатором).
      /// Если нет такого ресурса, возвращается null.
      /// </summary>
      /// <param name="name">Идентификатор ресурса</param>
      /// <returns>Описание ресурса или null</returns>
      public NameInfo GetByName(ResourceID name)
      {
        for (int i = 0; i < Count; i++)
        {
          if (this[i].Name == name)
            return this[i];
        }
        return null;
      }

      #endregion
    }

    #endregion

    #region Имя ресурса

    /// <summary>
    /// Ресурс с заданным именем (идентификатором).
    /// Содержит один (обычно) или несколько объектов данных, отличающихся кодовой страницей
    /// </summary>
    public sealed class NameInfo : List<CPInfo>
    {
      #region Конструктор

      internal NameInfo(TypeInfo resourceType, ResourceID name)
      {
        _ResourceType = resourceType;
        _Name = name;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Описание типа ресурса
      /// </summary>
      public TypeInfo ResourceType { get { return _ResourceType; } }
      private readonly TypeInfo _ResourceType;

      /// <summary>
      /// Имя (идентификатор) ресурса
      /// </summary>
      public ResourceID Name { get { return _Name; } }
      private readonly ResourceID _Name;

      /// <summary>
      /// Объект-владелец
      /// </summary>
      public ResourceTable Table { get { return ResourceType.Table; } }

      #endregion
    }

    #endregion

    #region Кодовая страница и данные ресурса

    /// <summary>
    /// Экземпляр данных ресурса с заданной кодовой страницей
    /// </summary>
    public sealed class CPInfo
    {
      #region Конструктор

      internal CPInfo(NameInfo name, int codePage, long offset, int size, string errorMessage)
      {
        _Name = name;
        _CodePage = codePage;
        _Offset = offset;
        _Size = size;
        _ErrorMessage = errorMessage;
      }

      #endregion

      /// <summary>
      /// Описание имени (идентификатора) ресурса, к которому относятся данные
      /// </summary>
      public NameInfo Name { get { return _Name; } }
      private readonly NameInfo _Name;

      /// <summary>
      /// Кодовая страница
      /// </summary>
      public int CodePage { get { return _CodePage; } }
      private readonly int _CodePage;

      /// <summary>
      /// Смещение. Интерпретируется классом - наследником <see cref="ResourceTable"/>.
      /// </summary>
      public long Offset { get { return _Offset; } }
      private readonly long _Offset;

      /// <summary>
      /// Размер блока данных ресурса
      /// </summary>
      public int Size { get { return _Size; } }
      private readonly int _Size;

      /// <summary>
      /// Сообщение об ошибке, если <see cref="Offset"/> или <see cref="Size"/> имеют неправильное значение
      /// </summary>
      public string ErrorMessage { get { return _ErrorMessage; } }
      private readonly string _ErrorMessage;

      /// <summary>
      /// Объект - владелец
      /// </summary>
      public ResourceTable Table { get { return Name.Table; } }
    }

    #endregion

    #region Коллекция типов

    /// <summary>
    /// Коллекция типов ресурсов
    /// </summary>
    public sealed class TypeCollection : Dictionary<ResourceID, TypeInfo>
    {
      internal TypeCollection(ResourceTable table)
      {
        _Table = table;
      }
      private readonly ResourceTable _Table;

      /// <summary>
      /// Возвращает описание (нестандартного) типа ресурсов.
      /// Если описания еще нет, оно создается и добавляется в словарь.
      /// Для стандартных типов ресурсов используйте перегрузку с аргументом <see cref="ResourceType"/>.
      /// </summary>
      /// <param name="typeId">Идентификатор типа ресурсов</param>
      /// <returns>Описатель</returns>
      public new TypeInfo this[ResourceID typeId]
      {
        get
        {
          TypeInfo res;
          if (!base.TryGetValue(typeId, out res))
          {
            res = new TypeInfo(_Table, typeId);
            base.Add(typeId, res);
          }
          return res;
        }
      }

      /// <summary>
      /// Возвращает описание стандартного типа ресурсов.
      /// Если описания еще нет, оно создается и добавляется в словарь.
      /// </summary>
      /// <param name="rt">Тип ресурсов</param>
      /// <returns>Описатель</returns>
      public TypeInfo this[ResourceType rt]
      {
        get
        {
          ResourceID typeId = new ResourceID((int)rt);
          return this[typeId];
        }
      }
    }

    /// <summary>
    /// Словарь типов ресурсов
    /// </summary>
    public TypeCollection Types { get { return _Types; } }
    private readonly TypeCollection _Types;


    //public TypeInfo Icons { get { return Types[ResourceTypes.RT_ICON]; } }


    #endregion

    #region Заполнение таблицы

    /// <summary>
    /// 
    /// </summary>
    public void Add(ResourceID typeId, ResourceID name, int codePage, long offset, int size, string errorMessage)
    {
      if (typeId.IsEmpty)
        throw new ArgumentException("Не задан тип ресурса", "typeId");
      if (name.IsEmpty)
        throw new ArgumentException("Не задан идентификатор ресурса", "name");

      TypeInfo ti = Types[typeId];
      NameInfo ni = ti.GetByName(name);
      if (ni == null)
      {
        ni = new NameInfo(ti, name);
        ti.Add(ni);
      }

      CPInfo cpi = new CPInfo(ni, codePage, offset, size, errorMessage);
      ni.Add(cpi);
    }

    #endregion

    #region Абстрактные методы

    /// <summary>
    /// Загружает данные для ресурса
    /// </summary>
    /// <param name="cpi">Описание блока данных ресурса</param>
    /// <returns>Массив байтов блока данных</returns>
    public abstract byte[] GetBytes(CPInfo cpi);

    #endregion

    #region Значки

    /// <summary>
    /// Параметры значка. 
    /// Структура однократной записи
    /// </summary>
    public struct IconInfo : IEquatable<IconInfo>
    {
      #region Конструктор
      
      /// <summary>
      /// Инициализация структуры
      /// </summary>
      /// <param name="width">Ширина</param>
      /// <param name="height">Высота</param>
      /// <param name="bpp">Количество бит на пиксель</param>
      public IconInfo(int width, int height, int bpp)
      {
        if (width < 1)
          throw new ArgumentOutOfRangeException("width");
        if (height < 0)
          throw new ArgumentOutOfRangeException("height");
        if (Array.IndexOf<int>(ValidBPPs, bpp) < 0)
          throw new ArgumentOutOfRangeException("bpp", bpp, "Неправильное значение BPP. Допустимые значения: "+
            DataTools.ToStringJoin<int>(", ", ValidBPPs));

        _Width = width;
        _Height = height;
        _BPP = bpp;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Ширина значка в пикселях
      /// </summary>
      public int Width { get { return _Width; } }
      private readonly int _Width;

      /// <summary>
      /// Высота значка в пикселях (без удвоения высоты, заданной в битовой матрице)
      /// </summary>
      public int Height { get { return _Height; } }
      private readonly int _Height;
      
      /// <summary>
      /// Количество бит на пиксель
      /// </summary>
      public int BPP { get { return _BPP; } }
      private readonly int _BPP;

      /// <summary>
      /// Допустимые значения свойства <see cref="BPP"/>.
      /// </summary>
      public static readonly int[] ValidBPPs = new int[] { 1, 4, 8, 16, 24, 32 };

      #endregion

      #region Текстовое представление

      /// <summary>
      /// Для отладки
      /// </summary>
      /// <returns>Текстовое представление</returns>
      public override string ToString()
      {
        if (_Width == 0)
          return "Empty";
        return Width.ToString() + "x" + Height.ToString() + ", " + BPP.ToString() + "bpp";
      }

      #endregion

      #region Сравнение

      /// <summary>
      /// Сравнение двух описателей
      /// </summary>
      /// <param name="a">Первый описатель</param>
      /// <param name="b">Второй описатель</param>
      /// <returns>Результат сравнения</returns>
      public static bool operator ==(IconInfo a, IconInfo b)
      {
        return a.Width == b.Width && a.Height == b.Height && a.BPP == b.BPP;
      }

      /// <summary>
      /// Сравнение двух описателей
      /// </summary>
      /// <param name="a">Первый описатель</param>
      /// <param name="b">Второй описатель</param>
      /// <returns>Результат сравнения</returns>
      public static bool operator !=(IconInfo a, IconInfo b)
      {
        return !(a == b);
      }

      /// <summary>
      /// Сравнение с другим описателем
      /// </summary>
      /// <param name="other">Второй описатель</param>
      /// <returns>Результат сравнения</returns>
      public bool Equals(IconInfo other)
      {
        return this == other;
      }

      /// <summary>
      /// Сравнение с другим описателем
      /// </summary>
      /// <param name="obj">Второй описатель</param>
      /// <returns>Результат сравнения</returns>
      public override bool Equals(object obj)
      {
        if (obj is IconInfo)
          return Equals((IconInfo)obj);
        else
          return false;
      }

      /// <summary>
      /// Хэш-код для использования в словарях
      /// </summary>
      /// <returns>Хэш-код</returns>
      public override int GetHashCode()
      {
        return (Width << 16) | (Height << 8) | BPP;
      }

      #endregion
    }

    /// <summary>
    /// Коллекция значков разного размера/цветности, относящаяся к одному ресурсу <see cref="ResourceType.GroupIcon"/>
    /// </summary>
    public class GroupIconInfo : Dictionary<IconInfo, CPInfo>
    {
    }

    /// <summary>
    /// Получение списка значков разного размера/цветности, относящихся к одному ресурсу
    /// </summary>
    /// <param name="groupIconId">Идентификатор ресурса <see cref="ResourceType.GroupIcon"/></param>
    /// <returns>Список значков</returns>
    public GroupIconInfo GetGroupIconInfo(ResourceID groupIconId)
    {
      GroupIconInfo dict = new GroupIconInfo();

      NameInfo niDir = Types[ResourceType.GroupIcon].GetByName(groupIconId);
      if (niDir == null)
        throw new ArgumentException("Ресурс не найден");
      byte[] b = GetBytes(niDir[0]);
      if (b.Length == 0)
        return dict;
      MemoryStream ms = new MemoryStream(b);
      BinaryReader rdr = new BinaryReader(ms);
      if (rdr.ReadUInt16() != 0)
        throw new InvalidOperationException("GRPICONDIR.Reserved != 0");
      if (rdr.ReadUInt16() != 1)
        throw new InvalidOperationException("GRPICONDIR.Type != 1");
      int nIcons = rdr.ReadUInt16();

      for (int i = 0; i < nIcons; i++)
      {
        int width = rdr.ReadByte();
        int height = rdr.ReadByte();
        int colorCount = rdr.ReadByte(); // colorCount
        rdr.ReadByte(); // reserved
        rdr.ReadUInt16(); // Planes
        int bpp = rdr.ReadUInt16(); 
        rdr.ReadUInt32(); // BytesInRes
        int iconId = rdr.ReadUInt16();

        if (width == 0)
          width = 256;
        if (height == 0)
          height = 256;

        if (bpp == 0)
        {
          // 04.12.2023
          // Может быть задано количество цветов, а не количество bpp

          switch (colorCount)
          {
            case 2:bpp = 1;break;
            case 16: bpp = 4;break;
            default: throw new InvalidOperationException("Для значка не задан BPP и задан недопустимый ColorCount="+colorCount.ToString());            
          }
        }

        IconInfo ii = new IconInfo(width, height, bpp);

        ResourceID iconId2 = new ResourceID(iconId);
        NameInfo niIcon = Types[ResourceType.Icon].GetByName(iconId2);
        if (niIcon == null)
          throw new InvalidOperationException("Не найден значок " + iconId2.ToString());

        dict.Add(ii, niIcon[0]);
      }
      return dict;
    }

    /// <summary>
    /// Возвращает данные для значка, добавляя 22-байтный заголовок.
    /// Значок содержит единственное изображение.
    /// Полученный массив можно преобразовать в объект Icon.
    /// </summary>
    /// <param name="iconId">Идентификатор значка <see cref="ResourceType.Icon"/></param>
    /// <returns>Данные значка</returns>
    public byte[] GetSingleImageIconBytes(ResourceID iconId)
    {
      if (iconId.IsEmpty)
        throw new ArgumentException("Идентификатор не задан", "iconId");
      TypeInfo ti = Types[ResourceType.Icon];
      NameInfo ni = ti.GetByName(iconId);
      if (ni == null)
        throw new ArgumentException("Нет значка с идентификатором " + iconId.ToString(), "iconId");
      if (ni.Count == 0)
        throw new ArgumentException("Пустой ресурс значка с идентификатором " + ni.Name);

      return GetSingleImageIconBytes(ni[0]);
    }

    /// <summary>
    /// Возвращает данные для значка, добавляя 22-байтный заголовок.
    /// Значок содержит единственное изображение.
    /// Полученный массив можно преобразовать в объект Icon.
    /// </summary>
    /// <param name="cpi">Ссылка на ресурс <see cref="ResourceType.Icon"/></param>
    /// <returns>Данные значка</returns>
    public byte[] GetSingleImageIconBytes(CPInfo cpi)
    {
      IconInfo iconInfo;
      return GetSingleImageIconBytes(cpi, out iconInfo);
    }

    /// <summary>
    /// Возвращает данные для значка, добавляя 22-байтный заголовок.
    /// Значок содержит единственное изображение.
    /// Полученный массив можно преобразовать в объект Icon.
    /// </summary>
    /// <param name="cpi">Ссылка на ресурс</param>
    /// <param name="iconInfo">Сюда помещаются параметры загруженного значка.
    /// Загрузив полученное изображение в объект System.Drawing.Icon, можно получить размеры значка, но не количество цветов</param>
    /// <returns>Данные значка</returns>
    public byte[] GetSingleImageIconBytes(CPInfo cpi, out IconInfo iconInfo)
    {
      iconInfo = new IconInfo();
      byte[] b = GetBytes(cpi);
      if (b.Length == 0)
        return null;
      MemoryStream ms1 = new MemoryStream(b);
      BinaryReader rdr = new BinaryReader(ms1);

      int w, h, planes, bpp;

      int headLen = rdr.ReadInt32();
      if (headLen == 0x28)
      {
        w = rdr.ReadInt32();
        h = rdr.ReadInt32() / 2; // удвоенная высота
        if (w < 1 | w > 256)
          throw new BugException("Ширина");
        if (h < 1 | h > 256)
          throw new BugException("Высота");

        planes = rdr.ReadInt16();
        bpp = rdr.ReadInt16();
      }
      else
      {
        ms1.Position = 0;
        if (FileTools.StartsWith(ms1, new byte[8] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }))
        {
          // PNG-файл
          ms1.Position = 8;
          int recLen = rdr.ReadInt32();
          int recType = rdr.ReadInt32();

          const int specBitMask = (32 << 24) | (32 << 16) | (32 << 8) | 32;
          const int nameMask = ~specBitMask;
          recType = recType & nameMask;
          if (recType != 0x52444849) //"IHDR"
            throw new InvalidOperationException("Первым блоком в PNG-файле должен быть заголовок IHDR");
          w = rdr.ReadInt32();
          h = rdr.ReadInt32();
          bpp = rdr.ReadByte();
          rdr.ReadByte(); // colorType
          rdr.ReadByte(); // compression
          rdr.ReadByte(); // filter
          rdr.ReadByte(); // interface

          planes = 1; // не знаю, зачем это поле
        }
        else
          throw new InvalidOperationException("Неизестный заголовок");
      }

      iconInfo = new IconInfo(w, h, bpp);

      if (w > 255)
        w = 0;
      if (h > 255)
        h = 0;

      MemoryStream ms = new MemoryStream();
      BinaryWriter wrt = new BinaryWriter(ms);
      wrt.Write((UInt16)0); // reserved
      wrt.Write((UInt16)1); // .ico
      wrt.Write((UInt16)1); // NumberOfImages

      wrt.Write((byte)w);
      wrt.Write((byte)h);
      wrt.Write((byte)0); // number of colors
      wrt.Write((byte)0); // reserved
      wrt.Write((short)planes); // color planes
      wrt.Write((short)(iconInfo.BPP)); // BPP
      wrt.Write(b.Length);
      wrt.Write(22); // смещение от начала файла

      wrt.Write(b);

      return ms.GetBuffer();
    }

    /// <summary>
    /// Возвращает данные для значка.
    /// Значок содержит несколько изображений различного размера/цветности.
    /// Полученный массив можно преобразовать в объект Icon.
    /// </summary>
    /// <param name="groupIconId">Идентификатор ресурса <see cref="ResourceType.GroupIcon"/></param>
    /// <returns>Данные значка</returns>
    public byte[] GetIconBytes(ResourceID groupIconId)
    {
      GroupIconInfo grpInfo = GetGroupIconInfo(groupIconId);
      if (grpInfo.Count == 0)
        return null;
      MemoryStream ms = new MemoryStream();
      BinaryWriter wrt = new BinaryWriter(ms);
      wrt.Write((UInt16)0); // reserved
      wrt.Write((UInt16)1); // .ico
      wrt.Write((UInt16)(grpInfo.Count)); // NumberOfImages

      int offset = 6 + 16 * grpInfo.Count;
      foreach (KeyValuePair<IconInfo, CPInfo> pair in grpInfo)
      {
        int w = pair.Key.Width;
        int h = pair.Key.Height;
        if (w > 255) w = 0;
        if (h > 255) h = 0;
        int bpp = pair.Key.BPP;
        int planes = 1;
        wrt.Write((byte)w);
        wrt.Write((byte)h);
        wrt.Write((byte)0); // number of colors
        wrt.Write((byte)0); // reserved
        wrt.Write((short)planes); // color planes
        wrt.Write((short)bpp); // BPP
        wrt.Write(pair.Value.Size);
        wrt.Write(offset); // смещение от начала файла
        offset += pair.Value.Size;
      }
      foreach (KeyValuePair<IconInfo, CPInfo> pair in grpInfo)
      {
        wrt.Write(GetBytes(pair.Value));
      }
      return ms.GetBuffer();
    }

    /// <summary>
    /// Возвращает данные для значка.
    /// Значок содержит несколько изображений различного размера/цветности.
    /// Полученный массив можно преобразовать в объект Icon.
    /// Используется первый по счету ресурс <see cref="ResourceType.GroupIcon"/>.
    /// Если нет такого ресурса, но есть <see cref="ResourceType.Icon"/>, то возвращается значок с единственным изображением.
    /// Если в Exe-файле значков нет, возвращается null.
    /// </summary>
    /// <returns>Данные значка</returns>
    public byte[] GetIconBytes()
    {
      //return GetIconBytes(new IconInfo(16, 16, 24));

      try
      {
        if (Types[ResourceType.GroupIcon].Count > 0)
        {
          ResourceID groupIconId = Types[ResourceType.GroupIcon][0].Name;
          return GetIconBytes(groupIconId);
        }
      }
      catch { }

      // Берем первый попавшийся значок
      if (Types[ResourceType.Icon].Count > 0)
        return GetSingleImageIconBytes(Types[ResourceType.Icon][0].Name);
      else
        return null;
    }

    /// <summary>
    /// Эмуляция системного вызова ExtractIcon().
    /// Масштабирование значка не выполняется.
    /// Если <paramref name="iconIndex"/> находится вне диапазона, возвращается null
    /// </summary>
    /// <param name="iconIndex">Номер ресурса значка в ресурсах GROUPICON. Нумерация начинается с 1.</param>
    /// <param name="smallIcon">true - вернуть маленький значок 16x16, false - большой значок 32x32. 
    /// Игнорируется, если ресурс не содержит значков разного размера.</param>
    /// <returns></returns>
    public byte[] ExtractIconBytes(int iconIndex, bool smallIcon)
    {
      if (iconIndex < 1 || iconIndex > Types[ResourceType.GroupIcon].Count)
        return null;

      NameInfo ni = Types[ResourceType.GroupIcon][iconIndex - 1];
      GroupIconInfo grpInfo = GetGroupIconInfo(ni.Name);
      int wantedSize = smallIcon ? 16 : 32;
      KeyValuePair<IconInfo, CPInfo> bestPair=new KeyValuePair<IconInfo, CPInfo>();

      foreach (KeyValuePair<IconInfo, CPInfo> pair in grpInfo)
      {
        if (bestPair.Value == null)
          bestPair=pair;
        else
        {
          int delta1 = Math.Abs(pair.Key.Width - wantedSize);
          int delta2 = Math.Abs(bestPair.Key.Width - wantedSize);
          if (delta1 < delta2)
            bestPair = pair;
        }
      }

      if (bestPair.Value == null)
        return null;
      else
        return GetSingleImageIconBytes(bestPair.Value);
    }

    #endregion
  }
}
