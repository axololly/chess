using Chess.Magics;
using Chess.Tables;
using Chess.Types.Bitboards;

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

        public static Bitboard ForKnight(Bitboard friendlyOccupancy, int square)
            => ~friendlyOccupancy & mt.KNIGHT_MOVES_TABLE[square];

        public static Bitboard ForKing(Bitboard opponentAttacks, int square)
        {
            return mt.KING_MOVES_TABLE[square] & ~opponentAttacks;
        }

        public static Bitboard RayBetween(int square1, int square2, bool inclusive = false)
        {
            int file1, rank1, file2, rank2;

            file1 = square1 % 8;
            rank1 = square1 / 8;

            file2 = square2 % 8;
            rank2 = square2 / 8;

            if (file1 == file2 || rank1 == rank2) // same row or file
            {
                Bitboard bb = ForRook(1UL << square2, square1)
                            & ForRook(1UL << square1, square2);
                
                if (inclusive) bb |= (1UL << square1) | (1UL << square2);

                return bb;
            }

            if (Math.Abs(file1 - file2) == Math.Abs(rank1 - rank2))
            {
                Bitboard bb = ForBishop(1UL << square2, square1)
                            & ForBishop(1UL << square1, square2);
                
                if (inclusive) bb |= (1UL << square1) | (1UL << square2);

                return bb;
            }

            throw new Exception($"cannot form ray between squares \"{square1}\" and \"{square2}\" becuase they are not on the same line.");
        }
    }
}