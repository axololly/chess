using ChessBoard;
using MoveGeneration;
using Utilities;

class Program
{
    static int perftTest(int depth)
    {
        if (depth == 0) return 1;

        var nextMoves = board.GenerateLegalMoves();

        int count = 0;

        foreach (Move next in nextMoves)
        {
            board.MakeMove(next);
            count += perftTest(depth - 1);
            board.UndoMove();
        }

        return count;
    }

    static void MoveByMovePerft(int depth)
    {
        Console.WriteLine(board);
        Console.WriteLine();

        int total = 0;

        foreach (Move next in board.GenerateLegalMoves())
        {
            int nodes = perftTest(depth - 1);
            total += nodes;

            board.MakeMove(next);
            
            Console.WriteLine($"Move: {next}  |  Explored {nodes} nodes.");
            Console.WriteLine($"Board state: {board}");
            Console.WriteLine($"Next moves: [{string.Join(", ", board.GenerateLegalMoves())}]");
            Console.WriteLine();

            board.UndoMove();
        }

        Console.WriteLine();
        Console.WriteLine($"Perft test at depth {depth}: {total} moves found.");
    }

    static void GlobalPerft(int stopAtDepth)
    {
        for (int depth = 1; depth < stopAtDepth + 1; depth++)
        {
            Console.WriteLine($"Perft test at depth {depth}: {perftTest(depth)} moves found.");
        }
    }

    static void OutputPerftTest()
    {
        FileStream fs = new("output.txt", FileMode.Create, FileAccess.Write);
        StreamWriter sw = new(fs);
        Console.SetOut(sw);
        
        Console.WriteLine("Start:");
        Console.WriteLine(board);

        Console.WriteLine();

        MoveByMovePerft(3);

        Console.WriteLine();

        Console.WriteLine("End:");
        Console.WriteLine(board);

        sw.Close();
    }

    static void Main()
    {
        board.MakeMove(Move.FromString("a2a3"));
        MoveByMovePerft(2);
    }

    public static Board board = new();
}