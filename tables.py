"""
MIT License

Copyright (c) 2024 James Griggs

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
"""


RELEVANT_ROOK_BITMASKS = [
    0x000101010101017E, 0x000202020202027C, 0x000404040404047A, 0x0008080808080876,
    0x001010101010106E, 0x002020202020205E, 0x004040404040403E, 0x008080808080807E,
    0x0001010101017E00, 0x0002020202027C00, 0x0004040404047A00, 0x0008080808087600,
    0x0010101010106E00, 0x0020202020205E00, 0x0040404040403E00, 0x0080808080807E00,
    0x00010101017E0100, 0x00020202027C0200, 0x00040404047A0400, 0x0008080808760800,
    0x00101010106E1000, 0x00202020205E2000, 0x00404040403E4000, 0x00808080807E8000,
    0x000101017E010100, 0x000202027C020200, 0x000404047A040400, 0x0008080876080800,
    0x001010106E101000, 0x002020205E202000, 0x004040403E404000, 0x008080807E808000,
    0x0001017E01010100, 0x0002027C02020200, 0x0004047A04040400, 0x0008087608080800,
    0x0010106E10101000, 0x0020205E20202000, 0x0040403E40404000, 0x0080807E80808000,
    0x00017E0101010100, 0x00027C0202020200, 0x00047A0404040400, 0x0008760808080800,
    0x00106E1010101000, 0x00205E2020202000, 0x00403E4040404000, 0x00807E8080808000,
    0x007E010101010100, 0x007C020202020200, 0x007A040404040400, 0x0076080808080800,
    0x006E101010101000, 0x005E202020202000, 0x003E404040404000, 0x007E808080808000,
    0x7E01010101010100, 0x7C02020202020200, 0x7A04040404040400, 0x7608080808080800,
    0x6E10101010101000, 0x5E20202020202000, 0x3E40404040404000, 0x7E80808080808000
]

ROOK_MAGICS = [
    0x0080001020400080, 0x0040001000200040, 0x0080081000200080, 0x0080040800100080,
    0x0080020400080080, 0x0080010200040080, 0x0080008001000200, 0x0080002040800100,
    0x0000800020400080, 0x0000400020005000, 0x0000801000200080, 0x0000800800100080,
    0x0000800400080080, 0x0000800200040080, 0x0000800100020080, 0x0000800040800100,
    0x0000208000400080, 0x0000404000201000, 0x0000808010002000, 0x0000808008001000,
    0x0000808004000800, 0x0000808002000400, 0x0000010100020004, 0x0000020000408104,
    0x0000208080004000, 0x0000200040005000, 0x0000100080200080, 0x0000080080100080,
    0x0000040080080080, 0x0000020080040080, 0x0000010080800200, 0x0000800080004100,
    0x0000204000800080, 0x0000200040401000, 0x0000100080802000, 0x0000080080801000,
    0x0000040080800800, 0x0000020080800400, 0x0000020001010004, 0x0000800040800100,
    0x0000204000808000, 0x0000200040008080, 0x0000100020008080, 0x0000080010008080,
    0x0000040008008080, 0x0000020004008080, 0x0000010002008080, 0x0000004081020004,
    0x0000204000800080, 0x0000200040008080, 0x0000100020008080, 0x0000080010008080,
    0x0000040008008080, 0x0000020004008080, 0x0000800100020080, 0x0000800041000080,
    0x00FFFCDDFCED714A, 0x007FFCDDFCED714A, 0x003FFFCDFFD88096, 0x0000040810002101,
    0x0001000204080011, 0x0001000204000801, 0x0001000082000401, 0x0001FFFAABFAD1A2
]

ROOK_BIT_SHIFT = 52



