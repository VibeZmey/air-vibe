import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { APP_NAME, ROUTES } from '../config';
import { registerUser, loginUser } from '../api/auth';
import { Button } from '../components/ui/Button';
import { Input } from '../components/ui/Input';
import { Spinner } from '../components/ui/Spinner';
import styles from './AuthPage.module.css';

const initialValues = {
  email: '',
  password: '',
};

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function getErrorMessage(error) {
  const status = error?.response?.status;
  const message =
    error?.response?.data?.message ||
    error?.response?.data ||
    error?.message ||
    '';

  if (status === 401) {
    return 'Invalid email or password. Please check your details and try again.';
  }

  if (status >= 500) {
    return 'The server is temporarily unavailable. Please try again later.';
  }

  if (typeof message === 'string' && message.trim()) {
    return message;
  }

  return 'We could not send the email. Please try again later.';
}

function validate(values, mode) {
  const nextErrors = {};
  const email = values.email.trim();
  const password = values.password;

  if (!email) {
    nextErrors.email = 'Enter your email';
  } else if (!emailPattern.test(email)) {
    nextErrors.email = 'Enter a valid email address';
  }

  if (!password) {
    nextErrors.password = 'Enter your password';
  } else if (mode === 'register' && password.length < 8) {
    nextErrors.password = 'Password must be at least 8 characters long';
  }

  return nextErrors;
}

export function AuthPage() {
  const [mode, setMode] = useState('login');
  const [values, setValues] = useState(initialValues);
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState('neutral');

  const heading = useMemo(
    () => (mode === 'register' ? 'Create your account' : 'Sign in'),
    [mode]
  );

  function handleModeChange(nextMode) {
    setMode(nextMode);
    setErrors({});
    setMessage('');
    setMessageType('neutral');
  }

  function handleChange(event) {
    const { name, value } = event.target;
    setValues((current) => ({ ...current, [name]: value }));
    setErrors((current) => ({ ...current, [name]: '' }));
  }

  async function handleSubmit(event) {
    event.preventDefault();

    const nextErrors = validate(values, mode);
    setErrors(nextErrors);

    if (Object.keys(nextErrors).length > 0) {
      return;
    }

    setIsSubmitting(true);
    setMessage('');

    try {
      if (mode === 'register') {
        await registerUser({
          email: values.email.trim(),
          password: values.password,
        });
        setMessage('We sent a verification link to your email address.');
        toast.success('Verification email sent');
      } else {
        await loginUser({
          email: values.email.trim(),
          password: values.password,
        });
        setMessage('We sent a sign-in verification link to your email address.');
        toast.success('Sign-in email sent');
      }

      setMessageType('success');
    } catch (error) {
      setMessageType('error');
      setMessage(getErrorMessage(error));
      toast.error('We could not send the email. Please try again later.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className={styles.page}>
      <section className={styles.hero}>
        <div className={styles.heroBadge}>{APP_NAME}</div>
        <h1>Fly with vibe</h1>
        <p>Simple travel, smooth booking, and a calm way to get where you are going.</p>
        <div className={styles.heroList}>
          <div>
            <strong>Easy to start</strong>
            <span>Sign in or create your account in just a few steps.</span>
          </div>
          <div>
            <strong>Travel made simple</strong>
            <span>Book, manage, and explore flights in one place.</span>
          </div>
          <div>
            <strong>Designed for comfort</strong>
            <span>Clean visuals and a light violet palette keep the experience calm.</span>
          </div>
        </div>
      </section>

      <section className={styles.card}>
        <div className={styles.cardTop}>
          <div>
            <span className={styles.cardEyebrow}>Account</span>
            <h2>{heading}</h2>
          </div>
          <img className={styles.logo} src="/logo.svg" alt={APP_NAME} />
        </div>

        <div className={styles.tabs} role="tablist" aria-label="Sign in and registration">
          <button
            type="button"
            className={mode === 'login' ? styles.tabActive : styles.tab}
            onClick={() => handleModeChange('login')}
          >
            Sign in
          </button>
          <button
            type="button"
            className={mode === 'register' ? styles.tabActive : styles.tab}
            onClick={() => handleModeChange('register')}
          >
            Create account
          </button>
        </div>

        <form className={styles.form} onSubmit={handleSubmit}>
          <Input
            label="Email"
            type="email"
            name="email"
            autoComplete="email"
            value={values.email}
            onChange={handleChange}
            error={errors.email}
            placeholder="name@domain.com"
          />

          <Input
            label="Password"
            type="password"
            name="password"
            autoComplete={mode === 'register' ? 'new-password' : 'current-password'}
            value={values.password}
            onChange={handleChange}
            error={errors.password}
            placeholder="Enter your password"
            hint={mode === 'register' ? 'Your password must be at least 8 characters long.' : ''}
          />

          {message ? (
            <div
              className={
                messageType === 'success'
                  ? styles.successMessage
                  : messageType === 'error'
                    ? styles.errorMessage
                    : styles.neutralMessage
              }
              role={messageType === 'error' ? 'alert' : 'status'}
            >
              {message}
            </div>
          ) : null}

          <Button type="submit" fullWidth disabled={isSubmitting}>
            {isSubmitting ? <Spinner label="Sending" /> : mode === 'register' ? 'Send verification link' : 'Send sign-in link'}
          </Button>

          <p className={styles.bottomText}>
            A verification link will be sent to your email address.
          </p>

          <p className={styles.bottomNote}>
            By continuing, you agree to receive emails from AirVibe.
          </p>
        </form>

        <div className={styles.footerLink}>
          <Link to={ROUTES.home}>Go to home</Link>
        </div>
      </section>
    </div>
  );
}
