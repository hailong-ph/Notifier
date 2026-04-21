A simple desktop application that sends a notification every 40 minutes to take a break from the screen.

If you stand up and walk for a few minutes can do a great help for health, why not be reminded?

Supports:

- Windows native toast notifications
- Linux desktop notifications over `org.freedesktop.Notifications` D-Bus

On Ubuntu KDE, no extra system package is required in a normal desktop session.
If the D-Bus notification service is unavailable, the app will still try `notify-send` as a fallback when present.
