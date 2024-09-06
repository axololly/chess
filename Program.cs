using PerftUtils;
using ChessBoard;
using MoveGeneration;
using System.Diagnostics;
using Utilities;
using Bitboards;

Board board = new();

Console.WriteLine("Board:");
Console.WriteLine(board);
Console.WriteLine();

Move[] moves = [
    // Move.FromString("e2e4", MoveType.PawnDoublePush)
];

int depth = 7 - moves.Length;

Stopwatch sw = new();

sw.Start();
ulong total = Perft.MoveByMovePerft(board, depth);
sw.Stop();

double time = sw.ElapsedMilliseconds;

Console.WriteLine();

Console.WriteLine("Finished outputting perft test.");
Console.WriteLine($"Perft test at depth {depth}: {total} moves found.");
Console.WriteLine();

Console.WriteLine("Statistics:\n-----------\n");
Console.WriteLine($"Time elapsed: {Math.Round(time / 1000, 2)}s.");
Console.WriteLine($"Moves played: {total}.");
Console.WriteLine($"Nodes/second: {(int)(total / time * 1000)}.");
Console.WriteLine();

Console.WriteLine($"Board after:\n{board}");