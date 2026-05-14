import { useEffect, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import toast from 'react-hot-toast';
import { APP_NAME, ROUTES } from '../config';
import { confirmEmail, getMe } from '../api/auth';
import { useAuthStore } from '../store/authStore';
import { Button } from '../components/ui/Button';
import { Spinner } from '../components/ui/Spinner';
import styles from './ConfirmEmailPage.module.css';

function getErrorMessage(error) {
  const status = error?.response?.status;

  if (status === 401) {
    return 'This link is invalid or expired. Please request a new email and try again.';
  }

  if (status >= 500) {
    return 'The server is temporarily unavailable. Please try again later.';
  }

  return 'We could not confirm your email. Please try again later.';
}

export function ConfirmEmailPage() {
  const [params] = useSearchParams();
  const navigate = useNavigate();
  const [state, setState] = useState('loading');
  const [message, setMessage] = useState('Confirming your email...');

  useEffect(() => {
    let active = true;

    async function run() {
      const token = params.get('token');

      if (!token) {
        if (active) {
          setState('error');
          setMessage('No confirmation token was found in the link. Please open the email again.');
        }
        return;
      }

      try {
        const response = await confirmEmail(token);
        useAuthStore.getState().setUser({
          login: response.data.login,
          email: null,
        });

        try {
          await getMe();
        } catch (profileError) {
          void profileError;
        }

        if (!active) {
          return;
        }

        useAuthStore.getState().setStatus('authenticated');
        setState('success');
        setMessage('Your email is confirmed. Redirecting to home...');
        toast.success('Email confirmed');

        window.setTimeout(() => {
          if (active) {
            navigate(ROUTES.home, { replace: true });
          }
        }, 900);
      } catch (error) {
        if (!active) {
          return;
        }

        setState('error');
        setMessage(getErrorMessage(error));
        toast.error('We could not confirm your email. Please try again later.');
      }
    }

    run();

    return () => {
      active = false;
    };
  }, [navigate, params]);

  return (
    <div className={styles.page}>
      <div className={styles.card}>
        <img className={styles.logo} src="/logo.svg" alt={APP_NAME} />
        <h1>Email confirmation</h1>
        <p>{message}</p>

        {state === 'loading' ? <Spinner label="Confirming email" /> : null}

        {state === 'error' ? (
          <div className={styles.actions}>
            <Link to={ROUTES.auth} className={styles.linkButton}>
              Back to sign in
            </Link>
            <Button variant="secondary" onClick={() => navigate(ROUTES.auth, { replace: true })}>
              Try again
            </Button>
          </div>
        ) : null}
      </div>
    </div>
  );
}
