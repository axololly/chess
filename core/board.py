from __future__ import annotations
from basics import RegularBoard, Move
from typehints import Binary
from typing import List

class White:
    bishops: Binary = 0b00100100_00000000_00000000_00000000_00000000_00000000_00000000_00000000
    knights: Binary = 0b01000010_00000000_00000000_00000000_00000000_00000000_00000000_00000000
    rooks:   Binary = 0b10000001_00000000_00000000_00000000_00000000_00000000_00000000_00000000
    king:    Binary = 0b00001000_00000000_00000000_00000000_00000000_00000000_00000000_00000000
    queen:   Binary = 0b00010000_00000000_00000000_00000000_00000000_00000000_00000000_00000000
    pawns:   Binary = 0b00000000_11111111_00000000_00000000_00000000_00000000_00000000_00000000
    
    @classmethod
    def mask(cls) -> Binary:
        return cls.bishops | cls.knights | cls.rooks | cls.king | cls.queen | cls.pawns

class Black:
    bishops: Binary = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00100100
    knights: Binary = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_01000010
    rooks:   Binary = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_10000001
    king:    Binary = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00001000
    queen:   Binary = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00010000
    pawns:   Binary = 0b00000000_00000000_00000000_00000000_00000000_00000000_11111111_00000000
    
    @classmethod
    def mask(cls) -> Binary:
        return cls.bishops | cls.knights | cls.rooks | cls.king | cls.queen | cls.pawns
    
    @classmethod
    def next_moves(cls) -> List[Move]:
        pass

class Board:
    def __init__(self):
        self.white_side = White
        self.black_side = Black
        self.total_mask = White.mask() | Black.mask()

        self.moves = []
        self.counter = 0

if __name__ == '__main__':
    B = Board()
    print(RegularBoard(B.total_mask))