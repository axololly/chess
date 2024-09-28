// Thanks to Ciekcie's Stormphrax for this section
// on Zobrist hashing. This was a big help.

using Chess;
using Chess960;
using Chess.Castling;
using Chess.Utilities;
using Types.Bitboards;
using Types.Squares;

namespace Types.Zobrist
{
    struct XorRandom
    {
        public XorRandom() {}

        private ulong seed = 0x9f0417f4bf790ee8UL;

        public ulong Next()
        {
            // Shuffle the value of the seed using XOR
            seed ^= seed << 13;
            seed ^= seed >> 7;
            seed ^= seed << 17;

            return seed;
        }
    }

    struct Sizes
    {
        public static int PieceSquares = 12 * 64;
        public static int Colour = 1;
        public static int Castling = 16;
        public static int EnPassant = 8;

        public static int Total = PieceSquares + Colour + Castling + EnPassant;
    }

    struct Offsets
    {
        public static int PieceSquares = 0;
        public static int Colour = PieceSquares + Sizes.PieceSquares;
        public static int Castling = Colour + Sizes.Colour;
        public static int EnPassant = Castling + Sizes.Castling;
    }
    
    public struct Zobrist
    {
        static ulong[] Keys = new ulong[Sizes.Total];

        // Initialise zobrist hashes on construction
        public static void Init()
        {
            XorRandom rng = new();

            for (int i = 0; i < Keys.Length; i++)
            {
                Keys[i] = rng.Next();
            }
        }

        public static ulong HashPieceAndSquare(Piece piece, Square square)
        {
            return Keys[
                Offsets.PieceSquares
                + square * 12
                + (int)piece
            ];
        }

        public static ulong HashColor(Colour colour) => colour == Colour.White ? 0 : Keys[Offsets.Colour];

        public static ulong HashCastlingRights(CastlingRights rights) => Keys[Offsets.Castling + rights.GetBits()];

        public static ulong HashEnPassant(Square epSquare)
        {
            if (!epSquare) return 0;

            return Keys[Offsets.EnPassant + epSquare.File];
        }
    }
}