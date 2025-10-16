using System.ComponentModel;

namespace GameLogic
{
    /// <summary>
    /// Типы фигур
    /// </summary>
    public enum ShapeType
    {
        [Description("Круг")]
        Circle = 0,

        [Description("Квадрат")]
        Square = 1,

        [Description("Треугольник")]
        Triangle = 2
    }
}