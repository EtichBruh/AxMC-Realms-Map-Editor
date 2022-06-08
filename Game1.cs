﻿using AxMC_Realms_Client.Map;
using AxMC_Realms_ME.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace AxMC_Realms_ME
{
    public class Game1 : Game
    {
        public static Tile[] MapTiles;
        public static Vector2[] MapBlocks;
        public static int MapWidth = 256, MapHeight = 256;
        public static byte[] byteMap;
        public static Entity[] Entities;
        public static int numTiles = 9;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private int blockSize = 25;
        private float _blockSize;

        bool ShowGrid;
        bool Anims;
        bool DeleteMode;
        int ScrollVal;
        int choosedBlock = 0;
        Modes Mode = 0;

        Rectangle RectFill;
        MouseState MState;
        Point TMPos; // Tiled mouse Pos

        Texture2D GridTile, GridPixel;
        Texture2D TileSet; //environment nvm no environment here ! its entity now, because im lazy to make it like that
        Texture2D Picker, Bucket;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.TextInput += Window_TextInput;

            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.ApplyChanges();

            Window.AllowUserResizing = true;
        }
        protected override void Initialize()
        {

            // init map arrays
            byteMap = new byte[MapWidth * MapHeight];
            MapTiles = new Tile[byteMap.Length];
            Entities = new Entity[byteMap.Length];

            // fill byte map array with 255 ,because 255 is null tile
            Array.Fill<byte>(byteMap, 255);

            // Set blocksize factor. Will be removed with camera
            _blockSize = 1f / blockSize;

            // Welcome the user :)
            Console.WriteLine(
                "Welcome to the AxMC Realms Map Editor!\n" +
                " Scroll mouse wheel to choose block.\n" +
                " Press Z for picker mode.\n" +
                " Press X for bucket mode.\n" +
                " Press C to fill map with choosed block.\n" +
                " Press F for rectangle filling mode.\n" +
                " Press S for line filling mode.\n" +
                " Press D to delete ( Toggleable ).\n" +
                " Press A to toggle animations.\n" +
                " TAB to show grid.\n" +
                " Enter to save map.\n" +
                " Space to load map.\n" +
                "Have fun!");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var w = Color.White;
            var t = Color.Transparent;

            TileSet = Content.Load<Texture2D>("MCRTile");

            Entity.SpriteSheets = Content.Load<Texture2D>("Entities");

            Picker = Content.Load<Texture2D>("picker");
            Bucket = Content.Load<Texture2D>("busket");

            GridTile = new Texture2D(GraphicsDevice, 16, 16);
            GridTile.SetData(new Color[]
            {
                w,w,w,w,w,w,w,w,w,w,w,w,w,w,w,w,
                w,t,t,t,t,t,t,t,t,t,t,t,t,t,t,w,
                w,t,t,t,t,t,t,t,t,t,t,t,t,t,t,w,
                w,t,t,t,t,t,t,t,t,t,t,t,t,t,t,w,
                w,t,t,t,t,t,t,t,t,t,t,t,t,t,t,w,
                w,t,t,t,t,t,t,t,t,t,t,t,t,t,t,w,
                w,t,t,t,t,t,t,t,t,t,t,t,t,t,t,w,
                w,t,t,t,t,t,t,t,t,t,t,t,t,t,t,w,
                w,t,t,t,t,t,t,t,t,t,t,t,t,t,t,w,
                w,t,t,t,t,t,t,t,t,t,t,t,t,t,t,w,
                w,t,t,t,t,t,t,t,t,t,t,t,t,t,t,w,
                w,t,t,t,t,t,t,t,t,t,t,t,t,t,t,w,
                w,t,t,t,t,t,t,t,t,t,t,t,t,t,t,w,
                w,t,t,t,t,t,t,t,t,t,t,t,t,t,t,w,
                w,t,t,t,t,t,t,t,t,t,t,t,t,t,t,w,
                w,w,w,w,w,w,w,w,w,w,w,w,w,w,w,w,
            });
            GridPixel = new Texture2D(GraphicsDevice, 1, 1);
            GridPixel.SetData(new Color[] { Color.White });
            // TODO: use this.Content to load your game content here
        }
        class Node
        {
            public Node last;
            public Point pos;
            public int state;
        }

        class NodePool
        {
            private Stack<Node> _pool;

            public NodePool()
            {
                _pool = new Stack<Node>(64);
            }

            public Node Get()
            {
                if (_pool.Count > 0)
                {
                    return _pool.Pop();
                }
                return new Node();
            }

            public void Free(Node node)
            {
                _pool.Push(node);
            }
        }

        NodePool nodePool = new NodePool();
        private void RectangleFill(int startX, int startY, int EndX, int EndY)
        {
            if (EndX - startX < 0)
            {
                int Startxx = startX;
                startX = EndX;
                EndX = Startxx;
            }
            if (EndY - startY < 0)
            {
                int Startyy = startY;
                startY = EndY;
                EndY = Startyy;
            }
            if (DeleteMode)
            {
                for (int x = startX; x <= EndX; x++)
                    for (int y = startY; y <= EndY; y++)
                    {
                        int index = x + y * MapWidth;
                        if (index >= byteMap.Length)
                        {
                            continue;
                        }
                        byteMap[index] = byte.MaxValue;
                        MapTiles[index] = null;
                    }
            }
            else
            {
                for (int x = startX; x <= EndX; x++)
                    for (int y = startY; y <= EndY; y++)
                    {
                        int index = x + y * MapWidth;
                        if (index >= byteMap.Length)
                        {
                            continue;
                        }
                        byteMap[index] = (byte)choosedBlock;
                        MapTiles[index] = new Tile();
                    }
            }
        }
        private void LineFill(float startX, float startY, int EndX, int EndY)
        {
            Vector2 d = new(EndX - startX, EndY - startY);

            while (Math.Round(startY) != EndY || Math.Round(startX) != EndX)
            {
                int index = (int)startX + (int)startY * MapWidth;
                if (index >= byteMap.Length || index < 0)
                {
                    continue;
                }
                byteMap[index] = (byte)choosedBlock;
                MapTiles[index] = new Tile();
                if (startY != EndY)
                {
                    d.Y = EndY - startY;
                    startY += Vector2.Normalize(d).Y;
                }
                if (startX != EndX)
                {
                    d.X = EndX - startX;
                    startX += Vector2.Normalize(d).X;
                }
            }
        }
        private void Fill(int x, int y) // Thanks to akseli for this function <3
        {
            byte tile = byteMap[x + y * MapWidth];
            byte filler = (byte)choosedBlock;

            Point[] states = new Point[4];
            states[0] = new Point(1, 0);
            states[1] = new Point(-1, 0);
            states[2] = new Point(0, 1);
            states[3] = new Point(0, -1);

            Node current = nodePool.Get();
            current.pos = new Point(x, y);
            current.last = null;
            current.state = 0;

            while (current != null)
            {

                if (current.state >= 4)
                {
                    nodePool.Free(current);
                    current = current.last;
                    continue;
                }

                Point state = states[current.state++];
                Point p = new Point(current.pos.X + state.X, current.pos.Y + state.Y);
                int index = p.X + p.Y * MapWidth;
                if (index >= byteMap.Length || index < 0)
                {
                    continue;
                }

                byte newTile = byteMap[index];

                if (newTile != tile || newTile == filler)
                {
                    continue;
                }
                byteMap[index] = filler;
                MapTiles[index] = new Tile();
                var lastNode = current;
                current = nodePool.Get();
                current.state = 0;
                current.last = lastNode;
                current.pos = p;
            }
        }
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (IsActive)
            {
                ScrollVal = MState.ScrollWheelValue; // Get previous scroll value
                MState = Mouse.GetState();
                //Set mouse pos in tiles units ( im not sure what i said lol )
                TMPos.X = (int)(MState.X * _blockSize);
                TMPos.Y = (int)(MState.Y * _blockSize);

                if (ScrollVal < MState.ScrollWheelValue && choosedBlock > 0)
                {
                    choosedBlock--;
                    Tile.nextTileSrcPos = 16 * (choosedBlock % numTiles);
                }
                else if (ScrollVal > MState.ScrollWheelValue)
                {
                    choosedBlock++;
                    Tile.nextTileSrcPos = 16 * (choosedBlock % numTiles);
                }

                if (Mode == Modes.RectangleFill || Mode == Modes.LineFill)
                {
                    RectFill.Width = TMPos.X;
                    RectFill.Height = TMPos.Y;
                }
                if (Anims)
                {
                    for (int i = 0; i < MapTiles.Length; i++)
                    {
                        if (byteMap[i] != 255 && byteMap[i] >= 7)
                        {
                            var tile = MapTiles[i];
                            if ((tile.SrcRect.Y += 16) >= 512) tile.SrcRect.Y = 0;
                        }
                    }
                }

                if (MState.LeftButton == ButtonState.Pressed)
                {
                    var index = TMPos.X + TMPos.Y * MapWidth;

                    if (index < MapTiles.Length && index > -1)
                    {
                        switch (Mode)
                        {
                            case Modes.None:
                                if (!DeleteMode)
                                {
                                    if (choosedBlock < numTiles)
                                    {
                                        byteMap[index] = (byte)choosedBlock;
                                        MapTiles[index] = new Tile();
                                    }
                                    else
                                    {
                                        Entities[index] = new((byte)(choosedBlock - numTiles));
                                    }
                                    break;
                                }
                                else
                                {
                                    byteMap[index] = byte.MaxValue;
                                    MapTiles[index] = null;
                                    Entities[index] = null;
                                    break;
                                }
                            case Modes.Picker:
                                choosedBlock = byteMap[index];

                                Tile.nextTileSrcPos = 16 * (choosedBlock % numTiles);
                                Mouse.SetCursor(MouseCursor.Arrow);

                                Mode = Modes.None;
                                break;
                            case Modes.Bucket:
                                Fill(TMPos.X, (int)(MState.Y * _blockSize));
                                Mouse.SetCursor(MouseCursor.Arrow);

                                Mode = Modes.None;
                                break;
                            case Modes.RectangleFill:
                                RectangleFill(RectFill.X, RectFill.Y, RectFill.Width, RectFill.Height);
                                IsMouseVisible = true;

                                Mode = Modes.None;
                                break;
                            case Modes.LineFill:
                                LineFill(RectFill.X, RectFill.Y, RectFill.Width, RectFill.Height);

                                Mode = Modes.None;
                                break;
                        }
                    }
                }
            }
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        private void Window_TextInput(object sender, TextInputEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Enter:
                    Console.WriteLine("Write map name you want to save (or path)");

                    string path = Console.ReadLine();
                    byte[] mapents = new byte[Entities.Length];

                    for (int i = 0; i < Entities.Length; i++)
                    {
                        if (Entities[i] == null) { mapents[i] = 255; continue; } // Generate entity id map
                        mapents[i] = Entities[i].Id;
                    }

                    nekoT.Map.Save(byteMap, mapents, MapWidth, path);

                    Console.WriteLine($"Map saved in {path}.json");
                    break;

                case Keys.Space:
                    Console.WriteLine("Write map name you want to load");
                    path = Console.ReadLine();
                    nekoT.Map.Load(path);
                    Console.WriteLine($"Map loaded from {path}.json");
                    break;

                case Keys.C:
                    Array.Fill(byteMap, (byte)choosedBlock);
                    Array.Fill(MapTiles, new Tile() { SrcRect = new Rectangle(Tile.nextTileSrcPos, 0, 16, 16) });
                    break;
                    #region removed
                    /*
                default:
                    int a = (int)char.GetNumericValue(e.Character) - 1;
                    if (a > -1)
                    {
                        //Mode = 0;
                        //Mouse.SetCursor(MouseCursor.Arrow);
                        choosedBlock = a < 6 ? a : 0;
                        Tile.nextTileSrcPos = 16 * (choosedBlock % 6);
                    }
                    else if (a >= 6) { Console.WriteLine("Welding... choose value from 1 to 6!"); }
                    break;*/ // removed due scroll is better
                    #endregion
            }
            //Toggle for Picker
            if (e.Key == Keys.Z && Mode != Modes.Picker)
            {
                Mode = Modes.Picker;
                Mouse.SetCursor(MouseCursor.FromTexture2D(Picker, 0, Picker.Height));
                Console.WriteLine("Youre in picker mode ( click on block on map to choose it )");
            }
            //Toggle for Bucket fill mode
            if (e.Key == Keys.X && Mode != Modes.Bucket)
            {
                Mode = Modes.Bucket;
                Mouse.SetCursor(MouseCursor.FromTexture2D(Bucket, 0, Bucket.Height));
                Console.WriteLine("Youre in bucket mode");
            }
            //Toggle for Rectfill
            if (e.Key == Keys.F && Mode != Modes.RectangleFill)
            {
                Mode = Modes.RectangleFill;
                // set starting point
                RectFill.X = TMPos.X;
                RectFill.Y = TMPos.Y;

                Console.WriteLine("Youre in rectangle filling mode");
            }
            //Toggle for linefill
            if (e.Key == Keys.S && Mode != Modes.LineFill)
            {
                Mode = Modes.LineFill;
                // set starting point
                RectFill.X = TMPos.X;
                RectFill.Y = TMPos.Y;

                Console.WriteLine("Youre in line filling mode");
            }
            // Little Toggles
            if (e.Key == Keys.D) DeleteMode = !DeleteMode;
            if (e.Key == Keys.A) Anims = !Anims;
            if (e.Key == Keys.Tab) ShowGrid = !ShowGrid;
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            var blockpos = new Vector2();

            for (int x = 0; x < MapWidth; x++)
                for (int y = 0; y < MapHeight; y++)
                {
                    int index = x + y * MapWidth;
                    // length check removed due its not possible :D
                    blockpos.X = x * blockSize;
                    blockpos.Y = y * blockSize;

                    if (MapTiles[index] is Tile tile)
                    {
                        _spriteBatch.Draw(TileSet, blockpos, tile.SrcRect, Color.White, 0, Vector2.Zero, scale: 1.5625f, 0, 0);
                    }
                    if (Entities[index] is Entity ent)
                    {
                        _spriteBatch.Draw(Entity.SpriteSheets, blockpos, Entity.SRect[ent.Id], Color.White, 0, Vector2.Zero, (Vector2.One * 25) / Entity.SRect[Entities[index].Id].Size.ToVector2(), 0, 0);
                    }
                }
            blockpos.X = TMPos.X * 25; // INSANE OPTIMIZATION FOR YELLOW/RED HIGHLIGHT
            blockpos.Y = TMPos.Y * 25;
            // Grid loop
            if (ShowGrid)
            {
                for (int x = 0; x < GraphicsDevice.Viewport.Width; x += blockSize)
                {
                    // Draw vertical grid line
                    _spriteBatch.Draw(GridPixel, new Rectangle(x, 0, 1, GraphicsDevice.Viewport.Height), Color.White);
                    for (int y = 0; y < GraphicsDevice.Viewport.Height; y += blockSize)
                    {
                        //Draw horizontal grid line
                        _spriteBatch.Draw(GridPixel, new Rectangle(0, y, GraphicsDevice.Viewport.Width, 1), Color.White);
                    }
                }
            }

            if (Mode == Modes.RectangleFill)
            {
                // Copy rectangle because why not
                int EndX = RectFill.Width,
                    EndY = RectFill.Height,
                    startX = RectFill.X,
                    startY = RectFill.Y;

                if (EndX - startX < 0)
                {
                    int Startxx = startX;
                    startX = EndX;
                    EndX = Startxx;
                }
                if (EndY - startY < 0)
                {
                    int Startyy = startY;
                    startY = EndY;
                    EndY = Startyy;
                }
                var v = new Vector2();
                for (int x = startX; x <= EndX; x++)
                    for (int y = startY; y <= EndY; y++)
                    {
                        v.X = x;
                        v.Y = y;
                        _spriteBatch.Draw(GridTile, v * blockSize, null, DeleteMode ? Color.Red : Color.DeepSkyBlue, 0, Vector2.Zero, scale: 1.5625f, 0, 0);
                    }
            }
            else if (Mode == Modes.LineFill)
            {
                _spriteBatch.Draw(GridTile, RectFill.Location.ToVector2() * blockSize, null, Color.Yellow, 0, Vector2.Zero, scale: 1.5625f, 0, 0);
                _spriteBatch.Draw(GridTile, new Vector2(RectFill.Width, RectFill.Height) * blockSize, null, Color.Yellow, 0, Vector2.Zero, scale: 1.5625f, 0, 0);
            }

            else if (Mode == Modes.None) // Draw Yellow/Red highlight
            {
                _spriteBatch.Draw(GridTile, blockpos, null, DeleteMode ? Color.Red : Color.Yellow, 0, Vector2.Zero, scale: 1.5625f, 0, 0);
            }

            var BlockinvPos = new Vector2(Window.ClientBounds.Width - 16 * Entity.SRect.Length - 10, 36); // 10 is small offset, so it look cool :sunglasses:

            _spriteBatch.Draw(Entity.SpriteSheets, new Rectangle((int)BlockinvPos.X, (int)BlockinvPos.Y, 16 * Entity.SRect.Length, 16), Color.White);

            BlockinvPos.X -= TileSet.Width;

            _spriteBatch.Draw(TileSet, BlockinvPos, new(0, 0, TileSet.Width, 16), Color.White);
            _spriteBatch.Draw(GridTile, BlockinvPos + new Vector2(choosedBlock * 16 - 1, -1), null, Color.Yellow, 0, Vector2.Zero, 1.125f, 0, 0);

            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}