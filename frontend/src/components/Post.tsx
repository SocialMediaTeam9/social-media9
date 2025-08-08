import React from 'react';
import { PostResponse } from '../types/types';


interface PostData {
  postId: string;
  authorUsername: string;
  postContent: string;
  attachmentUrls: string[];
  createdAt: string;
  boostedBy?: string;
}

interface PostProps {
  post: PostResponse;
}

const Post: React.FC<PostProps> = ({ post }) => (
  <div className="border-b border-gray-700 p-4 flex gap-4 transition-colors hover:bg-gray-800">
    {/* Placeholder for a user's profile picture */}
    <div className="w-12 h-12 bg-gray-600 rounded-full flex-shrink-0"></div>
    <div className="flex-1">
      {/* Optional: Render a "Boosted" message if the data exists */}
      {post.boostedBy && (
        <div className="text-xs text-gray-400 mb-2 font-semibold">
          Boosted by {post.boostedBy}
        </div>
      )}
      <div className="flex items-center space-x-2">
        {/* Use the authorUsername from the post object */}
        <span className="font-bold text-white">{post.authorUsername}</span>
        <span className="text-gray-500">@{post.authorUsername}</span>
      </div>
      {/* Use the postContent from the post object */}
      <p className="text-gray-300 mt-1 whitespace-pre-wrap break-words">{post.content}</p>

      {/* ADDED: Conditionally render images if they exist in the data */}
      {post.attachments?.length > 0 && (
        <div className="post-attachments">
          <img src={post.attachments[0]} alt="Post attachment" className="post-image" />
        </div>
      )}

      {/* ADDED: Display the timestamp from the data */}
      <p className="text-xs text-gray-500 mt-4">{new Date(post.createdAt).toLocaleString()}</p>
    </div>
  </div>
);

export default Post;