// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
//
// Original TreeViewAdv component from Aga.Controls.dll
// Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
// http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
// http://sourceforge.net/projects/treeviewadv/

using System;
using FreeLibSet.Controls.TreeViewAdvNodeControls;

namespace FreeLibSet.Controls
{
  /// <summary>
  /// Определяет режим прорисовки для ячейки иерархического просмотра
  /// </summary>
  public enum TreeViewAdvDrawSelectionMode
  {
    /// <summary>
    /// Обычная ячейка. Фон не прорисовывается (используется <see cref="TreeViewAdv.BackColor"/>
    /// а текст выводится цветом <see cref="TreeViewAdv.ForeColor"/>.
    /// </summary>
    None,

    /// <summary>
    /// Выбранный узел, когда фокус ввода находится в дереве.
    /// Используются цвета <see cref="System.Drawing.SystemColors.Highlight"/> для фона и
    /// <see cref="System.Drawing.SystemColors.HighlightText"/> для текста
    /// </summary>
    Active,

    /// <summary>
    /// Выбранный узел, когда дерево не содержит фокуса ввода.
    /// Если свойство <see cref="TreeViewAdv.HideSelection"/>=true, то подсветка скрывается и используется
    /// режим <see cref="None"/>.
    /// 
    /// Используются цвета <see cref="System.Drawing.SystemColors.InactiveBorder"/> для фона и
    /// <see cref="System.Drawing.SystemColors.ControlText"/> для текста
    /// </summary>
    Inactive,

    /// <summary>
    /// Тоже, что и <see cref="Active"/>, но не требуется рисовать фон ячейки, так как он заполняется
    /// одномоментно для всей строки узла.
    /// Для текста используется цвет <see cref="System.Drawing.SystemColors.HighlightText"/>.
    /// </summary>
    FullRowSelect
  }

  /// <summary>
  /// Режим выбора узлов в <see cref="TreeViewAdv.SelectionMode"/>.
  /// </summary>
  public enum TreeViewAdvSelectionMode
  {
    /// <summary>
    /// Можно выбрать только один узел
    /// </summary>
    Single,

    /// <summary>
    /// Можно выбрать несколько произвольных узлов
    /// </summary>
    Multi,

    /// <summary>
    /// Можно выбрать несколько узлов, если они относятся к одному родительскому узлу (или все являются узлами верхнего уровня)
    /// </summary>
    MultiSameParent
  }

  /// <summary>
  /// Позиция вставки при отпускании мыши в операции drag-and-drop
  /// </summary>
  public enum TreeViewAdvNodePosition
  {
    /// <summary>
    /// На узел (замещение)
    /// </summary>
    Inside,

    /// <summary>
    /// Перед узлом
    /// </summary>
    Before,

    /// <summary>
    /// После узла
    /// </summary>
    After
  }

#if XXX
  public enum VerticalAlignment
  {
    Top, Bottom, Center
  }
#endif

  //public enum TreeViewAdvIncrementalSearchMode
  //{
  //  None, Standard, Continuous
  //}

  /// <summary>
  /// Значения свойства <see cref="TreeViewAdv.GridLineStyle"/>
  /// </summary>
  [Flags]
  public enum TreeViewAdvGridLineStyle
  {
    /// <summary>
    /// Нет линий
    /// </summary>
    None = 0,

    /// <summary>
    /// Горизонтальные линии
    /// </summary>
    Horizontal = 1,

    /// <summary>
    /// Вертикальные линии
    /// </summary>
    Vertical = 2,

    /// <summary>
    /// Все линии
    /// </summary>
    HorizontalAndVertical = 3
  }

  /// <summary>
  /// Значение свойства <see cref="NodeIcon.ScaleMode"/>
  /// </summary>
  public enum TreeViewAdvImageScaleMode
  {
    /// <summary>
    /// Don't scale
    /// </summary>
    Clip,

    /// <summary>
    /// Scales image to fit the display rectangle, aspect ratio is not fixed.
    /// </summary>
    Fit,

    /// <summary>
    /// Scales image down if it is larger than display rectangle, taking aspect ratio into account
    /// </summary>
    ScaleDown,

    /// <summary>
    /// Scales image up if it is smaller than display rectangle, taking aspect ratio into account
    /// </summary>
    ScaleUp,

    /// <summary>
    /// Scales image to match the display rectangle, taking aspect ratio into account
    /// </summary>
    AlwaysScale,

  }
}
