﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Controls;

// Пользовательский интерфейс для конкретных видов разрешений.
// Классы, производные от UserPermissionUI

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Абстрактный класс интерфейса разрешения, задаваемого перечислимым значением.
  /// Может также использоваться для разрешений, задаваемых логическим значением, типа "Разрешено-Запрещено"
  /// </summary>
  public abstract class EnumUserPermissionUI : UserPermissionUI
  {
    #region Конструктор

    /// <summary>
    /// Создает интферейс для класса разрешений
    /// </summary>
    /// <param name="classCode">Код класса разрешений</param>
    /// <param name="textValues">Текстовые значения элементво перечисления</param>
    /// <param name="imageKeys">Значки для элементов перечисления</param>
    protected EnumUserPermissionUI(string classCode, string[] textValues, string[] imageKeys)
      : base(classCode)
    {
      if (textValues.Length != imageKeys.Length)
        throw new ArgumentException();
      _TextValues = textValues;
      _ImageKeys = imageKeys;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текстовые значения, соответствующие значениям перечисления
    /// </summary>
    public string[] TextValues { get { return _TextValues; } }
    private readonly string[] _TextValues;

    /// <summary>
    /// Значки, соответствующие значениям перечисления
    /// </summary>
    public string[] ImageKeys { get { return _ImageKeys; } }
    private readonly string[] _ImageKeys;

    #endregion

    #region Абстрактные методы

    /// <summary>
    /// Возвращает перечислимое значение из разрешения, приведенное к целому числу
    /// </summary>
    /// <param name="permission">Разрешение</param>
    /// <returns>Перечислимое значение</returns>
    protected abstract int GetIndexValue(UserPermission permission);

    /// <summary>
    /// Устанавливает перечислимое значение для разрешения
    /// </summary>
    /// <param name="permission">Разрешение</param>
    /// <param name="value">Перечислимое значение как целое число</param>
    protected abstract void SetIndexValue(UserPermission permission, int value);

    #endregion

    #region Получение информации

    /// <summary>
    /// Заполнение информации о разрешении для пользовательского интерфейса
    /// </summary>
    /// <param name="permission">Разрешение, откуда берется информация</param>
    /// <param name="info">Заполняемый объект</param>
    public override void GetInfo(UserPermission permission, UserPermissionUIInfo info)
    {
      base.GetInfo(permission, info);
      int indexValue = GetIndexValue(permission);
      info.ValueImageKey = ImageKeys[indexValue];
    }

    #endregion

    #region Редактирование

    /// <summary>
    /// Заголовок группы радиокнопок в редакторе
    /// </summary>
    protected virtual string GroupTitle { get { return Res.EnumUserPermissionUI_Title_GroupTitle; } }

    /// <summary>
    /// Создает панель для редактирования разрешения
    /// </summary>
    /// <param name="editor">Редактор разрешения, который требуется инициализировать</param>
    public override void CreateEditor(UserPermissionEditor editor)
    {
      RadioGroupBox control = new RadioGroupBox(TextValues);
      control.Text = GroupTitle;
      control.ImageKeys = ImageKeys;
      editor.Control = control;
      EFPRadioButtons efpRB = new EFPRadioButtons(editor.BaseProvider, control);
      efpRB.Enabled = !editor.IsReadOnly;
      editor.UserData = efpRB;
      editor.ReadValues += new UserPermissionEditorRWEventHandler(Editor_ReadValues);
      editor.WriteValues += new UserPermissionEditorRWEventHandler(Editor_WriteValues);
    }

    void Editor_ReadValues(object sender, UserPermissionEditorRWEventArgs args)
    {
      int intValue = GetIndexValue(args.Permission);
      UserPermissionEditor editor = (UserPermissionEditor)sender;
      EFPRadioButtons efpRB = (EFPRadioButtons)(editor.UserData);
      efpRB.SelectedIndex = intValue;
    }

    void Editor_WriteValues(object sender, UserPermissionEditorRWEventArgs args)
    {
      UserPermissionEditor editor = (UserPermissionEditor)sender;
      EFPRadioButtons efpRB = (EFPRadioButtons)(editor.UserData);
      int intValue = efpRB.SelectedIndex;
      SetIndexValue(args.Permission, intValue);
    }

    #endregion
  }

  /// <summary>
  /// Интерфейс разрешения на базу данных в целом
  /// </summary>
  public class WholeDBPermissionUI : EnumUserPermissionUI
  {
    #region Конструктор

    /// <summary>
    /// Создает экземпляр интерфейса разрешения
    /// </summary>
    public WholeDBPermissionUI()
      : base("DB", DBUserPermission.ValueNames, ValueImageKeys)
    {
      base.DisplayName = Res.WholeDBPermissionUI_Name_Default;
      base.ImageKey = "Database";
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
      return (int)(((WholeDBPermission)permission).Mode);
    }

    /// <summary>
    /// Устанавливает перечислимое значение для разрешения
    /// </summary>
    /// <param name="permission">Разрешение</param>
    /// <param name="value">Перечислимое значение как целое число</param>
    protected override void SetIndexValue(UserPermission permission, int value)
    {
      ((WholeDBPermission)permission).Mode = (DBxAccessMode)value;
    }

    #endregion

    #region Статические списки

    /// <summary>
    /// Имена изображений в <see cref="EFPApp.MainImages"/>, соответствующие перечислению <see cref="DBxAccessMode"/>
    /// </summary>
    public static readonly string[] ValueImageKeys = new string[] { "Edit", "View", "No" };

    /// <summary>
    /// Возвращает имя изображения в <see cref="EFPApp.MainImages"/>, соответствующие перечислению <see cref="DBxAccessMode"/>
    /// </summary>
    /// <param name="mode">Режим доступа</param>
    /// <returns>Имя изображения</returns>
    public static string GetModeImageKey(DBxAccessMode mode)
    {
      if ((int)mode < 0 || (int)mode >= ValueImageKeys.Length)
        return "UnknownImage";
      else
        return ValueImageKeys[(int)mode];
    }

    #endregion
  }

  /// <summary>
  /// Интерфейс разрешения на доступ к одной или нескольким таблицам базы данных.
  /// Этот тип разрешения редко используется
  /// </summary>
  public class TableUserPermissionUI : EnumUserPermissionUI // TODO: Этот тип плохо подходит
  {
    #region Конструктор

    /// <summary>
    /// Создает интерфейс разрешения
    /// </summary>
    public TableUserPermissionUI()
      : base("Table", DBUserPermission.ValueNames, WholeDBPermissionUI.ValueImageKeys)
    {
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
      return (int)(((TablePermission)permission).Mode);
    }

    /// <summary>
    /// Устанавливает перечислимое значение для разрешения
    /// </summary>
    /// <param name="permission">Разрешение</param>
    /// <param name="value">Перечислимое значение как целое число</param>
    protected override void SetIndexValue(UserPermission permission, int value)
    {
      ((TablePermission)permission).Mode = (DBxAccessMode)value;
    }

    /// <summary>
    /// Не реализовано
    /// </summary>
    /// <param name="editor"></param>
    public override void CreateEditor(UserPermissionEditor editor)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  /// <summary>
  /// Интерфейс разрешения на пересчет вычисляемых полей
  /// </summary>
  public class RecalcColumnsPermissionUI : EnumUserPermissionUI
  {
    #region Конструктор

    /// <summary>
    /// Создает интерфейс разрешения
    /// </summary>
    public RecalcColumnsPermissionUI()
      : base("RecalcColumns", RecalcColumnsPermission.ValueNames, ValueImageKeys)
    {
      base.DisplayName = Res.RecalcColumnsPermissionUI_Name_Default;
      base.ImageKey = "RecalcColumns";
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
      return (int)(((RecalcColumnsPermission)permission).Mode);
    }

    /// <summary>
    /// Устанавливает перечислимое значение для разрешения
    /// </summary>
    /// <param name="permission">Разрешение</param>
    /// <param name="value">Перечислимое значение как целое число</param>
    protected override void SetIndexValue(UserPermission permission, int value)
    {
      ((RecalcColumnsPermission)permission).Mode = (RecalcColumnsPermissionMode)value;
    }

    #endregion

    #region Статические списки

    /// <summary>
    /// Имена изображений в <see cref="EFPApp.MainImages"/>, соответствующие перечислению <see cref="RecalcColumnsPermissionMode"/>
    /// </summary>
    public static readonly string[] ValueImageKeys = new string[] { "No", "CircleYellow", "Ok" };

    /// <summary>
    /// Возвращает имя изображения в <see cref="EFPApp.MainImages"/>, соответствующие перечислению <see cref="RecalcColumnsPermissionMode"/>
    /// </summary>
    /// <param name="mode">Режим пересчета полей</param>
    /// <returns>Имя изображения</returns>
    public static string GetModeImageKey(RecalcColumnsPermissionMode mode)
    {
      if ((int)mode < 0 || (int)mode >= ValueImageKeys.Length)
        return "UnknownImage";
      else
        return ValueImageKeys[(int)mode];
    }

    #endregion
  }
}
