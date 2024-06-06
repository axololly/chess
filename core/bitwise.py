from enum import IntEnum

class Piece(IntEnum):
    Empty = 0
    Pawn = 1
    King = 2
    Bishop = 3
    Knight = 4
    Rook = 5
    Queen = 6

    White = 8
    Black = 16

Drawings = {
    # dot for empty spaces
    Piece.Empty : ' . ',
    
    # characters for all the white pieces
    Piece.White | Piece.Pawn : ' ♙ ',
    Piece.White | Piece.Bishop : ' ♗ ',
    Piece.White | Piece.King : ' ♔ ',
    Piece.White | Piece.Queen : ' ♕ ',
    Piece.White | Piece.Rook : ' ♖ ',
    Piece.White | Piece.Knight : ' ♘ ',

    # characters for all the black pieces
    Piece.Black | Piece.Pawn : ' ♟ ',
    Piece.Black | Piece.Bishop : ' ♝ ',
    Piece.Black | Piece.King : ' ♚ ',
    Piece.Black | Piece.Queen : ' ♛ ',
    Piece.Black | Piece.Rook : ' ♜ ',
    Piece.Black | Piece.Knight : ' ♞ '
}

class ChessBoard:
    def __init__(self):
        self.board = [[0 for _ in range(8)] for _ in range(8)]
        self.pieces = {

        }
    
    def __getitem__(self, item: int) -> int:
        x, y = divmod(item, 8)
        return self.board[x][y]

    def __setitem__(self, place: int, item: int) -> None:
        x, y = divmod(place, 8)
        self.board[x][y] = item
    
    def __repr__(self) -> str:
        board = ''

        for x, i in enumerate(self.board):
            state = ''

            for y, j in enumerate(i):
                if j != Piece.Empty:
                    state += str(Drawings[j])
                else:
                    d = f'{x * 8 + y} '
                    state += d if len(d) == 3 else f' {d}'
            
            board += state + '\n'
        
        return board.strip()
    
class File(IntEnum):
    A = 0b10000000_10000000_10000000_10000000_10000000_10000000_10000000_10000000
    B = 0b01000000_01000000_01000000_01000000_01000000_01000000_01000000_01000000
    C = 0b00100000_00100000_00100000_00100000_00100000_00100000_00100000_00100000
    D = 0b00010000_00010000_00010000_00010000_00010000_00010000_00010000_00010000
    E = 0b00001000_00001000_00001000_00001000_00001000_00001000_00001000_00001000
    F = 0b00000100_00000100_00000100_00000100_00000100_00000100_00000100_00000100
    G = 0b00000010_00000010_00000010_00000010_00000010_00000010_00000010_00000010
    H = 0b00000001_00000001_00000001_00000001_00000001_00000001_00000001_00000001

class Rank(IntEnum):
    n1 = 0b11111111_00000000_00000000_00000000_00000000_00000000_00000000_00000000
    n2 = 0b00000000_11111111_00000000_00000000_00000000_00000000_00000000_00000000
    n3 = 0b00000000_00000000_11111111_00000000_00000000_00000000_00000000_00000000
    n4 = 0b00000000_00000000_00000000_11111111_00000000_00000000_00000000_00000000
    n5 = 0b00000000_00000000_00000000_00000000_11111111_00000000_00000000_00000000
    n6 = 0b00000000_00000000_00000000_00000000_00000000_11111111_00000000_00000000
    n7 = 0b00000000_00000000_00000000_00000000_00000000_00000000_11111111_00000000
    n8 = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_11111111

class RegularBoard:
    def __init__(self, board: int):
        self.board = board

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


class SearchMoves:
    rook_directions = {
        -1 : File.A,
        1 : File.H,
        -8 : Rank.n1,
        8 : Rank.n8
    }

    bishop_directions = {
        9 : File.H | Rank.n8,
        -7 : File.H | Rank.n1,
        7 : File.A | Rank.n8,
        -9 : File.A | Rank.n1
    }

    @classmethod
    def search_directions(cls, square: int, directions: dict[int, int]) -> list[int]:
        moves = 1 << square
        
        for offset, mask in directions.items():
            C = 1 << square

            if offset > 0: # positive offset
                while not C & mask:
                    C |= C >> offset
            
            else: # negative offset, reverse operation
                while not C & mask:
                    C |= C << abs(offset)

            moves |= C
        
        moves ^= 1 << square
        
        return moves
    
    @classmethod
    def find_bishop_moves(cls, square: int) -> int:
        return cls.search_directions(
            square = square,
            directions = cls.bishop_directions
        )

    @classmethod
    def find_rook_moves(cls, square: int) -> int:
        return cls.search_directions(
            square = square,
            directions = cls.rook_directions
        )
    
    @classmethod
    def find_queen_moves(cls, square: int) -> int:
        return cls.search_directions(
            square = square,
            directions = cls.bishop_directions | cls.rook_directions
        )
    
    @classmethod
    def find_pawn_moves(cls, square: int) -> int:
        moves = 1 << square

        if 48 <= square < 56:
            moves |= 1 << square - 16
        
        moves |= 1 << square - 8
        
        moves ^= 1 << square # remove existing square

        return moves

    @classmethod
    def find_king_moves(cls, square: int) -> int:
        moves = 1 << square

        offsets = (cls.bishop_directions | cls.rook_directions).keys()
        # offsets = [abs(val) for val in (cls.bishop_directions | cls.rook_directions).keys()]

        for offset in offsets:
            move = 1 << square + offset

            if (1 << square) & File.A and move & File.H:
                continue
            
            if (1 << square) & File.H and move & File.A:
                continue

            moves |= move

        moves ^= 1 << square
        
        return moves
    
print(RegularBoard(SearchMoves.find_king_moves(63)))