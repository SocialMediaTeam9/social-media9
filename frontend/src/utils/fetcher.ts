import { GenerateUploadUrlPayload, GenerateUploadUrlResponse } from "../types/types";

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

// Updated endpoint mapping to include direct paths for follow/unfollow
const endpointMapping: Record<string, string> = {
    '/login/google': '/api/users/google-login',
    '/login/google-redirect': '/api/users/google-login-redirect',
    '/user/profile': '/api/users',
    '/user/update': '/api/users',
    // Existing follow/unfollow mappings (can be kept if still used elsewhere, but not for PostCard)
    '/user/follow': '/api/users',
    '/user/unfollow': '/api/users',
    '/user/followers': '/api/users',
    '/user/following': '/api/users',
    // New direct mappings for follow/unfollow and is-following status check
    '/api/follow/is-following': '/api/follow/is-following',
    '/api/follow': '/api/follow',
    '/api/unfollow': '/api/unfollow',
    '/search/users': '/api/search/users',
    '/search/posts': '/api/search/posts',
    '/search/hashtags': '/api/search/hashtags',
    '/posts/create': '/api/posts',
    '/media/upload-url': '/api/media/generate-upload-url',
    // Assuming timeline endpoint is also directly mapped
    '/api/timeline/home': '/api/timeline/home',
};

export const getUploadUrl = async (payload: GenerateUploadUrlPayload): Promise<GenerateUploadUrlResponse> => {
    return fetcher<GenerateUploadUrlResponse>('/media/upload-url', {
        method: 'POST',
        body: payload,
    });
};

export const uploadFileToS3 = async (uploadUrl: string, file: File) => {
    const response = await fetch(uploadUrl, {
        method: 'PUT',
        body: file,
        headers: {
            'Content-Type': file.type,
        },
    });

    if (!response.ok) {
        throw new Error('Failed to upload file to S3.');
    }
};

export async function fetcher<T>(
    url: string,
    options?: FetcherOptions & { userId?: string } // userId is now less critical for follow/unfollow
): Promise<T> {
    try {
        const userId = options?.userId;
        const [baseUrlPart, queryString] = url.split('?');
        
        // Use the direct mapping if available, otherwise fallback to original URL part
        let mappedEndpoint = endpointMapping[baseUrlPart] || baseUrlPart;

        // Conditional path construction for user-related endpoints that use userId in path
        // Ensure this logic doesn't interfere with the new direct follow/unfollow endpoints
        if (userId && mappedEndpoint.includes('/api/users')) {
            // These are for the original /user/ endpoints that use userId in path
            if (baseUrlPart === '/user/follow') mappedEndpoint += `/${userId}/follow`;
            else if (baseUrlPart === '/user/unfollow') mappedEndpoint += `/${userId}/unfollow`;
            else if (baseUrlPart === '/user/followers') mappedEndpoint += `/${userId}/followers`;
            else if (baseUrlPart === '/user/following') mappedEndpoint += `/${userId}/following`;
            else if (baseUrlPart === '/user/profile' || baseUrlPart === '/user/update') mappedEndpoint += `/${userId}`;
        }

        const fullUrl = `${baseURL}${mappedEndpoint}${queryString ? `?${queryString}` : ''}`;
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
                // Set Content-Type for JSON bodies, ensure it's not overwritten if already set
                if (!headers.has('Content-Type')) {
                    headers.set('Content-Type', 'application/json');
                }
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

        // Handle cases where response might be empty (e.g., DELETE requests)
        if (response.status === 204 || response.headers.get('Content-Length') === '0') {
            return {} as T; // Return an empty object for no content responses
        }

        const data = await response.json();
        return transformData(url, data) as T; // Pass the original URL for transformation
    } catch (error) {
        console.error('Fetcher error:', error);
        if (error instanceof TypeError && error.message.includes('fetch')) {
            throw new Error('Unable to connect to the API. Please ensure the backend server is running.');
        }
        throw error;
    }
}

function transformData(endpoint: string, data: any): any {
    // Use the original endpoint URL for transformation logic
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

        // Updated cases for the new direct API endpoints
        case '/api/follow': // For POST /api/follow
        case '/api/unfollow': // For DELETE /api/unfollow
            return {
                message: data.message || "Action completed.",
            };
        case '/api/follow/is-following': // For GET /api/follow/is-following
            return {
                isFollowing: data.isFollowing || false, // Assuming backend returns { isFollowing: true/false }
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
        case '/posts/create':
            return {
                id: data.postId,
                author: data.authorUsername,
                text: data.content,
                created: formatDate(data.createdAt),
            };
        case '/api/timeline/home': // Added for timeline response transformation
            return data; // Assuming timeline response is already in the desired format
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
