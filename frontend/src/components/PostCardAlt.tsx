import React, { useState, useEffect } from 'react';
import { PostResponse } from '../types/types';
import LikeButton from './LikeButton';

interface PostCardProps {
  post: PostResponse;
}

interface Post {
  postId: string;
  authorUsername: string;
  postContent: string;
  attachmentUrls: string[];
  createdAt: string;
  boostedBy?: string;
  likeCount: number;
  isLikedByUser: boolean;
}

const PostCardAlt: React.FC<PostCardProps> = ({ post }) => {
  const [likeCount, setLikeCount] = useState(post.likeCount);
  const [isLiked, setIsLiked] = useState(post.isLikedByUser);

  // New like handler
  const handleLikeChange = (newIsLiked: boolean, newLikeCount: number) => {
    setIsLiked(newIsLiked);
    setLikeCount(newLikeCount);
  };

  // useEffect(() => {
  //     const checkFollowStatus = async () => {
  //         if (currentLoggedInUsername === post.authorUsername) {
  //             return;
  //         }

  //         setIsActionLoading(true);
  //         setActionError(null);
  //         try {
  //             const response = await fetcher<{ isFollowing: boolean }>(
  //                 `/api/follow/is-following?localUsername=${encodeURIComponent(currentLoggedInUsername)}&targetUsername=${encodeURIComponent(post.authorUsername)}`
  //             );
  //             setIsFollowing(response.isFollowing);
  //         } catch (err: any) {
  //             console.error("Error checking follow status:", err);
  //             setActionError("Failed to check follow status.");
  //         } finally {
  //             setIsActionLoading(false);
  //         }
  //     };

  //     if (currentLoggedInUsername) {
  //         checkFollowStatus();
  //     }
  // }, [post.authorUsername, currentLoggedInUsername]);

  return (
    <div className="post-card">
      <div className="post-avatar">{/* Avatar placeholder */}</div>
      <div className="post-content">
        <div className="post-header">
          <span className="post-author">{post.authorUsername}</span>
          <span className="post-username">@{post.authorUsername}</span>
          <span className="text-gray-500 mx-2">·</span>
          <span className="text-gray-500">{new Date(post.createdAt).toLocaleDateString()}</span>
          <div className="flex items-center justify-between mt-4 max-w-md">
            <LikeButton
              postId={post.postId}
              isLiked={isLiked}
              likeCount={likeCount}
              onLikeChange={handleLikeChange}
            />
          </div>
        </div>
        <p className="post-text" dangerouslySetInnerHTML={{ __html: post.content }} />
        {post.attachments?.length > 0 && (
          <div className="post-attachments">
            <img src={post.attachments[0]} alt="Post attachment" className="post-image" />
          </div>
        )}
      </div>
    </div>
  )
};

export default PostCardAlt;