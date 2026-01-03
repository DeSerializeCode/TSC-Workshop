# WinFormsMain — Workshop manager sample

This WinForms app targets .NET 7 for Windows and demonstrates a lightweight workshop workflow:

- Capture vehicle details by registration, VIN, make, model, engine, transmission and owner contact info.
- Schedule jobs against a vehicle with a description and planned date.
- Preview SMS reminders and booking confirmations to notify vehicle owners.

## Prerequisites
- .NET 7 SDK installed on Windows

## Build and run
```powershell
dotnet build C:\Users\frank\Documents\GitHub\TSC-Workshop\app\WinFormsMain\WinFormsMain.csproj
dotnet run --project C:\Users\frank\Documents\GitHub\TSC-Workshop\app\WinFormsMain\WinFormsMain.csproj
```

## How it works
- Add or update vehicles with the **Add / Update Vehicle** button. Vehicles are listed in the grid and available to select in the jobs and SMS areas.
- Add a job by choosing a vehicle, entering a description and a date, then select **Add Job** to track it in the jobs grid.
- Use the SMS panel to pick a vehicle and send a service reminder or booking confirmation. Messages are previewed in-app and displayed in a dialog to mimic sending; a phone number is required before sending.
