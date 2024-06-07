class Piece:
    def __init__(self, color, position):
        self.color = color # -> 0 = white and 1 = black
        self.position = position

    def move(self, new_position):
        self.position = new_position

    @staticmethod
    def possible_moves(func):
        def wrapper(*args, **kwargs):
            moves = func(*args, **kwargs)
            valid_moves = [(x, y) for x, y in moves if 0 <= x < 8 and 0 <= y < 8]
            return valid_moves
        return wrapper


class Pawn(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)
    
    def __str__(self):
        if self.color:
            return "♟︎"
        else:
            return "♙"

    @Piece.possible_moves
    def move_(self):
        x, y = self.position
        return [(x, y + 1)] if self.color == 0 else [(x, y - 1)]

class Rook(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)

    def __str__(self):
        if self.color:
            return "♜"
        else:
            return "♖"
    
    @Piece.possible_moves
    def move_(self):
        x, y = self.position
        return [(x, y + i) for i in range(1, 8)] + [(x, y - i) for i in range(1, 8)] + [(x + i, y) for i in range(1, 8)] + [(x - i, y) for i in range(1, 8)]

class Knight(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)

    def __str__(self):
        if self.color:
            return "♞"
        else:
            return "♘"
    
    @Piece.possible_moves
    def move_(self):
        x, y = self.position
        return [(x + 2, y + 1), (x + 2, y - 1), (x - 2, y + 1), (x - 2, y - 1), (x + 1, y + 2), (x - 1, y + 2), (x + 1, y - 2), (x - 1, y - 2)]

class Bishop(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)

    def __str__(self):
        if self.color:
            return "♝"
        else:
            return "♗"
    
    @Piece.possible_moves
    def move_(self):
        x, y = self.position
        return [(x + i, y + i) for i in range(1, 8)] + [(x + i, y - i) for i in range(1, 8)] + [(x - i, y + i) for i in range(1, 8)] + [(x - i, y - i) for i in range(1, 8)]

class Queen(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)

    def __str__(self):
        if self.color:
            return "♛"
        else:
            return "♕"
    
    @Piece.possible_moves
    def move_(self):
        x, y = self.position
        return [(x, y + i) for i in range(1, 8)] + [(x, y - i) for i in range(1, 8)] + [(x + i, y) for i in range(1, 8)] + [(x - i, y) for i in range(1, 8)] + [(x + i, y + i) for i in range(1, 8)] + [(x + i, y - i) for i in range(1, 8)] + [(x - i, y + i) for i in range(1, 8)] + [(x - i, y - i) for i in range(1, 8)]

class King(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)

    def __str__(self):
        if self.color:
            return "♚"
        else:
            return "♔"
    
    @Piece.possible_moves
    def move_(self):
        x, y = self.position
        return [(x, y + 1), (x, y - 1), (x + 1, y), (x - 1, y), (x + 1, y + 1), (x + 1, y - 1), (x - 1, y + 1), (x - 1, y - 1)]


if __name__ == "__main__":
    pawn = Pawn(0, (1, 7))
    print(pawn.move_())