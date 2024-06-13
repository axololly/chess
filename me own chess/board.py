class Board:
    def __init__(self):
        self.clear_board()

    def __repr__(self):
        return self.board
    
    def clear_board(self):
        self.board = [[None for _ in range(8)] for _ in range(8)]

    def set_pieces(self, pieces):
        self.clear_board()
        for piece in pieces:
            self.board[piece.position[1]][piece.position[0]] = piece

    def update_piece(self, old_position, new_position):
        self.board[old_position[1]][old_position[0]], self.board[new_position[1]][new_position[0]] = None, self.board[old_position[1]][old_position[0]]

    def print_board(self):
        print("   A B C D E F G H")
        for i, row in enumerate(reversed(self.board)):
            row_string = " ".join(str(element) if element is not None else "." for element in row)
            print(f"{8 - i}  {row_string}")

    def raw_print(self):
        for row in self.board:
            print(row)


if __name__ == "__main__":
    board = Board()
    board.print_board()