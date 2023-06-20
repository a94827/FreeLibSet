// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Core;

// В autodoc'е не определены ссылки на класс System.Configuration.ConnectionStringSettings,
// т.к. нет ссылки на сборку System.configurations.dll

namespace FreeLibSet.Data
{
  /// <summary>
  /// Генератор объектов <see cref="DBx"/> для строк подключения.
  /// Для каждого типа базы данных выводится свой (закрытый) класс, производный от <see cref="DBxManager"/>.
  /// Статический список <see cref="DBxManager.Managers"/> содержит список менеждеров, по одному на каждый тип
  /// базы данных.
  /// В качестве кода используется имя провайдера базы данных, принятое в Net Framework. 
  /// Например, для Microsoft SQL Server используется код "System.Data.SqlClient".
  /// См. свойство System.Configuration.ConnectionStringSettings.ProviderName.
  /// Этот класс является потокобезопасным.
  /// </summary>
  public abstract class DBxManager : ObjectWithCode
  {
    #region Защищенный конструктор

    /// <summary>
    /// Создает генератор
    /// </summary>
    /// <param name="providerName">Имя провайдера Net Framework</param>
    protected DBxManager(string providerName)
      : base(providerName)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя провайдера Net Framework.
    /// Является ключом в статическом списке <see cref="Managers"/>.
    /// </summary>
    public string ProviderName { get { return base.Code; } }

    /// <summary>
    /// Генератор объектов, специфических для базы данных.
    /// Если есть созданный объект базы данных <see cref="DBx"/>, используйте свойство <see cref="DBx.ProviderFactory"/>.
    /// </summary>
    public abstract DbProviderFactory ProviderFactory { get; }

    #endregion

    #region Виртуальные методы

    /// <summary>
    /// Создать объект базы данных, используя строку подключения
    /// </summary>
    /// <param name="connectionString">Строка подключения</param>
    /// <returns>Созданный объект <see cref="DBx"/></returns>
    public abstract DBx CreateDBObject(string connectionString);

    /// <summary>
    /// Получить строку подключения для другой базы данных "по образцу".
    /// Реализация метода получает "структурированную" строку подключения (производную от <see cref="DBConnectionString"/>)
    /// и выполняет замену свойств, например, DatabaseName.
    /// Если в строке подключения заданы имена файлов (или каталогов), они также должны быть осмысленно
    /// изменены.
    /// </summary>
    /// <param name="connectionString">Существующая строка подключения к базе данных <paramref name="oldDBName"/></param>
    /// <param name="oldDBName">Имя базы данных в cуществующей строке подключения</param>
    /// <param name="newDBName">Имя новой базы данных</param>
    /// <returns></returns>
    /// <remarks>
    /// Метод должен учитывать возможность наличия префикса и суффикса в имени базы данных.
    /// Например, если в исходной строке подключения задано имя базы данных "myprog_db1", а требуется
    /// заменить имя базы данных "db1" на "db2", то метод должен задать в строке подключения имя 
    /// базы данных "myprog_db2".
    /// Для выполнения замены следует использовать методы <see cref="ReplaceDBItem(string, string, string)"/> и <see cref="ReplaceFileItem(string, string, string)"/>,
    /// которые обрабатывают наличие префиксов
    /// </remarks>
    public abstract string ReplaceDBName(string connectionString, string oldDBName, string newDBName);

    /// <summary>
    /// Создает объект <see cref="DbConnectionStringBuilder"/> для заданной строки подключения.
    /// Возвращается объект производного класса
    /// </summary>
    /// <param name="connectionString">Строка подключения</param>
    /// <returns>Объект для манипуляции строкой подключения</returns>
    public abstract DbConnectionStringBuilder CreateConnectionStringBuilder(string connectionString);

    #endregion

    #region Методы для реализации ReplaceDBName

    /// <summary>
    /// Замена имени базы данных <paramref name="oldName"/> в элементе строки подключения <paramref name="oldItem"/>
    /// на <paramref name="newName"/>
    /// </summary>
    /// <param name="oldItem">"Элемент строки подключения</param>
    /// <param name="oldName">Имя базы даннык, которое уже задано в <paramref name="oldItem"/>. 
    /// Если аргумент не задан, то имя базы данных берется равным <paramref name="oldItem"/>.
    /// В этом случае нельзя использовать префикс/суффикс для базы данных</param>
    /// <param name="newName">Имя новой базы данных, которое должно быть задано</param>
    /// <returns>Измененный текст</returns>
    protected static string ReplaceDBItem(string oldItem, string oldName, string newName)
    {
      if (String.IsNullOrEmpty(oldItem))
        return oldItem;
      if (String.IsNullOrEmpty(oldName))
        oldName = oldItem;
      if (String.IsNullOrEmpty(newName))
        throw new ArgumentNullException("newName");

      int p = oldItem.LastIndexOf(oldName, StringComparison.OrdinalIgnoreCase);
      if (p < 0)
        throw new ArgumentException("Имя базы данных \"" + oldItem + "\" не содержит в себе фрагмента \"" + oldName + "\", который требуется заменить");
      return oldItem.Substring(0, p) + newName + oldItem.Substring(p + oldName.Length);
    }

