using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V. 
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

namespace FreeLibSet.IO
{
  #region ������������ PathEnumerateKind

  /// <summary>
  /// ��� ������ �������������: �����, �������� ��� �� � ������
  /// </summary>
  public enum PathEnumerateKind
  {
    /// <summary>
    /// ������������� ����� ���������� ����� ������.
    /// </summary>
    Files,

    /// <summary>
    /// ������������� ����� ���������� ����� ���������. ����� ��������������� �� �����.
    /// �������� ������� �� ������������ ��� ������������.
    /// </summary>
    Directories,

    /// <summary>
    /// ������������� ����� ���������� � ����� ������ � ����� ���������.
    /// �������� ������� �� ������������ ��� ������������.
    /// </summary>
    FilesAndDirectories
  }

  #endregion

  #region ������������ PathEnumerateMode

  /// <summary>
  /// ������� �������� ������ � ��������� � �������� ������������� �������� � ������� FileEnumerable
  /// </summary>
  public enum PathEnumerateMode
  {
    /// <summary>
    /// ������� �����, ����� ��������. ���� ����� ������������ �� ���������.
    /// </summary>
    FilesAndDirectories,

    /// <summary>
    /// ������� ��������� ��������, ����� - ����� � ������� ��������
    /// </summary>
    DirectoriesAndFiles,

    /// <summary>
    /// ������������� ������� ������
    /// </summary>
    FilesOnly,

    /// <summary>
    /// ����������� �������� ��������� ��������� ��� ��������� ������ � ������� ��������
    /// </summary>
    DirectoriesOnly,

    /// <summary>
    /// ������� ������� ������ ���� ��������
    /// </summary>
    None
  }

  #endregion

  #region ������������ PathEnumarateSort

  /// <summary>
  /// ������� ���������� ������ � ��������� ��� ��������
  /// </summary>
  public enum PathEnumerateSort
  {
    /// <summary>
    /// ������� �� ���������.
    /// </summary>
    None,

    /// <summary>
    /// ���������� �� �����
    /// </summary>
    ByName,

    /// <summary>
    /// ���������� �� ����������.
    /// ��� ������� ����������, ���� "*.fb2.zip" ������������ ������ ��������� ����������
    /// </summary>
    ByExtension
  }

  #endregion

  #region ������������ PathEnumerateStage

  internal enum PathEnumerateStage
  {
    Start,
    Files,
    Directories,
    Finish
  }

  #endregion

  /// <summary>
  /// ��������� �������.
  /// ��������� ��������� AbsPathEnumerable.Enumerator � �������� �������� ����� ���������� ���������� ��������.
  /// </summary>
  public sealed class EnumDirectoryEventArgs : EventArgs
  {
    #region ���������� �����������

    internal EnumDirectoryEventArgs(AbsPath directory, RelPath directoryRel, int level)
    {
      _Directory = directory;
      _DirectoryRel = directoryRel;
      _Level = level;

      _Stage = PathEnumerateStage.Start;
    }

    /// <summary>
    /// ������������� ������������� �������
    /// </summary>
    /// <param name="source"></param>
    internal void Init(PathEnumerableBase source)
    {
      _EnumerateMode = source.EnumerateMode;
      _FileSearchPattern = source.FileSearchPattern;
      _FileSort = source.FileSort;
      _ReverseFiles = source.ReverseFiles;
      _DirectorySearchPattern = source.DirectorySearchPattern;
      _DirectorySort = source.DirectorySort;
      _ReverseDirectories = source.ReverseDirectories;
    }

    #endregion

    #region ������������� ��������

    /// <summary>
    /// ��������������� ������� - ���������� ����
    /// </summary>
    public AbsPath Directory { get { return _Directory; } }
    private AbsPath _Directory;


    /// <summary>
    /// ��������������� ������� - ���� ������������ RootDirectory
    /// </summary>
    public RelPath DirectoryRel { get { return _DirectoryRel; } }
    private RelPath _DirectoryRel;

