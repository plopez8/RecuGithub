﻿using System;
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

namespace Barcos.WPF
{
    /// <summary>
    /// Lógica de interacción para MenuWindow.xaml
    /// </summary>
    public partial class MenuWindow : Window
    {
        private string username;
        public MenuWindow(string username)
        {
            
            this.username = username;
            InitializeComponent();
        }
        private void LoadGameButton_Click(object sender, RoutedEventArgs e)
        {
            SavedGamesWindow savedGamesWindow = new SavedGamesWindow(this.username);
            savedGamesWindow.Show();
            this.Close();
        }
        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            GameWindow gameWindow = new GameWindow(this.username);
            gameWindow.Show();
            this.Close();
        }
    }
}
