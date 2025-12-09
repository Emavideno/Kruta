using Kruta.Server.Networking;

Console.Title = "Kruta Server";

var server = new GameServer();
await server.StartAsync();