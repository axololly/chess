using Utilities;
using MoveGeneration;
using System.Numerics;
using Bitboards;

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

        public ulong mask { get { return Bishops | Knights | Rooks | Pawns | Queens | King; } }

        public int KingSquare { get { return BitOperations.TrailingZeroCount(King); } }

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

        private ulong BaseAttackingBitmask(ulong occupancy)
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

            attacks |= Moves.fmt.KING_MOVES_TABLE[KingSquare]; // Get king moves from table

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

            return attacks;
        }

        public ulong AttackingBitmask(ulong opponentOccupancy)
        {
            return BaseAttackingBitmask(mask | opponentOccupancy) & ~mask;
        }

        public ulong ProtectedBitmask(ulong opponentOccupancy)
        {
            return BaseAttackingBitmask(mask | opponentOccupancy) & mask & ~King; // cannot protect king
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

    public class BoardInfo
    {
        public ulong EPsquare;
        // public int halftimeCounter;
        public byte castlingRights;
        public Piece capturedPiece = Piece.Empty;
        
        public ulong checkmask;
        public ulong D_pinmask;
        public ulong HV_pinmask;
    }

    public class Board
    {
        public Piece[] BoardArray = new Piece[64];
        public PieceSet White = new PieceSet(Colour.White);
        public PieceSet Black = new PieceSet(Colour.Black);
        public int moveCounter;
        public byte castlingRights; // 0b1111 => KQkq
        public Stack<Move> moveHistory = new();
        public Stack<BoardInfo> boardHistory = new();

        public ulong HV_pinmask;
        public ulong D_pinmask;
        public ulong checkmask;

        public ulong boardMask { get { return White.mask | Black.mask; } }
        
        public int SideToMove { get { return moveCounter & 1; } }
        public Colour ColourToMove { get { return (Colour)SideToMove; } }
        public PieceSet PlayerToMove { get { return SideToMove == 0 ? White : Black; } }
        public PieceSet OpponentToMove { get { return SideToMove == 0 ? Black : White; }}

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
            Array.Fill(BoardArray, Piece.Empty);

            int cursor = 0;

            // Fill the board with ecah character
            foreach (char c in boardFEN)
            {
                // Skip these because we don't use a 2D array
                if (c == '/') continue;

                // If the current character is a number (eg. N)
                if (int.TryParse(c.ToString(), out int emptySpaces))
                {
                    // Add N empty spaces to the 
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

            if (FENcastlingRights != "-")
            {
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
            }


            // Set an en-passant square
            if (epSquareString != "-")
            {
                int rank = "12345678".IndexOf(epSquareString[1]);
                int file = "abcdefgh".IndexOf(epSquareString[0]);

                int epSquareIndex = rank * 8 + file;

                epSquare = 1UL << epSquareIndex;
            }

            UpdatePinsAndCheckers();

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

                Piece.Empty => throw new Exception($"cannot obtain bitboard for an empty piece. Perhaps your start square is wrong?"),

                _ => throw new Exception($"cannot set bitboard for unaccounted enum \"{pieceEnum}\".") // raised errors if not in place
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

                Piece.Empty => throw new Exception($"cannot set bitboard for an empty piece enum."),
                
                _ => throw new Exception($"cannot set bitboard for unaccounted enum \"{pieceEnum}\".") // raised errors if not in place
            };
        }

        public void MakeMove(Move move)
        {
            // Archive move
            moveHistory.Push(move);
            moveCounter++;

            // Board info for archiving
            BoardInfo boardInfo = new()
            {
                EPsquare = epSquare,
                castlingRights = castlingRights,
                checkmask = checkmask,
                HV_pinmask = HV_pinmask,
                D_pinmask = D_pinmask
            };

            // Modify the board based on the type of move played
            switch (move.type)
            {
                case MoveType.Normal:
                    Piece pieceToMove = BoardArray[move.src];

                    // Handle disabling castling rights when moving kings or rooks
                    switch (pieceToMove)
                    {
                        case Piece.WhiteKing:
                            // Remove white castling rights
                            castlingRights ^= 0b1100;
                            break;
                        
                        case Piece.BlackKing:
                            castlingRights ^= 0b0011;
                            break;

                        case Piece.WhiteRook:
                            castlingRights ^= (byte)(move.src < PlayerToMove.KingSquare ? 0b0100 : 0b1000);
                            break;
                        
                        case Piece.BlackRook:
                            castlingRights ^= (byte)(move.src < PlayerToMove.KingSquare ? 0b0001 : 0b0010);
                            break;

                        default:
                            break;
                    }
                    
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

                        boardInfo.capturedPiece = pieceCaptured;
                    }

                    // Update board array
                    BoardArray[move.src] = Piece.Empty;
                    BoardArray[move.dst] = pieceToMove;

                    // Clear en-passant square
                    epSquare = 0;

                    break;

                case MoveType.EnPassant:
                    // Operates like a normal move
                    pieceToMove = Piece.BlackPawn - SideToMove;
                    
                    // Update bitboard of piece
                    bb = GetBitboardFromEnum(pieceToMove);
                    bb ^= 1UL << move.src | 1UL << move.dst;
                    SetBitboardFromEnum(pieceToMove, bb);

                    // Get square of pawn to capture
                    int inFrontOfEPsquare = move.dst + (SideToMove == 1 ? -8 : 8);
                    
                    Piece opponentPawnType = BoardArray[inFrontOfEPsquare];
                    
                    // Remove pawn from bitboard and update it
                    ulong opponentPawnBB = GetBitboardFromEnum(opponentPawnType);

                    opponentPawnBB ^= 1UL << inFrontOfEPsquare;
                    SetBitboardFromEnum(opponentPawnType, opponentPawnBB);

                    // Remove pawn from board array
                    BoardArray[move.src] = Piece.Empty;
                    BoardArray[move.dst] = pieceToMove;
                    BoardArray[inFrontOfEPsquare] = Piece.Empty;

                    // Clear EP square (already been used)
                    epSquare = 0;

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
                    Piece kingToMove = Piece.WhiteKing + SideToMove;
                    
                    // Reset castling rights depending on side
                    castlingRights ^= (byte)(SideToMove == 0 ? 0b1100 : 0b0011);

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
                        
                        _ => throw new Exception($"invalid castling destination square: {move.dst} (Move: {move})")
                    };

                    // Rook will always end up between the king's start and end square
                    int endRookPosition = (move.src + move.dst) / 2;

                    // Get rook enum (used for obtaining bitboard)
                    Piece rookEnum = Piece.BlackRook - SideToMove;
                    
                    // Update bitboard of rook
                    ulong rookBB = GetBitboardFromEnum(rookEnum);
                    rookBB ^= 1UL << rookPosition | 1UL << endRookPosition;
                    SetBitboardFromEnum(rookEnum, rookBB);

                    // Update array
                    BoardArray[move.src] = Piece.Empty;
                    BoardArray[rookPosition] = Piece.Empty;
                    
                    BoardArray[move.dst] = kingToMove;
                    BoardArray[endRookPosition] = rookEnum;

                    break;
                
                default: // move flag was unaccounted for
                    throw new Exception($"move flag \"{move.type}\" on move {move} unaccounted for.");
            }

            boardHistory.Push(boardInfo);
            UpdatePinsAndCheckers();
        }

        public void UndoMove()
        {
            if (moveHistory.Count == 0)
            {
                throw new Exception("cannot undo when no moves on the board have been played.");
            }

            // Get last move and last board info
            Move previousMove = moveHistory.Pop();
            BoardInfo previousBoardInfo = boardHistory.Pop();

            checkmask = previousBoardInfo.checkmask;
            D_pinmask = previousBoardInfo.D_pinmask;
            HV_pinmask = previousBoardInfo.HV_pinmask;
            
            moveCounter--; // Decrease move counter

            // Edit board based on type of previous move
            switch (previousMove.type)
            {
                case MoveType.Normal:
                    Piece pieceThatMoved = BoardArray[previousMove.dst];

                    // Update bitboard of piece
                    ulong bb = GetBitboardFromEnum(pieceThatMoved);

                    bb ^= 1UL << previousMove.dst | 1UL << previousMove.src;
                    SetBitboardFromEnum(pieceThatMoved, bb);

                    // Check if a piece was captured
                    // If so, update their bitboard as well
                    if (previousBoardInfo.capturedPiece != Piece.Empty)
                    {
                        bb = GetBitboardFromEnum(previousBoardInfo.capturedPiece);
                        bb ^= 1UL << previousMove.dst;
                        SetBitboardFromEnum(previousBoardInfo.capturedPiece, bb);
                    }

                    BoardArray[previousMove.dst] = previousBoardInfo.capturedPiece;
                    BoardArray[previousMove.src] = pieceThatMoved;

                    break;

                case MoveType.EnPassant:
                    // Get whichever pawn type did the en-passant
                    Piece pawnType = Piece.WhitePawn + SideToMove;
                    
                    // Update bitboard of that piece
                    bb = GetBitboardFromEnum(pawnType);
                    bb ^= 1UL << previousMove.dst | 1UL << previousMove.src;
                    SetBitboardFromEnum(pawnType, bb);

                    Piece opponentPawnType = Piece.BlackPawn - SideToMove;

                    // Get the square in front of the EP square (relative to the side moving)
                    int squarePawnWasTakenFrom = previousMove.dst + (SideToMove == 0 ? -8 : 8);

                    // Update bitboard of opponent pawn type
                    // (replace the pawn that was captured)
                    bb = GetBitboardFromEnum(opponentPawnType);
                    bb ^= 1UL << squarePawnWasTakenFrom;
                    SetBitboardFromEnum(opponentPawnType, bb);

                    // Update board array
                    BoardArray[previousMove.src] = pawnType;
                    BoardArray[previousMove.dst] = Piece.Empty;
                    BoardArray[squarePawnWasTakenFrom] = opponentPawnType;

                    // En-passant square is previous move destination
                    epSquare = previousBoardInfo.EPsquare;

                    break;

                case MoveType.PawnDoublePush:
                    pawnType = BoardArray[previousMove.dst];

                    bb = GetBitboardFromEnum(pawnType);
                    bb ^= 1UL << previousMove.dst | 1UL << previousMove.src;
                    SetBitboardFromEnum(pawnType, bb);

                    // Remove en-passant square
                    epSquare = 0;

                    // Update board array
                    BoardArray[previousMove.src] = pawnType;
                    BoardArray[previousMove.dst] = Piece.Empty;

                    break;

                case MoveType.Castling:
                    Piece kingEnum = Piece.BlackKing - SideToMove;

                    // Update king bitboard
                    bb = GetBitboardFromEnum(kingEnum);
                    bb ^= 1UL << previousMove.dst | 1UL << previousMove.src;
                    SetBitboardFromEnum(kingEnum, bb);

                    // Get rook position
                    // Rook position is always between the king's source
                    // and the king's destination square
                    int rookPosition = (previousMove.src + previousMove.dst) / 2;

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
                    
                    Piece rookEnum = Piece.BlackRook - SideToMove;

                    // Update rook bitboard
                    bb = GetBitboardFromEnum(rookEnum);
                    bb ^= 1UL << rookPosition | 1UL << endRookPosition;
                    SetBitboardFromEnum(rookEnum, bb);

                    // Reset castling rights
                    castlingRights = previousBoardInfo.castlingRights;

                    // Update board array
                    BoardArray[previousMove.dst] = Piece.Empty;
                    BoardArray[previousMove.src] = kingEnum;

                    BoardArray[rookPosition] = rookEnum;
                    BoardArray[endRookPosition] = Piece.Empty;

                    // Remove en-passant square
                    epSquare = 0;

                    break;

                default:
                    throw new Exception($"move flag \"{previousMove.type}\" on move {previousMove} unaccounted for.");
            }

            UpdatePinsAndCheckers();
        }

        public List<Move> GenerateLegalMoves()
        {
            List<Move> moves = [];

            foreach (int position in PieceSet.BitIndexes(PlayerToMove.Queens | PlayerToMove.Rooks))
            {
                Moves.GenerateRookMoves(
                    friendlyOccupancy: PlayerToMove.mask,
                    opponentOccupancy: OpponentToMove.mask,
                    square: position,
                    HV_pinmask: HV_pinmask,
                    D_pinmask: D_pinmask,
                    checkmask: checkmask,
                    moveListToAddTo: moves
                );
            }

            foreach (int position in PieceSet.BitIndexes(PlayerToMove.Queens | PlayerToMove.Bishops))
            {
                Moves.GenerateBishopMoves(
                    friendlyOccupancy: PlayerToMove.mask,
                    opponentOccupancy: OpponentToMove.mask,
                    square: position,
                    HV_pinmask: HV_pinmask,
                    D_pinmask: D_pinmask,
                    checkmask: checkmask,
                    moveListToAddTo: moves
                );
            }

            foreach (int position in PieceSet.BitIndexes(PlayerToMove.Knights))
            {
                Moves.GenerateKnightMoves(
                    friendlyOccupancy: PlayerToMove.mask,
                    square: position,
                    moveListToAddTo: moves,
                    pinmask: D_pinmask | HV_pinmask,
                    checkmask: checkmask
                );
            }

            Moves.GeneratePawnMoves(
                friendlyPieces: PlayerToMove,
                opponentPieces: OpponentToMove,
                epSquareBitboard: epSquare,
                moveListToAddTo: moves,
                HV_pinmask: HV_pinmask,
                D_pinmask: D_pinmask,
                checkmask: checkmask,
                side: ColourToMove
            );

            Moves.GenerateKingMoves(
                friendlyPieces: PlayerToMove,
                opponentPieces: OpponentToMove,
                square: PlayerToMove.KingSquare,
                moveListToAddTo: moves
            );

            Moves.GenerateCastlingMoves(
                sideToMove: ColourToMove,
                friendlyPieces: PlayerToMove,
                opponentPieces: OpponentToMove,
                castlingRights: castlingRights,
                moveListToAddTo: moves
            );
            
            return moves;
        }


        static ulong RayBetween(int square1, int square2)
        {
            int file1, rank1, file2, rank2;

            file1 = square1 % 8;
            rank1 = square1 / 8;

            file2 = square2 % 8;
            rank2 = square2 / 8;

            if (file1 == file2 || rank1 == rank2) // same row or file
            {
                return Moves.GetRookMoveBitmask(1UL << square2, square1)
                    & Moves.GetRookMoveBitmask(1UL << square1, square2);
            }

            if (Math.Abs(file1 - file2) == Math.Abs(rank1 - rank2))
            {
                return Moves.GetBishopMoveBitmask(1UL << square2, square1)
                    & Moves.GetBishopMoveBitmask(1UL << square1, square2);
            }

            throw new Exception($"cannot form ray between squares \"{square1}\" and \"{square2}\" becuase they are not on the same line.");
        }


        public void UpdatePinsAndCheckers()
        {
            PieceSet us = PlayerToMove;
            PieceSet them = OpponentToMove;

            // For checking knights:
            ulong knightCheckers = Moves.GetKnightMoveBitmask(0, us.KingSquare) & them.Knights;

            // For checking pawns:
            var pawnMoves = Moves.GetPawnMoves(
                friendlyOccupancy: us.mask,
                opponentOccupancy: them.mask,
                pawnBitboard: us.King,
                epSquareBitboard: 0,
                HV_pinmask: HV_pinmask,
                D_pinmask: D_pinmask,
                checkmask: checkmask,
                side: ColourToMove
            );
            ulong pawnCheckers = (pawnMoves.LeftAttacks | pawnMoves.RightAttacks) & them.Pawns;

            // For rooks, queens and bishops
            ulong queens = GetBitboardFromEnum(Piece.BlackQueen - SideToMove);

            ulong bishopsQueens = GetBitboardFromEnum(Piece.BlackBishop - SideToMove) ^ queens;
            ulong rooksQueens = GetBitboardFromEnum(Piece.BlackRook - SideToMove) ^ queens;

            ulong bishopAttacks = bishopsQueens & Moves.GetBishopMoveBitmask(them.mask, us.KingSquare);
            ulong rookAttacks = rooksQueens & Moves.GetRookMoveBitmask(them.mask, us.KingSquare );
            
            checkmask = pawnCheckers | knightCheckers;
            
            D_pinmask = 0;
            HV_pinmask = 0;
            
            while (bishopAttacks > 0)
            {
                int sq = BitOperations.TrailingZeroCount(bishopAttacks);
                bishopAttacks ^= 1UL << sq; // remove LSB

                ulong checkray = RayBetween(us.KingSquare, sq);
                ulong blockers = checkray & us.mask;
                int numBlockers = BitOperations.PopCount(blockers);

                if (numBlockers == 0)
                {
                    checkmask |= checkray | 1UL << sq;
                }
                else if (numBlockers == 1)
                {
                    D_pinmask |= checkray | 1UL << sq | blockers;
                }
            }

            while (rookAttacks > 0)
            {
                int sq = BitOperations.TrailingZeroCount(rookAttacks);
                rookAttacks ^= 1UL << sq; // remove LSB

                ulong checkray = RayBetween(us.KingSquare, sq);
                ulong blockers = checkray & us.mask;
                int numBlockers = BitOperations.PopCount(blockers);

                if (numBlockers == 0)
                {
                    checkmask |= checkray | 1UL << sq;
                }
                else if (numBlockers == 1)
                {
                    HV_pinmask |= checkray | 1UL << sq | blockers;
                }
            }

            if (checkmask == 0)
            {
                checkmask = ulong.MaxValue;
            }
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
                    }
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

            // Reverse bits of FEN to be legal
            FEN = string.Join("/", FEN.Split("/").Reverse());

            // Add the side to move
            string[] sidesToMove = ["w", "b"];
            FEN += " " + sidesToMove[SideToMove];

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

            // Add the en-passant square
            if (epSquare == 0)
            {
                FEN += " -";
            }
            else
            {
                int sq = BitOperations.TrailingZeroCount(epSquare);
                string sqName = $"{"abcedfgh"[sq % 8]}{"12345678"[sq / 8]}";
                FEN += " " + sqName;
            }

            // Add halftime move counter
            FEN += " 0"; // TODO: needs changing

            // Add fulltime move counter
            FEN += " " + Convert.ToString((moveCounter - SideToMove) / 2);
            
            return FEN;
        }
    }
}