import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetcher } from '../utils/fetcher';

// Define the shape of the data your API will return for a created post.
// This should match the PostResponse DTO in your C# code.
interface PostResponse {
  postId: string;
  authorUsername: string;
  content: string;
  createdAt: string;
  commentCount: number;
}

const CreatePostPage: React.FC = () => {
  const [content, setContent] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!content.trim() || content.length > 280) {
      setError("Post content must be between 1 and 280 characters.");
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      // --- THIS IS THE KEY CHANGE ---
      // Use your generic fetcher to make the API call.
      await fetcher<PostResponse>('/posts/create', {
        method: 'POST',
        body: {
          // The body must match the CreatePostRequest DTO in C#
          content: content,
        },
      });
      // --- END OF CHANGE ---
      
      console.log('Post created successfully!');
      // On success, navigate the user back to their home timeline/dashboard
      navigate('/dashboard'); 
    } catch (err: any) {
      setError(err.message || 'Failed to create post. Please try again.');
      console.error(err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const characterCount = content.length;
  const isOverLimit = characterCount > 280;

  return (
    <div className="page-content" style={{ padding: '1.5rem' }}>
      <form onSubmit={handleSubmit} className="create-post-form">
        <textarea
          value={content}
          onChange={(e) => setContent(e.target.value)}
          placeholder="What's on your mind?"
          className="post-textarea"
          rows={5}
          disabled={isSubmitting}
        />
        <div className="form-footer">
          <span className={`char-counter ${isOverLimit ? 'over-limit' : ''}`}>
            {characterCount} / 280
          </span>
          <button 
            type="submit" 
            className="submit-post-button" 
            disabled={isSubmitting || !content.trim() || isOverLimit}
          >
            {isSubmitting ? 'Posting...' : 'Post'}
          </button>
        </div>
        {error && <p className="post-error">{error}</p>}
      </form>
    </div>
  );
};

export default CreatePostPage;