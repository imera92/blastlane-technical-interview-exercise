export interface User {
  id: string;
  displayName: string;
  email: string;
}

export interface AuthenticationResponse {
  user: User;
}

export interface LoginRequest {
  email: string;
  password: string;
}
