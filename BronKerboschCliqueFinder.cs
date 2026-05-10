using System.Collections.Generic;
using System.Linq;

namespace CliqueCoursework
{
    /// <summary>
    /// Bron–Kerbosch algorithm with pivot selection (Tomita variant).
    /// Finds ALL maximal cliques in an undirected graph.
    /// Time complexity: O(3^(V/3)) in the worst case.
    /// </summary>
    public class BronKerboschCliqueFinder : ICliqueFinder
    {
        public string AlgorithmName => "Bron–Kerbosch";

        private List<List<int>> _allCliques;
        private Graph _graph;

        public List<List<int>> FindCliques(Graph graph)
        {
            _graph = graph;
            _allCliques = new List<List<int>>();

            var R = new List<int>();                  // Current clique being built
            var P = graph.GetAllVertices();           // Prospective vertices
            var X = new List<int>();                  // Already-processed vertices

            BronKerbosch(R, P, X);

            return _allCliques;
        }

        private void BronKerbosch(List<int> R, List<int> P, List<int> X)
        {
            if (P.Count == 0 && X.Count == 0)
            {
                // R is a maximal clique
                _allCliques.Add(new List<int>(R));
                return;
            }

            // Choose pivot vertex u from P ∪ X to minimize branching
            int pivot = ChoosePivot(P, X);
            var pivotNeighbors = _graph.GetNeighbors(pivot);

            // Iterate over P \ N(pivot)
            var candidates = P.Except(pivotNeighbors).ToList();

            foreach (int v in candidates)
            {
                var neighbors = _graph.GetNeighbors(v);

                var newR = new List<int>(R) { v };
                var newP = P.Intersect(neighbors).ToList();
                var newX = X.Intersect(neighbors).ToList();

                BronKerbosch(newR, newP, newX);

                P.Remove(v);
                X.Add(v);
            }
        }

        /// <summary>
        /// Tomita pivot: choose the vertex in P ∪ X with the most neighbors in P.
        /// This minimizes the number of recursive calls.
        /// </summary>
        private int ChoosePivot(List<int> P, List<int> X)
        {
            var union = P.Concat(X).ToList();
            int pivot = union[0];
            int maxConnections = P.Count(v => _graph.HasEdge(pivot, v));

            foreach (int u in union)
            {
                int connections = P.Count(v => _graph.HasEdge(u, v));
                if (connections > maxConnections)
                {
                    maxConnections = connections;
                    pivot = u;
                }
            }
            return pivot;
        }
    }
}