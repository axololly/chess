using Chess.Bitmasks;
using Chess.Castling;
using Chess.Types.Bitboards;
using Chess.Types.Squares;
using Chess.Utilities;
using Chess960;

namespace Chess.Moves
{
    public struct Generator
    {
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
            List<Move> moveListToAddTo,
            bool onlyCaptures = false
        )
        {
            Bitboard moveBitmask = Bitmask.ForRook(friendlyOccupancy | opponentOccupancy, square);

            // If generating only captures, only accept bits that collide with
            // the opponent's bitboard. Otherwise, just accept bits that aren't
            // colliding with our own (don't take our own pieces).
            moveBitmask &= onlyCaptures ? opponentOccupancy : ~friendlyOccupancy;

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
            List<Move> moveListToAddTo,
            bool onlyCaptures = false
        )
        {
            Bitboard moveBitmask = Bitmask.ForBishop(friendlyOccupancy | opponentOccupancy, square);

            // If generating only captures, only accept bits that collide with
            // the opponent's bitboard. Otherwise, just accept bits that aren't
            // colliding with our own (don't take our own pieces).
            moveBitmask &= onlyCaptures ? opponentOccupancy : ~friendlyOccupancy;

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
            List<Move> moveListToAddTo,
            bool onlyCaptures = false
        )
        {
            Bitboard boardMask = friendlyPieces.Mask | opponentPieces.Mask;
            
            // Exclude opponent attack rays and protected pieces (combined)
            Bitboard opponentAttacks = opponentPieces.BaseAttackingBitmask(boardMask ^ friendlyPieces.King);

            Bitboard moveBitmask = Bitmask.ForKing(
                opponentAttacks: opponentAttacks,
                square: square
            );

            // If generating only captures, only accept bits that collide with
            // the opponent's bitboard. Otherwise, just accept bits that aren't
            // colliding with our own (don't take our own pieces).
            moveBitmask &= onlyCaptures ? opponentPieces.Mask : ~friendlyPieces.Mask;

            GenerateMovesFromSameSquare(moveBitmask, square, moveListToAddTo);
        }

        

        public static void GenerateKnightMoves(
            Bitboard friendlyOccupancy,
            Bitboard opponentOccupancy,
            int square,
            Bitboard pinmask,
            Bitboard checkmask,
            List<Move> moveListToAddTo,
            bool onlyCaptures = false
        )
        {
            // There's no need to differentiate pinmasks because
            // if the knight is pinned, it can't move anywhere.
            if (pinmask & 1UL << square) return;

            Bitboard moveBitmask = Bitmask.ForKnight(friendlyOccupancy, square) & checkmask;

            // If generating only captures, only accept bits that collide with
            // the opponent's bitboard. Otherwise, just accept bits that aren't
            // colliding with our own (don't take our own pieces).
            moveBitmask &= onlyCaptures ? opponentOccupancy : ~friendlyOccupancy;

            GenerateMovesFromSameSquare(
                moveBitmask,
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
            List<Move> moveList,
            bool onlyCaptures = true
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

            // Only generate pushes unless we are generating captures, because
            // a push can never be a capture.
            if (!onlyCaptures)
            {
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
            }

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

            Square epPawn   = epSquare - pawnShifts[1];
            
            Bitboard epMask = epSquare.Bitboard | epPawn.Bitboard;

            // If the en-passant square and the enemy pawn are not on
            // the checkmask, then en-passant is not available.
            if (!(checkmask & epMask)) return;

            int kingSquare = us.KingSquare;
            Bitboard kingMask = us.King & 0xFFUL << epPawn.Rank;
            
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