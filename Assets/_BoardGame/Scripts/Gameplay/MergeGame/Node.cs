using UnityEngine;

namespace BoardGame
{
    public class Node : MyMonoBehaviour
    {
        public Vector2 Pos => transform.position;

        public Block OccupiedBlock;
    }
}