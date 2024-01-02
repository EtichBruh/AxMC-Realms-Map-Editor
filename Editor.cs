using AxMC_Realms_Client.Map;
using AxMC_Realms_ME.Graphics;
using AxMC_Realms_ME.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace AxMC_Realms_ME
{
    static class ext
    {
        public static Point Divide(this Point a, int b)
        {
            a.X /= b;
            a.Y /= b;
            return a;
        }
    }
    public class Editor : Game
    {
        public static JsonSerializerOptions JsonOptions = new() { IncludeFields = true, WriteIndented = true };

        public static Tile[] MapTiles;
        public static Vector2[] MapBlocks;
        public static int MapWidth = 256, MapHeight = 256;
        public static byte[] byteMap;
        public static Entity[] Entities;
        public static int numTiles = 18;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        const int blockSize = 50;
        const float _blockSize = 1f / blockSize;
        const float bSscale = blockSize / 16f;// block size scale factor

        bool ShowGrid;
        bool Anims;
        bool DeleteMode;
        int ScrollVal;
        int choosedBlock = 0;
        int cx;
        int cy;
        Modes Mode = 0;

        Rectangle RectFill;
        MouseState MState;
        Point TMPos; // Tiled mouse Pos

        Texture2D GridTile, GridPixel;
        Texture2D TileSet; //environment nvm no environment here ! its entity now, because im lazy to make it like that
        Texture2D Picker, Bucket;
        SpriteFont Font;
        public Editor()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.TextInput += Window_TextInput;
            Window.ClientSizeChanged += OnResize;

            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.ApplyChanges();

            Window.AllowUserResizing = true;
        }

        private void OnResize(object sender, EventArgs e)
        {
            int w = Camera.View.Width, h = Camera.View.Height;
            Camera.View = GraphicsDevice.Viewport;
            Camera.Position.X += (Camera.View.Width - w) / 2;
            Camera.Position.Y += (Camera.View.Height - h) / 2;

            cx = GraphicsDevice.Viewport.Width / 16;
            cy = GraphicsDevice.Viewport.Height / 16;
        }

        protected override void Initialize()
        {

            // init map arrays
            byteMap = new byte[MapWidth * MapHeight];
            MapTiles = new Tile[byteMap.Length];
            Entities = new Entity[byteMap.Length];

            // fill byte map array with 255 ,because 255 is null tile
            Array.Fill<byte>(byteMap, 255);

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

            Entity.Load("GameData/EntityData.json");
            Tile.Initialize("GameData/Tiles.json");

            Camera.Init(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            OnResize(null, null);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var w = Color.White;
            var t = Color.Transparent;

            TileSet = Content.Load<Texture2D>("Assets");

            Picker = Content.Load<Texture2D>("picker");
            Bucket = Content.Load<Texture2D>("busket");

            Font = Content.Load<SpriteFont>("Font");

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
            }); // embedded texture 
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
                int swapx = startX;
                startX = EndX;
                EndX = swapx;
            }
            if (EndY - startY < 0)
            {
                int swapy = startY;
                startY = EndY;
                EndY = swapy;
            }
            if (DeleteMode)
            {
                for (int x = startX; x <= EndX; x++)
                    for (int y = startY; y <= EndY; y++)
                    {
                        int index = x + y * MapWidth;
                        if (index >= byteMap.Length)
                            continue;

                        byteMap[index] = byte.MaxValue;
                        MapTiles[index] = null;
                        Entities[index] = null;
                    }
            }
            else
            {
                for (int x = startX; x <= EndX; x++)
                    for (int y = startY; y <= EndY; y++)
                    {
                        int index = x + y * MapWidth;
                        if (index >= byteMap.Length)
                            continue;

                        byteMap[index] = (byte)choosedBlock;
                        MapTiles[index] = new Tile();
                    }
            }
        }
        private void LineFill(float startX, float startY, int EndX, int EndY)
        {
            Vector2 d = Vector2.Normalize(new(EndX - startX, EndY - startY));
            int RX = (int)startX, RY = (int)startY;// rounded startX and startY

            while (RY != EndY || RX != EndX)
            {
                int index = RX + RY * MapWidth;
                if (index >= byteMap.Length || index < 0)
                    return;

                byteMap[index] = (byte)choosedBlock;
                MapTiles[index] = new Tile();
                
                if (startY != EndY)
                {
                    startY += d.Y;
                    RY = (int)Math.Round(startY);
                }
                if (startX != EndX)
                {
                    startX += d.X;
                    RX = (int)Math.Round(startX);
                }
            }
        }
        private void Fill(int x, int y) // Thanks to akseli for this function <3
        {
            byte tile = byteMap[x + y * MapWidth];
            byte filler = (byte)choosedBlock;

            Point[] states = [new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1)];
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
                Point p = new(current.pos.X + state.X, current.pos.Y + state.Y);
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
            if (!IsActive) return;
            ScrollVal = MState.ScrollWheelValue; // Get previous scroll value
            var pos = MState.Position;
            MState = Mouse.GetState();

            if (MState.RightButton == ButtonState.Pressed)
            {
                Camera.Position += pos - MState.Position;
            }
            //Set mouse pos in tiles units ( im not sure what i said lol )

            if (ScrollVal < MState.ScrollWheelValue)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                {
                    Camera.Zoom += 0.02f;
                }
                else if (choosedBlock > 0)
                {
                    choosedBlock--;
                    Tile.NextTileSrcPos = 16 * (choosedBlock % numTiles);
                }
            }
            else if (ScrollVal > MState.ScrollWheelValue)
            {

                if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                {
                    Camera.Zoom -= 0.02f;
                }
                else if (choosedBlock + 1 < numTiles + Entity.Data.Length)
                {
                    choosedBlock++;
                    Tile.NextTileSrcPos = 16 * (choosedBlock % numTiles);
                }
                else
                {
                    choosedBlock = 0;
                }
            }
            Camera.Follow();
            TMPos = (Vector2.Transform(MState.Position.ToVector2(), Matrix.Invert(Camera.Transform)) * _blockSize).ToPoint();

            if (Mode == Modes.RectangleFill || Mode == Modes.LineFill)
            {
                RectFill.Width = TMPos.X;
                RectFill.Height = TMPos.Y;
            }

            if (Anims)
                for (int i = 0; i < MapTiles.Length; i++)
                    if (byteMap[i] != 255 && (byteMap[i] == 5 || byteMap[i] == 6))
                    {
                        var tile = MapTiles[i];
                        if ((tile.SrcRect.Y += 16) >= 512)
                            tile.SrcRect.Y = 0;
                    }

            if (MState.LeftButton == ButtonState.Pressed)
            {
                var index = TMPos.X + TMPos.Y * MapWidth;

                if (index < MapTiles.Length && index > -1)
                    switch (Mode)
                    {
                        case Modes.None:
                            if (DeleteMode)
                            {
                                byteMap[index] = byte.MaxValue;
                                MapTiles[index] = null;
                                Entities[index] = null;
                                break;
                            }
                            if (choosedBlock < numTiles)
                            {
                                byteMap[index] = (byte)choosedBlock;
                                MapTiles[index] = new Tile();
                                break;
                            }
                            Entities[index] = new((byte)(choosedBlock - numTiles));
                            break;

                        case Modes.Picker:
                            choosedBlock = byteMap[index];

                            Tile.NextTileSrcPos = 16 * (choosedBlock % numTiles);
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
                    if (string.IsNullOrEmpty(path))
                    {
                        Console.WriteLine("Wrong map name");
                        break;
                    }
                    byte[] mapents = new byte[Entities.Length];

                    for (int i = 0; i < Entities.Length; i++)
                    {
                        if (Entities[i] == null) { mapents[i] = 255; continue; } // Generate entity id map
                        mapents[i] = Entities[i].Id;
                    }

                    nekoT.Map.Save(byteMap, mapents, MapWidth, path);

                    Console.WriteLine($"Map saved in {path}.bm");
                    break;

                case Keys.Space:
                    Console.WriteLine("Write map name you want to load");
                    path = Console.ReadLine();
                    if (path == "")
                    {
                        Console.WriteLine("Wrong map name");
                        break;
                    }
                    nekoT.Map.Load(path);
                    Console.WriteLine($"Map loaded from {path}.bm");
                    break;

                case Keys.C:
                    Array.Fill(byteMap, (byte)choosedBlock);
                    for (int i = 0; i < MapTiles.Length; i++) MapTiles[i] = new Tile();
                    break;
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

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Camera.Transform);

            var blockpos = new Vector2();

            //int camx = Math.Max(0, (int)(Camera.TPos.X - Camera.ScaleFactor));
            //int camy = Math.Max(0, (int)(Camera.TPos.Y - Camera.ScaleFactor));

            for (int x = 0; x < MapWidth; x++)
                for (int y = 0; y < MapHeight; y++)
                {
                    int index = x + y * MapWidth;
                    // length check removed due its not possible to trigger :D
                    blockpos.X = x * blockSize;
                    blockpos.Y = y * blockSize;

                    if (MapTiles[index] is Tile tile)
                    {
                        _spriteBatch.Draw(TileSet, blockpos, tile.SrcRect, Color.White, 0, Vector2.Zero, bSscale, 0, 0);
                    }
                    if (Entities[index] is Entity ent)
                    {
                        _spriteBatch.Draw(TileSet, new Rectangle((int)blockpos.X, (int)blockpos.Y, 50, 50), Entity.SRect[ent.Id], Color.White);
                    }
                }

            if (ShowGrid)
            {
                int temp = (int)Math.Ceiling(Camera.ScaleFactor);
                for (int x = 0; x < MapWidth; x++)
                {
                    // Draw vertical grid line
                    _spriteBatch.Draw(GridPixel, new Rectangle(x * 50, 0, temp, MapHeight * 50), Color.White);
                    //Draw horizontal grid line
                    _spriteBatch.Draw(GridPixel, new Rectangle(0, x * 50, MapWidth * 50, temp), Color.White);
                }
                for (int x = 0; x < byteMap.Length; x++)
                {
                    _spriteBatch.DrawString(Font, x.ToString(), new Vector2(x % MapWidth * blockSize + blockSize * 0.5f, x / MapWidth * blockSize + blockSize * 0.5f), Color.White, 0, Font.MeasureString(x.ToString()) * 0.5f, 1, 0, 0);
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
                        _spriteBatch.Draw(GridTile, v * blockSize, null, DeleteMode ? Color.Red : Color.DeepSkyBlue, 0, Vector2.Zero, bSscale, 0, 0);
                    }
            }
            else if (Mode == Modes.LineFill)
            {
                float EndX = RectFill.Width,
                    EndY = RectFill.Height,
                    startX = RectFill.X,
                    startY = RectFill.Y;
                int RX = (int)startX, RY = (int)startY;

                Vector2 d = Vector2.Normalize(new(EndX - startX, EndY - startY));

                while (RY != EndY || RX != EndX)
                {
                    _spriteBatch.Draw(GridTile, new Vector2(RX, RY) * blockSize, null, Color.Yellow, 0, Vector2.Zero, bSscale, 0, 0);
                    if (startY != EndY)
                    {
                        startY += d.Y;
                        RY = (int)Math.Round(startY);
                    }
                    if (startX != EndX)
                    {
                        startX += d.X;
                        RX = (int)Math.Round(startX);
                    }
                }
            }

            else if (Mode == Modes.None) // Draw Yellow/Red highlight
            {
                _spriteBatch.Draw(GridTile, TMPos.ToVector2() * blockSize - Vector2.One, null, DeleteMode ? Color.Red : Color.Yellow, 0, Vector2.Zero, 3.25f, 0, 0);
            }

            _spriteBatch.End();

            // UI spritebatch
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            var BlockinvPos = new Vector2(Window.ClientBounds.Width - 16 * Entity.SRect.Length - 10, 36); // 10 is small offset, so it look cool :sunglasses:

            var offset = 0;
            for (int i = 0; i < Entity.SRect.Length; i++)
            {
                _spriteBatch.Draw(TileSet, BlockinvPos + new Vector2(offset, 0), Entity.SRect[i], Color.White);
                offset += 16;
            }
            //_spriteBatch.Draw(Entity.SpriteSheets, new Rectangle((int)BlockinvPos.X, (int)BlockinvPos.Y, 16 * Entity.SRect.Length, 16), Color.White);

            BlockinvPos.X -= TileSet.Width;

            _spriteBatch.Draw(TileSet, BlockinvPos, new(0, 0, TileSet.Width, 16), Color.White);
            _spriteBatch.Draw(GridTile, BlockinvPos + new Vector2(choosedBlock * 16 - 1, -1), null, Color.Yellow, 0, Vector2.Zero, 1.125f, 0, 0);

            _spriteBatch.DrawString(Font, TMPos.ToString(), Vector2.Zero, Color.Black);
            _spriteBatch.DrawString(Font, Camera.Position.ToString(), new Vector2(0, 12), Color.Black);
            _spriteBatch.DrawString(Font, Camera.TPos.ToString() + " Camera Zoom: " + Camera.ScaleFactor.ToString(), new Vector2(0, 26), Color.Black);

            _spriteBatch.DrawString(Font, choosedBlock < numTiles ? Tile.Data[choosedBlock].Name : Entity.Data[choosedBlock - numTiles].Name, new Vector2(0, 38), Color.Black);

            if (Mode == Modes.RectangleFill)
            {
                _spriteBatch.DrawString(Font, RectFill.ToString(), new Vector2(0, 38), Color.Black);
            }

            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}