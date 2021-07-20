using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Globalization;
using System.Runtime.Serialization;

namespace AgeyevAV
{
  /// <summary>
  /// Описание для одного поля DBF-таблицы
  /// </summary>
  [Serializable]
  public struct DbfFieldInfo
  {
    #region Конструкторы

    public DbfFieldInfo(string Name, char Type, int Length, int Precision)
    {
#if DEBUG
      if (!IsValidFieldName(Name))
        throw new ArgumentException("Неправильное имя DBF-поля: \"" + Name + "\"");
#endif
      FName = Name;
      FType = Type;
      FLength = Length;
      FPrecision = Precision;
    }

    public DbfFieldInfo(string Name, DbfFieldInfo OtherInfo)
    {
#if DEBUG
      if (!IsValidFieldName(Name))
        throw new ArgumentException("Неправильное имя DBF-поля: \"" + Name + "\"");
#endif
      FName = Name;
      FType = OtherInfo.Type;
      FLength = OtherInfo.Length;
      FPrecision = OtherInfo.Precision;
    }

    public static DbfFieldInfo CreateString(string Name, int Length)
    {
      return new DbfFieldInfo(Name, 'C', Length, 0);
    }

    public static DbfFieldInfo CreateDate(string Name)
    {
      return new DbfFieldInfo(Name, 'D', 8, 0);
    }

    public static DbfFieldInfo CreateNum(string Name, int Length)
    {
      return new DbfFieldInfo(Name, 'N', Length, 0);
    }

    public static DbfFieldInfo CreateNum(string Name, int Length, int Precision)
    {
      return new DbfFieldInfo(Name, 'N', Length, Precision);
    }

    public static DbfFieldInfo CreateBool(string Name)
    {
      return new DbfFieldInfo(Name, 'L', 1, 0);
    }

    public static DbfFieldInfo CreateMemo(string Name)
    {
      return new DbfFieldInfo(Name, 'M', 10, 0);
    }

    #endregion

    #region Основные свойства

    public string Name { get { return FName; } }
    private string FName;

    public char Type { get { return FType; } }
    private char FType;

    public int Length { get { return FLength; } }
    private int FLength;

    public int Precision { get { return FPrecision; } }
    private int FPrecision;

    #endregion

    #region Дополнительные свойства

    public bool IsEmpty { get { return String.IsNullOrEmpty(FName); } }

    public string TypeText
    {
      get
      {
        switch (FType)
        {
          case 'C':
            return "Строковый";
          case 'N':
            return "Числовой";
          case 'L':
            return "Логический";
          case 'D':
            return "Дата";
          case 'M':
            return "Мемо";
          case '\0':
            return string.Empty;
          default:
            return "Неизвестный";
        }
      }
    }

    public string TypeSizeText
    {
      get
      {
        string s = TypeText;
        switch (FType)
        {
          case 'C':
            return s + " (" + FLength.ToString() + ")";
          case 'N':
            if (FPrecision == 0)
              return s + " (" + FLength.ToString() + ")";
            else
              return s + " (" + FLength.ToString() + "," + FPrecision.ToString() + ")";
          default:
            return s;
        }
      }
    }

    public override string ToString()
    {
      if (IsEmpty)
        return "[пусто]";
      return Name + " (" + TypeSizeText + ")";
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Проверка корректности одиночного имени поля DBF-таблицы
    /// Возвращает true, если выполняются следующие условия:
    /// 1. Имя содержит от 1 до 10 знаков
    /// 2. Имя состоит только из заглавных латинских букв, цифр и знака "_"
    /// 3. Имя не начинается с цифры
    /// </summary>
    /// <param name="FieldName">Имя поля</param>
    /// <returns>true, если имя поля является корректным</returns>
    public static bool IsValidFieldName(string FieldName)
    {
      const string ValidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ_0123456789";
      if (String.IsNullOrEmpty(FieldName))
        return false;
      if (FieldName.Length < 1 || FieldName.Length > 10)
        return false;
      for (int i = 0; i < FieldName.Length; i++)
      {
        if (ValidChars.IndexOf(FieldName[i]) < 0)
          return false;
        if (i == 0 && FieldName[0] >= '0' && FieldName[0] <= '9')
          return false;
      }
      return true;
    }

    #endregion
  }

