import api from './client';
import type { AuthResponse, LoginRequest, RegisterRequest } from '../types';

export const authApi = {
  login: (data: LoginRequest) =>
    api.post<AuthResponse>('/Auth/login', data).then((r) => r.data),

  register: (data: RegisterRequest) =>
    api.post<AuthResponse>('/Auth/register', data).then((r) => r.data),
};
