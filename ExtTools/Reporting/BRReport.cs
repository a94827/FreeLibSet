// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Config;
using FreeLibSet.Core;
using System.Text.RegularExpressions;

namespace FreeLibSet.Reporting
{
  /// <summary>
  /// Объект отчета, основной объект.
  /// Отчет состоит из нескольких секций <see cref="BRSection"/>.
  /// </summary>
  public sealed class BRReport
  {
    #region Константы

    /// <summary>
    /// Нулевое значение свойства <see cref="BRRowInfo.Height"/> означает автоматический подюор высоты строки
    /// </summary>
    public const int AutoRowHeight = 0;

    /// <summary>
    /// Минимальная высота строки в единицах 0.1мм
    /// </summary>
    public const int MinRowHeight = 10;

    /// <summary>
    /// Максимальная высота строки в единицах 0.1мм.
    /// </summary>
    public const int MaxRowHeight = 10000;

    /// <summary>
    /// Минимальная ширина столбца в единицах 0.1мм
    /// </summary>
    public const int MinColumnWidth = 10;

    /// <summary>
    /// Максимальная ширина столбца в единицах 0.1мм
    /// </summary>
    public const int MaxColumnWidth = 10000;

    /// <summary>
    /// Имя шрифта по умолчанию
    /// </summary>
    public static string DefaultFontName
    {
      // TODO: Определять из системных настроек
      get
      {
        switch (Environment.OSVersion.Platform)
        {
          case PlatformID.Unix:
            return "Liberation Sans";
          default:
            return "Arial";
        }
      }
    }

    /// <summary>
    /// Высота шрифта по умолчанию (10пт)
    /// </summary>
    public const int DefaultFontHeightTwip = 10 * 20;

    /// <summary>
    /// Минимальная высота шрифта (1 пункт)
    /// </summary>
    public const int MinFontHeightTwip = 1 * 20;

    /// <summary>
    /// Максимальная высота шрифта
    /// </summary>
    public const int MaxFontHeightTwip = 200 * 20;

    /// <summary>
    /// Минимальное значение для свойства <see cref="BRCellStyle.FontWidthPercent"/>.
    /// </summary>
    public const int MinFontWidthPercent = 25;

    /// <summary>
    /// Максимальное значение для свойства <see cref="BRCellStyle.FontWidthPercent"/>.
    /// </summary>
    public const int MaxFontWidthPercent = 400;

    /// <summary>
    /// Минимальное значение для свойства <see cref="BRCellStyle.FontWidthTwip"/>.
    /// </summary>
    public const int MinFontWidthTwip = MinFontHeightTwip / 4;

    /// <summary>
    /// Максимальное значение для свойства <see cref="BRCellStyle.FontWidthTwip"/>.
    /// </summary>
    public const int MaxFontWidthTwip = MaxFontHeightTwip * 4;

    /// <summary>
    /// Минимальное значение для свойства <see cref="BRCellStyle.MaxEnlargePercent"/>.
    /// </summary>
    public const int MinFontEnlargePercent = 100;

    /// <summary>
    /// Максимальное значение для свойства <see cref="BRCellStyle.MaxEnlargePercent"/>.
    /// </summary>
    public const int MaxFontEnlargePercent = 400;

    /// <summary>
    /// Минимальное значение для свойства <see cref="BRCellStyle.IndentLevel"/>.
    /// </summary>
    public const int MinIndentLevel = 0;

    /// <summary>
    /// Максимальное значение для свойства <see cref="BRCellStyle.IndentLevel"/>.
    /// </summary>
    public const int MaxIndentLevel = 15;

    #endregion

    #region Конструктор

    /// <summary>
    /// Создает отчет без секций и с параметрами по умолчанию.
    /// </summary>
    public BRReport()
    {
      _DefaultEmptyRowHeight = AppDefaultEmptyRowHeight;
      _DefaultColumnWidth = AppDefaultColumnWidth;
      _DefaultCellStyle = new BRDefaultCellStyle(this);
      _SectionList = new List<BRSection>();
      _NamedCellStyleList = new NamedList<BRNamedCellStyle>();
      _DocumentProperties = _AppDefaultDocumentProperties.Clone();
      _BookmarkList = new NamedList<BRBookmark>();
    }

    #endregion

    #region Список секций

    private readonly List<BRSection> _SectionList;

    /// <summary>
    /// Реализация свойства <see cref="BRReport.Sections"/>.
    /// </summary>
    public struct SectionCollection : IEnumerable<BRSection>
    {
      internal SectionCollection(BRReport report)
      {
        _Report = report;
      }

      private readonly BRReport _Report;

      /// <summary>
      /// Возвращает количество секций
      /// </summary>
      public int Count { get { return _Report._SectionList.Count; } }

      /// <summary>
      /// Доступ к секции по индексу
      /// </summary>
      /// <param name="index">Индекс секции в диапазоне от 0 до (<see cref="Count"/>-1)</param>
      /// <returns>Объект секции</returns>
      public BRSection this[int index] { get { return _Report._SectionList[index]; } }

      /// <summary>
      /// Поиск секции
      /// </summary>
      /// <param name="section">Секция</param>
      /// <returns>Индекс секции. (-1), если секция не относится к отчету</returns>
      public int IndexOf(BRSection section)
      {
        return _Report._SectionList.IndexOf(section);
      }

      /// <summary>
      /// Создает новую секцию и добавляет ее в конец списка
      /// </summary>
      /// <returns>Новая секция</returns>
      public BRSection Add()
      {
        BRSection section = new BRSection(_Report);
        _Report._SectionList.Add(section);
        return section;
      }

      #region GetEnumerator()

