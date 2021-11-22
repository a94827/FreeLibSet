// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Logging;
using System.Data.Common;
using System.Threading;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Data
{
  /*
   * Расширение работы с БД
   * Возможности:
   *   - Асинхронная отправка запросов к одной базе данных
   *   - Возможность автоматического обновления структуры
   *   - Одинаковый набор функций для всех БД без использования SQL
   *   - Повышение безопасности за счет исключения Injected Code
   *   - Возможность передачи запросов по сети в модели с сервером приложений без 
   *     организации соединения клиента с БД
   * 
   * Абстрактный уровень
   *   - Класс DBx описывает базу данных. Создается в единственном экземпляре на базу данных в программе
   *   - Класс DBxEntry задает точку подключения. Задает права пользователя. К одной базе данных могут
   *     подключаться пользователи с разными правами. Или разные права могут требоваться для выполнения
   *     разных задач.
   *   - Класс DBxConBase предназначен для работы в потоке и создается в DBxEntry. Он содержит абстрактные методы 
   *     для выполнения SQL-запросов. Сам по себе такой объект не используется в пользовательском коде
   *   - Класс DBxCon предназначен для использования в пользовательком коде. Объекты создаются и разрушаются
   *     после выполнения запросов. Этот класс не является абстрактным.
   *   - Классы DBxColumns, DBxFilter (производные), DBxOrder (производные) используются для формирования
   *     запроса. Преобразованием в текст SQL-запроса занимается класс, производный от DBx
   *
   * Реализация
   *   - Классы DBxJetOleDb и DBxConJetOleDb - реализация доступа к Jet OLE DB Provider
   *   - Классы DBxSqlServer и DBxConSqlServer - реализация доступа к Sql Server
   * 
   * Работа
   *   - Сначала в программе создается один (или несколько) объектов, производных от DBx. 
   *   - Если необходимо автоматическое создание/обновление структуры базы данных, то заполняется свойство DBx.DBStruct и вызывается метод
   *     DBx.CreateOrUpdate()
   *   - Создаются, по необходимости, объекты DBxEntry. Объект, производный от DBx может создавать "главную"
   *     точку входа автоматически
   *   - Далее, в потоке асинхронно создается объект DBxCon.
   *   - В полученном DBxCon выполняются SQL-операторы. Все вызовы для соединения должны выполняться в потоке, для которого соединение получено
   *     Для выполнения обычного SQL-запроса SELECT:
   *     - создаются объекты DBxColumns, DBxFilter и DBxOrder
   *     - вызывается метод DBxCon.FillSelect()
   *   - Вызывается DBxCon.Dispose(), освобождающий соединение (или используется оператор using)
   *   - Перед завершением работы программы вызывается DBx.Dispose(). В этот момент должны быть
   *     разрушены все созданные ранее DBxCon
   * 
   * Подключение клиента
   *   В модели клиента без прямого подключения к базе данных, клиент должен иметь возможности выполнять
   *   SQL-запросы в рамках определенного для него подключения, но не должен иметь доступа к объекту DBx,
   *   т.к. такой доступ позволяет получить несанкционированный доступ к базе данных. Требуется объект-посредник
   *   DBxCon, реализующий передачу marshal-by-reference
   */

  #region Делегаты

  /// <summary>
  /// Аргументы события DBx.CreateSqlException
  /// </summary>
  public class DBxCreateSqlExceptionEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает аргументы события.
    /// Конструктор не должен использоваться в пользовательском коде
    /// </summary>
    /// <param name="con">Соединение, для которого выполнялся SQL-запрос</param>
    /// <param name="cmdText">Текст SQL-запроса, вызвавшего исключение</param>
    /// <param name="innerException">Перехваченное исключение от ADO.NET</param>
    public DBxCreateSqlExceptionEventArgs(DBxConBase con, string cmdText, Exception innerException)
    {
      _Con = con;
      _CmdText = cmdText;
      _InnerException = innerException;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Соединение, для которого выполнялся SQL-запрос
    /// </summary>
    public DBxConBase Con { get { return _Con; } }
    private DBxConBase _Con;

    /// <summary>
    /// Текст SQL-запроса, вызвавшего исключение
    /// </summary>
    public string CmdText { get { return _CmdText; } }
    private string _CmdText;

    /// <summary>
    /// Перехваченное исключение от ADO.NET
    /// </summary>
    public Exception InnerException { get { return _InnerException; } }
    private Exception _InnerException;

    /// <summary>
    /// Сюда должен быть помещен объект исключения
    /// </summary>
    public Exception Exception { get { return _Exception; } set { _Exception = value; } }
    private Exception _Exception;

    #endregion
  }

  /// <summary>
  /// Делегат события DBx.CreateSqlException
  /// </summary>
  /// <param name="sender">Объект DBx</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DBxCreateSqlExceptionEventHandler(object sender, DBxCreateSqlExceptionEventArgs args);

  /// <summary>
  /// Аргументы события DBx.SqlQueryStarted
  /// </summary>
  public class DBxSqlQueryStartedEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает аргументы события
    /// </summary>
    /// <param name="cmdText"></param>
    public DBxSqlQueryStartedEventArgs(string cmdText)
    {
      _CmdText = cmdText;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выполняемый запрос
    /// </summary>
    public string CmdText { get { return _CmdText; } }
    private string _CmdText;

    #endregion
  }

  /// <summary>
  /// Делегат для события DBx.SqlQueryStarted
  /// </summary>
  /// <param name="sender">Объект DBx</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DBxSqlQueryStartedEventHandler(object sender, DBxSqlQueryStartedEventArgs args);


  /// <summary>
  /// Аргументы события DBx.SqlQueryFinished
  /// </summary>
  public class DBxSqlQueryFinishedEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает аргументы события
    /// </summary>
    /// <param name="cmdText"></param>
    /// <param name="executingTime"></param>
    /// <param name="exception"></param>
    public DBxSqlQueryFinishedEventArgs(string cmdText, TimeSpan executingTime, Exception exception)
    {
      _CmdText = cmdText;
      _ExecutingTime = executingTime;
      _Exception = exception;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выполненный запрос
    /// </summary>
    public string CmdText { get { return _CmdText; } }
    private string _CmdText;

    /// <summary>
    /// Время выполнения запроса (для отладки)
    /// </summary>
    public TimeSpan ExecutingTime { get { return _ExecutingTime; } }
    private TimeSpan _ExecutingTime;

    /// <summary>
    /// Объект исключения, если при выполнении запроса возникла ошибка.
    /// Возвращает null, если запрос успешно выполнен
    /// </summary>
    public Exception Exception { get { return _Exception; } }
    private Exception _Exception;

    #endregion
  }

  /// <summary>
  /// Делегат для события DBx.SqlQueryFinished
  /// </summary>
  /// <param name="sender">Объект DBx</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DBxSqlQueryFinishedEventHandler(object sender, DBxSqlQueryFinishedEventArgs args);

  /// <summary>
  /// Аргумент события DBx.LogoutException
  /// </summary>
  public class DBxLogoutExceptionEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект исключения
    /// </summary>
    /// <param name="con">Соединение с базой данной, для которого произошло исключение</param>
    /// <param name="exception">Объект ошибки</param>
    /// <param name="title">Описание места возникновения ошибки</param>
    public DBxLogoutExceptionEventArgs(DBxConBase con, Exception exception, string title)
    {
      _Con = con;
      _Exception = exception;
      _Title = title;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Соединение с базой данной, для которого произошло исключение
    /// </summary>
    public DBxConBase Con { get { return _Con; } }
    private DBxConBase _Con;

    /// <summary>
    /// Объект ошибки
    /// </summary>
    public Exception Exception { get { return _Exception; } }
    private Exception _Exception;

    /// <summary>
    /// Описание места возникновения ошибки
    /// </summary>
    public string Title { get { return _Title; } }
    private string _Title;

    #endregion
  }

  /// <summary>
  /// Делегат события DBx.LogoutException
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void DBxLogoutExceptionEventHandler(object sender, DBxLogoutExceptionEventArgs args);

  #endregion

  #region Перечисление DBxLockMode

  /// <summary>
  /// Ограничения на блокировку.
  /// Значение возвращается свойством DBx.LockMode
  /// </summary>
  public enum DBxLockMode
  {
    /// <summary>
    /// Нет ограничений.
    /// Одновременно может выполняться множество операторов записи
    /// </summary>
    None,

    /// <summary>
    /// Одновременно может выполняться только одна запись и сколько угодно операторов чтения
    /// </summary>
    SingleWrite
  }

  #endregion

  /// <summary>
  /// Параметры обновления структуры базы данных.
  /// Передается в качестве аргумента методу DBx.UpdateStruct()
  /// </summary>
  public sealed class DBxUpdateStructOptions
  {
    #region Конструктор

    /// <summary>
    /// Инициализация набора параметров знвчениями по умолчанию
    /// </summary>
    public DBxUpdateStructOptions()
    {
      _ForeignKeys = true;
      _DropUnusedIndices = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Нужно ли создавать внешние ключи для ссылочных полей.
    /// По умолчанию - true - ключи создаются.
    /// Свойство может быть сброшено в false на время импорта данных, чтобы избежать проблем ссылочной 
    /// целостности. После импорта объект DBx должен быть удален и создан заново с обновлением структуры, 
    /// уже с включенным режимом
    /// </summary>
    public bool ForeignKeys { get { return _ForeignKeys; } set { _ForeignKeys = value; } }
    private bool _ForeignKeys;

    /// <summary>
    /// Нужно ли удалять индексы, описаний которых нет в требуемой DBxStruct.
    /// По умолчанию - true - лишние индексы удаляются.
    /// Свойство должно быть сброшено в false, если предполагается возможность использования индексов,
    /// создаваемых вне приложения.
    /// </summary>
    public bool DropUnusedIndices { get { return _DropUnusedIndices; } set { _DropUnusedIndices = value; } }
    private bool _DropUnusedIndices;

    #endregion
  }

  /// <summary>
  /// База данных.
  /// Абстрактный класс
  /// </summary>
  public abstract class DBx : DisposableObject
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Создает базу данных и добавляет ее в список AllDBList.
    /// </summary>
    public DBx()
    {
      _Cons = new SyncCollection<DBxConBase>();
      AllDBList.Add(this);
      lock (AllDBList.SyncRoot)
      {
        DBxTools.InitLogout();

        // Отображаемое имя по умолчанию
        _DisplayName = "DB #" + AllDBList.Count.ToString();
      }

      _CommandTimeout = DefaultCommandTimeout;

      _UpdateTableLockObjects = new Dictionary<string, object>();
    }


    /// <summary>
    /// Удаляет базу данных из списка AllDBList, если <paramref name="disposing"/>=true.
    /// </summary>
    /// <param name="disposing">True, если был вызван Dispose() для базы данных.
    /// False, если был вызван деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
        AllDBList.Remove(this);
      base.Dispose(disposing);
    }

    #endregion

    #region Отображаемое название

    /// <summary>
    /// Отображаемое имя базы данных
    /// </summary>
    public string DisplayName
    {
      get
      {
        lock (_Cons.SyncRoot)
        {
          return _DisplayName;
        }
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException();
        lock (_Cons.SyncRoot)
        {
          _DisplayName = value;
        }
      }
    }
    private string _DisplayName;

    /// <summary>
    /// Возвращает свойство DisplayName и признак "(disposed)"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (IsDisposed)
        return DisplayName + " (disposed)";
      else
        return DisplayName;
    }

    #endregion

    #region Список соединений

    /// <summary>
    /// Список всех открытых соединений
    /// </summary>
    public ICollection<DBxConBase> Cons { get { return _Cons; } }
    internal SyncCollection<DBxConBase> _Cons;

    /// <summary>
    /// Свойство возвращает true, если для базы данных, хотя бы однажды, было создано соединение
    /// </summary>
    public bool HasConnected { get { return _HasConnected; } }
    internal bool _HasConnected;

    /// <summary>
    /// Генерирует исключение, если для базы данных, хотя бы однажды, было создано соединение
    /// </summary>
    public void CheckHasNotConnected()
    {
      if (_HasConnected)
        throw new InvalidOperationException("С базой данных уже было установлено соединение");
    }

    #endregion

    #region Подключения

    /// <summary>
    /// Основная точка входа, позволяющая выполнять все действия
    /// </summary>
    public DBxEntry MainEntry
    {
      get
      {
        lock (_Cons.SyncRoot)
        {
          return _MainEntry;
        }
      }
      internal set // Устанавливается только в конструкторе DBxEntry
      {
        lock (_Cons.SyncRoot)
        {
          _MainEntry = value;
          if (_Struct == null && value != null)
            _Struct = new DBxStruct(new DBxRealStructSource(value));
        }
      }
    }
    private DBxEntry _MainEntry;

    /// <summary>
    /// Создает дополнительную точку входа с заданымми разрешениями
    /// </summary>
    /// <param name="dbPermissions">Разрешения на базу данных, таблицы и поля</param>
    /// <returns>Точка входа для создания подключений</returns>
    public abstract DBxEntry CreateEntry(DBxPermissions dbPermissions);

    /// <summary>
    /// Возвращает строку подключения, не содержащую пароль.
    /// Если свойство MainEntry=null, возвращает пустую строку.
    /// Если строки подключения не поддерживается базой данных, возвращает пустую строку.
    /// </summary>
    public string UnpasswordedConnectionString
    {
      get
      {
        DBxEntry en = MainEntry;
        if (en == null)
          return String.Empty;
        else
          return MainEntry.UnpasswordedConnectionString;
      }
    }

    #endregion

    #region Структура таблиц

    /// <summary>
    /// Структура базы данных (описания таблиц).
    /// Если свойство не установлено в явном виде, возвращает реальную структуру таблиц.
    /// Для обновления структуры базы данных следует установить это свойство и вызвать DBxEntry.UpdateStruct().
    /// Установка свойства устанавливает DBxStruct.IsReadOnly=true
    /// </summary>
    public DBxStruct Struct
    {
      get
      {
        lock (_Cons.SyncRoot)
        {
          return _Struct;
        }
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException();

        value.SetReadOnly(); // 18.02.2019
        value.CheckStruct(this); // 18.02.2019

        lock (_Cons.SyncRoot)
        {
          if (_Cons.Count > 0)
            throw new InvalidOperationException("Нельзя задать структуру, если есть установленные соединения");

          _Struct = value;
          _StructHasBeenSet = true;
        }
      }
    }
    private DBxStruct _Struct;

    /// <summary>
    /// Возвращает true, если свойство Struct было установлено.
    /// Если false, то Struct возвращает реальную структуру базы данных
    /// </summary>
    public bool StructHasBeenSet { get { return _StructHasBeenSet; } }
    private bool _StructHasBeenSet;

    /// <summary>
    /// Восстанавливает свойство Struct в значение по умолчанию, то есть отображающее реальную структуру таблиц в базе данных.
    /// Может быть полезным, если таблицы создаются/удаляются/модифицируются вручную
    /// </summary>
    public void ResetStruct()
    {
      lock (_Cons.SyncRoot)
      {
        _Struct = GetRealStruct();
        _StructHasBeenSet = false;
        _StructHasBeenUpdated = false;
      }
    }

    /// <summary>
    /// Возвращает новый объект DBxStruct, соответствующий реальной структуре базы данных
    /// </summary>
    /// <returns></returns>
    public DBxStruct GetRealStruct()
    {
      if (_MainEntry == null)
        return null;
      else
        return new DBxStruct(new DBxRealStructSource(_MainEntry));
    }

    #endregion

    #region Инициализация базы данных

    /// <summary>
    /// Свойство возвращает true, если база данных существует (для SQL Server Express - и присоединена к серверу)
    /// </summary>
    public abstract bool DatabaseExists { get; }

    /// <summary>
    /// Метод должен создать пустую базу данных и присоединить ее к серверу, если ее не существует
    /// </summary>
    public abstract void CreateIfRequired();

    /// <summary>
    /// Возвращает true, если был успешный вызов метода UpdateStruct().
    /// Вызов ResetDBStruct() сбрасывает флаг в false.
    /// </summary>
    public bool StructHasBeenUpdated { get { return _StructHasBeenUpdated; } }
    private bool _StructHasBeenUpdated;

    /// <summary>                               
    /// Метод должен выполнить обновление структуры существующей базы
    /// данных на основании созданного описание в свойстве DBx.Struct.
    /// На момент вызова база данных (возможно, пустая) должна существовать.
    /// Эта версия позволяет настроить параметры обновления.
    /// </summary>
    /// <param name="splash">Здесь устанавливается свойство PhaseText для отображения выполненямых действий</param>
    /// <param name="errors">Сюда помещаются предупреждения и информационные сообщения. Если никаких изменений
    /// не вносится не вносится, сообщения не добавляются</param>
    /// <param name="options">Опции обновления</param>
    /// <returns>true, если в базу данных были внесены изменения</returns>
    public bool UpdateStruct(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options)
    {
      if (!StructHasBeenSet)
        throw new InvalidOperationException("StructHasBeenSet=false");
      bool res = OnUpdateStruct(splash, errors, options);
      _StructHasBeenUpdated = true;
      return res;
    }

    /// <summary>                               
    /// Метод должен выполнить обновление структуры существующей базы
    /// данных на основании созданного описание в свойстве DBx.Struct.
    /// На момент вызова база данных (возможно, пустая) должна существовать.
    /// Эта версия позволяет настроить параметры обновления.
    /// </summary>
    /// <param name="splash">Здесь устанавливается свойство PhaseText для отображения выполненямых действий</param>
    /// <param name="errors">Сюда помещаются предупреждения и информационные сообщения. Если никаких изменений
    /// не вносится не вносится, сообщения не добавляются</param>
    /// <param name="options">Опции обновления</param>
    /// <returns>true, если в базу данных были внесены изменения</returns>
    protected abstract bool OnUpdateStruct(ISplash splash, ErrorMessageList errors, DBxUpdateStructOptions options);


    /// <summary>
    /// Метод должен выполнить обновление структуры существующей базы
    /// данных на основании созданного описание в свойстве DBx.Struct.
    /// На момент вызова база данных (возможно, пустая) должна существовать.
    /// Эта перегрузка использует параметры обновления по умолчанию, которые применимы в большинстве случаев
    /// </summary>
    /// <param name="splash">Здесь устанавливается свойство PhaseText для отображения выполненямых действий</param>
    /// <param name="errors">Сюда помещаются предупреждения и информационные сообщения. Если никаких изменений
    /// не вносится не вносится, сообщения не добавляются</param>
    /// <returns>true, если в базу данных были внесены изменения</returns>
    public bool UpdateStruct(ISplash splash, ErrorMessageList errors)
    {
      return UpdateStruct(splash, errors, new DBxUpdateStructOptions());
    }

    /// <summary>
    /// Обновление структуры существующей базы данных на основании созданного описание в свойстве DBx.Struct.
    /// На момент вызова база данных (возможно, пустая) должна существовать.
    /// Эта перегрузка редко используется, так как не позволяет получить список ошибок.
    /// </summary>
    /// <returns>true, если в базу данных были внесены изменения</returns>
    public bool UpdateStruct()
    {
      ISplash Splash = new DummySplash();
      ErrorMessageList Errors = new ErrorMessageList();
      return UpdateStruct(Splash, Errors, new DBxUpdateStructOptions());
    }

    /// <summary>
    /// Удаление базы данных, если она существует
    /// </summary>
    /// <returns>True, если существующая база данных была удалена.
    /// False, если база данных не зарегистрирована</returns>
    public abstract bool DropDatabaseIfExists();

    /// <summary>
    /// Удаляет таблицу данных, если она существует.
    /// Этот метод должен вызываться до установки свойства DBx.Struct и вызова UpdateStruct().
    /// Если обновление структуры не предполагается, после последовательности вызовов этого метода,
    /// должна быть выполнена установка DB.Struct=null, чтобы обновить список таблиц
    /// </summary>
    /// <param name="tableName">Имя удаляемой таблицы</param>
    public virtual void DropTableIfExists(string tableName)
    {
      if (this.Struct.Tables.Contains(tableName))
        DropTable(tableName);
    }

    /// <summary>
    /// Удаляет таблицу из базы данных.
    /// Таблица должна сушествовать.
    /// </summary>
    /// <param name="tableName">Имя удаляемой таблицы</param>
    public virtual void DropTable(string tableName)
    {
      using (DBxConBase Con = MainEntry.CreateCon())
      {
        Con.NameCheckingEnabled = false;
        Con.Validator.CheckTableName(tableName, DBxAccessMode.Full);
        DBxSqlBuffer Buffer = new DBxSqlBuffer(this.Formatter);
        Buffer.SB.Append("DROP TABLE ");
        Buffer.FormatTableName(tableName);
        Con.SQLExecuteNonQuery(Buffer.SB.ToString());
      }
    }

    #endregion

    #region Проверка имен

    /// <summary>
    /// Проверка корректности имени таблицы.
    /// Возвращает true, если имя правильное. 
    /// Наличие реальной таблицы с таким именем не проверяется
    /// </summary>
    /// <param name="tableName">Проверяемое имя таблицы</param>
    /// <param name="errorText">Сообщение об ошибке</param>
    /// <returns>true, если имя правильное</returns>
    public virtual bool IsValidTableName(string tableName, out string errorText)
    {
      if (String.IsNullOrEmpty(tableName))
      {
        errorText = "Имя таблицы не задано";
        return false;
      }

      return CheckInvalidChars(tableName, out errorText);
    }

    /// <summary>
    /// Проверка корректности имени поля, которое (возможно) содержит точки, задающее ссылочное поле.
    /// Наличие реального поля не проверяется
    /// </summary>
    /// <param name="columnName">Проверяемое имя поля</param>
    /// <param name="allowDots">True, если имя может содержать точки</param>
    /// <param name="errorText"></param>
    /// <returns></returns>
    public bool IsValidColumnName(string columnName, bool allowDots, out string errorText)
    {
      if (String.IsNullOrEmpty(columnName))
      {
        errorText = "Имя поля не задано";
        return false;
      }

      if (allowDots && columnName.IndexOf('.') >= 0)
      {
        string[] a = columnName.Split('.');
        for (int i = 0; i < a.Length; i++)
        {
          if (!IsValidColumnName(a[i], out errorText))
          {
            errorText = "Неправильная составная часть имени поля с точками №" + (i + 1).ToString() + ". " + errorText;
            return false;
          }
        }
        errorText = null;
        return true;
      }
      else
        return IsValidColumnName(columnName, out errorText);
    }

    /// <summary>
    /// Проверка корректности имени поля.
    /// Проверяется только наличие недопустимых символов, а не принадлежность поля к таблице.
    /// Точка "." не является допустимым символом.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <param name="errorText">Сюда помещается сообщение, если имя поля не корректно</param>
    /// <returns>true, если имя поля является допустимым</returns>
    protected virtual bool IsValidColumnName(string columnName, out string errorText)
    {
      if (String.IsNullOrEmpty(columnName))
      {
        errorText = "Имя столбца не задано";
        return false;
      }

      return CheckInvalidChars(columnName, out errorText);
    }


    //private static readonly CharArrayIndexer InvalidNameChars = new CharArrayIndexer(":;<=>?@[\\]{|}~+-*/\"\'");

    //private static bool CheckInvalidChars(string name, out string errorText)
    //{
    //  for (int i = 0; i < name.Length; i++)
    //  {
    //    if ((name[i] < '0') || (InvalidNameChars.IndexOf(name[i]) >= 0))
    //    {
    //      errorText = "Недопустимый символ \"" + name[i] + "\" в позиции " + (i + 1).ToString();
    //      return false;
    //    }
    //  }

    //  errorText = null;
    //  return true;
    //}

    // 16.10.2019
    // Ужесточили проверку
    private static bool CheckInvalidChars(string name, out string errorText)
    {
      for (int i = 0; i < name.Length; i++)
      {
        if (!Char.IsLetterOrDigit(name, i))
        {
          if (name[i] == '_')
            continue; // допускается

          errorText = "Недопустимый символ \"" + name[i] + "\" в позиции " + (i + 1).ToString();
          return false;
        }
      }
      if (Char.IsDigit(name, 0))
      {
        errorText = "Имя не может начинаться с цифры";
        return false;
      }

      errorText = null;
      return true;
    }

    #endregion

    #region Форматирование SQL-запросов

    /// <summary>
    /// Объект для форматирования SQL-запросов
    /// Задается в конструкторе базы данных
    /// Используется единственный экземпляр для всех соединений.
    /// У каждого соединения есть свой экземпляр DBxSqlBuffer
    /// </summary>
    public DBxSqlFormatter Formatter { get { return _Formatter; } }
    private DBxSqlFormatter _Formatter;

    /// <summary>
    /// Метод должен быть вызван в конструкторе базы данных
    /// </summary>
    /// <param name="formatter">Форматировщик</param>
    protected void SetFormatter(DBxSqlFormatter formatter)
    {
#if DEBUG
      if (_Formatter != null)
        throw new InvalidOperationException("Повторная установка форматировщика не допускается");
#endif
      _Formatter = formatter;
    }

    #endregion

    #region Блокировка таблиц

    /// <summary>
    /// Коллекция объектов для блокирования таблиц с помощью DBxConBase.Begin/EndUpdate().
    /// Ключ - имя таблицы
    /// Значение - объект, для которого вызывается Monitor.Enter() и Exit().
    /// </summary>
    private Dictionary<string, object> _UpdateTableLockObjects;

    /// <summary>
    /// Получить объект, используемый для блокировки таблиц.
    /// Метод является потокобезопасным
    /// </summary>
    /// <param name="tableName">Имя таблиц</param>
    /// <returns>Объект блокировки</returns>
    internal object GetUpdateTableLockObject(string tableName)
    {
      object LockObj;
      lock (_UpdateTableLockObjects)
      {
        if (!_UpdateTableLockObjects.TryGetValue(tableName, out LockObj))
        {
          LockObj = new object();
          _UpdateTableLockObjects.Add(tableName, LockObj);
        }
      }
      return LockObj;
    }

    #endregion

    #region Регистрация ошибок

    /// <summary>
    /// Обработчик может создавать собственное исключение
    /// Событие может вызываться асинхронно
    /// </summary>
    public event DBxCreateSqlExceptionEventHandler CreateSqlException;

    /// <summary>
    /// Внутренний счетчик для нумерации исключений в пределах базы данных
    /// </summary>
    private int _ExceptionCount;

    /// <summary>
    /// Вызывает обработчик события CreateSqlException, если он установлен.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    public void OnCreateSqlException(DBxCreateSqlExceptionEventArgs args)
    {
      DBxCreateSqlExceptionEventHandler ehCreateSqlException = CreateSqlException; // 12.01.2021. Учитываем возможность асинхронного присоединения и отсоединения обработчиков событий
      if (ehCreateSqlException != null)
        ehCreateSqlException(this, args);

      if (args.Exception == null)
        args.Exception = args.InnerException;

      args.Exception.Data["SQL"] = args.CmdText;
      args.Exception.Data["DB.DisplayName"] = this.DisplayName;
      int ExNo = Interlocked.Increment(ref _ExceptionCount);
      args.Exception.Data["DB.ExceptionSerialNumber"] = ExNo; // 19.09.2019
      if (!String.IsNullOrEmpty(args.InnerException.StackTrace))
        args.Exception.Data["OriginalStackTrace"] = args.InnerException.StackTrace; // 09.06.2018
      args.Exception.Data["DBx.OnCreateSqlException.StackTrace"] = Environment.StackTrace; // 20.11.2019
    }

    /// <summary>
    /// Обработчик для регистрации внутренних ошибок.
    /// Событие может вызываться асинхронно
    /// Если обработчик события не установлен приложением, вызывается метод LogoutTools.LogoutException()
    /// </summary>
    public event DBxLogoutExceptionEventHandler LogoutException;

    /// <summary>
    /// Возвращает количество обрабатываемых в данный момент ошибок.
    /// Возвращаемое значение увеличивается на 1 при вызове OnLogoutException() и уменьшается на 1 при выходе из метода.
    /// </summary>
    public int CurrentLogoutExceptionCount { get { return _CurrentLogoutExceptionCount; } }
    private int _CurrentLogoutExceptionCount;

    /// <summary>
    /// Вызов обработчика события LogoutException, если он установлен.
    /// Иначе вызывается метод LogoutTools.LogoutException для регистрации исключения.
    /// Вложенные ошибки при вызове обработчика события "проглатываются".
    /// </summary>
    /// <param name="args">Аргументы события</param>
    public void OnLogoutException(DBxLogoutExceptionEventArgs args)
    {
      Interlocked.Increment(ref _CurrentLogoutExceptionCount);
      try
      {
        DBxLogoutExceptionEventHandler elLogoutException = LogoutException; // 12.01.2021. Учитываем возможность асинхронного присоединения и отсоединения обработчиков событий
        if (elLogoutException != null)
        {
          try
          {
            elLogoutException(this, args);
          }
          catch
          {
          }
        }
        else
        {
          if (_CurrentLogoutExceptionCount <= 2)
            LogoutTools.LogoutException(args.Exception, args.Title);
          else // 19.09.2019
            args.Exception.Data["DBx.OnLogoutException()"] = "Вывод ошибки в log-файл отменен для предотвращения исчерпания ресурсов, так как в текущий момент в log-файлы выводятся другие ошибки";
        }
      }
      finally
      {
        Interlocked.Decrement(ref _CurrentLogoutExceptionCount);
      }
    }

    #endregion

    #region Прочее

    /// <summary>
    /// Текстовое представление версии сервера, например "Microsoft SQL Server 10 Express Edition"
    /// </summary>
    public abstract string ServerVersionText { get; }

    /// <summary>
    /// Имя базы данных, если применимо для провайдера, иначе пустая строка
    /// </summary>
    public virtual string DatabaseName { get { return String.Empty; } }

    /// <summary>
    /// Идентификатор базы данных.
    /// Используется при работе с буфером обмена в нескольких программах, чтобы определить, что данные
    /// относятся к одному набору данных, а не к разным.
    /// Непереопределенный метод возвращает строку "ИмяКомпьютера|ИмяБазыДанных"
    /// </summary>
    public virtual string DBIdentity
    {
      get { return Environment.MachineName + "|" + DatabaseName; }
    }

    /// <summary>
    /// Способ установки блокировок.
    /// Непереопределенное свойство возвращает None, так как большинство баз данных способны выполнять запросы параллельно
    /// </summary>
    public virtual DBxLockMode LockMode
    {
      get { return DBxLockMode.None; }
    }

    /// <summary>
    /// Время выполнения команд в секундах по умолчанию. 
    /// Начальное значение для свойства CommandTimeOut.
    /// Равно 30 секугдам
    /// </summary>
    public const int DefaultCommandTimeout = 30;

    /// <summary>
    /// Время выполнения команд в секундах. 0-бесконечное время ожидания.
    /// По умолчанию свойство имеет значение 30 (в соответствии с принятым стандартным значением в IDbCommand).
    /// Установка этого свойства влияет на все вновь создаваемые соединения и позволяет устанавливать
    /// тайм-аут на уровне всей базы данных.
    /// Свойство может задаваться индивидуально для каждого соединения DBxConBase.
    /// </summary>
    public int CommandTimeout
    {
      get { return _CommandTimeout; }
      set
      {
        if (_CommandTimeout < 0)
          throw new ArgumentException("Значение не может быть отрицательным");
        _CommandTimeout = value;
      }
    }
    private int _CommandTimeout;

    /// <summary>
    /// Метод должен вернуть true, если данное исключение означает, что превышен лимит на размер базы данных
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public virtual bool IsDBSizeLimitExceededException(Exception e)
    {
      return false;
    }

    /// <summary>
    /// Возвращает текущий размер базы данных в байтах.
    /// </summary>
    /// <returns>Размер всех файлов</returns>
    public abstract long GetDBSize();

    /// <summary>
    /// Значение свойство DBSizeLimit, когда размер не ограничен
    /// </summary>
    public const long DBSizeUnlimited = 0x7FFFFFFFFFFFFFFFL;

    /// <summary>
    /// Возвращает максимальный размер базы данных в байтах.
    /// Это ограничение связано с лецинзионными ограничениями (например, для MS SQL Server 2008R2 Express Edition равно 8Гб) 
    /// или техническими ограничениями (для MS Access равно 2Гб), а не доступным местом на диске.
    /// Если нет установленного ограничения, возвращает константу DBSizeUnlimited
    /// </summary>
    public virtual long DBSizeLimit { get { return DBSizeUnlimited; } }

    /// <summary>
    /// Возвращает объект DBxManager для данного типа базы данных.
    /// Если менеджера нет, свойство возвращает null.
    /// </summary>
    public virtual DBxManager Manager { get { return null; } }

    /// <summary>
    /// Возвращает объект DbProviderFactory, специфический для провайдера ADO.NET
    /// </summary>
    public abstract DbProviderFactory ProviderFactory { get; }

    #endregion

    #region Статический список

    /// <summary>
    /// Полный список всех объявленных баз данных
    /// </summary>
    public static readonly SyncCollection<DBx> AllDBList = new SyncCollection<DBx>();

    #endregion

    #region Трассировка

    /// <summary>
    /// Событие вызывается перед выполнением SQL-запроса.
    /// Обработчик события может использоваться, например, для нестандартной трассировки запросов.
    /// Обработчик должен выполняться быстро, чтобы не замедлять выполнение запросов.
    /// Событие может вызываться асинхронно.
    /// </summary>
    public event DBxSqlQueryStartedEventHandler SqlQueryStarted;

    internal void OnSqlQueryStarted(DBxSqlQueryStartedEventArgs args)
    {
      if (SqlQueryStarted != null)
        SqlQueryStarted(this, args);
    }

    /// <summary>
    /// Событие вызывается после выполнения SQL-запроса, независимо от того, выполнен запрос успешно или возникло исключение.
    /// Обработчик события может использоваться, например, для нестандартной трассировки запросов.
    /// Обработчик должен выполняться быстро, чтобы не замедлять выполнение запросов.
    /// Событие может вызываться асинхронно. 
    /// Порядок вызова событий SqlQueryFinished не обязан совпадать с SqlQueryStarted, так как запросы могут выполняться параллельно с разной скоростью.
    /// Если запрос завершился с ошибкой, то сначала происходит событие SqlQueryFinished, а затем - LogoutException.
    /// </summary>
    public event DBxSqlQueryFinishedEventHandler SqlQueryFinished;

    internal void OnQueryFinished(DBxSqlQueryFinishedEventArgs args)
    {
      if (SqlQueryFinished != null)
        SqlQueryFinished(this, args);
    }

    /// <summary>
    /// Управляет трассировкой SQL-запросов на глобальном уровне.
    /// Трассировкой запросов можно также управлять на уровне базы данных с помощью нестатического свойства DBx.TraceEnabled
    /// и на уровне соединения с помощью свойства DBxConBase.TraceEnabled.
    /// 
    /// По умолчанию, глобальная трассировка отключена.
    /// </summary>
    public static readonly System.Diagnostics.BooleanSwitch TraceSwitch = new System.Diagnostics.BooleanSwitch("TraceSQL", "Трассировка SQL-запросов");

    /// <summary>
    /// Управление трассировкой SQL-запросов на уровне базы.
    /// Если свойство не установлено в явном виде для базы данных, то оно возвращет текущее значение в соответствии с глобальным переключателем TraceSwitch.
    /// Управление трассировкой возможно также на уровне отдельных соединений с помощью свойства DBxCon/DBxConBase.TraceEnabled
    /// </summary>
    public bool TraceEnabled { get { return _TraceEnabled ?? TraceSwitch.Enabled; } set { _TraceEnabled = value; } }
    private bool? _TraceEnabled;

    /// <summary>
    /// Восстановление признака трассировки SQL-запросов в значение по умолчанию
    /// </summary>
    public void ResetTraceEnabled()
    {
      _TraceEnabled = null;
    }

    #endregion
  }
}
