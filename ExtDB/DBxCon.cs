using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV;
using System.Threading;
using System.Data;
using System.Data.Common;
using System.Collections;
using AgeyevAV.Logging;
using System.Runtime.Serialization;
using System.Diagnostics;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace AgeyevAV.ExtDB
{
  /// <summary>
  /// Методы для работы с транзакциями, реализованные в DBxCon и DBxConBase
  /// </summary>
  public interface IDBxConTransactions
  {
    #region Транзакции

    /// <summary>
    /// Обрабатывается ли сейчас транзакция ?
    /// </summary>
    bool InsideTransaction { get; }

    /// <summary>
    /// Начать транзакцию
    /// </summary>
    void TransactionBegin();

    /// <summary>
    /// Завершение транзакции с сохранением изменений в базе данных
    /// </summary>
    void TransactionCommit();

    /// <summary>
    /// Откат транзакции
    /// </summary>
    void TransactionRollback();

    #endregion
  }

  /// <summary>
  /// Базовый интерфейс только для запросов SELECT.
  /// Запросы, связанные с идентификаторами записи типа Int32 выделены в интерфейс IDBxConReadOnlyPKInt32
  /// </summary>
  public interface IDBxConReadOnlyBase
  {
    #region SELECT (DataTable)

    /// <summary>
    /// Загрузка всей таблицы.
    /// Загружаются все поля SELECT * FROM [TableName] 
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Заполненная таблица DataTable</returns>
    DataTable FillSelect(string tableName);

    /// <summary>
    /// Загрузка выбранных полей всей таблицы.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <returns>Заполненная таблица DataTable</returns>
    DataTable FillSelect(string tableName, DBxColumns columnNames);

    /// <summary>
    /// Загрузка выбранных полей для строк таблицы, отобранных по условию
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. Если список не задан, будут возвращены все поля таблицы</param>
    /// <param name="where">Условие фильтрации</param>
    /// <returns>Заполненная таблица DataTable</returns>
    DataTable FillSelect(string tableName, DBxColumns columnNames, DBxFilter where);

    /// <summary>
    /// Загрузка выбранных полей для строк таблицы, отобранных по условию с заданным порядком сортировки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <param name="where">Условие фильтрации</param>
    /// <param name="orderBy">Порядок сортировки</param>
    /// <returns>Заполненная таблица DataTable</returns>
    DataTable FillSelect(string tableName, DBxColumns columnNames, DBxFilter where, DBxOrder orderBy);

    /// <summary>
    /// Получение списка уникальных значений поля SELECT DISTINCT
    /// В полученной таблице будет одно поле. Таблица будет упорядочена по этому полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="where">Необязательный фильтр записей</param>
    /// <returns>Таблица с единственной колонкой</returns>
    DataTable FillUniqueColumnValues(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Универсальный метод выполнения запроса SELECT.
    /// </summary>
    /// <param name="info">Параметры запроса</param>
    /// <returns>Заполненная таблица</returns>
    DataTable FillSelect(DBxSelectInfo info);

    #endregion

#if XXX
    #region FindRecordValue

    /// <summary>
    /// Найти строку в таблице с указанными значениями полей (ключами) и вернуть значение
    /// требуемого поля. Если строка, удовлетворяющая условию не найдена, возвращается null.
    /// Если есть несколько строк, удовлетворяющих условию, то какая из них будет использована, 
    /// не определено
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="searchColumnNames">Имена ключевых полей для поиска. Ссылочные поля (с точками) не поддерживаются</param>
    /// <param name="searchValues">Значения ключевых полей</param>
    /// <param name="resColumnName">Имя результирующего поля. Ссылочные поля (с точками) не поддерживаются</param>
    /// <returns>Значение поля <paramref name="resColumnName"/> или null, если строка не найдена</returns>
    object FindRecordValue(string tableName, DBxColumns searchColumnNames, object[] searchValues, string resColumnName);

    /// <summary>
    /// Найти строку в таблице с указанным значением заданного поля (используется ValueFilter) и вернуть значение
    /// требуемого поля. Если строка, удовлетворяющая условию не найдена, возвращается null.
    /// Если есть несколько строк, удовлетворяющих условию, то какая из них будет использована, 
    /// не определено.
    /// Этот метод не поддерживает использование ссылочных полей (с точками).
    /// Используйте метод FillSelect() при необходимости.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="searchColumnName">Имя поля фильтра. Ссылочные поля (с точками) не поддерживаются</param>
    /// <param name="searchValue">Значение поля фильтра <paramref name="searchColumnName"/></param>
    /// <param name="resColumnName">Имя результирующего поля. Ссылочные поля (с точками) не поддерживаются</param>
    /// <returns>Значение поля <paramref name="resColumnName"/> или null, если строка не найдена</returns>
    object FindRecordValue(string tableName, string searchColumnName, object searchValue, string resColumnName);

    #endregion
#endif

    #region Информационные методы

    /// <summary>
    /// Получить общее число записей в таблице
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Число записей</returns>
    int GetRecordCount(string tableName);

    /// <summary>
    /// Получить число записей в таблице, удовлетворяющих условию
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие</param>
    /// <returns>Число записей</returns>
    int GetRecordCount(string tableName, DBxFilter where);

    /// <summary>
    /// Возвращает true, если в таблице нет ни одной строки.
    /// Тоже самое, что GetRecordCount()==0, но может быть оптимизировано.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Отсутствие записей</returns>
    bool IsTableEmpty(string tableName);

    #endregion

    #region Получение значений полей

    /// <summary>
    /// Получить максимальное значение числового поля
    /// Строки таблицы, содержащие значения NULL, игнорируются.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для поиска среди всех строк таблицы)</param>
    /// <returns>Максимальное значение или null</returns>
    object GetMaxValue(string tableName, string columnName, DBxFilter where);

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
    object[] GetValuesForMax(string tableName, DBxColumns columnNames, string maxColumnName, DBxFilter where);

    /// <summary>
    /// Получить минимальное значение числового поля
    /// Строки таблицы, содержащие значения NULL, игнорируются
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для поиска среди всех строк таблицы)</param>
    /// <returns>Минимальное значение или null</returns>
    object GetMinValue(string tableName, string columnName, DBxFilter where);

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
    object[] GetValuesForMin(string tableName, DBxColumns columnNames,
      string minColumnName, DBxFilter where);

    /// <summary>
    /// Получить суммарное значение числового поля для выбранных записей
    /// Строки таблицы, содержащие значения NULL, игнорируются
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для суммирования всех строк таблицы)</param>
    /// <returns></returns>
    object GetSumValue(string tableName, string columnName, DBxFilter where);

    #endregion

    #region GetUniqueXxxValues()

    /// <summary>
    /// Получить строковые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    string[] GetUniqueStringValues(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    int[] GetUniqueIntValues(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    long[] GetUniqueInt64Values(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    float[] GetUniqueSingleValues(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    double[] GetUniqueDoubleValues(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить числовые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    decimal[] GetUniqueDecimalValues(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить значения поля даты и/или времени без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    DateTime[] GetUniqueDateTimeValues(string tableName, string columnName, DBxFilter where);

    /// <summary>
    /// Получить значения поля GUID без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    Guid[] GetUniqueGuidValues(string tableName, string columnName, DBxFilter where);

    #endregion
  }

  /// <summary>
  /// Расширение интерфейса IDBxConReadOnlyBase на запросы, принимающие целочисленные идентификаторы первичного ключа.
  /// Этот интерфейс реализуется также классом DBxGridProducer в ExtDBDocs.dll
  /// </summary>
  public interface IDBxConReadOnlyPKInt32 : IDBxConReadOnlyBase
  {
    #region FindRecord

    /// <summary>
    /// Найти строку с заданным значением поля
    /// Возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля условия</param>
    /// <param name="value">Значение поля условия</param>
    /// <returns>Идентификатор строки или 0</returns>
    Int32 FindRecord(string tableName, string columnName, object value);

    /// <summary>
    /// Найти строку с заданными значениями полей
    /// Возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Пары ИмяПоля-Значение</param>
    /// <returns>Идентификатор строки или 0</returns>
    Int32 FindRecord(string tableName, IDictionary columnNamesAndValues);

    /// <summary>
    /// Найти строку с заданными значениями полей
    /// Возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей условия</param>
    /// <param name="values">Значения полей условия</param>
    /// <returns>Идентификатор строки или 0</returns>
    Int32 FindRecord(string tableName, DBxColumns columnNames, object[] values);

    /// <summary>
    /// Найти строку с заданными значениями полей. 
    /// Если задан порядок сортировки, то отыскиваются все строки с заданными значениями, 
    /// они упорядочиваются и возвращается идентификатор первой строки. Если OrderBy=null,
    /// то возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей условия</param>
    /// <param name="values">Значения полей условия</param>
    /// <param name="orderBy">Порядок сортировки (может быть null)</param>
    /// <returns>Идентификатор строки или 0</returns>
    Int32 FindRecord(string tableName, DBxColumns columnNames, object[] values, DBxOrder orderBy);

    ///// <summary>
    ///// Поиск любой строки таблицы без всяких условий.
    ///// </summary>
    ///// <param name="tableName">Имя таблицы</param>
    ///// <returns>Идентификатор первой попавшейся записи или 0, если таблица не содержит записей</returns>
    //Int32 FindRecord(string tableName);

    /// <summary>
    /// Поиск первой строки, удовлетворяющей условию при заданном порядке строк
    /// Если задан порядок сортировки, то отыскиваются все строки с заданными значениями, 
    /// они упорядочиваются и возвращается идентификатор первой строки. Если OrderBy=null,
    /// то возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр</param>
    /// <param name="orderBy">Порядок сортировки, при котором нужна первая строка. Может быть null</param>
    /// <returns>Идентификатор первой записи, удовлетворяющей условию</returns>
    Int32 FindRecord(string tableName, DBxFilter where, DBxOrder orderBy);

    /// <summary>
    /// Поиск первой строки, удовлетворяющей условию.
    /// Если есть несколько подходящих строк, то возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр</param>
    Int32 FindRecord(string tableName, DBxFilter where);

    /// <summary>
    /// Поиск записи в таблице.
    /// Таблица должна иметь первичный ключ по числовому полю.
    /// Возвращает идентификатор строки (значение первичного ключа), если запись найдена.
    /// Возвращает 0, если запись не найдена.
    /// Можно задать дополнительное ограничение на уникальнойсть найденной записи.
    /// Если огранчиение указано и найдено больше одной строки, возвращается 0.
    /// Имена полей в фильтрах <paramref name="where"/> могут содержать точки (ссылочные поля).
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие поиска</param>
    /// <param name="singleOnly">Если true, то найденная запись должна быть уникальной</param>
    /// <returns>Имя столбца (обычно, "Id")</returns>
    Int32 FindRecord(string tableName, DBxFilter where, bool singleOnly);

    #endregion

    #region GetIds

    /// <summary>
    /// Получить массив идентификаторов строк с заданным значением поля
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля условия</param>
    /// <param name="value">Значение поля условия</param>
    /// <returns>Массив идентификаторов</returns>
    IdList GetIds(string tableName, string columnName, object value);

    /// <summary>
    /// Получить массив идентификаторов строк с заданными значениями полей
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Пары ИмяПоля-Значение</param>
    /// <returns>Массив идентификаторов строк</returns>
    IdList GetIds(string tableName, IDictionary columnNamesAndValues);

    /// <summary>
    /// Получить массив идентификаторов строк таблицы с заданными значениями полей
    /// </summary>
    /// <param name="tableName">Таблица</param>
    /// <param name="columnNames">Имена полей условия</param>
    /// <param name="values">Значения полей условия</param>
    /// <returns>Массив идентификаторов</returns>
    IdList GetIds(string tableName, DBxColumns columnNames, object[] values);

    /// <summary>
    /// Получение массива идентификаторов первичного ключа (обычно, поля "Id") для всех строк, удовлетворяющих
    /// условию.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие (фильтр)</param>
    /// <returns>Массив идентификаторов (значений поля "Id")</returns>
    IdList GetIds(string tableName, DBxFilter where);

    #endregion

    #region GetValue(), GetValues()

    /// <summary>
    /// Получение значения для одного поля. Имя поля может содержать точки для
    /// извлечения значения из зависимой таблицы. 
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// Имя поля может содержать точки для получения значения ссылочного поля с помощью INNER JOIN.
    /// Если <paramref name="id"/>=0, возвращает null.
    /// Выбрасывает исключение DBxRecordNotFoundException, если задан идентификатор несуществующей записи.
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которой выполняется поиск</param>
    /// <param name="id">Идентификатор строки. Может быть 0</param>
    /// <param name="columnName">Имя поля (может быть с точками)</param>
    /// <returns>Значение</returns>
    object GetValue(string tableName, Int32 id, string columnName);

    /// <summary>
    /// Получение значения для одного поля. Имя поля может содержать точки для
    /// извлечения значения из зависимой таблицы. Расширенная версия возвращает
    /// значение поля по ссылке, а как результат возвращается признак того, что
    /// строка найдена.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// Имя поля может содержать точки для получения значения ссылочного поля с помощью INNER JOIN.
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которой выполняется поиск</param>
    /// <param name="id">Идентификатор строки. Может быть 0, тогда возвращается Value=null</param>
    /// <param name="columnName">Имя поля (может быть с точками)</param>
    /// <param name="value">Сюда по ссылке записывается значение</param>
    /// <returns>true, если поле было найдено</returns>
    bool GetValue(string tableName, Int32 id, string columnName, out object value);

    /// <summary>
    /// Получить значения для заданного списка полей.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// Если <paramref name="id"/>=0, возвращается массив значений null подходящей длины.
    /// Выбрасывает исключение DBxRecordNotFoundException, если задан идентификатор несуществующей записи.
    /// Имена полей могут содержать точки для получения значений ссылочных полей с помощью INNER JOIN.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки таблицы (значение первичного ключа)</param>
    /// <param name="columnNames">Список столбцов</param>
    /// <returns>Массив значений полей строки</returns>
    object[] GetValues(string tableName, Int32 id, DBxColumns columnNames);

    /// <summary>
    /// Получить значения для заданного списка полей.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// Если <paramref name="id"/>=0, возвращается массив значений null подходящей длины.
    /// Выбрасывает исключение DBxRecordNotFoundException, если задан идентификатор несуществующей записи.
    /// Имена полей могут содержать точки для получения значений ссылочных полей с помощью INNER JOIN.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки таблицы (значение первичного ключа)</param>
    /// <param name="columnNames">Список столбцов</param>
    /// <returns>Массив значений полей строки</returns>
    object[] GetValues(string tableName, Int32 id, string columnNames);

    #endregion
  }

  /// <summary>
  /// Общий интерфейс для DBxConBase и DBxCon
  /// </summary>
  public interface IDBxCon : IDBxConTransactions, IDBxConReadOnlyPKInt32
  {
    #region Свойства

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то при возникновении ошибки при выполнении запросов,
    /// на сервере записывается LOG-файл с ошибкой
    /// </summary>
    bool LogoutSqlExceptions { get; set; }

    /// <summary>
    /// Если свойство установлено в true, то при выполнении запросов AddRecord(), SetValues(), SetValue(),
    /// значения строковых полей будут обрезаться, если их длина превышает заявляенную в структуре базы данных
    /// </summary>
    bool TrimValues { get; set; }

    /// <summary>
    /// Время выполнения команд в секундах. 0 - бесконечное ожидание выполнения запроса.
    /// </summary>
    int CommandTimeout { get; }

    /// <summary>
    /// Выполняется ли проверка существования описаний таблиц и полей в реальной структуры таблицы
    /// при выполнении запросов.
    /// </summary>
    bool NameCheckingEnabled { get; }

    /// <summary>
    /// Текущий выполняющийся запрос (для отладки)
    /// </summary>
    DBxConQueryInfo CurrentQuery { get; }

    /// <summary>
    /// Источник для создания структуры базы данных
    /// </summary>
    IDBxStructSource StructSource { get; }

    #endregion

    #region Выполнение запросов

    #region SELECT (DbDataReader)

    /// <summary>
    /// Загрузка всей таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns></returns>
    DbDataReader ReaderSelect(string tableName);

    /// <summary>
    /// Загрузка выбранных полей всей таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <returns>Объект DbDataReader</returns>
    DbDataReader ReaderSelect(string tableName, DBxColumns columnNames);

    /// <summary>
    /// Загрузка выбранных полей для строк таблицы, отобранных по условию.
    /// Объект DataReader должен быть закрыт по окончании чтения.
    /// На время чтения текущее соединение занято и не должно использоваться для других запросов.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <param name="where">Условие фильтрации</param>
    /// <returns>Объект DbDataReader</returns>
    DbDataReader ReaderSelect(string tableName, DBxColumns columnNames, DBxFilter where);

    /// <summary>
    /// Загрузка выбранных полей для строк таблицы, отобранных по условию.
    /// Объект DataReader должен быть закрыт по окончании чтения.
    /// На время чтения текущее соединение занято и не должно использоваться для других запросов.
    /// </summary>
    /// <param name="TableName">Имя таблицы</param>
    /// <param name="ColumnNames">Список имен полей, разделенных запятыми</param>
    /// <param name="Where">Условие фильтрации</param>
    /// <param name="OrderBy">Порядок сортировки</param>
    /// <returns>Объект DataReader</returns>
    DbDataReader ReaderSelect(string TableName, DBxColumns ColumnNames, DBxFilter Where, DBxOrder OrderBy);

    /// <summary>
    /// Вызов оператора SELECT с заполнением таблицы в через DbDataReader
    /// Объект DataReader должен быть закрыт после чтения данных
    /// !!! В текущей реализации имена полей в выходном наборе могут отличаться от
    /// !!! исходных при наличии ссылочных полей
    /// </summary>
    /// <param name="info">Параметры запроса</param>
    /// <returns>Объект DataReader</returns>
    DbDataReader ReaderSelect(DBxSelectInfo info);

    #endregion

    #region Запись значений полей

    /// <summary>
    /// Установить значения одного поля для одной строки.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// Если задан идентификатор <paramref name="id"/>=0 или недействительный идентификатор записи, выбрасывается исключение.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки (значение первичного ключа)</param>
    /// <param name="columnName">Имя устанавливаемого поля</param>
    /// <param name="value">Значение</param>
    void SetValue(string tableName, Int32 id, string columnName, object value);

    /// <summary>
    /// Установить значение одного поля для нескольких строк таблицы.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам. Если null, то значение устанавливается для всех строк таблицы</param>
    /// <param name="columnName">Имя устанавливаемого поля</param>
    /// <param name="value">Значение</param>
    void SetValue(string tableName, DBxFilter where, string columnName, object value);

    /// <summary>
    /// Установить значения полей для записи с заданным идентификатором
    /// В отличие от других вариантов вызова функции, выполняется 5 попыток 
    /// установить значение, если происходит блокировка записи в другом потоке.
    /// Если задан идентификатор <paramref name="id"/>=0 или недействительный идентификатор записи, выбрасывается исключение.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор записи</param>
    /// <param name="columnNames">Имена устанавливаемых полей</param>
    /// <param name="values">Записываемые значения полей</param>
    void SetValues(string tableName, Int32 id, DBxColumns columnNames, object[] values);

    /// <summary>
    /// Установка значений нескольких полей для нескольких строк таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам таблицы. Если null, то значение устанавливается для всех строк таблицы</param>
    /// <param name="columnNames">Имена устаналиваемых полей</param>
    /// <param name="values">Устанавливаемые значения. Длина массива должна совпадать с <paramref name="columnNames"/></param>
    void SetValues(string tableName, DBxFilter where, DBxColumns columnNames, object[] values);

    /// <summary>
    /// Установка значений нескольких полей для одной таблицы.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// Если задан идентификатор <paramref name="id"/>=0 или недействительный идентификатор записи, выбрасывается исключение.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки таблицы (значение первичного ключа)</param>
    /// <param name="columnNamesAndValues">Имена устанавливаемых столбцов и значения</param>
    void SetValues(string tableName, Int32 id, IDictionary columnNamesAndValues);

    /// <summary>
    /// Установка значений нескольких полей для нескольких строк таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам таблицы. Если null, то значение устанавливается для всех строк таблицы</param>
    /// <param name="columnNamesAndValues">Имена устанавливаемых столбцов и значения</param>
    void SetValues(string tableName, DBxFilter where, IDictionary columnNamesAndValues);

    #endregion

    #region Добавление записей (INSERT INTO)

    #region Версии для таблицы с числовым идентификатором (возвращают поле Id)

    /// <summary>
    /// Добавляет новую строку в таблицу и возвращает ее идентификатор (поле Id).
    /// В строку записывается значение только одного поля (не считая автоинкрементного), поэтому, обычно следует использовать 
    /// другие перегрузки.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя устанавливаемого столбца</param>
    /// <param name="value">Значение поля</param>
    /// <returns>Идентификатор строки таблицы (значение первичного ключа)</returns>
    Int32 AddRecordWithIdResult(string tableName, string columnName, object value);

    /// <summary>
    /// Добавляет новую строку в таблицу и возвращает ее идентификатор (поле Id).
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Имена устанавливаемых полей и значения</param>
    /// <returns>Идентификатор строки таблицы (значение первичного ключа)</returns>
    Int32 AddRecordWithIdResult(string tableName, IDictionary columnNamesAndValues);

    /// <summary>
    /// Добавляет новую строку в таблицу и возвращает ее идентификатор (поле Id).
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена устанавливаемых столбцов</param>
    /// <param name="values">Устанавливаемые значения. Длина массива должна совпадать с ColumnNames</param>
    /// <returns>Идентификатор строки таблицы (значение первичного ключа)</returns>
    Int32 AddRecordWithIdResult(string tableName, DBxColumns columnNames, object[] values);

    #endregion

    #region Без возврата идентификатора

    /// <summary>
    /// Добавляет новую строку в таблицу, но не возвращает ее идентификатор.
    /// Эта перегрузка позволяет задать только одно значение поля для строки,
    /// поэтому имеет очень ограниченное применение.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя столбца, для которого задается значение</param>
    /// <param name="value">Значение для поля <paramref name="columnName"/></param>
    void AddRecord(string tableName, string columnName, object value);

    /// <summary>
    /// Добавляет новую строку в таблицу, но не возвращает ее идентификатор.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Имена столбцов и значения полей</param>
    void AddRecord(string tableName, IDictionary columnNamesAndValues);

    /// <summary>
    /// Добавить строку в таблицу.
    /// Список столбцов <paramref name="columnNames"/> может содержать, а может и не содержать
    /// ключевое поле. Если ключевого поля нет, то значение присваивается автоматически.
    /// Полученный идентификатор не возвращается, благодаря чему можно ограничиться запросом INSERT INTO 
    /// без дополнительных действий по получению идентификатора.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена столбцов. В списке не должно быть поля первичного ключа</param>
    /// <param name="values">Значения. Порядок значений должен соответствовать списку столбцов</param>
    void AddRecord(string tableName, DBxColumns columnNames, object[] values);

    #endregion

    #region Добавление нескольких записей

    /// <summary>
    /// Начать групповое добавление и/или изменение строк таблицы.
    /// После вызова метода и до вызова DBxDataWriter.Dispose() нельзя вызывать другие методы для текущего соединения.
    /// </summary>
    /// <param name="writerInfo">Параметры обработки</param>
    /// <returns>Объект для записи</returns>
    DBxDataWriter CreateWriter(DBxDataWriterInfo writerInfo);

    /// <summary>
    /// Добавление множества записей из таблицы данных.
    /// Используется BULK COPY, если эта возможность реализована в базе данных.
    /// Иначе выполняется поштучное добавление строк с помощью INSERT COPY.
    /// Метод не возвращает идентификаторы добавленных записей.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица может содержать, а может и не содержать ключевое поле. Если ключевого поля нет, то значение присваивается автоматически.
    /// </summary>
    /// <param name="table">Таблица данных</param>
    void AddRecords(DataTable table);

    /// <summary>
    /// Добавление множества записей из таблицы данных.
    /// Используется BULK COPY, если эта возможность реализована в базе данных.
    /// Иначе выполняется поштучное добавление строк с помощью INSERT COPY.
    /// Метод не возвращает идентификаторы добавленных записей.
    /// Таблица может содержать, а может и не содержать ключевое поле. Если ключевого поля нет, то значение присваивается автоматически.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="table">Таблица данных</param>
    void AddRecords(string tableName, DataTable table);

    /// <summary>
    /// Добавление множества записей из таблицы данных.
    /// Используется BULK COPY, если эта возможность реализована в базе данных.
    /// Иначе выполняется поштучное добавление строк с помощью INSERT COPY.
    /// Метод не возвращает идентификаторы добавленных записей.
    /// Пары можгут содержать, а могут и не содержать ключевое поле. Если ключевого поля нет, то значение присваивается автоматически.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValuesArray">Массив хэш-таблиц, по одной таблице в массмвк для каждой записи.
    /// Каждая хэш таблица содержит пары "ИмяПоля"-"Значение" для одной записи</param>
    void AddRecords(string tableName, IDictionary[] columnNamesAndValuesArray);

    /// <summary>
    /// Добавление множества записей из нескольких таблиц данных.
    /// Используется BULK COPY, если эта возможность реализована в базе данных.
    /// Иначе выполняется поштучное добавление строк с помощью INSERT COPY.
    /// Метод не возвращает идентификаторы добавленных записей.
    /// Имена таблиц, в которые выполняется добавление строк, извлекаются из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица могут содержать, а могут и не содержать ключевые поля. Если ключевого поля нет, то значение присваивается автоматически.
    /// </summary>
    /// <param name="ds">Набор таблиц</param>
    void AddRecords(DataSet ds);

    /// <summary>
    /// Групповое добавление записей (Bulk Copy) из открытого объекта DbDataReader.
    /// Если для базы данных не предусмотрена такая возможность, выполняется поштучное добавление записей
    /// Таблица может содержать, а может и не содержать
    /// ключевое поле. Если ключевого поля нет, то значение присваивается автоматически
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которую добавляются записи</param>
    /// <param name="source">Открытая на чтение таблица данных, возможно, в другой базе данных</param>
    void AddRecords(string tableName, DbDataReader source);

    #endregion

    #region Обновление / добавление записей

    /// <summary>
    /// Обновление множества записей из таблицы данных.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица <paramref name="table"/> должна содержать поле (или поля), соответствующее первичному ключу таблицы в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблице <paramref name="table"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="table">Таблица исходных данных</param>
    void UpdateRecords(DataTable table);

    /// <summary>
    /// Групповое обновление записей из таблицы данных.
    /// Таблица <paramref name="table"/> должна содержать поле (или поля), соответствующее первичному ключу таблицы в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблице <paramref name="table"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="tableName">Имя таблицы базы данных, в которую добавляются записи</param>
    /// <param name="table">Таблица исходных данных</param>
    void UpdateRecords(string tableName, DataTable table);

    /// <summary>
    /// Групповое обновление записей из нескольких таблиц данных.
    /// Имена таблиц, для которых выполняется обновление строк, извлекаются из свойства <see cref="System.Data.DataTable.TableName"/>.
    /// Таблицы должны содержать поле (или поля), соответствующие первичному ключу таблиц в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблицах <paramref name="ds"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="ds">Набор данных</param>
    void UpdateRecords(DataSet ds);

    /// <summary>
    /// Обновление множества записей из таблицы данных и добавление недостающих.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица должна содержать поле (или поля), соответствующие первичному ключу таблицы в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблице <paramref name="table"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="table">Таблица исходных данных</param>
    void AddOrUpdateRecords(DataTable table);

    /// <summary>
    /// Обновление множества записей из таблицы данных и добавление недостающих.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица должна содержать поле (или поля), соответствующие первичному ключу таблицы в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблице <paramref name="table"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="tableName">Имя таблицы в базе данных, в которую добавляются записи</param>
    /// <param name="table">Таблица исходных данных</param>
    void AddOrUpdateRecords(string tableName, DataTable table);

    /// <summary>
    /// Обновление множества записей из нескольких таблиц данных и добавление недостающих.
    /// Имена таблиц, для которых выполняется обновление строк, извлекаются из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблицы должны содержать поле (или поля), соответствующие первичному ключу таблиц в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблицах <paramref name="ds"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="ds">Набор таблиц</param>
    void AddOrUpdateRecords(DataSet ds);

    #endregion

    #region Комбинированный поиск и добавление записи

    /// <summary>
    /// Поиск строки по значениям полей, заданным в виде списка пар. Если запись
    /// с подходящими значениями не найдена, то добавляется новая запись
    /// На время выполнения запроса база данных блокируется во избежание двойного
    /// добавления.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="TableName">Имя таблицы</param>
    /// <param name="ColumnNamesAndValues">Имена и значения полей</param>
    /// <param name="Id">Возвращается идентификатор Id найденной или новой записи. Не может быть 0</param>
    /// <returns>true, если была добавлена новая запись, false-если найдена существующая</returns>
    bool FindOrAddRecord(string TableName, IDictionary ColumnNamesAndValues, out Int32 Id);

    /// <summary>
    /// Поиск строки по значениям полей. Если запись
    /// с подходящими значениями не найдена, то добавляется новая запись
    /// На время выполнения запроса база данных блокируется во избежание двойного
    /// добавления.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="TableName">Имя таблицы</param>
    /// <param name="ColumnNames">Имена полей</param>
    /// <param name="Values">Значения полей</param>
    /// <param name="Id">Возвращается идентификатор Id найденной или новой записи. Не может быть 0</param>
    /// <returns>true, если была добавлена новая запись, false-если найдена существующая</returns>
    bool FindOrAddRecord(string TableName, DBxColumns ColumnNames, object[] Values, out Int32 Id);

    /// <summary>
    /// Поиск строки по значениям полей, заданным в виде списка пар. Если запись
    /// с подходящими значениями не найдена, то добавляется новая запись
    /// На время выполнения запроса база данных блокируется во избежание двойного
    /// добавления.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="TableName">Имя таблицы</param>
    /// <param name="ColumnNamesAndValues">Имена и значения полей</param>
    /// <returns>Возвращается идентификатор Id найденной или новой записи. Не может быть 0</returns>
    Int32 FindOrAddRecord(string TableName, IDictionary ColumnNamesAndValues);

    /// <summary>
    /// Поиск строки по значениям полей. Если запись
    /// с подходящими значениями не найдена, то добавляется новая запись
    /// На время выполнения запроса база данных блокируется во избежание двойного
    /// добавления.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей</param>
    /// <param name="values">Значения полей</param>
    /// <returns>Возвращается идентификатор Id найденной или новой записи. Не может быть 0</returns>
    Int32 FindOrAddRecord(string tableName, DBxColumns columnNames, object[] values);

    /// <summary>
    /// Групповой поиск или добавление множества записей в таблицу базы данных.
    /// Искомые строки передаются в таблице DataTable. В таблице должно быть одно или несколько полей, на основании
    /// которых выполняется поиск. Эта таблица НЕ ДОЛЖНА иметь поля идентификатора.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица в базе данных должна иметь первичный ключ типа Int32.
    /// Предполагается, что в базе данных имеется индекс по полям, по которым выполняется поиск, иначе будет медленно.
    /// Должно быть разрешение на запись таблицы.
    /// Возвращает массив идентификаторов найденных или созданных строк. Длина массива и порядок элемента совпадает
    /// со строками исходной таблицы <paramref name="table"/>.
    /// </summary>
    /// <param name="table">Строки для поиска. Этот объект не меняется</param>
    /// <returns>Идентификаторы строк</returns>
    Int32[] FindOrAddRecords(DataTable table);

    /// <summary>
    /// Групповой поиск или добавление множества записей в таблицу базы данных.
    /// Искомые строки передаются в таблице DataTable. В таблице должно быть одно или несколько полей, на основании
    /// которых выполняется поиск. Эта таблица НЕ ДОЛЖНА иметь поля идентификатора.
    /// Таблица в базе данных должна иметь первичный ключ типа Int32.
    /// Предполагается, что в базе данных имеется индекс по полям, по которым выполняется поиск, иначе будет медленно.
    /// Должно быть разрешение на запись таблицы.
    /// Возвращает массив идентификаторов найденных или созданных строк. Длина массива и порядок элемента совпадает
    /// со строками исходной таблицы <paramref name="table"/>.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="table">Строки для поиска. Этот объект не меняется</param>
    /// <returns>Идентификаторы строк</returns>
    Int32[] FindOrAddRecords(string tableName, DataTable table);

    #endregion

    #endregion

    #region Удаление записи

    /// <summary>
    /// Удаление одной строки таблицы.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки таблицы (значение первичного ключа)</param>
    void Delete(string tableName, Int32 id);

    /// <summary>
    /// Удаление нескольких строк таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по удаляемым строкам</param>
    void Delete(string tableName, DBxFilter where);

    /// <summary>
    /// Удалить все строки таблицы. Сама таблица не удаляется
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    void DeleteAll(string tableName);

    #endregion

    #region BLOB-поля

    /// <summary>
    /// Запись значения BLOB-поля как байтового массива.
    /// Ссылочные поля (с точками) не поддерживаются.
    /// Для очистки содержимого поля используйте <paramref name="value"/>=null. 
    /// Значение null и пустой массив различаются.
    /// Условие <paramref name="where"/> должно быть задано обязательно.
    /// Как правило, это фильтр по первичному ключу.
    /// Если фильтру соответствует несколько строк таблиц, значение поля устанавливается для всех из них.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам. Не может быть null</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <param name="value">Заполняемое значение или null</param>
    void WriteBlob(string tableName, DBxFilter where, string columnName, byte[] value);

    /// <summary>
    /// Запись значения BLOB-поля как байтового массива.
    /// Ссылочные поля (с точками) не поддерживаются.
    /// Для очистки содержимого поля используйте <paramref name="value"/>=null. 
    /// Значение null и пустой массив различаются.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор записи. Не может быть 0</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <param name="value">Заполняемое значение или null</param>
    void WriteBlob(string tableName, Int32 id, string columnName, byte[] value);

    /// <summary>
    /// Получить значение BLOB-поля как байтового массива.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// Если <paramref name="id"/>=0, возвращает null.
    /// Выбрасывает исключение DBxRecordNotFoundException, если задан идентификатор несуществующей записи.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор записи. Не может быть 0</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <returns>Значение поля или null</returns>
    byte[] ReadBlob(string tableName, Int32 id, string columnName);

    /// <summary>
    /// Получить значение BLOB-поля как байтового массива.
    /// Условие <paramref name="where"/> должно быть задано обязательно.
    /// Как правило, это фильтр по первичному ключу.
    /// Если фильтру соответствует несколько строк таблиц, то возвращается значение поля из первой попавшейся строки,
    /// что обычно нежелательно.
    /// 
    /// Если нет ни одной строки, удовлетворяющей условию фильтра, то возвращается null.
    /// Null также возвращается, если в найденной строке поле имеет значение NULL.
    /// Используйте перегрузку, возвразающую значение Value по ссылке, чтобы различить эти ситуации.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам. Не может быть null</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <returns>Значение поля или null</returns>
    byte[] ReadBlob(string tableName, DBxFilter where, string columnName);

    /// <summary>
    /// Получить значение BLOB-поля как байтового массива.
    /// Значение поля возвращается по ссылке <paramref name="value"/>. Если строка не найдена или
    /// поле содержит значение NULL, по ссылке передается null.
    /// Условие <paramref name="where"/> должно быть задано обязательно.
    /// Как правило, это фильтр по первичному ключу.
    /// Если фильтру соответствует несколько строк таблиц, то возвращается значение поля из первой попавшейся строки,
    /// что обычно нежелательно.
    /// Эта перегрузка метода позволяет определить наличие или отсутствие строки, удовлетворяющей фильтру.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам. Не может быть null</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <param name="value">Сюда записывается значение поля или null</param>
    /// <returns>True, если найдена строка, удовлетворяющая условию <paramref name="where"/>.
    /// False, если строка не найдена</returns>
    bool ReadBlob(string tableName, DBxFilter where, string columnName, out byte[] value);

    #endregion

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Возвращает пустую таблицу с заданными столбцами.
    /// Всегда создается новый объект DataTable.
    /// Можно использовать ссылочные столбцы, содержащие ".".
    /// Вызывает DBxStruct.CreateDataTable().
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список столбцов. Если null, то возвращаются все столбцы, определенные для таблицы</param>
    /// <returns>Пустая таблица</returns>
    DataTable CreateEmptyTable(string tableName, DBxColumns columnNames);

    #endregion
  }


  /// <summary>
  /// Информация о текущем выполняющемся запросе, возвращаемая свойством DBxConBase.CurrentQuery и DBxCon.CurrentQuery
  /// </summary>
  public sealed class DBxConQueryInfo
  {
    // Сохраняемая информация между TraceSqlBegin и TraceSqlEnd.
    // Нельзя использовать поля класса DBxConBase, т.к. запросы могут выполняться вложенно при создании объекта DBConnection
    // TraceEnabled может асинхронно измениться в процессе выполнения запроса

    #region Защищенный Конструктор

    internal DBxConQueryInfo(string cmdText, bool traceEnabled, DBxConQueryInfo prevInfo)
    {
      _StartTicks = Stopwatch.GetTimestamp();
      _TraceEnabled = traceEnabled;
      _CmdText = cmdText;
      _PrevInfo = prevInfo;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выполняемый запрос
    /// </summary>
    public string CmdText { get { return _CmdText; } }
    private string _CmdText;

    /// <summary>
    /// Время начала выполнения SQL-запроса
    /// </summary>
    private long _StartTicks;

    /// <summary>
    /// Время выполнения запроса (для отладки)
    /// </summary>
    public TimeSpan ExecutingTime 
    { 
      get 
      {
        long delta = Stopwatch.GetTimestamp() - _StartTicks;
        if (delta <= 0L)
          return TimeSpan.Zero; // на случай сбоя
        else
          return TimeSpan.FromSeconds((double)delta / Stopwatch.Frequency);
      } 
    }

    /// <summary>
    /// Признак трассировки.
    /// Определяется в начале выполнения запроса, а нужен после выполнения
    /// </summary>
    internal bool TraceEnabled { get { return _TraceEnabled; } }
    private bool _TraceEnabled;

    /// <summary>
    /// Построение связанного списка запросов
    /// </summary>
    internal DBxConQueryInfo PrevInfo { get { return _PrevInfo; } }
    private DBxConQueryInfo _PrevInfo;

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Возвращает свойство CmdText (для отладки)
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _CmdText;
    }

    #endregion
  }


  /// <summary>
  /// Клиентское соединение с базой данных.
  /// Объект может создаваться на стороне клиента или сервера. По окончании использования объект должен
  /// быть сразу освобожден вызовом метода Dispose() (или, в C#, находиться в блоке using)
  /// </summary>
  public class DBxCon : MarshalByRefDisposableObject, IDBxCon, ICloneable, IDBxCacheSource
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Основная версия конструктора.
    /// </summary>
    /// <param name="entry">Точка подключения к базе данных. не может быть null.</param>
    public DBxCon(DBxEntry entry)
    {
#if DEBUG
      if (entry == null)
        throw new ArgumentNullException("entry");
#endif

      _Source = entry.CreateCon();
    }

    /// <summary>
    /// Версия конструктора с настраиваемым DBxConBase (например, можно отключить проверку имен).
    /// DBxConBase должен быть создан и настроен до вызова конструктора. DBxConBase является "персональным"
    /// для этого соединения, т.к. DBxCon.Dispose() разрушит DBxConBase
    /// </summary>
    /// <param name="source">Внутренний объект соединения</param>
    public DBxCon(DBxConBase source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif

      _Source = source;
    }

    /// <summary>
    /// Закрывает соединение.
    /// Вызывает метод Dispose() для нижележащего соединения, которое было передано в конструктор.
    /// После этого связь с нижележащим соединением разрывается
    /// </summary>
    /// <param name="disposing">True, если был вызван метод Dispose(), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (_Source != null)
      {
        _Source.Dispose();
        _Source = null;
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Базовый объект соединения, к которому клиентское приложение не должно иметь доступа
    /// </summary>
    protected DBxConBase Source { get { return _Source; } }
    private DBxConBase _Source;

    /// <summary>
    /// Текстовое представление (для отладки)
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s;
      if (_Source == null)
        s = "DBxCon отсоединен от DBxConBase";
      else
        s = "DBxConBase=" + _Source.ToString();
      if (IsDisposed)
        s = "DBxCon is disposed. " + s;
      return s;
    }

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то при возникновении ошибки при выполнении запросов,
    /// на сервере записывается LOG-файл с ошибкой
    /// </summary>
    public bool LogoutSqlExceptions
    {
      get { return _Source.LogoutSqlExceptions; }
      set { _Source.LogoutSqlExceptions = value; }
    }

    /// <summary>
    /// Если свойство установлено в true, то при выполнении запросов AddRecord(), SetValues(), SetValue(),
    /// значения строковых полей будут обрезаться, если их длина превышает заявляенную в структуре базы данных
    /// </summary>
    public bool TrimValues
    {
      get { return _Source.TrimValues; }
      set { _Source.TrimValues = value; }
    }

    /// <summary>
    /// Время выполнения команд в секундах. 0 - бесконечное ожидание выполнения запроса.
    /// Это свойство позволяет только получить значение. Для установки необходимо иметь доступ
    /// к базовому объекту DBxConBase (безопасность)
    /// </summary>
    public int CommandTimeout { get { return _Source.CommandTimeout; } }

    /// <summary>
    /// Выполняется ли проверка существования описаний таблиц и полей в реальной структуры таблицы
    /// при выполнении запросов.
    /// Это свойство позволяет только получить значение. Для установки необходимо иметь доступ
    /// к базовому объекту DBxConBase (безопасность)
    /// </summary>
    public bool NameCheckingEnabled { get { return _Source.NameCheckingEnabled; } }

    /// <summary>
    /// Текущий выполняющийся запрос (для отладки)
    /// </summary>
    public DBxConQueryInfo CurrentQuery { get { return _Source.CurrentQuery; } }

    #endregion

    #region Информация о базе данных

    /// <summary>
    /// Источник для создания структуры базы данных
    /// </summary>
    public IDBxStructSource StructSource { get { return new DBxRealStructSource(_Source.Entry); } }

    #endregion

    #region Выполнение запросов

    #region SELECT (DataTable)

    /// <summary>
    /// Загрузка всей таблицы.
    /// Загружаются все поля SELECT * FROM [TableName] 
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Заполненная таблица DataTable</returns>
    public DataTable FillSelect(string tableName)
    {
      return FillSelect(tableName, null, null, null);
    }

    /// <summary>
    /// Загрузка выбранных полей всей таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <returns>Заполненная таблица DataTable</returns>
    public DataTable FillSelect(string tableName, DBxColumns columnNames)
    {
      return FillSelect(tableName, columnNames, null, null);
    }

    /// <summary>
    /// Загрузка выбранных полей для строк таблицы, отобранных по условию
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <param name="where">Условие фильтрации</param>
    /// <returns>Заполненная таблица DataTable</returns>
    public DataTable FillSelect(string tableName, DBxColumns columnNames, DBxFilter where)
    {
      return FillSelect(tableName, columnNames, where, null);
    }

    /// <summary>
    /// Загрузка выбранных полей для строк таблицы, отобранных по условию с заданным порядком сортировки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <param name="where">Условие фильтрации</param>
    /// <param name="orderBy">Порядок сортировки</param>
    /// <returns>Заполненная таблица DataTable</returns>
    public DataTable FillSelect(string tableName, DBxColumns columnNames, DBxFilter where, DBxOrder orderBy)
    {
      return _Source.FillSelect(tableName, columnNames, where, orderBy);
    }


    /// <summary>
    /// Получение списка уникальных значений поля SELECT DISTINCT
    /// В полученной таблице будет одно поле. Таблица будет упорядочена по этому полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="where">Необязательный фильтр записей</param>
    /// <returns>Таблица с единственной колонкой</returns>
    public DataTable FillUniqueColumnValues(string tableName, string columnName, DBxFilter where)
    {
      return _Source.FillUniqueColumnValues(tableName, columnName, where);
    }

    /// <summary>
    /// Универсальный метод выполнения запроса SELECT.
    /// </summary>
    /// <param name="info">Параметры запроса</param>
    /// <returns>Заполненная таблица</returns>
    public DataTable FillSelect(DBxSelectInfo info)
    {
      return _Source.FillSelect(info);
    }

    #endregion

    #region SELECT (DbDataReader)

    /// <summary>
    /// Загрузка всей таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Объект DataReader</returns>
    public DbDataReader ReaderSelect(string tableName)
    {
      return ReaderSelect(tableName, null, null, null);
    }

    /// <summary>
    /// Загрузка выбранных полей всей таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <returns>Объект DataReader</returns>
    public DbDataReader ReaderSelect(string tableName, DBxColumns columnNames)
    {
      return ReaderSelect(tableName, columnNames, null, null);
    }

    /// <summary>
    /// Загрузка выбранных полей для строк таблицы, отобранных по условию.
    /// Объект DataReader должен быть закрыт по окончании чтения.
    /// На время чтения текущее соединение занято и не должно использоваться для других запросов.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей. 
    /// Список может содержать поля с точками (например, "Человек.Фамилия") для выполнения LEFT JOIN.
    /// Если список не задан, будут возвращены все поля таблицы</param>
    /// <param name="where">Условие фильтрации</param>
    /// <returns>Объект DataReader</returns>
    public DbDataReader ReaderSelect(string tableName, DBxColumns columnNames, DBxFilter where)
    {
      return ReaderSelect(tableName, columnNames, where, null);
    }

    /// <summary>
    /// Загрузка выбранных полей для строк таблицы, отобранных по условию.
    /// Объект DataReader должен быть закрыт по окончании чтения.
    /// На время чтения текущее соединение занято и не должно использоваться для других запросов.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список имен полей, разделенных запятыми</param>
    /// <param name="where">Условие фильтрации</param>
    /// <param name="orderBy">Порядок сортировки</param>
    /// <returns>Объект DataReader</returns>
    public DbDataReader ReaderSelect(string tableName, DBxColumns columnNames, DBxFilter where, DBxOrder orderBy)
    {
      return _Source.ReaderSelect(tableName, columnNames, where, orderBy);
    }

    /// <summary>
    /// Универсальный метод выполнения запроса SELECT.
    /// </summary>
    /// <param name="info">Параметры запроса</param>
    /// <returns>Объект DataReader</returns>
    public DbDataReader ReaderSelect(DBxSelectInfo info)
    {
      return _Source.ReaderSelect(info);
    }

    #endregion

    #region Для IDBxCacheSource

    /// <summary>
    /// Реализация интерфейса IDBxCacheSource.
    /// Загружает страницы для таблицы кэша, используя FillSelect() и фильтр NumRangeFilter.
    /// </summary>
    public DBxCacheLoadResponse LoadCachePages(DBxCacheLoadRequest request)
    {
      if (request == null)
        throw new ArgumentNullException("request");

      DBxCacheLoadResponse response = new DBxCacheLoadResponse();
      string DBIdentityMD5 = DataTools.MD5SumFromString(Source.DB.DBIdentity);

      #region Страницы таблиц

      if (request.HasTablePages)
      {
        // TODO: Оптимизация
        for (int i = 0; i < request.TablePages.Count; i++)
        {
          DBxCacheLoadRequest.PageInfo pi = request.TablePages[i];

          string IdColumnName = _Source.Validator.CheckTablePrimaryKeyInt32(pi.TableName);
          NumRangeFilter Filter = new NumRangeFilter(IdColumnName, pi.FirstId, pi.FirstId + DBxCache.PageRowCount - 1);
          DataTable Table = FillSelect(pi.TableName, pi.ColumnNames, Filter);

          DBxCacheTablePage page = new DBxCacheTablePage(
            DBIdentityMD5,
            pi.TableName,
            pi.FirstId,
            DataTools.MD5SumFromString(pi.ColumnNames.AsString),
            Table, _Source.Entry.DB.Struct.Tables[pi.TableName].PrimaryKey.AsString);

          response.TablePages.Add(pi.InternalKey, page);
        }
      }

      #endregion

      #region Индивидуальные значения

      if (request.HasIndividualValues)
      {
        for (int i = 0; i < request.IndividualValues.Count; i++)
        {
          DBxCacheLoadRequest.IndividualInfo ii = request.IndividualValues[i];

          _Source.Validator.CheckTablePrimaryKeyInt32(ii.TableName);

          object v = GetValue(ii.TableName, ii.Id, ii.ColumnName);

          DBxCacheIndividualValue page = new DBxCacheIndividualValue(
            DBIdentityMD5,
            ii.TableName,
            ii.Id,
            ii.ColumnName,
            v);

          response.IndividualValues.Add(ii.InternalKey, page);
        }
      }

      #endregion

      return response;
    }

    string IDBxCacheSource.DBIdentity
    {
      get { return _Source.DB.DBIdentity; }
    }

    DBxTableCacheInfo IDBxCacheSource.GetTableCacheInfo(string tableName)
    {
      return null;
    }

    void IDBxCacheSource.ClearCachePages(string tableName, DBxColumns columnNames, Int32[] firstIds)
    {
      // Ничего не делаем
    }

    #endregion

    #region Поиск строки по условию

    #region FindRecord

    /// <summary>
    /// Найти строку с заданным значением поля
    /// Возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля условия</param>
    /// <param name="value">Значение поля условия</param>
    /// <returns>Идентификатор строки или 0</returns>
    public Int32 FindRecord(string tableName, string columnName, object value)
    {
      return _Source.FindRecord(tableName, columnName, value);
    }

    /// <summary>
    /// Найти строку с заданными значениями полей
    /// Возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Пары ИмяПоля-Значение</param>
    /// <returns>Идентификатор строки или 0</returns>
    public Int32 FindRecord(string tableName, IDictionary columnNamesAndValues)
    {
      string[] ColumnNames;
      object[] Values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out ColumnNames, out Values);
      return FindRecord(tableName, new DBxColumns(ColumnNames), Values);
    }

    /// <summary>
    /// Найти строку с заданными значениями полей
    /// Возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей условия</param>
    /// <param name="values">Значения полей условия</param>
    /// <returns>Идентификатор строки или 0</returns>
    public Int32 FindRecord(string tableName, DBxColumns columnNames, object[] values)
    {
      return _Source.FindRecord(tableName, columnNames, values);
    }

    /// <summary>
    /// Найти строку с заданными значениями полей. 
    /// Если задан порядок сортировки, то отыскиваются все строки с заданными значениями, 
    /// они упорядочиваются и возвращается идентификатор первой строки. Если OrderBy=null,
    /// то возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей условия</param>
    /// <param name="values">Значения полей условия</param>
    /// <param name="orderBy">Порядок сортировки (может быть null)</param>
    /// <returns>Идентификатор строки или 0</returns>
    public Int32 FindRecord(string tableName, DBxColumns columnNames, object[] values, DBxOrder orderBy)
    {
      return _Source.FindRecord(tableName, columnNames, values, orderBy);
    }

    ///// <summary>
    ///// Поиск любой строки таблицы без всяких условий
    ///// </summary>
    ///// <param name="tableName">Имя таблицы</param>
    ///// <returns>Идентификатор первой попавшейся записи или 0, если таблица не содержит записей</returns>
    //public Int32 FindRecord(string tableName)
    //{
    //  return FindRecord(tableName, (DBxFilter)null, (DBxOrder)null);
    //}

    /// <summary>
    /// Поиск первой строки, удовлетворяющей условию при заданном порядке строк
    /// Если задан порядок сортировки, то отыскиваются все строки с заданными значениями, 
    /// они упорядочиваются и возвращается идентификатор первой строки. Если OrderBy=null,
    /// то возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр</param>
    /// <param name="orderBy">Порядок сортировки, при котором нужна первая строка. Может быть null</param>
    /// <returns>Идентификатор первой записи, удовлетворяющей условию</returns>
    public Int32 FindRecord(string tableName, DBxFilter where, DBxOrder orderBy)
    {
      return _Source.FindRecord(tableName, where, orderBy);
    }

    /// <summary>
    /// Поиск первой строки, удовлетворяющей условию.
    /// Если есть несколько подходящих строк, то возвращается идентификатор первой попавшейся строки
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр</param>
    public Int32 FindRecord(string tableName, DBxFilter where)
    {
      return FindRecord(tableName, where, false);
    }

    /// <summary>
    /// Поиск записи в таблице.
    /// Таблица должна иметь первичный ключ по числовому полю.
    /// Возвращает идентификатор строки (значение первичного ключа), если запись найдена.
    /// Возвращает 0, если запись не найдена.
    /// Можно задать дополнительное ограничение на уникальнойсть найденной записи.
    /// Если огранчиение указано и найдено больше одной строки, возвращается 0.
    /// Имена полей в фильтрах <paramref name="where"/> могут содержать точки (ссылочные поля).
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие поиска</param>
    /// <param name="singleOnly">Если true, то найденная запись должна быть уникальной</param>
    /// <returns>Имя столбца (обычно, "Id")</returns>
    public Int32 FindRecord(string tableName, DBxFilter where, bool singleOnly)
    {
      return _Source.FindRecord(tableName, where, singleOnly);
    }

    #endregion

