﻿using Game.Assets;
using Game.Terrains;
using Game.Util;
using System;

namespace Game.Logics {

    [Serializable]
    internal abstract class LogicData : TileAttribs {
        internal abstract void Update(int x, int y);
    }

    [Serializable]
    internal abstract class PowerSourceData : LogicData {

        //power available to draw from each side
        protected BoundedFloat poweroutL = BoundedFloat.Zero;
        protected BoundedFloat poweroutR = BoundedFloat.Zero;
        protected BoundedFloat poweroutU = BoundedFloat.Zero;
        protected BoundedFloat poweroutD = BoundedFloat.Zero;
    }

    [Serializable]
    internal abstract class PowerTransmitterData : LogicData {

        //power on each side
        protected BoundedFloat poweroutL = BoundedFloat.Zero;
        protected BoundedFloat poweroutR = BoundedFloat.Zero;
        protected BoundedFloat poweroutU = BoundedFloat.Zero;
        protected BoundedFloat poweroutD = BoundedFloat.Zero;

        internal BoundedFloat powerinL = BoundedFloat.Zero;
        internal BoundedFloat powerinR = BoundedFloat.Zero;
        internal BoundedFloat powerinU = BoundedFloat.Zero;
        internal BoundedFloat powerinD = BoundedFloat.Zero;

        protected BoundedFloat dissipate = new BoundedFloat(0, 0, 1);

        protected void UpdateL(int x, int y) {
            PowerTransmitterData tl = Terrain.TileAt(x - 1, y).tileattribs as PowerTransmitterData;
            if (tl != null) BoundedFloat.MoveVals(ref poweroutL, ref tl.powerinR, poweroutL.val);

            PowerDrainData dl = Terrain.TileAt(x - 1, y).tileattribs as PowerDrainData;
            if (dl != null) BoundedFloat.MoveVals(ref poweroutL, ref dl.powerinR, poweroutL.val);
        }
        protected void UpdateR(int x, int y) {
            PowerTransmitterData tr = Terrain.TileAt(x + 1, y).tileattribs as PowerTransmitterData;
            if (tr != null) BoundedFloat.MoveVals(ref poweroutR, ref tr.powerinL, poweroutR.val);

            PowerDrainData dr = Terrain.TileAt(x + 1, y).tileattribs as PowerDrainData;
            if (dr != null) BoundedFloat.MoveVals(ref poweroutR, ref dr.powerinL, poweroutR.val);
        }
        protected void UpdateU(int x, int y) {
            PowerTransmitterData tu = Terrain.TileAt(x, y + 1).tileattribs as PowerTransmitterData;
            if (tu != null) BoundedFloat.MoveVals(ref poweroutU, ref tu.powerinD, poweroutU.val);

            PowerDrainData du = Terrain.TileAt(x, y + 1).tileattribs as PowerDrainData;
            if (du != null) BoundedFloat.MoveVals(ref poweroutU, ref du.powerinD, poweroutU.val);
        }
        protected void UpdateD(int x, int y) {
            PowerTransmitterData td = Terrain.TileAt(x, y - 1).tileattribs as PowerTransmitterData;
            if (td != null) BoundedFloat.MoveVals(ref poweroutD, ref td.powerinU, poweroutD.val);

            PowerDrainData dd = Terrain.TileAt(x, y - 1).tileattribs as PowerDrainData;
            if (dd != null) BoundedFloat.MoveVals(ref poweroutD, ref dd.powerinU, poweroutD.val);
        }
        protected void UpdateAll(int x, int y) {
            UpdateL(x, y);
            UpdateR(x, y);
            UpdateU(x, y);
            UpdateD(x, y);
        }
    }

    [Serializable]
    internal abstract class PowerDrainData : LogicData {

        protected BoundedFloat power = BoundedFloat.Zero;
        //power received
        internal BoundedFloat powerinL = BoundedFloat.Zero;
        internal BoundedFloat powerinR = BoundedFloat.Zero;
        internal BoundedFloat powerinU = BoundedFloat.Zero;
        internal BoundedFloat powerinD = BoundedFloat.Zero;

        protected BoundedFloat cost = BoundedFloat.Zero;
    }

}