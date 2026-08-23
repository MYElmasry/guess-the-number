import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5080',
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
    'ngrok-skip-browser-warning': 'true',
  },
});

export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as
      | { error?: string; Error?: string; message?: string }
      | string
      | undefined;

    if (typeof data === 'string' && data.trim()) {
      return data;
    }

    if (data && typeof data === 'object') {
      const message = data.error ?? data.Error ?? data.message;
      if (message) {
        return message;
      }
    }

    if (error.response?.status === 401) {
      return 'Your session expired. Please sign in again.';
    }

    if (error.response?.status === 409) {
      return 'An account with this email already exists.';
    }

    if (!error.response) {
      return 'Unable to reach the server. Please try again.';
    }

    return 'Something went wrong.';
  }

  if (error instanceof Error) {
    return error.message;
  }

  return 'Something went wrong.';
}

export default api;
