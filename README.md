# Chess Move Generator

If you can't beat em, join em. And that's what I did when I stayed inside for the last month every evening developing this chess move generator.

_If I ever hate myself enough to make a Python version, I'll link it here._

## Using my project

Quick heads up: I used `.NET 8.0` for this.

I have no idea where or how you publish a C# package, so I'll just list how you use the code here, if you ever build the code yourself.

### Example Code

```cs
using Chess;

class Program
{
    static void Main()
    {
        Board board = new();

        var nextMoves = board.GenerateLegalMoves();

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

## Thanks

Honestly, thank you to everyone in the [Stockfish Discord server](https://discord.gg/stockfish), from [Analog Hors](https://github.com/analog-hors) and her [magic bitboards article](https://analog-hors.github.io/site/magic-bitboards/), all the way to Disservin, who I already mentioned above for his code relating to en-passant moves.

It means a lot to me, knowing I've finally completed a project that frustrated me back when I started in June. I learnt a new language, made new friends, and, most importantly, had a lot of fun with this.

So proud of how far I've come. Time to celebrate! :tada: