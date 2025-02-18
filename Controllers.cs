using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

public enum ControlType
{
    Keyboard1,
    Gamepad1,
    Keyboard2,
    Gamepad2,
}

public enum ControlAction
{
    Accelerate,
    Left,
    Right,
    Missile,
    Chaff,
    Flip,
    Laser,
    ZoomIn,
    ZoomOut,
    Activate,
    Acknowledge
}

[Serializable]
public abstract class Controllers
{
    public ControlType ControlType { get; set; }

    public abstract bool IsActionPressed(ControlAction action);
}

[Serializable]
public class KeyboardControls : Controllers
{
    public Keys AccelerateKey { get; set; }
    public Keys LeftKey { get; set; }
    public Keys RightKey { get; set; }
    public Keys MissileKey { get; set; }
    public Keys ChaffKey { get; set; }
    public Keys FlipKey { get; set; }
    public Keys LaserKey { get; set; }
    public Keys ZoomInKey { get; set; }
    public Keys ZoomOutKey { get; set; }
    public Keys ActivateKey { get; set; }
    public Keys AcknowledgeKey { get; set; }

    public KeyboardControls() { }

    public KeyboardControls(ControlType controlType)
    {
        ControlType = controlType;
        // Setup default keys
        if (controlType == ControlType.Keyboard1)
        {
            AccelerateKey = Keys.Up;
            LeftKey = Keys.Left;
            RightKey = Keys.Right;
            MissileKey = Keys.RightShift;
            ChaffKey = Keys.OemQuestion;
            FlipKey = Keys.OemPeriod;
            LaserKey = Keys.RightControl;
            ZoomInKey = Keys.K;
            ZoomOutKey = Keys.L;
            ActivateKey = Keys.Down;
            AcknowledgeKey = Keys.Enter;
        }
        else
        {
            AccelerateKey = Keys.W;
            LeftKey = Keys.A;
            RightKey = Keys.D;
            MissileKey = Keys.T;
            ChaffKey = Keys.U;
            FlipKey = Keys.I;
            LaserKey = Keys.Y;
            ZoomInKey = Keys.G;
            ZoomOutKey = Keys.H;
            ActivateKey = Keys.E;
            AcknowledgeKey = Keys.Enter;
        }
    }

    public Keys GetKeyForAction(ControlAction action)
    {
        return action switch
        {
            ControlAction.Accelerate => AccelerateKey,
            ControlAction.Left => LeftKey,
            ControlAction.Right => RightKey,
            ControlAction.Missile => MissileKey,
            ControlAction.Chaff => ChaffKey,
            ControlAction.Flip => FlipKey,
            ControlAction.Laser => LaserKey,
            ControlAction.ZoomIn => ZoomInKey,
            ControlAction.ZoomOut => ZoomOutKey,
            ControlAction.Activate => ActivateKey,
            ControlAction.Acknowledge => AcknowledgeKey,
            _ => Keys.None, // Default in case of invalid action
        };
    }

    // Implement abstract method to check if a specific action is pressed
    public override bool IsActionPressed(ControlAction action)
    {
        var keyboardState = Keyboard.GetState();

        return action switch
        {
            ControlAction.Accelerate => keyboardState.IsKeyDown(AccelerateKey),
            ControlAction.Left => keyboardState.IsKeyDown(LeftKey),
            ControlAction.Right => keyboardState.IsKeyDown(RightKey),
            ControlAction.Missile => keyboardState.IsKeyDown(MissileKey),
            ControlAction.Chaff => keyboardState.IsKeyDown(ChaffKey),
            ControlAction.Flip => keyboardState.IsKeyDown(FlipKey),
            ControlAction.Laser => keyboardState.IsKeyDown(LaserKey),
            ControlAction.ZoomIn => keyboardState.IsKeyDown(ZoomInKey),
            ControlAction.ZoomOut => keyboardState.IsKeyDown(ZoomOutKey),
            ControlAction.Activate => keyboardState.IsKeyDown(ActivateKey),
            ControlAction.Acknowledge => keyboardState.IsKeyDown(AcknowledgeKey),
            _ => false,
        };
    }

    public override string ToString()
    {
        return $"Control Type: {ControlType}\n" +
               $"{FormatControl("Accelerate", AccelerateKey)}" +
               $"{FormatControl("Left", LeftKey)}" +
               $"{FormatControl("Right", RightKey)}" +
               $"{FormatControl("Missile", MissileKey)}" +
               $"{FormatControl("Chaff", ChaffKey)}" +
               $"{FormatControl("Flip", FlipKey)}" +
               $"{FormatControl("Laser", LaserKey)}" +
               $"{FormatControl("Zoom In", ZoomInKey)}" +
               $"{FormatControl("Zoom Out", ZoomOutKey)}" +
               $"{FormatControl("Activate", ActivateKey)}" +
               $"{FormatControl("Acknowledge", AcknowledgeKey)}";
    }

