namespace SparkEngine;

internal class Gamepad
{
    public static float Axis(Axis axis)
    {
        return Program.Instance.JoystickStates.First().GetAxis((int)axis);
    }

    public static bool Down(Buttons button)
    {
        return Program.Instance.JoystickStates.First().IsButtonDown((int)button);
    }
}
public enum Axis
{
    LX,
    LY,
    RX,
    L2,
    R2,
    RY
}
public enum Buttons
{
    B0,
    B1,
    Square,
    Cross,
    Circle,
    Triangle,
    L1,
    R1,
    L2,
    R2,
    Share,
    Options,
    L3,
    R3,
    B12, //idk
    Touchpad,
    Up,
    Right,
    Down,
    Left
}