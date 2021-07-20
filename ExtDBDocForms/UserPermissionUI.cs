using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.ExtDB;
using AgeyevAV.ExtDB.Docs;
using System.Windows.Forms;
using AgeyevAV.Config;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

/*
 * Пользовательский интерфейс для разрешений.
 * Общие классы
 */

namespace AgeyevAV.ExtForms.Docs
{
  /// <summary>
  /// Заполняемая информация о разрешении
  /// </summary>
  public class UserPermissionUIInfo : EventArgs
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект.
    /// Свойство ObjectImageKey устанавливается в "UserPermission",
    /// а ValueImageKey - в "EmptyImage"
    /// </summary>
    public UserPermissionUIInfo()
    {
      _ObjectImageKey = "UserPermission";
      _ValueImageKey = "EmptyImage";
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Отображаемое имя класса разрешения
    /// </summary>
    public string DisplayName
    {
      get { return _DisplayName; }
      set { _DisplayName = value; }
    }
    private string _DisplayName;

    /// <summary>
    /// Имя изображения, связанное с классом разрешения или объектом (например, таблицей) для данного разрешения
    /// </summary>
    public string ObjectImageKey
    {
      get { return _ObjectImageKey; }
      set { _ObjectImageKey = value; }
    }
    private string _ObjectImageKey;

    /// <summary>
    /// Имя изображения для отображения состояния разрешения (обычно, цветной кружок)
    /// </summary>
    public string ValueImageKey
    {
      get { return _ValueImageKey; }
      set { _ValueImageKey = value; }
    }
    private string _ValueImageKey;

    #endregion
  }

