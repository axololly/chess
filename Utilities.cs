using System.Runtime.Serialization;
using Chess.Bitboards;

namespace Chess.Utilities
{
    public struct Config
    {
        public static bool ColourBitboards = true;
        public static bool UseZeroes       = false;
    }

    public struct TerminalColours
    {
        public static string RED = "\u001b[31m";
        public static string GREEN = "\u001b[32m";
        public static string WHITE = "\u001b[37m";
    }

    public class Display
    {
        private static string BIT_ON  = Config.ColourBitboards ? TerminalColours.GREEN + "1 " : "1 ";
        private static string BIT_OFF = Config.UseZeroes ? (
            Config.ColourBitboards ? TerminalColours.RED + "0 " : "0 ")
        : (
            Config.ColourBitboards ? TerminalColours.RED + ". " : ". "
        );

        public static string StringifyBitboard(Bitboard bitboard)
        {
            string[] board = new string[8];

            for (int i = 0; i < 8; i++)
            {
                string line = "";

                for (int j = 0; j < 8; j++)
                {
                    ulong sq = 1UL << i * 8 + j;

                    line += bitboard & sq ? BIT_ON : BIT_OFF;
                }

                board[7 - i] = line;
            }

            return string.Join('\n', board) + TerminalColours.WHITE;
        }

        public static void PrintBitboard(ulong bitboard)
        {
            Console.WriteLine(StringifyBitboard(bitboard));
        }

        public static string StringifyMultipleBitboards(Bitboard[] bitboards, string[]? labels = null)
        {
            List<string[]> boardsToPrint = [];
            
            string finalResult = "";

            if (labels != null)
            {
                foreach (string label in labels)
                {
                    string convertedLabel = label;

                    if (label.Length > 17) convertedLabel = label[.. 13] + "...:";

                    finalResult += convertedLabel + new string(' ', 17 - convertedLabel.Length) + "   ";
                }

                finalResult += '\n';
            }

            foreach (Bitboard bitboard in bitboards)
            {
                string[] board = new string[8];

                for (int i = 0; i < 8; i++)
                {
                    string line = "";

                    for (int j = 0; j < 8; j++)
                    {
                        ulong sq = 1UL << i * 8 + j;

                        line += bitboard & sq ? BIT_ON : BIT_OFF;
                    }

                    board[7 - i] = line;
                }

                boardsToPrint.Add(board);
            }

            for (int line = 0; line < 8; line++)
            {
                List<string> linesToPrint = boardsToPrint.Select(board => board[line]).ToList();
                
                string lineToPrint = string.Join("    ", linesToPrint);

                finalResult += lineToPrint + (line < 7 ? "\n" : "");
            }

            return finalResult + TerminalColours.WHITE;
        }

        public static void PrintBitboards(Bitboard[] bitboards, string[]? labels = null)
        {
            Console.WriteLine(StringifyMultipleBitboards(bitboards, labels));
        }
    }


    public class BoardFile
    {
        public static readonly ulong A = 0x101010101010101;
        public static readonly ulong B = 0x202020202020202;
        public static readonly ulong C = 0x404040404040404;
        public static readonly ulong D = 0x808080808080808;
        public static readonly ulong E = 0x1010101010101010;
        public static readonly ulong F = 0x2020202020202020;
        public static readonly ulong G = 0x4040404040404040;
        public static readonly ulong H = 0x8080808080808080;

        
        public static readonly ulong AB = A | B;
        public static readonly ulong GH = G | H;
    }

    public class BoardRank
    {
        public static readonly ulong First = 0xFF;
        public static readonly ulong Second = 0xFF00;
        public static readonly ulong Third = 0xFF0000;
        public static readonly ulong Fourth = 0xFF000000;
        public static readonly ulong Fifth = 0xFF00000000;
        public static readonly ulong Sixth = 0xFF0000000000;
        public static readonly ulong Seventh = 0xFF000000000000;
        public static readonly ulong Eighth = 0xFF00000000000000;
    }

    public enum Colour
    {
        White,
        Black
    }
}