RELEVANT_BISHOP_BITMASKS = [
    0x0040201008040200, 0x0000402010080400, 0x0000004020100A00, 0x0000000040221400,
    0x0000000002442800, 0x0000000204085000, 0x0000020408102000, 0x0002040810204000,
    0x0020100804020000, 0x0040201008040000, 0x00004020100A0000, 0x0000004022140000,
    0x0000000244280000, 0x0000020408500000, 0x0002040810200000, 0x0004081020400000,
    0x0010080402000200, 0x0020100804000400, 0x004020100A000A00, 0x0000402214001400,
    0x0000024428002800, 0x0002040850005000, 0x0004081020002000, 0x0008102040004000,
    0x0008040200020400, 0x0010080400040800, 0x0020100A000A1000, 0x0040221400142200,
    0x0002442800284400, 0x0004085000500800, 0x0008102000201000, 0x0010204000402000,
    0x0004020002040800, 0x0008040004081000, 0x00100A000A102000, 0x0022140014224000,
    0x0044280028440200, 0x0008500050080400, 0x0010200020100800, 0x0020400040201000,
    0x0002000204081000, 0x0004000408102000, 0x000A000A10204000, 0x0014001422400000,
    0x0028002844020000, 0x0050005008040200, 0x0020002010080400, 0x0040004020100800,
    0x0000020408102000, 0x0000040810204000, 0x00000A1020400000, 0x0000142240000000,
    0x0000284402000000, 0x0000500804020000, 0x0000201008040200, 0x0000402010080400,
    0x0002040810204000, 0x0004081020400000, 0x000A102040000000, 0x0014224000000000,
    0x0028440200000000, 0x0050080402000000, 0x0020100804020000, 0x0040201008040200
]

BISHOP_SHIFTS = [
    58, 59, 59, 59, 59, 59, 59, 58,
    59, 59, 59, 59, 59, 59, 59, 59,
    59, 59, 57, 57, 57, 57, 59, 59,
    59, 59, 57, 55, 55, 57, 59, 59,
    59, 59, 57, 55, 55, 57, 59, 59,
    59, 59, 57, 57, 57, 57, 59, 59,
    59, 59, 59, 59, 59, 59, 59, 59,
    58, 59, 59, 59, 59, 59, 59, 58
]

BISHOP_MAGICS = [
    0x0002020202020200, 0x0002020202020000, 0x0004010202000000, 0x0004040080000000,
    0x0001104000000000, 0x0000821040000000, 0x0000410410400000, 0x0000104104104000,
    0x0000040404040400, 0x0000020202020200, 0x0000040102020000, 0x0000040400800000,
    0x0000011040000000, 0x0000008210400000, 0x0000004104104000, 0x0000002082082000,
    0x0004000808080800, 0x0002000404040400, 0x0001000202020200, 0x0000800802004000,
    0x0000800400A00000, 0x0000200100884000, 0x0000400082082000, 0x0000200041041000,
    0x0002080010101000, 0x0001040008080800, 0x0000208004010400, 0x0000404004010200,
    0x0000840000802000, 0x0000404002011000, 0x0000808001041000, 0x0000404000820800,
    0x0001041000202000, 0x0000820800101000, 0x0000104400080800, 0x0000020080080080,
    0x0000404040040100, 0x0000808100020100, 0x0001010100020800, 0x0000808080010400,
    0x0000820820004000, 0x0000410410002000, 0x0000082088001000, 0x0000002011000800,
    0x0000080100400400, 0x0001010101000200, 0x0002020202000400, 0x0001010101000200,
    0x0000410410400000, 0x0000208208200000, 0x0000002084100000, 0x0000000020880000,
    0x0000001002020000, 0x0000040408020000, 0x0004040404040000, 0x0002020202020000,
    0x0000104104104000, 0x0000002082082000, 0x0000000020841000, 0x0000000000208800,
    0x0000000010020200, 0x0000000404080200, 0x0000040404040400, 0x0002020202020200
]


CAP_TO_64_BITS = lambda N: N & (1 << 64) - 1

DEBRUIJN64 = 0x07EDD5E59A4E28C2

