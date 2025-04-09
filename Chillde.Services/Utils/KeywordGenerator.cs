using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chillde.Services.Utils
{
    public class KeywordGenerator
    {
        private readonly HashSet<string> _stopwords;

        public KeywordGenerator()
        {
            try
            {
                if (File.Exists("StopWords.txt"))
                    _stopwords = new HashSet<string>(File.ReadAllLines("StopWords.txt"));
                else
                    _stopwords = new HashSet<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading stop words: {ex.Message}");
                _stopwords = new HashSet<string>();
            }
        }

        public List<string> GenerateKeywords(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return new List<string>();

            string cleanedName = Regex.Replace(productName.ToLower(), @"[^\p{L}\p{Nd}\s]", "").Trim();

            var allWords = cleanedName.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            var filteredWords = allWords.Where(w => !_stopwords.Contains(w)).ToList();

            var keywords = new HashSet<string>();

            keywords.Add(string.Join(" ", filteredWords));

            if (filteredWords.Count > 1)
            {
                var abbreviation = string.Concat(filteredWords.Select(w => w[0]));
                keywords.Add(abbreviation);
            }

            for (int len = 1; len <= filteredWords.Count; len++)
            {
                for (int i = 0; i <= filteredWords.Count - len; i++)
                {
                    var phrase = string.Join(" ", filteredWords.Skip(i).Take(len));
                    keywords.Add(phrase);
                }
            }

            var englishWords = filteredWords.Where(w => Regex.IsMatch(w, @"^[a-zA-Z]+$")).ToList();
            var vietnameseWords = filteredWords.Except(englishWords).ToList();

            keywords.Add(string.Join(" ", englishWords));
            keywords.Add(string.Join(" ", vietnameseWords));

            return keywords
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Distinct()
                .ToList();
        }


    }
}