    /// <summary>
    /// ������� �������� �������� Directory ������������ �������� ��������, � �������� ���������� �������.
    /// ��� ������ ������ ������� ��� RootDirectory ����� �������� 0. ��� ������������ ������� ������ - 1, � �.�.
    /// </summary>
    public int Level { get { return _Level; } }
    private int _Level;

    /// <summary>
    /// ��� �������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      return Directory.Path + " (Level=" + Level.ToString() + ")";
    }

    #endregion

    #region ������������� ��������

    private void CheckNotReadOnly()
    {
      if (_Stage != PathEnumerateStage.Start)
        throw new ObjectReadOnlyException("������ �������� �������� ����� ������ � ����������� ������� BeforeDirectory");
    }

    /// <summary>
    /// ����� ��������� ��������.
    /// ���������������� ���������� �����, ��������, ���������� �������� �������
    /// </summary>
    public PathEnumerateMode EnumerateMode
    {
      get { return _EnumerateMode; }
      set
      {
        CheckNotReadOnly();
        _EnumerateMode = value;
      }
    }
    private PathEnumerateMode _EnumerateMode;

    /// <summary>
    /// ����� ��� ������������ ������. 
    /// </summary>
    public string FileSearchPattern
    {
      get { return _FileSearchPattern; }
      set
      {
        CheckNotReadOnly();
        _FileSearchPattern = value;
      }
    }
    private string _FileSearchPattern;

    /// <summary>
    /// ���������� ������ ��� ��������. 
    /// </summary>
    public PathEnumerateSort FileSort
    {
      get { return _FileSort; }
      set
      {
        CheckNotReadOnly();
        _FileSort = value;
      }
    }
    private PathEnumerateSort _FileSort;

    /// <summary>
    /// ���� ���������� � true, �� ����� ����� ������������ � �������� ������� ����������.
    /// </summary>
    public bool ReverseFiles
    {
      get { return _ReverseFiles; }
      set
      {
        CheckNotReadOnly();
        _ReverseFiles = value;
      }
    }
    private bool _ReverseFiles;

    /// <summary>
    /// ����� ��� ������������ ���������. 
    /// </summary>
    public string DirectorySearchPattern
    {
      get { return _DirectorySearchPattern; }
      set
      {
        CheckNotReadOnly();
        _DirectorySearchPattern = value;
      }
    }
    private string _DirectorySearchPattern;

    /// <summary>
    /// ���������� ��������� ��� ��������. 
    /// </summary>
    public PathEnumerateSort DirectorySort
    {
      get { return _DirectorySort; }
      set
      {
        CheckNotReadOnly();
        _DirectorySort = value;
      }
    }
    private PathEnumerateSort _DirectorySort;

    /// <summary>
    /// ���� ���������� � true, �� ����� ����� ������������ � �������� ������� ����������.
    /// </summary>
    public bool ReverseDirectories
    {
      get { return _ReverseDirectories; }
      set
      {
        CheckNotReadOnly();
        _ReverseDirectories = value;
      }
    }
    private bool _ReverseDirectories;

    #endregion

    #region ���������� ����, ������������ ��������������

    internal string[] Items;

    internal int CurrentIndex;

    /// <summary>
    /// ������������ � ��������� Stage=Directory.
    /// </summary>
    internal bool SubDirFlag;

    internal PathEnumerateStage Stage { get { return _Stage; } }
    private PathEnumerateStage _Stage;

    /// <summary>
    /// ��������� � ���������� ����� ��������.
    /// ������ ������ �������� Stage � �������� ��������� �� ���������, ������ �� EnumerateMode.
    /// �������� ��������� �� �����������
    /// ��� ��������� Finish ������ �� ������.
    /// </summary>
    private void SetNextStageValue()
    {
      switch (_Stage)
      {
        case PathEnumerateStage.Start:
          switch (_EnumerateMode)
          {
            case PathEnumerateMode.FilesOnly:
            case PathEnumerateMode.FilesAndDirectories:
              _Stage = PathEnumerateStage.Files;
              break;
            case PathEnumerateMode.DirectoriesOnly:
            case PathEnumerateMode.DirectoriesAndFiles:
              _Stage = PathEnumerateStage.Directories;
              break;
            default:
              _Stage = PathEnumerateStage.Finish;
              break;
          }
          break;
        case PathEnumerateStage.Files:
          switch (_EnumerateMode)
          {
            case PathEnumerateMode.FilesAndDirectories:
              _Stage = PathEnumerateStage.Directories;
              break;
            default:
              _Stage = PathEnumerateStage.Finish;
              break;
          }
          break;
        case PathEnumerateStage.Directories:
          switch (_EnumerateMode)
          {
            case PathEnumerateMode.DirectoriesAndFiles:
              _Stage = PathEnumerateStage.Files;
              break;
            default:
              _Stage = PathEnumerateStage.Finish;
              break;
          }
          break;
        default:
          _Stage = PathEnumerateStage.Finish;
          break;
      }
    }

