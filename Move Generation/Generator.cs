using Types.Bitboards;

using Chess.Utilities;
using Chess.Castling;
using Chess.Bitmasks;
using Chess.Tables;

using Chess960;

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Types.Squares;

namespace Chess.MoveGen
{
    public enum MoveType
    {
        Normal,
        PawnDoublePush,
        Castling,
        Castling960,
        EnPassant,
        Promotion
    }

    public enum PromoPiece
    {
        None,
        Knight,
        Bishop,
        Rook,
        Queen
    }

    public struct Move // TODO: find out what needs Move to be partial
    {
        public Square src; // Where the move starts
        public Square dst; // Where the move ends
        public MoveType type = MoveType.Normal; // Type of move
        public PromoPiece promoPiece = PromoPiece.None;

        public Move(
            int src,
            int dst,
            MoveType type = MoveType.Normal,
            PromoPiece promoPiece = PromoPiece.None
        )
        {
            this.src = src;
            this.dst = dst;
            this.type = type;
            this.promoPiece = promoPiece;
        }

        public static Move FromString(string moveString, MoveType type = MoveType.Normal, PromoPiece promoPiece = PromoPiece.None)
        {
            if (
                (type != MoveType.Promotion && promoPiece != PromoPiece.None)
             || (type == MoveType.Promotion && promoPiece == PromoPiece.None)
            )
            {
                throw new Exception("promoted piece and promotion flag must be set together.");
            }

            Regex regex = new("^[a-h][1-8][a-h][1-8][qbrn]?$");
            string match = regex.Match(moveString).Value;

            static int convert(string value) => (value[1] - '1') * 8 + value[0] - 'a';

            if (match.Length == 5) // promotion move present
            {
                return new Move()
                {
                    src = convert(match[..2]),
                    dst = convert(match.Substring(2, 2)),
                    type = MoveType.Promotion,
                    promoPiece = match[4] switch
                    {
                        'r' => PromoPiece.Rook,
                        'b' => PromoPiece.Bishop,
                        'n' => PromoPiece.Knight,
                        'q' => PromoPiece.Queen,
                        
                        _ => throw new Exception("error when deconstructing promotion piece type.")
                    }
                };
            }

            Move move = new()
            {
                src = convert(match[..2]),
                dst = convert(match.Substring(2, 2)),
                promoPiece = promoPiece,
                type = type
            };

            return move;
        }

        public override string ToString()
        {
            string convert(int index)
            {
                if (index < 0 || index > 63)
                {
                    throw new Exception($"square index '{index}' cannot be translated to a square value.");
                }

                return "abcdefgh"[index % 8].ToString() + "12345678"[index / 8].ToString();
            }

            string promoPieceString = promoPiece switch
            {
                PromoPiece.Bishop => "b",
                PromoPiece.Knight => "n",
                PromoPiece.Rook   => "r",
                PromoPiece.Queen  => "q",
                PromoPiece.None   => "",
                _ => throw new Exception("promotion piece enum unaccounted for.")
            };

            return convert(src) + convert(dst) + promoPieceString;
        }

        public override bool Equals([NotNullWhen(true)] object? other)
        {
            if (other is not Move) return false;

            Move otherMove = (Move)other;
            
            return src == otherMove.src
                && dst == otherMove.dst
                && type == otherMove.type
                && promoPiece == otherMove.promoPiece;
        }

        public override int GetHashCode()
        {
            Tuple<int, int> moveTuple = new(src, dst);
            return moveTuple.GetHashCode();
        }
        
        public static bool operator ==(Move left, Move right) =>  left.Equals(right);
        public static bool operator !=(Move left, Move right) => !left.Equals(right);
    }

    public struct Moves
    {
        private static MovementTables mt = new();

        public static void GenerateMovesFromSameSquare(Bitboard moveBitmask, int startSquare, List<Move> moveListToAddTo)
        {
            while (moveBitmask)
            {
                Move move = new()
                {
                    src = startSquare,
                    dst = moveBitmask.PopLSB(),
                    type = MoveType.Normal
                };

                moveListToAddTo.Add(move);
            }
        }

