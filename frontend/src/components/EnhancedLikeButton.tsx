import React, { useState, useEffect } from 'react';
import { useLikes } from '../hooks/useLikes';

interface HeartIconProps {
    filled: boolean;
    className?: string;
    isAnimating?: boolean;
}

const HeartIcon: React.FC<HeartIconProps> = ({ filled, className = "", isAnimating = false }) => {
    return (
        <svg
            className={`heart-icon ${className} ${isAnimating ? 'filling' : ''}`}
            fill={filled ? "currentColor" : "none"}
            stroke="currentColor"
            viewBox="0 0 24 24"
            xmlns="http://www.w3.org/2000/svg"
            strokeWidth={filled ? 0 : 2}
        >
            <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"
            />
        </svg>
    );
};

interface EnhancedLikeButtonProps {
    postId: string;
    isLiked: boolean;
    likeCount: number;
    onLikeChange: (isLiked: boolean, newLikeCount: number) => void;
    className?: string;
}

const EnhancedLikeButton: React.FC<EnhancedLikeButtonProps> = ({
    postId,
    isLiked,
    likeCount,
    onLikeChange,
    className = ""
}) => {
    const { handleLikeToggle, likingPosts } = useLikes();
    const [isAnimating, setIsAnimating] = useState(false);
    const [displayCount, setDisplayCount] = useState(likeCount);
    const [countAnimating, setCountAnimating] = useState(false);

    const isLoading = likingPosts.has(postId);

    // Update display count when likeCount prop changes
    useEffect(() => {
        if (displayCount !== likeCount) {
            setCountAnimating(true);
            setDisplayCount(likeCount);
            const timer = setTimeout(() => setCountAnimating(false), 300);
            return () => clearTimeout(timer);
        }
    }, [likeCount, displayCount]);

    const onClick = async () => {
        if (isLoading) return;

        // Start heart animation
        setIsAnimating(true);

        // Reset animation after it completes
        setTimeout(() => setIsAnimating(false), 300);

        handleLikeToggle(postId, isLiked, (newLikedState, countDelta) => {
            onLikeChange(newLikedState, displayCount + countDelta);
        });
    };

    return (
        <button
            onClick={onClick}
            disabled={isLoading}
            className={`like-button ${isLiked ? 'liked' : ''} ${isLoading ? 'loading' : ''} ${className}`}
            title={isLiked ? 'Unlike this post' : 'Like this post'}
            aria-label={`${isLiked ? 'Unlike' : 'Like'} this post. ${displayCount} ${displayCount === 1 ? 'like' : 'likes'}`}
        >
            <HeartIcon
                filled={isLiked}
                isAnimating={isAnimating}
            />
            <span className={`like-count ${countAnimating ? 'animate' : ''}`}>
                {displayCount > 0 ? displayCount.toLocaleString() : ''}
            </span>
        </button>
    );
};

export default EnhancedLikeButton;