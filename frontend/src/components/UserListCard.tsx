import React from 'react';
import { Link } from 'react-router-dom';
import { UserProfile } from '../types/types';


interface UserListCardProps {
  user: UserProfile;
}

const UserListCard: React.FC<UserListCardProps> = ({ user }) => {

  const avatarUrl = user.profilePictureUrl 
    || `https://ui-avatars.com/api/?name=${encodeURIComponent(user.fullName || user.username)}&background=4b5563&color=e0e0e0`;

  return (
  
    <Link to={`/dashboard/profile/${user.username}`} className="user-list-card-link">
      <div className="user-list-card">
        <img
          src={avatarUrl}
          alt={`${user.username}'s profile`}
          className="user-list-avatar"
        />
        <div className="user-list-info">
          <p className="user-list-fullname">{user.fullName || user.username}</p>
          <p className="user-list-username">@{user.username}</p>
        </div>

      </div>
    </Link>
  );
};

export default UserListCard;