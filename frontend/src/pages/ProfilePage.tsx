import React, { useState, useEffect } from 'react';
import { fetcher } from '../utils/fetcher';
import { UserProfile } from '../types/types';
import { UpdateProfileResponse } from '../types/types'; 

const ProfilePage: React.FC = () => {
  const [userProfile, setUserProfile] = useState<UserProfile | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isEditing, setIsEditing] = useState(false);

  
  const [fullName, setFullName] = useState('');
  const [bio, setBio] = useState('');
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const currentUserId = localStorage.getItem('userId');

  useEffect(() => {
    const fetchUserProfile = async () => {
      if (!currentUserId) {
        setError("User ID not found. Please log in.");
        setIsLoading(false);
        return;
      }
      try {
        setIsLoading(true);
        const data = await fetcher<UserProfile>(`/user/profile`, {
          method: 'GET',
          userId: currentUserId,
        });
        setUserProfile(data);
        setFullName(data.fullName || '');
        setBio(data.bio || '');
      } catch (err: any) {
        console.error('Failed to fetch user profile:', err);
        setError(err.message || 'Failed to load profile. Please try again.');
      } finally {
        setIsLoading(false);
      }
    };

    fetchUserProfile();
  }, [currentUserId]);

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    if (event.target.files && event.target.files.length > 0) {
      setSelectedFile(event.target.files[0]);
    } else {
      setSelectedFile(null);
    }
  };

  const handleUpdateProfile = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!currentUserId) {
      setError("User ID not found. Cannot update profile.");
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      let newProfilePictureUrl = userProfile?.profilePictureUrl;

      // 1. Upload new profile picture if selected
      if (selectedFile) {
        const formData = new FormData();
        formData.append('file', selectedFile);

        const uploadResponse = await fetcher<{ profilePictureUrl: string }>(
          `/users/${currentUserId}/profile-picture`,
          {
            method: 'POST',
            body: formData,
          }
        );
        newProfilePictureUrl = uploadResponse.profilePictureUrl;
      }

      // 2. Update user profile with potentially new picture URL and other fields
      // The fetcher's transformData function should handle converting the backend response
      // to the `UpdateProfileResponse` type.
      const updatedData = await fetcher<UpdateProfileResponse>(`/user/update`, {
        method: 'PUT',
        userId: currentUserId,
        body: {
          fullName: fullName,
          bio: bio,
          profilePictureUrl: newProfilePictureUrl,
        },
      });

      setUserProfile(updatedData.updatedUser);
      setIsEditing(false);
      setSelectedFile(null);
    } catch (err: any) {
      console.error('Failed to update profile:', err);
      setError(err.message || 'Failed to update profile. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return <div className="p-8 text-white">Loading profile...</div>;
  }

  if (error) {
    return <div className="p-8 text-red-500">Error: {error}</div>;
  }

  if (!userProfile) {
    return <div className="p-8 text-white">No profile data found.</div>;
  }

  return (
    <div className="page-content" style={{ padding: '2rem' }}>
      <h2 className="text-2xl font-bold mb-4 text-white">My Profile</h2>

      {isEditing ? (
        <form onSubmit={handleUpdateProfile} className="space-y-4">
          <div className="flex items-center space-x-4">
            {userProfile.profilePictureUrl && (
              <img
                src={userProfile.profilePictureUrl}
                alt="Profile"
                className="w-24 h-24 rounded-full object-cover border-2 border-blue-500"
              />
            )}
            <div>
              <label htmlFor="profilePicture" className="block text-gray-400 text-sm font-bold mb-2">
                Change Profile Picture:
              </label>
              <input
                type="file"
                id="profilePicture"
                accept="image/*"
                onChange={handleFileChange}
                className="block w-full text-sm text-gray-400 file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
              />
              {selectedFile && <p className="text-sm text-gray-500 mt-1">Selected: {selectedFile.name}</p>}
            </div>
          </div>

          <div>
            <label htmlFor="fullName" className="block text-gray-400 text-sm font-bold mb-2">
              Full Name:
            </label>
            <input
              type="text"
              id="fullName"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline bg-gray-700 text-white"
            />
          </div>

          <div>
            <label htmlFor="bio" className="block text-gray-400 text-sm font-bold mb-2">
              Bio:
            </label>
            <textarea
              id="bio"
              value={bio}
              onChange={(e) => setBio(e.target.value)}
              rows={4}
              className="shadow appearance-none border rounded w-full py-2 px-3 text-gray-700 leading-tight focus:outline-none focus:shadow-outline bg-gray-700 text-white"
            ></textarea>
          </div>

          <div className="flex space-x-4">
            <button
              type="submit"
              className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded-full focus:outline-none focus:shadow-outline"
              disabled={isLoading}
            >
              {isLoading ? 'Saving...' : 'Save Changes'}
            </button>
            <button
              type="button"
              onClick={() => {
                setIsEditing(false);
                setFullName(userProfile.fullName || '');
                setBio(userProfile.bio || '');
                setSelectedFile(null);
              }}
              className="bg-gray-600 hover:bg-gray-700 text-white font-bold py-2 px-4 rounded-full focus:outline-none focus:shadow-outline"
            >
              Cancel
            </button>
          </div>
        </form>
      ) : (
        <div className="space-y-4">
          <div className="flex items-center space-x-4">
            {userProfile.profilePictureUrl ? (
              <img
                src={userProfile.profilePictureUrl}
                alt={`${userProfile.username}'s profile`}
                className="w-32 h-32 rounded-full object-cover border-4 border-blue-500"
              />
            ) : (
              <div className="w-32 h-32 rounded-full bg-gray-600 flex items-center justify-center text-gray-300 text-5xl font-bold">
                {userProfile.username ? userProfile.username[0].toUpperCase() : 'U'}
              </div>
            )}
            <div>
              <p className="text-gray-400 text-lg">@{userProfile.username}</p>
              <h3 className="text-white text-3xl font-bold">{userProfile.fullName}</h3>
            </div>
          </div>

          <p className="text-gray-300 text-lg">{userProfile.bio || 'No bio available.'}</p>

          <div className="flex space-x-6 text-gray-400">
            <p className="text-lg">
              <span className="font-bold text-white">{userProfile.followersCount || 0}</span> Followers
            </p>
            <p className="text-lg">
              <span className="font-bold text-white">{userProfile.followingCount || 0}</span> Following
            </p>
          </div>
          <p className="text-sm text-gray-500">Joined: {userProfile.createdAt}</p>

          <button
            onClick={() => setIsEditing(true)}
            className="mt-4 bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded-full focus:outline-none focus:shadow-outline"
          >
            Edit Profile
          </button>
        </div>
      )}
    </div>
  );
};

export default ProfilePage;
