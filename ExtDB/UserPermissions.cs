using System;
using System.Collections.Generic;
using System.Text;

using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Collections;

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

/*
 * Списки ограничений UserPermissions
 * ----------------------------------
 * 1. Список UserPermissions создается на стороне сервера. 
 *    Для каждого работающего пользователя или роли пользователей создается отдельный список
 *    Если в программе не предусмотрено явное назначение прав пользователей, то список создается чисто
 *    программным способом. 
 *    Если в программе предусмотрено назначение прав отдельным пользователям или группам пользователем,
 *    то создание списка является комбинированным. Часть разрешений добавляется из записей в базе данных,
 *    из соответствующих записей справочников пользователей (и / или групп пользователей, в зависимости
 *    от программы). Другие разрешения могут добавляться программно, как в начало списка, так и в конец
 * 2. Порядок разрешений в UserPermissions имеет значение. Последние записи могут изменять или отменять
 *    предыдущие
 * 3. Некоторые из классов UserPermission могут формировать ограничения для работы пользователя в базе данных,
 *    формируя объект DBxPermissions. Другие ограничения могут не относится к базе данных непосредственно
 * 4. Классы UserPermission являются классами однократной записи
 * 5. Классы UserPermission не являются самостоятельно сериализуемыми, т.к. могут содержать пользовательский
 *    код. 
 * 6. Для передачи списка разрешений от сервера к клиенту используется XML-формат 
 *    <Config>
 *      <Permission1>
 *        <Class>DB</Class>
 *        <Mode>ReadOnly</Mode>
 *      </Permission1>
 *      <Permission2>
 *        <Class>Table</Class>
 *        <Tables>Таблица1,Таблица2</Tables>
 *        <Mode>Full</Mode>
 *      </Permission2>
 *      ...
 *    </Config>
 * 
 *    Если требуется хранить разрешения в справочнике "Пользователи", то можно либо хранить данные в одном
 *    поле, как выше, либо каждое разрешение в отдельных строках  
 *    1     DB        <Config><Mode>ReadOnly</Mode></Config>
 *    2     Table     <Config><Tables>Таблица1,Таблица2</Tables><Mode>Full</Mode></Config>
 * 
 * 7. Для записи списка в XML-формат, каждый UserPermission реализует виртуальный метод Write(CfgPart).
 *    Метод должен записать необходимые данные для последующего создания разрешения
 *    Также есть свойство "ClassCode"
 *    Дополнительные объекты для создания XML не требуются
 * 8. Для загрузки разрешений на стороне клиента создается коллекция "ClassCode: Обработчик". Обработчик 
 *    (delegate) получает CfgPart и должен вернуть объект UserPermission. 
 *    Для удобства, в большинстве классов, производных от UserPermission, определен статический метод Create(CfgPart)
 *    Если в коллекции не определен нужный ClassCode, создается объект-заглушка UnknownUserPermission
 *    Нельзя использовать сериализацию, т.к. обработчик может быть привязан к объектам клиента
 * 9. Если в программе есть возможность явно настраивать права пользователей, то для клиента обычно реализуются 
 *    классы, производные не UserPermissionClassUI. Они содержат методы для пользовательского интерфейса:
 *    - Значок разрешения
 *    - Признак возможности создания разрешения для пользователя
 *    - Диалоги для добавления разрешения 
 *    Коллекция UserPermissionClassesUI также реализует список обработчиков для создания UserPermission
 * -------------------------------------------
 * - Классы UserPermission создаются независимо на стороне клиента (при необходимости) и сервера
 * - Для создания объектов UserPermission используются описатели. На стороне сервера используются
 *   UserPermissionClass и UserPermissionClasses. На стороне клиента - UserPermissionClassUI и UserPermissionClassesUI,
 *   производные от серверных классов. Классы клиента реализуют дополнительно значки состояний и редакторы
 *   разрешений
 * - Экземпляр UserPermissionClasses принадлежит объекту DBxServerDocProviderSource
 * - Экземпляр UserPermissionClassesUI принадлежит DBUI 
 */

