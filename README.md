# Chess Move Generator v1.1.6

If you can't beat em, join em. And that's what I did when I stayed inside for the last month every evening developing this chess move generator.

_If I ever hate myself enough to make a Python version, I'll link it here._

Documentation will probably be coming soon, although I'm not sure whether I should include docstrings, manually written documentation in markdown, or both. I'll decide at some point. Right now, I have cooler stuff to be making.

## Using my project

Quick heads up: I used `.NET 8.0` for this.

If you want to use this library in your own code, you can find it on NuGet under the name `Axololly.Chess.MoveGenerator`. A new version was published because I'm a little bit dumb and I missed a few things.

I'll probably add PGN inputting and outputting at some point. Don't hold me accountable for this.

### Example Code

```cs
using Chess;
using Chess.MoveGen; // Used for the Move struct

class Program
{
    static void Main()
    {
        // Create an empty board.
        // Use a null value because structs will default to null
        // if you use the parameterless constructor. 
        Board board = new(null);

        // Get a list of the next moves
        List<Move> nextMoves = board.GenerateLegalMoves();

        // Print out the board and print out the next possible moves.
        Console.WriteLine($"Board:\n{board}\n");
        Console.WriteLine($"Next moves: [{string.Join(", ", nextMoves)}]");
    }
}
```

### Result
```yml
Board:
r n b q k b n r
p p p p p p p p
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
P P P P P P P P
R N B Q K B N R

Next moves: [b1a3, b1c3, g1f3, g1h3, a2a3, b2b3, c2c3, d2d3, e2e3, f2f3, g2g3, h2h3, a2a4, b2b4, c2c4, d2d4, e2e4, f2f4, g2g4, h2h4]
```

## Fixes

So, for my 6th pull request, I believe it is, it turns out I messed a few things up, mostly to do with outside move generation:

### Three-fold Repetition

When checking for violation of the three-fold repetition rule, I was counting repeated **moves** instead of repeated **positions**. To fix this, I added in a PCG algorithm called `XorRandom` that with a given seed, can generate consistently random numbers used for hashing a position.

For counting them, I took inspiration from Python's `collections.Counter` class and constructed this:
```cs
Dictionary<ulong, int> pastHashes = [];

foreach (ulong pastHash in PastZobristHashes)
{
    if (!pastHashes.TryAdd(pastHash, 1))
    {
        if (pastHashes[pastHash]++ == 2) return true;
    }
}
```

There was also a counting issue where I was checking for four-fold repetition in this snippet below:
```cs
if (pastHashes[pastHash]++ == 3)
```

This is because the `++` operator increments _after_ evaluation, so it _would_ have been 3, but only after this `if` statement, which wouldn't have worked.

Thankfully, that's all fixed now.

If you're curious, the hash of a position can be retrieved in the `ZobristKey` property. This is for both of the Chess and Chess960 board classes.

### 50-Move Rule

When checking for violation of the 50-move rule, I handled the clock wrong in two ways:

1. Incrementing after each side has made a move instead of when both sides have made a move
2. Resetting the clock after a pawn **promotes** (supposed to be when a pawn moves)

Fixing these issues was fairly easy: for the first one, I can just treat it as a 100-move clock and then in the FEN strings, divide it by 2 and discard any stray halves, and for the second one, I just had to check that the piece moving was a white or black pawn.

Another issue arose where after a double-push move like `1. e4`, it was erroring because it couldn't find the rank the en-passant pawn was on. Either way, that's solved now.

Another error I found was doing `1UL << someSquare.Bitboard`, which, under the hood, is literally `1UL << (1UL << someSquare)`, creating some sort of super bitboard. That's all fixed now, and I don't have to deal with that anymore.

### Structs

Another bunch of errors came when making the board into a struct. Turns out all structs spawn with an empty parameterless constructor, so instead of making a board off a default value, it was literally just making a `null` board value, raising a ton of errors when trying to operate on that board. This is fixed now. :thumbsup:

All of these fixes will be present in version `v1.1.6` of the NuGet package.

## Statistics

Quick section about the statistics of this project:

### Source Code

