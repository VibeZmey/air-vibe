import styles from './Spinner.module.css';

export function Spinner({ label = 'Loading' }) {
  return (
    <span className={styles.spinner} role="status" aria-label={label}>
      <span />
      <span />
      <span />
    </span>
  );
}
