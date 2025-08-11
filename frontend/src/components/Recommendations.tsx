import React, { useState, useEffect } from 'react';
import { UserProfile } from '../types/types';
import { getRecommendedUsers } from '../utils/fetcher';
import UserListCard from './UserListCard';


const Recommendations: React.FC = () => {
  const [recommendations, setRecommendations] = useState<UserProfile[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchRecommendations = async () => {
      try {
        setIsLoading(true);
        const users = await getRecommendedUsers();
        setRecommendations(users);
      } catch (err: any) {
        setError("Could not load recommendations.");
        console.error(err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchRecommendations();
  }, []); 

  const renderContent = () => {
    if (isLoading) {
      return <p className="text-sm text-gray-400">Finding people for you...</p>;
    }
    if (error) {
      return <p className="text-sm text-red-500">{error}</p>;
    }
    if (recommendations.length === 0) {
      return <p className="text-sm text-gray-400">No recommendations right now.</p>;
    }
    
    return (
      <div className="recommendations-list">
        {recommendations.map(user => (
          <UserListCard key={user.username} user={user} />
        ))}
      </div>
    );
  };

  return (
    <div className="box">
      <h3 className="box-title">People You May Know</h3>
      {renderContent()}
    </div>
  );
};

export default Recommendations;