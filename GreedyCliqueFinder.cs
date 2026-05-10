using System.Collections.Generic;
using System.Linq;

namespace CliqueCoursework
{
    /// <summary>
    /// Greedy heuristic for finding a large (not necessarily maximum) clique.
    /// Strategy: start from the vertex with the highest degree, then expand
    /// the clique by repeatedly adding the neighbor with the most connections
    /// to the current clique members.
    /// Time complexity: O(V²) in the worst case.
    /// </summary>
    public class GreedyCliqueFinder : ICliqueFinder
    {
        public string AlgorithmName => "Greedy Clique Finder";

        public List<List<int>> FindCliques(Graph graph)
        {
            var clique = new List<int>();

            // Start with the vertex that has the highest degree
            int startVertex = GetHighestDegreeVertex(graph);
            clique.Add(startVertex);

            // Candidates: neighbors of the starting vertex
            var candidates = new List<int>(graph.GetNeighbors(startVertex));

            while (candidates.Count > 0)
            {
                // Pick the candidate most connected to the current clique
                int best = PickBestCandidate(graph, clique, candidates);
                clique.Add(best);

                // Narrow candidates to those adjacent to the newly added vertex
                candidates = candidates
                    .Where(c => c != best && graph.HasEdge(best, c))
                    .ToList();
            }

            // The greedy algorithm produces a single clique
            return new List<List<int>> { clique };
        }

        private int GetHighestDegreeVertex(Graph graph)
        {
            int best = 0;
            int bestDegree = graph.GetNeighbors(0).Count;

            for (int v = 1; v < graph.VertexCount; v++)
            {
                int degree = graph.GetNeighbors(v).Count;
                if (degree > bestDegree)
                {
                    bestDegree = degree;
                    best = v;
                }
            }
            return best;
        }

        private int PickBestCandidate(Graph graph, List<int> clique, List<int> candidates)
        {
            int best = candidates[0];
            int bestConnections = CountConnectionsToClique(graph, candidates[0], clique);

            for (int i = 1; i < candidates.Count; i++)
            {
                int connections = CountConnectionsToClique(graph, candidates[i], clique);
                if (connections > bestConnections)
                {
                    bestConnections = connections;
                    best = candidates[i];
                }
            }
            return best;
        }

        private int CountConnectionsToClique(Graph graph, int vertex, List<int> clique)
        {
            return clique.Count(v => graph.HasEdge(vertex, v));
        }
    }
}