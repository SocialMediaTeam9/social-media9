import React, { useEffect, useState, useRef } from 'react';
import { PostResponse } from '../types/types'; // Assuming types are correctly defined here
import { fetcher } from '../utils/fetcher'; // Assuming fetcher utility exists
import PostInteractions, { PostInteractionsRef } from './PostInteractions'; // Import the new PostInteractions component AND its ref type

interface PostCardProps {
  post: PostResponse;
  currentLoggedInUsername: string; // The username of the currently logged-in user
}

const PostCardAlt: React.FC<PostCardProps> = ({ post, currentLoggedInUsername }) => {
  const [isFollowing, setIsFollowing] = useState<boolean>(false);
  // State to handle loading status for follow/unfollow actions
  const [isActionLoading, setIsActionLoading] = useState<boolean>(false);
  // State to handle errors during follow/unfollow actions
  const [actionError, setActionError] = useState<string | null>(null);

  // State for the comment input in PostCardAlt
  const [newCommentContent, setNewCommentContent] = useState('');
  // State for loading/error specific to posting a new comment from this component
  const [isCommentPosting, setIsCommentPosting] = useState(false);
  const [commentPostError, setCommentPostError] = useState<string | null>(null);

  // Ref to directly access methods of the PostInteractions component
  const postInteractionsRef = useRef<PostInteractionsRef>(null);

  // Effect to check the follow status when the component mounts or author changes
  useEffect(() => {
    const checkFollowStatus = async () => {
      // Do not check follow status if the author is the current logged-in user
      if (currentLoggedInUsername === post.authorUsername) {
        setIsFollowing(false); // Can't follow yourself
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

    if (currentLoggedInUsername && post.authorUsername) { // Only run if a user is logged in and post author is available
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
      // Assuming your backend has an unfollow endpoint like DELETE /api/unfollow
      const response = await fetcher('/api/unfollow', {
        method: 'DELETE', // Or POST with a specific unfollow action
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          localUsername: currentLoggedInUsername,
          unfollowedActorUrl: `https://your-domain.com/users/${post.authorUsername}`, // Adjust this URL based on your backend
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

 
  const handlePostComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newCommentContent.trim() === '' || !postInteractionsRef.current) {
      setCommentPostError("Comment cannot be empty.");
      return;
    }

    setIsCommentPosting(true); 
    setCommentPostError(null);

    try {
      // Call the submitComment method on the PostInteractions component via its ref
      const addedComment = await postInteractionsRef.current.submitComment(newCommentContent);
      if (addedComment) {
        setNewCommentContent(''); // Clear the input on successful submission
      } else {
        // If submitComment returns null, it means an error occurred within PostInteractions
        setCommentPostError("Failed to add comment. Please try again.");
      }
    } catch (error: any) {
      // Catch any unexpected errors during the call to submitComment
      console.error("Error posting comment from PostCardAlt:", error);
      setCommentPostError(error.message || "An unexpected error occurred while posting your comment.");
    } finally {
      setIsCommentPosting(false); // Reset loading state
    }
  };

  const showFollowButton = currentLoggedInUsername && currentLoggedInUsername !== post.authorUsername;

  return (
    <div className="post-card">
      <div className="post-avatar">{/* Avatar placeholder */}</div>
      <div className="post-content">
        <div className="user-info">
          <div className="post-header">
            <span className="post-author">{post.authorUsername}</span>
            <span className="post-username">@{post.authorUsername}</span> {/* Display username again if needed */}
            <span className="text-gray-500 mx-2">·</span>
            <span className="text-gray-500">{new Date(post.createdAt).toLocaleDateString()}</span>
          </div>

          {showFollowButton && (
            <div className="follow-button-wrapper mt-2">
              {isFollowing ? (
                <button
                  className="following-button bg-gray-600 hover:bg-gray-700 text-white font-bold py-1 px-3 rounded-full text-sm transition-colors duration-200"
                  onClick={handleUnfollow}
                  disabled={isActionLoading}
                >
                  {isActionLoading ? 'Unfollowing...' : 'Following'}
                </button>
              ) : (
                <button
                  className="follow-button bg-blue-600 hover:bg-blue-700 text-white font-bold py-1 px-3 rounded-full text-sm transition-colors duration-200"
                  onClick={handleFollow}
                  disabled={isActionLoading}
                >
                  {isActionLoading ? 'Following...' : 'Follow'}
                </button>
              )}
              {actionError && <p className="text-red-500 text-sm mt-1">{actionError}</p>}
            </div>
          )}
        </div>
        
        <p className="post-text" dangerouslySetInnerHTML={{ __html: post.content }} />
        {post.attachments?.length > 0 && (
          <div className="post-attachments mt-3">
            <img src={post.attachments[0]} alt="Post attachment" className="post-image w-full h-auto rounded-lg" />
          </div>
        )}

      
        <form onSubmit={handlePostComment} className="mb-6 mt-4 p-4 bg-gray-50 rounded-lg">
            <textarea
                className="w-full p-3 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 resize-y min-h-[60px] text-gray-800"
                placeholder="Add a comment..."
                value={newCommentContent}
                onChange={(e) => setNewCommentContent(e.target.value)}
                disabled={isCommentPosting} 
                aria-label="Add a new comment"
            />
            {commentPostError && <p className="text-red-500 text-sm mt-1 mb-2">{commentPostError}</p>}
            <button
                type="submit"
                className="mt-2 px-4 py-2 bg-blue-600 text-white font-semibold rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
                disabled={isCommentPosting || newCommentContent.trim() === ''} 
            >
                {isCommentPosting ? 'Posting...' : 'Post Comment'}
            </button>
        </form>


        <PostInteractions postId={post.postId} ref={postInteractionsRef} /> 
      </div>
    </div>
  );
};

export default PostCardAlt;
