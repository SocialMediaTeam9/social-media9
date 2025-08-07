import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { fetcher } from "../utils/fetcher";

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
      await fetcher<PostResponse>('/posts/create', {
        method: 'POST',
        body: { Content: content },
      });
      console.log('Post created successfully!');
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
    <>
      {/* 1. Add a consistent page header */}
      <div className="page-header">
        Create Post
      </div>

      {/* 2. Use the 'post-card' structure for the form for a familiar look */}
      <div className="post-card">
        <div className="post-avatar">
          {/* You can add the logged-in user's avatar here later */}
        </div>
        <div className="post-content">
          <form onSubmit={handleSubmit} className="create-post-form">
            <textarea
              value={content}
              onChange={(e) => setContent(e.target.value)}
              placeholder="What's on your mind?"
              className="post-textarea" 
              rows={5}
              disabled={isSubmitting}
              autoFocus
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
      </div>
    </>
  );
};

export default CreatePostPage;