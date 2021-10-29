using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
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

/*
 * Original TreeViewAdv component from Aga.Controls.dll
 * http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
 * http://sourceforge.net/projects/treeviewadv/
 */

#pragma warning disable 1591

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  public abstract class BindableControl : NodeControl
  {
    private struct MemberAdapter
    {
      private object _obj;
      private PropertyInfo _pi;
      private FieldInfo _fi;

      public static readonly MemberAdapter Empty = new MemberAdapter();

      public Type MemberType
      {
        get
        {
          if (_pi != null)
            return _pi.PropertyType;
          else if (_fi != null)
            return _fi.FieldType;
          else
            return null;
        }
      }

      public object Value
      {
        get
        {
          if (_pi != null && _pi.CanRead)
            return _pi.GetValue(_obj, null);
          else if (_fi != null)
            return _fi.GetValue(_obj);
          else
            return null;
        }
        set
        {
          if (_pi != null && _pi.CanWrite)
            _pi.SetValue(_obj, value, null);
          else if (_fi != null)
            _fi.SetValue(_obj, value);
        }
      }

      public MemberAdapter(object obj, PropertyInfo pi)
      {
        _obj = obj;
        _pi = pi;
        _fi = null;
      }

      public MemberAdapter(object obj, FieldInfo fi)
      {
        _obj = obj;
        _fi = fi;
        _pi = null;
      }
    }

    #region Properties

    private bool _virtualMode = false;
    [DefaultValue(false), Category("Data")]
    public bool VirtualMode
    {
      get { return _virtualMode; }
      set { _virtualMode = value; }
    }

    private string _propertyName = "";
    [DefaultValue(""), Category("Data")]
    public string DataPropertyName
    {
      get { return _propertyName; }
      set
      {
        //if (_propertyName == null)
        if (value == null) // 23.04.2021 Исправил Агеев А.В.
          _propertyName = string.Empty;
        else
          _propertyName = value;
      }
    }

    private bool _incrementalSearchEnabled = false;
    [DefaultValue(false)]
    public bool IncrementalSearchEnabled
    {
      get { return _incrementalSearchEnabled; }
      set { _incrementalSearchEnabled = value; }
    }

    #endregion

    public virtual object GetValue(TreeNodeAdv node)
    {
      if (VirtualMode)
      {
        NodeControlValueEventArgs args = new NodeControlValueEventArgs(node);
        OnValueNeeded(args);
        return args.Value;
      }
      else
      {
        try
        {
          return GetMemberAdapter(node).Value;
        }
        catch (TargetInvocationException ex)
        {
          if (ex.InnerException != null)
            throw new ArgumentException(ex.InnerException.Message, ex.InnerException);
          else
            throw new ArgumentException(ex.Message);
        }
      }
    }

    public virtual void SetValue(TreeNodeAdv node, object value)
    {
      if (VirtualMode)
      {
        NodeControlValueEventArgs args = new NodeControlValueEventArgs(node);
        args.Value = value;
        OnValuePushed(args);
      }
      else
      {
        try
        {
          MemberAdapter ma = GetMemberAdapter(node);
          ma.Value = value;
        }
        catch (TargetInvocationException ex)
        {
          if (ex.InnerException != null)
            throw new ArgumentException(ex.InnerException.Message, ex.InnerException);
          else
            throw new ArgumentException(ex.Message);
        }
      }
    }

    public Type GetPropertyType(TreeNodeAdv node)
    {
      return GetMemberAdapter(node).MemberType;
    }

    private MemberAdapter GetMemberAdapter(TreeNodeAdv node)
    {
      if (node.Tag != null && !string.IsNullOrEmpty(DataPropertyName))
      {
        Type type = node.Tag.GetType();
        PropertyInfo pi = type.GetProperty(DataPropertyName);
        if (pi != null)
          return new MemberAdapter(node.Tag, pi);
        else
        {
          FieldInfo fi = type.GetField(DataPropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
          if (fi != null)
            return new MemberAdapter(node.Tag, fi);
        }
      }
      return MemberAdapter.Empty;
    }

    public override string ToString()
    {
      if (string.IsNullOrEmpty(DataPropertyName))
        return GetType().Name;
      else
        return string.Format("{0} ({1})", GetType().Name, DataPropertyName);
    }

    public event EventHandler<NodeControlValueEventArgs> ValueNeeded;
    private void OnValueNeeded(NodeControlValueEventArgs args)
    {
      if (ValueNeeded != null)
        ValueNeeded(this, args);
    }

    public event EventHandler<NodeControlValueEventArgs> ValuePushed;
    private void OnValuePushed(NodeControlValueEventArgs args)
    {
      if (ValuePushed != null)
        ValuePushed(this, args);
    }
  }
}
