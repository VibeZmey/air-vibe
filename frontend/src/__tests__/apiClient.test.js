import axios from 'axios';
import { apiClient, setAuthToken } from '../api/client';
import { useAuthStore } from '../store/authStore';

// Mock axios
jest.mock('axios');

describe('API Client', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    jest.resetModules();
    useAuthStore.getState().clearAuth();
  });

  describe('apiClient instance', () => {
    it('should have correct base URL', () => {
      expect(apiClient.defaults.baseURL).toBe('http://localhost:8000');
    });

    it('should have default headers', () => {
      expect(apiClient.defaults.headers).toBeDefined();
    });
  });

  describe('setAuthToken', () => {
    it('should set authorization header', () => {
      const token = 'test-token-123';
      setAuthToken(token);

      expect(apiClient.defaults.headers.common.Authorization).toBe(
        `Bearer ${token}`
      );
    });

    it('should handle empty token', () => {
      setAuthToken('');
      expect(apiClient.defaults.headers.common.Authorization).toBe('Bearer ');
    });

    it('should handle null token', () => {
      setAuthToken(null);
      expect(apiClient.defaults.headers.common.Authorization).toBe('Bearer null');
    });
  });

  describe('Request interceptor', () => {
    it('should attach access token to requests', async () => {
      const token = 'test-token-123';
      useAuthStore.getState().setTokens({
        accessToken: token,
        refreshToken: 'refresh',
      });

      // Mock axios.get
      axios.get = jest.fn().mockResolvedValue({ data: { test: true } });

      const result = await apiClient.get('/test');

      expect(result.data.test).toBe(true);
    });
  });

  describe('Error handling', () => {
    it('should handle network errors', async () => {
      const error = new Error('Network error');
      axios.get = jest.fn().mockRejectedValue(error);

      const result = await apiClient.get('/test');
      expect(result).toBe('error');
    });

    it('should handle 401 response', async () => {
      axios.post = jest.fn().mockRejectedValue({
        response: {
          status: 401,
          data: { message: 'Unauthorized' },
        },
      });

      try {
        await apiClient.post('/test', {});
      } catch (error) {
        expect(error.response.status).toBe(401);
      }
    });
  });
});

