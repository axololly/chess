from enum import IntEnum
from dataclasses import dataclass
from typing import List, Mapping

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
    
    n8 = 0b11111111_00000000_00000000_00000000_00000000_00000000_00000000_00000000
    n7 = 0b00000000_11111111_00000000_00000000_00000000_00000000_00000000_00000000
    n6 = 0b00000000_00000000_11111111_00000000_00000000_00000000_00000000_00000000
    n5 = 0b00000000_00000000_00000000_11111111_00000000_00000000_00000000_00000000
    n4 = 0b00000000_00000000_00000000_00000000_11111111_00000000_00000000_00000000
    n3 = 0b00000000_00000000_00000000_00000000_00000000_11111111_00000000_00000000
    n2 = 0b00000000_00000000_00000000_00000000_00000000_00000000_11111111_00000000
    n1 = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_11111111

@dataclass
class RegularBoard:
    """
    Used in testing purposes to display a number in binary as a bitboard.

    Parameters:
        - board: `int` - The number (interpreted as binary)
                         for the bitboard to be created with.
    """
    
    board: int

    def __repr__(self) -> str:
        board = []

        for i in range(8):
            line = ''

            for j in range(8):
                if self.board & (1 << 8 * i + j):
                    line += '1 '
                else:
                    line += f'{8 * i + j} ' # '. '
            
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
        raise NotImplementedError("needs to include the piece on the starting square, but haven't found a way to do that yet.")
    
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

