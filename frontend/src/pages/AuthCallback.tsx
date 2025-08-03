import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

const AuthCallback = () => {
  const navigate = useNavigate();

  useEffect(() => {
    const completeLogin = async () => {
      try {
        const res = await fetch(`${process.env.REACT_APP_API_URL}/api/users/google-callback`, {
          credentials: 'include',
        });

        if (!res.ok) throw new Error('Login failed');
        const data = await res.json();

        localStorage.setItem('token', data.token);
        localStorage.setItem('userId', data.userId);
        localStorage.setItem('username', data.username);

        navigate('/dashboard');
      } catch (err) {
        console.error(err);
        navigate('/login');
      }
    };

    completeLogin();
  }, [navigate]);

  return <div>Signing you in...</div>;
};

export default AuthCallback;
