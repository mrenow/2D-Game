﻿using Game.Core;
using Game.Entities;
using Game.Fluids;
using Game.Particles;
using Game.Terrains;
using Game.Util;
using OpenGL;
using System;

namespace Game.Items {
    [Serializable]
    abstract class ItemAttribs {

        public int stackSize { get; private set; }
        public string name;

        public ItemAttribs(int stackSize, string name) {
            this.stackSize = stackSize;
            this.name = name;
        }

        public abstract void Use(Inventory inv, Vector2i invslot, Vector2 position, Vector2 direction);

    }

    [Serializable]
    class Item_No_Attribs : ItemAttribs {

        public Item_No_Attribs(string name, int stackSize) : base(stackSize, name) { }

        public override void Use(Inventory inv, Vector2i invslot, Vector2 position, Vector2 direction) { }
    }

    [Serializable]
    class Item_Tile_Attribs : ItemAttribs {

        protected Tile tile;

        public Item_Tile_Attribs(string name, Tile tile) : base(999, name) {
            this.tile = tile;
        }

        public override void Use(Inventory inv, Vector2i invslot, Vector2 position, Vector2 direction) {
            int x = (int)position.x;
            int y = (int)position.y;
            if (!Terrain.TileAt(x, y).tileattribs.solid && EntityManager.GetEntitiesAt(position).Length == 0 && inv.RemoveItem(invslot.x, invslot.y))
                Terrain.SetTile(x, y, tile);
        }
    }

    [Serializable]
    class Item_DataTile_Attribs : ItemAttribs {

        protected Func<Tile> tile;

        public Item_DataTile_Attribs(string name, Func<Tile> tile) : base(999, name) {
            this.tile = tile;
        }

        public override void Use(Inventory inv, Vector2i invslot, Vector2 position, Vector2 direction) {
            int x = (int)position.x;
            int y = (int)position.y;
            if (!Terrain.TileAt(x, y).tileattribs.solid && EntityManager.GetEntitiesAt(position).Length == 0 && inv.RemoveItem(invslot.x, invslot.y))
                Terrain.SetTile(x, y, tile());
        }
    }

    [Serializable]
    class Item_DirectionalTile_Attribs : Item_DataTile_Attribs {


        public Item_DirectionalTile_Attribs(string name, Func<Tile> tile) : base(name, tile) {
        }

        public override void Use(Inventory inv, Vector2i invslot, Vector2 position, Vector2 direction) {
            int x = (int)position.x;
            int y = (int)position.y;
            if (!Terrain.TileAt(x, y).tileattribs.solid && EntityManager.GetEntitiesAt(position).Length == 0 && inv.RemoveItem(invslot.x, invslot.y))
                Terrain.SetTile(x, y, tile(), direction);
        }
    }

    [Serializable]
    class Item_Fluid_Attribs : Item_DataTile_Attribs {

        public Item_Fluid_Attribs(string name, Func<Tile> tile) : base(name, tile) {
        }

        public override void Use(Inventory inv, Vector2i invslot, Vector2 position, Vector2 direction) {
            int x = (int)position.x;
            int y = (int)position.y;
            if (!Terrain.TileAt(x, y).tileattribs.solid && !(Terrain.TileAt(x, y).tileattribs is FluidAttribs) && inv.RemoveItem(invslot.x, invslot.y))
                Terrain.SetTile(x, y, tile());
        }

    }

    [Serializable]
    abstract class ItemStaffParticleAttribs : ItemAttribs {

        public ItemStaffParticleAttribs(string name) : base(1, name) { }

        protected Vector2 CalculatePos() {
            Vector2 pos = Player.Instance.data.pos.val;
            pos += new Vector2(Player.Instance.hitbox.Size.x / 2, Player.Instance.hitbox.Size.y / 2);
            pos += MathUtil.RandVector2(Program.Rand, new Vector2(-1, -1), new Vector2(1, 1));
            return pos;
        }

        protected Vector2 CalculateVel() {
            Vector2 vel = Input.RayCast().Normalize();
            Vector2 playervel = new Vector2(Player.Instance.data.vel.x, Player.Instance.data.vel.y);
            vel += playervel;
            return vel;
        }

    }

    [Serializable]
    class Item_RedStaff_Attribs : ItemStaffParticleAttribs {
        public Item_RedStaff_Attribs() : base("Staff of Destruction") {
        }

        public override void Use(Inventory inv, Vector2i invslot, Vector2 position, Vector2 direction) {
            SParc_Destroy.Create(CalculatePos(), CalculateVel());
        }
    }

    [Serializable]
    class Item_PurpleStaff_Attribs : ItemStaffParticleAttribs {
        public Item_PurpleStaff_Attribs() : base("Staff of Damage") {
        }
        public override void Use(Inventory inv, Vector2i invslot, Vector2 position, Vector2 direction) {
            SParc_Damage.Create(CalculatePos(), CalculateVel());
        }
    }

    [Serializable]
    class Item_GreenStaff_Attribs : ItemStaffParticleAttribs {
        public Item_GreenStaff_Attribs() : base("Staff of Speed") {
        }
        public override void Use(Inventory inv, Vector2i invslot, Vector2 position, Vector2 direction) {
            SParc_Speed.Create(CalculatePos(), CalculateVel());
        }
    }

    [Serializable]
    class Item_BlueStaff_Attribs : ItemStaffParticleAttribs {
        public Item_BlueStaff_Attribs() : base("Staff of Water") {
        }
        public override void Use(Inventory inv, Vector2i invslot, Vector2 position, Vector2 direction) {
            SParc_Water.Create(CalculatePos(), CalculateVel());
        }
    }

    [Serializable]
    class Item_YellowStaff_Attribs : ItemStaffParticleAttribs {
        public Item_YellowStaff_Attribs() : base("Staff of Creation") {
        }
        public override void Use(Inventory inv, Vector2i invslot, Vector2 position, Vector2 direction) {
            SParc_Place.Create(CalculatePos(), CalculateVel());
        }
    }
}
