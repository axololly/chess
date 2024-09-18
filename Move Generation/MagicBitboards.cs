using Types.Bitboards;
using Chess.Utilities;

namespace Chess.Magics
{
    public record MagicEntry
    {
        public Bitboard mask;
        public Bitboard magic;
        public int shift;
        public int offset;

        public MagicEntry(Bitboard mask, Bitboard magic, int shift, int offset)
        {
            this.mask = mask;
            this.magic = magic;
            this.shift = shift;
            this.offset = offset;
        }
    }

    public struct MagicBitboards
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
        public Bitboard[] SLIDING_PIECE_MOVE_TABLE = new Bitboard[294_912];

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

                    // Carry-Ripper trick to iterate through all occupancies of mask
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

                    // Carry-Ripper trick to iterate through all occupancies of mask
                    combo = (combo - entry.mask) & entry.mask;
                
                } while (combo != 0);
            }
        }

        public static Bitboard TraceOutRookMoves(Bitboard occupancy, int square)
        {
            Bitboard bitmask = 0;
            
            // Get a row of bits os we don't cross to the other side of the board
            Bitboard bitsOfRank = 0xFFUL << 8 * (square / 8);

            // Moving up
            Bitboard bit = 1UL << square;

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

        public static Bitboard TraceOutBishopMoves(Bitboard occupancy, int square)
        {
            Bitboard bitmask = 0;

            // Set boundaries so we don't leave the board
            Bitboard topLeft  = Files.A | Ranks.Eighth;
            Bitboard topRight = Files.H | Ranks.Eighth;
            
            Bitboard bottomLeft  = Files.A | Ranks.First;
            Bitboard bottomRight = Files.H | Ranks.First;

            Bitboard bit = 1UL << square;

            // Going to top left
            while ((bit & ~(bit & topLeft)) != 0)
            {
                bit <<= 7;
                bitmask |= bit;

                if ((bit & occupancy) != 0) break;
            }

            bit = 1UL << square;

            // Going to top right
            while ((bit & ~(bit & topRight)) != 0)
            {
                bit <<= 9;
                bitmask |= bit;

                if ((bit & occupancy) != 0) break;
            }

            bit = 1UL << square;

            // Going to bottom left
            while ((bit & ~(bit & bottomLeft)) != 0)
            {
                bit >>= 9;
                bitmask |= bit;

                if ((bit & occupancy) != 0) break;
            }

            bit = 1UL << square;

            // Going to bottom right
            while ((bit & ~(bit & bottomRight)) != 0)
            {
                bit >>= 7;
                bitmask |= bit;

                if ((bit & occupancy) != 0) break;
            }

            return bitmask;
        }
    }
}