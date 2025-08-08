import React from 'react';
import EnhancedLikeButton from './EnhancedLikeButton';

interface PostInteractionsProps {
    postId: string;
    isLiked: boolean;
    likeCount: number;
    onLikeChange: (isLiked: boolean, newLikeCount: number) => void;
    // You can add more interaction props here (retweet, reply, share, etc.)
}

const PostInteractions: React.FC<PostInteractionsProps> = ({
    postId,
    isLiked,
    likeCount,
    onLikeChange
}) => {
    return (
        <div className="post-interactions">
            <EnhancedLikeButton
                postId={postId}
                isLiked={isLiked}
                likeCount={likeCount}
                onLikeChange={onLikeChange}
            />
        </div>
    );
};

export default PostInteractions;