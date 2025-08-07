import React from 'react';

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
}

const PostCard: React.FC<PostCardProps> = ({ post }) => {
    return (
        <div className="bg-gray-800 p-4 rounded-lg border border-gray-700 w-full">
            {post.boostedBy && (
                <div className="text-xs text-gray-400 mb-2 font-semibold">
                    Boosted by {post.boostedBy}
                </div>
            )}
            <div className="flex items-center mb-3">
                <div className="w-12 h-12 rounded-full bg-gray-600 mr-4 flex-shrink-0" />
                <div>
                    <p className="font-bold text-white">{post.authorUsername}</p>
                    <p className="text-sm text-gray-400">@{post.authorUsername}</p>
                </div>
            </div>
            <p className="text-gray-200 whitespace-pre-wrap break-words">{post.postContent}</p>
            {post.attachmentUrls?.length > 0 && (
                <div className="mt-4 rounded-lg overflow-hidden">
                    <img src={post.attachmentUrls[0]} alt="Post attachment" className="w-full h-auto object-cover" />
                </div>
            )}
            <p className="text-xs text-gray-500 mt-4">{new Date(post.createdAt).toLocaleString()}</p>
        </div>
    );
};

export default PostCard;