using System;
using UnityEngine;

namespace CatsVsPillows.Data
{
    /// <summary>Purely visual prop placed in the room (paintings, plants, lamps...).</summary>
    [Serializable]
    public sealed class DecorItem
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Vector3 position;
        [SerializeField] private float yRotation;
        [SerializeField, Min(0.01f)] private float scale = 1f;

        public DecorItem() { }

        public DecorItem(GameObject prefab, Vector3 position, float yRotation, float scale)
        {
            this.prefab = prefab;
            this.position = position;
            this.yRotation = yRotation;
            this.scale = scale;
        }

        public GameObject Prefab => prefab;
        public Vector3 Position => position;
        public float YRotation => yRotation;
        public float Scale => scale;
    }
}
