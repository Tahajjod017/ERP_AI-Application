using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers
{
    public static class DuplicateChecker
    {
        public static string NormalizeString(this string input)
        {
            if (input == null) return null;

            return new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToLowerInvariant();
        }

        public static bool IsDuplicate(string input, IEnumerable<string> existingStrings)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            var normalizedInput = input.NormalizeString();

            return existingStrings.Where(s => !string.IsNullOrWhiteSpace(s)).Any(s => s.NormalizeString() == normalizedInput);
        }
    }
}
