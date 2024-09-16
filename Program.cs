using Chess;
using Chess.MoveGen;
using Types.Bitboards;
using Chess.Utilities;
using Chess.Perft;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;

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

class Program
{
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
            
            for (int depth = 1; depth < maxDepth + 1; depth++) // search from depth 1 to 4 (efficiency)
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

    static int Verify(Board board1, Board board2)
    {
        // Check if both boards are equal by sequence
        if (!Enumerable.SequenceEqual(board1.BoardArray, board2.BoardArray))
        {
            return 1;
        }
        
        // Check all the bitboards
        if (board1.White.Pawns    !=  board2.White.Pawns)    return 2;
        if (board1.White.Bishops  !=  board2.White.Bishops)  return 3;
        if (board1.White.Knights  !=  board2.White.Knights)  return 4;
        if (board1.White.Rooks    !=  board2.White.Rooks)    return 5;
        if (board1.White.Queens   !=  board2.White.Queens)   return 6;
        if (board1.White.King     !=  board2.White.King)     return 7;

        if (board1.Black.Pawns    !=  board2.Black.Pawns)    return 8;
        if (board1.Black.Bishops  !=  board2.Black.Bishops)  return 9;
        if (board1.Black.Knights  !=  board2.Black.Knights)  return 10;
        if (board1.Black.Rooks    !=  board2.Black.Rooks)    return 11;
        if (board1.Black.Queens   !=  board2.Black.Queens)   return 12;
        if (board1.Black.King     !=  board2.Black.King)     return 13;

        if (board1.checkmask      !=  board2.checkmask)      return 14;
        if (board1.pinD           !=  board2.pinD)           return 15;
        if (board1.pinHV          !=  board2.pinHV)          return 16;

        if (board1.epSquare       !=  board2.epSquare)       return 17;
        if (board1.checkers       !=  board2.checkers)       return 18;

        if (board1.castlingRights !=  board2.castlingRights) return 19;

        if (board1.SideToMove     !=  board2.SideToMove)     return 20;

        return 0;
    }

    static string TranslateVerifyCode(int result)
    {        
        if (result == 0) return "Both the mailbox and all bitboards are entirely equal.";

        Dictionary<int, string> responses = new()
        {
            {1, "array"},
            
            {2, "white pawn bitboard"}, {3, "white bishop bitboard"}, {4, "white knight bitboard"},
            {5, "white rook bitboard"}, {6, "white queen bitboard"}, {7, "white king bitboard"},

            {8, "black pawn bitboard"}, {9, "black bishop bitboard"}, {10, "black knight bitboard"},
            {11, "black rook bitboard"}, {12, "black queen bitboard"}, {13, "black king bitboard"},

            {14, "checkmask"}, {15, "diagonal pinmask"}, {16, "orthogonal pinmask"},

            {17, "en-passant square"}, {18, "checkers bitboard"}, {19, "castling rights"},

            {20, "side-to-move"}
        };

        return $"Board 1 and Board 2's {responses[result]} are unequal.";
    }

    public static string StringAll(Board x)
    {
        return '\n' + Display.StringifyMultipleBitboards(
            [
                x.White.Pawns, x.White.Knights, x.White.Bishops, x.White.Rooks, x.White.Queens, x.White.King,
                x.checkmask
            ],
            x, [
                "White Pawns", "White Knights", "White Bishops", "White Rooks", "White Queens", "White King",
                "Checkmask"
            ], 1
        ) + "\n\n" + Display.StringifyMultipleBitboards(
            [
                x.Black.Pawns, x.Black.Knights, x.Black.Bishops, x.Black.Rooks, x.Black.Queens, x.Black.King,
                x.checkmask
            ],
            x, [
                "Black Pawns", "Black Knights", "Black Bishops", "Black Rooks", "Black Queens", "Black King",
                "Checkmask"
            ], 1
        );
    }

    public static bool CopyVerifyPerftTest(Board board, int depth)
    {
        if (depth == 0) return false;

        Board boardAtEntry = new(board.FEN);

        foreach (Move next in board.GenerateLegalMoves())
        {
            Board boardBeforeMove = new(board.FEN);

            try
            {
                board.MakeMove(next);
            }
            catch
            {
                Console.WriteLine($"Move: {next}  |  Move stack: [{string.Join(", ", board.moveHistory.Reverse())}]\nBoard:\n{board}\n\nBoard before:\n{StringAll(boardBeforeMove)}\n\nBoard at start:\n{StringAll(boardAtEntry)}\n[ Error overriden. ]");
                break;
            }
            board.UndoMove();

            int error = Verify(boardBeforeMove, board);

            if (error != 0)
            {
                Console.WriteLine($"{TranslateVerifyCode(error)}\n\nBoard at start:\n{StringAll(boardAtEntry)}\n\nBoard before move:\n\n{StringAll(boardBeforeMove)}\n\nBoard now:\n{StringAll(board)}");
                Environment.Exit(0);
            }

            board.MakeMove(next);
            CopyVerifyPerftTest(board, depth - 1);
            board.UndoMove();
        }
        
        return false;
    }

    static void Main()
    {
        RunTests(4);
    }
}