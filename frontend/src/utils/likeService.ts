import { fetcher } from './fetcher';

export interface LikeResponse {
    likeId: string;
    postId: string;
    userId: string;
    createdAt: string;
}

export interface PostLikesResponse {
    postId: string;
    likeCount: number;
    isLikedByUser: boolean;
    likes: LikeResponse[];
}

export const likePost = async (postId: string): Promise<LikeResponse> => {
    return await fetcher<LikeResponse>('/api/likes', {
        method: 'POST',
        body: { postId }
    });
};

export const unlikePost = async (postId: string): Promise<void> => {
    await fetcher('/api/likes', {
        method: 'DELETE',
        body: { postId }
    });
};

export const getPostLikes = async (postId: string): Promise<PostLikesResponse> => {
    return await fetcher<PostLikesResponse>(`/api/likes/${postId}`);
};

export const getPostsLikedStatus = async (postIds: string[]): Promise<Record<string, boolean>> => {
    return await fetcher<Record<string, boolean>>('/api/likes/batch-status', {
        method: 'POST',
        body: postIds
    });
};