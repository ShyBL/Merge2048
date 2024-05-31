using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BoardGame
{
    public class BoardGame : MyMonoBehaviour
    {

        [SerializeField] private Node _nodePrefab;
        [SerializeField] private Block _blockPrefab;
        [SerializeField] private SpriteRenderer _boardPrefab;
        [SerializeField] private List<BlockType> _types;

        [SerializeField] private BoardGameConfiguration config;

        // [SerializeField] private GameObject _mergeEffectPrefab;
        // [SerializeField] private FloatingText _floatingTextPrefab;

        // [SerializeField] private AudioClip[] _moveClips;
        // [SerializeField] private AudioClip[] _matchClips;
        // [SerializeField] private AudioSource _source;

        private List<Node> _nodes;
        private List<Block> _blocks;

        private BlockType GetBlockTypeByValue(int value) => _types.First(t => t.Value == value);

        private void Awake()
        {
            GameManager.EventsManager.AddListener(EventsType.GenerateGrid, GenerateGrid);
            GameManager.EventsManager.AddListener(EventsType.SpawnBlocks, SpawnBlocks);
        }

        private void OnDestroy()
        {
            GameManager.EventsManager.RemoveListener(EventsType.GenerateGrid, GenerateGrid);
            GameManager.EventsManager.RemoveListener(EventsType.SpawnBlocks, SpawnBlocks);
        }

        void Start()
        {
            GameManager.ChangeState(MergeGameState.GenerateLevel);
        }
        
        void Update()
        {
            if (GameManager.CurrentState != MergeGameState.WaitingInput) return;
//#if UNITY_EDITOR
            // if (Input.touchCount > 0)
            // {
            //     Touch touch = Input.GetTouch(0);
            //     
            //     Vector2 touchDelta = touch.deltaPosition;
            //
            //     if (Mathf.Abs(touchDelta.x) > Mathf.Abs(touchDelta.y))
            //     {
            //         // Horizontal swipe
            //         if (touchDelta.x > 0)
            //             Shift(Vector2.right);
            //         else
            //             Shift(Vector2.left);
            //     }
            //     else
            //     {
            //         // Vertical swipe
            //         if (touchDelta.y > 0)
            //             Shift(Vector2.up);
            //         else
            //             Shift(Vector2.down);
            //     }
            // }
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
            if (Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
            if (Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
            if (Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);
// #endif
// #if UNITY_ANDROID 
            // if (Input.touchCount > 0)
            // {
            //     Touch touch = Input.GetTouch(0);
            //
            //     if (touch.phase == TouchPhase.Began)
            //     {
            //         Vector2 touchStartPosition = touch.position;
            //         Vector2 touchDelta = touch.deltaPosition;
            //
            //         // Adjust the sensitivity based on your preference
            //         float touchSensitivity = 50f;
            //
            //         if (touchDelta.magnitude > touchSensitivity)
            //         {
            //             if (Mathf.Abs(touchDelta.x) > Mathf.Abs(touchDelta.y))
            //             {
            //                 // Horizontal swipe
            //                 if (touchDelta.x > 0)
            //                     Shift(Vector2.right);
            //                 else
            //                     Shift(Vector2.left);
            //             }
            //             else
            //             {
            //                 // Vertical swipe
            //                 if (touchDelta.y > 0)
            //                     Shift(Vector2.up);
            //                 else
            //                     Shift(Vector2.down);
            //             }
            //         }
            //     }
            // }
//#endif
        }

        void GenerateGrid(object o)
        {
            GameManager.Round = 0;
            _nodes = new List<Node>();
            _blocks = new List<Block>();
            for (int x = 0; x < config._width; x++)
            {
                for (int y = 0; y < config._height; y++)
                {
                    var node = Instantiate(_nodePrefab, new Vector2(x, y), Quaternion.identity);
                    _nodes.Add(node);
                }
            }

            var center = new Vector2((float)config._width / 2 - 0.5f, (float)config._height / 2 - 0.5f);

            var board = Instantiate(_boardPrefab, center, Quaternion.identity);
            board.size = new Vector2(config._width, config._height);

            Camera.main.transform.position = new Vector3(center.x, center.y, -10);

            GameManager.ChangeState(MergeGameState.SpawningBlocks);
        }

        void SpawnBlocks(object o)
        {
            int amount = (int)o;
            var freeNodes = _nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => Random.value).ToList();

            foreach (var node in freeNodes.Take(amount))
            {
                SpawnBlock(node, Random.value > 0.8f ? 4 : 2);
            }

            if (freeNodes.Count() == 1)
            {
                GameManager.ChangeState(MergeGameState.Lose);
                return;
            }

            GameManager.ChangeState(_blocks.Any(b => b.Value == config._winCondition) ? MergeGameState.Win : MergeGameState.WaitingInput);
        }

        void SpawnBlock(Node node, int value)
        {
            var block = Instantiate(_blockPrefab, node.Pos, Quaternion.identity);
            block.Init(GetBlockTypeByValue(value));
            block.SetBlock(node);
            _blocks.Add(block);
        }

        void Shift(Vector2 dir)
        {
            GameManager.ChangeState(MergeGameState.Moving);

            var orderedBlocks = _blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
            if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

            foreach (var block in orderedBlocks)
            {
                var next = block.Node;
                do
                {
                    block.SetBlock(next);

                    var possibleNode = GetNodeAtPosition(next.Pos + dir);
                    if (possibleNode != null)
                    {
                        // We know a node is present
                        // If it's possible to merge, set merge
                        if (possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value))
                        {
                            block.MergeBlock(possibleNode.OccupiedBlock);
                        }
                        // Otherwise, can we move to this spot?
                        else if (possibleNode.OccupiedBlock == null) next = possibleNode;

                        // None hit? End do while loop
                    }
                } while (next != block.Node);
            }

            var sequence = DOTween.Sequence();

            foreach (var block in orderedBlocks)
            {
                var movePoint = block.MergingBlock != null ? block.MergingBlock.Node.Pos : block.Node.Pos;

                sequence.Insert(0, block.transform.DOMove(movePoint, config._travelTime).SetEase(Ease.InQuad));
            }

            sequence.OnComplete(() =>
            {
                var mergeBlocks = orderedBlocks.Where(b => b.MergingBlock != null).ToList();
                foreach (var block in mergeBlocks)
                {
                    MergeBlocks(block.MergingBlock, block);
                }

                // if (mergeBlocks.Any()) _source.PlayOneShot(_matchClips[Random.Range(0, _matchClips.Length)], 0.2f);
                GameManager.ChangeState(MergeGameState.SpawningBlocks);
            });

            // _source.PlayOneShot(_moveClips[Random.Range(0, _moveClips.Length)], 0.2f);
        }

        void MergeBlocks(Block baseBlock, Block mergingBlock)
        {
            var newValue = baseBlock.Value * 2;

            // Instantiate(_mergeEffectPrefab, baseBlock.Pos, Quaternion.identity);
            // Instantiate(_floatingTextPrefab, baseBlock.Pos, Quaternion.identity).Init(newValue);

            SpawnBlock(baseBlock.Node, newValue);

            RemoveBlock(baseBlock);
            RemoveBlock(mergingBlock);
        }

        void RemoveBlock(Block block)
        {
            _blocks.Remove(block);
            Destroy(block.gameObject);
        }

        Node GetNodeAtPosition(Vector2 pos)
        {
            return _nodes.FirstOrDefault(n => n.Pos == pos);
        }
    }

    [Serializable]
    public struct BlockType
    {
        public int Value;
        public Color Color;
    }
}