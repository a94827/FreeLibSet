// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Resources;
using System.Threading;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Реализация свойства EFPApp.MainImages.
  /// Хранит коллекцию Bitmap размером 16x16 пикселей для использования в элементах пользовательского интерфейса.
  /// Основное свойство Images предоставляет доступ к объектам Bitmap по строковому ключу.
  /// Свойство ImageList можно использовать в управляющих элементах TabControl, ListView и TreeView.
  /// Свойство Icons используется для получения значков для форм. 
  /// Коллекции ImageList и Icons являются вторичными и заполняются автоматическими.
  /// Класс является потокобезопасным.
  /// </summary>
  public sealed class EFPAppMainImages
  {
    #region Защищенный конструктор

    internal EFPAppMainImages()
    {
      DoAdd(MainImagesResource.ResourceManager, Color.Transparent, null);
      _Icons = new IconCollection(this);
    }

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
      DoAdd(resourceManager, transparentColor, Images.Items);
    }

    private void DoAdd(ResourceManager resourceManager, Color transparentColor, Dictionary<string, Bitmap> oldDict)
    {
#if DEBUG
      if (resourceManager == null)
        throw new ArgumentNullException("resourceManager");
#endif

      Dictionary<string, Bitmap> newDict;
      if (oldDict == null)
        newDict = new Dictionary<string, Bitmap>();
      else
        newDict = new Dictionary<string, Bitmap>(oldDict);

      ResourceSet rs = resourceManager.GetResourceSet(System.Globalization.CultureInfo.InvariantCulture, true, true);
      foreach (System.Collections.DictionaryEntry de in rs)
      {
        Bitmap bmp = de.Value as Bitmap;
        if (bmp != null)
        {
          if (transparentColor != Color.Transparent)
            bmp.MakeTransparent(transparentColor);

          newDict[(String)de.Key] = bmp;
        }
      }

      // Замена ссылки является безопасной атомартной операцией
      _Images = new ImageCollection(newDict);
    }

    #endregion

    #region Основной список изображений

    /// <summary>                                                    +
    /// Реализация свойства Images.
    /// Предусмотрена возможность добавления пользовательских изображений.
    /// Удаление элементов не предусмотрено.
    /// </summary>
    public sealed class ImageCollection : IDictionary<string, Bitmap>
    {
      #region Защищенный конструктор

      internal ImageCollection(Dictionary<string, Bitmap> dict)
      {
        _Items = dict;

        _Empty = _Items["EmptyImage"];
        _UnknownState = _Items["UnknownState"];
        _HourGlass = _Items["HourGlass"];

        _ThreadImageListDict = new Dictionary<int, System.Windows.Forms.ImageList>();
        _Icons = new Dictionary<string, Icon>();
      }

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
      /// Установка свойства запрещена. Для добавления значков используйте метод Add() основного класса EFPAppMainImages, который создаст новый экземпляр коллекции
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
          throw new NotImplementedException();
        }
      }

      internal Dictionary<string, Bitmap> Items { get { return _Items; } }
      private readonly Dictionary<string, Bitmap> _Items;

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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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

      bool ICollection<KeyValuePair<string, Bitmap>>.IsReadOnly { get { return true; } }

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

      #region Списки ImageList

      /// <summary>
      /// Списки ImageList для потоков.
      /// Ключ - идентификатор потока.
      /// Значение - созданный и заполненный список изображений.
      /// При доступе к словарю объект словаря блокируется.
      /// </summary>
      private Dictionary<int, System.Windows.Forms.ImageList> _ThreadImageListDict;

      internal System.Windows.Forms.ImageList GetImageList()
      {
        System.Windows.Forms.ImageList res;
        lock (_ThreadImageListDict)
        {
          if (!_ThreadImageListDict.TryGetValue(Thread.CurrentThread.ManagedThreadId, out res))
          {
            res = new System.Windows.Forms.ImageList();
            res.ColorDepth = System.Windows.Forms.ColorDepth.Depth4Bit; // поменьше пусть будет
            res.ImageSize = new Size(16, 16);
            foreach (KeyValuePair<string, Bitmap> pair in this)
              res.Images.Add(pair.Key, pair.Value);

            _ThreadImageListDict.Add(Thread.CurrentThread.ManagedThreadId, res);
          }
        }
        return res;
      }

      internal bool GetIsImageListCreated()
      {
        lock (_ThreadImageListDict)
        {
          return _ThreadImageListDict.Count > 0;
        }
      }

      #endregion

      #region Значки Icon

      /// <summary>
      /// Эта коллекция пополняется из разных потоков, поэтому должна выполняться блокировка.
      /// </summary>
      private readonly Dictionary<string, Icon> _Icons;

      internal Icon GetIcon(string imageKey)
      {
        Icon res;
        lock (_Icons)
        {
          if (!_Icons.TryGetValue(imageKey, out res))
          {
            Bitmap bmp = this[imageKey];
            res = Icon.FromHandle(bmp.GetHicon());
            _Icons.Add(imageKey, res);
          }
          return res;
        }
      }

      #endregion
    }

    /// <summary>
    /// Доступ к изображениям в формате Bitmap.
    /// Изображения в словаре являются собственностью этой коллекции и не должны удаляться в прикладном коде.
    /// При добавлении ресурсов изображений методом Add() это свойство заменяется на новое.
    /// </summary>
    public ImageCollection Images { get { return _Images; } }
    private ImageCollection _Images;

    #endregion

    #region Список ImageList

    /// <summary>
    /// Возвращает объект ImageList, который можно использовать с управляющими элементами TabControl, ListView и TreeView.
    /// Прикладной код не должен модифицировать этот список, так как он автоматически синхронизируется с основной коллекцией Images.
    /// При доступе из разных потоков возвращаются разные экземпляры объектов ImageList, так как этот класс не является потокобезопасным.
    /// При добавлении изображений методом Add() возвращется новый ImageList.
    /// </summary>
    public System.Windows.Forms.ImageList ImageList
    {
      get
      {
        return _Images.GetImageList();
      }
    }

    internal bool IsImageListCreated { get { return _Images.GetIsImageListCreated(); } }

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
          if (String.IsNullOrEmpty(imageKey))
            return null;
          return _Owner.Images.GetIcon(imageKey);
        }
      }

      #endregion

      #region Иницализация формы

      /// <summary>
      /// Установка значка формы (свойства Form.Icon и Form.ShowIcon)
      /// Если задано имя изображения, то форма будет иметь значок.
      /// Иначе свойство ShowIcon сбрасывается в false, но форме присваивается иконка приложения, чтобы форма
      /// правильно отображалась в панели задач.
      /// </summary>
      /// <param name="form">Инициализируемая форма</param>
      /// <param name="imageKey">Имя изображения из списка EFPApp.MainImages</param>
      /// <param name="isModal">True, если форма будет показана в модальном режиме, false - если в немодальном</param>
      public void InitForm(System.Windows.Forms.Form form, string imageKey, bool isModal)
      {
#if DEBUG
        if (form == null)
          throw new ArgumentNullException("form");
#endif

        if (!String.IsNullOrEmpty(imageKey))
        {
          form.ShowIcon = true;
          form.Icon = this[imageKey];
          return;
        }

        if (isModal && EFPApp.DialogOwnerWindow != null)
        {
          // 01.03.2021
          form.ShowIcon = false;
          return;
        }

        WinFormsTools.InitAppIcon(form);
        form.ShowIcon = !EFPApp.MainWindowVisible;
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
