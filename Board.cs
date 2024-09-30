using Chess.Utilities;
using Chess.MoveGen;
using Chess.Bitmasks;
using Chess.Castling;
using Types.Bitboards;
using Types.Squares;
using Types.Zobrist;

namespace Chess
{
    public struct PieceSet(Colour colour)
    {
        public Colour colour = colour;

        public Bitboard Bishops = 0;
        public Bitboard Knights = 0;
        public Bitboard Rooks = 0;
        public Bitboard Pawns = 0;
        public Bitboard Queens = 0;
        public Bitboard King = 0;

        public Bitboard Mask { get { return Bishops | Knights | Rooks | Pawns | Queens | King; } }

        public Square KingSquare { get {
            if (!King) throw new Exception($"{colour} king bitboard is not present.");
            if (King.BitCount > 2) throw new Exception($"{colour} has multiple kings on the board.");
            
            return King.ReadLSB();
        } }

        public Bitboard BaseAttackingBitmask(Bitboard occupancy)
        {
            Bitboard attacks = 0;
            Bitboard T;

            T = Bishops | Queens;
            while (T) attacks |= Bitmask.ForBishop(occupancy, T.PopLSB());

            T = Rooks | Queens;
            while (T) attacks |= Bitmask.ForRook(occupancy, T.PopLSB());

            T = Knights;
            while (T) attacks |= Bitmask.ForKnight(0, T.PopLSB());


            attacks |= Bitmask.ForKing(
                enemyOrEmpty: Bitboard.Filled,
                opponentAttacks: 0,
                square: KingSquare
            );

            Direction upLeft, upRight;

            if (colour == Colour.White)
            {
                upLeft  = Direction.Northwest;
                upRight = Direction.Northeast;
            }
            else
            {
                upLeft  = Direction.Southeast;
                upRight = Direction.Southwest;
            }

            attacks |= Pawns.Shift(upLeft) | Pawns.Shift(upRight);

            return attacks;
        }

        public Bitboard AttackingBitmask(Bitboard opponentOccupancy)
        {
            return BaseAttackingBitmask(Mask | opponentOccupancy) & ~Mask;
        }

        public Bitboard ProtectedBitmask(Bitboard opponentOccupancy)
        {
            return BaseAttackingBitmask(Mask | opponentOccupancy) & Mask & ~King; // cannot protect king
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

    public struct BoardInfo
    {
        public Square EPsquare;
        public uint halfMoveClock;
        public CastlingRights castlingRights;
        public Piece capturedPiece = Piece.Empty;
        
        public Bitboard checkmask;
        public Bitboard pinD;
        public Bitboard pinHV;

        public BoardInfo() {}
    }

    public class Board
    {
        public Piece[] Mailbox;
        public PieceSet White;
        public PieceSet Black;
        public int moveCounter;
        public uint halfMoveClock;
        public CastlingRights castlingRights; // 0b1111 => KQkq
        public Stack<Move> moveHistory = new();
        public Stack<BoardInfo> boardHistory = new();

        public Bitboard pinHV;
        public Bitboard pinD;
        public Bitboard checkmask;
        public Bitboard checkers;

        public Bitboard boardMask { get { return White.Mask | Black.Mask; } }
        public string FEN { get { return GetFEN(); } }
        
        public int SideToMove { get { return moveCounter & 1; } }
        public Colour ColourToMove { get { return (Colour)SideToMove; } }
        public PieceSet PlayerToMove { get { return SideToMove == 0 ? White : Black; } }
        public PieceSet OpponentToMove { get { return SideToMove == 0 ? Black : White; }}

        public Square epSquare;

        public ulong ZobristKey { get; set; }
        public Stack<ulong> PastZobristHashes { get; set; }

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
            string halftimeCounter    =  FENstringSections[4];
            string currentDoubleMove  =  FENstringSections[5];

            // Reverse FEN string to have a reversed board format
            boardFEN = string.Join("/", boardFEN.Split('/').Reverse());
            
            // Fill the board with pieces based on the FEN string
            Mailbox = new Piece[64];
            Array.Fill(Mailbox, Piece.Empty);

            Zobrist.Init();

            ZobristKey = 0;
            PastZobristHashes = [];

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
                        Mailbox[cursor++] = Piece.Empty;
                    }

                    // cursor += emptySpaces; // Keep cursor updated
                    continue;
                }

