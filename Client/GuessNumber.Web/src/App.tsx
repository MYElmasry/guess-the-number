import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { ProtectedRoute, PublicRoute } from './components/ProtectedRoute';
import { AuthProvider } from './context/AuthContext';
import { GamePage } from './pages/GamePage';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route element={<PublicRoute />}>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
          </Route>
          <Route element={<ProtectedRoute />}>
            <Route path="/game" element={<GamePage />} />
          </Route>
          <Route path="/" element={<Navigate to="/game" replace />} />
          <Route path="*" element={<Navigate to="/game" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
