using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chillde.Services.Helpers
{
    public class GenerateCodeHelper
    {
        private static readonly Random _random = new Random();
        private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string GenerateOrderCode()
        {
            const string prefix = "ORDCHD";
            var randomPart = new string(Enumerable.Repeat(Characters, 8)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
            return prefix + randomPart;
        }
    }
}
