import { Navigate } from "react-router-dom";
import { useAuth, type UserRole } from "../auth/AuthContext";
import type { ReactNode } from "react";

interface Props {
  role: UserRole;
  children: ReactNode;
}

export function RoleGuard({ role, children }: Props) {
  const { user } = useAuth();

  if (!user || user.role !== role) {
    return <Navigate to="/forbidden" replace />;
  }

  return <>{children}</>;
}
