// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FreeLibSet.Core;
using FreeLibSet.Controls;

// Этот модуль содержит закладки отчета с управляющими элементами, отличными от табличных просмотров

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Закладка с произвольным управляющим элементом.
  /// Обычно используется для создания сложных страниц с несколькими элементами управления.
  /// Для страниц с единственным элементом управления лучше использовать специализированные классы,
  /// например, <see cref="EFPReportGridPage"/>.
  /// </summary>
  /// <remarks>
  /// Часто в качестве управляющего элемента используется <see cref="Panel"/> или другой контейнерный элемент,
  /// который берется из другого объекта <see cref="Form"/>. 
  /// Панель содержит несколько других элементов, для которых нужны провайдеры, производные от <see cref="EFPControlBase"/>.
  /// Для создания провайдеров управляющих элементов нельзя создавать провайдер формы <see cref="EFPFormProvider"/>, 
  /// т.к. сама форма, на которой расположены элементы, не используется и провайдер работать не будет. 
  /// Вместо этого следует использовать свойство <see cref="EFPReportPage.BaseProvider"/> этого объекта. 
  /// Форма-заготовка обычно должна иметь конструктор с аргументом <see cref="EFPBaseProvider"/>.
  /// Если форма-заготовка используется не только в данном отчете, но и самостоятельно, у нее должен быть также
  /// конструктор, создающий свой <see cref="EFPFormProvider"/>.
  /// 
  /// При добавлении табличных просмотров, устанавливайте свойство 
  /// <see cref="EFPDataGridViewCommandItems.UseRefresh"/> = false, чтобы работала команда "Обновить отчет".
  /// </remarks>
  public class EFPReportControlPage : EFPReportPage
  {
    #region Конструктор

    /// <summary>
    /// Создает страницу
    /// </summary>
    public EFPReportControlPage()
    {
      _Control = null;
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Запоминает родительский управляющий элемент (панель), на которой будет 
    /// создана страница отчета.  
    /// </summary>
    /// <param name="parentControl">Пустая панель, на которой будет расположена страница</param>
    public override void AssignParentControl(Panel parentControl)
    {
      base.AssignParentControl(parentControl);

      // Сразу присоединяем свой управляющий элемент, иначе для него не будет вызван Disposing()
      SetPageCreated();
    }

    /// <summary>
    /// Создает страницу.
    /// Если свойству <see cref="Control"/> не было присвоено значение, то создается метка с текстом "Нет данных".
    /// </summary>
    /// <param name="parent">Панель, на которой размещается страница</param>
    protected override void CreatePage(Panel parent)
    {
      if (_Control == null)
      {
        _Control = new Label();
        _Control.Text = "Нет данных";
      }
      _Control.Dock = DockStyle.Fill;

      // TODO: Требуется заменять все имена

      parent.Controls.Add(_Control);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основной управляюший элемент. 
    /// </summary>
    public Control Control
    {
      get { return _Control; }
      set
      {
#if DEBUG
        if (value == null)
          throw new ArgumentNullException();
#endif

        Control parentControl = null;
        if (base.PageCreated)
        {
#if DEBUG
          if (_Control == null)
            throw new BugException("Control==null при PageCreated==true");
#endif
          parentControl = _Control.Parent;
        }

        if (_Control != null)
        {
          _Control.Parent = null;
          _Control.Dispose();
        }

        _Control = value;

        if (base.PageCreated)
        {
          _Control.Dock = DockStyle.Fill;
          parentControl.Controls.Add(_Control);
        }
      }
    }
    private Control _Control;

    #endregion
  }

  /// <summary>
  /// Закладка с просмотром текстового-документв
  /// </summary>
  public class EFPReportTextViewPage : EFPReportPage
  {
    #region Конструктор

    /// <summary>
    /// Создает страницу отчета
    /// </summary>
    public EFPReportTextViewPage()
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Загрузка данных как текста
    /// Свойство можно установить до создания самого просмотра (вызова SetPageCreated)
    /// </summary>
    public string Text
    {
      get
      {
        if (_TheTextBox == null)
          return _Text;
        else
          return _TheTextBox.Control.Text;
      }
      set
      {
        if (_TheTextBox == null)
          _Text = value;
        else
          _TheTextBox.Control.Text = value;
      }
    }
    private string _Text;

    /// <summary>
    /// Возвращает true, если данные не были установлены
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return String.IsNullOrEmpty(Text);
      }
    }

    /// <summary>
    /// Провайдер основного управляющего элемента.
    /// Он появляется только после создания страницы.
    /// Для форсированного создания страницы используйте SetPageCreated()
    /// </summary>
    public EFPTextBox TheTextBox { get { return _TheTextBox; } }
    private EFPTextBox _TheTextBox;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Создание страницы
    /// </summary>
    /// <param name="parent"></param>
    protected override void CreatePage(Panel parent)
    {
      bool isEmpty2 = IsEmpty;

      EFPControlWithToolBar<TextBox> cwt = new EFPControlWithToolBar<TextBox>(BaseProvider, parent);
      cwt.Control.Multiline = true;
      cwt.Control.ScrollBars = ScrollBars.Both;
      cwt.Control.WordWrap = false;
      cwt.Control.ReadOnly = true;
      _TheTextBox = new EFPTextBox(cwt);

      if (!isEmpty2)
        _TheTextBox.Control.Text = _Text;

      _Text = null;

      // Убрано 21.05.2022. См. EFPReportGridPage.CreateControlProvider()
      //_TheTextBox.PrepareCommandItems(); // активация TooBar
      SetControlName(_TheTextBox, "TextBox");
    }

    #endregion
  }


  // TODO: Не уверен, что надо использовать EFPWebBrowser, а не какой-нибудь производный класс
