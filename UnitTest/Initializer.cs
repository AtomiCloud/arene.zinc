using CarboxylicLithiumTestHelper;
using FluentAssertions;
using FluentAssertions.Extensibility;
using UnitTest;

[assembly: AssertionEngineInitializer(typeof(Initializer), nameof(Initializer.Initialize))]

namespace UnitTest;

public static class Initializer
{
  public static void Initialize()
  {
    AssertionConfiguration.Current.Equivalency.Modify(o =>
      o.Using(new ResultEquivalencyStep())
    );
  }
}
