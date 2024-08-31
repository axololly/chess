using Utilities;
using MoveGeneration;
using System.Numerics;

namespace ChessBoard
{
    public class PieceSet(Colour colour)
    {
        public Colour colour = colour;

        public ulong Bishops = 0;
        public ulong Knights = 0;
        public ulong Rooks = 0;
        public ulong Pawns = 0;
        public ulong Queens = 0;
        public ulong King = 0;

        public ulong ALL() => Bishops | Knights | Rooks | Pawns | Queens | King;

        public static List<int> BitIndexes(ulong bitboard)
        {
            List<int> positions = [];

            while (bitboard != 0)
            {
                int index = BitOperations.TrailingZeroCount(bitboard);
                ulong LSB = 1UL << index;
                
                positions.Add(index);
                bitboard ^= LSB;
            }

            return positions;
        }

        public ulong AttackingBitmask(ulong occupancy)
        {
            ulong attacks = 0;

            foreach (int square in BitIndexes(Bishops))
            {
                attacks |= Moves.GetBishopMoveBitmask(occupancy, square);
            }

            foreach (int square in BitIndexes(Rooks))
            {
                attacks |= Moves.GetRookMoveBitmask(occupancy, square);
            }

            foreach (int square in BitIndexes(Queens))
            {
                attacks |= Moves.GetBishopMoveBitmask(occupancy, square);
                attacks |= Moves.GetRookMoveBitmask(occupancy, square);
            }

            foreach (int square in BitIndexes(Knights))
            {
                attacks |= Moves.fmt.KNIGHT_MOVES_TABLE[square];
            }

            int kingPosition = BitIndexes(King)[0];
            attacks |= Moves.fmt.KING_MOVES_TABLE[kingPosition]; // Get king moves from table

            ulong pawnLeftAttacks;
            ulong pawnRightAttacks;

            if (colour == Colour.White)
            {
                pawnLeftAttacks  = (Pawns & ~BoardFile.A) << 7;
                pawnRightAttacks = (Pawns & ~BoardFile.H) << 9;
            }
            else
            {
                pawnLeftAttacks  = (Pawns & ~BoardFile.A) >> 9;
                pawnRightAttacks = (Pawns & ~BoardFile.H) >> 7;
            }

            attacks |= pawnLeftAttacks;

            attacks |= pawnRightAttacks;

            attacks &= ~ALL();

            return attacks;
        }
    }

    public enum Piece
    {
        WhitePawn,
        BlackPawn,
        WhiteKnight,
        BlackKnight,
        WhiteBishop,
        BlackBishop,
        WhiteRook,
        BlackRook,
        WhiteQueen,
        BlackQueen,
        WhiteKing,
        BlackKing,
        Empty
    }

    public class Board
    {
        public Piece[] BoardArray;
        public PieceSet White;
        public PieceSet Black;
        public int moveCounter;
        public byte castlingRights; // 0b1111 => KQkq
        public List<Move> moveHistory;
        public ulong epSquare;

