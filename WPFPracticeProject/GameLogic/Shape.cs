using System;

namespace GameLogic
{
    /// <summary>
    /// Класс, представляющий фигуру на игровом поле
    /// </summary>
    public class Shape
    {
        private ShapeType _type;
        private ColorType _color;

        public ShapeType Type
        {
            get => _type;
            set => _type = value;
        }

        public ColorType Color
        {
            get => _color;
            set => _color = value;
        }

        public int Age { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Guid Id { get; private set; }

        /// <summary>
        /// Создает новую фигуру
        /// </summary>
        public Shape(ShapeType type, ColorType color, int x, int y)
        {
            Type = type;
            Color = color;
            X = x;
            Y = y;
            Age = 0;
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Создает копию фигуры
        /// </summary>
        public Shape Clone()
        {
            Shape clone = new Shape(Type, Color, X, Y);
            clone.Age = Age;
            clone.Id = Id;
            return clone;
        }

        /// <summary>
        /// Возвращает строковое представление фигуры
        /// </summary>
        public override string ToString()
        {
            return $"{Type.Description()} {Color.Description()} " +
                   $"в позиции ({X},{Y}), Возраст: {Age}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Shape shape)
            {
                return Id.Equals(shape.Id);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}