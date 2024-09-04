using PerftUtils;
using Utilities;
using Bitboards;
using ChessBoard;
using MoveGeneration;

Board board = new();

Move[] moves = [
    
];

int depth = 6 - moves.Length;

foreach (Move r in moves) board.MakeMove(r);

Perft.OutputPerftTest(board, depth, outputPath: "compare moves/my results.yml");