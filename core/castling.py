from basics import RegularBoard, Square
from board import White, Black
from typehints import Binary
from enum import Enum

class Castle(Enum):
    Queenside = 0
    Kingside = 1

class Castling():
    def __init__(self):
        self.white = White
        self.black = Black
        
        self.castle_right = True
        self.castle_left = True
    
    def can_castle(self, direction: Castle):
        match direction:
            case Castle.Kingside:
                ...
            
            case Castle.Queenside:
                ...
    
    def __repr__(self) -> str:
        return str(RegularBoard(self.white.mask() | self.black.mask()))

C = Castling()

print(C)
# print(C.can_castle(Castle.Kingside))