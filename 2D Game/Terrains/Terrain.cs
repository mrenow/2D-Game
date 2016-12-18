﻿using System;
using OpenGL;
using System.Collections.Generic;
using Game.Entities;
using Game.Assets;
using Game.Util;
using System.Diagnostics;
using Game.Logics;
using Game.Core;
using Game.Fluids;
using Game.Terrains.Gen;

namespace Game.Terrains {

    static class Terrain {

        #region Fields
        internal static Tile[,] Tiles;
        internal static Biome[] TerrainBiomes;
        internal static int[] Heights;

        private static Random Rand = new Random();

        public static bool UpdateMesh = true;
        public static TerrainVAO vao;

        public static Dictionary<Vector2i, LogicAttribs> LogicDict = new Dictionary<Vector2i, LogicAttribs>();
        public static Dictionary<Vector2i, FluidAttribs> FluidDict = new Dictionary<Vector2i, FluidAttribs>();

        private const int TerrainTextureSize = 16;

        public static ShaderProgram TerrainShader { get; private set; }
        #endregion

        #region Init
        public static void CreateNew(int seed) {
            Console.WriteLine("Generating terrain...");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            TerrainGen.Generate(seed);
            watch.Stop();
            Console.WriteLine("Terrain generation finished in " + watch.ElapsedMilliseconds + " ms");
        }

        public static void CreateNew(string seed) {
            int sum = 0;
            for (int i = 0; i < seed.Length; i++) {
                sum += (int)Math.Pow(2, i) * seed[i];
            }
            CreateNew(sum);
        }

        public static void Load(Tile[,] Tiles) {
            Terrain.Tiles = Tiles;
            for (int i = 0; i < Terrain.Tiles.GetLength(0); i++) {
                for (int j = 0; j < Terrain.Tiles.GetLength(1); j++) {
                    LogicAttribs logic = Terrain.Tiles[i, j].tileattribs as LogicAttribs;
                    if (logic != null) {
                        LogicDict.Add(new Vector2i(i, j), logic);
                    }

                    FluidAttribs fluid = Terrain.Tiles[i, j].tileattribs as FluidAttribs;
                    if (fluid != null) {
                        FluidDict.Add(new Vector2i(i, j), fluid);
                    }
                }
            }
        }

        public static void LoadShaders() {
            TerrainShader = new ShaderProgram(Shaders.TerrainVert, Shaders.TerrainFrag);
            Console.WriteLine("Terrain Shader Log: ");
            Console.WriteLine(TerrainShader.ProgramLog);
        }

        public static void Init() {
            InitHeights();
            Lighting.Init();
            InitMesh();
        }

