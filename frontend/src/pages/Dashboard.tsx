import React, { useState } from 'react';
import Sidebar from '../components/Sidebar';
import ExplorePage from './ExplorePage';
import ProfilePage from './ProfilePage';
import CreatePostPage from './CreatePostPage';
import SearchPage from './SearchPage';

// --- Dashboard Component ---
// This is the main layout component that contains the sidebar and the main content area.
const Dashboard: React.FC<{ onLogout: () => void }> = ({ onLogout }) => {
  const [currentPage, setCurrentPage] = useState<'explore' | 'profile' | 'createPost' | 'search'>('explore');

  // A helper function to render the correct component based on the state.
  const renderPage = () => {
    switch (currentPage) {
      case 'explore':
        return <ExplorePage />;
      case 'profile':
        return <ProfilePage />;
      case 'createPost':
        return <CreatePostPage />;
      case 'search':
        return <SearchPage />;
      default:
        return <ExplorePage />;
    }
  };

  // A helper function to get the page title.
  const getPageTitle = () => {
    switch (currentPage) {
      case 'explore':
        return 'Explore';
      case 'profile':
        return 'Profile';
      case 'createPost':
        return 'Create a Post';
      case 'search':
        return 'Search';
      default:
        return 'Explore';
    }
  };

  return (
    <div className="dashboard-container">
      <Sidebar currentPage={currentPage} onPageChange={setCurrentPage} onLogout={onLogout} />

      {/* Main Content Area */}
      <main className="main-content">
        <div className="feed-header">{getPageTitle()}</div>
        {/* We check for 'explore' to avoid extra padding on the feed */}
        {currentPage === 'explore' ? <ExplorePage /> : <div style={{padding: '2rem'}}>{renderPage()}</div>}
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
