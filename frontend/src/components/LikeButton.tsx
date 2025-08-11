import React from "react";
import HeartIcon from "./HeartIcon";
import { useLikes } from "../hooks/useLikes";

interface LikeButtonProps {
    postId: string;
    isLiked: boolean;
    likeCount: number;
    onToggle?: () => void; // simplest: parent toggles and manages count/state
    disabled?: boolean;
    className?: string;
}

const LikeButton: React.FC<LikeButtonProps> = ({
    postId,
    isLiked,
    likeCount,
    onToggle,
    disabled = false,
    className = ""
}) => {
    const { handleLikeToggle, likingPosts } = useLikes();
    const isLoading = likingPosts.has(postId);

    const handleClick = () => {
        if (onToggle) {
            onToggle();
            return;
        }
    }

    // const onClick = () => {
    //     handleLikeToggle(postId, isLiked, (newLikedState, countDelta) => {
    //         // Ensure likeCount is always treated as a number
    //         const safeLikeCount = Number(likeCount) || 0;
    //         onLikeChange(newLikedState, safeLikeCount + countDelta);
    //     });
    // };

    return (
        <button
            onClick={handleClick}
            disabled={isLoading}
            className={`flex items-center space-x-2 transition-colors group ${isLiked
                ? "text-red-500 hover:text-red-600"
                : "text-gray-500 hover:text-red-500"
                } ${isLoading ? "opacity-60 cursor-not-allowed" : "cursor-pointer"
                } ${className}`}
            title={isLiked ? "Unlike" : "Like"}
        >
            <div
                className={`p-2 rounded-full transition-colors ${isLiked
                    ? "group-hover:bg-red-50 group-hover:bg-opacity-10"
                    : "group-hover:bg-red-50 group-hover:bg-opacity-10"
                    }`}
            >
                <HeartIcon
                    filled={isLiked}
                    className={`w-5 h-5 transition-transform ${isLoading
                        ? "scale-110"
                        : "group-hover:scale-110"
                        }`}
                />
            </div>
            <span className="text-sm font-medium">
                {Number(likeCount) || 0}
            </span>
        </button>
    );
};

export default LikeButton;