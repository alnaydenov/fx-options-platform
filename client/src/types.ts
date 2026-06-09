export interface FxOptionItem {
  id: number;
  name: string;
  price: number;
  updatedAt: string;
}

export type PriceDirection = 'up' | 'down' | 'same';

export interface FxOptionDisplay extends FxOptionItem {
  direction: PriceDirection;
}
