import { useState, useCallback } from 'react';
import { likePost, unlikePost } from '../utils/likeService';

export const useLikes = () => {
    const [likingPosts, setLikingPosts] = useState<Set<string>>(new Set());

    const handleLikeToggle = useCallback(async (
        postId: string,
        isCurrentlyLiked: boolean,
        onSuccess: (newLikedState: boolean, newLikeCount: number) => void
    ) => {
        if (likingPosts.has(postId)) return;

        setLikingPosts(prev => new Set(prev).add(postId));

        try {
            if (isCurrentlyLiked) {
                await unlikePost(postId);
                onSuccess(false, -1); // Decrease like count by 1
            } else {
                await likePost(postId);
                onSuccess(true, 1); // Increase like count by 1
            }
        } catch (error) {
            console.error('Error toggling like:', error);
            // You might want to show an error message to the user here
        } finally {
            setLikingPosts(prev => {
                const next = new Set(prev);
                next.delete(postId);
                return next;
            });
        }
    }, [likingPosts]);

    return {
        handleLikeToggle,
        likingPosts
    };
};