DEBRUIJN_LOOKUP_TABLE = [
    63,  0, 58,  1, 59, 47, 53,  2,
    60, 39, 48, 27, 54, 33, 42,  3,
    61, 51, 37, 40, 49, 18, 28, 20,
    55, 30, 34, 11, 43, 14, 22,  4,
    62, 57, 46, 52, 38, 26, 32, 41,
    50, 36, 17, 19, 29, 10, 13, 21,
    56, 45, 25, 31, 35, 16,  9, 12,
    44, 24, 15,  8, 23,  7,  6,  5
]

def bit_index(bitmask: int) -> int:
    """
    Return the index of the single bit in the bitmask given.

    Parameters
    ----------
    bitmask: `int`
        the bitmask with only 1 isolated bit.

    Returns
    -------
    `int`
        the index of the isolated bit.

    Raises
    ------
    `ArithmeticError`
        a number without only 1 bit was submitted.
    """

    if bitmask.bit_count() != 1:
        raise ArithmeticError('cannot')

    return DEBRUIJN_LOOKUP_TABLE[CAP_TO_64_BITS(bitmask * DEBRUIJN64) >> 58]


def rook_moves_hash(bitmask: int, square: int):
    """
    Return a custom hash used to access the rook moves table with a bitmask
    of all pieces on the board and square the piece is on.

    Parameters
    ----------
    bitmask: `int`
        the bitmask of the current board.
    square: `int`
        the square the rook is currently on.
    """

    hash = bitmask & RELEVANT_ROOK_BITMASKS[square]
    hash = CAP_TO_64_BITS(hash * ROOK_MAGICS[square])
    hash >>= ROOK_BIT_SHIFT
    
    return hash

def bishop_moves_hash(bitmask: int, square: int):
    """
    Return a custom hash used to access the bishop moves table with a bitmask
    of all pieces on the board and square the piece is on.

    Parameters
    ----------
    bitmask: `int`
        the bitmask of the current board.
    square: `int`
        the square the rook is currently on.
    """

    hash = bitmask & RELEVANT_BISHOP_BITMASKS[square]
    hash = CAP_TO_64_BITS(hash * BISHOP_MAGICS[square])
    hash >>= BISHOP_SHIFTS[square]
    
    return hash


# Empty rook moves and bishop moves table
ROOK_MOVES_TABLE = [[0 for _ in range(4096)] for _ in range(64)]
BISHOP_MOVES_TABLE = [[0 for _ in range(512)] for _ in range(64)]


def mapLinearOccToSquares(squarePositions: list[int], linearOccupation: int) -> int:
    """
    Shift a binary number representing a linear occupation of pieces into a bitmask of the pieces on a bitboard.

    Example can be found in the `doc/tables.md` documentation.
    """

    bitmask = 0 # empty bitmask

    for i in range(linearOccupation.bit_length()): # number of bits in the integer
        # If there is a 1 at the index
        if linearOccupation & 1 << i:
            # bitwise OR a bit at the corresponding
            # location in the squares list
            bitmask |= 1 << squarePositions[i]

    return bitmask


def traceOutRookMoves(occupancy: int, square: int) -> int:
    """
    Trace out all the possible rook moves manually with a bitmask of pieces on the board and a square to start from.

    Parameters
    ----------
    occupancy: `int`
        the bitmask of pieces on the board.
    square: `int`
        the square to start the tracing from.

    Returns
    -------
    `int`
        a bitmask of moves traced out.
    """

    bitmask = 0

    # Get a row of bits so we don't cross to the other side of the board
    bitsOfRank = 0xFF << square // 8 * 8

    
    bit = 1 << square

    while CAP_TO_64_BITS(bit & ~(bit & occupancy)):
        bit <<= 8
        bitmask |= bit
    
    bit = 1 << square

    while CAP_TO_64_BITS(bit & ~(bit & occupancy)):
        bit >>= 8
        bitmask |= bit
    
    bit = 1 << square

    while CAP_TO_64_BITS(bit & ~(bit & occupancy)):
        bit <<= 1

        if bit & bitsOfRank: # if still on rank
            bitmask |= bit
        else:
            break
    
    bit = 1 << square

    while CAP_TO_64_BITS(bit & ~(bit & occupancy)):
        bit >>= 1

        if bit & bitsOfRank: # if still on rank
            bitmask |= bit
        else:
            break
    
    return bitmask


