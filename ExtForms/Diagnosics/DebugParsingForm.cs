﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Parsing;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Diagnostics
{
  /// <summary>
  /// Отладочная форма для просмотра выражений
  /// </summary>
  internal partial class DebugParsingForm : Form
  {
    #region Конструктор

    public DebugParsingForm()
    {
      InitializeComponent();
      Icon = EFPApp.MainImages.Icons["Debug"];

      EFPFormProvider efpForm = new EFPFormProvider(this);
      TheTabControl.ImageList = EFPApp.MainImages.ImageList;

      efpText = new EFPRichTextBox(efpForm, edText);

      #region Парсинг

      tpTokens.ImageKey = "Table";

      efpTokens = new EFPDataGridView(efpForm, grTokens);
      efpTokens.Columns.AddInt("NPop", false, "# token", 2);
      efpTokens.Columns.AddInt("Start", false, "Start", 3);
      efpTokens.Columns.AddInt("Length", false, "Length", 3);
      efpTokens.Columns.AddText("TokenType", false, "Token type", 10, 5);
      efpTokens.Columns.AddTextFill("Text", false, "Text", 100, 5);
      efpTokens.Columns.AddText("Parser", false, "Parser", 15, 5);
      efpTokens.DisableOrdering();
      efpTokens.UseAlternation = false;
      efpTokens.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(efpTokens_GetRowAttributes);
      efpTokens.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(efpTokens_GetCellAttributes);
      efpTokens.UseRowImages = true;
      efpTokens.ShowErrorCountInTopLeftCell = true;

      efpTokens.ReadOnly = true;
      efpTokens.Control.ReadOnly = true;
      efpTokens.Control.MultiSelect = false;
      efpTokens.CanView = true;
      efpTokens.EditData += new EventHandler(efpTokens_EditData);
      efpTokens.CommandItems.EnterAsOk = false;

      efpText.Control.SelectionChanged += new EventHandler(edText_SelectionChanged);
      efpTokens.Control.CurrentCellChanged += new EventHandler(grTokens_CurrentCellChanged);

      #endregion

      #region Выражение

      tpExpr.ImageKey = "TreeView";

      efpExpr = new EFPTreeView(efpForm, tvExpr);
      tvExpr.LabelEdit = false;
      tvExpr.ImageList = EFPApp.MainImages.ImageList;
      tvExpr.HideSelection = false;
      tvExpr.DoubleClick += new EventHandler(tvExpr_DoubleClick);
      _TokenIndexExprNodes = new Dictionary<Token, TreeNode>();

      #endregion

      #region Ошибки

      efpErrors = new EFPErrorDataGridView(efpForm, grErrors);
      efpErrors.ControlledTabPage = tpErrors;
      efpErrors.ToolBarPanel = PanSpbErrors;

      #endregion
    }

    #endregion

    #region Вкладка "Парсинг"

    private readonly EFPRichTextBox efpText;

    private readonly EFPDataGridView efpTokens;

    public ParsingData ParsingData
    {
      get { return _ParsingData; }
      set
      {
        _ParsingData = value;
        InitParsing();
      }
    }
    private ParsingData _ParsingData;

    private void InitParsing()
    {
      if (_ParsingData == null)
        return;

      efpText.Text = _ParsingData.Text.Text;

      efpTokens.Control.RowCount = _ParsingData.Tokens.Count;

      efpText.Control.BackColor = SystemColors.Window;

      for (int i = 0; i < _ParsingData.Tokens.Count; i++)
      {
        Token tk = _ParsingData.Tokens[i];
        efpText.Control.Select(tk.Start, tk.Length);
        if ((i % 2) == 0)
          efpText.Control.SelectionBackColor = SystemColors.Window;
        else
          efpText.Control.SelectionBackColor = EFPApp.Colors.GridAlterBackColor;

        if (tk.ErrorMessage.HasValue)
        {
          switch (tk.ErrorMessage.Value.Kind)
          {
            case FreeLibSet.Core.ErrorMessageKind.Error:
              efpText.Control.SelectionBackColor = EFPApp.Colors.GridErrorBackColor;
              break;
            case FreeLibSet.Core.ErrorMessageKind.Warning:
              efpText.Control.SelectionBackColor = EFPApp.Colors.GridWarningBackColor;
              break;
          }
        }
      }

      efpErrors.ErrorMessages = _ParsingData.GetErrorMessages(true);
      efpErrors.EditMessage += new ErrorMessageItemEventHandler(EditError);
    }

    Token _CurrToken;

    private void InitCurrToken(int rowIndex)
    {
      _CurrToken = null;
      if (_ParsingData == null)
        return;
      if (rowIndex >= _ParsingData.Tokens.Count)
        return;
      _CurrToken = _ParsingData.Tokens[rowIndex];
    }


    void efpTokens_GetRowAttributes(object sender, EFPDataGridViewRowAttributesEventArgs args)
    {
      InitCurrToken(args.RowIndex);
      if (_CurrToken == null)
        return;

      if ((args.RowIndex % 2) == 1)
        args.ColorType = UIDataViewColorType.Alter;

      if (_CurrToken.ErrorMessage.HasValue)
        args.AddRowErrorMessage(_CurrToken.ErrorMessage.Value);
    }

    void efpTokens_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      if (_CurrToken == null)
        return;

      switch (args.ColumnIndex)
      {
        case 0:
          args.Value = args.RowIndex + 1;
          break;
        case 1:
          args.Value = _CurrToken.Start;
          break;
        case 2:
          args.Value = _CurrToken.Length;
          break;
        case 3:
          args.Value = _CurrToken.TokenType;
          break;
        case 4:
          args.Value = _CurrToken.Text;
          break;
        case 5:
          object Parser = _CurrToken.Parser;
          args.Value = Parser.GetType().Name;
          break;
      }
    }

    void efpTokens_EditData(object sender, EventArgs args)
    {
      if (!efpTokens.CheckSingleRow())
        return;

      Token tk = ParsingData.Tokens[efpTokens.CurrentRowIndex];
      DebugTools.DebugObject(tk, "Token #" + (efpTokens.CurrentRowIndex + 1).ToString());
    }

    #endregion

    #region Вкладка "Выражение"

    private readonly EFPTreeView efpExpr;

    /// <summary>
    /// Узлы дерева выражений, соответствующих индексам лексем
    /// </summary>
    private readonly Dictionary<Token, TreeNode> _TokenIndexExprNodes;

    public IExpression Expression
    {
      get { return _Expression; }
      set
      {
        _Expression = value;
        InitExpression();
      }
    }
    private IExpression _Expression;

    private void InitExpression()
    {
      tvExpr.Nodes.Clear();
      _TokenIndexExprNodes.Clear();

      if (_Expression == null)
      {
        TreeNode dummyNode = new TreeNode("Parsing has not been proceed");
        tvExpr.Nodes.Add(dummyNode);
        dummyNode.ImageKey = "No";
        dummyNode.SelectedImageKey = dummyNode.ImageKey;
        return;
      }

      AddExprNode(tvExpr.Nodes, _Expression);
      tvExpr.ExpandAll();
    }

    private void AddExprNode(TreeNodeCollection nodes, IExpression expression)
    {
      string s = expression.ToString();
      /*
      string s2 = Expression.GetType().ToString();
      if (s2 != s)
        s += " [" + s2 + "]";
       * */
      TreeNode node = new TreeNode(s);
      node.Tag = expression;
      node.ToolTipText = s + Environment.NewLine+"[" + expression.GetType().ToString() + "]";
      nodes.Add(node);

      List<IExpression> children = new List<IExpression>();
      expression.GetChildExpressions(children);
      for (int i = 0; i < children.Count; i++)
        AddExprNode(node.Nodes, children[i]);


      ErrorMessageList errors = ParsingData.TokenMap.GetErrorMessages(expression);
      switch (errors.Severity)
      {
        case ErrorMessageKind.Error:
          node.ImageKey = "Error";
          node.SelectedImageKey = node.ImageKey;
          break;
        case ErrorMessageKind.Warning:
          node.ImageKey = "Warning";
          node.SelectedImageKey = node.ImageKey;
          break;
        default:
          if (children.Count == 0)
          {
            node.ImageKey = "Item";
            node.SelectedImageKey = node.ImageKey;
          }
          else
          {
            node.ImageKey = "TreeViewClosedFolder";
            node.SelectedImageKey = "TreeViewOpenFolder";
          }
          break;
      }

      Token[] tokens = this.ParsingData.TokenMap.GetTokens(expression);
      for (int i = 0; i < tokens.Length; i++)
      {
        if (!_TokenIndexExprNodes.ContainsKey(tokens[i]))
          _TokenIndexExprNodes.Add(tokens[i], node);
      }
    }

    void tvExpr_DoubleClick(object sender, EventArgs args)
    {
      try
      {
        DoExpr_DoubleClick();
      }
      catch (Exception e)
      {
        DebugTools.ShowException(e); // не используем EFPApp
      }
    }

    private void DoExpr_DoubleClick()
    {
      TreeNode node = tvExpr.SelectedNode;
      if (node == null)
        return;

      IExpression expr = node.Tag as IExpression;
      DebugTools.DebugObject(expr, node.Text);
    }

    #endregion

    #region Вкладка "Ошибки"

    private readonly EFPErrorDataGridView efpErrors;

    private void EditError(object sender, ErrorMessageItemEventArgs args)
    {
      ParsingErrorItemData data = (ParsingErrorItemData)(args.Item.Tag);
      efpTokens.SetFocus();
      efpTokens.CurrentRowIndex = data.TokenIndex;
    }

    #endregion

    #region Синхронная подсветка

    private bool _InsideSync = false;

    void grTokens_CurrentCellChanged(object sender, EventArgs args)
    {
      try
      {
        if (_InsideSync)
          return;
        _InsideSync = true;
        try
        {
          InitCurrToken(efpTokens.CurrentRowIndex);
          if (_CurrToken != null)
            efpText.Control.Select(_CurrToken.Start, _CurrToken.Length);
        }
        finally
        {
          _InsideSync = false;
        }
      }
      catch (Exception) { }
    }

    void edText_SelectionChanged(object sender, EventArgs args)
    {
      try
      {
        if (_ParsingData == null)
          return;

        if (_InsideSync)
          return;
        _InsideSync = true;
        try
        {
          if (efpText.SelectionLength == 0)
          {
            for (int i = 0; i < _ParsingData.Tokens.Count; i++)
            {
              Token tk = _ParsingData.Tokens[i];
              if (efpText.SelectionStart >= tk.Start && efpText.SelectionStart < (tk.Start + tk.Length))
              {
                efpTokens.CurrentRowIndex = i;

                TreeNode ExprNode;
                if (_TokenIndexExprNodes.TryGetValue(_ParsingData.Tokens[i], out ExprNode))
                  efpExpr.Control.SelectedNode = ExprNode;

                break;
              }
            }
          }
        }
        finally
        {
          _InsideSync = false;
        }
      }
      catch (Exception) { }
    }

    #endregion
  }
}
