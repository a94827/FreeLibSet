// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.IO;
using FreeLibSet.Shell;
using System.Drawing;
using System.ComponentModel;
using FreeLibSet.Logging;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// ���������� ������ "�������" � "������� � �������" ��� ���������� ���� ������������ ��������,
  /// ������� ����� ��������� ����� ������������� ����.
  /// �� ������ �������� ������������ �������� ���� �� ������ (�� �����) ������������.
  /// ������ �����, ���� ����� ����������� �� ����������, ���� ������������ �������� ���� �� ������.
  /// � ����� ������, ���� �� ����� ����� ����� ������ ����������, ��� ���������� � ������������
  /// ����������. ��������, ����� � ������������ ������ ���������� ".txt", � ����� �������� ���� "myfile.log"
  /// </summary>
  public sealed class EFPFileAssociationsCommandItemsHandler
  {
    #region �����������

    /// <summary>
    /// ������� �������, ����������� � ����� � �������� �����������.
    /// ���� ��� ������������ ������� �� ����������� �������� ����������, ������� �� �����������
    /// </summary>
    /// <param name="commandItems">������ ��� ���������� ������</param>
    /// <param name="fileExt">���������� �����, ������� �����.
    /// ��������, ".txt", ".xml". ������ ���� ������ �����������.</param>
    public EFPFileAssociationsCommandItemsHandler(EFPCommandItems commandItems, string fileExt)
    {
      if (commandItems == null)
        throw new ArgumentNullException("commandItems");
      commandItems.CheckNotReadOnly();
      if (String.IsNullOrEmpty(fileExt))
        throw new ArgumentNullException("fileExt");
      if (fileExt[0] != '.')
        throw new ArgumentException("���������� ������ ���������� � �����", "fileExt");
      _FileExt = fileExt;

      #region ���������� ������

      _AllCommands = new List<EFPCommandItem>();

      try
      {
        FileAssociations FAItems;
        if (EFPApp.FileExtAssociations.IsSupported)
          FAItems = EFPApp.FileExtAssociations[fileExt];
        else
          FAItems = FileAssociations.Empty;

        if (FAItems.OpenItem != null)
        {
          EFPCommandItem ci = CreateCommandItem(FAItems.OpenItem);
          ci.MenuText = "�������";
          ci.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ToolBar;
          if (ci.HasImage)
            ci.ImageKey = "UnknownState";
          commandItems.Add(ci);
          //CmdItems.DefaultCommandItem = ci;
          _AllCommands.Add(ci);
        }
        else
        {
          EFPCommandItem ci = new EFPCommandItem("File", "OpenNowhere");
          ci.MenuText = "�������";
          if (FAItems.Exception == null)
          {
            ci.ToolTipText = "��� ����������, ������� ����� ��������� ����� � ����������� \"" + fileExt + "\"";
            ci.ImageKey = "UnknownState";
          }
          else
          {
            ci.ToolTipText = "������� ������ ��� ��������� �������� ����������. " + FAItems.Exception.Message;
            ci.ImageKey = "Error";
          }
          ci.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ToolBar;
          ci.Enabled = false;
          commandItems.Add(ci);
          _AllCommands.Add(ci);
        }

        EFPCommandItem smOpenWith = new EFPCommandItem("File", "OpenWith");
        smOpenWith.MenuText = "������� � �������";
        smOpenWith.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ToolBarDropDown;
        commandItems.Add(smOpenWith);
        _AllCommands.Add(smOpenWith);

        if (FAItems.OpenWithItems.Count > 0)
        {
          for (int i = 0; i < FAItems.OpenWithItems.Count; i++)
          {
            EFPCommandItem ci = CreateCommandItem(FAItems.OpenWithItems[i]);
            ci.Parent = smOpenWith;
            ci.Usage = EFPCommandItemUsage.Menu; // � ������ ������������ �� ����
            commandItems.Add(ci);
            _AllCommands.Add(ci);
          }
        }
        else if (FAItems.Exception == null)
        {
          EFPCommandItem ci = new EFPCommandItem("File", "OpenWithNone");
          ci.Parent = smOpenWith;
          ci.MenuText = "[ ��� ���������� ]";
          ci.ImageKey = "UnknownState";
          ci.Usage = EFPCommandItemUsage.Menu; // � ������ ������������ �� ����
          ci.Enabled = false;
          commandItems.Add(ci);
          _AllCommands.Add(ci);
        }
        else
        {
          EFPCommandItem ci = new EFPCommandItem("File", "OpenWithError");
          ci.Parent = smOpenWith;
          ci.MenuText = "[ ������ ]";
          ci.ImageKey = "Error";
          ci.Usage = EFPCommandItemUsage.Menu; // � ������ ������������ �� ����
          ci.Enabled = true;
          ci.Tag = FAItems.Exception;
          ci.Click += new EventHandler(ciOpenWithError_Click);
          commandItems.Add(ci);
          _AllCommands.Add(ci);
        }
      }
      catch (Exception e)
      {
        e.Data["FileExt"] = fileExt;
        e.Data["CommandItems"] = commandItems.ToString();

        // ���������� ���������� ���� ���, ������ ������ ������� � log-����

        string Title = "������ ������������� EFPFileAssociationsCommandItemsHandler";
        if (_ExceptionShown)
          LogoutTools.LogoutException(e, Title);
        else
        {
          _ExceptionShown = false;
          EFPApp.ShowException(e, Title);
        }
      }

      #endregion
    }

    private static bool _ExceptionShown = false;

    #endregion

    #region ��� ����� � ����������

    /// <summary>
    /// ���������� �����, ������� �����.
    /// �������� � ������������
    /// </summary>
    public string FileExt { get { return _FileExt; } }
    private string _FileExt;

    /// <summary>
    /// ���� � ������������� ����� �� �����.
    /// �������� ����� ���� ����������� ���� �� ������ �������� �� �����, ���� ���� ���������� �������,
    /// ���� � ����������� ������� FileNeeded
    /// </summary>
    public AbsPath FilePath
    {
      get { return _FilePath; }
      set { _FilePath = value; }
    }
    private AbsPath _FilePath;

    /// <summary>
    /// ������� ���������� ����� ����������� ����� �� ������.
    /// ���� �������� FilePath ��������������� �� ������ ������������ �������� �� �����, �� ���������� �� �����.
    /// ������ ���������� ������ ��������� FilePath.IsEmpty. ���� �������� �� ���� �����������, ����������
    /// ���������� (�� ��������� �������) ���� � ������������� �������� FilePath.
    /// ���� ����� ������ ����������� �������� FilePath �� ����������� ��� ����� ��� �� �����, �������������
    /// ���������� ��� ���������� �������.
    /// ���������� ����� ���������� �������� Cancel=true, ����� ������������� ���������� ������� ��� ������ ����������.
    /// � ���� ������ ����������� ������ ��������� �� ������ ����� �� ����������� �������.
    /// </summary>
    public event CancelEventHandler FileNeeded;

    /// <summary>
    /// ���������� �����.
    /// ���������� ����� ����������� ����� �������.
    /// 1. �������� ������� FileNeeded, ���� ���� ����������.
    /// 2. ���������, ��� �������� FilePath �����������.
    /// 3. ��������� ������� ����� �� �����
    /// </summary>
    /// <returns>���������� false, ���� ���������� FileNeeded ��������� �������� Cancel</returns>
    public bool PrepareFile()
    {
      if (FileNeeded != null)
      {
        CancelEventArgs Args = new CancelEventArgs();
        FileNeeded(this, Args);
        if (Args.Cancel)
          return false;
      }
      if (FilePath.IsEmpty)
        throw new NullReferenceException("�������� FilePath �� �����������");
      if (!System.IO.File.Exists(FilePath.Path))
        throw new System.IO.FileNotFoundException("���� �� ����������", FilePath.Path);

      return true;
    }

    #endregion

    #region ������� ����

    /// <summary>
    /// ��� ��������� ������� ����
    /// </summary>
    private List<EFPCommandItem> _AllCommands;

    private EFPCommandItem CreateCommandItem(FileAssociationItem fa)
    {
      EFPCommandItem ci = new EFPCommandItem("File", "Open" + Guid.NewGuid().ToString());
      ci.MenuText = fa.DisplayName;
      ci.Tag = fa;
      ci.Click += OpenFile_Click;

      ci.Image = EFPApp.FileExtAssociations.GetIconImage(fa.IconPath, fa.IconIndex, true);
      if (ci.Image == null)
        ci.ImageKey = "EmptyImage"; // ����� �� ����� ������ �� ������ ������������

      ci.ToolTipText = "������� ���� � ������� ���������� " + fa.DisplayName + " (" + fa.ProgramPath.FileName + ")";

      return ci;
    }

    private void OpenFile_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      FileAssociationItem FA = (FileAssociationItem)(ci.Tag);
      if (!PrepareFile())
        return;
      try
      {
        FA.Execute(FilePath);
      }
      catch (Exception e)
      {
        e.Data["FileAssociationItem"] = FA;
        e.Data["FilePath"] = FilePath;
        throw;
      }
    }

    void ciOpenWithError_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      Exception ex = (Exception)(ci.Tag);
      EFPApp.ExceptionMessageBox("�� ������� �������� �������� ���������� ��� ���������� \"" + this.FileExt + "\"", ex,
        "������� � �������");
    }

    #endregion

    #region �������� Visible

    /// <summary>
    /// ��������� ������ ��������.
    /// �� ��������� ����� true.
    /// </summary>
    public bool Visible
    {
      get
      {
        if (_AllCommands.Count == 0)
          return false;
        else
          return _AllCommands[0].Visible;
      }
      set
      {
        for (int i = 0; i < _AllCommands.Count; i++)
          _AllCommands[i].Visible = value;
      }
    }

    #endregion

    #region �������� Tag

    /// <summary>
    /// ������������ ���������������� ������
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    #endregion
  }
}
