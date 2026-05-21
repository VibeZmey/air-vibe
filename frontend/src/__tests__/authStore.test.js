import { useAuthStore } from '../store/authStore';

// Mock storage functions
jest.mock('../utils/storage', () => ({
  writeAuthStorage: jest.fn(),
  readAuthStorage: jest.fn(() => ({
    accessToken: null,
    refreshToken: null,
  })),
  clearAuthStorage: jest.fn(),
}));

describe('useAuthStore', () => {
  beforeEach(() => {
    useAuthStore.getState().clearAuth();
  });

  describe('initial state', () => {
    it('should have initial anonymous state', () => {
      const state = useAuthStore.getState();

      expect(state.accessToken).toBeNull();
      expect(state.refreshToken).toBeNull();
      expect(state.user).toBeNull();
      expect(state.status).toBe('anonymous');
      expect(state.error).toBeNull();
    });
  });

  describe('setTokens', () => {
    it('should set tokens and update status', () => {
      const tokens = {
        accessToken: 'access-token-123',
        refreshToken: 'refresh-token-456',
      };

      useAuthStore.getState().setTokens(tokens);

      const state = useAuthStore.getState();
      expect(state.accessToken).toBe(tokens.accessToken);
      expect(state.refreshToken).toBe(tokens.refreshToken);
      expect(state.status).toBe('authenticated');
    });

    it('should clear error when setting tokens', () => {
      useAuthStore.getState().setError('Previous error');
      useAuthStore.getState().setTokens({
        accessToken: 'token',
        refreshToken: 'refresh',
      });

      expect(useAuthStore.getState().error).toBeNull();
    });
  });

  describe('setUser', () => {
    it('should set user data', () => {
      const user = {
        id: 'user-123',
        email: 'test@example.com',
        login: 'test-user',
        role: 'admin',
      };

      useAuthStore.getState().setUser(user);

      expect(useAuthStore.getState().user).toEqual(user);
    });
  });

  describe('setStatus', () => {
    it('should set status', () => {
      useAuthStore.getState().setStatus('restoring');
      expect(useAuthStore.getState().status).toBe('restoring');

      useAuthStore.getState().setStatus('authenticated');
      expect(useAuthStore.getState().status).toBe('authenticated');
    });
  });

  describe('setError', () => {
    it('should set error message', () => {
      const errorMsg = 'Authentication failed';
      useAuthStore.getState().setError(errorMsg);

      expect(useAuthStore.getState().error).toBe(errorMsg);
    });
  });

  describe('clearAuth', () => {
    it('should clear all auth data', () => {
      // Set up some data first
      useAuthStore.getState().setTokens({
        accessToken: 'token',
        refreshToken: 'refresh',
      });
      useAuthStore.getState().setUser({ id: 'user-123' });
      useAuthStore.getState().setError('Some error');

      // Clear auth
      useAuthStore.getState().clearAuth();

      const state = useAuthStore.getState();
      expect(state.accessToken).toBeNull();
      expect(state.refreshToken).toBeNull();
      expect(state.user).toBeNull();
      expect(state.status).toBe('anonymous');
      expect(state.error).toBeNull();
    });
  });

  describe('isAuthenticated', () => {
    it('should return false when not authenticated', () => {
      expect(useAuthStore.getState().isAuthenticated()).toBe(false);
    });

    it('should return true when authenticated', () => {
      useAuthStore.getState().setTokens({
        accessToken: 'token',
        refreshToken: 'refresh',
      });

      expect(useAuthStore.getState().isAuthenticated()).toBe(true);
    });

    it('should return false after clearing auth', () => {
      useAuthStore.getState().setTokens({
        accessToken: 'token',
        refreshToken: 'refresh',
      });

      expect(useAuthStore.getState().isAuthenticated()).toBe(true);

      useAuthStore.getState().clearAuth();

      expect(useAuthStore.getState().isAuthenticated()).toBe(false);
    });
  });
});

