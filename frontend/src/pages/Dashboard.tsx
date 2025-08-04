import React from 'react';
import { Routes, Route } from 'react-router-dom';

import Sidebar from '../components/Sidebar';
import ExplorePage from './ExplorePage';
import ProfilePage from './ProfilePage';
import CreatePostPage from './CreatePostPage';
import SearchPage from './SearchPage';

const Dashboard: React.FC<{ onLogout: () => void }> = ({ onLogout }) => {
  return (
    <div className="dashboard-container">
      <Sidebar onLogout={onLogout} />

      {/* Main Content */}
      <main className="main-content">
        <Routes>
          <Route index element={<ExplorePage />} />
          <Route path="profile" element={<ProfilePage />} />
          <Route path="create" element={<CreatePostPage />} />
          <Route path="search" element={<SearchPage />} />
        </Routes>
      </main>

      {/* Right Sidebar */}
      <aside className="right-sidebar">
        <div className="right-sidebar-header">Who to Follow</div>
        <div className="right-sidebar-item">
          <p>Follow a user to see their posts!</p>
        </div>
      </aside>
    </div>
  );
};

export default Dashboard;
