using System.Text.RegularExpressions;

namespace Chess.PGN
{
    public struct PGN
    {
        public static List<string> ReadPGNFile(string filePath)
        {
            string rawPGN = File.ReadAllText(filePath);

            // This regex maps to the start of a line (begins with a newline)
            Regex startRegex = new(@"^1\.", RegexOptions.Multiline);
            Regex endRegex   = new(@"[^\""](1\/2-1\/2)|(0-1)|(1-0)");

            List<int> startPositions = startRegex
                .Matches(rawPGN)
                .Select(m => m.Index)
                .ToList();
            
            List<int> endPositions = endRegex
                .Matches(rawPGN)
                .Select(m => m.Index + m.Length)
                .ToList();
            
            if (startPositions.Count != endPositions.Count)
            {
                throw new Exception($"found {startPositions.Count} starts but only {endPositions.Count} ends - unequal.");
            }

            List<string> extractedPGNs = [];
            
            for (int i = 0; i < startPositions.Count; i++)
            {
                int start = startPositions[i];
                int end   =   endPositions[i];

                extractedPGNs.Add(rawPGN[start..end].Replace('\n', ' '));
            }

            return extractedPGNs;
        }

        public static Board InterpretPGN(string PGN)
        {
            Board board = new();

            Regex moveRegex = new(@"((O-O-O)|(O-O))|(([a-h]x)?[a-h][1-8](=[QNBR])?[#\+]?)|([BNQRKbnqrk][a-h]?[1-8]?x?[a-h][1-8][#\+]?)");

            // Go through all of the matches in the regex.
            foreach (Match match in moveRegex.Matches(PGN))
            {
                board.MakeMove(ParseSAN(board, match.Value));
            }

            return board;
        }

        public static string ConvertToPGN(Board board)
        {
            string result;

            List<Move> moves = board.GenerateLegalMoves();

            // Win for white: 1-0
            // Win for black: 0-1
            // Draw for both: 1/2-1/2
            // Game not done: *
            if (board.IsDraw || (moves.Count == 0 && !board.InCheck))
            {
                result = "1/2-1/2";
            }
            else
            {
                if (moves.Count == 0 && board.InCheck)
                {
                    // Since the checkmate move passes the play onto the other
                    // person, we need to invert the side-to-move to get the
                    // side that played the checkmate.
                    result = (board.SideToMove ^ 1) == 0 ? "1-0" : "0-1";
                }
                else
                {
                    result = "*";
                }
            }

            string PGN = "[Site \"?\"]\n[Event \"?\"]\n[White \"?\"]\n[Black \"?\"]\n";

            if (board.OriginalFEN != Perft.Perft.Startpos)
            {
                PGN += $"[FEN \"{board.OriginalFEN}\"]\n";
            }

            PGN += $"[Result \"{result}\"]\n";
            
            Board backtrackBoard = new();
            List<Move> movesToRetrace = board.moveHistory.Reverse().ToList();

            for (int i = 0; i < movesToRetrace.Count; i++)
            {
                // If this is white's turn to move, add a number
                if (backtrackBoard.SideToMove == 0)
                {
                    PGN += $"{i / 2 + 1}.";
                }

                PGN += MoveToSAN(backtrackBoard, movesToRetrace[i]) + ' ';
            }

            if (result != "*")
            {
                PGN += result;
            }

            return PGN;
        }

        public static string MoveToSAN(Board board, Move move)
        {
            if (move.type == MoveType.Castling)
            {
                return move.src < move.dst ? "O-O" : "O-O-O";
            }
            
            Piece pieceToMove = board.Mailbox[move.src];
            
            if (pieceToMove == Piece.Empty)
            {
                throw new Exception("cannot parse a move to SAN that involves starting on an empty square.");
            }
            
            board.MakeMove(move);
                
            bool inCheck = board.InCheck;
            bool noMoves = board.GenerateLegalMoves().Count == 0;
            bool isCheckmate = inCheck && noMoves;
                
            board.UndoMove();
            
            string moveString;
            
            bool moveWasCapture = board.Mailbox[move.dst] != Piece.Empty;
            
            // If it was a pawn move
            if (pieceToMove - board.SideToMove == Piece.WhitePawn)
            {
                moveString = moveWasCapture || move.type == MoveType.EnPassant ? $"{(char)('a' + move.src.File)}x" : "";
                
                moveString += $"{move.dst}";
                
                if (move.type == MoveType.Promotion)
                {
                    moveString += "=" + move.promoPiece switch
                    {
                        PromoPiece.Bishop => 'B',
                        PromoPiece.Knight => 'N',
                        PromoPiece.Rook   => 'R',
                        PromoPiece.Queen  => 'Q',
                            
                        _ => throw new Exception($"unrecognised promotion piece '{move.promoPiece}'")
                    };
                }
                
                moveString += isCheckmate ? '#' : inCheck ? '+' : "";
            }
            
            // If it was a normal piece move
            else
            {
                // Add the piece character
                moveString = (pieceToMove - board.SideToMove) switch
                {
                    Piece.WhiteBishop => "B",
                    Piece.WhiteKnight => "N",
                    Piece.WhiteRook   => "R",
                    Piece.WhiteQueen  => "Q",
                    Piece.WhiteKing   => "K",
                        
                    _ => throw new Exception($"cannot interpret piece '{pieceToMove - board.SideToMove}'")
                };
                
                List<Move> moveLookup = board
                    .GenerateLegalMoves()
                    .Where(
                        m => m.dst == move.dst
                        && board.Mailbox[m.src] == pieceToMove
                    )
                    .ToList();
                
                if (moveLookup.Count > 1)
                {
                    // Only file differentiation needed
                    if (moveLookup.Where(m => m.src.File == move.src.File).ToList().Count == 1)
                    {
                        moveString += (char)('a' + move.src.File);
                    }
                    
                    // Only rank differentiation needed
                    else if (moveLookup.Where(m => m.src.Rank == move.src.Rank).ToList().Count == 1)
                    {
                        moveString += (char)('1' + move.src.Rank);
                    }
                    
                    // Exact square differentiation needed
                    else
                    {
                        moveString += $"{move.src}";
                    }
                }
                
                if (moveWasCapture) moveString += 'x';
                
                moveString += $"{move.dst}";
                
                moveString += isCheckmate ? '#' : inCheck ? '+' : "";
            }
            
            return moveString;
        }

        public static Move ParseSAN(Board board, string move)
        {
            List<Move> moveLookup = board
                .GenerateLegalMoves()
                .Where(m => MoveToSAN(board, m) == move)
                .ToList();
            
            switch (moveLookup.Count)
            {
                case 0:   throw new Exception($"cannot find a move matching the SAN '{move}'.");

                case 1:   return moveLookup[0];
                
                default:  throw new Exception($"found multiple contenders for the SAN '{move}': [{string.Join(", ", moveLookup)}]");
            }
        }
    }
}