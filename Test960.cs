using Chess.Perft;
using Chess.MoveGen;
using Chess960;
using Chess960.Castling;
using Chess.Utilities;
using System.Diagnostics;

class Program
{
    static void x()
    {
        // Test();
        // PerftTest();

        Board960 board = new();

        board.MakeMove(Move.FromString("b1a3"));
        board.MakeMove(Move.FromString("b8a6"));
        
        board.MakeMove(Move.FromString("a1b1"));

        Console.WriteLine($"[ {board.ColourToMove} to play. ]");
        Console.WriteLine($"Board to start with:\n{board}\n\n");

        Console.WriteLine($"Can castle left? {Castling960Moves.CanCastleQueenside(board)}");
        Console.WriteLine($"Can castle right? {Castling960Moves.CanCastleKingside(board)}");

        // Perft960.MoveByMovePerft(board, 1, false);
        // board.UndoMove();

        // board.MakeMove(Move.FromString("g1f3"));
        // Perft960.MoveByMovePerft(board, 1, false);
        // board.UndoMove();

        // Console.WriteLine($"Board:\n------\n{board}\n\nNext moves: [{string.Join(", ", board.GenerateLegalMoves())}]");

        // Perft960.MoveByMovePerft(board, 5 - board.moveHistory.Count);
    }

    static void Test()
    {
        Board960 board = new("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w HAha - 0 1");

        // string title = "Starting Perft test on Depth 5:";
        // Console.WriteLine($"{title}\n{new string('-', title.Length)}");
        
        Perft960.MoveByMovePerft(board, 5, silence: true, stopBefore: "e5g6"); // Error here: [e5g6, a8c8, h8g6, h8h4]

        // Console.WriteLine($"{new string('-', title.Length)}\nConcluded Perft Test!\n{new string('-', title.Length)}");

        board.MakeMove(Move.FromString("e5g6"));

        board.MakeMove(Move.FromString("a8b8"));
        Perft960.MoveByMovePerft(board, 5 - board.moveHistory.Count, silence: true, stopBefore: "g6h8");

        Console.WriteLine($"Board after:\n{board}\n");
    }

    static void n()
    {
        // Test();

        Console.WriteLine($"[ Process started ]\n");

        Board960 board = new("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w HAha - 0 1");

        Console.WriteLine($"[1] Board:\n{board}\nStarts: [{string.Join(", ", board.rookStarts)}]\nRights: {board.castlingRights}\n");

        Perft960.MoveByMovePerft(board, 5, silence: true, stopBefore: "e5g6");
        board.MakeMove(Move.FromString("e5g6"));

        Console.WriteLine($"[2] Board:\n{board}\nRights: {board.castlingRights}\n");

        board.MakeMove(Move.FromString("a8b8"));

        Console.WriteLine($"[3] Board:\n{board}\nRights: {board.castlingRights}\n");

        Perft960.MoveByMovePerft(board, 3, silence: true, stopBefore: "g6h8");
        
        board.MakeMove(Move.FromString("g6h8"));

        Console.WriteLine($"[4] Board:\n{board}\nRights: {board.castlingRights}\n");

        // Perft960.MoveByMovePerft(board, 2, silence: false, includeBoard: true);

        // Console.WriteLine($"[X] Board:\n{board}\n");

        Console.WriteLine($"Next moves: [{string.Join(", ", board.GenerateLegalMoves())}]");
        Console.WriteLine($"King moves: [{string.Join(", ", board.GenerateLegalMoves().Where(m => m.src == board.Black.KingSquare))}]");
    }

    static void Main()
    {   
        Board960 board = new("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w HAha - 0 1");
        
        Console.WriteLine("Starting Kiwipete on Depth 6:");
        Console.WriteLine("-----------------------------");

        Stopwatch sw = new();

        sw.Start();
        
       ulong nodes = Perft960.MoveByMovePerft(board, 6);

       sw.Stop();

       double seconds = (double)sw.ElapsedMilliseconds / 1000;

       int nps = (int)(nodes / seconds);

        Console.WriteLine("-----------------------------\n");

        Console.WriteLine("Statistics:");
        Console.WriteLine("-----------");
        Console.WriteLine($"Nodes found:  {nodes:,}");
        Console.WriteLine($"Time taken:   {seconds}s");
        Console.WriteLine($"Nodes/second: {nps:,}");

        Console.WriteLine();

        Console.WriteLine("-----------------------------");
    }
}