#if XXX
    #region FindRecordValue

    /// <summary>
    /// Найти строку в таблице с указанными значениями полей (ключами) и вернуть значение
    /// требуемого поля. Если строка, удовлетворяющая условию не найдена, возвращается null.
    /// Если есть несколько строк, удовлетворяющих условию, то какая из них будет использована, 
    /// не определено
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="searchColumnNames">Имена ключевых полей для поиска. Ссылочные поля (с точками) не поддерживаются</param>
    /// <param name="searchValues">Значения ключевых полей</param>
    /// <param name="resColumnName">Имя результирующего поля. Ссылочные поля (с точками) не поддерживаются</param>
    /// <returns>Значение поля <paramref name="resColumnName"/> или null, если строка не найдена</returns>
    public object FindRecordValue(string tableName, DBxColumns searchColumnNames, object[] searchValues, string resColumnName)
    {
      return _Source.FindRecordValue(tableName, searchColumnNames, searchValues, resColumnName);
    }

    /// <summary>
    /// Найти строку в таблице с указанным значением заданного поля (используется ValueFilter) и вернуть значение
    /// требуемого поля. Если строка, удовлетворяющая условию не найдена, возвращается null.
    /// Если есть несколько строк, удовлетворяющих условию, то какая из них будет использована, 
    /// не определено.
    /// Этот метод не поддерживает использование ссылочных полей (с точками).
    /// Используйте метод FillSelect() при необходимости.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="searchColumnName">Имя поля фильтра. Ссылочные поля (с точками) не поддерживаются</param>
    /// <param name="searchValue">Значение поля фильтра <paramref name="searchColumnName"/></param>
    /// <param name="resColumnName">Имя результирующего поля. Ссылочные поля (с точками) не поддерживаются</param>
    /// <returns>Значение поля <paramref name="resColumnName"/> или null, если строка не найдена</returns>
    public object FindRecordValue(string tableName, string searchColumnName, object searchValue, string resColumnName)
    {
      return _Source.FindRecordValue(tableName, searchColumnName, searchValue, resColumnName);
    }

    #endregion
