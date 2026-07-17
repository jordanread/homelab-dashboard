# The Hold — Blazor Edition

A .NET 10 **Blazor Web App** (Interactive Server render mode) port of the static
`homelab-dashboard` site. Same look, same mocked data, same page structure —
now backed by C# models instead of hand-duplicated HTML.

> **Heads up:** this sandbox doesn't have the .NET SDK or access to nuget.org,
> so I wrote and reviewed everything by hand but couldn't run `dotnet build`
> or `dotnet run` to confirm it compiles clean. Do a build as your first step
> after pulling this down — see below — and ping me with any error output if
> something doesn't line up.

## Running it

```bash
cd HomelabDashboard
dotnet restore
dotnet watch run
```

Then open the URL `dotnet` prints (defaults to `http://localhost:5223`).

## What changed vs. the static site

- **11 hand-written HTML pages → 1 data-driven route.** `/services/{slug}`
  renders from `Data/ServiceCatalog.cs`, which holds every service's card
  copy, hero copy, and detail sections (paragraphs/lists/code blocks/tips)
  as C# objects instead of duplicated markup. Add a new self-hosted app by
  adding one `ServiceInfo` entry — nav, card, and detail page all pick it up.
- **Mocked stats → `IDashboardStatsProvider`.** The Ship's Log numbers (movie
  counts, ads blocked, disk gauge, etc.) come from `MockDashboardStatsProvider`
  in `Data/DashboardStats.cs`, registered in `Program.cs`. Swap in a real
  implementation later (Pi-hole API, a `df` shell-out, a Docker socket query)
  without touching any `.razor` file.
- **`js/main.js` → `wwwroot/js/interop.js` + C# state.** Theme toggle now
  persists via `localStorage` through JS interop (`holdTheme.init`/`.set`),
  called from `MainLayout.razor`. Mobile nav open/close and the certificate
  page's OS tabs are now plain C# `@code` state — no JS needed for those.
  Fade-in-on-scroll, the count-up numbers, the disk gauge fill, and the
  floating hero anchor are still small, purely-cosmetic JS (`IntersectionObserver`
  + `requestAnimationFrame`), re-wired after each Blazor enhanced-navigation.
- **One shared layout.** `Components/Layout/MainLayout.razor` owns the nav,
  mobile drawer, theme toggle, and footer that used to be copy-pasted at the
  top and bottom of every `.html` file.

## Project layout

```
Components/
  App.razor              — root HTML document
  Routes.razor            — router + 404
  _Imports.razor
  Layout/MainLayout.razor — nav, mobile nav, theme toggle, footer
  Pages/
    Home.razor             — "/" — hero, stats, deck, admiral, cert sections
    ServiceDetail.razor    — "/services/{slug}"
    Error.razor
  Shared/
    StatCard.razor, ServiceCard.razor, AdmiralCard.razor,
    Gauge.razor, RopeDivider.razor, DetailSectionView.razor
Data/
  ServiceInfo.cs           — models: ServiceInfo, DetailSection, OsInstructions
  ServiceCatalog.cs        — the 11 services, ported verbatim from pages/*.html
  DashboardStats.cs        — mocked stat-card + disk numbers
wwwroot/
  css/app.css              — ported stylesheet, new color tokens (see below)
  js/interop.js            — theme persistence + scroll effects
  certs/thehold-ca.crt     — placeholder; replace with the real Caddy root CA
```

## Color scheme

`wwwroot/css/app.css` now defines your token set at the top of `:root`:

```css
--navy, --navy-mid, --navy-light, --cream, --cream-dim,
--gold, --gold-light, --red, --red-bright, --white,
--font-display, --font-deco, --font-body,
--radius, --radius-lg, --transition, --max-w, --section-pad
```

Everything else in the stylesheet still references the *semantic* variables
it always did (`--bg-primary`, `--accent-teal`, `--text-secondary`, etc.) —
I just repointed those at your new tokens instead of retyping ~1,100 lines
of CSS. Mapping, dark theme (default):

| Semantic var | New value |
|---|---|
| `--bg-primary` / `--bg-secondary` / `--bg-card` | `--navy` / `--navy-mid` / `--navy-mid` |
| `--bg-card-hover` | `--navy-light` |
| `--text-primary` / `--text-secondary` / `--text-muted` | `--cream` / `--cream-dim` / `#a89b85` |
| `--accent-teal` / `--accent-green` | `--gold-light` (used for links, "good" status, gauges) |
| `--accent-gold` | `--gold` |
| `--accent-coral` / `--accent-purple` | `--red-bright` / `--red` |
| `--border-color` / `--border-bright` | your `--border` token / a brighter gold rgba |

Light theme (`data-theme="light"`) flips it: cream backgrounds, navy text,
gold/red accents darkened slightly for contrast on a light surface.

Fonts: `Playfair Display` for headings (unchanged), `DM Sans` replaces the
old `Crimson Pro` body serif, `Cinzel Decorative` is loaded and available via
`var(--font-deco)` if you want it on the nav brand or hero title later — I
left it unused for now since it wasn't in the original hero styling and I
didn't want to guess at how heavy-handed you want it. `Space Mono` stays for
code blocks, labels, and the mono stat values, since your palette didn't
specify a replacement.

## Known gaps / next steps

- Not build-tested locally (no SDK in this environment) — run `dotnet build`
  first thing.
- The cert download is a placeholder text file; drop the real Caddy root CA
  in at `wwwroot/certs/thehold-ca.crt`.
- Stats and service "Running/Connected" status are still fully mocked, same
  as the original static site — wire up `IDashboardStatsProvider` and a real
  service-status source whenever you're ready to make it live.
