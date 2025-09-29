// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Core
{
  /// <summary>
  /// Интерфейс доступа только для чтения к именованным значениям.
  /// Порядок элементов в коллекции считается неопределенным.
  /// </summary>
  public interface INamedValuesAccess
  {
    #region Методы

    /// <summary>
    /// Получить значение с заданным именем
    /// </summary>
    /// <param name="name">Имя. Не может быть пустой строкой</param>
    /// <returns>Значение. Что происходит, если запрошено несуществующее имя, зависит от реализации</returns>
    object GetValue(string name);

    /// <summary>
    /// Определить наличие заданного имени в коллекции.
    /// </summary>
    /// <param name="name">Проверяемое имя</param>
    /// <returns>True, если в коллекции есть значение с таким именем</returns>
    bool Contains(string name);

    /// <summary>
    /// Получить список всех имен, которые есть в коллекции
    /// </summary>
    /// <returns>Массив имен</returns>
    string[] GetNames();

    #endregion
  }

  /// <summary>
  /// Пустая реализация интерфейса <see cref="INamedValuesAccess"/>
  /// </summary>
  public sealed class DummyNamedValues:INamedValuesAccess
  {
    #region INamedValuesAccess Members

    /// <summary>
    /// Выбрасывает исключение
    /// </summary>
    /// <param name="name">Не используется</param>
    /// <returns>Не возвращается</returns>
    public object GetValue(string name)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает false
    /// </summary>
    /// <param name="name">Не используется</param>
    /// <returns>false</returns>
    public bool Contains(string name)
    {
      return false;
    }

    /// <summary>
    /// ВОзвращает пустой массив
    /// </summary>
    /// <returns></returns>
    public string[] GetNames()
    {
      return EmptyArray<string>.Empty;
    }

    #endregion
  }
}

namespace FreeLibSet.Data
{
  /// <summary>
  /// Расширение интерфейса <see cref="INamedValuesAccess"/> для доступа к значениям строки <see cref="DataRow"/>.
  /// Реализуется классоми <see cref="DataTableValues"/> и производными от него.
  /// </summary>
  public interface IDataRowNamedValuesAccess : INamedValuesAccess
  {
    /// <summary>
    /// Текущая строка.
    /// Свойство должно быть установлено перед доступом к значениям.
    /// </summary>
    DataRow CurrentRow { get; set;}
  }
}
