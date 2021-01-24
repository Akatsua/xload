namespace XLoad.Helpers
{
    using System;
    using System.Linq;

    public static class ArgumentsExtension
    {
        public static bool GetStrArg(this string[] args, string placeholder, out string value, string defaultValue)
        {
            var index = Array.IndexOf(args, placeholder);

            value = defaultValue;

            if (index < 0 || index + 1 >= args.Length)
            {
                return false;
            }

            if (args[index + 1].StartsWith("-"))
            {
                return false;
            }

            value = args[index + 1];
            return true;
        }

        public static bool GetIntArg(this string[] args, string placeholder, out int value, int defaultValue)
        {
            var index = Array.IndexOf(args, placeholder);
            
            value = defaultValue;

            return !(index < 0 || index + 1 >= args.Length || int.TryParse(args[index + 1], out value));
        }

        public static bool GetFloatArg(this string[] args, string placeholder, out float value, float defaultValue)
        {
            var index = Array.IndexOf(args, placeholder);

            value = defaultValue;

            return !(index < 0 || index + 1 >= args.Length || float.TryParse(args[index + 1], out value));
        }

        public static bool GetArgExists(this string[] args, string placeholder)
        {
            return args.Contains(placeholder);
        }
    }
}
