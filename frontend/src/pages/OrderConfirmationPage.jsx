import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { AppShell } from '../components/layout/AppShell';
import { apiClient } from '../api/client';
import { useAuthStore } from '../store/authStore';
import styles from './OrderConfirmationPage.module.css';

export function OrderConfirmationPage() {
  const { orderId } = useParams();
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  
  const [order, setOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    async function loadOrder() {
      if (!orderId) {
        setError('No order ID provided');
        return;
      }

      try {
        // Fetch order details
        const response = await apiClient.get(`/orders/${orderId}`);
        setOrder(response.data);
      } catch (err) {
        setError('Failed to load order details');
        console.error(err);
      } finally {
        setLoading(false);
      }
    }

    loadOrder();
  }, [orderId]);

  async function handleConfirm() {
    setProcessing(true);
    setError('');
    
    try {
      await apiClient.post(`/orders/${orderId}/confirm`);
      setSuccess('Order confirmed successfully!');
      setTimeout(() => {
        navigate('/profile', { state: { tab: 'orders' } });
      }, 2000);
    } catch (err) {
      setError(`Failed to confirm order: ${err.response?.data?.message || err.message}`);
    } finally {
      setProcessing(false);
    }
  }

  async function handleCancel() {
    if (!window.confirm('Are you sure you want to cancel this order?')) {
      return;
    }

    setProcessing(true);
    setError('');
    
    try {
      await apiClient.post(`/orders/${orderId}/cancel`);
      navigate('/profile', { state: { tab: 'orders' } });
    } catch (err) {
      setError(`Failed to cancel order: ${err.response?.data?.message || err.message}`);
    } finally {
      setProcessing(false);
    }
  }

  if (loading) {
    return (
      <AppShell title="Order Confirmation" subtitle="Review your booking before confirming">
        <div className={styles.container}>
          <div className={styles.loading}>Loading order details...</div>
        </div>
      </AppShell>
    );
  }

  if (!order) {
    return (
      <AppShell title="Order Confirmation" subtitle="Review your booking before confirming">
        <div className={styles.container}>
          <div className={styles.error}>{error || 'Order not found'}</div>
          <button onClick={() => navigate('/profile')} className={styles.backBtn}>
            Go Back
          </button>
        </div>
      </AppShell>
    );
  }

  return (
    <AppShell title="Order Confirmation" subtitle="Please confirm or cancel your booking">
      <div className={styles.container}>
        <div className={styles.content}>
          <div className={styles.confirmationBox}>
            <div className={styles.checkmark}>✓</div>
            <h1>Booking Complete!</h1>
            <p>Your booking has been created. Please review the details below and confirm to proceed.</p>

            <div className={styles.orderDetails}>
              <div className={styles.detailRow}>
                <span className={styles.label}>Order ID:</span>
                <span className={styles.value}>{orderId}</span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.label}>Status:</span>
                <span className={`${styles.value} ${styles.statusPending}`}>Pending Confirmation</span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.label}>Total Price:</span>
                <span className={styles.value}>${order.totalPrice?.toFixed(2) || '0.00'}</span>
              </div>
              <div className={styles.detailRow}>
                <span className={styles.label}>Bookings:</span>
                <span className={styles.value}>{order.bookings?.length || 0} passenger(s)</span>
              </div>
            </div>

            {order.bookings && order.bookings.length > 0 && (
              <div className={styles.bookingsList}>
                <h2>Bookings</h2>
                {order.bookings.map((booking, i) => (
                  <div key={i} className={styles.bookingCard}>
                    <div className={styles.bookingInfo}>
                      <p className={styles.passengerName}>
                        {booking.passengerName || booking.PassengerName || `Passenger ${i + 1}`}
                      </p>
                      <p className={styles.seatInfo}>
                        Seat: {booking.seatNumber || booking.SeatNumber}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            )}

            {error && <div className={styles.errorMessage}>{error}</div>}
            {success && <div className={styles.successMessage}>{success}</div>}

            <div className={styles.actions}>
              <button
                className={styles.cancelBtn}
                onClick={handleCancel}
                disabled={processing}
              >
                {processing ? 'Processing...' : 'Cancel Order'}
              </button>
              <button
                className={styles.confirmBtn}
                onClick={handleConfirm}
                disabled={processing}
              >
                {processing ? 'Confirming...' : 'Confirm Order'}
              </button>
            </div>

            <p className={styles.note}>
              Please confirm your booking to complete the purchasing process. You can still cancel if needed.
            </p>
          </div>
        </div>
      </div>
    </AppShell>
  );
}

