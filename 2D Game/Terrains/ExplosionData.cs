﻿using Game.Guis;
using Game.Items;
using System;

namespace Game.Terrains {

    [Serializable]
    class ExplosionAttribs : TileAttribs {

        public float radius, error;

        public object HotbarExplosion { get; private set; }

        public override void Interact(int x, int y) {
            if (PlayerInventory.Instance.CurrentlySelectedItem().rawitem.id == ItemID.Igniter)
                Terrain.Explode(x, y, radius, error);
        }
    }

}
