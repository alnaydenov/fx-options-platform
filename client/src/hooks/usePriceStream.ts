import { useCallback, useRef, useState } from 'react';
import { FxOptionDisplay, PriceDirection } from '../types';

type PriceDeltaTuple = [number, number, string]; // [id, price, timestamp]

export function usePriceStream(
  items: FxOptionDisplay[],
  setItems: React.Dispatch<React.SetStateAction<FxOptionDisplay[]>>
) {
  const [subscribed, setSubscribed] = useState(false);
  const wsRef = useRef<WebSocket | null>(null);
  const pricesRef = useRef<Map<number, number>>(new Map());

  // Initialize price map from current items
  if (pricesRef.current.size === 0 && items.length > 0) {
    items.forEach((item) => pricesRef.current.set(item.id, item.price));
  }

  const subscribe = useCallback(() => {
    if (wsRef.current) return;

    const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
    const ws = new WebSocket(`${protocol}//${window.location.host}/ws`);

    ws.onmessage = (event) => {
      const deltas: PriceDeltaTuple[] = JSON.parse(event.data);

      setItems((prev) => {
        const updated = prev.map((item) => {
          const delta = deltas.find((d) => d[0] === item.id);
          if (!delta) return { ...item, direction: 'same' as PriceDirection };

          const [, newPrice, timestamp] = delta;
          const oldPrice = pricesRef.current.get(item.id) ?? item.price;

          let direction: PriceDirection = 'same';
          if (newPrice > oldPrice) direction = 'up';
          else if (newPrice < oldPrice) direction = 'down';

          pricesRef.current.set(item.id, newPrice);

          return {
            ...item,
            price: newPrice,
            updatedAt: timestamp,
            direction,
          };
        });
        return updated;
      });
    };

    ws.onopen = () => setSubscribed(true);
    ws.onclose = () => {
      setSubscribed(false);
      wsRef.current = null;
    };

    wsRef.current = ws;
  }, [setItems]);

  const unsubscribe = useCallback(() => {
    if (wsRef.current) {
      wsRef.current.close();
      wsRef.current = null;
    }
    setSubscribed(false);
    // Reset all direction indicators when unsubscribed
    setItems((prev) =>
      prev.map((item) => ({ ...item, direction: 'same' as PriceDirection }))
    );
  }, [setItems]);

  return { subscribed, subscribe, unsubscribe };
}
