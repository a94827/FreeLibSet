using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Text;
using FreeLibSet.Remoting;
using System.Globalization;
using FreeLibSet.Collections;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
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
 * Синтаксический разбор текста
 * Класс Parser содержит коллекцию "правил" ParserPart. Класс является "многоразовым"
 * Parser и ParserPart не содержат никаких ссылок на разбиваемый текст
 * Основной метод Parse() выполняет разбор переданных данных
 * 
 * Класс TextWithRows предназначен для хранения исходного текста для разбора. Получая в конструкторе объект
 * string, он выполняет разбиение на строки и позоволяет преобразовывать абсолютную позицию в номер строки и столбца
 * и обратно
 * 
 * Класс ParsingData содержит данные для разбора. Объект создается каждый раз, когда необходимо выполнить разбор
 * - Свойство Text содержит исходный текст в виде TextWithRows
 * - Свойство Tokens содержит результат разбора
 * 
 * Основным результатом синтаксического разбора является массив объектов Token. Token является структурой 
 * фиксированного размера. Тип лексемы определяется строковым полем Token.TokenType, например,
 * "TableRef", "Operation", "Comment", "Space", "Error" и т.д.
 * Объект Token содержит диапазон позиций в исходном тексте, чтобы можно было выполнить подсветку синтаксиса
 * Также он может содержать дополнительные данные, если работы с лексемой недостаточно только текста. 
 * Например, для лексемы "TableRef" задается имя ссылки на таблицы
 * 
 * В процессе разбора, метод Parser.Parse() предлагает каждому объекту ParserPart распознать очередную позицию
 * текста. Класс, производный от ParserPart, пытается сравнить фрагмент текста, начиная с текущей позиции, со
 * "своим" синтаксисом. Например, класс OperationParser проверяет, что очередная позиция содержит "+", "-", "*"
 * или "/". Если условие выполняется, создается лексема "Operation". 
 * 
 * Цикл по ParserPart прекращается, когда очередной элемент смог выполнить разбор.
 * Отсюда следует, что порядок размещения элементов ParserPart имеет значение. Например, если используется 
 * комментарий "//", то CommentParser должен идти до OperationParser. В противном случае, вместо лексемы "Comment"
 * будет добавлено две лексемы "Operation" для операций "/", а затем возникнет ошибка
 * 
 * Если ни один ParserPart не смог распознать текст, добавляется ErrorToken, содержащий один символ, после чего
 * выполняется переход к следующей позиции. Если и следующая позиция содержит ошибку, то новый ErrorToken
 * не добавляется, а к предыдущему добавляется один символ
 * 
 * Пробельные символы образуют Token типа "Space", также группирующие идущие подряд пробелы
 * Объект Token может содержать ошибку или предупреждение. Метод ParsingData.GetErrorMessages() позволяет
 * собрать список ошибок.
 * 
 * Список объектов Token еще не образуют дерево вычислений, т.к. список является "линейным". Кроме того, 
 * возможно бессмысленное расположение лексем. Например, могут быть обнаружены лексемы:
 * 1. "Operation"
 * 2. "Space"
 * 3. "Operation"
 * 
 * Следующий шаг - это построение дерева вычислений. Он может быть выполнен, только если нет ни одной ErrorToken,
 * то есть когда первый шаг завершился без ошибок.
 * 
 * В общем случае, целью парсинга может быть не получение дерева вычислений, а, например, получение диапазона
 * к которому применимо выражение
 * 
 * Также, из одного исходного условия может быть получено несколько вычисляемых выражений
 * Таким образом, задача парсинга, результатом которой являются объекты Token, отделяется от задачи получения
 * вычисляемого выражения Expression.
 * Для этого используется второй метод IParser.CreateExpression(), получающий на входе ParsingData и
 * возвращающий объект IExpression. 
 * На момент вызова метода все Token уже распознаны.
 * В отличие от первого шага, метод IParser.CreateExpression() вызывается для того IParser, который создал
 * очередную лексему, перебор объектов IParser не нужен.
 * Метод может вернуть null, если лексема не имеет значения, например, это пробел или комментарий. В этом
 * случае разбирается следующая лексема. 
 * 
 * Сериализация
 * ------------
 * Объекты ParserList и реализации IParser, ParsingData и Token не являются сериализуемыми
 * Для передачи ошибок от сервера к клиенту можно использовать метод ParsingData.GetErrorMessages(),
 * который возвращает сериализуемые данные. К каждому ErrorMessageItem к полю Tag присоединяется сериализуемый 
 * объект ParsingErrorItemData, который содержит некоторую часть данных из объекта Token
 */

namespace FreeLibSet.Parsing
{
  #region IExpression

  /// <summary>
  /// Интерфейс объекта - вычислителя, возвращаемого ParserList.CreateExpression
  /// </summary>
  public interface IExpression
  {
    /// <summary>
    /// Основной метод, выполняющий вычисления.
    /// Метод не получает никакого контекста для вычисления. Если для вычисления требуются данные, например,
    /// для извлечения ячейки электронной таблицы, то контекст должен быть сохранен в полях объекта при его
    /// создании. Для передачи данных следует использовать свойство ParsingData.UserData
    /// </summary>
    /// <returns>Результат вычислений</returns>
    object Calc();

    /// <summary>
    /// Свойство возвращает true, если выражение является константой
    /// Используется для оптимизации
    /// </summary>
    bool IsConst { get; }

    /// <summary>
    /// Получить лексемы, относящиеся к выражению. 
    /// Обычно добавляется единственная лексема, например, для BinaryOpExpression возвращается лексема знака 
    /// операции. Однако, для функции возвращается три лексемы: имя функции, открывающая скобка и закрывающая скобка
    /// Возвращаются лексемы, относящиеся непосредсственно к этому вычислителю.
    /// Если требуется получить все лексемы, включая относящиеся к дочерним выражениям, следует использовать
    /// статический метод ParsingData.GetTokens()
    /// </summary>
    /// <param name="tokens">Список, куда следует добавить ссылку</param>
    void GetTokens(IList<Token> tokens);

    /// <summary>
    /// Получить зависимые выражения
    /// Для констант и методов извлечения внешних значений, например, ссылок на ячейку электронной таблицы,
    /// никаких действий не выполняется. Для бинарных операций добавляется пара выражений. Для функций 
    /// добавляются все аргументы.
    /// Рекурсивный вызов не выполняется.
    /// Если требуется получить все вычислители рекурсивно, следует использовать статический метод
    /// ParsingData.GetExpressions()
    /// </summary>
    /// <param name="expressions">Список, куда следует добавить ссылки</param>
    void GetChildExpressions(IList<IExpression> expressions);

    /// <summary>
    /// Получение текстового представление для вычислителя
    /// </summary>
    /// <param name="data">Заполняемые данные</param>
    void Synthesize(SynthesisData data);
  }

  #endregion

  #region IParser

  /// <summary>
  /// Интефрейс одного парсера, добавляемого в список ParserList
  /// </summary>
  public interface IParser
  {
    /// <summary>
    /// Попытка выполнения парсинга.
    /// Метод должен попытаться выполнить парсинг текста Data.Text, начиная с позиции Data.CurrPos.
    /// Если парсинг выполнен успешно, метод должен добавить одну или несколько записей в массив Tokens.
    /// При этом опрос следующих элементов в списке Parser.Parts не выполняется и выполняется парсинг следующего
    /// фрагмента текста.
    /// 
    /// Если текущая часть текста не обрабатывается данным парсером, метод не выполняет никаких действий. Будет
    /// запрошен следующий объект в Parser.Parts. Если ни один парсер не распознал текст, добавляется лексема
    /// с ошибкой "нераспознанный символ"
    /// 
    /// Если парсер распознал часть текста, то не смог распознать лексему полностью, или распознанное значение
    /// некорректно, метод добавляет лексему(ы) с ошибками в список Data.SuspectedTokens. После этого будут
    /// опрошены оставшиеся парсеры в списке Parser.Parts. Если какой-нибудь из них сумеет выполнить парсинг
    /// без ошибок, список SuspectedTokens будет отброшен. Если парсинг не будет выполнен, элементы из
    /// SuspectedTokens будут перенесены в основной список Tokens
    /// </summary>
    /// <param name="data">Данные для парсинга</param>
    void Parse(ParsingData data);

    /// <summary>
    /// Второй шаг парсинга
    /// </summary>
    /// <param name="data">Данные для парсинга</param>
    /// <param name="leftExpression">Левая часть выражения</param>
    /// <returns>Созданный вычисляемый объект</returns>
    IExpression CreateExpression(ParsingData data, IExpression leftExpression);
  }

  #endregion

  /// <summary>
  /// Текущее состояние объекта ParsingData
  /// </summary>
  public enum ParsingState
  {
    /// <summary>
    /// Метод ParserList.Parse() еще не вызывался
    /// </summary>
    Init,

