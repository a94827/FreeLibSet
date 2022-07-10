// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Resources;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Реализация свойства EFPApp.MainImages.
  /// Хранит коллекцию Bitmap размером 16x16 пикселей для использования в элементах пользовательского интерфейса.
  /// Основное свойство Images предоставляет доступ к объектам Bitmap по строковому ключу.
  /// Свойство ImageList можно использовать в управляющих элементах TabControl, ListView и TreeView.
  /// Свойство Icons используется для получения значков для форм. 
  /// Коллекции ImageList и Icons являются вторичными и заполняются автоматическими.
  /// </summary>
  public sealed class EFPAppMainImages
  {
    #region Защищенный конструктор

    internal EFPAppMainImages()
    {
      _Images = new ImageCollection(this);
      _Icons = new IconCollection(this);
    }

    #endregion

    #region Основной список изображений

    /// <summary>
    /// Реализация свойства Images.
    /// Предусмотрена возможность добавления пользовательских изображений.
    /// Удаление элементов не предусмотрено.
    /// </summary>
    public sealed class ImageCollection : IDictionary<string, Bitmap>
    {
      #region Защищенный конструктор

      internal ImageCollection(EFPAppMainImages owner)
      {
        _Owner = owner;
        _Items = new Dictionary<string, Bitmap>();

        Add(MainImagesResource.ResourceManager);
        _Empty = _Items["EmptyImage"];
        _UnknownState = _Items["UnknownState"];
        _HourGlass = _Items["HourGlass"];
      }

      private EFPAppMainImages _Owner;

      #endregion

      #region Доступ к изображениям

      /// <summary>
      /// Основное свойство для получения изображения по ключу.
      /// При чтении свойства возвращается ссылка на хранимое изображение, а не копия, как при обращении к ImageList.Items.
      /// Для полученного объекта нельзя вызывать метод Dispose(). Если требуется передать изображение во владение другому элементу,
      /// используйте метод Clone().
      /// Если <paramref name="imageKey"/> - пустая строка или null, возвращается пустое изображение EmptyImage.
      /// Если передан ключ, для которого нет изображения в словаре, возвращается изображение "?".
      ///
      /// Установка свойства выполняет добавление или изменение существующего изображение, при этом <paramref name="imageKey"/> должен быть непустой строкой.
      /// </summary>
      /// <param name="imageKey">Ключ для доступа к изображению</param>
      /// <returns>Изображение</returns>
      public Bitmap this[string imageKey]
      {
        get
        {
          if (String.IsNullOrEmpty(imageKey))
            return _Empty;
          Bitmap res;
          if (_Items.TryGetValue(imageKey, out res))
            return res;
          else
            return _UnknownState;
        }
        set
        {
          if (String.IsNullOrEmpty(imageKey))
            throw new ArgumentNullException("imageKey");
          if (value == null)
            throw new ArgumentNullException("value");

          _Items[imageKey] = value;

          if (_Owner._ImageList != null)
          {
            int p = _Owner._ImageList.Images.IndexOfKey(imageKey);
            if (p >= 0)
              _Owner._ImageList.Images[p] = value;
            else
              _Owner._ImageList.Images.Add(imageKey, value);
          }

          if (_Owner.Icons != null) // может быть не инициализирован
            _Owner._Icons.Remove(imageKey);
        }
      }

      private readonly Dictionary<string, Bitmap> _Items;

      #endregion

      #region Методы добавления изображений


      /// <summary>
      /// Добавляет изображения из ресурсов.
      /// </summary>
      /// <param name="resourceManager">Менеджер ресурсов</param>
      public void Add(ResourceManager resourceManager)
      {
        Add(resourceManager, Color.Transparent);
      }

      /// <summary>
      /// Добавляет изображения из ресурсов.
      /// </summary>
      /// <param name="resourceManager">Менеджер ресурсов</param>
      /// <param name="transparentColor">Если задано значение, отличное от Color.Transparent, будет вызван метод Bitmap.</param>
      public void Add(ResourceManager resourceManager, Color transparentColor)
      {
#if DEBUG
        if (resourceManager == null)
          throw new ArgumentNullException("resourceManager");
#endif
        ResourceSet rs = resourceManager.GetResourceSet(System.Globalization.CultureInfo.InvariantCulture, true, true);
        foreach (System.Collections.DictionaryEntry de in rs)
        {
          Bitmap bmp = de.Value as Bitmap;
          if (bmp != null)
          {
            if (transparentColor != Color.Transparent)
              bmp.MakeTransparent(transparentColor);

            this[(String)de.Key] = bmp;
          }
        }
      }

      #endregion

      #region Предопределенные изображения

      /// <summary>
      /// Пустое изображение
      /// </summary>
      public Bitmap Empty { get { return _Empty; } }
      private readonly Bitmap _Empty;

      /// <summary>
      /// Изображение со знаком "?"
      /// </summary>
      public Bitmap UnknownState { get { return _UnknownState; } }
      private readonly Bitmap _UnknownState;

      /// <summary>
      /// Изображение с песочными часами
      /// </summary>
      public Bitmap HourGlass { get { return _HourGlass; } }
      private readonly Bitmap _HourGlass;

      #endregion

      #region IDictionary<string,Bitmap> Members

      void IDictionary<string, Bitmap>.Add(string key, Bitmap value)
      {
        this[key] = value;
      }

      /// <summary>
      /// Возвращает true, если в коллекции есть такое изображение.
      /// </summary>
      /// <param name="imageKey">Ключ для доступа к изображению</param>
      /// <returns>Наличие изображения</returns>
      public bool ContainsKey(string imageKey)
      {
        return _Items.ContainsKey(imageKey);
      }

      /// <summary>
      /// Возвращает коллекцию существующих ключей изображений
      /// </summary>
      public ICollection<string> Keys { get { return _Items.Keys; } }

      bool IDictionary<string, Bitmap>.Remove(string key)
      {
        throw new NotImplementedException();
      }

      /// <summary>
      /// Возвращает изображение, если оно есть в словаре.
      /// В отличие от основного свойства, которое возвращает "?" для неправильного ключа, метод TryGetValue() не выполняет замену.
      /// </summary>
      /// <param name="imageKey">Ключ для доступа к изображению</param>
      /// <param name="value">Сюда помещается ссылка на изображение или null</param>
      /// <returns></returns>
      public bool TryGetValue(string imageKey, out Bitmap value)
      {
        if (String.IsNullOrEmpty(imageKey))
        {
          value = null;
          return false;
        }
        return _Items.TryGetValue(imageKey, out value);
      }

      /// <summary>
      /// Возвращает коллекцию изображений
      /// </summary>
      public ICollection<Bitmap> Values { get { return _Items.Values; } }

      #endregion

      #region ICollection<KeyValuePair<string,Bitmap>> Members

      void ICollection<KeyValuePair<string, Bitmap>>.Add(KeyValuePair<string, Bitmap> item)
      {
        this[item.Key] = item.Value;
      }

      void ICollection<KeyValuePair<string, Bitmap>>.Clear()
      {
        throw new NotImplementedException();
      }

      bool ICollection<KeyValuePair<string, Bitmap>>.Contains(KeyValuePair<string, Bitmap> item)
      {
        throw new NotImplementedException();
      }

      /// <summary>
      /// Копирует пары "Ключ-изображение" в пользовательский массив
      /// </summary>
      /// <param name="array"></param>
      /// <param name="arrayIndex"></param>
      public void CopyTo(KeyValuePair<string, Bitmap>[] array, int arrayIndex)
      {
        ((ICollection<KeyValuePair<string, Bitmap>>)(_Items)).CopyTo(array, arrayIndex);
      }

      /// <summary>
      /// Возвращает количество изображений в коллекции
      /// </summary>
      public int Count { get { return _Items.Count; } }

      bool ICollection<KeyValuePair<string, Bitmap>>.IsReadOnly { get { return false; } }

      bool ICollection<KeyValuePair<string, Bitmap>>.Remove(KeyValuePair<string, Bitmap> item)
      {
        throw new NotImplementedException();
      }

      #endregion

      #region IEnumerable<KeyValuePair<string,Bitmap>> Members

      /// <summary>
      /// Возвращает перечислитель по парам "ключ-изображение"
      /// </summary>
      /// <returns></returns>
      public Dictionary<string, Bitmap>.Enumerator GetEnumerator()
      {
        return _Items.GetEnumerator();
      }

      IEnumerator<KeyValuePair<string, Bitmap>> IEnumerable<KeyValuePair<string, Bitmap>>.GetEnumerator()
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
    /// Доступ к изображениям в формате Bitmap.
    /// Изображения в словаре являются собственностью этой коллекции и не должны удаляться в прикладном коде.
    /// </summary>
    public ImageCollection Images { get { return _Images; } }
    private readonly ImageCollection _Images;

    #endregion

    #region Список ImageList

    /// <summary>
    /// Возвращает объект ImageList, который можно использовать с управляющими элементами TabControl, ListView и TreeView.
    /// Прикладной код не должен модифицировать этот список, так как он автоматически синхронизируется с основной коллекцией Images.
    /// </summary>
    public System.Windows.Forms.ImageList ImageList
    {
      get
      {
        if (_ImageList == null)
        {
          //EFPApp.BeginWait("");
          System.Windows.Forms.ImageList tmp = new System.Windows.Forms.ImageList();
          tmp.ColorDepth = System.Windows.Forms.ColorDepth.Depth4Bit; // поменьше пусть будет
          tmp.ImageSize = new Size(16, 16);
          foreach (KeyValuePair<string, Bitmap> pair in Images)
            tmp.Images.Add(pair.Key, pair.Value);

          _ImageList = tmp;
        }
        return _ImageList;
      }
    }
    private System.Windows.Forms.ImageList _ImageList;

    internal bool ImageListCreated { get { return _ImageList != null; } }

    #endregion

    #region Список Icons

    /// <summary>
    /// Реализация свойства Icons
    /// </summary>
    public sealed class IconCollection
    {
      #region Конструктор

      internal IconCollection(EFPAppMainImages owner)
      {
        _Owner = owner;
        _Items = new Dictionary<string, Icon>();
      }

      private EFPAppMainImages _Owner;

      #endregion

      #region Доступ к значкам

      /// <summary>
      /// Получить значок, соответствующий изображению.
      /// Если <paramref name="imageKey"/> - пустая строка, возращается null, а не пустой значок.
      /// Если нет такого изображения в коллекции Images, возвращается значок "?".
      /// Иконки являются собственностью этой коллекции. Для них не должен вызываться метод Dispose().
      /// </summary>
      /// <param name="imageKey"></param>
      /// <returns></returns>
      public Icon this[string imageKey]
      {
        get
        {
          if (EFPApp.MainThread == null)
            return null; // Не было вызова InitApp()

#if DEBUG
          EFPApp.CheckMainThread();
#endif

          if (String.IsNullOrEmpty(imageKey))
            return null;
          Icon res;
          if (!_Items.TryGetValue(imageKey, out res))
          {
            Bitmap bmp = _Owner.Images[imageKey] as Bitmap;
            res = Icon.FromHandle(bmp.GetHicon());
            _Items.Add(imageKey, res);
          }
          return res;
        }
      }
      private readonly Dictionary<string, Icon> _Items;

      internal void Remove(string imageKey)
      {
        _Items.Remove(imageKey); // не вызываем Dispose(), так как значок может быть присоединен к форме.
      }

      #endregion
    }

    /// <summary>
    /// Значки, соответствующие изображениям.
    /// Иконки содержат единственное изображение 16x16 пикселей.
    /// Их можно использовать для значков форм.
    /// </summary>
    public IconCollection Icons { get { return _Icons; } }
    private readonly IconCollection _Icons;

    #endregion
  }
}
