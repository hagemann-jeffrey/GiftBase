using System.Buffers.Text;
using System.Security.Cryptography;

namespace GiftBase.Shared.Common;

public static class TokenGenerator
{
    public static string Generate() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
}
