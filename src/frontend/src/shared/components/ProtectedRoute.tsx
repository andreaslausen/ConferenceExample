import { useEffect } from "react";
import { useAuth } from "../auth/AuthContext";
import type { ReactNode } from "react";

interface Props {
  children: ReactNode;
}

export function ProtectedRoute({ children }: Props) {
  const { user, isLoading, login } = useAuth();

  useEffect(() => {
    if (!isLoading && !user) {
      login();
    }
  }, [isLoading, user, login]);

  if (isLoading || !user) {
    return null;
  }

  return <>{children}</>;
}
