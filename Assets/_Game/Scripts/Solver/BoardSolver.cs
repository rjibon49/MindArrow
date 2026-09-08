using System.Collections.Generic;
using MindArrow.Board;

namespace MindArrow.Solver
{
    public static class BoardSolver
    {
        public static List<int> GetEscapableArrows(BoardModel model)
        {
            List<int> result = new();

            if (model == null)
            {
                return result;
            }

            foreach (int id in model.GetArrowIds())
            {
                if (model.CanEscape(id))
                {
                    result.Add(id);
                }
            }

            return result;
        }

        public static bool TryFindSolution(
            BoardModel model,
            out List<int> removalOrder)
        {
            removalOrder = new List<int>();

            if (model == null)
            {
                return false;
            }

            HashSet<string> visited = new();
            List<int> remaining = new(model.GetArrowIds());

            return Search(
                model,
                remaining,
                removalOrder,
                visited);
        }

        private static bool Search(
            BoardModel model,
            List<int> remaining,
            List<int> order,
            HashSet<string> visited)
        {
            if (remaining.Count == 0)
            {
                return true;
            }

            remaining.Sort();
            string key = string.Join(",", remaining);

            if (!visited.Add(key))
            {
                return false;
            }

            List<int> candidates = new();

            for (int i = 0; i < remaining.Count; i++)
            {
                int id = remaining[i];

                if (model.CanEscape(id))
                {
                    candidates.Add(id);
                }
            }

            if (candidates.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                int id = candidates[i];

                if (!model.TryGetArrow(id, out ArrowData stored) || stored == null)
                {
                    continue;
                }

                model.RemoveArrow(id);
                remaining.Remove(id);
                order.Add(id);

                bool solved = Search(
                    model,
                    remaining,
                    order,
                    visited);

                // Always restore the live model before returning/backtracking.
                model.AddArrow(stored);

                if (solved)
                {
                    return true;
                }

                order.RemoveAt(order.Count - 1);
                remaining.Add(id);
            }

            return false;
        }
    }
}
