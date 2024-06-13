using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Barcos.Socket;
using Barcos.Controls;
using Barcos.Models;

namespace Barcos.WPF
{
    /// <summary>
    /// Lógica de interacción para GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        private SocketClient socketClient;
        private string username;
        private string startingPlayer;
        private bool[,] playerBoard;
        private bool[,] enemyBoard;
        private string[,] playerMovesB;
        private string[,] enemyMovesB;

        private string currentPlayer;
        private string gameid;



        public GameWindow(string username, string gameid = null)
        {
            InitializeComponent();
            this.username = username;
            this.gameid = gameid;
            MessageBox.Show($"Welcome, {gameid}!");
            this.playerBoard = new bool[10, 10];
            this.enemyBoard = new bool[10, 10];
            InitializeGame();
            PlayerBoard.CellClicked += OnCellClicked;
        }
        private async void OnCellClicked(int row, int col)
        {
            if (IsPlayerTurn)
            {
                string message = $"MOVE:{row},{col}";
                await socketClient.Send(message);
            }
            else
            {
                MessageBox.Show("No es tu turno.");
            }
        }



        private void InitializeGame()
        {
            socketClient = new SocketClient(this);
            ConnectToServer();
        }

        private async void ConnectToServer()
        {
            await socketClient.Connect("127.0.0.1", 12345, username, gameid); // Reemplazar con la IP y el puerto reales
            //if (gameid != null)
            //{
            //    await Task.Delay(1000);
            //    await socketClient.Send($"LOADGAME:{gameid}");
            //}
        }

        public void ProcessServerResponse(string response)
        {
            var parts = response.Split(':');
            string command = parts[0];
            string data = parts.Length > 1 ? parts[1] : null;
            string startingPlayer = parts.Length > 2 ? parts[2] : null;
            string player1Board = parts.Length > 3 ? parts[3] : null;
            string player2Board = parts.Length > 4 ? parts[4] : null;

            if (command == "PLAYERS")
            {
                this.startingPlayer = startingPlayer;
                currentPlayer = startingPlayer; // Set the initial turn

                // Convert board strings to boolean arrays
                bool[,] board1 = StringToBoard(player1Board);
                bool[,] board2 = StringToBoard(player2Board);

                if (username == data.Split(',')[0])
                {
                    playerBoard = board2;
                    enemyBoard = board1;
                }
                else
                {
                    playerBoard = board1;
                    enemyBoard = board2;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    EnemyBoard.UpdateBoard(enemyBoard, "ship");
                });
            }
            else if (command == "TURN")
            {
                currentPlayer = data;
            }
            else if (command == "RESULT")
            {
                string result = parts[1];
                int row = int.Parse(parts[2]);
                int col = int.Parse(parts[3]);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (result == "hit")
                    {
                        PlayerBoard.UpdateCell(row, col, true);
                    }
                    else if (result == "miss")
                    {
                        PlayerBoard.UpdateCell(row, col, false);
                    }
                });
            }
            else if (command == "MOVE")
            {
                string result = parts[1];
                int row = int.Parse(parts[2]);
                int col = int.Parse(parts[3]);
                string nextPlayer = parts[4];

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (result == "hit")
                    {
                        EnemyBoard.UpdateCell(row, col, true);
                    }
                    else if (result == "miss")
                    {
                        EnemyBoard.UpdateCell(row, col, false);
                    }

                    currentPlayer = nextPlayer; // Update the current player to the next one
                });
            }
            else if (command == "SAVE")
            {
                string result = parts[1];
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (result == "2")
                    {
                        MessageBox.Show("Game saved successfully.");
                        this.Menu();
                    }
                    else
                    {
                    MessageBox.Show("No game to save");
                    this.Menu();
                    }
                });

            }
            else if (command == "DISCONNECT")
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                        MessageBox.Show("Desconectado de la partida");
                        this.Menu();
                });

            }
            else if (command == "PREPARE")
            {
                string user1 = parts[1];
                string user2 = parts[2];
                string user1b = parts[3];
                string user2b = parts[4];
                if (username == user1)
                {
                    playerMovesB = MovesStringToBoard(user1b);
                    enemyMovesB = MovesStringToBoard(user2b);
                }
                else
                {
                    playerMovesB = MovesStringToBoard(user2b);
                    enemyMovesB = MovesStringToBoard(user1b);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    SetMoves(PlayerBoard, enemyMovesB);
                    SetMoves(EnemyBoard, playerMovesB);
                });
            }
            else if (command == "WIN")
            {
                string winner = parts[1];
                if(winner == username)
                {
                    MessageBox.Show("¡Felicidades! Has ganado.");
                }
                else
                {
                    MessageBox.Show("¡Has perdido! Mejor suerte la próxima vez.");
                }
            }
            else if (command == "INVALID")
            {
                    MessageBox.Show($"¡La casilla {parts[1]},{parts[2]} ya ha sido atacada!");
            }
        }

        private void SetMoves(BoardControl boardControl, string[,] boardMoves)
        {
            int size = boardMoves.GetLength(0); // Assuming the board is square
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (boardMoves[i, j] == "m")
                    {
                        boardControl.UpdateCell(i, j, false);
                    }
                    else if (boardMoves[i, j] == "h")
                    {
                        boardControl.UpdateCell(i, j, true);
                    }
                    // If "o", do nothing
                }
            }
        }

        private bool[,] StringToBoard(string boardString)
        {
            bool[,] board = new bool[10, 10];
            string[] rows = boardString.Split('|');

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    board[i, j] = rows[i][j] == '1';
                }
            }
            return board;
        }
        private string[,] MovesStringToBoard(string boardString)
        {
            string[,] board = new string[10, 10];
            string[] rows = boardString.Split('|');

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    board[i, j] = rows[i][j].ToString();
                }
            }
            return board;
        }
        private void Menu()
        {
            MenuWindow menuWindow = new MenuWindow(username);
            menuWindow.Show();
            this.Close();

        }
        private async void SaveGameButton_Click(object sender, RoutedEventArgs e)
        {
            string message = $"SAVE";
            await socketClient.Send(message);
        }

        private bool IsPlayerTurn => username == currentPlayer;
    }
}