    /// <summary>
    /// В данный момент выполняется вызов ParserList.Parse()
    /// </summary>
    Parsing,

    /// <summary>
    /// Метод ParserList.Parse() выполнен, CreateExpression() еще не вызывался
    /// </summary>
    Parsed,

    /// <summary>
    /// В данный момен выполняется вызов ParserList.CreateExpression()
    /// </summary>
    ExpressionCreating,

    /// <summary>
    /// Был выполнен вызов ParserList.CreateExpression()
    /// </summary>
    ExpressionCreated,
  }

  /// <summary>
  /// Коллекция парсеров "многоразового применения", реализующих интефейс IParser
  /// </summary>
  public class ParserList : ListWithReadOnly<IParser>
  {
    #region Конструктор

    /// <summary>
    /// Создает список, содержащий единственный парсер, который определяет "нераспознанные символы".
    /// </summary>
    public ParserList()
    {
      _ErrorParser = new DefaultErrorParser();
    }

    #endregion

    #region Создание лексем (первый проход парсинга)

    /// <summary>
    /// Первый проход парсинга. Заполняет список ParsingData.Tokens
    /// </summary>
    /// <param name="data"></param>
    public void Parse(ParsingData data)
    {
      base.SetReadOnly();

      if (data == null)
        throw new ArgumentNullException("data");

      data.Tokens.Clear();
      data.Parsers = this;
      data.State = ParsingState.Parsing;

      data.InitCurrPos();

      DoParse(data, DataTools.EmptyStrings);

      #region Проверяем перекрытие лексем

      if (data.FirstErrorToken == null)
      {
        int LastPos = 0;
        for (int i = 0; i < data.Tokens.Count; i++)
        {
          if (data.Tokens[i].Start < 0)
          {
            data.Tokens[i].SetError("Начальная позиция лексемы \"" + data.Tokens[i].TokenType+ "\" не может быть отрицательной");
            break;
          }
          if (data.Tokens[i].Start < LastPos)
          {
            data.Tokens[i].SetError("Лексема \"" + data.Tokens[i].TokenType + "\" перекрывается с предыдущей \"" + data.Tokens[i-1].TokenType + "\"");
            break;
          }
          if ((data.Tokens[i].Start + data.Tokens[i].Length) > data.Text.Text.Length)
          {
            data.Tokens[i].SetError("Лексема \"" + data.Tokens[i].TokenType + "\" выходит за границу текста");
            break;
          }
          LastPos = data.Tokens[i].Start + data.Tokens[i].Length;
        }
      }

      #endregion

      data.State = ParsingState.Parsed;
    }

    private void DoParse(ParsingData data, string[] endTokens)
    {
      List<Token> TempTokens = new List<Token>();

      while (data.CurrPos < data.Text.Text.Length)
      {
        int CurrTokenCount = data.Tokens.Count;
        TempTokens.Clear();
        for (int i = 0; i < Count; i++)
        {
          data.SuspectedTokens.Clear();

          this[i].Parse(data); // основной метод

          if (data.Tokens.Count > CurrTokenCount)
          {
            // Распознавание успешно выполнено
            data.InitCurrPos();
            break;
          }

          if (data.SuspectedTokens.Count > 0)
          {
            // Выполнено частичное распознавание
            if (TempTokens.Count == 0) // приоритет отдается первом парсеру в списке
              TempTokens.AddRange(data.SuspectedTokens);
            // продолжаем разбор
          }
        } // цикл по парсерам

        if (data.Tokens.Count == CurrTokenCount)
        {
          // Успешного распознавания не было
          if (TempTokens.Count > 0)
          {
            // но было частичное распознавание
            data.Tokens.AddRange(TempTokens);
            data.InitCurrPos();
          }
          else
          {
            // Добавляем лексему ошибки "нераспознанный символ"
            ErrorParser.Parse(data);
            data.InitCurrPos();
          }
        }

        if (Array.IndexOf<string>(endTokens, data.CurrTokenType) >= 0)
          break;
      } // цикл по символам текста
    }

    /// <summary>
    /// Выполнить распознание лексем с помощью альтернативного списка парсеров.
    /// Этот метод может вызываться исключительно из метода, реализующего IParser.Parse().
    /// Метод вызывается для списка парсеров, отличного от того, в который входит текущий парсер.
    /// </summary>
    /// <param name="data">Данные парсинга, для которых сейчас выполняется метод Parse()</param>
    /// <param name="endTokens">Коды заверщающих лексем. Если в процессе парсинга добавляется соответствующая лексема,
    /// метод SubParse() завершается и продолжается парсинг с помощью основного списка ParserList</param>
    public void SubParse(ParsingData data, string[] endTokens)
    {
      base.SetReadOnly();

#if DEBUG
      if (data == null)
        throw new ArgumentNullException("data");
#endif
      if (data.State != ParsingState.Parsing)
        throw new InvalidOperationException("Метод SubParse() может вызываться исключительно из реализации IParser.Parse()");

      if (endTokens == null)
        endTokens = DataTools.EmptyStrings;
      data.InitCurrPos();
      DoParse(data, endTokens);
    }

    #endregion

    #region "Распознаватель" ошибки

    /// <summary>
    /// Парсер, добавляющий сообщение об ошибке, если ни один парсер в списке не смог распознать текст
    /// Свойство может быть установлено, если стандартное сообщение об ошибке не устраивает
    /// Парсер обязан добавить лексему ненулевой длины или удлиннить предыдущую лексему
    /// </summary>
    public IParser ErrorParser
    {
      get { return _ErrorParser; }
      set
      {
        CheckNotReadOnly();
        if (value == null)
          throw new ArgumentException();
        _ErrorParser = value;
      }
    }
    private IParser _ErrorParser;

    private class DefaultErrorParser : IParser
    {
      public void Parse(ParsingData data)
      {
        if (data.CurrTokenParser == this)
        {
          data.CurrToken.Length++;
          if (data.CurrToken.Length == 2)
          {
            data.CurrToken.ClearError();
            data.CurrToken.SetError("Нераспознанные символы");
          }
        }
        else
        {
          data.Tokens.Add(new Token(data, this, "Error", data.CurrPos, 1));
          data.CurrToken.SetError("Нераспознанный символ \"" + data.CurrToken.Text + "\"");
        }
      }

      public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
      {
        return null;
      }
    }

    #endregion

    #region Построение выражений (второй проход парсинга)

    /// <summary>
    /// Создать вычисляемое выражение.
    /// Эта версия позволяет автоматически выбрасывать исключение в случае ошибки
    /// </summary>
    /// <param name="data">Данные парсинга</param>
    /// <param name="throwException">Если true, то будет выброшено исключение, если не удалось создать выражение</param>
    /// <returns></returns>
    public IExpression CreateExpression(ParsingData data, bool throwException)
    {
      IExpression Expr = CreateExpression(data);
      Token et = data.FirstErrorToken;
      if (Expr == null)
      {
        if (throwException)
        {
          if (et == null)
            throw new BugException("Не удалось создать вычислимое выражение. Причина ошибки неизвестна");
          else
            throw new ParserCreateExpressionException("Ошибка парсинга. " + et.ErrorMessage.Value.Text);
        }
      }
      else
      {
        if (et != null)
        {
          if (throwException)
            throw new ParserCreateExpressionException("Ошибка получения выражения. " + et.ErrorMessage.Value.Text);
          // не удаляем, оно может пригодиться при анализе ошибок Expr = null;
        }
      }

      return Expr;
    }

    /// <summary>
    /// Создать вычисляемое выражение.
    /// В случае ошибки возвращается null.
    /// </summary>
    /// <param name="data">Данные парсинга</param>
    /// <returns>Выражение, которое может быть вычислено. Если есть ошибочные лексемы, вовзращается null</returns>
    public IExpression CreateExpression(ParsingData data)
    {
      if (data == null)
        throw new ArgumentNullException("data");
      if (data.Parsers == null)
        throw new NullReferenceException("Парсинг лексем не выполнялся");
      if (data.Parsers != this)
        throw new NullReferenceException("Парсинг лексем выполнялся другим списком парсеров");

      if (data.InternalTokenIndex != -1)
        throw new ReenteranceException("Вложенный вызов метода не допускается");

      if (data.FirstErrorToken != null)
        return null; // были ошибки на первой фазе парсинга

      data.State = ParsingState.ExpressionCreating;
      IExpression ResExpr;
      data.InternalTokenIndex = 0;
      data.EndTokens = DataTools.EmptyStrings;
      try
      {
        ResExpr = CreateSubExpression(data, null);
      }
      finally
      {
        data.InternalTokenIndex = -1;
        data.EndTokens = null;
        data.State = ParsingState.ExpressionCreated;
      }

      return ResExpr;
    }

