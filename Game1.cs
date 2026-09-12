using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameStage1;

public struct PlacedBlock
{
    public Vector2 Position;
    public Texture2D Texture;
}

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _black_square;
    private Texture2D _grass;
    private Texture2D _body;
    private Texture2D _plank;
    private Texture2D _cobblestone;
    private Texture2D _log;

    private Vector2 _bodyPosition = new Vector2(400, 400);
    private float _velocityY = 0f;
    private bool _isOnGround = true;

    private const int GridSize = 32;
    private List<PlacedBlock> _placedBlocks = new List<PlacedBlock>();

    private bool _wasMouseDownLastFrame = false;
    private bool _wasRightMouseDownLastFrame = false;

    private Rectangle _selectedIconRectangle = new Rectangle(0, 0, GridSize, GridSize);
    private Texture2D _selectedTexture;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _black_square = Content.Load<Texture2D>("square");
        _grass = Content.Load<Texture2D>("image");
        _body = Content.Load<Texture2D>("body");
        _plank = Content.Load<Texture2D>("plank");
        _cobblestone = Content.Load<Texture2D>("cobblestone");
        _log = Content.Load<Texture2D>("log");

        _selectedTexture = _plank;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var keyboardState = Keyboard.GetState();

        // --- Block type selection ---
        if (keyboardState.IsKeyDown(Keys.D1))
            _selectedTexture = _plank;

        if (keyboardState.IsKeyDown(Keys.D2))
            _selectedTexture = _cobblestone;

        if (keyboardState.IsKeyDown(Keys.D3))
            _selectedTexture = _log;

        // --- Horizontal movement, then check block collision ---
        if (keyboardState.IsKeyDown(Keys.Right))
            _bodyPosition.X += 3;

        if (keyboardState.IsKeyDown(Keys.Left))
            _bodyPosition.X -= 3;

        Rectangle bodyRectangle = new Rectangle((int)_bodyPosition.X, (int)_bodyPosition.Y, 64, 64);

        foreach (PlacedBlock block in _placedBlocks)
        {
            Rectangle blockRectangle = new Rectangle((int)block.Position.X, (int)block.Position.Y, GridSize, GridSize);

            if (bodyRectangle.Intersects(blockRectangle))
            {
                if (bodyRectangle.Center.X < blockRectangle.Center.X)
                    _bodyPosition.X = blockRectangle.Left - 64;
                else
                    _bodyPosition.X = blockRectangle.Right;

                bodyRectangle = new Rectangle((int)_bodyPosition.X, (int)_bodyPosition.Y, 64, 64);
            }
        }

        // --- Jump ---
        if (keyboardState.IsKeyDown(Keys.Up) && _isOnGround)
        {
            _velocityY = -8f;
            _isOnGround = false;
        }

        // --- Gravity ---
        _velocityY += 0.3f;
        _bodyPosition.Y += _velocityY;

        // --- Vertical block collision ---
        bodyRectangle = new Rectangle((int)_bodyPosition.X, (int)_bodyPosition.Y, 64, 64);
        _isOnGround = false;

        foreach (PlacedBlock block in _placedBlocks)
        {
            Rectangle blockRectangle = new Rectangle((int)block.Position.X, (int)block.Position.Y, GridSize, GridSize);

            if (bodyRectangle.Intersects(blockRectangle))
            {
                if (_velocityY > 0)
                {
                    _bodyPosition.Y = blockRectangle.Top - 64;
                    _velocityY = 0f;
                    _isOnGround = true;
                }
                else if (_velocityY < 0)
                {
                    _bodyPosition.Y = blockRectangle.Bottom;
                    _velocityY = 0f;
                }

                bodyRectangle = new Rectangle((int)_bodyPosition.X, (int)_bodyPosition.Y, 64, 64);
            }
        }

        // --- Walls ---
        if (_bodyPosition.X < 0)
            _bodyPosition.X = 0;

        if (_bodyPosition.X > 800 - 64)
            _bodyPosition.X = 800 - 64;

        if (_bodyPosition.Y < 0)
        {
            _bodyPosition.Y = 0;
            _velocityY = 0f;
        }

        // --- Ground ---
        if (_bodyPosition.Y > 448 - 64)
        {
            _bodyPosition.Y = 448 - 64;
            _velocityY = 0f;
            _isOnGround = true;
        }

        // --- Block placement (left click) and breaking (right click) ---
        var mouseState = Mouse.GetState();
        int snappedX = (mouseState.X / GridSize) * GridSize;
        int snappedY = (mouseState.Y / GridSize) * GridSize;
        Vector2 snappedPosition = new Vector2(snappedX, snappedY);

        Rectangle snappedRectangle = new Rectangle(snappedX, snappedY, GridSize, GridSize);
        bool isOverSelectedIcon = snappedRectangle == _selectedIconRectangle;

        bool isMouseDownNow = mouseState.LeftButton == ButtonState.Pressed;

        if (isMouseDownNow && !_wasMouseDownLastFrame && !isOverSelectedIcon)
        {
            PlacedBlock newBlock = new PlacedBlock { Position = snappedPosition, Texture = _selectedTexture };
            _placedBlocks.Add(newBlock);
        }

        _wasMouseDownLastFrame = isMouseDownNow;

        bool isRightMouseDownNow = mouseState.RightButton == ButtonState.Pressed;

        if (isRightMouseDownNow && !_wasRightMouseDownLastFrame)
        {
            _placedBlocks.RemoveAll(b => b.Position == snappedPosition);
        }

        _wasRightMouseDownLastFrame = isRightMouseDownNow;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        for (int i = 0; i < 25; i++)
        {
            _spriteBatch.Draw(_grass, new Rectangle(i * 32, 448, 32, 32), Color.White);
        }

        foreach (PlacedBlock block in _placedBlocks)
        {
            _spriteBatch.Draw(block.Texture, new Rectangle((int)block.Position.X, (int)block.Position.Y, GridSize, GridSize), Color.White);
        }

        _spriteBatch.Draw(_body, new Rectangle((int)_bodyPosition.X, (int)_bodyPosition.Y, 64, 64), Color.White);

        var mouseState = Mouse.GetState();
        int previewX = (mouseState.X / GridSize) * GridSize;
        int previewY = (mouseState.Y / GridSize) * GridSize;
        _spriteBatch.Draw(_selectedTexture, new Rectangle(previewX, previewY, GridSize, GridSize), Color.White * 0.5f);

        _spriteBatch.Draw(_selectedTexture, _selectedIconRectangle, Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}