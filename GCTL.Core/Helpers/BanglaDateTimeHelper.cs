using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers
{
    public static class BanglaDateHelper
    {
        private static readonly Dictionary<char, char> BanglaDigits = new()
        {
            ['0'] = '০',
            ['1'] = '১',
            ['2'] = '২',
            ['3'] = '৩',
            ['4'] = '৪',
            ['5'] = '৫',
            ['6'] = '৬',
            ['7'] = '৭',
            ['8'] = '৮',
            ['9'] = '৯'
        };

        private static readonly Dictionary<int, string> BanglaMonths = new()
        {
            [1] = "জানুয়ারি",
            [2] = "ফেব্রুয়ারি",
            [3] = "মার্চ",
            [4] = "এপ্রিল",
            [5] = "মে",
            [6] = "জুন",
            [7] = "জুলাই",
            [8] = "আগস্ট",
            [9] = "সেপ্টেম্বর",
            [10] = "অক্টোবর",
            [11] = "নভেম্বর",
            [12] = "ডিসেম্বর"
        };

        public static string ToBanglaDate(this DateTime date)
        {
            string day = ConvertToBanglaDigits(date.Day.ToString("00"));
            string month = BanglaMonths[date.Month];
            string year = ConvertToBanglaDigits(date.Year.ToString());

            return $"{day} {month}, {year}";
        }

        private static string ConvertToBanglaDigits(string input)
        {
            return new string(input.Select(c => BanglaDigits.ContainsKey(c) ? BanglaDigits[c] : c).ToArray());
        }
    }
}
