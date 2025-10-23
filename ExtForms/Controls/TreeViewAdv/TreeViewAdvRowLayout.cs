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
using System.Drawing;
using FreeLibSet.Controls.TreeViewAdvNodeControls;

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvInternal
{
  internal interface ITreeViewAdvRowLayout
  {
    int PreferredRowHeight
    {
      get;
      set;
    }

    int PageRowCount
    {
      get;
    }

    int CurrentPageSize
    {
      get;
    }

    Rectangle GetRowBounds(int rowNo);

    int GetRowAt(Point point);

    int GetFirstRow(int lastPageRow);

    void ClearCache();
  }

  internal class FixedRowHeightLayout : ITreeViewAdvRowLayout
  {
    private readonly TreeViewAdv _treeView;

    public FixedRowHeightLayout(TreeViewAdv treeView, int rowHeight)
    {
      _treeView = treeView;
      PreferredRowHeight = rowHeight;
    }

    private int _rowHeight;
    public int PreferredRowHeight
    {
      get { return _rowHeight; }
      set { _rowHeight = value; }
    }

    public Rectangle GetRowBounds(int rowNo)
    {
      return new Rectangle(0, rowNo * _rowHeight, 0, _rowHeight);
    }

    public int PageRowCount
    {
      get
      {
        return Math.Max((_treeView.DisplayRectangle.Height - _treeView.ColumnHeaderHeight) / _rowHeight, 0);
      }
    }

    public int CurrentPageSize
    {
      get
      {
        return PageRowCount;
      }
    }

    public int GetRowAt(Point point)
    {
      if (point.Y < _treeView.ColumnHeaderHeight)
        return -1; // 16.01.2019 Агеев А.В.

      point = new Point(point.X, point.Y + (_treeView.FirstVisibleRow * _rowHeight) - _treeView.ColumnHeaderHeight);
      return point.Y / _rowHeight;
    }

    public int GetFirstRow(int lastPageRow)
    {
      return Math.Max(0, lastPageRow - PageRowCount + 1);
    }

    public void ClearCache()
    {
    }
  }

  internal class TreeViewAdvAutoRowHeightLayout : ITreeViewAdvRowLayout
  {
    private TreeViewAdvDrawContext _measureContext;
    private readonly TreeViewAdv _treeView;
    private readonly List<Rectangle> _rowCache;

    public TreeViewAdvAutoRowHeightLayout(TreeViewAdv treeView, int rowHeight)
    {
      _rowCache = new List<Rectangle>();
      _treeView = treeView;
      PreferredRowHeight = rowHeight;
      _measureContext = new TreeViewAdvDrawContext();
      _measureContext.Graphics = Graphics.FromImage(new Bitmap(1, 1));
    }

    private int _rowHeight;
    public int PreferredRowHeight
    {
      get { return _rowHeight; }
      set { _rowHeight = value; }
    }


    public int PageRowCount
    {
      get
      {
        if (_treeView.RowCount == 0)
          return 0;
        else
        {
          int pageHeight = _treeView.DisplayRectangle.Height - _treeView.ColumnHeaderHeight;
          int y = 0;
          for (int i = _treeView.RowCount - 1; i >= 0; i--)
          {
            y += GetRowHeight(i);
            if (y > pageHeight)
              return Math.Max(0, _treeView.RowCount - 1 - i);
          }
          return _treeView.RowCount;
        }
      }
    }

    public int CurrentPageSize
    {
      get
      {
        if (_treeView.RowCount == 0)
          return 0;
        else
        {
          int pageHeight = _treeView.DisplayRectangle.Height - _treeView.ColumnHeaderHeight;
          int y = 0;
          for (int i = _treeView.FirstVisibleRow; i < _treeView.RowCount; i++)
          {
            y += GetRowHeight(i);
            if (y > pageHeight)
              return Math.Max(0, i - _treeView.FirstVisibleRow);
          }
          return Math.Max(0, _treeView.RowCount - _treeView.FirstVisibleRow);
        }
      }
    }

    public Rectangle GetRowBounds(int rowNo)
    {
      if (rowNo >= _rowCache.Count)
      {
        int count = _rowCache.Count;
        int y = count > 0 ? _rowCache[count - 1].Bottom : 0;
        for (int i = count; i <= rowNo; i++)
        {
          int height = GetRowHeight(i);
          _rowCache.Add(new Rectangle(0, y, 0, height));
          y += height;
        }
        if (rowNo < _rowCache.Count - 1)
          return Rectangle.Empty;
      }
      if (rowNo >= 0 && rowNo < _rowCache.Count)
        return _rowCache[rowNo];
      else
        return Rectangle.Empty;
    }

    private int GetRowHeight(int rowNo)
    {
      if (rowNo < _treeView.RowMap.Count)
      {
        TreeNodeAdv node = _treeView.RowMap[rowNo];
        if (node.Height == null)
        {
          int res = 0;
          _measureContext.Font = _treeView.Font;
          foreach (NodeControl nc in _treeView.NodeControls)
          {
            int h = nc.GetActualSize(node, _measureContext).Height;
            if (h > res)
              res = h;
          }
          node.Height = res;
        }
        return node.Height.Value;
      }
      else
        return 0;
    }

    public int GetRowAt(Point point)
    {
      int py = point.Y - _treeView.ColumnHeaderHeight;
      int y = 0;
      for (int i = _treeView.FirstVisibleRow; i < _treeView.RowCount; i++)
      {
        int h = GetRowHeight(i);
        if (py >= y && py < y + h)
          return i;
        else
          y += h;
      }
      return -1;
    }

    public int GetFirstRow(int lastPageRow)
    {
      int pageHeight = _treeView.DisplayRectangle.Height - _treeView.ColumnHeaderHeight;
      int y = 0;
      for (int i = lastPageRow; i >= 0; i--)
      {
        y += GetRowHeight(i);
        if (y > pageHeight)
          return Math.Max(0, i + 1);
      }
      return 0;
    }

    public void ClearCache()
    {
      _rowCache.Clear();
    }
  }
}