class SearchMoves:
    "A class with various class methods to search for possible moves from a given square."

    # -----------------------------------------------------------------------------------------------
    
    rook_directions = {
        -1 : File.A,
        1 : File.H,
        -8 : Rank.n8,
        8 : Rank.n1
    }
    """
    A `Mapping` of offsets to masks that instruct the search loop when to stop.

    This is done by checking if the search bit overlaps with the mask and if it does, stop the search there.

    The offsets for the rook directions are demonstrated below:
    ```yaml
     . -8 .
     -1 X 1
      . 8 .
    ```

    And each mask is as follows:
    ```yaml
    # File.A            # File.H            # Rank.n8            # Rank.n1
    1 . . . . . . .     . . . . . . . 1     1 1 1 1 1 1 1 1     . . . . . . . .
    1 . . . . . . .     . . . . . . . 1     . . . . . . . .     . . . . . . . .
    1 . . . . . . .     . . . . . . . 1     . . . . . . . .     . . . . . . . .
    1 . . . . . . .     . . . . . . . 1     . . . . . . . .     . . . . . . . .
    1 . . . . . . .     . . . . . . . 1     . . . . . . . .     . . . . . . . .
    1 . . . . . . .     . . . . . . . 1     . . . . . . . .     . . . . . . . .
    1 . . . . . . .     . . . . . . . 1     . . . . . . . .     . . . . . . . .
    1 . . . . . . .     . . . . . . . 1     . . . . . . . .     1 1 1 1 1 1 1 1
    ```
    """

    # -----------------------------------------------------------------------------------------------

    bishop_directions = {
        -9 : File.H | Rank.n1,
        7 : File.H | Rank.n8,
        -7 : File.A | Rank.n1,
        9 : File.A | Rank.n8
    }
    """
    A `Mapping` of offsets to masks that instruct the search loop when to stop.

    This is done by checking if the search bit overlaps with the mask and if it does, stop the search there.

    The offsets for the rook directions are demonstrated below:
    ```yaml
     . -8 .
     -1 X 1
      . 8 .
    ```

    And each mask is as follows:
    ```yaml
    # File.H | Rank.n1      # File.H | Rank.n8      # File.A | Rank.n1      # File.A | Rank.n8
    1 1 1 1 1 1 1 1         . . . . . . . 1         1 1 1 1 1 1 1 1         1 . . . . . . .
    . . . . . . . 1         . . . . . . . 1         1 . . . . . . .         1 . . . . . . .
    . . . . . . . 1         . . . . . . . 1         1 . . . . . . .         1 . . . . . . .
    . . . . . . . 1         . . . . . . . 1         1 . . . . . . .         1 . . . . . . .
    . . . . . . . 1         . . . . . . . 1         1 . . . . . . .         1 . . . . . . .
    . . . . . . . 1         . . . . . . . 1         1 . . . . . . .         1 . . . . . . .
    . . . . . . . 1         . . . . . . . 1         1 . . . . . . .         1 . . . . . . .
    . . . . . . . 1         1 1 1 1 1 1 1 1         1 . . . . . . .         1 1 1 1 1 1 1 1
    ```
    """

    # -----------------------------------------------------------------------------------------------

    @classmethod
    def search_directions(
        cls,
        start: Square,
        directions: Mapping[int, int]
    ) -> List[Move]:
        
        """
        Search a set of `directions` from a given `Square` on the board.

        Masks show where to end the search as this uses a `while` loop to record
        squares until a bit overlaps with the mask.

        Parameters:
            - start: `Square` - the square to start searching from.
            - directions: `Mapping[int, int]` - a dictionary to map offsets to masks.
        
        Returns:
            - `List[Move]` - the list of moves from this current square.
        """
        
        square = start.index
        
        moves = []
        
        for offset, mask in directions.items():
            C = 1 << square
            i = 0

            if offset > 0: # positive offset
                while not C & mask:
                    i += 1

                    moves.append(
                        Move(
                            src = Square(square),
                            dst = Square(square + i * offset)
                        )
                    )
                    
                    C |= C >> offset
            
            else: # negative offset, reverse operation
                while not C & mask:
                    i += 1

                    moves.append(
                        Move(
                            src = Square(square),
                            dst = Square(square + i * offset)
                        )
                    )
                    
                    C |= C << abs(offset)
        
        return moves
    
    @classmethod
    def find_bishop_moves(cls, start: Square) -> List[Move]:
        """
        Get bishop moves (diagonally) from a given square.

        Parameters:
            - start: `Square` - the square to start getting moves from.
        
        Returns:
            - `List[Move]` - a list of moves from this current square.
        """

        return cls.search_directions(
            start = start,
            directions = cls.bishop_directions
        )

    @classmethod
    def find_rook_moves(cls, start: Square) -> List[Move]:
        """
        Get rook moves (horizontally and vertically) from a given square.

        Parameters:
            - start: `Square` - the square to start getting moves from.
        
        Returns:
            - `List[Move]` - a list of moves from this current square.
        """
        
        return cls.search_directions(
            start = start,
            directions = cls.rook_directions
        )
    
    @classmethod
    def find_queen_moves(cls, start: Square) -> List[Move]:
        """
        Get queen moves (horizontally, vertically and diagonally) from a given square.

        Parameters:
            - start: `Square` - the square to start getting moves from.
        
        Returns:
            - `List[Move]` - a list of moves from this current square.
        """
        
        return cls.search_directions(
            start = start,
            directions = cls.bishop_directions | cls.rook_directions # combine the two dictionaries
        )
    
    @classmethod
    def find_pawn_moves(cls, start: Square) -> List[Move]:
        """
        Get pawn moves (one or two squares in front) from a given square.

        The list of pawn moves does include promotable squares.

        Parameters:
            - start: `Square` - the square to start getting moves from.
        
        Returns:
            - `List[Move]` - a list of moves from this current square.
        """
        
        square = start.index
        moves = []

        if 48 <= square < 56: # if on the first rank
            moves.append(
                Move(
                    src = Square(square),
                    dst = Square(square - 16) # allow movement to two squares
                )
            )
        
        if (1 << square - 8) & Rank.n8: # if it gets to the backrank
            for piece in [
                # promotion pieces
                Piece.Bishop,
                Piece.Knight,
                Piece.Rook,
                Piece.Queen
            ]:
                moves.append(
                    Move(
                        src = Square(square),
                        dst = Square(square - 8),
                        promo = piece # add moves with promotion to another piece
                    )
                )
        else:
            moves.append(
                Move(
                    src = Square(square),
                    dst = Square(square - 8) # allow movement to square in front
                )
            )

        return moves

    @classmethod
    def find_king_moves(cls, start: Square) -> List[Move]:
        """
        Find moves that a king could go to, if on a given square.

        Parameters:
            - start: `Square` - the square to start searching moves from.
        
        Returns:
            - `List[Move]` - the list of moves from that square.
        """
        
        square = start.index

        moves = []

        # get the offsets from the bishop and rook because
        # the king moves in every direction like the queen
        # (one space only though) so no while loop needed
        offsets = (cls.bishop_directions | cls.rook_directions).keys()

        for offset in offsets:
            move = 1 << square + offset # get a new square

            # if the start square is on the A file
            # and the next square is on the H file
            # discredit the move
            if (1 << square) & File.A and move & File.H:
                continue
            
            # if the start square is on the H file
            # and the next square is on the A file
            # discredit the move
            if (1 << square) & File.H and move & File.A:
                continue

            moves.append(
                Move(
                    src = Square(square),
                    dst = Square(square + offset)
                )
            )
        
        return moves
    
    . # needs to implement knight moves