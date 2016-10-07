﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using Game.TitleScreen;
using Game.Core;

namespace Game.Interaction {
    static class GameGuiRenderer {

        public static void Init() {
            Inventory.Init();
            Hotbar.Init();
            Hotbar.Update();
            Healthbar.Init(Player.Instance.MaxHealth);
        }

        public static void Update() {
            Hotbar.Update();
            Inventory.Update();
        }

        public static void Render() {
            Gl.LineWidth(7);

            Gl.UseProgram(Gui.shader.ProgramID);

            //healthbar
            RenderInstance(Healthbar.Bar, new Vector2(((2 - Healthbar.BarWidth) / 2) - 1, 0.01 + Hotbar.SizeY * 2 - 1), new Vector2(Healthbar.Health / Healthbar.MaxHealth, 1), new Vector3(1, 1, 1));

            //inventory
            if (Inventory.toggle) {
                if (Inventory.Selected.HasValue) {
                    int cx = Inventory.Selected.Value.x, cy = Inventory.Selected.Value.y;
                    RenderInstance(Inventory.SelectedDisplay, Inventory.Pos + new Vector2(cx * Hotbar.SizeX, cy * Hotbar.SizeY * Program.AspectRatio));
                }
                RenderInstance(Inventory.Frame, Inventory.Pos);
                RenderInstance(Inventory.ItemDisplay, Inventory.Pos);
                RenderInstance(Inventory.Background, Inventory.Pos);
            }

            //hotbar
            RenderInstance(Hotbar.SelectedDisplay, new Vector2(((2 - Inventory.InvColumns * Hotbar.SizeX) / 2 + Hotbar.CurSelectedSlot * Hotbar.SizeX) - 1, -1));
            RenderInstance(Hotbar.ItemDisplay, new Vector2(((2 - Inventory.InvColumns * Hotbar.SizeX) / 2) - 1, -1));
            RenderInstance(Hotbar.Frame, new Vector2(((2 - Inventory.InvColumns * Hotbar.SizeX) / 2) - 1, -1));

            Gl.UseProgram(0);

            Gl.LineWidth(1);
        }

        private static void RenderInstance(GuiModel model, Vector2 position) {
            RenderInstance(model, position, new Vector2(1, 1), new Vector3(1, 1, 1));
        }

        private static void RenderInstance(GuiModel model, Vector2 position, Vector2 size, Vector3 colour) {
            ShaderProgram shader = Gui.shader;
            shader["position"].SetValue(position);
            shader["size"].SetValue(size);
            shader["colour"].SetValue(colour);
            Gl.BindVertexArray(model.vao.ID);
            Gl.BindTexture(model.texture.TextureTarget, model.texture.TextureID);
            Gl.DrawElements(model.drawmode, model.vao.count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Gl.BindTexture(model.texture.TextureTarget, 0);
            Gl.BindVertexArray(0);
        }

        public static void Dispose() {
            Inventory.Dispose();
            Hotbar.Dispose();
            Healthbar.Dispose();
        }
    }
}
