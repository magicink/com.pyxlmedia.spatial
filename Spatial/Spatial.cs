using UnityEngine;

namespace PyxlMedia.Spatial
{
    public abstract class Spatial : MonoBehaviour, ISpatial
    {
        public abstract float DistanceTo(ISpatial b);
    }
}