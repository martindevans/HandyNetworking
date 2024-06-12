using System.Text;
using HandyNetworking;

namespace Demo;

public class PasswordFilter
    : IConnectionFilter
{
    private readonly string _password;

    public PasswordFilter(string password)
    {
        _password = password;
    }

    public bool AcceptConnection(ReadOnlyMemory<byte> packet)
    {
        var password = Encoding.UTF8.GetString(packet.Span);
        return _password == password;
    }
}