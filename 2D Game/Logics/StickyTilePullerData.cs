﻿using Game.Terrains;
using Game.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Logics {
    [Serializable]
    class StickyTilePullerData : PowerDrainData {

        [NonSerialized]
        private CooldownTimer cooldown;

        private const int MaxTiles = 32;

        public bool state { get; private set; }

        public StickyTilePullerData() {
            powerinL.max = powerinR.max = powerinU.max = powerinD.max = 64;
            power.max = 32;
            cooldown = new CooldownTimer(40);
        }

        internal override void Update(int x, int y) {

            BoundedFloat.MoveVals(ref powerinL, ref power, powerinL.val);
            BoundedFloat.MoveVals(ref powerinR, ref power, powerinR.val);
            BoundedFloat.MoveVals(ref powerinU, ref power, powerinU.val);
            BoundedFloat.MoveVals(ref powerinD, ref power, powerinD.val);

            state = false;
            BoundedFloat dissipate = new BoundedFloat(0, 0, 1);
            if (power.val == power.max) {
                state = true;
                if (cooldown.Ready()) {
                    HashSet<Vector2i> done;
                    if (PushTilesHelper(x, y, out done)) {
                        var list = done.ToList();
                        list.Sort(new TilePositionComparer());
                        foreach (var v in list) {
                            Terrain.MoveTile(v.x, v.y, Direction.Left);
                        }
                        BoundedFloat cost = new BoundedFloat(0, 0, 32);
                        BoundedFloat.MoveVals(ref power, ref cost, power.max);
                        cooldown.Reset();
                    }
                }
            }
            BoundedFloat.MoveVals(ref power, ref dissipate, dissipate.max);
        }

        //returns false if tiles cannot be moved, else returns true
        private bool PushTilesHelper(int x, int y, out HashSet<Vector2i> processed) {
            x++;
            processed = new HashSet<Vector2i>();
            var processing = new HashSet<Vector2i>();

            if (Terrain.TileAt(x, y).enumId != TileEnum.Air) return false;

            processing.Add(new Vector2i(x, y));

            while (processing.Count > 0) {
                if (processed.Count > MaxTiles)
                    return false;

                var list = new List<Vector2i>(processing);
                foreach (var v in list) {

                    processing.Remove(v);

                    Vector2i lv = new Vector2i(v.x - 1, v.y);
                    TileID l = Terrain.TileAt(lv);
                    if (lv.x - 1 >= x && l.enumId != TileEnum.Air && !processed.Contains(lv)) {
                        processed.Add(lv);
                        Vector2i[] arr = Neighbours(lv);
                        processing.Add(lv);
                        foreach (var a in arr)
                            if (Terrain.TileAt(a).enumId != TileEnum.Air && a.x - 1 > x)
                                processing.Add(a);
                    } else {
                        processing.Remove(lv);
                    }

                    Vector2i rv = new Vector2i(v.x + 1, v.y);
                    TileID r = Terrain.TileAt(rv);
                    if (rv.x - 1 >= x && r.enumId != TileEnum.Air && !processed.Contains(rv)) {
                        processed.Add(rv);
                        Vector2i[] arr = Neighbours(rv);
                        processing.Add(rv);
                        foreach (var a in arr)
                            if (Terrain.TileAt(a).enumId != TileEnum.Air && a.x - 1 > x)
                                processing.Add(a);
                    } else {
                        processing.Remove(rv);
                    }

                    Vector2i uv = new Vector2i(v.x, v.y + 1);
                    TileID u = Terrain.TileAt(uv);
                    if (uv.x - 1 >= x && u.enumId != TileEnum.Air && !processed.Contains(uv)) {
                        processed.Add(uv);
                        Vector2i[] arr = Neighbours(uv);
                        processing.Add(uv);
                        foreach (var a in arr)
                            if (Terrain.TileAt(a).enumId != TileEnum.Air && a.x - 1 > x)
                                processing.Add(a);
                    } else {
                        processing.Remove(uv);
                    }

                    Vector2i dv = new Vector2i(v.x, v.y - 1);
                    TileID d = Terrain.TileAt(dv);
                    if (dv.x - 1 >= x && d.enumId != TileEnum.Air && !processed.Contains(dv)) {
                        processed.Add(dv);
                        Vector2i[] arr = Neighbours(dv);
                        processing.Add(dv);
                        foreach (var a in arr)
                            if (Terrain.TileAt(a).enumId != TileEnum.Air && a.x - 1 > x)
                                processing.Add(a);
                    } else {
                        processing.Remove(dv);
                    }

                }
            }

            return true;
        }

        private Vector2i[] Neighbours(Vector2i v) {
            int x = v.x, y = v.y;
            return new Vector2i[] {
              new Vector2i(x-1,y),
              new Vector2i(x+1,y),
              new Vector2i(x, y+1),
              new Vector2i(x, y-1)
            };
        }

        private class TilePositionComparer : IComparer<Vector2i> {
            public int Compare(Vector2i x, Vector2i y) {
                if (x.x > y.x) return 1;
                else if (x.x == y.x) return 0;
                else return -1;
            }
        }
    }
}