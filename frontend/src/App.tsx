import React, { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import Dashboard from './pages/Dashboard';
import LoginSuccess from './pages/LoginSuccess';

// This new component contains all the routing logic and hooks that need to be
// inside the <Router> context.
const AppRoutes: React.FC = () => {
  const location = useLocation();
  
  // Use state to manage the authentication status
  const [isAuthenticated, setIsAuthenticated] = useState(!!localStorage.getItem('token'));
  
  // Listen for changes to localStorage
  useEffect(() => {
    const handleStorageChange = () => {
      const newToken = localStorage.getItem('token');
      setIsAuthenticated(!!newToken);
    };

    window.addEventListener('storage', handleStorageChange);

    // Cleanup the event listener when the component unmounts
    return () => {
      window.removeEventListener('storage', handleStorageChange);
    };
  }, []); // Empty dependency array means this runs only once

  const handleLogout = () => {
    localStorage.clear();
    setIsAuthenticated(false);
  };

  console.log("--- App.tsx Render Cycle ---");
  console.log("Current Path:", location.pathname);
  console.log("Is Authenticated (from state):", isAuthenticated);
  console.log("----------------------------");

  return (
    <Routes>
      <Route path="/" element={isAuthenticated ? <Navigate to="/dashboard" /> : <LoginPage />} />
      <Route path="/login-success" element={<LoginSuccess onLoginSuccess={setIsAuthenticated} />} />
      <Route
        path="/dashboard/*"
        element={isAuthenticated ? <Dashboard onLogout={handleLogout} /> : <Navigate to="/" />}
      />
    </Routes>
  );
};

// The main App component only contains the Router, and renders AppRoutes inside it.
const App: React.FC = () => {
  return (
    <Router>
      <AppRoutes />
    </Router>
  );
};

export default App;
