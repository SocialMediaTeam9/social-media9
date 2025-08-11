import React from 'react';
import { Link } from 'react-router-dom';
import { UserProfile } from '../types/types';

// Define the shape of the data this component expects.
// It's a good practice to import this from a central types file (e.g., src/types/types.ts).
// export interface UserSummary {
//   username: string;
//   fullName: string;
//   profilePictureUrl: string | null;
// }

interface UserListCardProps {
  user: UserProfile;
}

const UserListCard: React.FC<UserListCardProps> = ({ user }) => {
  // Use a fallback avatar if no profile picture is available.
  // This uses a service that generates an avatar with the user's initials.
  const avatarUrl = user.profilePictureUrl 
    || `https://ui-avatars.com/api/?name=${encodeURIComponent(user.fullName || user.username)}&background=4b5563&color=e0e0e0`;

  return (
    // The entire card is a link to the user's profile page.
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
        {/* In the future, you could add a Follow button here */}
        {/* <button className="follow-button-small">Follow</button> */}
      </div>
    </Link>
  );
};

export default UserListCard;