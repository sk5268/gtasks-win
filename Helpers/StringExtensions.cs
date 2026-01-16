using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Google_Tasks_Client.Helpers
{
    public static class StringExtensions
    {
        private static readonly Regex EmojiRegex = new Regex(@"[\uFE00-\uFE0F]|\p{So}|\p{Cs}", RegexOptions.Compiled);

        public static string StripEmojis(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return EmojiRegex.Replace(text, "").Trim();
        }
    }
}
