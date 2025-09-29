using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Core
{
  /// <summary>
  /// Вычисление сумм по алгоритму MD5.
  /// Используется объект <see cref="System.Security.Cryptography.MD5"/>.
  /// </summary>
  public static class MD5Tools
  {
    #region Вычисление ХЭШ-сумм

    /// <summary>
    /// Получение хэш-суммы массива байтов по алгоритму MD5.
    /// Возвращает результат в виде 32-разрядной строки с 16-ричными символами.
    /// Если <paramref name="bytes"/>=null, то возвращается хэш-сумма для массива нулевой длины.
    /// Для расчета суммы для <see cref="System.IO.Stream"/> или файла, используйте методы в <see cref="FreeLibSet.IO.FileTools"/>.
    /// </summary>
    /// <param name="bytes">Исходный массив байт</param>
    /// <returns>Строка хэш-суммы</returns>
    public static string MD5Sum(byte[] bytes)
    {
      if (bytes == null)
        bytes = EmptyArray<byte>.Empty;
      System.Security.Cryptography.MD5 md5Hasher = System.Security.Cryptography.MD5.Create();
      byte[] HashRes = md5Hasher.ComputeHash(bytes);
      return StringTools.BytesToHex(HashRes, false);
    }

    /// <summary>
    /// Получение суммы MD5 для строки.
    /// Строка представляется в кодировке Unicode.
    /// Для расчета суммы для <see cref="System.IO.Stream"/> или файла, используйте методы в <see cref="FreeLibSet.IO.FileTools"/>.
    /// </summary>
    /// <param name="s">Строка, для которой вычисляется сумма. Может быть пустой строкой.
    /// Null считается пустой строкой</param>
    /// <returns>Сумма MD5</returns>
    public static string MD5SumFromString(string s)
    {
      if (s == null)
        s = String.Empty;
      byte[] b = Encoding.Unicode.GetBytes(s);
      return MD5Sum(b);
    }

    /// <summary>
    /// Получение хэш-суммы данных из потока по алгоритму MD5.
    /// Возвращает результат в виде 32-разрядной строки с 16-ричными символами.
    /// Если <paramref name="stream"/>=null, то возвращается хэш-сумма для массива нулевой длины.
    /// Для расчета суммы для массива байтов или строки используйте методы в <see cref="DataTools"/>.
    /// </summary>
    /// <param name="stream">Поток данных для чтения</param>
    /// <returns>Строка хэш-суммы</returns>
    public static string MD5Sum(System.IO.Stream stream)
    {
      if (stream == null)
        return MD5Tools.MD5Sum(new byte[0]);
      System.Security.Cryptography.MD5 md5Hasher = System.Security.Cryptography.MD5.Create();
      byte[] hashRes = md5Hasher.ComputeHash(stream);
      return StringTools.BytesToHex(hashRes, false);
    }

    /// <summary>
    /// Получение хэш-суммы для файла по алгоритму MD5.
    /// Возвращает результат в виде 32-разрядной строки с 16-ричными символами.
    /// Файл должен существовать.
    /// Для расчета суммы для массива байтов или строки используйте методы в <see cref="DataTools"/>.
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>Строка хэш-суммы</returns>
    public static string MD5Sum(FreeLibSet.IO.AbsPath filePath)
    {
      if (filePath.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("filePath");

      string str;
      System.IO.FileStream fs = new System.IO.FileStream(filePath.Path, 
        System.IO.FileMode.Open, 
        System.IO.FileAccess.Read, 
        System.IO.FileShare.Read);
      try
      {
        str = MD5Sum(fs);
      }
      finally
      {
        fs.Close();
      }
      return str;
    }

    #endregion
  }
}
