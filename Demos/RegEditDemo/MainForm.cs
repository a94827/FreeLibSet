using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Controls;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.Models.Tree;
using FreeLibSet.Win32;
using Microsoft.Win32;

namespace RegeditDemo
{
  public partial class MainForm : Form
  {
    #region Конструктор формы

    public MainForm()
    {
      InitializeComponent();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      #region Путь

      efpPath = new EFPTextBox(efpForm, edPath);
      efpPath.CanBeEmpty = true;

      btnSetPath.Image = EFPApp.MainImages.Images["ArrowDownThenLeft"];
      btnSetPath.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;
      EFPButton efpSetPath = new EFPButton(efpForm, btnSetPath);
      efpSetPath.DisplayName = "Найти путь в дереве";
      efpSetPath.Click += EfpSetPath_Click;

      efpPath.DefaultButton = btnSetPath;

      #endregion

      #region Дерево узлов

      efpTree = new EFPTreeViewAdv(efpForm, tvTree);
      efpTree.Control.UseColumns = false;
      efpTree.Control.LoadOnDemand = true;
      efpTree.ToolBarPanel = panSpbTree;
      //efpTree.NodeStateIcon.LeafImage = efpTree.NodeStateIcon.ClosedImage;
      NodeStateIcon nsi = new NodeStateIcon();
      nsi.LeafImage = nsi.ClosedImage;
      efpTree.Control.NodeControls.Add(nsi);
      NodeTextBox ntb = new NodeTextBox();
      ntb.DataPropertyName = NodeTextBox.ToStringDataPropertyName;
      efpTree.Control.Model = new RegistryTree2Model(); // Можно не вызывать Dispose() для модели
      efpTree.Control.SelectionChanged += efpTree_SelectionChanged;

      #endregion

      #region Таблица значений

      efpValues = new EFPDataGridView(efpForm, grValues);
      efpValues.ToolBarPanel = panSpbValues;

      efpValues.Control.AutoGenerateColumns = false;
      efpValues.Columns.AddImage();
      efpValues.Columns.AddText("Name", false, "Имя", 20, 10);
      efpValues.Columns.AddText("Type", false, "Тип", 10, 5);
      efpValues.Columns.AddTextFill("Value", false, "Значение", 100, 10);
      efpValues.DisableOrdering();
      efpValues.ReadOnly = true;
      efpValues.Control.ReadOnly = true;
      efpValues.CanView = false;

      lblError.Visible = false;

      #endregion
    }

    #endregion

    #region Путь

    EFPTextBox efpPath;

    private void EfpSetPath_Click(object sender, EventArgs args)
    {
      RegistryTree2Model model = (RegistryTree2Model)(efpTree.Control.Model);
      using (EFPApp.Wait("Поиск узла дерева", "Goto"))
      {
        TreePath path = model.FindPath(efpPath.Text);
        if (path.IsEmpty)
          EFPApp.ShowTempMessage("Раздел реестра не найден");
        else
        {
          TreeNodeAdv node = efpTree.Control.FindNode(path, true);
          efpTree.Control.EnsureVisible(node);
          efpTree.Control.SelectedNode = node;
          efpTree_SelectionChanged(null, null);
        }
      }
    }

    #endregion

    #region Дерево узлов

    EFPTreeViewAdv efpTree;

    private void efpTree_SelectionChanged(object sender, EventArgs args)
    {
      if (efpTree.Control.SelectedNode == null)
      {
        InitValueTable((RegistryTree2Model.KeyTag)null);
        efpPath.Text = String.Empty;
      }
      else
      {
        RegistryTree2Model.KeyTag tag = efpTree.Control.SelectedNode.Tag as RegistryTree2Model.KeyTag;
        InitValueTable(tag);
        efpPath.Text = tag.Name;
      }
    }

    private void InitValueTable(RegistryTree2Model.KeyTag tag)
    {
      try
      {
        efpValues.Control.RowCount = 0;
        if (tag == null)
          InitValueTable2(null);
        else
        {
          RegistryKey2 key = tag.Model.RegTree[tag.Name];
          InitValueTable2(key);
        }

        lblError.Visible = false;
      }
      catch (Exception e)
      {
        lblError.Text = e.Message;
        lblError.Visible = true;
      }
    }

