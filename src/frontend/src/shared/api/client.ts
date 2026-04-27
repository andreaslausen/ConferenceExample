import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "./openapi.d";

const TOKEN_KEY = "auth_token";

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token);
}

export function clearToken(): void {
  localStorage.removeItem(TOKEN_KEY);
}

const authMiddleware: Middleware = {
  async onRequest({ request }) {
    const token = getToken();
    if (token) {
      request.headers.set("Authorization", `Bearer ${token}`);
    }
    return request;
  },
};

const errorMiddleware: Middleware = {
  async onResponse({ response }) {
    if (response.status === 401) {
      clearToken();
      window.location.href = "/login";
    }
    return response;
  },
};

const apiClient = createClient<paths>({ baseUrl: "/" });
apiClient.use(authMiddleware);
apiClient.use(errorMiddleware);

export default apiClient;