    /// <summary>
    /// ��������� � ���������� ����� ��������.
    /// � ��������� ������ ������ ��� ���������, � ����������� �� Stage.
    /// CurrentIndex ��������� �������� (-1).
    /// </summary>
    internal void SetNextStage()
    {
      SetNextStageValue();

      switch (_Stage)
      {
        case PathEnumerateStage.Files:
          Items = System.IO.Directory.GetFiles(Directory.Path,
            String.IsNullOrEmpty(FileSearchPattern) ? "*" : FileSearchPattern,
            System.IO.SearchOption.TopDirectoryOnly);
          SortItems(FileSort, ReverseFiles);
          break;
        case PathEnumerateStage.Directories:
          Items = System.IO.Directory.GetDirectories(Directory.Path,
            String.IsNullOrEmpty(DirectorySearchPattern) ? "*" : DirectorySearchPattern,
            System.IO.SearchOption.TopDirectoryOnly);
          SortItems(DirectorySort, ReverseDirectories);
          break;
        default:
          Items = null;
          break;
      }

      CurrentIndex = -1;
      SubDirFlag = false;
    }

    private void SortItems(PathEnumerateSort sort, bool reverse)
    {
      switch (sort)
      {
        case PathEnumerateSort.ByName:
          Array.Sort<string>(Items, ByNameComparison);
          break;
        case PathEnumerateSort.ByExtension:
          Array.Sort<string>(Items, ByExtensionComparison);
          break;
      }

      if (reverse)
        Array.Reverse(Items);
    }

    private static int ByNameComparison(string x, string y)
    {
      string x2 = System.IO.Path.GetFileName(x);
      string y2 = System.IO.Path.GetFileName(y);

      int res = String.Compare(x2, y2, StringComparison.OrdinalIgnoreCase);
      if (res != 0)
        return res;

      // ��� �������� ������.
      // � Windows ������� �� ����� ���������� ����, ������������ ���������,
      // ������� ���������� ������� ������ ����� ���������
      // if (AbsPath.ComparisonType == StringComparison.Ordinal)
      return String.Compare(x2, y2, StringComparison.Ordinal);
    }


    private static int ByExtensionComparison(string x, string y)
    {
      #region ��������� ����������

      string x3 = System.IO.Path.GetExtension(x);
      string y3 = System.IO.Path.GetExtension(y);

      int res = String.Compare(x3, y3, StringComparison.OrdinalIgnoreCase);
      if (res != 0)
        return res;

      if (AbsPath.ComparisonType == StringComparison.Ordinal)
      {
        res = String.Compare(x3, y3, StringComparison.Ordinal);
        if (res != 0)
          return res;
      }

      #endregion

      #region ��������� �����

      string x2 = System.IO.Path.GetFileNameWithoutExtension(x);
      string y2 = System.IO.Path.GetFileNameWithoutExtension(y);
      res = String.Compare(x2, y2, StringComparison.OrdinalIgnoreCase);
      if (res != 0)
        return res;

      return String.Compare(x2, y2, StringComparison.Ordinal);

      #endregion
    }

