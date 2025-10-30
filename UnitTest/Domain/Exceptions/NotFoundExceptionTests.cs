using Domain.Exceptions;
using FluentAssertions;

namespace UnitTest.Domain.Exceptions;

public class NotFoundExceptionTests
{
  // ========================================
  // Constructor with Type and Identifier Tests
  // ========================================

  [Theory]
  [InlineData(typeof(string), "user-123")]
  [InlineData(typeof(int), "project-456")]
  [InlineData(typeof(DateTime), "order-789")]
  public void Constructor_WithTypeAndIdentifier_ShouldSetProperties(Type type, string identifier)
  {
    // Arrange & Act
    var exception = new NotFoundException(type, identifier);

    // Assert
    exception.Type.Should().Be(type);
    exception.RequestIdentifier.Should().Be(identifier);
    exception.Message.Should().NotBeNullOrEmpty();
  }

  // ========================================
  // Constructor with Message, Type and Identifier Tests
  // ========================================

  [Theory]
  [MemberData(nameof(ConstructorWithMessageTestData))]
  public void Constructor_WithMessageTypeAndIdentifier_ShouldSetAllProperties(
    string message,
    Type type,
    string identifier)
  {
    // Arrange & Act
    var exception = new NotFoundException(message, type, identifier);

    // Assert
    exception.Message.Should().Be(message);
    exception.Type.Should().Be(type);
    exception.RequestIdentifier.Should().Be(identifier);
  }

  public static IEnumerable<object[]> ConstructorWithMessageTestData()
  {
    yield return new object[] { "User not found", typeof(string), "user-abc" };
    yield return new object[] { "Project does not exist", typeof(int), "project-123" };
    yield return new object[] { "Resource missing", typeof(Guid), "resource-xyz" };
  }

  // ========================================
  // Constructor with Message, InnerException, Type and Identifier Tests
  // ========================================

  [Theory]
  [MemberData(nameof(ConstructorWithInnerExceptionTestData))]
  public void Constructor_WithMessageInnerExceptionTypeAndIdentifier_ShouldSetAllProperties(
    string message,
    Exception innerException,
    Type type,
    string identifier)
  {
    // Arrange & Act
    var exception = new NotFoundException(message, innerException, type, identifier);

    // Assert
    exception.Message.Should().Be(message);
    exception.InnerException.Should().Be(innerException);
    exception.Type.Should().Be(type);
    exception.RequestIdentifier.Should().Be(identifier);
  }

  public static IEnumerable<object[]> ConstructorWithInnerExceptionTestData()
  {
    yield return new object[]
    {
      "Database query failed",
      new InvalidOperationException("Connection timeout"),
      typeof(string),
      "user-001"
    };

    yield return new object[]
    {
      "API call failed",
      new HttpRequestException("404 Not Found"),
      typeof(int),
      "resource-002"
    };

    yield return new object[]
    {
      "Cache miss",
      new KeyNotFoundException("Key not in cache"),
      typeof(Guid),
      "cache-003"
    };
  }

  // ========================================
  // Null Message Tests
  // ========================================

  [Theory]
  [InlineData(typeof(string), "id-null-1")]
  [InlineData(typeof(int), "id-null-2")]
  [InlineData(typeof(Guid), "id-null-3")]
  public void Constructor_WithNullMessage_ShouldHandleGracefully(Type type, string identifier)
  {
    // Arrange & Act
    var exception = new NotFoundException(null, type, identifier);

    // Assert
    exception.Type.Should().Be(type);
    exception.RequestIdentifier.Should().Be(identifier);
  }

  [Theory]
  [InlineData(typeof(string), "id-null-inner-1")]
  [InlineData(typeof(int), "id-null-inner-2")]
  [InlineData(typeof(DateTime), "id-null-inner-3")]
  public void Constructor_WithNullMessageAndInnerException_ShouldHandleGracefully(Type type, string identifier)
  {
    // Arrange
    var innerException = new Exception("Inner exception");

    // Act
    var exception = new NotFoundException(null, innerException, type, identifier);

    // Assert
    exception.InnerException.Should().Be(innerException);
    exception.Type.Should().Be(type);
    exception.RequestIdentifier.Should().Be(identifier);
  }

  [Theory]
  [InlineData(typeof(string), "id-null-both-1")]
  [InlineData(typeof(int), "id-null-both-2")]
  [InlineData(typeof(Guid), "id-null-both-3")]
  public void Constructor_WithNullInnerException_ShouldHandleGracefully(Type type, string identifier)
  {
    // Arrange & Act
    var exception = new NotFoundException("Message", null, type, identifier);

    // Assert
    exception.Message.Should().Be("Message");
    exception.InnerException.Should().BeNull();
    exception.Type.Should().Be(type);
    exception.RequestIdentifier.Should().Be(identifier);
  }
}
