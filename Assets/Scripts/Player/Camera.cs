using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player {
    public class Camera : MonoBehaviour
    {
        public float stickMinZoom;
        public float stickMaxZoom;
        public float swivelMinZoom;
        public float swivelMaxZoom;
        public float moveSpeedMinZoom;
        public float moveSpeedMaxZoom;
        public float rotationSpeed;

        private float zoom = 1f;
        private float rotationAngle;
        
        private Transform swivel;
        private Transform stick;

        private void Awake() {
            swivel = transform.GetChild(0);
            stick = swivel.GetChild(0);
        }

        private void Update() {
            float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            float rotationDelta = Input.GetAxis("Rotation");
            float xDelta = Input.GetAxis("Horizontal");
            float zDelta = Input.GetAxis("Vertical");

            if (zoomDelta != 0f) AdjustZoom(zoomDelta);
            if (rotationDelta != 0f) AdjustRotation(rotationDelta);
            if (xDelta != 0f || zDelta != 0f) AdjustPosition(xDelta, zDelta);
        }

        private void AdjustZoom(float delta) {
            zoom = Mathf.Clamp01(zoom + delta);

            float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
            stick.localPosition = new Vector3(0f, 0f, distance);

            float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
            swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
        }

        private void AdjustPosition(float xDelta, float zDelta) {
            Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
            float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
            float distance =Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;
            
            Vector3 position = transform.localPosition + (direction * distance);
            transform.localPosition = ClampPosition(position);
        }

        private Vector3 ClampPosition(Vector3 position) {
            float xMax = (World.Config.WorldWidthInChunks * World.Config.ChunkWidth - 1f) * (1.5f * World.Config.OuterRadius);
            position.x = Mathf.Clamp(position.x, 0f, xMax);

            float zMax = (World.Config.WorldHeightInChunks * World.Config.ChunkHeight - 0.5f) * (2f * World.Config.InnerRadius);
            position.z = Mathf.Clamp(position.z, 0f, zMax);
            
            return position;
        }

        private void AdjustRotation(float delta) {
            rotationAngle += delta * rotationSpeed * Time.deltaTime;
            rotationAngle %= 360;
            transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
        }
    }
}
