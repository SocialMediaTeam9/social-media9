import React, { useState, useCallback } from 'react';
import { fetcher } from '../utils/fetcher';

interface SearchResult {
  resultType: 'User' | 'Post';
  userId?: string;
  fullName?: string;
  profilePictureUrl?: string;
  postId?: string;
  content?: string;
  username: string;
  createdAt: string;
}

const SearchPage: React.FC = () => {
  const [query, setQuery] = useState('');
  const [searchType, setSearchType] = useState<'users' | 'all'>('users');
  const [results, setResults] = useState<SearchResult[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hasSearched, setHasSearched] = useState(false);

  const handleSearch = useCallback(async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!query.trim()) return;

    setIsLoading(true);
    setError(null);
    setHasSearched(true);
    setResults([]);

    try {
        const endpoint = `/api/search/${searchType}?q=${encodeURIComponent(query)}`;
        const data = await fetcher<SearchResult[]>(endpoint, { method: 'GET' });
        setResults(data);
    } catch (err: any) {
        setError(err.message || 'An error occurred during search.');
        setResults([]);
    } finally {
        setIsLoading(false);
    }
  }, [query, searchType]);

  const renderUser = (user: SearchResult) => (
    <div key={user.userId || user.username} className="flex items-center p-3 bg-gray-800 rounded-lg">
      <img src={user.profilePictureUrl || 'https://via.placeholder.com/50'} alt={user.username} className="w-12 h-12 rounded-full mr-4" />
      <div>
        <p className="font-bold text-white">{user.fullName}</p>
        <p className="text-gray-400">@{user.username}</p>
      </div>
    </div>
  );

  const renderPost = (post: SearchResult) => (
    <div key={post.postId} className="p-4 bg-gray-800 rounded-lg">
      <p className="font-semibold text-white">@{post.username}</p>
      <p className="text-gray-300 mt-2 whitespace-pre-wrap">{post.content}</p>
      <p className="text-xs text-gray-500 mt-2">{new Date(post.createdAt).toLocaleString()}</p>
    </div>
  );

  const renderResults = () => {
    if (isLoading) return <p className="text-gray-400">Searching...</p>;
    if (error) return <p className="text-red-500">{error}</p>;
    if (results.length === 0 && hasSearched) return <p className="text-gray-400">No results found for "{query}".</p>;

    return (
      <div className="space-y-4">
        {results.map((item) => {
          if (item.resultType === 'User') {
            return renderUser(item);
          }
          if (item.resultType === 'Post') {
            return renderPost(item);
          }
          return null;
        })}
      </div>
    );
  };

  return (
    <div className="page-content" style={{ padding: '2rem' }}>
      <h2 className="text-2xl font-bold mb-4 text-white">Search</h2>
      
      <form onSubmit={handleSearch} className="mb-6">
        <div className="flex rounded-full bg-gray-800 p-1">
          <input
            type="text"
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder={searchType === 'users' ? 'Search for user@server.com...' : 'Search for users or content...'}
            className="w-full bg-transparent p-3 text-white focus:outline-none"
          />
          <button type="submit" className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-6 rounded-full">
            Search
          </button>
        </div>
      </form>
      
      <div className="flex space-x-2 mb-6 border-b border-gray-700">
        <button onClick={() => setSearchType('users')} className={`px-4 py-2 font-semibold ${searchType === 'users' ? 'text-blue-500 border-b-2 border-blue-500' : 'text-gray-400'}`}>Users</button>
        <button onClick={() => setSearchType('all')} className={`px-4 py-2 font-semibold ${searchType === 'all' ? 'text-blue-500 border-b-2 border-blue-500' : 'text-gray-400'}`}>Search</button>
      </div>

      <div>
        {renderResults()}
      </div>
    </div>
  );
};

export default SearchPage;