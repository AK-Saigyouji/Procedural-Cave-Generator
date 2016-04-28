using UnityEngine;

namespace MeshHelpers
{
    /// <summary>
    /// Node associates a position with an index.  
    /// </summary>
    class Node
    {
        public Vector3 position { get; private set; }
        public int vertexIndex { get; set; }

        public Node(Vector3 position)
        {
            this.position = position;
            vertexIndex = -1;
        }
    }

    /// <summary>
    /// Each square will have a control node in each corner. Which control nodes are active will determine configuration. 
    /// In addition, the control nodes will manage the additional midpoint nodes to obtain a finger triangulation of squares.
    /// </summary>
    class ControlNode : Node
    {
        public bool active { get; private set; }
        public Node above { get; private set; }
        public Node right { get; private set; }

        public ControlNode(Vector3 position, bool active, float squareSize) : base(position)
        {
            this.active = active;
            above = new Node(position + Vector3.forward * (squareSize / 2f));
            right = new Node(position + Vector3.right * (squareSize / 2f));
        }
    }
}
