using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
    /// <summary>
    /// Класс, представляющий игровое поле и управляющий игровой логикой
    /// </summary>
    public class GameField
    {
        private const int FieldSize = 128;
        private const double ColorChangeProbabilityFactor = 0.05;
        private const int NewShapesPerMove = 3;
        private const int LineScore = 10;

        private List<Shape>[,] field;
        private List<List<Shape>> history;
        private int currentHistoryIndex;
        private Random random;

        /// <summary>
        /// Текущее количество очков игрока
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// Количество сделанных ходов
        /// </summary>
        public int MoveCount { get; private set; }

        /// <summary>
        /// Создает новое игровое поле
        /// </summary>
        public GameField()
        {
            InitializeField();
            random = new Random();
            ResetGame();
        }

        /// <summary>
        /// Инициализирует игровое поле
        /// </summary>
        private void InitializeField()
        {
            field = new List<Shape>[FieldSize, FieldSize];
            for (int i = 0; i < FieldSize; i++)
            {
                for (int j = 0; j < FieldSize; j++)
                {
                    field[i, j] = new List<Shape>();
                }
            }
        }

        /// <summary>
        /// Сбрасывает игру в начальное состояние
        /// </summary>
        public void ResetGame()
        {
            InitializeField();
            history = new List<List<Shape>>();
            history.Add(new List<Shape>());
            currentHistoryIndex = 0;
            Score = 0;
            MoveCount = 0;
        }

        /// <summary>
        /// Добавляет случайные фигуры на поле
        /// </summary>
        /// <param name="count">Количество добавляемых фигур</param>
        public void AddRandomShape(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                int x = random.Next(FieldSize);
                int y = random.Next(FieldSize);
                ShapeType type = EnumHelper.GetRandomEnumValue<ShapeType>(random);
                ColorType color = EnumHelper.GetRandomEnumValue<ColorType>(random);

                AddShape(new Shape(type, color, x, y));
            }
        }

        /// <summary>
        /// Добавляет фигуру на поле
        /// </summary>
        /// <param name="shape">Добавляемая фигура</param>
        public void AddShape(Shape shape)
        {
            if (shape == null)
                throw new ArgumentNullException(nameof(shape));

            if (!IsValidPosition(shape.X, shape.Y))
                throw new ArgumentException($"Неверная позиция: ({shape.X}, {shape.Y})");

            field[shape.X, shape.Y].Add(shape);
            GetCurrentState().Add(shape);
        }

        /// <summary>
        /// Выполняет ход вперед
        /// </summary>
        public void MoveForward()
        {
            SaveCurrentState();
            MoveCount++;

            UpdateShapesAgeAndColor();
            MoveAllShapes();
            CheckAndRemoveLines();
            AddRandomShape(NewShapesPerMove);
        }

        /// <summary>
        /// Выполняет ход назад
        /// </summary>
        public void MoveBackward()
        {
            if (currentHistoryIndex > 0)
            {
                currentHistoryIndex--;
                MoveCount--;
                RestoreState();
            }
        }

        /// <summary>
        /// Обновляет возраст и цвет всех фигур
        /// </summary>
        private void UpdateShapesAgeAndColor()
        {
            foreach (var shape in GetCurrentState())
            {
                shape.Age++;
                TryChangeShapeColor(shape);
            }
        }

        /// <summary>
        /// Пытается изменить цвет фигуры в зависимости от возраста
        /// </summary>
        /// <param name="shape">Фигура для изменения цвета</param>
        private void TryChangeShapeColor(Shape shape)
        {
            double changeProbability = shape.Age * ColorChangeProbabilityFactor;
            if (random.NextDouble() < changeProbability)
            {
                shape.Color = EnumHelper.GetRandomEnumValue<ColorType>(random);
            }
        }

        /// <summary>
        /// Перемещает все фигуры на поле
        /// </summary>
        private void MoveAllShapes()
        {
            var shapesToMove = GetCurrentState().ToList();

            foreach (var shape in shapesToMove)
            {
                MoveShape(shape);
            }
        }

        /// <summary>
        /// Перемещает конкретную фигуру
        /// </summary>
        /// <param name="shape">Фигура для перемещения</param>
        private void MoveShape(Shape shape)
        {
            int direction = random.Next(4);
            int newX = shape.X;
            int newY = shape.Y;

            switch (direction)
            {
                case 0: newY--; break; // Вверх
                case 1: newX++; break; // Вправо
                case 2: newY++; break; // Вниз
                case 3: newX--; break; // Влево
            }

            if (IsValidPosition(newX, newY))
            {
                field[shape.X, shape.Y].Remove(shape);

                if (field[newX, newY].Count > 0)
                {
                    HandleCollision(shape, newX, newY);
                }
                else
                {
                    MoveToEmptyCell(shape, newX, newY);
                }
            }
        }

        /// <summary>
        /// Обрабатывает столкновение фигур
        /// </summary>
        /// <param name="attacker">Атакующая фигура</param>
        /// <param name="targetX">Координата X цели</param>
        /// <param name="targetY">Координата Y цели</param>
        private void HandleCollision(Shape attacker, int targetX, int targetY)
        {
            Shape defender = field[targetX, targetY][0];

            if (CanReplace(attacker, defender))
            {
                ReplaceShape(attacker, defender, targetX, targetY);
            }
            else
            {
                ReturnToOriginalPosition(attacker);
            }
        }

        /// <summary>
        /// Проверяет, может ли одна фигура заменить другую
        /// </summary>
        /// <param name="attacker">Атакующая фигура</param>
        /// <param name="defender">Защищающаяся фигура</param>
        /// <returns>True если замена возможна, иначе False</returns>
        private bool CanReplace(Shape attacker, Shape defender)
        {
            // Круг сильнее треугольника
            if (attacker.Type == ShapeType.Circle && defender.Type == ShapeType.Triangle)
            {
                return true;
            }

            // Треугольник сильнее квадрата
            if (attacker.Type == ShapeType.Triangle && defender.Type == ShapeType.Square)
            {
                return true;
            }

            // Квадрат сильнее круга
            if (attacker.Type == ShapeType.Square && defender.Type == ShapeType.Circle)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Заменяет фигуру на поле
        /// </summary>
        /// <param name="attacker">Атакующая фигура</param>
        /// <param name="defender">Защищающаяся фигура</param>
        /// <param name="targetX">Координата X цели</param>
        /// <param name="targetY">Координата Y цели</param>
        private void ReplaceShape(Shape attacker, Shape defender, int targetX, int targetY)
        {
            field[targetX, targetY].Remove(defender);
            GetCurrentState().Remove(defender);
            field[targetX, targetY].Add(attacker);
            attacker.X = targetX;
            attacker.Y = targetY;
        }

        /// <summary>
        /// Возвращает фигуру на исходную позицию
        /// </summary>
        /// <param name="shape">Фигура для возврата</param>
        private void ReturnToOriginalPosition(Shape shape)
        {
            field[shape.X, shape.Y].Add(shape);
        }

        /// <summary>
        /// Перемещает фигуру в пустую клетку
        /// </summary>
        /// <param name="shape">Перемещаемая фигура</param>
        /// <param name="newX">Новая координата X</param>
        /// <param name="newY">Новая координата Y</param>
        private void MoveToEmptyCell(Shape shape, int newX, int newY)
        {
            field[newX, newY].Add(shape);
            shape.X = newX;
            shape.Y = newY;
        }

        /// <summary>
        /// Проверяет и удаляет линии из одинаковых фигур
        /// </summary>
        private void CheckAndRemoveLines()
        {
            int removedLines = 0;

            removedLines += CheckHorizontalLines();
            removedLines += CheckVerticalLines();

            if (removedLines > 0)
            {
                Score += removedLines * LineScore;
            }
        }

        /// <summary>
        /// Проверяет горизонтальные линии
        /// </summary>
        /// <returns>Количество удаленных горизонтальных линий</returns>
        private int CheckHorizontalLines()
        {
            int removedLines = 0;

            for (int y = 0; y < FieldSize; y++)
            {
                for (int x = 0; x < FieldSize - 2; x++)
                {
                    if (CheckLine(x, y, 1, 0))
                    {
                        removedLines++;
                    }
                }
            }

            return removedLines;
        }

        /// <summary>
        /// Проверяет вертикальные линии
        /// </summary>
        /// <returns>Количество удаленных вертикальных линий</returns>
        private int CheckVerticalLines()
        {
            int removedLines = 0;

            for (int x = 0; x < FieldSize; x++)
            {
                for (int y = 0; y < FieldSize - 2; y++)
                {
                    if (CheckLine(x, y, 0, 1))
                    {
                        removedLines++;
                    }
                }
            }

            return removedLines;
        }

        /// <summary>
        /// Проверяет линию в заданном направлении
        /// </summary>
        /// <param name="startX">Начальная координата X</param>
        /// <param name="startY">Начальная координата Y</param>
        /// <param name="dx">Приращение по X</param>
        /// <param name="dy">Приращение по Y</param>
        /// <returns>True если линия была найдена и удалена, иначе False</returns>
        private bool CheckLine(int startX, int startY, int dx, int dy)
        {
            List<Shape> lineShapes = new List<Shape>();

            for (int i = 0; i < 3; i++)
            {
                int x = startX + i * dx;
                int y = startY + i * dy;

                if (field[x, y].Count > 0)
                {
                    lineShapes.Add(field[x, y][0]);
                }
            }

            if (lineShapes.Count == 3 && IsUniformLine(lineShapes))
            {
                RemoveLine(lineShapes);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверяет, является ли линия uniform (все фигуры одинакового типа и цвета)
        /// </summary>
        /// <param name="shapes">Список фигур в линии</param>
        /// <returns>True если линия uniform, иначе False</returns>
        private bool IsUniformLine(List<Shape> shapes)
        {
            ShapeType firstType = shapes[0].Type;
            ColorType firstColor = shapes[0].Color;

            foreach (var shape in shapes)
            {
                if (shape.Type != firstType || shape.Color != firstColor)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Удаляет линию фигур с поля
        /// </summary>
        /// <param name="shapes">Фигуры для удаления</param>
        private void RemoveLine(List<Shape> shapes)
        {
            foreach (var shape in shapes)
            {
                field[shape.X, shape.Y].Remove(shape);
                GetCurrentState().Remove(shape);
            }
        }

        /// <summary>
        /// Сохраняет текущее состояние игры
        /// </summary>
        private void SaveCurrentState()
        {
            List<Shape> newState = new List<Shape>();

            foreach (var shape in GetCurrentState())
            {
                newState.Add(shape.Clone());
            }

            history.Add(newState);
            currentHistoryIndex++;
        }

        /// <summary>
        /// Восстанавливает состояние игры из истории
        /// </summary>
        private void RestoreState()
        {
            ClearField();
            RestoreShapesToField();
        }

        /// <summary>
        /// Очищает игровое поле
        /// </summary>
        private void ClearField()
        {
            for (int i = 0; i < FieldSize; i++)
            {
                for (int j = 0; j < FieldSize; j++)
                {
                    field[i, j].Clear();
                }
            }
        }

        /// <summary>
        /// Восстанавливает фигуры на поле из текущего состояния
        /// </summary>
        private void RestoreShapesToField()
        {
            foreach (var shape in GetCurrentState())
            {
                if (IsValidPosition(shape.X, shape.Y))
                {
                    field[shape.X, shape.Y].Add(shape);
                }
            }
        }

        /// <summary>
        /// Получает текущее состояние игры
        /// </summary>
        /// <returns>Список фигур текущего состояния</returns>
        private List<Shape> GetCurrentState()
        {
            return history[currentHistoryIndex];
        }

        /// <summary>
        /// Проверяет, является ли позиция valid
        /// </summary>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        /// <returns>True если позиция valid, иначе False</returns>
        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < FieldSize && y >= 0 && y < FieldSize;
        }

        /// <summary>
        /// Проверяет, завершена ли игра
        /// </summary>
        /// <returns>True если игра завершена, иначе False</returns>
        public bool IsGameOver()
        {
            var shapes = GetCurrentState();
            if (shapes.Count < 2)
            {
                return false;
            }

            bool allSameColor = CheckAllSameColor(shapes);
            bool allSameType = CheckAllSameType(shapes);

            return allSameColor || allSameType;
        }

        /// <summary>
        /// Проверяет, все ли фигуры одного цвета
        /// </summary>
        /// <param name="shapes">Список фигур</param>
        /// <returns>True если все фигуры одного цвета, иначе False</returns>
        private bool CheckAllSameColor(List<Shape> shapes)
        {
            ColorType firstColor = shapes[0].Color;

            foreach (var shape in shapes)
            {
                if (shape.Color != firstColor)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Проверяет, все ли фигуры одного типа
        /// </summary>
        /// <param name="shapes">Список фигур</param>
        /// <returns>True если все фигуры одного типа, иначе False</returns>
        private bool CheckAllSameType(List<Shape> shapes)
        {
            ShapeType firstType = shapes[0].Type;

            foreach (var shape in shapes)
            {
                if (shape.Type != firstType)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Получает все фигуры текущего состояния
        /// </summary>
        /// <returns>Перечисление всех фигур</returns>
        public IEnumerable<Shape> GetAllShapes()
        {
            return GetCurrentState();
        }

        /// <summary>
        /// Получает статус игры в виде строки
        /// </summary>
        /// <returns>Строка с информацией о статусе игры</returns>
        public string GetDetailedStatus()
        {
            var shapes = GetAllShapes();
            var status = IsGameOver() ? "Игра окончена" : "Игра продолжается";

            var colorStats = shapes.GroupBy(s => s.Color)
                                  .Select(g => $"{g.Key.Description()}: {g.Count()}");

            var typeStats = shapes.GroupBy(s => s.Type)
                                 .Select(g => $"{g.Key.Description()}: {g.Count()}");

            return $"Ход: {MoveCount}, Очки: {Score}, Фигур: {shapes.Count()}\n" +
                   $"Цвета: {string.Join(", ", colorStats)}\n" +
                   $"Типы: {string.Join(", ", typeStats)}\n" +
                   $"Состояние: {status}";
        }

        /// <summary>
        /// Выводит текущее состояние поля в консоль
        /// </summary>
        public void PrintFieldState()
        {
            Console.WriteLine("=== ТЕКУЩЕЕ СОСТОЯНИЕ ПОЛЯ ===");
            Console.WriteLine(GetDetailedStatus());

            foreach (var shape in GetAllShapes().OrderBy(s => s.X).ThenBy(s => s.Y))
            {
                Console.WriteLine(shape);
            }

            Console.WriteLine("==============================");
        }
    }
}