def traceOutBishopMoves(occupancy: int, square: int) -> int:
    """
    Trace out all the possible bishop moves manually with a bitmask of pieces on the board and a square to start from.

    Parameters
    ----------
    occupancy: `int`
        the bitmask of pieces on the board.
    square: `int`
        the square to start the tracing from.

    Returns
    -------
    `int`
        a bitmask of moves traced out.
    """
    
    bitmask = 0
    bitsOfRank = 0xFF << square // 8 * 8

    bit = 1 << square
    bit2 = bit

    while bit & ~(bit & occupancy):
        bit <<= 8 + 1
        bit2 >>= 1

        if bit2 & bitsOfRank:
            bitmask |= bit
        else:
            break
    
    bit = 1 << square
    bit2 = bit

    while bit & ~(bit & occupancy):
        bit <<= 8 - 1
        bit2 <<= 1

        if bit2 & bitsOfRank:
            bitmask |= bit
        else:
            break
    
    bit = 1 << square
    bit2 = bit

    while bit & ~(bit & occupancy):
        bit >>= 8 + 1
        bit2 <<= 1

        if bit2 & bitsOfRank:
            bitmask |= bit
        else:
            break
    
    bit = 1 << square
    bit2 = bit

    while bit & ~(bit & occupancy):
        bit >>= 8 - 1
        bit2 >>= 1

        if bit2 & bitsOfRank:
            bitmask |= bit
        else:
            break
    
    return bitmask


# Instantiate bishop moves
for squarePosition in range(64):
    squares = []
    mask = RELEVANT_BISHOP_BITMASKS[squarePosition]

    while mask:
        LSB = mask & -mask
        squares.append(bit_index(LSB))
        mask ^= LSB

    for linearOccupation in range(1 << len(squares)):
        mappedOccupancy = mapLinearOccToSquares(squares, linearOccupation)
        
        hash = CAP_TO_64_BITS(mappedOccupancy * BISHOP_MAGICS[squarePosition]) >> BISHOP_SHIFTS[squarePosition]
        
        BISHOP_MOVES_TABLE[squarePosition][hash] = traceOutBishopMoves(mappedOccupancy, squarePosition)


# Instantiate rook moves
for squarePosition in range(64):
    squares = []
    mask = RELEVANT_BISHOP_BITMASKS[squarePosition]

    while mask:
        LSB = mask & -mask
        squares.append(bit_index(LSB))
        mask ^= LSB
    
    for linearOccupation in range(1 << len(squares)):
        mappedOccupancy = mapLinearOccToSquares(squares, linearOccupation)

        hash = CAP_TO_64_BITS(mappedOccupancy * ROOK_MAGICS[squarePosition]) >> ROOK_BIT_SHIFT

        ROOK_MOVES_TABLE[squarePosition][hash] = traceOutRookMoves(mappedOccupancy, squarePosition)


def getRookMoves(bitboard: int, square: int) -> int:
    occ = bitboard & RELEVANT_ROOK_BITMASKS[square]
    occ = CAP_TO_64_BITS(occ * ROOK_MAGICS[square])
    key = occ >> ROOK_BIT_SHIFT
    
    return ROOK_MOVES_TABLE[square][key]


def getBishopMoves(bitboard: int, square: int) -> int:
    occ = bitboard & RELEVANT_BISHOP_BITMASKS[square]
    occ = CAP_TO_64_BITS(occ * BISHOP_MAGICS[square])
    key = occ >> BISHOP_SHIFTS[square]

    return BISHOP_MOVES_TABLE[square][key]