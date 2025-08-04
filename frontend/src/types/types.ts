export type FetcherOptions = Omit<RequestInit, 'body'> & {
  body?: BodyInit | null;
};


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
  userId: number;
  username: string;
  email: string;
  fullName: string;
  bio: string;
  profilePictureUrl: string;
  followersCount: number;
  followingCount: number;
  createdAt: string;
  googleId: string;
}

export interface UpdateProfileResponse {
  success: boolean;
  updatedUser: UserProfile;
}