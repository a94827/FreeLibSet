using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.IO;
using FreeLibSet.Models.Tree;
using FreeLibSet.Core;
using FreeLibSet.Controls;
using FreeLibSet.Controls.TreeViewAdvNodeControls;

namespace TestTreeViews
{
  public partial class TestCheckBoxesForm : Form
  {
    public TestCheckBoxesForm(bool showCheckBoxes)
    {
      InitializeComponent();

      SimpleFileModel model = new SimpleFileModel(FileTools.ApplicationBaseDir);

      EFPFormProvider efpForm = new EFPFormProvider(this);

      EFPControlWithToolBar<TreeView> cwt1 = new EFPControlWithToolBar<TreeView>(efpForm, panel1);
      efp1 = new EFPTreeView(cwt1);
      efp1.Control.ImageList = EFPApp.MainImages.ImageList;
      CreateTreeNodesFromModel(efp1.Control.Nodes, model);

      EFPControlWithToolBar<TreeViewAdv> cwt2 = new EFPControlWithToolBar<TreeViewAdv>(efpForm, panel2);
      efp2 = new EFPTreeViewAdv(cwt2);
      efp2.Control.UseColumns = false;
      efp2.CheckBoxStorage = new EFPTreeViewAdvCheckBoxStorage();
      // Инициализация NodeStateIcon и NodeTextBox будет выполнена автоматически
      efp2.Control.Model = model;

      EFPControlWithToolBar<TreeViewAdv> cwt3 = new EFPControlWithToolBar<TreeViewAdv>(efpForm, panel3);
      efp3 = new EFPTreeViewAdv(cwt3);
      efp3.Control.UseColumns = true;

      TreeColumn colName = new TreeColumn("Имя файла", 200);
      efp3.Control.Columns.Add(colName);

      NodeStateIcon ni = new NodeStateIcon();
      ni.ParentColumn = colName;
      efp3.Control.NodeControls.Add(ni);
                        
      NodeTextBox tbName = new NodeTextBox();
      tbName.DataPropertyName = "FileName";
      tbName.ParentColumn = colName;
      efp3.Control.NodeControls.Add(tbName);

      TreeColumn colLength = new TreeColumn("Размер", 100);
      efp3.Control.Columns.Add(colLength);
      NodeTextBox tbLength = new NodeTextBox();
      tbLength.DataPropertyName = "Length";
      tbLength.ParentColumn = colLength;
      tbLength.TextAlign = HorizontalAlignment.Right;
      efp3.Control.NodeControls.Add(tbLength);
                        
      TreeColumn colReadOnly = new TreeColumn("Только чтение", 60);
      efp3.Control.Columns.Add(colReadOnly);
      NodeCheckBox cbReadOnly = new NodeCheckBox();
      cbReadOnly.DataPropertyName = "ReadOnly";
      cbReadOnly.ParentColumn = colReadOnly;
      efp3.Control.NodeControls.Add(cbReadOnly);

      TreeColumn colHidden = new TreeColumn("Скрытый", 60);
      efp3.Control.Columns.Add(colHidden);
      NodeCheckBox cbHidden = new NodeCheckBox();
      cbHidden.DataPropertyName = "Hidden";
      cbHidden.ParentColumn = colHidden;
      efp3.Control.NodeControls.Add(cbHidden);
                      
      efp3.CheckBoxStorage = new EFPTreeViewAdvCheckBoxStorage();
      efp3.Control.Model = model;

      efp1.CheckBoxes = showCheckBoxes;
      efp2.CheckBoxes = showCheckBoxes;
      efp3.CheckBoxes = showCheckBoxes;


      EFPCheckBox efpCheckBoxes = new EFPCheckBox(efpForm, cbCheckBoxes);
      efpCheckBoxes.Checked = showCheckBoxes;
      efpCheckBoxes.CheckedEx.ValueChanged += efpCheckBoxes_ValueChanged;
    }

    EFPTreeView efp1;
    EFPTreeViewAdv efp2, efp3;

    void efpCheckBoxes_ValueChanged(object sender, EventArgs args)
    {
      efp1.CheckBoxes = cbCheckBoxes.Checked;
      efp2.CheckBoxes = cbCheckBoxes.Checked;
      efp3.CheckBoxes = cbCheckBoxes.Checked;
    }

    private static void CreateTreeNodesFromModel(TreeNodeCollection nodes, ITreeModel model)
    {
      DoAddNodesFromModel(nodes, model, TreePath.Empty);
    }

    private static void DoAddNodesFromModel(TreeNodeCollection nodes, ITreeModel model, TreePath path)
    {
      foreach (object tag in model.GetChildren(path))
      {
        TreePath path2 = new TreePath(path, tag);
        bool isLeaf=model.IsLeaf(path2);
        TreeNode node=new TreeNode(tag.ToString());
        if (isLeaf)
        {
          node.ImageKey = "Item";
          node.SelectedImageKey = "Item";
        }
        else
        {
          node.ImageKey = "TreeViewClosedFolder";
          node.SelectedImageKey = "TreeViewOpenFolder";
        }
        nodes.Add(node);
        if (!isLeaf)
          DoAddNodesFromModel(node.Nodes, model, path2);
      }
    }
  }

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
        item.Hidden = (fi.Attributes & System.IO.FileAttributes.Hidden)!=0;
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
