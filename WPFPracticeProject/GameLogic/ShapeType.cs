using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic
{
    /// <summary>
    /// Статический класс для работы с типами фигур
    /// </summary>
    public static class ShapeType
    {
        public const int Circle = 0;
        public const int Square = 1;
        public const int Triangle = 2;

        /// <summary>
        /// Получает название типа фигуры по его числовому идентификатору
        /// </summary>
        /// <param name="type">Числовой идентификатор типа фигуры</param>
        /// <returns>Название типа фигуры</returns>
        public static string GetName(int type)
        {
            switch (type)
            {
                case Circle:
                    return "Круг";
                case Square:
                    return "Квадрат";
                case Triangle:
                    return "Треугольник";
                default:
                    return "Неизвестно";
            }
        }
    }
}
