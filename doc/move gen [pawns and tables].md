# Move Generation [Pawns and Tables]

Pawns are quite special pieces. For a start, their attacks are not aligned with their movements, they can promote to a range of pieces at the backrank and they can do a stupid French move that's really cool to watch.

## Analysis
```yml
# Example of the moves and attacks
# for the pawn on D2
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

**Note: For this section, I'll be focusing on white pawns. Invert the operations for black pawns.**

## Left and Right Attacks

The offsets for all the `2` squares are 7 and 9, so we can go up (shift left) by 7 and 9 for white pawns, and go down (shift right) 7 and 9 for black pawns.

```py
leftAttacks  = occupancy & (whitePawns << 7)
rightAttacks = occupancy & (whitePawns << 9)
```

### Problems

Simple, right? Yes. _However,_ we forgot to account for pawns on the A and H files, shown here.
```yml
# pawn on A file    # pawn on H file
. . . . . . . .     . . . . . . . .
. . . . . . . .     . . . . . . . .
. . . . . . . .     . . . . . . . .
. . . . . . . .     . . . . . . . .
. . . . . . . .     . . . . . . . .
3 . . . . . . .     . . . . . . . 3
1 2 . . . . . .     . . . . . . 2 1
P . . . . . . .     . . . . . . . P
. . . . . . . .     . . . . . . . .
```

If we shift the pawns by 9 for the pawn on the A file (7 for on H file), we end up with attacks like this:
```yml
. . . . . . . .     . . . . . . . .
. . . . . . . .     . . . . . . . .
. . . . . . . .     . . . . . . . .
. . . . . . . .     . . . . . . . .
. . . . . . . .     1 . . . . . . .
. 1 . . . . . .     . . . . . . 1 .
P . . . . . . 1     . . . . . . . P
. . . . . . . .     . . . . . . . .
```

This is of course, not legal. You can't attack a piece on the other side of the board. So we need to fix our logic.

### Solution

Well, if we get left attacks of all the pawns, the only one we encounter problems with is the one on the A file. (You can verify this yourself in the previous diagram.) So, since that's where our problems are, let's just remove it.

We can exclude all bits on the A file by getting all the bits _not_ on the A file then bitwise ANDing that against the bitmask of the pawns. 

Like this:
```yml
# example           # ~fileA            # nothing on A file
. . . . . . . .     . 1 1 1 1 1 1 1     . . . . . . . .
. . . . . . . .     . 1 1 1 1 1 1 1     . . . . . . . .
. . . . . . . .     . 1 1 1 1 1 1 1     . . . . . . . .
. . . . . . . .  &  . 1 1 1 1 1 1 1  =  . . . . . . . .
1 . . . . . . .     . 1 1 1 1 1 1 1     . . . . . . . .
. . 1 . . . . 1     . 1 1 1 1 1 1 1     . . 1 . . . . 1
. 1 . . . 1 1 .     . 1 1 1 1 1 1 1     . 1 . . . 1 1 .
. . . . . . . .     . 1 1 1 1 1 1 1     . . . . . . . .
```

Now we can safely get left attacks without needing to compensate for errors from the A file pawns.

### Replication

We can do the _exact_ same thing for the pawns on the H file: when we want to get right attacks, we ignore the pawns on the H file using `~fileH` and bitwise ANDing it against the pawn bitmask to get a safe bitmask to check for right attacks on.

### Combining everything

```py
leftAttacks  = occupancy & ((pawns & ~fileA) << 7)
rightAttacks = occupancy & ((pawns & ~fileH) << 9)
```

## Single and Double Pushes

Pawns have two other moves we need to account for: moving forward once and moving forward twice, but _only_ on their first move. Keeping track of whether all 8 pawns have moved or not is a bit pointless since we can just check if they're on their start position. Pawns can't move backwards, so once they get moved, they cannot move two spaces again.

### Single Pushes

Let's start with single pushes. Pawns move 1 square in front, so we can just shift left by 8. However, if there's a piece in front of the pawn, it _cannot_ move forward.

Take this position for example:

![Pawn Pushes Demonstration](./assets/pawn%20pushes.png)

As you can see, the pawn on D3 is obstructed by the knight on D4 so it can't move forward. However, the pawn on E4 is _not_ obstructed by anything, so it can move forward.

We can take a board mask of the occupancy on the board, which would look like this:
```yml
. . . . . . . 1
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . 1 1 . . .
. . . 1 . . . .
. . . . . . . .
. . . . . . . 1
```

And then we can take a board mask of all the pawns on the board, which would look like this:
```yml
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . 1 . . .
. . . 1 . . . .
. . . . . . . .
. . . . . . . .
```

We can shift this bitboard up by 8 to get this:
```yml
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . 1 . . .
. . . 1 . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
```

Then do a bitwise NOT on the board mask to get only empty spaces to get this:
```yml
1 1 1 1 1 1 1 .
1 1 1 1 1 1 1 1
1 1 1 1 1 1 1 1
1 1 1 1 1 1 1 1
1 1 1 . . 1 1 1
1 1 1 . 1 1 1 1
1 1 1 1 1 1 1 1
1 1 1 1 1 1 1 .
```

And do a bitwise AND against the empty spaces to get only valid pawn pushes.
```yml
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . 1 . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
. . . . . . . .
```

And that's it. That's how we can do single pushes.

In code, that looks like this:
```py
singlePushes = ~occupancy & (pawns << 8)
```

### Double Pushes

For double pushes, we _could_ do this:
```py
doublePushes = ~occupancy & (pawns << 16)
```

However, if there's a piece in the way on the first square, this would still show that you can do a double push, yet the piece is clearly blocking, so we have to revise our answer.

### Using Single Pushes

After we push once, all the pawns on the second rank end up where? The third rank. So we can just check for the pawns on the third rank, then do the _exact_ same thing but shifted up 8 higher.

So we do a single push, twice. Sounds stupid, but it works.

With single pushes that look like this:
```py
singlePushes = ~occupancy & (pawns << 8)
```

We can structure our double push checks like this:
```py
doublePushes = ~occupancy & ((singlePushes & rank3) << 8)
```

The only difference is swapping the `pawns` variable with `singlePushes & rank3` which gets the single pushes on the third rank with a bitwise AND, then we check if there is a piece blocking us from going to the second square.

And that's it. That's everything.

### Complete code sample

```py
leftAttacks  = occupancy & ((pawns & ~fileA) << 7)
rightAttacks = occupancy & ((pawns & ~fileH) << 9)

singlePushes = ~occupancy & (pawns << 8)
doublePushes = ~occupancy & ((singlePushes & rank3) << 8)
```