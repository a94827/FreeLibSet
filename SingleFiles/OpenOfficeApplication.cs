// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.ComponentModel;
using FreeLibSet.IO;
using FreeLibSet.Core;
using FreeLibSet.Shell;

/*
 * Excel-подобная модель для работы с  OpenOffice / LibreOffice
 * 
 * Этот модуль не входит в ExtForms.dll, т.к. требует компиляции библиотек cli_*.dll,
 * входящих в состав OpenOffice SDK
 * 
 * Содержит набор структур-оберток для классов CLI, таких, как unoidl.com.sun.star.sheet.XSpreadsheet.
 * Для верхнего уровня предусмотрен IDisposable-класс
 */

namespace FreeLibSet.OpenOffice
{
  /// <summary>
  /// Хранит интерфейсы XComponentContext, XMultiServiceFactory и XComponentLoader 
  /// </summary>
  public class OpenOfficeApplication : DisposableObject
  {
    #region Конструктор и Dispose

    public OpenOfficeApplication()
      : this(GetDefaultOfficeInfo())
    {
    }

    private static OpenOfficeTools.OfficeInfo GetDefaultOfficeInfo()
    {
      if (OpenOfficeTools.Installations.Length == 0)
        throw new InvalidOperationException("На Вашем компьютере нет установленного OpenOffice или LibreOffice");

      string errorText = null; // чтобы не было предупреждения компилятора
      for (int i = 0; i < OpenOfficeTools.Installations.Length; i++)
      {
        if (OpenOfficeTools.Installations[i].Kind == OpenOfficeKind.LibreOffice)
        {
          errorText = "LibreOffice пока не поддерживается";
          continue;
        }

        // Найдена подходящая копия
        errorText = null;
        return OpenOfficeTools.Installations[i];
      }

      // Нет подходящей копии.
      // ErrorText уже содержит описание ошибки
      throw new InvalidOperationException(errorText);
    }

    public OpenOfficeApplication(OpenOfficeTools.OfficeInfo officeInfo)
    {
      if (officeInfo == null)
        throw new ArgumentNullException("officeInfo", "Не указана используемая версия офиса");

      _OfficeInfo = officeInfo;

      _Visible = true;

      string v1 = Environment.GetEnvironmentVariable("UNO_PATH");
      string v2 = Environment.GetEnvironmentVariable("PATH");
      try
      {
        Environment.SetEnvironmentVariable("UNO_PATH", officeInfo.ProgramDir.Path);
        Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + officeInfo.ProgramDir.Path);

        _XComponentContext = uno.util.Bootstrap.bootstrap();
        _XMultiServiceFactory = (unoidl.com.sun.star.lang.XMultiServiceFactory)(XComponentContext.getServiceManager());
        _XComponentLoader = (unoidl.com.sun.star.frame.XComponentLoader)XMultiServiceFactory.createInstance("com.sun.star.frame.Desktop");
      }
      finally
      {
        Environment.SetEnvironmentVariable("UNO_PATH", v1);
        Environment.SetEnvironmentVariable("PATH", v2);
      }

      /*
      try
      {
        //FTheWbk.Caption = "Обработка в АССОО2";
      }
      catch
      {
        //Desktop.Quit();
        throw;
      } */

      //unoidl.com.sun.star.lang.XMultiServiceFactory msf = FXMultiServiceFactory;
      //string[] aaa = msf.getAvailableServiceNames();
      //Array.Sort<string>(aaa);


      QuitOnDispose = true;
    }

