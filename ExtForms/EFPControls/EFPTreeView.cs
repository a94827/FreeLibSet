﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;

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

namespace FreeLibSet.Forms
{
  #region IEFPTreeView

  /// <summary>
  /// Интефрейс, реализуемый EFPTreeView и EFPTreeViewAdv
  /// (для поддержки общих команд локального меню)
  /// </summary>
  public interface IEFPTreeView : IEFPControl
  {
    /// <summary>
    /// Возвращает true, если в дереве показываются флажки для отметки
    /// </summary>
    bool CheckBoxes { get;}

    /// <summary>
    /// Выполяет установку или снятие всех флажков
    /// </summary>
    /// <param name="isChecked"></param>
    void CheckAll(bool isChecked);

    /// <summary>
    /// Контекст для поиска текста
    /// </summary>
    IEFPTextSearchContext TextSearchContext { get; }
  }

  #endregion

  #region EFPTreeViewAutoCheckMode

  /// <summary>
  /// Режимы взаимосвязанной установки флажков Checkbox в древовидном просмотре TreeView.
  /// Свойство EFPTreeView.AutoCheckMode
  /// </summary>
  public enum EFPTreeViewAutoCheckMode
  {
    /// <summary>
    /// Режим по умолчанию.
    /// Установка / сброс отдельных флажков никак не влияет на другие флажки
    /// </summary>
    None = 0,

    /// <summary>
    /// Установка или сброс флажка вызывает рекурсивную установку всех дочерних узлов в такое же состояние.
    /// Родительский узел (рекурсивно) помечается, если все его дочерние узлы отмечены.
    /// У родительского узла отметка снимается, если нет отметки у хотя бы одного его дочернего узла
    /// </summary>
    ParentsAndChildren = 1,
  }

  #endregion

  /// <summary>
  /// Провайдер для стандартного TreeView
  /// </summary>
  public class EFPTreeView : EFPControl<TreeView>, IEFPTreeView
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провадйер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPTreeView(EFPBaseProvider baseProvider, TreeView control)
      : base(baseProvider, control, true)
    {
      _AutoCheckMode = EFPTreeViewAutoCheckMode.None;
      _TextSearchContext = null;
      _TextSearchEnabled = true;
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    public EFPTreeView(IEFPControlWithToolBar<TreeView> controlWithToolBar)
      : base(controlWithToolBar, true)
    {
      _AutoCheckMode = EFPTreeViewAutoCheckMode.None;
      _TextSearchContext = null;
      _TextSearchEnabled = true;
    }

    #endregion

    #region Команды локального меню

    /// <summary>
    /// Создает EFPTreeViewCommandItems
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      //if (EFPApp.EasyInterface)
      //  return base.GetCommandItems();
      //else
      // 20.05.2021. Так нельзя, так как класс наследник может полагаться на наличие расширенного класса
      return new EFPTreeViewCommandItems(this);
    }

    /// <summary>
    /// Команды локального меню
    /// </summary>
    public new EFPTreeViewCommandItems CommandItems
    {
      get { return (EFPTreeViewCommandItems)(base.CommandItems); }
    }

    #endregion

    #region Установка всех отметок

    /// <summary>
    /// Рекурсивная установка всех отметок в дереве.
    /// Обработка AutoCheckMode не выполняется
    /// </summary>
    /// <param name="isChecked">true - установка отметок, false - удаление</param>
    public void CheckAll(bool isChecked)
    {
      if (!Control.CheckBoxes)
        throw new InvalidOperationException("Свойство TreeView.CheckBoxes не установлено");

      _InsideAfterCheck = true;
      Control.BeginUpdate();
      try
      {
        DoCheckAll(Control.Nodes, isChecked);
      }
      finally
      {
        Control.EndUpdate();
        _InsideAfterCheck = false;
      }
    }


