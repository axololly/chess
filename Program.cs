using System.Numerics;
using Chess;
using Chess.Bitmasks;
using Chess.MoveGen;

class ZobristTest
{
    static string ToHex(ulong n)
    {
        ulong bits;
        char hexNibble;
        string result = "";

        int limit = 63 - BitOperations.LeadingZeroCount(n);
        if (limit % 4 > 0) limit += 4 - limit % 4;

        for (int i = limit; i >= -1; i -= 4)
        {
            bits = n >> i & 0xF;
            hexNibble = (char)(bits >= 10 ? 'A' + (bits % 10) : '0' + bits);
            result += hexNibble;
        }

        return $"0x{result}UL";
    }

    static void Main()
    {
        Board board = new("6k1/8/5Q2/8/8/8/8/7K w - - 1 1");

        string moves = "f6g6, g8f8, g6f6, f8g8, f6g6, g8f8, g6f6, f8g8";

        foreach (string m in moves.Split(", "))
        {
            board.MakeMove(Move.FromString(m));
        }

        Console.WriteLine($"{board.ColourToMove} to play. [Turn {(board.moveCounter & ~1) >> 1}]\nBoard:\n{board}\n\nPrevious hashes: [{string.Join(", ", board.PastZobristHashes.Select(ToHex))}]\nViolated repetition rule? {(board.ViolatedRepetitionRule() ? "Yes." : "No.")}");
    }
}