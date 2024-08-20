# `tables.py` Documentation

This is a markdown file created to annotate every aspect of the `tables.py` file, preventing it from being
flooded with comments and docstrings to keep it clean and concise.



## Rook Dependencies


### `RELEVANT_ROOK_BITMASKS`

An array of 64 bitmasks representing only the bits on the bitboard given that are relevant for
determining the movement of the rook.

We can exclude the bits on the outer edges because whether the square at the end of the line has a
piece or not, it does not affect the possible moves of the piece, allowing us to trim down the bits
required for indexing the table.

For example, this is the relevant bitmask for the A1 square:

```yml
. . . . . . . .
1 . . . . . . .
1 . . . . . . .
1 . . . . . . .
1 . . . . . . .
1 . . . . . . .
1 . . . . . . .
. 1 1 1 1 1 1 .
```


### `ROOK_MAGICS`

_By far the most important part of this project._

A hardcoded array of 64 magic numbers (taken from Pradyumna Kannan's `magicmoves.c` project) that
we use for hashing to create an index into the rook moves lookup table.

We hardcode them to ensure low loading times.

These magics are also said to be "very close to optimal", according to Pradyumna, ensuring optimal performance.


### `ROOK_BIT_SHIFT`

A constant with a value of `52` that is used when hashing to create an index into the table.



## Bishop Dependencies


### `RELEVANT_BISHOP_BITMASKS`

An array of 64 bitmasks representing only the bits on the bitboard we care about, that are relevant for
determining the movement of the bishop.

For example, this is the bitmask of the C3 bishop:
```yml
. . . . . . . .
. . . . . . 1 .
. . . . . 1 . .
. . . . 1 . . .
. 1 . 1 . . . .
. . . . . . . .
. 1 . 1 . . . .
. . . . . . . .
```


### `BISHOP_SHIFTS`

An array of relative shifts for indexing into the bishop table.

Calculated by `64 - index_bits` where `index_bits` is the number of bits in the corresponding relevant bitmask.

```py
# List comprehension example with the bitmasks from earlier.
ROOK_SHIFTS = [64 - bitmask.bit_count() for bitmask in RELEVANT_ROOK_BITMASKS]
```


### `BISHOP_MAGICS`

_By far the most important part of this project._

A hardcoded array of 64 magic numbers (taken from Pradyumna Kannan's `magicmoves.c` project) that
we use for hashing to create an index into the bishop moves lookup table.

We hardcode them to ensure low loading times.

These magics are also said to be "very close to optimal", according to Pradyumna, ensuring optimal performance.



## Bitwise Dependencies


### `CAP_TO_64_BITS`

Because this was taken from a lower-level language, where there is a fixed maximum 64-bit integer limit, and
translated to a language without that integer limit, we must manually cap values to be only 64 bits long.

I have done this through a constant lambda function called `CAP_TO_64_BITS` that takes a number as an input then
does a bitwise AND (`&`) with a value of `(1 << 64) - 1` to trim it down to be less than 64 bits.

```yml
                        . 1 . . . 1 1 1  # bits higher than 
                        . . . . 1 . . .  # 1 << 64 get trimmed
# (1 << 64) - 1         ---------------
1 1 1 1 1 1 1 1         . . . . . . . .         . . . . . . . .
1 1 1 1 1 1 1 1         . . . . . 1 . .         . . . . . 1 . .
1 1 1 1 1 1 1 1         . . . . . . . .         . . . . . . . .
1 1 1 1 1 1 1 1    &    . . . 1 . . . .    =    . . . 1 . . . .
1 1 1 1 1 1 1 1         . . . . . 1 . .         . . . . . 1 . .
1 1 1 1 1 1 1 1         . . 1 1 . . . .         . . 1 1 . . . .
1 1 1 1 1 1 1 1         . . . . . . . .         . . . . . . . .
1 1 1 1 1 1 1 1         . . . . . . 1 .         . . . . . . 1 .
```


### `DEBRUIJN64`

A special 64-bit integer (packed as a hexadecimal) with a value of `0x07EDD5E59A4E28C2`.

This is used to find the index of an isolated LSB (least significant bit) in a binary 64-bit number.


### `DEBRUIJN_LOOKUP_TABLE`

A hardcoded array of the numbers from 0 to 63 (inclusive) used in combination with `DEBRUIJN64` to find the index of an isolated LSB in a 64-bit number.

It works by multipying the isolated LSB by the `DEBRUIJN64` number, then right-shifting by 58 to produce an index of 6 bits in length, which is conventiently a number less than 64.

Remember that since we are in Python, we have to manually cap our integers to be no more than 64 bits long.


### `bit_index(bitmask: int)`

This is a helper function that indexes the De Bruijn lookup table for us with an isolated bit input.



## Hashing Functions


### `rook_moves_hash(bitmask: int, square: int)`

A helper function to calculate and return the hash value of a bitmask and the square where a piece is located.

This works with 4 steps:

1. Getting the relevant bits of the bitmask by indexing for the relevant mask using the `square` parameter.
2. Multiplying the relevant bits by a specific magic number hardcoded earlier.
3. Cap the number to be 64 bits long (multiplication would lead to it being more)
4. Shift all the bits to the right by the `ROOK_BIT_SHIFT` constant to get a custom index into the moves table.


### `bishop_moves_hash(bitmask: int, square: int)`

A helper function to calculate and return the hash value of a bitmask and the square where a piece is located.

This works with 4 steps:

1. Getting the relevant bits of the bitmask by indexing for the relevant mask using the `square` parameter.
2. Multiplying the relevant bits by a specific magic number hardcoded earlier.
3. Cap the number to be 64 bits long (multiplication would lead to it being more)
4. Shift all the bits to the right by the relevant bit shift, found in the `BISHOP_SHIFTS` array, to get a custom index into the moves table.



## Tables


### `ROOK_MOVES_TABLE`

A 2D array of size 64x4096 that gets filled up at certain indexes with a bitmask of the available moves with blockers included.

Defined as:
```py
ROOK_MOVES_TABLE = [[0 for _ in range(4096)] for _ in range(64)]
```


### `BISHOP_MOVES_TABLE`

A 2D array of size 64x512 that gets filled up at specific indexes with a bitmask of the available moves with the given blockers.

Defined as:
```py
BISHOP_MOVES_TABLE = [[0 for _ in range(512)] for _ in range(64)]
```


## Move Generation


### `mapLinearOccToSquares(squares: list[int], linearOccupation: int)`

Map an 8-bit number, representing the pieces occupying a line, to its 64-bit representation on the board.

An example can be found below of how the mapping works for the diagonal mask for the `A1` square.

```yaml
                        . . . . . . . .
                        . . . . . . 1 .
                        . . . . . 2 . .
                        . . . . 3 . . .
                 ->     . . . 4 . . . .
                        . . 5 . . . . .
                        . 6 . . . . . .
1 2 3 4 5 6 . .         . . . . . . . .
                        ^
                       here
```


### `traceOutRookMoves(occupancy: int, square: int)`

Trace out all the possible rook moves manually with a bitmask of pieces on the board and a square to start from, using 4 `while` loops counting in each direction and stopping if:

- the end of the board is reached.
- a blocking piece is encountered.


### `traceOutBishopMoves(occupancy: int, square: int)`

Trace out all the possible bishop moves manually with a bitmask of pieces on the board and a square to start from, using 4 `while` loops counting in each direction and stopping if:

- the end of the board is reached.
- a blocking piece is encountered.



## Final Loops

There are two `for` loops that fill in slots of the rook and bishop moves tables. This is how they work.


### Bishop Moves Loop

```py
for squarePosition in range(64): # step 1
    squares = []
    mask = RELEVANT_BISHOP_BITMASKS[squarePosition]

    # step 2
    while mask:
        LSB = mask & -mask
        squares.append(bit_index(LSB))
        mask ^= LSB

    # step 3
    for linearOccupation in range(1 << len(squares)):
        # step 4
        mappedOccupancy = mapLinearOccToSquares(squares, linearOccupation)
        
        # step 5
        hash = CAP_TO_64_BITS(mappedOccupancy * BISHOP_MAGICS[squarePosition]) >> BISHOP_SHIFTS[squarePosition]
        
        # step 6
        ROOK_MOVES_TABLE[squarePosition][hash] = traceOutBishopMoves(mappedOccupancy, squarePosition)
```

**Steps:**

1. Loop through all 64 squares of the board
2. Get the indexes of all the bits in the corresponding relevant bitmask
3. Loop through all possible combinations of occupancies
4. Trace out possible moves for all of these occupancies
5. Get the hash key of where in the table to insert the bitmask.
6. Insert this occupancy into the table at that designated hash location

The part that correlates to step 2 is this:
```py
while mask:
    LSB = mask & -mask
    squares.append(bit_index(LSB))
    mask ^= LSB
```

The variables `mask` is some integer with some number of bits and when all those bits have been checked, `mask` will be 0 which Python treats as `False`.

The part `mask & -mask` is a well-known bit twiddling trick used to extract the LSB (least significant bit) in the number, then we call `bit_index()` on it to get the index of that bit, which gets added to the `squares` list, and then we XOR (remove) the LSB from the mask.


### Rook Moves Loop

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

**Steps:**

1. Loop through all 64 squares of the board
2. Get the indexes of all the bits in the corresponding relevant bitmask
3. Loop through all possible combinations of occupancies
4. Trace out possible moves for all of these occupancies
5. Insert this occupancy into the table at a designated hash location

_All the parts related to each step are the same as in the section above._