using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;

namespace FreeLibSet.Core
{

  /*
   * Правила именования строковых ресурсов:
   * "ИмяКласса_Тип_Описание"
   * 
   * ИмяКласса - имя класса без указания пространства имен. Шаблонные параметры не указываются.
   * Исключения: "Common" - используется в ExceptionTools.
   * Команды меню в ExtForms.dll:
   * "Cmd_Тип_Категория_Команда" (исключение из общего правила)
   * "Cmd_Тип_Категория_Команда_Условие" - изменение стандартного текста в зависимости от условий
   * Кнопки блоков диалога
   * "Btn_Text_КодКнопки"
   * "Btn_ToolTip_КодКнопки"
   * 
   * Тип - одно из значений:
   * "Action"      - Наименование выполняемого действия. Обычно является частью сообщения "Phase" или "Msg"
   * "Arg"         - Текст исключения ArgumentException или производного класса
   * "Err"         - Текст исключения, не связанного с проверкой аргументов
   * "ErrTitle"    - Аргумент title при вызове LogoutTools.LogoutException()
   * "Msg"         - Текст сообщения для выдачи пользователю
   * "Title"       - Заголок MessageBox, кроме сообщений об ошибке
   * "Phase"       - Текст фазы для splash-заставки и EFPApp.BeginWait
   * "SwitchDescr" - System.Diagnostics.Switch.Description
   * "Menu"        - Название команды меню. Может содержать "&"
   * "Text"        - Текст кнопки блока диалога. Может содержать "&"
   * "ToolTip"     - Текст всплывающей подсказки 
   * "Status"      - Текст, отображаемый в панели статусной строки для EFPCommandItem
   * "Name"        - Значение свойства DisplayName  
   * "Prompt"      - Текст метки в блоке. Может содержать "&" 
   * "ColTitle"    - Заголовок столбца табличного просмотра
   * "Order"       - Название порядка сортировки 
   * "FileType"    - Тип файла для использования в FileDialog.Filter (без "|" и правой части)
   * 
   * Описание - дополнительный текст
   */

  /// <summary>
  /// Методы, возвращающие объекты <see cref="Exception"/> для выбрасывания исключений в различных ситуациях
  /// </summary>
  public static class ExceptionFactory
  {
    #region Генерация исключений

    #region Проверка аргументов

    /// <summary>
    /// Возвращает <see cref="ArgumentException"/> с сообщением "Значение не задано".
    /// Используется для проверки аргументов-структур.
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgIsEmpty(string paramName)
    {
      return new ArgumentException(Res.Common_Arg_IsEmpty, paramName);
    }

    /// <summary>
    /// Возврашает <see cref="ArgumentNullException"/> для проверки строкового аргумента
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgStringIsNullOrEmpty(string paramName)
    {
      return new ArgumentNullException(paramName, Res.Common_Arg_StringIsNullOrEmpty);
    }

    /// <summary>
    /// Возвращает <see cref="ArgumentException"/> с сообщением "Неизвестное значение".
    /// Обычно используется для аргумента перечислимого типа.
    /// Список допустимых значений не выводится.
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="value">Значение, переданное в метод</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgUnknownValue(string paramName, object value)
    {
      return ArgUnknownValue(paramName, value, null);
    }

    /// <summary>
    /// Возвращает <see cref="ArgumentException"/> с сообщением "Неизвестное значение".
    /// Обычно используется для аргумента перечислимого типа.
    /// Выводится список допустимых значений не выводится.
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="value">Значение, переданное в метод</param>
    /// <param name="wantedValues">Список допустимых значение. Если null, то список не выводится.
    /// Если допустимым значением является null, следует включить его в массив.</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgUnknownValue(string paramName, object value, Array wantedValues)
    {
      if (value == null)
        value = "null";
      if (wantedValues == null)
        return new ArgumentException(String.Format(Res.Common_Arg_UnknownValue, value), paramName);
      else
        return new ArgumentException(String.Format(Res.Common_Arg_UnknownValueWithWanted, value, GetWantedValueList(wantedValues)), paramName);
    }

    private static string GetWantedValueList(Array wantedValues)
    {
      StringBuilder sb = new StringBuilder();
      foreach (Object obj in wantedValues)
      {
        if (sb.Length > 0)
          sb.Append(", ");
        if (obj == null)
          sb.Append("null");
        else if (obj is String)
        {
          sb.Append('"');
          sb.Append(obj);
          sb.Append('"');
        }
        else
          sb.Append(obj);
      }
      if (sb.Length == 0)
        return "list is empty";
      else
        return sb.ToString();
    }



