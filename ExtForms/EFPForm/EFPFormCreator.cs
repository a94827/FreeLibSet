using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;

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
  /// ��������� ��� ������ ������ ���������� IEFPCormCreator.CreateForm()
  /// </summary>
  public sealed class EFPFormCreatorParams
  {
    #region ���������� �����������

    internal EFPFormCreatorParams(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      _Config = cfg;
      _ClassName = cfg.GetString("Class");
      _ConfigSectionName = cfg.GetString("ConfigSectionName");
      _Title = cfg.GetString("Title");
    }

    #endregion

    #region ��������

    /// <summary>
    /// �������� ���� Class
    /// </summary>
    public string ClassName { get { return _ClassName; } }
    private string _ClassName;

    /// <summary>
    /// �������� ���� ConfigName
    /// </summary>
    public string ConfigSectionName { get { return _ConfigSectionName; } }
    private string _ConfigSectionName;

    /// <summary>
    /// ��� ������ ��� ����������� ����� �� ������ ������������� "Composition"-"UI"
    /// </summary>
    public CfgPart Config { get { return _Config; } }
    private CfgPart _Config;

    /// <summary>
    /// ��������� �����
    /// </summary>
    public string Title { get { return _Title; } }
    private string _Title;

    /// <summary>
    /// ��������� ������������� ���������� ��������� ����
    /// ��� "[ ��� ��������� ]"
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      if (String.IsNullOrEmpty(Title))
        return "[ ��� ��������� ]";
      else
        return Title;
    }

    #endregion
  }

  /// <summary>
  /// �������, ����������� ���������, ������ ���� ��������� � ������ EFPApp.FormCreators
  /// </summary>
  public interface IEFPFormCreator
  {
    /// <summary>
    /// ������� ����� �� �������� ����������.
    /// ����� ���������� ��������� ����� ��� null, ���� ���������� ClassName �� �������������� ���� ��������.
    /// ���� ����� �� ����� ���� ������� �� ������ ��������, ���������, ��-�� ���������� � ������������ ����,
    /// ������ ���� ��������� ����������� ����������.
    /// </summary>
    /// <param name="creatorParams">������ ���������� ��� �������� �����</param>
    /// <returns>��������� ����� ��� null</returns>
    Form CreateForm(EFPFormCreatorParams creatorParams);
  }
}