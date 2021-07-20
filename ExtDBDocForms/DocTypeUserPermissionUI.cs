using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using AgeyevAV.ExtDB;
using AgeyevAV.ExtDB.Docs;

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

// Интерфейс разрешений, связанных с DocType

namespace AgeyevAV.ExtForms.Docs
{
  /// <summary>
  /// Шаблон формы редактирования разрешения
  /// </summary>
  internal partial class EditDocTypePermissionForm : Form
  {
    #region Конструктор формы

    public EditDocTypePermissionForm(EFPBaseProvider baseProvider, DBUI ui, string[] docTypeNames, string[] excludeDocTypeNames, string[] textValues, string[] imageKeys, bool isReadOnly)
    {
      InitializeComponent();

      efpSelMode = new EFPRadioButtons(baseProvider, rbAllTypes);
      efpSelMode.DisplayName = "Режим выбора все документы / выбранный тип";
      if (isReadOnly)
        efpSelMode.Enabled = false;

      if (docTypeNames == null)
      {
        // 28.04.2021
        // Убираем виды документов - деревья групп

        docTypeNames = ui.DocProvider.DocTypes.GetDocTypeNames();
        StringArrayIndexer groupDocTypeIndexer = new StringArrayIndexer(ui.DocProvider.DocTypes.GetGroupDocTypeNames());
        if (groupDocTypeIndexer.Count > 0)
        {
          List<string> lst = new List<string>(docTypeNames.Length - groupDocTypeIndexer.Count);
          for (int i = 0; i < docTypeNames.Length; i++)
          {
            if (!groupDocTypeIndexer.Contains(docTypeNames[i]))
              lst.Add(docTypeNames[i]);
          }
          docTypeNames = lst.ToArray();
        }
      }

      if (excludeDocTypeNames != null)
      {
        if (excludeDocTypeNames.Length > 0)
        {
          List<string> lst = new List<string>(docTypeNames.Length - excludeDocTypeNames.Length);
          StringArrayIndexer excludeDocTypeIndexer = new StringArrayIndexer(excludeDocTypeNames);
          for (int i = 0; i < docTypeNames.Length; i++)
          {
            if (!excludeDocTypeIndexer.Contains(docTypeNames[i]))
              lst.Add(docTypeNames[i]);
          }
          docTypeNames = lst.ToArray();
        }
      }

      efpDocType = new EFPDocTypeComboBox(baseProvider, cbDocType, ui, docTypeNames);
      efpDocType.CanBeEmpty = false;
      if (isReadOnly)
        efpDocType.Enabled = false;
      else
        efpDocType.EnabledEx = efpSelMode[1].CheckedEx;

      grpMode.Items = textValues;
      grpMode.ImageKeys = imageKeys;
      efpMode = new EFPRadioButtons(baseProvider, grpMode);
      if (isReadOnly)
        efpMode.Enabled = false;

    }

    #endregion

    #region Поля

    public EFPRadioButtons efpSelMode;

    public EFPDocTypeComboBox efpDocType;

    public EFPRadioButtons efpMode;

    #endregion
  }

  /// <summary>
  /// Базовый класс для DocTypePermissionUI и DocTypeViewHistoryPermissionUI
  /// Поддерживается установка разрешения только для одного вида документов
  /// </summary>
  public abstract class DocTypePermissionBaseUI : EnumUserPermissionUI
  {
    #region Конструктор

