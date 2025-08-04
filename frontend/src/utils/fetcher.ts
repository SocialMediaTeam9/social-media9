
const baseURL = process.env.REACT_APP_API_URL || "http://localhost:5245";

// Define a type for the Fetcher options and user data
// In a real application, these would be in a separate types file.
type FetcherOptions = {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';
  headers?: HeadersInit;
  body?: any;
  mode?: RequestMode;
  credentials?: RequestCredentials;
  cache?: RequestCache;
  redirect?: RequestRedirect;
  referrerPolicy?: ReferrerPolicy;
  signal?: AbortSignal;
};

type User = {
  id: string;
  username: string;
  fullName: string;
  bio?: string;
  profilePictureUrl?: string;
};

const endpointMapping: Record<string, string> = {
  '/login/google': '/api/users/google-login',
  '/login/google-redirect': '/api/users/google-login-redirect',
  '/user/profile': '/api/users',
  '/user/update': '/api/users',
  '/user/follow': '/api/users',
  '/user/unfollow': '/api/users',
  '/user/followers': '/api/users',
  '/user/following': '/api/users',
};

export async function fetcher<T>(
  url: string,
  options?: FetcherOptions & { userId?: string }
): Promise<T> {
  try {
    const userId = options?.userId;
    let mappedEndpoint = endpointMapping[url] || url;

    if (userId && mappedEndpoint.includes('/api/users')) {
      if (url === '/user/follow') mappedEndpoint += `/${userId}/follow`;
      else if (url === '/user/unfollow') mappedEndpoint += `/${userId}/unfollow`;
      else if (url === '/user/followers') mappedEndpoint += `/${userId}/followers`;
      else if (url === '/user/following') mappedEndpoint += `/${userId}/following`;
      else if (url === '/user/profile' || url === '/user/update') mappedEndpoint += `/${userId}`;
    }

    const fullUrl = `${baseURL}${mappedEndpoint}`;
    console.log(`Fetcher is calling: ${fullUrl}`);

    const token = localStorage.getItem('token');
    const headers = new Headers(options?.headers);
    
    if (token) {
      headers.set('Authorization', `Bearer ${token}`);
    }
    
    let requestBody: BodyInit | null = null;
    if (options?.body != null) {
      if (
        typeof options.body === 'string' ||
        options.body instanceof FormData ||
        options.body instanceof ArrayBuffer
      ) {
        requestBody = options.body;
      } else {
        requestBody = JSON.stringify(options.body);
        headers.set('Content-Type', 'application/json');
      }
    }

    const response = await fetch(fullUrl, {
      method: options?.method || 'GET',
      headers: headers,
      body: requestBody,
      mode: options?.mode || 'cors',
      credentials: options?.credentials || 'omit',
      cache: options?.cache,
      redirect: options?.redirect,
      referrerPolicy: options?.referrerPolicy,
      signal: options?.signal,
    });

    if (!response.ok) {
      if (response.status === 401 || response.status === 403) {
        console.error('Authentication error: Token invalid or expired. Logging out.');
        localStorage.clear();
      }
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText || response.statusText}`);
    }

    const data = await response.json();
    return transformData(url, data) as T;
  } catch (error) {
    console.error('Fetcher error:', error);
    if (error instanceof TypeError && error.message.includes('fetch')) {
      throw new Error('Unable to connect to the API. Please ensure the backend server is running.');
    }
    throw error;
  }
}

function transformData(endpoint: string, data: any): any {
  switch (endpoint) {
    case '/user/profile':
      return {
        id: data.userId,
        username: data.username,
        fullName: data.fullName,
        bio: data.bio,
        profilePictureUrl: data.profilePictureUrl,
        createdAt: formatDate(data.createdAt),
        updatedAt: formatDate(data.updatedAt),
      };

    case '/user/update':
      return {
        success: true,
        updatedUser: {
          id: data.userId,
          fullName: data.fullName,
          bio: data.bio,
          profilePictureUrl: data.profilePictureUrl,
        },
      };

    case '/user/follow':
    case '/user/unfollow':
      return {
        message: data.message || "Action completed.",
      };

    case '/user/followers':
    case '/user/following':
      return (data || []).map((user: any) => ({
        userId: user.userId,
        username: user.username,
        fullName: user.fullName,
        profilePictureUrl: user.profilePictureUrl,
      }));

    case '/login/google':
    case '/login/google-redirect':
      return {
        userId: data.userId,
        username: data.username,
        token: data.token,
      };

    default:
      return data;
  }
}

function formatDate(dateString: string | number | Date): string {
  if (!dateString) return '';
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return '';
  return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
}
