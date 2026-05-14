import axios from 'axios';
import { API_BASE_URL } from '../config';
import { useAuthStore } from '../store/authStore';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

let refreshingPromise = null;

export async function refreshStoredSession() {
  const { refreshToken, setTokens, clearAuth } = useAuthStore.getState();

  if (!refreshToken) {
    throw new Error('No refresh token');
  }

  if (!refreshingPromise) {
    refreshingPromise = axios
      .post(
        `${API_BASE_URL}/auth/refresh`,
        JSON.stringify(refreshToken),
        {
          headers: {
            'Content-Type': 'application/json',
          },
        }
      )
      .then((response) => {
        const data = response.data;
        setTokens({
          accessToken: data.accessToken,
          refreshToken: data.refreshToken,
        });
        return data;
      })
      .catch((error) => {
        clearAuth();
        throw error;
      })
      .finally(() => {
        refreshingPromise = null;
      });
  }

  return refreshingPromise;
}

apiClient.interceptors.request.use((request) => {
  // Convert any date-like values in params or body to UTC ISO strings
  function isDateString(value) {
    if (typeof value !== 'string') return false;
    // YYYY-MM-DD
    if (/^\d{4}-\d{2}-\d{2}$/.test(value)) return true;
    // ISO-like strings or other date parsable strings
    // Date.parse returns NaN for invalid
    return !Number.isNaN(Date.parse(value));
  }

  function convertValue(v) {
    if (v instanceof Date) return v.toISOString();
    if (typeof v === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(v)) {
      // Treat date-only values as UTC midnight
      return `${v}T00:00:00Z`;
    }
    if (typeof v === 'string' && !Number.isNaN(Date.parse(v))) {
      const d = new Date(v);
      return d.toISOString();
    }
    return v;
  }

  function convertDatesRecursively(obj) {
    if (obj == null) return obj;
    if (Array.isArray(obj)) return obj.map(convertDatesRecursively);
    if (obj instanceof Date) return obj.toISOString();
    if (typeof obj === 'object') {
      const res = {};
      for (const [k, val] of Object.entries(obj)) {
        res[k] = convertDatesRecursively(val);
      }
      return res;
    }
    // primitive
    return convertValue(obj);
  }

  if (request.params) {
    try {
      request.params = convertDatesRecursively(request.params);
    } catch (e) {
      // ignore conversion errors
    }
  }
  if (request.data) {
    try {
      request.data = convertDatesRecursively(request.data);
    } catch (e) {
      // ignore conversion errors
    }
  }

  // If caller embedded query params directly into the URL (e.g. "/flights?DepartureDate=2026-05-12"),
  // try to normalize any date-like values to UTC ISO strings as well.
  if (!request.params && request.url && request.url.includes('?')) {
    try {
      // Use base URL to correctly parse relative URLs
      const parsed = new URL(request.url, API_BASE_URL);
      const search = parsed.searchParams;
      let changed = false;
      for (const key of Array.from(search.keys())) {
        const val = search.get(key);
        if (val && typeof val === 'string') {
          if (/^\d{4}-\d{2}-\d{2}$/.test(val)) {
            search.set(key, `${val}T00:00:00Z`);
            changed = true;
          } else if (!Number.isNaN(Date.parse(val))) {
            const d = new Date(val);
            search.set(key, d.toISOString());
            changed = true;
          }
        }
      }
      if (changed) {
        // rebuild request.url preserving pathname and new query
        request.url = parsed.pathname + (parsed.search ? `?${parsed.searchParams.toString()}` : '');
      }
    } catch (e) {
      // ignore URL parsing errors
    }
  }

  const token = useAuthStore.getState().accessToken;

  if (token) {
    request.headers = request.headers || {};
    request.headers.Authorization = `Bearer ${token}`;
  }

  return request;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    const requestUrl = originalRequest?.url || '';

    if (
      error.response?.status !== 401 ||
      originalRequest?._retry ||
      requestUrl.startsWith('/auth/')
    ) {
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    try {
      await refreshStoredSession();
      const token = useAuthStore.getState().accessToken;

      if (token) {
        originalRequest.headers = originalRequest.headers || {};
        originalRequest.headers.Authorization = `Bearer ${token}`;
      }

      return apiClient(originalRequest);
    } catch (refreshError) {
      return Promise.reject(refreshError);
    }
  }
);



