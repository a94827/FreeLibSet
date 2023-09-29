using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FreeLibSet.Core;
using FreeLibSet.IO;

namespace FreeLibSet.Win32
{

  /// <summary>
  /// Извлечение данных из выполняемого файла.
  /// </summary>
  public sealed class ExeFileInfo : IDisposable
  {
    #region Конструкторы и Dispose()

    /// <summary>
    /// Создает объект на основании потока.
    /// Поток остается собственностью вызывающего кода или передается в собственность <see cref="ExeFileInfo"/>. Поток должен существовать все время работы с <see cref="ExeFileInfo"/>.
    /// Поток должен поддерживать произвольное позиционирование.
    /// </summary>
    /// <param name="stream">Открытый поток</param>
    /// <param name="ownStream">Если true, то <see cref="ExeFileInfo"/> становится владедьцем потока</param>
    public ExeFileInfo(Stream stream, bool ownStream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");
      if (!(stream.CanRead && stream.CanSeek))
        throw new ArgumentException("Unsupported stream", "stream");
      _Stream = stream;
      _Reader = new BinaryReader(_Stream);
      _OwnStream = ownStream;
    }

    /// <summary>
    /// Открывает на просмотр указанный файл и создает объект на основании потока
    /// </summary>
    /// <param name="path">Путь к файлу</param>
    public ExeFileInfo(AbsPath path)
      : this(CreateFileStream(path), true)
    {
    }

