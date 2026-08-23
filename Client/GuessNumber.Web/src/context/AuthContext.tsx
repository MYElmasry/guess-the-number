import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import * as authApi from '../api/auth';
import { getErrorMessage } from '../api/client';
import type { User } from '../types/api';

interface AuthContextValue {
  user: User | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, confirmPassword: string) => Promise<void>;
  logout: () => Promise<void>;
  refreshUser: (clearOnFailure?: boolean) => Promise<void>;
  updateBestScore: (bestScore: number) => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  const refreshUser = useCallback(async (clearOnFailure = false) => {
    try {
      const currentUser = await authApi.getCurrentUser();
      setUser(currentUser);
    } catch {
      if (clearOnFailure) {
        setUser(null);
      }
    }
  }, []);

  const updateBestScore = useCallback((bestScore: number) => {
    setUser((prev) => (prev ? { ...prev, bestScore } : prev));
  }, []);

  useEffect(() => {
    void (async () => {
      await refreshUser(true);
      setLoading(false);
    })();
  }, [refreshUser]);

  const login = useCallback(async (email: string, password: string) => {
    const loggedInUser = await authApi.login(email, password);
    setUser(loggedInUser);
  }, []);

  const register = useCallback(async (email: string, password: string, confirmPassword: string) => {
    await authApi.register(email, password, confirmPassword);
  }, []);

  const logout = useCallback(async () => {
    await authApi.logout();
    setUser(null);
  }, []);

  const value = useMemo(
    () => ({ user, loading, login, register, logout, refreshUser, updateBestScore }),
    [user, loading, login, register, logout, refreshUser, updateBestScore],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}

export function getAuthErrorMessage(error: unknown): string {
  return getErrorMessage(error);
}
