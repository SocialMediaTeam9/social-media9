import React from 'react';
import HeartIcon from './HeartIcon';
import { useLikes } from '../hooks/useLikes';

interface LikeButtonProps {
    postId: string;
    isLiked: boolean;
    likeCount: number;
    onLikeChange: (isLiked: boolean, newLikeCount: number) => void;
    className?: string;
}

const LikeButton: React.FC<LikeButtonProps> = ({
    postId,
    isLiked,
    likeCount,
    onLikeChange,
    className = ""
}) => {
    const { handleLikeToggle, likingPosts } = useLikes();
    const isLoading = likingPosts.has(postId);

    const onClick = () => {
        handleLikeToggle(postId, isLiked, (newLikedState, countDelta) => {
            onLikeChange(newLikedState, likeCount + countDelta);
        });
    };

    return (
        <button
            onClick={onClick}
            disabled={isLoading}
            className={`flex items-center space-x-2 transition-colors group ${isLiked
                ? 'text-red-500 hover:text-red-600'
                : 'text-gray-500 hover:text-red-500'
                } ${isLoading ? 'opacity-60 cursor-not-allowed' : 'cursor-pointer'} ${className}`}
            title={isLiked ? 'Unlike' : 'Like'}
        >
            <div className={`p-2 rounded-full transition-colors ${isLiked
                ? 'group-hover:bg-red-50 group-hover:bg-opacity-10'
                : 'group-hover:bg-red-50 group-hover:bg-opacity-10'
                }`}>
                <HeartIcon
                    filled={isLiked}
                    className={`w-5 h-5 transition-transform ${isLoading ? 'scale-110' : 'group-hover:scale-110'
                        }`}
                />
            </div>
            <span className="text-sm font-medium">{likeCount}</span>
        </button>
    );
};

export default LikeButton;