#endif

    #region GetIds

    /// <summary>
    /// Получить массив идентификаторов строк с заданным значением поля
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя поля условия</param>
    /// <param name="value">Значение поля условия</param>
    /// <returns>Массив идентификаторов</returns>
    public IdList GetIds(string tableName, string columnName, object value)
    {
      return _Source.GetIds(tableName, columnName, value);
    }

    /// <summary>
    /// Получить массив идентификаторов строк с заданными значениями полей
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Пары ИмяПоля-Значение</param>
    /// <returns>Массив идентификаторов строк</returns>
    public IdList GetIds(string tableName, IDictionary columnNamesAndValues)
    {
      string[] ColumnNames;
      object[] Values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out ColumnNames, out Values);
      return GetIds(tableName, new DBxColumns(ColumnNames), Values);
    }

    /// <summary>
    /// Получить массив идентификаторов строк таблицы с заданными значениями полей
    /// </summary>
    /// <param name="tableName">Таблица</param>
    /// <param name="columnNames">Имена полей условия</param>
    /// <param name="values">Значения полей условия</param>
    /// <returns>Массив идентификаторов</returns>
    public IdList GetIds(string tableName, DBxColumns columnNames, object[] values)
    {
      return _Source.GetIds(tableName, columnNames, values);
    }

    /// <summary>
    /// Получение массива идентификаторов первичного ключа (обычно, поля "Id") для всех строк, удовлетворяющих
    /// условию.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие (фильтр)</param>
    /// <returns>Массив идентификаторов (значений поля "Id")</returns>
    public IdList GetIds(string tableName, DBxFilter where)
    {
      return _Source.GetIds(tableName, where);
    }

    #endregion

    #region GetInheritorIds

    /// <summary>
    /// Возвращает список идентификаторов дочерних строк для таблиц, в которых реализована
    /// иерахическая структура с помощью поля, ссылающегося на эту же таблицу, которое задает родительский
    /// элемент. Родительская строка <paramref name="parentId"/> не входит в список
    /// Метод не зацикливается, если структура дерева нарушена (зациклено).
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="parentIdColumnName">Имя ссылочного столбца, например "ParentId"</param>
    /// <param name="parentId">Идентификатор родительской строки. Если 0, то будут возвращены 
    /// идентификаторы строк узлов верхнего уровня или всех строк (при <paramref name="nested"/>=true)</param>
    /// <param name="nested">true, если требуется рекурсивный поиск. false, если требуется вернуть только непосредственные дочерние элементы</param>
    /// <returns>Список идентификаторов дочерних элементов</returns>
    public IdList GetInheritorIds(string tableName, string parentIdColumnName, Int32 parentId, bool nested)
    {
      Int32 LoopedId;
      return GetInheritorIds(tableName, parentIdColumnName, parentId, nested, null, out LoopedId);
    }

    /// <summary>
    /// Возвращает список идентификаторов дочерних строк для таблиц, в которых реализована
    /// иерахическая структура с помощью поля, ссылающегося на эту же таблицу, которое задает родительский
    /// элемент. Родительская строка <paramref name="parentId"/> не входит в список
    /// Метод не зацикливается, если структура дерева нарушена (зациклено).
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="parentIdColumnName">Имя ссылочного столбца, например "ParentId"</param>
    /// <param name="parentId">Идентификатор родительской строки. Если 0, то будут возвращены 
    /// идентификаторы строк узлов верхнего уровня или всех строк (при <paramref name="nested"/>=true)</param>
    /// <param name="nested">true, если требуется рекурсивный поиск. false, если требуется вернуть только непосредственные дочерние элементы</param>
    /// <param name="where">Дополнительный фильтр. Может быть null, если фильтра нет</param>
    /// <returns>Список идентификаторов дочерних элементов</returns>
    public IdList GetInheritorIds(string tableName, string parentIdColumnName, Int32 parentId, bool nested, DBxFilter where)
    {
      Int32 LoopedId;
      return GetInheritorIds(tableName, parentIdColumnName, parentId, nested, where, out LoopedId);
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
    public IdList GetInheritorIds(string tableName, string parentIdColumnName, Int32 parentId, bool nested, DBxFilter where, out Int32 loopedId)
    {
      _Source.Validator.CheckTableName(tableName, DBxAccessMode.ReadOnly);
      _Source.Validator.CheckTablePrimaryKeyInt32(tableName);
      _Source.Validator.CheckTableColumnName(tableName, parentIdColumnName, false, DBxAccessMode.ReadOnly);

      loopedId = 0;

      DBxFilter SrcFilter;
      if (parentId == 0)
        SrcFilter = new ValueFilter(parentIdColumnName, null, typeof(Int32));
      else
        SrcFilter = new ValueFilter(parentIdColumnName, parentId);

      IdList ResIds = new IdList();

      while (true)
      {
        DBxFilter Filter2;
        if (where == null)
          Filter2 = SrcFilter;
        else
          Filter2 = new AndFilter(SrcFilter, where);

        IdList ResIds2 = GetIds(tableName, Filter2);
        if (ResIds2.Count == 0)
          return ResIds; // рекурсия закончилась нормально

        if (ResIds.Count == 0)
        {
          // Первый такт
          if (parentId != 0)
          {
            if (ResIds2.Contains(parentId))
            {
              ResIds2.Remove(parentId);
              loopedId = parentId;
            }
          }

          if (!nested)
            return ResIds2; // нерекурсивный вызов
        }
        else
        {
          // второй и далее уровень

          Int32 LoopedId2; // отдельная переменная нужна, чтобы не заменить найденный ранее LoopedId на 0
          if (ResIds2.ContainsAny(ResIds, out LoopedId2))
          {
            // Зациклилось
            loopedId = LoopedId2;
            ResIds2.Remove(ResIds);
            if (ResIds2.Count == 0)
              return ResIds; // найден только зацилившийся узел
          }
        }

        ResIds.Add(ResIds2);
        SrcFilter = new IdsFilter(parentIdColumnName, ResIds2);
      }
    }

#if XXX // Вариант 1
    public IdList GetInheritorIds(string TableName, Int32 ParentId, string ParentIdColumnName, bool Nested, DBxFilter Where, out Int32 LoopedId)
    {
      FSource.CheckTableName(TableName, DBxAccessMode.ReadOnly);
      FSource.CheckTablePrimaryKeyInt32(TableName);
      FSource.CheckTableColumnName(TableName, ParentIdColumnName, false, DBxAccessMode.ReadOnly);
      
      LoopedId = 0;

    #region Дочерние элементы первого уровня

      DBxFilter SrcFilter;

      if (ParentId == 0)
        SrcFilter = new ValueFilter(ParentIdColumnName, null, typeof(Int32));
      else
        SrcFilter = new ValueFilter(ParentIdColumnName, ParentId);
      DBxFilter Filter2;
      if (Where == null)
        Filter2 = SrcFilter;
      else
        Filter2 = new AndFilter(SrcFilter, Where);
      IdList ResIds = GetIds(TableName, Filter2);
      if (ParentId != 0)
      {
        if (ResIds.Contains(ParentId))
          LoopedId = ParentId;
      }
      if (!Nested)
        return ResIds;

      if (ResIds.Count==0)
        return IdList.Empty;

    #endregion

    #region Рекурсивный поиск

      IdList ResIds2 = ResIds;
      while (true)
      {
        SrcFilter = new IdsFilter(ParentIdColumnName, ResIds2);
        if (Where == null)
          Filter2 = SrcFilter;
        else
          Filter2 = new AndFilter(SrcFilter, Where);

        ResIds2 = GetIds(TableName, Filter2);
        if (ResIds2.Count == 0)
          return ResIds; // рекурсия закончилась
        if (ResIds2.ContainsAny(ResIds, out LoopedId))
        { 
          // зациклилось
          return ResIds | ResIds2;
        }

        ResIds |= ResIds2;
      }

    #endregion
    }
#endif

    #endregion

    #endregion

    #region Информационные методы

    /// <summary>
    /// Получить общее число записей в таблице
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Число записей</returns>
    public int GetRecordCount(string tableName)
    {
      return _Source.GetRecordCount(tableName);
    }

    /// <summary>
    /// Получить число записей в таблице, удовлетворяющих условию
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Условие</param>
    /// <returns>Число записей</returns>
    public int GetRecordCount(string tableName, DBxFilter where)
    {
      return _Source.GetRecordCount(tableName, where);
    }

    /// <summary>
    /// Возвращает true, если в таблице нет ни одной строки.
    /// Тоже самое, что GetRecordCount()==0, но может быть оптимизировано.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <returns>Отсутствие записей</returns>
    public bool IsTableEmpty(string tableName)
    {
      return _Source.IsTableEmpty(tableName);
    }

    #endregion

    #region Получение значений полей

    /// <summary>
    /// Получение значения для одного поля. Имя поля может содержать точки для
    /// извлечения значения из зависимой таблицы. 
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// Имя поля может содержать точки для получения значения ссылочного поля с помощью INNER JOIN.
    /// Если <paramref name="id"/>=0, возвращает null.
    /// Выбрасывает исключение DBxRecordNotFoundException, если задан идентификатор несуществующей записи.
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которой выполняется поиск</param>
    /// <param name="id">Идентификатор строки. Может быть 0</param>
    /// <param name="columnName">Имя поля (может быть с точками)</param>
    /// <returns>Значение</returns>
    public object GetValue(string tableName, Int32 id, string columnName)
    {
      return _Source.GetValue(tableName, id, columnName);
    }

    /// <summary>
    /// Получение значения для одного поля. Имя поля может содержать точки для
    /// извлечения значения из зависимой таблицы. Расширенная версия возвращает
    /// значение поля по ссылке, а как результат возвращается признак того, что
    /// строка найдена.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// Имя поля может содержать точки для получения значения ссылочного поля с помощью INNER JOIN.
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которой выполняется поиск</param>
    /// <param name="id">Идентификатор строки. Может быть 0, тогда возвращается Value=null</param>
    /// <param name="columnName">Имя поля (может быть с точками)</param>
    /// <param name="value">Сюда по ссылке записывается значение</param>
    /// <returns>true, если поле было найдено</returns>
    public bool GetValue(string tableName, Int32 id, string columnName, out object value)
    {
      return _Source.GetValue(tableName, id, columnName, out value);
    }

    /// <summary>
    /// Получить значения для заданного списка полей.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// Если <paramref name="id"/>=0, возвращается массив значений null подходящей длины.
    /// Выбрасывает исключение DBxRecordNotFoundException, если задан идентификатор несуществующей записи.
    /// Имена полей могут содержать точки для получения значений ссылочных полей с помощью INNER JOIN.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки таблицы (значение первичного ключа)</param>
    /// <param name="columnNames">Список столбцов</param>
    /// <returns>Массив значений полей строки</returns>
    public object[] GetValues(string tableName, Int32 id, DBxColumns columnNames)
    {
      return _Source.GetValues(tableName, id, columnNames);
    }

    /// <summary>
    /// Получить значения для заданного списка полей.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// Если задан нулевой или несуществующий идентификатор записи, возвращается массив значений null
    /// требуемой длины.
    /// Имена полей могут содержать точки для получения значений ссылочных полей с помощью INNER JOIN.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки таблицы (значение первичного ключа)</param>
    /// <param name="columnNames">Список столбцов</param>
    /// <returns>Массив значений полей строки</returns>
    public object[] GetValues(string tableName, Int32 id, string columnNames)
    {
      DBxColumns ColumnNames2 = new DBxColumns(columnNames);
      return GetValues(tableName, id, ColumnNames2);
    }

    /// <summary>
    /// Получить максимальное значение числового поля
    /// Строки таблицы, содержащие значения NULL, игнорируются
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для поиска среди всех строк таблицы)</param>
    /// <returns>Максимальное значение или null</returns>
    public object GetMaxValue(string tableName, string columnName, DBxFilter where)
    {
      return _Source.GetMaxValue(tableName, columnName, where);
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
    public object[] GetValuesForMax(string tableName, DBxColumns columnNames,
      string maxColumnName, DBxFilter where)
    {
      return _Source.GetValuesForMax(tableName, columnNames, maxColumnName, where);
    }

    /// <summary>
    /// Получить минимальное значение числового поля
    /// Строки таблицы, содержащие значения NULL, игнорируются
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для поиска среди всех строк таблицы)</param>
    /// <returns>Минимальное значение или null</returns>
    public object GetMinValue(string tableName, string columnName, DBxFilter where)
    {
      return _Source.GetMinValue(tableName, columnName, where);
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
    public object[] GetValuesForMin(string tableName, DBxColumns columnNames,
      string minColumnName, DBxFilter where)
    {
      return _Source.GetValuesForMin(tableName, columnNames, minColumnName, where);
    }

    /// <summary>
    /// Получить суммарное значение числового поля для выбранных записей
    /// Строки таблицы, содержащие значения NULL, игнорируются
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя числового поля</param>
    /// <param name="where">Фильтр (null для суммирования всех строк таблицы)</param>
    /// <returns></returns>
    public object GetSumValue(string tableName, string columnName, DBxFilter where)
    {
      return _Source.GetSumValue(tableName, columnName, where);
    }


#if XXX
    /// <summary>
    /// Получить значение для ссылочного поля
    /// Анализирует структуру таблицы, чтобы узнать имя таблицы, на которую ссылается
    /// поле.
    /// </summary>
    /// <param name="TableName">Имя таблицы, содержащей ссылочное поле</param>
    /// <param name="RefFieldName">Имя поля в виде ИмяСсылочногоПоля.ИмяТребуемогоПоля</param>
    /// <param name="RefValue">Значение ссылочного поля</param>
    /// <returns>Значение требуемого поля</returns>
    public object GetRefValue(string TableName, string RefFieldName, int RefValue)
    {
      if (String.IsNullOrEmpty(TableName))
        throw new ArgumentNullException("TableName");
      if (String.IsNullOrEmpty(RefFieldName))
        throw new ArgumentNullException("RefFieldName");

      if (RefValue == 0)
        return null;

      // Ищем описание таблицы
      DBStruct.Table TableDef = DBStruct.Tables[TableName];
      if (TableDef == null)
        throw new ArgumentException("Не найдено описание таблицы \"" + TableName + "\"");

      // Ищем описание поля
      int p = RefFieldName.IndexOf('.');
      if (p < 0)
        throw new ArgumentException("Неправильный формат имени ссылочного поля \"" + RefFieldName + "\". Нет точки");
      string MainFieldName = RefFieldName.Substring(0, p);
      DBStruct.Field FieldDef = TableDef.Fields[MainFieldName];
      if (FieldDef == null)
        throw new ArgumentException("Неправильное имя поля \"" + RefFieldName + "\". Описание основного поля \"" + MainFieldName +
          "\" не найдено в таблице \"" + TableDef.TableName + "\"");
      if (FieldDef.FieldType != DBStruct.AccDepFieldType.Reference)
        throw new ArgumentException("Неправильное имя поля \"" + RefFieldName + "\". Основное поле \"" + MainFieldName +
          "\" не является ссылочным");

      string WantedFieldName = RefFieldName.Substring(p + 1);
      return GetValue(FieldDef.MasterTableName, RefValue, WantedFieldName);
    }
#endif

    #endregion

    #region GetXxxValues()

    /// <summary>
    /// Получить строковые значения поля без повторов.
    /// Если в таблице встречаются значения NULL, то они пропускаются.
    /// Возвращаемый массив сортируется.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя строкового поля. Может содержать точки, если требуется получить значения ссылочного поля</param>
    /// <param name="where">Фильтр. Если null, то просматриваются все строки таблицы</param>
    /// <returns>Массив значений</returns>
    public string[] GetUniqueStringValues(string tableName, string columnName, DBxFilter where)
    {
      return _Source.GetUniqueStringValues(tableName, columnName, where);
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
    public int[] GetUniqueIntValues(string tableName, string columnName, DBxFilter where)
    {
      return _Source.GetUniqueIntValues(tableName, columnName, where);
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
    public long[] GetUniqueInt64Values(string tableName, string columnName, DBxFilter where)
    {
      return _Source.GetUniqueInt64Values(tableName, columnName, where);
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
    public float[] GetUniqueSingleValues(string tableName, string columnName, DBxFilter where)
    {
      return _Source.GetUniqueSingleValues(tableName, columnName, where);
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
    public double[] GetUniqueDoubleValues(string tableName, string columnName, DBxFilter where)
    {
      return _Source.GetUniqueDoubleValues(tableName, columnName, where);
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
    public decimal[] GetUniqueDecimalValues(string tableName, string columnName, DBxFilter where)
    {
      return _Source.GetUniqueDecimalValues(tableName, columnName, where);
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
    public DateTime[] GetUniqueDateTimeValues(string tableName, string columnName, DBxFilter where)
    {
      return _Source.GetUniqueDateTimeValues(tableName, columnName, where);
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
    public Guid[] GetUniqueGuidValues(string tableName, string columnName, DBxFilter where)
    {
      return _Source.GetUniqueGuidValues(tableName, columnName, where);
    }

    #endregion

    #region Запись значений полей

    /// <summary>
    /// Установить значения одного поля для одной строки.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки (значение первичного ключа)</param>
    /// <param name="columnName">Имя устанавливаемого поля</param>
    /// <param name="value">Значение</param>
    public void SetValue(string tableName, Int32 id, string columnName, object value)
    {
      _Source.SetValue(tableName, id, columnName, value);
    }

    /// <summary>
    /// Установить значение одного поля для нескольких строк таблицы.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам. Если null, то значение устанавливается для всех строк таблицы</param>
    /// <param name="columnName">Имя устанавливаемого поля</param>
    /// <param name="value">Значение</param>
    public void SetValue(string tableName, DBxFilter where, string columnName, object value)
    {
      _Source.SetValue(tableName, where, columnName, value);
    }


    /// <summary>
    /// Установить значения полей для записи с заданным идентификатором
    /// В отличие от других вариантов вызова функции, выполняется 5 попыток 
    /// установить значение, если происходит блокировка записи в другом потоке
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор записи</param>
    /// <param name="columnNames">Имена устанавливаемых полей</param>
    /// <param name="values">Записываемые значения полей</param>
    public void SetValues(string tableName, Int32 id, DBxColumns columnNames, object[] values)
    {
      for (int i = 0; i < 5; i++)
      {
        try
        {
          _Source.SetValues(tableName, id, columnNames, values);
          break; // 22.06.2016
        }
        catch
        {
          if (i == 4)
            throw;
        }
        Thread.Sleep(100); // 22.06.2016
      }
    }

    /// <summary>
    /// Установлка значений нескольких полей для нескольких строк таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам таблицы. Если null, то значение устанавливается для всех строк таблицы</param>
    /// <param name="columnNames">Имена устаналиваемых полей</param>
    /// <param name="values">Устанавливаемые значения. Длина массива должна совпадать с <paramref name="columnNames"/></param>
    public void SetValues(string tableName, DBxFilter where, DBxColumns columnNames, object[] values)
    {
      _Source.SetValues(tableName, where, columnNames, values);
    }

    /// <summary>
    /// Установка значений нескольких полей для одной таблицы.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки таблицы (значение первичного ключа)</param>
    /// <param name="columnNamesAndValues">Имена устанавливаемых столбцов и значения</param>
    public void SetValues(string tableName, Int32 id, IDictionary columnNamesAndValues)
    {
      string[] columnNames;
      object[] values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out columnNames, out values);
      SetValues(tableName, id, new DBxColumns(columnNames), values);
    }

    /// <summary>
    /// Установлка значений нескольких полей для нескольких строк таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам таблицы. Если null, то значение устанавливается для всех строк таблицы</param>
    /// <param name="columnNamesAndValues">Имена устанавливаемых столбцов и значения</param>
    public void SetValues(string tableName, DBxFilter where, IDictionary columnNamesAndValues)
    {
      string[] columnNames;
      object[] values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out columnNames, out values);
      SetValues(tableName, where, new DBxColumns(columnNames), values);
    }

#if XXX // такая версия только сбивает с толку
    /// <summary>
    /// 
    /// </summary>
    /// <param name="TableName"></param>
    /// <param name="Id"></param>
    /// <param name="ColumnNames"></param>
    /// <param name="Values"></param>
    public void SetValues(string TableName, Int32 Id, string ColumnNames, object[] Values)
    {
      SetValues(TableName, Id, new DBxColumns(ColumnNames), Values);
    }
#endif

    #endregion

    #region Добавление записей (INSERT INTO)

    /*
    /// <summary>
    /// Возвращает объект блокировки, используемый при добавлении записи
    /// </summary>
    /// <param name="TableName">Имя таблицы</param>
    /// <returns>Объект блокировки</returns>
    public ServerExecLock GetAddRecordLocker(string TableName)
    {
      return DBStruct.Tables[TableName].AddRecordLocker;
    } */

    #region Версии для таблицы с числовым идентификатором (возвращают поле Id)

    /// <summary>
    /// Добавляет новую строку в таблицу и возвращает ее идентификатор (поле Id).
    /// В строку записывается значение только одного поля (не считая автоинкрементного), поэтому, обычно следует использовать 
    /// другие перегрузки.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя устанавливаемого столбца</param>
    /// <param name="value">Значение поля</param>
    /// <returns>Идентификатор строки таблицы (значение первичного ключа)</returns>
    public Int32 AddRecordWithIdResult(string tableName, string columnName, object value)
    {
      object[] Values = new object[1];
      Values[0] = value;
      return AddRecordWithIdResult(tableName, new DBxColumns(columnName), Values);
    }

    /// <summary>
    /// Добавляет новую строку в таблицу и возвращает ее идентификатор (поле Id).
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Имена устанавливаемых полей и значения</param>
    /// <returns>Идентификатор строки таблицы (значение первичного ключа)</returns>
    public Int32 AddRecordWithIdResult(string tableName, IDictionary columnNamesAndValues)
    {
      string[] ColumnNames;
      object[] Values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out ColumnNames, out Values);
      return AddRecordWithIdResult(tableName, new DBxColumns(ColumnNames), Values);
    }

    /// <summary>
    /// Добавляет новую строку в таблицу и возвращает ее идентификатор (поле Id).
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена устанавливаемых столбцов</param>
    /// <param name="values">Устанавливаемые значения. Длина массива должна совпадать с ColumnNames</param>
    /// <returns>Идентификатор строки таблицы (значение первичного ключа)</returns>
    public Int32 AddRecordWithIdResult(string tableName, DBxColumns columnNames, object[] values)
    {
      return _Source.AddRecordWithIdResult(tableName, columnNames, values);
    }

    #endregion

    #region Без возврата идентификатора

    /// <summary>
    /// Добавляет новую строку в таблицу, но не возвращает ее идентификатор.
    /// Эта перегрузка позволяет задать только одно значение поля для строки,
    /// поэтому имеет очень ограниченное применение.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnName">Имя столбца, для которого задается значение</param>
    /// <param name="value">Значение для поля <paramref name="columnName"/></param>
    public void AddRecord(string tableName, string columnName, object value)
    {
      _Source.AddRecord(tableName, columnName, value);
    }

    /// <summary>
    /// Добавляет новую строку в таблицу, но не возвращает ее идентификатор.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Имена столбцов и значения полей</param>
    public void AddRecord(string tableName, IDictionary columnNamesAndValues)
    {
      string[] ColumnNames;
      object[] Values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out ColumnNames, out Values);
      AddRecord(tableName, new DBxColumns(ColumnNames), Values);
    }

    /// <summary>
    /// Добавить строку в таблицу.
    /// Список столбцов <paramref name="columnNames"/> может содержать, а может и не содержать
    /// ключевое поле. Если ключевого поля нет, то значение присваивается автоматически.
    /// Полученный идентификатор не возвращается, благодаря чему можно ограничиться запросом INSERT INTO 
    /// без дополнительных действий по получению идентификатора.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена столбцов. В списке не должно быть поля первичного ключа</param>
    /// <param name="values">Значения. Порядок значений должен соответствовать списку столбцов</param>
    public void AddRecord(string tableName, DBxColumns columnNames, object[] values)
    {
      _Source.AddRecord(tableName, columnNames, values);
    }

    #endregion

    #region Добавление нескольких записей

    /// <summary>
    /// Начать групповое добавление и/или изменение строк таблицы.
    /// После вызова метода и до вызова DBxDataWriter.Dispose() нельзя вызывать другие методы для текущего соединения.
    /// При использовании DBxDataWriter по сети, рекомендуется использовать методы LoadFrom(), так как поштучные вызовы
    /// метода для каждой строки невыгодны.
    /// </summary>
    /// <param name="writerInfo">Параметры обработки</param>
    /// <returns>Объект для записи</returns>
    public DBxDataWriter CreateWriter(DBxDataWriterInfo writerInfo)
    {
      return _Source.CreateWriter(writerInfo);
    }

    /// <summary>
    /// Добавление множества записей из таблицы данных.
    /// Используется BULK COPY, если эта возможность реализована в базе данных.
    /// Иначе выполняется поштучное добавление строк с помощью INSERT COPY.
    /// Метод не возвращает идентификаторы добавленных записей.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица может содержать, а может и не содержать ключевое поле. Если ключевого поля нет, то значение присваивается автоматически.
    /// </summary>
    /// <param name="table">Таблица данных</param>
    public void AddRecords(DataTable table)
    {
      _Source.AddRecords(table);
    }

    /// <summary>
    /// Добавление множества записей из таблицы данных.
    /// Используется BULK COPY, если эта возможность реализована в базе данных.
    /// Иначе выполняется поштучное добавление строк с помощью INSERT COPY.
    /// Метод не возвращает идентификаторы добавленных записей.
    /// Таблица может содержать, а может и не содержать ключевое поле. Если ключевого поля нет, то значение присваивается автоматически.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="table">Таблица данных</param>
    public void AddRecords(string tableName, DataTable table)
    {
      _Source.AddRecords(tableName, table);
    }

    /// <summary>
    /// Добавление множества записей из таблицы данных.
    /// Используется BULK COPY, если эта возможность реализована в базе данных.
    /// Иначе выполняется поштучное добавление строк с помощью INSERT COPY.
    /// Метод не возвращает идентификаторы добавленных записей.
    /// Пары можгут содержать, а могут и не содержать ключевое поле. Если ключевого поля нет, то значение присваивается автоматически.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValuesArray">Массив хэш-таблиц, по одной таблице в массмвк для каждой записи.
    /// Каждая хэш таблица содержит пары "ИмяПоля"-"Значение" для одной записи</param>
    public void AddRecords(string tableName, IDictionary[] columnNamesAndValuesArray)
    {
      _Source.AddRecords(tableName, columnNamesAndValuesArray);
    }

    /// <summary>
    /// Добавление множества записей из нескольких таблиц данных.
    /// Используется BULK COPY, если эта возможность реализована в базе данных.
    /// Иначе выполняется поштучное добавление строк с помощью INSERT COPY.
    /// Метод не возвращает идентификаторы добавленных записей.
    /// Имена таблиц, в которые выполняется добавление строк, извлекаются из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица могут содержать, а могут и не содержать ключевые поля. Если ключевого поля нет, то значение присваивается автоматически.
    /// </summary>
    /// <param name="ds">Набор таблиц</param>
    public void AddRecords(DataSet ds)
    {
      _Source.AddRecords(ds);
    }

    /// <summary>
    /// Групповое добавление записей (Bulk Copy) из открытого объекта DbDataReader.
    /// Если для базы данных не предусмотрена такая возможность, выполняется поштучное добавление записей
    /// Таблица может содержать, а может и не содержать
    /// ключевое поле. Если ключевого поля нет, то значение присваивается автоматически
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которую добавляются записи</param>
    /// <param name="source">Открытая на чтение таблица данных, возможно, в другой базе данных</param>
    public void AddRecords(string tableName, DbDataReader source)
    {
      _Source.AddRecords(tableName, source);
    }

    #endregion

    #region Обновление / добавление записей

    /// <summary>
    /// Обновление множества записей из таблицы данных.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица <paramref name="table"/> должна содержать поле (или поля), соответствующее первичному ключу таблицы в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблице <paramref name="table"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="table">Таблица исходных данных</param>
    public void UpdateRecords(DataTable table)
    {
      _Source.UpdateRecords(table);
    }

    /// <summary>
    /// Групповое обновление записей из таблицы данных.
    /// Таблица <paramref name="table"/> должна содержать поле (или поля), соответствующее первичному ключу таблицы в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблице <paramref name="table"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="tableName">Имя таблицы базы данных, в которую добавляются записи</param>
    /// <param name="table">Таблица исходных данных</param>
    public void UpdateRecords(string tableName, DataTable table)
    {
      _Source.UpdateRecords(tableName, table);
    }

    /// <summary>
    /// Групповое обновление записей из нескольких таблиц данных.
    /// Имена таблиц, для которых выполняется обновление строк, извлекаются из свойства <see cref="System.Data.DataTable.TableName"/>.
    /// Таблицы должны содержать поле (или поля), соответствующие первичному ключу таблиц в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблицах <paramref name="ds"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="ds">Набор данных</param>
    public void UpdateRecords(DataSet ds)
    {
      _Source.UpdateRecords(ds);
    }


    /// <summary>
    /// Обновление множества записей из таблицы данных и добавление недостающих.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица должна содержать поле (или поля), соответствующие первичному ключу таблицы в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблице <paramref name="table"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="table">Таблица исходных данных</param>
    public void AddOrUpdateRecords(DataTable table)
    {
      _Source.AddOrUpdateRecords(table);
    }

    /// <summary>
    /// Обновление множества записей из таблицы данных и добавление недостающих.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица должна содержать поле (или поля), соответствующие первичному ключу таблицы в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблице <paramref name="table"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="tableName">Имя таблицы в базе данных, в которую добавляются записи</param>
    /// <param name="table">Таблица исходных данных</param>
    public void AddOrUpdateRecords(string tableName, DataTable table)
    {
      _Source.AddOrUpdateRecords(tableName, table);
    }

    /// <summary>
    /// Обновление множества записей из нескольких таблиц данных и добавление недостающих.
    /// Имена таблиц, для которых выполняется обновление строк, извлекаются из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблицы должны содержать поле (или поля), соответствующие первичному ключу таблиц в базе данных (свойство DBxTableStruct.PrimaryKey).
    /// Также должно быть, как минимум, одно поле, не входящее в первичный ключ.
    /// Поддерживаются таблицы с первичным ключом любого типа, включая составной. Для таблиц без первичного ключа обновление невозможно.
    /// Наличие первичного ключа в таблицах <paramref name="ds"/> (свойство DataTable.PrimaryKey) не имеет значения.
    /// </summary>
    /// <param name="ds">Набор таблиц</param>
    public void AddOrUpdateRecords(DataSet ds)
    {
      _Source.AddOrUpdateRecords(ds);
    }

    #endregion

    #region Комбинированный поиск и добавление записи

    /// <summary>
    /// Поиск строки по значениям полей, заданным в виде списка пар. Если запись
    /// с подходящими значениями не найдена, то добавляется новая запись
    /// На время выполнения запроса база данных блокируется во избежание двойного
    /// добавления.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Имена и значения полей</param>
    /// <param name="id">Возвращается идентификатор Id найденной или новой записи. Не может быть 0</param>
    /// <returns>true, если была добавлена новая запись, false-если найдена существующая</returns>
    public bool FindOrAddRecord(string tableName, IDictionary columnNamesAndValues, out Int32 id)
    {
      string[] ColumnNames;
      object[] Values;
      DataTools.PairsToNamesAndValues(columnNamesAndValues, out ColumnNames, out Values);
      return FindOrAddRecord(tableName, new DBxColumns(ColumnNames), Values, out id);
    }

    /// <summary>
    /// Поиск строки по значениям полей. Если запись
    /// с подходящими значениями не найдена, то добавляется новая запись
    /// На время выполнения запроса база данных блокируется во избежание двойного
    /// добавления.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей</param>
    /// <param name="values">Значения полей</param>
    /// <param name="id">Возвращается идентификатор Id найденной или новой записи. Не может быть 0</param>
    /// <returns>true, если была добавлена новая запись, false-если найдена существующая</returns>
    public bool FindOrAddRecord(string tableName, DBxColumns columnNames, object[] values, out int id)
    {
      return _Source.FindOrAddRecord(tableName, columnNames, values, out id);
    }

    /// <summary>
    /// Поиск строки по значениям полей, заданным в виде списка пар. Если запись
    /// с подходящими значениями не найдена, то добавляется новая запись
    /// На время выполнения запроса база данных блокируется во избежание двойного
    /// добавления.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNamesAndValues">Имена и значения полей</param>
    /// <returns>Возвращается идентификатор Id найденной или новой записи. Не может быть 0</returns>
    public Int32 FindOrAddRecord(string tableName, IDictionary columnNamesAndValues)
    {
      Int32 Id;
      FindOrAddRecord(tableName, columnNamesAndValues, out Id);
      return Id;
    }

    /// <summary>
    /// Поиск строки по значениям полей. Если запись
    /// с подходящими значениями не найдена, то добавляется новая запись
    /// На время выполнения запроса база данных блокируется во избежание двойного
    /// добавления.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Имена полей</param>
    /// <param name="values">Значения полей</param>
    /// <returns>Возвращается идентификатор Id найденной или новой записи. Не может быть 0</returns>
    public Int32 FindOrAddRecord(string tableName, DBxColumns columnNames, object[] values)
    {
      return _Source.FindOrAddRecord(tableName, columnNames, values);
    }

    /// <summary>
    /// Групповой поиск или добавление множества записей в таблицу базы данных.
    /// Искомые строки передаются в таблице DataTable. В таблице должно быть одно или несколько полей, на основании
    /// которых выполняется поиск. Эта таблица НЕ ДОЛЖНА иметь поля идентификатора.
    /// Имя таблицы, в которую выполняется добавление строк, извлекается из свойства <see cref="System.Data.DataTable.TableName"/>
    /// Таблица в базе данных должна иметь первичный ключ типа Int32.
    /// Предполагается, что в базе данных имеется индекс по полям, по которым выполняется поиск, иначе будет медленно.
    /// Должно быть разрешение на запись таблицы.
    /// Возвращает массив идентификаторов найденных или созданных строк. Длина массива и порядок элемента совпадает
    /// со строками исходной таблицы <paramref name="table"/>.
    /// </summary>
    /// <param name="table">Строки для поиска. Этот объект не меняется</param>
    /// <returns>Идентификаторы строк</returns>
    public Int32[] FindOrAddRecords(DataTable table)
    {
      return _Source.FindOrAddRecords(table);
    }

    /// <summary>
    /// Групповой поиск или добавление множества записей в таблицу базы данных.
    /// Искомые строки передаются в таблице DataTable. В таблице должно быть одно или несколько полей, на основании
    /// которых выполняется поиск. Эта таблица НЕ ДОЛЖНА иметь поля идентификатора.
    /// Таблица в базе данных должна иметь первичный ключ типа Int32.
    /// Предполагается, что в базе данных имеется индекс по полям, по которым выполняется поиск, иначе будет медленно.
    /// Должно быть разрешение на запись таблицы.
    /// Возвращает массив идентификаторов найденных или созданных строк. Длина массива и порядок элемента совпадает
    /// со строками исходной таблицы <paramref name="table"/>.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="table">Строки для поиска. Этот объект не меняется</param>
    /// <returns>Идентификаторы строк</returns>
    public Int32[] FindOrAddRecords(string tableName, DataTable table)
    {
      return _Source.FindOrAddRecords(tableName, table);
    }

    #endregion

    #endregion

    #region Удаление записи

    /// <summary>
    /// Удаление одной строки таблицы.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор строки таблицы (значение первичного ключа)</param>
    public void Delete(string tableName, Int32 id)
    {
      _Source.Delete(tableName, id);
    }

    /// <summary>
    /// Удаление нескольких строк таблицы
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по удаляемым строкам</param>
    public void Delete(string tableName, DBxFilter where)
    {
      _Source.Delete(tableName, where);
    }

    /// <summary>
    /// Удалить все строки таблицы. Сама таблица не удаляется
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    public void DeleteAll(string tableName)
    {
      _Source.DeleteAll(tableName);
    }


    #endregion

    #region BLOB-поля

    /// <summary>
    /// Запись значения BLOB-поля как байтового массива.
    /// Ссылочные поля (с точками) не поддерживаются.
    /// Для очистки содержимого поля используйте <paramref name="value"/>=null. 
    /// Значение null и пустой массив различаются.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор записи. Не может быть 0</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <param name="value">Заполняемое значение или null</param>
    public void WriteBlob(string tableName, Int32 id, string columnName, byte[] value)
    {
      _Source.WriteBlob(tableName, id, columnName, value);
    }

    /// <summary>
    /// Запись значения BLOB-поля как байтового массива.
    /// Ссылочные поля (с точками) не поддерживаются.
    /// Для очистки содержимого поля используйте <paramref name="value"/>=null. 
    /// Значение null и пустой массив различаются.
    /// Условие <paramref name="where"/> должно быть задано обязательно.
    /// Как правило, это фильтр по первичному ключу.
    /// Если фильтру соответствует несколько строк таблиц, значение поля устанавливается для всех из них.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам. Не может быть null</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <param name="value">Заполняемое значение или null</param>
    public void WriteBlob(string tableName, DBxFilter where, string columnName, byte[] value)
    {
      _Source.WriteBlob(tableName, where, columnName, value);
    }
    /// <summary>
    /// Получить значение BLOB-поля как байтового массива.
    /// Таблица должна иметь первичный ключ по целочисленному (Int32) полю.
    /// Если <paramref name="id"/>=0, возвращает null.
    /// Выбрасывает исключение DBxRecordNotFoundException, если задан идентификатор несуществующей записи.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="id">Идентификатор записи. Не может быть 0</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <returns>Значение поля или null</returns>
    public byte[] ReadBlob(string tableName, Int32 id, string columnName)
    {
      return _Source.ReadBlob(tableName, id, columnName);
    }

    /// <summary>
    /// Получить значение BLOB-поля как байтового массива.
    /// Условие <paramref name="where"/> должно быть задано обязательно.
    /// Как правило, это фильтр по первичному ключу.
    /// Если фильтру соответствует несколько строк таблиц, то возвращается значение поля из первой попавшейся строки,
    /// что обычно нежелательно.
    /// 
    /// Если нет ни одной строки, удовлетворяющей условию фильтра, то возвращается null.
    /// Null также возвращается, если в найденной строке поле имеет значение NULL.
    /// Используйте перегрузку, возвразающую значение Value по ссылке, чтобы различить эти ситуации.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам. Не может быть null</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <returns>Значение поля или null</returns>
    public byte[] ReadBlob(string tableName, DBxFilter where, string columnName)
    {
      return _Source.ReadBlob(tableName, where, columnName);
    }

    /// <summary>
    /// Получить значение BLOB-поля как байтового массива.
    /// Значение поля возвращается по ссылке <paramref name="value"/>. Если строка не найдена или
    /// поле содержит значение NULL, по ссылке передается null.
    /// Условие <paramref name="where"/> должно быть задано обязательно.
    /// Как правило, это фильтр по первичному ключу.
    /// Если фильтру соответствует несколько строк таблиц, то возвращается значение поля из первой попавшейся строки,
    /// что обычно нежелательно.
    /// Эта перегрузка метода позволяет определить наличие или отсутствие строки, удовлетворяющей фильтру.
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="where">Фильтр по строкам. Не может быть null</param>
    /// <param name="columnName">Имя BLOB-поля</param>
    /// <param name="value">Сюда записывается значение поля или null</param>
    /// <returns>True, если найдена строка, удовлетворяющая условию <paramref name="where"/>.
    /// False, если строка не найдена</returns>
    public bool ReadBlob(string tableName, DBxFilter where, string columnName, out byte[] value)
    {
      return _Source.ReadBlob(tableName, where, columnName, out value);
    }

    #endregion

    #endregion

    #region Транзакции

    /*
     * Одно соединение поддерживает только одну транзакцию одновременно
     * Порядок вызова функций:
     * TransactionBegin();
     * try
     * {
     *   ... // Выполнение нескольких SQL-запросов
     * 
     *   TransactionCommit();
     * }
     * catch
     * {
     *   TransactionRollback();
     * }
     * 
     * Если есть несколько соединений, выгодно использовать объект DBxTransactionArray
     */

    /// <summary>
    /// Обрабатывается ли сейчас транзакция ?
    /// </summary>
    public bool InsideTransaction { get { return _Source.InsideTransaction; } }

    /// <summary>
    /// Начать транзакцию
    /// </summary>
    public void TransactionBegin()
    {
      _Source.TransactionBegin();
    }

    /// <summary>
    /// Завершение транзакции с сохранением изменений в базе данных
    /// </summary>
    public void TransactionCommit()
    {
      _Source.TransactionCommit();
    }

    /// <summary>
    /// Откат транзакции
    /// </summary>
    public void TransactionRollback()
    {
      _Source.TransactionRollback();
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Возвращает пустую таблицу с заданными столбцами.
    /// Всегда создается новый объект DataTable.
    /// Можно использовать ссылочные столбцы, содержащие ".".
    /// Вызывает DBxStruct.CreateDataTable().
    /// </summary>
    /// <param name="tableName">Имя таблицы</param>
    /// <param name="columnNames">Список столбцов. Если null, то возвращаются все столбцы, определенные для таблицы</param>
    /// <returns>Пустая таблица</returns>
    public DataTable CreateEmptyTable(string tableName, DBxColumns columnNames)
    {
      return _Source.CreateEmptyTable(tableName, columnNames);
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создать копию соединения, которую можно использовать в отдельном потоке.
    /// Для полученной копии должен быть обязательно вызван метод Dispose()
    /// </summary>
    /// <returns></returns>
    public DBxCon Clone()
    {
      DBxConBase Con2 = Source.Entry.CreateCon();
      Source.CopyTo(Con2);
      return new DBxCon(Con2);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region Трассировка SQL-запросов

    /// <summary>
    /// Управление запроса на уровне соединения.
    /// Если свойство не установлено в явном виде для этого соединения, возвращается значение DBx.TraceEnabled.
    /// Это свойство соответствует одноименному свойству в DBxConBase
    /// </summary>
    public bool TraceEnabled { get { return _Source.TraceEnabled; } set { _Source.TraceEnabled = value; } }

    /// <summary>
    /// Восстановление значения по умолчанию для TraceEnabled
    /// Вызывает одноименный метод в DBxConBase
    /// </summary>
    public void ResetTraceEnabled()
    {
      _Source.ResetTraceEnabled();
    }

    #endregion
  }

  /// <summary>
  /// Массив объектов DBxCon
  /// Используется, когда требуется одновременный доступ к нескольким соединениям с базами данных, чтобы не
  /// использовать большое количество вложенных блоков using
  /// </summary>
  public class DBxConArray : SimpleDisposableObject, IEnumerable<DBxCon>
  {
    // 02.01.2021
    // Можно использовать SimpleDisposableObject в качестве базового класса

    #region Конструктор и Dispose()

    /// <summary>
    /// Создает массив соединений.
    /// </summary>
    /// <param name="entries">Список точек входа. В списке могут быть значения null. Для них соединения не будут созданы, но нумерация соединений сохраняется</param>
    public DBxConArray(params DBxEntry[] entries)
    {
      _Items = new DBxCon[entries.Length];
      try
      {
        for (int i = 0; i < entries.Length; i++)
        {
          if (entries[i] != null) // 06.10.2017
            _Items[i] = new DBxCon(entries[i]);
        }
      }
      catch
      {
        Clear();
        throw;
      }
    }

    /// <summary>
    /// Закрывает все соединения в массиве
    /// </summary>
    /// <param name="disposing">true, если вызван метод Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
        Clear();
      base.Dispose(disposing);
    }

    private void Clear()
    {
      for (int i = _Items.Length - 1; i >= 0; i--)
      {
        if (_Items[i] != null)
        {
          try
          {
            _Items[i].Dispose();
          }
          catch (Exception e)
          {
            LogoutTools.LogoutException(e, "Ошибка закрытия соединения");
          }
          _Items[i] = null;
        }
      }
    }

    #endregion

    #region Доступ к соединенеиям

    /// <summary>
    /// Доступ к созданным соединениям по индексу.
    /// Порядок элементов в массиве соответствует аргументам конструктора.
    /// Некоторые элементы могут иметь значение null, если соответствующий параметр конструктора имел значение null.
    /// </summary>
    /// <param name="index">Индекс соединения от 0 до Count-1</param>
    /// <returns>Соединение</returns>
    public DBxCon this[int index]
    {
      get { return _Items[index]; }
    }

    private DBxCon[] _Items;

    /// <summary>
    /// Возвращает количество параметров переданных конструктору.
    /// Количество реально открытых соединений может быть меньше, т.к. некоторые параметры конструктора могли иметь значения null.
    /// </summary>
    public int Count { get { return _Items.Length; } }

    #endregion

    #region IEnumerable<DBxCon> Members

    /// <summary>
    /// Возвращает перечислитель по соединениям.
    /// Следует учитывать, что при перечислении могут появляться значения null.
    /// 
    /// Тип возвращаемого значения (ArrayEnumerator) может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public ArrayEnumerator<DBxCon> GetEnumerator()
    {
      return new ArrayEnumerator<DBxCon>(_Items);
    }

    IEnumerator<DBxCon> IEnumerable<DBxCon>.GetEnumerator()
    {
      return new ArrayEnumerator<DBxCon>(_Items);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new ArrayEnumerator<DBxCon>(_Items);
    }

    #endregion
  }

  /// <summary>
  /// Небезопасное соединение, реализующее передачу по ссылке.
  /// Добавляет в DBxCon возможность выполнять любые SQL-запросы без проверок.
  /// Объект следует передавать только в AppDomain, созданные на сервере.
  /// </summary>
  public class DBxUnsafeCon : DBxCon, ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// Создать соединение
    /// </summary>
    /// <param name="entry">Точка входа</param>
    public DBxUnsafeCon(DBxEntry entry)
      : base(entry)
    {
    }

    /// <summary>
    /// Версия конструктора с настраиваемым DBxConBase (например, можно отключить проверку имен).
    /// DBxConBase должен быть создан и настроен до вызова конструктора. DBxConBase является "персональным"
    /// для этого соединения, т.к. DBxCon.Dispose() разрушит DBxConBase
    /// </summary>
    /// <param name="source">Внутренний объект соединения</param>
    public DBxUnsafeCon(DBxConBase source)
      : base(source)
    {
    }

    #endregion

    #region Небезопасные методы SQL-запросов

    /// <summary>
    /// Выполнение SQL-запроса, возвращающего единственное значение
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <returns>Возвращаемое значение</returns>
    public object SQLExecuteScalar(string cmdText)
    {
      return Source.SQLExecuteScalar(cmdText);
    }

    /// <summary>
    /// Выполнение SQL-запроса, возвращающего единственное значение
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <param name="paramValues">Значения параметров запроса</param>
    /// <returns>Возвращаемое значение</returns>
    public object SQLExecuteScalar(string cmdText, object[] paramValues)
    {
      return Source.SQLExecuteScalar(cmdText, paramValues);
    }

    /// <summary>
    /// Выполнение SQL-запроса, не возвращающего значения
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    public void SQLExecuteNonQuery(string cmdText)
    {
      Source.SQLExecuteNonQuery(cmdText);
    }

    /// <summary>
    /// Выполнение SQL-запроса, не возвращающего значения
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <param name="paramValues">Значения параметров запроса</param>
    public void SQLExecuteNonQuery(string cmdText, object[] paramValues)
    {
      Source.SQLExecuteNonQuery(cmdText, paramValues);
    }


    /// <summary>
    /// Выполнение запроса, возвращающего набор строк. Результат записывается в
    /// объект DataTable без имени.
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <returns>Набор данных</returns>
    public DataTable SQLExecuteDataTable(string cmdText)
    {
      return Source.SQLExecuteDataTable(cmdText);
    }

    /// <summary>
    /// Выполнение запроса, возвращающего набор строк. Результат записывается в
    /// объект DataTable с указанным именем.
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <param name="tableName">Имя создаваемой таблицы</param>
    /// <returns>Набор данных</returns>
    public DataTable SQLExecuteDataTable(string cmdText, string tableName)
    {
      return Source.SQLExecuteDataTable(cmdText, tableName);
    }

    /// <summary>
    /// Выполнение запроса, возвращающего набор строк. Результат записывается в
    /// объект DataTable с указанным именем.
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <param name="tableName">Имя создаваемой таблицы</param>
    /// <param name="paramValues">Значения параметров запроса</param>
    /// <returns>Набор данных</returns>
    public DataTable SQLExecuteDataTable(string cmdText, string tableName, object[] paramValues)
    {
      return Source.SQLExecuteDataTable(cmdText, tableName, paramValues);
    }

    /// <summary>
    /// Выполнение запроса, возвращающего набор строк. Результат не извлекается,
    /// а возвращается DataReader. По окончании работы должен быть вызван метод
    /// Close(). До этого не разрешается выполнять другие запросы.
    /// Использование DbDataReader через границу домена не рекомендуется, т.к. многократные 
    /// вызовы методов и свойств этого объекта будут неэффективными. 
    /// Следует использовать SQLExecuteDataTable(), который возвращает набор данных за один раз
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <returns>Объект DataReader</returns>
    public DbDataReader SQLExecuteReader(string cmdText)
    {
      return Source.SQLExecuteReader(cmdText);
    }

    /// <summary>
    /// Выполнение запроса, возвращающего набор строк. Результат не извлекается,
    /// а возвращается DataReader. По окончании работы должен быть вызван метод
    /// Close(). До этого не разрешается выполнять другие запросы.
    /// Использование DbDataReader через границу домена не рекомендуется, т.к. многократные 
    /// вызовы методов и свойств этого объекта будут неэффективными. 
    /// Следует использовать SQLExecuteDataTable(), который возвращает набор данных за один раз
    /// </summary>
    /// <param name="cmdText">SQL-оператор</param>
    /// <param name="paramValues">Значения параметров запроса</param>
    /// <returns>Объект DataReader</returns>
    public DbDataReader SQLExecuteReader(string cmdText, object[] paramValues)
    {
      return Source.SQLExecuteReader(cmdText, paramValues);
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создать копию соединения, которую можно использовать в отдельном потоке.
    /// Для полученной копии должен быть обязательно вызван метод Dispose()
    /// </summary>
    /// <returns></returns>
    public new DBxUnsafeCon Clone()
    {
      DBxConBase Con2 = Source.Entry.CreateCon();
      Source.CopyTo(Con2);
      return new DBxUnsafeCon(Con2);
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }

  /// <summary>
  /// Организация транзакций для нескольких соединений
  /// Объект System.Transactions.TransactionScope работает не на всех компьютерах
  /// В отличие от TransactionScope, сначала создаются соединения, а затем - объект DBxTransactionArray 
  /// Это не замена TransactionScope, т.к. двухтактное подтверждение не используется. Если первая транзакция
  /// завершилась успешно, а вторая - нет, то первая транзакция не откатывается.
  /// Объект предназначен исключительно для упрощения исходного кода программы, чтобы уменьшить количество
  /// блоков try/catch/finally.
  /// Несмотря на название класса, может использоваться для упрощения транзакции для одного соединения.
  /// Допускается создание объектов во вложенном коде для одного соединения. В конструкторе проверяется, что
  /// для соединения еще не была начата транзакция. Если же транзакция уже есть, то для этого соединения никаких
  /// действий не выполняется. Предполагается, что подтверждением или откатом транзации занимается тот объект,
  /// который начал транзакцию.
  /// 
  /// Вызывайте конструктор объекта в инструкции using. Последним оператором в блоке using должен быть вызов метода Commit().
  /// </summary>
  public class DBxTransactionArray : SimpleDisposableObject
  {
    // 02.01.2021
    // Можно использовать SimpleDisposableObject в качестве базового класса

    #region Конструктор и Dispose

    /// <summary>
    /// Получает произвольное количество соединений и вызывает для них DBxCon.TransactionBegin()
    /// </summary>
    /// <param name="cons">Соединения</param>
    public DBxTransactionArray(params IDBxConTransactions[] cons)
    {
      _Cons = new IDBxConTransactions[cons.Length];
      try
      {
        for (int i = 0; i < cons.Length; i++)
        {
          if (cons[i] != null)
          {
            if (!cons[i].InsideTransaction) // 23.09.2019
            {
              cons[i].TransactionBegin();
              _Cons[i] = cons[i];
            }
          }
        }
      }
      catch
      {
        Rollback();
        throw;
      }
    }

    /// <summary>
    /// Если не было вызова метода Commit() или не для всех содинений он успешно завершился,
    /// то вызывается метод DBxCon.TransactionRollback().
    /// Это действие выполняется только для <paramref name="disposing"/>=true.
    /// </summary>
    /// <param name="disposing">True, если вызван метод Dispose(), false, если вызван деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
        Rollback();
      base.Dispose(disposing);
    }

    #endregion

    #region Complete() и Rollback()

    /// <summary>
    /// Массив соединений с незакрытыми транзакциями.
    /// Если для соединения тразакция выполнена или уже завершена, элемент содержит null
    /// </summary>
    private IDBxConTransactions[] _Cons;

    /// <summary>
    /// Вызов TransactionCommit() для всех соединений
    /// Транзакции завершаются в порядке объявления соединений ([0], [1], ...)
    /// </summary>
    public void Commit()
    {
      for (int i = 0; i < _Cons.Length; i++)
      {
        if (_Cons[i] != null)
        {
          if (_Cons[i].InsideTransaction) // условие добавлено 03.06.2020
            _Cons[i].TransactionCommit();
          _Cons[i] = null;
        }
      }
    }

    private void Rollback()
    {
      for (int i = 0; i < _Cons.Length; i++)
      {
        if (_Cons[i] != null)
        {
          try
          {
            if (_Cons[i].InsideTransaction) // условие добавлено 03.06.2020
              _Cons[i].TransactionRollback();
          }
          catch (Exception e)
          {
            LogoutTools.LogoutException(e, "Ошибка отката транзакции");
          }
          _Cons[i] = null;
        }
      }
    }

    #endregion
  }
}
