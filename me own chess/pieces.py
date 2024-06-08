class Piece:
    def __init__(self, color, position):
        self.color = color  # 0 = white and 1 = black
        self.position = position

    def move(self, new_position):
        self.position = new_position


class Pawn(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)
    
    def __str__(self):
        return "♟︎" if self.color else "♙"
    
    def possible_moves(self, board):
        x, y = self.position
        moves = []
        
        for dx, dy in [(1, 0)] if self.color == 0 else [(-1, 0)]:
            nx, ny = x + dx, y + dy
            if 0 <= nx < 8 and 0 <= ny < 8 and board[nx][ny] in [".", "1"]:
                moves.append((nx, ny))
        
        return moves


class Rook(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)

    def __str__(self):
        return "♜" if self.color else "♖"
    
    def possible_moves(self, board):
        x, y = self.position
        moves = []
        
        for dx, dy in [(1, 0), (-1, 0), (0, 1), (0, -1)]:
            nx, ny = x + dx, y + dy
            while 0 <= nx < 8 and 0 <= ny < 8:
                if board[nx][ny] not in [".", "1"]:
                    break
                moves.append((nx, ny))
                nx += dx
                ny += dy
        
        return moves


class Knight(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)

    def __str__(self):
        return "♞" if self.color else "♘"
    
    def possible_moves(self, board):
        x, y = self.position
        moves = []
        
        for dx, dy in [(-1, 2), (-1, -2), (1, 2), (1, -2), (-2, 1), (2, 1), (-2, -1), (2, -1)]:
            nx, ny = x + dx, y + dy
            if 0 <= nx < 8 and 0 <= ny < 8 and board[nx][ny] in [".", "1"]:
                moves.append((nx, ny))
        
        return moves


class Bishop(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)

    def __str__(self):
        return "♝" if self.color else "♗"

    def possible_moves(self, board):
        x, y = self.position
        moves = []
        
        for dx, dy in [(1, 1), (-1, -1), (1, -1), (-1, 1)]:
            nx, ny = x + dx, y + dy
            while 0 <= nx < 8 and 0 <= ny < 8:
                if board[nx][ny] not in [".", "1"]:
                    break
                moves.append((nx, ny))
                nx += dx
                ny += dy
        
        return moves


class Queen(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)

    def __str__(self):
        return "♛" if self.color else "♕"
    
    def possible_moves(self, board):
        x, y = self.position
        moves = []
        
        for dx, dy in [(1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (-1, -1), (1, -1), (-1, 1)]:
            nx, ny = x + dx, y + dy
            while 0 <= nx < 8 and 0 <= ny < 8:
                if board[nx][ny] not in [".", "1"]:
                    break
                moves.append((nx, ny))
                nx += dx
                ny += dy
        
        return moves


class King(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)

    def __str__(self):
        return "♚" if self.color else "♔"
    
    def possible_moves(self, board):
        x, y = self.position
        moves = []
        
        for dx, dy in [(x - 1, y), (x + 1, y), (x, y + 1), (x, y - 1), (x - 1, y + 1), (x - 1, y - 1), (x + 1, y + 1), (x + 1, y - 1)]:
            nx, ny = x + dx, y + dy
            while 0 <= nx < 8 and 0 <= ny < 8:
                if board[nx][ny] not in [".", "1"]:
                    break
                moves.append((nx, ny))
                nx += dx
                ny += dy
        
        return moves



if __name__ == "__main__":
    pawn = Pawn(0, (6, 0))
    print(pawn.possible_moves())
