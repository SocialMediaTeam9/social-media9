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
  attachments: string[];
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
  isFollowing: boolean;
  actorUrl: string
}

export interface UpdateProfileResponse {
  success: boolean;
  updatedUser: UserProfile;
}

export interface UserSearchResult {
  userId: string;
  username: string;
  fullName: string;
  profilePictureUrl?: string;
}

export interface PostSearchResult {
  postId: string;
  userId: string;
  username: string;
  content: string;
  hashtags: string[];
  createdAt: string;
}


export interface GenerateUploadUrlPayload {
  fileName: string;
  contentType: string;
}

export interface GenerateUploadUrlResponse {
  uploadUrl: string;
  finalUrl: string;
}

export interface PaginatedPostResponse {
  items: PostResponse[],
  nextCursor?: string
}

export interface PostResponse {
  postId: string;
  authorUsername: string;
  content: string;
  createdAt: string;
  commentCount: number;
  attachments: string[];
  boostedBy?: string;
  likeCount: number;
  isLikedByUser: boolean;
}