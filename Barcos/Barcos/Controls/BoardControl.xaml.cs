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
    /// Lógica de interacción para BoardControl.xaml
    /// </summary>
    public partial class BoardControl : UserControl
    {
        private bool[,] shipPositions; // Array para almacenar la presencia de barcos en el tablero

        public BoardControl()
        {
            InitializeComponent();
            CreateBoard();
            shipPositions = new bool[10, 10]; // Inicializa el array de posiciones de los barcos
            PlaceShips(); // Coloca los barcos en el tablero de forma aleatoria
        }

        private void CreateBoard()
        {
            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    var cell = new CellControl
                    {
                        Row = row,
                        Col = col
                    };
                    cell.CellClicked += Cell_CellClicked;
                    BoardGrid.Children.Add(cell);
                }
            }
        }

        private void Cell_CellClicked(int row, int col)
        {
            CellClicked?.Invoke(row, col);
        }

        public void UpdateCell(int row, int col, bool isHit)
        {
            foreach (CellControl cell in BoardGrid.Children)
            {
                if (cell.Row == row && cell.Col == col)
                {
                    if (isHit)
                    {
                        cell.SetHit();
                    }
                    else
                    {
                        cell.SetMiss();
                    }
                    break;
                }
            }
        }

        public void PlaceShips()
        {
            Random rand = new Random();

            // Colocar barcos aleatoriamente en el tablero
            // Por ejemplo, coloca un barco de tamaño 4 horizontalmente
            int startRow = rand.Next(0, 10);
            int startCol = rand.Next(0, 7); // Asegúrate de que el barco quepa en el tablero
            for (int i = 0; i < 4; i++)
            {
                shipPositions[startRow, startCol + i] = true; // Marca la posición del barco en el array
            }

            // Repite esto para otros barcos con diferentes tamaños y orientaciones según sea necesario
        }

        public void UpdateBoard(bool[,] shipPositions, string action)
        {
            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    if (shipPositions[row, col])
                    {
                        var cell = GetCellControl(row, col);
                        if (cell != null)
                        {
                            switch (action)
                            {
                                case "hit":
                                    cell.SetHit();
                                    break;
                                case "miss":
                                    cell.SetMiss();
                                    break;
                                case "ship":
                                    cell.SetShip();
                                    break;
                                default:
                                    throw new ArgumentException("Invalid action provided");
                            }
                        }
                    }
                }
            }
        }

        private CellControl GetCellControl(int row, int col)
        {
            foreach (CellControl cell in BoardGrid.Children)
            {
                if (cell.Row == row && cell.Col == col)
                {
                    return cell;
                }
            }
            return null;
        }

        public delegate void CellClickedEventHandler(int row, int col);
        public event CellClickedEventHandler CellClicked;
    }
}
