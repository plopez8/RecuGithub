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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Barcos.Models;
using Barcos.WPF;

namespace Barcos.Controls
{
    /// <summary>
    /// Lógica de interacción para GameControl.xaml
    /// </summary>
    public partial class GameControl : UserControl
    {
        private Game game;
        private string username;
        private string gameid;
        public GameControl(string username, Game game, string gameid)
        {
            InitializeComponent();
            this.game = game;
            this.username = username;
            this.gameid = gameid;
            GameID.Text = gameid;
            Player1Name.Text = game.Player1.Username;
            Player2Name.Text = game.Player2.Username;
        }
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {

            // Crea una nueva ventana de juego
            if (game.IsFinished)
            {
                MessageBox.Show("Este juego esta terminado.");
            }
            else
            {

            var gameWindow = new GameWindow(username, gameid);

            // Muestra la ventana de juego
            gameWindow.Show();
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
            }

        }
    }
}
