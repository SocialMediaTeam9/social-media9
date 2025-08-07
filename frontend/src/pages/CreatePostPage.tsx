import React, { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetcher, getUploadUrl, uploadFileToS3 } from '../utils/fetcher'; 
import PhotoIcon from '../components/PhotoIcon';

interface PostResponse {
  postId: string;
}

const CreatePostPage: React.FC = () => {

  const [content, setContent] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [isUploading, setIsUploading] = useState(false);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [finalAttachmentUrl, setFinalAttachmentUrl] = useState<string | null>(null);
  
  const navigate = useNavigate();
  const fileInputRef = useRef<HTMLInputElement>(null);


  const handleContentChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {

    setContent(event.target.value);
    if (error) setError(null);
  };

  const handleFileSelect = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;
    
    const localPreviewUrl = URL.createObjectURL(file);
    setPreviewUrl(localPreviewUrl);
    setIsUploading(true);
    setError(null);

    try {
      const { uploadUrl, finalUrl } = await getUploadUrl({
        fileName: file.name,
        contentType: file.type,
      });
      await uploadFileToS3(uploadUrl, file);
      setFinalAttachmentUrl(finalUrl);
    } catch (err: any) {
      setError(err.message || "Failed to upload image.");
      setPreviewUrl(null); 
    } finally {
      setIsUploading(false);
      URL.revokeObjectURL(localPreviewUrl);
    }
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!content.trim() && !finalAttachmentUrl) return;
    if (content.length > 280) return;

    setIsSubmitting(true);
    setError(null);

    try {
      await fetcher<PostResponse>('/posts/create', {
        method: 'POST',
        body: {
          Content: content,
          AttachmentUrls: finalAttachmentUrl ? [finalAttachmentUrl] : [],
        },
      });
      navigate('/dashboard'); 
    } catch (err: any) {
      setError(err.message || 'Failed to create post.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const characterCount = content.length;
  const isOverLimit = characterCount > 280;

  return (
    <>
      <div className="page-header">Create Post</div>
      <div className="post-card">
        <div className="post-avatar">{/* User Avatar */}</div>
        <div className="post-content">
          <form onSubmit={handleSubmit} className="create-post-form">
            <div className="textarea-wrapper">
              <textarea
                value={content}
                onChange={handleContentChange} // <-- THIS IS THE FIX
                placeholder="What's on your mind?"
                className="post-textarea"
                rows={5}
                disabled={isSubmitting}
                autoFocus
              />
            </div>

            {previewUrl && (
              <div className="image-preview-container">
                <img src={previewUrl} alt="Upload preview" className="image-preview" />
                {isUploading && <div className="upload-spinner"></div>}
              </div>
            )}
            
            {error && <p className="form-error-message">{error}</p>}

            <div className="form-footer">
              <div className="footer-actions">
                <input 
                  type="file" 
                  ref={fileInputRef} 
                  onChange={handleFileSelect}
                  style={{ display: 'none' }}
                  accept="image/jpeg,image/png,image/gif,image/webp"
                />
                <button 
                  type="button" 
                  // THIS IS THE FIX FOR THE ICON
                  // This onClick handler programmatically clicks the hidden input.
                  onClick={() => fileInputRef.current?.click()} 
                  className="icon-button" 
                  disabled={isUploading || isSubmitting}
                  title="Add image"
                >
                  <PhotoIcon />
                </button>
              </div>

              <div className="footer-submit">
                <span className={`char-counter ${isOverLimit ? 'over-limit' : ''}`}>
                  {characterCount} / 280
                </span>
                <button 
                  type="submit" 
                  className="submit-post-button" 
                  disabled={isSubmitting || isUploading || (!content.trim() && !finalAttachmentUrl) || isOverLimit}
                >
                  {isSubmitting ? 'Posting...' : 'Post'}
                </button>
              </div>
            </div>
          </form>
        </div>
      </div>
    </>
  );
};

export default CreatePostPage;