import React from 'react';
import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { SearchPage } from '../pages/SearchPage';
import { apiClient } from '../api/client';

// Mock components and modules
jest.mock('../api/client');
jest.mock('../components/layout/AppShell', () => ({
  AppShell: ({ children, title, subtitle }) => (
    <div data-testid="app-shell">
      <h1>{title}</h1>
      <p>{subtitle}</p>
      {children}
    </div>
  ),
}));

jest.mock('../components/ui/AirportAutosuggest', () => ({
  AirportAutosuggest: ({ value, onChange, placeholder }) => (
    <input
      type="text"
      placeholder={placeholder}
      value={value}
      onChange={(e) => onChange(e.target.value)}
      data-testid={`airport-${placeholder}`}
    />
  ),
}));

jest.mock('../components/ui/Button', () => ({
  Button: ({ children, onClick, disabled, className, type, variant }) => (
    <button
      type={type}
      onClick={onClick}
      disabled={disabled}
      className={className}
      data-testid={`button-${children}`}
    >
      {children}
    </button>
  ),
}));

jest.mock('../store/authStore', () => ({
  useAuthStore: (selector) => {
    const mockState = {
      accessToken: 'test-token',
      refreshToken: 'test-refresh',
      user: { id: 'user-1', email: 'test@example.com' },
      isAuthenticated: () => true,
    };
    return selector ? selector(mockState) : mockState;
  },
}));

const renderWithRouter = (component) => {
  return render(<BrowserRouter>{component}</BrowserRouter>);
};

describe('SearchPage', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should render search panel with form fields', () => {
    renderWithRouter(<SearchPage />);

    expect(screen.getByTestId('app-shell')).toBeInTheDocument();
    expect(screen.getByTestId('airport-From')).toBeInTheDocument();
    expect(screen.getByTestId('airport-To')).toBeInTheDocument();
    expect(screen.getByTestId('button-Search flights')).toBeInTheDocument();
  });

  it('should show error when required fields are missing', async () => {
    const user = userEvent.setup();
    renderWithRouter(<SearchPage />);

    const submitButton = screen.getByTestId('button-Search flights');
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/Please choose both cities/i)).toBeInTheDocument();
    });
  });

  it('should update origin city when user types', async () => {
    const user = userEvent.setup();
    renderWithRouter(<SearchPage />);

    const originInput = screen.getByTestId('airport-From');
    await user.type(originInput, 'New York');

    expect(originInput.value).toBe('New York');
  });

  it('should update destination city when user types', async () => {
    const user = userEvent.setup();
    renderWithRouter(<SearchPage />);

    const destinationInput = screen.getByTestId('airport-To');
    await user.type(destinationInput, 'London');

    expect(destinationInput.value).toBe('London');
  });

  it('should have passenger controls', () => {
    renderWithRouter(<SearchPage />);

    const passengerButton = screen.getByRole('button', {
      name: /1 passenger/i,
    });
    
    expect(passengerButton).toBeInTheDocument();
  });

  it('should display initial hint message', () => {
    renderWithRouter(<SearchPage />);

    expect(
      screen.getByText(/Choose your route and filters, then search for flights/i)
    ).toBeInTheDocument();
  });

  it('should show sort filters', () => {
    renderWithRouter(<SearchPage />);

    const sortBySelect = screen.getByDisplayValue('Price');
    expect(sortBySelect).toBeInTheDocument();

    const directionSelect = screen.getByDisplayValue('Ascending');
    expect(directionSelect).toBeInTheDocument();
  });

  it('should handle swap cities button', async () => {
    const user = userEvent.setup();
    renderWithRouter(<SearchPage />);

    const originInput = screen.getByTestId('airport-From');
    const destinationInput = screen.getByTestId('airport-To');

    await user.type(originInput, 'New York');
    await user.type(destinationInput, 'London');

    const swapButton = screen.getByRole('button', { name: /Swap cities/i });
    await user.click(swapButton);

    await waitFor(() => {
      expect(originInput.value).toBe('London');
      expect(destinationInput.value).toBe('New York');
    });
  });
});


