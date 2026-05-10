using System.Collections.Generic;

namespace CliqueCoursework
{
    public class Graph
    {
        private readonly int _vertexCount;
        private readonly bool[,] _adjacencyMatrix;

        public int VertexCount => _vertexCount;

        public Graph(int vertexCount)
        {
            _vertexCount = vertexCount;
            _adjacencyMatrix = new bool[vertexCount, vertexCount];
        }

        public void AddEdge(int u, int v)
        {
            if (u < 0 || u >= _vertexCount || v < 0 || v >= _vertexCount)
                throw new ArgumentOutOfRangeException("Vertex index out of range.");
            if (u == v)
                throw new ArgumentException("Self-loops are not allowed.");

            _adjacencyMatrix[u, v] = true;
            _adjacencyMatrix[v, u] = true;
        }

        public bool HasEdge(int u, int v) => _adjacencyMatrix[u, v];

        public List<int> GetNeighbors(int vertex)
        {
            var neighbors = new List<int>();
            for (int i = 0; i < _vertexCount; i++)
                if (_adjacencyMatrix[vertex, i])
                    neighbors.Add(i);
            return neighbors;
        }

        public List<int> GetAllVertices()
        {
            var vertices = new List<int>();
            for (int i = 0; i < _vertexCount; i++)
                vertices.Add(i);
            return vertices;
        }

        // Returns vertices common to both lists that are adjacent to a given vertex
        public List<int> Intersect(List<int> set, int vertex)
        {
            var result = new List<int>();
            foreach (int v in set)
                if (HasEdge(vertex, v))
                    result.Add(v);
            return result;
        }
    }
}