    /// <summary>
    /// Создает интерфейс для класса разрешений
    /// </summary>
    /// <param name="classCode">Класс разрешений</param>
    /// <param name="textValues">Список перечислимых вариантов разрешения</param>
    /// <param name="imageKeys">Значки для значений <paramref name="textValues"/></param>
    /// <param name="ui">Интерфейс доступа к документам</param>
    protected DocTypePermissionBaseUI(string classCode, string[] textValues, string[] imageKeys, DBUI ui)
      : base(classCode, textValues, imageKeys)
    {
      _UI = ui;
    }
    /*
protected DocTypeEditor(InitDocEditFormEventArgsBase Args, string Title, DBUI UI, string[] DocTypeNames, string[] ButtonItems)
{
Form = new EditDocTypePermissionForm();
DocEditPage Page = Args.AddPage(Title, Form.MainPanel);

efpDocType = new EFPDocTypeComboBox(Page.BaseProvider, Form.cbDocType, UI, DocTypeNames);
efpDocType.CanBeEmpty = false;
if (Args.IsReadOnly)
efpDocType.Enabled = false;

Form.grpMode.Items = ButtonItems;
efpMode = new EFPRadioButtons(Page.BaseProvider, Form.grpMode.Buttons);
if (Args.IsReadOnly)
efpMode.Enabled = false;
}
      */
    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс доступа к документам
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    /// <summary>
    /// Список видов документов, из которых можно выбирать.
    /// Значение null (по умолчанию) задает возможность выбора из всех видов документов, заданных в Ui.DocProvider.DocTypes
    /// </summary>
    public string[] DocTypeNames { get { return _DocTypeNames; } set { _DocTypeNames = value; } }
    private string[] _DocTypeNames;

    /// <summary>
    /// Список видов документов, которые должны быть исключены из выбора.
    /// Используйте это свойство вместо DocTypeNames, если надо давать выбор из большинства видов документов,
    /// за исключением немногих
    /// Значение null (по умолчанию) - нет исключаемых видов
    /// </summary>
    public string[] ExcludedDocTypeNames { get { return _ExcludedDocTypeNames; } set { _ExcludedDocTypeNames = value; } }
    private string[] _ExcludedDocTypeNames;

    /// <summary>
    /// Если свойство установлено в true, то пользователь может выбрать вариант "Все типы документов".
    /// По умолчанию - false
    /// </summary>
    protected bool CanBeEmpty { get { return _CanBeEmpty; } set { _CanBeEmpty = value; } }
    private bool _CanBeEmpty;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Заполнение информации о разрешении для пользовательского интерфейса
    /// </summary>
    /// <param name="permission">Разрешение, откуда берется информация</param>
    /// <param name="info">Заполняемый объект</param>
    public override void GetInfo(UserPermission permission, UserPermissionUIInfo info)
    {
      base.GetInfo(permission, info);

      if (UseDocTypeObjectImage)
      {
        string[] DocTypeNames = GetDocTypeNames(permission);

        if (DocTypeNames == null)
          info.ObjectImageKey = "Table";
        else if (DocTypeNames.Length == 1)
        {
          if (UI.DocProvider.DocTypes.Contains(DocTypeNames[0])) // 16.06.2017
          {
            DocTypeUI dtui = UI.DocTypes[DocTypeNames[0]];
            info.ObjectImageKey = dtui.ImageKey;
          }
          else
            info.ObjectImageKey = "UnknownState";
        }
        else
          info.ObjectImageKey = "Table";
      }
    }

    /// <summary>
    /// Если true, то для значка ObjectImageKey используется значок вида документа.
    /// Если false, используется свойство ImageKey
    /// </summary>
    protected abstract bool UseDocTypeObjectImage { get; }

    #endregion

    #region Редактирование

    /// <summary>
    /// Создает панель для редактирования разрешения
    /// </summary>
    /// <param name="editor">Редактор разрешения, который требуется инициализировать</param>
    public override void CreateEditor(UserPermissionEditor editor)
    {
      // Не вызываем base.CreateEditor(Editor);

      EditDocTypePermissionForm Form = new EditDocTypePermissionForm(editor.BaseProvider, UI, DocTypeNames, ExcludedDocTypeNames, base.TextValues, base.ImageKeys, editor.IsReadOnly);
      Form.efpSelMode[0].Enabled = CanBeEmpty;
      if (CanBeEmpty)
      { 
        Form.efpDocType.EmptyText = "[ Все типы документов ]";
        Form.efpDocType.EmptyImageKey = "Table";
      }
      editor.Control = Form.MainPanel;
      editor.UserData = Form;
      editor.ReadValues += new UserPermissionEditorRWEventHandler(Editor_ReadValues);
      editor.WriteValues += new UserPermissionEditorRWEventHandler(Editor_WriteValues);
    }

