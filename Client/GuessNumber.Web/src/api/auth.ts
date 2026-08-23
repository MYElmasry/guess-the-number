import api from './client';
import type { User } from '../types/api';

export async function register(email: string, password: string, confirmPassword: string): Promise<User> {
  const response = await api.post<User>('/api/auth/register', {
    email,
    password,
    confirmPassword,
  });
  return response.data;
}

export async function login(email: string, password: string): Promise<User> {
  const response = await api.post<User>('/api/auth/login', { email, password });
  return response.data;
}

export async function logout(): Promise<void> {
  await api.post('/api/auth/logout');
}

export async function getCurrentUser(): Promise<User> {
  const response = await api.get<User>('/api/auth/me');
  return response.data;
}
