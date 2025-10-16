using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace GameLogic
{
    public static class EnumHelper
    {
        /// <summary>
        /// Получает описание значения перечисления
        /// </summary>
        public static string Description(this Enum value)
        {
            if (value == null) return string.Empty;

            FieldInfo field = value.GetType().GetField(value.ToString());
            if (field == null) return value.ToString();

            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }

        /// <summary>
        /// Получает все значения и описания перечисления
        /// </summary>
        public static IEnumerable<Tuple<object, object>> GetAllValuesAndDescriptions(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("Тип должен быть перечислением");

            return Enum.GetValues(enumType)
                      .Cast<object>()
                      .Select(e => new Tuple<object, object>(e, ((Enum)e).Description()));
        }

        /// <summary>
        /// Получает значение перечисления по строковому представлению
        /// </summary>
        public static object GetValue(Type enumType, string value)
        {
            return Enum.Parse(enumType, value);
        }

        /// <summary>
        /// Генерирует случайное значение перечисления
        /// </summary>
        public static T GetRandomEnumValue<T>(Random random) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(random.Next(values.Length));
        }
    }
}