namespace FreeLibSet.Data
{
  /// <summary>
  /// Базовый класс для пользовательского разрешения
  /// В приложении могут быть созданы собственные классы-наследники, например, задающие ограничения сразу для
  /// нескольких таблиц
  /// Классы должны быть "однократной записи" (кроме вспомогательных полей)
  /// </summary>
  public abstract class UserPermission : IReadOnlyObject
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="classCode">Код класса разрешения</param>
    protected UserPermission(string classCode)
    {
      if (String.IsNullOrEmpty(classCode))
        throw new ArgumentNullException("classCode");
      _ClassCode = classCode;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Код класса разрешения, например, "DB", "Table", ...
    /// </summary>
    public string ClassCode { get { return _ClassCode; } }
    private string _ClassCode;

    /// <summary>
    /// Коллекция-владелец разрешения.
    /// Если разрешение не присоединено к коллекции, свойство возвращает null
    /// </summary>
    public UserPermissions Owner
    {
      get { return _Owner; }
      internal set { _Owner = value; }
    }
    private UserPermissions _Owner;

    /// <summary>
    /// Свойство должно возвращать описание объекта, например, "Таблица \"Люди\""
    /// </summary>
    public virtual string ObjectText { get { return ClassCode; } }

    /// <summary>
    /// Свойство должно возвращать значение разрешения в текстовом виде, например, "Полный доступ"
    /// </summary>
    public abstract string ValueText { get;}

    /// <summary>
    /// Информация об источнике разрешения (строка)
    /// Формат строки определяется приложением (может не использоваться)
    /// Используется при передаче разрешений в XML-формате
    /// </summary>
    public string SourceInfo
    {
      get { return _SourceInfo; }
      set { _SourceInfo = value; }
    }
    private string _SourceInfo;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Метод должен выполнить запись данных в XML-формате
    /// </summary>
    /// <param name="cfg">Секция конфигурации для записи разрешения</param>
    public abstract void Write(CfgPart cfg);

    /// <summary>
    /// Метод должен выполнить чтение данных в XML-формате
    /// </summary>
    /// 
    /// <param name="cfg">Секция конфигурации для чтения разрешения</param>
    public abstract void Read(CfgPart cfg);

    #endregion

    #region Прочее

    /// <summary>
    /// Переопределенный метод, если ограничение относится к базе данных, должен применить себя к создаваемому
    /// объекту ограничений
    /// </summary>
    /// <param name="dbPermissions">Права доступа к таблицам и полям базы данных, которые можно устанавливать</param>
    public virtual void ApplyDbPermissions(DBxPermissions dbPermissions)
    {
    }

    /// <summary>
    /// Возвращает "ObjectText = ValueText"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return ObjectText + " = " + ValueText;
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Свойство возвращает true, если разрешение присоединено к коллекции (Owner!=null) и коллекция
    /// переведена в состояние "только для чтения"
    /// </summary>
    public bool IsReadOnly
    {
      get
      {
        if (_Owner == null)
          return false;
        else
          return _Owner.IsReadOnly;
      }
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion
  }

  /// <summary>
  /// Список разрешений пользователя
  /// </summary>
  public sealed class UserPermissions : ListWithReadOnly<UserPermission>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список разрешений.
    /// Список генераторов разрешений <paramref name="creators"/> не может быть пустым.
    /// </summary>
    /// <param name="creators">Список генераторов разрешений</param>
    public UserPermissions(UserPermissionCreators creators)
    {
      if (creators == null)
        throw new ArgumentNullException("creators");
      creators.SetReadOnly();
      _Creators = creators;
    }

    #endregion

    #region Чтение и запись XML

    /// <summary>
    /// Представление в виде XML, совместимого с конфигурационными данными (корневой узел "Config")
    /// Для установки используется метод Read()
    /// </summary>
    public string AsXmlText
    {
      get
      {
        TempCfg Cfg = new TempCfg();
        this.Write(Cfg);
        return Cfg.AsXmlText;
      }
    }

    /// <summary>
    /// Запись пользовательских разрешений в секцию конфигурации.
    /// Используется при передаче разрешений от сервера к клиенту
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void Write(CfgPart cfg)
    {
      for (int i = 0; i < Count; i++)
      {
        CfgPart cfgOne = cfg.GetChild("Permission" + (i + 1).ToString(), true);
        cfgOne.SetString("Class", this[i].ClassCode);
        cfgOne.SetString("SourceInfo", this[i].SourceInfo, true);
        this[i].Write(cfgOne);
      }
    }

    /// <summary>
    /// Чтение пользовательских разрешений из секции конфигурации.
    /// Используется при передаче разрешений от сервера к клиенту
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void Read(CfgPart cfg)
    {
      CheckNotReadOnly();
      Clear();
      for (int i = 1; i < int.MaxValue; i++)
      {
        CfgPart cfgOne = cfg.GetChild("Permission" + i.ToString(), false);
        if (cfgOne == null)
          break;

        string ClassCode = cfgOne.GetString("Class");
        UserPermission p = Add(ClassCode);
        p.Read(cfgOne);
        p.SourceInfo = cfgOne.GetString("SourceInfo");
      }
    }

    #endregion

    #region Создание разрешений

    /// <summary>
    /// Список генераторов разрешений, используемых методом Add().
    /// Задается в конструкторе и имеет IsReadOnly=true.
    /// </summary>
    public UserPermissionCreators Creators { get { return _Creators; } }
    private UserPermissionCreators _Creators;

    /// <summary>
    /// Добавляет разрешение в список, используя генератор разрешения.
    /// Перебираются все генераторы разрешений в списке Creators и для каждого из них
    /// </summary>
    /// <param name="classCode">Код класса разрешения</param>
    /// <returns>Созданное разрешение</returns>
    public UserPermission Add(string classCode)
    {
      UserPermission Permission = Creators.Create(classCode);
#if DEBUG
      if (Permission == null)
        throw new NullReferenceException("Не создано разрешение для ClassCode=\"" + classCode + "\"");
#endif
      Permission.Owner = this;
      base.Add(Permission);
      return Permission;
    }

    /// <summary>
    /// Добавляет разрешение в список.
    /// Кроме вызова метода базового класса, устанавливает свойство UserPermission.Owner.
    /// </summary>
    /// <param name="permission">Добавляемое разрешение</param>
    public new void Add(UserPermission permission)
    {
      if (permission == null)
        throw new ArgumentNullException("permission");
      if (permission.Owner != null)
        throw new InvalidOperationException("Повторное добавление разрешения в список не допускается");
      permission.Owner = this;
      base.Add(permission);
    }

    #endregion

    #region Прочие методы и свойства

    /// <summary>
    /// Переводит список разрешений в режим "Только чтение"
    /// </summary>
    public new void SetReadOnly()
    {
      base.SetReadOnly();
      // Выполнять действия с отдельными разрешениями не надо, т.к. они переходят в IsReadOnly автоматически
    }

    /// <summary>
    /// Установка разрешений на базу данных, таблицы и поля.
    /// Для каждого разрешения в списке вызывается виртуальный метод UserPermission.ApplyDbPermissions().
    /// Для текущего списка UserPermissions устанавливается IsReadOnly=true (но не для <paramref name="dbPermissions"/>).
    /// </summary>
    /// <param name="dbPermissions">Заполняемый объект разрешений. Должен иметь свойство IsReadOnly=false</param>
    public void ApplyDbPermissions(DBxPermissions dbPermissions)
    {
      SetReadOnly();
      dbPermissions.CheckNotReadOnly();

      IUserPermissionCreatorWithDbInitialization[] DbInitializators = Creators.GetDbInitializators();
      object[] userData = new object[DbInitializators.Length];
      for (int i = 0; i < DbInitializators.Length; i++)
        userData[i] = DbInitializators[i].BeforeApplyDbPermissions(this, dbPermissions);
      for (int i = 0; i < Count; i++)
        this[i].ApplyDbPermissions(dbPermissions);
      for (int i = 0; i < DbInitializators.Length; i++)
        DbInitializators[i].AfterApplyDbPermissions(this, dbPermissions, userData[i]);
    }

    /// <summary>
    /// Получить последнее в списке разрешение заданного класса.
    /// Если в списке нет ни одного разрешения такого класса (или производного от него), возвращается null.
    /// </summary>
    /// <typeparam name="T">Тип разрешения, производного от UserPermission</typeparam>
    /// <returns>Найденное разрешение или null</returns>
    public T GetLast<T>()
      where T : UserPermission
    {
      for (int i = Count - 1; i >= 0; i--)
      {
        T Res = this[i] as T;
        if (Res != null)
          return Res;
      }
      return null;
    }

    #endregion

    #region Статический экземпляр Empty

    /// <summary>
    /// Пустой список пользовательских разрешений
    /// </summary>
    public static readonly UserPermissions Empty = CreateEmpty();

    private static UserPermissions CreateEmpty()
    {
      UserPermissions Res = new UserPermissions(UserPermissionCreators.Empty);
      Res.SetReadOnly();
      return Res;
    }

    #endregion
  }