    #endregion

    #region Таблица значений

    EFPDataGridView efpValues;

    private void InitValueTable2(RegistryKey2 key)
    {
      if (key == null)
        return;

      try
      {
        string[] aNames = key.GetValueNames(); // Значение по умолчанию может быть или не быть
        Array.Sort<string>(aNames, StringComparer.OrdinalIgnoreCase);
        bool hasDefVal = (aNames.Length > 0 && aNames[0].Length == 0);
        if (!hasDefVal)
          aNames = DataTools.MergeArrays<string>(new string[1] { String.Empty }, aNames);
        efpValues.Control.RowCount = aNames.Length;
        for (int i = 0; i < aNames.Length; i++)
          InitValueRow(efpValues.Control.Rows[i], aNames[i], key);
      }
      catch
      {
        efpValues.Control.RowCount = 0;
      }
    }

    private static readonly Dictionary<Microsoft.Win32.RegistryValueKind, string> _DisplayTypeDict = CreateDisplayTypeDict();

    private static Dictionary<RegistryValueKind, string> CreateDisplayTypeDict()
    {
      Dictionary<RegistryValueKind, string> dict = new Dictionary<RegistryValueKind, string>();

      dict.Add(RegistryValueKind.String, "REG_SZ");
      dict.Add(RegistryValueKind.ExpandString, "REG_EXPAND_SZ");
      dict.Add(RegistryValueKind.Binary, "REG_BINARY");
      dict.Add(RegistryValueKind.DWord, "REG_DWORD");
      dict.Add(RegistryValueKind.MultiString, "REG_MULTI_SZ");
      dict.Add(RegistryValueKind.QWord, "REG_QWORD");

      return dict;
    }

    private void InitValueRow(DataGridViewRow row, string name, RegistryKey2 key)
    {
      if (String.IsNullOrEmpty(name))
        row.Cells[1].Value = "(По умолчанию)";
      else
        row.Cells[1].Value = name;
      Microsoft.Win32.RegistryValueKind type;
      object value = key.GetValue(name, null, out type);
      if (value == null)
      {
        row.Cells[2].Value = null;
        row.Cells[3].Value = "(Значение не присвоено)";
      }
      else
      {
        string sType;
        if (_DisplayTypeDict.TryGetValue(type, out sType))
          row.Cells[2].Value = sType;
        else
          row.Cells[2].Value = type.ToString();

        switch (type)
        {
          case RegistryValueKind.DWord:
            row.Cells[3].Value = "0x" + ((IFormattable)value).ToString("x8", null) + " (" + value.ToString() + ")";
            break;
          case RegistryValueKind.QWord:
            row.Cells[3].Value = "0x" + ((IFormattable)value).ToString("x16", null) + " (" + value.ToString() + ")";
            break;
          case RegistryValueKind.Binary:
            row.Cells[3].Value = DataTools.BytesToHex((byte[])value, false, " ");
            break;
          default:
            row.Cells[3].Value = DataTools.GetString(value);
            break;
        }
      }
    }

    #endregion

    private void panel1_Paint(object sender, PaintEventArgs e)
    {

    }
  }

  /// <summary>
  /// Модель реестра для иерархического просмотра
  /// </summary>
  public class RegistryTree2Model : DisposableObject, ITreeModel
  {
    #region Конструктор и Dispose

    public RegistryTree2Model()
    {
      _RegTree = new RegistryTree2(true); // для редактирования не используем

      string[] rootKeyNames = new string[] {
        "HKEY_CLASSES_ROOT",
        "HKEY_CURRENT_USER",
        "HKEY_LOCAL_MACHINE",
        "HKEY_USERS",
        "HKEY_CURRENT_CONFIG"};

      _RootTags = new KeyTag[rootKeyNames.Length];
      for (int i = 0; i < rootKeyNames.Length; i++)
        _RootTags[i] = new KeyTag(this, rootKeyNames[i]);
    }

