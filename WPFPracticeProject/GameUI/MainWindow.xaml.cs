using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using GameLogic;
using System.Linq;
using Shape = GameLogic.Shape;

namespace GameUI
{
    public partial class MainWindow : Window
    {
        private GameField gameField;
        private const int CellSize = 10; // Размер клетки в пикселях

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            try
            {
                gameField = new GameField();
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка инициализации игры: {ex.Message}");
            }
        }

        private void MoveForward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (gameField.IsGameOver())
                {
                    MessageBox.Show("Игра завершена! Начните новую игру.", "Игра окончена",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                gameField.MoveForward();
                UpdateDisplay();

                if (gameField.IsGameOver())
                {
                    MessageBox.Show($"Игра завершена! Ваш счет: {gameField.Score}", "Поздравляем!",
                                  MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при ходе вперед: {ex.Message}");
            }
        }

        private void MoveBackward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                gameField.MoveBackward();
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при ходе назад: {ex.Message}");
            }
        }

        private void ResetGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                gameField.ResetGame();
                UpdateDisplay();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при сбросе игры: {ex.Message}");
            }
        }

        private void UpdateDisplay()
        {
            try
            {
                // Обновляем текстовую информацию
                ScoreText.Text = gameField.Score.ToString();
                MoveCountText.Text = gameField.MoveCount.ToString();

                var shapes = gameField.GetAllShapes();
                ShapesCountText.Text = shapes.Count().ToString();

                StatusText.Text = gameField.GetDetailedStatus();

                // Обновляем визуальное представление поля
                UpdateFieldVisualization();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка обновления отображения: {ex.Message}");
            }
        }

        private void UpdateFieldVisualization()
        {
            GameCanvas.Children.Clear();

            foreach (var shape in gameField.GetAllShapes())
            {
                DrawShape(shape);
            }
        }

        private void DrawShape(Shape shape)
        {
            try
            {
                var visualShape = CreateShapeVisual(shape);
                if (visualShape != null)
                {
                    // Позиционируем фигуру на канвасе
                    Canvas.SetLeft(visualShape, shape.X * CellSize);
                    Canvas.SetTop(visualShape, shape.Y * CellSize);

                    GameCanvas.Children.Add(visualShape);

                    // Добавляем ToolTip с информацией о фигуре
                    var toolTip = new ToolTip();
                    toolTip.Content = $"Тип: {ShapeType.GetName(shape.Type)}\n" +
                                    $"Цвет: {ColorType.GetName(shape.Color)}\n" +
                                    $"Возраст: {shape.Age}\n" +
                                    $"Позиция: ({shape.X}, {shape.Y})";
                    visualShape.ToolTip = toolTip;
                }
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки отрисовки отдельных фигур
                System.Diagnostics.Debug.WriteLine($"Ошибка отрисовки фигуры: {ex.Message}");
            }
        }

        private System.Windows.Shapes.Shape CreateShapeVisual(GameLogic.Shape shape)
        {
            Brush fillBrush = GetColorBrush(shape.Color);
            Brush strokeBrush = Brushes.Black;
            double strokeThickness = 0.5;

            switch (shape.Type)
            {
                case ShapeType.Circle:
                    return new Ellipse
                    {
                        Width = CellSize - 2,
                        Height = CellSize - 2,
                        Fill = fillBrush,
                        Stroke = strokeBrush,
                        StrokeThickness = strokeThickness
                    };

                case ShapeType.Square:
                    return new Rectangle
                    {
                        Width = CellSize - 2,
                        Height = CellSize - 2,
                        Fill = fillBrush,
                        Stroke = strokeBrush,
                        StrokeThickness = strokeThickness
                    };

                case ShapeType.Triangle:
                    var polygon = new Polygon
                    {
                        Fill = fillBrush,
                        Stroke = strokeBrush,
                        StrokeThickness = strokeThickness
                    };

                    // Создаем треугольник
                    polygon.Points.Add(new System.Windows.Point(CellSize / 2, 1));
                    polygon.Points.Add(new System.Windows.Point(1, CellSize - 1));
                    polygon.Points.Add(new System.Windows.Point(CellSize - 1, CellSize - 1));

                    return polygon;

                default:
                    return new Rectangle
                    {
                        Width = CellSize - 2,
                        Height = CellSize - 2,
                        Fill = Brushes.Gray,
                        Stroke = Brushes.Black,
                        StrokeThickness = 1
                    };
            }
        }

        private Brush GetColorBrush(int color)
        {
            switch (color)
            {
                case ColorType.Red:
                    return Brushes.Red;
                case ColorType.Yellow:
                    return Brushes.Yellow;
                case ColorType.Blue:
                    return Brushes.Blue;
                default:
                    return Brushes.Gray;
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                // Очистка ресурсов если нужно
                base.OnClosing(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при закрытии: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

}