    protected override void Dispose(bool disposing)
    {
      if (XComponentLoader != null)
      {
        try
        {
          // 21.01.2016
          // Если в Excel Application были открыты посторонние книги, например, тыканьем в Проводнике Windows,
          // то не надо закрывать приложение, иначе пользователь будет недоволен

          //!!!if (QuitOnDispose &&
          //!!!App.Workbooks.Count == 0) // 21.01.2016
          //!!!App.Quit();
        }
        catch
        {
        }
        _XComponentLoader = null;
        _XMultiServiceFactory = null;
        _XComponentContext = null;
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Информация об используемой версии офиса
    /// </summary>
    public OpenOfficeTools.OfficeInfo OfficeInfo { get { return _OfficeInfo; } }
    private OpenOfficeTools.OfficeInfo _OfficeInfo;

    /// <summary>
    /// Используется при открытии документов.
    /// Если свойство сброшено в false, то документы будут открываться в "невидимом" режиме.
    /// По умолчанию - true.
    /// Свойство используется только при открытии документов
    /// </summary>
    public bool Visible
    {
      get { return _Visible; }
      set { _Visible = value; }
    }
    private bool _Visible;

    #endregion

    #region Интерфейсы

    /// <summary>
    /// Интерфейс XComponentContext
    /// </summary>
    public unoidl.com.sun.star.uno.XComponentContext XComponentContext { get { return _XComponentContext; } }
    private unoidl.com.sun.star.uno.XComponentContext _XComponentContext;

    /// <summary>
    /// Интерфейс XMultiServiceFactory 
    /// </summary>
    public unoidl.com.sun.star.lang.XMultiServiceFactory XMultiServiceFactory { get { return _XMultiServiceFactory; } }
    private unoidl.com.sun.star.lang.XMultiServiceFactory _XMultiServiceFactory;

    /// <summary>
    /// Интерфейс XComponentLoader
    /// </summary>
    public unoidl.com.sun.star.frame.XComponentLoader XComponentLoader { get { return _XComponentLoader; } }
    private unoidl.com.sun.star.frame.XComponentLoader _XComponentLoader;

    public unoidl.com.sun.star.frame.XDesktop XDesktop { get { return _XComponentLoader as unoidl.com.sun.star.frame.XDesktop; } }

    // В документации (https://wiki.openoffice.org/wiki/Documentation/DevGuide/OfficeDev/Using_the_Desktop) 
    // сказано, что интерфейс XFrame в Desktop является недействующим
    // Надо использовать XFramesSupplier
    // public unoidl.com.sun.star.frame.XFrame XFrame { get { return FXComponentLoader as unoidl.com.sun.star.frame.XFrame;} }

    public unoidl.com.sun.star.frame.XDispatchProvider XDispatchProvider { get { return _XComponentLoader as unoidl.com.sun.star.frame.XDispatchProvider; } }

    public unoidl.com.sun.star.frame.XDispatchInformationProvider XDispatchInformationProvider { get { return _XComponentLoader as unoidl.com.sun.star.frame.XDispatchInformationProvider; } }

    public unoidl.com.sun.star.frame.XDispatchProviderInterception XDispatchProviderInterception { get { return _XComponentLoader as unoidl.com.sun.star.frame.XDispatchProviderInterception; } }

    public unoidl.com.sun.star.frame.XFramesSupplier XFramesSupplier { get { return _XComponentLoader as unoidl.com.sun.star.frame.XFramesSupplier; } }

    public unoidl.com.sun.star.task.XStatusIndicatorFactory XStatusIndicatorFactory { get { return _XComponentLoader as unoidl.com.sun.star.task.XStatusIndicatorFactory; } }

    public unoidl.com.sun.star.beans.XPropertySet XPropertySet { get { return _XComponentLoader as unoidl.com.sun.star.beans.XPropertySet; } }

    #endregion

    #region Свойства службы Frame

    public string Title
    {
      get
      {
        return DataTools.GetString(XPropertySet.getPropertyValue("Title").Value);
      }
    }
    public void SetTitle(string value)
    {
      XPropertySet.setPropertyValue("Title", new uno.Any(value));
    }

    public void Activate()
    {
      unoidl.com.sun.star.frame.XFrame xFrame = XFramesSupplier.getActiveFrame();
      if (xFrame != null)
      {
        unoidl.com.sun.star.awt.XWindow wnd = xFrame.getContainerWindow();
        wnd.setFocus();
      }
    }

    #endregion

    #region Дополнительные свойства

    public bool QuitOnDispose;

    #endregion
  }

  /// <summary>
  /// Размещение управляющих элементов поверх листа Excel
  /// </summary>
  public struct DrawPage
  {
    #region Конструктор

    public DrawPage(unoidl.com.sun.star.drawing.XDrawPage xDrawPage, unoidl.com.sun.star.lang.XMultiServiceFactory xMultiServiceFactory)
    {
      if (xDrawPage == null)
        throw new ArgumentNullException("xDrawPage");
      if (xMultiServiceFactory == null)
        throw new ArgumentNullException("xMultiServiceFactory");

      _XDrawPage = xDrawPage;
      _XMultiServiceFactory = xMultiServiceFactory;
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.drawing.XDrawPage XDrawPage { get { return _XDrawPage; } }
    private unoidl.com.sun.star.drawing.XDrawPage _XDrawPage;

    public unoidl.com.sun.star.lang.XMultiServiceFactory XMultiServiceFactory { get { return _XMultiServiceFactory; } }
    private unoidl.com.sun.star.lang.XMultiServiceFactory _XMultiServiceFactory;

    public unoidl.com.sun.star.form.XFormsSupplier XFormsSupplier { get { return _XDrawPage as unoidl.com.sun.star.form.XFormsSupplier; } }

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XDrawPage != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion

    #region Добавление элементов

    /// <summary>
    /// Добавление командной кнопки
    /// </summary>
    /// <param name="x">Левая граница в единицах 0.01мм</param>
    /// <param name="y">Верхняя граница в единицах 0.01мм</param>
    /// <param name="width">Ширина в единицах 0.01мм</param>
    /// <param name="height">Высота в единицах 0.01мм</param>
    /// <param name="label">Текст на кнопке</param>
    /// <param name="click">Обработчик события нажатия кнопки</param>
    /// <returns>Объект кнопки</returns>
    public CommandButton AddButton(int x, int y, int width, int height, string label, EventHandler click)
    {
      return AddButton(new unoidl.com.sun.star.awt.Point(x, y),
        new unoidl.com.sun.star.awt.Size(width, height),
        label, click);
    }

    public CommandButton AddButton(unoidl.com.sun.star.awt.Point position,
      unoidl.com.sun.star.awt.Size size,
      string label, EventHandler click)
    {
      unoidl.com.sun.star.drawing.XControlShape shp = XMultiServiceFactory.createInstance("com.sun.star.drawing.ControlShape") as unoidl.com.sun.star.drawing.XControlShape;
      shp.setPosition(position);
      shp.setSize(size);

      unoidl.com.sun.star.awt.XControlModel btnModel = XMultiServiceFactory.createInstance("com.sun.star.form.component.CommandButton") as unoidl.com.sun.star.awt.XControlModel;
      //unoidl.com.sun.star.awt.XControlModel btnModel = XMultiServiceFactory.createInstance("com.sun.star.awt.UnoControlButtonModel") as unoidl.com.sun.star.awt.XControlModel;
      shp.setControl(btnModel);

      CommandButton Control = new CommandButton(btnModel);
      Control.SetLabel(label);
      Control.Click += click;

      XDrawPage.add(shp);

      return Control;
    }

    #endregion
  }

  /// <summary>
  /// Командная кнопка
  /// </summary>
  public class CommandButton : unoidl.com.sun.star.awt.XActionListener
  {
    // Не может быть структурой,
    // т.к. вешаем себя в качестве обработчика события

    #region Конструктор

    public CommandButton(unoidl.com.sun.star.awt.XControlModel xControlModel)
    {
      if (xControlModel == null)
        throw new ArgumentNullException("xControlModel");

      _XControlModel = xControlModel;
      unoidl.com.sun.star.awt.XControl ctrl = xControlModel as unoidl.com.sun.star.awt.XControl;


      unoidl.com.sun.star.beans.XPropertySetInfo psi = XPropertySet.getPropertySetInfo();
      unoidl.com.sun.star.beans.Property[] props = psi.getProperties();

      //      XButton.addActionListener(this);
    }

    #endregion

    #region Интерфейсы

    public unoidl.com.sun.star.awt.XControlModel XControlModel { get { return _XControlModel; } }
    private unoidl.com.sun.star.awt.XControlModel _XControlModel;

    public unoidl.com.sun.star.lang.XComponent XComponent { get { return _XControlModel as unoidl.com.sun.star.lang.XComponent; } }

    public unoidl.com.sun.star.beans.XPropertySet XPropertySet { get { return _XControlModel as unoidl.com.sun.star.beans.XPropertySet; } }

    public unoidl.com.sun.star.io.XPersistObject XPersistObject { get { return _XControlModel as unoidl.com.sun.star.io.XPersistObject; } }

    public unoidl.com.sun.star.util.XCloneable XCloneable { get { return _XControlModel as unoidl.com.sun.star.util.XCloneable; } }


    public unoidl.com.sun.star.form.XFormComponent XFormComponent { get { return _XControlModel as unoidl.com.sun.star.form.XFormComponent; } }

    public unoidl.com.sun.star.container.XNamed XNamed { get { return _XControlModel as unoidl.com.sun.star.container.XNamed; } }

    public unoidl.com.sun.star.form.submission.XSubmissionSupplier XSubmissionSupplier { get { return _XControlModel as unoidl.com.sun.star.form.submission.XSubmissionSupplier; } }

    public unoidl.com.sun.star.form.XImageProducerSupplier XImageProducerSupplier { get { return _XControlModel as unoidl.com.sun.star.form.XImageProducerSupplier; } }

    public unoidl.com.sun.star.form.XReset XReset { get { return _XControlModel as unoidl.com.sun.star.form.XReset; } }

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XControlModel != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion

    #region Свойства

    public string Label
    {
      get { return DataTools.GetString(XPropertySet.getPropertyValue("Label").Value); }
    }

    public void SetLabel(string value)
    {
      XPropertySet.setPropertyValue("Label", new uno.Any(value));
    }

    #endregion

    #region События

    public event EventHandler Click;

    public void OnClick()
    {
      if (Click != null)
        Click(this, EventArgs.Empty);
    }

    #endregion

    #region XActionListener Members

    void unoidl.com.sun.star.awt.XActionListener.actionPerformed(unoidl.com.sun.star.awt.ActionEvent args)
    {
      OnClick();
    }

    void unoidl.com.sun.star.lang.XEventListener.disposing(unoidl.com.sun.star.lang.EventObject source)
    {
      //      XButton.removeActionListener(this);
    }

    #endregion
  }

  /// <summary>
  /// Выполнение команд, как в макросах OpenOffice
  /// </summary>
  public struct DispatchHelper
  {
    #region Конструкторы

    public DispatchHelper(unoidl.com.sun.star.frame.XDispatchHelper xDispatchHelper)
    {
      if (xDispatchHelper == null)
        throw new ArgumentNullException("xDispatchHelper");
      _XDispatchHelper = xDispatchHelper;
    }

    public DispatchHelper(unoidl.com.sun.star.lang.XMultiServiceFactory xMultiServiceFactory)
      : this(GetXDispatchHelper(xMultiServiceFactory))
    {
    }

    private static unoidl.com.sun.star.frame.XDispatchHelper GetXDispatchHelper(unoidl.com.sun.star.lang.XMultiServiceFactory xMultiServiceFactory)
    {
      object obj = xMultiServiceFactory.createInstance("com.sun.star.frame.DispatchHelper");
      if (obj == null)
      {
        Exception e = new NotSupportedException("Переданный интерфейс XMultiServiceFactory не умеет создавать службу com.sun.star.frame.DispatchHelper");
        e.Data["AvailableServiceNames"] = xMultiServiceFactory.getAvailableServiceNames();
        throw e;
      }
      return obj as unoidl.com.sun.star.frame.XDispatchHelper;
    }

    #endregion

    #region Интерфейсы

    /// <summary>
    /// Интерфейс выполнения команды
    /// </summary>
    public unoidl.com.sun.star.frame.XDispatchHelper XDispatchHelper { get { return _XDispatchHelper; } }
    private unoidl.com.sun.star.frame.XDispatchHelper _XDispatchHelper;

    #endregion

    #region Существование интерфейса

    /// <summary>
    /// Возвращшает true, если структура инициализирована
    /// </summary>
    public bool Exists { get { return _XDispatchHelper != null; } }

    public void CheckIfExists()
    {
      if (!Exists)
        throw new NullReferenceException("Структура не была инициализирована");
    }

    #endregion

    #region Выполнение команды

    public void Execute(unoidl.com.sun.star.frame.XDispatchProvider xDispatchProvider, string url,
      string[] propNames, object[] propValues)
    {
      CheckIfExists();
      if (xDispatchProvider == null)
        throw new ArgumentNullException("xDispatchProvider");
      if (String.IsNullOrEmpty(url))
        throw new ArgumentNullException("url");
      if (propNames == null)
        throw new ArgumentNullException("propNames");
      if (propValues == null)
        throw new ArgumentNullException("propValues");
      if (propValues.Length != propNames.Length)
        throw new ArgumentException("Количество значений должно быть равно количеству имен", "propValues");

      unoidl.com.sun.star.beans.PropertyValue[] props = new unoidl.com.sun.star.beans.PropertyValue[propNames.Length];
      for (int i = 0; i < props.Length; i++)
      {
        if (String.IsNullOrEmpty(propNames[i]))
          throw new ArgumentNullException("propNames[" + i.ToString() + "]");

        props[i] = new unoidl.com.sun.star.beans.PropertyValue();
        props[i].Name = propNames[i];
        if (propValues[i] == null)
          props[i].Value = uno.Any.VOID;
        else
          props[i].Value = new uno.Any(props[i].GetType(), props[i]);
      }

      XDispatchHelper.executeDispatch(xDispatchProvider, url, String.Empty, 0, props);
    }

    #endregion
  }
}