    public RegistryTree2 RegTree { get { return _RegTree; } }
    private RegistryTree2 _RegTree;

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_RegTree != null)
        {
          _RegTree.Dispose();
          _RegTree = null;
        }
        _RootTags = null;
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Ссылка на уровень дерева

    /// <summary>
    /// Тег узла для дерева
    /// </summary>
    public class KeyTag
    {
      #region Конструктор
      public KeyTag(RegistryTree2Model model, string name)
      {
        _Model = model;
        _Name = name;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Объект - владелец
      /// </summary>
      public RegistryTree2Model Model { get { return _Model; } }
      private readonly RegistryTree2Model _Model;

      /// <summary>
      /// Полный путь к узлу реестра, например "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows"
      /// </summary>
      public string Name { get { return _Name; } }
      private readonly string _Name;

      /// <summary>
      /// Возвращает последнюю часть имени
      /// </summary>
      /// <returns>Текстовое представление для отображения в дереве</returns>
      public override string ToString()
      {
        int p = _Name.LastIndexOf('\\');
        if (p >= 0)
          return _Name.Substring(p + 1);
        else
          return _Name; // корневой узел
      }

      public KeyTag[] Children
      {
        get
        {
          if (_Children == null)
          {
            try
            {
              RegistryKey2 key = _Model._RegTree[_Name]; // Тут может быть исключение
              string[] aNames = key.GetSubKeyNames();
              if (aNames.Length == 0)
                _Children = EmptyArray;
              else
              {
                Array.Sort<string>(aNames, StringComparer.OrdinalIgnoreCase);

                StringBuilder sb = new StringBuilder();
                KeyTag[] a = new KeyTag[aNames.Length];
                for (int i = 0; i < aNames.Length; i++)
                {
                  sb.Length = 0;
                  sb.Append(_Name);
                  sb.Append('\\');
                  sb.Append(aNames[i]);
                  a[i] = new KeyTag(_Model, sb.ToString());
                }
                _Children = a;
              }
            }
            catch
            {
              _Children = EmptyArray;
            }
          }
          return _Children;
        }
      }
      private KeyTag[] _Children;

      private static readonly KeyTag[] EmptyArray = new KeyTag[0];

      #endregion
    }

    #endregion

    #region ITreeModel

    private KeyTag[] _RootTags;

    public IEnumerable GetChildren(TreePath treePath)
    {
      if (treePath.IsEmpty)
        return _RootTags;
      else
      {
        KeyTag tag = treePath.LastNode as KeyTag;
        return tag.Children;
      }
    }

    public bool IsLeaf(TreePath treePath)
    {
      KeyTag tag = treePath.LastNode as KeyTag;
      return tag.Children.Length == 0;
    }

    #endregion

    #region События

    public event EventHandler<TreePathEventArgs> StructureChanged;
    public event EventHandler<TreeModelEventArgs> NodesChanged;
    public event EventHandler<TreeModelEventArgs> NodesInserted;
    public event EventHandler<TreeModelEventArgs> NodesRemoved;

    #endregion

    #region Поиск узла

    public TreePath FindPath(string name)
    {
      if (String.IsNullOrEmpty(name))
        return TreePath.Empty;
      string[] a = name.Split('\\');
      KeyTag[] tags = new KeyTag[a.Length];
      tags[0] = DoFindTag("", _RootTags, a[0]);
      if (tags[0] == null)
        return TreePath.Empty;
      for (int i = 1; i < a.Length; i++)
      {
        tags[i] = DoFindTag(tags[i - 1].Name, tags[i - 1].Children, a[i]);
        if (tags[i] == null)
          return TreePath.Empty;
      }
      return new TreePath(tags);
    }

    private static KeyTag DoFindTag(string parentName, KeyTag[] children, string childName)
    {
      foreach (KeyTag tag in children)
      {
        if (String.Equals(tag.ToString(), childName, StringComparison.OrdinalIgnoreCase))
          return tag;
      }
      return null;
    }

    #endregion
  }
}
