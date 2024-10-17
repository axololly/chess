using Chess.Types.Bitboards;

namespace Chess.Types.Squares
{
    public struct Square
    {
        private int _sq;

        public Square(int? index) => _sq = index ?? -1;
        public Square(string value) => _sq = value == "-" ? -1 : (value[0] - 'a' + 8 * (value[1] - '1'));

        public Square(int rank, int file) => _sq = rank * 8 + file;
        
        public int File { get { return _sq % 8; } }
        public int Rank { get { return _sq / 8; } }
        public Bitboard Bitboard { get { return 1UL << _sq; } }
        
        // Converting TO squares
        public static implicit operator Square  (int value)    => new(value);

        // Converting FROM squares
        public static implicit operator int     (Square sq)    => sq._sq;
        public static implicit operator bool    (Square sq)    => sq._sq != -1;

        public override string ToString() => _sq == -1 ? "-" : $"{(char)('a' + File)}{(char)('1' + Rank)}";
        public override int GetHashCode() => _sq;
        public override bool Equals(object? obj) => obj != null && ((Square)obj)._sq == _sq;

        public static bool operator ==(Square sq1, Square sq2) =>  sq1.Equals(sq2);
        public static bool operator !=(Square sq1, Square sq2) => !sq1.Equals(sq2);
    }

    /**************************************************
      Before all of you yell at me for not using an
      enum, I cannot live without implicit conversion
      methods. They are my pride and soul.

      That's why I'm using polymorphism. So I can do
      extra stuff that's not traditionally in an enum.
    **************************************************/

    public struct Squares
    {
        public readonly static Square None = -1;
        
        // Only the square literals are used in castling,
        // so we can exclude all the others because they
        // are never used.

        public readonly static Square A1 = 0;
        public readonly static Square C1 = 2;
        public readonly static Square G1 = 6;
        public readonly static Square H1 = 7;
        public readonly static Square A8 = 56;
        public readonly static Square C8 = 58;
        public readonly static Square G8 = 62;
        public readonly static Square H8 = 63;
    }
}