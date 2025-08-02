import React from 'react';

// Define the props that the Post component will accept
interface PostProps {
  author: string;
  username: string;
  content: string;
}

// Post component that displays a single post
const Post: React.FC<PostProps> = ({ author, username, content }) => (
  <div className="border-b border-gray-700 p-4 flex gap-4 transition-colors hover:bg-gray-800">
    {/* Placeholder for a user's profile picture */}
    <div className="w-12 h-12 bg-gray-600 rounded-full flex-shrink-0"></div>
    <div className="flex-1">
      <div className="flex items-center space-x-2">
        <span className="font-bold text-white">{author || "Anonymous"}</span>
        <span className="text-gray-500">@{username}</span>
      </div>
      <p className="text-gray-300 mt-1">{content}</p>
    </div>
  </div>
);

export default Post;
