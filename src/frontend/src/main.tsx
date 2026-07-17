import React from 'react';
import ReactDOM from 'react-dom/client';
import axios from 'axios';
import App from './App';

axios.defaults.baseURL = '';

ReactDOM.createRoot(document.getElementById('root') as HTMLElement).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
);