        private static void InitHeights() {
            Heights = new int[Tiles.GetLength(0)];
            for (int i = 0; i < Tiles.GetLength(0); i++) {
                for (int j = Tiles.GetLength(1) - 1; j > 0; j--) {
                    if (!Tiles[i, j].tileattribs.transparent) {
                        Heights[i] = j;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Matrices
        public static void UpdateViewMatrix(Matrix4 mat) {
            Debug.Assert(TerrainShader != null);
            Gl.UseProgram(TerrainShader.ProgramID);
            TerrainShader["viewMatrix"].SetValue(mat);
            Gl.UseProgram(0);
        }

        public static void SetProjectionMatrix(Matrix4 mat) {
            Debug.Assert(TerrainShader != null);
            Gl.UseProgram(TerrainShader.ProgramID);
            TerrainShader["projectionMatrix"].SetValue(mat);
            Gl.UseProgram(0);
        }
        #endregion

        #region Mesh

        private static void InitMesh() {
            Vector2[] vertices;
            int[] elements;
            Vector2[] uvs;
            CalculateMesh(out vertices, out elements, out uvs);

            Lighting.Init();
            float[] lightings = Lighting.CalcMesh();

            vao = new TerrainVAO(vertices, elements, uvs, lightings);
        }

        public static void Range(out int minx, out int maxx, out int miny, out int maxy) {
            float posX, posY;
            if (Player.Instance != null) {
                posX = (int)Player.Instance.data.pos.x;
                posY = (int)Player.Instance.data.pos.y;
            } else {
                posX = Player.StartX;
                posY = Player.StartY;
            }

            minx = (int)(posX + GameRenderer.zoom / 2);
            maxx = (int)(posX - GameRenderer.zoom / 2);
            miny = (int)(posY + GameRenderer.zoom / 2 / Program.AspectRatio);
            maxy = (int)(posY - GameRenderer.zoom / 2 / Program.AspectRatio);
            MathUtil.Clamp(ref minx, 0, Tiles.GetLength(0) - 1);
            MathUtil.Clamp(ref miny, 0, Tiles.GetLength(1) - 1);
            MathUtil.Clamp(ref maxx, 0, Tiles.GetLength(0) - 1);
            MathUtil.Clamp(ref maxy, 0, Tiles.GetLength(1) - 1);
        }

        public static void CalculateMesh(out Vector2[] vertices, out int[] elements, out Vector2[] uvs) {
            int startX, endX, startY, endY;
            Range(out startX, out endX, out startY, out endY);
            List<Vector2> verticesList = new List<Vector2>((endX - startX) * (endY - startY));
            List<Vector2> uvList = new List<Vector2>((endX - startX) * (endY - startY));
            for (int i = startX; i <= endX; i++) {
                for (int j = startY; j <= endY; j++) {
                    if (Tiles[i, j].enumId != TileEnum.Air) {
                        TileEnum t = Tiles[i, j].enumId;

                        float x = ((float)((int)t % TerrainTextureSize)) / TerrainTextureSize;
                        float y = ((float)((int)t / TerrainTextureSize)) / TerrainTextureSize;
                        float s = 1f / TerrainTextureSize;
                        //half pixel correction
                        float h = 1f / (TerrainTextureSize * TerrainTextureSize * 2);

                        // float height = 1;
                        //TODO
                        //something like
                        float height = 1;
                        FluidAttribs fattribs = Tiles[i, j].tileattribs as FluidAttribs;
                        if (fattribs != null) {
                            height = fattribs.height;
                        }

                        //top left, bottom left, bottom right, top right
                        verticesList.AddRange(new Vector2[] {
                            new Vector2(i,j+height),
                            new Vector2(i,j),
                            new Vector2(i+1,j),
                            new Vector2(i+1,j+height)
                        });

                        switch (Tiles[i, j].tileattribs.rotation) {
                            case Direction.Up:
                                //top left, bottom left, bottom right, top right
                                uvList.AddRange(new Vector2[] {
                                    new Vector2(x+h,y+s-h),
                                    new Vector2(x+h,y+h),
                                    new Vector2(x+s-h,y+h),
                                    new Vector2(x+s-h,y+s-h)
                                });
                                break;
                            case Direction.Right:
                                uvList.AddRange(new Vector2[] {
                                    new Vector2(x+h,y+h),
                                    new Vector2(x+s-h,y+h),
                                    new Vector2(x+s-h,y+s-h),
                                    new Vector2(x+h,y+s-h)
                                });
                                break;
                            case Direction.Down:
                                uvList.AddRange(new Vector2[] {
                                    new Vector2(x+s-h,y+h),
                                    new Vector2(x+s-h,y+s-h),
                                    new Vector2(x+h,y+s-h),
                                    new Vector2(x+h,y+h),
                                });
                                break;
                            case Direction.Left:
                                uvList.AddRange(new Vector2[] {
                                    new Vector2(x+s-h,y+s-h),
                                    new Vector2(x+h,y+s-h),
                                    new Vector2(x+h,y+h),
                                    new Vector2(x+s-h,y+h)
                                });
                                break;
                        }
                    }
                }
            }
            vertices = verticesList.ToArray();
            uvs = uvList.ToArray();

            elements = new int[verticesList.Count / 4 * 6];
            for (int i = 0; i < verticesList.Count / 4; i++) {
                elements[6 * i] = 4 * i;
                elements[6 * i + 1] = 4 * i + 1;
                elements[6 * i + 2] = 4 * i + 2;
                elements[6 * i + 3] = 4 * i + 2;
                elements[6 * i + 4] = 4 * i + 3;
                elements[6 * i + 5] = 4 * i + 0;
            }
        }

        internal static Tile[,] ShallowCopyTileData() {
            Tile[,] result = new Tile[Tiles.GetLength(0), Tiles.GetLength(1)];
            for (int i = 0; i < Tiles.GetLength(0); i++) {
                for (int j = 0; j < Tiles.GetLength(1); j++) {
                    result[i, j] = Tiles[i, j];
                }
            }
            return result;
        }

        #endregion Mesh

        #region Collision
        public static bool WillCollide(Entity entity, Vector2 offset, out Tile collidedTile, out int collision_x, out int collision_y) { return WillCollide(entity.hitbox, offset, out collidedTile, out collision_x, out collision_y); }
        public static bool WillCollide(Hitbox hitbox, Vector2 offset, out Tile collidedTile, out int collision_x, out int collision_y) {
            int x1 = (int)Math.Floor(hitbox.Position.x + offset.x);
            int x2 = (int)Math.Floor(hitbox.Position.x + hitbox.Size.x + offset.x);

            int y1 = (int)Math.Floor(hitbox.Position.y + offset.y);
            int y2 = (int)Math.Floor(hitbox.Position.y + hitbox.Size.y + offset.y);

            for (int i = x1; i <= x2; i++) {
                for (int j = y1; j <= y2; j++) {
                    if (TileAt(i, j).tileattribs.solid) {
                        collidedTile = TileAt(i, j);
                        collision_x = i;
                        collision_y = j;
                        return true;
                    }
                }
            }
            collidedTile = Tile.Air;
            collision_x = -1;
            collision_y = -1;
            return false;
        }

        public static bool IsColliding(Entity entity) {
            Tile col;
            return IsColliding(entity, out col);
        }

        public static bool IsColliding(Hitbox h) {
            Tile col;
            return IsColliding(h, out col);
        }

        public static bool IsColliding(Hitbox h, out Tile col) {
            int colx, coly;
            return WillCollide(h, Vector2.Zero, out col, out colx, out coly);
        }

        public static bool IsColliding(Entity entity, out Tile col) {
            return IsColliding(entity.hitbox, out col);
        }

        public static Vector2 CorrectTerrainCollision(Entity entity) {
            int height = -1;
            //find highest non intersecting terrain
            for (int i = (int)entity.data.pos.x; i < entity.data.pos.x + entity.hitbox.Size.x + MathUtil.Epsilon; i++) {
                int curHeight = NonCollisionAbove(i, (int)entity.data.pos.y, (int)(entity.hitbox.Size.y + MathUtil.Epsilon));
                if (curHeight > height) height = curHeight;
            }
            if (height == -1)
                return entity.data.pos.val;
            return new Vector2(entity.data.pos.x, height);
        }

        private static int NonCollisionAbove(int x, int y, int height) {
            int ry = y;
            while (true) {
                if (Valid(x, ry, height)) break;
                ry++;
            }
            return ry;
        }

        private static bool Valid(int x, int y, int height) {
            if (x < 0 || x >= Tiles.GetLength(0) || y < 0 || y >= Tiles.GetLength(1)) return true;
            for (int i = 0; i < height; i++) {
                if (y + i >= Tiles.GetLength(1)) continue;
                if (Tiles[x, y + i].enumId != TileEnum.Air) return false;
            }
            return true;
        }

        internal static int HighestPoint(int x) {
            if (x < 0 || x >= Tiles.GetLength(0)) return 0;
            for (int i = Tiles.GetLength(1) - 1; i > 0; i--) {
                if (Tiles[x, i].enumId != TileEnum.Air) return i + 1;
            }
            return 1;
        }
        #endregion Collision

        #region Tile Interaction
        public static Tile TileAt(Vector2i v) { return TileAt(v.x, v.y); }
        public static Tile TileAt(int x, int y) { return x < 0 || x >= Tiles.GetLength(0) || y < 0 || y >= Tiles.GetLength(1) ? Tile.Invalid : Tiles[x, y]; }
        public static Tile TileAt(float x, float y) { return TileAt((int)x, (int)y); }

        public static void Explode(float x, float y, float radius, float error) {
            for (float i = -radius + MathUtil.RandFloat(Rand, -error, error); i <= radius + MathUtil.RandFloat(Rand, -error, error); i++) {
                for (float j = -radius + MathUtil.RandFloat(Rand, -error, error); j <= radius + MathUtil.RandFloat(Rand, -error, error); j++) {
                    if (i * i + j * j <= radius * radius) {
                        if (BreakTileNoLightingUpdate((int)(x + i), (int)(j + y)).enumId == TileEnum.Tnt) Explode(x + i, j + y, radius, error);
                    }
                }
            }
            UpdateMesh = true;
            Lighting.RecalcAll();
        }

        internal static void SetTileTerrainGen(int x, int y, Tile tile, bool overwrite) {
            if (x < 0 || x >= Tiles.GetLength(0) || y < 0 || y >= Tiles.GetLength(1)) return;
            if (!overwrite && Tiles[x, y].enumId != TileEnum.Air) return;
            FluidAttribs fluid = tile.tileattribs as FluidAttribs;
            if (fluid != null) {
                FluidDict.Add(new Vector2i(x, y), fluid);
            }
            Tiles[x, y] = tile;
        }

        public static void SetTile(int x, int y, Tile tile, Vector2 v) {
            Direction d = DirectionUtil.FromVector2(v);
            tile.tileattribs.rotation = d;
            SetTile(x, y, tile);
        }
        public static void SetTile(int x, int y, Tile tile) { SetTile(x, y, tile, true); }
        private static void SetTileNoUpdate(int x, int y, Tile tile) { SetTile(x, y, tile, false); }
        private static void SetTile(int x, int y, Tile tile, bool update) {
            if (x < 0 || x >= Tiles.GetLength(0) || y < 0 || y >= Tiles.GetLength(1)) return;
            if (Tiles[x, y].enumId != TileEnum.Air) return;

            //remove this 
            int xmin = (int)Math.Floor(Player.Instance.data.pos.x), xmax = (int)Math.Ceiling(Player.Instance.data.pos.x + Player.Instance.hitbox.Size.x - 1);
            int ymin = (int)Math.Floor(Player.Instance.data.pos.y), ymax = (int)Math.Ceiling(Player.Instance.data.pos.y + Player.Instance.hitbox.Size.y - 1);
            if (x >= xmin && x <= xmax && y >= ymin && y <= ymax) return;
            //

            Tiles[x, y] = tile;

            LogicAttribs logic = tile.tileattribs as LogicAttribs;
            if (logic != null && update) {
                LogicDict.Add(new Vector2i(x, y), logic);
            }

            LightAttribs light = tile.tileattribs as LightAttribs;
            if (light != null) {
                Lighting.AddLight(x, y, light.intensity);
            }

            FluidAttribs fluid = tile.tileattribs as FluidAttribs;
            if (fluid != null) {
                FluidDict.Add(new Vector2i(x, y), fluid);
            }

            UpdateMesh = true;
            if (y > Heights[x]) Heights[x] = y;
            Lighting.QueueUpdate(x, y);
        }

        public static Tile BreakTile(int x, int y) {
            if (x < 0 || x >= Tiles.GetLength(0) || y < 0 || y >= Tiles.GetLength(1)) return Tile.Invalid;
            Tile tile = TileAt(x, y);
            if (tile.enumId == TileEnum.Air) return Tile.Air;
            if (tile.enumId == TileEnum.Bedrock) return Tile.Invalid;

            LogicDict.Remove(new Vector2i(x, y));
            FluidDict.Remove(new Vector2i(x, y));
            Lighting.RemoveLight(x, y);

            Tiles[x, y] = Tile.Air;
            UpdateMesh = true;
            if (Heights[x] == y) {
                for (int j = y - 1; j > 0; j--) {
                    if (!Tiles[x, j].tileattribs.transparent) {
                        Heights[x] = j;
                        break;
                    }
                }
            }
            Lighting.QueueUpdate(x, y);
            return tile;
        }
        public static Tile BreakTile(Vector2i v) {
            return BreakTile(v.x, v.y);
        }
        public static Tile BreakTile(Vector2 v) {
            return BreakTile(v.x, v.y);
        }
        public static Tile BreakTile(float x, float y) {
            return BreakTile((int)x, (int)y);
        }
        private static Tile BreakTileNoLightingUpdate(int x, int y) {
            if (x < 0 || x >= Tiles.GetLength(0) || y < 0 || y >= Tiles.GetLength(1)) return Tile.Invalid;
            Tile tile = TileAt(x, y);
            if (tile.enumId == TileEnum.Air) return Tile.Air;
            if (tile.enumId == TileEnum.Bedrock) return Tile.Invalid;
            LogicDict.Remove(new Vector2i(x, y));
            FluidDict.Remove(new Vector2i(x, y));
            Tiles[x, y] = Tile.Air;
            UpdateMesh = true;

            if (Heights[x] == y) {
                for (int j = y - 1; j > 0; j--) {
                    if (!Tiles[x, j].tileattribs.transparent) {
                        Heights[x] = j;
                        break;
                    }
                }
            }
            return tile;
        }

        public static void MoveTile(int x, int y, Direction dir) {
            Vector2i v = new Vector2i(x, y);
            switch (dir) {
                case Direction.Left:
                    if (TileAt(x - 1, y).enumId == TileEnum.Air) {
                        LogicAttribs logic;
                        if (LogicDict.TryGetValue(v, out logic)) {
                            LogicDict.Remove(v);
                            LogicDict.Add(new Vector2i(x - 1, y), logic);
                        }
                        SetTileNoUpdate(x - 1, y, TileAt(x, y));
                        BreakTile(x, y);
                        UpdateMesh = true;
                        Lighting.QueueUpdate(x - 1, y);
                    }
                    break;
                case Direction.Right:
                    if (TileAt(x + 1, y).enumId == TileEnum.Air) {
                        LogicAttribs logic;
                        if (LogicDict.TryGetValue(v, out logic)) {
                            LogicDict.Remove(v);
                            LogicDict.Add(new Vector2i(x + 1, y), logic);
                        }
                        SetTileNoUpdate(x + 1, y, TileAt(x, y));
                        BreakTile(x, y);
                        UpdateMesh = true;
                        Lighting.QueueUpdate(x + 1, y);
                    }
                    break;
                case Direction.Up:
                    if (TileAt(x, y + 1).enumId == TileEnum.Air) {
                        LogicAttribs logic;
                        if (LogicDict.TryGetValue(v, out logic)) {
                            LogicDict.Remove(v);
                            LogicDict.Add(new Vector2i(x, y + 1), logic);
                        }
                        SetTileNoUpdate(x, y + 1, TileAt(x, y));
                        BreakTile(x, y);
                        UpdateMesh = true;
                        Lighting.QueueUpdate(x, y + 1);
                    }
                    break;
                case Direction.Down:
                    if (TileAt(x, y - 1).enumId == TileEnum.Air) {
                        LogicAttribs logic;
                        if (LogicDict.TryGetValue(v, out logic)) {
                            LogicDict.Remove(v);
                            LogicDict.Add(new Vector2i(x, y - 1), logic);
                        }
                        SetTileNoUpdate(x, y - 1, TileAt(x, y));
                        BreakTile(x, y);
                        UpdateMesh = true;
                        Lighting.QueueUpdate(x, y - 1);
                    }
                    break;
            }
            Lighting.QueueUpdate(x, y);

        }
        #endregion

        #region Update & Render
        public static void Render() {
            Gl.UseProgram(TerrainShader.ProgramID);
            Gl.BindVertexArray(vao.ID);
            Gl.BindTexture(Textures.TerrainTexture.TextureTarget, Textures.TerrainTexture.TextureID);
            Gl.DrawElements(BeginMode.Triangles, vao.count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            Gl.BindTexture(Textures.TerrainTexture.TextureTarget, 0);
            Gl.BindVertexArray(0);
            Gl.UseProgram(0);
        }

        public static void Update() {

            FluidManager.Update();
            LogicManager.Update();

            if (UpdateMesh) {
                Vector2[] vertices;
                int[] elements;
                Vector2[] uvs;
                CalculateMesh(out vertices, out elements, out uvs);

                float[] lightings = Lighting.CalcMesh();

                vao.UpdateData(vertices, elements, uvs, lightings);
            }
            UpdateMesh = false;

        }


        public static void CleanUp() {
            TerrainShader.DisposeChildren = true;
            TerrainShader.Dispose();
        }

        #endregion

    }
}
