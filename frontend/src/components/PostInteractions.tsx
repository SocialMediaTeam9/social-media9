import React, { useState, useEffect } from 'react';
import EnhancedLikeButton from './EnhancedLikeButton'; 

interface PostInteractionsProps {
    postId: string;
    isLiked: boolean;
    likeCount: number;
    onLikeChange: (isLiked: boolean, newLikeCount: number) => void;
}

interface Comment {
    commentId: string;
    postId: string;
    username: string;
    content: string;
    createdAt: string; 
}

const PostInteractions: React.FC<PostInteractionsProps> = ({
    postId,
    isLiked,
    likeCount,
    onLikeChange
}) => {
    const [comments, setComments] = useState<Comment[]>([]);
    const [newCommentContent, setNewCommentContent] = useState('');
    const [isLoadingComments, setIsLoadingComments] = useState(false);
    const [errorComments, setErrorComments] = useState<string | null>(null);

   
    const fetchComments = async () => {
        setIsLoadingComments(true);
        setErrorComments(null);
        try {
            
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
    };

    useEffect(() => {
        fetchComments();
    }, [postId]);

  
    const handleAddComment = async (e: React.FormEvent) => {
        e.preventDefault();
        if (newCommentContent.trim() === '') return;

        try {
            const commentPayload = {
                content: newCommentContent 
            };

            const response = await fetch(`/api/posts/${postId}/comments`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(commentPayload)
            });

            if (!response.ok) {
                let errorMessage = `Failed to add comment: ${response.statusText}`;
                try {
                    const errorData = await response.json();
                    if (errorData && errorData.message) {
                        errorMessage = errorData.message;
                    } else if (errorData && typeof errorData === 'string') {
                        errorMessage = errorData;
                    }
                } catch (jsonError) {
                    console.warn("Could not parse error response JSON", jsonError);
                }
                throw new Error(errorMessage);
            }

            const addedComment: Comment = await response.json();
            setComments((prevComments) => [...prevComments, addedComment]);
            setNewCommentContent(''); 
        } catch (error: any) {
            console.error("Error adding comment:", error);
            setErrorComments(error.message);
        }
    };

    return (
        <div className="post-interactions bg-white p-4 rounded-lg shadow-md mt-4">
            <div className="flex items-center mb-4">
                <EnhancedLikeButton
                    postId={postId}
                    isLiked={isLiked}
                    likeCount={likeCount}
                    onLikeChange={onLikeChange}
                />
                <span className="ml-4 text-gray-600">
                    {comments.length} Comments
                </span>
            </div>

            {/* Comment Input Section */}
            <form onSubmit={handleAddComment} className="mb-6">
                <textarea
                    className="w-full p-3 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 resize-y min-h-[60px]"
                    placeholder="Add a comment..."
                    value={newCommentContent}
                    onChange={(e) => setNewCommentContent(e.target.value)}
                    aria-label="Add a new comment"
                />
                <button
                    type="submit"
                    className="mt-2 px-4 py-2 bg-blue-600 text-white font-semibold rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                    Post Comment
                </button>
            </form>

          
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
};

export default PostInteractions;