  /// <summary>
  /// Интерфейс генератора объекта UserPermission
  /// </summary>
  public interface IUserPermissionCreator : IObjectWithCode
  {
    /// <summary>
    /// Создать новый объект UserPermission со значениями свойств по умолчанию
    /// </summary>
    /// <returns>Созданное разрешение</returns>
    UserPermission CreateUserPermission();
  }

  /// <summary>
  /// Расширение интерфейса IUserPermissionCreator методами пре- и пост-инициализациит разрешений для базы данных.
  /// Эти методы будут вызываться в до и после вызова методов ApplyDbPermissions() для всех разрешений.
  /// Реализуется в ExtDBDocs классом DocTypePermission.Creator
  /// </summary>
  public interface IUserPermissionCreatorWithDbInitialization
  {
    /// <summary>
    /// Вызывается перед циклом вызова методов UserPermission.ApplyDbPermission()
    /// </summary>
    /// <param name="userPermissions">Список пользовательских разрешений. Его нельзя менять</param>
    /// <param name="dbPermissions">Разрешения для базы данных</param>
    /// <returns>Произвольные пользовательские данные</returns>
    object BeforeApplyDbPermissions(UserPermissions userPermissions, DBxPermissions dbPermissions);

    /// <summary>
    /// Вызывается после цикла вызова методов UserPermission.ApplyDbPermission()
    /// </summary>
    /// <param name="userPermissions">Список пользовательских разрешений. Его нельзя менять</param>
    /// <param name="dbPermissions">Разрешения для базы данных</param>
    /// <param name="userData">Пользовательские данные, полученные от BeforeApplyDbPermissions</param>
    void AfterApplyDbPermissions(UserPermissions userPermissions, DBxPermissions dbPermissions, object userData);
  }

