using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using AgeyevAV.Config;

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

namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// ��������� ���������� ����������� ���������������� ��������� ���������� ����������.
  /// ������������ ��������� EFPApp.SnapshotManager
  /// </summary>
  public interface IEFPSnapshotManager
  {
    /// <summary>
    /// ��������� �����������
    /// </summary>
    /// <param name="configInfo">������������� �����������</param>
    /// <param name="snapshot">����������� �����������. ����� ���� null</param>
    void SaveSnapshot(EFPConfigSectionInfo configInfo, Bitmap snapshot);

    /// <summary>
    /// ��������� �����������.
    /// ���� ����������� �� ���� ���������, ����� ������ ������� null
    /// </summary>
    /// <param name="configInfo">������������� �����������</param>
    /// <returns>����������� ��� null</returns>
    Bitmap LoadSnapshot(EFPConfigSectionInfo configInfo);
  }

  /// <summary>
  /// ���������� ���������� ���������� �����������, ������������ �� ���������.
  /// ������ ����������� � ������ ������������ � ���� ��������� ������ � ��������� Base64
  /// </summary>
  public class EFPSnapshotConfigManager : IEFPSnapshotManager
  {
    #region �����������

    /// <summary>
    /// ������� ��������
    /// </summary>
    public EFPSnapshotConfigManager()
    {
      _ImageFormat = System.Drawing.Imaging.ImageFormat.Gif;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ������ ������ ����������� ����� ��������������� � Base64.
    /// �� ��������� - GIF
    /// </summary>
    public System.Drawing.Imaging.ImageFormat ImageFormat
    {
      get { return _ImageFormat; }
      set { _ImageFormat = value; }
    }
    private System.Drawing.Imaging.ImageFormat _ImageFormat;

    #endregion

    #region IEFPSnapshotManager Members

    /// <summary>
    /// ��������� �����������.
    /// ������������������ ����� �������� ������ ������������ ������� EFPApp.ConfigManager.GetConfig().
    /// ����������� ������������� � ������ � ��������� Base64.
    /// ������ ������������ � ������ ������������ ��� ��������� �������� "Snapshot".
    /// ���� <paramref name="snapshot"/>=null, �� �������� "Snapshot" ��������� �� ������ ������������.
    /// </summary>
    /// <param name="configInfo">��� � ��������� ������ ������������</param>
    /// <param name="snapshot">�����������. ����� ���� null</param>
    public virtual void SaveSnapshot(EFPConfigSectionInfo configInfo, Bitmap snapshot)
    {
      CfgPart cfg;
      using (EFPApp.ConfigManager.GetConfig(configInfo, EFPConfigMode.Write, out cfg))
      {
        if (snapshot == null)
          cfg.Remove("Snapshot");
        else
        {
          using (System.IO.MemoryStream strm = new System.IO.MemoryStream())
          {
            snapshot.Save(strm, System.Drawing.Imaging.ImageFormat.Gif);
            //string s = Convert.ToBase64String(strm.GetBuffer());
            string s = Convert.ToBase64String(strm.ToArray()); // 20.11.2020
            cfg.SetString("Snapshot", s);
          }
        }
      }
    }

    /// <summary>
    /// ��������� �����������.
    /// ������������������ ����� �������� ������ ������������ ������� EFPApp.ConfigManager.GetConfig().
    /// �� ������ �������� ��������� �������� "Snapshot".
    /// ���� �������� ��� ��� ��� �������� ������ �������, ������������ null.
    /// ����� ������ ������������� � �����������, �����������, ��� ��� �������� � ��������� Base64.
    /// � ������ ������ �������������� ������������ null ��� ������� ����������.
    /// </summary>
    /// <param name="configInfo">��� � ��������� ������ ������������</param>
    /// <returns>����������� ����������� ��� null</returns>
    public virtual Bitmap LoadSnapshot(EFPConfigSectionInfo configInfo)
    {
      Bitmap Bmp = null;
      CfgPart cfg;
      using (EFPApp.ConfigManager.GetConfig(configInfo, EFPConfigMode.Read, out cfg))
      {
        string sSnapshot = cfg.GetString("Snapshot");
        if (!String.IsNullOrEmpty(sSnapshot))
        {
          try
          {
            byte[] b = Convert.FromBase64String(sSnapshot);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(b))
            {
              Bmp = Image.FromStream(ms) as Bitmap;
            }
          }
          catch //(Exception e)
          {
          }
        }
      }
      return Bmp;
    }

    #endregion
  }
}
