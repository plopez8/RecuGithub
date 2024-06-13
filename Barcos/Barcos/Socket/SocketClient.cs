using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Barcos.WPF;

namespace Barcos.Socket
{
    public class SocketClient
    {
        private TcpClient client;
        private NetworkStream stream;
        private GameWindow gameWindow;

        public SocketClient(GameWindow gameWindow)
        {
            this.gameWindow = gameWindow;
        }

        public async Task Connect(string ip, int port, string username, string gameid)
        {
            client = new TcpClient();
            await client.ConnectAsync(ip, port);
            stream = client.GetStream();
            if (gameid == null)
            {
                gameid = "new";
            }
                await Send($"LOADGAME:{gameid}");
            _ = Task.Run(ReceiveMessages);
            await Task.Delay(1000);
            await Send($"USERNAME:{username}");
            _ = Task.Run(ReceiveMessages);
        }

        public async Task Send(string message)
        {
            var data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);
        }

        private async Task ReceiveMessages()
        {
            var buffer = new byte[1024];

            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                gameWindow.ProcessServerResponse(message);
            }
        }
    }
}
