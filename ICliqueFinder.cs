using System.Collections.Generic;

namespace CliqueCoursework
{
    /// <summary>
    /// Common interface for all clique-finding algorithm implementations.
    /// </summary>
    public interface ICliqueFinder
    {
        /// <summary>
        /// Human-readable name of the algorithm.
        /// </summary>
        string AlgorithmName { get; }

        /// <summary>
        /// Executes the algorithm on the given graph and returns the found clique(s).
        /// Each inner list represents one clique (a set of vertex indices).
        /// </summary>
        List<List<int>> FindCliques(Graph graph);
    }
}