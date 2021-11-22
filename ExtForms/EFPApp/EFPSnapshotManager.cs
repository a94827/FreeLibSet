// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using FreeLibSet.Config;

namespace FreeLibSet.Forms
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
