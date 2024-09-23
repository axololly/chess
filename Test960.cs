using Chess.Perft;
using Chess.MoveGen;
using Chess960;
using Chess960.Castling;
using Chess.Bitmasks;

class Program
{
    static void PerftTest()
    {
        Board960 board = new("6k1/8/8/8/8/8/8/q1R2K1R w HC - 0 1");

        // board.MakeMove(Move.FromString("h1h8"));
        // board.MakeMove(Move.FromString("g8g7"));
        // board.MakeMove(Move.FromString("h8h1"));

        Console.WriteLine($"Board:\n{board}\n\n");

        Perft960.MoveByMovePerft(board, 5);
    }

    static void Test()
    {
        Board960 board = new("6k1/8/8/8/8/8/8/q1R2K1R w HC - 0 1");

        Move errorMove = board.GenerateLegalMoves().Where(x => x.dst == 15).First();

        Console.WriteLine($"[Move {errorMove}] Board to start with:\n{board}\nRights: {board.castlingRights}\n\n");

        board.MakeMove(errorMove);
        
        Console.WriteLine($"Board after castling:\n{board}\nRights: {board.castlingRights}\n\n");

        board.UndoMove();

        Console.WriteLine($"Board after undoing:\n{board}\nRights: {board.castlingRights}");
    }

    static void _Main()
    {
        // Test();
        // PerftTest();

        Board960 board = new();

        board.MakeMove(Move.FromString("b1a3"));
        board.MakeMove(Move.FromString("b8a6"));
        
        board.MakeMove(Move.FromString("a1b1"));

        Console.WriteLine($"[ {board.ColourToMove} to play. ]");
        Console.WriteLine($"Board to start with:\n{board}\n\n");

        Console.WriteLine($"Can castle left? {Castling960.CanCastleQueenside(board)}");
        Console.WriteLine($"Can castle right? {Castling960.CanCastleKingside(board)}");

        // Perft960.MoveByMovePerft(board, 1, false);
        // board.UndoMove();

        // board.MakeMove(Move.FromString("g1f3"));
        // Perft960.MoveByMovePerft(board, 1, false);
        // board.UndoMove();

        // Console.WriteLine($"Board:\n------\n{board}\n\nNext moves: [{string.Join(", ", board.GenerateLegalMoves())}]");

        // Perft960.MoveByMovePerft(board, 5 - board.moveHistory.Count);
    }

    static void Main()
    {
        // Console.WriteLine(Bitmask.RayBetween(56, 60).ToString());

        Board960 board = new("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w HAha - 0 1");
        
        Perft960.BasePerftTest(board, 4);
    }
}