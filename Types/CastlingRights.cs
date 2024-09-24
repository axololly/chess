using Chess.Utilities;

namespace Types.CastlingRights
{
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

            return new CastlingRights() { castlingBits = n };
        }

        public bool CanQueenside(Colour colour) => colour == Colour.White ? CanWhiteQueenside : CanBlackQueenside;
        public bool CanKingside (Colour colour) => colour == Colour.White ? CanWhiteKingside  : CanBlackKingside ;

        public void DisableQueenside(Colour colour) => castlingBits &= 15 ^ 0b0100 >> 2 * (int)colour;
        public void DisableKingside (Colour colour) => castlingBits &= 15 ^ 0b1000 >> 2 * (int)colour;
        public void DisableBoth     (Colour colour) => castlingBits &= 15 ^ 0b1100 >> 2 * (int)colour;

        public void AddBits(int bits) => castlingBits |= 15 & bits;

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
}