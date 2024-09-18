using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Types.Nibble
{
    public struct Nibble
    {
        private byte value;

        public static int MaxValue { get { return 15; } }
        public static int MinValue { get { return 0; } }

        public int LeadingZeroCount { get { return BitOperations.Log2(value); } }
        
        public Nibble(byte n) => value = n;

        public static Nibble operator + (Nibble n1, Nibble n2) => new((byte)( n1.value  +  n2.value  &  15));
        public static Nibble operator - (Nibble n1, Nibble n2) => new((byte)( n1.value  -  n2.value  &  15));
        public static Nibble operator * (Nibble n1, Nibble n2) => new((byte)( n1.value  *  n2.value  &  15));
        public static Nibble operator / (Nibble n1, Nibble n2) => new((byte)( n1.value  /  n2.value  &  15));
        public static Nibble operator >>(Nibble n1, Nibble n2) => new((byte)( n1.value >>  n2.value  &  15));
        public static Nibble operator <<(Nibble n1, Nibble n2) => new((byte)( n1.value <<  n2.value  &  15));
        public static Nibble operator & (Nibble n1, Nibble n2) => new((byte)( n1.value  &  n2.value  &  15));
        public static Nibble operator | (Nibble n1, Nibble n2) => new((byte)( n1.value  |  n2.value  &  15));
        public static Nibble operator ^ (Nibble n1, Nibble n2) => new((byte)( n1.value  ^  n2.value  &  15));
        
        public static Nibble operator ~ (Nibble n) => new((byte)(n.value ^ 15));

        public static bool operator ==(Nibble n1, Nibble n2) => n1.value == n2.value;
        public static bool operator !=(Nibble n1, Nibble n2) => n1.value != n2.value;

        public static implicit operator Nibble(int x)   => new((byte)(x & 15));
        public static implicit operator Nibble(ulong x) => new((byte)(x & 15));
        public static implicit operator Nibble(byte x)  => new((byte)(x & 15));

        public static implicit operator bool  (Nibble n) => n.value > 0;

        public static implicit operator byte  (Nibble n) => n.value;
        public static implicit operator int   (Nibble n) => n.value;
        public static implicit operator long  (Nibble n) => n.value;
        public static implicit operator ulong (Nibble n) => n.value;
        public static implicit operator uint  (Nibble n) => n.value;

        public override int GetHashCode() => value;
        public override string ToString() => $"{value}";
        public override bool Equals([NotNullWhen(true)] object? obj) => obj != null && obj.GetHashCode() == GetHashCode();

        public string AsBinary()
        {
            string result = "";
            
            for (int i = 3; i > -1; i--) result += $"{value >> i & 1}";
            
            return "0b" + result;
        }
    }
}