    /// <summary>
    /// Создает вычислимое выражение, используюшее лексемы, до заданного списка включительно.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="endTokens"></param>
    /// <returns></returns>
    public IExpression CreateSubExpression(ParsingData data, string[] endTokens)
    {
      if (data.EndTokens == null)
        throw new InvalidOperationException("Метод должен вызываться только в процессе разбора всего выражения, а не самостоятельно");

      if (endTokens == null)
        endTokens = data.EndTokens;

      IExpression ResExpr = null;

      string[] OldEndTokens = data.EndTokens;
      data.EndTokens = endTokens;
      try
      {
        while (data.InternalTokenIndex < data.Tokens.Count)
        {
          int CurrIndex = data.InternalTokenIndex;

          if (Array.IndexOf<string>(endTokens, data.CurrTokenType) >= 0)
            // найдена завершающая скобка
            break;

          IExpression ThisExpr = data.Tokens[CurrIndex].Parser.CreateExpression(data, ResExpr);
          if (ThisExpr != null)
            ResExpr = ThisExpr;

          if (data.InternalTokenIndex == CurrIndex) // текущая позиция не передвинута
            data.InternalTokenIndex++;
        }

      }
      finally
      {
        data.EndTokens = OldEndTokens;
      }

      return ResExpr;
    }

    #endregion

    #region Вспомогательные методы работы со списком

    /// <summary>
    /// Возвращает парсер заданного типа из списка
    /// или null, если такого парсера нет.
    /// Возвращается парсер производного типа
    /// </summary>
    /// <typeparam name="T">Тип парсера</typeparam>
    /// <returns>Парсер или null</returns>
    public T GetParser<T>()
      where T : IParser
    {
      int Index = IndexOf(typeof(T));
      if (Index < 0)
        return default(T);
      else
        return (T)(base[Index]);
    }

    /// <summary>
    /// Возвращает индекс парсера заданного типа в списке
    /// или (-1), если такого парсера нет
    /// Возвращается парсер производного типа
    /// </summary>
    /// <param name="parserType">Тип парсера</param>
    /// <returns>Индекс найденного парсера</returns>
    public int IndexOf(Type parserType)
    {
      for (int i = 0; i < Count; i++)
      {
        if (base[i].GetType() == parserType)
          return i;
        if (base[i].GetType().IsSubclassOf(parserType))
          return i;
      }

      return -1;
    }

    #endregion
  }

  /// <summary>
  /// Одна лексема
  /// </summary>
  public sealed class Token
  {
    #region Конструктор

    /// <summary>
    /// Версия конструктора без доп. данных
    /// </summary>
    /// <param name="data">Данные парсинга, куда потом будет добавлена лексема</param>
    /// <param name="parser">Парсер, создающий лексему</param>
    /// <param name="tokenType">Тип лексемы</param>
    /// <param name="start">Позиция начала лексемы</param>
    /// <param name="length">Длина текста, определяющего лексему</param>
    public Token(ParsingData data, IParser parser, string tokenType, int start, int length)
      : this(data, parser, tokenType, start, length, null, null)
    {
    }

    /// <summary>
    /// Версия конструктора с доп. данными
    /// </summary>
    /// <param name="data">Данные парсинга, куда потом будет добавлена лексема</param>
    /// <param name="parser">Парсер, создающий лексему</param>
    /// <param name="tokenType">Тип лексемы</param>
    /// <param name="start">Позиция начала лексемы</param>
    /// <param name="length">Длина текста, определяющего лексему</param>
    /// <param name="auxData">Дополнительные данные для лексемы, например, имя функции, знак операции или значение константы.
    /// Назначение определяется парсером</param>
    public Token(ParsingData data, IParser parser, string tokenType, int start, int length, object auxData)
      : this(data, parser, tokenType, start, length, auxData, null)
    {
    }

    /// <summary>
    /// Версия конструктора с возможным сообщением об ошибке
    /// </summary>
    /// <param name="data">Данные парсинга, куда потом будет добавлена лексема</param>
    /// <param name="parser">Парсер, создающий лексему</param>
    /// <param name="tokenType">Тип лексемы</param>
    /// <param name="start">Позиция начала лексемы</param>
    /// <param name="length">Длина текста, определяющего лексему</param>
    /// <param name="auxData">Дополнительные данные для лексемы, например, имя функции, знак операции или значение константы.
    /// Назначение определяется парсером</param>
    /// <param name="errorMessage">Если не null, то для лексемы будет задано сообщение об ошибке или предупреждение</param>
    public Token(ParsingData data, IParser parser, string tokenType, int start, int length, object auxData, ErrorMessageItem? errorMessage)
    {
      if (data == null)
        throw new ArgumentException("data");
      if (data.State != ParsingState.Parsing)
        throw new InvalidOperationException("Набор данных не находится в состоянии парсинга");
      if (parser == null)
        throw new ArgumentNullException("parser");
      if (String.IsNullOrEmpty(tokenType))
        throw new ArgumentNullException("tokenType");
      if (length > 0)
      {
        if (start < 0 || start >= data.Text.Text.Length)
          throw new ArgumentOutOfRangeException("start", start, "Start должно быть в диапазоне от 0 до " +
            (data.Text.Text.Length - 1).ToString());
      }
      if (length < 0 || (start + length) > data.Text.Text.Length)
        throw new ArgumentOutOfRangeException("length", length, "Length должно быть в диапазоне от 1 до " +
          (data.Text.Text.Length - 1 - start).ToString());

      _Data = data;
      _Parser = parser;
      _TokenType = tokenType;
      _Start = start;
      _Length = length;
      _AuxData = auxData;
      _ErrorMessage = errorMessage;
      if (errorMessage == null)
        _ErrorState = ParsingState.Init;
      else
        _ErrorState = data.State;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Ссылка на объект исходных данных
    /// </summary>
    public ParsingData Data { get { return _Data; } }
    private readonly ParsingData _Data;

    /// <summary>
    /// Парсер, добавившего данную лексему
    /// </summary>
    public IParser Parser { get { return _Parser; } }
    private readonly IParser _Parser;

    /// <summary>
    /// Тип лексемы
    /// При поиске является регистрочувствительным
    /// </summary>
    public string TokenType { get { return _TokenType; } }
    private readonly string _TokenType;

    /// <summary>
    /// Начало объекта в тексте Data.Text
    /// </summary>
    public int Start { get { return _Start; } }
    private readonly int _Start;

    /// <summary>
    /// Длина лексемы
    /// Допускаются лексемы нулевой длины для добавления сообщений
    /// Установка свойства используется для лексем "Error", чтобы не создавать множество последовательных объектов
    /// </summary>
    public int Length
    {
      get { return _Length; }
      set
      {
        if (value < 0 || (Start + value) > Data.Text.Text.Length)
          throw new ArgumentOutOfRangeException("value", value, "Length должно быть в диапазоне от 1 до " +
            (Data.Text.Text.Length - 1 - Start).ToString());

        _Length = value;
      }
    }
    private int _Length;

    /// <summary>
    /// Возвращает фрагмент текста из Data.Text, исходя из свойств Start и Length.
    /// </summary>
    public string Text
    {
      get
      {
        if (_Data == null)
          return String.Empty;
        else
          return _Data.Text.Text.Substring(_Start, _Length);
      }
    }

    /// <summary>
    /// Текстовое представление лексемы
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (Data == null)
        return "Empty";

      StringBuilder sb = new StringBuilder();
      sb.Append(TokenType);
      sb.Append(" (");
      sb.Append(Start.ToString());
      sb.Append(", ");
      sb.Append(Length.ToString());
      sb.Append(")");

      if (_ErrorMessage.HasValue)
      {
        sb.Append(". ");
        sb.Append(_ErrorMessage.Value.Kind.ToString());
        sb.Append(": ");
        sb.Append(_ErrorMessage.Value.Text);
      }
      else
      {
        if (Length > 0)
        {
          sb.Append(" \"");
          sb.Append(Text.ToString());
          sb.Append("\"");
        }
        else
          sb.Append(" (EmptyText)");
      }

      return sb.ToString();
    }

    /// <summary>
    /// Дополнительные данные, обрабатываемые Parser 
    /// </summary>
    public object AuxData { get { return _AuxData; } }
    private readonly object _AuxData;

    /// <summary>
    /// Если свойство отлично от null, лексема содержит ошибку или предупреждение
    /// Установка свойства работает только "на повышение"
    /// </summary>
    public ErrorMessageItem? ErrorMessage
    {
      get { return _ErrorMessage; }
      set
      {
        if (!value.HasValue)
          return;
        if (!_ErrorMessage.HasValue)
        {
          _ErrorMessage = value;
          _ErrorState = Data.State;
          return;
        }

        // Замещающая установка свойства
        if (ErrorMessageList.Compare(value.Value.Kind, _ErrorMessage.Value.Kind) > 0)
        {
          _ErrorMessage = value;
          _ErrorState = Data.State;
        }
      }
    }
    private ErrorMessageItem? _ErrorMessage;

