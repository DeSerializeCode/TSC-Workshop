# WinFormsMain — Workshop manager sample

This WinForms app targets .NET 10 for Windows and demonstrates a lightweight workshop workflow:

- Capture vehicle details by registration, VIN, make, model, engine, transmission and owner contact info.
- Schedule jobs against a vehicle with a description and planned date.
- Preview SMS reminders and booking confirmations to notify vehicle owners.
- Look up vehicle details by registration and state using the MotorWeb Open API (vehicle metadata only; no owner or claims data).
- Maintain customers (name, email, phone), generate MYOB-style PDF invoices, and email invoices directly to customers.

## Prerequisites
- .NET 10 SDK installed on Windows

## Build and run
```powershell
dotnet build C:\Users\frank\Documents\GitHub\TSC-Workshop\app\WinFormsMain\WinFormsMain.csproj
dotnet run --project C:\Users\frank\Documents\GitHub\TSC-Workshop\app\WinFormsMain\WinFormsMain.csproj
```

Configure MotorWeb credentials via `appsettings.json` or environment variables before running:

```json
{
  "MotorWeb": {
    "BaseUrl": "https://api.motorweb.com.au/open/vehicle/",
    "ApiKey": "<your-api-key>"
  }
}
```

Environment variable alternative:

```powershell
$env:MOTORWEB__BASEURL="https://api.motorweb.com.au/open/vehicle/"
$env:MOTORWEB__APIKEY="your-api-key"
```

Configure SMTP for invoice emails via `appsettings.json` or environment variables:

```json
{
  "Email": {
    "Host": "smtp.yourprovider.com",
    "Port": 587,
    "FromAddress": "noreply@yourworkshop.com",
    "Username": "smtp-user",
    "Password": "smtp-password",
    "EnableSsl": true
  }
}
```

Environment variable alternative:

```powershell
$env:EMAIL__HOST="smtp.yourprovider.com"
$env:EMAIL__PORT="587"
$env:EMAIL__FROMADDRESS="noreply@yourworkshop.com"
$env:EMAIL__USERNAME="smtp-user"
$env:EMAIL__PASSWORD="smtp-password"
$env:EMAIL__ENABLESSL="true"
```

## How it works
- Add or update vehicles with the **Add / Update Vehicle** button. Vehicles are listed in the grid and available to select in the jobs and SMS areas.
- Add a job by choosing a vehicle, entering a description and a date, then select **Add Job** to track it in the jobs grid.
- Use the SMS panel to pick a vehicle and send a service reminder or booking confirmation. Messages are previewed in-app and displayed in a dialog to mimic sending; a phone number is required before sending.
- Use **Lookup via MotorWeb** after entering a registration and state to fetch vehicle details (VIN, make, model, body, fuel, transmission, drivetrain, colour). The lookup updates the fields and table while leaving owner data untouched.
- On the **Customers & Invoices** tab, add customers with name/email/phone, add invoice line items, generate a PDF invoice (saved to the local temp folder), and email the invoice to the selected customer via SMTP.