    /// <summary>
    /// Рекурсивная установка всех отметок в дереве, начиная с заданного узла.
    /// Отметка устанавливается для узла <paramref name="parentNode"/> и, рекурсивно, для всех его дочерних узлов
    /// </summary>
    /// <param name="parentNode">Родительский узел</param>
    /// <param name="isChecked">true - установка отметок, false - удаление</param>
    public void CheckAll(TreeNode parentNode, bool isChecked)
    {
      if (!Control.CheckBoxes)
        throw new InvalidOperationException("Свойство TreeView.CheckBoxes не установлено");
      if (parentNode == null)
        throw new ArgumentNullException("parentNode");

      _InsideAfterCheck = true;
      Control.BeginUpdate();
      try
      {
        parentNode.Checked = isChecked;
        DoCheckAll(parentNode.Nodes, isChecked);
      }
      finally
      {
        Control.EndUpdate();
        _InsideAfterCheck = false;
      }

      DoAfterCheck(parentNode, false);
    }

    private static void DoCheckAll(TreeNodeCollection nodes, bool isChecked)
    {
      for (int i = 0; i < nodes.Count; i++)
      {
        nodes[i].Checked = isChecked;
        DoCheckAll(nodes[i].Nodes, isChecked);
      }
    }

    #endregion

    #region Взаимозависимая установка Check Box'ов

    /// <summary>
    /// Режим взаимосвязанной установки флажков, когда свойство CheckBoxes=true.
    /// По умолчанию - none - флажки не влияют друг на друга
    /// </summary>
    public EFPTreeViewAutoCheckMode AutoCheckMode
    {
      get { return _AutoCheckMode; }
      set
      {
        switch (value)
        {
          case EFPTreeViewAutoCheckMode.None:
          case EFPTreeViewAutoCheckMode.ParentsAndChildren:
            break;
          default:
            throw new ArgumentException();
        }

        _AutoCheckMode = value;
        if (!_Control_AfterCheck_Attached)
        {
          Control.AfterCheck += new TreeViewEventHandler(Control_AfterCheck);
          _Control_AfterCheck_Attached = true;
        }
      }
    }
    private EFPTreeViewAutoCheckMode _AutoCheckMode;

    /// <summary>
    /// Предотвращение повторной инициализации обработчика
    /// </summary>
    private bool _Control_AfterCheck_Attached;

    private bool _InsideAfterCheck = false;
    void Control_AfterCheck(object sender, TreeViewEventArgs args)
    {
      // Событие AfterCheck вызывается, даже если реально состояние TreeNode.CheckedEx
      // не изменилось.
      // Поэтому нельзя выполнять действия рекурсивно

      if (_InsideAfterCheck)
        return;
      _InsideAfterCheck = true;
      try
      {
        DoAfterCheck(args.Node, true);
      }
      finally
      {
        _InsideAfterCheck = false;
      }
    }

    private void DoAfterCheck(TreeNode node, bool isRequrse)
    {
      switch (AutoCheckMode)
      {
        case EFPTreeViewAutoCheckMode.ParentsAndChildren:
          int i;
          if (node.Checked)
          {
            for (i = 0; i < node.Nodes.Count; i++)
              DoSetCheckedChildren(node.Nodes[i], true, true);
            if (node.Parent != null)
            {
              if (AreAllChildNodesChecked(node.Parent))
                DoSetCheckedChildren(node.Parent, true, true);
            }
          }
          else
          {
            if (isRequrse)
            {
              for (i = 0; i < node.Nodes.Count; i++)
                DoSetCheckedChildren(node.Nodes[i], false, true);
            }
            if (node.Parent != null)
              DoSetCheckedChildren(node.Parent, false, false);
          }
          break;
        case EFPTreeViewAutoCheckMode.None:
          break;
        default:
          throw new BugException("Неизвестный режим");
      }
    }

    private void DoSetCheckedChildren(TreeNode node, bool isChecked, bool isRequrse)
    {
      if (node.Checked == isChecked)
        return;
      node.Checked = isChecked;
      DoAfterCheck(node, isRequrse);
    }

