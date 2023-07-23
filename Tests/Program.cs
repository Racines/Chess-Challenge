// See https://aka.ms/new-console-template for more information
using ChessChallenge.API;
using ChessChallenge.Example;

var FEN = "rnbqkbnr/ppppp1pp/8/5p2/8/4P1P1/PPPP1P1P/RNBQKBNR b KQkq - 0 2";
//var FEN = "rn1qk1n1/pp2b1p1/3p3r/8/2p2N1P/8/PPPPPP2/R1BQKB2 w Qq - 0 13";


var board = Board.CreateBoardFromFEN(FEN);
var timer = new ChessChallenge.API.Timer(int.MaxValue);

var bot = new MyBot();
//var bot = new EvilBot();

EvaluateLegalMoves(board, timer, bot);
//Think(board, timer, bot);
//EvaluateMove(board, "g7g5", timer, bot);
//EvaluateMove(board, "g7g6", timer, bot);
//EvaluateMove(board, "f2f3", timer, bot);

static void Think(Board board, ChessChallenge.API.Timer timer, BrainBot bot)
{
    var bestMove = bot.Think(board, timer);
    Console.WriteLine(bestMove.ToString());
}

static void EvaluateLegalMoves(Board board, ChessChallenge.API.Timer timer, BrainBot bot)
{
    var bestMoves = bot.EvaluateLegalMoves(board, timer);
    bestMoves.SortByScoreDesc();

    Console.WriteLine(bestMoves.ToString());
}

static void EvaluateMove(Board board, string moveName, ChessChallenge.API.Timer timer, BrainBot bot)
{
    var move = new Move(moveName, board);

    var scoredMove = bot.EvaluateMove(board, timer, move);
    Console.WriteLine(scoredMove.ToString());
}