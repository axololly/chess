from board import Board
from pieces import Pawn, Rook, Knight, Bishop, Queen, King, Piece


init_pieces = [Pawn(0, (1, 0)), Pawn(0, (1, 1)), Pawn(0, (1, 2)), Pawn(0, (1, 3)), Pawn(0, (1, 4)), Pawn(0, (1, 5)), Pawn(0, (1, 6)), Pawn(0, (1, 7)), Rook(0, (0, 0)), Rook(0, (0, 7)), Knight(0, (0, 1)), Knight(0, (0, 6)), Bishop(0, (0, 2)), Bishop(0, (0, 5)), Queen(0, (0, 3)), King(0, (0, 4)), Pawn(1, (6, 0)), Pawn(1, (6, 1)), Pawn(1, (6, 2)), Pawn(1, (6, 3)), Pawn(1, (6, 4)), Pawn(1, (6, 5)), Pawn(1, (6, 6)), Pawn(1, (6, 7)), Rook(1, (7, 0)), Rook(1, (7, 7)), Knight(1, (7, 1)), Knight(1, (7, 6)), Bishop(1, (7, 2)), Bishop(1, (7, 5)), Queen(1, (7, 3)), King(1, (7, 4))]


def show_board_rays(board, pieces):
    ray_board = board.board
    for piece in pieces:
        moves = piece.possible_moves(board.board)
        for move in moves:
            ray_board[move[0]][move[1]] = "1"
    print("   A B C D E F G H")
    for i, row in enumerate(ray_board):
        row_string = " ".join(str(element) for element in row)
        print(f"{8 - i}  {row_string}")




if __name__ == "__main__":
    board = Board()
    used_pieces = init_pieces
    board.set_pieces(used_pieces)

    used_pieces[3].move(used_pieces[3].possible_moves(board.board)[0])

    board.set_pieces(used_pieces)

    used_pieces[3].possible_moves(board.board)

    board.print_board()
    
