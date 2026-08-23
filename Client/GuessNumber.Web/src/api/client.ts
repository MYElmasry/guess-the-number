import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5080',
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
});

export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as { error?: string } | undefined;
    return data?.error ?? error.message;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return 'Something went wrong.';
}

export default api;