#if XXX
  /// <summary>
  /// Закладка с просмотром HTML-документв
  /// </summary>
  public class EFPReportHtmlViewPage : EFPReportPage
  {
    #region Конструктор

    public EFPReportHtmlViewPage()
    {
      FSimpleText = "Ждите...";
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер для просмотра
    /// </summary>
    public EFPWebBrowser HtmlView { get { return FHtmlView; } }
    private EFPWebBrowser FHtmlView;

    /// <summary>
    /// Задание простого текста для страницы
    /// </summary>
    public string SimpleText
    {
      get
      {
        if (HtmlView == null)
          return FSimpleText;
        else
        {
          return HtmlView.Control.Document.Body.InnerText;
        }
      }
      set
      {
        if (HtmlView == null)
          FSimpleText = value;
        else
        {
          HtmlView.Control.Document.Body.InnerText = value;
          HtmlView.Control.Update();
        }
      }
    }
    private string FSimpleText;

    #endregion

    #region Переопределенные методы

    protected override void CreatePage(Panel Parent)
    {
      EFPControlWithToolBar<WebBrowser> cwt = new EFPControlWithToolBar<WebBrowser>(BaseProvider, Parent);
      FHtmlView = new EFPWebBrowser(cwt);
      FHtmlView.Control.DocumentText = String.Empty;
      FHtmlView.Control.Document.Write(FSimpleText);

      FHtmlView.GetReadyCommandItems(); // активация TooBar
    }

    #endregion
  }
#endif

  /// <summary>
  /// Закладка с просмотром XML-документв
  /// </summary>
  public class EFPReportXmlViewPage : EFPReportPage
  {
    #region Конструктор

    /// <summary>
    /// Создает страницу отчета
    /// </summary>
    public EFPReportXmlViewPage()
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Путь к внешнему XML-файлу для просмотра
    /// Свойство можно установить до создания самого просмотра (вызова <see cref="EFPReportPage.SetPageCreated()"/>)
    /// </summary>
    public string XmlFilePath
    {
      get
      {
        if (_XmlView == null)
          return _XmlFilePath;
        else
          return _XmlView.Control.XmlFilePath;
      }
      set
      {
        if (_XmlView == null)
          _XmlFilePath = value;
        else
          _XmlView.Control.XmlFilePath = value;
        _XmlBytes = null;
        _XmlDocument = null;
        _XmlText = null;
      }
    }
    private string _XmlFilePath;

    /// <summary>
    /// Загрузка документа XML из памяти
    /// Свойство можно установить до создания самого просмотра (вызова <see cref="EFPReportPage.SetPageCreated()"/>)
    /// </summary>
    public byte[] XmlBytes
    {
      get
      {
        if (_XmlView == null)
          return _XmlBytes;
        else
          return _XmlView.Control.XmlBytes;
      }
      set
      {
        if (_XmlView == null)
          _XmlBytes = value;
        else
          _XmlView.Control.XmlBytes = value;
        _XmlFilePath = null;
        _XmlDocument = null;
        _XmlText = null;
      }
    }
    private byte[] _XmlBytes;

    /// <summary>
    /// Загрузка данных как XML-документа
    /// Свойство можно установить до создания самого просмотра (вызова <see cref="EFPReportPage.SetPageCreated()"/>)
    /// </summary>
    public XmlDocument XmlDocument
    {
      get
      {
        if (_XmlView == null)
          return _XmlDocument;
        else
          return _XmlView.Control.XmlDocument;
      }
      set
      {
        if (_XmlView == null)
          _XmlDocument = value;
        else
          _XmlView.Control.XmlDocument = value;
        _XmlFilePath = null;
        _XmlBytes = null;
        _XmlText = null;
      }
    }
    private XmlDocument _XmlDocument;

    /// <summary>
    /// Загрузка данных как строки XML
    /// Свойство можно установить до создания самого просмотра (вызова SetPageCreated)
    /// </summary>
    public string XmlText
    {
      get
      {
        if (_XmlView == null)
          return _XmlText;
        else
          return _XmlView.Control.XmlText;
      }
      set
      {
        if (_XmlView == null)
          _XmlText = value;
        else
          _XmlView.Control.XmlText = value;
        _XmlFilePath = null;
        _XmlBytes = null;
        _XmlDocument = null;
      }
    }
    private string _XmlText;

    /// <summary>
    /// Возвращает true, если данные не были установлены
    /// </summary>
    public bool IsEmpty
    {
      get
      {
        return String.IsNullOrEmpty(XmlFilePath) && XmlBytes == null && XmlDocument == null && String.IsNullOrEmpty(XmlText);
      }
    }

    /// <summary>
    /// Провайдер основного управляющего элемента.
    /// Он появляется только после создания страницы.
    /// Для форсированного создания страницы используйте <see cref="EFPReportPage.SetPageCreated()"/>.
    /// </summary>
    public EFPXmlViewBox XmlView { get { return _XmlView; } }
    private EFPXmlViewBox _XmlView;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Создание страницы
    /// </summary>
    /// <param name="parent">Панель отчета, куда добавляется просмотр</param>
    protected override void CreatePage(Panel parent)
    {
      bool isEmpty2 = IsEmpty;

      EFPControlWithToolBar<XmlViewBox> cwt = new EFPControlWithToolBar<XmlViewBox>(BaseProvider, parent);
      _XmlView = new EFPXmlViewBox(cwt);

      if (!isEmpty2)
      {
        EFPApp.BeginWait("Загрузка XML-просмотра", "XML");
        try
        {
          if (!String.IsNullOrEmpty(_XmlFilePath))
            _XmlView.Control.XmlFilePath = _XmlFilePath;
          else if (_XmlBytes != null)
            _XmlView.Control.XmlBytes = _XmlBytes;
          else if (_XmlDocument != null)
            _XmlView.Control.XmlDocument = _XmlDocument;
          else if (!String.IsNullOrEmpty(_XmlText))
            _XmlView.Control.XmlText = _XmlText;
        }
        finally
        {
          EFPApp.EndWait();
        }
      }

      _XmlFilePath = null;
      _XmlBytes = null;
      _XmlDocument = null;
      _XmlText = null;

      // Убрано 21.05.2022. См. EFPReportGridPage.CreateControlProvider()
      //_XmlView.PrepareCommandItems(); // активация TooBar
      SetControlName(_XmlView, "XmlView");
    }

    #endregion
  }
}
