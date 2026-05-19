# CoachingFit — Admin Dashboard (Blazor Server)

## What This Is
The **admin-facing** Blazor Server web app for CoachingFit. Admins log in here to review pending coaches, approve/reject their certificates, and activate them so they go live in the marketplace.

This is the developer's first Blazor / frontend project. Pick stack and patterns that lean on existing C# / .NET knowledge — no JS, no npm, no React/Angular/Vue.

---

## Stack
```
.NET 10                  — Microsoft.NET.Sdk.Web
Blazor Server            — interactive Server render mode
MudBlazor 8.x            — Material Design component library
ProjectReferences to:
  ../../Backend/src/Services/Identity/CoachingFit.Identity.Shared/
  ../../Backend/src/Services/User/CoachingFit.User.Shared/
```

No EF Core, no DbContext. The dashboard is a thin UI shell over the existing YARP gateway — it has no database of its own.

---

## Architecture — Feature-Light, One Project

```
CoachingFit.AdminDashboard/
├── Program.cs
├── appsettings.json
├── CoachingFit.AdminDashboard.csproj
├── Components/
│   ├── App.razor                        — head + scripts; injects MudBlazor CSS/JS
│   ├── Routes.razor                     — CascadingAuthenticationState + AuthorizeRouteView + RedirectToLogin
│   ├── RedirectToLogin.razor            — small helper that bounces unauthenticated users to /login
│   ├── _Imports.razor                   — global @using directives
│   ├── Layout/
│   │   ├── MainLayout.razor             — MudLayout + AppBar + NavMenu (drawer)
│   │   ├── EmptyLayout.razor            — used by Login.razor (no app bar / drawer)
│   │   └── NavMenu.razor
│   └── Pages/
│       ├── Home.razor                   — / — redirects to /coaches/pending
│       ├── Login.razor                  — /login (uses EmptyLayout)
│       ├── PendingCoaches.razor         — /coaches/pending
│       ├── CoachDetail.razor            — /coaches/{userId}
│       └── RejectReasonDialog.razor     — MudDialog used from CoachDetail
├── Services/
│   ├── TokenStore.cs                    — scoped: holds AuthResponse (incl. JWT) for current circuit
│   ├── CircuitAuthenticationStateProvider.cs
│   │                                    — scoped: builds ClaimsPrincipal from TokenStore; notifies on sign-in/out
│   ├── AuthApiClient.cs                 — POST /api/Auth/login
│   ├── CoachApiClient.cs                — GET pending list, GET profiles, PUT activate
│   └── CertificateApiClient.cs          — GET coach certs, PUT approve, PUT reject
└── Infrastructure/
    └── BearerTokenHandler.cs            — DelegatingHandler that adds Authorization: Bearer
```

---

## Auth flow (the deliberate choice)

There is **no cookie auth and no minimal-API login endpoint**. Both add complexity that an internal admin tool used by one or two people doesn't need.

Instead:
1. `Login.razor` calls `AuthApiClient.LoginAsync(...)` → backend `POST /api/Auth/login`.
2. On 200, if `role == "Admin"` → `CircuitAuthenticationStateProvider.SignIn(authResponse)`.
3. That:
   - Stores the `AuthResponse` (with the JWT) in scoped `TokenStore`.
   - Builds a `ClaimsPrincipal` (NameIdentifier = userId, Role = "Admin", etc.) and notifies `AuthenticationState` listeners.
4. `[Authorize(Roles="Admin")]` on every protected page is satisfied.
5. `BearerTokenHandler` reads `TokenStore.AccessToken` on every outbound HTTP call → adds `Authorization: Bearer <jwt>`.
6. Logout = `CircuitAuthenticationStateProvider.SignOut()` → clears `TokenStore`, redirects to `/login`.

**Known consequence:** state is per-circuit. Browser reload = new circuit = re-login.
This is acceptable for v1. Cookies + persistent auth = later.

**No refresh-token logic.** A backend 401 (access token expired) currently surfaces as a request failure. Re-login by hand. Acceptable for an admin tool used in short bursts.

---

## Backend contract — endpoints we consume

| Method | Route | Used by |
|---|---|---|
| POST | `/api/Auth/login` | `AuthApiClient` |
| GET  | `/api/Auth/coaches/pending` | `CoachApiClient.GetPendingUserIdsAsync` — returns `IEnumerable<string>` |
| GET  | `/api/CoachProfile/pending?userIds=` | `CoachApiClient.GetProfilesByUserIdsAsync` — takes list of userIds |
| GET  | `/api/CoachCertificate/coach/{coachUserId}` | `CertificateApiClient.GetForCoachAsync` |
| PUT  | `/api/CoachCertificate/{id:guid}/approve` | `CertificateApiClient.ApproveAsync` |
| PUT  | `/api/CoachCertificate/{id:guid}/reject` (JSON body: `RejectCertificateRequest`) | `CertificateApiClient.RejectAsync` |
| PUT  | `/api/Auth/coaches/{id}/activate` | `CoachApiClient.ActivateAsync` (id = userId string) |

