# FX Options Pricing Platform

A real-time FX options pricing dashboard built with **React + TypeScript** on the frontend and **.NET 6 Minimal API** on the backend. Prices stream over a **WebSocket** connection with a compact wire format to minimise network traffic.

---

## Quick Start

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Node.js 18+](https://nodejs.org/)
- [Git](https://git-scm.com/)

### Clone the Repository

```bash
git clone https://github.com/alnaydenov/fx-options-platform.git
cd fx-options-platform
```

### Run the Server

```bash
cd server
dotnet run
```

The API starts on `http://localhost:5000`.

### Run the Client

```bash
cd client
npm install
npm run dev
```

Open `http://localhost:5173` in your browser. Vite proxies `/api` and `/ws` requests to the .NET server automatically.

### Run the Tests

```bash
cd server.tests
dotnet test
```

---

## Architecture Overview

```
┌─────────────────────────────┐        WebSocket (JSON)       ┌─────────────────────────────┐
│          React UI           │ ◄──────────────────────────── │        .NET Server           │
│  Vite · TypeScript · CSS    │        GET /api/options        │  Minimal API · WebSockets   │
│                             │ ──────────────────────────► │                             │
└─────────────────────────────┘                               └─────────────────────────────┘
```

### Server (`server/`)

| File | Purpose |
|------|---------|
| `Program.cs` | Entry point — registers services, maps REST and WebSocket endpoints |
| `Models/FxOption.cs` | Domain model for an FX option |
| `Models/PriceDelta.cs` | Lightweight record sent over the wire |
| `Services/OptionsRepository.cs` | In-memory store seeded from `Data/items.json` |
| `Services/PriceEngine.cs` | Generates random price movements (~±1 %) every tick |
| `Services/PriceTickerService.cs` | `BackgroundService` that ticks every second and broadcasts deltas to subscribers |
| `Data/items.json` | Seed data — 10 FX option instruments |

**Key design choices:**

- **Minimal API** — no MVC overhead; the entire server is ~150 lines.
- **WebSocket push** — server pushes only changed prices, not the full list.
- **Compact wire format** — each delta is a `[id, price, timestamp]` tuple array, keeping payloads small.
- **Thread-safe repository** — `lock`-based concurrency around the in-memory list; sufficient for a single-server scenario.
- **Testability** — `IOptionsRepository` and `IPriceEngine` are interface-driven and registered via DI, making them straightforward to mock.

### Client (`client/`)

| File | Purpose |
|------|---------|
| `src/App.tsx` | Root component — fetches initial data, wires subscribe/unsubscribe |
| `src/components/OptionsList.tsx` | Renders the options grid with price direction indicators |
| `src/hooks/usePriceStream.ts` | Custom hook managing the WebSocket lifecycle and delta merging |
| `src/types.ts` | Shared TypeScript interfaces |
| `src/index.css` | CSS variables and global dark-theme styles |
| `src/App.css` | Layout and button styles |
| `src/components/OptionsList.css` | Grid and colour-coded price indicators |

**Key design choices:**

- **Zero UI library dependencies** — only React, ReactDOM, and Vite tooling. All styling is hand-written CSS with custom properties.
- **`usePriceStream` hook** — encapsulates all WebSocket logic, keeping the component tree declarative.
- **O(1) delta merging** — incoming deltas are indexed into a `Map` before merging, avoiding O(n×m) scans.
- **Direction tracking via ref** — `pricesRef` persists previous prices across renders without causing extra re-renders.
- **Testability** — the hook accepts `items` and `setItems` as parameters, so it can be tested with a mock state setter. `OptionsList` is a pure presentational component.

---

## How It Works

1. On load, the client fetches all 10 options via `GET /api/options`.
2. Clicking **Subscribe** opens a WebSocket to `/ws`.
3. The server's `PriceTickerService` generates price deltas every second and broadcasts them.
4. Each delta is a compact `[[id, price, "ISO-timestamp"], ...]` array — only instruments whose price changed are included.
5. The client merges deltas into state, computes up/down/same direction, and re-renders.
6. Clicking **Unsubscribe** closes the WebSocket; direction indicators reset.

---

## Areas for Improvement

### Resilience & Reliability

- **WebSocket reconnection** — the client does not currently retry on unexpected disconnection. Adding automatic reconnection with exponential back-off and a visual "reconnecting…" indicator would make the app production-ready.
- **Server-Sent Events fallback** — some corporate proxies and firewalls block WebSocket upgrades. An SSE fallback (`EventSource`) would provide a degraded-but-functional path.
- **Error boundaries** — wrapping the component tree in a React error boundary would prevent a single rendering failure from crashing the whole UI.

### Performance & Scalability

- **Virtualised list** — with only 10 items the DOM is trivial, but scaling to hundreds or thousands of instruments would benefit from windowing (e.g. `react-window`) to keep DOM size constant.
- **Binary wire protocol** — the current JSON tuples are already compact, but replacing them with MessagePack or Protocol Buffers would further reduce payload size and parse time.
- **Batched / throttled renders** — at higher tick frequencies (e.g. 10–100 Hz), batching incoming deltas and rendering on a `requestAnimationFrame` cadence would prevent dropped frames.
- **Concurrent broadcasts** — `PriceTickerService` currently sends to each subscriber sequentially. Using `Task.WhenAll` for parallel sends would reduce broadcast latency with many clients.

### Data & Persistence

- **Database-backed repository** — prices are held in memory and reset on restart. Persisting to a lightweight store (SQLite, Redis) would preserve state across deployments.
- **Historical price data** — storing a time-series of price ticks would enable sparklines or mini-charts per instrument on the UI.

### Security & Operations

- **Authentication** — the WebSocket endpoint is currently open. Adding JWT or cookie-based auth to the upgrade handshake would restrict access.
- **CORS configuration** — configure explicit allowed origins for production rather than the current permissive default.
- **Health check endpoint** — expose `GET /health` for load balancers and container orchestration readiness probes.
- **Structured logging** — add correlation IDs and structured log fields for easier production debugging.

### Testing

- **Server unit tests** — `PriceEngine` and `OptionsRepository` are interface-driven and DI-registered, making them straightforward to test with xUnit and mocked dependencies.
- **Client unit tests** — `OptionsList` is a pure presentational component testable with React Testing Library; `usePriceStream` can be tested with a mock WebSocket.
- **End-to-end tests** — a Playwright suite covering the full subscribe → receive updates → unsubscribe flow would catch integration regressions.

### UX Enhancements

- **Price flash animation** — briefly flash the cell background green/red on price change for stronger visual feedback.
- **Sorting & filtering** — allow users to sort by column or filter by currency pair.
- **Responsive layout** — adapt the grid to smaller screens with a card-based layout on mobile.
- **Dark/light theme toggle** — the CSS custom properties are already in place; adding a toggle would be minimal effort.
