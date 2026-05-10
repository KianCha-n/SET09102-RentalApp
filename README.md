#Rental App (SET09102 Coursework)

A .NET MAUI Android application implementing a "Library of Things" peer-to-peer rental marketplace.
Built on top of the SET09102 StarterApp template, this project demonstrates clean architecture,
MVVM, the Repository pattern, automated testing and CI/CD.

**Module:** SET09102 Software Engineering — Edinburgh Napier University


---

## Features

- **JWT authentication** against the shared coursework API (`/auth/token`, `/auth/register`)
- **Item browsing** — list all items, view item details, owner-only edit
- **Add / edit item** with input validation (title length, daily-rate range, category required)
- **Rental requests** — request, approve / reject, returned / completed flow
- **Incoming vs Outgoing rentals** — two-tab list for items lent out vs items borrowed
- **Token-expiry handling** — expired sessions are forced to re-authenticate before any API call
- **Role-aware UI** — admin-only routes are gated by `IAuthenticationService.HasRole`


### Prerequisites

- .NET 10 SDK
- MAUI workload: `dotnet workload install maui-android`
- Android emulator (or physical device with USB debugging) — recommended API 34
- `adb` on your `PATH`

### Build & Run on Android

```bash
dotnet build StarterApp/StarterApp.csproj -f net10.0-android -c Debug
adb install -r StarterApp/bin/Debug/net10.0-android/com.companyname.starterapp-Signed.apk
```

Then launch **StarterApp** from the emulator/device app drawer. Register a new account or log in
with existing credentials — the app talks to the live API, so any account created persists.

### Running the Tests

```bash
dotnet test StarterApp.Tests/StarterApp.Tests.csproj
```


