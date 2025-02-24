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
                    _stopwords = new HashSet<string>(); // Tránh lỗi file không tồn tại
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

            string cleanedName = Regex.Replace(productName.ToLower(), @"[^a-z0-9\s]", "");
            var words = cleanedName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                   .Where(w => !_stopwords.Contains(w))
                                   .ToList();

            var keywords = new HashSet<string>(words);

            // Tạo từ viết tắt
            if (words.Count > 1)
            {
                var abbreviation = string.Concat(words.Select(w => w[0]));
                keywords.Add(abbreviation);
            }

            // Kết hợp từ thành cụm
            for (int i = 0; i < words.Count; i++)
            {
                for (int j = i + 1; j < words.Count; j++)
                {
                    keywords.Add($"{words[i]} {words[j]}");
                }
            }

            return keywords.ToList();
        }
    }
}
