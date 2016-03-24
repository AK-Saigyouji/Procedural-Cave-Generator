using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshHelpers
{
    class Node
    {
        internal Vector3 position { get; private set; }
        internal int vertexIndex { get; set; }

        internal Node(Vector3 position)
        {
            this.position = position;
            vertexIndex = -1;
        }
    }

    class ControlNode : Node
    {
        internal bool active { get; private set; }
        internal Node above { get; private set; }
        internal Node right { get; private set; }

        internal ControlNode(Vector3 position, bool active, float squareSize) : base(position)
        {
            this.active = active;
            above = new Node(position + Vector3.forward * (squareSize / 2f));
            right = new Node(position + Vector3.right * (squareSize / 2f));
        }
    }
}
