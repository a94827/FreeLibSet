// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.IO;
using FreeLibSet.Config;
using FreeLibSet.Caching;
using FreeLibSet.Remoting;
using System.Diagnostics;

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// Аргументы события для восстановления подключения к серверу
  /// </summary>
  public class DBxRetriableExceptionEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Объекты не создаются пользовательским кодом
    /// </summary>
    /// <param name="exception">Объект исключения</param>
    /// <param name="repeatCount">Количество повторов</param>
    public DBxRetriableExceptionEventArgs(Exception exception, int repeatCount)
    {
      if (exception == null)
        throw new ArgumentNullException("exception");
      _Exception = exception;
      _RepeatCount = repeatCount;
      _Retry = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возникшее исключение.
    /// Пользовательский обработчик обязательно должен проверить, что исключение связано с подключением к серверу
    /// </summary>
    public Exception Exception { get { return _Exception; } }
    private Exception _Exception;

    /// <summary>
    /// Счетчик повторных вызовов.
    /// При возникновении исключения свойство имеет значение 0. Если в том же методе возникает повторное
    /// исключение, свойство возвразает 1, и т.д.
    /// Пользовательский обработчик может, например, выполнять восстановление соединение только при первом
    /// вызове.
    /// Счетчик относится к конкретному вызову метода, а не к соединению вообще.
    /// </summary>
    public int RepeatCount { get { return _RepeatCount; } }
    private int _RepeatCount;

    /// <summary>
    /// Это свойство должно быть установлено в true, если соединение с сервером восстановлено и следует
    /// повторить попытку вызвать метод сервера
    /// </summary>
    public bool Retry { get { return _Retry; } set { _Retry = value; } }
    private bool _Retry;

    #endregion
  }

  /// <summary>
  /// Тип события DBxDocProvider.ExceptionCaught
  /// </summary>
  /// <param name="sender">Ссылка на DBxChainDocProvider.</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DBxRetriableExceptionEventHandler(object sender, DBxRetriableExceptionEventArgs args);

  /// <summary>
  /// Данные, необходимые для организации цепочки провайдеров.
  /// Требуется конструктору DBxChainDocProvider. Для получения прокси требуется вызов DBxDocProvider.CreateProxy().
  /// Передается по сети как сериализуемый объект.
  /// Содержит ссылку родительский провайдер (производный от MasterByRefObject) и фиксированные данные.
  /// Не содержит общедоступных свойств и методов
  /// </summary>
  [Serializable]
  public sealed class DBxDocProviderProxy
  {
    #region Защищенный конструктор

    internal DBxDocProviderProxy(DBxDocProvider source, NamedValues fixedInfo)
    {
      _Source = source;
      _FixedInfo = fixedInfo;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер-источник, для которого был вызван метод CreateProxy()
    /// </summary>
    public DBxDocProvider Source { get { return _Source; } }
    private DBxDocProvider _Source;

    internal NamedValues FixedInfo { get { return _FixedInfo; } }
    private NamedValues _FixedInfo;

    #endregion
  }

  /// <summary>
  /// Базовый класс для реализации цепочек провайдеров.
  /// Присоединяется к провайдеру-источнику. Источник может быть в текущем AppDomain или доступным через Remoting.
  /// Реализует хранение кэша DBxCache и кэша двоичных данных, если источник данных является удаленным.
  /// Класс DBxChainDocProvider сам по себе является потокобезопасным, но использование объекта может быть искусственно
  /// огранчиено одним потоком, если задан флаг в конструкторе.
  /// </summary>
  public class DBxChainDocProvider : DBxDocProvider
  {
    #region Конструктор

    /// <summary>
    /// Создает цепочечный провайдер
    /// </summary>
    /// <param name="sourceProxy">Результат вызова DBxDocProvider.CreateProxy() для предыдущего провайдера в цепочке</param>
    /// <param name="currentThreadOnly">Если true, то вызовы нового провайдера будут разрешены только из текущего потока</param>
    public DBxChainDocProvider(DBxDocProviderProxy sourceProxy, bool currentThreadOnly)
      : base(sourceProxy.FixedInfo, currentThreadOnly)
    {
      _Source = sourceProxy.Source;
      _SourceIsRemote = System.Runtime.Remoting.RemotingServices.IsTransparentProxy(sourceProxy.Source);

      //ServerTimeDiff = Source.ServerTime - DateTime.Now;
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Исходный провайдер, выполняющий основной объем действий
    /// </summary>
    protected DBxDocProvider Source { get { return _Source; } }
    private DBxDocProvider _Source;

    /// <summary>
    /// Возвращает true, если провайдер-источник является удаленным объектом (TransparentProxy).
    /// В этом случае DBxChainDocProvider использует собственную копию DBxCache
    /// </summary>
    public bool SourceIsRemote { get { return _SourceIsRemote; } }
    private bool _SourceIsRemote;

    #endregion

    #region Методы и свойства, реализуемые головным DocProvider

    /// <summary>
    /// Загрузить документы (без поддокументов)
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="docIds">Массив идентификаторов</param>
    /// <returns>Таблица документов</returns>
    protected override DataTable DoLoadDocData(string docTypeName, Int32[] docIds)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.LoadDocData(docTypeName, docIds);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Загрузить документы (без поддокументов)
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="filter">Условия для отбора документов</param>
    /// <returns>Таблица документов</returns>
    protected override DataTable DoLoadDocData(string docTypeName, DBxFilter filter)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.LoadDocData(docTypeName, filter);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Загрузить поддокументы.
    /// Предполагается, что таблица документов уже загружена
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документов</param>
    /// <param name="subDocTypeName">Имя таблицы поддокументов</param>
    /// <param name="docIds">Массив идентификаторов документов, для которых загружаются поддокументы</param>
    /// <returns>Таблица поддокументов</returns>
    protected override DataTable DoLoadSubDocData(string docTypeName, string subDocTypeName, Int32[] docIds)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.LoadSubDocData(docTypeName, subDocTypeName, docIds);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Применение изменений.
    /// Выполняется создание, изменение и удаление документов и поддокументов
    /// </summary>
    /// <param name="dataSet">Набор данных</param>
    /// <param name="reloadData">Если true, то будет возвращен тот же набор данных.
    /// В нем фиктивные идентификаторы новых документов и поддокументов будут заменены на реальные,
    /// а перекрестные ссылки на них исправлены. Это требуется в интерфейсе пользователя, когда
    /// пользователь нажимает кнопку "Применить", чтобы можно было продолжить сеанс редактирования.
    /// Если false, то исправленный набор данных не возвращается</param>
    /// <returns>Набор с исправленными ссылками или null</returns>
    protected override DataSet DoApplyChanges(DataSet dataSet, bool reloadData)
    {
      CheckThread();

      return Source.ApplyChanges(dataSet, reloadData);
    }

    /// <summary>
    /// Получить таблицу версий строк документа.
    /// Возвращает таблицу с одной строкой
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <param name="docVersion">Версия документа</param>
    /// <returns>Таблица</returns>
    protected override DataTable DoLoadDocDataVersion(string docTypeName, Int32 docId, int docVersion)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.LoadDocDataVersion(docTypeName, docId, docVersion);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить таблицу версий строк поддокументов.
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="subDocTypeName">Имя таблицы поддокументов</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <param name="docVersion">Версия документа</param>
    /// <returns>Таблица</returns>
    protected override DataTable DoLoadSubDocDataVersion(string docTypeName, string subDocTypeName, Int32 docId, int docVersion)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.LoadSubDocDataVersion(docTypeName, subDocTypeName, docId, docVersion);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Возвращает все таблицы данных по документу. Возвращает таблицы документов (с одной строкой) и поддокументов,
    /// включая удаленные записи. В набор добавляются строки из таблиц UserActions и DocActions.
    /// В набор добавляются таблицы документов из базы данных истории. Чтобы отличить их от основных
    /// документов, перед именами таблиц добавляется префикс "Undo_".
    /// Метод предназначен для отладочных целей
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <returns>Набор таблиц</returns>
    protected override DataSet DoLoadUnformattedDocData(string docTypeName, Int32 docId)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.LoadUnformattedDocData(docTypeName, docId);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }


    /// <summary>
    /// Выполнение SQL-запроса SELECT с заданием всех возможных параметров
    /// </summary>
    /// <param name="info">Параметры для запроса</param>
    /// <returns>Таблица данных</returns>
    protected override DataTable DoFillSelect(DBxSelectInfo info)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.FillSelect(info);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Найти запись
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие отбора</param>
    /// <param name="orderBy">Порядок сортировки. Имеет значение, только если найдено
    /// больше одной записи, удовлетворяющей условию <paramref name="where"/>.
    /// Будет возвращена первая из записей, в соответствии с порядком.
    /// Если порядок не задан, какая запись будет возвращена, не определено</param>
    /// <returns>Идентификатор найденной записи или 0, если запись не найдена</returns>
    protected override Int32 DoFindRecord(string tableName, DBxFilter where, DBxOrder orderBy)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.FindRecord(tableName, where, orderBy);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Найти запись
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие отбора</param>
    /// <param name="singleOnly">Если true и найдено больше одной записи, удовлетворяющей условию
    /// <paramref name="where"/>, то возвращается 0</param>
    /// <returns>Идентификатор найденной записи или 0, если запись не найдена</returns>
    protected override Int32 DoFindRecord(string tableName, DBxFilter where, bool singleOnly)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.FindRecord(tableName, where, singleOnly);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получение значения для одного поля. Имя поля может содержать точки для
    /// извлечения значения из зависимой таблицы. Расширенная версия возвращает
    /// значение поля по ссылке, а как результат возвращается признак того, что
    /// строка найдена
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которой выполняется поиск</param>
    /// <param name="id">Идентификатор строки. Может быть 0, тогда возвращается Value=null</param>
    /// <param name="columnName">Имя поля (может быть с точками)</param>
    /// <param name="value">Сюда по ссылке записывается значение</param>
    /// <returns>true, если поле было найдено</returns>
    protected override bool DoGetValue(string tableName, Int32 id, string columnName, out object value)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetValue(tableName, id, columnName, out value);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить значения для заданного списка полей для одной записи.
    /// Если не найдена строка с заданным идентификатором <paramref name="id"/>, 
    /// то возвращается массив, содержазий одни значения null.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки</param>
    /// <param name="columnNames">Имена столбцов, значения которых нужно получить</param>
    /// <returns>Массив значений</returns>
    protected override object[] DoGetValues(string tableName, Int32 id, DBxColumns columnNames)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetValues(tableName, id, columnNames);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить список идентификаторов в таблице для строк, соответствующих заданному фильтру.
    /// Фильтры по полю Deleted должны быть заданы в явном виде
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтры</param>
    /// <returns>Список идентификаторов</returns>
    protected override IdList DoGetIds(string tableName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetIds(tableName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить минимальное значение числового поля.
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Если нет ни одной строки, удовлетворяющей условию <paramref name="where"/>, возвращается null.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для поиска среди всех строк таблицы)</param>
    /// <returns>Минимальное значение</returns>
    protected override object DoGetMinValue(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetMinValue(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить максимальное значение числового поля
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Если нет ни одной строки, удовлетворяющей условию <paramref name="where"/>, возвращается null.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для поиска среди всех строк таблицы)</param>
    /// <returns>Максимальное значение</returns>
    protected override object DoGetMaxValue(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetMaxValue(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить значения полей для строки, содержащей минимальное значение заданного
    /// поля.
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Если не найдено ни одной строки, удовлетворяющей условию <paramref name="where"/>,
    /// возвращается массив, содержащий одни значения null.
    /// Имена полей в <paramref name="columnNames"/>, <paramref name="minColumnName"/> и <paramref name="where"/>
    /// могут содержать точки. В этом случае используются значения из связанных таблиц.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей, значения которых нужно получить</param>
    /// <param name="minColumnName">Имя поля, минимальное значение которого является условием выбора строки</param>
    /// <param name="where">Фильтр строк, участвующих в отборе</param>
    /// <returns>Массив значений для полей, заданных в <paramref name="columnNames"/></returns>
    protected override object[] DoGetValuesForMin(string tableName, DBxColumns columnNames, string minColumnName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetValuesForMin(tableName, columnNames, minColumnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить значения полей для строки, содержащей максимальное значение заданного
    /// поля.
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// Если не найдено ни одной строки, удовлетворяющей условию <paramref name="where"/>,
    /// возвращается массив, содержащий одни значения null.
    /// Имена полей в <paramref name="columnNames"/>, <paramref name="maxColumnName"/> и <paramref name="where"/>
    /// могут содержать точки. В этом случае используются значения из связанных таблиц.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей, значения которых нужно получить</param>
    /// <param name="maxColumnName">Имя поля, максимальное значение которого является условием выбора строки</param>
    /// <param name="where">Фильтр строк, участвующих в отборе</param>
    /// <returns>Массив значений для полей, заданных в <paramref name="columnNames"/></returns>
    protected override object[] DoGetValuesForMax(string tableName, DBxColumns columnNames, string maxColumnName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetValuesForMax(tableName, columnNames, maxColumnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить суммарное значение числового поля для выбранных записей
    /// Строки таблицы, содержащие значения NULL, игнорируются
    /// Если нет ни одной строки, удовлетворяющей условию <paramref name="where"/>, возвращается null.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для суммирования всех строк таблицы)</param>
    /// <returns>Суммарное значение или null</returns>
    protected override object DoGetSumValue(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetSumValue(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получение списка уникальных значений поля SELECT DISTINCT
    /// В полученной таблице будет одно поле. Таблица будет упорядочена по этому полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="where">Необязательный фильтр записей</param>
    /// <returns>Таблица с единственной колонкой</returns>
    protected override DataTable DoFillUniqueColumnValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.FillUniqueColumnValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить строковые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    protected override string[] DoGetUniqueStringValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueStringValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    protected override int[] DoGetUniqueIntValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueIntValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }


    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    protected override long[] DoGetUniqueInt64Values(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueInt64Values(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    protected override float[] DoGetUniqueSingleValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueSingleValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    protected override double[] DoGetUniqueDoubleValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueDoubleValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    protected override decimal[] DoGetUniqueDecimalValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueDecimalValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить значения поля даты и/или времени без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    protected override DateTime[] DoGetUniqueDateTimeValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueDateTimeValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить значения поля GUID без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    protected override Guid[] DoGetUniqueGuidValues(string tableName, string columnName, DBxFilter where)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUniqueGuidValues(tableName, columnName, where);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }


    /// <summary>
    /// Загрузить страницу таблицы кэша
    /// </summary>
    /// <param name="request">Параметры запроса</param>
    /// <returns>Кэш страницы</returns>
    protected override DBxCacheLoadResponse DoLoadCachePages(DBxCacheLoadRequest request)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.LoadCachePages(request);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    #region GetRecordCount() и IsTableEmpty()

    /// <summary>
    /// Получить общее число записей в таблице
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Число записей</returns>
    protected override int DoGetRecordCount(string tableName)
    {
      return _Source.GetRecordCount(tableName);
    }

    /// <summary>
    /// Получить число записей в таблице, удовлетворяющих условию
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие</param>
    /// <returns>Число записей</returns>
    protected override int DoGetRecordCount(string tableName, DBxFilter where)
    {
      return _Source.GetRecordCount(tableName, where);
    }

    /// <summary>
    /// Возвращает true, если в таблице нет ни одной строки.
    /// Тоже самое, что GetRecordCount()==0, но может быть оптимизировано.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Отсутствие записей</returns>
    protected override bool DoIsTableEmpty(string tableName)
    {
      return _Source.IsTableEmpty(tableName);
    }

    #endregion

    /// <summary>
    /// Очистить страницу таблицы кэша
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список столбцов</param>
    /// <param name="firstIds">Начальные идентификаторы страниц</param>
    protected override void DoClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds)
    {
      _Source.ClearCachePages(tableName, columnNames, firstIds);
    }

    /// <summary>
    /// Получить таблицу истории для документа
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <returns>Таблица истории</returns>
    protected override DataTable DoGetDocHistTable(string docTypeName, Int32 docId)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetDocHistTable(docTypeName, docId);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить таблицу действий пользователя или всех пользователей за определенный период
    /// </summary>
    /// <param name="firstDate">Начальная дата</param>
    /// <param name="lastDate">Конечная дата</param>
    /// <param name="userId">Идентификатор пользователя. 0-все пользователи</param>
    /// <param name="singleDocTypeName">Имя таблицы документа. Пустая строка - документы всех видов</param>
    /// <returns>Таблица действий</returns>
    protected override DataTable DoGetUserActionsTable(DateTime? firstDate, DateTime? lastDate, Int32 userId, string singleDocTypeName)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUserActionsTable(firstDate, lastDate, userId, singleDocTypeName);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Получить таблицу документов для одного действия пользователя
    /// </summary>
    /// <param name="actionId">Идентификатор действия в таблице UserActions</param>
    /// <returns>Таблица документов</returns>
    protected override DataTable DoGetUserActionDocTable(Int32 actionId)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUserActionDocTable(actionId);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Возвращает время последнего действия пользователя (включая компонент времени).
    /// Время возвращается в формате DataSetDateTime.Unspecified.
    /// Если для пользователя нет ни одной записи в таблице UserActions, возвращается null
    /// </summary>
    /// <param name="userId">Идентификатор пользователя, для которого надо получить данные</param>
    /// <returns>Время или null</returns>
    protected override DateTime? DoGetUserActionsLastTime(Int32 userId)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return Source.GetUserActionsLastTime(userId);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Установить длительную блокировку
    /// Если какой-либо из документов уже заблокирован, выбрасывается исключение DBxLockDocsException
    /// </summary>
    /// <param name="docSel">Выборка документов, которую требуется заблокировать</param>
    /// <returns>Идентификатор установленной блокировки</returns>
    protected override Guid DoAddLongLock(DBxDocSelection docSel)
    {
      CheckThread();

      return Source.AddLongLock(docSel);
    }

    /// <summary>
    /// Удалить длительную блокировку
    /// </summary>
    /// <param name="lockGuid">Идентификатор установленной блокировки</param>
    /// <returns>true, если блокировка была удалена. false, если блокировка не найдена (была удалена ранее)</returns>
    protected override bool DoRemoveLongLock(Guid lockGuid)
    {
      CheckThread();

      return Source.RemoveLongLock(lockGuid);
    }

    /// <summary>
    /// Получение текстового представления для документа / поддокумента
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор</param>
    /// <returns>Текстовое представление</returns>
    protected override string DoGetTextValue(string tableName, Int32 id)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return _Source.GetTextValue(tableName, id);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Внутренний метод получения текста.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="primaryDS">Первичный набор данных</param>
    /// <returns>Текст для документа или поддокумента</returns>
    protected override string DoInternalGetTextValue(string tableName, Int32 id, DataSet primaryDS)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return _Source.InternalGetTextValue(tableName, id, primaryDS);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Внутренний метод получения идентификатора двоичных данных
    /// </summary>
    /// <param name="md5">Контрольная сумма</param>
    /// <returns>Идентификатор записи или 0, если таких данных нет</returns>
    protected override int DoInternalFindBinData(string md5)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return _Source.InternalFindBinData(md5);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Внутренний метод получения идентификатора хранимого файла
    /// </summary>
    /// <param name="fileInfo">Информация о файле</param>
    /// <param name="md5">Контрольная сумма содержимого файла</param>
    /// <param name="binDataId">Сюда помещается идентификатор двоичных данных,
    /// возвращаемый методом InternalFindBinData()</param>
    /// <returns>Идентификатор записи файла в базе данных</returns>
    protected override Int32 DoInternalFindDBFile(StoredFileInfo fileInfo, string md5, out Int32 binDataId)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return _Source.InternalFindDBFile(fileInfo, md5, out binDataId);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Пересчитать вычисляемые поля в документах.
    /// </summary>
    /// <param name="docTypeName">Имя вида документв</param>
    /// <param name="docIds">Массив идентификаторов.
    /// null означает пересчет всех документов. Пересчету подлежат в том числе и удаленные документы</param>
    protected override void DoRecalcColumns(string docTypeName, Int32[] docIds)
    {
      CheckThread();
      _Source.RecalcColumns(docTypeName, docIds);
    }

    /// <summary>
    /// Получить набор таблиц для просмотра ссылок на один документ.
    /// </summary>
    /// <param name="docTypeName">Имя таблицы документа, на который ищутся ссылки</param>
    /// <param name="docId">Идентификатор документа, на который ищутся ссылки</param>
    /// <param name="showDeleted">Надо ли включать в таблицу ссылки из удаленных документов и поддокументов,
    /// а также ссылки на удаленные поддокументы выбранного документа.
    /// Не имеет значение, помечен ли сам документ <paramref name="docId"/> на удаление (за исключением ссылок документа на самого себя</param>
    /// <param name="unique">Если true, то будет возвращено только по одной ссылке из каждого документа.
    /// Это рекомендуемый вариант для показа таблицы ссылок. Если false, то в таблице может быть несколько ссылок из одного исходного документа</param>
    /// <param name="fromSingleDocTypeName">Единственный вид документа, из которого берутся ссылки. Если не задано, то берутся ссылки из всех документов</param>
    /// <param name="fromSingleDocId">Идентификатор единственного документа, из которого берутся ссылки. Если 0, то берутся ссылки из всех документов</param>
    /// <returns>Таблица ссылок</returns>
    protected override DataTable DoGetDocRefTable(string docTypeName, Int32 docId, bool showDeleted, bool unique, string fromSingleDocTypeName, Int32 fromSingleDocId)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return _Source.GetDocRefTable(docTypeName, docId, showDeleted, unique, fromSingleDocTypeName, fromSingleDocId);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    /// <summary>
    /// Возвращает список идентификаторов дочерних строк для таблиц, в которых реализована
    /// иерахическая структура с помощью поля, ссылающегося на эту же таблицу, которое задает родительский
    /// элемент. Родительская строка <paramref name="parentId"/> не входит в список
    /// Метод не зацикливается, если структура дерева нарушена (зациклено).
    /// Эта перегрузка возвращает идентификатор "зацикленного" узла. Возвращается только один узел, 
    /// а не вся цепочка зацикливания. Также таблица может содержать несколько цепочек зацикливания.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="parentIdColumnName">Имя ссылочного столбца, например "ParentId"</param>
    /// <param name="parentId">Идентификатор родительской строки. Если 0, то будут возвращены 
    /// идентификаторы строк узлов верхнего уровня или всех строк (при <paramref name="nested"/>=true)</param>
    /// <param name="nested">true, если требуется рекурсивный поиск. false, если требуется вернуть только непосредственные дочерние элементы</param>
    /// <param name="where">Дополнительный фильтр. Может быть null, если фильтра нет</param>
    /// <param name="loopedId">Сюда записывается идентификатор "зацикленного" узла</param>
    /// <returns>Список идентификаторов дочерних элементов</returns>
    protected override IdList DoGetInheritorIds(string tableName, string parentIdColumnName, Int32 parentId, bool nested, DBxFilter where, out Int32 loopedId)
    {
      CheckThread();

      int repeatCount = 0;
      while (true)
      {
        try
        {
          return _Source.GetInheritorIds(tableName, parentIdColumnName, parentId, nested, where, out loopedId);
        }
        catch (Exception e)
        {
          CatchException(e, ref repeatCount);
        }
      }
    }

    #endregion

    #region DBCache

    /// <summary>
    /// Система кэширования данных.
    /// Если данный экземпляр объекта DBxDocProvider является прокси для серверного объекта, то
    /// он содержит собственную копию DBxCache.
    /// Если же текущий DBxDocProvider ссылается на другой провайдер в цепочке без использования
    /// Remoting, то используется Source.DBCache.
    /// </summary>
    protected override DBxCache DoGetDBCache()
    {
      CheckThread();

      if (_SourceIsRemote)
      {
        if (_DBCache == null)
          _DBCache = new DBxCache(this, CurrentThreadOnly);
        return _DBCache;
      }
      else
        return Source.DBCache;
    }
    private DBxCache _DBCache;


    /// <summary>
    /// Очистка кэша
    /// </summary>
    protected override void DoClearCache()
    {
      CheckThread();

      base.DoClearCache(); // испр. 12.07.2022
      _Source.ClearCache();
      if (_DBCache != null)
        _DBCache.Clear(); // очистка кэша клиента
    }

    #endregion

    #region Доступ к двоичным данным и файлам

    /// <summary>
    /// Метод получения двоичных данных, реализуемый в DBxRealDocProvider.
    /// Этот метод не должен использоваться в прикладном коде.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnName">Имя числового столбца, содержащего идентификатор двоичных данных</param>
    /// <param name="wantedId">Идентификатор документа, поддокумента и двочных данных, которые нужно получить</param>
    /// <param name="docVersion">Версия документа. 0 - текущая версия</param>
    /// <param name="preloadIds">Идентификаторы документов, поддокументов и двочных данных,
    /// которые желательно загрузить</param>
    /// <returns>Словарь загруженных данных. Ключ - идентификатор двоичных данных. Значение - загруженные данные</returns>
    protected override Dictionary<Int32, byte[]> DoInternalGetBinData2(string tableName, string columnName, DBxDocProvider.DocSubDocDataId wantedId, int docVersion, List<DBxDocProvider.DocSubDocDataId> preloadIds)
    {
      return _Source.InternalGetBinData2(tableName, columnName, wantedId, docVersion, preloadIds);
    }

    /// <summary>
    /// Внутренний метод получения хранимого файла
    /// Этот метод не должен использоваться в прикладном коде.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="columnName">Имя числового столбца, содержащего идентификатор файла</param>
    /// <param name="wantedId">Идентификатор документа, поддокумента и файла, который нужно получить</param>
    /// <param name="docVersion">Версия документа. 0 - текущая версия</param>
    /// <param name="preloadIds">Идентификаторы документов, поддокументов и файлов,
    /// которые желательно загрузить</param>
    /// <returns>Словарь загруженных данных. Ключ - идентификатор файла. Значение - контейнер с файлом</returns>
    protected override Dictionary<Int32, FileContainer> DoInternalGetDBFile2(string tableName, string columnName, DBxDocProvider.DocSubDocDataId wantedId, int docVersion, List<DBxDocProvider.DocSubDocDataId> preloadIds)
    {
      return _Source.InternalGetDBFile2(tableName, columnName, wantedId, docVersion, preloadIds);
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Создает копию провайдера-источника, а затем - копию DBxChainDocProvider
    /// Этот метод является потокобезопасным
    /// </summary>
    /// <returns></returns>
    protected override DBxDocProvider DoClone()
    {
      DBxDocProvider source2 = Source.Clone();
      return new DBxChainDocProvider(source2.CreateProxy(), CurrentThreadOnly);
    }

    /// <summary>
    /// Не используется в пользовательском коде
    /// </summary>
    /// <returns></returns>
    protected override DistributedCallData DoStartServerExecProc(NamedValues args)
    {
      return Source.StartServerExecProc(args);
    }

    #endregion

    #region Права пользователя

    /// <summary>
    /// Коллекция объектов, предназначенная для создания объектов UserPermission
    /// Если свойство установлено, то свойства UserPermissions и DocPermissions возвращают
    /// локальные копии воссозданных разрешений.
    /// </summary>
    public UserPermissionCreators UserPermissionCreators
    {
      get
      {
        CheckThread();

        return _UserPermissionCreators;
      }
      set
      {
        CheckThread();

        if (_UserPermissionCreators != null)
          throw new InvalidOperationException("Повторная установка свойства не допускается");
        if (value == null)
          throw new ArgumentNullException();
        _UserPermissionCreators = value;
        _UserPermissionCreators.SetReadOnly();

        DBxChainDocProvider source2 = Source as DBxChainDocProvider;
        if (source2 != null && (!SourceIsRemote))
        {
          if (source2.UserPermissionCreators == null)
            source2.UserPermissionCreators = value;
        }

        ResetPermissions();
      }
    }
    private UserPermissionCreators _UserPermissionCreators;

    /// <summary>
    /// Сброс прав пользователя
    /// </summary>
    protected override void DoResetPermissions()
    {
      CheckThread();

      if (_UserPermissions != null) // 04.09.2017
      {
        _UserPermissions = null;

        int repeatCount = 0;
        while (true)
        {
          try
          {
            _Source.ResetPermissions();
            break;
          }
          catch (Exception e)
          {
            CatchException(e, ref repeatCount);  // 04.09.2017
          }
        }
      }
      base.DoResetPermissions();
    }

    /// <summary>
    /// Права, назначенные пользователю (включая классы, определенные в программе).
    /// Объект DBxChainDocProvider содержит собственную копию разрешений
    /// </summary>
    protected override UserPermissions DoGetUserPermissions()
    {
      CheckThread();

      if (_UserPermissions == null)
      {
        if (_UserPermissionCreators == null)
        {
          if (SourceIsRemote)
            return null;
          _UserPermissions = _Source.UserPermissions;
        }
        else
        {
          TempCfg cfg = new TempCfg();
          int repeatCount = 0;
          while (true)
          {
            try
            {
              cfg.AsXmlText = UserPermissionsAsXmlString;
              break;
            }
            catch (Exception e)
            {
              CatchException(e, ref repeatCount);  // 04.09.2017
            }
          }
          _UserPermissions = new UserPermissions(_UserPermissionCreators);
          _UserPermissions.Read(cfg);
        }
      }
      return _UserPermissions;
    }
    private UserPermissions _UserPermissions;

    #endregion

    #region Обработка ошибок

    /// <summary>
    /// Добавляет отладочную информацию в объект исключения.
    /// Используется в блоках catch.
    /// </summary>
    /// <param name="e">Исключение</param>
    protected override void DoAddExceptionInfo(Exception e)
    {
      base.DoAddExceptionInfo(e); // испр. 12.07.2022
      e.Data["DBxChainDocProvider.SourceIsRemote"] = SourceIsRemote;
    }

    /// <summary>
    /// Событие вызывается при возникновении исключения при вызове метода в базовом провайдере (Source).
    /// Пользовательский обработчик может проверить исключение и, если оно связано с сетью, попробовать
    /// восстановить соединение с сервером. После этого следует установить свойство Retry в аргументе события.
    /// При использовании ExtDBDocForms.dll следует добавлять обработчик события к DBUI, а не здесь.
    /// </summary>
    /// <remarks>
    /// Предупреждение.
    /// Если есть цепочка DBxChainDocProvider и устанавливается обработчик события на стороне клиента,
    /// то этот обработчик на самом деле присоединяется к последнему провайдеру в цепочке, после которого
    /// используется Net Remoting для соединения со следующим провайдером.
    /// </remarks>
    public event DBxRetriableExceptionEventHandler ExceptionCaught
    {
      add
      {
        if (SourceIsRemote)
          _ExceptionCaught += value;
        else if (Source is DBxChainDocProvider)
          ((DBxChainDocProvider)Source).ExceptionCaught += value;
        else
          _ExceptionCaught += value;
      }
      remove
      {
        if (SourceIsRemote)
          _ExceptionCaught -= value;
        else if (Source is DBxChainDocProvider)
          ((DBxChainDocProvider)Source).ExceptionCaught -= value;
        else
          _ExceptionCaught -= value;
      }
    }

    private event DBxRetriableExceptionEventHandler _ExceptionCaught;

    /// <summary>
    /// Метод вызывается, если при вызове метода в Source возникло исключение.
    /// Если исключения связано с сетью, метод должен попытаться восстановить подключение к серверу
    /// и вернуть true, чтобы выполнить полытку запроса еще раз.
    /// Тип исключения должен быть проверен, т.к. метод вызывается при любом исключении, не обязательно
    /// связанным с потерей соединения
    /// </summary>
    /// <param name="exception">Возникшее исключение</param>
    /// <param name="repeatCount">Счетчик повторов. При первом вызове равно 0, затем 1 и т.д.
    /// Учитываются только вызовы в пределах одного данного действия.
    /// Переопределенный метод может, например, выполнять только ограниченное количество попыток восстановления</param>
    /// <returns>true, если следует повторить операцию</returns>
    protected bool OnExceptionCaught(Exception exception, int repeatCount)
    {
      if (!SourceIsRemote)
      {
        if (Source is DBxChainDocProvider)
          return ((DBxChainDocProvider)Source).OnExceptionCaught(exception, repeatCount);
      }

      if (_ExceptionCaught == null)
        return false;

      DBxRetriableExceptionEventArgs args = new DBxRetriableExceptionEventArgs(exception, repeatCount);
      _ExceptionCaught(this, args);

      return args.Retry;
    }

    private void CatchException(Exception exception, ref int repeatCount)
    {
      AddExceptionInfo(exception);
      if (SourceIsRemote)
      {
        try
        {
          exception.Data["DBxChainDocProvider"] = this.ToString();
        }
        catch { }
        exception.Data["DBxChainDocProvider.GetType()"] = this.GetType().ToString();
        exception.Data["DBxChainDocProvider.CatchException:RepeatCount"] = repeatCount;
      }

      Stopwatch sw = null; // 17.02.2021
      if (SourceIsRemote)
        sw = Stopwatch.StartNew();
      if (!OnExceptionCaught(exception, repeatCount))
      {
        if (SourceIsRemote)
        {
          sw.Stop();
          exception.Data["ExceptionCaughtHandleTime"] = sw.Elapsed;
        }
        throw exception;
      }

      repeatCount++;
    }

    #endregion

    #region Текущее время сервера
#if XXX
    public override DateTime ServerTime
    {
      get { return DateTime.Now+ServerTimeDiff; }
    }

    /// <summary>
    /// Возвращает интервал между временем сервера и текущим временем
    /// </summary>
    private TimeSpan ServerTimeDiff;
#endif
    #endregion
  }
}
