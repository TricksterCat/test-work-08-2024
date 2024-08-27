using System;
using UnityEngine;

namespace Runtime.Controllers
{
    public abstract class BaseEntry : MonoBehaviour
    {
        public abstract Rect ResolveRect();
        public abstract bool PushDamage(float value);
        public abstract void Destroy();

        /*
        private void LateUpdate()
        {
            var rect = ResolveRect();
            
            Debug.DrawLine(rect.min, new Vector3(rect.xMin, rect.yMax), Color.red, 0.1f);
            Debug.DrawLine(new Vector3(rect.xMin, rect.yMax), rect.max, Color.red, 0.1f);
            Debug.DrawLine(rect.max, new Vector3(rect.xMax, rect.yMin), Color.red, 0.1f);
            Debug.DrawLine(new Vector3(rect.xMax, rect.yMin), rect.min, Color.red, 0.1f);
        }
        */
    }
}