    void Editor_ReadValues(object sender, UserPermissionEditorRWEventArgs args)
    {
      UserPermissionEditor Editor = (UserPermissionEditor)sender;
      EditDocTypePermissionForm Form = (EditDocTypePermissionForm)(Editor.UserData);

      string[] DocTypeNames = GetDocTypeNames(args.Permission);
      if (DocTypeNames == null)
        DocTypeNames = DataTools.EmptyStrings;
      if (DocTypeNames.Length == 0)
      {
        if (CanBeEmpty)
          Form.efpSelMode.SelectedIndex = 0;
        else
          Form.efpSelMode.SelectedIndex = 1;
        Form.efpDocType.DocTypeName = String.Empty;
      }
      else
      {
        Form.efpDocType.DocTypeName = DocTypeNames[0];
        Form.efpSelMode.SelectedIndex = 1;
        if (DocTypeNames.Length > 1)
          EFPApp.ErrorMessageBox("В редактируемом разрешениии указано несколько видов документов. Редактирование таких разрешений не поддерживается");
      }

      int IntValue = GetIndexValue(args.Permission);
      Form.efpMode.SelectedIndex = IntValue;
    }

    void Editor_WriteValues(object sender, UserPermissionEditorRWEventArgs args)
    {
      UserPermissionEditor Editor = (UserPermissionEditor)sender;
      EditDocTypePermissionForm Form = (EditDocTypePermissionForm)(Editor.UserData);

      if (Form.efpSelMode.SelectedIndex == 0)
        SetDocTypeNames(args.Permission, null);
      else if (Form.efpDocType.DocType == null)
        throw new BugException("Тип документов не выбран");
      else
        SetDocTypeNames(args.Permission, new string[] { Form.efpDocType.DocTypeName });

      int IntValue = Form.efpMode.SelectedIndex;
      SetIndexValue(args.Permission, IntValue);
    }

    /// <summary>
    /// Возвращает имена таблиц видов документов для заданного разрешения
    /// </summary>
    /// <param name="permission">Разрешение для извлечения списка</param>
    /// <returns>Имена видов документов</returns>
    public abstract string[] GetDocTypeNames(UserPermission permission);

    /// <summary>
    /// Устанавливает для разрешения список имен таблиц видов документов
    /// </summary>
    /// <param name="permission">Разрешение, в который помещается список</param>
    /// <param name="docTypeNames">Имена видов документов</param>
    public abstract void SetDocTypeNames(UserPermission permission, string[] docTypeNames);

    #endregion
  }

  /// <summary>
      /// Редактор разрешения DocTypePermission
      /// Поддерживается установка разрешения только для одного вида документов
      /// </summary>
  public class DocTypePermissionUI : DocTypePermissionBaseUI
  {
    #region Конструктор

    /// <summary>
    /// Создает интерфейс для класса разрешения
    /// </summary>
    /// <param name="ui">Интерфейс доступа к документам</param>
    public DocTypePermissionUI(DBUI ui)
      : base("DocType", DBUserPermission.ValueNames, WholeDBPermissionUI.ValueImageKeys, ui)
    {
      base.DisplayName = "Вид документов";
      base.ImageKey = "Table";
      base.CanBeEmpty = false;
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Возвращает перечислимое значение из разрешения, приведенное к целому числу
    /// </summary>
    /// <param name="permission">Разрешение</param>
    /// <returns>Перечислимое значение</returns>
    protected override int GetIndexValue(UserPermission permission)
    {
      return (int)(((DocTypePermission)permission).Mode);
    }

    /// <summary>
    /// Устанавливает перечислимое значение для разрешения
    /// </summary>
    /// <param name="permission">Разрешение</param>
    /// <param name="value">Перечислимое значение как целое число</param>
    protected override void SetIndexValue(UserPermission permission, int value)
    {
      ((DocTypePermission)permission).Mode = (DBxAccessMode)value;
    }

    /// <summary>
    /// Возвращает имена таблиц видов документов для заданного разрешения
    /// </summary>
    /// <param name="permission">Разрешение для извлечения списка</param>
    /// <returns>Имена видов документов</returns>
    public override string[] GetDocTypeNames(UserPermission permission)
    {
      return ((DocTypePermission)permission).DocTypeNames;
    }

    /// <summary>
    /// Устанавливает для разрешения список имен таблиц видов документов
    /// </summary>
    /// <param name="permission">Разрешение, в который помещается список</param>
    /// <param name="docTypeNames">Имена видов документов</param>
    public override void SetDocTypeNames(UserPermission permission, string[] docTypeNames)
    {
      ((DocTypePermission)permission).DocTypeNames = docTypeNames;
    }

    /// <summary>
    /// Возвращает true
    /// </summary>
    protected override bool UseDocTypeObjectImage { get { return true; } }

    #endregion
  }

