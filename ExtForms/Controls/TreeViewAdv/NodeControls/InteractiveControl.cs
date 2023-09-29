// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
//
// Original TreeViewAdv component from Aga.Controls.dll
// Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
// http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
// http://sourceforge.net/projects/treeviewadv/

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  /// <summary>
  /// Добавляет к элементу возможность редактирования значения пользователем.
  /// Базовый класс для текстовых полей <see cref="EditableControl"/> / <see cref="BaseTextControl"/> и флажков <see cref="NodeCheckBox"/>.
  /// </summary>
  public abstract class InteractiveControl : BindableControl
  {
    /// <summary>
    /// Если установить в true, то пользователь может редактировать значение.
    /// Для отдельных строк дерева редактирование можно запретить с помощью события <see cref="IsEditEnabledValueNeeded"/>.
    /// </summary>
    [DefaultValue(false)]
    public bool EditEnabled
    {
      get { return _editEnabled; }
      set { _editEnabled = value; }
    }
    private bool _editEnabled = false;

    /// <summary>
    /// Возвращает true, если разрешено редактирование элемента для строки дерева.
    /// Проверяется значение свойства <see cref="EditEnabled"/>. Если редактирование разрешено, вызывается событие <see cref="IsEditEnabledValueNeeded"/>.
    /// </summary>
    /// <param name="node">Узел дерева</param>
    /// <returns>Возможность редактирования</returns>
    protected bool IsEditEnabled(TreeNodeAdv node)
    {
      if (EditEnabled)
      {
        NodeControlValueEventArgs args = new NodeControlValueEventArgs(node);
        args.Value = true;
        OnIsEditEnabledValueNeeded(args);
        return Convert.ToBoolean(args.Value);
      }
      else
        return false;
    }

    /// <summary>
    /// Событие вызывается перед выполнением редактирования значения элемента.
    /// Свойство <see cref="EditEnabled"/> должно быть установлено.
    /// Если обработчик события установит <see cref="NodeControlValueEventArgs.Value"/>=false, то редактирование будет запрещено.
    /// 
    /// Предупреждение. Свойство <see cref="NodeControlValueEventArgs.Value"/> задает для этого значения признак возможности редактирования,
    /// а не текущее значение для строки дерева. Для получения текущего значения используйте метод <see cref="BindableControl.GetValue(TreeNodeAdv)"/>.
    /// </summary>
    public event EventHandler<NodeControlValueEventArgs> IsEditEnabledValueNeeded;
    private void OnIsEditEnabledValueNeeded(NodeControlValueEventArgs args)
    {
      if (IsEditEnabledValueNeeded != null)
        IsEditEnabledValueNeeded(this, args);
    }
  }
}