|File name|Lines|Characters|File size|
|:-|:-:|:-:|:-:|
|`Board.cs`|1,189|46,245|46.32 KB|       
|`Board960.cs`|1,076|42,772|42.82 KB|    
|`Bitmasks.cs`|86|2,679|2.7 KB|
|`Bitboard.cs`|119|3,820|3.85 KB|        
|`Castling.cs`|210|7,900|7.92 KB|        
|`Generator.cs`|562|19,424|19.52 KB|     
|`MagicBitboards.cs`|318|17,169|17.08 KB|
|`Perft.cs`|364|11,865|11.94 KB|
|`Square.cs`|132|5,244|5.25 KB|
|`Tables.cs`|59|1,851|1.86 KB|
|`Utilities.cs`|222|7,160|7.21 KB|       
|`Zobrist.cs`|83|2,153|2.18 KB|

### Note about Chess960

Chess960 is the exact same, just change the `using` directory to be `Chess960` and use the `Board960` class instead of the `Board` class. The methods for controlling the state of the board are identical.

## Move Generation Order

1. Rooks
    - Using magic bitboards
    - Includes the rook part of queen moves

2. Bishops
    - Using magic bitboards
    - includes the diagonal part of queen moves

3. Knights

4. Pawns
    - Single pushes (with and without promotion)
    - Double pushes
    - Left attacks (with and without promotion)
    - Right attacks (with and without promotion)
    - En-passant (thanks to [`@Disservin`](https://github.com/Disservin) for [`Smallbrain`](https://github.com/Disservin/Smallbrain) and its EP code)

5. Normal King Moves
    - Evading checks
    - Not stepping on squares attacked by the opponent
    - Not capturing pieces protected by the opponent

6. Castling
    - Not supported if in check
    - Not supported if pieces are blocking the path
    - Not supported if pieces are attacking the path
    - Not supported if already castled

All of how I programmed my move generation can be found in the conveniently named folder in this repository called [`Move Generation`](https://github.com/axololly/chess/tree/main/Move%20Generation).

At some point, I'll write up a bunch of documentation, well, documenting everything about this project in the [`Documentation`](https://github.com/axololly/chess/tree/main/Documentation) folder, which, as of writing this, is about two and a half, maybe three weeks behind schedule. Over the next few days, I'll update it with everything it's missing.

## Perft Data

I translated [`TheRealGioviok's`](https://github.com/TheRealGioviok) raw text file of [example FEN positions](https://github.com/TheRealGioviok/Perseus-Engine/blob/main/perft.txt) and their associated depths, and converted it into a JSON file 74,272 lines long. Substantially longer, but easier to manage.

Each document is structured like this:
```json
{
    "FEN": ...,
    "Depths": {
        "1": ...,
        "2": ...,
        "3": ...,
        "4": ...,
        "5": ...,
        "6": ...
    }
}
```

And in total, there are around 6,600 of these that you can use to verify that your engine returns the correct move counts.

If you want to perft test my own project, you can do it with the `Perft` and `Perft960` classes, which have the available features:
|Type|Method|Explanation|
|:-|:-|:-|
|Basic|`.BasePerftTest()`|Returns the number of counted nodes at the end of the function.|
|Move-by-Move|`.MoveByMovePerftTest()`|Prints the move and number of counted nodes, returning the total at the end.|
|Global|`.GlobalPerftTest()`|Prints the number of counted nodes until a given depth is reached.|
|Output|`.OutputPerftTest()`|Redirects the console output into a file for further review.|

All of these work with Chess960 boards - you just need to use `Perft960` instead of `Perft`.

There's also a `.Kiwipete` property that holds the FEN string for the Kiwipete versions of Chess and Chess960.

## Thanks

Honestly, thank you to everyone in the [Stockfish Discord server](https://discord.gg/stockfish), from [Analog Hors](https://github.com/analog-hors) and her [magic bitboards article](https://analog-hors.github.io/site/magic-bitboards/), all the way to Disservin, who I already mentioned above for his code relating to en-passant moves.

It means a lot to me, knowing I've finally completed a project that frustrated me back when I started in June. I learnt a new language, made new friends, and, most importantly, had a lot of fun with this.

So proud of how far I've come. Time to celebrate! :tada: