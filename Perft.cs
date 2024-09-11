using System.Diagnostics;
using Chess;
using Chess.MoveGen;
using Chess.Utilities;

namespace PerftUtils
{
    public static class Perft
    {
        public static ulong BasePerftTest(Board board, int depth, bool bulk = true)
        {
            if (depth == 0) return 1;

            var nextMoves = board.GenerateLegalMoves();

            if (bulk && depth == 1) return (ulong)nextMoves.Count;

            ulong count = 0;

            foreach (Move next in nextMoves)
            {
                board.MakeMove(next);
                count += BasePerftTest(board, depth - 1, bulk);
                board.UndoMove();
            }

            return count;
        }

        public static ulong MoveByMovePerft(Board board, int depth, bool bulk = true, string pauseBeforeThisMove = "")
        {
            var nextMoves = board.GenerateLegalMoves();
            List<string> results = [];
            ulong total = 0;

            foreach (Move next in nextMoves)
            {
                if (next.ToString() == pauseBeforeThisMove) return 0;
                
                board.MakeMove(next);

                ulong nodes = BasePerftTest(board, depth - 1, bulk);
                total += nodes;
                
                results.Add($"{next} - {nodes}");

                board.UndoMove();
            }

            Console.WriteLine($"Total: {total}\n{string.Join('\n', results)}");

            return total;
        }

        public static void OutputPerftTest(Board board, int depth, bool bulk = true, string outputPath = "output.txt")
        {
            TextWriter tmp = Console.Out;
            FileStream fs = new(outputPath, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new(fs);
            Console.SetOut(sw);

            var watch = Stopwatch.StartNew();
            
            ulong total = MoveByMovePerft(board, depth, bulk);
            
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

        public static void GlobalPerft(Board board, int maxDepth, bool bulk = true)
        {
            Stopwatch sw = new();
            Console.WriteLine($"Running global perft on board:\n{board}\n");

            for (int depth = 1; depth < maxDepth + 1; depth++)
            {
                sw.Reset();

                sw.Start();
                ulong count = BasePerftTest(board, depth, bulk);
                sw.Stop();

                Console.WriteLine($"Depth: {depth}  |  Nodes: {count}  |  Time taken: {sw.ElapsedMilliseconds}ms");
            }
        }
    }
}