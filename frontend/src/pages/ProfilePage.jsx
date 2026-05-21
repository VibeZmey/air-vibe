import { useState, useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import { AppShell } from '../components/layout/AppShell';
import { apiClient } from '../api/client';
import { useAuthStore } from '../store/authStore';
import styles from './ProfilePage.module.css';

const TABS = {
  personal: 'personal',
  passengers: 'passengers',
  orders: 'orders',
};

// Enum mappings from backend
const PASSENGER_TYPE = {
  Adult: 0,
  Kid: 1,
  Baby: 2,
  None: 3,
};

const PASSENGER_TYPE_NAMES = {
  0: 'Adult',
  1: 'Kid',
  2: 'Baby',
  3: 'None',
};

const DOCUMENT_TYPE = {
  Passport: 0,
  ForeignPassport: 1,
  BirthCertificate: 2,
  Other: 3,
};

const DOCUMENT_TYPE_NAMES = {
  0: 'Passport',
  1: 'Foreign Passport',
  2: 'Birth Certificate',
  3: 'Other',
};

const GENDER = {
  Male: 0,
  Female: 1,
};

const GENDER_NAMES = {
  0: 'Male',
  1: 'Female',
};

const ORDER_STATUS = {
  0: 'Pending',
  1: 'Confirmed',
  2: 'Completed',
  3: 'Cancelled',
};

function PersonalTab({ user, onUserUpdate }) {
  const [isEditing, setIsEditing] = useState(false);
  const [editData, setEditData] = useState({
    country: user?.country || '',
    citizenship: user?.citizenship || '',
    currency: user?.currency || '',
  });
  const [passwordData, setPasswordData] = useState({
    currentPassword: '',
    currentPasswordConfirm: '',
    newPassword: '',
  });
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loadingPassword, setLoadingPassword] = useState(false);

  async function handlePasswordChange(e) {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (passwordData.currentPassword !== passwordData.currentPasswordConfirm) {
      setError('Current passwords do not match');
      return;
    }

    if (!passwordData.newPassword || passwordData.newPassword.length < 8) {
      setError('New password must be at least 8 characters');
      return;
    }

    setLoadingPassword(true);
    try {
      await apiClient.post('/users/change/password', {
        oldPassword: passwordData.currentPassword,
        newPassword: passwordData.newPassword,
      });
      setSuccess('Password changed successfully');
      setPasswordData({ currentPassword: '', currentPasswordConfirm: '', newPassword: '' });
    } catch (err) {
      setError('Failed to change password. Please check your current password.');
    } finally {
      setLoadingPassword(false);
    }
  }

  return (
    <div className={styles.tab}>
      <h3>Personal Information</h3>
      
      <div className={styles.infoBlock}>
        <div className={styles.infoRow}>
          <span className={styles.label}>Email:</span>
          <span className={styles.value}>{user?.email}</span>
        </div>
        <div className={styles.infoRow}>
          <span className={styles.label}>Country:</span>
          <span className={styles.value}>{user?.country || 'Not specified'}</span>
        </div>
        <div className={styles.infoRow}>
          <span className={styles.label}>Citizenship:</span>
          <span className={styles.value}>{user?.citizenship || 'Not specified'}</span>
        </div>
        <div className={styles.infoRow}>
          <span className={styles.label}>Currency:</span>
          <span className={styles.value}>{user?.currency || 'Not specified'}</span>
        </div>
        <div className={styles.infoRow}>
          <span className={styles.label}>Member since:</span>
          <span className={styles.value}>
            {user?.createdAt ? new Date(user.createdAt).toLocaleDateString('en-US') : 'Unknown'}
          </span>
        </div>
      </div>

      <div className={styles.section}>
        <h4>Change Password</h4>
        <form onSubmit={handlePasswordChange} className={styles.form}>
          <div className={styles.formGroup}>
            <label>Current Password</label>
            <input
              type="password"
              value={passwordData.currentPassword}
              onChange={(e) => setPasswordData({ ...passwordData, currentPassword: e.target.value })}
              placeholder="Enter current password"
              required
            />
          </div>

          <div className={styles.formGroup}>
            <label>Confirm Current Password</label>
            <input
              type="password"
              value={passwordData.currentPasswordConfirm}
              onChange={(e) => setPasswordData({ ...passwordData, currentPasswordConfirm: e.target.value })}
              placeholder="Confirm current password"
              required
            />
          </div>

          <div className={styles.formGroup}>
            <label>New Password</label>
            <input
              type="password"
              value={passwordData.newPassword}
              onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
              placeholder="Enter new password (minimum 8 characters)"
              required
            />
          </div>

          {error && <div className={styles.error}>{error}</div>}
          {success && <div className={styles.success}>{success}</div>}

          <button type="submit" className={styles.submitBtn} disabled={loadingPassword}>
            {loadingPassword ? 'Changing...' : 'Change Password'}
          </button>
        </form>
      </div>
    </div>
  );
}

