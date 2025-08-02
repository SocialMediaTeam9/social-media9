// src/utils/types.ts

/**
 * Defines the options for the fetcher utility function.
 * This is a simplified version of the standard RequestInit interface.
 */
export interface FetcherOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
  // Allows for a simple object or other BodyInit types.
  body?: object | string | FormData | ArrayBuffer;
  headers?: Record<string, string>;
}

/**
 * Defines the structure for a social media post.
 */
export interface Post {
  id: number;
  userId: number;
  username: string;
  avatarUrl: string;
  content: string;
  timestamp: string;
  likes: number;
  comments: number;
}

/**
 * Defines the structure for a user's profile.
 */
export interface UserProfile {
  id: number;
  username: string;
  bio: string;
  avatarUrl: string;
  followers: number;
  following: number;
}
