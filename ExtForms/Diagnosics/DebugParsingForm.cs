using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Parsing;
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
      Icon = EFPApp.MainImageIcon("Debug");

      EFPFormProvider efpForm = new EFPFormProvider(this);
      TheTabControl.ImageList = EFPApp.MainImages;

      efpText = new EFPRichTextBox(efpForm, edText);

      #region Парсинг

      tpTokens.ImageKey = "Table";

      efpTokens = new EFPDataGridView(efpForm, grTokens);
      efpTokens.Columns.AddInt("NPop", false, "№ лексемы", 2);
      efpTokens.Columns.AddInt("Start", false, "Начальная позиция", 3);
      efpTokens.Columns.AddInt("Length", false, "Длина", 3);
      efpTokens.Columns.AddText("TokenType", false, "Лексема", 10, 5);
      efpTokens.Columns.AddTextFill("Text", false, "Текст", 100, 5);
      efpTokens.Columns.AddText("Parser", false, "Парсер", 15, 5);
      efpTokens.DisableOrdering();
      efpTokens.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(efpTokens_GetRowAttributes);
      efpTokens.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(efpTokens_GetCellAttributes);
      efpTokens.UseRowImages = true;

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
      tvExpr.ImageList = EFPApp.MainImages;
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
      efpTokens.InitTopLeftCellTotalInfo();

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
        args.ColorType = EFPDataGridViewColorType.Alter;

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
      DebugTools.DebugObject(tk, "Лексема №" + (efpTokens.CurrentRowIndex + 1).ToString());
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
        TreeNode DummyNode = new TreeNode("Разбор выражения не выполнен");
        tvExpr.Nodes.Add(DummyNode);
        DummyNode.ImageKey = "No";
        DummyNode.SelectedImageKey = DummyNode.ImageKey;
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
      TreeNode Node = new TreeNode(s);
      Node.Tag = expression;
      Node.ToolTipText = s + Environment.NewLine+"[" + expression.GetType().ToString() + "]";
      nodes.Add(Node);

      List<IExpression> Children = new List<IExpression>();
      expression.GetChildExpressions(Children);
      for (int i = 0; i < Children.Count; i++)
        AddExprNode(Node.Nodes, Children[i]);


      ErrorMessageList Errors = ParsingData.GetErrorMessages(expression);
      switch (Errors.Severity)
      {
        case ErrorMessageKind.Error:
          Node.ImageKey = "Error";
          Node.SelectedImageKey = Node.ImageKey;
          break;
        case ErrorMessageKind.Warning:
          Node.ImageKey = "Warning";
          Node.SelectedImageKey = Node.ImageKey;
          break;
        default:
          if (Children.Count == 0)
          {
            Node.ImageKey = "Item";
            Node.SelectedImageKey = Node.ImageKey;
          }
          else
          {
            Node.ImageKey = "TreeViewClosedFolder";
            Node.SelectedImageKey = "TreeViewOpenFolder";
          }
          break;
      }

      List<Token> Tokens = new List<Token>();
      expression.GetTokens(Tokens);
      for (int i = 0; i < Tokens.Count; i++)
      {
        if (!_TokenIndexExprNodes.ContainsKey(Tokens[i]))
          _TokenIndexExprNodes.Add(Tokens[i], Node);
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
        DebugTools.ShowException(e, "Ошибка обработки DoubleClick"); // не используем EFPApp
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
      ParsingErrorItemData Data = (ParsingErrorItemData)(args.Item.Tag);
      efpTokens.SetFocus();
      efpTokens.CurrentRowIndex = Data.TokenIndex;
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