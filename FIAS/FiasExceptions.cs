// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace FreeLibSet.FIAS
{
  /// <summary>
  /// Исключение выбрасывается, если настройки, заданные в классе FiasDBSettings, не позволяют выполнить
  /// операцию. Например, требуется получить информацию из таблицы домов, а DBxSettings.UseHouse=false
  /// </summary>
  [Serializable]
  public class FiasDBSettingsException : InvalidOperationException
  {
    #region Конструктор

    /// <summary>
    /// Создает объект исключения с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public FiasDBSettingsException(string message)
      : this(message, null)
    {
    }


    /// <summary>
    /// Создает объект исключения с заданным сообщением и вложенным исключением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    /// <param name="innerException">Вложенное исключение</param>
    public FiasDBSettingsException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected FiasDBSettingsException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// Исключение выбрасывается, если настройки, переданные конструктору FiasDB, не совпадают с существующими в базе данных
  /// </summary>
  [Serializable]
  public class FiasDBSettingsDifferentException : InvalidOperationException
  {
    #region Конструктор

    /// <summary>
    /// Создает объект исключения с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public FiasDBSettingsDifferentException(string message)
      : this(message, null)
    {
    }


    /// <summary>
    /// Создает объект исключения с заданным сообщением и вложенным исключением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    /// <param name="innerException">Вложенное исключение</param>
    public FiasDBSettingsDifferentException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected FiasDBSettingsDifferentException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }
}
