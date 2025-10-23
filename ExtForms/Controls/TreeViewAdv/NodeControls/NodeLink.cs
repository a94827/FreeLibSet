using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  /// <summary>
  /// Гиперссылка в иерархическом просмотре <see cref="TreeViewAdv"/>
  /// </summary>
  public class NodeLink : NodeTextBox
  {
    #region Конструктор

    /// <summary>
    /// Создает элемент просмотра
    /// </summary>
    public NodeLink()
    {
      if (_DefaultLinkColor.IsEmpty)
      {
        DataGridViewLinkColumn dummy = new DataGridViewLinkColumn();
        _DefaultLinkColor = dummy.LinkColor;
        _DefaultVisitedLinkColor = dummy.VisitedLinkColor;
        _DefaultActiveLinkColor = dummy.ActiveLinkColor;
      }

      _LinkColor = _DefaultLinkColor;
      _VisitedLinkColor = _DefaultVisitedLinkColor;
      _ActiveLinkColor = _DefaultActiveLinkColor;

      base.Font = new Font(base.Font, base.Font.Style | FontStyle.Underline);
      base.TextAlign = HorizontalAlignment.Left;

      _VisitedLinks = new Dictionary<object, object>();
    }

    #endregion

    #region Цвета

    private static Color _DefaultLinkColor;

    private static Color _DefaultVisitedLinkColor;

    private static Color _DefaultActiveLinkColor;

    /// <summary>
    /// Цвет гиперссылки (синий)
    /// </summary>
    public Color LinkColor { get { return _LinkColor; } set { _LinkColor = value; } }
    private Color _LinkColor;

    /// <summary>
    /// Цвет посещенной гиперссылки
    /// </summary>
    public Color VisitedLinkColor { get { return _VisitedLinkColor; } set { _VisitedLinkColor = value; } }
    private Color _VisitedLinkColor;

    /// <summary>
    /// Цвет гиперссылки в выделенной ячейке
    /// </summary>
    public Color ActiveLinkColor { get { return _ActiveLinkColor; } set { _ActiveLinkColor = value; } }
    private Color _ActiveLinkColor;


    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Возвращает true
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected override bool DrawTextMustBeFired(TreeNodeAdv node)
    {
      return true;
    }

    /// <summary>
    /// Инициализация шрифта и цвета
    /// </summary>
    /// <param name="args"></param>
    protected override void OnDrawText(DrawEventArgs args)
    {
      args.Font = Font;
      switch (args.Context.DrawSelection)
      {
        case TreeViewAdvDrawSelectionMode.Active:
        case TreeViewAdvDrawSelectionMode.FullRowSelect:
          args.TextColor = ActiveLinkColor;
          break;
        default:
          args.TextColor = LinkColor;
          object v = GetValue(args.Node);
          if (v != null && _VisitedLinks.ContainsKey(v))
            args.TextColor = VisitedLinkColor;
          break;
      }

      base.OnDrawText(args);
    }

    internal override Cursor GetCursor(TreeNodeAdv node)
    {
      object v = GetValue(node);
      if (v == null)
        return null;
      if (v.ToString().Length == 0)
        return null;

      return Cursors.Hand;
    }

    /// <summary>
    /// Обработка нажатия пробела
    /// </summary>
    /// <param name="args"></param>
    public override void KeyDown(KeyEventArgs args)
    {
      base.KeyDown(args);
      if (args.Handled)
        return;

      if (args.KeyCode == Keys.Space && Parent.CurrentNode != null && Parent.CurrentEditor == null)
      {
        TreeNodeAdvControlEventArgs args2 = new TreeNodeAdvControlEventArgs(Parent.CurrentNode, this);
        Parent.OnNodeControlContentClick(args2);
        args.Handled = true;
      }
    }

    #endregion

    #region Посещенные ссылки

    private readonly Dictionary<object, object> _VisitedLinks;

    /// <summary>
    /// Помечает значение, возвращаемое для узла <paramref name="node"/>, как посещенную ссылку
    /// </summary>
    /// <param name="node">Узел в дереве для извлечения значения</param>
    public void SetLinkVisited(TreeNodeAdv node)
    {
      object v = GetValue(node);
      if (v != null)
      {
        if (!_VisitedLinks.ContainsKey(v))
        {
          _VisitedLinks.Add(v, null);
          node.Tree.UpdateView();
        }
      }
    }

    #endregion
  }
}
