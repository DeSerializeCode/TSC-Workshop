# WinFormsMain — Simple Windows main page

This is a minimal C# WinForms main page targeting .NET 7 for Windows.

Prerequisites
- .NET 7 SDK installed on Windows

Build and run
```powershell
dotnet build C:\Users\frank\Documents\GitHub\TSC-Workshop\app\WinFormsMain\WinFormsMain.csproj
dotnet run --project C:\Users\frank\Documents\GitHub\TSC-Workshop\app\WinFormsMain\WinFormsMain.csproj
```

What you get
- A main window with a title, two buttons (`Open`, `Settings`) and a status label.

Next steps
- Replace the button handlers with real app logic.
- Convert to WPF or WinUI if you prefer modern UI features.
