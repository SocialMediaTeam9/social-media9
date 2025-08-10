import { useState } from "react";

export const useLikes = () => {
    const [likingPosts, setLikingPosts] = useState<Set<string>>(new Set());

    const handleLikeToggle = async (
        postId: string,
        isLiked: boolean,
        onChange: (newLikedState: boolean, countDelta: number) => void
    ) => {
        setLikingPosts(prev => new Set(prev).add(postId));

        try {
            if (isLiked) {
                await fetch(`/api/posts/${postId}/like`, { method: "DELETE" });
                onChange(false, -1);
            } else {
                await fetch(`/api/posts/${postId}/like`, { method: "POST" });
                onChange(true, +1);
            }
        } catch (error) {
            console.error("Failed to toggle like", error);
        } finally {
            setLikingPosts(prev => {
                const next = new Set(prev);
                next.delete(postId);
                return next;
            });
        }
    };

    return { handleLikeToggle, likingPosts };
};