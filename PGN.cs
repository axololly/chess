using Chess.MoveGen;
using Chess.Types.Squares;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Chess.PGN
{
    public struct PGN
    {
        public static List<string> ReadPGNFile(string filePath)
        {
            string rawPGN = File.ReadAllText(filePath);

            // This regex maps to the start of a line (begins with a newline)
            Regex startRegex = new(@"^1\.", RegexOptions.Multiline);
            Regex endRegex   = new(@"  ((1\/2-1\/2)|(0-1)|(1-0))");

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
            List<string> pastFENs = [];

            Regex regex = new(@"((O-O-O)|(O-O))|(([a-h]x)?[a-h][1-8](=[QNBR])?[#\+]?)|([BNQRKbnqrk][a-h]?[1-8]?x?[a-h][1-8][#\+]?)");

            Regex differentiationRegex = new(@"([a-h][1-8])|([a-h])|([1-8])");
            Regex promotionRegex = new(@"=[QBNR]");

            int translatedMatches = 0;

            // Go through all of the matches in the regex.
            foreach (Match match in regex.Matches(PGN))
            {
                pastFENs.Add(board.FEN);

                translatedMatches++;

                string move = match.Value;

                List<Move> LegalMoves = board.GenerateLegalMoves();
                
                // If the move is a castling move
                if (move.StartsWith("O-"))
                {
                    // Try and find the castling move, raising an error if it's not found
                    var castlingMoveLookup = LegalMoves
                        .Where(
                            m => m.type == MoveType.Castling
                                  // If the move is short castle, check for moves
                                  // where the start is before the end. Otherwise,
                                  // check for moves where the start is after the end.
                              && (move == "O-O" ? m.src < m.dst : m.src > m.dst)
                        )
                        .ToList();

                    if (castlingMoveLookup.Count == 0) throw new Exception($"invalid castling move '{move}' - castling cannot be played in this position.\n\nBoard:\n{board}\n\nCastling rights: {board.castlingRights}");

                    Move castlingMoveFound = castlingMoveLookup.First();

                    // Make the move and move on
                    board.MakeMove(castlingMoveFound);
                    continue;
                }

                // If this is a piece move...
                if (char.IsUpper(move[0]))
                {
                    // Get the square the move says it will end up.
                    // Make sure we shift our slice, depending on whether
                    // or not the move contains game-centric characters (+ and #).
                    Square destinationSquare = new(move[^1] == '+' || move[^1] == '#' ? move[^3..^1] : move[^2..]);

                    // Get the enum representing the character
                    Piece pieceType = move[0] switch
                    {
                        'B' => Piece.WhiteBishop + board.SideToMove,
                        'N' => Piece.WhiteKnight + board.SideToMove,
                        'R' => Piece.WhiteRook 	 + board.SideToMove,
                        'Q' => Piece.WhiteQueen  + board.SideToMove,
                        'K' => Piece.WhiteKing   + board.SideToMove,

                        _ => throw new Exception($"invalid character '{move[0]}' in move string \"{move}\".\nBoard:\n{board}")
                    };

                    List<Move> contenders = LegalMoves
                        .Where(
                            m => board.Mailbox[m.src] == pieceType
                            && m.dst == destinationSquare
                            )
                        .ToList();

                    if (contenders.Count == 0)
                    {
                        throw new Exception($"no contenders for destination square '{destinationSquare}' and piece type '{pieceType}'.\n\nBoard:\n{board}\n\nMoves: [{string.Join(", ", LegalMoves)}]\n");
                    }        
                    else if (contenders.Count == 1)
                    {
                        board.MakeMove(contenders[0]);
                    }
                    else
                    {
                        // Needs differentiation.
                        string differentiation = differentiationRegex
                            .Matches(move)
                            .First()
                            .Value;
                        
                        // Exact square differentiation
                        if (differentiation.Length == 2)
                        {
                            Square startSquare = new(differentiation);
                            
                            List<Move> moveLookup = LegalMoves
                                .Where(
                                    m => board.Mailbox[m.src] == pieceType
                                    && m.dst == startSquare
                                )
                                .ToList();
                            
                            if (moveLookup.Count == 0) throw new Exception($"illegal move '{move}' found during exact square differentiation.");
                            
                            board.MakeMove(moveLookup[0]);
                        }
                        
                        // Rank differentiation
                        else if (char.IsAsciiDigit(differentiation[0]))
                        {
                            int targetRank = differentiation[0] - '1';
                            
                            List<Move> moveLookup = LegalMoves
                                .Where(
                                    m => board.Mailbox[m.src] == pieceType
                                    && m.src.Rank == targetRank
                                    && m.dst == destinationSquare
                                )
                                .ToList();
                            
                            if (moveLookup.Count == 0)
                            {
                                throw new Exception($"illegal move '{move}' found during rank differentiation.");
                            }
                            else if (moveLookup.Count == 1)
                            {
                                board.MakeMove(moveLookup[0]);
                            }
                            else
                            {
                                throw new Exception($"multiple contenders found during rank differentiation - [{string.Join(", ", moveLookup)}]");
                            }
                        }
                        
                        // File differentiation
                        else
                        {
                            int targetFile = differentiation[0] - 'a';
                            
                            List<Move> moveLookup = LegalMoves
                                .Where(
                                    m => board.Mailbox[m.src] == pieceType
                                    && m.src.File == targetFile
                                    && m.dst == destinationSquare
                                )
                                .ToList();
                            
                            if (moveLookup.Count == 0)
                            {
                                throw new Exception($"illegal move '{move}' found during file differentiation.");
                            }
                            else if (moveLookup.Count == 1)
                            {
                                board.MakeMove(moveLookup[0]);
                            }
                            else
                            {
                                throw new Exception($"multiple contenders found during rank differentiation - [{string.Join(", ", moveLookup)}]");
                            }
                        }
                    }
                }
                
                // If this is a pawn move
                else
                {
                    Piece pawnType = Piece.WhitePawn + board.SideToMove;
                    
                    // Check if the move is a capture, in which the destination square will be
                    // after the capture. If the move is not a capture, the destination square
                    // will be at the start of the move string.
                    Square destinationSquare = new(move[1] == 'x' ? move[2..4] : move[..2]);

                    // Lookup the move based purely on the destination square
                    List<Move> moveLookup = LegalMoves
                        .Where(
                            m => board.Mailbox[m.src] == pawnType
                            && m.dst == destinationSquare
                        )
                        .ToList();

                    // Add an extra filter if the move was a capture or not
                    if (move[1] == 'x')
                    {
                        int targetFile = move[0] - 'a';

                        moveLookup = moveLookup
                            .Where(m => m.src.File == targetFile)
                            .ToList();
                    }
                    
                    string promotionString = promotionRegex.Match(move).Value;

                    // Add an extra filter if the move is promotional or not
                    if (promotionString != "")
                    {
                        PromoPiece identifiedPromoPiece = promotionString[^1] switch
                        {
                            'Q' => PromoPiece.Queen,
                            'R' => PromoPiece.Rook,
                            'N' => PromoPiece.Knight,
                            'B' => PromoPiece.Bishop,

                            _ => throw new Exception($"invalid promotion character '{promotionString[^1]}' found.")
                        };

                        moveLookup = moveLookup
                            .Where(
                                m => m.type == MoveType.Promotion
                                && m.promoPiece == identifiedPromoPiece
                            )
                            .ToList();
                    }

                    if (moveLookup.Count == 0)
                    {
                        throw new Exception(
                            $"no contenders were found for move '{move}' with destination square '{destinationSquare}' and pawn type '{pawnType}'."
                        );
                    }
                    else if (moveLookup.Count == 1)
                    {
                        board.MakeMove(moveLookup[0]);
                    }
                    else
                    {
                        throw new Exception(
                            $"multiple contenders were found for move '{move}'.\n\nFound these moves: [{string.Join(", ", moveLookup)}]\n"
                        );
                    }
                }
            }

            return board;
        }
    }
}