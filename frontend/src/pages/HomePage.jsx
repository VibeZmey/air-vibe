import { AppShell } from '../components/layout/AppShell';
import { useAuthStore } from '../store/authStore';
import styles from './HomePage.module.css';

export function HomePage() {
  const user = useAuthStore((state) => state.user);

  return (
    <AppShell
      title="Welcome to AirVibe"
      subtitle="Your account is ready. This is your space for flights, bookings, and travel plans."
    >
      <div className={styles.grid}>
        <section className={styles.card}>
          <span className={styles.cardLabel}>Profile</span>
          <h2>{user?.login || 'Traveler'}</h2>
          <div className={styles.list}>
            <div>
              <span>Email</span>
              <strong>{user?.email || 'Not set'}</strong>
            </div>
            <div>
              <span>Country</span>
              <strong>{user?.country || 'Not set'}</strong>
            </div>
            <div>
              <span>Citizenship</span>
              <strong>{user?.citizenship || 'Not set'}</strong>
            </div>
            <div>
              <span>Currency</span>
              <strong>{user?.currency || 'Not set'}</strong>
            </div>
          </div>
        </section>

        <section className={styles.cardAccent}>
          <span className={styles.cardLabel}>Next</span>
          <h2>Discover flights, compare options, and plan your next trip</h2>
          <p>Everything you need for a smooth travel experience will live here.</p>
          <div className={styles.pillRow}>
            <span>Flights</span>
            <span>Bookings</span>
            <span>Profile</span>
          </div>
        </section>
      </div>
    </AppShell>
  );
}
