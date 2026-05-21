import {
  writeAuthStorage,
  readAuthStorage,
  clearAuthStorage,
} from '../utils/storage';

describe('Storage Utils', () => {
  const AUTH_STORAGE_KEY = 'air-vibe-auth';

  beforeEach(() => {
    localStorage.clear();
  });

  describe('writeAuthStorage', () => {
    it('should write tokens to localStorage', () => {
      const tokens = {
        accessToken: 'access-123',
        refreshToken: 'refresh-456',
      };

      writeAuthStorage(tokens);

      const stored = JSON.parse(localStorage.getItem(AUTH_STORAGE_KEY));
      expect(stored).toEqual(tokens);
    });

    it('should overwrite existing tokens', () => {
      writeAuthStorage({
        accessToken: 'old-token',
        refreshToken: 'old-refresh',
      });

      writeAuthStorage({
        accessToken: 'new-token',
        refreshToken: 'new-refresh',
      });

      const stored = JSON.parse(localStorage.getItem(AUTH_STORAGE_KEY));
      expect(stored.accessToken).toBe('new-token');
      expect(stored.refreshToken).toBe('new-refresh');
    });
  });

  describe('readAuthStorage', () => {
    it('should read tokens from localStorage', () => {
      const tokens = {
        accessToken: 'access-123',
        refreshToken: 'refresh-456',
      };

      localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(tokens));

      const result = readAuthStorage();
      expect(result).toEqual(tokens);
    });

    it('should return default empty tokens if nothing in storage', () => {
      const result = readAuthStorage();

      expect(result).toEqual({
        accessToken: null,
        refreshToken: null,
      });
    });

    it('should handle corrupted data', () => {
      localStorage.setItem(AUTH_STORAGE_KEY, 'not-valid-json');

      const result = readAuthStorage();
      expect(result).toEqual({
        accessToken: null,
        refreshToken: null,
      });
    });
  });

  describe('clearAuthStorage', () => {
    it('should remove auth tokens from localStorage', () => {
      const tokens = {
        accessToken: 'access-123',
        refreshToken: 'refresh-456',
      };

      localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(tokens));
      expect(localStorage.getItem(AUTH_STORAGE_KEY)).toBeTruthy();

      clearAuthStorage();
      expect(localStorage.getItem(AUTH_STORAGE_KEY)).toBeNull();
    });

    it('should handle clearing when storage is already empty', () => {
      expect(() => {
        clearAuthStorage();
      }).not.toThrow();
    });
  });

  describe('Storage persistence', () => {
    it('should persist tokens across read/write cycles', () => {
      const tokens = {
        accessToken: 'access-token-long',
        refreshToken: 'refresh-token-long',
      };

      writeAuthStorage(tokens);
      const firstRead = readAuthStorage();
      expect(firstRead).toEqual(tokens);

      const secondRead = readAuthStorage();
      expect(secondRead).toEqual(tokens);
    });
  });
});

