using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Janthus.Game.Input;

public class InputManager
{
    private KeyboardState _currentKeyboard;
    private KeyboardState _previousKeyboard;
    private MouseState _currentMouse;
    private MouseState _previousMouse;

    public KeyboardState CurrentKeyboard => _currentKeyboard;
    public MouseState CurrentMouse => _currentMouse;

    public void Update()
    {
        _previousKeyboard = _currentKeyboard;
        _previousMouse = _currentMouse;
        _currentKeyboard = Keyboard.GetState();
        _currentMouse = Mouse.GetState();
    }

    public bool IsKeyPressed(Keys key) =>
        _currentKeyboard.IsKeyDown(key) && _previousKeyboard.IsKeyUp(key);

    public bool IsKeyDown(Keys key) => _currentKeyboard.IsKeyDown(key);

    public bool IsLeftClickPressed() =>
        _currentMouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released;

    public bool IsRightClickPressed() =>
        _currentMouse.RightButton == ButtonState.Pressed && _previousMouse.RightButton == ButtonState.Released;

    public int ScrollDelta => _currentMouse.ScrollWheelValue - _previousMouse.ScrollWheelValue;

    public Point MousePosition => _currentMouse.Position;
}
