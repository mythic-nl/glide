using UnityEngine;

namespace Utils
{
    public class Common
    {
        /// <summary>
        /// Get the interpolation time based on a response value and delta time.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public static float GetInterpolationTime(float response, float deltaTime)
        {
            return 1f - Mathf.Exp(-response * deltaTime);
        }
    }
}