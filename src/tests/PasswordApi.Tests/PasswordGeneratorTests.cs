using PasswordApi.Services;

namespace PasswordApi.Tests;

public class PasswordGeneratorTests
{
    private const int Iterations = 10_000;
    private const string AllowedSpecials = "!@#$%^&*()-_=+[]{}<>?";

    private static readonly IReadOnlyList<string> Passwords = GeneratePasswords();

    private static List<string> GeneratePasswords()
    {
        var generator = new PasswordGenerator();
        return Enumerable.Range(0, Iterations).Select(_ => generator.Generate()).ToList();
    }

    [Fact]
    public void Generate_ReturnsPasswordWithAtLeast16Characters()
    {
        Assert.All(Passwords, password => Assert.True(password.Length >= 16, $"Length {password.Length}: {password}"));
    }

    [Fact]
    public void Generate_ReturnsPasswordWithAtMost128Characters()
    {
        Assert.All(Passwords, password => Assert.True(password.Length <= 128, $"Length {password.Length}: {password}"));
    }

    [Fact]
    public void Generate_ReachesBothLengthBounds()
    {
        Assert.Contains(Passwords, password => password.Length == 16);
        Assert.Contains(Passwords, password => password.Length == 128);
    }

    [Fact]
    public void Generate_ReturnsPasswordWithUppercaseLetter()
    {
        Assert.All(Passwords, password => Assert.Contains(password, char.IsAsciiLetterUpper));
    }

    [Fact]
    public void Generate_ReturnsPasswordWithLowercaseLetter()
    {
        Assert.All(Passwords, password => Assert.Contains(password, char.IsAsciiLetterLower));
    }

    [Fact]
    public void Generate_ReturnsPasswordWithDigit()
    {
        Assert.All(Passwords, password => Assert.Contains(password, char.IsAsciiDigit));
    }

    [Fact]
    public void Generate_ReturnsPasswordWithSpecialCharacter()
    {
        Assert.All(Passwords, password => Assert.Contains(password, c => AllowedSpecials.Contains(c)));
    }

    [Fact]
    public void Generate_ReturnsPasswordWithoutWhitespace()
    {
        Assert.All(Passwords, password => Assert.DoesNotContain(password, char.IsWhiteSpace));
    }

    [Fact]
    public void Generate_ReturnsPasswordWithOnlyAllowedCharacters()
    {
        Assert.All(Passwords, password => Assert.All(password, c =>
            Assert.True(char.IsAsciiLetterOrDigit(c) || AllowedSpecials.Contains(c), $"Unexpected character '{c}' in {password}")));
    }
}
