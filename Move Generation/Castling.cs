using Types.Bitboards;
using Chess.Bitmasks;
using Chess.Utilities;
using Chess960;

namespace Chess.Castling
{
    /*********************************************
       Everything below is for standard castling.
    *********************************************/

    public struct CastlingRights
    {
        private int castlingBits;

        public readonly bool CanWhiteQueenside { get { return (castlingBits & 0b0100) != 0; } }
        public readonly bool CanWhiteKingside { get { return (castlingBits & 0b1000) != 0; } }
        
        public readonly bool CanBlackQueenside { get { return (castlingBits & 0b0001) != 0; } }
        public readonly bool CanBlackKingside { get { return (castlingBits & 0b0010) != 0; } }
        

        // public CastlingRights(int bits) => castlingBits = (byte)bits;

        public static CastlingRights FromString(string castlingRights)
        {
            int n = 0;

            if (castlingRights != "-")
            {
                foreach (char right in castlingRights)
                {
                    n |= right switch
                    {
                        'K' => 0b1000,
                        'Q' => 0b0100,
                        'k' => 0b0010,
                        'q' => 0b0001,

                        _ => throw new Exception($"illegal castling right '{right}' found.")
                    };
                }
            }

            return new CastlingRights() { castlingBits = n };
        }

        public bool CanQueenside(Colour colour) => colour == Colour.White ? CanWhiteQueenside : CanBlackQueenside;
        public bool CanKingside (Colour colour) => colour == Colour.White ? CanWhiteKingside  : CanBlackKingside ;

        public void DisableQueenside(Colour colour) => castlingBits &= 15 ^ 0b0100 >> 2 * (int)colour;
        public void DisableKingside (Colour colour) => castlingBits &= 15 ^ 0b1000 >> 2 * (int)colour;
        public void DisableBoth     (Colour colour) => castlingBits &= 15 ^ 0b1100 >> 2 * (int)colour;

        public void AddBits(int bits) => castlingBits |= 15 & bits;

        public int GetBits() => castlingBits;

        public override string ToString()
        {
            string rights = "qkQK";
            string result = "";

            for (int i = 3; i > -1; i--)
            {
                if ((castlingBits >> i & 1) == 1) result += rights[i];
            }

            return result;
        }
    }

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

    /***************************************
       Everything below is for Castling960.
       
       For Chess960, it still uses the same
       CastlingRights struct as normal.
    ****************************************/

    enum CastleType
    {
        WhiteQueenside,
        WhiteKingside,
        BlackQueenside,
        BlackKingside
    }

    public struct Castling960Moves
    {
        public static bool CanCastleKingside(Board960 board)
        {
            Colour side = board.ColourToMove;
            bool sideIsWhite = side == Colour.White;

            int right = 0b1000 >> (int)side;

            // If the castling right is not present, castling is already
            // off the board   
            if (!board.castlingRights.CanKingside(side)) return false;

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

            // If the castling right is not present, castling is already
            // off the board   
            if (!board.castlingRights.CanQueenside(side)) return false;

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