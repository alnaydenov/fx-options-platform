import { FxOptionDisplay } from '../types';
import './OptionsList.css';

interface OptionsListProps {
  items: FxOptionDisplay[];
}

export function OptionsList({ items }: OptionsListProps) {
  return (
    <div className="options-list">
      <div className="options-header">
        <span className="col col-id">ID</span>
        <span className="col col-name">Name</span>
        <span className="col col-price">Price</span>
        <span className="col col-updated">Updated</span>
      </div>
      {items.map((item) => (
        <div key={item.id} className="options-row">
          <span className="col col-id">{item.id}</span>
          <span className="col col-name">{item.name}</span>
          <span className="col col-price">
            {item.price.toFixed(4)}
          </span>
          <span className="col col-updated">
            {new Date(item.updatedAt).toLocaleTimeString()}
          </span>
        </div>
      ))}
    </div>
  );
}
