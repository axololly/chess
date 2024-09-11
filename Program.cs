using Chess;
using Chess.MoveGen;
using Chess.Bitboards;
using Chess.Utilities;
using PerftUtils;
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
    public int FailedDepth { get; set; }
    public ulong ExpectedNodes { get; set; }
    public ulong? ReturnedNodes { get; set; }
    public bool PassedTest { get { return ExpectedNodes == ReturnedNodes; } }
    public double? TimeTaken { get; set; }
    public string? ErrorMessage { get; set; }
}

class Program
{
    static void RunTests(int maxDepth, int[]? blockPositions = null)
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

        for (int i = 0; i < json.Length; i++)
        {
            if (blockPositions.Contains(i + 1)) continue;
            
            GameData entry = json[i];

            Board board = new(entry.FEN);

            Stopwatch sw = new();
            
            for (int depth = 1; depth < maxDepth + 1; depth++) // search from depth 1 to 4 (efficiency)
            {
                Console.Clear();
                Console.WriteLine($"Position {i + 1}:\n----------{new string('-', $"{i + 1}".Length)}\nFEN: {entry.FEN}\nBoard:\n{board}");

                sw.Reset();
                ulong expected = entry.Depths[depth - 1];

                sw.Start();
                ulong returned = Perft.BasePerftTest(board, depth);
                sw.Stop();

                // Console.WriteLine($"\nDepth: {depth} | Expected: {expected} | Returned: {returned}");

                double time = (double)sw.ElapsedMilliseconds / 1000;

                unitTestResults.Add(
                    new UnitTestResult()
                    {
                        FEN = entry.FEN,
                        FailedDepth = depth,
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

    static Board Debug(string path, string FEN)
    {
        // RunTests(4);

        Board board = new(FEN);

        Console.WriteLine($"FEN: {board.FEN}\n\nStarting board:\n{board}\n");

        string[] rawMoves = File.ReadAllText(path).Split(", ");

        List<Move?> moves = [];
        static int convert(string s) => "abcdefgh".IndexOf(s[0]) + "12345678".IndexOf(s[1]) * 8;

        foreach (string moveString in rawMoves)
        {
            if (moveString == "0")
            {
                moves.Add(null);
                continue;
            }

            Move move = new()
            {
                src = convert(moveString.Substring(0, 2)),
                dst = convert(moveString.Substring(2, 2)),
                type = moveString == "e8g8" ? MoveType.Castling : (moveString.Length == 5 ? MoveType.Promotion : MoveType.Normal),
                promoPiece = moveString.Length == 5 ? moveString[4] switch
                {
                    'q' => PromoPiece.Queen,
                    'r' => PromoPiece.Rook,
                    'b' => PromoPiece.Bishop,
                    'n' => PromoPiece.Knight,
                    _ => throw new Exception("i dunno")
                } : PromoPiece.None
            };

            moves.Add(move);
        }

        foreach (Move? move in moves)
        {
            if (move == null)
            {
                var T = board.moveHistory.Pop();
                board.moveHistory.Push(T);

                board.UndoMove();
                Console.WriteLine($"Unmaking move {T}.\nBoard:\n{board}\n");
            }
            else
            {
                board.MakeMove(move);

                Console.WriteLine($"Making move {move}.\nBoard:\n{board}\n");
            }

            Display.PrintBitboards(
                [board.White.Pawns, board.White.Knights, board.White.Bishops, board.White.Rooks, board.White.Queens, board.White.King],
                ["White Pawns", "White Knights", "White Bishops", "White Rooks", "White Queens", "White King"]
            );
            Console.WriteLine();
        }

        Console.WriteLine($"Board now:\n{board}\n");

        return board;
    }

    static void Output(string FEN)
    {
        // RunTests(4);

        FileStream fs = new("output.yml", FileMode.Open, FileAccess.Write);
        TextWriter tmp = Console.Out;
        
        StreamWriter sw = new(fs);
        Console.SetOut(sw);

        Debug("input.txt", FEN);

        Console.SetOut(tmp);

        Console.WriteLine("All done!");

        sw.Close();
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
        if (board1.D_pinmask      !=  board2.D_pinmask)      return 15;
        if (board1.HV_pinmask     !=  board2.HV_pinmask)     return 16;

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
            catch (Exception e)
            {
                if (!e.Message.Contains("cannot obtain bitboard"))
                {
                    throw;
                }

                Console.WriteLine($"Move: {next}\nBoard:\n{board}\n\n[ Error overriden. ]");
                break;
            }
            board.UndoMove();

            int error = Verify(boardBeforeMove, board);

            if (error != 0)
            {
                Console.WriteLine($"{TranslateVerifyCode(error)}\n\nBoard at start:\n{boardAtEntry}\n\nBoard before move:\n\n{boardBeforeMove}\n\nBoard now:\n{board}");
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
        CopyVerifyPerftTest(new Board("r1b1k3/p2p1Nr1/3b3p/1n1pp1pP/2BB1P2/P6R/Q1P3P1/R3K1N1 w Qq - 0 1"), 4);
    }
}