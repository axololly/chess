using Types.Bitboards;
using Chess960;

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
        private static string BIT_ON  = Config.ColourBitboards ? TerminalColours.GREEN + "1 " + TerminalColours.WHITE : "1 ";
        private static string BIT_OFF = Config.UseZeroes ? (
            Config.ColourBitboards ? TerminalColours.RED + "0 " + TerminalColours.WHITE : "0 ")
        : (
            Config.ColourBitboards ? TerminalColours.RED + ". " + TerminalColours.WHITE : ". "
        );

        public static string StringifyBitboard(Bitboard bitboard)
        {
            string[] board = new string[8];

            for (int i = 0; i < 8; i++)
            {
                string line = "";

                for (int j = 0; j < 8; j++)
                {
                    Bitboard sq = 1UL << i * 8 + j;

                    line += bitboard & sq ? BIT_ON : BIT_OFF;
                }

                board[7 - i] = line;
            }

            return string.Join('\n', board) + TerminalColours.WHITE;
        }

        public static void PrintBitboard(Bitboard bitboard)
        {
            Console.WriteLine(StringifyBitboard(bitboard));
        }

        public static string StringifyMultipleBitboards(Bitboard[] bitboards, Board? optionalBoard = null, List<string>? labels = null, int spacing = 4)
        {
            List<string[]> boardsToPrint = [];

            if (optionalBoard != null)
            {
                boardsToPrint.Add(optionalBoard.ToString().Split('\n'));
                
                if (labels != null)
                {
                    labels.Insert(0, "Board");
                }
            }
            
            string finalResult = "";

            if (labels != null)
            {
                foreach (string label in labels)
                {
                    string convertedLabel = label;

                    if (label.Length > 16) convertedLabel = label[.. 12] + "...";

                    finalResult += convertedLabel + new string(' ', 16 - convertedLabel.Length) + new string(' ', spacing);
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
                        Bitboard sq = 1UL << i * 8 + j;

                        line += bitboard & sq ? BIT_ON : BIT_OFF;
                    }

                    board[7 - i] = line;
                }

                boardsToPrint.Add(board);
            }

            for (int line = 0; line < 8; line++)
            {
                List<string> linesToPrint = boardsToPrint.Select(board => board[line]).ToList();
                
                string lineToPrint = string.Join(new string(' ', spacing), linesToPrint);

                finalResult += lineToPrint + (line < 7 ? "\n" : "");
            }

            return finalResult + TerminalColours.WHITE;
        }

        public static string StringifyMultipleBitboards960(Bitboard[] bitboards, Board960? optionalBoard = null, List<string>? labels = null, int spacing = 4)
        {
            List<string[]> boardsToPrint = [];

            if (optionalBoard != null)
            {
                boardsToPrint.Add(optionalBoard.ToString().Split('\n'));
                
                if (labels != null)
                {
                    labels.Insert(0, "Board");
                }
            }
            
            string finalResult = "";

            if (labels != null)
            {
                foreach (string label in labels)
                {
                    string convertedLabel = label;

                    if (label.Length > 16) convertedLabel = label[.. 12] + "...";

                    finalResult += convertedLabel + new string(' ', 16 - convertedLabel.Length) + new string(' ', spacing);
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
                        Bitboard sq = 1UL << i * 8 + j;

                        line += bitboard & sq ? BIT_ON : BIT_OFF;
                    }

                    board[7 - i] = line;
                }

                boardsToPrint.Add(board);
            }

            for (int line = 0; line < 8; line++)
            {
                List<string> linesToPrint = boardsToPrint.Select(board => board[line]).ToList();
                
                string lineToPrint = string.Join(new string(' ', spacing), linesToPrint);

                finalResult += lineToPrint + (line < 7 ? "\n" : "");
            }

            return finalResult + TerminalColours.WHITE;
        }

        public static void PrintBitboards(Bitboard[] bitboards, Board? optionalBoard = null, List<string>? labels = null, int spacing = 4)
        {
            Console.WriteLine(StringifyMultipleBitboards(bitboards, optionalBoard, labels, spacing));
        }

        public static void PrintBitboards960(Bitboard[] bitboards, Board960? optionalBoard = null, List<string>? labels = null, int spacing = 4)
        {
            Console.WriteLine(StringifyMultipleBitboards960(bitboards, optionalBoard, labels, spacing));
        }
    }


    public struct Files
    {
        public static readonly Bitboard A = 0x101010101010101;
        public static readonly Bitboard B = 0x202020202020202;
        public static readonly Bitboard C = 0x404040404040404;
        public static readonly Bitboard D = 0x808080808080808;
        public static readonly Bitboard E = 0x1010101010101010;
        public static readonly Bitboard F = 0x2020202020202020;
        public static readonly Bitboard G = 0x4040404040404040;
        public static readonly Bitboard H = 0x8080808080808080;

        
        public static readonly Bitboard AB = A | B;
        public static readonly Bitboard GH = G | H;
    }

    public struct Ranks
    {
        public static readonly Bitboard First = 0xFF;
        public static readonly Bitboard Second = 0xFF00;
        public static readonly Bitboard Third = 0xFF0000;
        public static readonly Bitboard Fourth = 0xFF000000;
        public static readonly Bitboard Fifth = 0xFF00000000;
        public static readonly Bitboard Sixth = 0xFF0000000000;
        public static readonly Bitboard Seventh = 0xFF000000000000;
        public static readonly Bitboard Eighth = 0xFF00000000000000;
    }

    public enum Colour
    {
        White,
        Black
    }
}