  /// <summary>
  /// Коллекция генераторов разрешений UserPermission
  /// </summary>
  public class UserPermissionCreators : NamedCollection<IUserPermissionCreator>
  {
    #region Методы

    /// <summary>
    /// Добавляет генератор разрешения в список.
    /// Свойство IsReadOnly списка должно иметь значение false.
    /// Если в списке уже есть генератор с заданным кодом (<paramref name="сreator"/>.Code),
    /// то старый генератор предварительно удаляется из списка и заменяется новым генератором.
    /// </summary>
    /// <param name="сreator">Добавляемый генератор. Не может быть null.</param>
    public new void Add(IUserPermissionCreator сreator)
    {
      if (сreator == null)
        throw new ArgumentNullException("сreator");
      base.Remove(сreator.Code);
      base.Add(сreator);
    }

    /// <summary>
    /// Переводит список генераторов в режим "только чтение".
    /// Повторные вызовы игнорируются.
    /// Нет необходимости вызывать этот метод из пользовательского кода, так как он автоматически
    /// вызывается при создании списка разрешений UserPermissions.
    /// </summary>
    public new void SetReadOnly()
    {
      base.SetReadOnly();
    }

    /// <summary>
    /// Создает разрешение заданного класса (свойство IUserPermissionCreator.Code).
    /// Если в списке нет генератора разрешения заданного класса, возвращается разрешение UnknownUserPermission.
    /// Таким образом, возвращаемое значение не может быть null.
    /// Используется методом UserPermissions.Add().
    /// </summary>
    /// <param name="classCode">Код класса разрешения. Не может быть пустой строкой</param>
    /// <returns>Созданное разрешение</returns>
    public UserPermission Create(string classCode)
    {
      UserPermission Permission;
      IUserPermissionCreator Creator = this[classCode];
      if (Creator == null)
      {
        if (String.IsNullOrEmpty(classCode))
          throw new ArgumentNullException("classCode");
        Permission = new UnknownUserPermission(classCode);
      }
      else
      {
        Permission = Creator.CreateUserPermission();
        if (Permission == null)
          throw new BugException("Метод " + Creator.GetType().ToString() + ".CreateUserPermission() вернул null");
      }

      if (Permission.ClassCode != classCode)
        throw new BugException("Полученное разрешение " + Permission.GetType().ToString() +
          " имеет ClassCode=\"" + classCode + "\", отличное от запрошенного \"" + classCode + "\"");

      return Permission;
    }

