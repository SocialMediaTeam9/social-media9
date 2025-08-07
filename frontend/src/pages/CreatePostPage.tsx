import React, { useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetcher, getUploadUrl, uploadFileToS3 } from '../utils/fetcher';


const PhotoIcon = () => <svg /* ... your svg code ... */ />;

const CreatePostPage: React.FC = () => {
  const [content, setContent] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  // --- NEW STATE FOR FILE UPLOADS ---
  const [uploadingFile, setUploadingFile] = useState<File | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [finalAttachmentUrl, setFinalAttachmentUrl] = useState<string | null>(null);

  const [state, setState] = useState({
    content: '',
    isSubmitting: false,
    isUploading: false,
    previewUrl: null as string | null,
    finalAttachmentUrl: null as string | null,
    error: null as string | null,
  });
  
  const navigate = useNavigate();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const validateFile = (file: File): string | null => {
    const MAX_FILE_SIZE_MB = 10;
    const ALLOWED_MIME_TYPES = ["image/jpeg", "image/png", "image/gif", "image/webp"];

    if (file.size > MAX_FILE_SIZE_MB * 1024 * 1024) {
      return `File is too large. Maximum size is ${MAX_FILE_SIZE_MB}MB.`;
    }
    if (!ALLOWED_MIME_TYPES.includes(file.type)) {
      return "Invalid file type. Please select a JPG, PNG, GIF, or WEBP image.";
    }
    return null;
  };

  const handleFileSelect = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const validationError = validateFile(file);
    if (validationError) {
      setState(s => ({ ...s, error: validationError }));
      return;
    }
    
    // Reset previous uploads
    setFinalAttachmentUrl(null);
    setError(null);

    // Create a temporary local URL for the preview
    const localPreviewUrl = URL.createObjectURL(file);

    setState(s => ({ 
      ...s, 
      isUploading: true, 
      previewUrl: localPreviewUrl, 
      finalAttachmentUrl: null, // Reset previous successful upload
      error: null 
    }));
    // setPreviewUrl(localPreviewUrl);
    // setUploadingFile(file);
    // setIsUploading(true);

    try {
      const { uploadUrl, finalUrl } = await getUploadUrl({
        fileName: file.name,
        contentType: file.type,
      });

      await uploadFileToS3(uploadUrl, file);
      
      setState(s => ({ ...s, finalAttachmentUrl: finalUrl, isUploading: false }));
      
    } catch (err: any) {
      setState(s => ({
        ...s,
        error: err.message || "Failed to upload image. Please try again.",
        previewUrl: null, // Clear the preview on failure
        isUploading: false
      }));
    } finally {
      URL.revokeObjectURL(localPreviewUrl);
    }
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!state.content.trim() && !state.finalAttachmentUrl) {
      setState(s => ({ ...s, error: "Post must have text or an image." }));
      return;
    }

    const isOverLimit = state.content.length > 280;

    if (isOverLimit) {
      setState(s => ({ ...s, error: "Post content cannot exceed 280 characters." }));
      return;
    }

    setState(s => ({ ...s, isSubmitting: true, error: null }));

    try {
      await fetcher('/posts/create', {
        method: 'POST',
        body: {
          content: state.content,
          attachmentUrls: state.finalAttachmentUrl ? [state.finalAttachmentUrl] : [],
        },
      });
      
      navigate('/dashboard');
    } catch (err: any) {
      setState(s => ({ ...s, error: err.message, isSubmitting: false }));
    }
  };

  const handleIconClick = () => {
    fileInputRef.current?.click();
  };

  const characterCount = state.content.length;
  const isOverLimit = characterCount > 280;

  return (
    <>
      <div className="page-header">Create Post</div>
      <div className="post-card">
        <div className="post-avatar">{/* ... */}</div>
        <div className="post-content">
          <form onSubmit={handleSubmit} className="create-post-form">
            <textarea
              value={content}
              onChange={(e) => setState(s => ({ ...s, content: e.target.value, error: null }))}
              placeholder="What's on your mind?"
              className="post-textarea"
              rows={5}
              disabled={isSubmitting}
              autoFocus
            />

            {state.previewUrl && (
              <div className="image-preview-container">
                <img src={state.previewUrl} alt="Preview" className="image-preview" />
                {state.isUploading && <div className="upload-spinner"></div>}
              </div>
            )}

            {state.error && <p className="form-error-message">{state.error}</p>}

            <div className="form-footer">
              <input 
                type="file" 
                ref={fileInputRef} 
                onChange={handleFileSelect}
                style={{ display: 'none' }}
                accept="image/jpeg, image/png, image/gif, image/webp"
              />

              <button type="button" onClick={() => fileInputRef.current?.click()} className="icon-button" disabled={state.isUploading}>
                <PhotoIcon />
              </button>

              <div className="footer-right">
                <span className={`char-counter ${isOverLimit ? 'over-limit' : ''}`}>
                  {characterCount} / 280
                </span>
                <button 
                  type="submit" 
                  className="submit-post-button" 
                  disabled={state.isSubmitting || state.isUploading || (!state.content.trim() && !state.finalAttachmentUrl)}
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