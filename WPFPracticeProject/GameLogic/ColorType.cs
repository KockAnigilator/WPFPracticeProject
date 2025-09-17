using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic
{
    /// <summary>
    /// Статический класс для работы с цветами фигур
    /// </summary>
    public static class ColorType
    {
        public const int Red = 0;
        public const int Yellow = 1;
        public const int Blue = 2;

        /// <summary>
        /// Получает название цвета по его числовому идентификатору
        /// </summary>
        /// <param name="color">Числовой идентификатор цвета</param>
        /// <returns>Название цвета</returns>
        public static string GetName(int color)
        {
            switch (color)
            {
                case Red:
                    return "Красный";
                case Yellow:
                    return "Жёлтый";
                case Blue:
                    return "Синий";
                default:
                    return "Неизвестно";
            }
        }

        /// <summary>
        /// Генерирует случайный цвет
        /// </summary>
        /// <param name="random">Экземпляр генератора случайных чисел</param>
        /// <returns>Случайный цвет</returns>
        public static int GetRandomColor(System.Random random)
        {
            return random.Next(0, 3);
        }
    }

}
