namespace XLoad.Helpers
{
    using System;

    public static class ConfigHelper
    {
        public static string GetStrFromArgs(string[] args, string placeholder, string current)
        {
            var index = Array.IndexOf(args, placeholder);

            if (index < 0 || index + 1 >= args.Length)
            {
                return current;
            }

            if (args[index + 1].StartsWith("-"))
            {
                return current;
            }

            return args[index + 1];
        }

        public static int? GetIntFromArgs(this string[] args, string placeholder, int? current)
        {
            var index = Array.IndexOf(args, placeholder);

            if (index >= 0 && 
                index + 1 < args.Length && 
                int.TryParse(args[index + 1], out int value))
            {
                return value;
            }

            return current;
        }

        public static float? GetFloatFromArgs(this string[] args, string placeholder, float? current)
        {
            var index = Array.IndexOf(args, placeholder);

            if (index >= 0 && 
                index + 1 < args.Length && 
                float.TryParse(args[index + 1], out float value))
            {
                return value;
            }

            return current;
        }

        public static bool? GetBoolFromArgs(this string[] args, string placeholder, bool? current)
        {
            var index = Array.IndexOf(args, placeholder);

            if (index >= 0 &&
                index + 1 < args.Length &&
                bool.TryParse(args[index + 1], out bool value))
            {
                return value;
            }

            return current;
        }

        public static float? GetEnvironmentFloat(string var, float? current)
        {
            var str = Environment.GetEnvironmentVariable(var);

            if (string.IsNullOrWhiteSpace(str))
            {
                return current;
            }

            if (!float.TryParse(str, out float value))
            {
                return current;
            }

            return value;
        }

        public static string GetEnvironmentStr(string var, string current)
        {
            var str = Environment.GetEnvironmentVariable(var);

            if (string.IsNullOrWhiteSpace(str))
            {
                return current; 
            }

            return str;
        }

        public static int? GetEnvironmentInt(string var, int? current)
        {
            var str = Environment.GetEnvironmentVariable(var);

            if (string.IsNullOrWhiteSpace(str))
            {
                return current;
            }

            if(int.TryParse(str, out int value))
            {
                return current;
            }

            return value;
        }

        public static bool? GetEnvironmentBool(string var, bool? current)
        {
            var str = Environment.GetEnvironmentVariable(var);

            if (string.IsNullOrWhiteSpace(str))
            {
                return current;
            }

            if(bool.TryParse(str, out bool value))
            {
                return current;
            }

            return value;
        }
    }
}