function PassengersTab() {
   const [passengers, setPassengers] = useState([]);
   const [loading, setLoading] = useState(true);
   const [error, setError] = useState('');
   const [expandedId, setExpandedId] = useState(null);
   const [newPassengerType, setNewPassengerType] = useState(0);
   const [showAddForm, setShowAddForm] = useState(false);
   const [addingPassenger, setAddingPassenger] = useState(false);
   const [showAddDocForm, setShowAddDocForm] = useState(null);
   const [docData, setDocData] = useState({
     type: 'Passport',
     gender: 'Male',
     firstName: '',
     middleName: '',
     lastName: '',
     number: '',
     series: '',
     dateOfBirth: '',
     validityPeriod: '',
   });
   const [addingDoc, setAddingDoc] = useState(false);
   const user = useAuthStore((state) => state.user);

  useEffect(() => {
    fetchPassengers();
  }, []);

  async function fetchPassengers() {
    try {
      const response = await apiClient.get('/passengers/me');
      setPassengers(Array.isArray(response.data) ? response.data : []);
      setError('');
    } catch (err) {
      setError('Failed to load passengers');
      setPassengers([]);
    } finally {
      setLoading(false);
    }
  }

   async function addPassenger() {
     if (!user?.id) return;
     
     setAddingPassenger(true);
     try {
       await apiClient.post('/passengers', {
         userId: user.id,
         type: newPassengerType,
         isSaved: false,
       });
       setShowAddForm(false);
       setNewPassengerType(0);
       await fetchPassengers();
     } catch (err) {
       setError('Failed to add passenger');
     } finally {
       setAddingPassenger(false);
     }
   }

  async function deletePassenger(passengerId) {
    if (!confirm('Are you sure you want to delete this passenger?')) return;
    
    try {
      await apiClient.delete(`/passengers/${passengerId}`);
      await fetchPassengers();
    } catch (err) {
      setError('Failed to delete passenger');
    }
  }

   async function deleteDocument(docId) {
     if (!confirm('Are you sure you want to delete this document?')) return;
     
     try {
       await apiClient.delete(`/docs/${docId}`);
       await fetchPassengers();
     } catch (err) {
       setError('Failed to delete document');
     }
   }

    async function addDocument(passengerId) {
      setAddingDoc(true);
      try {
        await apiClient.post('/documents', {
          passengerId: passengerId,
          type: DOCUMENT_TYPE[docData.type],
          firstName: docData.firstName,
          middleName: docData.middleName,
          lastName: docData.lastName,
          gender: GENDER[docData.gender],
          dateOfBirth: docData.dateOfBirth,
          validityPeriod: docData.validityPeriod,
          number: docData.number,
          series: docData.series,
        });
       setShowAddDocForm(null);
       setDocData({
         type: 'Passport',
         gender: 'Male',
         firstName: '',
         middleName: '',
         lastName: '',
         number: '',
         series: '',
         dateOfBirth: '',
         validityPeriod: '',
       });
       await fetchPassengers();
     } catch (err) {
       setError('Failed to add document');
     } finally {
       setAddingDoc(false);
     }
   }

  return (
    <div className={styles.tab}>
      <div className={styles.tabHeader}>
        <h3>My Passengers</h3>
        <button className={styles.addBtn} onClick={() => setShowAddForm(!showAddForm)}>
          {showAddForm ? 'Cancel' : '+ Add Passenger'}
        </button>
      </div>

       {showAddForm && (
         <div className={styles.addForm}>
           <div className={styles.formGroup}>
             <label>Passenger Type</label>
             <select value={newPassengerType} onChange={(e) => setNewPassengerType(parseInt(e.target.value))}>
               <option value="0">Adult</option>
               <option value="1">Kid</option>
               <option value="2">Baby</option>
             </select>
           </div>
           <button onClick={addPassenger} disabled={addingPassenger} className={styles.submitBtn}>
             {addingPassenger ? 'Adding...' : 'Add Passenger'}
           </button>
         </div>
       )}

      {loading ? (
        <div className={styles.loading}>Loading passengers...</div>
      ) : error ? (
        <div className={styles.error}>{error}</div>
      ) : passengers.length === 0 ? (
        <div className={styles.empty}>No passengers added yet.</div>
      ) : (
        <div className={styles.passengersList}>
           {passengers.map((passenger) => {
             const passengerId = passenger.id || passenger.Id;
             const passengerType = PASSENGER_TYPE_NAMES[passenger.type] || PASSENGER_TYPE_NAMES[passenger.Type] || PASSENGER_TYPE_NAMES[0];
             const firstDoc = passenger.documents && passenger.documents.length > 0 ? passenger.documents[0] : null;
             const passengerName = firstDoc 
               ? `${firstDoc.firstName || firstDoc.FirstName} ${firstDoc.lastName || firstDoc.LastName}`
               : passengerType;
             
             return (
             <div key={passengerId} className={styles.passengerCard}>
               <div className={styles.passengerHeader}>
                 <div>
                   <h4>{passengerName}</h4>
                   <small>{passengerType}</small>
                   <button
                     className={styles.expandBtn}
                     onClick={() => setExpandedId(expandedId === passengerId ? null : passengerId)}
                   >
                     {expandedId === passengerId ? 'Hide Documents' : 'View Documents'}
                   </button>
                 </div>
                 <button
                   className={styles.deleteBtn}
                   onClick={() => deletePassenger(passengerId)}
                 >
                   Delete
                 </button>
               </div>

               {expandedId === passengerId && (
                 <div className={styles.documentsSection}>
                   <div className={styles.documentsHeader}>
                     <h5>Documents</h5>
                     <button 
                       className={styles.addBtn}
                       onClick={() => setShowAddDocForm(showAddDocForm === passengerId ? null : passengerId)}
                     >
                       {showAddDocForm === passengerId ? 'Cancel' : '+ Add Document'}
                     </button>
                   </div>

                   {showAddDocForm === passengerId && (
                    <div className={styles.addDocForm}>
                      <div className={styles.formRow}>
                        <div className={styles.formGroup}>
                          <label>Document Type</label>
                          <select value={docData.type} onChange={(e) => setDocData({ ...docData, type: e.target.value })}>
                            <option value="Passport">Passport</option>
                            <option value="ForeignPassport">Foreign Passport</option>
                            <option value="BirthCertificate">Birth Certificate</option>
                            <option value="Other">Other</option>
                          </select>
                        </div>
                        <div className={styles.formGroup}>
                          <label>Gender</label>
                          <select value={docData.gender} onChange={(e) => setDocData({ ...docData, gender: e.target.value })}>
                            <option value="Male">Male</option>
                            <option value="Female">Female</option>
                          </select>
                        </div>
                      </div>

                      <div className={styles.formRow}>
                        <div className={styles.formGroup}>
                          <label>First Name</label>
                          <input
                            type="text"
                            value={docData.firstName}
                            onChange={(e) => setDocData({ ...docData, firstName: e.target.value })}
                            placeholder="First name"
                            required
                          />
                        </div>
                        <div className={styles.formGroup}>
                          <label>Middle Name</label>
                          <input
                            type="text"
                            value={docData.middleName}
                            onChange={(e) => setDocData({ ...docData, middleName: e.target.value })}
                            placeholder="Middle name (optional)"
                          />
                        </div>
                        <div className={styles.formGroup}>
                          <label>Last Name</label>
                          <input
                            type="text"
                            value={docData.lastName}
                            onChange={(e) => setDocData({ ...docData, lastName: e.target.value })}
                            placeholder="Last name"
                            required
                          />
                        </div>
                      </div>

                      <div className={styles.formRow}>
                        <div className={styles.formGroup}>
                          <label>Document Number</label>
                          <input
                            type="text"
                            value={docData.number}
                            onChange={(e) => setDocData({ ...docData, number: e.target.value })}
                            placeholder="Document number"
                            required
                          />
                        </div>
                        <div className={styles.formGroup}>
                          <label>Document Series</label>
                          <input
                            type="text"
                            value={docData.series}
                            onChange={(e) => setDocData({ ...docData, series: e.target.value })}
                            placeholder="Series (optional)"
                          />
                        </div>
                      </div>

                      <div className={styles.formRow}>
                        <div className={styles.formGroup}>
                          <label>Date of Birth</label>
                          <input
                            type="date"
                            value={docData.dateOfBirth}
                            onChange={(e) => setDocData({ ...docData, dateOfBirth: e.target.value })}
                            required
                          />
                        </div>
                        <div className={styles.formGroup}>
                          <label>Validity Period</label>
                          <input
                            type="date"
                            value={docData.validityPeriod}
                            onChange={(e) => setDocData({ ...docData, validityPeriod: e.target.value })}
                            placeholder="Validity period (optional)"
                          />
                        </div>
                      </div>

                      {error && <div className={styles.error}>{error}</div>}

                       <button 
                         onClick={() => addDocument(passengerId)} 
                         disabled={addingDoc} 
                         className={styles.submitBtn}
                       >
                         {addingDoc ? 'Adding...' : 'Add Document'}
                       </button>
                    </div>
                  )}

                   {passenger.documents && passenger.documents.length > 0 ? (
                     <div className={styles.documentsList}>
                       {passenger.documents.map((doc) => {
                         const docId = doc.id || doc.Id;
                         const docType = DOCUMENT_TYPE_NAMES[doc.type] || DOCUMENT_TYPE_NAMES[doc.Type] || doc.type;
                         return (
                         <div key={docId} className={styles.documentItem}>
                           <div>
                             <strong>{docType}</strong>
                             <p>
                               {doc.firstName || doc.FirstName} {doc.lastName || doc.LastName}
                             </p>
                             <small>DOB: {new Date(doc.dateOfBirth || doc.DateOfBirth).toLocaleDateString()}</small>
                           </div>
                           <button
                             className={styles.deleteBtn}
                             onClick={() => deleteDocument(docId)}
                           >
                             Delete
                           </button>
                         </div>
                         );
                       })}
                     </div>
                   ) : (
                     <p className={styles.empty}>No documents added</p>
                   )}
                 </div>
               )}
             </div>
             );
           })}
         </div>
      )}
    </div>
  );
}
function OrdersTab() {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [expandedOrderId, setExpandedOrderId] = useState(null);
  const user = useAuthStore((state) => state.user);

  useEffect(() => {
    if (user?.id) {
      fetchOrders();
    }
  }, [user?.id]);

  async function fetchOrders() {
    try {
      const response = await apiClient.get(`/orders/me`);
      if (Array.isArray(response.data)) {
        setOrders(response.data);
        setError('');
      } else {
        setOrders([]);
        setError('Invalid orders data');
      }
    } catch (err) {
      setError('Failed to load orders');
      setOrders([]);
    } finally {
      setLoading(false);
    }
  }

  function getStatusClass(status) {
    const statusMap = {
      0: styles.orderStatusPending,
      1: styles.orderStatusConfirmed,
      2: styles.orderStatusExpired,
      3: styles.orderStatusCancelled,
      Pending: styles.orderStatusPending,
      Confirmed: styles.orderStatusConfirmed,
      Expired: styles.orderStatusExpired,
      Cancelled: styles.orderStatusCancelled,
    };
    return statusMap[status] || styles.orderStatusPending;
  }

  function getStatusText(status) {
    const statusMap = {
      0: 'Pending',
      1: 'Confirmed',
      2: 'Expired',
      3: 'Cancelled',
      Pending: 'Pending',
      Confirmed: 'Confirmed',
      Expired: 'Expired',
      Cancelled: 'Cancelled',
    };
    return statusMap[status] || 'Unknown';
  }

  return (
    <div className={styles.tab}>
      <h3>My Orders</h3>
      {loading ? (
        <div className={styles.loading}>Loading orders...</div>
      ) : error ? (
        <div className={styles.error}>{error}</div>
      ) : orders.length === 0 ? (
        <div className={styles.empty}>You haven't placed any orders yet.</div>
      ) : (
        <div className={styles.ordersList}>
          {orders.map((order) => {
            const orderId = order.orderId || order.id;
            const shortId = orderId ? orderId.split('-')[0] : 'N/A';
            const createdAt = order.createdAt ? new Date(order.createdAt).toLocaleDateString('en-US', {
              year: 'numeric',
              month: 'short',
              day: 'numeric',
            }) : 'Unknown';
            const status = order.status;
            const totalPrice = order.totalPrice || 0;
            const bookings = order.bookings || [];
            const isExpanded = expandedOrderId === orderId;

            return (
              <div key={orderId} className={styles.orderCard}>
                <div className={styles.orderMainHeader}>
                  <div className={styles.orderIdBlock}>
                    <div className={styles.orderStatusBadge + ' ' + getStatusClass(status)}>
                      {getStatusText(status)}
                    </div>
                    <div className={styles.orderId}>Order #{shortId}</div>
                    <div className={styles.orderDate}>{createdAt}</div>
                  </div>
                  <div className={styles.orderTotalPrice}>
                    {new Intl.NumberFormat('en-US', {
                      style: 'currency',
                      currency: 'USD',
                    }).format(totalPrice)}
                  </div>
                </div>

                <button
                  className={styles.orderToggleBtn}
                  onClick={() => setExpandedOrderId(isExpanded ? null : orderId)}
                >
                  {isExpanded ? 'Hide Bookings' : `Show Bookings (${bookings.length})`}
                </button>

                {isExpanded && bookings.length > 0 && (
                  <div className={styles.bookingsList}>
                    {bookings.map((booking, index) => {
                      const flight = booking.flight || {};
                      const fromCity = flight.fromAirport?.city || flight.FromAirport?.city || 'Unknown';
                      const toCity = flight.toAirport?.city || flight.ToAirport?.city || 'Unknown';
                      const departureTime = flight.departureTime || flight.DepartureTime;
                      const passengerName = `${booking.firstName || ''} ${booking.lastName || ''}`.trim() || 'Passenger';

                      return (
                        <div key={booking.id || index} className={styles.bookingItem}>
                          <div className={styles.bookingHeader}>
                            <div>
                              <h4 className={styles.bookingRoute}>{fromCity} → {toCity}</h4>
                              <p className={styles.bookingPassenger}>
                                {passengerName} • {departureTime ? new Date(departureTime).toLocaleDateString('en-US') : 'N/A'}
                              </p>
                            </div>
                            <div className={styles.bookingSeat}>
                              Seat: {booking.seatNumber || booking.SeatNumber || 'N/A'}
                            </div>
                          </div>
                          <div className={styles.bookingDetails}>
                            <span>Booking ID: {booking.id || booking.Id}</span>
                            <span>
                              Amenities:
                              {booking.hasLuggage ? ' Luggage' : ''}
                              {booking.hasFood ? ' Food' : ''}
                              {booking.isBusiness ? ' Business' : ''}
                              {(!booking.hasLuggage && !booking.hasFood && !booking.isBusiness) ? ' None' : ''}
                            </span>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

export function ProfilePage() {
  const location = useLocation();
  const user = useAuthStore((state) => state.user);
  const [userInfo, setUserInfo] = useState(user);
  const [activeTab, setActiveTab] = useState(() => {
    // Check if tab was passed via location state
    return location.state?.tab || TABS.personal;
  });
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchUserInfo() {
      try {
        const response = await apiClient.get('/users/me');
        setUserInfo(response.data);
      } catch (err) {
        console.error('Failed to load user info');
      } finally {
        setLoading(false);
      }
    }

    fetchUserInfo();
  }, []);

  if (loading) {
    return (
      <AppShell title="My Account" subtitle="View and manage your profile">
        <div className={styles.container}>
          <div className={styles.loading}>Loading profile...</div>
        </div>
      </AppShell>
    );
  }

  return (
    <AppShell title="My Account" subtitle="View and manage your profile">
      <div className={styles.container}>
        <div className={styles.layout}>
          <aside className={styles.sidebar}>
            <div className={styles.userCard}>
              <div className={styles.avatar}>{userInfo?.email?.[0]?.toUpperCase() || 'U'}</div>
              <div className={styles.userDetails}>
                <h3>{userInfo?.email || 'User'}</h3>
              </div>
            </div>

            <nav className={styles.tabsNav}>
              <button
                className={`${styles.tabBtn} ${activeTab === TABS.personal ? styles.tabBtnActive : ''}`}
                onClick={() => setActiveTab(TABS.personal)}
              >
                Personal Info
              </button>
              <button
                className={`${styles.tabBtn} ${activeTab === TABS.passengers ? styles.tabBtnActive : ''}`}
                onClick={() => setActiveTab(TABS.passengers)}
              >
                Passengers
              </button>
              <button
                className={`${styles.tabBtn} ${activeTab === TABS.orders ? styles.tabBtnActive : ''}`}
                onClick={() => setActiveTab(TABS.orders)}
              >
                Orders
              </button>
            </nav>
          </aside>

          <main className={styles.content}>
            {activeTab === TABS.personal && <PersonalTab user={userInfo} />}
            {activeTab === TABS.passengers && <PassengersTab />}
            {activeTab === TABS.orders && <OrdersTab />}
          </main>
        </div>
      </div>
    </AppShell>
  );
}



