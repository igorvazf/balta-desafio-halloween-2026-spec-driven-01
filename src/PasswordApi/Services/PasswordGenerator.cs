using System.Security.Cryptography;

namespace PasswordApi.Services;

public class PasswordGenerator : IPasswordGenerator
{
    public const int MinLength = 16;
    public const int MaxLength = 128;

    public const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    public const string Digits = "0123456789";
    public const string Specials = "!@#$%^&*()-_=+[]{}<>?";

    private const string AllCharacters = Uppercase + Lowercase + Digits + Specials;

    public string Generate()
    {
        var length = RandomNumberGenerator.GetInt32(MinLength, MaxLength + 1);
        var password = new char[length];

        password[0] = PickFrom(Uppercase);
        password[1] = PickFrom(Lowercase);
        password[2] = PickFrom(Digits);
        password[3] = PickFrom(Specials);

        for (var i = 4; i < length; i++)
        {
            password[i] = PickFrom(AllCharacters);
        }

        Shuffle(password);

        return new string(password);
    }

    private static char PickFrom(string characters) =>
        characters[RandomNumberGenerator.GetInt32(characters.Length)];

    private static void Shuffle(char[] characters)
    {
        for (var i = characters.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (characters[i], characters[j]) = (characters[j], characters[i]);
        }
    }
}
