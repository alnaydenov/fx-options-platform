# Further Optimizations

## Binary Protocol & Compression

### Current payload profile

Each tick sends ~7 items (70% of 10). A typical JSON WebSocket frame:

```json
[[1,0.0234,"2025-06-09T12:00:00.0000000Z"],[3,1.2341,"2025-06-09T12:00:00.0000000Z"],...]
```

This is roughly **300–500 bytes per tick** — about 0.5 KB/s at 1 Hz.

### Binary encoding (MessagePack / Protocol Buffers)

Binary serialization would shrink each message to ~200–350 bytes, saving ~150 bytes/second. For 10 items at 1 Hz, the trade-offs are unfavourable:

- **Marginal savings** — ~150 bytes/s is negligible on any modern connection.
- **Added dependencies** — requires a MessagePack or Protobuf library on both .NET and JavaScript sides.
- **Harder to debug** — WebSocket frames become opaque in browser DevTools.

### Gzip / per-message deflate

- **Compression overhead exceeds savings** — gzip has a fixed ~20-byte header and works best on larger, repetitive payloads. On a 400-byte message it can produce a *larger* output.
- **CPU cost per tick** — compressing and decompressing every second on every connection adds latency for near-zero bandwidth gain.
- **`permessage-deflate`** — the WebSocket protocol supports this extension, but it causes memory pressure on the server with many connections and is often disabled in production.

### When it would be worth it

| Scenario | Recommended approach |
|----------|---------------------|
| 1,000+ instruments (5–50 KB/tick) | Binary encoding (MessagePack) |
| High-frequency ticks (10–100 Hz) | Binary + batching multiple ticks into one frame |
| Mobile / constrained networks | `permessage-deflate` or application-level compression |
| Many concurrent subscribers | Binary to reduce total egress bandwidth |

### Conclusion

The current compact tuple format `[id, price, timestamp]` — sending only changed items as minimal JSON — is the right choice for this scale. Binary encoding and compression should be considered only when payload size, tick frequency, or subscriber count increase by an order of magnitude.