  /// <summary>
  /// Структура полей DBF-таблицы
  /// </summary>
  [Serializable]
  public class DbfStruct : IEnumerable<DbfFieldInfo>
  {
    #region Конструктор

    public DbfStruct()
    {
      FItems = new List<DbfFieldInfo>();
    }

    #endregion

    #region Свойства

    private List<DbfFieldInfo> FItems;

    public DbfFieldInfo this[int Index]
    {
      get { return FItems[Index]; }
    }

    public DbfFieldInfo this[string Name]
    {
      get
      {
        int p = IndexOf(Name);
        if (p >= 0)
          return FItems[p];
        else
          return new DbfFieldInfo();
      }
    }

    public int Count { get { return FItems.Count; } }

    #endregion

    #region Методы

    public int IndexOf(string Name)
    {
      for (int i = 0; i < FItems.Count; i++)
      {
        if (FItems[i].Name == Name)
          return i;
      }
      return -1;
    }

    public void Add(DbfFieldInfo Item)
    {
      CheckNotReadOnly();

      if (Item.IsEmpty)
        throw new ArgumentException("Item is empty", "Item");

      FItems.Add(Item);
    }

    public override string ToString()
    {
      return "Count=" + FItems.Count.ToString() + (ReadOnly ? " ReadOnly" : "");
    }

    #endregion

    #region Свойство ReadOnly

    public bool ReadOnly { get { return FReadOnly; } }
    private bool FReadOnly;

    public void SetReadOnly()
    {
      FReadOnly = true;
    }

    public void CheckNotReadOnly()
    {
      if (ReadOnly)
        throw new InvalidOperationException("Список полей находится в режиме ReadOnly");
    }

    #endregion

    #region IEnumerable<DbfFieldInfo> Members

    public IEnumerator<DbfFieldInfo> GetEnumerator()
    {
      return FItems.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return FItems.GetEnumerator();
    }

    #endregion
  }

#if XXX
  /// <summary>
  /// Чтение и запись DBF-файла версии 3 по одной строке
  /// </summary>
  public class DbfReaderWriter:DisposableObject
  {
  #region Конструктор и Dispose

    private DbfReaderWriter(DbfStruct Struct)
    {
      FStruct = Struct;
      FStruct.SetReadOnly();

      FieldOffs = new int[FStruct.Count];
      int len = 1; // 0-маркер удаленной строки
      for (int i = 0; i < FieldOffs.Length; i++)
      {
        FieldOffs[i] = len;
        len += Struct[i].Length;
      }
    }

    protected override void Dispose(bool Disposing)
    {
      if (FDbtStream!=null)
      {
        if (!ReadOnly)
          FDbtStream.Flush();
        if (DisposeStreams)
          FDbtStream.Dispose();
        FDbtStream = null;
      }
      if (FDbfStream != null)
      {
        if (!ReadOnly)
          FDbfStream.Flush();
        if (DisposeStreams)
          FDbfStream.Dispose();
        FDbfStream = null;
      }

      base.Dispose(Disposing);
    }


  #endregion

  #region Методы создания

    //public static DbfReaderWriter Create()

  #endregion

  #region Структура

    public DbfStruct Struct { get { return FStruct; } }
    private DbfStruct FStruct;

    /// <summary>
    /// Смещения начала каждого поля
    /// </summary>
    private int[] FieldOffs;

  #endregion

  #region Буфер текущей строки

    public Encoding Encoding { get { return FEncoding; } }
    private Encoding FEncoding;

