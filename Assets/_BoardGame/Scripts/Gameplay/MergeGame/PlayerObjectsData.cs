using UnityEngine;

namespace BoardGame
{
    [CreateAssetMenu(menuName = "2084/Player Objects Data",fileName = "PlayerObjectsData")]
    public class PlayerObjectsData : ScriptableObject
    {
        [SerializeField] public GameObject _winScreen, _loseScreen;
    }
}