    /// <summary>
    /// Возвращает true, если у всех дочерних узлов данного узла установлены отметки.
    /// Наличие отметки у самого узла <paramref name="node"/> не проверяется.
    /// Метод НЕ является рекусивным
    /// </summary>
    /// <param name="node">Родительский узел для проверяемых узлов</param>
    /// <returns>truem </returns>
    public static bool AreAllChildNodesChecked(TreeNode node)
    {
      for (int i = 0; i < node.Nodes.Count; i++)
      {
        if (!node.Nodes[i].Checked)
          return false;
      }
      return true;
    }

    #endregion

    #region Поиск текста

    /// <summary>
    /// Контекст поиска по Ctrl-F / F3
    /// Свойство возвращает null, если TextSearchEnabled=false
    /// </summary>
    public IEFPTextSearchContext TextSearchContext
    {
      get
      {
        if (_TextSearchEnabled)
        {
          if (_TextSearchContext == null)
            _TextSearchContext = CreateTextSearchContext();
        }
        return _TextSearchContext;
      }
    }
    private IEFPTextSearchContext _TextSearchContext;

    /// <summary>
    /// Вызывается при первом обращении к свойству TextSearchContext.
    /// Непереопределенный метод создает и возвращает объект EFPTreeViewSearchContext
    /// </summary>
    /// <returns></returns>
    protected virtual IEFPTextSearchContext CreateTextSearchContext()
    {
      return new EFPTreeViewSearchContext(this);
    }

    /// <summary>
    /// Если true (по умолчанию), то доступна команда "Найти" (Ctrl-F).
    /// Если false, то свойство TextSearchContext возвращает null и поиск недоступен.
    /// Свойство можно устанавливать только до вывода просмотра на экран
    /// </summary>
    public bool TextSearchEnabled
    {
      get { return _TextSearchEnabled; }
      set
      {
        CheckHasNotBeenCreated();
        _TextSearchEnabled = value;
      }
    }
    private bool _TextSearchEnabled;

    #endregion

    #region IEFPTreeView Members

    /// <summary>
    /// Дублирует свойство TreeView.CheckBoxes.
    /// </summary>
    public bool CheckBoxes
    {
      get { return Control.CheckBoxes; }
      set { Control.CheckBoxes = value; }
    }

    #endregion

    #region Предыдущий и следующий узлы

    /// <summary>
    /// Возвращает следующий узел относительно текущего, с учетом иерархии
    /// </summary>
    /// <param name="node">Узел, от которого находится следующий узел</param>
    /// <returns>Найденный узел или null</returns>
    public TreeNode GetNextTreeNode(TreeNode node)
    {
      if (node == null)
        return null;

      if (node.GetNodeCount(false) > 0)
        return node.Nodes[0];

      while (node != null)
      {
        if (node.NextNode != null)
          return node.NextNode;
        node = node.Parent;
      }
      return null;
    }

    /// <summary>
    /// Возвращает предыдущий узел относительно текущего, с учетом иерархии
    /// </summary>
    /// <param name="node">Узел, от которого находится следующий узел</param>
    /// <returns>Найденный узел или null</returns>
    public TreeNode GetPreviousTreeNode(TreeNode node)
    {
      if (node == null)
        return null;

      if (node.PrevNode != null)
        node = node.PrevNode;
      else
      {
        node = node.Parent;
        return node;
      }

      while (true)
      {
        if (node.GetNodeCount(false) == 0)
          return node;
        node = node.Nodes[node.Nodes.Count - 1];
      }
    }

    /// <summary>
    /// Возвращает первый дочерний узел корневого узла
    /// </summary>
    public TreeNode FirstTreeNode
    {
      get
      {
        if (Control.Nodes.Count > 0)
          return Control.Nodes[0];
        else
          return null;
      }
    }

    /// <summary>
    /// Возвращает последний узел дерева в иерарахии, который был бы внизу при развороте всего дерева
    /// </summary>
    public TreeNode LastTreeNode
    {
      get
      {
        if (Control.Nodes.Count > 0)
        {
          TreeNode Node = Control.Nodes[Control.Nodes.Count - 1];
          while (true)
          {
            if (Node.GetNodeCount(false) == 0)
              return Node;
            Node = Node.Nodes[Node.Nodes.Count - 1];
          }
        }
        else
          return null;
      }
    }

    #endregion
  }
}