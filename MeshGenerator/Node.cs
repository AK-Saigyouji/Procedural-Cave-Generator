using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshHelpers
{
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
