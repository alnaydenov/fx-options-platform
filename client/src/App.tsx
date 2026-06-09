import { useEffect, useState } from 'react';
import { FxOptionDisplay, FxOptionItem } from './types';
import { OptionsList } from './components/OptionsList';
import './App.css';

function App() {
  const [items, setItems] = useState<FxOptionDisplay[]>([]);
  const [loading, setLoading] = useState(true);
  const [subscribed, setSubscribed] = useState(false);

  useEffect(() => {
    fetch('/api/options')
      .then((res) => res.json())
      .then((data: FxOptionItem[]) => {
        setItems(data.map((item) => ({ ...item, direction: 'same' as const })));
        setLoading(false);
      })
      .catch((err) => {
        console.error('Failed to load options:', err);
        setLoading(false);
      });
  }, []);

  const handleSubscribe = () => {
    setSubscribed(true);
  };

  const handleUnsubscribe = () => {
    setSubscribed(false);
  };

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  return (
    <div className="app">
      <header className="app-header">
        <h1>FX Options Pricing</h1>
        <div className="controls">
          <button
            className="btn btn--subscribe"
            onClick={handleSubscribe}
            disabled={subscribed}
          >
            Subscribe
          </button>
          <button
            className="btn btn--unsubscribe"
            onClick={handleUnsubscribe}
            disabled={!subscribed}
          >
            Unsubscribe
          </button>
        </div>
      </header>
      <main>
        <OptionsList items={items} />
      </main>
    </div>
  );
}

export default App;
