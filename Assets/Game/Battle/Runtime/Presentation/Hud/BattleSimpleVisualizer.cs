using System.Collections.Generic;
using Game.Battle.Runtime.Bootstrap;
using Game.Battle.Runtime.Entities;
using Game.Battle.Runtime.Entities.AI;
using Game.Battle.Runtime.Entities.Bullet;
using Game.Battle.Runtime.Entities.Hero;
using UnityEngine;

namespace Game.Battle.Runtime.Presentation.Hud
{
    /// <summary>
    /// Minimal visual bridge that mirrors runtime entities to simple scene objects.
    /// </summary>
    public sealed class BattleSimpleVisualizer : MonoBehaviour
    {
        [SerializeField]
        private BattleRunnerBehaviour? _runner;

        [Header("Optional Prefabs")]
        [SerializeField]
        private GameObject? _heroPrefab;

        [SerializeField]
        private GameObject? _enemyPrefab;

        [SerializeField]
        private GameObject? _bulletPrefab;

        [Header("Spawn Height Offset")]
        [SerializeField]
        private float _heroYOffset = 0.5f;

        [SerializeField]
        private float _enemyYOffset = 0.5f;

        [SerializeField]
        private float _bulletYOffset = 0.2f;

        private readonly Dictionary<string, GameObject> _heroViews = new();
        private readonly Dictionary<string, GameObject> _enemyViews = new();
        private readonly Dictionary<string, GameObject> _bulletViews = new();

        private void Awake()
        {
            if (_runner == null)
            {
                _runner = FindObjectOfType<BattleRunnerBehaviour>();
            }
        }

        private void LateUpdate()
        {
            if (_runner?.World == null)
            {
                ClearAllViews();
                return;
            }

            EntityRegistry registry = _runner.World.Context.Registry;
            SyncHeroes(registry.Heroes);
            SyncEnemies(registry.Enemies);
            SyncBullets(registry.Bullets);
        }

        private void OnDestroy()
        {
            ClearAllViews();
        }

        private void SyncHeroes(List<HeroEntity> heroes)
        {
            HashSet<string> aliveIds = new();
            for (int i = 0; i < heroes.Count; i++)
            {
                HeroEntity hero = heroes[i];
                aliveIds.Add(hero.Id);

                GameObject view = GetOrCreateView(
                    hero.Id,
                    _heroViews,
                    _heroPrefab,
                    PrimitiveType.Capsule,
                    Color.cyan,
                    "HeroView");

                view.SetActive(true);
                view.transform.position = hero.Position + new Vector3(0f, _heroYOffset, 0f);
            }

            RemoveMissingViews(_heroViews, aliveIds);
        }

        private void SyncEnemies(List<AIEntity> enemies)
        {
            HashSet<string> existingIds = new();
            for (int i = 0; i < enemies.Count; i++)
            {
                AIEntity enemy = enemies[i];
                existingIds.Add(enemy.Id);

                GameObject view = GetOrCreateView(
                    enemy.Id,
                    _enemyViews,
                    _enemyPrefab,
                    PrimitiveType.Cube,
                    Color.red,
                    "EnemyView");

                view.SetActive(enemy.IsAlive);
                view.transform.position = enemy.Position + new Vector3(0f, _enemyYOffset, 0f);
            }

            RemoveMissingViews(_enemyViews, existingIds);
        }

        private void SyncBullets(List<BulletEntity> bullets)
        {
            HashSet<string> aliveIds = new();
            for (int i = 0; i < bullets.Count; i++)
            {
                BulletEntity bullet = bullets[i];
                if (!bullet.IsActive)
                {
                    continue;
                }

                aliveIds.Add(bullet.Id);
                GameObject view = GetOrCreateView(
                    bullet.Id,
                    _bulletViews,
                    _bulletPrefab,
                    PrimitiveType.Sphere,
                    Color.yellow,
                    "BulletView");

                view.transform.localScale = Vector3.one * 0.25f;
                view.SetActive(true);
                view.transform.position = bullet.Position + new Vector3(0f, _bulletYOffset, 0f);
            }

            RemoveMissingViews(_bulletViews, aliveIds);
        }

        private GameObject GetOrCreateView(
            string entityId,
            Dictionary<string, GameObject> map,
            GameObject? prefab,
            PrimitiveType fallbackPrimitive,
            Color fallbackColor,
            string labelPrefix)
        {
            if (map.TryGetValue(entityId, out GameObject existing))
            {
                return existing;
            }

            GameObject view;
            if (prefab != null)
            {
                view = Instantiate(prefab, transform);
            }
            else
            {
                view = GameObject.CreatePrimitive(fallbackPrimitive);
                view.transform.SetParent(transform, worldPositionStays: false);
                TrySetColor(view, fallbackColor);
            }

            view.name = $"{labelPrefix}_{entityId}";
            map[entityId] = view;
            return view;
        }

        private void RemoveMissingViews(Dictionary<string, GameObject> map, HashSet<string> aliveIds)
        {
            List<string> removeIds = new();
            foreach (KeyValuePair<string, GameObject> pair in map)
            {
                if (!aliveIds.Contains(pair.Key))
                {
                    if (pair.Value != null)
                    {
                        Destroy(pair.Value);
                    }

                    removeIds.Add(pair.Key);
                }
            }

            for (int i = 0; i < removeIds.Count; i++)
            {
                map.Remove(removeIds[i]);
            }
        }

        private void ClearAllViews()
        {
            ClearViewMap(_heroViews);
            ClearViewMap(_enemyViews);
            ClearViewMap(_bulletViews);
        }

        private static void ClearViewMap(Dictionary<string, GameObject> map)
        {
            foreach (KeyValuePair<string, GameObject> pair in map)
            {
                if (pair.Value != null)
                {
                    Destroy(pair.Value);
                }
            }

            map.Clear();
        }

        private static void TrySetColor(GameObject view, Color color)
        {
            if (!view.TryGetComponent(out Renderer renderer) || renderer.material == null)
            {
                return;
            }

            renderer.material.color = color;
        }
    }
}
