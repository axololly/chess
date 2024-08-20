using MagicBitboards;

public class Program
{
    public static void Main()
    {
        ulong exampleOccupancy = 0b0101011011010110010111100101011001010111010101100111011001010110;

        Display.PrintBitboard(exampleOccupancy);

        Console.WriteLine();

        var moves = SlidingPieceMoves.GetRookMoves(exampleOccupancy, 43);

        Display.PrintBitboard(moves);
    }
}