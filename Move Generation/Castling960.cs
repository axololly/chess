using Chess;
using Chess.Bitmasks;
using Chess.MoveGen;
using Chess.Utilities;
using Types.Bitboards;

namespace Chess960.Castling
{
    enum CastleType
    {
        WhiteQueenside,
        WhiteKingside,
        BlackQueenside,
        BlackKingside
    }

    public struct Castling960
    {
        public static bool CanCastleKingside(Board960 board)
        {
            Colour side = board.ColourToMove;
            bool sideIsWhite = side == Colour.White;

            int right = 0b1000 >> (int)side;

            // If the castling right is not present, castling is already
            // off the board   
            if (!(right & board.castlingRights)) return false;

            // If the king is in check, castling is already off the board
            if (board.checkers > 0) return false;
            
            int rookPos = board.rookStarts[sideIsWhite ? 0 : 2];
            
            // Check for pinned rooks - if a rook is pinned then
            // you cannot castle in that direction because the king
            // will be revealed to attack
            if (1UL << rookPos & board.pinHV) return false;

            int kingPos = board.PlayerToMove.KingSquare;
            int endKingPos = sideIsWhite ? 6 : 62;

            Bitboard kingTravelRay = Bitmask.RayBetween(kingPos, endKingPos, true);
            Bitboard castlingSpace = Bitmask.RayBetween(kingPos, rookPos);

            // If there are any pieces blocking the castling region,
            // you cannot castle anyway
            if (board.boardMask & castlingSpace) return false;

            Bitboard oppAttacks = board.OpponentToMove.AttackingBitmask(board.PlayerToMove.Mask);

            // If any opponent pieces are attacking the squares the king travels through,
            // or attacking the square we land on, we cannot castle.
            if (oppAttacks & (kingTravelRay | 1UL << endKingPos)) return false;

            // If all these checks pass, we can castle
            return true;
        }

        public static bool CanCastleQueenside(Board960 board)
        {
            Colour side = board.ColourToMove;
            bool sideIsWhite = side == Colour.White;

            int right = sideIsWhite ? 0b0100 : 0b0001;

            // If the castling right is not present, castling is already
            // off the board   
            if (!(right & board.castlingRights)) return false;

            // If the king is in check, castling is already off the board
            if (board.checkers > 0) return false;
            
            int rookPos = board.rookStarts[sideIsWhite ? 1 : 3];
            
            // Check for pinned rooks - if a rook is pinned then
            // you cannot castle in that direction because the king
            // will be revealed to attack
            if (1UL << rookPos & board.pinHV) return false;

            int kingPos = board.PlayerToMove.KingSquare;
            int endKingPos = sideIsWhite ? 2 : 58;

            Bitboard kingTravelRay = Bitmask.RayBetween(kingPos, endKingPos);
            Bitboard castlingSpace = Bitmask.RayBetween(kingPos, rookPos);

            // If there are any pieces blocking the castling region,
            // you cannot castle anyway
            if (board.boardMask & castlingSpace) return false;

            Bitboard oppAttacks = board.OpponentToMove.AttackingBitmask(board.PlayerToMove.Mask);

            // If any opponent pieces are attacking the squares the king travels through,
            // or attacking the square we land on, we cannot castle.
            if (oppAttacks & (kingTravelRay | 1UL << endKingPos)) return false;

            // If all these checks pass, we can castle
            return true;
        }
    }
}