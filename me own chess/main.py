import copy

from board import Board
from pieces import Pawn, Rook, Knight, Bishop, Queen, King, Piece
from constants import *

def position_to_notation(position):
    return chr(97 + position[0]) + str(position[1] + 1)

def notation_to_position(notation):
    return (8 - int(notation[1]), ord(notation[0]) - 97)

def print_board_rays(board):
    board_copy = [row.copy() for row in board]
    for row in board_copy:
        for piece in row:
            if piece is not None and piece != "1":
                moves = piece.possible_moves(board_copy)
                for move in moves:
                    board_copy[move[0]][move[1]] = "1"
    print("   A B C D E F G H")
    for i, row in enumerate(board_copy):
        row_string = " ".join(str(element) for element in row)
        print(f"{8 - i}  {row_string}")
        ("\n")


def initialize_board():
    board = Board()
    
    white_pieces = [Pawn(WHITE, (i, 1)) for i in range(8)] + \
                   [Rook(WHITE, (0, 0)), Rook(WHITE, (7, 0)), 
                    Knight(WHITE, (1, 0)), Knight(WHITE, (6, 0)), 
                    Bishop(WHITE, (2, 0)), Bishop(WHITE, (5, 0)), 
                    Queen(WHITE, (3, 0)), King(WHITE, (4, 0))]
    
    black_pieces = [Pawn(BLACK, (i, 6)) for i in range(8)] + \
                   [Rook(BLACK, (0, 7)), Rook(BLACK, (7, 7)), 
                    Knight(BLACK, (1, 7)), Knight(BLACK, (6, 7)), 
                    Bishop(BLACK, (2, 7)), Bishop(BLACK, (5, 7)), 
                    Queen(BLACK, (3, 7)), King(BLACK, (4, 7))]
    
    all_pieces = white_pieces + black_pieces
    board.set_pieces(white_pieces)
    
    return board

    




if __name__ == "__main__":
    board = initialize_board()
    


