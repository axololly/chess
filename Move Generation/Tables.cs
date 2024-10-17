using Chess.Types.Bitboards;
using Chess.Utilities;

namespace Chess.Tables
{
    public struct MovementTables
    {
        public Bitboard[] KING_MOVES_TABLE = new Bitboard[64];
        public Bitboard[] KNIGHT_MOVES_TABLE = new Bitboard[64];

        public MovementTables()
        {
            // King moves
            for (int square = 0; square < 64; square++)
            {
                Bitboard sq = 1UL << square;
                Bitboard moves = 0;

                int[] directions = [7, 8, 9, 1];

                foreach (int direction in directions) 
                {
                    moves |= sq << direction;
                    moves |= sq >> direction;
                }

                // If on the left edge, ignore moves that appear on the right edge
                if ((sq & Files.A) != 0) moves &= ~Files.H;
                
                // If on the right edge, ignore moves that appear on the left edge
                if ((sq & Files.H) != 0) moves &= ~Files.A;

                KING_MOVES_TABLE[square] = moves;
            }

            // Knight moves
            for (int square = 0; square < 64; square++)
            {
                Bitboard sq = 1UL << square;
                Bitboard moves = 0;
                int[] directions = [17, 15, 10, 6];

                foreach (int direction in directions)
                {
                    moves |= sq << direction;
                    moves |= sq >> direction;
                }

                // If on the left side, ignore any that go onto the right
                if ((sq & Files.AB) != 0) moves &= ~Files.GH;
                
                // If on the right side, ignore any that go onto the left
                if ((sq & Files.GH) != 0) moves &= ~Files.AB;

                KNIGHT_MOVES_TABLE[square] = moves;
            }
        }
    }
}