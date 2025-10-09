using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace GomokuGame.Core.Init
{
    /// <summary>
    /// Responsible for deterministic initialization of scene components.
    /// Finds all MonoBehaviours implementing <see cref="IInitializable"/>,
    /// sorts them by <see cref="InitPriorityAttribute"/> (descending), then calls Initialize().
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class InitManager : MonoBehaviour
    {
        public static InitManager Instance { get; private set; }

        private bool initialized = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            // Perform initialization immediately
            RunInitialization();
        }

        private void RunInitialization()
        {
            if (initialized) return;

            // Find all MonoBehaviours in scene that implement IInitializable
            var all = FindObjectsOfType<MonoBehaviour>(true)
                .OfType<IInitializable>()
                .Select(mb => mb as MonoBehaviour)
                .Where(mb => mb != null)
                .ToList();

            // Map to priority
            var list = new List<(MonoBehaviour mb, int priority)>();
            foreach (var mb in all)
            {
                var attr = mb.GetType().GetCustomAttributes(typeof(InitPriorityAttribute), true);
                int priority = 0;
                if (attr != null && attr.Length > 0)
                {
                    priority = ((InitPriorityAttribute)attr[0]).Priority;
                }
                list.Add((mb, priority));
            }

            // Sort by priority descending (higher priority first), then by name to keep deterministic
            var ordered = list.OrderByDescending(x => x.priority).ThenBy(x => x.mb.name).Select(x => x.mb).ToArray();

            // Call Initialize on each
            foreach (var mb in ordered)
            {
                try
                {
                    (mb as IInitializable).Initialize();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"InitManager: Exception while initializing {mb.GetType().FullName} ({mb.name}): {ex}");
                }
            }

            initialized = true;
        }
    }
}
