import { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
import { usersApi } from '../api/users';
import type { UserResponse } from '../types';

interface AuthContextType {
  token: string | null;
  user: UserResponse | null;
  isAdmin: boolean;
  isAuthenticated: boolean;
  loading: boolean;
  login: (token: string) => Promise<void>;
  logout: () => void;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
  const [user, setUser] = useState<UserResponse | null>(null);
  const [loading, setLoading] = useState(!!localStorage.getItem('token'));

  const fetchUser = async () => {
    try {
      const userData = await usersApi.getMe();
      setUser(userData);
    } catch {
      setToken(null);
      setUser(null);
      localStorage.removeItem('token');
    }
  };

  useEffect(() => {
    if (token) {
      fetchUser().finally(() => setLoading(false));
    } else {
      setLoading(false);
    }
  }, [token]);

  const login = async (newToken: string) => {
    localStorage.setItem('token', newToken);
    setToken(newToken);
  };

  const logout = () => {
    localStorage.removeItem('token');
    setToken(null);
    setUser(null);
  };

  const refreshUser = async () => {
    await fetchUser();
  };

  return (
    <AuthContext.Provider
      value={{
        token,
        user,
        isAdmin: user?.type === 'Admin',
        isAuthenticated: !!token && !!user,
        loading,
        login,
        logout,
        refreshUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within AuthProvider');
  return context;
}
