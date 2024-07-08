using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeLibSet.Core;
using FreeLibSet.IO;
using FreeLibSet.Models.Tree;

namespace TestTreeViews
{

  class SimpleFileModel : ITreeModel
  {
    public SimpleFileModel(AbsPath dir)
    {
      _Root = new FileItem(dir.FileName, 0, true);
      AddOneDir(dir, _Root);
    }

    private static void AddOneDir(AbsPath dir, FileItem parent)
    {
      string[] a1 = System.IO.Directory.GetDirectories(dir.Path);
      for (int i = 0; i < a1.Length; i++)
      {
        AbsPath childDir = new AbsPath(a1[i]);
        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(childDir.Path);
        FileItem item = new FileItem(childDir.FileName, 0, true);
        item.Hidden = (di.Attributes & System.IO.FileAttributes.Hidden) != 0;
        parent.Children.Add(item);
        AddOneDir(childDir, item); // рекурсивный вызов
      }

      string[] a2 = System.IO.Directory.GetFiles(dir.Path);
      for (int i = 0; i < a2.Length; i++)
      {
        AbsPath filePath = new AbsPath(a2[i]);
        System.IO.FileInfo fi = new System.IO.FileInfo(filePath.Path);
        FileItem item = new FileItem(filePath.FileName, fi.Length, false);
        item.ReadOnly = (fi.Attributes & System.IO.FileAttributes.ReadOnly) != 0;
        item.Hidden = (fi.Attributes & System.IO.FileAttributes.Hidden) != 0;
        parent.Children.Add(item);
      }
    }

    private class FileItem
    {
      public FileItem(string fileName, long length, bool isDir)
      {
        _FileName = fileName;
        _Length = length;
        if (isDir)
          _Children = new List<FileItem>();
        else
          _Children = null;
      }

      public string FileName { get { return _FileName; } }
      private string _FileName;

      public long Length { get { return _Length; } }
      private long _Length;

      public List<FileItem> Children { get { return _Children; } }
      private List<FileItem> _Children;

      public bool ReadOnly { get { return _ReadOnly; } set { _ReadOnly = value; } }
      private bool _ReadOnly;

      public bool Hidden { get { return _Hidden; } set { _Hidden = value; } }
      private bool _Hidden;

      public override string ToString()
      {
        return FileName;
      }
    }

    private FileItem _Root;

    #region ITreeModel Members

    public System.Collections.IEnumerable GetChildren(TreePath treePath)
    {
      if (treePath.IsEmpty)
        return _Root.Children;

      FileItem item = treePath.LastNode as FileItem;
      if (item != null)
      {
        if (item.Children == null)
          return new DummyEnumerable<object>();
        else
          return item.Children;
      }
      else
        return new DummyEnumerable<object>();
    }

    public bool IsLeaf(TreePath treePath)
    {
      if (treePath.IsEmpty)
        return false;

      FileItem item = treePath.LastNode as FileItem;
      if (item == null)
        return true;
      else
        return item.Children == null;
    }

    public event EventHandler<TreeModelEventArgs> NodesChanged;

    public event EventHandler<TreeModelEventArgs> NodesInserted;

    public event EventHandler<TreeModelEventArgs> NodesRemoved;

    public event EventHandler<TreePathEventArgs> StructureChanged;

    #endregion
  }
}
