using UnityEngine;

namespace DataTypes
{
    [CreateAssetMenu(fileName = "VectorVariable", menuName = "Data Types/Vector Variable", order = 2)]
    public class VectorVariable : ScriptableObject
    {
        public Vector3 value;

        public void Set(Vector3 newValue)
        {
            value = newValue;
        }
    }
}