    #endregion
  }

  /// <summary>
  /// ������� ��� ������� EnumDirectory
  /// </summary>
  /// <param name="sender">AbsPatheEnumerable ��� RelPathEnumerable</param>
  /// <param name="args">��������� �������. � ��� ����� �������� ������� �������� ��� ���������� ��������</param>
  public delegate void EnumDirectoryEventHandler(object sender, EnumDirectoryEventArgs args);

  /// <summary>
  /// ������� ����� ��� AbsPathEnumerable (� RelPathEnumerable, ����� ����� ����������)
  /// </summary>
  public class PathEnumerableBase
  {
    #region ���������� �����������

    /// <summary>
    /// ������� ������, ���������� �������� ��������� RootDirectory � EnumerateKind.
    /// </summary>
    /// <param name="rootDirectory">�������� ������� ��� ������������. ������ ���� �����</param>
    /// <param name="enumerateKind">��� ������ ������������ ��� ������������: ����� �/��� ��������</param>
    protected PathEnumerableBase(AbsPath rootDirectory, PathEnumerateKind enumerateKind)
    {
      if (rootDirectory.IsEmpty)
        throw new ArgumentException("�� ����� �������� ������� ��� ������������");

      _RootDirectory = rootDirectory;
      _EnumerateKind = enumerateKind;

      _FileSearchPattern = "*";
      _FileSort = PathEnumerateSort.None;
      _DirectorySearchPattern = "*";
      _DirectorySort = PathEnumerateSort.None;
    }

    #endregion

    #region ��������, ����������� ���������

    /// <summary>
    /// �������� ������� ��� ������������.
    /// �� ����� ���� ������.
    /// �������� � ������������.
    /// </summary>
    public AbsPath RootDirectory { get { return _RootDirectory; } }
    private AbsPath _RootDirectory;

    /// <summary>
    /// ��� ������ ������������ ��� ������������: ����� �/��� ��������.
    /// �������� � ������������.
    /// </summary>
    public PathEnumerateKind EnumerateKind { get { return _EnumerateKind; } }
    private PathEnumerateKind _EnumerateKind;

    /// <summary>
    /// ����� ��������. �� ��������� - FilesAndDirectories - ������� �����, ����� - �����������.
    /// </summary>
    public PathEnumerateMode EnumerateMode { get { return _EnumerateMode; } set { _EnumerateMode = value; } }
    private PathEnumerateMode _EnumerateMode;

    /// <summary>
    /// ����� ��� ������������ ������. �� ��������� - "*" - ��� �����.
    /// </summary>
    public string FileSearchPattern { get { return _FileSearchPattern; } set { _FileSearchPattern = value; } }
    private string _FileSearchPattern;

    /// <summary>
    /// ���������� ������ ��� ��������. �� ��������� - None - ������� �� ���������.
    /// </summary>
    public PathEnumerateSort FileSort { get { return _FileSort; } set { _FileSort = value; } }
    private PathEnumerateSort _FileSort;

    /// <summary>
    /// ���� ���������� � true, �� ����� ����� ������������ � �������� ������� ����������.
    /// �� ��������� - false.
    /// </summary>
    public bool ReverseFiles { get { return _ReverseFiles; } set { _ReverseFiles = value; } }
    private bool _ReverseFiles;

    /// <summary>
    /// ����� ��� ������������ ���������. �� ��������� - "*" - ��� ��������.
    /// </summary>
    public string DirectorySearchPattern { get { return _DirectorySearchPattern; } set { _DirectorySearchPattern = value; } }
    private string _DirectorySearchPattern;


    /// <summary>
    /// ���������� ��������� ��� ��������. �� ��������� - None - ������� �� ���������.
    /// </summary>
    public PathEnumerateSort DirectorySort { get { return _DirectorySort; } set { _DirectorySort = value; } }
    private PathEnumerateSort _DirectorySort;

    /// <summary>
    /// ���� ���������� � true, �� ����� ����� ������������ � �������� ������� ����������.
    /// �� ��������� - false.
    /// </summary>
    public bool ReverseDirectories { get { return _ReverseDirectories; } set { _ReverseDirectories = value; } }
    private bool _ReverseDirectories;

    #endregion

    #region ������� �������� ��������

    /// <summary>
    /// ������� ���������� ����� ���������� ������� ��������.
    /// ��� �������, ���������� ���� ��� ��� ��������� ��������� ��������.
    /// � ��������� ������� ���������� ��������� ��������� �� ����� �������, � ���������� ����� �� ��������.
    /// ��������, ����� ���������� �������� ��������
    /// </summary>
    public event EnumDirectoryEventHandler BeforeDirectory;

    /// <summary>
    /// ������� ���������� ����� ��������� ������� ��������, ��� �������� ���������� ������� BeforeDirectory.
    /// ������� ����������, ���� ���� ������� ������������ ���������� �������� EnumerateMode=None.
    /// ���������� ������� �� ����� ������ �������� � ���������� ������� �� �����, ��������, ������� �������.
    /// </summary>
    public event EnumDirectoryEventHandler AfterDirectory;

    #endregion

    #region ����� ���������� �������������

    internal bool MoveNext(Stack<EnumDirectoryEventArgs> stack)
    {
      if (stack.Count == 0)
        PushDirectory(stack, String.Empty, 0);

      while (stack.Count > 0)
      {
        EnumDirectoryEventArgs args = stack.Peek();
        switch (args.Stage)
        {
          case PathEnumerateStage.Files:
            args.CurrentIndex++;
            if (args.CurrentIndex < args.Items.Length)
            {
              switch (EnumerateKind)
              {
                case PathEnumerateKind.Files:
                case PathEnumerateKind.FilesAndDirectories:
                  return true;

              }
            }
            args.SetNextStage();
            break;

          case PathEnumerateStage.Directories:
            if (!args.SubDirFlag)
            {
              args.CurrentIndex++;
              if (args.CurrentIndex < args.Items.Length)
              {
                args.SubDirFlag = true;
                switch (EnumerateKind)
                {
                  case PathEnumerateKind.Directories:
                  case PathEnumerateKind.FilesAndDirectories:
                    return true;  // ��������� ������� ������� �������
                }
              }
              else
              {
                args.SetNextStage();
                continue;
              }
            }
            args.SubDirFlag = false;
            switch (args.EnumerateMode)
            {
              case PathEnumerateMode.DirectoriesOnly:
              case PathEnumerateMode.FilesAndDirectories:
              case PathEnumerateMode.DirectoriesAndFiles:
                string rel2 = System.IO.Path.GetFileName(args.Items[args.CurrentIndex]);
                if (args.DirectoryRel.Path.Length > 0)
                  rel2 = args.DirectoryRel.Path + System.IO.Path.DirectorySeparatorChar + rel2;
                PushDirectory(stack, rel2, args.Level + 1);
                break;
              default:
                args.SetNextStage();
                break;
            }
            break;

          default: // Stage=Finish;
            PopDirectory(stack);
            break;
        }
      }
      return false;
    }

    /// <summary>
    /// ������� ����� ������ EnumDirectoryEventArgs, �������� ������� BeforeDirectory, � ��������� ������ � ����
    /// </summary>
    private void PushDirectory(Stack<EnumDirectoryEventArgs> stack, string relDir, int level)
    {
      EnumDirectoryEventArgs args = new EnumDirectoryEventArgs(RootDirectory + relDir, new RelPath(relDir), level);
      args.Init(this);
      if (BeforeDirectory != null)
        BeforeDirectory(this, args);

      // ������������ ����� ��������.
      // ���� ������������ ������ ��������, �������� ������� ������
      if (this.EnumerateKind == PathEnumerateKind.Directories)
      {
        switch (args.EnumerateMode)
        {
          case PathEnumerateMode.FilesAndDirectories:
          case PathEnumerateMode.DirectoriesAndFiles:
            args.EnumerateMode = PathEnumerateMode.DirectoriesOnly;
            break;
          case PathEnumerateMode.FilesOnly:
            args.EnumerateMode = PathEnumerateMode.None;
            break;
        }
      }

      args.SetNextStage();
      stack.Push(args);
    }

    /// <summary>
    /// �������� ������� AfterDirectory � ������� EnumDirectoryEventArgs �� �����
    /// </summary>
    private void PopDirectory(Stack<EnumDirectoryEventArgs> stack)
    {
#if DEBUG
      if (stack.Count == 0)
        throw new BugException("������ ����");
#endif
      EnumDirectoryEventArgs args = stack.Peek();
      if (AfterDirectory != null)
        AfterDirectory(this, args);

      stack.Pop();
    }

    #endregion
  }

  /// <summary>
  /// ����������� ������������ ������ � ������������ � ��������.
  /// � ������� �� System.Directory.GetFiles() � System.Directory.EnumerateFiles() (� Net Framework 4),
  /// ��������� ��������� ��������� ��������, ����� �� ������������� ��������, ������� �� �����.
  /// ����� ������ � ��������� �������� ��� ��������� AbsPath.
  /// </summary>
  public sealed class AbsPathEnumerable : PathEnumerableBase, IEnumerable<AbsPath>
  {
    #region ������������

    /// <summary>
    /// ������� ������, ���������� �������� ��������� RootDirectory � EnumerateKind.
    /// </summary>
    /// <param name="rootDirectory">�������� ������� ��� ������������. ������ ���� �����</param>
    /// <param name="enumerateKind">��� ������ ������������ ��� ������������: ����� �/��� ��������</param>
    public AbsPathEnumerable(AbsPath rootDirectory, PathEnumerateKind enumerateKind)
      : base(rootDirectory, enumerateKind)
    {
    }


    /// <summary>
    /// ������� ������, ���������� �������� ��������� RootDirectory.
    /// EnumerateKind ��������� �������� Files, �� ���� ������������� ����� ����� ������.
    /// </summary>
    /// <param name="rootDirectory">�������� ������� ��� ������������. ������ ���� �����</param>
    public AbsPathEnumerable(AbsPath rootDirectory)
      : this(rootDirectory, PathEnumerateKind.Files)
    {
    }

    #endregion

    #region �������������

    /// <summary>
    /// �������������
    /// </summary>
    public struct Enumerator : IEnumerator<AbsPath>
    {
      #region ���������� �����������

      internal Enumerator(AbsPathEnumerable owner)
      {
        _Owner = owner;
        _Stack = null;
      }

      #endregion

      #region ����

      private AbsPathEnumerable _Owner;

      private Stack<EnumDirectoryEventArgs> _Stack;

      #endregion

      #region IEnumerator<AbsPath> Members

      /// <summary>
      /// ���������� ��������� ���� ��� �������
      /// </summary>
      public AbsPath Current
      {
        get
        {
          if (_Stack == null)
            return AbsPath.Empty;
          if (_Stack.Count == 0)
            return AbsPath.Empty;

          EnumDirectoryEventArgs args = _Stack.Peek();
          return new AbsPath(args.Items[args.CurrentIndex]);
        }
      }

      /// <summary>
      /// ������ �� ������
      /// </summary>
      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current { get { return Current; } }

      /// <summary>
      /// ������� � ���������� ����� ��� ��������
      /// </summary>
      /// <returns>true, ���� ������� ��� �� ��������</returns>
      public bool MoveNext()
      {
        if (_Stack == null)
          _Stack = new Stack<EnumDirectoryEventArgs>();
        return _Owner.MoveNext(_Stack);
      }

      /// <summary>
      /// ����� ������������� � �������� ���������
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        _Stack = null;
      }

      #endregion
    }

    #endregion

    #region ��������� IEnumerable<AbsPath>

    /// <summary>
    /// ������� �������������
    /// </summary>
    /// <returns>�������������</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<AbsPath> IEnumerable<AbsPath>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion

    #region ��������������� ������

    /// <summary>
    /// ���������� ���������� ������ �/��� ���������, �������� ������������
    /// </summary>
    /// <returns></returns>
    public int GetCount()
    {
      int cnt = 0;
      foreach (AbsPath item in this)
        cnt++;
      return cnt;
    }

    /// <summary>
    /// ���������� ������ �����, �������� ������������
    /// </summary>
    public AbsPath[] ToArray()
    {
      List<AbsPath> lst = new List<AbsPath>();
      foreach (AbsPath item in this)
        lst.Add(item);
      return lst.ToArray();
    }

    #endregion
  }

  /// <summary>
  /// ����������� ������������ ������ � ������������ � ��������.
  /// � ������� �� System.Directory.GetFiles() � System.Directory.EnumerateFiles() (� Net Framework 4),
  /// ��������� ��������� ��������� ��������, ����� �� ������������� ��������, ������� �� �����.
  /// ����� ������ � ��������� �������� ��� ��������� RelPath.
  /// </summary>
  public sealed class RelPathEnumerable : PathEnumerableBase, IEnumerable<RelPath>
  {
    #region ������������

    /// <summary>
    /// ������� ������, ���������� �������� ��������� RootDirectory � EnumerateKind.
    /// </summary>
    /// <param name="rootDirectory">�������� ������� ��� ������������. ������ ���� �����</param>
    /// <param name="enumerateKind">��� ������ ������������ ��� ������������: ����� �/��� ��������</param>
    public RelPathEnumerable(AbsPath rootDirectory, PathEnumerateKind enumerateKind)
      : base(rootDirectory, enumerateKind)
    {
      _RootDirectoryLen = rootDirectory.SlashedPath.Length;
    }


    /// <summary>
    /// ������� ������, ���������� �������� ��������� RootDirectory.
    /// EnumerateKind ��������� �������� Files, �� ���� ������������� ����� ����� ������.
    /// </summary>
    /// <param name="rootDirectory">�������� ������� ��� ������������. ������ ���� �����</param>
    public RelPathEnumerable(AbsPath rootDirectory)
      : this(rootDirectory, PathEnumerateKind.Files)
    {
    }

    // ���������� �������� � ������� ��������, ����� ������� �������� ���� ��� ��������
    private int _RootDirectoryLen;

    #endregion

    #region �������������

    /// <summary>
    /// �������������
    /// </summary>
    public struct Enumerator : IEnumerator<RelPath>
    {
      #region ���������� �����������

      internal Enumerator(RelPathEnumerable owner)
      {
        _Owner = owner;
        _Stack = null;
      }

      #endregion

      #region ����

      private RelPathEnumerable _Owner;

      private Stack<EnumDirectoryEventArgs> _Stack;

      #endregion

      #region IEnumerator<RelPath> Members

      /// <summary>
      /// ���������� ��������� ���� ��� �������
      /// </summary>
      public RelPath Current
      {
        get
        {
          if (_Stack == null)
            return new RelPath(String.Empty); // ��������
          if (_Stack.Count == 0)
            return new RelPath(String.Empty); // ��������

          EnumDirectoryEventArgs args = _Stack.Peek();
          return new RelPath(args.Items[args.CurrentIndex].Substring(_Owner._RootDirectoryLen));
        }
      }

      /// <summary>
      /// ������ �� ������
      /// </summary>
      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current { get { return Current; } }

      /// <summary>
      /// ������� � ���������� ����� ��� ��������
      /// </summary>
      /// <returns>true, ���� ������� ��� �� ��������</returns>
      public bool MoveNext()
      {
        if (_Stack == null)
          _Stack = new Stack<EnumDirectoryEventArgs>();
        return _Owner.MoveNext(_Stack);
      }

      /// <summary>
      /// ����� ������������� � �������� ���������
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        _Stack = null;
      }

      #endregion
    }

    #endregion

    #region ��������� IEnumerable<RelPath>

    /// <summary>
    /// ������� �������������
    /// </summary>
    /// <returns>�������������</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<RelPath> IEnumerable<RelPath>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion

    #region ��������������� ������

    /// <summary>
    /// ���������� ���������� ������ �/��� ���������, �������� ������������
    /// </summary>
    /// <returns></returns>
    public int GetCount()
    {
      int cnt = 0;
      foreach (RelPath item in this)
        cnt++;
      return cnt;
    }

    /// <summary>
    /// ���������� ������ �����, �������� ������������
    /// </summary>
    public RelPath[] ToArray()
    {
      List<RelPath> lst = new List<RelPath>();
      foreach (RelPath item in this)
        lst.Add(item);
      return lst.ToArray();
    }

    #endregion
  }
}