    /// <summary>
    /// Очищает сообщение об ошибке.
    /// Используется, когда нужно заменить одно сообщение на другое
    /// </summary>
    public void ClearError()
    {
      _ErrorMessage = null;
      _ErrorState = ParsingState.Init;
    }

    /// <summary>
    /// Стадия парсинга, на которой обнаружена ошибка
    /// </summary>
    public ParsingState ErrorState
    {
      get { return _ErrorState; }
    }
    private ParsingState _ErrorState;

    /// <summary>
    /// Пустой массив лексем
    /// </summary>
    public static readonly Token[] EmptyArray = new Token[0];

    #endregion

    #region Вспомогательные методы установки ошибок

    /// <summary>
    /// Устанавливает ошибку для лексемы
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    public void SetError(string text)
    {
      this.ErrorMessage = new ErrorMessageItem(ErrorMessageKind.Error, text);
    }

    /// <summary>
    /// Устанавливает ошибку для лексемы
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="code">Код ошибки</param>
    public void SetError(string text, string code)
    {
      this.ErrorMessage = new ErrorMessageItem(ErrorMessageKind.Error, text, code);
    }

    /// <summary>
    /// Устанавливает предупреждение для лексемы
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    public void SetWarning(string text)
    {
      this.ErrorMessage = new ErrorMessageItem(ErrorMessageKind.Warning, text);
    }

    /// <summary>
    /// Устанавливает предупреждение для лексемы
    /// </summary>
    /// <param name="text">Текст сообщения</param>
    /// <param name="code">Код ошибки</param>
    public void SetWarning(string text, string code)
    {
      this.ErrorMessage = new ErrorMessageItem(ErrorMessageKind.Warning, text, code);
    }

    #endregion

    #region Сортировка

    /// <summary>
    /// Возвращает свойство Start
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return Start;
    }

