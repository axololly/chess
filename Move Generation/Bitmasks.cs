using Chess.Magics;
using Chess.Tables;
using Types.Bitboards;

namespace Chess.Bitmasks
{
    public struct Bitmask
    {
        public readonly static MagicBitboards mb = new();
        public readonly static MovementTables mt = new();

        public static Bitboard ForBishop(Bitboard occupancy, int square)
        {
            ulong occ;

            MagicEntry entry = mb.BISHOP_MAGICS[square];
            
            occ = occupancy & entry.mask;
            occ *= entry.magic;
            occ >>= entry.shift;

            int index = (int)occ + entry.offset + mb.BISHOPS_START_FROM;

            return mb.SLIDING_PIECE_MOVE_TABLE[index];
        }

        public static Bitboard ForRook(Bitboard occupancy, int square)
        {
            ulong occ;

            MagicEntry entry = mb.ROOK_MAGICS[square];

            occ = occupancy & entry.mask;
            occ *= entry.magic;
            occ >>= entry.shift;

            int index = (int)occ + entry.offset;

            return mb.SLIDING_PIECE_MOVE_TABLE[index];
        }

        public static Bitboard ForQueen(Bitboard occupancy, int square)
            => ForBishop(occupancy, square) | ForRook(occupancy, square);

        public static Bitboard ForKnight(Bitboard friendlyOccupancy, int square)
            => ~friendlyOccupancy & mt.KNIGHT_MOVES_TABLE[square];

        public static Bitboard ForKing(Bitboard enemyOrEmpty, Bitboard opponentAttacks, int square)
        {
            return mt.KING_MOVES_TABLE[square] & enemyOrEmpty & ~opponentAttacks;
        }
    }
}