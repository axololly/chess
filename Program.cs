using ChessBoard;
using MoveGeneration;
using Utilities;
using System.Numerics;
using Bitboards;
using System.Diagnostics;

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

        var watch = Stopwatch.StartNew();
        
        int total = MoveByMovePerft(depth);
        
        watch.Stop();

        double time = watch.ElapsedMilliseconds; // get time in seconds

        sw.Close();

        Console.SetOut(tmp);

        Console.WriteLine("Finished outputting perft test.");
        Console.WriteLine($"Perft test at depth {depth}: {total} moves found.");
        Console.WriteLine();
        
        Console.WriteLine("Statistics:\n-----------\n");
        Console.WriteLine($"Time elapsed: {time}ms.");
        Console.WriteLine($"Moves played: {total}.");
        Console.WriteLine($"Nodes/second: {(int)(total / time * 1000)}.");

        Console.WriteLine();
        Console.WriteLine($"Board:\n{board}");
    }

    static void Main()
    {
        Move[] moves = [
            /*
            Move.FromString("e2e3"),
            Move.FromString("e7e6"),
            Move.FromString("d1h5")
            */
        ];
        int depth = 6;

        foreach (Move m in moves)
        {
            board.MakeMove(m);
            depth--;
        }
        
        OutputPerftTest(depth, "compare moves/my results.yml");

        /*
        var P = Moves.GetBlackPawnMoves(
            board.White.ALL(),
            board.Black.ALL(),
            board.Black.Pawns,
            board.epSquare,
            board.HV_pinmask,
            board.D_pinmask,
            board.checkmask
        );
        
        Console.WriteLine($"Board:\n{board}\n\nBitboards:");
        Display.PrintMultipleBitboards([
            board.D_pinmask,
            board.HV_pinmask,
            P.SinglePushForward,
        ]);
        */
    }

    public static Board board = new();
}