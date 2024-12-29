using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenKh.Tools.ModsManager.Services
{
    public class KeywordsMatcherService
    {
        public Func<string, bool> CreateMatcher(string inputKeywords)
        {
            var inputKeywordArray = Regex.Split(inputKeywords.ToLowerInvariant(), "\\s+")
                .Where(it => it.Length != 0)
                .ToArray();

            return testKeywords =>
            {
                testKeywords = testKeywords.ToLowerInvariant();
                return inputKeywordArray.All(it => 0 <= testKeywords.IndexOf(it));
            };
        }
    }
}
