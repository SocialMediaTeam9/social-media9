import React from 'react';

const LoginPage: React.FC<{ onLogin: () => void }> = ({ onLogin }) => {
  return (
    <div className="login-container">
      <div className="login-box">
        <h1 className="login-title">Social Media App</h1>
        <p className="login-subtitle">Connect with friends and the world around you.</p>
        <button className="login-button" onClick={onLogin}>Sign in with Google</button>
      </div>
    </div>
  );
};

export default LoginPage;