import Keycloak from "keycloak-js";

// Local dev default matches the Keycloak service in docker-compose.yml — override by
// editing this constant for other environments (same approach as the backend proxy URL
// in vite.config.ts).
const KEYCLOAK_URL = "http://localhost:8080";

const keycloak = new Keycloak({
  url: KEYCLOAK_URL,
  realm: "conference-example",
  clientId: "conference-example-frontend",
});

export default keycloak;
