using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace Utilities
{
    public class Display
    {
        public static string StringifyBitboard(ulong bitboard)
        {
            string[] board = new string[8];

            for (int i = 0; i < 8; i++)
            {
                string line = "";

                for (int j = 0; j < 8; j++)
                {
                    ulong sq = 1UL << i * 8 + j;

                    if ((bitboard & sq) != 0)
                    {
                        line += "1 ";
                    }
                    else
                    {
                        line += ". ";
                    }
                }

                board[7 - i] = line;
            }

            return string.Join('\n', board);
        }

        public static void PrintBitboard(ulong bitboard)
        {
            Console.WriteLine(StringifyBitboard(bitboard));
        }

        public static void PrintMultipleBitboards(ulong[] bitboards)
        {
            List<string[]> boardsToPrint = [];

            foreach (ulong bitboard in bitboards)
            {
                string[] board = new string[8];

                for (int i = 0; i < 8; i++)
                {
                    string line = "";

                    for (int j = 0; j < 8; j++)
                    {
                        ulong sq = 1UL << i * 8 + j;

                        if ((bitboard & sq) != 0)
                        {
                            line += "1 ";
                        }
                        else
                        {
                            line += ". ";
                        }
                    }

                    board[7 - i] = line;
                }

                boardsToPrint.Add(board);
            }

            for (int line = 0; line < 8; line++)
            {
                List<string> linesToPrint = boardsToPrint.Select(board => board[line]).ToList();
                
                string lineToPrint = string.Join("    ", linesToPrint);

                Console.WriteLine(lineToPrint);
            }
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