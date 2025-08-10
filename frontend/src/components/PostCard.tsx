import React, { useState, useEffect } from 'react';
import { fetcher } from '../utils/fetcher';
import LikeButton from './LikeButton';

// Updated interface to include like information
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

interface PostCardProps {
    post: Post;
    currentLoggedInUsername: string;
}

const PostCard: React.FC<PostCardProps> = ({ post, currentLoggedInUsername }) => {
    // Existing follow-related state
    const [isFollowing, setIsFollowing] = useState<boolean>(false);
    const [isActionLoading, setIsActionLoading] = useState<boolean>(false);
    const [actionError, setActionError] = useState<string | null>(null);

    // New like-related state
    const [likeCount, setLikeCount] = useState(post.likeCount);
    const [isLiked, setIsLiked] = useState(post.isLikedByUser);

    // Existing follow logic (unchanged)
    useEffect(() => {
        const checkFollowStatus = async () => {
            if (currentLoggedInUsername === post.authorUsername) {
                return;
            }

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

    // Existing follow handlers (unchanged)
    const handleFollow = async () => {
        setIsActionLoading(true);
        setActionError(null);
        try {
            const response = await fetcher('/api/follow', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    localUsername: currentLoggedInUsername,
                    targetUsername: post.authorUsername,
                }),
            });
            if (response) {
                setIsFollowing(true);
            }
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
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    localUsername: currentLoggedInUsername,
                    unfollowedActorUrl: `https://your-domain.com/users/${post.authorUsername}`,
                }),
            });
            if (response) {
                setIsFollowing(false);
            }
        } catch (err: any) {
            console.error("Error unfollowing user:", err);
            setActionError(err.message || "Failed to unfollow user.");
        } finally {
            setIsActionLoading(false);
        }
    };

    // New like handler
    const handleLikeChange = (newIsLiked: boolean, newLikeCount: number) => {
        setIsLiked(newIsLiked);
        setLikeCount(newLikeCount);
    };

    const showFollowButton = currentLoggedInUsername && currentLoggedInUsername !== post.authorUsername;

    return (
        <div className="post-container">
            {post.boostedBy && (
                <div className="text-xs text-gray-400 mb-2 font-semibold">
                    Boosted by {post.boostedBy}
                </div>
            )}
            <div className="profile-picture-placeholder">
                {post.authorUsername.charAt(0).toUpperCase()}
            </div>
            <div className="post-content-wrapper">
                <div className="user-info">
                    <p className="author-name">{post.authorUsername}</p>
                    <p className="username-tag">@{post.authorUsername}</p>
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
                <p className="post-text">{post.postContent}</p>
                {post.attachmentUrls?.length > 0 && (
                    <div className="post-image-container">
                        <img src={post.attachmentUrls[0]} alt="Post attachment" className="post-image" />
                    </div>
                )}

                {/* New interaction section with like button */}
                <div className="flex items-center justify-between mt-4 max-w-md">
                    {/* <LikeButton
                        postId={post.postId}
                        isLiked={isLiked}
                        likeCount={likeCount}
                        onLikeChange={handleLikeChange}
                    /> */}
                </div>

                <p className="text-xs text-gray-500 mt-2">{new Date(post.createdAt).toLocaleString()}</p>
                {actionError && <p className="text-red-500 text-sm mt-2">{actionError}</p>}
            </div>
        </div>
    );
};

export default PostCard;