    private static Stream CreateFileStream(AbsPath path)
    {
      if (path.IsEmpty)
        throw new ArgumentException("Путь не задан", "path");
      return new FileStream(path.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    /// <summary>
    /// Закрывает файловый поток, если <see cref="ExeFileInfo"/> владеет им
    /// </summary>
    public void Dispose()
    {
      if (_OwnStream)
      {
        _OwnStream = false;
        _Stream.Dispose();
      }
    }

    private Stream _Stream;
    private bool _OwnStream;
    private BinaryReader _Reader;

    #endregion

    #region Основной и дополнительный заголовок PE

#pragma warning disable 1591

    public enum PEMachine
    {
      IMAGE_FILE_MACHINE_UNKNOWN = 0x0,
      IMAGE_FILE_MACHINE_ALPHA = 0x184,
      IMAGE_FILE_MACHINE_ALPHA64 = 0x284,
      IMAGE_FILE_MACHINE_AM33 = 0x1d3,
      IMAGE_FILE_MACHINE_AMD64 = 0x8664,
      IMAGE_FILE_MACHINE_ARM = 0x1c0,
      IMAGE_FILE_MACHINE_ARM64 = 0xaa64,
      IMAGE_FILE_MACHINE_ARMNT = 0x1c4,
      IMAGE_FILE_MACHINE_AXP64 = 0x284,
      IMAGE_FILE_MACHINE_EBC = 0xebc,
      IMAGE_FILE_MACHINE_I386 = 0x14c,
      IMAGE_FILE_MACHINE_IA64 = 0x200,
      IMAGE_FILE_MACHINE_LOONGARCH32 = 0x6232,
      IMAGE_FILE_MACHINE_LOONGARCH64 = 0x6264,
      IMAGE_FILE_MACHINE_M32R = 0x9041,
      IMAGE_FILE_MACHINE_MIPS16 = 0x266,
      IMAGE_FILE_MACHINE_MIPSFPU = 0x366,
      IMAGE_FILE_MACHINE_MIPSFPU16 = 0x466,
      IMAGE_FILE_MACHINE_POWERPC = 0x1f0,
      IMAGE_FILE_MACHINE_POWERPCFP = 0x1f1,
      IMAGE_FILE_MACHINE_R4000 = 0x166,
      IMAGE_FILE_MACHINE_RISCV32 = 0x5032,
      IMAGE_FILE_MACHINE_RISCV64 = 0x5064,
      IMAGE_FILE_MACHINE_RISCV128 = 0x5128,
      IMAGE_FILE_MACHINE_SH3 = 0x1a2,
      IMAGE_FILE_MACHINE_SH3DSP = 0x1a3,
      IMAGE_FILE_MACHINE_SH4 = 0x1a6,
      IMAGE_FILE_MACHINE_SH5 = 0x1a8,
      IMAGE_FILE_MACHINE_THUMB = 0x1c2,
      IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x169,
    }

    [Flags]
    public enum PECharacteristics
    {
      IMAGE_FILE_RELOCS_STRIPPED = 0x0001,
      IMAGE_FILE_EXECUTABLE_IMAGE = 0x0002,
      IMAGE_FILE_LINE_NUMS_STRIPPED = 0x0004,
      IMAGE_FILE_LOCAL_SYMS_STRIPPED = 0x0008,
      IMAGE_FILE_AGGRESSIVE_WS_TRIM = 0x0010,
      IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x0020,
      IMAGE_FILE_BYTES_REVERSED_LO = 0x0080,
      IMAGE_FILE_32BIT_MACHINE = 0x0100,
      IMAGE_FILE_DEBUG_STRIPPED = 0x0200,
      IMAGE_FILE_REMOVABLE_RUN_FROM_SWAP = 0x0400,
      IMAGE_FILE_NET_RUN_FROM_SWAP = 0x0800,
      IMAGE_FILE_SYSTEM = 0x1000,
      IMAGE_FILE_DLL = 0x2000,
      IMAGE_FILE_UP_SYSTEM_ONLY = 0x4000,
      IMAGE_FILE_BYTES_REVERSED_HI = 0x8000,
    }

#pragma warning restore 1591

    /// <summary>
    /// Заголовок PE-файла (Win32 exe/dll)
    /// </summary>
    public class PEFileHeader
    {
      internal ExeFileInfo _Owner;

      /// <summary>
      /// Начало данных заголовка (без сингнатуры) в потоке
      /// </summary>
      internal long _StartPos;

      /// <summary>
      /// Тип целевой архитектуры
      /// </summary>
      public PEMachine Machine { get { return _Machine; } }
      internal PEMachine _Machine;

      /// <summary>
      /// Время создания файла
      /// </summary>
      public DateTime CreationTime { get { return _CreationTime; } }
      internal DateTime _CreationTime;

      /// <summary>
      /// Характеристики (массив флагов)
      /// </summary>
      public PECharacteristics Characteristics { get { return _Characteristics; } }
      internal PECharacteristics _Characteristics;

      /// <summary>
      /// Дополнительный заголовок.
      /// Должен присутствовать в EXE и DLL-файлах
      /// </summary>
      public PEOptionalHeader OptionalHeader { get { return _OptionalHeader; } }
      internal PEOptionalHeader _OptionalHeader;

      /// <summary>
      /// Описания секций
      /// </summary>
      public PESection[] Sections { get { return _Sections; } }
      internal PESection[] _Sections;

      /// <summary>
      /// Внутренняя таблица ресурсов в виде дерева.
      /// Рекомендуется использовать основное свойство <see cref="ExeFileInfo.Resources"/>, предоставляющее более удобный доступ к ресурсам.
      /// </summary>
      public PEResourceTree Resources
      {
        get
        {
          if (!_ResourcesDefined)
          {
            _ResourcesDefined = true;
            _Resources = InitResources();
          }
          return _Resources;
        }
      }
      private PEResourceTree _Resources;
      private bool _ResourcesDefined;

      private PEResourceTree InitResources()
      {
        PESection sect = null;
        for (int i = 0; i < Sections.Length; i++)
        {
          if (String.Equals(Sections[i].Name, ".rsrc", StringComparison.Ordinal))
          {
            sect = Sections[i];
            break;
          }
        }
        if (sect == null)
          return null;

        PEResourceTree rt = new PEResourceTree();
        rt._Owner = _Owner;
        //rt._StartPos = 0/*_StartPos*/ -
        //  OptionalHeader._BaseOfCode + 
        //  OptionalHeader._DataDirectories[(int)(PEDataDirectotyKind.ResourceTable)].Address;
        rt._StartPos = sect.PointerToRawData;
        if (rt._StartPos < 0 || rt._StartPos >= _Owner._Stream.Length)
          throw new BugException("Начало таблицы ресурсов выходит за пределы файла");

        DoInitResourceDirectory(rt, rt, 0L);

        return rt;
      }

      private void DoInitResourceDirectory(PEResourceTree rt, PEResourceDirectory res, long offset)
      {
        const uint Bit31Mask = 0x80000000u;
        const uint OtherBitMask = 0x7FFFFFFFu;

        _Owner._Stream.Seek(rt._StartPos + offset, SeekOrigin.Begin);

        // Чтение IMAGE_RESOURCE_DIRECTORY
        uint characterisics = _Owner._Reader.ReadUInt32(); // Characterisics
        uint timeDateStamp = _Owner._Reader.ReadUInt32(); // TimeDateStamp
        int majorVersion = _Owner._Reader.ReadUInt16(); // MajorVersion
        int minorVersion = _Owner._Reader.ReadUInt16(); // MinorVersion

        const int IMAGE_RESOURCE_DIRECTORY_SIZE = 16;
        const int IMAGE_RESOURCE_DIRECTORY_ENTRY_SIZE = 8;


        int numberOfNamedEntries = _Owner._Reader.ReadUInt16();
        int numberOfIdEntries = _Owner._Reader.ReadUInt16();
        res._Entries = new PEResourceEntry[numberOfNamedEntries + numberOfIdEntries];
        for (int i = 0; i < res._Entries.Length; i++)
        {
          _Owner._Stream.Seek(rt._StartPos + offset + IMAGE_RESOURCE_DIRECTORY_SIZE + i * IMAGE_RESOURCE_DIRECTORY_ENTRY_SIZE, SeekOrigin.Begin);

          res._Entries[i] = new PEResourceEntry();
          res._Entries[i]._Owner = rt._Owner;

          uint eName = _Owner._Reader.ReadUInt32();
          uint eOffset = _Owner._Reader.ReadUInt32();

          if ((eName & Bit31Mask) != 0)
          {
            _Owner._Stream.Seek(rt._StartPos + (eName & OtherBitMask), SeekOrigin.Begin);
            // Чтение структуры IMAGE_RESOURCE_DIR_STRING_U
            int nChars = _Owner._Reader.ReadUInt16();
            byte[] buf = _Owner._Reader.ReadBytes(nChars*2);
            res._Entries[i]._ID = new ResourceID(Encoding.Unicode.GetString(buf));
          }
          else
            res._Entries[i]._ID = new ResourceID((int)(eName & OtherBitMask));

          if ((eOffset & Bit31Mask) != 0)
          {
            // Рекурсивный вызов
            PEResourceDirectory child = new PEResourceDirectory();
            DoInitResourceDirectory(rt, child, (int)(eOffset & OtherBitMask));
            res._Entries[i]._Child = child;
          }
          else
          {
            _Owner._Stream.Seek(rt._StartPos + (eOffset & OtherBitMask), SeekOrigin.Begin);

            // Чтение структуры IMAGE_RESOURCE_DATA_ENTRY 
            uint dVA = _Owner._Reader.ReadUInt32(); // Виртуальный адрес, из него нужно вычесть начало таблицы
            uint dOffset = dVA - OptionalHeader._DataDirectories[(int)PEDataDirectotyKind.ResourceTable].Address;
            res._Entries[i]._DataStartPos = rt._StartPos + dOffset;
            if (res._Entries[i]._DataStartPos < 0 || res._Entries[i]._DataStartPos >= _Owner._Stream.Length)
              throw new BugException("Начальная позиция ресурса выходит за пределы файла");
            res._Entries[i]._DataSize = (int)_Owner._Reader.ReadUInt32();
            if ((res._Entries[i]._DataStartPos + res._Entries[i]._DataSize)> _Owner._Stream.Length)
              throw new BugException("Конец ресурса выходит за пределы файла");
            res._Entries[i]._CodePage = (int)_Owner._Reader.ReadUInt32();
            _Owner._Reader.ReadUInt32(); // Reserved
          }
        }
      }
    }

    /// <summary>
    /// Тип дополнительного заголовка
    /// </summary>
    public enum PEOptionalHeaderKind
    {
      /// <summary>
      /// PE32
      /// </summary>
      PE32 = 0x10b,

      /// <summary>
      /// PE32+
      /// </summary>
      PE32Plus = 0x20b,
    }

    internal struct IMAGE_DATA_DIRECTORY
    {
      public UInt32 Address;
      public UInt32 Size;
    }

#pragma warning disable 1591

    public enum PEDataDirectotyKind
    {
      ExportTable = 0,
      ImportTable = 1,
      ResourceTable = 2,
      ExceptionTable = 3,
      CertificateTable = 4,
      BaseRelocationTable = 5,
      Debug = 6,
      Architecture = 7,
      Reserved1 = 8,
      GlobalPtr = 9,
      TLSTable = 10,
      LoadConfigTable = 11,
      BoundImport = 12,
      IAT = 13,
      DelayImportDescriptor = 14,
      CLRRuntimeHeader = 15,
      Reserved2 = 16
    }

#pragma warning restore 1591


    /// <summary>
    /// Дополнительный заголовок PE32 или PE32+
    /// </summary>
    public class PEOptionalHeader
    {
      /// <summary>
      /// Тип заголовка: PE32 или PE32+
      /// </summary>
      public PEOptionalHeaderKind Kind { get { return _Kind; } }
      internal PEOptionalHeaderKind _Kind;

      /// <summary>
      /// Версия линкера
      /// </summary>
      public Version LinkerVersion { get { return _LinkerVersion; } }
      internal Version _LinkerVersion;

      /// <summary>
      /// Наличие точки запуска
      /// </summary>
      public bool HasEntryPoint { get { return _HasEntryPoint; } }
      internal bool _HasEntryPoint;

      /// <summary>
      /// 
      /// </summary>
      public IntPtr BaseOfCode { get { return new IntPtr((long)_BaseOfCode); } }
      internal uint _BaseOfCode;

      internal IMAGE_DATA_DIRECTORY[] _DataDirectories;

      /// <summary>
      /// Возвращает true, если exe-файл содержит указанную секцию
      /// </summary>
      /// <param name="kind"></param>
      /// <returns></returns>
      public bool HasTable(PEDataDirectotyKind kind)
      {
        return _DataDirectories[(int)kind].Address != 0L;
      }
    }

    /// <summary>
    /// Описание секции
    /// </summary>
    public class PESection
    {
      /// <summary>
      /// Имя секции, например ".rsrc"
      /// </summary>
      public string Name { get { return _Name; } }
      internal string _Name;

      /// <summary>
      /// 
      /// </summary>
      public uint SizeOfRawData { get { return _SizeOfRawData; } }
      internal uint _SizeOfRawData;

      /// <summary>
      /// 
      /// </summary>
      public uint PointerToRawData { get { return _PointerToRawData; } }
      internal uint _PointerToRawData;
    }

    /// <summary>
    /// Заголовок PE-файла.
    /// Если файл не является PE, возвращает null
    /// </summary>
    public PEFileHeader PE
    {
      get
      {
        if (!_PEDefined)
          InitPE();
        return _PE;
      }
    }

    private void InitPE()
    {
      _PEDefined = true;

      #region Основной заголовок PE

      // Проверка сигнатуры MZ
      _Stream.Position = 0;
      if (_Reader.ReadUInt16() != 0x5A4D)
        return;
      // Указатель на заголовок PE
      _Stream.Position = 0x3c;
      long pePos = _Reader.ReadUInt32();

      // Проверка сигнатуры PE
      _Stream.Position = pePos;
      if (_Reader.ReadUInt32() != 0x00004550) // "PE\0\0"
        return;

      _PE = new PEFileHeader();
      _PE._Owner = this;
      _PE._StartPos = _Stream.Position;
      _PE._Machine = (PEMachine)(_Reader.ReadUInt16());
      int numberOfSections = _Reader.ReadUInt16();

      long dateTimeStamp = _Reader.ReadUInt32();
      TimeSpan ts = new TimeSpan(TimeSpan.TicksPerSecond * dateTimeStamp);
      _PE._CreationTime = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc) + ts;

      _Reader.ReadUInt32(); // PointerToSymbolTable
      _Reader.ReadUInt32(); // NumberOfSymbols
      int szOptionalHeader = _Reader.ReadUInt16(); // SizeOfOptionalHeader
      _PE._Characteristics = (PECharacteristics)(_Reader.ReadUInt16());

      #endregion

      #region Дополнительный заголовок

      if (szOptionalHeader > 0)
      {
        PEOptionalHeader optHead = new PEOptionalHeader();
        _PE._OptionalHeader = optHead;

        optHead._Kind = (PEOptionalHeaderKind)(_Reader.ReadUInt16());
        switch (optHead._Kind)
        {
          case PEOptionalHeaderKind.PE32:
          case PEOptionalHeaderKind.PE32Plus:
            break;
          default:
            throw new Exception("Неправильная сигнатура дополнительного заголовка");
        }
        int lvMajor = _Reader.ReadByte();
        int lvMinor = _Reader.ReadByte();
        optHead._LinkerVersion = new Version(lvMajor, lvMinor);
        _Reader.ReadUInt32(); // SizeOfCode
        _Reader.ReadUInt32(); // SizeOfInitializedData
        _Reader.ReadUInt32(); // SizeOfUninitializedData
        long addrEP = _Reader.ReadUInt32(); // AddressOfEntryPoint
        optHead._HasEntryPoint = (addrEP != 0L);
        optHead._BaseOfCode = _Reader.ReadUInt32();

        if (optHead._Kind == PEOptionalHeaderKind.PE32)
        {
          _Reader.ReadUInt32(); // BaseOfData
        }

        // Windows-specific data
        _Reader.ReadBytes((optHead.Kind == PEOptionalHeaderKind.PE32 ? 68 : 88) - 4);
        int numberOfRvaAndSizes = (int)_Reader.ReadUInt32(); // количество входов

        optHead._DataDirectories = new IMAGE_DATA_DIRECTORY[(int)PEDataDirectotyKind.Reserved2 + 1];
        int n = Math.Min(optHead._DataDirectories.Length, numberOfRvaAndSizes);
        for (int i = 0; i < n; i++)
        {
          optHead._DataDirectories[i].Address = _Reader.ReadUInt32(); // виртуальный адрес, по которому будет загружена секция
          optHead._DataDirectories[i].Size = _Reader.ReadUInt32();
        }
      }

      #endregion

      #region Секции

      _PE._Sections = new PESection[numberOfSections];
      for (int i = 0; i < numberOfSections; i++)
      {
        PESection sect = new PESection();
        _PE.Sections[i] = sect;

        byte[] bName = _Reader.ReadBytes(8);
        sect._Name = GetSectionName(bName);

        _Reader.ReadUInt32(); // PhysicalAddress
        _Reader.ReadUInt32(); // VirtualAddress
        sect._SizeOfRawData = _Reader.ReadUInt32();
        sect._PointerToRawData = _Reader.ReadUInt32();
        _Reader.ReadUInt32(); // PointerToRelocations
        _Reader.ReadUInt32(); // PointerToLinenumbers
        _Reader.ReadUInt16(); // NumberOfRelocations
        _Reader.ReadUInt16(); // NumberOfLinenumbers
        _Reader.ReadUInt32(); // Characteristics
      }

      #endregion
    }

    private static string GetSectionName(byte[] b)
    {
      StringBuilder sb = new StringBuilder(b.Length);
      for (int i = 0; i < b.Length; i++)
      {
        if (b[i] == 0)
          break;
        char c = Convert.ToChar(b[i]);
        sb.Append(c);
      }
      return sb.ToString();
    }

    private PEFileHeader _PE;
    private bool _PEDefined;

    #endregion

    #region Ресурсы PE (внутреннее представление)

    /// <summary>
    /// Внутрннее иерархическое представление дерева ресурсов
    /// </summary>
    public class PEResourceDirectory : IEnumerable<PEResourceEntry>
    {
      internal PEResourceEntry[] _Entries;

      /// <summary>
      /// Возращает перечислитель
      /// </summary>
      /// <returns></returns>
      public ArrayEnumerable<PEResourceEntry>.Enumerator GetEnumerator()
      {
        return new ArrayEnumerable<PEResourceEntry>(_Entries).GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      IEnumerator<PEResourceEntry> IEnumerable<PEResourceEntry>.GetEnumerator()
      {
        return GetEnumerator();
      }

      /// <summary>
      /// Возвращает количество элементов на данном уровне
      /// </summary>
      public int Count { get { return _Entries.Length; } }

      /// <summary>
      /// Доступ к элементу по индексу
      /// </summary>
      /// <param name="index"></param>
      /// <returns></returns>
      public PEResourceEntry this[int index] { get { return _Entries[index]; } }
    }

    /// <summary>
    /// Один элемент (узел) дерева ресурсов.
    /// Ссылается на дочерний подкаталог ресурсов или непосредственно на ресурс
    /// </summary>
    public class PEResourceEntry
    {
      /// <summary>
      /// Имя или числовой идентификатор ресурса
      /// </summary>
      public ResourceID ID { get { return _ID; } }
      internal ResourceID _ID;

      /// <summary>
      /// Дочерние узлы, если текущий узел является подкаталогом.
      /// Null, если узел ссылается на ресурс.
      /// </summary>
      public PEResourceDirectory Child { get { return _Child; } }
      internal PEResourceDirectory _Child;

      /// <summary>
      /// Данные ресурса или null, если узел является подкаталогом.
      /// </summary>
      public byte[] ResourceData
      {
        get
        {
          if (_DataStartPos == 0)
            return null;
          else
          {
            _Owner._Stream.Seek(_DataStartPos, SeekOrigin.Begin);
            return _Owner._Reader.ReadBytes(_DataSize);
          }
        }
      }

      internal ExeFileInfo _Owner;

      /// <summary>
      /// Положение данных относительно потока или 0, если это подкаталог
      /// </summary>
      internal long _DataStartPos;

      /// <summary>
      /// Размер данных ресурса, если узел является ссылкой на ресурс.
      /// </summary>
      public int DataSize { get { return _DataSize; } }
      internal int _DataSize;

      /// <summary>
      /// Кодовая страница ресурса.
      /// </summary>
      public int CodePage { get { return _CodePage; } }
      internal int _CodePage;

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString()
      {
        return ID.ToString();
      }
    }

    /// <summary>
    /// Дерево ресурсов
    /// </summary>
    public class PEResourceTree : PEResourceDirectory
    {
      internal ExeFileInfo _Owner;
      /// <summary>
      /// Смещение от начала потока
      /// </summary>
      internal long _StartPos;
    }

    #endregion

    #region Таблица ресурсов

    private class ExtResourceTable : ResourceTable
    {
      internal ExeFileInfo _Owner;

      public override byte[] GetBytes(CPInfo cpi)
      {
        _Owner._Stream.Seek(cpi.Offset, SeekOrigin.Begin);
        return _Owner._Reader.ReadBytes(cpi.Size);
      }
    }

    /// <summary>
    /// Таблица ресурсов.
    /// Если файл не содержит ресурсов, то таблица пустая.
    /// </summary>
    public ResourceTable Resources
    {
      get
      {
        if (_Resources == null)
          _Resources = CreateResourceTable();
        return _Resources;
      }
    }

    private ExtResourceTable _Resources;

    private ExtResourceTable CreateResourceTable()
    {
      ExtResourceTable rt = new ExtResourceTable();
      rt._Owner = this;
      if (PE != null)
      {
        if (PE.Resources != null)
        {
          foreach (PEResourceEntry typeEntry in PE.Resources)
          {
            if (typeEntry.Child != null)
            {
              foreach (PEResourceEntry nameEntry in typeEntry.Child)
              {
                if (nameEntry.Child != null)
                {
                  foreach (PEResourceEntry cpEntry in nameEntry.Child)
                  {
                    if (cpEntry.DataSize == 0)
                      continue;

                    rt.Add(typeEntry.ID, nameEntry.ID, cpEntry.CodePage, cpEntry._DataStartPos, cpEntry._DataSize);
                  }
                }
              }
            }
          }
        }
      }
      return rt;
    }

    #endregion

    #region Статические методы

    private static readonly string[] _WantedExts = new string[] { ".exe", ".dll" }; // TODO: Расширить список

    /// <summary>
    /// Возвращает true, если расширение файла соответствует формату MZ.
    /// Регистр символов не учитывается
    /// </summary>
    /// <param name="extension">Проверяемое расширение, включая ведущую точку</param>
    /// <returns></returns>
    public static bool IsExeExtension(string extension)
    {
      if (String.IsNullOrEmpty(extension))
        return false;

      for (int i = 0; i < _WantedExts.Length; i++)
      {
        if (String.Equals(extension, _WantedExts[i], StringComparison.OrdinalIgnoreCase))
          return true;
      }

      return false;
    }

    #endregion
  }
}
