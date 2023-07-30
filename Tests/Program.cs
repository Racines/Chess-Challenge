// See https://aka.ms/new-console-template for more information
using ChessChallenge.API;

//var FEN = "rnbqkbnr/ppppp1pp/8/5p2/8/4P1P1/PPPP1P1P/RNBQKBNR b KQkq - 0 2";
//var FEN = "rn1qk1n1/pp2b1p1/3p3r/8/2p2N1P/8/PPPPPP2/R1BQKB2 w Qq - 0 13";
//var FEN = "rn2kb1r/1bqp1ppp/4pn2/1p6/3NP3/2P1BB2/PP3PPP/RN1QK2R b KQkq - 0 9";
//var FEN = "rnb1rk2/1p1p2p1/4pn1p/2qp3P/P4P2/4PNKR/1Pp1B3/R1Q3N1 b - - 4 26";
//var FEN = "5R2/6R1/8/8/3K4/8/8/7k w - - 15 227";


//var FEN = "3k4/7p/p2pp3/2p3K1/4q1N1/P7/6bP/8 b - - 1 45";
var FEN = "k7/r2Q4/bp6/pNp1B2p/P1P1n2P/1P1pP1P1/3P4/4K3 w - - 4 48";
//
var board = Board.CreateBoardFromFEN(FEN);


var timer = new ChessChallenge.API.Timer(int.MaxValue);

//var bot = new AlphaBeta4BrainBot();
//var bot2 = new AlphaBeta4NoTranspositionBrainBot();
//var bot = new AlphaBeta4BrainBot();
var bot = new AlphaBeta4BrainBot();
//var bot = new AlphaBeta4NotSmartBrainBot();
var bot2 = new AlphaBeta4NoTranspositionBrainBot();

CompareBots<AlphaBeta4NotSmartBrainBot, AlphaBeta4NoTranspositionBrainBot>(20);
//CompareBots<MiniMax3BrainBot, AlphaBeta3NoTranspositionBrainBot>(10);
return;

AlphaBetaBrainBot.m_LeafNodes = new();
EvaluateLegalMoves(board, timer, bot);
File.WriteAllLines("Bot1_Leaft.txt", AlphaBetaBrainBot.m_LeafNodes.ToArray());


Console.WriteLine();
Console.WriteLine();
Console.WriteLine();


if (board.GetFenString() != FEN)
{
    Console.Error.WriteLine("board.GetFenString() != FEN");
}


AlphaBetaBrainBot.m_LeafNodes = new();
EvaluateLegalMoves(board, timer, bot2);
File.WriteAllLines("Bot2_Leaft.txt", AlphaBetaBrainBot.m_LeafNodes.ToArray());



var bot3 = new AlphaBeta4NotSmartBrainBot();
AlphaBetaBrainBot.m_LeafNodes = new();
EvaluateLegalMoves(board, timer, bot3);
File.WriteAllLines("Bot3_Leaft.txt", AlphaBetaBrainBot.m_LeafNodes.ToArray());

////Think(board, timer, bot);
////EvaluateMove(board, "g7g5", timer, bot);
////EvaluateMove(board, "g7g6", timer, bot);
////EvaluateMove(board, "f2f3", timer, bot);
//Console.WriteLine();
//Console.WriteLine();

//EvaluateMove(board, "e4c2", timer, bot);
//EvaluateMove(board, "e4c2", timer, bot2);




static void CompareBots<T1, T2>(int gameCount = 1) 
    where T1 : BrainBot, new()
    where T2 : BrainBot, new()
{
    Parallel.For(0, gameCount, i =>
    {
        var b1 = new T1();
        var b2 = new T2();

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
    var movesB1 = b1.EvaluateLegalMoves(board, timer, false);
    var movesB2 = b2.EvaluateLegalMoves(board, timer, false);

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
    var bestMoves = bot.EvaluateLegalMoves(board, timer, false);
    bestMoves.SortByScoreDesc();

    Console.WriteLine(bestMoves.ToString());
}

static void EvaluateMove(Board board, string moveName, ChessChallenge.API.Timer timer, BrainBot bot)
{
    var move = new Move(moveName, board);

    var scoredMove = bot.EvaluateMove(board, timer, move);
    Console.WriteLine(scoredMove.ToString());
}