        public Board(string FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            // Get sections of FEN string
            string[] FENstringSections = FEN.Split();

            // Generate piece bitboard classes
            White = new(Colour.White);
            Black = new(Colour.Black);

            // Get the castling rights
            string FENcastlingRights = FENstringSections[2];
            
            // Set the castling rights
            foreach (char castlingRight in FENcastlingRights)
            {
                castlingRights |= castlingRight switch
                {
                    'K' => 0b1000,
                    'Q' => 0b0100,
                    'k' => 0b0010,
                    'q' => 0b0001,
                    _ => 0, // to cover '\0' and '-' character
                };
            }

            // Set the en-passant square
            string epSquareSection = FENstringSections[3];

            if (epSquareSection == "-")
            {
                epSquare = 0;
            }
            else
            {
                int file = "abcdefgh".IndexOf(epSquareSection[0]);
                int rank = "12345678".IndexOf(epSquareSection[1]);
                epSquare = 1UL << (rank * 8 + file);
            }

            // Display the board
            BoardArray = new Piece[64];
            Array.Fill(BoardArray, Piece.Empty);

            string boardFEN = FENstringSections[0];
            
            // Position of where to input a piece to
            // in BoardRepresentation
            int position = 0;

            // Array of characters. Index relates to the
            // enum value of each piece the char represents
            char[] pieces = [
                'P', 'p', 'N', 'n', 'B', 'b',
                'R', 'r', 'Q', 'q', 'K', 'k'
            ];

            foreach (char C in boardFEN)
            {
                // Skip useless character
                if (C == '/') continue;
                
                // Add empty spaces for each number
                if (int.TryParse(C.ToString(), out int emptySpaceCount))
                {
                    position += emptySpaceCount;
                    continue;
                }

                // Not a useless character and not an empty space indicator.
                // This is a piece to add to the board.
                int T = Array.IndexOf(pieces, C);
                Piece piece = (Piece)T;

                // Add to bitboards depending on piece.
                switch (piece)
                {
                    case Piece.WhitePawn:
                        White.Pawns |= 1UL << (position ^ 56);
                        break;
                    
                    case Piece.WhiteBishop:
                        White.Bishops |= 1UL << (position ^ 56);
                        break;

                    case Piece.WhiteKnight:
                        White.Knights |= 1UL << (position ^ 56);
                        break;
                    
                    case Piece.WhiteRook:
                        White.Rooks |= 1UL << (position ^ 56);
                        break;
                    
                    case Piece.WhiteQueen:
                        White.Queens |= 1UL << (position ^ 56);
                        break;
                    
                    case Piece.WhiteKing:
                        White.King |= 1UL << (position ^ 56);
                        break;

                    case Piece.BlackPawn:
                        Black.Pawns |= 1UL << (position ^ 56);
                        break;
                    
                    case Piece.BlackBishop:
                        Black.Bishops |= 1UL << (position ^ 56);
                        break;

                    case Piece.BlackKnight:
                        Black.Knights |= 1UL << (position ^ 56);
                        break;

                    case Piece.BlackRook:
                        Black.Rooks |= 1UL << (position ^ 56);
                        break;

                    case Piece.BlackQueen:
                        Black.Queens |= 1UL << (position ^ 56);
                        break;

                    case Piece.BlackKing:
                        Black.King |= 1UL << (position ^ 56);
                        break;
                };

                BoardArray[position++] = piece;
            }

            int sideToMove = FENstringSections[1] switch
            {
                "w" => 0, "b" => 1,
                _ => 0 // default case, never reached
            };

            int.TryParse(FENstringSections[5], out int onWhatMove);

            onWhatMove -= 1;
            moveCounter = sideToMove + onWhatMove;

            moveHistory = [];
        }


        public ulong GetBitboardFromEnum(Piece pieceEnum)
        {
            return pieceEnum switch
            {
                Piece.BlackBishop =>  Black.Bishops,
                Piece.BlackKnight =>  Black.Knights,
                Piece.BlackKing   =>  Black.King,
                Piece.BlackQueen  =>  Black.Queens,
                Piece.BlackRook   =>  Black.Rooks,
                Piece.BlackPawn   =>  Black.Pawns,

                Piece.WhiteBishop =>  White.Bishops,
                Piece.WhiteKnight =>  White.Knights,
                Piece.WhitePawn   =>  White.Pawns,
                Piece.WhiteQueen  =>  White.Queens,
                Piece.WhiteRook   =>  White.Rooks,
                Piece.WhiteKing   =>  White.King,

                _ => throw new Exception($"invalid enum type - cannot get bitboard for the enum \"{pieceEnum}\".")
            };
        }

        public void SetBitboardFromEnum(Piece pieceEnum, ulong bitboard)
        {
            var _ = pieceEnum switch
            {
                Piece.BlackBishop =>  Black.Bishops = bitboard,
                Piece.BlackKnight =>  Black.Knights = bitboard,
                Piece.BlackKing   =>  Black.King = bitboard,
                Piece.BlackQueen  =>  Black.Queens = bitboard,
                Piece.BlackRook   =>  Black.Rooks = bitboard,
                Piece.BlackPawn   =>  Black.Pawns = bitboard,

                Piece.WhiteBishop =>  White.Bishops = bitboard,
                Piece.WhiteKnight =>  White.Knights = bitboard,
                Piece.WhitePawn   =>  White.Pawns = bitboard,
                Piece.WhiteQueen  =>  White.Queens = bitboard,
                Piece.WhiteRook   =>  White.Rooks = bitboard,
                Piece.WhiteKing   =>  White.King = bitboard,

                _ => throw new Exception("invalid enum type - cannot set bitboard for this enum.")
            };
        }


