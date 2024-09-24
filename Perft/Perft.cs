using System.Diagnostics;
using Chess.MoveGen;
using Chess960;
using Newtonsoft.Json;
using System.Text;
using Chess.Utilities;

namespace Chess.Perft
{
    public struct GameData
    {
        public string FEN { get; set; }
        public Dictionary<int, ulong> Depths { get; set; }

        public override string ToString() => $"<GameData FEN = {FEN}, depths = {string.Join(", ", Depths)}>";
    }

    public struct UnitTestResult
    {
        public string FEN { get; set; }
        public int Depth { get; set; }
        public ulong ExpectedNodes { get; set; }
        public ulong? ReturnedNodes { get; set; }
        public bool PassedTest { get { return ExpectedNodes == ReturnedNodes; } }
        public double? TimeTaken { get; set; }
        public string? ErrorMessage { get; set; }
    }

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

        public static ulong MoveByMovePerft(Board board, int depth, bool bulk = true)
        {
            var nextMoves = board.GenerateLegalMoves();
            ulong total = 0;

            foreach (Move next in nextMoves)
            {                
                board.MakeMove(next);

                ulong nodes = BasePerftTest(board, depth - 1, bulk);
                total += nodes;
                
                Console.WriteLine($"{next} - {nodes}");

                board.UndoMove();
            }

            Console.WriteLine($"Total: {total}");

            return total;
        }

        public static ulong TimedMoveByMovePerft(Board board, int depth, bool bulk = true)
        {
            var nextMoves = board.GenerateLegalMoves();
            ulong total = 0;
            
            Stopwatch sw = new();
            
            long totalMilliseconds = 0;

            foreach (Move next in nextMoves)
            {
                board.MakeMove(next);

                sw.Reset();

                sw.Start();
                ulong nodes = BasePerftTest(board, depth - 1, bulk);
                sw.Stop();

                total += nodes;
                totalMilliseconds += sw.ElapsedMilliseconds;
                
                Console.WriteLine($"{next} - {nodes}  [taken {sw.ElapsedMilliseconds}ms, {nodes / ((double)sw.ElapsedMilliseconds / 1000)} nps]");

                board.UndoMove();
            }

            Console.WriteLine($"\nTotal nodes: {total}\nTotal time: {totalMilliseconds / 1000}s\nTotal nps: {total / ((double)totalMilliseconds / 1000)}");

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

        static void RunTests(int maxDepth, int[]? blockPositions = null, int startAt = 0)
        {
            blockPositions ??= [];

            Console.WriteLine("Loading tests...");

            FileStream fs = new("perft.json", FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer);

            string content = Encoding.UTF8.GetString(buffer);

            GameData[]? json = JsonConvert.DeserializeObject<GameData[]>(content) ?? throw new Exception("JSON was null.");

            Console.WriteLine("Finished loading tests.");

            int correct = 0;
            int incorrect = 0;
            int errors = 0;

            List<UnitTestResult> unitTestResults = [];

            // --------------------------------------------------

            for (int i = startAt; i < json.Length; i++)
            {
                if (blockPositions.Contains(i + 1)) continue;
                
                GameData entry = json[i];

                Board board = new(entry.FEN);

                Stopwatch sw = new();
                
                for (int depth = 1; depth < maxDepth + 1; depth++)
                {
                    Console.Clear();
                    Console.WriteLine($"Position {i + 1}:\n----------{new string('-', $"{i + 1}".Length)}\nFEN: {entry.FEN}\nBoard:\n{board}\n\n");

                    sw.Reset();
                    ulong expected = entry.Depths[depth - 1];

                    sw.Start();
                    ulong returned = Perft.BasePerftTest(board, depth);
                    sw.Stop();

                    if (expected != returned)
                    {
                        throw new Exception($"incorrect amount returned - expected {expected} and got back {returned}. [Failed depth {depth}]");
                    }

                    double time = (double)sw.ElapsedMilliseconds / 1000;

                    unitTestResults.Add(
                        new UnitTestResult()
                        {
                            FEN = entry.FEN,
                            Depth = depth,
                            ExpectedNodes = expected,
                            ReturnedNodes = returned,
                            TimeTaken = time,
                            ErrorMessage = null
                        }
                    );

                    if (returned == expected) correct++; else incorrect++;
                }
            }

            Console.WriteLine($"Finished tests!\n\nCorrect: {correct}\nIncorrect: {incorrect}\nErrors: {errors}\n");

            Console.WriteLine("Uploading to JSON file...");

            string JSONresults = JsonConvert.SerializeObject(unitTestResults, Formatting.Indented);

            File.WriteAllText("results.json", JSONresults);

            Console.WriteLine("Finished writing to JSON file!");
        }
    }

    public static class Perft960
    {
        public static ulong BasePerftTest(Board960 board, int depth, bool bulk = true)
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

        public static ulong MoveByMovePerft(Board960 board, int depth, bool bulk = true)
        {
            var nextMoves = board.GenerateLegalMoves();
            ulong total = 0;

            for (int i = 0; i < nextMoves.Count; i++)
            {
                Move next = nextMoves[i];

                board.MakeMove(next);

                ulong nodes = BasePerftTest(board, depth - 1, bulk);
                total += nodes;

                board.UndoMove();
            }

            return total;
        }

        public static ulong TimedMoveByMovePerft(Board960 board, int depth, bool bulk = true)
        {
            var nextMoves = board.GenerateLegalMoves();
            ulong total = 0;
            
            Stopwatch sw = new();
            
            long totalMilliseconds = 0;

            foreach (Move next in nextMoves)
            {
                board.MakeMove(next);

                sw.Reset();

                sw.Start();
                ulong nodes = BasePerftTest(board, depth - 1, bulk);
                sw.Stop();

                total += nodes;
                totalMilliseconds += sw.ElapsedMilliseconds;
                
                Console.WriteLine($"{next} - {nodes}  [taken {sw.ElapsedMilliseconds}ms, {nodes / ((double)sw.ElapsedMilliseconds / 1000)} nps]");

                board.UndoMove();
            }

            Console.WriteLine($"\nTotal nodes: {total}\nTotal time: {totalMilliseconds / 1000}s\nTotal nps: {total / ((double)totalMilliseconds / 1000)}");

            return total;
        }

        public static void OutputPerftTest(Board960 board, int depth, bool bulk = true, string outputPath = "output.txt")
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

        public static void GlobalPerft(Board960 board, int maxDepth, bool bulk = true)
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