  /// <summary>
  /// Интерфейс для одного вида разрешений.
  /// Хранится в коллекции UserPermissionsUI
  /// </summary>
  public abstract class UserPermissionUI : ObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает интерфейс для вида разрешений
    /// </summary>
    /// <param name="classCode">Код класса разрешений</param>
    public UserPermissionUI(string classCode)
      : base(classCode)
    {
      _UseInRB = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Код класса разрешений
    /// </summary>
    public string ClassCode { get { return base.Code; } }

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то разрешения данного типа можно хранить в справочнике
    /// "Пользователи" (и других, если используется)
    /// Свойство должно быть сброшено в false, если данный вид разрешения может устанавливаться только программным
    /// способом
    /// </summary>
    public bool UseInRB
    {
      get { return _UseInRB; }
      set { _UseInRB = value; }
    }
    private bool _UseInRB;

    /// <summary>
    /// Отображаемое имя, используемое при выборе разрешения для добавления в справочник
    /// Если свойство не установлено явно, возвращается ClassCode
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
          return ClassCode;
        else
          return _DisplayName;
      }
      set
      {
        _DisplayName = value;
      }
    }
    private string _DisplayName;

    /// <summary>
    /// Значок, используемый в списке для выбора разрешения
    /// </summary>
    public string ImageKey
    {
      get
      {
        if (String.IsNullOrEmpty(_ImageKey))
          return "UserPermission";
        else
          return _ImageKey;
      }
      set
      {
        _ImageKey = value;
      }
    }
    private string _ImageKey;

    #endregion

    #region Получение информации о разрешении

    /// <summary>
    /// Переопределенный метод может заполнить информацию о разрешении
    /// </summary>
    /// <param name="permission">Разрешение, откуда берется информация</param>
    /// <param name="info">Заполняемый объект</param>
    public virtual void GetInfo(UserPermission permission, UserPermissionUIInfo info)
    {
    }

    /// <summary>
    /// Метод должен:
    /// 1. Присоединить к Editor.ParentControl управляющий элемент (панель) для редактирования формы.
    /// 2. Создать объекты EFPControl. 
    /// 3. Учесть свойство Editor.ReadOnly
    /// 4. Добавить обработчики событий Editor.ReadValues и WriteValues
    /// </summary>
    /// <param name="editor">Объект редактора</param>
    public virtual void CreateEditor(UserPermissionEditor editor)
    {
      throw new NotSupportedException("Редактирование разрешения " + DisplayName + " не реализовано");
    }

    #endregion

    #region Просмотр разрешения

    /// <summary>
    /// Показывает окно просмотра разрешения
    /// </summary>
    /// <param name="permission"></param>
    public void View(UserPermission permission)
    {
      if (permission == null)
        throw new ArgumentNullException("permission");

      if (!UseInRB)
      {
        EFPApp.ErrorMessageBox("Просмотр разрешений \"" + DisplayName + "\" не предусмотрен", this.DisplayName);
        return;
      }

      OKCancelForm Form = new OKCancelForm(false);
      WinFormsTools.OkCancelFormToOkOnly(Form);
      Form.Text = this.DisplayName;
      Form.Icon = EFPApp.MainImageIcon(this.ImageKey);

      UserPermissionEditor Editor = new UserPermissionEditor(this, true, Form.FormProvider);
      CreateEditor(Editor);
      Form.MainPanel.Controls.Add(Editor.Control);
      Form.MainPanel.AutoSize = true;
      //Form.MainPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      Form.AutoSize = true;
      //Form.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      Editor.PerformReadValues(permission);
      EFPApp.ShowFormOrDialog(Form);
    }

    #endregion
  }

  /// <summary>
  /// Коллекция обработчиков пользовательского интерфейса для классов разрешений
  /// </summary>
  public sealed class UserPermissionsUI : NamedCollection<UserPermissionUI>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public UserPermissionsUI()
      : this(new UserPermissionCreators())
    {
    }

    /// <summary>
    /// Создает список, используя заданные генераторы разрешений
    /// </summary>
    /// <param name="creators">Генераторы разрешений</param>
    public UserPermissionsUI(UserPermissionCreators creators)
    {
      if (creators == null)
        throw new ArgumentNullException("creators");
      //Creators.CheckNotReadOnly();
      //Creators.SetReadOnly();
      _Creators = creators;
      _InfoCache = new UserPermissionInfoCache(this);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Список генераторов разрешений
    /// </summary>
    public UserPermissionCreators Creators { get { return _Creators; } }
    private UserPermissionCreators _Creators;

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Добавляет интерфейс для класса разрешения
    /// </summary>
    /// <param name="item">Интерфейс для класса разрешения</param>
    public new void Add(UserPermissionUI item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      base.Remove(item.ClassCode);
      base.Add(item);
      if (!Creators.Contains(item.ClassCode))
        throw new InvalidOperationException("Нельзя добавить интерфейс разрешения \"" + item.ClassCode + "\", для которого нет объекта в списке Creators");
    }

    /// <summary>
    /// Установка состояния IsReadOnly=true.
    /// Также переводит в состояние IsReadOnly список Creators
    /// </summary>
    public new void SetReadOnly()
    {
      base.SetReadOnly();
      _Creators.SetReadOnly();
    }

    #endregion

    #region Получение информации о разрешении

    /// <summary>
    /// Получить информацию для разрешения
    /// </summary>
    /// <param name="permission">Разрешение</param>
    /// <returns>Информация о разрешении</returns>
    public UserPermissionUIInfo GetInfo(UserPermission permission)
    {
      if (permission == null)
        throw new ArgumentNullException("permission");

      UserPermissionUIInfo Info = new UserPermissionUIInfo();
      UserPermissionUI UI;
      if (permission is UnknownUserPermission)
        UI = TheUnknownUserPermissionUI;
      else
        UI = this[permission.ClassCode];
      if (UI == null)
      {
        //UI.DisplayName = "Класс \"" + permission.ClassCode + "\"";
        Info.DisplayName = "Класс \"" + permission.ClassCode + "\""; // 28.12.2020
        Info.ObjectImageKey = "UnknownState";
      }
      else
      {
        Info.DisplayName = UI.DisplayName;
        Info.ObjectImageKey = UI.ImageKey; // обобщенное изображение
        try
        {
          UI.GetInfo(permission, Info);
        }
        catch (Exception/* e*/) // 16.06.2017
        {
          // UI.DisplayName += ". !!! Ошибка. " + e.Message;
          Info.ValueImageKey = "Error";
        }
      }
      return Info;
    }

    #endregion

    #region Поддержка табличных просмотров

    #region Кэш разрешений

    /// <summary>
    /// Кэшированная коллекция разрешений, в которой ключом являются код класса разрешения плюс
    /// XML-значение разрешения (как текст), а значением - объекты UserPermission
    /// </summary>
    public class UserPermissionInfoCache
    {
      #region Конструктор

      internal UserPermissionInfoCache(UserPermissionsUI owner)
      {
        _Owner = owner;
        _Dict = new Dictionary<string, UserPermission>();
      }

      #endregion

      #region Свойства

      private UserPermissionsUI _Owner;

      private Dictionary<string, UserPermission> _Dict;

      /// <summary>
      /// Получить кэшированное значение.
      /// Если в кэше еще нет подходящего объекта разрешение, он создается.
      /// Если <paramref name="classCode"/> задает разрешение, которое не зарегистрировано в 
      /// списке UserPermissionsUI, то создается и сохраняется объект UnknownUserPermission.
      /// </summary>
      /// <param name="classCode">код класса разрешения</param>
      /// <param name="xmlText">XML-значение разрешения (как текст)</param>
      /// <returns>Объект разрешения</returns>
      public UserPermission this[string classCode, string xmlText]
      {
        get
        {
          string Key = classCode + ": " + xmlText;
          UserPermission Res;
          if (!_Dict.TryGetValue(Key, out Res))
          {
            TempCfg Cfg = new TempCfg();
            Cfg.AsXmlText = xmlText;

            UserPermissionUI Cls = _Owner[classCode];
            if (Cls == null)
              Res = new UnknownUserPermission(classCode);
            else
              Res = _Owner.Creators.Create(classCode);
            Res.Read(Cfg);

            _Dict.Add(Key, Res);
          }
          return Res;
        }
      }

      #endregion
    }

    /// <summary>
    /// Кэшированная коллекция разрешений, в которой ключом являются код класса разрешения плюс
    /// XML-значение разрешения (как текст), а значением - объекты UserPermission.
    /// </summary>
    public UserPermissionInfoCache InfoCache { get { return _InfoCache; } }
    private UserPermissionInfoCache _InfoCache;

    #endregion

    #region Значок поддокументов

    /// <summary>
    /// Добавляет в DBUIImageHandlers обработчик, возвращающий значок для класса разрешений,
    /// который будет выводится в просмотре поддокументов с разрешениями
    /// </summary>
    /// <param name="subDocTypeUI">Интерфейс доступа к поддокументам</param>
    /// <param name="classCodeColumnName">Имя текстового столбца с классом разрешения</param>
    /// <param name="dataColumnName">Имя текстового столбца, в котором хранится значение разрешения в XML-формате</param>
    public void InitImages(DocTypeUIBase subDocTypeUI, string classCodeColumnName, string dataColumnName)
    {
      subDocTypeUI.UI.ImageHandlers.Add(subDocTypeUI.DocTypeBase.Name, "UserPermission",
        new DBxColumns(new string[] { classCodeColumnName, dataColumnName }),
        new DBxImageValueNeededEventHandler(ImageValueNeeded));
    }

    private void ImageValueNeeded(object sender, DBxImageValueNeededEventArgs args)
    {
      string ClassCode = args.GetString(0);
      if (String.IsNullOrEmpty(ClassCode))
      {
        args.ImageKey = "UnknownState";
        return;
      }

      string XmlText = args.GetString(1);
      UserPermission p = InfoCache[ClassCode, XmlText];

      args.ImageKey = this.GetInfo(p).ObjectImageKey;
      args.ToolTipText = p.ToString() + Environment.NewLine +
        "Класс разрешения: " + p.ClassCode;
    }

    #endregion

    #region Столбцы просмотра

    /// <summary>
    /// Добавляет в генератор табличного просмотра поддокументов разрешений вычисляемый столбец с 
    /// именем "ObjectText" и заголовком "Разрешение"
    /// </summary>
    /// <param name="producer">Генератор просмотра поддокументов, куда добавляется столбец</param>
    /// <param name="classCodeColumnName">Имя текстового столбца с классом разрешения</param>
    /// <param name="dataColumnName">Имя текстового столбца, в котором хранится значение разрешения в XML-формате</param>
    /// <returns>Вычисляемый столбец</returns>
    public EFPGridProducerColumn AddObjectTextColumn(EFPGridProducer producer, string classCodeColumnName, string dataColumnName)
    {
      return producer.Columns.AddUserText("ObjectText", classCodeColumnName + "," + dataColumnName,
        new EFPGridProducerValueNeededEventHandler(ObjectTextColumnValueNeeded), "Разрешение", 25, 10);
    }

    private void ObjectTextColumnValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      string ClassCode = args.GetString(0);
      if (String.IsNullOrEmpty(ClassCode))
      {
        args.Value = "Не задан класс разрешения";
        return;
      }

      string XmlText = args.GetString(1);
      UserPermission p = InfoCache[ClassCode, XmlText];

      args.Value = p.ObjectText;
    }

    /// <summary>
    /// Добавляет в генератор табличного просмотра поддокументов разрешений вычисляемый столбец с 
    /// именем "ValueText" и заголовком "Значение"
    /// </summary>
    /// <param name="producer">Генератор просмотра поддокументов, куда добавляется столбец</param>
    /// <param name="classCodeColumnName">Имя текстового столбца с классом разрешения</param>
    /// <param name="dataColumnName">Имя текстового столбца, в котором хранится значение разрешения в XML-формате</param>
    /// <returns>Вычисляемый столбец</returns>
    public EFPGridProducerColumn AddValueTextColumn(EFPGridProducer producer, string classCodeColumnName, string dataColumnName)
    {
      return producer.Columns.AddUserText("ValueText", classCodeColumnName + "," + dataColumnName,
        new EFPGridProducerValueNeededEventHandler(ValueTextColumnValueNeeded), "Значение", 15, 5);
    }

    private void ValueTextColumnValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      string ClassCode = args.GetString(0);
      if (String.IsNullOrEmpty(ClassCode))
      {
        args.Value = "Не задан класс разрешения";
        return;
      }

      string XmlText = args.GetString(1);
      UserPermission p = InfoCache[ClassCode, XmlText];

      args.Value = p.ValueText;
    }

    /// <summary>
    /// Добавляет в генератор табличного просмотра поддокументов разрешений вычисляемый столбец-значок с 
    /// именем "ValueImage", в котором выводится значок для состояния разрешения
    /// </summary>
    /// <param name="producer">Генератор просмотра поддокументов, куда добавляется столбец</param>
    /// <param name="classCodeColumnName">Имя текстового столбца с классом разрешения</param>
    /// <param name="dataColumnName">Имя текстового столбца, в котором хранится значение разрешения в XML-формате</param>
    /// <returns>Вычисляемый столбец</returns>
    public EFPGridProducerColumn AddValueImageColumn(EFPGridProducer producer, string classCodeColumnName, string dataColumnName)
    {
      EFPGridProducerColumn Col = producer.Columns.AddUserImage("ValueImage", classCodeColumnName + "," + dataColumnName,
        new EFPGridProducerValueNeededEventHandler(ValueImageColumnValueNeeded), "");
      Col.DisplayName = "Значок значения разрешения";
      return Col;
    }

    private void ValueImageColumnValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      string ClassCode = args.GetString(0);
      if (String.IsNullOrEmpty(ClassCode))
      {
        args.Value = EFPApp.MainImages.Images["EmptyImage"];
        return;
      }

      string XmlText = args.GetString(1);
      UserPermission p = InfoCache[ClassCode, XmlText];

      args.Value = EFPApp.MainImages.Images[this.GetInfo(p).ValueImageKey];
    }

    #endregion

    #endregion

    #region Диалог выбора класса

    /// <summary>
    /// Показывает диалог выбора класса разрешения из текущего списка.
    /// В списке показываются только классы разрешений, у которых свойство UserPermission.UseInRB=true
    /// </summary>
    /// <param name="classCode">Вход и выход: Выбранный класс разрешения</param>
    /// <param name="title">Заголовок блока диалога. Если не задан, то "Выбор класса разрешения"</param>
    /// <param name="imageKey">Имя изображения в EFPApp.MainImages для значка диалога.
    /// Если не задано, то используется UserPermission</param>
    /// <returns>True, если пользователь выбрал класс разрешения и нажал "OK"</returns>
    public bool SelectClass(ref string classCode, string title, string imageKey)
    {
      ListSelectDialog dlg = new ListSelectDialog();
      if (String.IsNullOrEmpty(title))
        dlg.Title = "Выбор класса разрешения";
      else
        dlg.Title = title;
      if (String.IsNullOrEmpty(imageKey))
        dlg.ImageKey = "UserPermission";
      else
        dlg.ImageKey = imageKey;
      dlg.ListTitle = "Класс разрешения";

      List<UserPermissionUI> UsedClasses = new List<UserPermissionUI>();
      foreach (UserPermissionUI Item in this)
      {
        if (Item.UseInRB)
          UsedClasses.Add(Item);
      }

      dlg.Items = new string[UsedClasses.Count];
      for (int i = 0; i < UsedClasses.Count; i++)
      {
        dlg.Items[i] = UsedClasses[i].DisplayName;
        dlg.ImageKeys[i] = UsedClasses[i].ImageKey;
        if (UsedClasses[i].ClassCode == classCode)
          dlg.SelectedIndex = i;
      }
      dlg.ConfigSectionName = "UserPermissionsUI.SelectClass";

      if (dlg.ShowDialog() != DialogResult.OK)
        return false;

      classCode = UsedClasses[dlg.SelectedIndex].ClassCode;
      return true;
    }

    #endregion

    #region Редактирование поддокумента

    private class DataEditItem : XmlCfgDocEditItem
    {
      #region Конструктор

      public DataEditItem(DBxDocValue docValue, UserPermissionEditor editor, IUserPermissionCreator creator)
        : base(docValue)
      {
        _Editor = editor;
        _Creator = creator;
      }

      #endregion

      #region Свойства

      private UserPermissionEditor _Editor;
      private IUserPermissionCreator _Creator;

      #endregion

      #region Переопределенные методы

      public override void ReadValues()
      {
        base.ReadValues();

        UserPermission Permission = _Creator.CreateUserPermission();
        try
        {
          Permission.Read(base.Data);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка загрузки данных для разрешения \"" + _Editor.UI.DisplayName +
            "\" . Будет использовано значение по умолчанию");
        }

        _Editor.PerformReadValues(Permission);
      }

      public override void WriteValues()
      {
        UserPermission Permission = _Creator.CreateUserPermission();
        _Editor.PerformWrtiteValues(Permission);
        Permission.Write(base.Data);

        base.WriteValues();
      }

      #endregion
    }

    /// <summary>
    /// Инициализирует страницу редактора поддокумента-разрешения.
    /// В редакторе можно менять только значение разрешения, но не его класс.
    /// </summary>
    /// <param name="args">Аргументы события инициализации редактора поддокумента</param>
    /// <param name="classCodeColumnName">Имя текстового столбца с классом разрешения</param>
    /// <param name="dataColumnName">Имя текстового столбца, в котором хранится значение разрешения в XML-формате</param>
    public void InitSubDocEditForm(InitSubDocEditFormEventArgs args, string classCodeColumnName, string dataColumnName)
    {
      string ClassCode = args.Editor.SubDocs.Values[classCodeColumnName].AsString;
      if (String.IsNullOrEmpty(ClassCode))
        throw new InvalidOperationException("Не установлено значение поля \"" + classCodeColumnName + "\"");
      UserPermissionUI UI = this[ClassCode];
      if (UI == null)
        throw new InvalidOperationException("Не найден объект UserPermissionClassUI для класса разрешения \"" + ClassCode + "\"");

      //Panel ParentPanel = new Panel();
      //ParentPanel.Dock = DockStyle.Fill;

      EFPBaseProvider BaseProvider = new EFPBaseProvider();
      BaseProvider.DisplayName = "Редактор разрешения";
      UserPermissionEditor Editor = new UserPermissionEditor(UI, args.Editor.IsReadOnly, BaseProvider);
      UI.CreateEditor(Editor);

      DocEditPage Page = args.AddPage(UI.DisplayName, Editor.Control);
      BaseProvider.Parent = Page.BaseProvider;
      Page.ImageKey = UI.ImageKey;

      IUserPermissionCreator Creator = Creators[ClassCode];
      if (Creator == null)
        throw new NullReferenceException("В списке Creators нет генератора разрешений \"" + ClassCode + "\"");

      DataEditItem EditItem = new DataEditItem(args.Editor.SubDocs.Values[dataColumnName], Editor, Creator);
      args.AddDocEditItem(EditItem);
    }

    #endregion

    #region Просмотр разрешений

    /// <summary>
    /// Открывает окно просмотра разрешения (без возможности редактирования)
    /// </summary>
    /// <param name="permission">Просматриваемое разрешение</param>
    public void View(UserPermission permission)
    {
      if (permission == null)
        throw new ArgumentNullException("permission");

      UserPermissionUI UI = this[permission.ClassCode];
      if (UI == null)
      {
        EFPApp.ErrorMessageBox("Нельзя просмотреть разрешение типа \"" + permission.ClassCode + "\", т.к. для него не зарегистрирован объект пользовательского интерфейса");
        return;
      }

      UI.View(permission);
    }

    #endregion

    #region Неизвестный тип разрешения

    /// <summary>
    /// Интерфейс "неизвестного" разрешения
    /// </summary>
    private class UnknownUserPermissionUI : UserPermissionUI
    {
      #region Конструктор

      public UnknownUserPermissionUI()
        : base("Unknown")
      {
        //base.ImageKey = "UnknownState";
        base.ImageKey = "Error";
        base.UseInRB = false;
        base.DisplayName = "Неизвестное разрешение";
      }

      #endregion

      /// <summary>
      /// Гененирует исключение
      /// </summary>
      /// <param name="editor"></param>
      public override void CreateEditor(UserPermissionEditor editor)
      {
        throw new NotSupportedException("Нельзя редактировать неизвестное разрешения");
      }

      /// <summary>
      /// Заполнение информации о разрешении для пользовательского интерфейса
      /// </summary>
      /// <param name="permission">Разрешение, откуда берется информация</param>
      /// <param name="uiInfo">Заполняемый объект</param>
      public override void GetInfo(UserPermission permission, UserPermissionUIInfo uiInfo)
      {
        base.GetInfo(permission, uiInfo);
        uiInfo.ValueImageKey = "Error";
      }
    }

    /// <summary>
    /// Статический экземпляр интерфейса для неизвестного разрешения
    /// </summary>
    public static readonly UserPermissionUI TheUnknownUserPermissionUI = new UnknownUserPermissionUI();

    #endregion
  }

  /// <summary>
  /// Аргументы событий UserPermissionEditor.ReadValues и WriteValues
  /// </summary>
  public class UserPermissionEditorRWEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Объект не должен создаваться в пользовательском коде
    /// </summary>
    /// <param name="permission">Разрешение, которое надо перенести в поля ввода или заполнить из редактора</param>
    public UserPermissionEditorRWEventArgs(UserPermission permission)
    {
#if DEBUG
      if (permission == null)
        throw new ArgumentNullException("permission");
#endif
      _Permission = permission;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Разрешение, которое надо перенести в поля ввода или заполнить из редактора
    /// </summary>
    public UserPermission Permission { get { return _Permission; } }
    private UserPermission _Permission;

    #endregion
  }

  /// <summary>
  /// Делегат событий UserPermissionEditor.ReadValues и WriteValues
  /// </summary>
  /// <param name="sender">Объект UserPermissionEditor</param>
  /// <param name="args">Аргументы события</param>
  public delegate void UserPermissionEditorRWEventHandler(object sender,
    UserPermissionEditorRWEventArgs args);

  /// <summary>
  /// Редактор одиночного разрешения
  /// </summary>
  public sealed class UserPermissionEditor
  {
    #region Конструктор

    /// <summary>
    /// Создает редактор разрешения.
    /// Не используется в пользовательском коде
    /// </summary>
    /// <param name="ui">Интерфейс для редактируемого разрешения</param>
    /// <param name="isReadOnly">True, если выполняется только просмотр разрешения,
    /// а не редактирование</param>
    /// <param name="baseProvider">Базовый провайдер</param>
    public UserPermissionEditor(UserPermissionUI ui, bool isReadOnly, EFPBaseProvider baseProvider)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      if (baseProvider == null)
        throw new ArgumentNullException("baseProvider");

      _UI = ui;
      _IsReadOnly = isReadOnly;
      _BaseProvider = baseProvider;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс разрешения, к которому относится редактор.
    /// Задается в конструкторе
    /// </summary>
    public UserPermissionUI UI { get { return _UI; } }
    private UserPermissionUI _UI;


    /// <summary>
    /// Возвращает true, если редактор открывается для просмотра разрешения, а не для редактирования
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Управляющий элемент для редактирования разрешения.
    /// Свойство должно быть инициализировано в методе UserPermissionUI.CreateEditor()
    /// </summary>
    public Control Control
    {
      get { return _Control; }
      set { _Control = value; }
    }
    private Control _Control;

    /// <summary>
    /// Сюда должны присоединяться провайдер(ы) управляющих элементов в методе UserPermissionUI.CreateEditor().
    /// Свойство инциализируется в конструкторе
    /// </summary>
    public EFPBaseProvider BaseProvider { get { return _BaseProvider; } }
    private EFPBaseProvider _BaseProvider;

    /// <summary>
    /// Произвольные пользовательские данные, которые модгут быть установлены методом,
    /// производным от UserPermissionUI.InitEditor. Затем эти данные могут быть использованы
    /// обработчиками ReadValues и WriteValues 
    /// </summary>
    public object UserData { get { return _UserData; } set { _UserData = value; } }
    private object _UserData;

    #endregion

    #region События

    /// <summary>
    /// Событие вызывается при открытии редактора
    /// Обработчик события должен извлечь данные из Args.Permission и поместить их в поля формы
    /// </summary>
    public event UserPermissionEditorRWEventHandler ReadValues;

    /// <summary>
    /// Событие вызывается при окончании редактирования фориы
    /// Обработчик события должен извлечь данные из полей формы и установить свойства реазрешения в Args.Permission
    /// </summary>
    public event UserPermissionEditorRWEventHandler WriteValues;

    /// <summary>
    /// Вызывает событие ReadValues для заданного разрешения
    /// </summary>
    /// <param name="permission">Разрешение</param>
    public void PerformReadValues(UserPermission permission)
    {
      if (ReadValues == null)
        return;
      UserPermissionEditorRWEventArgs Args = new UserPermissionEditorRWEventArgs(permission);
      ReadValues(this, Args);
    }

    /// <summary>
    /// Вызывает событие WriteValues для заданного разрешения
    /// </summary>
    /// <param name="permission">Разрешение</param>
    public void PerformWrtiteValues(UserPermission permission)
    {
      if (WriteValues == null)
        return;
      UserPermissionEditorRWEventArgs Args = new UserPermissionEditorRWEventArgs(permission);
      WriteValues(this, Args);
    }

    #endregion
  }
}