        public void MakeMove(Move move)
        {
            // Get the piece being moved
            Piece pieceToMove = BoardArray[move.src ^ 56];

            if (pieceToMove == Piece.Empty)
            {
                throw new Exception($"cannot move an empty piece.\nMove: {move}\nBoard:\n{this}");
            }

            // Get the bitboard associated with the piece being moved.
            ulong bb = GetBitboardFromEnum(pieceToMove);

            // Update the bitboard to remove previous position and add new position.
            bb ^= 1UL << move.src | 1UL << move.dst;
            SetBitboardFromEnum(pieceToMove, bb);

            Piece pieceLandedOn;
            
            if (move.type == MoveType.EnPassant)
            {
                int epSquareIndex;

                try
                {
                    epSquareIndex = PieceSet.BitIndexes(epSquare)[0];
                }
                catch
                {
                    throw new Exception("cannot play en-passant where no en-passant location is apparent");
                }

                int shift = ((moveCounter + 1) & 1) == 0 ? 8 : -8;

                pieceLandedOn = BoardArray[(epSquareIndex + shift) ^ 56];
                
                // Clear the piece off the board array since the final
                // update to the array doesn't account for en-passant.
                BoardArray[(epSquareIndex + shift) ^ 56] = Piece.Empty;
            }
            else
            {
                pieceLandedOn = BoardArray[move.dst ^ 56];
            }

            if (move.type == MoveType.PawnDoublePush)
            {
                // When we push pawns, they will travel 2 squares. This means
                // we don't need to backtrack 1 square depending on which way
                // the pawn moved (up for white and down for black), we can
                // just "average" between the two to get exactly where the new
                // EP square should be.
                int newEPsquare = (move.src + move.dst) / 2;
                epSquare = 1UL << newEPsquare;

                // string square = $"{"abcdefgh"[newEPsquare % 8]}{"12345678"[newEPsquare / 8]}";
                // Console.WriteLine($"New EP square: {square}  (Move: {move})");
            }
            else
            {
                epSquare = 0;
            }

            move.capturedPiece = pieceLandedOn;

            // Update any bitboards with collisions (on captures)
            if (pieceLandedOn != Piece.Empty)
            {
                bb = GetBitboardFromEnum(pieceLandedOn);
                
                if (move.type != MoveType.EnPassant)
                {
                    bb ^= 1UL << move.dst;
                }
                else
                {
                    // If the pawn moving is white, shift bit down by 8.
                    // If the pawn moving is black, shift bit up by 8.
                    int shift = ((int)pieceToMove & 1) == 1 ? 8 : -8;
                    bb ^= 1UL << move.dst + shift;
                }

                SetBitboardFromEnum(pieceLandedOn, bb);
            }

            // Update the array
            BoardArray[move.src ^ 56] = Piece.Empty;
            BoardArray[move.dst ^ 56] = pieceToMove;

            moveCounter += 1;

            moveHistory.Add(move);
        }

        public void UndoMove()
        {
            // Get last move made and remove it off list
            Move previousMove = moveHistory.Last();
            moveHistory.RemoveAt(moveHistory.Count - 1);
            
            moveCounter--;

            // Get bitboard of piece that moved
            Piece pieceThatMoved = BoardArray[previousMove.dst ^ 56];

            ulong bb;

            if (pieceThatMoved != Piece.Empty)
            {
                bb = GetBitboardFromEnum(pieceThatMoved);
                bb ^= 1UL << previousMove.dst | 1UL << previousMove.src; // Undo move on bitboard

                if (previousMove.type == MoveType.EnPassant)
                {
                    Piece opponentPawnType = (moveCounter & 1) == 1 ? Piece.WhitePawn : Piece.BlackPawn;

                    ulong opponentPawnBB = GetBitboardFromEnum(opponentPawnType);

                    int shift = ((int)pieceThatMoved & 1) == 1 ? 8 : -8;
                    opponentPawnBB ^= 1UL << previousMove.dst + shift;

                    SetBitboardFromEnum(opponentPawnType, opponentPawnBB);

                    BoardArray[(previousMove.dst + shift) ^ 56] = opponentPawnType;

                    // Make sure to set the EP square back to what it was before
                    epSquare = 1UL << previousMove.dst;
                }
                
                SetBitboardFromEnum(pieceThatMoved, bb);
            }

            // Check if capture and update that bitboard
            if ( previousMove.capturedPiece != Piece.Empty
                 && previousMove.type != MoveType.EnPassant )
            {
                bb = GetBitboardFromEnum(previousMove.capturedPiece);
                bb ^= 1UL << previousMove.dst;
                
                SetBitboardFromEnum(previousMove.capturedPiece, bb);
            }

            // Update the board array
            BoardArray[previousMove.dst ^ 56] = Piece.Empty;
            BoardArray[previousMove.src ^ 56] = pieceThatMoved;
        }

        
        public ulong GetSquareAttackers(int square, Colour sideAttacking)
        {
            PieceSet attackingSideClass = sideAttacking == Colour.White ? White : Black;
            PieceSet sideBeingAttacked  = sideAttacking == Colour.White ? Black : White;

            ulong boardMask = attackingSideClass.ALL() | sideBeingAttacked.ALL();

            ulong attackers = 0;

            // Check for enemy knights
            attackers |= attackingSideClass.Knights & Moves.GetKnightMoveBitmask(boardMask, square);
            
            // Check for enemy bishops. Add a variable for reuse
            ulong bishopMoves = Moves.GetBishopMoveBitmask(boardMask, square);
            attackers |= attackingSideClass.Bishops & bishopMoves;

            // Check for enemy rooks. Add a variable for reuse
            ulong rookMoves = Moves.GetRookMoveBitmask(boardMask, square);
            attackers |= attackingSideClass.Rooks & rookMoves;

            // Check for queen attacks. Add a variable to combine both
            ulong queenMoves = bishopMoves | rookMoves;
            attackers |= attackingSideClass.Queens & queenMoves;

            // Check for king moves
            attackers |= attackingSideClass.King & Moves.GetKingMoveBitmask(boardMask, square);

            // Get pawn attacks
            ulong pawnAttacks;
            ulong squareToCheck = 1UL << square;
            
            if (sideAttacking == Colour.White)
            {
                pawnAttacks = (squareToCheck & ~BoardFile.A) >> 7 | (squareToCheck & ~BoardFile.H) >> 9;
            }
            else
            {
                pawnAttacks = (squareToCheck & ~BoardFile.A) << 7 | (squareToCheck & ~BoardFile.H) << 9;
            }

            // Check for pawn attacks
            attackers |= pawnAttacks & attackingSideClass.Pawns;

            return attackers;
        }


