import { FetcherOptions, Post, UserProfile } from "../types/types";

/**
 * A generic fetcher utility function for a social media application.
 * It includes URL mapping and data transformation logic, simulating a real API.
 *
 * @param url - The user-facing endpoint (e.g., '/feed').
 * @param options - The fetcher options, including method, body, etc.
 * @returns A promise that resolves with the transformed data.
 */
export async function fetcher<T>(url: string, options?: FetcherOptions): Promise<T> {
  try {
    // In a real app, you'd load this from process.env
    const baseURL = "https://localhost:7109;http://localhost:5245";

    // This mapping simulates how your front-end URLs might map to different
    // internal API endpoints.
    const endpointMapping: Record<string, string> = {
      '/feed': '/posts',
      '/profile': '/users/profile',
      '/posts/:id': '/posts', // Example for a specific post
      '/google-login': '/google-login', // Added new endpoint for Google login
    };

    // Get the mapped endpoint or use the original URL if no mapping exists
    const mappedEndpoint = endpointMapping[url] || url;
    const fullUrl = `${baseURL}${mappedEndpoint}`;

    console.log(`Fetching from: ${fullUrl}`);

    let requestBody: BodyInit | null = null;
    if (options?.body) {
      if (typeof options.body === 'string' || options.body instanceof FormData || options.body instanceof ArrayBuffer) {
        requestBody = options.body;
      } else {
        requestBody = JSON.stringify(options.body);
      }
    }

    // --- Start of Mock Network Logic ---
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        let mockResponseData: any;
        let responseStatus = 200;

        switch (mappedEndpoint) {
          case '/posts':
            mockResponseData = [
              { id: 1, userId: 101, username: 'JaneDoe', avatarUrl: 'https://placehold.co/40x40/FF5733/FFFFFF?text=JD', content: 'Just finished coding a new feature! So excited!', timestamp: new Date().toISOString(), likes: 25, comments: 5 },
              { id: 2, userId: 102, username: 'JohnSmith', avatarUrl: 'https://placehold.co/40x40/33A1FF/FFFFFF?text=JS', content: 'Loving the new design of this app!', timestamp: new Date(Date.now() - 3600000).toISOString(), likes: 120, comments: 15 },
              { id: 3, userId: 103, username: 'SamathaW', avatarUrl: 'https://placehold.co/40x40/8D33FF/FFFFFF?text=SW', content: 'Hello, world! First post here.', timestamp: new Date(Date.now() - 7200000).toISOString(), likes: 3, comments: 1 },
            ];
            break;
          case '/users/profile':
            mockResponseData = {
              id: 101,
              username: 'JaneDoe',
              bio: 'Full-stack developer and coffee enthusiast.',
              avatarUrl: 'https://placehold.co/40x40/FF5733/FFFFFF?text=JD',
              followers: 500,
              following: 150,
            };
            break;
          case '/google-login':
            // Log a message to confirm this endpoint was called
            console.log('Mocking a successful Google login...');
            
            // Simulate a successful login response with a mock token and user data
            mockResponseData = {
              success: true,
              token: 'mock-jwt-token-12345',
              user: {
                id: 104,
                username: 'GoogleUser',
                email: 'googleuser@example.com',
                avatarUrl: 'https://placehold.co/40x40/007BFF/FFFFFF?text=GU',
              }
            };
            break;
          default:
            // For an unknown or invalid endpoint
            responseStatus = 404;
            mockResponseData = { error: 'Endpoint not found' };
            break;
        }

        if (responseStatus >= 200 && responseStatus < 300) {
          const transformedData = transformData(url, mockResponseData);
          resolve(transformedData as T);
        } else {
          const errorText = JSON.stringify(mockResponseData);
          reject(new Error(`HTTP ${responseStatus}: ${errorText}`));
        }
      }, 1000); // Simulate a 1-second network delay
    });
    // --- End of Mock Network Logic ---

  } catch (error) {
    console.error('Fetcher error:', error);
    if (error instanceof TypeError) {
      throw new Error('Unable to connect to the API. Please ensure the backend server is running.');
    }
    throw error;
  }
}

/**
 * Transforms mock data from the "backend" to match frontend expectations.
 * This is a simplified version; a real app would have more complex logic.
 *
 * @param endpoint - The user-facing endpoint URL.
 * @param data - The raw data from the mock API.
 * @returns The transformed data.
 */
function transformData(endpoint: string, data: any): any {
  // We can add more complex transformation logic here if needed.
  // For this example, we'll assume the mock data is already in the correct format.
  switch (endpoint) {
    case '/feed':
      return data as Post[];
    case '/profile':
      return data as UserProfile;
    default:
      return data;
  }
}