        public static void GenerateMovesWithOffset(Bitboard moveBitmask, int offset, List<Move> moveListToAddTo, MoveType type = MoveType.Normal)
        {
            if (type == MoveType.Promotion)
            {
                PromoPiece[] promoPieces = [
                    PromoPiece.Bishop,
                    PromoPiece.Knight,
                    PromoPiece.Rook,
                    PromoPiece.Queen
                ];

                while (moveBitmask)
                {
                    int sq = moveBitmask.PopLSB();

                    // Add all 4 promo pieces as moves to move list
                    for (int i = 0; i < 4; i++)
                    {
                        moveListToAddTo.Add(
                            new Move()
                            {
                                src = sq - offset,
                                dst = sq,
                                type = type,
                                promoPiece = promoPieces[i]
                            }
                        );
                    }
                }
            }
            else
            {
                while (moveBitmask)
                {
                    int sq = moveBitmask.PopLSB();

                    Move move = new()
                    {
                        src = sq - offset,
                        dst = sq,
                        type = type
                    };

                    moveListToAddTo.Add(move);
                }
            }
        }


        public static void GenerateRookMoves(
            Bitboard friendlyOccupancy,
            Bitboard opponentOccupancy,
            Bitboard pinD,
            Bitboard pinHV,
            Bitboard checkmask,
            int square,
            List<Move> moveListToAddTo
        )
        {
            Bitboard moveBitmask = Bitmask.ForRook(friendlyOccupancy | opponentOccupancy, square) & ~friendlyOccupancy;

            moveBitmask &= checkmask;
            
            // Cannot move if on diagonal pinmask
            if (pinD >> square & 1) return;
            
            // Can move on an orthogonal pinmask, mask moves against pinmask
            if (pinHV >> square & 1) moveBitmask &= pinHV;
            
            GenerateMovesFromSameSquare(moveBitmask, square, moveListToAddTo);
        }

        public static void GenerateBishopMoves(
            Bitboard friendlyOccupancy,
            Bitboard opponentOccupancy,
            Bitboard pinD,
            Bitboard pinHV,
            Bitboard checkmask,
            int square,
            List<Move> moveListToAddTo
        )
        {
            Bitboard moveBitmask = Bitmask.ForBishop(friendlyOccupancy | opponentOccupancy, square) & ~friendlyOccupancy;

            moveBitmask &= checkmask;
            
            // Cannot move if on orthogonal pinmask
            if (pinHV >> square & 1) return;
            
            // Can move on a diagonal pinmask, mask moves against pinmask
            if (pinD >> square & 1) moveBitmask &= pinD;
            
            GenerateMovesFromSameSquare(moveBitmask, square, moveListToAddTo);
        }

        public static void GenerateKingMoves(
            PieceSet friendlyPieces,
            PieceSet opponentPieces,
            int square,
            List<Move> moveListToAddTo
        )
        {
            Bitboard boardMask = friendlyPieces.Mask | opponentPieces.Mask;
            
            // Exclude opponent attack rays and protected pieces (combined)
            Bitboard opponentAttacks = opponentPieces.BaseAttackingBitmask(boardMask ^ friendlyPieces.King);

            Bitboard moveBitmask = Bitmask.ForKing(
                enemyOrEmpty: ~friendlyPieces.Mask,
                opponentAttacks: opponentAttacks,
                square: square
            );

            GenerateMovesFromSameSquare(moveBitmask, square, moveListToAddTo);
        }

        

        public static void GenerateKnightMoves(
            Bitboard friendlyOccupancy,
            int square,
            Bitboard pinmask,
            Bitboard checkmask,
            List<Move> moveListToAddTo
        )
        {
            if (pinmask & 1UL << square) return; // if knight is pinned, it can't move anywhere

            GenerateMovesFromSameSquare(
                Bitmask.ForKnight(friendlyOccupancy, square) & checkmask,
                square,
                moveListToAddTo
            );
        }


