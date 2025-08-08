import React, { useState, useEffect, useCallback, useImperativeHandle, forwardRef } from 'react';
import EnhancedLikeButton from './EnhancedLikeButton'; // Assuming this component exists

interface Comment {
    commentId: string;
    postId: string;
    username: string;
    content: string;
    createdAt: string; 
}

interface LikeEntity {
    username: string;
    createdAt: string;
}

// Define the shape of the imperative handle, allowing parent to call these functions
export interface PostInteractionsRef {
    submitComment: (content: string) => Promise<Comment | null>;
    refreshComments: () => Promise<void>; 
}

interface PostInteractionsProps {
    postId: string;
}

// Use forwardRef to allow PostCardAlt to get a ref to this component's internal methods
const PostInteractions = forwardRef<PostInteractionsRef, PostInteractionsProps>(
    ({ postId }, ref) => {
        // State for comments section (internal to PostInteractions)
        const [comments, setComments] = useState<Comment[]>([]);
        const [isLoadingComments, setIsLoadingComments] = useState(false);
        const [errorComments, setErrorComments] = useState<string | null>(null);

        // State for likes section (internal to PostInteractions)
        const [currentLikeCount, setCurrentLikeCount] = useState(0);
        const [isPostLiked, setIsPostLiked] = useState(false);
        const [isLoadingLikes, setIsLoadingLikes] = useState(false);
        const [errorLikes, setErrorLikes] = useState<string | null>(null);

        // Placeholder for current logged-in username for liking/commenting logic.
        // IMPORTANT: In a real app, replace "testuser" with the actual logged-in username
        // obtained from your authentication context or global state.
        const currentLoggedInUsername = "testuser"; 

        // Function to fetch comments for the post
        const fetchComments = useCallback(async () => {
            setIsLoadingComments(true);
            setErrorComments(null);
            try {
                // Calls backend: GET /api/posts/{postId}/comments
                const response = await fetch(`/api/posts/${postId}/comments`);
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                const data: Comment[] = await response.json();
                setComments(data);
            } catch (error: any) {
                console.error("Error fetching comments:", error);
                setErrorComments(error.message);
            } finally {
                setIsLoadingComments(false);
            }
        }, [postId]);

        // Function to fetch likes for the post and determine if current user liked it
        const fetchLikes = useCallback(async () => {
            setIsLoadingLikes(true);
            setErrorLikes(null);
            try {
                // Calls backend: GET /api/posts/{postId}/likes
                const response = await fetch(`/api/posts/${postId}/likes`);
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                const data: LikeEntity[] = await response.json();
                setCurrentLikeCount(data.length);
                // Check if the current logged-in user's username is in the list of likers
                setIsPostLiked(data.some(like => like.username === currentLoggedInUsername));
            } catch (error: any) {
                console.error("Error fetching likes:", error);
                setErrorLikes(error.message);
            } finally {
                setIsLoadingLikes(false);
            }
        }, [postId, currentLoggedInUsername]);

        // Effect to fetch initial comments and likes when component mounts or postId changes
        useEffect(() => {
            fetchComments();
            fetchLikes();
        }, [fetchComments, fetchLikes]);

        // Handler for when the like status changes (called by EnhancedLikeButton)
        const handleLikeChange = useCallback(async (newIsLiked: boolean) => {
            // Prevent action if already loading or no user is logged in
            if (isLoadingLikes || !currentLoggedInUsername) return;

            setIsLoadingLikes(true);
            setErrorLikes(null);
            try {
                // Endpoint depends on whether liking or unliking. Adjust as per your backend.
                // Assuming POST to /like for liking, DELETE to /unlike for unliking
                const endpoint = newIsLiked ? `/api/posts/${postId}/like` : `/api/posts/${postId}/unlike`;
                const method = newIsLiked ? 'POST' : 'DELETE'; 

                const response = await fetch(endpoint, {
                    method: method,
                    headers: { 'Content-Type': 'application/json' },
                    // Authorization header should be handled by your global fetch setup or context
                });

                if (!response.ok) {
                    let errorMessage = `Failed to ${newIsLiked ? 'like' : 'unlike'} post: ${response.statusText}`;
                    try { 
                        const errorData = await response.json(); 
                        if (errorData && errorData.message) { errorMessage = errorData.message; } 
                    } catch (jsonError) { 
                        console.warn("Could not parse error response JSON", jsonError); 
                    }
                    throw new Error(errorMessage);
                }

                // Update local state based on successful API call
                setIsPostLiked(newIsLiked);
                setCurrentLikeCount(prevCount => newIsLiked ? prevCount + 1 : prevCount - 1);

            } catch (error: any) {
                console.error(`Error during ${newIsLiked ? 'like' : 'unlike'} operation:`, error);
                setErrorLikes(error.message);
            } finally {
                setIsLoadingLikes(false);
            }
        }, [postId, currentLoggedInUsername, isLoadingLikes]);

  
        useImperativeHandle(ref, () => ({
            submitComment: async (content: string) => {
                if (content.trim() === '') {
                    setErrorComments("Comment content cannot be empty.");
                    return null;
                }
                setIsLoadingComments(true);
                setErrorComments(null); 

                try {
                    const commentPayload = { content: content };
                  
                    const response = await fetch(`/api/posts/${postId}/comments`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(commentPayload)
                    });

                    if (!response.ok) {
                        let errorMessage = `Failed to add comment: ${response.statusText}`;
                        try { 
                            const errorData = await response.json(); 
                            if (errorData && errorData.message) { errorMessage = errorData.message; } 
                        } catch (jsonError) { 
                            console.warn("Could not parse error response JSON", jsonError); 
                        }
                        throw new Error(errorMessage);
                    }

                    const addedComment: Comment = await response.json();
                    setComments((prevComments) => [...prevComments, addedComment]);
                    return addedComment;
                } catch (error: any) {
                    console.error("Error adding comment:", error);
                    setErrorComments(error.message);
                    return null;
                } finally {
                    setIsLoadingComments(false);
                }
            },
        
            refreshComments: async () => {
                await fetchComments();
            }
        }));

        return (
            <div className="post-interactions bg-white p-4 rounded-lg shadow-md mt-4">
                {/* Likes section */}
                <div className="flex items-center mb-4">
                    {isLoadingLikes ? (
                        <span className="text-gray-500">Loading likes...</span>
                    ) : (
                        <EnhancedLikeButton
                            postId={postId}
                            isLiked={isPostLiked}
                            likeCount={currentLikeCount}
                            onLikeChange={handleLikeChange} // Pass the internal handler
                        />
                    )}
                    {errorLikes && <p className="text-red-500 text-sm ml-2">Error: {errorLikes}</p>}
                    <span className="ml-4 text-gray-600">
                        {comments.length} Comments {/* Display comment count */}
                    </span>
                </div>

                {/* Comments Display Section (no input/button here anymore) */}
                <div className="comments-section">
                    <h3 className="text-lg font-semibold mb-3">Comments</h3>
                    {isLoadingComments && <p className="text-gray-500">Loading comments...</p>}
                    {errorComments && <p className="text-red-500">Error: {errorComments}</p>}
                    {!isLoadingComments && comments.length === 0 && (
                        <p className="text-gray-500">No comments yet. Be the first to comment!</p>
                    )}
                    <div className="space-y-4">
                        {comments
                            .sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()) // Sort by oldest first
                            .map((comment) => (
                                <div key={comment.commentId} className="bg-gray-50 p-3 rounded-md border border-gray-200">
                                    <div className="flex items-center text-sm mb-1">
                                        <span className="font-semibold text-gray-800">{comment.username}</span>
                                        <span className="text-gray-500 ml-2">
                                            {new Date(comment.createdAt).toLocaleString()}
                                        </span>
                                    </div>
                                    <p className="text-gray-700">{comment.content}</p>
                                </div>
                            ))}
                    </div>
                </div>
            </div>
        );
    }
);

export default PostInteractions;
