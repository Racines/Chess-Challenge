using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonteCarlo
{
    public interface IState<TPlayer, TAction>
    {
        IState<TPlayer, TAction> Clone();

        TPlayer CurrentPlayer { get; }

        IList<TAction> Actions { get; }

        void ApplyAction(TAction action);
        bool IsTerminal();

        double GetResult(TPlayer forPlayer);
    }

    public class MonteCarloTreeSearch
    {
        private class Node<TPlayer, TAction> : IMctsNode<TAction>
        {
            public Node(IState<TPlayer, TAction> state, TAction action = default(TAction), Node<TPlayer, TAction> parent = null)
            {
                this.Parent = parent;
                Player = state.CurrentPlayer;
                State = state;
                Action = action;
                UntriedActions = new HashSet<TAction>(state.Actions);
            }

            public Node<TPlayer, TAction> Parent { get; }

            public IList<Node<TPlayer, TAction>> Children { get; } = new List<Node<TPlayer, TAction>>();

            public int NumRuns { get; set; }

            public double NumWins { get; set; }

            public TPlayer Player { get; }

            public IState<TPlayer, TAction> State { get; }

            public TAction Action { get; }

            public ISet<TAction> UntriedActions { get; }

            public IList<TAction> Actions => State.Actions;

            private static double c = Math.Sqrt(2);

            public double ExploitationValue => NumWins / NumRuns;

            public double ExplorationValue => (Math.Sqrt(2 * Math.Log(Parent.NumRuns) / NumRuns));

            private double UCT => ExploitationValue + ExplorationValue;

            public Node<TPlayer, TAction> SelectChild()
            {
                return Children.MaxElementBy(e => e.UCT);
            }

            public Node<TPlayer, TAction> AddChild(TAction action, IState<TPlayer, TAction> state)
            {
                var child = new Node<TPlayer, TAction>(state, action, this);
                UntriedActions.Remove(action);
                Children.Add(child);

                return child;
            }

            public void BuildTree(Func<int, long, bool> shouldContinue, Timer timer)
            {
                var iterations = 0;
                while (shouldContinue(iterations++, timer.MillisecondsElapsedThisTurn))
                {
                    var node = this;
                    var state = State.Clone();

                    //select
                    while (!node.UntriedActions.Any() && node.Actions.Any())
                    {
                        node = node.SelectChild();
                        state.ApplyAction(node.Action);
                    }

                    //expand
                    if (node.UntriedActions.Any())
                    {
                        var action = node.UntriedActions.RandomChoice();
                        state.ApplyAction(action);
                        node = node.AddChild(action, state);
                    }

                    //simulate
                    while (!state.IsTerminal())
                        state.ApplyAction(state.Actions.RandomChoice());

                    //backpropagate
                    while (node != null)
                    {
                        node.NumRuns++;
                        node.NumWins += state.GetResult(this.Player);
                        node = node.Parent;
                    }
                }
            }

            public override string ToString()
            {
                return $"{NumWins}/{NumRuns}: ({ExploitationValue}/{ExplorationValue}={UCT})";
            }
        }

        public static IEnumerable<IMctsNode<TAction>> GetTopActions<TPlayer, TAction>(IState<TPlayer, TAction> state, int maxIterations, Timer timer)
        {
            return GetTopActions(state, maxIterations, long.MaxValue, timer);
        }

        public static IEnumerable<IMctsNode<TAction>> GetTopActions<TPlayer, TAction>(IState<TPlayer, TAction> state, long timeBudget, Timer timer) 
        {
            return GetTopActions(state, int.MaxValue, timeBudget, timer);
        }

        public static IEnumerable<IMctsNode<TAction>> GetTopActions<TPlayer, TAction>(IState<TPlayer, TAction> state, int maxIterations, long timeBudget, Timer timer)
        {
            var root = new Node<TPlayer, TAction>(state);
            root.BuildTree((numIterations, elapsedMs) => numIterations < maxIterations && elapsedMs < timeBudget, timer);
            return root.Children
                .OrderByDescending(n => n.NumRuns);
        }
    }

    public interface IMctsNode<TAction>
    {
        TAction Action { get; }

        int NumRuns { get; }

        double NumWins { get; }
    }

    internal static class CollectionExtensions
    {
        private static Random _random = new Random();

        public static T RandomChoice<T>(this ICollection<T> source, Random random = null)
        {
            var i = (random ?? _random).Next(source.Count);
            return source.ElementAt(i);
        }

        public static T MaxElementBy<T>(this IEnumerable<T> source, Func<T, double> selector)
        {
            var currentMaxElement = default(T);
            var currentMaxValue = double.MinValue;

            foreach (var element in source)
            {
                var value = selector(element);
                if (currentMaxValue < value)
                {
                    currentMaxValue = value;
                    currentMaxElement = element;
                }
            }

            return currentMaxElement;
        }
    }
}