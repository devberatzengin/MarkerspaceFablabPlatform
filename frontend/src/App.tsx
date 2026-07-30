import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import Layout from './components/Layout';
import Login from './pages/Auth/Login';
import Register from './pages/Auth/Register';
import Dashboard from './pages/Dashboard/Dashboard';
import AnnouncementList from './pages/Announcements/AnnouncementList';
import EquipmentList from './pages/Equipment/EquipmentList';
import MyEquipment from './pages/Equipment/MyEquipment';
import CategoryList from './pages/Categories/CategoryList';
import UserList from './pages/Users/UserList';
import Profile from './pages/Profile/Profile';

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />

          <Route
            element={
              <ProtectedRoute>
                <Layout />
              </ProtectedRoute>
            }
          >
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/announcements" element={<AnnouncementList />} />
            <Route path="/equipment" element={<EquipmentList />} />
            <Route path="/my-equipment" element={<MyEquipment />} />
            <Route
              path="/categories"
              element={
                <ProtectedRoute adminOnly>
                  <CategoryList />
                </ProtectedRoute>
              }
            />
            <Route
              path="/users"
              element={
                <ProtectedRoute adminOnly>
                  <UserList />
                </ProtectedRoute>
              }
            />
            <Route path="/profile" element={<Profile />} />
          </Route>

          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
