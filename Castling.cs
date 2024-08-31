using ChessBoard;
using Utilities;

namespace Castling
{
    public static class CastlingRights
    {
        private static ulong WhiteQSC = 0b00001110UL;
        private static ulong WhiteKSC = 0b01100000UL;
        private static ulong BlackKSC = 0b00001110UL << 56;
        private static ulong BlackQSC = 0b01100000UL << 56;

        public static bool CanCastle(ulong castlingRegion, PieceSet friendlyPieces, PieceSet opponentPieces)
        {
            ulong boardMask = friendlyPieces.ALL() | opponentPieces.ALL();
            ulong opponentAttacks = opponentPieces.AttackingBitmask(boardMask);

            return (opponentAttacks & castlingRegion) == 0
                && (boardMask       & castlingRegion) == 0;
        }

        public static bool CanCastleQueenside(Colour sideToMove, PieceSet friendlyPieces, PieceSet opponentPieces)
        {
            // Console.WriteLine("Checked for castling queenside.");
            return CanCastle(sideToMove == Colour.White ? WhiteQSC : BlackQSC, friendlyPieces, opponentPieces);
        }

        public static bool CanCastleKingside(Colour sideToMove, PieceSet friendlyPieces, PieceSet opponentPieces)
        {
            // Console.WriteLine("Checked for castling kingside.");
            return CanCastle(sideToMove == Colour.White ? WhiteKSC : BlackKSC, friendlyPieces, opponentPieces);
        }
    }
}