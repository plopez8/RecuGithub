using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Documents;
using Barcos.Models;
using Barcos.Services;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Barcos.Socket
{
    public class SocketServer
    {
        private TcpListener listener;
        private List<(TcpClient client, string username, int hits)> players;
        private List<string> gameids;
        private Dictionary<string, Board> playerBoards;  // Store ship positions for each player
        private int currentPlayerIndex;

        public SocketServer()
        {
            players = new List<(TcpClient, string, int)>();
            playerBoards = new Dictionary<string, Board>();
            currentPlayerIndex = 0;
            gameids = new List<string>();
        }

        public async Task Start(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine("Server started...");

            while (players.Count < 2)
            {
                var client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected...");
                _ = Task.Run(() => HandleClient(client));
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            var buffer = new byte[1024];
            var stream = client.GetStream();
            string username = null;

            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    client.Close();
                    Console.WriteLine("Client disconnected...");
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Receivedd: {message}");
                Console.WriteLine($"Received2: {message}");
                var parts = message.Split(':');
                string command = parts[0];
                string data = parts.Length > 1 ? parts[1] : null;
                //Console.WriteLine($"Command: {command}, Data: {data}");
                if (command == "USERNAME")
                {
                    username = data;
                    players.Add((client, username, 0));
                    playerBoards[username] = GenerateRandomBoard();
                    Console.WriteLine($"Username set: {username}");

                    if (players.Count == 2)
                    {
                        Console.WriteLine(gameids[0]);
                        Console.WriteLine(gameids[1]);
                        if (gameids[0] != "new" && gameids[1] != "new" && gameids[0] == gameids[1])
                            {
                            Console.WriteLine("same game");
                            await LoadGame(gameids[0]);
                            await NotifyPlayers();
                            await NotifyPrepare();
                        }
                        else if(gameids[0] == gameids[1])
                        {
                            Console.WriteLine("new game");
                            await NotifyPlayers();
                        }
                        else
                        {
                            Console.WriteLine("No es el mismo codigo");
                            NotifyDisconnect();
                        }
                    }
                }
                else if (command == "MOVE")
                {
                    Console.WriteLine($"Processing MOVE command from {username}: {data}");
                    await ProcessMoveCommand(username, data);
                }
                else if (command == "SAVE")
                {
                    Console.WriteLine("else save");
                    if(players.Count == 2)
                    {
                        await SaveData(false);
                    }
                    else
                    {
                        NotifySave();
                    }
                    
                }
                else if (command == "LOADGAME")
                {
                    Console.WriteLine($"Processing LOAD {data}");
                    gameids.Add(data);
                }
            }
        }

        public async Task IncrementPlayerHits(string username, int increment)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].username == username)
                {
                    var player = players[i];
                    players[i] = (player.client, player.username, player.hits + increment);
                    break;
                }
            }
        }

        private async Task ProcessMoveCommand(string username, string data)
        {
            var parts = data.Split(',');
            int row = int.Parse(parts[0]);
            int col = int.Parse(parts[1]);

            if (playerBoards[players[1 - currentPlayerIndex].username].Cells[row, col].IsAttacked)
            {
                Console.WriteLine($"{username} attempted to attack an already attacked position ({row}, {col}).");

                // Notify the player of the invalid move
                var send = $"INVALID:{row}:{col}";
                var dataBytes2 = Encoding.UTF8.GetBytes(send);
                await players[currentPlayerIndex].client.GetStream().WriteAsync(dataBytes2, 0, dataBytes2.Length);

                return;
            }

            string result = playerBoards[players[1 - currentPlayerIndex].username].Cells[row, col].ContainsShip ? "hit" : "miss";
            playerBoards[players[1 - currentPlayerIndex].username].Cells[row, col].IsAttacked = true;
            if (result == "hit")
            {
                IncrementPlayerHits(username, 1);
            }
            Console.WriteLine($"{username} attacked position ({row}, {col}): {result}");

            // Notify the current player of the result
            string message = $"RESULT:{result}:{row}:{col}";
            var dataBytes = Encoding.UTF8.GetBytes(message);
            await players[currentPlayerIndex].client.GetStream().WriteAsync(dataBytes, 0, dataBytes.Length);

            // Notify the opponent of the move
            message = $"MOVE:{result}:{row}:{col}:{players[1 - currentPlayerIndex].username}";
            dataBytes = Encoding.UTF8.GetBytes(message);
            await players[1 - currentPlayerIndex].client.GetStream().WriteAsync(dataBytes, 0, dataBytes.Length);

            // Change turn
            Console.WriteLine($"hits {players[currentPlayerIndex].username}");
            Console.WriteLine($"hits {players[currentPlayerIndex].ToString}");
            Console.WriteLine($"hits {players[currentPlayerIndex].hits}");
            //AQUI
            if (players[currentPlayerIndex].hits == 17)
            {
                Console.WriteLine("Game over");
                NotifyWinner(username);
                SaveData(true);
                NotifySave();
            }
            else
            {

            currentPlayerIndex = 1 - currentPlayerIndex;
            await NotifyTurn();
            }
        }

        private async Task NotifyWinner(string username)
        {
            string message = $"WIN:{username}";
            var data = Encoding.UTF8.GetBytes(message);
            foreach (var player in players)
            {
                await player.client.GetStream().WriteAsync(data, 0, data.Length);
            }
        }

        private async Task NotifyTurn()
        {
            string message = $"TURN:{players[currentPlayerIndex].username}";
            var data = Encoding.UTF8.GetBytes(message);
            foreach (var player in players)
            {
                await player.client.GetStream().WriteAsync(data, 0, data.Length);
            }
            Console.WriteLine($"It's now {players[currentPlayerIndex].username}'s turn.");
        }
        private async Task NotifySave()
        {
            string message = $"SAVE:{players.Count}";
            var data = Encoding.UTF8.GetBytes(message);
            foreach (var player in players)
            {
                await player.client.GetStream().WriteAsync(data, 0, data.Length);
                player.client.Close();
            }
            Console.WriteLine($"Notify Save send");
            Reset();
        }
        private async Task NotifyDisconnect()
        {
            string message = $"DISCONNECT:X";
            var data = Encoding.UTF8.GetBytes(message);
            foreach (var player in players)
            {
                await player.client.GetStream().WriteAsync(data, 0, data.Length);
                player.client.Close();
            }
            Console.WriteLine($"Notify Save send");
            Reset();
        }
        private void Reset()
        {
            players.Clear();
            playerBoards.Clear();
            currentPlayerIndex = 0;
            gameids.Clear();
        }

        private async Task NotifyPlayers()
        {
            Console.WriteLine("Notifying players...");
            // Choose a random player to start
            Random rand = new Random();
            currentPlayerIndex = rand.Next(2);
            string startingPlayer = players[currentPlayerIndex].username;
            string message = $"PLAYERS:{players[0].username},{players[1].username}:{startingPlayer}";

            // Include boards in the message
            string player1Board = BoardToString(playerBoards[players[0].username]);
            string player2Board = BoardToString(playerBoards[players[1].username]);

            message += $":{player1Board}:{player2Board}";

            var data = Encoding.UTF8.GetBytes(message);

            foreach (var player in players)
            {
                var stream = player.client.GetStream();
                await stream.WriteAsync(data, 0, data.Length);
                Console.WriteLine($"Notified player: {player.username}");
            }
        }

        private async Task NotifyPrepare()
        {
            Console.WriteLine("Notifying players prepare...");
            // Choose a random player to start

            // Include boards in the message
            string player1Board = BoardMovesToString(playerBoards[players[0].username], players[0].username);
            string player2Board = BoardMovesToString(playerBoards[players[1].username], players[1].username);

            string message = $"PREPARE:{players[0].username}:{players[1].username}:{player1Board}:{player2Board}";
            Console.WriteLine(message);
            var data = Encoding.UTF8.GetBytes(message);

            foreach (var player in players)
            {
                var stream = player.client.GetStream();
                await stream.WriteAsync(data, 0, data.Length);
                Console.WriteLine($"Notified player: {player.username}");
            }
        }


        private Board GenerateRandomBoard()
        {
            bool[,] boolBoard = new bool[10, 10];
            int[] shipSizes = { 5, 4, 3, 3, 2 };
            Random rand = new Random();

            foreach (int size in shipSizes)
            {
                bool placed = false;
                while (!placed)
                {
                    int row = rand.Next(10);
                    int col = rand.Next(10);
                    bool horizontal = rand.Next(2) == 0;

                    if (CanPlaceShip(boolBoard, row, col, size, horizontal))
                    {
                        PlaceShip(boolBoard, row, col, size, horizontal);
                        placed = true;
                    }
                }
            }
            return ConvertToBoard(boolBoard);
        }

        private bool CanPlaceShip(bool[,] board, int row, int col, int size, bool horizontal)
        {
            if (horizontal)
            {
                if (col + size > 10) return false;
                for (int i = 0; i < size; i++)
                {
                    if (board[row, col + i]) return false;
                }
            }
            else
            {
                if (row + size > 10) return false;
                for (int i = 0; i < size; i++)
                {
                    if (board[row + i, col]) return false;
                }
            }
            return true;
        }

        private void PlaceShip(bool[,] board, int row, int col, int size, bool horizontal)
        {
            if (horizontal)
            {
                for (int i = 0; i < size; i++)
                {
                    board[row, col + i] = true;
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    board[row + i, col] = true;
                }
            }
        }

        string BoardToString(Board board)
        {
            StringBuilder sb = new StringBuilder();
            int size = board.Cells.GetLength(0); // Assuming the board is square
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    sb.Append(board.Cells[i, j].ContainsShip ? "1" : "0");
                }
                sb.Append("|");
            }
            return sb.ToString();
        }
        string BoardMovesToString(Board board, string username)
        {
            StringBuilder sb = new StringBuilder();
            int size = board.Cells.GetLength(0); // Assuming the board is square
            int total = 0;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (!board.Cells[i, j].IsAttacked)
                    {
                        sb.Append("o");
                    }
                    else if (board.Cells[i, j].IsAttacked && !board.Cells[i, j].ContainsShip)
                    {
                        sb.Append("m");
                    }
                    else if (board.Cells[i, j].IsAttacked && board.Cells[i, j].ContainsShip)
                    {
                        total++;
                        sb.Append("h");
                    }
                }
                sb.Append("|");
            }
            IncrementPlayerHits(username, total);
            return sb.ToString();
        }
        public async Task SaveData(bool finished)
        {
            var gameService = new GameService();

            // Convert boolean boards to Board objects
            Console.WriteLine("Saving game data...");
            Console.WriteLine("Player boards:");
            foreach (var playerBoard in this.playerBoards)
            {
                Console.WriteLine(playerBoard);
            }
            //var playerBoards = this.playerBoards.Select(board => ConvertToBoard(board)).ToList();
            var player1 = new Player
            {
                Username = playerBoards.Keys.ElementAt(0),
                Board = playerBoards.Values.ElementAt(0)
            };
            var player2 = new Player
            {
                Username = playerBoards.Keys.ElementAt(1),
                Board = playerBoards.Values.ElementAt(1)
            };

            var game = new Game
            {
                Player1 = player1,
                Player2 = player2,
                CurrentPlayer = this.players[this.currentPlayerIndex].username,
                IsFinished = finished
            };

            // Save the game
            await gameService.SaveGame(game);
            this.NotifySave();
        }

        private Board ConvertToBoard(bool[,] boolBoard)
        {
            var board = new Board
            {
                Cells = new Cell[10, 10]
            };

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    board.Cells[i, j] = new Cell
                    {
                        ContainsShip = boolBoard[i, j],
                        IsAttacked = false // Assuming the board is not attacked initially
                    };
                }
            }

            return board;
        }
        public async Task LoadGame(string gameId)
        {
            var gameService = new GameService();
            var gameDoc = await gameService.GetGameById(gameId);
            Game game = BsonSerializer.Deserialize<Game>(gameDoc["game"].AsBsonDocument);
            playerBoards.Clear();
            if (game != null)
            {
                playerBoards.Add(game.Player1.Username, game.Player1.Board);
                playerBoards.Add(game.Player2.Username, game.Player2.Board);
            }
            else
            {
                Console.WriteLine($"Game with ID {gameId} not found.");
            }
        }
    }
}

