using Blazy.Sockets.Network;

namespace Blazy.Sockets.Encoding;

public static class JsonEncoderExtension
{
    public static NetworkBuilder UseDefaultEncoder(this NetworkBuilder builder)
    {
        return builder.Use<IEncoder, JsonEncoder>();
    }
}