using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using NUnit.Framework;

namespace FreeLibSet.Tests
{

  /// <summary>
  /// Базовый класс для тестов, в которых нужно использовать однократную инициализацию/завершение
  /// В MonoDevelop (версия 5.10) почему-то не работают атрибуты [OneTimeSetUp] / [OneTimeTearDown]
  /// Можно использовать этот класс в качестве базового, чтобы обойтись без атрибутов [SetUp], [TearDown], [OneTimeSetUp], [OneTimeTearDown]
  /// </summary>
  public abstract /* Реально нет абстрактных методов */ class FixtureWithSetUp:DisposableObject
  {
    #region Конструктор и Dispose()

    protected FixtureWithSetUp()
    {
    }

    protected override void Dispose(bool disposing)
    {
      if (_OneTimeSetUpSimulated)
      {
        _OneTimeSetUpSimulated = false;
        OnOneTimeTearDown();
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Методы с атрибутами

    [SetUp]
    public void InternalSetUp()
    {
      if (!_OneTimeSetUpCalled)
      {
        if (!_OneTimeSetUpSimulated)
        {
          _OneTimeSetUpSimulated = true;
          OnOneTimeSetUp();
        }
      }
      OnSetUp();
    }

    [TearDown]
    public void InternalTearDown()
    {
      OnTearDown();
    }

    private bool _OneTimeSetUpCalled;
    private bool _OneTimeSetUpSimulated;

    [OneTimeSetUp]
    public void InternalOneTimeSetUp()
    {
      _OneTimeSetUpCalled = true;
      OnOneTimeSetUp();
    }

    [OneTimeTearDown]
    public void InternalOneTimeTearDown()
    {
      OnOneTimeTearDown();
    }

    #endregion

    #region Виртуальные методы

    protected virtual void OnSetUp() { }

    protected virtual void OnTearDown() { }

    protected virtual void OnOneTimeSetUp() { }

    protected virtual void OnOneTimeTearDown() { }

    #endregion
  }
}
