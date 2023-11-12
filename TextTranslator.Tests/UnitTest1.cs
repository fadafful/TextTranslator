// Your test class in YourProject.Tests namespace

using Xunit;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        // Arrange
        var value = 2;
        var expected = 4;

        // Act
        var result = value * 2;

        // Assert
        Assert.Equal(expected, result);
    }
}

