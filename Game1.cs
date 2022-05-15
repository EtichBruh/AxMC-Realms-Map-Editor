using AxMC_Realms_Client.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace AxMC_Realms_ME
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public static Tile[] MapTiles;
        int choosedBlock = 0;
        public static Vector2[] MapBlocks;
        public static int MapWidth = 256;
        private int blockSize = 25;
        private float _blockSize;
        public static byte[] byteMap;
        byte[] Entities;
        bool ShowGrid;
        Modes Mode = 0;
        Vector2 choosedBlockPos = -Vector2.One;
        Rectangle RectFill;
        int ScrollVal;
        MouseState MState;
        /// <summary>
        /// Tiled mouse pos
        /// </summary>
        Point TMPos;
        Texture2D GridTile, GridPixel;
        Texture2D TileSet, EntsSet;
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
            // TODO: Add your initialization logic here
            byteMap = new byte[MapWidth * MapWidth];
            MapTiles = new Tile[byteMap.Length];
            Array.Fill<byte>(byteMap, 255);
            _blockSize = 1f / blockSize;
            Console.WriteLine("Welcome to the AxMC Realms Map Editor!\n" +
                " Choose value from 1 to 6 to switch tiles.\n" +
                " Press Z to activate picker mode.\n" +
                " Press X for bucket mode.\n" +
                " Press C to full fill map with choosed block.\n" +
                " Press F to activate rectangle filling mode.\n Press S to activate line filling mode.\n" +
                " TAB to show grid.\n" +
                " Enter to save map.\n" +
                " Space to load map.\n Have fun!");
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            var w = Color.White;
            var t = Color.Transparent;
            TileSet = Content.Load<Texture2D>("MCRTile");// it probably try network sorry
            Picker = Content.Load<Texture2D>("picker");// it probably try network sorry
            Bucket = Content.Load<Texture2D>("busket");// it probably try network sorry
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
            for (int x = startX; x <= EndX; x++)
                for (int y = startY; y <= EndY; y++)
                {
                    int index = x + y * MapWidth;
                    if (index >= byteMap.Length || index < 0)
                    {
                        continue;
                    }
                    byteMap[index] = (byte)choosedBlock;
                    MapTiles[index] = new Tile();
                    MapTiles[index].SrcRect.X = Tile.nextTileSrcPos;
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
                MapTiles[index].SrcRect.X = Tile.nextTileSrcPos;
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
        private void Fill(int x, int y)
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
                MapTiles[index].SrcRect.X = Tile.nextTileSrcPos;
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
                ScrollVal = MState.ScrollWheelValue;
                MState = Mouse.GetState();
                TMPos.X = (int)(MState.X * _blockSize);
                TMPos.Y = (int)(MState.Y * _blockSize);
                if(ScrollVal < MState.ScrollWheelValue && choosedBlock - 1 > -1)
                {
                    choosedBlock--;
                    Tile.nextTileSrcPos = 16 * (choosedBlock % 6);
                }
                else if(ScrollVal > MState.ScrollWheelValue )
                {
                    choosedBlock++;
                    Tile.nextTileSrcPos = 16 * (choosedBlock % 6);
                }
                if (Mode == Modes.RectangleFill || Mode == Modes.LineFill)
                {
                    RectFill.Width = TMPos.X;
                    RectFill.Height = TMPos.Y;
                }
                if (MState.LeftButton == ButtonState.Pressed)
                {
                    var index = TMPos.X + TMPos.Y * MapWidth;
                    if (index < MapTiles.Length && index > -1)
                    {
                        switch (Mode)
                        {
                            case Modes.None:
                                byteMap[index] = (byte)choosedBlock;
                                MapTiles[index] = new Tile();
                                MapTiles[index].SrcRect.X = Tile.nextTileSrcPos;
                                break;
                            case Modes.Picker:
                                choosedBlock = byteMap[index];
                                Tile.nextTileSrcPos = 16 * (choosedBlock % 6);
                                Console.WriteLine(choosedBlock);
                                Mouse.SetCursor(MouseCursor.Arrow);
                                break;
                            case Modes.Bucket:
                                Fill(TMPos.X, (int)(MState.Y * _blockSize));
                                Mouse.SetCursor(MouseCursor.Arrow);
                                break;
                            case Modes.RectangleFill:
                                RectangleFill(RectFill.X, RectFill.Y, RectFill.Width, RectFill.Height);
                                IsMouseVisible = true;
                                break;
                            case Modes.LineFill:
                                LineFill(RectFill.X, RectFill.Y, RectFill.Width, RectFill.Height);
                                break;
                        }
                        Mode = Modes.None;
                    }
                    /*if (Mode == 0 && index < MapTiles.Length && index > -1 && byteMap[index] != (byte)choosedBlock)
                    {

                    }*/
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
                    Console.WriteLine("Write map name you want to save");
                    string path = Console.ReadLine();
                    Map.Save(byteMap, MapWidth, path);
                    Console.WriteLine($"Map saved in {path}.json");
                    break;
                case Keys.Space:
                    Console.WriteLine("Write map name you want to load");
                    path = Console.ReadLine();
                    Map.Load(path);
                    Console.WriteLine($"Map loaded from {path}.json");
                    break;
                case Keys.C:
                    Array.Fill(byteMap, (byte)choosedBlock);
                    Array.Fill(MapTiles, new Tile() { SrcRect = new Rectangle(Tile.nextTileSrcPos, 0, 16, 16) });
                    break;
                default:
                    int a = (int)char.GetNumericValue(e.Character) - 1;
                    if (a > -1)
                    {
                        //Mode = 0;
                        //Mouse.SetCursor(MouseCursor.Arrow);
                        choosedBlock = a < 6 ? a : 0;
                        Tile.nextTileSrcPos = 16 * (choosedBlock % 6);
                    }
                    else if (a >=6 ){ Console.WriteLine("Welding... choose value from 1 to 6!"); }
                    break;
            }
            if (!ShowGrid && (e.Key == Keys.Tab))
            {
                ShowGrid = true;
                Console.WriteLine("Tile grid is being shown!");
            }
            else if (ShowGrid && e.Key == Keys.Tab)
            {
                ShowGrid = false;
                Console.WriteLine("Tile grid is hidden.");
            }
            if (e.Key == Keys.Z && Mode != Modes.Picker)
            {
                Mode = Modes.Picker;
                Mouse.SetCursor(MouseCursor.FromTexture2D(Picker, 0, Picker.Height));
                Console.WriteLine("Youre in picker mode ( click on block on map to choose it, dumbass )");
            }
            if (e.Key == Keys.X && Mode != Modes.Bucket)
            {
                Mode = Modes.Bucket;
                Mouse.SetCursor(MouseCursor.FromTexture2D(Bucket, 0, Bucket.Height));
                Console.WriteLine("Youre in bucket mode");
            }
            if (e.Key == Keys.F && Mode != Modes.RectangleFill)
            {
                Mode = Modes.RectangleFill;
                RectFill.X = TMPos.X;
                RectFill.Y = TMPos.Y;
                Console.WriteLine("Youre in rectangle filling mode");
            }
            if (e.Key == Keys.S && Mode != Modes.LineFill)
            {
                Mode = Modes.LineFill;
                RectFill.X = TMPos.X;
                RectFill.Y = TMPos.Y;
                Console.WriteLine("Youre in line filling mode");
            }
            if (e.Key == Keys.D && Mode != Modes.Delete)
            {
                Mode = Modes.Delete;
                RectFill.X = TMPos.X;
                RectFill.Y = TMPos.Y;
                Console.WriteLine("Youre in deleting mode");
            }


        }
        bool gridblockfound;
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            for (int x = 0; x < MapWidth; x++)
                for (int y = 0; y < MapWidth; y++)
                {
                    int index = x + y * MapWidth;
                    if (index > MapTiles.Length || MapTiles[index] is null) continue;
                    _spriteBatch.Draw(TileSet, new Vector2(x,y) * blockSize, MapTiles[index].SrcRect, Color.White, 0, Vector2.Zero, scale: 1.5625f, 0, 0);
                }
            for (int x = 0; x < GraphicsDevice.Viewport.Width; x += blockSize)
            {

                for (int y = 0; y < GraphicsDevice.Viewport.Height; y += blockSize)
                {
                    if (ShowGrid)
                    {
                        _spriteBatch.Draw(GridPixel, new Rectangle(0, y, GraphicsDevice.Viewport.Width, 1), Color.White);
                    }
                    if (!gridblockfound)
                    {
                        if (gridblockfound = (TMPos.X == x * _blockSize && TMPos.Y == y * _blockSize))
                        {
                            choosedBlockPos.X = x;
                            choosedBlockPos.Y = y;
                            continue;
                        }
                    }
                }
                if (ShowGrid)
                {
                    _spriteBatch.Draw(GridPixel, new Rectangle(x, 0, 1, GraphicsDevice.Viewport.Height), Color.White);
                }
            }
            if (gridblockfound && Mode == Modes.None)
            {
                _spriteBatch.Draw(GridTile, choosedBlockPos, null, Color.Yellow, 0, Vector2.Zero, scale: 1.5625f, 0, 0);
                gridblockfound = false;
            }
            else if (Mode == Modes.RectangleFill)
            {
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
                        _spriteBatch.Draw(GridTile, v * blockSize, null, Color.DeepSkyBlue, 0, Vector2.Zero, scale: 1.5625f, 0, 0);
                    }
            }
            else if (Mode == Modes.LineFill)
            {

                _spriteBatch.Draw(GridTile, RectFill.Location.ToVector2() * blockSize, null, Color.Yellow, 0, Vector2.Zero, scale: 1.5625f, 0, 0);
                _spriteBatch.Draw(GridTile, new Vector2(RectFill.Width, RectFill.Height) * blockSize, null, Color.Yellow, 0, Vector2.Zero, scale: 1.5625f, 0, 0);
            }
            _spriteBatch.Draw(TileSet, new Vector2(Window.ClientBounds.Width - TileSet.Width - 10, 90), Color.White);
            _spriteBatch.Draw(GridTile, new Rectangle(Window.ClientBounds.Width - TileSet.Width - 11 + choosedBlock * 16, 89, 18, 18), Color.Yellow);
            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}