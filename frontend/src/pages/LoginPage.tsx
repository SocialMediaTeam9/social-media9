
import React from 'react';

const LoginPage: React.FC = () => {
  const handleGoogleLogin = () => {
    const api_url = process.env.REACT_APP_API_URL;
    const redirect_target = `${api_url}/api/users/signin-google`;

    console.log("API URL configured:", api_url);
    console.log("Attempting to redirect to:", redirect_target);

    window.location.href = redirect_target;
  };

  return (
    <div className="login-container">
      <div className="login-box">
        <h1 className="login-title">SM App</h1>
        <p className="login-subtitle">Connect with friends and the world around you.</p>
        <button className="login-button" onClick={handleGoogleLogin}>
          Sign in with Google
        </button>
      </div>
    </div>
  );
};

export default LoginPage;