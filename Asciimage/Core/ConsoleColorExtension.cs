using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Asciimage.Core
{
    public static class ConsoleColorExtension
    {
        /// <summary>
        /// ANSI escape sequence: Default foreground color
        /// </summary>
        private const string ANSIDefaultForegroundColor = "\x1b[39m";

        /// <summary>
        /// ANSI escape sequence: Default background color
        /// </summary>
        private const string ANSIDefaultBackgroundColor = "\x1b[49m";

        /// <summary>
        /// Convert ConsoleColor to ANSI escape sequence for foreground color.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ToANSIForegroundColor(this ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Black => "\x1b[30m",
                ConsoleColor.DarkBlue => "\x1b[34m",
                ConsoleColor.DarkGreen => "\x1b[32m",
                ConsoleColor.DarkCyan => "\x1b[36m",
                ConsoleColor.DarkRed => "\x1b[31m",
                ConsoleColor.DarkMagenta => "\x1b[35m",
                ConsoleColor.DarkYellow => "\x1b[33m",
                ConsoleColor.Gray => "\x1b[37m",
                ConsoleColor.DarkGray => "\x1b[90m",
                ConsoleColor.Blue => "\x1b[94m",
                ConsoleColor.Green => "\x1b[92m",
                ConsoleColor.Cyan => "\x1b[96m",
                ConsoleColor.Red => "\x1b[91m",
                ConsoleColor.Magenta => "\x1b[95m",
                ConsoleColor.Yellow => "\x1b[93m",
                ConsoleColor.White => "\x1b[97m",
                _ => ANSIDefaultForegroundColor
            };
        }

        /// <summary>
        /// Convert ConsoleColor to ANSI escape sequence for background color.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ToANSIBackgroundColor(this ConsoleColor color)
        {
            return color switch
            {
                ConsoleColor.Black => "\x1b[40m",
                ConsoleColor.DarkBlue => "\x1b[44m",
                ConsoleColor.DarkGreen => "\x1b[42m",
                ConsoleColor.DarkCyan => "\x1b[46m",
                ConsoleColor.DarkRed => "\x1b[41m",
                ConsoleColor.DarkMagenta => "\x1b[45m",
                ConsoleColor.DarkYellow => "\x1b[43m",
                ConsoleColor.Gray => "\x1b[47m",
                ConsoleColor.DarkGray => "\x1b[100m",
                ConsoleColor.Blue => "\x1b[104m",
                ConsoleColor.Green => "\x1b[102m",
                ConsoleColor.Cyan => "\x1b[106m",
                ConsoleColor.Red => "\x1b[101m",
                ConsoleColor.Magenta => "\x1b[105m",
                ConsoleColor.Yellow => "\x1b[103m",
                ConsoleColor.White => "\x1b[107m",
                _ => ANSIDefaultBackgroundColor
            };
        }
    }
}
