using UnityEngine;

namespace ScriptableObjects 
{
    [CreateAssetMenu(fileName = "BoolVariable", menuName = "ScriptableObjects/Variables/BoolVariable", order = 4)]
    public class BoolVariable : ScriptableObject
    {
        [SerializeField] private bool value;
        
        public bool Value
        {
            get => value;
            set => this.value = value;
        }
    }
}