    private string FormatControl(string action, Keys key)
    {
        return $"{action,-20}: {key}\n";
    }
}

[Serializable]
public class GamepadControls : Controllers
{
    public Buttons AccelerateButton { get; set; }
    public Buttons MoveStick { get; set; }
    public Buttons MissileButton { get; set; }
    public Buttons ChaffButton { get; set; }
    public Buttons FlipButton { get; set; }
    public Buttons LaserButton { get; set; }
    public Buttons ZoomInButton { get; set; }
    public Buttons ZoomOutButton { get; set; }
    public Buttons ActivateButton { get; set; }
    public PlayerIndex GamepadIndex { get; set; }
    public Buttons AcknowledgeButton { get; set; }
    public GamepadControls() {}
    
    public GamepadControls(ControlType controlType)
    {
        ControlType = controlType;

        // Set default buttons
        MoveStick = Buttons.LeftStick;
        AccelerateButton = Buttons.A;
        MissileButton = Buttons.LeftTrigger;
        LaserButton = Buttons.RightTrigger;
        ChaffButton = Buttons.B;
        FlipButton = Buttons.RightStick;
        ZoomInButton = Buttons.LeftShoulder;
        ZoomOutButton = Buttons.RightShoulder;
        ActivateButton = Buttons.X;
        if (controlType == ControlType.Gamepad1)
            GamepadIndex = PlayerIndex.One;
        else
            GamepadIndex = PlayerIndex.Two;
        AcknowledgeButton = Buttons.Y;
    }

    public Buttons GetButtonForAction(ControlAction action)
    {
        return action switch
        {
            ControlAction.Accelerate => AccelerateButton,
            ControlAction.Left => MoveStick, // Example mapping
            ControlAction.Right => MoveStick, // Example mapping
            ControlAction.Missile => MissileButton,
            ControlAction.Chaff => ChaffButton,
            ControlAction.Flip => FlipButton,
            ControlAction.Laser => LaserButton,
            ControlAction.ZoomIn => ZoomInButton,
            ControlAction.ZoomOut => ZoomOutButton,
            ControlAction.Activate => ActivateButton,
            ControlAction.Acknowledge => AcknowledgeButton,
            _ => 0, // Default for invalid action
        };
    }

    // Implement abstract method to check if a specific action is pressed
    public override bool IsActionPressed(ControlAction action)
    {
        var gamePadState = GamePad.GetState(GamepadIndex);

        return action switch
        {
            ControlAction.Accelerate => gamePadState.IsButtonDown(AccelerateButton),
            ControlAction.Missile => gamePadState.IsButtonDown(MissileButton),
            ControlAction.Chaff => gamePadState.IsButtonDown(ChaffButton),
            ControlAction.Flip => gamePadState.IsButtonDown(FlipButton),
            ControlAction.Laser => gamePadState.IsButtonDown(LaserButton),
            ControlAction.ZoomIn => gamePadState.IsButtonDown(ZoomInButton),
            ControlAction.ZoomOut => gamePadState.IsButtonDown(ZoomOutButton),
            ControlAction.Activate => gamePadState.IsButtonDown(ActivateButton),
            ControlAction.Acknowledge => gamePadState.IsButtonDown(AcknowledgeButton),
            _ => false,
        };
    }
    public override string ToString()
    {
        return $"Control Type: {ControlType}\n" +
                $"{FormatControl("Accelerate", AccelerateButton)}" +
               $"{FormatControl("Move", MoveStick)}" +
               $"{FormatControl("Missile", MissileButton)}" +
               $"{FormatControl("Chaff", ChaffButton)}" +
               $"{FormatControl("Flip", FlipButton)}" +
               $"{FormatControl("Laser", LaserButton)}" +
               $"{FormatControl("Zoom In", ZoomInButton)}" +
               $"{FormatControl("Zoom Out", ZoomOutButton)}" +
               $"{FormatControl("Activate", ActivateButton)}" +
               $"{FormatControl("Gamepad Index", GamepadIndex)}" +
               $"{FormatControl("Acknowledge", AcknowledgeButton)}";
    }

    private string FormatControl(string action, Buttons button)
    {
        return $"{action,-20}: {button}\n";
    }

    private string FormatControl(string action, PlayerIndex index)
    {
        return $"{action,-20}: {index}\n";
    }

}
