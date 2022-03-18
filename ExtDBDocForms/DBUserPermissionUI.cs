// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Controls;

// ���������������� ��������� ��� ���������� ����� ����������
// ������, ����������� �� UserPermissionUI

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// ����������� ����� ���������� ����������, ����������� ������������ ���������.
  /// ����� ����� �������������� ��� ����������, ���������� ���������� ���������, ���� "���������-���������"
  /// </summary>
  public abstract class EnumUserPermissionUI : UserPermissionUI
  {
    #region �����������

    /// <summary>
    /// ������� ��������� ��� ������ ����������
    /// </summary>
    /// <param name="classCode">��� ������ ����������</param>
    /// <param name="textValues">��������� �������� ��������� ������������</param>
    /// <param name="imageKeys">������ ��� ��������� ������������</param>
    protected EnumUserPermissionUI(string classCode, string[] textValues, string[] imageKeys)
      : base(classCode)
    {
      if (textValues.Length != imageKeys.Length)
        throw new ArgumentException();
      _TextValues = textValues;
      _ImageKeys = imageKeys;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��������� ��������, ��������������� ��������� ������������
    /// </summary>
    public string[] TextValues { get { return _TextValues; } }
    private string[] _TextValues;

    /// <summary>
    /// ������, ��������������� ��������� ������������
    /// </summary>
    public string[] ImageKeys { get { return _ImageKeys; } }
    private string[] _ImageKeys;

    #endregion

    #region ����������� ������

    /// <summary>
    /// ���������� ������������ �������� �� ����������, ����������� � ������ �����
    /// </summary>
    /// <param name="permission">����������</param>
    /// <returns>������������ ��������</returns>
    protected abstract int GetIndexValue(UserPermission permission);

    /// <summary>
    /// ������������� ������������ �������� ��� ����������
    /// </summary>
    /// <param name="permission">����������</param>
    /// <param name="value">������������ �������� ��� ����� �����</param>
    protected abstract void SetIndexValue(UserPermission permission, int value);

    #endregion

    #region ��������� ����������

    /// <summary>
    /// ���������� ���������� � ���������� ��� ����������������� ����������
    /// </summary>
    /// <param name="permission">����������, ������ ������� ����������</param>
    /// <param name="info">����������� ������</param>
    public override void GetInfo(UserPermission permission, UserPermissionUIInfo info)
    {
      base.GetInfo(permission, info);
      int indexValue = GetIndexValue(permission);
      info.ValueImageKey = ImageKeys[indexValue];
    }

    #endregion

    #region ��������������

    /// <summary>
    /// ��������� ������ ����������� � ���������
    /// </summary>
    protected virtual string GroupTitle { get { return "����� �������"; } }

    /// <summary>
    /// ������� ������ ��� �������������� ����������
    /// </summary>
    /// <param name="editor">�������� ����������, ������� ��������� ����������������</param>
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
  /// ��������� ���������� �� ���� ������ � �����
  /// </summary>
  public class WholeDBPermissionUI : EnumUserPermissionUI
  {
    #region �����������

    /// <summary>
    /// ������� ��������� ���������� ����������
    /// </summary>
    public WholeDBPermissionUI()
      : base("DB", DBUserPermission.ValueNames, ValueImageKeys)
    {
      base.DisplayName = "���� ������";
      base.ImageKey = "Database";
    }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ���������� ������������ �������� �� ����������, ����������� � ������ �����
    /// </summary>
    /// <param name="permission">����������</param>
    /// <returns>������������ ��������</returns>
    protected override int GetIndexValue(UserPermission permission)
    {
      return (int)(((WholeDBPermission)permission).Mode);
    }

    /// <summary>
    /// ������������� ������������ �������� ��� ����������
    /// </summary>
    /// <param name="permission">����������</param>
    /// <param name="value">������������ �������� ��� ����� �����</param>
    protected override void SetIndexValue(UserPermission permission, int value)
    {
      ((WholeDBPermission)permission).Mode = (DBxAccessMode)value;
    }

    #endregion

    #region ����������� ������

    /// <summary>
    /// ����� ����������� � EFPApp.MainImages, ��������������� ������������ DBxAccessMode
    /// </summary>
    public static readonly string[] ValueImageKeys = new string[] { "Edit", "View", "No" };

    /// <summary>
    /// ���������� ��� ����������� � EFPApp.MainImages, ��������������� ������������ DBxAccessMode
    /// </summary>
    /// <param name="mode">����� �������</param>
    /// <returns>��� �����������</returns>
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
  /// ��������� ���������� �� ������ � ����� ��� ���������� �������� ���� ������.
  /// ���� ��� ���������� ����� ������������
  /// </summary>
  public class TableUserPermissionUI : EnumUserPermissionUI // TODO: ���� ��� ����� ��������
  {
    #region �����������

    /// <summary>
    /// ������� ��������� ����������
    /// </summary>
    public TableUserPermissionUI()
      : base("Table", DBUserPermission.ValueNames, WholeDBPermissionUI.ValueImageKeys)
    {
    }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ���������� ������������ �������� �� ����������, ����������� � ������ �����
    /// </summary>
    /// <param name="permission">����������</param>
    /// <returns>������������ ��������</returns>
    protected override int GetIndexValue(UserPermission permission)
    {
      return (int)(((TablePermission)permission).Mode);
    }

    /// <summary>
    /// ������������� ������������ �������� ��� ����������
    /// </summary>
    /// <param name="permission">����������</param>
    /// <param name="value">������������ �������� ��� ����� �����</param>
    protected override void SetIndexValue(UserPermission permission, int value)
    {
      ((TablePermission)permission).Mode = (DBxAccessMode)value;
    }

    /// <summary>
    /// �� �����������
    /// </summary>
    /// <param name="editor"></param>
    public override void CreateEditor(UserPermissionEditor editor)
    {
      throw new NotImplementedException();
    }

    #endregion
  }


  /// <summary>
  /// ��������� ���������� �� �������� ����������� �����
  /// </summary>
  public class RecalcColumnsPermissionUI : EnumUserPermissionUI
  {
    #region �����������

    /// <summary>
    /// ������� ��������� ����������
    /// </summary>
    public RecalcColumnsPermissionUI()
      : base("RecalcColumns", RecalcColumnsPermission.ValueNames, ValueImageKeys)
    {
      base.DisplayName = "�������� ����������� �����";
      base.ImageKey = "RecalcColumns";
    }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ���������� ������������ �������� �� ����������, ����������� � ������ �����
    /// </summary>
    /// <param name="permission">����������</param>
    /// <returns>������������ ��������</returns>
    protected override int GetIndexValue(UserPermission permission)
    {
      return (int)(((RecalcColumnsPermission)permission).Mode);
    }

    /// <summary>
    /// ������������� ������������ �������� ��� ����������
    /// </summary>
    /// <param name="permission">����������</param>
    /// <param name="value">������������ �������� ��� ����� �����</param>
    protected override void SetIndexValue(UserPermission permission, int value)
    {
      ((RecalcColumnsPermission)permission).Mode = (RecalcColumnsPermissionMode)value;
    }

    #endregion

    #region ����������� ������

    /// <summary>
    /// ����� ����������� � EFPApp.MainImages, ��������������� ������������ RecalcColumnsPermissionMode
    /// </summary>
    public static readonly string[] ValueImageKeys = new string[] { "No", "CircleYellow", "Ok" };

    /// <summary>
    /// ���������� ��� ����������� � EFPApp.MainImages, ��������������� ������������ RecalcColumnsPermissionMode
    /// </summary>
    /// <param name="mode">����� ��������� �����</param>
    /// <returns>��� �����������</returns>
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
