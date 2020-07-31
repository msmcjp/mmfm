﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFileManager
{
    public static class FileSizeStringExtension
    {
        private static string ThreeNonZeroDigits(double value)
        {
            if (value >= 100)
            {
                // No digits after the decimal.
                return value.ToString("0,0");
            }
            else if (value >= 10)
            {
                // One digit after the decimal.
                return value.ToString("0.0");
            }
            else
            {
                // Two digits after the decimal.
                return value.ToString("0.00");
            }
        }

        public static string ToFileSize(this long value)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            for (int i = 0; i < suffixes.Length; i++)
            {
                if (value <= (Math.Pow(1024, i + 1)))
                {
                    return $"{((i == 0) ? ((int)value).ToString() : ThreeNonZeroDigits(value / Math.Pow(1024, i)))} {suffixes[i]}";
                }
            }

            return $"{ThreeNonZeroDigits(value / Math.Pow(1024, suffixes.Length - 1))} {suffixes[suffixes.Length - 1]}";
        }
    }
}