Everything goes through the YARP gateway. **Never call services directly.**

Base URL is read from `Api:BaseUrl` in `appsettings.json` — default `http://localhost:5000/`.

---

## DTO sharing — relative ProjectReference

`.csproj` references two backend Shared projects via relative path:

```xml
<ProjectReference Include="..\..\Backend\src\Services\Identity\CoachingFit.Identity.Shared\CoachingFit.Identity.Shared.csproj" />
<ProjectReference Include="..\..\Backend\src\Services\User\CoachingFit.User.Shared\CoachingFit.User.Shared.csproj" />
```

This requires the workspace layout `CoachingFit/{Backend,Admin Dashboard}/...`. If those folders are missing (e.g. on a CI box that only checks out the Admin Dashboard repo), the build fails. Acceptable for v1 — revisit when we set up CI by publishing the `Shared` projects as private NuGet packages.

Both `Identity.Shared.Wrappers.GenericResponse<T>` and `User.Shared.Wrappers.GenericResponse<T>` exist as separate types with the same shape. The API clients use each service's own wrapper for its own endpoints — System.Text.Json doesn't care which class is used to deserialize since the JSON shape is identical, but using the matching type keeps intent clear.

---

## MudBlazor — what we use

- `MudLayout` + `MudAppBar` + `MudDrawer` + `MudMainContent` (shell)
- `MudNavMenu` + `MudNavLink` (sidebar)
- `MudTable` (pending coaches list)
- `MudPaper` + `MudGrid` + `MudItem` + `MudStack` (layout primitives)
- `MudAvatar`, `MudImage`, `MudIcon` (chrome)
- `MudButton`, `MudChip`, `MudTextField`, `MudForm` (controls)
- `MudDialog` + `IDialogService.ShowAsync<T>` and `ShowMessageBox` (modals)
- `ISnackbar` (toast notifications)
- `MudProgressCircular`, `MudAlert` (status)

Theme: `MudThemeProvider IsDarkMode="true"` registered in MainLayout + EmptyLayout.

---

## Running locally

```bash
dotnet run --project "Admin Dashboard\CoachingFit.AdminDashboard\CoachingFit.AdminDashboard.csproj"
```

Dev URL: **http://localhost:5100** (HTTP) or **https://localhost:7100** (HTTPS).
The backend gateway must be running on `http://localhost:5000` (`Api:BaseUrl`).

Admin credentials come from `Seeding:AdminEmail` / `Seeding:AdminPassword` user secrets on the Identity service.

---

## What's built (v1 MVP)

- Login (/login) — MudForm, role check, redirects with returnUrl
- Pending coaches list (/coaches/pending) — MudTable with photo, userId, gender, experience, created-at
- Coach detail (/coaches/{userId}) — profile card + certificate list + per-cert Approve/Reject + Activate Coach
- Reject dialog — required reason, MudTextField multiline
- AuthorizeRouteView + RedirectToLogin guard

## Not built (deferred)

- User search / list (all coaches, all trainees) — needs backend endpoints
- Dashboard / metrics — needs backend aggregate endpoints
- Trainee profile review
- Plan / order / wallet management — those services don't exist yet
- SignalR live notifications ("new pending coach" toast) — revisit after plan-request feature
- Token refresh — re-login on 401
- Audit log
- Pagination — N/A at current scale
- Production hosting / Docker / CI
- NuGet packaging of `Backend/...Shared/`

---

## Conventions

- One project, no Onion layering. Adding layers prematurely would slow the build without solving a real problem.
- All endpoint URLs live inside the typed `*ApiClient` classes — no scattered string literals.
- All UI text and colors go through MudBlazor components / `Color.Primary` / `Color.Success` / etc. — never hardcode hex.
- Per-circuit state lives in scoped DI services. Don't try to persist to cookies / local storage — that complication is deferred.
- `[Authorize(Roles="Admin")]` on every protected page. Login page is the only page with no `[Authorize]`.
- Routes use kebab-case where they map to a concept (`/coaches/pending`, `/coaches/{userId}`).
- Pages use `OnInitializedAsync` for first fetch, catch `HttpRequestException` and surface a friendly error in MudAlert / MudSnackbar.

---

## Don'ts

- Don't call backend services directly — only the gateway (`Api:BaseUrl`).
- Don't store the JWT in cookies or localStorage — it lives in scoped `TokenStore` only.
- Don't add EF Core or a database — this app is stateless beyond the SignalR circuit.
- Don't add a `Counter.razor` or `Weather.razor` style placeholder — those template files were deleted intentionally.
- Don't hardcode `http://localhost:5000` in code — always read `Api:BaseUrl`.
- Don't put `AlignItems` on `MudGrid` — MudBlazor analyzer flags it (MUD0002). Put it on `MudStack` / `MudItem` instead.

---

## Repository
*Pending first push — `git init` done, no remote yet.*
