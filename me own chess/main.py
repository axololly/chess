from board import Board
from pieces import Pawn, Rook, Knight, Bishop, Queen, King, Piece

if __name__ == "__main__":
    init_pieces = [Pawn(0, (1, 0)), Pawn(0, (1, 1)), Pawn(0, (1, 2)), Pawn(0, (1, 3)), Pawn(0, (1, 4)), Pawn(0, (1, 5)), Pawn(0, (1, 6)), Pawn(0, (1, 7)), Rook(0, (0, 0)), Rook(0, (0, 7)), Knight(0, (0, 1)), Knight(0, (0, 6)), Bishop(0, (0, 2)), Bishop(0, (0, 5)), Queen(0, (0, 3)), King(0, (0, 4)), Pawn(1, (6, 0)), Pawn(1, (6, 1)), Pawn(1, (6, 2)), Pawn(1, (6, 3)), Pawn(1, (6, 4)), Pawn(1, (6, 5)), Pawn(1, (6, 6)), Pawn(1, (6, 7)), Rook(1, (7, 0)), Rook(1, (7, 7)), Knight(1, (7, 1)), Knight(1, (7, 6)), Bishop(1, (7, 2)), Bishop(1, (7, 5)), Queen(1, (7, 3)), King(1, (7, 4))]
    test_pieces = [Rook(0, (2, 4)), Bishop(0, (5, 4))]
    
    board = Board()
    board.set_pieces(init_pieces)


    for piece in init_pieces:
        moves = piece.possible_moves(board.board)
        for move in moves:
            board.board[move[0]][move[1]] = "1"
    board.print_board()

    piece: Pawn = init_pieces[3]
    print(piece)
    pos_moves = piece.possible_moves(board.board)
    print(piece.possible_moves(board.board))
    piece.move(pos_moves[0])
    print(piece.position)
    board.board[piece.position[0]][piece.position[1]] = str(piece)
    
