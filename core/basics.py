from enum import IntEnum
from dataclasses import dataclass
from typing import NewType

Binary = NewType('Binary', int)

class Piece(IntEnum):
    Empty = 0
    Pawn = 1
    King = 2
    Bishop = 3
    Knight = 4
    Rook = 5
    Queen = 6
    
class File(IntEnum):
    """
    Represents a file of the board in binary.

    These appear flipped in code for two reasons:

        1) For how the board representation is conducted.
        
        2) To make placing squares on the board easier.
            (eg. `34` is `34` instead of secretly being `37`)

    ### File.H
    ```yaml
    . . . . . . . 1
    . . . . . . . 1
    . . . . . . . 1
    . . . . . . . 1
    . . . . . . . 1
    . . . . . . . 1
    . . . . . . . 1
    . . . . . . . 1
    ```

    ### File.C
    ```yaml
    . . 1 . . . . .
    . . 1 . . . . .
    . . 1 . . . . .
    . . 1 . . . . .
    . . 1 . . . . .
    . . 1 . . . . .
    . . 1 . . . . .
    . . 1 . . . . .
    ```
    """
    A = 0b00000001_00000001_00000001_00000001_00000001_00000001_00000001_00000001
    B = 0b00000010_00000010_00000010_00000010_00000010_00000010_00000010_00000010
    C = 0b00000100_00000100_00000100_00000100_00000100_00000100_00000100_00000100
    D = 0b00001000_00001000_00001000_00001000_00001000_00001000_00001000_00001000
    E = 0b00010000_00010000_00010000_00010000_00010000_00010000_00010000_00010000
    F = 0b00100000_00100000_00100000_00100000_00100000_00100000_00100000_00100000
    G = 0b01000000_01000000_01000000_01000000_01000000_01000000_01000000_01000000
    H = 0b10000000_10000000_10000000_10000000_10000000_10000000_10000000_10000000

class Rank(IntEnum):
    """
    Represents a rank of the board in binary.

    ### Rank.n7 (7th rank)
    ```yaml
    . . . . . . . .
    1 1 1 1 1 1 1 1
    . . . . . . . .
    . . . . . . . .
    . . . . . . . .
    . . . . . . . .
    . . . . . . . .
    . . . . . . . .

    ### Rank.n3
    ```yaml
    . . . . . . . .
    . . . . . . . .
    . . . . . . . .
    . . . . . . . .
    . . . . . . . .
    1 1 1 1 1 1 1 1
    . . . . . . . .
    . . . . . . . .
    ```
    """
    
    n1 = 0b11111111_00000000_00000000_00000000_00000000_00000000_00000000_00000000
    n2 = 0b00000000_11111111_00000000_00000000_00000000_00000000_00000000_00000000
    n3 = 0b00000000_00000000_11111111_00000000_00000000_00000000_00000000_00000000
    n4 = 0b00000000_00000000_00000000_11111111_00000000_00000000_00000000_00000000
    n5 = 0b00000000_00000000_00000000_00000000_11111111_00000000_00000000_00000000
    n6 = 0b00000000_00000000_00000000_00000000_00000000_11111111_00000000_00000000
    n7 = 0b00000000_00000000_00000000_00000000_00000000_00000000_11111111_00000000
    n8 = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_11111111

@dataclass
class RegularBoard:
    """
    Used in testing purposes to display a number in binary as a bitboard.

    Parameters:
        - board: `Binary` - The number (interpreted as binary)
                         for the bitboard to be created with.
    """
    
    board: Binary

    def __repr__(self) -> str:
        board = []

        for i in range(8):
            line = ''

            for j in range(8):
                if self.board & (1 << 8 * i + j):
                    line += '1 '
                else:
                    line += '. '
            
            board += [line]

        return '\n'.join(board)
    
@dataclass
class Square:
    index: int

    def __repr__(self) -> str:
        letters = 'abcdefgh'
        
        rank, file = divmod(self.index, 8)

        return f"{letters[file]}{9 - rank}" # a8, b3, c6, etc

@dataclass
class Move:
    """
    Represents a move that can be made on the board.

    Parameters:
        - src: `Square` - the square that the move starts on.
        - dst: `Square` - the square that the move ends on.
        - promo: `Piece` - the piece to promote to. Defaults to `None`.
    """
    
    src: Square
    dst: Square
    promo: Piece = None

    def __str__(self) -> str:            
        return repr(self.src) + repr(self.dst)
    
    def __repr__(self) -> str:
        return repr(self.src) + repr(self.dst)
    
        # raise NotImplementedError("needs to include the piece on the starting square, but haven't found a way to do that yet.")
    
        pieces = {
            Piece.Knight: 'N',
            Piece.Queen: 'Q',
            Piece.Rook: 'R',
            Piece.Bishop: 'B'
        }

        move = str(self.src) + str(self.dst)

        if P := self.promo:
            move + f'={pieces[P]}'
        
        return move