    #endregion
  }

  /// <summary>
  /// Данные для парсинга
  /// Объект не является потокобезопасным в процессе парсинга
  /// </summary>
  public class ParsingData
  {
    #region Конструктор

    /// <summary>
    /// Создает объект TextWithRows, а затем - ParsingData
    /// </summary>
    /// <param name="text">Распознаваемый текст. Не может быть null</param>
    public ParsingData(string text)
      : this(new TextWithRows(text))
    {
    }

    /// <summary>
    /// Создает объект парсинга.
    /// </summary>
    /// <param name="text">Объект для извлечения текста. Не может быть null</param>
    public ParsingData(TextWithRows text)
    {
      if (text == null)
        throw new ArgumentNullException("text");
      _Text = text;
      _Tokens = new TokenList();
      _SuspectedTokens = new List<Token>();
      _CurrTokenIndex = -1; // до перехода в стадию создания выражений
      _UserData = new NamedValues();
      _State = ParsingState.Init;
    }

    #endregion

    #region Исходный текст

    /// <summary>
    /// Объект извлечения текста.
    /// Задается в конструкторе.
    /// </summary>
    public TextWithRows Text { get { return _Text; } }
    private readonly TextWithRows _Text;

    #endregion

    #region Текущая позиция и вспомогательные методы извлечения текста

    /// <summary>
    /// Текущая позиция в тексте в процессе парсинга на первом шаге (Parse)
    /// </summary>
    public int CurrPos
    {
      get { return _CurrPos; }
      //internal set { _CurrPos = value; }
    }
    private int _CurrPos;

    internal void InitCurrPos()
    {
      Token t = CurrToken;
      if (t == null)
        _CurrPos = 0;
      else
        _CurrPos = CurrToken.Start + CurrToken.Length;
    }

    /// <summary>
    /// Возвращает true, если текст, начиная с текущей позиции, содержит указанную подстроку
    /// </summary>
    /// <param name="s">Искомая подстрока</param>
    /// <param name="ignoreCase">true, если не нужно учитывать регистр</param>
    /// <returns>true, если строка есть</returns>
    public bool StartsWith(string s, bool ignoreCase)
    {
      if (_CurrPos + s.Length > Text.Text.Length)
        return false; // не помещается

      string s2 = Text.Text.Substring(_CurrPos, s.Length);
      return String.Compare(s, s2, ignoreCase) == 0; // TODO: Equals()
    }

    /// <summary>
    /// Возвращает true, если текст, начиная с указанной позиции, содержит указанную подстроку
    /// </summary>
    /// <param name="startPos">Начальная позиция для поиска</param>
    /// <param name="s">Искомая подстрока</param>
    /// <param name="ignoreCase">true, если не нужно учитывать регистр</param>
    /// <returns>true, если строка есть</returns>
    public bool StartsWith(int startPos, string s, bool ignoreCase)
    {
      if (startPos < 0)
        return false;
      if (startPos + s.Length > Text.Text.Length)
        return false; // не помещается

      string s2 = Text.Text.Substring(startPos, s.Length);
      return String.Compare(s, s2, ignoreCase) == 0; // TODO: Equals()
    }

    /// <summary>
    /// Возвращает длину строки от текущей позиции (включительно) до конца строки (без символов перевода строки)
    /// </summary>
    /// <returns></returns>
    public int GetRowLength()
    {
      int Row = Text.GetRow(_CurrPos);
      int Start = Text.GetRowStartIndex(Row);
      int Len = Text.GetRowLength(Row);
      return Len - (_CurrPos - Start);
    }

    /// <summary>
    /// Возвращает символ в указанной позиции.
    /// Если индекс находится вне диапазона, возвращается '0x0'
    /// </summary>
    /// <param name="index">Индекс символа</param>
    /// <returns>Символ</returns>
    public char GetChar(int index)
    {
      if (index < 0 || index >= Text.Text.Length)
        return '\0';
      else
        return Text.Text[index];
    }

    /// <summary>
    /// Возвращает символ в указанной позиции.
    /// Эквивалентно вызову GetChar(CurrPos).
    /// Если CurrPos находится вне диапазона, возвращается '0x0'
    /// </summary>
    public char CurrChar
    {
      get { return GetChar(CurrPos); }
    }

    /// <summary>
    /// Возвращает текст от текущей позиции (включительно) до конца строки
    /// </summary>
    public string StringRemainder
    {
      get
      {
        int l = GetRowLength();
        if (l == 0)
          return String.Empty;
        else
          return Text.Text.Substring(CurrPos, l);
      }
    }

    /// <summary>
    /// Найти позицию заданного символа <paramref name="ch"/>, начиная с заданной позиции <paramref name="startIndex"/>.
    /// Возвращает (-1), если нет такого символа.
    /// </summary>
    /// <param name="ch">Символ для поиска</param>
    /// <param name="startIndex">Позиция, с которой следует начать поиск.
    /// Если значение находится вне диапазоне 0 .. (Text.Length-1), исключение не генерируется, вместо этого возвращается (-1)</param>
    /// <returns></returns>
    public int IndexOf(char ch, int startIndex)
    {
      if (startIndex < 0 || startIndex >= Text.Text.Length)
        return -1;
      return Text.Text.IndexOf(ch, startIndex, Text.Text.Length - startIndex);
    }

    #endregion

    #region Лексемы

    /// <summary>
    /// Список добавленных лексем (свойство ParsingData.Tokens)
    /// </summary>
    public class TokenList : List<Token>
    {
      #region Дополнительные методы и свойства

      /// <summary>
      /// Индекс первой лексемы заданного типа.
      /// Возвращает (-1), если нет такой лексемы
      /// </summary>
      /// <param name="tokenType">Тип лексемы (свойство Token.TokenType)</param>
      /// <returns>Найденный индекс</returns>
      public int IndexOf(string tokenType)
      {
        for (int i = 0; i < Count; i++)
        {
          if (this[i].TokenType == tokenType)
            return i;
        }

        return -1;
      }

      /// <summary>
      /// Индекс последней лексемы заданного типа.
      /// Возвращает (-1), если нет такой лексемы
      /// </summary>
      /// <param name="tokenType">Тип лексемы (свойство Token.TokenType)</param>
      /// <returns>Найденный индекс</returns>
      public int LastIndexOf(string tokenType)
      {
        for (int i = Count - 1; i >= 0; i--)
        {
          if (this[i].TokenType == tokenType)
            return i;
        }

        return -1;
      }

      /// <summary>
      /// Определение наличия лексемы заданного типа.
      /// </summary>
      /// <param name="tokenType">Тип лексемы (свойство Token.TokenType)</param>
      /// <returns>true, если есть лексема</returns>
      public bool Contains(string tokenType)
      {
        return IndexOf(tokenType) >= 0;
      }

      /// <summary>
      /// Поиск первой лексемы заданного типа.
      /// Возвращает найденную лексему или null, если нет такой лексемы
      /// </summary>
      /// <param name="tokenType">Тип лексемы (свойство Token.TokenType)</param>
      /// <returns>Лексема</returns>
      public Token GetFirst(string tokenType)
      {
        int p = IndexOf(tokenType);
        if (p < 0)
          return null;
        else
          return this[p];
      }

      /// <summary>
      /// Поиск последней лексемы заданного типа.
      /// Возвращает найденную лексему или null, если нет такой лексемы
      /// </summary>
      /// <param name="tokenType">Тип лексемы (свойство Token.TokenType)</param>
      /// <returns>Лексема</returns>
      public Token GetLast(string tokenType)
      {
        int p = LastIndexOf(tokenType);
        if (p < 0)
          return null;
        else
          return this[p];
      }

      /// <summary>
      /// Возвращает массив всех лексем заданного вида.
      /// Если ни одной лексемы не найдено, возвращается пустой массив
      /// </summary>
      /// <param name="tokenType">Тип лексем</param>
      /// <returns>Массив найденных лексем</returns>
      public Token[] ToArray(string tokenType)
      {
        List<Token> lst = null;
        for (int i = 0; i < Count; i++)
        {
          if (this[i].TokenType == tokenType)
          {
            if (lst == null)
              lst = new List<Token>();
            lst.Add(this[i]);
          }
        }

        if (lst == null)
          return Token.EmptyArray;
        else
          return lst.ToArray();
      }

      /// <summary>
      /// Возвращает лексему в заданной позиции текста.
      /// Возвращает null, если заданная позиция не относится ни к одной из лексем, добавленных в список
      /// </summary>
      /// <param name="textPosition">Позиция текста</param>
      /// <returns>Лексема</returns>
      public Token GetAtPosition(int textPosition)
      {
        for (int i = 0; i < Count; i++)
        {
          if (this[i].Start > textPosition)
            return null;

          if (textPosition < (this[i].Start + this[i].Length))
            return this[i];
        }
        return null;
      }

      /// <summary>
      /// Удаляет все лексемы заданного типа
      /// </summary>
      /// <param name="tokenType">Тип лексем, например, "Space"</param>
      public void RemoveAll(string tokenType)
      {
        for (int i = Count - 1; i >= 0; i--)
        {
          if (this[i].TokenType == tokenType)
            RemoveAt(i);
        }
      }

      #endregion
    }

    /// <summary>
    /// Результаты парсинга. Список найденных лексем
    /// </summary>
    public TokenList Tokens { get { return _Tokens; } }
    private readonly TokenList _Tokens;

    /// <summary>
    /// Возвращает текущую лексему.
    /// В процессе добавления лексем возвращает последнюю добавленную лексему
    /// </summary>
    public Token CurrToken
    {
      get
      {
        if (_Tokens.Count == 0)
          return null;
        else if (_CurrTokenIndex < 0)
          return _Tokens[_Tokens.Count - 1];
        else if (_CurrTokenIndex >= _Tokens.Count)
          return null;
        else
          return _Tokens[_CurrTokenIndex];
      }
    }

    /// <summary>
    /// Возвращает тип текущей или последней лексемы
    /// </summary>
    public string CurrTokenType
    {
      get
      {
        Token t = CurrToken;
        if (t == null)
          return String.Empty;
        else
          return t.TokenType;
      }
    }

    /// <summary>
    /// Возвращает парсер, добавивший последнюю лексему
    /// Возвращает null, если лексем нет в списке
    /// </summary>
    public IParser CurrTokenParser
    {
      get
      {
        Token t = CurrToken;
        if (t == null)
          return null;
        else
          return t.Parser;
      }
    }

    /// <summary>
    /// Результаты парсинга для одного парсера, когда точное распознание текста не выполнено
    /// </summary>
    public List<Token> SuspectedTokens { get { return _SuspectedTokens; } }
    private readonly List<Token> _SuspectedTokens;

    /// <summary>
    /// Текущая лексема в процессе создания выражений.
    /// В процессе добавления лексем возвращает индекс последней лексемы
    /// Возвращает -1, если ни одной лексемы еще не добавлено
    /// </summary>
    public int CurrTokenIndex
    {
      get
      {
        if (_CurrTokenIndex < 0) // стадия сбора лексем
          return _Tokens.Count - 1;
        else
          return _CurrTokenIndex;
      }
    }

    /// <summary>
    /// Неотрицательное значение возвращается только в процессе работы метода ParserList.CreateExpression().
    /// В остальное время, в том числе при работе метода Parse(), возвращается (-1)
    /// </summary>
    internal int InternalTokenIndex
    {
      get { return _CurrTokenIndex; }
      set { _CurrTokenIndex = value; }
    }
    private int _CurrTokenIndex;

    /// <summary>
    /// Переместить текущую позицию в списке на лексему, следующую за указанной
    /// Метод может использоваться только в процессе получения вычислителя
    /// </summary>
    /// <param name="tokenIndex"></param>
    public void SkipToken(int tokenIndex)
    {
      if (_CurrTokenIndex < 0)
        throw new InvalidOperationException("Метод может использоваться только в процессе получения вычислителя");

      if (tokenIndex < 0 || tokenIndex >= Tokens.Count)
        throw new ArgumentOutOfRangeException("tokenIndex", tokenIndex, "Номер лексемы должен быть в диапазоне от 0 до " + (Tokens.Count - 1).ToString());

      _CurrTokenIndex = Math.Max(_CurrTokenIndex, tokenIndex + 1);
    }

    /// <summary>
    /// Переместить текущую позицию в списке на следующую лексему
    /// Метод может использоваться только в процессе получения вычислителя
    /// </summary>
    public void SkipToken()
    {
      SkipToken(CurrTokenIndex);
    }

    /// <summary>
    /// Возвращает следующую лексему, после <paramref name="currTokenIndex"/>, отличную от перечисленных в
    /// <paramref name="skipTokens"/>. Если <paramref name="currTokenIndex"/>=-1, то поиск ведется с начала
    /// списка
    /// </summary>
    /// <param name="currTokenIndex">Текущая лексема или (-1)</param>
    /// <param name="skipTokens">Пропускаемые коды лексем</param>
    /// <returns>Найденная лексема или null</returns>
    public Token GetNextToken(int currTokenIndex, string[] skipTokens)
    {
      int Index = GetNextTokenIndex(currTokenIndex, skipTokens);
      if (Index < 0)
        return null;
      else
        return Tokens[Index];
    }

    private int GetNextTokenIndex(int currTokenIndex, string[] SkipTokens)
    {
      if (SkipTokens == null)
        SkipTokens = DataTools.EmptyStrings;
      for (int Index = currTokenIndex + 1; Index < Tokens.Count; Index++)
      {
        if (Array.IndexOf<string>(SkipTokens, Tokens[Index].TokenType) < 0)
          return Index;
      }
      return -1;
    }

    /// <summary>
    /// Возвращает предыдущую лексему, перед <paramref name="currTokenIndex"/>, отличную от перечисленных в
    /// <paramref name="skipTokens"/>. Если <paramref name="currTokenIndex"/>=-1, то поиск ведется с конца
    /// списка
    /// </summary>
    /// <param name="currTokenIndex">Текущая лексема</param>
    /// <param name="skipTokens">Пропускаемые коды лексем</param>
    /// <returns>Лексема</returns>
    public Token GetPrevToken(int currTokenIndex, string[] skipTokens)
    {
      int Index = GetPrevTokenIndex(currTokenIndex, skipTokens);
      if (Index < 0)
        return null;
      else
        return Tokens[Index];
    }

    private int GetPrevTokenIndex(int currTokenIndex, string[] skipTokens)
    {
      if (skipTokens == null)
        skipTokens = DataTools.EmptyStrings;

      int StartIndex;
      if (currTokenIndex >= 0)
        StartIndex = currTokenIndex - 1;
      else
        StartIndex = Tokens.Count - 1;

      for (int Index = StartIndex; Index >= 0; Index--)
      {
        if (Array.IndexOf<string>(skipTokens, Tokens[Index].TokenType) < 0)
          return Index;
      }
      return -1;
    }

    /// <summary>
    /// Создать копию текущего объекта, содержащего только часть лексем, начиная с заданной
    /// </summary>
    /// <param name="firstTokenIndex">Индекс первой лексемы для копирования от 0 до Tokens.Count-1.
    /// Если равно Tokens.Count, то будет создана копия без лексем</param>
    /// <param name="tokenCount">Количество лексем для копирования</param>
    /// <returns>Копия объекта ParsingData</returns>
    public ParsingData CreateSubTokens(int firstTokenIndex, int tokenCount)
    {
      if (Parsers == null)
        throw new NullReferenceException("Парсинг лексем не выполнялся");

      if (firstTokenIndex < 0 || firstTokenIndex > Tokens.Count)
        throw new ArgumentOutOfRangeException("firstTokenIndex", firstTokenIndex, "Номер первой лексемы должен быть в диапазоне от 0 до " + Tokens.Count.ToString());
      if (tokenCount < 0 || (firstTokenIndex + tokenCount) > Tokens.Count)
        throw new ArgumentOutOfRangeException("tokenCount", tokenCount, "Число лексем должно быть в диапазоне от 0 до " + (Tokens.Count - firstTokenIndex).ToString());

      ParsingData Data2 = new ParsingData(Text);
      Data2._UserData = UserData.Clone(); // обязательно! Реализация может устанавливать разные параметры при разборе частей
      Data2.Parsers = Parsers;
      Data2.State = ParsingState.Parsed;

      for (int i = 0; i < tokenCount; i++)
      {
        Token tk = Tokens[firstTokenIndex + i];
        Data2.Tokens.Add(tk);
      }

      return Data2;
    }

    /// <summary>
    /// Создать копию текущего объекта, содержащего только часть лексем, начиная с заданной
    /// </summary>
    /// <param name="firstTokenIndex">Индекс первой лексемы для копирования от 0 до Tokens.Count-1.
    /// Если равно Tokens.Count, то будет создана копия без лексем</param>
    /// <returns>Копия объекта ParsingData</returns>
    public ParsingData CreateSubTokens(int firstTokenIndex)
    {
      int TokenCount = Tokens.Count - firstTokenIndex;
      return CreateSubTokens(firstTokenIndex, TokenCount);
    }

    /// <summary>
    /// Создает массив объектов ParsingData, содержащих лексемы, находящиеся между лексемами-разделителями заданного типа. 
    /// Лексемы-разделители не входят в результирующие данные.
    /// Среди элементов массива могут встретиться ParsingData без лексем
    /// </summary>
    /// <param name="tokenType">Тип лексемы-разделителя</param>
    /// <returns>Массив созданных объктов ParsingData</returns>
    public ParsingData[] SplitTokens(string tokenType)
    {
      int n = 1;
      for (int i = 0; i < Tokens.Count; i++)
      {
        if (Tokens[i].TokenType == tokenType)
          n++;
      }

      ParsingData[] Res = new ParsingData[n];
      n = 0;
      int FirstToken = 0;
      for (int i = 0; i < Tokens.Count; i++)
      {
        if (Tokens[i].TokenType == tokenType)
        {
          Res[n] = CreateSubTokens(FirstToken, i - FirstToken);
          n++;
          FirstToken = i + 1;
        }
      }
      // Последняя группа всегда есть
      Res[n] = CreateSubTokens(FirstToken, Tokens.Count - FirstToken);
      return Res;
    }

    /// <summary>
    /// Возвращает тип лексемы с заданным индексом.
    /// В отличие от вызова Tokens[tokenIndex].TokenType, этот метод возвращает пустую строку, а не выбрасывает исключение,
    /// если индекс лежит вне диапазона.
    /// </summary>
    /// <param name="tokenIndex">Индекс лексемы в списке Tokens</param>
    /// <returns>Toke.TokenType или пустая строка</returns>
    public string GetTokenType(int tokenIndex)
    {
      if (tokenIndex < 0 || tokenIndex >= Tokens.Count)
        return String.Empty;
      else
        return Tokens[tokenIndex].TokenType;
    }

    #endregion

    #region Список ошибок

    /// <summary>
    /// Возвращает список ошибок из списка лексем
    /// </summary>
    /// <returns></returns>
    public ErrorMessageList GetErrorMessages()
    {
      return GetErrorMessages(false);
    }

    /// <summary>
    /// Возвращает список всех сообщений.
    /// </summary>
    /// <param name="addTokenNumberAndState">Если true, то в сообщения будут добавлены номера п/п и стадия парсинга,
    /// на которой возникла ошибка</param>
    /// <returns>Список сообщений</returns>
    public ErrorMessageList GetErrorMessages(bool addTokenNumberAndState)
    {
      ErrorMessageList List = new ErrorMessageList();
      for (int i = 0; i < Tokens.Count; i++)
      {
        if (Tokens[i].ErrorMessage.HasValue)
        {
          ParsingErrorItemData Data = new ParsingErrorItemData(i,
            Tokens[i].TokenType, Tokens[i].Start, Tokens[i].Length);
          ErrorMessageItem Src = Tokens[i].ErrorMessage.Value;
          string Text = Src.Text;
          if (addTokenNumberAndState)
            Text = "№" + (i + 1).ToString() + " [" + Tokens[i].ErrorState.ToString() + "]. " + Text;
          List.Add(new ErrorMessageItem(Src.Kind, Text, Src.Code, Data));
        }
      }
      return List;
    }

    /// <summary>
    /// Получить список ошибок, обнаруженных при разборе данного выражения.
    /// Если выражение не задано, возвращается пустой список ошибок
    /// Обычно, при возникновении ошибок, выражения не возвращается, поэтому
    /// этот метод малополезен
    /// </summary>
    /// <param name="expression">Выражение</param>
    /// <returns>Список ошибок</returns>
    public static ErrorMessageList GetErrorMessages(IExpression expression)
    {
      ErrorMessageList List = new ErrorMessageList();
      List<Token> Tokens = new List<Token>();
      GetTokens(expression, Tokens);
      for (int i = 0; i < Tokens.Count; i++)
      {
        if (Tokens[i].ErrorMessage.HasValue)
        {
          ParsingErrorItemData Data = new ParsingErrorItemData(i,
            Tokens[i].TokenType, Tokens[i].Start, Tokens[i].Length);
          ErrorMessageItem Src = Tokens[i].ErrorMessage.Value;
          List.Add(new ErrorMessageItem(Src.Kind, Src.Text, Src.Code, Data));
        }
      }
      return List;
    }

    /// <summary>
    /// Максимальный уровень серьезности сообщений.
    /// Если нет ни одного присоединенного сообщения, то возвращается уровень "Info"
    /// </summary>
    public ErrorMessageKind Severity
    {
      get
      {
        bool HasWarning = false;
        for (int i = 0; i < Tokens.Count; i++)
        {
          if (Tokens[i].ErrorMessage.HasValue)
          {
            switch (Tokens[i].ErrorMessage.Value.Kind)
            {
              case ErrorMessageKind.Error:
                return ErrorMessageKind.Error;
              case ErrorMessageKind.Warning:
                HasWarning = true;
                break;
            }
          }
        }
        if (HasWarning)
          return ErrorMessageKind.Warning;
        else
          return ErrorMessageKind.Info;
      }
    }

    /// <summary>
    /// Возвращает первую лексему, содержащую ошибку
    /// Если ошибок нет, возвращается null
    /// </summary>
    public Token FirstErrorToken
    {
      get
      {
        for (int i = 0; i < Tokens.Count; i++)
        {
          if (Tokens[i].ErrorMessage.HasValue)
          {
            if (Tokens[i].ErrorMessage.Value.Kind == ErrorMessageKind.Error)
              return Tokens[i];
          }
        }
        return null;
      }
    }

    /// <summary>
    /// Возвращает первую лексему, содержащую ошибку, предупреждение или сообщение
    /// Если сообщений нет, возвращается null
    /// </summary>
    public Token FirstTokenWithMessage
    {
      get
      {
        for (int i = 0; i < Tokens.Count; i++)
        {
          if (Tokens[i].ErrorMessage.HasValue)
            return Tokens[i];
        }
        return null;
      }
    }

    #endregion

    #region Поиск лексем

    /// <summary>
    /// Получает список всех лексем, входящих в выражение
    /// Выполняется рекурсивный вызов IExpression.GetTokens().
    /// Найденные лексемы добавляются в список <paramref name="tokens"/>
    /// </summary>
    /// <param name="expression">Объект, в котором выполняется поиск</param>
    /// <param name="tokens">Список для добавления найденных лексем</param>
    public static void GetTokens(IExpression expression, List<Token> tokens)
    {
#if DEBUG
      if (tokens == null)
        throw new ArgumentNullException("tokens");
#endif

      if (expression == null)
        return;

      int Count = tokens.Count;
      expression.GetTokens(tokens);
      for (int i = Count; i < tokens.Count; i++)
      {
        if (tokens[i] == null)
          throw new NullReferenceException("Для выражения " + expression.ToString() + " метод GetTokens() добавил в список значение null");
      }

      List<IExpression> Children = new List<IExpression>();
      expression.GetChildExpressions(Children);
      for (int i = 0; i < Children.Count; i++)
        GetTokens(Children[i], tokens);
    }

    /// <summary>
    /// Получает список всех лексем заданного типа, входящих в выражение
    /// Выполняется рекурсивный вызов IExpression.GetTokens()
    /// </summary>
    /// <param name="expression">Объект, в котором выполняется поиск</param>
    /// <param name="Tokens">Список для добавления найденных лексем</param>
    /// <param name="tokenType">Тип лексем (свойство Token.TokenType), которые требуется найти</param>
    public static void GetTokens(IExpression expression, List<Token> Tokens, string tokenType)
    {
      if (expression == null)
        return;

      if (String.IsNullOrEmpty(tokenType))
        throw new ArgumentNullException("tokenType");

      List<Token> Tokens2 = new List<Token>();
      expression.GetTokens(Tokens2);
      for (int i = 0; i < Tokens2.Count; i++)
      {
        if (Tokens2[i].TokenType == tokenType)
          Tokens.Add(Tokens2[i]);
      }
      List<IExpression> Children = new List<IExpression>();
      expression.GetChildExpressions(Children);
      for (int i = 0; i < Children.Count; i++)
        GetTokens(Children[i], Tokens, tokenType);
    }

    /// <summary>
    /// Возвращает первую лексему заданного типа
    /// Выполняется рекурсивный вызов IExpression.GetTokens()
    /// </summary>
    /// <param name="expression">Объект, в котором выполняется поиск</param>
    /// <param name="tokenType">Тип лексемы (свойство Token.TokenType), которую требуется найти</param>
    /// <returns>Найденная лексема</returns>
    public static Token GetFirstToken(IExpression expression, string tokenType)
    {
      if (expression == null)
        return null;

      if (String.IsNullOrEmpty(tokenType))
        throw new ArgumentNullException("tokenType");

      List<Token> DummyTokens = new List<Token>();
      return DoGetFirstToken(DummyTokens, expression, tokenType);
    }

    private static Token DoGetFirstToken(List<Token> dummyTokens, IExpression expression, string tokenType)
    {
      dummyTokens.Clear();
      expression.GetTokens(dummyTokens);
      for (int i = 0; i < dummyTokens.Count; i++)
      {
        if (dummyTokens[i].TokenType == tokenType)
          return dummyTokens[i];
      }
      List<IExpression> Children = new List<IExpression>();
      expression.GetChildExpressions(Children);
      for (int i = 0; i < Children.Count; i++)
      {
        Token tk = DoGetFirstToken(dummyTokens, Children[i], tokenType);
        if (tk != null)
          return tk;
      }
      return null;
    }

    /// <summary>
    /// Возвращает первую лексему любого типа.
    /// По необходимости, выполняется рекурсивный вызов IExpression.GetTokens().
    /// </summary>
    /// <param name="expression">Объект, в котором выполняется поиск</param>
    /// <returns>Найденная лексема</returns>
    public static Token GetFirstToken(IExpression expression)
    {
      if (expression == null)
        return null;

      List<Token> Tokens2 = new List<Token>();
      expression.GetTokens(Tokens2);
      if (Tokens2.Count > 0)
        return Tokens2[0];

      List<IExpression> Children = new List<IExpression>();
      expression.GetChildExpressions(Children);
      for (int i = 0; i < Children.Count; i++)
      {
        Token tk = GetFirstToken(Children[i]);
        if (tk != null)
          return tk;
      }

      return null;
    }

    #endregion

    #region Прочее

    /// <summary>
    /// Произвольные пользовательские данные, которые могут потребоваться парсерам
    /// </summary>
    public NamedValues UserData { get { return _UserData; } }
    private NamedValues _UserData;

    /// <summary>
    /// Список парсеров, выполняющих распознаение
    /// Свойство устанавливается однократно при вызове метода ParserList.Parse()
    /// </summary>
    public ParserList Parsers
    {
      get { return _Parsers; }
      internal set { _Parsers = value; }
    }
    private ParserList _Parsers;

    /// <summary>
    /// Текущее состояние парсинга
    /// </summary>
    public ParsingState State
    {
      get { return _State; }
      internal set { _State = value; }
    }
    private ParsingState _State;

    /// <summary>
    /// Используется при рекурсивном вызове ParserList.CreateSubExpression()
    /// </summary>
    internal string[] EndTokens;


    /// <summary>
    /// Поиск всех выражений, входящих в заданное выражение.
    /// Текущее выражение также добавляется в список.
    /// Выполняется рекурсивный вызов IExpression.GetChildExpressions()
    /// </summary>
    /// <param name="expression">Выражение</param>
    /// <param name="expressions">Список для заполнения</param>
    public static void GetExpressions(IExpression expression, List<IExpression> expressions)
    {
#if DEBUG
      if (expressions == null)
        throw new ArgumentNullException("expressions");
#endif

      if (expression == null)
        return;
      if (expressions.Contains(expression))
        return;

      expressions.Add(expression);
      DoGetChildExpressions(expression, expressions);
    }

    private static void DoGetChildExpressions(IExpression expression, List<IExpression> expressions)
    {
      int First = expressions.Count;
      expression.GetChildExpressions(expressions);
      int Last = expressions.Count - 1;
      for (int i = First; i <= Last; i++)
      {
        if (expressions[i] == null)
          throw new NullReferenceException("Для выражения " + expression.ToString() + " метод GetChildExpressions() добавил в список значение null");
        DoGetChildExpressions(expressions[i], expressions);
      }
    }

    /// <summary>
    /// Возвращает список всех выражений, входящих в заданное выражение.
    /// Текущее выражение также добавляется в список
    /// Выполняется рекурсивный вызов IExpression.GetChildExpressions()
    /// </summary>
    /// <param name="expression">Выражение</param>
    /// <returns>Массив найденных объектов</returns>
    public static IExpression[] GetExpressions(IExpression expression)
    {
      List<IExpression> Expressions = new List<IExpression>();
      GetExpressions(expression, Expressions);
      return Expressions.ToArray();
    }

    /// <summary>
    /// Поиск всех выражений заданного типа, входящих в заданное выражение.
    /// Текущее выражение также добавляется в список
    /// Выполняется рекурсивный вызов IExpression.GetChildExpressions()
    /// </summary>
    /// <typeparam name="T">Тип выражений для поиска</typeparam>
    /// <param name="expression">Выражение</param>
    /// <param name="expressions">Список для добавления найденных выражений</param>
    public static void GetExpressions<T>(IExpression expression, List<T> expressions)
      where T : IExpression
    {
#if DEBUG
      if (expressions == null)
        throw new ArgumentNullException("expressions");
#endif

      if (expression == null)
        return;

      if (expression is T)
      {
        if (expressions.Contains((T)expression))
          return;

        expressions.Add((T)expression);
      }

      DoGetChildExpressions<T>(expression, expressions);
    }

    private static void DoGetChildExpressions<T>(IExpression expression, List<T> expressions)
      where T : IExpression
    {
      //int First = expressions.Count;
      List<IExpression> Exprs2 = new List<IExpression>();
      expression.GetChildExpressions(Exprs2);
      for (int i = 0; i < Exprs2.Count; i++)
      {
        if (Exprs2[i] == null)
          throw new NullReferenceException("Для выражения " + expression.ToString() + " метод GetChildExpressions() добавил в список значение null");
        GetExpressions<T>(Exprs2[i], expressions);
      }
    }

    /// <summary>
    /// Поиск всех выражений заданного типа, входящих в заданное выражение.
    /// Текущее выражение также добавляется в список
    /// Выполняется рекурсивный вызов IExpression.GetChildExpressions().
    /// </summary>
    /// <typeparam name="T">Тип выражений для поиска</typeparam>
    /// <param name="expression">Выражение</param>
    /// <returns>Массив найденных выражений</returns>
    public static T[] GetExpressions<T>(IExpression expression)
      where T : IExpression
    {
      List<T> Expressions = new List<T>();
      GetExpressions<T>(expression, Expressions);
      return Expressions.ToArray();
    }

    #endregion
  }

  /// <summary>
  /// Данные о лексеме с ошибкой, присоединяемые к полю ErrorMessageItem.Tag, в методе GetErrorMessages
  /// Класс "однократной записи"
  /// </summary>
  [Serializable]
  public class ParsingErrorItemData
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="tokenIndex">Индекс лексемы в массиве ParsingData.Tokens</param>
    /// <param name="tokenType">Тип лексемы</param>
    /// <param name="start">Начало объекта в тексте Data.Text</param>
    /// <param name="length">Длина лексемы</param>
    public ParsingErrorItemData(int tokenIndex, string tokenType, int start, int length)
    {
      _TokenIndex = tokenIndex;
      _TokenType = tokenType;
      _Start = start;
      _Length = length;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Индекс лексемы в массиве ParsingData.Tokens
    /// </summary>
    public int TokenIndex { get { return _TokenIndex; } }
    private readonly int _TokenIndex;

    /// <summary>
    /// Тип лексемы
    /// </summary>
    public string TokenType { get { return _TokenType; } }
    private readonly string _TokenType;

    /// <summary>
    /// Начало объекта в тексте Data.Text
    /// </summary>
    public int Start { get { return _Start; } }
    private readonly int _Start;

    /// <summary>
    /// Длина лексемы
    /// Допускаются лексемы нулевой длины
    /// </summary>
    public int Length { get { return _Length; } }
    private readonly int _Length;

    #endregion
  }

  // Обратная операция - синтез текстового выражения по имеющемуся вычислителю

  /// <summary>
  /// Одна лексема при синтезе выражения
  /// </summary>
  public class SynthesisToken
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="data">Ссылка на объект заполняемых данных</param>
    /// <param name="expression">Объект-вычислитель, создавший лексему</param>
    /// <param name="tokenType">Тип лексемы</param>
    /// <param name="text">Текст лексемы</param>
    /// <param name="auxData">Дополнительные данные</param>
    public SynthesisToken(SynthesisData data, IExpression expression, string tokenType, string text, object auxData)
    {
      if (data == null)
        throw new ArgumentNullException("data");
      if (expression == null)
        throw new ArgumentNullException("expression");
      if (String.IsNullOrEmpty(tokenType))
        throw new ArgumentNullException("tokenType");
      if (text == null) // пустая строка может быть
        throw new ArgumentNullException("text");

      _Data = data;
      _Expression = expression;
      _TokenType = tokenType;
      _Text = text;
      _AuxData = auxData;
    }

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="data">Ссылка на объект заполняемых данных</param>
    /// <param name="expression">Объект-вычислитель, создавший лексему</param>
    /// <param name="tokenType">Тип лексемы</param>
    /// <param name="text">Текст лексемы</param>
    public SynthesisToken(SynthesisData data, IExpression expression, string tokenType, string text)
      : this(data, expression, tokenType, text, null)
    {
    }

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="data">Ссылка на объект заполняемых данных</param>
    /// <param name="expression">Объект-вычислитель, создавший лексему</param>
    /// <param name="text">Тип и текст лексемы</param>
    public SynthesisToken(SynthesisData data, IExpression expression, string text)
      : this(data, expression, text, text, null)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Ссылка на объект заполняемых данных
    /// </summary>
    public SynthesisData Data { get { return _Data; } }
    private readonly SynthesisData _Data;

    /// <summary>
    /// Объект-вычислитель, создавший лексему
    /// </summary>
    public IExpression Expression { get { return _Expression; } }
    private readonly IExpression _Expression;

    /// <summary>
    /// Тип лексемы
    /// </summary>
    public string TokenType { get { return _TokenType; } }
    private readonly string _TokenType;

    /// <summary>
    /// Текст лексемы (собственно, цель разбора)
    /// </summary>
    public string Text { get { return _Text; } }
    private readonly string _Text;

    /// <summary>
    /// Начальная позиция лексемы (если она была добавлена)
    /// Текущая реализация - медленная
    /// </summary>
    public int Start
    {
      get
      {
        int p = 0;
        for (int i = 0; i < Data.Tokens.Count; i++)
        {
          if (Data.Tokens[i] == this)
            return p;
          p += Data.Tokens[i].Length;
        }

        return -1; // лексема еще не добавлена
      }
    }

    /// <summary>
    /// Длина лексемы
    /// </summary>
    public int Length { get { return _Text.Length; } }


    /// <summary>
    /// Дополнительные данные (нигде не используются)
    /// </summary>
    public object AuxData { get { return _AuxData; } }
    private readonly object _AuxData;

    #endregion

    #region ToString()

    /// <summary>
    /// Реализация аналогична Token.ToString()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (Data == null)
        return "Empty";

      StringBuilder sb = new StringBuilder();
      sb.Append(TokenType);
      sb.Append(" (");
      sb.Append(Start.ToString());
      sb.Append(", ");
      sb.Append(Length.ToString());
      sb.Append(")");

      if (Length > 0)
      {
        sb.Append(" \"");
        sb.Append(Text.ToString());
        sb.Append("\"");
      }
      else
        sb.Append(" (EmptyText)");

      return sb.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Синтезированное выражение
  /// После создания объекта SynthesisData следует вызвать IExpression.Synthesise().
  /// Для получения текста используется SynthesisData.ToString()
  /// </summary>
  public class SynthesisData
  {
    #region Конструктор

    /// <summary>
    /// Создает объекты
    /// </summary>
    public SynthesisData()
    {
      _Tokens = new List<SynthesisToken>();
      _UseFormulas = true;
      _UserData = new NamedValues();
      _CultureInfo = CultureInfo.CurrentCulture;
    }

    #endregion

    #region Управляющие свойства

    /// <summary>
    /// Объект для форматирования числовых констант
    /// </summary>
    public CultureInfo CultureInfo
    {
      get { return _CultureInfo; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _CultureInfo = value;
      }
    }
    private CultureInfo _CultureInfo;

    /// <summary>
    /// Разделитель элементов списка.
    /// Возвращает CultureInfo.TextInfo.ListSeparator
    /// </summary>
    public string ListSeparator { get { return _CultureInfo.TextInfo.ListSeparator; } }

    /// <summary>
    /// Требуется ли использовать при синтезе формулы (true) или значения (false)
    /// По умолчанию - true (формулы) 
    /// Свойство влияет только на те IExpression, которые извлекают внешние данные, например, значения 
    /// ячейки электронной таблицы. То есть, если UseFormulas=true, то будет возвращено имя ячейки "A1",
    /// а если UseFormulas=false, то будет возвращено значение ячейки "123"
    /// Свойство не влияет на вычислители функций и математических выражений, а также на константы
    /// </summary>
    public bool UseFormulas
    {
      get { return _UseFormulas; }
      set { _UseFormulas = value; }
    }
    private bool _UseFormulas;

    /// <summary>
    /// Если свойство установено в true, то вокруг математических операций будут добавлены пробелы,
    /// то есть будет "1 + 2". Если свойство не установлено (по умолчанию), то пробелы не добавляются:
    /// "1+2"
    /// </summary>
    public bool UseSpaces
    {
      get { return _UseSpaces; }
      set { _UseSpaces = value; }
    }
    private bool _UseSpaces;



    /// <summary>
    /// Произвольные пользовательские данные, которые могут потребоваться парсерам
    /// </summary>
    public NamedValues UserData { get { return _UserData; } }
    private readonly NamedValues _UserData;

    #endregion

    #region Список лексем

    /// <summary>
    /// Список лексем, заполняемый при вызове IExpression.Synthesize()
    /// </summary>
    public List<SynthesisToken> Tokens { get { return _Tokens; } }
    private readonly List<SynthesisToken> _Tokens;

    /// <summary>
    /// Возвращает лексему для заданного смещения текста
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public SynthesisToken GetTokenAtPosition(int pos)
    {
      // Не выгодно использовать свойство SynthesisToken.Start, т.к. оно медленно работает
      int CurrOff = 0;

      for (int i = 0; i < Tokens.Count; i++)
      {
        if (pos >= CurrOff && pos < (CurrOff + Tokens[i].Length))
          return Tokens[i];

        CurrOff += Tokens[i].Length;
      }

      return null;
    }

    #endregion

    #region Получение текста

    /// <summary>
    /// Возвращает объединенный текст для всех лексем
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < _Tokens.Count; i++)
        sb.Append(_Tokens[i].Text);

      return sb.ToString();
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Возвращает текстовое представление для значения.
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>Текстовое представление</returns>
    public string CreateValueText(object value)
    {
      if (value == null)
        return "null"; // ??

      if (value is DateTime)
      {
        DateTime dt = (DateTime)value;
        if (dt.TimeOfDay.Ticks == 0)
          return dt.ToString("d", CultureInfo);
        else
          return dt.ToString("g", CultureInfo);
      }

      return Convert.ToString(value, CultureInfo);
    }

    #endregion
  }


  /// <summary>
  /// Исключение, выбрасываемое методом ParserList.CreateExpression()
  /// </summary>
  [Serializable]
  public class ParserCreateExpressionException : ApplicationException
  {
    #region Конструктор

    /// <summary>
    /// Создает объект исключения с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public ParserCreateExpressionException(string message)
      : this(message, null)
    {
    }


    /// <summary>
    /// Создает объект исключения с заданным сообщением и вложенным исключением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    /// <param name="innerException">Вложенное исключение</param>
    public ParserCreateExpressionException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected ParserCreateExpressionException(System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

}
