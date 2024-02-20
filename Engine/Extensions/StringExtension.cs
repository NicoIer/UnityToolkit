using UnityEngine;

namespace UnityToolkit
{
    public static class StringExtension
    {
        public static string Green(this string s) => s.Color(UnityEngine.Color.green);
        public static string White(this string s) => s.Color(UnityEngine.Color.white);
        public static string Purple(this string s) => s.Color(UnityEngine.Color.magenta);
        public static string Yellow(this string s) => s.Color(UnityEngine.Color.yellow);
        public static string Orange(this string s) => s.Color(new Color(1, 0.5f, 0));
        public static string Pink(this string s) => s.Color(new Color(1, 0.5f, 0.5f));
        public static string Blue(this string s) => s.Color(UnityEngine.Color.blue);
        public static string Red(this string s) => s.Color(UnityEngine.Color.red);
        public static string Sky(this string s) => s.Color(new Color(0.5f, 0.5f, 1));

        public static string Color(this string s, Color color)
        {
            string hex = ColorUtility.ToHtmlStringRGB(color);
            return $"<color=#{hex}>{s}</color>";
        }
    }
}