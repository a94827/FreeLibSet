// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;

#pragma warning disable 1591

namespace FreeLibSet.Forms.Diagnostics
{
  /// <summary>
  /// Просмотр свойств объекта
  /// </summary>
  public class ObjectDebugControl
  {
    #region Конструктор

    /// <summary>
    /// Добавляет в переданный родительский элемент объект TreeView
    /// </summary>
    /// <param name="parent">Родительский элемент (Panel или TabPage). Должен быть пустым</param>
    public ObjectDebugControl(Control parent)
    {
#if DEBUG
      if (parent == null)
        throw new ArgumentNullException("parent");
      if (parent.FindForm() == null)
        throw new ArgumentException("Parent control must be placed in the form before ObjectDebugControl creation", "parent");
      if (parent.HasChildren)
        throw new ArgumentException("Parent control must have no children", "parent");
#endif

      _TheTV = new TreeView();
      _TheTV.Dock = DockStyle.Fill;
      _TheTV.ShowNodeToolTips = true;
      _TheTV.BeforeExpand += new TreeViewCancelEventHandler(TheTV_BeforeExpand);
      _TheTV.ImageList = EFPApp.MainImages.ImageList;
      parent.Controls.Add(_TheTV);

      RefreshGrid();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект, свойства которого отображаются
    /// </summary>
    public object SelectedObject
    {
      get { return _SelectedObject; }
      set
      {
        _SelectedObject = value;
        RefreshGrid();
      }
    }
    private object _SelectedObject;

    private TreeView _TheTV;

    #endregion

    #region Заполнение строк таблицы

    private void RefreshGrid()
    {
      _TheTV.Nodes.Clear();

      // Корневой узел соответствует объекту
      TreeNode rootNode = AddNode(_TheTV.Nodes, "(this)", _SelectedObject);
      rootNode.ImageKey = "TreeViewOpenFolder";
      rootNode.SelectedImageKey = rootNode.ImageKey;
      rootNode.Nodes.Clear(); // возможна заглушка

      if (_SelectedObject != null)
      {
        AddPropNodes(rootNode.Nodes, _SelectedObject, _SelectedObject is Type);
      }
      rootNode.Expand();
    }

    private TreeNode AddNode(TreeNodeCollection nodes, string name, object value)
    {
      object theValue = value;
      bool errFlag = false;

      StringBuilder sb1 = new StringBuilder(); // текст
      StringBuilder sb2 = new StringBuilder(); // подсказка
      sb1.Append(name);

      sb2.Append(name);
      if (value == null)
      {
        sb1.Append("[null]");

        sb2.Append("Null value");
      }
      else
      {
        sb1.Append(": ");

        sb2.Append(Environment.NewLine);
        sb2.Append("type: ");
        sb2.Append(value.GetType().ToString());
        sb2.Append(Environment.NewLine);
        try
        {
          sb1.Append(value.ToString());

          sb2.Append("Value:");
          sb2.Append(value.ToString());

        }
        catch (Exception e)
        {
          sb1.Append(e.Message);

          sb2.Append("Getting value error: ");
          sb2.Append(e.GetType().ToString());
          sb2.Append(Environment.NewLine);
          sb2.Append("Exception message: ");
          sb2.Append(e.Message);
          errFlag = true;
        }
      }

      TreeNode node = nodes.Add(sb1.ToString());
      node.ToolTipText = sb2.ToString();
      if (errFlag)
      {
        node.ImageKey = "Error";
        node.SelectedImageKey = "Error";
      }
      else
      {
        node.ImageKey = "Item";
        node.SelectedImageKey = "Item";
      }
      AddDummySubNodes(node, theValue);
      return node;
    }

    private void AddDummySubNodes(TreeNode node, object value)
    {
      if (value == null)
        return;

      if (value is String)
        return;

      Type tp = value.GetType();
      if (tp.IsPrimitive)
        return;

      if (tp.IsEnum)
        return;

      TreeNode DummyNode = node.Nodes.Add("???");
      DummyNode.Tag = value;
    }

    private void AddPropNodes(TreeNodeCollection nodes, object obj, bool asType)
    {
      Type tp;
      if (asType)
      {
        tp = (Type)obj;
        obj = null;
      }
      else
      {
        tp = obj.GetType();
      }

      if (!asType)
      {
        AddPropNodes1(nodes, obj, tp, false);
        AddFieldNodes1(nodes, obj, tp, false);
      }
      AddPropNodes1(nodes, obj, tp, true);
      AddFieldNodes1(nodes, obj, tp, true);


      if (!asType)
      {
        if (obj is IEnumerable)
        {
          int idx = 0;
          foreach (object xx in ((IEnumerable)obj))
          {
            AddNode(nodes, "[" + idx.ToString() + "]", xx);
            idx++;
          }
        }
      }
    }

    private void AddPropNodes1(TreeNodeCollection nodes, object obj, Type typ, bool isStatic)
    {
      PropertyInfo[] pi = typ.GetProperties((isStatic ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.Public);

      StringBuilder sb1 = new StringBuilder(); // текст
      StringBuilder sb2 = new StringBuilder(); // подсказка

      for (int i = 0; i < pi.Length; i++)
      {
        //if (pi[i].IsSpecialName)
        //  continue;
        ParameterInfo[] pari = pi[i].GetIndexParameters();
        if (pari != null)
        {
          if (pari.Length > 0)
            continue;
        }

        sb1.Length = 0;
        sb2.Length = 0;

        object theValue = null;
        bool errFlag = false;

        sb1.Append(pi[i].Name);
        sb1.Append(": ");

        sb2.Append("Property ");
        sb2.Append(pi[i].Name);

        sb2.Append(Environment.NewLine);
        sb2.Append("Property type: ");
        sb2.Append(pi[i].PropertyType.ToString());
        sb2.Append(Environment.NewLine);

        try
        {
          object v = pi[i].GetValue(obj, null);
          if (v == null)
          {
            sb1.Append("[null]");
            sb2.Append("Null value");
          }
          else
          {
            sb1.Append(v.ToString());

            sb2.Append("Value type: ");
            sb2.Append(v.GetType().ToString());
            sb2.Append(Environment.NewLine);
            sb2.Append("Value:");
            sb2.Append(v.ToString());

            theValue = v;
          }
        }
        catch (Exception e)
        {
          sb1.Append(e.Message);

          sb2.Append("Getting value error: ");
          sb2.Append(e.GetType().ToString());
          sb2.Append(Environment.NewLine);
          sb2.Append("Exception message: ");
          sb2.Append(e.Message);
          errFlag = true;
        }

        TreeNode node = nodes.Add(sb1.ToString());
        node.ToolTipText = sb2.ToString();
        if (errFlag)
        {
          node.ImageKey = "Error";
          node.SelectedImageKey = "Error";
        }
        else
        {
          node.ImageKey = "CLRProperty";
          node.SelectedImageKey = node.ImageKey;
        }
        AddDummySubNodes(node, theValue);
      }

    }

    private void AddFieldNodes1(TreeNodeCollection nodes, object obj, Type typ, bool isStatic)
    {
      FieldInfo[] fi = typ.GetFields((isStatic ? BindingFlags.Static : BindingFlags.Instance) | BindingFlags.Public);

      StringBuilder sb1 = new StringBuilder(); // текст
      StringBuilder sb2 = new StringBuilder(); // подсказка

      for (int i = 0; i < fi.Length; i++)
      {
        //if (pi[i].IsSpecialName)
        //  continue;

        sb1.Length = 0;
        sb2.Length = 0;

        object theValue = null;
        bool errFlag = false;

        sb1.Append(fi[i].Name);
        sb1.Append(": ");

        sb2.Append("Field ");
        sb2.Append(fi[i].Name);

        sb2.Append(Environment.NewLine);
        sb2.Append("Field type: ");
        sb2.Append(fi[i].FieldType.ToString());
        sb2.Append(Environment.NewLine);

        try
        {
          object v = fi[i].GetValue(obj);
          if (v == null)
          {
            sb1.Append("[null]");
            sb2.Append("Null value");
          }
          else
          {
            sb1.Append(v.ToString());

            sb2.Append("Value type: ");
            sb2.Append(v.GetType().ToString());
            sb2.Append(Environment.NewLine);
            sb2.Append("Value:");
            sb2.Append(v.ToString());

            theValue = v;
          }
        }
        catch (Exception e)
        {
          sb1.Append(e.Message);

          sb2.Append("Getting value error: ");
          sb2.Append(e.GetType().ToString());
          sb2.Append(Environment.NewLine);
          sb2.Append("Exception message: ");
          sb2.Append(e.Message);
          errFlag = true;
        }

        TreeNode node = nodes.Add(sb1.ToString());
        node.ToolTipText = sb2.ToString();
        if (errFlag)
        {
          node.ImageKey = "Error";
          node.SelectedImageKey = "Error";
        }
        else
        {
          node.ImageKey = "CLRField";
          node.SelectedImageKey = node.ImageKey;
        }
        AddDummySubNodes(node, theValue);
      }

    }

    void TheTV_BeforeExpand(object sender, TreeViewCancelEventArgs args)
    {
      if (args.Node.Nodes.Count != 1)
        return;
      if (args.Node.Nodes[0].Text != "???")
        return;
      object obj = args.Node.Nodes[0].Tag;
      args.Node.Nodes.Clear();
      AddPropNodes(args.Node.Nodes, obj, false);
    }

    #endregion

    #region Статический метод

    public static void ShowDebugObject(object obj, string title)
    {
      Form frm = new Form();
      EFPApp.SetFormSize(frm, 70, 100);
      frm.Icon = EFPApp.MainImages.Icons["Debug"];

      frm.Text = title;
      ObjectDebugControl ctrl = new ObjectDebugControl(frm);

      ctrl.SelectedObject = obj;
      EFPFormProvider efpForm = new EFPFormProvider(frm);

      EFPApp.ShowDialog(frm, true);
    }

    #endregion
  }

  /// <summary>
  /// Окно с закладками для вывода отладочной информации о нескольких объектах 
  /// </summary>
  public class MultiDebugInfo
  {
    #region Конструктор

    public MultiDebugInfo()
    {
      _Titles = new List<string>();
      _Objects = new List<object>();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Заголовки закладок
    /// </summary>
    private List<string> _Titles;

    /// <summary>
    /// Отлаживаемые объекты или типы объектов
    /// </summary>
    private List<object> _Objects;

    #endregion

    #region Методы

    public void Add(string title, object obj)
    {
      _Titles.Add(title);
      _Objects.Add(obj);
    }

    #endregion

    #region Вывод окна

    /// <summary>
    /// Создать форму с закладками. На каждой закладке будет размещено по одному
    /// объекту ObjectDebugControl
    /// </summary>
    /// <returns></returns>
    public Form CreateForm()
    {
      Form frm = new Form();
      frm.Icon = EFPApp.MainImages.Icons["Debug"];

      TabControl TheTabControl = new TabControl();
      TheTabControl.Dock = DockStyle.Fill;
      frm.Controls.Add(TheTabControl);

      for (int i = 0; i < _Titles.Count; i++)
      {
        TabPage page = new TabPage(_Titles[i]);
        TheTabControl.TabPages.Add(page);

        ObjectDebugControl ctrl = new ObjectDebugControl(page);

        ctrl.SelectedObject = _Objects[i];
      }
      return frm;
    }

    public void ShowForm(string title)
    {
      Form frm = CreateForm();
      EFPApp.SetFormSize(frm, 70, 100);

      frm.Text = title;
      EFPFormProvider efpForm = new EFPFormProvider(frm);

      EFPApp.ShowDialog(frm, true);
    }

    #endregion
  }

}
