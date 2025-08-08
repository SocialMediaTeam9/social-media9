import React, { useState, useEffect, useRef, useCallback } from 'react';
import { fetcher } from '../utils/fetcher'; // Assuming you have a fetcher utility
import PostCard from '../components/PostCard'; // Using your existing PostCard component

// --- These types match your C# DTOs from TimelineService ---
interface PostData {
  postId: string;
  authorUsername: string;
  postContent: string;
  attachmentUrls: string[];
  createdAt: string;
  boostedBy?: string;
  likeCount: number;
  isLikedByUser: boolean;
}

interface PaginatedTimelineResponse {
  items: PostData[];
  nextCursor?: string;
}

const ExplorePage: React.FC = () => {
  // --- State management for the dynamic feed ---
  const [posts, setPosts] = useState<PostData[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [cursor, setCursor] = useState<string | undefined>(undefined);
  const [hasMore, setHasMore] = useState(true);

  // Placeholder for the currently logged-in user's username.
  // In a real application, this would come from an authentication context (e.g., React Context, Redux, Zustand).
  // For demonstration, I'm setting a static value. Please replace this with your actual user context.
  const currentLoggedInUsername = "yourLoggedInUsername"; // <<< IMPORTANT: Replace with actual logged-in username

  const observer = useRef<IntersectionObserver | null>(null);

  // --- Infinite scroll logic ---
  const lastPostElementRef = useCallback((node: HTMLDivElement) => {
    if (isLoading) return;
    if (observer.current) observer.current.disconnect();

    observer.current = new IntersectionObserver(entries => {
      if (entries[0].isIntersecting && hasMore) {
        loadMorePosts();
      }
    });

    if (node) observer.current.observe(node);
  }, [isLoading, hasMore]);

  // --- API call logic ---
  const loadMorePosts = useCallback(async () => {
    if (isLoading || !hasMore) return;
    setIsLoading(true);
    setError(null);

    try {
      // This endpoint calls your TimelineController
      let endpoint = '/api/timeline/home?limit=20';
      if (cursor) {
        endpoint += `&cursor=${encodeURIComponent(cursor)}`;
      }

      const data = await fetcher<PaginatedTimelineResponse>(endpoint);
      setPosts(prevPosts => [...prevPosts, ...data.items]);
      setCursor(data.nextCursor);
      if (!data.nextCursor) {
        setHasMore(false);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load timeline.');
    } finally {
      setIsLoading(false);
    }
  }, [cursor, isLoading, hasMore]);

  // --- Initial data load ---
  useEffect(() => {
    loadMorePosts();
  }, []);

  // --- Dynamic rendering logic ---
  return (
    <div className="page-content" style={{ padding: '0 1rem' }}>
      <h1 className="text-2xl font-bold mb-4 text-white sticky top-0 bg-gray-900 bg-opacity-80 backdrop-blur-sm p-4 z-10">
        Home
      </h1>
      <div className="feed-content space-y-4">
        {posts.map((post, index) => (
          // Pass the currentLoggedInUsername to the PostCard component
          posts.length === index + 1
            ? <div ref={lastPostElementRef} key={post.postId}><PostCard post={post} currentLoggedInUsername={currentLoggedInUsername} /></div>
            : <PostCard key={post.postId} post={post} currentLoggedInUsername={currentLoggedInUsername} />
        ))}
      </div>

      {/* Loading, error, and end-of-feed messages */}
      {isLoading && <p className="text-center text-gray-400 mt-4 py-4">Loading...</p>}
      {!isLoading && !hasMore && posts.length > 0 && <p className="text-center text-gray-500 mt-4 py-4">You've reached the end!</p>}
      {error && <p className="text-center text-red-500 mt-4 py-4">{error}</p>}
      {!isLoading && posts.length === 0 && !error && <p className="text-center text-gray-400 mt-4 py-4">Your timeline is empty. Follow people to see posts here!</p>}
    </div>
  );
};

export default ExplorePage;