    /// <summary>
    /// Буфер строки
    /// </summary>
    private byte[] StrBuf;
           /*
    public object this[int FieldIndex]
    {
      get
      { 
      }
    }        */

  #endregion

  #region Потоки

    /// <summary>
    /// Возвращает true, если разрешено только чтение данных, а не запись
    /// </summary>
    public bool ReadOnly { get { return FReadOnly; } }
    private bool FReadOnly;

    /// <summary>
    /// Если true, то потоки должны быть разрушены вместе с этим объектом
    /// </summary>
    private bool DisposeStreams;

    private Stream FDbfStream;

    private Stream FDbtStream;

  #endregion
  }
#endif

  /// <summary>
  /// Класс исключения, генерируемого при нарушении формата DBF-файла
  /// </summary>
  [Serializable]
  public class DbfFileFormatException : Exception
  {
    #region Конструктор

    public DbfFileFormatException(string Message)
      : base(Message)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected DbfFileFormatException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

#if XXX
  /// <summary>
  /// Класс для одностороннего чтения DBF файла
  /// </summary>
  public class DbfReader : DisposableObject
  {
    // Использование:
    // using(DbfReader rdr=new DbfReader("c:\\test.dbf"))
    // {
    //   while (rdr.Read())
    //   {
    //     string s=rdr.GetString("Code");
    //     int x=rdr.GetInt(2);
    //   }
    // }

    #region Конструкторы и Dispose

    public DbfReader(string FilePath)
      : this(FilePath, DefaultEncoding)
    {
    }