  /// <summary>
  /// Редактор разрешения DocTypeViewHistoryPermission
  /// </summary>
  public class DocTypeViewHistoryPermissionUI : DocTypePermissionBaseUI
  {
    #region Конструктор

      /// <summary>
      /// Создает интерфейс разрешения
      /// </summary>
      /// <param name="ui">Интерфейс для документов</param>
    public DocTypeViewHistoryPermissionUI(DBUI ui)
      : base("History", new string[] { "Разрешено", "Запрещено" }, new string[] { "Ok", "No" }, ui)
    {
      base.DisplayName = "Просмотр истории";
      base.ImageKey = "Information";
      base.CanBeEmpty = true;
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Возвращает перечислимое значение из разрешения, приведенное к целому числу
    /// </summary>
    /// <param name="permission">Разрешение</param>
    /// <returns>Перечислимое значение</returns>
    protected override int GetIndexValue(UserPermission permission)
    {
      return ((DocTypeViewHistoryPermission)permission).Allowed ? 0 : 1;
    }

    /// <summary>
    /// Устанавливает перечислимое значение для разрешения
    /// </summary>
    /// <param name="permission">Разрешение</param>
    /// <param name="value">Перечислимое значение как целое число</param>
    protected override void SetIndexValue(UserPermission permission, int value)
    {
      ((DocTypeViewHistoryPermission)permission).Allowed = (value == 0);
    }

    /// <summary>
    /// Возвращает имена таблиц видов документов для заданного разрешения
    /// </summary>
    /// <param name="permission">Разрешение для извлечения списка</param>
    /// <returns>Имена видов документов</returns>
    public override string[] GetDocTypeNames(UserPermission permission)
    {
      return ((DocTypeViewHistoryPermission)permission).DocTypeNames;
    }

    /// <summary>
    /// Устанавливает для разрешения список имен таблиц видов документов
    /// </summary>
    /// <param name="permission">Разрешение, в который помещается список</param>
    /// <param name="docTypeNames">Имена видов документов</param>
    public override void SetDocTypeNames(UserPermission permission, string[] docTypeNames)
    {
      ((DocTypeViewHistoryPermission)permission).DocTypeNames = docTypeNames;
    }

    /// <summary>
    /// Возвращает false
    /// </summary>
    protected override bool UseDocTypeObjectImage { get { return false; } }

    #endregion

  }

  /// <summary>
  /// Интерфейс разрешения на просмотр действий других пользователей
  /// </summary>
  public class ViewOtherUsersActionPermissionUI : EnumUserPermissionUI
  {
    #region Конструктор

    /// <summary>
    /// Создает интерфейс разрешения
    /// </summary>
    public ViewOtherUsersActionPermissionUI()
      : base("ViewOtherUsersAction", new string[] { "Разрешен", "Запрещен" }, new string[] { "Ok", "No" })
    {
      base.DisplayName = "Просмотр действий других пользователей";
      base.ImageKey = "UserActions";
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Возвращает перечислимое значение из разрешения, приведенное к целому числу
    /// </summary>
    /// <param name="permission">Разрешение</param>
    /// <returns>Перечислимое значение</returns>
    protected override int GetIndexValue(UserPermission permission)
    {
      return (int)(((ViewOtherUsersActionPermission)permission).Allowed ? 0 : 1);
    }

    /// <summary>
    /// Устанавливает перечислимое значение для разрешения
    /// </summary>
    /// <param name="permission">Разрешение</param>
    /// <param name="value">Перечислимое значение как целое число</param>
    protected override void SetIndexValue(UserPermission permission, int value)
    {
      ((ViewOtherUsersActionPermission)permission).Allowed = (value == 0);
    }

    #endregion
  }
}