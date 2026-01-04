# WinFormsMain — 65 point checklist

A lightweight Windows Forms app for recording and printing a 65 point vehicle checklist tied to a registration.

## What it does
- Record a registration and mark off each of the 65 inspection points.
- Flag issues on any item with M (minor) or R (requires attention).
- Save multiple checklists by registration so you can reload or update them later.
- Print a formatted checklist preview for physical records.

## Prerequisites
- .NET 10 SDK installed on Windows

## Build and run
```powershell
dotnet build C:\Users\frank\Documents\GitHub\TSC-Workshop\app\WinFormsMain\WinFormsMain.csproj
dotnet run --project C:\Users\frank\Documents\GitHub\TSC-Workshop\app\WinFormsMain\WinFormsMain.csproj
```

## How it works
- Enter the vehicle registration at the top of the checklist.
- Tick completed items and mark any issues as **M** or **R** in the grid.
- Select **Save checklist** to store the current state against the registration. Saved entries appear on the right.
- Double-click or select a saved entry and choose **Load selected** to continue working on it.
- Open **Print preview** to review or print the checklist for the registration.
