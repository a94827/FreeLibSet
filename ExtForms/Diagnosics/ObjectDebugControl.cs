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
        throw new ArgumentException("Родительский управляющий элемент должен быть присоединен к форме до создания ObjectDebugControl", "parent");
      if (parent.HasChildren)
        throw new ArgumentException("Родительский управляющий элемент не должен иметь других детей", "parent");
#endif

      _TheTV = new TreeView();
      _TheTV.Dock = DockStyle.Fill;
      _TheTV.ShowNodeToolTips = true;
      _TheTV.BeforeExpand += new TreeViewCancelEventHandler(TheTV_BeforeExpand);
      _TheTV.ImageList = EFPApp.MainImages;
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
      TreeNode RootNode = AddNode(_TheTV.Nodes, "(this)", _SelectedObject);
      RootNode.ImageKey = "TreeViewOpenFolder";
      RootNode.SelectedImageKey = RootNode.ImageKey;
      RootNode.Nodes.Clear(); // возможна заглушка

      if (_SelectedObject != null)
      {
        AddPropNodes(RootNode.Nodes, _SelectedObject, _SelectedObject is Type);
      }
      RootNode.Expand();
    }

    private TreeNode AddNode(TreeNodeCollection nodes, string name, object value)
    {
      object TheValue = value;
      bool ErrFlag = false;

      StringBuilder sb1 = new StringBuilder(); // текст
      StringBuilder sb2 = new StringBuilder(); // подсказка
      sb1.Append(name);

      sb2.Append(name);
      if (value == null)
      {
        sb1.Append("[null]");

        sb2.Append("Значение null");
      }
      else
      {
        sb1.Append(": ");

        sb2.Append(Environment.NewLine);
        sb2.Append("тип: ");
        sb2.Append(value.GetType().ToString());
        sb2.Append(Environment.NewLine);
        try
        {
          sb1.Append(value.ToString());

          sb2.Append("Значение:");
          sb2.Append(value.ToString());

        }
        catch (Exception e)
        {
          sb1.Append(e.Message);

          sb2.Append("Ошибка при получении значения: ");
          sb2.Append(e.GetType().ToString());
          sb2.Append(Environment.NewLine);
          sb2.Append("Сообщение: ");
          sb2.Append(e.Message);
          ErrFlag = true;
        }
      }

      TreeNode Node = nodes.Add(sb1.ToString());
      Node.ToolTipText = sb2.ToString();
      if (ErrFlag)
      {
        Node.ImageKey = "Error";
        Node.SelectedImageKey = "Error";
      }
      else
      {
        Node.ImageKey = "Item";
        Node.SelectedImageKey = "Item";
      }
      AddDummySubNodes(Node, TheValue);
      return Node;
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
      Type typ;
      if (asType)
      {
        typ = (Type)obj;
        obj = null;
      }
      else
      {
        typ = obj.GetType();
      }

      if (!asType)
      {
        AddPropNodes1(nodes, obj, typ, false);
        AddFieldNodes1(nodes, obj, typ, false);
      }
      AddPropNodes1(nodes, obj, typ, true);
      AddFieldNodes1(nodes, obj, typ, true);


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

        object TheValue = null;
        bool ErrFlag = false;

        sb1.Append(pi[i].Name);
        sb1.Append(": ");

        sb2.Append("Свойство ");
        sb2.Append(pi[i].Name);

        sb2.Append(Environment.NewLine);
        sb2.Append("Тип свойства: ");
        sb2.Append(pi[i].PropertyType.ToString());
        sb2.Append(Environment.NewLine);

        try
        {
          object v = pi[i].GetValue(obj, null);
          if (v == null)
          {
            sb1.Append("[null]");
            sb2.Append("Значение null");
          }
          else
          {
            sb1.Append(v.ToString());

            sb2.Append("Тип значения: ");
            sb2.Append(v.GetType().ToString());
            sb2.Append(Environment.NewLine);
            sb2.Append("Значение:");
            sb2.Append(v.ToString());

            TheValue = v;
          }
        }
        catch (Exception e)
        {
          sb1.Append(e.Message);

          sb2.Append("Ошибка получения значения: ");
          sb2.Append(e.GetType().ToString());
          sb2.Append(Environment.NewLine);
          sb2.Append("Сообщение: ");
          sb2.Append(e.Message);
          ErrFlag = true;
        }

        TreeNode Node = nodes.Add(sb1.ToString());
        Node.ToolTipText = sb2.ToString();
        if (ErrFlag)
        {
          Node.ImageKey = "Error";
          Node.SelectedImageKey = "Error";
        }
        else
        {
          Node.ImageKey = "CLRProperty";
          Node.SelectedImageKey = Node.ImageKey;
        }
        AddDummySubNodes(Node, TheValue);
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

        object TheValue = null;
        bool ErrFlag = false;

        sb1.Append(fi[i].Name);
        sb1.Append(": ");

        sb2.Append("Поле ");
        sb2.Append(fi[i].Name);

        sb2.Append(Environment.NewLine);
        sb2.Append("Тип поля: ");
        sb2.Append(fi[i].FieldType.ToString());
        sb2.Append(Environment.NewLine);

        try
        {
          object v = fi[i].GetValue(obj);
          if (v == null)
          {
            sb1.Append("[null]");
            sb2.Append("Значение null");
          }
          else
          {
            sb1.Append(v.ToString());

            sb2.Append("Тип значения: ");
            sb2.Append(v.GetType().ToString());
            sb2.Append(Environment.NewLine);
            sb2.Append("Значение:");
            sb2.Append(v.ToString());

            TheValue = v;
          }
        }
        catch (Exception e)
        {
          sb1.Append(e.Message);

          sb2.Append("Ошибка получения значения: ");
          sb2.Append(e.GetType().ToString());
          sb2.Append(Environment.NewLine);
          sb2.Append("Сообщение: ");
          sb2.Append(e.Message);
          ErrFlag = true;
        }

        TreeNode Node = nodes.Add(sb1.ToString());
        Node.ToolTipText = sb2.ToString();
        if (ErrFlag)
        {
          Node.ImageKey = "Error";
          Node.SelectedImageKey = "Error";
        }
        else
        {
          Node.ImageKey = "CLRField";
          Node.SelectedImageKey = Node.ImageKey;
        }
        AddDummySubNodes(Node, TheValue);
      }

    }

    void TheTV_BeforeExpand(object sender, TreeViewCancelEventArgs args)
    {
      if (args.Node.Nodes.Count != 1)
        return;
      if (args.Node.Nodes[0].Text != "???")
        return;
      object Obj = args.Node.Nodes[0].Tag;
      args.Node.Nodes.Clear();
      AddPropNodes(args.Node.Nodes, Obj, false);
    }

    #endregion

    #region Статический метод

    public static void ShowDebugObject(object obj, string title)
    {
      Form frm = new Form();
      EFPApp.SetFormSize(frm, 70, 100);
      frm.Icon = EFPApp.MainImageIcon("Debug");

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
      frm.Icon = EFPApp.MainImageIcon("Debug");

      TabControl TheTabControl = new TabControl();
      TheTabControl.Dock = DockStyle.Fill;
      frm.Controls.Add(TheTabControl);

      for (int i = 0; i < _Titles.Count; i++)
      {
        TabPage Page = new TabPage(_Titles[i]);
        TheTabControl.TabPages.Add(Page);

        ObjectDebugControl ctrl = new ObjectDebugControl(Page);

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
