# Move Generation [Sliding Pieces]
Sliding pieces can move in any number of squares along the board, and but their moves are halted if "blockers" are in the way. This is for rooks, bishops and queens.

Take this board occupancy as an example. This is a bitmask of all the pieces on the board.
```yml
. . . . . . . .
1 1 1 . 1 . . 1
. . . . . . 1 .
. 1 1 . R . . .
. . 1 . . 1 1 .
. 1 . . . . . 1
. 1 . . 1 . . .
. . . . . . . .
```

Let's say we know there's a rook on E5 and we want a bitmask of where it can move to. How would we go about that?

We can use the power of magic bitboards for this.

Note: This is going to be a very brief overview of the steps. You can read more into this subject at these two sources:

1. [Analog Hors' Article on Bitboards](https://analog-hors.github.io/site/magic-bitboards/)
2. [Chess Programming Wiki's Page about Bitboards](https://www.chessprogramming.org/Magic_Bitboards)


## Magic Bitboards

### Explanation

The concept of magic bitboards is four steps (with examples):

1. Get a mask of the relevant bits in the position
```yml
# pieces on board     # relevant bitmask    # resulting bitmask
. . . . . . . .       . . . . . . . .       . . . . . . . .
1 1 1 . 1 . . 1       . . . . 1 . . .       . . . . 1 . . .
. . . . . . 1 .       . . . . 1 . . .       . . . . . . . .
. 1 1 . R . . .   &   . 1 1 1 . 1 1 1   =   . 1 1 . R . . .
. . 1 . . 1 1 .       . . . . 1 . . .       . . . . . . . .
. 1 . . . . . 1       . . . . 1 . . .       . . . . . . . .
. 1 . . 1 . . .       . . . . 1 . . .       . . . . 1 . . .
. . . . . . . .       . . . . . . . .       . . . . . . . .
```

2. Multiply this resulting bitmask by a magic number to get an index mapping where all the bits are compiled together.
```yml
. . . . . . . .       . . . . . . . .       4 5 A B C D E]
. . . . 5 . . .       . . . . . . . .       . . . .[1 2 3
. . . . 4 . . .       . . . . . . . .       . . . . . . .
. A B C . D E .   *   . .some magic .   =   . . . . . . .
. . . . 3 . . .       . . . bits. . .       . .garbage. .
. . . . 2 . . .       . . . . . . . .       . . . . . . .
. . . . 1 . . .       . . . . . . . .       . . . . . . .
. . . . . . . .       . . . . . . . .       . . . . . . .
```

3. Right shift the index mapping by 64-n bits (n is the number of bits in the relevant bitmask) to create a custom index.
```yml
4 5 A B C D E]                  . . . . . . . .
. . . .[1 2 3                   . . . . . . . .
. . . . . . .                   . . . . . . . .
. . . . . . .                   . . . . . . . .
. .garbage. .  >> (64 - 10)  =  . . . . . . . .
. . . . . . .                   . . . . . . . .
. . . . . . .                   D E]. . . . . .
. . . . . . .                  [1 2 3 4 5 A B C
```

4. Use the index to access a database of pre-generated moves.
```py
rook_moves[square][hashKey]
```

### Implementation

Putting all 4 steps together, we get this:
```py
RELEVANT_ROOK_BITMASKS = [...] # relevant masks fore ach square
ROOK_MAGICS = [...] # 64 magics for each square
ROOK_SHIFTS = [...] # 64 shifts for each magic

# database with moves, set as empty
ROOK_MOVE_TABLE = [[0 for _ in range(4096)] for _ in range(64)]

def shitty_rook_hash(occupancy: int, square: int) -> int:
    occ = occupancy & RELEVANT_ROOK_BITMASKS[square]
    occ *= ROOK_MAGICS[square]
    hashKey = occ >> ROOK_SHIFTS[square]
    return ROOK_MOVE_TABLE[square][hashKey]
```

As for how we add move masks to the table, this is what we do:
```py
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
```

This is an implementation of **plain** magic bitboards: the easiest of the batch. However, we can do better.

You can read all about this and the other code [here in my gist](https://gist.github.com/axololly/fba4d65d2a52898d61bcb07c41b20140).

There is a code sample for plain magic bitboards and documentation for it.


## _Fancy_ Magic Bitboards

For a start, this nested array system is way too slow. In C#, accessing 2D arrays is 35% slower than 1D arrays so we're adding extra overhead for no reason. What we can do is group a few things together for more efficiency.

### Speedup 1: Summing the tables together

Instead of a group of 2D arrays, we can group all the 2D arrays together then include an offset for where to index from. All the arrays have a fixed length of 4096, so we can calculate an offset like this:
```py
def get_rook_moves(occupancy: int, square: int) -> int:
    hashKey = ... # generate hash
    
    offset = square * 4096

    return ROOK_MOVE_TABLE[offset + hashKey]
```

### Speedup 2: Structure of Arrays vs Array of Structures

Currently, the relevant bitmasks, magics and offsets are all in 3 separate arrays, meaning 3 separate accesses. We can group them together in a `record` to end up with just one array of objects with 3 attributes.

```py
class MagicEntry:
    def __init__(self, mask: int, magic: int, shift: int, offset: int) -> None:
        self.mask = mask
        self.magic = magic
        self.shift = shift
        self.offset = offset
```

And the C# equivalent in case you're curious.
```cs
public record MagicEntry
{
    public ulong mask;
    public ulong magic;
    public ulong shift;
    public ulong offset;

    public MagicEntry(ulong mask, ulong magic, ulong shift, ulong offset)
    {
        this.mask = mask;
        this.magic = magic;
        this.shift = shift;
        this.offset = offset;
    }
}
```

We can then assemble these into an array then access the array with the `square` parameter in the function. This way, we have only one lookup in an array then a second for the attacks.

```py
ROOK_ATTACK_TABLE = [...] # 64 * 4096 = 262,144 in length
ROOK_MAGICS: list[MagicEntry] = [...] # 64 rook magics

def get_rook_moves(occupancy: int, square: int) -> int:
    entry = ROOK_MAGICS[square]

    occ = occupancy & entry.mask
    occ *= entry.magic
    key = occ >> entry.shift

    return ROOK_ATTACK_TABLE[entry.offset + key]
```

### Speedup 3: Summing together bishop and rook attack tables

To compact things a bit more, it makes sense to join the rook and bishop tables together, producing a single attacks array of length `262,144 + 32,768 = 294,912`. However, because we blended the two tables together, we need to know where one starts and the other stops.

To fix this, we can create a variable called `BISHOPS_START_FROM` that holds a value of `262,144` then use that as a _second_ offset to the `get_bishop_moves()` function.

```py
SLIDING_PIECE_ATTACKS_TABLE = [...] # length of 294,912 | [0 for _ in range(294_912)]
BISHOP_MAGICS: list[MagicEntry] = [...]

BISHOPS_START_FROM = 262_144

def get_bishop_moves(occupancy: int, square: int) -> int:
    entry = BISHOP_MAGICS[square]

    ... # hash stuff

    index = BISHOPS_START_FROM + entry.offset + key

    return SLIDING_PIECE_ATTACKS_TABLE[index]
```