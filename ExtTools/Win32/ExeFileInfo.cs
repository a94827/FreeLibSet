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
      /// Начало данных заголовка (без сигнатуры) в потоке
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
            byte[] buf = _Owner._Reader.ReadBytes(nChars * 2);
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
            uint dOffset = dVA - OptionalHeader._DataDirectories[(int)PEDataDirectoryKind.ResourceTable].Address;
            res._Entries[i]._DataStartPos = rt._StartPos + dOffset;
            res._Entries[i]._DataSize = (int)_Owner._Reader.ReadUInt32();
            if (res._Entries[i]._DataStartPos < 0 || res._Entries[i]._DataStartPos >= _Owner._Stream.Length)
              res._Entries[i]._ErrorMessage = "Начальная позиция ресурса выходит за пределы файла";
            else if ((res._Entries[i]._DataStartPos + res._Entries[i]._DataSize) > _Owner._Stream.Length)
              res._Entries[i]._ErrorMessage = "Конец ресурса выходит за пределы файла";
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

    public enum PEDataDirectoryKind
    {
      ExportTable = 0,
      ImportTable = 1,
      ResourceTable = 2,
      ExceptionTable = 3,
      CertificateTable = 4,
      BaseRelocationTable = 5,
      Debug = 6,
      Architecture = 7,
      GlobalPtr = 8,
      TLSTable = 9,
      LoadConfigTable = 10,
      BoundImport = 11,
      IAT = 12,
      DelayImportDescriptor = 13,
      CLRRuntimeHeader = 14,
      Reserved2 = 15
    }

    public enum PEWindowsSubsystem
    {
      IMAGE_SUBSYSTEM_UNKNOWN = 0,
      IMAGE_SUBSYSTEM_NATIVE = 1,
      IMAGE_SUBSYSTEM_WINDOWS_GUI = 2,
      IMAGE_SUBSYSTEM_WINDOWS_CUI = 3,
      IMAGE_SUBSYSTEM_OS2_CUI = 5,
      IMAGE_SUBSYSTEM_POSIX_CUI = 7,
      IMAGE_SUBSYSTEM_NATIVE_WINDOWS = 8,
      IMAGE_SUBSYSTEM_WINDOWS_CE_GUI = 9,
      IMAGE_SUBSYSTEM_EFI_APPLICATION = 10,
      IMAGE_SUBSYSTEM_EFI_BOOT_SERVICE_DRIVER = 11,
      IMAGE_SUBSYSTEM_EFI_RUNTIME_DRIVER = 12,
      IMAGE_SUBSYSTEM_EFI_ROM = 13,
      IMAGE_SUBSYSTEM_XBOX = 14,
      IMAGE_SUBSYSTEM_WINDOWS_BOOT_APPLICATION = 16,
    }

    [Flags]
    public enum ImageDllCharacteristics
    {
      /// <summary>
      /// Image can handle a high entropy 64-bit virtual address space.
      /// </summary>
      IMAGE_DLLCHARACTERISTICS_HIGH_ENTROPY_VA = 0x0020,

      /// <summary>
      /// DLL can be relocated at load time.
      /// </summary>
      IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE = 0x0040,

      /// <summary>
      /// Code Integrity checks are enforced.
      /// </summary>
      IMAGE_DLLCHARACTERISTICS_FORCE_INTEGRITY = 0x0080,

      /// <summary>
      /// Image is NX compatible.
      /// </summary>
      IMAGE_DLLCHARACTERISTICS_NX_COMPAT = 0x0100,

      /// <summary>
      /// Isolation aware, but do not isolate the image.
      /// </summary>
      IMAGE_DLLCHARACTERISTICS_NO_ISOLATION = 0x0200,

      /// <summary>
      /// Does not use structured exception (SE) handling.No SE handler may be called in this image.
      /// </summary>
      IMAGE_DLLCHARACTERISTICS_NO_SEH = 0x0400,

      /// <summary>
      /// Do not bind the image.
      /// </summary>
      IMAGE_DLLCHARACTERISTICS_NO_BIND = 0x0800,

      /// <summary>
      /// Image must execute in an AppContainer.
      /// </summary>
      IMAGE_DLLCHARACTERISTICS_APPCONTAINER = 0x1000,

      /// <summary>
      /// A WDM driver.
      /// </summary>
      IMAGE_DLLCHARACTERISTICS_WDM_DRIVER = 0x2000,

      /// <summary>
      /// Image supports Control Flow Guard.
      /// </summary>
      IMAGE_DLLCHARACTERISTICS_GUARD_CF = 0x4000,

      /// <summary>
      /// Terminal Server aware.
      /// </summary>
      IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE = 0x8000,
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
      /// 
      /// </summary>
      public uint SizeOfCode { get { return _SizeOfCode; } }
      internal uint _SizeOfCode;

      /// <summary>
      /// 
      /// </summary>
      public uint SizeOfInitializedData { get { return _SizeOfInitializedData; } }
      internal uint _SizeOfInitializedData;

      /// <summary>
      /// 
      /// </summary>
      public uint SizeOfUninitializedData { get { return _SizeOfUninitializedData; } }
      internal uint _SizeOfUninitializedData;

      /// <summary>
      /// Наличие точки запуска
      /// </summary>
      public bool HasEntryPoint { get { return _HasEntryPoint; } }
      internal bool _HasEntryPoint;

      /// <summary>
      /// 
      /// </summary>
      public uint BaseOfCode { get { return _BaseOfCode; } }
      internal uint _BaseOfCode;

      /// <summary>
      /// Только для заголовка PE32, но не PE32+
      /// </summary>
      public uint BaseOfData { get { return _BaseOfData; } }
      internal uint _BaseOfData;

      /// <summary>
      /// 
      /// </summary>
      public ulong ImageBase { get { return _ImageBase; } }
      internal ulong _ImageBase;

      /// <summary>
      /// 
      /// </summary>
      public int SectionAlignment { get { return _SectionAlignment; } }
      internal int _SectionAlignment;

      /// <summary>
      /// 
      /// </summary>
      public int FileAlignment { get { return _FileAlignment; } }
      internal int _FileAlignment;

      /// <summary>
      /// Требуемая операционная система
      /// </summary>
      public Version OSVersion { get { return _OSVersion; } }
      internal Version _OSVersion;

      /// <summary>
      /// 
      /// </summary>
      public Version ImageVersion { get { return _ImageVersion; } }
      internal Version _ImageVersion;

      /// <summary>
      /// 
      /// </summary>
      public Version SubsystemVersion { get { return _SubsystemVersion; } }
      internal Version _SubsystemVersion;


      /// <summary>
      /// 
      /// </summary>
      public uint Win32VersionValue { get { return _Win32VersionValue; } }
      internal uint _Win32VersionValue;

      /// <summary>
      /// 
      /// </summary>
      public uint SizeOfImage { get { return _SizeOfImage; } }
      internal uint _SizeOfImage;

      /// <summary>
      /// 
      /// </summary>
      public uint SizeOfHeaders { get { return _SizeOfHeaders; } }
      internal uint _SizeOfHeaders;

      /// <summary>
      /// 
      /// </summary>
      public uint CheckSum { get { return _CheckSum; } }
      internal uint _CheckSum;


      /// <summary>
      /// 
      /// </summary>
      public PEWindowsSubsystem Subsystem { get { return _Subsystem; } }
      internal PEWindowsSubsystem _Subsystem;

      /// <summary>
      /// 
      /// </summary>
      public ImageDllCharacteristics DllCharacteristics { get { return _DllCharacteristics; } }
      internal ImageDllCharacteristics _DllCharacteristics;

      internal IMAGE_DATA_DIRECTORY[] _DataDirectories;

      /// <summary>
      /// Возвращает true, если exe-файл содержит указанную секцию
      /// </summary>
      /// <param name="kind"></param>
      /// <returns></returns>
      public bool HasTable(PEDataDirectoryKind kind)
      {
        return _DataDirectories[(int)kind].Address != 0L;
      }
    }

    /// <summary>
    /// Флаги для секции
    /// </summary>
    [Flags]
    public enum PESectionFlags
    {
      /// <summary>
      /// The section should not be padded to the next boundary. This flag is obsolete and is replaced by IMAGE_SCN_ALIGN_1BYTES. This is valid only for object files.
      /// </summary>
      IMAGE_SCN_TYPE_NO_PAD = 0x00000008,

      /// <summary>
      /// The section contains executable code.
      /// </summary>
      IMAGE_SCN_CNT_CODE = 0x00000020,

      /// <summary>
      /// The section contains initialized data.
      /// </summary>
      IMAGE_SCN_CNT_INITIALIZED_DATA = 0x00000040,

      /// <summary>
      /// The section contains uninitialized data.
      /// </summary>
      IMAGE_SCN_CNT_UNINITIALIZED_DATA = 0x00000080,

      /// <summary>
      /// Reserved for future use.
      /// </summary>
      IMAGE_SCN_LNK_OTHER = 0x00000100,

      /// <summary>
      /// The section contains comments or other information.The.drectve section has this type.This is valid for object files only.
      /// </summary>
      IMAGE_SCN_LNK_INFO = 0x00000200,

      /// <summary>
      /// The section will not become part of the image.This is valid only for object files.
      /// </summary>
      IMAGE_SCN_LNK_REMOVE = 0x00000800,

      /// <summary>
      /// The section contains COMDAT data.For more information, see COMDAT Sections (Object Only). This is valid only for object files.
      /// </summary>
      IMAGE_SCN_LNK_COMDAT = 0x00001000,

      /// <summary>
      /// The section contains data referenced through the global pointer (GP).
      /// </summary>
      IMAGE_SCN_GPREL = 0x00008000,

      /// <summary>
      /// Reserved for future use.
      /// </summary>
      IMAGE_SCN_MEM_PURGEABLE = 0x00020000,

      /// <summary>
      /// Reserved for future use.
      /// </summary>
      IMAGE_SCN_MEM_16BIT = 0x00020000,

      /// <summary>
      /// Reserved for future use.
      /// </summary>
      IMAGE_SCN_MEM_LOCKED = 0x00040000,

      /// <summary>
      /// Reserved for future use.
      /// </summary>
      IMAGE_SCN_MEM_PRELOAD = 0x00080000,

      /// <summary>
      /// Align data on a 1-byte boundary. Valid only for object files.
      /// </summary>
      IMAGE_SCN_ALIGN_1BYTES = 0x00100000,

      /// <summary>
      /// Align data on a 2-byte boundary. Valid only for object files.
      /// </summary>
      IMAGE_SCN_ALIGN_2BYTES = 0x00200000,

      /// <summary>
      /// Align data on a 4-byte boundary. Valid only for object files.
      /// </summary>
      IMAGE_SCN_ALIGN_4BYTES = 0x00300000,

      /// <summary>
      /// Align data on an 8-byte boundary. Valid only for object files.
      /// </summary>
      IMAGE_SCN_ALIGN_8BYTES = 0x00400000,

      /// <summary>
      /// Align data on a 16-byte boundary. Valid only for object files.
      /// </summary>
      IMAGE_SCN_ALIGN_16BYTES = 0x00500000,

      /// <summary>
      /// Align data on a 32-byte boundary. Valid only for object files.
      /// </summary>
      IMAGE_SCN_ALIGN_32BYTES = 0x00600000,

      /// <summary>
      /// Align data on a 64-byte boundary. Valid only for object files.
      /// </summary>
      IMAGE_SCN_ALIGN_64BYTES = 0x00700000,

      /// <summary>
      /// Align data on a 128-byte boundary. Valid only for object files.
      /// </summary>
      IMAGE_SCN_ALIGN_128BYTES = 0x00800000,

      /// <summary>
      /// Align data on a 256-byte boundary. Valid only for object files.
      /// </summary>
      IMAGE_SCN_ALIGN_256BYTES = 0x00900000,

      /// <summary>
      /// Align data on a 512-byte boundary. Valid only for object files.
      /// </summary>
      IMAGE_SCN_ALIGN_512BYTES = 0x00A00000,

      /// <summary>
      /// Align data on a 1024-byte boundary. Valid only for object files.
      /// </summary>
      IMAGE_SCN_ALIGN_1024BYTES = 0x00B00000,

      /// <summary>
      /// Align data on a 2048-byte boundary. Valid only for object files.
      /// </summary>
      IMAGE_SCN_ALIGN_2048BYTES = 0x00C00000,

      /// <summary>
      /// Align data on a 4096-byte boundary. Valid only for object files.
      /// </summary>
      IMAGE_SCN_ALIGN_4096BYTES = 0x00D00000,

      /// <summary>
      /// Align data on an 8192-byte boundary. Valid only for object files.
      /// </summary>
      IMAGE_SCN_ALIGN_8192BYTES = 0x00E00000,

      /// <summary>
      /// The section contains extended relocations.
      /// </summary>
      IMAGE_SCN_LNK_NRELOC_OVFL = 0x01000000,

      /// <summary>
      /// The section can be discarded as needed.
      /// </summary>
      IMAGE_SCN_MEM_DISCARDABLE = 0x02000000,

      /// <summary>
      /// The section cannot be cached.
      /// </summary>
      IMAGE_SCN_MEM_NOT_CACHED = 0x04000000,

      /// <summary>
      /// The section is not pageable.
      /// </summary>
      IMAGE_SCN_MEM_NOT_PAGED = 0x08000000,

      /// <summary>
      /// The section can be shared in memory.
      /// </summary>
      IMAGE_SCN_MEM_SHARED = 0x10000000,

      /// <summary>
      /// The section can be executed as code.
      /// </summary>
      IMAGE_SCN_MEM_EXECUTE = 0x20000000,

      /// <summary>
      /// The section can be read.
      /// </summary>
      IMAGE_SCN_MEM_READ = 0x40000000,

      /// <summary>
      /// The section can be written to.
      /// </summary>
      IMAGE_SCN_MEM_WRITE = unchecked((int)0x80000000),

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
      /// Размер в памяти
      /// </summary>
      public uint VirtualSize { get { return _VirtualSize; } }
      internal uint _VirtualSize;

      /// <summary>
      /// RVA
      /// </summary>
      public uint VirtualAddress { get { return _VirtualAddress; } }
      internal uint _VirtualAddress;

      /// <summary>
      /// Размер в файле
      /// </summary>
      public uint SizeOfRawData { get { return _SizeOfRawData; } }
      internal uint _SizeOfRawData;

      /// <summary>
      /// Смещение от начала exe-файла
      /// </summary>
      public uint PointerToRawData { get { return _PointerToRawData; } }
      internal uint _PointerToRawData;

      /// <summary>
      /// Флаги секции
      /// </summary>
      public PESectionFlags Flags { get { return _Flags; } }
      internal PESectionFlags _Flags;

      /// <summary>
      /// Выводит свойство <see cref="Name"/>
      /// </summary>
      /// <returns>Текстовое представление</returns>
      public override string ToString()
      {
        return Name;
      }
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
      _PE._Characteristics = (PECharacteristics)(int)(_Reader.ReadUInt16());

      #endregion

      #region Дополнительный заголовок

      if (szOptionalHeader > 0)
      {
        PEOptionalHeader optHead = new PEOptionalHeader();
        _PE._OptionalHeader = optHead;

        optHead._Kind = (PEOptionalHeaderKind)(int)(_Reader.ReadUInt16());
        switch (optHead._Kind)
        {
          case PEOptionalHeaderKind.PE32:
          case PEOptionalHeaderKind.PE32Plus:
            break;
          default:
            throw new InvalidOperationException("Неправильная сигнатура дополнительного заголовка");
        }
        int lvMajor = _Reader.ReadByte();
        int lvMinor = _Reader.ReadByte();
        optHead._LinkerVersion = new Version(lvMajor, lvMinor);
        optHead._SizeOfCode = _Reader.ReadUInt32();
        optHead._SizeOfInitializedData = _Reader.ReadUInt32();
        optHead._SizeOfUninitializedData = _Reader.ReadUInt32();
        long addrEP = _Reader.ReadUInt32(); // AddressOfEntryPoint
        optHead._HasEntryPoint = (addrEP != 0L);
        optHead._BaseOfCode = _Reader.ReadUInt32();

        if (optHead._Kind == PEOptionalHeaderKind.PE32)
        {
          optHead._BaseOfData = _Reader.ReadUInt32();
        }

        // Windows-specific data
        if (optHead.Kind == PEOptionalHeaderKind.PE32Plus)
          optHead._ImageBase = _Reader.ReadUInt64();
        else
          optHead._ImageBase = _Reader.ReadUInt32();
        optHead._SectionAlignment = _Reader.ReadInt32();
        optHead._FileAlignment = _Reader.ReadInt32();
        int osverMajor = _Reader.ReadInt16();
        int osverMinor = _Reader.ReadInt16();
        optHead._OSVersion = new Version(osverMajor, osverMinor);
        int imgverMajor = _Reader.ReadInt16();
        int imgverMinor = _Reader.ReadInt16();
        optHead._ImageVersion = new Version(imgverMajor, imgverMinor);
        int ssverMajor = _Reader.ReadInt16();
        int ssverMinor = _Reader.ReadInt16();
        optHead._SubsystemVersion = new Version(ssverMajor, ssverMinor);
        optHead._Win32VersionValue = _Reader.ReadUInt32();
        optHead._SizeOfImage = _Reader.ReadUInt32();
        optHead._SizeOfHeaders = _Reader.ReadUInt32();
        optHead._CheckSum = _Reader.ReadUInt32();
        optHead._Subsystem = (PEWindowsSubsystem)(int)(_Reader.ReadUInt16());
        optHead._DllCharacteristics = (ImageDllCharacteristics)(int)(_Reader.ReadUInt16());

        _Reader.ReadBytes((optHead.Kind == PEOptionalHeaderKind.PE32 ? 16 : 32));
        _Reader.ReadInt32(); // LoaderFlags
        int numberOfRvaAndSizes = (int)_Reader.ReadUInt32(); // количество входов

        optHead._DataDirectories = new IMAGE_DATA_DIRECTORY[(int)PEDataDirectoryKind.Reserved2 + 1];
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

        sect._VirtualSize = _Reader.ReadUInt32();
        sect._VirtualAddress = _Reader.ReadUInt32();
        sect._SizeOfRawData = _Reader.ReadUInt32();
        sect._PointerToRawData = _Reader.ReadUInt32();
        _Reader.ReadUInt32(); // PointerToRelocations
        _Reader.ReadUInt32(); // PointerToLinenumbers
        _Reader.ReadUInt16(); // NumberOfRelocations
        _Reader.ReadUInt16(); // NumberOfLinenumbers
        sect._Flags = (PESectionFlags)(int)(_Reader.ReadUInt32()); // Characteristics
      }

      #endregion

      #region Заголовок CLR

      InitCLR();

      #endregion
    }

    #region Преобразование адресов RVA->RAW

    // Источник: https://habr.com/ru/articles/129241/

    private struct Aligner
    {
      public const uint FORCED_FILE_ALIGNMENT = 0x200;
      public const uint MIN_SECTION_ALIGNMENT = 0x1000;

      public Aligner(uint fileAlignment, uint sectionAlignement)
      {
        _FileAlignment = fileAlignment;
        _SectionAlignement = sectionAlignement;
      }

      private uint _FileAlignment;
      private uint _SectionAlignement;

      public uint GetVirtualSize(uint size)
      {
        return NeedAlign(_SectionAlignement) ?
            AlignUp(size, _SectionAlignement) :
            size;
      }

      public uint GetVirtualAddress(uint address)
      {
        return NeedAlign(_SectionAlignement) ?
            AlignDown(address, _SectionAlignement) :
            address;
      }

      public uint GetFileOffset(uint offset)
      {
        return NeedAlign(_SectionAlignement) ?
            AlignDown(offset, FORCED_FILE_ALIGNMENT) :
            offset;
      }

      public uint GetSectionSize(PESection sect)
      {
        uint fileSize = sect.SizeOfRawData;
        uint virtualSize = sect.VirtualSize;
        if (NeedAlign(_SectionAlignement))
        {
          fileSize = AlignUp(fileSize, _FileAlignment);
          virtualSize = AlignUp(virtualSize, _SectionAlignement);
        }
        return Math.Min(fileSize, virtualSize);
      }

      private static bool NeedAlign(uint sectionAlignement)
      {
        return sectionAlignement >= MIN_SECTION_ALIGNMENT;
      }

      private static uint AlignDown(uint value, uint factor)
      {
        return value & ~(factor - 1);
      }

      private static uint AlignUp(uint value, uint factor)
      {
        return AlignDown(value - 1, factor) + factor;
      }
    }

    const uint INVALID_RAW = uint.MaxValue;

    private uint RvaToRaw(uint rva)
    {
      uint result = INVALID_RAW;

      if (_PE.OptionalHeader == null)
        throw new BugException("No PE optional header");


      if (rva < _PE.OptionalHeader.SizeOfHeaders)
        return rva;

      Aligner aligner = new Aligner((uint)(_PE.OptionalHeader.FileAlignment), (uint)(_PE.OptionalHeader.SectionAlignment));

      //ulong imageBase = _PE.OptionalHeader.ImageBase;

      if (_PE.Sections.Length > 0)
      {
        foreach (PESection section in _PE.Sections)
        {
          if (section.PointerToRawData == 0)
            continue;

          ulong sectionStart = /*imageBase +*/  aligner.GetVirtualAddress(section.VirtualAddress);
          uint sectionSize = aligner.GetSectionSize(section);
          ulong sectionEnd = sectionStart + sectionSize;

          if (sectionStart <= rva && rva < sectionEnd)
          {
            ulong sectionOffset = aligner.GetFileOffset(section.PointerToRawData);
            checked
            {
              sectionOffset += (ulong)rva - sectionStart;
            }
            if ((long)sectionOffset < _Stream.Length)
            {
              result = (uint)sectionOffset;
              break; // Агеев
            }
          }
        } // for
      }
      else if (rva < aligner.GetVirtualSize(_PE.OptionalHeader.SizeOfImage))
      {
        result = rva;
      }
      return result;
    }

    #endregion

    private void InitCLR()
    {
      if (_PE._OptionalHeader == null)
        return;
      if (!_PE.OptionalHeader.HasTable(PEDataDirectoryKind.CLRRuntimeHeader))
        return;

      uint pos = RvaToRaw(_PE.OptionalHeader._DataDirectories[(int)PEDataDirectoryKind.CLRRuntimeHeader].Address);
      if (pos == INVALID_RAW)
        return;
      _Stream.Position = pos;
      uint headSize = _Reader.ReadUInt32();
      if (headSize < 64)
        return;

      // Есть корректный заголовок CLR
      _CLR = new CLRInfo();

      int rtMajor = _Reader.ReadUInt16();
      int rtMinor = _Reader.ReadUInt16();
      _CLR.Header._RuntimeVersion = new Version(rtMajor, rtMinor);
      _Reader.ReadUInt32(); // RVA of the physical metadata
      _Reader.ReadUInt32(); // size of the physical metadata
      _CLR.Header._Flags = (CLRRuntimeFlags)(_Reader.ReadInt32());

      // Остальные поля

      #region Архитектура

      bool is32bit = (_PE.Characteristics & PECharacteristics.IMAGE_FILE_32BIT_MACHINE) != 0;
      switch (_PE.OptionalHeader.Kind)
      {
        case PEOptionalHeaderKind.PE32:
          _CLR._ProcessorArchitecture = is32bit ? System.Reflection.ProcessorArchitecture.X86 : System.Reflection.ProcessorArchitecture.MSIL;
          break;
        case PEOptionalHeaderKind.PE32Plus:
          if (!is32bit)
            _CLR._ProcessorArchitecture = System.Reflection.ProcessorArchitecture.Amd64;
          break;
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

    #region Информация CLR (NetFramework)

    /// <summary>
    /// Флаги заголовка CLR.
    /// ECMA-335, II.25.3.3.1
    /// </summary>
    [Flags]
    public enum CLRRuntimeFlags
    {
      /// <summary>
      ///  Shall be 1
      /// </summary>
      COMIMAGE_FLAGS_ILONLY = 0x00000001,

      /// <summary>
      /// Image can only be loaded into a 32-bit process, for instance if there are 32-bit vtablefixups, or
      /// casts from native integers to int32. 
      /// CLI implementations that have 64-bit native integers shall refuse loading binaries with this flag set.
      /// </summary>
      COMIMAGE_FLAGS_32BITREQUIRED = 0x00000002,

      /// <summary>
      /// Image has a strong name signature.
      /// </summary>
      COMIMAGE_FLAGS_STRONGNAMESIGNED = 0x00000008,

      /// <summary>
      /// Shall be 0.
      /// </summary>
      COMIMAGE_FLAGS_NATIVE_ENTRYPOINT = 0x00000010,

      /// <summary>
      ///  Should be 0
      /// </summary>
      COMIMAGE_FLAGS_TRACKDEBUGDATA = 0x00010000,
    }

    /// <summary>
    /// Заголовок CLR.
    /// ECMA-335, II.25.3.3
    /// </summary>
    public struct CLRHeader
    {
      /// <summary>
      /// Минимальная версия среды времени выполнения
      /// </summary>
      public Version RuntimeVersion { get { return _RuntimeVersion; } }
      internal Version _RuntimeVersion;

      /// <summary>
      /// Flags describing this runtime image
      /// </summary>
      public CLRRuntimeFlags Flags { get { return _Flags; } }
      internal CLRRuntimeFlags _Flags;
    }

    /// <summary>
    /// Описание CLR
    /// </summary>
    public sealed class CLRInfo
    {
      /// <summary>
      /// Архитектура процессора (вычисляется из флагов PE)
      /// </summary>
      public System.Reflection.ProcessorArchitecture ProcessorArchitecture { get { return _ProcessorArchitecture; } }
      internal System.Reflection.ProcessorArchitecture _ProcessorArchitecture;

      /// <summary>
      /// Заголовок CLR
      /// </summary>
      public CLRHeader Header;
    }

    /// <summary>
    /// Информация CLR (Net Framework).
    /// Если exe-файл не относится к CLR, то ссылка содержит null.
    /// </summary>
    public CLRInfo CLR { get { return _CLR; } }
    private CLRInfo _CLR;

    #endregion

    #region Ресурсы PE (внутреннее представление)

    /// <summary>
    /// Внутрннее иерархическое представление дерева ресурсов
    /// </summary>
    public class PEResourceDirectory : IEnumerable<PEResourceEntry>
    {
      internal PEResourceEntry[] _Entries;

      /// <summary>
      /// Возвращает перечислитель
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
      /// Также возвращает null в случае, если запись указывает неправильное положение ресурса.
      /// В этом случае текст сообщения об ошибке возвращается свойством <see cref="ErrorMessage"/>.
      /// </summary>
      public byte[] ResourceData
      {
        get
        {
          if (_DataStartPos == 0 || _ErrorMessage != null)
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
      /// Текст сообщения об ошибке, если файл содержит ошибку
      /// </summary>
      public string ErrorMessage { get { return _ErrorMessage; } }
      internal string _ErrorMessage;

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

                    rt.Add(typeEntry.ID, nameEntry.ID, cpEntry.CodePage, cpEntry._DataStartPos, cpEntry._DataSize, cpEntry.ErrorMessage);
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

    private static readonly string[] _WantedExts = new string[] { ".exe", ".dll", ".ocx", ".mui", "*.acm", ".ax", ".cpl", ".drv", ".efi", ".mof", ".rll", ".rs", ".scr", ".sys", ".tsp" }; 

    /// <summary>
    /// Возвращает true, если расширение файла соответствует формату MZ.
    /// Регистр символов не учитывается.
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