    /// <summary>
    /// Возвращает <see cref="ArgumentException"/>, когда аргумент имеет неизвестный тип.
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="value">Значение параметра</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgUnknownType(string paramName, object value)
    {
      string sType;
      if (value == null)
        sType = "null";
      else
        sType = value.GetType().ToString();
      return new ArgumentException(String.Format(Res.Common_Arg_UnknownType, sType), paramName);
    }


    /// <summary>
    /// Возвращает <see cref="ArgumentException"/>, когда аргумент нельзя преобразовать к заданному типу
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="value">Значение параметра</param>
    /// <param name="wantedType">Тип, к которому требуется преобразовать аргумент</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgNoType(string paramName, object value, Type wantedType)
    {
      if (wantedType == null)
        throw new ArgumentNullException("wantedType");

      string sType;
      if (value == null)
        sType = "null";
      else
        sType = value.GetType().ToString();

      return new ArgumentException(String.Format(Res.Common_Arg_NoType, sType, wantedType.ToString()), paramName);
    }


    /// <summary>
    /// Создает исключение <see cref="ArgumentOutOfRangeException"/>.
    /// Есть возможность задать минимальное и/или максимальное значение, или не задавать оба.
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="value">Переданное значение аргумента</param>
    /// <param name="min">Минимальное значение или null</param>
    /// <param name="max">Максимальное значение или null</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentOutOfRangeException ArgOutOfRange(string paramName, object value, object min, object max)
    {
      //if (value == null)
      //  value = "null";
      if (min == null)
      {
        if (max == null)
          return new ArgumentOutOfRangeException(paramName, value, Res.Common_Arg_OutOfRange);
        else
          return new ArgumentOutOfRangeException(paramName, value, String.Format(Res.Common_Arg_OutOfRangeMax, max));
      }
      else
      {
        if (max == null)
          return new ArgumentOutOfRangeException(paramName, value, String.Format(Res.Common_Arg_OutOfRangeMin, min));
        else if (IsInvertedRange(min, max))
          return new ArgumentOutOfRangeException(paramName, value, String.Format(Res.Common_Arg_OutOfRangeMinMaxInv, min, max));
        else
          return new ArgumentOutOfRangeException(paramName, value, String.Format(Res.Common_Arg_OutOfRangeMinMax, min, max));
      }
    }

    private static bool IsInvertedRange(object min, object max)
    {
      IComparable icomp = min as IComparable;
      if (icomp == null)
        return false;
      if (min.GetType() != max.GetType())
        return false;
      return icomp.CompareTo(max) > 0;
    }

    /// <summary>
    /// Создает исключение <see cref="ArgumentException"/>, используемые при проверке пары аргументов, задающих диапазон значений,
    /// для случая, когда первое значение больше второго
    /// </summary>
    /// <param name="firstParamName">Имя первого параметра метода</param>
    /// <param name="firstValue">Начальное значение диапазона</param>
    /// <param name="lastParamName">Имя второго параметра метода</param>
    /// <param name="lastValue">Конечное значение диапазона</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgRangeInverted(string firstParamName, object firstValue, string lastParamName, object lastValue)
    {
      if (firstValue == null)
        firstValue = "null";
      if (lastValue == null)
        lastValue = "null";
      return new ArgumentException(String.Format(Res.Common_Arg_RangeInverted, firstParamName, firstValue, lastParamName, lastValue), lastParamName);
    }

    /// <summary>
    /// Создает <see cref="ArgumentException"/> для строкового аргумента, содержащего недопустимый символ в заданной позиции
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="value">Переданное значение аргумента</param>
    /// <param name="badCharIndex">Позиция в пределах <paramref name="value"/>. Нумерация символов начинается с 0</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgBadChar(string paramName, string value, int badCharIndex)
    {
      if (String.IsNullOrEmpty(value))
        throw new ArgumentNullException("value");
      char ch = value[badCharIndex];
      return new ArgumentException(String.Format(Res.Common_Arg_BadChar, value, ch, badCharIndex + 1), paramName);
    }

    /// <summary>
    /// Создает <see cref="ArgumentException"/> для строкового аргумента, содержащего недопустимый символ в заданной позиции
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="value">Переданное значение аргумента</param>
    /// <param name="badChar">Плохой символ. Его позиция будет найдена.
    /// Используется строковый аргумент, а не <see cref="Char"/>, чтобы избежать путаницы с основной перегрузкой метода</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgBadChar(string paramName, string value, string badChar)
    {
      if (String.IsNullOrEmpty(value))
        throw new ArgumentNullException("value");
      if (String.IsNullOrEmpty(badChar))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("badChar");
      if (badChar.Length != 1)
        throw new ArgumentException("One-char string required", "badChar");

      int pos = value.IndexOf(badChar);
      return ArgBadChar(paramName, value, pos);
    }

