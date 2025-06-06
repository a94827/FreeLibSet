﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using FreeLibSet.IO;
using FreeLibSet.Core;

#pragma warning disable 1591

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Источник данных XML-документа (свойство <see cref="XmlViewBox.SourceKind"/>)
  /// </summary>
  public enum XmlViewBoxSourceKind
  {
    /// <summary>
    /// Данные не были присоединены
    /// </summary>
    None,

    /// <summary>
    /// Был указан путь к файлу (свойство <see cref="XmlViewBox.XmlFilePath"/>)
    /// </summary>
    File,

    /// <summary>
    /// Был задан массив данных (свойство <see cref="XmlViewBox.XmlBytes"/>)
    /// </summary>
    Bytes,

    /// <summary>
    /// Был присоединен документ в памяти (свойство <see cref="XmlViewBox.XmlDocument"/>)
    /// </summary>
    XmlDocument
  }

  /// <summary>
  /// Объект для просмотра XML-документов
  /// </summary>
  [Description("XML-document viewer. Extends WebBrowser.")]
  [ToolboxBitmap(typeof(XmlViewBox), "XmlViewBox.bmp")]
  [ToolboxItem(true)]
  public class XmlViewBox : WebBrowser
  {
    #region Конструктор

    public XmlViewBox()
    {
      base.AllowNavigation = false;
      base.AllowWebBrowserDrop = false;
      base.WebBrowserShortcutsEnabled = false;
      base.IsWebBrowserContextMenuEnabled = false;

      _SourceKind = XmlViewBoxSourceKind.None;
    }

    protected override void Dispose(bool disposing)
    {
      if (_TempDir != null)
      {
        try
        {
          _TempDir.Dispose();
        }
        catch { }
        _TempDir = null;
      }

      base.Dispose(disposing);
    }

    #endregion

    #region Просматриваемые данные

    /*
     * Возможен просмотр XML-документов из четырех источников
     * 1. Реальный файл на диске (XmlFilePath)
     * 2. Массив байт в памяти (XmlBytes)
     * 3. Объект XmlDocument (XmlDocument)
     * 4. Строка в памяти (XmlText) (этот вариант является вторичным и преобразуется в XmlDocument)
     * 
     * WebBrowser умеет правильно работать с XML, только при его загрузке из файла,
     * а не из памяти.
     * Поэтому используем, по необходимости, временный каталог, который стираем
     * в методе Dispose
     */

    /// <summary>
    /// Откуда были загружены XML-данные
    /// </summary>
    [Browsable(false)]
    public XmlViewBoxSourceKind SourceKind { get { return _SourceKind; } }
    private XmlViewBoxSourceKind _SourceKind;

    #region Внешний файл

    /// <summary>
    /// Свойство используется для просмотра постоянно существующего файла на диске
    /// </summary>
    [Description("Name of disk file to view")]
    [Category("Appearance")]
    [DefaultValue("")]
    public string XmlFilePath
    {
      get
      {
        if (string.IsNullOrEmpty(_XmlFilePath))
        {
          if (_XmlBytes != null)
            return _TempDir.Dir.SlashedPath + "1.xml";
          else
            return String.Empty;
        }
        return _XmlFilePath;
      }
      set
      {
        if (String.IsNullOrEmpty(value))
        {
          ClearXml();
          return;
        }

        _XmlBytes = null;
        _XmlDocument = null;
        _XmlFilePath = value;
        base.Navigate("file://" + _XmlFilePath);
        base.Refresh();

        _SourceKind = XmlViewBoxSourceKind.File;
        OnXmlDataChanged();
      }
    }
    private string _XmlFilePath;

    #endregion

    #region Массив байт

    /// <summary>
    /// Представление XML-документа как массива байт
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public Byte[] XmlBytes
    {
      get
      {
        if (_XmlBytes == null)
        {
          if (!String.IsNullOrEmpty(_XmlFilePath))
            _XmlBytes = File.ReadAllBytes(_XmlFilePath);
          else if (_XmlDocument != null)
            _XmlBytes = DataTools.XmlDocumentToByteArray(_XmlDocument);
        }
        return _XmlBytes;
      }
      set
      {
        if (value == null)
        {
          ClearXml();
          return;
        }
        if (value.Length == 0)
        {
          ClearXml();
          return;
        }

        _XmlBytes = value;
        _XmlDocument = null;
        _XmlFilePath = null;
        // Не знаю, как по другому.
        // Только записать во временный файл, затем загрузить
        // Свойства DoumentText и DocumentStream подходят только для HTML-страниц
        if (_TempDir == null)
          _TempDir = new TempDirectory();
        File.WriteAllBytes(_TempDir.Dir.SlashedPath + "1.xml", _XmlBytes);
        base.Navigate("file://" + _TempDir.Dir.SlashedPath + "1.xml");
        base.Refresh();

        _SourceKind = XmlViewBoxSourceKind.Bytes;
        OnXmlDataChanged();
      }
    }

    private Byte[] _XmlBytes;

    private TempDirectory _TempDir;

    #endregion

    #region XmlDocument

    /// <summary>
    /// Текущий XML-документ
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public XmlDocument XmlDocument
    {
      get
      {
        if (_XmlDocument == null)
        {
          if (!String.IsNullOrEmpty(_XmlFilePath))
            _XmlDocument = FileTools.ReadXmlDocument(new AbsPath(_XmlFilePath));
          else if (_XmlBytes != null)
            _XmlDocument = DataTools.XmlDocumentFromByteArray(_XmlBytes);
        }
        return _XmlDocument;
      }
      set
      {
        if (value == null)
        {
          ClearXml();
          return;
        }

        if (value.DocumentElement == null) // 23.06.2015
        {
          ClearXml();
          return;
        }

        _XmlFilePath = null;
        _XmlBytes = null;
        _XmlDocument = value;

        if (_TempDir == null)
          _TempDir = new TempDirectory();
        AbsPath Path = new AbsPath(_TempDir.Dir, "1.xml");
        File.WriteAllBytes(Path.Path, XmlBytes);
        string url = Path.Uri.ToString();
        //MessageBox.Show(url);
        //Clipboard.SetText(url);
        base.Navigate(url); // TODO: 06.04.2018 Не работает в Mono (и в Wine+Mono). Выдает NullReferenceException
        base.Refresh();

        _SourceKind = XmlViewBoxSourceKind.XmlDocument;
        OnXmlDataChanged();
      }
    }
    private XmlDocument _XmlDocument;

    /// <summary>
    /// Очистка документа
    /// </summary>
    public void ClearXml()
    {
      if (IsEmpty)
        return;

      _XmlFilePath = null;
      _XmlBytes = null;
      _XmlDocument = null;
      base.Navigate("about:blank");
      base.Refresh();

      _SourceKind = XmlViewBoxSourceKind.None;
      OnXmlDataChanged();
    }

    /// <summary>
    /// Возвращает true, если данные не были установлены
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public bool IsEmpty
    {
      get
      {
        return String.IsNullOrEmpty(XmlFilePath) && XmlBytes == null && XmlDocument == null;
      }
    }

    /// <summary>
    /// Событие вызывается после присоединения данных (установки одного из свойств <see cref="XmlFilePath"/>, <see cref="XmlDocument"/>, <see cref="XmlBytes"/> или <see cref="XmlText"/>)
    /// </summary>
    [Description("Called when the XmlPath, XmlBytes, XmlText or XmlDocument property changed")]
    [Category("Property Changed")]
    public event EventHandler XmlDataChanged;

    protected virtual void OnXmlDataChanged()
    {
      if (XmlDataChanged != null)
        XmlDataChanged(this, EventArgs.Empty);
    }

    #endregion

    #region Как строка текста

    /// <summary>
    /// Представление XML-документа как строки текста
    /// </summary>
    [Description("XML-document to view as a String")]
    [Category("Appearance")]
    [DefaultValue("")]
    public string XmlText
    {
      get
      {
        return DataTools.XmlDocumentToString(XmlDocument);
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          ClearXml();
        else
          XmlDocument = DataTools.XmlDocumentFromString(value);
      }
    }

    #endregion

    #endregion

    #region Заглушки для свойств

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool AllowNavigation
    {
      get { return base.AllowNavigation; }
      set { base.AllowNavigation = value; }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool AllowWebBrowserDrop
    {
      get { return base.AllowWebBrowserDrop; }
      set { base.AllowWebBrowserDrop = value; }
    }

    [DefaultValue(false)]
    public new bool WebBrowserShortcutsEnabled
    {
      get { return base.WebBrowserShortcutsEnabled; }
      set { base.WebBrowserShortcutsEnabled = value; }
    }

    [DefaultValue(false)]
    public new bool IsWebBrowserContextMenuEnabled
    {
      get { return base.IsWebBrowserContextMenuEnabled; }
      set { base.IsWebBrowserContextMenuEnabled = value; }
    }

    #endregion
  }
}
