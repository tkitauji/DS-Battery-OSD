# Microsoft Store listing (English - United States)

## Product name

DS Battery OSD

## Short description

See your DualSense battery level in a simple overlay while you play.

## Description

DS Battery OSD is a lightweight Windows utility that displays the battery status of a connected DualSense or DualSense Edge controller on screen.

- Automatically detects USB and Bluetooth connections
- Shows battery level, charging status, and FULL status
- Turns red when the battery level drops below 20%
- Uses a clean, background-free, always-on-top overlay
- Drag the overlay anywhere and keep its position after restarting
- Automatically hides while the controller is disconnected
- Starts automatically when you sign in to Windows after the first launch (can be disabled in Windows Settings)

“FULL” is displayed while the DualSense reports a fully charged status. After unplugging the cable, the display switches to the controller’s stepped battery-level reading, so it may immediately show a value such as 80%. This does not mean the battery suddenly lost charge; it is a characteristic of how DualSense reports its battery level.

The app works in windowed and borderless fullscreen modes. Exclusive fullscreen mode is not supported.

The app does not collect, store, or transmit personal information. It only reads local HID data from a connected supported controller.

## Search terms

DualSense, battery, controller, OSD, PlayStation, gaming

## Restricted capability justification

`runFullTrust` is required to display an always-on-top WPF overlay on the desktop and read the battery status of a locally connected DualSense or DualSense Edge controller through the Windows HID API. The app does not communicate with external services and does not collect, store, or transmit personal information.
