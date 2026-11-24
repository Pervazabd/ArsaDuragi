const API_BASE_URL = (
  import.meta.env.VITE_API_BASE_URL || "/api"
).replace(/\/+$/, "");

const SESSION_KEY = "arsaDuragi.session";

let refreshPromise = null;

export class ApiError extends Error {
  constructor(message, status, errors = []) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.errors = errors;
  }
}

export function getSession() {
  try {
    const value = localStorage.getItem(SESSION_KEY);
    return value ? JSON.parse(value) : null;
  } catch {
    localStorage.removeItem(SESSION_KEY);
  window.dispatchEvent(new Event("arsaDuragi:sessionChanged"));
    return null;
  }
}

export function saveSession(authData) {
  if (!authData?.accessToken || !authData?.refreshToken) {
    throw new Error("Backend authentication response is incomplete.");
  }

  const session = {
    userId: authData.userId,
    fullName: authData.fullName,
    email: authData.email,
    roles: authData.roles ?? [],
    accessToken: authData.accessToken,
    accessTokenExpiresAt: authData.accessTokenExpiresAt,
    refreshToken: authData.refreshToken,
    refreshTokenExpiresAt: authData.refreshTokenExpiresAt,
  };

  localStorage.setItem(SESSION_KEY, JSON.stringify(session));
  window.dispatchEvent(new Event("arsaDuragi:sessionChanged"));
  return session;
}

export function clearSession() {
  localStorage.removeItem(SESSION_KEY);
  window.dispatchEvent(new Event("arsaDuragi:sessionChanged"));
}

async function readResponse(response) {
  if (response.status === 204) {
    return null;
  }

  const contentType = response.headers.get("content-type") || "";

  if (!contentType.includes("application/json")) {
    return null;
  }

  return response.json();
}

function createApiError(response, result) {
  return new ApiError(
    result?.message || `Request failed (${response.status}).`,
    response.status,
    result?.errors ?? []
  );
}

async function refreshAccessToken() {
  if (refreshPromise) {
    return refreshPromise;
  }

  refreshPromise = (async () => {
    const session = getSession();

    if (!session?.refreshToken) {
      clearSession();
      throw new ApiError("Your session has expired. Please log in again.", 401);
    }

    try {
      const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          refreshToken: session.refreshToken,
        }),
      });

      const result = await readResponse(response);

      if (!response.ok || !result?.success || !result?.data) {
        throw createApiError(response, result);
      }

      const updatedSession = saveSession({
        ...session,
        ...result.data,
      });

      return updatedSession.accessToken;
    } catch (error) {
      clearSession();
      throw error;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

async function sendRequest(path, options, token) {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  const headers = new Headers(options.headers || {});

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  let body = options.body;

  if (
    body !== undefined &&
    body !== null &&
    !(body instanceof FormData) &&
    typeof body !== "string"
  ) {
    headers.set("Content-Type", "application/json");
    body = JSON.stringify(body);
  }

  return fetch(`${API_BASE_URL}${normalizedPath}`, {
    ...options,
    headers,
    body,
  });
}

export async function apiRequest(path, options = {}) {
  const {
    auth = true,
    retry = true,
    ...fetchOptions
  } = options;

  const initialSession = getSession();
  const originalToken = auth ? initialSession?.accessToken : null;

  let response = await sendRequest(path, fetchOptions, originalToken);

  const isRefreshEndpoint = path.includes("/auth/refresh");
  const isLogoutEndpoint = path.includes("/auth/logout");

  if (
    response.status === 401 &&
    auth &&
    retry &&
    originalToken &&
    !isRefreshEndpoint &&
    !isLogoutEndpoint
  ) {
    const currentSession = getSession();
    let newToken;

    if (currentSession?.accessToken !== originalToken) {
      newToken = currentSession?.accessToken;
    } else {
      newToken = await refreshAccessToken();
    }

    if (newToken) {
      response = await sendRequest(path, fetchOptions, newToken);
    }
  }

  const result = await readResponse(response);

  if (!response.ok || result?.success === false) {
    throw createApiError(response, result);
  }

  return result?.data ?? null;
}

export async function login(credentials) {
  const data = await apiRequest("/auth/login", {
    method: "POST",
    auth: false,
    body: credentials,
  });

  return saveSession(data);
}

export async function register(userData) {
  const data = await apiRequest("/auth/register", {
    method: "POST",
    auth: false,
    body: userData,
  });

  return saveSession(data);
}

export function requestRegistrationVerification(email) {
  return apiRequest("/auth/register/request-verification", {
    method: "POST",
    auth: false,
    body: { email },
  });
}

export function requestPasswordReset(email) {
  return apiRequest("/auth/forgot-password", {
    method: "POST",
    auth: false,
    body: { email },
  });
}

export function resetPassword(resetData) {
  return apiRequest("/auth/reset-password", {
    method: "POST",
    auth: false,
    body: resetData,
  });
}

export async function logout() {
  const session = getSession();

  try {
    if (session?.refreshToken) {
      await apiRequest("/auth/logout", {
        method: "POST",
        auth: false,
        retry: false,
        body: {
          refreshToken: session.refreshToken,
        },
      });
    }
  } finally {
    clearSession();
  }
}
