using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic
{
    /// <summary>
    /// Класс, представляющий фигуру на игровом поле
    /// </summary>
    public class Shape
    {
        public int Type { get; set; }
        public int Color { get; set; }
        public int Age { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Guid Id { get; private set; }

        /// <summary>
        /// Создает новую фигуру
        /// </summary>
        /// <param name="type">Тип фигуры</param>
        /// <param name="color">Цвет фигуры</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        public Shape(int type, int color, int x, int y)
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
        /// <returns>Клон фигуры</returns>
        public Shape Clone()
        {
            Shape clone = new Shape(Type, Color, X, Y);
            clone.Age = Age;
            clone.Id = Id; // Сохраняем тот же ID для отслеживания
            return clone;
        }

        /// <summary>
        /// Возвращает строковое представление фигуры
        /// </summary>
        /// <returns>Строка с информацией о фигуре</returns>
        public override string ToString()
        {
            return $"{ShapeType.GetName(Type)} {ColorType.GetName(Color)} " +
                   $"в позиции ({X},{Y}), Возраст: {Age}";
        }

        /// <summary>
        /// Проверяет равенство фигур по идентификатору
        /// </summary>
        /// <param name="obj">Объект для сравнения</param>
        /// <returns>True если фигуры равны, иначе False</returns>
        public override bool Equals(object obj)
        {
            if (obj is Shape shape)
            {
                return Id.Equals(shape.Id);
            }
            return false;
        }

        /// <summary>
        /// Возвращает хэш-код фигуры
        /// </summary>
        /// <returns>Хэш-код</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
