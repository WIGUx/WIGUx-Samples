﻿using UnityEngine;

namespace WIGU.Modules
{
    public class SampleModule : WiguModule
    {
        /// <summary>
        /// Event executed on WIGU initialization.
        /// </summary>
        public override void OnInitialize()
        {
            Debug.Log($"{GetType().Assembly.FullName} OnInitialize");
        }

        /// <summary>
        /// This event is executed on each room loaded.
        /// </summary>
        /// <param name="index">Level's index loaded</param>
        public override void OnLevelLoaded(int index)
        {
            Debug.Log($"{GetType().Assembly.FullName} OnLevelLoaded: {index}");
        }

        /// <summary>
        /// Executes on game initialization when all the UGC's are loaded into memory.
        /// You can modifiy initial properties or add/remove components.
        /// </summary>
        /// <param name="gameObject2">GameObject loaded.</param>
        public override void OnUgcLoaded(AssetBundle assetBundle, GameObject gameObject2)
        {
            Debug.Log($"{GetType().Assembly.FullName} OnUgcLoaded: {gameObject2.name}");
        }
    }
}
