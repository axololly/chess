using Types.Bitboards;
using Chess.Utilities;

namespace Chess.Castling
{
    public struct CastlingMoves
    {
        public static Bitboard OccupyingWhiteQSC = 0b00001110UL;
        public static Bitboard MovementWhiteQSC  = 0b00001100UL;
        public static Bitboard WhiteKSC = 0b01100000UL;

        public static Bitboard OccupyingBlackQSC = OccupyingWhiteQSC << 56;
        public static Bitboard MovementBlackQSC  = MovementWhiteQSC  << 56;
        public static Bitboard BlackKSC = WhiteKSC << 56;

        public static bool CanCastle(Bitboard occpuyingRegion, Bitboard movementRegion, PieceSet friendlyPieces, PieceSet opponentPieces)
        {
            Bitboard boardMask = friendlyPieces.Mask | opponentPieces.Mask;
            Bitboard opponentAttacks = opponentPieces.AttackingBitmask(friendlyPieces.Mask);

            return !(opponentAttacks & movementRegion)
                && !(boardMask       & occpuyingRegion);
        }

        public static bool CanCastleQueenside(Colour sideToMove, PieceSet friendlyPieces, PieceSet opponentPieces)
        {
            return CanCastle(
                sideToMove == Colour.White ? OccupyingWhiteQSC : OccupyingBlackQSC,
                sideToMove == Colour.White ? MovementWhiteQSC : MovementBlackQSC,
                friendlyPieces,
                opponentPieces
            );
        }

        public static bool CanCastleKingside(Colour sideToMove, PieceSet friendlyPieces, PieceSet opponentPieces)
        {
            return CanCastle(
                sideToMove == Colour.White ? WhiteKSC : BlackKSC,
                sideToMove == Colour.White ? WhiteKSC : BlackKSC,
                friendlyPieces,
                opponentPieces
            );
        }
    }
}