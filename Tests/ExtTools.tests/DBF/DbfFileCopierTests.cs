using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using FreeLibSet.DBF;
using FreeLibSet.IO;
// TODO:
#if XXX

namespace ExtTools_tests.DBF
{
  [TestFixture]
  public class DbfFileCopierTests
  {
    #region Вспомогательные объекты

    private struct DbfFileWrapper:IDisposable
    {
      #region Конструкторы

      public void CreateExist(string format, bool isReadOnly)
      {
        _Dir = new TempDirectory();
        _PathDbf = new AbsPath(_Dir.Dir, format + ".dbf");
        bool hasMemo = format.EndsWith("m");
        if (hasMemo)
          _PathDbt = new AbsPath(_Dir.Dir, format + ".dbt");
        else
          _PathDbt = AbsPath.Empty;

        // Загружаем файл из ресурсов
        System.IO.File.WriteAllBytes(_PathDbf.Path, (byte[])(DbfResource.ResourceManager.GetObject(format + "_dbf")));
        if (hasMemo)
          System.IO.File.WriteAllBytes(_PathDbt.Path, (byte[])(DbfResource.ResourceManager.GetObject(format + "_dbt")));
        _File = new DbfFile(_PathDbf, System.Text.Encoding.GetEncoding(1251), isReadOnly);
      }

      public void CreateNew(string format)
      {
        CreateNew(format, Encoding.GetEncoding(1251));
      }

      public void CreateNew(string format, Encoding encoding)
      {
        _Dir = new TempDirectory();
        _PathDbf = new AbsPath(_Dir.Dir, format + ".dbf");
        bool hasMemo = format.EndsWith("m");
        if (hasMemo)
          _PathDbt = new AbsPath(_Dir.Dir, format + ".dbt");
        else
          _PathDbt = AbsPath.Empty;

        System.IO.MemoryStream ms=new System.IO.MemoryStream();
        byte[] b=(byte[])(DbfResource.ResourceManager.GetObject(format + "_dbf"));
        ms.Write(b,0, b.Length);
        ms.Position = 0;
        DbfFile dummy=new DbfFile(ms, null, Encoding.GetEncoding(1251), true);
        DbfStruct dbs = dummy.DBStruct;

        // Загружаем файл из ресурсов
        System.IO.File.WriteAllBytes(_PathDbf.Path, (byte[])(DbfResource.ResourceManager.GetObject(format + "_dbf")));
        if (hasMemo)
          System.IO.File.WriteAllBytes(_PathDbt.Path, (byte[])(DbfResource.ResourceManager.GetObject(format + "_dbt")));
        _File = new DbfFile(_PathDbf, System.Text.Encoding.GetEncoding(1251), isReadOnly);
      }

      #endregion

      #region Свойства

      public DbfFile File { get { return _File; } }
      private DbfFile _File;

      private TempDirectory _Dir;

      public AbsPath PathDbf { get { return _PathDbf; } }
      private AbsPath _PathDbf;

      public AbsPath PathDbt { get { return _PathDbt; } }
      private AbsPath _PathDbt;

      #endregion

      #region IDisposable Members

      public void Dispose()
      {
        throw new NotImplementedException();
      }

      #endregion
    }

    #endregion

    #region Конструктор

    public void Constructor()
    {
      //DbfFileCopier sut = new DbfFileCopier();
    }

    #endregion
  }
}
#endif