        public bool IsSquareAttacked(int square, Colour sideAttacking)
        {
            PieceSet attackingSideClass = sideAttacking == Colour.White ? White : Black;
            PieceSet sideBeingAttacked  = sideAttacking == Colour.White ? Black : White;

            ulong boardMask = attackingSideClass.ALL() | sideBeingAttacked.ALL();

            bool doesIntersect(ulong bb1, ulong bb2) { return (bb1 & bb2) != 0; }

            // Check for enemy knights
            if (doesIntersect(attackingSideClass.Knights, Moves.GetKnightMoveBitmask(boardMask, square)))
            {
                return true;
            }
            
            // Check for enemy bishops
            ulong bishopMoves = Moves.GetBishopMoveBitmask(boardMask, square);
            
            if (doesIntersect(attackingSideClass.Bishops, bishopMoves))
            {
                return true;
            }

            // Check for enemy rooks
            ulong rookMoves = Moves.GetRookMoveBitmask(boardMask, square);

            if (doesIntersect(attackingSideClass.Rooks, rookMoves))
            {
                return true;
            }

            // Check for enemy queens (combo of bishop and rooks).
            if (doesIntersect(attackingSideClass.Queens, bishopMoves | rookMoves))
            {
                return true;
            }

            // Check for king moves
            if (doesIntersect(attackingSideClass.King, Moves.GetKingMoveBitmask(boardMask, square)))
            {
                return true;
            }

            // Get pawn attacks
            ulong pawnAttacks;
            ulong squareToCheck = 1UL << square;
            
            if (sideAttacking == Colour.White)
            {
                pawnAttacks = (squareToCheck & ~BoardFile.A) >> 7 | (squareToCheck & ~BoardFile.H) >> 9;
            }
            else
            {
                pawnAttacks = (squareToCheck & ~BoardFile.A) << 7 | (squareToCheck & ~BoardFile.H) << 9;
            }

            // Check for pawn attacks
            if (doesIntersect(pawnAttacks, attackingSideClass.Pawns))
            {
                return true;
            }

            return false;
        }


