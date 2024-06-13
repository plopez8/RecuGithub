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

namespace Barcos.Controls
{
    /// <summary>
    /// Lógica de interacción para CellControl.xaml
    /// </summary>
    public partial class CellControl : UserControl
    {
        public delegate void CellClickedEventHandler(int row, int col);
        public event CellClickedEventHandler CellClicked;

        public int Row { get; set; }
        public int Col { get; set; }

        public CellControl()
        {
            InitializeComponent();
        }
        private void CellButton_Click(object sender, RoutedEventArgs e)
        {
            CellClicked?.Invoke(Row, Col);
        }

        public void SetHit()
        {
            CellButton.Background = Brushes.Red;
        }

        public void SetMiss()
        {
            CellButton.Background = Brushes.Gray;
        }
        public void SetShip()
        {
            CellButton.Background = Brushes.Black;
        }
    }
}
