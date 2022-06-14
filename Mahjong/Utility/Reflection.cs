using System;
using System.Reflection;

namespace Mahjong.Utility
{
    class ReflectionUtility
    {
        public static object GetValue(object instance, string property, bool ignoreCase = false)
        {
            BindingFlags flags = ignoreCase ? defaultFlags | BindingFlags.IgnoreCase : defaultFlags;
            return instance.GetType().GetProperty(property, flags).GetValue(instance);
        }

        public static void SetValue(object instance, string property, object value, bool ignoreCase = false)
        {
            BindingFlags flags = ignoreCase ? defaultFlags | BindingFlags.IgnoreCase : defaultFlags;
            instance.GetType().GetProperty(property, flags).SetValue(instance, value);
        }

        public static void Increment(object instance, string property, int number = 1, bool ignoreCase = false)
        {
            int value = Convert.ToInt32(GetValue(instance, property, ignoreCase)) + number;
            SetValue(instance, property, value, ignoreCase);
        }

        public static void Increment(object instance, string property, decimal number, bool ignoreCase = false)
        {
            decimal value = Convert.ToInt32(GetValue(instance, property, ignoreCase)) + number;
            SetValue(instance, property, value, ignoreCase);
        }

        private static readonly BindingFlags defaultFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
    }
}