        public static void GeneratePawnMoves(
            Colour side,
            PieceSet whitePieces,
            PieceSet blackPieces,
            Square epSquare,
            Bitboard checkmask,
            Bitboard pinHV,
            Bitboard pinD,
            List<Move> moveList
        )
        {
            PieceSet us, them;
            Direction up, upLeft, upRight, downLeft, downRight;
            Bitboard promotionRank, doublePushRank;
            int[] pawnShifts;
            
            if (side == Colour.White)
            {
                us   = whitePieces;
                them = blackPieces;

                up      = Direction.North;
                upLeft  = Direction.Northwest;
                upRight = Direction.Northeast;

                downLeft = Direction.Southwest;
                downRight = Direction.Southeast;

                promotionRank  = Ranks.Eighth;
                doublePushRank = Ranks.Third;

                pawnShifts = [7, 8, 9, 16];
            }
            else
            {
                us   = blackPieces;
                them = whitePieces;

                up      = Direction.South;
                upLeft  = Direction.Southwest;
                upRight = Direction.Southeast;

                downLeft  = Direction.Northwest;
                downRight = Direction.Northeast;

                promotionRank  = Ranks.First;
                doublePushRank = Ranks.Sixth;

                pawnShifts = [-9, -8, -7, -16];
            }

            Bitboard pawns = us.Pawns;
            Bitboard empty = ~(us.Mask | them.Mask);
            Bitboard enemy = them.Mask;

            Bitboard unpinnedPawns = pawns & ~pinD  & ~pinHV;
            Bitboard pinnedHVpawns = pawns & ~pinD  &  pinHV;
            Bitboard pinnedDpawns  = pawns & ~pinHV &  pinD;

            // Pawns pinned diagonally can't move forward and pawns pinned
            // orthogonally have to be restricted to only move on the pinmask.
            Bitboard singlePushes = empty & (unpinnedPawns.Shift(up) | pinnedHVpawns.Shift(up) & pinHV);

            // Only pawns (after being pushed) on the (relative) third rank
            // would have gone from the (relative) second rank, so only shift
            // those upwards and check if there are any spaces in the way.
            //
            // The third rank is also the en-passant rank, so we can reuse
            // that here.
            Bitboard doublePushes = empty & (singlePushes & doublePushRank).Shift(up);

            // Add moves to move list - doing single pushes
            // (Add to checkmask first so that double pushes
            // can be used to block checks)
            GenerateMovesWithOffset(checkmask & singlePushes & ~promotionRank, pawnShifts[1], moveList);
            GenerateMovesWithOffset(checkmask & singlePushes &  promotionRank, pawnShifts[1], moveList, type: MoveType.Promotion);

            // Add moves to move list - doing double pushes (at start)
            GenerateMovesWithOffset(checkmask & doublePushes, pawnShifts[3], moveList, type: MoveType.PawnDoublePush);

            // Pawns pinned orthogonally can't capture pieces and pawns pinned
            // diagonally must only be able to take pieces on the pinmask.
            Bitboard leftAttacks  = checkmask & enemy & (unpinnedPawns.Shift(upLeft)  | pinnedDpawns.Shift(upLeft)  & pinD);
            Bitboard rightAttacks = checkmask & enemy & (unpinnedPawns.Shift(upRight) | pinnedDpawns.Shift(upRight) & pinD);

            // Add moves to move list - doing left attacks
            GenerateMovesWithOffset(leftAttacks & ~promotionRank, pawnShifts[0], moveList);
            GenerateMovesWithOffset(leftAttacks &  promotionRank, pawnShifts[0], moveList, type: MoveType.Promotion);

            // Add moves to move list - doing right attacks
            GenerateMovesWithOffset(rightAttacks & ~promotionRank, pawnShifts[2], moveList);
            GenerateMovesWithOffset(rightAttacks &  promotionRank, pawnShifts[2], moveList, type: MoveType.Promotion);

            // -----------------------------------------------------------------------------------------------------------------

            // If there's no en-passant square, return here
            if (!epSquare) return;

            int epPawn   = epSquare - pawnShifts[1];
            
            Bitboard epMask = epSquare.Bitboard | 1UL << epPawn;

            // If the en-passant square and the enemy pawn are not on
            // the checkmask, then en-passant is not available.
            if (!(checkmask & epMask)) return;

            int kingSquare = us.KingSquare;
            Bitboard kingMask = us.King & Ranks.ContainsPosition(epPawn);
            
            Bitboard enemyQueenRook = them.Queens | them.Rooks;

            bool isPossiblePin = !kingMask.IsEmpty && !enemyQueenRook.IsEmpty;

            // Pawns pinned orthogonally cannot take any pieces because
            // they would leave their pinmask.
            Bitboard pawnsAttackingEP = pawns & ~pinnedHVpawns & (
                                        epSquare.Bitboard.Shift(downLeft)
                                      | epSquare.Bitboard.Shift(downRight)
                                    );

            while (pawnsAttackingEP)
            {
                int from = pawnsAttackingEP.PopLSB();
                int to = epSquare;
                
                // If the pawn is pinned but the en-passant square is
                // not in the pinmask, the move is illegal, so skip it.
                if (1UL << from & pinD && !(pinD & 1UL << epSquare)) continue;

                Bitboard connectingPawns = 1UL << epPawn | 1UL << from;

                // If the en-passant would expose a check on the king,
                // the en-passant move is illegal, so disqualify it.
                if (isPossiblePin && Bitmask.ForRook(~empty & ~connectingPawns, kingSquare) & enemyQueenRook) break;

                moveList.Add(
                    new Move()
                    {
                        src = from,
                        dst = to,
                        type = MoveType.EnPassant
                    }
                );
            }
        }


