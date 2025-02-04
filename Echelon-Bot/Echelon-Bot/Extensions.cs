using System.Text;

namespace EchelonBot
{
    public static class Extensions
    {
        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return char.ToUpper(input[0]) + input.Substring(1);
        }

        public static string Prettyfy(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            StringBuilder sb = new();

            string underscoresReplaced = input.Replace('_', ' ');

            string[] splits = input.Split('_');

            foreach (string split in splits)
            {
                sb.Append(split.FirstCharToUpper());
                sb.Append(" ");
            }

            return sb.ToString().TrimEnd();


        }
    }
}
