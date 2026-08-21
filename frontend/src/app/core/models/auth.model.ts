export interface User {
  id: string;
  email: string;
  fullName: string;
  role: 'Admin' | 'Manager' | 'Employee' | string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken?: string;
  user: User;
}
