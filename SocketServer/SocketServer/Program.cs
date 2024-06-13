using System;
using System.Threading.Tasks;
using Barcos.Socket;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new SocketServer();
        await server.Start(12345); // Reemplaza con el puerto deseado
    }
}
