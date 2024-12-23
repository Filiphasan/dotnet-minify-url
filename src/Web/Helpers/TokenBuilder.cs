using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Web.Helpers;

public class TokenBuilder
{
    private const string Charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int CharsetLength = 62;

    private long _epoch;
    private int _additionalCharLength = 2;

    public TokenBuilder WithEpoch(string epochDate)
    {
        _epoch = DateTimeOffset.Parse(epochDate, new DateTimeFormatInfo
        {
            FullDateTimePattern = "yyyy-MM-dd"
        }).ToUnixTimeMilliseconds();
        return this;
    }

    public TokenBuilder WithAdditionalCharLength(int length)
    {
        _additionalCharLength = length;
        return this;
    }

    public string Build()
    {
        var epochToken = GenerateTokenFromEpoch();
        var additionalToken = GenerateToken();
        return $"{epochToken}{additionalToken}";
    }

    private string GenerateTokenFromEpoch()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _epoch;
        var token = new StringBuilder();

        while (timestamp > CharsetLength)
        {
            token.Append(Charset[(int)(timestamp % CharsetLength)]);
            timestamp /= CharsetLength;
        }

        return token.ToString();
    }

    private string GenerateToken()
    {
        if (_additionalCharLength <= 0)
        {
            return string.Empty;
        }

        var token = new StringBuilder();
        for (var i = 0; i < _additionalCharLength; i++)
        {
            token.Append(Charset[RandomNumberGenerator.GetInt32(0, CharsetLength)]);
        }

        return token.ToString();
    }
}