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
    /// 最小可视化桥：把逻辑层实体位置同步到简单 GameObject（或可选 prefab）上。
    /// <para>
    /// 边界：
    /// - 只读 <see cref="Game.Battle.Runtime.Core.BattleContext"/> 数据，不反向修改战斗状态。
    /// - 属于 Presentation 层调试工具，不代表最终资产与性能方案。
    /// </para>
    /// </summary>
    public sealed class BattleSimpleVisualizer : MonoBehaviour
    {
        [Header("References")]
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

        /// <summary>HeroId -> 视图对象</summary>
        private readonly Dictionary<string, GameObject> _heroViews = new();

        /// <summary>EnemyId -> 视图对象</summary>
        private readonly Dictionary<string, GameObject> _enemyViews = new();

        /// <summary>BulletId -> 视图对象</summary>
        private readonly Dictionary<string, GameObject> _bulletViews = new();

        private void Awake()
        {
            if (_runner == null)
            {
                // 允许把 Visualizer 与 Runner 分开放置；Editor 下也可手动拖拽覆盖该行为。
                _runner = FindObjectOfType<BattleRunnerBehaviour>();
            }
        }

        /// <summary>
        /// 放在 LateUpdate：尽量等逻辑帧推进后再同步，减少同一帧内“逻辑未更新但先画一帧”的错觉。
        /// </summary>
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

        /// <summary>同步英雄视图：始终显示（最小闭环默认只有一个英雄）。</summary>
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

        /// <summary>同步敌人视图：死亡会隐藏，但对象仍保留在字典里直到敌人实体被移除。</summary>
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

        /// <summary>同步子弹视图：仅显示 IsActive 的子弹；失效子弹会销毁视图。</summary>
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

        /// <summary>
        /// 获取或创建视图对象：优先使用 prefab；否则创建带 Collider 的 primitive（仅用于调试可视化）。
        /// </summary>
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

        /// <summary>移除已不存在的实体对应的视图对象。</summary>
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

        /// <summary>清空所有视图（战斗结束或 Runner 丢失时调用）。</summary>
        private void ClearAllViews()
        {
            ClearViewMap(_heroViews);
            ClearViewMap(_enemyViews);
            ClearViewMap(_bulletViews);
        }

        /// <summary>销毁并清空某个映射表。</summary>
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

        /// <summary>
        /// 尝试设置材质颜色（Editor 下调试用）。
        /// <para>
        /// 注意：访问 <see cref="Renderer.material"/> 会实例化材质副本；正式项目更推荐 sharedMaterial 或统一材质球。
        /// </para>
        /// </summary>
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
