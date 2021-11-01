using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
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
  /// <summary>
  /// ���������� ������ � ������� ������.
  /// ������ ������/������ ��������� �� ��������� ������� ��������� � ������ ������, ������ ��� ������ ��������� �� ������.
  /// � ������ ������������ ������ �������� ��������� � ������� MessageBox().
  /// ����� ��������� �������� EFPApp.Clipboard
  /// </summary>
  public sealed class EFPAppClipboard
  {
    #region �����������

    internal EFPAppClipboard()
    {
      _RepeatCount = 10;
      _RepeatDelay = 100;


      // ��������� ������ �� ���������

      // � �������� ������� Net Framework ���:
      //if (Environment.OSVersion.Platform != System.PlatformID.Win32NT ||
      //    Environment.OSVersion.Version.Major < 5)
      //  _DefaultTextFormat = TextDataFormat.Text;
      //else
      //  _DefaultTextFormat = TextDataFormat.UnicodeText;

      // � Mono ������ ������������� TextDataFormat.UnicodeText ������.

      // �����������
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
          if (Environment.OSVersion.Version.Major < 5)
            _DefaultTextFormat = TextDataFormat.Text;
          else
            _DefaultTextFormat = TextDataFormat.UnicodeText;
          break;
        case PlatformID.Win32Windows:
        case PlatformID.WinCE:
        case PlatformID.Win32S:
          _DefaultTextFormat = TextDataFormat.Text;
          break;
        default:
          _DefaultTextFormat = TextDataFormat.UnicodeText;
          break;
      }
    }

    #endregion

    #region ����������� ��������

    /// <summary>
    /// ���������� ������� ��������� �������� � ������� ������, ������ ��� ������ ��������� �� ������.
    /// �� ��������� �������� 10 �������
    /// </summary>
    public int RepeatCount
    {
      get { return _RepeatCount; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException();
        _RepeatCount = value;
      }
    }
    private int _RepeatCount;

    /// <summary>
    /// �������� ������� � ������������� ����� ��������� ��������� �������� � ������� ������.
    /// �������� �� ��������� - 100 ��. ����������� �������� 1.
    /// �������� ����� ����� ��� RepeatCount ������ 1.
    /// </summary>
    public int RepeatDelay
    {
      get { return _RepeatDelay; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException();
        _RepeatDelay = value;
      }
    }
    private int _RepeatDelay;

    #endregion

    #region ������ � ������ � �����

    /// <summary>
    /// ����� ������� GetXXX() � SetXXX() �������� ���������� true, ���� �������� ������ ��� ������ � ������� ������ �
    /// false, ���� �������� ��������� �������.
    /// ������� ������ �� ������������, ���� � ������ ��� ������ � ���������� �������
    /// </summary>
    public bool HasError { get { return _HasError; } }
    private bool _HasError;

    #region Text

    /// <summary>
    /// ���������� TextDataFormat.UnicodeText ��� TextDataFormat.Text, � ����������� �� ������������ �������
    /// </summary>
    public TextDataFormat DefaultTextFormat { get { return _DefaultTextFormat; } }
    private TextDataFormat _DefaultTextFormat;

    /// <summary>
    /// �������� ����� � ����� ������.
    /// ����������� RepeatCount ������� � ��������� RepeatDelau ����� ���������. ����� ��������� ��������� �� ������.
    /// </summary>
    /// <param name="s">���������� �����</param>
    public void SetText(string s)
    {
      EFPApp.BeginWait("����������� ������ � ����� ������", "Copy");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            if (String.IsNullOrEmpty(s))
              Clipboard.Clear();
            else
              Clipboard.SetText(s);
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, "������ ������ � ����� ������");
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// ��������� ����� �� ������ ������.
    /// ����������� RepeatCount ������� � ��������� RepeatDelault ����� ���������. 
    /// ���� ����� ������ ���� ��� �� �������� ������, ��������� �� ������ �� ��������.
    /// </summary>
    /// <returns>����� �� ������ ������ ��� ������ ������</returns>
    public string GetText()
    {
      return GetText(DefaultTextFormat, false);
    }


    /// <summary>
    /// ��������� ����� �� ������ ������.
    /// ����������� RepeatCount ������� � ��������� RepeatDelault ����� ���������. ����� ��������� ��������� �� ������,
    /// ���� <paramref name="messageIfEmpty"/>=true.
    /// </summary>
    /// <param name="messageIfEmpty">���� true, �� ����� �������� ��������� �� ���������� ������ � ������ ������</param>
    /// <returns>����� �� ������ ������ ��� ������ ������</returns>
    public string GetText(bool messageIfEmpty)
    {
      return GetText(DefaultTextFormat, messageIfEmpty);
    }

    /// <summary>
    /// ��������� ����� �� ������ ������.
    /// ����������� RepeatCount ������� � ��������� RepeatDelault ����� ���������. ����� ��������� ��������� �� ������,
    /// ���� <paramref name="messageIfEmpty"/>=true.
    /// </summary>
    /// <param name="format">������ ������</param>
    /// <param name="messageIfEmpty">���� true, �� ����� �������� ��������� �� ���������� ������ � ������ ������</param>
    /// <returns>����� �� ������ ������ ��� ������ ������</returns>
    public string GetText(TextDataFormat format, bool messageIfEmpty)
    {
      string Res = String.Empty;
      EFPApp.BeginWait("���������� ������ �� ������ ������", "Paste");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            Res = Clipboard.GetText(format);
            if (Res == null)
              Res = String.Empty; // �� ������ ������
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, "������ ������ �� ������ ������");
              messageIfEmpty = false;
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }

      if (Res.Length == 0 && messageIfEmpty)
        EFPApp.ErrorMessageBox("����� ������ �� �������� ������");

      return Res;
    }


    /// <summary>
    /// ���������� ������������� ���� ������ �� ������� CSV ��� ��������� �������.
    /// ����������� RepeatCount ������� � ��������� RepeatDelault ����� ���������. 
    /// ������������ ������ � ������� TextDataFormat.CommaSeparatedValue, � ��� ���������� - � TextDataFormat.Text.
    /// ��� ���������� ������ � ������ ������ ��������� �� ������ �� ���������.
    /// ������ ������� ������������ ������ EFPPasteTextMatrixFormat ��� ���������� �������.
    /// </summary>
    /// <returns>������� ������ ��� null, ���� ����� ������ ����, ��� �������� ������</returns>
    public string[,] GetTextMatrix()
    {
      return GetTextMatrix(false);
    }

    /// <summary>
    /// ���������� ������������� ���� ������ �� ������� CSV ��� ��������� �������.
    /// ����������� RepeatCount ������� � ��������� RepeatDelault ����� ���������. ����� ��������� ��������� �� ������,
    /// ���� <paramref name="messageIfEmpty"/>=true.
    /// ������������ ������ � ������� TextDataFormat.CommaSeparatedValue, � ��� ���������� - � TextDataFormat.Text.
    /// ������ ������� ������������ ������ EFPPasteTextMatrixFormat ��� ���������� �������.
    /// </summary>
    /// <param name="messageIfEmpty">���� true, �� ����� �������� ��������� �� ���������� ������ � ������ ������</param>
    /// <returns>������� ������ ��� null, ���� ����� ������ ����, ��� �������� ������</returns>
    public string[,] GetTextMatrix(bool messageIfEmpty)
    {
      #region CSV

      string s = GetText(TextDataFormat.CommaSeparatedValue, false); // �� ������� ���������
      if (HasError)
        return null;

      if (s.Length > 0)
      {
        try
        {
          try
          {
            // ����������� ������ RFC-4180
            return DataTools.CommaStringToArray2(s);
          }
          catch
          {
            // ������ Excel
            return DataTools.CommaStringToArray2(s, ';');
          }
        }
        catch /*(Exception e)*/
        {
          _HasError = true;
          EFPApp.ErrorMessageBox("������ � ������ ������ � ������� CSV ������ ������������� � �������");
          return null;
        }
      }

      #endregion

      #region Text

      s = GetText(DefaultTextFormat, false); // �� ������� ���������
      if (HasError)
        return null;

      if (s.Length > 0)
      {
        try
        {
          return DataTools.TabbedStringToArray2(s);
        }
        catch /*(Exception e)*/
        {
          _HasError = true;
          EFPApp.ErrorMessageBox("����� � ������ ������ ������ ������������� � �������");
          return null;
        }
      }

      #endregion

      if (messageIfEmpty)
        EFPApp.ErrorMessageBox("����� ������ �� �������� ������ ��� ������ � ������� CSV");

      return null;
    }

    #endregion

    #region DataObject

    /// <summary>
    /// �������� ������ � ����� ������.
    /// ����������� RepeatCount ������� � ��������� RepeatDelau ����� ���������. ����� ��������� ��������� �� ������.
    /// </summary>
    /// <param name="data">������</param>
    /// <param name="copy">���� ����� ������� ������� �������������� ����� ���������� ����������</param>
    public void SetDataObject(object data, bool copy)
    {
      EFPApp.BeginWait("����������� ������ � ����� ������", "Copy");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            Clipboard.SetDataObject(data, copy, 1, 0);
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, "������ ������ � ����� ������");
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// ��������� ������ �� ������ ������.
    /// ����������� RepeatCount ������� � ��������� RepeatDelau ����� ���������. ����� ��������� ��������� �� ������.
    /// </summary>
    /// <returns>������ �� ������ ������ ��� null � ������ ������</returns>
    public IDataObject GetDataObject()
    {
      IDataObject res = null;
      EFPApp.BeginWait("���������� ������ �� ������ ������", "Paste");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            res = Clipboard.GetDataObject();
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, "������ ������ �� ������ ������");
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
      return res;
    }

    #endregion

    #region Data

    /// <summary>
    /// �������� ������ � ����� ������.
    /// ����������� RepeatCount ������� � ��������� RepeatDelau ����� ���������. ����� ��������� ��������� �� ������.
    /// </summary>
    /// <param name="format">������ ������</param>
    /// <param name="data">������</param>
    public void SetData(string format, object data)
    {
      EFPApp.BeginWait("����������� ������ � ����� ������", "Copy");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            Clipboard.SetData(format, data);
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, "������ ������ � ����� ������");
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// ��������� ������ �� ������ ������.
    /// ����������� RepeatCount ������� � ��������� RepeatDelau ����� ���������. ����� ��������� ��������� �� ������.
    /// </summary>
    /// <param name="format">��������� ������ ������</param>
    /// <returns>������ �� ������ ������ ��� null � ������ ������ ��� ���� ��� ������ � ����� �������</returns>
    public object GetData(string format)
    {
      object res = null;
      EFPApp.BeginWait("���������� ������ �� ������ ������", "Paste");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            res = Clipboard.GetData(format);
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, "������ ������ �� ������ ������");
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
      return res;
    }

    #endregion

    #region Image

    /// <summary>
    /// �������� ����������� � ����� ������.
    /// ����������� RepeatCount ������� � ��������� RepeatDelau ����� ���������. ����� ��������� ��������� �� ������.
    /// </summary>
    /// <param name="image">�����������</param>
    public void SetImage(Image image)
    {
      EFPApp.BeginWait("����������� ����������� � ����� ������", "Copy");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            Clipboard.SetImage(image);
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, "������ ������ � ����� ������");
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// ��������� ����������� �� ������ ������.
    /// ����������� RepeatCount ������� � ��������� RepeatDelau ����� ���������. ����� ��������� ��������� �� ������.
    /// </summary>
    /// <returns>������ �� ������ ������ ��� null � ������ ������ ��� ���� ��� ������ � ����� �������</returns>
    public Image GetImage()
    {
      Image res = null;
      EFPApp.BeginWait("���������� ����������� �� ������ ������", "Paste");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            res = Clipboard.GetImage();
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, "������ ������ �� ������ ������");
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
      return res;
    }

    #endregion

    #endregion
  }
}