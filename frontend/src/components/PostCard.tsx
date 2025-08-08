import React, { useState, useEffect } from 'react';
import { fetcher } from '../utils/fetcher'; // Assuming you have a fetcher utility

// This interface must match the 'TimelineItemResponse' from your back-end
interface Post {
    postId: string;
    authorUsername: string;
    postContent: string;
    attachmentUrls: string[];
    createdAt: string;
    boostedBy?: string;
}

interface PostCardProps {
    post: Post;
    currentLoggedInUsername: string; // Add this prop to know who is logged in
}

const PostCard: React.FC<PostCardProps> = ({ post, currentLoggedInUsername }) => {
    // State to manage whether the logged-in user is following the post author
    const [isFollowing, setIsFollowing] = useState<boolean>(false);
    // State to handle loading status for follow/unfollow actions
    const [isActionLoading, setIsActionLoading] = useState<boolean>(false);
    // State to handle errors during follow/unfollow actions
    const [actionError, setActionError] = useState<string | null>(null);

    // Effect to check the follow status when the component mounts or author changes
    useEffect(() => {
        const checkFollowStatus = async () => {
            // Do not check follow status if the author is the current logged-in user
            if (currentLoggedInUsername === post.authorUsername) {
                return;
            }

            setIsActionLoading(true);
            setActionError(null);
            try {
                // Assuming your backend has an endpoint to check follow status
                // For example: GET /api/follow/is-following?localUsername={loggedInUser}&targetUsername={author}
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

        if (currentLoggedInUsername) { // Only run if a user is logged in
            checkFollowStatus();
        }
    }, [post.authorUsername, currentLoggedInUsername]); // Re-run if author or logged-in user changes

    // Handler for the follow action
    const handleFollow = async () => {
        setIsActionLoading(true);
        setActionError(null);
        try {
            // Assuming your backend has a follow endpoint like POST /api/follow
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
            if (response) { // Assuming fetcher throws on non-OK status
                setIsFollowing(true);
            }
        } catch (err: any) {
            console.error("Error following user:", err);
            setActionError(err.message || "Failed to follow user.");
        } finally {
            setIsActionLoading(false);
        }
    };

    // Handler for the unfollow action
    const handleUnfollow = async () => {
        setIsActionLoading(true);
        setActionError(null);
        try {
            // Assuming your backend has an unfollow endpoint like DELETE /api/follow
            const response = await fetcher('/api/unfollow', {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    localUsername: currentLoggedInUsername,
                    unfollowedActorUrl: `https://your-domain.com/users/${post.authorUsername}`, // Construct actor URL based on your backend's expectation
                }),
            });
            if (response) { // Assuming fetcher throws on non-OK status
                setIsFollowing(false);
            }
        } catch (err: any) {
            console.error("Error unfollowing user:", err);
            setActionError(err.message || "Failed to unfollow user.");
        } finally {
            setIsActionLoading(false);
        }
    };

    // Do not render the follow/unfollow button if the author is the current logged-in user
    const showFollowButton = currentLoggedInUsername && currentLoggedInUsername !== post.authorUsername;

    return (
        <div className="post-container"> {/* Changed to use .post-container from app.css */}
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
                <p className="text-xs text-gray-500 mt-4">{new Date(post.createdAt).toLocaleString()}</p>
                {actionError && <p className="text-red-500 text-sm mt-2">{actionError}</p>}
            </div>
        </div>
    );
};

export default PostCard;
