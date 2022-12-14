using Blazy.Sockets.Network;

namespace Blazy.Sockets.Encoding;

public static class JsonEncoderExtension
{
    public static void UseDefaultEncoder(this NetworkBuilder builder)
    {
        builder.Use<IEncoder, JsonEncoder>();
    }
}