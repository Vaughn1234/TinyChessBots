//#define DEBUG_TIMER
using ChessChallenge.API;
using System;
using System.Linq;

public class MyBot : IChessBot
{
    //                     .  P    K    B    R    Q    K
    int[] kPieceValues = { 0, 100, 320, 330, 500, 900, 20000 };
    int kMassiveNum = 99999999;

    int mDepth;
    Move mBestMove;
    Square[] CentralSquares = { new Square(27), new Square(28), new Square(35), new Square(36) };
    int gameState = 0;

    public Move Think(Board board, Timer timer)
    {
        mDepth = 5;


        int eval = EvaluateBoardNegaMax(board, mDepth, -kMassiveNum, kMassiveNum, board.IsWhiteToMove ? 1 : -1);

        Console.WriteLine(eval);
        return mBestMove;
    }

    int EvaluateBoardNegaMax(Board board, int depth, int alpha, int beta, int color)
    {
        Move[] legalMoves;

        if (board.IsDraw())
            return 0;

        if (depth == 0 || (legalMoves = board.GetLegalMoves()).Length == 0)
        {
            // EVALUATE

            int sum = 0;
            int sumWhite = 0;
            int sumBlack = 0;

            // Wähle den Zug, der zum schnellsten Schachmatt führt, um ewiges "Matt in 2" zu vermeiden
            if (board.IsInCheckmate())
                return -kMassiveNum + board.PlyCount * -color;



            for (int i = 0; ++i < 7;)
            {
                sumWhite += board.GetPieceList((PieceType)i, true).Count * kPieceValues[i];
                sumBlack += board.GetPieceList((PieceType)i, false).Count * kPieceValues[i];

                sum += sumWhite - sumBlack;

            }
            // Beginne das 'Lategame' wenn entweder weiß oder schwarz eine Gesamtevaluation von unter 22500 haben
            if (Math.Min(sumWhite, sumBlack) <= 22500)
            {
                gameState = 2;
            }
            // Beginne das 'Midgame', wenn weiß oder schwarz eine Gesamtevaluation von unter 23500 haben
            else if (Math.Min(sumWhite, sumBlack) <= 23500)
            {
                gameState = 1;
            }
            // EVALUATE

            int centralPawns = 0;
            // Belohne Bauern in der Mitte des Schachbretts
            foreach (Square square in CentralSquares)
            {
                
                Piece piece = board.GetPiece(square);
                if (piece.PieceType == PieceType.Pawn)
                {
                    centralPawns++;
                }
            }
            sum += 300 * centralPawns * color;

            // Bestrafe Bewegungen der Königin zu Beginn des Spiels
            if (gameState == 0)
            {
                Piece whiteQueenSquare = board.GetPiece(new Square("d1"));
                if (!whiteQueenSquare.IsQueen && whiteQueenSquare.IsWhite)
                    sum -= 200 * color;

                Piece blackQueenSquare = board.GetPiece(new Square("d8"));
                if (!blackQueenSquare.IsQueen && !blackQueenSquare.IsWhite)
                    sum -= 200 * color;

            }
            // Bestrafe Bewegungen des Königs im 'Midgame' und 'Early Game'
            if(gameState == 0 || gameState == 1)
            {
                Piece whiteKingSquare = board.GetPiece(new Square("e1"));
                if (!whiteKingSquare.IsKing)
                    sum -= 200 * color;

                Piece blackKingSquare = board.GetPiece(new Square("e8"));
                if (!blackKingSquare.IsKing)
                    sum -= 400 * color;
            }
            
            // Belohne Bewegungen der Bauern zum letzten Rank am Ende des Spiels
            if (gameState == 2)
            {
                var playerPawns = board.GetPieceList(PieceType.Pawn, board.IsWhiteToMove);
                var oppositionPawns = board.GetPieceList(PieceType.Pawn, !board.IsWhiteToMove);
                if(board.IsWhiteToMove)
                    sum += (2 * playerPawns.Sum(p => p.Square.Rank) * color) + (2 * oppositionPawns.Sum(p => 7 - p.Square.Rank) * (color * -1));
                if(!board.IsWhiteToMove)
                    sum += (2 * playerPawns.Sum(p => 7 - p.Square.Rank) * color) + (2 * oppositionPawns.Sum(p => p.Square.Rank) * (color * -1));

                //if (board.IsInCheck())
                //{
                //    sum -= 200 * color;
                //}

                // Belohne Züge, die die Entfernung des gegnerischen Königs zu den Ecken des Boards verringern
                var kingSquare = board.GetKingSquare(board.IsWhiteToMove);
                var distanceX = Math.Min(Math.Abs(kingSquare.File), Math.Abs(kingSquare.File - 7));
                sum -= (10 - distanceX) * 20 * color;
            }
            return color * sum;
        }

        // TREE SEARCH
        int recordEval = -kMassiveNum;
        foreach (Move move in legalMoves)
        {
            
            board.MakeMove(move);
            int evaluation = -EvaluateBoardNegaMax(board, depth - 1, -beta, -alpha, -color);
            board.UndoMove(move);


            if (recordEval < evaluation)
            {
                recordEval = evaluation;
                if (depth == mDepth) mBestMove = move;
            }
            alpha = Math.Max(alpha, recordEval);
            if (alpha >= beta) break;
        }
        // TREE SEARCH

        return recordEval;
    }
    //int QuiescenceSearch(Board board, int alpha, int beta)
    //{
    //    var standPat = 
    //}
}