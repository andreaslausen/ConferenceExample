import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "./openapi.d";
import keycloak from "../auth/keycloak";

const authMiddleware: Middleware = {
  async onRequest({ request }) {
    if (keycloak.token) {
      request.headers.set("Authorization", `Bearer ${keycloak.token}`);
    }
    return request;
  },
};

const errorMiddleware: Middleware = {
  async onResponse({ response }) {
    if (response.status === 401) {
      keycloak.login();
    }
    return response;
  },
};

const apiClient = createClient<paths>({ baseUrl: "/" });
apiClient.use(authMiddleware);
apiClient.use(errorMiddleware);

export default apiClient;
