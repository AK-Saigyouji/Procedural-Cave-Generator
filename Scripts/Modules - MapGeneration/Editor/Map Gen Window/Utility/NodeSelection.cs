using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace AKSaigyouji.Modules.MapGeneration
{
    public sealed class NodeSelection
    {
        /// <summary>
        /// id of currently selected node.
        /// </summary>
        public int Current { get { return currentSelection; } set { currentSelection = value; } }

        /// <summary>
        /// Was a selection made during the current event?
        /// </summary>
        public bool MadeThisEvent { get { return selectionChangedThisEvent; } set { selectionChangedThisEvent = value; } }

        int currentSelection;
        bool selectionChangedThisEvent;

        public NodeSelection()
        {
            currentSelection = -1;
            selectionChangedThisEvent = false;
        }
    } 
}