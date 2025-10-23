// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.Config;
using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.RI
{
  #region Перечисление RIValueCfgType

  /// <summary>
  /// Тип секции конфигурации для сохранения значения
  /// </summary>
  [Serializable]
  public enum RIValueCfgType
  {
    /// <summary>
    /// Основная секция конфигурации, относящаяся к пользователю
    /// </summary>
    Default,

    /// <summary>
    /// Секция конфигурации, относящаяся к пользователю и компьютеру.
    /// В ней хранятся, например, имена файлов и каталогов.
    /// Если программа выполняется на другом компьютере, то эти данные недействительны.
    /// Под компьютером понимается обычно клиентский (вызываемый) компьютер
    /// </summary>
    MachineSpecific
  }

  #endregion

  /// <summary>
  /// Интерфейс объекта, который может читать и восстанавливать состояние в одной или двух секциях конфигурации.
  /// Интерфейс реализуется всеми объектами RIIItem.
  /// </summary>
  public interface IRIValueSaveable
  {
    #region Методы

    /// <summary>
    /// Если класс поддерживает сохранение значений между вызовами, он должен вернуть true для той секции конфигурации (или нескольких секций), 
    /// которая используется для хранения значения.
    /// Метод должен вернуть false, если свойство Name не установлено.
    /// Метод переопределяется для составных элементов, например RIBand.
    /// Вместо этого метода обычно следует переопределить OnSupportsCfgType.
    /// </summary>
    /// <param name="cfgType">Запрашиваемый тип секции конфигурации</param>
    /// <returns>true, если секция используется</returns>
    bool SupportsCfgType(RIValueCfgType cfgType);


    /// <summary>
    /// Записать значения в секцию конфигурации для сохранения.
    /// </summary>
    /// <param name="part">Записываемая секция конфигурации</param>
    /// <param name="cfgType">Тип секции конфигурации. 
    /// Должен проверяться перед записью, так как метод может вызываться и для секций, для которых SupportsCfgType() вернул false.</param>
    void WriteValues(CfgPart part, RIValueCfgType cfgType);

    /// <summary>
    /// Прочитать значения из секции конфигурации.
    /// </summary>
    /// <param name="part">Считываемая секция конфигурации</param>
    /// <param name="cfgType">Тип секции конфигурации. 
    /// Должен проверяться перед записью, так как метод может вызываться и для секций, для которых SupportsCfgType() вернул false.</param>
    void ReadValues(CfgPart part, RIValueCfgType cfgType);

    #endregion
  }

  /// <summary>
  /// Интерфейс, реализующий чтение/запись секций конфигурации.
  /// Для работы с интерфейсом используются статические методы в RITools.
  /// </summary>
  public interface IRIValueSaver
  {
    #region Методы

    /// <summary>
    /// Метод должен возвращать true для <paramref name="cfgType"/>=Default.
    /// Для значения MachineSpecific метод должен вернуть true, если данные хранятся на сервере (что обычно бывает. Иначе - зачем нужен удаленный интерфейс).
    /// Если метод вернет false, значит машинно-специфические данные будут храниться в основной секции данных.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации</param>
    /// <returns>true, если данные должны храниться отдельно</returns>
    bool SupportsCfgType(RIValueCfgType cfgType);

    /// <summary>
    /// Этот метод вызывается перед чтением или записью значений.
    /// Если метод вернет null, данные не будут читаться или записываться.
    /// Если метод вызывается, обязательно будет вызван EndReadWrite(), даже если возникает исключение.
    /// </summary>
    /// <param name="cfgType">Тип секции конфигурации, которая требуется</param>
    /// <param name="isWrite">true-запись значений, false - чтение значений</param>
    /// <returns>Секция для чтения или записи значений, или null</returns>
    CfgPart BeginReadWrite(RIValueCfgType cfgType, bool isWrite);

    /// <summary>
    /// Этот метод вызывается после чтения или записи значений.
    /// Вызов является парным для BeginReadWrite
    /// </summary>
    /// <param name="cfg">Секция, полученная от BeginReadWrite(). Не может быть null</param>
    /// <param name="cfgType">Тип секции конфигурации</param>
    /// <param name="isWrite">true-запись значений, false - чтение значений</param>
    void EndReadWrite(CfgPart cfg, RIValueCfgType cfgType, bool isWrite);

    #endregion
  }

  /// <summary>
  /// Вспомогательные функции для удаленного пользовательского интерфейса
  /// </summary>
  public static class RITools
  {
    #region GetMessageBoxIcon

    /// <summary>
    /// Возвращает значок Error, Warning или Information для сообщения с заданным уровнем серьезности.
    /// Полученный значок может использоваться при вызове метода MessageBox().
    /// </summary>
    /// <param name="kind">Уровень серьезности</param>
    /// <returns>Идентификатор значка для MessageBox()</returns>
    public static MessageBoxIcon GetMessageBoxIcon(ErrorMessageKind kind)
    {
      switch (kind)
      {
        case ErrorMessageKind.Info: return MessageBoxIcon.Information;
        case ErrorMessageKind.Warning: return MessageBoxIcon.Warning;
        default: return MessageBoxIcon.Error;
      }
    }

    /// <summary>
    /// Возвращает значок Error, Warning или Information для сообщения с заданным уровнем серьезности.
    /// Для значения <paramref name="kind"/>=null возвращается None.
    /// Полученный значок может использоваться при вызове метода MessageBox().
    /// </summary>
    /// <param name="kind">Уровень серьезности</param>
    /// <returns>Идентификатор значка для MessageBox()</returns>
    public static MessageBoxIcon GetMessageBoxIcon(ErrorMessageKind? kind)
    {
      if (kind.HasValue)
        return GetMessageBoxIcon(kind.Value);
      else
        return MessageBoxIcon.None;
    }

    /// <summary>
    /// Возвращает значок Error, Warning или Information для списка сообщений.
    /// Если <paramref name="errors"/>=null или список не содержит сообщений, возвращается None.
    /// Полученный значок может использоваться при вызове метода MessageBox().
    /// </summary>
    /// <param name="errors">Список сообщений</param>
    /// <returns>Идентификатор значка для MessageBox()</returns>
    public static MessageBoxIcon GetMessageBoxIcon(ErrorMessageList errors)
    {
      return GetMessageBoxIcon(errors, false);
    }

    /// <summary>
    /// Возвращает значок Error, Warning или Information для списка сообщений.
    /// Если <paramref name="errors"/>=null или список не содержит сообщений, то возвращается None или Information,
    /// в зависимости от параметра <paramref name="informationIfEmpty"/>.
    /// Полученный значок может использоваться при вызове метода MessageBox().
    /// </summary>
    /// <param name="errors">Список сообщений</param>
    /// <param name="informationIfEmpty">Определяет значок, когда список <paramref name="errors"/>пустой.
    /// Если true, то возвращается Information, если false - то None</param>
    /// <returns>Идентификатор значка для MessageBox()</returns>
    public static MessageBoxIcon GetMessageBoxIcon(ErrorMessageList errors, bool informationIfEmpty)
    {
      if (errors == null)
        errors = ErrorMessageList.Empty;
      if (errors.Count == 0)
        return informationIfEmpty ? MessageBoxIcon.Information : MessageBoxIcon.None;
      else
        return GetMessageBoxIcon(errors.Severity);
    }

    #endregion

    #region Чтение и запись значений

    /// <summary>
    /// Работа с интерфейсом IRIValueSaver
    /// Выполняет запись значений, если есть сохраняемые данные и <paramref name="saver"/> поддерживает секции конфигурации.
    /// Перехватывает все исключения
    /// </summary>
    /// <param name="saver">Объект, реализующий IRIValueSaver. Может быть null</param>
    /// <param name="item">Объект Dialog, StandardDialog или пользовательский объект, который желает хранить данные</param>
    /// <returns>true, если запись выполнена</returns>
    public static bool WriteValues(IRIValueSaver saver, IRIValueSaveable item)
    {
      return ReadWriteValues(saver, item, true);
    }

    /// <summary>
    /// Работа с интерфейсом IRIValueSaver
    /// Выполняет чтение значений, если есть сохраняемые данные и <paramref name="saver"/> поддерживает секции конфигурации.
    /// Перехватывает все исключения
    /// </summary>
    /// <param name="saver">Объект, реализующий IRIValueSaver. Может быть null</param>
    /// <param name="item">Объект Dialog, StandardDialog или пользовательский объект, который желает хранить данные</param>
    /// <returns>true, если чтение выполнено</returns>
    public static bool ReadValues(IRIValueSaver saver, IRIValueSaveable item)
    {
      return ReadWriteValues(saver, item, false);
    }

    /// <summary>
    /// Работа с интерфейсом IRIValueSaver
    /// Выполняет чтение или запись значений, если есть сохраняемые данные и Saver поддерживает секции конфигурации.
    /// Перехватывает все исключения
    /// </summary>
    /// <param name="saver">Объект, реализующий IRIValueSaver. Может быть null</param>
    /// <param name="item">Объект Dialog, StandardDialog или пользовательский объект, который желает хранить данные</param>
    /// <param name="isWrite">true-запись значений, false - чтение</param>
    /// <returns>true, нсли чтение или запись выполнена</returns>
    private static bool ReadWriteValues(IRIValueSaver saver, IRIValueSaveable item, bool isWrite)
    {
      if (saver == null)
        return false;
      if (item == null)
        return false;

      try
      {
        if (!saver.SupportsCfgType(RIValueCfgType.Default))
          return false; // Обработчик отказался сохранять данные

        bool needsDefaultCfg = item.SupportsCfgType(RIValueCfgType.Default);
        bool needsMachineSpecificCfg = item.SupportsCfgType(RIValueCfgType.MachineSpecific);

        if (needsDefaultCfg)
        {
          if (needsMachineSpecificCfg)
          {
            // Требуется обе секции
            if (saver.SupportsCfgType(RIValueCfgType.MachineSpecific))
            {
              // Обрабатываем секции отдельно

              if (!ReadWriteValues2(saver, item, isWrite, RIValueCfgType.Default, RIValueCfgType.Default))
                return false;
              if (!ReadWriteValues2(saver, item, isWrite, RIValueCfgType.MachineSpecific, RIValueCfgType.MachineSpecific))
                return false;
              return true;
            }
            else
            {
              // Обрабатываем обе секции вместе
              CfgPart cfg = saver.BeginReadWrite(RIValueCfgType.Default, isWrite);
              if (cfg == null)
                return false;
              try
              {
                try
                {
                  ReadWriteValues3(item, cfg, RIValueCfgType.Default, isWrite);
                  ReadWriteValues3(item, cfg, RIValueCfgType.MachineSpecific, isWrite);
                }
                finally
                {
                  saver.EndReadWrite(cfg, RIValueCfgType.Default, isWrite);
                }
              }
              catch
              {
                return false;
              }
              return true;
            }
          }
          else
          {
            // Требуется только основная секция
            return ReadWriteValues2(saver, item, isWrite, RIValueCfgType.Default, RIValueCfgType.Default);
          }
        }
        else
        {
          if (needsMachineSpecificCfg)
          {
            // Требуется только секция машинных данных
            if (saver.SupportsCfgType(RIValueCfgType.MachineSpecific))
              return ReadWriteValues2(saver, item, isWrite, RIValueCfgType.MachineSpecific, RIValueCfgType.MachineSpecific);
            else
              return ReadWriteValues2(saver, item, isWrite, RIValueCfgType.Default, RIValueCfgType.MachineSpecific);
          }
          else
            // Ничего не нужно
            return false;
        }
      }
      catch
      {
        return false;
      }
    }

    private static bool ReadWriteValues2(IRIValueSaver saver, IRIValueSaveable item, bool isWrite, RIValueCfgType saverCfgType, RIValueCfgType itemCfgType)
    {
      CfgPart cfg = saver.BeginReadWrite(saverCfgType, isWrite);
      if (cfg == null)
        return false;
      try
      {
        try
        {
          ReadWriteValues3(item, cfg, itemCfgType, isWrite);
        }
        finally
        {
          saver.EndReadWrite(cfg, saverCfgType, isWrite);
        }
        return true;
      }
      catch
      {
        return false;
      }
    }

    private static void ReadWriteValues3(IRIValueSaveable item, CfgPart part, RIValueCfgType cfgType, bool IsWrite)
    {
      if (IsWrite)
        item.WriteValues(part, cfgType);
      else
        item.ReadValues(part, cfgType);
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс для RIValueWriter и RIValueReader
  /// </summary>
  public abstract class RIValueReaderWriter : SimpleDisposableObject
  {
    #region Конструктор и Dispose

    internal RIValueReaderWriter(IRIValueSaver saver, RIValueCfgType cfgType, bool isWrite)
    {
      _Saver = saver;
      _CfgType = cfgType;
      _IsWrite = isWrite;

      if (saver != null)
        _Part = saver.BeginReadWrite(cfgType, isWrite);

      if (_Part == null)
        _Part = new TempCfg();
      else
        _IsSupported = true;
    }

    /// <summary>
    /// Вызывает метод IRIValueSaver.EndReadWrite()
    /// </summary>
    /// <param name="disposing">true, если вызван метод Dispose(), а не деструктор</param>
    protected sealed override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_Part != null && _IsSupported)
        {
          _Saver.EndReadWrite(_Part, _CfgType, _IsWrite);
          _Part = null;
        }
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    private readonly IRIValueSaver _Saver;

    private readonly RIValueCfgType _CfgType;

    private readonly bool _IsWrite;

    /// <summary>
    /// Основное свойство.
    /// Часть секции конфигурации, предназначенная для чтения/записи значений.
    /// Это свойство не возвращает null, даже если нет данных для чтения или сохранение не поддерживается.
    /// В этом случае возвращается заглушка.
    /// После вызова метода Dispose() возвращает null.
    /// </summary>
    public CfgPart Part { get { return _Part; } }
    private CfgPart _Part;

    /// <summary>
    /// Свойство возвращает true, если чтение и запись реально выполняются.
    /// Возвращает false, если используется заглушка TempCfg.
    /// </summary>
    public bool IsSupported { get { return _IsSupported; } }
    private readonly bool _IsSupported;

    #endregion
  }

  /// <summary>
  /// Объект для упрощения записи значений из прикладного кода.
  /// Используйте оператор using с этим объектом. Свойство Part дает доступ к записываемой секции конфигурации.
  /// </summary>
  public sealed class RIValueWriter : RIValueReaderWriter
  {
    #region Конструкторы

    /// <summary>
    /// Открыть секцию конфигурации.
    /// </summary>
    /// <param name="saver">Интерфейс получения секций конфигурации. Может быть null</param>
    /// <param name="cfgType">Тип сохраняемых значений (наличие привязки к компьютеру)</param>
    public RIValueWriter(IRIValueSaver saver, RIValueCfgType cfgType)
      : base(saver, cfgType, true)
    {
    }


    /// <summary>
    /// Открыть секцию конфигурации.
    /// Эта перегрузка конструктора использует тип сохраняемых значений RIValueCfgType.Default.
    /// </summary>
    /// <param name="saver">Интерфейс получения секций конфигурации. Может быть null</param>
    public RIValueWriter(IRIValueSaver saver)
      : base(saver, RIValueCfgType.Default, true)
    {
    }

    #endregion
  }

  /// <summary>
  /// Объект для упрощения записи значений из прикладного кода.
  /// Используйте оператор using с этим объектом. Свойство Part дает доступ к записываемой секции конфигурации.
  /// </summary>
  public sealed class RIValueReader : RIValueReaderWriter
  {
    #region Конструкторы

    /// <summary>
    /// Открыть секцию конфигурации.
    /// </summary>
    /// <param name="saver">Интерфейс получения секций конфигурации. Может быть null</param>
    /// <param name="cfgType">Тип сохраняемых значений (наличие привязки к компьютеру)</param>
    public RIValueReader(IRIValueSaver saver, RIValueCfgType cfgType)
      : base(saver, cfgType, true)
    {
    }


    /// <summary>
    /// Открыть секцию конфигурации.
    /// Эта перегрузка конструктора использует тип сохраняемых значений RIValueCfgType.Default.
    /// </summary>
    /// <param name="saver">Интерфейс получения секций конфигурации. Может быть null</param>
    public RIValueReader(IRIValueSaver saver)
      : base(saver, RIValueCfgType.Default, true)
    {
    }

    #endregion
  }

  /// <summary>
  /// Простейшая реализация интерфейса IRIValueSaver для хранения всех данных в одной секции конфигурации.
  /// </summary>
  public class RISimpleValueSaver : IRIValueSaver
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник для секции конфигурации, например, для TempCfg.
    /// </summary>
    /// <param name="singlePart">Секция конифгурации, которая будкт использоваться для чтения и записи</param>
    public RISimpleValueSaver(CfgPart singlePart)
    {
      _SinglePart = singlePart;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Секция конифгурации, которая будкт использоваться для чтения и записи.
    /// </summary>
    public CfgPart SinglePart { get { return _SinglePart; } }
    private readonly CfgPart _SinglePart;

    #endregion

    #region IRIValueSaver

    /// <summary>
    /// Возвращает true для типа Default и false для MachineSpecific
    /// </summary>
    /// <param name="cfgType">Запрашиваемый тип данных</param>
    /// <returns>Есть ли поддержка</returns>
    public bool SupportsCfgType(RIValueCfgType cfgType)
    {
      return cfgType == RIValueCfgType.Default;
    }

    /// <summary>
    /// Возвращает SinglePart
    /// </summary>
    /// <param name="cfgType">Игнорируется</param>
    /// <param name="isWrite">Игнорируется</param>
    /// <returns>Секция конфигурации</returns>
    public CfgPart BeginReadWrite(RIValueCfgType cfgType, bool isWrite)
    {
      return _SinglePart;
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="cfg">Должно совпадать с SinglePart</param>
    /// <param name="cfgType">Игнорируется</param>
    /// <param name="isWrite">Игнорируется</param>
    public void EndReadWrite(CfgPart cfg, RIValueCfgType cfgType, bool isWrite)
    {
      if (!Object.ReferenceEquals(cfg, _SinglePart))
        throw new ArgumentException(Res.RI_Arg_AlienCfgPart, "cfg");
    }

    #endregion
  }
}
