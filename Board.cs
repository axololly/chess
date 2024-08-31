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
        public List<Move> moveHistory = [];
        public int sideToMove { get { return moveCounter & 1; } }
        public ulong epSquare;

        public Board(string FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            // Create piece sets for each side
            White = new(Colour.White);
            Black = new(Colour.Black);

            // Split the FEN string into sections
            string[] FENstringSections = FEN.Split(' ');

            // Assign variables to each of the sections
            string boardFEN           =  FENstringSections[0];
            string sideToMoveString   =  FENstringSections[1];
            string FENcastlingRights  =  FENstringSections[2];
            string epSquareString     =  FENstringSections[3];
            // string halftimeCounter    =  FENstringSections[4];
            string currentDoubleMove  =  FENstringSections[5];


            // Reverse FEN string to have a reversed board format
            boardFEN = string.Join("/", boardFEN.Split('/').Reverse());
            
            // Fill the board with pieces based on the FEN string
            BoardArray = new Piece[64];
            int cursor = 0;

            // Fill the board with ecah character
            foreach (char c in boardFEN)
            {
                // Skip these because we don't use a 2D array
                if (c == '/') continue;

                // If the current character is a number (eg. N)
                if (int.TryParse(c.ToString(), out int emptySpaces))
                {
                    // Add N empty spaces to the board.
                    for (int i = 0; i < emptySpaces; i++)
                    {
                        BoardArray[cursor++] = Piece.Empty;
                    }

                    // cursor += emptySpaces; // Keep cursor updated
                    continue;
                }

                // Pattern match each character in the FEN string to its
                // relative Piece enum to be inserted into the board.
                BoardArray[cursor] = c switch
                {
                    'b' => Piece.BlackBishop,
                    'n' => Piece.BlackKnight,
                    'r' => Piece.BlackRook,
                    'q' => Piece.BlackQueen,
                    'p' => Piece.BlackPawn,
                    'k' => Piece.BlackKing,

                    'B' => Piece.WhiteBishop,
                    'N' => Piece.WhiteKnight,
                    'R' => Piece.WhiteRook,
                    'Q' => Piece.WhiteQueen,
                    'K' => Piece.WhiteKing,
                    'P' => Piece.WhitePawn,

                    _ => throw new Exception($"invalid character '{c}' found in FEN string.")
                };

                // Pattern match each character in the FEN string to the bitboard
                // to insert a bit into, using the cursor argument.
                var _ = c switch
                {
                    'b' => Black.Bishops |= 1UL << cursor,
                    'n' => Black.Knights |= 1UL << cursor,
                    'r' => Black.Rooks   |= 1UL << cursor,
                    'q' => Black.Queens  |= 1UL << cursor,
                    'k' => Black.King    |= 1UL << cursor,
                    'p' => Black.Pawns   |= 1UL << cursor,

                    'B' => White.Bishops |= 1UL << cursor,
                    'N' => White.Knights |= 1UL << cursor,
                    'R' => White.Rooks   |= 1UL << cursor,
                    'Q' => White.Queens  |= 1UL << cursor,
                    'K' => White.King    |= 1UL << cursor,
                    'P' => White.Pawns   |= 1UL << cursor,

                    _ => throw new Exception($"invalid character '{c}' found in FEN string.")
                };

                // Increase the cursor to the new position
                cursor++;
            }


            // Set move counter based on side to move and current double move
            int sideToMoveIncrease = sideToMoveString switch
            {
                "w" => 0, "b" => 1,
                _ => throw new Exception($"invalid side to move character.")
            };

            if (!int.TryParse(currentDoubleMove, out int currentDoubleMoveNumber))
            {
                throw new Exception($"invalid current move character.");
            }

            moveCounter = currentDoubleMoveNumber * 2 + sideToMoveIncrease;


            // Set castling rights
            castlingRights = 0;

            foreach (char castlingRight in FENcastlingRights)
            {
                castlingRights |= castlingRight switch {
                    'K' => 0b1000,
                    'Q' => 0b0100,
                    'k' => 0b0010,
                    'q' => 0b0001,
                    _ => throw new Exception("invalid castling rights.")
                };
            }


            // Set an en-passant square
            if (epSquareString != "-")
            {
                int rank = "12345678".IndexOf(epSquareString[1]);
                int file = "abcdefgh".IndexOf(epSquareString[0]);

                int epSquareIndex = rank * 8 + file;

                epSquare = 1UL << epSquareIndex;
            }


            // TODO: Set halftime move counter
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

                _ => throw new Exception($"invalid enum type - cannot set bitboard for enum \"{pieceEnum}\".")
            };
        }


        public void MakeMove(Move move)
        {
            // Archive move
            moveHistory.Add(move);
            moveCounter++;

            // Modify the board based on the type of move played
            switch (move.type)
            {
                case MoveType.Normal:
                    Piece pieceToMove = BoardArray[move.src];
                    
                    // Update piece bitboard
                    ulong bb = GetBitboardFromEnum(pieceToMove);
                    bb ^= 1UL << move.src | 1UL << move.dst;
                    SetBitboardFromEnum(pieceToMove, bb);

                    // Check if piece was a capture
                    if (BoardArray[move.dst] != Piece.Empty)
                    {
                        // Get the piece that was captured
                        Piece pieceCaptured = BoardArray[move.dst];

                        // Update the captured piece's bitboard
                        bb = GetBitboardFromEnum(pieceCaptured);
                        bb ^= 1UL << move.dst;
                        SetBitboardFromEnum(pieceCaptured, bb);

                        move.capturedPiece = pieceCaptured;
                    }

                    // Update board array
                    BoardArray[move.src] = Piece.Empty;
                    BoardArray[move.dst] = pieceToMove;

                    // Clear en-passant square
                    epSquare = 0;

                    break;

                case MoveType.EnPassant:
                    // Clear EP square (already been used)
                    epSquare = 0;

                    // Operates like a normal move
                    pieceToMove = sideToMove == 0 ? Piece.WhitePawn : Piece.BlackPawn;
                    
                    // Update bitboard of piece
                    bb = GetBitboardFromEnum(pieceToMove);
                    bb ^= 1UL << move.src | 1UL << move.dst;
                    SetBitboardFromEnum(pieceToMove, bb);

                    // Update board array
                    BoardArray[move.src] = Piece.Empty;
                    BoardArray[move.dst] = pieceToMove;

                    // Get square of pawn to capture
                    int inFrontOfEPsquare = move.dst + (sideToMove == 0 ? -8 : 8);
                    
                    Piece opponentPawnType = sideToMove == 0 ? Piece.BlackPawn : Piece.WhitePawn;
                    
                    // Remove pawn from bitboard and update it
                    ulong opponentPawnBB = GetBitboardFromEnum(opponentPawnType);
                    opponentPawnBB ^= 1UL << inFrontOfEPsquare;
                    SetBitboardFromEnum(opponentPawnType, opponentPawnBB);

                    // Remove pawn from board array
                    BoardArray[inFrontOfEPsquare] = Piece.Empty;

                    break;
                
                case MoveType.PawnDoublePush:
                    // Get the square behind the pawn by getting the
                    // middle square between the start and end of the
                    // double pawn push.
                    int newEPsquare = (move.src + move.dst) / 2;
                    epSquare = 1UL << newEPsquare;

                    // Get piece to move
                    Piece pawnToMove = BoardArray[move.src];
                    
                    // Update piece bitboard
                    bb = GetBitboardFromEnum(pawnToMove);
                    bb ^= 1UL << move.src | 1UL << move.dst;
                    SetBitboardFromEnum(pawnToMove, bb);

                    // Update board array
                    BoardArray[move.src] = Piece.Empty;
                    BoardArray[move.dst] = pawnToMove;

                    break;
                
                case MoveType.Castling:
                    // Clear en-passant square
                    epSquare = 0;

                    // Get the king moving
                    Piece kingToMove = sideToMove == 0 ? Piece.WhiteKing : Piece.BlackKing;
                    
                    // Reset castling rights depending on side
                    castlingRights ^= (byte)(sideToMove == 0 ? 0b1100 : 0b0011);

                    // Update bitboard of king
                    bb = GetBitboardFromEnum(kingToMove);
                    bb ^= 1UL << move.src | 1UL << move.dst;
                    SetBitboardFromEnum(kingToMove, bb);

                    // Get position of rook to castle with on the board
                    int rookPosition = move.dst switch
                    {
                        2 => 0,    // from C1 to A1
                        6 => 7,    // from G1 to H1

                        58 => 56,  // from C8 to A8
                        62 => 63,  // from G8 to H8
                        
                        _ => throw new Exception($"invalid castling destination square. Move: {move}")
                    };

                    // Rook will always end up between the king's start and end square
                    int endRookPosition = (move.src + move.dst) / 2;

                    // Get rook enum (used for obtaining bitboard)
                    Piece rookEnum = sideToMove == 0 ? Piece.WhiteRook : Piece.BlackRook;
                    
                    // Update bitboard of rook
                    ulong rookBB = GetBitboardFromEnum(rookEnum);
                    rookBB ^= 1UL << rookPosition | 1UL << endRookPosition;
                    SetBitboardFromEnum(rookEnum, rookBB);

                    break;
                
                default: // move flag was unaccounted for
                    throw new Exception($"move flag \"{move.type}\" on move {move} unaccounted for.");
            }
        }

        public void UndoMove()
        {
            if (moveHistory.Count == 0)
            {
                throw new Exception("cannot undo when no moves on the board have been played.");
            }

            // Get and remove last move from move history
            Move prev = moveHistory[^1]; // Get last item from list
            moveHistory.RemoveAt(moveHistory.Count - 1);
            
            moveCounter--; // Decrease move counter

            // Edit board based on type of previous move
            switch (prev.type)
            {
                case MoveType.Normal:
                    Piece pieceThatMoved = BoardArray[prev.dst];

                    // Update bitboard of piece
                    ulong bb = GetBitboardFromEnum(pieceThatMoved);
                    bb ^= 1UL << prev.dst | 1UL << prev.src;
                    SetBitboardFromEnum(pieceThatMoved, bb);

                    // Check if a piece was captured
                    // If so, update their bitboard as well
                    if (prev.capturedPiece != Piece.Empty)
                    {
                        bb = GetBitboardFromEnum(prev.capturedPiece);
                        bb ^= 1UL << prev.dst;
                        SetBitboardFromEnum(prev.capturedPiece, bb);
                    }

                    BoardArray[prev.dst] = Piece.Empty;
                    BoardArray[prev.src] = pieceThatMoved;

                    break;

                case MoveType.EnPassant:
                    // Get whichever pawn type did the en-passant
                    Piece pawnType = sideToMove == 0 ? Piece.WhitePawn : Piece.BlackPawn;
                    
                    // Update bitboard of that piece
                    bb = GetBitboardFromEnum(pawnType);
                    bb ^= 1UL << prev.dst | 1UL << prev.src;
                    SetBitboardFromEnum(pawnType, bb);

                    Piece opponentPawnType = sideToMove == 0 ? Piece.BlackPawn : Piece.WhitePawn;

                    // Get the square in front of the EP square (relative to the side moving)
                    int squarePawnWasTakenFrom = prev.dst + (sideToMove == 0 ? -8 : 8);

                    // Update bitboard of opponent pawn type
                    // (replace the pawn that was captured)
                    bb = GetBitboardFromEnum(opponentPawnType);
                    bb ^= 1UL << squarePawnWasTakenFrom;
                    SetBitboardFromEnum(opponentPawnType, bb);

                    // Update board array
                    BoardArray[prev.src] = pawnType;
                    BoardArray[prev.dst] = Piece.Empty;
                    BoardArray[squarePawnWasTakenFrom] = opponentPawnType;

                    // En-passant square is previous move destination
                    epSquare = 1UL << prev.dst;

                    break;

                case MoveType.PawnDoublePush:
                    pawnType = sideToMove == 0 ? Piece.WhitePawn : Piece.BlackPawn;

                    bb = GetBitboardFromEnum(pawnType);
                    bb ^= 1UL << prev.dst | 1UL << prev.src;
                    SetBitboardFromEnum(pawnType, bb);

                    // Remove en-passant square
                    epSquare = 0;

                    // Update board array
                    BoardArray[prev.src] = pawnType;
                    BoardArray[prev.dst] = Piece.Empty;

                    break;

                case MoveType.Castling:
                    Piece kingEnum = sideToMove == 0 ? Piece.WhiteKing : Piece.BlackKing;

                    // Update king bitboard
                    bb = GetBitboardFromEnum(kingEnum);
                    bb ^= 1UL << prev.dst | 1UL << prev.src;
                    SetBitboardFromEnum(kingEnum, bb);

                    // Get rook position
                    // Rook position is always between the king's source
                    // and the king's destination square
                    int rookPosition = (prev.src + prev.dst) / 2;

                    // Get the end rook position:
                    /*
                       1 => start pos
                       2 => end pos

                       2 . . 1 . 1 . 2
                       . . . . . . . .
                       . . . . . . . .
                       . . . . . . . .
                       . . . . . . . .
                       . . . . . . . .
                       . . . . . . . .
                       2 . . 1 . 1 . 2
                    */
                    int endRookPosition = rookPosition switch
                    {
                        3 => 0, // from D1 to A1
                        5 => 7, // from F1 to H1

                        59 => 56, // from D8 to A8
                        61 => 63, // from F8 to H8

                        _ => throw new Exception("invalid rook position.")
                    };
                    
                    Piece rookEnum = sideToMove == 0 ? Piece.WhiteRook : Piece.BlackRook;

                    // Update rook bitboard
                    bb = GetBitboardFromEnum(rookEnum);
                    bb ^= 1UL << rookPosition | 1UL << endRookPosition;
                    SetBitboardFromEnum(rookEnum, bb);

                    // Reset castling rights conditionally
                    if (prev.src < prev.dst) // castle left
                    {
                        if (sideToMove == 0) // white castling
                        {
                            castlingRights |= 0b1000;
                        }
                        else // black castling
                        {
                            castlingRights |= 0b0010;
                        }
                    }
                    else // castle right
                    {
                        if (sideToMove == 0) // white castling
                        {
                            castlingRights |= 0b0100;
                        }
                        else // black castling
                        {
                            castlingRights |= 0b0001;
                        }
                    }

                    // Update board array
                    BoardArray[prev.dst] = Piece.Empty;
                    BoardArray[prev.src] = kingEnum;

                    BoardArray[rookPosition] = rookEnum;
                    BoardArray[endRookPosition] = Piece.Empty;

                    // Remove en-passant square
                    epSquare = 0;

                    break;

                default:
                    throw new Exception($"move flag \"{prev.type}\" on move {prev} unaccounted for.");
            }
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

                board[7 - rank] = line;
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
                    }                }

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