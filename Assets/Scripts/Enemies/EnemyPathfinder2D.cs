using System.Collections.Generic;
using ShadowDash.Gameplay;
using UnityEngine;

namespace ShadowDash.Enemies
{
    public sealed class EnemyPathfinder2D : MonoBehaviour
    {
        [SerializeField] private float pathRefreshInterval = 0.35f;
        [SerializeField] private float waypointReachDistance = 0.35f;

        private readonly List<Vector2> path = new List<Vector2>();
        private RoomNavigationGrid navigationGrid;
        private Transform target;
        private float nextRefreshTime;
        private int waypointIndex;

        public float WaypointReachDistance => waypointReachDistance;
        public bool IsUsingDirectPath { get; private set; } = true;

        private void Awake()
        {
            navigationGrid = FindAnyObjectByType<RoomNavigationGrid>();
        }

        public Vector2 GetMoveTarget(Vector2 currentPosition, Transform currentTarget)
        {
            target = currentTarget;
            if (target == null)
            {
                path.Clear();
                return currentPosition;
            }

            Vector2 targetPosition = target.position;
            if (navigationGrid == null)
            {
                navigationGrid = FindAnyObjectByType<RoomNavigationGrid>();
            }

            if (navigationGrid == null || navigationGrid.HasLineOfSight(currentPosition, targetPosition))
            {
                IsUsingDirectPath = true;
                path.Clear();
                return targetPosition;
            }

            IsUsingDirectPath = false;
            RefreshPathIfNeeded(currentPosition, targetPosition);
            AdvanceWaypoint(currentPosition);

            if (path.Count == 0 || waypointIndex >= path.Count)
            {
                return targetPosition;
            }

            return path[waypointIndex];
        }

        private void RefreshPathIfNeeded(Vector2 currentPosition, Vector2 targetPosition)
        {
            if (Time.time < nextRefreshTime && path.Count > 0)
            {
                return;
            }

            nextRefreshTime = Time.time + pathRefreshInterval;
            waypointIndex = 0;

            if (!navigationGrid.TryFindPath(currentPosition, targetPosition, path))
            {
                path.Clear();
            }

            AdvanceWaypoint(currentPosition);
        }

        private void AdvanceWaypoint(Vector2 currentPosition)
        {
            while (waypointIndex < path.Count &&
                   Vector2.Distance(currentPosition, path[waypointIndex]) <= waypointReachDistance)
            {
                waypointIndex++;
            }
        }
    }
}
