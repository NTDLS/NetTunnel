using System.Text;

namespace NetTunnel.UI.Helpers
{
    internal static class TextHelpers
    {
        internal static string InsertLineBreaks(string text, int maxLineLength)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            var words = text.Split(' ');
            var stringBuilder = new StringBuilder();
            var currentLineLength = 0;

            foreach (var word in words)
            {
                if (currentLineLength + word.Length + 1 > maxLineLength)
                {
                    stringBuilder.AppendLine();
                    currentLineLength = 0;
                }

                if (currentLineLength > 0)
                {
                    stringBuilder.Append(' ');
                    currentLineLength++;
                }

                stringBuilder.Append(word);
                currentLineLength += word.Length;
            }

            return stringBuilder.ToString();
        }
    }
}
