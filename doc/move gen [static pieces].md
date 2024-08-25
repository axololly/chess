# Move Generation [Static Pieces]

## Explanation
Static pieces have fixed reach limits that will always remain the same, no matter the position. Pawns cannot move backwards, kings cannot move off of the board - you get the idea.

Because these all have fixed rules, we can generate them all once and store them in a table. Here's an example for all king moves.


## Code
```py
fileA = 0x101010101010101
fileH = 0x8080808080808080


def get_king_moves(square: int):
    directions = [7, 8, 9, 1]
    moves = 0

    current = 1 << square

    for direction in directions:
        moves |= current << direction
        moves |= current >> direction

    if current & fileA:
        moves &= ~fileH
    
    if current & fileH:
        moves &= ~fileA

    return moves


def create_king_table() -> list[int]:
    return [get_king_moves(square) for square in range(64)]
```

This is simple enough to understand. Let's dive a little deeper.

## Explanation

We have a square parameter, say it's 35, then we shift 1 to that amount to create a bitmask like this:

```yml
# bb of 1 << 35
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . 1 . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
```

Then we take an empty bitboard (value of 0) then we OR values onto it, which is like pasting bits onto the bitboard. 

```yml
# current square    # direction: 7      # direction: 8      # direction: 9      # direction: 1
. . . . . . . .     . . . . . . . .     . . . . . . . .     . . . . . . . .     . . . . . . . .
. . . . . . . .     . . . . . . . .     . . . . . . . .     . . . . . . . .     . . . . . . . .
. . . . . . . .     . . 1 . . . . .     . . . 1 . . . .     . . . . 1 . . .     . . . . . . . .
. . . 1 . . . .     . . . . . . . .     . . . . . . . .     . . . . . . . .     . . 1 . 1 . . .
. . . . . . . .     . . . . 1 . . .     . . . 1 . . . .     . . 1 . . . . .     . . . . . . . .
. . . . . . . .     . . . . . . . .     . . . . . . . .     . . . . . . . .     . . . . . . . .
. . . . . . . .     . . . . . . . .     . . . . . . . .     . . . . . . . .     . . . . . . . .
. . . . . . . .     . . . . . . . .     . . . . . . . .     . . . . . . . .     . . . . . . . .
```

Shifting left moves the bit higher up the bitboard and shifting right moves the bit lower down the bitboard. When combining all four of these bitmasks, we get this:
```yml
. . . . . . . .
. . . . . . . .
. . 1 1 1 . . .
. . 1 . 1 . . .
. . 1 1 1 . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
```

However, if the square was 32, this is what the moves would look like:
```yml
. . . . . . . .
. . . . . . . 1
1 1 . . . . . 1
. 1 . . . . . 1
1 1 . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
```

The bits overlap to the other side! To counter this, we include two `if` statements where if the current square is on the A file, we discard any bits from the H file, and vice versa, resulting in this:
```yml
# current bits      # ~fileH            # current & ~fileH
. . . . . . . .     1 1 1 1 1 1 1 .     . . . . . . . .
. . . . . . . 1     1 1 1 1 1 1 1 .     . . . . . . . .
1 1 . . . . . 1     1 1 1 1 1 1 1 .     1 1 . . . . . .
. 1 . . . . . 1     1 1 1 1 1 1 1 .     . 1 . . . . . .
1 1 . . . . . .     1 1 1 1 1 1 1 .     1 1 . . . . . .
. . . . . . . .     1 1 1 1 1 1 1 .     . . . . . . . .
. . . . . . . .     1 1 1 1 1 1 1 .     . . . . . . . .
. . . . . . . .     1 1 1 1 1 1 1 .     . . . . . . . .
```

We bitwise AND our generated mask against all the bits that are not on the H file, removing all the bits on the H file. We can do a similar thing for the A file with:
```py
if current & fileH:
    current &= ~fileA
```

We then create a list comprehension of all the bitmasks and store it in a list.

### _Note: This is written in Python for ease of understanding. Code for a chess engine should be written in a language like C, C++, C# or any other faster language. It is not ideal to use Python for writing a chess engine._


## Pawns and Tables

Pawns are quite special pieces. For a start, their attacks are not aligned with their movements, they can promote to a range of pieces at the backrank and they can do a stupid French move that's really cool to watch.

```yml
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . 3 . . . .
. . 2 1 2 . . .
. . . P . . . .
. . . . . . . .
```

As we can see here:
- the movement square is listed as `1`
- the two-spaces pawn push is listed as `3` (because it's optional)
- the attack squares are listed as `2`'s

You'd think to just take a bitmask of all the pawns, shift it left (up the board) by 7 and 9 then check if they intersect with any of the bits on the occupancy bitboard [continue later]

