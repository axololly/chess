# Move Generation

The first and most important step of the game of chess is moving pieces. How else would you be able to even play a game of chess if not for being able to move pieces?

I'm sure you know about chess (why else would you be reading this?) and the rules on the board. Rules like:

- do not lob the board with its pieces at your opponent (at any time).
- pawns may not be flicked across the board at your opponent's eyes.
- taking the opponent's queen off the board and blowing a rasperry at them is not permitted.

Apologies. Wrong rule list. Just found the right one now:

- you cannot castle into check.
- bishops cannot move orthogonally.
- pawns cannot promote to a king.

## So how do we code movement?

Well, consider all the six different types of pieces in the game of Chess:

- pawns
- knights
- bishops
- rooks
- queens
- kings

We can categorise them into two types: static and sliding. Static pieces have fixed reach limits that will always remain the same, no matter the position. Pawns cannot move backwards, kings cannot move off of the board - you get the idea. However, sliding pieces can move in any number of squares along the board, and but their moves are halted if "blockers" are in the way. In short, static pieces have moves that are _not_ determined by the state of the board, whereas sliding pieces have moves that _are_ affected by the current state of the board.