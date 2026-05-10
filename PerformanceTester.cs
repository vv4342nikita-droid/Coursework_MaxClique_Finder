using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CliqueCoursework
{
    /// <summary>
    /// Measures execution time of ICliqueFinder implementations
    /// and saves structured results to a .txt file.
    /// </summary>
    public class PerformanceTester
    {
        public record TestResult(
            string AlgorithmName,
            int VertexCount,
            int EdgeCount,
            int CliquesFound,
            int MaxCliqueSize,
            double ElapsedMilliseconds, 
            DateTime TestedAt
        );

        private readonly List<TestResult> _results = new();

        /// <summary>
        /// Runs the given algorithm on the graph, records timing, and returns the result.
        /// </summary>
        public TestResult Run(ICliqueFinder finder, Graph graph)
        {
            int edgeCount = CountEdges(graph);

            var stopwatch = Stopwatch.StartNew();
            var cliques = finder.FindCliques(graph);
            stopwatch.Stop();

            int maxSize = cliques.Count > 0 ? cliques.Max(c => c.Count) : 0;

            var result = new TestResult(
                AlgorithmName: finder.AlgorithmName,
                VertexCount: graph.VertexCount,
                EdgeCount: edgeCount,
                CliquesFound: cliques.Count,
                MaxCliqueSize: maxSize,
                ElapsedMilliseconds: stopwatch.Elapsed.TotalMilliseconds, 
                TestedAt: DateTime.Now
            );

            _results.Add(result);
            return result;
        }

        /// <summary>
        /// Runs the algorithm several times and returns aggregated statistics.
        /// </summary>
        public (TestResult Best, double AverageMs) RunBenchmark(
            ICliqueFinder finder, Graph graph, int iterations = 5)
        {
            var times = new List<double>(); // Изменено на double
            TestResult? last = null;

            for (int i = 0; i < iterations; i++)
            {
                last = Run(finder, graph);
                times.Add(last.ElapsedMilliseconds);
            }

            double avg = times.Average();
            return (last!, avg);
        }

        /// <summary>
        /// Saves all collected results to a UTF-8 text file.
        /// </summary>
        public void SaveResultsToFile(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Звіт про продуктивність алгоритмів пошуку кліки ===");
            sb.AppendLine($"Згенеровано: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine(new string('=', 60));
            sb.AppendLine();

            foreach (var r in _results)
            {
                sb.AppendLine($"Алгоритм         : {r.AlgorithmName}");
                sb.AppendLine($"Час тестування   : {r.TestedAt:HH:mm:ss}");
                sb.AppendLine($"Кількість вершин : {r.VertexCount}");
                sb.AppendLine($"Кількість ребер  : {r.EdgeCount}");
                sb.AppendLine($"Знайдено клік    : {r.CliquesFound}");
                sb.AppendLine($"Макс. розмір     : {r.MaxCliqueSize}");
                sb.AppendLine($"Час (мс)         : {r.ElapsedMilliseconds:F4}"); 
                sb.AppendLine(new string('-', 60));
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// Clears the accumulated result history.
        /// </summary>
        public void ClearResults() => _results.Clear();

        public IReadOnlyList<TestResult> Results => _results.AsReadOnly();

        // ── helpers ──────────────────────────────────────────────────────────

        private static int CountEdges(Graph graph)
        {
            int count = 0;
            int n = graph.VertexCount;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    if (graph.HasEdge(i, j))
                        count++;
            return count;
        }
    }
}