        public static void GenerateCastlingMoves(
            Colour sideToMove,
            PieceSet friendlyPieces,
            PieceSet opponentPieces,
            CastlingRights castlingRights,
            List<Move> moveListToAddTo
        )
        {
            int KSC = 0b1000 >> 2 * (int)sideToMove;
            int QSC = 0b0100 >> 2 * (int)sideToMove;

            if (castlingRights.CanKingside(sideToMove)
                && CastlingMoves.CanCastleKingside(sideToMove, friendlyPieces, opponentPieces))
            {
                Move move = new()
                {
                    /*
                    1 => 6, 2 => 62

                    . . . . k . 2 .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . K . 1 .
                    */
                    src = sideToMove == Colour.White ? 4 : 60,
                    dst = sideToMove == Colour.White ? 6 : 62,
                    type = MoveType.Castling
                };

                moveListToAddTo.Add(move);
            }

            if (castlingRights.CanQueenside(sideToMove)
             && CastlingMoves.CanCastleQueenside(sideToMove, friendlyPieces, opponentPieces))
            {
                Move move = new()
                {
                    /*
                    . . 2 . k . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . 1 . K . . .
                    */
                    src = sideToMove == Colour.White ? 4 : 60,
                    dst = sideToMove == Colour.White ? 2 : 58,
                    type = MoveType.Castling
                };

                moveListToAddTo.Add(move);
            }
        }

        public static void GenerateCastling960Moves(Board960 board, List<Move> moveList)
        {
            if (Castling960Moves.CanCastleQueenside(board))
            {
                int dest = board.rookStarts[2 * (int)board.ColourToMove + 1];
                
                // Make sure that we cannot castle if
                // there is no destination
                if (dest == -1) return;

                moveList.Add(new Move(){
                    src = board.PlayerToMove.KingSquare,
                /*
                    . . . . . . x .  < 62
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . x .  < 6
                */
                    dst = dest,
                    type = MoveType.Castling960
                });
            }

            if (Castling960Moves.CanCastleKingside(board))
            {
                int dest = board.rookStarts[2 * (int)board.ColourToMove];
                
                // Make sure that we cannot castle if
                // there is no destination
                if (dest == -1) return;

                moveList.Add(new Move(){
                    src = board.PlayerToMove.KingSquare,
                /*
                    . . x . . . . .  < 58
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . . . . . . .
                    . . x . . . . .  < 2
                */
                    dst = dest,
                    type = MoveType.Castling960
                });
            }
        }
    }
}