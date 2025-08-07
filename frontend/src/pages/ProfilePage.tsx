import React, { useEffect, useState, useCallback, useMemo, useRef } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import PostCard from '../components/PostCard';
import { PostResponse, UserProfile } from '../types/types';
import { fetcher, getPostsByUsername, getUploadUrl, lookupProfile, uploadFileToS3 } from '../utils/fetcher';
import PostCardAlt from '../components/PostCardAlt';

const ProfilePage: React.FC = () => {
    const { handle } = useParams<{ handle: string }>();
    const [profile, setProfile] = useState<UserProfile | null>(null);
    const [posts, setPosts] = useState<PostResponse[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [isEditing, setIsEditing] = useState(false);
    const [isFollowLoading, setIsFollowLoading] = useState(false);

    const loggedInUsername = useMemo(() => localStorage.getItem('username'), []);
    const isOwnProfile = profile?.username === loggedInUsername;

    const usernameToFetch = handle || loggedInUsername; 


    const fetchProfileData = useCallback(async () => {
        if (!handle) return;
        setIsLoading(true);
        setError(null);
        try {
            const profileData = await lookupProfile(usernameToFetch ?? '');
            setProfile(profileData);
            if (!handle.includes('@')) {
                const postsData = await getPostsByUsername(handle);
                setPosts(postsData);
            } else {
                setPosts([]);
            }
        } catch (err: any) {
            setError(err.message || 'Failed to load profile.');
        } finally {
            setIsLoading(false);
        }
    }, [handle]);

    useEffect(() => {
        fetchProfileData();
    }, [fetchProfileData]);

    const handleFollowToggle = async () => {
        if (!profile || isFollowLoading || isOwnProfile) return;
        setIsFollowLoading(true);
        const isCurrentlyFollowing = profile.isFollowing;
        const newFollowerCount = isCurrentlyFollowing ? profile.followersCount - 1 : profile.followersCount + 1;
        
        setProfile(p => p ? { ...p, isFollowing: !isCurrentlyFollowing, followersCount: newFollowerCount } : null);
        try {
            const method = isCurrentlyFollowing ? 'DELETE' : 'POST';
            await fetcher(`/api/v1/profiles/${profile.username}/follow`, { method });
        } catch (err) {
            setProfile(p => p ? { ...p, isFollowing: isCurrentlyFollowing, followersCount: profile.followersCount } : null);
            alert(`Failed to ${isCurrentlyFollowing ? 'unfollow' : 'follow'} user.`);
        } finally {
            setIsFollowLoading(false);
        }
    };
    
    const handleSaveProfile = (updatedProfile: UserProfile) => {
        setProfile(updatedProfile);
        setIsEditing(false);
    };

    if (isLoading) return <div className="page-header">Loading Profile...</div>;
    if (error) return <div className="page-header text-red-500">{error}</div>;
    if (!profile) return <div className="page-header">User not found.</div>;

    return (
        <div className="profile-page">
            {isEditing && isOwnProfile ? (
                <>
                    <div className="page-header">Edit Profile</div>
                    <EditProfileForm 
                        initialProfile={profile} 
                        onCancel={() => setIsEditing(false)}
                        onSave={handleSaveProfile}
                    />
                </>
             ) : (
                <>
                    <div className="page-header">{profile.fullName}</div>
                    <ProfileHeader 
                        profile={profile}
                        isOwnProfile={isOwnProfile}
                        onEditClick={() => setIsEditing(true)}
                        onFollowToggle={handleFollowToggle}
                        isFollowLoading={isFollowLoading}
                    />
                </>
             )}
            
            <div className="profile-posts-feed">
                <h3 className="feed-title">Posts</h3>
                {posts.length > 0 ? (
                    posts.map(post => <PostCardAlt key={post.postId} post={post} />)
                ) : (
                   <p className="p-4 text-gray-400">{!handle?.includes('@') ? "This user hasn't posted anything yet." : "Viewing posts from remote users is not yet supported."}</p>
                )}
            </div>
        </div>
    );
};


const ProfileHeader: React.FC<{
    profile: UserProfile;
    isOwnProfile: boolean;
    onEditClick: () => void;
    onFollowToggle: () => void;
    isFollowLoading: boolean;
}> = ({ profile, isOwnProfile, onEditClick, onFollowToggle, isFollowLoading }) => (
    <div className="profile-header">
        <img 
            src={profile.profilePictureUrl || `https://ui-avatars.com/api/?name=${profile.fullName || profile.username}&background=334155&color=e2e8f0&size=128`} 
            alt={profile.username} 
            className="profile-avatar" 
        />
        <div className="profile-actions">
            {isOwnProfile ? (
                <button onClick={onEditClick} className="profile-button-secondary">Edit Profile</button>
            ) : profile.isFollowing ? (
                <button onClick={onFollowToggle} disabled={isFollowLoading} className="profile-button-secondary">Following</button>
            ) : (
                <button onClick={onFollowToggle} disabled={isFollowLoading} className="profile-button-primary">Follow</button>
            )}
        </div>
        <div className="profile-info">
            <h3 className="profile-fullname">{profile.fullName}</h3>
            <p className="profile-username">@{profile.username}</p>
            <p className="profile-bio">{profile.bio}</p>
            <div className="profile-stats">
                <span className="stat-link"><span className="font-bold text-white">{profile.followingCount}</span> Following</span>
                <span className="stat-link"><span className="font-bold text-white">{profile.followersCount}</span> Followers</span>
            </div>
        </div>
    </div>
);


const EditProfileForm: React.FC<{
    initialProfile: UserProfile,
    onCancel: () => void,
    onSave: (updatedProfile: UserProfile) => void
}> = ({ initialProfile, onCancel, onSave }) => {
    const [fullName, setFullName] = useState(initialProfile.fullName || '');
    const [bio, setBio] = useState(initialProfile.bio || '');
    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [isSaving, setIsSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const handleSave = async (e: React.FormEvent) => {
        e.preventDefault();
        setIsSaving(true);
        setError(null);
        
        try {
            let profilePictureUrl = initialProfile.profilePictureUrl;
            if (selectedFile) {
                const { uploadUrl, finalUrl } = await getUploadUrl({
                  fileName: selectedFile.name, contentType: selectedFile.type
                });
                await uploadFileToS3(uploadUrl, selectedFile);
                profilePictureUrl = finalUrl;
            }
            
            const updatedProfile = await fetcher<UserProfile>(`/api/v1/profiles/me`, {
                method: 'PUT',
                body: { fullName, bio, profilePictureUrl },
            });
            onSave(updatedProfile);
        } catch (err: any) {
            setError(err.message || "Failed to save profile.");
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <div className="p-4">
            <form onSubmit={handleSave} className="edit-profile-form space-y-4">
                <div>
                    <label className="block text-gray-400 text-sm font-bold mb-2">Profile Picture</label>
                    <input type="file" ref={fileInputRef} onChange={e => setSelectedFile(e.target.files?.[0] || null)} accept="image/*" />
                </div>
                <div>
                    <label className="block text-gray-400 text-sm font-bold mb-2">Full Name</label>
                    <input type="text" value={fullName} onChange={e => setFullName(e.target.value)} className="w-full bg-gray-700 text-white rounded p-2" />
                </div>
                <div>
                    <label className="block text-gray-400 text-sm font-bold mb-2">Bio</label>
                    <textarea value={bio} onChange={e => setBio(e.target.value)} className="w-full bg-gray-700 text-white rounded p-2" rows={4} />
                </div>

                <div className="flex space-x-4">
                    <button type="submit" disabled={isSaving} className="profile-button-primary">{isSaving ? 'Saving...' : 'Save Changes'}</button>
                    <button type="button" onClick={onCancel} className="profile-button-secondary">Cancel</button>
                </div>
                {error && <p className="text-red-500 mt-2">{error}</p>}
            </form>
        </div>
    );
};

export default ProfilePage;