                // Pattern match each character in the FEN string to its
                // relative Piece enum to be inserted into the board and
                // add a bit to the corresponding part of each bitboard.
                switch (c)
                {
                    case 'b':
                        Mailbox[cursor] = Piece.BlackBishop;
                        Black.Bishops |= 1UL << cursor;
                        
                        ZobristKey ^= Zobrist.HashPieceAndSquare(Piece.BlackBishop, cursor);
                        
                        break;

                    case 'n':
                        Mailbox[cursor] = Piece.BlackKnight;
                        Black.Knights |= 1UL << cursor;
                        
                        ZobristKey ^= Zobrist.HashPieceAndSquare(Piece.BlackKnight, cursor);
                        
                        break;

                    case 'r':
                        Mailbox[cursor] = Piece.BlackRook;
                        Black.Rooks |= 1UL << cursor;
                        
                        ZobristKey ^= Zobrist.HashPieceAndSquare(Piece.BlackRook, cursor);
                        
                        break;
                    
                    case 'q':
                        Mailbox[cursor] = Piece.BlackQueen;
                        Black.Queens |= 1UL << cursor;
                        
                        ZobristKey ^= Zobrist.HashPieceAndSquare(Piece.BlackQueen, cursor);
                        
                        break;
                    
                    case 'k':
                        Mailbox[cursor] = Piece.BlackKing;
                        Black.King |= 1UL << cursor;
                        
                        ZobristKey ^= Zobrist.HashPieceAndSquare(Piece.BlackKing, cursor);
                        
                        break;
                    
                    case 'p':
                        Mailbox[cursor] = Piece.BlackPawn;
                        Black.Pawns |= 1UL << cursor;
                        
                        ZobristKey ^= Zobrist.HashPieceAndSquare(Piece.BlackPawn, cursor);
                        
                        break;
                    
                    case 'B':
                        Mailbox[cursor] = Piece.WhiteBishop;
                        White.Bishops |= 1UL << cursor;
                        
                        ZobristKey ^= Zobrist.HashPieceAndSquare(Piece.WhiteBishop, cursor);
                        
                        break;

                    case 'N':
                        Mailbox[cursor] = Piece.WhiteKnight;
                        White.Knights |= 1UL << cursor;
                        
                        ZobristKey ^= Zobrist.HashPieceAndSquare(Piece.WhiteKnight, cursor);
                        
                        break;

                    case 'R':
                        Mailbox[cursor] = Piece.WhiteRook;
                        White.Rooks |= 1UL << cursor;
                        
                        ZobristKey ^= Zobrist.HashPieceAndSquare(Piece.WhiteRook, cursor);
                        
                        break;
                    
                    case 'Q':
                        Mailbox[cursor] = Piece.WhiteQueen;
                        White.Queens |= 1UL << cursor;
                        
                        ZobristKey ^= Zobrist.HashPieceAndSquare(Piece.WhiteQueen, cursor);
                        
                        break;
                    
                    case 'K':
                        Mailbox[cursor] = Piece.WhiteKing;
                        White.King |= 1UL << cursor;
                        
                        ZobristKey ^= Zobrist.HashPieceAndSquare(Piece.WhiteKing, cursor);
                        
                        break;
                    
                    case 'P':
                        Mailbox[cursor] = Piece.WhitePawn;
                        White.Pawns |= 1UL << cursor;
                        
                        ZobristKey ^= Zobrist.HashPieceAndSquare(Piece.WhitePawn, cursor);
                        
                        break;

                    default:
                        throw new Exception($"invalid character '{c}' was found in the FEN string.");
                }

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

            ZobristKey ^= Zobrist.HashColor((Colour)(sideToMoveIncrease ^ 1));

            // Set castling rights
            castlingRights = CastlingRights.FromString(FENcastlingRights);

            ZobristKey ^= Zobrist.HashCastlingRights(castlingRights);

            // Set the halftime counter
            if (!uint.TryParse(halftimeCounter, out uint HTC))
            {
                throw new Exception("half time counter was not a positive integer.");
            }

            halfMoveClock = HTC * 2;

            // Set an en-passant square
            epSquare = new(epSquareString);

            ZobristKey ^= Zobrist.HashEnPassant(epSquare);

            PastZobristHashes.Push(ZobristKey);

            UpdatePinsAndCheckers();

            if (ViolatedRepetitionRule())
            {
                throw new Exception($"cannot construct position: violated repetition rule. Game is already a draw.");
            }

            if (Violated50MoveRule())
            {
                throw new Exception($"cannot construct position: violated 50-move rule. Game is already a draw.");
            }
        }


