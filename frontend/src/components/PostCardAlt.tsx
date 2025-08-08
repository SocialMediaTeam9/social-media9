import React from 'react';
import { PostResponse } from '../types/types';

interface PostCardProps {
  post: PostResponse;
}

const PostCardAlt: React.FC<PostCardProps> = ({ post }) => (
  <div className="post-card">
    <div className="post-avatar">{/* Avatar placeholder */}</div>
    <div className="post-content">
      <div className="post-header">
        <span className="post-author">{post.authorUsername}</span>
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
    </div>
  </div>
);

export default PostCardAlt;