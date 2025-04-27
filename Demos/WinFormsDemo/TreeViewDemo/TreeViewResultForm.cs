using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.IO;
using FreeLibSet.Models.Tree;
using FreeLibSet.Core;
using FreeLibSet.Controls;
using FreeLibSet.Controls.TreeViewAdvNodeControls;

namespace WinFormsDemo.TreeViewDemo
{
  public partial class TreeViewResultForm : Form
  {
    public TreeViewResultForm(ITreeModel model, bool showCheckBoxes, bool fullRowSelect, bool hideSelection)
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      #region TreeView

      EFPControlWithToolBar<TreeView> cwt1 = new EFPControlWithToolBar<TreeView>(efpForm, panel1);
      efp1 = new EFPTreeView(cwt1);
      efp1.Control.ImageList = EFPApp.MainImages.ImageList;
      efp1.Control.FullRowSelect = fullRowSelect;
      efp1.Control.HideSelection = hideSelection;
      InitLocalMenu(efp1);
      CreateTreeNodesFromModel(efp1.Control.Nodes, model);

      #endregion

      #region TreeViewAdv без столбцов

      EFPControlWithToolBar<TreeViewAdv> cwt2 = new EFPControlWithToolBar<TreeViewAdv>(efpForm, panel2);
      efp2 = new EFPTreeViewAdv(cwt2);
      efp2.Control.UseColumns = false;
      efp2.CheckBoxStorage = new EFPTreeViewAdvCheckBoxStorage();
      efp2.Control.FullRowSelect = fullRowSelect;
      efp2.Control.HideSelection = hideSelection;
      InitLocalMenu(efp2);
      // Инициализация NodeStateIcon и NodeTextBox будет выполнена автоматически
      efp2.Control.Model = model;

      #endregion

      #region TreeViewAdv со столбцами

      EFPControlWithToolBar<TreeViewAdv> cwt3 = new EFPControlWithToolBar<TreeViewAdv>(efpForm, panel3);
      efp3 = new EFPTreeViewAdv(cwt3);
      efp3.Control.UseColumns = true;

      TreeColumn colName = new TreeColumn("File name", 200);
      efp3.Control.Columns.Add(colName);

      NodeStateIcon ni = new NodeStateIcon();
      ni.ParentColumn = colName;
      efp3.Control.NodeControls.Add(ni);
                        
      NodeTextBox tbName = new NodeTextBox();
      tbName.DataPropertyName = "FileName";
      tbName.ParentColumn = colName;
      efp3.Control.NodeControls.Add(tbName);

      TreeColumn colLength = new TreeColumn("Size", 100);
      efp3.Control.Columns.Add(colLength);
      NodeTextBox tbLength = new NodeTextBox();
      tbLength.DataPropertyName = "Length";
      tbLength.ParentColumn = colLength;
      tbLength.TextAlign = HorizontalAlignment.Right;
      efp3.Control.NodeControls.Add(tbLength);
                        
      TreeColumn colReadOnly = new TreeColumn("Read only", 60);
      efp3.Control.Columns.Add(colReadOnly);
      NodeCheckBox cbReadOnly = new NodeCheckBox();
      cbReadOnly.DataPropertyName = "ReadOnly";
      cbReadOnly.ParentColumn = colReadOnly;
      efp3.Control.NodeControls.Add(cbReadOnly);

      TreeColumn colHidden = new TreeColumn("Hidden", 60);
      efp3.Control.Columns.Add(colHidden);
      NodeCheckBox cbHidden = new NodeCheckBox();
      cbHidden.DataPropertyName = "Hidden";
      cbHidden.ParentColumn = colHidden;
      efp3.Control.NodeControls.Add(cbHidden);
                      
      efp3.CheckBoxStorage = new EFPTreeViewAdvCheckBoxStorage();
      efp3.Control.FullRowSelect = fullRowSelect;
      efp3.Control.HideSelection = hideSelection;
      InitLocalMenu(efp3);
      efp3.Control.Model = model;

      efp1.CheckBoxes = showCheckBoxes;
      efp2.CheckBoxes = showCheckBoxes;
      efp3.CheckBoxes = showCheckBoxes;

      #endregion

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


    private static void InitLocalMenu(IEFPTreeView efp)
    {
      EFPCommandItem ci = new EFPCommandItem("Service", "CurrentPopupPosition");
      ci.MenuText = "CurrentPopupPosition";
      ci.Tag = efp;
      ci.Click += CiCurrentPopupPosition_Click;
      ci.GroupBegin = true;
      efp.CommandItems.Add(ci);
    }

    private static void CiCurrentPopupPosition_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      IEFPTreeView efp = (IEFPTreeView)(ci.Tag);
      TextInputDialog dlg = new TextInputDialog();
      dlg.Title = "CurrentPopupPosition";
      dlg.Text = "Тест";
      dlg.Prompt = "Fictive dialog box";
      dlg.DialogPosition = efp.CurrentPopupPosition;
      dlg.ShowDialog();
    }
  }
}
