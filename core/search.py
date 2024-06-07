from typing import List, Mapping
from basics import File, Rank, Square, Move, Piece, Binary, RegularBoard

class SearchMoves:
    "A class with various class methods to search for possible moves from a given square."

    # -----------------------------------------------------------------------------------------------
    
    rook_directions = {
        -1 : File.H,
        1 : File.A,
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

    The offsets for the bishop directions are demonstrated below:
    ```yaml
    -9  . -7
     .  X  .
     7  .  9
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
        directions: Mapping[int, int],
        player_mask: Binary,
        opponent_mask: Binary
    ) -> List[Move]:
        
        """
        Search a set of `directions` from a given `Square` on the board.

        Edge masks show where to end the search as this uses a `while` loop to record
        squares until a bit overlaps with the mask.

        Parameters:
            - start: `Square` - the square to start searching from.
            - directions: `Mapping[int, int]` - a dictionary to map offsets to edge masks.
        
        Returns:
            - `List[Move]` - the list of moves from this current square.
        """
        
        square = start.index
        
        moves = []
        
        for offset, edge_mask in directions.items():
            C = 1 << square # start at the square
            i = 0

            if offset > 0: # positive offset, going down
                while not C & edge_mask:
                    if (C >> offset) & player_mask:
                        break

                    i += 1

                    moves.append(
                        Move(
                            src = Square(square),
                            dst = Square(square - i * offset)
                        )
                    )

                    if (C >> offset) & opponent_mask: break
                    
                    C |= C >> offset
            
            else: # negative offset, going up
                while not C & edge_mask:
                    if (C << abs(offset)) & player_mask:
                        break
                    
                    i += 1

                    moves.append(
                        Move(
                            src = Square(square),
                            dst = Square(square - i * offset)
                        )
                    )

                    if (C << abs(offset)) & opponent_mask: break
                    
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
    def micro_search_directions(
        cls,
        start: Square,
        offsets: List[int]
    ) -> List[Move]:
        
        """
        Search one move in an offset instead of until collision with a mask.

        Intended for use with the king and knight.

        Paramters:
            - start: `Square` - the square to start searching around.
            - offsets: `List[int]` - the offsets to use when searching.

        Returns:
            - `List[Move]` - a list of moves that can be made.
        """
        
        square = start.index

        moves = []

        for offset in offsets:
            move = 1 << square + offset # get a new square

            # if the start square is on the A or B file
            # and the next square is on the G or H file
            # (meaning they moved across the board)
            # discredit the move
            if (1 << square) & (File.A | File.B) and move & (File.G | File.H):
                continue
            
            # if the start square is on the G or H file
            # and the next square is on the A or B file
            # (meaning they moved across the board)
            # discredit the move
            if (1 << square) & (File.G | File.H) and move & (File.A | File.B):
                continue

            moves.append(
                Move(
                    src = Square(square),
                    dst = Square(square + offset)
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
        
        return cls.micro_search_directions(
            start = start,
            offsets = [
                -7, -8,  9
                -1,      1,
                 7,  8,  9
            ]
        )
    
    @classmethod
    def find_knight_moves(cls, start: Square) -> List[Move]:
        """
        Find moves that a knight could go to, if on a given square.

        Parameters:
            - start: `Square` - the square to start searching moves from.
        
        Returns:
            - `List[Move]` - the list of moves from that square.
        

        ### Offsets

        ```yaml
          . -17 -16 -15   .  
        -10   .  -8   .  -6
         -2  -1   X   1   2  
          6   .   8   .  10
          .  15  16  17   .
        
          . -17   . -15   .  
        -10   .   .   .  -6
          .   .   X   .   .  
          6   .   .   .  10
          .  15   .  17   .
        ```
        """

        return cls.micro_search_directions(
            start = start,
            offsets = [
                -17, -15, -10, -6, # knight moves upwards
                 17,  15,  10,  6  # knight moves downwards
            ]
        )

if __name__ == '__main__':
    import test2