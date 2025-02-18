using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

public static class ControlsManager
{
    // Save any controller type to an XML file (Keyboard or Gamepad)
    public static void SaveControls<T>(string filePath, T controls) where T : Controllers
    {
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            serializer.Serialize(writer, controls);
        }
    }

    // Load any controller type from an XML file (Keyboard or Gamepad)
    public static T LoadControls<T>(string filePath, Func<T> getDefaultControls) where T : Controllers
    {
        if (File.Exists(filePath))
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (StreamReader reader = new StreamReader(filePath))
                {
                    return (T)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex) 
            { 
                Debug.WriteLine(ex.Message);
            }
        }      

        // If file doesn't exist, return default controls
        return getDefaultControls();

    }

    // Save all player controls
    public static void SaveAllPlayerControls(Controllers[] playerControllers)
    {
        string configDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Controls");
        Directory.CreateDirectory(configDirectory); // Ensure the directory exists

        for (int i = 0; i < playerControllers.Length; i++)
        {
            string controlType = playerControllers[i].ControlType.ToString();
            string filePath = Path.Combine(configDirectory, $"{controlType}_controls.xml");

            // Save the controller based on its type
            if (playerControllers[i] is KeyboardControls keyboardControls)
            {
                SaveControls(filePath, keyboardControls);
            }
            else if (playerControllers[i] is GamepadControls gamepadControls)
            {
                SaveControls(filePath, gamepadControls);
            }
        }
    }

    // Load all player controls, or use defaults if files don't exist
    public static Controllers[] LoadAllPlayerControls()
    {
        string configDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Controls");
        Directory.CreateDirectory(configDirectory); // Ensure the directory exists

        // Create array to hold player controllers
        Controllers[] playerControllers = new Controllers[4];

        // Load or create default controls for each player
        playerControllers[0] = LoadControls(
            Path.Combine(configDirectory, "Keyboard1_controls.xml"),
            () => new KeyboardControls(ControlType.Keyboard1)
        );

        playerControllers[1] = LoadControls(
            Path.Combine(configDirectory, "Gamepad1_controls.xml"),
            () => new GamepadControls(ControlType.Gamepad1)
        );

        playerControllers[2] = LoadControls(
            Path.Combine(configDirectory, "Keyboard2_controls.xml"),
            () => new KeyboardControls(ControlType.Keyboard2)
        );

        playerControllers[3] = LoadControls(
            Path.Combine(configDirectory, "Gamepad2_controls.xml"),
            () => new GamepadControls(ControlType.Gamepad2)
        );

        return playerControllers;
    }
}
