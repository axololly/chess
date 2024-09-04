using System.Diagnostics;
using ChessBoard;
using MoveGeneration;

namespace PerftUtils
{
    public static class Perft
    {
        static int perftTest(Board board, int depth, bool bulk = true)
        {
            if (depth == 0) return 1;

            var nextMoves = board.GenerateLegalMoves();

            if (bulk && depth == 1) return nextMoves.Count;

            int count = 0;

            foreach (Move next in nextMoves)
            {
                board.MakeMove(next);
                count += perftTest(board, depth - 1, bulk);
                board.UndoMove();
            }

            return count;
        }

        public static int MoveByMovePerft(Board board, int depth, bool bulk = true)
        {
            var nextMoves = board.GenerateLegalMoves();

            int total = 0;

            foreach (Move next in nextMoves)
            {
                board.MakeMove(next);

                int nodes = perftTest(board, depth - 1, bulk);
                total += nodes;
                
                Console.WriteLine($"{next} - {nodes}");

                board.UndoMove();
            }

            return total;
        }

        public static void OutputPerftTest(Board board, int depth, bool bulk = true, string outputPath = "output.txt")
        {
            TextWriter tmp = Console.Out;
            FileStream fs = new(outputPath, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new(fs);
            Console.SetOut(sw);

            var watch = Stopwatch.StartNew();
            
            int total = MoveByMovePerft(board, depth, bulk);
            
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
    }
}