# Move Generation [Castling]

This is a move that's a bit complicated and features a few steps. Let's do our best to go through them and find a way to perform all of them.

## Types of Castling

For a start, there's 4 types of castling:

### White Kingside Castling

![Arrow depicting where White's king will end up after castling.](./assets/white%20kingside%20castling.png)

### White Queenside Castling
![Arrow depicting where White's king will end up after castling.](./assets/white%20queenside%20castling.png)

[insert castling type 3]

[insert castling type 4]

Now, in all of these cases, let's try to find some similarities:

1. King always moves two spaces
2. Rook always goes on the opposite side to the direction the king moved (on the right of the king if it moved left)

## Adding Move Flags

Since castling is a "special move", ie. we have to do more than just pick up one piece and put it down on another square, we'll need to include a flag for our moves.

```py
from dataclasses import dataclass
from enum import Enum

class MoveType(Enum):
    Normal = 0
    Castling = 1

@dataclass
class Move:
    src: int
    dst: int
    type: MoveType = MoveType.Normal
```

Now, with this new approach, we can attach additional behaviour to our moves with the use of move types (or move flags as some people call them - I'll be using both).

Now, with this new castling flag, when we play a move, we can check to see if it's a castling move, then we can add extra move sugar onto it, like moving rooks at the same time.

For reference, this is how I structure my bitboards. I use a Little Endian format:
```yml
56 57 58 59 60 61 62 63
48 59 50 51 52 53 54 55
40 41 42 43 44 45 46 47
32 33 34 35 36 37 38 39
24 25 26 27 28 29 30 31
16 17 18 19 20 21 22 23
 8  9 10 11 12 13 14 15
 0  1  2  3  4  5  6  7
```

## What goes where?`

There are four different types of castling: white queenside, white kingside, black queenside and black kingside. Let's arrange these in a table, along with where the king starts and ends up.

| Colour | Side | King Start | King End
|:-:|:-:|:-:|:-:
| White | Queenside | 4 | 2 
| Black | Queenside | 60 | 58
| White | Kingside | 4 | 6
| White | Queenside | 60 | 62

As you can see, the king positions are always 2 off, so we don't need to do much for that.

```py
# for kingside castling
move = Move(
    src = current,
    dst = current + 2,
    type = MoveType.Castling
)

# for queenside castling
move = Move(
    src = current,
    dst = current - 2,
    type = MoveType.Castling
)
```

However, we need to know where the _rooks_ are going to be. As a table, this looks like:

| Colour | Side | Rook Start | Rook End
|:-:|:-:|:-:|:-:
|White|Queenside|0|3
|White|Kingside|7|5
|Black|Queenside|56|59
|Black|Kingside|63|61

We can of course hardcode the first part of this table using a dictionary which is dependent on the end square of the king.

```py
rookPositions = {
    2: 0,   # WQ's rook
    6: 7,   # WK's rook
    58: 56, # BQ's rook
    62: 63  # BK's rook   
}

rookPos = rookPositions[move.dst]
```

In my C# code I use a `switch` statement, which looks like this:
```cs
int rookPos = move.dst switch
{
    2 => 0,
    6 => 7,
    58 => 56,
    62 => 63
};
```


## What next?

We have our start rook positions. However, where are they going to end up?

![Where White's king and rook end up after kingside castling.](./assets/where%20the%20rook%20and%20king%20will%20go%20after%20castling.png)

When castling, notice the two squares the king moves from and to, and where the rook ends up.

If you look closely, the rook always lands between the start king position and the end king position, so, to get where the rook is going to end up, it's as easy as "averaging" between the start square and the end square: the middle number between 1 and 3 is 2, and that can be calculated with (1 + 3) / 2 which equals 2.

Or in general, this formula: (start + end) / 2.

```py
endRookPos = (move.src + move.dst) / 2
```

## What does this all mean?

Well, now that we have the start and end for both the king and rook, we can treat it like _two normal moves_ instead of one.

I use a bit trick I learnt from
Sebastian Lague in my code that ORs together the start and end bit, then XORs it against the corresponding piece bitboard to flip the bit for the old square off and the bit for the new square on:

```py
rookBitboard ^= 1 << rookPos | 1 << endRookPos
kingBitboard ^= 1 << move.src | 1 << move.dst
```

## Castling Rules

So we can castle in terms of moves: now we need to integrate castling with legality. Castling is allowed if:

1. You haven't moved the king or  the corresponding rook
2. The squares between the king and the rook are vacant
3. The squares between the king and the rook are not under attack
4. You cannot castle out of check

Let's go from the top.

### Piece Lookup

For ease of access, I store a `Piece[64]` array which is an array of only attributes of a Piece enum I have laid out like this in C#:
```cs
public enum Piece
{
    WhitePawn,
    BlackPawn,
    WhiteKnight,
    // Other pieces...
    Empty
}

Piece[] BoardArray = new Piece[64];
Array.Fill(BoardArray, Piece.Empty)
```

The same can be done in Python:
```py
from enum import Enum

class Piece(Enum):
    WhitePawn = 0
    BlackPawn = 1
    # Other pieces...
    Empty = 12
    
BoardArray = [Piece.Empty for _ in range(64)]
```

Then, when we want to lookup a piece on `X` square, we can do `BoardArray[X]` to directly get the piece type involved.

Although, do keep in mind that when we want to pretty print our board, we need to iterate through the ranks backwards. Otherwise, we'll end up needing to constantly flip between bitboard squares and array squares to make everything line up.

### 1. Haven't moved the rook or the king

In our encompassing `Board` class, we have a function to make a move: for me, mine is appropriately named `MakeMove()`. In that function where we process the move, we can check for the piece that we're moving.

```py
def make_move(self, move: Move):
    pieceMoving: Piece = BoardArray[move.src]
    currentKing: Piece = Piece(Piece.BlackKing.value - self.moveCounter & 1)
    currentRook: Piece = Piece(Piece.BlackRook.value - self.moveCounter & 1)
    
    if pieceMoving == currentKing:
        if currentKing == Piece.WhiteKing:
            self.castlingRights ^= 0b1100 # remove white rights
    else:
        self.castlingRights ^= 0b0011 # remove black rights
      
    if pieceMoving == Piece.WhiteRook:
        if move.src < self.White.KingSquare: # left of king, queenside
            self.castlingRights ^= 0b0100
        else: # right of king, kingside
            self.castlingRights ^= 0b1000
      
   ...
```

Now, if we move our king, we remove all our castling rights for the rest of the game. And if we move a rook, we remove our castling rights for just that side and all the others stay intact.

### 2. The squares between the rook and the king are vacant

In our basic implementation that only enforces the rights themselves, castling is dependent on whether or not the rook or king have moved. In this case below, our program says "Yes, you can castle here":

![White cannot castle here because a piece is blocking its path.](./assets/white%20cannot%20castle%20here.png)

The bishop is in the way of where we want to castling, which prevents us from doing so.

The easiest way to check for this is to hardcode four castling zones:

- one for White on the queenside
- one for White on the kingside
- one for Black on the queenside
- one for Black on the kingside

These "zones" are just integers that hold the locations of where we can castle.

For example, this is what white's kingside zone looks like as a bitboard:
```yml
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . 1 1 .
```

These are the only bits we care about when it comes to checking if there are pieces in the way on the bitboard.

We can then OR together all the bitboards to get one giant mask that covers all the pieces on the board:
```yml
1 1 1 1 1 1 . 1
1 1 1 1 . 1 1 1
. . . . . 1 . .
. . . . 1 . . .
. . . . 1 . . .
. . . . . 1 . .
1 1 1 1 . 1 1 1
1 1 1 1 1 1 . 1
```

And then AND this against the castling region. If we get a non-zero result then there are pieces blocking White from castling, but if it's zero, then White can castle freely on that side.

```yml
# all bitboards     # castling zone   # end result
1 1 1 1 1 1 . 1     . . . . . . .     . . . . . . .
1 1 1 1 . 1 1 1     . . . . . . .     . . . . . . .
. . . . . 1 . .     . . . . . . .     . . . . . . .
. . . . 1 . . .     . . . . . . .     . . . . . . .         (non-zero result -> cannot castle)
. . . . 1 . . .  &  . . . . . . .  =  . . . . . . .
. . . . . 1 . .     . . . . . . .     . . . . . . .
1 1 1 1 . 1 1 1     . . . . . . .     . . . . . . .
1 1 1 1 1 1 . 1     . . . . 1 1 .     . . . . 1 . .
```

As you can see, this is a non-zero result, so we cannot castle kingside in this position.

### 3. The squares between the king and rook must not be under attack.

Now, this is a bit tricky. We have two ways:

1. Using a "superpiece" method (explained further down)
2. Using a bitmask of all the attacked squares

With the super piece method, we get all the squares where the opponent's pieces would have to be to attack a given square and check if any of the opponent's pieces are there. With the bitmask method, we check to see all the squares the opponent _is already attacking_ and check to see if the bit representing a given square falls under those attacked bits using a bitwise AND and checking for a non-zero result.

If that sounds confusing, don't worry. I'll show some diagrams.

![Black's knight has White's king in check.](./assets/black%20bishop%20preventing%20castling.png)

As you can see, Black's bishop is attacking the square next to our king, which prevents us from castling because if you think about it, it's moving two squares in one move, so you'd technically be leaving your king to be taken "on the next move".

So, let's evaluate our two methods.

## Checking Attacks: Attacking Bitmask

What we do is go through all the pieces of one side (I have a `PieceSet` class for this) and create a big bitmask of all the squares each piece attacks. A square that's attacked by a piece is simply a square where a piece can be moved to. If you play `c4` in the position I showed, the bishop can capture it, because the bishop could move to it in the first place.

There are 6 types of pieces:
1. Pawns
2. Knights
3. Bishops
4. Rooks
5. Queens
6. Kings

This is a bit of a lie: queens are just rooks and bishops taped together, so we can shorten this list.
1. Pawns
2. Knights
3. Bishops | Queens
4. Rooks | Queens
5. Kings

Amazing. Now, the final part. All we have to do is get the move bitmasks for each bit index in each bitboard. We can use a `.PopLSB()` method to do this for us, returning the index of the bit once we remove it from the bitboard. For this, I'll use C# since it makes more sense than Python.

```cs
public struct Bitboard
{
    private ulong bitboard;

    // ...

    public int PopLSB()
    {
        // Check if we try to pop an LSB off an empty board
        if (bitboard == 0) throw new Exception("cannot pop LSB off an empty board.");

        // Get index of least significant bit
        int LSB = BitOperations.TrailingZeroCount(bitboard);

        // Remove least significant bit from bitboard
        bitboard &= bitboard - 1;

        // Return LSB
        return LSB;
    }
}
```

### What's the deal with the check beforehand?

The reason we check beforehand and raise an error if the bitboard is empty, I'll explain now for you. We'll use a byte for this example, since it's only 8 bits and makes the demonstration easier.

Say we have a byte of `0b10010100`. If we count trailing zeroes, we get this:
```yml
                   x  2  1
                   v  v  v
0b  1  0  0  1  0  1  0  0
```

Now this 2 is actually the exact position of the LSB. `1 << 2` produces `0b00000100` which is 4. Since all the zeroes gets really visually noisy, I'll use full stops instead, so instead of `0b00000100`, you'll see `. . . . . 1 . .` which is a lot more visually pleasing.

Now we remove this LSB using `bb & (bb - 1)` which you can see how it works below.
```yml
                      LSB
                       v
    0b  1  0  0  1  0  1  0  0     |  bb
 &  0b  1  0  0  1  0  0  1  1     |  bb - 1
    --------------------------
    0b  1  0  0  1  0  0  0  0     |  bb & (bb - 1)
```

As a comparison to double-check, you can see here that the second binary number is the same as the first but with the LSB removed.

```yml
    0b  1  0  0  1  0  1  0  0     |  bb
    0b  1  0  0  1  0  0  0  0     |  bb & (bb - 1)
```

On this new byte, we count trailing zeroes again:
```yml
             x  4  3  2  1
             v  v  v  v  v
0b  1  0  0  1  0  0  0  0
```

4 trailing zeroes. Now remove the LSB and repeat.
```yml
    x  7  6  5  4  3  2  1
    v  v  v  v  v  v  v  v
0b  1  0  0  0  0  0  0  0
```

7 trailing zeroes. Now remove the LSB and repeat.

At the end, we get left with this:
```yml
0b  0  0  0  0  0  0  0  0
```

Our byte is fully consumed: it is now 0. However, watch what happens when we count the bits again:
```yml
    8  7  6  5  4  3  2  1
    v  v  v  v  v  v  v  v
0b  0  0  0  0  0  0  0  0    
```

We have 8 trailing zeroes. Now, bytes in C# are capped to 255, so doing `1 << 8` loops back around to `1 << 0`, which is of course 1. So, if we wanted to get a bitmask of where the LSB was like `1UL << bb.PopLSB()`, we'd end up looping back to `1`, creating a ghost bit on our bitboard.

That's why we check beforehand to prevent this issue. I ran into an issue while generating moves where it hung and didn't produce output _because_ it was freaking out that the bitboard wasn't entirely consumed because it was kept alive by that ghost bit. This section is in here, so you don't make the same mistake I did.

### Generating Attacks

Say our bishop bitboard looks like this:
```yml
. . . . . . . .
. . . . 1 . . .
. . 1 . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
```

What we do is get the indexes of both of these bishop bits, which come out to be squares `42` and `53`, then we get all the bishop moves for those bits with our sliding piece moves function from earlier. (with an occupancy of `0`, pretend our board is empty)

ORing the two attack bitboards together gets us this bitboard:
```yml
1 . . 1 1 1 . .
. 1 . 1 . . . .
. . . 1 . 1 . .
. 1 1 1 . . 1 .
1 1 . . 1 . . 1
1 . . . . 1 . .
. . . . . . 1 .
. . . . . . . 1
```

Now, this is our bishop attack bitboard. We do the same for rooks, queens, pawns and everything else. Do note that we have to generate pawns separately because we make sure that pawns can't move diagonally to squares without an enemy piece, which of course, doesn't work if you want all the squares that _can_ be attacked by a piece.

As a speedup, you can group bishops and queens together, then rooks and queens together instead of doing all 3 separately because a queen is really just a bishop taped to a rook - they can be treated like a rook in one and treated like a bishop in the other.

## Checking Attacks: Super Piece

The idea is simple. Say we want to check if `e3` is attacked by our opponent. Let's turn that into a bitmask and take a look at it below:
```yml
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . 1 . . .
. . . . . . . .
. . . . . . . .
```

Now, we need to check _all_ of the opponent's pieces, one after the other. I'll explain why.

### Misconception

I'll admit, I made this error too. I thought a "super piece" meant I should do this:
```yml
# All attack rays   # Opponent pieces   # Attackers
. . . . 1 . . .     . . . . 1 . 1 .     . . . . 1 . . .
1 . . . 1 . . .     . . . 1 . . . .     . . . . . . . .
. 1 . . 1 . . 1     . . . . . . 1 .     . . . . . . . .
. . 1 1 1 1 1 .     . . 1 . . . . .     . . . . . . . .
. . 1 1 1 1 1 .  &  1 . . . 1 . . .  =  . . . . 1 . . .
1 1 1 1 . 1 1 1     1 . . . . . . .     1 . . . . . . .
. . 1 1 1 1 1 .     . 1 . . . . . .     . . . . . . . .
. . 1 1 1 1 1 .     . . . . . . . .     . . . . . . . .
```

The problem with this is that a knight could lie on the queen's rays and this would treat the knight (which couldn't attack the square anyway) as another bishop/rook/queen that _could_ attack the piece.

There's no differentiation which is why this implementation doesn't work.

### Correct Solution

Let's check knights to start with. We ask the question: where would a knight need to be to attack `e3`? Marking down the squares, we get these resulting bits:
```yml
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . 1 . 1 . .
. . 1 . . . 1 .
. . . . . . . .
. . 1 . . . 1 .
. . . 1 . 1 . .
```

Notice anything?

These are the exact same bits as if we looked for knight moves from `e3` instead of looking for all the squares that a knight would have to be to attack `e3`. In short, we generate bitmasks with the positions in reverse.

Say the opponent's knight bitboard looks like this:
```yml
. . . . . . . .
. . . . . . . .
1 . . . . . . .
. . . . . 1 . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
```

Instead of generating two sets of attacks for both knights, we can do a bitwise AND on the enemy knight bitboard and the squares to check to get a bitmask of where the knights attacking the square are.

Hopefully, the diagram below makes more sense.
```yml
# Enemy knights     # Attack squares    # Knight checkers
. . . . . . . .     . . . . . . . .     . . . . . . . .
. . . . . . . .     . . . . . . . .     . . . . . . . .
1 . . . . . . .     . . . . . . . .     . . . . . . . .
. . . . . 1 . .     . . . 1 . 1 . .     . . . . . 1 . .
. . . . . . . .  &  . . 1 . . . 1 .  =  . . . . . . . .
. . . . . . . .     . . . . . . . .     . . . . . . . .
. . . . . . . .     . . 1 . . . 1 .     . . . . . . . .
. . . . . . . .     . . . 1 . 1 . .     . . . . . . . .
```

Now that we see that there are knights attacking, we can immediately return `true` to notify that this square is under attack.

### Continuing for other pieces

Again with the process of checking `e3`, let's do bishops. Here are our bitboards:
```yml
# Enemy bishops     # Attack lines      # Bishop attacks
. . . . . . . .     . . . . . . . .     . . . . . . . .
. . . 1 . . . .     1 . . . . . . .     . . . . . . . .
. 1 . . . . . .     . 1 . . . . . 1     . 1 . . . . . .
. . . . . . . .     . . 1 . . . 1 .     . . . . . . . .
. . . . . . . .  &  . . . 1 . 1 . .  =  . . . . . . . .
. . . . . . . .     . . . . . . . .     . . . . . . . .
. . . . . . . .     . . . 1 . 1 . .     . . . . . . . .
. . . . . . . .     . . 1 . . . 1 .     . . . . . . . .
```

As you can see, there are bishops attacking the square too.

The process is identical for all the other pieces:
1. Generate the squares where an enemy piece type would have to be to attack the square
2. Bitwise AND it against the bitboard corresponding to the enemy piece type
3. The result is the attackers on that square

## 