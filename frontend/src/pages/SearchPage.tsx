import React, { useState, useCallback } from 'react';
import { fetcher } from '../utils/fetcher';
import { UserSearchResult, PostSearchResult } from '../types/types';

const SearchPage: React.FC = () => {
  const [query, setQuery] = useState('');
  const [searchType, setSearchType] = useState<'users' | 'posts' | 'hashtags'>('users');
  const [results, setResults] = useState<(UserSearchResult | PostSearchResult)[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [hasSearched, setHasSearched] = useState(false);

  const handleSearch = useCallback(async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!query.trim()) return;

    setIsLoading(true);
    setError(null);
    setHasSearched(true);

    try {
        const endpoint = `/search/${searchType}?q=${encodeURIComponent(query)}`;
        const data = await fetcher<any[]>(endpoint, { method: 'GET' });
        setResults(data);
    } catch (err: any) {
        setError(err.message || 'An error occurred during search.');
        setResults([]);
    } finally {
        setIsLoading(false);
    }
  }, [query, searchType]);

  const renderResults = () => {
    if (isLoading) return <p className="text-gray-400">Searching...</p>;
    if (error) return <p className="text-red-500">{error}</p>;
    if (results.length === 0 && hasSearched) return <p className="text-gray-400">No results found for "{query}".</p>;

    if (searchType === 'users') {
      return (
        <div className="space-y-4">
          {(results as UserSearchResult[]).map((user) => (
            <div key={user.userId} className="flex items-center p-3 bg-gray-800 rounded-lg">
              <img src={user.profilePictureUrl || 'https://via.placeholder.com/50'} alt={user.username} className="w-12 h-12 rounded-full mr-4" />
              <div>
                <p className="font-bold text-white">{user.fullName}</p>
                <p className="text-gray-400">@{user.username}</p>
              </div>
            </div>
          ))}
        </div>
      );
    }

    if (searchType === 'posts' || searchType === 'hashtags') {
      return (
        <div className="space-y-4">
          {(results as PostSearchResult[]).map((post) => (
            <div key={post.postId} className="p-4 bg-gray-800 rounded-lg">
              <p className="font-semibold text-white">@{post.username}</p>
              <p className="text-gray-300 mt-2">{post.content}</p>
              <p className="text-xs text-gray-500 mt-2">{new Date(post.createdAt).toLocaleString()}</p>
            </div>
          ))}
        </div>
      );
    }

    return null;
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
            placeholder={`Search for ${searchType}...`}
            className="w-full bg-transparent p-3 text-white focus:outline-none"
          />
          <button type="submit" className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-6 rounded-full">
            Search
          </button>
        </div>
      </form>
      
      <div className="flex space-x-2 mb-6 border-b border-gray-700">
        <button onClick={() => setSearchType('users')} className={`px-4 py-2 font-semibold ${searchType === 'users' ? 'text-blue-500 border-b-2 border-blue-500' : 'text-gray-400'}`}>Users</button>
        <button onClick={() => setSearchType('posts')} className={`px-4 py-2 font-semibold ${searchType === 'posts' ? 'text-blue-500 border-b-2 border-blue-500' : 'text-gray-400'}`}>Posts</button>
        <button onClick={() => setSearchType('hashtags')} className={`px-4 py-2 font-semibold ${searchType === 'hashtags' ? 'text-blue-500 border-b-2 border-blue-500' : 'text-gray-400'}`}>Hashtags</button>
      </div>

      <div>
        {renderResults()}
      </div>
    </div>
  );
};

export default SearchPage;