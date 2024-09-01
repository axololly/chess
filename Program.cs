using ChessBoard;
using MoveGeneration;
using Utilities;
using System.Threading;

class Program
{
    static int perftTest(int depth)
    {
        if (depth == 0) return 1;

        var nextMoves = board.GenerateLegalMoves();

        int count = 0;

        for (int i = 0; i < nextMoves.Count; i++)
        {
            Move next = nextMoves[i];

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

        var nextMoves = board.GenerateLegalMoves();

        for (int i= 0; i < nextMoves.Count; i++)
        {
            Move next = nextMoves[i];

            board.MakeMove(next);

            int nodes = perftTest(depth - 1);
            total += nodes;
            
            // Console.WriteLine($"Move: {next}  |  Explored {nodes} nodes.");
            Console.WriteLine($"{next} - {nodes}");
            // Console.WriteLine($"Board after:\n{board}\n");

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

    static void OutputPerftTest(int depth)
    {
        FileStream fs = new("output.txt", FileMode.Create, FileAccess.Write);
        StreamWriter sw = new(fs);
        Console.SetOut(sw);
        
        Console.WriteLine("Start:");

        MoveByMovePerft(depth);

        Console.WriteLine();

        Console.WriteLine("End:");
        Console.WriteLine(board);

        sw.Close();
    }

    static void Main()
    {
        // OutputPerftTest(1);
        MoveByMovePerft(3);
    }

    public static Board board = new();
}