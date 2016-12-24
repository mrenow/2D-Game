﻿using System;
using Game.Items;

namespace Game.Logics {
    [Serializable]
    class TileBreakerAttribs : PowerDrainData {

        public TileBreakerAttribs() : base(delegate() { return RawItem.TileBreaker; }) {
        }

        internal override void Update(int x, int y) {
           
        }
    }
}