    public DbfReader(string FilePath, Encoding Encoding)
    {
      if (!File.Exists(FilePath))
        throw new FileNotFoundException("Файл не найден: \"" + FilePath + "\"");

      fsDBF = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
      try
      {
        if (fsDBF.Length < 1)
          throw new DbfFileFormatException("Файл \"" + FilePath + "\" имеет нулевую длину");

        // Определяем наличие МЕМО-файла
        int Code = fsDBF.ReadByte();
        fsDBF.Position = 0; // обязательно возвращаем на начало
        if (Code == 0x83)
        {
          string DBTFilePath = Path.ChangeExtension(FilePath, ".DBT");
          if (!File.Exists(DBTFilePath))
            throw new FileNotFoundException("МЕМО-файл не найден: \"" + DBTFilePath + "\"");

          fsDBT = new FileStream(DBTFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        rdrDBF = new BinaryReader(fsDBF);
        if (fsDBT != null)
          rdrDBT = new BinaryReader(fsDBT);

        Init(rdrDBF, rdrDBT, Encoding);
      }
      catch
      {
        DisposeStreams();
        throw;
      }
    }

    public DbfReader(Stream DbfStream)
      : this(DbfStream, (Stream)null, DefaultEncoding)
    {
    }

    public DbfReader(Stream DbfStream, Stream DbtStream)
      : this(DbfStream, DbtStream, DefaultEncoding)
    {
    }

    public DbfReader(Stream DbfStream, Stream DbtStream, Encoding Encoding)
    {
      if (DbfStream == null)
        throw new ArgumentNullException("DbfStream");
      if (Encoding == null)
        throw new ArgumentNullException("Encoding");

      ShouldDisposeRdrs = true;

      rdrDBF = new BinaryReader(fsDBF);
      if (fsDBT != null)
        rdrDBT = new BinaryReader(fsDBT);

      Init(rdrDBF, rdrDBT, Encoding);
    }

    public DbfReader(BinaryReader DbfReader)
      : this(DbfReader, (BinaryReader)null, DefaultEncoding)
    {
    }

    public DbfReader(BinaryReader DbfReader, BinaryReader DbtReader)
      : this(DbfReader, DbtReader, DefaultEncoding)
    {
    }

    public DbfReader(BinaryReader DbfReader, BinaryReader DbtReader, Encoding Encoding)
    {
      ShouldDisposeRdrs = false;
      Init(DbfReader, DbtReader, Encoding);
    }

    private void Init(BinaryReader DbfReader, BinaryReader DbtReader, Encoding Encoding)
    {
      #region Инициализация общих свойств

      if (Encoding == null)
        Encoding = DefaultEncoding;
      FEncoding = Encoding;

      if (DbfReader == null)
        throw new ArgumentNullException("DbfReader");

      rdrDBF = DbfReader;
      rdrDBT = DbtReader;
      SkipDeleted = true;

      #endregion

      #region 1. Читаем заголовок

      bool HasMemoFile;
      // Идентификатор DBF-файла
      int DbfId = rdrDBF.ReadByte();
      switch (DbfId)
      {
        case 0x03:
          HasMemoFile = false;
          break;
        case 0x83:
          HasMemoFile = true;
          break;
        default:
          throw new DbfFileFormatException("Не DBF-файл");
      }

      // Метка даты
      rdrDBF.ReadByte();
      rdrDBF.ReadByte();
      rdrDBF.ReadByte();

      // Число записей
      LastRec = rdrDBF.ReadUInt32();
      // Смещение до начала данных
      DataOffset = rdrDBF.ReadUInt16();
      // Размер записи
      RecSize = rdrDBF.ReadUInt16();
      if (RecSize < 1)
        throw new DbfFileFormatException("Размер записи не может быть равен 0");

      long WantedSize = DataOffset + (long)RecSize * (long)LastRec;
      if (rdrDBF.BaseStream.Length < WantedSize)
        throw new DbfFileFormatException("Длина файла (" + WantedSize.ToString() + ") не соответствует размеру и числу записей, заданных в заголовке. Длина файла должна быть не меньше, чем " + WantedSize.ToString());

      // Заполнитель
      rdrDBF.ReadBytes(20);

      #endregion

      #region 2. Читаем список полей

      int RealRecSize = 1; // Место для признака удаленной строки
      byte[] bFldName = new byte[11];
      FDBStruct = new DbfStruct();
      while (true)
      {
        if (rdrDBF.Read(bFldName, 0, 1) < 1)
          throw new DbfFileFormatException("Неожиданный конец файла. Список полей не закончен");

        if (bFldName[0] == 0x0D)
          break; // Конец списка полей

        // Имя поля
        if (rdrDBF.Read(bFldName, 1, 10) < 10)
          throw new DbfFileFormatException("Неожиданный конец файла. Список полей не закончен");

        int EndPos = Array.IndexOf<byte>(bFldName, 0);
        if (EndPos < 0)
          throw new DbfFileFormatException("В списке полей для поля " + (DBStruct.Count + 1).ToString() + " не найден нулевой байт окончания имени поля");

        if (EndPos == 0)
          throw new DbfFileFormatException("В списке полей для поля " + (DBStruct.Count + 1).ToString() + " не задано имя поля");

        string FieldName = Encoding.GetString(bFldName, 0, EndPos);
        // Тип поля
        byte[] bFldType = rdrDBF.ReadBytes(1);
        string FieldType = Encoding.GetString(bFldType);

        rdrDBF.ReadBytes(4); // пропуск

        int Len;
        int Prec;
        if (FieldType == "C")
        {
          Len = rdrDBF.ReadUInt16();
          Prec = 0;
        }
        else
        {
          Len = rdrDBF.ReadByte();
          Prec = rdrDBF.ReadByte();
        }

        rdrDBF.ReadBytes(14);

        switch (FieldType)
        {
          case "C":
          case "N":
          case "D":
          case "L":
          case "M":
            break;
          default:
            /*
            FErrors.AddError("Поле \"" + FieldName + "\" имеет неизвестный тип \"" + FieldType +
              "\". Загружено как строковое поле");
             * */
            FieldType = "C";
            break;
        }

        FDBStruct.Add(new DbfFieldInfo(FieldName, FieldType[0], Len, Prec));
        FieldOffsets.Add(RealRecSize);
        RealRecSize += Len;
      }

      if (RealRecSize != RecSize)
        throw new DbfFileFormatException("Размер записи, заданный в заголовке (" + RecSize.ToString() +
          ") не совпадает с размером всех полей (" + RealRecSize.ToString() + ")");

      if (DataOffset < rdrDBF.BaseStream.Position)
        throw new DbfFileFormatException("Неправильная позиция начала данных (" + DataOffset.ToString() + "). Описания полей перекрывают область данных");

      #endregion

      #region 3. Подготовка буфера строки

      UseMemoFile = (rdrDBT != null) && HasMemoFile;
      /*
      if (FHasMemoFile != HasMemoFile2)
      {
        if (FHasMemoFile)
          FErrors.AddError("В заголовке таблицы не задано использование DBT-файла, но в описании присутствуют MEMO-поля");
        else
          FErrors.AddWarning("В заголовке таблицы задано использование DBT-файла, хотя не объявлено ни одного MEMO-поля");
      }
      FHasMemoFile = HasMemoFile2;
       * */

      RecBuffer = new byte[RealRecSize];

      #endregion

      #region Проверка заголовка МЕМО-файла

      uint MemoBlockCount = 0;
      if (rdrDBT != null)
      {
        if (rdrDBT.BaseStream.Length < 5)
          throw new DbfFileFormatException("МЕМО-файл имеет недопустимо малую длину");

        MemoBlockCount = rdrDBT.ReadUInt32();
        if (MemoBlockCount < 1 || MemoBlockCount > (uint)(FileTools.GByte * 2 / 512))
          throw new DbfFileFormatException("В заголовке МЕМО-файла указано недопустимое число 512-байтных блоков: " + MemoBlockCount.ToString());

        long MinMemoFileSize = (long)(MemoBlockCount - 1) * 512 + 2;
        long MaxMemoFileSize = (long)(MemoBlockCount) * 512 + 1;
        if (rdrDBT.BaseStream.Length < MinMemoFileSize)
          throw new DbfFileFormatException("Реальный размер МЕМО-файла (" + rdrDBT.BaseStream.Length +
            ") меньше, чем вычисленный минимально возможный размер, исходя из заголовка (" +
            MinMemoFileSize.ToString() + ")");
        // Максимально возможный размер не проверяем, вдруг там ЭЦП
      }

      #endregion

      /***
      // 4. Чтение данных
      // Чтобы не перебирать каждый раз таблицу структуры, один раз читаем описания
      // полей в массивы
      string[] FieldNames = new string[FDBStruct.Rows.Count];
      char[] FieldTypes = new char[FDBStruct.Rows.Count];
      int[] Lens = new int[FDBStruct.Rows.Count];
      for (int i = 0; i < FDBStruct.Rows.Count; i++)
      {
        FieldNames[i] = DataTools.GetString(FDBStruct.Rows[i], "Name");
        FieldTypes[i] = DataTools.GetString(FDBStruct.Rows[i], "Type")[0];
        Lens[i] = DataTools.GetInt(FDBStruct.Rows[i], "Length");
        if (FieldTypes[i] == 'M' && !FHasMemoFile)
        {
          FErrors.AddWarning("МЕМО-поле \"" + FieldNames[i] + "\" загружаться не будет");
          FieldTypes[i] = ' ';
        }

        // Уточняем тип для числового поля
        if (FieldTypes[i] == 'N')
        {
          if (FData.Columns[i].DataType == typeof(int))
            FieldTypes[i] = 'I';
          else
          {
            if (FData.Columns[i].DataType == typeof(decimal))
              FieldTypes[i] = 'Y';
          }
        }
      }

       * **/
    }

    protected override void Dispose(bool Disposing)
    {
      DisposeStreams();
      base.Dispose(Disposing);
    }

    private void DisposeStreams()
    {
      if (ShouldDisposeRdrs)
      {
        if (rdrDBT != null)
        {
          rdrDBT.Close();
          rdrDBT = null;
        }
        if (rdrDBF != null)
        {
          rdrDBF.Close();
          rdrDBF = null;
        }
      }

      if (fsDBT != null)
      {
        fsDBT.Close();
        fsDBT = null;
      }
      if (fsDBF != null)
      {
        fsDBF.Close();
        fsDBF = null;
      }
    }

    #endregion


    #region Потоки

    /// <summary>
    /// Потоки, при условии, что их нужно будет освобождать
    /// </summary>
    private Stream fsDBF, fsDBT;

    /// <summary>
    /// Объекты для чтения двоичных данных
    /// </summary>
    private BinaryReader rdrDBF, rdrDBT;

    private bool ShouldDisposeRdrs;

    #endregion

    #region Структура таблицы

    /// <summary>
    /// Список полей таблицы
    /// </summary>
    public DbfStruct DBStruct { get { return FDBStruct; } }
    private DbfStruct FDBStruct;

    /// <summary>
    /// Смещения данных для каждого поля.
    /// Длина списка совпадает с DBStruct.Count
    /// </summary>
    private List<int> FieldOffsets;

    /// <summary>
    /// Признак использования memo-файла (наличие полей и DBT-файла)
    /// </summary>
    private bool UseMemoFile;

    #endregion

    #region Кодировка

    /// <summary>
    /// Используемая кодировка. Задается в конструкторе. Если не задана в явном виде, возвращает DefaultEncoding
    /// </summary>
    public Encoding Encoding { get { return FEncoding; } }
    private Encoding FEncoding;

    /// <summary>
    /// Возвращает кодировку по умолчанию
    /// </summary>
    public static Encoding DefaultEncoding
    {
      get
      {
        int CodePage = CultureInfo.CurrentCulture.TextInfo.OEMCodePage;
        return Encoding.GetEncoding(CodePage);
      }
    }

    #endregion

    #region Поля для поиска записи

    /// <summary>
    /// Число записей
    /// </summary>
    private uint LastRec;

    /// <summary>
    /// Смещение от начала файла до начала данных
    /// </summary>
    private ushort DataOffset;

    /// <summary>
    ///  Размер одной записи
    /// </summary>
    private ushort RecSize;

    #endregion

    #region Перебор записей

    /// <summary>
    /// Номер текущей записи, начиная с 1
    /// При инициализации устанавливается равным 0
    /// </summary>
    public int RecNo
    {
      get { return FRecNo; }
      set
      {
        if (value == FRecNo)
          return;

        if (value < 0 || value > LastRec)
          throw new ArgumentOutOfRangeException();

        FRecNo = value;
        if (value == 0)
          DataTools.FillArray<byte>(RecBuffer, 0);
        else
        {
          rdrDBF.BaseStream.Position = DataOffset + (value - 1) * RecSize;
          rdrDBF.BaseStream.Read(RecBuffer,0,RecSize);
        }
      }
    }
    private int FRecNo;

    /// <summary>
    /// Необходимость пропуска удаленных записей методом Read().
    /// По умолчанию - true - записи пропускаются
    /// </summary>
    public bool SkipDeleted;

    public bool Read()
    {
      while (RecNo < (LastRec - 1))
      {
        RecNo++;
        if (!RecordDeleted)
          return true;
      }
      return false;
    }

    #endregion

    #region Чтение / запись значений полей

    /// <summary>
    /// Буфер строки размером RecSize
    /// </summary>
    private byte[] RecBuffer;

    /// <summary>
    /// true - если текущая запись помечена на удаление
    /// </summary>
    public bool RecordDeleted
    {
      get
      {
        return RecBuffer[0] == '*';
      }
    }

    #endregion
  }
#endif
}
