# ignore a bunch of shit in here -- it's for testing purposes

import pygame
from enum import IntEnum, Enum
from typing import List

class Piece(IntEnum):
    Empty = 0
    King = 1
    Pawn = 2
    Knight = 3
    Bishop = 4
    Rook = 5
    Queen = 6

    White = 8
    Black = 16

Images = {
    Piece.Empty : None,

    Piece.White | Piece.King : pygame.image.load('./assets/white king.png'),
    Piece.White | Piece.Knight : pygame.image.load('./assets/white knight.png'),
    Piece.White | Piece.Queen : pygame.image.load('./assets/white queen.png'),
    Piece.White | Piece.Rook : pygame.image.load('./assets/white rook.png'),
    Piece.White | Piece.Bishop : pygame.image.load('./assets/white bishop.png'),
    Piece.White | Piece.Pawn : pygame.image.load('./assets/white pawn.png'),

    Piece.Black | Piece.King : pygame.image.load('./assets/black king.png'),
    Piece.Black | Piece.Knight : pygame.image.load('./assets/black knight.png'),
    Piece.Black | Piece.Queen : pygame.image.load('./assets/black queen.png'),
    Piece.Black | Piece.Rook : pygame.image.load('./assets/black rook.png'),
    Piece.Black | Piece.Bishop : pygame.image.load('./assets/black bishop.png'),
    Piece.Black | Piece.Pawn : pygame.image.load('./assets/black pawn.png'),
}

for key, image in Images.items():
    if not image: continue    
    image = pygame.transform.scale(image, (60, 60))
    Images[key] = image

class ChessBoard:
    def __init__(self):
        self.board: List[int] = [Piece.Empty for _ in range(64)]
        
        self.board[0] = Piece.White | Piece.Bishop
        self.board[63] = Piece.Black | Piece.Queen
        self.board[7] = Piece.White | Piece.Knight
    
    def __getitem__(self, item):
        return self.board[item]