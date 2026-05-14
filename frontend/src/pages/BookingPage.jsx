import { useState, useEffect } from 'react';
import { useParams, useLocation, useNavigate } from 'react-router-dom';
import { AppShell } from '../components/layout/AppShell';
import { apiClient } from '../api/client';
import { useAuthStore } from '../store/authStore';
import styles from './BookingPage.module.css';

// Enum mappings
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

const GENDER = {
  Male: 0,
  Female: 1,
};

const FLIGHT_STATUS = {
  Scheduled: 0,
  CheckIn: 1,
  Boarding: 2,
  Departed: 3,
  Arrived: 4,
  Cancelled: 5,
  Delayed: 6,
};

function Step1PassengerDocuments({ flight, passengers, onNext, onBack }) {
  const user = useAuthStore((state) => state.user);
  const [passengerData, setPassengerData] = useState([]);
  const [savedPassengers, setSavedPassengers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [creating, setCreating] = useState(false);

  useEffect(() => {
    async function loadPassengers() {
      try {
        const response = await apiClient.get('/passengers/me');
        setSavedPassengers(Array.isArray(response.data) ? response.data : []);
      } catch (err) {
        console.error('Failed to load saved passengers:', err);
      } finally {
        setLoading(false);
      }
    }

    loadPassengers();

    // Initialize passenger data for each passenger
    const totalPassengers = passengers.adults + passengers.kids + passengers.babies;
    setPassengerData(
      Array.from({ length: totalPassengers }, (_, i) => ({
        index: i,
        type: i < passengers.adults ? PASSENGER_TYPE.Adult : i < passengers.adults + passengers.kids ? PASSENGER_TYPE.Kid : PASSENGER_TYPE.Baby,
        useExisting: false,
        existingPassengerId: null,
        documents: {
          type: 'Passport',
          gender: 'Male',
          firstName: '',
          middleName: '',
          lastName: '',
          number: '',
          series: '',
          dateOfBirth: '',
          validityPeriod: '',
        },
      }))
    );
  }, [passengers]);

  // Check if all passengers have required fields filled
  const allFieldsFilled = passengerData.every((p) => {
    if (p.useExisting && p.existingPassengerId) return true;
    return (
      p.documents.firstName &&
      p.documents.lastName &&
      p.documents.number &&
      p.documents.dateOfBirth &&
      p.documents.type &&
      p.documents.gender
    );
  });

  async function handleNext() {
    setCreating(true);
    setError('');

    try {
      const createdPassengers = [];

      for (let i = 0; i < passengerData.length; i++) {
        const pData = passengerData[i];
        let passengerId;

        if (pData.useExisting && pData.existingPassengerId) {
          passengerId = pData.existingPassengerId;
        } else {
          // Create passenger
          const passengerResp = await apiClient.post('/passengers', {
            userId: user.id,
            type: pData.type,
            isSaved: true,
          });
          passengerId = passengerResp.data.id || passengerResp.data.Id;

          // Create document
          await apiClient.post('/documents', {
            passengerId: passengerId,
            type: DOCUMENT_TYPE[pData.documents.type] || 0,
            firstName: pData.documents.firstName,
            middleName: pData.documents.middleName,
            lastName: pData.documents.lastName,
            gender: GENDER[pData.documents.gender] || 0,
            dateOfBirth: pData.documents.dateOfBirth,
            validityPeriod: pData.documents.validityPeriod || null,
            number: pData.documents.number,
            series: pData.documents.series || null,
            userId: user.id,
          });
        }

        createdPassengers.push({
          ...pData,
          savedPassengerId: passengerId,
        });
      }

      onNext(createdPassengers);
    } catch (err) {
      setError(`Failed to create passengers: ${err.response?.data?.message || err.message}`);
    } finally {
      setCreating(false);
    }
  }

  return (
    <div className={styles.stepContent}>
      <h2>Step 1: Passenger Information</h2>
      <p>Add or select passengers with their documents for this booking.</p>

      {error && <div className={styles.error}>{error}</div>}

      {loading ? (
        <div className={styles.loading}>Loading saved passengers...</div>
      ) : (
        <div className={styles.passengersGrid}>
          {passengerData.map((pData) => (
            <div key={pData.index} className={styles.passengerCard}>
              <div className={styles.passengerTitle}>
                Passenger {pData.index + 1} ({PASSENGER_TYPE_NAMES[pData.type]})
              </div>

              <div className={styles.toggleOption}>
                <label>
                  <input
                    type="checkbox"
                    checked={pData.useExisting}
                    onChange={(e) => {
                      const updated = [...passengerData];
                      updated[pData.index].useExisting = e.target.checked;
                      setPassengerData(updated);
                    }}
                  />
                  Use existing passenger
                </label>
              </div>

              {pData.useExisting && savedPassengers.length > 0 ? (
                <select
                  value={pData.existingPassengerId || ''}
                  onChange={(e) => {
                    const updated = [...passengerData];
                    updated[pData.index].existingPassengerId = e.target.value;
                    setPassengerData(updated);
                  }}
                  className={styles.selectPassenger}
                >
                  <option value="">Select saved passenger...</option>
                  {savedPassengers.map((p) => {
                    const id = p.id || p.Id;
                    const doc = p.documents && p.documents.length > 0 ? p.documents[0] : null;
                    const name = doc ? `${doc.firstName || doc.FirstName} ${doc.lastName || doc.LastName}` : `Passenger ${id}`;
                    return (
                      <option key={id} value={id}>
                        {name}
                      </option>
                    );
                  })}
                </select>
              ) : (
                <div className={styles.documentForm}>
                  <div className={styles.formRow}>
                    <div className={styles.formGroup}>
                      <label>Document Type</label>
                      <select
                        value={pData.documents.type}
                        onChange={(e) => {
                          const updated = [...passengerData];
                          updated[pData.index].documents.type = e.target.value;
                          setPassengerData(updated);
                        }}
                      >
                        <option value="Passport">Passport</option>
                        <option value="ForeignPassport">Foreign Passport</option>
                        <option value="BirthCertificate">Birth Certificate</option>
                        <option value="Other">Other</option>
                      </select>
                    </div>
                    <div className={styles.formGroup}>
                      <label>Gender</label>
                      <select
                        value={pData.documents.gender}
                        onChange={(e) => {
                          const updated = [...passengerData];
                          updated[pData.index].documents.gender = e.target.value;
                          setPassengerData(updated);
                        }}
                      >
                        <option value="Male">Male</option>
                        <option value="Female">Female</option>
                      </select>
                    </div>
                  </div>

                  <div className={styles.formRow}>
                    <div className={styles.formGroup}>
                      <label>First Name *</label>
                      <input
                        type="text"
                        value={pData.documents.firstName}
                        onChange={(e) => {
                          const updated = [...passengerData];
                          updated[pData.index].documents.firstName = e.target.value;
                          setPassengerData(updated);
                        }}
                        placeholder="First name"
                      />
                    </div>
                    <div className={styles.formGroup}>
                      <label>Middle Name</label>
                      <input
                        type="text"
                        value={pData.documents.middleName}
                        onChange={(e) => {
                          const updated = [...passengerData];
                          updated[pData.index].documents.middleName = e.target.value;
                          setPassengerData(updated);
                        }}
                        placeholder="Middle name (optional)"
                      />
                    </div>
                    <div className={styles.formGroup}>
                      <label>Last Name *</label>
                      <input
                        type="text"
                        value={pData.documents.lastName}
                        onChange={(e) => {
                          const updated = [...passengerData];
                          updated[pData.index].documents.lastName = e.target.value;
                          setPassengerData(updated);
                        }}
                        placeholder="Last name"
                      />
                    </div>
                  </div>

                  <div className={styles.formRow}>
                    <div className={styles.formGroup}>
                      <label>Document Number *</label>
                      <input
                        type="text"
                        value={pData.documents.number}
                        onChange={(e) => {
                          const updated = [...passengerData];
                          updated[pData.index].documents.number = e.target.value;
                          setPassengerData(updated);
                        }}
                        placeholder="Document number"
                      />
                    </div>
                    <div className={styles.formGroup}>
                      <label>Series (optional)</label>
                      <input
                        type="text"
                        value={pData.documents.series}
                        onChange={(e) => {
                          const updated = [...passengerData];
                          updated[pData.index].documents.series = e.target.value;
                          setPassengerData(updated);
                        }}
                        placeholder="Series"
                      />
                    </div>
                  </div>

                  <div className={styles.formRow}>
                    <div className={styles.formGroup}>
                      <label>Date of Birth *</label>
                      <input
                        type="date"
                        value={pData.documents.dateOfBirth}
                        onChange={(e) => {
                          const updated = [...passengerData];
                          updated[pData.index].documents.dateOfBirth = e.target.value;
                          setPassengerData(updated);
                        }}
                      />
                    </div>
                    <div className={styles.formGroup}>
                      <label>Validity Period (optional)</label>
                      <input
                        type="date"
                        value={pData.documents.validityPeriod}
                        onChange={(e) => {
                          const updated = [...passengerData];
                          updated[pData.index].documents.validityPeriod = e.target.value;
                          setPassengerData(updated);
                        }}
                      />
                    </div>
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      <div className={styles.buttonRow}>
        <button className={styles.backBtn} onClick={onBack}>
          Back
        </button>
        <button className={styles.nextBtn} onClick={handleNext} disabled={!allFieldsFilled || creating}>
          {creating ? 'Creating...' : 'Next: Select Seats'}
        </button>
      </div>
    </div>
  );
}

function Step2SeatSelection({ flight, passengerData, onNext, onBack }) {
  const [selectedSeats, setSelectedSeats] = useState({});
  const bookedSeats = flight.bookings || [];

  useEffect(() => {
    // Initialize selected seats
    const seats = {};
    passengerData.forEach((p, i) => {
      seats[i] = null;
    });
    setSelectedSeats(seats);
  }, [passengerData]);

  // Generate seat map and logic
  const generateSeatMap = () => {
    const airplane = flight.airplane;
    const seats = [];

    // Business class seats (rows 1 to buisnessRows)
    for (let row = 1; row <= airplane.buisnessRows; row++) {
      const rowSeats = [];
      const cols = ['A', 'C', 'D', 'F']; // Skip B and E
      for (let col = 0; col < airplane.buisnessColumns; col++) {
        const seatLetter = cols[col];
        const seatNumber = `${row}${seatLetter}`;
        rowSeats.push({
          number: seatNumber,
          isBooked: bookedSeats.includes(seatNumber),
          isBusiness: true,
        });
      }
      seats.push({ type: 'business', rows: rowSeats });
    }

    // Spacer (visual delimiter at row 13)
    seats.push({ type: 'spacer' });

    // Economy class seats
    const economyStartRow = airplane.buisnessRows + 1;
    for (let row = economyStartRow; row < economyStartRow + airplane.rows; row++) {
      const rowSeats = [];
      for (let col = 0; col < airplane.columns; col++) {
        const seatLetter = String.fromCharCode(65 + col); // A, B, C, D, E, F
        const seatNumber = `${row}${seatLetter}`;
        rowSeats.push({
          number: seatNumber,
          isBooked: bookedSeats.includes(seatNumber),
          isBusiness: false,
        });
      }
      seats.push({ type: 'economy', rows: rowSeats });
    }

    return seats;
  };

  const seatMap = generateSeatMap();

  function handleSeatClick(seatNumber, passengerIndex) {
    setSelectedSeats((prev) => ({
      ...prev,
      [passengerIndex]: seatNumber,
    }));
  }

  const allSeatsSelected = Object.values(selectedSeats).every((s) => s !== null);

  return (
    <div className={styles.stepContent}>
      <h2>Step 2: Select Seats</h2>
      <p>Choose a seat for each passenger</p>

      <div className={styles.seatSelectionContainer}>
        <div className={styles.seatSelectionLeft}>
          <div className={styles.seatMap}>
            <div className={styles.seatTitle}>Airplane Seating Chart</div>
            <div className={styles.seatMapContent}>
              {seatMap.map((row, rowIndex) => {
                if (row.type === 'spacer') {
                  return <div key="spacer" className={styles.spacPlus}></div>;
                }
                return (
                  <div key={rowIndex} className={styles.seatRow}>
                    {row.rows.map((seat) => (
                      <button
                        key={seat.number}
                        className={`${styles.seat} ${seat.isBooked ? styles.seatBooked : ''} ${seat.isBusiness ? styles.seatBusiness : ''} ${
                          Object.values(selectedSeats).includes(seat.number) ? styles.seatSelected : ''
                        }`}
                        onClick={() => {
                          // Allow passenger to click and assign seat
                          // Find if there's a passenger with no seat for assignment
                          const unassignedIndex = passengerData.findIndex((_, i) => !selectedSeats[i] || selectedSeats[i] === null);
                          if (unassignedIndex >= 0) {
                            handleSeatClick(seat.number, unassignedIndex);
                          }
                        }}
                        disabled={seat.isBooked}
                        title={seat.isBooked ? 'Booked' : seat.number}
                      >
                        {seat.number}
                      </button>
                    ))}
                  </div>
                );
              })}
            </div>
          </div>
        </div>

        <div className={styles.passengerSeatAssignment}>
          <h3>Seat Assignment</h3>
          {passengerData.map((p, i) => (
            <div key={i} className={styles.passengerSeatRow}>
              <span>Passenger {i + 1}</span>
              <div className={styles.seatDisplay}>{selectedSeats[i] || 'Not assigned'}</div>
            </div>
          ))}
        </div>
      </div>

      <div className={styles.buttonRow}>
        <button className={styles.backBtn} onClick={onBack}>
          Back
        </button>
        <button className={styles.nextBtn} onClick={() => onNext(selectedSeats)} disabled={!allSeatsSelected}>
          Next: Review
        </button>
      </div>
    </div>
  );
}

function Step3Review({ flight, passengerData, selectedSeats, onConfirm, onBack, navigate }) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const user = useAuthStore((state) => state.user);

  async function handleConfirmBooking() {
    setLoading(true);
    setError('');

    try {
      // Prepare booking data
      const bookings = passengerData.map((p, i) => ({
        passengerId: p.savedPassengerId,
        flightId: flight.id || flight.Id,
        seatNumber: selectedSeats[i],
        hasLuggage: false,
        hasFood: false,
        isBusiness: false,
      }));

      // Create order
      const response = await apiClient.post('/orders', {
        userId: user.id,
        bookings: bookings,
      });

      const orderId = response.data || response.data.id;
      // Navigate to confirmation page
      navigate(`/order-confirmation/${orderId}`);
    } catch (err) {
      setError(`Failed to create booking: ${err.response?.data?.message || err.message}`);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className={styles.stepContent}>
      <h2>Step 3: Review Your Booking</h2>
      <p>Please review your booking details before confirming.</p>

      {error && <div className={styles.error}>{error}</div>}

      <div className={styles.reviewSummary}>
        <div className={styles.reviewSection}>
          <h3>Flight Details</h3>
          <div className={styles.reviewRow}>
            <span>Route:</span>
            <span>{flight?.fromAirport?.city || 'N/A'} → {flight?.toAirport?.city || 'N/A'}</span>
          </div>
          <div className={styles.reviewRow}>
            <span>Price per passenger:</span>
            <span>${flight?.flightPrice?.toFixed(2) || '0.00'}</span>
          </div>
        </div>

        <div className={styles.reviewSection}>
          <h3>Passengers & Seats</h3>
          {passengerData.map((p, i) => (
            <div key={i} className={styles.reviewRow}>
              <span>Passenger {i + 1}:</span>
              <span>{selectedSeats[i]} - Seat {selectedSeats[i]}</span>
            </div>
          ))}
        </div>

        <div className={styles.reviewTotal}>
          <h3>Total Price: ${((flight?.flightPrice || 0) * passengerData.length).toFixed(2)}</h3>
        </div>
      </div>

      <div className={styles.buttonRow}>
        <button className={styles.backBtn} onClick={onBack}>
          Back
        </button>
        <button className={styles.confirmBtn} onClick={handleConfirmBooking} disabled={loading}>
          {loading ? 'Booking...' : 'Confirm Booking'}
        </button>
      </div>
    </div>
  );
}

export function BookingPage() {
  const { flightId } = useParams();
  const location = useLocation();
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);

  const [flight, setFlight] = useState(null);
  const [passengers, setPassengers] = useState(location.state?.passengers || {});
  const [step, setStep] = useState(0);
  const [passengerData, setPassengerData] = useState([]);
  const [selectedSeats, setSelectedSeats] = useState({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    async function loadFlight() {
      if (!flightId) {
        setError('No flight selected');
        return;
      }

      try {
        const response = await apiClient.get(`/flights/${flightId}`);
        const flightData = response.data;

        const status = flightData.status;
        if (status !== FLIGHT_STATUS.Scheduled && status !== FLIGHT_STATUS.CheckIn) {
          setError('This flight is no longer available for booking.');
          return;
        }

        setFlight(flightData);
      } catch (err) {
        setError('Failed to load flight details');
        console.error(err);
      } finally {
        setLoading(false);
      }
    }

    loadFlight();
  }, [flightId]);

  if (loading) {
    return (
      <AppShell title="Book Your Flight" subtitle="Complete your booking">
        <div className={styles.container}>
          <div className={styles.loading}>Loading flight details...</div>
        </div>
      </AppShell>
    );
  }

  if (error || !flight) {
    return (
      <AppShell title="Book Your Flight" subtitle="Complete your booking">
        <div className={styles.container}>
          <div className={styles.error}>{error || 'Flight not found'}</div>
          <button onClick={() => navigate(-1)} className={styles.backBtn}>
            Go Back
          </button>
        </div>
      </AppShell>
    );
  }

  return (
    <AppShell title="Book Your Flight" subtitle="Complete your booking">
      <div className={styles.container}>
        <div className={styles.stepsIndicator}>
          <div className={`${styles.step} ${step === 0 ? styles.stepActive : step > 0 ? styles.stepComplete : ''}`}>
            0. Flight Info
          </div>
          <div className={`${styles.step} ${step === 1 ? styles.stepActive : step > 1 ? styles.stepComplete : ''}`}>
            1. Passengers
          </div>
          <div className={`${styles.step} ${step === 2 ? styles.stepActive : step > 2 ? styles.stepComplete : ''}`}>
            2. Seats
          </div>
          <div className={`${styles.step} ${step === 3 ? styles.stepActive : step > 3 ? styles.stepComplete : ''}`}>
            3. Review
          </div>
        </div>

        {step === 0 && (
          <div className={styles.stepContent}>
            <h2>Flight Information</h2>
            <div className={styles.flightInfoBox}>
              <div className={styles.infoSection}>
                <h3>Route</h3>
                <p>{flight?.fromAirport?.city || 'N/A'} → {flight?.toAirport?.city || 'N/A'}</p>
              </div>

              <div className={styles.infoGrid}>
                <div className={styles.infoSection}>
                  <h3>Departure</h3>
                  <p>
                    {flight?.departureTime
                      ? new Date(flight.departureTime).toLocaleDateString('en-US') +
                        ' at ' +
                        new Date(flight.departureTime).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })
                      : 'N/A'}
                  </p>
                </div>

                <div className={styles.infoSection}>
                  <h3>Arrival</h3>
                  <p>
                    {flight?.arrivalTime
                      ? new Date(flight.arrivalTime).toLocaleDateString('en-US') +
                        ' at ' +
                        new Date(flight.arrivalTime).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })
                      : 'N/A'}
                  </p>
                </div>
              </div>

              <div className={styles.infoGrid}>
                <div className={styles.infoSection}>
                  <h3>Aircraft</h3>
                  <p>{flight?.airplane?.name || 'N/A'}</p>
                </div>

                <div className={styles.infoSection}>
                  <h3>Duration</h3>
                  <p>
                    {flight?.durationMins
                      ? `${Math.floor(flight.durationMins / 60)}h ${flight.durationMins % 60}m`
                      : 'N/A'}
                  </p>
                </div>
              </div>

              <div className={styles.infoGrid}>
                <div className={styles.infoSection}>
                  <h3>Price per passenger</h3>
                  <p>${flight?.flightPrice?.toFixed(2) || '0.00'}</p>
                </div>

                <div className={styles.infoSection}>
                  <h3>Available seats</h3>
                  <p>{((flight?.totalSeats || 0) - (flight?.bookedSeats || 0))} seats</p>
                </div>
              </div>
            </div>

            <div className={styles.buttonRow}>
              <button className={styles.backBtn} onClick={() => navigate(-1)}>
                Cancel
              </button>
              <button className={styles.nextBtn} onClick={() => setStep(1)}>
                Proceed to Booking
              </button>
            </div>
          </div>
        )}

        {step === 1 && (
          <Step1PassengerDocuments
            flight={flight}
            passengers={passengers}
            onNext={(data) => {
              setPassengerData(data);
              setStep(2);
            }}
            onBack={() => setStep(0)}
          />
        )}

        {step === 2 && (
          <Step2SeatSelection
            flight={flight}
            passengerData={passengerData}
            onNext={(seats) => {
              setSelectedSeats(seats);
              setStep(3);
            }}
            onBack={() => setStep(1)}
          />
        )}

        {step === 3 && (
          <Step3Review
            flight={flight}
            passengerData={passengerData}
            selectedSeats={selectedSeats}
            onConfirm={(data) => {
              // Booking confirmed
            }}
            onBack={() => setStep(2)}
            navigate={navigate}
          />
        )}
      </div>
    </AppShell>
  );
}










