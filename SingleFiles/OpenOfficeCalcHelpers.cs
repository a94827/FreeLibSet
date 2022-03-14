// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.ComponentModel;
using FreeLibSet.IO;
using System.Diagnostics;
using FreeLibSet.Core;
using FreeLibSet.Shell;

/*
 * Excel-подобная модель для работы с  OpenOffice / LibreOffice Calc
 * 
 * Этот модуль не входит в ExtForms.dll, т.к. требует компиляции библиотек cli_*.dll,
 * входящих в состав OpenOffice SDK
 * 
 * Содержит набор структур-оберток для классов CLI, таких, как unoidl.com.sun.star.sheet.XSpreadsheet.
 * Структуры содержат ссылки на оригинальные интнерфейсы. Такиен свойства имеют имена, начинаюшиеся на "X".
 * Каждая структура имеет свойство Exists, которое возвращает true, если структура была инициализирована.
 * Если у структуры нет ссылки на "свой" интерфейс, свойство возвращает false
 * 
 * Структуры-обертки имеют свойства только для чтения и методы SetXXX() для установки значений. 
 * Использовать свойства для установки значений нельзя, т.к будет ошибка компилации
 * CS1612: Cannot modify the return value of 'expression' because it is not a variable
 */

namespace unoidl.com.sun.star.sheet
{
  /// <summary>
  /// Расширенный интерфейс для LibreOffice
  /// </summary>
  public interface XSpreadsheets2 : XSpreadsheets
  {
    int importSheet(com.sun.star.sheet.XSpreadsheetDocument srcDoc, string srcName, int nDestPosition);
  }
}


namespace FreeLibSet.OpenOffice.Calc
{
  /// <summary>
  /// Доступ к книге (документу ODS).
  /// Основной интерфейс - XSpreadsheetDocument
  /// </summary>
  public struct Workbook
  {
    #region Конструктор

    /// <summary>
    /// Открывает существующий ODS-файл
    /// </summary>
    /// <param name="application"></param>
    /// <param name="odsPath"></param>
    /// <param name="readOnly"></param>
    public Workbook(OpenOfficeApplication application, AbsPath odsPath, bool readOnly)
    {
      if (application == null)
        throw new ArgumentNullException("application");
      application.CheckNotDisposed();
      if (odsPath.IsEmpty)
        throw new ArgumentException("Путь не задан", "odsPath");

      unoidl.com.sun.star.beans.PropertyValue[] props = new unoidl.com.sun.star.beans.PropertyValue[2];
      props[0] = new unoidl.com.sun.star.beans.PropertyValue("ReadOnly", 0, new uno.Any(readOnly), unoidl.com.sun.star.beans.PropertyState.DIRECT_VALUE);
      props[1] = new unoidl.com.sun.star.beans.PropertyValue("Hidden", 0, new uno.Any(!application.Visible), unoidl.com.sun.star.beans.PropertyState.DIRECT_VALUE);

      unoidl.com.sun.star.lang.XComponent xComponent = application.XComponentLoader.loadComponentFromURL(
        odsPath.Uri.ToString(), "_blank", 0,
        props);

      _XSpreadsheetDocument = (unoidl.com.sun.star.sheet.XSpreadsheetDocument)xComponent;
    }

    public Workbook(unoidl.com.sun.star.sheet.XSpreadsheetDocument xSpreadsheetDocument)
    {
      if (xSpreadsheetDocument == null)
        throw new ArgumentNullException("xSpreadsheetDocument");

      _XSpreadsheetDocument = xSpreadsheetDocument;
    }

    /// <summary>
    /// Создает пустой файл с заданным числом листов
    /// </summary>
    /// <param name="application"></param>
    /// <param name="sheetCount"></param>
    public Workbook(OpenOfficeApplication application, int sheetCount)
    {
      if (application == null)
        throw new ArgumentNullException("application");
      application.CheckNotDisposed();
      if (sheetCount < 1)
        throw new ArgumentOutOfRangeException("sheetCount");

      unoidl.com.sun.star.beans.PropertyValue[] props = new unoidl.com.sun.star.beans.PropertyValue[1];
      props[0] = new unoidl.com.sun.star.beans.PropertyValue("Hidden", 0, new uno.Any(!application.Visible), unoidl.com.sun.star.beans.PropertyState.DIRECT_VALUE);


      unoidl.com.sun.star.lang.XComponent xComponent = application.XComponentLoader.loadComponentFromURL(
        "private:factory/scalc", "_blank", 0, props);

      _XSpreadsheetDocument = (unoidl.com.sun.star.sheet.XSpreadsheetDocument)xComponent;

      // TODO: SheetCount
    }

    #endregion

    #region Интерфейсы

    /// <summary>
    /// Открытая книга (ODS-документ)
    /// </summary>
    public unoidl.com.sun.star.sheet.XSpreadsheetDocument XSpreadsheetDocument { get { return _XSpreadsheetDocument; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetDocument _XSpreadsheetDocument;

    public unoidl.com.sun.star.util.XNumberFormatsSupplier XNumberFormatsSupplier
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.util.XNumberFormatsSupplier; } }