    /// <summary>
    /// Создает исключение <see cref="ArgumentException"/> при проверке аргумента-массива или коллекции на соответствие ожидаемой длине
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="value">Переданное значение аргумента (массив или коллекция)</param>
    /// <param name="wantedCount">Ожидаемое количество элементов</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgWrongCollectionCount(string paramName, System.Collections.ICollection value, int wantedCount)
    {
      return new ArgumentException(String.Format(Res.Common_Arg_WrongCollectionCount, value.Count, wantedCount), paramName);
    }

    /// <summary>
    /// Возвращает исключение <see cref="ArgumentException"/>, когда у аргумента свойство не установлено или имеет неправильное значение.
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="paramValue">Значение параметра</param>
    /// <param name="propertyName">Имя свойства</param>
    /// <param name="propertyValue">Неправильное значение свойства</param>
    /// <param name="requires">Если не null, то будет выведен список допустимых значений.
    /// Если допустимым значением является null, то следует передать массив из одного элемента null.</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgProperty(string paramName, object paramValue, string propertyName, object propertyValue, Array requires)
    {
      return new ArgumentException(GetPropertyMessage(paramValue, propertyName, propertyValue, requires), paramName);
    }

    private static string GetPropertyMessage(object obj, string propertyName, object propertyValue, Array requires)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      if (String.IsNullOrEmpty(propertyName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("propertyName");

      if (propertyValue == null)
        propertyValue = "null";

      if (requires == null)
        return String.Format(Res.Common_Err_ObjectProperty, propertyName, obj.ToString(), propertyValue);
      else
      {
        string sReqs = GetWantedValueList(requires);
        return String.Format(Res.Common_Err_ObjectPropertyWithRequires, propertyName, obj.ToString(), propertyValue, sReqs);
      }
    }

    /// <summary>
    /// Возвращает исключение <see cref="ArgumentException"/> при проверке аргумента-коллекции (обычно для метода AddRange()),
    /// если он равен текущему объекту-коллекции.
    /// Обычно такая проверка должна выполняться только в конфигурации DEBUG. В конфигурации RELEASE ошибка возникнет при попытке
    /// перечисления исходной коллекции, так как она будет модифицирована.
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgCollectionSameAsThis(string paramName)
    {
      return new ArgumentException(Res.Common_Arg_CollectionSameAsThis, paramName);
    }

    /// <summary>
    /// Возвращает исключение <see cref="ArgumentException"/>, когда два аргумента (обычно, источник и приемник) совпадают
    /// </summary>
    /// <param name="paramName1">Имя первого параметра метода</param>
    /// <param name="paramName2">Имя второго параметра метода</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgAreSame(string paramName1, string paramName2)
    {
      return new ArgumentException(String.Format(Res.Common_Arg_AreSame, paramName1, paramName2), paramName2);
    }

    /// <summary>
    /// Создает <see cref="ArgumentException"/> для второго аргумента, когда два аргумента имеют несовместимые типы
    /// </summary>
    /// <param name="paramName1">Имя первого параметра метода</param>
    /// <param name="value1">Первое значение</param>
    /// <param name="paramName2">Имя второго параметра метода</param>
    /// <param name="value2">Второе значение</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgIncompatibleTypes(string paramName1, object value1, string paramName2, object value2)
    {
      string sType1 = value1 == null ? "none" : value1.GetType().ToString();
      string sType2 = value2 == null ? "none" : value2.GetType().ToString();

      return new ArgumentException(String.Format(Res.Common_Arg_IncompatibleTypes,
        paramName1, sType1, paramName2, sType2), paramName2);
    }

    /// <summary>
    /// Создает <see cref="ArgumentException"/> для второго аргумента, когда два строковых аргумента имеют разную длину
    /// </summary>
    /// <param name="paramName1">Имя первого параметра метода</param>
    /// <param name="value1">Первое значение</param>
    /// <param name="paramName2">Имя второго параметра метода</param>
    /// <param name="value2">Второе значение</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgStringsWithDifferentLength(string paramName1, string value1, string paramName2, string value2)
    {
      if (value1 == null)
        value1 = String.Empty;
      if (value2 == null)
        value2 = String.Empty;

      return new ArgumentException(String.Format(Res.Common_Arg_StringsWithDifferentLength,
        value1, value1.Length, value2, value2.Length), paramName2);
    }

    /// <summary>
    /// Создает <see cref="ArgumentException"/> для аргумента перечислимого типа (коллекции, массива, списка),
    /// в котором обнаружено значение неподходящего типа.
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="enumerable">Значение перечислимого аргумента</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgInvalidEnumerableType(string paramName, System.Collections.IEnumerable enumerable)
    {
      string typeName = "null";
      foreach (object obj in enumerable)
      {
        if (obj != null)
        {
          typeName = obj.GetType().ToString();
          break;
        }
      }

      return new ArgumentException(String.Format(Res.Common_Arg_InvalidEnumerableType, typeName), paramName);
    }

    /// <summary>
    /// Создает <see cref="ArgumentException"/>, когда список или массив содержит недопустимое значение
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="collection">значение аргумента - список или массив</param>
    /// <param name="itemIndex">индекс плохого элемента в <paramref name="collection"/></param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgInvalidListItem(string paramName, System.Collections.IList collection, int itemIndex)
    {
      object itemValue = collection[itemIndex];
      if (itemValue == null)
        itemValue = "null";

      return new ArgumentException(String.Format(Res.Common_Arg_InvalidListItem, itemValue, itemIndex), paramName);
    }

    /// <summary>
    /// Создает <see cref="ArgumentException"/>, когда коллекция содержит недопустимое значение.
    /// Используйте <see cref="ArgInvalidListItem(string, System.Collections.IList, int)"/>, если коллекция является
    /// списком с доступом по числовому индексу.
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="collection">значение аргумента - список или массив</param>
    /// <param name="itemValue">найденный плохой элемент в <paramref name="collection"/></param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgInvalidEnumerableItem(string paramName, System.Collections.IEnumerable collection, object itemValue)
    {
      int idx = 0;
      int foundIndex = -1;
      foreach (object item in collection)
      {
        if (Object.Equals(item, itemValue))
        {
          foundIndex = idx;
          break;
        }
        idx++;
      }

      object itemValue2 = itemValue;
      if (itemValue == null)
        itemValue2 = "null";

      return new ArgumentException(String.Format(Res.Common_Arg_InvalidListItem, itemValue2, foundIndex), paramName);
    }

    /// <summary>
    /// Создает <see cref="ArgumentException"/>, когда аргумент не принадлежит свойству-коллекции.
    /// Эта версия используется, когда метод принадлежит классу, содержащему свойство-коллекцию, но не классу-коллекции
    /// </summary>
    /// <param name="paramName">Имя параметра</param>
    /// <param name="value">Значение параметра, которое не удалось найти. Может быть null.</param>
    /// <param name="obj">Объект, содержащий свойство</param>
    /// <param name="propertyName">Имя свойства-коллекции</param>
    /// <param name="propertyValue">Значение свойства-коллекции</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgNotInCollection(string paramName, object value, object obj, string propertyName, System.Collections.ICollection propertyValue)
    {
      if (value == null)
        value = "null";
      if (obj == null)
        throw new ArgumentNullException("obj");
      if (String.IsNullOrEmpty(propertyName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("propertyName");
      if (propertyValue == null)
        throw new ArgumentNullException("propertyValue");

      return new ArgumentException(String.Format(Res.Common_Arg_NotInPropertyCollection, 
        value, propertyName, propertyValue.ToString(), obj.ToString()), paramName);
    }

    /// <summary>
    /// Создает <see cref="ArgumentException"/>, когда аргумент не объекту-коллекции.
    /// </summary>
    /// <param name="paramName">Имя параметра</param>
    /// <param name="value">Значение параметра, которое не удалось найти. Может быть null.</param>
    /// <param name="obj">Объект-коллекция</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgNotInCollection(string paramName, object value, System.Collections.ICollection obj)
    {
      if (value == null)
        value = "null";
      if (obj == null)
        throw new ArgumentNullException("obj");

      return new ArgumentException(String.Format(Res.Common_Arg_NotInCollection, value, obj.ToString()), paramName);
    }


    /// <summary>
    /// Возвращает <see cref="ArgumentException"/> для проверки аргумента на непустое значение кода.
    /// Аргумент реализует интерфейс <see cref="FreeLibSet.Collections.IObjectWithCode"/>.
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="value">Значение аргумента</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgObjectWithoutCode(string paramName, FreeLibSet.Collections.IObjectWithCode value)
    {
      string objName;
      if (value == null)
        objName = "null";
      else
        objName = value.GetType().ToString();
      return new ArgumentException(String.Format(Res.Common_Arg_ObjectWithoutCode, objName), paramName);
    }

    #endregion

    #region Преобразование типов

    /// <summary>
    /// Создает исключение <see cref="InvalidCastException"/> или <see cref="FormatException"/> при невозможности преобразовать значение в требуемый тип
    /// </summary>
    /// <param name="srcValue">Исходное значение, которое нужно преобразовать</param>
    /// <param name="destType">Тип, в который не удалось преобразовать</param>
    /// <returns>Объект исключения</returns>
    public static InvalidCastException Inconvertible(object srcValue, Type destType)
    {
      if (destType == null)
        throw new ArgumentNullException("destType");

      //if (srcValue is String)
      //  return new FormatException(UICore.UITools.ConvertErrorMessage((string)srcValue, destType));

      string sSrcType;
      if (srcValue == null)
      {
        srcValue = "null";
        sSrcType = "none";
      }
      else
        sSrcType = srcValue.GetType().ToString();

      return new InvalidCastException(String.Format(Res.Common_Err_Inconvertible, srcValue, sSrcType, destType.ToString()));
    }


    #endregion

    #region InvalidOperationException 

    /// <summary>
    /// Возвращает исключение, если структура не была инициализирована, то есть для нее не был вызван конструктор в явном виде
    /// </summary>
    /// <param name="type">Тип структуры</param>
    /// <returns>Объект исключения</returns>
    public static InvalidOperationException StructureNotInit(Type type)
    {
      if (type == null)
        throw new ArgumentNullException("type");
      return new InvalidOperationException(String.Format(Res.Common_Err_StructureNotInit, type.ToString()));
    }

    /// <summary>
    /// Возвращает исключение, когда у объекта свойство не установлено или имеет неправильное значение.
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="propertyName">Имя свойства</param>
    /// <param name="propertyValue">Неправильное значение свойства</param>
    /// <param name="requires">Если не null, то будет выведен список допустимых значений.
    /// Если допустимым значением является null, то следует передать массив из одного элемента null.</param>
    /// <returns>Объект исключения</returns>
    public static InvalidOperationException ObjectProperty(object obj, string propertyName, object propertyValue, Array requires)
    {
      return new InvalidOperationException(GetPropertyMessage(obj, propertyName, propertyValue, requires));
    }

    /// <summary>
    /// Возвращает исключение, когда у объекта не установлено свойство
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="propertyName">Имя свойства</param>
    /// <returns>Объект исключения</returns>
    public static InvalidOperationException ObjectPropertyNotSet(object obj, string propertyName)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      if (String.IsNullOrEmpty(propertyName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("propertyName");

      return new InvalidOperationException(String.Format(Res.Common_Err_ObjectPropertyNotSet, propertyName, obj.ToString()));
    }

    /// <summary>
    /// Возвращает исключение, когда у объекта уже установлено свойство
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="propertyName">Имя свойства</param>
    /// <returns>Объект исключения</returns>
    public static InvalidOperationException ObjectPropertyAlreadySet(object obj, string propertyName)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      if (String.IsNullOrEmpty(propertyName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("propertyName");

      return new InvalidOperationException(String.Format(Res.Common_Err_ObjectPropertyAlreadySet, propertyName, obj.ToString()));
    }

    /// <summary>
    /// Возвращает исключение <see cref="InvalidOperationException"/>, когда выполняется недопустимое переключение
    /// объекта из одного состояния в другое
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="propertyName">Имя свойства</param>
    /// <param name="oldValue">Текущее значение свойства</param>
    /// <param name="newValue">Новое (неправильное) значение свойства</param>
    /// <returns>Объект исключения</returns>
    public static InvalidOperationException ObjectPropertySwitch(object obj, string propertyName, object oldValue, object newValue)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      if (String.IsNullOrEmpty(propertyName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("propertyName");
      if (oldValue == null)
        oldValue = "null";
      if (newValue == null)
        newValue = "null";

      return new InvalidOperationException(String.Format(Res.Common_Err_ObjectPropertySwitch, propertyName, obj.ToString(), oldValue, newValue));
    }


    /// <summary>
    /// Возвращает исключение, когда у объекта не установлен обработчик события
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="eventName">Имя свойства</param>
    /// <returns>Объект исключения</returns>
    public static InvalidOperationException ObjectEventHandlerNotSet(object obj, string eventName)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      if (String.IsNullOrEmpty(eventName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("eventName");

      return new InvalidOperationException(String.Format(Res.Common_Err_ObjectEventHandlerNotSet, eventName, obj.ToString()));
    }

    /// <summary>
    /// Возвращает исключение, когда у объекта свойство-коллекция имеет неправильную длину
    /// </summary>
    /// <param name="obj">Объект</param>
    /// <param name="propertyName">Имя свойства</param>
    /// <param name="propertyValue">Неправильное значение свойства</param>
    /// <param name="wantedCount">Ожидавшаяся длина коллекции.</param>
    /// <returns>Объект исключения</returns>
    public static InvalidOperationException ObjectPropertyCount(object obj, string propertyName, System.Collections.ICollection propertyValue, int wantedCount)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      if (String.IsNullOrEmpty(propertyName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("propertyName");
      if (propertyValue == null)
        throw new ArgumentNullException("propertyValue");
      if (wantedCount < 0)
        throw ExceptionFactory.ArgOutOfRange("wantedCount", wantedCount, 0, null);

      return new InvalidOperationException(String.Format(Res.Common_Err_ObjectPropertyCount, propertyName, obj.ToString(), propertyValue.Count, wantedCount));
    }

    /// <summary>
    /// Создает исключение <see cref="InvalidOperationException"/>, если тип не реализует интерфейс <see cref="ICloneable"/>
    /// </summary>
    /// <param name="t">Тип объекта</param>
    /// <returns>Объект исключения</returns>
    public static InvalidOperationException TypeNotCloneable(Type t)
    {
      return new InvalidOperationException(String.Format(Res.Common_Err_TypeNotCloneable, t));
    }

    #endregion

    #region Словари

    /// <summary>
    /// Возвращает <see cref="KeyNotFoundException"/> для случая, когда словарь или коллекция не содержит элемента с заданным ключом
    /// </summary>
    /// <param name="key">Ключ, который не удалось найти</param>
    /// <returns>Объект исключения</returns>
    public static KeyNotFoundException KeyNotFound(object key)
    {
      return new KeyNotFoundException(String.Format(Res.Common_Err_KeyNotFound, GetKeyText(key)));
    }

    private static string GetKeyText(object key)
    {
      if (key == null)
        return "null"; // не должно быть
      if (key is Array)
        return DataTools.ToStringJoin(",", (Array)key);
      else
        return key.ToString();
    }

    /// <summary>
    /// Возвращает <see cref="InvalidOperationException"/> для случая, когда словарь или коллекция уже содержит элемент с заданным ключом
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Объект исключения</returns>
    public static InvalidOperationException KeyAlreadyExists(object key)
    {
      return new InvalidOperationException(String.Format(Res.Common_Err_KeyNotFound, GetKeyText(key)));
    }

    /// <summary>
    /// Возвращает <see cref="InvalidOperationException"/> при повторном добавлении элемента в коллекцию, когда причиной является состояние этого элемента, а не наличие ключа в коллекции
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    /// <returns>Объект исключения</returns>
    public static InvalidOperationException CannotAddItemAgain(object item)
    {
      if (item == null)
        item = "null";
      return new InvalidOperationException(String.Format(Res.Common_Err_CannotAddItemAgain, item.ToString()));
    }

    #endregion

    #region Проверка вызова метода

    /// <summary>
    /// Создает <see cref="InvalidOperationException"/> при вызове метода EndXXX() без предыдущего вызова BeginXXX()
    /// </summary>
    /// <param name="callee"></param>
    /// <param name="beginMethodName"></param>
    /// <param name="endErrorMethod"></param>
    /// <returns></returns>
    public static InvalidOperationException UnpairedCall(object callee, string beginMethodName, string endErrorMethod)
    {
      string sPrefix = String.Empty;
      if (callee != null)
      {
        if (callee is Type)
          sPrefix = callee.ToString() + ".";
        else
          sPrefix = callee.GetType().ToString() + ".";
      }
      return new InvalidOperationException(String.Format(Res.Common_Err_UnpairedCallOfEndMethod, sPrefix + beginMethodName, endErrorMethod));
    }

    /// <summary>
    /// Создает <see cref="InvalidOperationException"/> при повторном вызове одноразового метода
    /// </summary>
    /// <param name="callee"></param>
    /// <param name="methodName"></param>
    /// <returns></returns>
    public static InvalidOperationException RepeatedCall(object callee, string methodName)
    {
      string sPrefix = String.Empty;
      if (callee != null)
      {
        if (callee is Type)
          sPrefix = callee.ToString() + ".";
        else
          sPrefix = callee.GetType().ToString() + ".";
      }
      return new InvalidOperationException(String.Format(Res.Common_Err_RepeatedCallOfMethod, sPrefix + methodName));
    }


    #endregion

    #region Путь к файлу

    /// <summary>
    /// Возвращает исключение <see cref="System.IO.FileNotFoundException"/>
    /// </summary>
    /// <param name="path">Путь к файлу</param>
    /// <returns>Объект исключения</returns>
    public static System.IO.FileNotFoundException FileNotFound(FreeLibSet.IO.AbsPath path)
    {
      return new System.IO.FileNotFoundException(String.Format(Res.Common_Err_FileNotFound, path.Path), path.Path);
    }

    /// <summary>
    /// Возвращает исключение <see cref="System.IO.DirectoryNotFoundException"/>
    /// </summary>
    /// <param name="path">Путь к каталогу</param>
    /// <returns>Объект исключения</returns>
    public static System.IO.DirectoryNotFoundException DirectoryNotFound(FreeLibSet.IO.AbsPath path)
    {
      return new System.IO.DirectoryNotFoundException(String.Format(Res.Common_Err_DirNotFound, path.Path));
    }

    #endregion

    #region DataTable

    /// <summary>
    /// Возвращает <see cref="ArgumentException"/> для аргумента, задающего имя столбца, когда таблица не содержит столбца с таким именем
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="table">Таблица</param>
    /// <param name="columnName">Имя ненайденного столбца</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgUnknownColumnName(string paramName, System.Data.DataTable table, string columnName)
    {
      return new ArgumentException(String.Format(Res.Common_Arg_DataTableHasNoColumn, table.TableName, columnName), paramName);
    }

    /// <summary>
    /// Возвращает <see cref="ArgumentException"/> для аргумента, задающего имя столбца, когда таблица содержит столбец неправильного типа
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="column">Столбец</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgInvalidColumnType(string paramName, System.Data.DataColumn column)
    {
      string tableName;
      if (column.Table != null)
        tableName = column.Table.TableName;
      else
        tableName = "null";
      return new ArgumentException(String.Format(Res.Common_Arg_DataColumnInvalidType, tableName, column.ColumnName, DataTools.GetString(column.DataType)), paramName);
    }

    /// <summary>
    /// Создает исключение <see cref="ArgumentException"/>, когда таблица не содержит первичного ключа 
    /// (не задано свойство <see cref="System.Data.DataTable.PrimaryKey"/>.
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="table">Таблица без ключа</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgDataTableWithoutPrimaryKey(string paramName, System.Data.DataTable table)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      return new ArgumentException(String.Format(Res.Common_Arg_DataTableWithoutPrimaryKey, table.TableName), paramName);
    }

    /// <summary>
    /// Создает исключение <see cref="ArgumentException"/>, когда таблица не содержит первичного ключа или содержит составной первичный 
    /// (не задано свойство <see cref="System.Data.DataTable.PrimaryKey"/>.
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="table">Таблица без ключа</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgDataTableMustHaveSingleColumnPrimaryKey(string paramName, System.Data.DataTable table)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      int nPK = 0;
      if (table.PrimaryKey != null)
        nPK = table.PrimaryKey.Length;
      if (nPK == 0)
        return new ArgumentException(String.Format(Res.Common_Arg_DataTableWithoutPrimaryKey, table.TableName), paramName);
      else
        return new ArgumentException(String.Format(Res.Common_Arg_DataTableMustHaveSingleColumnPrimaryKey,
          table.TableName, nPK, DataTools.GetPrimaryKey(table)), paramName);
    }

    /// <summary>
    /// Возврашает <see cref="ArgumentException"/>, когда таблица имеет первичный ключ по подю неподходящего типа
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="table">Таблица с первичным ключом</param>
    /// <param name="wantedType">Ожидавшийся тип поля первичного ключа</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgDataTablePrimaryKeyWrongType(string paramName, System.Data.DataTable table, Type wantedType)
    {
      if (table == null)
        throw new ArgumentNullException("table");
      if (wantedType == null)
        throw new ArgumentNullException("wantedType");
      string sPK = DataTools.GetPrimaryKey(table);
      string sPKType;
      if (sPK.Length == 0)
        sPKType = "none";
      else
      {
        Type[] aPK = new Type[table.PrimaryKey.Length];
        for (int i = 0; i < aPK.Length; i++)
          aPK[i] = table.PrimaryKey[i].DataType;
        sPKType = DataTools.ToStringJoin<Type>(",", aPK);
      }

      return new ArgumentException(String.Format(Res.Common_Arg_DataTablePrimaryKeyWrongType,
        table.TableName, sPK, sPKType, wantedType.ToString()), paramName);
    }

    /// <summary>
    /// Создает <see cref="ArgumentException"/>, когда две таблицы относятся к разным <see cref="System.Data.DataSet"/>
    /// </summary>
    /// <param name="paramName1">Имя параметра первой таблицы</param>
    /// <param name="table1">Первая таблица</param>
    /// <param name="paramName2">Имя параметра второй таблицы</param>
    /// <param name="table2">Вторая таблица</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgDataTablesNotSameDataSet(string paramName1, System.Data.DataTable table1, string paramName2, System.Data.DataTable table2)
    {
      // Имена таблиц не используются. И paramName1 тоже

      return new ArgumentException(Res.Common_Arg_DataTablesNotSameDataSet, paramName2);
    }

    /// <summary>
    /// Создает <see cref="ArgumentException"/>, когда строка относится к другой таблице данных
    /// </summary>
    /// <param name="paramName">Имя параметра метода</param>
    /// <param name="row">Строка - значение аргумента</param>
    /// <param name="wantedTable">Таблица, к которой должна принадлежать строка</param>
    /// <returns>Объект исключения</returns>
    public static ArgumentException ArgDataRowNotInSameTable(string paramName, System.Data.DataRow row, System.Data.DataTable wantedTable)
    {
      // Аргументы пока не используются

      return new ArgumentException(Res.Common_Arg_DataRowNotInSameTable, paramName);
    }


    /// <summary>
    /// Создает <see cref="InvalidOperationException"/>, когда в таблице не найдена строка с заданным значением ключа.
    /// Поддерживаются составные ключи.
    /// </summary>
    /// <param name="table">Таблица для поиска</param>
    /// <param name="keyValues">Значения ключевых полей</param>
    /// <returns>Объект исключения</returns>
    public static InvalidOperationException DataRowNotFound(System.Data.DataTable table, object[] keyValues)
    {
      //int nPK = 0;
      //if (table.PrimaryKey != null)
      //  nPK = table.PrimaryKey.Length;
      string sKeyValues = DataTools.ToStringJoin(", ", keyValues);
      return new InvalidOperationException(String.Format(Res.Common_Err_DataRowNotFound,
        table.TableName, DataTools.GetPrimaryKey(table), sKeyValues));
    }

    #endregion

    #region Прочие исключения

    /// <summary>
    /// Создает <see cref="ObjectReadOnlyException"/> для объекта, реализующего интерфейс <see cref="IReadOnlyObject"/>.
    /// В <see cref="Exception.Message"/> присутствует текстовое представление объекта и его тип.
    /// Используется в реализациях метода интерфейса <see cref="IReadOnlyObject.CheckNotReadOnly()"/>.
    /// </summary>
    /// <param name="obj">Объект в режиме "Только чтение"</param>
    /// <returns>Объект исключения</returns>
    public static ObjectReadOnlyException ObjectReadOnly(IReadOnlyObject obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");

      string s1 = obj.ToString();
      string s2 = obj.GetType().ToString();
      if (String.Equals(s1, s2, StringComparison.Ordinal))
        return new ObjectReadOnlyException(String.Format(Res.Common_Err_ObjectReadOnlyWithType, s2));
      else
        return new ObjectReadOnlyException(String.Format(Res.Common_Err_ObjectReadOnlyWithStringAndType, s1, s2));
    }

    /// <summary>
    /// Создает <see cref="NotImplementedException"/>, когда свойство или метод должны быть переопределены в производном классе.
    /// Обычно в этом случае используются абстрактные а не виртуальные методы и свойства.
    /// </summary>
    /// <param name="obj">Объект (ссылка this)</param>
    /// <param name="name">Имя метода или свойства</param>
    /// <returns>Объект исключения</returns>
    public static NotImplementedException MustBeReimplemented(object obj, string name)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      if (String.IsNullOrEmpty(name))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("name");

      return new NotImplementedException(String.Format(Res.Common_Err_MustBeReimplemented, name, obj.GetType().ToString()));
    }

    /// <summary>
    /// Создает исключение DllNotFoundException
    /// </summary>
    /// <param name="dllName">Имя dll-файла</param>
    /// <returns>Объект исключения</returns>
    public static DllNotFoundException DllNotFound(string dllName)
    {
      if (String.IsNullOrEmpty(dllName))
        throw ArgStringIsNullOrEmpty("dllName");
      return new DllNotFoundException(String.Format(Res.Common_Err_DllNotFound, dllName));
    }

    #endregion

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Объединение текста сообщения для исключения с внутренним исключением, если оно задано.
    /// Используется в конструкторе класса исключения.
    /// </summary>
    /// <param name="message">Аргумент для конструктора объекта базового класса</param>
    /// <param name="innerException">Переданный аргумент или null</param>
    /// <returns>Составной текст сообщения</returns>
    public static string MergeInnerException(string message, Exception innerException)
    {
      if (innerException == null)
        return message;
      else
        return message + ". " + innerException.Message;
    }

    #endregion
  }
}