      /// <summary>
      /// Создает перечислитель по секциям отчета
      /// </summary>
      /// <returns>Перечислитель</returns>
      public List<BRSection>.Enumerator GetEnumerator()
      {
        return _Report._SectionList.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      IEnumerator<BRSection> IEnumerable<BRSection>.GetEnumerator()
      {
        return GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Список секций отчета
    /// </summary>
    public SectionCollection Sections { get { return new SectionCollection(this); } }

    /// <summary>
    /// Возвращает общее количество полос во всех секциях
    /// </summary>
    public int BandCount
    {
      get
      {
        int cnt = 0;
        foreach (BRSection sect in Sections)
          cnt += sect.Bands.Count;
        return cnt;
      }
    }

    /// <summary>
    /// Возвращает true, если в документе есть хотя бы одна гиперссылка
    /// </summary>
    public bool HasLinks
    {
      get
      {
        foreach (BRSection sect in Sections)
        {
          if (sect.HasLinks)
            return true;
        }
        return false;
      }
    }

    #endregion

    #region Параметры страницы

    /// <summary>
    /// Параметры страницы по умолчанию, используемые для всех отчетов в программе.
    /// При создании первой секции отчета <see cref="BRSection"/>, данные отсюда копируются в свойства <see cref="BRSection.PageSetup"/>
    /// </summary>
    public static BRPageSetup AppDefaultPageSetup { get { return _AppDefaultPageSetup; } }
    private static readonly BRPageSetup _AppDefaultPageSetup = new BRPageSetup(false);

    #endregion

    #region Размеры ячеек

    /// <summary>
    /// Высота пустой строки по умолчанию, используемая при создании объектов <see cref="BRReport"/> в единицах 0.1мм.
    /// По умолчанию - 5мм
    /// </summary>
    public static int AppDefaultEmptyRowHeight
    {
      get { return _AppDefaultEmptyRowHeight; }
      set
      {
        if (value < MinRowHeight || value > MaxRowHeight)
          throw ExceptionFactory.ArgOutOfRange("value", value, MinRowHeight, MaxRowHeight);
        _AppDefaultEmptyRowHeight = value;
      }
    }
    private static int _AppDefaultEmptyRowHeight = 50;


    /// <summary>
    /// Высота строки по умолчанию в единицах 0.1мм
    /// </summary>
    public int DefaultEmptyRowHeight
    {
      get { return _DefaultEmptyRowHeight; }
      set
      {
        if (value < MinRowHeight || value > MaxRowHeight)
          throw ExceptionFactory.ArgOutOfRange("value", value, MinRowHeight, MaxRowHeight);
        _DefaultEmptyRowHeight = value;
      }
    }
    private int _DefaultEmptyRowHeight;

    /// <summary>
    /// Ширина столбца, используемая при создании объектов <see cref="BRReport"/> в единицах 0.1мм.
    /// По умолчанию - 2см.
    /// </summary>
    public static int AppDefaultColumnWidth
    {
      get { return _AppDefaultColumnWidth; }
      set
      {
        if (value < MinColumnWidth || value > MaxColumnWidth)
          throw ExceptionFactory.ArgOutOfRange("value", value, MinColumnWidth, MaxColumnWidth);
        _AppDefaultColumnWidth = value;
      }
    }
    private static int _AppDefaultColumnWidth = 200;

    /// <summary>
    /// Ширина столбца по умолчанию в единицах 0.1мм
    /// </summary>
    public int DefaultColumnWidth
    {
      get { return _DefaultColumnWidth; }
      set
      {
        if (value < MinColumnWidth || value > MaxColumnWidth)
          throw ExceptionFactory.ArgOutOfRange("value", value, MinColumnWidth, MaxColumnWidth);
        _DefaultColumnWidth = value;
      }
    }
    private int _DefaultColumnWidth;

    #endregion

    #region Стили ячеек

    private class BRDefaultCellStyle : BRCellStyleStorage
    {
      #region Конструктор

      internal BRDefaultCellStyle(BRReport report)
        : base(null)
      {
        _Report = report;
        if (report != null)
        {
          for (int i = 0; i < Array_Size; i++)
          {
            if (i == Index_ParentStyleName)
              continue;
            object v = BRReport.AppDefaultCellStyle.GetValue(null, i);
            SetValue(i, v);
          }
        }
      }

      #endregion

      #region Свойства 

      internal override BRReport Report { get { return _Report; } }
      private BRReport _Report;

      #endregion

      #region Обработка значений

      /// <summary>
      /// Этот метод может вызываться дважды.
      /// В первый раз - от стиля секции по цепочке вызовов. В этом случае надо проверить именной стиль и перенаправить вызов.
      /// Во второй раз - от именного стиля. В этом случе аргумент caller не передается
      /// </summary>
      /// <param name="caller"></param>
      /// <param name="index"></param>
      /// <returns></returns>
      internal override object GetValue(BRCellStyle caller, int index)
      {
        if (index == Index_ParentStyleName)
          return String.Empty;

        if (caller != null)
        {
          string nm = caller.ParentStyleName;
          if (nm.Length > 0)
          {
            BRNamedCellStyle st = _Report._NamedCellStyleList.GetRequired(nm);
            return st.GetValue(null, index);
          }
        }

        return base.GetValue(caller, index);
      }

      internal override void SetValue(int index, object value)
      {
        if (index == Index_ParentStyleName)
          throw new InvalidOperationException(Res.BRReport_Err_PropertyCannotBeSet);
        base.SetValue(index, value);
      }

      #endregion
    }

    /// <summary>
    /// Статический набор стилей. Он копируется в <see cref="DefaultCellStyle"/> при создании <see cref="BRReport"/>.
    /// В нем можно задать, например, предпочитаемый шрифт, который будет использоваться по умолчанию во всех отчетах.
    /// Обычно следует задавать параметры для конкретного отчета, используя <see cref="DefaultCellStyle"/>.
    /// </summary>
    public static BRCellStyle AppDefaultCellStyle { get { return _AppDefaultCellStyle; } }
    private static readonly BRDefaultCellStyle _AppDefaultCellStyle = new BRDefaultCellStyle(null);

    /// <summary>
    /// Стили ячеек по умолчанию.
    /// При создании объекта <see cref="BRReport"/> заполняется копиями значений из <see cref="AppDefaultCellStyle"/>.
    /// </summary>
    public BRCellStyle DefaultCellStyle { get { return _DefaultCellStyle; } }
    private readonly BRDefaultCellStyle _DefaultCellStyle;

    private NamedList<BRNamedCellStyle> _NamedCellStyleList;

    /// <summary>
    /// Реализация свойства <see cref="NamedCellStyles"/>.
    /// </summary>
    public struct NamedCellStyleCollection : IEnumerable<BRNamedCellStyle>
    {
      #region Конструктор

      internal NamedCellStyleCollection(BRReport report)
      {
        _Report = report;
      }

      #endregion

      #region Свойства

      private BRReport _Report;

      /// <summary>
      /// Количество определенных именных стилей
      /// </summary>
      public int Count { get { return _Report._NamedCellStyleList.Count; } }

      /// <summary>
      /// Возвращает описание именного стиля по индексу
      /// </summary>
      /// <param name="index">Индекс именного стиля в диапазоне от 0 до (<see cref="Count"/>-1)</param>
      /// <returns>Описание именного стиля</returns>
      public BRNamedCellStyle this[int index] { get { return _Report._NamedCellStyleList[index]; } }

      /// <summary>
      /// Возвращает описание именного стиля по индексу.
      /// Если нет стиля с указанным именем, возвращается null.
      /// </summary>
      /// <param name="name">Имя стиля</param>
      /// <returns>Описание именного стиля или null</returns>
      public BRNamedCellStyle this[string name] { get { return _Report._NamedCellStyleList[name]; } }

      #endregion

      #region GetEnumerator

      /// <summary>
      /// Создает перечислитель по именным стилям.
      /// Стили перечисляются в порядке добавления.
      /// </summary>
      /// <returns>Перечислитель</returns>
      public List<BRNamedCellStyle>.Enumerator GetEnumerator()
      {
        return _Report._NamedCellStyleList.GetEnumerator();
      }

      IEnumerator<BRNamedCellStyle> IEnumerable<BRNamedCellStyle>.GetEnumerator()
      {
        return _Report._NamedCellStyleList.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return _Report._NamedCellStyleList.GetEnumerator();
      }

      #endregion

      #region Добавление

      /// <summary>
      /// Создает именной стиль и добавляет его в коллекцию.
      /// </summary>
      /// <param name="name">Имя нового стиля. Должно быть задано</param>
      /// <param name="parent">Родительский стиль, из которого будут браться значения параметров, не определенные в новом стиле.
      /// Если null, то недостающие данные будут браться из <see cref="BRReport.DefaultCellStyle"/>.</param>
      /// <returns>Описание нового стиля</returns>
      public BRNamedCellStyle Add(string name, BRNamedCellStyle parent)
      {
        BRCellStyle parent2;
        if (parent == null)
          parent2 = _Report.DefaultCellStyle;
        else
        {
          if (!Object.ReferenceEquals(parent.Report, _Report))
            throw new ArgumentException(Res.BRReport_Arg_ParentStyleForDifferentReport);
          parent2 = parent;
        }
        BRNamedCellStyle obj = new BRNamedCellStyle(name, parent2);
        _Report._NamedCellStyleList.Add(obj);
        return obj;
      }

      /// <summary>
      /// Создает именной стиль и добавляет его в коллекцию.
      /// </summary>
      /// <param name="name">Имя нового стиля. Должно быть задано</param>
      /// <param name="parentName">Имя родительского стиля, из которого будут браться значения параметров, не определенные в новом стиле.
      /// Если пустая строка или null, то недостающие данные будут браться из <see cref="BRReport.DefaultCellStyle"/>.
      /// Должно быть задано имя уже добавленного стиля, в противном случае будет выброшено исключение</param>
      /// <returns>Описание нового стиля</returns>
      public BRNamedCellStyle Add(string name, string parentName)
      {
        if (String.IsNullOrEmpty(parentName))
          return Add(name, (BRNamedCellStyle)null);
        else
        {
          BRNamedCellStyle parent = _Report._NamedCellStyleList.GetRequired(parentName);
          return Add(name, parent);
        }
      }

      /// <summary>
      /// Создает именной стиль и добавляет его в коллекцию.
      /// Новый стиль будет брать недостающие значения параметров из <see cref="BRReport.DefaultCellStyle"/>.
      /// </summary>
      /// <param name="name">Имя нового стиля. Должно быть задано</param>
      /// <returns>Описание нового стиля</returns>
      public BRNamedCellStyle Add(string name)
      {
        return Add(name, (BRNamedCellStyle)null);
      }

      #endregion
    }

    /// <summary>
    /// Коллекция именных стилей.
    /// Именные стили образуют дерево для извлечения значений параметров, которые не определены в конкретном стиле.
    /// Корнем дерева является набор стилей <see cref="DefaultCellStyle"/>.
    /// Чтобы использовать именные стили, необходимо указать имя в свойстве <see cref="BRCellStyle.ParentStyleName"/> для стилей ячеек 
    /// (или других стилей, образующих иерархию).
    /// </summary>
    public NamedCellStyleCollection NamedCellStyles { get { return new NamedCellStyleCollection(this); } }

    #endregion

    #region Список шрифтов

    /// <summary>
    /// Возвращает список всех используемых шрифтов
    /// </summary>
    /// <returns></returns>
    public string[] GetFontNames()
    {
      SingleScopeStringList lst = new SingleScopeStringList(false);
      lst.Add(DefaultCellStyle.FontName); // пусть будет первым

      foreach (BRSection section in Sections)
      {
        foreach (BRBand band in section.Bands)
        {
          BRSelector sel = band.CreateSelector();
          for (int i = 0; i < band.RowCount; i++)
          {
            sel.RowIndex = i;
            for (int j = 0; j < band.ColumnCount; j++)
            {
              sel.ColumnIndex = j;
              lst.Add(sel.CellStyle.FontName);
            }
          }
        }
      }
      return lst.ToArray();
    }

    #endregion

    #region Сводка

    /// <summary>
    /// Свойства документов (сводка), общие для всего приложения.
    /// По умолчанию поля не заполнены.
    /// </summary>
    public static BRDocumentProperties AppDefaultDocumentProperties { get { return _AppDefaultDocumentProperties; } }
    private static readonly BRDocumentProperties _AppDefaultDocumentProperties = new BRDocumentProperties();

    /// <summary>
    /// Свойства документа (сводка).
    /// При создании <see cref="BRReport"/> копируются из <see cref="AppDefaultDocumentProperties"/>.
    /// </summary>
    public BRDocumentProperties DocumentProperties
    {
      get { return _DocumentProperties; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _DocumentProperties = value;
      }
    }
    private BRDocumentProperties _DocumentProperties;

    #endregion

    #region Закладки

    private readonly NamedList<BRBookmark> _BookmarkList;

    /// <summary>
    /// Реализация свойства <see cref="Bookmarks"/>
    /// </summary>
    public struct BookmarkCollection : ICollection<BRBookmark>
    {
      #region Конструктор

      internal BookmarkCollection(BRReport owner)
      {
        _Owner = owner;
      }

      private BRReport _Owner;

      #endregion

      #region ICollection<BRBookmark>

      /// <summary>
      /// Возвращает количество закладок
      /// </summary>
      public int Count { get { return _Owner._BookmarkList.Count; } }

      bool ICollection<BRBookmark>.IsReadOnly { get { return false; } }

      /// <summary>
      /// Добавляет закладку
      /// </summary>
      /// <param name="item">Добавляемая закладка</param>
      public void Add(BRBookmark item)
      {
        if (item == null)
          throw new ArgumentNullException("item");
        if (item.Report != _Owner)
          throw ExceptionFactory.ArgProperty("item", item, "Report", item.Report, new object[] { _Owner });
        _Owner._BookmarkList.Add(item);
        if (item.Band.BookmarkList == null)
          item.Band.BookmarkList = new List<BRBookmark>();
        item.Band.BookmarkList.Add(item);
      }

      void ICollection<BRBookmark>.Clear()
      {
        throw new NotImplementedException();
        //_Owner._BookmarkList.Clear();
      }

      bool ICollection<BRBookmark>.Contains(BRBookmark item)
      {
        return _Owner._BookmarkList.Contains(item);
      }

      void ICollection<BRBookmark>.CopyTo(BRBookmark[] array, int arrayIndex)
      {
        _Owner._BookmarkList.CopyTo(array, arrayIndex);
      }

      bool ICollection<BRBookmark>.Remove(BRBookmark item)
      {
        throw new NotImplementedException();
        //return _Owner._BookmarkList.Remove(item);
      }

      /// <summary>
      /// Возвращает перечислитель по закладкам
      /// </summary>
      /// <returns>Перечислитель</returns>
      public IEnumerator<BRBookmark> GetEnumerator()
      {
        return _Owner._BookmarkList.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Список всех закладок отчета
    /// </summary>
    public BookmarkCollection Bookmarks { get { return new BookmarkCollection(this); } }

    #endregion
  }

  /// <summary>
  /// Поля сводки документа, используемые при сохранении отчета в файле
  /// </summary>
  public sealed class BRDocumentProperties : ICloneable
  {
    #region Поля для сводки

    /// <summary>
    /// Сводка документа - название
    /// </summary>
    public string Title { get { return GetText(_Title); } set { _Title = value; } }
    private string _Title;

    /// <summary>
    /// Сводка документа - тема
    /// </summary>
    public string Subject { get { return GetText(_Subject); } set { _Subject = value; } }
    private string _Subject;

    /// <summary>
    /// Сводка документа - автор
    /// </summary>
    public string Author { get { return GetText(_Author); } set { _Author = value; } }
    private string _Author;

    /// <summary>
    /// Сводка документа - учреждение
    /// </summary>
    public string Company { get { return GetText(_Company); } set { _Company = value; } }
    private string _Company;

    private static string GetText(string s)
    {
      if (s == null)
        return String.Empty;
      s = s.Replace(Environment.NewLine, " ");
      s = DataTools.ReplaceCharRange(s, (char)0, (char)31, ' ');
      s = s.Replace(DataTools.SoftHyphenStr, "");
      s = s.Replace(DataTools.NonBreakSpaceChar, ' ');
      return s;
    }

    #endregion

    #region Другие свойства

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("Title: ");
      sb.Append(Title);
      sb.Append(", Subject: ");
      sb.Append(Subject);
      sb.Append(", Author: ");
      sb.Append(Author);
      sb.Append(", Company: ");
      sb.Append(Company);
      return sb.ToString();
    }

    #endregion

    #region Клонирование

    /// <summary>
    /// Создает копию сводки
    /// </summary>
    /// <returns>Новый объект BRDocumentProperties</returns>
    public BRDocumentProperties Clone()
    {
      BRDocumentProperties res = new BRDocumentProperties();
      res.Title = Title;
      res.Subject = Subject;
      res.Company = Company;
      res.Author = Author;
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }

  /// <summary>
  /// Секция отчета.
  /// Секция содержит одну или несколько полос.
  /// Также для секции задаются параметры страницы.
  /// Секция соответствует разделу документа Word, который печатается с новой страницы.
  /// В Excel секция соответствует одному листу.
  /// Для создания секции и добавления ее в отчет используйте метод <see cref="BRReport.SectionCollection.Add()"/>.
  /// </summary>
  public sealed class BRSection
  {
    #region Конструктор

    internal BRSection(BRReport report)
    {
      _Report = report;
      _BandList = new List<BRBand>();
      if (report.Sections.Count == 0)
        _PageSetup = new BRPageSetup();
      else
      {
        BRSection lastSection = report.Sections[report.Sections.Count - 1];
        _PageSetup = lastSection.PageSetup.Clone();
      }
    }

    #endregion

    #region Список полос

    private readonly List<BRBand> _BandList;

    /// <summary>
    /// Реализация свойства <see cref="Bands"/>.
    /// </summary>
    public struct BandCollection : IEnumerable<BRBand>
    {
      internal BandCollection(BRSection section)
      {
        _Section = section;
      }

      private readonly BRSection _Section;

      /// <summary>
      /// Возвращает количество полос в секции
      /// </summary>
      public int Count { get { return _Section._BandList.Count; } }

      /// <summary>
      /// Доступ к полосе по индексу
      /// </summary>
      /// <param name="index">Индекс полосы в диапазоне от 0 до (<see cref="Count"/>-1)</param>
      /// <returns>Описание полосы</returns>
      public BRBand this[int index] { get { return _Section._BandList[index]; } }

      /// <summary>
      /// Поиск полосы
      /// </summary>
      /// <param name="band">Описание полосы</param>
      /// <returns>Индекс полосы или (-1), если полоса не относится к секции</returns>
      public int IndexOf(BRBand band)
      {
        return _Section._BandList.IndexOf(band);
      }

      /// <summary>
      /// Создает обычную (невиртуальную) таблицу <see cref="BRTable"/> с указанным числом строк и столбцов, и долавляет ее к секции.
      /// </summary>
      /// <param name="rowCount">Количество строк в таблице. Должно быть не меньше 1.</param>
      /// <param name="columnCount">Количество столбцов в таблице. Должно быть не меньше 1.</param>
      /// <returns>Объект таблицы</returns>
      public BRTable Add(int rowCount, int columnCount)
      {
        BRTable table = new BRTable(_Section, rowCount, columnCount);
        _Section._BandList.Add(table);
        return table;
      }

      /// <summary>
      /// Добавляет виртуальную или обычную таблицу в список полос для секции.
      /// Не допускается повторное добавление полосы в ту же или в любую другую секцию.
      /// </summary>
      /// <param name="band">Таблица. Не может быть null</param>
      public void Add(BRBand band)
      {
        if (band == null)
          throw new ArgumentNullException("band");
        if (band.Section != _Section)
          throw ExceptionFactory.ArgProperty("band", band, "Section", band.Section, new object[] { band.Section });
        _Section._BandList.Add(band);
      }

      #region IEnumerable members

      /// <summary>
      /// Создает перечислитель по полосам секции
      /// </summary>
      /// <returns>Перечислитель</returns>
      public List<BRBand>.Enumerator GetEnumerator()
      {
        return _Section._BandList.GetEnumerator();
      }

      IEnumerator<BRBand> IEnumerable<BRBand>.GetEnumerator()
      {
        return GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Коллекция полос (таблиц) для секции
    /// </summary>
    public BandCollection Bands { get { return new BandCollection(this); } }

    #endregion

    #region Свойства

    /// <summary>
    /// Отчет, к которому относится секция
    /// </summary>
    public BRReport Report { get { return _Report; } }
    private readonly BRReport _Report;

    /// <summary>
    /// Индекс секции в отчете <see cref="BRReport.Sections"/>
    /// </summary>
    public int SectionIndex { get { return _Report.Sections.IndexOf(this); } }

    /// <summary>
    /// Параметры страницы для секции.
    /// Когда создается первая секция, то в параметры страницы копируются значения из статического набора параметров <see cref="BRReport.AppDefaultPageSetup"/>.
    /// Для второй и последующих секций параметры копируются из предыдущей секции.
    /// </summary>
    public BRPageSetup PageSetup
    {
      get { return _PageSetup; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _PageSetup = value;
      }
    }
    private BRPageSetup _PageSetup;

    /// <summary>
    /// Имя листа при передаче в Excel/Calc
    /// </summary>
    public string Name
    {
      get
      {
        if (String.IsNullOrEmpty(_Name))
          return String.Format(Res.BRReport_Msg_ExcelSheetName, SectionIndex + 1);
        else
          return _Name;
      }
      set
      { _Name = value; }
    }
    private string _Name;

    /// <summary>
    /// Возвращает свойство <see cref="Name"/> и количество полос
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Name + " (BandCount=" + Bands.Count.ToString() + ")";
    }

    /// <summary>
    /// Возвращает true, если в секции есть хотя бы одна гиперссылка
    /// </summary>
    public bool HasLinks
    {
      get
      {
        foreach (BRBand band in Bands)
        {
          if (band.HasLinks)
            return true;
        }
        return false;
      }
    }

    #endregion
  }

  /// <summary>
  /// Описание полосы отчета.
  /// Полосой является либо обычная таблица <see cref="BRTable"/>, либо виртуальная таблица <see cref="BRVirtualTable"/>.
  /// В любом случае, полоса содержит определяемое в конструкторе количество строк и столбцов, которое нельзя изменить в дальнейшем.
  /// Для доступа к значениям и параметрам ячеек, а также параметрам строк и столбцов, используется селектор (класс, производный от <see cref="BRSelector"/>).
  /// </summary>
  public abstract class BRBand
  {
    #region Конструктор

    /// <summary>
    /// Инициализация объекта
    /// </summary>
    /// <param name="section">Секция, куда будет добавлена полоса. Присоединение выполняется отдельным вызовом.</param>
    /// <param name="rowCount">Количество строк. Должно быть больше 0</param>
    /// <param name="columnCount">Количество столбцов. Должно быть больше 0</param>
    public BRBand(BRSection section, int rowCount, int columnCount)
    {
      if (section == null)
        throw new ArgumentNullException("section");
      if (rowCount < 1)
        throw ExceptionFactory.ArgOutOfRange("rowCount", rowCount, 1, null);
      if (columnCount < 1)
        throw ExceptionFactory.ArgOutOfRange("columnCount", columnCount, 1, null);

      _Section = section;
      _RowCount = rowCount;
      _ColumnCount = columnCount;
      _KeepWhole = true;
    }

    #endregion

    #region Фиксированные свойства

    /// <summary>
    /// Секция, к которой относится полоса
    /// </summary>
    public BRSection Section { get { return _Section; } }
    private readonly BRSection _Section;

    /// <summary>
    /// Отчет
    /// </summary>
    public BRReport Report { get { return Section.Report; } }

    /// <summary>
    /// Индекс полосы в секции <see cref="BRSection.Bands"/>.
    /// </summary>
    public int BandIndex { get { return Section.Bands.IndexOf(this); } }

    /// <summary>
    /// Количество строк
    /// </summary>
    public int RowCount { get { return _RowCount; } }
    private readonly int _RowCount;

    /// <summary>
    /// Количество столбцов
    /// </summary>
    public int ColumnCount { get { return _ColumnCount; } }
    private readonly int _ColumnCount;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "BandIndex=" + BandIndex.ToString() + ", (" + RowCount.ToString() + "x" + ColumnCount.ToString() + ")";
    }

    #endregion

    #region Свойства полосы

    /// <summary>
    /// Задает отступ от предыдущей полосы в единицах 0.1мм. 
    /// По умолчанию - 0 - нет отступа.
    /// Отступ не используется, если полоса является первой на странице.
    /// Если для соседних полос на странице заданы свойства <see cref="BottomMargin"/> и <see cref="TopMargin"/>, то зазор между полосами будет равен максимальному из двух значений.
    /// </summary>
    public int TopMargin { get { return _TopMargin; } set { _TopMargin = value; } }
    private int _TopMargin;

    /// <summary>
    /// Задает отступ до следующей полосы в единицах 0.1мм.
    /// По умолчанию - 0 - нет отступа.
    /// Отступ не используется, если полоса является последней на странице.
    /// Если для соседних полос на странице заданы свойства <see cref="BottomMargin"/> и <see cref="TopMargin"/>, то зазор между полосами будет равен максимальному из двух значений.
    /// </summary>
    public int BottomMargin { get { return _BottomMargin; } set { _BottomMargin = value; } }
    private int _BottomMargin;

    /// <summary>
    /// Если true, то эта полоса будет размещена на странице вместе со следующей.
    /// По умолчанию - false.
    /// Устанавливайте значение например для заголовков.
    /// Свойство игнорируется для последней полосы секции.
    /// Установка свойства <see cref="KeepWithNext"/> для полосы эквивалентно установке <see cref="KeepWithPrev"/> для следующей полосы.
    /// 
    /// Свойства <see cref="KeepWithNext"/> / <see cref="KeepWithPrev"/> имеют более высокий приоритет, чем <see cref="KeepWhole"/> и <see cref="BRRowInfo.KeepWithNext"/> / <see cref="BRRowInfo.KeepWithPrev"/>.
    /// Если две сцепленные полосы не могут быть размещены на одной странице, то разрыв будет выполнен внутри второй полосы, а не между полос. При этом будет
    /// учтены свойства <see cref="BRRowInfo.KeepWithNext"/> / <see cref="BRRowInfo.KeepWithPrev"/>. Если же они не позволяют выполнить разрыв второй полосы,
    /// то выполняется попытка разрыва первой полосы. Если же и она не может быть разорвана, то выполняется разрыв между полосами.
    /// </summary>
    public bool KeepWithNext { get { return _KeepWithNext; } set { _KeepWithNext = value; } }
    private bool _KeepWithNext;

    /// <summary>
    /// Если true, то эта полоса будет размещена на странице вместе с предыдущей.
    /// По умолчанию - false.
    /// Устанавливайте значение например для примечаний или подписей.
    /// Свойство игнорируется для первой полосы секции.
    /// Установка свойства <see cref="KeepWithNext"/> для полосы эквивалентно установке <see cref="KeepWithPrev"/> для следующей полосы.
    /// 
    /// См. комментарий к свойству <see cref="KeepWithNext"/> по приоритету свойств.
    /// </summary>
    public bool KeepWithPrev { get { return _KeepWithPrev; } set { _KeepWithPrev = value; } }
    private bool _KeepWithPrev;

    /// <summary>
    /// Если true (по умолчанию), то при разбиении на страницы делается попытка разместить полосу на одной странице, как если бы свойство <see cref="BRRowInfo.KeepWithNext"/> 
    /// было установлено для всех строк таблицы.
    /// Если false, то будет размещена часть таблицы, которая помещается до конца текущей страницы, с учетом локальных запретов разрывов строк.
    /// Свойство игнорируется, если размер всех строк таблицы превышает размер области печати и таблица не может быть размещена на одной странице.
    /// 
    /// См. комментарий к свойству <see cref="KeepWithNext"/> по приоритету свойств.
    /// </summary>
    public bool KeepWhole { get { return _KeepWhole; } set { _KeepWhole = value; } }
    private bool _KeepWhole;

    #endregion

    /// <summary>
    /// Создает новый селектор, который можно использовать для чтения/записи значений, атрибутов ячеек, строк и столбцов.
    /// При необходимости можно создать несколько селекторов.
    /// </summary>
    /// <returns>Новый селектор</returns>
    public abstract BRSelector CreateSelector();

    #region Закладки

    /// <summary>
    /// Реализация свойства <see cref="Bookmarks"/>.
    /// В текущей реализации нет доступа к закладкам для полосы
    /// </summary>
    public struct BookmarkCollection
    {
      #region Конструктор

      internal BookmarkCollection(BRBand owner)
      {
        _Owner = owner;
      }

      private readonly BRBand _Owner;

      #endregion

      #region Доступ к закладкам

      private static readonly BRBookmark[] _EmptyBookmarkArray = new BRBookmark[0];

      /// <summary>
      /// Возвращает массив закладок, относящихся к ячейке.
      /// Если закладок нет, возвращается пустой массив
      /// </summary>
      /// <param name="rowIndex">Ин</param>
      /// <param name="columnIndex"></param>
      /// <returns>Массив закладок</returns>
      public BRBookmark[] this[int rowIndex, int columnIndex]
      {
        get
        {
          if (_Owner.BookmarkList == null)
            return _EmptyBookmarkArray;

          List<BRBookmark> lst = null;
          foreach (BRBookmark bm in _Owner.BookmarkList)
          {
            if (bm.RowIndex == rowIndex && bm.ColumnIndex == columnIndex)
            {
              if (lst == null)
                lst = new List<BRBookmark>();
              lst.Add(bm);
            }
          }

          if (lst == null)
            return _EmptyBookmarkArray;
          else
            return lst.ToArray();
        }
      }

      /// <summary>
      /// Возвращает количество закладок для полосы.
      /// Если закладок нет, возвращается 0.
      /// </summary>
      public int Count
      {
        get
        {
          if (_Owner.BookmarkList == null)
            return 0;
          else
            return _Owner.BookmarkList.Count;
        }
      }

      #endregion

      #region Методы добавления

      /// <summary>
      /// Добавляет закладку в отчет
      /// </summary>
      /// <param name="name">Имя закладки</param>
      /// <param name="rowIndex">Индекс строки</param>
      /// <param name="columnIndex">Индекс столбца</param>
      /// <returns>Созданная закладка</returns>
      public BRBookmark Add(string name, int rowIndex, int columnIndex)
      {
        BRBookmark bookmark = new BRBookmark(name, _Owner, rowIndex, columnIndex);
        _Owner.Report.Bookmarks.Add(bookmark);
        return bookmark;
      }

      /// <summary>
      /// Добавляет закладку в отчет
      /// </summary>
      /// <param name="name">Имя закладки</param>
      /// <returns>Созданная закладка</returns>
      public BRBookmark Add(string name)
      {
        return Add(name, 0, 0);
      }

      #endregion
    }

    /// <summary>
    /// Свойство для добавления закладок
    /// </summary>
    public BookmarkCollection Bookmarks { get { return new BookmarkCollection(this); } }


    /// <summary>
    /// Список закладок, относящихся к полосе.
    /// Пополняется из BRReport.BookMarks
    /// </summary>
    internal List<BRBookmark> BookmarkList;

    #endregion

    #region Дополнительные методы и свойства

    /// <summary>
    /// Возвращает true, если у всех ячеек границы <see cref="BRCellStyle.LeftBorder"/>, <see cref="BRCellStyle.TopBorder"/>,
    /// <see cref="BRCellStyle.BottomBorder"/> и <see cref="BRCellStyle.RightBorder"/> установлены в одинаковые значения 
    /// (в том числе, границ нет). Диагональные линии не учитываются.
    /// Вычисление свойство является затратным
    /// </summary>
    /// <returns>Признак совпадения границ</returns>
    public bool AreAllBordersSame
    {
      get
      {
        BRSelector sel = CreateSelector();
        BRLine firstCellBorders = new BRLine();
        for (int i = 0; i < RowCount; i++)
        {
          sel.RowIndex = i;
          for (int j = 0; j < ColumnCount; j++)
          {
            sel.ColumnIndex = j;
            if (!sel.CellStyle.AreaAllBordersSame)
              return false;

            if (i == 0 && j == 0)
              firstCellBorders = sel.CellStyle.LeftBorder;
            else if (sel.CellStyle.LeftBorder != firstCellBorders)
              return false;
          }
        }
        return true;
      }
    }

    /// <summary>
    /// Возвращает true, если для полосы есть хотя бы одна гиперссылка
    /// </summary>
    public bool HasLinks
    {
      get
      {
        BRSelector sel = CreateSelector();
        for (int i = 0; i < RowCount; i++)
        {
          sel.RowIndex = i;
          for (int j = 0; j < ColumnCount; j++)
          {
            sel.ColumnIndex = j;
            if (sel.HasLink)
              return true;
          }
        }
        return false;
      }
    }

    #endregion
  }

  /// <summary>
  /// Описание прямоугольного блока ячеек в полосе. Не содержит ссылки на полосу.
  /// Структура однократной записи.
  /// </summary>
  public struct BRRange
  {
    #region Конструктор

    /// <summary>
    /// Инициализация структуры
    /// </summary>
    /// <param name="firstRowIndex">Индекс первой строки</param>
    /// <param name="firstColumnIndex">Индекс первого столбца</param>
    /// <param name="rowCount">Количество строк</param>
    /// <param name="columnCount">Количество столбцов</param>
    public BRRange(int firstRowIndex, int firstColumnIndex, int rowCount, int columnCount)
    {
      if (rowCount < 1)
        throw ExceptionFactory.ArgOutOfRange("rowCount", rowCount, 1, null);
      if (columnCount < 1)
        throw ExceptionFactory.ArgOutOfRange("columnCount", columnCount, 1, null);

      _FirstRowIndex = firstRowIndex;
      _FirstColumnIndex = firstColumnIndex;
      _RowCount = rowCount;
      _ColumnCount = columnCount;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Индекс первой строки
    /// </summary>
    public int FirstRowIndex { get { return _FirstRowIndex; } }
    private readonly int _FirstRowIndex;

    /// <summary>
    /// Индекс первого столбца
    /// </summary>
    public int FirstColumnIndex { get { return _FirstColumnIndex; } }
    private readonly int _FirstColumnIndex;

    /// <summary>
    /// Количество строк
    /// </summary>
    public int RowCount { get { return _RowCount; } }
    private readonly int _RowCount;

    /// <summary>
    /// Количество столбцов
    /// </summary>
    public int ColumnCount { get { return _ColumnCount; } }
    private readonly int _ColumnCount;

    /// <summary>
    /// Индекс последней строки
    /// </summary>
    public int LastRowIndex { get { return _FirstRowIndex + _RowCount - 1; } }

    /// <summary>
    /// Индекс последнего столбца
    /// </summary>
    public int LastColumnIndex { get { return _FirstColumnIndex + _ColumnCount - 1; } }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("Row=");
      sb.Append(FirstRowIndex);
      if (RowCount > 0)
      {
        sb.Append("-");
        sb.Append(LastRowIndex);
      }
      sb.Append(", Column=");
      sb.Append(FirstColumnIndex);
      if (ColumnCount > 0)
      {
        sb.Append("-");
        sb.Append(LastColumnIndex);
      }
      return sb.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Хранит значение (любого вида, не обязательно строку) и ссылку.
  /// Ссылка должна быть общепринятого вида ("https://", "mailto://"), чтобы по ней мог выполняться переход из
  /// браузера, Excel/Word/Calc/Writer.
  /// Когда отчет выводится на экран или на печать, ссылка не используется.
  /// Может использоваться в качестве значения свойства <see cref="BRSelector.Value"/> и связанных методах.
  /// </summary>
  [Serializable]
  public sealed class BRValueWithLink : IFormattable
  {
    // Нет смысла делать структурой, так как всегда будет боксинг

    #region Конструктор

    /// <summary>
    /// Создает объект, у которого значение и ссылка могут не совпадать
    /// </summary>
    /// <param name="value">Значение. Не может быть null</param>
    /// <param name="linkData">Ссылка. Не может быть пустой строкой</param>
    public BRValueWithLink(object value, string linkData)
    {
      if (value == null)
        throw new ArgumentNullException("value");
      if (String.IsNullOrEmpty(linkData))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("linkData");

      if (linkData[0] == '#')
      {
        string errorText;
        if (!BRBookmark.IsValidBookmarkName(linkData.Substring(1), out errorText))
          throw new ArgumentException(errorText, "linkData");
      }
      else
        new Uri(linkData); // проверка значения

      if (value is BRValueWithLink) // попытка вложенного создания
        value = ((BRValueWithLink)value).Value;

      _Value = value;
      _LinkData = linkData;
    }

    /// <summary>
    /// Создает объект с текстовым значением, которое совпадает с ссылкой.
    /// Обычно используется для интернет-ссылок
    /// </summary>
    /// <param name="linkData">Значение и ссылка. Не может быть пустой строкой.</param>
    public BRValueWithLink(string linkData)
      : this(linkData, linkData)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Отображаемое значение
    /// </summary>
    public object Value { get { return _Value; } }
    private readonly object _Value;

    /// <summary>
    /// Ссылка
    /// </summary>
    public string LinkData { get { return _LinkData; } }
    private string _LinkData;

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Вызывает <see cref="Object.ToString()"/> для значения <see cref="Value"/>.
    /// Форматирование не используется.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Value.ToString();
    }

    /// <summary>
    /// Если <see cref="Value"/> поддерживает форматирование, то вызывается <see cref="IFormattable.ToString(string, IFormatProvider)"/>.
    /// Иначе вызывается <see cref="Object.ToString()"/> без форматирования.
    /// </summary>
    /// <param name="format">Формат</param>
    /// <param name="formatProvider">Провайдер форматирования</param>
    /// <returns>Текстовое представление</returns>
    public string ToString(string format, IFormatProvider formatProvider)
    {
      IFormattable v2 = _Value as IFormattable;
      if (v2 == null)
        return _Value.ToString();
      else
        return v2.ToString(format, formatProvider);
    }

    #endregion

    #region Статические методы создания

    /// <summary>
    /// Создает ссылку "file://" на файл или каталог
    /// </summary>
    /// <param name="path">Путь к файлу или каталогу</param>
    /// <param name="text">Текстовое значение. 
    /// Если не задано, используется имя файла (каталога) из пути<paramref name="path"/>.</param>
    /// <returns>Ссылочное значение или null</returns>
    public static BRValueWithLink FromPath(FreeLibSet.IO.AbsPath path, string text)
    {
      if (path.IsEmpty)
        return null;
      if (String.IsNullOrEmpty(text))
      {
        text = path.FileName;
        if (String.IsNullOrEmpty(text))
          text = path.Path;
      }

      return new BRValueWithLink(text, path.UriString);
    }

    /// <summary>
    /// Создает ссылку "file://" на файл или каталог.
    /// В качестве <see cref="BRValueWithLink.Value"/> используется имя файла (каталога) без пути.
    /// </summary>
    /// <param name="path">Путь к файлу или каталогу</param>
    /// <returns>Ссылочное значение или null</returns>
    public static BRValueWithLink FromPath(FreeLibSet.IO.AbsPath path)
    {
      return FromPath(path, String.Empty);
    }

    /// <summary>
    /// Возвращает ссылку "mailto://" для адреса электронной почты
    /// </summary>
    /// <param name="address">адрес электронной почты</param>
    /// <returns>Ссылка или null</returns>
    public static BRValueWithLink FromEMail(string address)
    {
      if (String.IsNullOrEmpty(address))
        return null;

      return new BRValueWithLink(address, "mailto://" + address);
    }

    #endregion
  }

  /// <summary>
  /// Закладка в отчете, на которую может быть сделана ссылка (например, для оглавления).
  /// Закладка задается для ячейки.
  /// Закладка имеет имя, состоящее из латинских букв, цифр и знака подчеркивания. Имя должно начинаться с буквы.
  /// Имя не может начинаться или заканчиваться подчеркиванием, или содержать два подчеркивания подряд.
  /// Имя закладки не начинается со знака "#", а ссылка - начинается.
  /// Имена должны быть уникальными в пределах <see cref="BRReport"/> (без учета регистра). Имена в пределах листа Excel не поддерживаются.
  /// Закладки добавляются в список <see cref="BRReport.Bookmarks"/> или могут быть заданы непосредственно в объекте <see cref="BRBand"/>.
  /// </summary>
  public sealed class BRBookmark : IObjectWithCode
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект закладки
    /// </summary>
    /// <param name="name">Имя закладки</param>
    /// <param name="band">Полоса, в которой располагается закладка</param>
    /// <param name="rowIndex">Индекс строки ячейки в пределах <paramref name="band"/></param>
    /// <param name="columnIndex">Индекс столбца ячейки в пределах <paramref name="band"/></param>
    public BRBookmark(string name, BRBand band, int rowIndex, int columnIndex)
    {
      if (String.IsNullOrEmpty(name))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("name");

      string errorText;
      if (!IsValidBookmarkName(name, out errorText))
        throw new ArgumentException(errorText, "name");

      if (band == null)
        throw new ArgumentNullException("band");

      if (rowIndex < 0 || rowIndex >= band.RowCount)
        throw ExceptionFactory.ArgOutOfRange("rowIndex", rowIndex, 0, band.RowCount - 1);
      if (columnIndex < 0 || columnIndex >= band.ColumnCount)
        throw ExceptionFactory.ArgOutOfRange("columnIndex", columnIndex, 0, band.ColumnCount - 1);

      _Name = name;
      _Band = band;
      _RowIndex = rowIndex;
      _ColumnIndex = columnIndex;
    }

    /// <summary>
    /// Создает объект закладки на первую ячейку полосы
    /// </summary>
    /// <param name="name">Имя закладки</param>
    /// <param name="band">Полоса, в которой располагается закладка</param>
    public BRBookmark(string name, BRBand band)
      : this(name, band, 0, 0)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя закладки (без ведущего символа "#")
    /// </summary>
    public string Name { get { return _Name; } }
    private readonly string _Name;

    string IObjectWithCode.Code { get { return _Name; } }

    /// <summary>
    /// Полоса, в которой располагается закладка
    /// </summary>
    public BRBand Band { get { return _Band; } }
    private readonly BRBand _Band;

    /// <summary>
    /// Индекс строки
    /// </summary>
    public int RowIndex { get { return _RowIndex; } }
    private readonly int _RowIndex;

    /// <summary>
    /// Индекс столбца
    /// </summary>
    public int ColumnIndex { get { return _ColumnIndex; } }
    private readonly int _ColumnIndex;

    /// <summary>
    /// Возвращает <see cref="Name"/>
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Name;
    }

    /// <summary>
    /// Ссылка на отчет
    /// </summary>
    public BRReport Report { get { return _Band.Report; } }

    #endregion

    #region Проверка корректности имени

    /// <summary>
    /// Проверка корректности имени закладки
    /// </summary>
    /// <param name="name">Проверяемое имя</param>
    /// <param name="errorText">Сюда записывается сообщение об ошибке</param>
    /// <returns>true, если имя корректное</returns>
    public static bool IsValidBookmarkName(string name, out string errorText)
    {
      if (String.IsNullOrEmpty(name))
      {
        errorText = Res.BRBookmark_Err_NameIsEmpty;
        return false;
      }
      if (!Regex.IsMatch(name, "^[A-Za-z][A-Za-z0-9_]*$"))
      {
        errorText = String.Format(Res.BRBookmark_Err_NameIsInvalid, name);
        return false;
      }
      if (name.EndsWith("_", StringComparison.Ordinal) || name.IndexOf("__") >= 0)
      {
        errorText = String.Format(Res.BRBookmark_Err_NameUnderscore, name);
        return false;
      }

      errorText = null;
      return true;
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс селектора. Для обычной таблицы используется селектор <see cref="BRTableCellSelector"/>, а для виртуальной - <see cref="BRVirtualSelector"/>.
  /// Селектор создается вызовом <see cref="BRBand.CreateSelector()"/>
  /// Селектор в каждый момент времени ссылается на одну ячейку таблицы и позволяет читать (а для <see cref="BRTableCellSelector"/> и записывать) значения ячеек.
  /// Также дает доступ к актрибутам ячейки, параметрам строк и столбцов.
  /// </summary>
  public abstract class BRSelector
  {
    #region Конструктор

    /// <summary>
    /// Инициализация селектора.
    /// Первоначально селектор ссылается на первую ячейку таблицы
    /// </summary>
    /// <param name="band">Таблица, к которой относится селектор</param>
    protected BRSelector(BRBand band)
    {
      if (band == null)
        throw new ArgumentNullException("band");
      _Band = band;
    }

    /// <summary>
    /// Полоса, к которой относится селектор
    /// </summary>
    public BRBand Band { get { return _Band; } }
    private readonly BRBand _Band;

    #endregion

    #region Выбор строки и столбца

    /// <summary>
    /// Индекс текущей строки в диапазоне от 0 до (<see cref="BRBand.RowCount"/>-1)
    /// </summary>
    public int RowIndex
    {
      get { return _RowIndex; }
      set
      {
        if (value == _RowIndex)
          return;

        if (value < 0 || value >= _Band.RowCount)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, _Band.RowCount - 1);
        _RowIndex = value;

        OnRowIndexChanged();
      }
    }
    private int _RowIndex;

    /// <summary>
    /// Вызывается при изменении свойства <see cref="RowIndex"/>
    /// </summary>
    protected virtual void OnRowIndexChanged()
    {
    }

    /// <summary>
    /// Индекс текущего столбца в диапазоне от 0 до (<see cref="BRBand.ColumnCount"/>-1)
    /// </summary>
    public int ColumnIndex
    {
      get { return _ColumnIndex; }
      set
      {
        if (value == _ColumnIndex)
          return;
        if (value < 0 || value >= _Band.ColumnCount)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, _Band.ColumnCount - 1);
        _ColumnIndex = value;
        OnColumnIndexChanged();
      }
    }
    private int _ColumnIndex;

    /// <summary>
    /// Вызывается при изменении значения свойства <see cref="ColumnIndex"/>
    /// </summary>
    protected virtual void OnColumnIndexChanged()
    {
    }

    /// <summary>
    /// Одновременная установка текущей строки и столбца
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <param name="columnIndex"></param>
    public void Select(int rowIndex, int columnIndex)
    {
      this.RowIndex = rowIndex;
      this.ColumnIndex = columnIndex;
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "RowIndex=" + RowIndex + ", ColumnIndex=" + ColumnIndex;
    }

    #endregion

    #region Текущие данные - абстрактные свойства

    /// <summary>
    /// Текущее значение ячейки, выбранной в селекторе.
    /// Запись значения разрешена только для <see cref="BRTableCellSelector"/>.
    /// </summary>
    public abstract object Value { get; set; }

    /// <summary>
    /// Стиль выбранной ячейки
    /// </summary>
    public abstract BRCellStyle CellStyle { get; }

    /// <summary>
    /// Параметры строки, задаваемой свойством <see cref="RowIndex"/>
    /// </summary>
    public abstract BRRowInfo RowInfo { get; }

    /// <summary>
    /// Параметры столбца, задаваемого свойством <see cref="ColumnIndex"/>
    /// </summary>
    public abstract BRColumnInfo ColumnInfo { get; }

    /// <summary>
    /// Возвращает диапазон объединения, в который входит текущая выбранная ячейка.
    /// Если ячейка не входит в объединение, возвращает <see cref="BRRange"/> из одной ячейки
    /// </summary>
    public virtual BRRange MergeInfo
    {
      get
      {
        return new BRRange(RowIndex, ColumnIndex, 1, 1);
      }
    }

    #endregion

    #region Дополнительные свойства

    ///// <summary>
    ///// Возвращает значение как отформатированный массив строк.
    ///// Если <see cref="Value"/>==null, возвращается пустой массив строк.
    ///// </summary>
    //public string[] Lines
    //{
    //  get
    //  {
    //    if (Value == null)
    //      return DataTools.EmptyStrings;

    //    IFormattable v2 = Value as IFormattable;
    //    string s;
    //    if (v2 == null)
    //      s = Value.ToString();
    //    else
    //      s = v2.ToString(CellStyle.Format, CellStyle.FormatProvider);
    //    if (s.Length == 0)
    //      return DataTools.EmptyStrings;

    //    if (s.IndexOf(Environment.NewLine) < 0)
    //    {
    //      _SingleLineArray[0] = s;
    //      return _SingleLineArray;
    //    }
    //    else
    //      return s.Split(DataTools.NewLineSeparators, StringSplitOptions.None);
    //  }
    //}
    //private string[] _SingleLineArray;

    /// <summary>
    /// Возвращает текстовое представление для значения <see cref="Value"/> с учетом установленного форматирования.
    /// Если значение равно null, возвращает пустую строку.
    /// </summary>
    public string AsString
    {
      get
      {
        if (Value == null)
          return String.Empty;
        IFormattable v2 = Value as IFormattable;
        if (v2 == null)
          return Value.ToString();
        else
          return v2.ToString(CellStyle.Format, CellStyle.FormatProvider);
      }
    }

    /// <summary>
    /// Возвращает <see cref="BRValueWithLink.Value"/>, если в ячейке задана ссылка.
    /// Для обычных ячеек возвращает свойство <see cref="Value"/>.
    /// </summary>
    public object ActualValue
    {
      get
      {
        object v = Value;
        BRValueWithLink v2 = v as BRValueWithLink;
        if (v2 == null)
          return v;
        else
          return v2.Value;
      }
    }

    /// <summary>
    /// Возвращает <see cref="BRValueWithLink.LinkData"/>, если в ячейке задана ссылка.
    /// Для обычных ячеек возвращает пустую строку.
    /// </summary>
    public string LinkData
    {
      get
      {
        object v = Value;
        BRValueWithLink v2 = v as BRValueWithLink;
        if (v2 == null)
          return String.Empty;
        else
          return v2.LinkData;
      }
    }

    /// <summary>
    /// Возвращает true, если в ячейке задана ссылка.
    /// </summary>
    public bool HasLink { get { return Value is BRValueWithLink; } }

    /// <summary>
    /// Возвращает реальное выравнивание по горизонтали для случая <see cref="CellStyle"/>.HAlign=<see cref="BRHAlign.Auto"/>, в зависимости от типа значения <see cref="Value"/>
    /// </summary>
    public BRHAlign ActualHAlign
    {
      get
      {
        BRHAlign res = CellStyle.HAlign;
        if (res != BRHAlign.Auto)
          return res;

        object v = Value;
        if (v is BRValueWithLink)
          v = ((BRValueWithLink)v).Value;

        if (v == null)
          return BRHAlign.Left;
        Type t = v.GetType();
        if (DataTools.IsNumericType(t))
          return BRHAlign.Right;
        if (t == typeof(DateTime) || t == typeof(bool))
          return BRHAlign.Center;
        return BRHAlign.Left;
      }
    }

    /// <summary>
    /// Возвращает true, если текущая ячейка не входит в объединение или является верхней левой ячейкой объединения
    /// </summary>
    public bool IsMainCell
    {
      get
      {
        BRRange r = MergeInfo;
        return r.FirstRowIndex == RowIndex && r.FirstColumnIndex == ColumnIndex;
      }
    }

    #endregion

    #region Закладки

    /// <summary>
    /// Реализация свойства <see cref="Bookmarks"/>
    /// </summary>
    public struct BookmarkCollection
    {
      #region Конструктор

      internal BookmarkCollection(BRSelector owner)
      {
        _Owner = owner;
      }

      private readonly BRSelector _Owner;

      #endregion

      #region Доступ к закладкам

      /// <summary>
      /// Возвращает массив закладок для текущей ячейки.
      /// Если закладок нет, возвращается пустой массив
      /// </summary>
      /// <returns>Массив закладок</returns>
      public BRBookmark[] ToArray()
      {
        return _Owner.Band.Bookmarks[_Owner.RowIndex, _Owner.ColumnIndex];
      }

      #endregion

      #region Добавление закладки

      /// <summary>
      /// Добавляет закладку на текущую ячейку
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      public BRBookmark Add(string name)
      {
        return _Owner.Band.Bookmarks.Add(name, _Owner.RowIndex, _Owner.ColumnIndex);
      }

      #endregion
    }

    /// <summary>
    /// Доступ к закладкам, относящимся к ячейке
    /// </summary>
    public BookmarkCollection Bookmarks { get { return new BookmarkCollection(this); } }

    #endregion
  }

  /// <summary>
  /// Ориентация бумаги
  /// </summary>
  public enum BROrientation
  {
    /// <summary>
    /// Портретная
    /// </summary>
    Portrait = 0,

    /// <summary>
    /// Альбомная
    /// </summary>
    Landscape = 1
  }

  /// <summary>
  /// Параметры страницы для секции <see cref="BRSection"/>.
  /// Также класс реализует чтение/запись значений в секцию конфигурации.
  /// </summary>
  public sealed class BRPageSetup : ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// Конструктор для <see cref="BRReport.AppDefaultPageSetup"/>
    /// </summary>
    /// <param name="dummy">Не используется</param>
    internal BRPageSetup(bool dummy)
    {
      _PaperWidth = 2100;
      _PaperHeight = 2970;
      _Orientation = BROrientation.Portrait;
      _CenterVertical = false;
      _CenterHorizontal = false;
      _LeftMargin = 200;
      _TopMargin = 100;
      _RightMargin = 100;
      _BottomMargin = 170;
      _DuplexNewPage = false;
    }

    /// <summary>
    /// Создает новый объект и копирует в него стандартные параметры страницы из <see cref="BRReport.AppDefaultPageSetup"/>
    /// </summary>
    public BRPageSetup()
      : this(BRReport.AppDefaultPageSetup)
    {
    }

    private BRPageSetup(BRPageSetup src)
    {
      _PaperWidth = src.PaperWidth;
      _PaperHeight = src.PaperHeight;
      _Orientation = src.Orientation;
      _CenterHorizontal = src.CenterHorizontal;
      _CenterVertical = src.CenterVertical;
      _LeftMargin = src.LeftMargin;
      _TopMargin = src.TopMargin;
      _RightMargin = src.RightMargin;
      _BottomMargin = src.BottomMargin;
      _DuplexNewPage = src.DuplexNewPage;
    }

    #endregion

    #region Размер бумаги

    /// <summary>
    /// Ширина бумаги в единицах 0,1 мм (с учетом текущей ориентации)
    /// </summary>
    public int PaperWidth { get { return _PaperWidth; } set { _PaperWidth = value; } }
    private int _PaperWidth;

    /// <summary>
    /// Высота бумаги в единицах 0,1 мм (с учетом текущей ориентации)
    /// </summary>
    public int PaperHeight { get { return _PaperHeight; } set { _PaperHeight = value; } }
    private int _PaperHeight;

    /// <summary>
    /// Ориентация - книжная или альбомная.
    /// Установка свойства не меняет размер бумаги и полей.
    /// Используйте методы <see cref="InvertOrientation()"/> или <see cref="SetOrientation(BROrientation, bool)"/>, чтобы переключить поля.
    /// По умолчанию - книжная ориентация.
    /// </summary>
    public BROrientation Orientation
    {
      get { return _Orientation; }
      set { _Orientation = value; }
    }
    private BROrientation _Orientation;

    /// <summary>
    /// Установить альбомную или портретную ориентацию
    /// </summary>
    /// <param name="value">Ориентация</param>
    /// <param name="changeSizes">Нужно ли обменять местами размеры бумаги и полей</param>
    public void SetOrientation(BROrientation value, bool changeSizes)
    {
      if (changeSizes)
      {
        if (value != Orientation)
          InvertOrientation();
      }
      else
        this.Orientation = value;
    }

    /// <summary>
    /// Меняет ориентацию бумаги с портретной на альбомную, синхронно меняя размер бумаги и поля
    /// </summary>
    public void InvertOrientation()
    {
      if (Orientation == BROrientation.Portrait)
        Orientation = BROrientation.Landscape;
      else
        Orientation = BROrientation.Portrait;
      int tmp = _PaperWidth;
      _PaperWidth = _PaperHeight;
      _PaperHeight = tmp;

      if (_Orientation == BROrientation.Landscape)
      {
        // по часовой стрелке
        tmp = _TopMargin;
        _TopMargin = _LeftMargin;
        _LeftMargin = _BottomMargin;
        _BottomMargin = _RightMargin;
        _RightMargin = tmp;
      }
      else
      {
        // против часовой стрелки
        tmp = _TopMargin;
        _TopMargin = _RightMargin;
        _RightMargin = _BottomMargin;
        _BottomMargin = _LeftMargin;
        _LeftMargin = tmp;
      }
    }

    ///// <summary>
    ///// Формат бумаги (только для чтения)
    ///// </summary>
    //public PaperKind PaperKind
    //{
    //  get
    //  {
    //    return PaperDocPageSetup.PaperSizeToPaperKind(PaperWidth, _PaperHeight, Landscape);
    //  }
    //}

    #endregion

    #region Центрирование

    /// <summary>
    /// Если true, то изображение будет центрировано на странице по горизонтали, если оно не занимает всю страницу целиком
    /// </summary>
    public bool CenterHorizontal { get { return _CenterHorizontal; } set { _CenterHorizontal = value; } }
    private bool _CenterHorizontal;

    /// <summary>
    /// Если true, то изображение будет центрировано на странице по горизонтали, если оно не занимает всю страницу целиком
    /// </summary>
    public bool CenterVertical { get { return _CenterVertical; } set { _CenterVertical = value; } }
    private bool _CenterVertical;

    #endregion

    #region Отступы

    /// <summary>
    /// Левое поле в единицах 0.1 мм 
    /// </summary>
    public int LeftMargin { get { return _LeftMargin; } set { _LeftMargin = value; } }
    private int _LeftMargin;

    /// <summary>
    /// Верхнее поле в единицах 0.1 мм 
    /// </summary>
    public int TopMargin { get { return _TopMargin; } set { _TopMargin = value; } }
    private int _TopMargin;

    /// <summary>
    /// Правое поле в единицах 0.1 мм 
    /// </summary>
    public int RightMargin { get { return _RightMargin; } set { _RightMargin = value; } }
    private int _RightMargin;

    /// <summary>
    /// Нижнее поле в едницах 0.1 мм 
    /// </summary>
    public int BottomMargin { get { return _BottomMargin; } set { _BottomMargin = value; } }
    private int _BottomMargin;

    /// <summary>
    /// Высота доступной области для печати (высота страниы минус верхнее и нижнее поле)
    /// в единицах 0.1 мм
    /// </summary>
    public int PrintAreaHeight
    {
      get
      {
        return PaperHeight - TopMargin - BottomMargin;
      }
    }

    /// <summary>
    /// Ширина доступной области для печати (ширина страницы минус левое и правое поле)
    /// в единицах 0.1 мм
    /// </summary>
    public int PrintAreaWidth
    {
      get
      {
        return PaperWidth - LeftMargin - RightMargin;
      }
    }


    #endregion

    #region Другие свойства

    /// <summary>
    /// При двусторонней печати - признак печати с новой страницы, а не на обороте
    /// По умолчанию - false
    /// Используется при объединении нескольких документов в один
    /// </summary>
    public bool DuplexNewPage { get { return _DuplexNewPage; } set { _DuplexNewPage = value; } }
    private bool _DuplexNewPage;

    #endregion

    #region Клонирование

    /// <summary>
    /// Создает копию объекта
    /// </summary>
    /// <returns></returns>
    public BRPageSetup Clone()
    {
      return new BRPageSetup(this);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }

  /// <summary>
  /// Реализация набора хранимых данных <see cref="SettingsDataItem"/> для параметров страницы отчета <see cref="BRPageSetup"/>
  /// </summary>
  public sealed class BRPageSettingsDataItem : SettingsDataItem, ICloneable
  {
    #region Конструктор

    /// <summary>
    /// Создает объект для заданного внешнего <see cref="BRPageSetup"/> 
    /// </summary>
    /// <param name="pageSetup">Параметры страницы</param>
    public BRPageSettingsDataItem(BRPageSetup pageSetup)
    {
      if (pageSetup == null)
        throw new ArgumentNullException("pageSetup");
      _PageSetup = pageSetup;
    }

    /// <summary>
    /// Создает объект и новый экземпляр <see cref="BRPageSetup"/>.
    /// </summary>
    public BRPageSettingsDataItem()
      : this(new BRPageSetup())
    {
    }

    /// <summary>
    /// Параметры страницы, в которых хранятся значения
    /// </summary>
    public BRPageSetup PageSetup { get { return _PageSetup; } }
    private readonly BRPageSetup _PageSetup;

    #endregion

    #region ISettingsDataItem

    /// <summary>
    /// Возвращает <see cref="SettingsPart.User"/>
    /// </summary>
    public override SettingsPart UsedParts { get { return SettingsPart.User; } }

    /// <summary>
    /// Запись значений в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Заполняемая секция</param>
    /// <param name="part">Должно быть <see cref="SettingsPart.User"/></param>
    public override void WriteConfig(CfgPart cfg, SettingsPart part)
    {
      if (part == SettingsPart.User)
      {
        cfg.SetBool("Landscape", _PageSetup.Orientation == BROrientation.Landscape);
        cfg.SetInt("PaperWidth", _PageSetup.PaperWidth);
        cfg.SetInt("PaperHeight", _PageSetup.PaperHeight);
        cfg.SetBool("CenterVertical", _PageSetup.CenterVertical);
        cfg.SetBool("CenterHorizontal", _PageSetup.CenterHorizontal);
        cfg.SetInt("LeftMargin", _PageSetup.LeftMargin);
        cfg.SetInt("TopMargin", _PageSetup.TopMargin);
        cfg.SetInt("RightMargin", _PageSetup.RightMargin);
        cfg.SetInt("BottomMargin", _PageSetup.BottomMargin);
        cfg.SetBool("DuplexNewPage", _PageSetup.DuplexNewPage);
      }
    }

    /// <summary>
    /// Чтение значений из секции конфигурации
    /// </summary>
    /// <param name="cfg">Секция для чтения</param>
    /// <param name="part">Должно быть <see cref="SettingsPart.User"/></param>
    public override void ReadConfig(CfgPart cfg, SettingsPart part)
    {
      if (part == SettingsPart.User)
      {
        _PageSetup.Orientation = cfg.GetBoolDef("Landscape", BRReport.AppDefaultPageSetup.Orientation == BROrientation.Landscape) ? BROrientation.Landscape : BROrientation.Portrait;
        _PageSetup.PaperWidth = cfg.GetIntDef("PaperWidth", BRReport.AppDefaultPageSetup.PaperWidth);
        _PageSetup.PaperHeight = cfg.GetIntDef("PaperHeight", BRReport.AppDefaultPageSetup.PaperHeight);
        _PageSetup.CenterVertical = cfg.GetBoolDef("CenterVertical", BRReport.AppDefaultPageSetup.CenterVertical);
        _PageSetup.CenterHorizontal = cfg.GetBoolDef("CenterHorizontal", BRReport.AppDefaultPageSetup.CenterHorizontal);
        _PageSetup.LeftMargin = cfg.GetIntDef("LeftMargin", BRReport.AppDefaultPageSetup.LeftMargin);
        _PageSetup.TopMargin = cfg.GetIntDef("TopMargin", BRReport.AppDefaultPageSetup.TopMargin);
        _PageSetup.RightMargin = cfg.GetIntDef("RightMargin", BRReport.AppDefaultPageSetup.RightMargin);
        _PageSetup.BottomMargin = cfg.GetIntDef("BottomMargin", BRReport.AppDefaultPageSetup.BottomMargin);
        _PageSetup.DuplexNewPage = cfg.GetBoolDef("DuplexNewPage", BRReport.AppDefaultPageSetup.DuplexNewPage);
      }
    }

    #endregion

    #region Clone()

    /// <summary>
    /// Создает копию объекта
    /// </summary>
    /// <returns>Новый объект</returns>
    public BRPageSettingsDataItem Clone()
    {
      BRPageSettingsDataItem res = new BRPageSettingsDataItem(this.PageSetup.Clone());
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }
}
