<!-- Language Switcher -->
[Türkçe](README.tr.md) | **English**

# Thermal Watcher

A lightweight Windows application designed to monitor your CPU and GPU temperatures in real-time, displaying them through a minimalistic and customizable overlay.

## Features

*   **Real-time Monitoring:** Tracks CPU and GPU temperatures using the [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) library.
*   **Minimal Overlay:** Displays temperatures in a small, unobtrusive overlay panel, typically in the top-right corner of the screen.
*   **System Tray Integration:** Runs conveniently in the system tray with a context menu for quick access.
*   **Customizable Settings:**
    *   **Update Intervals:** Set different refresh rates for when the overlay is visible versus hidden (Auto-Hide mode).
    *   **Temperature Thresholds:** Define temperature ranges (e.g., <50°C, 50-70°C, >70°C).
    *   **Color Coding:** Assign distinct colors (Low, Mid, High) to temperature ranges for easy visual feedback.
    *   **Auto-Hide:** Automatically hide the overlay when the mouse is not near it.
    *   **Hover to Show:** Optionally show the overlay automatically when the mouse enters the overlay's "hot zone" (only when Auto-Hide is enabled).
    *   **Hide Delay:** Configure how long to wait before hiding the overlay after the mouse leaves the hot zone.
*   **High-Temperature Override:** Automatically keeps the overlay visible and updates frequently if temperatures exceed the 'High' threshold, regardless of Auto-Hide settings.
*   **Settings Persistence:** Saves your preferences to the Windows Registry (`HKEY_CURRENT_USER\\Software\\ThermalApp`) so they are remembered across application restarts.

## Installation & Usage

1.  **Download:** Get the latest `Thermal.exe` file from the **[Releases](https://github.com/emi-ran/Thermal-Watcher/releases/)** page.
2.  **Run:** Execute `Thermal.exe`. It's recommended to run it **as Administrator** to ensure it can access hardware sensor data correctly.
3.  **System Tray:** The application icon will appear in your system tray. Right-click it to:
    *   **Settings...:** Open the settings window to customize behavior and appearance.
    *   **Auto-Hide:** Toggle the auto-hide feature on or off.
    *   **Exit:** Close the application.

## Dependencies

*   **.NET 9 Desktop Runtime:** If you download the *framework-dependent* release, you need to have the .NET 9 Desktop Runtime installed. The *self-contained* release includes the runtime but is larger. You can download the runtime from [Microsoft's .NET website](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).
*   **LibreHardwareMonitorLib:** This library is used for accessing sensor data. It's included in the project via NuGet.

## Building from Source

1.  Clone the repository: `git clone https://github.com/emi-ran/Thermal-Watcher.git`
2.  Open the `Thermal.sln` file in Visual Studio (2022 or later recommended with .NET 9 SDK).
3.  Build the solution (Build > Build Solution).

Alternatively, use the .NET CLI:

```bash
git clone https://github.com/emi-ran/Thermal-Watcher.git
cd Thermal-Watcher
dotnet build -c Release
```

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue for bugs, feature requests, or suggestions.

## License

This project is licensed under the [MIT License](LICENSE).