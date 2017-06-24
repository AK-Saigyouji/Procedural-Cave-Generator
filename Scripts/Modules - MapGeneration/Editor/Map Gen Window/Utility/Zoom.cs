using System;
using UnityEngine;

namespace AKSaigyouji.Modules.MapGeneration
{
    public sealed class Zoom
    {
        public float Current { get { return currentZoom; } }
        float currentZoom = 1f;

        Matrix4x4? matrixBeforeZoom = null;

        readonly float min;
        readonly float max;
        readonly float increment;

        const float DEFAULT_MIN = 1f;
        const float DEFAULT_MAX = 3f;
        const float DEFAULT_INCREMENT = 0.1f;

        public Zoom() : this(DEFAULT_MIN, DEFAULT_MAX, DEFAULT_INCREMENT) { }

        public Zoom(float min, float max, float increment)
        {
            this.min = min;
            this.max = max;
            this.increment = increment;
        }

        public void In()
        {
            currentZoom = Mathf.Min(currentZoom + increment, max);
        }

        public void Out()
        {
            currentZoom = Mathf.Max(currentZoom - increment, min);
        }

        public void Reset()
        {
            currentZoom = 1f;
        }


        public void Begin()
        {
            if (matrixBeforeZoom.HasValue)
                throw new InvalidOperationException("Already called Begin. Must call End before beginning again.");

            matrixBeforeZoom = GUI.matrix;
            Vector3 halfScreen = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            var translation = Matrix4x4.TRS(halfScreen, Quaternion.identity, Vector3.one);
            var scale = Matrix4x4.Scale(new Vector3(currentZoom, currentZoom, 1.0f));

            GUI.matrix = translation * scale * translation.inverse;
        }

        public void End()
        {
            if (!matrixBeforeZoom.HasValue)
                throw new InvalidOperationException("Must call Begin before calling End.");

            GUI.matrix = matrixBeforeZoom.Value;

            matrixBeforeZoom = null;
        }
    } 
}