        public List<Move> GenerateLegalMoves()
        {
            List<Move> moves = [];
            PieceSet sideToMove = (moveCounter & 1) == 0 ? White : Black;
            
            ulong whiteMask = White.ALL();
            ulong blackMask = Black.ALL();

            ulong boardMask = whiteMask | blackMask;

            void generateMoves(ulong bitboard, Action<ulong, ulong, int, List<Move>> func)
            {
                if (sideToMove == White)
                {
                    foreach (int position in PieceSet.BitIndexes(bitboard))
                    {
                        func(whiteMask, blackMask, position, moves);
                    }
                }
                else
                {
                    foreach (int position in PieceSet.BitIndexes(bitboard))
                    {
                        func(blackMask, whiteMask, position, moves);
                    }
                }
            }
            
            if ((moveCounter & 1) == 0) // White to move
            {
                Moves.GenerateWhitePawnMoves(whiteMask, blackMask, White.Pawns, epSquare, moves);
            }
            else
            {
                Moves.GenerateBlackPawnMoves(whiteMask, blackMask, Black.Pawns, epSquare, moves);
            }
        
            generateMoves(sideToMove.Bishops, Moves.GenerateBishopMoves);
            generateMoves(sideToMove.Knights, Moves.GenerateKnightMoves);
            generateMoves(sideToMove.Rooks,   Moves.GenerateRookMoves  );
            generateMoves(sideToMove.Queens,  Moves.GenerateQueenMoves );

            Moves.GenerateKingMoves(
                friendlyPieces: sideToMove,
                opponentPieces: sideToMove == White ? Black : White,
                square: PieceSet.BitIndexes(sideToMove.King)[0],
                moveListToAddTo: moves
            );

            Moves.GenerateCastlingMoves(
                sideToMove: (Colour)(moveCounter & 1),
                friendlyPieces: sideToMove,
                opponentPieces: sideToMove == White ? Black : White,
                castlingRights: castlingRights,
                moveListToAddTo: moves
            );
            
            return moves;
        }


        public override string ToString()
        {
            string[] board = new string[8];

            for (int rank = 0; rank < 8; rank++)
            {
                string line = "";

                for (int file = 0; file < 8; file++)
                {
                    int index = rank * 8 + file;

                    char stringPiece = BoardArray[index] switch
                    {
                        Piece.WhitePawn => 'P',
                        Piece.WhiteBishop => 'B',
                        Piece.WhiteKnight => 'N',
                        Piece.WhiteQueen => 'Q',
                        Piece.WhiteRook => 'R',
                        Piece.WhiteKing => 'K',

                        Piece.BlackPawn => 'p',
                        Piece.BlackBishop => 'b',
                        Piece.BlackKnight => 'n',
                        Piece.BlackRook => 'r',
                        Piece.BlackQueen => 'q',
                        Piece.BlackKing => 'k',

                        _ => '.'
                    };

                    line += stringPiece + " ";
                }

                board[rank] = line;
            }

            return string.Join("\n", board);
        }

        public string GetFEN()
        {
            string FEN = "";
            int emptySpaces = 0;

            // Encode the board into a FEN string
            for (int i = 0; i < 64; i++)
            {
                if (BoardArray[i] == Piece.Empty)
                {
                    emptySpaces += 1;
                    
                    if ((i & 7) == 7)
                    {
                        FEN += $"{emptySpaces}/";
                        emptySpaces = 0;
                    }

                    continue;
                }

                if (emptySpaces > 0)
                {
                    FEN += Convert.ToString(emptySpaces);
                    emptySpaces = 0;

                    if ((i & 7) == 7)
                    {
                        FEN += $"{emptySpaces}/";
                    }

                    // continue;
                }

                FEN += BoardArray[i] switch
                {
                    Piece.WhitePawn => 'P',
                    Piece.WhiteBishop => 'B',
                    Piece.WhiteKnight => 'N',
                    Piece.WhiteQueen => 'Q',
                    Piece.WhiteRook => 'R',
                    Piece.WhiteKing => 'K',

                    Piece.BlackPawn => 'p',
                    Piece.BlackBishop => 'b',
                    Piece.BlackKnight => 'n',
                    Piece.BlackRook => 'r',
                    Piece.BlackQueen => 'q',
                    Piece.BlackKing => 'k',

                    _ => throw new Exception("piece enum unaccounted for.")
                };

                if ((i & 7) == 7 && i != 63)
                {
                    FEN += "/";
                }
            }

            // Add the side to move
            string[] sidesToMove = ["w", "b"];
            FEN += " " + sidesToMove[moveCounter & 1];

            // Add the en-passant square
            FEN += " -"; // TODO: needs changing

            string castlingRightsToAdd;

            // Translate the castling rights from a byte to a string
            if (castlingRights == 0)
            {
                castlingRightsToAdd = "-";
            }
            else
            {
                castlingRightsToAdd = "";
                string[] castlingRightsValues = ["q", "k", "Q", "K"];

                // Go back to front (start at MSB instead of LSB)
                for (int x = 3; x > -1; x--)
                {
                    if ((castlingRights & (1 << x)) != 0)
                    {
                        castlingRightsToAdd += castlingRightsValues[x];
                    }
                }
            }

            // Add the castling rights
            FEN += " " + castlingRightsToAdd;

            // Add halftime move counter
            FEN += " 0"; // TODO: needs changing

            // Add fulltime move counter
            FEN += " " + Convert.ToString(moveCounter - (moveCounter & 1));
            
            return FEN;
        }
    }
}