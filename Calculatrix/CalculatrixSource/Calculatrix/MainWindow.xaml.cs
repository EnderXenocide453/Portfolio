using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Calculatrix
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<MatrixUI> matrixList = new List<MatrixUI>();
        List<ComboBox> operands = new List<ComboBox>();
        TextBlock answer;

        int count = 1;

        public MainWindow()
        {
            InitializeComponent();
            AddTab(null, null);
            ChooseFunc(null, null);
        }

        public void AddTab(object sender, RoutedEventArgs e)
        {
            TabItem newTab = new TabItem();
            MatrixUI matrix = new MatrixUI(newTab, "Матрица " + count);
            count++;

            matrixList.Add(matrix);
            TabsContainer.Items.Add(matrix.item);
            TabsContainer.SelectedIndex = TabsContainer.Items.Count - 1;
            ChooseFunc(null, null);
        }

        public void DeleteTab(object sender, RoutedEventArgs e)
        {
            ChooseFunc(null, null);
            matrixList.RemoveAt(TabsContainer.SelectedIndex);
            TabsContainer.Items.RemoveAt(TabsContainer.SelectedIndex);
            if (matrixList.Count == 0)
            {
                AddTab(null, null);
            }
        }

        public void ClearMatrix(object sender, RoutedEventArgs e)
        {
            matrixList[TabsContainer.SelectedIndex].Clear();
        }

        public void ResizeMatrix(object sender, RoutedEventArgs e)
        {
            int n = Convert.ToInt32(WidthBox.Text);
            int m = Convert.ToInt32(HeightBox.Text);
            matrixList[TabsContainer.SelectedIndex].ResizeMatrix(n, m);
        }

        public void ChooseFunc(object sender, RoutedEventArgs e)
        {

            switch (FuncBox.SelectedIndex)
            {
                case 0:
                    UpdateFunc("Найти определитель матрицы &matrix&break&Определитель равен &answer&");
                    break;
                case 1:
                    UpdateFunc("Транспонировать матрицу &matrix&");
                    break;
                case 2:
                    UpdateFunc("Найти обратную матрицу &matrix&");
                    break;
                case 3:
                    UpdateFunc("Найти матрицу миноров, соответствующую матрице &matrix&");
                    break;
                case 4:
                    UpdateFunc("Сложить матрицы &matrix& и &matrix&");
                    break;
                case 5:
                    UpdateFunc("Умножить матрицы &matrix& и &matrix&");
                    break;
                default:
                    UpdateFunc("");
                    break;
            }
        }

        private void UpdateFunc(string func)
        {
            FuncConteiner?.Children.Clear();
            operands.Clear();
            string[] sepFunc = func.Split('&');
            foreach (string item in sepFunc)
            {
                if (item == "matrix") {
                    ComboBox box = new ComboBox();
                    box.Height = 20;
                    box.Margin = new Thickness(0, 5, 0, 5);
                    foreach (MatrixUI matrix in matrixList) {
                        box.Items.Add(matrix.name);
                    }
                    FuncConteiner?.Children.Add(box);
                    operands.Add(box);
                    box.SelectedIndex = 0;
                    continue;
                }
                if (item == "answer") {
                    TextBlock block = new TextBlock();
                    block.Text = "NaN";
                    block.Height = 20;
                    block.Margin = new Thickness(0, 5, 0, 5);
                    FuncConteiner?.Children.Add(block);
                    answer = block;
                    continue;
                }
                if (item == "break")
                {
                    TextBlock block = new TextBlock();
                    block.Height = 1;
                    block.Width = 1000;
                    FuncConteiner?.Children.Add(block);
                    continue;
                }
                TextBlock text = new TextBlock();
                text.Text = item;
                text.Height = 20;
                text.Margin = new Thickness(5);
                FuncConteiner?.Children.Add(text);
            }
        }

        public void Calculate(object sender, RoutedEventArgs e)
        {
            Matrix matrix;
            switch (FuncBox.SelectedIndex)
            {
                case 0:
                    try {
                        answer.Text = (matrixList[operands[0].SelectedIndex].matrix.Determinant()).ToString();
                    }
                    catch (NotSqareException) {
                        TextBlock block = new TextBlock();
                        SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        block.Width = 1000;
                        block.Text = "Матрица должна быть квадратной";
                        block.Foreground = brush;
                        FuncConteiner.Children.Add(block);
                    }
                    break;
                case 1:
                    matrix = matrixList[operands[0].SelectedIndex].matrix.Transpose();
                    AddTab(null, null);
                    matrixList[matrixList.Count - 1].matrix = matrix;
                    matrixList[matrixList.Count - 1].DrawMatrix();
                    break;
                case 2:
                    try { 
                        matrix = matrixList[operands[0].SelectedIndex].matrix.Inverse();
                        AddTab(null, null);
                        matrixList[matrixList.Count - 1].matrix = matrix;
                        matrixList[matrixList.Count - 1].DrawMatrix();
                    }
                    catch (NotSqareException) {
                        TextBlock block = new TextBlock();
                        SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        block.Width = 1000;
                        block.Text = "Матрица должна быть квадратной";
                        block.Foreground = brush;
                        FuncConteiner.Children.Add(block);
                    }
                    break;
                case 3:
                    try {
                        matrix = matrixList[operands[0].SelectedIndex].matrix.AlgebraicAdditionMatrix();
                        AddTab(null, null);
                        matrixList[matrixList.Count - 1].matrix = matrix;
                        matrixList[matrixList.Count - 1].DrawMatrix();
                    }
                    catch (NotSqareException) {
                        TextBlock block = new TextBlock();
                        SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        block.Width = 1000;
                        block.Text = "Матрица должна быть квадратной";
                        block.Foreground = brush;
                        FuncConteiner.Children.Add(block);
                    }
                    break;
                case 4:
                    try {
                        matrix = matrixList[operands[0].SelectedIndex].matrix.MatrixSum(matrixList[operands[1].SelectedIndex].matrix);
                        AddTab(null, null);
                        matrixList[matrixList.Count - 1].matrix = matrix;
                        matrixList[matrixList.Count - 1].DrawMatrix();
                    }
                    catch (NotEqualRangeException) {
                        TextBlock block = new TextBlock();
                        SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        block.Width = 1000;
                        block.Text = "Матрицы должны иметь одинаковый размер";
                        block.Foreground = brush;
                        FuncConteiner.Children.Add(block);
                    }
                    break;
                case 5:
                    try {
                        matrix = matrixList[operands[0].SelectedIndex].matrix.MatrixMult(matrixList[operands[1].SelectedIndex].matrix);
                        AddTab(null, null);
                        matrixList[matrixList.Count - 1].matrix = matrix;
                        matrixList[matrixList.Count - 1].DrawMatrix();
                    }
                    catch (MultRangeException) {
                        TextBlock block = new TextBlock();
                        SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        block.Width = 1000;
                        block.Text = "Ширина первой матрицы должна быть равна высоте второй";
                        block.Foreground = brush;
                        FuncConteiner.Children.Add(block);
                    }
                    break;
                default:
                    UpdateFunc("");
                    break;
            }
        }
    }

    public class MatrixUI
    {
        public Matrix matrix;
        public Grid matrixContainer;
        public TabItem item;
        public List<List<Cell>> cells = new List<List<Cell>>();
        public TextBlock range;
        public string name;

        public MatrixUI (TabItem tab, string _name)
        {
            item = tab;
            name = _name;
            item.Header = new TextBlock { Text = name };
            matrixContainer = new Grid();
            ScrollViewer scroll = new ScrollViewer();

            matrix = new Matrix(3, 3);
            range = new TextBlock();

            DrawMatrix();

            scroll.Content = matrixContainer;
            scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

            item.Content = scroll;
        }

        public void DrawMatrix()
        {
            List<Cell> tmp = new List<Cell>();
            matrixContainer.Children.Clear();
            matrixContainer.RowDefinitions.Clear();
            matrixContainer.ColumnDefinitions.Clear();

            for (int i = 0; i <= matrix.N; i++)
            {
                ColumnDefinition column = new ColumnDefinition();
                column.Width = new GridLength(80);
                matrixContainer.ColumnDefinitions.Add(column);
            }

            for (int i = 0; i <= matrix.M; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(50);
                matrixContainer.RowDefinitions.Add(row);
            }

            for (int i = 0; i < matrix.N; i++) { 
                tmp.Clear();
                for (int j = 0; j < matrix.M; j++)
                {
                    Cell cell = new Cell(i, j);
                    cell.cell.Text = matrix.GetElement(i, j).ToString();
                    cell.Changed += UpdateMatrix;
                    tmp.Add(cell);

                    matrixContainer.Children.Add(cell.border);
                    Grid.SetColumn(cell.border, i);
                    Grid.SetRow(cell.border, j);
                }
                cells.Add(tmp);
            }

            Button addRowButton = new Button();
            Button addColumnButton = new Button();

            Border rowBorder = RoundButt(addRowButton, 30);
            Border columnBorder = RoundButt(addColumnButton, 30);

            addRowButton.Click += AddRow;
            addColumnButton.Click += AddColumn;

            matrixContainer.Children.Add(rowBorder);
            matrixContainer.Children.Add(columnBorder);

            Grid.SetColumn(columnBorder, matrixContainer.ColumnDefinitions.Count - 1);
            Grid.SetRow(columnBorder, 0);

            Grid.SetColumn(rowBorder, 0);
            Grid.SetRow(rowBorder, matrixContainer.RowDefinitions.Count - 1);

            rowBorder.HorizontalAlignment = HorizontalAlignment.Left;
            columnBorder.VerticalAlignment = VerticalAlignment.Top;
        }

        private Border RoundButt(Button butt, double diameter)
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(217, 217, 217));
            Border border = new Border();

            butt.Background = brush.Clone();
            butt.Background.Opacity = 0;
            butt.BorderThickness = new Thickness(0);
            butt.Content = "+";
            butt.HorizontalContentAlignment = HorizontalAlignment.Center;
            butt.VerticalContentAlignment = VerticalAlignment.Center;
            butt.FontSize = 16;

            border.MaxWidth = diameter;
            border.MinWidth = diameter;
            border.MaxHeight = diameter;
            border.MinHeight = diameter;
            border.Child = butt;
            border.Background = brush;
            border.CornerRadius = new CornerRadius(diameter);
            border.Margin = new Thickness(5);

            return border;
        }

        public void UpdateMatrix(int i, int j, double num)
        {
            matrix.SetElement(i, j, num);
        }

        public void ResizeMatrix(int n, int m)
        {
            matrix.Resize(n, m);
            DrawMatrix();
        }

        public void AddRow(object sender, RoutedEventArgs e)
        {
            ResizeMatrix(matrix.N, matrix.M + 1);
        }

        public void AddColumn(object sender, RoutedEventArgs e)
        {
            ResizeMatrix(matrix.N + 1, matrix.M);
        }

        public void Clear()
        {
            matrix.SetNumToAll(0);
            ResizeMatrix(3, 3);
        }
    }

    public class Cell
    {
        public TextBox cell;
        public Border border;
        public int I;
        public int J;

        public delegate void ChangeHandler(int i, int j, double num);
        public event ChangeHandler Changed;

        public Cell(int i, int j)
        {
            I = i;
            J = j;

            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(217, 217, 217));

            cell = new TextBox();
            cell.HorizontalContentAlignment = HorizontalAlignment.Center;
            cell.VerticalContentAlignment = VerticalAlignment.Center;
            cell.FontSize = 16;
            cell.FontFamily = new FontFamily("Segoe UI");
            cell.BorderThickness = new Thickness(0);
            cell.Background = brush.Clone();
            cell.Background.Opacity = 0;

            border = new Border();
            border.Child = cell;
            border.Background = brush;
            border.CornerRadius = new CornerRadius(10);
            border.Margin = new Thickness(5);


            cell.TextChanged += ChangeCell;
        }

        private void ChangeCell(object sender, RoutedEventArgs e)
        {
            RemoveSymbols();
            if (cell.Text != "")
                Changed?.Invoke(I, J, Convert.ToDouble(cell.Text));
            else cell.Text = "0";
        }

        private void RemoveSymbols()
        {
            string tmp = "";
            foreach (char item in cell.Text)
            {
                if (char.IsDigit(item))
                {
                    tmp += item;
                }
            }
            if (tmp == "")
                tmp = "0";
            cell.Text = Convert.ToDouble(tmp).ToString();
        }
    }
}
