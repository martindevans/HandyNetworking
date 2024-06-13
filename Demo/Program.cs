using System.Text;
using Demo;
using HandyNetworking;
using HandyNetworking.LiteNetLibBackend;
using HandyNetworking.Logging;
using Spectre.Console;

Console.WriteLine("Hello, World!");

var server = AnsiConsole.Confirm("Run As Server?");
var port = AnsiConsole.Ask<ushort>("Port", 61401);
var password = AnsiConsole.Ask<string>("What's the password?", "password");

var manager = new NetworkManager<Backend, int>(null, new Backend(new ConsoleLogger()));

if (server)
{
    manager.StartServer(port, new PasswordFilter(password));
}
else
{
    var address = AnsiConsole.Ask<string>("Server Address", "localhost");
    manager.StartClient(address, port, Encoding.UTF8.GetBytes(password));
}

manager.Subscribe<KeyCharPacket>((sender, pkt) =>
{
    Console.ForegroundColor = (ConsoleColor)(sender.Value % 12 + 1);
    Console.Write(pkt.Character);
});

AnsiConsole.MarkupLine("[green]Connecting...[/]");
while (manager.Status != ConnectionStatus.Connected)
    manager.Update();
AnsiConsole.MarkupLine("[green]Connected![/]");

while (true)
{
    manager.Update();
    Console.Title = $"Status:{manager.Status} Peers:{manager.Peers.Count} Rcv:{manager.Statistics.BytesReceived}B Send:{manager.Statistics.BytesSent}B";

    if (Console.KeyAvailable)
    {
        Console.ForegroundColor = ConsoleColor.White;
        var key = Console.ReadKey(false);

        foreach (var peer in manager.Peers)
            manager.Send(peer.Id, new KeyCharPacket(key.KeyChar), 0, PacketReliability.ReliableOrdered);
    }
}