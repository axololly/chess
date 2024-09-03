using System.Numerics;
using Utilities;
using ChessBoard;
using Castling;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MoveGeneration
{
    public record MagicEntry
    {
        public ulong mask;
        public ulong magic;
        public int shift;
        public int offset;

        public MagicEntry(ulong mask, ulong magic, int shift, int offset)
        {
            this.mask = mask;
            this.magic = magic;
            this.shift = shift;
            this.offset = offset;
        }
    }

    public class MagicBitboards
    {
        public MagicEntry[] ROOK_MAGICS = [
            new(mask: 0x000101010101017E, magic: 0x5080008011400020, shift: 52, offset: 0),
            new(mask: 0x000202020202027C, magic: 0x0140001000402000, shift: 53, offset: 4096),
            new(mask: 0x000404040404047A, magic: 0x0280091000200480, shift: 53, offset: 6144),
            new(mask: 0x0008080808080876, magic: 0x0700081001002084, shift: 53, offset: 8192),
            new(mask: 0x001010101010106E, magic: 0x0300024408010030, shift: 53, offset: 10240),
            new(mask: 0x002020202020205E, magic: 0x510004004E480100, shift: 53, offset: 12288),
            new(mask: 0x004040404040403E, magic: 0x0400044128020090, shift: 53, offset: 14336),
            new(mask: 0x008080808080807E, magic: 0x8080004100012080, shift: 52, offset: 16384),
            new(mask: 0x0001010101017E00, magic: 0x0220800480C00124, shift: 53, offset: 20480),
            new(mask: 0x0002020202027C00, magic: 0x0020401001C02000, shift: 54, offset: 22528),
            new(mask: 0x0004040404047A00, magic: 0x000A002204428050, shift: 54, offset: 23552),
            new(mask: 0x0008080808087600, magic: 0x004E002040100A00, shift: 54, offset: 24576),
            new(mask: 0x0010101010106E00, magic: 0x0102000A00041020, shift: 54, offset: 25600),
            new(mask: 0x0020202020205E00, magic: 0x0A0880040080C200, shift: 54, offset: 26624),
            new(mask: 0x0040404040403E00, magic: 0x0002000600018408, shift: 54, offset: 27648),
            new(mask: 0x0080808080807E00, magic: 0x0025001200518100, shift: 53, offset: 28672),
            new(mask: 0x00010101017E0100, magic: 0x8900328001400080, shift: 53, offset: 30720),
            new(mask: 0x00020202027C0200, magic: 0x0848810020400100, shift: 54, offset: 32768),
            new(mask: 0x00040404047A0400, magic: 0xC001410020010153, shift: 54, offset: 33792),
            new(mask: 0x0008080808760800, magic: 0x4110C90020100101, shift: 54, offset: 34816),
            new(mask: 0x00101010106E1000, magic: 0x00A0808004004800, shift: 54, offset: 35840),
            new(mask: 0x00202020205E2000, magic: 0x401080801C000601, shift: 54, offset: 36864),
            new(mask: 0x00404040403E4000, magic: 0x0100040028104221, shift: 54, offset: 37888),
            new(mask: 0x00808080807E8000, magic: 0x840002000900A054, shift: 53, offset: 38912),
            new(mask: 0x000101017E010100, magic: 0x1000348280004000, shift: 53, offset: 40960),
            new(mask: 0x000202027C020200, magic: 0x001000404000E008, shift: 54, offset: 43008),
            new(mask: 0x000404047A040400, magic: 0x0424410300200035, shift: 54, offset: 44032),
            new(mask: 0x0008080876080800, magic: 0x2008C22200085200, shift: 54, offset: 45056),
            new(mask: 0x001010106E101000, magic: 0x0005304D00080100, shift: 54, offset: 46080),
            new(mask: 0x002020205E202000, magic: 0x000C040080120080, shift: 54, offset: 47104),
            new(mask: 0x004040403E404000, magic: 0x8404058400080210, shift: 54, offset: 48128),
            new(mask: 0x008080807E808000, magic: 0x0001848200010464, shift: 53, offset: 49152),
            new(mask: 0x0001017E01010100, magic: 0x6000204001800280, shift: 53, offset: 51200),
            new(mask: 0x0002027C02020200, magic: 0x2410004003C02010, shift: 54, offset: 53248),
            new(mask: 0x0004047A04040400, magic: 0x0181200A80801000, shift: 54, offset: 54272),
            new(mask: 0x0008087608080800, magic: 0x000C60400A001200, shift: 54, offset: 55296),
            new(mask: 0x0010106E10101000, magic: 0x0B00040180802800, shift: 54, offset: 56320),
            new(mask: 0x0020205E20202000, magic: 0xC00A000280804C00, shift: 54, offset: 57344),
            new(mask: 0x0040403E40404000, magic: 0x4040080504005210, shift: 54, offset: 58368),
            new(mask: 0x0080807E80808000, magic: 0x0000208402000041, shift: 53, offset: 59392),
            new(mask: 0x00017E0101010100, magic: 0xA200400080628000, shift: 53, offset: 61440),
            new(mask: 0x00027C0202020200, magic: 0x0021020240820020, shift: 54, offset: 63488),
            new(mask: 0x00047A0404040400, magic: 0x1020027000848022, shift: 54, offset: 64512),
            new(mask: 0x0008760808080800, magic: 0x0020500018008080, shift: 54, offset: 65536),
            new(mask: 0x00106E1010101000, magic: 0x10000D0008010010, shift: 54, offset: 66560),
            new(mask: 0x00205E2020202000, magic: 0x0100020004008080, shift: 54, offset: 67584),
            new(mask: 0x00403E4040404000, magic: 0x0008020004010100, shift: 54, offset: 68608),
            new(mask: 0x00807E8080808000, magic: 0x12241C0880420003, shift: 53, offset: 69632),
            new(mask: 0x007E010101010100, magic: 0x4000420024810200, shift: 53, offset: 71680),
            new(mask: 0x007C020202020200, magic: 0x0103004000308100, shift: 54, offset: 73728),
            new(mask: 0x007A040404040400, magic: 0x008C200010410300, shift: 54, offset: 74752),
            new(mask: 0x0076080808080800, magic: 0x2410008050A80480, shift: 54, offset: 75776),
            new(mask: 0x006E101010101000, magic: 0x0820880080040080, shift: 54, offset: 76800),
            new(mask: 0x005E202020202000, magic: 0x0044220080040080, shift: 54, offset: 77824),
            new(mask: 0x003E404040404000, magic: 0x2040100805120400, shift: 54, offset: 78848),
            new(mask: 0x007E808080808000, magic: 0x0129000080C20100, shift: 53, offset: 79872),
            new(mask: 0x7E01010101010100, magic: 0x0010402010800101, shift: 52, offset: 81920),
            new(mask: 0x7C02020202020200, magic: 0x0648A01040008101, shift: 53, offset: 86016),
            new(mask: 0x7A04040404040400, magic: 0x0006084102A00033, shift: 53, offset: 88064),
            new(mask: 0x7608080808080800, magic: 0x0002000870C06006, shift: 53, offset: 90112),
            new(mask: 0x6E10101010101000, magic: 0x0082008820100402, shift: 53, offset: 92160),
            new(mask: 0x5E20202020202000, magic: 0x0012008410050806, shift: 53, offset: 94208),
            new(mask: 0x3E40404040404000, magic: 0x2009408802100144, shift: 53, offset: 96256),
            new(mask: 0x7E80808080808000, magic: 0x821080440020810A, shift: 52, offset: 98304)
        ];
        
        public MagicEntry[] BISHOP_MAGICS = [
            new(mask: 0x0040201008040200, magic: 0x2020420401002200, shift: 58, offset: 0),
            new(mask: 0x0000402010080400, magic: 0x05210A020A002118, shift: 59, offset: 64),
            new(mask: 0x0000004020100A00, magic: 0x1110040454C00484, shift: 59, offset: 96),
            new(mask: 0x0000000040221400, magic: 0x1008095104080000, shift: 59, offset: 128),
            new(mask: 0x0000000002442800, magic: 0xC409104004000000, shift: 59, offset: 160),
            new(mask: 0x0000000204085000, magic: 0x0002901048080200, shift: 59, offset: 192),
            new(mask: 0x0000020408102000, magic: 0x0044040402084301, shift: 59, offset: 224),
            new(mask: 0x0002040810204000, magic: 0x2002030188040200, shift: 58, offset: 256),
            new(mask: 0x0020100804020000, magic: 0x0000C8084808004A, shift: 59, offset: 320),
            new(mask: 0x0040201008040000, magic: 0x1040040808010028, shift: 59, offset: 352),
            new(mask: 0x00004020100A0000, magic: 0x40040C0114090051, shift: 59, offset: 384),
            new(mask: 0x0000004022140000, magic: 0x40004820802004C4, shift: 59, offset: 416),
            new(mask: 0x0000000244280000, magic: 0x0010042420260012, shift: 59, offset: 448),
            new(mask: 0x0000020408500000, magic: 0x10024202300C010A, shift: 59, offset: 480),
            new(mask: 0x0002040810200000, magic: 0x000054013D101000, shift: 59, offset: 512),
            new(mask: 0x0004081020400000, magic: 0x0100020482188A0A, shift: 59, offset: 544),
            new(mask: 0x0010080402000200, magic: 0x0120090421020200, shift: 59, offset: 576),
            new(mask: 0x0020100804000400, magic: 0x1022204444040C00, shift: 59, offset: 608),
            new(mask: 0x004020100A000A00, magic: 0x0008000400440288, shift: 57, offset: 640),
            new(mask: 0x0000402214001400, magic: 0x0008060082004040, shift: 57, offset: 768),
            new(mask: 0x0000024428002800, magic: 0x0044040081A00800, shift: 57, offset: 896),
            new(mask: 0x0002040850005000, magic: 0x021200014308A010, shift: 57, offset: 1024),
            new(mask: 0x0004081020002000, magic: 0x8604040080880809, shift: 59, offset: 1152),
            new(mask: 0x0008102040004000, magic: 0x0000802D46009049, shift: 59, offset: 1184),
            new(mask: 0x0008040200020400, magic: 0x00500E8040080604, shift: 59, offset: 1216),
            new(mask: 0x0010080400040800, magic: 0x0024030030100320, shift: 59, offset: 1248),
            new(mask: 0x0020100A000A1000, magic: 0x2004100002002440, shift: 57, offset: 1280),
            new(mask: 0x0040221400142200, magic: 0x02090C0008440080, shift: 55, offset: 1408),
            new(mask: 0x0002442800284400, magic: 0x0205010000104000, shift: 55, offset: 1920),
            new(mask: 0x0004085000500800, magic: 0x0410820405004A00, shift: 57, offset: 2432),
            new(mask: 0x0008102000201000, magic: 0x8004140261012100, shift: 59, offset: 2560),
            new(mask: 0x0010204000402000, magic: 0x0A00460000820100, shift: 59, offset: 2592),
            new(mask: 0x0004020002040800, magic: 0x201004A40A101044, shift: 59, offset: 2624),
            new(mask: 0x0008040004081000, magic: 0x840C024220208440, shift: 59, offset: 2656),
            new(mask: 0x00100A000A102000, magic: 0x000C002E00240401, shift: 57, offset: 2688),
            new(mask: 0x0022140014224000, magic: 0x2220A00800010106, shift: 55, offset: 2816),
            new(mask: 0x0044280028440200, magic: 0x88C0080820060020, shift: 55, offset: 3328),
            new(mask: 0x0008500050080400, magic: 0x0818030B00A81041, shift: 57, offset: 3840),
            new(mask: 0x0010200020100800, magic: 0xC091280200110900, shift: 59, offset: 3968),
            new(mask: 0x0020400040201000, magic: 0x08A8114088804200, shift: 59, offset: 4000),
            new(mask: 0x0002000204081000, magic: 0x228929109000C001, shift: 59, offset: 4032),
            new(mask: 0x0004000408102000, magic: 0x1230480209205000, shift: 59, offset: 4064),
            new(mask: 0x000A000A10204000, magic: 0x0A43040202000102, shift: 57, offset: 4096),
            new(mask: 0x0014001422400000, magic: 0x1011284010444600, shift: 57, offset: 4224),
            new(mask: 0x0028002844020000, magic: 0x0003041008864400, shift: 57, offset: 4352),
            new(mask: 0x0050005008040200, magic: 0x0115010901000200, shift: 57, offset: 4480),
            new(mask: 0x0020002010080400, magic: 0x01200402C0840201, shift: 59, offset: 4608),
            new(mask: 0x0040004020100800, magic: 0x001A009400822110, shift: 59, offset: 4640),
            new(mask: 0x0000020408102000, magic: 0x2002111128410000, shift: 59, offset: 4672),
            new(mask: 0x0000040810204000, magic: 0x8420410288203000, shift: 59, offset: 4704),
            new(mask: 0x00000A1020400000, magic: 0x0041210402090081, shift: 59, offset: 4736),
            new(mask: 0x0000142240000000, magic: 0x8220002442120842, shift: 59, offset: 4768),
            new(mask: 0x0000284402000000, magic: 0x0140004010450000, shift: 59, offset: 4800),
            new(mask: 0x0000500804020000, magic: 0xC0408860086488A0, shift: 59, offset: 4832),
            new(mask: 0x0000201008040200, magic: 0x0090203E00820002, shift: 59, offset: 4864),
            new(mask: 0x0000402010080400, magic: 0x0820020083090024, shift: 59, offset: 4896),
            new(mask: 0x0002040810204000, magic: 0x1040440210900C05, shift: 58, offset: 4928),
            new(mask: 0x0004081020400000, magic: 0x0818182101082000, shift: 59, offset: 4992),
            new(mask: 0x000A102040000000, magic: 0x0200800080D80800, shift: 59, offset: 5024),
            new(mask: 0x0014224000000000, magic: 0x32A9220510209801, shift: 59, offset: 5056),
            new(mask: 0x0028440200000000, magic: 0x0000901010820200, shift: 59, offset: 5088),
            new(mask: 0x0050080402000000, magic: 0x0000014064080180, shift: 59, offset: 5120),
            new(mask: 0x0020100804020000, magic: 0xA001204204080186, shift: 59, offset: 5152),
            new(mask: 0x0040201008040200, magic: 0xC04010040258C048, shift: 58, offset: 5184)
        ];
        public ulong[] SLIDING_PIECE_MOVE_TABLE = new ulong[294_912];

        public int BISHOPS_START_FROM = 1 << 18;

        public MagicBitboards()
        {
            for (int square = 0; square < 64; square++)
            {
                MagicEntry entry = ROOK_MAGICS[square];

                ulong combo = 0;

                do
                {
                    SLIDING_PIECE_MOVE_TABLE[entry.offset + (int)((combo * entry.magic) >> entry.shift)] = 
                    TraceOutRookMoves(combo, square);
                    combo = (combo - entry.mask) & entry.mask;
                } while (combo != 0);
            }

            for (int square = 0; square < 64; square++)
            {
                MagicEntry entry = BISHOP_MAGICS[square];
                ulong combo = 0;

                do
                {
                    SLIDING_PIECE_MOVE_TABLE[BISHOPS_START_FROM + entry.offset + (int)((combo * entry.magic) >> entry.shift)] = 
                    TraceOutBishopMoves(combo, square);
                    combo = (combo - entry.mask) & entry.mask;
                } while (combo != 0);
            }
        }

        public static ulong TraceOutRookMoves(ulong occupancy, int square)
        {
            ulong bitmask = 0;
            
            // Get a row of bits os we don't cross to the other side of the board
            ulong bitsOfRank = 0xFFUL << 8 * (square / 8);

            // Moving up
            ulong bit = 1UL << square;

            while ((bit & ~(bit & occupancy)) != 0)
            {
                bit <<= 8;
                bitmask |= bit;
            }

            bit = 1UL << square;

            // Moving down
            while ((bit & ~(bit & occupancy)) != 0)
            {
                bit >>= 8;
                bitmask |= bit;
            }

            bit = 1UL << square;

            // Moving left
            while ((bit & ~(bit & occupancy)) != 0)
            {
                bit <<= 1;

                if ((bit & bitsOfRank) != 0)
                {
                    bitmask |= bit;
                }
                else
                {
                    break;
                }
            }

            bit = 1UL << square;

            // Moving right
            while ((bit & ~(bit & occupancy)) != 0)
            {
                bit >>= 1;

                if ((bit & bitsOfRank) != 0)
                {
                    bitmask |= bit;
                }
                else
                {
                    break;
                }
            }

            return bitmask;
        }

        public static ulong TraceOutBishopMoves(ulong occupancy, int square)
        {
            ulong bitmask = 0;
            
            /*
            top_left = fileA | rank8
            top_right = fileH | rank8
            bottom_left = fileA | rank1
            bottom_right = fileH | rank1
            */

            ulong topLeft  = BoardFile.A | BoardRank.Eighth;
            ulong topRight = BoardFile.H | BoardRank.Eighth;
            
            ulong bottomLeft  = BoardFile.A | BoardRank.First;
            ulong bottomRight = BoardFile.H | BoardRank.First;

            ulong bit = 1UL << square;

            while ((bit & ~(bit & topLeft)) != 0)
            {
                bit <<= 7;
                bitmask |= bit;

                if ((bit & occupancy) != 0) break;
            }

            bit = 1UL << square;

            while ((bit & ~(bit & topRight)) != 0)
            {
                bit <<= 9;
                bitmask |= bit;

                if ((bit & occupancy) != 0) break;
            }

            bit = 1UL << square;

            while ((bit & ~(bit & bottomLeft)) != 0)
            {
                bit >>= 9;
                bitmask |= bit;

                if ((bit & occupancy) != 0) break;
            }

            bit = 1UL << square;

            while ((bit & ~(bit & bottomRight)) != 0)
            {
                bit >>= 7;
                bitmask |= bit;

                if ((bit & occupancy) != 0) break;
            }

            return bitmask;
        }
    }

    public class FixedMovementTables
    {
        public ulong[] KING_MOVES_TABLE = new ulong[64];
        public ulong[] KNIGHT_MOVES_TABLE = new ulong[64];

        public FixedMovementTables()
        {
            // King moves
            for (int square = 0; square < 64; square++)
            {
                ulong sq = 1UL << square;
                ulong moves = 0;

                int[] directions = [7, 8, 9, 1];

                foreach (int direction in directions) 
                {
                    moves |= sq << direction;
                    moves |= sq >> direction;
                }

                // If on the left edge, ignore moves that appear on the right edge
                if ((sq & BoardFile.A) != 0) moves &= ~BoardFile.H;
                
                // If on the right edge, ignore moves that appear on the left edge
                if ((sq & BoardFile.H) != 0) moves &= ~BoardFile.A;

                KING_MOVES_TABLE[square] = moves;
            }

            // Knight moves
            for (int square = 0; square < 64; square++)
            {
                ulong sq = 1UL << square;
                ulong moves = 0;
                int[] directions = [17, 15, 10, 6];

                foreach (int direction in directions)
                {
                    moves |= sq << direction;
                    moves |= sq >> direction;
                }

                // If on the left side, ignore any that go onto the right
                if ((sq & BoardFile.AB) != 0) moves &= ~BoardFile.GH;
                
                // If on the right side, ignore any that go onto the left
                if ((sq & BoardFile.GH) != 0) moves &= ~BoardFile.AB;

                KNIGHT_MOVES_TABLE[square] = moves;
            }
        }
    }


    public enum MoveType
    {
        Normal,
        PawnDoublePush,
        Castling,
        EnPassant,
        Promotion
    }

    public partial class Move // TODO: find out what needs Move to be partial
    {
        public int src; // Where the move starts
        public int dst; // Where the move ends
        public MoveType type; // Type of move

        public static Move FromString(string moveString, MoveType type = MoveType.Normal)
        {
            Regex regex = MoveRegex();
            string match = regex.Match(moveString).Value;

            int convert(string value)
            {
                return "12345678".IndexOf(value[1]) * 8 + "abcdefgh".IndexOf(value[0]);
            }

            Move move = new()
            {
                src = convert(match[..2]),
                dst = convert(match.Substring(2, 2)),
                type = type
            };

            return move;
        }

        public override string ToString()
        {
            if (type == MoveType.Castling)
            {
                if (src < dst) // dst is more to the right than src, kingside
                {
                    return "O-O";
                }
                else
                {
                    return "O-O-O";
                }
            }

            static string convert(int index)
            {
                int file = index % 8;
                int rank = index / 8;

                return "abcdefgh"[file].ToString() + "12345678"[rank].ToString();
            }

            return convert(src) + convert(dst);
        }

        public override bool Equals([NotNullWhen(true)] object? other)
        {
            if (other is not Move otherMove) return false;
            
            return src == otherMove.src && dst == otherMove.dst;
        }

        public override int GetHashCode()
        {
            Tuple<int, int> moveTuple = new(src, dst);
            return moveTuple.GetHashCode();
        }


        [GeneratedRegex("^[a-h][1-8][a-h][1-8]$")]
        private static partial Regex MoveRegex();

        
        public static bool operator ==(Move left, Move right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Move left, Move right)
        {
            return !(left == right);
        }
    }

    public static class Moves
    {
        public readonly static MagicBitboards mb = new();
        public readonly static FixedMovementTables fmt = new();

        public static ulong GetBishopMoveBitmask(ulong occupancy, int square)
        {
            ulong occ;

            MagicEntry entry = mb.BISHOP_MAGICS[square];
            
            occ = occupancy & entry.mask;
            occ *= entry.magic;
            occ >>= entry.shift;

            int index = (int)occ + entry.offset + mb.BISHOPS_START_FROM;

            return mb.SLIDING_PIECE_MOVE_TABLE[index];
        }

        public static ulong GetRookMoveBitmask(ulong occupancy, int square)
        {
            ulong occ;

            MagicEntry entry = mb.ROOK_MAGICS[square];

            occ = occupancy & entry.mask;
            occ *= entry.magic;
            occ >>= entry.shift;

            int index = (int)occ + entry.offset;

            return mb.SLIDING_PIECE_MOVE_TABLE[index];
        }

        public static ulong GetQueenMoveBitmask(ulong occupancy, int square)
        {
            return GetBishopMoveBitmask(occupancy, square) | GetRookMoveBitmask(occupancy, square);
        }

        public static void GenerateMovesFromSameSquare(ulong moveBitmask, int startSquare, List<Move> moveListToAddTo)
        {
            while (moveBitmask != 0)
            {
                int LSB_index = BitOperations.TrailingZeroCount(moveBitmask);

                Move move = new()
                {
                    src = startSquare,
                    dst = LSB_index,
                    type = MoveType.Normal
                };

                moveListToAddTo.Add(move);

                moveBitmask ^= 1UL << LSB_index;
            }
        }

        public static void GenerateMovesWithOffset(ulong moveBitmask, int offset, List<Move> moveListToAddTo, MoveType moveFlag = MoveType.Normal)
        {
            while (moveBitmask != 0)
            {
                int LSB_index = BitOperations.TrailingZeroCount(moveBitmask);

                Move move = new()
                {
                    src = LSB_index - offset,
                    dst = LSB_index,
                    type = moveFlag
                };

                moveListToAddTo.Add(move);

                moveBitmask ^= 1UL << LSB_index;
            }
        }


        public static void GenerateRookMoves(
            ulong friendlyOccupancy,
            ulong opponentOccupancy,
            ulong pinmask,
            ulong checkmask,
            int square,
            List<Move> moveListToAddTo
        )
        {
            ulong moveBitmask = GetRookMoveBitmask(friendlyOccupancy | opponentOccupancy, square) & ~friendlyOccupancy;

            moveBitmask &= checkmask;
            
            if ((pinmask & 1UL << square) > 0) moveBitmask &= pinmask;
            
            GenerateMovesFromSameSquare(moveBitmask, square, moveListToAddTo);
        }

        public static void GenerateBishopMoves(ulong friendlyOccupancy, ulong opponentOccupancy, ulong pinmask, ulong checkmask, int square, List<Move> moveListToAddTo)
        {
            ulong moveBitmask = GetBishopMoveBitmask(friendlyOccupancy | opponentOccupancy, square) & ~friendlyOccupancy;

            moveBitmask &= checkmask;
            
            if ((pinmask & 1UL << square) > 0) moveBitmask &= pinmask;
            
            GenerateMovesFromSameSquare(moveBitmask, square, moveListToAddTo);
        }
        
        public static ulong GetKingMoveBitmask(ulong occupancy, int square)
        {
            return ~occupancy & fmt.KING_MOVES_TABLE[square];
        }

        public static void GenerateKingMoves(
            PieceSet friendlyPieces,
            PieceSet opponentPieces,
            int square,
            List<Move> moveListToAddTo
        )
        {
            ulong friendlyOccupancy = friendlyPieces.ALL();
            ulong opponentOccupancy = opponentPieces.ALL();

            ulong boardMask = friendlyOccupancy | opponentOccupancy;

            ulong opponentAttacks = opponentPieces.AttackingBitmask(boardMask);

            ulong moveBitmask = GetKingMoveBitmask(boardMask, square) & ~friendlyOccupancy & ~opponentAttacks;

            GenerateMovesFromSameSquare(moveBitmask, square, moveListToAddTo);
        }

        public static ulong GetKnightMoveBitmask(ulong friendlyOccupancy, int square)
        {
            return ~friendlyOccupancy & fmt.KNIGHT_MOVES_TABLE[square];
        }

        public static void GenerateKnightMoves(
            ulong friendlyOccupancy,
            int square,
            ulong pinmask,
            ulong checkmask,
            List<Move> moveListToAddTo
        )
        {
            ulong moveBitmask = GetKnightMoveBitmask(friendlyOccupancy, square);

            moveBitmask &= checkmask;

            if ((pinmask & 1UL << square) != 0) moveBitmask &= pinmask;

            GenerateMovesFromSameSquare(moveBitmask, square, moveListToAddTo);
        }

        
        public struct PawnMoves
        {
            public ulong LeftAttacks;
            public ulong RightAttacks;
            public ulong SinglePushForward;
            public ulong DoublePushForward;
        }

        
        public static PawnMoves GetWhitePawnMoves(
            ulong whiteMask,
            ulong blackMask,
            ulong pawnBitboard,
            ulong epSquareBitboard,
            ulong HV_pinmask,
            ulong D_pinmask,
            ulong checkmask
        )
        {
            PawnMoves moves;

            blackMask |= epSquareBitboard;

            ulong occupancy = whiteMask | blackMask;

            ulong pawnBitboardForAttacks = pawnBitboard & ~HV_pinmask;

            moves.LeftAttacks = occupancy & (~BoardFile.A & pawnBitboardForAttacks) << 7;
            moves.RightAttacks = occupancy & (~BoardFile.H & pawnBitboardForAttacks) << 9;

            moves.LeftAttacks &= ~whiteMask;
            moves.RightAttacks &= ~whiteMask;

            moves.LeftAttacks &= checkmask;
            moves.RightAttacks &= checkmask;

            ulong pawnBitboardForPushes = pawnBitboard & ~D_pinmask;

            moves.SinglePushForward = ~occupancy & (pawnBitboardForPushes << 8);
            moves.DoublePushForward = ~occupancy & (moves.SinglePushForward & BoardRank.Third) << 8;

            moves.SinglePushForward &= checkmask;
            moves.DoublePushForward &= checkmask;

            return moves;
        }

        public static PawnMoves GetBlackPawnMoves(
            ulong blackMask,
            ulong whiteMask,
            ulong pawnBitboard,
            ulong epSquareBitboard,
            ulong HV_pinmask,
            ulong D_pinmask,
            ulong checkmask
        )
        {
            PawnMoves moves;

            ulong pawnBitboardForAttacks = pawnBitboard & ~HV_pinmask;

            whiteMask |= epSquareBitboard;

            ulong occupancy = whiteMask | blackMask;

            moves.LeftAttacks = occupancy & (~BoardFile.A & pawnBitboardForAttacks) >> 9;
            moves.RightAttacks = occupancy & (~BoardFile.H & pawnBitboardForAttacks) >> 7;

            moves.LeftAttacks &= ~blackMask;
            moves.RightAttacks &= ~blackMask;

            moves.LeftAttacks &= checkmask;
            moves.RightAttacks &= checkmask;

            ulong pawnBitboardForPushes = pawnBitboard & ~D_pinmask;

            moves.SinglePushForward = ~occupancy & (pawnBitboardForPushes >> 8);
            moves.DoublePushForward = ~occupancy & (moves.SinglePushForward & BoardRank.Sixth) >> 8;

            moves.SinglePushForward &= checkmask;
            moves.DoublePushForward &= checkmask;

            return moves;
        }


        public static PawnMoves GetPawnMoves(
            ulong friendlyOccupancy,
            ulong opponentOccupancy,
            ulong pawnBitboard,
            ulong epSquareBitboard,
            ulong HV_pinmask,
            ulong D_pinmask,
            ulong checkmask,
            Colour side
        )
        {            
            if (side == Colour.White)
            {
                return GetWhitePawnMoves(
                    whiteMask: friendlyOccupancy,
                    blackMask: opponentOccupancy,
                    pawnBitboard: pawnBitboard,
                    epSquareBitboard: epSquareBitboard,
                    HV_pinmask: HV_pinmask,
                    D_pinmask: D_pinmask,
                    checkmask: checkmask
                );
            }
            else
            {
                return GetBlackPawnMoves(
                    blackMask: friendlyOccupancy,
                    whiteMask: opponentOccupancy,
                    pawnBitboard: pawnBitboard,
                    epSquareBitboard: epSquareBitboard,
                    HV_pinmask: HV_pinmask,
                    D_pinmask: D_pinmask,
                    checkmask: checkmask
                );
            }
        }

        public static void GenerateWhitePawnMoves(
            ulong whiteMask,
            ulong blackMask,
            ulong pawnBitboard,
            ulong epSquareBitboard,
            ulong HV_pinmask,
            ulong D_pinmask,
            ulong checkmask,
            List<Move> moveListToAddTo
        )
        {
            PawnMoves pawnMoves = GetWhitePawnMoves(
                whiteMask: whiteMask,
                blackMask: blackMask,
                pawnBitboard: pawnBitboard,
                epSquareBitboard: epSquareBitboard,
                HV_pinmask: HV_pinmask,
                D_pinmask: D_pinmask,
                checkmask: checkmask
            );

            // Exclude the en-passant bitboard originally to take down all normal moves
            GenerateMovesWithOffset(pawnMoves.LeftAttacks  & ~epSquareBitboard, 7, moveListToAddTo);
            GenerateMovesWithOffset(pawnMoves.RightAttacks & ~epSquareBitboard, 9, moveListToAddTo);

            // Generate all moves WITH the en passant bitboard only so we can include the en-passant flag
            GenerateMovesWithOffset(pawnMoves.LeftAttacks  & epSquareBitboard, 7, moveListToAddTo, moveFlag: MoveType.EnPassant);
            GenerateMovesWithOffset(pawnMoves.RightAttacks & epSquareBitboard, 9, moveListToAddTo, moveFlag: MoveType.EnPassant);

            GenerateMovesWithOffset(pawnMoves.SinglePushForward, 8, moveListToAddTo);
            GenerateMovesWithOffset(pawnMoves.DoublePushForward, 16, moveListToAddTo, moveFlag: MoveType.PawnDoublePush);
        }

        public static void GenerateBlackPawnMoves(
            ulong whiteMask,
            ulong blackMask,
            ulong pawnBitboard,
            ulong epSquareBitboard,
            ulong HV_pinmask,
            ulong D_pinmask,
            ulong checkmask,
            List<Move> moveListToAddTo
        )
        {
            PawnMoves pawnMoves = GetBlackPawnMoves(
                whiteMask: whiteMask,
                blackMask: blackMask,
                pawnBitboard: pawnBitboard,
                epSquareBitboard: epSquareBitboard,
                HV_pinmask: HV_pinmask,
                D_pinmask: D_pinmask,
                checkmask: checkmask
            );

            // Exclude the en-passant bitboard originally to take down all normal moves
            GenerateMovesWithOffset(pawnMoves.LeftAttacks  & ~epSquareBitboard, -9, moveListToAddTo);
            GenerateMovesWithOffset(pawnMoves.RightAttacks & ~epSquareBitboard, -7, moveListToAddTo);

            // Generate all moves WITH the en passant bitboard only so we can include the en-passant flag
            GenerateMovesWithOffset(pawnMoves.LeftAttacks  & epSquareBitboard, -9, moveListToAddTo, moveFlag: MoveType.EnPassant);
            GenerateMovesWithOffset(pawnMoves.RightAttacks & epSquareBitboard, -7, moveListToAddTo, moveFlag: MoveType.EnPassant);

            GenerateMovesWithOffset(pawnMoves.SinglePushForward, -8, moveListToAddTo);
            GenerateMovesWithOffset(pawnMoves.DoublePushForward, -16, moveListToAddTo, moveFlag: MoveType.PawnDoublePush);
        }

        public static void GeneratePawnMoves(
            PieceSet friendlyPieces,
            PieceSet opponentPieces,
            ulong epSquareBitboard,
            ulong HV_pinmask,
            ulong D_pinmask,
            ulong checkmask,
            List<Move> moveListToAddTo,
            Colour side
        )
        {
            if (side == Colour.White)
            {
                GenerateWhitePawnMoves(
                    whiteMask: friendlyPieces.ALL(),
                    blackMask: opponentPieces.ALL(),
                    pawnBitboard: friendlyPieces.Pawns,
                    epSquareBitboard: epSquareBitboard,
                    HV_pinmask: HV_pinmask,
                    D_pinmask: D_pinmask,
                    checkmask: checkmask,
                    moveListToAddTo: moveListToAddTo
                );
            }
            else
            {
                GenerateBlackPawnMoves(
                    whiteMask: opponentPieces.ALL(),
                    blackMask: friendlyPieces.ALL(),
                    pawnBitboard: friendlyPieces.Pawns,
                    epSquareBitboard: epSquareBitboard,
                    HV_pinmask: HV_pinmask,
                    D_pinmask: D_pinmask,
                    checkmask: checkmask,
                    moveListToAddTo: moveListToAddTo
                );
            }
        }

        public static void GenerateCastlingMoves(
            Colour sideToMove,
            PieceSet friendlyPieces,
            PieceSet opponentPieces,
            byte castlingRights,
            List<Move> moveListToAddTo
        )
        {
            byte KSC = (byte)(sideToMove == Colour.White ? 0b1000 : 0b0010);
            byte QSC = (byte)(sideToMove == Colour.White ? 0b0100 : 0b0001);

            if (CastlingRights.CanCastleKingside(sideToMove, friendlyPieces, opponentPieces)
                && (castlingRights & KSC) != 0)
            {
                Move move = new()
                {
                    /*
                    1 => 6, 2 => 62

                    . . . . k . 2 .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . K . 1 .
                    */
                    src = sideToMove == Colour.White ? 4 : 60,
                    dst = sideToMove == Colour.White ? 6 : 62,
                    type = MoveType.Castling
                };

                moveListToAddTo.Add(move);
            }

            if (CastlingRights.CanCastleQueenside(sideToMove, friendlyPieces, opponentPieces)
                && (castlingRights & QSC) != 0)
            {
                Move move = new()
                {
                    /*
                    1 => 5, 2 => 57

                    . . 2 . k . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . 1 . K . . .
                    */
                    src = sideToMove == Colour.White ? 4 : 60,
                    dst = sideToMove == Colour.White ? 2 : 58,
                    type = MoveType.Castling
                };

                moveListToAddTo.Add(move);
            }
        }
    }
}