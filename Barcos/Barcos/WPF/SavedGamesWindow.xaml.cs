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
using Barcos.Controls;
using Barcos.Models;
using Barcos.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Barcos.WPF
{
    /// <summary>
    /// Lógica de interacción para SavedGamesWindow.xaml
    /// </summary>
    public partial class SavedGamesWindow : Window

    {
        private string username;

        private GameService _gameService; // Add this line to create a GameService instance

        public SavedGamesWindow(string username)
        {
            this.username = username;
            InitializeComponent();

            _gameService = new GameService(); // Initialize the GameService instance

            LoadUserGames(); // Call the LoadUserGames method
        }

        private async void LoadUserGames() // Add this method to load the user's games
        {
            List<BsonDocument> games = await _gameService.GetUserGames(username);

            foreach (var gameDoc in games)
            {
                // Deserializa el documento Bson a un objeto Game
                Game game = BsonSerializer.Deserialize<Game>(gameDoc["game"].AsBsonDocument);
                string gameid = gameDoc["_id"].ToString();
                GameControl gameControl = new GameControl(username, game, gameid);
                // Add the GameControl to the window
                // Replace "YourContainer" with the name of the container where you want to add the GameControl
                GameListPanel.Children.Add(gameControl);
            }
        }
    }
}
