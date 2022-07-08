// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using FreeLibSet.Controls;
using FreeLibSet.Core;

#pragma warning disable 1591 // неохота делать

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Редактирование данных в XML-формате
  /// Содержит TabControl с двумя закладками: просмотром XmlViewBox и обычным 
  /// TextEditor.
  /// Данные редактируются в виде массива байт.
  /// Объект может быть помещен на форму OKCancelForm
  /// </summary>
  public class XmlEditor
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="baseProvider"></param>
    /// <param name="parentControl"></param>
    public XmlEditor(EFPBaseProvider baseProvider, Control parentControl)
    {
      _TheTabControl = new TabControl();
      _TheTabControl.Dock = DockStyle.Fill;
      _TheTabControl.ImageList = EFPApp.MainImages.ImageList;
      parentControl.Controls.Add(_TheTabControl);

      TabPage tpView = new TabPage();
      tpView.Text = "Просмотр XML";
      _TheTabControl.TabPages.Add(tpView);
      tpView.ImageKey = "View";
      tpView.ToolTipText = "Просмотр форматированного XML-документа";

      TabPage tpEdit = new TabPage();
      tpEdit.Text = "Редактирование";
      _TheTabControl.TabPages.Add(tpEdit);
      tpEdit.ImageKey = "Edit";
      tpEdit.ToolTipText = "Редактирование XML-документа в виде неформатированного текста";


      EFPControlWithToolBar<XmlViewBox> cwtView = new EFPControlWithToolBar<XmlViewBox>(baseProvider, tpView);
      _ViewBox = new EFPXmlViewBox(cwtView);

      EFPControlWithToolBar<RichTextBox> cwtEdit = new EFPControlWithToolBar<RichTextBox>(baseProvider, tpEdit);
      _EditBox = new EFPRichTextBox(cwtEdit);

      _TheTabControl.Deselecting += new TabControlCancelEventHandler(TheTabControl_Deselecting);
    }

    #endregion

    #region Управляющие элементы

    private TabControl _TheTabControl;

    public EFPXmlViewBox ViewBox { get { return _ViewBox; } }
    private EFPXmlViewBox _ViewBox;

    public EFPRichTextBox EditBox { get { return _EditBox; } }
    private EFPRichTextBox _EditBox;

    #endregion

    #region XML-данные

    /// <summary>
    /// Основное свойство. Содержит редактируемые данные в виде массива байт
    /// </summary>
    public byte[] XmlBytes
    {
      get { return _ViewBox.Control.XmlBytes; }
      set
      {
        _ViewBox.Control.XmlBytes = value;
        AfterSetXmlBytes();
      }
    }

    private void AfterSetXmlBytes()
    {
      Encoding enc = DataTools.GetXmlEncoding(_ViewBox.Control.XmlDocument);
      _EditBox.Control.Text = File.ReadAllText(_ViewBox.Control.XmlFilePath, enc);
      Modified = false;
      _OrgMD5 = DataTools.MD5Sum(_ViewBox.Control.XmlBytes);
    }

    /// <summary>
    /// Альтернативное свойство. Содержит редактируемые данные в виде объекта XmlDocument
    /// </summary>
    public XmlDocument XmlDocument
    {
      get { return _ViewBox.Control.XmlDocument; }
      set
      {
        _ViewBox.Control.XmlDocument = value;
        AfterSetXmlBytes();
      }
    }

    /// <summary>
    /// Альтернативное свойство. Содержит редактируемые данные в виде объекта XmlDocument
    /// </summary>
    public string XmlText
    {
      get { return _ViewBox.Control.XmlText; }
      set
      {
        try
        {
          _ViewBox.Control.XmlText = value;
          // может возникнуть ошибка
        }
        catch
        { 
        }
        AfterSetXmlBytes();
        // на случай ошибки при преобразовании
        _EditBox.Control.Text = value;
      }
    }

    private string _OrgMD5;

    #endregion

    #region Признак наличия изменений

    /// <summary>
    /// Признак наличия изменений.
    /// Свойство сбрасывается в false при присвоении значении свойству XmlBytes 
    /// и устанавливается в true, когда пользователь перекчается с закладки 2
    /// на закладку 1
    /// </summary>
    public bool Modified
    {
      get { return _Modified; }
      set
      {
        if (value == _Modified)
          return;
        _Modified = value;
        OnModifiedChanged();
      }
    }
    private bool _Modified;

    /// <summary>
    /// Событие вызывается при изменении свойства Modified
    /// </summary>
    public event EventHandler ModifiedChanged;

    protected virtual void OnModifiedChanged()
    {
      if (ModifiedChanged != null)
        ModifiedChanged(this, EventArgs.Empty);
    }

    #endregion

    #region Окончание редактирования

    void TheTabControl_Deselecting(object sender, TabControlCancelEventArgs args)
    {
      try
      {
        if (!CommitEdit())
          args.Cancel = true;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка окончания редактирования");
        args.Cancel = true;
      }
    }

    /// <summary>
    /// Этот метод вызывается при переключении с закладки 2 на закладку 1 и 
    /// при нажатии кнопки "ОК" в форме.
    /// Если активна вкладка "Редактирование", то метод извлекает текст из тектового
    /// редактора и пытается преобразовать его в XML. В случае успеха обновляется
    /// свойство XmlBytes и возвращается true. В случае ошибки выдается сообщение
    /// и возвращается false
    /// </summary>
    public bool CommitEdit()
    {
      if (_TheTabControl.SelectedIndex != 1)
        return true; // активна страница просмотра

      XmlDocument xmlDoc = new XmlDocument();
      try
      {
        xmlDoc.LoadXml(_EditBox.Control.Text);
      }
      catch (Exception e)
      {
        XmlException e2 = e as XmlException;
        if (e2 != null)
        {
          // позиционирование!
          // лень высчитывать
          //  FEditBox.Control.Select(
          //e2.LineNumber
        }
        EFPApp.ErrorMessageBox("Введенный текст содержит ошибки и не может быть преобразован в XML-документ. " + e.Message,
          "Ошибка в редакторе");
        return false;
      }

      Encoding enc = DataTools.GetXmlEncoding(xmlDoc);
      _ViewBox.Control.XmlBytes = enc.GetBytes(_EditBox.Control.Text);

      string newMD5 = DataTools.MD5Sum(_ViewBox.Control.XmlBytes);

      Modified = (newMD5 != _OrgMD5);

      return true;
    }

    #endregion
  }

  /// <summary>
  /// Форма для редактирования XML с кнопками "ОК" и "Отмена"
  /// </summary>
  public class XmlEdiitorForm : OKCancelForm
  {
    #region Конструктор

    public XmlEdiitorForm()
      :base(false)
    {
      Icon = EFPApp.MainImageIcon("XML");
      base.Text = "Документ XML";
      _Editor = new XmlEditor(FormProvider, MainPanel);
    }

    #endregion

    #region Свойства

    public XmlEditor Editor { get { return _Editor; } }
    private XmlEditor _Editor;

    #endregion

    #region Проверка закрытия формы

    protected override void OnFormClosing(FormClosingEventArgs args)
    {
      if ((!args.Cancel) && DialogResult == DialogResult.OK)
      {
        try
        {
          if (!Editor.CommitEdit())
            args.Cancel = true;
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка закрытия формы редактора");
          args.Cancel = true;
        }
      }

      base.OnFormClosing(args);
    }

    #endregion
  }
}