        public Bitboard GetBitboardFromEnum(Piece pieceEnum)
        {
            Move getLastMovePlayed()
            {
                var T = moveHistory.Pop();
                moveHistory.Push(T);
                return T;
            }

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

                Piece.Empty => throw new Exception($"cannot obtain bitboard for an empty piece on move {getLastMovePlayed()}.\nBoard:\n{this}\n\nMove history: [{string.Join(", ", moveHistory.Reverse())}]\n\nPerhaps your start square is wrong?\n"),

                _ => throw new Exception($"cannot set bitboard for unaccounted enum \"{pieceEnum}\".") // raised errors if not in place
            };
        }

        public void SetBitboardFromEnum(Piece pieceEnum, Bitboard bitboard)
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
            if (ViolatedRepetitionRule() || Violated50MoveRule())
            {
                throw new Exception($"cannot play move {move}: violated repetition rule. Game is already a draw.");
            }

            // Archive move
            moveHistory.Push(move);
            moveCounter++;

            Piece pieceToMove = Mailbox[move.src];
            Piece pieceCaptured = Mailbox[move.dst];

            // Board info for archiving
            BoardInfo boardInfo = new()
            {
                EPsquare = epSquare,
                castlingRights = castlingRights,
                checkmask = checkmask,
                pinHV = pinHV,
                pinD = pinD
            };

            // Modify the board based on the type of move played
            switch (move.type)
            {
                case MoveType.Normal:                    
                    // Handle disabling castling rights when moving kings or rooks
                    switch (pieceToMove)
                    {
                        case Piece.WhiteKing: // Remove white castling rights
                            castlingRights.DisableBoth(Colour.White);
                            break;
                        
                        case Piece.BlackKing:
                            castlingRights.DisableBoth(Colour.Black);
                            
                            break;

                        case Piece.WhiteRook:
                            if (move.src == Squares.A1) castlingRights.DisableQueenside(Colour.White); // starting queenside rook position
                            if (move.src == Squares.H1) castlingRights.DisableKingside(Colour.White); // starting kingside rook position

                            break;
                        
                        case Piece.BlackRook:
                            if (move.src == Squares.A8) castlingRights.DisableQueenside(Colour.Black); // starting queenside rook position
                            if (move.src == Squares.H8) castlingRights.DisableKingside (Colour.Black); // starting kingside rook position
                            
                            break;

                        default:
                            break;
                    }
                    
                    // Update piece bitboard
                    Bitboard bb = GetBitboardFromEnum(pieceToMove);

                    bb ^= move.src.Bitboard | move.dst.Bitboard;
                    SetBitboardFromEnum(pieceToMove, bb);

                    // Check if piece was a capture
                    if (pieceCaptured != Piece.Empty)
                    {
                        // If the piece captured was a rook, remove castling rights
                        // for whatever side it was taken from
                        if (pieceCaptured == Piece.WhiteRook)
                        {
                            if (move.dst == Squares.A1) castlingRights.DisableQueenside(Colour.White); // starting queenside rook position
                            if (move.dst == Squares.H1) castlingRights.DisableKingside (Colour.White); // starting kingside rook position
                        }

                        if (pieceCaptured == Piece.BlackRook)
                        {
                            if (move.dst == Squares.A8) castlingRights.DisableQueenside(Colour.Black); // starting queenside rook position
                            if (move.dst == Squares.H8) castlingRights.DisableKingside (Colour.Black); // starting kingside rook position
                        }

                        // Update the captured piece's bitboard
                        bb = GetBitboardFromEnum(pieceCaptured);
                        bb ^= move.dst.Bitboard;
                        SetBitboardFromEnum(pieceCaptured, bb);

                        boardInfo.capturedPiece = pieceCaptured;
                    }

                    // Update board array
                    Mailbox[move.src] = Piece.Empty;
                    Mailbox[move.dst] = pieceToMove;

                    // Clear en-passant square
                    epSquare = 0;

                    break;

                case MoveType.EnPassant:
                    // Update zobrist hash
                    // After playing EP, there won't be another EP square,
                    // so we don't need to re-XOR to the hash.
                    if (boardHistory.TryPeek(out BoardInfo previousBoardInfo))
                    {
                        ZobristKey ^= Zobrist.HashEnPassant(previousBoardInfo.EPsquare);
                    }

                    // Update bitboard of piece
                    bb = GetBitboardFromEnum(pieceToMove);
                    bb ^= move.src.Bitboard | move.dst.Bitboard;
                    SetBitboardFromEnum(pieceToMove, bb);

                    // Get square of pawn to capture
                    Square inFrontOfEPsquare = move.dst - 8 * (move.src < move.dst ? 1 : -1);
                    
                    Piece opponentPawnType = Mailbox[inFrontOfEPsquare];
                    
                    // Remove pawn from bitboard and update it
                    Bitboard opponentPawnBB = GetBitboardFromEnum(opponentPawnType);
                    opponentPawnBB ^= inFrontOfEPsquare.Bitboard;
                    SetBitboardFromEnum(opponentPawnType, opponentPawnBB);

                    // Remove pawn from board array
                    Mailbox[move.src] = Piece.Empty;
                    Mailbox[move.dst] = pieceToMove;
                    Mailbox[inFrontOfEPsquare] = Piece.Empty;

                    // Clear EP square (already been used)
                    epSquare = 0;

                    break;
                
                case MoveType.PawnDoublePush:
                    // Get the square behind the pawn by getting the
                    // middle square between the start and end of the
                    // double pawn push.
                    int newEPsquare = (move.src + move.dst) / 2;
                    epSquare = new(newEPsquare);

                    // Update the zobrist hash with the new EP square
                    ZobristKey ^= Zobrist.HashEnPassant(epSquare);
                    
                    // Update piece bitboard
                    bb = GetBitboardFromEnum(pieceToMove);
                    bb ^= move.src.Bitboard | move.dst.Bitboard;
                    SetBitboardFromEnum(pieceToMove, bb);

                    // Update board array
                    Mailbox[move.src] = Piece.Empty;
                    Mailbox[move.dst] = pieceToMove;

                    break;
                
                case MoveType.Castling:
                    // Clear en-passant square
                    epSquare = 0;
                    
                    // Reset castling rights depending on side
                    castlingRights.DisableBoth(ColourToMove);

                    // Update zobrist hash with new castling rights
                    ZobristKey ^= Zobrist.HashCastlingRights(castlingRights);

                    // Update bitboard of king
                    bb = GetBitboardFromEnum(pieceToMove);
                    bb ^= move.src.Bitboard | move.dst.Bitboard;
                    SetBitboardFromEnum(pieceToMove, bb);

                    // Get position of rook to castle with on the board
                    Square rookSquare;
                    
                    // White castling squares
                         if (move.dst == Squares.C1) rookSquare = Squares.A1;
                    else if (move.dst == Squares.G1) rookSquare = Squares.H1;
                    
                    // Black castling squares 
                    else if (move.dst == Squares.C8) rookSquare = Squares.A8;
                    else if (move.dst == Squares.G8) rookSquare = Squares.H8;

                    else throw new Exception($"invalid castling destination square: {move.dst} (Move: {move})");

                    // Rook will always end up between the king's start and end square
                    Square endRookSquare = (move.src + move.dst) / 2;

                    // Get rook enum (used for obtaining bitboard)
                    Piece rookEnum = Piece.BlackRook - SideToMove;
                    
                    // Update bitboard of rook
                    Bitboard rookBB = GetBitboardFromEnum(rookEnum);
                    rookBB ^= rookSquare.Bitboard | endRookSquare.Bitboard;
                    SetBitboardFromEnum(rookEnum, rookBB);

                    // Update array
                    Mailbox[move.src] = Piece.Empty;
                    Mailbox[rookSquare] = Piece.Empty;
                    
                    Mailbox[move.dst] = pieceToMove;
                    Mailbox[endRookSquare] = rookEnum;

                    break;
                
                case MoveType.Promotion:
                    // Clear EP square
                    epSquare = 0;

                    // Move piece on array
                    Mailbox[move.src] = Piece.Empty;
                    
                    Piece promotedPiece = move.promoPiece switch
                    {
                        PromoPiece.Bishop => Piece.BlackBishop - SideToMove,
                        PromoPiece.Knight => Piece.BlackKnight - SideToMove,
                        PromoPiece.Rook   => Piece.BlackRook   - SideToMove,
                        PromoPiece.Queen  => Piece.BlackQueen  - SideToMove,
                        _ => throw new Exception($"promotion piece \"{move.promoPiece}\" unaccounted for.")
                    };

                    if (pieceCaptured != Piece.Empty)
                    {
                        if (pieceCaptured == Piece.WhiteRook)
                        {
                            if (move.dst == Squares.A1) castlingRights.DisableQueenside(Colour.White); // starting queenside rook position
                            if (move.dst == Squares.H1) castlingRights.DisableKingside(Colour.White); // starting kingside rook position
                        }

                        if (pieceCaptured == Piece.BlackRook)
                        {
                            if (move.dst == Squares.A8) castlingRights.DisableQueenside(Colour.Black); // starting queenside rook position
                            if (move.dst == Squares.H8) castlingRights.DisableKingside (Colour.Black); // starting kingside rook position
                        }

                        boardInfo.capturedPiece = pieceCaptured;

                        bb = GetBitboardFromEnum(pieceCaptured);
                        bb ^= move.dst.Bitboard;
                        SetBitboardFromEnum(pieceCaptured, bb);
                    }

                    Mailbox[move.dst] = promotedPiece;

                    // Update both sets of bitboards
                    bb = GetBitboardFromEnum(pieceToMove);
                    bb ^= move.src.Bitboard;
                    SetBitboardFromEnum(pieceToMove, bb);

                    bb = GetBitboardFromEnum(promotedPiece);
                    bb ^= move.dst.Bitboard;
                    SetBitboardFromEnum(promotedPiece, bb);

                    break;
                
                default: // move flag was unaccounted for
                    throw new Exception($"move flag \"{move.type}\" on move {move} unaccounted for.");
            }

            // If the move played was a promotion move (pawn was promoted) or a piece
            // on the board was captured, reset the halfmove clock.
            if (pieceToMove == Piece.WhitePawn
             || pieceToMove == Piece.BlackPawn
             || boardInfo.capturedPiece != Piece.Empty) halfMoveClock = 0;

            // Otherwise, increase the halfmove clock
            else halfMoveClock++;

            // Remove it from the zobrist hash
            /*
            if (moveHistory.TryPeek(out Move previousMove))
            {
                ZobristKey ^= Zobrist.HashPieceAndSquare(
                    previousMove.type == MoveType.Promotion
                        ? Piece.BlackPawn - SideToMove
                        : Mailbox[previousMove.dst],
                    previousMove.dst
                );
            }
            */

            // Add the newly moved piece to the zobrist hash
            ZobristKey ^= Zobrist.HashPieceAndSquare(pieceToMove, move.src);
            ZobristKey ^= Zobrist.HashPieceAndSquare(pieceToMove, move.dst);

            // Add the piece if captured - defaults to 0 internally if no piece
            // was captured this move.
            ZobristKey ^= Zobrist.HashPieceAndSquare(pieceCaptured, move.dst);

            // Remove the hash for the previous player
            // ZobristKey ^= Zobrist.HashColor((Colour)(SideToMove ^ 1));

            // Add the hash for the current player
            ZobristKey ^= Zobrist.HashColor(ColourToMove);

            // Add the hash to the list of past hashes
            PastZobristHashes.Push(ZobristKey);
            
            boardHistory.Push(boardInfo);
            
            UpdatePinsAndCheckers();
        }

        public void UndoMove()
        {
            PastZobristHashes.Pop();

            ZobristKey = PastZobristHashes.Peek();

            if (moveHistory.Count == 0) throw new Exception("cannot undo when no moves on the board have been played.");

            // Get last move and last board info
            Move previousMove = moveHistory.Pop();
            BoardInfo previousBoardInfo = boardHistory.Pop();

            checkmask       =  previousBoardInfo.checkmask;
            pinD            =  previousBoardInfo.pinD;
            pinHV           =  previousBoardInfo.pinHV;
            castlingRights  =  previousBoardInfo.castlingRights;
            epSquare        =  previousBoardInfo.EPsquare;
            halfMoveClock   =  previousBoardInfo.halfMoveClock;
            
            moveCounter--; // Decrease move counter

            Piece pieceThatMoved = Mailbox[previousMove.dst];

            // Edit board based on type of previous move
            switch (previousMove.type)
            {
                case MoveType.Normal:
                    // Update bitboard of piece
                    Bitboard bb = GetBitboardFromEnum(pieceThatMoved);

                    bb ^= previousMove.dst.Bitboard | previousMove.src.Bitboard;
                    SetBitboardFromEnum(pieceThatMoved, bb);

                    // Check if a piece was captured
                    // If so, update their bitboard as well
                    if (previousBoardInfo.capturedPiece != Piece.Empty)
                    {
                        bb = GetBitboardFromEnum(previousBoardInfo.capturedPiece);
                        bb ^= previousMove.dst.Bitboard;
                        SetBitboardFromEnum(previousBoardInfo.capturedPiece, bb);
                    }

                    Mailbox[previousMove.dst] = previousBoardInfo.capturedPiece;
                    Mailbox[previousMove.src] = pieceThatMoved;

                    break;

                case MoveType.EnPassant:
                    // Get whichever pawn type did the en-passant
                    Piece pawnType = Piece.WhitePawn + SideToMove;
                    
                    // Update bitboard of that piece
                    bb = GetBitboardFromEnum(pawnType);
                    bb ^= previousMove.dst.Bitboard | previousMove.src.Bitboard;
                    SetBitboardFromEnum(pawnType, bb);

                    Piece opponentPawnType = Piece.BlackPawn - SideToMove;

                    // Get the square in front of the EP square (relative to the side moving)
                    Square squarePawnWasTakenFrom = previousMove.dst + (SideToMove == 0 ? -8 : 8);

                    // Update bitboard of opponent pawn type
                    // (replace the pawn that was captured)
                    bb = GetBitboardFromEnum(opponentPawnType);
                    bb ^= squarePawnWasTakenFrom.Bitboard;
                    SetBitboardFromEnum(opponentPawnType, bb);

                    // Update board array
                    Mailbox[previousMove.src] = pawnType;
                    Mailbox[previousMove.dst] = Piece.Empty;
                    Mailbox[squarePawnWasTakenFrom] = opponentPawnType;

                    // En-passant square is previous move destination
                    epSquare = previousBoardInfo.EPsquare;

                    break;

                case MoveType.PawnDoublePush:
                    pawnType = Mailbox[previousMove.dst];

                    bb = GetBitboardFromEnum(pawnType);
                    bb ^= previousMove.dst.Bitboard | previousMove.src.Bitboard;
                    SetBitboardFromEnum(pawnType, bb);

                    // Reset en-passant square
                    epSquare = previousBoardInfo.EPsquare;

                    // Update board array
                    Mailbox[previousMove.src] = pawnType;
                    Mailbox[previousMove.dst] = Piece.Empty;

                    break;

                case MoveType.Castling:
                    Piece kingEnum = Piece.WhiteKing + SideToMove;

                    // Update king bitboard
                    bb = GetBitboardFromEnum(kingEnum);
                    bb ^= previousMove.dst.Bitboard | previousMove.src.Bitboard;
                    SetBitboardFromEnum(kingEnum, bb);

                    // Get rook position
                    // Rook position is always between the king's source
                    // and the king's destination square
                    Square rookSquare = (previousMove.src + previousMove.dst) / 2;

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
                    Square endRookSquare;

                    // White castling squares
                         if (previousMove.dst == Squares.C1) endRookSquare = Squares.A1;
                    else if (previousMove.dst == Squares.G1) endRookSquare = Squares.H1;
                    
                    // Black castling squares 
                    else if (previousMove.dst == Squares.G1) endRookSquare = Squares.H1;
                    else if (previousMove.dst == Squares.G1) endRookSquare = Squares.H1;

                    else throw new Exception("invalid rook position");
                    
                    Piece rookEnum = Piece.WhiteRook + SideToMove;

                    // Update rook bitboard
                    bb = GetBitboardFromEnum(rookEnum);
                    bb ^= rookSquare.Bitboard | endRookSquare.Bitboard;
                    SetBitboardFromEnum(rookEnum, bb);

                    // Reset castling rights
                    castlingRights = previousBoardInfo.castlingRights;

                    // Update board array
                    Mailbox[previousMove.dst] = Piece.Empty;
                    Mailbox[previousMove.src] = kingEnum;

                    Mailbox[rookSquare] = Piece.Empty;
                    Mailbox[endRookSquare] = rookEnum;

                    // Reset en-passant square
                    epSquare = previousBoardInfo.EPsquare;

                    break;

                case MoveType.Promotion:
                    // Reset en-passant square
                    epSquare = previousBoardInfo.EPsquare;

                    Piece promotedPiece = previousMove.promoPiece switch
                    {
                        PromoPiece.Bishop => Piece.WhiteBishop + SideToMove,
                        PromoPiece.Knight => Piece.WhiteKnight + SideToMove,
                        PromoPiece.Rook   => Piece.WhiteRook   + SideToMove,
                        PromoPiece.Queen  => Piece.WhiteQueen  + SideToMove,
                        _ => throw new Exception($"promotion piece \"{previousMove.promoPiece}\" unaccounted for.")
                    };

                    pawnType = Piece.WhitePawn + SideToMove;

                    if (previousBoardInfo.capturedPiece != Piece.Empty)
                    {
                        bb = GetBitboardFromEnum(previousBoardInfo.capturedPiece);
                        bb ^= previousMove.dst.Bitboard;
                        SetBitboardFromEnum(previousBoardInfo.capturedPiece, bb);
                    }

                    // Update board array
                    Mailbox[previousMove.src] = pawnType;
                    Mailbox[previousMove.dst] = previousBoardInfo.capturedPiece;

                    // Update bitboards
                    bb = GetBitboardFromEnum(pawnType);
                    bb ^= previousMove.src.Bitboard;
                    SetBitboardFromEnum(pawnType, bb);

                    bb = GetBitboardFromEnum(promotedPiece);
                    bb ^= previousMove.dst.Bitboard;
                    SetBitboardFromEnum(promotedPiece, bb);

                    break;

                default:
                    throw new Exception($"move flag \"{previousMove.type}\" on move {previousMove} unaccounted for.");
            }

            UpdatePinsAndCheckers();
        }

        public List<Move> GenerateLegalMoves()
        {
            List<Move> moves = [];

            // If there are two checkers, only generate king moves
            if (checkers.BitCount == 2)
            {
                Moves.GenerateKingMoves(
                    friendlyPieces: PlayerToMove,
                    opponentPieces: OpponentToMove,
                    square: PlayerToMove.KingSquare,
                    moveListToAddTo: moves
                );
                
                return moves;
            }

            Bitboard rooksQueens = PlayerToMove.Queens | PlayerToMove.Rooks;

            while (rooksQueens)
            {
                Moves.GenerateRookMoves(
                    friendlyOccupancy: PlayerToMove.Mask,
                    opponentOccupancy: OpponentToMove.Mask,
                    square: rooksQueens.PopLSB(),
                    pinHV: pinHV,
                    pinD: pinD,
                    checkmask: checkmask,
                    moveListToAddTo: moves
                );
            }

            Bitboard bishopsQueens = PlayerToMove.Bishops | PlayerToMove.Queens;

            while (bishopsQueens)
            {
                Moves.GenerateBishopMoves(
                    friendlyOccupancy: PlayerToMove.Mask,
                    opponentOccupancy: OpponentToMove.Mask,
                    square: bishopsQueens.PopLSB(),
                    pinHV: pinHV,
                    pinD: pinD,
                    checkmask: checkmask,
                    moveListToAddTo: moves
                );
            }

            Bitboard knights = PlayerToMove.Knights;

            while (knights)
            {
                Moves.GenerateKnightMoves(
                    friendlyOccupancy: PlayerToMove.Mask,
                    square: knights.PopLSB(),
                    moveListToAddTo: moves,
                    pinmask: pinD | pinHV,
                    checkmask: checkmask
                );
            }

            Moves.GeneratePawnMoves(
                whitePieces: White,
                blackPieces: Black,
                epSquare: epSquare,
                moveList: moves,
                pinHV: pinHV,
                pinD: pinD,
                checkmask: checkmask,
                side: ColourToMove
            );

            Moves.GenerateKingMoves(
                friendlyPieces: PlayerToMove,
                opponentPieces: OpponentToMove,
                square: PlayerToMove.KingSquare,
                moveListToAddTo: moves
            );

            if (checkers.BitCount == 0) // not in check
            {
                Moves.GenerateCastlingMoves(
                    sideToMove: ColourToMove,
                    friendlyPieces: PlayerToMove,
                    opponentPieces: OpponentToMove,
                    castlingRights: castlingRights,
                    moveListToAddTo: moves
                );
            }

            return moves;
        }


        public void UpdatePinsAndCheckers()
        {
            PieceSet us = PlayerToMove;
            PieceSet them = OpponentToMove;

            Bitboard knightCheckers = Bitmask.ForKnight(0, us.KingSquare) & them.Knights;

            // Set directions for generating pawn checkers
            Direction upLeft, upRight;

            if (us.colour == Colour.White)
            {
                upLeft  = Direction.Northwest;
                upRight = Direction.Northeast;
            }
            else
            {
                upLeft  = Direction.Southeast;
                upRight = Direction.Southwest;
            }

            // Manually generate pawn checkers instead of using a function because the function
            // accounts for taking pieces instead of simply attacking squares, meaning we can't
            // use it for this purpose.
            Bitboard pawnCheckers = them.Pawns & (us.King.Shift(upLeft) | us.King.Shift(upRight));

            // For rooks, queens and bishops
            Bitboard queens = GetBitboardFromEnum(Piece.BlackQueen - SideToMove);

            Bitboard bishopsQueens = GetBitboardFromEnum(Piece.BlackBishop - SideToMove) ^ queens;
            Bitboard rooksQueens   = GetBitboardFromEnum(Piece.BlackRook   - SideToMove) ^ queens;

            Bitboard bishopAttacks = bishopsQueens & Bitmask.ForBishop(them.Mask, us.KingSquare);
            Bitboard rookAttacks = rooksQueens & Bitmask.ForRook(them.Mask, us.KingSquare);
            
            checkers = pawnCheckers | knightCheckers;
            checkmask = pawnCheckers | knightCheckers;
            
            pinD = 0;
            pinHV = 0;
            
            while (bishopAttacks)
            {
                Square sq = bishopAttacks.PopLSB();

                Bitboard checkray = Bitmask.RayBetween(us.KingSquare, sq);
                Bitboard blockers = checkray & us.Mask;
                int numBlockers = blockers.BitCount;

                if (numBlockers == 0)
                {
                    checkmask |= checkray | sq.Bitboard;
                    checkers |= sq.Bitboard;
                }
                else if (numBlockers == 1)
                {
                    pinD |= checkray | sq.Bitboard | blockers;
                }
            }

            while (rookAttacks)
            {
                Square sq = rookAttacks.PopLSB();

                Bitboard checkray = Bitmask.RayBetween(us.KingSquare, sq);
                Bitboard blockers = checkray & us.Mask;
                int numBlockers = blockers.BitCount;

                if (numBlockers == 0)
                {
                    checkmask |= checkray | sq.Bitboard;
                    checkers |= sq.Bitboard;
                }
                else if (numBlockers == 1)
                {
                    pinHV |= checkray | sq.Bitboard | blockers;
                }
            }

            if (!checkmask) checkmask = Bitboard.Filled;
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

                    char stringPiece = Mailbox[index] switch
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

        public bool ViolatedRepetitionRule()
        {
            Dictionary<ulong, int> pastHashes = [];

            foreach (ulong pastHash in PastZobristHashes)
            {
                if (!pastHashes.TryAdd(pastHash, 1))
                {
                    // If we have found 2 repetitions already, this
                    // third one means we have violated the three-
                    // fold repetition rule.
                    if (pastHashes[pastHash]++ == 2) return true;
                }
            }

            return false;
        }

        public bool Violated50MoveRule() => halfMoveClock > 100;

        private string GetFEN()
        {
            string[] linesOfFEN = new string[8];
            int emptySpaces = 0;

            // Encode the board into a FEN string
            for (int rank = 7; rank != -1; rank--)
            {
                string line = "";

                for (int file = 0; file < 8; file++)
                {
                    int index = rank * 8 + file;

                    if (Mailbox[index] == Piece.Empty)
                    {
                        emptySpaces++;
                    }
                    else
                    {
                        if (emptySpaces > 0)
                        {
                            line += $"{emptySpaces}";
                            emptySpaces = 0;
                        }
                        
                        line += Mailbox[index] switch
                        {
                            Piece.WhiteBishop => 'B',
                            Piece.BlackBishop => 'b',
                            Piece.WhiteKnight => 'N',
                            Piece.BlackKnight => 'n',
                            Piece.WhiteRook   => 'R',
                            Piece.BlackRook   => 'r',
                            Piece.WhitePawn   => 'P',
                            Piece.BlackPawn   => 'p',
                            Piece.WhiteQueen  => 'Q',
                            Piece.BlackQueen  => 'q',
                            Piece.WhiteKing   => 'K',
                            Piece.BlackKing   => 'k',
                            
                            _ => throw new Exception($"piece enum {Mailbox[index]} unaccounted for while constructing FEN string.")
                        };
                    }
                }

                if (emptySpaces > 0)
                {
                    line += $"{emptySpaces}";
                    emptySpaces = 0;
                }

                linesOfFEN[rank] = line;
            }

            string FEN = string.Join("/", linesOfFEN.Reverse());

            // Add all the other properties of the FEN string.
            // Half-move clock is treated as a 100-move clock so that's
            // why we need to halve it before adding it to the FEN string.
            FEN += $" {(ColourToMove == Colour.White ? "w" : "b")} {castlingRights} {epSquare.ToString().ToLower()} {halfMoveClock / 2} {(moveCounter - SideToMove) / 2}";
            
            return FEN;
        }
    }
}