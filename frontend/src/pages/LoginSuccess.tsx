import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';


const LoginSuccess = ({ onLoginSuccess }: { onLoginSuccess: (isAuth: boolean) => void }) => {
 const [params] = useSearchParams();
 const navigate = useNavigate();

useEffect(() => {
 const token = params.get('token');
 const userId = params.get('userId');
 const username = params.get('username');

if (token && userId && username) {
    localStorage.clear();

      localStorage.setItem('token', token);
      localStorage.setItem('userId', userId);
      localStorage.setItem('username', username);

      onLoginSuccess(true);
      
      navigate('/dashboard');
    } else {
      navigate('/login');
    }
  }, [params, navigate, onLoginSuccess]);

  return <div>Redirecting...</div>;
};

export default LoginSuccess;