    private static readonly string InvalidFileNameChars = new String(System.IO.Path.GetInvalidFileNameChars());

    /// <summary>
    /// Замена имени базы данных в имени файла.
    /// Этот метод заменяет только имя файла, но не каталог, в котором файл расположен
    /// </summary>
    /// <param name="oldItem">Существующее имя файла в строке подключения</param>
    /// <param name="oldName">Имя существующей базы данных. Если аргумент не задан, используется
    /// имя файла без распширения, извлеченное из <paramref name="oldItem"/>.</param>
    /// <param name="newName">Имя новой базы данных</param>
    /// <returns></returns>
    protected static string ReplaceFileItem(string oldItem, string oldName, string newName)
    {
      if (String.IsNullOrEmpty(oldItem))
        return oldItem;

      // Не будем использовать AbsPath для преобразования, т.к. имя файла может быть задано
      // каким-нибудь хитрым образом

      int p = oldItem.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
      string dir = oldItem.Substring(0, p + 1); // включая символ каталога
      string fileName = oldItem.Substring(p + 1); // без пути

      if (DataTools.IndexOfAny(fileName, InvalidFileNameChars) >= 0)
        throw new ArgumentException("Имя файла \"" + fileName + "\" содержит недопустимые символы", "oldItem");
      // Путь dir не проверяем

      string ext = System.IO.Path.GetExtension(fileName);
      fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);

      if (String.IsNullOrEmpty(oldName))
        oldName = fileName;
      if (String.IsNullOrEmpty(newName))
        throw new ArgumentNullException("newName");

      p = fileName.LastIndexOf(oldName, StringComparison.OrdinalIgnoreCase);
      if (p < 0)
        throw new ArgumentException("Имя файла базы данных \"" + oldItem + "\" не содержит в себе фрагмента \"" + oldName + "\", который требуется заменить");
      fileName = fileName.Substring(0, p) + newName + fileName.Substring(p + oldName.Length);

      return dir + fileName + ext;
    }

    #endregion

    #region Статический список

    // Нельзя инициализировать список непосредственно или в статическом конструкторе DBxManager,
    // т.к. будет конфликт: В список надо добавить объекты классов, производных от DBxManager.
    // Во избежание неприятностей делаем отложенную инициалацию списка
    //public static SyncNamedCollection<DBxManager> Managers { get { return _Managers; } }
    //private static SyncNamedCollection<DBxManager> _Managers =  new SyncNamedCollection<DBxManager>();

    /// <summary>
    /// Список зарегистрированных менеджеров.
    /// По умолчанию список содержит менеджеры для всех провайдеров, реализованных в ExtDB.dll.
    /// См. константы в <see cref="DBxProviderNames"/>.
    /// Можно добавить менеджеры, реализованные в приложении
    /// </summary>
    public static SyncNamedCollection<DBxManager> Managers 
    { 
      get 
      {
        lock (_SyncRoot)
        {
          if (_Managers == null)
          {
            _Managers = new SyncNamedCollection<DBxManager>();
            _Managers.Add(FreeLibSet.Data.SqlClient.SqlDBxManager.TheManager);
            _Managers.Add(FreeLibSet.Data.Npgsql.NpgsqlDBxManager.TheManager);
            _Managers.Add(FreeLibSet.Data.SQLite.SQLiteDBxManager.TheManager);
          }
          return _Managers;
        }
      }
    }
    private static SyncNamedCollection<DBxManager> _Managers = null;

    private static object _SyncRoot = new object();

    #endregion
  }

  /// <summary>
  /// Строки, задающие имена провайдеров, реализованных в ExtDB.dll
  /// Если для подключения к БД используется файл конфигурации приложения,
  /// то константы также определяют имена для свойства ConnectionStringSettings.ProviderName
  /// </summary>
  public static class DBxProviderNames
  {
    #region Константы

    /// <summary>
    /// Имя провайдера для MS SQL Server
    /// </summary>
    public const string Sql = "System.Data.SqlClient";

    /// <summary>
    /// Имя провайдера для PostgreSQL
    /// </summary>
    public const string Npgsql = "Npgsql";

    /// <summary>
    /// Имя провайдера для SQLite
    /// </summary>
    public const string SQLite = "SQLite";

    #endregion
  }
}
