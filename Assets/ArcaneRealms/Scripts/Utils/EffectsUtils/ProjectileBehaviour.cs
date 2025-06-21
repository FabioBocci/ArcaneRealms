using System;
using System.Threading.Tasks;
using ArcaneRealms.Scripts.Cards.GameCards;
using UnityEngine;

namespace ArcaneRealms.Scripts.Utils.EffectsUtils
{
    public class ProjectileBehaviour : MonoBehaviour
    {
        public float maxTime = 10;
        public float speed = 5f;
        public float maxParableHeight = 20f;
        public double angle = 45f;

        private float timer;

        private MonsterPlatformController target = null;
        private bool hasFinished = false;
        private Vector3 initialPosition = Vector3.up;
        private double angleRadians;
        

        public void SetTarget(MonsterPlatformController newTarget)
        {
            target = newTarget;
            initialPosition = transform.position;
            angleRadians = Mathf.Deg2Rad * (float)angle;
            timer = 0;
        }

        private void Update()
        {
            if (hasFinished) return;
            if (target == null) return;

            timer += Time.deltaTime;
            if (timer >= maxTime)
            {
                // Max time reached, destroy and call callback
                hasFinished = true;
                Destroy(gameObject);
                return;
            }

            float t = timer / maxTime;
            Vector3 direction = target.GetMonsterPosition().position - initialPosition;
            direction.Normalize();

            // Calculate parabolic trajectory
            float verticalDistance = maxParableHeight * Mathf.Sin((float)angleRadians);
            float horizontalSpeed = speed; // horizontalDistance / (Mathf.Cos((float)angleRadians) * maxTime);
            Vector3 newPosition = initialPosition + direction * (horizontalSpeed * t);
            newPosition.y = initialPosition.y + Mathf.Sin((float)angleRadians) * horizontalSpeed * t - 0.5f * verticalDistance * t * t;

            transform.position = newPosition;
        }

        private void OnCollisionEnter(Collision other)
        {
            HandleCollision(other.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleCollision(other);
        }

        private void HandleCollision(Collider other)
        {
            throw new NotImplementedException();
        }

        public async Task WaitForHit()
        {
            while (!hasFinished)
            {
                await Awaitable.NextFrameAsync();
            }
        }
    }
}