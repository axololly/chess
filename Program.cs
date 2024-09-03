using ChessBoard;
using MoveGeneration;
using Utilities;
using System.Numerics;
using Bitboards;

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

    static int MoveByMovePerft(int depth)
    {
        var nextMoves = board.GenerateLegalMoves();

        int total = 0;

        foreach (Move next in nextMoves)
        {
            board.MakeMove(next);

            int nodes = perftTest(depth - 1);
            total += nodes;
            
            Console.WriteLine($"{next} - {nodes}");
            // Console.WriteLine($"Board:\n{board}");
            // Console.WriteLine();

            board.UndoMove();
        }

        return total;
    }

    static void GlobalPerft(int stopAtDepth)
    {
        for (int depth = 1; depth < stopAtDepth + 1; depth++)
        {
            Console.WriteLine($"Perft test at depth {depth}: {perftTest(depth)} moves found.");
        }
    }

    static void OutputPerftTest(int depth, string outputPath = "output.txt")
    {
        TextWriter tmp = Console.Out;
        FileStream fs = new(outputPath, FileMode.Create, FileAccess.Write);
        StreamWriter sw = new(fs);
        Console.SetOut(sw);

        int total = MoveByMovePerft(depth);

        sw.Close();

        Console.SetOut(tmp);

        Console.WriteLine("Finished outputting perft test.");
        Console.WriteLine($"Perft test at depth {depth}: {total} moves found.");

        Console.WriteLine();
        Console.WriteLine($"Board:\n{board}");
    }

    static void Main()
    {
        Move[] moves = [
            Move.FromString("b2b4", MoveType.PawnDoublePush),
            Move.FromString("c7c5", MoveType.PawnDoublePush),
            Move.FromString("d2d4", MoveType.PawnDoublePush),
            Move.FromString("d8a5")
        ];
        int depth = 5;

        foreach (Move m in moves)
        {
            board.MakeMove(m);
            depth--;
        }
        
        OutputPerftTest(depth, "compare moves/my results.yml");
        // Display.PrintMultipleBitboards([board.checkmask]);
    }

    // public static Board board = new(File.ReadAllText("board.fen"));
    public static Board board = new();
}