# FX Options Pricing Platform

A real-time FX options pricing dashboard built with **React + TypeScript** on the frontend and **.NET 6 Minimal API** on the backend. Prices stream over a **WebSocket** connection with a compact wire format to minimise network traffic.

---

## Quick Start

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Node.js 18+](https://nodejs.org/)

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

## Possible Improvements

- **Reconnection logic** — automatically reconnect the WebSocket on unexpected disconnection with exponential back-off.
- **Server-Sent Events fallback** — provide an SSE endpoint for environments where WebSockets are blocked.
- **Virtualised list** — if the item count grows significantly, use windowing (e.g. `react-window`) to keep DOM size constant.
- **Binary protocol** — replace JSON with MessagePack or Protocol Buffers for even smaller payloads.
- **Batched UI updates** — throttle React renders to ~10 fps if tick frequency increases beyond 1 Hz.
- **Persistence** — back the repository with a database so prices survive server restarts.
- **Authentication** — add token-based auth to the WebSocket handshake.
- **Unit tests** — add xUnit tests for `PriceEngine` and `OptionsRepository`; add React Testing Library tests for `usePriceStream` and `OptionsList`.
- **End-to-end tests** — Playwright tests covering subscribe/unsubscribe flow.
- **Health check endpoint** — expose `/health` for container orchestration readiness probes.
- **CORS configuration** — configure allowed origins for production deployment.
