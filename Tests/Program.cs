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

static void CompareBots(Func<BrainBot> createB1, Func<BrainBot> createB2, int gameCount = 1)
{
    Parallel.For(0, gameCount, i =>
    {
        var b1 = createB1();
        var b2 = createB2();

        Console.WriteLine($"[{i}] - Compare bots {b1.GetType()} vs {b2.GetType()}");

        var board = Board.CreateBoardFromFEN(ChessChallenge.Chess.FenUtility.StartPositionFEN);
        var timer = new ChessChallenge.API.Timer(int.MaxValue);
        while (!board.IsTerminal())
        {
            var move = CompareBotsMoves(b1, b2, board, timer, $"[{i}] - ");
            board.MakeMove(move);
        }

        Console.WriteLine($"[{i}] - Game over");
        Console.WriteLine($"");
    });
}

static Move CompareBotsMoves(BrainBot b1, BrainBot b2, Board board, ChessChallenge.API.Timer timer, string logPrefix = "")
{
    var movesB1 = b1.EvaluateLegalMoves(board, timer);
    var movesB2 = b2.EvaluateLegalMoves(board, timer);

    bool diffDetected = false;
    foreach (var mvB1 in movesB1)
    {
        var mvB2 = movesB2.FirstOrDefault(x => x.Move == mvB1.Move);
        if (mvB2.Score != mvB1.Score)
        {
            Console.WriteLine($"{logPrefix}Divergence on move {mvB1.Move} - {mvB1.Score} vs {mvB2.Score}");
            diffDetected = true;
        }
    }

    if (diffDetected)
        Console.WriteLine(board.GetFenString());

    var score = movesB1.Max(x => x.Score);
    if (!board.IsWhiteToMove)
        score = movesB1.Min(x => x.Score);

    var bestMvs = movesB1.Where(x => x.Score == score).ToArray();
    return bestMvs[Random.Shared.Next(bestMvs.Length)].Move;
}

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