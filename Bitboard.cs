using System.Numerics;
using Utilities;

namespace Bitboards
{
    public struct Bitboard
    {
        private ulong bitboard;

        public static ulong Filled { get { return ulong.MaxValue; } }

        public Bitboard(ulong bitboard = 0) => this.bitboard = bitboard;

        // For changing between types
        public static implicit operator ulong(Bitboard bb) => bb.bitboard;
        public static implicit operator Bitboard(ulong u) => new(u);
        public static implicit operator bool(Bitboard bb) => bb.bitboard != 0;

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


        // Bit tricks
        public int PopLSB()
        {
            if (bitboard == 0) throw new Exception("cannot pop LSB off of an empty bitboard.");

            int LSB = BitOperations.TrailingZeroCount(bitboard);
            
            bitboard &= bitboard - 1;
            
            return LSB;
        }

        public int BitCount() => BitOperations.PopCount(bitboard);

        // For displaying a bitboard
        public override string ToString() => Display.StringifyBitboard(bitboard);
    }
}