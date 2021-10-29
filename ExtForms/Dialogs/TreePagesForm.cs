using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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


// !!! Не используется !!!

#pragma warning disable 1591

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Форма диалога с закладками и иерархическим списком слева для выбора закладки
  /// </summary>
  internal partial class TreePagesForm : Form
  {
    #region Конструктор

    public TreePagesForm()
    {
      InitializeComponent();
    }

    #endregion
  }

  public class TreePagesFormEventArgs : EventArgs
  {
    #region Поля

    /// <summary>
    /// Управляющий элемент (панель или закладка)
    /// </summary>
    public Control Control;

    /// <summary>
    /// Этот обработчик будет вызываться при нажатии кнопки "Ok"
    /// </summary>
    public EventHandler WriteValues;

    #endregion
  }

  public delegate void TreePagesFormEventHandler(object sender,
    TreePagesFormEventArgs args);

  /// <summary>
  /// Описатель для одной закладки в TreePagesForm 
  /// </summary>
  public class TreePagesFormItem
  {
    #region Конструктор

    public TreePagesFormItem(string displayName)
    {
      _DisplayName = displayName;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя закладки в древовидном просмотре. Для получения иерархии должен 
    /// использоваться символ "|", который отделяет один уровень от другого
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private string _DisplayName;

    #endregion
  }

  /// <summary>
  /// Объект, содержащий описания закладок, которые будут показаны в диалоге 
  /// с закладками и иерархическим списком для выбора закладки
  /// </summary>
  public class TreePagesFormManager
  {
    #region Конструктор

    public TreePagesFormManager()
    {
      _Items = new List<TreePagesFormItem>();
    }

    #endregion

    #region Коллекция закладок

    public int Count { get { return _Items.Count; } }

    public TreePagesFormItem this[int Index] { get { return _Items[Index]; } }

    private List<TreePagesFormItem> _Items;

    public void Add(TreePagesFormItem item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      _Items.Add(item);
    }

    #endregion

    #region Форма

    /// <summary>
    /// Создание формы с закладками. В форме инициализируются обработчики для
    /// загрузки закладок и нажатия кнопок "ОК" и "Отмена".
    /// Созданная форма, после инициализации заголовка, может быть передана методам 
    /// EFPApp.ShowDialog() или EFPApp.ShowForm() для вывода на экран.
    /// Если форма не разрушается явно этими методами, то должен быть вызван Dispose()
    /// </summary>
    /// <returns></returns>
    public Form CreateForm()
    {
      TreePagesForm Form = new TreePagesForm();

      // Создаем список TreeView

      // Список пустых элементов TreeNode, содержащих только уровни иерархии
      Dictionary<string, TreeNode> TextNodes = new Dictionary<string, TreeNode>();
      for (int i = 0; i < _Items.Count; i++)
      {
        string s = _Items[i].DisplayName;
        int p = s.LastIndexOf('|');
        string Name;
        TreeNode ParentNode;
        if (p < 0)
        {
          Name = s;
          ParentNode = null;
        }
        else
        {
          Name = s.Substring(p + 1);
          string BaseName = s.Substring(0, p);
          ParentNode = DoTextNode(Form.TheTV, TextNodes, BaseName); // рекурсивный вызов
        }
        TreeNode ItemNode = new TreeNode(Name);
        ItemNode.Tag = _Items[i];
        if (ParentNode == null)
          Form.TheTV.Nodes.Add(ItemNode);
        else
          ParentNode.Nodes.Add(ItemNode);
      }
      return Form;
    }

    private static TreeNode DoTextNode(TreeView theTV,
      Dictionary<string, TreeNode> textNodes,
      string name)
    {
      TreeNode ResNode;
      if (textNodes.TryGetValue(name, out ResNode))
        return ResNode; // уже есть такой узел

      int p = name.LastIndexOf('|');
      if (p < 0)
      {
        ResNode = new TreeNode(name);
        theTV.Nodes.Add(ResNode);
      }
      else
      {
        string BaseName = name.Substring(0, p);
        TreeNode ParentNode = DoTextNode(theTV, textNodes, BaseName);
        name = name.Substring(p + 1);
        ResNode = new TreeNode(name);
        ParentNode.Nodes.Add(ResNode);
      }
      return ResNode;
    }

    #endregion
  }
}

