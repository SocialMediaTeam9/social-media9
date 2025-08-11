import React from 'react';
import { Routes, Route } from 'react-router-dom';

import Sidebar from '../components/Sidebar';
import ExplorePage from './ExplorePage';
import ProfilePage from './ProfilePage';
import CreatePostPage from './CreatePostPage';
import SearchPage from './SearchPage';
import Recommendations from '../components/Recommendations';

const Dashboard: React.FC<{ onLogout: () => void }> = ({ onLogout }) => {
  return (
    <div className="dashboard-container">
      <Sidebar onLogout={onLogout} />

      <main className="main-content">
        <Routes>
          <Route index element={<ExplorePage />} />
          <Route path="profile/:handle" element={<ProfilePage />} />
          <Route path="create" element={<CreatePostPage />} />
          <Route path="search" element={<SearchPage />} />
          <Route path="recommendations" element={<Recommendations />} />
        </Routes>
      </main>
    </div>
  );
};

export default Dashboard;