    public unoidl.com.sun.star.util.XCloseable XCloseable
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.util.XCloseable; } }

    public unoidl.com.sun.star.lang.XMultiServiceFactory XMultiServiceFactory
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.lang.XMultiServiceFactory; } }

    public unoidl.com.sun.star.frame.XModel XModel
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.frame.XModel; } }

    public unoidl.com.sun.star.util.XModifiable XModifiable
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.util.XModifiable; } }

    public unoidl.com.sun.star.frame.XStorable XStorable
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.frame.XStorable; } }

    public unoidl.com.sun.star.view.XPrintable XPrintable
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.view.XPrintable; } }

    public unoidl.com.sun.star.view.XPrintJobBroadcaster XPrintJobBroadcaster
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.view.XPrintJobBroadcaster; } }

    public unoidl.com.sun.star.document.XDocumentEventBroadcaster XDocumentEventBroadcaster
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.document.XDocumentEventBroadcaster; } }

    public unoidl.com.sun.star.document.XEventsSupplier XEventsSupplier
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.document.XEventsSupplier; } }

    public unoidl.com.sun.star.document.XDocumentInfoSupplier XDocumentInfoSupplier
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.document.XDocumentInfoSupplier; } }

    public unoidl.com.sun.star.document.XViewDataSupplier XViewDataSupplier
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.document.XViewDataSupplier; } }

    public unoidl.com.sun.star.util.XProtectable XProtectable
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.util.XProtectable; } }

    public unoidl.com.sun.star.document.XDocumentPropertiesSupplier XDocumentPropertiesSupplier
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.document.XDocumentPropertiesSupplier; } }

    public unoidl.com.sun.star.style.XStyleFamiliesSupplier XStyleFamiliesSupplier
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.style.XStyleFamiliesSupplier; } }

    public unoidl.com.sun.star.beans.XPropertySet XPropertySet { get { return _XSpreadsheetDocument as unoidl.com.sun.star.beans.XPropertySet; } }

    public unoidl.com.sun.star.document.XActionLockable XActionLockable { get { return _XSpreadsheetDocument as unoidl.com.sun.star.document.XActionLockable; } }

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XSpreadsheetDocument != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion

    #region Доступ к листам

    /// <summary>
    /// Коллекция листов (свойство Sheets)
    /// </summary>
    public struct WorksheetCollection : IEnumerable<Worksheet>
    {
      #region Защищенный конструктор

      internal WorksheetCollection(Workbook owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Интерфейсы

      public unoidl.com.sun.star.sheet.XSpreadsheets XSpreadsheets { get { return _Owner._XSpreadsheetDocument.getSheets(); } }

      public unoidl.com.sun.star.container.XIndexAccess XIndexAccess { get { return _Owner._XSpreadsheetDocument.getSheets() as unoidl.com.sun.star.container.XIndexAccess; } }

      public unoidl.com.sun.star.container.XEnumerationAccess XEnumerationAccess { get { return _Owner._XSpreadsheetDocument.getSheets() as unoidl.com.sun.star.container.XEnumerationAccess; } }

      /// <summary>
      /// Не понимаю, к чему дает доступ этот интерфейс
      /// </summary>
      public unoidl.com.sun.star.sheet.XCellRangesAccess XCellRangesAccess { get { return _Owner._XSpreadsheetDocument.getSheets() as unoidl.com.sun.star.sheet.XCellRangesAccess; } }

      /// <summary>
      /// Этот интерфейс есть только для LibreOffice
      /// </summary>
      public unoidl.com.sun.star.sheet.XSpreadsheets2 XSpreadsheets2 { get { return _Owner._XSpreadsheetDocument.getSheets() as unoidl.com.sun.star.sheet.XSpreadsheets2; } }

      #endregion

      #region Свойства

      private Workbook _Owner;

      /// <summary>
      /// Возвращает лист по индексу.
      /// Нумерация ведется с нуля
      /// </summary>
      /// <param name="index">Индекс листа в книге</param>
      /// <returns></returns>
      public Worksheet this[int index]
      {
        get
        {
          unoidl.com.sun.star.sheet.XSpreadsheet sht = XIndexAccess.getByIndex(index).Value as unoidl.com.sun.star.sheet.XSpreadsheet;
          return new Worksheet(sht, _Owner._XSpreadsheetDocument);
        }
      }

      /// <summary>
      /// Возвращает лист по имени.
      /// Если такого листа нет, вызывается исключение
      /// </summary>
      /// <param name="name">Имя листа</param>
      /// <returns></returns>
      public Worksheet this[string name]
      {
        get
        {
          if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException("name");
          unoidl.com.sun.star.sheet.XSpreadsheet sht = XSpreadsheets.getByName(name).Value as unoidl.com.sun.star.sheet.XSpreadsheet;
          if (sht == null)
            throw new ArgumentException("Книга не содержит листа с именем \"" + name + "\"");
          return new Worksheet(sht, _Owner._XSpreadsheetDocument);
        }
      }

      /// <summary>
      /// Возвращает количество листов
      /// </summary>
      public int Count
      {
        get
        {
          return XIndexAccess.getCount();
        }
      }

      #endregion

      #region Методы

      /// <summary>
      /// Возвращает true, если есть лист с таким именем
      /// </summary>
      /// <param name="name">Имя листа</param>
      /// <returns></returns>
      public bool Contains(string name)
      {
        if (String.IsNullOrEmpty(name))
          return false;

        return XSpreadsheets.hasByName(name);
      }

      /// <summary>
      /// Возвращает имена всех листов в книге.
      /// Порядок соответствует их расположению в книге
      /// </summary>
      /// <returns>Массив имен листов</returns>
      public string[] GetAllNames()
      {
        string[] a = new string[Count];
        for (int i = 0; i < a.Length; i++)
          a[i] = this[i].Name;
        return a;
      }

      /// <summary>
      /// Создает новый лист
      /// </summary>
      /// <returns>Новый лист</returns>
      public Worksheet Add()
      {
        XSpreadsheets.insertNewByName(String.Empty, Int16.MaxValue);
        return this[Count - 1];
      }

      /// <summary>
      /// Создает новый лист и вставляет его в указанную позицию
      /// </summary>
      /// <param name="index">Индекс в диапазоне (0 - Count-1) для вставки листа</param>
      /// <returns>Новый лист</returns>
      public Worksheet Insert(int index)
      {
        if (index < 0 || index >= Count)
          throw new ArgumentOutOfRangeException("index");
        XSpreadsheets.insertNewByName(String.Empty, (short)index);
        return this[index];
      }

      /// <summary>
      /// Удаление листа с заданным индексом.
      /// Нельзя удалить последний лист
      /// </summary>
      /// <param name="index">Индекс листа в диапазоне (0 - Count-1)</param>
      public void RemoveAt(int index)
      {
        //if (Index < 0 || Index >= Count)
        //  throw new ArgumentOutOfRangeException("Index");

        Worksheet sht = this[index];
        this.XSpreadsheets.removeByName(sht.Name);
      }

      /// <summary>
      /// Создает копию листа
      /// </summary>
      /// <param name="source">Исходный лист, относящийся, возможно, к другой книге</param>
      /// <returns>Новый лист</returns>
      public Worksheet AddCopy(Worksheet source)
      {
        source.CheckIfExists();
        if (XSpreadsheets2 == null)
          throw new NotSupportedException("Интерфейс XSpreadsheets2 не поддерживается");

        XSpreadsheets2.importSheet(source.Workbook.XSpreadsheetDocument, source.Name, Count);

        return this[Count - 1];
      }

      public override string ToString()
      {
        try
        {
          return String.Join(", ", GetAllNames());
        }
        catch (Exception e)
        {
          return "Ошибка при получении списка листов: " + e.Message;
        }
      }

      #endregion

      #region IEnumerable<Worksheet> Members

      public IEnumerator<Worksheet> GetEnumerator()
      {
        return new WorksheetEnumerator(this);
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return new WorksheetEnumerator(this);
      }

      #endregion
    }

    /// <summary>
    /// Перечислитель для листов книги
    /// </summary>
    private class WorksheetEnumerator : IEnumerator<Worksheet>
    {
      #region Конструктор

      public WorksheetEnumerator(WorksheetCollection sheets)
      {
        _Sheets = sheets;
        _Index = -1;
      }

      #endregion

      #region Свойства

      private WorksheetCollection _Sheets;

      private int _Index;

      #endregion

      #region IEnumerator<Worksheet> Members

      public Worksheet Current
      {
        get { return _Sheets[_Index]; }
      }

      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current
      {
        get { return _Sheets[_Index]; }
      }

      public bool MoveNext()
      {
        _Index++;
        return _Index < _Sheets.Count;
      }

      public void Reset()
      {
        _Index = -1;
      }

      #endregion
    }

    /// <summary>
    /// Доступ к листам книги
    /// </summary>
    public WorksheetCollection Sheets { get { return new WorksheetCollection(this); } }

    #endregion

    #region Доступ к именем ячеек

    public struct NameCollection
    {
      #region Защищенный конструктор

      internal NameCollection(Workbook owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Свойства

      private Workbook _Owner;

      /// <summary>
      /// Интерфейс XNamedRanges
      /// </summary>
      public unoidl.com.sun.star.sheet.XNamedRanges XNamedRanges
      {
        get
        {
          unoidl.com.sun.star.beans.XPropertySet xDocProps = (unoidl.com.sun.star.beans.XPropertySet)(_Owner._XSpreadsheetDocument);
          uno.Any aRangesObj = xDocProps.getPropertyValue("NamedRanges");

          unoidl.com.sun.star.sheet.XNamedRanges x = (unoidl.com.sun.star.sheet.XNamedRanges)(aRangesObj.Value);
          if (x == null)
            throw new NullReferenceException("Не удалось получить интерфейс XNamedRanges");
          return x;
        }
      }

      /// <summary>
      /// Возвращает диапазон ячеек для заданного имени.
      /// Если книга не содержит такого имени, вызывается исключение.
      /// Если не требуется вызов исключения, используйте метод GetIfExists()
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      public Range this[string name]
      {
        get
        {
          if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException("name");
          Range r = GetRangeIfExists(name);
          if (!r.Exists)
            throw new ArgumentException("Книга не содержит диапазона ячеек с именем \"" + name + "\"", "name");
          return r;
        }
      }

      /// <summary>
      /// Возвращает список имен через запятую
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
        try
        {
          return String.Join(", ", ToArray());
        }
        catch (Exception e)
        {
          return "Ошибка при получении списка имен: " + e.Message;
        }
      }

      #endregion

      #region Методы

      /// <summary>
      /// Возвращает массив всех имен, определенных для книги
      /// </summary>
      /// <returns></returns>
      public string[] ToArray()
      {
        return XNamedRanges.getElementNames();
      }

      /// <summary>
      /// Выполняет попытку найти диапазон ячеек, если есть диапазон с таким заданным именем.
      /// Если диапазона нет, возвращается неинициализированная структура
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      //[DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
      public Range GetRangeIfExists(string name)
      {
        if (XNamedRanges.hasByName(name))
        {
          unoidl.com.sun.star.sheet.XNamedRange nr = XNamedRanges.getByName(name).Value as unoidl.com.sun.star.sheet.XNamedRange;
          if (nr == null)
            return new Range(); // 29.01.2020

          unoidl.com.sun.star.sheet.XCellRangeReferrer crr = nr as unoidl.com.sun.star.sheet.XCellRangeReferrer;
          if (crr == null)
            return new Range(); // 10.10.2018

          unoidl.com.sun.star.table.XCellRange cr = crr.getReferredCells();
          if (cr == null)
            return new Range();

          return new Range(cr as unoidl.com.sun.star.sheet.XSheetCellRange, _Owner.XSpreadsheetDocument);
        }
        else
          return new Range();
      }

      /// <summary>
      /// Выполняет попытку найти ячейку ячеек, если есть диапазон с таким заданным именем.
      /// Возвращается первая ячейка диапазоная, если диапазон содержит несколько ячеек
      /// Если диапазона нет, возвращается неинициализированная структура
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      //[DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
      public Cell GetCellIfExists(string name)
      {
        if (XNamedRanges == null)
          throw new NullReferenceException("XNamedRanges==null");

        if (XNamedRanges.hasByName(name))
        {
          unoidl.com.sun.star.sheet.XNamedRange nr = XNamedRanges.getByName(name).Value as unoidl.com.sun.star.sheet.XNamedRange;
          if (nr == null)
            return new Cell(); // 14.01.2019
          else
          {
            // Это не то: unoidl.com.sun.star.table.CellAddress Addr= nr.getReferencePosition();
            //return FOwner[Addr];
            unoidl.com.sun.star.sheet.XCellRangeReferrer crr = nr as unoidl.com.sun.star.sheet.XCellRangeReferrer;
            if (crr == null)
              return new Cell(); // 14.01.2019
            else
            {
              unoidl.com.sun.star.table.XCellRange cr = crr.getReferredCells();
              if (cr == null)
                return new Cell(); // 25.01.2022
              unoidl.com.sun.star.table.XCell c = cr.getCellByPosition(0, 0);
              return new Cell(c, _Owner.XSpreadsheetDocument);
            }
          }
        }
        else
          return new Cell();
      }

      /// <summary>
      /// Удалить имя, если оно существует.
      /// Возвращает true, если имя существовало и было удалено
      /// </summary>
      /// <param name="name">Удаляемое имя</param>
      public bool Remove(string name)
      {
        if (XNamedRanges.hasByName(name))
        {
          XNamedRanges.removeByName(name);
          return true;
        }
        else
          return false;
      }

      /// <summary>
      /// Изменение имени ячейки
      /// </summary>
      /// <param name="oldName">Существующее имя</param>
      /// <param name="newName">Новое имя</param>
      public void Rename(string oldName, string newName)
      {
        unoidl.com.sun.star.sheet.XNamedRange nr = XNamedRanges.getByName(oldName).Value as unoidl.com.sun.star.sheet.XNamedRange;
        if (nr == null)
          throw new ArgumentException("Неизвестное имя: " + oldName);
        nr.setName(newName);
      }

      #endregion
    }

    /// <summary>
    /// Доступ к именам диапазонов ячеек, определенных на уровне книги
    /// </summary>
    public NameCollection Names { get { return new NameCollection(this); } }

    #endregion

    #region Сохранение и закрытие файла

    /// <summary>
    /// Сохраняет файл
    /// </summary>
    public void Save()
    {
      XStorable.store();
    }

    /// <summary>
    /// Сохраняет файл с указанным именем.
    /// Формат файла определяется из расширения
    /// </summary>
    /// <param name="newPath">Путь к файлу</param>
    public void SaveAs(AbsPath newPath)
    {
      string ext = newPath.Extension.ToUpperInvariant();
      string filterName;
      switch (ext)
      {
        case ".ODS": filterName = "calc8"; break;
        case ".XLS": filterName = "MS Excel 97"; break;
        default:
          throw new ArgumentException("Неизвестное расширение файла \"" + ext + "\"");
      }
      SaveAs(newPath, filterName);
    }

    /// <summary>
    /// Сохраняет файл с указанным именем.
    /// </summary>
    /// <param name="newPath">Путь к файлу</param>
    /// <param name="filterName">Имя фильтра, использумого для записи</param>
    private void SaveAs(AbsPath newPath, string filterName)
    {
      unoidl.com.sun.star.beans.PropertyValue[] props = new unoidl.com.sun.star.beans.PropertyValue[1];
      props[0] = new unoidl.com.sun.star.beans.PropertyValue();
      props[0].Name = "FilterName";
      props[0].Value = new uno.Any(filterName);
      XStorable.storeAsURL(newPath.Uri.ToString(), props);
    }


    /// <summary>
    /// Закрывает документ.
    /// После этого все полученные объекты, включая этот, становятся недействительными
    /// </summary>
    public void Close()
    {
      XCloseable.close(true);
    }

    /// <summary>
    /// Возвращает true, если документ содержит несохраненные изменения
    /// </summary>
    public bool Modified
    {
      get { return XModifiable.isModified(); }
    }

    /// <summary>
    /// Устанавливает признак наличия несохраненных изменений
    /// </summary>
    /// <param name="value">true - установить признак наличия изменений. false - сбросить признак</param>
    public void SetModified(bool value)
    {
      XModifiable.setModified(value);
    }

    /// <summary>
    /// Возвращает путь к книге.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      try
      {
        if (XStorable.hasLocation())
          return XStorable.getLocation();
        else
          return "Без имени";
      }
      catch (Exception e)
      {
        return "Ошибка при получении имени книги: " + e.Message;
      }
    }

    #endregion

    #region Свойства документа

    public string Title
    {
      get { return XDocumentPropertiesSupplier.getDocumentProperties().Title; }
    }
    public void SetTitle(string value)
    {
      XDocumentPropertiesSupplier.getDocumentProperties().Title = value;
    }

    public string Subject
    {
      get { return XDocumentPropertiesSupplier.getDocumentProperties().Subject; }
    }
    public void SetSubject(string value)
    {
      XDocumentPropertiesSupplier.getDocumentProperties().Subject = value;
    }

    public string Author
    {
      get { return XDocumentPropertiesSupplier.getDocumentProperties().Author; }
    }
    public void SetAuthor(string value)
    {
      XDocumentPropertiesSupplier.getDocumentProperties().Author = value;
    }

    #endregion

    #region Доступ к ячейкам

    /// <summary>
    /// Возвращает ячейку с заданным адресом
    /// </summary>
    /// <param name="address">Адрес ячейки</param>
    /// <returns>Ячейка</returns>
    public Cell this[unoidl.com.sun.star.table.CellAddress address]
    {
      get
      {
        Worksheet sht = Sheets[address.Sheet];
        return sht[address.Row, address.Column];
      }
    }

    /// <summary>
    /// Возвращает диапазон ячеек с заданным адресом
    /// </summary>
    /// <param name="address">Адрес диапазона</param>
    /// <returns>Диапазон</returns>
    public Range this[unoidl.com.sun.star.table.CellRangeAddress address]
    {
      get
      {
        Worksheet sht = Sheets[address.Sheet];
        return sht.GetRange(address.StartRow, address.StartColumn, address.EndRow, address.EndColumn);
      }
    }

    #endregion

    #region Защита книги

    /// <summary>
    /// Установить защиту книги
    /// </summary>
    /// <param name="password">Пароль (необязательный)</param>
    public void Protect(string password)
    {
      XProtectable.protect(password);
    }

    /// <summary>
    /// Снять защиту книги
    /// </summary>
    /// <param name="password">Пароль</param>
    public void Unprotect(string password)
    {
      XProtectable.unprotect(password);
    }

    /// <summary>
    /// Возвращает true, если книга защищен
    /// </summary>
    public bool IsProtected { get { return XProtectable.isProtected(); } }

    #endregion

    #region Доступ к контроллеру документа

    /// <summary>
    /// Возвращает текущий контроллер документа
    /// </summary>
    public SpreadsheetView CurrentController
    {
      get
      {
        unoidl.com.sun.star.frame.XController xController = XModel.getCurrentController();
        unoidl.com.sun.star.sheet.XSpreadsheetView xSpreadsheetView = xController as unoidl.com.sun.star.sheet.XSpreadsheetView;
        if (xSpreadsheetView == null)
          return new SpreadsheetView(); // не инициализированный объект
        else
          return new SpreadsheetView(xSpreadsheetView, XSpreadsheetDocument);
      }
    }

    #endregion

    #region Форматы ячеек

    /// <summary>
    /// Возвращает список всех числовых форматов ячеек
    /// </summary>
    /// <returns></returns>
    public IDictionary<int, NumberFormatProperties> GetAllNumberFormats()
    {
      return GetAllNumberFormats(new unoidl.com.sun.star.lang.Locale());
    }

    public IDictionary<int, NumberFormatProperties> GetAllNumberFormats(unoidl.com.sun.star.lang.Locale locale)
    {
      //unoidl.com.sun.star.lang.Locale Locale=new unoidl.com.sun.star.lang.Locale();
      //unoidl.com.sun.star.lang.Locale Locale = new unoidl.com.sun.star.lang.Locale("ru", "RU", "");
      int[] keys = XNumberFormatsSupplier.getNumberFormats().queryKeys(unoidl.com.sun.star.util.NumberFormat.ALL, locale, false);
      Dictionary<int, NumberFormatProperties> dict = new Dictionary<int, NumberFormatProperties>(keys.Length);
      for (int i = 0; i < keys.Length; i++)
      {
        unoidl.com.sun.star.beans.XPropertySet ps = XNumberFormatsSupplier.getNumberFormats().getByKey(keys[i]);
        dict.Add(keys[i], new NumberFormatProperties(ps, XSpreadsheetDocument));
      }
      return dict;
    }

    /// <summary>
    /// Выполнить замену числового формата всех ячейках документа
    /// </summary>
    /// <param name="oldNumberFormat">Существующий формат</param>
    /// <param name="newNumberFormat">Новый формат</param>
    /// <returns>Количество замен</returns>
    public int ReplaceNumberFormat(string oldNumberFormat, string newNumberFormat)
    {
      return ReplaceNumberFormat(oldNumberFormat, newNumberFormat,
        new unoidl.com.sun.star.lang.Locale());
    }

    public int ReplaceNumberFormat(string oldNumberFormat, string newNumberFormat, unoidl.com.sun.star.lang.Locale locale)
    {
      int oldKey = XNumberFormatsSupplier.getNumberFormats().queryKey(oldNumberFormat, locale, false);
      if (oldKey < 0)
        return 0;
      int newKey = XNumberFormatsSupplier.getNumberFormats().queryKey(newNumberFormat, locale, false);
      if (newKey < 0)
        newKey = XNumberFormatsSupplier.getNumberFormats().addNew(newNumberFormat, locale);

      return ReplaceNumberFormat(oldKey, newKey);
    }

    public int ReplaceNumberFormat(int oldKey, int newKey)
    {
      if (oldKey < 0)
        throw new ArgumentException("oldKey");
      if (newKey < 0)
        throw new ArgumentException("newKey");

      int res = 0;
      foreach (Worksheet sht in Sheets)
        res += sht.Range.ReplaceNumberFormat(oldKey, newKey);
      return res;
    }

    #endregion

    #region Активация приложения

    /// <summary>
    /// Активирует просмотр с книгой
    /// </summary>
    public void Activate()
    {
      // ?? Что надо выбрать
      unoidl.com.sun.star.frame.XFrame xFrame = XModel.getCurrentController().getFrame();
      if (xFrame != null)
      {
        unoidl.com.sun.star.awt.XWindow wnd1 = xFrame.getContainerWindow();
        if (wnd1 != null)
          wnd1.setFocus();
        unoidl.com.sun.star.awt.XWindow wnd2 = xFrame.getComponentWindow();
        if (wnd2 != null)
          wnd2.setFocus();
      }
    }

    #endregion
  }

  /// <summary>
  /// Лист книги Calc.
  /// Хранит интерфейс XSpreadsheet.
  /// Также храиит ссылку на интерфейс XSpreadsheetDocument, потому что ее нельзя получить
  /// </summary>
  public struct Worksheet
  {
    #region Конструктор

    public Worksheet(unoidl.com.sun.star.sheet.XSpreadsheet xSpreadsheet, unoidl.com.sun.star.sheet.XSpreadsheetDocument xSpreadsheetDocument)
    {
#if DEBUG
      if (xSpreadsheet == null)
        throw new ArgumentNullException("xSpreadsheet");
      if (xSpreadsheetDocument == null)
        throw new ArgumentNullException("xSpreadsheetDocument");
#endif
      _XSpreadsheet = xSpreadsheet;
      _XSpreadsheetDocument = xSpreadsheetDocument;
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.sheet.XSpreadsheet XSpreadsheet { get { return _XSpreadsheet; } }
    private unoidl.com.sun.star.sheet.XSpreadsheet _XSpreadsheet;

    public unoidl.com.sun.star.sheet.XSpreadsheetDocument XSpreadsheetDocument { get { return _XSpreadsheetDocument; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetDocument _XSpreadsheetDocument;

    //public unoidl.com.sun.star.table.XTable XTable { get { return FXSpreadsheet as unoidl.com.sun.star.table.XTable; } }

    public unoidl.com.sun.star.table.XColumnRowRange XColumnRowRange { get { return Range.XSheetCellRange as unoidl.com.sun.star.table.XColumnRowRange; } }

    public unoidl.com.sun.star.container.XNamed XNamed { get { return _XSpreadsheet as unoidl.com.sun.star.container.XNamed; } }

    public unoidl.com.sun.star.util.XProtectable XProtectable { get { return _XSpreadsheet as unoidl.com.sun.star.util.XProtectable; } }

    public unoidl.com.sun.star.sheet.XCellRangeMovement XCellRangeMovement { get { return _XSpreadsheet as unoidl.com.sun.star.sheet.XCellRangeMovement; } }

    public unoidl.com.sun.star.sheet.XPrintAreas XPrintAreas { get { return _XSpreadsheet as unoidl.com.sun.star.sheet.XPrintAreas; } }

    public unoidl.com.sun.star.sheet.XSheetPageBreak XSheetPageBreak { get { return _XSpreadsheet as unoidl.com.sun.star.sheet.XSheetPageBreak; } }

    public unoidl.com.sun.star.sheet.XSheetOutline XSheetOutline { get { return _XSpreadsheet as unoidl.com.sun.star.sheet.XSheetOutline; } }

    public unoidl.com.sun.star.beans.XPropertySet XPropertySet { get { return _XSpreadsheet as unoidl.com.sun.star.beans.XPropertySet; } }

    public unoidl.com.sun.star.sheet.XSheetAnnotationsSupplier XSheetAnnotationsSupplier { get { return _XSpreadsheet as unoidl.com.sun.star.sheet.XSheetAnnotationsSupplier; } }

    public unoidl.com.sun.star.drawing.XDrawPageSupplier XDrawPageSupplier { get { return _XSpreadsheet as unoidl.com.sun.star.drawing.XDrawPageSupplier; } }

    #endregion

    #region Свойства

    public Workbook Workbook { get { return new Workbook(_XSpreadsheetDocument); } }

    /// <summary>
    /// Возвращает или устанавливает имя листа
    /// </summary>
    public string Name { get { return XNamed.getName(); } }

    public void SetName(string value)
    {
      XNamed.setName(value);
    }

    /// <summary>
    /// Видимость листа
    /// </summary>
    public bool IsVisible
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("IsVisible").Value); }
    }
    public void SetVisible(bool value)
    {
      XPropertySet.setPropertyValue("IsVisible", new uno.Any(value));
    }

    /// <summary>
    /// Цвет ярлычка
    /// </summary>
    public Int32 TabColor
    {
      get { return DataTools.GetInt(XPropertySet.getPropertyValue("TabColor").Value); }
    }
    /// <summary>
    /// Установить цвет ярлычка
    /// </summary>
    /// <param name="value"></param>
    public void SetTabColor(Int32 value)
    {
      XPropertySet.setPropertyValue("TabColor", new uno.Any(value));
    }


    public override string ToString()
    {
      if (_XSpreadsheet == null)
        return "[ Empty ]";
      else
        return "$\'" + Name.Replace("\'", "\'\'") + "\'";
    }

    /// <summary>
    /// Объект для размещения управляющих элементов поверх листа
    /// </summary>
    public DrawPage DrawPage
    {
      get
      {
        return new DrawPage(XDrawPageSupplier.getDrawPage(), Workbook.XMultiServiceFactory);
      }
    }

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XSpreadsheet != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion

    #region Доступ к ячейкам

    /// <summary>
    /// Возвращает диапазон, содержащий все ячейки листа
    /// </summary>
    public Range Range
    {
      get
      {
        return new Range(_XSpreadsheet as unoidl.com.sun.star.sheet.XSheetCellRange, _XSpreadsheetDocument);
      }
    }

    /// <summary>
    /// Доступ к одиночной ячейке по индексу строки и столбца.
    /// Нумерация начинается с 0, а не с 1, как в Excel
    /// </summary>
    /// <param name="rowIndex">Индекс строки. Нумерация начинается с 0</param>
    /// <param name="columnIndex">Индекс столбца. Нумерация начинается с 0</param>
    /// <returns></returns>
    public Cell this[int rowIndex, int columnIndex]
    {
      get
      {
        unoidl.com.sun.star.sheet.XSheetCellRange scr = _XSpreadsheet as unoidl.com.sun.star.sheet.XSheetCellRange;
        if (scr == null)
          throw new NullReferenceException("XSheetCellRange==null");
        unoidl.com.sun.star.table.XCell c = scr.getCellByPosition(columnIndex, rowIndex);
        return new Cell(c, _XSpreadsheetDocument);
      }
    }


    /// <summary>
    /// Возвращает диапазон ячеек
    /// </summary>
    /// <param name="firstRowIndex">Индекс первой строки. Нумерация начинается с 0</param>
    /// <param name="firstColumnIndex">Индекс первого столбца. Нумерация начинается с 0</param>
    /// <param name="lastRowIndex">Индекс последней строки. Нумерация начинается с 0</param>
    /// <param name="lastColumnIndex">Индекс последнего столбца. Нумерация начинается с 0</param>
    /// <returns></returns>
    public Range GetRange(int firstRowIndex, int firstColumnIndex, int lastRowIndex, int lastColumnIndex)
    {
      unoidl.com.sun.star.sheet.XSheetCellRange scr = _XSpreadsheet as unoidl.com.sun.star.sheet.XSheetCellRange;
      unoidl.com.sun.star.table.XCellRange cr = scr.getCellRangeByPosition(firstColumnIndex, firstRowIndex, lastColumnIndex, lastRowIndex);
      return new Range(cr as unoidl.com.sun.star.sheet.XSheetCellRange, _XSpreadsheetDocument);
    }

    /// <summary>
    /// Возвращает диапазон ячеек по заданному адресу
    /// </summary>
    /// <param name="address">текстовое представление адреса</param>
    /// <returns></returns>
    public Range GetRange(string address)
    {
      unoidl.com.sun.star.sheet.XSheetCellRange scr = _XSpreadsheet as unoidl.com.sun.star.sheet.XSheetCellRange;
      unoidl.com.sun.star.table.XCellRange cr = scr.getCellRangeByName(address);
      return new Range(cr as unoidl.com.sun.star.sheet.XSheetCellRange, _XSpreadsheetDocument);
    }

    #endregion

    #region Строки и столбцы

    /// <summary>
    /// Коллекция строк диапазона ячеек
    /// </summary>
    public TableRows Rows { get { return new TableRows(XColumnRowRange.getRows(), _XSpreadsheetDocument); } }

    /// <summary>
    /// Коллекция столбцов диапазона ячеек
    /// </summary>
    public TableColumns Columns { get { return new TableColumns(XColumnRowRange.getColumns(), _XSpreadsheetDocument); } }

    /// <summary>
    /// Возвращает число строк на листе
    /// </summary>
    public int RowCount { get { return XColumnRowRange.getRows().getCount(); } }

    /// <summary>
    /// Вохвращает число столбцов на листе
    /// </summary>
    public int ColumnCount { get { return XColumnRowRange.getColumns().getCount(); } }

    /// <summary>
    /// Получить диапазон для целых строк
    /// </summary>
    /// <param name="firstRowIndex">Индекс первой строки. Нумерация начинается с 0</param>
    /// <param name="lastRowIndex">Индекс последней строки. Нумерация начинается с 0</param>
    /// <returns>Диапазон</returns>
    public Range GetRowsRange(int firstRowIndex, int lastRowIndex)
    {
      if (firstRowIndex < 0 || firstRowIndex >= RowCount)
        throw new ArgumentOutOfRangeException("firstRowIndex", firstRowIndex, "Неправильный индекс первой строки");
      if (lastRowIndex < 0 || lastRowIndex >= RowCount)
        throw new ArgumentOutOfRangeException("lastRowIndex", lastRowIndex, "Неправильный индекс последней строки");
      if (firstRowIndex > lastRowIndex)
        throw new ArgumentException("Индекс последней строки (" + lastRowIndex.ToString() + ") меньше, чем первой строки (" + firstRowIndex.ToString() + ")" + firstRowIndex.ToString(), "lastRowIndex");

      unoidl.com.sun.star.table.XCellRange cr = XSpreadsheet.getCellRangeByPosition(0, firstRowIndex, ColumnCount - 1, lastRowIndex);
      return new Range(cr as unoidl.com.sun.star.sheet.XSheetCellRange, _XSpreadsheetDocument);
    }

    /// <summary>
    /// Получить диапазон для целой строки
    /// </summary>
    /// <param name="rowIndex">Индекс строки. Нумерация начинается с 0</param>
    /// <returns>Диапазон</returns>
    public Range GetRowRange(int rowIndex)
    {
      return GetRowsRange(rowIndex, rowIndex);
    }

    /// <summary>
    /// Получить диапазон для целых столбцов
    /// </summary>
    /// <param name="firstColumnIndex">Индекс первого столбца. Нумерация начинается с 0</param>
    /// <param name="lastColumnIndex">Индекс последнего столбца. Нумерация начинается с 0</param>
    /// <returns>Диапазон</returns>
    public Range GetColumnsRange(int firstColumnIndex, int lastColumnIndex)
    {
      if (firstColumnIndex < 0 || firstColumnIndex >= ColumnCount)
        throw new ArgumentOutOfRangeException("firstColumnIndex", firstColumnIndex, "Неправильный индекс первого столбца");
      if (lastColumnIndex < 0 || lastColumnIndex >= ColumnCount)
        throw new ArgumentOutOfRangeException("lastColumnIndex", lastColumnIndex, "Неправильный индекс последней строки");
      if (firstColumnIndex > lastColumnIndex)
        throw new ArgumentException("Индекс последнего столбца (" + lastColumnIndex.ToString() + ") меньше, чем первого столбца (" + firstColumnIndex.ToString() + ")" + firstColumnIndex.ToString(), "lastColumnIndex");

      unoidl.com.sun.star.table.XCellRange cr = XSpreadsheet.getCellRangeByPosition(firstColumnIndex, 0, lastColumnIndex, RowCount - 1);
      return new Range(cr as unoidl.com.sun.star.sheet.XSheetCellRange, _XSpreadsheetDocument);
    }

    /// <summary>
    /// Получить диапазон для целого столбца
    /// </summary>
    /// <param name="columnIndex">Индекс столбца. Нумерация начинается с 0</param>
    /// <returns>Диапазон</returns>
    public Range GetColumnRange(int columnIndex)
    {
      return GetColumnsRange(columnIndex, columnIndex);
    }

    #endregion

    #region Доступ к именем ячеек

    public struct NameCollection
    {
      #region Защищенный конструктор

      internal NameCollection(Worksheet owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Свойства

      private Worksheet _Owner;

      /// <summary>
      /// Возвращает true, если поддерживаются имена на уровне отдельных листов
      /// </summary>
      public bool Exist { get { return XNamedRanges != null; } }

      /// <summary>
      /// Интерфейс XNamedRanges
      /// </summary>
      public unoidl.com.sun.star.sheet.XNamedRanges XNamedRanges
      {
        [DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
        get
        {
          // Можно было бы сначала проверить, есть ли такое свойство,
          // но проще выбросить исключение

          try
          {
            unoidl.com.sun.star.beans.XPropertySet xSheetProps = _Owner.XPropertySet;
            uno.Any aRangesObj = xSheetProps.getPropertyValue("NamedRanges");

            return (unoidl.com.sun.star.sheet.XNamedRanges)(aRangesObj.Value);
          }
          catch
          {
            return null;
          }
        }
      }

      /// <summary>
      /// Возвращает диапазон ячеек для заданного имени.
      /// Если лист не содержит такого имени, вызывается исключение.
      /// Если не требуется вызов исключения, используйте метод GetIfExists()
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      public Range this[string name]
      {
        get
        {
          if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException("name");
          Range r = GetRangeIfExists(name);
          if (!r.Exists)
            throw new ArgumentException("Лист не содержит диапазона ячеек с именем \"" + name + "\"", "name");
          return r;
        }
      }

      #endregion

      #region Методы

      /// <summary>
      /// Возвращает массив всех имен, определенных для книги
      /// </summary>
      /// <returns></returns>
      public string[] ToArray()
      {
        return XNamedRanges.getElementNames();
      }

      /// <summary>
      /// Выполняет попытку найти диапазон ячеек, если есть диапазон с таким заданным именем.
      /// Если диапазона нет, возвращается неинициализированная структура
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      //[DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
      public Range GetRangeIfExists(string name)
      {
        if (XNamedRanges.hasByName(name))
        {
          unoidl.com.sun.star.sheet.XNamedRange nr = XNamedRanges.getByName(name).Value as unoidl.com.sun.star.sheet.XNamedRange;
          unoidl.com.sun.star.sheet.XCellRangeReferrer crr = nr as unoidl.com.sun.star.sheet.XCellRangeReferrer;
          unoidl.com.sun.star.table.XCellRange cr = crr.getReferredCells();
          if (cr == null)
            return new Range();
          return new Range(cr as unoidl.com.sun.star.sheet.XSheetCellRange, _Owner.XSpreadsheetDocument);
        }
        else
          return new Range();
      }

      /// <summary>
      /// Выполняет попытку найти ячейку ячеек, если есть диапазон с таким заданным именем.
      /// Возвращается первая ячейка диапазоная, если диапазон содержит несколько ячеек
      /// Если диапазона нет, возвращается неинициализированная структура
      /// </summary>
      /// <param name="name"></param>
      /// <returns></returns>
      //[DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
      public Cell GetCellIfExists(string name)
      {
        if (XNamedRanges.hasByName(name))
        {
          unoidl.com.sun.star.sheet.XNamedRange nr = XNamedRanges.getByName(name).Value as unoidl.com.sun.star.sheet.XNamedRange;
          // Это не то: unoidl.com.sun.star.table.CellAddress Addr= nr.getReferencePosition();
          //return FOwner[Addr];
          unoidl.com.sun.star.sheet.XCellRangeReferrer crr = nr as unoidl.com.sun.star.sheet.XCellRangeReferrer;
          unoidl.com.sun.star.table.XCell c = crr.getReferredCells().getCellByPosition(0, 0);
          return new Cell(c, _Owner.XSpreadsheetDocument);
        }
        else
          return new Cell();
      }

      /// <summary>
      /// Удалить имя, если оно существует.
      /// Возвращает true, если имя существовало и было удалено
      /// </summary>
      /// <param name="name">Удаляемое имя</param>
      public bool Remove(string name)
      {
        if (XNamedRanges.hasByName(name))
        {
          XNamedRanges.removeByName(name);
          return true;
        }
        else
          return false;
      }

      #endregion
    }

    /// <summary>
    /// Доступ к именам диапазонов ячеек, определенных на уровне листа.
    /// LibreOffice Calc поддерживает это свойство, а OpenOffice - нет.
    /// Перед использованием следует проверить свойство Names.Exist
    /// </summary>
    public NameCollection Names { get { return new NameCollection(this); } }

    #endregion

    #region Используемая область

    /// <summary>
    /// Возвращает область, содержащую данные
    /// </summary>
    public Range UsedRange
    {
      get
      {
        unoidl.com.sun.star.sheet.XSheetCellCursor xCursor = XSpreadsheet.createCursor();
        unoidl.com.sun.star.sheet.XUsedAreaCursor xUsedCursor = (unoidl.com.sun.star.sheet.XUsedAreaCursor)xCursor;

        xUsedCursor.gotoStartOfUsedArea(true);
        unoidl.com.sun.star.table.XCell xCell1 = xCursor.getCellByPosition(0, 0);
        Cell Cell1 = new Cell(xCell1, _XSpreadsheetDocument);

        xUsedCursor.gotoEndOfUsedArea(true);
        Range R2 = new Range(xCursor as unoidl.com.sun.star.sheet.XSheetCellRange, _XSpreadsheetDocument);
        return new Range(Cell1, R2.LastCell);
      }
    }

    #endregion

    #region Защита листа

    /// <summary>
    /// Установить защиту листа
    /// </summary>
    /// <param name="password">Пароль (необязательный)</param>
    public void Protect(string password)
    {
      XProtectable.protect(password);
    }

    /// <summary>
    /// Снять защиту листа
    /// </summary>
    /// <param name="password">Пароль</param>
    public void Unprotect(string password)
    {
      XProtectable.unprotect(password);
    }

    /// <summary>
    /// Возвращает true, если лист защищен
    /// </summary>
    public bool IsProtected { get { return XProtectable.isProtected(); } }

    #endregion

    #region Выделение листа

    public void Select()
    {
      unoidl.com.sun.star.sheet.XSpreadsheetView ssv = Workbook.XModel.getCurrentController() as unoidl.com.sun.star.sheet.XSpreadsheetView;
      if (ssv == null)
        return;
      ssv.setActiveSheet(XSpreadsheet);

#if DEBUG
      unoidl.com.sun.star.container.XNamed xNamed2 = ssv.getActiveSheet() as unoidl.com.sun.star.container.XNamed;
      string newName = xNamed2.getName();
#endif
    }

    #endregion
  }

  /// <summary>
  /// Диапазон ячеек.
  /// Хранит ссылку на интерфейс XSheetCellRange.
  /// Дополнительно содержит ссылку на XSpreadsheetDocument, т.к. ее нельзя получить OpenOffice API.
  /// Диапазон может относиться только к одному листу книги
  /// </summary>
  public struct Range
  {
    #region Конструкторы

    public Range(unoidl.com.sun.star.sheet.XSheetCellRange xSheetCellRange, unoidl.com.sun.star.sheet.XSpreadsheetDocument xSpreadsheetDocument)
    {
#if DEBUG
      if (xSheetCellRange == null)
        throw new ArgumentNullException("xSheetCellRange");
      if (xSpreadsheetDocument == null)
        throw new ArgumentNullException("xSpreadsheetDocument");
#endif
      _XSheetCellRange = xSheetCellRange;
      _XSpreadsheetDocument = xSpreadsheetDocument;
    }

    /// <summary>
    /// Создает прямоугольный диапазон ячеек
    /// </summary>
    /// <param name="firstCell">Левая верхняя ячейка диапазона</param>
    /// <param name="lastCell">Правая нижняя ячейка диапазона</param>
    public Range(Cell firstCell, Cell lastCell)
      : this(GetXSheetCellRange(firstCell, lastCell), firstCell.XSpreadsheetDocument)
    {
    }

    private static unoidl.com.sun.star.sheet.XSheetCellRange GetXSheetCellRange(Cell firstCell, Cell lastCell)
    {
      firstCell.CheckIfExists();
      lastCell.CheckIfExists();

      unoidl.com.sun.star.table.XCellRange cr = firstCell.Sheet.XSpreadsheet.getCellRangeByPosition(firstCell.ColumnIndex, firstCell.RowIndex, lastCell.ColumnIndex, lastCell.RowIndex);
      return cr as unoidl.com.sun.star.sheet.XSheetCellRange;
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.sheet.XSheetCellRange XSheetCellRange { get { return _XSheetCellRange; } }
    private unoidl.com.sun.star.sheet.XSheetCellRange _XSheetCellRange;

    public unoidl.com.sun.star.sheet.XSpreadsheetDocument XSpreadsheetDocument { get { return _XSpreadsheetDocument; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetDocument _XSpreadsheetDocument;

    public unoidl.com.sun.star.table.XCellRange XCellRange { get { return _XSheetCellRange as unoidl.com.sun.star.table.XCellRange; } }

    public unoidl.com.sun.star.beans.XPropertySet XPropertySet { get { return _XSheetCellRange as unoidl.com.sun.star.beans.XPropertySet; } }

    public unoidl.com.sun.star.table.XColumnRowRange XColumnRowRange { get { return _XSheetCellRange as unoidl.com.sun.star.table.XColumnRowRange; } }

    public unoidl.com.sun.star.util.XMergeable XMergeable { get { return _XSheetCellRange as unoidl.com.sun.star.util.XMergeable; } }

    public unoidl.com.sun.star.sheet.XCellRangeData XCellRangeData { get { return _XSheetCellRange as unoidl.com.sun.star.sheet.XCellRangeData; } }

    public unoidl.com.sun.star.sheet.XCellRangeFormula XCellRangeFormula { get { return _XSheetCellRange as unoidl.com.sun.star.sheet.XCellRangeFormula; } }

    public unoidl.com.sun.star.sheet.XCellRangeAddressable XCellRangeAddressable { get { return _XSheetCellRange as unoidl.com.sun.star.sheet.XCellRangeAddressable; } }

    public unoidl.com.sun.star.sheet.XCellFormatRangesSupplier XCellFormatRangesSupplier { get { return _XSheetCellRange as unoidl.com.sun.star.sheet.XCellFormatRangesSupplier; } }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Лист, к которому относится диапазон ячеек
    /// </summary>
    public Worksheet Sheet
    {
      get
      {
        unoidl.com.sun.star.sheet.XSpreadsheet xSpreadsheet = _XSheetCellRange.getSpreadsheet();
        return new Worksheet(xSpreadsheet, _XSpreadsheetDocument);
      }
    }

    /// <summary>
    /// ODS-документ-владелец
    /// </summary>
    public Workbook Workbook
    {
      get
      {
        return new Workbook(_XSpreadsheetDocument);
      }
    }

    /// <summary>
    /// Свойства диапазоная ячеек
    /// </summary>
    public CellProperties Properties { get { return new CellProperties(XPropertySet, XSpreadsheetDocument); } }

    public override string ToString()
    {
      if (Exists)
      {
        if (RowCount == 1 && ColumnCount == 1)
          return FirstCell.ToString();
        else
          return Sheet.ToString() + "." + // Разделитель - точка
            "$" + MicrosoftOfficeTools.GetExcelColumnName(FirstColumnIndex + 1) +
            "$" + (FirstRowIndex + 1).ToString() + ":" +
            "$" + MicrosoftOfficeTools.GetExcelColumnName(LastColumnIndex + 1) +
            "$" + (LastRowIndex + 1).ToString();
      }
      else
        return "[ Empty ]";
    }

    /// <summary>
    /// Адрес диапазона ячеек
    /// </summary>
    public unoidl.com.sun.star.table.CellRangeAddress RangeAddress
    {
      get
      {
        return XCellRangeAddressable.getRangeAddress();
      }
    }

    /// <summary>
    /// Координаты верхнего левого угла диапазона ячеек относительно области листа в единицах 0.01мм
    /// </summary>
    public unoidl.com.sun.star.awt.Point Position
    {
      get
      {
        uno.Any x = XPropertySet.getPropertyValue("Position");
        return (unoidl.com.sun.star.awt.Point)(x.Value);
      }
    }

    /// <summary>
    /// Размеры области в единицах 0.01мм
    /// </summary>
    public unoidl.com.sun.star.awt.Size Size
    {
      get
      {
        uno.Any x = XPropertySet.getPropertyValue("Size");
        return (unoidl.com.sun.star.awt.Size)(x.Value);
      }
    }

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XSheetCellRange != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion

    #region Доступ к ячейкам

    /// <summary>
    /// Доступ к одиночной ячейке по индексу строки и столбца.
    /// Нумерация начинается с 0, а не с 1, как в Excel
    /// </summary>
    /// <param name="rowIndex">Смещение строки в пределах диапазона. Нумерация начинается с 0</param>
    /// <param name="columnIndex">Индекс столбца в пределах диапазона. Нумерация начинается с 0</param>
    /// <returns></returns>
    public Cell this[int rowIndex, int columnIndex]
    {
      get
      {
        unoidl.com.sun.star.table.XCell c = _XSheetCellRange.getCellByPosition(columnIndex, rowIndex);
        return new Cell(c, _XSpreadsheetDocument);
      }
    }

    /// <summary>
    /// Верхняя левая ячейка диапазона
    /// </summary>
    public Cell FirstCell { get { return this[0, 0]; } }

    /// <summary>
    /// Правая нижняя ячейка диапазона
    /// </summary>
    public Cell LastCell
    {
      get { return Sheet[LastRowIndex, LastColumnIndex]; }
    }

    /// <summary>
    /// Возвращает количество ячеек в диапазона
    /// </summary>
    public int CellCount
    {
      get { return RowCount * ColumnCount; }
    }

    #endregion

    #region Строки и столбцы

    /// <summary>
    /// Коллекция строк диапазона ячеек
    /// </summary>
    public TableRows Rows { get { return new TableRows(XColumnRowRange.getRows(), _XSpreadsheetDocument); } }

    /// <summary>
    /// Коллекция столбцов диапазона ячеек
    /// </summary>
    public TableColumns Columns { get { return new TableColumns(XColumnRowRange.getColumns(), _XSpreadsheetDocument); } }

    /// <summary>
    /// Возвращает число строк в диапазоне
    /// </summary>
    public int RowCount { get { return Rows.Count; } }

    /// <summary>
    /// Возвращает число столбцов в диапазоне.
    /// </summary>
    public int ColumnCount { get { return Columns.Count; } }

    /// <summary>
    /// Индекс первой строки диапазона на листе.
    /// Нумерация начинается с 0.
    /// </summary>
    public int FirstRowIndex { get { return FirstCell.CellAddress.Row; } }

    /// <summary>
    /// Индекс первого столбца диапазона на листе.
    /// Нумерация начинается с 0.
    /// </summary>
    public int FirstColumnIndex { get { return FirstCell.CellAddress.Column; } }

    /// <summary>
    /// Индекс последней строки диапазона на листе.
    /// Нумерация начинается с 0.
    /// </summary>
    public int LastRowIndex { get { return FirstRowIndex + RowCount - 1; } }

    /// <summary>
    /// Индекс последнего столбца диапазона на листе.
    /// Нумерация начинается с 0.
    /// </summary>
    public int LastColumnIndex { get { return FirstColumnIndex + ColumnCount - 1; } }

    #endregion

    #region Данные

    /// <summary>
    /// Массив данных в виде jagged-массива типа Any
    /// </summary>
    public uno.Any[][] DataArray
    {
      get { return XCellRangeData.getDataArray(); }
    }
    public void SetDataArray(uno.Any[][] value)
    {
#if DEBUG
      if (value.Length != RowCount)
        throw new ArgumentException("Неправильное число строк", "value");
      int cc = ColumnCount;
      for (int i = 0; i < value.Length; i++)
      {
        if (value[i].Length != cc)
          throw new ArgumentException("Неправильное число столбцов", "value");
      }
#endif

      XCellRangeData.setDataArray(value);
    }

    public string[][] FormulaArray
    {
      get { return XCellRangeFormula.getFormulaArray(); }
    }
    public void SetFormulaArray(string[][] value)
    {
      XCellRangeFormula.setFormulaArray(value);
    }

    #endregion

    #region Объединенные ячейки

    public void Merge()
    {
      XMergeable.merge(true);
    }

    public void Unmerge()
    {
      XMergeable.merge(false);
    }

    public bool IsMerged
    {
      get { return XMergeable.getIsMerged(); }
    }

    #endregion

    #region Имена ячеек

    /// <summary>
    /// Добавляет в список для книги имя, ссылающееся на заданный диапазон.
    /// Имя должно быть уникальным.
    /// Если нет уверенности, что такого имени нет, используйте перегрузку с аргментом RemoveOldName
    /// </summary>
    /// <param name="name">Имя, которое будет присвоено диапазону</param>
    public void AddName(string name)
    {
      AddName(name, false);
    }

    /// <summary>
    /// Добавляет в список для книги имя, ссылающееся на заданный диапазон.
    /// </summary>
    /// <param name="name">Имя, которое будет присвоено диапазону</param>
    /// <param name="removeOldName">Если true, то перед присвоением имени будет проверено наличие уже существующего имени, которое будет удалено</param>
    public void AddName(string name, bool removeOldName)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
      if (removeOldName)
        Workbook.Names.Remove(name);
      Workbook.Names.XNamedRanges.addNewByName(name, ToString(), new unoidl.com.sun.star.table.CellAddress(), 0);
    }

    #endregion

    #region Добавление / удаление ячеек

    /// <summary>
    /// Добавляет строки перед этим диапазоном.
    /// Число добавляемых строк соответствует числу строк в этом диапазоне
    /// </summary>
    public void InsertRows()
    {
      Sheet.Rows.XTableRows.insertByIndex(FirstRowIndex, RowCount);
    }

    /// <summary>
    /// Удаляет все строки, входящие в диапазон
    /// </summary>
    public void RemoveRows()
    {
      Sheet.Rows.XTableRows.removeByIndex(FirstRowIndex, RowCount);
    }

    /// <summary>
    /// Добавляет столбцы слева от этого диапазона.
    /// Число добавляемых столбцов соответствует числу столбцов в этом диапазоне
    /// </summary>
    public void InsertColumns()
    {
      Sheet.Columns.XTableColumns.insertByIndex(FirstColumnIndex, ColumnCount);
    }

    /// <summary>
    /// Удаляет все столбцы, входящие в дипазон
    /// </summary>
    public void RemoveColumns()
    {
      Sheet.Columns.XTableColumns.removeByIndex(FirstColumnIndex, ColumnCount);
    }

    #endregion

    #region Копирование данных

    /// <summary>
    /// Копирует текущий диапазон ячеек в другой, начиная с указанной ячейки
    /// </summary>
    /// <param name="firstDestCell"></param>
    public void CopyTo(Cell firstDestCell)
    {
      Sheet.XCellRangeMovement.copyRange(firstDestCell.CellAddress, RangeAddress);
    }

    public void CopyTo(Range dest)
    {
      if (dest.RowCount != RowCount || dest.ColumnCount != ColumnCount)
        throw new InvalidOperationException("Конечный диапазон имеет другой размер");
      CopyTo(dest.FirstCell);
    }

    #endregion

    #region Групповое изменение формата ячеек

    public CellFormatRanges CellFormatRanges
    {
      get
      {
        return new CellFormatRanges(XCellFormatRangesSupplier.getCellFormatRanges(), _XSpreadsheetDocument);
      }
    }

    internal int ReplaceNumberFormat(int oldKey, int newKey)
    {
      int res = 0;
      foreach (Range r in CellFormatRanges)
      {
        if (r.Properties.NumberFormatIndex == oldKey)
        {
          r.Properties.SetNumberFormatIndex(newKey);
          res += r.CellCount;
        }
      }
      return res;
    }

    #endregion
  }

  /// <summary>
  /// Одиночная ячейка.
  /// Хранит ссылку на интерфейс XCell.
  /// Дополнительно содержит ссылку на XSpreadsheetDocument, т.к. ее нельзя получить OpenOffice API
  /// </summary>
  public struct Cell
  {
    #region Конструктор

    public Cell(unoidl.com.sun.star.table.XCell xCell, unoidl.com.sun.star.sheet.XSpreadsheetDocument xSpreadsheetDocument)
    {
#if DEBUG
      if (xCell == null)
        throw new ArgumentNullException("xCell");
      if (xSpreadsheetDocument == null)
        throw new ArgumentNullException("xSpreadsheetDocument");
#endif
      _XCell = xCell;
      _XSpreadsheetDocument = xSpreadsheetDocument;
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.table.XCell XCell { get { return _XCell; } }
    private unoidl.com.sun.star.table.XCell _XCell;

    public unoidl.com.sun.star.sheet.XSpreadsheetDocument XSpreadsheetDocument { get { return _XSpreadsheetDocument; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetDocument _XSpreadsheetDocument;

    public unoidl.com.sun.star.beans.XPropertySet XPropertySet { get { return _XCell as unoidl.com.sun.star.beans.XPropertySet; } }

    public unoidl.com.sun.star.text.XText XText { get { return _XCell as unoidl.com.sun.star.text.XText; } }

    public unoidl.com.sun.star.sheet.XSheetCellRange XSheetCellRange
    { get { return _XCell as unoidl.com.sun.star.sheet.XSheetCellRange; } }

    public unoidl.com.sun.star.table.XCellRange XCellRange
    { get { return _XCell as unoidl.com.sun.star.table.XCellRange; } }

    public unoidl.com.sun.star.util.XMergeable XMergeable { get { return _XCell as unoidl.com.sun.star.util.XMergeable; } }

    public unoidl.com.sun.star.sheet.XSheetAnnotationAnchor XSheetAnnotationAnchor { get { return _XCell as unoidl.com.sun.star.sheet.XSheetAnnotationAnchor; } }

    /*
    /// <summary>
    /// Этот интерфейс имеет статус unpublished
    /// </summary>
    unoidl.com.sun.star.table.XMergeableCell XMergeableCell { get { return FXCell as unoidl.com.sun.star.table.XMergeableCell; } }
     */

    #endregion

    #region Общие свойства

    /// <summary>
    /// Лист, к которому относится ячейка
    /// </summary>
    public Worksheet Sheet
    {
      get
      {
        unoidl.com.sun.star.sheet.XSheetCellRange scr = _XCell as unoidl.com.sun.star.sheet.XSheetCellRange;
        unoidl.com.sun.star.sheet.XSpreadsheet xSpreadsheet = scr.getSpreadsheet();
        return new Worksheet(xSpreadsheet, _XSpreadsheetDocument);
      }
    }

    /// <summary>
    /// ODS-документ-владелец
    /// </summary>
    public Workbook Workbook
    {
      get
      {
        return new Workbook(_XSpreadsheetDocument);
      }
    }

    /// <summary>
    /// Возвращает диапазон, содержащий одну ячейку.
    /// Объединение ячеек не учитывается.
    /// Используйте свойство MergedRange, если ячейка может входить в состав объединения
    /// </summary>
    public Range Range
    {
      get
      {
        unoidl.com.sun.star.sheet.XSheetCellRange scr = _XCell as unoidl.com.sun.star.sheet.XSheetCellRange;
        return new Range(scr, _XSpreadsheetDocument);
      }
    }

    /// <summary>
    /// Свойства ячейки
    /// </summary>
    public CellProperties Properties { get { return new CellProperties(XPropertySet, XSpreadsheetDocument); } }

    /// <summary>
    /// 
    /// </summary>
    public CellValidation Validation
    {
      get
      {
        unoidl.com.sun.star.beans.XPropertySet psValidation =
          XPropertySet.getPropertyValue("Validation").Value as unoidl.com.sun.star.beans.XPropertySet;

        return new CellValidation(psValidation, _XSpreadsheetDocument);
      }
    }

    /// <summary>
    /// Возвращает имя ячейки с указанием листа, например, "Лист1!A1"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (Exists)
        return Sheet.ToString() + "." + // Разделитель - точка, а не восклицательный знак
            "$" + MicrosoftOfficeTools.GetExcelColumnName(ColumnIndex + 1) +
            "$" + (RowIndex + 1).ToString();
      else
        return "[ Empty ]";
    }

    /// <summary>
    /// Координаты верхнего левого угла ячейки относительно области листа в единицах 0.01мм
    /// </summary>
    public unoidl.com.sun.star.awt.Point Position
    {
      get
      {
        uno.Any x = XPropertySet.getPropertyValue("Position");
        return (unoidl.com.sun.star.awt.Point)(x.Value);
      }
    }

    /// <summary>
    /// Размеры ячейки в единицах 0.01мм
    /// </summary>
    public unoidl.com.sun.star.awt.Size Size
    {
      get
      {
        uno.Any x = XPropertySet.getPropertyValue("Size");
        return (unoidl.com.sun.star.awt.Size)(x.Value);
      }
    }

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XCell != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion

    #region Строки и столбцы

    /// <summary>
    /// Адрес ячейки
    /// </summary>
    public unoidl.com.sun.star.table.CellAddress CellAddress
    {
      get
      {
        unoidl.com.sun.star.sheet.XCellAddressable ca = (unoidl.com.sun.star.sheet.XCellAddressable)_XCell;
        return ca.getCellAddress();
      }
    }

    /// <summary>
    /// Индекс строки, в которой расположена ячейка.
    /// Нумерация начинается с 0
    /// </summary>
    public int RowIndex { get { return CellAddress.Row; } }

    /// <summary>
    /// Индекс столбца, в котором расположена ячейка.
    /// Нумерация начинается с 0
    /// </summary>
    public int ColumnIndex { get { return CellAddress.Column; } }

    /// <summary>
    /// Возвращает объект строки, в которой находится ячейка
    /// </summary>
    public TableRow Row { get { return Sheet.Rows[RowIndex]; } }

    /// <summary>
    /// Возвращает объект столбца, в котором находится ячейка
    /// </summary>
    public TableColumn Column { get { return Sheet.Columns[ColumnIndex]; } }

    public Cell GetOffset(int DeltaRows, int DeltaColumns)
    {
      return Sheet[RowIndex + DeltaRows, ColumnIndex + DeltaColumns];
    }

    #endregion

    #region Формула и значение

    /// <summary>
    /// Возвращает true, если ячейка содержит формулу
    /// </summary>
    public bool HasFormula
    {
      get { return XCell.getType() == unoidl.com.sun.star.table.CellContentType.FORMULA; }
    }

    /// <summary>
    /// Возвращает формулу, начинающуюся со знака равенства.
    /// Если ячейка не содержит формулы, возвращается пустая строка
    /// </summary>
    public string Formula { get { return XCell.getFormula(); } }

    public void SetFormula(string value)
    {
      XCell.setFormula(value);
    }

    /// <summary>
    /// Возвращает значение.
    /// Может вернуть строку, число типа Double или null.
    /// Значение возвращается даже при наличии формулы
    /// </summary>
    public object Value
    {
      get
      {
        unoidl.com.sun.star.table.CellContentType typ = XCell.getType();
        switch (typ) // 18.10.2016
        {
          case unoidl.com.sun.star.table.CellContentType.EMPTY:
            return null;
          case unoidl.com.sun.star.table.CellContentType.VALUE:
            return XCell.getValue();
          case unoidl.com.sun.star.table.CellContentType.TEXT:
            return XText.getString();
          default: // case unoidl.com.sun.star.table.CellContentType.FORMULA:
            // Формула
            // Этот вариант был до 18.10.2016
            // Тут вообще непонятно, как отличить текстовое значение от числового
            double v = XCell.getValue();
            if (v != 0.0)
              return v;

            string s = XText.getString();
            if (s.Length == 0)
              return null;
            else
              return s;
        }
      }
    }

    /// <summary>
    /// Присваивает значение ячейке
    /// </summary>
    /// <param name="value">Значение</param>
    public void SetValue(object value)
    {
      if (value == null)
        SetFormula(String.Empty);
      else if (value is String)
        XText.setString((string)value);
      else if (value is DateTime)
        XCell.setValue(((DateTime)value).ToOADate());
      else if (value is Boolean)
        XCell.setValue((bool)value ? 1 : 0); // ???
      else
      {
        double v = Convert.ToDouble(value);
        XCell.setValue(v);
      }
    }


    /// <summary>
    /// Возвращает текстовое представление значения.
    /// Если ячейка не содержит значения, возвращается пустая строка.
    /// Если ячейка содержит формулу, все равно возвращается значение
    /// </summary>
    public string AsString
    {
      get { return XText.getString(); }
    }

    /// <summary>
    /// Возвращает ненулевое значение, если в ячейке есть формула
    /// </summary>
    public int Error
    {
      get
      {
        return XCell.getError();
      }
    }

    #endregion

    #region Объединенные ячейки

    /// <summary>
    /// Возвращает диапазон, с учетом расширения до объединенных ячеек.
    /// Если ячейка не входит в состав объединения, возвращается диапазон из одной этой ячейки
    /// </summary>
    public Range MergedRange
    {
      get
      {
        unoidl.com.sun.star.sheet.XSheetCellCursor xCursor = XSheetCellRange.getSpreadsheet().createCursorByRange(XSheetCellRange);
        xCursor.collapseToMergedArea();
        return new Range(xCursor, _XSpreadsheetDocument);
      }
    }

    #endregion

    #region Имена ячейки

    /// <summary>
    /// Возвращает имена, относящиеся к данной ячейке.
    /// Метод очень медленный!
    /// </summary>
    /// <returns>Массив имен</returns>
    public string[] GetWorkbookNames()
    {
      List<string> lstNames = null;
      // Только через ....
      unoidl.com.sun.star.sheet.XNamedRanges nrs = Workbook.Names.XNamedRanges;// FXSpreadsheetDocument as unoidl.com.sun.star.sheet.XNamedRanges;
      string[] AllNames = nrs.getElementNames();
      for (int i = 0; i < AllNames.Length; i++)
      {
        // Надо проверять, т.к. AllNames содержит лишние имена, которые относятся не к книге, а к листам (?)
        if (nrs.hasByName(AllNames[i]))
        {
          unoidl.com.sun.star.sheet.XNamedRange nr = nrs.getByName(AllNames[i]).Value as unoidl.com.sun.star.sheet.XNamedRange;
          unoidl.com.sun.star.sheet.XCellRangeReferrer crr = nr as unoidl.com.sun.star.sheet.XCellRangeReferrer;
          unoidl.com.sun.star.table.XCellRange cr = crr.getReferredCells();
          if (cr == null)
            continue;
          unoidl.com.sun.star.table.XCell c = cr.getCellByPosition(0, 0);
          unoidl.com.sun.star.sheet.XCellAddressable ca = (unoidl.com.sun.star.sheet.XCellAddressable)c;
          unoidl.com.sun.star.table.CellAddress Addr2 = ca.getCellAddress();

          if (Compare(Addr2, this.CellAddress) == 0)
          {
            if (lstNames == null)
              lstNames = new List<string>();
            lstNames.Add(AllNames[i]);
          }
        }
      }
      if (lstNames == null)
        return DataTools.EmptyStrings;
      else
        return lstNames.ToArray();
    }

    /// <summary>
    /// Возвращает true, если заданное имя (в документе) ссылается на данную ячейку
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool ContainsWorkbookName(string name)
    {
      unoidl.com.sun.star.sheet.XNamedRanges nrs = Workbook.Names.XNamedRanges;// FXSpreadsheetDocument as unoidl.com.sun.star.sheet.XNamedRanges;
      if (!nrs.hasByName(name))
        return false;

      unoidl.com.sun.star.sheet.XNamedRange nr = nrs.getByName(name).Value as unoidl.com.sun.star.sheet.XNamedRange;
      unoidl.com.sun.star.sheet.XCellRangeReferrer crr = nr as unoidl.com.sun.star.sheet.XCellRangeReferrer;
      unoidl.com.sun.star.table.XCell c = crr.getReferredCells().getCellByPosition(0, 0);
      unoidl.com.sun.star.sheet.XCellAddressable ca = c as unoidl.com.sun.star.sheet.XCellAddressable;
      unoidl.com.sun.star.table.CellAddress addr2 = ca.getCellAddress();
      return Compare(CellAddress, addr2) == 0;
    }

    /// <summary>
    /// Добавляет в список для книги имя, ссылающееся на заданную ячейку.
    /// Имя должно быть уникальным.
    /// Если нет уверенности, что такого имени нет, используйте перегрузку с аргментом RemoveOldName
    /// </summary>
    /// <param name="name">Имя, которое будет присвоено ячейке</param>
    public void AddName(string name)
    {
      AddName(name, false);
    }

    /// <summary>
    /// Добавляет в список для книги имя, ссылающееся на заданную ячейку.
    /// </summary>
    /// <param name="name">Имя, которое будет присвоено ячейке</param>
    /// <param name="removeOldName">Если true, то перед присвоением имени будет проверено наличие уже существующего имени, которое будет удалено</param>
    public void AddName(string name, bool removeOldName)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
      if (removeOldName)
        Workbook.Names.Remove(name);

      Workbook.Names.XNamedRanges.addNewByName(name, ToString(), new unoidl.com.sun.star.table.CellAddress(), 0);
    }

    #endregion

    #region Методы сравнения

    public static int Compare(unoidl.com.sun.star.table.CellAddress a, unoidl.com.sun.star.table.CellAddress b)
    {
      if (a.Sheet != b.Sheet)
        return a.Sheet.CompareTo(b.Sheet);
      if (a.Row != b.Row)
        return a.Row.CompareTo(b.Row);
      else
        return a.Column.CompareTo(b.Column);
    }

    public static bool operator ==(Cell a, Cell b)
    {
      if (a.Exists && b.Exists)
      {
        if (a.XSpreadsheetDocument != b.XSpreadsheetDocument)
          return false;
        if (a._XCell == b._XCell)
          return true;

        return a.CellAddress == b.CellAddress;
      }

      if (a.Exists || b.Exists)
        return false;
      else
        return true;
    }

    public static bool operator !=(Cell a, Cell b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      if (obj is Cell)
        return this == (Cell)obj;
      else
        return false;
    }

    public override int GetHashCode()
    {
      return (CellAddress.Row << 16) | CellAddress.Column;
    }

    #endregion

    #region Выделение ячейки

    public void Select()
    {
      unoidl.com.sun.star.sheet.XSpreadsheetView ssv = Workbook.XModel.getCurrentController() as unoidl.com.sun.star.sheet.XSpreadsheetView;
      if (ssv == null)
        return;
      //      ssv.setActiveSheet(Sheet.XSpreadsheet); // Это в Sheet.Select()
      (ssv as unoidl.com.sun.star.view.XSelectionSupplier).select(new uno.Any(XCell.GetType(), (object)XCell));
    }

    #endregion

    #region Примечание для ячейки

    public string Annotation
    {
      get
      {
        unoidl.com.sun.star.sheet.XSheetAnnotation sa = XSheetAnnotationAnchor.getAnnotation();
        unoidl.com.sun.star.text.XSimpleText st = sa as unoidl.com.sun.star.text.XSimpleText;
        return st.getString();
      }
    }

    public void SetAnnotation(string text)
    {
      unoidl.com.sun.star.sheet.XSheetAnnotations sas = Sheet.XSheetAnnotationsSupplier.getAnnotations();
      sas.insertNew(CellAddress, text);
    }

    #endregion
  }

  /// <summary>
  /// Все возможные границы ячейки или диапазона
  /// </summary>
  public enum BorderKind
  {
    /// <summary>
    /// Все внешние границы ячейки или диапазона (без диагоналей)
    /// </summary>
    All,
    Top, Left, Right, Bottom,

    /// <summary>
    /// Диагональная линия "backslash"
    /// </summary>
    TLBR,

    /// <summary>
    /// Диагональная линия "/"
    /// </summary>
    BLTR,

    /// <summary>
    /// Диагональные линии
    /// </summary>
    Diagonals
  }

  /// <summary>
  /// Свойства одиночной ячейки или диапазона ячеек.
  /// </summary>
  public struct CellProperties
  {
    // Можно было бы сделать nullable-свойства.
    // Например, для CellBackColor возвращается (-1), если в диапазоне заданы разные цвета фона.
    // Для этого свойства было бы легко выделить значение null.
    // А для IsCellBackgroundTransparent возвращается true, вместо null. 
    // Чтобы отличить null-значение, требуется использовать интерфейс XPropertyState (?).
    // Это приведет к падению производительности, а в большинстве случаев реально нужны свойства ячейки, а не диапазона

    #region Конструктор

    public CellProperties(unoidl.com.sun.star.beans.XPropertySet xPropertySet, unoidl.com.sun.star.sheet.XSpreadsheetDocument xSpreadsheetDocument)
    {
#if DEBUG
      if (xPropertySet == null)
        throw new ArgumentNullException("xPropertySet");
      if (xSpreadsheetDocument == null)
        throw new ArgumentNullException("xSpreadsheetDocument");
#endif

      _XPropertySet = xPropertySet;
      _XSpreadsheetDocument = xSpreadsheetDocument;
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.beans.XPropertySet XPropertySet { get { return _XPropertySet; } }
    private unoidl.com.sun.star.beans.XPropertySet _XPropertySet;

    public unoidl.com.sun.star.sheet.XSpreadsheetDocument XSpreadsheetDocument { get { return _XSpreadsheetDocument; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetDocument _XSpreadsheetDocument;

    public unoidl.com.sun.star.util.XNumberFormatsSupplier XNumberFormatsSupplier
    { get { return _XSpreadsheetDocument as unoidl.com.sun.star.util.XNumberFormatsSupplier; } }

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XPropertySet != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion

    #region Числовой формат

    /// <summary>
    /// Индекс числового формата
    /// </summary>
    public int NumberFormatIndex
    {
      get
      {
        return DataTools.GetInt(_XPropertySet.getPropertyValue("NumberFormat").Value);
      }
    }
    public void SetNumberFormatIndex(int value)
    {
      _XPropertySet.setPropertyValue("NumberFormat", new uno.Any(value));
    }

    /// <summary>
    /// Числовой формат ячейки (условно совместимый с Excel)
    /// </summary>
    public string NumberFormat
    {
      get
      {
        unoidl.com.sun.star.beans.XPropertySet ps2 = XNumberFormatsSupplier.getNumberFormats().getByKey(NumberFormatIndex);
        return DataTools.GetString(ps2.getPropertyValue("FormatString").Value);
      }
    }

    #endregion

    #region Шрифт

    /// <summary>
    /// Цвет текста
    /// </summary>
    public Int32 CharColor
    {
      get { return DataTools.GetInt(XPropertySet.getPropertyValue("CharColor").Value); }
    }
    public void SetCharColor(Int32 value)
    {
      XPropertySet.setPropertyValue("CharColor", new uno.Any(value));
    }

    /// <summary>
    /// Высота символов
    /// </summary>
    public float CharHeight
    {
      get
      {
        return DataTools.GetSingle(XPropertySet.getPropertyValue("CharHeight").Value);
      }
    }

    public void SetCharHeight(float value)
    {
      XPropertySet.setPropertyValue("CharHeight", new uno.Any(value));
    }

    /// <summary>
    /// Жирный
    /// </summary>
    public bool Bold
    {
      get
      {
        float w = DataTools.GetSingle(XPropertySet.getPropertyValue("CharWeight").Value);
        return w > unoidl.com.sun.star.awt.FontWeight.NORMAL;
      }
    }
    public void SetBold(bool value)
    {
      float w = value ? unoidl.com.sun.star.awt.FontWeight.BOLD : unoidl.com.sun.star.awt.FontWeight.NORMAL;
      XPropertySet.setPropertyValue("CharWeight", new uno.Any(w));
    }

    /// <summary>
    /// Наклонный
    /// </summary>
    public bool Italic
    {
      get { return false; /*!!!!!return DataTools.GetBool(XPropertySet.getPropertyValue("IsTextWrapped").Value);*/ }
    }
    public void SetItalic(bool value)
    {
      //!!!!XPropertySet.setPropertyValue("IsTextWrapped", new uno.Any(value));
    }

    /// <summary>
    /// Подчеркнутый
    /// </summary>
    public bool Underline
    {
      get { return false; /*!!! return DataTools.GetBool(XPropertySet.getPropertyValue("IsTextWrapped").Value);*/ }
    }
    public void SetUnderline(bool value)
    {
      //!!!XPropertySet.setPropertyValue("IsTextWrapped", new uno.Any(value));
    }

    #endregion

    #region Выравнивание

    /// <summary>
    /// Горизонтальное выравнивание
    /// </summary>
    public unoidl.com.sun.star.table.CellHoriJustify HoriJustify
    {
      get { return (unoidl.com.sun.star.table.CellHoriJustify)(XPropertySet.getPropertyValue("HoriJustify").Value); }
    }
    public void SetHoriJustify(unoidl.com.sun.star.table.CellHoriJustify value)
    {
      XPropertySet.setPropertyValue("HoriJustify", new uno.Any(typeof(unoidl.com.sun.star.table.CellHoriJustify), value));
    }

    /// <summary>
    /// Вертикальное выравнивание
    /// </summary>
    public unoidl.com.sun.star.table.CellVertJustify VertJustify
    {
      get { return (unoidl.com.sun.star.table.CellVertJustify)(XPropertySet.getPropertyValue("VertJustify").Value); }
    }
    public void SetVertJustify(unoidl.com.sun.star.table.CellVertJustify value)
    {
      // 09.11.2016
      // В LibreOffice очередной глюк.
      // Установка вертикального выравнивания как значения перечисления не работает.
      // Но если устанавливать значение просто как целое число, то все работает.
      // При этом установка горизонтального выравнивания работает нормально.
      // Может быть, есть смысл вообще все установщики перечислимых свойств переделать на использование
      // типа Int32

      // XPropertySet.setPropertyValue("VertJustify", new uno.Any(typeof(unoidl.com.sun.star.table.CellVertJustify), value));

      XPropertySet.setPropertyValue("VertJustify", new uno.Any((int)value));
    }

    /// <summary>
    /// Перенос по словам
    /// </summary>
    public bool IsTextWrapped
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("IsTextWrapped").Value); }
    }
    public void SetTextWrapped(bool value)
    {
      XPropertySet.setPropertyValue("IsTextWrapped", new uno.Any(value));
    }

    /// <summary>
    /// Отступ в единицах 0.01 мм
    /// </summary>
    public int ParaIndent
    {
      get { return DataTools.GetInt(XPropertySet.getPropertyValue("ParaIndent").Value); }
    }
    public void SetParaIndent(int value)
    {
      XPropertySet.setPropertyValue("ParaIndent", new uno.Any((short)value));
    }

    /// <summary>
    /// Отступ в единицах 2.5мм
    /// </summary>
    public int IndentLevel
    {
      get { return ParaIndent / 250; }
    }

    public void SetIndentLevel(int value)
    {
      SetParaIndent(value * 250);
    }

    /// <summary>
    /// Вертикальное выравнивание
    /// </summary>
    public unoidl.com.sun.star.table.CellOrientation Orientation
    {
      get { return (unoidl.com.sun.star.table.CellOrientation)(XPropertySet.getPropertyValue("Orientation").Value); }
    }
    public void SetOrientation(unoidl.com.sun.star.table.CellOrientation value)
    {
      XPropertySet.setPropertyValue("Orientation", new uno.Any(typeof(unoidl.com.sun.star.table.CellOrientation), value));
    }

    // TODO: Вращение

    #endregion

    #region Фон

    /// <summary>
    /// Цвет фона
    /// </summary>
    public Int32 CellBackColor
    {
      get { return DataTools.GetInt(XPropertySet.getPropertyValue("CellBackColor").Value); }
    }
    public void SetCellBackColor(Int32 value)
    {
      XPropertySet.setPropertyValue("CellBackColor", new uno.Any(value));
    }

    /// <summary>
    /// true, если цвет фона является прозрачным
    /// </summary>
    public bool IsCellBackgroundTransparent
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("IsCellBackgroundTransparent").Value); }
    }
    public void SetCellBackgroundTransparent(bool value)
    {
      XPropertySet.setPropertyValue("IsCellBackgroundTransparent", new uno.Any(value));
    }

    #endregion

    #region Граница

    public struct BorderCollection
    {
      #region Конструктор

      internal BorderCollection(CellProperties owner)
      {
        _Owner = owner;
      }

      private CellProperties _Owner;

      #endregion

      #region Доступ к границам

      public unoidl.com.sun.star.table.BorderLine this[BorderKind border]
      {
        get
        {
          switch (border)
          {
            case BorderKind.Top:
              return (unoidl.com.sun.star.table.BorderLine)(_Owner._XPropertySet.getPropertyValue("TopBorder").Value);
            case BorderKind.Left:
              return (unoidl.com.sun.star.table.BorderLine)(_Owner._XPropertySet.getPropertyValue("LeftBorder").Value);
            case BorderKind.Right:
              return (unoidl.com.sun.star.table.BorderLine)(_Owner._XPropertySet.getPropertyValue("RightBorder").Value);
            case BorderKind.Bottom:
              return (unoidl.com.sun.star.table.BorderLine)(_Owner._XPropertySet.getPropertyValue("BottomBorder").Value);
            case BorderKind.TLBR:
              return (unoidl.com.sun.star.table.BorderLine)(_Owner._XPropertySet.getPropertyValue("DiagonalTLBR").Value);
            case BorderKind.BLTR:
              return (unoidl.com.sun.star.table.BorderLine)(_Owner._XPropertySet.getPropertyValue("DiagonalBLTR").Value);
            case BorderKind.All:
              return CellProperties.MergeBorders(this[BorderKind.Left], this[BorderKind.Right], this[BorderKind.Top], this[BorderKind.Bottom]);
            case BorderKind.Diagonals:
              return CellProperties.MergeBorders(this[BorderKind.TLBR], this[BorderKind.BLTR]);
            default:
              throw new ArgumentException("Неизвестный Border=" + border.ToString(), "border");
          }
        }
      }

      public void SetBorder(BorderKind border, unoidl.com.sun.star.table.BorderLine value)
      {
        uno.Any value2 = new uno.Any(typeof(unoidl.com.sun.star.table.BorderLine), value);

        switch (border)
        {
          case BorderKind.Top: _Owner.XPropertySet.setPropertyValue("TopBorder", value2); break;
          case BorderKind.Left: _Owner.XPropertySet.setPropertyValue("LeftBorder", value2); break;
          case BorderKind.Right: _Owner.XPropertySet.setPropertyValue("RightBorder", value2); break;
          case BorderKind.Bottom: _Owner.XPropertySet.setPropertyValue("BottomBorder", value2); break;
          case BorderKind.TLBR: _Owner.XPropertySet.setPropertyValue("DiagonalTLBR", value2); break;
          case BorderKind.BLTR: _Owner.XPropertySet.setPropertyValue("DiagonalBLTR", value2); break;
          case BorderKind.All:
            _Owner.XPropertySet.setPropertyValue("TopBorder", value2);
            _Owner.XPropertySet.setPropertyValue("LeftBorder", value2);
            _Owner.XPropertySet.setPropertyValue("RightBorder", value2);
            _Owner.XPropertySet.setPropertyValue("BottomBorder", value2);
            break;
          case BorderKind.Diagonals:
            _Owner.XPropertySet.setPropertyValue("DiagonalTLBR", value2);
            _Owner.XPropertySet.setPropertyValue("DiagonalBLTR", value2);
            break;
          default:
            throw new ArgumentException("Неизвестный Border=" + border.ToString(), "border");
        }
      }

      public void ClearBorder(BorderKind border)
      {
        SetBorder(border, new unoidl.com.sun.star.table.BorderLine());
        //uno.Any Value2 = uno.Any.VOID;

        //switch (Border)
        //{
        //  case BorderKind.Top: FOwner.XPropertySet.setPropertyValue("TopBorder", Value2); break;
        //  case BorderKind.Left: FOwner.XPropertySet.setPropertyValue("LeftBorder", Value2); break;
        //  case BorderKind.Right: FOwner.XPropertySet.setPropertyValue("RightBorder", Value2); break;
        //  case BorderKind.Bottom: FOwner.XPropertySet.setPropertyValue("BottomBorder", Value2); break;
        //  case BorderKind.TLBR: FOwner.XPropertySet.setPropertyValue("DiagonalTLBR", Value2); break;
        //  case BorderKind.BLTR: FOwner.XPropertySet.setPropertyValue("DiagonalBLTR", Value2); break;
        //  case BorderKind.All:
        //    FOwner.XPropertySet.setPropertyValue("TopBorder", Value2);
        //    FOwner.XPropertySet.setPropertyValue("LeftBorder", Value2);
        //    FOwner.XPropertySet.setPropertyValue("RightBorder", Value2);
        //    FOwner.XPropertySet.setPropertyValue("BottomBorder", Value2);
        //    break;
        //  case BorderKind.Diagonals:
        //    FOwner.XPropertySet.setPropertyValue("DiagonalTLBR", Value2);
        //    FOwner.XPropertySet.setPropertyValue("DiagonalBLTR", Value2);
        //    break;
        //  default:
        //    throw new ArgumentException("Неизвестный Border="+Border.ToString(), "Border");
        //}
      }


      #endregion
    }

    /// <summary>
    /// Доступ к границам ячейки или диапазона
    /// </summary>
    public BorderCollection Borders { get { return new BorderCollection(this); } }

    public static unoidl.com.sun.star.table.BorderLine MergeBorders(params unoidl.com.sun.star.table.BorderLine[] src)
    {
      if (src.Length == 0)
        return new unoidl.com.sun.star.table.BorderLine();
      unoidl.com.sun.star.table.BorderLine Res = src[0];
      for (int i = 1; i < src.Length; i++)
      {
        if (Res.Color != src[i].Color)
          Res.Color = 0;
        Res.InnerLineWidth = Math.Min(Res.InnerLineWidth, src[i].InnerLineWidth);
        Res.OuterLineWidth = Math.Min(Res.OuterLineWidth, src[i].OuterLineWidth);
        Res.LineDistance = Math.Min(Res.LineDistance, src[i].LineDistance);
      }

      return Res;
    }

    #endregion

    #region Защита

    /// <summary>
    /// Защита ячейки
    /// </summary>
    public unoidl.com.sun.star.util.CellProtection CellProtection
    {
      get { return (unoidl.com.sun.star.util.CellProtection)(XPropertySet.getPropertyValue("CellProtection").Value); }
    }
    public void SetCellProtection(unoidl.com.sun.star.util.CellProtection value)
    {
      XPropertySet.setPropertyValue("CellProtection", new uno.Any(typeof(unoidl.com.sun.star.util.CellProtection), value));
    }

    /// <summary>
    /// Упрощенный доступ к CellProtection
    /// </summary>
    public bool IsLocked { get { return CellProtection.IsLocked; } }

    /// <summary>
    /// Устанавливает свойство CellProtection.Locked
    /// </summary>
    /// <param name="value"></param>
    public void SetLocked(bool value)
    {
      unoidl.com.sun.star.util.CellProtection cp = this.CellProtection;
      cp.IsLocked = value;
      SetCellProtection(cp);
    }

    #endregion
  }

  /// <summary>
  /// Коллекция диапазонов ячеек
  /// </summary>
  public struct CellFormatRanges : IEnumerable<Range>
  {
    #region Конструктор

    public CellFormatRanges(unoidl.com.sun.star.container.XIndexAccess xIndexAccess, unoidl.com.sun.star.sheet.XSpreadsheetDocument xSpreadsheetDocument)
    {
#if DEBUG
      if (xIndexAccess == null)
        throw new ArgumentNullException("xIndexAccess");
      if (xSpreadsheetDocument == null)
        throw new ArgumentNullException("xSpreadsheetDocument");
#endif

      _XIndexAccess = xIndexAccess;
      _XSpreadsheetDocument = xSpreadsheetDocument;
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.container.XIndexAccess XIndexAccess { get { return _XIndexAccess; } }
    private unoidl.com.sun.star.container.XIndexAccess _XIndexAccess;

    public unoidl.com.sun.star.sheet.XSpreadsheetDocument XSpreadsheetDocument { get { return _XSpreadsheetDocument; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetDocument _XSpreadsheetDocument;

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XIndexAccess != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion

    #region Доступ к диапазонам

    public int Count { get { return _XIndexAccess.getCount(); } }

    public Range this[int index]
    {
      get
      {
        unoidl.com.sun.star.sheet.XSheetCellRange xSheetCellRange = _XIndexAccess.getByIndex(index).Value as unoidl.com.sun.star.sheet.XSheetCellRange;
        return new Range(xSheetCellRange, _XSpreadsheetDocument);
      }
    }

    #endregion

    #region IEnumerable<Range>

    /// <summary>
    /// Перечислитель для листов книги
    /// </summary>
    private class RangeEnumerator : IEnumerator<Range>
    {
      #region Конструктор

      public RangeEnumerator(CellFormatRanges ranges)
      {
        _Ranges = ranges;
        _Index = -1;
      }

      #endregion

      #region Свойства

      private CellFormatRanges _Ranges;

      private int _Index;

      #endregion

      #region IEnumerator<Range> Members

      public Range Current
      {
        get { return _Ranges[_Index]; }
      }

      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current
      {
        get { return _Ranges[_Index]; }
      }

      public bool MoveNext()
      {
        _Index++;
        return _Index < _Ranges.Count;
      }

      public void Reset()
      {
        _Index = -1;
      }

      #endregion
    }

    public IEnumerator<Range> GetEnumerator()
    {
      return new RangeEnumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new RangeEnumerator(this);
    }

    #endregion
  }

  /// <summary>
  /// Свойства одного числового формата
  /// </summary>
  public struct NumberFormatProperties
  {
    // Можно было бы сделать nullable-свойства.
    // Например, для CellBackColor возвращается (-1), если в диапазоне заданы разные цвета фона.
    // Для этого свойства было бы легко выделить значение null.
    // А для IsCellBackgroundTransparent возвращается true, вместо null. 
    // Чтобы отличить null-значение, требуется использовать интерфейс XPropertyState (?).
    // Это приведет к падению производительности, а в большинстве случаев реально нужны свойства ячейки, а не диапазона

    #region Конструктор

    public NumberFormatProperties(unoidl.com.sun.star.beans.XPropertySet xPropertySet, unoidl.com.sun.star.sheet.XSpreadsheetDocument xSpreadsheetDocument)
    {
#if DEBUG
      if (xPropertySet == null)
        throw new ArgumentNullException("xPropertySet");
      if (xSpreadsheetDocument == null)
        throw new ArgumentNullException("xSpreadsheetDocument");
#endif

      _XPropertySet = xPropertySet;
      _XSpreadsheetDocument = xSpreadsheetDocument;
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.beans.XPropertySet XPropertySet { get { return _XPropertySet; } }
    private unoidl.com.sun.star.beans.XPropertySet _XPropertySet;

    public unoidl.com.sun.star.sheet.XSpreadsheetDocument XSpreadsheetDocument { get { return _XSpreadsheetDocument; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetDocument _XSpreadsheetDocument;

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XPropertySet != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion

    #region Свойства

    public string FormatString { get { return DataTools.GetString(XPropertySet.getPropertyValue("FormatString").Value); } }

    public unoidl.com.sun.star.lang.Locale Locale { get { return XPropertySet.getPropertyValue("Locale").Value as unoidl.com.sun.star.lang.Locale; } }

    /// <summary>
    /// Возвращает константу из com.sun.star.util.NumberFormat
    /// </summary>
    public int Type { get { return DataTools.GetInt(XPropertySet.getPropertyValue("Type").Value); } }

    public string Comment { get { return DataTools.GetString(XPropertySet.getPropertyValue("Comment").Value); } }

    #endregion
  }


  /// <summary>
  /// Свойства для проверки значенией одиночной ячейки или диапазона ячеек
  /// </summary>
  public struct CellValidation
  {
    #region Конструктор

    public CellValidation(unoidl.com.sun.star.beans.XPropertySet xPropertySet, unoidl.com.sun.star.sheet.XSpreadsheetDocument xSpreadsheetDocument)
    {
#if DEBUG
      if (xPropertySet == null)
        throw new ArgumentNullException("xPropertySet");
      if (xSpreadsheetDocument == null)
        throw new ArgumentNullException("xSpreadsheetDocument");
#endif

      _XPropertySet = xPropertySet;
      _XSpreadsheetDocument = xSpreadsheetDocument;
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.beans.XPropertySet XPropertySet { get { return _XPropertySet; } }
    private unoidl.com.sun.star.beans.XPropertySet _XPropertySet;

    public unoidl.com.sun.star.sheet.XSpreadsheetDocument XSpreadsheetDocument { get { return _XSpreadsheetDocument; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetDocument _XSpreadsheetDocument;

    public unoidl.com.sun.star.sheet.XSheetCondition XSheetCondition { get { return _XPropertySet as unoidl.com.sun.star.sheet.XSheetCondition; } }

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XPropertySet != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion

    #region Свойства объекта проверки

    /// <summary>
    /// Тип проверки
    /// </summary>
    public unoidl.com.sun.star.sheet.ValidationType Type
    {
      get
      {
        object v = _XPropertySet.getPropertyValue("Type").Value;
        return (unoidl.com.sun.star.sheet.ValidationType)v;
      }
    }

    public unoidl.com.sun.star.sheet.ConditionOperator Operator { get { return XSheetCondition.getOperator(); } }

    public string Formula1 { get { return XSheetCondition.getFormula1(); } }

    public string Formula2 { get { return XSheetCondition.getFormula2(); } }

    #endregion

    #region Свойства из PropertySet

    public bool ShowErrorMessage
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("ShowErrorMessage").Value); }
    }

    // TODO: Остальное!

    #endregion
  }

  /// <summary>
  /// Коллекция строк
  /// </summary>
  public struct TableRows
  {
    #region Конструктор

    public TableRows(unoidl.com.sun.star.table.XTableRows xTableRows, unoidl.com.sun.star.sheet.XSpreadsheetDocument xSpreadsheetDocument)
    {
#if DEBUG
      if (xTableRows == null)
        throw new ArgumentNullException("xTableRows");
      if (xSpreadsheetDocument == null)
        throw new ArgumentNullException("xSpreadsheetDocument");
#endif

      _XTableRows = xTableRows;
      _XSpreadsheetDocument = xSpreadsheetDocument;
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.table.XTableRows XTableRows { get { return _XTableRows; } }
    private unoidl.com.sun.star.table.XTableRows _XTableRows;

    public unoidl.com.sun.star.sheet.XSpreadsheetDocument XSpreadsheetDocument { get { return _XSpreadsheetDocument; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetDocument _XSpreadsheetDocument;

    public unoidl.com.sun.star.container.XEnumerationAccess XEnumerationAccess { get { return _XTableRows as unoidl.com.sun.star.container.XEnumerationAccess; } }

    public unoidl.com.sun.star.beans.XPropertySet XPropertySet { get { return _XTableRows as unoidl.com.sun.star.beans.XPropertySet; } }

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XTableRows != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion

    #region Доступ к строкам

    /// <summary>
    /// Возвращает количество строк в диапазоне
    /// </summary>
    public int Count { get { return _XTableRows.getCount(); } }

    /// <summary>
    /// Возвращает строку с заданным индексом. Индекс отсчитывается от начала диапазона.
    /// Нумерация начинается с 0
    /// </summary>
    /// <param name="index">Индекс строки</param>
    /// <returns></returns>
    public TableRow this[int index]
    {
      get
      {
        unoidl.com.sun.star.table.XCellRange cr = _XTableRows.getByIndex(index).Value as unoidl.com.sun.star.table.XCellRange;
        return new TableRow(cr, _XSpreadsheetDocument);
      }
    }

    #endregion

    #region Свойства набора строк

    // ?? Может быть, нужно сделать nullable-свойства для чтения

    /// <summary>
    /// Высота строк в единицах 0.01мм
    /// </summary>
    public int Height
    {
      get { return DataTools.GetInt(XPropertySet.getPropertyValue("Height").Value); }
    }

    /// <summary>
    /// Устанавливает высоту каждой строки в наборе в единицах 0.01мм
    /// </summary>
    /// <param name="value">Высота строки</param>
    public void SetHeight(int value)
    {
      XPropertySet.setPropertyValue("Height", new uno.Any(value));
    }


    /// <summary>
    /// Возвращает высоту строк в пунктах (как принято в Excel)
    /// </summary>
    public double HeightPt
    {
      get
      {
        return Height / 2540.0 * 72.0;
      }
    }

    /// <summary>
    /// Установливает высоту каждой строки в пунктах (как принято в Excel)
    /// </summary>
    /// <param name="value">Высота в пунктах</param>
    public void SetHeightPt(double value)
    {
      SetHeight((int)(value / 72.0 * 2540.0));
    }

    /// <summary>
    /// Если true, то высота строки подбирается по содержимому
    /// </summary>
    public bool OptimalHeight
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("OptimalHeight").Value); }
    }

    public void SetOptimalHeight(bool value)
    {
      XPropertySet.setPropertyValue("OptimalHeight", new uno.Any(value));
    }

    /// <summary>
    /// Если true, то строка видимая
    /// </summary>
    public bool IsVisible
    {
      get
      {
        object v = XPropertySet.getPropertyValue("IsVisible").Value;
        return DataTools.GetBool(v);
      }
    }

    public void SetVisible(bool value)
    {
      XPropertySet.setPropertyValue("IsVisible", new uno.Any(value));
    }

    /// <summary>
    /// Если true, то печать строки начинается с новой страницы
    /// </summary>
    public bool IsStartOfNewPage
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("IsStartOfNewPage").Value); }
    }

    public void SerStartOfNewPage(bool value)
    {
      XPropertySet.setPropertyValue("IsStartOfNewPage", new uno.Any(value));
    }

    #endregion
  }

  /// <summary>
  /// Строка таблицы
  /// </summary>
  public struct TableRow
  {
    #region Конструктор

    public TableRow(unoidl.com.sun.star.table.XCellRange xCellRange, unoidl.com.sun.star.sheet.XSpreadsheetDocument xSpreadsheetDocument)
    {
#if DEBUG
      if (xCellRange == null)
        throw new ArgumentNullException("xCellRange");
      if (xSpreadsheetDocument == null)
        throw new ArgumentNullException("xSpreadsheetDocument");
#endif

      _XCellRange = xCellRange;
      _XSpreadsheetDocument = xSpreadsheetDocument;
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.table.XCellRange XCellRange { get { return _XCellRange; } }
    private unoidl.com.sun.star.table.XCellRange _XCellRange;

    public unoidl.com.sun.star.sheet.XSpreadsheetDocument XSpreadsheetDocument { get { return _XSpreadsheetDocument; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetDocument _XSpreadsheetDocument;

    // public unoidl.com.sun.star.container.XNamed XNamed { get { return FXCellRange as unoidl.com.sun.star.container.XNamed; } }

    public unoidl.com.sun.star.beans.XPropertySet XPropertySet { get { return _XCellRange as unoidl.com.sun.star.beans.XPropertySet; } }

    #endregion

    #region Свойства

    // public string Name { get { return XNamed.getName(); } }

    /// <summary>
    /// Высота строки в единицах 0.01мм
    /// </summary>
    public int Height
    {
      get { return DataTools.GetInt(XPropertySet.getPropertyValue("Height").Value); }
    }

    /// <summary>
    /// Установить высоту строки в единицах 0.01мм
    /// </summary>
    public void SetHeight(int value)
    {
      XPropertySet.setPropertyValue("Height", new uno.Any(value));
    }

    /// <summary>
    /// Возвращает высоту строки в пунктах (как принято в Excel)
    /// </summary>
    public double HeightPt
    {
      get
      {
        return Height / 2540.0 * 72.0;
      }
    }

    /// <summary>
    /// Установливает высоту строки в пунктах (как принято в Excel)
    /// </summary>
    /// <param name="value">Высота в пунктах</param>
    public void SetHeightPt(double value)
    {
      SetHeight((int)(value / 72.0 * 2540.0));
    }

    /// <summary>
    /// Если true, то высота строки подбирается по содержимому
    /// </summary>
    public bool OptimalHeight
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("OptimalHeight").Value); }
    }

    public void SetOptimalHeight(bool value)
    {
      XPropertySet.setPropertyValue("OptimalHeight", new uno.Any(value));
    }

    /// <summary>
    /// Если true, то строка видимая
    /// </summary>
    public bool IsVisible
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("IsVisible").Value); }
    }

    public void SetVisible(bool value)
    {
      XPropertySet.setPropertyValue("IsVisible", new uno.Any(value));
    }

    /// <summary>
    /// Если true, то печать строки начинается с новой страницы
    /// </summary>
    public bool IsStartOfNewPage
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("IsStartOfNewPage").Value); }
    }

    public void SetStartOfNewPage(bool value)
    {
      XPropertySet.setPropertyValue("IsStartOfNewPage", new uno.Any(value));
    }

    /// <summary>
    /// Диапазон ячеек для одной строки
    /// </summary>
    public Range Range
    {
      get
      {
        return new Range(_XCellRange as unoidl.com.sun.star.sheet.XSheetCellRange, _XSpreadsheetDocument);
      }
    }

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XCellRange != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion
  }

  /// <summary>
  /// Коллекция столбцов
  /// </summary>
  public struct TableColumns
  {
    #region Конструктор

    public TableColumns(unoidl.com.sun.star.table.XTableColumns xTableColumns, unoidl.com.sun.star.sheet.XSpreadsheetDocument xSpreadsheetDocument)
    {
#if DEBUG
      if (xTableColumns == null)
        throw new ArgumentNullException("xTableColumns");
      if (xSpreadsheetDocument == null)
        throw new ArgumentNullException("xSpreadsheetDocument");
#endif

      _XTableColumns = xTableColumns;
      _XSpreadsheetDocument = xSpreadsheetDocument;
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.table.XTableColumns XTableColumns { get { return _XTableColumns; } }
    private unoidl.com.sun.star.table.XTableColumns _XTableColumns;

    public unoidl.com.sun.star.sheet.XSpreadsheetDocument XSpreadsheetDocument { get { return _XSpreadsheetDocument; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetDocument _XSpreadsheetDocument;

    public unoidl.com.sun.star.container.XEnumerationAccess XEnumerationAccess { get { return _XTableColumns as unoidl.com.sun.star.container.XEnumerationAccess; } }

    public unoidl.com.sun.star.beans.XPropertySet XPropertySet { get { return _XTableColumns as unoidl.com.sun.star.beans.XPropertySet; } }

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XTableColumns != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion

    #region Доступ к столбцам

    /// <summary>
    /// Возвращает количество столбцов в диапазоне
    /// </summary>
    public int Count { get { return _XTableColumns.getCount(); } }

    /// <summary>
    /// Возвращает столбец с заданным индексом. Индекс отсчитывается от начала диапазона.
    /// Нумерация начинается с 0.
    /// </summary>
    /// <param name="index">Индекс столбца</param>
    /// <returns></returns>
    public TableColumn this[int index]
    {
      get
      {
        unoidl.com.sun.star.table.XCellRange cr = _XTableColumns.getByIndex(index).Value as unoidl.com.sun.star.table.XCellRange;
        return new TableColumn(cr, _XSpreadsheetDocument);
      }
    }

    #endregion

    #region Свойства для группы столбцов

    // ?? Может быть, нужно сделать nullable-свойства для чтения

    /// <summary>
    /// Ширина столбца в единицах 0.01мм
    /// </summary>
    public int Width
    {
      get { return DataTools.GetInt(XPropertySet.getPropertyValue("Width").Value); }
    }

    public void SetWidth(int value)
    {
      XPropertySet.setPropertyValue("Width", new uno.Any(value));
    }

    /// <summary>
    /// Если true, то ширина столбца подбирается по содержимому
    /// </summary>
    public bool OptimalWidth
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("OptimalWidth").Value); }
    }

    public void SetOptimalWidth(bool value)
    {
      XPropertySet.setPropertyValue("OptimalWidth", new uno.Any(value));
    }

    /// <summary>
    /// Если true, то столбец видимый
    /// </summary>
    public bool IsVisible
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("IsVisible").Value); }
    }

    public void SetVisible(bool value)
    {
      XPropertySet.setPropertyValue("IsVisible", new uno.Any(value));
    }

    /// <summary>
    /// Если true, то печать столбца начинается с новой страницы
    /// </summary>
    public bool IsStartOfNewPage
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("IsStartOfNewPage").Value); }
    }

    public void SetStartOfNewPage(bool value)
    {
      XPropertySet.setPropertyValue("IsStartOfNewPage", new uno.Any(value));
    }

    #endregion
  }

  /// <summary>
  /// Столбец таблицы
  /// </summary>
  public struct TableColumn
  {
    #region Конструктор

    public TableColumn(unoidl.com.sun.star.table.XCellRange xCellRange, unoidl.com.sun.star.sheet.XSpreadsheetDocument xSpreadsheetDocument)
    {
#if DEBUG
      if (xCellRange == null)
        throw new ArgumentNullException("xCellRange");
      if (xSpreadsheetDocument == null)
        throw new ArgumentNullException("xSpreadsheetDocument");
#endif

      _XCellRange = xCellRange;
      _XSpreadsheetDocument = xSpreadsheetDocument;
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.table.XCellRange XCellRange { get { return _XCellRange; } }
    private unoidl.com.sun.star.table.XCellRange _XCellRange;

    public unoidl.com.sun.star.sheet.XSpreadsheetDocument XSpreadsheetDocument { get { return _XSpreadsheetDocument; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetDocument _XSpreadsheetDocument;

    public unoidl.com.sun.star.container.XNamed XNamed { get { return _XCellRange as unoidl.com.sun.star.container.XNamed; } }

    public unoidl.com.sun.star.beans.XPropertySet XPropertySet { get { return _XCellRange as unoidl.com.sun.star.beans.XPropertySet; } }

    #endregion

    #region Свойства

    public string Name { get { return XNamed.getName(); } }

    /// <summary>
    /// Ширина столбца в единицах 0.01мм
    /// </summary>
    public int Width
    {
      get { return DataTools.GetInt(XPropertySet.getPropertyValue("Width").Value); }
    }

    public void SetWidth(int value)
    {
      XPropertySet.setPropertyValue("Width", new uno.Any(value));
    }

    /// <summary>
    /// Если true, то ширина столбца подбирается по содержимому
    /// </summary>
    public bool OptimalWidth
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("OptimalWidth").Value); }
    }

    public void SetOptimalWidth(bool value)
    {
      XPropertySet.setPropertyValue("OptimalWidth", new uno.Any(value));
    }

    /// <summary>
    /// Если true, то столбец видимый
    /// </summary>
    public bool IsVisible
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("IsVisible").Value); }
    }

    public void SetVisible(bool value)
    {
      XPropertySet.setPropertyValue("IsVisible", new uno.Any(value));
    }

    /// <summary>
    /// Если true, то печать столбца начинается с новой страницы
    /// </summary>
    public bool IsStartOfNewPage
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("IsStartOfNewPage").Value); }
    }

    public void SetStartOfNewPage(bool value)
    {
      XPropertySet.setPropertyValue("IsStartOfNewPage", new uno.Any(value));
    }

    /// <summary>
    /// Диапазон ячеек для одного столбца
    /// </summary>
    public Range Range
    {
      get
      {
        return new Range(_XCellRange as unoidl.com.sun.star.sheet.XSheetCellRange, _XSpreadsheetDocument);
      }
    }

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XCellRange != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion
  }

  /// <summary>
  /// Просмотр для листа (контроллер).
  /// Для доступа к контроллеру используйте свойство Workbook.CurrentControoler
  /// </summary>
  public struct SpreadsheetView
  {
    #region Конструктор

    public SpreadsheetView(unoidl.com.sun.star.sheet.XSpreadsheetView xSpreadsheetView, unoidl.com.sun.star.sheet.XSpreadsheetDocument xSpreadsheetDocument)
    {
#if DEBUG
      if (xSpreadsheetView == null)
        throw new ArgumentNullException("xSpreadsheetView");
      if (xSpreadsheetDocument == null)
        throw new ArgumentNullException("xSpreadsheetDocument");
#endif

      _XSpreadsheetView = xSpreadsheetView;
      _XSpreadsheetDocument = xSpreadsheetDocument;
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.sheet.XSpreadsheetView XSpreadsheetView { get { return _XSpreadsheetView; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetView _XSpreadsheetView;

    public unoidl.com.sun.star.sheet.XSpreadsheetDocument XSpreadsheetDocument { get { return _XSpreadsheetDocument; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetDocument _XSpreadsheetDocument;

    /// <summary>
    /// Для перебора XSpreadsheetViewPane
    /// </summary>
    public unoidl.com.sun.star.container.XIndexAccess XIndexAccess { get { return _XSpreadsheetView as unoidl.com.sun.star.container.XIndexAccess; } }

    /// <summary>
    /// Для перебора XSpreadsheetViewPane
    /// </summary>
    public unoidl.com.sun.star.container.XEnumerationAccess XEnumerationAccess { get { return _XSpreadsheetView as unoidl.com.sun.star.container.XEnumerationAccess; } }

    public unoidl.com.sun.star.view.XSelectionSupplier XSelectionSupplier { get { return _XSpreadsheetView as unoidl.com.sun.star.view.XSelectionSupplier; } }

    public unoidl.com.sun.star.sheet.XViewSplitable XViewSplitable { get { return _XSpreadsheetView as unoidl.com.sun.star.sheet.XViewSplitable; } }

    public unoidl.com.sun.star.sheet.XViewFreezable XViewFreezable { get { return _XSpreadsheetView as unoidl.com.sun.star.sheet.XViewFreezable; } }

    public unoidl.com.sun.star.sheet.XRangeSelection XRangeSelection { get { return _XSpreadsheetView as unoidl.com.sun.star.sheet.XRangeSelection; } }

    public unoidl.com.sun.star.sheet.XEnhancedMouseClickBroadcaster XEnhancedMouseClickBroadcaster { get { return _XSpreadsheetView as unoidl.com.sun.star.sheet.XEnhancedMouseClickBroadcaster; } }

    public unoidl.com.sun.star.sheet.XActivationBroadcaster XActivationBroadcaster { get { return _XSpreadsheetView as unoidl.com.sun.star.sheet.XActivationBroadcaster; } }

    /// <summary>
    /// Доступ к основной панели контроллера
    /// </summary>
    public unoidl.com.sun.star.sheet.XViewPane XViewPane { get { return _XSpreadsheetView as unoidl.com.sun.star.sheet.XViewPane; } }

    /// <summary>
    /// Доступ к основной панели контроллера
    /// </summary>
    public unoidl.com.sun.star.sheet.XCellRangeReferrer XCellRangeReferrer { get { return _XSpreadsheetView as unoidl.com.sun.star.sheet.XCellRangeReferrer; } }

    /// <summary>
    /// Свойства контроллера (служба SpreadsheetViewSettings)
    /// </summary>
    public unoidl.com.sun.star.beans.XPropertySet XPropertySet { get { return _XSpreadsheetView as unoidl.com.sun.star.beans.XPropertySet; } }

    public unoidl.com.sun.star.frame.XDispatchProvider XDispatchProvider { get { return _XSpreadsheetView as unoidl.com.sun.star.frame.XDispatchProvider; } }

    #endregion

    #region Панели с ячейками

    /// <summary>
    /// Основная панель контроллера
    /// </summary>
    public SpreadsheetViewPane DefaultPane
    {
      get { return new SpreadsheetViewPane(XViewPane, XSpreadsheetDocument); }
    }

    /// <summary>
    /// Свойство Panes
    /// </summary>
    public struct PaneCollection
    {
      #region Конструктор

      public PaneCollection(SpreadsheetView owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Свойства

      private SpreadsheetView _Owner;

      /// <summary>
      /// Количество панелей (1, 2 или 4)
      /// </summary>
      public int Count
      {
        get { return _Owner.XIndexAccess.getCount(); }
      }

      /// <summary>
      /// Получение панели по индексу
      /// </summary>
      /// <param name="index">Индекс панели (от 0 до 3)</param>
      /// <returns>Панель</returns>
      public SpreadsheetViewPane this[int index]
      {
        get
        {
          uno.Any Res = _Owner.XIndexAccess.getByIndex(index);
          unoidl.com.sun.star.sheet.XViewPane XViewPane = Res.Value as unoidl.com.sun.star.sheet.XViewPane;
          return new SpreadsheetViewPane(XViewPane, _Owner.XSpreadsheetDocument);
        }
      }

      #endregion
    }

    /// <summary>
    /// Доступ к панелям контроллера
    /// </summary>
    public PaneCollection Panes { get { return new PaneCollection(this); } }

    #endregion

    #region Свойства контроллера

    // https://www.openoffice.org/api/docs/common/ref/com/sun/star/sheet/SpreadsheetViewSettings.html#ZoomValue

    /// <summary>
    /// Выводить формулы вместо результатов вычисления
    /// </summary>
    public bool ShowFormulas
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("ShowFormulas").Value); }
    }

    public void SetShowFormulas(bool value)
    {
      XPropertySet.setPropertyValue("ShowFormulas", new uno.Any(value));
    }

    /// <summary>
    /// enables display of zero-values
    /// </summary>
    public bool ShowZeroValues
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("ShowZeroValues").Value); }
    }

    public void SetShowZeroValues(bool value)
    {
      XPropertySet.setPropertyValue("ShowZeroValues", new uno.Any(value));
    }

    /// <summary>
    /// controls whether strings, values, and formulas are displayed in different colors
    /// </summary>
    public bool IsValueHighlightingEnabled
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("IsValueHighlightingEnabled").Value); }
    }

    public void SetIsValueHighlightingEnabled(bool value)
    {
      XPropertySet.setPropertyValue("IsValueHighlightingEnabled", new uno.Any(value));
    }

    /// <summary>
    /// Показывать подсказки к ячейкам
    /// </summary>
    public bool ShowNotes
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("ShowNotes").Value); }
    }

    public void SetShowNotes(bool value)
    {
      XPropertySet.setPropertyValue("ShowNotes", new uno.Any(value));
    }



    /// <summary>
    /// Наличие вертикальной полосы прокрутки
    /// </summary>
    public bool HasVerticalScrollBar
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("HasVerticalScrollBar").Value); }
    }

    public void SetHasVerticalScrollBar(bool value)
    {
      XPropertySet.setPropertyValue("HasVerticalScrollBar", new uno.Any(value));
    }

    /// <summary>
    /// Наличие горизонтальной полосы прокрутки
    /// </summary>
    public bool HasHasHorizontalScrollBar
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("HasHorizontalScrollBar").Value); }
    }

    public void SetHasHorizontalScrollBar(bool value)
    {
      XPropertySet.setPropertyValue("HasHorizontalScrollBar", new uno.Any(value));
    }

    /// <summary>
    /// Наличие ярлычков листов
    /// </summary>
    public bool HasSheetTabs
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("HasSheetTabs").Value); }
    }

    public void SetHasSheetTabs(bool value)
    {
      XPropertySet.setPropertyValue("HasSheetTabs", new uno.Any(value));
    }


    /// <summary>
    /// enables the display of outline symbols
    /// </summary>
    public bool IsOutlineSymbolsSet
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("IsOutlineSymbolsSet").Value); }
    }

    public void SetIsOutlineSymbolsSet(bool value)
    {
      XPropertySet.setPropertyValue("IsOutlineSymbolsSet", new uno.Any(value));
    }

    /// <summary>
    /// Отображение заголовков строк и столбцов
    /// </summary>
    public bool HasColumnRowHeaders
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("HasColumnRowHeaders").Value); }
    }

    public void SetHasColumnRowHeaders(bool value)
    {
      XPropertySet.setPropertyValue("HasColumnRowHeaders", new uno.Any(value));
    }

    /// <summary>
    /// Отображение сетки ячеек
    /// </summary>
    public bool ShowGrid
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("ShowGrid").Value); }
    }

    public void SetShowGrid(bool value)
    {
      XPropertySet.setPropertyValue("ShowGrid", new uno.Any(value));
    }

    /// <summary>
    /// Цвет сетки
    /// </summary>
    public Int32 GridColor
    {
      get { return DataTools.GetInt(XPropertySet.getPropertyValue("GridColor").Value); }
    }

    public void SetGridColor(Int32 value)
    {
      XPropertySet.setPropertyValue("GridColor", new uno.Any(value));
    }

    /// <summary>
    /// enables display of help lines when moving drawing objects
    /// </summary>
    public bool ShowHelpLines
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("ShowHelpLines").Value); }
    }

    public void SetShowHelpLines(bool value)
    {
      XPropertySet.setPropertyValue("ShowHelpLines", new uno.Any(value));
    }


    /// <summary>
    /// enables display of anchor symbols when drawing objects are selected.
    /// </summary>
    public bool ShowAnchor
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("ShowAnchor").Value); }
    }

    public void SetShowAnchor(bool value)
    {
      XPropertySet.setPropertyValue("ShowAnchor", new uno.Any(value));
    }

    /// <summary>
    /// Показывать границы страниц
    /// </summary>
    public bool ShowPageBreaks
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("ShowPageBreaks").Value); }
    }

    public void SetShowPageBreaks(bool value)
    {
      XPropertySet.setPropertyValue("ShowPageBreaks", new uno.Any(value));
    }

    /// <summary>
    /// enables solid (colored) handles when drawing objects are selected. 
    /// </summary>
    public bool SolidHandles
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("SolidHandles").Value); }
    }

    public void SetSolidHandles(bool value)
    {
      XPropertySet.setPropertyValue("SolidHandles", new uno.Any(value));
    }

    /// <summary>
    /// enables display of embedded objects in the view. 
    /// </summary>
    public bool ShowObjects
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("ShowObjects").Value); }
    }

    public void SetShowObjects(bool value)
    {
      XPropertySet.setPropertyValue("ShowObjects", new uno.Any(value));
    }

    /// <summary>
    /// enables the display of charts in the view. 
    /// </summary>
    public bool ShowCharts
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("ShowCharts").Value); }
    }

    public void SetShowCharts(bool value)
    {
      XPropertySet.setPropertyValue("ShowCharts", new uno.Any(value));
    }

    /// <summary>
    /// enables the display of drawing objects in the view. 
    /// </summary>
    public bool ShowDrawing
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("ShowDrawing").Value); }
    }

    public void SetShowDrawing(bool value)
    {
      XPropertySet.setPropertyValue("ShowDrawing", new uno.Any(value));
    }

    /// <summary>
    /// disables the display of marks from online spelling
    /// </summary>
    public bool HideSpellMarks
    {
      get { return DataTools.GetBool(XPropertySet.getPropertyValue("HideSpellMarks").Value); }
    }

    public void SetHideSpellMarks(bool value)
    {
      XPropertySet.setPropertyValue("HideSpellMarks", new uno.Any(value));
    }


    /// <summary>
    /// Способ масштабирования.
    /// Константа unoidl.com.sun.star.view.DocumentZoomType 
    /// </summary>
    public int ZoomType
    {
      get { return DataTools.GetInt(XPropertySet.getPropertyValue("ZoomType").Value); }
    }

    /// <summary>
    /// Задать метод масштабирования
    /// </summary>
    /// <param name="value">Константа unoidl.com.sun.star.view.DocumentZoomType</param>
    public void SetZoomType(int value)
    {
      // Использование типа Int16 является обязательным!
      XPropertySet.setPropertyValue("ZoomType", new uno.Any(typeof(Int16), (Int16)value));
    }

    /// <summary>
    /// Масштаб отображения в процентах.
    /// </summary>
    public int ZoomValue
    {
      get { return DataTools.GetInt(XPropertySet.getPropertyValue("ZoomValue").Value); }
    }

    /// <summary>
    /// Установить масштаб в процентах.
    /// Предварительно должен быть вызван метод SetZoomType(unoidl.com.sun.star.view.DocumentZoomType.BY_VALUE)
    /// </summary>
    /// <param name="value"></param>
    public void SetZoomValue(int value)
    {
      // Использование типа Int16 является обязательным!
      XPropertySet.setPropertyValue("ZoomValue", new uno.Any(typeof(Int16), (Int16)value));
    }

    #endregion

    #region Другие свойства

    /// <summary>
    /// Текущий просматриваемый лист.
    /// Допускается установка свойства для активации листа
    /// </summary>
    public Worksheet ActiveSheet
    {
      get
      {
        unoidl.com.sun.star.sheet.XSpreadsheet xSpreadsheet = XSpreadsheetView.getActiveSheet();
        return new Worksheet(xSpreadsheet, XSpreadsheetDocument);
      }
      set
      {
        value.CheckIfExists();
        XSpreadsheetView.setActiveSheet(value.XSpreadsheet);
      }
    }

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XSpreadsheetView != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion
  }

  /// <summary>
  /// Часть контроллера для просмотра прямоугольной области ячеек.
  /// Для доступа к панели используется свойство SpreadsheetView.DefaultPane или можно получить доступ 
  /// к нужной части, если в просмотре выполнено разбиение просмотра по вертикали и/или горизонтали
  /// </summary>
  public struct SpreadsheetViewPane
  {
    #region Конструктор

    public SpreadsheetViewPane(unoidl.com.sun.star.sheet.XViewPane xViewPane, unoidl.com.sun.star.sheet.XSpreadsheetDocument xSpreadsheetDocument)
    {
#if DEBUG
      if (xViewPane == null)
        throw new ArgumentNullException("xViewPane");
      if (xSpreadsheetDocument == null)
        throw new ArgumentNullException("xSpreadsheetDocument");
#endif

      _XViewPane = xViewPane;
      _XSpreadsheetDocument = xSpreadsheetDocument;
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.sheet.XViewPane XViewPane { get { return _XViewPane; } }
    private unoidl.com.sun.star.sheet.XViewPane _XViewPane;

    public unoidl.com.sun.star.sheet.XSpreadsheetDocument XSpreadsheetDocument { get { return _XSpreadsheetDocument; } }
    private unoidl.com.sun.star.sheet.XSpreadsheetDocument _XSpreadsheetDocument;

    public unoidl.com.sun.star.sheet.XCellRangeReferrer XCellRangeReferrer { get { return _XViewPane as unoidl.com.sun.star.sheet.XCellRangeReferrer; } }

    public unoidl.com.sun.star.view.XControlAccess XControlAccess { get { return _XViewPane as unoidl.com.sun.star.view.XControlAccess; } }

    #endregion

    #region Свойства


    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XViewPane != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion
  }
}