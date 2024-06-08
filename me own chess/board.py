class Board:
    def __init__(self):
        self.board = [["." for _ in range(8)] for _ in range(8)]


    def set_pieces(self, pieces):
        for piece in pieces:
            self.board[piece.position[0]][piece.position[1]] = str(piece)

    def print_board(self):
        print("   A B C D E F G H")
        for i, row in enumerate(self.board):
            row_string = " ".join(str(element) for element in row)
            print(f"{8 - i}  {row_string}")



if __name__ == "__main__":
    board = Board()
    board.print_board()