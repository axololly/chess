using System.Numerics;
using Utilities;

namespace Bitboards
{
    public struct Bitboard
    {
        private ulong bitboard;

        public Bitboard(ulong bitboard = 0) => this.bitboard = bitboard;

        // For changing between types
        public static implicit operator ulong(Bitboard bb) => bb.bitboard;
        public static implicit operator Bitboard(ulong u) => new(u);
        public static implicit operator bool(Bitboard bb) => bb != 0;

        // Bit tricks
        public int PopLSB()
        {
            int LSB = BitOperations.TrailingZeroCount(bitboard);
            
            bitboard ^= 1UL << LSB;
            
            return LSB;
        }

        public int BitCount() => BitOperations.PopCount(bitboard);

        // For displaying a bitboard
        public override string ToString() => Display.StringifyBitboard(bitboard);
    }
}