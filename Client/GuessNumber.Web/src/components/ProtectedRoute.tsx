import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export function ProtectedRoute() {
  const { user, loading } = useAuth();

  if (loading) {
    return <div className="page-center">Loading...</div>;
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}

export function PublicRoute() {
  const { user, loading } = useAuth();

  if (loading) {
    return <div className="page-center">Loading...</div>;
  }

  if (user) {
    return <Navigate to="/game" replace />;
  }

  return <Outlet />;
}
