using System.Numerics;
using Chess.Utilities;

namespace Types.Bitboards
{
    public enum Direction
    {
        North,
        South,
        Northeast,
        Southeast,
        Northwest,
        Southwest,
        East,
        West
    }

    public struct Bitboard
    {
        private ulong bitboard;

        public readonly bool IsEmpty { get { return bitboard == 0; } }

        public static Bitboard Filled { get { return ulong.MaxValue; } }

        public Bitboard(ulong bitboard = 0) => this.bitboard = bitboard;

        // For changing between types
        public static implicit operator ulong(Bitboard bb) => bb.bitboard;
        public static implicit operator int(Bitboard bb)   => (int)bb.bitboard;
        public static implicit operator Bitboard(ulong u)  => new(u);
        public static implicit operator bool(Bitboard bb)  => bb.bitboard != 0;

        // For using bitwise operators
        public static Bitboard operator |(Bitboard first, Bitboard second)
            => new(first.bitboard | second.bitboard);
        
        public static Bitboard operator ^(Bitboard first, Bitboard second)
            => new(first.bitboard ^ second.bitboard);
        
        public static Bitboard operator &(Bitboard first, Bitboard second)
            => new(first.bitboard & second.bitboard);
        
        public static Bitboard operator ~(Bitboard bb)
            => new(~bb.bitboard);
        
        public static Bitboard operator >>(Bitboard bb, int shift)
            => new(bb.bitboard >> shift);
        
        public static Bitboard operator <<(Bitboard bb, int shift)
            => new(bb.bitboard << shift);
        
        public static bool operator ==(Bitboard? bb1, Bitboard? bb2)
            => Equals(bb1, bb2);
        
        public static bool operator !=(Bitboard? bb1, Bitboard? bb2)
            => !Equals(bb1, bb2);

        public override int GetHashCode()
            => (int)bitboard;
        
        public override bool Equals(object? other)
        {
            if (other is not Bitboard) return false;
            return ((Bitboard)other).bitboard == bitboard;
        }


        // Bit tricks
        public int PopLSB()
        {
            if (bitboard == 0) throw new Exception("cannot pop LSB off of an empty bitboard.");

            int LSB = BitOperations.TrailingZeroCount(bitboard);
            
            bitboard &= bitboard - 1;
            
            return LSB;
        }

        public int IndexLSB()
        {
            if (bitboard == 0) throw new Exception("cannot read LSB off of an empty bitboard.");

            return BitOperations.TrailingZeroCount(bitboard);
        }

        public Bitboard Shift(Direction direction)
        {
            return direction switch
            {
                Direction.North => this << 8,
                Direction.South => this >> 8,

                Direction.East => (this & ~Files.H) << 1,
                Direction.West => (this & ~Files.A) >> 1,

                Direction.Northeast => (this & ~Files.H) << 9,
                Direction.Northwest => (this & ~Files.A) << 7,
                Direction.Southeast => (this & ~Files.H) >> 7,
                Direction.Southwest => (this & ~Files.A) >> 9,

                _ => throw new Exception($"invalid Direction enum \"{direction}\"")
            };
        }

        public int BitCount() => BitOperations.PopCount(bitboard);

        // For displaying a bitboard
        public override string ToString() => Display.StringifyBitboard(bitboard);
    }
}