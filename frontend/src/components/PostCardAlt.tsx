import React, { useEffect, useState } from 'react';
import { PostResponse } from '../types/types';
import { fetcher } from '../utils/fetcher';
import LikeButton from './LikeButton';

interface PostCardProps {
  post: PostResponse;
  currentLoggedInUsername: string;
}

const PostCardAlt: React.FC<PostCardProps> = ({ post, currentLoggedInUsername }) => {
  const [isFollowing, setIsFollowing] = useState<boolean>(false);
  const [isActionLoading, setIsActionLoading] = useState<boolean>(false);
  const [actionError, setActionError] = useState<string | null>(null);

  // Ensure defaults to 0 if undefined
  const [likeCount, setLikeCount] = useState<number>(post.likeCount ?? 0);
  const [isLiked, setIsLiked] = useState(post.isLikedByUser);

  useEffect(() => {
    const checkFollowStatus = async () => {
      if (currentLoggedInUsername === post.authorUsername) return;

      setIsActionLoading(true);
      setActionError(null);
      try {
        const response = await fetcher<{ isFollowing: boolean }>(
          `/api/follow/is-following?localUsername=${encodeURIComponent(currentLoggedInUsername)}&targetUsername=${encodeURIComponent(post.authorUsername)}`
        );
        setIsFollowing(response.isFollowing);
      } catch (err: any) {
        console.error("Error checking follow status:", err);
        setActionError("Failed to check follow status.");
      } finally {
        setIsActionLoading(false);
      }
    };

    if (currentLoggedInUsername) {
      checkFollowStatus();
    }
  }, [post.authorUsername, currentLoggedInUsername]);

  const handleFollow = async () => {
    setIsActionLoading(true);
    setActionError(null);
    try {
      const response = await fetcher('/api/follow', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          localUsername: currentLoggedInUsername,
          targetUsername: post.authorUsername,
        }),
      });
      if (response) setIsFollowing(true);
    } catch (err: any) {
      console.error("Error following user:", err);
      setActionError(err.message || "Failed to follow user.");
    } finally {
      setIsActionLoading(false);
    }
  };

  const handleUnfollow = async () => {
    setIsActionLoading(true);
    setActionError(null);
    try {
      const response = await fetcher('/api/unfollow', {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          localUsername: currentLoggedInUsername,
          unfollowedActorUrl: `https://your-domain.com/users/${post.authorUsername}`,
        }),
      });
      if (response) setIsFollowing(false);
    } catch (err: any) {
      console.error("Error unfollowing user:", err);
      setActionError(err.message || "Failed to unfollow user.");
    } finally {
      setIsActionLoading(false);
    }
  };

  const handleLikeChange = (newIsLiked: boolean, countDelta: number) => {
    setIsLiked(newIsLiked);
    setLikeCount(prev => prev + countDelta); // use updater to avoid stale value
  };

  const showFollowButton = currentLoggedInUsername && currentLoggedInUsername !== post.authorUsername;

  return (
    <div className="post-card">
      <div className="post-avatar">{/* Avatar placeholder */}</div>
      <div className="post-content">
        <div className="user-info">
          {showFollowButton && (
            <div className="follow-button-wrapper">
              {isFollowing ? (
                <button
                  className="following-button"
                  onClick={handleUnfollow}
                  disabled={isActionLoading}
                >
                  {isActionLoading ? 'Unfollowing...' : 'Following'}
                </button>
              ) : (
                <button
                  className="follow-button"
                  onClick={handleFollow}
                  disabled={isActionLoading}
                >
                  {isActionLoading ? 'Following...' : 'Follow'}
                </button>
              )}
            </div>
          )}
        </div>
        <div className="post-header">
          <span className="post-username">@{post.authorUsername}</span>
          <span className="text-gray-500 mx-2">·</span>
          <span className="text-gray-500">{new Date(post.createdAt).toLocaleDateString()}</span>
        </div>
        <p className="post-text" dangerouslySetInnerHTML={{ __html: post.content }} />
        {post.attachments?.length > 0 && (
          <div className="post-attachments">
            <img src={post.attachments[0]} alt="Post attachment" className="post-image" />
          </div>
        )}
        <LikeButton
          postId={post.postId}
          isLiked={isLiked}
          likeCount={likeCount}
          onLikeChange={handleLikeChange}
        />
      </div>
    </div>
  );
};

export default PostCardAlt;