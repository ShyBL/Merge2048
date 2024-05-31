using UnityEngine;

namespace BoardGame
{
    [CreateAssetMenu(menuName = "2084/Game Configuration",fileName = "BoardGameConfiguration")]
    public class BoardGameConfiguration : ScriptableObject
    {
        [SerializeField] public int _width = 4;
        [SerializeField] public int _height = 4;
        [SerializeField] public float _travelTime = 0.2f;
        [SerializeField] public int _winCondition = 2048;
    }
}