from constants import WHITE, BLACK


class Piece:
    def __init__(self, color, position):
        self.color = color  # 1 = white and 0 = black
        self.position = position

    def move(self, new_position):
        self.position = new_position


class Pawn(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)
        self.first_move = True
    
    def __str__(self):
        return "♟︎" if self.color else "♙"
    
    def __repr__(self):
        return "♟︎" if self.color else "♙"
    
    def possible_moves(self, board):
        x, y = self.position
        moves = []
        
        direction = 1 if self.color == WHITE else -1
        
        # Normal move
        if board[y + direction][x] is None:
            moves.append((x, y + direction))
            # First move double step
            if self.first_move and board[y + 2 * direction][x] is None:
                moves.append((x, y + 2 * direction))
        
        # Captures
        for dx, dy in [(1, direction), (-1, direction)]:
            nx, ny = x + dx, y + dy
            if 0 <= nx < 8 and 0 <= ny < 8 and isinstance(board[ny][nx], Piece) and board[ny][nx].color != self.color:
                moves.append((nx, ny))
        
        return moves


class Rook(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)

    def __str__(self):
        return "♜" if self.color else "♖"
    
    def __repr__(self):
        return "♜" if self.color else "♖"
    
    def possible_moves(self, board):
        x, y = self.position
        moves = []
        
        for dx, dy in [(1, 0), (-1, 0), (0, 1), (0, -1)]:
            nx, ny = x + dx, y + dy
            while 0 <= nx < 8 and 0 <= ny < 8:
                if board[ny][nx] is not None:
                    if isinstance(board[ny][nx], Piece) and board[ny][nx].color != self.color:
                        moves.append((nx, ny))
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
    
    def __repr__(self):
        return "♞" if self.color else "♘"
    
    def possible_moves(self, board):
        x, y = self.position
        moves = []
        
        for dx, dy in [(-2, -1), (-2, 1), (-1, -2), (-1, 2), (1, -2), (1, 2), (2, -1), (2, 1)]:
            nx, ny = x + dx, y + dy
            if 0 <= nx < 8 and 0 <= ny < 8 and (board[ny][nx] is None or (isinstance(board[ny][nx], Piece) and board[ny][nx].color != self.color)):
                moves.append((nx, ny))
        
        return moves


class Bishop(Piece):
    def __init__(self, color, position):
        super().__init__(color, position)

    def __str__(self):
        return "♝" if self.color else "♗"

    def __repr__(self):
        return "♝" if self.color else "♗"
    
    def possible_moves(self, board):
        x, y = self.position
        moves = []
        
        for dx, dy in [(1, 1), (-1, -1), (1, -1), (-1, 1)]:
            nx, ny = x + dx, y + dy
            while 0 <= nx < 8 and 0 <= ny < 8:
                if board[ny][nx] is not None:
                    if isinstance(board[ny][nx], Piece) and board[ny][nx].color != self.color:
                        moves.append((nx, ny))
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
    
    def __repr__(self):
        return "♛" if self.color else "♕"
    
    def possible_moves(self, board):
        x, y = self.position
        moves = []
        
        for dx, dy in [(1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (-1, -1), (1, -1), (-1, 1)]:
            nx, ny = x + dx, y + dy
            while 0 <= nx < 8 and 0 <= ny < 8:
                if board[ny][nx] is not None:
                    if isinstance(board[ny][nx], Piece) and board[ny][nx].color != self.color:
                        moves.append((nx, ny))
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
    
    def __repr__(self):
        return "♚" if self.color else "♔"
    
    def possible_moves(self, board):
        x, y = self.position
        moves = []
        
        for dx, dy in [(-1, 0), (1, 0), (0, -1), (0, 1), (-1, -1), (1, 1), (-1, 1), (1, -1)]:
            nx, ny = x + dx, y + dy
            if 0 <= nx < 8 and 0 <= ny < 8 and (board[ny][nx] is None or (isinstance(board[ny][nx], Piece) and board[ny][nx].color != self.color)):
                moves.append((nx, ny))
        
        return moves