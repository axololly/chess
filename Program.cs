using MoveGeneration;

class Program
{
    static void Main()
    {
        /*
        Board position:
        . 1 . . . . . . 
        . . . . . . . . 
        . . 1 . . . . .
        . . . 1 . 1 1 1
        . . . . . . 1 .
        1 . . 1 1 1 1 .
        1 1 . . . . . .
        . . . 1 . . . .
        */
        ulong exampleOccupancy = 0x20004E840790308;
        ulong pawnBitboard = 0x780300;

        Console.WriteLine();

        // var moves = Moves.GetWhitePawnMoves(exampleOccupancy, pawnBitboard);

        Display.PrintMultipleBitboards([exampleOccupancy, pawnBitboard]);

        var gen = Moves.GenerateWhitePawnMoves(exampleOccupancy, pawnBitboard);

        Console.WriteLine('[' + string.Join(", ", gen) + ']');
    }
}