using Common.Shared.Exceptions;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Common.Shared
{
    /// <summary>
    /// Utility class providing common conversion methods
    /// </summary>
    public static class MyConverter
    {
        /// <summary>
        /// Validates that an object is not null or empty
        /// </summary>
        /// <param name="variableName">Name of the variable being validated</param>
        /// <param name="value">Value to validate</param>
        /// <exception cref="BadRequestException">Thrown when value is null or empty</exception>
        public static void ValidateNotNullOrEmpty(string variableName, object? value)
        {
            if (value == null)
            {
                throw new BadRequestException($"{variableName} cannot be null");
            }
            
            if (value is string str && string.IsNullOrWhiteSpace(str))
            {
                throw new BadRequestException($"{variableName} cannot be null or empty");
            }
        }

        /// <summary>
        /// Converts a string to its hexadecimal representation
        /// </summary>
        /// <param name="input">Input string to convert</param>
        /// <returns>Hexadecimal representation of the input string</returns>
        public static string ConvertToHex(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var hexBuilder = new StringBuilder();
            foreach (char c in input)
            {
                hexBuilder.Append(((int)c).ToString("X2"));
            }
            return hexBuilder.ToString();
        }

        /// <summary>
        /// Converts a hexadecimal string back to its original string representation
        /// </summary>
        /// <param name="hex">Hexadecimal string to convert</param>
        /// <returns>Original string representation</returns>
        public static string ConvertFromHex(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
                return string.Empty;

            var result = new StringBuilder();
            for (int i = 0; i < hex.Length; i += 2)
            {
                string hexPair = hex.Substring(i, 2);
                int charCode = Convert.ToInt32(hexPair, 16);
                result.Append((char)charCode);
            }
            return result.ToString();
        }

        /// <summary>
        /// Converts an object to JSON string
        /// </summary>
        /// <param name="obj">Object to convert</param>
        /// <param name="indented">Whether to format the JSON with indentation</param>
        /// <returns>JSON string representation of the object</returns>
        public static string ToJson(object obj, bool indented = false)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = indented,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(obj, options);
        }

        /// <summary>
        /// Converts a JSON string to an object of specified type
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <param name="json">JSON string to deserialize</param>
        /// <returns>Deserialized object</returns>
        public static T? FromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(T);

            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (JsonException)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Converts a string to Base64
        /// </summary>
        /// <param name="input">Input string to convert</param>
        /// <returns>Base64 encoded string</returns>
        public static string ToBase64(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Converts a Base64 string back to its original string representation
        /// </summary>
        /// <param name="base64">Base64 string to convert</param>
        /// <returns>Original string representation</returns>
        public static string FromBase64(string base64)
        {
            if (string.IsNullOrEmpty(base64))
                return string.Empty;

            try
            {
                var bytes = Convert.FromBase64String(base64);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Converts a string to title case
        /// </summary>
        /// <param name="input">Input string to convert</param>
        /// <returns>Title case string</returns>
        public static string ToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }

        /// <summary>
        /// Converts a string to snake_case
        /// </summary>
        /// <param name="input">Input string to convert</param>
        /// <returns>Snake case string</returns>
        public static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var result = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]) && i > 0)
                {
                    result.Append('_');
                }
                result.Append(char.ToLower(input[i]));
            }
            return result.ToString();
        }

        /// <summary>
        /// Converts a string to camelCase
        /// </summary>
        /// <param name="input">Input string to convert</param>
        /// <returns>Camel case string</returns>
        public static string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
                return string.Empty;

            var result = new StringBuilder(words[0].ToLower());
            for (int i = 1; i < words.Length; i++)
            {
                result.Append(ToTitleCase(words[i]));
            }
            return result.ToString();
        }

        /// <summary>
        /// Safely converts a string to an integer
        /// </summary>
        /// <param name="input">Input string to convert</param>
        /// <param name="defaultValue">Default value if conversion fails</param>
        /// <returns>Converted integer or default value</returns>
        public static int ToInt(string input, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(input))
                return defaultValue;

            return int.TryParse(input, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// Safely converts a string to a double
        /// </summary>
        /// <param name="input">Input string to convert</param>
        /// <param name="defaultValue">Default value if conversion fails</param>
        /// <returns>Converted double or default value</returns>
        public static double ToDouble(string input, double defaultValue = 0.0)
        {
            if (string.IsNullOrEmpty(input))
                return defaultValue;

            return double.TryParse(input, out double result) ? result : defaultValue;
        }

        /// <summary>
        /// Safely converts a string to a boolean
        /// </summary>
        /// <param name="input">Input string to convert</param>
        /// <param name="defaultValue">Default value if conversion fails</param>
        /// <returns>Converted boolean or default value</returns>
        public static bool ToBool(string input, bool defaultValue = false)
        {
            if (string.IsNullOrEmpty(input))
                return defaultValue;

            return bool.TryParse(input, out bool result) ? result : defaultValue;
        }
    }
}