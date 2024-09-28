using System.Diagnostics.CodeAnalysis;
using Types.Bitboards;

namespace Types.Squares
{
    public struct Square
    {
        private int _sq;

        private void RaiseWhenMinus1(string propertyAttemptedToAccess)
        {
            if (_sq == -1)
            {
                throw new Exception($"cannot obtain {propertyAttemptedToAccess} while null.");
            }
        }

        public Square(int? index) => _sq = index ?? -1;
        public Square(string value)
        {
            if (value == "-")
            {
                _sq = -1;
                return;
            }

            value = value.ToUpper();
            _sq = (value[0] - 'A') * 8 + value[1] - '1';
        }

        public Square(int rank, int file) => _sq = rank * 8 + file;
        
        public int File { get { return _sq % 8; } }
        public int Rank { get { return _sq / 8; } }
        public Bitboard Bitboard { get { return 1UL << _sq; } }
        
        // Converting TO squares
        public static implicit operator Square  (int value)    => new(value);

        // Converting FROM squares
        public static implicit operator int     (Square sq)    => sq._sq;
        public static implicit operator bool    (Square sq)    => sq._sq != -1;

        public override string ToString() => _sq == -1 ? "-" : $"{"ABCDEFGH"[File]}{"12345678"[Rank]}";
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

        public readonly static Square A1 = 0;
        public readonly static Square B1 = 1;
        public readonly static Square C1 = 2;
        public readonly static Square D1 = 3;
        public readonly static Square E1 = 4;
        public readonly static Square F1 = 5;
        public readonly static Square G1 = 6;
        public readonly static Square H1 = 7;
        public readonly static Square A2 = 8;
        public readonly static Square B2 = 9;
        public readonly static Square C2 = 10;
        public readonly static Square D2 = 11;
        public readonly static Square E2 = 12;
        public readonly static Square F2 = 13;
        public readonly static Square G2 = 14;
        public readonly static Square H2 = 15;
        public readonly static Square A3 = 16;
        public readonly static Square B3 = 17;
        public readonly static Square C3 = 18;
        public readonly static Square D3 = 19;
        public readonly static Square E3 = 20;
        public readonly static Square F3 = 21;
        public readonly static Square G3 = 22;
        public readonly static Square H3 = 23;
        public readonly static Square A4 = 24;
        public readonly static Square B4 = 25;
        public readonly static Square C4 = 26;
        public readonly static Square D4 = 27;
        public readonly static Square E4 = 28;
        public readonly static Square F4 = 29;
        public readonly static Square G4 = 30;
        public readonly static Square H4 = 31;
        public readonly static Square A5 = 32;
        public readonly static Square B5 = 33;
        public readonly static Square C5 = 34;
        public readonly static Square D5 = 35;
        public readonly static Square E5 = 36;
        public readonly static Square F5 = 37;
        public readonly static Square G5 = 38;
        public readonly static Square H5 = 39;
        public readonly static Square A6 = 40;
        public readonly static Square B6 = 41;
        public readonly static Square C6 = 42;
        public readonly static Square D6 = 43;
        public readonly static Square E6 = 44;
        public readonly static Square F6 = 45;
        public readonly static Square G6 = 46;
        public readonly static Square H6 = 47;
        public readonly static Square A7 = 48;
        public readonly static Square B7 = 49;
        public readonly static Square C7 = 50;
        public readonly static Square D7 = 51;
        public readonly static Square E7 = 52;
        public readonly static Square F7 = 53;
        public readonly static Square G7 = 54;
        public readonly static Square H7 = 55;
        public readonly static Square A8 = 56;
        public readonly static Square B8 = 57;
        public readonly static Square C8 = 58;
        public readonly static Square D8 = 59;
        public readonly static Square E8 = 60;
        public readonly static Square F8 = 61;
        public readonly static Square G8 = 62;
        public readonly static Square H8 = 63;

        public Square FromString(string squareName) => new(squareName[0] - 'A', squareName[1] - '1');
    }
}