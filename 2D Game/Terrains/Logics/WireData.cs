﻿using Game.Items;
using Game.Terrains.Lighting;
using Game.Util;
using Pencil.Gaming.MathUtils;
using System;

namespace Game.Terrains.Logics {

    [Serializable]
    class WireAttribs : PowerTransmitter, IMultiLight {

        protected bool state;

        public WireAttribs() : base(delegate () { return RawItem.Wire; }) {
            BoundedFloat p = new BoundedFloat(64);
            powerOut.SetPowerAll(p);
            powerIn.SetPowerAll(p);
            transparent = true;
        }

        protected override void UpdateMechanics(int x, int y) {
            BoundedFloat buffer = new BoundedFloat(0, 0, 256);
            CacheInputs();
            powerIn.GivePowerAll(ref buffer);
            EmptyInputs();
            buffer -= dissipate;

            TransferPowerAll(x, y);
            state = powerOut.GetPower(Direction.Left).val > 0 || powerOut.GetPower(Direction.Right).val > 0 || powerOut.GetPower(Direction.Up).val > 0 || powerOut.GetPower(Direction.Down).val > 0;
            CacheOutputs();
            EmptyOutputs();

            UpdateMultiLight(x, y, state ? 1 : 0, this);
            Terrain.TileAt(x, y).enumId = state ? TileID.WireOn : TileID.WireOff;
        }

        ILight[] IMultiLight.Lights() => new ILight[] { new CLight(0, 0, Vector3.Zero), new CLight(4, 0.2f, new Vector3(1, 0.2f, 0.5f)) };
        int IMultiLight.State { get; set; }
    }
}
