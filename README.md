# Chess Move Generator v1.2.3

Documentation is slowly progressing. You can check it out [here](https://axololly.github.io/chess-documentation), and make sure to leave positive feedback. :heart:

## Statistics

Here are the current file statistics for this project:

|File name|Lines|Characters|File size|
|:-|:-:|:-:|:-:|
|`Board.cs`|1,230|47,912|47.99 KB|
|`Board960.cs`|1,099|43,664|43.71 KB|
|`Bitmasks.cs`|86|2,685|2.71 KB|
|`Bitboard.cs`|119|3,818|3.84 KB|
|`Castling.cs`|212|7,962|7.98 KB|
|`Generator.cs`|549|19,141|19.23 KB|
|`MagicBitboards.cs`|318|17,175|17.08 KB|
|`PGN.cs`|238|8164|8.2 KB|
|`Perft.cs`|364|11,865|11.94 KB|
|`Square.cs`|59|2,248|2.25 KB|
|`Tables.cs`|59|1,857|1.87 KB|
|`Utilities.cs`|222|7,166|7.21 KB|
|`Zobrist.cs`|80|2,113|2.14 KB|
|Total|4,678|179,788|180.13 KB|

I'm incredibly proud of how much I've written, how much I've learnt and how far I've come. It feels amazing to write this.

## PGN Support

I added PGN support now, which you can find in `PGN.cs`, and use under the namespace `Chess.PGN`. Now you can read games in PGN format. Do note that some PGNs are raising draw errors, mostly under the three-fold repetition rule. I became aware of this issue after running my code through a set of 7,000 PGNs I found [here](https://www.pgnmentor.com/files.html). For my experiments, I took games from [Viswanatan Anand](https://en.wikipedia.org/wiki/Viswanathan_Anand) and [Alireza Firouzja](https://en.wikipedia.org/wiki/Alireza_Firouzja), then ran them through a custom-made interpreter.

### Inputs and Outputs

Both inputting PGNs to a returned `Board` struct and outputting a PGN from a given `Board` struct are supported. Of course, outputting a PGN from a board can now be performed under the `.PGN` property.

To input a PGN:
```cs
using Chess;
using Chess.PGN;

string examplePGN;

Board outputtedBoard = PGN.InterpretPGN(examplePGN);
```

And to output a PGN:
```cs
using Chess;
using Chess.PGN;

Board board;

// Option 1 (cleaner)
string outputPGN = board.PGN;

// Option 2 (raw)
string outputPGN = PGN.ConvertToPGN(board);
```

It's as easy as that.

### Results

For Anand's game set, I got these results on an i5-4210U:
```yml
Found 4162 PGNs in 110 milliseconds.

Translated 4162 PGNs in 11032 milliseconds.
Successfully translated 4108 PGNs and failed translating 54 PGNs.
```

This results in 377 PGNs translated every second, or one PGN every 0.0027s.

All the fails were draw-related, which has already been explained above.

***

For Firouzja's game set, I got these results on the same CPU:
```yml
Found 3713 PGNs in 60 milliseconds.

Translated 3713 PGNs in 6589 milliseconds.
Successfully translated 3664 PGNs and failed translating 49 PGNs.
```

This results in 413 PGNs translated every second, or one PGN every 0.0024 seconds.

Fails are for the same reasons.

### Inner Workings

It works using regex to identify the start and end of a PGN's body - it starts with `1.` and ends with either `0-1`, `1-0`, or `1/2-1/2`. Note that this may also be present in a `[Result "..."]` tag, so we need to make sure that there isn't a leading quotation mark.

These can easily be compiled into regex with:
```cs
// For start
@"1\.";

// For end
@"[^""]((0-1)|(1-0)|(1/2-1/2))";
```

However, note that this does blindly extract moves, so if you gave it:
```yml
1. e4 e5 1. Nc6
```

It would interpret this as:
```yml
["e4", "e5", "Nc6"]
```

Then proceed to translate the moves to long algebraic notation, lookup them in the generated legal moves based on criteria extracted from the move, for example, like:

- an "N" means it's a knight move, and this can be either a `Piece.WhiteKnight` or `Piece.BlackKnight`, which we check the `.SideToMove` property for.

- if there's no piece character, it's a pawn move. For which side? We use the `.SideToMove` property for this again.

- if rank differentiation is provided, search for a move where the start square's rank originates on that specified rank, written as the "target rank"

## Usage

> :memo::wrench: **Note:** To use this, make sure you have `.NET 8.0`.

To start, include the `Chess` namespace. Then you can create a `Board` instance and go from there.

For extra information, here's a table of all the functions you'd be using, and what they do:

|Function|Explanation|
|:-:|:-|
|`.MakeMove()`|Makes a move on the board.<br>_Note that the legality of the move is not verified._<br><br>To get the `Move` struct, use the `Chess.MoveGen` namespace.|
|`.UndoMove()`|Undoes a move on the board.<br><br>This will raise an error if no moves were previously played on the board.|
|`.GenerateLegalMoves()`|Returns a list of `Move` structs representing all the legal moves that can be played on the board.<br><br>If the list is empty, the board state is either checkmate or stalemate, both of which can be found out with the `.InCheck` property, where if true, the state is checkmate, and if false, it's stalemate.|
|`.IsDraw`|Returns a boolean representing whether or not the current board state is a draw.<br><br>_Note that this generalises between the 50-move rule, three-fold repetition and insufficient material._|
|`.InCheck`|Returns a boolean representing whether or not the current player's king is in check or not.|

### Example
```cs
using Chess;

class Program
{
    static void Main()
    {
        Board board = new();

        Console.WriteLine($"Board:\n{board}\n");
        Console.WriteLine($"Next moves: [{string.Join(", ", board.GenerateLegalMoves())}]");
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

## Perft

I translated [this](https://github.com/TheRealGioviok/Perseus-Engine/blob/main/perft.txt) file into a more readable JSON structure where each document looks like this:
```json
{
    "FEN": "...",
    "Depths": {
        "1": "...",
        "2": "...",
        "3": "...",
        "4": "...",
        "5": "...",
        "6": "..."
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

## Thanks

Honestly, thank you to everyone in the [Stockfish Discord server](https://discord.gg/stockfish), from [Analog Hors](https://github.com/analog-hors) and her [magic bitboards article](https://analog-hors.github.io/site/magic-bitboards/), all the way to Disservin, who I already mentioned above for his code relating to en-passant moves.

It means a lot to me, knowing I've finally completed a project that frustrated me back when I started in June. I learnt a new language, made new friends, and, most importantly, had a lot of fun with this.

So proud of how far I've come. Time to celebrate! :tada: