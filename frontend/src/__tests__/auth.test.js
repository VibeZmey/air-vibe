import { loginUser, registerUser, confirmEmail, getMe, buildLoginFromEmail } from '../api/auth';
import { apiClient } from '../api/client';
import { useAuthStore } from '../store/authStore';

jest.mock('../api/client', () => ({
  apiClient: {
    post: jest.fn(),
    get: jest.fn(),
  },
  refreshStoredSession: jest.fn(),
}));

describe('Auth API', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    useAuthStore.getState().clearAuth();
  });

  describe('buildLoginFromEmail', () => {
    it('should generate login from email', () => {
      const email = 'john.doe@example.com';
      const login = buildLoginFromEmail(email);

      expect(login).toMatch(/^john-doe-/);
      expect(login.length).toBeLessThanOrEqual(32);
    });

    it('should handle special characters', () => {
      const email = 'john+doe@example.com';
      const login = buildLoginFromEmail(email);

      expect(login).toMatch(/^[a-z0-9-]+$/);
    });

    it('should use default login for empty email', () => {
      const email = '@example.com';
      const login = buildLoginFromEmail(email);

      expect(login).toMatch(/^user-/);
    });
  });

  describe('registerUser', () => {
    it('should send correct payload to backend', async () => {
      const payload = {
        email: 'test@example.com',
        password: 'TestPassword123',
      };

      apiClient.post.mockResolvedValue({
        data: { success: true },
      });

      const result = await registerUser(payload);

      expect(apiClient.post).toHaveBeenCalledWith('/auth/register', {
        login: expect.any(String),
        email: payload.email,
        password: payload.password,
      });
      expect(result.data.success).toBe(true);
    });

    it('should handle registration errors', async () => {
      const payload = {
        email: 'test@example.com',
        password: 'TestPassword123',
      };

      apiClient.post.mockRejectedValue(new Error('Registration failed'));

      await expect(registerUser(payload)).rejects.toThrow('Registration failed');
    });
  });

  describe('loginUser', () => {
    it('should send email and password', async () => {
      const payload = {
        email: 'test@example.com',
        password: 'TestPassword123',
      };

      apiClient.post.mockResolvedValue({
        data: {
          accessToken: 'token123',
          refreshToken: 'refresh123',
        },
      });

      const result = await loginUser(payload);

      expect(apiClient.post).toHaveBeenCalledWith('/auth/login', payload);
      expect(result.data.accessToken).toBe('token123');
    });

    it('should handle login errors', async () => {
      apiClient.post.mockRejectedValue(new Error('Invalid credentials'));

      await expect(
        loginUser({ email: 'test@example.com', password: 'wrong' })
      ).rejects.toThrow('Invalid credentials');
    });
  });

  describe('confirmEmail', () => {
    it('should confirm email with token and set tokens', async () => {
      const token = 'test-token-123';
      const jwtToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjoiQWRtaW4iLCJlbWFpbCI6InRlc3RAZXhhbXBsZS5jb20iLCJ1c2VySWQiOiIxMjM0NTY3OCJ9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ';

      apiClient.post.mockResolvedValue({
        data: {
          accessToken: jwtToken,
          refreshToken: 'refresh-token',
        },
      });

      await confirmEmail(token);

      expect(apiClient.post).toHaveBeenCalledWith(
        `/auth/email-confirm?token=${encodeURIComponent(token)}`
      );

      const state = useAuthStore.getState();
      expect(state.accessToken).toBe(jwtToken);
      expect(state.refreshToken).toBe('refresh-token');
      // User will be set from the decoded JWT token
      expect(state.user).toBeDefined();
      expect(state.user.role).toBe('Admin');
    });

    it('should handle confirmation errors', async () => {
      apiClient.post.mockRejectedValue(new Error('Invalid token'));

      await expect(confirmEmail('bad-token')).rejects.toThrow('Invalid token');
    });
  });

  describe('getMe', () => {
    it('should fetch user data and set it in store', async () => {
      const userData = {
        id: 'user-id',
        email: 'test@example.com',
        login: 'test-user',
      };

      apiClient.get.mockResolvedValue({
        data: userData,
      });

      // Set a mock token with role in store first
      useAuthStore.getState().setTokens({
        accessToken: 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJyb2xlIjoiQWRtaW4iLCJlbWFpbCI6InRlc3RAZXhhbXBsZS5jb20iLCJ1c2VySWQiOiIxMjM0NTY3OCJ9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ',
        refreshToken: 'refresh-token',
      });

      const result = await getMe();

      expect(apiClient.get).toHaveBeenCalledWith('/users/me');
      expect(result.email).toBe(userData.email);

      const state = useAuthStore.getState();
      expect(state.user).toBeDefined();
      expect(state.user.role).toBe('Admin');
    });

    it('should handle errors when fetching user data', async () => {
      apiClient.get.mockRejectedValue(new Error('Fetch failed'));

      await expect(getMe()).rejects.toThrow('Fetch failed');
    });
  });
});




