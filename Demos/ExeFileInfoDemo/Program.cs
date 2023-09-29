using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FreeLibSet.Forms;
using System.Drawing;
using FreeLibSet.IO;
using FreeLibSet.Win32;
using System.IO;
using FreeLibSet.Core;
using System.Drawing.Imaging;
using FreeLibSet.Controls;
using System.Text;

namespace ExeFileInfoDemo
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      EFPApp.InitApp();
      try
      {
        FolderBrowserDialog dlg = new FolderBrowserDialog();
        dlg.Description = "Каталог с файлами *.exe и *.dll";
        dlg.ShowNewFolderButton = false;
        if (EFPApp.ShowDialog(dlg) != DialogResult.OK)
          return;

        List<MyFileInfo> lst = new List<MyFileInfo>();

        using (Splash spl = new Splash("Просмотр файлов"))
        {
          string[] aFiles1 = System.IO.Directory.GetFiles(dlg.SelectedPath, "*.exe", SearchOption.TopDirectoryOnly);
          string[] aFiles2 = System.IO.Directory.GetFiles(dlg.SelectedPath, "*.dll", SearchOption.TopDirectoryOnly);
          string[] aFiles = DataTools.MergeArrays<string>(aFiles1, aFiles2);
          Array.Sort<string>(aFiles);
          spl.PercentMax = aFiles.Length;
          spl.AllowCancel = true;
          for (int i = 0; i < aFiles.Length; i++)
          {
            AbsPath filePath = new AbsPath(aFiles[i]);
            spl.PhaseText = filePath.FileName;
            lst.Add(new MyFileInfo(filePath));
            spl.IncPercent();
          }
        }

        SimpleGridForm form = new SimpleGridForm();
        form.Text = dlg.SelectedPath;
        form.StartPosition = FormStartPosition.WindowsDefaultBounds;
        EFPDataGridView efpGrid = new EFPDataGridView(form.ControlWithToolBar);
        efpGrid.Control.AutoGenerateColumns = true;
        efpGrid.Control.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        efpGrid.Control.DataSource = lst;
        efpGrid.DisableOrdering();
        efpGrid.Control.ReadOnly = true;
        efpGrid.ReadOnly = false;
        efpGrid.CanView = false;
        efpGrid.CanInsert = false;
        efpGrid.CanDelete = false;
        efpGrid.EditData += new EventHandler(efpGrid_EditData);

        Application.Run(form);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка запуска программы");
      }
    }

    static void efpGrid_EditData(object sender, EventArgs args)
    {
      EFPDataGridView efpGrid = (EFPDataGridView)sender;
      if (!efpGrid.CheckSingleRow())
        return;

      MyFileInfo fi = efpGrid.CurrentGridRow.DataBoundItem as MyFileInfo;
      if (fi == null)
      {
        EFPApp.ErrorMessageBox("Упс!");
        return;
      }
      ShowSingleFileInfo(fi.FilePath);
    }

    /// <summary>
    /// Показ информации об одном файле
    /// </summary>
    /// <param name="filePath"></param>
    private static void ShowSingleFileInfo(AbsPath filePath)
    {
      Form form = new Form();
      form.Text = filePath.FileName;
      form.StartPosition = FormStartPosition.WindowsDefaultBounds;
      EFPFormProvider efpForm = new EFPFormProvider(form);
      EFPTabControl efpTC = new EFPTabControl(efpForm);

      #region Вкладка "Resource"

      EFPTabPage efpTPRes = efpTC.TabPages.Add("Resources");
      EFPControlWithToolBar<TreeView> cwtRes = new EFPControlWithToolBar<TreeView>(efpTPRes);
      EFPTreeView efpRes = new EFPTreeView(cwtRes);
      efpRes.Control.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(efpRes_NodeMouseDoubleClick);

      #endregion

      #region Вкладка "Icons"

      EFPTabPage efpTPIcons = efpTC.TabPages.Add("Icons");
      EFPControlWithToolBar<DataGridView> cwtIcons = new EFPControlWithToolBar<DataGridView>(efpTPIcons);
      EFPDataGridView efpIcons = new EFPDataGridView(cwtIcons);
      efpIcons.Control.AutoGenerateColumns = true;
      efpIcons.DisableOrdering();
      efpIcons.Control.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
      efpIcons.Control.ReadOnly = true;
      efpIcons.ReadOnly = false;
      efpIcons.CanView = false;

      #endregion

      #region Вкладка "ExtractIcon"

      EFPTabPage efpTPExtIcons = efpTC.TabPages.Add("ExtractIcon");
      EFPControlWithToolBar<DataGridView> cwtExtIcons = new EFPControlWithToolBar<DataGridView>(efpTPExtIcons);
      EFPDataGridView efpExtIcons = new EFPDataGridView(cwtExtIcons);
      efpExtIcons.Control.AutoGenerateColumns = true;
      efpExtIcons.DisableOrdering();
      efpExtIcons.Control.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
      efpExtIcons.Control.ReadOnly = true;
      efpExtIcons.ReadOnly = false;
      efpExtIcons.CanView = false;


      #endregion

      List<MyIconInfo> lstIcons = new List<MyIconInfo>();
      List<IconExtractInfo> lstExtractInfo = new List<IconExtractInfo>();

      using (ExeFileInfo fi = new ExeFileInfo(filePath))
      {
        foreach (KeyValuePair<ResourceID, ResourceTable.TypeInfo> pair in fi.Resources.Types)
        {
          TreeNode tNode = efpRes.Control.Nodes.Add(pair.Value.ToString());
          foreach (ResourceTable.NameInfo ni in pair.Value)
          {
            TreeNode nNode = tNode.Nodes.Add(ni.Name.ToString());
            foreach (ResourceTable.CPInfo cpi in ni)
            {
              TreeNode cpNode = nNode.Nodes.Add("CodePage=" + cpi.CodePage.ToString() + ", Size=" + cpi.Size);
              cpNode.Tag = fi.Resources.GetBytes(cpi);
            }
          }

          if (pair.Value.ResourceType == ResourceType.Icon)
          {
            foreach (ResourceTable.NameInfo ni in pair.Value)
            {
              foreach (ResourceTable.CPInfo cpi in ni)
              {
                Image iconImage = null;
                ResourceTable.IconInfo iconInfo;
                try
                {
                  byte[] b = fi.Resources.GetSingleImageIconBytes(cpi, out iconInfo);
                  MemoryStream ms = new MemoryStream(b);
                  Icon ic = new Icon(ms);
                  iconImage = ic.ToBitmap();
                }
                catch
                {
                  iconInfo = new ResourceTable.IconInfo();
                }
                lstIcons.Add(new MyIconInfo(iconImage, ni.Name, cpi.CodePage, iconInfo));
              }
            }
          }
        }
        for (int i = 1; i < 1000; i++)
        {
          Image img1 = null;
          Image img2 = null;
          try
          {
            byte[] b1 = fi.Resources.ExtractIconBytes(i, false);
            byte[] b2 = fi.Resources.ExtractIconBytes(i, true);
            if (b1 == null)
              break;

            MemoryStream ms1 = new MemoryStream(b1);
            Icon ic1 = new Icon(ms1);
            img1 = ic1.ToBitmap();

            MemoryStream ms2 = new MemoryStream(b2);
            Icon ic2 = new Icon(ms2);
            img2 = ic2.ToBitmap();
          }
          catch { }
          lstExtractInfo.Add(new IconExtractInfo(i, img1, img2));
        }

      } // using
      efpIcons.Control.DataSource = lstIcons;
      efpExtIcons.Control.DataSource = lstExtractInfo;


      EFPApp.ShowDialog(form, true);
    }

    static void efpRes_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs args)
    {
      byte[] b = args.Node.Tag as byte[];
      if (b == null)
        return;
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < b.Length; i += 16)
      {
        sb.Append(i.ToString("X4"));
        sb.Append(" | ");
        int n = Math.Min(b.Length - i, 16);
        for (int j = 0; j < n; j++)
        {
          sb.Append(b[i + j].ToString("X2"));
          if (j == 7)
            sb.Append(" ");
          sb.Append(" ");
        }
        sb.Append(Environment.NewLine);
      }
      EFPApp.ShowTextView(sb.ToString(), "Просмотр");
    }
  }

  /// <summary>
  /// Отображаемая информация в списке файлов
  /// </summary>
  public class MyFileInfo
  {
    #region Конструктор

    public MyFileInfo(AbsPath filePath)
    {
      _FilePath = filePath;

      try
      {
        using (ExeFileInfo fi = new ExeFileInfo(filePath))
        {
          if (fi.PE == null)
          {
            _Description = "Не является PE-файлом";
            return;
          }

          foreach (KeyValuePair<ResourceID, ResourceTable.TypeInfo> pair in fi.Resources.Types)
          {
            foreach (ResourceTable.NameInfo ni in pair.Value)
            {
              foreach (ResourceTable.CPInfo cpi in ni)
              {
                switch (pair.Value.ResourceType)
                {
                  case ResourceType.Icon: _IconCount++; break;
                  case ResourceType.Bitmap: _BitmapCount++; break;
                  case ResourceType.Version: _VersionCount++; break;
                  case ResourceType.Manifest: _ManifestCount++; break;
                  default: _OtherCount++; break;
                }
              }
            }
          }
          _IconCount = fi.Resources.Types[ResourceType.Icon].Count;
          if (_IconCount > 0)
          {
            byte[] b = fi.Resources.GetIconBytes();
            MemoryStream ms = new MemoryStream(b);
            Icon ic = new Icon(ms);
            _IconImage = ic.ToBitmap();
          }
        }
      }
      catch (Exception e)
      {
        _Description = "*** Ошибка ***." + e.Message;
      }
    }

    #endregion

    #region Отображаемые Свойства

    public string FileName { get { return _FilePath.FileName; } }

    public Image IconImage { get { return _IconImage; } }
    private Image _IconImage;

    public int IconCount { get { return _IconCount; } }
    private int _IconCount;

    public int BitmapCount { get { return _BitmapCount; } }
    private int _BitmapCount;

    public int VersionCount { get { return _VersionCount; } }
    private int _VersionCount;

    public int ManifestCount { get { return _ManifestCount; } }
    private int _ManifestCount;

    public int OtherCount { get { return _OtherCount; } }
    private int _OtherCount;

    public string Description { get { return _Description; } }
    private string _Description;

    #endregion

    #region Прочие свойства

    internal AbsPath FilePath { get { return _FilePath; } }
    private AbsPath _FilePath;

    #endregion
  }

  public class MyIconInfo
  {
    public MyIconInfo(Image iconImage, ResourceID resourceId, int localeId, ResourceTable.IconInfo iconInfo)
    {
      _IconImage = iconImage;
      _ResoureId = resourceId;
      _LocaleId = localeId;
      _IconInfo = iconInfo;
    }

    public Image IconImage { get { return _IconImage; } }
    private Image _IconImage;

    public ResourceID ResoureId { get { return _ResoureId; } }
    private ResourceID _ResoureId;

    public int LocaleId { get { return _LocaleId; } }
    private int _LocaleId;

    private ResourceTable.IconInfo _IconInfo;

    public int Width { get { return _IconInfo.Width; } }
    public int Height { get { return _IconInfo.Height; } }
    public int BitsPerPixel { get { return _IconInfo.BPP; } }
  }

  public class IconExtractInfo
  {
    public IconExtractInfo(int iconIndex, Image largeIcon, Image smallIcon)
    {
      _IconIndex = iconIndex;
      _LargeIcon = largeIcon;
      _SmallIcon = smallIcon;
    }

    public int IconIndex { get { return _IconIndex; } }
    private int _IconIndex;

    public Image LargeIcon { get { return _LargeIcon; } }
    private Image _LargeIcon;

    public Image SmallIcon { get { return _SmallIcon; } }
    private Image _SmallIcon;
  }
}