    /// <summary>
    /// Создает разрешение заданного класса (свойство IUserPermissionCreator.Code)
    /// и выполняет чтение значения для разрешения из секции конфигурации.
    /// Если в списке нет генератора разрешения заданного класса, возвращается разрешение UnknownUserPermission.
    /// Таким образом, возвращаемое значение не может быть null.
    /// </summary>
    /// <param name="classCode">Код класса разрешения. Не может быть пустой строкой</param>
    /// <param name="cfg">Секция конфигурации. Не может быть null</param>
    /// <returns>Созданное разрешение</returns>
    public UserPermission Create(string classCode, CfgPart cfg)
    {
      if (cfg == null)
        throw new ArgumentNullException("cfg");

      UserPermission p = Create(classCode);
      p.Read(cfg);
      return p;
    }

    /// <summary>
    /// Возвращает массив объектов, реализующих интерфейс IUserPermissionCreatorWithDbInitialization
    /// </summary>
    /// <returns>Массив ссылок на интерфейс</returns>
    public IUserPermissionCreatorWithDbInitialization[] GetDbInitializators()
    {
      List<IUserPermissionCreatorWithDbInitialization> lst = null;
      foreach (IUserPermissionCreator item in this)
      {
        IUserPermissionCreatorWithDbInitialization item2 = item as IUserPermissionCreatorWithDbInitialization;
        if (item2 != null)
        {
          if (lst == null)
            lst = new List<IUserPermissionCreatorWithDbInitialization>();
          lst.Add(item2);
        }
      }
      if (lst == null)
        return new IUserPermissionCreatorWithDbInitialization[0];
      else
        return lst.ToArray();
    }

    #endregion

    #region Статический экземпляр

    /// <summary>
    /// Пустой список генераторов
    /// </summary>
    public static readonly UserPermissionCreators Empty = CreateEmpty();

    private static UserPermissionCreators CreateEmpty()
    {
      UserPermissionCreators Creators = new UserPermissionCreators();
      Creators.SetReadOnly();
      return Creators;
    }

    #endregion
  }


  /// <summary>
  /// Разрешение - заглушка.
  /// Использует внутренний объект TempCfg для хранения "параметров разрешения", которые самим
  /// объектом UnknownUserPermission не используются.
  /// Заглушка используется, когда в базе данных записаны разрешения пользователя неизвестного
  /// класса. Такие разрешения могут теоретически появиться при неудачном изменении конфигурации
  /// программного обеспечения.
  /// Заглушка сохраняет настройки неизвестного разрешения.
  /// </summary>
  public sealed class UnknownUserPermission : UserPermission
  {
    #region Конструктор

    /// <summary>
    /// Создает заглушку
    /// </summary>
    /// <param name="classCode">Код класса</param>
    public UnknownUserPermission(string classCode)
      : base(classCode)
    {
      _Cfg = new TempCfg();
    }

    #endregion

    #region Переопределенные свойства и методы

    /// <summary>
    /// Выгружает сохраненные настройки из TempCfg в <paramref name="cfg"/>.
    /// </summary>
    /// <param name="cfg">Записываемая секция конфигурации</param>
    public override void Write(CfgPart cfg)
    {
      _Cfg.CopyTo(cfg);
    }

    /// <summary>
    /// Сохраняет копию <paramref name="cfg"/> во внутреннем объекте TempCfg
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public override void Read(CfgPart cfg)
    {
      cfg.CopyTo(_Cfg);
    }

    private TempCfg _Cfg;

    /// <summary>
    /// Возвращает "Неизвестное разрешение"
    /// </summary>
    public override string ObjectText
    {
      get { return "Неизвестное разрешение \"" + ClassCode + "\""; }
    }

    /// <summary>
    /// Возвращает пустую строку
    /// </summary>
    public override string ValueText { get { return String.Empty; } }

    #endregion
  }
}
